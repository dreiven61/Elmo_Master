using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        internal static void RegisterAxisCommandRecoveryIntegrationTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AxisCommand.FreshStopOneCommandThreeStatus",
                AxisCommandFreshStopOneCommandThreeStatus);
            tests.Add(
                "Wpf.AxisCommand.FreshResetOneCommandThreeStatus",
                AxisCommandFreshResetOneCommandThreeStatus);
            tests.Add(
                "Wpf.AxisCommand.AcceptedStopRestartStatusOnly",
                AxisCommandAcceptedStopRestartStatusOnly);
            tests.Add(
                "Wpf.AxisCommand.AcceptedResetRestartStatusOnly",
                AxisCommandAcceptedResetRestartStatusOnly);
            tests.Add(
                "Wpf.AxisCommand.FreshStopNackResolves",
                AxisCommandFreshStopNackResolves);
            tests.Add(
                "Wpf.AxisCommand.FreshResetNackResolves",
                AxisCommandFreshResetNackResolves);
            tests.Add(
                "Wpf.AxisCommand.StopObserverThrowResumesWithoutReplay",
                AxisCommandStopObserverThrowResumesWithoutReplay);
            tests.Add(
                "Wpf.AxisCommand.ResetObserverThrowResumesWithoutReplay",
                AxisCommandResetObserverThrowResumesWithoutReplay);
            tests.Add(
                "Wpf.AxisCommand.HeldResetAbortReconnectStopOnce",
                AxisCommandHeldResetAbortReconnectStopOnce);
            tests.Add(
                "Wpf.AxisCommand.CompletedResetBeforeAbortStillDispatchesStopOnce",
                AxisCommandCompletedResetBeforeAbortStillDispatchesStopOnce);
            tests.Add(
                "Wpf.AxisCommand.CompletedResetStopNackResolvesWithoutResetRestore",
                AxisCommandCompletedResetStopNackResolvesWithoutResetRestore);
            tests.Add(
                "Wpf.AxisCommand.CompletedResetStopNackIdentityMismatchKeepsStopRecoveryRequired",
                AxisCommandCompletedResetStopNackIdentityMismatchKeepsStopRecoveryRequired);
            tests.Add(
                "Wpf.AxisCommand.CompletedResetAfterReconnectStillDispatchesStopOnce",
                AxisCommandCompletedResetAfterReconnectStillDispatchesStopOnce);
            tests.Add(
                "Wpf.AxisCommand.TakeoverStopNackRestoresReset",
                AxisCommandTakeoverStopNackRestoresReset);
            tests.Add(
                "Wpf.AxisCommand.TakeoverPrewireFaultRestoresResetZeroStop",
                AxisCommandTakeoverPrewireFaultRestoresResetZeroStop);
            tests.Add(
                "Wpf.AxisCommand.TakeoverPostwriteLossKeepsStopRecoveryRequired",
                AxisCommandTakeoverPostwriteLossKeepsStopRecoveryRequired);
            tests.Add(
                "Wpf.AxisCommand.TakeoverSessionMismatchKeepsNewTransportAndRestoresReset",
                AxisCommandTakeoverSessionMismatchKeepsNewTransportAndRestoresReset);
            tests.Add(
                "Wpf.AxisCommand.MotionAndAcceptedStopRestartResolveInOrder",
                AxisCommandMotionAndAcceptedStopRestartResolveInOrder);
        }

        internal static FakeRpcStep[] CreateAxisCommandProcessRpcSteps(
            bool reset,
            ManualResetEventSlim firstStatusRelease,
            ManualResetEventSlim firstStatusEntered)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(reset ? AxisResetCommandStep() : AxisStopCommandStep());
            var heldStatus = AxisResetStatusStep(state: 0x02000000u);
            heldStatus.BeforeResponse = () =>
            {
                if (firstStatusEntered != null)
                {
                    firstStatusEntered.Set();
                }
                if (firstStatusRelease != null
                    && !firstStatusRelease.Wait(15000))
                {
                    throw new TimeoutException(
                        "The held Axis Stop/Reset status response was not released.");
                }
            };
            heldStatus.AllowClientDisconnectAfterRequest = true;
            heldStatus.ContinueWithNextClientAfterResponseWriteDisconnect =
                true;
            steps.Add(heldStatus);

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        internal static FakeRpcStep[]
            CreateAxisCommandAckBoundaryProcessRpcSteps(bool reset)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(reset ? AxisResetCommandStep() : AxisStopCommandStep());
            steps.Add(new FakeRpcStep(0, null)
            {
                RequireClientDisconnectBeforeRequest = true
            });
            return steps.ToArray();
        }

        internal static FakeRpcStep[]
            CreateMotionAxisStopProcessRpcSteps(
                ManualResetEventSlim firstStatusRelease,
                ManualResetEventSlim firstStatusEntered)
        {
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisStopCommandStep());
            var heldStatus = AxisResetStatusStep(state: 0x02000000u);
            heldStatus.BeforeResponse = () =>
            {
                if (firstStatusEntered != null)
                {
                    firstStatusEntered.Set();
                }
                if (firstStatusRelease != null
                    && !firstStatusRelease.Wait(15000))
                {
                    throw new TimeoutException(
                        "The held Motion/Axis Stop status response was not released.");
                }
            };
            heldStatus.AllowClientDisconnectAfterRequest = true;
            heldStatus.ContinueWithNextClientAfterResponseWriteDisconnect =
                true;
            steps.Add(heldStatus);

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(new FakeRpcStep(0, null)
            {
                RequireClientDisconnectBeforeRequest = true,
                ContinueWithNextClientAfterDisconnect = true
            });

            steps.AddRange(CreateConnectAndTopologySteps(capabilities));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());
            return steps.ToArray();
        }

        private static void AxisCommandFreshStopOneCommandThreeStatus()
        {
            RunFreshAxisCommandSuccess(false);
        }

        private static void AxisCommandFreshResetOneCommandThreeStatus()
        {
            RunFreshAxisCommandSuccess(true);
        }

        private static void RunFreshAxisCommandSuccess(bool reset)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(reset ? AxisResetCommandStep() : AxisStopCommandStep());
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CloseStep());
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForAxisCommand(window);
                    Click(reset ? window.ButtonReset : window.ButtonStop);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                reset ? "Reset completed" : "Stop verified",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "The fresh Axis command did not reach stable proof.");
                    AssertEx.Equal(
                        1,
                        CountCommand(server, reset ? (ushort)0x2024 : (ushort)0x2022));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                    AssertEx.False(GetAxisCommandJournal(window).HasActiveRecord);
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void AxisCommandAcceptedStopRestartStatusOnly()
        {
            RunAcceptedAxisCommandRestartStatusOnly(false);
        }

        private static void AxisCommandAcceptedResetRestartStatusOnly()
        {
            RunAcceptedAxisCommandRestartStatusOnly(true);
        }

        private static void RunAcceptedAxisCommandRestartStatusOnly(bool reset)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(CloseStep());
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    SeedAcceptedAxisCommand(root, server.Port, reset);
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForAxisCommand(window);
                    Click(reset ? window.ButtonReset : window.ButtonStop);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                reset ? "Reset completed" : "Stop verified",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "The restarted Axis command did not finish status-only proof.");
                    AssertEx.Equal(0, CountCommand(server, 0x2022));
                    AssertEx.Equal(0, CountCommand(server, 0x2024));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                    AssertEx.False(GetAxisCommandJournal(window).HasActiveRecord);
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void AxisCommandFreshStopNackResolves()
        {
            RunFreshAxisCommandNack(false);
        }

        private static void AxisCommandFreshResetNackResolves()
        {
            RunFreshAxisCommandNack(true);
        }

        private static void RunFreshAxisCommandNack(bool reset)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(new FakeRpcStep(
                reset ? (ushort)0x2024 : (ushort)0x2022,
                TestFrame.Response(0, TestFrame.Hex("01 00 F9 FF"))));
            steps.Add(CloseStep());
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForAxisCommand(window);
                    Click(reset ? window.ButtonReset : window.ButtonStop);
                    WaitUntil(
                        () => CountCommand(
                                server,
                                reset ? (ushort)0x2024 : (ushort)0x2022) == 1
                            && !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)GetPrivateField(window, "safetyCommandRunning"),
                        "The rejected Axis command did not settle.");
                    AssertEx.Equal(0, CountCommand(server, 0x2028));
                    AssertEx.False(GetAxisCommandJournal(window).HasActiveRecord);
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void AxisCommandStopObserverThrowResumesWithoutReplay()
        {
            RunAxisCommandObserverThrow(false);
        }

        private static void AxisCommandResetObserverThrowResumesWithoutReplay()
        {
            RunAxisCommandObserverThrow(true);
        }

        private static void RunAxisCommandObserverThrow(bool reset)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(reset ? AxisResetCommandStep() : AxisStopCommandStep());
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CloseStep());
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForAxisCommand(window);
                    window.AxisCommandAcceptedBeforeDurableMarkTestHook =
                        record =>
                        {
                            throw new InvalidOperationException(
                                "Injected accepted observer failure.");
                        };
                    Click(reset ? window.ButtonReset : window.ButtonStop);
                    if (!reset)
                    {
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Stop verified",
                                    StringComparison.Ordinal)
                                && window.ButtonCloseConnection.IsEnabled,
                            "Stop did not continue the exact accepted continuation after the observer failure.");
                        AssertEx.Equal(1, CountCommand(server, 0x2022));
                        AssertEx.Equal(3, CountCommand(server, 0x2028));
                        AssertEx.False(
                            GetAxisCommandJournal(window).HasActiveRecord);
                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                        return;
                    }
                    WaitUntil(
                        () => CountCommand(
                                server,
                                reset ? (ushort)0x2024 : (ushort)0x2022) == 1
                            && !(bool)GetPrivateField(window, "operationRunning")
                            && !(bool)GetPrivateField(window, "safetyCommandRunning"),
                        "The accepted observer failure did not settle.");
                    AssertEx.Equal(0, CountCommand(server, 0x2028));
                    AssertEx.Equal(
                        AxisCommandRecoveryState.RecoveryRequired,
                        GetAxisCommandJournal(window).CurrentRecord.State);
                    window.AxisCommandAcceptedBeforeDurableMarkTestHook = null;
                    Click(reset ? window.ButtonReset : window.ButtonStop);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                reset ? "Reset completed" : "Stop verified",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "The exact pending Axis continuation did not resume status-only.");
                    AssertEx.Equal(
                        1,
                        CountCommand(server, reset ? (ushort)0x2024 : (ushort)0x2022));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                    AssertEx.False(GetAxisCommandJournal(window).HasActiveRecord);
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void AxisCommandHeldResetAbortReconnectStopOnce()
        {
            RunResetTakeover(false);
        }

        private static void
            AxisCommandCompletedResetBeforeAbortStillDispatchesStopOnce()
        {
            RunCompletedResetBeforeAbort(false);
        }

        private static void
            AxisCommandCompletedResetStopNackResolvesWithoutResetRestore()
        {
            RunCompletedResetBeforeAbort(true, false);
        }

        private static void
            AxisCommandCompletedResetStopNackIdentityMismatchKeepsStopRecoveryRequired()
        {
            RunCompletedResetBeforeAbort(true, true);
        }

        private static void RunCompletedResetBeforeAbort(
            bool rejectStop,
            bool mismatchFinalIdentity = false)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var resetCompletionPublished =
                new ManualResetEventSlim(false))
            using (var releaseResetCompletion =
                new ManualResetEventSlim(false))
            using (var stopPrepared = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisResetCommandStep());
                steps.Add(AxisResetStatusStep(state: 0u));
                steps.Add(AxisResetStatusStep(state: 0u));
                steps.Add(AxisResetStatusStep(state: 0u));
                steps.Add(rejectStop
                    ? new FakeRpcStep(
                        0x2022,
                        TestFrame.Response(0, TestFrame.Hex("01 00 F9 FF")))
                    : AxisStopCommandStep());
                if (!rejectStop)
                {
                    steps.Add(AxisResetStatusStep(state: 0x02000000u));
                    steps.Add(AxisResetStatusStep(state: 0x02000000u));
                    steps.Add(AxisResetStatusStep(state: 0x02000000u));
                    steps.Add(CapabilitiesStep(11, capabilities));
                }
                else
                {
                    var finalCapabilities = CapabilitiesPayload(
                        11,
                        capabilities,
                        0);
                    if (mismatchFinalIdentity)
                    {
                        TestFrame.WriteUInt32(
                            finalCapabilities,
                            24,
                            DiagnosticMapRevision + 1);
                    }
                    steps.Add(new FakeRpcStep(
                        0x7E00,
                        TestFrame.Response(0, finalCapabilities)));
                }
                if (!mismatchFinalIdentity)
                {
                    steps.Add(CloseStep());
                }
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        ConnectAndLoadAxisForAxisCommand(window);
                        LMCAxisResetWaitContinuation resetContinuation = null;
                        window.AxisResetAfterStatusPublicationTestHook =
                            continuation =>
                            {
                                if (continuation.State
                                    != LMCAxisResetWaitContinuationState
                                        .Completed)
                                {
                                    return;
                                }
                                resetContinuation = continuation;
                                resetCompletionPublished.Set();
                                if (!releaseResetCompletion.Wait(5000))
                                {
                                    throw new TimeoutException(
                                        "The naturally completed Reset publication was not released.");
                                }
                            };
                        Click(window.ButtonReset);
                        WaitUntil(
                            () => resetCompletionPublished.IsSet,
                            "The Reset did not naturally complete on three moving/error-clear samples.");
                        AssertEx.True(
                            resetContinuation != null
                                && resetContinuation.StatusPollCount == 3
                                && resetContinuation.StableErrorClearSampleCount
                                    == 3
                                && resetContinuation.LastObservedStatus != null
                                && !resetContinuation.LastObservedStatus
                                    .HasAxisError
                                && !resetContinuation.LastObservedStatus
                                    .IsStandstill,
                            "The completed Reset did not retain moving/error-clear proof.");
                        var resetRecord = GetAxisCommandJournal(window)
                            .CurrentRecord.Copy();

                        var originalConnection = GetPrivateField(
                            window,
                            "connection");
                        AxisCommandRecoveryRecord preparedStop = null;
                        window.AxisStopBeforeBeginDispatchTestHook =
                            (ignoredConnection, record) =>
                            {
                                preparedStop = record.Copy();
                                stopPrepared.Set();
                            };
                        Click(window.ButtonStop);
                        WaitUntil(
                            () => stopPrepared.IsSet,
                            "The completed-Reset Stop was not prepared.");
                        releaseResetCompletion.Set();
                        if (!rejectStop)
                        {
                            WaitUntil(
                                () => string.Equals(
                                        window.TextOperationState.Text,
                                        "Stop verified",
                                        StringComparison.Ordinal)
                                    && window.ButtonCloseConnection.IsEnabled,
                                "The pre-abort completed Reset race did not dispatch and prove Stop.");
                        }
                        else if (!mismatchFinalIdentity)
                        {
                            WaitUntil(
                                () => !GetAxisCommandJournal(window)
                                        .HasActiveRecord
                                    && !(bool)GetPrivateField(
                                        window,
                                        "safetyCommandRunning"),
                                "The completed-predecessor Stop NACK did not resolve as known-no-effect.");
                        }
                        else
                        {
                            WaitUntil(
                                () =>
                                {
                                    var active = GetAxisCommandJournal(window)
                                        .CurrentRecord;
                                    return active != null
                                        && active.IsActive
                                        && active.Operation
                                            == AxisCommandRecoveryOperation.Stop
                                        && active.State
                                            == AxisCommandRecoveryState
                                                .RecoveryRequired
                                        && !(bool)GetPrivateField(
                                            window,
                                            "safetyCommandRunning");
                                },
                                "The final-D0 mismatch did not preserve Stop RecoveryRequired.");
                        }

                        AssertEx.True(
                            ReferenceEquals(
                                originalConnection,
                                GetPrivateField(window, "connection")),
                            "The already-completed Reset race unnecessarily replaced transport.");
                        var tombstone = GetAxisCommandJournal(window)
                            .CurrentRecord;
                        AssertEx.True(
                            preparedStop != null
                                && tombstone != null
                                && tombstone.Operation
                                    == AxisCommandRecoveryOperation.Stop
                                && tombstone.State
                                    == (mismatchFinalIdentity
                                        ? AxisCommandRecoveryState
                                            .RecoveryRequired
                                        : AxisCommandRecoveryState.Resolved)
                                && tombstone.Identity == preparedStop.Identity
                                && tombstone.SupersededResetIdentity
                                    == resetRecord.Identity,
                            "The Stop did not retain its exact identity, expected state, and Reset predecessor.");
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(1, CountCommand(server, 0x2022));
                        AssertEx.Equal(
                            rejectStop ? 3 : 6,
                            CountCommand(server, 0x2028));
                        AssertEx.Equal(3, CountCommand(server, 0x7E00));
                        AssertEx.Equal(1, server.AcceptedClientCount);
                        AssertEx.Equal(
                            mismatchFinalIdentity,
                            GetAxisCommandJournal(window).HasActiveRecord);
                        if (mismatchFinalIdentity)
                        {
                            AssertEx.False(
                                window.ButtonCloseConnection.IsEnabled);
                            AssertEx.False(window.ButtonReset.IsEnabled);
                        }
                        else
                        {
                            CloseConnectedWindow(window);
                            window = null;
                        }
                        server.Verify();
                    }
                }
                finally
                {
                    releaseResetCompletion.Set();
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void
            AxisCommandCompletedResetAfterReconnectStillDispatchesStopOnce()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var statusEntered = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisResetCommandStep());
                steps.Add(AxisResetStatusStep(state: 0u));
                var heldStatus = AxisResetStatusStep(state: 0u);
                heldStatus.BeforeResponse = () => statusEntered.Set();
                heldStatus.WaitForClientDisconnectBeforeResponseAndContinue =
                    true;
                steps.Add(heldStatus);
                steps.Add(InitStep());
                steps.Add(CallbackStep());
                steps.Add(CapabilitiesStep(1, capabilities));
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisStopCommandStep());
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(CapabilitiesStep(2, capabilities));
                steps.Add(CloseStep());
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        ConnectAndLoadAxisForAxisCommand(window);
                        Click(window.ButtonReset);
                        WaitUntil(
                            () => statusEntered.IsSet,
                            "The post-reconnect Reset status was not held.");
                        var resetContinuation =
                            (LMCAxisResetWaitContinuation)GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation");
                        WaitUntil(
                            () => resetContinuation != null
                                && resetContinuation.StatusPollCount == 1
                                && resetContinuation.LastObservedStatus != null,
                            "The moving/error-clear Reset sample was not observed before abort.");
                        AssertEx.False(
                            resetContinuation.LastObservedStatus.HasAxisError);
                        AssertEx.False(
                            resetContinuation.LastObservedStatus.IsStandstill);
                        var resetRecord = GetAxisCommandJournal(window)
                            .CurrentRecord.Copy();

                        var afterReconnectHookCalled = false;
                        AxisCommandRecoveryRecord preparedStop = null;
                        window.AxisStopAfterSafetyReconnectTestHook =
                            continuation =>
                            {
                                AssertEx.True(
                                    ReferenceEquals(
                                        resetContinuation,
                                        continuation),
                                    "The late completion hook received a foreign Reset continuation.");
                                MarkResetContinuationCompletedForRace(
                                    continuation);
                                afterReconnectHookCalled = true;
                            };
                        window.AxisStopBeforeBeginDispatchTestHook =
                            (ignoredConnection, record) =>
                                preparedStop = record.Copy();
                        Click(window.ButtonStop);
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Stop verified",
                                    StringComparison.Ordinal)
                                && window.ButtonCloseConnection.IsEnabled,
                            "The post-reconnect completed Reset race did not dispatch and prove Stop.");

                        AssertEx.True(
                            afterReconnectHookCalled,
                            "The late Reset completion interleave was not exercised.");
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(1, CountCommand(server, 0x2022));
                        AssertEx.Equal(5, CountCommand(server, 0x2028));
                        AssertEx.Equal(2, server.AcceptedClientCount);
                        AssertEx.Equal(
                            0,
                            CountCommandInSession(server, 1, 0x2022));
                        AssertEx.Equal(
                            1,
                            CountCommandInSession(server, 2, 0x2022));
                        var tombstone = GetAxisCommandJournal(window)
                            .CurrentRecord;
                        AssertEx.True(
                            preparedStop != null
                                && tombstone != null
                                && tombstone.Identity == preparedStop.Identity
                                && tombstone.SupersededResetIdentity
                                    == resetRecord.Identity,
                            "The reconnect race lost the exact Stop identity or Reset predecessor.");
                        AssertEx.False(
                            GetAxisCommandJournal(window).HasActiveRecord);
                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void MarkResetContinuationCompletedForRace(
            LMCAxisResetWaitContinuation continuation)
        {
            var markCompleted = typeof(LMCAxisResetWaitContinuation)
                .GetMethod(
                    "MarkCompleted",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            if (markCompleted == null)
            {
                throw new MissingMethodException(
                    typeof(LMCAxisResetWaitContinuation).FullName,
                    "MarkCompleted");
            }
            markCompleted.Invoke(continuation, null);
            AssertEx.Equal(
                LMCAxisResetWaitContinuationState.Completed,
                continuation.State);
        }

        private static void AxisCommandTakeoverStopNackRestoresReset()
        {
            RunResetTakeover(true);
        }

        private static void
            AxisCommandTakeoverPrewireFaultRestoresResetZeroStop()
        {
            RunResetTakeoverTransportFailure(true);
        }

        private static void
            AxisCommandTakeoverPostwriteLossKeepsStopRecoveryRequired()
        {
            RunResetTakeoverTransportFailure(false);
        }

        private static void
            AxisCommandTakeoverSessionMismatchKeepsNewTransportAndRestoresReset()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var statusEntered = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisResetCommandStep());
                var heldStatus = AxisResetStatusStep();
                heldStatus.BeforeResponse = () => statusEntered.Set();
                heldStatus.WaitForClientDisconnectBeforeResponseAndContinue =
                    true;
                steps.Add(heldStatus);
                steps.Add(InitStep());
                steps.Add(CallbackStep());
                steps.Add(CapabilitiesStep(11, capabilities));
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(CapabilitiesStep(12, capabilities));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(CapabilitiesStep(13, capabilities));
                steps.Add(CloseStep());
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        ConnectAndLoadAxisForAxisCommand(window);
                        Click(window.ButtonReset);
                        WaitUntil(
                            () => statusEntered.IsSet,
                            "The mismatch Reset status observation was not held.");
                        window.AxisStopBeforeSafetyAbortTestHook =
                            async (currentConnection, record) =>
                            {
                                currentConnection
                                    .AbortTransportForSafetyPreemption();
                                await currentConnection
                                    .RpcInitConnectionAsync(
                                        record.EndpointIp,
                                        record.EndpointPort,
                                        "127.0.0.1",
                                        0,
                                        1u,
                                        CancellationToken.None);
                            };
                        Click(window.ButtonStop);
                        WaitUntil(
                            () =>
                            {
                                var current = GetAxisCommandJournal(window)
                                    .CurrentRecord;
                                var liveConnection = GetPrivateField(
                                    window,
                                    "connection") as LMCConnection;
                                return current != null
                                    && current.IsActive
                                    && current.Operation
                                        == AxisCommandRecoveryOperation.Reset
                                    && liveConnection != null
                                    && liveConnection.IsConnected
                                    && !(bool)GetPrivateField(
                                        window,
                                        "safetyCommandRunning")
                                    && !(bool)GetPrivateField(
                                        window,
                                        "operationRunning")
                                    && (int)GetPrivateField(
                                        window,
                                        "safetyMonitorCount") == 0;
                            },
                            "The session mismatch did not restore Reset while preserving the newer transport.");
                        AssertEx.Equal(0, CountCommand(server, 0x2022));
                        AssertEx.True(
                            GetPrivateField(window, "axis") == null,
                            "The old-session axis handle survived session mismatch.");
                        AssertEx.True(
                            GetPrivateField(
                                window,
                                "pendingAxisResetWaitContinuation") == null,
                            "The old-session Reset continuation survived session mismatch.");

                        window.AxisStopBeforeSafetyAbortTestHook = null;
                        Click(window.ButtonDiagnosticsCapabilities);
                        WaitUntil(
                            () => !(bool)GetPrivateField(
                                window,
                                "operationRunning"),
                            "The newer transport capabilities did not refresh.");
                        if (!string.Equals(
                                window.TextOperationState.Text,
                                "Refresh Diagnostics Capabilities completed",
                                StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                "Capability refresh state="
                                + window.TextOperationState.Text
                                + ", Enabled="
                                + window.ButtonDiagnosticsCapabilities.IsEnabled
                                + ", Requests="
                                + server.ReceivedRequests.Count
                                    .ToString(CultureInfo.InvariantCulture)
                                + Environment.NewLine
                                + window.TextExecutionLog.Text);
                        }
                        Click(window.ButtonLookupAxis);
                        WaitUntil(
                            () => window.ButtonReset.IsEnabled,
                            "The exact Reset axis was not reloaded on the newer transport.");
                        Click(window.ButtonReset);
                        WaitUntil(
                            () => string.Equals(
                                    window.TextOperationState.Text,
                                    "Reset completed",
                                    StringComparison.Ordinal)
                                && window.ButtonCloseConnection.IsEnabled,
                            "The restored Reset did not finish with new-session status-only proof.");
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(0, CountCommand(server, 0x2022));
                        AssertEx.Equal(4, CountCommand(server, 0x2028));
                        AssertEx.Equal(2, server.AcceptedClientCount);
                        AssertEx.Equal(0, CountCommandInSession(
                            server,
                            1,
                            0x405D));
                        CloseConnectedWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void RunResetTakeover(bool rejectStop)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var statusEntered = new ManualResetEventSlim(false))
            using (var oldSessionDisconnected = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisResetCommandStep());
                var heldStatus = AxisResetStatusStep();
                heldStatus.BeforeResponse = () =>
                {
                    statusEntered.Set();
                };
                heldStatus.WaitForClientDisconnectBeforeResponseAndContinue =
                    true;
                heldStatus.AfterClientDisconnect = () =>
                    oldSessionDisconnected.Set();
                steps.Add(heldStatus);
                steps.Add(InitStep());
                steps.Add(CallbackStep());
                steps.Add(CapabilitiesStep(1, capabilities));
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(rejectStop
                    ? new FakeRpcStep(
                        0x2022,
                        TestFrame.Response(0, TestFrame.Hex("01 00 F9 FF")))
                    : AxisStopCommandStep());
                if (rejectStop)
                {
                    steps.Add(CapabilitiesStep(2, capabilities));
                }
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(AxisResetStatusStep(state: 0x02000000u));
                steps.Add(CapabilitiesStep(
                    rejectStop ? 3u : 2u,
                    capabilities));
                steps.Add(CloseStep());
                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        ConnectAndLoadAxisForAxisCommand(window);
                        Click(window.ButtonReset);
                        WaitUntil(
                            () => statusEntered.IsSet,
                            "The Reset status observation was not held.");
                        var started = DateTime.UtcNow;
                        Click(window.ButtonStop);
                        WaitUntil(
                            () =>
                            {
                                var current = GetAxisCommandJournal(window)
                                    .CurrentRecord;
                                return current != null
                                    && current.IsActive
                                    && current.Operation
                                        == AxisCommandRecoveryOperation.Stop;
                            },
                            "Stop did not durably replace Reset before abort.",
                            1000);
                        WaitUntil(
                            () => oldSessionDisconnected.IsSet,
                            "The pinned safety abort did not close the held Reset transport promptly.",
                            1000);

                        if (!rejectStop)
                        {
                            WaitUntil(
                                () => string.Equals(
                                        window.TextOperationState.Text,
                                        "Stop verified",
                                        StringComparison.Ordinal)
                                    && window.ButtonCloseConnection.IsEnabled,
                                "The reconnect Stop did not prove standstill.");
                            AssertEx.False(
                                GetAxisCommandJournal(window).HasActiveRecord);
                        }
                        else
                        {
                            WaitUntil(
                                () =>
                                {
                                    var current = GetAxisCommandJournal(window)
                                        .CurrentRecord;
                                    return current != null
                                        && current.IsActive
                                        && current.Operation
                                            == AxisCommandRecoveryOperation.Reset
                                        && window.ButtonReset.IsEnabled;
                                },
                                "The Stop NACK did not restore the exact Reset predecessor.");
                            Click(window.ButtonReset);
                            WaitUntil(
                                () => string.Equals(
                                        window.TextOperationState.Text,
                                        "Reset completed",
                                        StringComparison.Ordinal)
                                    && window.ButtonCloseConnection.IsEnabled,
                                "The restored Reset did not finish status-only proof.");
                        }

                        AssertEx.True(
                            (DateTime.UtcNow - started).TotalSeconds < 8,
                            "Safety takeover did not complete promptly.");
                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(1, CountCommand(server, 0x2022));
                        AssertEx.Equal(4, CountCommand(server, 0x2028));
                        AssertEx.Equal(2, server.AcceptedClientCount);
                        AssertEx.Equal(0, CountCommandInSession(
                            server,
                            1,
                            0x2022));
                        AssertEx.Equal(1, CountCommandInSession(
                            server,
                            2,
                            0x2022));
                        AssertEx.Equal(0, CountCommandInSession(
                            server,
                            1,
                            0x405D));
                        CloseConnectedWindow(window);
                        AssertEx.Equal(1, CountCommandInSession(
                            server,
                            2,
                            0x405D));
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void
            AxisCommandMotionAndAcceptedStopRestartResolveInOrder()
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            var capabilities = LMCDiagnosticCapability.EtherCATTopology;
            var steps = CreateConnectAndTopologySteps(capabilities);
            steps.Add(CapabilitiesStep(11, capabilities));
            steps.Add(CapabilitiesStep(12, capabilities));
            steps.Add(D5AxisLookupStep(1));
            steps.Add(D5AxisInfoStep(1));
            steps.Add(CapabilitiesStep(13, capabilities));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(AxisResetStatusStep(state: 0x02000000u));
            steps.Add(CapabilitiesStep(14, capabilities));
            steps.Add(CapabilitiesStep(15, capabilities));
            steps.Add(CloseStep());
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    SeedMotionAndAcceptedStop(root, server.Port);
                    window = CreateWindow(root, server.Port);
                    ConnectAndLoadAxisForAxisCommand(window);
                    var observedMotionResolvedFirst = false;
                    window.AxisCommandBeforeDurableResolveTestHook = record =>
                    {
                        observedMotionResolvedFirst =
                            !GetMotionUncertaintyJournal(window)
                                .HasActiveRecord;
                    };
                    Click(window.ButtonStop);
                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Stop verified",
                                StringComparison.Ordinal)
                            && window.ButtonCloseConnection.IsEnabled,
                        "The coupled Motion/Stop restart proof did not finish.");
                    AssertEx.True(observedMotionResolvedFirst);
                    AssertEx.Equal(0, CountCommand(server, 0x2022));
                    AssertEx.Equal(3, CountCommand(server, 0x2028));
                    AssertEx.False(GetMotionUncertaintyJournal(window)
                        .HasActiveRecord);
                    AssertEx.False(GetAxisCommandJournal(window)
                        .HasActiveRecord);
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteAxisPowerOnTemporaryDirectory(root);
            }
        }

        private static void RunResetTakeoverTransportFailure(bool prewire)
        {
            var root = CreateAxisPowerOnTemporaryDirectory();
            MainWindow window = null;
            using (var statusEntered = new ManualResetEventSlim(false))
            using (var oldSessionDisconnected = new ManualResetEventSlim(false))
            {
                var capabilities = LMCDiagnosticCapability.EtherCATTopology;
                var steps = CreateConnectAndTopologySteps(capabilities);
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                steps.Add(AxisResetCommandStep());
                var heldStatus = AxisResetStatusStep();
                heldStatus.BeforeResponse = () =>
                {
                    statusEntered.Set();
                };
                heldStatus.WaitForClientDisconnectBeforeResponseAndContinue =
                    true;
                heldStatus.AfterClientDisconnect = () =>
                    oldSessionDisconnected.Set();
                steps.Add(heldStatus);
                steps.Add(InitStep());
                steps.Add(CallbackStep());
                steps.Add(CapabilitiesStep(1, capabilities));
                steps.Add(D5AxisLookupStep(1));
                steps.Add(D5AxisInfoStep(1));
                if (prewire)
                {
                    steps.Add(new FakeRpcStep(0, null)
                    {
                        RequireClientDisconnectBeforeRequest = true
                    });
                }
                else
                {
                    steps.Add(new FakeRpcStep(0x2022, null)
                    {
                        CloseClientBeforeResponse = true
                    });
                }

                try
                {
                    using (var server = new FakeRpcServer(steps.ToArray()))
                    {
                        window = CreateWindow(root, server.Port);
                        ConnectAndLoadAxisForAxisCommand(window);
                        Click(window.ButtonReset);
                        WaitUntil(
                            () => statusEntered.IsSet,
                            "The Reset status observation was not held.");
                        if (prewire)
                        {
                            window.AxisStopBeforeBeginDispatchTestHook =
                                (replacement, record) =>
                                    replacement
                                        .AbortTransportForSafetyPreemption();
                        }
                        Click(window.ButtonStop);
                        WaitUntil(
                            () =>
                            {
                                var current = GetAxisCommandJournal(window)
                                    .CurrentRecord;
                                return current != null
                                    && current.IsActive
                                    && current.Operation
                                        == AxisCommandRecoveryOperation.Stop;
                            },
                            "Stop did not durably replace Reset before transport failure.",
                            1000);
                        WaitUntil(
                            () => oldSessionDisconnected.IsSet,
                            "The pinned safety abort did not close the held Reset transport promptly.",
                            1000);
                        WaitUntil(
                            () =>
                            {
                                var current = GetAxisCommandJournal(window)
                                    .CurrentRecord;
                                return current != null
                                    && current.IsActive
                                    && current.Operation
                                        == (prewire
                                            ? AxisCommandRecoveryOperation.Reset
                                            : AxisCommandRecoveryOperation.Stop)
                                    && (prewire
                                        || current.State
                                            == AxisCommandRecoveryState.RecoveryRequired)
                                    && !(bool)GetPrivateField(
                                        window,
                                        "safetyCommandRunning");
                            },
                            "The replacement transport failure was not classified durably.");

                        AssertEx.Equal(1, CountCommand(server, 0x2024));
                        AssertEx.Equal(
                            prewire ? 0 : 1,
                            CountCommand(server, 0x2022));
                        var finalRecord = GetAxisCommandJournal(window)
                            .CurrentRecord;
                        AssertEx.Equal(
                            prewire
                                ? AxisCommandRecoveryOperation.Reset
                                : AxisCommandRecoveryOperation.Stop,
                            finalRecord.Operation);
                        if (!prewire)
                        {
                            AssertEx.Equal(
                                AxisCommandRecoveryState.RecoveryRequired,
                                finalRecord.State);
                        }
                        AssertEx.Equal(2, server.AcceptedClientCount);
                        AssertEx.Equal(0, CountCommandInSession(
                            server,
                            1,
                            0x2022));
                        AssertEx.Equal(
                            prewire ? 0 : 1,
                            CountCommandInSession(server, 2, 0x2022));
                        AssertEx.Equal(0, CountCommandInSession(
                            server,
                            1,
                            0x405D));
                        window.AxisStopBeforeBeginDispatchTestHook = null;
                        ForceCloseMotionRecoveryWindow(window);
                        window = null;
                        server.Verify();
                    }
                }
                finally
                {
                    CloseWindowBestEffort(window);
                    DeleteAxisPowerOnTemporaryDirectory(root);
                }
            }
        }

        private static void ConnectAndLoadAxisForAxisCommand(
            MainWindow window)
        {
            Click(window.ButtonConnect);
            WaitUntil(
                () => window.ButtonLookupAxis.IsEnabled,
                "The Axis command test did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => window.ButtonStop.IsEnabled
                    || window.ButtonReset.IsEnabled,
                "The Axis command test did not load the axis.");
        }

        private static AxisCommandRecoveryJournal GetAxisCommandJournal(
            MainWindow window)
        {
            return (AxisCommandRecoveryJournal)GetPrivateField(
                window,
                "axisCommandRecoveryJournal");
        }

        private static void SeedAcceptedAxisCommand(
            string root,
            int endpointPort,
            bool reset)
        {
            using (var journal = AxisCommandRecoveryJournal.Open(
                Path.Combine(root, "AxisCommandRecovery")))
            {
                var armed = journal.ArmBeforeDispatch(
                    reset
                        ? AxisCommandRecoveryOperation.Reset
                        : AxisCommandRecoveryOperation.Stop,
                    "127.0.0.1",
                    endpointPort,
                    "_LMCAxis1",
                    1,
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    reset ? 0 : 1000,
                    reset ? 0 : 10000,
                    3,
                    DateTime.UtcNow.AddSeconds(-2));
                journal.MarkAccepted(
                    armed.Identity,
                    DateTime.UtcNow);
            }
        }

        private static void SeedMotionAndAcceptedStop(
            string root,
            int endpointPort)
        {
            using (var motion = MotionUncertaintyJournal.Open(
                Path.Combine(root, "MotionUncertaintyRecovery")))
            {
                motion.ArmBeforeDispatch(
                    "127.0.0.1",
                    endpointPort,
                    MotionUncertaintyTargetKind.Axis,
                    "_LMCAxis1",
                    1,
                    "Move Absolute",
                    DiagnosticsBootId,
                    DiagnosticMapRevision,
                    DateTime.UtcNow.AddSeconds(-3));
            }
            SeedAcceptedAxisCommand(root, endpointPort, false);
        }
    }
}

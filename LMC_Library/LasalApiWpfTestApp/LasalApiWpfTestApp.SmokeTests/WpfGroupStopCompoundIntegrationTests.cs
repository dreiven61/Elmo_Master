using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        internal static void RegisterGroupStopCompoundTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.GroupStopCompound.QueuedPowerOnIsZeroWireThenOneStopThreeStable",
                QueuedPowerOnIsZeroWireThenOneStopThreeStable);
            tests.Add(
                "Wpf.GroupStopCompound.ExternalPowerOffPreemptsDelayedStatusAndTransmits",
                ExternalPowerOffPreemptsDelayedStatusAndTransmits);
            tests.Add(
                "Wpf.GroupStopCompound.DelayedResetResultDiscardedBeforeExternalStop",
                DelayedResetResultDiscardedBeforeExternalStop);
            tests.Add(
                "Wpf.GroupStopCompound.AcceptedStatusFailureCleanupResumesWithoutReplay",
                AcceptedStatusFailureCleanupResumesWithoutReplay);
        }

        private static void QueuedPowerOnIsZeroWireThenOneStopThreeStable()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupStopStep());
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            SemaphoreSlim commandGate = null;
            var gateOwned = false;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);
                    commandGate = (SemaphoreSlim)GetPrivateField(
                        window,
                        "commandSendGate");
                    AssertEx.True(commandGate.Wait(1000));
                    gateOwned = true;

                    Click(window.ButtonGroupPowerOn);
                    WaitUntil(
                        () => (bool)GetPrivateField(
                            window,
                            "operationRunning"),
                        "Group Power On did not queue behind commandSendGate.");

                    var currentGroup = (LMCGroupAxis)GetPrivateField(
                        window,
                        "group");
                    var stopTask = InvokeQualificationGroupStopWait(
                        window,
                        currentGroup,
                        1000,
                        1000,
                        true);

                    commandGate.Release();
                    gateOwned = false;

                    WaitUntil(
                        () => stopTask.IsCompleted
                            && !(bool)GetPrivateField(
                                window,
                                "operationRunning"),
                        "The queued Power On and qualification Stop did not settle.");
                    var result = stopTask.GetAwaiter().GetResult();

                    AssertEx.Equal(
                        LMCGroupStopSubmissionOutcome.Accepted,
                        result.SubmissionOutcome);
                    AssertEx.Equal(3, result.StatusPollCount);
                    AssertEx.Equal(3, result.StableSampleCount);
                    AssertEx.Equal(3, result.RequiredStableSampleCount);
                    AssertEx.Equal(
                        0,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204A),
                        "The queued ordinary Group Power On reached the wire.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2085));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));
                    AssertEx.Contains(
                        "cancelled before transmission",
                        window.TextExecutionLog.Text);

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (gateOwned && commandGate != null)
                {
                    commandGate.Release();
                }

                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            ExternalPowerOffPreemptsDelayedStatusAndTransmits()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupStopStep());
            var delayedOldStatus = GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby);
            delayedOldStatus.ResponseDelayMilliseconds = 500;
            steps.Add(delayedOldStatus);
            steps.Add(GroupPowerOffStep());
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(GroupEnableWaitStatusStep(0));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            var qualificationCancellation = new CancellationTokenSource();
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        true);
                    SetPrivateField(window, "qualificationRunning", true);
                    SetPrivateField(
                        window,
                        "qualificationCancellation",
                        qualificationCancellation);
                    InvokePrivate(window, "UpdateUiState");

                    AssertEx.True(
                        window.ButtonGroupPowerOff.IsEnabled,
                        "Group Power Off must remain available during qualification.");
                    AssertEx.True(
                        window.ButtonGroupStop.IsEnabled,
                        "Group Stop must remain available during qualification.");

                    var currentGroup = (LMCGroupAxis)GetPrivateField(
                        window,
                        "group");
                    var stopTask = InvokeQualificationGroupStopWait(
                        window,
                        currentGroup,
                        1000,
                        1000,
                        true);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045) == 1,
                        "The delayed qualification status read did not start.");
                    AssertEx.False(
                        stopTask.IsCompleted,
                        "The old compound completed before external safety reserved.");
                    AssertEx.True(window.ButtonGroupPowerOff.IsEnabled);

                    var coordinator = (LMCSendPriorityCoordinator)
                        GetPrivateField(window, "sendPriorityCoordinator");
                    var oldGeneration = coordinator.CurrentGeneration;
                    Click(window.ButtonGroupPowerOff);
                    AssertEx.Equal(
                        oldGeneration + 1,
                        coordinator.CurrentGeneration,
                        "External Power Off did not reserve priority before waiting for the gate.");
                    AssertEx.True(
                        qualificationCancellation.IsCancellationRequested,
                        "External Power Off did not cancel the active qualification.");

                    var oldError = ObserveTaskFailure(
                        stopTask,
                        "The preempted qualification compound did not finish.");
                    var preempted = oldError as LMCSendPreemptedException;
                    AssertEx.NotNull(
                        preempted,
                        "Expected exact SDK send-preemption evidence, actual "
                            + oldError.GetType().FullName
                            + ": "
                            + oldError.Message);
                    AssertEx.Equal(
                        LMCSendPreemptionPhase.ResultDiscarded,
                        preempted.Phase);
                    AssertEx.Equal((ushort)0x2045, preempted.Command);

                    var pendingStop = GetPrivateField(
                        window,
                        "pendingGroupStopWaitContinuation")
                        as LMCGroupStopWaitContinuation;
                    AssertEx.True(
                        pendingStop != null && pendingStop.IsPending,
                        "The accepted GroupStop continuation was not retained after status preemption.");
                    AssertEx.True(ReferenceEquals(
                        pendingStop,
                        currentGroup.PendingGroupStopWaitContinuation));

                    WaitUntil(
                        () => CountRequestCommand(
                                server.ReceivedRequests,
                                0x204B) == 1
                            && !(bool)GetPrivateField(
                                window,
                                "groupPowerOffVerificationPending")
                            && window.TextGroupResult.Text.IndexOf(
                                "Group Power Off verified",
                                StringComparison.Ordinal) >= 0,
                        "External Group Power Off did not transmit and verify stable PowerOn=False.");

                    var qualificationLog = string.Join(
                        Environment.NewLine,
                        (List<string>)GetPrivateField(
                            window,
                            "qualificationLogLines"));
                    AssertEx.True(
                        qualificationLog.IndexOf(
                            "event=ACK|cmd=0x2085|submission=Accepted|phase=begin|statusReads=0",
                            StringComparison.Ordinal) >= 0,
                        "The accepted Begin boundary was not logged before status verification was preempted.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2085));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x204B));
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    ClearManualQualificationState(
                        window,
                        qualificationCancellation);
                    qualificationCancellation = null;
                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                if (window != null && qualificationCancellation != null)
                {
                    ClearManualQualificationState(
                        window,
                        qualificationCancellation);
                    qualificationCancellation = null;
                }

                CloseWindowBestEffort(window);
                if (qualificationCancellation != null)
                {
                    qualificationCancellation.Dispose();
                }

                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            AcceptedStatusFailureCleanupResumesWithoutReplay()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            steps.Add(GroupStopStep());
            steps.Add(GroupStopFailedStatusStep());
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);
                    var currentGroup = (LMCGroupAxis)GetPrivateField(
                        window,
                        "group");

                    var primaryError = ObserveTaskFailure(
                        InvokeQualificationGroupStopWait(
                            window,
                            currentGroup,
                            1000,
                            1000,
                            true),
                        "The accepted GroupStop status failure did not surface.");
                    AssertEx.True(
                        primaryError is LMCGroupStopStatusException,
                        "Expected a typed accepted-status failure, actual "
                            + primaryError.GetType().FullName
                            + ": "
                            + primaryError.Message);

                    var pendingStop = GetPrivateField(
                        window,
                        "pendingGroupStopWaitContinuation")
                        as LMCGroupStopWaitContinuation;
                    AssertEx.True(
                        pendingStop != null && pendingStop.IsPending,
                        "The accepted GroupStop continuation was not preserved for cleanup.");
                    AssertEx.True(ReferenceEquals(
                        pendingStop,
                        currentGroup.PendingGroupStopWaitContinuation));

                    var cleanup = (Task)InvokePrivate(
                        window,
                        "CleanupQualificationGroupMotionAsync",
                        currentGroup,
                        1000,
                        1000);
                    WaitUntil(
                        () => cleanup.IsCompleted,
                        "The status-only GroupStop cleanup did not finish.");
                    cleanup.GetAwaiter().GetResult();

                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2085),
                        "Cleanup replayed an already accepted GroupStop.");
                    AssertEx.Equal(
                        4,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045),
                        "Expected one failed status and three status-only cleanup proof reads.");
                    AssertEx.True(
                        GetPrivateField(
                            window,
                            "pendingGroupStopWaitContinuation") == null,
                        "Successful status-only cleanup retained the WPF continuation.");
                    AssertEx.True(
                        currentGroup.PendingGroupStopWaitContinuation == null,
                        "Successful status-only cleanup retained the SDK continuation.");
                    AssertEx.Contains(
                        "action=resume_accepted_GroupStop_status_only",
                        string.Join(
                            Environment.NewLine,
                            (List<string>)GetPrivateField(
                                window,
                                "qualificationLogLines")));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static void
            DelayedResetResultDiscardedBeforeExternalStop()
        {
            var steps = CreateConnectAndTopologySteps(
                LMCDiagnosticCapability.EtherCATTopology);
            steps.Add(GroupEnableWaitLookupStep());
            var delayedReset = CreateGroupResetStep();
            delayedReset.ResponseDelayMilliseconds = 400;
            steps.Add(delayedReset);
            var delayedStop = GroupStopStep();
            delayedStop.ResponseDelayMilliseconds = 400;
            steps.Add(delayedStop);
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(GroupEnableWaitStatusStep(
                GroupEnableWaitPowerOn | GroupEnableWaitStandby));
            steps.Add(CloseStep());

            var journalDirectory = CreateJournalDirectory();
            MainWindow window = null;
            try
            {
                using (var server = new FakeRpcServer(steps.ToArray()))
                {
                    window = CreatePreparedGroupPowerWindow(
                        journalDirectory,
                        server.Port,
                        false);
                    AssertEx.True(window.ButtonGroupReset.IsEnabled);
                    window.TextGroupResult.Text = "RESET_RESULT_SENTINEL";

                    Click(window.ButtonGroupReset);
                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2049) == 1,
                        "The delayed Group Reset request did not start.");
                    AssertEx.True(
                        window.ButtonGroupStop.IsEnabled,
                        "Group Stop must remain available while Reset drains its response.");

                    var coordinator = (LMCSendPriorityCoordinator)
                        GetPrivateField(window, "sendPriorityCoordinator");
                    var oldGeneration = coordinator.CurrentGeneration;
                    Click(window.ButtonGroupStop);
                    AssertEx.Equal(
                        oldGeneration + 1,
                        coordinator.CurrentGeneration);

                    WaitUntil(
                        () => CountRequestCommand(
                            server.ReceivedRequests,
                            0x2085) == 1,
                        "External Group Stop did not transmit after Reset response drain.");
                    AssertEx.Equal(
                        "RESET_RESULT_SENTINEL",
                        window.TextGroupResult.Text,
                        "The stale Reset ACK was applied before external Stop completed.");
                    AssertEx.Contains(
                        "response for command 0x2049 was discarded",
                        window.TextExecutionLog.Text);
                    AssertEx.False(
                        window.TextExecutionLog.Text.IndexOf(
                            "Group Reset PASS",
                            StringComparison.Ordinal) >= 0,
                        "The stale Reset ACK was reported as PASS.");

                    WaitUntil(
                        () => string.Equals(
                                window.TextOperationState.Text,
                                "Group Stop verified",
                                StringComparison.Ordinal)
                            && !(bool)GetPrivateField(
                                window,
                                "safetyCommandRunning")
                            && (int)GetPrivateField(
                                window,
                                "safetyMonitorCount") == 0,
                        "External Group Stop did not finish stable verification.");
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2049));
                    AssertEx.Equal(
                        1,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2085));
                    AssertEx.Equal(
                        3,
                        CountRequestCommand(
                            server.ReceivedRequests,
                            0x2045));

                    CloseConnectedWindow(window);
                    window = null;
                    server.Verify();
                }
            }
            finally
            {
                CloseWindowBestEffort(window);
                DeleteJournalDirectory(journalDirectory);
            }
        }

        private static Task<LMCGroupStopWaitResult>
            InvokeQualificationGroupStopWait(
                MainWindow window,
                LMCGroupAxis currentGroup,
                int decelerationRaw,
                int jerkRaw,
                bool logAcknowledgement)
        {
            return (Task<LMCGroupStopWaitResult>)InvokePrivate(
                window,
                "DispatchQualificationGroupStopWaitAsync",
                "WPF smoke Group Stop compound",
                currentGroup,
                decelerationRaw,
                jerkRaw,
                logAcknowledgement);
        }

        private static Exception ObserveTaskFailure(
            Task task,
            string waitMessage)
        {
            WaitUntil(() => task.IsCompleted, waitMessage);
            try
            {
                task.GetAwaiter().GetResult();
            }
            catch (Exception error)
            {
                return error;
            }

            throw new InvalidOperationException(
                "Expected the task to fail, but it completed successfully.");
        }

        private static void ClearManualQualificationState(
            MainWindow window,
            CancellationTokenSource cancellation)
        {
            SetPrivateField(window, "qualificationRunning", false);
            SetPrivateField(window, "qualificationCancellation", null);
            cancellation.Dispose();
            InvokePrivate(window, "UpdateUiState");
        }

        private static FakeRpcStep CreateGroupResetStep()
        {
            return new FakeRpcStep(
                0x2049,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00 00 00 00 00")))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2049,
                        GroupEnableWaitReference,
                        new byte[] { 1 }),
                    request)
            };
        }

        private static FakeRpcStep GroupStopFailedStatusStep()
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(
                payload,
                0,
                GroupEnableWaitPowerOn | GroupEnableWaitStandby);
            TestFrame.WriteUInt16(payload, 4, 0x0010);
            TestFrame.WriteInt16(payload, 6, -31);
            TestFrame.WriteUInt16(payload, 8, 7);

            var requestPayload = new byte[8];
            TestFrame.WriteInt32(
                requestPayload,
                0,
                GroupEnableWaitReference);
            TestFrame.WriteInt32(requestPayload, 4, 1);

            return new FakeRpcStep(
                0x2045,
                TestFrame.Response(0, payload))
            {
                InspectRequest = request => AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x2045,
                        GroupEnableWaitReference,
                        requestPayload),
                    request)
            };
        }
    }
}

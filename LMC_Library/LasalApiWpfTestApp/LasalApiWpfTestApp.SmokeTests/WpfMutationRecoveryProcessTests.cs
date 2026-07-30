using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static class WpfMutationRecoveryProcessTests
    {
        private const string ChildMode = "--mutation-recovery-child";
        private const string ReadyPrefix = "READY mutation recovery ";
        private const string ObservationRequest = "OBSERVE";
        private const string ObservationPrefix =
            "OBSERVED mutation recovery ";
        private const string CleanupExitRequest = "EXIT";
        private const string RecorderDoubleScenario = "RecorderDouble";
        private const string MotionDispatchScenario = "MotionDispatchKill";
        private const string MotionRecoveryScenario = "MotionRecoveryStop";
        private const string AxisPowerAcceptedDispatchScenario =
            "AxisPowerAcceptedDispatchKill";
        private const string AxisPowerAcceptedRecoveryScenario =
            "AxisPowerAcceptedStatusRecovery";
        private const string AxisPowerOffAcceptedDispatchScenario =
            "AxisPowerOffAcceptedDispatchKill";
        private const string AxisPowerOffAcceptedRecoveryScenario =
            "AxisPowerOffAcceptedStatusRecovery";
        private const string AxisStopAcceptedDispatchScenario =
            "AxisStopAcceptedDispatchKill";
        private const string AxisStopAcceptedRecoveryScenario =
            "AxisStopAcceptedStatusRecovery";
        private const string AxisResetAcceptedDispatchScenario =
            "AxisResetAcceptedDispatchKill";
        private const string AxisResetAcceptedRecoveryScenario =
            "AxisResetAcceptedStatusRecovery";
        private const string AxisStopArmedDispatchScenario =
            "AxisStopAckBeforeDurableMarkKill";
        private const string AxisStopArmedRestartScenario =
            "AxisStopArmedRestartInspection";
        private const string AxisResetArmedDispatchScenario =
            "AxisResetAckBeforeDurableMarkKill";
        private const string AxisResetArmedRestartScenario =
            "AxisResetArmedRestartInspection";
        private const string MotionAxisStopAcceptedDispatchScenario =
            "MotionAxisStopAcceptedDispatchKill";
        private const string MotionAxisStopAcceptedRecoveryScenario =
            "MotionAxisStopAcceptedStatusRecovery";
        private const string MotionAxisStopFinalRecoveryScenario =
            "MotionAxisStopFinalStatusRecovery";
        private const string GroupPowerOnAcceptedDispatchScenario =
            "GroupPowerOnAcceptedDispatchKill";
        private const string GroupPowerOnAcceptedRecoveryScenario =
            "GroupPowerOnAcceptedStatusRecovery";
        private const string GroupPowerOffAcceptedDispatchScenario =
            "GroupPowerOffAcceptedDispatchKill";
        private const string GroupPowerOffAcceptedRecoveryScenario =
            "GroupPowerOffAcceptedStatusRecovery";
        private const string GroupEnableAcceptedDispatchScenario =
            "GroupEnableAcceptedDispatchKill";
        private const string GroupEnableAcceptedRecoveryScenario =
            "GroupEnableAcceptedStatusRecovery";
        private const string GroupDisableAcceptedDispatchScenario =
            "GroupDisableAcceptedDispatchKill";
        private const string GroupDisableAcceptedRecoveryScenario =
            "GroupDisableAcceptedStatusRecovery";
        private const string TestDirectoryPrefix = "ElmoWpfRecovery-";
        private const int WaitTimeoutMilliseconds = 15000;
        private const uint BootId = 0x12345678u;
        private const uint MotionDiagnosticsBootId = 0x10203040u;
        private const uint MotionMapRevision = 1u;
        private const uint AxisPowerDiagnosticsBootId = 0x10203040u;
        private const uint AxisPowerMapRevision = 0xE245539Au;
        private const string MotionAxisName = "_LMCAxis1";
        private const ushort MotionAxisReference = 1;
        private const string MotionOperation = "Move Absolute";
        private const string GroupPowerName = "_LMCRobotBase1";
        private const ushort GroupPowerReference = 0x0100;
        private const uint GroupProfileLockDiagnosticsBootId = 0x10203040u;
        private const uint GroupProfileLockMapRevision = 0xE245539Au;
        private const uint RecorderDoubleMapRevision = 0x957F101Eu;
        private const uint RecorderDoubleConfigId = 0x31415926u;
        private const long SessionGeneration = 7;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.MutationRecovery.ProcessTerminationPreservesInterlockAndNoReplay",
                ProcessTerminationPreservesInterlockAndNoReplay);
            tests.Add(
                "Wpf.Recorder.DoubleProcessTerminationPreservesJournalAndNoReplay",
                DoubleProcessTerminationPreservesJournalAndNoReplay);
            tests.Add(
                "Wpf.MotionRecovery.ProcessTerminationPreservesJournalNoReplayAndStopResolves",
                MotionProcessTerminationPreservesJournalNoReplayAndStopResolves);
            tests.Add(
                "Wpf.AxisPowerOnRecovery.ProcessTerminationAcceptedRestartIsStatusOnly",
                AxisPowerOnAcceptedProcessTerminationRestartIsStatusOnly);
            tests.Add(
                "Wpf.AxisPowerOffRecovery.ProcessTerminationAcceptedRestartIsStatusOnlyAndReacquiresJournal",
                AxisPowerOffAcceptedProcessTerminationRestartIsStatusOnly);
            tests.Add(
                "Wpf.AxisCommandRecovery.StopProcessTerminationAcceptedRestartIsStatusOnly",
                AxisStopAcceptedProcessTerminationRestartIsStatusOnly);
            tests.Add(
                "Wpf.AxisCommandRecovery.ResetProcessTerminationAcceptedRestartIsStatusOnly",
                AxisResetAcceptedProcessTerminationRestartIsStatusOnly);
            tests.Add(
                "Wpf.AxisCommandRecovery.AckBeforeDurableMarkProcessTerminationPromotesArmedWithoutReplay",
                AxisCommandAckBeforeDurableMarkProcessTerminationPromotesArmedWithoutReplay);
            tests.Add(
                "Wpf.AxisCommandRecovery.MotionAndStopProcessTerminationResolvesMotionThenStop",
                MotionAndStopProcessTerminationResolvesMotionThenStop);
            tests.Add(
                "Wpf.GroupPowerRecovery.ProcessTerminationAcceptedRestartIsStatusOnlyAndReacquiresJournal",
                GroupPowerAcceptedProcessTerminationRestartIsStatusOnly);
            tests.Add(
                "Wpf.GroupEnableRecovery.ProcessTerminationAcceptedRestartIsStatusOnlyAndReacquiresJournal",
                GroupEnableAcceptedProcessTerminationRestartIsStatusOnly);
            tests.Add(
                "Wpf.GroupDisableRecovery.ProcessTerminationAcceptedRestartIsStatusOnlyAndReacquiresJournal",
                GroupDisableAcceptedProcessTerminationRestartIsStatusOnly);
        }

        internal static bool IsChildInvocation(string[] args)
        {
            return args != null
                && args.Length != 0
                && string.Equals(
                    args[0],
                    ChildMode,
                    StringComparison.Ordinal);
        }

        internal static int RunChild(string[] args)
        {
            try
            {
                if (args == null || args.Length != 4)
                {
                    Console.Error.WriteLine(
                        "ERROR mutation-recovery child requires directory, RPC port, and mutation kind.");
                    return 64;
                }

                var directoryPath = RequireTestDirectoryPath(args[1]);
                int rpcPort;
                if (!int.TryParse(
                        args[2],
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out rpcPort)
                    || rpcPort < 1
                    || rpcPort > 65535)
                {
                    Console.Error.WriteLine(
                        "ERROR mutation-recovery child RPC port is invalid.");
                    return 64;
                }

                if (string.Equals(
                    args[3],
                    RecorderDoubleScenario,
                    StringComparison.Ordinal))
                {
                    RunRecorderDoubleChildCore(directoryPath, rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    MotionDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunMotionDispatchChildCore(directoryPath, rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    MotionRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunMotionRecoveryChildCore(directoryPath, rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisPowerAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisPowerAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisPowerAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisPowerAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisPowerOffAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisPowerOffAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisPowerOffAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisPowerOffAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisStopAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort,
                        false);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisStopAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort,
                        false);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisResetAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort,
                        true);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisResetAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort,
                        true);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisStopArmedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandArmedDispatchChildCore(
                        directoryPath,
                        rpcPort,
                        false);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisResetArmedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandArmedDispatchChildCore(
                        directoryPath,
                        rpcPort,
                        true);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisStopArmedRestartScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandArmedRestartInspectionChildCore(
                        directoryPath,
                        rpcPort,
                        false);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    AxisResetArmedRestartScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandArmedRestartInspectionChildCore(
                        directoryPath,
                        rpcPort,
                        true);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    MotionAxisStopAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunMotionAxisStopAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    MotionAxisStopAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunMotionAxisStopAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    MotionAxisStopFinalRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunAxisCommandAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort,
                        false);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupPowerOnAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupPowerAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort,
                        true);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupPowerOnAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupPowerAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort,
                        true);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupPowerOffAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupPowerAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort,
                        false);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupPowerOffAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupPowerAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort,
                        false);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupEnableAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupEnableAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupEnableAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupEnableAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupDisableAcceptedDispatchScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupDisableAcceptedDispatchChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                if (string.Equals(
                    args[3],
                    GroupDisableAcceptedRecoveryScenario,
                    StringComparison.Ordinal))
                {
                    RunGroupDisableAcceptedRecoveryChildCore(
                        directoryPath,
                        rpcPort);
                    return 0;
                }

                DiagnosticsMutationKind kind;
                if (!Enum.TryParse(args[3], false, out kind)
                    || (kind != DiagnosticsMutationKind.SdoWrite
                        && kind
                            != DiagnosticsMutationKind.DigitalOutputWrite))
                {
                    Console.Error.WriteLine(
                        "ERROR mutation-recovery child RPC port or mutation kind is invalid.");
                    return 64;
                }

                RunChildCore(directoryPath, rpcPort, GetScenario(kind));
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(
                    "ERROR mutation-recovery WPF child failed.");
                Console.Error.WriteLine(error);
                return 1;
            }
        }

        private static void ProcessTerminationPreservesInterlockAndNoReplay()
        {
            RunScenario(GetScenario(DiagnosticsMutationKind.SdoWrite));
            RunScenario(
                GetScenario(DiagnosticsMutationKind.DigitalOutputWrite));
        }

        private static void
            AxisPowerOnAcceptedProcessTerminationRestartIsStatusOnly()
        {
            var directoryPath = CreateTestDirectoryPath();
            var axisJournalDirectory = Path.Combine(
                directoryPath,
                "AxisPowerOnRecovery");
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess recoveryChild = null;
            using (var firstStatusRelease = new ManualResetEventSlim(false))
            using (var firstStatusEntered = new ManualResetEventSlim(false))
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateAxisPowerOnProcessRpcSteps(
                            firstStatusRelease,
                            firstStatusEntered)))
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        AxisPowerAcceptedDispatchScenario);
                    dispatchChild.WaitUntilReady();
                    AssertEx.True(
                        firstStatusEntered.Wait(WaitTimeoutMilliseconds),
                        "The first Axis Power On status request was not held by the fake server.");
                    dispatchChild.RequestObservationBarrier();
                    AssertEx.False(dispatchChild.Process.HasExited);
                    AssertEx.Throws<IOException>(
                        () => AxisPowerOnRecoveryJournal.Open(
                            axisJournalDirectory));

                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;
                    firstStatusRelease.Set();

                    Guid identity;
                    using (var journal = AxisPowerOnRecoveryJournal.Open(
                        axisJournalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                            journal.CurrentRecord.State);
                        identity = journal.CurrentRecord.Identity;
                    }

                    recoveryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        AxisPowerAcceptedRecoveryScenario);
                    recoveryChild.WaitUntilReady();
                    recoveryChild.WaitForSuccessfulExit();
                    recoveryChild.Dispose();
                    recoveryChild = null;
                    server.Verify();

                    var powerOnRequests = server.ReceivedRequests
                        .Select((request, index) => new
                        {
                            Request = request,
                            Session = server
                                .ReceivedRequestSessionOrdinals[index]
                        })
                        .Where(item =>
                            TestFrame.ReadUInt16(item.Request, 0) == 0x2023
                            && item.Request.Length > 12
                            && item.Request[12] == 1)
                        .ToArray();
                    AssertEx.Equal(1, powerOnRequests.Length);
                    AssertEx.Equal(1, powerOnRequests[0].Session);
                    AssertEx.False(server.ReceivedRequests
                        .Select((request, index) => new
                        {
                            Request = request,
                            Session = server
                                .ReceivedRequestSessionOrdinals[index]
                        })
                        .Any(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2023));

                    using (var journal = AxisPowerOnRecoveryJournal.Open(
                        axisJournalDirectory))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                        AssertEx.Equal(
                            AxisPowerOnRecoveryState.Resolved,
                            journal.CurrentRecord.State);
                    }
                }
            }
            finally
            {
                firstStatusRelease.Set();
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }

                if (recoveryChild != null)
                {
                    recoveryChild.Dispose();
                }

                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            AxisPowerOffAcceptedProcessTerminationRestartIsStatusOnly()
        {
            var directoryPath = CreateTestDirectoryPath();
            var axisJournalDirectory = Path.Combine(
                directoryPath,
                "AxisPowerOnRecovery");
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess recoveryChild = null;
            using (var firstStatusRelease = new ManualResetEventSlim(false))
            using (var firstStatusEntered = new ManualResetEventSlim(false))
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateAxisPowerOffProcessRpcSteps(
                            firstStatusRelease,
                            firstStatusEntered)))
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        AxisPowerOffAcceptedDispatchScenario);
                    AssertEx.True(
                        firstStatusEntered.Wait(WaitTimeoutMilliseconds),
                        "The first Axis Power Off status request was not held by the fake server.");
                    dispatchChild.WaitUntilReady();
                    dispatchChild.RequestObservationBarrier();
                    AssertEx.False(dispatchChild.Process.HasExited);
                    AssertEx.Throws<IOException>(
                        () => AxisPowerOnRecoveryJournal.Open(
                            axisJournalDirectory),
                        "The live Axis Power Off child must retain the journal single-writer lock.");

                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;
                    firstStatusRelease.Set();

                    Guid identity;
                    using (var journal = AxisPowerOnRecoveryJournal.Open(
                        axisJournalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                            journal.CurrentRecord.State);
                        AssertAxisPowerRecoveryIdentity(
                            journal.CurrentRecord,
                            server.Port,
                            false);
                        identity = journal.CurrentRecord.Identity;
                    }

                    recoveryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        AxisPowerOffAcceptedRecoveryScenario);
                    recoveryChild.WaitUntilReady();
                    recoveryChild.WaitForSuccessfulExit();
                    recoveryChild.Dispose();
                    recoveryChild = null;
                    server.Verify();

                    var received = server.ReceivedRequests
                        .Select((request, index) => new
                        {
                            Request = request,
                            Session = server
                                .ReceivedRequestSessionOrdinals[index]
                        })
                        .ToArray();
                    var powerOffRequests = received
                        .Where(item =>
                            TestFrame.ReadUInt16(item.Request, 0) == 0x2023
                            && item.Request.Length > 12
                            && item.Request[12] == 0)
                        .ToArray();
                    AssertEx.Equal(1, powerOffRequests.Length);
                    AssertEx.Equal(1, powerOffRequests[0].Session);
                    AssertEx.Equal(
                        0,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(
                                item.Request,
                                0) == 0x2023));
                    AssertEx.Equal(
                        3,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(
                                item.Request,
                                0) == 0x2028));

                    using (var journal = AxisPowerOnRecoveryJournal.Open(
                        axisJournalDirectory))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                        AssertEx.Equal(
                            AxisPowerOnRecoveryState.Resolved,
                            journal.CurrentRecord.State);
                        AssertAxisPowerRecoveryIdentity(
                            journal.CurrentRecord,
                            server.Port,
                            false);
                    }
                }
            }
            finally
            {
                firstStatusRelease.Set();
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }

                if (recoveryChild != null)
                {
                    recoveryChild.Dispose();
                }

                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            AxisStopAcceptedProcessTerminationRestartIsStatusOnly()
        {
            RunAxisCommandAcceptedProcessTerminationRestartIsStatusOnly(
                false);
        }

        private static void
            AxisResetAcceptedProcessTerminationRestartIsStatusOnly()
        {
            RunAxisCommandAcceptedProcessTerminationRestartIsStatusOnly(
                true);
        }

        private static void
            RunAxisCommandAcceptedProcessTerminationRestartIsStatusOnly(
                bool reset)
        {
            var directoryPath = CreateTestDirectoryPath();
            var journalDirectory = Path.Combine(
                directoryPath,
                "AxisCommandRecovery");
            var dispatchScenario = reset
                ? AxisResetAcceptedDispatchScenario
                : AxisStopAcceptedDispatchScenario;
            var recoveryScenario = reset
                ? AxisResetAcceptedRecoveryScenario
                : AxisStopAcceptedRecoveryScenario;
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess recoveryChild = null;
            using (var firstStatusRelease = new ManualResetEventSlim(false))
            using (var firstStatusEntered = new ManualResetEventSlim(false))
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateAxisCommandProcessRpcSteps(
                            reset,
                            firstStatusRelease,
                            firstStatusEntered)))
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        dispatchScenario);
                    AssertEx.True(
                        firstStatusEntered.Wait(WaitTimeoutMilliseconds),
                        "The first Axis Stop/Reset status request was not held.");
                    dispatchChild.WaitUntilReady();
                    dispatchChild.RequestObservationBarrier();
                    AssertEx.False(dispatchChild.Process.HasExited);
                    AssertEx.Throws<IOException>(
                        () => AxisCommandRecoveryJournal.Open(
                            journalDirectory),
                        "The live Axis command child must retain the journal writer lock.");

                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;
                    firstStatusRelease.Set();

                    Guid identity;
                    using (var journal = AxisCommandRecoveryJournal.Open(
                        journalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            reset
                                ? AxisCommandRecoveryOperation.Reset
                                : AxisCommandRecoveryOperation.Stop,
                            journal.CurrentRecord.Operation);
                        AssertEx.Equal(
                            AxisCommandRecoveryState.AcceptedAwaitingProof,
                            journal.CurrentRecord.State);
                        identity = journal.CurrentRecord.Identity;
                    }

                    recoveryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        recoveryScenario);
                    recoveryChild.WaitUntilReady();
                    recoveryChild.WaitForSuccessfulExit();
                    recoveryChild.Dispose();
                    recoveryChild = null;
                    server.Verify();

                    var command = reset ? (ushort)0x2024 : (ushort)0x2022;
                    var received = server.ReceivedRequests
                        .Select((request, index) => new
                        {
                            Request = request,
                            Session = server
                                .ReceivedRequestSessionOrdinals[index]
                        })
                        .ToArray();
                    AssertEx.Equal(
                        1,
                        received.Count(item =>
                            TestFrame.ReadUInt16(item.Request, 0)
                                == command));
                    AssertEx.Equal(
                        0,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == command));
                    AssertEx.Equal(
                        3,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2028));

                    using (var journal = AxisCommandRecoveryJournal.Open(
                        journalDirectory))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                        AssertEx.Equal(
                            AxisCommandRecoveryState.Resolved,
                            journal.CurrentRecord.State);
                    }
                }
            }
            finally
            {
                firstStatusRelease.Set();
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }
                if (recoveryChild != null)
                {
                    recoveryChild.Dispose();
                }
                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            AxisCommandAckBeforeDurableMarkProcessTerminationPromotesArmedWithoutReplay()
        {
            RunAxisCommandAckBoundaryProcessTermination(false);
            RunAxisCommandAckBoundaryProcessTermination(true);
        }

        private static void RunAxisCommandAckBoundaryProcessTermination(
            bool reset)
        {
            var directoryPath = CreateTestDirectoryPath();
            var journalDirectory = Path.Combine(
                directoryPath,
                "AxisCommandRecovery");
            var dispatchScenario = reset
                ? AxisResetArmedDispatchScenario
                : AxisStopArmedDispatchScenario;
            var restartScenario = reset
                ? AxisResetArmedRestartScenario
                : AxisStopArmedRestartScenario;
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess restartChild = null;
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateAxisCommandAckBoundaryProcessRpcSteps(reset)))
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        dispatchScenario);
                    dispatchChild.WaitUntilReady();
                    AssertEx.False(dispatchChild.Process.HasExited);
                    AssertEx.Throws<IOException>(
                        () => AxisCommandRecoveryJournal.Open(
                            journalDirectory),
                        "The ACK-boundary child must retain the Axis command journal writer lock.");

                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;

                    AxisCommandRecoveryRecord armedSnapshot;
                    using (var journal = AxisCommandRecoveryJournal.Open(
                        journalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            reset
                                ? AxisCommandRecoveryOperation.Reset
                                : AxisCommandRecoveryOperation.Stop,
                            journal.CurrentRecord.Operation);
                        AssertEx.Equal(
                            AxisCommandRecoveryState.ArmedBeforeDispatch,
                            journal.CurrentRecord.State);
                        armedSnapshot = journal.CurrentRecord.Copy();
                    }

                    restartChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        restartScenario);
                    restartChild.WaitUntilReady();
                    restartChild.WaitForSuccessfulExit();
                    restartChild.Dispose();
                    restartChild = null;
                    server.Verify();

                    var receivedCommands = server.ReceivedRequests
                        .Select(request => TestFrame.ReadUInt16(request, 0))
                        .ToArray();
                    AssertEx.Equal(
                        1,
                        receivedCommands.Count(command => command
                            == (reset ? (ushort)0x2024 : (ushort)0x2022)));
                    AssertEx.Equal(
                        0,
                        receivedCommands.Count(command => command == 0x2028));
                    AssertEx.Equal(
                        0,
                        receivedCommands.Count(command => command
                            == (reset ? (ushort)0x2022 : (ushort)0x2024)));

                    using (var journal = AxisCommandRecoveryJournal.Open(
                        journalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            armedSnapshot.Identity,
                            journal.CurrentRecord.Identity);
                        AssertEx.Equal(
                            AxisCommandRecoveryState.RecoveryRequired,
                            journal.CurrentRecord.State);
                        AssertEx.Equal(
                            armedSnapshot.EndpointIp,
                            journal.CurrentRecord.EndpointIp);
                        AssertEx.Equal(
                            armedSnapshot.EndpointPort,
                            journal.CurrentRecord.EndpointPort);
                        AssertEx.Equal(
                            armedSnapshot.AxisName,
                            journal.CurrentRecord.AxisName);
                        AssertEx.Equal(
                            armedSnapshot.AxisReference,
                            journal.CurrentRecord.AxisReference);
                        AssertEx.Equal(
                            armedSnapshot.DiagnosticsBootId,
                            journal.CurrentRecord.DiagnosticsBootId);
                        AssertEx.Equal(
                            armedSnapshot.MapRevision,
                            journal.CurrentRecord.MapRevision);
                        AssertEx.Equal(
                            armedSnapshot.StopDeceleration,
                            journal.CurrentRecord.StopDeceleration);
                        AssertEx.Equal(
                            armedSnapshot.StopJerk,
                            journal.CurrentRecord.StopJerk);
                    }
                }
            }
            finally
            {
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }
                if (restartChild != null)
                {
                    restartChild.Dispose();
                }
                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            MotionAndStopProcessTerminationResolvesMotionThenStop()
        {
            var directoryPath = CreateTestDirectoryPath();
            var motionJournalDirectory = Path.Combine(
                directoryPath,
                "MotionUncertaintyRecovery");
            var axisJournalDirectory = Path.Combine(
                directoryPath,
                "AxisCommandRecovery");
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess boundaryChild = null;
            RecoveryChildProcess finalChild = null;
            using (var firstStatusRelease = new ManualResetEventSlim(false))
            using (var firstStatusEntered = new ManualResetEventSlim(false))
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateMotionAxisStopProcessRpcSteps(
                            firstStatusRelease,
                            firstStatusEntered)))
                {
                    using (var motion = MotionUncertaintyJournal.Open(
                        motionJournalDirectory))
                    {
                        motion.ArmBeforeDispatch(
                            "127.0.0.1",
                            server.Port,
                            MotionUncertaintyTargetKind.Axis,
                            MotionAxisName,
                            MotionAxisReference,
                            MotionOperation,
                            AxisPowerDiagnosticsBootId,
                            AxisPowerMapRevision,
                            DateTime.UtcNow.AddSeconds(-2));
                    }

                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        MotionAxisStopAcceptedDispatchScenario);
                    AssertEx.True(
                        firstStatusEntered.Wait(WaitTimeoutMilliseconds),
                        "The first Motion/Stop status request was not held.");
                    dispatchChild.WaitUntilReady();
                    dispatchChild.RequestObservationBarrier();
                    AssertEx.Throws<IOException>(
                        () => MotionUncertaintyJournal.Open(
                            motionJournalDirectory));
                    AssertEx.Throws<IOException>(
                        () => AxisCommandRecoveryJournal.Open(
                            axisJournalDirectory));
                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;
                    firstStatusRelease.Set();

                    Guid stopIdentity;
                    using (var motion = MotionUncertaintyJournal.Open(
                        motionJournalDirectory))
                    {
                        AssertEx.True(motion.HasActiveRecord);
                    }
                    using (var axis = AxisCommandRecoveryJournal.Open(
                        axisJournalDirectory))
                    {
                        AssertEx.True(axis.HasActiveRecord);
                        AssertEx.Equal(
                            AxisCommandRecoveryOperation.Stop,
                            axis.CurrentRecord.Operation);
                        AssertEx.Equal(
                            AxisCommandRecoveryState.AcceptedAwaitingProof,
                            axis.CurrentRecord.State);
                        stopIdentity = axis.CurrentRecord.Identity;
                    }

                    boundaryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        MotionAxisStopAcceptedRecoveryScenario);
                    boundaryChild.WaitUntilReady();
                    boundaryChild.TerminateAndVerifyForced();
                    boundaryChild.Dispose();
                    boundaryChild = null;

                    using (var motion = MotionUncertaintyJournal.Open(
                        motionJournalDirectory))
                    {
                        AssertEx.False(motion.HasActiveRecord);
                        AssertEx.Equal(
                            MotionUncertaintyState.Resolved,
                            motion.CurrentRecord.State);
                    }
                    using (var axis = AxisCommandRecoveryJournal.Open(
                        axisJournalDirectory))
                    {
                        AssertEx.True(axis.HasActiveRecord);
                        AssertEx.Equal(stopIdentity, axis.CurrentRecord.Identity);
                        AssertEx.Equal(
                            AxisCommandRecoveryState.AcceptedAwaitingProof,
                            axis.CurrentRecord.State);
                    }

                    finalChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        MotionAxisStopFinalRecoveryScenario);
                    finalChild.WaitUntilReady();
                    finalChild.WaitForSuccessfulExit();
                    finalChild.Dispose();
                    finalChild = null;
                    server.Verify();

                    var received = server.ReceivedRequests
                        .Select((request, index) => new
                        {
                            Request = request,
                            Session = server
                                .ReceivedRequestSessionOrdinals[index]
                        })
                        .ToArray();
                    AssertEx.Equal(
                        1,
                        received.Count(item =>
                            TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2022));
                    AssertEx.Equal(
                        0,
                        received.Count(item =>
                            item.Session > 1
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2022));
                    AssertEx.Equal(
                        3,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2028));
                    AssertEx.Equal(
                        3,
                        received.Count(item =>
                            item.Session == 3
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2028));
                    using (var axis = AxisCommandRecoveryJournal.Open(
                        axisJournalDirectory))
                    {
                        AssertEx.False(axis.HasActiveRecord);
                        AssertEx.Equal(stopIdentity, axis.CurrentRecord.Identity);
                        AssertEx.Equal(
                            AxisCommandRecoveryState.Resolved,
                            axis.CurrentRecord.State);
                    }
                }
            }
            finally
            {
                firstStatusRelease.Set();
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }
                if (boundaryChild != null)
                {
                    boundaryChild.Dispose();
                }
                if (finalChild != null)
                {
                    finalChild.Dispose();
                }
                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            GroupPowerAcceptedProcessTerminationRestartIsStatusOnly()
        {
            RunGroupPowerAcceptedProcessTerminationRestartIsStatusOnly(true);
            RunGroupPowerAcceptedProcessTerminationRestartIsStatusOnly(false);
        }

        private static void
            RunGroupPowerAcceptedProcessTerminationRestartIsStatusOnly(
                bool expectedPowerOn)
        {
            var directoryPath = CreateTestDirectoryPath();
            var groupJournalDirectory = Path.Combine(
                directoryPath,
                "GroupPowerRecovery");
            var dispatchScenario = expectedPowerOn
                ? GroupPowerOnAcceptedDispatchScenario
                : GroupPowerOffAcceptedDispatchScenario;
            var recoveryScenario = expectedPowerOn
                ? GroupPowerOnAcceptedRecoveryScenario
                : GroupPowerOffAcceptedRecoveryScenario;
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess recoveryChild = null;
            using (var firstStatusRelease = new ManualResetEventSlim(false))
            using (var firstStatusEntered = new ManualResetEventSlim(false))
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateGroupPowerProcessRpcSteps(
                            expectedPowerOn,
                            firstStatusRelease,
                            firstStatusEntered)))
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        dispatchScenario);
                    dispatchChild.WaitUntilReady();
                    AssertEx.True(
                        firstStatusEntered.Wait(WaitTimeoutMilliseconds),
                        "The first Group Power status request was not held by the fake server.");
                    dispatchChild.RequestObservationBarrier();
                    AssertEx.False(dispatchChild.Process.HasExited);
                    AssertEx.Throws<IOException>(
                        () => GroupPowerRecoveryJournal.Open(
                            groupJournalDirectory),
                        "The live Group Power child must retain the journal single-writer lock.");

                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;
                    firstStatusRelease.Set();

                    Guid identity;
                    using (var journal = GroupPowerRecoveryJournal.Open(
                        groupJournalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            GroupPowerRecoveryState.AcceptedAwaitingProof,
                            journal.CurrentRecord.State);
                        AssertEx.Equal(
                            expectedPowerOn,
                            journal.CurrentRecord.ExpectedPowerOn);
                        identity = journal.CurrentRecord.Identity;
                    }

                    recoveryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        recoveryScenario);
                    recoveryChild.WaitUntilReady();
                    recoveryChild.WaitForSuccessfulExit();
                    recoveryChild.Dispose();
                    recoveryChild = null;
                    server.Verify();

                    var command = expectedPowerOn
                        ? (ushort)0x204A
                        : (ushort)0x204B;
                    AssertEx.Equal(
                        1,
                        server.ReceivedRequests.Count(request =>
                            TestFrame.ReadUInt16(request, 0) == command));
                    AssertEx.Equal(
                        0,
                        server.ReceivedRequests
                            .Select((request, index) => new
                            {
                                Request = request,
                                Session = server
                                    .ReceivedRequestSessionOrdinals[index]
                            })
                            .Count(item =>
                                item.Session == 2
                                && TestFrame.ReadUInt16(
                                    item.Request,
                                    0) == command));
                    AssertEx.Equal(
                        3,
                        server.ReceivedRequests
                            .Select((request, index) => new
                            {
                                Request = request,
                                Session = server
                                    .ReceivedRequestSessionOrdinals[index]
                            })
                            .Count(item =>
                                item.Session == 2
                                && TestFrame.ReadUInt16(
                                    item.Request,
                                    0) == 0x2045));

                    using (var journal = GroupPowerRecoveryJournal.Open(
                        groupJournalDirectory))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                        AssertEx.Equal(
                            GroupPowerRecoveryState.Resolved,
                            journal.CurrentRecord.State);
                    }
                }
            }
            finally
            {
                firstStatusRelease.Set();
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }

                if (recoveryChild != null)
                {
                    recoveryChild.Dispose();
                }

                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            GroupEnableAcceptedProcessTerminationRestartIsStatusOnly()
        {
            var directoryPath = CreateTestDirectoryPath();
            var groupJournalDirectory = Path.Combine(
                directoryPath,
                "GroupProfileLockRecovery");
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess recoveryChild = null;
            using (var firstStatusRelease = new ManualResetEventSlim(false))
            using (var firstStatusEntered = new ManualResetEventSlim(false))
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateGroupEnableAcceptedProcessRpcSteps(
                            firstStatusRelease,
                            firstStatusEntered)))
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        GroupEnableAcceptedDispatchScenario);
                    dispatchChild.WaitUntilReady();
                    AssertEx.True(
                        firstStatusEntered.Wait(WaitTimeoutMilliseconds),
                        "The first Group Enable status request was not held by the fake server.");
                    dispatchChild.RequestObservationBarrier();
                    AssertEx.False(
                        dispatchChild.Process.HasExited,
                        "The accepted Group Enable child exited before forced termination.");
                    AssertEx.Throws<IOException>(
                        () => GroupProfileLockRecoveryJournal.Open(
                            groupJournalDirectory),
                        "The live Group Enable child must retain the journal single-writer lock.");

                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;
                    firstStatusRelease.Set();

                    Guid identity;
                    using (var journal =
                        GroupProfileLockRecoveryJournal.Open(
                            groupJournalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            GroupProfileLockRecoveryState
                                .AcceptedAwaitingProof,
                            journal.CurrentRecord.State);
                        AssertGroupProfileLockRecoveryIdentity(
                            journal.CurrentRecord,
                            server.Port);
                        identity = journal.CurrentRecord.Identity;
                    }

                    recoveryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        GroupEnableAcceptedRecoveryScenario);
                    recoveryChild.WaitUntilReady();
                    recoveryChild.WaitForSuccessfulExit();
                    recoveryChild.Dispose();
                    recoveryChild = null;
                    server.Verify();

                    var received = server.ReceivedRequests
                        .Select((request, index) => new
                        {
                            Request = request,
                            Session = server
                                .ReceivedRequestSessionOrdinals[index]
                        })
                        .ToArray();
                    AssertEx.Equal(
                        1,
                        received.Count(item =>
                            TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2047),
                        "The two-process test did not observe exactly one Group Enable request.");
                    AssertEx.Equal(
                        1,
                        received.Count(item =>
                            item.Session == 1
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2047),
                        "The Group Enable request was not confined to the killed first session.");
                    AssertEx.Equal(
                        0,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2047),
                        "The restarted process replayed Group Enable.");
                    AssertEx.Equal(
                        3,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2045),
                        "The restarted process did not use exactly three status-only samples.");

                    using (var journal =
                        GroupProfileLockRecoveryJournal.Open(
                            groupJournalDirectory))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                        AssertEx.Equal(
                            GroupProfileLockRecoveryState.Resolved,
                            journal.CurrentRecord.State);
                        AssertGroupProfileLockRecoveryIdentity(
                            journal.CurrentRecord,
                            server.Port);
                    }
                }
            }
            finally
            {
                firstStatusRelease.Set();
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }

                if (recoveryChild != null)
                {
                    recoveryChild.Dispose();
                }

                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            GroupDisableAcceptedProcessTerminationRestartIsStatusOnly()
        {
            var directoryPath = CreateTestDirectoryPath();
            var groupJournalDirectory = Path.Combine(
                directoryPath,
                "GroupProfileLockRecovery");
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess recoveryChild = null;
            using (var firstStatusRelease = new ManualResetEventSlim(false))
            using (var firstStatusEntered = new ManualResetEventSlim(false))
            try
            {
                using (var server = new FakeRpcServer(
                    WpfMainWindowIntegrationTests
                        .CreateGroupDisableAcceptedProcessRpcSteps(
                            firstStatusRelease,
                            firstStatusEntered)))
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        GroupDisableAcceptedDispatchScenario);
                    dispatchChild.WaitUntilReady();
                    AssertEx.True(
                        firstStatusEntered.Wait(WaitTimeoutMilliseconds),
                        "The first Group Disable status request was not held by the fake server.");
                    dispatchChild.RequestObservationBarrier();
                    AssertEx.False(
                        dispatchChild.Process.HasExited,
                        "The accepted Group Disable child exited before forced termination.");
                    AssertEx.Throws<IOException>(
                        () => GroupProfileLockRecoveryJournal.Open(
                            groupJournalDirectory),
                        "The live Group Disable child must retain the journal single-writer lock.");

                    dispatchChild.TerminateAndVerifyForced();
                    dispatchChild.Dispose();
                    dispatchChild = null;
                    firstStatusRelease.Set();

                    Guid identity;
                    using (var journal =
                        GroupProfileLockRecoveryJournal.Open(
                            groupJournalDirectory))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.False(
                            journal.CurrentRecord.ExpectedProfileLocked);
                        AssertEx.Equal(
                            GroupProfileLockRecoveryState
                                .AcceptedAwaitingProof,
                            journal.CurrentRecord.State);
                        AssertGroupProfileLockRecoveryIdentity(
                            journal.CurrentRecord,
                            server.Port);
                        identity = journal.CurrentRecord.Identity;
                    }

                    recoveryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        GroupDisableAcceptedRecoveryScenario);
                    recoveryChild.WaitUntilReady();
                    recoveryChild.WaitForSuccessfulExit();
                    recoveryChild.Dispose();
                    recoveryChild = null;
                    server.Verify();

                    var received = server.ReceivedRequests
                        .Select((request, index) => new
                        {
                            Request = request,
                            Session = server
                                .ReceivedRequestSessionOrdinals[index]
                        })
                        .ToArray();
                    AssertEx.Equal(
                        1,
                        received.Count(item =>
                            TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2048),
                        "The two-process test did not observe exactly one Group Disable request.");
                    AssertEx.Equal(
                        1,
                        received.Count(item =>
                            item.Session == 1
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2048),
                        "The Group Disable request was not confined to the killed first session.");
                    AssertEx.Equal(
                        0,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2048),
                        "The restarted process replayed Group Disable.");
                    AssertEx.Equal(
                        3,
                        received.Count(item =>
                            item.Session == 2
                            && TestFrame.ReadUInt16(item.Request, 0)
                                == 0x2045),
                        "The restarted process did not use exactly three status-only samples.");

                    using (var journal =
                        GroupProfileLockRecoveryJournal.Open(
                            groupJournalDirectory))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);
                        AssertEx.False(
                            journal.CurrentRecord.ExpectedProfileLocked);
                        AssertEx.Equal(
                            GroupProfileLockRecoveryState.Resolved,
                            journal.CurrentRecord.State);
                        AssertGroupProfileLockRecoveryIdentity(
                            journal.CurrentRecord,
                            server.Port);
                    }
                }
            }
            finally
            {
                firstStatusRelease.Set();
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }

                if (recoveryChild != null)
                {
                    recoveryChild.Dispose();
                }

                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            MotionProcessTerminationPreservesJournalNoReplayAndStopResolves()
        {
            var directoryPath = CreateTestDirectoryPath();
            var motionDirectoryPath = Path.Combine(
                directoryPath,
                "MotionUncertaintyRecovery");
            RecoveryChildProcess dispatchChild = null;
            RecoveryChildProcess recoveryChild = null;
            try
            {
                using (var server = new MotionRecoveryRpcServer())
                {
                    dispatchChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        MotionDispatchScenario);
                    dispatchChild.WaitUntilReady();
                    server.WaitForMoveRequest();
                    dispatchChild.RequestObservationBarrier();
                    server.AssertFirstSessionStillObserving();
                    AssertEx.False(
                        dispatchChild.Process.HasExited,
                        "The Move-dispatch WPF child exited before forced termination.");
                    AssertEx.Throws<IOException>(
                        () =>
                        {
                            using (MotionUncertaintyJournal.Open(
                                motionDirectoryPath))
                            {
                            }
                        },
                        "The live Move-dispatch child must retain the motion journal single-writer lock.");

                    server.MarkFirstTerminationExpected();
                    dispatchChild.TerminateAndVerifyForced();
                    server.WaitForFirstSessionStop();
                    dispatchChild.Dispose();
                    dispatchChild = null;

                    AssertMotionDispatchWire(
                        server.SnapshotDispatchCommands());

                    Guid identity;
                    using (var journal = MotionUncertaintyJournal.Open(
                        motionDirectoryPath))
                    {
                        AssertEx.True(
                            journal.HasActiveRecord,
                            "Forced process termination lost the active motion uncertainty record.");
                        var record = journal.CurrentRecord;
                        AssertEx.NotNull(record);
                        identity = record.Identity;
                        AssertEx.Equal(
                            MotionUncertaintyState.ArmedBeforeDispatch,
                            record.State,
                            "The killed process changed the pre-dispatch arm without a Move response.");
                        AssertMotionRecoveryIdentity(record, server.Port);
                    }

                    recoveryChild = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        MotionRecoveryScenario);
                    recoveryChild.WaitUntilReady();
                    recoveryChild.WaitForSuccessfulExit();
                    server.WaitForRecoveryProof();
                    server.WaitForCompletion();
                    recoveryChild.Dispose();
                    recoveryChild = null;

                    AssertMotionRecoveryWire(
                        server.SnapshotRecoveryCommands());
                    using (var journal = MotionUncertaintyJournal.Open(
                        motionDirectoryPath))
                    {
                        AssertEx.False(
                            journal.HasActiveRecord,
                            "Verified Stop did not clear the active motion recovery record.");
                        var record = journal.CurrentRecord;
                        AssertEx.NotNull(record);
                        AssertEx.Equal(identity, record.Identity);
                        AssertEx.Equal(
                            MotionUncertaintyState.Resolved,
                            record.State,
                            "Verified Stop did not persist the Resolved tombstone.");
                        AssertMotionRecoveryIdentity(record, server.Port);
                    }
                }
            }
            finally
            {
                if (dispatchChild != null)
                {
                    dispatchChild.Dispose();
                }

                if (recoveryChild != null)
                {
                    recoveryChild.Dispose();
                }

                DeleteTestDirectory(directoryPath);
            }
        }

        private static void
            DoubleProcessTerminationPreservesJournalAndNoReplay()
        {
            var directoryPath = CreateTestDirectoryPath();
            var doubleDirectoryPath = Path.Combine(
                directoryPath,
                "RecorderDoubleRecovery");
            var identity = Guid.NewGuid();
            var createdUtc = new DateTime(
                638892000020000000L,
                DateTimeKind.Utc);
            var journalPath = Path.Combine(
                doubleDirectoryPath,
                RecorderDoubleRecoveryJournal.JournalFileName);
            try
            {
                using (var journal = RecorderDoubleRecoveryJournal.Open(
                    doubleDirectoryPath))
                {
                    journal.ArmBeforeConfigureDispatch(
                        identity,
                        createdUtc,
                        BootId,
                        RecorderDoubleMapRevision,
                        RecorderDoubleConfigId);
                }

                var originalBytes = File.ReadAllBytes(journalPath);
                AssertEx.True(
                    originalBytes.Length != 0,
                    "The parent did not persist the unresolved Double-bank recovery record.");

                RunAndTerminateRecorderDoubleChild(
                    directoryPath,
                    doubleDirectoryPath,
                    originalBytes,
                    true);
                AssertRecoveredRecorderDoubleRecord(
                    doubleDirectoryPath,
                    identity,
                    createdUtc);

                RunAndTerminateRecorderDoubleChild(
                    directoryPath,
                    doubleDirectoryPath,
                    originalBytes,
                    false);
                AssertRecoveredRecorderDoubleRecord(
                    doubleDirectoryPath,
                    identity,
                    createdUtc);
            }
            finally
            {
                DeleteTestDirectory(directoryPath);
            }
        }

        private static void RunAndTerminateRecorderDoubleChild(
            string directoryPath,
            string doubleDirectoryPath,
            byte[] expectedJournalBytes,
            bool verifySecondWriter)
        {
            RecoveryChildProcess child = null;
            using (var server = new RecoveryRpcObserverServer())
            {
                try
                {
                    child = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        RecorderDoubleScenario);
                    child.WaitUntilReady();
                    child.RequestObservationBarrier();
                    server.WaitForConnectSequence();
                    server.AssertStillObserving();
                    AssertConnectOnlyWire(server.SnapshotCommands());

                    AssertEx.False(
                        child.Process.HasExited,
                        "The Double-bank recovery child exited before forced termination.");
                    if (verifySecondWriter)
                    {
                        AssertEx.Throws<IOException>(
                            () =>
                            {
                                using (RecorderDoubleRecoveryJournal.Open(
                                    doubleDirectoryPath))
                                {
                                }
                            },
                            "The live WPF child must retain the Double-bank journal single-writer lock.");
                    }

                    server.MarkTerminationExpected();
                    child.TerminateAndVerifyForced();
                    server.WaitForStop();
                    AssertConnectOnlyWire(server.SnapshotCommands());
                    AssertEx.SequenceEqual(
                        expectedJournalBytes,
                        File.ReadAllBytes(
                            Path.Combine(
                                doubleDirectoryPath,
                                RecorderDoubleRecoveryJournal
                                    .JournalFileName)),
                        "Opening, connecting, forcing the dormant recovery entrypoint, and terminating the WPF child changed the Double-bank journal bytes.");
                }
                finally
                {
                    if (child != null)
                    {
                        child.Dispose();
                    }
                }
            }
        }

        private static void AssertRecoveredRecorderDoubleRecord(
            string doubleDirectoryPath,
            Guid identity,
            DateTime createdUtc)
        {
            using (var journal = RecorderDoubleRecoveryJournal.Open(
                doubleDirectoryPath))
            {
                AssertEx.True(journal.HasActiveRecord);
                var record = journal.CurrentRecord;
                AssertEx.NotNull(record);
                AssertEx.Equal(identity, record.Identity);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState
                        .ArmedBeforeConfigureDispatch,
                    record.State);
                AssertEx.Equal(createdUtc, record.CreatedUtc);
                AssertEx.Equal(createdUtc, record.UpdatedUtc);
                AssertEx.Equal(BootId, record.DiagnosticsBootId);
                AssertEx.Equal(
                    RecorderDoubleMapRevision,
                    record.MapRevision);
                AssertEx.Equal(
                    RecorderDoubleConfigId,
                    record.RequestedConfigId);
                AssertEx.Equal((uint)0, record.ConfigRevision);
                AssertEx.Equal(0, record.Banks.Count);
                AssertEx.Equal(
                    RecorderDoubleRecoveryTokenMarker.ClientTokenV1,
                    record.RecoveryTokenMarker);
                AssertEx.Equal(identity, record.RecoveryToken);
            }
        }

        private static void RunScenario(RecoveryScenario scenario)
        {
            var directoryPath = CreateTestDirectoryPath();
            var identity = Guid.NewGuid();
            var createdUtc = scenario.CreatedUtc;
            var journalPath = Path.Combine(
                directoryPath,
                DiagnosticsMutationJournal.JournalFileName);
            try
            {
                using (var journal =
                    DiagnosticsMutationJournal.Open(directoryPath))
                {
                    journal.Arm(
                        scenario.Kind,
                        identity,
                        createdUtc,
                        BootId,
                        scenario.Revision,
                        SessionGeneration,
                        scenario.Target,
                        scenario.Expected);
                    journal.Transition(
                        identity,
                        DiagnosticsMutationState.OutcomeUnverified,
                        scenario.UpdatedUtc,
                        0);
                }

                var originalBytes = File.ReadAllBytes(journalPath);
                AssertEx.True(
                    originalBytes.Length != 0,
                    "The parent did not persist the unresolved mutation record.");

                RunAndTerminateChild(
                    directoryPath,
                    scenario,
                    originalBytes,
                    true);
                AssertRecoveredRecord(
                    directoryPath,
                    scenario,
                    identity,
                    createdUtc);

                RunAndTerminateChild(
                    directoryPath,
                    scenario,
                    originalBytes,
                    false);
                AssertRecoveredRecord(
                    directoryPath,
                    scenario,
                    identity,
                    createdUtc);
            }
            finally
            {
                DeleteTestDirectory(directoryPath);
            }
        }

        private static void RunAndTerminateChild(
            string directoryPath,
            RecoveryScenario scenario,
            byte[] expectedJournalBytes,
            bool verifySecondWriter)
        {
            RecoveryChildProcess child = null;
            using (var server = new RecoveryRpcObserverServer())
            {
                try
                {
                    child = RecoveryChildProcess.Start(
                        directoryPath,
                        server.Port,
                        scenario.Kind);
                    child.WaitUntilReady();
                    child.RequestObservationBarrier();
                    server.WaitForConnectSequence();
                    server.AssertStillObserving();
                    AssertConnectOnlyWire(server.SnapshotCommands());

                    AssertEx.False(
                        child.Process.HasExited,
                        "The WPF recovery child exited before forced termination.");
                    if (verifySecondWriter)
                    {
                        AssertEx.Throws<IOException>(
                            () =>
                            {
                                using (DiagnosticsMutationJournal.Open(
                                    directoryPath))
                                {
                                }
                            },
                            "The live WPF child must retain the journal single-writer lock.");
                    }

                    server.MarkTerminationExpected();
                    child.TerminateAndVerifyForced();
                    server.WaitForStop();
                    AssertConnectOnlyWire(server.SnapshotCommands());
                    AssertEx.SequenceEqual(
                        expectedJournalBytes,
                        File.ReadAllBytes(
                            Path.Combine(
                                directoryPath,
                                DiagnosticsMutationJournal.JournalFileName)),
                        "Opening, connecting, and forcibly terminating the WPF recovery child changed the journal bytes.");
                }
                finally
                {
                    if (child != null)
                    {
                        child.Dispose();
                    }
                }
            }
        }

        private static void AssertRecoveredRecord(
            string directoryPath,
            RecoveryScenario scenario,
            Guid identity,
            DateTime createdUtc)
        {
            using (var journal =
                DiagnosticsMutationJournal.Open(directoryPath))
            {
                AssertEx.True(journal.HasActiveRecord);
                var record = journal.CurrentRecord;
                AssertEx.NotNull(record);
                AssertEx.Equal(identity, record.Identity);
                AssertEx.Equal(scenario.Kind, record.Kind);
                AssertEx.Equal(
                    DiagnosticsMutationState.OutcomeUnverified,
                    record.State);
                AssertEx.Equal(createdUtc, record.CreatedUtc);
                AssertEx.Equal(scenario.UpdatedUtc, record.UpdatedUtc);
                AssertEx.Equal(BootId, record.DiagnosticsBootId);
                AssertEx.Equal(scenario.Revision, record.IdentityRevision);
                AssertEx.Equal(SessionGeneration, record.SessionGeneration);
                AssertEx.Equal((uint)0, record.TicketId);
                AssertEx.Equal(scenario.Target, record.TargetText);
                AssertEx.Equal(scenario.Expected, record.ExpectedText);
            }
        }

        private static void RunChildCore(
            string directoryPath,
            int rpcPort,
            RecoveryScenario scenario)
        {
            var window = new MainWindow(directoryPath)
            {
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000
            };
            window.TextRemoteIp.Text = "127.0.0.1";
            window.TextRemotePort.Text = rpcPort.ToString(
                CultureInfo.InvariantCulture);
            window.TextLocalIp.Text = "127.0.0.1";
            window.TextCallbackPort.Text = "0";
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The mutation-recovery WPF window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextConnectionState.Text,
                        "Connected",
                        StringComparison.Ordinal)
                    && window.ButtonLoadEtherCATTopology.IsEnabled
                    && window.TextEtherCATTopologySummary.Text.IndexOf(
                        "LOAD FAILED (automatic post-connect load)",
                        StringComparison.Ordinal) >= 0,
                "The mutation-recovery WPF child did not reach the connected idle state after capability-off auto-load.");

            var status = window.TextPersistedMutationStatus.Text;
            AssertEx.Contains("RECOVERED UNRESOLVED MUTATION.", status);
            AssertEx.Contains("Automatic replay is disabled.", status);
            AssertEx.Contains("Kind=" + scenario.Kind, status);
            AssertEx.Contains("State=OutcomeUnverified", status);
            AssertEx.Contains("Ticket=UNKNOWN", status);
            AssertEx.Contains("BootId=0x12345678", status);
            AssertEx.Contains(
                "Revision=0x" + scenario.Revision.ToString("X8"),
                status);
            AssertEx.Contains("Target=" + scenario.Target, status);
            AssertEx.Contains("Expected=" + scenario.Expected, status);

            AssertEx.False(window.ButtonSubmitSdo.IsEnabled);
            AssertEx.False(window.ButtonSubmitDigitalOutputWrite.IsEnabled);
            AssertEx.False(window.ButtonCloseConnection.IsEnabled);
            AssertEx.True(
                window.CheckPersistedMutationPhysicallyVerified.IsEnabled);
            AssertEx.False(
                window.ButtonAcknowledgePersistedMutation.IsEnabled);
            AssertEx.False(
                window.CheckPersistedMutationPhysicallyVerified.IsChecked
                    == true);

            window.Close();
            PumpDispatcherOnce();
            AssertEx.True(
                window.IsLoaded,
                "The recovered unresolved mutation did not block window close.");
            AssertEx.Contains(
                "Window close is blocked while diagnostics mutation or durable recovery evidence is unresolved.",
                window.TextExecutionLog.Text);

            RunChildObservationLoop(scenario.Kind.ToString());
        }

        private static void RunRecorderDoubleChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = new MainWindow(directoryPath)
            {
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000
            };
            window.TextRemoteIp.Text = "127.0.0.1";
            window.TextRemotePort.Text = rpcPort.ToString(
                CultureInfo.InvariantCulture);
            window.TextLocalIp.Text = "127.0.0.1";
            window.TextCallbackPort.Text = "0";
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Double-bank recovery WPF window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextConnectionState.Text,
                        "Connected",
                        StringComparison.Ordinal)
                    && window.ButtonLoadEtherCATTopology.IsEnabled
                    && window.TextEtherCATTopologySummary.Text.IndexOf(
                        "LOAD FAILED (automatic post-connect load)",
                        StringComparison.Ordinal) >= 0,
                "The Double-bank recovery WPF child did not reach the connected idle state after capability-off auto-load.");

            var status = window.TextRecorderDoubleRecoveryStatus.Text;
            AssertEx.Contains(
                "RECOVERED UNRESOLVED DOUBLE RECORD.",
                status);
            AssertEx.Contains("Automatic replay is disabled.", status);
            AssertEx.Contains(
                "State=ArmedBeforeConfigureDispatch",
                status);
            AssertEx.Contains("BootId=0x12345678", status);
            AssertEx.Contains(
                "MapRevision=0x"
                    + RecorderDoubleMapRevision.ToString("X8"),
                status);
            AssertEx.Contains(
                "ConfigId=0x"
                    + RecorderDoubleConfigId.ToString("X8"),
                status);
            AssertEx.Contains("ConfigRevision=0x00000000", status);
            AssertEx.Contains("Banks=none", status);
            AssertEx.Contains("TokenMarker=ClientTokenV1", status);

            AssertEx.False(window.ButtonSubmitSdo.IsEnabled);
            AssertEx.False(
                window.ButtonSubmitDigitalOutputWrite.IsEnabled);
            AssertEx.False(
                window.ButtonRunRecorderDoubleQualification.IsEnabled);
            AssertEx.False(
                window.ButtonRecoverRecorderDoubleJournal.IsEnabled);
            AssertEx.False(window.ButtonCloseConnection.IsEnabled);

            Click(window.ButtonRecoverRecorderDoubleJournal);
            AssertEx.Equal(
                "RecorderDoubleRecovery failed",
                window.TextOperationState.Text);
            AssertEx.Contains(
                "ReconnectRecovery proof gate is CLOSED",
                window.TextExecutionLog.Text);

            window.Close();
            PumpDispatcherOnce();
            AssertEx.True(
                window.IsLoaded,
                "The recovered unresolved Double-bank record did not block window close.");
            AssertEx.Contains(
                "Window close is blocked while diagnostics mutation or durable recovery evidence is unresolved.",
                window.TextExecutionLog.Text);

            RunChildObservationLoop(RecorderDoubleScenario);
        }

        private static void RunMotionDispatchChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The motion-dispatch WPF window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The motion-dispatch WPF child did not connect.");

            window.TextAxisName.Text = MotionAxisName;
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonMoveAbsolute.IsEnabled,
                "The motion-dispatch WPF child did not load the test axis.");

            Click(window.ButtonMoveAbsolute);
            WaitUntil(
                () =>
                {
                    var journal = GetMotionUncertaintyJournal(window);
                    return (bool)GetPrivateField(
                            window,
                            "motionMayBeActive")
                        && journal.HasActiveRecord
                        && journal.CurrentRecord.State
                            == MotionUncertaintyState.ArmedBeforeDispatch;
                },
                "The Move was not durably armed before dispatch.");

            RunChildObservationLoop(MotionDispatchScenario);
        }

        private static void RunAxisPowerAcceptedDispatchChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Axis Power On dispatch child window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The Axis Power On dispatch child did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonPowerOn.IsEnabled,
                "The Axis Power On dispatch child did not load the axis.");

            Click(window.ButtonPowerOn);
            var journal = GetAxisPowerOnRecoveryJournal(window);
            WaitUntil(
                () => journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == AxisPowerOnRecoveryState.AcceptedAwaitingProof
                    && (bool)GetPrivateField(window, "operationRunning"),
                "The exact accepted Power On ACK was not durably recorded before the held first status response.");
            AssertEx.Equal(MotionAxisName, journal.CurrentRecord.AxisName);
            AssertEx.Equal(MotionAxisReference, journal.CurrentRecord.AxisReference);
            AssertEx.False(window.ButtonPowerOn.IsEnabled);

            RunChildObservationLoop(AxisPowerAcceptedDispatchScenario);
        }

        private static void RunAxisCommandAcceptedDispatchChildCore(
            string directoryPath,
            int rpcPort,
            bool reset)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Axis command dispatch child window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The Axis command dispatch child did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && (reset
                        ? window.ButtonReset.IsEnabled
                        : window.ButtonStop.IsEnabled),
                "The Axis command dispatch child did not load the axis.");

            Click(reset ? window.ButtonReset : window.ButtonStop);
            var journal = GetAxisCommandRecoveryJournal(window);
            WaitUntil(
                () => journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == AxisCommandRecoveryState.AcceptedAwaitingProof
                    && journal.CurrentRecord.Operation
                        == (reset
                            ? AxisCommandRecoveryOperation.Reset
                            : AxisCommandRecoveryOperation.Stop)
                    && (reset
                        ? (bool)GetPrivateField(window, "operationRunning")
                        : (int)GetPrivateField(window, "safetyMonitorCount") > 0),
                "The accepted Axis Stop/Reset ACK was not durably recorded before the held first status response.");
            RunChildObservationLoop(
                reset
                    ? AxisResetAcceptedDispatchScenario
                    : AxisStopAcceptedDispatchScenario);
        }

        private static void RunAxisCommandArmedDispatchChildCore(
            string directoryPath,
            int rpcPort,
            bool reset)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The ACK-boundary Axis command child window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The ACK-boundary Axis command child did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && (reset
                        ? window.ButtonReset.IsEnabled
                        : window.ButtonStop.IsEnabled),
                "The ACK-boundary Axis command child did not load the axis.");

            var journal = GetAxisCommandRecoveryJournal(window);
            window.AxisCommandAcceptedBeforeDurableMarkTestHook = record =>
            {
                AssertEx.True(journal.HasActiveRecord);
                AssertEx.Equal(record.Identity, journal.CurrentRecord.Identity);
                AssertEx.Equal(
                    AxisCommandRecoveryState.ArmedBeforeDispatch,
                    journal.CurrentRecord.State);
                AssertEx.Equal(
                    reset
                        ? AxisCommandRecoveryOperation.Reset
                        : AxisCommandRecoveryOperation.Stop,
                    journal.CurrentRecord.Operation);
                RunChildObservationLoop(
                    reset
                        ? AxisResetArmedDispatchScenario
                        : AxisStopArmedDispatchScenario);
            };
            Click(reset ? window.ButtonReset : window.ButtonStop);
            Dispatcher.Run();
            throw new InvalidOperationException(
                "The ACK-boundary Axis command hook returned before process termination.");
        }

        private static void
            RunAxisCommandArmedRestartInspectionChildCore(
                string directoryPath,
                int rpcPort,
                bool reset)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetAxisCommandRecoveryJournal(window);
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.Equal(
                reset
                    ? AxisCommandRecoveryOperation.Reset
                    : AxisCommandRecoveryOperation.Stop,
                journal.CurrentRecord.Operation);
            AssertEx.Equal(
                AxisCommandRecoveryState.RecoveryRequired,
                journal.CurrentRecord.State);
            Console.WriteLine(
                ReadyPrefix
                + (reset
                    ? AxisResetArmedRestartScenario
                    : AxisStopArmedRestartScenario));
            Console.Out.Flush();
        }

        private static void RunAxisCommandAcceptedRecoveryChildCore(
            string directoryPath,
            int rpcPort,
            bool reset)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetAxisCommandRecoveryJournal(window);
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.Equal(
                AxisCommandRecoveryState.AcceptedAwaitingProof,
                journal.CurrentRecord.State);
            AssertEx.Equal(
                reset
                    ? AxisCommandRecoveryOperation.Reset
                    : AxisCommandRecoveryOperation.Stop,
                journal.CurrentRecord.Operation);

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Axis command recovery child window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The Axis command recovery child did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && (reset
                        ? window.ButtonReset.IsEnabled
                        : window.ButtonStop.IsEnabled),
                "The Axis command recovery child did not load the axis.");

            Click(reset ? window.ButtonReset : window.ButtonStop);
            WaitUntil(
                () => !journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == AxisCommandRecoveryState.Resolved
                    && window.ButtonCloseConnection.IsEnabled,
                "Restart status-only proof did not resolve the Axis command journal.");
            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The resolved Axis command recovery connection did not close.");
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The resolved Axis command recovery child window did not close.");
            Console.WriteLine(
                ReadyPrefix
                + (reset
                    ? AxisResetAcceptedRecoveryScenario
                    : AxisStopAcceptedRecoveryScenario));
            Console.Out.Flush();
        }

        private static void RunMotionAxisStopAcceptedDispatchChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var motionJournal = GetMotionUncertaintyJournal(window);
            AssertEx.True(motionJournal.HasActiveRecord);
            AssertEx.True((bool)GetPrivateField(window, "motionMayBeActive"));
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Motion/Stop dispatch child window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The Motion/Stop dispatch child did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonStop.IsEnabled,
                "The Motion/Stop dispatch child did not load the axis.");
            Click(window.ButtonStop);
            var axisJournal = GetAxisCommandRecoveryJournal(window);
            WaitUntil(
                () => motionJournal.HasActiveRecord
                    && axisJournal.HasActiveRecord
                    && axisJournal.CurrentRecord.Operation
                        == AxisCommandRecoveryOperation.Stop
                    && axisJournal.CurrentRecord.State
                        == AxisCommandRecoveryState.AcceptedAwaitingProof
                    && (int)GetPrivateField(window, "safetyMonitorCount") > 0,
                "The Motion/Stop child did not retain both durable records at the held first status.");
            RunChildObservationLoop(
                MotionAxisStopAcceptedDispatchScenario);
        }

        private static void RunMotionAxisStopAcceptedRecoveryChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var motionJournal = GetMotionUncertaintyJournal(window);
            var axisJournal = GetAxisCommandRecoveryJournal(window);
            AssertEx.True(motionJournal.HasActiveRecord);
            AssertEx.True(axisJournal.HasActiveRecord);
            AssertEx.Equal(
                AxisCommandRecoveryState.AcceptedAwaitingProof,
                axisJournal.CurrentRecord.State);
            window.AxisCommandBeforeDurableResolveTestHook = record =>
            {
                AssertEx.False(
                    motionJournal.HasActiveRecord,
                    "Motion must be durably resolved before the Axis Stop callback boundary.");
                AssertEx.True(axisJournal.HasActiveRecord);
                Console.WriteLine(
                    ReadyPrefix + MotionAxisStopAcceptedRecoveryScenario);
                Console.Out.Flush();
                Thread.Sleep(Timeout.Infinite);
            };

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Motion/Stop recovery-boundary child window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The Motion/Stop recovery-boundary child did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonStop.IsEnabled,
                "The Motion/Stop recovery-boundary child did not load the axis.");
            Click(window.ButtonStop);
            WaitUntil(
                () => false,
                "The Motion/Stop durable ordering hook was not reached.");
        }

        private static void RunAxisPowerAcceptedRecoveryChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetAxisPowerOnRecoveryJournal(window);
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.Equal(
                AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                journal.CurrentRecord.State);
            AssertEx.True((bool)GetPrivateField(
                window,
                "axisPowerOnAcceptedRestartRecovery"));
            AssertEx.False((bool)GetPrivateField(
                window,
                "axisPowerOnRecoveryRequired"));

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The accepted Axis Power On recovery window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The accepted Axis Power On recovery endpoint did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonPowerOn.IsEnabled,
                "The accepted Axis Power On recovery axis did not load.");

            Click(window.ButtonPowerOn);
            WaitUntil(
                () => !journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == AxisPowerOnRecoveryState.Resolved
                    && window.ButtonCloseConnection.IsEnabled,
                "Status-only restart verification did not resolve Axis Power On recovery.");
            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The resolved Axis Power On recovery connection did not close.");
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The resolved Axis Power On recovery window did not close.");

            Console.WriteLine(ReadyPrefix + AxisPowerAcceptedRecoveryScenario);
            Console.Out.Flush();
        }

        private static void RunAxisPowerOffAcceptedDispatchChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Axis Power Off dispatch child window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The Axis Power Off dispatch child did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonPowerOff.IsEnabled,
                "The Axis Power Off dispatch child did not load the axis.");

            Click(window.ButtonPowerOff);
            var journal = GetAxisPowerOnRecoveryJournal(window);
            WaitUntil(
                () => journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == AxisPowerOnRecoveryState.AcceptedAwaitingProof
                    && !journal.CurrentRecord.ExpectedPowerOn,
                "The exact accepted Power Off ACK was not durably recorded before the held first status response.");
            AssertAxisPowerRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort,
                false);
            AssertEx.False(window.ButtonPowerOff.IsEnabled);

            RunChildObservationLoop(AxisPowerOffAcceptedDispatchScenario);
        }

        private static void RunAxisPowerOffAcceptedRecoveryChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetAxisPowerOnRecoveryJournal(window);
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.Equal(
                AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                journal.CurrentRecord.State);
            AssertAxisPowerRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort,
                false);
            AssertEx.True((bool)GetPrivateField(
                window,
                "axisPowerOnAcceptedRestartRecovery"));
            AssertEx.False((bool)GetPrivateField(
                window,
                "axisPowerOnRecoveryRequired"));

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The accepted Axis Power Off recovery window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The accepted Axis Power Off recovery endpoint did not connect.");
            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonPowerOff.IsEnabled,
                "The accepted Axis Power Off recovery axis did not load.");
            AssertEx.Contains(
                "No 0x2023 Replay",
                Convert.ToString(
                    window.ButtonPowerOff.Content,
                    CultureInfo.InvariantCulture));

            Click(window.ButtonPowerOff);
            WaitUntil(
                () => !journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == AxisPowerOnRecoveryState.Resolved
                    && window.ButtonCloseConnection.IsEnabled,
                "Status-only restart verification did not resolve Axis Power Off recovery.");
            AssertAxisPowerRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort,
                false);
            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The resolved Axis Power Off recovery connection did not close.");
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The resolved Axis Power Off recovery window did not close.");

            Console.WriteLine(
                ReadyPrefix + AxisPowerOffAcceptedRecoveryScenario);
            Console.Out.Flush();
        }

        private static void RunGroupPowerAcceptedDispatchChildCore(
            string directoryPath,
            int rpcPort,
            bool expectedPowerOn)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Group Power dispatch child window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupGroup.IsEnabled,
                "The Group Power dispatch child did not connect.");
            window.TextGroupName.Text = GroupPowerName;
            Click(window.ButtonLookupGroup);
            var powerButton = expectedPowerOn
                ? window.ButtonGroupPowerOn
                : window.ButtonGroupPowerOff;
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Group completed",
                        StringComparison.Ordinal)
                    && powerButton.IsEnabled,
                "The Group Power dispatch child did not load the group.");

            Click(powerButton);
            var journal = GetGroupPowerRecoveryJournal(window);
            WaitUntil(
                () => journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == GroupPowerRecoveryState.AcceptedAwaitingProof,
                "The exact accepted Group Power ACK was not durably recorded before the held first status response.");
            AssertEx.Equal(
                expectedPowerOn,
                journal.CurrentRecord.ExpectedPowerOn);
            AssertEx.Equal(GroupPowerName, journal.CurrentRecord.GroupName);
            AssertEx.Equal(
                GroupPowerReference,
                journal.CurrentRecord.GroupReference);
            AssertEx.False(powerButton.IsEnabled);

            RunChildObservationLoop(
                expectedPowerOn
                    ? GroupPowerOnAcceptedDispatchScenario
                    : GroupPowerOffAcceptedDispatchScenario);
        }

        private static void RunGroupPowerAcceptedRecoveryChildCore(
            string directoryPath,
            int rpcPort,
            bool expectedPowerOn)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetGroupPowerRecoveryJournal(window);
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.Equal(
                GroupPowerRecoveryState.AcceptedAwaitingProof,
                journal.CurrentRecord.State);
            AssertEx.Equal(
                expectedPowerOn,
                journal.CurrentRecord.ExpectedPowerOn);
            AssertEx.True((bool)GetPrivateField(
                window,
                "groupPowerAcceptedRestartRecovery"));
            AssertEx.False((bool)GetPrivateField(
                window,
                "groupPowerRecoveryRequired"));

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The accepted Group Power recovery window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupGroup.IsEnabled,
                "The accepted Group Power recovery endpoint did not connect.");
            Click(window.ButtonLookupGroup);
            var recoveryButton = expectedPowerOn
                ? window.ButtonGroupPowerOn
                : window.ButtonGroupPowerOff;
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Group completed",
                        StringComparison.Ordinal)
                    && recoveryButton.IsEnabled,
                "The accepted Group Power recovery group did not load.");
            AssertEx.Contains(
                expectedPowerOn ? "No 0x204A Replay" : "No 0x204B Replay",
                Convert.ToString(
                    recoveryButton.Content,
                    CultureInfo.InvariantCulture));

            Click(recoveryButton);
            WaitUntil(
                () => !journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == GroupPowerRecoveryState.Resolved
                    && window.ButtonCloseConnection.IsEnabled,
                "Status-only restart verification did not resolve Group Power recovery.");
            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The resolved Group Power recovery connection did not close.");
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The resolved Group Power recovery window did not close.");

            Console.WriteLine(
                ReadyPrefix
                + (expectedPowerOn
                    ? GroupPowerOnAcceptedRecoveryScenario
                    : GroupPowerOffAcceptedRecoveryScenario));
            Console.Out.Flush();
        }

        private static void RunGroupEnableAcceptedDispatchChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Group Enable dispatch child window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupGroup.IsEnabled,
                "The Group Enable dispatch child did not connect.");
            window.TextGroupName.Text = GroupPowerName;
            Click(window.ButtonLookupGroup);
            WaitUntil(
                () => string.Equals(
                    window.TextOperationState.Text,
                    "Load Group completed",
                    StringComparison.Ordinal),
                "The Group Enable dispatch child did not load the group.");

            SetPrivateField(window, "groupActiveVerified", true);
            SetPrivateField(window, "groupIdentityConfigured", true);
            InvokePrivate(window, "UpdateUiState");
            AssertEx.True(
                window.ButtonGroupEnable.IsEnabled,
                "The prepared Group Enable dispatch button was not enabled.");

            Click(window.ButtonGroupEnable);
            var journal = GetGroupProfileLockRecoveryJournal(window);
            WaitUntil(
                () => journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == GroupProfileLockRecoveryState
                            .AcceptedAwaitingProof
                    && (bool)GetPrivateField(window, "operationRunning"),
                "The exact Group Enable ACK was not durably recorded before the held first status response.");
            AssertGroupProfileLockRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort);
            AssertEx.False(window.ButtonGroupEnable.IsEnabled);

            RunChildObservationLoop(GroupEnableAcceptedDispatchScenario);
        }

        private static void RunGroupEnableAcceptedRecoveryChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetGroupProfileLockRecoveryJournal(window);
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.Equal(
                GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                journal.CurrentRecord.State);
            AssertGroupProfileLockRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort);
            AssertEx.True((bool)GetPrivateField(
                window,
                "groupProfileLockAcceptedRestartRecovery"));
            AssertEx.False((bool)GetPrivateField(
                window,
                "groupProfileLockRecoveryRequired"));
            AssertEx.True((bool)GetPrivateField(
                window,
                "groupProfileLockVerificationPending"));
            AssertEx.Equal(GroupPowerName, window.TextGroupName.Text);

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The accepted Group Enable recovery window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupGroup.IsEnabled,
                "The accepted Group Enable recovery endpoint did not connect.");
            Click(window.ButtonLookupGroup);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Group completed",
                        StringComparison.Ordinal)
                    && window.ButtonGroupEnable.IsEnabled,
                "The accepted Group Enable recovery group did not load.");
            AssertEx.Contains(
                "No 0x2047 Replay",
                Convert.ToString(
                    window.ButtonGroupEnable.Content,
                    CultureInfo.InvariantCulture));

            Click(window.ButtonGroupEnable);
            WaitUntil(
                () => !journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == GroupProfileLockRecoveryState.Resolved
                    && (bool)GetPrivateField(window, "groupProfileLocked")
                    && window.ButtonCloseConnection.IsEnabled,
                "Status-only restart verification did not resolve Group Enable recovery.");
            AssertGroupProfileLockRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort);

            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The resolved Group Enable recovery connection did not close.");
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The resolved Group Enable recovery window did not close.");

            Console.WriteLine(
                ReadyPrefix + GroupEnableAcceptedRecoveryScenario);
            Console.Out.Flush();
        }

        private static void RunGroupDisableAcceptedDispatchChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The Group Disable dispatch child window did not load.");

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupGroup.IsEnabled,
                "The Group Disable dispatch child did not connect.");
            window.TextGroupName.Text = GroupPowerName;
            Click(window.ButtonLookupGroup);
            WaitUntil(
                () => string.Equals(
                    window.TextOperationState.Text,
                    "Load Group completed",
                    StringComparison.Ordinal),
                "The Group Disable dispatch child did not load the group.");

            SetPrivateField(window, "groupActiveVerified", true);
            SetPrivateField(window, "groupIdentityConfigured", true);
            SetPrivateField(window, "groupProfileLocked", true);
            InvokePrivate(window, "UpdateUiState");
            AssertEx.True(
                window.ButtonGroupDisable.IsEnabled,
                "The prepared Group Disable dispatch button was not enabled.");

            Click(window.ButtonGroupDisable);
            var journal = GetGroupProfileLockRecoveryJournal(window);
            WaitUntil(
                () => journal.HasActiveRecord
                    && !journal.CurrentRecord.ExpectedProfileLocked
                    && journal.CurrentRecord.State
                        == GroupProfileLockRecoveryState
                            .AcceptedAwaitingProof
                    && (bool)GetPrivateField(window, "operationRunning"),
                "The exact Group Disable ACK was not durably recorded before the held first status response.");
            AssertGroupProfileLockRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort);
            AssertEx.False(window.ButtonGroupDisable.IsEnabled);
            AssertEx.False(window.ButtonGroupMoveLinear.IsEnabled);

            RunChildObservationLoop(GroupDisableAcceptedDispatchScenario);
        }

        private static void RunGroupDisableAcceptedRecoveryChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetGroupProfileLockRecoveryJournal(window);
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.False(journal.CurrentRecord.ExpectedProfileLocked);
            AssertEx.Equal(
                GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                journal.CurrentRecord.State);
            AssertGroupProfileLockRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort);
            AssertEx.True((bool)GetPrivateField(
                window,
                "groupProfileUnlockAcceptedRestartRecovery"));
            AssertEx.False((bool)GetPrivateField(
                window,
                "groupProfileLockRecoveryRequired"));
            AssertEx.True((bool)GetPrivateField(
                window,
                "groupProfileUnlockVerificationPending"));
            AssertEx.False((bool)GetPrivateField(
                window,
                "groupProfileLocked"));
            AssertEx.Equal(GroupPowerName, window.TextGroupName.Text);

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The accepted Group Disable recovery window did not load.");
            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupGroup.IsEnabled,
                "The accepted Group Disable recovery endpoint did not connect.");
            Click(window.ButtonLookupGroup);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Group completed",
                        StringComparison.Ordinal)
                    && window.ButtonGroupDisable.IsEnabled,
                "The accepted Group Disable recovery group did not load.");
            AssertEx.Contains(
                "No 0x2048 Replay",
                Convert.ToString(
                    window.ButtonGroupDisable.Content,
                    CultureInfo.InvariantCulture));

            Click(window.ButtonGroupDisable);
            WaitUntil(
                () => !journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == GroupProfileLockRecoveryState.Resolved
                    && !(bool)GetPrivateField(window, "groupProfileLocked")
                    && window.ButtonCloseConnection.IsEnabled,
                "Status-only restart verification did not resolve Group Disable recovery.");
            AssertEx.False(journal.CurrentRecord.ExpectedProfileLocked);
            AssertGroupProfileLockRecoveryIdentity(
                journal.CurrentRecord,
                rpcPort);

            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The resolved Group Disable recovery connection did not close.");
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The resolved Group Disable recovery window did not close.");

            Console.WriteLine(
                ReadyPrefix + GroupDisableAcceptedRecoveryScenario);
            Console.Out.Flush();
        }

        private static void RunMotionRecoveryChildCore(
            string directoryPath,
            int rpcPort)
        {
            var window = CreateHiddenWindow(directoryPath, rpcPort);
            var journal = GetMotionUncertaintyJournal(window);
            AssertEx.True((bool)GetPrivateField(
                window,
                "motionUncertaintyRecoveredAtStartup"));
            AssertEx.True((bool)GetPrivateField(
                window,
                "motionMayBeActive"));
            AssertEx.True(journal.HasActiveRecord);
            AssertEx.Equal(
                MotionUncertaintyState.RecoveryRequired,
                journal.CurrentRecord.State,
                "Startup did not promote the killed Move arm to RecoveryRequired.");
            AssertMotionRecoveryIdentity(journal.CurrentRecord, rpcPort);
            AssertEx.Equal(MotionAxisName, window.TextAxisName.Text);
            AssertEx.Equal("127.0.0.1", window.TextRemoteIp.Text);
            AssertEx.Equal(
                rpcPort.ToString(CultureInfo.InvariantCulture),
                window.TextRemotePort.Text);

            window.Show();
            WaitUntil(
                () => window.IsLoaded,
                "The motion-recovery WPF window did not load.");
            AssertEx.False(window.ButtonMoveAbsolute.IsEnabled);

            Click(window.ButtonConnect);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Connect completed",
                        StringComparison.Ordinal)
                    && window.ButtonLookupAxis.IsEnabled,
                "The exact motion-recovery endpoint did not connect.");
            AssertEx.False(window.ButtonMoveAbsolute.IsEnabled);

            Click(window.ButtonLookupAxis);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Load Axis completed",
                        StringComparison.Ordinal)
                    && window.ButtonStop.IsEnabled,
                "The exact motion-recovery axis did not load.");
            AssertEx.False(window.ButtonMoveAbsolute.IsEnabled);

            Click(window.ButtonStop);
            WaitUntil(
                () => string.Equals(
                        window.TextOperationState.Text,
                        "Stop verified",
                        StringComparison.Ordinal)
                    && !(bool)GetPrivateField(
                        window,
                        "motionMayBeActive")
                    && !journal.HasActiveRecord
                    && journal.CurrentRecord.State
                        == MotionUncertaintyState.Resolved
                    && window.ButtonCloseConnection.IsEnabled,
                "Stop and three stable status samples did not resolve motion recovery.");

            Click(window.ButtonCloseConnection);
            WaitUntil(
                () => string.Equals(
                    window.TextConnectionState.Text,
                    "Disconnected",
                    StringComparison.Ordinal),
                "The resolved motion-recovery connection did not close.");
            window.Close();
            WaitUntil(
                () => !window.IsLoaded,
                "The resolved motion-recovery WPF window did not close.");

            Console.WriteLine(ReadyPrefix + MotionRecoveryScenario);
            Console.Out.Flush();
        }

        private static MainWindow CreateHiddenWindow(
            string directoryPath,
            int rpcPort)
        {
            var window = new MainWindow(directoryPath)
            {
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000
            };
            window.TextRemoteIp.Text = "127.0.0.1";
            window.TextRemotePort.Text = rpcPort.ToString(
                CultureInfo.InvariantCulture);
            window.TextLocalIp.Text = "127.0.0.1";
            window.TextCallbackPort.Text = "0";
            return window;
        }

        private static MotionUncertaintyJournal
            GetMotionUncertaintyJournal(MainWindow window)
        {
            return (MotionUncertaintyJournal)GetPrivateField(
                window,
                "motionUncertaintyJournal");
        }

        private static AxisPowerOnRecoveryJournal
            GetAxisPowerOnRecoveryJournal(MainWindow window)
        {
            return (AxisPowerOnRecoveryJournal)GetPrivateField(
                window,
                "axisPowerOnRecoveryJournal");
        }

        private static AxisCommandRecoveryJournal
            GetAxisCommandRecoveryJournal(MainWindow window)
        {
            return (AxisCommandRecoveryJournal)GetPrivateField(
                window,
                "axisCommandRecoveryJournal");
        }

        private static GroupPowerRecoveryJournal
            GetGroupPowerRecoveryJournal(MainWindow window)
        {
            return (GroupPowerRecoveryJournal)GetPrivateField(
                window,
                "groupPowerRecoveryJournal");
        }

        private static GroupProfileLockRecoveryJournal
            GetGroupProfileLockRecoveryJournal(MainWindow window)
        {
            return (GroupProfileLockRecoveryJournal)GetPrivateField(
                window,
                "groupProfileLockRecoveryJournal");
        }

        private static object GetPrivateField(
            object instance,
            string fieldName)
        {
            var field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    instance.GetType().FullName,
                    fieldName);
            }

            return field.GetValue(instance);
        }

        private static void SetPrivateField(
            object instance,
            string fieldName,
            object value)
        {
            var field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    instance.GetType().FullName,
                    fieldName);
            }

            field.SetValue(instance, value);
        }

        private static object InvokePrivate(
            object instance,
            string methodName,
            params object[] arguments)
        {
            var method = instance.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(
                    instance.GetType().FullName,
                    methodName);
            }

            return method.Invoke(instance, arguments);
        }

        private static void RunChildObservationLoop(string scenarioName)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var observationThread = new Thread(
                () =>
                {
                    string request;
                    while ((request = Console.ReadLine()) != null)
                    {
                        if (string.Equals(
                                request,
                                ObservationRequest,
                                StringComparison.Ordinal))
                        {
                            dispatcher.BeginInvoke(
                                DispatcherPriority.ApplicationIdle,
                                new Action(
                                    () =>
                                    {
                                        Console.WriteLine(
                                            ObservationPrefix
                                            + scenarioName);
                                        Console.Out.Flush();
                                    }));
                        }
                        else if (string.Equals(
                            request,
                            CleanupExitRequest,
                            StringComparison.Ordinal))
                        {
                            dispatcher.BeginInvokeShutdown(
                                DispatcherPriority.Send);
                            return;
                        }
                    }
                })
            {
                IsBackground = true,
                Name = "WPF recovery observation control"
            };
            observationThread.Start();

            Console.WriteLine(ReadyPrefix + scenarioName);
            Console.Out.Flush();
            Dispatcher.Run();
        }

        private static void AssertConnectOnlyWire(IList<ushort> commands)
        {
            AssertEx.Equal(
                3,
                commands.Count,
                "Recovered WPF startup sent an unexpected RPC request.");
            AssertEx.Equal((ushort)0x8080, commands[0]);
            AssertEx.Equal((ushort)0x405C, commands[1]);
            AssertEx.Equal((ushort)0x7E00, commands[2]);

            var sdoWriteCount = 0;
            var digitalOutputWriteCount = 0;
            var recorderCommandCount = 0;
            foreach (var command in commands)
            {
                if (command == 0x7E50)
                {
                    sdoWriteCount++;
                }
                else if (command == 0x7E23)
                {
                    digitalOutputWriteCount++;
                }

                if (command >= 0x7E40 && command <= 0x7E4F)
                {
                    recorderCommandCount++;
                }
            }

            AssertEx.Equal(
                0,
                sdoWriteCount,
                "Recovered SDO Write evidence was automatically replayed.");
            AssertEx.Equal(
                0,
                digitalOutputWriteCount,
                "Recovered digital output Write evidence was automatically replayed.");
            AssertEx.Equal(
                0,
                recorderCommandCount,
                "Recovered Double-bank evidence caused an automatic Recorder request.");
        }

        private static void AssertMotionRecoveryIdentity(
            MotionUncertaintyRecord record,
            int rpcPort)
        {
            AssertEx.Equal("127.0.0.1", record.EndpointIp);
            AssertEx.Equal(rpcPort, record.EndpointPort);
            AssertEx.Equal(
                MotionUncertaintyTargetKind.Axis,
                record.TargetKind);
            AssertEx.Equal(MotionAxisName, record.TargetName);
            AssertEx.Equal(MotionAxisReference, record.TargetReference);
            AssertEx.Equal(MotionOperation, record.Operation);
            AssertEx.Equal(
                MotionDiagnosticsBootId,
                record.DiagnosticsBootId);
            AssertEx.Equal(MotionMapRevision, record.MapRevision);
        }

        private static void AssertAxisPowerRecoveryIdentity(
            AxisPowerOnRecoveryRecord record,
            int rpcPort,
            bool expectedPowerOn)
        {
            AssertEx.Equal(expectedPowerOn, record.ExpectedPowerOn);
            AssertEx.Equal("127.0.0.1", record.EndpointIp);
            AssertEx.Equal(rpcPort, record.EndpointPort);
            AssertEx.Equal(MotionAxisName, record.AxisName);
            AssertEx.Equal(MotionAxisReference, record.AxisReference);
            AssertEx.Equal(
                AxisPowerDiagnosticsBootId,
                record.DiagnosticsBootId);
            AssertEx.Equal(AxisPowerMapRevision, record.MapRevision);
        }

        private static void AssertGroupProfileLockRecoveryIdentity(
            GroupProfileLockRecoveryRecord record,
            int rpcPort)
        {
            AssertEx.Equal("127.0.0.1", record.EndpointIp);
            AssertEx.Equal(rpcPort, record.EndpointPort);
            AssertEx.Equal(GroupPowerName, record.GroupName);
            AssertEx.Equal(
                GroupPowerReference,
                record.GroupReference);
            AssertEx.Equal(
                GroupProfileLockDiagnosticsBootId,
                record.DiagnosticsBootId);
            AssertEx.Equal(
                GroupProfileLockMapRevision,
                record.MapRevision);
        }

        private static void AssertMotionDispatchWire(
            IList<ushort> commands)
        {
            AssertExactCommands(
                commands,
                new ushort[]
                {
                    0x8080,
                    0x405C,
                    0x7E00,
                    0x103C,
                    0x202B,
                    0x2028,
                    0x202E,
                    0x7E00,
                    0x209F
                },
                "The killed Move-dispatch session did not stop exactly at the unanswered Move request.");
        }

        private static void AssertMotionRecoveryWire(
            IList<ushort> commands)
        {
            AssertExactCommands(
                commands,
                new ushort[]
                {
                    0x8080,
                    0x405C,
                    0x7E00,
                    0x7E00,
                    0x103C,
                    0x202B,
                    0x7E00,
                    0x2022,
                    0x2028,
                    0x2028,
                    0x2028,
                    0x7E00,
                    0x7E00,
                    0x405D
                },
                "The restarted WPF recovery session did not use the exact read-only lookup and single-Stop recovery path.");

            var moveCount = 0;
            var stopCount = 0;
            var statusCount = 0;
            foreach (var command in commands)
            {
                if (command == 0x209F)
                {
                    moveCount++;
                }
                else if (command == 0x2022)
                {
                    stopCount++;
                }
                else if (command == 0x2028)
                {
                    statusCount++;
                }
            }

            AssertEx.Equal(
                0,
                moveCount,
                "The restarted WPF process replayed Move Absolute.");
            AssertEx.Equal(
                1,
                stopCount,
                "Motion recovery did not send exactly one Stop request.");
            AssertEx.Equal(
                3,
                statusCount,
                "Motion recovery did not require exactly three stable status samples.");
        }

        private static void AssertExactCommands(
            IList<ushort> actual,
            IList<ushort> expected,
            string message)
        {
            AssertEx.Equal(expected.Count, actual.Count, message);
            for (var index = 0; index < expected.Count; index++)
            {
                AssertEx.Equal(
                    expected[index],
                    actual[index],
                    message + " CommandIndex=" + index + ".");
            }
        }

        private static RecoveryScenario GetScenario(
            DiagnosticsMutationKind kind)
        {
            if (kind == DiagnosticsMutationKind.SdoWrite)
            {
                return new RecoveryScenario(
                    kind,
                    0x10203040u,
                    new DateTime(638892000000000000L, DateTimeKind.Utc),
                    "Slave=2,Object=0x2F00,SubIndex=24,Type=Int32,Length=4",
                    "WriteData=2A-00-00-00");
            }

            if (kind == DiagnosticsMutationKind.DigitalOutputWrite)
            {
                return new RecoveryScenario(
                    kind,
                    0x50607080u,
                    new DateTime(638892000010000000L, DateTimeKind.Utc),
                    "Node=0x00010001,IOReference=0x00020001",
                    "FullValue=0x0000000000000001,Mask=0x0000000000000001,SourceRevision=0x00000009");
            }

            throw new ArgumentOutOfRangeException("kind");
        }

        private static string CreateTestDirectoryPath()
        {
            return Path.Combine(
                Path.GetTempPath(),
                TestDirectoryPrefix + Guid.NewGuid().ToString("N"));
        }

        private static string RequireTestDirectoryPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "A mutation-recovery test directory is required.",
                    "path");
            }

            var fullPath = Path.GetFullPath(path);
            var tempRoot = Path.GetFullPath(Path.GetTempPath())
                .TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(
                    tempRoot,
                    StringComparison.OrdinalIgnoreCase)
                || !Path.GetFileName(fullPath).StartsWith(
                    TestDirectoryPrefix,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Mutation-recovery child paths must be generated under the process temporary directory.");
            }

            return fullPath;
        }

        private static void DeleteTestDirectory(string path)
        {
            var fullPath = RequireTestDirectoryPath(path);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }

        private static void Click(Button button)
        {
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
        }

        private static void WaitUntil(Func<bool> condition, string message)
        {
            var timer = Stopwatch.StartNew();
            while (!condition())
            {
                if (timer.ElapsedMilliseconds >= WaitTimeoutMilliseconds)
                {
                    throw new TimeoutException(message);
                }

                PumpDispatcherOnce();
            }
        }

        private static void PumpDispatcherOnce()
        {
            var frame = new DispatcherFrame();
            var timer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(5),
                DispatcherPriority.Background,
                (sender, args) =>
                {
                    ((DispatcherTimer)sender).Stop();
                    frame.Continue = false;
                },
                Dispatcher.CurrentDispatcher);
            timer.Start();
            Dispatcher.PushFrame(frame);
        }

        private static string QuoteProcessArgument(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var result = new StringBuilder();
            result.Append('"');
            var backslashCount = 0;
            foreach (var character in value)
            {
                if (character == '\\')
                {
                    backslashCount++;
                    continue;
                }

                if (character == '"')
                {
                    result.Append('\\', backslashCount * 2 + 1);
                    result.Append('"');
                    backslashCount = 0;
                    continue;
                }

                result.Append('\\', backslashCount);
                backslashCount = 0;
                result.Append(character);
            }

            result.Append('\\', backslashCount * 2);
            result.Append('"');
            return result.ToString();
        }

        private sealed class RecoveryScenario
        {
            internal RecoveryScenario(
                DiagnosticsMutationKind kind,
                uint revision,
                DateTime createdUtc,
                string target,
                string expected)
            {
                Kind = kind;
                Revision = revision;
                CreatedUtc = createdUtc;
                UpdatedUtc = createdUtc.AddSeconds(1);
                Target = target;
                Expected = expected;
            }

            internal DiagnosticsMutationKind Kind { get; private set; }
            internal uint Revision { get; private set; }
            internal DateTime CreatedUtc { get; private set; }
            internal DateTime UpdatedUtc { get; private set; }
            internal string Target { get; private set; }
            internal string Expected { get; private set; }
        }

        private sealed class RecoveryChildProcess : IDisposable
        {
            private readonly object outputSync = new object();
            private readonly StringBuilder output = new StringBuilder();
            private readonly ManualResetEventSlim ready =
                new ManualResetEventSlim(false);
            private readonly ManualResetEventSlim observed =
                new ManualResetEventSlim(false);
            private readonly ManualResetEventSlim exited =
                new ManualResetEventSlim(false);

            private RecoveryChildProcess(Process process)
            {
                Process = process;
            }

            internal Process Process { get; private set; }

            internal static RecoveryChildProcess Start(
                string directoryPath,
                int rpcPort,
                DiagnosticsMutationKind kind)
            {
                return Start(directoryPath, rpcPort, kind.ToString());
            }

            internal static RecoveryChildProcess Start(
                string directoryPath,
                int rpcPort,
                string scenarioName)
            {
                var executablePath = Assembly.GetExecutingAssembly().Location;
                var arguments = string.Join(
                    " ",
                    new[]
                    {
                        QuoteProcessArgument(ChildMode),
                        QuoteProcessArgument(directoryPath),
                        QuoteProcessArgument(
                            rpcPort.ToString(CultureInfo.InvariantCulture)),
                        QuoteProcessArgument(scenarioName)
                    });
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = executablePath,
                        Arguments = arguments,
                        WorkingDirectory = Path.GetDirectoryName(executablePath),
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                var child = new RecoveryChildProcess(process);
                process.OutputDataReceived += child.OnOutput;
                process.ErrorDataReceived += child.OnOutput;
                process.Exited += child.OnExited;
                if (!process.Start())
                {
                    child.Dispose();
                    throw new InvalidOperationException(
                        "Failed to start the WPF mutation-recovery child.");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                return child;
            }

            internal void WaitUntilReady()
            {
                var signaled = WaitHandle.WaitAny(
                    new[] { ready.WaitHandle, exited.WaitHandle },
                    WaitTimeoutMilliseconds);
                if (signaled == 0)
                {
                    return;
                }

                if (signaled == 1)
                {
                    Process.WaitForExit();
                    throw new InvalidOperationException(
                        "The WPF mutation-recovery child exited before READY. ExitCode="
                        + Process.ExitCode
                        + Environment.NewLine
                        + GetOutput());
                }

                throw new TimeoutException(
                    "The WPF mutation-recovery child did not publish READY within "
                    + WaitTimeoutMilliseconds
                    + " ms."
                    + Environment.NewLine
                    + GetOutput());
            }

            internal void RequestObservationBarrier()
            {
                if (Process.HasExited)
                {
                    Process.WaitForExit();
                    throw new InvalidOperationException(
                        "The WPF mutation-recovery child exited before the observation barrier."
                        + Environment.NewLine
                        + GetOutput());
                }

                Process.StandardInput.WriteLine(ObservationRequest);
                Process.StandardInput.Flush();
                var signaled = WaitHandle.WaitAny(
                    new[] { observed.WaitHandle, exited.WaitHandle },
                    WaitTimeoutMilliseconds);
                if (signaled == 0)
                {
                    return;
                }

                if (signaled == 1)
                {
                    Process.WaitForExit();
                    throw new InvalidOperationException(
                        "The WPF mutation-recovery child exited before the observation barrier completed. ExitCode="
                        + Process.ExitCode
                        + Environment.NewLine
                        + GetOutput());
                }

                throw new TimeoutException(
                    "The WPF mutation-recovery child did not complete the dispatcher observation barrier."
                    + Environment.NewLine
                    + GetOutput());
            }

            internal void TerminateAndVerifyForced()
            {
                if (Process.HasExited)
                {
                    Process.WaitForExit();
                    throw new InvalidOperationException(
                        "The WPF mutation-recovery child exited before Kill."
                        + Environment.NewLine
                        + GetOutput());
                }

                Process.Kill();
                if (!Process.WaitForExit(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The WPF mutation-recovery child did not terminate after Kill.");
                }

                Process.WaitForExit();
            }

            internal void WaitForSuccessfulExit()
            {
                if (!Process.WaitForExit(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The WPF mutation-recovery child did not exit after publishing READY."
                        + Environment.NewLine
                        + GetOutput());
                }

                Process.WaitForExit();
                AssertEx.Equal(
                    0,
                    Process.ExitCode,
                    "The WPF mutation-recovery child failed."
                    + Environment.NewLine
                    + GetOutput());
            }

            public void Dispose()
            {
                if (Process == null)
                {
                    return;
                }

                if (!Process.HasExited)
                {
                    var killIssued = false;
                    System.ComponentModel.Win32Exception killError = null;
                    try
                    {
                        Process.Kill();
                        killIssued = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // The process exited after the HasExited check.
                    }
                    catch (System.ComponentModel.Win32Exception error)
                    {
                        killError = error;
                    }

                    if (!Process.HasExited
                        && killIssued)
                    {
                        Process.WaitForExit(WaitTimeoutMilliseconds);
                    }

                    if (!Process.HasExited)
                    {
                        try
                        {
                            Process.StandardInput.WriteLine(
                                CleanupExitRequest);
                            Process.StandardInput.Flush();
                        }
                        catch
                        {
                            // The definitive bounded wait below decides cleanup.
                        }

                        if (!Process.WaitForExit(WaitTimeoutMilliseconds))
                        {
                            throw new TimeoutException(
                                "The WPF mutation-recovery child remained alive after Kill and cleanup fallback. Process handles were retained.",
                                killError);
                        }
                    }
                }

                Process.WaitForExit();
                Process.OutputDataReceived -= OnOutput;
                Process.ErrorDataReceived -= OnOutput;
                Process.Exited -= OnExited;
                Process.Dispose();
                Process = null;
                ready.Dispose();
                observed.Dispose();
                exited.Dispose();
            }

            private void OnOutput(object sender, DataReceivedEventArgs e)
            {
                if (e.Data == null)
                {
                    return;
                }

                lock (outputSync)
                {
                    output.AppendLine(e.Data);
                }

                if (e.Data.StartsWith(
                        ReadyPrefix,
                        StringComparison.Ordinal))
                {
                    ready.Set();
                }
                else if (e.Data.StartsWith(
                    ObservationPrefix,
                    StringComparison.Ordinal))
                {
                    observed.Set();
                }
            }

            private void OnExited(object sender, EventArgs e)
            {
                exited.Set();
            }

            private string GetOutput()
            {
                lock (outputSync)
                {
                    return output.ToString();
                }
            }
        }

        private sealed class MotionRecoveryRpcServer : IDisposable
        {
            private readonly object sync = new object();
            private readonly TcpListener listener;
            private readonly Thread worker;
            private readonly List<ushort> dispatchCommands =
                new List<ushort>();
            private readonly List<ushort> recoveryCommands =
                new List<ushort>();
            private readonly ManualResetEventSlim moveRequestObserved =
                new ManualResetEventSlim(false);
            private readonly ManualResetEventSlim firstSessionStopped =
                new ManualResetEventSlim(false);
            private readonly ManualResetEventSlim recoveryProofObserved =
                new ManualResetEventSlim(false);
            private readonly ManualResetEventSlim completed =
                new ManualResetEventSlim(false);
            private TcpClient acceptedClient;
            private Exception workerException;
            private volatile bool disposed;
            private volatile bool firstTerminationExpected;

            internal MotionRecoveryRpcServer()
            {
                listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                Port = ((IPEndPoint)listener.LocalEndpoint).Port;
                worker = new Thread(Run)
                {
                    IsBackground = true,
                    Name = "WPF forced-kill motion recovery RPC server"
                };
                worker.Start();
            }

            internal int Port { get; private set; }

            internal void WaitForMoveRequest()
            {
                WaitForSignal(
                    moveRequestObserved,
                    "The Move-dispatch child did not send Move Absolute.");
            }

            internal void AssertFirstSessionStillObserving()
            {
                Exception failure;
                bool hasClient;
                lock (sync)
                {
                    failure = workerException;
                    hasClient = acceptedClient != null;
                }

                if (failure != null)
                {
                    throw new InvalidOperationException(
                        "The forced-kill motion RPC server failed before process termination.",
                        failure);
                }

                AssertEx.True(
                    hasClient
                        && worker.IsAlive
                        && !firstSessionStopped.IsSet,
                    "The unanswered Move RPC stream closed before forced termination.");
            }

            internal void MarkFirstTerminationExpected()
            {
                AssertFirstSessionStillObserving();
                firstTerminationExpected = true;
            }

            internal void WaitForFirstSessionStop()
            {
                WaitForSignal(
                    firstSessionStopped,
                    "The first motion RPC session did not stop after Kill.");
            }

            internal void WaitForRecoveryProof()
            {
                WaitForSignal(
                    recoveryProofObserved,
                    "The restarted WPF process did not complete the single-Stop recovery proof.");
            }

            internal void WaitForCompletion()
            {
                if (!worker.Join(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The forced-kill motion RPC server did not finish the recovery session.");
                }

                ThrowIfFailed(
                    "The forced-kill motion RPC server failed.");
            }

            internal IList<ushort> SnapshotDispatchCommands()
            {
                lock (sync)
                {
                    return dispatchCommands.ToArray();
                }
            }

            internal IList<ushort> SnapshotRecoveryCommands()
            {
                lock (sync)
                {
                    return recoveryCommands.ToArray();
                }
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                TcpClient client;
                lock (sync)
                {
                    client = acceptedClient;
                }

                if (client != null)
                {
                    client.Close();
                }

                listener.Stop();
                if (worker.IsAlive)
                {
                    worker.Join(WaitTimeoutMilliseconds);
                }

                moveRequestObserved.Dispose();
                firstSessionStopped.Dispose();
                recoveryProofObserved.Dispose();
                completed.Dispose();
            }

            private void Run()
            {
                try
                {
                    RunDispatchSession();
                    if (!disposed)
                    {
                        RunRecoverySession();
                    }
                }
                catch (ObjectDisposedException) when (disposed)
                {
                }
                catch (SocketException) when (disposed)
                {
                }
                catch (Exception error)
                {
                    if (!disposed)
                    {
                        lock (sync)
                        {
                            workerException = error;
                        }
                    }
                }
                finally
                {
                    lock (sync)
                    {
                        acceptedClient = null;
                    }

                    completed.Set();
                }
            }

            private void RunDispatchSession()
            {
                try
                {
                    using (var client = listener.AcceptTcpClient())
                    {
                        SetAcceptedClient(client);
                        client.NoDelay = true;
                        using (var stream = client.GetStream())
                        {
                            while (true)
                            {
                                byte[] request;
                                try
                                {
                                    request = ReadRequest(stream);
                                }
                                catch (EndOfStreamException error)
                                {
                                    EnsureFirstDisconnectExpected(error);
                                    return;
                                }
                                catch (IOException error)
                                {
                                    EnsureFirstDisconnectExpected(error);
                                    return;
                                }

                                var command = TestFrame.ReadUInt16(
                                    request,
                                    0);
                                lock (sync)
                                {
                                    dispatchCommands.Add(command);
                                }

                                if (command == 0x209F)
                                {
                                    AssertEx.Equal(
                                        MotionAxisReference,
                                        TestFrame.ReadUInt16(request, 6),
                                        "Move Absolute targeted the wrong axis reference.");
                                    moveRequestObserved.Set();
                                    continue;
                                }

                                WriteResponse(
                                    stream,
                                    CreateDispatchResponse(
                                        command,
                                        request));
                            }
                        }
                    }
                }
                finally
                {
                    lock (sync)
                    {
                        acceptedClient = null;
                    }

                    firstSessionStopped.Set();
                }
            }

            private void RunRecoverySession()
            {
                var stopSeen = false;
                var stableStatusCount = 0;
                using (var client = listener.AcceptTcpClient())
                {
                    SetAcceptedClient(client);
                    client.NoDelay = true;
                    using (var stream = client.GetStream())
                    {
                        while (true)
                        {
                            byte[] request;
                            try
                            {
                                request = ReadRequest(stream);
                            }
                            catch (EndOfStreamException error)
                            {
                                throw new IOException(
                                    "The restarted WPF recovery stream ended before Close Connection.",
                                    error);
                            }
                            catch (IOException error)
                            {
                                throw new IOException(
                                    "The restarted WPF recovery stream failed before Close Connection.",
                                    error);
                            }

                            var command = TestFrame.ReadUInt16(request, 0);
                            lock (sync)
                            {
                                recoveryCommands.Add(command);
                            }

                            AssertEx.False(
                                command == 0x209F,
                                "The restarted WPF process replayed Move Absolute.");
                            if (command == 0x2022)
                            {
                                AssertEx.False(
                                    stopSeen,
                                    "The restarted WPF process sent Stop more than once.");
                                stopSeen = true;
                            }
                            else if (command == 0x2028)
                            {
                                AssertEx.True(
                                    stopSeen,
                                    "The recovery status proof started before Stop.");
                                stableStatusCount++;
                            }

                            WriteResponse(
                                stream,
                                CreateRecoveryResponse(
                                    command,
                                    request));

                            if (stableStatusCount == 3)
                            {
                                recoveryProofObserved.Set();
                            }

                            if (command == 0x405D)
                            {
                                AssertEx.True(
                                    stopSeen && stableStatusCount == 3,
                                    "Close Connection arrived before the complete Stop recovery proof.");
                                return;
                            }
                        }
                    }
                }
            }

            private void EnsureFirstDisconnectExpected(Exception error)
            {
                if (disposed || firstTerminationExpected)
                {
                    return;
                }

                throw new IOException(
                    "The unanswered Move RPC stream ended before the parent authorized forced termination.",
                    error);
            }

            private void SetAcceptedClient(TcpClient client)
            {
                lock (sync)
                {
                    acceptedClient = client;
                }
            }

            private void WaitForSignal(
                ManualResetEventSlim signal,
                string timeoutMessage)
            {
                var signaled = WaitHandle.WaitAny(
                    new[] { signal.WaitHandle, completed.WaitHandle },
                    WaitTimeoutMilliseconds);
                if (signaled == 0)
                {
                    ThrowIfFailed(
                        "The forced-kill motion RPC server failed.");
                    return;
                }

                if (signaled == 1)
                {
                    ThrowIfFailed(
                        "The forced-kill motion RPC server stopped early.");
                    throw new InvalidOperationException(timeoutMessage);
                }

                throw new TimeoutException(timeoutMessage);
            }

            private void ThrowIfFailed(string message)
            {
                Exception failure;
                lock (sync)
                {
                    failure = workerException;
                }

                if (failure != null)
                {
                    throw new InvalidOperationException(message, failure);
                }
            }

            private static byte[] CreateDispatchResponse(
                ushort command,
                byte[] request)
            {
                if (command == 0x2028)
                {
                    AssertAxisReference(request, "Read Status");
                    var payload = new byte[12];
                    TestFrame.WriteUInt32(payload, 0, 0x00000001u);
                    return TestFrame.Response(0, payload);
                }

                if (command == 0x202E)
                {
                    AssertAxisReference(request, "Read Position");
                    return TestFrame.Response(0, new byte[8]);
                }

                return CreateSharedResponse(command, request, false);
            }

            private static byte[] CreateRecoveryResponse(
                ushort command,
                byte[] request)
            {
                if (command == 0x2022)
                {
                    AssertAxisReference(request, "Stop");
                    return TestFrame.Response(0, new byte[8]);
                }

                if (command == 0x2028)
                {
                    AssertAxisReference(request, "Recovery Read Status");
                    var payload = new byte[12];
                    TestFrame.WriteUInt32(payload, 0, 0x02000001u);
                    return TestFrame.Response(0, payload);
                }

                return CreateSharedResponse(command, request, true);
            }

            private static byte[] CreateSharedResponse(
                ushort command,
                byte[] request,
                bool allowClose)
            {
                if (command == 0x8080)
                {
                    var payload = new byte[24];
                    TestFrame.WriteUInt32(payload, 0, 64);
                    return TestFrame.Response(0, payload);
                }

                if (command == 0x405C
                    || (allowClose && command == 0x405D))
                {
                    return TestFrame.Response(0, new byte[4]);
                }

                if (command == 0x7E00)
                {
                    var requestId = request.Length >= 16
                        ? TestFrame.ReadUInt32(request, 12)
                        : 1u;
                    return TestFrame.Response(
                        0,
                        CreateCapabilitiesPayload(requestId));
                }

                if (command == 0x103C)
                {
                    AssertAxisLookupRequest(request);
                    var payload = new byte[6];
                    TestFrame.WriteUInt16(
                        payload,
                        4,
                        MotionAxisReference);
                    return TestFrame.Response(0, payload);
                }

                if (command == 0x202B)
                {
                    AssertAxisInfoRequest(request);
                    var payload = new byte[8];
                    TestFrame.WriteUInt32(
                        payload,
                        0,
                        MotionAxisReference);
                    return TestFrame.Response(0, payload);
                }

                throw new InvalidOperationException(
                    "Unexpected motion RPC command 0x"
                    + command.ToString("X4", CultureInfo.InvariantCulture)
                    + ".");
            }

            private static byte[] CreateCapabilitiesPayload(uint requestId)
            {
                var payload = new byte[68];
                TestFrame.WriteUInt16(payload, 0, 1);
                TestFrame.WriteUInt32(payload, 8, requestId);
                TestFrame.WriteUInt32(payload, 16, 1);
                TestFrame.WriteUInt32(
                    payload,
                    20,
                    (uint)LMCDiagnosticCapability.None);
                TestFrame.WriteUInt32(
                    payload,
                    24,
                    MotionMapRevision);
                TestFrame.WriteUInt32(payload, 40, 1000);
                TestFrame.WriteUInt16(payload, 44, 1320);
                TestFrame.WriteUInt16(payload, 46, 2040);
                TestFrame.WriteUInt16(payload, 48, 1280);
                TestFrame.WriteUInt16(payload, 50, 80);
                TestFrame.WriteUInt16(payload, 52, 16);
                TestFrame.WriteUInt16(payload, 60, 4);
                TestFrame.WriteUInt32(
                    payload,
                    64,
                    MotionDiagnosticsBootId);
                return payload;
            }

            private static void AssertAxisLookupRequest(byte[] request)
            {
                var payload = new byte[80];
                var name = Encoding.ASCII.GetBytes(MotionAxisName);
                Buffer.BlockCopy(name, 0, payload, 0, name.Length);
                AssertEx.SequenceEqual(
                    TestFrame.Request(0x103C, 0, payload),
                    request,
                    "Motion recovery looked up a target other than the exact recorded axis.");
            }

            private static void AssertAxisInfoRequest(byte[] request)
            {
                var payload = new byte[12];
                TestFrame.WriteInt32(payload, 0, 5);
                TestFrame.WriteInt32(payload, 8, 1);
                AssertEx.SequenceEqual(
                    TestFrame.Request(
                        0x202B,
                        MotionAxisReference,
                        payload),
                    request,
                    "Motion recovery validated the wrong axis reference.");
            }

            private static void AssertAxisReference(
                byte[] request,
                string operation)
            {
                AssertEx.Equal(
                    MotionAxisReference,
                    TestFrame.ReadUInt16(request, 6),
                    operation + " targeted the wrong axis reference.");
            }

            private static byte[] ReadRequest(NetworkStream stream)
            {
                var header = ReadExact(stream, 8);
                var payloadLength = TestFrame.ReadUInt16(header, 4);
                var payload = payloadLength == 0
                    ? new byte[0]
                    : ReadExact(stream, payloadLength);
                var request = new byte[header.Length + payload.Length];
                Buffer.BlockCopy(header, 0, request, 0, header.Length);
                if (payload.Length != 0)
                {
                    Buffer.BlockCopy(
                        payload,
                        0,
                        request,
                        header.Length,
                        payload.Length);
                }

                return request;
            }

            private static byte[] ReadExact(
                NetworkStream stream,
                int count)
            {
                var bytes = new byte[count];
                var offset = 0;
                while (offset < count)
                {
                    var read = stream.Read(
                        bytes,
                        offset,
                        count - offset);
                    if (read <= 0)
                    {
                        throw new EndOfStreamException();
                    }

                    offset += read;
                }

                return bytes;
            }

            private static void WriteResponse(
                NetworkStream stream,
                byte[] response)
            {
                stream.Write(response, 0, response.Length);
                stream.Flush();
            }
        }

        private sealed class RecoveryRpcObserverServer : IDisposable
        {
            private readonly object sync = new object();
            private readonly TcpListener listener;
            private readonly Thread worker;
            private readonly List<ushort> commands = new List<ushort>();
            private readonly ManualResetEventSlim connectSequenceObserved =
                new ManualResetEventSlim(false);
            private TcpClient acceptedClient;
            private Exception workerException;
            private volatile bool disposed;
            private volatile bool terminationExpected;

            internal RecoveryRpcObserverServer()
            {
                listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                Port = ((IPEndPoint)listener.LocalEndpoint).Port;
                worker = new Thread(Run)
                {
                    IsBackground = true,
                    Name = "WPF recovery RPC observer"
                };
                worker.Start();
            }

            internal int Port { get; private set; }

            internal void WaitForConnectSequence()
            {
                if (!connectSequenceObserved.Wait(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The WPF recovery observer did not receive the connect sequence.");
                }
            }

            internal IList<ushort> SnapshotCommands()
            {
                lock (sync)
                {
                    return commands.ToArray();
                }
            }

            internal void AssertStillObserving()
            {
                Exception failure;
                bool hasClient;
                lock (sync)
                {
                    failure = workerException;
                    hasClient = acceptedClient != null;
                }

                if (failure != null)
                {
                    throw new InvalidOperationException(
                        "The WPF recovery RPC observer failed before forced termination.",
                        failure);
                }

                AssertEx.True(
                    hasClient && worker.IsAlive,
                    "The WPF recovery RPC stream closed before forced termination.");
            }

            internal void MarkTerminationExpected()
            {
                AssertStillObserving();
                terminationExpected = true;
            }

            internal void WaitForStop()
            {
                if (!worker.Join(WaitTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "The WPF recovery RPC observer did not stop after child termination.");
                }

                Exception failure;
                lock (sync)
                {
                    failure = workerException;
                }

                if (failure != null)
                {
                    throw new InvalidOperationException(
                        "The WPF recovery RPC observer failed.",
                        failure);
                }
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                TcpClient client;
                lock (sync)
                {
                    client = acceptedClient;
                }

                if (client != null)
                {
                    client.Close();
                }

                listener.Stop();
                if (worker.IsAlive)
                {
                    worker.Join(WaitTimeoutMilliseconds);
                }

                connectSequenceObserved.Dispose();
            }

            private void Run()
            {
                var requestCount = 0;
                try
                {
                    using (var client = listener.AcceptTcpClient())
                    {
                        lock (sync)
                        {
                            acceptedClient = client;
                        }

                        client.NoDelay = true;
                        using (var stream = client.GetStream())
                        {
                            while (true)
                            {
                                byte[] request;
                                try
                                {
                                    request = ReadRequest(stream);
                                }
                                catch (EndOfStreamException error)
                                {
                                    RecordDisconnectIfUnexpected(error);
                                    break;
                                }
                                catch (IOException error)
                                {
                                    RecordDisconnectIfUnexpected(error);
                                    break;
                                }

                                var command = TestFrame.ReadUInt16(request, 0);
                                lock (sync)
                                {
                                    commands.Add(command);
                                }

                                requestCount++;
                                WriteResponse(
                                    stream,
                                    CreateResponse(command, request));
                                if (requestCount >= 3)
                                {
                                    connectSequenceObserved.Set();
                                }
                            }
                        }
                    }
                }
                catch (ObjectDisposedException) when (disposed)
                {
                }
                catch (SocketException) when (disposed)
                {
                }
                catch (Exception error)
                {
                    if (!disposed)
                    {
                        lock (sync)
                        {
                            workerException = error;
                        }
                    }
                }
                finally
                {
                    lock (sync)
                    {
                        acceptedClient = null;
                    }
                }
            }

            private void RecordDisconnectIfUnexpected(Exception error)
            {
                if (disposed || terminationExpected)
                {
                    return;
                }

                lock (sync)
                {
                    workerException = new IOException(
                        "The WPF recovery RPC stream ended before the parent authorized forced termination.",
                        error);
                }
            }

            private static byte[] CreateResponse(
                ushort command,
                byte[] request)
            {
                if (command == 0x8080)
                {
                    var payload = new byte[24];
                    TestFrame.WriteUInt32(payload, 0, 64);
                    return TestFrame.Response(0, payload);
                }

                if (command == 0x405C || command == 0x405D)
                {
                    return TestFrame.Response(
                        0,
                        TestFrame.Hex("00 00 00 00"));
                }

                if (command == 0x7E00)
                {
                    var requestId = request.Length >= 16
                        ? TestFrame.ReadUInt32(request, 12)
                        : 1u;
                    return TestFrame.Response(
                        0,
                        CreateCapabilitiesPayload(requestId));
                }

                return TestFrame.Response(
                    0,
                    TestFrame.Hex("01 00 FF FF"));
            }

            private static byte[] CreateCapabilitiesPayload(uint requestId)
            {
                var payload = new byte[68];
                TestFrame.WriteUInt16(payload, 0, 1);
                TestFrame.WriteUInt32(payload, 8, requestId);
                TestFrame.WriteUInt32(payload, 16, 1);
                TestFrame.WriteUInt32(
                    payload,
                    20,
                    (uint)LMCDiagnosticCapability.None);
                TestFrame.WriteUInt32(payload, 24, 1);
                TestFrame.WriteUInt32(payload, 40, 1000);
                TestFrame.WriteUInt16(payload, 44, 1320);
                TestFrame.WriteUInt16(payload, 46, 2040);
                TestFrame.WriteUInt16(payload, 48, 1280);
                TestFrame.WriteUInt16(payload, 50, 80);
                TestFrame.WriteUInt16(payload, 52, 16);
                TestFrame.WriteUInt16(payload, 60, 4);
                TestFrame.WriteUInt32(payload, 64, 0x10203040u);
                return payload;
            }

            private static byte[] ReadRequest(NetworkStream stream)
            {
                var header = ReadExact(stream, 8);
                var payloadLength = TestFrame.ReadUInt16(header, 4);
                var payload = payloadLength == 0
                    ? new byte[0]
                    : ReadExact(stream, payloadLength);
                var request = new byte[header.Length + payload.Length];
                Buffer.BlockCopy(header, 0, request, 0, header.Length);
                if (payload.Length != 0)
                {
                    Buffer.BlockCopy(
                        payload,
                        0,
                        request,
                        header.Length,
                        payload.Length);
                }

                return request;
            }

            private static byte[] ReadExact(NetworkStream stream, int count)
            {
                var bytes = new byte[count];
                var offset = 0;
                while (offset < count)
                {
                    var read = stream.Read(bytes, offset, count - offset);
                    if (read <= 0)
                    {
                        throw new EndOfStreamException();
                    }

                    offset += read;
                }

                return bytes;
            }

            private static void WriteResponse(
                NetworkStream stream,
                byte[] response)
            {
                stream.Write(response, 0, response.Length);
                stream.Flush();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (WpfExecutableRelaunchIntegrationTests.IsInvocation(args))
            {
                return WpfExecutableRelaunchIntegrationTests.Run(args);
            }

            var dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(dispatcher));
            var application = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };

            try
            {
                if (WpfMutationRecoveryProcessTests.IsChildInvocation(args))
                {
                    return WpfMutationRecoveryProcessTests.RunChild(args);
                }

                string filter = null;
                if (args != null && args.Length != 0)
                {
                    if (args.Length == 2
                        && string.Equals(
                            args[0],
                            "--filter",
                            StringComparison.Ordinal)
                        && !string.IsNullOrWhiteSpace(args[1]))
                    {
                        filter = args[1];
                    }
                    else
                    {
                        Console.Error.WriteLine(
                            "ERROR use --filter <test-name-substring>, or the exact mutation-recovery child mode.");
                        return 64;
                    }
                }

                var tests = new List<TestCase>();
                LasalMotionControlApiExample
                    .UiLocalizationTests.Register(tests);
                LasalMotionControlApiExample
                    .ApplicationInstanceLeaseTests.Register(tests);
                LasalMotionControlApiExample
                    .RecoveryRecordRetirementTests.Register(tests);
                LasalMotionControlApiExample
                    .GroupProfileLockRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .AxisPowerOnRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .AxisSetPositionRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .AxisSetOperationModeRecoveryJournalTests.Register(tests);
                AxisSetOperationModeSdkRecoveryIdentityTests.Register(tests);
                LasalMotionControlApiExample
                    .AxisDs402HomeExRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .MaintenanceActionRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .AxisCommandRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .AxisQualificationRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .GroupPowerRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .GroupResetRecoveryJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .MotionUncertaintyJournalTests.Register(tests);
                LasalMotionControlApiExample
                    .SdoWriteActivationQualificationProofTests.Register(tests);
                WpfMainWindowIntegrationTests
                    .RegisterMotionRecoveryIntegrationTests(tests);
                WpfMainWindowIntegrationTests
                    .RegisterAxisPowerOnRecoveryTests(tests);
                WpfMainWindowIntegrationTests
                    .RegisterAxisSetOperationModeRecoveryTests(tests);
                WpfMainWindowIntegrationTests
                    .RegisterRecoveryRecordRetirementIntegrationTests(tests);
                WpfMainWindowIntegrationTests
                    .RegisterAxisCommandRecoveryIntegrationTests(tests);
                WpfMainWindowIntegrationTests
                    .RegisterAxisQualificationIntegrationTests(tests);
                WpfMaintenanceActionIntegrationTests.Register(tests);
                WpfTopologyIoLiveEvidenceTests.Register(tests);
                WpfMainWindowIntegrationTests.Register(tests);
                WpfMainWindowIntegrationTests
                    .RegisterGroupStopCompoundTests(tests);
                WpfMainWindowIntegrationTests
                    .RegisterGroupResetRecoveryTests(tests);
                WpfMutationRecoveryProcessTests.Register(tests);

                var selected = 0;
                var failed = 0;
                foreach (var test in tests)
                {
                    if (filter != null
                        && test.Name.IndexOf(
                            filter,
                            StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    selected++;
                    try
                    {
                        test.Body();
                        Console.WriteLine("PASS " + test.Name);
                    }
                    catch (Exception error)
                    {
                        failed++;
                        Console.Error.WriteLine("FAIL " + test.Name);
                        Console.Error.WriteLine(error);
                    }
                }

                Console.WriteLine(
                    "TOTAL "
                    + selected
                    + ", PASSED "
                    + (selected - failed)
                    + ", FAILED "
                    + failed);
                if (selected == 0)
                {
                    Console.Error.WriteLine(
                        "ERROR no tests matched the requested filter.");
                    return 64;
                }

                return failed == 0 ? 0 : 1;
            }
            finally
            {
                application.Shutdown();
            }
        }
    }
}

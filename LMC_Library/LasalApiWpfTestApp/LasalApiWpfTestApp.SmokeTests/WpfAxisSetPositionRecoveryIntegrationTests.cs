using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        internal static void RegisterAxisSetPositionRecoveryTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.SetPositionRecovery.StartupArmedPromotesAndInterlocks",
                SetPositionStartupArmedPromotesAndInterlocks);
            tests.Add(
                "Wpf.SetPositionRecovery.DormantCapabilityKeepsStartClosed",
                SetPositionDormantCapabilityKeepsStartClosed);
            tests.Add(
                "Wpf.SetPositionRecovery.WindowCloseReleasesJournalLock",
                SetPositionWindowCloseReleasesJournalLock);
        }

        private static void SetPositionStartupArmedPromotesAndInterlocks()
        {
            var root = CreateSetPositionTemporaryDirectory();
            MainWindow window = null;
            try
            {
                var journalDirectory = Path.Combine(
                    root,
                    "AxisSetPositionRecovery");
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(journalDirectory))
                {
                    journal.ArmBeforeDispatch(
                        new Guid("00112233-4455-6677-8899-aabbccddeeff"),
                        "127.0.0.1",
                        4000,
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        "_LMCAxis2",
                        2,
                        0x89ABCDEFU,
                        0x01234567U,
                        0x76543210U,
                        0xFEDCBA98U,
                        0x10203040U,
                        -1234567,
                        7654321,
                        (ushort)LMCAxisSetPositionSemanticMode
                            .ActualAndDestinationApplicationUnits,
                        1,
                        FixedSetPositionUtc());
                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                        journal.CurrentRecord.State);
                }

                window = CreateHiddenWindow(root);
                window.InitializeAxisSetPositionRecoveryForTests();

                var recovered =
                    window.ActiveAxisSetPositionRecoveryRecordForTests;
                AssertEx.NotNull(recovered);
                AssertEx.Equal(
                    AxisSetPositionRecoveryState.RecoveryRequired,
                    recovered.State);
                AssertEx.True(window.AxisSetPositionRecoveryInterlockForTests);
                AssertEx.NotNull(window.AxisSetPositionRecoveryGroupForTests);
                AssertEx.False(window.AxisSetPositionStartButtonForTests.IsEnabled);
                AssertEx.True(window.AxisSetPositionRecoverButtonForTests.IsEnabled);
                AssertEx.False(window.TextRemoteIp.IsEnabled);
                AssertEx.False(window.TextRemotePort.IsEnabled);
                AssertEx.Equal("127.0.0.1", window.TextRemoteIp.Text);
                AssertEx.Equal("4000", window.TextRemotePort.Text);
                AssertEx.Equal((ushort)2, recovered.AxisReference);
                AssertEx.Equal(-1234567, recovered.TargetPosition);
                AssertEx.Equal(7654321, recovered.ExpectedActualPosition);
                AssertEx.Equal(
                    "-1234567",
                    window.AxisSetPositionTargetForTests.Text);
                AssertEx.Equal(
                    "7654321",
                    window.AxisSetPositionExpectedActualForTests.Text);
                AssertEx.Equal(
                    (ushort)2,
                    (ushort)window.AxisSetPositionReferenceForTests.SelectedItem);
            }
            finally
            {
                CloseSetPositionWindow(window);
                DeleteSetPositionRecoveryDirectory(root);
                DeleteSetPositionTemporaryRootBestEffort(root);
            }
        }

        private static void SetPositionDormantCapabilityKeepsStartClosed()
        {
            var root = CreateSetPositionTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = CreateHiddenWindow(root);
                window.InitializeAxisSetPositionRecoveryForTests();

                AssertEx.NotNull(window.AxisSetPositionRecoveryGroupForTests);
                AssertEx.False(window.AxisSetPositionRecoveryInterlockForTests);
                AssertEx.False(
                    window.AxisSetPositionConfirmationForTests.IsChecked == true);
                AssertEx.False(window.AxisSetPositionStartButtonForTests.IsEnabled);

                window.AxisSetPositionTargetForTests.Text = "100";
                window.AxisSetPositionExpectedActualForTests.Text = "90";
                window.AxisSetPositionReferenceForTests.SelectedItem = (ushort)1;
                window.AxisSetPositionConfirmationForTests.IsChecked = true;
                window.RefreshAxisSetPositionRecoveryUiForTests();

                AssertEx.False(
                    window.AxisSetPositionStartButtonForTests.IsEnabled,
                    "WPF must not infer SetPosition activation from local confirmation when the current PLC capability triad is absent.");
                AssertEx.False(window.AxisSetPositionRecoveryInterlockForTests);
            }
            finally
            {
                CloseSetPositionWindow(window);
                DeleteSetPositionRecoveryDirectory(root);
                DeleteSetPositionTemporaryRootBestEffort(root);
            }
        }

        private static void SetPositionWindowCloseReleasesJournalLock()
        {
            var root = CreateSetPositionTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = CreateHiddenWindow(root);
                window.InitializeAxisSetPositionRecoveryForTests();
                AssertEx.NotNull(window.AxisSetPositionRecoveryJournalForTests);

                CloseSetPositionWindow(window);
                window = null;

                var journalDirectory = Path.Combine(
                    root,
                    "AxisSetPositionRecovery");
                using (var reopened =
                    AxisSetPositionRecoveryJournal.Open(journalDirectory))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                }
            }
            finally
            {
                CloseSetPositionWindow(window);
                DeleteSetPositionRecoveryDirectory(root);
                DeleteSetPositionTemporaryRootBestEffort(root);
            }
        }

        private static DateTime FixedSetPositionUtc()
        {
            return new DateTime(
                2026,
                8,
                27,
                1,
                0,
                0,
                DateTimeKind.Utc);
        }

        private static string CreateSetPositionTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoSetPositionWpfSmoke",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void CloseSetPositionWindow(MainWindow window)
        {
            if (window == null || !window.IsLoaded)
            {
                return;
            }

            window.Close();
            WaitForUiCondition(
                () => !window.IsLoaded,
                "The SetPosition test MainWindow did not close.");
        }

        private static void DeleteSetPositionRecoveryDirectory(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return;
            }

            var journalDirectory = Path.Combine(
                root,
                "AxisSetPositionRecovery");
            if (Directory.Exists(journalDirectory))
            {
                // This deletion is intentionally strict. If the SetPosition
                // journal still owns its lock after Window.Close, the focused
                // smoke test must fail rather than hiding the lifecycle leak.
                Directory.Delete(journalDirectory, true);
            }
        }

        private static void DeleteSetPositionTemporaryRootBestEffort(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return;
            }

            try
            {
                // MainWindow also opens legacy/shared recovery journals whose
                // lifetime is outside this focused SetPosition contract. Keep
                // their historical cleanup behavior separate from the strict
                // SetPosition journal assertion above.
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}

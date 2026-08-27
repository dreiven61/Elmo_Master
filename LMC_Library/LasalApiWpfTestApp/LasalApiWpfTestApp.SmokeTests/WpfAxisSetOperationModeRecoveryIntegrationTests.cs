using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        internal static void RegisterAxisSetOperationModeRecoveryTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.SetOperationModeRecovery.StartupArmedPromotesAndLocksEndpoint",
                SetOperationModeStartupArmedPromotesAndLocksEndpoint);
            tests.Add(
                "Wpf.SetOperationModeRecovery.DynamicUiRequiresExplicitConfirmation",
                SetOperationModeDynamicUiRequiresExplicitConfirmation);
            tests.Add(
                "Wpf.SetOperationModeRecovery.SelectorRemainsUsableWithoutPlcMask",
                SetOperationModeSelectorRemainsUsableWithoutPlcMask);
            tests.Add(
                "Wpf.SetOperationModeRecovery.DefinitiveRejectArchivesAndClearsInterlock",
                SetOperationModeDefinitiveRejectArchivesAndClearsInterlock);
            tests.Add(
                "Wpf.SetOperationModeRecovery.RejectIdentityMismatchRetainsInterlock",
                SetOperationModeRejectIdentityMismatchRetainsInterlock);
        }

        private static void SetOperationModeStartupArmedPromotesAndLocksEndpoint()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                var journalDirectory = Path.Combine(
                    root,
                    "AxisSetOperationModeRecovery");
                using (var journal =
                    AxisSetOperationModeRecoveryJournal.Open(
                        journalDirectory))
                {
                    journal.ArmBeforeDispatch(
                        new Guid("00112233-4455-6677-8899-aabbccddeeff"),
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        CreateSetOperationModeRecoveryKey(),
                        FixedSetOperationModeUtc());
                    AssertEx.Equal(
                        AxisSetOperationModeRecoveryState.ArmedBeforeDispatch,
                        journal.CurrentRecord.State);
                }

                window = new MainWindow(root);
                var recovered =
                    window.ActiveAxisSetOperationModeRecoveryRecordForTests;
                AssertEx.NotNull(recovered);
                AssertEx.Equal(
                    AxisSetOperationModeRecoveryState.RecoveryRequired,
                    recovered.State);
                AssertEx.True(
                    window.AxisSetOperationModeRecoveryInterlockForTests);
                AssertEx.NotNull(
                    window.AxisSetOperationModeRecoveryGroupForTests);
                AssertEx.False(
                    window.AxisSetOperationModeStartButtonForTests.IsEnabled);
                AssertEx.False(window.TextRemoteIp.IsEnabled);
                AssertEx.False(window.TextRemotePort.IsEnabled);
                AssertEx.Equal("127.0.0.1", window.TextRemoteIp.Text);
                AssertEx.Equal("4000", window.TextRemotePort.Text);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void SetOperationModeDynamicUiRequiresExplicitConfirmation()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                AssertEx.NotNull(
                    window.AxisSetOperationModeRecoveryGroupForTests);
                AssertEx.NotNull(
                    window.AxisSetOperationModeConfirmationForTests);
                AssertEx.False(
                    window.AxisSetOperationModeConfirmationForTests.IsChecked
                        == true);
                AssertEx.False(
                    window.AxisSetOperationModeStartButtonForTests.IsEnabled);
                AssertEx.False(
                    window.AxisSetOperationModeRecoveryInterlockForTests);

                var journal =
                    window.AxisSetOperationModeRecoveryJournalForTests;
                AssertEx.NotNull(journal);
                journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    CreateSetOperationModeRecoveryKey(),
                    FixedSetOperationModeUtc());
                window.RefreshAxisSetOperationModeRecoveryUiForTests();

                AssertEx.True(
                    window.AxisSetOperationModeRecoveryInterlockForTests);
                AssertEx.False(window.TextRemoteIp.IsEnabled);
                AssertEx.False(window.TextRemotePort.IsEnabled);
                AssertEx.False(
                    window.AxisSetOperationModeStartButtonForTests.IsEnabled);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void SetOperationModeSelectorRemainsUsableWithoutPlcMask()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                var selector = window.AxisSetOperationModeRequestedModeForTests;
                AssertEx.NotNull(selector);
                AssertEx.Equal(4, selector.Items.Count);
                AssertEx.Equal(
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    (LMCDriveOperationMode)selector.SelectedItem);
                AssertEx.True(selector.IsEnabled);
                AssertEx.False(window.AxisSetOperationModeStartButtonForTests.IsEnabled);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void
            SetOperationModeDefinitiveRejectArchivesAndClearsInterlock()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                var journal =
                    window.AxisSetOperationModeRecoveryJournalForTests;
                var key = CreateSetOperationModeRecoveryKey();
                var armed = journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    key,
                    FixedSetOperationModeUtc());
                var recoveryRequired = journal.PromoteToRecoveryRequired(
                    armed,
                    FixedSetOperationModeUtc().AddSeconds(1));
                window.RefreshAxisSetOperationModeRecoveryUiForTests();
                AssertEx.True(
                    window.AxisSetOperationModeRecoveryInterlockForTests);

                var evidencePath =
                    window.ResolveAxisSetOperationModeDefinitiveRejectionForTests(
                        recoveryRequired,
                        key,
                        1,
                        1,
                        -31000,
                        key.OriginalRequestId,
                        44,
                        false);
                window.RefreshAxisSetOperationModeRecoveryUiForTests();

                AssertEx.False(
                    window.AxisSetOperationModeRecoveryInterlockForTests);
                AssertEx.False(
                    window.AxisSetOperationModeRecoveryJournalForTests
                        .HasActiveRecord);
                AssertEx.True(File.Exists(evidencePath));
                var evidence = File.ReadAllText(evidencePath);
                AssertEx.Contains("ELMOASOMREJECT1", evidence);
                AssertEx.Contains(
                    "OriginalRequestId="
                        + key.OriginalRequestId.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains("ResponseDetailCode=44", evidence);
                AssertEx.Contains("ResponseErrorId=-31000", evidence);
                AssertEx.Contains(
                    "RequestedModeRaw="
                        + ((sbyte)LMCDriveOperationMode
                            .CyclicSynchronousPosition).ToString(
                                CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains(
                    "DiagnosticsBuild="
                        + key.DiagnosticsBuild.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains(
                    "DiagnosticsBootId="
                        + key.DiagnosticsBootId.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains(
                    "MapRevision="
                        + key.MapRevision.ToString(
                            CultureInfo.InvariantCulture),
                    evidence);
                AssertEx.Contains("RejectedKeyExact=True", evidence);
                AssertEx.Contains("OriginalJournalSha256=", evidence);
                AssertEx.Contains("OriginalJournalBase64=", evidence);
                AssertEx.Contains("SHA256=", evidence);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static void
            SetOperationModeRejectIdentityMismatchRetainsInterlock()
        {
            var root = CreateSetOperationModeTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                var journal =
                    window.AxisSetOperationModeRecoveryJournalForTests;
                var key = CreateSetOperationModeRecoveryKey();
                var armed = journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    key,
                    FixedSetOperationModeUtc());
                var recoveryRequired = journal.PromoteToRecoveryRequired(
                    armed,
                    FixedSetOperationModeUtc().AddSeconds(1));
                var mismatchedKey = new LMCAxisSetOperationModeRecoveryKey(
                    1,
                    key.OriginalRequestId + 1,
                    key.DiagnosticsBuild,
                    key.DiagnosticsBootId,
                    key.MapRevision,
                    key.ClientIntentId0,
                    key.ClientIntentId1,
                    key.ClientIntentId2,
                    key.ClientIntentId3,
                    key.AxisReference,
                    LMCDriveOperationMode.CyclicSynchronousPosition,
                    key.TimeoutMilliseconds);

                AssertEx.Throws<InvalidOperationException>(
                    () => window
                        .ResolveAxisSetOperationModeDefinitiveRejectionForTests(
                            recoveryRequired,
                            mismatchedKey,
                            1,
                            1,
                            -31000,
                            key.OriginalRequestId,
                            44,
                            false));

                AssertEx.True(
                    window.AxisSetOperationModeRecoveryJournalForTests
                        .HasActiveRecord);
                AssertEx.Equal(
                    AxisSetOperationModeRecoveryState.RecoveryRequired,
                    window.AxisSetOperationModeRecoveryJournalForTests
                        .CurrentRecord.State);
                AssertEx.Equal(
                    0,
                    Directory.GetFiles(
                        Path.Combine(root, "AxisSetOperationModeRecovery"),
                        "*.evidence").Length);
            }
            finally
            {
                if (window != null)
                {
                    window.Close();
                }
                DeleteSetOperationModeTemporaryDirectory(root);
            }
        }

        private static LMCAxisSetOperationModeRecoveryKey
            CreateSetOperationModeRecoveryKey()
        {
            return new LMCAxisSetOperationModeRecoveryKey(
                1,
                0x10203040u,
                0x11223344u,
                0x55667788u,
                0x99AABBCCu,
                0x01020304u,
                0x11121314u,
                0x21222324u,
                0x31323334u,
                1,
                LMCDriveOperationMode.CyclicSynchronousPosition,
                5000u);
        }

        private static DateTime FixedSetOperationModeUtc()
        {
            return new DateTime(
                2026,
                8,
                24,
                8,
                0,
                0,
                DateTimeKind.Utc);
        }

        private static string CreateSetOperationModeTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoSetOperationModeWpfSmoke",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteSetOperationModeTemporaryDirectory(
            string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return;
            }

            try
            {
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

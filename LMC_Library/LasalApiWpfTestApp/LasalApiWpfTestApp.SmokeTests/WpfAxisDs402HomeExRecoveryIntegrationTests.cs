using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using LasalMotionControlApiExample;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalApiWpfTestApp.SmokeTests
{
    internal static partial class WpfMainWindowIntegrationTests
    {
        internal static void RegisterAxisDs402HomeExRecoveryTests(
            ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.HomeDS402ExRecovery.StartupArmedPromotesAndInterlocks",
                HomeDs402ExStartupArmedPromotesAndInterlocks);
            tests.Add(
                "Wpf.HomeDS402ExRecovery.PreDispatchArmIsDurableZeroConnection",
                HomeDs402ExPreDispatchArmIsDurableZeroConnection);
            tests.Add(
                "Wpf.HomeDS402ExRecovery.TerminalRestartRehydratesRetireInputWithoutQuery",
                HomeDs402ExTerminalRestartRehydratesRetireInputWithoutQuery);
            tests.Add(
                "Wpf.HomeDS402ExRecovery.NoStartControlSurface",
                HomeDs402ExNoStartControlSurface);
        }

        private static void HomeDs402ExStartupArmedPromotesAndInterlocks()
        {
            var root = CreateHomeDs402ExTemporaryDirectory();
            MainWindow window = null;
            try
            {
                var journalDirectory = Path.Combine(
                    root,
                    "AxisDs402HomeExRecovery");
                using (var journal =
                    AxisDs402HomeExRecoveryJournal.Open(journalDirectory))
                {
                    journal.ArmBeforeDispatch(
                        new Guid("00112233-4455-6677-8899-aabbccddeeff"),
                        "127.0.0.1",
                        4000,
                        "_LMCAxis2",
                        CreateHomeDs402ExRecoveryKey(),
                        FixedHomeDs402ExUtc());
                    AssertEx.Equal(
                        AxisDs402HomeExRecoveryState.ArmedBeforeDispatch,
                        journal.CurrentRecord.State);
                }

                window = new MainWindow(root);
                window.InitializeAxisDs402HomeExRecoveryForTests();
                var recovered =
                    window.ActiveAxisDs402HomeExRecoveryRecordForTests;
                AssertEx.NotNull(recovered);
                AssertEx.Equal(
                    AxisDs402HomeExRecoveryState.RecoveryRequired,
                    recovered.State);
                AssertEx.True(
                    window.AxisDs402HomeExRecoveryInterlockForTests);
                AssertEx.NotNull(
                    window.AxisDs402HomeExRecoveryGroupForTests);
                AssertEx.True(
                    window.AxisDs402HomeExRecoverButtonForTests.IsEnabled);
                AssertEx.False(window.TextRemoteIp.IsEnabled);
                AssertEx.False(window.TextRemotePort.IsEnabled);
                AssertEx.Equal("127.0.0.1", window.TextRemoteIp.Text);
                AssertEx.Equal("4000", window.TextRemotePort.Text);
                AssertEx.Equal(2, (int)recovered.AxisReference);
                AssertEx.Equal(-100, recovered.Position);
            }
            finally
            {
                CloseHomeDs402ExWindow(window);
                DeleteHomeDs402ExTemporaryDirectory(root);
            }
        }

        private static void HomeDs402ExPreDispatchArmIsDurableZeroConnection()
        {
            var root = CreateHomeDs402ExTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                window.InitializeAxisDs402HomeExRecoveryForTests();
                AssertEx.True(
                    GetHomeDs402ExPrivateField(window, "connection") == null,
                    "The pre-dispatch fixture unexpectedly has a live connection.");

                var key = CreateHomeDs402ExRecoveryKey();
                var armed = window.ArmAxisDs402HomeExRecoveryKeyForTests(
                    "127.0.0.1",
                    4000,
                    "_LMCAxis2",
                    key,
                    FixedHomeDs402ExUtc());
                AssertEx.Equal(
                    AxisDs402HomeExRecoveryState.ArmedBeforeDispatch,
                    armed.State);
                AssertEx.True(armed.MatchesRecoveryKey(key));
                AssertEx.True(
                    File.Exists(
                        window.AxisDs402HomeExRecoveryJournalForTests
                            .JournalFilePath));
                AssertEx.True(
                    window.AxisDs402HomeExRecoveryInterlockForTests);
                AssertEx.True(
                    GetHomeDs402ExPrivateField(window, "connection") == null,
                    "Durable HomeDS402Ex arm must not create a connection or send Start.");
            }
            finally
            {
                CloseHomeDs402ExWindow(window);
                DeleteHomeDs402ExTemporaryDirectory(root);
            }
        }

        private static void
            HomeDs402ExTerminalRestartRehydratesRetireInputWithoutQuery()
        {
            var root = CreateHomeDs402ExTemporaryDirectory();
            MainWindow window = null;
            try
            {
                var journalDirectory = Path.Combine(
                    root,
                    "AxisDs402HomeExRecovery");
                var key = CreateHomeDs402ExRecoveryKey();
                var proof = CreateHomeDs402ExTerminalProof();
                using (var journal =
                    AxisDs402HomeExRecoveryJournal.Open(journalDirectory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        Guid.NewGuid(),
                        "127.0.0.1",
                        4000,
                        "_LMCAxis2",
                        key,
                        FixedHomeDs402ExUtc());
                    var recovery = journal.PromoteToRecoveryRequired(
                        armed,
                        FixedHomeDs402ExUtc().AddSeconds(1));
                    journal.RecordTerminalOutcomeProof(
                        recovery,
                        key,
                        proof,
                        FixedHomeDs402ExUtc().AddSeconds(2));
                }

                window = new MainWindow(root);
                window.InitializeAxisDs402HomeExRecoveryForTests();
                var recovered =
                    window.ActiveAxisDs402HomeExRecoveryRecordForTests;
                AssertEx.Equal(
                    AxisDs402HomeExRecoveryState.TerminalOutcomeObserved,
                    recovered.State);
                AssertEx.Equal(
                    proof.RecordGeneration,
                    recovered.TerminalOutcomeProof.RecordGeneration);

                var method = typeof(MainWindow).GetMethod(
                    "RehydrateAxisDs402HomeExTerminalOutcome",
                    BindingFlags.Static | BindingFlags.NonPublic);
                AssertEx.NotNull(method);
                var outcome = method.Invoke(
                    null,
                    new object[]
                    {
                        recovered.ToRecoveryKey(),
                        recovered.TerminalOutcomeProof
                    }) as LMCAxisDs402HomeExOutcomeResult;
                AssertEx.NotNull(outcome);
                AssertEx.True(outcome.IsTerminal);
                AssertEx.Equal(
                    LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                    outcome.RecordState);
                AssertEx.Equal(proof.RecordGeneration, outcome.RecordGeneration);
                AssertEx.Equal(100, outcome.ActualPosition);
                AssertEx.Equal(100, outcome.ExpectedFinalPosition);
                AssertEx.Equal(
                    proof.QueryRequestId,
                    outcome.QueryRequestId);
                AssertEx.True(
                    recovered.MatchesRecoveryKey(outcome.RecoveryKey));
            }
            finally
            {
                CloseHomeDs402ExWindow(window);
                DeleteHomeDs402ExTemporaryDirectory(root);
            }
        }

        private static void HomeDs402ExNoStartControlSurface()
        {
            var root = CreateHomeDs402ExTemporaryDirectory();
            MainWindow window = null;
            try
            {
                window = new MainWindow(root);
                window.InitializeAxisDs402HomeExRecoveryForTests();
                var group = window.AxisDs402HomeExRecoveryGroupForTests;
                AssertEx.NotNull(group);
                AssertEx.Contains("Start UI closed", group.Header.ToString());

                foreach (var field in typeof(MainWindow).GetFields(
                    BindingFlags.Instance
                    | BindingFlags.NonPublic
                    | BindingFlags.Public))
                {
                    if (field.FieldType != typeof(Button)
                        || field.Name.IndexOf(
                            "AxisDs402HomeEx",
                            StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    AssertEx.False(
                        field.Name.IndexOf(
                            "Start",
                            StringComparison.OrdinalIgnoreCase) >= 0,
                        "HomeDS402Ex recovery UI must not expose a Start button field.");
                }
            }
            finally
            {
                CloseHomeDs402ExWindow(window);
                DeleteHomeDs402ExTemporaryDirectory(root);
            }
        }

        private static LMCAxisDs402HomeExRecoveryKey
            CreateHomeDs402ExRecoveryKey()
        {
            return LMCAxisDs402HomeExRecovery.Rehydrate(
                1,
                0x10203040U,
                0x11223344U,
                0x55667788U,
                0x99AABBCCU,
                new LMCAxisDs402HomeExClientIntentId(
                    0x01020304U,
                    0x11121314U,
                    0x21222324U,
                    0x31323334U),
                2,
                1,
                -100,
                -4,
                250,
                500,
                25,
                0,
                0,
                LMCDs402HomeBufferMode.Aborting,
                60000,
                5000,
                new byte[LMCAxisDs402HomeExExecutionPlan.SpareLength]);
        }

        private static AxisDs402HomeExTerminalOutcomeProof
            CreateHomeDs402HomeExTerminalProofAlias()
        {
            return CreateHomeDs402ExTerminalProof();
        }

        private static AxisDs402HomeExTerminalOutcomeProof
            CreateHomeDs402ExTerminalProof()
        {
            return new AxisDs402HomeExTerminalOutcomeProof(
                0x44556677U,
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                0,
                0,
                0,
                0x1234,
                100,
                100,
                100,
                110,
                0,
                0x33445566U,
                LMCAxisDs402HomeExCleanupProofFlags.RequiredForSafeTerminal,
                0x1020U,
                -100);
        }

        private static DateTime FixedHomeDs402ExUtc()
        {
            return new DateTime(
                2026,
                8,
                25,
                2,
                30,
                0,
                DateTimeKind.Utc);
        }

        private static string CreateHomeDs402ExTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoHomeDs402ExWpfSmoke",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static object GetHomeDs402ExPrivateField(
            object target,
            string fieldName)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    target.GetType().FullName,
                    fieldName);
            }
            return field.GetValue(target);
        }

        private static void CloseHomeDs402ExWindow(MainWindow window)
        {
            if (window == null)
            {
                return;
            }
            try
            {
                window.Close();
            }
            catch
            {
            }
        }

        private static void DeleteHomeDs402ExTemporaryDirectory(string path)
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

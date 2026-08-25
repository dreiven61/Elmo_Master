using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AxisDs402HomeExRecoveryJournalTests
    {
        private const uint DiagnosticsBuild = 0x01020304U;
        private const uint DiagnosticsBootId = 0x11223344U;
        private const uint MapRevision = 0x55667788U;
        private const uint OriginalRequestId = 0x10203040U;
        private const uint Intent0 = 0x89ABCDEFU;
        private const uint Intent1 = 0x01234567U;
        private const uint Intent2 = 0x76543210U;
        private const uint Intent3 = 0xFEDCBA98U;
        private const uint RecordGeneration = 0x33445566U;
        private const string EndpointIp = "127.0.0.1";
        private const string AxisName = "_LMCAxis2";

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.HomeDS402ExJournal.SurfaceNoReplay",
                SurfaceAndDefaultPathAreStable);
            tests.Add(
                "Wpf.HomeDS402ExJournal.StartupPromotion",
                ArmedRecordBecomesRecoveryRequiredAfterReopen);
            tests.Add(
                "Wpf.HomeDS402ExJournal.TerminalRetire",
                ExactTerminalAndRetirementProofResolve);
            tests.Add(
                "Wpf.HomeDS402ExJournal.KeyMismatch",
                WrongRecoveryKeyCannotChangeDurableBytes);
            tests.Add(
                "Wpf.HomeDS402ExJournal.StaleCopy",
                StaleCopiesCannotAdvanceDurableState);
            tests.Add(
                "Wpf.HomeDS402ExJournal.TerminalValidation",
                NonTerminalOrWeakSuccessProofIsRejected);
            tests.Add(
                "Wpf.HomeDS402ExJournal.Integrity",
                TamperedJournalIsRejected);
        }

        private static void SurfaceAndDefaultPathAreStable()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisDs402HomeExRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                AxisDs402HomeExRecoveryJournal.GetDefaultDirectoryPath()
                    .ToUpperInvariant());
            AssertEx.Equal(
                1,
                (int)AxisDs402HomeExRecoveryState.ArmedBeforeDispatch);
            AssertEx.Equal(
                2,
                (int)AxisDs402HomeExRecoveryState.RecoveryRequired);
            AssertEx.Equal(
                3,
                (int)AxisDs402HomeExRecoveryState.Resolved);
            AssertEx.Equal(
                4,
                (int)AxisDs402HomeExRecoveryState.TerminalOutcomeObserved);

            foreach (var method in typeof(AxisDs402HomeExRecoveryJournal)
                .GetMethods(
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly))
            {
                AssertEx.False(
                    method.Name.IndexOf(
                        "Replay",
                        StringComparison.OrdinalIgnoreCase) >= 0,
                    "The durable HomeDS402Ex journal must not expose replay.");
                AssertEx.False(
                    method.Name.IndexOf(
                        "Start",
                        StringComparison.OrdinalIgnoreCase) >= 0,
                    "The durable HomeDS402Ex journal must not expose a Start sender.");
            }
        }

        private static void ArmedRecordBecomesRecoveryRequiredAfterReopen()
        {
            var directory = CreateTemporaryDirectory();
            var identity = new Guid(
                "00112233-4455-6677-8899-aabbccddeeff");
            try
            {
                using (var journal = AxisDs402HomeExRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        identity,
                        EndpointIp,
                        4000,
                        AxisName,
                        RecoveryKey(),
                        FixedUtc());
                    AssertEx.Equal(
                        AxisDs402HomeExRecoveryState.ArmedBeforeDispatch,
                        armed.State);
                    AssertEx.Equal(1U, armed.Revision);
                    AssertEx.True(armed.IsActive);
                    AssertEx.True(armed.MatchesRecoveryKey(RecoveryKey()));
                    AssertEx.True(File.Exists(journal.JournalFilePath));
                }

                using (var journal = AxisDs402HomeExRecoveryJournal.Open(directory))
                {
                    var recovery = journal.CurrentRecord;
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(identity, recovery.Identity);
                    AssertEx.Equal(
                        AxisDs402HomeExRecoveryState.RecoveryRequired,
                        recovery.State);
                    AssertEx.Equal(2U, recovery.Revision);
                    AssertEx.True(recovery.MatchesRecoveryKey(RecoveryKey()));
                    AssertEx.Equal(-100, recovery.Position);
                    AssertEx.Equal(1, recovery.HomingMethod);
                    AssertEx.Equal(60000U, recovery.OverallTimeoutMilliseconds);
                    AssertEx.Equal(5000U, recovery.DetectionTimeoutMilliseconds);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void ExactTerminalAndRetirementProofResolve()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisDs402HomeExRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal);
                    var recovery = journal.PromoteToRecoveryRequired(
                        armed,
                        FixedUtc().AddSeconds(1));
                    var terminalProof = SuccessfulTerminalProof();
                    var observed = journal.RecordTerminalOutcomeProof(
                        recovery,
                        RecoveryKey(),
                        terminalProof,
                        FixedUtc().AddSeconds(2));
                    AssertEx.Equal(
                        AxisDs402HomeExRecoveryState.TerminalOutcomeObserved,
                        observed.State);
                    AssertEx.True(observed.HasTerminalOutcomeProof);
                    AssertEx.Equal(
                        RecordGeneration,
                        observed.TerminalOutcomeProof.RecordGeneration);
                    AssertEx.True(journal.HasActiveRecord);

                    var resolved = journal.ResolveAfterRetirementProof(
                        observed,
                        RecoveryKey(),
                        0x55667711U,
                        RetirementProofMatching(terminalProof),
                        FixedUtc().AddSeconds(3));
                    AssertEx.Equal(
                        AxisDs402HomeExRecoveryState.Resolved,
                        resolved.State);
                    AssertEx.False(resolved.IsActive);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(0x55667711U, resolved.RetirementRequestId);
                    AssertEx.Equal(
                        terminalProof.QueryRequestId,
                        resolved.TerminalOutcomeProof.QueryRequestId);
                }

                using (var journal = AxisDs402HomeExRecoveryJournal.Open(directory))
                {
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        AxisDs402HomeExRecoveryState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(
                        RecordGeneration,
                        journal.CurrentRecord.TerminalOutcomeProof
                            .RecordGeneration);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void WrongRecoveryKeyCannotChangeDurableBytes()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisDs402HomeExRecoveryJournal.Open(directory))
                {
                    var recovery = journal.PromoteToRecoveryRequired(
                        Arm(journal),
                        FixedUtc().AddSeconds(1));
                    var before = File.ReadAllBytes(journal.JournalFilePath);
                    ExpectThrows<InvalidOperationException>(() =>
                        journal.RecordTerminalOutcomeProof(
                            recovery,
                            RecoveryKey(originalRequestId: OriginalRequestId + 1U),
                            SuccessfulTerminalProof(),
                            FixedUtc().AddSeconds(2)));
                    AssertBytesEqual(
                        before,
                        File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(
                        recovery.Revision,
                        journal.CurrentRecord.Revision);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void StaleCopiesCannotAdvanceDurableState()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisDs402HomeExRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal);
                    var recovery = journal.PromoteToRecoveryRequired(
                        armed,
                        FixedUtc().AddSeconds(1));
                    var before = File.ReadAllBytes(journal.JournalFilePath);
                    ExpectThrows<InvalidOperationException>(() =>
                        journal.RecordTerminalOutcomeProof(
                            armed,
                            RecoveryKey(),
                            SuccessfulTerminalProof(),
                            FixedUtc().AddSeconds(2)));
                    AssertBytesEqual(
                        before,
                        File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(
                        recovery.Revision,
                        journal.CurrentRecord.Revision);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void NonTerminalOrWeakSuccessProofIsRejected()
        {
            ExpectThrows<InvalidOperationException>(() =>
                new AxisDs402HomeExTerminalOutcomeProof(
                    1,
                    LMCAxisDs402HomeExOutcomeRecordState.Running,
                    0,
                    0,
                    0,
                    0x1234,
                    0,
                    0,
                    10,
                    0,
                    0,
                    RecordGeneration,
                    LMCAxisDs402HomeExCleanupProofFlags.None,
                    1,
                    -100));

            ExpectThrows<InvalidOperationException>(() =>
                new AxisDs402HomeExTerminalOutcomeProof(
                    1,
                    LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                    0,
                    0,
                    0,
                    0x1234,
                    99,
                    100,
                    10,
                    20,
                    0,
                    RecordGeneration,
                    LMCAxisDs402HomeExCleanupProofFlags
                        .RequiredForSafeTerminal,
                    1,
                    -100));

            ExpectThrows<InvalidOperationException>(() =>
                new AxisDs402HomeExTerminalOutcomeProof(
                    1,
                    LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                    0,
                    0,
                    0,
                    0x1234,
                    100,
                    100,
                    10,
                    20,
                    0,
                    RecordGeneration,
                    LMCAxisDs402HomeExCleanupProofFlags.StartBitLow,
                    1,
                    -100));

            var good = SuccessfulTerminalProof();
            AssertEx.Equal(
                LMCAxisDs402HomeExOutcomeRecordState.Succeeded,
                good.RecordState);
        }

        private static void TamperedJournalIsRejected()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                string path;
                using (var journal = AxisDs402HomeExRecoveryJournal.Open(directory))
                {
                    Arm(journal);
                    path = journal.JournalFilePath;
                }

                var bytes = File.ReadAllBytes(path);
                var changed = false;
                for (var index = 0; index < bytes.Length; index++)
                {
                    if (bytes[index] == (byte)'8')
                    {
                        bytes[index] = (byte)'7';
                        changed = true;
                        break;
                    }
                }
                AssertEx.True(changed, "Fixture must contain a mutable digit.");
                File.WriteAllBytes(path, bytes);

                ExpectThrows<InvalidDataException>(() =>
                {
                    using (AxisDs402HomeExRecoveryJournal.Open(directory))
                    {
                    }
                });
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static AxisDs402HomeExRecoveryRecord Arm(
            AxisDs402HomeExRecoveryJournal journal)
        {
            return journal.ArmBeforeDispatch(
                Guid.NewGuid(),
                EndpointIp,
                4000,
                AxisName,
                RecoveryKey(),
                FixedUtc());
        }

        private static LMCAxisDs402HomeExRecoveryKey RecoveryKey(
            uint originalRequestId = OriginalRequestId)
        {
            return LMCAxisDs402HomeExRecovery.Rehydrate(
                1,
                originalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCAxisDs402HomeExClientIntentId(
                    Intent0,
                    Intent1,
                    Intent2,
                    Intent3),
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
            SuccessfulTerminalProof()
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
                RecordGeneration,
                LMCAxisDs402HomeExCleanupProofFlags.RequiredForSafeTerminal,
                0x1020U,
                -100);
        }

        private static AxisDs402HomeExTerminalOutcomeProof
            RetirementProofMatching(
                AxisDs402HomeExTerminalOutcomeProof terminal)
        {
            return new AxisDs402HomeExTerminalOutcomeProof(
                0,
                terminal.RecordState,
                terminal.OriginalCommandStatus,
                terminal.OriginalErrorId,
                terminal.OriginalDetailCode,
                terminal.Ds402StatusWord,
                terminal.ActualPosition,
                terminal.ExpectedFinalPosition,
                terminal.StartCycle,
                terminal.CompletionCycle,
                terminal.NativeCommandState,
                terminal.RecordGeneration,
                terminal.CleanupProofFlags,
                terminal.SdoExecutorToken,
                -100);
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(638916480000000000L, DateTimeKind.Utc);
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoHomeDs402ExJournalTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteTemporaryDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void AssertBytesEqual(byte[] expected, byte[] actual)
        {
            AssertEx.Equal(expected.Length, actual.Length);
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], actual[index]);
            }
        }

        private static T ExpectThrows<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T error)
            {
                return error;
            }

            throw new Exception(
                "Expected exception " + typeof(T).FullName + ".");
        }
    }
}

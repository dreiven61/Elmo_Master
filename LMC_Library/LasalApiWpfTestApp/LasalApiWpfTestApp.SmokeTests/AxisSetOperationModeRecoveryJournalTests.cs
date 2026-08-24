using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AxisSetOperationModeRecoveryJournalTests
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
        private const string AxisName = "_LMCAxis1";

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("Wpf.AxisSetOperationModeJournal.SurfaceNoReplay", SurfaceAndDefaultPathAreStable);
            tests.Add("Wpf.AxisSetOperationModeJournal.StartupPromotion", ArmedRecordBecomesRecoveryRequiredAfterReopen);
            tests.Add("Wpf.AxisSetOperationModeJournal.TerminalRetire", ExactTerminalAndRetirementProofResolve);
            tests.Add("Wpf.AxisSetOperationModeJournal.KeyMismatch", WrongRecoveryKeyCannotChangeDurableBytes);
            tests.Add("Wpf.AxisSetOperationModeJournal.StaleCopy", StaleCopiesCannotAdvanceDurableState);
            tests.Add("Wpf.AxisSetOperationModeJournal.TerminalValidation", NonTerminalOrWeakSuccessProofIsRejected);
            tests.Add("Wpf.AxisSetOperationModeJournal.Integrity", TamperedJournalIsRejected);
        }

        private static void SurfaceAndDefaultPathAreStable()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisSetOperationModeRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                AxisSetOperationModeRecoveryJournal.GetDefaultDirectoryPath().ToUpperInvariant());
            AssertEx.Equal(1, (int)AxisSetOperationModeRecoveryState.ArmedBeforeDispatch);
            AssertEx.Equal(2, (int)AxisSetOperationModeRecoveryState.RecoveryRequired);
            AssertEx.Equal(3, (int)AxisSetOperationModeRecoveryState.Resolved);
            AssertEx.Equal(4, (int)AxisSetOperationModeRecoveryState.TerminalOutcomeObserved);

            foreach (var method in typeof(AxisSetOperationModeRecoveryJournal).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly))
            {
                AssertEx.False(
                    method.Name.IndexOf("Replay", StringComparison.OrdinalIgnoreCase) >= 0,
                    "The durable SetOperationMode journal must not expose a replay surface.");
                AssertEx.False(
                    method.Name.IndexOf("Start", StringComparison.OrdinalIgnoreCase) >= 0,
                    "The durable SetOperationMode journal must not expose a Start sender.");
            }
        }

        private static void ArmedRecordBecomesRecoveryRequiredAfterReopen()
        {
            var directory = CreateTemporaryDirectory();
            var identity = new Guid("00112233-4455-6677-8899-aabbccddeeff");
            try
            {
                AxisSetOperationModeRecoveryRecord armed;
                using (var journal = AxisSetOperationModeRecoveryJournal.Open(directory))
                {
                    armed = journal.ArmBeforeDispatch(
                        identity,
                        EndpointIp,
                        4000,
                        AxisName,
                        RecoveryKey(),
                        FixedUtc());
                    AssertEx.Equal(AxisSetOperationModeRecoveryState.ArmedBeforeDispatch, armed.State);
                    AssertEx.Equal(1U, armed.Revision);
                    AssertEx.True(armed.IsActive);
                    AssertEx.True(armed.MatchesRecoveryKey(RecoveryKey()));
                    AssertEx.True(File.Exists(journal.JournalFilePath));
                }

                using (var journal = AxisSetOperationModeRecoveryJournal.Open(directory))
                {
                    var recovery = journal.CurrentRecord;
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(identity, recovery.Identity);
                    AssertEx.Equal(AxisSetOperationModeRecoveryState.RecoveryRequired, recovery.State);
                    AssertEx.Equal(2U, recovery.Revision);
                    AssertEx.True(recovery.MatchesRecoveryKey(RecoveryKey()));
                    AssertEx.Equal((sbyte)8, recovery.RequestedModeRaw);
                    AssertEx.Equal(0U, recovery.Flags);
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
                using (var journal = AxisSetOperationModeRecoveryJournal.Open(directory))
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
                        AxisSetOperationModeRecoveryState.TerminalOutcomeObserved,
                        observed.State);
                    AssertEx.True(observed.HasTerminalOutcomeProof);
                    AssertEx.Equal(RecordGeneration, observed.TerminalOutcomeProof.RecordGeneration);
                    AssertEx.True(journal.HasActiveRecord);

                    var resolved = journal.ResolveAfterRetirementProof(
                        observed,
                        RecoveryKey(),
                        0x55667711U,
                        RetirementProofMatching(terminalProof),
                        FixedUtc().AddSeconds(3));
                    AssertEx.Equal(AxisSetOperationModeRecoveryState.Resolved, resolved.State);
                    AssertEx.False(resolved.IsActive);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(0x55667711U, resolved.RetirementRequestId);
                    AssertEx.Equal(terminalProof.QueryRequestId,
                        resolved.TerminalOutcomeProof.QueryRequestId);
                }

                using (var journal = AxisSetOperationModeRecoveryJournal.Open(directory))
                {
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(AxisSetOperationModeRecoveryState.Resolved,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(RecordGeneration,
                        journal.CurrentRecord.TerminalOutcomeProof.RecordGeneration);
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
                using (var journal = AxisSetOperationModeRecoveryJournal.Open(directory))
                {
                    var recovery = journal.PromoteToRecoveryRequired(
                        Arm(journal),
                        FixedUtc().AddSeconds(1));
                    var before = File.ReadAllBytes(journal.JournalFilePath);
                    ExpectThrows<InvalidOperationException>(() =>
                        journal.RecordTerminalOutcomeProof(
                            recovery,
                            RecoveryKey(OriginalRequestId + 1U),
                            SuccessfulTerminalProof(),
                            FixedUtc().AddSeconds(2)));
                    AssertBytesEqual(before, File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(recovery.Revision, journal.CurrentRecord.Revision);
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
                using (var journal = AxisSetOperationModeRecoveryJournal.Open(directory))
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
                    AssertBytesEqual(before, File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(recovery.Revision, journal.CurrentRecord.Revision);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void NonTerminalOrWeakSuccessProofIsRejected()
        {
            var terminalEvidence =
                LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable;
            ExpectThrows<InvalidOperationException>(() =>
                new AxisSetOperationModeTerminalOutcomeProof(
                    1,
                    LMCAxisSetOperationModeOutcomeRecordState.Running,
                    8,
                    0,
                    0,
                    0,
                    1,
                    terminalEvidence,
                    10,
                    11,
                    0,
                    RecordGeneration,
                    8,
                    0,
                    0x1234,
                    0xABCDEF01,
                    8));

            ExpectThrows<InvalidOperationException>(() =>
                new AxisSetOperationModeTerminalOutcomeProof(
                    1,
                    LMCAxisSetOperationModeOutcomeRecordState.Succeeded,
                    8,
                    0,
                    0,
                    0,
                    1,
                    terminalEvidence,
                    10,
                    11,
                    0,
                    RecordGeneration,
                    8,
                    0,
                    0x1234,
                    0xABCDEF01,
                    8));

            var good = SuccessfulTerminalProof();
            AssertEx.Equal(LMCAxisSetOperationModeOutcomeRecordState.Succeeded, good.RecordState);
        }

        private static void TamperedJournalIsRejected()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                string path;
                using (var journal = AxisSetOperationModeRecoveryJournal.Open(directory))
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
                    using (AxisSetOperationModeRecoveryJournal.Open(directory))
                    {
                    }
                });
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static AxisSetOperationModeRecoveryRecord Arm(
            AxisSetOperationModeRecoveryJournal journal)
        {
            return journal.ArmBeforeDispatch(
                Guid.NewGuid(),
                EndpointIp,
                4000,
                AxisName,
                RecoveryKey(),
                FixedUtc());
        }

        private static LMCAxisSetOperationModeRecoveryKey RecoveryKey(
            uint originalRequestId = OriginalRequestId)
        {
            return new LMCAxisSetOperationModeRecoveryKey(
                1,
                originalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                Intent0,
                Intent1,
                Intent2,
                Intent3,
                1,
                (LMCDriveOperationMode)8,
                5000);
        }

        private static AxisSetOperationModeTerminalOutcomeProof SuccessfulTerminalProof()
        {
            return new AxisSetOperationModeTerminalOutcomeProof(
                0x44556677U,
                LMCAxisSetOperationModeOutcomeRecordState.Succeeded,
                8,
                0,
                0,
                0,
                0x1020U,
                LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadCompleted
                    | LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                    | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable,
                100,
                110,
                0,
                RecordGeneration,
                8,
                0,
                0x1234,
                0xAABBCCDD,
                8);
        }

        private static AxisSetOperationModeTerminalOutcomeProof RetirementProofMatching(
            AxisSetOperationModeTerminalOutcomeProof terminal)
        {
            return new AxisSetOperationModeTerminalOutcomeProof(
                0,
                terminal.RecordState,
                terminal.ObservedModeRaw,
                terminal.OriginalCommandStatus,
                terminal.OriginalErrorId,
                terminal.OriginalDetailCode,
                terminal.SdoExecutorToken,
                terminal.EvidenceFlags,
                terminal.StartCycle,
                terminal.CompletionCycle,
                terminal.NativeCommandState,
                terminal.RecordGeneration,
                terminal.PreviousModeRaw,
                terminal.QuarantineReason,
                terminal.Ds402StatusWord,
                terminal.ContextCheck,
                8);
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(638916480000000000L, DateTimeKind.Utc);
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoSetOperationModeJournalTests",
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

            throw new Exception("Expected exception " + typeof(T).FullName + ".");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AxisSetPositionRecoveryJournalTests
    {
        private const int MaximumFileLength = 8192;
        private const int ChecksumLength = 32;
        private const uint DiagnosticsBuild = 0x01020304U;
        private const uint OriginalDiagnosticsBootId = 0x11223344U;
        private const uint CurrentDiagnosticsBootId = 0x99AABBCCU;
        private const uint MapRevision = 0x55667788U;
        private const uint Intent0 = 0x89ABCDEFU;
        private const uint Intent1 = 0x01234567U;
        private const uint Intent2 = 0x76543210U;
        private const uint Intent3 = 0xFEDCBA98U;
        private const uint OriginalRequestId = 0x10203040U;
        private const int TargetPosition = -1234567;
        private const int ExpectedActualPosition = 7654321;
        private const uint RecordGeneration = 0x33445566U;
        private const string EndpointIp = "127.0.0.1";
        private const string AxisName = "_LMCAxis1";

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("Wpf.AxisSetPositionJournal.DefaultPathAndSurface", DefaultPathMagicStatesAndSurfaceAreStable);
            tests.Add("Wpf.AxisSetPositionJournal.V2EvidenceLifecycle", V2EvidenceLifecyclePersistsAcrossReopen);
            tests.Add("Wpf.AxisSetPositionJournal.V1Compatibility", V1ActiveAndResolvedRecordsRemainCompatible);
            tests.Add("Wpf.AxisSetPositionJournal.EvidenceMismatch", WrongEvidenceCannotChangeDurableBytes);
            tests.Add("Wpf.AxisSetPositionJournal.FailedRpcResponse", FailedRpcResponsesCannotResolveTheJournal);
            tests.Add("Wpf.AxisSetPositionJournal.CasStaleCopy", StaleCopiesCannotAdvanceTheJournal);
            tests.Add("Wpf.AxisSetPositionJournal.Integrity", ChecksumTrailingAndProofTamperAreRejected);
            tests.Add("Wpf.AxisSetPositionJournal.WriteFailurePreservesBytes", FailedAtomicReplacementPreservesExactBytes);
            tests.Add("Wpf.AxisSetPositionJournal.StartupPromotionFailure", FailedStartupPromotionFailsOpenAndPreservesBytes);
            tests.Add("Wpf.AxisSetPositionJournal.Bounds", InvalidRecordAndFileBoundsAreRejected);
            tests.Add("Wpf.AxisSetPositionJournal.DeterministicBytes", SerializationIsDeterministic);
        }

        private static void DefaultPathMagicStatesAndSurfaceAreStable()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Elmo", "LasalMotionControlApiExample",
                "AxisSetPositionRecoveryJournal", "v1");
            AssertEx.Equal(expected.ToUpperInvariant(),
                AxisSetPositionRecoveryJournal.GetDefaultDirectoryPath().ToUpperInvariant());
            AssertEx.Equal(1, (int)AxisSetPositionRecoveryState.ArmedBeforeDispatch);
            AssertEx.Equal(2, (int)AxisSetPositionRecoveryState.RecoveryRequired);
            AssertEx.Equal(3, (int)AxisSetPositionRecoveryState.Resolved);
            AssertEx.Equal(4, (int)AxisSetPositionRecoveryState.TerminalOutcomeObserved);
            AssertEx.True(typeof(AxisSetPositionRecoveryJournal).GetMethod(
                    "Resolve", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) == null,
                "Evidence-free AxisSetPositionRecoveryJournal.Resolve must not exist.");

            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    Arm(journal, Guid.NewGuid());
                    var bytes = File.ReadAllBytes(journal.JournalFilePath);
                    AssertEx.Equal("ELMOASP1", Encoding.ASCII.GetString(bytes, 0, 8));
                    AssertEx.Equal(2U, ReadFormatVersion(bytes));
                    AssertEx.Equal(AxisSetPositionRecoveryRecord.CurrentStorageFormatVersion,
                        journal.CurrentRecord.StorageFormatVersion);
                }
            }
            finally { DeleteTemporaryDirectory(directory); }
        }

        private static void V2EvidenceLifecyclePersistsAcrossReopen()
        {
            var key = RecoveryKey();
            var directory = CreateTemporaryDirectory();
            var armedDirectory = CreateTemporaryDirectory();
            var identity = new Guid("00112233-4455-6677-8899-aabbccddeeff");
            LMCAxisSetPositionOutcomeResult outcome;
            LMCAxisSetPositionOutcomeRetirementResult retirement;
            try
            {
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, identity);
                    AssertExactIntent(armed, identity);
                    AssertEx.Equal(AxisSetPositionRecoveryState.ArmedBeforeDispatch, armed.State);
                    AssertEx.True(armed.IsActive);
                    AssertEx.False(armed.HasTerminalOutcomeProof);
                    AssertEx.True(armed.MatchesRecoveryIdentity(EndpointIp, 4000,
                        DiagnosticsBuild, OriginalDiagnosticsBootId, MapRevision,
                        AxisName, 1));
                    AssertEx.False(armed.MatchesRecoveryIdentity(EndpointIp, 4000,
                        DiagnosticsBuild + 1U, OriginalDiagnosticsBootId, MapRevision,
                        AxisName, 1));
                    AssertEx.True(armed.MatchesIntent(Intent0, Intent1, Intent2,
                        Intent3, OriginalRequestId, TargetPosition,
                        ExpectedActualPosition, 1, 1));
                    AssertEx.False(armed.MatchesIntent(Intent0, Intent1, Intent2,
                        Intent3, OriginalRequestId + 1U, TargetPosition,
                        ExpectedActualPosition, 1, 1));
                    var recovery = journal.PromoteToRecoveryRequired(armed, FixedUtc().AddSeconds(1));
                    outcome = ReadPublicOutcome(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    var observed = journal.RecordTerminalOutcome(
                        recovery, outcome, FixedUtc().AddSeconds(2));
                    AssertEx.Equal(AxisSetPositionRecoveryState.TerminalOutcomeObserved, observed.State);
                    AssertEx.True(observed.IsActive);
                    AssertTerminalProof(observed, outcome);
                }
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var observed = journal.CurrentRecord;
                    AssertEx.True(journal.HasActiveRecord);
                    AssertExactIntent(observed, identity);
                    AssertTerminalProof(observed, outcome);
                    retirement = RetirePublicEvidence(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        observed.TerminalOutcomeProof.RecordGeneration);
                    var resolved = journal.ResolveAfterRetirement(
                        observed, retirement, FixedUtc().AddSeconds(3));
                    AssertEx.Equal(AxisSetPositionRecoveryState.Resolved, resolved.State);
                    AssertEx.False(resolved.IsActive);
                    AssertEx.Equal(retirement.RetireRequestId, resolved.RetirementRequestId);
                    AssertTerminalProof(resolved, outcome);
                }
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var resolved = journal.CurrentRecord;
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(AxisSetPositionRecoveryState.Resolved, resolved.State);
                    AssertEx.Equal(retirement.RetireRequestId, resolved.RetirementRequestId);
                    AssertTerminalProof(resolved, outcome);
                    AssertEx.Equal(2U, ReadFormatVersion(File.ReadAllBytes(journal.JournalFilePath)));
                }

                using (var journal = AxisSetPositionRecoveryJournal.Open(armedDirectory))
                {
                    var armed = Arm(journal, Guid.NewGuid());
                    var armedOutcome = ReadPublicOutcome(RecoveryKey(),
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    var observed = journal.RecordTerminalOutcome(
                        armed, armedOutcome, FixedUtc().AddSeconds(1));
                    AssertEx.Equal(AxisSetPositionRecoveryState.TerminalOutcomeObserved,
                        observed.State);
                    AssertTerminalProof(observed, armedOutcome);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
                DeleteTemporaryDirectory(armedDirectory);
            }
        }

        private static void V1ActiveAndResolvedRecordsRemainCompatible()
        {
            var key = RecoveryKey();
            var activeDirectory = CreateTemporaryDirectory();
            var resolvedDirectory = CreateTemporaryDirectory();
            var armedDirectory = CreateTemporaryDirectory();
            try
            {
                WriteLegacyJournal(activeDirectory, Guid.NewGuid(),
                    AxisSetPositionRecoveryState.RecoveryRequired, FixedUtc(), FixedUtc());
                using (var journal = AxisSetPositionRecoveryJournal.Open(activeDirectory))
                {
                    var active = journal.CurrentRecord;
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(AxisSetPositionRecoveryRecord.LegacyStorageFormatVersion,
                        active.StorageFormatVersion);
                    AssertEx.False(active.HasTerminalOutcomeProof);
                    var before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved<InvalidOperationException>(journal, before,
                        () => journal.ResolveAfterRetirement(
                            active, null, FixedUtc().AddSeconds(1)));
                    var outcome = ReadPublicOutcome(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    var observed = journal.RecordTerminalOutcome(
                        active, outcome, FixedUtc().AddSeconds(1));
                    AssertEx.Equal(AxisSetPositionRecoveryRecord.CurrentStorageFormatVersion,
                        observed.StorageFormatVersion);
                    AssertTerminalProof(observed, outcome);
                    AssertEx.Equal(2U, ReadFormatVersion(File.ReadAllBytes(journal.JournalFilePath)));
                }
                using (var journal = AxisSetPositionRecoveryJournal.Open(activeDirectory))
                {
                    var observed = journal.CurrentRecord;
                    var retirement = RetirePublicEvidence(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        observed.TerminalOutcomeProof.RecordGeneration);
                    var resolved = journal.ResolveAfterRetirement(
                        observed, retirement, FixedUtc().AddSeconds(2));
                    AssertEx.Equal(AxisSetPositionRecoveryState.Resolved, resolved.State);
                }

                WriteLegacyJournal(resolvedDirectory, Guid.NewGuid(),
                    AxisSetPositionRecoveryState.Resolved, FixedUtc(), FixedUtc().AddSeconds(1));
                using (var journal = AxisSetPositionRecoveryJournal.Open(resolvedDirectory))
                {
                    var resolved = journal.CurrentRecord;
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(AxisSetPositionRecoveryState.Resolved, resolved.State);
                    AssertEx.Equal(AxisSetPositionRecoveryRecord.LegacyStorageFormatVersion,
                        resolved.StorageFormatVersion);
                    AssertEx.False(resolved.HasTerminalOutcomeProof);
                    AssertEx.Equal(0U, resolved.RetirementRequestId);
                    AssertEx.Equal(1U, ReadFormatVersion(File.ReadAllBytes(journal.JournalFilePath)));
                }

                WriteLegacyJournal(armedDirectory, Guid.NewGuid(),
                    AxisSetPositionRecoveryState.ArmedBeforeDispatch, FixedUtc(), FixedUtc());
                using (var journal = AxisSetPositionRecoveryJournal.Open(armedDirectory))
                {
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(AxisSetPositionRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);
                    AssertEx.Equal(AxisSetPositionRecoveryRecord.CurrentStorageFormatVersion,
                        journal.CurrentRecord.StorageFormatVersion);
                    AssertEx.Equal(2U, ReadFormatVersion(File.ReadAllBytes(journal.JournalFilePath)));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(activeDirectory);
                DeleteTemporaryDirectory(resolvedDirectory);
                DeleteTemporaryDirectory(armedDirectory);
            }
        }

        private static void WrongEvidenceCannotChangeDurableBytes()
        {
            var key = RecoveryKey();
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var recovery = journal.PromoteToRecoveryRequired(
                        Arm(journal, Guid.NewGuid()), FixedUtc().AddSeconds(1));
                    var correctOutcome = ReadPublicOutcome(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    var wrongKey = RecoveryKey(OriginalRequestId + 1U);
                    var wrongKeyOutcome = ReadPublicOutcome(wrongKey,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    var before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved<InvalidOperationException>(journal, before,
                        () => journal.RecordTerminalOutcome(
                            recovery, wrongKeyOutcome, FixedUtc().AddSeconds(2)));
                    AssertOutcomeRecoveryKeyFieldMatrixRejected(
                        journal, recovery, correctOutcome, before);

                    var impossibleOutcome = ReadPublicOutcome(key,
                        LMCAxisSetPositionOutcomeRecordState.Rejected,
                        RecordGeneration,
                        LMCAdminDetailCode.CoordinatePreconditionFailed);
                    foreach (var impossibleDetail in new[]
                    {
                        LMCAdminDetailCode.UnsupportedSchema,
                        LMCAdminDetailCode.UnsupportedFlags,
                        LMCAdminDetailCode.InvalidRequestId,
                        LMCAdminDetailCode.InvalidReference,
                        LMCAdminDetailCode.InvalidPayloadLength,
                        LMCAdminDetailCode.UnsupportedParameter,
                        LMCAdminDetailCode.MissingClient,
                        LMCAdminDetailCode.InvalidSelection,
                        LMCAdminDetailCode.InvalidMotionParameters,
                        LMCAdminDetailCode.DiagnosticsBuildMismatch,
                        LMCAdminDetailCode.BootIdMismatch,
                        LMCAdminDetailCode.MapRevisionMismatch,
                        LMCAdminDetailCode.SetPositionOutcomeSlotOccupied,
                        LMCAdminDetailCode.SetPositionOutcomeStorageUnavailable
                    })
                    {
                        SetPrivateProperty(
                            impossibleOutcome,
                            "OriginalDetailCodeValue",
                            (uint)impossibleDetail);
                        var error = AssertEx.Throws<ArgumentException>(() =>
                            journal.RecordTerminalOutcome(
                                recovery,
                                impossibleOutcome,
                                FixedUtc().AddSeconds(2)));
                        AssertEx.Equal(typeof(ArgumentException), error.GetType());
                        AssertBytesEqual(
                            before,
                            File.ReadAllBytes(journal.JournalFilePath));
                        AssertEx.Equal(
                            AxisSetPositionRecoveryState.RecoveryRequired,
                            journal.CurrentRecord.State);
                    }
                    var observed = journal.RecordTerminalOutcome(
                        recovery, correctOutcome, FixedUtc().AddSeconds(2));
                    var wrongKeyRetirement = RetirePublicEvidence(wrongKey,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    var correctRetirement = RetirePublicEvidence(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved<InvalidOperationException>(journal, before,
                        () => journal.ResolveAfterRetirement(
                            observed, wrongKeyRetirement, FixedUtc().AddSeconds(3)));
                    AssertRetirementRecoveryKeyFieldMatrixRejected(
                        journal, observed, correctRetirement, before);
                    AssertRetirementSnapshotFieldMatrixRejected(
                        journal, observed, correctRetirement, before);
                    AssertEx.Equal(AxisSetPositionRecoveryState.TerminalOutcomeObserved,
                        journal.CurrentRecord.State);
                }
            }
            finally { DeleteTemporaryDirectory(directory); }
        }

        private static void FailedRpcResponsesCannotResolveTheJournal()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var recovery = journal.PromoteToRecoveryRequired(
                        Arm(journal, Guid.NewGuid()), FixedUtc().AddSeconds(1));
                    var before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPublicQueryFailure(RecoveryKey());
                    AssertBytesEqual(before, File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(AxisSetPositionRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);

                    var outcome = ReadPublicOutcome(RecoveryKey(),
                        LMCAxisSetPositionOutcomeRecordState.Succeeded, RecordGeneration);
                    var observed = journal.RecordTerminalOutcome(
                        recovery, outcome, FixedUtc().AddSeconds(2));
                    before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPublicRetirementFailure(RecoveryKey(), RecordGeneration);
                    AssertBytesEqual(before, File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(AxisSetPositionRecoveryState.TerminalOutcomeObserved,
                        journal.CurrentRecord.State);
                    AssertTerminalProof(observed, outcome);
                }
            }
            finally { DeleteTemporaryDirectory(directory); }
        }

        private static void StaleCopiesCannotAdvanceTheJournal()
        {
            var key = RecoveryKey();
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, Guid.NewGuid());
                    var staleArmed = armed.Copy();
                    var recovery = journal.PromoteToRecoveryRequired(armed, FixedUtc().AddSeconds(1));
                    var staleRecovery = recovery.Copy();
                    var outcome = ReadPublicOutcome(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    var before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved<InvalidOperationException>(journal, before,
                        () => journal.RecordTerminalOutcome(
                            staleArmed, outcome, FixedUtc().AddSeconds(2)));
                    var observed = journal.RecordTerminalOutcome(
                        recovery, outcome, FixedUtc().AddSeconds(2));
                    var retirement = RetirePublicEvidence(key,
                        LMCAxisSetPositionOutcomeRecordState.Succeeded,
                        RecordGeneration);
                    before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved<InvalidOperationException>(journal, before,
                        () => journal.ResolveAfterRetirement(
                            staleRecovery, retirement, FixedUtc().AddSeconds(3)));
                    var staleObserved = observed.Copy();
                    journal.ResolveAfterRetirement(
                        observed, retirement, FixedUtc().AddSeconds(3));
                    before = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved<InvalidOperationException>(journal, before,
                        () => journal.ResolveAfterRetirement(
                            staleObserved, retirement, FixedUtc().AddSeconds(4)));
                }
            }
            finally { DeleteTemporaryDirectory(directory); }
        }

        private static void ChecksumTrailingAndProofTamperAreRejected()
        {
            var outcome = ReadPublicOutcome(RecoveryKey(),
                LMCAxisSetPositionOutcomeRecordState.Succeeded, RecordGeneration);
            var source = CreateTemporaryDirectory();
            var checksumDir = CreateTemporaryDirectory();
            var trailingDir = CreateTemporaryDirectory();
            var proofDir = CreateTemporaryDirectory();
            var versionDir = CreateTemporaryDirectory();
            try
            {
                byte[] original;
                using (var journal = AxisSetPositionRecoveryJournal.Open(source))
                {
                    AssertEx.Throws<IOException>(() =>
                        AxisSetPositionRecoveryJournal.Open(source));
                    var recovery = journal.PromoteToRecoveryRequired(
                        Arm(journal, Guid.NewGuid()), FixedUtc().AddSeconds(1));
                    journal.RecordTerminalOutcome(recovery, outcome, FixedUtc().AddSeconds(2));
                    original = File.ReadAllBytes(journal.JournalFilePath);
                }
                var checksum = (byte[])original.Clone();
                checksum[checksum.Length - 1] ^= 0x40;
                WriteJournalBytes(checksumDir, checksum);
                AssertOpenInvalid(checksumDir);

                WriteJournalBytes(trailingDir, AppendTrailingPayloadByte(original, 0x5A));
                AssertOpenInvalid(trailingDir);

                var proof = (byte[])original.Clone();
                var marker = FindV2ProofMarkerOffset(proof);
                AssertEx.Equal((byte)1, proof[marker]);
                TestFrame.WriteUInt32(proof, marker + 23, 0);
                RecomputeChecksum(proof);
                WriteJournalBytes(proofDir, proof);
                AssertOpenInvalid(proofDir);

                var version = (byte[])original.Clone();
                TestFrame.WriteInt32(version, 8, 3);
                RecomputeChecksum(version);
                WriteJournalBytes(versionDir, version);
                AssertOpenInvalid(versionDir);
            }
            finally
            {
                DeleteTemporaryDirectory(source);
                DeleteTemporaryDirectory(checksumDir);
                DeleteTemporaryDirectory(trailingDir);
                DeleteTemporaryDirectory(proofDir);
                DeleteTemporaryDirectory(versionDir);
            }
        }

        private static void FailedAtomicReplacementPreservesExactBytes()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, Guid.NewGuid());
                    var bytes = File.ReadAllBytes(journal.JournalFilePath);
                    using (var blocker = new FileStream(journal.JournalFilePath,
                        FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        AssertEx.Throws<IOException>(() => journal.PromoteToRecoveryRequired(
                            armed, FixedUtc().AddSeconds(1)));
                    }
                    AssertEx.Equal(AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                        journal.CurrentRecord.State);
                    AssertBytesEqual(bytes, File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(0, Directory.GetFiles(directory, "*.tmp").Length);
                }
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                    AssertEx.Equal(AxisSetPositionRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);
            }
            finally { DeleteTemporaryDirectory(directory); }
        }

        private static void FailedStartupPromotionFailsOpenAndPreservesBytes()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                string path;
                byte[] bytes;
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                {
                    Arm(journal, Guid.NewGuid());
                    path = journal.JournalFilePath;
                    bytes = File.ReadAllBytes(path);
                }
                using (var blocker = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    AssertEx.Throws<IOException>(() => AxisSetPositionRecoveryJournal.Open(directory));
                AssertBytesEqual(bytes, File.ReadAllBytes(path));
                AssertEx.Equal(0, Directory.GetFiles(directory, "*.tmp").Length);
                using (var journal = AxisSetPositionRecoveryJournal.Open(directory))
                    AssertEx.Equal(AxisSetPositionRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);
            }
            finally { DeleteTemporaryDirectory(directory); }
        }

        private static void InvalidRecordAndFileBoundsAreRejected()
        {
            var now = FixedUtc();
            AssertEx.Throws<ArgumentException>(() => CreateRecord(Guid.Empty, 1, 1, 1, now, now));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(Guid.NewGuid(), 0, 1, 1, now, now));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(Guid.NewGuid(), 1, 0, 1, now, now));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(Guid.NewGuid(), 1, 1, 2, now, now));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(
                Guid.NewGuid(), 1, 1, 1, now, now.AddTicks(-1)));
            AssertEx.Throws<ArgumentException>(() => new AxisSetPositionRecoveryRecord(
                Guid.NewGuid(), EndpointIp, 4000, 1, 2, 3, "Axis\uD55C", 1,
                1, 2, 3, 4, 5, 10, 20, 1, 1,
                AxisSetPositionRecoveryState.ArmedBeforeDispatch, now, now));
            AssertEx.Throws<ArgumentException>(() => new AxisSetPositionRecoveryRecord(
                Guid.NewGuid(), EndpointIp, 4000, 1, 2, 3, AxisName, 1,
                1, 2, 3, 4, 5, 10, 20, 1, 1,
                AxisSetPositionRecoveryState.Resolved, now, now));
            var directory = CreateTemporaryDirectory();
            try
            {
                WriteJournalBytes(directory, new byte[MaximumFileLength + 1]);
                AssertOpenInvalid(directory);
            }
            finally { DeleteTemporaryDirectory(directory); }
        }

        private static void SerializationIsDeterministic()
        {
            var a = CreateTemporaryDirectory();
            var b = CreateTemporaryDirectory();
            try
            {
                var id = new Guid("fedcba98-7654-3210-fedc-ba9876543210");
                byte[] bytesA;
                byte[] bytesB;
                using (var journal = AxisSetPositionRecoveryJournal.Open(a))
                {
                    Arm(journal, id);
                    bytesA = File.ReadAllBytes(journal.JournalFilePath);
                }
                using (var journal = AxisSetPositionRecoveryJournal.Open(b))
                {
                    Arm(journal, id);
                    bytesB = File.ReadAllBytes(journal.JournalFilePath);
                }
                AssertBytesEqual(bytesA, bytesB);
            }
            finally { DeleteTemporaryDirectory(a); DeleteTemporaryDirectory(b); }
        }

        private static LMCAxisSetPositionOutcomeResult ReadPublicOutcome(
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            uint generation,
            LMCAdminDetailCode detail = LMCAdminDetailCode.None)
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), AxisLookupStep(), AxisInfoStep(),
                AdminCapabilitiesStep(1), DiagnosticsCapabilitiesStep(1),
                OutcomeStep(2, key, state, generation, detail), CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, AxisName);
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var outcome = axis.ReadSetPositionOutcome(key, admin, diagnostics);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D14));
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
                return outcome;
            }
        }

        private static LMCAxisSetPositionOutcomeRetirementResult RetirePublicEvidence(
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            uint generation,
            LMCAdminDetailCode detail = LMCAdminDetailCode.None)
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), AxisLookupStep(), AxisInfoStep(),
                AdminCapabilitiesStep(1), DiagnosticsCapabilitiesStep(1),
                RetirementStep(2, key, state, generation, detail), CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, AxisName);
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                var retirement = axis.RetireSetPositionOutcome(
                    key, generation, admin, diagnostics);
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(1, CountCommand(server, 0x7D1A));
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
                return retirement;
            }
        }

        private static void AssertPublicQueryFailure(
            LMCAxisSetPositionRecoveryKey key)
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), AxisLookupStep(), AxisInfoStep(),
                AdminCapabilitiesStep(1), DiagnosticsCapabilitiesStep(1),
                new FakeRpcStep(0x7D14, TestFrame.Response(0,
                    FailurePayload(2, LMCAdminDetailCode.SetPositionOutcomeNotFound))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, AxisName);
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                AssertEx.Throws<LMCAxisSetPositionOutcomeQueryException>(() =>
                    axis.ReadSetPositionOutcome(key, admin, diagnostics));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
            }
        }

        private static void AssertPublicRetirementFailure(
            LMCAxisSetPositionRecoveryKey key,
            uint generation)
        {
            using (var server = new FakeRpcServer(
                InitStep(), CallbackStep(), AxisLookupStep(), AxisInfoStep(),
                AdminCapabilitiesStep(1), DiagnosticsCapabilitiesStep(1),
                new FakeRpcStep(0x7D1A, TestFrame.Response(0,
                    FailurePayload(2, LMCAdminDetailCode.SetPositionOutcomeNotFound))),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Connect(connection, server.Port);
                var axis = new LMCSingleAxis(connection, AxisName);
                var admin = connection.Admin.GetCapabilities();
                var diagnostics = connection.Diagnostics.GetCapabilities();
                AssertEx.Throws<LMCAxisSetPositionOutcomeRetirementException>(() =>
                    axis.RetireSetPositionOutcome(key, generation, admin, diagnostics));
                connection.CloseConnection();
                server.Verify();
                AssertEx.Equal(0, CountCommand(server, 0x7D12));
            }
        }

        private static FakeRpcStep OutcomeStep(uint requestId,
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            uint generation,
            LMCAdminDetailCode detail)
        {
            return new FakeRpcStep(0x7D14, TestFrame.Response(0,
                TerminalPayload(requestId, key, state, generation, detail)));
        }

        private static FakeRpcStep RetirementStep(uint requestId,
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            uint generation,
            LMCAdminDetailCode detail)
        {
            return new FakeRpcStep(0x7D1A, TestFrame.Response(0,
                TerminalPayload(requestId, key, state, generation, detail)));
        }

        private static byte[] TerminalPayload(uint requestId,
            LMCAxisSetPositionRecoveryKey key,
            LMCAxisSetPositionOutcomeRecordState state,
            uint generation,
            LMCAdminDetailCode detail)
        {
            var succeeded = state == LMCAxisSetPositionOutcomeRecordState.Succeeded;
            var payload = CommonPayload(requestId, 84);
            TestFrame.WriteUInt16(payload, 16, (ushort)state);
            TestFrame.WriteUInt16(payload, 18, (ushort)key.SemanticMode);
            TestFrame.WriteUInt32(payload, 20, key.DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, key.DiagnosticsBootId);
            TestFrame.WriteUInt32(payload, 28, key.MapRevision);
            TestFrame.WriteUInt32(payload, 32, key.OriginalRequestId);
            TestFrame.WriteUInt32(payload, 36, key.ClientIntentId0);
            TestFrame.WriteUInt32(payload, 40, key.ClientIntentId1);
            TestFrame.WriteUInt32(payload, 44, key.ClientIntentId2);
            TestFrame.WriteUInt32(payload, 48, key.ClientIntentId3);
            TestFrame.WriteUInt16(payload, 52, key.AxisReference);
            TestFrame.WriteInt32(payload, 56, key.TargetPosition);
            TestFrame.WriteInt32(payload, 60, key.ExpectedActualPosition);
            TestFrame.WriteInt32(payload, 64, succeeded ? key.TargetPosition : 0);
            TestFrame.WriteUInt16(payload, 68, succeeded ? (ushort)0 : (ushort)1);
            TestFrame.WriteInt16(payload, 70, succeeded ? (short)0 : (short)-31000);
            TestFrame.WriteUInt32(payload, 72, succeeded ? 0U : (uint)detail);
            TestFrame.WriteUInt32(payload, 76, 0);
            TestFrame.WriteUInt32(payload, 80, generation);
            return payload;
        }

        private static byte[] FailurePayload(uint requestId, LMCAdminDetailCode detail)
        {
            var payload = CommonPayload(requestId, 16);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteInt16(payload, 6, -31000);
            TestFrame.WriteUInt32(payload, 12, (uint)detail);
            return payload;
        }

        private static byte[] CommonPayload(uint requestId, int length)
        {
            var payload = new byte[length];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            return payload;
        }

        private static FakeRpcStep AdminCapabilitiesStep(uint requestId)
        {
            var features = LMCAdminFeature.AxisSetPositionOutcomeRead
                | LMCAdminFeature.AxisSetPositionOutcomeRetirement;
            var payload = CommonPayload(requestId, 40);
            TestFrame.WriteUInt32(payload, 16, (uint)features);
            TestFrame.WriteUInt32(payload, 20, 0x3FU);
            TestFrame.WriteUInt32(payload, 24, (uint)LMCGroupParameterSelection.All);
            TestFrame.WriteUInt16(payload, 28, 4);
            TestFrame.WriteUInt16(payload, 30, 1);
            TestFrame.WriteUInt16(payload, 32, 0x0100);
            TestFrame.WriteUInt16(payload, 34, 3);
            TestFrame.WriteUInt16(payload, 36, 2);
            return new FakeRpcStep(0x7D00, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep DiagnosticsCapabilitiesStep(uint requestId)
        {
            var payload = new byte[68];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteUInt32(payload, 8, requestId);
            TestFrame.WriteUInt32(payload, 16, DiagnosticsBuild);
            TestFrame.WriteUInt32(payload, 24, MapRevision);
            TestFrame.WriteUInt32(payload, 40, 1000);
            TestFrame.WriteUInt16(payload, 44, 1320);
            TestFrame.WriteUInt16(payload, 46, 2040);
            TestFrame.WriteUInt16(payload, 48, 1280);
            TestFrame.WriteUInt32(payload, 64, CurrentDiagnosticsBootId);
            return new FakeRpcStep(0x7E00, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(0x8080, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(0x405C,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep AxisLookupStep()
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, 1);
            return new FakeRpcStep(0x103C, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep AxisInfoStep()
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, 1);
            return new FakeRpcStep(0x202B, TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(0x405D,
                TestFrame.Response(0, TestFrame.Hex("00 00 00 00")));
        }

        private static void Connect(LMCConnection connection, int port)
        {
            connection.RpcInitConnection("127.0.0.1", port, "127.0.0.1", 0,
                LMCConnection.DefaultEventMask);
        }

        private static int CountCommand(FakeRpcServer server, ushort command)
        {
            var count = 0;
            foreach (var request in server.ReceivedRequests)
                if (TestFrame.ReadUInt16(request, 0) == command) count++;
            return count;
        }

        private static LMCAxisSetPositionRecoveryKey RecoveryKey(
            uint originalRequestId = OriginalRequestId)
        {
            return new LMCAxisSetPositionRecoveryKey(
                1, originalRequestId, DiagnosticsBuild, OriginalDiagnosticsBootId,
                MapRevision, Intent0, Intent1, Intent2, Intent3, 1,
                TargetPosition, ExpectedActualPosition,
                LMCAxisSetPositionSemanticMode.ActualAndDestinationApplicationUnits);
        }

        private static AxisSetPositionRecoveryRecord Arm(
            AxisSetPositionRecoveryJournal journal,
            Guid identity)
        {
            return journal.ArmBeforeDispatch(identity, "127.1", 4000,
                DiagnosticsBuild, OriginalDiagnosticsBootId, MapRevision,
                AxisName, 1, Intent0, Intent1, Intent2, Intent3,
                OriginalRequestId, TargetPosition, ExpectedActualPosition,
                1, 1, FixedUtc());
        }

        private static AxisSetPositionRecoveryRecord CreateRecord(
            Guid identity,
            uint diagnosticsBuild,
            uint requestId,
            ushort semanticMode,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            return new AxisSetPositionRecoveryRecord(identity, EndpointIp, 4000,
                diagnosticsBuild, 2, 3, "Axis", 1, 1, 2, 3, 4,
                requestId, 10, 20, semanticMode, 1,
                AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                createdUtc, updatedUtc);
        }

        private static void AssertExactIntent(
            AxisSetPositionRecoveryRecord record,
            Guid identity)
        {
            AssertEx.NotNull(record);
            AssertEx.Equal(identity, record.Identity);
            AssertEx.Equal(EndpointIp, record.EndpointIp);
            AssertEx.Equal(4000, record.EndpointPort);
            AssertEx.Equal(DiagnosticsBuild, record.DiagnosticsBuild);
            AssertEx.Equal(OriginalDiagnosticsBootId, record.DiagnosticsBootId);
            AssertEx.Equal(MapRevision, record.MapRevision);
            AssertEx.Equal(AxisName, record.AxisName);
            AssertEx.Equal((ushort)1, record.AxisReference);
            AssertEx.Equal(Intent0, record.ClientIntentId0);
            AssertEx.Equal(Intent1, record.ClientIntentId1);
            AssertEx.Equal(Intent2, record.ClientIntentId2);
            AssertEx.Equal(Intent3, record.ClientIntentId3);
            AssertEx.Equal(OriginalRequestId, record.RequestId);
            AssertEx.Equal(TargetPosition, record.TargetPosition);
            AssertEx.Equal(ExpectedActualPosition, record.ExpectedActualPosition);
            AssertEx.Equal((ushort)1, record.SemanticMode);
            AssertEx.Equal((ushort)1, record.SchemaVersion);
            AssertEx.Equal(FixedUtc(), record.CreatedUtc);
        }

        private static void AssertTerminalProof(
            AxisSetPositionRecoveryRecord record,
            LMCAxisSetPositionOutcomeResult outcome)
        {
            AssertEx.True(record.HasTerminalOutcomeProof);
            var proof = record.TerminalOutcomeProof;
            AssertEx.Equal(outcome.QueryRequestId, proof.QueryRequestId);
            AssertEx.Equal(outcome.RecordState, proof.RecordState);
            AssertEx.Equal(outcome.AppliedPosition, proof.AppliedPosition);
            AssertEx.Equal(outcome.OriginalCommandStatus, proof.OriginalCommandStatus);
            AssertEx.Equal(outcome.OriginalErrorId, proof.OriginalErrorId);
            AssertEx.Equal(outcome.OriginalDetailCodeValue, proof.OriginalDetailCode);
            AssertEx.Equal(outcome.NativeCommandState, proof.NativeCommandState);
            AssertEx.Equal(outcome.RecordGeneration, proof.RecordGeneration);
        }

        private static void AssertOutcomeRecoveryKeyFieldMatrixRejected(
            AxisSetPositionRecoveryJournal journal,
            AxisSetPositionRecoveryRecord recovery,
            LMCAxisSetPositionOutcomeResult outcome,
            byte[] expectedBytes)
        {
            AssertRecoveryKeyFieldMatrixRejected(
                journal,
                expectedBytes,
                outcome.RecoveryKey,
                () => journal.RecordTerminalOutcome(
                    recovery,
                    outcome,
                    FixedUtc().AddSeconds(2)));
        }

        private static void AssertRetirementRecoveryKeyFieldMatrixRejected(
            AxisSetPositionRecoveryJournal journal,
            AxisSetPositionRecoveryRecord observed,
            LMCAxisSetPositionOutcomeRetirementResult retirement,
            byte[] expectedBytes)
        {
            AssertRecoveryKeyFieldMatrixRejected(
                journal,
                expectedBytes,
                retirement.RecoveryKey,
                () => journal.ResolveAfterRetirement(
                    observed,
                    retirement,
                    FixedUtc().AddSeconds(3)));
        }

        private static void AssertRecoveryKeyFieldMatrixRejected(
            AxisSetPositionRecoveryJournal journal,
            byte[] expectedBytes,
            LMCAxisSetPositionRecoveryKey key,
            Action operation)
        {
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "SchemaVersion", (ushort)2, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "OriginalRequestId",
                OriginalRequestId + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "DiagnosticsBuild",
                DiagnosticsBuild + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "DiagnosticsBootId",
                OriginalDiagnosticsBootId + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "MapRevision",
                MapRevision + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key.ClientIntentId, "Word0",
                Intent0 + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key.ClientIntentId, "Word1",
                Intent1 + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key.ClientIntentId, "Word2",
                Intent2 + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key.ClientIntentId, "Word3",
                Intent3 + 1U, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "AxisReference", (ushort)2, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "TargetPosition",
                TargetPosition + 1, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "ExpectedActualPosition",
                ExpectedActualPosition + 1, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, key, "SemanticMode",
                (LMCAxisSetPositionSemanticMode)2, operation);
        }

        private static void AssertRetirementSnapshotFieldMatrixRejected(
            AxisSetPositionRecoveryJournal journal,
            AxisSetPositionRecoveryRecord observed,
            LMCAxisSetPositionOutcomeRetirementResult retirement,
            byte[] expectedBytes)
        {
            Action operation = () => journal.ResolveAfterRetirement(
                observed,
                retirement,
                FixedUtc().AddSeconds(3));
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, retirement, "RecordState",
                LMCAxisSetPositionOutcomeRecordState.Rejected, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, retirement, "AppliedPosition",
                TargetPosition + 1, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, retirement, "OriginalCommandStatus",
                (ushort)1, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, retirement, "OriginalErrorId",
                (short)-1, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, retirement, "OriginalDetailCodeValue",
                (uint)LMCAdminDetailCode.CoordinatePreconditionFailed, operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, retirement, "NativeCommandState", 1U,
                operation);
            AssertPrivatePropertyMismatchPreserved<InvalidOperationException>(
                journal, expectedBytes, retirement, "RecordGeneration",
                RecordGeneration + 1U, operation);
        }

        private static void AssertPrivatePropertyMismatchPreserved<TException>(
            AxisSetPositionRecoveryJournal journal,
            byte[] expectedBytes,
            object target,
            string propertyName,
            object mismatchedValue,
            Action operation)
            where TException : Exception
        {
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            if (property == null || property.GetSetMethod(true) == null)
            {
                throw new InvalidOperationException(
                    "Expected private SDK result setter is unavailable: "
                    + target.GetType().Name
                    + "."
                    + propertyName);
            }

            var setter = property.GetSetMethod(true);
            var originalValue = property.GetValue(target, null);
            try
            {
                setter.Invoke(target, new[] { mismatchedValue });
                AssertPreserved<TException>(journal, expectedBytes, operation);
            }
            finally
            {
                setter.Invoke(target, new[] { originalValue });
            }
        }

        private static void AssertPreserved<TException>(
            AxisSetPositionRecoveryJournal journal,
            byte[] expected,
            Action operation)
            where TException : Exception
        {
            var expectedState = journal.CurrentRecord.State;
            var error = AssertEx.Throws<TException>(operation);
            AssertEx.Equal(typeof(TException), error.GetType());
            AssertBytesEqual(expected, File.ReadAllBytes(journal.JournalFilePath));
            AssertEx.Equal(expectedState, journal.CurrentRecord.State);
        }

        private static void SetPrivateProperty(
            object target,
            string propertyName,
            object value)
        {
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            if (property == null || property.GetSetMethod(true) == null)
            {
                throw new InvalidOperationException(
                    "Expected private SDK result setter is unavailable: "
                    + propertyName);
            }
            property.GetSetMethod(true).Invoke(target, new[] { value });
        }

        private static void WriteLegacyJournal(
            string directory,
            Guid identity,
            AxisSetPositionRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            byte[] payload;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(identity.ToByteArray());
                writer.Write((int)state);
                writer.Write(createdUtc.Ticks);
                writer.Write(updatedUtc.Ticks);
                writer.Write(DiagnosticsBuild);
                writer.Write(OriginalDiagnosticsBootId);
                writer.Write(MapRevision);
                writer.Write(4000);
                writer.Write((ushort)1);
                writer.Write((ushort)1);
                writer.Write((ushort)1);
                writer.Write(OriginalRequestId);
                writer.Write(Intent0);
                writer.Write(Intent1);
                writer.Write(Intent2);
                writer.Write(Intent3);
                writer.Write(TargetPosition);
                writer.Write(ExpectedActualPosition);
                WriteLegacyText(writer, EndpointIp);
                WriteLegacyText(writer, AxisName);
                writer.Flush();
                payload = stream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("ELMOASP1"));
                writer.Write(1);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
                prefix = stream.ToArray();
            }
            byte[] checksum;
            using (var sha = SHA256.Create()) checksum = sha.ComputeHash(prefix);
            var bytes = new byte[prefix.Length + checksum.Length];
            Buffer.BlockCopy(prefix, 0, bytes, 0, prefix.Length);
            Buffer.BlockCopy(checksum, 0, bytes, prefix.Length, checksum.Length);
            WriteJournalBytes(directory, bytes);
        }

        private static void WriteLegacyText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static uint ReadFormatVersion(byte[] bytes)
        {
            return TestFrame.ReadUInt32(bytes, 8);
        }

        private static byte[] AppendTrailingPayloadByte(byte[] original, byte value)
        {
            var oldChecksumOffset = original.Length - ChecksumLength;
            var expanded = new byte[original.Length + 1];
            Buffer.BlockCopy(original, 0, expanded, 0, oldChecksumOffset);
            expanded[oldChecksumOffset] = value;
            TestFrame.WriteInt32(expanded, 12,
                checked((int)TestFrame.ReadUInt32(original, 12) + 1));
            RecomputeChecksum(expanded);
            return expanded;
        }

        private static int FindV2ProofMarkerOffset(byte[] bytes)
        {
            const int payloadOffset = 16;
            using (var stream = new MemoryStream(bytes, payloadOffset,
                checked((int)TestFrame.ReadUInt32(bytes, 12)), false))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                stream.Position = 86;
                SkipText(reader);
                SkipText(reader);
                return payloadOffset + checked((int)stream.Position);
            }
        }

        private static void SkipText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length < 1 || length > 1024)
                throw new InvalidDataException("Test journal text is invalid.");
            if (reader.ReadBytes(length).Length != length)
                throw new InvalidDataException("Test journal text is truncated.");
        }

        private static void RecomputeChecksum(byte[] bytes)
        {
            var checksumOffset = bytes.Length - ChecksumLength;
            byte[] checksum;
            using (var sha = SHA256.Create())
                checksum = sha.ComputeHash(bytes, 0, checksumOffset);
            Buffer.BlockCopy(checksum, 0, bytes, checksumOffset, ChecksumLength);
        }

        private static void WriteJournalBytes(string directory, byte[] bytes)
        {
            File.WriteAllBytes(Path.Combine(directory,
                "axis-set-position-recovery.bin"), bytes);
        }

        private static void AssertOpenInvalid(string directory)
        {
            AssertEx.Throws<InvalidDataException>(() =>
                AxisSetPositionRecoveryJournal.Open(directory));
        }

        private static void AssertBytesEqual(byte[] expected, byte[] actual)
        {
            AssertEx.Equal(expected.Length, actual.Length);
            for (var index = 0; index < expected.Length; index++)
                AssertEx.Equal(expected[index], actual[index]);
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(638895500000000000L, DateTimeKind.Utc);
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(),
                "ElmoAxisSetPositionJournalTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteTemporaryDirectory(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

    }
}

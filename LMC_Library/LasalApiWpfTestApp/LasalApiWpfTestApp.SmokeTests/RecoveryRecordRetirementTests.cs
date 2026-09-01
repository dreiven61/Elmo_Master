using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class RecoveryRecordRetirementTests
    {
        private const uint StoredBootId = 0x10203040;
        private const uint StoredMapRevision = 0x50607080;
        private const uint CurrentBootId = StoredBootId + 1;
        private const string OperatorIdentity = "TEST\\operator";
        private const string RetirementReason =
            "Operator confirmed that the source recovery identity was superseded.";

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.RecoveryRetirementLedger.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.RecoveryRetirementLedger.ExactEvidenceReopenAndSingleWriter",
                ExactEvidenceSurvivesReopenAndSingleWriterIsEnforced);
            tests.Add(
                "Wpf.RecoveryRetirementLedger.LegacyV1V2ReadCompatibility",
                LegacyV1V2ReadCompatibility);
            tests.Add(
                "Wpf.RecoveryRetirementLedger.CorruptionAndImmutableContext",
                CorruptionAndImmutableDecisionContextFailClosed);
            tests.Add(
                "Wpf.RecoveryRetirementLedger.PreexistingEntryIsNotReplaced",
                PreexistingEntryIsNotReplaced);
            tests.Add(
                "Wpf.RecoveryRetirement.AxisPowerExactCas",
                AxisPowerExactCasAndStaleEvidence);
            tests.Add(
                "Wpf.RecoveryRetirement.AxisQualificationExactSourceBytesAndTombstone",
                AxisQualificationExactSourceBytesAndTombstone);
            tests.Add(
                "Wpf.RecoveryRetirement.AxisCommandExactCas",
                AxisCommandExactCasAndStaleEvidence);
            tests.Add(
                "Wpf.RecoveryRetirement.MotionExactCas",
                MotionExactCasAndStaleEvidence);
            tests.Add(
                "Wpf.RecoveryRetirement.GroupProfileExactCas",
                GroupProfileExactCasAndStaleEvidence);
            tests.Add(
                "Wpf.RecoveryRetirement.GroupPowerExactCas",
                GroupPowerExactCasAndStaleEvidence);
            tests.Add(
                "Wpf.RecoveryRetirement.DiagnosticsMutationLegacyEndpointBindingExactCas",
                DiagnosticsMutationLegacyEndpointBindingExactCas);
            tests.Add(
                "Wpf.DiagnosticsMutationJournal.SdoWriteBaselineGuardV4RoundTrip",
                DiagnosticsMutationSdoWriteBaselineGuardV4RoundTrip);
        }

        private static void
            DiagnosticsMutationSdoWriteBaselineGuardV4RoundTrip()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var baseline = new byte[] { 0x34, 0x12 };
                var expected = new byte[] { 0x78, 0x56 };
                using (var journal = DiagnosticsMutationJournal.Open(root))
                {
                    journal.Arm(
                        DiagnosticsMutationKind.SdoWrite,
                        Guid.NewGuid(),
                        FixedUtc(),
                        StoredBootId,
                        StoredMapRevision,
                        5,
                        "Slave=2,Object=0x2001,SubIndex=3,Type=UInt16,Length=2",
                        "WriteData=78-56",
                        new DiagnosticsSdoWriteMutationMetadata(
                            2,
                            0x2001,
                            3,
                            LMCSignalValueType.UInt16,
                            2,
                            250,
                            "127.0.0.1",
                            4000,
                            1,
                            baseline,
                            baseline,
                            expected));
                }

                using (var reopened = DiagnosticsMutationJournal.Open(root))
                {
                    var metadata = reopened.CurrentRecord.SdoWriteMetadata;
                    AssertEx.True(metadata.HasBaselineGuardEvidence);
                    AssertEx.SequenceEqual(
                        baseline,
                        metadata.BaselineData);
                    AssertEx.SequenceEqual(
                        baseline,
                        metadata.PreWriteGuardData);
                    AssertEx.SequenceEqual(
                        expected,
                        metadata.ExpectedWriteData);
                }

                var tw20Metadata =
                    new DiagnosticsSdoWriteMutationMetadata(
                        1,
                        0x3204,
                        0,
                        LMCSignalValueType.UInt16,
                        2,
                        100,
                        new byte[] { 0, 0 });
                AssertEx.Equal((ushort)0x3204, tw20Metadata.ObjectIndex);
                var tw19Metadata =
                    new DiagnosticsSdoWriteMutationMetadata(
                        1,
                        0x20FC,
                        0,
                        LMCSignalValueType.UInt32,
                        4,
                        100,
                        new byte[] { 0, 0, 0, 0 });
                AssertEx.Equal((ushort)0x20FC, tw19Metadata.ObjectIndex);
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void
            DiagnosticsMutationLegacyEndpointBindingExactCas()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var journalDirectory = Path.Combine(
                    root,
                    "diagnostics-mutation");
                var ledgerDirectory = Path.Combine(root, "ledger");
                var created = FixedUtc();
                RecoveryJournalSourceEvidence evidence;
                RecoveryRecordRetirementDecision decision;
                Guid identity;

                using (var journal = DiagnosticsMutationJournal.Open(
                    journalDirectory))
                {
                    identity = Guid.NewGuid();
                    journal.Arm(
                        DiagnosticsMutationKind.SdoWrite,
                        identity,
                        created,
                        StoredBootId,
                        StoredMapRevision,
                        7,
                        "Slave=1,Object=0x2F00,SubIndex=24,Type=Int32,Length=4",
                        "WriteData=00-00-00-00",
                        new DiagnosticsSdoWriteMutationMetadata(
                            1,
                            0x2F00,
                            24,
                            LMCSignalValueType.Int32,
                            4,
                            100,
                            new byte[] { 0, 0, 0, 0 }));
                    journal.Transition(
                        identity,
                        DiagnosticsMutationState.AcceptedPendingTerminal,
                        created.AddSeconds(1),
                        3);
                    journal.Transition(
                        identity,
                        DiagnosticsMutationState.OutcomeUnverified,
                        created.AddSeconds(2),
                        3);

                    evidence = journal
                        .CaptureLegacyEndpointBoundRetirementEvidence(
                            "127.0.0.1",
                            4000);
                    AssertEvidence(
                        evidence,
                        RecoveryRecordOwner.DiagnosticsMutation,
                        journal.JournalFilePath);
                    AssertEx.Equal(
                        RecoveryEndpointEvidenceKind
                            .OperatorClassifiedLegacyEndpoint,
                        evidence.EndpointEvidenceKind);
                    AssertEx.Equal((ushort)1, evidence.TargetReference);
                    AssertEx.True(
                        evidence.SemanticFingerprint.Contains(
                            "EndpointBinding=OperatorClassifiedCurrentQuarantineEndpoint"));

                    using (var ledger =
                        RecoveryRecordRetirementLedger.Open(
                            ledgerDirectory))
                    {
                        decision = Commit(
                            ledger,
                            evidence,
                            created.AddSeconds(3));
                    }

                    var resolved = journal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        created.AddSeconds(4));
                    AssertEx.Equal(
                        DiagnosticsMutationState.Resolved,
                        resolved.State);
                    AssertEx.False(journal.HasActiveRecord);
                }

                using (var reopenedLedger =
                    RecoveryRecordRetirementLedger.Open(ledgerDirectory))
                {
                    AssertEx.Equal(
                        1,
                        reopenedLedger.CommittedDecisions.Count);
                    var reopened = reopenedLedger.CommittedDecisions[0];
                    AssertEx.Equal(
                        RecoveryEndpointEvidenceKind
                            .OperatorClassifiedLegacyEndpoint,
                        reopened.SourceEvidence.EndpointEvidenceKind);
                    AssertEx.SequenceEqual(
                        evidence.GetOriginalBytes(),
                        reopened.SourceEvidence.GetOriginalBytes());
                }

                var staleJournalDirectory = Path.Combine(
                    root,
                    "diagnostics-mutation-stale");
                using (var staleJournal = DiagnosticsMutationJournal.Open(
                    staleJournalDirectory))
                {
                    var staleIdentity = Guid.NewGuid();
                    staleJournal.Arm(
                        DiagnosticsMutationKind.SdoWrite,
                        staleIdentity,
                        created,
                        StoredBootId,
                        StoredMapRevision,
                        8,
                        "Slave=1,Object=0x2F00,SubIndex=24,Type=Int32,Length=4",
                        "WriteData=00-00-00-00",
                        new DiagnosticsSdoWriteMutationMetadata(
                            1,
                            0x2F00,
                            24,
                            LMCSignalValueType.Int32,
                            4,
                            100,
                            new byte[] { 0, 0, 0, 0 }));
                    staleJournal.Transition(
                        staleIdentity,
                        DiagnosticsMutationState.AcceptedPendingTerminal,
                        created.AddSeconds(1),
                        4);
                    staleJournal.Transition(
                        staleIdentity,
                        DiagnosticsMutationState.OutcomeUnverified,
                        created.AddSeconds(2),
                        4);
                    var staleEvidence = staleJournal
                        .CaptureLegacyEndpointBoundRetirementEvidence(
                            "127.0.0.1",
                            4000);
                    RecoveryRecordRetirementDecision staleDecision;
                    using (var ledger =
                        RecoveryRecordRetirementLedger.Open(
                            Path.Combine(root, "stale-ledger")))
                    {
                        staleDecision = Commit(
                            ledger,
                            staleEvidence,
                            created.AddSeconds(3));
                    }
                    staleJournal.Transition(
                        staleIdentity,
                        DiagnosticsMutationState.ReadbackMismatch,
                        created.AddSeconds(4),
                        4);
                    AssertEx.Throws<InvalidOperationException>(
                        () => staleJournal.ResolveOperatorRetirement(
                            staleEvidence,
                            staleDecision,
                            created.AddSeconds(5)));
                    AssertEx.Equal(
                        DiagnosticsMutationState.ReadbackMismatch,
                        staleJournal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void DefaultPathIsVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "RecoveryRecordRetirementLedger",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                RecoveryRecordRetirementLedger
                    .GetDefaultDirectoryPath()
                    .ToUpperInvariant());
        }

        private static void LegacyV1V2ReadCompatibility()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                AssertLegacyLedgerFormatReopens(
                    Path.Combine(root, "v1"),
                    1,
                    0,
                    0);
                AssertLegacyLedgerFormatReopens(
                    Path.Combine(root, "v2"),
                    2,
                    0x01020304,
                    0x05060708);

                var unsupportedDirectory = Path.Combine(
                    root,
                    "unsupported");
                Directory.CreateDirectory(unsupportedDirectory);
                var unsupportedDecision = CreateLegacyDecision(
                    2,
                    0x11121314,
                    0x15161718);
                var unsupportedBytes = SerializeLegacyLedgerEntry(
                    unsupportedDecision,
                    2);
                RewriteLedgerFormatVersion(unsupportedBytes, 99);
                WriteLegacyLedgerEntry(
                    unsupportedDirectory,
                    unsupportedDecision.SourceEvidence,
                    unsupportedBytes);

                AssertEx.Throws<InvalidDataException>(
                    () => RecoveryRecordRetirementLedger.Open(
                        unsupportedDirectory));
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void AssertLegacyLedgerFormatReopens(
            string ledgerDirectory,
            int formatVersion,
            uint sourceDiagnosticsBuild,
            uint currentDiagnosticsBuild)
        {
            Directory.CreateDirectory(ledgerDirectory);
            var expected = CreateLegacyDecision(
                formatVersion,
                sourceDiagnosticsBuild,
                currentDiagnosticsBuild);
            var entryBytes = SerializeLegacyLedgerEntry(
                expected,
                formatVersion);
            WriteLegacyLedgerEntry(
                ledgerDirectory,
                expected.SourceEvidence,
                entryBytes);

            using (var ledger = RecoveryRecordRetirementLedger.Open(
                ledgerDirectory))
            {
                AssertEx.Equal(1, ledger.CommittedDecisions.Count);
                var actual = ledger.CommittedDecisions[0];
                AssertEx.True(actual.IsDurablyCommitted);
                AssertEx.Equal(
                    expected.DecisionIdentity,
                    actual.DecisionIdentity);
                AssertEx.Equal(
                    RecoveryEndpointEvidenceKind.RecordedSourceEndpoint,
                    actual.SourceEvidence.EndpointEvidenceKind);
                AssertEx.Equal(
                    sourceDiagnosticsBuild,
                    actual.SourceEvidence.DiagnosticsBuild);
                AssertEx.Equal(
                    currentDiagnosticsBuild,
                    actual.CurrentDiagnosticsBuild);
                AssertEx.True(
                    actual.MatchesSourceEvidence(expected.SourceEvidence));
                AssertEx.SequenceEqual(
                    expected.SourceEvidence.GetOriginalBytes(),
                    actual.SourceEvidence.GetOriginalBytes());
                AssertEx.Equal(
                    RecoveryJournalSourceEvidence.ComputeSha256(entryBytes),
                    actual.DurableEntrySha256);
            }
        }

        private static RecoveryRecordRetirementDecision
            CreateLegacyDecision(
                int formatVersion,
                uint sourceDiagnosticsBuild,
                uint currentDiagnosticsBuild)
        {
            var created = FixedUtc();
            var recordIdentity = formatVersion == 1
                ? new Guid("11111111-2222-3333-4444-555555555551")
                : new Guid("11111111-2222-3333-4444-555555555552");
            var decisionIdentity = formatVersion == 1
                ? new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEE1")
                : new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEE2");
            var evidence = new RecoveryJournalSourceEvidence(
                RecoveryRecordOwner.AxisPower,
                recordIdentity,
                1,
                created,
                created.AddSeconds(1),
                "127.0.0.1",
                4000,
                sourceDiagnosticsBuild,
                StoredBootId,
                StoredMapRevision,
                "Axis",
                "_LMCAxis1",
                1,
                "PowerOn",
                "PowerOn=true;LegacyFormat="
                    + formatVersion.ToString(
                        CultureInfo.InvariantCulture),
                new byte[]
                {
                    0x45,
                    0x4C,
                    0x4D,
                    0x4F,
                    checked((byte)formatVersion)
                });
            return new RecoveryRecordRetirementDecision(
                decisionIdentity,
                evidence,
                evidence.EndpointIp,
                evidence.EndpointPort,
                currentDiagnosticsBuild,
                CurrentBootId,
                evidence.MapRevision,
                OperatorIdentity,
                RetirementReason,
                created.AddSeconds(2));
        }

        private static byte[] SerializeLegacyLedgerEntry(
            RecoveryRecordRetirementDecision decision,
            int formatVersion)
        {
            if (formatVersion != 1 && formatVersion != 2)
            {
                throw new ArgumentOutOfRangeException("formatVersion");
            }

            var source = decision.SourceEvidence;
            byte[] payload;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(
                stream,
                Encoding.UTF8,
                true))
            {
                writer.Write(decision.DecisionIdentity.ToByteArray());
                writer.Write((int)source.Owner);
                writer.Write(source.RecordIdentity.ToByteArray());
                writer.Write(source.StateCode);
                writer.Write(source.CreatedUtc.Ticks);
                writer.Write(source.UpdatedUtc.Ticks);
                if (formatVersion >= 2)
                {
                    writer.Write(source.DiagnosticsBuild);
                }
                writer.Write(source.DiagnosticsBootId);
                writer.Write(source.MapRevision);
                writer.Write(source.EndpointPort);
                writer.Write(source.TargetReference);
                writer.Write(decision.DecisionUtc.Ticks);
                if (formatVersion >= 2)
                {
                    writer.Write(decision.CurrentDiagnosticsBuild);
                }
                writer.Write(decision.CurrentDiagnosticsBootId);
                writer.Write(decision.CurrentMapRevision);
                writer.Write(decision.CurrentEndpointPort);
                WriteLegacyLedgerText(writer, source.EndpointIp);
                WriteLegacyLedgerText(writer, source.TargetKind);
                WriteLegacyLedgerText(writer, source.TargetName);
                WriteLegacyLedgerText(writer, source.Operation);
                WriteLegacyLedgerText(
                    writer,
                    source.SemanticFingerprint);
                WriteLegacyLedgerText(writer, source.OriginalSha256);
                WriteLegacyLedgerText(
                    writer,
                    decision.CurrentEndpointIp);
                WriteLegacyLedgerText(writer, decision.OperatorIdentity);
                WriteLegacyLedgerText(writer, decision.Reason);
                var original = source.GetOriginalBytes();
                writer.Write(original.Length);
                writer.Write(original);
                writer.Flush();
                payload = stream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(
                stream,
                Encoding.ASCII,
                true))
            {
                writer.Write(Encoding.ASCII.GetBytes("ELMORET1"));
                writer.Write(formatVersion);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
                prefix = stream.ToArray();
            }

            byte[] checksum;
            using (var algorithm = SHA256.Create())
            {
                checksum = algorithm.ComputeHash(prefix);
            }
            var result = new byte[prefix.Length + checksum.Length];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(
                checksum,
                0,
                result,
                prefix.Length,
                checksum.Length);
            return result;
        }

        private static void WriteLegacyLedgerText(
            BinaryWriter writer,
            string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static void RewriteLedgerFormatVersion(
            byte[] entryBytes,
            int formatVersion)
        {
            const int magicLength = 8;
            const int checksumLength = 32;
            using (var stream = new MemoryStream(entryBytes, true))
            using (var writer = new BinaryWriter(
                stream,
                Encoding.ASCII,
                true))
            {
                stream.Position = magicLength;
                writer.Write(formatVersion);
                writer.Flush();
            }

            var checksumOffset = entryBytes.Length - checksumLength;
            byte[] checksum;
            using (var algorithm = SHA256.Create())
            {
                checksum = algorithm.ComputeHash(
                    entryBytes,
                    0,
                    checksumOffset);
            }
            Buffer.BlockCopy(
                checksum,
                0,
                entryBytes,
                checksumOffset,
                checksum.Length);
        }

        private static void WriteLegacyLedgerEntry(
            string ledgerDirectory,
            RecoveryJournalSourceEvidence evidence,
            byte[] entryBytes)
        {
            var fileName = ((int)evidence.Owner).ToString(
                    "D2",
                    CultureInfo.InvariantCulture)
                + "-"
                + evidence.RecordIdentity.ToString("N")
                + "-"
                + evidence.OriginalSha256
                + ".retired";
            File.WriteAllBytes(
                Path.Combine(ledgerDirectory, fileName),
                entryBytes);
        }

        private static void
            ExactEvidenceSurvivesReopenAndSingleWriterIsEnforced()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var journalDirectory = Path.Combine(root, "axis-power");
                var ledgerDirectory = Path.Combine(root, "ledger");
                RecoveryJournalSourceEvidence evidence;
                RecoveryRecordRetirementDecision decision;
                var created = FixedUtc();
                using (var journal = AxisPowerOnRecoveryJournal.Open(
                    journalDirectory))
                {
                    journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEx.SequenceEqual(
                        File.ReadAllBytes(journal.JournalFilePath),
                        evidence.GetOriginalBytes());
                    AssertEx.Equal(
                        RecoveryRecordRetirementLedger.GetDefaultDirectoryPath()
                            .EndsWith(
                                Path.Combine(
                                    "RecoveryRecordRetirementLedger",
                                    "v1"),
                                StringComparison.OrdinalIgnoreCase),
                        true);

                    using (var ledger =
                        RecoveryRecordRetirementLedger.Open(ledgerDirectory))
                    {
                        AssertEx.Throws<IOException>(
                            () => RecoveryRecordRetirementLedger.Open(
                                ledgerDirectory));
                        decision = Commit(ledger, evidence, created.AddSeconds(1));
                        AssertEx.Equal(1, ledger.CommittedDecisions.Count);
                        AssertEx.True(decision.MatchesSourceEvidence(evidence));
                        var defensive = decision.SourceEvidence.GetOriginalBytes();
                        defensive[0] ^= 0xFF;
                        AssertEx.False(
                            RecoveryJournalSourceEvidence.ConstantTimeEquals(
                                defensive,
                                decision.SourceEvidence.GetOriginalBytes()));
                    }
                }

                using (var reopened =
                    RecoveryRecordRetirementLedger.Open(ledgerDirectory))
                using (var journal = AxisPowerOnRecoveryJournal.Open(
                    journalDirectory))
                {
                    AssertEx.Equal(1, reopened.CommittedDecisions.Count);
                    var pending = reopened.FindPendingDecision(evidence);
                    AssertEx.NotNull(pending);
                    AssertEx.Equal(
                        decision.DecisionIdentity,
                        pending.DecisionIdentity);
                    AssertEx.SequenceEqual(
                        evidence.GetOriginalBytes(),
                        pending.SourceEvidence.GetOriginalBytes());
                    AssertEx.Equal(
                        evidence.OriginalSha256,
                        pending.SourceEvidence.OriginalSha256);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        journal.ResolveOperatorRetirement(
                            evidence,
                            pending,
                            created.AddSeconds(2)).State);
                    AssertEx.False(journal.HasActiveRecord);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void CorruptionAndImmutableDecisionContextFailClosed()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var journalDirectory = Path.Combine(root, "axis-power");
                var ledgerDirectory = Path.Combine(root, "ledger");
                RecoveryJournalSourceEvidence evidence;
                var created = FixedUtc();
                using (var journal = AxisPowerOnRecoveryJournal.Open(
                    journalDirectory))
                {
                    journal.ArmBeforeDispatch(
                        true,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    evidence = journal.CaptureActiveRetirementEvidence();
                }

                using (var ledger =
                    RecoveryRecordRetirementLedger.Open(ledgerDirectory))
                {
                    Commit(ledger, evidence, created.AddSeconds(1));
                    AssertEx.Throws<InvalidOperationException>(
                        () => ledger.CommitOperatorRetirement(
                            evidence,
                            evidence.EndpointIp,
                            evidence.EndpointPort,
                            CurrentBootId,
                            evidence.MapRevision,
                            OperatorIdentity,
                            "A different immutable reason.",
                            created.AddSeconds(2)));
                    AssertEx.Throws<InvalidOperationException>(
                        () => ledger.CommitOperatorRetirement(
                            evidence,
                            evidence.EndpointIp,
                            evidence.EndpointPort,
                            evidence.DiagnosticsBootId,
                            evidence.MapRevision,
                            OperatorIdentity,
                            RetirementReason,
                            created.AddSeconds(2)));
                }

                var entries = Directory.GetFiles(
                    ledgerDirectory,
                    "*.retired",
                    SearchOption.TopDirectoryOnly);
                AssertEx.Equal(1, entries.Length);
                var bytes = File.ReadAllBytes(entries[0]);
                bytes[bytes.Length / 2] ^= 0x5A;
                File.WriteAllBytes(entries[0], bytes);
                AssertEx.Throws<InvalidDataException>(
                    () => RecoveryRecordRetirementLedger.Open(
                        ledgerDirectory));
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void PreexistingEntryIsNotReplaced()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var journalDirectory = Path.Combine(root, "axis-power");
                var ledgerDirectory = Path.Combine(root, "ledger");
                var created = FixedUtc();
                using (var journal = AxisPowerOnRecoveryJournal.Open(
                    journalDirectory))
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    ledgerDirectory))
                {
                    journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    var entryName = ((int)evidence.Owner).ToString(
                            "D2",
                            CultureInfo.InvariantCulture)
                        + "-"
                        + evidence.RecordIdentity.ToString("N")
                        + "-"
                        + evidence.OriginalSha256
                        + ".retired";
                    var entryPath = Path.Combine(ledgerDirectory, entryName);
                    var sentinel = new byte[] { 0x19, 0x27, 0x30 };
                    File.WriteAllBytes(entryPath, sentinel);

                    AssertEx.Throws<IOException>(
                        () => Commit(
                            ledger,
                            evidence,
                            created.AddSeconds(1)));
                    AssertEx.SequenceEqual(
                        sentinel,
                        File.ReadAllBytes(entryPath));
                    AssertEx.Equal(0, ledger.CommittedDecisions.Count);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void AxisPowerExactCasAndStaleEvidence()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "ledger")))
                using (var journal = AxisPowerOnRecoveryJournal.Open(
                    Path.Combine(root, "exact")))
                {
                    journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEvidence(
                        evidence,
                        RecoveryRecordOwner.AxisPower,
                        journal.JournalFilePath);
                    var uncommitted = new RecoveryRecordRetirementDecision(
                        Guid.NewGuid(),
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        CurrentBootId,
                        evidence.MapRevision,
                        OperatorIdentity,
                        RetirementReason,
                        created.AddSeconds(1));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            uncommitted,
                            created.AddSeconds(2)));
                    AssertEx.True(journal.HasActiveRecord);
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    var resolved = journal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        created.AddSeconds(2));
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        resolved.State);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.SequenceEqual(
                        evidence.GetOriginalBytes(),
                        decision.SourceEvidence.GetOriginalBytes());
                }

                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "stale-ledger")))
                using (var journal = AxisPowerOnRecoveryJournal.Open(
                    Path.Combine(root, "stale")))
                {
                    var armed = journal.ArmBeforeDispatch(
                        true,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    journal.PromoteToRecoveryRequired(
                        armed.Identity,
                        created.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(3)));
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void
            AxisQualificationExactSourceBytesAndTombstone()
        {
            const uint diagnosticsBuild = 0x01020304u;
            var root = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                var exactJournalDirectory = Path.Combine(root, "aq-exact");
                var exactLedgerDirectory = Path.Combine(
                    root,
                    "aq-ledger");
                Guid resolvedIdentity;
                long resolvedRevision;
                byte[] resolvedChecksum;
                RecoveryJournalSourceEvidence exactEvidence;
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    exactLedgerDirectory))
                using (var journal = AxisQualificationRecoveryJournal.Open(
                    exactJournalDirectory))
                {
                    var accepted = journal.MarkPowerOnAccepted(
                        ArmAxisQualification(
                            journal,
                            diagnosticsBuild,
                            created),
                        created.AddMilliseconds(1));
                    exactEvidence =
                        journal.CaptureActiveRetirementEvidence();
                    AssertEvidence(
                        exactEvidence,
                        RecoveryRecordOwner.AxisQualification,
                        journal.JournalFilePath);
                    AssertEx.Equal(
                        diagnosticsBuild,
                        exactEvidence.DiagnosticsBuild);
                    AssertEx.Equal(
                        (int)AxisQualificationRecoveryStage.PowerOnAccepted,
                        exactEvidence.StateCode);
                    AssertEx.Equal("Axis", exactEvidence.TargetKind);
                    AssertEx.Equal("_LMCAxis1", exactEvidence.TargetName);
                    AssertEx.Equal(
                        (ushort)1,
                        exactEvidence.TargetReference);
                    AssertEx.Equal(
                        "Qualification",
                        exactEvidence.Operation);
                    AssertEx.Contains(
                        "Revision=2;SessionGeneration=7;Delta=120;Velocity=230;Acceleration=340;Deceleration=450;Jerk=0;Tolerance=5;HasTarget=false;Start=0;Target=0;SafetyGeneration=0;CrashPromoted=false",
                        exactEvidence.SemanticFingerprint);

                    var uncommitted =
                        new RecoveryRecordRetirementDecision(
                            Guid.NewGuid(),
                            exactEvidence,
                            exactEvidence.EndpointIp,
                            exactEvidence.EndpointPort,
                            exactEvidence.DiagnosticsBuild,
                            CurrentBootId,
                            exactEvidence.MapRevision,
                            OperatorIdentity,
                            RetirementReason,
                            created.AddSeconds(1));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            exactEvidence,
                            uncommitted,
                            created.AddSeconds(2)));

                    var decision = ledger.CommitOperatorRetirement(
                        exactEvidence,
                        exactEvidence.EndpointIp,
                        exactEvidence.EndpointPort,
                        exactEvidence.DiagnosticsBuild,
                        CurrentBootId,
                        exactEvidence.MapRevision,
                        OperatorIdentity,
                        RetirementReason,
                        created.AddSeconds(1));
                    AssertEx.True(decision.IsDurablyCommitted);
                    AssertEx.True(
                        decision.MatchesSourceEvidence(exactEvidence));
                    AssertEx.SequenceEqual(
                        exactEvidence.GetOriginalBytes(),
                        decision.SourceEvidence.GetOriginalBytes());
                    AssertEx.Equal(
                        exactEvidence.SemanticFingerprint,
                        decision.SourceEvidence.SemanticFingerprint);

                    var resolved = journal.ResolveOperatorRetirement(
                        exactEvidence,
                        decision,
                        created.AddSeconds(2));
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.SafeResolved,
                        resolved.Stage);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        accepted.RecordRevision + 1,
                        resolved.RecordRevision);
                    resolvedIdentity = resolved.Identity;
                    resolvedRevision = resolved.RecordRevision;
                    resolvedChecksum = resolved.Checksum;
                }

                using (var reopenedLedger =
                    RecoveryRecordRetirementLedger.Open(
                        exactLedgerDirectory))
                using (var reopenedJournal =
                    AxisQualificationRecoveryJournal.Open(
                        exactJournalDirectory))
                {
                    AssertEx.Equal(
                        1,
                        reopenedLedger.CommittedDecisions.Count);
                    var durableDecision =
                        reopenedLedger.FindPendingDecision(exactEvidence);
                    AssertEx.NotNull(durableDecision);
                    AssertEx.True(durableDecision.IsDurablyCommitted);
                    AssertEx.True(
                        durableDecision.MatchesSourceEvidence(
                            exactEvidence));
                    AssertEx.False(reopenedJournal.HasActiveRecord);
                    AssertEx.Equal(
                        resolvedIdentity,
                        reopenedJournal.CurrentRecord.Identity);
                    AssertEx.Equal(
                        resolvedRevision,
                        reopenedJournal.CurrentRecord.RecordRevision);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.SafeResolved,
                        reopenedJournal.CurrentRecord.Stage);
                    AssertEx.SequenceEqual(
                        resolvedChecksum,
                        reopenedJournal.CurrentRecord.Checksum);
                    AssertEx.Throws<InvalidOperationException>(
                        () => reopenedJournal
                            .CaptureActiveRetirementEvidence());
                }

                using (var staleLedger =
                    RecoveryRecordRetirementLedger.Open(
                        Path.Combine(root, "sl")))
                using (var staleJournal =
                    AxisQualificationRecoveryJournal.Open(
                        Path.Combine(root, "sj")))
                {
                    var accepted = staleJournal.MarkPowerOnAccepted(
                        ArmAxisQualification(
                            staleJournal,
                            diagnosticsBuild,
                            created),
                        created.AddMilliseconds(1));
                    var staleEvidence =
                        staleJournal.CaptureActiveRetirementEvidence();
                    var staleDecision =
                        staleLedger.CommitOperatorRetirement(
                            staleEvidence,
                            staleEvidence.EndpointIp,
                            staleEvidence.EndpointPort,
                            staleEvidence.DiagnosticsBuild,
                            CurrentBootId,
                            staleEvidence.MapRevision,
                            OperatorIdentity,
                            RetirementReason,
                            created.AddSeconds(1));
                    staleJournal.MarkPowerOnStable(
                        accepted,
                        created.AddMilliseconds(2));
                    var currentEvidence =
                        staleJournal.CaptureActiveRetirementEvidence();
                    AssertEx.False(
                        staleEvidence.ExactSourceEquals(currentEvidence));
                    AssertEx.False(string.Equals(
                        staleEvidence.SemanticFingerprint,
                        currentEvidence.SemanticFingerprint,
                        StringComparison.Ordinal));
                    AssertEx.False(string.Equals(
                        staleEvidence.OriginalSha256,
                        currentEvidence.OriginalSha256,
                        StringComparison.Ordinal));
                    AssertEx.Throws<InvalidOperationException>(
                        () => staleJournal.ResolveOperatorRetirement(
                            staleEvidence,
                            staleDecision,
                            created.AddSeconds(2)));
                    AssertEx.True(staleJournal.HasActiveRecord);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnStable,
                        staleJournal.CurrentRecord.Stage);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static AxisQualificationRecoveryRecord
            ArmAxisQualification(
                AxisQualificationRecoveryJournal journal,
                uint diagnosticsBuild,
                DateTime createdUtc)
        {
            return journal.ArmBeforePowerOn(
                "127.0.0.1",
                4000,
                7,
                "_LMCAxis1",
                1,
                diagnosticsBuild,
                StoredBootId,
                StoredMapRevision,
                120,
                230,
                340,
                450,
                0,
                5,
                0,
                createdUtc);
        }

        private static void AxisCommandExactCasAndStaleEvidence()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "ledger")))
                using (var journal = AxisCommandRecoveryJournal.Open(
                    Path.Combine(root, "exact")))
                {
                    journal.ArmBeforeDispatch(
                        AxisCommandRecoveryOperation.Stop,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        1000,
                        200,
                        3,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEvidence(
                        evidence,
                        RecoveryRecordOwner.AxisCommand,
                        journal.JournalFilePath);
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    AssertEx.Equal(
                        AxisCommandRecoveryState.Resolved,
                        journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(2)).State);
                    AssertEx.False(journal.HasActiveRecord);
                }

                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "stale-ledger")))
                using (var journal = AxisCommandRecoveryJournal.Open(
                    Path.Combine(root, "stale")))
                {
                    var armed = journal.ArmBeforeDispatch(
                        AxisCommandRecoveryOperation.Reset,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        0,
                        0,
                        3,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    journal.PromoteToRecoveryRequired(
                        armed.Identity,
                        created.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(3)));
                    AssertEx.Equal(
                        AxisCommandRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void MotionExactCasAndStaleEvidence()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "ledger")))
                using (var journal = MotionUncertaintyJournal.Open(
                    Path.Combine(root, "exact")))
                {
                    journal.ArmBeforeDispatch(
                        "127.0.0.1",
                        4000,
                        MotionUncertaintyTargetKind.Axis,
                        "_LMCAxis1",
                        1,
                        "MoveAbsolute",
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEvidence(
                        evidence,
                        RecoveryRecordOwner.Motion,
                        journal.JournalFilePath);
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    AssertEx.Equal(
                        MotionUncertaintyState.Resolved,
                        journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(2)).State);
                }

                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "stale-ledger")))
                using (var journal = MotionUncertaintyJournal.Open(
                    Path.Combine(root, "stale")))
                {
                    var armed = journal.ArmBeforeDispatch(
                        "127.0.0.1",
                        4000,
                        MotionUncertaintyTargetKind.Group,
                        "_LMCGroup1",
                        1,
                        "MoveLinearAbsolute",
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    journal.PromoteToRecoveryRequired(
                        armed.Identity,
                        created.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(3)));
                    AssertEx.Equal(
                        MotionUncertaintyState.RecoveryRequired,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void GroupProfileExactCasAndStaleEvidence()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "ledger")))
                using (var journal = GroupProfileLockRecoveryJournal.Open(
                    Path.Combine(root, "exact")))
                {
                    journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        4000,
                        "_LMCGroup1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEvidence(
                        evidence,
                        RecoveryRecordOwner.GroupProfileLock,
                        journal.JournalFilePath);
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.Resolved,
                        journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(2)).State);
                }

                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "stale-ledger")))
                using (var journal = GroupProfileLockRecoveryJournal.Open(
                    Path.Combine(root, "stale")))
                {
                    var armed = journal.ArmBeforeDispatch(
                        true,
                        "127.0.0.1",
                        4000,
                        "_LMCGroup1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    journal.PromoteToRecoveryRequired(
                        armed.Identity,
                        created.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(3)));
                    AssertEx.Equal(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static void GroupPowerExactCasAndStaleEvidence()
        {
            var root = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "ledger")))
                using (var journal = GroupPowerRecoveryJournal.Open(
                    Path.Combine(root, "exact")))
                {
                    journal.ArmBeforeDispatch(
                        false,
                        "127.0.0.1",
                        4000,
                        "_LMCGroup1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEvidence(
                        evidence,
                        RecoveryRecordOwner.GroupPower,
                        journal.JournalFilePath);
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    AssertEx.Equal(
                        GroupPowerRecoveryState.Resolved,
                        journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(2)).State);
                }

                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(root, "stale-ledger")))
                using (var journal = GroupPowerRecoveryJournal.Open(
                    Path.Combine(root, "stale")))
                {
                    var armed = journal.ArmBeforeDispatch(
                        true,
                        "127.0.0.1",
                        4000,
                        "_LMCGroup1",
                        1,
                        StoredBootId,
                        StoredMapRevision,
                        created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    var decision = Commit(
                        ledger,
                        evidence,
                        created.AddSeconds(1));
                    journal.PromoteToRecoveryRequired(
                        armed.Identity,
                        created.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(3)));
                    AssertEx.Equal(
                        GroupPowerRecoveryState.RecoveryRequired,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(root);
            }
        }

        private static RecoveryRecordRetirementDecision Commit(
            RecoveryRecordRetirementLedger ledger,
            RecoveryJournalSourceEvidence evidence,
            DateTime decisionUtc)
        {
            return ledger.CommitOperatorRetirement(
                evidence,
                evidence.EndpointIp,
                evidence.EndpointPort,
                CurrentBootId,
                evidence.MapRevision,
                OperatorIdentity,
                RetirementReason,
                decisionUtc);
        }

        private static void AssertEvidence(
            RecoveryJournalSourceEvidence evidence,
            RecoveryRecordOwner expectedOwner,
            string journalPath)
        {
            AssertEx.NotNull(evidence);
            AssertEx.Equal(expectedOwner, evidence.Owner);
            AssertEx.Equal(StoredBootId, evidence.DiagnosticsBootId);
            AssertEx.Equal(StoredMapRevision, evidence.MapRevision);
            AssertEx.SequenceEqual(
                File.ReadAllBytes(journalPath),
                evidence.GetOriginalBytes());
            AssertEx.Equal(
                RecoveryJournalSourceEvidence.ComputeSha256(
                    evidence.GetOriginalBytes()),
                evidence.OriginalSha256);
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(638895000000000000L, DateTimeKind.Utc);
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoRecoveryRecordRetirementTests",
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
    }
}

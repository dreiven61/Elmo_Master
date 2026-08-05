using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class GroupResetRecoveryJournalTests
    {
        private const int VersionOffset = 8;
        private const int ChecksumLength = 32;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.GroupResetJournal.DefaultPath",
                DefaultPathIsUniqueAndVersioned);
            tests.Add(
                "Wpf.GroupResetJournal.Lifecycle",
                LifecycleIsAtomicAndOutcomeAware);
            tests.Add(
                "Wpf.GroupResetJournal.AcceptedReopen",
                AcceptedRecordReopensDeterministically);
            tests.Add(
                "Wpf.GroupResetJournal.ArmedRestart",
                ArmedRestartPromotesToUncertainRecovery);
            tests.Add(
                "Wpf.GroupResetJournal.IdentityMembers",
                EndpointIdentityAndOrderedMembersAreExact);
            tests.Add(
                "Wpf.GroupResetJournal.CorruptionVersionChecksum",
                CorruptionVersionAndChecksumFailClosed);
            tests.Add(
                "Wpf.GroupResetJournal.SingleWriter",
                SecondWriterAndActiveOverwriteAreBlocked);
            tests.Add(
                "Wpf.GroupResetJournal.ImmutableCopies",
                MemberAndChecksumCopiesAreDefensive);
            tests.Add(
                "Wpf.GroupResetJournal.InvalidSemantics",
                InvalidSemanticFieldsFailClosed);
            tests.Add(
                "Wpf.GroupResetJournal.ExactCas",
                ExactCasRejectsStaleAndRepeatedTransitions);
            tests.Add(
                "Wpf.GroupResetJournal.RetirementBuildOnlyMismatchExactCas",
                RetirementBuildOnlyMismatchCommitsAndResolvesExactSource);
            tests.Add(
                "Wpf.GroupResetJournal.RetirementStaleBytesFailClosed",
                RetirementStaleSourceBytesFailClosed);
        }

        private static void
            RetirementBuildOnlyMismatchCommitsAndResolvesExactSource()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                var journalPath = Path.Combine(directory, "journal");
                var ledgerPath = Path.Combine(directory, "ledger");
                RecoveryRecordRetirementDecision committed;
                using (var journal = GroupResetRecoveryJournal.Open(
                    journalPath))
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    ledgerPath))
                {
                    Arm(journal, created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    AssertEx.Equal(
                        0x01020304U,
                        evidence.DiagnosticsBuild);
                    AssertEx.SequenceEqual(
                        File.ReadAllBytes(journal.JournalFilePath),
                        evidence.GetOriginalBytes());

                    AssertEx.Throws<InvalidOperationException>(
                        () => ledger.CommitOperatorRetirement(
                            evidence,
                            evidence.EndpointIp,
                            evidence.EndpointPort,
                            evidence.DiagnosticsBuild,
                            evidence.DiagnosticsBootId,
                            evidence.MapRevision,
                            "TEST\\operator",
                            "Build-only Group Reset retirement test.",
                            created.AddSeconds(1)));

                    var uncommitted =
                        new RecoveryRecordRetirementDecision(
                            Guid.NewGuid(),
                            evidence,
                            evidence.EndpointIp,
                            evidence.EndpointPort,
                            evidence.DiagnosticsBuild + 1,
                            evidence.DiagnosticsBootId,
                            evidence.MapRevision,
                            "TEST\\operator",
                            "Build-only Group Reset retirement test.",
                            created.AddSeconds(1));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            uncommitted,
                            created.AddSeconds(2)));

                    committed = ledger.CommitOperatorRetirement(
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        evidence.DiagnosticsBuild + 1,
                        evidence.DiagnosticsBootId,
                        evidence.MapRevision,
                        "TEST\\operator",
                        "Build-only Group Reset retirement test.",
                        created.AddSeconds(1));
                    AssertEx.True(committed.IsDurablyCommitted);
                    AssertEx.Equal(
                        evidence.DiagnosticsBuild + 1,
                        committed.CurrentDiagnosticsBuild);
                    var resolved = journal.ResolveOperatorRetirement(
                        evidence,
                        committed,
                        created.AddSeconds(2));
                    AssertEx.Equal(
                        GroupResetRecoveryState.Resolved,
                        resolved.State);
                    AssertEx.False(journal.HasActiveRecord);
                }

                using (var reopened = RecoveryRecordRetirementLedger.Open(
                    ledgerPath))
                {
                    AssertEx.Equal(1, reopened.CommittedDecisions.Count);
                    AssertEx.Equal(
                        committed.CurrentDiagnosticsBuild,
                        reopened.CommittedDecisions[0]
                            .CurrentDiagnosticsBuild);
                    AssertEx.Equal(
                        0x01020304U,
                        reopened.CommittedDecisions[0]
                            .SourceEvidence.DiagnosticsBuild);
                }
            });
        }

        private static void RetirementStaleSourceBytesFailClosed()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                using (var journal = GroupResetRecoveryJournal.Open(
                    Path.Combine(directory, "journal")))
                using (var ledger = RecoveryRecordRetirementLedger.Open(
                    Path.Combine(directory, "ledger")))
                {
                    var armed = Arm(journal, created);
                    var evidence = journal.CaptureActiveRetirementEvidence();
                    var decision = ledger.CommitOperatorRetirement(
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        evidence.DiagnosticsBuild + 1,
                        evidence.DiagnosticsBootId,
                        evidence.MapRevision,
                        "TEST\\operator",
                        "Stale Group Reset source rejection test.",
                        created.AddSeconds(1));
                    journal.MarkAccepted(
                        armed,
                        created.AddMilliseconds(500));

                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            decision,
                            created.AddSeconds(2)));
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        GroupResetRecoveryState.AcceptedAwaitingProof,
                        journal.CurrentRecord.State);
                }
            });
        }

        private static void DefaultPathIsUniqueAndVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "GroupResetRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                GroupResetRecoveryJournal.GetDefaultDirectoryPath()
                    .ToUpperInvariant());
        }

        private static void LifecycleIsAtomicAndOutcomeAware()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, created);
                    AssertRecord(
                        armed,
                        GroupResetRecoveryState.ArmedBeforeDispatch,
                        GroupResetRecoveryPriorOutcome.NotAttempted,
                        1,
                        created);
                    var accepted = journal.MarkAccepted(
                        armed,
                        created.AddSeconds(1));
                    AssertRecord(
                        accepted,
                        GroupResetRecoveryState.AcceptedAwaitingProof,
                        GroupResetRecoveryPriorOutcome.Accepted,
                        2,
                        created.AddSeconds(1));
                    var recovery = journal.PromoteRecoveryRequired(
                        accepted,
                        created.AddSeconds(2));
                    AssertRecord(
                        recovery,
                        GroupResetRecoveryState.RecoveryRequired,
                        GroupResetRecoveryPriorOutcome.Accepted,
                        3,
                        created.AddSeconds(2));
                    var resolved = journal.Resolve(
                        recovery,
                        created.AddSeconds(3));
                    AssertRecord(
                        resolved,
                        GroupResetRecoveryState.Resolved,
                        GroupResetRecoveryPriorOutcome.Accepted,
                        4,
                        created.AddSeconds(3));
                    AssertEx.False(journal.HasActiveRecord);
                }

                using (var reopened =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    AssertEx.Equal(
                        GroupResetRecoveryState.Resolved,
                        reopened.Current.State);
                    AssertEx.Equal(4L, reopened.Current.RecordRevision);
                    AssertEx.False(reopened.HasActiveRecord);
                }
            });
        }

        private static void AcceptedRecordReopensDeterministically()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                Guid identity;
                byte[] checksum;
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var accepted = journal.MarkAccepted(
                        Arm(journal, created),
                        created.AddMilliseconds(1));
                    identity = accepted.Identity;
                    checksum = accepted.Checksum;
                }

                using (var reopened =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var record = reopened.Current;
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, record.Identity);
                    AssertEx.Equal(2L, record.RecordRevision);
                    AssertEx.Equal(
                        GroupResetRecoveryState.AcceptedAwaitingProof,
                        record.State);
                    AssertEx.Equal(
                        GroupResetRecoveryPriorOutcome.Accepted,
                        record.PriorOutcome);
                    AssertEx.SequenceEqual(checksum, record.Checksum);
                    AssertExactIdentity(record);
                }
            });
        }

        private static void ArmedRestartPromotesToUncertainRecovery()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    Arm(journal, created);
                }

                using (var reopened =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var armed = reopened.Current;
                    AssertEx.Equal(
                        GroupResetRecoveryPriorOutcome.NotAttempted,
                        armed.PriorOutcome);
                    var recovery = reopened.PromoteRecoveryRequired(
                        armed,
                        created.AddSeconds(1));
                    AssertEx.Equal(
                        GroupResetRecoveryState.RecoveryRequired,
                        recovery.State);
                    AssertEx.Equal(
                        GroupResetRecoveryPriorOutcome.OutcomeUncertain,
                        recovery.PriorOutcome);
                    AssertEx.Equal(2L, recovery.RecordRevision);
                }

                using (var reopenedAgain =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    AssertEx.Equal(
                        GroupResetRecoveryPriorOutcome.OutcomeUncertain,
                        reopenedAgain.Current.PriorOutcome);
                }
            });
        }

        private static void EndpointIdentityAndOrderedMembersAreExact()
        {
            WithTestDirectory(delegate(string directory)
            {
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var record = Arm(journal, FixedUtc());
                    AssertEx.Equal("127.0.0.1", record.PlcIp);
                    AssertEx.Equal("10.0.0.1", record.LocalIpv4);
                    AssertEx.True(record.MatchesEndpoint(
                        "127.1",
                        4000,
                        "10.1",
                        5000));
                    AssertEx.False(record.MatchesEndpoint(
                        "127.1",
                        4001,
                        "10.1",
                        5000));
                    AssertEx.False(record.MatchesEndpoint(
                        "::1",
                        4000,
                        "10.1",
                        5000));
                    AssertExactIdentity(record);

                    AssertEx.False(Matches(
                        record,
                        MembersReversed(),
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        9,
                        3));
                    AssertEx.False(Matches(
                        record,
                        Members(),
                        0x01020305U,
                        0x11223344U,
                        0x55667788U,
                        9,
                        3));
                    AssertEx.False(Matches(
                        record,
                        Members(),
                        0x01020304U,
                        0x11223345U,
                        0x55667788U,
                        9,
                        3));
                    AssertEx.False(Matches(
                        record,
                        Members(),
                        0x01020304U,
                        0x11223344U,
                        0x55667789U,
                        9,
                        3));
                    AssertEx.False(Matches(
                        record,
                        Members(),
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        10,
                        3));
                    AssertEx.False(Matches(
                        record,
                        Members(),
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        9,
                        4));
                    AssertEx.False(record.MatchesRecoveryIdentity(
                        "127.1",
                        4000,
                        "10.1",
                        5000,
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        "OtherGroup",
                        7,
                        9,
                        Members(),
                        3));
                }
            });
        }

        private static void CorruptionVersionAndChecksumFailClosed()
        {
            AssertMutatedJournalRejected(delegate(byte[] bytes)
            {
                bytes[20] ^= 0x5a;
            });
            AssertMutatedJournalRejected(delegate(byte[] bytes)
            {
                WriteInt32(bytes, 12, 0);
                RecomputeChecksum(bytes);
            });
            AssertMutatedJournalRejected(delegate(byte[] bytes)
            {
                WriteInt32(bytes, VersionOffset, 99);
                RecomputeChecksum(bytes);
            });
        }

        private static void SecondWriterAndActiveOverwriteAreBlocked()
        {
            WithTestDirectory(delegate(string directory)
            {
                using (var first =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var armed = Arm(first, FixedUtc());
                    AssertEx.Throws<IOException>(
                        () => GroupResetRecoveryJournal.Open(directory));
                    AssertEx.Throws<InvalidOperationException>(
                        () => first.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            4000,
                            "10.1",
                            5000,
                            0x01020304U,
                            0x11223344U,
                            0x55667788U,
                            "OtherGroup",
                            8,
                            9,
                            Members(),
                            3,
                            FixedUtc().AddSeconds(1)));
                    AssertEx.Equal(armed.Identity, first.Current.Identity);
                }
            });
        }

        private static void MemberAndChecksumCopiesAreDefensive()
        {
            WithTestDirectory(delegate(string directory)
            {
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var input = Members();
                    var record = journal.ArmBeforeDispatch(
                        Guid.NewGuid(),
                        "127.1",
                        4000,
                        "10.1",
                        5000,
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        "ResetGroup",
                        7,
                        9,
                        input,
                        3,
                        FixedUtc());
                    input[0] = new GroupResetRecoveryMember("Changed", 10, 10);
                    var exposed = record.Members;
                    exposed[0] = new GroupResetRecoveryMember("ChangedAgain", 11, 11);
                    var checksum = record.Checksum;
                    checksum[0] ^= 0xff;

                    var current = journal.Current;
                    AssertEx.Equal("AxisOne", current.Members[0].AxisName);
                    AssertEx.Equal((ushort)1, current.Members[0].AxisReference);
                    AssertEx.False(object.ReferenceEquals(record, current));
                    AssertEx.False(object.ReferenceEquals(
                        record.Members,
                        current.Members));
                    AssertEx.False(checksum[0] == current.Checksum[0]);
                }
            });
        }

        private static void InvalidSemanticFieldsFailClosed()
        {
            WithTestDirectory(delegate(string directory)
            {
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    AssertArmInvalid(journal, null, Members(), 3, 1, 1, 1, 9);
                    AssertArmInvalid(journal, "::1", Members(), 3, 1, 1, 1, 9);
                    AssertArmInvalid(journal, "127.1 ", Members(), 3, 1, 1, 1, 9);
                    AssertArmInvalid(journal, "127.1", Members(), 0, 1, 1, 1, 9);
                    AssertArmInvalid(journal, "127.1", Members(), 101, 1, 1, 1, 9);
                    AssertArmInvalid(journal, "127.1", Members(), 3, 0, 1, 1, 9);
                    AssertArmInvalid(journal, "127.1", Members(), 3, 1, 0, 1, 9);
                    AssertArmInvalid(journal, "127.1", Members(), 3, 1, 1, 0, 9);
                    AssertArmInvalid(journal, "127.1", Members(), 3, 1, 1, 1, 0);
                    AssertArmInvalid(journal, "127.1", new GroupResetRecoveryMember[0], 3, 1, 1, 1, 9);
                    AssertArmInvalid(journal, "127.1", SeventeenMembers(), 3, 1, 1, 1, 9);
                    AssertArmInvalid(journal, "127.1", DuplicateMembers(), 3, 1, 1, 1, 9);
                    AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                        journal.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            0,
                            "10.1",
                            5000,
                            1,
                            1,
                            1,
                            "ResetGroup",
                            7,
                            9,
                            Members(),
                            3,
                            FixedUtc()));
                    AssertEx.Throws<ArgumentException>(() =>
                        journal.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            4000,
                            "::1",
                            5000,
                            1,
                            1,
                            1,
                            "ResetGroup",
                            7,
                            9,
                            Members(),
                            3,
                            FixedUtc()));
                    AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                        journal.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            4000,
                            "10.1",
                            65536,
                            1,
                            1,
                            1,
                            "ResetGroup",
                            7,
                            9,
                            Members(),
                            3,
                            FixedUtc()));
                    AssertEx.Throws<ArgumentException>(() =>
                        journal.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            4000,
                            "10.1",
                            5000,
                            1,
                            1,
                            1,
                            " BadGroup",
                            7,
                            9,
                            Members(),
                            3,
                            FixedUtc()));
                    AssertEx.Throws<ArgumentException>(() =>
                        journal.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            4000,
                            "10.1",
                            5000,
                            1,
                            1,
                            1,
                            "Group\u0080",
                            7,
                            9,
                            Members(),
                            3,
                            FixedUtc()));
                    AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                        journal.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            4000,
                            "10.1",
                            5000,
                            1,
                            1,
                            1,
                            "ResetGroup",
                            0,
                            9,
                            Members(),
                            3,
                            FixedUtc()));
                    AssertEx.Throws<ArgumentException>(() =>
                        journal.ArmBeforeDispatch(
                            Guid.NewGuid(),
                            "127.1",
                            4000,
                            "10.1",
                            5000,
                            1,
                            1,
                            1,
                            "ResetGroup",
                            7,
                            9,
                            Members(),
                            3,
                            DateTime.SpecifyKind(
                                FixedUtc(),
                                DateTimeKind.Unspecified)));
                    AssertEx.Throws<ArgumentOutOfRangeException>(
                        () => new GroupResetRecoveryMember("Axis", 0, 0));
                    AssertEx.Throws<ArgumentException>(
                        () => new GroupResetRecoveryMember(" Bad", 1, 0));
                    AssertEx.Throws<ArgumentException>(
                        () => new GroupResetRecoveryMember("Axis\u0080", 1, 0));
                    AssertEx.False(journal.HasActiveRecord);
                }
            });

            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRawRecord(
                    (GroupResetRecoveryState)99,
                    GroupResetRecoveryPriorOutcome.NotAttempted));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => CreateRawRecord(
                    GroupResetRecoveryState.ArmedBeforeDispatch,
                    (GroupResetRecoveryPriorOutcome)99));
            AssertEx.Throws<ArgumentException>(
                () => CreateRawRecord(
                    GroupResetRecoveryState.ArmedBeforeDispatch,
                    GroupResetRecoveryPriorOutcome.Accepted));
            AssertEx.Throws<ArgumentException>(
                () => CreateRawRecord(
                    GroupResetRecoveryState.AcceptedAwaitingProof,
                    GroupResetRecoveryPriorOutcome.OutcomeUncertain));
            AssertEx.Throws<ArgumentException>(
                () => CreateRawRecord(
                    GroupResetRecoveryState.RecoveryRequired,
                    GroupResetRecoveryPriorOutcome.NotAttempted));
        }

        private static void ExactCasRejectsStaleAndRepeatedTransitions()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, created);
                    var counterfeit = new GroupResetRecoveryRecord(
                        armed.Identity,
                        armed.RecordRevision,
                        armed.State,
                        armed.PriorOutcome,
                        armed.PlcIp,
                        armed.PlcTcpPort,
                        armed.LocalIpv4,
                        armed.CallbackUdpPort,
                        armed.DiagnosticsBuild,
                        armed.DiagnosticsBootId,
                        armed.MapRevision,
                        armed.GroupName,
                        armed.GroupReference,
                        armed.OwnerSessionGeneration + 1,
                        armed.Members,
                        armed.RequiredStableSampleCount,
                        armed.CreatedUtc,
                        armed.UpdatedUtc,
                        armed.Checksum);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.MarkAccepted(
                            counterfeit,
                            created.AddSeconds(1)));
                    var accepted = journal.MarkAccepted(
                        armed,
                        created.AddSeconds(1));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.MarkAccepted(
                            armed,
                            created.AddSeconds(2)));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.PromoteRecoveryRequired(
                            armed,
                            created.AddSeconds(2)));

                    var recovery = journal.PromoteRecoveryRequired(
                        accepted,
                        created.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.Resolve(
                            accepted,
                            created.AddSeconds(3)));
                    var resolved = journal.Resolve(
                        recovery,
                        created.AddSeconds(3));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.Resolve(
                            recovery,
                            created.AddSeconds(4)));
                    AssertEx.Equal(4L, resolved.RecordRevision);

                    var replacement = journal.ArmBeforeDispatch(
                        Guid.NewGuid(),
                        "127.1",
                        4000,
                        "10.1",
                        5000,
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        "ResetGroup",
                        7,
                        10,
                        Members(),
                        3,
                        created.AddSeconds(4));
                    AssertEx.Equal(1L, replacement.RecordRevision);
                }
            });
        }

        private static GroupResetRecoveryRecord Arm(
            GroupResetRecoveryJournal journal,
            DateTime createdUtc)
        {
            return journal.ArmBeforeDispatch(
                Guid.NewGuid(),
                "127.1",
                4000,
                "10.1",
                5000,
                0x01020304U,
                0x11223344U,
                0x55667788U,
                "ResetGroup",
                7,
                9,
                Members(),
                3,
                createdUtc);
        }

        private static void AssertRecord(
            GroupResetRecoveryRecord record,
            GroupResetRecoveryState state,
            GroupResetRecoveryPriorOutcome outcome,
            long revision,
            DateTime updatedUtc)
        {
            AssertEx.Equal(state, record.State);
            AssertEx.Equal(outcome, record.PriorOutcome);
            AssertEx.Equal(revision, record.RecordRevision);
            AssertEx.Equal(updatedUtc, record.UpdatedUtc);
            AssertEx.Equal(DateTimeKind.Utc, record.CreatedUtc.Kind);
            AssertEx.Equal(DateTimeKind.Utc, record.UpdatedUtc.Kind);
            AssertEx.Equal(ChecksumLength, record.Checksum.Length);
        }

        private static void AssertExactIdentity(
            GroupResetRecoveryRecord record)
        {
            AssertEx.True(Matches(
                record,
                Members(),
                0x01020304U,
                0x11223344U,
                0x55667788U,
                9,
                3));
            AssertEx.Equal(4000, record.PlcTcpPort);
            AssertEx.Equal(5000, record.CallbackUdpPort);
            AssertEx.Equal("ResetGroup", record.GroupName);
            AssertEx.Equal((ushort)7, record.GroupReference);
            AssertEx.Equal(9L, record.OwnerSessionGeneration);
            AssertEx.Equal(3, record.RequiredStableSampleCount);
        }

        private static bool Matches(
            GroupResetRecoveryRecord record,
            GroupResetRecoveryMember[] members,
            uint diagnosticsBuild,
            uint bootId,
            uint mapRevision,
            long ownerSessionGeneration,
            int stableSamples)
        {
            return record.MatchesRecoveryIdentity(
                "127.1",
                4000,
                "10.1",
                5000,
                diagnosticsBuild,
                bootId,
                mapRevision,
                "ResetGroup",
                7,
                ownerSessionGeneration,
                members,
                stableSamples);
        }

        private static GroupResetRecoveryMember[] Members()
        {
            return new[]
            {
                new GroupResetRecoveryMember("AxisOne", 1, 101),
                new GroupResetRecoveryMember("AxisTwo", 2, 102)
            };
        }

        private static GroupResetRecoveryMember[] MembersReversed()
        {
            var members = Members();
            return new[] { members[1], members[0] };
        }

        private static GroupResetRecoveryMember[] DuplicateMembers()
        {
            return new[]
            {
                new GroupResetRecoveryMember("AxisOne", 1, 101),
                new GroupResetRecoveryMember("AxisDuplicate", 1, 102)
            };
        }

        private static GroupResetRecoveryMember[] SeventeenMembers()
        {
            var members = new GroupResetRecoveryMember[17];
            for (var index = 0; index < members.Length; index++)
            {
                members[index] = new GroupResetRecoveryMember(
                    "Axis" + (index + 1),
                    checked((ushort)(index + 1)),
                    checked((ushort)(100 + index)));
            }
            return members;
        }

        private static void AssertArmInvalid(
            GroupResetRecoveryJournal journal,
            string plcIp,
            GroupResetRecoveryMember[] members,
            int stableCount,
            uint build,
            uint bootId,
            uint mapRevision,
            long ownerSessionGeneration)
        {
            AssertEx.Throws<ArgumentException>(() =>
                journal.ArmBeforeDispatch(
                    Guid.NewGuid(),
                    plcIp,
                    4000,
                    "10.1",
                    5000,
                    build,
                    bootId,
                    mapRevision,
                    "ResetGroup",
                    7,
                    ownerSessionGeneration,
                    members,
                    stableCount,
                    FixedUtc()));
        }

        private static GroupResetRecoveryRecord CreateRawRecord(
            GroupResetRecoveryState state,
            GroupResetRecoveryPriorOutcome outcome)
        {
            return new GroupResetRecoveryRecord(
                Guid.NewGuid(),
                1,
                state,
                outcome,
                "127.0.0.1",
                4000,
                "10.0.0.1",
                5000,
                1,
                2,
                3,
                "ResetGroup",
                7,
                9,
                Members(),
                3,
                FixedUtc(),
                FixedUtc(),
                null);
        }

        private static void AssertMutatedJournalRejected(
            Action<byte[]> mutation)
        {
            WithTestDirectory(delegate(string directory)
            {
                using (var journal =
                    GroupResetRecoveryJournal.Open(directory))
                {
                    Arm(journal, FixedUtc());
                }
                var path = Path.Combine(
                    directory,
                    GroupResetRecoveryJournal.JournalFileName);
                var bytes = File.ReadAllBytes(path);
                mutation(bytes);
                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(
                    () => GroupResetRecoveryJournal.Open(directory));
            });
        }

        private static void RecomputeChecksum(byte[] bytes)
        {
            var checksumOffset = bytes.Length - ChecksumLength;
            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(bytes, 0, checksumOffset);
            }
            Buffer.BlockCopy(
                checksum,
                0,
                bytes,
                checksumOffset,
                ChecksumLength);
        }

        private static void WriteInt32(byte[] bytes, int offset, int value)
        {
            bytes[offset] = unchecked((byte)value);
            bytes[offset + 1] = unchecked((byte)(value >> 8));
            bytes[offset + 2] = unchecked((byte)(value >> 16));
            bytes[offset + 3] = unchecked((byte)(value >> 24));
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);
        }

        private static void WithTestDirectory(Action<string> body)
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "ElmoGroupResetJournalTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                body(directory);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }
    }
}

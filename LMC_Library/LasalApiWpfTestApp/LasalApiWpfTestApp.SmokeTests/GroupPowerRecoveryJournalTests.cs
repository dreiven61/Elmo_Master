using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class GroupPowerRecoveryJournalTests
    {
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 16384;
        private const int VersionOffset = 8;
        private const int PayloadLengthOffset = 12;
        private const int IdentityOffset = 16;
        private const int StateOffset = 32;
        private const int DirectionOffset = 36;
        private const int CreatedUtcTicksOffset = 37;
        private const int UpdatedUtcTicksOffset = 45;
        private const int BootIdOffset = 53;
        private const int MapRevisionOffset = 57;
        private const int EndpointPortOffset = 61;
        private const int GroupReferenceOffset = 65;
        private const int EndpointTextLengthOffset = 67;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.GroupPowerJournal.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.GroupPowerJournal.PowerOnRoundtrip",
                PowerOnRoundtripSurvivesReopen);
            tests.Add(
                "Wpf.GroupPowerJournal.PowerOffRoundtrip",
                PowerOffRoundtripSurvivesReopen);
            tests.Add(
                "Wpf.GroupPowerJournal.Transitions",
                DirectionSpecificTransitionsAreEnforced);
            tests.Add(
                "Wpf.GroupPowerJournal.AtomicOnToOffTakeover",
                AtomicOnToOffTakeoverSurvivesReopen);
            tests.Add(
                "Wpf.GroupPowerJournal.AtomicTakeoverFailure",
                FailedAtomicTakeoverPreservesCurrentRecord);
            tests.Add(
                "Wpf.GroupPowerJournal.IdentityAndEndpoint",
                IdentityDirectionAndEndpointAreFailClosed);
            tests.Add(
                "Wpf.GroupPowerJournal.ActiveRecordAndWriter",
                ActiveRecordAndSecondWriterAreBlocked);
            tests.Add(
                "Wpf.GroupPowerJournal.CorruptionBounds",
                CorruptionTruncationAndOversizeFailClosed);
            tests.Add(
                "Wpf.GroupPowerJournal.InvalidSemantics",
                InvalidSemanticFieldsFailClosed);
        }

        private static void DefaultPathIsVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "GroupPowerRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                GroupPowerRecoveryJournal.GetDefaultDirectoryPath()
                    .ToUpperInvariant());
        }

        private static void PowerOnRoundtripSurvivesReopen()
        {
            AssertDirectionRoundtrip(true, "PowerOnGroup");
        }

        private static void PowerOffRoundtripSurvivesReopen()
        {
            AssertDirectionRoundtrip(false, "PowerOffGroup");
        }

        private static void AssertDirectionRoundtrip(
            bool expectedPowerOn,
            string groupName)
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var identity = Guid.NewGuid();
                var createdUtc = FixedUtc();
                using (var journal =
                    GroupPowerRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        identity,
                        expectedPowerOn,
                        "127.1",
                        4000,
                        groupName,
                        7,
                        0x11223344U,
                        0x55667788U,
                        createdUtc);
                    AssertEx.Equal(
                        "127.0.0.1",
                        armed.EndpointIp);
                    journal.MarkAccepted(
                        identity,
                        createdUtc.AddSeconds(1));
                }

                using (var reopened =
                    GroupPowerRecoveryJournal.Open(directory))
                {
                    var record = reopened.CurrentRecord;
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, record.Identity);
                    AssertEx.Equal(expectedPowerOn, record.ExpectedPowerOn);
                    AssertEx.Equal("127.0.0.1", record.EndpointIp);
                    AssertEx.Equal(4000, record.EndpointPort);
                    AssertEx.Equal(groupName, record.GroupName);
                    AssertEx.Equal((ushort)7, record.GroupReference);
                    AssertEx.Equal(0x11223344U, record.DiagnosticsBootId);
                    AssertEx.Equal(0x55667788U, record.MapRevision);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.AcceptedAwaitingProof,
                        record.State);
                    AssertEx.Equal(createdUtc, record.CreatedUtc);
                    AssertEx.Equal(
                        createdUtc.AddSeconds(1),
                        record.UpdatedUtc);
                    AssertEx.Equal(
                        DateTimeKind.Utc,
                        record.CreatedUtc.Kind);
                    AssertEx.Equal(
                        DateTimeKind.Utc,
                        record.UpdatedUtc.Kind);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void DirectionSpecificTransitionsAreEnforced()
        {
            var onDirectory = CreateTemporaryDirectory();
            var offDirectory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = FixedUtc();
                using (var journal =
                    GroupPowerRecoveryJournal.Open(onDirectory))
                {
                    var on = journal.ArmBeforeDispatch(
                        true,
                        "192.168.1.10",
                        5000,
                        "OnTransitions",
                        1,
                        11,
                        22,
                        createdUtc);
                    journal.MarkAccepted(
                        on.Identity,
                        createdUtc.AddSeconds(1));
                    journal.PromoteToRecoveryRequired(
                        on.Identity,
                        createdUtc.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.MarkAccepted(
                            on.Identity,
                            createdUtc.AddSeconds(3)));
                    journal.Resolve(
                        on.Identity,
                        createdUtc.AddSeconds(3));
                    AssertEx.False(journal.HasActiveRecord);
                }

                using (var journal =
                    GroupPowerRecoveryJournal.Open(offDirectory))
                {
                    var off = journal.ArmBeforeDispatch(
                        false,
                        "192.168.1.10",
                        5000,
                        "OffTransitions",
                        2,
                        11,
                        22,
                        createdUtc);
                    journal.PromoteToRecoveryRequired(
                        off.Identity,
                        createdUtc.AddSeconds(1));
                    var accepted = journal.MarkAccepted(
                        off.Identity,
                        createdUtc.AddSeconds(2));
                    AssertEx.Equal(
                        GroupPowerRecoveryState.AcceptedAwaitingProof,
                        accepted.State);
                    journal.Resolve(
                        off.Identity,
                        createdUtc.AddSeconds(3));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.Resolve(
                            off.Identity,
                            createdUtc.AddSeconds(4)));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(onDirectory);
                DeleteTemporaryDirectory(offDirectory);
            }
        }

        private static void AtomicOnToOffTakeoverSurvivesReopen()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = FixedUtc();
                Guid oldIdentity;
                Guid newIdentity;
                using (var journal =
                    GroupPowerRecoveryJournal.Open(directory))
                {
                    var on = journal.ArmBeforeDispatch(
                        true,
                        "10.0.0.5",
                        6000,
                        "SafetyGroup",
                        3,
                        101,
                        202,
                        createdUtc);
                    oldIdentity = on.Identity;
                    journal.MarkAccepted(
                        oldIdentity,
                        createdUtc.AddSeconds(1));

                    var off = journal
                        .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            oldIdentity,
                            "10.0.0.5",
                            6000,
                            "SafetyGroup",
                            3,
                            101,
                            202,
                            createdUtc.AddSeconds(2));
                    newIdentity = off.Identity;
                    AssertEx.False(newIdentity == oldIdentity);
                    AssertEx.False(off.ExpectedPowerOn);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.ArmedBeforeDispatch,
                        off.State);
                    AssertEx.Equal(off.Identity, journal.CurrentRecord.Identity);
                }

                using (var reopened =
                    GroupPowerRecoveryJournal.Open(directory))
                {
                    var off = reopened.CurrentRecord;
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(newIdentity, off.Identity);
                    AssertEx.False(off.ExpectedPowerOn);
                    AssertEx.Equal(
                        GroupPowerRecoveryState.ArmedBeforeDispatch,
                        off.State);
                    AssertEx.True(off.MatchesRecoveryIdentity(
                        "10.0.0.5",
                        6000,
                        "SafetyGroup",
                        3,
                        101,
                        202,
                        false));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void FailedAtomicTakeoverPreservesCurrentRecord()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = FixedUtc();
                Guid onIdentity;
                using (var journal =
                    GroupPowerRecoveryJournal.Open(directory))
                {
                    var on = journal.ArmBeforeDispatch(
                        true,
                        "10.10.0.5",
                        6001,
                        "PreserveGroup",
                        4,
                        303,
                        404,
                        createdUtc);
                    onIdentity = on.Identity;
                    journal.PromoteToRecoveryRequired(
                        on.Identity,
                        createdUtc.AddSeconds(1));

                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        on.Identity,
                        Guid.NewGuid(),
                        "10.10.0.5",
                        6001,
                        "PreserveGroup",
                        4,
                        303,
                        404,
                        createdUtc.AddSeconds(2));
                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        on.Identity,
                        on.Identity,
                        "10.10.0.6",
                        6001,
                        "PreserveGroup",
                        4,
                        303,
                        404,
                        createdUtc.AddSeconds(2));
                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        on.Identity,
                        on.Identity,
                        "10.10.0.5",
                        6002,
                        "PreserveGroup",
                        4,
                        303,
                        404,
                        createdUtc.AddSeconds(2));
                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        on.Identity,
                        on.Identity,
                        "10.10.0.5",
                        6001,
                        "OtherGroup",
                        4,
                        303,
                        404,
                        createdUtc.AddSeconds(2));
                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        on.Identity,
                        on.Identity,
                        "10.10.0.5",
                        6001,
                        "PreserveGroup",
                        5,
                        303,
                        404,
                        createdUtc.AddSeconds(2));
                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        on.Identity,
                        on.Identity,
                        "10.10.0.5",
                        6001,
                        "PreserveGroup",
                        4,
                        304,
                        404,
                        createdUtc.AddSeconds(2));
                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        on.Identity,
                        on.Identity,
                        "10.10.0.5",
                        6001,
                        "PreserveGroup",
                        4,
                        303,
                        405,
                        createdUtc.AddSeconds(2));

                    AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                        journal.ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            on.Identity,
                            "10.10.0.5",
                            6001,
                            "PreserveGroup",
                            4,
                            303,
                            404,
                            createdUtc));
                    AssertCurrentRecord(
                        journal,
                        on.Identity,
                        true,
                        GroupPowerRecoveryState.RecoveryRequired);
                }

                using (var journal =
                    GroupPowerRecoveryJournal.Open(directory))
                {
                    AssertCurrentRecord(
                        journal,
                        onIdentity,
                        true,
                        GroupPowerRecoveryState.RecoveryRequired);
                    var off = journal
                        .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            onIdentity,
                            "10.10.0.5",
                            6001,
                            "PreserveGroup",
                            4,
                            303,
                            404,
                            createdUtc.AddSeconds(2));
                    AssertEx.Throws<InvalidOperationException>(() =>
                        journal.ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            off.Identity,
                            "10.10.0.5",
                            6001,
                            "PreserveGroup",
                            4,
                            303,
                            404,
                            createdUtc.AddSeconds(3)));
                    AssertCurrentRecord(
                        journal,
                        off.Identity,
                        false,
                        GroupPowerRecoveryState.ArmedBeforeDispatch);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void AssertTakeoverRejectedAndPreserved(
            GroupPowerRecoveryJournal journal,
            Guid expectedIdentity,
            Guid suppliedIdentity,
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            AssertEx.Throws<InvalidOperationException>(() =>
                journal.ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                    suppliedIdentity,
                    endpointIp,
                    endpointPort,
                    groupName,
                    groupReference,
                    diagnosticsBootId,
                    mapRevision,
                    createdUtc));
            AssertCurrentRecord(
                journal,
                expectedIdentity,
                true,
                GroupPowerRecoveryState.RecoveryRequired);
        }

        private static void IdentityDirectionAndEndpointAreFailClosed()
        {
            var timestamp = FixedUtc();
            var record = new GroupPowerRecoveryRecord(
                Guid.NewGuid(),
                true,
                "127.1",
                4000,
                "IdentityGroup",
                9,
                501,
                601,
                GroupPowerRecoveryState.AcceptedAwaitingProof,
                timestamp,
                timestamp);

            AssertEx.Equal("127.0.0.1", record.EndpointIp);
            AssertEx.True(record.MatchesEndpoint("127.1", 4000));
            AssertEx.True(record.MatchesEndpoint("127.0.0.1", 4000));
            AssertEx.False(record.MatchesEndpoint("127.0.0.2", 4000));
            AssertEx.False(record.MatchesEndpoint("::1", 4000));
            AssertEx.False(record.MatchesEndpoint("127.0.0.1", 4001));
            AssertEx.True(record.MatchesRecoveryIdentity(
                "127.0.0.1",
                4000,
                "IdentityGroup",
                9,
                501,
                601,
                true));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "127.0.0.1",
                4000,
                "OtherGroup",
                9,
                501,
                601,
                true));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "127.0.0.1",
                4000,
                "IdentityGroup",
                10,
                501,
                601,
                true));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "127.0.0.1",
                4000,
                "IdentityGroup",
                9,
                502,
                601,
                true));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "127.0.0.1",
                4000,
                "IdentityGroup",
                9,
                501,
                602,
                true));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "127.0.0.1",
                4000,
                "IdentityGroup",
                9,
                501,
                601,
                false));

            AssertEx.Throws<ArgumentException>(() =>
                CreateRecord(Guid.Empty, true, 1, 1, 1));
            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                CreateRecord(Guid.NewGuid(), true, 0, 1, 1));
            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                CreateRecord(Guid.NewGuid(), true, 1, 0, 1));
            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                CreateRecord(Guid.NewGuid(), true, 1, 1, 0));
        }

        private static void ActiveRecordAndSecondWriterAreBlocked()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = FixedUtc();
                using (var journal =
                    GroupPowerRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        true,
                        "192.168.5.5",
                        4000,
                        "WriterGroup",
                        1,
                        7,
                        8,
                        createdUtc);
                    AssertEx.Throws<IOException>(() =>
                    {
                        using (GroupPowerRecoveryJournal.Open(directory))
                        {
                        }
                    });
                    AssertEx.Throws<InvalidOperationException>(() =>
                        journal.ArmBeforeDispatch(
                            false,
                            "192.168.5.5",
                            4000,
                            "WriterGroup",
                            1,
                            7,
                            8,
                            createdUtc.AddSeconds(1)));
                    AssertEx.Throws<InvalidOperationException>(() =>
                        journal.MarkAccepted(
                            Guid.NewGuid(),
                            createdUtc.AddSeconds(1)));
                    AssertCurrentRecord(
                        journal,
                        armed.Identity,
                        true,
                        GroupPowerRecoveryState.ArmedBeforeDispatch);
                    journal.Resolve(
                        armed.Identity,
                        createdUtc.AddSeconds(1));
                    var next = journal.ArmBeforeDispatch(
                        false,
                        "192.168.5.5",
                        4000,
                        "WriterGroup",
                        1,
                        7,
                        8,
                        createdUtc.AddSeconds(2));
                    AssertEx.False(next.Identity == armed.Identity);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void CorruptionTruncationAndOversizeFailClosed()
        {
            AssertMutatedRecordRejected(
                bytes => bytes[bytes.Length - 1] ^= 0x40,
                false);
            AssertMutatedRecordRejected(
                bytes => WriteInt32(bytes, VersionOffset, 999),
                true);
            AssertMutatedRecordRejected(
                bytes => WriteInt32(
                    bytes,
                    PayloadLengthOffset,
                    BitConverter.ToInt32(bytes, PayloadLengthOffset) + 1),
                true);

            var truncatedDirectory = CreateTemporaryDirectory();
            try
            {
                var path = CreateValidJournalFile(truncatedDirectory);
                var bytes = File.ReadAllBytes(path);
                var truncated = new byte[bytes.Length - 1];
                Buffer.BlockCopy(bytes, 0, truncated, 0, truncated.Length);
                File.WriteAllBytes(path, truncated);
                AssertEx.Throws<InvalidDataException>(() =>
                    GroupPowerRecoveryJournal.Open(truncatedDirectory));
            }
            finally
            {
                DeleteTemporaryDirectory(truncatedDirectory);
            }

            var oversizeDirectory = CreateTemporaryDirectory();
            try
            {
                File.WriteAllBytes(
                    Path.Combine(
                        oversizeDirectory,
                        GroupPowerRecoveryJournal.JournalFileName),
                    new byte[MaximumFileLength + 1]);
                AssertEx.Throws<InvalidDataException>(() =>
                    GroupPowerRecoveryJournal.Open(oversizeDirectory));
            }
            finally
            {
                DeleteTemporaryDirectory(oversizeDirectory);
            }
        }

        private static void InvalidSemanticFieldsFailClosed()
        {
            AssertMutatedRecordRejected(bytes =>
            {
                for (var index = 0; index < 16; index++)
                {
                    bytes[IdentityOffset + index] = 0;
                }
            }, true);
            AssertMutatedRecordRejected(
                bytes => WriteInt32(bytes, StateOffset, 99),
                true);
            AssertMutatedRecordRejected(
                bytes => bytes[DirectionOffset] = 2,
                true);
            AssertMutatedRecordRejected(
                bytes => WriteUInt32(bytes, BootIdOffset, 0),
                true);
            AssertMutatedRecordRejected(
                bytes => WriteUInt32(bytes, MapRevisionOffset, 0),
                true);
            AssertMutatedRecordRejected(
                bytes => WriteInt32(bytes, EndpointPortOffset, 0),
                true);
            AssertMutatedRecordRejected(
                bytes => WriteUInt16(bytes, GroupReferenceOffset, 0),
                true);
            AssertMutatedRecordRejected(
                bytes => WriteInt32(bytes, EndpointTextLengthOffset, 2048),
                true);
            AssertMutatedRecordRejected(bytes =>
            {
                var createdTicks = BitConverter.ToInt64(
                    bytes,
                    CreatedUtcTicksOffset);
                WriteInt64(
                    bytes,
                    UpdatedUtcTicksOffset,
                    createdTicks - 1);
            }, true);
        }

        private static GroupPowerRecoveryRecord CreateRecord(
            Guid identity,
            bool expectedPowerOn,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var timestamp = FixedUtc();
            return new GroupPowerRecoveryRecord(
                identity,
                expectedPowerOn,
                "127.0.0.1",
                4000,
                "ValidationGroup",
                groupReference,
                diagnosticsBootId,
                mapRevision,
                GroupPowerRecoveryState.ArmedBeforeDispatch,
                timestamp,
                timestamp);
        }

        private static void AssertCurrentRecord(
            GroupPowerRecoveryJournal journal,
            Guid identity,
            bool expectedPowerOn,
            GroupPowerRecoveryState state)
        {
            var current = journal.CurrentRecord;
            AssertEx.NotNull(current);
            AssertEx.Equal(identity, current.Identity);
            AssertEx.Equal(expectedPowerOn, current.ExpectedPowerOn);
            AssertEx.Equal(state, current.State);
        }

        private static void AssertMutatedRecordRejected(
            Action<byte[]> mutate,
            bool recomputeChecksum)
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var path = CreateValidJournalFile(directory);
                var bytes = File.ReadAllBytes(path);
                mutate(bytes);
                if (recomputeChecksum)
                {
                    RecomputeChecksum(bytes);
                }

                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(() =>
                    GroupPowerRecoveryJournal.Open(directory));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static string CreateValidJournalFile(string directory)
        {
            string path;
            using (var journal =
                GroupPowerRecoveryJournal.Open(directory))
            {
                journal.ArmBeforeDispatch(
                    true,
                    "192.168.10.20",
                    5000,
                    "CorruptionGroup",
                    2,
                    11,
                    22,
                    FixedUtc());
                path = journal.JournalFilePath;
            }

            return path;
        }

        private static void RecomputeChecksum(byte[] bytes)
        {
            var checksumOffset = bytes.Length - ChecksumLength;
            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(
                    bytes,
                    0,
                    checksumOffset);
            }

            Buffer.BlockCopy(
                checksum,
                0,
                bytes,
                checksumOffset,
                checksum.Length);
        }

        private static void WriteInt16(
            byte[] destination,
            int offset,
            short value)
        {
            Buffer.BlockCopy(
                BitConverter.GetBytes(value),
                0,
                destination,
                offset,
                sizeof(short));
        }

        private static void WriteUInt16(
            byte[] destination,
            int offset,
            ushort value)
        {
            WriteInt16(destination, offset, unchecked((short)value));
        }

        private static void WriteInt32(
            byte[] destination,
            int offset,
            int value)
        {
            Buffer.BlockCopy(
                BitConverter.GetBytes(value),
                0,
                destination,
                offset,
                sizeof(int));
        }

        private static void WriteUInt32(
            byte[] destination,
            int offset,
            uint value)
        {
            WriteInt32(destination, offset, unchecked((int)value));
        }

        private static void WriteInt64(
            byte[] destination,
            int offset,
            long value)
        {
            Buffer.BlockCopy(
                BitConverter.GetBytes(value),
                0,
                destination,
                offset,
                sizeof(long));
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(
                638894400000000000L,
                DateTimeKind.Utc);
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoGroupPowerJournalTests",
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

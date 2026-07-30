using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AxisPowerOnRecoveryJournalTests
    {
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 8192;
        private const int VersionOffset = 8;
        private const int DirectionOffset = 36;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AxisPowerOnJournal.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.AxisPowerOnJournal.AcceptedSurvivesReopen",
                AcceptedSurvivesReopen);
            tests.Add(
                "Wpf.AxisPowerOnJournal.ArmedPromotesAndResolves",
                ArmedPromotesAndResolves);
            tests.Add(
                "Wpf.AxisPowerOnJournal.ExactIdentity",
                ExactIdentityIsFailClosed);
            tests.Add(
                "Wpf.AxisPowerOnJournal.SingleWriterAndChecksum",
                SingleWriterAndChecksumAreEnforced);
            tests.Add(
                "Wpf.AxisPowerOnJournal.LegacyV1Compatibility",
                LegacyV1LoadsAsPowerOnAndUpgradesOnWrite);
            tests.Add(
                "Wpf.AxisPowerOnJournal.V2DirectionRoundtrip",
                V2DirectionRoundtripIsDeterministic);
            tests.Add(
                "Wpf.AxisPowerOnJournal.PowerOffLifecycle",
                PowerOffLifecycleIsDurable);
            tests.Add(
                "Wpf.AxisPowerOnJournal.AtomicOnToOffTakeover",
                AtomicOnToOffTakeoverSurvivesReopen);
            tests.Add(
                "Wpf.AxisPowerOnJournal.TakeoverFailurePreservesOriginal",
                FailedTakeoverPreservesOriginal);
            tests.Add(
                "Wpf.AxisPowerOnJournal.V2Bounds",
                V2BoundsAndDirectionAreFailClosed);
        }

        private static void DefaultPathIsVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisPowerOnRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                AxisPowerOnRecoveryJournal.GetDefaultDirectoryPath()
                    .ToUpperInvariant());
        }

        private static void AcceptedSurvivesReopen()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = new DateTime(
                    638893500000000000L,
                    DateTimeKind.Utc);
                Guid identity;
                using (var journal =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        7,
                        0x11223344,
                        0x55667788,
                        created);
                    identity = armed.Identity;
                    var accepted = journal.MarkAccepted(
                        identity,
                        created.AddSeconds(1));
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                        accepted.State);
                }

                using (var reopened =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    var accepted = reopened.CurrentRecord;
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, accepted.Identity);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                        accepted.State);
                    reopened.Resolve(
                        identity,
                        created.AddSeconds(2));
                }

                using (var reopened =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.Resolved,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void ArmedPromotesAndResolves()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = new DateTime(
                    638893510000000000L,
                    DateTimeKind.Utc);
                using (var journal =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        "10.0.0.5",
                        5000,
                        "AxisA",
                        2,
                        11,
                        22,
                        created);
                    var recovery = journal.PromoteToRecoveryRequired(
                        armed.Identity,
                        created.AddMilliseconds(1));
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.RecoveryRequired,
                        recovery.State);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.MarkAccepted(
                            armed.Identity,
                            created.AddMilliseconds(2)));
                    journal.Resolve(
                        armed.Identity,
                        created.AddMilliseconds(3));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void ExactIdentityIsFailClosed()
        {
            var timestamp = new DateTime(
                638893520000000000L,
                DateTimeKind.Utc);
            var record = new AxisPowerOnRecoveryRecord(
                Guid.NewGuid(),
                "192.168.1.20",
                4000,
                "_LMCAxis1",
                9,
                101,
                202,
                AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                timestamp,
                timestamp);
            AssertEx.True(record.MatchesRecoveryIdentity(
                "192.168.1.20",
                4000,
                "_LMCAxis1",
                9,
                101,
                202));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "192.168.1.21",
                4000,
                "_LMCAxis1",
                9,
                101,
                202));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "192.168.1.20",
                4000,
                "_LMCAxis1",
                10,
                101,
                202));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "192.168.1.20",
                4000,
                "_LMCAxis1",
                9,
                102,
                202));
            AssertEx.False(record.MatchesRecoveryIdentity(
                "192.168.1.20",
                4000,
                "_LMCAxis1",
                9,
                101,
                203));
        }

        private static void SingleWriterAndChecksumAreEnforced()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                string path;
                using (var journal =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    journal.ArmBeforeDispatch(
                        "127.0.0.1",
                        4000,
                        "AxisChecksum",
                        1,
                        7,
                        8,
                        DateTime.UtcNow);
                    path = journal.JournalFilePath;
                    AssertEx.Throws<IOException>(
                        () => AxisPowerOnRecoveryJournal.Open(directory));
                }

                var bytes = File.ReadAllBytes(path);
                bytes[bytes.Length - 1] ^= 0x40;
                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(
                    () => AxisPowerOnRecoveryJournal.Open(directory));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void LegacyV1LoadsAsPowerOnAndUpgradesOnWrite()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var identity = new Guid(
                    "00112233-4455-6677-8899-aabbccddeeff");
                var createdUtc = FixedUtc();
                var path = Path.Combine(
                    directory,
                    "axis-power-on-recovery.bin");
                File.WriteAllBytes(
                    path,
                    CreateLegacyV1Record(
                        identity,
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                        createdUtc,
                        createdUtc.AddSeconds(1)));

                using (var journal =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    var record = journal.CurrentRecord;
                    AssertEx.Equal(identity, record.Identity);
                    AssertEx.True(record.ExpectedPowerOn);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                        record.State);
                    journal.PromoteToRecoveryRequired(
                        identity,
                        createdUtc.AddSeconds(2));
                }

                var upgraded = File.ReadAllBytes(path);
                AssertEx.Equal(2, BitConverter.ToInt32(
                    upgraded,
                    VersionOffset));
                AssertEx.Equal((byte)1, upgraded[DirectionOffset]);
                using (var reopened =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    AssertCurrentRecord(
                        reopened,
                        identity,
                        true,
                        AxisPowerOnRecoveryState.RecoveryRequired);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void V2DirectionRoundtripIsDeterministic()
        {
            var onDirectoryA = CreateTemporaryDirectory();
            var onDirectoryB = CreateTemporaryDirectory();
            var offDirectoryA = CreateTemporaryDirectory();
            var offDirectoryB = CreateTemporaryDirectory();
            try
            {
                var identity = new Guid(
                    "10213243-5465-7687-98a9-bacbdcedfe0f");
                var onA = CreateDeterministicV2Record(
                    onDirectoryA,
                    identity,
                    true);
                var onB = CreateDeterministicV2Record(
                    onDirectoryB,
                    identity,
                    true);
                var offA = CreateDeterministicV2Record(
                    offDirectoryA,
                    identity,
                    false);
                var offB = CreateDeterministicV2Record(
                    offDirectoryB,
                    identity,
                    false);

                AssertBytesEqual(onA, onB);
                AssertBytesEqual(offA, offB);
                AssertEx.Equal(2, BitConverter.ToInt32(onA, VersionOffset));
                AssertEx.Equal((byte)1, onA[DirectionOffset]);
                AssertEx.Equal((byte)0, offA[DirectionOffset]);

                using (var reopened =
                    AxisPowerOnRecoveryJournal.Open(offDirectoryA))
                {
                    var record = reopened.CurrentRecord;
                    AssertEx.Equal(identity, record.Identity);
                    AssertEx.False(record.ExpectedPowerOn);
                    AssertEx.Equal("127.0.0.1", record.EndpointIp);
                    AssertEx.Equal("AxisDirection", record.AxisName);
                    AssertEx.Equal((ushort)7, record.AxisReference);
                    AssertEx.Equal(0x11223344U, record.DiagnosticsBootId);
                    AssertEx.Equal(0x55667788U, record.MapRevision);
                    AssertEx.Equal(FixedUtc(), record.CreatedUtc);
                    AssertEx.Equal(FixedUtc(), record.UpdatedUtc);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(onDirectoryA);
                DeleteTemporaryDirectory(onDirectoryB);
                DeleteTemporaryDirectory(offDirectoryA);
                DeleteTemporaryDirectory(offDirectoryB);
            }
        }

        private static void PowerOffLifecycleIsDurable()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = FixedUtc();
                Guid identity;
                using (var journal =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    var off = journal.ArmBeforeDispatch(
                        false,
                        "192.168.10.5",
                        5000,
                        "AxisOff",
                        3,
                        101,
                        202,
                        createdUtc);
                    identity = off.Identity;
                    AssertEx.False(off.ExpectedPowerOn);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.ArmedBeforeDispatch,
                        off.State);
                    journal.MarkAccepted(
                        identity,
                        createdUtc.AddSeconds(1));
                    journal.PromoteToRecoveryRequired(
                        identity,
                        createdUtc.AddSeconds(2));
                    var acceptedRetry = journal.MarkAccepted(
                        identity,
                        createdUtc.AddSeconds(3));
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                        acceptedRetry.State);
                    journal.Resolve(
                        identity,
                        createdUtc.AddSeconds(4));
                }

                using (var reopened =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    AssertCurrentRecord(
                        reopened,
                        identity,
                        false,
                        AxisPowerOnRecoveryState.Resolved);
                    AssertEx.False(reopened.HasActiveRecord);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void AtomicOnToOffTakeoverSurvivesReopen()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = FixedUtc();
                Guid offIdentity;
                using (var journal =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    var on = journal.ArmBeforeDispatch(
                        "10.0.0.5",
                        6000,
                        "AxisSafety",
                        4,
                        303,
                        404,
                        createdUtc);
                    journal.MarkAccepted(
                        on.Identity,
                        createdUtc.AddSeconds(1));
                    var off = journal
                        .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            on.Identity,
                            "10.0.0.5",
                            6000,
                            "AxisSafety",
                            4,
                            303,
                            404,
                            createdUtc.AddSeconds(2));
                    offIdentity = off.Identity;
                    AssertEx.False(off.Identity == on.Identity);
                    AssertEx.False(off.ExpectedPowerOn);
                    AssertEx.Equal(
                        AxisPowerOnRecoveryState.ArmedBeforeDispatch,
                        off.State);
                }

                using (var reopened =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    AssertCurrentRecord(
                        reopened,
                        offIdentity,
                        false,
                        AxisPowerOnRecoveryState.ArmedBeforeDispatch);
                    AssertEx.True(
                        reopened.CurrentRecord.MatchesRecoveryIdentity(
                            "10.0.0.5",
                            6000,
                            "AxisSafety",
                            4,
                            303,
                            404,
                            false));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void FailedTakeoverPreservesOriginal()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = FixedUtc();
                Guid onIdentity;
                string path;
                byte[] originalBytes;
                using (var journal =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    var on = journal.ArmBeforeDispatch(
                        "10.10.0.5",
                        6001,
                        "AxisPreserve",
                        5,
                        505,
                        606,
                        createdUtc);
                    onIdentity = on.Identity;
                    journal.PromoteToRecoveryRequired(
                        onIdentity,
                        createdUtc.AddSeconds(1));
                    path = journal.JournalFilePath;
                    originalBytes = File.ReadAllBytes(path);

                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        Guid.NewGuid(),
                        "10.10.0.5",
                        6001,
                        "AxisPreserve",
                        5,
                        505,
                        606,
                        createdUtc.AddSeconds(2),
                        onIdentity,
                        originalBytes);
                    AssertTakeoverRejectedAndPreserved(
                        journal,
                        onIdentity,
                        "10.10.0.6",
                        6001,
                        "AxisPreserve",
                        5,
                        505,
                        606,
                        createdUtc.AddSeconds(2),
                        onIdentity,
                        originalBytes);

                    AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                        journal.ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                            onIdentity,
                            "10.10.0.5",
                            6001,
                            "AxisPreserve",
                            5,
                            505,
                            606,
                            createdUtc));
                    AssertCurrentRecord(
                        journal,
                        onIdentity,
                        true,
                        AxisPowerOnRecoveryState.RecoveryRequired);
                    AssertBytesEqual(
                        originalBytes,
                        File.ReadAllBytes(path));

                    using (var blocker = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                    {
                        AssertEx.Throws<IOException>(() =>
                            journal
                                .ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                                    onIdentity,
                                    "10.10.0.5",
                                    6001,
                                    "AxisPreserve",
                                    5,
                                    505,
                                    606,
                                    createdUtc.AddSeconds(2)));
                    }

                    AssertCurrentRecord(
                        journal,
                        onIdentity,
                        true,
                        AxisPowerOnRecoveryState.RecoveryRequired);
                    AssertBytesEqual(
                        originalBytes,
                        File.ReadAllBytes(path));
                }

                using (var reopened =
                    AxisPowerOnRecoveryJournal.Open(directory))
                {
                    AssertCurrentRecord(
                        reopened,
                        onIdentity,
                        true,
                        AxisPowerOnRecoveryState.RecoveryRequired);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void V2BoundsAndDirectionAreFailClosed()
        {
            var directionDirectory = CreateTemporaryDirectory();
            var oversizeDirectory = CreateTemporaryDirectory();
            try
            {
                var path = CreateV2RecordFile(
                    directionDirectory,
                    false);
                var bytes = File.ReadAllBytes(path);
                bytes[DirectionOffset] = 2;
                RecomputeChecksum(bytes);
                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(() =>
                    AxisPowerOnRecoveryJournal.Open(directionDirectory));

                File.WriteAllBytes(
                    Path.Combine(
                        oversizeDirectory,
                        "axis-power-on-recovery.bin"),
                    new byte[MaximumFileLength + 1]);
                AssertEx.Throws<InvalidDataException>(() =>
                    AxisPowerOnRecoveryJournal.Open(oversizeDirectory));
            }
            finally
            {
                DeleteTemporaryDirectory(directionDirectory);
                DeleteTemporaryDirectory(oversizeDirectory);
            }
        }

        private static byte[] CreateDeterministicV2Record(
            string directory,
            Guid identity,
            bool expectedPowerOn)
        {
            string path;
            using (var journal =
                AxisPowerOnRecoveryJournal.Open(directory))
            {
                journal.ArmBeforeDispatch(
                    identity,
                    expectedPowerOn,
                    "127.1",
                    4000,
                    "AxisDirection",
                    7,
                    0x11223344U,
                    0x55667788U,
                    FixedUtc());
                path = journal.JournalFilePath;
            }

            return File.ReadAllBytes(path);
        }

        private static string CreateV2RecordFile(
            string directory,
            bool expectedPowerOn)
        {
            string path;
            using (var journal =
                AxisPowerOnRecoveryJournal.Open(directory))
            {
                journal.ArmBeforeDispatch(
                    expectedPowerOn,
                    "192.168.20.1",
                    4000,
                    "AxisBounds",
                    1,
                    11,
                    22,
                    FixedUtc());
                path = journal.JournalFilePath;
            }

            return path;
        }

        private static void AssertTakeoverRejectedAndPreserved(
            AxisPowerOnRecoveryJournal journal,
            Guid suppliedIdentity,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc,
            Guid expectedIdentity,
            byte[] expectedBytes)
        {
            AssertEx.Throws<InvalidOperationException>(() =>
                journal.ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                    suppliedIdentity,
                    endpointIp,
                    endpointPort,
                    axisName,
                    axisReference,
                    diagnosticsBootId,
                    mapRevision,
                    createdUtc));
            AssertCurrentRecord(
                journal,
                expectedIdentity,
                true,
                AxisPowerOnRecoveryState.RecoveryRequired);
            AssertBytesEqual(
                expectedBytes,
                File.ReadAllBytes(journal.JournalFilePath));
        }

        private static void AssertCurrentRecord(
            AxisPowerOnRecoveryJournal journal,
            Guid identity,
            bool expectedPowerOn,
            AxisPowerOnRecoveryState state)
        {
            var current = journal.CurrentRecord;
            AssertEx.NotNull(current);
            AssertEx.Equal(identity, current.Identity);
            AssertEx.Equal(expectedPowerOn, current.ExpectedPowerOn);
            AssertEx.Equal(state, current.State);
        }

        private static void AssertBytesEqual(
            byte[] expected,
            byte[] actual)
        {
            AssertEx.Equal(expected.Length, actual.Length);
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], actual[index]);
            }
        }

        private static byte[] CreateLegacyV1Record(
            Guid identity,
            AxisPowerOnRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            using (var writer = new BinaryWriter(
                payloadStream,
                Encoding.ASCII,
                true))
            {
                writer.Write(identity.ToByteArray());
                writer.Write((int)state);
                writer.Write(createdUtc.Ticks);
                writer.Write(updatedUtc.Ticks);
                writer.Write(0x11223344U);
                writer.Write(0x55667788U);
                writer.Write(4000);
                writer.Write((ushort)7);
                WriteLegacyText(writer, "192.168.20.1");
                WriteLegacyText(writer, "AxisLegacy");
                writer.Flush();
                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(
                stream,
                Encoding.ASCII,
                true))
            {
                writer.Write(Encoding.ASCII.GetBytes("ELMOAXP1"));
                writer.Write(1);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
                prefix = stream.ToArray();
            }

            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(prefix);
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

        private static void WriteLegacyText(
            BinaryWriter writer,
            string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
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
                "ElmoAxisPowerOnJournalTests",
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

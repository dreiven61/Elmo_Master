using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AxisSetPositionRecoveryJournalTests
    {
        private const int MaximumFileLength = 8192;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AxisSetPositionJournal.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.AxisSetPositionJournal.ExactRoundTrip",
                ExactRecordRoundTripIsDurable);
            tests.Add(
                "Wpf.AxisSetPositionJournal.StateLifecycle",
                StateLifecycleSurvivesReopen);
            tests.Add(
                "Wpf.AxisSetPositionJournal.InvalidTransitionPreservesBytes",
                InvalidTransitionsPreserveExactBytes);
            tests.Add(
                "Wpf.AxisSetPositionJournal.WriteFailurePreservesBytes",
                FailedAtomicReplacementPreservesExactBytes);
            tests.Add(
                "Wpf.AxisSetPositionJournal.StartupPromotionFailure",
                FailedStartupPromotionFailsOpenAndPreservesBytes);
            tests.Add(
                "Wpf.AxisSetPositionJournal.SingleWriterAndChecksum",
                SingleWriterAndChecksumAreEnforced);
            tests.Add(
                "Wpf.AxisSetPositionJournal.Bounds",
                InvalidRecordAndFileBoundsAreRejected);
            tests.Add(
                "Wpf.AxisSetPositionJournal.DeterministicBytes",
                SerializationIsDeterministic);
        }

        private static void DefaultPathIsVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisSetPositionRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                AxisSetPositionRecoveryJournal.GetDefaultDirectoryPath()
                    .ToUpperInvariant());
        }

        private static void ExactRecordRoundTripIsDurable()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var identity = new Guid(
                    "00112233-4455-6677-8899-aabbccddeeff");
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, identity);
                    AssertExactRecord(
                        armed,
                        AxisSetPositionRecoveryState.ArmedBeforeDispatch);
                    AssertEx.True(armed.IsActive);
                    AssertEx.True(armed.MatchesRecoveryIdentity(
                        "127.0.0.1",
                        4000,
                        0x01020304U,
                        0x11223344U,
                        0x55667788U,
                        "_LMCAxis1",
                        1));
                    AssertEx.False(armed.MatchesRecoveryIdentity(
                        "127.0.0.1",
                        4000,
                        0x01020305U,
                        0x11223344U,
                        0x55667788U,
                        "_LMCAxis1",
                        1));
                    AssertEx.True(armed.MatchesIntent(
                        0x89ABCDEFU,
                        0x01234567U,
                        0x76543210U,
                        0xFEDCBA98U,
                        0x10203040U,
                        -1234567,
                        7654321,
                        1,
                        1));
                    AssertEx.False(armed.MatchesIntent(
                        0x89ABCDEFU,
                        0x01234567U,
                        0x76543210U,
                        0xFEDCBA98U,
                        0x10203041U,
                        -1234567,
                        7654321,
                        1,
                        1));
                }

                using (var reopened =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    var recovered = reopened.CurrentRecord;
                    AssertExactIntent(recovered);
                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.RecoveryRequired,
                        recovered.State);
                    AssertEx.True(recovered.UpdatedUtc >= FixedUtc());
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void StateLifecycleSurvivesReopen()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var identity = Guid.NewGuid();
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    Arm(journal, identity);
                    var recovery = journal.PromoteToRecoveryRequired(
                        journal.CurrentRecord,
                        FixedUtc().AddSeconds(1));
                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.RecoveryRequired,
                        recovery.State);
                }

                using (var reopened =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.RecoveryRequired,
                        reopened.CurrentRecord.State);
                    reopened.Resolve(
                        reopened.CurrentRecord,
                        FixedUtc().AddSeconds(2));
                }

                using (var reopened =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.Resolved,
                        reopened.CurrentRecord.State);
                    AssertEx.False(reopened.CurrentRecord.IsActive);
                }

                var directDirectory = CreateTemporaryDirectory();
                try
                {
                    using (var journal =
                        AxisSetPositionRecoveryJournal.Open(directDirectory))
                    {
                        var direct = Arm(journal, Guid.NewGuid());
                        journal.Resolve(
                            direct,
                            FixedUtc().AddMilliseconds(1));
                        AssertEx.False(journal.HasActiveRecord);
                    }
                }
                finally
                {
                    DeleteTemporaryDirectory(directDirectory);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void InvalidTransitionsPreserveExactBytes()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, Guid.NewGuid());
                    var expected = File.ReadAllBytes(journal.JournalFilePath);

                    AssertPreserved(
                        journal,
                        expected,
                        () => journal.Resolve(
                            CreateRecord(
                                Guid.NewGuid(),
                                1,
                                1,
                                1,
                                FixedUtc(),
                                FixedUtc()),
                            FixedUtc().AddSeconds(1)));
                    AssertPreserved(
                        journal,
                        expected,
                        () => journal.PromoteToRecoveryRequired(
                            armed,
                            FixedUtc().AddTicks(-1)));
                    AssertPreserved(
                        journal,
                        expected,
                        () => Arm(journal, Guid.NewGuid()));

                    var staleArmed = armed.Copy();
                    var recovery = journal.PromoteToRecoveryRequired(
                        armed,
                        FixedUtc().AddSeconds(1));
                    expected = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved(
                        journal,
                        expected,
                        () => journal.PromoteToRecoveryRequired(
                            staleArmed,
                            FixedUtc().AddSeconds(2)));
                    AssertPreserved(
                        journal,
                        expected,
                        () => journal.Resolve(
                            staleArmed,
                            FixedUtc().AddSeconds(2)));

                    journal.Resolve(
                        recovery,
                        FixedUtc().AddSeconds(3));
                    expected = File.ReadAllBytes(journal.JournalFilePath);
                    AssertPreserved(
                        journal,
                        expected,
                        () => journal.Resolve(
                            recovery,
                            FixedUtc().AddSeconds(4)));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void FailedAtomicReplacementPreservesExactBytes()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, Guid.NewGuid());
                    var originalBytes = File.ReadAllBytes(
                        journal.JournalFilePath);
                    using (var blocker = new FileStream(
                        journal.JournalFilePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                    {
                        AssertEx.Throws<IOException>(() =>
                            journal.PromoteToRecoveryRequired(
                                armed,
                                FixedUtc().AddSeconds(1)));
                    }

                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                        journal.CurrentRecord.State);
                    AssertBytesEqual(
                        originalBytes,
                        File.ReadAllBytes(journal.JournalFilePath));
                    AssertEx.Equal(
                        0,
                        Directory.GetFiles(directory, "*.tmp").Length);
                }

                using (var reopened =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.RecoveryRequired,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void FailedStartupPromotionFailsOpenAndPreservesBytes()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                string path;
                byte[] armedBytes;
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    Arm(journal, Guid.NewGuid());
                    path = journal.JournalFilePath;
                    armedBytes = File.ReadAllBytes(path);
                }

                using (var blocker = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {
                    AssertEx.Throws<IOException>(() =>
                        AxisSetPositionRecoveryJournal.Open(directory));
                }
                AssertBytesEqual(armedBytes, File.ReadAllBytes(path));
                AssertEx.Equal(
                    0,
                    Directory.GetFiles(directory, "*.tmp").Length);

                using (var reopened =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    AssertEx.Equal(
                        AxisSetPositionRecoveryState.RecoveryRequired,
                        reopened.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void SingleWriterAndChecksumAreEnforced()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                string path;
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directory))
                {
                    Arm(journal, Guid.NewGuid());
                    path = journal.JournalFilePath;
                    AssertEx.Throws<IOException>(() =>
                        AxisSetPositionRecoveryJournal.Open(directory));
                }

                var bytes = File.ReadAllBytes(path);
                bytes[bytes.Length - 1] ^= 0x40;
                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(() =>
                    AxisSetPositionRecoveryJournal.Open(directory));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void InvalidRecordAndFileBoundsAreRejected()
        {
            var timestamp = FixedUtc();
            AssertEx.Throws<ArgumentException>(() => CreateRecord(
                Guid.Empty,
                1,
                1,
                1,
                timestamp,
                timestamp));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(
                Guid.NewGuid(),
                0,
                1,
                1,
                timestamp,
                timestamp));
            AssertEx.Throws<ArgumentException>(() =>
                new AxisSetPositionRecoveryRecord(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    1,
                    2,
                    3,
                    "Axis",
                    1,
                    0,
                    0,
                    0,
                    0,
                    1,
                    10,
                    20,
                    1,
                    1,
                    AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                    timestamp,
                    timestamp));
            AssertEx.Throws<ArgumentOutOfRangeException>(() =>
                new AxisSetPositionRecoveryRecord(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    1,
                    2,
                    3,
                    "Axis",
                    5,
                    1,
                    2,
                    3,
                    4,
                    5,
                    10,
                    20,
                    1,
                    1,
                    AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                    timestamp,
                    timestamp));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(
                Guid.NewGuid(),
                1,
                0,
                1,
                timestamp,
                timestamp));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(
                Guid.NewGuid(),
                1,
                1,
                2,
                timestamp,
                timestamp));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => CreateRecord(
                Guid.NewGuid(),
                1,
                1,
                1,
                timestamp,
                timestamp.AddTicks(-1)));
            AssertEx.Throws<ArgumentException>(() =>
                new AxisSetPositionRecoveryRecord(
                    Guid.NewGuid(),
                    "127.0.0.1",
                    4000,
                    1,
                    2,
                    3,
                    "Axis\uD55C",
                    1,
                    1,
                    2,
                    3,
                    4,
                    5,
                    10,
                    20,
                    1,
                    1,
                    AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                    timestamp,
                    timestamp));

            var directory = CreateTemporaryDirectory();
            try
            {
                File.WriteAllBytes(
                    Path.Combine(
                        directory,
                        "axis-set-position-recovery.bin"),
                    new byte[MaximumFileLength + 1]);
                AssertEx.Throws<InvalidDataException>(() =>
                    AxisSetPositionRecoveryJournal.Open(directory));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void SerializationIsDeterministic()
        {
            var directoryA = CreateTemporaryDirectory();
            var directoryB = CreateTemporaryDirectory();
            try
            {
                var identity = new Guid(
                    "fedcba98-7654-3210-fedc-ba9876543210");
                byte[] bytesA;
                byte[] bytesB;
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directoryA))
                {
                    Arm(journal, identity);
                    bytesA = File.ReadAllBytes(journal.JournalFilePath);
                }
                using (var journal =
                    AxisSetPositionRecoveryJournal.Open(directoryB))
                {
                    Arm(journal, identity);
                    bytesB = File.ReadAllBytes(journal.JournalFilePath);
                }
                AssertBytesEqual(bytesA, bytesB);
            }
            finally
            {
                DeleteTemporaryDirectory(directoryA);
                DeleteTemporaryDirectory(directoryB);
            }
        }

        private static AxisSetPositionRecoveryRecord Arm(
            AxisSetPositionRecoveryJournal journal,
            Guid identity)
        {
            return journal.ArmBeforeDispatch(
                identity,
                "127.1",
                4000,
                0x01020304U,
                0x11223344U,
                0x55667788U,
                "_LMCAxis1",
                1,
                0x89ABCDEFU,
                0x01234567U,
                0x76543210U,
                0xFEDCBA98U,
                0x10203040U,
                -1234567,
                7654321,
                1,
                1,
                FixedUtc());
        }

        private static AxisSetPositionRecoveryRecord CreateRecord(
            Guid identity,
            uint diagnosticsBuild,
            uint requestId,
            ushort semanticMode,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            return new AxisSetPositionRecoveryRecord(
                identity,
                "127.0.0.1",
                4000,
                diagnosticsBuild,
                2,
                3,
                "Axis",
                1,
                1,
                2,
                3,
                4,
                requestId,
                10,
                20,
                semanticMode,
                1,
                AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                createdUtc,
                updatedUtc);
        }

        private static void AssertExactRecord(
            AxisSetPositionRecoveryRecord record,
            AxisSetPositionRecoveryState state)
        {
            AssertEx.NotNull(record);
            AssertExactIntent(record);
            AssertEx.Equal(state, record.State);
            AssertEx.Equal(FixedUtc(), record.UpdatedUtc);
        }

        private static void AssertExactIntent(
            AxisSetPositionRecoveryRecord record)
        {
            AssertEx.NotNull(record);
            AssertEx.Equal(
                new Guid("00112233-4455-6677-8899-aabbccddeeff"),
                record.Identity);
            AssertEx.Equal("127.0.0.1", record.EndpointIp);
            AssertEx.Equal(4000, record.EndpointPort);
            AssertEx.Equal(0x01020304U, record.DiagnosticsBuild);
            AssertEx.Equal(0x11223344U, record.DiagnosticsBootId);
            AssertEx.Equal(0x55667788U, record.MapRevision);
            AssertEx.Equal("_LMCAxis1", record.AxisName);
            AssertEx.Equal((ushort)1, record.AxisReference);
            AssertEx.Equal(0x89ABCDEFU, record.ClientIntentId0);
            AssertEx.Equal(0x01234567U, record.ClientIntentId1);
            AssertEx.Equal(0x76543210U, record.ClientIntentId2);
            AssertEx.Equal(0xFEDCBA98U, record.ClientIntentId3);
            AssertEx.Equal(0x10203040U, record.RequestId);
            AssertEx.Equal(-1234567, record.TargetPosition);
            AssertEx.Equal(7654321, record.ExpectedActualPosition);
            AssertEx.Equal((ushort)1, record.SemanticMode);
            AssertEx.Equal((ushort)1, record.SchemaVersion);
            AssertEx.Equal(FixedUtc(), record.CreatedUtc);
        }

        private static void AssertPreserved(
            AxisSetPositionRecoveryJournal journal,
            byte[] expectedBytes,
            Action operation)
        {
            AssertEx.Throws<Exception>(operation);
            AssertBytesEqual(
                expectedBytes,
                File.ReadAllBytes(journal.JournalFilePath));
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

        private static DateTime FixedUtc()
        {
            return new DateTime(638895500000000000L, DateTimeKind.Utc);
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoAxisSetPositionJournalTests",
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    public static class MotionUncertaintyJournalTests
    {
        private const int ChecksumLength = 32;
        private const int VersionOffset = 8;

        private static readonly Guid AxisIdentity = new Guid(
            "43801296-268e-4fc3-a4b7-91d3ced2be82");
        private static readonly DateTime CreatedUtc = new DateTime(
            638894304000000000L,
            DateTimeKind.Utc);

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.MotionUncertaintyJournal.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.ArmBeforeDispatch",
                ArmBeforeDispatchIsDurable);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.RestartPromotion",
                RestartPromotionIsDurable);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.ExactIdentity",
                ExactIdentityRejectsDrift);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.ResolvedLifecycle",
                ResolvedTombstoneAndIdentityLifecycle);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.DeterministicRoundtrip",
                DeterministicFormatAndRoundtrip);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.CorruptionAndVersion",
                CorruptionAndUnsupportedVersionFailClosed);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.SingleWriterAtomic",
                SingleWriterAndAtomicTemporaryCleanup);
            tests.Add(
                "Wpf.MotionUncertaintyJournal.ImmutableRecord",
                RecordIsDefensiveAndImmutable);
        }

        public static int RunAll()
        {
            var tests = new Dictionary<string, Action>
            {
                {
                    "MotionUncertaintyJournal.DefaultPath",
                    DefaultPathIsVersioned
                },
                {
                    "MotionUncertaintyJournal.ArmBeforeDispatch",
                    ArmBeforeDispatchIsDurable
                },
                {
                    "MotionUncertaintyJournal.RestartPromotion",
                    RestartPromotionIsDurable
                },
                {
                    "MotionUncertaintyJournal.ExactIdentity",
                    ExactIdentityRejectsDrift
                },
                {
                    "MotionUncertaintyJournal.ResolvedLifecycle",
                    ResolvedTombstoneAndIdentityLifecycle
                },
                {
                    "MotionUncertaintyJournal.DeterministicRoundtrip",
                    DeterministicFormatAndRoundtrip
                },
                {
                    "MotionUncertaintyJournal.CorruptionAndVersion",
                    CorruptionAndUnsupportedVersionFailClosed
                },
                {
                    "MotionUncertaintyJournal.SingleWriterAtomic",
                    SingleWriterAndAtomicTemporaryCleanup
                },
                {
                    "MotionUncertaintyJournal.ImmutableRecord",
                    RecordIsDefensiveAndImmutable
                }
            };

            var failed = 0;
            foreach (var test in tests)
            {
                try
                {
                    test.Value();
                    Console.WriteLine("PASS " + test.Key);
                }
                catch (Exception error)
                {
                    failed++;
                    Console.Error.WriteLine("FAIL " + test.Key);
                    Console.Error.WriteLine(error);
                }
            }

            Console.WriteLine(
                "TOTAL "
                + tests.Count
                + ", PASSED "
                + (tests.Count - failed)
                + ", FAILED "
                + failed);
            return failed;
        }

        private static void DefaultPathIsVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "MotionUncertaintyJournal",
                "v1");
            AssertTrue(
                string.Equals(
                    expected,
                    MotionUncertaintyJournal.GetDefaultDirectoryPath(),
                    StringComparison.OrdinalIgnoreCase),
                "The default journal path is not the v1 LocalApplicationData path.");
        }

        private static void ArmBeforeDispatchIsDurable()
        {
            WithTestDirectory(delegate(string directoryPath)
            {
                string journalPath;
                using (var journal =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    AssertTrue(
                        !journal.HasActiveRecord,
                        "A new journal unexpectedly has an active record.");
                    var armed = ArmAxis(journal);
                    journalPath = journal.JournalFilePath;
                    AssertTrue(
                        File.Exists(journalPath),
                        "ArmBeforeDispatch returned before the durable file existed.");
                    AssertTrue(
                        File.ReadAllBytes(journalPath).Length > ChecksumLength,
                        "The durable arm file is incomplete.");
                    AssertRecord(
                        armed,
                        AxisIdentity,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        MotionUncertaintyState.ArmedBeforeDispatch,
                        CreatedUtc,
                        CreatedUtc);
                }

                using (var reopened =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    AssertTrue(
                        reopened.HasActiveRecord,
                        "The armed record did not survive restart.");
                    AssertRecord(
                        reopened.CurrentRecord,
                        AxisIdentity,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        MotionUncertaintyState.ArmedBeforeDispatch,
                        CreatedUtc,
                        CreatedUtc);
                }
            });
        }

        private static void RestartPromotionIsDurable()
        {
            WithTestDirectory(delegate(string directoryPath)
            {
                using (var journal =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    ArmAxis(journal);
                }

                using (var restarted =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    AssertEqual(
                        MotionUncertaintyState.ArmedBeforeDispatch,
                        restarted.CurrentRecord.State,
                        "Restart did not expose the pre-dispatch arm for conservative promotion.");
                    var promoted = restarted.PromoteToRecoveryRequired(
                        AxisIdentity,
                        CreatedUtc.AddSeconds(1));
                    AssertEqual(
                        MotionUncertaintyState.RecoveryRequired,
                        promoted.State,
                        "Restart promotion did not enter RecoveryRequired.");
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        restarted.PromoteToRecoveryRequired(
                            AxisIdentity,
                            CreatedUtc.AddSeconds(2));
                    });
                }

                using (var reopened =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    AssertTrue(
                        reopened.HasActiveRecord,
                        "The promoted recovery record was not active after restart.");
                    AssertEqual(
                        MotionUncertaintyState.RecoveryRequired,
                        reopened.CurrentRecord.State,
                        "RecoveryRequired did not persist across restart.");
                    AssertEqual(
                        CreatedUtc.AddSeconds(1),
                        reopened.CurrentRecord.UpdatedUtc,
                        "The restart promotion timestamp did not persist.");
                }
            });
        }

        private static void ExactIdentityRejectsDrift()
        {
            WithTestDirectory(delegate(string directoryPath)
            {
                using (var journal =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    var record = ArmAxis(journal);
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667788U,
                        true,
                        "The exact recovery identity did not match.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.22",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667788U,
                        false,
                        "Endpoint IP drift matched.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5021,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667788U,
                        false,
                        "Endpoint port drift matched.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Group,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667788U,
                        false,
                        "Target kind drift matched.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis02",
                        1,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667788U,
                        false,
                        "Target name drift matched.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        2,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667788U,
                        false,
                        "Target reference drift matched.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveRelative",
                        0x11223344U,
                        0x55667788U,
                        false,
                        "Operation drift matched.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        0x11223345U,
                        0x55667788U,
                        false,
                        "Diagnostics BootId drift matched.");
                    AssertIdentityMatch(
                        record,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667789U,
                        false,
                        "Map revision drift matched.");
                    AssertIdentityMatch(
                        record,
                        "not-an-ip",
                        5020,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        0x11223344U,
                        0x55667788U,
                        false,
                        "An invalid endpoint IP matched.");
                }
            });
        }

        private static void ResolvedTombstoneAndIdentityLifecycle()
        {
            WithTestDirectory(delegate(string directoryPath)
            {
                using (var journal =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    ArmAxis(journal);
                    var resolved = journal.Resolve(
                        AxisIdentity,
                        CreatedUtc.AddSeconds(1));
                    AssertEqual(
                        MotionUncertaintyState.Resolved,
                        resolved.State,
                        "The armed record did not resolve.");
                    AssertTrue(
                        !journal.HasActiveRecord,
                        "A resolved record remained active.");
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.Resolve(
                            AxisIdentity,
                            CreatedUtc.AddSeconds(2));
                    });
                }

                using (var reopened =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    AssertTrue(
                        !reopened.HasActiveRecord,
                        "The resolved tombstone became active after restart.");
                    AssertEqual(
                        AxisIdentity,
                        reopened.CurrentRecord.Identity,
                        "The resolved tombstone identity was not preserved.");
                    AssertEqual(
                        MotionUncertaintyState.Resolved,
                        reopened.CurrentRecord.State,
                        "The resolved tombstone state was not preserved.");
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        reopened.ArmBeforeDispatch(
                            AxisIdentity,
                            "192.168.10.21",
                            5020,
                            MotionUncertaintyTargetKind.Axis,
                            "Axis01",
                            1,
                            "MoveAbsolute",
                            0x11223344U,
                            0x55667788U,
                            CreatedUtc.AddSeconds(2));
                    });

                    var groupIdentity = new Guid(
                        "28f96565-9df3-423b-af31-b32065d83c89");
                    var group = reopened.ArmBeforeDispatch(
                        groupIdentity,
                        "192.168.10.21",
                        5020,
                        MotionUncertaintyTargetKind.Group,
                        "Group01",
                        7,
                        "MoveLinearAbsolute",
                        0x11223344U,
                        0x55667788U,
                        CreatedUtc.AddSeconds(2));
                    AssertRecord(
                        group,
                        groupIdentity,
                        MotionUncertaintyTargetKind.Group,
                        "Group01",
                        7,
                        "MoveLinearAbsolute",
                        MotionUncertaintyState.ArmedBeforeDispatch,
                        CreatedUtc.AddSeconds(2),
                        CreatedUtc.AddSeconds(2));
                }
            });
        }

        private static void DeterministicFormatAndRoundtrip()
        {
            WithTwoTestDirectories(delegate(
                string firstDirectory,
                string secondDirectory)
            {
                WriteCompleteRecord(firstDirectory);
                WriteCompleteRecord(secondDirectory);
                var first = File.ReadAllBytes(Path.Combine(
                    firstDirectory,
                    MotionUncertaintyJournal.JournalFileName));
                var second = File.ReadAllBytes(Path.Combine(
                    secondDirectory,
                    MotionUncertaintyJournal.JournalFileName));
                AssertSequenceEqual(
                    first,
                    second,
                    "Identical records did not produce deterministic bytes.");
                AssertTrue(
                    first.Length < 4096,
                    "The v1 record exceeded its bounded format.");

                using (var reopened =
                    MotionUncertaintyJournal.Open(firstDirectory))
                {
                    AssertRecord(
                        reopened.CurrentRecord,
                        AxisIdentity,
                        MotionUncertaintyTargetKind.Axis,
                        "Axis01",
                        1,
                        "MoveAbsolute",
                        MotionUncertaintyState.RecoveryRequired,
                        CreatedUtc,
                        CreatedUtc.AddSeconds(1));
                }
            });
        }

        private static void CorruptionAndUnsupportedVersionFailClosed()
        {
            WithTestDirectory(delegate(string directoryPath)
            {
                string path;
                using (var journal =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    ArmAxis(journal);
                    path = journal.JournalFilePath;
                }

                var valid = File.ReadAllBytes(path);
                var corrupt = (byte[])valid.Clone();
                corrupt[24] ^= 0x5A;
                File.WriteAllBytes(path, corrupt);
                AssertThrows<InvalidDataException>(delegate
                {
                    using (MotionUncertaintyJournal.Open(directoryPath))
                    {
                    }
                });
                AssertTrue(
                    File.Exists(path),
                    "A corrupt active journal was silently removed.");
                AssertSequenceEqual(
                    corrupt,
                    File.ReadAllBytes(path),
                    "Fail-closed corruption handling changed the rejected file.");

                var unsupported = (byte[])valid.Clone();
                WriteInt32LittleEndian(unsupported, VersionOffset, 2);
                RewriteChecksum(unsupported);
                File.WriteAllBytes(path, unsupported);
                AssertThrows<NotSupportedException>(delegate
                {
                    using (MotionUncertaintyJournal.Open(directoryPath))
                    {
                    }
                });
                AssertTrue(
                    File.Exists(path),
                    "An unsupported active journal was silently removed.");
                AssertSequenceEqual(
                    unsupported,
                    File.ReadAllBytes(path),
                    "Fail-closed version handling changed the rejected file.");
            });
        }

        private static void SingleWriterAndAtomicTemporaryCleanup()
        {
            WithTestDirectory(delegate(string directoryPath)
            {
                var staleTemporaryPath = Path.Combine(
                    directoryPath,
                    MotionUncertaintyJournal.JournalFileName
                        + ".stale.tmp");
                File.WriteAllBytes(staleTemporaryPath, new byte[] { 1, 2, 3 });

                using (var first =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    AssertTrue(
                        !File.Exists(staleTemporaryPath),
                        "Open did not clean a stale journal temporary file.");
                    AssertThrows<IOException>(delegate
                    {
                        using (MotionUncertaintyJournal.Open(directoryPath))
                        {
                        }
                    });

                    ArmAxis(first);
                    AssertNoTemporaryFiles(directoryPath);
                    first.PromoteToRecoveryRequired(
                        AxisIdentity,
                        CreatedUtc.AddSeconds(1));
                    AssertNoTemporaryFiles(directoryPath);
                    first.Resolve(
                        AxisIdentity,
                        CreatedUtc.AddSeconds(2));
                    AssertNoTemporaryFiles(directoryPath);
                }

                using (var reopened =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    AssertEqual(
                        MotionUncertaintyState.Resolved,
                        reopened.CurrentRecord.State,
                        "The atomic replacement lifecycle did not roundtrip.");
                }
            });
        }

        private static void RecordIsDefensiveAndImmutable()
        {
            WithTestDirectory(delegate(string directoryPath)
            {
                using (var journal =
                    MotionUncertaintyJournal.Open(directoryPath))
                {
                    var returnedFromArm = ArmAxis(journal);
                    var firstSnapshot = journal.CurrentRecord;
                    var secondSnapshot = journal.CurrentRecord;
                    AssertTrue(
                        !object.ReferenceEquals(
                            returnedFromArm,
                            firstSnapshot),
                        "Arm exposed the journal's record reference.");
                    AssertTrue(
                        !object.ReferenceEquals(
                            firstSnapshot,
                            secondSnapshot),
                        "CurrentRecord did not return a defensive copy.");

                    journal.PromoteToRecoveryRequired(
                        AxisIdentity,
                        CreatedUtc.AddSeconds(1));
                    AssertEqual(
                        MotionUncertaintyState.ArmedBeforeDispatch,
                        firstSnapshot.State,
                        "A returned snapshot changed in place.");
                }

                var recordType = typeof(MotionUncertaintyRecord);
                foreach (var property in recordType.GetProperties(
                    BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic))
                {
                    AssertTrue(
                        property.GetSetMethod(true) == null,
                        "Record property has a setter: " + property.Name);
                }

                foreach (var field in recordType.GetFields(
                    BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic))
                {
                    AssertTrue(
                        field.IsInitOnly,
                        "Record field is not readonly: " + field.Name);
                }
            });
        }

        private static MotionUncertaintyRecord ArmAxis(
            MotionUncertaintyJournal journal)
        {
            return journal.ArmBeforeDispatch(
                AxisIdentity,
                "192.168.10.21",
                5020,
                MotionUncertaintyTargetKind.Axis,
                "Axis01",
                1,
                "MoveAbsolute",
                0x11223344U,
                0x55667788U,
                CreatedUtc);
        }

        private static void WriteCompleteRecord(string directoryPath)
        {
            using (var journal =
                MotionUncertaintyJournal.Open(directoryPath))
            {
                ArmAxis(journal);
                journal.PromoteToRecoveryRequired(
                    AxisIdentity,
                    CreatedUtc.AddSeconds(1));
            }
        }

        private static void AssertRecord(
            MotionUncertaintyRecord record,
            Guid identity,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            MotionUncertaintyState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            AssertTrue(record != null, "The durable record is missing.");
            AssertEqual(identity, record.Identity, "Identity did not roundtrip.");
            AssertEqual(
                "192.168.10.21",
                record.EndpointIp,
                "Endpoint IP did not roundtrip.");
            AssertEqual(
                5020,
                record.EndpointPort,
                "Endpoint port did not roundtrip.");
            AssertEqual(
                targetKind,
                record.TargetKind,
                "Target kind did not roundtrip.");
            AssertEqual(
                targetName,
                record.TargetName,
                "Target name did not roundtrip.");
            AssertEqual(
                targetReference,
                record.TargetReference,
                "Target reference did not roundtrip.");
            AssertEqual(
                operation,
                record.Operation,
                "Operation did not roundtrip.");
            AssertEqual(
                0x11223344U,
                record.DiagnosticsBootId,
                "Diagnostics BootId did not roundtrip.");
            AssertEqual(
                0x55667788U,
                record.MapRevision,
                "Map revision did not roundtrip.");
            AssertEqual(state, record.State, "State did not roundtrip.");
            AssertEqual(
                createdUtc,
                record.CreatedUtc,
                "Creation timestamp did not roundtrip.");
            AssertEqual(
                updatedUtc,
                record.UpdatedUtc,
                "Update timestamp did not roundtrip.");
            AssertEqual(
                DateTimeKind.Utc,
                record.CreatedUtc.Kind,
                "Creation timestamp kind is not UTC.");
            AssertEqual(
                DateTimeKind.Utc,
                record.UpdatedUtc.Kind,
                "Update timestamp kind is not UTC.");
        }

        private static void AssertIdentityMatch(
            MotionUncertaintyRecord record,
            string endpointIp,
            int endpointPort,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            uint diagnosticsBootId,
            uint mapRevision,
            bool expected,
            string message)
        {
            AssertEqual(
                expected,
                record.MatchesRecoveryIdentity(
                    endpointIp,
                    endpointPort,
                    targetKind,
                    targetName,
                    targetReference,
                    operation,
                    diagnosticsBootId,
                    mapRevision),
                message);
        }

        private static void AssertNoTemporaryFiles(string directoryPath)
        {
            AssertEqual(
                0,
                Directory.GetFiles(
                    directoryPath,
                    MotionUncertaintyJournal.JournalFileName + ".*.tmp")
                    .Length,
                "Atomic persistence left a journal temporary file.");
        }

        private static void RewriteChecksum(byte[] bytes)
        {
            var checksumOffset = bytes.Length - ChecksumLength;
            using (var sha256 = SHA256.Create())
            {
                var checksum = sha256.ComputeHash(
                    bytes,
                    0,
                    checksumOffset);
                Buffer.BlockCopy(
                    checksum,
                    0,
                    bytes,
                    checksumOffset,
                    checksum.Length);
            }
        }

        private static void WriteInt32LittleEndian(
            byte[] destination,
            int offset,
            int value)
        {
            destination[offset] = (byte)value;
            destination[offset + 1] = (byte)(value >> 8);
            destination[offset + 2] = (byte)(value >> 16);
            destination[offset + 3] = (byte)(value >> 24);
        }

        private static TException AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException error)
            {
                return error;
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    "Expected "
                        + typeof(TException).Name
                        + " but observed "
                        + error.GetType().Name
                        + ".",
                    error);
            }

            throw new InvalidOperationException(
                "Expected " + typeof(TException).Name + ".");
        }

        private static void AssertSequenceEqual(
            byte[] expected,
            byte[] actual,
            string message)
        {
            if (object.ReferenceEquals(expected, actual))
            {
                return;
            }

            if (expected == null
                || actual == null
                || expected.Length != actual.Length)
            {
                throw new InvalidOperationException(message);
            }

            for (var index = 0; index < expected.Length; index++)
            {
                if (expected[index] != actual[index])
                {
                    throw new InvalidOperationException(
                        message + " Difference at byte " + index + ".");
                }
            }
        }

        private static void AssertEqual<T>(
            T expected,
            T actual,
            string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    message
                        + " Expected="
                        + expected
                        + ", Actual="
                        + actual
                        + ".");
            }
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void WithTestDirectory(Action<string> action)
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "ElmoMotionUncertaintyJournalTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);
            try
            {
                action(directoryPath);
            }
            finally
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
        }

        private static void WithTwoTestDirectories(
            Action<string, string> action)
        {
            WithTestDirectory(delegate(string first)
            {
                WithTestDirectory(delegate(string second)
                {
                    action(first, second);
                });
            });
        }
    }
}

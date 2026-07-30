using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    public static class GroupProfileLockRecoveryJournalTests
    {
        private const int ChecksumLength = 32;
        private const int VersionOffset = 8;
        private const int PayloadLengthOffset = 12;
        private const int ExpectedProfileLockedOffset = 36;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.GroupProfileLockJournal.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.GroupProfileLockJournal.RoundtripReopen",
                RoundtripAndReopenPreserveIdentity);
            tests.Add(
                "Wpf.GroupProfileLockJournal.AcceptedRoundtripReopen",
                AcceptedRoundtripAndReopenPreserveIdentity);
            tests.Add(
                "Wpf.GroupProfileLockJournal.SecondWriter",
                SecondWriterFails);
            tests.Add(
                "Wpf.GroupProfileLockJournal.Corruption",
                CorruptionFailsClosed);
            tests.Add(
                "Wpf.GroupProfileLockJournal.IdentityAndTransitions",
                IdentityAndTransitionsAreValidated);
            tests.Add(
                "Wpf.GroupProfileLockJournal.DefensiveImmutableRecord",
                RecordIsDefensiveAndImmutable);
            tests.Add(
                "Wpf.GroupProfileLockJournal.DirectionRoundtripAndLegacyV1",
                DirectionRoundtripAndLegacyV1DefaultsToLocked);
            tests.Add(
                "Wpf.GroupProfileLockJournal.LockToUnlockAtomicReplacement",
                LockToUnlockReplacementIsAtomicAndExact);
            tests.Add(
                "Wpf.GroupProfileLockJournal.StateEnumValuesStable",
                StateEnumValuesRemainStable);
            tests.Add(
                "Wpf.GroupProfileLockJournal.UnlockRecoveryExplicitRetryAccepted",
                UnlockRecoveryExplicitRetryCanBecomeAccepted);
        }

        public static int RunAll()
        {
            var tests = new Dictionary<string, Action>
            {
                {
                    "GroupProfileLockJournal.DefaultPath",
                    DefaultPathIsVersioned
                },
                {
                    "GroupProfileLockJournal.RoundtripReopen",
                    RoundtripAndReopenPreserveIdentity
                },
                {
                    "GroupProfileLockJournal.AcceptedRoundtripReopen",
                    AcceptedRoundtripAndReopenPreserveIdentity
                },
                {
                    "GroupProfileLockJournal.SecondWriter",
                    SecondWriterFails
                },
                {
                    "GroupProfileLockJournal.Corruption",
                    CorruptionFailsClosed
                },
                {
                    "GroupProfileLockJournal.IdentityAndTransitions",
                    IdentityAndTransitionsAreValidated
                },
                {
                    "GroupProfileLockJournal.DefensiveImmutableRecord",
                    RecordIsDefensiveAndImmutable
                },
                {
                    "GroupProfileLockJournal.DirectionRoundtripAndLegacyV1",
                    DirectionRoundtripAndLegacyV1DefaultsToLocked
                },
                {
                    "GroupProfileLockJournal.LockToUnlockAtomicReplacement",
                    LockToUnlockReplacementIsAtomicAndExact
                },
                {
                    "GroupProfileLockJournal.StateEnumValuesStable",
                    StateEnumValuesRemainStable
                },
                {
                    "GroupProfileLockJournal.UnlockRecoveryExplicitRetryAccepted",
                    UnlockRecoveryExplicitRetryCanBecomeAccepted
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
                "GroupProfileLockRecoveryJournal",
                "v1");
            AssertTrue(
                string.Equals(
                    expected,
                    GroupProfileLockRecoveryJournal
                        .GetDefaultDirectoryPath(),
                    StringComparison.OrdinalIgnoreCase),
                "The default journal path is not the v1 LocalApplicationData path.");
        }

        private static void RoundtripAndReopenPreserveIdentity()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var identity = new Guid(
                    "c4d89586-d6b8-4319-a6ed-150520c83de8");
                var createdUtc = new DateTime(
                    638893440000000000L,
                    DateTimeKind.Utc);

                using (var journal =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        identity,
                        "192.168.10.21",
                        5020,
                        "Group01",
                        7,
                        0x11223344U,
                        0x55667788U,
                        createdUtc);
                    AssertRecord(
                        armed,
                        identity,
                        GroupProfileLockRecoveryState
                            .ArmedBeforeDispatch,
                        createdUtc,
                        createdUtc);

                    var promoted = journal.PromoteToRecoveryRequired(
                        identity,
                        createdUtc.AddSeconds(1));
                    AssertRecord(
                        promoted,
                        identity,
                        GroupProfileLockRecoveryState
                            .RecoveryRequired,
                        createdUtc,
                        createdUtc.AddSeconds(1));
                }

                using (var reopened =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    AssertTrue(
                        reopened.HasActiveRecord,
                        "The promoted record was not active after reopen.");
                    AssertRecord(
                        reopened.CurrentRecord,
                        identity,
                        GroupProfileLockRecoveryState
                            .RecoveryRequired,
                        createdUtc,
                        createdUtc.AddSeconds(1));
                    reopened.Resolve(
                        identity,
                        createdUtc.AddSeconds(2));
                }

                using (var reopened =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    AssertTrue(
                        !reopened.HasActiveRecord,
                        "The resolved record remained active after reopen.");
                    AssertRecord(
                        reopened.CurrentRecord,
                        identity,
                        GroupProfileLockRecoveryState.Resolved,
                        createdUtc,
                        createdUtc.AddSeconds(2));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void SecondWriterFails()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var first =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    AssertThrows<IOException>(delegate
                    {
                        using (GroupProfileLockRecoveryJournal.Open(directory))
                        {
                        }
                    });
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void AcceptedRoundtripAndReopenPreserveIdentity()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var identity = new Guid(
                    "53e18637-7bfd-438e-bd4d-1e70e2023b7a");
                var createdUtc = new DateTime(
                    638893445000000000L,
                    DateTimeKind.Utc);

                using (var journal =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    journal.ArmBeforeDispatch(
                        identity,
                        "192.168.10.21",
                        5020,
                        "Group01",
                        7,
                        0x11223344U,
                        0x55667788U,
                        createdUtc);
                    var accepted = journal.MarkAccepted(
                        identity,
                        createdUtc.AddSeconds(1));
                    AssertRecord(
                        accepted,
                        identity,
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        createdUtc,
                        createdUtc.AddSeconds(1));
                }

                using (var reopened =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    AssertTrue(
                        reopened.HasActiveRecord,
                        "The accepted record was not active after reopen.");
                    AssertRecord(
                        reopened.CurrentRecord,
                        identity,
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        createdUtc,
                        createdUtc.AddSeconds(1));
                    reopened.Resolve(
                        identity,
                        createdUtc.AddSeconds(2));
                }

                using (var reopened =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    AssertTrue(
                        !reopened.HasActiveRecord,
                        "The resolved accepted record remained active after reopen.");
                    AssertRecord(
                        reopened.CurrentRecord,
                        identity,
                        GroupProfileLockRecoveryState.Resolved,
                        createdUtc,
                        createdUtc.AddSeconds(2));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void CorruptionFailsClosed()
        {
            AssertCorruptionRejected(delegate(byte[] bytes)
            {
                bytes[bytes.Length - 1] ^= 0x80;
            });
            AssertCorruptionRejected(delegate(byte[] bytes)
            {
                WriteInt32(bytes, VersionOffset, 999);
                RecomputeChecksum(bytes);
            });
            AssertCorruptionRejected(delegate(byte[] bytes)
            {
                WriteInt32(
                    bytes,
                    PayloadLengthOffset,
                    BitConverter.ToInt32(bytes, PayloadLengthOffset) + 1);
                RecomputeChecksum(bytes);
            });
            AssertCorruptionRejected(delegate(byte[] bytes)
            {
                bytes[ExpectedProfileLockedOffset] = 2;
                RecomputeChecksum(bytes);
            });
            AssertOversizedJournalRejected();
        }

        private static void IdentityAndTransitionsAreValidated()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var identity = new Guid(
                    "4e0a837c-82d2-4633-9ebc-069a33a9d96a");
                var otherIdentity = new Guid(
                    "b451bf0b-d3df-47ac-9340-dd29b9532314");
                var createdUtc = new DateTime(
                    638893450000000000L,
                    DateTimeKind.Utc);

                using (var journal =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        identity,
                        "10.0.0.5",
                        6000,
                        "MotionGroup",
                        3,
                        101,
                        202,
                        createdUtc);

                    AssertTrue(
                        armed.MatchesRecoveryIdentity(
                            "10.0.0.5",
                            6000,
                            "MotionGroup",
                            3,
                            101,
                            202),
                        "The exact recovery identity did not match.");
                    AssertTrue(
                        !armed.MatchesRecoveryIdentity(
                            "10.0.0.6",
                            6000,
                            "MotionGroup",
                            3,
                            101,
                            202),
                        "A different endpoint IP matched.");
                    AssertTrue(
                        !armed.MatchesRecoveryIdentity(
                            "10.0.0.5",
                            6001,
                            "MotionGroup",
                            3,
                            101,
                            202),
                        "A different endpoint port matched.");
                    AssertTrue(
                        !armed.MatchesRecoveryIdentity(
                            "10.0.0.5",
                            6000,
                            "OtherGroup",
                            3,
                            101,
                            202),
                        "A different group name matched.");
                    AssertTrue(
                        !armed.MatchesRecoveryIdentity(
                            "10.0.0.5",
                            6000,
                            "MotionGroup",
                            4,
                            101,
                            202),
                        "A different group reference matched.");
                    AssertTrue(
                        !armed.MatchesRecoveryIdentity(
                            "10.0.0.5",
                            6000,
                            "MotionGroup",
                            3,
                            102,
                            202),
                        "A different diagnostics BootId matched.");
                    AssertTrue(
                        !armed.MatchesRecoveryIdentity(
                            "10.0.0.5",
                            6000,
                            "MotionGroup",
                            3,
                            101,
                            203),
                        "A different map revision matched.");

                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.PromoteToRecoveryRequired(
                            otherIdentity,
                            createdUtc.AddSeconds(1));
                    });
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.ArmBeforeDispatch(
                            otherIdentity,
                            "10.0.0.5",
                            6000,
                            "MotionGroup",
                            3,
                            101,
                            202,
                            createdUtc.AddSeconds(1));
                    });

                    journal.PromoteToRecoveryRequired(
                        identity,
                        createdUtc.AddSeconds(1));
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.PromoteToRecoveryRequired(
                            identity,
                            createdUtc.AddSeconds(2));
                    });
                    AssertThrows<ArgumentOutOfRangeException>(delegate
                    {
                        journal.Resolve(identity, createdUtc);
                    });
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.Resolve(
                            otherIdentity,
                            createdUtc.AddSeconds(2));
                    });

                    journal.Resolve(
                        identity,
                        createdUtc.AddSeconds(2));
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.Resolve(
                            identity,
                            createdUtc.AddSeconds(3));
                    });
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.ArmBeforeDispatch(
                            identity,
                            "10.0.0.5",
                            6000,
                            "MotionGroup",
                            3,
                            101,
                            202,
                            createdUtc.AddSeconds(3));
                    });
                }

                AssertThrows<ArgumentOutOfRangeException>(delegate
                {
                    CreateRecord(0, 202);
                });
                AssertThrows<ArgumentOutOfRangeException>(delegate
                {
                    CreateRecord(101, 0);
                });
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void RecordIsDefensiveAndImmutable()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = new DateTime(
                    638893460000000000L,
                    DateTimeKind.Utc);
                using (var journal =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    var returnedFromArm = journal.ArmBeforeDispatch(
                        "172.16.1.8",
                        7000,
                        "ImmutableGroup",
                        8,
                        303,
                        404,
                        createdUtc);
                    var firstSnapshot = journal.CurrentRecord;
                    var secondSnapshot = journal.CurrentRecord;
                    AssertTrue(
                        !object.ReferenceEquals(
                            returnedFromArm,
                            firstSnapshot),
                        "Arm exposed the journal's mutable record reference.");
                    AssertTrue(
                        !object.ReferenceEquals(
                            firstSnapshot,
                            secondSnapshot),
                        "CurrentRecord did not return a defensive copy.");

                    journal.PromoteToRecoveryRequired(
                        firstSnapshot.Identity,
                        createdUtc.AddSeconds(1));
                    AssertEqual(
                        GroupProfileLockRecoveryState
                            .ArmedBeforeDispatch,
                        firstSnapshot.State,
                        "A previously returned snapshot changed in place.");
                    AssertEqual(
                        GroupProfileLockRecoveryState
                            .RecoveryRequired,
                        journal.CurrentRecord.State,
                        "The durable state did not advance.");
                }

                var recordType = typeof(GroupProfileLockRecoveryRecord);
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
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void DirectionRoundtripAndLegacyV1DefaultsToLocked()
        {
            var directory = CreateTemporaryDirectory();
            var legacyDirectory = CreateTemporaryDirectory();
            try
            {
                var unlockIdentity = new Guid(
                    "b75a725e-1d7c-4078-a4ee-7770a7067985");
                var createdUtc = new DateTime(
                    638893465000000000L,
                    DateTimeKind.Utc);
                using (var journal =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    var armed = journal.ArmBeforeDispatch(
                        unlockIdentity,
                        false,
                        "192.168.10.21",
                        5020,
                        "Group01",
                        7,
                        0x11223344U,
                        0x55667788U,
                        createdUtc);
                    AssertRecord(
                        armed,
                        unlockIdentity,
                        GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                        createdUtc,
                        createdUtc,
                        false);
                    var accepted = journal.MarkAccepted(
                        unlockIdentity,
                        createdUtc.AddSeconds(1));
                    AssertTrue(
                        !accepted.ExpectedProfileLocked,
                        "The unlock direction changed during transition.");
                }

                using (var reopened =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    AssertRecord(
                        reopened.CurrentRecord,
                        unlockIdentity,
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        createdUtc,
                        createdUtc.AddSeconds(1),
                        false);
                    AssertTrue(
                        reopened.CurrentRecord.MatchesRecoveryIdentity(
                            "192.168.10.21",
                            5020,
                            "Group01",
                            7,
                            0x11223344U,
                            0x55667788U,
                            false),
                        "The exact unlock recovery identity did not match.");
                    AssertTrue(
                        !reopened.CurrentRecord.MatchesRecoveryIdentity(
                            "192.168.10.21",
                            5020,
                            "Group01",
                            7,
                            0x11223344U,
                            0x55667788U),
                        "The legacy Lock identity overload matched an unlock record.");
                    AssertTrue(
                        !reopened.CurrentRecord.MatchesRecoveryIdentity(
                            "192.168.10.21",
                            5020,
                            "Group01",
                            7,
                            0x11223344U,
                            0x55667788U,
                            true),
                        "The unlock record matched the lock direction.");
                }

                var legacyIdentity = new Guid(
                    "d0a2a4d1-419b-435d-918a-84a86115ef41");
                WriteLegacyV1Record(
                    legacyDirectory,
                    legacyIdentity,
                    GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                    createdUtc,
                    createdUtc.AddSeconds(2));
                using (var legacy =
                    GroupProfileLockRecoveryJournal.Open(legacyDirectory))
                {
                    AssertRecord(
                        legacy.CurrentRecord,
                        legacyIdentity,
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        createdUtc,
                        createdUtc.AddSeconds(2),
                        true);
                    AssertTrue(
                        legacy.CurrentRecord.ExpectedProfileLocked,
                        "A v1 record was not interpreted as a lock operation.");
                    legacy.Resolve(
                        legacyIdentity,
                        createdUtc.AddSeconds(3));
                }

                using (var migrated =
                    GroupProfileLockRecoveryJournal.Open(legacyDirectory))
                {
                    AssertRecord(
                        migrated.CurrentRecord,
                        legacyIdentity,
                        GroupProfileLockRecoveryState.Resolved,
                        createdUtc,
                        createdUtc.AddSeconds(3),
                        true);
                    AssertTrue(
                        !migrated.HasActiveRecord,
                        "The v1-to-v2 migrated tombstone remained active.");
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
                DeleteTemporaryDirectory(legacyDirectory);
            }
        }

        private static void LockToUnlockReplacementIsAtomicAndExact()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var lockIdentity = new Guid(
                    "eae743ae-0d2b-418b-a50d-52f31bc99b8e");
                var unlockIdentity = new Guid(
                    "88dc65be-f580-4a40-9363-11ba649f3dd5");
                var otherIdentity = new Guid(
                    "95330ee4-b2f9-41b9-a918-b0a07d8699f4");
                var createdUtc = new DateTime(
                    638893466000000000L,
                    DateTimeKind.Utc);

                using (var journal =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    journal.ArmBeforeDispatch(
                        lockIdentity,
                        true,
                        "10.20.30.40",
                        4000,
                        "ReplacementGroup",
                        9,
                        0x01020304U,
                        0xA1A2A3A4U,
                        createdUtc);
                    var acceptedLock = journal.MarkAccepted(
                        lockIdentity,
                        createdUtc.AddSeconds(1));
                    var lockSnapshot = journal.CurrentRecord;
                    var journalPath = journal.JournalFilePath;
                    var lockBytes = File.ReadAllBytes(journalPath);

                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.ReplaceActiveLockWithUnlockBeforeDispatch(
                            otherIdentity,
                            unlockIdentity,
                            "10.20.30.40",
                            4000,
                            "ReplacementGroup",
                            9,
                            0x01020304U,
                            0xA1A2A3A4U,
                            createdUtc.AddSeconds(2));
                    });
                    AssertEqual(
                        lockIdentity,
                        journal.CurrentRecord.Identity,
                        "A rejected active identity changed the durable record.");
                    AssertBytesEqual(
                        lockBytes,
                        File.ReadAllBytes(journalPath));

                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.ReplaceActiveLockWithUnlockBeforeDispatch(
                            lockIdentity,
                            unlockIdentity,
                            "10.20.30.41",
                            4000,
                            "ReplacementGroup",
                            9,
                            0x01020304U,
                            0xA1A2A3A4U,
                            createdUtc.AddSeconds(2));
                    });
                    AssertEqual(
                        lockIdentity,
                        journal.CurrentRecord.Identity,
                        "A mismatched endpoint changed the durable record.");
                    AssertBytesEqual(
                        lockBytes,
                        File.ReadAllBytes(journalPath));

                    AssertThrows<ArgumentException>(delegate
                    {
                        journal.ReplaceActiveLockWithUnlockBeforeDispatch(
                            lockIdentity,
                            lockIdentity,
                            "10.20.30.40",
                            4000,
                            "ReplacementGroup",
                            9,
                            0x01020304U,
                            0xA1A2A3A4U,
                            createdUtc.AddSeconds(2));
                    });
                    AssertThrows<ArgumentOutOfRangeException>(delegate
                    {
                        journal.ReplaceActiveLockWithUnlockBeforeDispatch(
                            lockIdentity,
                            unlockIdentity,
                            "10.20.30.40",
                            4000,
                            "ReplacementGroup",
                            9,
                            0x01020304U,
                            0xA1A2A3A4U,
                            createdUtc);
                    });
                    AssertEqual(
                        lockIdentity,
                        journal.CurrentRecord.Identity,
                        "A rejected replacement changed the in-memory record.");
                    AssertBytesEqual(
                        lockBytes,
                        File.ReadAllBytes(journalPath));

                    using (var blocker = new FileStream(
                        journalPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                    {
                        AssertThrows<IOException>(delegate
                        {
                            journal.ReplaceActiveLockWithUnlockBeforeDispatch(
                                lockIdentity,
                                unlockIdentity,
                                "10.20.30.40",
                                4000,
                                "ReplacementGroup",
                                9,
                                0x01020304U,
                                0xA1A2A3A4U,
                                createdUtc.AddSeconds(2));
                        });
                    }

                    AssertEqual(
                        lockIdentity,
                        journal.CurrentRecord.Identity,
                        "A failed atomic file replacement changed the in-memory record.");
                    AssertTrue(
                        journal.CurrentRecord.ExpectedProfileLocked,
                        "A failed atomic file replacement changed direction.");
                    AssertBytesEqual(
                        lockBytes,
                        File.ReadAllBytes(journalPath));

                    var replacement = journal
                        .ReplaceActiveLockWithUnlockBeforeDispatch(
                            lockIdentity,
                            unlockIdentity,
                            "10.20.30.40",
                            4000,
                            "ReplacementGroup",
                            9,
                            0x01020304U,
                            0xA1A2A3A4U,
                            createdUtc.AddSeconds(2));
                    AssertEqual(
                        unlockIdentity,
                        replacement.Identity,
                        "The replacement identity was not persisted.");
                    AssertTrue(
                        !replacement.ExpectedProfileLocked,
                        "The replacement was not an unlock record.");
                    AssertEqual(
                        GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                        replacement.State,
                        "The unlock replacement was not armed before dispatch.");
                    AssertEqual(
                        createdUtc.AddSeconds(2),
                        replacement.CreatedUtc,
                        "The unlock replacement creation time is incorrect.");
                    AssertTrue(
                        replacement.MatchesRecoveryIdentity(
                            "10.20.30.40",
                            4000,
                            "ReplacementGroup",
                            9,
                            0x01020304U,
                            0xA1A2A3A4U,
                            false),
                        "The exact unlock replacement identity did not match.");
                    AssertTrue(
                        acceptedLock.ExpectedProfileLocked
                            && lockSnapshot.ExpectedProfileLocked,
                        "A previously returned lock snapshot changed direction.");
                    AssertEqual(
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        lockSnapshot.State,
                        "A previously returned lock snapshot changed state.");

                    AssertThrows<InvalidOperationException>(delegate
                    {
                        journal.ReplaceActiveLockWithUnlockBeforeDispatch(
                            unlockIdentity,
                            otherIdentity,
                            "10.20.30.40",
                            4000,
                            "ReplacementGroup",
                            9,
                            0x01020304U,
                            0xA1A2A3A4U,
                            createdUtc.AddSeconds(3));
                    });
                    AssertEqual(
                        unlockIdentity,
                        journal.CurrentRecord.Identity,
                        "An unlock-to-unlock replacement changed the record.");
                    var acceptedUnlock = journal.MarkAccepted(
                        unlockIdentity,
                        createdUtc.AddSeconds(3));
                    AssertTrue(
                        !acceptedUnlock.ExpectedProfileLocked,
                        "The unlock direction changed after acceptance.");
                }

                using (var reopened =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    var record = reopened.CurrentRecord;
                    AssertEqual(
                        unlockIdentity,
                        record.Identity,
                        "The unlock replacement did not survive reopen.");
                    AssertTrue(
                        !record.ExpectedProfileLocked,
                        "The unlock direction did not survive reopen.");
                    AssertEqual(
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        record.State,
                        "The unlock transition did not survive reopen.");
                    reopened.Resolve(
                        unlockIdentity,
                        createdUtc.AddSeconds(4));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void StateEnumValuesRemainStable()
        {
            AssertEqual(
                1,
                (int)GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                "ArmedBeforeDispatch serialized value changed.");
            AssertEqual(
                2,
                (int)GroupProfileLockRecoveryState.RecoveryRequired,
                "RecoveryRequired serialized value changed.");
            AssertEqual(
                3,
                (int)GroupProfileLockRecoveryState.Resolved,
                "Resolved serialized value changed.");
            AssertEqual(
                4,
                (int)GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                "AcceptedAwaitingProof serialized value changed.");
        }

        private static void UnlockRecoveryExplicitRetryCanBecomeAccepted()
        {
            var unlockDirectory = CreateTemporaryDirectory();
            var lockDirectory = CreateTemporaryDirectory();
            try
            {
                var createdUtc = new DateTime(
                    638893467000000000L,
                    DateTimeKind.Utc);
                using (var unlockJournal =
                    GroupProfileLockRecoveryJournal.Open(unlockDirectory))
                {
                    var unlock = unlockJournal.ArmBeforeDispatch(
                        false,
                        "192.168.10.21",
                        5020,
                        "Group01",
                        7,
                        0x11223344U,
                        0x55667788U,
                        createdUtc);
                    unlockJournal.PromoteToRecoveryRequired(
                        unlock.Identity,
                        createdUtc.AddSeconds(1));
                    var accepted = unlockJournal.MarkAccepted(
                        unlock.Identity,
                        createdUtc.AddSeconds(2));
                    AssertRecord(
                        accepted,
                        unlock.Identity,
                        GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                        createdUtc,
                        createdUtc.AddSeconds(2),
                        false);
                }

                using (var lockJournal =
                    GroupProfileLockRecoveryJournal.Open(lockDirectory))
                {
                    var profileLock = lockJournal.ArmBeforeDispatch(
                        true,
                        "192.168.10.21",
                        5020,
                        "Group01",
                        7,
                        0x11223344U,
                        0x55667788U,
                        createdUtc);
                    lockJournal.PromoteToRecoveryRequired(
                        profileLock.Identity,
                        createdUtc.AddSeconds(1));
                    AssertThrows<InvalidOperationException>(delegate
                    {
                        lockJournal.MarkAccepted(
                            profileLock.Identity,
                            createdUtc.AddSeconds(2));
                    });
                    AssertEqual(
                        GroupProfileLockRecoveryState.RecoveryRequired,
                        lockJournal.CurrentRecord.State,
                        "Lock recovery incorrectly accepted a replay ACK.");
                }
            }
            finally
            {
                DeleteTemporaryDirectory(unlockDirectory);
                DeleteTemporaryDirectory(lockDirectory);
            }
        }

        private static GroupProfileLockRecoveryRecord CreateRecord(
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var timestamp = new DateTime(
                638893470000000000L,
                DateTimeKind.Utc);
            return new GroupProfileLockRecoveryRecord(
                Guid.NewGuid(),
                "127.0.0.1",
                5000,
                "ValidationGroup",
                1,
                diagnosticsBootId,
                mapRevision,
                GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                timestamp,
                timestamp);
        }

        private static void WriteLegacyV1Record(
            string directory,
            Guid identity,
            GroupProfileLockRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            {
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
                    writer.Write(5020);
                    writer.Write((ushort)7);
                    WriteLegacyText(writer, "192.168.10.21");
                    WriteLegacyText(writer, "Group01");
                    writer.Flush();
                }

                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var fileStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    fileStream,
                    Encoding.ASCII,
                    true))
                {
                    writer.Write(Encoding.ASCII.GetBytes("ELMOGPL1"));
                    writer.Write(1);
                    writer.Write(payload.Length);
                    writer.Write(payload);
                    writer.Flush();
                }

                prefix = fileStream.ToArray();
            }

            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(prefix);
            }

            var bytes = new byte[prefix.Length + checksum.Length];
            Buffer.BlockCopy(prefix, 0, bytes, 0, prefix.Length);
            Buffer.BlockCopy(
                checksum,
                0,
                bytes,
                prefix.Length,
                checksum.Length);
            File.WriteAllBytes(
                Path.Combine(
                    directory,
                    GroupProfileLockRecoveryJournal.JournalFileName),
                bytes);
        }

        private static void WriteLegacyText(
            BinaryWriter writer,
            string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static void AssertCorruptionRejected(
            Action<byte[]> corrupt)
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal =
                    GroupProfileLockRecoveryJournal.Open(directory))
                {
                    journal.ArmBeforeDispatch(
                        "192.168.1.30",
                        5001,
                        "CorruptionGroup",
                        2,
                        11,
                        22,
                        new DateTime(
                            638893480000000000L,
                            DateTimeKind.Utc));
                }

                var path = Path.Combine(
                    directory,
                    GroupProfileLockRecoveryJournal.JournalFileName);
                var bytes = File.ReadAllBytes(path);
                corrupt(bytes);
                File.WriteAllBytes(path, bytes);

                AssertThrows<InvalidDataException>(delegate
                {
                    using (GroupProfileLockRecoveryJournal.Open(directory))
                    {
                    }
                });
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void AssertOversizedJournalRejected()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                File.WriteAllBytes(
                    Path.Combine(
                        directory,
                        GroupProfileLockRecoveryJournal.JournalFileName),
                    new byte[16385]);
                AssertThrows<InvalidDataException>(delegate
                {
                    using (GroupProfileLockRecoveryJournal.Open(directory))
                    {
                    }
                });
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
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

        private static void WriteInt32(
            byte[] destination,
            int offset,
            int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, destination, offset, bytes.Length);
        }

        private static void AssertBytesEqual(
            byte[] expected,
            byte[] actual)
        {
            AssertEqual(
                expected.Length,
                actual.Length,
                "Durable journal byte length changed.");
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEqual(
                    expected[index],
                    actual[index],
                    "Durable journal byte changed at offset " + index + ".");
            }
        }

        private static void AssertRecord(
            GroupProfileLockRecoveryRecord record,
            Guid identity,
            GroupProfileLockRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc,
            bool expectedProfileLocked = true)
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
                "Group01",
                record.GroupName,
                "Group name did not roundtrip.");
            AssertEqual(
                (ushort)7,
                record.GroupReference,
                "Group reference did not roundtrip.");
            AssertEqual(
                0x11223344U,
                record.DiagnosticsBootId,
                "Diagnostics BootId did not roundtrip.");
            AssertEqual(
                0x55667788U,
                record.MapRevision,
                "Map revision did not roundtrip.");
            AssertEqual(
                expectedProfileLocked,
                record.ExpectedProfileLocked,
                "Profile-lock direction did not roundtrip.");
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

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoGroupProfileLockJournalTests",
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using LasalMotionControlApiExample;

namespace LasalMotionControlLib.Tests
{
    internal static class RecorderDoubleRecoveryJournalTests
    {
        private static readonly Guid RecoveryIdentity = new Guid(
            "71c43f9e-8612-4a4b-9d0f-f046e20b69d7");
        private static readonly DateTime CreatedUtc = new DateTime(
            638893440000000000L,
            DateTimeKind.Utc);

        private const uint BootId = 0x10203040u;
        private const uint MapRevision = 0x50607080u;
        private const uint RequestedConfigId = 0x90A0B0C0u;
        private const uint ConfigRevision = 0xD0E0F001u;
        private const uint RecordA = 0x01020304u;
        private const uint RecordB = 0x05060708u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalArmsBeforeDispatch",
                RecoveryJournalArmsBeforeDispatch);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalMergesReplyAndInventory",
                RecoveryJournalMergesReplyAndInventory);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalUnknownRevisionRejectsRemoteEvidence",
                RecoveryJournalUnknownRevisionRejectsRemoteEvidence);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalRejectsIdentityDrift",
                RecoveryJournalRejectsIdentityDrift);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalRejectsAmbiguousBanks",
                RecoveryJournalRejectsAmbiguousBanks);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalLifecycleInterlock",
                RecoveryJournalLifecycleInterlock);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalDeterministicSerialization",
                RecoveryJournalDeterministicSerialization);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalV2ReadsLegacyUnbound",
                RecoveryJournalV2ReadsLegacyUnbound);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalResolvesTokenAbsenceWithoutRelease",
                RecoveryJournalResolvesTokenAbsenceWithoutRelease);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalValueOnlySchema",
                RecoveryJournalValueOnlySchema);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalCorruptionQuarantine",
                RecoveryJournalCorruptionQuarantine);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalUnsupportedVersion",
                RecoveryJournalUnsupportedVersion);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalV1FailsClosed",
                RecoveryJournalV1FailsClosed);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryJournalSingleWriterAndAtomicFiles",
                RecoveryJournalSingleWriterAndAtomicFiles);
        }

        private static void RecoveryJournalArmsBeforeDispatch()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    using (var journal =
                        RecorderDoubleRecoveryJournal.Open(directoryPath))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        var armed = journal.ArmBeforeConfigureDispatch(
                            RecoveryIdentity,
                            CreatedUtc,
                            BootId,
                            MapRevision,
                            RequestedConfigId);
                        AssertRecord(
                            armed,
                            RecorderDoubleRecoveryState
                                .ArmedBeforeConfigureDispatch,
                            CreatedUtc,
                            0,
                            new RecorderDoubleRecoveryBankEvidence[0]);
                        AssertEx.True(File.Exists(journal.JournalFilePath));
                        var wire = File.ReadAllBytes(
                            journal.JournalFilePath);
                        AssertEx.Equal((byte)3, wire[8]);
                        AssertEx.SequenceEqual(
                            RecoveryIdentity.ToByteArray(),
                            wire.Skip(16).Take(16).ToArray(),
                            "Schema v3 must persist the Guid raw bytes used as the wire recovery token.");
                        AssertEx.Equal(
                            (byte)RecorderDoubleRecoveryTokenMarker
                                .ClientTokenV1,
                            wire[wire.Length - 36],
                            "Schema v3 must persist an explicit ClientTokenV1 marker.");
                    }

                    using (var reopened =
                        RecorderDoubleRecoveryJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                        AssertRecord(
                            reopened.CurrentRecord,
                            RecorderDoubleRecoveryState
                                .ArmedBeforeConfigureDispatch,
                            CreatedUtc,
                            0,
                            new RecorderDoubleRecoveryBankEvidence[0]);
                    }
                });
        }

        private static void RecoveryJournalMergesReplyAndInventory()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    using (var journal = Arm(directoryPath))
                    {
                        var configured = journal.RecordConfigurationReply(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(1),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision);
                        AssertRecord(
                            configured,
                            RecorderDoubleRecoveryState
                                .ConfigurationIdentified,
                            CreatedUtc.AddSeconds(1),
                            ConfigRevision,
                            new RecorderDoubleRecoveryBankEvidence[0]);

                        var first = journal.RecordCaptureReply(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(2),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision,
                            RecordA,
                            0);
                        AssertBanks(
                            first,
                            new RecorderDoubleRecoveryBankEvidence(
                                0,
                                RecordA));

                        var complete = journal.RecordInventory(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(3),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision,
                            new[]
                            {
                                new RecorderDoubleRecoveryBankEvidence(
                                    1,
                                    RecordB),
                                new RecorderDoubleRecoveryBankEvidence(
                                    0,
                                    RecordA)
                            });
                        AssertRecord(
                            complete,
                            RecorderDoubleRecoveryState
                                .CaptureEvidenceAvailable,
                            CreatedUtc.AddSeconds(3),
                            ConfigRevision,
                            new[]
                            {
                                new RecorderDoubleRecoveryBankEvidence(
                                    0,
                                    RecordA),
                                new RecorderDoubleRecoveryBankEvidence(
                                    1,
                                    RecordB)
                            });

                        var bytesBeforeIdempotent = File.ReadAllBytes(
                            journal.JournalFilePath);
                        var repeated = journal.RecordInventory(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(4),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision,
                            new[]
                            {
                                new RecorderDoubleRecoveryBankEvidence(
                                    0,
                                    RecordA),
                                new RecorderDoubleRecoveryBankEvidence(
                                    1,
                                    RecordB)
                            });
                        AssertEx.Equal(
                            CreatedUtc.AddSeconds(3),
                            repeated.UpdatedUtc,
                            "Idempotent inventory must not rewrite durable evidence.");
                        AssertEx.SequenceEqual(
                            bytesBeforeIdempotent,
                            File.ReadAllBytes(journal.JournalFilePath));
                    }

                    using (var reopened =
                        RecorderDoubleRecoveryJournal.Open(directoryPath))
                    {
                        AssertBanks(
                            reopened.CurrentRecord,
                            new RecorderDoubleRecoveryBankEvidence(
                                0,
                                RecordA),
                            new RecorderDoubleRecoveryBankEvidence(
                                1,
                                RecordB));
                    }
                });
        }

        private static void RecoveryJournalUnknownRevisionRejectsRemoteEvidence()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    using (var journal = Arm(directoryPath))
                    {
                        var before = File.ReadAllBytes(
                            journal.JournalFilePath);
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordInventory(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(1),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision,
                                new[]
                                {
                                    new RecorderDoubleRecoveryBankEvidence(
                                        0,
                                        RecordA)
                                }));
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordCaptureReply(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(1),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision,
                                RecordA,
                                0));
                        AssertEx.SequenceEqual(
                            before,
                            File.ReadAllBytes(journal.JournalFilePath));
                        AssertRecord(
                            journal.CurrentRecord,
                            RecorderDoubleRecoveryState
                                .ArmedBeforeConfigureDispatch,
                            CreatedUtc,
                            0,
                            new RecorderDoubleRecoveryBankEvidence[0]);
                    }
                });
        }

        private static void RecoveryJournalRejectsIdentityDrift()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    using (var journal = Arm(directoryPath))
                    {
                        var before = File.ReadAllBytes(
                            journal.JournalFilePath);
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordConfigurationReply(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(1),
                                BootId + 1,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision));
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordConfigurationReply(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(1),
                                BootId,
                                RequestedConfigId + 1,
                                ConfigRevision,
                                MapRevision));
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordConfigurationReply(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(1),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision + 1));
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordConfigurationReply(
                                Guid.NewGuid(),
                                CreatedUtc.AddSeconds(1),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision));
                        AssertEx.SequenceEqual(
                            before,
                            File.ReadAllBytes(journal.JournalFilePath),
                            "Rejected identity drift must not rewrite the journal.");

                        journal.RecordConfigurationReply(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(2),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision);
                        var identified = File.ReadAllBytes(
                            journal.JournalFilePath);
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordConfigurationReply(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(3),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision + 1,
                                MapRevision));
                        AssertEx.SequenceEqual(
                            identified,
                            File.ReadAllBytes(journal.JournalFilePath));
                    }
                });
        }

        private static void RecoveryJournalRejectsAmbiguousBanks()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new RecorderDoubleRecoveryBankEvidence(2, RecordA));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new RecorderDoubleRecoveryBankEvidence(0, 0));

            WithTestDirectory(
                directoryPath =>
                {
                    using (var journal = Arm(directoryPath))
                    {
                        journal.RecordConfigurationReply(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(1),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision);
                        journal.RecordCaptureReply(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(2),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision,
                            RecordA,
                            0);
                        var before = File.ReadAllBytes(
                            journal.JournalFilePath);
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordCaptureReply(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(3),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision,
                                RecordB,
                                0));
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.RecordCaptureReply(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(3),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision,
                                RecordA,
                                1));
                        AssertEx.Throws<ArgumentException>(
                            () => journal.RecordInventory(
                                RecoveryIdentity,
                                CreatedUtc.AddSeconds(3),
                                BootId,
                                RequestedConfigId,
                                ConfigRevision,
                                MapRevision,
                                new[]
                                {
                                    new RecorderDoubleRecoveryBankEvidence(
                                        0,
                                        RecordA),
                                    new RecorderDoubleRecoveryBankEvidence(
                                        0,
                                        RecordB)
                                }));
                        AssertEx.SequenceEqual(
                            before,
                            File.ReadAllBytes(journal.JournalFilePath),
                            "Ambiguous bank evidence must not rewrite the journal.");
                    }
                });
        }

        private static void RecoveryJournalLifecycleInterlock()
        {
            AssertEx.Throws<ArgumentException>(
                () => new RecorderDoubleRecoveryRecord(
                    Guid.Empty,
                    RecorderDoubleRecoveryState
                        .ArmedBeforeConfigureDispatch,
                    CreatedUtc,
                    CreatedUtc,
                    BootId,
                    MapRevision,
                    RequestedConfigId,
                    0,
                    new RecorderDoubleRecoveryBankEvidence[0]));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new RecorderDoubleRecoveryRecord(
                    RecoveryIdentity,
                    RecorderDoubleRecoveryState
                        .ArmedBeforeConfigureDispatch,
                    CreatedUtc,
                    CreatedUtc,
                    BootId,
                    MapRevision,
                    0,
                    0,
                    new RecorderDoubleRecoveryBankEvidence[0]));

            WithTestDirectory(
                directoryPath =>
                {
                    using (var journal = Arm(directoryPath))
                    {
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.ArmBeforeConfigureDispatch(
                                Guid.NewGuid(),
                                CreatedUtc.AddSeconds(1),
                                BootId,
                                MapRevision,
                                RequestedConfigId + 1));
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            RecorderDoubleRecoveryState
                                .ArmedBeforeConfigureDispatch,
                            journal.CurrentRecord.State);
                    }
                });
        }

        private static void RecoveryJournalDeterministicSerialization()
        {
            WithTwoTestDirectories(
                (firstDirectory, secondDirectory) =>
                {
                    WriteCompleteRecord(firstDirectory);
                    WriteCompleteRecord(secondDirectory);
                    var firstPath = Path.Combine(
                        firstDirectory,
                        RecorderDoubleRecoveryJournal.JournalFileName);
                    var secondPath = Path.Combine(
                        secondDirectory,
                        RecorderDoubleRecoveryJournal.JournalFileName);
                    var first = File.ReadAllBytes(firstPath);
                    var second = File.ReadAllBytes(secondPath);
                    AssertEx.SequenceEqual(first, second);
                    AssertEx.Equal(
                        128,
                        first.Length,
                        "Schema v3 with two banks must remain fixed and reviewable.");
                });
        }

        private static void RecoveryJournalV2ReadsLegacyUnbound()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    string journalPath;
                    using (var journal = Arm(directoryPath))
                    {
                        journalPath = journal.JournalFilePath;
                    }

                    var v2 = ConvertV3ToV2(
                        File.ReadAllBytes(journalPath));
                    File.WriteAllBytes(journalPath, v2);
                    using (var reopened =
                        RecorderDoubleRecoveryJournal.Open(directoryPath))
                    {
                        AssertEx.Equal(
                            RecorderDoubleRecoveryTokenMarker.LegacyUnbound,
                            reopened.CurrentRecord.RecoveryTokenMarker);
                        AssertEx.Equal(
                            Guid.Empty,
                            reopened.CurrentRecord.RecoveryToken);
                        AssertEx.Equal(
                            (uint)0,
                            reopened.CurrentRecord.ConfigRevision);
                        AssertEx.True(reopened.CurrentRecord.IsActive);
                    }
                });
        }

        private static void
            RecoveryJournalResolvesTokenAbsenceWithoutRelease()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    using (var journal = Arm(directoryPath))
                    {
                        var resolved = journal.ResolveWithoutConfiguration(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(1),
                            BootId,
                            RequestedConfigId,
                            MapRevision,
                            RecoveryIdentity);
                        AssertEx.Equal(
                            RecorderDoubleRecoveryState
                                .ResolvedWithoutConfiguration,
                            resolved.State);
                        AssertEx.Equal((uint)0, resolved.ConfigRevision);
                        AssertEx.False(resolved.ConfigurationReleaseIntent);
                        AssertEx.False(
                            resolved.ConfigurationReleaseConfirmed);
                        AssertEx.False(resolved.HasReleaseOutcomeUncertain);
                        AssertEx.False(resolved.IsActive);
                    }

                    using (var reopened =
                        RecorderDoubleRecoveryJournal.Open(directoryPath))
                    {
                        AssertEx.Equal(
                            RecorderDoubleRecoveryState
                                .ResolvedWithoutConfiguration,
                            reopened.CurrentRecord.State);
                        AssertEx.False(reopened.HasActiveRecord);
                    }
                });
        }

        private static void RecoveryJournalValueOnlySchema()
        {
            var forbiddenTypeNames = new[]
            {
                "Object",
                "Byte[]",
                "String",
                "Exception",
                "CancellationToken",
                "Task"
            };
            var types = new[]
            {
                typeof(RecorderDoubleRecoveryRecord),
                typeof(RecorderDoubleRecoveryBankEvidence)
            };
            for (var typeIndex = 0;
                typeIndex < types.Length;
                typeIndex++)
            {
                var properties = types[typeIndex].GetProperties(
                    BindingFlags.Instance
                        | BindingFlags.NonPublic
                        | BindingFlags.Public);
                for (var propertyIndex = 0;
                    propertyIndex < properties.Length;
                    propertyIndex++)
                {
                    var propertyTypeName = properties[propertyIndex]
                        .PropertyType.Name;
                    for (var forbiddenIndex = 0;
                        forbiddenIndex < forbiddenTypeNames.Length;
                        forbiddenIndex++)
                    {
                        AssertEx.False(
                            string.Equals(
                                forbiddenTypeNames[forbiddenIndex],
                                propertyTypeName,
                                StringComparison.Ordinal),
                            types[typeIndex].Name
                                + "."
                                + properties[propertyIndex].Name
                                + " exposes forbidden recovery state type "
                                + propertyTypeName
                                + ".");
                    }
                }
            }

            AssertEx.Equal(
                typeof(uint),
                typeof(RecorderDoubleRecoveryBankEvidence)
                    .GetProperty(
                        "RecordId",
                        BindingFlags.Instance
                            | BindingFlags.NonPublic)
                    .PropertyType);
        }

        private static void RecoveryJournalCorruptionQuarantine()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    string journalPath;
                    using (var journal = Arm(directoryPath))
                    {
                        journalPath = journal.JournalFilePath;
                    }

                    var corrupt = File.ReadAllBytes(journalPath);
                    corrupt[24] ^= 0x5A;
                    File.WriteAllBytes(journalPath, corrupt);
                    var error = AssertEx.Throws<
                        RecorderDoubleRecoveryJournalCorruptException>(
                            () =>
                            {
                                using (RecorderDoubleRecoveryJournal.Open(
                                    directoryPath))
                                {
                                }
                            });
                    AssertEx.Equal(journalPath, error.ActiveFilePath);
                    AssertEx.True(File.Exists(journalPath));
                    AssertEx.True(File.Exists(error.QuarantineFilePath));
                    AssertEx.SequenceEqual(
                        corrupt,
                        File.ReadAllBytes(journalPath),
                        "Fail-closed open must preserve the active corrupt file.");
                    AssertEx.SequenceEqual(
                        corrupt,
                        File.ReadAllBytes(error.QuarantineFilePath),
                        "Quarantine must be an exact copy of the rejected file.");

                    var second = AssertEx.Throws<
                        RecorderDoubleRecoveryJournalCorruptException>(
                            () =>
                            {
                                using (RecorderDoubleRecoveryJournal.Open(
                                    directoryPath))
                                {
                                }
                            });
                    AssertEx.Equal(
                        error.QuarantineFilePath,
                        second.QuarantineFilePath,
                        "The checksum-derived quarantine path must be stable.");
                    AssertEx.Equal(
                        1,
                        Directory.GetFiles(
                            directoryPath,
                            RecorderDoubleRecoveryJournal
                                .QuarantineFilePrefix
                                + "*.dat").Length,
                        "Repeated rejection must not create duplicate quarantine copies.");
                });
        }

        private static void RecoveryJournalUnsupportedVersion()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    string journalPath;
                    using (var journal = Arm(directoryPath))
                    {
                        journalPath = journal.JournalFilePath;
                    }

                    var bytes = File.ReadAllBytes(journalPath);
                    WriteInt32LittleEndian(bytes, 8, 4);
                    RewriteChecksum(bytes);
                    File.WriteAllBytes(journalPath, bytes);
                    AssertEx.Throws<NotSupportedException>(
                        () =>
                        {
                            using (RecorderDoubleRecoveryJournal.Open(
                                directoryPath))
                            {
                            }
                        });
                    AssertEx.True(File.Exists(journalPath));
                    AssertEx.Equal(
                        0,
                        Directory.GetFiles(
                            directoryPath,
                            RecorderDoubleRecoveryJournal
                                .QuarantineFilePrefix
                                + "*.dat").Length,
                        "A checksum-valid future schema is unsupported, not corrupt.");
                });
        }

        private static void RecoveryJournalV1FailsClosed()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    string journalPath;
                    using (var journal = Arm(directoryPath))
                    {
                        journalPath = journal.JournalFilePath;
                    }

                    var bytes = File.ReadAllBytes(journalPath);
                    WriteInt32LittleEndian(bytes, 8, 1);
                    RewriteChecksum(bytes);
                    File.WriteAllBytes(journalPath, bytes);
                    AssertEx.Throws<NotSupportedException>(
                        () =>
                        {
                            using (RecorderDoubleRecoveryJournal.Open(
                                directoryPath))
                            {
                            }
                        });
                    AssertEx.True(
                        File.Exists(journalPath),
                        "A legacy v1 journal must remain in place for explicit operator handling.");
                });
        }

        private static void RecoveryJournalSingleWriterAndAtomicFiles()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    using (var first = Arm(directoryPath))
                    {
                        AssertEx.Throws<IOException>(
                            () =>
                            {
                                using (RecorderDoubleRecoveryJournal.Open(
                                    directoryPath))
                                {
                                }
                            });
                        first.RecordConfigurationReply(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(1),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision);
                        first.RecordCaptureReply(
                            RecoveryIdentity,
                            CreatedUtc.AddSeconds(2),
                            BootId,
                            RequestedConfigId,
                            ConfigRevision,
                            MapRevision,
                            RecordA,
                            0);
                        AssertEx.Equal(
                            0,
                            Directory.GetFiles(
                                directoryPath,
                                "*.tmp").Length,
                            "Successful atomic replacements must leave no temporary files.");
                    }

                    using (var reopened =
                        RecorderDoubleRecoveryJournal.Open(directoryPath))
                    {
                        AssertBanks(
                            reopened.CurrentRecord,
                            new RecorderDoubleRecoveryBankEvidence(
                                0,
                                RecordA));
                    }
                });
        }

        private static RecorderDoubleRecoveryJournal Arm(
            string directoryPath)
        {
            var journal = RecorderDoubleRecoveryJournal.Open(directoryPath);
            try
            {
                journal.ArmBeforeConfigureDispatch(
                    RecoveryIdentity,
                    CreatedUtc,
                    BootId,
                    MapRevision,
                    RequestedConfigId);
                return journal;
            }
            catch
            {
                journal.Dispose();
                throw;
            }
        }

        private static void WriteCompleteRecord(string directoryPath)
        {
            using (var journal = Arm(directoryPath))
            {
                journal.RecordConfigurationReply(
                    RecoveryIdentity,
                    CreatedUtc.AddSeconds(1),
                    BootId,
                    RequestedConfigId,
                    ConfigRevision,
                    MapRevision);
                journal.RecordInventory(
                    RecoveryIdentity,
                    CreatedUtc.AddSeconds(2),
                    BootId,
                    RequestedConfigId,
                    ConfigRevision,
                    MapRevision,
                    new[]
                    {
                        new RecorderDoubleRecoveryBankEvidence(
                            1,
                            RecordB),
                        new RecorderDoubleRecoveryBankEvidence(
                            0,
                            RecordA)
                    });
            }
        }

        private static void AssertRecord(
            RecorderDoubleRecoveryRecord record,
            RecorderDoubleRecoveryState state,
            DateTime updatedUtc,
            uint configRevision,
            IReadOnlyList<RecorderDoubleRecoveryBankEvidence> banks)
        {
            AssertEx.NotNull(record);
            AssertEx.Equal(RecoveryIdentity, record.Identity);
            AssertEx.Equal(state, record.State);
            AssertEx.Equal(CreatedUtc, record.CreatedUtc);
            AssertEx.Equal(updatedUtc, record.UpdatedUtc);
            AssertEx.Equal(BootId, record.DiagnosticsBootId);
            AssertEx.Equal(MapRevision, record.MapRevision);
            AssertEx.Equal(RequestedConfigId, record.RequestedConfigId);
            AssertEx.Equal(configRevision, record.ConfigRevision);
            AssertEx.Equal(
                RecorderDoubleRecoveryTokenMarker.ClientTokenV1,
                record.RecoveryTokenMarker);
            AssertEx.Equal(record.Identity, record.RecoveryToken);
            AssertBanks(record, banks.ToArray());
        }

        private static void AssertBanks(
            RecorderDoubleRecoveryRecord record,
            params RecorderDoubleRecoveryBankEvidence[] expected)
        {
            AssertEx.Equal(expected.Length, record.Banks.Count);
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(
                    expected[index].BufferId,
                    record.Banks[index].BufferId);
                AssertEx.Equal(
                    expected[index].RecordId,
                    record.Banks[index].RecordId);
            }
        }

        private static void RewriteChecksum(byte[] bytes)
        {
            var checksumOffset = bytes.Length - 32;
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

        private static byte[] ConvertV3ToV2(byte[] v3)
        {
            const int envelopeLength = 16;
            const int checksumLength = 32;
            const int markerLength = 4;
            var payloadLength = v3.Length
                - envelopeLength
                - checksumLength;
            var v2 = new byte[v3.Length - markerLength];
            Buffer.BlockCopy(v3, 0, v2, 0, envelopeLength);
            WriteInt32LittleEndian(v2, 8, 2);
            WriteInt32LittleEndian(
                v2,
                12,
                payloadLength - markerLength);
            Buffer.BlockCopy(
                v3,
                envelopeLength,
                v2,
                envelopeLength,
                payloadLength - markerLength);
            RewriteChecksum(v2);
            return v2;
        }

        private static void WriteInt32LittleEndian(
            byte[] target,
            int offset,
            int value)
        {
            target[offset] = (byte)value;
            target[offset + 1] = (byte)(value >> 8);
            target[offset + 2] = (byte)(value >> 16);
            target[offset + 3] = (byte)(value >> 24);
        }

        private static void WithTestDirectory(Action<string> action)
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "ElmoRecorderDoubleRecoveryJournalTests",
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
            WithTestDirectory(
                first => WithTestDirectory(
                    second => action(first, second)));
        }
    }
}

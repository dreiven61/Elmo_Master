using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class MaintenanceActionRecoveryJournalTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.MaintenanceJournal.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.MaintenanceJournal.RestartQuarantine",
                ArmedRecordBecomesRecoveryRequiredAfterRestart);
            tests.Add(
                "Wpf.MaintenanceJournal.FullLifecycle",
                FullLifecyclePreservesExactIdentity);
            tests.Add(
                "Wpf.MaintenanceJournal.StaleCasAndSingleWriter",
                StaleSnapshotAndSecondWriterAreRejected);
            tests.Add(
                "Wpf.MaintenanceJournal.BoundsAndChecksum",
                BoundsAndChecksumAreEnforced);
            tests.Add(
                "Wpf.MaintenanceJournal.CurrentIdentityAndLegacyV1V2V3V4",
                CurrentIdentityAndLegacyV1V2V3V4AreFailClosed);
            tests.Add(
                "Wpf.MaintenanceJournal.ResolvedLegacyV1V2V3V4RemainsInert",
                ResolvedLegacyV1V2V3V4RemainInertAcrossRepeatedOpen);
            tests.Add(
                "Wpf.MaintenanceJournal.NonzeroTransportCorrelation",
                TransportCorrelationMustBeNonZero);
            tests.Add(
                "Wpf.MaintenanceJournal.ConfirmedRejectionResolves",
                ConfirmedRejectionResolvesWithoutRecoveryQuarantine);
        }

        private static void DefaultPathIsVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "MaintenanceActionRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                MaintenanceActionRecoveryJournal.GetDefaultDirectoryPath()
                    .ToUpperInvariant());
        }

        private static void ArmedRecordBecomesRecoveryRequiredAfterRestart()
        {
            WithTemporaryDirectory(delegate(string directory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(
                        journal,
                        MaintenanceActionKind.LmcHome,
                        0x10203040U,
                        "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=123;TargetPosition=0;TimeoutMs=1000");
                    AssertEx.Equal(
                        MaintenanceActionRecoveryState.ArmedBeforeDispatch,
                        armed.State);
                    AssertEx.False(journal.RecoveredAtStartup);
                }

                using (var reopened =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    AssertEx.True(reopened.RecoveredAtStartup);
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        MaintenanceActionRecoveryState.RecoveryRequired,
                        reopened.CurrentRecord.State);
                    AssertEx.Equal(
                        0x10203040U,
                        reopened.CurrentRecord.TransportCorrelationId);
                    AssertEx.Equal(
                        MaintenanceActionKind.LmcHome,
                        reopened.CurrentRecord.Action);
                }
            });
        }

        private static void FullLifecyclePreservesExactIdentity()
        {
            WithTemporaryDirectory(delegate(string directory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(
                        journal,
                        MaintenanceActionKind
                            .EncoderTw20ErrorWarningReset,
                        77,
                        EncoderParameters(true, 4, 2));
                    AssertEx.Equal("127.0.0.1", armed.EndpointIp);
                    AssertEx.Equal(4000, armed.EndpointPort);
                    AssertEx.Equal(
                        0x01020304U,
                        armed.ObservedDiagnosticsBuild);
                    AssertEx.Equal(
                        0x11223344U,
                        armed.ObservedDiagnosticsBootId);
                    AssertEx.Equal(
                        0x55667788U,
                        armed.ObservedMapRevision);
                    AssertEx.Equal("_LMCAxis4", armed.AxisName);
                    AssertEx.Equal((ushort)4, armed.AxisReference);
                    AssertEx.Equal(0x89ABCDEFU, armed.ClientIntentId0);
                    AssertEx.Equal(0x01234567U, armed.ClientIntentId1);
                    AssertEx.Equal(0x76543210U, armed.ClientIntentId2);
                    AssertEx.Equal(0xFEDCBA98U, armed.ClientIntentId3);

                    var promoted = journal.PromoteToRecoveryRequired(
                        armed,
                        77,
                        FixedUtc().AddSeconds(1));
                    AssertEx.Equal(
                        MaintenanceActionRecoveryState.RecoveryRequired,
                        promoted.State);
                    AssertEx.Equal(77U, promoted.TransportCorrelationId);
                    AssertEx.True(promoted.ExactEquals(journal.CurrentRecord));

                    var resolved = journal.Resolve(
                        promoted,
                        FixedUtc().AddSeconds(2));
                    AssertEx.Equal(
                        MaintenanceActionRecoveryState.Resolved,
                        resolved.State);
                    AssertEx.False(resolved.IsActive);
                    AssertEx.False(journal.HasActiveRecord);
                }

                using (var reopened =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    AssertEx.False(reopened.RecoveredAtStartup);
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(
                        MaintenanceActionRecoveryState.Resolved,
                        reopened.CurrentRecord.State);
                }
            });
        }

        private static void StaleSnapshotAndSecondWriterAreRejected()
        {
            WithTemporaryDirectory(delegate(string directory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(
                        journal,
                        MaintenanceActionKind.Ds402Home,
                        9,
                        "Schema=1;Method=37;HomeOffset=0;Velocity=0;Acceleration=0;DistanceLimit=0;TorqueLimit=0;BufferMode=Aborting;TimeoutMs=1000");
                    AssertEx.Throws<IOException>(
                        () => MaintenanceActionRecoveryJournal.Open(directory));

                    var promoted = journal.PromoteToRecoveryRequired(
                        armed,
                        9,
                        FixedUtc().AddSeconds(1));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.Resolve(
                            armed,
                            FixedUtc().AddSeconds(2)));
                    AssertEx.True(
                        promoted.ExactEquals(journal.CurrentRecord),
                        "A stale recovery transition mutated the record.");
                }
            });
        }

        private static void BoundsAndChecksumAreEnforced()
        {
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new MaintenanceActionRecoveryRecord(
                    Guid.NewGuid(),
                    MaintenanceActionKind.LmcHome,
                    "127.0.0.1",
                    4000,
                    1,
                    2,
                    3,
                    "_LMCAxis5",
                    5,
                    1,
                    2,
                    3,
                    4,
                    5,
                    "Recipe=1",
                    MaintenanceActionRecoveryState.ArmedBeforeDispatch,
                    FixedUtc(),
                    FixedUtc()));
            WithTemporaryDirectory(delegate(string invalidDirectory)
            {
                AssertEx.Throws<ArgumentException>(
                    () =>
                    {
                        using (var journal =
                            MaintenanceActionRecoveryJournal.Open(
                                invalidDirectory))
                        {
                            journal.ArmBeforeDispatch(
                                MaintenanceActionKind.LmcHome,
                                "127.0.0.1",
                                4000,
                                1,
                                2,
                                3,
                                "_LMCAxis1",
                                1,
                                0,
                                0,
                                0,
                                0,
                                5,
                                "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=0;TargetPosition=0;TimeoutMs=1000",
                                FixedUtc());
                        }
                    });
            });
            WithTemporaryDirectory(delegate(string invalidDirectory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(
                        invalidDirectory))
                {
                    AssertEx.Throws<ArgumentException>(
                        () => journal.ArmBeforeDispatch(
                            MaintenanceActionKind
                                .EncoderTw20ErrorWarningReset,
                            "127.0.0.1",
                            4000,
                            1,
                            2,
                            3,
                            "_LMCAxis1",
                            1,
                            1,
                            2,
                            3,
                            4,
                            0,
                            "Schema=1;Semantic=Tw20ErrorWarningReset;Kind=1;Profile=1;Drive=1;Socket=1;CommandValue=1;Object=0x20FC;Sub=0x02;Type=UInt16;TimeoutMilliseconds=1000;Evidence0=1;Evidence1=2;Evidence2=3",
                            FixedUtc()));
                }
            });
            WithTemporaryDirectory(delegate(string invalidDirectory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(
                        invalidDirectory))
                {
                    AssertEx.Throws<ArgumentException>(
                        () => journal.ArmBeforeDispatch(
                            MaintenanceActionKind
                                .EncoderTw20ErrorWarningReset,
                            "127.0.0.1",
                            4000,
                            1,
                            2,
                            3,
                            "_LMCAxis1",
                            1,
                            1,
                            2,
                            3,
                            4,
                            5,
                            "Schema=1;Semantic=Tw20ErrorWarningReset;Kind=1;Profile=1;Drive=1;Socket=1;CommandValue=1;Object=0x20FC;Sub=0x02;Type=UInt16;TimeoutCycles=1000;Evidence0=1;Evidence1=2;Evidence2=3;Evidence3=4",
                            FixedUtc()));
                }
            });
            WithTemporaryDirectory(delegate(string invalidDirectory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(
                        invalidDirectory))
                {
                    AssertEx.Throws<ArgumentException>(
                        () => Arm(
                            journal,
                            MaintenanceActionKind
                                .EncoderTw19MultiturnPositionReset,
                            6,
                            EncoderParameters(false, 4, 2).Replace(
                                "CommandValue=1",
                                "CommandValue=2")));
                }
            });

            WithTemporaryDirectory(delegate(string directory)
            {
                string path;
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    Arm(
                        journal,
                        MaintenanceActionKind.LmcHome,
                        5,
                        "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=0;TargetPosition=0;TimeoutMs=1000");
                    path = Path.Combine(
                        directory,
                        MaintenanceActionRecoveryJournal.JournalFileName);
                }

                var bytes = File.ReadAllBytes(path);
                bytes[bytes.Length / 2] ^= 0x5A;
                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(
                    () => MaintenanceActionRecoveryJournal.Open(directory));
            });
        }

        private static void CurrentIdentityAndLegacyV1V2V3V4AreFailClosed()
        {
            foreach (var legacyVersion in new[] { 1, 2, 3, 4 })
            {
                WithTemporaryDirectory(delegate(string directory)
                {
                    string path;
                    using (var journal =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        var armed = Arm(
                            journal,
                            MaintenanceActionKind.LmcHome,
                            123,
                            "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=-25;TargetPosition=0;TimeoutMs=1000");
                        AssertEx.True(armed.HasAnyClientIntent);
                        AssertEx.Equal(123U, armed.TransportCorrelationId);
                        path = Path.Combine(
                            directory,
                            MaintenanceActionRecoveryJournal.JournalFileName);
                    }

                    RewriteFormatVersion(path, legacyVersion);
                    var error = AssertEx.Throws<InvalidDataException>(
                        () => MaintenanceActionRecoveryJournal.Open(directory));
                    AssertEx.Contains(
                        "version " + legacyVersion,
                        error.Message);
                    AssertEx.Contains("must not be reinterpreted", error.Message);
                    AssertEx.Contains("must not be", error.Message);
                });
            }

            foreach (var legacyVersion in new[] { 1, 2, 3, 4 })
            {
                WithTemporaryDirectory(delegate(string directory)
                {
                    string path;
                    using (var journal =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        Arm(
                            journal,
                            MaintenanceActionKind
                                .EncoderTw20ErrorWarningReset,
                            0x55667788U,
                            EncoderParameters(true, 4, 1));
                        path = Path.Combine(
                            directory,
                            MaintenanceActionRecoveryJournal.JournalFileName);
                    }

                    RewriteFormatVersion(path, legacyVersion);
                    var error = AssertEx.Throws<InvalidDataException>(
                        () => MaintenanceActionRecoveryJournal.Open(directory));
                    AssertEx.Contains(
                        "version " + legacyVersion,
                        error.Message);
                    AssertEx.Contains("must not be reinterpreted", error.Message);
                });
            }
        }

        private static void
            ResolvedLegacyV1V2V3V4RemainInertAcrossRepeatedOpen()
        {
            foreach (var legacyVersion in new[] { 1, 2, 3, 4 })
            {
                WithTemporaryDirectory(delegate(string directory)
                {
                    string path;
                    using (var journal =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        var armed = Arm(
                            journal,
                            MaintenanceActionKind.Ds402Home,
                            0x10203040U,
                            "Schema=1;Method=37;HomeOffset=0;Velocity=0;Acceleration=0;DistanceLimit=0;TorqueLimit=0;BufferMode=Aborting;TimeoutMs=1000");
                        journal.ResolveConfirmedRejection(
                            armed,
                            armed.TransportCorrelationId,
                            FixedUtc().AddSeconds(1));
                        path = Path.Combine(
                            directory,
                            MaintenanceActionRecoveryJournal.JournalFileName);
                    }

                    RewriteFormatVersion(path, legacyVersion);
                    ReplaceAsciiJournalValue(
                        path,
                        "Velocity=0",
                        "Velocity=1");
                    ReplaceAsciiJournalValue(
                        path,
                        "Acceleration=0",
                        "Acceleration=1");
                    var legacyBytes = File.ReadAllBytes(path);

                    using (var firstOpen =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        AssertEx.False(firstOpen.HasActiveRecord);
                        AssertEx.False(firstOpen.RecoveredAtStartup);
                        AssertEx.Equal(
                            MaintenanceActionKind.Ds402Home,
                            firstOpen.CurrentRecord.Action);
                        AssertEx.Equal(
                            MaintenanceActionRecoveryState.Resolved,
                            firstOpen.CurrentRecord.State);
                    }
                    AssertEx.SequenceEqual(
                        legacyBytes,
                        File.ReadAllBytes(path),
                        "Opening a resolved legacy record rewrote its bytes.");

                    using (var secondOpen =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        AssertEx.False(secondOpen.HasActiveRecord);
                        AssertEx.False(secondOpen.RecoveredAtStartup);
                        AssertEx.Equal(
                            MaintenanceActionRecoveryState.Resolved,
                            secondOpen.CurrentRecord.State);
                    }
                    AssertEx.SequenceEqual(
                        legacyBytes,
                        File.ReadAllBytes(path),
                        "Reopening a resolved legacy record rewrote its bytes.");

                    using (var replacement =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        var armed = Arm(
                            replacement,
                            MaintenanceActionKind.LmcHome,
                            0x55667788U,
                            "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=0;TargetPosition=0;TimeoutMs=1000");
                        replacement.ResolveConfirmedRejection(
                            armed,
                            armed.TransportCorrelationId,
                            FixedUtc().AddSeconds(2));
                    }
                    AssertEx.Equal(
                        5,
                        BitConverter.ToInt32(File.ReadAllBytes(path), 8));

                    using (var currentOpen =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        AssertEx.False(currentOpen.HasActiveRecord);
                        AssertEx.Equal(
                            MaintenanceActionKind.LmcHome,
                            currentOpen.CurrentRecord.Action);
                    }
                });
            }
        }

        private static void TransportCorrelationMustBeNonZero()
        {
            WithTemporaryDirectory(delegate(string directory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    var error = AssertEx.Throws<
                        ArgumentOutOfRangeException>(
                        () => Arm(
                            journal,
                            MaintenanceActionKind.LmcHome,
                            0,
                            "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=0;TargetPosition=0;TimeoutMs=1000"));
                    AssertEx.Equal(
                        "transportCorrelationId",
                        error.ParamName);
                    AssertEx.False(journal.HasActiveRecord);
                }
            });

            foreach (var resolvedBeforeMutation in
                new[] { false, true })
            {
                WithTemporaryDirectory(delegate(string directory)
                {
                    string path;
                    using (var journal =
                        MaintenanceActionRecoveryJournal.Open(directory))
                    {
                        var armed = Arm(
                            journal,
                            MaintenanceActionKind.LmcHome,
                            0x10203040U,
                            "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=0;TargetPosition=0;TimeoutMs=1000");
                        if (resolvedBeforeMutation)
                        {
                            journal.ResolveConfirmedRejection(
                                armed,
                                armed.TransportCorrelationId,
                                FixedUtc().AddSeconds(1));
                        }

                        path = Path.Combine(
                            directory,
                            MaintenanceActionRecoveryJournal.JournalFileName);
                    }

                    RewriteTransportCorrelationId(path, 0);
                    var error = AssertEx.Throws<InvalidDataException>(
                        () => MaintenanceActionRecoveryJournal.Open(directory));
                    AssertEx.Contains(
                        "non-zero transport correlation",
                        error.Message);
                });
            }
        }

        private static void
            ConfirmedRejectionResolvesWithoutRecoveryQuarantine()
        {
            AssertConfirmedRejectionResolves(
                MaintenanceActionKind.LmcHome,
                0x10203040U,
                0x10203040U,
                "Schema=1;Semantic=CurrentPositionZero;ExpectedActualPosition=10;TargetPosition=0;TimeoutMs=1000");
            AssertConfirmedRejectionResolves(
                MaintenanceActionKind.Ds402Home,
                0x10203040U,
                0x10203040U,
                "Schema=1;Method=37;HomeOffset=0;Velocity=0;Acceleration=0;DistanceLimit=0;TorqueLimit=0;BufferMode=Aborting;TimeoutMs=1000");
            AssertConfirmedRejectionResolves(
                MaintenanceActionKind.EncoderTw20ErrorWarningReset,
                0x10203040U,
                0x10203040U,
                EncoderParameters(true, 4, 1));
            AssertConfirmedRejectionResolves(
                MaintenanceActionKind.EncoderTw19MultiturnPositionReset,
                0x10203040U,
                0x10203040U,
                EncoderParameters(false, 4, 2));
        }

        private static void AssertConfirmedRejectionResolves(
            MaintenanceActionKind action,
            uint correlationId,
            uint rejectionCorrelationId,
            string parameters)
        {
            WithTemporaryDirectory(delegate(string directory)
            {
                using (var journal =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    var armed = Arm(
                        journal,
                        action,
                        correlationId,
                        parameters);
                    if (correlationId != 0)
                    {
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.ResolveConfirmedRejection(
                                armed,
                                rejectionCorrelationId == uint.MaxValue
                                    ? rejectionCorrelationId - 1
                                    : rejectionCorrelationId + 1,
                                FixedUtc().AddSeconds(1)));
                        AssertEx.True(journal.HasActiveRecord);
                    }

                    var resolved = journal.ResolveConfirmedRejection(
                        armed,
                        rejectionCorrelationId,
                        FixedUtc().AddSeconds(1));
                    AssertEx.Equal(
                        MaintenanceActionRecoveryState.Resolved,
                        resolved.State);
                    AssertEx.False(journal.HasActiveRecord);
                }

                using (var reopened =
                    MaintenanceActionRecoveryJournal.Open(directory))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.False(reopened.RecoveredAtStartup);
                }
            });
        }

        private static MaintenanceActionRecoveryRecord Arm(
            MaintenanceActionRecoveryJournal journal,
            MaintenanceActionKind action,
            uint correlationId,
            string parameters)
        {
            var axisReference = action
                == MaintenanceActionKind.EncoderTw20ErrorWarningReset
                    || action
                        == MaintenanceActionKind
                            .EncoderTw19MultiturnPositionReset
                ? (ushort)4
                : (ushort)1;
            return journal.ArmBeforeDispatch(
                action,
                "127.0.0.1",
                4000,
                0x01020304U,
                0x11223344U,
                0x55667788U,
                "_LMCAxis" + axisReference,
                axisReference,
                0x89ABCDEFU,
                0x01234567U,
                0x76543210U,
                0xFEDCBA98U,
                correlationId,
                parameters,
                FixedUtc());
        }

        private static string EncoderParameters(
            bool tw20,
            ushort drive,
            uint socket)
        {
            return "Schema=1;Semantic="
                + (tw20
                    ? "Tw20ErrorWarningReset"
                    : "Tw19MultiturnPositionReset")
                + ";Kind="
                + (tw20 ? "1" : "2")
                + ";Profile=1;Drive="
                + drive
                + ";Socket="
                + socket
                + ";CommandValue="
                + "1"
                + ";Object=0x20FC;Sub="
                + (tw20 ? "0x02" : "0x01")
                + ";Type=UInt16;TimeoutMilliseconds=1000;Evidence0=1;Evidence1=2;Evidence2=3;Evidence3=4";
        }

        private static void RewriteFormatVersion(string path, int version)
        {
            var bytes = File.ReadAllBytes(path);
            var versionBytes = BitConverter.GetBytes(version);
            Buffer.BlockCopy(versionBytes, 0, bytes, 8, versionBytes.Length);
            RewriteChecksumAndSave(path, bytes);
        }

        private static void RewriteTransportCorrelationId(
            string path,
            uint transportCorrelationId)
        {
            var bytes = File.ReadAllBytes(path);
            int correlationOffset;
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = new BinaryReader(stream))
            {
                stream.Position = 16;
                reader.ReadBytes(16);
                reader.ReadInt32();
                SkipSerializedText(reader);
                reader.ReadInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                SkipSerializedText(reader);
                reader.ReadUInt16();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                correlationOffset = checked((int)stream.Position);
            }

            Buffer.BlockCopy(
                BitConverter.GetBytes(transportCorrelationId),
                0,
                bytes,
                correlationOffset,
                sizeof(uint));
            RewriteChecksumAndSave(path, bytes);
        }

        private static void ReplaceAsciiJournalValue(
            string path,
            string oldValue,
            string newValue)
        {
            var oldBytes = System.Text.Encoding.ASCII.GetBytes(oldValue);
            var newBytes = System.Text.Encoding.ASCII.GetBytes(newValue);
            if (oldBytes.Length != newBytes.Length)
            {
                throw new InvalidOperationException(
                    "Journal test replacement values must have equal lengths.");
            }

            var bytes = File.ReadAllBytes(path);
            var matchOffset = FindUniqueSequence(bytes, oldBytes);
            Buffer.BlockCopy(
                newBytes,
                0,
                bytes,
                matchOffset,
                newBytes.Length);
            RewriteChecksumAndSave(path, bytes);
        }

        private static int FindUniqueSequence(
            byte[] bytes,
            byte[] expected)
        {
            var matchOffset = -1;
            for (var index = 0;
                index <= bytes.Length - expected.Length;
                index++)
            {
                var matched = true;
                for (var offset = 0; offset < expected.Length; offset++)
                {
                    if (bytes[index + offset] != expected[offset])
                    {
                        matched = false;
                        break;
                    }
                }

                if (!matched)
                {
                    continue;
                }

                if (matchOffset >= 0)
                {
                    throw new InvalidOperationException(
                        "Journal test replacement value is not unique.");
                }

                matchOffset = index;
            }

            if (matchOffset < 0)
            {
                throw new InvalidOperationException(
                    "Journal test replacement value was not found.");
            }

            return matchOffset;
        }

        private static void SkipSerializedText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length <= 0 || reader.ReadBytes(length).Length != length)
            {
                throw new InvalidDataException(
                    "Journal test text field is incomplete.");
            }
        }

        private static void RewriteChecksumAndSave(
            string path,
            byte[] bytes)
        {
            var checksumOffset = bytes.Length - 32;
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
                checksum.Length);
            File.WriteAllBytes(path, bytes);
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(638900000000000000L, DateTimeKind.Utc);
        }

        private static void WithTemporaryDirectory(Action<string> body)
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "ElmoMaintenanceJournalTests",
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

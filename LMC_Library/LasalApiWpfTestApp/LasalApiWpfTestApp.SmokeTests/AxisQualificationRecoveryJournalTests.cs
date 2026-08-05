using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AxisQualificationRecoveryJournalTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AxisQualificationJournal.CreateOpenSingleWriter",
                CreateOpenPreservesExactIdentityAndSingleWriter);
            tests.Add(
                "Wpf.AxisQualificationJournal.ChecksumCorruption",
                ChecksumCorruptionFailsClosed);
            tests.Add(
                "Wpf.AxisQualificationJournal.MonotonicExactCas",
                MonotonicStagesAndExactCasRejectMismatch);
            tests.Add(
                "Wpf.AxisQualificationJournal.CrashPromotion",
                RestartPromotesVolatileStagesConservatively);
            tests.Add(
                "Wpf.AxisQualificationJournal.DeferredCrashPromotion",
                DeferredCrashPromotionRequiresExplicitCommit);
            tests.Add(
                "Wpf.AxisQualificationJournal.SafeTombstone",
                SafeResolvePersistsTombstone);
            tests.Add(
                "Wpf.AxisQualificationJournal.RetirementOriginalByteCas",
                RetirementRequiresExactSemanticAndOriginalByteEvidence);
        }

        private static void
            CreateOpenPreservesExactIdentityAndSingleWriter()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                Guid identity;
                byte[] checksum;
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, created);
                    var accepted = journal.MarkPowerOnAccepted(
                        armed,
                        created.AddMilliseconds(1));
                    identity = accepted.Identity;
                    checksum = accepted.Checksum;

                    AssertEx.True(File.Exists(journal.JournalFilePath));
                    AssertEx.Equal("127.0.0.1", accepted.EndpointIp);
                    AssertEx.Equal(4000, accepted.EndpointPort);
                    AssertEx.Equal(9L, accepted.OwnerSessionGeneration);
                    AssertEx.Equal("AxisOne", accepted.AxisName);
                    AssertEx.Equal((ushort)1, accepted.AxisReference);
                    AssertEx.Equal(0x01020304U, accepted.DiagnosticsBuild);
                    AssertEx.Equal(0x11223344U, accepted.DiagnosticsBootId);
                    AssertEx.Equal(0x55667788U, accepted.MapRevision);
                    AssertEx.True(accepted.MatchesRecoveryIdentity(
                        "127.1",
                        4000,
                        9,
                        "AxisOne",
                        1,
                        0x01020304U,
                        0x11223344U,
                        0x55667788U));
                    AssertEx.True(accepted.MatchesInput(
                        100,
                        200,
                        300,
                        400,
                        0,
                        5));
                    AssertEx.False(accepted.HasTarget);
                    AssertEx.Throws<IOException>(
                        () => AxisQualificationRecoveryJournal.Open(
                            directory));
                }

                using (var reopened =
                    AxisQualificationRecoveryJournal.Open(directory))
                {
                    AssertEx.True(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, reopened.Current.Identity);
                    AssertEx.Equal(2L, reopened.Current.RecordRevision);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnAccepted,
                        reopened.Current.Stage);
                    AssertEx.False(reopened.Current.WasCrashPromoted);
                    AssertEx.SequenceEqual(
                        checksum,
                        reopened.Current.Checksum);
                }
            });
        }

        private static void ChecksumCorruptionFailsClosed()
        {
            WithTestDirectory(delegate(string directory)
            {
                string path;
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, FixedUtc());
                    journal.MarkPowerOnAccepted(
                        armed,
                        FixedUtc().AddMilliseconds(1));
                    path = journal.JournalFilePath;
                }

                var bytes = File.ReadAllBytes(path);
                bytes[20] ^= 0x5a;
                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(
                    () => AxisQualificationRecoveryJournal.Open(
                        directory));
            });
        }

        private static void MonotonicStagesAndExactCasRejectMismatch()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, created);
                    var powerOnAccepted = journal.MarkPowerOnAccepted(
                        armed,
                        created.AddMilliseconds(1));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.MarkPowerOnStable(
                            armed,
                            created.AddMilliseconds(2)));

                    var powerOnStable = journal.MarkPowerOnStable(
                        powerOnAccepted,
                        created.AddMilliseconds(2));
                    AssertEx.Throws<ArgumentException>(
                        () => journal.PrepareMove(
                            powerOnStable,
                            1000,
                            1101,
                            created.AddMilliseconds(3)));
                    var prepared = journal.PrepareMove(
                        powerOnStable,
                        1000,
                        1100,
                        created.AddMilliseconds(3));
                    AssertEx.True(prepared.MatchesTarget(1000, 1100));

                    var moveAccepted = journal.MarkMoveAccepted(
                        prepared,
                        created.AddMilliseconds(4));
                    var moveStable = journal.MarkMoveStable(
                        moveAccepted,
                        created.AddMilliseconds(5));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.MarkMoveStable(
                            moveAccepted,
                            created.AddMilliseconds(6)));
                    var stopAccepted = journal.MarkStopAccepted(
                        moveStable,
                        11,
                        created.AddMilliseconds(6));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.MarkPowerOffAccepted(
                            stopAccepted,
                            10,
                            created.AddMilliseconds(7)));
                    var stopStable = journal.MarkStopStable(
                        stopAccepted,
                        created.AddMilliseconds(7));
                    var powerOffAccepted =
                        journal.MarkPowerOffAccepted(
                            stopStable,
                            12,
                            created.AddMilliseconds(8));
                    var resolved = journal.ResolveSafe(
                        powerOffAccepted,
                        created.AddMilliseconds(9));

                    AssertEx.Equal(10L, resolved.RecordRevision);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.SafeResolved,
                        resolved.Stage);
                    AssertEx.Equal(12L, resolved.SafetyGeneration);
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveSafe(
                            powerOffAccepted,
                            created.AddMilliseconds(10)));
                }
            });
        }

        private static void RestartPromotesVolatileStagesConservatively()
        {
            WithTestDirectory(delegate(string directory)
            {
                var armedDirectory = Path.Combine(directory, "armed");
                var created = FixedUtc();
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(armedDirectory))
                {
                    Arm(journal, created);
                }
                using (var reopened =
                    AxisQualificationRecoveryJournal.Open(armedDirectory))
                {
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnAccepted,
                        reopened.Current.Stage);
                    AssertEx.Equal(2L, reopened.Current.RecordRevision);
                    AssertEx.True(reopened.Current.WasCrashPromoted);
                }

                var preparedDirectory = Path.Combine(
                    directory,
                    "prepared");
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(
                        preparedDirectory))
                {
                    var accepted = journal.MarkPowerOnAccepted(
                        Arm(journal, created),
                        created.AddMilliseconds(1));
                    var stable = journal.MarkPowerOnStable(
                        accepted,
                        created.AddMilliseconds(2));
                    journal.PrepareMove(
                        stable,
                        1000,
                        1100,
                        created.AddMilliseconds(3));
                }
                using (var reopened =
                    AxisQualificationRecoveryJournal.Open(
                        preparedDirectory))
                {
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.MoveAccepted,
                        reopened.Current.Stage);
                    AssertEx.Equal(5L, reopened.Current.RecordRevision);
                    AssertEx.True(reopened.Current.WasCrashPromoted);
                    AssertEx.True(reopened.Current.MatchesTarget(
                        1000,
                        1100));
                }
            });
        }

        private static void
            DeferredCrashPromotionRequiresExplicitCommit()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                var armedDirectory = Path.Combine(
                    directory,
                    "deferred-armed");
                string armedPath;
                byte[] armedOriginalBytes;
                byte[] armedPromotedBytes;
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(
                        armedDirectory))
                {
                    var armed = Arm(journal, created);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage
                            .ArmedBeforePowerOn,
                        armed.Stage);
                    AssertEx.False(armed.WasCrashPromoted);
                    armedPath = journal.JournalFilePath;
                    armedOriginalBytes = File.ReadAllBytes(armedPath);
                }

                using (var recovered =
                    AxisQualificationRecoveryJournal.Open(
                        armedDirectory,
                        true))
                {
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage
                            .ArmedBeforePowerOn,
                        recovered.Current.Stage);
                    AssertEx.Equal(1L, recovered.Current.RecordRevision);
                    AssertEx.False(
                        recovered.Current.WasCrashPromoted);
                    AssertEx.SequenceEqual(
                        armedOriginalBytes,
                        File.ReadAllBytes(armedPath));

                    recovered.PromoteRecoveredVolatileStage();

                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnAccepted,
                        recovered.Current.Stage);
                    AssertEx.Equal(2L, recovered.Current.RecordRevision);
                    AssertEx.True(
                        recovered.Current.WasCrashPromoted);
                    armedPromotedBytes = File.ReadAllBytes(armedPath);
                    AssertByteSequencesDiffer(
                        armedOriginalBytes,
                        armedPromotedBytes);
                }

                using (var durable =
                    AxisQualificationRecoveryJournal.Open(
                        armedDirectory,
                        true))
                {
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnAccepted,
                        durable.Current.Stage);
                    AssertEx.Equal(2L, durable.Current.RecordRevision);
                    AssertEx.True(durable.Current.WasCrashPromoted);
                    AssertEx.SequenceEqual(
                        armedPromotedBytes,
                        File.ReadAllBytes(armedPath));
                }

                var preparedDirectory = Path.Combine(
                    directory,
                    "deferred-prepared");
                string preparedPath;
                byte[] preparedOriginalBytes;
                byte[] preparedPromotedBytes;
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(
                        preparedDirectory))
                {
                    var accepted = journal.MarkPowerOnAccepted(
                        Arm(journal, created),
                        created.AddMilliseconds(1));
                    var stable = journal.MarkPowerOnStable(
                        accepted,
                        created.AddMilliseconds(2));
                    var prepared = journal.PrepareMove(
                        stable,
                        1000,
                        1100,
                        created.AddMilliseconds(3));
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.MovePrepared,
                        prepared.Stage);
                    AssertEx.False(prepared.WasCrashPromoted);
                    preparedPath = journal.JournalFilePath;
                    preparedOriginalBytes = File.ReadAllBytes(
                        preparedPath);
                }

                using (var recovered =
                    AxisQualificationRecoveryJournal.Open(
                        preparedDirectory,
                        true))
                {
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.MovePrepared,
                        recovered.Current.Stage);
                    AssertEx.Equal(4L, recovered.Current.RecordRevision);
                    AssertEx.False(
                        recovered.Current.WasCrashPromoted);
                    AssertEx.True(
                        recovered.Current.MatchesTarget(1000, 1100));
                    AssertEx.SequenceEqual(
                        preparedOriginalBytes,
                        File.ReadAllBytes(preparedPath));

                    recovered.PromoteRecoveredVolatileStage();

                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.MoveAccepted,
                        recovered.Current.Stage);
                    AssertEx.Equal(5L, recovered.Current.RecordRevision);
                    AssertEx.True(
                        recovered.Current.WasCrashPromoted);
                    AssertEx.True(
                        recovered.Current.MatchesTarget(1000, 1100));
                    preparedPromotedBytes = File.ReadAllBytes(
                        preparedPath);
                    AssertByteSequencesDiffer(
                        preparedOriginalBytes,
                        preparedPromotedBytes);
                }

                using (var durable =
                    AxisQualificationRecoveryJournal.Open(
                        preparedDirectory,
                        true))
                {
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.MoveAccepted,
                        durable.Current.Stage);
                    AssertEx.Equal(5L, durable.Current.RecordRevision);
                    AssertEx.True(durable.Current.WasCrashPromoted);
                    AssertEx.True(
                        durable.Current.MatchesTarget(1000, 1100));
                    AssertEx.SequenceEqual(
                        preparedPromotedBytes,
                        File.ReadAllBytes(preparedPath));
                }

                var defaultDirectory = Path.Combine(
                    directory,
                    "default-auto-promotion");
                string defaultPath;
                byte[] defaultOriginalBytes;
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(
                        defaultDirectory))
                {
                    Arm(journal, created);
                    defaultPath = journal.JournalFilePath;
                    defaultOriginalBytes = File.ReadAllBytes(
                        defaultPath);
                }
                using (var recovered =
                    AxisQualificationRecoveryJournal.Open(
                        defaultDirectory))
                {
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnAccepted,
                        recovered.Current.Stage);
                    AssertEx.Equal(2L, recovered.Current.RecordRevision);
                    AssertEx.True(
                        recovered.Current.WasCrashPromoted);
                    AssertByteSequencesDiffer(
                        defaultOriginalBytes,
                        File.ReadAllBytes(defaultPath));
                }
            });
        }

        private static void SafeResolvePersistsTombstone()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                var identity = Guid.NewGuid();
                byte[] checksum;
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(directory))
                {
                    var armed = Arm(journal, identity, created);
                    var resolved = journal.ResolveSafe(
                        armed,
                        created.AddMilliseconds(1));
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.SafeResolved,
                        resolved.Stage);
                    AssertEx.Equal(2L, resolved.RecordRevision);
                    AssertEx.False(journal.HasActiveRecord);
                    checksum = resolved.Checksum;
                }

                using (var reopened =
                    AxisQualificationRecoveryJournal.Open(directory))
                {
                    AssertEx.False(reopened.HasActiveRecord);
                    AssertEx.Equal(identity, reopened.Current.Identity);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.SafeResolved,
                        reopened.Current.Stage);
                    AssertEx.SequenceEqual(
                        checksum,
                        reopened.Current.Checksum);
                    AssertEx.Throws<InvalidOperationException>(
                        () => Arm(
                            reopened,
                            identity,
                            created.AddSeconds(1)));
                    var next = Arm(
                        reopened,
                        Guid.NewGuid(),
                        created.AddSeconds(1));
                    AssertEx.Equal(1L, next.RecordRevision);
                }
            });
        }

        private static void
            RetirementRequiresExactSemanticAndOriginalByteEvidence()
        {
            WithTestDirectory(delegate(string directory)
            {
                var created = FixedUtc();
                var successJournal = Path.Combine(directory, "success");
                var successLedger = Path.Combine(
                    directory,
                    "success-ledger");
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(successJournal))
                using (var ledger =
                    RecoveryRecordRetirementLedger.Open(successLedger))
                {
                    var accepted = journal.MarkPowerOnAccepted(
                        Arm(journal, created),
                        created.AddMilliseconds(1));
                    var evidence =
                        journal.CaptureActiveRetirementEvidence();
                    AssertEx.Equal(
                        RecoveryRecordOwner.AxisQualification,
                        evidence.Owner);
                    AssertEx.Contains(
                        "Revision=2",
                        evidence.SemanticFingerprint);
                    AssertEx.SequenceEqual(
                        File.ReadAllBytes(journal.JournalFilePath),
                        evidence.GetOriginalBytes());
                    var committed = ledger.CommitOperatorRetirement(
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        evidence.DiagnosticsBuild + 1,
                        evidence.DiagnosticsBootId,
                        evidence.MapRevision,
                        "TEST\\operator",
                        "Axis qualification retirement CAS test.",
                        created.AddSeconds(1));
                    var resolved = journal.ResolveOperatorRetirement(
                        evidence,
                        committed,
                        created.AddSeconds(2));
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.SafeResolved,
                        resolved.Stage);
                    AssertEx.Equal(
                        accepted.RecordRevision + 1,
                        resolved.RecordRevision);
                }

                var staleJournal = Path.Combine(directory, "stale");
                var staleLedger = Path.Combine(
                    directory,
                    "stale-ledger");
                using (var journal =
                    AxisQualificationRecoveryJournal.Open(staleJournal))
                using (var ledger =
                    RecoveryRecordRetirementLedger.Open(staleLedger))
                {
                    var accepted = journal.MarkPowerOnAccepted(
                        Arm(journal, created),
                        created.AddMilliseconds(1));
                    var evidence =
                        journal.CaptureActiveRetirementEvidence();
                    var committed = ledger.CommitOperatorRetirement(
                        evidence,
                        evidence.EndpointIp,
                        evidence.EndpointPort,
                        evidence.DiagnosticsBuild + 1,
                        evidence.DiagnosticsBootId,
                        evidence.MapRevision,
                        "TEST\\operator",
                        "Reject changed Axis qualification bytes.",
                        created.AddSeconds(1));
                    journal.MarkPowerOnStable(
                        accepted,
                        created.AddMilliseconds(2));

                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ResolveOperatorRetirement(
                            evidence,
                            committed,
                            created.AddSeconds(2)));
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(
                        AxisQualificationRecoveryStage.PowerOnStable,
                        journal.Current.Stage);
                }
            });
        }

        private static AxisQualificationRecoveryRecord Arm(
            AxisQualificationRecoveryJournal journal,
            DateTime createdUtc)
        {
            return Arm(journal, Guid.NewGuid(), createdUtc);
        }

        private static AxisQualificationRecoveryRecord Arm(
            AxisQualificationRecoveryJournal journal,
            Guid identity,
            DateTime createdUtc)
        {
            return journal.ArmBeforePowerOn(
                identity,
                "127.1",
                4000,
                9,
                "AxisOne",
                1,
                0x01020304U,
                0x11223344U,
                0x55667788U,
                100,
                200,
                300,
                400,
                0,
                5,
                10,
                createdUtc);
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(
                2026,
                7,
                31,
                1,
                2,
                3,
                DateTimeKind.Utc);
        }

        private static void AssertByteSequencesDiffer(
            byte[] expectedDifferent,
            byte[] actual)
        {
            if (expectedDifferent == null || actual == null)
            {
                throw new InvalidOperationException(
                    "Byte sequences must be non-null.");
            }
            if (expectedDifferent.Length != actual.Length)
            {
                return;
            }
            for (var index = 0;
                index < expectedDifferent.Length;
                index++)
            {
                if (expectedDifferent[index] != actual[index])
                {
                    return;
                }
            }
            throw new InvalidOperationException(
                "Expected durable journal bytes to change.");
        }

        private static void WithTestDirectory(Action<string> action)
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "aqj",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                action(directory);
            }
            finally
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                    // A failed assertion remains the primary test result.
                }
            }
        }
    }
}

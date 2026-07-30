using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AxisCommandRecoveryJournalTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AxisCommandJournal.DefaultPath",
                DefaultPathIsVersioned);
            tests.Add(
                "Wpf.AxisCommandJournal.StopRoundtrip",
                StopRoundtripPreservesParameters);
            tests.Add(
                "Wpf.AxisCommandJournal.ResetRoundtrip",
                ResetRoundtripAndTransitions);
            tests.Add(
                "Wpf.AxisCommandJournal.ResetToStopAtomicReplacement",
                ResetToStopReplacementIsAtomic);
            tests.Add(
                "Wpf.AxisCommandJournal.FailedReplacementPreservesReset",
                FailedReplacementPreservesReset);
            tests.Add(
                "Wpf.AxisCommandJournal.NotAttemptedStopRestoresExactReset",
                NotAttemptedStopRestoresExactReset);
            tests.Add(
                "Wpf.AxisCommandJournal.RollbackRejectsWrongPredecessor",
                RollbackRejectsWrongPredecessor);
            tests.Add(
                "Wpf.AxisCommandJournal.SingleWriterAndCorruption",
                SingleWriterAndCorruptionFailClosed);
            tests.Add(
                "Wpf.AxisCommandJournal.IdentityAndBounds",
                IdentityAndBoundsAreFailClosed);
        }

        private static void DefaultPathIsVersioned()
        {
            var expected = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisCommandRecoveryJournal",
                "v1");
            AssertEx.Equal(
                expected.ToUpperInvariant(),
                AxisCommandRecoveryJournal.GetDefaultDirectoryPath()
                    .ToUpperInvariant());
        }

        private static void StopRoundtripPreservesParameters()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                Guid identity;
                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var armed = ArmStop(journal, created);
                    identity = armed.Identity;
                    var accepted = journal.MarkAccepted(
                        identity,
                        created.AddTicks(1));
                    AssertEx.Equal(
                        AxisCommandRecoveryState.AcceptedAwaitingProof,
                        accepted.State);
                }

                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var record = journal.CurrentRecord;
                    AssertEx.True(journal.HasActiveRecord);
                    AssertEx.Equal(identity, record.Identity);
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Stop,
                        record.Operation);
                    AssertEx.Equal(1000, record.StopDeceleration);
                    AssertEx.Equal(200, record.StopJerk);
                    AssertEx.Equal(3, record.RequiredStableSampleCount);
                    AssertIdentity(record, 4000);
                    journal.Resolve(identity, created.AddTicks(2));
                }

                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    AssertEx.False(journal.HasActiveRecord);
                    AssertEx.Equal(
                        AxisCommandRecoveryState.Resolved,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void ResetRoundtripAndTransitions()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                Guid identity;
                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var reset = ArmReset(journal, created);
                    identity = reset.Identity;
                    var recovery = journal.PromoteToRecoveryRequired(
                        identity,
                        created.AddTicks(1));
                    AssertEx.Equal(
                        AxisCommandRecoveryState.RecoveryRequired,
                        recovery.State);
                    var accepted = journal.MarkAccepted(
                        identity,
                        created.AddTicks(2));
                    AssertEx.Equal(
                        AxisCommandRecoveryState.AcceptedAwaitingProof,
                        accepted.State);
                }

                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var record = journal.CurrentRecord;
                    AssertEx.Equal(identity, record.Identity);
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Reset,
                        record.Operation);
                    AssertEx.Equal(0, record.StopDeceleration);
                    AssertEx.Equal(0, record.StopJerk);
                    journal.Resolve(identity, created.AddTicks(3));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void ResetToStopReplacementIsAtomic()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                Guid resetIdentity;
                Guid stopIdentity;
                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var reset = ArmReset(journal, created);
                    resetIdentity = reset.Identity;
                    reset = journal.MarkAccepted(
                        reset.Identity,
                        created.AddTicks(1));
                    var stop = journal.ReplaceActiveResetWithStopBeforeDispatch(
                        reset.Identity,
                        "127.0.0.1",
                        4000,
                        "_LMCAxis1",
                        1,
                        0x10203040,
                        0x50607080,
                        900,
                        100,
                        3,
                        created.AddTicks(2));
                    AssertEx.Equal(
                        reset.Identity,
                        stop.SupersededResetIdentity);
                    stopIdentity = stop.Identity;
                    AssertEx.True(stopIdentity != resetIdentity);
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Stop,
                        stop.Operation);
                    AssertEx.Equal(
                        AxisCommandRecoveryState.ArmedBeforeDispatch,
                        stop.State);
                }

                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    AssertEx.Equal(stopIdentity, journal.CurrentRecord.Identity);
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Stop,
                        journal.CurrentRecord.Operation);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.Resolve(
                            resetIdentity,
                            created.AddTicks(3)));
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void RollbackRejectsWrongPredecessor()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var reset = ArmReset(journal, created);
                    var stop = journal.ReplaceActiveResetWithStopBeforeDispatch(
                        reset.Identity,
                        reset.EndpointIp,
                        reset.EndpointPort,
                        reset.AxisName,
                        reset.AxisReference,
                        reset.DiagnosticsBootId,
                        reset.MapRevision,
                        900,
                        100,
                        3,
                        created.AddTicks(1));
                    var wrongReset = new AxisCommandRecoveryRecord(
                        Guid.NewGuid(),
                        AxisCommandRecoveryOperation.Reset,
                        reset.EndpointIp,
                        reset.EndpointPort,
                        reset.AxisName,
                        reset.AxisReference,
                        reset.DiagnosticsBootId,
                        reset.MapRevision,
                        0,
                        0,
                        3,
                        Guid.Empty,
                        AxisCommandRecoveryState.ArmedBeforeDispatch,
                        created,
                        created);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.RestoreResetAfterStopNotAttempted(
                            stop.Identity,
                            wrongReset,
                            created.AddTicks(2)));
                    AssertEx.Equal(stop.Identity, journal.CurrentRecord.Identity);

                    journal.Resolve(stop.Identity, created.AddTicks(3));
                    var freshStop = ArmStop(journal, created.AddTicks(4));
                    AssertEx.Equal(Guid.Empty, freshStop.SupersededResetIdentity);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.RestoreResetAfterStopNotAttempted(
                            freshStop.Identity,
                            reset,
                            created.AddTicks(5)));
                    AssertEx.Equal(
                        freshStop.Identity,
                        journal.CurrentRecord.Identity);

                    journal.Resolve(freshStop.Identity, created.AddTicks(6));
                    var newerReset = ArmReset(journal, created.AddTicks(7));
                    var newerStop = journal.ReplaceActiveResetWithStopBeforeDispatch(
                        newerReset.Identity,
                        newerReset.EndpointIp,
                        newerReset.EndpointPort,
                        newerReset.AxisName,
                        newerReset.AxisReference,
                        newerReset.DiagnosticsBootId,
                        newerReset.MapRevision,
                        901,
                        101,
                        3,
                        created.AddTicks(8));
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.RestoreResetAfterStopNotAttempted(
                            stop.Identity,
                            reset,
                            created.AddTicks(9)));
                    AssertEx.Equal(
                        newerStop.Identity,
                        journal.CurrentRecord.Identity);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void FailedReplacementPreservesReset()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var reset = ArmReset(journal, created);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.ReplaceActiveResetWithStopBeforeDispatch(
                            reset.Identity,
                            "127.0.0.1",
                            4000,
                            "OtherAxis",
                            1,
                            0x10203040,
                            0x50607080,
                            900,
                            100,
                            3,
                            created.AddTicks(1)));
                    AssertEx.Equal(reset.Identity, journal.CurrentRecord.Identity);
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Reset,
                        journal.CurrentRecord.Operation);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void NotAttemptedStopRestoresExactReset()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                var created = FixedUtc();
                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    var reset = ArmReset(journal, created);
                    reset = journal.MarkAccepted(
                        reset.Identity,
                        created.AddTicks(1));
                    var stop = journal.ReplaceActiveResetWithStopBeforeDispatch(
                        reset.Identity,
                        reset.EndpointIp,
                        reset.EndpointPort,
                        reset.AxisName,
                        reset.AxisReference,
                        reset.DiagnosticsBootId,
                        reset.MapRevision,
                        900,
                        100,
                        3,
                        created.AddTicks(2));
                    var restored = journal.RestoreResetAfterStopNotAttempted(
                        stop.Identity,
                        reset,
                        created.AddTicks(3));
                    AssertEx.Equal(reset.Identity, restored.Identity);
                    AssertEx.Equal(reset.State, restored.State);
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Reset,
                        restored.Operation);
                    AssertEx.Throws<InvalidOperationException>(
                        () => journal.RestoreResetAfterStopNotAttempted(
                            stop.Identity,
                            reset,
                            created.AddTicks(4)));
                }

                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    AssertEx.Equal(
                        AxisCommandRecoveryOperation.Reset,
                        journal.CurrentRecord.Operation);
                    AssertEx.Equal(
                        AxisCommandRecoveryState.AcceptedAwaitingProof,
                        journal.CurrentRecord.State);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void SingleWriterAndCorruptionFailClosed()
        {
            var directory = CreateTemporaryDirectory();
            try
            {
                using (var journal = AxisCommandRecoveryJournal.Open(directory))
                {
                    ArmStop(journal, FixedUtc());
                    AssertEx.Throws<IOException>(
                        () => AxisCommandRecoveryJournal.Open(directory));
                }

                var path = Path.Combine(
                    directory,
                    AxisCommandRecoveryJournal.JournalFileName);
                var bytes = File.ReadAllBytes(path);
                bytes[bytes.Length - 1] ^= 0xFF;
                File.WriteAllBytes(path, bytes);
                AssertEx.Throws<InvalidDataException>(
                    () => AxisCommandRecoveryJournal.Open(directory));
            }
            finally
            {
                DeleteTemporaryDirectory(directory);
            }
        }

        private static void IdentityAndBoundsAreFailClosed()
        {
            var created = FixedUtc();
            var record = new AxisCommandRecoveryRecord(
                Guid.NewGuid(),
                AxisCommandRecoveryOperation.Stop,
                "127.0.0.1",
                4000,
                "_LMCAxis1",
                1,
                0x10203040,
                0x50607080,
                1000,
                200,
                3,
                Guid.Empty,
                AxisCommandRecoveryState.ArmedBeforeDispatch,
                created,
                created);
            AssertEx.True(record.MatchesPhysicalIdentity(
                "127.0.0.1",
                4000,
                "_LMCAxis1",
                1,
                0x10203040,
                0x50607080));
            AssertEx.False(record.MatchesPhysicalIdentity(
                "127.0.0.1",
                4000,
                "_LMCAxis1",
                2,
                0x10203040,
                0x50607080));
            AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new AxisCommandRecoveryRecord(
                    Guid.NewGuid(),
                    AxisCommandRecoveryOperation.Stop,
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    1,
                    1,
                    1,
                    0,
                    0,
                    3,
                    Guid.Empty,
                    AxisCommandRecoveryState.ArmedBeforeDispatch,
                    created,
                    created));
            AssertEx.Throws<ArgumentException>(
                () => new AxisCommandRecoveryRecord(
                    Guid.NewGuid(),
                    AxisCommandRecoveryOperation.Reset,
                    "127.0.0.1",
                    4000,
                    "_LMCAxis1",
                    1,
                    1,
                    1,
                    1,
                    0,
                    3,
                    Guid.Empty,
                    AxisCommandRecoveryState.ArmedBeforeDispatch,
                    created,
                    created));
        }

        private static AxisCommandRecoveryRecord ArmStop(
            AxisCommandRecoveryJournal journal,
            DateTime created)
        {
            return journal.ArmBeforeDispatch(
                AxisCommandRecoveryOperation.Stop,
                "127.0.0.1",
                4000,
                "_LMCAxis1",
                1,
                0x10203040,
                0x50607080,
                1000,
                200,
                3,
                created);
        }

        private static AxisCommandRecoveryRecord ArmReset(
            AxisCommandRecoveryJournal journal,
            DateTime created)
        {
            return journal.ArmBeforeDispatch(
                AxisCommandRecoveryOperation.Reset,
                "127.0.0.1",
                4000,
                "_LMCAxis1",
                1,
                0x10203040,
                0x50607080,
                0,
                0,
                3,
                created);
        }

        private static void AssertIdentity(
            AxisCommandRecoveryRecord record,
            int endpointPort)
        {
            AssertEx.Equal("127.0.0.1", record.EndpointIp);
            AssertEx.Equal(endpointPort, record.EndpointPort);
            AssertEx.Equal("_LMCAxis1", record.AxisName);
            AssertEx.Equal((ushort)1, record.AxisReference);
            AssertEx.Equal(0x10203040u, record.DiagnosticsBootId);
            AssertEx.Equal(0x50607080u, record.MapRevision);
        }

        private static DateTime FixedUtc()
        {
            return new DateTime(638895000000000000L, DateTimeKind.Utc);
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "ElmoAxisCommandJournalTests",
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

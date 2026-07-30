using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class DiagnosticsMutationJournalTests
    {
        private const uint BootId = 0x12345678u;
        private const uint SdoMapRevision = 0x10203040u;
        private const uint OutputTopologyRevision = 0x50607080u;
        private const long SessionGeneration = 7;
        private const string CrashChildMode =
            "mutation-journal-crash-child";
        private const string CrashHandshakeFileName =
            "crash-child.armed";
        private const int CrashChildTimeoutMilliseconds = 10000;
        private const int CrashChildUsageExitCode = 64;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.MutationJournal.RoundTripAndReopen",
                RoundTripAndReopen);
            tests.Add(
                "Qualification.MutationJournal.TransitionRequiresExactIdentity",
                TransitionRequiresExactIdentity);
            tests.Add(
                "Qualification.MutationJournal.ActiveOverwriteIsBlocked",
                ActiveOverwriteIsBlocked);
            tests.Add(
                "Qualification.MutationJournal.ResolvedTombstoneAllowsNewArm",
                ResolvedTombstoneAllowsNewArm);
            tests.Add(
                "Qualification.MutationJournal.CorruptionFailsClosed",
                CorruptionFailsClosed);
            tests.Add(
                "Qualification.MutationJournal.SecondWriterFailsClosed",
                SecondWriterFailsClosed);
            tests.Add(
                "Qualification.MutationJournal.ProcessTerminationReopenPreservesInterlock",
                ProcessTerminationReopenPreservesInterlock);
            tests.Add(
                "Qualification.MutationJournal.AnonymousStdinEofReopenPreservesInterlock",
                AnonymousStdinEofReopenPreservesInterlock);
            tests.Add(
                "Qualification.MutationJournal.TypedSdoV2RoundTripIsImmutable",
                TypedSdoV2RoundTripIsImmutable);
            tests.Add(
                "Qualification.MutationJournal.NonCanonicalV2MetadataMarkerFailsClosed",
                NonCanonicalV2MetadataMarkerFailsClosed);
            tests.Add(
                "Qualification.MutationJournal.LegacyV1RecoveryIsZeroWire",
                LegacyV1RecoveryIsZeroWire);
            tests.Add(
                "Qualification.MutationJournal.OutcomeUnverifiedCanBecomeReadbackMismatch",
                OutcomeUnverifiedCanBecomeReadbackMismatch);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryUnapprovedIsZeroWire",
                RestartRecoveryUnapprovedIsZeroWire);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryExactMatchPersistsResolvedBeforeReturn",
                RestartRecoveryExactMatchPersistsResolvedBeforeReturn);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryMismatchPersistsEvidence",
                RestartRecoveryMismatchPersistsEvidence);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryIdentityMismatchDoesNotRead",
                RestartRecoveryIdentityMismatchDoesNotRead);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryCapabilityStateChangeDoesNotReadOrCommit",
                RestartRecoveryCapabilityStateChangeDoesNotReadOrCommit);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryReadStateChangeDoesNotCommit",
                RestartRecoveryReadStateChangeDoesNotCommit);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryPostReadIdentityDriftDoesNotCommit",
                RestartRecoveryPostReadIdentityDriftDoesNotCommit);
            tests.Add(
                "Qualification.MutationJournal.RestartRecoveryAtomicTransitionRejectsStaleReadResult",
                RestartRecoveryAtomicTransitionRejectsStaleReadResult);
        }

        internal static bool IsCrashChildInvocation(string[] args)
        {
            return args != null
                && args.Length != 0
                && string.Equals(
                    args[0],
                    CrashChildMode,
                    StringComparison.Ordinal);
        }

        internal static int RunCrashChild(string[] args)
        {
            try
            {
                if (args == null || args.Length != 4)
                {
                    Console.Error.WriteLine(
                        "ERROR crash child requires directory, identity, and created ticks.");
                    return CrashChildUsageExitCode;
                }

                var directoryPath = RequireTestDirectoryPath(args[1]);
                Guid identity;
                long createdTicks;
                if (!Guid.TryParse(args[2], out identity)
                    || identity == Guid.Empty
                    || !long.TryParse(
                        args[3],
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out createdTicks)
                    || createdTicks <= DateTime.MinValue.Ticks
                    || createdTicks >= DateTime.MaxValue.Ticks)
                {
                    Console.Error.WriteLine(
                        "ERROR crash child identity or timestamp is invalid.");
                    return CrashChildUsageExitCode;
                }

                var createdUtc = new DateTime(
                    createdTicks,
                    DateTimeKind.Utc);
                var handshakePath = Path.Combine(
                    directoryPath,
                    CrashHandshakeFileName);
                using (var journal =
                    DiagnosticsMutationJournal.Open(directoryPath))
                {
                    var armed = ArmOutput(
                        journal,
                        identity,
                        createdUtc);
                    if (!journal.HasActiveRecord
                        || armed.State
                            != DiagnosticsMutationState.ArmedBeforeDispatch
                        || armed.TicketId != 0)
                    {
                        throw new InvalidOperationException(
                            "Crash child did not retain the armed mutation interlock.");
                    }

                    WriteCrashHandshake(
                        handshakePath,
                        BuildCrashHandshake(identity, createdUtc));
                    while (Console.In.Read() != -1)
                    {
                        // The parent never writes. Its redirected stdin handle
                        // closes automatically if the test runner terminates,
                        // so the child cannot outlive an unrelated reused PID.
                    }
                }

                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("ERROR crash child failed.");
                Console.Error.WriteLine(error);
                return 1;
            }
        }

        private static void RoundTripAndReopen()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = new DateTime(
                        638891712000000000L,
                        DateTimeKind.Utc);
                    var acceptedUtc = createdUtc.AddSeconds(1);

                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(
                            Path.GetFullPath(directoryPath),
                            journal.DirectoryPath);

                        var armed = journal.Arm(
                            DiagnosticsMutationKind.SdoWrite,
                            identity,
                            createdUtc,
                            BootId,
                            SdoMapRevision,
                            SessionGeneration,
                            "Slave=1,Object=0x2F00,SubIndex=24",
                            "Int32=0");
                        AssertRecord(
                            armed,
                            identity,
                            DiagnosticsMutationKind.SdoWrite,
                            DiagnosticsMutationState.ArmedBeforeDispatch,
                            createdUtc,
                            createdUtc,
                            BootId,
                            SdoMapRevision,
                            SessionGeneration,
                            0,
                            "Slave=1,Object=0x2F00,SubIndex=24",
                            "Int32=0");

                        var accepted = journal.Transition(
                            identity,
                            DiagnosticsMutationState
                                .AcceptedPendingTerminal,
                            acceptedUtc,
                            41);
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal((uint)41, accepted.TicketId);
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                        AssertRecord(
                            reopened.CurrentRecord,
                            identity,
                            DiagnosticsMutationKind.SdoWrite,
                            DiagnosticsMutationState
                                .AcceptedPendingTerminal,
                            createdUtc,
                            acceptedUtc,
                            BootId,
                            SdoMapRevision,
                            SessionGeneration,
                            41,
                            "Slave=1,Object=0x2F00,SubIndex=24",
                            "Int32=0");
                    }
                });
        }

        private static void TypedSdoV2RoundTripIsImmutable()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    var sourceData = new byte[] { 0x2A, 0x00, 0x00, 0x00 };
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        var metadata = CreateSdoMetadata(sourceData);
                        var armed = journal.Arm(
                            DiagnosticsMutationKind.SdoWrite,
                            identity,
                            createdUtc,
                            BootId,
                            SdoMapRevision,
                            SessionGeneration,
                            "Slave=1,Object=0x2F00,SubIndex=24",
                            "Int32=42",
                            metadata);
                        sourceData[0] = 0x7F;
                        AssertEx.True(armed.HasTypedSdoWriteMetadata);
                        AssertEx.SequenceEqual(
                            new byte[] { 0x2A, 0x00, 0x00, 0x00 },
                            armed.SdoWriteMetadata.ExpectedWriteData);

                        journal.Transition(
                            identity,
                            DiagnosticsMutationState.AcceptedPendingTerminal,
                            createdUtc.AddMilliseconds(1),
                            77);
                        journal.Transition(
                            identity,
                            DiagnosticsMutationState
                                .TerminalSuccessPendingReadback,
                            createdUtc.AddMilliseconds(2),
                            77);

                        var encoded = File.ReadAllBytes(
                            journal.JournalFilePath);
                        AssertEx.Equal(
                            2,
                            BitConverter.ToInt32(encoded, 8),
                            "New durable records must use journal format v2.");
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        var record = reopened.CurrentRecord;
                        AssertEx.True(record.HasTypedSdoWriteMetadata);
                        var metadata = record.SdoWriteMetadata;
                        AssertEx.Equal((ushort)1, metadata.SlaveReference);
                        AssertEx.Equal((ushort)0x2F00, metadata.ObjectIndex);
                        AssertEx.Equal((byte)24, metadata.SubIndex);
                        AssertEx.Equal(
                            LMCSignalValueType.Int32,
                            metadata.ValueType);
                        AssertEx.Equal((ushort)4, metadata.DataLength);
                        AssertEx.Equal((uint)1000, metadata.TimeoutCycles);
                        AssertEx.SequenceEqual(
                            new byte[] { 0x2A, 0x00, 0x00, 0x00 },
                            metadata.ExpectedWriteData);

                        var returnedCopy = metadata.ExpectedWriteData;
                        returnedCopy[0] = 0x00;
                        AssertEx.SequenceEqual(
                            new byte[] { 0x2A, 0x00, 0x00, 0x00 },
                            metadata.ExpectedWriteData,
                            "Typed journal data must remain immutable after reopen.");
                    }
                });
        }

        private static void NonCanonicalV2MetadataMarkerFailsClosed()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    string journalPath;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            Guid.NewGuid(),
                            DateTime.UtcNow,
                            new byte[] { 1, 0, 0, 0 });
                        journalPath = journal.JournalFilePath;
                    }

                    var bytes = File.ReadAllBytes(journalPath);
                    var markerOffset = FindV2MetadataMarkerOffset(bytes);
                    AssertEx.Equal((byte)1, bytes[markerOffset]);
                    bytes[markerOffset] = 2;
                    RewriteJournalChecksum(bytes);
                    File.WriteAllBytes(journalPath, bytes);

                    AssertEx.Throws<InvalidDataException>(
                        () =>
                        {
                            using (DiagnosticsMutationJournal.Open(
                                directoryPath))
                            {
                            }
                        },
                        "A checksum-valid non-canonical metadata marker must fail closed.");
                });
        }

        private static void LegacyV1RecoveryIsZeroWire()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    var journalPath = Path.Combine(
                        directoryPath,
                        DiagnosticsMutationJournal.JournalFileName);
                    WriteLegacyV1SdoRecord(
                        journalPath,
                        identity,
                        createdUtc,
                        51);
                    var persistedBefore = File.ReadAllBytes(journalPath);
                    var allowlistCalls = 0;
                    var capabilityCalls = 0;
                    var readCalls = 0;

                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.False(
                            journal.CurrentRecord
                                .HasTypedSdoWriteMetadata);
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata =>
                                    {
                                        allowlistCalls++;
                                        return true;
                                    },
                                    () =>
                                    {
                                        capabilityCalls++;
                                        return Task.FromResult(
                                            CreateRecoveryCapabilities());
                                    },
                                    metadata =>
                                    {
                                        readCalls++;
                                        return Task.FromResult(
                                            new byte[] { 0, 0, 0, 0 });
                                    })
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .NotEligible,
                            result.Disposition);
                        AssertEx.Equal(0, allowlistCalls);
                        AssertEx.Equal(0, capabilityCalls);
                        AssertEx.Equal(0, readCalls);
                        AssertEx.Equal(
                            DiagnosticsMutationState
                                .TerminalSuccessPendingReadback,
                            journal.CurrentRecord.State);
                    }

                    AssertEx.SequenceEqual(
                        persistedBefore,
                        File.ReadAllBytes(journalPath),
                        "Legacy v1 recovery must not rewrite or transition the record.");
                });
        }

        private static void OutcomeUnverifiedCanBecomeReadbackMismatch()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            identity,
                            createdUtc,
                            new byte[] { 1, 0, 0, 0 });
                        journal.Transition(
                            identity,
                            DiagnosticsMutationState.OutcomeUnverified,
                            createdUtc.AddMilliseconds(3),
                            0);
                        var mismatch = journal.Transition(
                            identity,
                            DiagnosticsMutationState.ReadbackMismatch,
                            createdUtc.AddMilliseconds(4),
                            0);
                        AssertEx.Equal(
                            DiagnosticsMutationState.ReadbackMismatch,
                            mismatch.State);
                        AssertEx.Equal((uint)77, mismatch.TicketId);
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.Equal(
                            DiagnosticsMutationState.ReadbackMismatch,
                            reopened.CurrentRecord.State);
                        AssertEx.Equal(
                            (uint)77,
                            reopened.CurrentRecord.TicketId);
                    }
                });
        }

        private static void RestartRecoveryUnapprovedIsZeroWire()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var capabilityCalls = 0;
                    var readCalls = 0;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            Guid.NewGuid(),
                            DateTime.UtcNow,
                            new byte[] { 1, 0, 0, 0 });
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => false,
                                    () =>
                                    {
                                        capabilityCalls++;
                                        return Task.FromResult(
                                            CreateRecoveryCapabilities());
                                    },
                                    metadata =>
                                    {
                                        readCalls++;
                                        return Task.FromResult(
                                            new byte[] { 1, 0, 0, 0 });
                                    })
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .TargetNotApproved,
                            result.Disposition);
                        AssertEx.Equal(0, capabilityCalls);
                        AssertEx.Equal(0, readCalls);
                        AssertEx.Equal(
                            DiagnosticsMutationState
                                .TerminalSuccessPendingReadback,
                            journal.CurrentRecord.State);
                    }
                });
        }

        private static void
            RestartRecoveryExactMatchPersistsResolvedBeforeReturn()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var expected = new byte[] { 0x2A, 0, 0, 0 };
                    var capabilityCalls = 0;
                    var readCalls = 0;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            Guid.NewGuid(),
                            DateTime.UtcNow,
                            expected);
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => true,
                                    () =>
                                    {
                                        capabilityCalls++;
                                        return Task.FromResult(
                                            CreateRecoveryCapabilities());
                                    },
                                    metadata =>
                                    {
                                        readCalls++;
                                        return Task.FromResult(
                                            (byte[])expected.Clone());
                                    })
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition.Verified,
                            result.Disposition);
                        AssertEx.Equal(2, capabilityCalls);
                        AssertEx.Equal(1, readCalls);
                        AssertEx.False(
                            journal.HasActiveRecord,
                            "Resolved must already be durable before Verified returns.");
                        AssertEx.Equal(
                            DiagnosticsMutationState.Resolved,
                            journal.CurrentRecord.State);
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.False(reopened.HasActiveRecord);
                        AssertEx.Equal(
                            DiagnosticsMutationState.Resolved,
                            reopened.CurrentRecord.State);
                    }
                });
        }

        private static void RestartRecoveryMismatchPersistsEvidence()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var capabilityCalls = 0;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            Guid.NewGuid(),
                            DateTime.UtcNow,
                            new byte[] { 1, 0, 0, 0 });
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => true,
                                    () =>
                                    {
                                        capabilityCalls++;
                                        return Task.FromResult(
                                            CreateRecoveryCapabilities());
                                    },
                                    metadata => Task.FromResult(
                                        new byte[] { 2, 0, 0, 0 }))
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .ReadbackMismatch,
                            result.Disposition);
                        AssertEx.Equal(2, capabilityCalls);
                        AssertEx.True(journal.HasActiveRecord);
                        AssertEx.Equal(
                            DiagnosticsMutationState.ReadbackMismatch,
                            journal.CurrentRecord.State);
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                        AssertEx.Equal(
                            DiagnosticsMutationState.ReadbackMismatch,
                            reopened.CurrentRecord.State);
                    }
                });
        }

        private static void RestartRecoveryIdentityMismatchDoesNotRead()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var capabilityCalls = 0;
                    var readCalls = 0;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            Guid.NewGuid(),
                            DateTime.UtcNow,
                            new byte[] { 1, 0, 0, 0 });
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => true,
                                    () =>
                                    {
                                        capabilityCalls++;
                                        return Task.FromResult(
                                            CreateRecoveryCapabilities(
                                                BootId + 1,
                                                SdoMapRevision));
                                    },
                                    metadata =>
                                    {
                                        readCalls++;
                                        return Task.FromResult(
                                            new byte[] { 1, 0, 0, 0 });
                                    })
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .IdentityMismatch,
                            result.Disposition);
                        AssertEx.Equal(1, capabilityCalls);
                        AssertEx.Equal(0, readCalls);
                        AssertEx.Equal(
                            DiagnosticsMutationState
                                .TerminalSuccessPendingReadback,
                            journal.CurrentRecord.State);
                    }
                });
        }

        private static void
            RestartRecoveryCapabilityStateChangeDoesNotReadOrCommit()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var readCalls = 0;
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            identity,
                            createdUtc,
                            new byte[] { 1, 0, 0, 0 });
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => true,
                                    () =>
                                    {
                                        journal.Transition(
                                            identity,
                                            DiagnosticsMutationState
                                                .OutcomeUnverified,
                                            createdUtc.AddMilliseconds(3),
                                            0);
                                        return Task.FromResult(
                                            CreateRecoveryCapabilities());
                                    },
                                    metadata =>
                                    {
                                        readCalls++;
                                        return Task.FromResult(
                                            new byte[] { 1, 0, 0, 0 });
                                    })
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .StateChanged,
                            result.Disposition);
                        AssertEx.Equal(0, readCalls);
                        AssertEx.Equal(
                            DiagnosticsMutationState.OutcomeUnverified,
                            journal.CurrentRecord.State,
                            "The orchestrator must not add a recovery commit after capability-time state replacement.");
                    }
                });
        }

        private static void RestartRecoveryReadStateChangeDoesNotCommit()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            identity,
                            createdUtc,
                            new byte[] { 1, 0, 0, 0 });
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => true,
                                    () => Task.FromResult(
                                        CreateRecoveryCapabilities()),
                                    metadata =>
                                    {
                                        journal.Transition(
                                            identity,
                                            DiagnosticsMutationState
                                                .OutcomeUnverified,
                                            createdUtc.AddMilliseconds(3),
                                            0);
                                        return Task.FromResult(
                                            new byte[] { 2, 0, 0, 0 });
                                    })
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .StateChanged,
                            result.Disposition);
                        AssertEx.Equal(
                            DiagnosticsMutationState.OutcomeUnverified,
                            journal.CurrentRecord.State,
                            "The orchestrator must not persist mismatch or resolve after read-time state replacement.");
                    }
                });
        }

        private static void
            RestartRecoveryPostReadIdentityDriftDoesNotCommit()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var capabilityCalls = 0;
                    var readCalls = 0;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            Guid.NewGuid(),
                            DateTime.UtcNow,
                            new byte[] { 1, 0, 0, 0 });
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => true,
                                    () =>
                                    {
                                        capabilityCalls++;
                                        return Task.FromResult(
                                            CreateRecoveryCapabilities(
                                                BootId,
                                                capabilityCalls == 1
                                                    ? SdoMapRevision
                                                    : SdoMapRevision + 1));
                                    },
                                    metadata =>
                                    {
                                        readCalls++;
                                        return Task.FromResult(
                                            new byte[] { 1, 0, 0, 0 });
                                    })
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .IdentityMismatch,
                            result.Disposition);
                        AssertEx.Equal(2, capabilityCalls);
                        AssertEx.Equal(1, readCalls);
                        AssertEx.Equal(
                            DiagnosticsMutationState
                                .TerminalSuccessPendingReadback,
                            journal.CurrentRecord.State,
                            "A post-read map drift must leave the durable interlock active.");
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                        AssertEx.Equal(
                            DiagnosticsMutationState
                                .TerminalSuccessPendingReadback,
                            reopened.CurrentRecord.State);
                    }
                });
        }

        private static void
            RestartRecoveryAtomicTransitionRejectsStaleReadResult()
        {
            AssertRestartRecoveryAtomicTransitionRejectsStaleReadResult(
                new byte[] { 1, 0, 0, 0 },
                "match");
            AssertRestartRecoveryAtomicTransitionRejectsStaleReadResult(
                new byte[] { 2, 0, 0, 0 },
                "mismatch");
        }

        private static void
            AssertRestartRecoveryAtomicTransitionRejectsStaleReadResult(
                byte[] actualData,
                string resultKind)
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var capabilityCalls = 0;
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmTypedTerminalSdo(
                            journal,
                            identity,
                            createdUtc,
                            new byte[] { 1, 0, 0, 0 });
                        var result =
                            DiagnosticsSdoRestartRecoveryOrchestrator
                                .TryRecoverAsync(
                                    journal,
                                    true,
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    metadata => true,
                                    () =>
                                    {
                                        capabilityCalls++;
                                        if (capabilityCalls == 2)
                                        {
                                            journal.Transition(
                                                identity,
                                                DiagnosticsMutationState
                                                    .OutcomeUnverified,
                                                createdUtc.AddMilliseconds(3),
                                                0);
                                        }

                                        return Task.FromResult(
                                            CreateRecoveryCapabilities());
                                    },
                                    metadata => Task.FromResult(
                                        (byte[])actualData.Clone()))
                                .GetAwaiter()
                                .GetResult();
                        AssertEx.Equal(
                            DiagnosticsSdoRestartRecoveryDisposition
                                .StateChanged,
                            result.Disposition);
                        AssertEx.Equal(2, capabilityCalls);
                        AssertEx.Equal(
                            DiagnosticsMutationState.OutcomeUnverified,
                            journal.CurrentRecord.State,
                            "A stale " + resultKind
                                + " read result must not overwrite a record replaced immediately before the atomic commit.");
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                        AssertEx.Equal(
                            DiagnosticsMutationState.OutcomeUnverified,
                            reopened.CurrentRecord.State);
                    }
                });
        }

        private static void TransitionRequiresExactIdentity()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmOutput(journal, identity, createdUtc);

                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.Transition(
                                Guid.NewGuid(),
                                DiagnosticsMutationState
                                    .AcceptedPendingTerminal,
                                createdUtc.AddMilliseconds(1),
                                91));
                        AssertEx.Equal(
                            DiagnosticsMutationState.ArmedBeforeDispatch,
                            journal.CurrentRecord.State);
                        AssertEx.Equal(identity, journal.CurrentRecord.Identity);

                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.Transition(
                                identity,
                                DiagnosticsMutationState
                                    .TerminalSuccessPendingReadback,
                                createdUtc.AddMilliseconds(2),
                                91));
                        AssertEx.Equal(
                            DiagnosticsMutationState.ArmedBeforeDispatch,
                            journal.CurrentRecord.State);

                        journal.Transition(
                            identity,
                            DiagnosticsMutationState.AcceptedPendingTerminal,
                            createdUtc.AddMilliseconds(3),
                            91);
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.Transition(
                                identity,
                                DiagnosticsMutationState
                                    .TerminalSuccessPendingReadback,
                                createdUtc.AddMilliseconds(4),
                                92));
                        AssertEx.Equal((uint)91, journal.CurrentRecord.TicketId);

                        journal.Transition(
                            identity,
                            DiagnosticsMutationState
                                .TerminalSuccessPendingReadback,
                            createdUtc.AddMilliseconds(5),
                            91);
                        journal.Transition(
                            identity,
                            DiagnosticsMutationState.ReadbackMismatch,
                            createdUtc.AddMilliseconds(6),
                            0);
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.Transition(
                                identity,
                                DiagnosticsMutationState
                                    .TerminalSuccessPendingReadback,
                                createdUtc.AddMilliseconds(7),
                                91));
                        AssertEx.Equal(
                            DiagnosticsMutationState.ReadbackMismatch,
                            journal.CurrentRecord.State);
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.Equal(identity, reopened.CurrentRecord.Identity);
                        AssertEx.Equal(
                            DiagnosticsMutationState.ReadbackMismatch,
                            reopened.CurrentRecord.State);
                        AssertEx.Equal((uint)91, reopened.CurrentRecord.TicketId);
                    }
                });
        }

        private static void ActiveOverwriteIsBlocked()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var firstIdentity = Guid.NewGuid();
                    var secondIdentity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmOutput(journal, firstIdentity, createdUtc);
                        AssertEx.Throws<InvalidOperationException>(
                            () => journal.Arm(
                                DiagnosticsMutationKind.SdoWrite,
                                secondIdentity,
                                createdUtc.AddSeconds(1),
                                BootId,
                                SdoMapRevision,
                                SessionGeneration,
                                "Slave=2,Object=0x2F00,SubIndex=24",
                                "Int32=1"));

                        AssertEx.Equal(
                            firstIdentity,
                            journal.CurrentRecord.Identity);
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                        AssertEx.Equal(
                            firstIdentity,
                            reopened.CurrentRecord.Identity);
                    }
                });
        }

        private static void ResolvedTombstoneAllowsNewArm()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var firstIdentity = Guid.NewGuid();
                    var secondIdentity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmOutput(journal, firstIdentity, createdUtc);
                        var resolved = journal.Resolve(
                            firstIdentity,
                            createdUtc.AddSeconds(1));
                        AssertEx.False(journal.HasActiveRecord);
                        AssertEx.Equal(
                            DiagnosticsMutationState.Resolved,
                            resolved.State);
                    }

                    using (var tombstone =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.False(tombstone.HasActiveRecord);
                        AssertEx.Equal(
                            firstIdentity,
                            tombstone.CurrentRecord.Identity);
                        AssertEx.Equal(
                            DiagnosticsMutationState.Resolved,
                            tombstone.CurrentRecord.State);

                        tombstone.Arm(
                            DiagnosticsMutationKind.SdoWrite,
                            secondIdentity,
                            createdUtc.AddSeconds(2),
                            BootId,
                            SdoMapRevision,
                            SessionGeneration + 1,
                            "Slave=3,Object=0x2F00,SubIndex=24",
                            "Int32=2");
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                        AssertEx.Equal(
                            secondIdentity,
                            reopened.CurrentRecord.Identity);
                        AssertEx.Equal(
                            DiagnosticsMutationKind.SdoWrite,
                            reopened.CurrentRecord.Kind);
                        AssertEx.Equal(
                            DiagnosticsMutationState.ArmedBeforeDispatch,
                            reopened.CurrentRecord.State);
                    }
                });
        }

        private static void CorruptionFailsClosed()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    string journalPath;
                    using (var journal =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        ArmOutput(journal, identity, DateTime.UtcNow);
                        journalPath = journal.JournalFilePath;
                    }

                    using (var stream = new FileStream(
                        journalPath,
                        FileMode.Open,
                        FileAccess.ReadWrite,
                        FileShare.None))
                    {
                        AssertEx.True(stream.Length > 24);
                        stream.Position = 20;
                        var original = stream.ReadByte();
                        AssertEx.True(original >= 0);
                        stream.Position = 20;
                        stream.WriteByte((byte)(original ^ 0x5A));
                        stream.Flush(true);
                    }

                    AssertEx.Throws<InvalidDataException>(
                        () =>
                        {
                            using (DiagnosticsMutationJournal.Open(
                                directoryPath))
                            {
                            }
                        });
                });
        }

        private static void SecondWriterFailsClosed()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    using (var first =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.Throws<IOException>(
                            () =>
                            {
                                using (DiagnosticsMutationJournal.Open(
                                    directoryPath))
                                {
                                }
                            });

                        var identity = Guid.NewGuid();
                        ArmOutput(first, identity, DateTime.UtcNow);
                        AssertEx.True(first.HasActiveRecord);
                    }

                    using (var reopened =
                        DiagnosticsMutationJournal.Open(directoryPath))
                    {
                        AssertEx.True(reopened.HasActiveRecord);
                    }
                });
        }

        private static void ProcessTerminationReopenPreservesInterlock()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    var handshakePath = Path.Combine(
                        directoryPath,
                        CrashHandshakeFileName);
                    var journalPath = Path.Combine(
                        directoryPath,
                        DiagnosticsMutationJournal.JournalFileName);
                    Process child = null;
                    try
                    {
                        child = StartCrashChild(
                            directoryPath,
                            identity,
                            createdUtc);
                        WaitForCrashHandshake(
                            child,
                            handshakePath,
                            BuildCrashHandshake(identity, createdUtc));
                        AssertEx.False(
                            child.HasExited,
                            "Crash child exited before the parent terminated it.");

                        var persistedBeforeCrash =
                            File.ReadAllBytes(journalPath);
                        AssertEx.True(
                            persistedBeforeCrash.Length != 0,
                            "Crash child did not persist a journal record.");
                        AssertEx.Throws<IOException>(
                            () =>
                            {
                                using (DiagnosticsMutationJournal.Open(
                                    directoryPath))
                                {
                                }
                            },
                            "The live crash child must retain the single-writer lock.");

                        TerminateProcess(child);
                        AssertEx.SequenceEqual(
                            persistedBeforeCrash,
                            File.ReadAllBytes(journalPath),
                            "Process termination changed the persisted journal bytes.");

                        using (var reopened =
                            DiagnosticsMutationJournal.Open(directoryPath))
                        {
                            AssertEx.True(reopened.HasActiveRecord);
                            AssertRecord(
                                reopened.CurrentRecord,
                                identity,
                                DiagnosticsMutationKind.DigitalOutputWrite,
                                DiagnosticsMutationState.ArmedBeforeDispatch,
                                createdUtc,
                                createdUtc,
                                BootId,
                                OutputTopologyRevision,
                                SessionGeneration,
                                0,
                                "Node=0x00010001,IOReference=0x00020001",
                                "Value=0x00000001,Mask=0x00000001");
                            AssertEx.Throws<InvalidOperationException>(
                                () => ArmOutput(
                                    reopened,
                                    Guid.NewGuid(),
                                    createdUtc.AddSeconds(1)),
                                "A recovered active record must keep new mutations interlocked.");
                        }

                        AssertEx.SequenceEqual(
                            persistedBeforeCrash,
                            File.ReadAllBytes(journalPath),
                            "Reopen must not transition or rewrite the armed record.");
                    }
                    finally
                    {
                        if (child != null)
                        {
                            try
                            {
                                TerminateProcess(child);
                            }
                            finally
                            {
                                child.Dispose();
                            }
                        }
                    }
                });
        }

        private static void AnonymousStdinEofReopenPreservesInterlock()
        {
            WithTestDirectory(
                directoryPath =>
                {
                    var identity = Guid.NewGuid();
                    var createdUtc = DateTime.UtcNow;
                    var handshakePath = Path.Combine(
                        directoryPath,
                        CrashHandshakeFileName);
                    var journalPath = Path.Combine(
                        directoryPath,
                        DiagnosticsMutationJournal.JournalFileName);
                    Process child = null;
                    var cleanExitObserved = false;
                    try
                    {
                        child = StartCrashChild(
                            directoryPath,
                            identity,
                            createdUtc);
                        WaitForCrashHandshake(
                            child,
                            handshakePath,
                            BuildCrashHandshake(identity, createdUtc));
                        AssertEx.False(
                            child.HasExited,
                            "Crash child exited before its stdin EOF watchdog was exercised.");

                        var persistedBeforeEof = File.ReadAllBytes(
                            journalPath);
                        AssertEx.True(
                            persistedBeforeEof.Length != 0,
                            "Crash child did not persist a journal record before stdin EOF.");
                        AssertEx.Throws<IOException>(
                            () =>
                            {
                                using (DiagnosticsMutationJournal.Open(
                                    directoryPath))
                                {
                                }
                            },
                            "The live stdin-watchdog child must retain the single-writer lock.");

                        child.StandardInput.Close();
                        if (!child.WaitForExit(
                                CrashChildTimeoutMilliseconds))
                        {
                            throw new TimeoutException(
                                "Mutation-journal crash child did not terminate after anonymous stdin EOF.");
                        }

                        AssertEx.Equal(
                            0,
                            child.ExitCode,
                            "The stdin EOF watchdog must release the journal and exit cleanly.");
                        cleanExitObserved = true;
                        AssertEx.SequenceEqual(
                            persistedBeforeEof,
                            File.ReadAllBytes(journalPath),
                            "Stdin EOF shutdown changed the persisted armed record bytes.");

                        using (var reopened =
                            DiagnosticsMutationJournal.Open(directoryPath))
                        {
                            AssertEx.True(reopened.HasActiveRecord);
                            AssertRecord(
                                reopened.CurrentRecord,
                                identity,
                                DiagnosticsMutationKind.DigitalOutputWrite,
                                DiagnosticsMutationState.ArmedBeforeDispatch,
                                createdUtc,
                                createdUtc,
                                BootId,
                                OutputTopologyRevision,
                                SessionGeneration,
                                0,
                                "Node=0x00010001,IOReference=0x00020001",
                                "Value=0x00000001,Mask=0x00000001");
                            AssertEx.Throws<InvalidOperationException>(
                                () => ArmOutput(
                                    reopened,
                                    Guid.NewGuid(),
                                    createdUtc.AddSeconds(1)),
                                "An EOF-recovered armed record must keep new mutations interlocked.");
                        }

                        AssertEx.SequenceEqual(
                            persistedBeforeEof,
                            File.ReadAllBytes(journalPath),
                            "EOF recovery must not rewrite, replay, or transition the armed record.");
                    }
                    finally
                    {
                        if (child != null)
                        {
                            try
                            {
                                if (!cleanExitObserved)
                                {
                                    TerminateProcess(child);
                                }
                                else
                                {
                                    child.WaitForExit();
                                }
                            }
                            finally
                            {
                                child.Dispose();
                            }
                        }
                    }
                });
        }

        private static DiagnosticsMutationRecord ArmOutput(
            DiagnosticsMutationJournal journal,
            Guid identity,
            DateTime createdUtc)
        {
            return journal.Arm(
                DiagnosticsMutationKind.DigitalOutputWrite,
                identity,
                createdUtc,
                BootId,
                OutputTopologyRevision,
                SessionGeneration,
                "Node=0x00010001,IOReference=0x00020001",
                "Value=0x00000001,Mask=0x00000001");
        }

        private static DiagnosticsSdoWriteMutationMetadata CreateSdoMetadata(
            byte[] expectedWriteData)
        {
            return new DiagnosticsSdoWriteMutationMetadata(
                1,
                0x2F00,
                24,
                LMCSignalValueType.Int32,
                4,
                1000,
                expectedWriteData);
        }

        private static DiagnosticsMutationRecord ArmTypedTerminalSdo(
            DiagnosticsMutationJournal journal,
            Guid identity,
            DateTime createdUtc,
            byte[] expectedWriteData)
        {
            journal.Arm(
                DiagnosticsMutationKind.SdoWrite,
                identity,
                createdUtc,
                BootId,
                SdoMapRevision,
                SessionGeneration,
                "Slave=1,Object=0x2F00,SubIndex=24",
                "WriteData=" + BitConverter.ToString(expectedWriteData),
                CreateSdoMetadata(expectedWriteData));
            journal.Transition(
                identity,
                DiagnosticsMutationState.AcceptedPendingTerminal,
                createdUtc.AddMilliseconds(1),
                77);
            return journal.Transition(
                identity,
                DiagnosticsMutationState.TerminalSuccessPendingReadback,
                createdUtc.AddMilliseconds(2),
                77);
        }

        private static DiagnosticsSdoRestartRecoveryCapabilities
            CreateRecoveryCapabilities()
        {
            return CreateRecoveryCapabilities(BootId, SdoMapRevision);
        }

        private static DiagnosticsSdoRestartRecoveryCapabilities
            CreateRecoveryCapabilities(
                uint bootId,
                uint mapRevision)
        {
            return new DiagnosticsSdoRestartRecoveryCapabilities(
                bootId,
                mapRevision,
                true,
                true,
                4);
        }

        private static void WriteLegacyV1SdoRecord(
            string path,
            Guid identity,
            DateTime createdUtc,
            uint ticketId)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    payloadStream,
                    Encoding.UTF8,
                    true))
                {
                    writer.Write(identity.ToByteArray());
                    writer.Write((int)DiagnosticsMutationKind.SdoWrite);
                    writer.Write((int)DiagnosticsMutationState
                        .TerminalSuccessPendingReadback);
                    writer.Write(createdUtc.Ticks);
                    writer.Write(createdUtc.AddMilliseconds(2).Ticks);
                    writer.Write(BootId);
                    writer.Write(SdoMapRevision);
                    writer.Write(SessionGeneration);
                    writer.Write(ticketId);
                    WriteLegacyText(
                        writer,
                        "Slave=1,Object=0x2F00,SubIndex=24");
                    WriteLegacyText(writer, "WriteData=01-00-00-00");
                    writer.Flush();
                }

                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    stream,
                    Encoding.UTF8,
                    true))
                {
                    writer.Write(Encoding.ASCII.GetBytes("ELMODMJ1"));
                    writer.Write(1);
                    writer.Write(payload.Length);
                    writer.Write(payload);
                    writer.Flush();
                }

                prefix = stream.ToArray();
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
            File.WriteAllBytes(path, bytes);
        }

        private static void WriteLegacyText(
            BinaryWriter writer,
            string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static int FindV2MetadataMarkerOffset(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = new BinaryReader(
                stream,
                Encoding.UTF8,
                true))
            {
                AssertEx.SequenceEqual(
                    Encoding.ASCII.GetBytes("ELMODMJ1"),
                    reader.ReadBytes(8));
                AssertEx.Equal(2, reader.ReadInt32());
                var payloadLength = reader.ReadInt32();
                AssertEx.True(payloadLength > 0);
                reader.ReadBytes(16);
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt64();
                reader.ReadInt64();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadInt64();
                reader.ReadUInt32();
                SkipEncodedText(reader);
                SkipEncodedText(reader);
                return checked((int)stream.Position);
            }
        }

        private static void SkipEncodedText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            AssertEx.True(length > 0);
            AssertEx.Equal(length, reader.ReadBytes(length).Length);
        }

        private static void RewriteJournalChecksum(byte[] bytes)
        {
            const int checksumLength = 32;
            var checksumOffset = bytes.Length - checksumLength;
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

        private static Process StartCrashChild(
            string directoryPath,
            Guid identity,
            DateTime createdUtc)
        {
            var executablePath = Assembly.GetExecutingAssembly().Location;
            var arguments = string.Join(
                " ",
                new[]
                {
                    QuoteProcessArgument(CrashChildMode),
                    QuoteProcessArgument(directoryPath),
                    QuoteProcessArgument(identity.ToString("D")),
                    QuoteProcessArgument(
                        createdUtc.Ticks.ToString(
                            CultureInfo.InvariantCulture))
                });
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true
            };
            var child = new Process { StartInfo = startInfo };
            if (!child.Start())
            {
                child.Dispose();
                throw new InvalidOperationException(
                    "Failed to start the mutation-journal crash child.");
            }

            return child;
        }

        private static void WaitForCrashHandshake(
            Process child,
            string handshakePath,
            string expectedHandshake)
        {
            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds
                < CrashChildTimeoutMilliseconds)
            {
                if (File.Exists(handshakePath))
                {
                    AssertEx.Equal(
                        expectedHandshake,
                        File.ReadAllText(
                            handshakePath,
                            Encoding.ASCII),
                        "Crash child handshake identity does not match.");
                    return;
                }

                if (child.HasExited)
                {
                    child.WaitForExit();
                    throw new InvalidOperationException(
                        "Mutation-journal crash child exited before handshake. ExitCode="
                        + child.ExitCode
                        + ". See the inherited test output for child diagnostics.");
                }

                Thread.Sleep(20);
            }

            throw new TimeoutException(
                "Mutation-journal crash child did not publish its process-termination handshake within "
                + CrashChildTimeoutMilliseconds
                + " ms.");
        }

        private static void TerminateProcess(Process process)
        {
            if (process.HasExited)
            {
                process.WaitForExit();
                return;
            }

            System.ComponentModel.Win32Exception killError = null;
            try
            {
                process.Kill();
            }
            catch (InvalidOperationException)
            {
                // The process exited between HasExited and Kill.
            }
            catch (System.ComponentModel.Win32Exception error)
            {
                killError = error;
                try
                {
                    process.StandardInput.Close();
                }
                catch
                {
                    // Wait below and report the original Kill failure.
                }
            }

            if (!process.WaitForExit(CrashChildTimeoutMilliseconds))
            {
                throw new TimeoutException(
                    "Mutation-journal crash child did not terminate after Kill.");
            }

            if (killError != null)
            {
                throw new InvalidOperationException(
                    "Mutation-journal crash child Kill failed; its stdin watchdog was closed for cleanup, so process-termination recovery was not proven.",
                    killError);
            }
        }

        private static string BuildCrashHandshake(
            Guid identity,
            DateTime createdUtc)
        {
            return "ARMED|"
                + identity.ToString("D")
                + "|"
                + createdUtc.Ticks.ToString(CultureInfo.InvariantCulture)
                + "|ArmedBeforeDispatch";
        }

        private static void WriteCrashHandshake(
            string handshakePath,
            string handshake)
        {
            var temporaryPath = handshakePath
                + "."
                + Guid.NewGuid().ToString("N")
                + ".tmp";
            var temporaryExists = false;
            try
            {
                var bytes = Encoding.ASCII.GetBytes(handshake);
                using (var stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough))
                {
                    temporaryExists = true;
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                File.Move(temporaryPath, handshakePath);
                temporaryExists = false;
            }
            finally
            {
                if (temporaryExists && File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                        // Preserve the primary handshake failure.
                    }
                }
            }
        }

        private static string QuoteProcessArgument(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var result = new StringBuilder();
            result.Append('"');
            var backslashCount = 0;
            foreach (var character in value)
            {
                if (character == '\\')
                {
                    backslashCount++;
                    continue;
                }

                if (character == '"')
                {
                    result.Append('\\', backslashCount * 2 + 1);
                    result.Append('"');
                    backslashCount = 0;
                    continue;
                }

                result.Append('\\', backslashCount);
                backslashCount = 0;
                result.Append(character);
            }

            result.Append('\\', backslashCount * 2);
            result.Append('"');
            return result.ToString();
        }

        private static void AssertRecord(
            DiagnosticsMutationRecord record,
            Guid identity,
            DiagnosticsMutationKind kind,
            DiagnosticsMutationState state,
            DateTime createdUtc,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint identityRevision,
            long sessionGeneration,
            uint ticketId,
            string targetText,
            string expectedText)
        {
            AssertEx.NotNull(record);
            AssertEx.Equal(identity, record.Identity);
            AssertEx.Equal(kind, record.Kind);
            AssertEx.Equal(state, record.State);
            AssertEx.Equal(createdUtc, record.CreatedUtc);
            AssertEx.Equal(updatedUtc, record.UpdatedUtc);
            AssertEx.Equal(diagnosticsBootId, record.DiagnosticsBootId);
            AssertEx.Equal(identityRevision, record.IdentityRevision);
            AssertEx.Equal(sessionGeneration, record.SessionGeneration);
            AssertEx.Equal(ticketId, record.TicketId);
            AssertEx.Equal(targetText, record.TargetText);
            AssertEx.Equal(expectedText, record.ExpectedText);
        }

        private static void WithTestDirectory(Action<string> body)
        {
            var directoryPath = Path.Combine(
                Path.GetTempPath(),
                "Elmo-DiagnosticsMutationJournalTests-"
                    + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directoryPath);
            try
            {
                body(directoryPath);
            }
            finally
            {
                DeleteTestDirectory(directoryPath);
            }
        }

        private static void DeleteTestDirectory(string directoryPath)
        {
            var fullPath = RequireTestDirectoryPath(directoryPath);
            Exception lastError = null;
            for (var attempt = 0; attempt < 20; attempt++)
            {
                try
                {
                    if (Directory.Exists(fullPath))
                    {
                        Directory.Delete(fullPath, true);
                    }

                    return;
                }
                catch (Exception error)
                    when (error is IOException
                        || error is UnauthorizedAccessException)
                {
                    lastError = error;
                    Thread.Sleep(50);
                }
            }

            throw new IOException(
                "Failed to delete the diagnostics journal test directory.",
                lastError);
        }

        private static string RequireTestDirectoryPath(string directoryPath)
        {
            var fullPath = Path.GetFullPath(directoryPath).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            var temporaryRoot = Path.GetFullPath(Path.GetTempPath()).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            var parentPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(parentPath)
                || !string.Equals(
                    parentPath.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar),
                    temporaryRoot,
                    StringComparison.OrdinalIgnoreCase)
                || !Path.GetFileName(fullPath).StartsWith(
                    "Elmo-DiagnosticsMutationJournalTests-",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Refusing to use a non-test diagnostics journal directory.");
            }

            if (Directory.Exists(fullPath)
                && (File.GetAttributes(fullPath) & FileAttributes.ReparsePoint)
                    != 0)
            {
                throw new InvalidOperationException(
                    "Refusing to use a reparse-point diagnostics journal test directory.");
            }

            return fullPath;
        }
    }
}

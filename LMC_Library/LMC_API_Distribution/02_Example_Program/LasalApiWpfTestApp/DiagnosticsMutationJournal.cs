using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum DiagnosticsMutationKind
    {
        SdoWrite = 1,
        DigitalOutputWrite = 2
    }

    internal enum DiagnosticsMutationState
    {
        ArmedBeforeDispatch = 1,
        AcceptedPendingTerminal = 2,
        TerminalSuccessPendingReadback = 3,
        OutcomeUnverified = 4,
        ReadbackMismatch = 5,
        Resolved = 6
    }

    internal sealed class DiagnosticsSdoWriteMutationMetadata
    {
        private readonly byte[] expectedWriteData;

        internal DiagnosticsSdoWriteMutationMetadata(
            ushort slaveReference,
            ushort objectIndex,
            byte subIndex,
            LMCSignalValueType valueType,
            ushort dataLength,
            uint timeoutCycles,
            byte[] expectedWriteData)
        {
            if (slaveReference < 1 || slaveReference > 4)
            {
                throw new ArgumentOutOfRangeException(
                    "slaveReference",
                    "Durable SDO recovery supports SlaveReference 1 through 4 only.");
            }

            if (objectIndex == 0 || IsPermanentlyUnsafeObject(objectIndex))
            {
                throw new ArgumentOutOfRangeException(
                    "objectIndex",
                    "Durable SDO recovery cannot target a direct motion/control object.");
            }

            if (valueType != LMCSignalValueType.Int32
                && valueType != LMCSignalValueType.UInt32)
            {
                throw new NotSupportedException(
                    "Durable SDO recovery supports only approved 32-bit integer targets.");
            }

            if (dataLength != 4)
            {
                throw new ArgumentOutOfRangeException(
                    "dataLength",
                    "Durable SDO recovery requires exactly four data bytes.");
            }

            if (timeoutCycles < 1 || timeoutCycles > 60000)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutCycles",
                    "Durable SDO recovery requires TimeoutCycles from 1 through 60000.");
            }

            if (expectedWriteData == null
                || expectedWriteData.Length != dataLength)
            {
                throw new ArgumentException(
                    "Expected SDO Write data must exactly match DataLength.",
                    "expectedWriteData");
            }

            SlaveReference = slaveReference;
            ObjectIndex = objectIndex;
            SubIndex = subIndex;
            ValueType = valueType;
            DataLength = dataLength;
            TimeoutCycles = timeoutCycles;
            this.expectedWriteData = (byte[])expectedWriteData.Clone();
        }

        internal ushort SlaveReference { get; private set; }
        internal ushort ObjectIndex { get; private set; }
        internal byte SubIndex { get; private set; }
        internal LMCSignalValueType ValueType { get; private set; }
        internal ushort DataLength { get; private set; }
        internal uint TimeoutCycles { get; private set; }
        internal byte[] ExpectedWriteData
        {
            get { return (byte[])expectedWriteData.Clone(); }
        }

        private static bool IsPermanentlyUnsafeObject(ushort objectIndex)
        {
            return objectIndex == 0x6040
                || objectIndex == 0x607A
                || objectIndex == 0x60FF
                || objectIndex == 0x6071;
        }
    }

    internal enum DiagnosticsSdoRestartRecoveryDisposition
    {
        NotEligible = 0,
        TargetNotApproved = 1,
        CapabilitiesUnsupported = 2,
        IdentityMismatch = 3,
        StateChanged = 4,
        Verified = 5,
        ReadbackMismatch = 6
    }

    internal sealed class DiagnosticsSdoRestartRecoveryCapabilities
    {
        internal DiagnosticsSdoRestartRecoveryCapabilities(
            uint diagnosticsBootId,
            uint mapRevision,
            bool supportsSdoRead,
            bool supportsGeneralInlineSdoRead,
            ushort maxSdoDataBytes)
        {
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            SupportsSdoRead = supportsSdoRead;
            SupportsGeneralInlineSdoRead =
                supportsGeneralInlineSdoRead;
            MaxSdoDataBytes = maxSdoDataBytes;
        }

        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal bool SupportsSdoRead { get; private set; }
        internal bool SupportsGeneralInlineSdoRead { get; private set; }
        internal ushort MaxSdoDataBytes { get; private set; }
    }

    internal sealed class DiagnosticsSdoRestartRecoveryResult
    {
        internal DiagnosticsSdoRestartRecoveryResult(
            DiagnosticsSdoRestartRecoveryDisposition disposition)
        {
            Disposition = disposition;
        }

        internal DiagnosticsSdoRestartRecoveryDisposition Disposition
        {
            get;
            private set;
        }
    }

    internal static class DiagnosticsSdoRestartRecoveryPolicy
    {
        internal static bool CanAttempt(
            DiagnosticsMutationRecord record,
            bool recoveredAtStartup,
            bool idle,
            bool connected,
            bool hasPendingVolatileReadback,
            bool hasD5TicketOrQuarantine,
            bool hasUnresolvedDigitalOutputWrite,
            bool alreadyAttempted)
        {
            return record != null
                && record.IsActive
                && record.Kind == DiagnosticsMutationKind.SdoWrite
                && record.State
                    == DiagnosticsMutationState
                        .TerminalSuccessPendingReadback
                && record.HasTypedSdoWriteMetadata
                && recoveredAtStartup
                && idle
                && connected
                && !hasPendingVolatileReadback
                && !hasD5TicketOrQuarantine
                && !hasUnresolvedDigitalOutputWrite
                && !alreadyAttempted;
        }
    }

    internal static class DiagnosticsSdoRestartRecoveryOrchestrator
    {
        internal static async Task<DiagnosticsSdoRestartRecoveryResult>
            TryRecoverAsync(
                DiagnosticsMutationJournal journal,
                bool recoveredAtStartup,
                bool idle,
                bool connected,
                bool hasPendingVolatileReadback,
                bool hasD5TicketOrQuarantine,
                bool hasUnresolvedDigitalOutputWrite,
                Func<DiagnosticsSdoWriteMutationMetadata, bool>
                    exactTargetApproved,
                Func<Task<DiagnosticsSdoRestartRecoveryCapabilities>>
                    readCapabilitiesAsync,
                Func<DiagnosticsSdoWriteMutationMetadata, Task<byte[]>>
                    readExactTargetAsync)
        {
            if (journal == null)
            {
                throw new ArgumentNullException("journal");
            }

            if (exactTargetApproved == null)
            {
                throw new ArgumentNullException("exactTargetApproved");
            }

            if (readCapabilitiesAsync == null)
            {
                throw new ArgumentNullException("readCapabilitiesAsync");
            }

            if (readExactTargetAsync == null)
            {
                throw new ArgumentNullException("readExactTargetAsync");
            }

            var record = journal.CurrentRecord;
            if (!DiagnosticsSdoRestartRecoveryPolicy.CanAttempt(
                    record,
                    recoveredAtStartup,
                    idle,
                    connected,
                    hasPendingVolatileReadback,
                    hasD5TicketOrQuarantine,
                    hasUnresolvedDigitalOutputWrite,
                    false))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition.NotEligible);
            }

            var metadata = record.SdoWriteMetadata;

            // This local allowlist decision deliberately precedes every
            // capability or SDO delegate. Legacy v1 records and disabled
            // compile-time targets therefore remain zero-wire.
            if (!exactTargetApproved(metadata))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition
                        .TargetNotApproved);
            }

            var capabilities = await readCapabilitiesAsync();
            if (!SupportsRequiredCapabilities(capabilities, metadata))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition
                        .CapabilitiesUnsupported);
            }

            if (!MatchesDurableIdentity(capabilities, record))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition
                        .IdentityMismatch);
            }

            var afterCapabilities = journal.CurrentRecord;
            if (!IsSameEligibleRecord(record, afterCapabilities))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition.StateChanged);
            }

            var actualData = await readExactTargetAsync(metadata);
            var afterRead = journal.CurrentRecord;
            if (!IsSameEligibleRecord(record, afterRead))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition.StateChanged);
            }

            if (actualData == null
                || actualData.Length != metadata.DataLength)
            {
                throw new InvalidDataException(
                    "Restart SDO readback did not return the exact typed data length.");
            }

            var postReadCapabilities = await readCapabilitiesAsync();
            if (!SupportsRequiredCapabilities(
                    postReadCapabilities,
                    metadata))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition
                        .CapabilitiesUnsupported);
            }

            if (!MatchesDurableIdentity(postReadCapabilities, record))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition
                        .IdentityMismatch);
            }

            if (ByteArraysEqual(
                    metadata.ExpectedWriteData,
                    actualData))
            {
                if (!journal.TryTransitionExpected(
                        record,
                        DiagnosticsMutationState
                            .TerminalSuccessPendingReadback,
                        DiagnosticsMutationState.Resolved,
                        GetTransitionUtc(record),
                        0))
                {
                    return CreateResult(
                        DiagnosticsSdoRestartRecoveryDisposition
                            .StateChanged);
                }

                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition.Verified);
            }

            if (!journal.TryTransitionExpected(
                    record,
                    DiagnosticsMutationState
                        .TerminalSuccessPendingReadback,
                    DiagnosticsMutationState.ReadbackMismatch,
                    GetTransitionUtc(record),
                    0))
            {
                return CreateResult(
                    DiagnosticsSdoRestartRecoveryDisposition.StateChanged);
            }

            return CreateResult(
                DiagnosticsSdoRestartRecoveryDisposition.ReadbackMismatch);
        }

        private static bool SupportsRequiredCapabilities(
            DiagnosticsSdoRestartRecoveryCapabilities capabilities,
            DiagnosticsSdoWriteMutationMetadata metadata)
        {
            return capabilities != null
                && capabilities.SupportsSdoRead
                && capabilities.SupportsGeneralInlineSdoRead
                && capabilities.MaxSdoDataBytes >= metadata.DataLength;
        }

        private static bool MatchesDurableIdentity(
            DiagnosticsSdoRestartRecoveryCapabilities capabilities,
            DiagnosticsMutationRecord record)
        {
            return capabilities != null
                && capabilities.DiagnosticsBootId
                    == record.DiagnosticsBootId
                && capabilities.MapRevision == record.IdentityRevision;
        }

        private static DiagnosticsSdoRestartRecoveryResult CreateResult(
            DiagnosticsSdoRestartRecoveryDisposition disposition)
        {
            return new DiagnosticsSdoRestartRecoveryResult(disposition);
        }

        private static DateTime GetTransitionUtc(
            DiagnosticsMutationRecord record)
        {
            var now = DateTime.UtcNow;
            return now < record.UpdatedUtc ? record.UpdatedUtc : now;
        }

        private static bool IsSameEligibleRecord(
            DiagnosticsMutationRecord expected,
            DiagnosticsMutationRecord actual)
        {
            return expected != null
                && actual != null
                && ReferenceEquals(expected, actual)
                && actual.Identity == expected.Identity
                && actual.Kind == expected.Kind
                && actual.State == expected.State
                && actual.DiagnosticsBootId
                    == expected.DiagnosticsBootId
                && actual.IdentityRevision == expected.IdentityRevision
                && actual.TicketId == expected.TicketId
                && actual.HasTypedSdoWriteMetadata;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }
    }

    internal sealed class DiagnosticsMutationRecord
    {
        private const int MaximumTextLength = 2048;

        internal DiagnosticsMutationRecord(
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
            string expectedText,
            DiagnosticsSdoWriteMutationMetadata sdoWriteMetadata = null)
        {
            ValidateIdentity(identity);
            ValidateKind(kind);
            ValidateState(state);
            ValidateTimestamps(createdUtc, updatedUtc);
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Diagnostics mutation evidence requires a non-zero BootId.");
            }

            if (identityRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "identityRevision",
                    "Diagnostics mutation evidence requires a non-zero identity revision.");
            }

            if (sessionGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "sessionGeneration",
                    "Diagnostics mutation evidence requires a positive session generation.");
            }

            if (RequiresTicket(state) && ticketId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "ticketId",
                    "The selected diagnostics mutation state requires a non-zero ticket.");
            }

            ValidateText(targetText, "targetText");
            ValidateText(expectedText, "expectedText");
            if (sdoWriteMetadata != null
                && kind != DiagnosticsMutationKind.SdoWrite)
            {
                throw new ArgumentException(
                    "Typed SDO metadata can be attached only to an SDO Write record.",
                    "sdoWriteMetadata");
            }

            Identity = identity;
            Kind = kind;
            State = state;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DiagnosticsBootId = diagnosticsBootId;
            IdentityRevision = identityRevision;
            SessionGeneration = sessionGeneration;
            TicketId = ticketId;
            TargetText = targetText;
            ExpectedText = expectedText;
            SdoWriteMetadata = sdoWriteMetadata;
        }

        internal Guid Identity { get; private set; }
        internal DiagnosticsMutationKind Kind { get; private set; }
        internal DiagnosticsMutationState State { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint IdentityRevision { get; private set; }
        internal long SessionGeneration { get; private set; }
        internal uint TicketId { get; private set; }
        internal string TargetText { get; private set; }
        internal string ExpectedText { get; private set; }
        internal DiagnosticsSdoWriteMutationMetadata SdoWriteMetadata
        {
            get;
            private set;
        }
        internal bool HasTypedSdoWriteMetadata
        {
            get { return SdoWriteMetadata != null; }
        }

        internal bool IsActive
        {
            get { return State != DiagnosticsMutationState.Resolved; }
        }

        internal DiagnosticsMutationRecord TransitionTo(
            DiagnosticsMutationState state,
            DateTime updatedUtc,
            uint ticketId)
        {
            if (!CanTransition(State, state))
            {
                throw new InvalidOperationException(
                    "Diagnostics mutation state cannot transition from "
                    + State
                    + " to "
                    + state
                    + ".");
            }

            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < UpdatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Mutation transition time must be UTC and cannot move backwards.");
            }

            if (TicketId != 0
                && ticketId != 0
                && TicketId != ticketId)
            {
                throw new InvalidOperationException(
                    "A diagnostics mutation ticket cannot be replaced.");
            }

            var effectiveTicketId = ticketId == 0 ? TicketId : ticketId;
            return new DiagnosticsMutationRecord(
                Identity,
                Kind,
                state,
                CreatedUtc,
                updatedUtc,
                DiagnosticsBootId,
                IdentityRevision,
                SessionGeneration,
                effectiveTicketId,
                TargetText,
                ExpectedText,
                SdoWriteMetadata);
        }

        internal static void ValidateKind(DiagnosticsMutationKind kind)
        {
            if (kind != DiagnosticsMutationKind.SdoWrite
                && kind != DiagnosticsMutationKind.DigitalOutputWrite)
            {
                throw new ArgumentOutOfRangeException("kind");
            }
        }

        internal static void ValidateState(DiagnosticsMutationState state)
        {
            if (state < DiagnosticsMutationState.ArmedBeforeDispatch
                || state > DiagnosticsMutationState.Resolved)
            {
                throw new ArgumentOutOfRangeException("state");
            }
        }

        private static void ValidateIdentity(Guid identity)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Diagnostics mutation identity cannot be empty.",
                    "identity");
            }
        }

        private static void ValidateTimestamps(
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (createdUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Diagnostics mutation creation time must be UTC.",
                    "createdUtc");
            }

            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentException(
                    "Diagnostics mutation update time must be UTC and cannot precede creation.",
                    "updatedUtc");
            }
        }

        private static void ValidateText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Diagnostics mutation text cannot be empty.",
                    parameterName);
            }

            if (value.Length > MaximumTextLength)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Diagnostics mutation text is too long.");
            }
        }

        private static bool RequiresTicket(DiagnosticsMutationState state)
        {
            return state
                    == DiagnosticsMutationState.AcceptedPendingTerminal
                || state
                    == DiagnosticsMutationState
                        .TerminalSuccessPendingReadback
                || state == DiagnosticsMutationState.ReadbackMismatch;
        }

        private static bool CanTransition(
            DiagnosticsMutationState current,
            DiagnosticsMutationState next)
        {
            if (current == DiagnosticsMutationState.Resolved
                || current == next)
            {
                return false;
            }

            if (next == DiagnosticsMutationState.Resolved)
            {
                return true;
            }

            switch (current)
            {
                case DiagnosticsMutationState.ArmedBeforeDispatch:
                    return next
                            == DiagnosticsMutationState
                                .AcceptedPendingTerminal
                        || next
                            == DiagnosticsMutationState.OutcomeUnverified;

                case DiagnosticsMutationState.AcceptedPendingTerminal:
                    return next
                            == DiagnosticsMutationState
                                .TerminalSuccessPendingReadback
                        || next
                            == DiagnosticsMutationState.OutcomeUnverified;

                case DiagnosticsMutationState
                    .TerminalSuccessPendingReadback:
                    return next == DiagnosticsMutationState.ReadbackMismatch
                        || next
                            == DiagnosticsMutationState.OutcomeUnverified;

                case DiagnosticsMutationState.OutcomeUnverified:
                    return next
                        == DiagnosticsMutationState.ReadbackMismatch;

                case DiagnosticsMutationState.ReadbackMismatch:
                    return false;

                default:
                    return false;
            }
        }
    }

    internal sealed class DiagnosticsMutationJournal : IDisposable
    {
        internal const string JournalFileName = "journal.dat";
        internal const string LockFileName = "journal.lock";

        private const int LegacyFormatVersion = 1;
        private const int FormatVersion = 2;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 65536;
        private const int MaximumTextByteLength = 8192;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMODMJ1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private DiagnosticsMutationRecord currentRecord;
        private bool disposed;

        private DiagnosticsMutationJournal(string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "A diagnostics mutation journal directory is required.",
                    "requestedDirectoryPath");
            }

            directoryPath = Path.GetFullPath(requestedDirectoryPath);
            journalFilePath = Path.Combine(
                directoryPath,
                JournalFileName);

            Directory.CreateDirectory(directoryPath);
            try
            {
                lockStream = new FileStream(
                    Path.Combine(directoryPath, LockFileName),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    1,
                    FileOptions.WriteThrough);
                currentRecord = LoadRecord(journalFilePath);
            }
            catch
            {
                if (lockStream != null)
                {
                    lockStream.Dispose();
                    lockStream = null;
                }

                throw;
            }
        }

        internal string DirectoryPath
        {
            get { return directoryPath; }
        }

        internal string JournalFilePath
        {
            get { return journalFilePath; }
        }

        internal DiagnosticsMutationRecord CurrentRecord
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord;
                }
            }
        }

        internal bool HasActiveRecord
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord != null
                        && currentRecord.IsActive;
                }
            }
        }

        internal static DiagnosticsMutationJournal Open(
            string directoryPath)
        {
            return new DiagnosticsMutationJournal(directoryPath);
        }

        internal static DiagnosticsMutationJournal OpenDefault()
        {
            return Open(GetDefaultDirectoryPath());
        }

        internal static string GetDefaultDirectoryPath()
        {
            var localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localApplicationData))
            {
                throw new InvalidOperationException(
                    "Windows LocalApplicationData is unavailable.");
            }

            return Path.Combine(
                localApplicationData,
                "Elmo",
                "LasalMotionControlApiExample",
                "DiagnosticsMutationJournal",
                "v1");
        }

        internal DiagnosticsMutationRecord Arm(
            DiagnosticsMutationKind kind,
            Guid identity,
            DateTime createdUtc,
            uint diagnosticsBootId,
            uint identityRevision,
            long sessionGeneration,
            string targetText,
            string expectedText,
            DiagnosticsSdoWriteMutationMetadata sdoWriteMetadata = null)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved diagnostics mutation record already exists.");
                }

                var armed = new DiagnosticsMutationRecord(
                    identity,
                    kind,
                    DiagnosticsMutationState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc,
                    diagnosticsBootId,
                    identityRevision,
                    sessionGeneration,
                    0,
                    targetText,
                    expectedText,
                    sdoWriteMetadata);
                PersistRecord(armed);
                currentRecord = armed;
                return armed;
            }
        }

        internal DiagnosticsMutationRecord Transition(
            Guid identity,
            DiagnosticsMutationState state,
            DateTime updatedUtc,
            uint ticketId)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var transitioned = current.TransitionTo(
                    state,
                    updatedUtc,
                    ticketId);
                PersistRecord(transitioned);
                currentRecord = transitioned;
                return transitioned;
            }
        }

        internal DiagnosticsMutationRecord Resolve(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                DiagnosticsMutationState.Resolved,
                updatedUtc,
                0);
        }

        internal RecoveryJournalSourceEvidence
            CaptureLegacyEndpointBoundRetirementEvidence(
                string operatorClassifiedEndpointIp,
                int operatorClassifiedEndpointPort)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                return CaptureLegacyEndpointBoundRetirementEvidenceCore(
                    operatorClassifiedEndpointIp,
                    operatorClassifiedEndpointPort);
            }
        }

        internal DiagnosticsMutationRecord ResolveOperatorRetirement(
            RecoveryJournalSourceEvidence expectedEvidence,
            RecoveryRecordRetirementDecision committedDecision,
            DateTime updatedUtc)
        {
            if (expectedEvidence == null)
            {
                throw new ArgumentNullException("expectedEvidence");
            }
            if (committedDecision == null)
            {
                throw new ArgumentNullException("committedDecision");
            }
            if (!committedDecision.IsDurablyCommitted)
            {
                throw new InvalidOperationException(
                    "Diagnostics mutation retirement requires a durably committed ledger decision.");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                if (!committedDecision.MatchesSourceEvidence(
                    expectedEvidence))
                {
                    throw new InvalidOperationException(
                        "The committed retirement decision does not match the expected diagnostics mutation source evidence.");
                }

                var currentEvidence =
                    CaptureLegacyEndpointBoundRetirementEvidenceCore(
                        expectedEvidence.EndpointIp,
                        expectedEvidence.EndpointPort);
                if (!expectedEvidence.ExactSourceEquals(currentEvidence)
                    || !committedDecision.MatchesSourceEvidence(
                        currentEvidence))
                {
                    throw new InvalidOperationException(
                        "Diagnostics mutation recovery changed after operator confirmation; retirement was not applied.");
                }

                var resolved = currentRecord.TransitionTo(
                    DiagnosticsMutationState.Resolved,
                    updatedUtc,
                    0);
                PersistRecord(resolved);
                currentRecord = resolved;
                return resolved;
            }
        }

        internal bool TryTransitionExpected(
            DiagnosticsMutationRecord expectedRecord,
            DiagnosticsMutationState expectedState,
            DiagnosticsMutationState state,
            DateTime updatedUtc,
            uint ticketId)
        {
            if (expectedRecord == null)
            {
                throw new ArgumentNullException("expectedRecord");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord == null
                    || !ReferenceEquals(currentRecord, expectedRecord)
                    || expectedRecord.State != expectedState
                    || currentRecord.State != expectedState)
                {
                    return false;
                }

                var transitioned = currentRecord.TransitionTo(
                    state,
                    updatedUtc,
                    ticketId);
                PersistRecord(transitioned);
                currentRecord = transitioned;
                return true;
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                if (lockStream != null)
                {
                    lockStream.Dispose();
                    lockStream = null;
                }
            }
        }

        private DiagnosticsMutationRecord RequireCurrentRecord(
            Guid identity)
        {
            if (currentRecord == null)
            {
                throw new InvalidOperationException(
                    "No diagnostics mutation record exists.");
            }

            if (identity == Guid.Empty
                || currentRecord.Identity != identity)
            {
                throw new InvalidOperationException(
                    "Diagnostics mutation transition identity does not match the durable record.");
            }

            return currentRecord;
        }

        private RecoveryJournalSourceEvidence
            CaptureLegacyEndpointBoundRetirementEvidenceCore(
                string operatorClassifiedEndpointIp,
                int operatorClassifiedEndpointPort)
        {
            if (currentRecord == null
                || !currentRecord.IsActive
                || currentRecord.Kind != DiagnosticsMutationKind.SdoWrite
                || currentRecord.State
                    != DiagnosticsMutationState.OutcomeUnverified
                || !currentRecord.HasTypedSdoWriteMetadata)
            {
                throw new InvalidOperationException(
                    "Only an active typed SDO Write OutcomeUnverified record can use legacy endpoint-bound operator retirement.");
            }

            var originalBytes = ReadRetirementSourceBytes();
            var diskRecord = DeserializeRecord(originalBytes);
            if (!RecordsEqual(currentRecord, diskRecord))
            {
                throw new InvalidDataException(
                    "Diagnostics mutation memory state does not match the exact durable source bytes.");
            }

            var metadata = diskRecord.SdoWriteMetadata;
            return new RecoveryJournalSourceEvidence(
                RecoveryRecordOwner.DiagnosticsMutation,
                diskRecord.Identity,
                (int)diskRecord.State,
                diskRecord.CreatedUtc,
                diskRecord.UpdatedUtc,
                operatorClassifiedEndpointIp,
                operatorClassifiedEndpointPort,
                0,
                diskRecord.DiagnosticsBootId,
                diskRecord.IdentityRevision,
                "DiagnosticsMutationLegacyEndpointUnbound",
                diskRecord.TargetText,
                metadata.SlaveReference,
                "SdoWrite/OutcomeUnverified",
                "EndpointBinding=OperatorClassifiedCurrentQuarantineEndpoint;"
                    + "Expected="
                    + diskRecord.ExpectedText
                    + ";Ticket="
                    + diskRecord.TicketId
                    + ";SessionGeneration="
                    + diskRecord.SessionGeneration,
                originalBytes,
                RecoveryEndpointEvidenceKind
                    .OperatorClassifiedLegacyEndpoint);
        }

        private byte[] ReadRetirementSourceBytes()
        {
            using (var stream = new FileStream(
                journalFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            {
                if (stream.Length < 1 || stream.Length > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation retirement source length is invalid.");
                }

                var bytes = new byte[checked((int)stream.Length)];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    var read = stream.Read(
                        bytes,
                        offset,
                        bytes.Length - offset);
                    if (read == 0)
                    {
                        throw new EndOfStreamException(
                            "Diagnostics mutation retirement source is incomplete.");
                    }
                    offset += read;
                }
                return bytes;
            }
        }

        private static bool RecordsEqual(
            DiagnosticsMutationRecord left,
            DiagnosticsMutationRecord right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return left.Identity == right.Identity
                && left.Kind == right.Kind
                && left.State == right.State
                && left.CreatedUtc == right.CreatedUtc
                && left.UpdatedUtc == right.UpdatedUtc
                && left.DiagnosticsBootId == right.DiagnosticsBootId
                && left.IdentityRevision == right.IdentityRevision
                && left.SessionGeneration == right.SessionGeneration
                && left.TicketId == right.TicketId
                && string.Equals(
                    left.TargetText,
                    right.TargetText,
                    StringComparison.Ordinal)
                && string.Equals(
                    left.ExpectedText,
                    right.ExpectedText,
                    StringComparison.Ordinal)
                && SdoWriteMetadataEqual(
                    left.SdoWriteMetadata,
                    right.SdoWriteMetadata);
        }

        private static bool SdoWriteMetadataEqual(
            DiagnosticsSdoWriteMutationMetadata left,
            DiagnosticsSdoWriteMutationMetadata right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return left.SlaveReference == right.SlaveReference
                && left.ObjectIndex == right.ObjectIndex
                && left.SubIndex == right.SubIndex
                && left.ValueType == right.ValueType
                && left.DataLength == right.DataLength
                && left.TimeoutCycles == right.TimeoutCycles
                && ByteArraysEqual(
                    left.ExpectedWriteData,
                    right.ExpectedWriteData);
        }

        private void PersistRecord(DiagnosticsMutationRecord record)
        {
            var bytes = SerializeRecord(record);
            var temporaryPath = Path.Combine(
                directoryPath,
                JournalFileName
                    + "."
                    + Guid.NewGuid().ToString("N")
                    + ".tmp");
            var temporaryExists = false;
            try
            {
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

                if (File.Exists(journalFilePath))
                {
                    File.Replace(
                        temporaryPath,
                        journalFilePath,
                        null,
                        true);
                }
                else
                {
                    File.Move(temporaryPath, journalFilePath);
                }

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
                        // Preserve the primary persistence failure.
                    }
                }
            }
        }

        private static DiagnosticsMutationRecord LoadRecord(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception error)
            {
                throw new IOException(
                    "The diagnostics mutation journal could not be read.",
                    error);
            }

            try
            {
                return DeserializeRecord(bytes);
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception error)
            {
                throw new InvalidDataException(
                    "The diagnostics mutation journal is corrupt.",
                    error);
            }
        }

        private static byte[] SerializeRecord(
            DiagnosticsMutationRecord record)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    payloadStream,
                    Encoding.UTF8,
                    true))
                {
                    writer.Write(record.Identity.ToByteArray());
                    writer.Write((int)record.Kind);
                    writer.Write((int)record.State);
                    writer.Write(record.CreatedUtc.Ticks);
                    writer.Write(record.UpdatedUtc.Ticks);
                    writer.Write(record.DiagnosticsBootId);
                    writer.Write(record.IdentityRevision);
                    writer.Write(record.SessionGeneration);
                    writer.Write(record.TicketId);
                    WriteText(writer, record.TargetText);
                    WriteText(writer, record.ExpectedText);
                    WriteSdoWriteMetadata(
                        writer,
                        record.SdoWriteMetadata);
                    writer.Flush();
                }

                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var fileStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    fileStream,
                    Encoding.UTF8,
                    true))
                {
                    writer.Write(Magic);
                    writer.Write(FormatVersion);
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

        private static DiagnosticsMutationRecord DeserializeRecord(
            byte[] bytes)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Diagnostics mutation journal length is invalid.");
            }

            var checksumOffset = bytes.Length - ChecksumLength;
            byte[] computedChecksum;
            using (var sha256 = SHA256.Create())
            {
                computedChecksum = sha256.ComputeHash(
                    bytes,
                    0,
                    checksumOffset);
            }

            if (!ChecksumEquals(
                    computedChecksum,
                    bytes,
                    checksumOffset))
            {
                throw new InvalidDataException(
                    "Diagnostics mutation journal checksum is invalid.");
            }

            using (var fileStream = new MemoryStream(
                bytes,
                0,
                checksumOffset,
                false))
            using (var reader = new BinaryReader(
                fileStream,
                Encoding.UTF8,
                true))
            {
                var magic = reader.ReadBytes(Magic.Length);
                if (!ByteArraysEqual(Magic, magic))
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation journal magic is invalid.");
                }

                var version = reader.ReadInt32();
                if (version != LegacyFormatVersion
                    && version != FormatVersion)
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation journal version is unsupported.");
                }

                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength
                        != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation journal payload length is invalid.");
                }

                var payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || fileStream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation journal payload is incomplete.");
                }

                return DeserializePayload(payload, version);
            }
        }

        private static DiagnosticsMutationRecord DeserializePayload(
            byte[] payload,
            int version)
        {
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(
                stream,
                Encoding.UTF8,
                true))
            {
                var identityBytes = reader.ReadBytes(16);
                if (identityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation identity is incomplete.");
                }

                var identity = new Guid(identityBytes);
                var kind = (DiagnosticsMutationKind)reader.ReadInt32();
                var state = (DiagnosticsMutationState)reader.ReadInt32();
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var diagnosticsBootId = reader.ReadUInt32();
                var identityRevision = reader.ReadUInt32();
                var sessionGeneration = reader.ReadInt64();
                var ticketId = reader.ReadUInt32();
                var targetText = ReadText(reader);
                var expectedText = ReadText(reader);
                var sdoWriteMetadata = version >= FormatVersion
                    ? ReadSdoWriteMetadata(reader)
                    : null;
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation journal has trailing payload data.");
                }

                try
                {
                    return new DiagnosticsMutationRecord(
                        identity,
                        kind,
                        state,
                        createdUtc,
                        updatedUtc,
                        diagnosticsBootId,
                        identityRevision,
                        sessionGeneration,
                        ticketId,
                        targetText,
                        expectedText,
                        sdoWriteMetadata);
                }
                catch (ArgumentException error)
                {
                    throw new InvalidDataException(
                        "Diagnostics mutation journal record is invalid.",
                        error);
                }
            }
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Diagnostics mutation text encoding is too large.");
            }

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static void WriteSdoWriteMetadata(
            BinaryWriter writer,
            DiagnosticsSdoWriteMutationMetadata metadata)
        {
            writer.Write(metadata != null);
            if (metadata == null)
            {
                return;
            }

            writer.Write(metadata.SlaveReference);
            writer.Write(metadata.ObjectIndex);
            writer.Write(metadata.SubIndex);
            writer.Write((int)metadata.ValueType);
            writer.Write(metadata.DataLength);
            writer.Write(metadata.TimeoutCycles);
            var expectedData = metadata.ExpectedWriteData;
            writer.Write(expectedData.Length);
            writer.Write(expectedData);
        }

        private static DiagnosticsSdoWriteMutationMetadata
            ReadSdoWriteMetadata(BinaryReader reader)
        {
            var presenceMarker = reader.ReadByte();
            if (presenceMarker == 0)
            {
                return null;
            }

            if (presenceMarker != 1)
            {
                throw new InvalidDataException(
                    "Diagnostics mutation SDO metadata marker is non-canonical.");
            }

            var slaveReference = reader.ReadUInt16();
            var objectIndex = reader.ReadUInt16();
            var subIndex = reader.ReadByte();
            var valueType = (LMCSignalValueType)reader.ReadInt32();
            var dataLength = reader.ReadUInt16();
            var timeoutCycles = reader.ReadUInt32();
            var expectedLength = reader.ReadInt32();
            if (expectedLength <= 0 || expectedLength > 12)
            {
                throw new InvalidDataException(
                    "Diagnostics mutation SDO data length is invalid.");
            }

            var expectedData = reader.ReadBytes(expectedLength);
            if (expectedData.Length != expectedLength)
            {
                throw new InvalidDataException(
                    "Diagnostics mutation SDO data is incomplete.");
            }

            return new DiagnosticsSdoWriteMutationMetadata(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles,
                expectedData);
        }

        private static string ReadText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length <= 0 || length > MaximumTextByteLength)
            {
                throw new InvalidDataException(
                    "Diagnostics mutation text length is invalid.");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Diagnostics mutation text is incomplete.");
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private static bool ChecksumEquals(
            byte[] expected,
            byte[] actual,
            int actualOffset)
        {
            if (expected == null
                || expected.Length != ChecksumLength
                || actual == null
                || actualOffset < 0
                || actual.Length - actualOffset != ChecksumLength)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < ChecksumLength; index++)
            {
                difference |= expected[index] ^ actual[actualOffset + index];
            }

            return difference == 0;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    "DiagnosticsMutationJournal");
            }
        }
    }
}

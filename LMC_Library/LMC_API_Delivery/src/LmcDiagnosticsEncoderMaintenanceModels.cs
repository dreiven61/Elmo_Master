using System;
using System.Security.Cryptography;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCEncoderFeedbackSocket : uint
    {
        Socket1 = 1,
        Socket2 = 2,
        Socket3 = 3,
        Socket4 = 4
    }

    public enum LMCEncoderMaintenanceKind : ushort
    {
        Tw20ErrorWarningReset = 1,
        Tw19MultiturnPositionReset = 2
    }

    public static class LMCEncoderMaintenanceCapabilities
    {
        public const uint Tw20ErrorWarningReset = 1u << 18;
        public const uint Tw19MultiturnPositionReset = 1u << 19;
    }

    public static class LMCEncoderMaintenanceSdoContract
    {
        public const ushort ObjectIndex = 0x20FC;
        public const byte Tw19MultiturnPositionResetSubIndex = 0x01;
        public const byte Tw20ErrorWarningResetSubIndex = 0x02;
        public const uint ResetCommandValue = 1;
        public const ushort WireSchemaCompatibilityProfileId = 1;
        public const LMCEncoderFeedbackSocket WireSchemaFeedbackSocket =
            LMCEncoderFeedbackSocket.Socket1;
        public const ushort WriteLength = 2;
        public const LMCSignalValueType ValueType =
            LMCSignalValueType.UInt16;

        public static byte SubIndex(LMCEncoderMaintenanceKind kind)
        {
            switch (kind)
            {
                case LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset:
                    return Tw19MultiturnPositionResetSubIndex;
                case LMCEncoderMaintenanceKind.Tw20ErrorWarningReset:
                    return Tw20ErrorWarningResetSubIndex;
                default:
                    throw new ArgumentOutOfRangeException("kind");
            }
        }
    }

    public enum LMCEncoderMaintenanceDetailCode : uint
    {
        CompatibilityMismatch = 33,
        OutcomeNotFound = 34,
        OutcomeIndeterminate = 35,
        OutcomeStoreCorrupt = 36,
        OutcomeKeyMismatch = 37,
        OutcomeStorageUnavailable = 38,
        ExecutionFailed = 39,
        Aborted = 40,
        OutcomeSlotOccupied = 41,
        SemanticVerificationFailed = 42
    }

    public enum LMCEncoderMaintenanceOutcomeRecordState : ushort
    {
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Aborted = 4
    }

    [Flags]
    public enum LMCEncoderMaintenanceVerificationFlags : uint
    {
        None = 0,
        DriveTargetSelected = 1u << 0,
        ExactSdoContractMatched = 1u << 1,
        MotorOffPreStable = 1u << 2,
        SdoWriteCompleted = 1u << 3,
        ExecutorDrained = 1u << 4,
        MotorOffPostStable = 1u << 5,
        StableWindowSatisfied = 1u << 6,
        OwnerCleanupComplete = 1u << 7,
        PostWriteStateObserved = 1u << 8,
        NoMotionCommandActive = 1u << 9,
        [Obsolete("Use DriveTargetSelected. This bit does not prove physical drive identity.")]
        DriveIdentityMatched = DriveTargetSelected,
        [Obsolete("Use ExactSdoContractMatched. This bit does not prove encoder family identity.")]
        EncoderIdentityMatched = ExactSdoContractMatched,
        [Obsolete("Use PostWriteStateObserved. Physical reset effect requires independent verification.")]
        SemanticResultVerified = PostWriteStateObserved
    }

    internal static class LMCEncoderMaintenanceContract
    {
        internal const int MaximumTimeoutMilliseconds = 60000;
        internal const uint Tw20ExecuteToken = 0x30325754u;
        internal const uint Tw19ExecuteToken = 0x39315754u;
        internal const uint KnownVerificationFlagsMask = 0x000003FFu;
        internal const uint RequiredSuccessVerificationFlags =
            KnownVerificationFlagsMask;

        internal static uint RequiredCapability(
            LMCEncoderMaintenanceKind kind)
        {
            switch (kind)
            {
                case LMCEncoderMaintenanceKind.Tw20ErrorWarningReset:
                    return LMCEncoderMaintenanceCapabilities
                        .Tw20ErrorWarningReset;
                case LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset:
                    return LMCEncoderMaintenanceCapabilities
                        .Tw19MultiturnPositionReset;
                default:
                    throw new ArgumentOutOfRangeException(
                        "kind",
                        "Encoder maintenance kind is not defined by schema version 1.");
            }
        }

        internal static uint ExecuteToken(LMCEncoderMaintenanceKind kind)
        {
            switch (kind)
            {
                case LMCEncoderMaintenanceKind.Tw20ErrorWarningReset:
                    return Tw20ExecuteToken;
                case LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset:
                    return Tw19ExecuteToken;
                default:
                    throw new ArgumentOutOfRangeException("kind");
            }
        }

        internal static void ValidateRequestFields(
            LMCEncoderMaintenanceKind kind,
            ushort compatibilityProfileId,
            ushort driveReference,
            LMCEncoderFeedbackSocket feedbackSocket,
            uint timeoutMilliseconds)
        {
            RequiredCapability(kind);

            if (compatibilityProfileId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "compatibilityProfileId",
                    "CompatibilityProfileId must be non-zero.");
            }

            if (driveReference < 1 || driveReference > 4)
            {
                throw new ArgumentOutOfRangeException(
                    "driveReference",
                    "Encoder maintenance supports drive references 1 through 4 only.");
            }

            if (feedbackSocket < LMCEncoderFeedbackSocket.Socket1
                || feedbackSocket > LMCEncoderFeedbackSocket.Socket4)
            {
                throw new ArgumentOutOfRangeException(
                    "feedbackSocket",
                    "Encoder maintenance accepts feedback socket values 1 through 4 only.");
            }

            if (timeoutMilliseconds == 0
                || timeoutMilliseconds > MaximumTimeoutMilliseconds)
            {
                throw new ArgumentOutOfRangeException(
                    "timeoutMilliseconds",
                    "TimeoutMilliseconds must be in the range 1 through 60000.");
            }
        }
    }

    public sealed class LMCEncoderMaintenanceClientIntentId
        : IEquatable<LMCEncoderMaintenanceClientIntentId>
    {
        public LMCEncoderMaintenanceClientIntentId(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            if (word0 == 0 && word1 == 0 && word2 == 0 && word3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit client intent identifier must not be all zero.");
            }

            Word0 = word0;
            Word1 = word1;
            Word2 = word2;
            Word3 = word3;
        }

        public uint Word0 { get; private set; }
        public uint Word1 { get; private set; }
        public uint Word2 { get; private set; }
        public uint Word3 { get; private set; }

        public static LMCEncoderMaintenanceClientIntentId Create()
        {
            var words = LMCEncoderMaintenanceRandomId.CreateWords();
            return new LMCEncoderMaintenanceClientIntentId(
                words[0],
                words[1],
                words[2],
                words[3]);
        }

        public bool Equals(LMCEncoderMaintenanceClientIntentId other)
        {
            return other != null
                && Word0 == other.Word0
                && Word1 == other.Word1
                && Word2 == other.Word2
                && Word3 == other.Word3;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCEncoderMaintenanceClientIntentId);
        }

        public override int GetHashCode()
        {
            return LMCEncoderMaintenanceRandomId.GetWordHash(
                Word0,
                Word1,
                Word2,
                Word3);
        }
    }

    public sealed class LMCEncoderMaintenanceCompatibilityEvidenceId
        : IEquatable<LMCEncoderMaintenanceCompatibilityEvidenceId>
    {
        public LMCEncoderMaintenanceCompatibilityEvidenceId(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            if (word0 == 0 && word1 == 0 && word2 == 0 && word3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit compatibility evidence identifier must not be all zero.");
            }

            Word0 = word0;
            Word1 = word1;
            Word2 = word2;
            Word3 = word3;
        }

        public static LMCEncoderMaintenanceCompatibilityEvidenceId Create()
        {
            var words = LMCEncoderMaintenanceRandomId.CreateWords();
            return new LMCEncoderMaintenanceCompatibilityEvidenceId(
                words[0],
                words[1],
                words[2],
                words[3]);
        }

        public uint Word0 { get; private set; }
        public uint Word1 { get; private set; }
        public uint Word2 { get; private set; }
        public uint Word3 { get; private set; }

        public bool Equals(LMCEncoderMaintenanceCompatibilityEvidenceId other)
        {
            return other != null
                && Word0 == other.Word0
                && Word1 == other.Word1
                && Word2 == other.Word2
                && Word3 == other.Word3;
        }

        public override bool Equals(object obj)
        {
            return Equals(
                obj as LMCEncoderMaintenanceCompatibilityEvidenceId);
        }

        public override int GetHashCode()
        {
            return LMCEncoderMaintenanceRandomId.GetWordHash(
                Word0,
                Word1,
                Word2,
                Word3);
        }
    }

    internal static class LMCEncoderMaintenanceRandomId
    {
        internal static uint[] CreateWords()
        {
            var bytes = new byte[16];
            using (var random = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    random.GetBytes(bytes);
                    var words = new uint[4];
                    for (var index = 0; index < words.Length; index++)
                    {
                        var offset = index * 4;
                        words[index] = (uint)bytes[offset]
                            | ((uint)bytes[offset + 1] << 8)
                            | ((uint)bytes[offset + 2] << 16)
                            | ((uint)bytes[offset + 3] << 24);
                    }

                    if (words[0] != 0
                        || words[1] != 0
                        || words[2] != 0
                        || words[3] != 0)
                    {
                        return words;
                    }
                }
            }
        }

        internal static int GetWordHash(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            unchecked
            {
                var hash = (int)word0;
                hash = (hash * 397) ^ (int)word1;
                hash = (hash * 397) ^ (int)word2;
                hash = (hash * 397) ^ (int)word3;
                return hash;
            }
        }
    }

    public abstract class LMCEncoderMaintenanceRequest
    {
        internal LMCEncoderMaintenanceRequest(
            LMCEncoderMaintenanceKind kind,
            ushort compatibilityProfileId,
            ushort driveReference,
            LMCEncoderFeedbackSocket feedbackSocket,
            uint timeoutMilliseconds,
            LMCEncoderMaintenanceCompatibilityEvidenceId compatibilityEvidenceId)
        {
            LMCEncoderMaintenanceContract.ValidateRequestFields(
                kind,
                compatibilityProfileId,
                driveReference,
                feedbackSocket,
                timeoutMilliseconds);

            if (compatibilityEvidenceId == null)
            {
                throw new ArgumentNullException("compatibilityEvidenceId");
            }

            Kind = kind;
            CompatibilityProfileId = compatibilityProfileId;
            DriveReference = driveReference;
            FeedbackSocket = feedbackSocket;
            TimeoutMilliseconds = timeoutMilliseconds;
            CompatibilityEvidenceId = compatibilityEvidenceId;
        }

        public LMCEncoderMaintenanceKind Kind { get; private set; }
        public ushort CompatibilityProfileId { get; private set; }
        public ushort DriveReference { get; private set; }
        public LMCEncoderFeedbackSocket FeedbackSocket { get; private set; }
        public uint CommandValue
        {
            get { return LMCEncoderMaintenanceSdoContract.ResetCommandValue; }
        }
        public ushort ObjectIndex
        {
            get { return LMCEncoderMaintenanceSdoContract.ObjectIndex; }
        }
        public byte SubIndex
        {
            get { return LMCEncoderMaintenanceSdoContract.SubIndex(Kind); }
        }
        public LMCSignalValueType ValueType
        {
            get { return LMCEncoderMaintenanceSdoContract.ValueType; }
        }
        public ushort WriteLength
        {
            get { return LMCEncoderMaintenanceSdoContract.WriteLength; }
        }
        /// <summary>
        /// Overall encoder-maintenance timeout measured by the PLC service
        /// clock in milliseconds.
        /// </summary>
        public uint TimeoutMilliseconds { get; private set; }
        public LMCEncoderMaintenanceCompatibilityEvidenceId
            CompatibilityEvidenceId { get; private set; }
    }

    public sealed class LMCTw20EncoderErrorWarningResetRequest
        : LMCEncoderMaintenanceRequest
    {
        public LMCTw20EncoderErrorWarningResetRequest(
            ushort driveReference,
            uint timeoutMilliseconds)
            : this(
                LMCEncoderMaintenanceSdoContract
                    .WireSchemaCompatibilityProfileId,
                driveReference,
                LMCEncoderMaintenanceSdoContract.WireSchemaFeedbackSocket,
                timeoutMilliseconds,
                LMCEncoderMaintenanceCompatibilityEvidenceId.Create())
        {
        }

        public LMCTw20EncoderErrorWarningResetRequest(
            ushort compatibilityProfileId,
            ushort driveReference,
            LMCEncoderFeedbackSocket feedbackSocket,
            uint timeoutMilliseconds,
            LMCEncoderMaintenanceCompatibilityEvidenceId compatibilityEvidenceId)
            : base(
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset,
                compatibilityProfileId,
                driveReference,
                feedbackSocket,
                timeoutMilliseconds,
                compatibilityEvidenceId)
        {
        }
    }

    public sealed class LMCTw19MultiturnPositionResetRequest
        : LMCEncoderMaintenanceRequest
    {
        public LMCTw19MultiturnPositionResetRequest(
            ushort driveReference,
            uint timeoutMilliseconds)
            : this(
                LMCEncoderMaintenanceSdoContract
                    .WireSchemaCompatibilityProfileId,
                driveReference,
                LMCEncoderMaintenanceSdoContract.WireSchemaFeedbackSocket,
                timeoutMilliseconds,
                LMCEncoderMaintenanceCompatibilityEvidenceId.Create())
        {
        }

        public LMCTw19MultiturnPositionResetRequest(
            ushort compatibilityProfileId,
            ushort driveReference,
            LMCEncoderFeedbackSocket feedbackSocket,
            uint timeoutMilliseconds,
            LMCEncoderMaintenanceCompatibilityEvidenceId compatibilityEvidenceId)
            : base(
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset,
                compatibilityProfileId,
                driveReference,
                feedbackSocket,
                timeoutMilliseconds,
                compatibilityEvidenceId)
        {
        }
    }

    public sealed class LMCTw20EncoderErrorWarningResetExecuteToken
    {
        private int consumed;

        private LMCTw20EncoderErrorWarningResetExecuteToken()
        {
        }

        public static LMCTw20EncoderErrorWarningResetExecuteToken Create()
        {
            return new LMCTw20EncoderErrorWarningResetExecuteToken();
        }

        public bool IsConsumed
        {
            get { return Volatile.Read(ref consumed) != 0; }
        }

        internal void ConsumeForPreparation()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This TW20 encoder maintenance confirmation token has already prepared an intent.");
            }
        }
    }

    public sealed class LMCTw19MultiturnPositionResetExecuteToken
    {
        private int consumed;

        private LMCTw19MultiturnPositionResetExecuteToken()
        {
        }

        public static LMCTw19MultiturnPositionResetExecuteToken Create()
        {
            return new LMCTw19MultiturnPositionResetExecuteToken();
        }

        public bool IsConsumed
        {
            get { return Volatile.Read(ref consumed) != 0; }
        }

        internal void ConsumeForPreparation()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This TW19 encoder maintenance confirmation token has already prepared an intent.");
            }
        }
    }

    public sealed class LMCEncoderMaintenanceRecoveryKey
        : IEquatable<LMCEncoderMaintenanceRecoveryKey>
    {
        public LMCEncoderMaintenanceRecoveryKey(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCEncoderMaintenanceClientIntentId clientIntentId,
            LMCEncoderMaintenanceKind kind,
            ushort compatibilityProfileId,
            ushort driveReference,
            LMCEncoderFeedbackSocket feedbackSocket,
            uint timeoutMilliseconds,
            LMCEncoderMaintenanceCompatibilityEvidenceId compatibilityEvidenceId)
        {
            if (schemaVersion != LMCDiagnostics.ProtocolSchemaVersion)
            {
                throw new ArgumentOutOfRangeException("schemaVersion");
            }

            if (originalRequestId == 0)
            {
                throw new ArgumentOutOfRangeException("originalRequestId");
            }

            if (diagnosticsBuild == 0
                || diagnosticsBootId == 0
                || mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBuild",
                    "DiagnosticsBuild, DiagnosticsBootId, and MapRevision must all be non-zero.");
            }

            if (clientIntentId == null)
            {
                throw new ArgumentNullException("clientIntentId");
            }

            if (compatibilityEvidenceId == null)
            {
                throw new ArgumentNullException("compatibilityEvidenceId");
            }

            LMCEncoderMaintenanceContract.ValidateRequestFields(
                kind,
                compatibilityProfileId,
                driveReference,
                feedbackSocket,
                timeoutMilliseconds);

            SchemaVersion = schemaVersion;
            OriginalRequestId = originalRequestId;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            ClientIntentId = clientIntentId;
            Kind = kind;
            CompatibilityProfileId = compatibilityProfileId;
            DriveReference = driveReference;
            FeedbackSocket = feedbackSocket;
            TimeoutMilliseconds = timeoutMilliseconds;
            CompatibilityEvidenceId = compatibilityEvidenceId;
        }

        public ushort SchemaVersion { get; private set; }
        public uint OriginalRequestId { get; private set; }
        public uint DiagnosticsBuild { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCEncoderMaintenanceClientIntentId ClientIntentId
        {
            get;
            private set;
        }
        public LMCEncoderMaintenanceKind Kind { get; private set; }
        public ushort CompatibilityProfileId { get; private set; }
        public ushort DriveReference { get; private set; }
        public LMCEncoderFeedbackSocket FeedbackSocket { get; private set; }
        public uint CommandValue
        {
            get { return LMCEncoderMaintenanceSdoContract.ResetCommandValue; }
        }
        public ushort ObjectIndex
        {
            get { return LMCEncoderMaintenanceSdoContract.ObjectIndex; }
        }
        public byte SubIndex
        {
            get { return LMCEncoderMaintenanceSdoContract.SubIndex(Kind); }
        }
        public LMCSignalValueType ValueType
        {
            get { return LMCEncoderMaintenanceSdoContract.ValueType; }
        }
        public ushort WriteLength
        {
            get { return LMCEncoderMaintenanceSdoContract.WriteLength; }
        }
        /// <summary>
        /// Overall encoder-maintenance timeout measured by the PLC service
        /// clock in milliseconds.
        /// </summary>
        public uint TimeoutMilliseconds { get; private set; }
        public LMCEncoderMaintenanceCompatibilityEvidenceId
            CompatibilityEvidenceId { get; private set; }

        public bool Equals(LMCEncoderMaintenanceRecoveryKey other)
        {
            return other != null
                && SchemaVersion == other.SchemaVersion
                && OriginalRequestId == other.OriginalRequestId
                && DiagnosticsBuild == other.DiagnosticsBuild
                && DiagnosticsBootId == other.DiagnosticsBootId
                && MapRevision == other.MapRevision
                && ClientIntentId.Equals(other.ClientIntentId)
                && Kind == other.Kind
                && CompatibilityProfileId == other.CompatibilityProfileId
                && DriveReference == other.DriveReference
                && FeedbackSocket == other.FeedbackSocket
                && TimeoutMilliseconds == other.TimeoutMilliseconds
                && CompatibilityEvidenceId.Equals(
                    other.CompatibilityEvidenceId);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCEncoderMaintenanceRecoveryKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)OriginalRequestId;
                hash = (hash * 397) ^ (int)DiagnosticsBuild;
                hash = (hash * 397) ^ (int)DiagnosticsBootId;
                hash = (hash * 397) ^ (int)MapRevision;
                hash = (hash * 397) ^ ClientIntentId.GetHashCode();
                hash = (hash * 397) ^ (int)Kind;
                hash = (hash * 397) ^ CompatibilityProfileId;
                hash = (hash * 397) ^ DriveReference;
                hash = (hash * 397) ^ (int)FeedbackSocket;
                hash = (hash * 397) ^ (int)TimeoutMilliseconds;
                hash = (hash * 397)
                    ^ CompatibilityEvidenceId.GetHashCode();
                return hash;
            }
        }
    }

    public sealed class LMCPreparedEncoderMaintenance
    {
        private int consumed;

        internal LMCPreparedEncoderMaintenance(
            LMCDiagnostics owner,
            long connectionSessionGeneration,
            LMCDiagnosticCapabilities verifiedCapabilities,
            LMCEncoderMaintenanceRequest request,
            LMCEncoderMaintenanceRecoveryKey recoveryKey,
            uint executeToken)
        {
            Owner = owner;
            ConnectionSessionGeneration = connectionSessionGeneration;
            VerifiedCapabilities = verifiedCapabilities;
            Request = request;
            RecoveryKey = recoveryKey;
            ExecuteToken = executeToken;
        }

        public LMCEncoderMaintenanceRequest Request { get; private set; }
        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public long ConnectionSessionGeneration { get; private set; }
        public bool IsConsumed
        {
            get { return Volatile.Read(ref consumed) != 0; }
        }

        internal LMCDiagnostics Owner { get; private set; }
        internal LMCDiagnosticCapabilities VerifiedCapabilities
        {
            get;
            private set;
        }
        internal uint ExecuteToken { get; private set; }

        internal void ThrowIfConsumed()
        {
            if (IsConsumed)
            {
                throw new InvalidOperationException(
                    "This prepared encoder maintenance command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }

        internal void ConsumeAtWriteBoundary()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This prepared encoder maintenance command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }
    }

    public sealed class LMCEncoderMaintenanceStartAcknowledgement
    {
        internal LMCEncoderMaintenanceStartAcknowledgement(
            LMCDiagnosticsResponse response,
            LMCEncoderMaintenanceRecoveryKey recoveryKey,
            uint recordGeneration,
            uint ownerGeneration,
            uint startCycle)
        {
            Response = response;
            RecoveryKey = recoveryKey;
            RecordGeneration = recordGeneration;
            OwnerGeneration = ownerGeneration;
            StartCycle = startCycle;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public LMCEncoderMaintenanceKind Kind
        {
            get { return RecoveryKey.Kind; }
        }
        public ushort DriveReference
        {
            get { return RecoveryKey.DriveReference; }
        }
        public LMCEncoderFeedbackSocket FeedbackSocket
        {
            get { return RecoveryKey.FeedbackSocket; }
        }
        public uint CommandValue
        {
            get { return RecoveryKey.CommandValue; }
        }
        public uint RecordGeneration { get; private set; }
        public uint OwnerGeneration { get; private set; }
        public uint StartCycle { get; private set; }
    }

    public sealed class LMCEncoderMaintenanceOutcomeResult
    {
        internal LMCEncoderMaintenanceOutcomeResult(
            LMCDiagnosticsResponse response,
            LMCEncoderMaintenanceRecoveryKey recoveryKey,
            LMCEncoderMaintenanceOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint sdoAbortCode,
            uint startCycle,
            uint writeCompletionCycle,
            uint completionCycle,
            uint executorState,
            uint verificationFlags,
            uint preEvidence0,
            uint postEvidence0,
            uint preEvidence1,
            uint postEvidence1,
            ushort statusWord,
            int axisError,
            uint driveErrorCode,
            int actualPosition,
            uint recordGeneration,
            uint ownerGeneration)
        {
            Response = response;
            RecoveryKey = recoveryKey;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            SdoAbortCode = sdoAbortCode;
            StartCycle = startCycle;
            WriteCompletionCycle = writeCompletionCycle;
            CompletionCycle = completionCycle;
            ExecutorState = executorState;
            VerificationFlagsValue = verificationFlags;
            PreEvidence0 = preEvidence0;
            PostEvidence0 = postEvidence0;
            PreEvidence1 = preEvidence1;
            PostEvidence1 = postEvidence1;
            StatusWord = statusWord;
            AxisError = axisError;
            DriveErrorCode = driveErrorCode;
            ActualPosition = actualPosition;
            RecordGeneration = recordGeneration;
            OwnerGeneration = ownerGeneration;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public LMCEncoderMaintenanceOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        public LMCEncoderMaintenanceKind Kind
        {
            get { return RecoveryKey.Kind; }
        }
        public ushort OriginalCommandStatus { get; private set; }
        public short OriginalErrorId { get; private set; }
        public uint OriginalDetailCode { get; private set; }
        public uint SdoAbortCode { get; private set; }
        public uint StartCycle { get; private set; }
        public uint WriteCompletionCycle { get; private set; }
        public uint CompletionCycle { get; private set; }
        public uint ExecutorState { get; private set; }
        public uint VerificationFlagsValue { get; private set; }
        public LMCEncoderMaintenanceVerificationFlags VerificationFlags
        {
            get
            {
                return (LMCEncoderMaintenanceVerificationFlags)
                    VerificationFlagsValue;
            }
        }
        public uint PreEvidence0 { get; private set; }
        public uint PostEvidence0 { get; private set; }
        public uint PreEvidence1 { get; private set; }
        public uint PostEvidence1 { get; private set; }
        public ushort StatusWord { get; private set; }
        public int AxisError { get; private set; }
        public uint DriveErrorCode { get; private set; }
        public int ActualPosition { get; private set; }
        public uint RecordGeneration { get; private set; }
        public uint OwnerGeneration { get; private set; }
        public bool IsTerminal
        {
            get
            {
                return RecordState
                    != LMCEncoderMaintenanceOutcomeRecordState.Running;
            }
        }
        public bool IsSuccessful
        {
            get
            {
                return RecordState
                    == LMCEncoderMaintenanceOutcomeRecordState.Succeeded;
            }
        }
        /// <summary>
        /// The protocol cannot prove the physical TW19/TW20 encoder effect.
        /// A successful record proves the exact SDO completion, drain, stable
        /// motor-off observation, and owner cleanup only.
        /// </summary>
        public bool IsPhysicalEffectVerified
        {
            get { return false; }
        }

        internal bool HasExactTerminalSnapshot(
            LMCEncoderMaintenanceOutcomeResult other)
        {
            return other != null
                && IsTerminal
                && other.IsTerminal
                && RecoveryKey.Equals(other.RecoveryKey)
                && RecordState == other.RecordState
                && OriginalCommandStatus == other.OriginalCommandStatus
                && OriginalErrorId == other.OriginalErrorId
                && OriginalDetailCode == other.OriginalDetailCode
                && SdoAbortCode == other.SdoAbortCode
                && StartCycle == other.StartCycle
                && WriteCompletionCycle == other.WriteCompletionCycle
                && CompletionCycle == other.CompletionCycle
                && ExecutorState == other.ExecutorState
                && VerificationFlagsValue == other.VerificationFlagsValue
                && PreEvidence0 == other.PreEvidence0
                && PostEvidence0 == other.PostEvidence0
                && PreEvidence1 == other.PreEvidence1
                && PostEvidence1 == other.PostEvidence1
                && StatusWord == other.StatusWord
                && AxisError == other.AxisError
                && DriveErrorCode == other.DriveErrorCode
                && ActualPosition == other.ActualPosition
                && RecordGeneration == other.RecordGeneration
                && OwnerGeneration == other.OwnerGeneration;
        }
    }

    public sealed class LMCEncoderMaintenanceOutcomeRetirementResult
    {
        internal LMCEncoderMaintenanceOutcomeRetirementResult(
            LMCEncoderMaintenanceOutcomeResult terminalOutcome)
        {
            TerminalOutcome = terminalOutcome;
        }

        public LMCEncoderMaintenanceOutcomeResult TerminalOutcome
        {
            get;
            private set;
        }
        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get { return TerminalOutcome.RecoveryKey; }
        }
        public uint RecordGeneration
        {
            get { return TerminalOutcome.RecordGeneration; }
        }
    }

    public sealed class LMCEncoderMaintenanceCommandRejectedException
        : InvalidOperationException
    {
        internal LMCEncoderMaintenanceCommandRejectedException(
            LMCPreparedEncoderMaintenance preparedCommand,
            Exception innerException)
            : base(
                "The PLC explicitly rejected the encoder maintenance start command. The one-shot prepared command remains consumed.",
                innerException)
        {
            PreparedCommand = preparedCommand;
            RecoveryKey = preparedCommand.RecoveryKey;
        }

        public LMCPreparedEncoderMaintenance PreparedCommand
        {
            get;
            private set;
        }
        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
    }

    public sealed class LMCEncoderMaintenanceOutcomeUncertainException
        : InvalidOperationException
    {
        internal LMCEncoderMaintenanceOutcomeUncertainException(
            LMCPreparedEncoderMaintenance preparedCommand,
            Exception innerException)
            : base(
                "The encoder maintenance start may have reached the PLC. Persist the recovery key, do not replay the prepared command, and query the dedicated outcome record.",
                innerException)
        {
            PreparedCommand = preparedCommand;
            RecoveryKey = preparedCommand.RecoveryKey;
        }

        public LMCPreparedEncoderMaintenance PreparedCommand
        {
            get;
            private set;
        }
        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
    }

    public sealed class LMCEncoderMaintenanceOutcomeQueryException
        : InvalidOperationException
    {
        internal LMCEncoderMaintenanceOutcomeQueryException(
            LMCEncoderMaintenanceRecoveryKey recoveryKey,
            Exception innerException)
            : base(
                "The PLC rejected the encoder maintenance outcome query.",
                innerException)
        {
            RecoveryKey = recoveryKey;
        }

        public LMCEncoderMaintenanceRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
    }

    public sealed class LMCEncoderMaintenanceOutcomeRetirementException
        : InvalidOperationException
    {
        internal LMCEncoderMaintenanceOutcomeRetirementException(
            LMCEncoderMaintenanceOutcomeResult terminalOutcome,
            Exception innerException)
            : base(
                "The PLC rejected retirement of the exact encoder maintenance terminal outcome.",
                innerException)
        {
            TerminalOutcome = terminalOutcome;
        }

        public LMCEncoderMaintenanceOutcomeResult TerminalOutcome
        {
            get;
            private set;
        }
    }
}

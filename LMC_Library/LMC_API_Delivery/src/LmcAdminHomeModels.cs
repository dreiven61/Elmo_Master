using System;
using System.Security.Cryptography;
using System.Threading;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Stable semantic meaning of LMC_Home command 0x7D13. The command does
    /// not move the axis and does not use a reference or limit switch.
    /// </summary>
    public enum LMCHomeSemanticMode : ushort
    {
        CurrentPositionZero = 1
    }

    /// <summary>
    /// Four raw U32 words identifying one LMC_Home intent. The words are
    /// serialized independently and do not use Guid byte-order semantics.
    /// </summary>
    public sealed class LMCHomeClientIntentId
        : IEquatable<LMCHomeClientIntentId>
    {
        public LMCHomeClientIntentId(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            if (word0 == 0 && word1 == 0 && word2 == 0 && word3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit LMC_Home client intent identifier must not be all zero.");
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

        public static LMCHomeClientIntentId Create()
        {
            var bytes = new byte[16];
            using (var random = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    random.GetBytes(bytes);
                    var word0 = ReadRawWord(bytes, 0);
                    var word1 = ReadRawWord(bytes, 4);
                    var word2 = ReadRawWord(bytes, 8);
                    var word3 = ReadRawWord(bytes, 12);
                    if (word0 != 0
                        || word1 != 0
                        || word2 != 0
                        || word3 != 0)
                    {
                        return new LMCHomeClientIntentId(
                            word0,
                            word1,
                            word2,
                            word3);
                    }
                }
            }
        }

        public bool Equals(LMCHomeClientIntentId other)
        {
            return other != null
                && Word0 == other.Word0
                && Word1 == other.Word1
                && Word2 == other.Word2
                && Word3 == other.Word3;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCHomeClientIntentId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)Word0;
                hash = (hash * 397) ^ (int)Word1;
                hash = (hash * 397) ^ (int)Word2;
                hash = (hash * 397) ^ (int)Word3;
                return hash;
            }
        }

        private static uint ReadRawWord(byte[] bytes, int offset)
        {
            return (uint)bytes[offset]
                | ((uint)bytes[offset + 1] << 8)
                | ((uint)bytes[offset + 2] << 16)
                | ((uint)bytes[offset + 3] << 24);
        }
    }

    /// <summary>
    /// Durable exact identity of one LMC_Home CurrentPositionZero request.
    /// Persist this key before sending the prepared command. A restart must
    /// use the read-only 0x7D18 query and must never replay 0x7D13.
    /// </summary>
    public sealed class LMCHomeRecoveryKey
        : IEquatable<LMCHomeRecoveryKey>
    {
        public LMCHomeRecoveryKey(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint originalDiagnosticsBootId,
            uint mapRevision,
            uint clientIntentId0,
            uint clientIntentId1,
            uint clientIntentId2,
            uint clientIntentId3,
            ushort axisReference,
            int expectedActualPosition,
            int timeoutMilliseconds)
            : this(
                schemaVersion,
                originalRequestId,
                diagnosticsBuild,
                originalDiagnosticsBootId,
                mapRevision,
                new LMCHomeClientIntentId(
                    clientIntentId0,
                    clientIntentId1,
                    clientIntentId2,
                    clientIntentId3),
                axisReference,
                expectedActualPosition,
                timeoutMilliseconds,
                LMCHomeSemanticMode.CurrentPositionZero)
        {
        }

        public LMCHomeRecoveryKey(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint originalDiagnosticsBootId,
            uint mapRevision,
            LMCHomeClientIntentId clientIntentId,
            ushort axisReference,
            int expectedActualPosition,
            int timeoutMilliseconds,
            LMCHomeSemanticMode semanticMode)
        {
            if (schemaVersion != LMCAdmin.ProtocolSchemaVersion)
            {
                throw new ArgumentOutOfRangeException(
                    "schemaVersion",
                    "Only LMC_Home recovery schema version 1 is supported.");
            }

            if (originalRequestId == 0)
            {
                throw new ArgumentOutOfRangeException("originalRequestId");
            }

            if (diagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBuild");
            }

            if (originalDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "originalDiagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            if (clientIntentId == null)
            {
                throw new ArgumentNullException("clientIntentId");
            }

            LMC_AdminFrame.ValidateAxisReference(axisReference);
            LMC_AdminFrame.ValidateLmcHome(
                semanticMode,
                timeoutMilliseconds);

            SchemaVersion = schemaVersion;
            OriginalRequestId = originalRequestId;
            DiagnosticsBuild = diagnosticsBuild;
            OriginalDiagnosticsBootId = originalDiagnosticsBootId;
            MapRevision = mapRevision;
            ClientIntentId = clientIntentId;
            AxisReference = axisReference;
            ExpectedActualPosition = expectedActualPosition;
            TimeoutMilliseconds = timeoutMilliseconds;
            SemanticMode = semanticMode;
        }

        public ushort SchemaVersion { get; private set; }
        public uint OriginalRequestId { get; private set; }
        public uint DiagnosticsBuild { get; private set; }
        public uint OriginalDiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCHomeClientIntentId ClientIntentId
        {
            get;
            private set;
        }
        public uint ClientIntentId0 { get { return ClientIntentId.Word0; } }
        public uint ClientIntentId1 { get { return ClientIntentId.Word1; } }
        public uint ClientIntentId2 { get { return ClientIntentId.Word2; } }
        public uint ClientIntentId3 { get { return ClientIntentId.Word3; } }
        public ushort AxisReference { get; private set; }
        public int ExpectedActualPosition { get; private set; }
        public int TargetPosition { get { return 0; } }
        public int TimeoutMilliseconds { get; private set; }
        public LMCHomeSemanticMode SemanticMode { get; private set; }

        public bool Equals(LMCHomeRecoveryKey other)
        {
            return other != null
                && SchemaVersion == other.SchemaVersion
                && OriginalRequestId == other.OriginalRequestId
                && DiagnosticsBuild == other.DiagnosticsBuild
                && OriginalDiagnosticsBootId
                    == other.OriginalDiagnosticsBootId
                && MapRevision == other.MapRevision
                && ClientIntentId.Equals(other.ClientIntentId)
                && AxisReference == other.AxisReference
                && ExpectedActualPosition == other.ExpectedActualPosition
                && TimeoutMilliseconds == other.TimeoutMilliseconds
                && SemanticMode == other.SemanticMode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCHomeRecoveryKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = SchemaVersion.GetHashCode();
                hash = (hash * 397) ^ OriginalRequestId.GetHashCode();
                hash = (hash * 397) ^ DiagnosticsBuild.GetHashCode();
                hash = (hash * 397)
                    ^ OriginalDiagnosticsBootId.GetHashCode();
                hash = (hash * 397) ^ MapRevision.GetHashCode();
                hash = (hash * 397) ^ ClientIntentId.GetHashCode();
                hash = (hash * 397) ^ AxisReference.GetHashCode();
                hash = (hash * 397) ^ ExpectedActualPosition;
                hash = (hash * 397) ^ TimeoutMilliseconds;
                hash = (hash * 397) ^ SemanticMode.GetHashCode();
                return hash;
            }
        }
    }

    /// <summary>
    /// One-use confirmation required to prepare LMC_Home. The fixed protocol
    /// token is never exposed as a caller-supplied integer.
    /// </summary>
    public sealed class LMCHomeExecuteToken
    {
        private int consumed;

        private LMCHomeExecuteToken()
        {
        }

        public static LMCHomeExecuteToken Create()
        {
            return new LMCHomeExecuteToken();
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
                    "This LMC_Home confirmation token has already prepared an intent and cannot be reused.");
            }
        }
    }

    /// <summary>
    /// Immutable one-shot LMC_Home request. It contains no search direction,
    /// velocity, travel, or switch parameter because the axis must not move.
    /// </summary>
    public sealed class LMCPreparedHome
    {
        private int consumed;

        internal LMCPreparedHome(
            LMCConnection connectionOwner,
            LMCSingleAxis axis,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long connectionSessionGeneration,
            LMCHomeRecoveryKey recoveryKey)
        {
            ConnectionOwner = connectionOwner;
            Axis = axis;
            VerifiedCapabilities = verifiedCapabilities;
            VerifiedDiagnosticCapabilities = verifiedDiagnosticCapabilities;
            ConnectionSessionGeneration = connectionSessionGeneration;
            RecoveryKey = recoveryKey;
        }

        public LMCHomeRecoveryKey RecoveryKey { get; private set; }
        public ushort SchemaVersion { get { return RecoveryKey.SchemaVersion; } }
        public uint RequestId { get { return RecoveryKey.OriginalRequestId; } }
        public uint DiagnosticsBuild { get { return RecoveryKey.DiagnosticsBuild; } }
        public uint OriginalDiagnosticsBootId
        {
            get { return RecoveryKey.OriginalDiagnosticsBootId; }
        }
        public uint MapRevision { get { return RecoveryKey.MapRevision; } }
        public LMCHomeClientIntentId ClientIntentId
        {
            get { return RecoveryKey.ClientIntentId; }
        }
        public ushort AxisReference { get { return RecoveryKey.AxisReference; } }
        public int ExpectedActualPosition
        {
            get { return RecoveryKey.ExpectedActualPosition; }
        }
        public int TargetPosition { get { return 0; } }
        public int TimeoutMilliseconds
        {
            get { return RecoveryKey.TimeoutMilliseconds; }
        }
        public LMCHomeSemanticMode SemanticMode
        {
            get { return RecoveryKey.SemanticMode; }
        }
        public long ConnectionSessionGeneration { get; private set; }

        public bool IsConsumed
        {
            get { return Volatile.Read(ref consumed) != 0; }
        }

        internal LMCConnection ConnectionOwner { get; private set; }
        internal LMCSingleAxis Axis { get; private set; }
        internal LMCAdminCapabilities VerifiedCapabilities { get; private set; }
        internal LMCDiagnosticCapabilities VerifiedDiagnosticCapabilities
        {
            get;
            private set;
        }

        internal void ThrowIfConsumed()
        {
            if (IsConsumed)
            {
                throw new InvalidOperationException(
                    "This prepared LMC_Home command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }

        internal void ConsumeAtWriteBoundary()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This prepared LMC_Home command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }
    }

    /// <summary>
    /// Correlated acceptance of LMC_Home. This is only a start ACK; terminal
    /// proof must be obtained with ReadLMC_HomeOutcome (0x7D18).
    /// </summary>
    public sealed class LMCHomeStartAcknowledgement
    {
        internal LMCHomeStartAcknowledgement(
            LMCAdminResponse response,
            LMCPreparedHome preparedCommand,
            LMCHomeSemanticMode semanticMode,
            uint nativeCommandState)
        {
            Response = response;
            PreparedCommand = preparedCommand;
            SemanticMode = semanticMode;
            NativeCommandState = nativeCommandState;
        }

        public LMCAdminResponse Response { get; private set; }
        public LMCPreparedHome PreparedCommand { get; private set; }
        public LMCHomeRecoveryKey RecoveryKey
        {
            get { return PreparedCommand.RecoveryKey; }
        }
        public uint RequestId { get { return RecoveryKey.OriginalRequestId; } }
        public ushort AxisReference { get { return RecoveryKey.AxisReference; } }
        public LMCHomeSemanticMode SemanticMode { get; private set; }
        public uint NativeCommandState { get; private set; }
        public bool IsAccepted
        {
            get { return Response != null && Response.IsSuccess; }
        }
    }

    public sealed class LMCHomeStartRejectedException
        : LMCAdminCommandException
    {
        internal LMCHomeStartRejectedException(
            LMCHomeStartAcknowledgement acknowledgement)
            : base(
                "LMC_Home was not accepted. ErrorId="
                    + acknowledgement.Response.ErrorId
                    + ", DetailCode="
                    + acknowledgement.Response.DetailCodeValue
                    + ". No terminal success is implied.",
                acknowledgement.Response)
        {
            Acknowledgement = acknowledgement;
        }

        public LMCHomeStartAcknowledgement Acknowledgement
        {
            get;
            private set;
        }
        public LMCHomeRecoveryKey RecoveryKey
        {
            get { return Acknowledgement.RecoveryKey; }
        }
    }

    public sealed class LMCHomeStartOutcomeUncertainException
        : InvalidOperationException
    {
        internal LMCHomeStartOutcomeUncertainException(
            LMCPreparedHome preparedCommand,
            Exception innerException)
            : base(
                "LMC_Home may have been accepted. Do not replay 0x7D13; reconnect and query the retained outcome with 0x7D18.",
                innerException)
        {
            PreparedCommand = preparedCommand;
        }

        public LMCPreparedHome PreparedCommand { get; private set; }
        public LMCHomeRecoveryKey RecoveryKey
        {
            get { return PreparedCommand.RecoveryKey; }
        }
    }

    public enum LMCHomeOutcomeRecordState : ushort
    {
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Aborted = 4,
        Quarantined = 5
    }

    internal static class LMCHomeOutcomeSemantics
    {
        internal const uint AxisStandstillMask = 0x02000000u;
        internal const uint RequiredEvidenceFlags = 0x0000003Bu;

        // RuntimePhase is intentionally absent. Wire v1 exposes it only as an
        // opaque diagnostic value and defines no numeric terminal phase.
        internal static bool IsSucceeded(
            bool responseSucceeded,
            LMCHomeOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint axisStatus,
            int axisError,
            int rawDrivePositionBefore,
            int rawDrivePositionAfter,
            int actualApplicationPositionAfter,
            int setApplicationPositionAfter,
            int actualInternalPositionAfter,
            int setInternalPositionAfter,
            int destinationInternalPositionAfter,
            int masterInternalPositionAfter,
            uint nativeCommandState,
            uint evidenceFlags,
            uint startMilliseconds,
            uint completionMilliseconds,
            uint stopState,
            uint recordGeneration)
        {
            return responseSucceeded
                && recordState == LMCHomeOutcomeRecordState.Succeeded
                && originalCommandStatus == 0
                && originalErrorId == 0
                && originalDetailCode == 0
                && (axisStatus & AxisStandstillMask) != 0
                && axisError == 0
                && actualApplicationPositionAfter == 0
                && setApplicationPositionAfter == 0
                && actualInternalPositionAfter == 0
                && setInternalPositionAfter == 0
                && destinationInternalPositionAfter == 0
                && masterInternalPositionAfter == 0
                && nativeCommandState == 0
                // Raw snapshots remain diagnostic-only in the temporary
                // SetPosition-only contract, so the RAW evidence bit is clear.
                && evidenceFlags == RequiredEvidenceFlags
                && startMilliseconds != 0
                && completionMilliseconds != 0
                // Current-position zero performs no motion and has no Stop.
                && stopState == 0
                && recordGeneration != 0;
        }
    }

    /// <summary>
    /// Exact retained LMC_Home runtime record. Only HomeSucceeded proves a
    /// completed CurrentPositionZero operation. Start ACK is insufficient.
    /// </summary>
    public sealed class LMCHomeOutcomeResult
    {
        internal LMCHomeOutcomeResult(
            LMCAdminResponse response,
            LMCHomeRecoveryKey recoveryKey,
            LMCHomeOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint axisStatus,
            int axisError,
            int rawDrivePositionBefore,
            int rawDrivePositionAfter,
            int actualApplicationPositionAfter,
            int setApplicationPositionAfter,
            int actualInternalPositionAfter,
            int setInternalPositionAfter,
            int destinationInternalPositionAfter,
            int masterInternalPositionAfter,
            uint nativeCommandState,
            uint evidenceFlags,
            uint startMilliseconds,
            uint completionMilliseconds,
            uint stopState,
            uint runtimePhase,
            uint recordGeneration)
        {
            Response = response;
            RecoveryKey = recoveryKey;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCodeValue = originalDetailCode;
            AxisStatus = axisStatus;
            AxisError = axisError;
            RawDrivePositionBefore = rawDrivePositionBefore;
            RawDrivePositionAfter = rawDrivePositionAfter;
            ActualApplicationPositionAfter = actualApplicationPositionAfter;
            SetApplicationPositionAfter = setApplicationPositionAfter;
            ActualInternalPositionAfter = actualInternalPositionAfter;
            SetInternalPositionAfter = setInternalPositionAfter;
            DestinationInternalPositionAfter =
                destinationInternalPositionAfter;
            MasterInternalPositionAfter = masterInternalPositionAfter;
            NativeCommandState = nativeCommandState;
            EvidenceFlags = evidenceFlags;
            StartMilliseconds = startMilliseconds;
            CompletionMilliseconds = completionMilliseconds;
            StopState = stopState;
            RuntimePhase = runtimePhase;
            RecordGeneration = recordGeneration;
        }

        public LMCAdminResponse Response { get; private set; }
        public uint QueryRequestId { get { return Response.RequestId; } }
        public LMCHomeRecoveryKey RecoveryKey { get; private set; }
        public LMCHomeOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        public ushort OriginalCommandStatus { get; private set; }
        public short OriginalErrorId { get; private set; }
        public uint OriginalDetailCodeValue { get; private set; }
        public LMCAdminDetailCode OriginalDetailCode
        {
            get { return (LMCAdminDetailCode)OriginalDetailCodeValue; }
        }
        public uint AxisStatus { get; private set; }
        public int AxisError { get; private set; }
        public int RawDrivePositionBefore { get; private set; }
        public int RawDrivePositionAfter { get; private set; }
        public int ActualApplicationPositionAfter { get; private set; }
        public int SetApplicationPositionAfter { get; private set; }
        public int ActualInternalPositionAfter { get; private set; }
        public int SetInternalPositionAfter { get; private set; }
        public int DestinationInternalPositionAfter { get; private set; }
        public int MasterInternalPositionAfter { get; private set; }
        public uint NativeCommandState { get; private set; }
        public uint EvidenceFlags { get; private set; }
        public uint StartMilliseconds { get; private set; }
        public uint CompletionMilliseconds { get; private set; }
        public uint StopState { get; private set; }
        public uint RuntimePhase { get; private set; }
        public uint RecordGeneration { get; private set; }

        public bool IsTerminal
        {
            get
            {
                return RecordState
                    != LMCHomeOutcomeRecordState.Running;
            }
        }

        public bool IsQuarantined
        {
            get
            {
                return RecordState
                    == LMCHomeOutcomeRecordState.Quarantined;
            }
        }

        public bool HomeSucceeded
        {
            get
            {
                return LMCHomeOutcomeSemantics.IsSucceeded(
                    Response != null && Response.IsSuccess,
                    RecordState,
                    OriginalCommandStatus,
                    OriginalErrorId,
                    OriginalDetailCodeValue,
                    AxisStatus,
                    AxisError,
                    RawDrivePositionBefore,
                    RawDrivePositionAfter,
                    ActualApplicationPositionAfter,
                    SetApplicationPositionAfter,
                    ActualInternalPositionAfter,
                    SetInternalPositionAfter,
                    DestinationInternalPositionAfter,
                    MasterInternalPositionAfter,
                    NativeCommandState,
                    EvidenceFlags,
                    StartMilliseconds,
                    CompletionMilliseconds,
                    StopState,
                    RecordGeneration);
            }
        }
    }

    public sealed class LMCHomeOutcomeRetirementResult
    {
        internal LMCHomeOutcomeRetirementResult(
            LMCHomeOutcomeResult terminalOutcome)
        {
            Outcome = terminalOutcome;
        }

        public LMCHomeOutcomeResult Outcome { get; private set; }
        public LMCAdminResponse Response { get { return Outcome.Response; } }
        public LMCHomeRecoveryKey RecoveryKey
        {
            get { return Outcome.RecoveryKey; }
        }
        public LMCHomeOutcomeRecordState RecordState
        {
            get { return Outcome.RecordState; }
        }
        public uint RecordGeneration { get { return Outcome.RecordGeneration; } }
        public bool RetirementConfirmed
        {
            get { return Outcome.IsTerminal && Response.IsSuccess; }
        }
    }

    public sealed class LMCHomeOutcomeQueryException
        : LMCAdminCommandException
    {
        internal LMCHomeOutcomeQueryException(
            LMCAdminResponse response,
            LMCHomeRecoveryKey recoveryKey)
            : base(
                "ReadLMC_HomeOutcome failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCodeValue
                    + ". The original LMC_Home outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
        }

        public LMCHomeRecoveryKey RecoveryKey { get; private set; }
    }

    public sealed class LMCHomeOutcomeRetirementException
        : LMCAdminCommandException
    {
        internal LMCHomeOutcomeRetirementException(
            LMCAdminResponse response,
            LMCHomeRecoveryKey recoveryKey,
            uint recordGeneration)
            : base(
                "RetireLMC_HomeOutcome failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCodeValue
                    + ". The retained outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
            RecordGeneration = recordGeneration;
        }

        public LMCHomeRecoveryKey RecoveryKey { get; private set; }
        public uint RecordGeneration { get; private set; }
    }
}

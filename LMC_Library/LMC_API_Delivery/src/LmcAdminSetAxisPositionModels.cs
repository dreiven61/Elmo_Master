using System;
using System.Security.Cryptography;
using System.Threading;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Stable LASAL-local semantic mode. This is not the native MotionLib enum.
    /// </summary>
    public enum LMCAxisSetPositionSemanticMode : ushort
    {
        ActualAndDestinationApplicationUnits = 1
    }

    /// <summary>
    /// Four raw U32 words that identify one SetAxisPosition intent. The words
    /// are serialized independently and do not use Guid byte-order semantics.
    /// </summary>
    public sealed class LMCAxisSetPositionClientIntentId
        : IEquatable<LMCAxisSetPositionClientIntentId>
    {
        public LMCAxisSetPositionClientIntentId(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            if (word0 == 0 && word1 == 0 && word2 == 0 && word3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit SetAxisPosition client intent identifier must not be all zero.");
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

        public static LMCAxisSetPositionClientIntentId Create()
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
                        return new LMCAxisSetPositionClientIntentId(
                            word0,
                            word1,
                            word2,
                            word3);
                    }
                }
            }
        }

        public bool Equals(LMCAxisSetPositionClientIntentId other)
        {
            return other != null
                && Word0 == other.Word0
                && Word1 == other.Word1
                && Word2 == other.Word2
                && Word3 == other.Word3;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCAxisSetPositionClientIntentId);
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
    /// Durable, process-independent identity for one SetAxisPosition request.
    /// Persist these values before executing the prepared mutation. The key can
    /// be reconstructed after restart and used only for an exact read-only
    /// outcome query.
    /// </summary>
    public sealed class LMCAxisSetPositionRecoveryKey
        : IEquatable<LMCAxisSetPositionRecoveryKey>
    {
        public LMCAxisSetPositionRecoveryKey(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            uint clientIntentId0,
            uint clientIntentId1,
            uint clientIntentId2,
            uint clientIntentId3,
            ushort axisReference,
            int targetPosition,
            int expectedActualPosition,
            LMCAxisSetPositionSemanticMode semanticMode)
            : this(
                schemaVersion,
                originalRequestId,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                new LMCAxisSetPositionClientIntentId(
                    clientIntentId0,
                    clientIntentId1,
                    clientIntentId2,
                    clientIntentId3),
                axisReference,
                targetPosition,
                expectedActualPosition,
                semanticMode)
        {
        }

        public LMCAxisSetPositionRecoveryKey(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCAxisSetPositionClientIntentId clientIntentId,
            ushort axisReference,
            int targetPosition,
            int expectedActualPosition,
            LMCAxisSetPositionSemanticMode semanticMode)
        {
            if (schemaVersion != LMCAdmin.ProtocolSchemaVersion)
            {
                throw new ArgumentOutOfRangeException(
                    "schemaVersion",
                    "Only SetAxisPosition recovery schema version 1 is supported.");
            }

            if (originalRequestId == 0)
            {
                throw new ArgumentOutOfRangeException("originalRequestId");
            }

            if (diagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBuild");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
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
            if (semanticMode
                != LMCAxisSetPositionSemanticMode
                    .ActualAndDestinationApplicationUnits)
            {
                throw new ArgumentOutOfRangeException("semanticMode");
            }

            SchemaVersion = schemaVersion;
            OriginalRequestId = originalRequestId;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            ClientIntentId = clientIntentId;
            AxisReference = axisReference;
            TargetPosition = targetPosition;
            ExpectedActualPosition = expectedActualPosition;
            SemanticMode = semanticMode;
        }

        public ushort SchemaVersion { get; private set; }
        public uint OriginalRequestId { get; private set; }
        public uint DiagnosticsBuild { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCAxisSetPositionClientIntentId ClientIntentId
        {
            get;
            private set;
        }
        public uint ClientIntentId0 { get { return ClientIntentId.Word0; } }
        public uint ClientIntentId1 { get { return ClientIntentId.Word1; } }
        public uint ClientIntentId2 { get { return ClientIntentId.Word2; } }
        public uint ClientIntentId3 { get { return ClientIntentId.Word3; } }
        public ushort AxisReference { get; private set; }
        public int TargetPosition { get; private set; }
        public int ExpectedActualPosition { get; private set; }
        public LMCAxisSetPositionSemanticMode SemanticMode { get; private set; }

        public bool Equals(LMCAxisSetPositionRecoveryKey other)
        {
            return other != null
                && SchemaVersion == other.SchemaVersion
                && OriginalRequestId == other.OriginalRequestId
                && DiagnosticsBuild == other.DiagnosticsBuild
                && DiagnosticsBootId == other.DiagnosticsBootId
                && MapRevision == other.MapRevision
                && ClientIntentId.Equals(other.ClientIntentId)
                && AxisReference == other.AxisReference
                && TargetPosition == other.TargetPosition
                && ExpectedActualPosition == other.ExpectedActualPosition
                && SemanticMode == other.SemanticMode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCAxisSetPositionRecoveryKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = SchemaVersion.GetHashCode();
                hash = (hash * 397) ^ OriginalRequestId.GetHashCode();
                hash = (hash * 397) ^ DiagnosticsBuild.GetHashCode();
                hash = (hash * 397) ^ DiagnosticsBootId.GetHashCode();
                hash = (hash * 397) ^ MapRevision.GetHashCode();
                hash = (hash * 397) ^ ClientIntentId.GetHashCode();
                hash = (hash * 397) ^ AxisReference.GetHashCode();
                hash = (hash * 397) ^ TargetPosition;
                hash = (hash * 397) ^ ExpectedActualPosition;
                hash = (hash * 397) ^ SemanticMode.GetHashCode();
                return hash;
            }
        }
    }

    /// <summary>
    /// Explicit confirmation required when preparing a SetAxisPosition command.
    /// The protocol value is fixed and is never accepted as a caller-supplied integer.
    /// </summary>
    public sealed class LMCAxisSetPositionExecuteToken
    {
        private int consumed;

        private LMCAxisSetPositionExecuteToken()
        {
        }

        public static LMCAxisSetPositionExecuteToken Create()
        {
            return new LMCAxisSetPositionExecuteToken();
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
                    "This SetAxisPosition confirmation token has already prepared an intent and cannot be reused.");
            }
        }
    }

    /// <summary>
    /// Immutable SetAxisPosition request context with a one-shot execution latch.
    /// The latch is consumed only at the final pre-write boundary and is never reset.
    /// </summary>
    public sealed class LMCPreparedAxisSetPosition
    {
        private int consumed;

        internal LMCPreparedAxisSetPosition(
            LMCConnection connectionOwner,
            LMCSingleAxis axis,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long connectionSessionGeneration,
            LMCAxisSetPositionRecoveryKey recoveryKey)
        {
            ConnectionOwner = connectionOwner;
            Axis = axis;
            VerifiedCapabilities = verifiedCapabilities;
            VerifiedDiagnosticCapabilities = verifiedDiagnosticCapabilities;
            ConnectionSessionGeneration = connectionSessionGeneration;
            RecoveryKey = recoveryKey;
        }

        public LMCAxisSetPositionRecoveryKey RecoveryKey { get; private set; }
        public ushort SchemaVersion { get { return RecoveryKey.SchemaVersion; } }
        public uint RequestId { get { return RecoveryKey.OriginalRequestId; } }
        public uint OriginalRequestId { get { return RecoveryKey.OriginalRequestId; } }
        public uint DiagnosticsBuild { get { return RecoveryKey.DiagnosticsBuild; } }
        public uint DiagnosticsBootId { get { return RecoveryKey.DiagnosticsBootId; } }
        public uint MapRevision { get { return RecoveryKey.MapRevision; } }
        public LMCAxisSetPositionClientIntentId ClientIntentId
        {
            get { return RecoveryKey.ClientIntentId; }
        }
        public uint ClientIntentId0 { get { return RecoveryKey.ClientIntentId0; } }
        public uint ClientIntentId1 { get { return RecoveryKey.ClientIntentId1; } }
        public uint ClientIntentId2 { get { return RecoveryKey.ClientIntentId2; } }
        public uint ClientIntentId3 { get { return RecoveryKey.ClientIntentId3; } }
        public ushort AxisReference { get { return RecoveryKey.AxisReference; } }
        public int TargetPosition { get { return RecoveryKey.TargetPosition; } }
        public int ExpectedActualPosition
        {
            get { return RecoveryKey.ExpectedActualPosition; }
        }
        public LMCAxisSetPositionSemanticMode SemanticMode
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
                    "This prepared SetAxisPosition command has already crossed its one-shot execution boundary and cannot be replayed.");
            }
        }

        internal void ConsumeAtWriteBoundary()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This prepared SetAxisPosition command has already crossed its one-shot execution boundary and cannot be replayed.");
            }
        }
    }

    public sealed class LMCAxisSetPositionResult
    {
        internal LMCAxisSetPositionResult(
            LMCAdminResponse response,
            LMCPreparedAxisSetPosition preparedCommand,
            int appliedPosition,
            LMCAxisSetPositionSemanticMode semanticMode,
            uint nativeCommandState)
        {
            Response = response;
            PreparedCommand = preparedCommand;
            RequestId = preparedCommand.RequestId;
            AxisReference = preparedCommand.AxisReference;
            TargetPosition = preparedCommand.TargetPosition;
            ExpectedActualPosition = preparedCommand.ExpectedActualPosition;
            RecoveryKey = preparedCommand.RecoveryKey;
            AppliedPosition = appliedPosition;
            SemanticMode = semanticMode;
            NativeCommandState = nativeCommandState;
        }

        public LMCAdminResponse Response { get; private set; }
        public LMCPreparedAxisSetPosition PreparedCommand { get; private set; }
        public LMCAxisSetPositionRecoveryKey RecoveryKey { get; private set; }
        public uint RequestId { get; private set; }
        public ushort AxisReference { get; private set; }
        public int TargetPosition { get; private set; }
        public int ExpectedActualPosition { get; private set; }
        public int AppliedPosition { get; private set; }
        public LMCAxisSetPositionSemanticMode SemanticMode { get; private set; }
        public uint NativeCommandState { get; private set; }

        public bool IsSuccess
        {
            get { return Response != null && Response.IsSuccess; }
        }
    }

    /// <summary>
    /// The PLC returned a correlated, schema-valid SetAxisPosition rejection.
    /// The full native command state is preserved in Result.
    /// </summary>
    public sealed class LMCAxisSetPositionRejectedException
        : LMCAdminCommandException
    {
        internal LMCAxisSetPositionRejectedException(
            LMCAxisSetPositionResult result)
            : base(
                "SetAxisPosition failed. ErrorId="
                    + result.Response.ErrorId
                    + ", DetailCode="
                    + result.Response.DetailCodeValue
                    + ", NativeCommandState=0x"
                    + result.NativeCommandState.ToString("X8")
                    + ".",
                result.Response)
        {
            Result = result;
        }

        public LMCAxisSetPositionResult Result { get; private set; }
        public LMCPreparedAxisSetPosition PreparedCommand
        {
            get { return Result.PreparedCommand; }
        }
        public LMCAxisSetPositionRecoveryKey RecoveryKey
        {
            get { return Result.RecoveryKey; }
        }
        public uint RequestId
        {
            get { return Result.RequestId; }
        }
        public ushort AxisReference
        {
            get { return Result.AxisReference; }
        }
        public int AppliedPosition
        {
            get { return Result.AppliedPosition; }
        }
        public LMCAxisSetPositionSemanticMode SemanticMode
        {
            get { return Result.SemanticMode; }
        }
        public uint NativeCommandState
        {
            get { return Result.NativeCommandState; }
        }
    }

    /// <summary>
    /// The command crossed its one-shot write boundary, but the SDK could not
    /// publish a definitive response. The prepared command remains consumed.
    /// </summary>
    public sealed class LMCAxisSetPositionOutcomeUncertainException
        : InvalidOperationException
    {
        internal LMCAxisSetPositionOutcomeUncertainException(
            LMCPreparedAxisSetPosition preparedCommand,
            Exception innerException)
            : base(
                "SetAxisPosition may have been applied. Its one-shot prepared command is consumed and must not be replayed.",
                innerException)
        {
            PreparedCommand = preparedCommand;
            RequestId = preparedCommand.RequestId;
        }

        public LMCPreparedAxisSetPosition PreparedCommand { get; private set; }
        public LMCAxisSetPositionRecoveryKey RecoveryKey
        {
            get { return PreparedCommand.RecoveryKey; }
        }
        public uint RequestId { get; private set; }
        public ushort AxisReference
        {
            get { return PreparedCommand.AxisReference; }
        }
        public int TargetPosition
        {
            get { return PreparedCommand.TargetPosition; }
        }
        public int ExpectedActualPosition
        {
            get { return PreparedCommand.ExpectedActualPosition; }
        }
        public LMCAxisSetPositionSemanticMode SemanticMode
        {
            get { return PreparedCommand.SemanticMode; }
        }
    }
}

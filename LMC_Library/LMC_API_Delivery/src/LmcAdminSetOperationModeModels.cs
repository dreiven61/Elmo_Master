using System;
using System.Security.Cryptography;
using System.Threading;

namespace LasalMotionControlLib
{
    public sealed class LMCAxisSetOperationModeClientIntentId
        : IEquatable<LMCAxisSetOperationModeClientIntentId>
    {
        public LMCAxisSetOperationModeClientIntentId(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            if (word0 == 0 && word1 == 0 && word2 == 0 && word3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit SetOperationMode client intent identifier must not be all zero.");
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

        public static LMCAxisSetOperationModeClientIntentId Create()
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
                        return new LMCAxisSetOperationModeClientIntentId(
                            word0,
                            word1,
                            word2,
                            word3);
                    }
                }
            }
        }

        public bool Equals(LMCAxisSetOperationModeClientIntentId other)
        {
            return other != null
                && Word0 == other.Word0
                && Word1 == other.Word1
                && Word2 == other.Word2
                && Word3 == other.Word3;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCAxisSetOperationModeClientIntentId);
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
    /// Durable identity for one Immediate-only CSP recovery request. Persist
    /// this exact key before Start and never replay Start after an uncertain
    /// response. Use the read-only outcome command instead.
    /// </summary>
    public sealed class LMCAxisSetOperationModeRecoveryKey
        : IEquatable<LMCAxisSetOperationModeRecoveryKey>
    {
        public LMCAxisSetOperationModeRecoveryKey(
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
            LMCDriveOperationMode requestedMode,
            uint timeoutMilliseconds)
            : this(
                schemaVersion,
                originalRequestId,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                new LMCAxisSetOperationModeClientIntentId(
                    clientIntentId0,
                    clientIntentId1,
                    clientIntentId2,
                    clientIntentId3),
                axisReference,
                requestedMode,
                timeoutMilliseconds)
        {
        }

        public LMCAxisSetOperationModeRecoveryKey(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCAxisSetOperationModeClientIntentId clientIntentId,
            ushort axisReference,
            LMCDriveOperationMode requestedMode,
            uint timeoutMilliseconds)
        {
            if (schemaVersion != LMCAdmin.ProtocolSchemaVersion)
            {
                throw new ArgumentOutOfRangeException("schemaVersion");
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
            LMC_AdminFrame.ValidateSetOperationModeRequest(
                requestedMode,
                timeoutMilliseconds,
                0);

            SchemaVersion = schemaVersion;
            OriginalRequestId = originalRequestId;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            ClientIntentId = clientIntentId;
            AxisReference = axisReference;
            RequestedMode = requestedMode;
            TimeoutMilliseconds = timeoutMilliseconds;
        }

        public ushort SchemaVersion { get; private set; }
        public uint OriginalRequestId { get; private set; }
        public uint DiagnosticsBuild { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCAxisSetOperationModeClientIntentId ClientIntentId
        {
            get;
            private set;
        }
        public uint ClientIntentId0 { get { return ClientIntentId.Word0; } }
        public uint ClientIntentId1 { get { return ClientIntentId.Word1; } }
        public uint ClientIntentId2 { get { return ClientIntentId.Word2; } }
        public uint ClientIntentId3 { get { return ClientIntentId.Word3; } }
        public ushort AxisReference { get; private set; }
        public LMCDriveOperationMode RequestedMode { get; private set; }
        public sbyte RequestedModeRaw { get { return (sbyte)RequestedMode; } }
        public uint TimeoutMilliseconds { get; private set; }
        public uint Flags { get { return 0; } }

        public bool Equals(LMCAxisSetOperationModeRecoveryKey other)
        {
            return other != null
                && SchemaVersion == other.SchemaVersion
                && OriginalRequestId == other.OriginalRequestId
                && DiagnosticsBuild == other.DiagnosticsBuild
                && DiagnosticsBootId == other.DiagnosticsBootId
                && MapRevision == other.MapRevision
                && ClientIntentId.Equals(other.ClientIntentId)
                && AxisReference == other.AxisReference
                && RequestedMode == other.RequestedMode
                && TimeoutMilliseconds == other.TimeoutMilliseconds;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCAxisSetOperationModeRecoveryKey);
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
                hash = (hash * 397) ^ RequestedMode.GetHashCode();
                hash = (hash * 397) ^ TimeoutMilliseconds.GetHashCode();
                return hash;
            }
        }
    }

    public sealed class LMCAxisSetOperationModeExecuteToken
    {
        private int consumed;

        private LMCAxisSetOperationModeExecuteToken()
        {
        }

        public static LMCAxisSetOperationModeExecuteToken Create()
        {
            return new LMCAxisSetOperationModeExecuteToken();
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
                    "This SetOperationMode confirmation token has already prepared an intent.");
            }
        }
    }

    public sealed class LMCPreparedAxisSetOperationMode
    {
        private int consumed;

        internal LMCPreparedAxisSetOperationMode(
            LMCConnection connectionOwner,
            LMCSingleAxis axis,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long connectionSessionGeneration,
            LMCAxisSetOperationModeRecoveryKey recoveryKey)
        {
            ConnectionOwner = connectionOwner;
            Axis = axis;
            VerifiedCapabilities = verifiedCapabilities;
            VerifiedDiagnosticCapabilities = verifiedDiagnosticCapabilities;
            ConnectionSessionGeneration = connectionSessionGeneration;
            RecoveryKey = recoveryKey;
        }

        public LMCAxisSetOperationModeRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public uint RequestId { get { return RecoveryKey.OriginalRequestId; } }
        public ushort AxisReference { get { return RecoveryKey.AxisReference; } }
        public LMCDriveOperationMode RequestedMode
        {
            get { return RecoveryKey.RequestedMode; }
        }
        public uint TimeoutMilliseconds
        {
            get { return RecoveryKey.TimeoutMilliseconds; }
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
                    "This prepared SetOperationMode command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }

        internal void ConsumeAtWriteBoundary()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This prepared SetOperationMode command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }
    }

    public sealed class LMCAxisSetOperationModeStartAcknowledgement
    {
        internal LMCAxisSetOperationModeStartAcknowledgement(
            LMCAdminResponse response,
            LMCPreparedAxisSetOperationMode preparedCommand,
            LMCDriveOperationMode requestedMode,
            uint nativeCommandState)
        {
            Response = response;
            PreparedCommand = preparedCommand;
            RequestedMode = requestedMode;
            NativeCommandState = nativeCommandState;
        }

        public LMCAdminResponse Response { get; private set; }
        public LMCPreparedAxisSetOperationMode PreparedCommand
        {
            get;
            private set;
        }
        public LMCDriveOperationMode RequestedMode { get; private set; }
        public uint NativeCommandState { get; private set; }
        public bool IsAccepted
        {
            get { return Response != null && Response.IsSuccess; }
        }
    }

    public sealed class LMCAxisSetOperationModeRejectedException
        : LMCAdminCommandException
    {
        internal LMCAxisSetOperationModeRejectedException(
            LMCAxisSetOperationModeStartAcknowledgement acknowledgement)
            : base(
                "StartAxisSetOperationMode was rejected. This is not mode-change completion evidence.",
                acknowledgement.Response)
        {
            Acknowledgement = acknowledgement;
        }

        public LMCAxisSetOperationModeStartAcknowledgement Acknowledgement
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisSetOperationModeOutcomeUncertainException
        : InvalidOperationException
    {
        internal LMCAxisSetOperationModeOutcomeUncertainException(
            LMCPreparedAxisSetOperationMode preparedCommand,
            Exception innerException)
            : base(
                "SetOperationMode may have been accepted. Never replay Start; resolve the exact recovery key with the read-only outcome command.",
                innerException)
        {
            PreparedCommand = preparedCommand;
            RecoveryKey = preparedCommand.RecoveryKey;
        }

        public LMCPreparedAxisSetOperationMode PreparedCommand
        {
            get;
            private set;
        }
        public LMCAxisSetOperationModeRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCDs402HomeBufferMode : ushort
    {
        Aborting = 1,
        Buffered = 2,
        BlendingLow = 3,
        BlendingPrevious = 4,
        BlendingNext = 5,
        BlendingHigh = 6
    }

    /// <summary>
    /// Typed LMC_HomeDS402 inputs expressed as LASAL application-unit DINTs.
    /// This product surface implements only the non-moving DS402 method 37
    /// current-position-zero operation.
    /// </summary>
    public sealed class LMCAxisDs402HomeParameters
    {
        public const int CurrentPositionZeroHomingMethod = 37;
        public const int CurrentPositionZeroHomeOffset = 0;

        /// <summary>
        /// Creates the only supported LMC_HomeDS402 operation: take the
        /// sensor-reported current position as Home and expose it as zero.
        /// Method 37 does not seek a Home or limit switch.
        /// </summary>
        public LMCAxisDs402HomeParameters(uint timeoutMilliseconds)
            : this(
                CurrentPositionZeroHomingMethod,
                CurrentPositionZeroHomeOffset,
                0,
                0,
                0,
                0,
                LMCDs402HomeBufferMode.Aborting,
                timeoutMilliseconds)
        {
        }

        /// <summary>
        /// Compatibility overload. Method 37 is non-moving, so all legacy
        /// dynamics and limit inputs must be zero and buffer mode must be
        /// Aborting.
        /// </summary>
        public LMCAxisDs402HomeParameters(
            int velocity,
            int acceleration,
            int distanceLimit,
            int torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            uint timeoutMilliseconds)
            : this(
                CurrentPositionZeroHomingMethod,
                CurrentPositionZeroHomeOffset,
                velocity,
                acceleration,
                distanceLimit,
                torqueLimit,
                bufferMode,
                timeoutMilliseconds)
        {
        }

        /// <summary>
        /// Compatibility overload. It fails closed unless homingMethod is 37
        /// and position, the DS402 0x607C Home offset, is zero.
        /// </summary>
        public LMCAxisDs402HomeParameters(
            int homingMethod,
            int position,
            int velocity,
            int acceleration,
            int distanceLimit,
            int torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            uint timeoutMilliseconds)
        {
            LMC_AdminFrame.ValidateAxisDs402HomeParameters(
                homingMethod,
                position,
                velocity,
                acceleration,
                distanceLimit,
                torqueLimit,
                bufferMode,
                timeoutMilliseconds);
            HomingMethod = homingMethod;
            Position = position;
            Velocity = velocity;
            Acceleration = acceleration;
            DistanceLimit = distanceLimit;
            TorqueLimit = torqueLimit;
            BufferMode = bufferMode;
            TimeoutMilliseconds = timeoutMilliseconds;
        }

        public int HomingMethod { get; private set; }
        /// <summary>
        /// DS402 object 0x607C Home offset. Method 37 defines the completed
        /// position actual value (0x6064) as this value. This surface fixes it
        /// to zero.
        /// </summary>
        public int Position { get; private set; }
        public int Velocity { get; private set; }
        public int Acceleration { get; private set; }
        public int DistanceLimit { get; private set; }
        public int TorqueLimit { get; private set; }
        public LMCDs402HomeBufferMode BufferMode { get; private set; }
        /// <summary>
        /// Overall PLC-side homing watchdog. This is not a single SDO
        /// TimeoutCycles value; every individual SDO remains bounded by the
        /// executor's separate 1..60000-cycle contract.
        /// </summary>
        public uint TimeoutMilliseconds { get; private set; }
    }

    public sealed class LMCAxisDs402HomeClientIntentId
        : IEquatable<LMCAxisDs402HomeClientIntentId>
    {
        public LMCAxisDs402HomeClientIntentId(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            if (word0 == 0 && word1 == 0 && word2 == 0 && word3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit DS402 Home client intent identifier must not be all zero.");
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

        public static LMCAxisDs402HomeClientIntentId Create()
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
                        return new LMCAxisDs402HomeClientIntentId(
                            words[0],
                            words[1],
                            words[2],
                            words[3]);
                    }
                }
            }
        }

        public bool Equals(LMCAxisDs402HomeClientIntentId other)
        {
            return other != null
                && Word0 == other.Word0
                && Word1 == other.Word1
                && Word2 == other.Word2
                && Word3 == other.Word3;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCAxisDs402HomeClientIntentId);
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
    }

    /// <summary>
    /// Durable identity to persist before sending DS402 Home. It does not
    /// authorize replay. Use only the exact 0x7D16 read-only outcome query to
    /// resolve an uncertain start after reconnecting to the same PLC identity.
    /// </summary>
    public sealed class LMCAxisDs402HomeRecoveryKey
    {
        public LMCAxisDs402HomeRecoveryKey(
            ushort schemaVersion,
            uint requestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCAxisDs402HomeClientIntentId clientIntentId,
            ushort axisReference,
            LMCAxisDs402HomeParameters parameters)
        {
            if (schemaVersion != LMCAdmin.ProtocolSchemaVersion)
            {
                throw new ArgumentOutOfRangeException("schemaVersion");
            }

            if (requestId == 0
                || diagnosticsBuild == 0
                || diagnosticsBootId == 0
                || mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "requestId",
                    "RequestId and diagnostics identity values must be non-zero.");
            }

            if (clientIntentId == null)
            {
                throw new ArgumentNullException("clientIntentId");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            LMC_AdminFrame.ValidateAxisReference(axisReference);
            SchemaVersion = schemaVersion;
            RequestId = requestId;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            ClientIntentId = clientIntentId;
            AxisReference = axisReference;
            Parameters = parameters;
        }

        public ushort SchemaVersion { get; private set; }
        public uint RequestId { get; private set; }
        public uint DiagnosticsBuild { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCAxisDs402HomeClientIntentId ClientIntentId
        {
            get;
            private set;
        }
        public ushort AxisReference { get; private set; }
        public LMCAxisDs402HomeParameters Parameters { get; private set; }
    }

    public sealed class LMCAxisDs402HomeExecuteToken
    {
        private int consumed;

        private LMCAxisDs402HomeExecuteToken()
        {
        }

        public static LMCAxisDs402HomeExecuteToken Create()
        {
            return new LMCAxisDs402HomeExecuteToken();
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
                    "This DS402 Home confirmation token has already prepared an intent.");
            }
        }
    }

    public sealed class LMCPreparedAxisDs402Home
    {
        private int consumed;

        internal LMCPreparedAxisDs402Home(
            LMCConnection connectionOwner,
            LMCSingleAxis axis,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long connectionSessionGeneration,
            LMCAxisDs402HomeRecoveryKey recoveryKey)
        {
            ConnectionOwner = connectionOwner;
            Axis = axis;
            VerifiedCapabilities = verifiedCapabilities;
            VerifiedDiagnosticCapabilities = verifiedDiagnosticCapabilities;
            ConnectionSessionGeneration = connectionSessionGeneration;
            RecoveryKey = recoveryKey;
        }

        public LMCAxisDs402HomeRecoveryKey RecoveryKey { get; private set; }
        public uint RequestId { get { return RecoveryKey.RequestId; } }
        public ushort AxisReference { get { return RecoveryKey.AxisReference; } }
        public LMCAxisDs402HomeParameters Parameters
        {
            get { return RecoveryKey.Parameters; }
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
                    "This prepared DS402 Home command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }

        internal void ConsumeAtWriteBoundary()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This prepared DS402 Home command has crossed its one-shot write boundary and cannot be replayed.");
            }
        }
    }

    public sealed class LMCAxisDs402HomeStartAcknowledgement
    {
        internal LMCAxisDs402HomeStartAcknowledgement(
            LMCAdminResponse response,
            LMCPreparedAxisDs402Home preparedCommand,
            int homingMethod,
            uint nativeCommandState)
        {
            Response = response;
            PreparedCommand = preparedCommand;
            HomingMethod = homingMethod;
            NativeCommandState = nativeCommandState;
        }

        public LMCAdminResponse Response { get; private set; }
        public LMCPreparedAxisDs402Home PreparedCommand { get; private set; }
        public int HomingMethod { get; private set; }
        public uint NativeCommandState { get; private set; }
        public bool IsAccepted
        {
            get { return Response != null && Response.IsSuccess; }
        }
    }

    public sealed class LMCAxisDs402HomeRejectedException
        : LMCAdminCommandException
    {
        internal LMCAxisDs402HomeRejectedException(
            LMCAxisDs402HomeStartAcknowledgement acknowledgement)
            : base(
                "StartAxisDs402Home was rejected. This is not homing completion evidence.",
                acknowledgement.Response)
        {
            Acknowledgement = acknowledgement;
        }

        public LMCAxisDs402HomeStartAcknowledgement Acknowledgement
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisDs402HomeOutcomeUncertainException
        : InvalidOperationException
    {
        internal LMCAxisDs402HomeOutcomeUncertainException(
            LMCPreparedAxisDs402Home preparedCommand,
            Exception innerException)
            : base(
                "DS402 Home may have been accepted. Persist the recovery key, never replay the prepared command, and verify homing state separately.",
                innerException)
        {
            PreparedCommand = preparedCommand;
            RecoveryKey = preparedCommand.RecoveryKey;
        }

        public LMCPreparedAxisDs402Home PreparedCommand { get; private set; }
        public LMCAxisDs402HomeRecoveryKey RecoveryKey { get; private set; }
    }
}

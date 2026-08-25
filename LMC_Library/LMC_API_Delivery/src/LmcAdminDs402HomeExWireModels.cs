using System;
using System.Security.Cryptography;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Exact 128-bit identity for one HomeDS402Ex Start intent. The intent is
    /// durable identity only; it never authorizes replay of an uncertain Start.
    /// </summary>
    public sealed class LMCAxisDs402HomeExClientIntentId
        : IEquatable<LMCAxisDs402HomeExClientIntentId>
    {
        public LMCAxisDs402HomeExClientIntentId(
            uint word0,
            uint word1,
            uint word2,
            uint word3)
        {
            if (word0 == 0 && word1 == 0 && word2 == 0 && word3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit HomeDS402Ex client intent identifier must not be all zero.");
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

        internal static LMCAxisDs402HomeExClientIntentId Create()
        {
            var bytes = new byte[16];
            using (var random = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    random.GetBytes(bytes);
                    var word0 = ReadWord(bytes, 0);
                    var word1 = ReadWord(bytes, 4);
                    var word2 = ReadWord(bytes, 8);
                    var word3 = ReadWord(bytes, 12);
                    if (word0 != 0 || word1 != 0 || word2 != 0 || word3 != 0)
                    {
                        return new LMCAxisDs402HomeExClientIntentId(
                            word0,
                            word1,
                            word2,
                            word3);
                    }
                }
            }
        }

        public bool Equals(LMCAxisDs402HomeExClientIntentId other)
        {
            return other != null
                && Word0 == other.Word0
                && Word1 == other.Word1
                && Word2 == other.Word2
                && Word3 == other.Word3;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LMCAxisDs402HomeExClientIntentId);
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

        private static uint ReadWord(byte[] bytes, int offset)
        {
            return (uint)bytes[offset]
                | ((uint)bytes[offset + 1] << 8)
                | ((uint)bytes[offset + 2] << 16)
                | ((uint)bytes[offset + 3] << 24);
        }
    }

    /// <summary>
    /// Frozen integer HomeDS402Ex execution plan after engineering-unit scale,
    /// rounding, range and allowlist validation. The constructor is internal so
    /// callers cannot bypass the future approved axis-profile Prepare stage.
    /// </summary>
    public sealed class LMCAxisDs402HomeExExecutionPlan
    {
        public const int SpareLength = 32;
        private readonly byte[] spare;

        internal LMCAxisDs402HomeExExecutionPlan(
            int homingMethod,
            int position,
            int detectionVelocityLimit,
            int acceleration,
            int velocityHigh,
            int velocityLow,
            int distanceLimit,
            int torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            uint overallTimeoutMilliseconds,
            uint detectionTimeoutMilliseconds,
            byte[] spare)
        {
            if (LMCAxisDs402HomeExParameters.ClassifyHomingMethod(homingMethod)
                != LMCDs402HomeExMethodClassification.StandardCandidate)
            {
                throw new NotSupportedException(
                    "HomeDS402Ex wire plan accepts only standard-method candidates.");
            }

            if (bufferMode != LMCDs402HomeBufferMode.Aborting)
            {
                throw new NotSupportedException(
                    "HomeDS402Ex schema version 1 is Aborting-only.");
            }

            if (overallTimeoutMilliseconds == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "overallTimeoutMilliseconds");
            }

            if (detectionTimeoutMilliseconds == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "detectionTimeoutMilliseconds");
            }

            if (spare == null)
            {
                throw new ArgumentNullException("spare");
            }

            if (spare.Length != SpareLength)
            {
                throw new ArgumentException(
                    "HomeDS402Ex wire plan requires exactly 32 spare bytes.",
                    "spare");
            }

            for (var index = 0; index < spare.Length; index++)
            {
                if (spare[index] != 0)
                {
                    throw new ArgumentException(
                        "HomeDS402Ex wire plan spare bytes must all be zero.",
                        "spare");
                }
            }

            HomingMethod = homingMethod;
            Position = position;
            DetectionVelocityLimit = detectionVelocityLimit;
            Acceleration = acceleration;
            VelocityHigh = velocityHigh;
            VelocityLow = velocityLow;
            DistanceLimit = distanceLimit;
            TorqueLimit = torqueLimit;
            BufferMode = bufferMode;
            OverallTimeoutMilliseconds = overallTimeoutMilliseconds;
            DetectionTimeoutMilliseconds = detectionTimeoutMilliseconds;
            this.spare = (byte[])spare.Clone();
        }

        public int HomingMethod { get; private set; }
        public int Position { get; private set; }
        public int DetectionVelocityLimit { get; private set; }
        public int Acceleration { get; private set; }
        public int VelocityHigh { get; private set; }
        public int VelocityLow { get; private set; }
        public int DistanceLimit { get; private set; }
        public int TorqueLimit { get; private set; }
        public LMCDs402HomeBufferMode BufferMode { get; private set; }
        public uint OverallTimeoutMilliseconds { get; private set; }
        public uint DetectionTimeoutMilliseconds { get; private set; }
        public byte[] Spare { get { return (byte[])spare.Clone(); } }
    }

    /// <summary>
    /// Full exact identity for one HomeDS402Ex outcome lifecycle. Query and
    /// Retire use this key but never execute Home or replay its parameter writes.
    /// </summary>
    public sealed class LMCAxisDs402HomeExRecoveryKey
    {
        internal LMCAxisDs402HomeExRecoveryKey(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCAxisDs402HomeExClientIntentId clientIntentId,
            ushort axisReference,
            LMCAxisDs402HomeExExecutionPlan executionPlan)
        {
            if (schemaVersion != LMCAdmin.ProtocolSchemaVersion)
            {
                throw new ArgumentOutOfRangeException("schemaVersion");
            }

            if (originalRequestId == 0
                || diagnosticsBuild == 0
                || diagnosticsBootId == 0
                || mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "originalRequestId",
                    "HomeDS402Ex RequestId and diagnostics identity must be nonzero.");
            }

            if (clientIntentId == null)
            {
                throw new ArgumentNullException("clientIntentId");
            }

            if (executionPlan == null)
            {
                throw new ArgumentNullException("executionPlan");
            }

            LMC_AdminFrame.ValidateAxisReference(axisReference);
            SchemaVersion = schemaVersion;
            OriginalRequestId = originalRequestId;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            ClientIntentId = clientIntentId;
            AxisReference = axisReference;
            ExecutionPlan = executionPlan;
        }

        public ushort SchemaVersion { get; private set; }
        public uint OriginalRequestId { get; private set; }
        public uint DiagnosticsBuild { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCAxisDs402HomeExClientIntentId ClientIntentId { get; private set; }
        public ushort AxisReference { get; private set; }
        public LMCAxisDs402HomeExExecutionPlan ExecutionPlan { get; private set; }
    }
}

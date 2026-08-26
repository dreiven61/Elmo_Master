using System;
using System.Collections.Generic;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Source-level classification for a HomeDS402Ex method candidate.
    /// This classification does not advertise PLC or drive support.
    /// </summary>
    public enum LMCDs402HomeExMethodClassification
    {
        Unsupported = 0,
        StandardCandidate = 1,
        GoldDriveQualificationRequired = 2,
        Reserved = 3,
        Obsolete = 4
    }

    /// <summary>
    /// Typed HomeDS402Ex engineering-unit inputs retained before a wire or
    /// PLC execution contract exists. Construction is intentionally limited
    /// to conservative standard method candidates. Defined buffer values are
    /// retained without claiming that a PLC or drive supports them.
    /// </summary>
    public sealed class LMCAxisDs402HomeExParameters
    {
        public const int SpareLength = 32;

        private readonly byte[] spare;

        public LMCAxisDs402HomeExParameters(
            double position,
            double detectionVelocityLimit,
            float acceleration,
            float velocityHigh,
            float velocityLow,
            float distanceLimit,
            float torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            int homingMethod,
            uint timeLimitMilliseconds,
            uint detectionTimeLimitMilliseconds)
            : this(
                position,
                detectionVelocityLimit,
                acceleration,
                velocityHigh,
                velocityLow,
                distanceLimit,
                torqueLimit,
                bufferMode,
                homingMethod,
                timeLimitMilliseconds,
                detectionTimeLimitMilliseconds,
                new byte[SpareLength])
        {
        }

        public LMCAxisDs402HomeExParameters(
            double position,
            double detectionVelocityLimit,
            float acceleration,
            float velocityHigh,
            float velocityLow,
            float distanceLimit,
            float torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            int homingMethod,
            uint timeLimitMilliseconds,
            uint detectionTimeLimitMilliseconds,
            byte[] spare)
        {
            ValidateFinite(position, "position");
            ValidateFinite(
                detectionVelocityLimit,
                "detectionVelocityLimit");
            ValidatePositive(acceleration, "acceleration");
            ValidatePositive(velocityHigh, "velocityHigh");
            ValidatePositive(velocityLow, "velocityLow");
            ValidateFinite(distanceLimit, "distanceLimit");
            ValidateNonNegative(torqueLimit, "torqueLimit");

            if (!Enum.IsDefined(
                typeof(LMCDs402HomeBufferMode),
                bufferMode))
            {
                throw new ArgumentOutOfRangeException("bufferMode");
            }

            var methodClassification = ClassifyHomingMethod(homingMethod);
            if (methodClassification
                != LMCDs402HomeExMethodClassification.StandardCandidate)
            {
                throw new NotSupportedException(
                    "HomeDS402Ex homing method "
                    + homingMethod
                    + " is not admitted by the dormant standard-candidate policy (classification: "
                    + methodClassification
                    + ").");
            }

            if (distanceLimit != 0.0f)
            {
                throw new NotSupportedException(
                    "The dormant standard-method HomeDS402Ex candidate requires DistanceLimit=0. Nonzero distance is documented for Gold Home-on-Block methods, which require separate drive qualification.");
            }

            if (timeLimitMilliseconds == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "timeLimitMilliseconds");
            }

            if (detectionTimeLimitMilliseconds == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "detectionTimeLimitMilliseconds");
            }

            ValidateSpare(spare);

            Position = position;
            DetectionVelocityLimit = detectionVelocityLimit;
            Acceleration = acceleration;
            VelocityHigh = velocityHigh;
            VelocityLow = velocityLow;
            DistanceLimit = distanceLimit;
            TorqueLimit = torqueLimit;
            BufferMode = bufferMode;
            HomingMethod = homingMethod;
            TimeLimitMilliseconds = timeLimitMilliseconds;
            DetectionTimeLimitMilliseconds =
                detectionTimeLimitMilliseconds;
            this.spare = (byte[])spare.Clone();
        }

        public double Position { get; private set; }
        public double DetectionVelocityLimit { get; private set; }
        public float Acceleration { get; private set; }
        public float VelocityHigh { get; private set; }
        public float VelocityLow { get; private set; }
        public float DistanceLimit { get; private set; }
        public float TorqueLimit { get; private set; }
        public LMCDs402HomeBufferMode BufferMode { get; private set; }
        public int HomingMethod { get; private set; }
        public uint TimeLimitMilliseconds { get; private set; }
        public uint DetectionTimeLimitMilliseconds { get; private set; }

        /// <summary>
        /// Returns a defensive copy of the required 32 zero spare bytes.
        /// </summary>
        public byte[] Spare
        {
            get { return (byte[])spare.Clone(); }
        }

        public static LMCDs402HomeExMethodClassification
            ClassifyHomingMethod(int homingMethod)
        {
            if ((homingMethod >= 1 && homingMethod <= 14)
                || (homingMethod >= 17 && homingMethod <= 30)
                || homingMethod == 33
                || homingMethod == 34)
            {
                return LMCDs402HomeExMethodClassification
                    .StandardCandidate;
            }

            if (homingMethod >= -4 && homingMethod <= -1)
            {
                return LMCDs402HomeExMethodClassification
                    .GoldDriveQualificationRequired;
            }

            if (homingMethod == 15
                || homingMethod == 16
                || homingMethod == 31
                || homingMethod == 32)
            {
                return LMCDs402HomeExMethodClassification.Reserved;
            }

            if (homingMethod == 35)
            {
                return LMCDs402HomeExMethodClassification.Obsolete;
            }

            return LMCDs402HomeExMethodClassification.Unsupported;
        }

        private static void ValidateFinite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        private static void ValidatePositive(float value, string name)
        {
            ValidateFinite(value, name);
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        private static void ValidateNonNegative(float value, string name)
        {
            ValidateFinite(value, name);
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }

        private static void ValidateSpare(byte[] spare)
        {
            if (spare == null)
            {
                throw new ArgumentNullException("spare");
            }

            if (spare.Length != SpareLength)
            {
                throw new ArgumentException(
                    "HomeDS402Ex requires exactly 32 spare bytes.",
                    "spare");
            }

            for (var index = 0; index < spare.Length; index++)
            {
                if (spare[index] != 0)
                {
                    throw new ArgumentException(
                        "HomeDS402Ex spare bytes must all be zero.",
                        "spare");
                }
            }
        }
    }

    internal enum LMCDs402HomeExApprovedRoundingMode
    {
        AwayFromZero = 1,
        ToEven = 2,
        TowardZero = 3
    }

    /// <summary>
    /// HOMEEX-01/02 software boundary. An instance represents one explicitly
    /// approved physical-axis profile. It is internal so pending design data
    /// cannot become a public raw-plan construction surface.
    /// </summary>
    internal sealed class LMCAxisDs402HomeExApprovedProfile
    {
        private readonly HashSet<int> methodAllowlist;

        internal LMCAxisDs402HomeExApprovedProfile(
            ushort axisReference,
            uint mapRevision,
            IEnumerable<int> methodAllowlist,
            double positionScale,
            double velocityScale,
            double accelerationScale,
            double torqueScale,
            LMCDs402HomeExApprovedRoundingMode roundingMode,
            int convertedMinimum,
            int convertedMaximum,
            bool detectionVelocityLimitEnabled,
            bool distanceLimitEnabled,
            bool torqueLimitEnabled)
        {
            if (axisReference < 1 || axisReference > 4)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }
            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }
            if (methodAllowlist == null)
            {
                throw new ArgumentNullException("methodAllowlist");
            }
            if (convertedMinimum > convertedMaximum)
            {
                throw new ArgumentOutOfRangeException("convertedMinimum");
            }
            if (!Enum.IsDefined(typeof(LMCDs402HomeExApprovedRoundingMode), roundingMode))
            {
                throw new ArgumentOutOfRangeException("roundingMode");
            }

            ValidateScale(positionScale, "positionScale");
            ValidateScale(velocityScale, "velocityScale");
            ValidateScale(accelerationScale, "accelerationScale");
            ValidateScale(torqueScale, "torqueScale");

            this.methodAllowlist = new HashSet<int>();
            foreach (var method in methodAllowlist)
            {
                if (LMCAxisDs402HomeExParameters.ClassifyHomingMethod(method)
                    != LMCDs402HomeExMethodClassification.StandardCandidate)
                {
                    throw new NotSupportedException(
                        "An approved HomeDS402Ex axis profile may contain only v1 standard-method candidates.");
                }
                if (!this.methodAllowlist.Add(method))
                {
                    throw new ArgumentException(
                        "The HomeDS402Ex method allowlist must not contain duplicates.",
                        "methodAllowlist");
                }
            }
            if (this.methodAllowlist.Count == 0)
            {
                throw new ArgumentException(
                    "An approved HomeDS402Ex axis profile requires a non-empty method allowlist.",
                    "methodAllowlist");
            }

            AxisReference = axisReference;
            MapRevision = mapRevision;
            PositionScale = positionScale;
            VelocityScale = velocityScale;
            AccelerationScale = accelerationScale;
            TorqueScale = torqueScale;
            RoundingMode = roundingMode;
            ConvertedMinimum = convertedMinimum;
            ConvertedMaximum = convertedMaximum;
            DetectionVelocityLimitEnabled = detectionVelocityLimitEnabled;
            DistanceLimitEnabled = distanceLimitEnabled;
            TorqueLimitEnabled = torqueLimitEnabled;
        }

        internal ushort AxisReference { get; private set; }
        internal uint MapRevision { get; private set; }
        internal double PositionScale { get; private set; }
        internal double VelocityScale { get; private set; }
        internal double AccelerationScale { get; private set; }
        internal double TorqueScale { get; private set; }
        internal LMCDs402HomeExApprovedRoundingMode RoundingMode { get; private set; }
        internal int ConvertedMinimum { get; private set; }
        internal int ConvertedMaximum { get; private set; }
        internal bool DetectionVelocityLimitEnabled { get; private set; }
        internal bool DistanceLimitEnabled { get; private set; }
        internal bool TorqueLimitEnabled { get; private set; }

        internal LMCAxisDs402HomeExExecutionPlan CreateExecutionPlan(
            LMCAxisDs402HomeExParameters parameters,
            uint verifiedMapRevision)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (verifiedMapRevision != MapRevision)
            {
                throw new InvalidOperationException(
                    "The approved HomeDS402Ex axis profile MapRevision does not match the verified diagnostics MapRevision.");
            }
            if (!methodAllowlist.Contains(parameters.HomingMethod))
            {
                throw new NotSupportedException(
                    "The requested HomeDS402Ex homing method is not approved for this physical axis.");
            }

            RequireDisabledValue(
                DetectionVelocityLimitEnabled,
                parameters.DetectionVelocityLimit,
                "DetectionVelocityLimit");
            RequireDisabledValue(
                DistanceLimitEnabled,
                parameters.DistanceLimit,
                "DistanceLimit");
            RequireDisabledValue(
                TorqueLimitEnabled,
                parameters.TorqueLimit,
                "TorqueLimit");

            var position = ConvertChecked(
                parameters.Position,
                PositionScale,
                "position");
            if (position == int.MinValue)
            {
                throw new OverflowException(
                    "HomeDS402Ex Position cannot convert to Int32.MinValue because final-position negation must remain representable.");
            }

            return new LMCAxisDs402HomeExExecutionPlan(
                parameters.HomingMethod,
                position,
                ConvertChecked(
                    parameters.DetectionVelocityLimit,
                    VelocityScale,
                    "detectionVelocityLimit"),
                ConvertChecked(
                    parameters.Acceleration,
                    AccelerationScale,
                    "acceleration"),
                ConvertChecked(
                    parameters.VelocityHigh,
                    VelocityScale,
                    "velocityHigh"),
                ConvertChecked(
                    parameters.VelocityLow,
                    VelocityScale,
                    "velocityLow"),
                ConvertChecked(
                    parameters.DistanceLimit,
                    PositionScale,
                    "distanceLimit"),
                ConvertChecked(
                    parameters.TorqueLimit,
                    TorqueScale,
                    "torqueLimit"),
                parameters.BufferMode,
                parameters.TimeLimitMilliseconds,
                parameters.DetectionTimeLimitMilliseconds,
                parameters.Spare);
        }

        private int ConvertChecked(double engineeringValue, double scale, string name)
        {
            var scaled = engineeringValue * scale;
            if (double.IsNaN(scaled) || double.IsInfinity(scaled))
            {
                throw new OverflowException(
                    "HomeDS402Ex " + name + " conversion is not finite.");
            }

            double rounded;
            switch (RoundingMode)
            {
                case LMCDs402HomeExApprovedRoundingMode.AwayFromZero:
                    rounded = Math.Round(scaled, MidpointRounding.AwayFromZero);
                    break;
                case LMCDs402HomeExApprovedRoundingMode.ToEven:
                    rounded = Math.Round(scaled, MidpointRounding.ToEven);
                    break;
                case LMCDs402HomeExApprovedRoundingMode.TowardZero:
                    rounded = Math.Truncate(scaled);
                    break;
                default:
                    throw new InvalidOperationException(
                        "Unknown approved HomeDS402Ex rounding mode.");
            }

            if (rounded < ConvertedMinimum || rounded > ConvertedMaximum
                || rounded < int.MinValue || rounded > int.MaxValue)
            {
                throw new OverflowException(
                    "HomeDS402Ex " + name + " conversion is outside the approved DINT range.");
            }
            return checked((int)rounded);
        }

        private static void RequireDisabledValue(
            bool enabled,
            double value,
            string name)
        {
            if (!enabled && value != 0.0)
            {
                throw new NotSupportedException(
                    "HomeDS402Ex " + name + " is disabled by the approved axis profile and must be zero.");
            }
        }

        private static void ValidateScale(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }
}

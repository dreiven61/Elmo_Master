using System;

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
}

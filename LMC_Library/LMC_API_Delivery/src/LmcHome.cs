using System;

namespace LasalMotionControlLib
{
    public enum LMCHomeMode : ushort
    {
        Direct = 1,
        AbsoluteSwitch = 2,
        LimitSwitch = 3,
        ReferencePulse = 4,
        Block = 5
    }

    public enum LMCHomeBufferMode : ushort
    {
        Buffered = 1
    }

    public enum LMCHomeDirection : ushort
    {
        NotApplicable = 0,
        Positive = 1,
        Negative = 2,
        SwitchPositive = 3,
        SwitchNegative = 4
    }

    public enum LMCHomeSwitchMode : ushort
    {
        NotApplicable = 0,
        On = 1,
        Off = 2,
        EdgeOn = 3,
        EdgeOff = 4,
        EdgeSwitchPositive = 5,
        EdgeSwitchNegative = 6
    }

    /// <summary>
    /// Typed LMC_Home parameters. Contract v2 supports Direct and the qualified
    /// LASAL MoveReference modes; Block remains fail-closed.
    /// </summary>
    public sealed class LMCHomeParameters
    {
        [Obsolete("Use the generic LMC_Home parameter constructor.")]
        public LMCHomeParameters(
            int expectedActualPosition,
            int timeoutMilliseconds)
            : this(expectedActualPosition, timeoutMilliseconds, true)
        {
        }

        private LMCHomeParameters(
            int expectedActualPosition,
            int timeoutMilliseconds,
            bool legacyCurrentPositionZero)
        {
            LMC_AdminFrame.ValidateLmcHome(
                LMCHomeSemanticMode.CurrentPositionZero,
                timeoutMilliseconds);
            ExpectedActualPosition = expectedActualPosition;
            TimeoutMilliseconds = timeoutMilliseconds;
            IsLegacyCurrentPositionZero = true;
            Position = 0;
            HomingMode = LMCHomeMode.Direct;
            BufferMode = LMCHomeBufferMode.Buffered;
            Direction = LMCHomeDirection.NotApplicable;
            SwitchMode = LMCHomeSwitchMode.NotApplicable;
        }

        internal static LMCHomeParameters CreateLegacyCurrentPositionZero(
            int expectedActualPosition,
            int timeoutMilliseconds)
        {
            return new LMCHomeParameters(
                expectedActualPosition,
                timeoutMilliseconds,
                true);
        }

        public LMCHomeParameters(
            int position,
            int velocity,
            int acceleration,
            int distanceLimit,
            int torqueLimit,
            LMCHomeMode homingMode,
            LMCHomeBufferMode bufferMode,
            LMCHomeDirection direction,
            LMCHomeSwitchMode switchMode,
            int timeoutMilliseconds)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = acceleration;
            DistanceLimit = distanceLimit;
            TorqueLimit = torqueLimit;
            HomingMode = homingMode;
            BufferMode = bufferMode;
            Direction = direction;
            SwitchMode = switchMode;
            TimeoutMilliseconds = timeoutMilliseconds;
            LMC_AdminFrame.ValidateLmcHome(this);
        }

        public LMCHomeSemanticMode SemanticMode
        {
            get
            {
                return IsLegacyCurrentPositionZero
                    ? LMCHomeSemanticMode.CurrentPositionZero
                    : LMCHomeSemanticMode.GenericHome;
            }
        }
        public int ExpectedActualPosition { get; private set; }
        public int TargetPosition { get { return Position; } }
        public int Position { get; private set; }
        public int Velocity { get; private set; }
        public int Acceleration { get; private set; }
        public int DistanceLimit { get; private set; }
        public int TorqueLimit { get; private set; }
        public int ReferenceVelocity1 { get { return Velocity; } }
        public int ReferenceVelocity2 { get { return TorqueLimit; } }
        public int ReferenceAcceleration { get { return Acceleration; } }
        public int ReferencePositionWindow { get { return DistanceLimit; } }
        public LMCHomeMode HomingMode { get; private set; }
        public LMCHomeBufferMode BufferMode { get; private set; }
        public LMCHomeDirection Direction { get; private set; }
        public LMCHomeSwitchMode SwitchMode { get; private set; }
        public int TimeoutMilliseconds { get; private set; }
        internal bool IsLegacyCurrentPositionZero { get; private set; }
    }

    public partial class LMCSingleAxis
    {
        public LMCPreparedHome PrepareLMC_Home(
            LMCHomeParameters parameters,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCHomeExecuteToken executeToken)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            EnsureCurrentSessionForUse();
            return connection.Admin.PrepareLmcHome(
                this,
                parameters,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }
    }
}

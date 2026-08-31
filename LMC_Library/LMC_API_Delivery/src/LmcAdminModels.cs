using System;

namespace LasalMotionControlLib
{
    [Flags]
    public enum LMCAdminFeature : uint
    {
        None = 0,
        AxisParameterRead = 1u << 0,
        GroupParameterRead = 1u << 1,
        GroupLinearRelative = 1u << 2,
        AxisSetPosition = 1u << 3,
        AxisHome = 1u << 4,
        AxisSetPositionOutcomeRead = 1u << 5,
        AxisDs402Home = 1u << 6,
        AxisSetPositionOutcomeRetirement = 1u << 7,
        AxisSetOperationModeStart = 1u << 8,
        AxisSetOperationModeOutcomeRead = 1u << 9,
        AxisSetOperationModeOutcomeRetire = 1u << 10,
        AxisDs402HomeEx = 1u << 11
    }

    public enum LMCAxisParameterKey : ushort
    {
        SoftwareMinPosition = 1,
        SoftwareMaxPosition = 2,
        EndPositionToleranceWindow = 3,
        MaxVelocity = 4,
        MaxAcceleration = 5,
        ReferencePosition = 6
    }

    [Flags]
    public enum LMCGroupParameterSelection : uint
    {
        None = 0,
        PathVelocityLimit = 1u << 0,
        PathAccelerationLimit = 1u << 1,
        JerkTime = 1u << 2,
        All = PathVelocityLimit | PathAccelerationLimit | JerkTime
    }

    public enum LMCGroupParameterKey : ushort
    {
        PathVelocityLimit = 1,
        PathAccelerationLimit = 2,
        JerkTime = 3
    }

    public enum LMCAdminValueType : ushort
    {
        Int32 = 1
    }

    public enum LMCAdminUnit : ushort
    {
        None = 0,
        ApplicationUnits = 1,
        ApplicationUnitsPerSecond = 2,
        ApplicationUnitsPerSecondSquared = 3,
        Milliseconds = 4
    }

    public enum LMCAdminDetailCode : uint
    {
        None = 0,
        UnsupportedSchema = 1,
        UnsupportedFlags = 2,
        InvalidRequestId = 3,
        InvalidReference = 4,
        InvalidPayloadLength = 5,
        UnsupportedParameter = 6,
        MissingClient = 7,
        InvalidSelection = 8,
        InvalidMotionParameters = 9,
        InvalidState = 10,
        NativeCommandRejected = 11,
        NonZeroVelocity = 12,
        ActiveAxisError = 13,
        InvalidSetPositionSafetyConfiguration = 14,
        CoordinatePreconditionFailed = 15,
        DiagnosticsBuildMismatch = 16,
        BootIdMismatch = 17,
        MapRevisionMismatch = 18,
        SetPositionOutcomeNotFound = 19,
        SetPositionOutcomeIndeterminate = 20,
        SetPositionOutcomeStoreCorrupt = 21,
        SetPositionOutcomeKeyMismatch = 22,
        SetPositionOutcomeSlotOccupied = 23,
        SetPositionOutcomeStorageUnavailable = 24,
        Ds402HomeOutcomeNotFound = 25,
        Ds402HomeOutcomeIndeterminate = 26,
        Ds402HomeOutcomeStoreCorrupt = 27,
        Ds402HomeOutcomeKeyMismatch = 28,
        Ds402HomeOutcomeStorageUnavailable = 29,
        Ds402HomeExecutionFailed = 30,
        Ds402HomeAborted = 31,
        Ds402HomeOutcomeSlotOccupied = 32,
        LmcHomeOutcomeNotFound = 33,
        LmcHomeOutcomeIndeterminate = 34,
        LmcHomeOutcomeStoreCorrupt = 35,
        LmcHomeOutcomeKeyMismatch = 36,
        LmcHomeOutcomeStorageUnavailable = 37,
        LmcHomeExecutionFailed = 38,
        LmcHomeAborted = 39,
        LmcHomeOutcomeSlotOccupied = 40,
        AxisOwnershipConflict = 41,
        AxisOwnershipQuarantined = 42,
        SetOperationModeUnsupportedMode = 43,
        SetOperationModeUnsafeState = 44,
        SetOperationModeOutcomeNotFound = 45,
        SetOperationModeOutcomeIndeterminate = 46,
        SetOperationModeOutcomeStoreCorrupt = 47,
        SetOperationModeOutcomeKeyMismatch = 48,
        SetOperationModeOutcomeStorageUnavailable = 49,
        SetOperationModeExecutionFailed = 50,
        SetOperationModeOutcomeSlotOccupied = 51,
        SetOperationModeOwnershipChannelUnavailable = 52,
        Ds402HomeExOutcomeNotFound = 53,
        Ds402HomeExOutcomeIndeterminate = 54,
        Ds402HomeExOutcomeStoreCorrupt = 55,
        Ds402HomeExOutcomeKeyMismatch = 56,
        Ds402HomeExOutcomeStorageUnavailable = 57,
        Ds402HomeExExecutionFailed = 58,
        Ds402HomeExAborted = 59,
        Ds402HomeExOutcomeSlotOccupied = 60,
        Ds402HomeExInvalidProfile = 61,
        Ds402HomeExCleanupIncomplete = 62,
        SetOperationModeAdmissionIdentityUnavailable = 63,
        SetOperationModeFeatureDisabled = 64
    }

    public sealed class LMCAdminResponse
    {
        internal LMCAdminResponse(
            LMC_Response transportResponse,
            ushort schemaVersion,
            ushort responseFlags,
            ushort commandStatus,
            short errorId,
            uint requestId,
            uint detailCode)
        {
            TransportResponse = transportResponse;
            SchemaVersion = schemaVersion;
            ResponseFlags = responseFlags;
            CommandStatus = commandStatus;
            ErrorId = errorId;
            RequestId = requestId;
            DetailCodeValue = detailCode;
        }

        public LMC_Response TransportResponse { get; private set; }
        public ushort SchemaVersion { get; private set; }
        public ushort ResponseFlags { get; private set; }
        public ushort CommandStatus { get; private set; }
        public short ErrorId { get; private set; }
        public uint RequestId { get; private set; }
        public uint DetailCodeValue { get; private set; }

        public LMCAdminDetailCode DetailCode
        {
            get { return (LMCAdminDetailCode)DetailCodeValue; }
        }

        public bool IsSuccess
        {
            get
            {
                return CommandStatus == 0
                    && ErrorId == 0
                    && DetailCodeValue == 0;
            }
        }
    }

    public sealed class LMCAdminCapabilities
    {
        internal LMCAdminCapabilities(
            LMCAdminResponse response,
            LMCConnection connectionOwner,
            long connectionSessionGeneration,
            LMCAdminFeature features,
            uint axisParameterMask,
            LMCGroupParameterSelection groupParameterSelection,
            ushort physicalAxisCount,
            ushort maxAxisParameterCount,
            ushort groupReference,
            ushort maxGroupParameterCount,
            ushort errorCatalogVersion,
            ushort setOperationModeSupportedMask)
        {
            Response = response;
            ConnectionOwner = connectionOwner;
            ConnectionSessionGeneration = connectionSessionGeneration;
            Features = features;
            AxisParameterMask = axisParameterMask;
            GroupParameterSelection = groupParameterSelection;
            PhysicalAxisCount = physicalAxisCount;
            MaxAxisParameterCount = maxAxisParameterCount;
            GroupReference = groupReference;
            MaxGroupParameterCount = maxGroupParameterCount;
            ErrorCatalogVersion = errorCatalogVersion;
            SetOperationModeSupportedMask = setOperationModeSupportedMask;
        }

        public LMCAdminResponse Response { get; private set; }
        public LMCAdminFeature Features { get; private set; }
        public uint AxisParameterMask { get; private set; }
        public LMCGroupParameterSelection GroupParameterSelection
        {
            get;
            private set;
        }
        public ushort PhysicalAxisCount { get; private set; }
        public ushort MaxAxisParameterCount { get; private set; }
        public ushort GroupReference { get; private set; }
        public ushort MaxGroupParameterCount { get; private set; }
        public ushort ErrorCatalogVersion { get; private set; }
        public ushort SetOperationModeSupportedMask { get; private set; }

        internal long ConnectionSessionGeneration { get; private set; }
        internal LMCConnection ConnectionOwner { get; private set; }
        internal LMCAdmin Owner { get; private set; }
        internal long ObservationSequence { get; private set; }

        internal LMCAdminCapabilities BindProvenance(
            LMCAdmin owner,
            long observationSequence)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (ConnectionOwner == null
                || ConnectionSessionGeneration <= 0
                || observationSequence <= 0)
            {
                throw new InvalidOperationException(
                    "Admin capabilities cannot be bound without connection and observation provenance.");
            }

            if (Owner != null
                && (!ReferenceEquals(Owner, owner)
                    || ObservationSequence != observationSequence))
            {
                throw new InvalidOperationException(
                    "Admin capabilities are already bound to another owner or observation.");
            }

            Owner = owner;
            ObservationSequence = observationSequence;
            return this;
        }

        internal bool IsBoundTo(
            LMCAdmin owner,
            long connectionSessionGeneration)
        {
            return owner != null
                && ReferenceEquals(Owner, owner)
                && ConnectionSessionGeneration
                    == connectionSessionGeneration
                && ObservationSequence > 0;
        }

        public bool Supports(LMCAdminFeature feature)
        {
            return feature != LMCAdminFeature.None
                && (Features & feature) == feature;
        }

        public bool SupportsSetOperationMode(LMCDriveOperationMode mode)
        {
            var raw = (int)(sbyte)mode;
            if (raw < 0 || raw > 15)
            {
                return false;
            }

            return (SetOperationModeSupportedMask & (1 << raw)) != 0;
        }

        public bool Supports(LMCAxisParameterKey key)
        {
            var keyValue = (ushort)key;
            if (keyValue < 1 || keyValue > 32)
            {
                return false;
            }

            return (AxisParameterMask & (1u << (keyValue - 1))) != 0;
        }

        public bool Supports(LMCGroupParameterSelection selection)
        {
            return selection != LMCGroupParameterSelection.None
                && (selection & ~GroupParameterSelection) == 0;
        }
    }

    public sealed class LMCAxisParameterResult
    {
        internal LMCAxisParameterResult(
            LMCAdminResponse response,
            ushort axisReference,
            LMCAxisParameterKey key,
            LMCAdminValueType valueType,
            LMCAdminUnit unit,
            int value)
        {
            Response = response;
            AxisReference = axisReference;
            Key = key;
            ValueType = valueType;
            Unit = unit;
            Value = value;
        }

        public LMCAdminResponse Response { get; private set; }
        public ushort AxisReference { get; private set; }
        public LMCAxisParameterKey Key { get; private set; }
        public LMCAdminValueType ValueType { get; private set; }
        public LMCAdminUnit Unit { get; private set; }
        public int Value { get; private set; }
    }

    public sealed class LMCGroupParametersResult
    {
        internal LMCGroupParametersResult(
            LMCAdminResponse response,
            ushort groupReference,
            LMCGroupParameterSelection selection,
            int pathVelocityLimit,
            int pathAccelerationLimit,
            int jerkTimeMilliseconds)
        {
            Response = response;
            GroupReference = groupReference;
            Selection = selection;
            PathVelocityLimit = pathVelocityLimit;
            PathAccelerationLimit = pathAccelerationLimit;
            JerkTimeMilliseconds = jerkTimeMilliseconds;
        }

        public LMCAdminResponse Response { get; private set; }
        public ushort GroupReference { get; private set; }
        public LMCGroupParameterSelection Selection { get; private set; }
        public int PathVelocityLimit { get; private set; }
        public int PathAccelerationLimit { get; private set; }
        public int JerkTimeMilliseconds { get; private set; }

        public bool TryGetValue(
            LMCGroupParameterKey key,
            out int value,
            out LMCAdminUnit unit)
        {
            switch (key)
            {
                case LMCGroupParameterKey.PathVelocityLimit:
                    value = PathVelocityLimit;
                    unit = LMCAdminUnit.ApplicationUnitsPerSecond;
                    return (Selection
                        & LMCGroupParameterSelection.PathVelocityLimit) != 0;

                case LMCGroupParameterKey.PathAccelerationLimit:
                    value = PathAccelerationLimit;
                    unit = LMCAdminUnit.ApplicationUnitsPerSecondSquared;
                    return (Selection
                        & LMCGroupParameterSelection.PathAccelerationLimit) != 0;

                case LMCGroupParameterKey.JerkTime:
                    value = JerkTimeMilliseconds;
                    unit = LMCAdminUnit.Milliseconds;
                    return (Selection
                        & LMCGroupParameterSelection.JerkTime) != 0;

                default:
                    value = 0;
                    unit = LMCAdminUnit.None;
                    return false;
            }
        }
    }

    public class LMCAdminCommandException : InvalidOperationException
    {
        internal LMCAdminCommandException(
            string message,
            LMCAdminResponse response)
            : base(message)
        {
            Response = response;
        }

        public LMCAdminResponse Response { get; private set; }
    }

    public sealed class LMCAdminNotSupportedException : NotSupportedException
    {
        internal LMCAdminNotSupportedException(
            string message,
            LMC_Response acknowledgement)
            : base(message)
        {
            Acknowledgement = acknowledgement;
        }

        public LMC_Response Acknowledgement { get; private set; }
    }
}

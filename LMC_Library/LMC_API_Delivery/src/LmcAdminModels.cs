using System;

namespace LasalMotionControlLib
{
    [Flags]
    public enum LMCAdminFeature : uint
    {
        None = 0,
        AxisParameterRead = 1u << 0,
        GroupParameterRead = 1u << 1
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
        InvalidSelection = 8
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
            long connectionSessionGeneration,
            LMCAdminFeature features,
            uint axisParameterMask,
            LMCGroupParameterSelection groupParameterSelection,
            ushort physicalAxisCount,
            ushort maxAxisParameterCount,
            ushort groupReference,
            ushort maxGroupParameterCount,
            ushort errorCatalogVersion)
        {
            Response = response;
            ConnectionSessionGeneration = connectionSessionGeneration;
            Features = features;
            AxisParameterMask = axisParameterMask;
            GroupParameterSelection = groupParameterSelection;
            PhysicalAxisCount = physicalAxisCount;
            MaxAxisParameterCount = maxAxisParameterCount;
            GroupReference = groupReference;
            MaxGroupParameterCount = maxGroupParameterCount;
            ErrorCatalogVersion = errorCatalogVersion;
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

        internal long ConnectionSessionGeneration { get; private set; }

        public bool Supports(LMCAdminFeature feature)
        {
            return feature != LMCAdminFeature.None
                && (Features & feature) == feature;
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

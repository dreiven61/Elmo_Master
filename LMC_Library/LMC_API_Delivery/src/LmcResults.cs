using System;

namespace LasalMotionControlLib
{
    internal static class LMC_ResultSemantics
    {
        internal const ushort CommandErrorMask = 0x0010;

        internal static bool HasCommandError(ushort functionStatus)
        {
            return (functionStatus & CommandErrorMask) != 0;
        }

        internal static bool IsFunctionResultSuccess(
            LMC_Response response,
            ushort functionStatus,
            short errorId)
        {
            return response != null
                && response.IsFrameValid
                && response.HeaderStatus == 0
                && errorId == 0
                && !HasCommandError(functionStatus);
        }
    }

    public sealed class LMCReadStatusResult
    {
        private const uint LasalAxisPowerOnMask = 0x00000001u;
        private const uint LasalAxisReferencedMask = 0x00000002u;
        private const uint LasalAxisStandstillMask = 0x02000000u;

        internal LMCReadStatusResult(
            LMC_Response response,
            uint state,
            ushort functionStatus,
            short errorId,
            ushort axisErrorId,
            ushort statusWord)
        {
            Response = response;
            State = state;
            FunctionStatus = functionStatus;
            ErrorId = errorId;
            AxisErrorId = axisErrorId;
            StatusWord = statusWord;
        }

        public LMC_Response Response { get; private set; }
        public uint State { get; private set; }
        public ushort FunctionStatus { get; private set; }
        public short ErrorId { get; private set; }
        public ushort AxisErrorId { get; private set; }
        public ushort StatusWord { get; private set; }

        /// <summary>
        /// True when the LASAL adapter reports the native axis power-on state.
        /// </summary>
        public bool IsPowerOn
        {
            get { return (State & LasalAxisPowerOnMask) != 0; }
        }

        /// <summary>
        /// True when the native LASAL _LMCAXIS_STATUS.IsReferenced bit is set.
        /// This is the axis reference/home-complete state, not a DS402 statusword bit.
        /// </summary>
        public bool IsReferenced
        {
            get { return (State & LasalAxisReferencedMask) != 0; }
        }

        public bool IsStandstill
        {
            get { return (State & LasalAxisStandstillMask) != 0; }
        }

        public bool HasCommandError
        {
            get { return LMC_ResultSemantics.HasCommandError(FunctionStatus); }
        }

        public bool IsSuccess
        {
            get
            {
                return LMC_ResultSemantics.IsFunctionResultSuccess(
                        Response,
                        FunctionStatus,
                        ErrorId)
                    && AxisErrorId == 0;
            }
        }
    }

    public sealed class LMCReadActualPositionResult
    {
        internal LMCReadActualPositionResult(
            LMC_Response response,
            int positionRaw,
            ushort functionStatus,
            short errorId)
        {
            Response = response;
            PositionRaw = positionRaw;
            FunctionStatus = functionStatus;
            ErrorId = errorId;
        }

        public LMC_Response Response { get; private set; }
        public int PositionRaw { get; private set; }
        public ushort FunctionStatus { get; private set; }
        public short ErrorId { get; private set; }

        public bool HasCommandError
        {
            get { return LMC_ResultSemantics.HasCommandError(FunctionStatus); }
        }

        public bool IsSuccess
        {
            get
            {
                return LMC_ResultSemantics.IsFunctionResultSuccess(
                    Response,
                    FunctionStatus,
                    ErrorId);
            }
        }
    }

    public sealed class LMCGroupReadStatusResult
    {
        // Disabled and standby retain the Maestro group-state mask values. The
        // power-ready bit and the LASAL conditions that drive all three masks
        // are this adapter's project-local contract.
        private const uint LasalGroupDisabledMask = 0x00010000u;
        private const uint LasalGroupStandbyMask = 0x00020000u;
        private const uint LasalGroupPowerReadyMask = 0x00040000u;

        internal LMCGroupReadStatusResult(
            LMC_Response response,
            uint state,
            ushort functionStatus,
            short errorId,
            ushort groupErrorId)
        {
            Response = response;
            State = state;
            FunctionStatus = functionStatus;
            ErrorId = errorId;
            GroupErrorId = groupErrorId;
        }

        public LMC_Response Response { get; private set; }
        public uint State { get; private set; }
        public ushort FunctionStatus { get; private set; }
        public short ErrorId { get; private set; }
        public ushort GroupErrorId { get; private set; }

        /// <summary>
        /// True when the LASAL adapter reports project-local group power ready.
        /// </summary>
        public bool IsPowerOn
        {
            get { return (State & LasalGroupPowerReadyMask) != 0; }
        }

        /// <summary>
        /// True when the standard Maestro group standby mask is set. The LASAL
        /// adapter sets it only while powered, profile-locked, and in position.
        /// </summary>
        public bool IsStandby
        {
            get { return (State & LasalGroupStandbyMask) != 0; }
        }

        /// <summary>
        /// Compatibility alias for IsStandby; this means profile locked, not servo power on.
        /// </summary>
        public bool IsEnabled
        {
            get { return IsStandby; }
        }

        /// <summary>
        /// True when the standard Maestro group disabled mask is set. The LASAL
        /// adapter uses this state for an unlocked profile.
        /// </summary>
        public bool IsDisabled
        {
            get { return (State & LasalGroupDisabledMask) != 0; }
        }

        public bool HasCommandError
        {
            get { return LMC_ResultSemantics.HasCommandError(FunctionStatus); }
        }

        public bool IsSuccess
        {
            get
            {
                return LMC_ResultSemantics.IsFunctionResultSuccess(
                        Response,
                        FunctionStatus,
                        ErrorId)
                    && GroupErrorId == 0;
            }
        }
    }

    public sealed class LMCGroupReadActualPositionResult
    {
        private readonly int[] positionsRaw;

        internal LMCGroupReadActualPositionResult(
            LMC_Response response,
            LMC_COORD_SYSTEM coordinateSystem,
            int[] positionsRaw,
            ushort functionStatus,
            short errorId)
        {
            Response = response;
            CoordinateSystem = coordinateSystem;
            this.positionsRaw = (int[])positionsRaw.Clone();
            FunctionStatus = functionStatus;
            ErrorId = errorId;
        }

        public LMC_Response Response { get; private set; }
        public LMC_COORD_SYSTEM CoordinateSystem { get; private set; }

        public int[] PositionsRaw
        {
            get { return (int[])positionsRaw.Clone(); }
        }

        public ushort FunctionStatus { get; private set; }
        public short ErrorId { get; private set; }

        public bool HasCommandError
        {
            get { return LMC_ResultSemantics.HasCommandError(FunctionStatus); }
        }

        public bool IsSuccess
        {
            get
            {
                return LMC_ResultSemantics.IsFunctionResultSuccess(
                    Response,
                    FunctionStatus,
                    ErrorId);
            }
        }
    }

    public sealed class LMCGroupMemberInfo
    {
        internal LMCGroupMemberInfo(
            int index,
            ushort axisReference,
            ushort deviceId,
            string axisName)
        {
            Index = index;
            AxisReference = axisReference;
            DeviceId = deviceId;
            AxisName = axisName ?? string.Empty;
        }

        public int Index { get; private set; }
        public ushort AxisReference { get; private set; }
        public ushort DeviceId { get; private set; }
        public string AxisName { get; private set; }
    }

    public sealed class LMCGroupMembersInfoResult
    {
        private readonly ushort[] axisReferences;
        private readonly ushort[] deviceIds;
        private readonly string[] axisNames;
        private readonly LMCGroupMemberInfo[] members;

        internal LMCGroupMembersInfoResult(
            LMC_Response response,
            ushort[] axisReferences,
            ushort[] deviceIds,
            string[] axisNames,
            byte axisCount,
            ushort functionStatus,
            short errorId)
        {
            Response = response;
            this.axisReferences = (ushort[])axisReferences.Clone();
            this.deviceIds = (ushort[])deviceIds.Clone();
            this.axisNames = (string[])axisNames.Clone();
            AxisCount = axisCount;
            FunctionStatus = functionStatus;
            ErrorId = errorId;

            members = new LMCGroupMemberInfo[axisCount];
            for (var index = 0; index < axisCount; index++)
            {
                members[index] = new LMCGroupMemberInfo(
                    index,
                    this.axisReferences[index],
                    this.deviceIds[index],
                    this.axisNames[index]);
            }
        }

        public LMC_Response Response { get; private set; }

        public ushort[] AxisReferences
        {
            get { return (ushort[])axisReferences.Clone(); }
        }

        public ushort[] DeviceIds
        {
            get { return (ushort[])deviceIds.Clone(); }
        }

        public string[] AxisNames
        {
            get { return (string[])axisNames.Clone(); }
        }

        public LMCGroupMemberInfo[] Members
        {
            get { return (LMCGroupMemberInfo[])members.Clone(); }
        }

        public byte AxisCount { get; private set; }
        public ushort FunctionStatus { get; private set; }
        public short ErrorId { get; private set; }

        public bool HasCommandError
        {
            get { return LMC_ResultSemantics.HasCommandError(FunctionStatus); }
        }

        public bool IsSuccess
        {
            get
            {
                return LMC_ResultSemantics.IsFunctionResultSuccess(
                    Response,
                    FunctionStatus,
                    ErrorId);
            }
        }
    }
}

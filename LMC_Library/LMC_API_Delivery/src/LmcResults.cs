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

        public bool IsPowerOn
        {
            get { return (State & LasalAxisPowerOnMask) != 0; }
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

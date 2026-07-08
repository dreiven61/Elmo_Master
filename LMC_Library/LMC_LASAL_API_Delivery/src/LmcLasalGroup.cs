using System;

namespace LmcLasalMotionApi
{
    public sealed class LMCGroup
    {
        private const int LookupReferenceOffset = 12;
        private const int MinimumLookupResponseLength = 14;
        private const int ResponseValueOffset = 8;
        private const int MinimumValueResponseLength = 12;

        private readonly LMCConnection connection;
        private readonly LMC_UnitConverter units;

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }

        public LMCGroup(
            LMCConnection connection,
            string groupName,
            LMC_UnitConverter units = null)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.units = units ?? new LMC_UnitConverter();

            GroupName = groupName;
            GroupReference = ResolveGroupReference(groupName);
        }

        public LMC_Response LMC_GetGroupMembersInfo()
        {
            return Send(LMC_Frame.Simple(LMC_CommandId.GetMembers, GroupReference));
        }

        public LMC_Response LMC_GroupEnableCmd()
        {
            return Send(LMC_Frame.Simple(LMC_CommandId.GroupEnable, GroupReference));
        }

        public LMC_Response LMC_GroupDisableCmd()
        {
            return Send(LMC_Frame.Simple(LMC_CommandId.GroupDisable, GroupReference));
        }

        public LMC_Response LMC_GroupResetCmd()
        {
            return Send(LMC_Frame.Simple(LMC_CommandId.GroupReset, GroupReference));
        }

        public LMC_Response LMC_GroupStopCmd(double deceleration, double jerk)
        {
            return Send(
                LMC_Frame.GroupStop(
                    GroupReference,
                    units.DecelerationToInternal(deceleration),
                    units.JerkToInternal(jerk)));
        }

        public uint LMC_GroupReadStatusCmd(out LMC_Response response)
        {
            var raw = connection.Exchange(
                LMC_Frame.GroupRead(LMC_CommandId.GroupStatus, GroupReference));

            response = new LMC_Response { Raw = raw };

            if (raw.Length < MinimumValueResponseLength)
            {
                return 0;
            }

            return LMC_Frame.ReadUInt32(raw, ResponseValueOffset);
        }

        public LMC_Response LMC_MoveLinearAbsoluteExCmd(
            double[] position,
            double velocity,
            double acceleration,
            double deceleration,
            double jerk)
        {
            return Send(
                LMC_Frame.MoveLinear(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    units));
        }

        private ushort ResolveGroupReference(string groupName)
        {
            var raw = connection.Exchange(
                LMC_Frame.Name(LMC_CommandId.GetGroupByName, groupName));

            if (raw.Length < MinimumLookupResponseLength)
            {
                throw new InvalidOperationException("Invalid LASAL group lookup response.");
            }

            return LMC_Frame.ReadUInt16(raw, LookupReferenceOffset);
        }

        private LMC_Response Send(byte[] request)
        {
            return LMCConnection.Parse(connection.Exchange(request));
        }
    }
}

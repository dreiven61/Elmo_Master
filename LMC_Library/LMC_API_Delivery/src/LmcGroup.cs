using System;

namespace LasalMotionControlLib
{
    public class LMCGroupAxis
    {
        private readonly LMCConnection connection;

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }

        public LMCGroupAxis(LMCConnection connection, string groupName)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));

            GroupName = groupName;
            GroupReference = ResolveGroupReference(groupName);
        }

        public LMC_Response GetGroupMembersInfo()
        {
            return Send(LMC_Frame.LMCGroupGetMembersInfo(GroupReference));
        }

        public LMC_Response LMC_GetGroupMembersInfo()
        {
            return GetGroupMembersInfo();
        }

        public LMC_Response GroupEnable()
        {
            return Send(LMC_Frame.LMCGroupEnable(GroupReference));
        }

        public LMC_Response LMC_GroupEnableCmd()
        {
            return GroupEnable();
        }

        public LMC_Response GroupDisable()
        {
            return Send(LMC_Frame.LMCGroupDisable(GroupReference));
        }

        public LMC_Response LMC_GroupDisableCmd()
        {
            return GroupDisable();
        }

        public LMC_Response GroupReset()
        {
            return Send(LMC_Frame.LMCGroupReset(GroupReference));
        }

        public LMC_Response LMC_GroupResetCmd()
        {
            return GroupReset();
        }

        public LMC_Response GroupStop(int deceleration, int jerk)
        {
            return Send(LMC_Frame.LMCGroupStop(GroupReference, deceleration, jerk));
        }

        public LMC_Response LMC_GroupStopCmd(int deceleration, int jerk)
        {
            return GroupStop(deceleration, jerk);
        }

        public uint GroupReadStatus()
        {
            LMC_Response response;
            return GroupReadStatus(out response);
        }

        public uint GroupReadStatus(out LMC_Response response)
        {
            return LMCConnection.ParseUInt32Value(
                connection.Exchange(
                    LMC_Frame.LMCGroupReadStatus(GroupReference)),
                out response);
        }

        public uint LMC_GroupReadStatusCmd(out LMC_Response response)
        {
            return GroupReadStatus(out response);
        }

        public LMC_Response MoveLinearAbsoluteEx(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return Send(
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk));
        }

        public LMC_Response LMC_MoveLinearAbsoluteExCmd(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return MoveLinearAbsoluteEx(
                position,
                velocity,
                acceleration,
                deceleration,
                jerk);
        }

        private ushort ResolveGroupReference(string groupName)
        {
            ushort groupReference;

            if (!LMCConnection.TryParseLookupReference(
                connection.Exchange(LMC_Frame.LMCGroupGetByName(groupName)),
                out _,
                out groupReference))
            {
                throw new InvalidOperationException("Invalid group lookup response.");
            }

            return groupReference;
        }

        private LMC_Response Send(byte[] request)
        {
            return LMCConnection.ParseAcknowledgement(connection.Exchange(request));
        }
    }

    public sealed class LMCGroup : LMCGroupAxis
    {
        public LMCGroup(LMCConnection connection, string groupName)
            : base(connection, groupName)
        {
        }
    }
}

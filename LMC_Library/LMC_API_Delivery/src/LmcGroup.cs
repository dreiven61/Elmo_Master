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
            return SendGetGroupMembersInfo();
        }

        public LMC_Response GroupEnable()
        {
            return SendGroupEnable();
        }

        public LMC_Response GroupDisable()
        {
            return SendGroupDisable();
        }

        public LMC_Response GroupReset()
        {
            return SendGroupReset();
        }

        public LMC_Response GroupStop(int deceleration, int jerk)
        {
            return SendGroupStop(deceleration, jerk);
        }

        public uint GroupReadStatus()
        {
            LMC_Response response;
            return ReadGroupStatusValue(out response);
        }

        public uint GroupReadStatus(out LMC_Response response)
        {
            return ReadGroupStatusValue(out response);
        }

        public LMC_Response MoveLinearAbsoluteEx(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return SendMoveLinearAbsolute(position, velocity, acceleration, deceleration, jerk);
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

        private LMC_Response SendGetGroupMembersInfo()
        {
            return Send(LMC_Frame.LMCGroupGetMembersInfo(GroupReference));
        }

        private LMC_Response SendGroupEnable()
        {
            return Send(LMC_Frame.LMCGroupEnable(GroupReference));
        }

        private LMC_Response SendGroupDisable()
        {
            return Send(LMC_Frame.LMCGroupDisable(GroupReference));
        }

        private LMC_Response SendGroupReset()
        {
            return Send(LMC_Frame.LMCGroupReset(GroupReference));
        }

        private LMC_Response SendGroupStop(int deceleration, int jerk)
        {
            return Send(LMC_Frame.LMCGroupStop(GroupReference, deceleration, jerk));
        }

        private uint ReadGroupStatusValue(out LMC_Response response)
        {
            return LMCConnection.ParseUInt32Value(
                connection.Exchange(
                    LMC_Frame.LMCGroupReadStatus(GroupReference)),
                out response);
        }

        private LMC_Response SendMoveLinearAbsolute(
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

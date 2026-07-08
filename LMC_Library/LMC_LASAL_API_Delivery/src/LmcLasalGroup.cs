using System;

namespace LmcLasalMotionApi
{
    public sealed class LMCGroup
    {
        private readonly LMCConnection connection;private readonly LMC_UnitConverter units;
        public string GroupName{get;private set;}public ushort GroupReference{get;private set;}
        public LMCGroup(LMCConnection connection,string groupName,LMC_UnitConverter units=null){this.connection=connection??throw new ArgumentNullException(nameof(connection));this.units=units??new LMC_UnitConverter();GroupName=groupName;var raw=connection.Exchange(LMC_Frame.Name(LMC_CommandId.GetGroupByName,groupName));if(raw.Length<14)throw new InvalidOperationException("Invalid LASAL group lookup response.");GroupReference=LMC_Frame.U16(raw,12);}
        public LMC_Response LMC_GetGroupMembersInfo(){return Send(LMC_Frame.Simple(LMC_CommandId.GetMembers,GroupReference));}
        public LMC_Response LMC_GroupEnableCmd(){return Send(LMC_Frame.Simple(LMC_CommandId.GroupEnable,GroupReference));}
        public LMC_Response LMC_GroupDisableCmd(){return Send(LMC_Frame.Simple(LMC_CommandId.GroupDisable,GroupReference));}
        public LMC_Response LMC_GroupResetCmd(){return Send(LMC_Frame.Simple(LMC_CommandId.GroupReset,GroupReference));}
        public LMC_Response LMC_GroupStopCmd(double decelerationRps2,double jerkRps3){return Send(LMC_Frame.GroupStop(GroupReference,units.RevolutionToInternal(decelerationRps2),units.RevolutionToInternal(jerkRps3)));}
        public uint LMC_GroupReadStatusCmd(out LMC_Response response){var raw=connection.Exchange(LMC_Frame.GroupRead(LMC_CommandId.GroupStatus,GroupReference));response=new LMC_Response{Raw=raw};return raw.Length>=12?LMC_Frame.U32(raw,8):0;}
        public LMC_Response LMC_MoveLinearAbsoluteExCmd(double[] positionRev,double velocityRps,double accelerationRps2,double decelerationRps2,double jerkRps3){return Send(LMC_Frame.MoveLinear(GroupReference,positionRev,velocityRps,accelerationRps2,decelerationRps2,jerkRps3,units));}
        private LMC_Response Send(byte[] b){return LMCConnection.Parse(connection.Exchange(b));}
    }
}

using System;

namespace LmcLasalMotionApi
{
    public sealed class LMCAxis
    {
        private readonly LMCConnection connection; private readonly LMC_UnitConverter units;
        public string AxisName{get;private set;} public ushort AxisReference{get;private set;}
        public LMCAxis(LMCConnection connection,string axisName,LMC_UnitConverter units=null)
        {this.connection=connection??throw new ArgumentNullException(nameof(connection));this.units=units??new LMC_UnitConverter();AxisName=axisName;var raw=connection.Exchange(LMC_Frame.Name(LMC_CommandId.GetAxisByName,axisName));if(raw.Length<14)throw new InvalidOperationException("Invalid LASAL axis lookup response.");AxisReference=LMC_Frame.U16(raw,12);connection.Exchange(LMC_Frame.AxisInfo(AxisReference));}
        public LMC_Response LMC_PowerCmd(bool enable){return Send(LMC_Frame.Power(AxisReference,enable));}
        public LMC_Response LMC_Reset(){return Send(LMC_Frame.Simple(LMC_CommandId.Reset,AxisReference));}
        public LMC_Response LMC_StopCmd(double decelerationRps2,double jerkRps3){return Send(LMC_Frame.Stop(AxisReference,units.RevolutionToInternal(decelerationRps2),units.RevolutionToInternal(jerkRps3)));}
        public LMC_Response LMC_MoveAbsoluteExCmd(double positionRev,double velocityRps,double accelerationRps2,double decelerationRps2,double jerkRps3,LMC_DIRECTION direction=LMC_DIRECTION.Shortest){return Move(LMC_CommandId.MoveAbsolute,positionRev,velocityRps,accelerationRps2,decelerationRps2,jerkRps3,direction);}
        public LMC_Response LMC_MoveRelativeExCmd(double distanceRev,double velocityRps,double accelerationRps2,double decelerationRps2,double jerkRps3,LMC_DIRECTION direction=LMC_DIRECTION.Shortest){return Move(LMC_CommandId.MoveRelative,distanceRev,velocityRps,accelerationRps2,decelerationRps2,jerkRps3,direction);}
        public LMC_Response LMC_MoveVelocityExCmd(double velocityRps,double accelerationRps2,double decelerationRps2,double jerkRps3,LMC_DIRECTION direction){return Send(LMC_Frame.Velocity(AxisReference,units.RevolutionToInternal(velocityRps),units.RevolutionToInternal(accelerationRps2),units.RevolutionToInternal(decelerationRps2),units.RevolutionToInternal(jerkRps3),direction));}
        public uint LMC_ReadStatusCmd(out LMC_Response response){var raw=connection.Exchange(LMC_Frame.ReadStatus(AxisReference));response=new LMC_Response{Raw=raw};return raw.Length>=12?LMC_Frame.U32(raw,8):0;}
        public double LMC_ReadActualPositionCmd(out LMC_Response response){var raw=connection.Exchange(LMC_Frame.ReadPosition(AxisReference));response=new LMC_Response{Raw=raw};return raw.Length>=12?units.InternalToRevolution(LMC_Frame.I32(raw,8)):0;}
        private LMC_Response Move(ushort command,double p,double v,double a,double d,double j,LMC_DIRECTION direction){return Send(LMC_Frame.AxisMove(command,AxisReference,units.RevolutionToInternal(p),units.RevolutionToInternal(v),units.RevolutionToInternal(a),units.RevolutionToInternal(d),units.RevolutionToInternal(j),direction));}
        private LMC_Response Send(byte[] b){return LMCConnection.Parse(connection.Exchange(b));}
    }
}

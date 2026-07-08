using System;

namespace LmcLasalMotionApi
{
    public enum LMC_DIRECTION : int { None=0, Positive=1, Shortest=2, Negative=3, Current=4 }

    public sealed class LMC_Response
    {
        public byte[] Raw { get; internal set; }
        public ushort Status { get; internal set; }
        public short ErrorId { get; internal set; }
        public bool IsSuccess { get { return Status == 0 && ErrorId == 0; } }
    }

    public static class LMC_Units
    {
        // Mirrors Lasal_PRG/Elmo_EtherCAT_Test_4Axis/Include/unit.h.
        public const int LMC_MMPSEC2 = 1;
        public const int LMC_DEG = 10000;
        public const int LMC_MM2 = 10;
        public const int LMC_KN = 1000;
        public const int LMC_N = 1;
        public const int LMC_M = 1000 * 10000;
        public const int LMC_MM = 10000;
        public const int LMC_GB = 1024 * 1024 * 1024;
        public const int LMC_KB = 1024;
        public const int LMC_MB = 1024 * 1024;
        public const int LMC_BAR = 1000;
        public const int LMC_RPM = 1000;
        public const int LMC_MLPMIN = 1;
        public const int LMC_MMPSEC = 10000;
        public const int LMC_HOURS = 60 * 60 * 1000;
        public const int LMC_MIN = 60 * 1000;
        public const int LMC_MS = 1;
        public const int LMC_SEC = 1000;
        public const int LMC_SECS = 1000;
        public const int LMC_CCM = 1;
    }

    public sealed class LMC_UnitConverter
    {
        public int PositionUnit { get; private set; }
        public int VelocityUnit { get; private set; }
        public int AccelerationUnit { get; private set; }
        public int DecelerationUnit { get; private set; }
        public int JerkUnit { get; private set; }

        public LMC_UnitConverter()
            : this(
                LMC_Units.LMC_MM,
                LMC_Units.LMC_MMPSEC,
                LMC_Units.LMC_MMPSEC2,
                LMC_Units.LMC_MMPSEC2,
                LMC_Units.LMC_MMPSEC2)
        {
        }

        public LMC_UnitConverter(int positionUnit,int velocityUnit,int accelerationUnit,int decelerationUnit,int jerkUnit)
        {
            ValidateUnit(positionUnit,nameof(positionUnit));
            ValidateUnit(velocityUnit,nameof(velocityUnit));
            ValidateUnit(accelerationUnit,nameof(accelerationUnit));
            ValidateUnit(decelerationUnit,nameof(decelerationUnit));
            ValidateUnit(jerkUnit,nameof(jerkUnit));
            PositionUnit=positionUnit;
            VelocityUnit=velocityUnit;
            AccelerationUnit=accelerationUnit;
            DecelerationUnit=decelerationUnit;
            JerkUnit=jerkUnit;
        }

        public int PositionToInternal(double value){return Scale(value,PositionUnit);}
        public int VelocityToInternal(double value){return Scale(value,VelocityUnit);}
        public int AccelerationToInternal(double value){return Scale(value,AccelerationUnit);}
        public int DecelerationToInternal(double value){return Scale(value,DecelerationUnit);}
        public int JerkToInternal(double value){return Scale(value,JerkUnit);}
        public double InternalToPosition(int value){return (double)value/PositionUnit;}

        private static void ValidateUnit(int unit,string name)
        {
            if(unit<=0) throw new ArgumentOutOfRangeException(name);
        }

        private static int Scale(double value,int unit)
        {
            if(double.IsNaN(value)||double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
            return checked((int)Math.Round(value*unit,MidpointRounding.AwayFromZero));
        }
    }

    internal static class LMC_CommandId
    {
        internal const ushort GetAxisByName=0x103C, GetGroupByName=0x1042;
        internal const ushort CloseConnection=0x405D, Power=0x2023, Reset=0x2024, Stop=0x2022;
        internal const ushort AxisInfo=0x202B, ReadStatus=0x2028, ReadPosition=0x202E;
        internal const ushort MoveAbsolute=0x209F, MoveRelative=0x20A0, MoveVelocity=0x20A2;
        internal const ushort GetMembers=0x20D2, GroupStatus=0x2045, GroupEnable=0x2047;
        internal const ushort GroupDisable=0x2048, GroupReset=0x2049, GroupStop=0x2085;
        internal const ushort GroupPosition=0x2051, MoveLinear=0x20A4;
    }

    internal static class LMC_Frame
    {
        internal static byte[] Header(ushort command,ushort reference,ushort payloadLength)
        { var b=new byte[8+payloadLength];U16(b,0,command);U16(b,4,payloadLength);U16(b,6,reference);return b; }
        internal static byte[] Name(ushort command,string name)
        { var b=Header(command,0,0x50);var s=System.Text.Encoding.ASCII.GetBytes(name??"");Buffer.BlockCopy(s,0,b,8,Math.Min(s.Length,79));return b; }
        internal static byte[] AxisInfo(ushort r)
        { var b=Header(LMC_CommandId.AxisInfo,r,12);I32(b,8,5);I32(b,16,1);return b; }
        internal static byte[] Power(ushort r,bool on)
        { var b=Header(LMC_CommandId.Power,r,8);I32(b,8,1);b[12]=on?(byte)1:(byte)0;b[13]=1;b[15]=1;return b; }
        internal static byte[] Simple(ushort command,ushort r)
        { var b=Header(command,r,1);b[8]=1;return b; }
        internal static byte[] ReadStatus(ushort r)
        { var b=Header(LMC_CommandId.ReadStatus,r,8);I32(b,8,r);I32(b,12,1);return b; }
        internal static byte[] ReadPosition(ushort r)
        { return Header(LMC_CommandId.ReadPosition,r,1); }
        internal static byte[] Stop(ushort r,int dec,int jerk)
        { var b=Header(LMC_CommandId.Stop,r,16);I32(b,8,dec);I32(b,12,jerk);I32(b,16,1);I32(b,20,1);return b; }
        internal static byte[] GroupStop(ushort r,int dec,int jerk)
        { var b=Header(LMC_CommandId.GroupStop,r,16);I32(b,8,dec);I32(b,12,jerk);I32(b,16,1);I32(b,20,1);return b; }
        internal static byte[] AxisMove(ushort command,ushort r,int value,int vel,int acc,int dec,int jerk,LMC_DIRECTION direction)
        { var b=Header(command,r,32);I32(b,8,value);I32(b,12,vel);I32(b,16,acc);I32(b,20,dec);I32(b,24,jerk);I32(b,28,(int)direction);I32(b,32,1);I32(b,36,1);return b; }
        internal static byte[] Velocity(ushort r,int vel,int acc,int dec,int jerk,LMC_DIRECTION direction)
        { var b=Header(LMC_CommandId.MoveVelocity,r,24);I32(b,8,vel);I32(b,12,acc);I32(b,16,dec);I32(b,20,jerk);I32(b,24,(int)direction);I32(b,28,1);return b; }
        internal static byte[] GroupRead(ushort command,ushort r)
        { var b=Header(command,r,8);I32(b,8,0);I32(b,12,1);return b; }
        internal static byte[] MoveLinear(ushort r,double[] position,double velocity,double acceleration,double deceleration,double jerk,LMC_UnitConverter units)
        { var b=Header(LMC_CommandId.MoveLinear,r,96);for(var i=0;i<16;i++)I32(b,8+i*4,units.PositionToInternal(position!=null&&i<position.Length?position[i]:0));I32(b,72,units.VelocityToInternal(velocity));I32(b,76,units.AccelerationToInternal(acceleration));I32(b,80,units.DecelerationToInternal(deceleration));I32(b,84,units.JerkToInternal(jerk));I32(b,88,0);I32(b,92,0);I32(b,96,1);I32(b,100,1);return b; }
        internal static ushort U16(byte[] b,int o){return (ushort)(b[o]|b[o+1]<<8);}
        internal static uint U32(byte[] b,int o){return (uint)(b[o]|b[o+1]<<8|b[o+2]<<16|b[o+3]<<24);}
        internal static int I32(byte[] b,int o){return unchecked((int)U32(b,o));}
        internal static void U16(byte[] b,int o,ushort v){b[o]=(byte)v;b[o+1]=(byte)(v>>8);}
        internal static void I32(byte[] b,int o,int v){var x=unchecked((uint)v);b[o]=(byte)x;b[o+1]=(byte)(x>>8);b[o+2]=(byte)(x>>16);b[o+3]=(byte)(x>>24);}
    }
}

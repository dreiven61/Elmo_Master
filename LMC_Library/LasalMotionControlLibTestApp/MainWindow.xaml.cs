using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using LasalMotionControlLib;

namespace LasalMotionControlLibTestApp
{
    public partial class MainWindow : Window
    {
        private LMCConnection _connection;
        private const string Single = "Single Axis", Group = "Coordinated Group";
        public MainWindow()
        {
            InitializeComponent(); TargetMode.Items.Add(Single); TargetMode.Items.Add(Group); TargetMode.SelectedIndex=0;
            Direction.Items.Add(LMC_DIRECTION.Positive); Direction.Items.Add(LMC_DIRECTION.Negative); Direction.SelectedIndex=0; FillApis();
        }
        private void Connect_Click(object s,RoutedEventArgs e){Run("RpcInitConnection",()=>{_connection?.Dispose();_connection=new LMCConnection();_connection.RpcInitConnection(RemoteIp.Text.Trim(),Int(RemotePort.Text),LocalIp.Text.Trim(),Int(CallbackPort.Text),LMCConnection.DefaultEventMask);});}
        private void Close_Click(object s,RoutedEventArgs e){Run("CloseConnection",()=>{_connection?.CloseConnection();_connection=null;});}
        private void TargetMode_SelectionChanged(object s,SelectionChangedEventArgs e){if(ApiOperation!=null)FillApis();}
        private void FillApis(){if(ApiOperation==null)return;ApiOperation.Items.Clear();var group=Convert.ToString(TargetMode.SelectedItem)==Group;var items=group?new[]{"GetMembers","ReadMemberStatus","PowerOnMembers","PowerOffMembers","GroupEnable","GroupDisable","GroupReset","GroupStop","GroupReadStatus","MoveLinearAbsoluteEx"}:new[]{"PowerOn","PowerOff","Reset","Stop","ReadStatus","ReadPosition","MoveAbsoluteEx","MoveRelativeEx","MoveVelocityEx"};foreach(var x in items)ApiOperation.Items.Add(x);ApiOperation.SelectedIndex=0;}
        private void Execute_Click(object s,RoutedEventArgs e){if(_connection==null){Write("Not connected.");return;}if(Convert.ToString(TargetMode.SelectedItem)==Group)ExecuteGroup();else ExecuteAxis();}
        private void ExecuteAxis(){var op=Convert.ToString(ApiOperation.SelectedItem);Run(op,()=>{var a=new LMCAxis(_connection,AxisName.Text.Trim());LMC_Response r;switch(op){case"PowerOn":r=a.PowerOn();break;case"PowerOff":r=a.PowerOff();break;case"Reset":r=a.Reset();break;case"Stop":r=a.Stop(U(Deceleration.Text),U(Jerk.Text));break;case"ReadStatus":var st=a.ReadStatus(out r);Write("Status=0x"+st.ToString("X"));break;case"ReadPosition":var p=a.GetActualPosition(out r);Write("Position counts="+p+", rev="+(p/Cpr()));break;case"MoveAbsoluteEx":r=a.MoveAbsoluteEx(U(Position.Text),U(Velocity.Text),U(Acceleration.Text),U(Deceleration.Text),U(Jerk.Text));break;case"MoveRelativeEx":r=a.MoveRelativeEx(U(Position.Text),U(Velocity.Text),U(Acceleration.Text),U(Deceleration.Text),U(Jerk.Text));break;case"MoveVelocityEx":r=a.MoveVelocityEx(U(Velocity.Text),U(Acceleration.Text),U(Deceleration.Text),U(Jerk.Text),(LMC_DIRECTION)Direction.SelectedItem);break;default:throw new InvalidOperationException(op);}Result(r);});}
        private void ExecuteGroup()
        {
            var op=Convert.ToString(ApiOperation.SelectedItem);
            Run(op,()=>
            {
                var g=new LMCGroup(_connection,GroupName.Text.Trim());
                var axes=MemberNames.Text.Split(',').Select(x=>new LMCAxis(_connection,x.Trim())).ToArray();
                LMC_Response r=null;
                switch(op)
                {
                    case "GetMembers": r=g.GetGroupMembersInfo(); break;
                    case "ReadMemberStatus":
                        foreach(var axis in axes){LMC_Response memberResponse;var state=axis.ReadStatus(out memberResponse);Write(axis.AxisName+" Ref="+axis.AxisReference+" Status=0x"+state.ToString("X"));}
                        return;
                    case "PowerOnMembers": PowerMembersWithNames(axes,true); return;
                    case "PowerOffMembers": PowerMembersWithNames(axes,false); return;
                    case "GroupEnable": r=g.GroupEnable(); break;
                    case "GroupDisable": r=g.GroupDisable(); break;
                    case "GroupReset": r=g.GroupReset(); break;
                    case "GroupStop": r=g.GroupStop(U(Deceleration.Text),U(Jerk.Text)); break;
                    case "GroupReadStatus": var st=g.GroupReadStatus(out r);Write("GroupStatus=0x"+st.ToString("X"));break;
                    case "MoveLinearAbsoluteEx": r=g.MoveLinearAbsoluteEx(GroupPositions.Text.Split(',').Select(U).ToArray(),U(Velocity.Text),U(Acceleration.Text),U(Deceleration.Text),U(Jerk.Text));break;
                    default: throw new InvalidOperationException(op);
                }
                Result(r);
            });
        }

        private void PowerMembersWithNames(LMCAxis[] axes,bool enable)
        {
            foreach(var axis in axes)
            {
                var response=enable?axis.PowerOn():axis.PowerOff();
                Write(axis.AxisName+" Ref="+axis.AxisReference+" "+(enable?"PowerOn":"PowerOff"));
                Result(response);
                WaitForPowerState(axis,enable,3000);
            }
        }

        private void WaitForPowerState(LMCAxis axis,bool enabled,int timeoutMs)
        {
            var started=Environment.TickCount;
            while(Environment.TickCount-started<timeoutMs)
            {
                Thread.Sleep(50);
                LMC_Response response;
                var state=axis.ReadStatus(out response);
                var disabled=(state&0x00000200u)!=0;
                if((enabled&&!disabled)||(!enabled&&disabled))
                {
                    Write(axis.AxisName+" Power "+(enabled?"ON":"OFF")+" verified, Status=0x"+state.ToString("X"));
                    return;
                }
            }
            throw new TimeoutException(axis.AxisName+" Power "+(enabled?"ON":"OFF")+" verification timeout.");
        }

        private void WaitForStandStill(LMCAxis[] axes,int timeoutMs,string stage)
        {
            foreach(var axis in axes)
            {
                var started=Environment.TickCount;
                uint lastState=0;
                while(Environment.TickCount-started<timeoutMs)
                {
                    LMC_Response response;
                    lastState=axis.ReadStatus(out response);
                    if((lastState&0x00000080u)!=0 && (lastState&0x00000200u)==0)
                    {
                        Write(axis.AxisName+" StandStill verified "+stage+", Status=0x"+lastState.ToString("X"));
                        break;
                    }
                    Thread.Sleep(50);
                }
                if((lastState&0x00000080u)==0 || (lastState&0x00000200u)!=0)
                    throw new TimeoutException(axis.AxisName+" is not enabled StandStill "+stage+", Status=0x"+lastState.ToString("X"));
            }
        }
        private void Result(LMC_Response r){if(r==null)return;Write("Response Status="+r.Status+", ErrorId="+r.ErrorId+", Bytes="+(r.Raw==null?0:r.Raw.Length));if(r.Status!=0)throw new InvalidOperationException("Controller rejected command. Status="+r.Status+", ErrorId="+r.ErrorId);}
        private void Run(string n,Action a){try{a();Write(n+" completed.");}catch(Exception ex){Write(n+" failed: "+ex.Message);}}
        private void Write(string s){Log.AppendText("["+DateTime.Now.ToString("HH:mm:ss.fff")+"] "+s+Environment.NewLine);Log.ScrollToEnd();}
        private double Cpr(){return D(CountsPerRev.Text);}private int U(string s){return checked((int)Math.Round(D(s)*Cpr()));}private static double D(string s){return double.Parse(s.Trim(),CultureInfo.InvariantCulture);}private static int Int(string s){return int.Parse(s.Trim(),CultureInfo.InvariantCulture);}
    }
}

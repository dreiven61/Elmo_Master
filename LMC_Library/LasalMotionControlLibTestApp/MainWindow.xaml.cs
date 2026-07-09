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
        private const string SingleTarget = "Single Axis";
        private const string GroupTarget = "Coordinated Group";

        private static readonly string[] AxisOperations =
        {
            "PowerOn",
            "PowerOff",
            "Reset",
            "Stop",
            "ReadStatus",
            "ReadPosition",
            "MoveAbsoluteEx",
            "MoveRelativeEx",
            "MoveVelocityEx"
        };

        private static readonly string[] GroupOperations =
        {
            "GetMembers",
            "ReadMemberStatus",
            "PowerOnMembers",
            "PowerOffMembers",
            "GroupEnable",
            "GroupDisable",
            "GroupReset",
            "GroupStop",
            "GroupReadStatus",
            "MoveLinearAbsoluteEx"
        };

        private LMCConnection _connection;

        public MainWindow()
        {
            InitializeComponent();

            TargetMode.Items.Add(SingleTarget);
            TargetMode.Items.Add(GroupTarget);
            TargetMode.SelectedIndex = 0;

            Direction.Items.Add(LMC_DIRECTION.Positive);
            Direction.Items.Add(LMC_DIRECTION.Negative);
            Direction.SelectedIndex = 0;

            FillApis();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Run(
                "RpcInitConnection",
                () =>
                {
                    _connection?.Dispose();
                    _connection = new LMCConnection();
                    _connection.RpcInitConnection(
                        RemoteIp.Text.Trim(),
                        Int(RemotePort.Text),
                        LocalIp.Text.Trim(),
                        Int(CallbackPort.Text),
                        LMCConnection.DefaultEventMask);
                });
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Run(
                "CloseConnection",
                () =>
                {
                    _connection?.CloseConnection();
                    _connection = null;
                });
        }

        private void TargetMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApiOperation != null)
            {
                FillApis();
            }
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            if (_connection == null)
            {
                Write("Not connected.");
                return;
            }

            if (Convert.ToString(TargetMode.SelectedItem) == GroupTarget)
            {
                ExecuteGroup();
            }
            else
            {
                ExecuteAxis();
            }
        }

        private void FillApis()
        {
            if (ApiOperation == null)
            {
                return;
            }

            ApiOperation.Items.Clear();

            var operations = Convert.ToString(TargetMode.SelectedItem) == GroupTarget
                ? GroupOperations
                : AxisOperations;

            foreach (var operation in operations)
            {
                ApiOperation.Items.Add(operation);
            }

            ApiOperation.SelectedIndex = 0;
        }

        private void ExecuteAxis()
        {
            var operation = Convert.ToString(ApiOperation.SelectedItem);

            Run(
                operation,
                () =>
                {
                    var axis = new LMCAxis(_connection, AxisName.Text.Trim());
                    LMC_Response response;

                    switch (operation)
                    {
                        case "PowerOn":
                            response = axis.PowerOn();
                            break;

                        case "PowerOff":
                            response = axis.PowerOff();
                            break;

                        case "Reset":
                            response = axis.Reset();
                            break;

                        case "Stop":
                            response = axis.Stop(U(Deceleration.Text), U(Jerk.Text));
                            break;

                        case "ReadStatus":
                            var status = axis.ReadStatus(out response);
                            Write("Status=0x" + status.ToString("X"));
                            break;

                        case "ReadPosition":
                            var position = axis.GetActualPosition(out response);
                            Write("Position counts=" + position + ", rev=" + (position / Cpr()));
                            break;

                        case "MoveAbsoluteEx":
                            response = axis.MoveAbsoluteEx(
                                U(Position.Text),
                                U(Velocity.Text),
                                U(Acceleration.Text),
                                U(Deceleration.Text),
                                U(Jerk.Text));
                            break;

                        case "MoveRelativeEx":
                            response = axis.MoveRelativeEx(
                                U(Position.Text),
                                U(Velocity.Text),
                                U(Acceleration.Text),
                                U(Deceleration.Text),
                                U(Jerk.Text));
                            break;

                        case "MoveVelocityEx":
                            response = axis.MoveVelocityEx(
                                U(Velocity.Text),
                                U(Acceleration.Text),
                                U(Deceleration.Text),
                                U(Jerk.Text),
                                (LMC_DIRECTION)Direction.SelectedItem);
                            break;

                        default:
                            throw new InvalidOperationException(operation);
                    }

                    Result(response);
                });
        }

        private void ExecuteGroup()
        {
            var operation = Convert.ToString(ApiOperation.SelectedItem);

            Run(
                operation,
                () =>
                {
                    var group = new LMCGroup(_connection, GroupName.Text.Trim());
                    var axes = MemberNames.Text
                        .Split(',')
                        .Select(name => new LMCAxis(_connection, name.Trim()))
                        .ToArray();

                    LMC_Response response = null;

                    switch (operation)
                    {
                        case "GetMembers":
                            response = group.GetGroupMembersInfo();
                            break;

                        case "ReadMemberStatus":
                            ReadMemberStatus(axes);
                            return;

                        case "PowerOnMembers":
                            PowerMembersWithNames(axes, true);
                            return;

                        case "PowerOffMembers":
                            PowerMembersWithNames(axes, false);
                            return;

                        case "GroupEnable":
                            response = group.GroupEnable();
                            break;

                        case "GroupDisable":
                            response = group.GroupDisable();
                            break;

                        case "GroupReset":
                            response = group.GroupReset();
                            break;

                        case "GroupStop":
                            response = group.GroupStop(U(Deceleration.Text), U(Jerk.Text));
                            break;

                        case "GroupReadStatus":
                            var status = group.GroupReadStatus(out response);
                            Write("GroupStatus=0x" + status.ToString("X"));
                            break;

                        case "MoveLinearAbsoluteEx":
                            response = group.MoveLinearAbsoluteEx(
                                GroupPositions.Text.Split(',').Select(U).ToArray(),
                                U(Velocity.Text),
                                U(Acceleration.Text),
                                U(Deceleration.Text),
                                U(Jerk.Text));
                            break;

                        default:
                            throw new InvalidOperationException(operation);
                    }

                    Result(response);
                });
        }

        private void ReadMemberStatus(LMCAxis[] axes)
        {
            foreach (var axis in axes)
            {
                LMC_Response response;
                var state = axis.ReadStatus(out response);
                Write(axis.AxisName + " Ref=" + axis.AxisReference + " Status=0x" + state.ToString("X"));
            }
        }

        private void PowerMembersWithNames(LMCAxis[] axes, bool enable)
        {
            foreach (var axis in axes)
            {
                var response = enable
                    ? axis.PowerOn()
                    : axis.PowerOff();

                Write(axis.AxisName + " Ref=" + axis.AxisReference + " " + (enable ? "PowerOn" : "PowerOff"));
                Result(response);
                WaitForPowerState(axis, enable, 3000);
            }
        }

        private void WaitForPowerState(LMCAxis axis, bool enabled, int timeoutMs)
        {
            var started = Environment.TickCount;

            while (Environment.TickCount - started < timeoutMs)
            {
                Thread.Sleep(50);

                LMC_Response response;
                var state = axis.ReadStatus(out response);
                var disabled = (state & 0x00000200u) != 0;

                if ((enabled && !disabled) || (!enabled && disabled))
                {
                    Write(axis.AxisName + " Power " + (enabled ? "ON" : "OFF") + " verified, Status=0x" + state.ToString("X"));
                    return;
                }
            }

            throw new TimeoutException(axis.AxisName + " Power " + (enabled ? "ON" : "OFF") + " verification timeout.");
        }

        private void WaitForStandStill(LMCAxis[] axes, int timeoutMs, string stage)
        {
            foreach (var axis in axes)
            {
                var started = Environment.TickCount;
                uint lastState = 0;

                while (Environment.TickCount - started < timeoutMs)
                {
                    LMC_Response response;
                    lastState = axis.ReadStatus(out response);

                    if ((lastState & 0x00000080u) != 0 && (lastState & 0x00000200u) == 0)
                    {
                        Write(axis.AxisName + " StandStill verified " + stage + ", Status=0x" + lastState.ToString("X"));
                        break;
                    }

                    Thread.Sleep(50);
                }

                if ((lastState & 0x00000080u) == 0 || (lastState & 0x00000200u) != 0)
                {
                    throw new TimeoutException(
                        axis.AxisName + " is not enabled StandStill " + stage + ", Status=0x" + lastState.ToString("X"));
                }
            }
        }

        private void Result(LMC_Response response)
        {
            if (response == null)
            {
                return;
            }

            Write(
                "Response Status=" + response.Status
                + ", ErrorId=" + response.ErrorId
                + ", Bytes=" + (response.Raw == null ? 0 : response.Raw.Length));

            if (response.Status != 0)
            {
                throw new InvalidOperationException(
                    "Controller rejected command. Status=" + response.Status + ", ErrorId=" + response.ErrorId);
            }
        }

        private void Run(string operation, Action action)
        {
            try
            {
                action();
                Write(operation + " completed.");
            }
            catch (Exception ex)
            {
                Write(operation + " failed: " + ex.Message);
            }
        }

        private void Write(string message)
        {
            Log.AppendText("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message + Environment.NewLine);
            Log.ScrollToEnd();
        }

        private double Cpr()
        {
            return D(CountsPerRev.Text);
        }

        private int U(string value)
        {
            return checked((int)Math.Round(D(value) * Cpr()));
        }

        private static double D(string value)
        {
            return double.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }

        private static int Int(string value)
        {
            return int.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }
    }
}

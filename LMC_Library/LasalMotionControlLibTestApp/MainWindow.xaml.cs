using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            "GroupReadActualPosition",
            "MoveLinearAbsoluteEx",
            "SetKinTransformCartesian4Axis"
        };

        private static readonly HashSet<string> UnsupportedPlcOperations =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "GroupReset",
                "GroupStop",
                "GroupReadActualPosition",
                "MoveLinearAbsoluteEx",
                "SetKinTransformCartesian4Axis"
            };

        private static readonly HashSet<string> PcWorkflowOperations =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "ReadMemberStatus",
                "PowerOnMembers",
                "PowerOffMembers"
            };

        private static readonly HashSet<string> HazardousPlcOperations =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "PowerOn",
                "Reset",
                "MoveAbsoluteEx",
                "MoveRelativeEx",
                "MoveVelocityEx",
                "PowerOnMembers",
                "GroupEnable",
                "GroupReset",
                "MoveLinearAbsoluteEx",
                "SetKinTransformCartesian4Axis"
            };

        private static readonly HashSet<string> ReadOnlyPlcOperations =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "ReadStatus",
                "ReadPosition",
                "GetMembers",
                "ReadMemberStatus",
                "GroupReadStatus",
                "GroupReadActualPosition"
            };

        private LMCConnection _connection;
        private CancellationTokenSource _operationCancellation;
        private bool _operationAllowsCancellation;
        private string _currentOperation;
        private bool _windowClosing;
        private bool _shutdownInProgress;
        private bool _allowWindowClose;
        private bool _velocityMotionMayBeActive;
        private string _velocityMotionAxisName;

        public MainWindow()
        {
            InitializeComponent();

            TargetMode.Items.Add(SingleTarget);
            TargetMode.Items.Add(GroupTarget);
            TargetMode.SelectedIndex = 0;

            Direction.Items.Add(LMC_DIRECTION.Positive);
            Direction.Items.Add(LMC_DIRECTION.Negative);
            Direction.SelectedIndex = 0;

            FillEnum(
                ReadCoordinateSystem,
                LMC_COORD_SYSTEM.Mcs);
            FillEnum(
                MotionCoordinateSystem,
                LMC_COORD_SYSTEM.None);
            FillEnum(
                TransitionMode,
                LMC_GROUP_TRANSITION_MODE.ExactStop);
            FillEnum(
                BufferMode,
                LMC_BUFFER_MODE.Aborting);

            FillApis();
            UpdateUiState();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            await RunAsync(
                "RpcInitConnection",
                async cancellationToken =>
                {
                    if (_velocityMotionMayBeActive)
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked because MoveVelocity may still be active on "
                            + _velocityMotionAxisName
                            + ". Send Stop or PowerOff first.");
                    }

                    var remoteAddress = RemoteIp.Text.Trim();
                    var remotePort = Int(RemotePort.Text);
                    var localAddress = LocalIp.Text.Trim();
                    var callbackPort = Int(CallbackPort.Text);

                    if (_connection != null)
                    {
                        try
                        {
                            await CloseCurrentConnectionAsync(cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            Write("Previous connection close warning: " + ex.Message);
                        }
                    }

                    var connection = new LMCConnection();
                    AttachConnection(connection);
                    _connection = connection;
                    UpdateUiState();

                    await connection.RpcInitConnectionAsync(
                        remoteAddress,
                        remotePort,
                        localAddress,
                        callbackPort,
                        LMCConnection.DefaultEventMask,
                        cancellationToken);

                    Write(
                        "RPC initialized. Callback="
                        + connection.CallbackLocalEndPoint
                        + ", EventMask=0x"
                        + connection.EventMask.ToString("X8"));
                });
        }

        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            await RunAsync(
                "CloseConnection",
                CloseCurrentConnectionAsync);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var cancellation = _operationCancellation;
            if (cancellation == null || cancellation.IsCancellationRequested)
            {
                return;
            }

            if (!_operationAllowsCancellation)
            {
                Write(
                    "Cancel is blocked while safety-critical operation "
                    + _currentOperation
                    + " is running. Wait for completion and verify controller state.");
                return;
            }

            Write(
                "Cancellation requested for "
                + _currentOperation
                + ". This aborts the PC transport/wait and does not send PLC Stop. "
                + "If a command was transmitted, its PLC outcome is unknown.");
            cancellation.Cancel();
        }

        private async void StopVelocity_Click(object sender, RoutedEventArgs e)
        {
            var connection = _connection;
            var axisName = _velocityMotionAxisName;
            if (connection == null || !connection.IsConnected)
            {
                Write(
                    "Cannot send Stop: the PLC connection is not active. "
                    + "Use the physical safety stop if motion is possible.");
                return;
            }

            if (!_velocityMotionMayBeActive || string.IsNullOrWhiteSpace(axisName))
            {
                Write("No MoveVelocity command is marked active by this test app.");
                return;
            }

            await RunAsync(
                "Stop active velocity " + axisName,
                async cancellationToken =>
                {
                    var axis = await CreateAxisAsync(
                        connection,
                        axisName,
                        cancellationToken);
                    var unitMultiplier = ParseUnitMultiplier(UnitMultiplier.Text);
                    var response = await axis.StopAsync(
                        ToLasalDint(Deceleration.Text, unitMultiplier),
                        ToLasalDint(Jerk.Text, unitMultiplier),
                        cancellationToken);
                    Result(response);
                    await WaitForStandstillAsync(
                        axis,
                        5000,
                        cancellationToken);
                    ClearVelocityMotion(axisName, "Stop active velocity");
                },
                false);
        }

        private void TargetMode_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            DisarmHazardousCommand("Target mode changed");
            if (ApiOperation != null)
            {
                FillApis();
            }
        }

        private void ApiOperation_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            DisarmHazardousCommand("API selection changed");
            UpdateOperationSupport();
        }

        private void TargetIdentity_Changed(
            object sender,
            TextChangedEventArgs e)
        {
            DisarmHazardousCommand("Target identity changed");
        }

        private void AllowUnsupportedNegativeTest_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateOperationSupport();
        }

        private void AllowHazardousCommand_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateOperationSupport();
        }

        private async void Execute_Click(object sender, RoutedEventArgs e)
        {
            var operation = Convert.ToString(ApiOperation.SelectedItem);
            var hazardousCommandWasArmed =
                AllowHazardousCommand.IsChecked == true;
            DisarmHazardousCommand("Execution attempted");

            var connection = _connection;
            if (connection == null || !connection.IsConnected)
            {
                Write("Not connected.");
                return;
            }

            if (IsUnsupportedPlcOperation(operation)
                && AllowUnsupportedNegativeTest.IsChecked != true)
            {
                Write(
                    operation
                    + " is unsupported by the current PLC interface. "
                    + "Enable the explicit -5 negative-test option to send it.");
                return;
            }

            var requiresLiveArm = RequiresHazardousCommandArm(operation);
            if (_velocityMotionMayBeActive && requiresLiveArm)
            {
                Write(
                    "Another live command is blocked while MoveVelocity may be active on "
                    + _velocityMotionAxisName
                    + ". Send Stop or PowerOff first.");
                return;
            }

            if (requiresLiveArm && !hazardousCommandWasArmed)
            {
                Write(
                    operation
                    + " can enable power/motion or change controller state. "
                    + "Arm the explicit hazardous-command option first.");
                return;
            }

            if (requiresLiveArm)
            {
                var confirmation = MessageBox.Show(
                    this,
                    "LIVE PLC COMMAND: "
                    + operation
                    + Environment.NewLine
                    + "Confirm physical E-stop, travel limits, low test values, and clear machine area.",
                    "Confirm live power or motion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (confirmation != MessageBoxResult.Yes)
                {
                    Write(operation + " cancelled before transmission.");
                    return;
                }
            }

            await RunAsync(
                operation,
                cancellationToken =>
                {
                    var input = CaptureOperationInput();
                    return input.IsGroup
                        ? ExecuteGroupAsync(connection, input, cancellationToken)
                        : ExecuteAxisAsync(connection, input, cancellationToken);
                },
                ReadOnlyPlcOperations.Contains(operation ?? string.Empty));
        }

        private OperationInput CaptureOperationInput()
        {
            return new OperationInput
            {
                IsGroup = Convert.ToString(TargetMode.SelectedItem) == GroupTarget,
                Operation = Convert.ToString(ApiOperation.SelectedItem),
                AxisName = AxisName.Text.Trim(),
                GroupName = GroupName.Text.Trim(),
                MemberNames = MemberNames.Text
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(name => name.Trim())
                    .Where(name => name.Length > 0)
                    .ToArray(),
                Position = Position.Text,
                Velocity = Velocity.Text,
                Acceleration = Acceleration.Text,
                Deceleration = Deceleration.Text,
                Jerk = Jerk.Text,
                GroupPositions = GroupPositions.Text,
                UnitMultiplier = ParseUnitMultiplier(UnitMultiplier.Text),
                Direction = (LMC_DIRECTION)Direction.SelectedItem,
                ReadCoordinateSystem =
                    (LMC_COORD_SYSTEM)ReadCoordinateSystem.SelectedItem,
                MotionOptions = new LMCGroupMotionOptions
                {
                    CoordinateSystem =
                        (LMC_COORD_SYSTEM)MotionCoordinateSystem.SelectedItem,
                    TransitionMode =
                        (LMC_GROUP_TRANSITION_MODE)TransitionMode.SelectedItem,
                    BufferMode = (LMC_BUFFER_MODE)BufferMode.SelectedItem,
                    Execute = true
                },
                ExpectUnsupported =
                    IsUnsupportedPlcOperation(
                        Convert.ToString(ApiOperation.SelectedItem))
                    && AllowUnsupportedNegativeTest.IsChecked == true
            };
        }

        private async Task ExecuteAxisAsync(
            LMCConnection connection,
            OperationInput input,
            CancellationToken cancellationToken)
        {
            var axis = await CreateAxisAsync(
                connection,
                input.AxisName,
                cancellationToken);

            LMC_Response response;

            switch (input.Operation)
            {
                case "PowerOn":
                    response = await axis.PowerOnAsync(cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "PowerOff":
                    response = await axis.PowerOffAsync(cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    await WaitForPowerStateAsync(
                        axis,
                        false,
                        3000,
                        cancellationToken);
                    await WaitForStandstillAsync(
                        axis,
                        3000,
                        cancellationToken);
                    ClearVelocityMotion(input.AxisName, "PowerOff");
                    break;

                case "Reset":
                    response = await axis.ResetAsync(cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "Stop":
                    response = await axis.StopAsync(
                        ToLasalDint(input.Deceleration, input.UnitMultiplier),
                        ToLasalDint(input.Jerk, input.UnitMultiplier),
                        cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    if (IsTrackedVelocityAxis(input.AxisName))
                    {
                        await WaitForStandstillAsync(
                            axis,
                            5000,
                            cancellationToken);
                        ClearVelocityMotion(input.AxisName, "Stop");
                    }
                    break;

                case "ReadStatus":
                    var status = await axis.ReadStatusResultAsync(cancellationToken);
                    EnsureReadStatusSuccess("ReadStatus", status);
                    Write("Status=0x" + status.State.ToString("X"));
                    break;

                case "ReadPosition":
                    var position = await axis.GetActualPositionResultAsync(
                        cancellationToken);
                    EnsureReadPositionSuccess("ReadPosition", position);
                    Write(
                        "Position raw="
                        + position.PositionRaw
                        + ", engineering="
                        + (position.PositionRaw / input.UnitMultiplier).ToString(
                            "0.########",
                            CultureInfo.InvariantCulture));
                    break;

                case "MoveAbsoluteEx":
                    response = await axis.MoveAbsoluteExAsync(
                        ToLasalDint(input.Position, input.UnitMultiplier),
                        ToLasalDint(input.Velocity, input.UnitMultiplier),
                        ToLasalDint(input.Acceleration, input.UnitMultiplier),
                        ToLasalDint(input.Deceleration, input.UnitMultiplier),
                        ToLasalDint(input.Jerk, input.UnitMultiplier),
                        LMC_DIRECTION.Shortest,
                        cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "MoveRelativeEx":
                    response = await axis.MoveRelativeExAsync(
                        ToLasalDint(input.Position, input.UnitMultiplier),
                        ToLasalDint(input.Velocity, input.UnitMultiplier),
                        ToLasalDint(input.Acceleration, input.UnitMultiplier),
                        ToLasalDint(input.Deceleration, input.UnitMultiplier),
                        ToLasalDint(input.Jerk, input.UnitMultiplier),
                        LMC_DIRECTION.Shortest,
                        cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "MoveVelocityEx":
                    var velocity = ToLasalDint(
                        input.Velocity,
                        input.UnitMultiplier);
                    var acceleration = ToLasalDint(
                        input.Acceleration,
                        input.UnitMultiplier);
                    var jerk = ToLasalDint(
                        input.Jerk,
                        input.UnitMultiplier);

                    MarkVelocityMotionUncertain(input.AxisName);
                    response = await axis.MoveVelocityExAsync(
                        velocity,
                        acceleration,
                        0,
                        jerk,
                        input.Direction,
                        cancellationToken);

                    if (response != null
                        && response.IsFrameValid
                        && !response.IsSuccess)
                    {
                        ClearVelocityMotion(
                            input.AxisName,
                            "confirmed MoveVelocity rejection");
                    }

                    Result(response, input.ExpectUnsupported);
                    Write(
                        "SAFETY: MoveVelocity accepted for "
                        + input.AxisName
                        + ". Use Stop active velocity or the physical safety stop. "
                        + "CloseConnection does not stop motion.");
                    break;

                default:
                    throw new InvalidOperationException(input.Operation);
            }
        }

        private async Task ExecuteGroupAsync(
            LMCConnection connection,
            OperationInput input,
            CancellationToken cancellationToken)
        {
            if (PcWorkflowOperations.Contains(input.Operation ?? string.Empty))
            {
                var axes = await CreateAxesAsync(
                    connection,
                    input.MemberNames,
                    cancellationToken);

                switch (input.Operation)
                {
                    case "ReadMemberStatus":
                        await ReadMemberStatusAsync(axes, cancellationToken);
                        return;

                    case "PowerOnMembers":
                        await PowerMembersWithNamesAsync(
                            axes,
                            true,
                            cancellationToken);
                        return;

                    case "PowerOffMembers":
                        await PowerMembersWithNamesAsync(
                            axes,
                            false,
                            cancellationToken);
                        return;
                }
            }

            var group = await CreateGroupAsync(
                connection,
                input.GroupName,
                cancellationToken);

            LMC_Response response;

            switch (input.Operation)
            {
                case "GetMembers":
                    var members = await group.GetGroupMembersInfoResultAsync(
                        cancellationToken);
                    EnsureGroupMembersSuccess("GetMembers", members);
                    Write("Member count=" + members.AxisCount);
                    foreach (var member in members.Members)
                    {
                        Write(
                            "Member["
                            + member.Index
                            + "] Name="
                            + member.AxisName
                            + ", Ref="
                            + member.AxisReference
                            + ", DeviceId="
                            + member.DeviceId);
                    }
                    break;

                case "GroupEnable":
                    response = await group.GroupEnableAsync(cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "GroupDisable":
                    response = await group.GroupDisableAsync(cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "GroupReset":
                    response = await group.GroupResetAsync(cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "GroupStop":
                    response = await group.GroupStopAsync(
                        ToLasalDint(input.Deceleration, input.UnitMultiplier),
                        ToLasalDint(input.Jerk, input.UnitMultiplier),
                        cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "GroupReadStatus":
                    var status = await group.GroupReadStatusResultAsync(
                        cancellationToken);
                    EnsureGroupStatusSuccess("GroupReadStatus", status);
                    Write("GroupStatus=0x" + status.State.ToString("X"));
                    break;

                case "GroupReadActualPosition":
                    var positions = await group.GroupReadActualPositionAsync(
                        input.ReadCoordinateSystem,
                        cancellationToken);
                    if (input.ExpectUnsupported)
                    {
                        Result(
                            positions == null ? null : positions.Response,
                            true);
                        break;
                    }

                    EnsureGroupPositionSuccess(
                        "GroupReadActualPosition",
                        positions);
                    Write(
                        "GroupPosition raw "
                        + positions.CoordinateSystem
                        + "=["
                        + string.Join(
                            ",",
                            positions.PositionsRaw.Select(
                                value => value.ToString(
                                    CultureInfo.InvariantCulture)))
                        + "]");
                    Write(
                        "GroupPosition engineering=["
                        + string.Join(
                            ",",
                            positions.PositionsRaw.Select(
                                value => (value / input.UnitMultiplier).ToString(
                                    "0.########",
                                    CultureInfo.InvariantCulture)))
                        + "]");
                    break;

                case "MoveLinearAbsoluteEx":
                    response = await group.MoveLinearAbsoluteExAsync(
                        input.GroupPositions
                            .Split(',')
                            .Select(value => ToLasalDint(
                                value,
                                input.UnitMultiplier))
                            .ToArray(),
                        ToLasalDint(input.Velocity, input.UnitMultiplier),
                        ToLasalDint(input.Acceleration, input.UnitMultiplier),
                        ToLasalDint(input.Deceleration, input.UnitMultiplier),
                        ToLasalDint(input.Jerk, input.UnitMultiplier),
                        input.MotionOptions,
                        cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    break;

                case "SetKinTransformCartesian4Axis":
                    if (input.MemberNames.Length != 4)
                    {
                        throw new InvalidOperationException(
                            "SetKinTransformCartesian4Axis requires exactly four member names ordered X,Y,Z,U.");
                    }

                    var axes = await CreateAxesAsync(
                        connection,
                        input.MemberNames,
                        cancellationToken);
                    response = await group.SetKinTransformCartesian4AxisAsync(
                        axes[0],
                        axes[1],
                        axes[2],
                        axes[3],
                        cancellationToken);
                    Result(response, input.ExpectUnsupported);
                    if (!input.ExpectUnsupported)
                    {
                        Write(
                            "Kinematic mapping X/Y/Z/U="
                            + string.Join(",", input.MemberNames));
                    }
                    break;

                default:
                    throw new InvalidOperationException(input.Operation);
            }
        }

        private async Task ReadMemberStatusAsync(
            LMCSingleAxis[] axes,
            CancellationToken cancellationToken)
        {
            foreach (var axis in axes)
            {
                var result = await axis.ReadStatusResultAsync(cancellationToken);
                EnsureReadStatusSuccess("ReadStatus " + axis.AxisName, result);
                Write(
                    axis.AxisName
                    + " Ref="
                    + axis.AxisReference
                    + " Status=0x"
                    + result.State.ToString("X"));
            }
        }

        private async Task PowerMembersWithNamesAsync(
            LMCSingleAxis[] axes,
            bool enable,
            CancellationToken cancellationToken)
        {
            var rollbackCandidates = new List<LMCSingleAxis>();

            try
            {
                foreach (var axis in axes)
                {
                    if (enable)
                    {
                        // The command outcome becomes uncertain as soon as send can start.
                        rollbackCandidates.Add(axis);
                    }

                    var response = enable
                        ? await axis.PowerOnAsync(cancellationToken)
                        : await axis.PowerOffAsync(cancellationToken);

                    Write(
                        axis.AxisName
                        + " Ref="
                        + axis.AxisReference
                        + " "
                        + (enable ? "PowerOn" : "PowerOff"));
                    Result(response);

                    await WaitForPowerStateAsync(
                        axis,
                        enable,
                        3000,
                        cancellationToken);

                    if (!enable)
                    {
                        await WaitForStandstillAsync(
                            axis,
                            3000,
                            cancellationToken);
                        ClearVelocityMotion(axis.AxisName, "PowerOffMembers");
                    }
                }
            }
            catch (Exception operationError)
            {
                if (enable && rollbackCandidates.Count > 0)
                {
                    Write(
                        "SAFETY: PowerOnMembers did not complete: "
                        + operationError.Message
                        + ". Best-effort PowerOff rollback starts for every member whose "
                        + "PowerOn send may have started, including the current member.");

                    var rollbackUncertain = false;
                    for (var index = rollbackCandidates.Count - 1;
                        index >= 0;
                        index--)
                    {
                        var axis = rollbackCandidates[index];
                        try
                        {
                            var rollback = await axis.PowerOffAsync(
                                CancellationToken.None);
                            EnsureResponseSuccess(
                                "PowerOff rollback " + axis.AxisName,
                                rollback);
                            await WaitForPowerStateAsync(
                                axis,
                                false,
                                3000,
                                CancellationToken.None);
                            await WaitForStandstillAsync(
                                axis,
                                3000,
                                CancellationToken.None);
                            Write(
                                "PowerOff rollback and standstill verified for "
                                + axis.AxisName
                                + ".");
                        }
                        catch (Exception rollbackError)
                        {
                            rollbackUncertain = true;
                            Write(
                                "SAFETY: PowerOff rollback failed for "
                                + axis.AxisName
                                + ": "
                                + rollbackError.Message
                                + ". Power and motion state are UNCERTAIN; use the physical "
                                + "safety stop or drive disable and verify the axis.");
                        }
                    }

                    Write(
                        rollbackUncertain
                            ? "SAFETY: PowerOnMembers rollback is INCOMPLETE. "
                                + "Do not trust software state; verify every member physically."
                            : "PowerOnMembers rollback completed; PowerOff and standstill "
                                + "were verified for all uncertain members.");
                }
                else if (!enable)
                {
                    Write(
                        "SAFETY: PowerOffMembers did not complete: "
                        + operationError.Message
                        + ". One or more member power and motion states may be UNCERTAIN; "
                        + "verify every member physically.");
                }

                throw;
            }
        }

        private async Task WaitForPowerStateAsync(
            LMCSingleAxis axis,
            bool enabled,
            int timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();

            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await axis.ReadStatusResultAsync(cancellationToken);
                EnsureReadStatusSuccess("ReadStatus " + axis.AxisName, result);

                if (result.IsPowerOn == enabled)
                {
                    Write(
                        axis.AxisName
                        + " Power "
                        + (enabled ? "ON" : "OFF")
                        + " verified, Status=0x"
                        + result.State.ToString("X"));
                    return;
                }

                await Task.Delay(50, cancellationToken);
            }

            throw new TimeoutException(
                axis.AxisName
                + " Power "
                + (enabled ? "ON" : "OFF")
                + " verification timeout.");
        }

        private async Task WaitForStandstillAsync(
            LMCSingleAxis axis,
            int timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();

            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await axis.ReadStatusResultAsync(cancellationToken);
                EnsureReadStatusSuccess("ReadStatus " + axis.AxisName, result);
                if (result.IsStandstill || !result.IsPowerOn)
                {
                    Write(
                        axis.AxisName
                        + " standstill verified, Status=0x"
                        + result.State.ToString("X"));
                    return;
                }

                await Task.Delay(50, cancellationToken);
            }

            throw new TimeoutException(
                axis.AxisName + " standstill verification timeout.");
        }

        private static Task<LMCSingleAxis> CreateAxisAsync(
            LMCConnection connection,
            string axisName,
            CancellationToken cancellationToken)
        {
            return LMCSingleAxis.CreateAsync(
                connection,
                axisName,
                cancellationToken);
        }

        private static Task<LMCGroupAxis> CreateGroupAsync(
            LMCConnection connection,
            string groupName,
            CancellationToken cancellationToken)
        {
            return LMCGroupAxis.CreateAsync(
                connection,
                groupName,
                cancellationToken);
        }

        private static async Task<LMCSingleAxis[]> CreateAxesAsync(
            LMCConnection connection,
            string[] axisNames,
            CancellationToken cancellationToken)
        {
            if (axisNames == null || axisNames.Length == 0)
            {
                throw new InvalidOperationException(
                    "At least one group member name is required.");
            }

            var axes = new LMCSingleAxis[axisNames.Length];
            for (var index = 0; index < axisNames.Length; index++)
            {
                axes[index] = await CreateAxisAsync(
                    connection,
                    axisNames[index],
                    cancellationToken);
            }

            return axes;
        }

        private async Task CloseCurrentConnectionAsync(
            CancellationToken cancellationToken)
        {
            var connection = _connection;
            if (connection == null)
            {
                return;
            }

            if (_velocityMotionMayBeActive)
            {
                throw new InvalidOperationException(
                    "CloseConnection is blocked because MoveVelocity may still be active on "
                    + _velocityMotionAxisName
                    + ". Send Stop or PowerOff first.");
            }

            try
            {
                await connection.CloseConnectionAsync(cancellationToken);
            }
            finally
            {
                if (ReferenceEquals(_connection, connection))
                {
                    _connection = null;
                }

                DetachConnection(connection);
                await Task.Run(() => connection.Dispose());
                _velocityMotionMayBeActive = false;
                _velocityMotionAxisName = null;
                UpdateUiState();
            }
        }

        private async Task RunAsync(
            string operation,
            Func<CancellationToken, Task> action,
            bool allowCancellation = true)
        {
            if (_operationCancellation != null)
            {
                Write("Another operation is already running.");
                return;
            }

            var cancellation = new CancellationTokenSource();
            _operationCancellation = cancellation;
            _operationAllowsCancellation = allowCancellation;
            _currentOperation = operation;
            UpdateUiState();

            try
            {
                await action(cancellation.Token);
                Write(operation + " completed.");
            }
            catch (OperationCanceledException)
            {
                Write(
                    operation
                    + " cancelled. PC transport/wait was aborted; if a command was "
                    + "transmitted, its PLC outcome is unknown.");
            }
            catch (Exception ex)
            {
                Write(operation + " failed: " + ex.Message);
            }
            finally
            {
                if (ReferenceEquals(_operationCancellation, cancellation))
                {
                    _operationCancellation = null;
                    _operationAllowsCancellation = false;
                    _currentOperation = null;
                }

                cancellation.Dispose();
                UpdateUiState();
            }
        }

        private void AttachConnection(LMCConnection connection)
        {
            connection.ConnectionStateChanged += Connection_StateChanged;
            connection.CallbackReceived += Connection_CallbackReceived;
            connection.CallbackListenerError += Connection_CallbackListenerError;
        }

        private void DetachConnection(LMCConnection connection)
        {
            connection.ConnectionStateChanged -= Connection_StateChanged;
            connection.CallbackReceived -= Connection_CallbackReceived;
            connection.CallbackListenerError -= Connection_CallbackListenerError;
        }

        private void Connection_StateChanged(
            object sender,
            LMCConnectionStateChangedEventArgs e)
        {
            RunOnUi(
                () =>
                {
                    if (!ReferenceEquals(sender, _connection))
                    {
                        return;
                    }

                    Write(
                        "Connection state "
                        + e.PreviousState
                        + " -> "
                        + e.CurrentState
                        + (e.Exception == null
                            ? string.Empty
                            : ": " + e.Exception.Message));
                    UpdateUiState();
                });
        }

        private void Connection_CallbackReceived(
            object sender,
            LMCCallbackEventArgs e)
        {
            var payload = e.Payload;
            var previewLength = Math.Min(payload.Length, 64);
            var hex = previewLength == 0
                ? string.Empty
                : BitConverter.ToString(payload, 0, previewLength);

            if (payload.Length > previewLength)
            {
                hex += "-...";
            }

            RunOnUi(
                () =>
                {
                    if (!ReferenceEquals(sender, _connection))
                    {
                        return;
                    }

                    Write(
                        "Callback UTC="
                        + e.ReceivedAtUtc.ToString(
                            "HH:mm:ss.fff",
                            CultureInfo.InvariantCulture)
                        + ", From="
                        + e.RemoteEndPoint
                        + ", Bytes="
                        + payload.Length
                        + ", Hex="
                        + hex);
                    UpdateUiState();
                });
        }

        private void Connection_CallbackListenerError(
            object sender,
            LMCCallbackErrorEventArgs e)
        {
            RunOnUi(
                () =>
                {
                    if (!ReferenceEquals(sender, _connection))
                    {
                        return;
                    }

                    Write(
                        "Callback listener error: "
                        + (e.Exception == null
                            ? "unknown error"
                            : e.Exception.Message));
                    UpdateUiState();
                });
        }

        private void UpdateUiState()
        {
            if (_windowClosing)
            {
                return;
            }

            var busy = _operationCancellation != null;
            var connection = _connection;
            var selectedOperation = Convert.ToString(ApiOperation.SelectedItem);
            var operationAllowed =
                !IsUnsupportedPlcOperation(selectedOperation)
                || AllowUnsupportedNegativeTest.IsChecked == true;
            var liveOperationAllowed =
                !RequiresHazardousCommandArm(selectedOperation)
                || AllowHazardousCommand.IsChecked == true;

            ConnectButton.IsEnabled = !busy;
            CloseButton.IsEnabled = !busy && connection != null;
            ExecuteButton.IsEnabled =
                !busy
                && connection != null
                && connection.IsConnected
                && operationAllowed
                && liveOperationAllowed;
            CancelButton.IsEnabled = busy && _operationAllowsCancellation;
            StopVelocityButton.IsEnabled =
                !busy
                && connection != null
                && connection.IsConnected
                && _velocityMotionMayBeActive;

            ConnectionStateText.Text = connection == null
                ? LMCConnectionState.Disconnected.ToString()
                : connection.State.ToString();

            CallbackStateText.Text = connection == null
                ? "Stopped"
                : (connection.IsCallbackListenerRunning
                    ? "Listening "
                        + connection.CallbackLocalEndPoint
                        + ", Rejected="
                        + connection.RejectedCallbackCount
                    : "Stopped, Rejected="
                        + connection.RejectedCallbackCount);
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
            UpdateOperationSupport();
        }

        private void UpdateOperationSupport()
        {
            if (ApiOperation == null
                || ApiSupportText == null
                || AllowUnsupportedNegativeTest == null
                || AllowHazardousCommand == null)
            {
                return;
            }

            var operation = Convert.ToString(ApiOperation.SelectedItem);
            if (IsUnsupportedPlcOperation(operation))
            {
                ApiSupportText.Text =
                    "PLC status: unsupported by the current TCPMotionInterface; "
                    + "the expected controller result is -5. "
                    + "Use the checkbox below only for a negative protocol test.";
            }
            else if (PcWorkflowOperations.Contains(operation ?? string.Empty))
            {
                ApiSupportText.Text =
                    "PC workflow: combines supported lookup and single-axis API calls.";
            }
            else
            {
                ApiSupportText.Text =
                    "PLC status: active in the current TCPMotionInterface.";
            }

            if (RequiresHazardousCommandArm(operation))
            {
                ApiSupportText.Text +=
                    " Safety gate: this command can enable power/motion or change controller state; "
                    + "explicit arm and confirmation are required.";
            }

            UpdateUiState();
        }

        private static bool IsUnsupportedPlcOperation(string operation)
        {
            return UnsupportedPlcOperations.Contains(
                operation ?? string.Empty);
        }

        private static bool RequiresHazardousCommandArm(string operation)
        {
            return HazardousPlcOperations.Contains(
                operation ?? string.Empty);
        }

        private void DisarmHazardousCommand(string reason)
        {
            if (AllowHazardousCommand == null
                || AllowHazardousCommand.IsChecked != true)
            {
                return;
            }

            AllowHazardousCommand.IsChecked = false;
            Write("Hazardous-command arm cleared: " + reason + ".");
        }

        private void MarkVelocityMotionUncertain(string axisName)
        {
            _velocityMotionMayBeActive = true;
            _velocityMotionAxisName = axisName;
            Write(
                "SAFETY: MoveVelocity send may start for "
                + axisName
                + "; motion state is now UNCERTAIN until confirmed rejection or "
                + "verified Stop/PowerOff and standstill.");
            UpdateUiState();
        }

        private bool IsTrackedVelocityAxis(string axisName)
        {
            return _velocityMotionMayBeActive
                && string.Equals(
                    _velocityMotionAxisName,
                    axisName,
                    StringComparison.Ordinal);
        }

        private void ClearVelocityMotion(string axisName, string reason)
        {
            if (!IsTrackedVelocityAxis(axisName))
            {
                return;
            }

            _velocityMotionMayBeActive = false;
            _velocityMotionAxisName = null;
            Write("Velocity-motion warning cleared after " + reason + ".");
            UpdateUiState();
        }

        private static void FillEnum<T>(ComboBox comboBox, T selectedValue)
        {
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                comboBox.Items.Add(value);
            }

            comboBox.SelectedItem = selectedValue;
        }

        private void Result(
            LMC_Response response,
            bool expectUnsupported = false)
        {
            if (response == null)
            {
                throw new InvalidOperationException(
                    "Controller command failed without a response.");
            }

            Write(
                "Response Status="
                + response.Status
                + ", ErrorId="
                + response.ErrorId
                + ", Bytes="
                + (response.Raw == null ? 0 : response.Raw.Length));

            if (expectUnsupported)
            {
                EnsureExpectedUnsupported("Controller command", response);
                Write("PASS: controller returned the expected unsupported ErrorId=-5.");
                return;
            }

            EnsureResponseSuccess("Controller command", response);
        }

        private static void EnsureExpectedUnsupported(
            string operation,
            LMC_Response response)
        {
            if (response == null)
            {
                throw new InvalidOperationException(
                    operation + " negative test failed without a response.");
            }

            if (!response.IsFrameValid || response.ErrorId != -5)
            {
                throw new InvalidOperationException(
                    operation
                    + " negative test failed. Expected a valid response with ErrorId=-5; "
                    + "FrameValid="
                    + response.IsFrameValid
                    + ", HeaderStatus="
                    + response.HeaderStatus
                    + ", CommandStatus="
                    + response.CommandStatus
                    + ", ErrorId="
                    + response.ErrorId);
            }
        }

        private static void EnsureResponseSuccess(
            string operation,
            LMC_Response response)
        {
            if (response == null)
            {
                throw new InvalidOperationException(
                    operation + " failed without a response.");
            }

            if (!response.IsFrameValid || !response.IsSuccess)
            {
                throw new InvalidOperationException(
                    operation
                    + " failed. FrameValid="
                    + response.IsFrameValid
                    + ", HeaderStatus="
                    + response.HeaderStatus
                    + ", CommandStatus="
                    + response.CommandStatus
                    + ", ErrorId="
                    + response.ErrorId);
            }
        }

        private static void EnsureReadStatusSuccess(
            string operation,
            LMCReadStatusResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                throw new InvalidOperationException(
                    operation
                    + " failed. AxisErrorId="
                    + (result == null ? 0 : result.AxisErrorId)
                    + ", ErrorId="
                    + (result == null ? 0 : result.ErrorId));
            }

            EnsureResponseSuccess(operation, result.Response);
        }

        private static void EnsureReadPositionSuccess(
            string operation,
            LMCReadActualPositionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                throw new InvalidOperationException(
                    operation
                    + " failed. ErrorId="
                    + (result == null ? 0 : result.ErrorId));
            }

            EnsureResponseSuccess(operation, result.Response);
        }

        private static void EnsureGroupStatusSuccess(
            string operation,
            LMCGroupReadStatusResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                throw new InvalidOperationException(
                    operation
                    + " failed. GroupErrorId="
                    + (result == null ? 0 : result.GroupErrorId)
                    + ", ErrorId="
                    + (result == null ? 0 : result.ErrorId));
            }

            EnsureResponseSuccess(operation, result.Response);
        }

        private static void EnsureGroupPositionSuccess(
            string operation,
            LMCGroupReadActualPositionResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                throw new InvalidOperationException(
                    operation
                    + " failed. ErrorId="
                    + (result == null ? 0 : result.ErrorId));
            }

            EnsureResponseSuccess(operation, result.Response);
        }

        private static void EnsureGroupMembersSuccess(
            string operation,
            LMCGroupMembersInfoResult result)
        {
            if (result == null || !result.IsSuccess)
            {
                throw new InvalidOperationException(
                    operation
                    + " failed. ErrorId="
                    + (result == null ? 0 : result.ErrorId));
            }

            EnsureResponseSuccess(operation, result.Response);
        }

        private void Write(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                RunOnUi(() => Write(message));
                return;
            }

            if (_windowClosing)
            {
                return;
            }

            Log.AppendText(
                "["
                + DateTime.Now.ToString("HH:mm:ss.fff")
                + "] "
                + message
                + Environment.NewLine);
            Log.ScrollToEnd();
        }

        private void RunOnUi(Action action)
        {
            if (_windowClosing || Dispatcher.HasShutdownStarted)
            {
                return;
            }

            if (Dispatcher.CheckAccess())
            {
                action();
                return;
            }

            Dispatcher.BeginInvoke(action);
        }

        private static int ToLasalDint(
            string value,
            double unitMultiplier)
        {
            var engineeringValue = D(value);
            if (double.IsNaN(engineeringValue)
                || double.IsInfinity(engineeringValue))
            {
                throw new InvalidOperationException(
                    "Engineering value must be finite.");
            }

            var scaled = engineeringValue * unitMultiplier;
            if (double.IsNaN(scaled) || double.IsInfinity(scaled))
            {
                throw new OverflowException(
                    "Engineering value multiplied by UNIT is not finite.");
            }

            var rounded = Math.Round(
                scaled,
                0,
                MidpointRounding.AwayFromZero);
            if (rounded < int.MinValue || rounded > int.MaxValue)
            {
                throw new OverflowException(
                    "Engineering value multiplied by UNIT is outside DINT range.");
            }

            return checked((int)rounded);
        }

        private static double ParseUnitMultiplier(string value)
        {
            var unitMultiplier = D(value);
            if (double.IsNaN(unitMultiplier)
                || double.IsInfinity(unitMultiplier)
                || unitMultiplier <= 0)
            {
                throw new InvalidOperationException(
                    "PLC UNIT multiplier must be a positive finite number.");
            }

            return unitMultiplier;
        }

        private static double D(string value)
        {
            return double.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }

        private static int Int(string value)
        {
            return int.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (_allowWindowClose)
            {
                base.OnClosing(e);
                return;
            }

            if (_operationCancellation != null
                && !_operationAllowsCancellation)
            {
                e.Cancel = true;
                Write(
                    "Window close is blocked while safety-critical operation "
                    + _currentOperation
                    + " is running. Closing now could abort transport and leave the "
                    + "PLC command outcome unknown.");
                base.OnClosing(e);
                return;
            }

            if (_velocityMotionMayBeActive)
            {
                var confirmation = MessageBox.Show(
                    this,
                    "MoveVelocity may still be active on "
                    + _velocityMotionAxisName
                    + ". Closing the app does NOT send Stop."
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Select No, send Stop/PowerOff, and verify standstill. "
                    + "Select Yes only after an external physical safety stop or drive disable has been verified.",
                    "Motion may still be active",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Stop,
                    MessageBoxResult.No);
                if (confirmation != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    base.OnClosing(e);
                    return;
                }
            }

            e.Cancel = true;
            base.OnClosing(e);

            if (_shutdownInProgress)
            {
                return;
            }

            _shutdownInProgress = true;
            _windowClosing = true;
            _operationCancellation?.Cancel();

            try
            {
                var connection = _connection;
                _connection = null;
                if (connection != null)
                {
                    DetachConnection(connection);

                    try
                    {
                        await connection.CloseConnectionAsync(
                            CancellationToken.None);
                    }
                    catch
                    {
                        // Window shutdown still owns local transport cleanup.
                    }

                    try
                    {
                        await Task.Run(() => connection.Dispose());
                    }
                    catch
                    {
                        // Local process shutdown is the final transport boundary.
                    }
                }
            }
            finally
            {
                _allowWindowClose = true;
                Close();
            }
        }

        private sealed class OperationInput
        {
            public bool IsGroup { get; set; }
            public string Operation { get; set; }
            public string AxisName { get; set; }
            public string GroupName { get; set; }
            public string[] MemberNames { get; set; }
            public string Position { get; set; }
            public string Velocity { get; set; }
            public string Acceleration { get; set; }
            public string Deceleration { get; set; }
            public string Jerk { get; set; }
            public string GroupPositions { get; set; }
            public double UnitMultiplier { get; set; }
            public LMC_DIRECTION Direction { get; set; }
            public LMC_COORD_SYSTEM ReadCoordinateSystem { get; set; }
            public LMCGroupMotionOptions MotionOptions { get; set; }
            public bool ExpectUnsupported { get; set; }
        }
    }
}

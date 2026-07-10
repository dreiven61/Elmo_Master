using System;
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

        private LMCConnection _connection;
        private CancellationTokenSource _operationCancellation;
        private bool _windowClosing;
        private bool _shutdownInProgress;
        private bool _allowWindowClose;

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

            Write("Cancellation requested.");
            cancellation.Cancel();
        }

        private void TargetMode_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ApiOperation != null)
            {
                FillApis();
            }
        }

        private async void Execute_Click(object sender, RoutedEventArgs e)
        {
            var connection = _connection;
            if (connection == null || !connection.IsConnected)
            {
                Write("Not connected.");
                return;
            }

            var operation = Convert.ToString(ApiOperation.SelectedItem);

            await RunAsync(
                operation,
                cancellationToken =>
                {
                    var input = CaptureOperationInput();
                    return input.IsGroup
                        ? ExecuteGroupAsync(connection, input, cancellationToken)
                        : ExecuteAxisAsync(connection, input, cancellationToken);
                });
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
                CountsPerRev = D(CountsPerRev.Text),
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
                }
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
                    Result(response);
                    break;

                case "PowerOff":
                    response = await axis.PowerOffAsync(cancellationToken);
                    Result(response);
                    break;

                case "Reset":
                    response = await axis.ResetAsync(cancellationToken);
                    Result(response);
                    break;

                case "Stop":
                    response = await axis.StopAsync(
                        ToDummyProfileDint(input.Deceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Jerk, input.CountsPerRev),
                        cancellationToken);
                    Result(response);
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
                        + ", dummy-rev="
                        + (position.PositionRaw / input.CountsPerRev).ToString(
                            "0.########",
                            CultureInfo.InvariantCulture));
                    break;

                case "MoveAbsoluteEx":
                    response = await axis.MoveAbsoluteExAsync(
                        ToDummyProfileDint(input.Position, input.CountsPerRev),
                        ToDummyProfileDint(input.Velocity, input.CountsPerRev),
                        ToDummyProfileDint(input.Acceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Deceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Jerk, input.CountsPerRev),
                        LMC_DIRECTION.Shortest,
                        cancellationToken);
                    Result(response);
                    break;

                case "MoveRelativeEx":
                    response = await axis.MoveRelativeExAsync(
                        ToDummyProfileDint(input.Position, input.CountsPerRev),
                        ToDummyProfileDint(input.Velocity, input.CountsPerRev),
                        ToDummyProfileDint(input.Acceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Deceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Jerk, input.CountsPerRev),
                        LMC_DIRECTION.Shortest,
                        cancellationToken);
                    Result(response);
                    break;

                case "MoveVelocityEx":
                    response = await axis.MoveVelocityExAsync(
                        ToDummyProfileDint(input.Velocity, input.CountsPerRev),
                        ToDummyProfileDint(input.Acceleration, input.CountsPerRev),
                        0,
                        ToDummyProfileDint(input.Jerk, input.CountsPerRev),
                        input.Direction,
                        cancellationToken);
                    Result(response);
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

                case "ReadMemberStatus":
                    await ReadMemberStatusAsync(
                        await CreateAxesAsync(
                            connection,
                            input.MemberNames,
                            cancellationToken),
                        cancellationToken);
                    break;

                case "PowerOnMembers":
                    await PowerMembersWithNamesAsync(
                        await CreateAxesAsync(
                            connection,
                            input.MemberNames,
                            cancellationToken),
                        true,
                        cancellationToken);
                    break;

                case "PowerOffMembers":
                    await PowerMembersWithNamesAsync(
                        await CreateAxesAsync(
                            connection,
                            input.MemberNames,
                            cancellationToken),
                        false,
                        cancellationToken);
                    break;

                case "GroupEnable":
                    response = await group.GroupEnableAsync(cancellationToken);
                    Result(response);
                    break;

                case "GroupDisable":
                    response = await group.GroupDisableAsync(cancellationToken);
                    Result(response);
                    break;

                case "GroupReset":
                    response = await group.GroupResetAsync(cancellationToken);
                    Result(response);
                    break;

                case "GroupStop":
                    response = await group.GroupStopAsync(
                        ToDummyProfileDint(input.Deceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Jerk, input.CountsPerRev),
                        cancellationToken);
                    Result(response);
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
                        "GroupPosition dummy-rev=["
                        + string.Join(
                            ",",
                            positions.PositionsRaw.Select(
                                value => (value / input.CountsPerRev).ToString(
                                    "0.########",
                                    CultureInfo.InvariantCulture)))
                        + "]");
                    break;

                case "MoveLinearAbsoluteEx":
                    response = await group.MoveLinearAbsoluteExAsync(
                        input.GroupPositions
                            .Split(',')
                            .Select(value => ToDummyProfileDint(
                                value,
                                input.CountsPerRev))
                            .ToArray(),
                        ToDummyProfileDint(input.Velocity, input.CountsPerRev),
                        ToDummyProfileDint(input.Acceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Deceleration, input.CountsPerRev),
                        ToDummyProfileDint(input.Jerk, input.CountsPerRev),
                        input.MotionOptions,
                        cancellationToken);
                    Result(response);
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
                    Result(response);
                    Write(
                        "Kinematic mapping X/Y/Z/U="
                        + string.Join(",", input.MemberNames));
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
            foreach (var axis in axes)
            {
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
                UpdateUiState();
            }
        }

        private async Task RunAsync(
            string operation,
            Func<CancellationToken, Task> action)
        {
            if (_operationCancellation != null)
            {
                Write("Another operation is already running.");
                return;
            }

            var cancellation = new CancellationTokenSource();
            _operationCancellation = cancellation;
            UpdateUiState();

            try
            {
                await action(cancellation.Token);
                Write(operation + " completed.");
            }
            catch (OperationCanceledException)
            {
                Write(operation + " cancelled.");
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

            ConnectButton.IsEnabled = !busy;
            CloseButton.IsEnabled = !busy && connection != null;
            ExecuteButton.IsEnabled =
                !busy
                && connection != null
                && connection.IsConnected;
            CancelButton.IsEnabled = busy;

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
        }

        private static void FillEnum<T>(ComboBox comboBox, T selectedValue)
        {
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                comboBox.Items.Add(value);
            }

            comboBox.SelectedItem = selectedValue;
        }

        private void Result(LMC_Response response)
        {
            if (response == null)
            {
                return;
            }

            Write(
                "Response Status="
                + response.Status
                + ", ErrorId="
                + response.ErrorId
                + ", Bytes="
                + (response.Raw == null ? 0 : response.Raw.Length));

            EnsureResponseSuccess("Controller command", response);
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

        private static int ToDummyProfileDint(
            string value,
            double countsPerRevolution)
        {
            return checked(
                (int)Math.Round(D(value) * countsPerRevolution));
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
            public double CountsPerRev { get; set; }
            public LMC_DIRECTION Direction { get; set; }
            public LMC_COORD_SYSTEM ReadCoordinateSystem { get; set; }
            public LMCGroupMotionOptions MotionOptions { get; set; }
        }
    }
}

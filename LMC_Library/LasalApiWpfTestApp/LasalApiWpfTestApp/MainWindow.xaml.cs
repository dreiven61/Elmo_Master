using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow : Window
    {
        private const int GroupLockDisabledSamplesForRetry = 3;
        private const int MinimumGroupMotionMonitorMilliseconds = 15000;
        private const int MaximumGroupMotionMonitorMilliseconds = 600000;
        private static readonly PlcUnitOption[] PlcUnitOptions =
        {
            new PlcUnitOption("None / raw DINT (no conversion)", "raw", 1, true),
            new PlcUnitOption("mm (x10000)", "mm", LMC_Units.MM, false),
            new PlcUnitOption("m (x10000000)", "m", LMC_Units.M, false),
            new PlcUnitOption("deg (x10000)", "deg", LMC_Units.DEG, false)
        };

        // The API async methods schedule transport work. Keep live and safety
        // command ordering explicit at the example-application boundary.
        private readonly SemaphoreSlim commandSendGate = new SemaphoreSlim(1, 1);
        private LMCConnection connection;
        private LMCSingleAxis axis;
        private LMCGroupAxis group;
        private bool operationRunning;
        private bool connectionTransitionRunning;
        private bool safetyCommandRunning;
        private int safetyMonitorCount;
        // Stop/PowerOff increments this before waiting for commandSendGate.
        // A live command must recheck the value after it owns the same gate.
        private int safetyRequestGeneration;
        private bool motionMayBeActive;
        private string motionAxisName;
        private string motionOperation;
        private bool motionWasObserved;
        private int motionTrackingGeneration;
        private bool groupPowerVerificationPending;
        private bool groupPowerOffVerificationPending;
        private bool groupStatusRefreshRequired;
        private bool groupActiveVerified;
        private bool groupIdentityHomeCheckComplete;
        private bool groupIdentityHomeCheckPassed;
        private bool groupIdentityConfigured;
        private bool groupProfileLockVerificationPending;
        private int groupProfileLockDisabledSamples;
        private bool groupProfileLocked;
        private bool shutdownInProgress;
        private bool allowWindowClose;

        public MainWindow()
        {
            InitializeComponent();

            ComboAxisUnit.ItemsSource = PlcUnitOptions;
            ComboAxisUnit.SelectedIndex = 1;
            ComboGroupUnit.ItemsSource = PlcUnitOptions;
            ComboGroupUnit.SelectedIndex = 1;

            ComboDirection.Items.Add(LMC_DIRECTION.Positive);
            ComboDirection.Items.Add(LMC_DIRECTION.Negative);
            ComboDirection.SelectedItem = LMC_DIRECTION.Positive;

            ComboGroupCoordinate.Items.Add(LMC_COORD_SYSTEM.None);
            ComboGroupCoordinate.Items.Add(LMC_COORD_SYSTEM.Acs);
            ComboGroupCoordinate.SelectedItem = LMC_COORD_SYSTEM.None;

            ComboGroupTransition.Items.Add(
                LMC_GROUP_TRANSITION_MODE.ExactStop);
            ComboGroupTransition.Items.Add(
                LMC_GROUP_TRANSITION_MODE.ContinuousDirect);
            ComboGroupTransition.SelectedItem =
                LMC_GROUP_TRANSITION_MODE.ExactStop;

            ComboGroupBuffer.Items.Add(LMC_BUFFER_MODE.Aborting);
            ComboGroupBuffer.Items.Add(LMC_BUFFER_MODE.Buffered);
            ComboGroupBuffer.SelectedItem = LMC_BUFFER_MODE.Aborting;

            InitializeDiagnosticsUi();
            InitializeReadOnlyApiUi();
            InitializeQualificationUi();

            WriteLog(
                "Example ready. Connect, load _LMCAxis1, and start with Read Status. "
                + "No command is sent automatically.");
            UpdateUiState();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(
                    () =>
                    {
                        ButtonConnect.Focus();
                        ScrollSelectedMotionTabToTop();
                    }));
        }

        private void MotionTabs_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.Source, sender))
            {
                return;
            }

            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(ScrollSelectedMotionTabToTop));
        }

        private void ComboGroupCoordinate_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateUiState();
        }

        private void ScrollSelectedMotionTabToTop()
        {
            if (TabsMotion == null)
            {
                return;
            }

            if (TabsMotion.SelectedIndex == 0)
            {
                ScrollSingleAxis?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 1)
            {
                ScrollGroupMotion?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 2)
            {
                ScrollDiagnostics?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 3)
            {
                ScrollBulkSnapshot?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 4)
            {
                ScrollRecorder?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 5)
            {
                ScrollDiagnosticsOperations?.ScrollToTop();
            }
            else if (TabsMotion.SelectedIndex == 6)
            {
                ScrollReadOnlyApi?.ScrollToTop();
            }
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Connect",
                async () =>
                {
                    if (HasUnresolvedD5SdoQualificationTicket
                        && connection != null
                        && connection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked while a D5 ticket, submission outcome, or Write readback is unresolved. "
                            + GetD5SdoResolutionGuidance()
                            + " Reconnect is allowed only after an external connection loss.");
                    }

                    if (motionMayBeActive)
                    {
                        throw new InvalidOperationException(
                            "Reconnect is blocked because motion may still be active on "
                            + motionAxisName
                            + ". Send Stop or PowerOff and verify standstill first.");
                    }

                    if (connection != null)
                    {
                        await CloseCurrentConnectionAsync(false);
                    }

                    var newConnection = new LMCConnection();
                    AttachConnection(newConnection);
                    connection = newConnection;
                    ClearLoadedObjects();
                    UpdateUiState();

                    try
                    {
                        await newConnection.RpcInitConnectionAsync(
                            RequiredText(TextRemoteIp.Text, "PLC IP"),
                            ParsePort(TextRemotePort.Text, "TCP port", false),
                            RequiredText(TextLocalIp.Text, "PC local IPv4"),
                            ParsePort(
                                TextCallbackPort.Text,
                                "Callback UDP port",
                                true),
                            LMCConnection.DefaultEventMask,
                            CancellationToken.None);
                    }
                    catch
                    {
                        if (ReferenceEquals(connection, newConnection))
                        {
                            connection = null;
                        }

                        DetachConnection(newConnection);
                        newConnection.Dispose();
                        ClearLoadedObjects();
                        UpdateUiState();
                        throw;
                    }

                    WriteLog(
                        "RPC initialized. Callback endpoint="
                        + newConnection.CallbackLocalEndPoint
                        + ", EventMask=0x"
                        + newConnection.EventMask.ToString("X8"));
                },
                true);
        }

        private async void ButtonCloseConnection_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Close Connection",
                () =>
                {
                    if (HasUnresolvedD5SdoQualificationTicket)
                    {
                        throw new InvalidOperationException(
                            "Close Connection is blocked while a D5 ticket, submission outcome, or Write readback is unresolved. "
                            + GetD5SdoResolutionGuidance());
                    }

                    return CloseCurrentConnectionAsync(true);
                },
                true);
        }

        private async void ButtonLookupAxis_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Load Axis",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var objectName = RequiredText(
                        TextAxisName.Text,
                        "Axis object name");
                    var loadedAxis = await LMCSingleAxis.CreateAsync(
                        currentConnection,
                        objectName,
                        CancellationToken.None);

                    axis = loadedAxis;
                    TextAxisReference.Text = loadedAxis.AxisReference.ToString(
                        CultureInfo.InvariantCulture);
                    TextAxisResult.Text =
                        "Loaded "
                        + loadedAxis.AxisName
                        + Environment.NewLine
                        + "Reference="
                        + loadedAxis.AxisReference
                        + Environment.NewLine
                        + FormatResponse(loadedAxis.AxisInfoResponse);
                    WriteLog(
                        "Axis loaded. Name="
                        + loadedAxis.AxisName
                        + ", Ref="
                        + loadedAxis.AxisReference);
                });
        }

        private async void ButtonLookupGroup_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Load Group",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var objectName = RequiredText(
                        TextGroupName.Text,
                        "Group object name");
                    var loadedGroup = await LMCGroupAxis.CreateAsync(
                        currentConnection,
                        objectName,
                        CancellationToken.None);

                    group = loadedGroup;
                    ResetGroupPreparationState();
                    TextGroupReference.Text = loadedGroup.GroupReference.ToString(
                        CultureInfo.InvariantCulture);
                    TextGroupResult.Text =
                        "Loaded "
                        + loadedGroup.GroupName
                        + Environment.NewLine
                        + "Reference="
                        + loadedGroup.GroupReference;
                    WriteLog(
                        "Group loaded. Name="
                        + loadedGroup.GroupName
                        + ", Ref="
                        + loadedGroup.GroupReference);
                });
        }

        private async void ButtonReadStatus_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Axis Status",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var result = await currentAxis.ReadStatusResultAsync(
                        CancellationToken.None);
                    EnsureAxisStatusSuccess("Read Status", result);
                    DisplayAxisStatus(result);

                    if (!IsTrackedMotionAxis(currentAxis.AxisName))
                    {
                        return;
                    }

                    if (!result.IsStandstill)
                    {
                        RecordMotionObserved(currentAxis.AxisName);
                        return;
                    }

                    if (!result.IsPowerOn || motionWasObserved)
                    {
                        var verified = await WaitForStandstillAsync(
                            currentAxis,
                            750,
                            0);
                        DisplayAxisStatus(verified);
                        ClearMotionWarning(
                            "Read Status verified three stable safe samples");
                        return;
                    }

                    WriteLog(
                        "SAFETY: Standstill was reported, but motion has not yet "
                        + "been observed. The motion warning remains active; use "
                        + "Stop or PowerOff to establish a known safe state.");
                });
        }

        private async void ButtonReadPosition_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Actual Position",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var unit = ReadAxisUnitSelection();
                    var result = await currentAxis.GetActualPositionResultAsync(
                        CancellationToken.None);
                    EnsureAxisPositionSuccess("Read Actual Position", result);

                    TextAxisResult.Text =
                        "Actual position"
                        + Environment.NewLine
                        + "Raw DINT="
                        + result.PositionRaw
                        + Environment.NewLine
                        + FormatEngineeringPosition(result.PositionRaw, unit)
                        + Environment.NewLine
                        + "FunctionStatus=0x"
                        + result.FunctionStatus.ToString("X4")
                        + ", ErrorId="
                        + result.ErrorId;
                });
        }

        private async void ButtonPowerOn_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Power On"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Power On",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Power On",
                        () => currentAxis.PowerOnAsync(
                            CancellationToken.None));
                    EnsureResponseSuccess("Power On", response);
                    var status = await WaitForPowerStateAsync(
                        currentAxis,
                        true,
                        5000);
                    EnsureNoNewSafetyRequest(
                        safetyGeneration,
                        "Power On verification");
                    DisplayAxisStatus(status);
                });
        }

        private async void ButtonPowerOff_Click(object sender, RoutedEventArgs e)
        {
            LMCSingleAxis currentAxis = null;
            var sent = await RunSafetyCommandAsync(
                "Power Off Send",
                async () =>
                {
                    currentAxis = RequireAxis();
                    var response = await currentAxis.PowerOffAsync(
                        CancellationToken.None);
                    EnsureResponseSuccess("Power Off", response);
                    TextAxisResult.Text = FormatResponse(response);
                },
                () => CancelQualificationForExternalSafety(
                    "Axis Power Off",
                    false));

            if (!sent || currentAxis == null)
            {
                return;
            }

            await RunSafetyMonitorAsync(
                "Power Off",
                currentAxis,
                async () =>
                {
                    await WaitForPowerStateAsync(
                        currentAxis,
                        false,
                        5000);
                    return await WaitForStandstillAsync(
                        currentAxis,
                        5000,
                        0);
                });
        }

        private async void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Reset"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Reset",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Reset",
                        () => currentAxis.ResetAsync(CancellationToken.None));
                    EnsureResponseSuccess("Reset", response);
                    TextAxisResult.Text = FormatResponse(response);
                });
        }

        private async void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            LMCSingleAxis currentAxis = null;
            var sent = await RunSafetyCommandAsync(
                "Stop Send",
                async () =>
                {
                    currentAxis = RequireAxis();
                    var input = ReadStopInput();
                    var response = await currentAxis.StopAsync(
                        input.DecelerationRaw,
                        input.JerkRaw,
                        CancellationToken.None);
                    EnsureResponseSuccess("Stop", response);
                    TextAxisResult.Text = FormatResponse(response);
                },
                () => CancelQualificationForExternalSafety(
                    "Axis Stop",
                    false));

            if (!sent || currentAxis == null)
            {
                return;
            }

            await RunSafetyMonitorAsync(
                "Stop",
                currentAxis,
                () => WaitForStandstillAsync(
                    currentAxis,
                    5000,
                    50));
        }

        private async void ButtonMoveAbsolute_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Move Absolute"))
            {
                return;
            }

            await RunFiniteMotionAsync(
                "Move Absolute",
                true,
                safetyRequestGeneration,
                async (currentAxis, input) =>
                    await currentAxis.MoveAbsoluteExAsync(
                        input.PositionRaw,
                        input.VelocityRaw,
                        input.AccelerationRaw,
                        input.DecelerationRaw,
                        input.JerkRaw,
                        LMC_DIRECTION.Shortest,
                        CancellationToken.None));
        }

        private async void ButtonMoveRelative_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Move Relative"))
            {
                return;
            }

            await RunFiniteMotionAsync(
                "Move Relative",
                false,
                safetyRequestGeneration,
                async (currentAxis, input) =>
                    await currentAxis.MoveRelativeExAsync(
                        input.PositionRaw,
                        input.VelocityRaw,
                        input.AccelerationRaw,
                        input.DecelerationRaw,
                        input.JerkRaw,
                        LMC_DIRECTION.Shortest,
                        CancellationToken.None));
        }

        private async void ButtonMoveVelocity_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Move Velocity"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Move Velocity",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var input = ReadVelocityMotionInput();
                    await EnsureAxisPoweredOnAsync(currentAxis);

                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Move Velocity",
                        async () =>
                        {
                            MarkMotionUncertain(
                                currentAxis.AxisName,
                                "Move Velocity");
                            return await currentAxis.MoveVelocityExAsync(
                                input.VelocityRaw,
                                input.AccelerationRaw,
                                0,
                                input.JerkRaw,
                                input.Direction,
                                CancellationToken.None);
                        });

                    ClearMotionOnConfirmedRejection(
                        currentAxis.AxisName,
                        "Move Velocity",
                        response);
                    EnsureResponseSuccess("Move Velocity", response);

                    TextAxisResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Motion remains active until Stop or PowerOff is verified.";
                    WriteLog(
                        "SAFETY: Move Velocity accepted. Use Stop or PowerOff; "
                        + "Close is blocked until standstill is verified.");
                });
        }

        private async Task RunFiniteMotionAsync(
            string operation,
            bool absolute,
            int safetyGeneration,
            Func<LMCSingleAxis, MotionInput, Task<LMC_Response>> send)
        {
            LMCSingleAxis monitoredAxis = null;
            var trackingGeneration = 0;
            var noMovementExpected = false;

            await RunOperationAsync(
                operation + " Send",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var input = ReadFiniteMotionInput();
                    await EnsureAxisPoweredOnAsync(currentAxis);

                    var startPosition = await currentAxis
                        .GetActualPositionResultAsync(CancellationToken.None);
                    EnsureAxisPositionSuccess(
                        operation + " start position",
                        startPosition);
                    noMovementExpected = absolute
                        ? startPosition.PositionRaw == input.PositionRaw
                        : input.PositionRaw == 0;
                    monitoredAxis = currentAxis;

                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        operation,
                        async () =>
                        {
                            trackingGeneration = MarkMotionUncertain(
                                currentAxis.AxisName,
                                operation);
                            return await send(currentAxis, input);
                        });
                    ClearMotionOnConfirmedRejection(
                        currentAxis.AxisName,
                        operation,
                        response);
                    EnsureResponseSuccess(operation, response);

                    TextAxisResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Command accepted; monitoring for movement and stable standstill.";
                });

            if (monitoredAxis == null
                || trackingGeneration == 0
                || !IsTrackedMotion(
                    monitoredAxis.AxisName,
                    trackingGeneration))
            {
                return;
            }

            await MonitorFiniteMotionAsync(
                operation,
                monitoredAxis,
                trackingGeneration,
                noMovementExpected);
        }

        private async Task MonitorFiniteMotionAsync(
            string operation,
            LMCSingleAxis monitoredAxis,
            int trackingGeneration,
            bool noMovementExpected)
        {
            WriteLog(
                operation
                + " monitor started. Stop and PowerOff remain available.");
            TextOperationState.Text = operation + " monitoring";
            UpdateUiState();

            try
            {
                var status = await WaitForFiniteMotionCompletionAsync(
                    monitoredAxis,
                    trackingGeneration,
                    noMovementExpected,
                    15000);
                if (status == null)
                {
                    WriteLog(
                        operation
                        + " monitor ended because another safety action cleared tracking.");
                    return;
                }

                DisplayAxisStatus(status);
                ClearMotionWarning(
                    operation + " completed at stable standstill",
                    trackingGeneration);
                WriteLog(operation + " completion PASS.");
                TextOperationState.Text = operation + " completed";
            }
            catch (Exception error)
            {
                if (IsTrackedMotion(
                    monitoredAxis.AxisName,
                    trackingGeneration))
                {
                    WriteLog(
                        operation
                        + " monitor FAILED: "
                        + error.Message
                        + " Stop or PowerOff is still required.");
                    TextOperationState.Text = operation + " monitor failed";
                }
                else
                {
                    WriteLog(
                        operation
                        + " monitor ended after the tracked motion was cleared: "
                        + error.Message);
                }
            }
            finally
            {
                UpdateUiState();
            }
        }

        private async Task MonitorGroupFiniteMotionAsync(
            string operation,
            LMCGroupAxis monitoredGroup,
            int trackingGeneration,
            bool noMovementExpected,
            int timeoutMilliseconds)
        {
            WriteLog(
                operation
                + " group monitor started with timeout "
                + timeoutMilliseconds
                + " ms. Group Stop remains available.");
            TextOperationState.Text =
                operation
                + " monitoring (limit "
                + (timeoutMilliseconds / 1000.0).ToString(
                    "0.###",
                    CultureInfo.InvariantCulture)
                + " s)";
            UpdateUiState();

            try
            {
                var status = await WaitForGroupMotionCompletionAsync(
                    monitoredGroup,
                    trackingGeneration,
                    noMovementExpected,
                    timeoutMilliseconds);
                if (status == null)
                {
                    WriteLog(
                        operation
                        + " monitor ended because a safety action cleared tracking.");
                    return;
                }

                DisplayGroupStatus(status);
                ClearMotionWarning(
                    operation + " completed at stable Group InPosition",
                    trackingGeneration);
                WriteLog(operation + " completion PASS.");
                TextOperationState.Text = operation + " completed";
            }
            catch (Exception error)
            {
                if (IsTrackedMotion(
                    monitoredGroup.GroupName,
                    trackingGeneration))
                {
                    WriteLog(
                        operation
                        + " monitor FAILED: "
                        + error.Message
                        + " Group Stop is still required.");
                    TextOperationState.Text = operation + " monitor failed";
                }
                else
                {
                    WriteLog(
                        operation
                        + " monitor ended after tracked motion was cleared: "
                        + error.Message);
                }
            }
            finally
            {
                UpdateUiState();
            }
        }

        private async void ButtonGetMembers_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Get Group Members",
                async () =>
                {
                    var result = await RequireGroup()
                        .GetGroupMembersInfoResultAsync(CancellationToken.None);
                    EnsureGroupMembersSuccess("Get Group Members", result);

                    var memberLines = result.Members.Select(
                        member =>
                            "["
                            + member.Index
                            + "] Name="
                            + member.AxisName
                            + ", Ref="
                            + member.AxisReference
                            + ", DeviceId="
                            + member.DeviceId);
                    TextGroupResult.Text =
                        "AxisCount="
                        + result.AxisCount
                        + Environment.NewLine
                        + string.Join(Environment.NewLine, memberLines);
                });
        }

        private async void ButtonGroupReadStatus_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Group Status",
                async () =>
                {
                    var currentGroup = RequireGroup();
                    var result = await currentGroup.GroupReadStatusResultAsync(
                        CancellationToken.None);
                    if (result == null || !result.IsSuccess)
                    {
                        InvalidateGroupPreparationAfterStatusFailure();
                    }
                    EnsureGroupStatusSuccess("Read Group Status", result);
                    groupStatusRefreshRequired = false;
                    DisplayGroupStatus(result);
                    var lockPendingStatusLogged = false;

                    if (result.IsPowerOn)
                    {
                        var firstActiveVerification = !groupActiveVerified;
                        groupPowerVerificationPending = false;
                        groupActiveVerified = true;
                        if (groupPowerOffVerificationPending)
                        {
                            WriteLog(
                                "Group Power Off was accepted/start only, but Power "
                                + "Ready is still reported. Read Status again after "
                                + "the LASAL mode change completes.");
                        }
                        else if (firstActiveVerification)
                        {
                            WriteLog(
                                "Group Power Ready/ACTIVE state verified. Set Identity "
                                + "is now available.");
                        }
                    }
                    else
                    {
                        var powerOffWasPending =
                            groupPowerOffVerificationPending;
                        if (groupActiveVerified
                            || groupIdentityHomeCheckComplete
                            || groupIdentityConfigured
                            || groupProfileLockVerificationPending
                            || groupProfileLocked)
                        {
                            WriteLog(
                                "Group status no longer reports PowerOn. The local "
                                + "Home, identity, and profile-lock state was cleared.");
                        }

                        groupActiveVerified = false;
                        groupIdentityConfigured = false;
                        ResetIdentityHomeCheckState();
                        groupProfileLockVerificationPending = false;
                        groupProfileLockDisabledSamples = 0;
                        groupProfileLocked = false;
                        groupPowerOffVerificationPending = false;
                        if (powerOffWasPending)
                        {
                            WriteLog(
                                "Group Power Off verified: Read Status reports "
                                + "PowerOn=False.");
                        }
                        if (groupPowerVerificationPending)
                        {
                            WriteLog(
                                "Group Power On was accepted/start only, but Power "
                                + "Ready is not verified yet. Read Status again after "
                                + "the LASAL mode change completes.");
                        }
                    }

                    if (result.IsPowerOn && result.IsStandby)
                    {
                        var lockVerificationWasPending =
                            groupProfileLockVerificationPending;
                        groupProfileLockVerificationPending = false;
                        groupProfileLockDisabledSamples = 0;
                        groupProfileLocked = true;
                        if (lockVerificationWasPending)
                        {
                            WriteLog(
                                "Group Lock Ready verified: Read Status reports "
                                + "Enabled/Locked Standby. Move Linear is now "
                                + "available.");
                        }
                        if (!groupIdentityConfigured)
                        {
                            WriteLog(
                                "Group status reports Enabled/Locked Standby, but "
                                + "this session did not configure the identity. "
                                + "Unlock, configure identity, then lock again.");
                        }
                    }
                    else if (result.IsDisabled)
                    {
                        var externalUnlockObserved = groupProfileLocked;
                        var lockVerificationFailed =
                            groupProfileLockVerificationPending;
                        groupProfileLocked = false;
                        if (externalUnlockObserved)
                        {
                            WriteLog(
                                "Group status reports Disabled/Unlocked; the local "
                                + "profile-lock state was cleared.");
                        }
                        if (lockVerificationFailed)
                        {
                            groupProfileLockDisabledSamples++;
                            if (groupProfileLockDisabledSamples
                                >= GroupLockDisabledSamplesForRetry)
                            {
                                groupProfileLockVerificationPending = false;
                                groupProfileLockDisabledSamples = 0;
                                WriteLog(
                                    "Group status reported Disabled/Unlocked "
                                    + "three consecutive times. Lock Ready was "
                                    + "not verified; Enable (Lock Profile) is "
                                    + "available to retry.");
                            }
                            else
                            {
                                WriteLog(
                                    "Group Lock Ready pending: Read Status still "
                                    + "reports Disabled/Unlocked (sample "
                                    + groupProfileLockDisabledSamples
                                    + "/"
                                    + GroupLockDisabledSamplesForRetry
                                    + "). Read Status again.");
                                lockPendingStatusLogged = true;
                            }
                        }
                    }
                    else
                    {
                        groupProfileLockDisabledSamples = 0;
                    }

                    if (groupProfileLockVerificationPending
                        && !(result.IsPowerOn && result.IsStandby)
                        && !lockPendingStatusLogged)
                    {
                        WriteLog(
                            "Group Enable was accepted, but Lock Ready is not "
                            + "verified yet. Run Read Status again until "
                            + "Enabled/Locked Standby=True.");
                    }

                    UpdateUiState();

                    if (!IsTrackedMotionAxis(currentGroup.GroupName))
                    {
                        return;
                    }

                    if (!result.IsPowerOn)
                    {
                        await RunGroupPowerOffSafetyMonitorAsync(
                            "Read Group Status Power Off recovery",
                            currentGroup);
                        return;
                    }

                    if (!IsGroupInPosition(result))
                    {
                        RecordMotionObserved(currentGroup.GroupName);
                        return;
                    }

                    if (motionWasObserved)
                    {
                        var verified = await WaitForGroupInPositionAsync(
                            currentGroup,
                            750,
                            0);
                        DisplayGroupStatus(verified);
                        ClearMotionWarning(
                            "Read Group Status verified three stable in-position samples");
                        return;
                    }

                    WriteLog(
                        "SAFETY: Group InPosition was reported, but motion has not "
                        + "yet been observed. The motion warning remains active; "
                        + "use Group Stop to establish a known stopped state.");
                });
        }

        private async void ButtonGroupReadPosition_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Group Position",
                async () =>
                {
                    var currentGroup = RequireGroup();
                    var coordinateSystem = ReadGroupPositionCoordinateSystem();
                    var unit = ReadGroupUnitSelection();
                    var result = await currentGroup
                        .GroupReadActualPositionAsync(
                            coordinateSystem,
                            CancellationToken.None);
                    EnsureGroupPositionSuccess(
                        "Read Group Position",
                        result);
                    DisplayGroupPosition(result, unit);
                });
        }

        private async void ButtonGroupPowerOn_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Group Power On"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Group Power On",
                async () =>
                {
                    var currentGroup = RequireGroup();
                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Group Power On",
                        () => currentGroup.GroupPowerOnAsync(
                            CancellationToken.None));
                    EnsureResponseSuccess("Group Power On", response);
                    ResetGroupPreparationState();
                    groupPowerVerificationPending = true;
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Power On accepted; use Read Status until "
                        + "PowerOn=True is reported.";
                    WriteLog(
                        "Group Power On accepted/start only. This is not an ACTIVE "
                        + "confirmation; run Read Status before Set Identity.");
                });
        }

        private async void ButtonGroupPowerOff_Click(
            object sender,
            RoutedEventArgs e)
        {
            LMCGroupAxis currentGroup = null;
            var sent = await RunSafetyCommandAsync(
                "Group Power Off Send",
                async () =>
                {
                    currentGroup = RequireGroup();
                    var response = await currentGroup.GroupPowerOffAsync(
                        CancellationToken.None);
                    EnsureResponseSuccess("Group Power Off", response);
                    groupPowerVerificationPending = false;
                    groupPowerOffVerificationPending = true;
                    groupProfileLockVerificationPending = false;
                    groupProfileLockDisabledSamples = 0;
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Power Off accepted; use Read Status until "
                        + "PowerOn=False is reported.";
                    WriteLog(
                        "Group Power Off accepted/start only. Read Status must "
                        + "verify PowerOn=False.");
                },
                () => CancelQualificationForExternalSafety(
                    "Group Power Off",
                    true));

            if (sent && currentGroup != null)
            {
                await RunGroupPowerOffSafetyMonitorAsync(
                    "Group Power Off",
                    currentGroup);
            }
        }

        private async void ButtonGroupEnable_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Group Enable (Lock Profile)"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Group Enable (Lock Profile)",
                async () =>
                {
                    var currentGroup = RequireGroup();
                    EnsureGroupActiveVerified();
                    if (!groupIdentityConfigured)
                    {
                        throw new InvalidOperationException(
                            "Set Identity (Configure) before Enable "
                            + "(Lock Profile).");
                    }

                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Group Enable (Lock Profile)",
                        () => currentGroup.GroupEnableAsync(
                            CancellationToken.None));
                    EnsureResponseSuccess(
                        "Group Enable (Lock Profile)",
                        response);
                    groupProfileLockVerificationPending = true;
                    groupProfileLockDisabledSamples = 0;
                    groupProfileLocked = false;
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Lock request accepted; run 5 Read Status until "
                        + "Enabled/Locked Standby=True before Move Linear.";
                    WriteLog(
                        "Group Enable accepted. Run 5 Read Status "
                        + "to verify Enabled/Locked Standby before Move Linear.");
                });
        }

        private async void ButtonGroupDisable_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Group Disable (Unlock Profile)",
                async () =>
                {
                    EnsureNoUnresolvedD5SdoQualificationTicket(
                        "Group Disable (Unlock Profile)");
                    var currentGroup = RequireGroup();
                    var response = await SendSerializedCommandAsync(
                        () => currentGroup.GroupDisableAsync(
                            CancellationToken.None));
                    EnsureResponseSuccess(
                        "Group Disable (Unlock Profile)",
                        response);
                    groupProfileLockVerificationPending = false;
                    groupProfileLockDisabledSamples = 0;
                    groupProfileLocked = false;
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Profile unlocked; Power Off is now available.";
                });
        }

        private async void ButtonGroupReset_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Group Reset"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Group Reset",
                async () =>
                {
                    var currentGroup = RequireGroup();
                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Group Reset",
                        () => currentGroup.GroupResetAsync(
                            CancellationToken.None));
                    EnsureResponseSuccess("Group Reset", response);
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Axis error reset accepted; group power, identity, "
                        + "and profile-lock preparation state is unchanged. "
                        + "Read Status to verify the error cleared.";
                });
        }

        private async void ButtonGroupStop_Click(
            object sender,
            RoutedEventArgs e)
        {
            LMCGroupAxis currentGroup = null;
            var sent = await RunSafetyCommandAsync(
                "Group Stop Send",
                async () =>
                {
                    currentGroup = RequireGroup();
                    var input = ReadGroupStopInput();
                    var response = await currentGroup.GroupStopAsync(
                        input.DecelerationRaw,
                        input.JerkRaw,
                        CancellationToken.None);
                    EnsureResponseSuccess("Group Stop", response);
                    TextGroupResult.Text = FormatResponse(response);
                },
                () => CancelQualificationForExternalSafety(
                    "Group Stop",
                    true));

            if (!sent || currentGroup == null)
            {
                return;
            }

            await RunGroupSafetyMonitorAsync(
                "Group Stop",
                currentGroup);
        }

        private async void ButtonGroupMoveLinear_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Move Linear Absolute"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            LMCGroupAxis monitoredGroup = null;
            var trackingGeneration = 0;
            var noMovementExpected = false;
            var monitorTimeoutMilliseconds =
                MinimumGroupMotionMonitorMilliseconds;

            await RunOperationAsync(
                "Move Linear Absolute Send",
                async () =>
                {
                    var currentGroup = RequireGroup();
                    EnsureGroupReadyForMotion();
                    var input = ReadGroupMotionInput();
                    var startPosition = await currentGroup
                        .GroupReadActualPositionAsync(
                            input.Options.CoordinateSystem,
                            CancellationToken.None);
                    EnsureGroupPositionSuccess(
                        "Move Linear Absolute start position",
                        startPosition);

                    var currentPositions = startPosition.PositionsRaw;
                    var monitorDistances = input.PositionsRaw
                        .Select(
                            (target, index) =>
                                (long)target - currentPositions[index])
                        .ToArray();
                    noMovementExpected = monitorDistances
                        .Take(4)
                        .All(distance => distance == 0);
                    monitorTimeoutMilliseconds =
                        CalculateGroupMotionMonitorTimeoutMilliseconds(
                            monitorDistances,
                            input.VelocityRaw,
                            input.AccelerationRaw,
                            input.DecelerationRaw);
                    WriteLog(
                        "Move Linear Absolute input: StartRaw="
                        + FormatGroupPositionsRaw(currentPositions)
                        + ", TargetRaw="
                        + FormatGroupPositionsRaw(input.PositionsRaw)
                        + ", VelocityRaw="
                        + input.VelocityRaw
                        + ", AccelerationRaw="
                        + input.AccelerationRaw
                        + ", DecelerationRaw="
                        + input.DecelerationRaw
                        + ", JerkRaw="
                        + input.JerkRaw
                        + ", Transition="
                        + input.Options.TransitionMode
                        + ", Buffer="
                        + input.Options.BufferMode
                        + ", MonitorTimeoutMs="
                        + monitorTimeoutMilliseconds
                        + ".");
                    monitoredGroup = currentGroup;

                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Move Linear Absolute",
                        async () =>
                        {
                            trackingGeneration = MarkMotionUncertain(
                                currentGroup.GroupName,
                                "Move Linear Absolute");
                            return await currentGroup
                                .MoveLinearAbsoluteExAsync(
                                    input.PositionsRaw,
                                    input.VelocityRaw,
                                    input.AccelerationRaw,
                                    input.DecelerationRaw,
                                    input.JerkRaw,
                                    input.Options,
                                    CancellationToken.None);
                        });

                    ClearMotionOnConfirmedRejection(
                        currentGroup.GroupName,
                        "Move Linear Absolute",
                        response);
                    if (response != null
                        && response.IsFrameValid
                        && response.ErrorId == 7)
                    {
                        var diagnostic =
                            "LASAL rejected the endpoint with "
                            + "_LMCPROF_SWE_ERROR (7): a runtime software end "
                            + "position limit was violated. StartRaw="
                            + FormatGroupPositionsRaw(currentPositions)
                            + ", TargetRaw="
                            + FormatGroupPositionsRaw(input.PositionsRaw)
                            + ". Compare the target with each axis "
                            + "AxReadSWEndPos maximum/minimum and "
                            + "ReadProfileError().SubErrorNo in LASAL.";
                        TextGroupResult.Text =
                            FormatResponse(response)
                            + Environment.NewLine
                            + diagnostic;
                        throw new InvalidOperationException(diagnostic);
                    }
                    EnsureResponseSuccess("Move Linear Absolute", response);
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "Command accepted; monitoring Group InPosition "
                        + "with timeout "
                        + monitorTimeoutMilliseconds
                        + " ms.";
                });

            if (monitoredGroup == null
                || trackingGeneration == 0
                || !IsTrackedMotion(
                    monitoredGroup.GroupName,
                    trackingGeneration))
            {
                return;
            }

            await MonitorGroupFiniteMotionAsync(
                "Move Linear Absolute",
                monitoredGroup,
                trackingGeneration,
                noMovementExpected,
                monitorTimeoutMilliseconds);
        }

        private async void ButtonGroupMoveLinearRelative_Click(
            object sender,
            RoutedEventArgs e)
        {
            const string operation = "Move Linear Relative";
            if (!CanStartLiveCommand(operation))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            LMCGroupAxis monitoredGroup = null;
            var trackingGeneration = 0;
            var noMovementExpected = false;
            var monitorTimeoutMilliseconds =
                MinimumGroupMotionMonitorMilliseconds;

            await RunOperationAsync(
                operation + " Send",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var currentGroup = RequireGroup();
                    EnsureGroupReadyForMotion();
                    var input = ReadGroupMotionInput();
                    var startPosition = await currentGroup
                        .GroupReadActualPositionAsync(
                            input.Options.CoordinateSystem,
                            CancellationToken.None);
                    EnsureGroupPositionSuccess(
                        operation + " start position",
                        startPosition);

                    var currentPositions = startPosition.PositionsRaw;
                    var monitorDistances = input.PositionsRaw
                        .Select(distance => (long)distance)
                        .ToArray();
                    noMovementExpected = monitorDistances
                        .Take(4)
                        .All(distance => distance == 0);
                    monitorTimeoutMilliseconds =
                        CalculateGroupMotionMonitorTimeoutMilliseconds(
                            monitorDistances,
                            input.VelocityRaw,
                            input.AccelerationRaw,
                            input.DecelerationRaw);
                    WriteLog(
                        operation
                        + " input: StartRaw="
                        + FormatGroupPositionsRaw(currentPositions)
                        + ", DeltaRaw="
                        + FormatGroupPositionsRaw(input.PositionsRaw)
                        + ", VelocityRaw="
                        + input.VelocityRaw
                        + ", AccelerationRaw="
                        + input.AccelerationRaw
                        + ", DecelerationRaw="
                        + input.DecelerationRaw
                        + ", JerkRaw="
                        + input.JerkRaw
                        + ", Transition="
                        + input.Options.TransitionMode
                        + ", Buffer="
                        + input.Options.BufferMode
                        + ", MonitorTimeoutMs="
                        + monitorTimeoutMilliseconds
                        + ".");
                    monitoredGroup = currentGroup;
                    var verifiedCapabilities = await currentConnection.Admin
                        .GetCapabilitiesAsync(CancellationToken.None);
                    if (!verifiedCapabilities.Supports(
                            LMCAdminFeature.GroupLinearRelative)
                        || verifiedCapabilities.GroupReference
                            != currentGroup.GroupReference)
                    {
                        throw new NotSupportedException(
                            "The connected PLC does not advertise the group "
                            + "linear-relative motion facade for the loaded "
                            + "group.");
                    }

                    try
                    {
                        var response = await SendLiveCommandAsync(
                            safetyGeneration,
                            operation,
                            async () =>
                            {
                                trackingGeneration = MarkMotionUncertain(
                                    currentGroup.GroupName,
                                    operation);
                                return await currentGroup
                                    .MoveLinearRelativeExAsync(
                                        input.PositionsRaw,
                                        input.VelocityRaw,
                                        input.AccelerationRaw,
                                        input.DecelerationRaw,
                                        input.JerkRaw,
                                        input.Options,
                                        verifiedCapabilities,
                                        CancellationToken.None);
                            });

                        ClearMotionOnConfirmedRejection(
                            currentGroup.GroupName,
                            operation,
                            response);
                        EnsureAdminResponseSuccess(operation, response);
                        TextGroupResult.Text =
                            FormatAdminResponse(response)
                            + Environment.NewLine
                            + "Command accepted; monitoring Group InPosition "
                            + "with timeout "
                            + monitorTimeoutMilliseconds
                            + " ms.";
                    }
                    catch (LMCAdminCommandException error)
                    {
                        ClearMotionOnConfirmedRejection(
                            currentGroup.GroupName,
                            operation,
                            error.Response);
                        TextGroupResult.Text = FormatAdminResponse(
                            error.Response);
                        throw;
                    }
                });

            if (monitoredGroup == null
                || trackingGeneration == 0
                || !IsTrackedMotion(
                    monitoredGroup.GroupName,
                    trackingGeneration))
            {
                return;
            }

            await MonitorGroupFiniteMotionAsync(
                operation,
                monitoredGroup,
                trackingGeneration,
                noMovementExpected,
                monitorTimeoutMilliseconds);
        }

        private async void ButtonCheckKinHome_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Identity Home Check",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    RequireGroup();
                    var homeCheck = await CheckIdentityAxesHomeAsync(
                        currentConnection,
                        CancellationToken.None);
                    if (!homeCheck.AllReferenced)
                    {
                        throw new InvalidOperationException(
                            "Home Check failed. Reference the following identity "
                            + "axes before Set Identity: "
                            + homeCheck.UnreferencedAxisSummary
                            + ".");
                    }
                });
        }

        private async void ButtonSetKinTransform_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!CanStartLiveCommand("Set Identity Kinematics"))
            {
                return;
            }

            var safetyGeneration = safetyRequestGeneration;
            await RunOperationAsync(
                "Set Identity Kinematics",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    var currentGroup = RequireGroup();
                    EnsureGroupActiveVerified();
                    if (groupProfileLocked
                        || groupProfileLockVerificationPending)
                    {
                        throw new InvalidOperationException(
                            "Finish 5 Read Status verification or Disable "
                            + "(Unlock Profile) before changing the identity "
                            + "configuration.");
                    }

                    groupIdentityConfigured = false;
                    var homeCheck = await CheckIdentityAxesHomeAsync(
                        currentConnection,
                        CancellationToken.None);
                    if (!homeCheck.AllReferenced)
                    {
                        groupIdentityConfigured = false;
                        throw new InvalidOperationException(
                            "Set Identity blocked because these identity axes "
                            + "are not referenced: "
                            + homeCheck.UnreferencedAxisSummary
                            + ". Run Home, then retry Set Identity.");
                    }

                    var axisX = homeCheck.AxisX.Axis;
                    var axisY = homeCheck.AxisY.Axis;
                    var axisZ = homeCheck.AxisZ.Axis;
                    var axisU = homeCheck.AxisU.Axis;

                    var response = await SendLiveCommandAsync(
                        safetyGeneration,
                        "Set Identity Kinematics",
                        () => currentGroup
                            .SetKinTransformCartesian4AxisAsync(
                                axisX,
                                axisY,
                                axisZ,
                                axisU,
                                CancellationToken.None));
                    EnsureResponseSuccess(
                        "Set Identity Kinematics",
                        response);
                    groupIdentityConfigured = true;
                    TextGroupResult.Text =
                        FormatResponse(response)
                        + Environment.NewLine
                        + "X="
                        + axisX.AxisName
                        + " ("
                        + axisX.AxisReference
                        + "), Y="
                        + axisY.AxisName
                        + " ("
                        + axisY.AxisReference
                        + "), Z="
                        + axisZ.AxisName
                        + " ("
                        + axisZ.AxisReference
                        + "), U="
                        + axisU.AxisName
                        + " ("
                        + axisU.AxisReference
                        + ")"
                        + Environment.NewLine
                        + "Identity configured; Enable (Lock Profile) is now "
                        + "available.";
                });
        }

        private async Task<IdentityHomeCheckResult> CheckIdentityAxesHomeAsync(
            LMCConnection currentConnection,
            CancellationToken cancellationToken)
        {
            groupIdentityHomeCheckComplete = false;
            groupIdentityHomeCheckPassed = false;

            try
            {
                var result = new IdentityHomeCheckResult(
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "X",
                        RequiredText(TextKinAxisX.Text, "X axis object"),
                        cancellationToken),
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "Y",
                        RequiredText(TextKinAxisY.Text, "Y axis object"),
                        cancellationToken),
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "Z",
                        RequiredText(TextKinAxisZ.Text, "Z axis object"),
                        cancellationToken),
                    await ReadIdentityAxisHomeAsync(
                        currentConnection,
                        "U",
                        RequiredText(TextKinAxisU.Text, "U axis object"),
                        cancellationToken));

                groupIdentityHomeCheckComplete = true;
                groupIdentityHomeCheckPassed = result.AllReferenced;
                if (!result.AllReferenced)
                {
                    groupIdentityConfigured = false;
                }

                DisplayIdentityHomeCheck(result);
                WriteLog(
                    "Identity Home Check "
                    + (result.AllReferenced ? "PASS" : "FAIL")
                    + ": "
                    + result.ReferencedCount
                    + "/4 axes referenced.");
                UpdateUiState();
                return result;
            }
            catch (Exception error)
            {
                groupIdentityConfigured = false;
                if (TextKinHomeStatus != null)
                {
                    TextKinHomeStatus.Text =
                        "Home Check ERROR"
                        + Environment.NewLine
                        + error.Message;
                }

                UpdateUiState();
                throw;
            }
        }

        private static async Task<IdentityAxisHomeStatus>
            ReadIdentityAxisHomeAsync(
                LMCConnection currentConnection,
                string coordinateName,
                string axisName,
                CancellationToken cancellationToken)
        {
            var selectedAxis = await LMCSingleAxis.CreateAsync(
                currentConnection,
                axisName,
                cancellationToken);
            var status = await selectedAxis.ReadStatusResultAsync(
                cancellationToken);
            EnsureAxisStatusSuccess(
                coordinateName + " axis Home Check",
                status);
            return new IdentityAxisHomeStatus(
                coordinateName,
                selectedAxis,
                status);
        }

        private void DisplayIdentityHomeCheck(IdentityHomeCheckResult result)
        {
            var axisLines = result.Axes.Select(
                item =>
                    item.CoordinateName
                    + " "
                    + item.Axis.AxisName
                    + " Ref="
                    + item.Axis.AxisReference
                    + " Home/Referenced="
                    + item.Status.IsReferenced
                    + " PowerOn="
                    + item.Status.IsPowerOn
                    + " Standstill="
                    + item.Status.IsStandstill
                    + " State=0x"
                    + item.Status.State.ToString("X8"));

            TextKinHomeStatus.Text =
                "Home Check "
                + (result.AllReferenced ? "PASS" : "FAIL")
                + " ("
                + result.ReferencedCount
                + "/4 referenced) at "
                + DateTime.Now.ToString(
                    "HH:mm:ss.fff",
                    CultureInfo.InvariantCulture)
                + Environment.NewLine
                + string.Join(Environment.NewLine, axisLines);
        }

        private void TextKinAxis_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            groupIdentityConfigured = false;
            ResetIdentityHomeCheckState();
            if (TextKinHomeStatus == null)
            {
                return;
            }

            UpdateUiState();
        }

        private void TextAxisName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (axis == null || TextAxisName == null)
            {
                return;
            }

            if (!string.Equals(
                axis.AxisName,
                TextAxisName.Text.Trim(),
                StringComparison.Ordinal))
            {
                axis = null;
                if (TextAxisReference != null)
                {
                    TextAxisReference.Text = "not loaded";
                }

                UpdateUiState();
            }
        }

        private void TextGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (group == null || TextGroupName == null)
            {
                return;
            }

            if (!string.Equals(
                group.GroupName,
                TextGroupName.Text.Trim(),
                StringComparison.Ordinal))
            {
                group = null;
                ResetGroupPreparationState();
                if (TextGroupReference != null)
                {
                    TextGroupReference.Text = "not loaded";
                }

                UpdateUiState();
            }
        }

        private void ButtonCopyLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TextExecutionLog.Text ?? string.Empty);
                TextOperationState.Text = "Log copied";
            }
            catch (Exception error)
            {
                WriteLog("Copy Log failed: " + error.Message);
            }
        }

        private void ButtonClearLog_Click(object sender, RoutedEventArgs e)
        {
            TextExecutionLog.Clear();
            TextOperationState.Text = "Log cleared";
        }

        private async Task RunOperationAsync(
            string operation,
            Func<Task> action,
            bool blockSafetyCommands = false)
        {
            if (operationRunning
                || safetyCommandRunning
                || safetyMonitorCount > 0
                || qualificationRunning)
            {
                WriteLog(
                    "Another operation, safety verification, or qualification is already running.");
                return;
            }

            operationRunning = true;
            connectionTransitionRunning = blockSafetyCommands;
            TextOperationState.Text = operation + " running";
            UpdateUiState();

            try
            {
                WriteLog(operation + " started.");
                await action();
                WriteLog(operation + " PASS.");
                TextOperationState.Text = operation + " completed";
            }
            catch (Exception error)
            {
                WriteLog(operation + " FAILED: " + error.Message);
                TextOperationState.Text = operation + " failed";
            }
            finally
            {
                operationRunning = false;
                connectionTransitionRunning = false;
                UpdateUiState();
            }
        }

        private async Task<bool> RunSafetyCommandAsync(
            string operation,
            Func<Task> action,
            Action safetyReserved = null)
        {
            safetyRequestGeneration++;
            if (safetyCommandRunning)
            {
                WriteLog("Another Stop or Power Off send is already running.");
                return false;
            }

            safetyCommandRunning = true;
            TextOperationState.Text = operation + " running";
            UpdateUiState();

            try
            {
                if (safetyReserved != null)
                {
                    safetyReserved();
                }

                WriteLog(operation + " queued with safety priority.");
                await commandSendGate.WaitAsync();
                try
                {
                    WriteLog(operation + " transmitting.");
                    await action();
                }
                finally
                {
                    commandSendGate.Release();
                }

                WriteLog(operation + " PASS.");
                TextOperationState.Text = operation + " accepted";
                return true;
            }
            catch (Exception error)
            {
                WriteLog(operation + " FAILED: " + error.Message);
                TextOperationState.Text = operation + " failed";
                return false;
            }
            finally
            {
                safetyCommandRunning = false;
                UpdateUiState();
            }
        }

        private async Task<T> SendLiveCommandAsync<T>(
            int expectedSafetyGeneration,
            string operation,
            Func<Task<T>> send)
        {
            await commandSendGate.WaitAsync();
            try
            {
                EnsureNoUnresolvedD5SdoQualificationTicket(operation);
                EnsureNoNewSafetyRequest(
                    expectedSafetyGeneration,
                    operation);
                return await send();
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task<T> SendSerializedCommandAsync<T>(Func<Task<T>> send)
        {
            await commandSendGate.WaitAsync();
            try
            {
                return await send();
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private void EnsureNoNewSafetyRequest(
            int expectedGeneration,
            string operation)
        {
            if (expectedGeneration == safetyRequestGeneration)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " was cancelled before transmission because Stop or Power Off "
                + "was requested.");
        }

        private async Task RunSafetyMonitorAsync(
            string operation,
            LMCSingleAxis currentAxis,
            Func<Task<LMCReadStatusResult>> verifySafeState)
        {
            safetyMonitorCount++;
            TextOperationState.Text = operation + " verifying standstill";
            WriteLog(
                operation
                + " accepted. Verifying three stable Standstill samples; "
                + "Stop and Power Off remain available.");
            UpdateUiState();

            try
            {
                var status = await verifySafeState();
                DisplayAxisStatus(status);
                ClearMotionWarning(
                    operation + " and stable standstill were verified");
                WriteLog(operation + " safety verification PASS.");
                TextOperationState.Text = operation + " verified";
            }
            catch (Exception error)
            {
                WriteLog(
                    operation
                    + " safety verification FAILED: "
                    + error.Message
                    + " Do not assume the axis is stopped.");
                TextOperationState.Text = operation + " verification failed";
            }
            finally
            {
                safetyMonitorCount--;
                UpdateUiState();
            }
        }

        private async Task RunGroupSafetyMonitorAsync(
            string operation,
            LMCGroupAxis currentGroup)
        {
            safetyMonitorCount++;
            TextOperationState.Text = operation + " verifying InPosition";
            WriteLog(
                operation
                + " accepted. Verifying three stable Group InPosition samples; "
                + "Group Stop remains available.");
            UpdateUiState();

            try
            {
                var status = await WaitForGroupInPositionAsync(
                    currentGroup,
                    5000,
                    50);
                DisplayGroupStatus(status);
                ClearMotionWarning(
                    operation + " and stable Group InPosition were verified");
                WriteLog(operation + " safety verification PASS.");
                TextOperationState.Text = operation + " verified";
            }
            catch (Exception error)
            {
                WriteLog(
                    operation
                    + " safety verification FAILED: "
                    + error.Message
                    + " Do not assume the group is stopped.");
                TextOperationState.Text = operation + " verification failed";
            }
            finally
            {
                safetyMonitorCount--;
                UpdateUiState();
            }
        }

        private async Task RunGroupPowerOffSafetyMonitorAsync(
            string operation,
            LMCGroupAxis currentGroup)
        {
            safetyMonitorCount++;
            TextOperationState.Text = operation + " verifying PowerOn=False";
            WriteLog(
                operation
                + " accepted. Verifying three stable PowerOn=False samples; "
                + "Group Stop remains available.");
            UpdateUiState();

            try
            {
                var deadline = DateTime.UtcNow.AddSeconds(5);
                var stableSamples = 0;
                LMCGroupReadStatusResult latest = null;
                while (DateTime.UtcNow < deadline)
                {
                    latest = await currentGroup.GroupReadStatusResultAsync(
                        CancellationToken.None);
                    EnsureGroupStatusSuccess(
                        operation + " verification",
                        latest);
                    stableSamples = !latest.IsPowerOn
                        ? stableSamples + 1
                        : 0;
                    if (stableSamples >= 3)
                    {
                        groupPowerVerificationPending = false;
                        groupPowerOffVerificationPending = false;
                        groupStatusRefreshRequired = false;
                        groupActiveVerified = false;
                        groupIdentityConfigured = false;
                        ResetIdentityHomeCheckState();
                        groupProfileLockVerificationPending = false;
                        groupProfileLockDisabledSamples = 0;
                        groupProfileLocked = false;
                        DisplayGroupStatus(latest);
                        ClearMotionWarning(
                            operation
                            + " and three stable PowerOn=False samples were verified");
                        WriteLog(operation + " safety verification PASS.");
                        TextOperationState.Text = operation + " verified";
                        return;
                    }

                    await Task.Delay(50);
                }

                throw new TimeoutException(
                    currentGroup.GroupName
                    + " did not report three stable PowerOn=False samples within 5000 ms.");
            }
            catch (Exception error)
            {
                WriteLog(
                    operation
                    + " safety verification FAILED: "
                    + error.Message
                    + " Do not assume the group is powered off or stopped.");
                TextOperationState.Text = operation + " verification failed";
                ButtonGroupReadStatus.Focus();
            }
            finally
            {
                safetyMonitorCount--;
                UpdateUiState();
            }
        }

        private async Task CloseCurrentConnectionAsync(bool reportCloseError)
        {
            if (motionMayBeActive)
            {
                throw new InvalidOperationException(
                    "Close is blocked because "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + ". Send Stop or PowerOff and verify standstill first.");
            }

            var currentConnection = connection;
            if (currentConnection == null)
            {
                return;
            }

            Exception closeError = null;
            try
            {
                await currentConnection.CloseConnectionAsync(
                    CancellationToken.None);
            }
            catch (Exception error)
            {
                closeError = error;
            }
            finally
            {
                if (ReferenceEquals(connection, currentConnection))
                {
                    connection = null;
                }

                DetachConnection(currentConnection);
                currentConnection.Dispose();
                ClearLoadedObjects();
                UpdateUiState();
            }

            if (closeError == null)
            {
                return;
            }

            if (reportCloseError)
            {
                throw closeError;
            }

            WriteLog("Connection cleanup warning: " + closeError.Message);
        }

        private void AttachConnection(LMCConnection newConnection)
        {
            newConnection.ConnectionStateChanged += Connection_StateChanged;
            newConnection.CallbackReceived += Connection_CallbackReceived;
            newConnection.CallbackListenerError +=
                Connection_CallbackListenerError;
        }

        private void DetachConnection(LMCConnection oldConnection)
        {
            oldConnection.ConnectionStateChanged -= Connection_StateChanged;
            oldConnection.CallbackReceived -= Connection_CallbackReceived;
            oldConnection.CallbackListenerError -=
                Connection_CallbackListenerError;
        }

        private void Connection_StateChanged(
            object sender,
            LMCConnectionStateChangedEventArgs e)
        {
            RunOnUi(
                () =>
                {
                    if (!ReferenceEquals(sender, connection))
                    {
                        return;
                    }

                    WriteLog(
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
            var previewLength = Math.Min(payload.Length, 48);
            var preview = previewLength == 0
                ? "<empty>"
                : BitConverter.ToString(payload, 0, previewLength);
            if (payload.Length > previewLength)
            {
                preview += "-...";
            }

            RunOnUi(
                () =>
                {
                    if (!ReferenceEquals(sender, connection))
                    {
                        return;
                    }

                    WriteLog(
                        "Raw callback UTC="
                        + e.ReceivedAtUtc.ToString("O")
                        + ", Remote="
                        + e.RemoteEndPoint
                        + ", Bytes="
                        + payload.Length
                        + ", Data="
                        + preview);
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
                    if (!ReferenceEquals(sender, connection))
                    {
                        return;
                    }

                    WriteLog(
                        "Callback listener error: "
                        + (e.Exception == null
                            ? "unknown error"
                            : e.Exception.Message));
                    UpdateUiState();
                });
        }

        private void RunOnUi(Action action)
        {
            if (shutdownInProgress || Dispatcher.HasShutdownStarted)
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

        private async Task EnsureAxisPoweredOnAsync(LMCSingleAxis currentAxis)
        {
            var status = await currentAxis.ReadStatusResultAsync(
                CancellationToken.None);
            EnsureAxisStatusSuccess("Motion power check", status);
            if (!status.IsPowerOn)
            {
                throw new InvalidOperationException(
                    "Motion is blocked because Read Status reports PowerOn=false.");
            }
        }

        private async Task<LMCReadStatusResult> WaitForPowerStateAsync(
            LMCSingleAxis currentAxis,
            bool expectedPowerOn,
            int timeoutMilliseconds)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                var status = await currentAxis.ReadStatusResultAsync(
                    CancellationToken.None);
                EnsureAxisStatusSuccess("Power state verification", status);
                if (status.IsPowerOn == expectedPowerOn)
                {
                    return status;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentAxis.AxisName
                + " did not reach PowerOn="
                + expectedPowerOn
                + " within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCReadStatusResult> WaitForStandstillAsync(
            LMCSingleAxis currentAxis,
            int timeoutMilliseconds,
            int minimumDelayMilliseconds)
        {
            if (minimumDelayMilliseconds > 0)
            {
                await Task.Delay(minimumDelayMilliseconds);
            }

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            LMCReadStatusResult latest = null;
            while (DateTime.UtcNow < deadline)
            {
                latest = await currentAxis.ReadStatusResultAsync(
                    CancellationToken.None);
                EnsureAxisStatusSuccess("Standstill verification", latest);
                if (latest.IsStandstill)
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return latest;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentAxis.AxisName
                + " did not report stable standstill within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCReadStatusResult>
            WaitForFiniteMotionCompletionAsync(
                LMCSingleAxis currentAxis,
                int trackingGeneration,
                bool noMovementExpected,
                int timeoutMilliseconds)
        {
            await Task.Delay(250);

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            while (DateTime.UtcNow < deadline)
            {
                if (!IsTrackedMotion(
                    currentAxis.AxisName,
                    trackingGeneration))
                {
                    return null;
                }

                if (operationRunning
                    || safetyCommandRunning
                    || safetyMonitorCount > 0)
                {
                    await Task.Delay(25);
                    continue;
                }

                var status = await currentAxis.ReadStatusResultAsync(
                    CancellationToken.None);
                if (!IsTrackedMotion(
                    currentAxis.AxisName,
                    trackingGeneration))
                {
                    return null;
                }

                EnsureAxisStatusSuccess(
                    "Finite motion standstill verification",
                    status);
                if (!status.IsStandstill)
                {
                    RecordMotionObserved(currentAxis.AxisName);
                    stableSamples = 0;
                }
                else if (!status.IsPowerOn
                    || noMovementExpected
                    || motionWasObserved)
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return status;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentAxis.AxisName
                + " did not show movement followed by three stable safe samples "
                + "within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCGroupReadStatusResult>
            WaitForGroupInPositionAsync(
                LMCGroupAxis currentGroup,
                int timeoutMilliseconds,
                int minimumDelayMilliseconds)
        {
            if (minimumDelayMilliseconds > 0)
            {
                await Task.Delay(minimumDelayMilliseconds);
            }

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            LMCGroupReadStatusResult latest = null;
            while (DateTime.UtcNow < deadline)
            {
                latest = await currentGroup.GroupReadStatusResultAsync(
                    CancellationToken.None);
                EnsureGroupStatusSuccess(
                    "Group InPosition verification",
                    latest);
                if (IsGroupInPosition(latest))
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return latest;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentGroup.GroupName
                + " did not report stable Group InPosition within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<LMCGroupReadStatusResult>
            WaitForGroupMotionCompletionAsync(
                LMCGroupAxis currentGroup,
                int trackingGeneration,
                bool noMovementExpected,
                int timeoutMilliseconds)
        {
            await Task.Delay(250);

            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stableSamples = 0;
            while (DateTime.UtcNow < deadline)
            {
                if (!IsTrackedMotion(
                    currentGroup.GroupName,
                    trackingGeneration))
                {
                    return null;
                }

                if (operationRunning
                    || safetyCommandRunning
                    || safetyMonitorCount > 0)
                {
                    await Task.Delay(25);
                    continue;
                }

                var status = await currentGroup.GroupReadStatusResultAsync(
                    CancellationToken.None);
                if (!IsTrackedMotion(
                    currentGroup.GroupName,
                    trackingGeneration))
                {
                    return null;
                }

                EnsureGroupStatusSuccess(
                    "Group motion completion verification",
                    status);
                if (!IsGroupInPosition(status))
                {
                    RecordMotionObserved(currentGroup.GroupName);
                    stableSamples = 0;
                }
                else if (noMovementExpected || motionWasObserved)
                {
                    stableSamples++;
                    if (stableSamples >= 3)
                    {
                        return status;
                    }
                }
                else
                {
                    stableSamples = 0;
                }

                await Task.Delay(50);
            }

            throw new TimeoutException(
                currentGroup.GroupName
                + " did not show motion followed by three stable Group "
                + "InPosition samples within "
                + timeoutMilliseconds
                + " ms.");
        }

        private bool CanStartLiveCommand(string operation)
        {
            if (HasUnresolvedD5SdoQualificationTicket)
            {
                WriteLog(
                    operation
                    + " blocked while a D5 ticket, submission outcome, or Write readback is unresolved. "
                    + GetD5SdoResolutionGuidance());
                return false;
            }

            if (motionMayBeActive)
            {
                WriteLog(
                    operation
                    + " blocked because "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + ".");
                return false;
            }

            return true;
        }

        private int MarkMotionUncertain(
            string currentAxisName,
            string operation)
        {
            motionTrackingGeneration++;
            motionMayBeActive = true;
            motionAxisName = currentAxisName;
            motionOperation = operation;
            motionWasObserved = false;
            WriteLog(
                "SAFETY: "
                + operation
                + " send may start for "
                + currentAxisName
                + ". Motion state is uncertain until rejection or verified standstill.");
            UpdateUiState();
            return motionTrackingGeneration;
        }

        private void ClearMotionOnConfirmedRejection(
            string currentAxisName,
            string operation,
            LMC_Response response)
        {
            if (response != null
                && response.IsFrameValid
                && !response.IsSuccess
                && IsTrackedMotionAxis(currentAxisName))
            {
                ClearMotionWarning(operation + " was rejected by a valid response");
            }
        }

        private void ClearMotionOnConfirmedRejection(
            string currentAxisName,
            string operation,
            LMCAdminResponse response)
        {
            if (response != null
                && response.TransportResponse != null
                && response.TransportResponse.IsFrameValid
                && !response.IsSuccess
                && IsTrackedMotionAxis(currentAxisName))
            {
                ClearMotionWarning(operation + " was rejected by a valid response");
            }
        }

        private void ClearMotionWarning(
            string reason,
            int? expectedTrackingGeneration = null)
        {
            if (!motionMayBeActive
                || (expectedTrackingGeneration.HasValue
                    && expectedTrackingGeneration.Value
                        != motionTrackingGeneration))
            {
                return;
            }

            motionTrackingGeneration++;
            motionMayBeActive = false;
            motionAxisName = null;
            motionOperation = null;
            motionWasObserved = false;
            WriteLog("Motion warning cleared: " + reason + ".");
            UpdateUiState();
        }

        private void RecordMotionObserved(string currentAxisName)
        {
            if (!IsTrackedMotionAxis(currentAxisName) || motionWasObserved)
            {
                return;
            }

            motionWasObserved = true;
            WriteLog(
                "SAFETY: Non-standstill motion was observed for "
                + currentAxisName
                + ".");
        }

        private bool IsTrackedMotionAxis(string currentAxisName)
        {
            return motionMayBeActive
                && string.Equals(
                    motionAxisName,
                    currentAxisName,
                    StringComparison.Ordinal);
        }

        private bool IsTrackedMotion(
            string currentAxisName,
            int trackingGeneration)
        {
            return IsTrackedMotionAxis(currentAxisName)
                && motionTrackingGeneration == trackingGeneration;
        }

        private MotionInput ReadFiniteMotionInput()
        {
            var unit = ReadAxisUnitSelection();
            return new MotionInput
            {
                PositionRaw = ToLasalDint(TextPosition.Text, unit, "Position"),
                VelocityRaw = ToPositiveLasalDint(
                    TextVelocity.Text,
                    unit,
                    "Velocity"),
                AccelerationRaw = ToPositiveLasalDint(
                    TextAcceleration.Text,
                    unit,
                    "Acceleration"),
                DecelerationRaw = ToPositiveLasalDint(
                    TextDeceleration.Text,
                    unit,
                    "Deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextJerk.Text,
                    unit,
                    "Jerk")
            };
        }

        private MotionInput ReadVelocityMotionInput()
        {
            var unit = ReadAxisUnitSelection();
            if (!(ComboDirection.SelectedItem is LMC_DIRECTION direction)
                || (direction != LMC_DIRECTION.Positive
                    && direction != LMC_DIRECTION.Negative))
            {
                throw new InvalidOperationException(
                    "Velocity direction must be Positive or Negative.");
            }

            return new MotionInput
            {
                VelocityRaw = ToPositiveLasalDint(
                    TextVelocity.Text,
                    unit,
                    "Velocity"),
                AccelerationRaw = ToPositiveLasalDint(
                    TextAcceleration.Text,
                    unit,
                    "Acceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextJerk.Text,
                    unit,
                    "Jerk"),
                Direction = direction
            };
        }

        private MotionInput ReadStopInput()
        {
            var unit = ReadAxisUnitSelection();
            return new MotionInput
            {
                DecelerationRaw = ToPositiveLasalDint(
                    TextDeceleration.Text,
                    unit,
                    "Stop deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextJerk.Text,
                    unit,
                    "Stop jerk")
            };
        }

        private GroupMotionInput ReadGroupMotionInput()
        {
            var unit = ReadGroupUnitSelection();
            if (!(ComboGroupTransition.SelectedItem
                is LMC_GROUP_TRANSITION_MODE transitionMode)
                || (transitionMode != LMC_GROUP_TRANSITION_MODE.ExactStop
                    && transitionMode
                        != LMC_GROUP_TRANSITION_MODE.ContinuousDirect))
            {
                throw new InvalidOperationException(
                    "Group transition must be ExactStop or ContinuousDirect.");
            }

            if (!(ComboGroupBuffer.SelectedItem is LMC_BUFFER_MODE bufferMode)
                || (bufferMode != LMC_BUFFER_MODE.Aborting
                    && bufferMode != LMC_BUFFER_MODE.Buffered))
            {
                throw new InvalidOperationException(
                    "Group buffer mode must be Aborting or Buffered.");
            }

            return new GroupMotionInput
            {
                PositionsRaw = new[]
                {
                    ToLasalDint(
                        TextGroupPositionX.Text,
                        unit,
                        "Group X target"),
                    ToLasalDint(
                        TextGroupPositionY.Text,
                        unit,
                        "Group Y target"),
                    ToLasalDint(
                        TextGroupPositionZ.Text,
                        unit,
                        "Group Z target"),
                    ToLasalDint(
                        TextGroupPositionU.Text,
                        unit,
                        "Group U target")
                },
                VelocityRaw = ToPositiveLasalDint(
                    TextGroupVelocity.Text,
                    unit,
                    "Group velocity"),
                AccelerationRaw = ToPositiveLasalDint(
                    TextGroupAcceleration.Text,
                    unit,
                    "Group acceleration"),
                DecelerationRaw = ToPositiveLasalDint(
                    TextGroupDeceleration.Text,
                    unit,
                    "Group deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextGroupJerk.Text,
                    unit,
                    "Group jerk"),
                Options = new LMCGroupMotionOptions
                {
                    CoordinateSystem = ReadGroupMotionCoordinateSystem(),
                    TransitionMode = transitionMode,
                    BufferMode = bufferMode,
                    Execute = true
                }
            };
        }

        private GroupMotionInput ReadGroupStopInput()
        {
            var unit = ReadGroupUnitSelection();
            return new GroupMotionInput
            {
                DecelerationRaw = ToPositiveLasalDint(
                    TextGroupDeceleration.Text,
                    unit,
                    "Group stop deceleration"),
                JerkRaw = ToNonNegativeLasalDint(
                    TextGroupJerk.Text,
                    unit,
                    "Group stop jerk")
            };
        }

        private LMC_COORD_SYSTEM ReadGroupPositionCoordinateSystem()
        {
            if (!(ComboGroupCoordinate.SelectedItem
                is LMC_COORD_SYSTEM coordinateSystem)
                || (coordinateSystem != LMC_COORD_SYSTEM.None
                    && coordinateSystem != LMC_COORD_SYSTEM.Acs))
            {
                throw new InvalidOperationException(
                    "Group Read Position supports Coordinate=None or ACS only.");
            }

            return coordinateSystem;
        }

        private LMC_COORD_SYSTEM ReadGroupMotionCoordinateSystem()
        {
            if (!(ComboGroupCoordinate.SelectedItem
                is LMC_COORD_SYSTEM coordinateSystem)
                || coordinateSystem != LMC_COORD_SYSTEM.None)
            {
                throw new InvalidOperationException(
                    "Group motion currently supports Coordinate=None only. "
                    + "Select None before Move Linear.");
            }

            return coordinateSystem;
        }

        private static int CalculateGroupMotionMonitorTimeoutMilliseconds(
            long[] distancesRaw,
            int velocityRaw,
            int accelerationRaw,
            int decelerationRaw)
        {
            if (distancesRaw == null || distancesRaw.Length < 4)
            {
                throw new ArgumentException(
                    "Group monitor distance requires four XYZU values.",
                    nameof(distancesRaw));
            }

            if (velocityRaw <= 0
                || accelerationRaw <= 0
                || decelerationRaw <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(velocityRaw),
                    "Group monitor dynamics must be positive.");
            }

            var conservativePathRaw = distancesRaw
                .Take(4)
                .Sum(distance => Math.Abs((double)distance));
            var nominalSeconds = conservativePathRaw / velocityRaw;
            var accelerationSeconds = velocityRaw / (double)accelerationRaw;
            var decelerationSeconds = velocityRaw / (double)decelerationRaw;
            var estimatedMilliseconds = Math.Ceiling(
                ((nominalSeconds
                    + accelerationSeconds
                    + decelerationSeconds)
                    * 1.25
                    + 5.0)
                * 1000.0);

            return (int)Math.Max(
                MinimumGroupMotionMonitorMilliseconds,
                Math.Min(
                    MaximumGroupMotionMonitorMilliseconds,
                    estimatedMilliseconds));
        }

        private PlcUnitOption ReadGroupUnitSelection()
        {
            var unit = ComboGroupUnit.SelectedItem as PlcUnitOption;
            if (unit == null)
            {
                throw new InvalidOperationException(
                    "Select a group PLC application UNIT.");
            }

            return unit;
        }

        private PlcUnitOption ReadAxisUnitSelection()
        {
            var unit = ComboAxisUnit.SelectedItem as PlcUnitOption;
            if (unit == null)
            {
                throw new InvalidOperationException(
                    "Select an axis PLC application UNIT.");
            }

            return unit;
        }

        private static int ToLasalDint(
            string value,
            PlcUnitOption unit,
            string fieldName)
        {
            if (unit.IsRaw)
            {
                return ParseRawDint(value, fieldName);
            }

            var engineeringValue = ParseDouble(value, fieldName);
            return ScaleToLasalDint(
                engineeringValue,
                unit.Multiplier,
                fieldName);
        }

        private static int ToPositiveLasalDint(
            string value,
            PlcUnitOption unit,
            string fieldName)
        {
            if (unit.IsRaw)
            {
                var rawValue = ParseRawDint(value, fieldName);
                if (rawValue <= 0)
                {
                    throw new InvalidOperationException(
                        fieldName + " raw DINT must be greater than zero.");
                }

                return rawValue;
            }

            var engineeringValue = ParseDouble(value, fieldName);
            if (engineeringValue <= 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be greater than zero.");
            }

            var raw = ScaleToLasalDint(
                engineeringValue,
                unit.Multiplier,
                fieldName);
            if (raw <= 0)
            {
                throw new InvalidOperationException(
                    fieldName + " multiplied by UNIT must be at least 1 DINT count.");
            }

            return raw;
        }

        private static int ToNonNegativeLasalDint(
            string value,
            PlcUnitOption unit,
            string fieldName)
        {
            if (unit.IsRaw)
            {
                var rawValue = ParseRawDint(value, fieldName);
                if (rawValue < 0)
                {
                    throw new InvalidOperationException(
                        fieldName + " raw DINT must be zero or greater.");
                }

                return rawValue;
            }

            var engineeringValue = ParseDouble(value, fieldName);
            if (engineeringValue < 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be zero or greater.");
            }

            var raw = ScaleToLasalDint(
                engineeringValue,
                unit.Multiplier,
                fieldName);
            if (engineeringValue > 0 && raw <= 0)
            {
                throw new InvalidOperationException(
                    fieldName + " multiplied by UNIT must be at least 1 DINT count.");
            }

            return raw;
        }

        private static int ScaleToLasalDint(
            double engineeringValue,
            double unitMultiplier,
            string fieldName)
        {
            var scaled = engineeringValue * unitMultiplier;
            if (double.IsNaN(scaled) || double.IsInfinity(scaled))
            {
                throw new OverflowException(
                    fieldName + " multiplied by UNIT is not finite.");
            }

            var rounded = Math.Round(
                scaled,
                0,
                MidpointRounding.AwayFromZero);
            if (rounded < int.MinValue || rounded > int.MaxValue)
            {
                throw new OverflowException(
                    fieldName + " multiplied by UNIT is outside DINT range.");
            }

            return checked((int)rounded);
        }

        private static int ParseRawDint(string value, string fieldName)
        {
            int parsed;
            if (!int.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out parsed))
            {
                throw new InvalidOperationException(
                    fieldName
                    + " must be an integer in the DINT range when UNIT is None / raw DINT.");
            }

            return parsed;
        }

        private static double ParseDouble(string value, string fieldName)
        {
            double parsed;
            if (!double.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out parsed)
                || double.IsNaN(parsed)
                || double.IsInfinity(parsed))
            {
                throw new InvalidOperationException(
                    fieldName + " must be a finite number using '.' as decimal separator.");
            }

            return parsed;
        }

        private static int ParsePort(
            string value,
            string fieldName,
            bool allowZero)
        {
            int parsed;
            if (!int.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out parsed)
                || parsed < (allowZero ? 0 : 1)
                || parsed > 65535)
            {
                throw new InvalidOperationException(
                    fieldName
                    + (allowZero
                        ? " must be between 0 and 65535."
                        : " must be between 1 and 65535."));
            }

            return parsed;
        }

        private static string RequiredText(string value, string fieldName)
        {
            var result = (value ?? string.Empty).Trim();
            if (result.Length == 0)
            {
                throw new InvalidOperationException(fieldName + " is required.");
            }

            return result;
        }

        private LMCConnection RequireConnection()
        {
            if (connection == null || !connection.IsConnected)
            {
                throw new InvalidOperationException("Connect to the PLC first.");
            }

            return connection;
        }

        private LMCSingleAxis RequireAxis()
        {
            RequireConnection();
            if (axis == null)
            {
                throw new InvalidOperationException("Load an axis object first.");
            }

            return axis;
        }

        private LMCGroupAxis RequireGroup()
        {
            RequireConnection();
            if (group == null)
            {
                throw new InvalidOperationException("Load a group object first.");
            }

            return group;
        }

        private void EnsureGroupActiveVerified()
        {
            if (groupActiveVerified)
            {
                return;
            }

            throw new InvalidOperationException(
                "Run Group Power On, then Read Status until PowerOn=True "
                + "is verified.");
        }

        private void EnsureGroupReadyForMotion()
        {
            EnsureGroupActiveVerified();
            if (!groupIdentityConfigured)
            {
                throw new InvalidOperationException(
                    "Set Identity (Configure) before Move Linear.");
            }

            if (!groupProfileLocked)
            {
                throw new InvalidOperationException(
                    groupProfileLockVerificationPending
                        ? "Run 5 Read Status until Enabled/Locked Standby=True "
                            + "before Move Linear."
                        : "Enable (Lock Profile), then run 5 Read Status before "
                            + "Move Linear.");
            }
        }

        private void ResetIdentityHomeCheckState()
        {
            groupIdentityHomeCheckComplete = false;
            groupIdentityHomeCheckPassed = false;
            if (TextKinHomeStatus != null)
            {
                TextKinHomeStatus.Text = "Home Check: not checked.";
            }
        }

        private void ResetGroupPreparationState()
        {
            groupPowerVerificationPending = false;
            groupPowerOffVerificationPending = false;
            groupStatusRefreshRequired = false;
            groupActiveVerified = false;
            groupIdentityConfigured = false;
            ResetIdentityHomeCheckState();
            groupProfileLockVerificationPending = false;
            groupProfileLockDisabledSamples = 0;
            groupProfileLocked = false;
        }

        private void InvalidateGroupPreparationAfterStatusFailure()
        {
            groupStatusRefreshRequired = true;
            groupActiveVerified = false;
            groupProfileLockDisabledSamples = 0;
            groupProfileLocked = false;
            UpdateUiState();
        }

        private void ClearLoadedObjects()
        {
            axis = null;
            group = null;
            ResetGroupPreparationState();
            ClearDiagnosticsState();
            ClearReadOnlyApiState();
            if (TextAxisReference != null)
            {
                TextAxisReference.Text = "not loaded";
            }

            if (TextGroupReference != null)
            {
                TextGroupReference.Text = "not loaded";
            }
        }

        private void DisplayAxisStatus(LMCReadStatusResult result)
        {
            TextAxisResult.Text =
                "State=0x"
                + result.State.ToString("X8")
                + Environment.NewLine
                + "PowerOn="
                + result.IsPowerOn
                + ", Home/Referenced="
                + result.IsReferenced
                + ", Standstill="
                + result.IsStandstill
                + Environment.NewLine
                + "FunctionStatus=0x"
                + result.FunctionStatus.ToString("X4")
                + ", ErrorId="
                + result.ErrorId
                + Environment.NewLine
                + "AxisErrorId="
                + result.AxisErrorId
                + ", StatusWord=0x"
                + result.StatusWord.ToString("X4");
        }

        private void DisplayGroupStatus(LMCGroupReadStatusResult result)
        {
            TextGroupResult.Text =
                "State=0x"
                + result.State.ToString("X8")
                + Environment.NewLine
                + "PowerOn="
                + result.IsPowerOn
                + Environment.NewLine
                + "Disabled/Unlocked="
                + result.IsDisabled
                + ", Enabled/LockedStandby="
                + result.IsStandby
                + Environment.NewLine
                + "FunctionStatus=0x"
                + result.FunctionStatus.ToString("X4")
                + ", ErrorId="
                + result.ErrorId
                + Environment.NewLine
                + "GroupErrorId="
                + result.GroupErrorId;
        }

        private void DisplayGroupPosition(
            LMCGroupReadActualPositionResult result,
            PlcUnitOption unit)
        {
            var positions = result.PositionsRaw;
            var raw = positions
                .Select(
                    (position, index) =>
                        "["
                        + index
                        + "]="
                        + position)
                .ToArray();
            var engineering = unit.IsRaw
                ? new[] { "conversion disabled (None / raw DINT)" }
                : positions
                    .Take(4)
                    .Select(
                        (position, index) =>
                            "XYZU"[index]
                            + "="
                            + (position / (double)unit.Multiplier).ToString(
                                "0.########",
                                CultureInfo.InvariantCulture)
                            + " "
                            + unit.Symbol)
                    .ToArray();

            TextGroupResult.Text =
                "Coordinate="
                + result.CoordinateSystem
                + Environment.NewLine
                + "Engineering: "
                + string.Join(", ", engineering)
                + Environment.NewLine
                + "Raw DINT: "
                + string.Join(", ", raw)
                + Environment.NewLine
                + "FunctionStatus=0x"
                + result.FunctionStatus.ToString("X4")
                + ", ErrorId="
                + result.ErrorId;
        }

        private static string FormatEngineeringPosition(
            int positionRaw,
            PlcUnitOption unit)
        {
            if (unit.IsRaw)
            {
                return "PLC UNIT=None / raw DINT; engineering conversion disabled";
            }

            return "Engineering="
                + (positionRaw / (double)unit.Multiplier).ToString(
                    "0.########",
                    CultureInfo.InvariantCulture)
                + " "
                + unit.Symbol;
        }

        private static string FormatGroupPositionsRaw(int[] positions)
        {
            if (positions == null)
            {
                return "<null>";
            }

            const string labels = "XYZU";
            return string.Join(
                ", ",
                positions
                    .Take(4)
                    .Select(
                        (position, index) =>
                            labels[index]
                            + "="
                            + position));
        }

        private static bool IsGroupInPosition(
            LMCGroupReadStatusResult result)
        {
            return result != null && result.IsStandby;
        }

        private static void EnsureResponseSuccess(
            string operation,
            LMC_Response response)
        {
            if (response != null && response.IsFrameValid && response.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. "
                + (response == null
                    ? "No response."
                    : FormatResponse(response)));
        }

        private static void EnsureAdminResponseSuccess(
            string operation,
            LMCAdminResponse response)
        {
            if (response != null && response.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. "
                + FormatAdminResponse(response));
        }

        private static void EnsureAxisStatusSuccess(
            string operation,
            LMCReadStatusResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ", AxisErrorId="
                + (result == null ? 0 : result.AxisErrorId)
                + ".");
        }

        private static void EnsureAxisPositionSuccess(
            string operation,
            LMCReadActualPositionResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ".");
        }

        private static void EnsureGroupStatusSuccess(
            string operation,
            LMCGroupReadStatusResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ", GroupErrorId="
                + (result == null ? 0 : result.GroupErrorId)
                + ".");
        }

        private static void EnsureGroupPositionSuccess(
            string operation,
            LMCGroupReadActualPositionResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ".");
        }

        private static void EnsureGroupMembersSuccess(
            string operation,
            LMCGroupMembersInfoResult result)
        {
            if (result != null && result.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. ErrorId="
                + (result == null ? 0 : result.ErrorId)
                + ".");
        }

        private static string FormatResponse(LMC_Response response)
        {
            if (response == null)
            {
                return "Response=<null>";
            }

            return
                "FrameValid="
                + response.IsFrameValid
                + ", Success="
                + response.IsSuccess
                + ", Status="
                + response.Status
                + ", ErrorId="
                + response.ErrorId
                + ", Bytes="
                + (response.Raw == null ? 0 : response.Raw.Length);
        }

        private static string FormatAdminResponse(LMCAdminResponse response)
        {
            if (response == null)
            {
                return "AdminResponse=<null>";
            }

            return
                "Schema="
                + response.SchemaVersion
                + ", CommandStatus="
                + response.CommandStatus
                + ", ErrorId="
                + response.ErrorId
                + ", RequestId="
                + response.RequestId
                + ", Detail="
                + response.DetailCode
                + " ("
                + response.DetailCodeValue
                + "), Transport="
                + FormatResponse(response.TransportResponse);
        }

        private void UpdateUiState()
        {
            if (shutdownInProgress || ButtonConnect == null)
            {
                return;
            }

            var currentConnection = connection;
            var connected = currentConnection != null
                && currentConnection.IsConnected;
            var axisReady = connected && axis != null;
            var groupReady = connected && group != null;
            var idle = !operationRunning
                && !safetyCommandRunning
                && safetyMonitorCount == 0
                && !qualificationRunning;
            var safetySendAvailable = !safetyCommandRunning
                && !connectionTransitionRunning;
            var d5TicketUnresolved =
                HasUnresolvedD5SdoQualificationTicket;
            var liveCommandAllowed = idle
                && !motionMayBeActive
                && !d5TicketUnresolved;
            var groupPowerTransitionPending = groupPowerVerificationPending
                || groupPowerOffVerificationPending;
            var groupMotionCoordinateReady =
                ComboGroupCoordinate.SelectedItem is LMC_COORD_SYSTEM
                    groupCoordinate
                && groupCoordinate == LMC_COORD_SYSTEM.None;

            ButtonConnect.IsEnabled = idle
                && !motionMayBeActive
                && (!d5TicketUnresolved || !connected);
            ButtonCloseConnection.IsEnabled =
                idle
                && currentConnection != null
                && !motionMayBeActive
                && !d5TicketUnresolved
                && !groupPowerTransitionPending;
            TextRemoteIp.IsEnabled = idle && currentConnection == null;
            TextRemotePort.IsEnabled = idle && currentConnection == null;
            TextLocalIp.IsEnabled = idle && currentConnection == null;
            TextCallbackPort.IsEnabled = idle && currentConnection == null;

            TextAxisName.IsEnabled = idle && !motionMayBeActive;
            ButtonLookupAxis.IsEnabled = connected && idle && !motionMayBeActive;
            ButtonReadStatus.IsEnabled = axisReady && idle;
            ButtonReadPosition.IsEnabled = axisReady && idle;
            ButtonPowerOn.IsEnabled = axisReady && liveCommandAllowed;
            ButtonPowerOff.IsEnabled = axisReady
                && safetySendAvailable
                && (!motionMayBeActive
                    || IsTrackedMotionAxis(axis.AxisName));
            ButtonReset.IsEnabled = axisReady && liveCommandAllowed;
            ButtonStop.IsEnabled = axisReady
                && safetySendAvailable
                && (!motionMayBeActive
                    || IsTrackedMotionAxis(axis.AxisName));
            ButtonMoveAbsolute.IsEnabled = axisReady && liveCommandAllowed;
            ButtonMoveRelative.IsEnabled = axisReady && liveCommandAllowed;
            ButtonMoveVelocity.IsEnabled = axisReady && liveCommandAllowed;

            ComboAxisUnit.IsEnabled = idle && !motionMayBeActive;
            TextPosition.IsEnabled = idle && !motionMayBeActive;
            TextVelocity.IsEnabled = idle && !motionMayBeActive;
            TextAcceleration.IsEnabled = idle && !motionMayBeActive;
            TextDeceleration.IsEnabled = !operationRunning
                && !safetyCommandRunning;
            TextJerk.IsEnabled = !operationRunning
                && !safetyCommandRunning;
            ComboDirection.IsEnabled = idle && !motionMayBeActive;

            TextGroupName.IsEnabled = idle
                && !motionMayBeActive
                && !groupPowerTransitionPending;
            ButtonLookupGroup.IsEnabled = connected
                && idle
                && !motionMayBeActive
                && !groupPowerTransitionPending;
            ButtonGetMembers.IsEnabled = groupReady
                && idle
                && !motionMayBeActive
                && !groupPowerOffVerificationPending;
            ButtonGroupReadStatus.IsEnabled = groupReady && idle;
            ButtonGroupReadStatus.Content = groupPowerOffVerificationPending
                ? "7 Verify Power Off (Read Status)"
                : "2 / 5 Read Status (Power Ready / Lock Ready)";
            ButtonGroupReadPosition.IsEnabled = groupReady
                && idle
                && !groupPowerOffVerificationPending;
            ButtonGroupPowerOn.IsEnabled = groupReady
                && liveCommandAllowed
                && !groupActiveVerified
                && !groupPowerVerificationPending
                && !groupPowerOffVerificationPending
                && !groupStatusRefreshRequired
                && !groupProfileLockVerificationPending
                && !groupProfileLocked;
            ButtonGroupPowerOff.IsEnabled = groupReady
                && safetySendAvailable
                && !groupPowerOffVerificationPending
                && (!motionMayBeActive
                    || IsTrackedMotionAxis(group.GroupName));
            ButtonGroupEnable.IsEnabled = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && groupIdentityConfigured
                && !groupProfileLockVerificationPending
                && !groupProfileLocked;
            ButtonGroupDisable.IsEnabled = groupReady
                && idle
                && !motionMayBeActive
                && !d5TicketUnresolved
                && !groupPowerOffVerificationPending
                && groupProfileLocked;
            ButtonGroupReset.IsEnabled = groupReady
                && liveCommandAllowed
                && !groupPowerTransitionPending;
            ButtonGroupStop.IsEnabled = groupReady
                && safetySendAvailable
                && (!motionMayBeActive
                    || IsTrackedMotionAxis(group.GroupName));
            ButtonGroupMoveLinear.IsEnabled = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && groupIdentityConfigured
                && !groupProfileLockVerificationPending
                && groupProfileLocked
                && groupMotionCoordinateReady;
            ButtonGroupMoveLinearRelative.IsEnabled = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && groupIdentityConfigured
                && !groupProfileLockVerificationPending
                && groupProfileLocked
                && groupMotionCoordinateReady;
            ButtonCheckKinHome.IsEnabled = groupReady
                && idle
                && !groupPowerOffVerificationPending;
            ButtonSetKinTransform.IsEnabled = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && !groupProfileLockVerificationPending
                && !groupProfileLocked;

            ComboGroupUnit.IsEnabled = idle && !motionMayBeActive;
            TextGroupPositionX.IsEnabled = idle && !motionMayBeActive;
            TextGroupPositionY.IsEnabled = idle && !motionMayBeActive;
            TextGroupPositionZ.IsEnabled = idle && !motionMayBeActive;
            TextGroupPositionU.IsEnabled = idle && !motionMayBeActive;
            TextGroupVelocity.IsEnabled = idle && !motionMayBeActive;
            TextGroupAcceleration.IsEnabled = idle && !motionMayBeActive;
            TextGroupDeceleration.IsEnabled = !operationRunning
                && !safetyCommandRunning;
            TextGroupJerk.IsEnabled = !operationRunning
                && !safetyCommandRunning;
            ComboGroupCoordinate.IsEnabled = idle && !motionMayBeActive;
            ComboGroupTransition.IsEnabled = idle && !motionMayBeActive;
            ComboGroupBuffer.IsEnabled = idle && !motionMayBeActive;
            var identityInputAllowed = groupReady
                && liveCommandAllowed
                && !groupPowerOffVerificationPending
                && groupActiveVerified
                && !groupProfileLockVerificationPending
                && !groupProfileLocked;
            TextKinAxisX.IsEnabled = identityInputAllowed;
            TextKinAxisY.IsEnabled = identityInputAllowed;
            TextKinAxisZ.IsEnabled = identityInputAllowed;
            TextKinAxisU.IsEnabled = identityInputAllowed;

            TextGroupPreparationState.Text = GetGroupPreparationStateText(
                groupReady);

            TextConnectionState.Text = currentConnection == null
                ? LMCConnectionState.Disconnected.ToString()
                : currentConnection.State.ToString();
            TextCallbackState.Text = currentConnection == null
                ? "Stopped"
                : (currentConnection.IsCallbackListenerRunning
                    ? "Listening "
                        + currentConnection.CallbackLocalEndPoint
                        + ", rejected="
                        + currentConnection.RejectedCallbackCount
                    : "Stopped, rejected="
                        + currentConnection.RejectedCallbackCount);

            UpdateDiagnosticsUiState(connected, idle);
            UpdateReadOnlyApiUiState(connected, idle);
            UpdateQualificationUiState(connected, idle);

            var trackedGroup = motionMayBeActive
                && group != null
                && IsTrackedMotionAxis(group.GroupName);
            TextMotionWarning.Text = motionMayBeActive
                ? "SAFETY: "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + (trackedGroup
                        ? ". Use Group Stop and verify InPosition."
                        : ". Use Stop or PowerOff and verify standstill.")
                : d5TicketUnresolved
                    ? "SAFETY: a D5 ticket, submission outcome, or Write readback is unresolved. New motion/diagnostic mutation and Close are blocked. "
                        + GetD5SdoResolutionGuidance()
                    : "Stop, PowerOff, and Group Stop remain available while connected. Closing the connection does not stop motion.";
        }

        private string GetGroupPreparationStateText(bool groupReady)
        {
            if (!groupReady)
            {
                return "Preparation: load the group first.";
            }

            var powerState = groupStatusRefreshRequired
                ? "Read Status failed, Power/Lock state unknown"
                : (groupPowerOffVerificationPending
                    ? "Power Off accepted/start only, Power Off pending"
                    : (groupActiveVerified
                        ? "Power Ready/ACTIVE verified"
                        : (groupPowerVerificationPending
                            ? "Power On accepted/start only, Power Ready pending"
                            : "Power On required")));
            var identityState = groupIdentityConfigured
                ? "identity configured"
                : "identity not configured";
            var homeState = groupIdentityHomeCheckComplete
                ? (groupIdentityHomeCheckPassed
                    ? "identity axes referenced"
                    : "identity axis Home required")
                : "identity Home not checked";
            var profileState = groupProfileLocked
                ? "profile locked/standby verified"
                : (groupProfileLockVerificationPending
                    ? "profile lock accepted, Lock Ready pending"
                    : "profile unlocked");

            string nextStep;
            if (groupPowerOffVerificationPending)
            {
                nextStep = "Next: Read Status until PowerOn=False.";
            }
            else if (groupStatusRefreshRequired)
            {
                nextStep = "Next: Read Status to refresh the group state.";
            }
            else if (!groupActiveVerified)
            {
                nextStep = groupPowerVerificationPending
                    ? "Next: Read Status until PowerOn=True."
                    : "Next: Power On.";
            }
            else if (!groupIdentityConfigured)
            {
                nextStep = groupIdentityHomeCheckComplete
                    && !groupIdentityHomeCheckPassed
                    ? "Next: Home the failed axes, then Set Identity."
                    : "Next: Set Identity (automatic Home Check).";
            }
            else if (groupProfileLockVerificationPending)
            {
                nextStep =
                    "Next: 5 Read Status until Enabled/Locked Standby=True.";
            }
            else if (!groupProfileLocked)
            {
                nextStep = "Next: Enable (Lock Profile).";
            }
            else
            {
                nextStep = "Ready: Move Linear or Disable (Unlock Profile).";
            }

            return "Preparation: "
                + powerState
                + " | "
                + homeState
                + " | "
                + identityState
                + " | "
                + profileState
                + ". "
                + nextStep;
        }

        private void WriteLog(string message)
        {
            if (TextExecutionLog == null)
            {
                return;
            }

            TextExecutionLog.AppendText(
                "["
                + DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)
                + "] "
                + message
                + Environment.NewLine);
            TextExecutionLog.ScrollToEnd();
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (allowWindowClose)
            {
                base.OnClosing(e);
                return;
            }

            e.Cancel = true;
            base.OnClosing(e);

            if (shutdownInProgress)
            {
                return;
            }

            if (operationRunning
                || safetyCommandRunning
                || safetyMonitorCount > 0
                || qualificationRunning)
            {
                if (qualificationRunning)
                {
                    CancelQualification(
                        "Window close requested",
                        false);
                }

                WriteLog(
                    "Window close is blocked while an API operation, safety "
                    + "verification, or qualification cleanup is running. "
                    + "Wait for its timeout or completion, then close again.");
                return;
            }

            if (motionMayBeActive)
            {
                WriteLog(
                    "Window is closing while "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + ". No Stop command is sent automatically.");
            }

            if (HasUnresolvedD5SdoQualificationTicket)
            {
                WriteLog(
                    "Window close is blocked while a D5 ticket, submission outcome, or Write readback is unresolved. "
                    + GetD5SdoResolutionGuidance());
                return;
            }

            shutdownInProgress = true;
            var currentConnection = connection;
            connection = null;
            ClearLoadedObjects();

            if (currentConnection != null)
            {
                DetachConnection(currentConnection);
                try
                {
                    await currentConnection.CloseConnectionAsync(
                        CancellationToken.None);
                }
                catch (Exception closeError)
                {
                    WriteLog("Shutdown close warning: " + closeError.Message);
                }

                try
                {
                    currentConnection.Dispose();
                }
                catch (Exception disposeError)
                {
                    WriteLog("Shutdown dispose warning: " + disposeError.Message);
                }
            }

            allowWindowClose = true;
            Close();
        }

        private sealed class IdentityAxisHomeStatus
        {
            public IdentityAxisHomeStatus(
                string coordinateName,
                LMCSingleAxis selectedAxis,
                LMCReadStatusResult status)
            {
                CoordinateName = coordinateName;
                Axis = selectedAxis;
                Status = status;
            }

            public string CoordinateName { get; }
            public LMCSingleAxis Axis { get; }
            public LMCReadStatusResult Status { get; }
        }

        private sealed class IdentityHomeCheckResult
        {
            public IdentityHomeCheckResult(
                IdentityAxisHomeStatus axisX,
                IdentityAxisHomeStatus axisY,
                IdentityAxisHomeStatus axisZ,
                IdentityAxisHomeStatus axisU)
            {
                AxisX = axisX;
                AxisY = axisY;
                AxisZ = axisZ;
                AxisU = axisU;
                Axes = new[] { AxisX, AxisY, AxisZ, AxisU };
            }

            public IdentityAxisHomeStatus AxisX { get; }
            public IdentityAxisHomeStatus AxisY { get; }
            public IdentityAxisHomeStatus AxisZ { get; }
            public IdentityAxisHomeStatus AxisU { get; }
            public IdentityAxisHomeStatus[] Axes { get; }

            public int ReferencedCount
            {
                get { return Axes.Count(item => item.Status.IsReferenced); }
            }

            public bool AllReferenced
            {
                get { return ReferencedCount == Axes.Length; }
            }

            public string UnreferencedAxisSummary
            {
                get
                {
                    return string.Join(
                        ", ",
                        Axes
                            .Where(item => !item.Status.IsReferenced)
                            .Select(
                                item =>
                                    item.CoordinateName
                                    + "="
                                    + item.Axis.AxisName));
                }
            }
        }

        private sealed class MotionInput
        {
            public int PositionRaw { get; set; }
            public int VelocityRaw { get; set; }
            public int AccelerationRaw { get; set; }
            public int DecelerationRaw { get; set; }
            public int JerkRaw { get; set; }
            public LMC_DIRECTION Direction { get; set; }
        }

        private sealed class GroupMotionInput
        {
            public int[] PositionsRaw { get; set; }
            public int VelocityRaw { get; set; }
            public int AccelerationRaw { get; set; }
            public int DecelerationRaw { get; set; }
            public int JerkRaw { get; set; }
            public LMCGroupMotionOptions Options { get; set; }
        }

        private sealed class PlcUnitOption
        {
            public PlcUnitOption(
                string displayName,
                string symbol,
                int multiplier,
                bool isRaw)
            {
                DisplayName = displayName;
                Symbol = symbol;
                Multiplier = multiplier;
                IsRaw = isRaw;
            }

            public string DisplayName { get; }
            public string Symbol { get; }
            public int Multiplier { get; }
            public bool IsRaw { get; }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}

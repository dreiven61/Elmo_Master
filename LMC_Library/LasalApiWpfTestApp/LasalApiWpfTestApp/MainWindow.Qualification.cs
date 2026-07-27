using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const int QualificationStableSamples = 3;
        private const int QualificationPollMilliseconds = 50;
        private const int QualificationCleanupGateTimeoutMilliseconds = 15000;
        private const int MaximumQualificationDeltaMagnitudeRaw = 1000000;

        private readonly List<string> qualificationLogLines =
            new List<string>();
        private bool qualificationRunning;
        private CancellationTokenSource qualificationCancellation;
        private Stopwatch qualificationStopwatch;
        private string qualificationRunId;
        private string qualificationScenario;
        private int qualificationStep;
        private string qualificationExternalSafetyOperation;
        private bool qualificationExternalGroupSafety;
        private int qualificationProgress;
        private int qualificationSafetyGeneration;

        private void InitializeQualificationUi()
        {
            ComboQualificationGroupAxis.Items.Add("X");
            ComboQualificationGroupAxis.Items.Add("Y");
            ComboQualificationGroupAxis.Items.Add("Z");
            ComboQualificationGroupAxis.Items.Add("U");
            ComboQualificationGroupAxis.SelectedIndex = 0;

            SetQualificationProgress(0, "No qualification has run yet.");
        }

        private async void ButtonRunGroupEnableQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "GroupEnableAcceptedThenLocked",
                RunGroupEnableQualificationAsync);
        }

        private async void ButtonRunBufferedQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "GroupTrueBuffered",
                RunGroupBufferedQualificationAsync);
        }

        private async void ButtonRunStopFirstQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "GroupDeterministicStopFirst",
                RunGroupStopFirstQualificationAsync);
        }

        private void ButtonCancelQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            CancelQualification("Cancel button", false);
        }

        private void ButtonSaveQualificationLog_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (qualificationLogLines.Count == 0)
            {
                WriteLog("Qualification log is empty.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save qualification log",
                Filter = "Text file (*.txt)|*.txt|Qualification log (*.log)|*.log|All files (*.*)|*.*",
                FileName = DateTime.Now.ToString(
                        "yyyyMMdd_HHmmss",
                        CultureInfo.InvariantCulture)
                    + "_"
                    + SanitizeFileName(qualificationScenario)
                    + ".txt"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            File.WriteAllLines(
                dialog.FileName,
                qualificationLogLines,
                new UTF8Encoding(false));
            WriteLog("Qualification log saved: " + dialog.FileName);
            TextOperationState.Text = "Qualification log saved";
        }

        private async Task RunQualificationAsync(
            string scenario,
            Func<CancellationToken, Task> action)
        {
            if (qualificationRunning
                || operationRunning
                || safetyCommandRunning
                || safetyMonitorCount > 0)
            {
                WriteLog(
                    "A qualification, operation, or safety verification is already running.");
                return;
            }

            var isD5PendingCleanup = string.Equals(
                scenario,
                "D5SdoPendingCleanup",
                StringComparison.Ordinal);
            if (HasUnresolvedD5SdoQualificationTicket
                && !isD5PendingCleanup)
            {
                WriteLog(
                    scenario
                    + " qualification is blocked while a D5 ticket or submission outcome is unresolved. Use Resolve D5 Quarantine; Stop, PowerOff, and existing-resource cleanup remain available.");
                return;
            }

            var currentConnection = connection;
            if (currentConnection == null || !currentConnection.IsConnected)
            {
                WriteLog("Qualification requires an active PLC connection.");
                return;
            }

            if (motionMayBeActive)
            {
                WriteLog(
                    "Qualification is blocked because "
                    + motionOperation
                    + " may still be active on "
                    + motionAxisName
                    + ".");
                return;
            }

            qualificationRunning = true;
            qualificationCancellation = new CancellationTokenSource();
            qualificationStopwatch = Stopwatch.StartNew();
            qualificationRunId = Guid.NewGuid().ToString("N");
            qualificationScenario = scenario;
            qualificationStep = 0;
            qualificationExternalSafetyOperation = null;
            qualificationExternalGroupSafety = false;
            qualificationSafetyGeneration = safetyRequestGeneration;
            var preservedQualificationLogLineCount =
                qualificationLogLines.Count;
            if (!isD5PendingCleanup)
            {
                qualificationLogLines.Clear();
                preservedQualificationLogLineCount = 0;
            }
            SetQualificationProgress(0, scenario + " preflight");
            TextOperationState.Text = scenario + " running";
            UpdateUiState();

            try
            {
                WriteQualificationLog(
                    "event=BEGIN",
                    "endpoint=" + QualificationValue(
                        TextRemoteIp.Text
                        + ":"
                        + TextRemotePort.Text));
                if (preservedQualificationLogLineCount > 0)
                {
                    WriteQualificationLog(
                        "event=D5_LOG_CONTINUATION",
                        "preservedLines="
                            + preservedQualificationLogLineCount.ToString(
                                CultureInfo.InvariantCulture),
                        "reason=preserve_original_failure_and_uncertain_submission_evidence");
                }
                await action(qualificationCancellation.Token);
                qualificationCancellation.Token.ThrowIfCancellationRequested();
                SetQualificationProgress(100, scenario + " PASS");
                WriteQualificationLog("event=END", "verdict=PASS");
                TextOperationState.Text = scenario + " PASS";
            }
            catch (OperationCanceledException)
            {
                SetQualificationProgress(
                    qualificationProgress,
                    scenario + " ABORTED");
                WriteQualificationLog(
                    "event=END",
                    "verdict=ABORTED",
                    "reason=" + QualificationValue(
                        qualificationExternalSafetyOperation
                            ?? "user cancellation"));
                TextOperationState.Text = scenario + " aborted";
            }
            catch (NotSupportedException error)
            {
                SetQualificationProgress(
                    qualificationProgress,
                    scenario + " SKIP: " + error.Message);
                WriteQualificationLog(
                    "event=END",
                    "verdict=SKIP",
                    "reason=" + QualificationValue(error.Message));
                TextOperationState.Text = scenario + " skipped";
            }
            catch (Exception error)
            {
                SetQualificationProgress(
                    qualificationProgress,
                    scenario + " FAIL: " + error.Message);
                WriteQualificationLog(
                    "event=END",
                    "verdict=FAIL",
                    "errorType=" + error.GetType().Name,
                    "error=" + QualificationValue(error.Message));
                TextOperationState.Text = scenario + " failed";
            }
            finally
            {
                qualificationStopwatch.Stop();
                qualificationCancellation.Dispose();
                qualificationCancellation = null;
                qualificationRunning = false;
                qualificationExternalSafetyOperation = null;
                qualificationExternalGroupSafety = false;
                UpdateUiState();
            }
        }

        private async Task<T> SendQualificationCommandAsync<T>(
            string operation,
            CancellationToken cancellationToken,
            Func<Task<T>> send)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await commandSendGate.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureNoNewSafetyRequest(
                    qualificationSafetyGeneration,
                    operation);
                return await send();
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task SendQualificationCommandAsync(
            string operation,
            CancellationToken cancellationToken,
            Func<Task> send)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await commandSendGate.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureNoNewSafetyRequest(
                    qualificationSafetyGeneration,
                    operation);
                await send();
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task<T> SendQualificationCleanupCommandAsync<T>(
            string operation,
            Func<Task<T>> send)
        {
            await AcquireQualificationCleanupGateAsync(operation);

            try
            {
                return await send();
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task SendQualificationCleanupCommandAsync(
            string operation,
            Func<Task> send)
        {
            await AcquireQualificationCleanupGateAsync(operation);

            try
            {
                await send();
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task AcquireQualificationCleanupGateAsync(
            string operation)
        {
            var timeout = Stopwatch.StartNew();
            while (true)
            {
                while (safetyCommandRunning || safetyMonitorCount > 0)
                {
                    var safetyWaitRemaining =
                        QualificationCleanupGateTimeoutMilliseconds
                        - checked((int)Math.Min(
                            int.MaxValue,
                            timeout.ElapsedMilliseconds));
                    if (safetyWaitRemaining <= 0)
                    {
                        throw CreateQualificationCleanupGateTimeout(
                            operation);
                    }

                    await Task.Delay(Math.Min(25, safetyWaitRemaining));
                }

                var gateWaitRemaining =
                    QualificationCleanupGateTimeoutMilliseconds
                    - checked((int)Math.Min(
                        int.MaxValue,
                        timeout.ElapsedMilliseconds));
                if (gateWaitRemaining <= 0
                    || !await commandSendGate.WaitAsync(gateWaitRemaining))
                {
                    throw CreateQualificationCleanupGateTimeout(operation);
                }

                if (!safetyCommandRunning && safetyMonitorCount == 0)
                {
                    return;
                }

                commandSendGate.Release();
            }
        }

        private static TimeoutException CreateQualificationCleanupGateTimeout(
            string operation)
        {
            return new TimeoutException(
                "Qualification cleanup could not acquire the command gate "
                + "within "
                + QualificationCleanupGateTimeoutMilliseconds.ToString(
                    CultureInfo.InvariantCulture)
                + " ms while preserving safety-command priority. Operation="
                + operation);
        }

        private async Task RunGroupEnableQualificationAsync(
            CancellationToken cancellationToken)
        {
            var currentGroup = RequireGroup();
            EnsureGroupActiveVerified();
            if (!groupIdentityConfigured)
            {
                throw new InvalidOperationException(
                    "Set Identity (Configure) before running the Group Enable qualification.");
            }

            if (groupProfileLockVerificationPending)
            {
                throw new InvalidOperationException(
                    "A previous Group Enable acknowledgement is still pending status verification. Read Status until it resolves before retrying.");
            }

            SetQualificationProgress(10, "Reading initial group status");
            var initial = await ReadQualificationGroupStatusAsync(
                currentGroup,
                cancellationToken);
            EnsureGroupStatusSuccess("Group Enable preflight", initial);
            WriteQualificationLog(
                "event=STATUS",
                "cmd=0x2045",
                "state=0x" + initial.State.ToString("X8"),
                "powerOn=" + initial.IsPowerOn,
                "standby=" + initial.IsStandby,
                "disabled=" + initial.IsDisabled);

            if (!initial.IsPowerOn)
            {
                throw new InvalidOperationException(
                    "Group Enable qualification requires PowerOn=True.");
            }

            if (initial.IsStandby)
            {
                throw new InvalidOperationException(
                    "The group is already locked. Disable it before this qualification.");
            }

            var disabledStable = initial.IsDisabled ? 1 : 0;
            var disabledDeadline = DateTime.UtcNow.AddSeconds(1);
            while (disabledStable < QualificationStableSamples
                && DateTime.UtcNow < disabledDeadline)
            {
                await Task.Delay(
                    QualificationPollMilliseconds,
                    cancellationToken);
                var disabledStatus = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupStatusSuccess(
                    "Group Enable disabled-state preflight",
                    disabledStatus);
                if (disabledStatus.IsStandby)
                {
                    throw new InvalidOperationException(
                        "The group became locked during preflight. Disable it before this qualification.");
                }

                disabledStable = disabledStatus.IsPowerOn
                    && disabledStatus.IsDisabled
                    ? disabledStable + 1
                    : 0;
            }

            if (disabledStable < QualificationStableSamples)
            {
                throw new InvalidOperationException(
                    "Group Enable qualification requires three stable PowerOn + Disabled/Unlocked samples. The group remained transitional or unknown.");
            }

            WriteQualificationLog(
                "event=ASSERT",
                "name=StableDisabledBeforeEnable",
                "samples=" + QualificationStableSamples,
                "verdict=PASS");

            SetQualificationProgress(30, "Sending one Group Enable request");
            var acknowledgement = await SendQualificationCommandAsync(
                "Qualification Group Enable",
                cancellationToken,
                () => currentGroup.GroupEnableAsync(CancellationToken.None));
            EnsureResponseSuccess(
                "Qualification Group Enable",
                acknowledgement);
            groupProfileLockVerificationPending = true;
            groupProfileLockDisabledSamples = 0;
            groupProfileLocked = false;
            WriteQualificationLog(
                "event=ACK",
                "cmd=0x2047",
                "frameValid=" + acknowledgement.IsFrameValid,
                "errorId=" + acknowledgement.ErrorId,
                "verdict=PASS");

            SetQualificationProgress(55, "Polling 0x2045 until lock ready");
            var deadline = DateTime.UtcNow.AddSeconds(5);
            var stable = 0;
            LMCGroupReadStatusResult latest = null;
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                latest = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupStatusSuccess(
                    "Group Enable lock verification",
                    latest);
                WriteQualificationLog(
                    "event=POLL",
                    "cmd=0x2045",
                    "state=0x" + latest.State.ToString("X8"),
                    "standby=" + latest.IsStandby);

                if (latest.IsPowerOn && latest.IsStandby)
                {
                    stable++;
                    if (stable >= QualificationStableSamples)
                    {
                        groupProfileLockVerificationPending = false;
                        groupProfileLocked = true;
                        DisplayGroupStatus(latest);
                        SetQualificationProgress(
                            95,
                            "Group lock ready verified");
                        WriteQualificationLog(
                            "event=ASSERT",
                            "name=AcceptedThenLocked",
                            "expected=ACK0_then_3_stable_standby",
                            "actual=ACK0_and_3_stable_standby",
                            "verdict=PASS");
                        return;
                    }
                }
                else
                {
                    stable = 0;
                }

                await Task.Delay(
                    QualificationPollMilliseconds,
                    cancellationToken);
            }

            groupProfileLockVerificationPending = true;
            groupProfileLocked = false;
            throw new TimeoutException(
                "Group Enable was accepted, but stable locked Standby was not observed within 5 seconds. LastState="
                + (latest == null
                    ? "none"
                    : "0x" + latest.State.ToString("X8"))
                + ".");
        }

        private async Task RunGroupBufferedQualificationAsync(
            CancellationToken cancellationToken)
        {
            var currentConnection = RequireConnection();
            var currentGroup = RequireGroup();
            EnsureGroupReadyForMotion();
            var input = ReadGroupBufferedQualificationInput();
            var motionStarted = false;
            var returnedToStart = false;

            try
            {
                SetQualificationProgress(5, "Validating group readiness");
                var capabilities = await ReadQualificationAdminCapabilitiesAsync(
                    currentConnection,
                    cancellationToken);
                if (!capabilities.Supports(LMCAdminFeature.GroupLinearRelative)
                    || capabilities.GroupReference != currentGroup.GroupReference)
                {
                    throw new NotSupportedException(
                        "The connected PLC does not advertise GroupLinearRelative for the loaded group.");
                }

                if (!capabilities.Supports(LMCAdminFeature.AxisParameterRead)
                    || !capabilities.Supports(
                        LMCAxisParameterKey.SoftwareMinPosition)
                    || !capabilities.Supports(
                        LMCAxisParameterKey.SoftwareMaxPosition))
                {
                    throw new NotSupportedException(
                        "Buffered qualification requires advertised software min/max axis parameters for endpoint preflight.");
                }

                if (!capabilities.Supports(LMCAdminFeature.GroupParameterRead)
                    || !capabilities.Supports(
                        LMCGroupParameterSelection.PathVelocityLimit
                        | LMCGroupParameterSelection.PathAccelerationLimit))
                {
                    throw new NotSupportedException(
                        "Buffered qualification requires advertised group velocity and acceleration limits.");
                }

                var members = await SendQualificationCommandAsync(
                    "Buffered qualification group members",
                    cancellationToken,
                    () => currentGroup.GetGroupMembersInfoResultAsync(
                        CancellationToken.None));
                EnsureGroupMembersSuccess(
                    "Buffered qualification group members",
                    members);
                if (members.AxisCount < 4
                    || members.Members.Length <= input.AxisIndex)
                {
                    throw new InvalidOperationException(
                        "Buffered qualification requires the expected four group members.");
                }

                var selectedMember = members.Members[input.AxisIndex];
                var softwareMinimum = await SendQualificationCommandAsync(
                    "Buffered qualification software minimum",
                    cancellationToken,
                    () => currentConnection.Admin.ReadAxisParameterAsync(
                        selectedMember.AxisReference,
                        LMCAxisParameterKey.SoftwareMinPosition,
                        CancellationToken.None));
                var softwareMaximum = await SendQualificationCommandAsync(
                    "Buffered qualification software maximum",
                    cancellationToken,
                    () => currentConnection.Admin.ReadAxisParameterAsync(
                        selectedMember.AxisReference,
                        LMCAxisParameterKey.SoftwareMaxPosition,
                        CancellationToken.None));
                if (!softwareMinimum.Response.IsSuccess
                    || !softwareMaximum.Response.IsSuccess)
                {
                    throw new InvalidOperationException(
                        "Failed to read software endpoint limits for "
                        + selectedMember.AxisName
                        + ".");
                }

                var groupLimits = await SendQualificationCommandAsync(
                    "Buffered qualification group limits",
                    cancellationToken,
                    () => currentConnection.Admin.ReadGroupParametersAsync(
                        currentGroup,
                        LMCGroupParameterSelection.PathVelocityLimit
                            | LMCGroupParameterSelection.PathAccelerationLimit,
                        CancellationToken.None));
                if (!groupLimits.Response.IsSuccess
                    || input.VelocityRaw > groupLimits.PathVelocityLimit
                    || input.AccelerationRaw
                        > groupLimits.PathAccelerationLimit
                    || input.DecelerationRaw
                        > groupLimits.PathAccelerationLimit)
                {
                    throw new InvalidOperationException(
                        "Qualification dynamics exceed the advertised group limits. Velocity="
                        + input.VelocityRaw
                        + "/"
                        + groupLimits.PathVelocityLimit
                        + ", Acceleration="
                        + input.AccelerationRaw
                        + "/"
                        + groupLimits.PathAccelerationLimit
                        + ", Deceleration="
                        + input.DecelerationRaw
                        + "/"
                        + groupLimits.PathAccelerationLimit
                        + ".");
                }

                var ready = await WaitForQualificationGroupInPositionAsync(
                    currentGroup,
                    5000,
                    cancellationToken);
                WriteQualificationLog(
                    "event=ASSERT",
                    "name=PreflightInPosition",
                    "state=0x" + ready.State.ToString("X8"),
                    "verdict=PASS");

                var startResult = await ReadQualificationGroupPositionAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupPositionSuccess(
                    "Buffered qualification start position",
                    startResult);
                var start = startResult.PositionsRaw.Take(4).ToArray();
                var expectedAfterA = (int[])start.Clone();
                var expectedFinal = (int[])start.Clone();
                expectedAfterA[input.AxisIndex] = CheckedAdd(
                    start[input.AxisIndex],
                    input.DeltaA,
                    "Start + Delta A");
                expectedFinal[input.AxisIndex] = CheckedAdd(
                    expectedAfterA[input.AxisIndex],
                    input.DeltaB,
                    "Start + Delta A + Delta B");
                var lowerBound = (long)softwareMinimum.Value
                    + input.ToleranceRaw;
                var upperBound = (long)softwareMaximum.Value
                    - input.ToleranceRaw;
                if (lowerBound > upperBound
                    || start[input.AxisIndex] < lowerBound
                    || start[input.AxisIndex] > upperBound
                    || expectedAfterA[input.AxisIndex] < lowerBound
                    || expectedAfterA[input.AxisIndex] > upperBound
                    || expectedFinal[input.AxisIndex] < lowerBound
                    || expectedFinal[input.AxisIndex] > upperBound)
                {
                    throw new InvalidOperationException(
                        "Buffered qualification endpoint exceeds the selected axis software-limit margin. Axis="
                        + selectedMember.AxisName
                        + ", Min="
                        + softwareMinimum.Value
                        + ", Max="
                        + softwareMaximum.Value
                        + ", Tolerance="
                        + input.ToleranceRaw
                        + ", Start="
                        + start[input.AxisIndex]
                        + ", AfterA="
                        + expectedAfterA[input.AxisIndex]
                        + ", Final="
                        + expectedFinal[input.AxisIndex]
                        + ".");
                }

                WriteQualificationLog(
                    "event=START_POSITION",
                    "cmd=0x2051",
                    "axis=" + input.AxisName,
                    "axisRef=" + selectedMember.AxisReference,
                    "softwareMin=" + softwareMinimum.Value,
                    "softwareMax=" + softwareMaximum.Value,
                    "start=" + start[input.AxisIndex],
                    "afterA=" + expectedAfterA[input.AxisIndex],
                    "final=" + expectedFinal[input.AxisIndex]);

                var options = new LMCGroupMotionOptions
                {
                    CoordinateSystem = LMC_COORD_SYSTEM.None,
                    TransitionMode = LMC_GROUP_TRANSITION_MODE.ExactStop,
                    BufferMode = LMC_BUFFER_MODE.Buffered,
                    Execute = true
                };
                var deltaA = CreateQualificationVector(
                    input.AxisIndex,
                    input.DeltaA);
                var deltaB = CreateQualificationVector(
                    input.AxisIndex,
                    input.DeltaB);
                var trackingGeneration = 0;

                SetQualificationProgress(20, "Sending Buffered command A");
                var responseA = await SendQualificationCommandAsync(
                    "Qualification Buffered A",
                    cancellationToken,
                    () =>
                    {
                        trackingGeneration = MarkMotionUncertain(
                            currentGroup.GroupName,
                            "Qualification Group Buffered A/B");
                        motionStarted = true;
                        return currentGroup.MoveLinearRelativeExAsync(
                            deltaA,
                            input.VelocityRaw,
                            input.AccelerationRaw,
                            input.DecelerationRaw,
                            input.JerkRaw,
                            options,
                            capabilities,
                            CancellationToken.None);
                    });
                ClearMotionOnConfirmedRejection(
                    currentGroup.GroupName,
                    "Qualification Buffered A",
                    responseA);
                motionStarted = IsTrackedMotionAxis(currentGroup.GroupName);
                EnsureAdminResponseSuccess(
                    "Qualification Buffered A",
                    responseA);
                WriteQualificationLog(
                    "event=ACK",
                    "cmd=0x7D22",
                    "sequence=A",
                    "requestId=" + responseA.RequestId,
                    "buffer=2",
                    "deltaRaw=" + input.DeltaA,
                    "verdict=PASS");

                SetQualificationProgress(
                    35,
                    "Waiting for A motion before queuing B");
                await WaitForQualificationGroupMotionStartedAsync(
                    currentGroup,
                    3000,
                    cancellationToken);
                RecordMotionObserved(currentGroup.GroupName);

                var beforeB = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupStatusSuccess(
                    "Buffered B pre-transmission status",
                    beforeB);
                if (IsGroupInPosition(beforeB))
                {
                    throw new InvalidOperationException(
                        "Command A reached InPosition before command B transmission. Increase Delta A or reduce velocity so a true queue window exists.");
                }

                WriteQualificationLog(
                    "event=STATUS",
                    "stage=immediately_before_B",
                    "cmd=0x2045",
                    "state=0x" + beforeB.State.ToString("X8"),
                    "inPosition=false");

                SetQualificationProgress(45, "Sending Buffered command B");
                var responseB = await SendQualificationCommandAsync(
                    "Qualification Buffered B",
                    cancellationToken,
                    () => currentGroup.MoveLinearRelativeExAsync(
                        deltaB,
                        input.VelocityRaw,
                        input.AccelerationRaw,
                        input.DecelerationRaw,
                        input.JerkRaw,
                        options,
                        capabilities,
                        CancellationToken.None));
                // A may still be moving when B is rejected. Do not clear the
                // motion warning from B's acknowledgement.
                EnsureAdminResponseSuccess(
                    "Qualification Buffered B",
                    responseB);
                WriteQualificationLog(
                    "event=ACK",
                    "cmd=0x7D22",
                    "sequence=B",
                    "requestId=" + responseB.RequestId,
                    "buffer=2",
                    "deltaRaw=" + input.DeltaB,
                    "verdict=PASS");

                var afterB = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupStatusSuccess(
                    "Buffered B post-acknowledgement status",
                    afterB);
                if (IsGroupInPosition(afterB))
                {
                    throw new InvalidOperationException(
                        "The group was already InPosition immediately after command B acknowledgement; queued execution was not discriminating.");
                }

                WriteQualificationLog(
                    "event=ASSERT",
                    "name=BDispatchedInsideMotionWindow",
                    "beforeBState=0x" + beforeB.State.ToString("X8"),
                    "afterBAckState=0x" + afterB.State.ToString("X8"),
                    "wireProof=REQUIRES_PCAP",
                    "verdict=PASS");

                var totalDistance = CreateQualificationVector(
                    input.AxisIndex,
                    checked(input.DeltaA + input.DeltaB));
                var completionTimeout =
                    CalculateGroupMotionMonitorTimeoutMilliseconds(
                        totalDistance.Select(value => (long)value).ToArray(),
                        input.VelocityRaw,
                        input.AccelerationRaw,
                        input.DecelerationRaw);
                SetQualificationProgress(60, "Verifying accumulated endpoint");
                var finalResult = await WaitForQualificationGroupPositionAsync(
                    currentGroup,
                    expectedFinal,
                    input.ToleranceRaw,
                    completionTimeout,
                    cancellationToken);
                ClearMotionWarning(
                    "Buffered A/B final position and stable InPosition verified",
                    trackingGeneration);
                motionStarted = false;
                DisplayGroupPosition(finalResult, ReadGroupUnitSelection());
                WriteQualificationLog(
                    "event=ASSERT",
                    "name=FinalPosition",
                    "expected=" + expectedFinal[input.AxisIndex],
                    "actual=" + finalResult.PositionsRaw[input.AxisIndex],
                    "tolerance=" + input.ToleranceRaw,
                    "verdict=PASS");

                SetQualificationProgress(78, "Returning to captured start position");
                await ReturnQualificationGroupToStartAsync(
                    currentGroup,
                    start,
                    input,
                    capabilities,
                    cancellationToken);
                returnedToStart = true;
                SetQualificationProgress(95, "Buffered qualification cleanup PASS");
            }
            catch (Exception primaryError)
            {
                if (motionStarted || IsTrackedMotionAxis(currentGroup.GroupName))
                {
                    try
                    {
                        await CleanupQualificationGroupMotionAsync(
                            currentGroup,
                            input.DecelerationRaw,
                            input.JerkRaw);
                    }
                    catch (Exception cleanupError)
                    {
                        throw CreateQualificationUnsafeCleanupException(
                            primaryError,
                            cleanupError);
                    }
                }

                throw;
            }
            finally
            {
                if (!returnedToStart
                    && !motionStarted
                    && !IsTrackedMotionAxis(currentGroup.GroupName))
                {
                    WriteQualificationLog(
                        "event=CLEANUP",
                        "returnToStart=false",
                        "reason=motion_not_started_or_already_safe");
                }
            }
        }

        private async Task RunGroupStopFirstQualificationAsync(
            CancellationToken cancellationToken)
        {
            var currentConnection = RequireConnection();
            var currentGroup = RequireGroup();
            EnsureGroupReadyForMotion();
            var input = ReadGroupBufferedQualificationInput();
            var capabilities = await ReadQualificationAdminCapabilitiesAsync(
                currentConnection,
                cancellationToken);
            if (!capabilities.Supports(LMCAdminFeature.GroupLinearRelative)
                || capabilities.GroupReference != currentGroup.GroupReference)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise GroupLinearRelative for the loaded group.");
            }

            if (!capabilities.Supports(LMCAdminFeature.GroupParameterRead)
                || !capabilities.Supports(
                    LMCGroupParameterSelection.PathVelocityLimit
                    | LMCGroupParameterSelection.PathAccelerationLimit))
            {
                throw new NotSupportedException(
                    "Stop-first qualification requires advertised group velocity and acceleration limits.");
            }

            var groupLimits = await SendQualificationCommandAsync(
                "Stop-first qualification group limits",
                cancellationToken,
                () => currentConnection.Admin.ReadGroupParametersAsync(
                    currentGroup,
                    LMCGroupParameterSelection.PathVelocityLimit
                        | LMCGroupParameterSelection.PathAccelerationLimit,
                    CancellationToken.None));
            if (!groupLimits.Response.IsSuccess
                || input.VelocityRaw > groupLimits.PathVelocityLimit
                || input.AccelerationRaw > groupLimits.PathAccelerationLimit
                || input.DecelerationRaw > groupLimits.PathAccelerationLimit)
            {
                throw new InvalidOperationException(
                    "Stop-first dynamics exceed the advertised group limits.");
            }

            await WaitForQualificationGroupInPositionAsync(
                currentGroup,
                5000,
                cancellationToken);
            SetQualificationProgress(20, "Holding command gate");

            var gateOwned = false;
            Action releaseGate = () =>
            {
                if (gateOwned)
                {
                    commandSendGate.Release();
                    gateOwned = false;
                }
            };

            cancellationToken.ThrowIfCancellationRequested();
            await commandSendGate.WaitAsync(cancellationToken);
            gateOwned = true;
            var expectedGeneration = safetyRequestGeneration;
            var zeroDelta = new int[4];
            var options = new LMCGroupMotionOptions
            {
                CoordinateSystem = LMC_COORD_SYSTEM.None,
                TransitionMode = LMC_GROUP_TRANSITION_MODE.ExactStop,
                BufferMode = LMC_BUFFER_MODE.Buffered,
                Execute = true
            };
            var moveDelegateInvocationCount = 0;
            Task<LMCAdminResponse> moveTask;
            Task<GroupStopStableStandbyResult> stopTask;
            try
            {
                moveTask = SendLiveCommandAsync(
                    expectedGeneration,
                    "Qualification queued zero-delta Move",
                    () =>
                    {
                        moveDelegateInvocationCount++;
                        return currentGroup.MoveLinearRelativeExAsync(
                            zeroDelta,
                            input.VelocityRaw,
                            input.AccelerationRaw,
                            input.DecelerationRaw,
                            input.JerkRaw,
                            options,
                            capabilities,
                            CancellationToken.None);
                    });
                stopTask = GroupStopQualificationOrchestrator
                    .StopAndVerifyStableStandbyAsync(
                        currentGroup,
                        input.DecelerationRaw,
                        input.JerkRaw,
                        stop => DispatchQualificationGroupStopAsync(
                            "Qualification deterministic Group Stop",
                            stop,
                            true),
                        status => SendQualificationCleanupCommandAsync(
                            "Qualification deterministic Group Stop status",
                            status),
                        5000,
                        QualificationPollMilliseconds,
                        milliseconds => Task.Delay(milliseconds));
            }
            catch
            {
                releaseGate();
                throw;
            }

            await GroupStopQualificationOrchestrator.RunWithFallbackAsync(
                async () =>
                {
                SetQualificationProgress(45, "Releasing gate after Stop request");
                releaseGate();

                Exception moveError = null;
                try
                {
                    await moveTask;
                }
                catch (Exception error)
                {
                    moveError = error;
                }

                var stopResult = await stopTask;

                var expectedMessage =
                    "was cancelled before transmission because Stop or Power Off was requested";
                if (!(moveError is InvalidOperationException)
                    || moveError.Message.IndexOf(
                        expectedMessage,
                        StringComparison.Ordinal) < 0
                    || moveDelegateInvocationCount != 0)
                {
                    throw new InvalidOperationException(
                        "Stop-first local assertion failed. MoveError="
                        + (moveError == null
                            ? "none"
                            : moveError.GetType().Name + ": " + moveError.Message)
                        + ", DelegateInvocations="
                        + moveDelegateInvocationCount
                        + ".");
                }

                WriteQualificationLog(
                    "event=ASSERT",
                    "name=StopFirstPreemption",
                    "moveDelegateInvocations=" + moveDelegateInvocationCount,
                    "moveWireExpected=0",
                    "stopWireExpected=1",
                    "verdict=PASS");
                SetQualificationProgress(
                    70,
                    "Verifying stable Group IsStandby");
                DisplayGroupStatus(stopResult.Status);
                WriteQualificationLog(
                    "event=ASSERT",
                    "name=StableIsStandby",
                    "stableSamples="
                        + GroupStopQualificationOrchestrator
                            .RequiredStableStandbySamples,
                    "statusReads=" + stopResult.StatusReadCount,
                    "verdict=PASS");
                SetQualificationProgress(95, "Stop-first qualification PASS");
                },
                releaseGate,
                () => CleanupQualificationGroupMotionAsync(
                    currentGroup,
                    input.DecelerationRaw,
                    input.JerkRaw),
                CreateQualificationUnsafeCleanupException);
        }

        private async Task<LMC_Response>
            DispatchQualificationGroupStopAsync(
                string operation,
                Func<Task<LMC_Response>> nonCancelableStop,
                bool logAcknowledgement)
        {
            LMC_Response response = null;
            Exception sendError = null;
            var sent = await RunSafetyCommandAsync(
                operation,
                async () =>
                {
                    try
                    {
                        response = await nonCancelableStop();
                        EnsureResponseSuccess(operation, response);
                        if (logAcknowledgement)
                        {
                            WriteQualificationLog(
                                "event=ACK",
                                "cmd=0x2085",
                                "verdict=PASS");
                        }
                    }
                    catch (Exception error)
                    {
                        sendError = error;
                        throw;
                    }
                });
            if (sent)
            {
                return response;
            }

            if (sendError != null)
            {
                ExceptionDispatchInfo.Capture(sendError).Throw();
            }

            throw new InvalidOperationException(
                operation + " could not be sent.");
        }

        private async Task ReturnQualificationGroupToStartAsync(
            LMCGroupAxis currentGroup,
            int[] start,
            QualificationGroupInput input,
            LMCAdminCapabilities capabilities,
            CancellationToken cancellationToken)
        {
            var options = new LMCGroupMotionOptions
            {
                CoordinateSystem = LMC_COORD_SYSTEM.None,
                TransitionMode = LMC_GROUP_TRANSITION_MODE.ExactStop,
                BufferMode = LMC_BUFFER_MODE.Aborting,
                Execute = true
            };
            var trackingGeneration = 0;
            try
            {
                var returnDistance = CreateQualificationVector(
                    input.AxisIndex,
                    checked(-(input.DeltaA + input.DeltaB)));
                var response = await SendQualificationCommandAsync(
                    "Qualification return to start",
                    cancellationToken,
                    () =>
                    {
                        trackingGeneration = MarkMotionUncertain(
                            currentGroup.GroupName,
                            "Qualification return to start");
                        return currentGroup.MoveLinearRelativeExAsync(
                            returnDistance,
                            input.VelocityRaw,
                            input.AccelerationRaw,
                            input.DecelerationRaw,
                            input.JerkRaw,
                            options,
                            capabilities,
                            CancellationToken.None);
                    });
                ClearMotionOnConfirmedRejection(
                    currentGroup.GroupName,
                    "Qualification return to start",
                    response);
                EnsureAdminResponseSuccess(
                    "Qualification return to start",
                    response);
                WriteQualificationLog(
                    "event=ACK",
                    "cmd=0x7D22",
                    "buffer=1",
                    "selectedAxisOnly=true",
                    "purpose=return_to_start",
                    "verdict=PASS");

                var distances = returnDistance
                    .Select(value => (long)value)
                    .ToArray();
                var timeout = CalculateGroupMotionMonitorTimeoutMilliseconds(
                    distances,
                    input.VelocityRaw,
                    input.AccelerationRaw,
                    input.DecelerationRaw);
                var final = await WaitForQualificationGroupPositionAsync(
                    currentGroup,
                    start,
                    input.ToleranceRaw,
                    timeout,
                    cancellationToken);
                ClearMotionWarning(
                    "Qualification return position and stable InPosition verified",
                    trackingGeneration);
                WriteQualificationLog(
                    "event=ASSERT",
                    "name=ReturnedToStart",
                    "actual=" + final.PositionsRaw[input.AxisIndex],
                    "expected=" + start[input.AxisIndex],
                    "tolerance=" + input.ToleranceRaw,
                    "verdict=PASS");
            }
            catch (Exception primaryError)
            {
                if (IsTrackedMotionAxis(currentGroup.GroupName))
                {
                    try
                    {
                        await CleanupQualificationGroupMotionAsync(
                            currentGroup,
                            input.DecelerationRaw,
                            input.JerkRaw);
                    }
                    catch (Exception cleanupError)
                    {
                        throw CreateQualificationUnsafeCleanupException(
                            primaryError,
                            cleanupError);
                    }
                }

                throw;
            }
        }

        private async Task CleanupQualificationGroupMotionAsync(
            LMCGroupAxis currentGroup,
            int decelerationRaw,
            int jerkRaw)
        {
            if (qualificationExternalGroupSafety)
            {
                if (await VerifyExternalGroupSafetyAsync(currentGroup))
                {
                    return;
                }

                WriteQualificationLog(
                    "event=CLEANUP",
                    "action=external_safety_fallback_GroupStop",
                    "verdict=START");
            }

            WriteQualificationLog(
                "event=CLEANUP",
                "action=GroupStop_then_stable_IsStandby",
                "verdict=START");
            try
            {
                var result = await GroupStopQualificationOrchestrator
                    .StopAndVerifyStableStandbyAsync(
                        currentGroup,
                        decelerationRaw,
                        jerkRaw,
                        stop => DispatchQualificationGroupStopAsync(
                            "Qualification cleanup Group Stop",
                            stop,
                            false),
                        status => SendQualificationCleanupCommandAsync(
                            "Qualification cleanup Group Stop status",
                            status),
                        5000,
                        QualificationPollMilliseconds,
                        milliseconds => Task.Delay(milliseconds));
                DisplayGroupStatus(result.Status);
                ClearMotionWarning(
                    "Qualification cleanup Group Stop and stable IsStandby verified");
                WriteQualificationLog(
                    "event=CLEANUP",
                    "action=GroupStop_then_stable_IsStandby",
                    "statusReads=" + result.StatusReadCount,
                    "verdict=PASS");
            }
            catch (Exception cleanupError)
            {
                WriteQualificationLog(
                    "event=CLEANUP",
                    "action=GroupStop_then_stable_IsStandby",
                    "verdict=FAIL",
                    "error=" + QualificationValue(cleanupError.Message));
                throw new InvalidOperationException(
                    "Qualification cleanup Group Stop and stable IsStandby were not verified.",
                    cleanupError);
            }
        }

        private static InvalidOperationException
            CreateQualificationUnsafeCleanupException(
                Exception primaryError,
                Exception cleanupError)
        {
            return new InvalidOperationException(
                "Qualification failed and cleanup did not verify a safe state. Primary="
                + primaryError.GetType().Name
                + ": "
                + primaryError.Message
                + "; Cleanup="
                + cleanupError.GetType().Name
                + ": "
                + cleanupError.Message,
                new AggregateException(primaryError, cleanupError));
        }

        private async Task<bool> VerifyExternalGroupSafetyAsync(
            LMCGroupAxis currentGroup)
        {
            var operation = qualificationExternalSafetyOperation
                ?? "external group safety";
            WriteQualificationLog(
                "event=CLEANUP",
                "action=verify_" + QualificationValue(operation),
                "verdict=START");

            var dispatchDeadline = DateTime.UtcNow.AddSeconds(8);
            while ((safetyCommandRunning || safetyMonitorCount > 0)
                && DateTime.UtcNow < dispatchDeadline)
            {
                await Task.Delay(25);
            }

            if (safetyCommandRunning || safetyMonitorCount > 0)
            {
                WriteQualificationLog(
                    "event=CLEANUP",
                    "action=verify_" + QualificationValue(operation),
                    "verdict=FAIL",
                    "error=external_safety_did_not_finish_within_8s");
                return false;
            }

            try
            {
                var deadline = DateTime.UtcNow.AddSeconds(5);
                var stablePowerOff = 0;
                var stableInPosition = 0;
                while (DateTime.UtcNow < deadline)
                {
                    var status = await SendQualificationCleanupCommandAsync(
                        "External Group safety verification",
                        () => currentGroup.GroupReadStatusResultAsync(
                            CancellationToken.None));
                    EnsureGroupStatusSuccess(
                        "External Group safety verification",
                        status);
                    stablePowerOff = !status.IsPowerOn
                        ? stablePowerOff + 1
                        : 0;
                    stableInPosition = IsGroupInPosition(status)
                        ? stableInPosition + 1
                        : 0;
                    if (stablePowerOff >= QualificationStableSamples)
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
                        DisplayGroupStatus(status);
                        ClearMotionWarning(
                            "External Group safety verified by three stable PowerOn=False samples");
                        WriteQualificationLog(
                            "event=CLEANUP",
                            "action=verify_external_GroupSafety",
                            "safeState=POWER_OFF",
                            "samples=" + QualificationStableSamples,
                            "verdict=PASS");
                        return true;
                    }

                    if (stableInPosition >= QualificationStableSamples)
                    {
                        DisplayGroupStatus(status);
                        ClearMotionWarning(
                            "External Group safety verified by stable InPosition");
                        WriteQualificationLog(
                            "event=CLEANUP",
                            "action=verify_external_GroupSafety",
                            "safeState=IN_POSITION",
                            "samples=" + QualificationStableSamples,
                            "verdict=PASS");
                        return true;
                    }

                    await Task.Delay(QualificationPollMilliseconds);
                }
            }
            catch (Exception error)
            {
                WriteQualificationLog(
                    "event=CLEANUP",
                    "action=verify_" + QualificationValue(operation),
                    "verdict=FAIL",
                    "error=" + QualificationValue(error.Message));
            }

            return false;
        }

        private async Task<LMCGroupReadStatusResult>
            WaitForQualificationGroupInPositionAsync(
                LMCGroupAxis currentGroup,
                int timeoutMilliseconds,
                CancellationToken cancellationToken,
                bool cleanupGate = false)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stable = 0;
            LMCGroupReadStatusResult latest = null;
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                latest = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken,
                    cleanupGate);
                EnsureGroupStatusSuccess(
                    "Qualification Group InPosition verification",
                    latest);
                if (IsGroupInPosition(latest))
                {
                    stable++;
                    if (stable >= QualificationStableSamples)
                    {
                        return latest;
                    }
                }
                else
                {
                    stable = 0;
                }

                await Task.Delay(
                    QualificationPollMilliseconds,
                    cancellationToken);
            }

            throw new TimeoutException(
                "Stable Group InPosition was not observed within "
                + timeoutMilliseconds
                + " ms. LastState="
                + (latest == null
                    ? "none"
                    : "0x" + latest.State.ToString("X8"))
                + ".");
        }

        private async Task WaitForQualificationGroupMotionStartedAsync(
            LMCGroupAxis currentGroup,
            int timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var status = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupStatusSuccess(
                    "Buffered A motion-start verification",
                    status);
                if (!IsGroupInPosition(status))
                {
                    WriteQualificationLog(
                        "event=ASSERT",
                        "name=MotionObservedBeforeB",
                        "state=0x" + status.State.ToString("X8"),
                        "verdict=PASS");
                    return;
                }

                await Task.Delay(
                    QualificationPollMilliseconds,
                    cancellationToken);
            }

            throw new TimeoutException(
                "Buffered command A completed or never left InPosition before B could be queued. Increase Delta A or reduce velocity.");
        }

        private async Task<LMCGroupReadActualPositionResult>
            WaitForQualificationGroupPositionAsync(
                LMCGroupAxis currentGroup,
                int[] expected,
                int tolerance,
                int timeoutMilliseconds,
                CancellationToken cancellationToken)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            var stable = 0;
            LMCGroupReadActualPositionResult latestPosition = null;
            LMCGroupReadStatusResult latestStatus = null;
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                latestStatus = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupStatusSuccess(
                    "Qualification Group completion status",
                    latestStatus);
                latestPosition = await ReadQualificationGroupPositionAsync(
                    currentGroup,
                    cancellationToken);
                EnsureGroupPositionSuccess(
                    "Qualification Group completion position",
                    latestPosition);

                if (!IsGroupInPosition(latestStatus))
                {
                    RecordMotionObserved(currentGroup.GroupName);
                }

                if (IsGroupInPosition(latestStatus)
                    && QualificationPositionMatches(
                        latestPosition.PositionsRaw,
                        expected,
                        tolerance))
                {
                    stable++;
                    if (stable >= QualificationStableSamples)
                    {
                        return latestPosition;
                    }
                }
                else
                {
                    stable = 0;
                }

                await Task.Delay(
                    QualificationPollMilliseconds,
                    cancellationToken);
            }

            throw new TimeoutException(
                "Group did not reach the expected position with stable InPosition within "
                + timeoutMilliseconds
                + " ms. Expected="
                + FormatGroupPositionsRaw(expected)
                + ", Actual="
                + (latestPosition == null
                    ? "none"
                    : FormatGroupPositionsRaw(latestPosition.PositionsRaw))
                + ", LastState="
                + (latestStatus == null
                    ? "none"
                    : "0x" + latestStatus.State.ToString("X8"))
                + ".");
        }

        private async Task<LMCGroupReadStatusResult>
            ReadQualificationGroupStatusAsync(
                LMCGroupAxis currentGroup,
                CancellationToken cancellationToken,
                bool cleanupGate = false)
        {
            if (cleanupGate)
            {
                return await SendQualificationCleanupCommandAsync(
                    "Qualification Group cleanup status",
                    () => currentGroup.GroupReadStatusResultAsync(
                        CancellationToken.None));
            }

            return await SendQualificationCommandAsync(
                "Qualification Group status",
                cancellationToken,
                () => currentGroup.GroupReadStatusResultAsync(
                    CancellationToken.None));
        }

        private async Task<LMCGroupReadActualPositionResult>
            ReadQualificationGroupPositionAsync(
                LMCGroupAxis currentGroup,
                CancellationToken cancellationToken)
        {
            return await SendQualificationCommandAsync(
                "Qualification Group position",
                cancellationToken,
                () => currentGroup.GroupReadActualPositionAsync(
                    LMC_COORD_SYSTEM.None,
                    CancellationToken.None));
        }

        private async Task<LMCAdminCapabilities>
            ReadQualificationAdminCapabilitiesAsync(
                LMCConnection currentConnection,
                CancellationToken cancellationToken)
        {
            return await SendQualificationCommandAsync(
                "Qualification Admin capabilities",
                cancellationToken,
                () => currentConnection.Admin.GetCapabilitiesAsync(
                    CancellationToken.None));
        }

        private QualificationGroupInput ReadGroupBufferedQualificationInput()
        {
            if (ComboQualificationGroupAxis.SelectedIndex < 0
                || ComboQualificationGroupAxis.SelectedIndex > 3)
            {
                throw new InvalidOperationException(
                    "Select qualification axis X, Y, Z, or U.");
            }

            var deltaA = ParseQualificationInt32(
                TextQualificationDeltaA.Text,
                "Delta A raw");
            var deltaB = ParseQualificationInt32(
                TextQualificationDeltaB.Text,
                "Delta B raw");
            if (deltaA == 0 || deltaB == 0)
            {
                throw new InvalidOperationException(
                    "Delta A and Delta B must both be non-zero.");
            }

            if (Math.Sign(deltaA) != Math.Sign(deltaB))
            {
                throw new InvalidOperationException(
                    "The first Buffered qualification requires Delta A and Delta B in the same direction.");
            }

            if (Math.Abs((long)deltaA)
                    > MaximumQualificationDeltaMagnitudeRaw
                || Math.Abs((long)deltaB)
                    > MaximumQualificationDeltaMagnitudeRaw)
            {
                throw new InvalidOperationException(
                    "Delta A and Delta B are limited to +/-"
                    + MaximumQualificationDeltaMagnitudeRaw
                    + " raw DINT per qualification command.");
            }

            var velocity = ParseQualificationPositiveInt32(
                TextQualificationVelocity.Text,
                "Velocity raw");
            var acceleration = ParseQualificationPositiveInt32(
                TextQualificationAcceleration.Text,
                "Acceleration raw");
            var deceleration = ParseQualificationPositiveInt32(
                TextQualificationDeceleration.Text,
                "Deceleration raw");
            var jerk = ParseQualificationNonNegativeInt32(
                TextQualificationJerk.Text,
                "Jerk raw");
            if (jerk != 0)
            {
                throw new InvalidOperationException(
                    "The first live qualification slice requires Jerk raw = 0.");
            }
            var tolerance = ParseQualificationNonNegativeInt32(
                TextQualificationTolerance.Text,
                "Tolerance raw");

            return new QualificationGroupInput
            {
                AxisIndex = ComboQualificationGroupAxis.SelectedIndex,
                AxisName = ComboQualificationGroupAxis.SelectedItem.ToString(),
                DeltaA = deltaA,
                DeltaB = deltaB,
                VelocityRaw = velocity,
                AccelerationRaw = acceleration,
                DecelerationRaw = deceleration,
                JerkRaw = jerk,
                ToleranceRaw = tolerance
            };
        }

        private void UpdateQualificationUiState(bool connected, bool idle)
        {
            var groupReady = connected && group != null;
            var canRunGroup = groupReady
                && idle
                && !motionMayBeActive
                && !HasUnresolvedD5SdoQualificationTicket
                && !groupPowerOffVerificationPending;
            var motionReady = canRunGroup
                && groupActiveVerified
                && groupIdentityConfigured
                && groupProfileLocked
                && !groupProfileLockVerificationPending;
            var transportQualificationReady = canRunGroup
                && !groupPowerVerificationPending
                && !groupPowerOffVerificationPending
                && !groupProfileLockVerificationPending
                && !groupStatusRefreshRequired;

            ButtonRunGroupEnableQualification.IsEnabled = canRunGroup
                && groupActiveVerified
                && groupIdentityConfigured
                && !groupProfileLockVerificationPending
                && !groupProfileLocked;
            ButtonRunBufferedQualification.IsEnabled = motionReady;
            ButtonRunStopFirstQualification.IsEnabled = motionReady;
            ButtonRunTransportQualification.IsEnabled =
                transportQualificationReady;
            ButtonCancelQualification.IsEnabled = qualificationRunning;
            ButtonSaveQualificationLog.IsEnabled =
                qualificationLogLines.Count > 0;
            ButtonSaveTransportQualificationCsv.IsEnabled =
                !qualificationRunning
                && (transportQualificationSummary != null
                    || transportQualificationSamples.Count > 0);

            var hasBulkResource = bulkConfiguration != null
                && !bulkConfiguration.IsReleased;
            var hasRecorderConfiguration = recorderConfiguration != null
                && !recorderConfiguration.IsReleased;
            var hasRecorderIdentity = recorderIdentity != null
                && !recorderIdentity.IsRecorderReleased;
            var diagnosticsReady = connected
                && idle
                && !HasUnresolvedD5SdoQualificationTicket
                && diagnosticCapabilities != null
                && diagnosticCatalog != null
                && !hasBulkResource
                && !hasRecorderConfiguration
                && !hasRecorderIdentity;
            var bulkEntries = diagnosticCatalog == null
                ? Array.Empty<LMCSignalCatalogEntry>()
                : diagnosticCatalog.Entries
                    .Where(
                        entry =>
                            (entry.AccessFlags
                                & LMCSignalAccessFlags.BulkReadable)
                            == LMCSignalAccessFlags.BulkReadable)
                    .ToArray();
            var bulkReady = diagnosticsReady
                && diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.BulkSnapshot)
                && diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.SignalCatalog)
                && bulkEntries.Length == 24
                && diagnosticCapabilities.MaxBulkSignals >= 24;
            ButtonRunBulkSnapshotSoakQualification.IsEnabled = bulkReady;
            ButtonRunBulkLifecycleQualification.IsEnabled = bulkReady;
            ButtonRunBulkPartialQualification.IsEnabled = bulkReady
                && groupReady;
            ButtonResumeBulkPartialQualification.IsEnabled =
                qualificationRunning
                && bulkPartialCheckpoint != null;
            ButtonResumeBulkPartialQualification.ToolTip =
                bulkPartialCheckpointName == null
                    ? "No external Bulk checkpoint is waiting."
                    : "Waiting checkpoint: " + bulkPartialCheckpointName;
            ButtonCancelBulkQualification.IsEnabled = qualificationRunning;
            ButtonSaveBulkQualificationLog.IsEnabled =
                qualificationLogLines.Count > 0;
            TextQualificationBulkIterations.IsEnabled = diagnosticsReady;
            TextQualificationBulkIntervalMs.IsEnabled = diagnosticsReady;

            var recordableCount = diagnosticCatalog == null
                ? 0
                : diagnosticCatalog.Entries.Count(
                    entry =>
                        (entry.AccessFlags & LMCSignalAccessFlags.Recordable)
                        == LMCSignalAccessFlags.Recordable);
            var recorderSingleReady = diagnosticsReady
                && diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.RecorderSingleBank)
                && diagnosticCapabilities.MaxRecorderChannels >= 4
                && diagnosticCapabilities.MaxRecorderSamples >= 1000
                && diagnosticCapabilities.RecorderBytesPerBank >= 16000
                && diagnosticCapabilities.MaxChunkDataBytes >= 16
                && recordableCount >= 4;
            var recorderTriggerReady = recorderSingleReady
                && diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.RecorderTrigger);
            ButtonRunRecorderSingleQualification.IsEnabled =
                recorderSingleReady;
            ButtonRunRecorderRingQualification.IsEnabled =
                recorderTriggerReady;
            ButtonRunRecorderSoakQualification.IsEnabled =
                recorderTriggerReady;
            ButtonRunRecorderReconnectExactQualification.IsEnabled =
                recorderTriggerReady;
            ButtonRunRecorderReconnectDiscoveryQualification.IsEnabled =
                recorderTriggerReady
                && diagnosticCapabilities.RecorderBufferCount == 1
                && !diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.RecorderDoubleBank);
            ButtonCancelRecorderQualification.IsEnabled =
                qualificationRunning;
            ButtonSaveRecorderQualificationLog.IsEnabled =
                qualificationLogLines.Count > 0;
            TextQualificationRecorderIterations.IsEnabled = diagnosticsReady;
            TextQualificationRecorderCapability.Text =
                diagnosticCapabilities == null
                    ? "Refresh Capabilities and load the PI Catalog."
                    : "Single="
                        + diagnosticCapabilities.Supports(
                            LMCDiagnosticCapability.RecorderSingleBank)
                        + ", Trigger="
                        + diagnosticCapabilities.Supports(
                            LMCDiagnosticCapability.RecorderTrigger)
                        + ", Double="
                        + (diagnosticCapabilities.Supports(
                                LMCDiagnosticCapability.RecorderDoubleBank)
                            ? "advertised"
                            : "SKIP: capability absent")
                        + ", Recordable="
                        + recordableCount
                        + ", ReconnectExact="
                        + recorderTriggerReady
                        + ", ReconnectDiscovery="
                        + (recorderTriggerReady
                            && diagnosticCapabilities.RecorderBufferCount == 1
                            && !diagnosticCapabilities.Supports(
                                LMCDiagnosticCapability.RecorderDoubleBank));

            var manualD5OperationPending = diagnosticOperationTicket != null
                && (diagnosticOperationStatus == null
                    || !diagnosticOperationStatus.IsTerminal);
            var d5QualificationRunning = qualificationRunning
                && string.Equals(
                    qualificationScenario,
                    "D5SdoAbortRecovery",
                    StringComparison.Ordinal);
            var d5QualificationReady = connected
                && idle
                && !motionMayBeActive
                && !manualD5OperationPending
                && !HasUnresolvedD5SdoQualificationTicket;
            TextD5SdoAbortSlaveReference.IsEnabled = d5QualificationReady;
            TextD5SdoAbortIndex.IsEnabled = d5QualificationReady;
            TextD5SdoAbortSubIndex.IsEnabled = d5QualificationReady;
            ComboD5SdoAbortValueType.IsEnabled = d5QualificationReady;
            TextD5SdoAbortDataLength.IsEnabled = d5QualificationReady;
            TextD5SdoAbortTimeoutCycles.IsEnabled = d5QualificationReady;
            ButtonRunD5SdoAbortQualification.IsEnabled =
                d5QualificationReady;
            ButtonCancelD5SdoQualification.IsEnabled =
                d5QualificationRunning
                || (connected
                    && idle
                    && !motionMayBeActive
                    && HasUnresolvedD5SdoQualificationTicket);
            ButtonCancelD5SdoQualification.Content =
                d5QualificationRunning
                    ? "Cancel Runner (not PLC Stop)"
                    : HasUnresolvedD5SdoQualificationTicket
                        ? "Resolve D5 Quarantine"
                        : "Cancel Runner (not PLC Stop)";
            ButtonSaveD5SdoQualificationLog.IsEnabled =
                qualificationLogLines.Count > 0;

            var inputEnabled = connected
                && idle
                && !HasUnresolvedD5SdoQualificationTicket;
            ComboQualificationGroupAxis.IsEnabled = inputEnabled;
            TextQualificationDeltaA.IsEnabled = inputEnabled;
            TextQualificationDeltaB.IsEnabled = inputEnabled;
            TextQualificationVelocity.IsEnabled = inputEnabled;
            TextQualificationAcceleration.IsEnabled = inputEnabled;
            TextQualificationDeceleration.IsEnabled = inputEnabled;
            TextQualificationJerk.IsEnabled = inputEnabled;
            TextQualificationTolerance.IsEnabled = inputEnabled;
            TextQualificationTransportWarmup.IsEnabled =
                transportQualificationReady;
            TextQualificationTransportIterations.IsEnabled =
                transportQualificationReady;
        }

        private void CancelQualificationForExternalSafety(
            string operation,
            bool establishesGroupSafety)
        {
            CancelQualification(operation, establishesGroupSafety);
        }

        private void CancelQualification(
            string reason,
            bool externalGroupSafety)
        {
            var cancellation = qualificationCancellation;
            if (!qualificationRunning || cancellation == null)
            {
                return;
            }

            if (externalGroupSafety)
            {
                qualificationExternalSafetyOperation = reason;
                qualificationExternalGroupSafety = true;
            }
            else if (!qualificationExternalGroupSafety)
            {
                qualificationExternalSafetyOperation = reason;
            }
            WriteQualificationLog(
                "event=CANCEL_REQUEST",
                "reason=" + QualificationValue(reason),
                "externalGroupSafety=" + externalGroupSafety);
            cancellation.Cancel();
            SetQualificationProgress(
                qualificationProgress,
                "Cancel requested; cleanup pending");
            UpdateUiState();
        }

        private void SetQualificationProgress(int progress, string summary)
        {
            qualificationProgress = Math.Max(0, Math.Min(100, progress));
            ProgressQualification.Value = qualificationProgress;
            ProgressBulkQualification.Value = qualificationProgress;
            ProgressRecorderQualification.Value = qualificationProgress;
            TextQualificationProgress.Text = summary;
            TextBulkQualificationProgress.Text = summary;
            TextRecorderQualificationProgress.Text = summary;
            RefreshQualificationSummary();
        }

        private void WriteQualificationLog(params string[] fields)
        {
            qualificationStep++;
            var elapsed = qualificationStopwatch == null
                ? 0L
                : qualificationStopwatch.ElapsedMilliseconds;
            var line = "QTEST|utc="
                + DateTime.UtcNow.ToString(
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    CultureInfo.InvariantCulture)
                + "|elapsedMs="
                + elapsed.ToString(CultureInfo.InvariantCulture)
                + "|run="
                + (qualificationRunId ?? "none")
                + "|scenario="
                + QualificationValue(qualificationScenario ?? "none")
                + "|step="
                + qualificationStep.ToString(CultureInfo.InvariantCulture);
            if (fields != null && fields.Length > 0)
            {
                line += "|" + string.Join("|", fields);
            }

            qualificationLogLines.Add(line);
            WriteLog(line);
            RefreshQualificationSummary();
        }

        private void RefreshQualificationSummary()
        {
            if (TextQualificationSummary == null)
            {
                return;
            }

            var start = Math.Max(0, qualificationLogLines.Count - 10);
            TextQualificationSummary.Text = qualificationLogLines.Count == 0
                ? "Structured QTEST results will appear here."
                : string.Join(
                    Environment.NewLine,
                    qualificationLogLines.Skip(start));
            TextBulkQualificationSummary.Text = TextQualificationSummary.Text;
            TextRecorderQualificationSummary.Text =
                TextQualificationSummary.Text;
        }

        private static int ParseQualificationInt32(
            string text,
            string fieldName)
        {
            int value;
            if (!int.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value))
            {
                throw new InvalidOperationException(
                    fieldName + " must be a signed decimal Int32.");
            }

            return value;
        }

        private static int ParseQualificationPositiveInt32(
            string text,
            string fieldName)
        {
            var value = ParseQualificationInt32(text, fieldName);
            if (value <= 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be greater than zero.");
            }

            return value;
        }

        private static int ParseQualificationNonNegativeInt32(
            string text,
            string fieldName)
        {
            var value = ParseQualificationInt32(text, fieldName);
            if (value < 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be zero or greater.");
            }

            return value;
        }

        private static int CheckedAdd(int left, int right, string fieldName)
        {
            var value = (long)left + right;
            if (value < int.MinValue || value > int.MaxValue)
            {
                throw new OverflowException(fieldName + " exceeds Int32.");
            }

            return (int)value;
        }

        private static int[] CreateQualificationVector(
            int axisIndex,
            int value)
        {
            var vector = new int[4];
            vector[axisIndex] = value;
            return vector;
        }

        private static bool QualificationPositionMatches(
            int[] actual,
            int[] expected,
            int tolerance)
        {
            if (actual == null || expected == null || actual.Length < 4
                || expected.Length < 4)
            {
                return false;
            }

            for (var index = 0; index < 4; index++)
            {
                if (Math.Abs((long)actual[index] - expected[index]) > tolerance)
                {
                    return false;
                }
            }

            return true;
        }

        private static string QualificationValue(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "/")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string SanitizeFileName(string value)
        {
            var source = string.IsNullOrWhiteSpace(value)
                ? "qualification"
                : value;
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                source = source.Replace(invalid, '_');
            }

            return source;
        }

        private sealed class QualificationGroupInput
        {
            public int AxisIndex { get; set; }
            public string AxisName { get; set; }
            public int DeltaA { get; set; }
            public int DeltaB { get; set; }
            public int VelocityRaw { get; set; }
            public int AccelerationRaw { get; set; }
            public int DecelerationRaw { get; set; }
            public int JerkRaw { get; set; }
            public int ToleranceRaw { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const int D5SdoQualificationPollMilliseconds = 25;
        private const int D5SdoQualificationSafetyTimeoutMilliseconds = 5000;
        private const int D5SdoQualificationCleanupTimeoutMilliseconds = 15000;
        private const int D5SdoQualificationMinimumTerminalWaitMilliseconds =
            5000;
        private const int D5SdoQualificationMaximumTerminalWaitMilliseconds =
            120000;

        private LMCOperationTicket d5SdoQualificationActiveTicket;
        private LMCOperationStatus d5SdoQualificationActiveStatus;
        private DateTime? d5SdoQualificationActiveDeadlineUtc;
        private LMCConnection d5SdoQualificationActiveConnection;
        private ushort d5SdoQualificationActiveSlaveReference;
        private uint d5SdoQualificationActiveTimeoutCycles;
        private readonly List<D5SdoQualificationOrphanEvidence>
            d5SdoQualificationOrphanedTickets =
                new List<D5SdoQualificationOrphanEvidence>();
        private string d5ExternalTrackingRunId;
        private string d5ExternalTrackingScenario;
        private int d5ExternalTrackingStep;
        private Stopwatch d5ExternalTrackingStopwatch;

        private bool HasUnresolvedD5SdoQualificationTicket
        {
            get
            {
                return d5SdoQualificationActiveTicket != null
                    || d5SdoQualificationOrphanedTickets.Count != 0;
            }
        }

        private async void ButtonRunD5SdoAbortQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            D5SdoQualificationInput input;
            try
            {
                input = ReadD5SdoQualificationInput();
            }
            catch (Exception error)
            {
                TextD5SdoQualificationProgress.Text =
                    "Not started: " + error.Message;
                TextOperationState.Text =
                    "D5 SDO Abort qualification validation failed";
                WriteLog(
                    "D5 SDO Abort qualification not started: "
                    + error.Message);
                return;
            }

            try
            {
                await RunQualificationAsync(
                    "D5SdoAbortRecovery",
                    cancellationToken =>
                        RunD5SdoAbortRecoveryQualificationAsync(
                            input,
                            cancellationToken));
            }
            finally
            {
                UpdateUiState();
                RefreshD5SdoQualificationOutput();
            }
        }

        private async void ButtonCancelD5SdoQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!qualificationRunning
                && HasUnresolvedD5SdoQualificationTicket)
            {
                await RunQualificationAsync(
                    "D5SdoPendingCleanup",
                    async cancellationToken =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        SetD5SdoQualificationProgress(
                            1,
                            "Resolving preserved D5 ticket/submission quarantine");
                        await ResolvePreservedD5SdoQualificationAsync(
                            cancellationToken);
                        SetD5SdoQualificationProgress(
                            100,
                            "Preserved D5 quarantine resolution proof completed");
                    });
                UpdateUiState();
                RefreshD5SdoQualificationOutput();
                return;
            }

            if (!qualificationRunning
                || !string.Equals(
                    qualificationScenario,
                    "D5SdoAbortRecovery",
                    StringComparison.Ordinal))
            {
                return;
            }

            CancelQualification(
                "D5 SDO runner cancellation (no PLC Stop command)",
                false);
        }

        private async Task RunD5SdoAbortRecoveryQualificationAsync(
            D5SdoQualificationInput input,
            CancellationToken cancellationToken)
        {
            if (d5SdoQualificationActiveTicket != null)
            {
                SetD5SdoQualificationProgress(
                    1,
                    "Resolving the previous D5 ticket before starting a new run");
                await CleanupPendingD5SdoQualificationAsync();
            }

            if (d5SdoQualificationOrphanedTickets.Count != 0)
            {
                throw new InvalidOperationException(
                    "A D5 ticket or uncertain submission remains quarantined. Use Resolve D5 Quarantine before starting a new qualification.");
            }

            d5SdoQualificationActiveStatus = null;
            d5SdoQualificationActiveDeadlineUtc = null;

            Exception primaryError = null;
            try
            {
                var currentConnection = RequireConnection();
                var diagnostics = currentConnection.Diagnostics;

                EnsureNoPendingManualD5Operation();
                SetD5SdoQualificationProgress(
                    4,
                    "Verifying selected physical axis is powered off and stationary");
                await VerifyD5SdoQualificationSafeAxisAsync(
                    currentConnection,
                    input.SlaveReference,
                    input.AxisObjectName,
                    cancellationToken);

                SetD5SdoQualificationProgress(
                    12,
                    "Refreshing and comparing D5 capabilities");
                var firstCapabilities =
                    await ReadD5SdoQualificationCapabilitiesAsync(
                        diagnostics,
                        cancellationToken,
                        "capability sample 1");
                await Task.Delay(
                    D5SdoQualificationPollMilliseconds,
                    cancellationToken);
                var capabilities =
                    await ReadD5SdoQualificationCapabilitiesAsync(
                        diagnostics,
                        cancellationToken,
                        "capability sample 2");
                RequireStableD5SdoQualificationCapabilities(
                    firstCapabilities,
                    capabilities,
                    "preflight");
                diagnosticCapabilities = capabilities;
                TextDiagnosticsCapabilities.Text =
                    FormatCapabilities(capabilities);

                var terminalWaitMilliseconds =
                    GetD5SdoQualificationTerminalWaitMilliseconds(
                        input.TimeoutCycles,
                        capabilities.BaseCycleTimeUs);
                WriteD5SdoQualificationLog(
                    "event=D5_PREFLIGHT",
                    "wireMutation=false",
                    "slave=" + input.SlaveReference.ToString(
                        CultureInfo.InvariantCulture),
                    "axis=" + QualificationValue(input.AxisObjectName),
                    "powerOn=false",
                    "standstill=true",
                    "bootId=0x" + capabilities.DiagnosticsBootId.ToString("X8"),
                    "mapRevision=0x" + capabilities.MapRevision.ToString("X8"),
                    "capabilities=SDORead+SDOReadGeneralInline",
                    "terminalWaitMs=" + terminalWaitMilliseconds.ToString(
                        CultureInfo.InvariantCulture),
                    "verdict=PASS");

                SetD5SdoQualificationProgress(
                    23,
                    "Reading known-valid 0x6061:0 baseline");
                var recoveryRequest = LMCSdoRequest.CreateRead(
                    input.SlaveReference,
                    0x6061,
                    0,
                    LMCSignalValueType.Int8,
                    1,
                    input.TimeoutCycles);
                var baselineTicket = await SubmitD5SdoQualificationAsync(
                    currentConnection,
                    diagnostics,
                    recoveryRequest,
                    capabilities.DiagnosticsBootId,
                    cancellationToken,
                     "baseline",
                     "0x6061",
                     0,
                     terminalWaitMilliseconds);
                var baselineStatus =
                    await WaitForD5SdoQualificationTerminalAsync(
                        diagnostics,
                        baselineTicket,
                        terminalWaitMilliseconds,
                        cancellationToken,
                        "baseline");
                var baselineData = baselineStatus.ResultData;
                if (baselineData.Length != 1)
                {
                    throw new InvalidOperationException(
                        "The known-valid 0x6061:0 baseline did not return exactly one byte.");
                }

                var expectedRecoveryValue = unchecked((sbyte)baselineData[0]);
                D5SdoQualificationAnalysis.ValidateKnownValidInt8Recovery(
                    baselineTicket,
                    baselineStatus,
                    capabilities.DiagnosticsBootId,
                    expectedRecoveryValue);
                WriteD5SdoQualificationLog(
                    "event=D5_BASELINE",
                    "ticket=" + baselineTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture),
                    "object=0x6061",
                    "subIndex=0",
                    "valueType=Int8",
                    "dataLength=1",
                    "value=" + expectedRecoveryValue.ToString(
                        CultureInfo.InvariantCulture),
                    "verdict=PASS");

                SetD5SdoQualificationProgress(
                    42,
                    "Submitting read-only abort candidate");
                var abortTicket = await SubmitD5SdoQualificationAsync(
                    currentConnection,
                    diagnostics,
                    input.AbortRequest,
                    capabilities.DiagnosticsBootId,
                    cancellationToken,
                     "abort",
                     "0x" + input.ObjectIndex.ToString("X4"),
                     input.SubIndex,
                     terminalWaitMilliseconds);
                var abortStatus =
                    await WaitForD5SdoQualificationTerminalAsync(
                        diagnostics,
                        abortTicket,
                        terminalWaitMilliseconds,
                        cancellationToken,
                        "abort");
                WriteD5SdoQualificationTerminalLog(
                    "abort",
                    abortTicket,
                    abortStatus);

                SetD5SdoQualificationProgress(
                    63,
                    "Rechecking BootId and MapRevision before recovery");
                var recoveryCapabilities =
                    await ReadD5SdoQualificationCapabilitiesAsync(
                        diagnostics,
                        cancellationToken,
                        "pre-recovery capability");
                RequireStableD5SdoQualificationCapabilities(
                    capabilities,
                    recoveryCapabilities,
                    "abort-to-recovery");

                SetD5SdoQualificationProgress(
                    70,
                    "Submitting new known-valid 0x6061:0 recovery ticket");
                var recoveryTicket = await SubmitD5SdoQualificationAsync(
                    currentConnection,
                    diagnostics,
                    recoveryRequest,
                    capabilities.DiagnosticsBootId,
                    cancellationToken,
                     "recovery",
                     "0x6061",
                     0,
                     terminalWaitMilliseconds);
                var recoveryStatus =
                    await WaitForD5SdoQualificationTerminalAsync(
                        diagnostics,
                        recoveryTicket,
                        terminalWaitMilliseconds,
                        cancellationToken,
                        "recovery");
                WriteD5SdoQualificationTerminalLog(
                    "recovery",
                    recoveryTicket,
                    recoveryStatus);

                var finalCapabilities =
                    await ReadD5SdoQualificationCapabilitiesAsync(
                        diagnostics,
                        cancellationToken,
                        "final capability");
                RequireStableD5SdoQualificationCapabilities(
                    capabilities,
                    finalCapabilities,
                    "full run");
                diagnosticCapabilities = finalCapabilities;
                TextDiagnosticsCapabilities.Text =
                    FormatCapabilities(finalCapabilities);

                SetD5SdoQualificationProgress(
                    92,
                    "Validating exact abort and recovery contracts");
                var result = D5SdoQualificationAnalysis
                    .ValidateAbortThenRecovery(
                        abortTicket,
                        abortStatus,
                        recoveryTicket,
                        recoveryStatus,
                        capabilities.DiagnosticsBootId,
                        expectedRecoveryValue);
                WriteD5SdoQualificationLog(
                    "event=D5_ASSERT",
                    "name=AbortThenSameBootRecovery",
                    "abortTicket=" + result.AbortTicketId.ToString(
                        CultureInfo.InvariantCulture),
                    "recoveryTicket=" + result.RecoveryTicketId.ToString(
                        CultureInfo.InvariantCulture),
                    "bootId=0x" + result.DiagnosticsBootId.ToString("X8"),
                    "abortCode=0x" + result.AbortCode.ToString("X8"),
                    "recoveredValue=" + result.RecoveredValue.ToString(
                        CultureInfo.InvariantCulture),
                    "wireMutation=false",
                    "verdict=PASS");
                SetD5SdoQualificationProgress(
                    98,
                    "D5 SDO Abort -> Recovery contract PASS");
            }
            catch (Exception error)
            {
                primaryError = error;
            }

            Exception cleanupError = null;
            try
            {
                await CleanupPendingD5SdoQualificationAsync();
            }
            catch (Exception error)
            {
                cleanupError = error;
            }

            if (primaryError != null)
            {
                if (cleanupError != null)
                {
                    throw new InvalidOperationException(
                        "D5 SDO qualification failed and the pending ticket cleanup also failed. Primary="
                        + primaryError.GetType().Name
                        + ": "
                        + primaryError.Message
                        + "; Cleanup="
                        + cleanupError.GetType().Name
                        + ": "
                        + cleanupError.Message,
                        new AggregateException(primaryError, cleanupError));
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
            }

            if (cleanupError != null)
            {
                throw new InvalidOperationException(
                    "D5 SDO qualification left a pending ticket that could not be resolved.",
                    cleanupError);
            }
        }

        private D5SdoQualificationInput ReadD5SdoQualificationInput()
        {
            var slaveReference = ParseUInt16Wire(
                TextD5SdoAbortSlaveReference.Text,
                "D5 abort slave reference",
                false);
            if (slaveReference < 1 || slaveReference > 4)
            {
                throw new InvalidOperationException(
                    "D5 abort slave reference must be between 1 and 4.");
            }

            var objectIndex = ParseUInt16Wire(
                TextD5SdoAbortIndex.Text,
                "D5 abort object index",
                false);
            var subIndex = ParseByteWire(
                TextD5SdoAbortSubIndex.Text,
                "D5 abort sub-index");
            var dataLength = ParseUInt16Wire(
                TextD5SdoAbortDataLength.Text,
                "D5 abort data length",
                false);
            var timeoutCycles = ParseUInt32(
                TextD5SdoAbortTimeoutCycles.Text,
                "D5 abort timeout cycles");

            var selectedType =
                ComboD5SdoAbortValueType.SelectedItem as ComboBoxItem;
            LMCSignalValueType valueType;
            if (selectedType == null
                || selectedType.Content == null
                || !Enum.TryParse(
                    selectedType.Content.ToString(),
                    true,
                    out valueType))
            {
                throw new InvalidOperationException(
                    "Select a valid D5 abort value type.");
            }

            if (dataLength != GetSdoReadDataLength(valueType))
            {
                throw new InvalidOperationException(
                    "D5 abort data length must match the selected type: 8-bit=1, 16-bit=2, 32-bit=4.");
            }

            if (timeoutCycles < 1 || timeoutCycles > 60000)
            {
                throw new InvalidOperationException(
                    "D5 abort timeout must be between 1 and 60000 cycles.");
            }

            var request = LMCSdoRequest.CreateRead(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles);
            return new D5SdoQualificationInput(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles,
                request);
        }

        private void EnsureNoPendingManualD5Operation()
        {
            if (diagnosticOperationTicket != null
                && (diagnosticOperationStatus == null
                    || !diagnosticOperationStatus.IsTerminal))
            {
                throw new InvalidOperationException(
                    "The manual SDO/PI operation ticket is still pending. Refresh it to terminal or cancel it before running D5 qualification.");
            }
        }

        private async Task VerifyD5SdoQualificationSafeAxisAsync(
            LMCConnection currentConnection,
            ushort slaveReference,
            string axisObjectName,
            CancellationToken cancellationToken)
        {
            var selectedAxis = await SendQualificationCommandAsync(
                "D5 SDO qualification axis lookup",
                cancellationToken,
                () => LMCSingleAxis.CreateAsync(
                    currentConnection,
                    axisObjectName,
                    CancellationToken.None));
            if (selectedAxis.AxisReference != slaveReference)
            {
                throw new InvalidOperationException(
                    "D5 SDO qualification axis mapping mismatch. Object "
                    + axisObjectName
                    + " resolved to AxisReference "
                    + selectedAxis.AxisReference.ToString(
                        CultureInfo.InvariantCulture)
                    + ", but the requested EtherCAT slave reference is "
                    + slaveReference.ToString(
                        CultureInfo.InvariantCulture)
                    + ".");
            }

            var timeout = Stopwatch.StartNew();
            var stableSamples = 0;
            int? previousPosition = null;
            LMCReadStatusResult latestStatus = null;
            LMCReadActualPositionResult latestPosition = null;

            while (timeout.ElapsedMilliseconds
                <= D5SdoQualificationSafetyTimeoutMilliseconds)
            {
                latestStatus = await SendQualificationCommandAsync(
                    "D5 SDO qualification safety status",
                    cancellationToken,
                    () => selectedAxis.ReadStatusResultAsync(
                        CancellationToken.None));
                EnsureAxisStatusSuccess(
                    "D5 SDO qualification safety status",
                    latestStatus);
                if (latestStatus.IsPowerOn || !latestStatus.IsStandstill)
                {
                    throw new InvalidOperationException(
                        "D5 SDO qualification requires "
                        + axisObjectName
                        + " PowerOn=False and Standstill=True. Actual PowerOn="
                        + latestStatus.IsPowerOn
                        + ", Standstill="
                        + latestStatus.IsStandstill
                        + ".");
                }

                latestPosition = await SendQualificationCommandAsync(
                    "D5 SDO qualification no-motion position",
                    cancellationToken,
                    () => selectedAxis.GetActualPositionResultAsync(
                        CancellationToken.None));
                EnsureAxisPositionSuccess(
                    "D5 SDO qualification no-motion position",
                    latestPosition);
                if (timeout.ElapsedMilliseconds
                    > D5SdoQualificationSafetyTimeoutMilliseconds)
                {
                    break;
                }

                stableSamples = previousPosition.HasValue
                        && previousPosition.Value == latestPosition.PositionRaw
                    ? stableSamples + 1
                    : 1;
                previousPosition = latestPosition.PositionRaw;
                if (stableSamples >= QualificationStableSamples)
                {
                    WriteD5SdoQualificationLog(
                        "event=D5_SAFETY_PREFLIGHT",
                        "axis=" + QualificationValue(axisObjectName),
                        "slave=" + slaveReference.ToString(
                            CultureInfo.InvariantCulture),
                        "axisReference=" + selectedAxis.AxisReference.ToString(
                            CultureInfo.InvariantCulture),
                        "powerOn=false",
                        "standstill=true",
                        "stablePositionSamples=" + stableSamples.ToString(
                            CultureInfo.InvariantCulture),
                        "positionRaw=" + latestPosition.PositionRaw.ToString(
                            CultureInfo.InvariantCulture),
                        "wireMutation=false",
                        "verdict=PASS");
                    return;
                }

                await Task.Delay(
                    QualificationPollMilliseconds,
                    cancellationToken);
            }

            throw new TimeoutException(
                "D5 SDO qualification did not observe three identical powered-off axis positions within "
                + D5SdoQualificationSafetyTimeoutMilliseconds.ToString(
                    CultureInfo.InvariantCulture)
                + " ms. LastPosition="
                + (latestPosition == null
                    ? "none"
                    : latestPosition.PositionRaw.ToString(
                        CultureInfo.InvariantCulture))
                + ".");
        }

        private async Task<LMCDiagnosticCapabilities>
            ReadD5SdoQualificationCapabilitiesAsync(
                LMCDiagnostics diagnostics,
                CancellationToken cancellationToken,
                string stage)
        {
            var capabilities = await SendQualificationCommandAsync(
                "D5 SDO " + stage,
                cancellationToken,
                () => diagnostics.GetCapabilitiesAsync(
                    CancellationToken.None));
            cancellationToken.ThrowIfCancellationRequested();
            RequireD5SdoQualificationCapabilities(capabilities);
            return capabilities;
        }

        private async Task<LMCDiagnosticCapabilities>
            ReadD5SdoRecoveryCapabilitiesAsync(
                LMCDiagnostics diagnostics,
                CancellationToken cancellationToken,
                string stage)
        {
            var capabilities = await SendQualificationCommandAsync(
                "D5 SDO " + stage,
                cancellationToken,
                () => diagnostics.GetCapabilitiesAsync(
                    CancellationToken.None));
            cancellationToken.ThrowIfCancellationRequested();
            RequireD5SdoRecoveryCapabilities(capabilities);
            return capabilities;
        }

        private static void RequireD5SdoQualificationCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            RequireD5SdoRecoveryCapabilities(capabilities);

            if (!capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline))
            {
                throw new NotSupportedException(
                    "D5 SDO qualification requires SDORead and SDOReadGeneralInline capabilities.");
            }

            if (capabilities.MaxSdoDataBytes != 4)
            {
                throw new InvalidOperationException(
                    "D5 SDO qualification requires MaxSdoDataBytes=4.");
            }
        }

        private static void RequireD5SdoRecoveryCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            if (capabilities == null)
            {
                throw new InvalidOperationException(
                    "D5 SDO recovery did not receive diagnostics capabilities.");
            }

            if (!capabilities.Supports(LMCDiagnosticCapability.SDORead))
            {
                throw new NotSupportedException(
                    "D5 SDO recovery requires SDORead capability.");
            }

            if (capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "D5 SDO recovery requires non-zero DiagnosticsBootId and MapRevision.");
            }

            if (capabilities.BaseCycleTimeUs == 0
                || capabilities.MaxSdoDataBytes < 4
                || capabilities.MaxRequestPayloadBytes < 32
                || capabilities.MaxResponsePayloadBytes < 64)
            {
                throw new InvalidOperationException(
                    "D5 SDO recovery capabilities cannot carry the required read/status contract.");
            }
        }

        private static void RequireStableD5SdoQualificationCapabilities(
            LMCDiagnosticCapabilities expected,
            LMCDiagnosticCapabilities actual,
            string stage)
        {
            RequireD5SdoQualificationCapabilities(expected);
            RequireD5SdoQualificationCapabilities(actual);
            if (actual.DiagnosticsBootId != expected.DiagnosticsBootId
                || actual.MapRevision != expected.MapRevision)
            {
                throw new InvalidOperationException(
                    "D5 SDO "
                    + stage
                    + " capability identity changed. Expected BootId=0x"
                    + expected.DiagnosticsBootId.ToString("X8")
                    + ", MapRevision=0x"
                    + expected.MapRevision.ToString("X8")
                    + "; Actual BootId=0x"
                    + actual.DiagnosticsBootId.ToString("X8")
                    + ", MapRevision=0x"
                    + actual.MapRevision.ToString("X8")
                    + ".");
            }
        }

        private static void RequireStableD5SdoRecoveryCapabilities(
            LMCDiagnosticCapabilities expected,
            LMCDiagnosticCapabilities actual,
            string stage)
        {
            RequireD5SdoRecoveryCapabilities(expected);
            RequireD5SdoRecoveryCapabilities(actual);
            var expectedGeneralInline = expected.Supports(
                LMCDiagnosticCapability.SDOReadGeneralInline);
            var actualGeneralInline = actual.Supports(
                LMCDiagnosticCapability.SDOReadGeneralInline);
            if (actual.DiagnosticsBootId != expected.DiagnosticsBootId
                || actual.MapRevision != expected.MapRevision
                || actual.BaseCycleTimeUs != expected.BaseCycleTimeUs
                || actual.MaxSdoDataBytes != expected.MaxSdoDataBytes
                || actual.MaxRequestPayloadBytes
                    != expected.MaxRequestPayloadBytes
                || actual.MaxResponsePayloadBytes
                    != expected.MaxResponsePayloadBytes
                || actualGeneralInline != expectedGeneralInline)
            {
                throw new InvalidOperationException(
                    "D5 SDO "
                    + stage
                    + " recovery capabilities changed during proof.");
            }
        }

        private static int GetD5SdoQualificationTerminalWaitMilliseconds(
            uint timeoutCycles,
            uint baseCycleTimeUs)
        {
            var requestedMilliseconds = checked(
                ((ulong)timeoutCycles * baseCycleTimeUs + 999UL) / 1000UL);
            var requiredMilliseconds = checked(requestedMilliseconds + 5000UL);
            if (requiredMilliseconds
                > D5SdoQualificationMaximumTerminalWaitMilliseconds)
            {
                throw new InvalidOperationException(
                    "The selected TimeoutCycles and PLC BaseCycleTimeUs require "
                    + requiredMilliseconds.ToString(CultureInfo.InvariantCulture)
                    + " ms, which exceeds the D5 qualification bounded wait of "
                    + D5SdoQualificationMaximumTerminalWaitMilliseconds.ToString(
                        CultureInfo.InvariantCulture)
                    + " ms. Reduce TimeoutCycles.");
            }

            return (int)Math.Max(
                D5SdoQualificationMinimumTerminalWaitMilliseconds,
                requiredMilliseconds);
        }

        private async Task<LMCOperationTicket> SubmitD5SdoQualificationAsync(
            LMCConnection ownerConnection,
            LMCDiagnostics diagnostics,
            LMCSdoRequest request,
            uint expectedDiagnosticsBootId,
            CancellationToken cancellationToken,
            string stage,
            string objectIndex,
            byte subIndex,
            int terminalWaitMilliseconds)
        {
            if (ownerConnection == null)
            {
                throw new ArgumentNullException("ownerConnection");
            }

            if (expectedDiagnosticsBootId == 0)
            {
                throw new InvalidOperationException(
                    "D5 SDO submission requires a non-zero expected DiagnosticsBootId.");
            }

            var submissionEvidence = new D5SdoQualificationOrphanEvidence(
                0,
                expectedDiagnosticsBootId,
                request.SlaveReference,
                request.TimeoutCycles,
                ownerConnection,
                stage,
                "submit_response_unavailable",
                Guid.NewGuid().ToString("N"));
            var outcomeArmed = false;
            LMCOperationTicket ticket;
            try
            {
                ticket = await SendQualificationCommandAsync(
                    "D5 SDO " + stage + " submit",
                    cancellationToken,
                    () =>
                    {
                        d5SdoQualificationOrphanedTickets.Add(
                            submissionEvidence);
                        outcomeArmed = true;
                        WriteD5SdoQualificationLog(
                            "event=D5_SUBMIT_OUTCOME_GUARD",
                            "stage=" + stage,
                            "evidence=" + submissionEvidence.EvidenceId,
                            "bootId=0x"
                                + expectedDiagnosticsBootId.ToString("X8"),
                            "slave=" + request.SlaveReference.ToString(
                                CultureInfo.InvariantCulture),
                            "state=ARMED_BEFORE_SUBMIT");
                        return diagnostics.SubmitSdoAsync(
                            request,
                            CancellationToken.None);
                    });
            }
            catch (LMCDiagnosticsCommandException error)
            {
                if (outcomeArmed)
                {
                    d5SdoQualificationOrphanedTickets.Remove(
                        submissionEvidence);
                }

                WriteD5SdoQualificationLog(
                    "event=D5_SUBMIT_OUTCOME_GUARD",
                    "stage=" + stage,
                    "evidence=" + submissionEvidence.EvidenceId,
                    "state=EXPLICIT_PLC_REJECTION",
                    "detail=" + (error.Response == null
                        ? "none"
                        : error.Response.Detail.ToString()),
                    "quarantine=false");
                throw;
            }
            catch (Exception error)
            {
                if (outcomeArmed
                    && d5SdoQualificationOrphanedTickets.Contains(
                        submissionEvidence))
                {
                    WriteD5SdoQualificationLog(
                        "event=D5_SUBMIT_OUTCOME_GUARD",
                        "stage=" + stage,
                        "evidence=" + submissionEvidence.EvidenceId,
                        "state=OUTCOME_UNCERTAIN",
                        "errorType=" + error.GetType().Name,
                        "quarantine=true",
                        "oldTerminalConfirmed=false");
                }

                throw;
            }

            d5SdoQualificationActiveTicket = ticket;
            d5SdoQualificationActiveStatus = null;
            d5SdoQualificationActiveDeadlineUtc = DateTime.UtcNow.AddMilliseconds(
                terminalWaitMilliseconds);
            d5SdoQualificationActiveConnection = ownerConnection;
            d5SdoQualificationActiveSlaveReference = request.SlaveReference;
            d5SdoQualificationActiveTimeoutCycles = request.TimeoutCycles;
            if (!d5SdoQualificationOrphanedTickets.Remove(
                submissionEvidence))
            {
                throw new InvalidOperationException(
                    "The D5 submission outcome guard was lost after an accepted ticket. The accepted ticket remains preserved for cleanup.");
            }

            WriteD5SdoQualificationLog(
                "event=D5_SUBMIT",
                "stage=" + stage,
                "ticket=" + ticket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "slave=" + request.SlaveReference.ToString(
                    CultureInfo.InvariantCulture),
                "object=" + objectIndex,
                "subIndex=" + subIndex.ToString(
                    CultureInfo.InvariantCulture),
                "valueType=" + request.ValueType,
                "dataLength=" + request.DataLength.ToString(
                    CultureInfo.InvariantCulture),
                    "timeoutCycles=" + request.TimeoutCycles.ToString(
                        CultureInfo.InvariantCulture),
                    "terminalWaitMs=" + terminalWaitMilliseconds.ToString(
                        CultureInfo.InvariantCulture),
                "bootId=0x" + ticket.DiagnosticsBootId.ToString("X8"),
                "wireMutation=false");
            return ticket;
        }

        private async Task<LMCOperationStatus>
            WaitForD5SdoQualificationTerminalAsync(
                LMCDiagnostics diagnostics,
                LMCOperationTicket ticket,
                int timeoutMilliseconds,
                CancellationToken cancellationToken,
                string stage)
        {
            var timeout = Stopwatch.StartNew();
            var poll = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                poll++;
                var status = await SendQualificationCommandAsync(
                    "D5 SDO " + stage + " status",
                    cancellationToken,
                    () => diagnostics.GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None));
                d5SdoQualificationActiveStatus = status;
                WriteD5SdoQualificationLog(
                    "event=D5_STATUS",
                    "stage=" + stage,
                    "poll=" + poll.ToString(CultureInfo.InvariantCulture),
                    "ticket=" + ticket.TicketId.ToString(
                        CultureInfo.InvariantCulture),
                    "state=" + status.State,
                    "outcome=" + status.Outcome,
                    "errorId=" + status.OperationErrorId.ToString(
                        CultureInfo.InvariantCulture),
                    "detail=0x" + status.OperationDetail.ToString("X8"),
                    "resultLength=" + status.ResultLength.ToString(
                        CultureInfo.InvariantCulture));
                if (status.IsTerminal)
                {
                    ClearActiveD5SdoQualificationTicket();
                    return status;
                }

                if (timeout.ElapsedMilliseconds >= timeoutMilliseconds)
                {
                    throw new TimeoutException(
                        "D5 SDO "
                        + stage
                        + " ticket "
                        + ticket.TicketId.ToString(CultureInfo.InvariantCulture)
                        + " did not reach a terminal state within "
                        + timeoutMilliseconds.ToString(
                            CultureInfo.InvariantCulture)
                        + " ms. LastState="
                        + status.State
                        + ".");
                }

                await Task.Delay(
                    D5SdoQualificationPollMilliseconds,
                    cancellationToken);
            }
        }

        private void WriteD5SdoQualificationTerminalLog(
            string stage,
            LMCOperationTicket ticket,
            LMCOperationStatus status)
        {
            WriteD5SdoQualificationLog(
                "event=D5_TERMINAL",
                "stage=" + stage,
                "ticket=" + ticket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "state=" + status.State,
                "outcome=" + status.Outcome,
                "errorId=" + status.OperationErrorId.ToString(
                    CultureInfo.InvariantCulture),
                "detail=0x" + status.OperationDetail.ToString("X8"),
                "submitCycle=" + status.SubmitCycle.ToString(
                    CultureInfo.InvariantCulture),
                "completionCycle=" + status.CompletionCycle.ToString(
                    CultureInfo.InvariantCulture),
                "cycleDelta=" + unchecked(
                    status.CompletionCycle - status.SubmitCycle).ToString(
                        CultureInfo.InvariantCulture),
                "resultType=" + status.ResultValueType,
                "resultLength=" + status.ResultLength.ToString(
                    CultureInfo.InvariantCulture));
        }

        private async Task CleanupPendingD5SdoQualificationAsync()
        {
            var ticket = d5SdoQualificationActiveTicket;
            if (ticket == null)
            {
                return;
            }

            var currentConnection = RequireConnection();
            if (d5SdoQualificationActiveConnection != null
                && !ReferenceEquals(
                    d5SdoQualificationActiveConnection,
                    currentConnection))
            {
                throw new InvalidOperationException(
                    "The preserved D5 ticket belongs to an earlier LMCConnection. Use the quarantine recovery proof instead of querying it from the new session.");
            }

            var diagnostics = currentConnection.Diagnostics;
            var status = d5SdoQualificationActiveStatus;
            WriteD5SdoQualificationLog(
                "event=D5_CLEANUP",
                "ticket=" + ticket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "action=INSPECT_PENDING_TICKET",
                "plcStopCommand=false",
                "verdict=START");

            if (status == null || !status.IsTerminal)
            {
                status = await SendQualificationCleanupCommandAsync(
                    "D5 SDO pending ticket status",
                    () => diagnostics.GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None));
                d5SdoQualificationActiveStatus = status;
            }

            var cancelAccepted = false;
            if (status.State == LMCOperationState.Queued)
            {
                try
                {
                    await SendQualificationCleanupCommandAsync(
                        "D5 SDO queued ticket cancel",
                        () => diagnostics.CancelOperationAsync(
                            ticket,
                            CancellationToken.None));
                    cancelAccepted = true;
                    WriteD5SdoQualificationLog(
                        "event=D5_CLEANUP",
                        "ticket=" + ticket.TicketId.ToString(
                            CultureInfo.InvariantCulture),
                        "action=CANCEL_QUEUED_TICKET",
                        "plcStopCommand=false",
                        "verdict=ACCEPTED");
                }
                catch (LMCDiagnosticsCommandException error)
                {
                    if (error.Response == null
                        || error.Response.Detail
                            != LMCDiagnosticsDetailCode.InvalidState)
                    {
                        throw;
                    }

                    WriteD5SdoQualificationLog(
                        "event=D5_CLEANUP",
                        "ticket=" + ticket.TicketId.ToString(
                            CultureInfo.InvariantCulture),
                        "action=CANCEL_QUEUED_TICKET",
                        "result=RACE_TO_NONQUEUED",
                        "plcStopCommand=false");
                }
            }

            if (!status.IsTerminal || cancelAccepted)
            {
                var cleanupWaitMilliseconds =
                    GetD5SdoQualificationCleanupWaitMilliseconds();
                status = await WaitForD5SdoQualificationCleanupTerminalAsync(
                    diagnostics,
                    ticket,
                    cleanupWaitMilliseconds);
            }

            if (!status.IsTerminal)
            {
                throw new TimeoutException(
                    "D5 SDO cleanup did not resolve the pending ticket.");
            }

            if (cancelAccepted
                && (status.State != LMCOperationState.Cancelled
                    || status.Outcome != LMCOperationOutcome.Cancelled))
            {
                throw new InvalidOperationException(
                    "CancelOperation was accepted but the D5 SDO ticket did not become Cancelled/Cancelled.");
            }

            d5SdoQualificationActiveStatus = status;
            SynchronizeManualDiagnosticOperationTerminal(
                ticket,
                status,
                cancelAccepted);
            ClearActiveD5SdoQualificationTicket();
            WriteD5SdoQualificationLog(
                "event=D5_CLEANUP",
                "ticket=" + ticket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "action=" + (cancelAccepted
                    ? "CANCEL_QUEUED_TICKET"
                    : "WAIT_RUNNING_OR_TERMINAL"),
                "state=" + status.State,
                "outcome=" + status.Outcome,
                "plcStopCommand=false",
                "verdict=PASS");
        }

        private async Task<LMCOperationStatus>
            WaitForD5SdoQualificationCleanupTerminalAsync(
                LMCDiagnostics diagnostics,
                LMCOperationTicket ticket,
                int timeoutMilliseconds)
        {
            var timeout = Stopwatch.StartNew();
            LMCOperationStatus status = null;
            while (timeout.ElapsedMilliseconds
                <= timeoutMilliseconds)
            {
                status = await SendQualificationCleanupCommandAsync(
                    "D5 SDO cleanup terminal status",
                    () => diagnostics.GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None));
                d5SdoQualificationActiveStatus = status;
                if (status.IsTerminal)
                {
                    return status;
                }

                await Task.Delay(D5SdoQualificationPollMilliseconds);
            }

            throw new TimeoutException(
                "D5 SDO ticket "
                + ticket.TicketId.ToString(CultureInfo.InvariantCulture)
                + " remained "
                + (status == null ? "unknown" : status.State.ToString())
                + " beyond the "
                + timeoutMilliseconds.ToString(
                    CultureInfo.InvariantCulture)
                + " ms cleanup bound. The Cancel Test button does not send a PLC Stop command.");
        }

        private int GetD5SdoQualificationCleanupWaitMilliseconds()
        {
            var remainingMilliseconds = 0L;
            if (d5SdoQualificationActiveDeadlineUtc.HasValue)
            {
                remainingMilliseconds = Math.Max(
                    0L,
                    (long)Math.Ceiling(
                        (d5SdoQualificationActiveDeadlineUtc.Value
                            - DateTime.UtcNow).TotalMilliseconds));
            }

            var requested = Math.Max(
                D5SdoQualificationCleanupTimeoutMilliseconds,
                remainingMilliseconds + 1000L);
            return checked((int)Math.Min(
                D5SdoQualificationMaximumTerminalWaitMilliseconds,
                requested));
        }

        private async Task ResolvePreservedD5SdoQualificationAsync(
            CancellationToken cancellationToken)
        {
            var currentConnection = RequireConnection();
            if (d5SdoQualificationActiveTicket != null
                && ReferenceEquals(
                    d5SdoQualificationActiveConnection,
                    currentConnection))
            {
                var currentCapabilities =
                    await SendQualificationCleanupCommandAsync(
                        "D5 SDO preserved ticket BootId preflight",
                        () => currentConnection.Diagnostics
                            .GetCapabilitiesAsync(CancellationToken.None));
                if (currentCapabilities == null
                    || currentCapabilities.DiagnosticsBootId == 0)
                {
                    throw new InvalidOperationException(
                        "D5 ticket resolution requires a non-zero current DiagnosticsBootId.");
                }

                if (currentCapabilities.DiagnosticsBootId
                    != d5SdoQualificationActiveTicket.DiagnosticsBootId)
                {
                    QuarantineStaleSessionD5SdoQualificationTicket(
                        "diagnostics_boot_id_changed");
                }
                else
                {
                    try
                    {
                        await CleanupPendingD5SdoQualificationAsync();
                    }
                    catch (LMCDiagnosticsCommandException error)
                        when (error.Response != null
                            && error.Response.Detail
                                == LMCDiagnosticsDetailCode.BootIdMismatch)
                    {
                        QuarantineStaleSessionD5SdoQualificationTicket(
                            "plc_boot_id_mismatch_response");
                    }
                    catch (LMCDiagnosticsCommandException error)
                        when (error.Response != null
                            && error.Response.Detail
                                == LMCDiagnosticsDetailCode
                                    .HandleOrGenerationStale)
                    {
                        QuarantineStaleSessionD5SdoQualificationTicket(
                            "plc_owner_session_epoch_stale");
                    }
                    catch (LMCDiagnosticsCommandException error)
                        when (error.Response != null
                            && error.Response.Detail
                                == LMCDiagnosticsDetailCode.TicketNotFound)
                    {
                        ResolveSupersededD5SdoQualificationTicket(
                            "plc_ticket_slot_superseded");
                    }
                    catch (InvalidOperationException error)
                        when (IsStaleD5OperationTicketException(error))
                    {
                        QuarantineStaleSessionD5SdoQualificationTicket(
                            "local_ticket_session_invalid");
                    }
                }
            }

            if (d5SdoQualificationActiveTicket != null)
            {
                QuarantineStaleSessionD5SdoQualificationTicket(
                    "connection_owner_or_session_changed");
            }

            if (d5SdoQualificationOrphanedTickets.Count != 0)
            {
                await ProveStaleSessionD5SdoRecoveryAsync(
                    currentConnection,
                    cancellationToken);
            }

            CloseExternalD5TrackingLogIfResolved(
                "QUARANTINE_RESOLUTION_COMPLETED");
        }

        private void QuarantineStaleSessionD5SdoQualificationTicket(
            string reason)
        {
            if (d5SdoQualificationActiveTicket == null)
            {
                return;
            }

            var evidence = new D5SdoQualificationOrphanEvidence(
                d5SdoQualificationActiveTicket.TicketId,
                d5SdoQualificationActiveTicket.DiagnosticsBootId,
                d5SdoQualificationActiveSlaveReference,
                d5SdoQualificationActiveTimeoutCycles,
                d5SdoQualificationActiveConnection,
                "preserved-ticket",
                reason,
                "ticket-"
                    + d5SdoQualificationActiveTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture)
                    + "-boot-"
                    + d5SdoQualificationActiveTicket.DiagnosticsBootId
                        .ToString("X8"));
            d5SdoQualificationOrphanedTickets.Add(evidence);
            WriteD5SdoQualificationLog(
                "event=D5_ORPHAN_QUARANTINE",
                "ticket=" + evidence.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "oldBootId=0x"
                    + evidence.DiagnosticsBootId.ToString("X8"),
                "slave=" + evidence.SlaveReference.ToString(
                    CultureInfo.InvariantCulture),
                "quarantineCount="
                    + d5SdoQualificationOrphanedTickets.Count.ToString(
                        CultureInfo.InvariantCulture),
                "oldTerminalConfirmed=false",
                "reason=" + QualificationValue(reason),
                "verdict=ORPHAN_UNVERIFIED");
            InvalidateQuarantinedManualDiagnosticOperation(
                d5SdoQualificationActiveTicket,
                reason);
            d5SdoQualificationActiveStatus = null;
            ClearActiveD5SdoQualificationTicket();
        }

        private void ResolveSupersededD5SdoQualificationTicket(
            string reason)
        {
            var ticket = d5SdoQualificationActiveTicket;
            if (ticket == null)
            {
                return;
            }

            WriteD5SdoQualificationLog(
                "event=D5_CLEANUP",
                "ticket=" + ticket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "action=TICKET_SLOT_SUPERSEDED",
                "state=TERMINAL_INFERRED",
                "outcome=UNKNOWN",
                "oldTerminalConfirmed=true",
                "reason=" + QualificationValue(reason),
                "verdict=RESOLVED_BY_SLOT_CONTRACT");
            ClearSupersededManualDiagnosticOperation(ticket, reason);
            d5SdoQualificationActiveStatus = null;
            ClearActiveD5SdoQualificationTicket();
        }

        private static bool IsStaleD5OperationTicketException(
            InvalidOperationException error)
        {
            if (error == null)
            {
                return false;
            }

            return error.Message
                    == "The operation ticket belongs to a different LMCConnection."
                || error.Message
                    == "The operation ticket belongs to a stale connection session."
                || error.Message
                    == "The operation ticket DiagnosticsBootId is stale.";
        }

        private async Task ProveStaleSessionD5SdoRecoveryAsync(
            LMCConnection currentConnection,
            CancellationToken cancellationToken)
        {
            var orphanedTickets = d5SdoQualificationOrphanedTickets.ToArray();
            if (orphanedTickets.Length == 0)
            {
                return;
            }

            var slaveReference = orphanedTickets[0].SlaveReference;
            if (slaveReference < 1 || slaveReference > 4)
            {
                throw new InvalidOperationException(
                    "The quarantined D5 ticket has no valid slave reference for recovery proof.");
            }

            if (orphanedTickets.Any(
                item => item.SlaveReference != slaveReference))
            {
                throw new InvalidOperationException(
                    "Quarantined D5 tickets span different slave references; automatic recovery proof is blocked.");
            }

            var timeoutCycles = orphanedTickets.Max(
                item => item.TimeoutCycles);
            if (timeoutCycles < 1 || timeoutCycles > 60000)
            {
                throw new InvalidOperationException(
                    "The quarantined D5 ticket has no valid TimeoutCycles for recovery proof.");
            }

            var axisObjectName = "_LMCAxis"
                + slaveReference.ToString(CultureInfo.InvariantCulture);
            SetD5SdoQualificationProgress(
                10,
                "Verifying stale-session D5 recovery safety state");
            await VerifyD5SdoQualificationSafeAxisAsync(
                currentConnection,
                slaveReference,
                axisObjectName,
                cancellationToken);

            var diagnostics = currentConnection.Diagnostics;
            var firstCapabilities =
                await ReadD5SdoRecoveryCapabilitiesAsync(
                    diagnostics,
                    cancellationToken,
                    "orphan recovery capability sample 1");
            await Task.Delay(
                D5SdoQualificationPollMilliseconds,
                cancellationToken);
            var capabilities = await ReadD5SdoRecoveryCapabilitiesAsync(
                diagnostics,
                cancellationToken,
                "orphan recovery capability sample 2");
            RequireStableD5SdoRecoveryCapabilities(
                firstCapabilities,
                capabilities,
                "orphan recovery preflight");
            var ownerChangedEvidenceCount = orphanedTickets.Count(
                item => item.OwnerConnection != null
                    && !ReferenceEquals(
                        item.OwnerConnection,
                        currentConnection));
            var sameOwnerEvidenceCount = orphanedTickets.Count(
                item => ReferenceEquals(
                    item.OwnerConnection,
                    currentConnection));
            var bootChangedEvidenceCount = orphanedTickets.Count(
                item => item.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId);
            var hasSameOwnerAndBootEvidence = orphanedTickets.Any(
                item => ReferenceEquals(item.OwnerConnection, currentConnection)
                    && item.DiagnosticsBootId
                        == capabilities.DiagnosticsBootId);
            var ownerConnectionChangedForAllEvidence =
                ownerChangedEvidenceCount == orphanedTickets.Length;
            var hasSameOwnerBootChange = orphanedTickets.Any(
                item => ReferenceEquals(item.OwnerConnection, currentConnection)
                    && item.DiagnosticsBootId
                        != capabilities.DiagnosticsBootId);
            var proofScope = hasSameOwnerAndBootEvidence
                ? "same_owner_connection_recovery"
                : hasSameOwnerBootChange
                    ? "new_diagnostics_boot_session"
                    : "new_connection_session";
            var proofScopeText = hasSameOwnerAndBootEvidence
                ? "same-owner connection recovery"
                : hasSameOwnerBootChange
                    ? "new diagnostics-Boot session"
                    : "new connection session";
            WriteD5SdoQualificationLog(
                "event=D5_QUARANTINE_PROOF_SCOPE",
                "scope=" + proofScope,
                "currentBootId=0x"
                    + capabilities.DiagnosticsBootId.ToString("X8"),
                "evidenceCount=" + orphanedTickets.Length.ToString(
                    CultureInfo.InvariantCulture),
                "ownerChangedEvidence=" + ownerChangedEvidenceCount.ToString(
                    CultureInfo.InvariantCulture),
                "sameOwnerEvidence=" + sameOwnerEvidenceCount.ToString(
                    CultureInfo.InvariantCulture),
                "bootChangedEvidence=" + bootChangedEvidenceCount.ToString(
                    CultureInfo.InvariantCulture),
                "newConnectionRecovery="
                    + (ownerConnectionChangedForAllEvidence
                        ? "true"
                        : "false"),
                "orphanQualified=false",
                "orphanProof=NOT_PROVEN_BY_WPF");
            var terminalWaitMilliseconds =
                GetD5SdoQualificationTerminalWaitMilliseconds(
                    timeoutCycles,
                    capabilities.BaseCycleTimeUs);
            var supportsGeneralInline = capabilities.Supports(
                LMCDiagnosticCapability.SDOReadGeneralInline);
            var proofObjectIndex = supportsGeneralInline
                ? (ushort)0x6061
                : (ushort)0x1000;
            var proofValueType = supportsGeneralInline
                ? LMCSignalValueType.Int8
                : LMCSignalValueType.UInt32;
            var proofDataLength = supportsGeneralInline
                ? (ushort)1
                : (ushort)4;
            var proofObjectText = "0x"
                + proofObjectIndex.ToString("X4");
            var request = LMCSdoRequest.CreateRead(
                slaveReference,
                proofObjectIndex,
                0,
                proofValueType,
                proofDataLength,
                timeoutCycles);

            SetD5SdoQualificationProgress(
                35,
                "Submitting first "
                    + proofScopeText
                    + " "
                    + proofObjectText
                    + " recovery proof");
            var firstTicket = await SubmitD5SdoQualificationAsync(
                currentConnection,
                diagnostics,
                request,
                capabilities.DiagnosticsBootId,
                cancellationToken,
                "orphan-recovery-1",
                proofObjectText,
                0,
                terminalWaitMilliseconds);
            var firstStatus = await WaitForD5SdoQualificationTerminalAsync(
                diagnostics,
                firstTicket,
                terminalWaitMilliseconds,
                cancellationToken,
                "orphan-recovery-1");
            var expectedValue = firstStatus.ResultData;
            D5SdoQualificationAnalysis.ValidateKnownValidRecovery(
                firstTicket,
                firstStatus,
                capabilities.DiagnosticsBootId,
                proofValueType,
                expectedValue);

            var middleCapabilities =
                await ReadD5SdoRecoveryCapabilitiesAsync(
                    diagnostics,
                    cancellationToken,
                    "orphan recovery middle capability");
            RequireStableD5SdoRecoveryCapabilities(
                capabilities,
                middleCapabilities,
                "orphan recovery between reads");

            SetD5SdoQualificationProgress(
                65,
                "Submitting second exact-value "
                    + proofScopeText
                    + " "
                    + proofObjectText
                    + " recovery proof");
            var secondTicket = await SubmitD5SdoQualificationAsync(
                currentConnection,
                diagnostics,
                request,
                capabilities.DiagnosticsBootId,
                cancellationToken,
                "orphan-recovery-2",
                proofObjectText,
                0,
                terminalWaitMilliseconds);
            var secondStatus = await WaitForD5SdoQualificationTerminalAsync(
                diagnostics,
                secondTicket,
                terminalWaitMilliseconds,
                cancellationToken,
                "orphan-recovery-2");
            D5SdoQualificationAnalysis.ValidateKnownValidRecovery(
                secondTicket,
                secondStatus,
                capabilities.DiagnosticsBootId,
                proofValueType,
                expectedValue);
            if (firstTicket.TicketId == secondTicket.TicketId)
            {
                throw new InvalidOperationException(
                    "D5 quarantine recovery proof requires two distinct recovery tickets.");
            }

            var finalCapabilities =
                await ReadD5SdoRecoveryCapabilitiesAsync(
                    diagnostics,
                    cancellationToken,
                    "orphan recovery final capability");
            RequireStableD5SdoRecoveryCapabilities(
                capabilities,
                finalCapabilities,
                "orphan recovery full proof");
            if (d5SdoQualificationOrphanedTickets.Count
                    != orphanedTickets.Length
                || !d5SdoQualificationOrphanedTickets.SequenceEqual(
                    orphanedTickets))
            {
                throw new InvalidOperationException(
                    "The D5 orphan quarantine changed during recovery proof; it will not be cleared.");
            }

            WriteD5SdoQualificationLog(
                "event=D5_QUARANTINE_RECOVERY",
                "oldTickets=" + string.Join(
                    ",",
                    orphanedTickets.Select(
                        item => item.TicketId == 0
                            ? "UNKNOWN:" + item.EvidenceId
                            : item.TicketId.ToString(
                                CultureInfo.InvariantCulture))),
                "evidenceBootIds=" + string.Join(
                    ",",
                    orphanedTickets.Select(
                        item => "0x" + item.DiagnosticsBootId.ToString("X8"))),
                "quarantineCount=" + orphanedTickets.Length.ToString(
                    CultureInfo.InvariantCulture),
                "uncertainSubmissions=" + orphanedTickets.Count(
                    item => item.TicketId == 0).ToString(
                        CultureInfo.InvariantCulture),
                "evidenceStages=" + string.Join(
                    ",",
                    orphanedTickets.Select(item => item.Stage)),
                "evidenceReasons=" + string.Join(
                    ",",
                    orphanedTickets.Select(item => item.Reason)),
                "oldTerminalConfirmed=false",
                "recoveryBootId=0x"
                    + capabilities.DiagnosticsBootId.ToString("X8"),
                "proofObject=" + proofObjectText,
                "proofValueType=" + proofValueType,
                "proofLength=" + proofDataLength.ToString(
                    CultureInfo.InvariantCulture),
                "proofTicket1=" + firstTicket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "proofTicket2=" + secondTicket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "proofScope=" + proofScope,
                "ownerChangedEvidence=" + ownerChangedEvidenceCount.ToString(
                    CultureInfo.InvariantCulture),
                "sameOwnerEvidence=" + sameOwnerEvidenceCount.ToString(
                    CultureInfo.InvariantCulture),
                "bootChangedEvidence=" + bootChangedEvidenceCount.ToString(
                    CultureInfo.InvariantCulture),
                "newConnectionRecovery="
                    + (ownerConnectionChangedForAllEvidence
                        ? "true"
                        : "false"),
                "orphanQualified=false",
                "orphanProof=NOT_PROVEN_BY_WPF",
                "proof=two_distinct_exact_type_length_value_reads",
                "verdict=PASS");
            d5SdoQualificationOrphanedTickets.Clear();
        }

        private void ClearActiveD5SdoQualificationTicket()
        {
            d5SdoQualificationActiveTicket = null;
            d5SdoQualificationActiveDeadlineUtc = null;
            d5SdoQualificationActiveConnection = null;
            d5SdoQualificationActiveSlaveReference = 0;
            d5SdoQualificationActiveTimeoutCycles = 0;
        }

        private void SynchronizeManualDiagnosticOperationTerminal(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            bool cancelAccepted)
        {
            if (!IsSameDiagnosticOperationTicket(
                    ticket,
                    diagnosticOperationTicket))
            {
                return;
            }

            diagnosticOperationStatus = status;
            diagnosticOperationCancelAccepted = cancelAccepted;
            diagnosticOperationResult = status != null
                && status.IsSuccessful
                && ticket.OperationKind == LMCOperationKind.SDORead
                && !ticket.UsesExtendedResultChunks
                    ? status.ResultData
                    : null;
            TextDiagnosticOperationSummary.Text =
                FormatOperationStatus(status)
                + Environment.NewLine
                + "Resolved by D5 quarantine cleanup.";
        }

        private void InvalidateQuarantinedManualDiagnosticOperation(
            LMCOperationTicket ticket,
            string reason)
        {
            if (!IsSameDiagnosticOperationTicket(
                    ticket,
                    diagnosticOperationTicket))
            {
                return;
            }

            diagnosticOperationTicket = null;
            diagnosticOperationStatus = null;
            diagnosticOperationResult = null;
            diagnosticOperationCancelAccepted = false;
            TextDiagnosticOperationSummary.Text =
                "The previous manual SDO ticket moved to D5 quarantine; "
                + "direct refresh is no longer allowed. reason="
                + reason;
        }

        private void ClearSupersededManualDiagnosticOperation(
            LMCOperationTicket ticket,
            string reason)
        {
            if (!IsSameDiagnosticOperationTicket(
                    ticket,
                    diagnosticOperationTicket))
            {
                return;
            }

            diagnosticOperationTicket = null;
            diagnosticOperationStatus = null;
            diagnosticOperationResult = null;
            diagnosticOperationCancelAccepted = false;
            TextDiagnosticOperationSummary.Text =
                "The previous manual SDO ticket is no longer in the PLC terminal slot. "
                + "Its terminal outcome is unavailable; the slot contract proves it was terminal before replacement. reason="
                + reason;
        }

        private static bool IsSameDiagnosticOperationTicket(
            LMCOperationTicket left,
            LMCOperationTicket right)
        {
            return left != null
                && right != null
                && left.TicketId == right.TicketId
                && left.DiagnosticsBootId == right.DiagnosticsBootId
                && left.OperationKind == right.OperationKind;
        }

        private async Task<LMCDiagnosticCapabilities>
            ReadExternalD5TrackingCapabilitiesAsync(
                LMCConnection ownerConnection,
                string stage,
                ushort requiredDataBytes,
                bool requireGeneralInline)
        {
            if (ownerConnection == null)
            {
                throw new ArgumentNullException("ownerConnection");
            }

            BeginExternalD5TrackingLog(stage);
            try
            {
                var capabilities = await ownerConnection.Diagnostics
                    .GetCapabilitiesAsync(CancellationToken.None);
                RequireExternalD5TrackingCapabilities(
                    capabilities,
                    requiredDataBytes,
                    requireGeneralInline);
                diagnosticCapabilities = capabilities;
                if (TextDiagnosticsCapabilities != null)
                {
                    TextDiagnosticsCapabilities.Text =
                        FormatCapabilities(capabilities);
                }

                WriteExternalD5TrackingLog(
                    "event=D5_EXTERNAL_PREFLIGHT",
                    "stage=" + stage,
                    "bootId=0x"
                        + capabilities.DiagnosticsBootId.ToString("X8"),
                    "mapRevision=0x"
                        + capabilities.MapRevision.ToString("X8"),
                    "requiredDataBytes=" + requiredDataBytes.ToString(
                        CultureInfo.InvariantCulture),
                    "generalInlineRequired="
                        + (requireGeneralInline ? "true" : "false"),
                    "verdict=PASS");
                return capabilities;
            }
            catch (Exception error)
            {
                WriteExternalD5TrackingLog(
                    "event=D5_EXTERNAL_PREFLIGHT",
                    "stage=" + stage,
                    "errorType=" + error.GetType().Name,
                    "error=" + QualificationValue(error.Message),
                    "verdict=FAIL");
                CloseExternalD5TrackingLogIfResolved("PREFLIGHT_FAILED");
                throw;
            }
        }

        private static void RequireExternalD5TrackingCapabilities(
            LMCDiagnosticCapabilities capabilities,
            ushort requiredDataBytes,
            bool requireGeneralInline)
        {
            RequireD5SdoRecoveryCapabilities(capabilities);
            if (requiredDataBytes == 0
                || capabilities.MaxSdoDataBytes < requiredDataBytes)
            {
                throw new NotSupportedException(
                    "External D5 tracking requires the requested SDO read length to fit MaxSdoDataBytes.");
            }

            if (requireGeneralInline
                && !capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline))
            {
                throw new NotSupportedException(
                    "This external D5 read requires SDOReadGeneralInline capability.");
            }
        }

        private static bool RequiresGeneralInlineSdoRead(
            LMCSdoRequest request)
        {
            return request == null
                || request.IsWrite
                || request.ObjectIndex != 0x1000
                || request.SubIndex != 0
                || request.ValueType != LMCSignalValueType.UInt32
                || request.DataLength != 4;
        }

        private bool Phase1AllowsPiWrite
        {
            get { return false; }
        }

        private D5SdoQualificationOrphanEvidence
            ArmExternalD5SubmissionOutcomeGuard(
                LMCConnection ownerConnection,
                uint expectedDiagnosticsBootId,
                ushort slaveReference,
                uint timeoutCycles,
                string stage)
        {
            var evidence = new D5SdoQualificationOrphanEvidence(
                0,
                expectedDiagnosticsBootId,
                slaveReference,
                timeoutCycles,
                ownerConnection,
                stage,
                "external_submit_response_unavailable",
                Guid.NewGuid().ToString("N"));
            d5SdoQualificationOrphanedTickets.Add(evidence);
            WriteExternalD5TrackingLog(
                "event=D5_EXTERNAL_SUBMIT_GUARD",
                "stage=" + stage,
                "evidence=" + evidence.EvidenceId,
                "slave=" + slaveReference.ToString(
                    CultureInfo.InvariantCulture),
                "bootId=0x" + expectedDiagnosticsBootId.ToString("X8"),
                "state=ARMED_BEFORE_SUBMIT");
            return evidence;
        }

        private void DisarmExternalD5SubmissionOutcomeGuard(
            D5SdoQualificationOrphanEvidence evidence,
            string state,
            string detail)
        {
            if (evidence == null
                || !d5SdoQualificationOrphanedTickets.Remove(evidence))
            {
                throw new InvalidOperationException(
                    "The external D5 submission outcome guard could not be released.");
            }

            WriteExternalD5TrackingLog(
                "event=D5_EXTERNAL_SUBMIT_GUARD",
                "stage=" + evidence.Stage,
                "evidence=" + evidence.EvidenceId,
                "state=" + state,
                "detail=" + QualificationValue(detail),
                "quarantine=false");
            CloseExternalD5TrackingLogIfResolved(state);
        }

        private void PreserveExternalD5SubmissionOutcomeUncertain(
            D5SdoQualificationOrphanEvidence evidence,
            Exception error)
        {
            if (evidence == null
                || !d5SdoQualificationOrphanedTickets.Contains(evidence))
            {
                throw new InvalidOperationException(
                    "The external D5 uncertain-submission evidence was lost.",
                    error);
            }

            WriteExternalD5TrackingLog(
                "event=D5_EXTERNAL_SUBMIT_GUARD",
                "stage=" + evidence.Stage,
                "evidence=" + evidence.EvidenceId,
                "state=OUTCOME_UNCERTAIN",
                "errorType=" + (error == null
                    ? "Unknown"
                    : error.GetType().Name),
                "quarantine=true",
                "oldTerminalConfirmed=false");
        }

        private void PreserveExternalD5Ticket(
            LMCOperationTicket ticket,
            LMCConnection ownerConnection,
            ushort slaveReference,
            uint timeoutCycles,
            string stage)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            if (d5SdoQualificationActiveTicket != null)
            {
                throw new InvalidOperationException(
                    "Another D5 ticket is already preserved.");
            }

            var terminalWaitMilliseconds =
                GetExternalD5TerminalWaitMilliseconds(timeoutCycles);
            d5SdoQualificationActiveTicket = ticket;
            d5SdoQualificationActiveStatus = null;
            d5SdoQualificationActiveDeadlineUtc =
                DateTime.UtcNow.AddMilliseconds(terminalWaitMilliseconds);
            d5SdoQualificationActiveConnection = ownerConnection;
            d5SdoQualificationActiveSlaveReference = slaveReference;
            d5SdoQualificationActiveTimeoutCycles = timeoutCycles;
            WriteExternalD5TrackingLog(
                "event=D5_EXTERNAL_TICKET_PRESERVED",
                "stage=" + stage,
                "ticket=" + ticket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "slave=" + slaveReference.ToString(
                    CultureInfo.InvariantCulture),
                "bootId=0x" + ticket.DiagnosticsBootId.ToString("X8"),
                "terminalWaitMs=" + terminalWaitMilliseconds.ToString(
                    CultureInfo.InvariantCulture));
        }

        private int GetExternalD5TerminalWaitMilliseconds(uint timeoutCycles)
        {
            if (diagnosticCapabilities != null
                && diagnosticCapabilities.BaseCycleTimeUs != 0)
            {
                return GetD5SdoQualificationTerminalWaitMilliseconds(
                    timeoutCycles,
                    diagnosticCapabilities.BaseCycleTimeUs);
            }

            return D5SdoQualificationCleanupTimeoutMilliseconds;
        }

        private void CompleteExternalD5TicketIfTerminal(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            string stage)
        {
            if (ticket == null
                || status == null
                || !status.IsTerminal
                || !ReferenceEquals(
                    d5SdoQualificationActiveTicket,
                    ticket))
            {
                return;
            }

            d5SdoQualificationActiveStatus = status;
            ClearActiveD5SdoQualificationTicket();
            WriteExternalD5TrackingLog(
                "event=D5_EXTERNAL_TICKET_TERMINAL",
                "stage=" + stage,
                "ticket=" + ticket.TicketId.ToString(
                    CultureInfo.InvariantCulture),
                "state=" + status.State,
                "outcome=" + status.Outcome,
                "verdict=CLEARED");
            CloseExternalD5TrackingLogIfResolved("KNOWN_TICKET_TERMINAL");
        }

        private async Task<T> RunTrackedExternalD5ReadAsync<T>(
            LMCConnection ownerConnection,
            ushort slaveReference,
            uint timeoutCycles,
            string stage,
            ushort requiredDataBytes,
            Func<Task<T>> read)
        {
            if (read == null)
            {
                throw new ArgumentNullException("read");
            }

            var capabilities =
                await ReadExternalD5TrackingCapabilitiesAsync(
                    ownerConnection,
                    stage,
                    requiredDataBytes,
                    true);
            var evidence = ArmExternalD5SubmissionOutcomeGuard(
                ownerConnection,
                capabilities.DiagnosticsBootId,
                slaveReference,
                timeoutCycles,
                stage);
            try
            {
                var result = await read();
                DisarmExternalD5SubmissionOutcomeGuard(
                    evidence,
                    "TERMINAL_SUCCESS",
                    "all_internal_tickets_terminal");
                return result;
            }
            catch (LMCSdoReadPollingTimeoutException error)
            {
                PreserveExternalD5Ticket(
                    error.Ticket,
                    ownerConnection,
                    slaveReference,
                    timeoutCycles,
                    stage);
                DisarmExternalD5SubmissionOutcomeGuard(
                    evidence,
                    "KNOWN_TICKET_PRESERVED",
                    "polling_timeout");
                throw;
            }
            catch (LMCSdoReadWaitCanceledException error)
            {
                PreserveExternalD5Ticket(
                    error.Ticket,
                    ownerConnection,
                    slaveReference,
                    timeoutCycles,
                    stage);
                DisarmExternalD5SubmissionOutcomeGuard(
                    evidence,
                    "KNOWN_TICKET_PRESERVED",
                    "wait_cancelled");
                throw;
            }
            catch (LMCSdoReadOperationException error)
            {
                DisarmExternalD5SubmissionOutcomeGuard(
                    evidence,
                    "TERMINAL_OPERATION_FAILURE",
                    error.OperationStatus.State
                        + "/"
                        + error.OperationStatus.Outcome);
                throw;
            }
            catch (LMCSdoReadCommandException error)
            {
                if (error.Stage == LMCSdoReadCommandStage.CapabilityPreflight
                    || error.Stage == LMCSdoReadCommandStage.Submission)
                {
                    DisarmExternalD5SubmissionOutcomeGuard(
                        evidence,
                        "PRE_TICKET_COMMAND_REJECTED",
                        error.Stage
                            + ":"
                            + (error.Response == null
                                ? "response_unavailable"
                                : error.Response.Detail.ToString()));
                }
                else if (error.Stage == LMCSdoReadCommandStage.StatusPolling
                    && error.Ticket != null)
                {
                    PreserveExternalD5Ticket(
                        error.Ticket,
                        ownerConnection,
                        slaveReference,
                        timeoutCycles,
                        stage);
                    DisarmExternalD5SubmissionOutcomeGuard(
                        evidence,
                        "KNOWN_TICKET_PRESERVED",
                        "status_command_failure:"
                            + (error.Response == null
                                ? "response_unavailable"
                                : error.Response.Detail.ToString()));
                }
                else
                {
                    PreserveExternalD5SubmissionOutcomeUncertain(
                        evidence,
                        error);
                }

                throw;
            }
            catch (Exception error)
            {
                PreserveExternalD5SubmissionOutcomeUncertain(
                    evidence,
                    error);
                throw;
            }
        }

        private void EnsureNoUnresolvedD5SdoQualificationTicket(
            string operation)
        {
            if (HasUnresolvedD5SdoQualificationTicket)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked while a D5 ticket or submission outcome is unresolved. Use Resolve D5 Quarantine; Stop, PowerOff, and existing-resource cleanup remain available.");
            }
        }

        private void SetD5SdoQualificationProgress(
            int progress,
            string summary)
        {
            SetQualificationProgress(progress, summary);
            ProgressD5SdoQualification.Value = qualificationProgress;
            TextD5SdoQualificationProgress.Text = summary;
            RefreshD5SdoQualificationOutput();
        }

        private void BeginExternalD5TrackingLog(string stage)
        {
            if (qualificationRunning)
            {
                throw new InvalidOperationException(
                    "External D5 tracking cannot start during a qualification run.");
            }

            if (d5ExternalTrackingRunId != null)
            {
                if (HasUnresolvedD5SdoQualificationTicket)
                {
                    return;
                }

                ResetExternalD5TrackingLogContext();
            }

            d5ExternalTrackingRunId = Guid.NewGuid().ToString("N");
            d5ExternalTrackingScenario = "D5ExternalTracking:"
                + (stage ?? "unknown");
            d5ExternalTrackingStep = 0;
            d5ExternalTrackingStopwatch = Stopwatch.StartNew();
            WriteExternalD5TrackingLog(
                "event=BEGIN",
                "stage=" + QualificationValue(stage ?? "unknown"));
        }

        private void WriteExternalD5TrackingLog(params string[] fields)
        {
            if (d5ExternalTrackingRunId == null)
            {
                d5ExternalTrackingRunId = Guid.NewGuid().ToString("N");
                d5ExternalTrackingScenario =
                    "D5ExternalTracking:recovered-context";
                d5ExternalTrackingStep = 0;
                d5ExternalTrackingStopwatch = Stopwatch.StartNew();
            }

            d5ExternalTrackingStep++;
            var elapsed = d5ExternalTrackingStopwatch == null
                ? 0L
                : d5ExternalTrackingStopwatch.ElapsedMilliseconds;
            var line = "QTEST|utc="
                + DateTime.UtcNow.ToString(
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    CultureInfo.InvariantCulture)
                + "|elapsedMs="
                + elapsed.ToString(CultureInfo.InvariantCulture)
                + "|run="
                + d5ExternalTrackingRunId
                + "|scenario="
                + QualificationValue(
                    d5ExternalTrackingScenario
                        ?? "D5ExternalTracking:unknown")
                + "|step="
                + d5ExternalTrackingStep.ToString(
                    CultureInfo.InvariantCulture);
            if (fields != null && fields.Length > 0)
            {
                line += "|" + string.Join("|", fields);
            }

            qualificationLogLines.Add(line);
            WriteLog(line);
            RefreshQualificationSummary();
            RefreshD5SdoQualificationOutput();
        }

        private void CloseExternalD5TrackingLogIfResolved(
            string resolution)
        {
            if (d5ExternalTrackingRunId == null
                || HasUnresolvedD5SdoQualificationTicket)
            {
                return;
            }

            WriteExternalD5TrackingLog(
                "event=END",
                "resolution=" + QualificationValue(
                    resolution ?? "resolved"),
                "verdict=RESOLVED");
            ResetExternalD5TrackingLogContext();
        }

        private void ResetExternalD5TrackingLogContext()
        {
            if (d5ExternalTrackingStopwatch != null)
            {
                d5ExternalTrackingStopwatch.Stop();
            }

            d5ExternalTrackingRunId = null;
            d5ExternalTrackingScenario = null;
            d5ExternalTrackingStep = 0;
            d5ExternalTrackingStopwatch = null;
        }

        private void WriteD5SdoQualificationLog(params string[] fields)
        {
            WriteQualificationLog(fields);
            RefreshD5SdoQualificationOutput();
        }

        private void RefreshD5SdoQualificationOutput()
        {
            if (ProgressD5SdoQualification == null)
            {
                return;
            }

            ProgressD5SdoQualification.Value = qualificationProgress;
            if (!qualificationRunning && TextQualificationProgress != null)
            {
                TextD5SdoQualificationProgress.Text =
                    TextQualificationProgress.Text;
            }

            var start = Math.Max(0, qualificationLogLines.Count - 12);
            TextD5SdoQualificationSummary.Text =
                qualificationLogLines.Count == 0
                    ? "Structured D5 QTEST results will appear here."
                    : string.Join(
                        Environment.NewLine,
                        qualificationLogLines.Skip(start));
        }

        private sealed class D5SdoQualificationOrphanEvidence
        {
            internal D5SdoQualificationOrphanEvidence(
                uint ticketId,
                uint diagnosticsBootId,
                ushort slaveReference,
                uint timeoutCycles,
                LMCConnection ownerConnection,
                string stage,
                string reason,
                string evidenceId)
            {
                TicketId = ticketId;
                DiagnosticsBootId = diagnosticsBootId;
                SlaveReference = slaveReference;
                TimeoutCycles = timeoutCycles;
                OwnerConnection = ownerConnection;
                Stage = stage;
                Reason = reason;
                EvidenceId = evidenceId;
            }

            internal uint TicketId { get; private set; }
            internal uint DiagnosticsBootId { get; private set; }
            internal ushort SlaveReference { get; private set; }
            internal uint TimeoutCycles { get; private set; }
            internal LMCConnection OwnerConnection { get; private set; }
            internal string Stage { get; private set; }
            internal string Reason { get; private set; }
            internal string EvidenceId { get; private set; }
        }

        private sealed class D5SdoQualificationInput
        {
            internal D5SdoQualificationInput(
                ushort slaveReference,
                ushort objectIndex,
                byte subIndex,
                LMCSignalValueType valueType,
                ushort dataLength,
                uint timeoutCycles,
                LMCSdoRequest abortRequest)
            {
                SlaveReference = slaveReference;
                ObjectIndex = objectIndex;
                SubIndex = subIndex;
                ValueType = valueType;
                DataLength = dataLength;
                TimeoutCycles = timeoutCycles;
                AbortRequest = abortRequest;
                AxisObjectName = "_LMCAxis"
                    + slaveReference.ToString(CultureInfo.InvariantCulture);
            }

            internal ushort SlaveReference { get; private set; }
            internal ushort ObjectIndex { get; private set; }
            internal byte SubIndex { get; private set; }
            internal LMCSignalValueType ValueType { get; private set; }
            internal ushort DataLength { get; private set; }
            internal uint TimeoutCycles { get; private set; }
            internal LMCSdoRequest AbortRequest { get; private set; }
            internal string AxisObjectName { get; private set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LasalMotionControlLib;
using Microsoft.Win32;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly List<EtherCATTopologyNodeRow> etherCATTopologyRows =
            new List<EtherCATTopologyNodeRow>();
        private readonly TopologyIoLiveMonitorPolicy topologyIoLiveMonitorPolicy =
            new TopologyIoLiveMonitorPolicy();
        private readonly TopologyIoLiveEvidenceJournal
            topologyIoLiveEvidenceJournal =
                new TopologyIoLiveEvidenceJournal();

        private LMCEtherCATTopology etherCATTopology;
        private int configuredCrevisEntryCount;
        private string etherCATTopologyLoadOrigin;
        private ConfiguredTopologySnapshot
            lastSuccessfulConfiguredTopologySnapshot;
        private ConfiguredTopologyComparison latestConfiguredTopologyComparison;
        private string latestConfiguredTopologyEvidence;
        private DispatcherTimer topologyIoLiveMonitorTimer;
        private bool topologyIoLiveMonitorClosed;
        private LMCDigitalIOValue selectedDigitalOutputShadow;
        private LMCDigitalOutputWriteRequest pendingDigitalOutputWriteRequest;
        private LMCEtherCATTopologyEntry pendingDigitalOutputWriteEntry;
        private LMCDigitalIOValue pendingDigitalOutputWriteOriginalShadow;
        private LMCOperationTicket pendingDigitalOutputWriteTicket;
        private LMCOperationTicket resolvedDigitalOutputWriteTicket;
        private string resolvedDigitalOutputWriteSummary;
        private bool digitalOutputWriteTerminalSucceeded;
        private readonly DigitalOutputUncertainAcknowledgementState
            digitalOutputUncertainAcknowledgementState =
                new DigitalOutputUncertainAcknowledgementState();

        private bool digitalOutputWriteOutcomeUncertain
        {
            get
            {
                return digitalOutputUncertainAcknowledgementState
                    .OutcomeUncertain;
            }
        }

        private bool HasUnresolvedDigitalOutputWrite
        {
            get
            {
                return pendingDigitalOutputWriteRequest != null
                    || pendingDigitalOutputWriteTicket != null
                    || digitalOutputWriteOutcomeUncertain;
            }
        }

        private bool HasUnresolvedDiagnosticMutation
        {
            get
            {
                return HasUnresolvedD5SdoQualificationTicket
                    || HasUnresolvedDigitalOutputWrite
                    || HasActiveDiagnosticsMutationJournalRecord
                    || HasActiveRecorderDoubleRecoveryJournalRecord;
            }
        }

        private bool HasDiagnosticsMutationCommandInterlock
        {
            get
            {
                return !EvaluateDiagnosticsAdmissionIgnoringPendingGroupReset(
                        DiagnosticsAdmissionOperation.NewLiveOrMutation)
                    .IsAllowed;
            }
        }

        private DiagnosticsAdmissionDecision EvaluateDiagnosticsAdmission(
            DiagnosticsAdmissionOperation operation,
            bool operationSlotAvailable = true)
        {
            return EvaluateDiagnosticsAdmissionCore(
                operation,
                operationSlotAvailable,
                false);
        }

        private DiagnosticsAdmissionDecision
            EvaluateDiagnosticsAdmissionIgnoringPendingGroupReset(
                DiagnosticsAdmissionOperation operation,
                bool operationSlotAvailable = true)
        {
            return EvaluateDiagnosticsAdmissionCore(
                operation,
                operationSlotAvailable,
                true);
        }

        private DiagnosticsAdmissionDecision EvaluateDiagnosticsAdmissionCore(
            DiagnosticsAdmissionOperation operation,
            bool operationSlotAvailable,
            bool allowPendingGroupReset)
        {
            if (!allowPendingGroupReset
                && HasUnresolvedGroupResetState()
                && (operation
                        == DiagnosticsAdmissionOperation.NewLiveOrMutation
                    || operation
                        == DiagnosticsAdmissionOperation.TrackedD5Submit
                    || (operation
                            == DiagnosticsAdmissionOperation.ConnectOrReconnect
                        && !GroupResetRecoveryReconnectAvailable)
                    || operation
                        == DiagnosticsAdmissionOperation.CloseConnection
                    || operation
                        == DiagnosticsAdmissionOperation.CloseWindow))
            {
                return DiagnosticsAdmissionDecision.Deny(
                    DiagnosticsAdmissionDenialReason.UnresolvedMutation);
            }

            var currentConnection = connection;
            var pendingReadback = d5SdoPendingWriteReadback;
            return DiagnosticsOperationAdmissionPolicy.Evaluate(
                operation,
                new DiagnosticsAdmissionState(
                    HasUnresolvedDiagnosticMutation,
                    AnyDiagnosticsMutationJournalUnavailable,
                    currentConnection != null
                        && currentConnection.IsConnected,
                    operationSlotAvailable,
                    pendingReadback != null,
                    pendingReadback != null
                        && pendingReadback.MatchesOwnerCurrentSession(
                            currentConnection),
                    HasD5SdoTicketOrQuarantine,
                    HasUnresolvedDigitalOutputWrite,
                    HasUnresolvedAxisPowerState()
                        || (HasUnresolvedAxisQualificationState()
                            && !IsCurrentAxisQualificationMutationScope()),
                    HasUnresolvedGroupPowerState(),
                    AxisPowerOnRecoveryJournalUnavailable
                        || AxisQualificationRecoveryJournalUnavailable
                        || GroupPowerRecoveryJournalUnavailable
                        || GroupResetRecoveryJournalUnavailable,
                    IsRecoveryIdentityReadOnlyExitPermitted()));
        }

        private InvalidOperationException CreateDiagnosticsAdmissionException(
            string operation,
            DiagnosticsAdmissionDecision decision)
        {
            if (decision == null)
            {
                throw new ArgumentNullException(nameof(decision));
            }

            switch (decision.DenialReason)
            {
                case DiagnosticsAdmissionDenialReason.UnresolvedMutation:
                    return new InvalidOperationException(
                        operation
                        + " is blocked while diagnostics mutation or durable recovery evidence is unresolved. "
                        + GetUnresolvedDiagnosticMutationGuidance());

                case DiagnosticsAdmissionDenialReason
                    .MutationJournalUnavailable:
                    return new InvalidOperationException(
                        operation
                        + " is blocked. "
                        + GetAnyDiagnosticsMutationJournalUnavailableGuidance());

                case DiagnosticsAdmissionDenialReason.OperationSlotOccupied:
                    return new InvalidOperationException(
                        operation
                        + " is blocked while another D5 operation ticket is non-terminal.");

                case DiagnosticsAdmissionDenialReason
                    .ExactReadbackNotPending:
                    return new InvalidOperationException(
                        operation
                        + " is blocked because no exact SDO Write readback is pending.");

                case DiagnosticsAdmissionDenialReason
                    .D5TicketOrQuarantineUnresolved:
                    return new InvalidOperationException(
                        operation
                        + " cannot start while another D5 ticket or quarantine entry is unresolved.");

                case DiagnosticsAdmissionDenialReason
                    .DigitalOutputWriteUnresolved:
                    return new InvalidOperationException(
                        operation
                        + " cannot start while a digital output Write ticket or exact shadow readback is unresolved. "
                        + GetUnresolvedDiagnosticMutationGuidance());

                case DiagnosticsAdmissionDenialReason
                    .ExactReadbackSessionMismatch:
                    return new InvalidOperationException(
                        operation
                        + " belongs to another or stale connection session. No readback was submitted and the interlock remains active.");

                case DiagnosticsAdmissionDenialReason
                    .ExternalDisconnectRequired:
                    return new InvalidOperationException(
                        operation
                        + " is blocked while diagnostics mutation or durable recovery evidence is unresolved. "
                        + GetUnresolvedDiagnosticMutationGuidance()
                        + " Reconnect is allowed only after an external connection loss.");

                case DiagnosticsAdmissionDenialReason
                    .AxisPowerOnUnresolved:
                    return new InvalidOperationException(
                        operation
                        + " is blocked while Axis Power recovery is unresolved. "
                        + GetAxisPowerOnRecoveryGuidance());

                case DiagnosticsAdmissionDenialReason
                    .GroupPowerUnresolved:
                    return new InvalidOperationException(
                        operation
                        + " is blocked while Group Power recovery is unresolved. "
                        + GetGroupPowerRecoveryGuidance());

                case DiagnosticsAdmissionDenialReason
                    .PowerRecoveryJournalUnavailable:
                    return new InvalidOperationException(
                        operation
                        + " is blocked because a durable Axis/Group control "
                        + "recovery journal is unavailable. Axis Stop, explicit "
                        + "Axis Power Off, Group Stop, read-only inspection, "
                        + "cleanup, reconnect, and Close remain available. "
                        + GetAnyDiagnosticsMutationJournalUnavailableGuidance());

                case DiagnosticsAdmissionDenialReason
                    .RecoveryIdentityReadOnly:
                    return new InvalidOperationException(
                        operation
                        + " is blocked because this connection is in recovery-identity read-only quarantine. "
                        + GetRecoveryIdentityReadOnlyGuidance());

                case DiagnosticsAdmissionDenialReason
                    .StaleRecoveryRetirementUnavailable:
                    return new InvalidOperationException(
                        operation
                        + " is available only on a connected recovery-identity "
                        + "read-only quarantine with an idle operation slot.");

                default:
                    return new InvalidOperationException(
                        operation
                        + " is blocked by diagnostics admission policy: "
                        + decision.DenialReason
                        + ".");
            }
        }

        private string GetUnresolvedDiagnosticMutationGuidance()
        {
            string guidance;
            if (HasUnresolvedD5SdoQualificationTicket
                && HasUnresolvedDigitalOutputWrite)
            {
                guidance = GetD5SdoResolutionGuidance()
                    + " The digital output write also requires a terminal ticket plus exact shadow reread, or physical verification and explicit acknowledgement.";
            }
            else if (HasUnresolvedD5SdoQualificationTicket)
            {
                guidance = GetD5SdoResolutionGuidance();
            }
            else if (HasUnresolvedDigitalOutputWrite)
            {
                guidance = "Refresh or cancel the digital output ticket. A successful terminal must be followed by an exact output-shadow reread. If the session was lost, physically verify the output before using Acknowledge Unverified Outcome; never replay automatically.";
            }
            else
            {
                guidance = string.Empty;
            }

            var durableGuidance =
                GetDiagnosticsMutationJournalGuidance();
            if (!string.IsNullOrEmpty(durableGuidance))
            {
                guidance = string.IsNullOrEmpty(guidance)
                    ? durableGuidance
                    : guidance + " " + durableGuidance;
            }

            var recorderDoubleGuidance =
                GetRecorderDoubleRecoveryJournalGuidance();
            if (!string.IsNullOrEmpty(recorderDoubleGuidance))
            {
                guidance = string.IsNullOrEmpty(guidance)
                    ? recorderDoubleGuidance
                    : guidance + " " + recorderDoubleGuidance;
            }

            return guidance;
        }

        private void EnsureNoUnresolvedDiagnosticMutation(string operation)
        {
            EnsureDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.NewLiveOrMutation,
                operation);
        }

        private void
            EnsureNoUnresolvedDiagnosticMutationIgnoringPendingGroupReset(
                string operation)
        {
            var decision =
                EvaluateDiagnosticsAdmissionIgnoringPendingGroupReset(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation);
            if (!decision.IsAllowed)
            {
                throw CreateDiagnosticsAdmissionException(
                    operation,
                    decision);
            }
        }

        private void EnsureDiagnosticsAdmission(
            DiagnosticsAdmissionOperation admissionOperation,
            string operation,
            bool operationSlotAvailable = true)
        {
            var decision = EvaluateDiagnosticsAdmission(
                admissionOperation,
                operationSlotAvailable);
            if (!decision.IsAllowed)
            {
                throw CreateDiagnosticsAdmissionException(
                    operation,
                    decision);
            }
        }

        private void MarkDigitalOutputWriteConnectionLost()
        {
            if (!HasUnresolvedDigitalOutputWrite)
            {
                return;
            }

            SetDigitalOutputWriteOutcomeUncertain(true);
            if (HasActiveDiagnosticsMutationJournalRecord
                && diagnosticsMutationJournal.CurrentRecord.Kind
                    == DiagnosticsMutationKind.DigitalOutputWrite)
            {
                MarkDigitalOutputMutationOutcomeUnverified();
            }
            if (CheckConfirmDigitalOutputWrite != null)
            {
                CheckConfirmDigitalOutputWrite.IsChecked = false;
            }

            if (TextDigitalOutputWriteStatus != null)
            {
                TextDigitalOutputWriteStatus.Text =
                    "OUTPUT WRITE CONNECTION LOST. The ticket outcome is unverified and same-session status recovery is unavailable. Automatic replay is blocked; physically verify the output before explicit acknowledgement.";
            }
        }

        private void ClearPendingDigitalOutputWriteSubmission()
        {
            pendingDigitalOutputWriteRequest = null;
            pendingDigitalOutputWriteEntry = null;
            pendingDigitalOutputWriteOriginalShadow = null;
            pendingDigitalOutputWriteTicket = null;
            digitalOutputWriteTerminalSucceeded = false;
            SetDigitalOutputWriteOutcomeUncertain(false);
        }

        private void SetDigitalOutputWriteOutcomeUncertain(bool value)
        {
            digitalOutputUncertainAcknowledgementState
                .SetOutcomeUncertain(value);
            ResetDigitalOutputWritePhysicalVerification();
        }

        private void ResetDigitalOutputWritePhysicalVerification()
        {
            digitalOutputUncertainAcknowledgementState
                .InvalidatePhysicalVerification();
            if (CheckDigitalOutputWritePhysicallyVerified != null)
            {
                CheckDigitalOutputWritePhysicallyVerified.IsChecked = false;
            }
        }

        private bool CanAcknowledgeDigitalOutputWriteUncertain()
        {
            var idle = !operationRunning
                && !safetyCommandRunning
                && safetyMonitorCount == 0
                && !qualificationRunning;
            return CanAcknowledgeDigitalOutputWriteUncertain(idle);
        }

        private bool CanAcknowledgeDigitalOutputWriteUncertain(bool idle)
        {
            return EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation.ExistingResourceCleanup)
                    .IsAllowed
                && digitalOutputUncertainAcknowledgementState
                    .CanAcknowledge(idle)
                && CheckDigitalOutputWritePhysicallyVerified != null
                && CheckDigitalOutputWritePhysicallyVerified.IsChecked == true;
        }

        private void InitializeTopologyIoUi()
        {
            GridEtherCATTopology.ItemsSource = etherCATTopologyRows;
            UpdateSelectedTopologyNodeText(null);
            ResetSelectedDigitalOutputShadow();
            topologyIoLiveMonitorTimer = new DispatcherTimer(
                DispatcherPriority.Background,
                Dispatcher);
            topologyIoLiveMonitorTimer.Interval = TimeSpan.FromMilliseconds(
                TopologyIoLiveMonitorPolicy.BoundIntervalMilliseconds(
                    TopologyIoLiveMonitorPolicy.DefaultIntervalMilliseconds));
            topologyIoLiveMonitorTimer.Tick +=
                TopologyIoLiveMonitorTimer_Tick;
            topologyIoLiveMonitorTimer.Start();
            UpdateTopologyIoLiveMonitorStatus(
                "waiting for a connected topology and live-read capabilities");
        }

        private async void TopologyIoLiveMonitorTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (topologyIoLiveMonitorClosed)
            {
                return;
            }

            // Never wait behind a foreground or safety send. One monitor
            // sample owns the shared send gate and performs one SDK read only.
            if (!await commandSendGate.WaitAsync(0))
            {
                UpdateTopologyIoLiveMonitorStatus(
                    "paused while a foreground or safety send owns the command gate");
                return;
            }

            TopologyIoLiveMonitorLease lease = null;
            var leaseCompleted = false;
            var liveReadAttempted = false;
            var capturedConnection = connection;
            var capturedTopology = etherCATTopology;
            var capturedCapabilities = diagnosticCapabilities;
            var capturedTopologyLoadOrigin = etherCATTopologyLoadOrigin;
            var capturedSelection = GridEtherCATTopology == null
                ? null
                : GridEtherCATTopology.SelectedItem
                    as EtherCATTopologyNodeRow;
            var capturedSafetyGeneration = safetyRequestGeneration;

            try
            {
                var request = CreateTopologyIoLiveMonitorRequest(
                    capturedConnection,
                    capturedTopology,
                    capturedCapabilities,
                    capturedSelection);
                TopologyIoLiveMonitorSkipReason skipReason;
                if (!topologyIoLiveMonitorPolicy.TryBegin(
                        request,
                        out lease,
                        out skipReason))
                {
                    UpdateTopologyIoLiveMonitorSkipStatus(skipReason);
                    return;
                }

                if (!CanStartTopologyIoLiveMonitorSend(
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedSafetyGeneration))
                {
                    topologyIoLiveMonitorPolicy.CompleteCancellation(lease);
                    leaseCompleted = true;
                    UpdateTopologyIoLiveMonitorStatus(
                        "paused because foreground, safety, or qualification work became active");
                    return;
                }

                if (lease.ReadsHealth)
                {
                    var row = FindEtherCATTopologyRow(lease.HealthNodeId);
                    if (row == null)
                    {
                        throw new InvalidOperationException(
                            "The scheduled live-health node is no longer in the configured topology.");
                    }

                    LMCEtherCATNodeHealth health;
                    using (sendPriorityCoordinator.BeginPreemptibleScope(
                        capturedSafetyGeneration,
                        "CREVIS live node-health monitor"))
                    {
                        liveReadAttempted = true;
                        health = await capturedConnection.Diagnostics
                            .ReadEtherCATNodeHealthAsync(
                                lease.HealthNodeId,
                                capturedTopology,
                                capturedCapabilities,
                                CancellationToken.None);
                    }
                    if (!CanCommitTopologyIoLiveMonitorHealth(
                            lease,
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedSafetyGeneration,
                            row,
                            health))
                    {
                        topologyIoLiveMonitorPolicy.CompleteCancellation(lease);
                        leaseCompleted = true;
                        return;
                    }

                    TryRecordTopologyIoHealthSuccess(
                        TopologyIoLiveEvidenceOrigin.Auto,
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedTopologyLoadOrigin,
                        row.Entry,
                        health);
                    row.ApplyHealth(health);
                    if (ReferenceEquals(
                            GridEtherCATTopology.SelectedItem,
                            row))
                    {
                        TextSelectedNodeHealth.Text =
                            FormatEtherCATNodeHealth(row.Entry, health);
                    }
                    UpdateTopologyIoLiveMonitorStatus(
                        "sampled health for "
                        + row.Entry.Name
                        + " at cycle "
                        + health.CycleCounter.ToString(
                            CultureInfo.InvariantCulture));
                }
                else if (lease.ReadsSelectedInput)
                {
                    var row = capturedSelection;
                    if (row == null)
                    {
                        throw new InvalidOperationException(
                            "The scheduled digital-input selection is no longer available.");
                    }

                    var inputRequest = new LMCDigitalIOReadRequest(
                        lease.TopologyRevision,
                        lease.SelectedInputReference,
                        LMCDigitalIODirection.Input,
                        lease.SelectedInputWidth);
                    LMCDigitalIOValue value;
                    using (sendPriorityCoordinator.BeginPreemptibleScope(
                        capturedSafetyGeneration,
                        "CREVIS live digital-input monitor"))
                    {
                        liveReadAttempted = true;
                        value = await capturedConnection.Diagnostics
                            .ReadDigitalIOAsync(
                                capturedTopology,
                                inputRequest,
                                capturedCapabilities,
                                CancellationToken.None);
                    }
                    if (!CanCommitTopologyIoLiveMonitorInput(
                            lease,
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedSafetyGeneration,
                            row,
                            value))
                    {
                        topologyIoLiveMonitorPolicy.CompleteCancellation(lease);
                        leaseCompleted = true;
                        return;
                    }

                    TryRecordTopologyIoDigitalInputSuccess(
                        TopologyIoLiveEvidenceOrigin.Auto,
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedTopologyLoadOrigin,
                        row.Entry,
                        inputRequest,
                        value);
                    row.ApplyDigitalInput(value);
                    if (!HasSelectedDigitalOutputShadowEvidence(row))
                    {
                        TextSelectedDigitalIO.Text =
                            FormatDigitalIOValue(row.Entry, value);
                    }
                    UpdateTopologyIoLiveMonitorStatus(
                        "sampled selected input "
                        + row.Entry.Name
                        + " at cycle "
                        + value.CycleCounter.ToString(
                            CultureInfo.InvariantCulture));
                }

                topologyIoLiveMonitorPolicy.CompleteSuccess(
                    lease,
                    DateTime.UtcNow);
                leaseCompleted = true;
            }
            catch (Exception error)
            {
                if (lease == null)
                {
                    return;
                }

                // A selection/topology/session invalidation can happen while
                // the SDK request is in flight. Discard that exception before
                // it can mutate the replacement row, monitor status, backoff,
                // or log stream.
                if (!topologyIoLiveMonitorPolicy.CanProcessFailure(lease))
                {
                    topologyIoLiveMonitorPolicy.CompleteCancellation(lease);
                    leaseCompleted = true;
                    return;
                }

                if (!IsCurrentTopologyIoLiveMonitorCapture(
                        lease,
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedSafetyGeneration))
                {
                    topologyIoLiveMonitorPolicy.CompleteCancellation(lease);
                    leaseCompleted = true;
                    return;
                }

                if (liveReadAttempted)
                {
                    var failureRow = lease.ReadsHealth
                        ? FindEtherCATTopologyRow(lease.HealthNodeId)
                        : capturedSelection;
                    if (failureRow != null)
                    {
                        var failureRequest = lease.ReadsSelectedInput
                            ? new LMCDigitalIOReadRequest(
                                lease.TopologyRevision,
                                lease.SelectedInputReference,
                                LMCDigitalIODirection.Input,
                                lease.SelectedInputWidth)
                            : null;
                        TryRecordTopologyIoReadFailure(
                            TopologyIoLiveEvidenceOrigin.Auto,
                            lease.ReadsHealth
                                ? TopologyIoLiveEvidenceKind.Health
                                : TopologyIoLiveEvidenceKind.DI,
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedTopologyLoadOrigin,
                            failureRow.Entry,
                            failureRequest,
                            error);
                    }
                }

                var signature = error.GetType().FullName + ":" + error.Message;
                var report = topologyIoLiveMonitorPolicy.CompleteFailure(
                    lease,
                    DateTime.UtcNow,
                    signature);
                leaseCompleted = true;
                ApplyTopologyIoLiveMonitorFailure(
                    lease,
                    error.GetType().Name + ": " + error.Message);
                UpdateTopologyIoLiveMonitorStatus(
                    "read failed; bounded retry backoff is active: "
                    + error.Message);
                if (report)
                {
                    WriteLog(
                        "CREVIS live monitor read failed; repeated identical errors are suppressed during backoff: "
                        + error.Message);
                }
            }
            finally
            {
                if (lease != null && !leaseCompleted)
                {
                    topologyIoLiveMonitorPolicy.CompleteCancellation(lease);
                }

                commandSendGate.Release();
            }
        }

        private TopologyIoLiveMonitorRequest
            CreateTopologyIoLiveMonitorRequest(
                LMCConnection capturedConnection,
                LMCEtherCATTopology capturedTopology,
                LMCDiagnosticCapabilities capturedCapabilities,
                EtherCATTopologyNodeRow capturedSelection)
        {
            var healthNodeIds = new List<uint>(etherCATTopologyRows.Count);
            foreach (var row in etherCATTopologyRows)
            {
                healthNodeIds.Add(row.Entry.NodeId);
            }

            var selectedInputAvailable = capturedSelection != null
                && CanReadDigitalIo(
                    capturedSelection.Entry,
                    LMCDigitalIODirection.Input);
            return new TopologyIoLiveMonitorRequest(
                CheckAutoRefreshTopologyIoLive != null
                    && CheckAutoRefreshTopologyIoLive.IsChecked == true
                    && !topologyIoLiveMonitorClosed,
                capturedConnection != null && capturedConnection.IsConnected,
                IsTopologyIoLiveMonitorBusy(),
                capturedCapabilities != null
                    && capturedCapabilities.Supports(
                        LMCDiagnosticCapability.EtherCATTopology),
                capturedCapabilities != null
                    && capturedCapabilities.Supports(
                        LMCDiagnosticCapability.EtherCATNodeHealth),
                capturedCapabilities != null
                    && capturedCapabilities.Supports(
                        LMCDiagnosticCapability.DigitalIORead),
                capturedTopology == null
                    ? 0
                    : capturedTopology.TopologyRevision,
                healthNodeIds,
                selectedInputAvailable
                    ? capturedSelection.Entry.NodeId
                    : 0,
                selectedInputAvailable
                    ? capturedSelection.Entry.IOReference
                    : 0,
                selectedInputAvailable
                    ? GetDigitalIoBitWidth(
                        capturedSelection.Entry,
                        LMCDigitalIODirection.Input)
                    : (byte)0,
                DateTime.UtcNow);
        }

        private bool IsTopologyIoLiveMonitorBusy()
        {
            return operationRunning
                || connectionTransitionRunning
                || safetyCommandRunning
                || safetyMonitorCount > 0
                || qualificationRunning;
        }

        private bool CanStartTopologyIoLiveMonitorSend(
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            long capturedSafetyGeneration)
        {
            return !topologyIoLiveMonitorClosed
                && !IsTopologyIoLiveMonitorBusy()
                && capturedSafetyGeneration == safetyRequestGeneration
                && ReferenceEquals(connection, capturedConnection)
                && capturedConnection != null
                && capturedConnection.IsConnected
                && ReferenceEquals(etherCATTopology, capturedTopology)
                && capturedTopology != null
                && ReferenceEquals(
                    diagnosticCapabilities,
                    capturedCapabilities)
                && capturedCapabilities != null;
        }

        private bool IsCurrentTopologyIoLiveMonitorCapture(
            TopologyIoLiveMonitorLease lease,
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            long capturedSafetyGeneration)
        {
            return lease != null
                && !topologyIoLiveMonitorClosed
                && capturedSafetyGeneration == safetyRequestGeneration
                && !safetyCommandRunning
                && ReferenceEquals(connection, capturedConnection)
                && capturedConnection != null
                && capturedConnection.IsConnected
                && ReferenceEquals(etherCATTopology, capturedTopology)
                && capturedTopology != null
                && ReferenceEquals(
                    diagnosticCapabilities,
                    capturedCapabilities)
                && capturedCapabilities != null
                && capturedTopology.TopologyRevision
                    == lease.TopologyRevision;
        }

        private bool CanCommitTopologyIoLiveMonitorHealth(
            TopologyIoLiveMonitorLease lease,
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            long capturedSafetyGeneration,
            EtherCATTopologyNodeRow row,
            LMCEtherCATNodeHealth health)
        {
            return health != null
                && IsCurrentTopologyIoLiveMonitorCapture(
                    lease,
                    capturedConnection,
                    capturedTopology,
                    capturedCapabilities,
                    capturedSafetyGeneration)
                && ContainsEtherCATTopologyRow(row)
                && row.Entry.NodeId == lease.HealthNodeId
                && topologyIoLiveMonitorPolicy.CanCommitHealth(
                    lease,
                    health.TopologyRevision,
                    health.NodeId);
        }

        private bool CanCommitTopologyIoLiveMonitorInput(
            TopologyIoLiveMonitorLease lease,
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            long capturedSafetyGeneration,
            EtherCATTopologyNodeRow row,
            LMCDigitalIOValue value)
        {
            return value != null
                && IsCurrentTopologyIoLiveMonitorCapture(
                    lease,
                    capturedConnection,
                    capturedTopology,
                    capturedCapabilities,
                    capturedSafetyGeneration)
                && ContainsEtherCATTopologyRow(row)
                && ReferenceEquals(GridEtherCATTopology.SelectedItem, row)
                && row.Entry.NodeId == lease.SelectedInputNodeId
                && topologyIoLiveMonitorPolicy.CanCommitSelectedInput(
                    lease,
                    value.TopologyRevision,
                    value.NodeId,
                    value.IOReference,
                    value.BitWidth);
        }

        private EtherCATTopologyNodeRow FindEtherCATTopologyRow(uint nodeId)
        {
            foreach (var row in etherCATTopologyRows)
            {
                if (row.Entry.NodeId == nodeId)
                {
                    return row;
                }
            }

            return null;
        }

        private bool ContainsEtherCATTopologyRow(EtherCATTopologyNodeRow row)
        {
            foreach (var current in etherCATTopologyRows)
            {
                if (ReferenceEquals(current, row))
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyTopologyIoLiveMonitorFailure(
            TopologyIoLiveMonitorLease lease,
            string message)
        {
            if (lease.ReadsHealth)
            {
                var row = FindEtherCATTopologyRow(lease.HealthNodeId);
                if (row != null)
                {
                    row.ApplyHealthReadError(message);
                    if (ReferenceEquals(
                            GridEtherCATTopology.SelectedItem,
                            row))
                    {
                        TextSelectedNodeHealth.Text =
                            FormatTopologyIoReadFailure(
                                row.Entry,
                                "NODE HEALTH",
                                message);
                    }
                }
            }
            else if (lease.ReadsSelectedInput)
            {
                var row = FindEtherCATTopologyRow(lease.SelectedInputNodeId);
                if (row != null)
                {
                    row.ApplyDigitalInputReadError(message);
                    if (ReferenceEquals(
                            GridEtherCATTopology.SelectedItem,
                            row)
                        && !HasSelectedDigitalOutputShadowEvidence(row))
                    {
                        TextSelectedDigitalIO.Text =
                            FormatTopologyIoReadFailure(
                                row.Entry,
                                "DIGITAL INPUT",
                                message);
                    }
                }
            }
        }

        private void UpdateTopologyIoLiveMonitorSkipStatus(
            TopologyIoLiveMonitorSkipReason reason)
        {
            switch (reason)
            {
                case TopologyIoLiveMonitorSkipReason.Disabled:
                    UpdateTopologyIoLiveMonitorStatus("disabled by the user");
                    break;
                case TopologyIoLiveMonitorSkipReason.Disconnected:
                    UpdateTopologyIoLiveMonitorStatus("waiting for connection");
                    break;
                case TopologyIoLiveMonitorSkipReason.Busy:
                    UpdateTopologyIoLiveMonitorStatus(
                        "paused for foreground, safety, or qualification work");
                    break;
                case TopologyIoLiveMonitorSkipReason.MissingTopology:
                    UpdateTopologyIoLiveMonitorStatus(
                        "waiting for configured topology bit 14 and a successful load");
                    break;
                case TopologyIoLiveMonitorSkipReason.MissingCapabilities:
                    UpdateTopologyIoLiveMonitorStatus(
                        "idle; PLC bit 15/16 is off, so live-read wire traffic is zero");
                    break;
                case TopologyIoLiveMonitorSkipReason.InFlight:
                    UpdateTopologyIoLiveMonitorStatus(
                        "one bounded read is already in flight");
                    break;
                case TopologyIoLiveMonitorSkipReason.Backoff:
                    UpdateTopologyIoLiveMonitorStatus(
                        "bounded retry backoff after the last read failure");
                    break;
            }
        }

        private void UpdateTopologyIoLiveMonitorStatus(string status)
        {
            if (TextTopologyIoLiveMonitorStatus == null)
            {
                return;
            }

            var text = "Auto live monitor: " + status + ". Configured columns remain static.";
            if (!string.Equals(
                    TextTopologyIoLiveMonitorStatus.Text,
                    text,
                    StringComparison.Ordinal))
            {
                TextTopologyIoLiveMonitorStatus.Text = text;
            }
        }

        private void InvalidateTopologyIoLiveMonitorSession()
        {
            topologyIoLiveMonitorPolicy.InvalidateSession();
        }

        private void InvalidateTopologyIoLiveMonitorTopology()
        {
            topologyIoLiveMonitorPolicy.InvalidateTopology();
        }

        private void InvalidateTopologyIoLiveMonitorSelection()
        {
            topologyIoLiveMonitorPolicy.InvalidateSelection();
        }

        private void CheckAutoRefreshTopologyIoLive_Changed(
            object sender,
            RoutedEventArgs e)
        {
            InvalidateTopologyIoLiveMonitorSelection();
            UpdateTopologyIoLiveMonitorStatus(
                CheckAutoRefreshTopologyIoLive != null
                    && CheckAutoRefreshTopologyIoLive.IsChecked == true
                    ? "enabled; waiting for the next eligible read slot"
                    : "disabled by the user");
        }

        private void Window_TopologyIoClosed(object sender, EventArgs e)
        {
            topologyIoLiveMonitorClosed = true;
            InvalidateTopologyIoLiveMonitorSession();
            if (topologyIoLiveMonitorTimer != null)
            {
                topologyIoLiveMonitorTimer.Stop();
                topologyIoLiveMonitorTimer.Tick -=
                    TopologyIoLiveMonitorTimer_Tick;
                topologyIoLiveMonitorTimer = null;
            }
        }

        private async void ButtonLoadEtherCATTopology_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Load EtherCAT Topology",
                () => LoadEtherCATTopologyAsync(
                    RequireConnection(),
                    true,
                    "manual reload"));
        }

        private async void ButtonSaveConfiguredTopologyEvidence_Click(
            object sender,
            RoutedEventArgs e)
        {
            var evidence = latestConfiguredTopologyEvidence;
            if (string.IsNullOrWhiteSpace(evidence))
            {
                WriteLog(
                    "Configured topology evidence requires a successful topology load first.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".txt",
                Filter = TranslateUiText(
                    "Text files (*.txt)|*.txt|All files (*.*)|*.*"),
                FileName = "configured-ethercat-topology-"
                    + DateTime.Now.ToString(
                        "yyyyMMdd-HHmmss",
                        CultureInfo.InvariantCulture)
                    + ".txt",
                OverwritePrompt = true,
                Title = TranslateUiText(
                    "Save Configured EtherCAT Topology Evidence")
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            await RunOperationAsync(
                "Save Configured EtherCAT Topology Evidence",
                async () =>
                {
                    await System.Threading.Tasks.Task.Run(
                        () => ConfiguredTopologyComparison.SaveEvidence(
                            dialog.FileName,
                            evidence));
                    TextOperationState.Text =
                        "Configured topology evidence saved";
                    WriteLog(
                        "Configured topology evidence saved: "
                        + dialog.FileName);
                });
        }

        private async void ButtonSaveTopologyIoLiveEvidence_Click(
            object sender,
            RoutedEventArgs e)
        {
            var snapshot = topologyIoLiveEvidenceJournal.CaptureSnapshot();
            if (snapshot.Records.Count == 0)
            {
                WriteLog(
                    "Live topology/I/O evidence requires at least one current-session Health or DI read record.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".txt",
                Filter = TranslateUiText(
                    "Text evidence (*.txt)|*.txt|CSV evidence (*.csv)|*.csv"),
                FilterIndex = 1,
                FileName = "live-ethercat-topology-io-"
                    + DateTime.Now.ToString(
                        "yyyyMMdd-HHmmss",
                        CultureInfo.InvariantCulture)
                    + ".txt",
                OverwritePrompt = true,
                Title = TranslateUiText(
                    "Save Live EtherCAT Topology / I/O Evidence")
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            var saveCsv = dialog.FilterIndex == 2
                || string.Equals(
                    System.IO.Path.GetExtension(dialog.FileName),
                    ".csv",
                    StringComparison.OrdinalIgnoreCase);
            var evidence = saveCsv
                ? snapshot.BuildCsvExport()
                : snapshot.BuildTextExport();
            await RunOperationAsync(
                "Save Live EtherCAT Topology / I/O Evidence",
                async () =>
                {
                    await System.Threading.Tasks.Task.Run(
                        () => TopologyIoLiveEvidenceFile.SaveUtf8NoBom(
                            dialog.FileName,
                            evidence));
                    TextOperationState.Text =
                        "Live topology/I/O evidence saved";
                    WriteLog(
                        "Live topology/I/O evidence saved: "
                        + dialog.FileName
                        + ", Retained="
                        + snapshot.Records.Count.ToString(
                            CultureInfo.InvariantCulture)
                        + ", Dropped="
                        + snapshot.DroppedOldestCount.ToString(
                            CultureInfo.InvariantCulture)
                        + ".");
                });
        }

        private void TryRecordTopologyIoHealthSuccess(
            TopologyIoLiveEvidenceOrigin origin,
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            string capturedTopologyLoadOrigin,
            LMCEtherCATTopologyEntry entry,
            LMCEtherCATNodeHealth health)
        {
            TryAppendTopologyIoLiveEvidence(
                () => TopologyIoLiveEvidenceRecord.CreateHealthSuccess(
                    CreateTopologyIoLiveEvidenceContext(
                        origin,
                        TopologyIoLiveEvidenceKind.Health,
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedTopologyLoadOrigin,
                        entry,
                        health.Response == null
                            ? (uint?)null
                            : health.Response.RequestId,
                        null),
                    DateTime.UtcNow,
                    health.CycleCounter,
                    health.SnapshotSequence,
                    health.TimestampMicroseconds,
                    health.HealthFlags.ToString(),
                    (health.HealthFlags
                        & LMCEtherCATNodeHealthFlags.DataValid) != 0,
                    health.Online,
                    health.EtherCATState,
                    health.ALStatusCode,
                    health.SlaveState,
                    health.ClassState,
                    health.DS402StatusWord,
                    health.AxisError,
                    health.LastValidCycle,
                    health.LastStateChangeCycle));
        }

        private void TryRecordTopologyIoDigitalInputSuccess(
            TopologyIoLiveEvidenceOrigin origin,
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            string capturedTopologyLoadOrigin,
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIOReadRequest request,
            LMCDigitalIOValue value)
        {
            TryAppendTopologyIoLiveEvidence(
                () => TopologyIoLiveEvidenceRecord.CreateDigitalInputSuccess(
                    CreateTopologyIoLiveEvidenceContext(
                        origin,
                        TopologyIoLiveEvidenceKind.DI,
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedTopologyLoadOrigin,
                        entry,
                        value.Response == null
                            ? (uint?)null
                            : value.Response.RequestId,
                        request),
                    DateTime.UtcNow,
                    value.CycleCounter,
                    null,
                    null,
                    value.StatusFlags.ToString(),
                    value.IsValid,
                    value.Value,
                    value.ValidMask,
                    value.Direction.ToString(),
                    value.BitWidth,
                    value.OutputRevision));
        }

        private void TryRecordTopologyIoReadFailure(
            TopologyIoLiveEvidenceOrigin origin,
            TopologyIoLiveEvidenceKind kind,
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            string capturedTopologyLoadOrigin,
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIOReadRequest request,
            Exception error)
        {
            TryAppendTopologyIoLiveEvidence(
                () => TopologyIoLiveEvidenceRecord.CreateFailure(
                    CreateTopologyIoLiveEvidenceContext(
                        origin,
                        kind,
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedTopologyLoadOrigin,
                        entry,
                        GetTopologyIoFailureRequestId(error),
                        request),
                    DateTime.UtcNow,
                    error.GetType().Name,
                    error.Message));
        }

        private TopologyIoLiveEvidenceContext
            CreateTopologyIoLiveEvidenceContext(
                TopologyIoLiveEvidenceOrigin origin,
                TopologyIoLiveEvidenceKind kind,
                LMCConnection capturedConnection,
                LMCEtherCATTopology capturedTopology,
                LMCDiagnosticCapabilities capturedCapabilities,
                string capturedTopologyLoadOrigin,
                LMCEtherCATTopologyEntry entry,
                uint? requestId,
                LMCDigitalIOReadRequest digitalInputRequest)
        {
            if (capturedConnection == null
                || capturedTopology == null
                || capturedCapabilities == null
                || entry == null)
            {
                throw new InvalidOperationException(
                    "Live topology/I/O evidence requires an exact connection, topology, capability, and node snapshot.");
            }

            if (kind == TopologyIoLiveEvidenceKind.DI
                && (digitalInputRequest == null
                    || digitalInputRequest.ExpectedDirection
                        != LMCDigitalIODirection.Input))
            {
                throw new InvalidOperationException(
                    "Live DI evidence requires the exact digital-input request.");
            }

            return new TopologyIoLiveEvidenceContext(
                origin,
                kind,
                GetConfiguredTopologyEndpoint(),
                capturedConnection.SessionGeneration,
                capturedCapabilities.DiagnosticsBootId,
                capturedCapabilities.MapRevision,
                capturedCapabilities.CapabilityBits,
                capturedTopologyLoadOrigin,
                capturedTopology.TopologyRevision,
                entry.NodeId,
                entry.Name,
                entry.TopologyIndex,
                entry.HasMasterSlaveIndex
                    ? entry.MasterSlaveIndex
                    : (ushort?)null,
                entry.HasSlotIndex ? entry.SlotIndex : (ushort?)null,
                digitalInputRequest == null
                    ? (uint?)null
                    : digitalInputRequest.IOReference,
                kind == TopologyIoLiveEvidenceKind.Health
                    ? "0x7E13 ReadEtherCATNodeHealth"
                    : "0x7E22 ReadDigitalIO",
                requestId,
                digitalInputRequest == null
                    ? null
                    : digitalInputRequest.ExpectedDirection.ToString(),
                digitalInputRequest == null
                    ? (byte?)null
                    : digitalInputRequest.ExpectedBitWidth);
        }

        private static uint? GetTopologyIoFailureRequestId(Exception error)
        {
            var commandError = error as LMCDiagnosticsCommandException;
            return commandError == null || commandError.Response == null
                ? (uint?)null
                : commandError.Response.RequestId;
        }

        private void TryAppendTopologyIoLiveEvidence(
            Func<TopologyIoLiveEvidenceRecord> createRecord)
        {
            try
            {
                topologyIoLiveEvidenceJournal.Append(createRecord());
                RefreshTopologyIoLiveEvidenceUi();
            }
            catch (Exception error)
            {
                WriteLog(
                    "Live topology/I/O evidence record was not appended; the read result remains unchanged: "
                    + error.Message);
            }
        }

        private void RefreshTopologyIoLiveEvidenceUi()
        {
            if (ButtonSaveTopologyIoLiveEvidence == null
                || TextTopologyIoLiveEvidenceSummary == null)
            {
                return;
            }

            var snapshot = topologyIoLiveEvidenceJournal.CaptureSnapshot();
            TextTopologyIoLiveEvidenceSummary.Text =
                "Live evidence: retained="
                + snapshot.Records.Count.ToString(
                    CultureInfo.InvariantCulture)
                + ", dropped="
                + snapshot.DroppedOldestCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", last sequence="
                + snapshot.LastSequence.ToString(
                    CultureInfo.InvariantCulture)
                + ". Successful samples are parsed current-session PLC responses; failure records contain no copied sample values. Not physical wiring or I/O proof.";
            var idle = !operationRunning
                && !safetyCommandRunning
                && safetyMonitorCount == 0
                && !qualificationRunning;
            ButtonSaveTopologyIoLiveEvidence.IsEnabled =
                !shutdownInProgress
                && idle
                && snapshot.Records.Count != 0;
        }

        private async System.Threading.Tasks.Task
            TryAutoLoadEtherCATTopologyAfterConnectAsync(
                LMCConnection currentConnection)
        {
            try
            {
                await LoadEtherCATTopologyAsync(
                    currentConnection,
                    true,
                    "automatic post-connect load");
            }
            catch (Exception error)
            {
                WriteLog(
                    "CREVIS / EtherCAT topology auto-load failed; the RPC connection remains available and Reload can be retried: "
                    + error.Message);
            }
        }

        private async System.Threading.Tasks.Task LoadEtherCATTopologyAsync(
            LMCConnection currentConnection,
            bool refreshCapabilities,
            string loadOrigin)
        {
            if (currentConnection == null)
            {
                throw new ArgumentNullException("currentConnection");
            }

            BeginEtherCATTopologyLoad(loadOrigin);

            try
            {
                if (refreshCapabilities)
                {
                    await RefreshDiagnosticsCapabilitiesAsync(
                        currentConnection);
                }

                EnsureCapability(
                    LMCDiagnosticCapability.EtherCATTopology,
                    "EtherCAT Topology");

                var topology = await currentConnection.Diagnostics
                    .GetEtherCATTopologyAsync(CancellationToken.None);
                var loadedRows = new List<EtherCATTopologyNodeRow>();
                var configuredCrevisEntryCount = 0;

                foreach (var entry in topology.Entries)
                {
                    if (entry.Name.StartsWith(
                            "GL_9086_1",
                            StringComparison.Ordinal))
                    {
                        configuredCrevisEntryCount++;
                    }

                    loadedRows.Add(new EtherCATTopologyNodeRow(entry));
                }

                EnsureCurrentEtherCATTopologyLoadSession(currentConnection);
                CommitEtherCATTopologyLoad(
                    currentConnection,
                    topology,
                    loadedRows,
                    configuredCrevisEntryCount,
                    loadOrigin);
            }
            catch (Exception error)
            {
                FailEtherCATTopologyLoad(error, loadOrigin);
                throw;
            }
        }

        private void EnsureCurrentEtherCATTopologyLoadSession(
            LMCConnection loadConnection)
        {
            if (!ReferenceEquals(connection, loadConnection)
                || !loadConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "The CREVIS / EtherCAT topology response belongs to a disconnected or replaced connection session. It was not displayed.");
            }
        }

        private void BeginEtherCATTopologyLoad(string loadOrigin)
        {
            InvalidateTopologyIoLiveMonitorTopology();
            etherCATTopology = null;
            configuredCrevisEntryCount = 0;
            etherCATTopologyLoadOrigin = null;
            RefreshLegacyHealthConfiguredSlaveIndices();
            etherCATTopologyRows.Clear();
            GridEtherCATTopology.ItemsSource = null;
            GridEtherCATTopology.ItemsSource = etherCATTopologyRows;
            GridEtherCATTopology.SelectedItem = null;
            TextEtherCATTopologySummary.Text =
                "CREVIS / EtherCAT topology loading ("
                + loadOrigin
                + "). Previous rows were cleared to prevent stale data.";
            UpdateConfiguredTopologyComparisonForUnavailableCurrent(
                "LOAD PENDING; displayed rows were cleared. The last successful comparison baseline and evidence remain unchanged.");
            UpdateSelectedTopologyNodeText(null);
            TextSelectedNodeHealth.Text =
                "Selected node health has not been read.";
            TextSelectedDigitalIO.Text =
                "Selected digital I/O has not been read.";
            ResetSelectedDigitalOutputShadow();
        }

        private void CommitEtherCATTopologyLoad(
            LMCConnection currentConnection,
            LMCEtherCATTopology topology,
            IList<EtherCATTopologyNodeRow> loadedRows,
            int configuredCrevisEntryCount,
            string loadOrigin)
        {
            var candidateSnapshot = ConfiguredTopologySnapshot.Capture(
                topology,
                GetConfiguredTopologyEndpoint(),
                currentConnection.SessionGeneration,
                loadOrigin,
                DateTime.UtcNow);
            var candidateComparison = ConfiguredTopologyComparison.Compare(
                lastSuccessfulConfiguredTopologySnapshot,
                candidateSnapshot);
            var candidateComparisonText =
                candidateComparison.BuildDisplayText();
            var candidateEvidence = candidateComparison.BuildEvidenceText(
                FormatEtherCATTopologyCapabilityEvidence());

            etherCATTopology = topology;
            this.configuredCrevisEntryCount = configuredCrevisEntryCount;
            etherCATTopologyLoadOrigin = loadOrigin;
            RefreshLegacyHealthConfiguredSlaveIndices();
            etherCATTopologyRows.Clear();
            etherCATTopologyRows.AddRange(loadedRows);
            GridEtherCATTopology.ItemsSource = null;
            GridEtherCATTopology.ItemsSource = etherCATTopologyRows;
            GridEtherCATTopology.SelectedItem =
                etherCATTopologyRows.Count == 0
                    ? null
                    : etherCATTopologyRows[0];
            TextSelectedNodeHealth.Text =
                "Selected node health has not been read.";
            TextSelectedDigitalIO.Text =
                "Selected digital I/O has not been read.";
            ResetSelectedDigitalOutputShadow();

            lastSuccessfulConfiguredTopologySnapshot = candidateSnapshot;
            latestConfiguredTopologyComparison = candidateComparison;
            latestConfiguredTopologyEvidence = candidateEvidence;
            TextConfiguredTopologyComparison.Text = candidateComparisonText;
            RefreshEtherCATTopologySummary();
            RefreshConfiguredTopologyEvidenceAvailability();

            WriteLog(
                "EtherCAT topology loaded ("
                + loadOrigin
                + "). Revision=0x"
                + topology.TopologyRevision.ToString("X8")
                + ", Nodes="
                + topology.Entries.Count.ToString(
                    CultureInfo.InvariantCulture)
                + ", CREVIS="
                + configuredCrevisEntryCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", ConfiguredComparison="
                + candidateComparison.Kind.ToString().ToUpperInvariant()
                + ", SHA256="
                + candidateSnapshot.Sha256
                + ".");
        }

        private void FailEtherCATTopologyLoad(
            Exception error,
            string loadOrigin)
        {
            etherCATTopology = null;
            configuredCrevisEntryCount = 0;
            etherCATTopologyLoadOrigin = null;
            RefreshLegacyHealthConfiguredSlaveIndices();
            etherCATTopologyRows.Clear();
            GridEtherCATTopology.ItemsSource = null;
            GridEtherCATTopology.ItemsSource = etherCATTopologyRows;
            GridEtherCATTopology.SelectedItem = null;
            UpdateSelectedTopologyNodeText(null);
            TextSelectedNodeHealth.Text =
                "Node health is unavailable because topology loading failed.";
            TextSelectedDigitalIO.Text =
                "Digital I/O is unavailable because topology loading failed.";
            ResetSelectedDigitalOutputShadow();

            TextEtherCATTopologySummary.Text =
                "CREVIS / EtherCAT topology LOAD FAILED ("
                + loadOrigin
                + "). No displayed topology row is current."
                + Environment.NewLine
                + FormatEtherCATTopologyCapabilityEvidence()
                + Environment.NewLine
                + error.GetType().Name
                + ": "
                + error.Message;
            UpdateConfiguredTopologyComparisonForUnavailableCurrent(
                "LOAD FAILED; no displayed row is current. The last successful comparison baseline and evidence remain unchanged. "
                    + error.GetType().Name
                    + ": "
                    + error.Message);
            RefreshConfiguredTopologyEvidenceAvailability();
        }

        private string FormatEtherCATTopologyCapabilityEvidence()
        {
            if (diagnosticCapabilities == null)
            {
                return "Capabilities=unavailable.";
            }

            return "Capabilities=0x"
                + ((uint)diagnosticCapabilities.Capabilities).ToString("X8")
                + ", DiagnosticsBuild="
                + diagnosticCapabilities.DiagnosticsBuild.ToString(
                    CultureInfo.InvariantCulture)
                + ", BootId=0x"
                + diagnosticCapabilities.DiagnosticsBootId.ToString("X8")
                + ", MapRevision=0x"
                + diagnosticCapabilities.MapRevision.ToString("X8")
                + ".";
        }

        private string GetConfiguredTopologyEndpoint()
        {
            if (!string.IsNullOrWhiteSpace(connectedRemoteIp)
                && connectedRemotePort >= 1
                && connectedRemotePort <= 65535)
            {
                return connectedRemoteIp.Trim()
                    + ":"
                    + connectedRemotePort.ToString(
                        CultureInfo.InvariantCulture);
            }

            throw new InvalidOperationException(
                "The connected PLC endpoint is unavailable; configured topology evidence was not committed.");
        }

        private void UpdateConfiguredTopologyComparisonForUnavailableCurrent(
            string state)
        {
            if (TextConfiguredTopologyComparison == null)
            {
                return;
            }

            var baseline = lastSuccessfulConfiguredTopologySnapshot;
            TextConfiguredTopologyComparison.Text = state
                + Environment.NewLine
                + (baseline == null
                    ? "Last successful baseline=none."
                    : "Last successful baseline preserved: Endpoint="
                        + baseline.Endpoint
                        + ", Revision=0x"
                        + baseline.TopologyRevision.ToString("X8")
                        + ", Nodes="
                        + baseline.EntryCount.ToString(
                            CultureInfo.InvariantCulture)
                        + ", SHA256="
                        + baseline.Sha256)
                + Environment.NewLine
                + "BOUNDARY=CONFIGURED SCHEMA ONLY; not runtime discovery or live I/O proof.";
        }

        private void RefreshConfiguredTopologyEvidenceAvailability()
        {
            if (ButtonSaveConfiguredTopologyEvidence != null)
            {
                ButtonSaveConfiguredTopologyEvidence.IsEnabled =
                    !string.IsNullOrWhiteSpace(
                        latestConfiguredTopologyEvidence);
            }
        }

        private async void ButtonReadSelectedNodeHealth_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Selected EtherCAT Node Health",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.EtherCATTopology,
                        "EtherCAT Topology");
                    EnsureCapability(
                        LMCDiagnosticCapability.EtherCATNodeHealth,
                        "EtherCAT Node Health");

                    var capturedConnection = RequireConnection();
                    var capturedTopology = RequireEtherCATTopology();
                    var capturedCapabilities = diagnosticCapabilities
                        ?? throw new InvalidOperationException(
                            "Diagnostics capabilities are not loaded.");
                    var capturedTopologyLoadOrigin =
                        etherCATTopologyLoadOrigin;
                    var capturedRow = RequireSelectedTopologyNode();
                    var capturedSelectionGeneration =
                        topologyIoLiveMonitorPolicy.SelectionGeneration;
                    LMCEtherCATNodeHealth health;
                    try
                    {
                        health = await capturedConnection.Diagnostics
                            .ReadEtherCATNodeHealthAsync(
                                capturedRow.Entry.NodeId,
                                capturedTopology,
                                capturedCapabilities,
                                CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        if (!CanApplyManualTopologyRowSample(
                                capturedConnection,
                                capturedTopology,
                                capturedCapabilities,
                                capturedRow))
                        {
                            WriteLog(
                                "Discarded late EtherCAT node-health failure because its connection, topology, or row is no longer current.");
                            throw;
                        }

                        var failure = error.GetType().Name
                            + ": "
                            + error.Message;
                        TryRecordTopologyIoReadFailure(
                            TopologyIoLiveEvidenceOrigin.Manual,
                            TopologyIoLiveEvidenceKind.Health,
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedTopologyLoadOrigin,
                            capturedRow.Entry,
                            null,
                            error);
                        capturedRow.ApplyHealthReadError(failure);
                        if (CanCommitManualTopologySelection(
                                capturedConnection,
                                capturedTopology,
                                capturedCapabilities,
                                capturedRow,
                                capturedSelectionGeneration))
                        {
                            TextSelectedNodeHealth.Text =
                                FormatTopologyIoReadFailure(
                                    capturedRow.Entry,
                                    "NODE HEALTH",
                                    failure);
                        }
                        throw;
                    }

                    if (!CanApplyManualTopologyRowSample(
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedRow))
                    {
                        WriteLog(
                            "Discarded late EtherCAT node-health response because its connection, topology, or row is no longer current.");
                        throw new InvalidOperationException(
                            "The EtherCAT node-health response was discarded because its connection, topology, or row is no longer current.");
                    }

                    TryRecordTopologyIoHealthSuccess(
                        TopologyIoLiveEvidenceOrigin.Manual,
                        capturedConnection,
                        capturedTopology,
                        capturedCapabilities,
                        capturedTopologyLoadOrigin,
                        capturedRow.Entry,
                        health);
                    capturedRow.ApplyHealth(health);
                    if (CanCommitManualTopologySelection(
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedRow,
                            capturedSelectionGeneration))
                    {
                        TextSelectedNodeHealth.Text =
                            FormatEtherCATNodeHealth(
                                capturedRow.Entry,
                                health);
                    }
                    WriteLog(
                        "EtherCAT node health read. NodeId="
                        + FormatHex32(capturedRow.Entry.NodeId)
                        + ", Flags="
                        + health.HealthFlags
                        + ".");
                });
        }

        private async void ButtonReadSelectedDigitalInput_Click(
            object sender,
            RoutedEventArgs e)
        {
            await ReadSelectedDigitalIoAsync(LMCDigitalIODirection.Input);
        }

        private async void ButtonReadSelectedDigitalOutput_Click(
            object sender,
            RoutedEventArgs e)
        {
            await ReadSelectedDigitalIoAsync(LMCDigitalIODirection.Output);
        }

        private async System.Threading.Tasks.Task ReadSelectedDigitalIoAsync(
            LMCDigitalIODirection direction)
        {
            await RunOperationAsync(
                direction == LMCDigitalIODirection.Input
                    ? "Read Selected Digital Input"
                    : "Read Selected Digital Output Shadow",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.EtherCATTopology,
                        "EtherCAT Topology");
                    EnsureCapability(
                        LMCDiagnosticCapability.DigitalIORead,
                        "Digital I/O Read");

                    var capturedConnection = RequireConnection();
                    var capturedTopology = RequireEtherCATTopology();
                    var capturedCapabilities = diagnosticCapabilities
                        ?? throw new InvalidOperationException(
                            "Diagnostics capabilities are not loaded.");
                    var capturedTopologyLoadOrigin =
                        etherCATTopologyLoadOrigin;
                    var capturedRow = RequireSelectedTopologyNode();
                    var capturedSelectionGeneration =
                        topologyIoLiveMonitorPolicy.SelectionGeneration;
                    var bitWidth = GetDigitalIoBitWidth(
                        capturedRow.Entry,
                        direction);
                    var request = new LMCDigitalIOReadRequest(
                        capturedTopology.TopologyRevision,
                        capturedRow.Entry.IOReference,
                        direction,
                        bitWidth);
                    LMCDigitalIOValue value;
                    try
                    {
                        value = await capturedConnection.Diagnostics
                            .ReadDigitalIOAsync(
                                capturedTopology,
                                request,
                                capturedCapabilities,
                                CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        if (!CanApplyManualTopologyRowSample(
                                capturedConnection,
                                capturedTopology,
                                capturedCapabilities,
                                capturedRow))
                        {
                            WriteLog(
                                "Discarded late digital-I/O failure because its connection, topology, or row is no longer current.");
                            throw;
                        }

                        var failure = error.GetType().Name
                            + ": "
                            + error.Message;
                        if (direction == LMCDigitalIODirection.Input)
                        {
                            TryRecordTopologyIoReadFailure(
                                TopologyIoLiveEvidenceOrigin.Manual,
                                TopologyIoLiveEvidenceKind.DI,
                                capturedConnection,
                                capturedTopology,
                                capturedCapabilities,
                                capturedTopologyLoadOrigin,
                                capturedRow.Entry,
                                request,
                                error);
                            capturedRow.ApplyDigitalInputReadError(failure);
                        }
                        if (CanCommitManualTopologySelection(
                                capturedConnection,
                                capturedTopology,
                                capturedCapabilities,
                                capturedRow,
                                capturedSelectionGeneration))
                        {
                            ResetSelectedDigitalOutputShadowForIoReadFailure(
                                capturedRow.Entry);
                            TextSelectedDigitalIO.Text =
                                FormatTopologyIoReadFailure(
                                    capturedRow.Entry,
                                    direction == LMCDigitalIODirection.Input
                                        ? "DIGITAL INPUT"
                                        : "OUTPUT SHADOW",
                                    failure);
                        }
                        throw;
                    }

                    if (!CanApplyManualTopologyRowSample(
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedRow))
                    {
                        WriteLog(
                            "Discarded late digital-I/O response because its connection, topology, or row is no longer current.");
                        throw new InvalidOperationException(
                            "The digital-I/O response was discarded because its connection, topology, or row is no longer current.");
                    }

                    var canCommitSelection =
                        CanCommitManualTopologySelection(
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedRow,
                            capturedSelectionGeneration);
                    if (direction == LMCDigitalIODirection.Input)
                    {
                        TryRecordTopologyIoDigitalInputSuccess(
                            TopologyIoLiveEvidenceOrigin.Manual,
                            capturedConnection,
                            capturedTopology,
                            capturedCapabilities,
                            capturedTopologyLoadOrigin,
                            capturedRow.Entry,
                            request,
                            value);
                        capturedRow.ApplyDigitalInput(value);
                    }
                    if (canCommitSelection)
                    {
                        if (direction == LMCDigitalIODirection.Input
                            && HasSelectedDigitalOutputShadowEvidence(
                                capturedRow))
                        {
                            ResetSelectedDigitalOutputShadow();
                        }
                        TextSelectedDigitalIO.Text =
                            FormatDigitalIOValue(capturedRow.Entry, value);
                        if (direction == LMCDigitalIODirection.Output)
                        {
                            selectedDigitalOutputShadow = value;
                            ResetDigitalOutputWritePhysicalVerification();
                            CheckConfirmDigitalOutputWrite.IsChecked = false;
                            TextDigitalOutputExpectedRevision.Text =
                                value.OutputRevision == 0
                                    ? "-"
                                    : FormatHex32(value.OutputRevision);
                            UpdateDigitalOutputWriteStatusAfterShadowRead(
                                value);
                        }
                    }
                    WriteLog(
                        "Digital I/O read. IOReference="
                        + FormatHex32(value.IOReference)
                        + ", Direction="
                        + value.Direction
                        + ", Valid="
                        + value.IsValid
                        + ".");
                });
        }

        private bool CanApplyManualTopologyRowSample(
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            EtherCATTopologyNodeRow capturedRow)
        {
            return ReferenceEquals(connection, capturedConnection)
                && capturedConnection != null
                && capturedConnection.IsConnected
                && ReferenceEquals(etherCATTopology, capturedTopology)
                && capturedTopology != null
                && capturedTopology.BelongsToCurrentSession(capturedConnection)
                && ReferenceEquals(
                    diagnosticCapabilities,
                    capturedCapabilities)
                && capturedCapabilities != null
                && capturedCapabilities.IsBoundTo(
                    capturedConnection.Diagnostics,
                    capturedConnection.SessionGeneration)
                && ContainsEtherCATTopologyRow(capturedRow);
        }

        private bool CanCommitManualTopologySelection(
            LMCConnection capturedConnection,
            LMCEtherCATTopology capturedTopology,
            LMCDiagnosticCapabilities capturedCapabilities,
            EtherCATTopologyNodeRow capturedRow,
            long capturedSelectionGeneration)
        {
            return CanApplyManualTopologyRowSample(
                    capturedConnection,
                    capturedTopology,
                    capturedCapabilities,
                    capturedRow)
                && topologyIoLiveMonitorPolicy.SelectionGeneration
                    == capturedSelectionGeneration
                && ReferenceEquals(
                    GridEtherCATTopology.SelectedItem,
                    capturedRow);
        }

        private bool HasSelectedDigitalOutputShadowEvidence(
            EtherCATTopologyNodeRow row)
        {
            return row != null
                && ReferenceEquals(GridEtherCATTopology.SelectedItem, row)
                && (HasCurrentDigitalOutputShadow(row.Entry)
                    || (CheckConfirmDigitalOutputWrite != null
                        && CheckConfirmDigitalOutputWrite.IsChecked == true));
        }

        private void ResetSelectedDigitalOutputShadowForIoReadFailure(
            LMCEtherCATTopologyEntry entry)
        {
            if (CanReadDigitalIo(
                    entry,
                    LMCDigitalIODirection.Output))
            {
                ResetSelectedDigitalOutputShadow();
            }
        }

        private static string FormatTopologyIoReadFailure(
            LMCEtherCATTopologyEntry entry,
            string channel,
            string failure)
        {
            return "LATEST "
                + channel
                + " READ FAILED for "
                + entry.Name
                + ". Any earlier value for this channel is stale."
                + Environment.NewLine
                + failure;
        }

        private async void ButtonSubmitDigitalOutputWrite_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Submit Digital Output Write",
                async () =>
                {
                    EnsureNoUnresolvedDiagnosticMutation(
                        "Submit Digital Output Write");
                    EnsureCapability(
                        LMCDiagnosticCapability.EtherCATTopology,
                        "EtherCAT Topology");
                    EnsureCapability(
                        LMCDiagnosticCapability.EtherCATNodeHealth,
                        "EtherCAT Node Health");
                    EnsureCapability(
                        LMCDiagnosticCapability.DigitalIORead,
                        "Digital I/O Read");
                    EnsureCapability(
                        LMCDiagnosticCapability.DigitalIOWrite,
                        "Digital I/O Write");

                    var row = RequireSelectedTopologyNode();
                    var topology = RequireEtherCATTopology();
                    var shadow = RequireCurrentDigitalOutputShadow(
                        topology,
                        row.Entry);
                    var ownerConnection = RequireConnection();
                    var diagnostics = ownerConnection.Diagnostics;
                    if (!IsApprovedDigitalOutputWriteReference(
                            diagnostics.GetApprovedDigitalOutputWriteReferences(),
                            row.Entry.IOReference))
                    {
                        throw new InvalidOperationException(
                            "The selected IOReference is not in the SDK digital-output write allowlist.");
                    }

                    if (CheckConfirmDigitalOutputWrite.IsChecked != true)
                    {
                        throw new InvalidOperationException(
                            "Explicitly confirm the selected output, mask, value, and current shadow revision first.");
                    }

                    var value = ParseUInt64Wire(
                        TextDigitalOutputWriteValue.Text,
                        "Digital output Value");
                    var mask = ParseNonZeroUInt64Wire(
                        TextDigitalOutputWriteMask.Text,
                        "Digital output Mask");
                    if ((mask & ~shadow.ValidMask) != 0)
                    {
                        throw new InvalidOperationException(
                            "Digital output Mask contains bits outside the current output ValidMask " +
                            "0x" + shadow.ValidMask.ToString("X16") + ".");
                    }

                    var request = diagnostics.CreateDigitalOutputWriteRequest(
                        shadow,
                        value,
                        mask);
                    var confirmation = MessageBox.Show(
                        TranslateUiText(
                            "Submit guarded digital output write?")
                            + Environment.NewLine
                            + "Node=" + row.Entry.Name
                            + ", IOReference=" + FormatHex32(row.Entry.IOReference)
                            + Environment.NewLine
                            + "Value=0x" + value.ToString("X16")
                            + ", Mask=0x" + mask.ToString("X16")
                            + Environment.NewLine
                            + "ExpectedOutputRevision="
                            + FormatHex32(shadow.OutputRevision),
                        TranslateUiText("Confirm Digital Output Write"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No);
                    if (confirmation != MessageBoxResult.Yes)
                    {
                        WriteLog("Digital output write confirmation was declined; no command was submitted.");
                        return;
                    }

                    ResetDigitalOutputWritePhysicalVerification();
                    ArmDigitalOutputMutationJournal(
                        request,
                        row.Entry,
                        shadow,
                        ownerConnection);
                    pendingDigitalOutputWriteRequest = request;
                    pendingDigitalOutputWriteEntry = row.Entry;
                    pendingDigitalOutputWriteOriginalShadow = shadow;
                    pendingDigitalOutputWriteTicket = null;
                    resolvedDigitalOutputWriteTicket = null;
                    resolvedDigitalOutputWriteSummary = null;
                    digitalOutputWriteTerminalSucceeded = false;
                    SetDigitalOutputWriteOutcomeUncertain(true);

                    LMCOperationTicket ticket;
                    try
                    {
                        ticket = await diagnostics.SubmitDigitalOutputWriteAsync(
                            request,
                            CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        try
                        {
                            LMCDigitalOutputWriteSubmissionFailureContext
                                failureContext;
                            if (LMCDigitalOutputWriteSubmissionFailureContext
                                .TryGet(error, out failureContext))
                            {
                                switch (failureContext.SubmissionOutcome)
                                {
                                    case LMCDigitalOutputWriteSubmissionOutcome
                                        .NotAttempted:
                                        ResolveDiagnosticsMutationJournal(
                                            DiagnosticsMutationKind
                                                .DigitalOutputWrite);
                                        ClearPendingDigitalOutputWriteSubmission();
                                        TextDigitalOutputWriteStatus.Text =
                                            "OUTPUT WRITE NOT SUBMITTED. Preflight failed during "
                                            + failureContext.Phase
                                            + "; no command write was started.";
                                        break;

                                    case LMCDigitalOutputWriteSubmissionOutcome
                                        .Rejected:
                                        ResolveDiagnosticsMutationJournal(
                                            DiagnosticsMutationKind
                                                .DigitalOutputWrite);
                                        ClearPendingDigitalOutputWriteSubmission();
                                        TextDigitalOutputWriteStatus.Text =
                                            "OUTPUT WRITE REJECTED by the RPC/PLC path. This request was not accepted and is not retained as an unresolved mutation.";
                                        break;

                                    case LMCDigitalOutputWriteSubmissionOutcome
                                        .Accepted:
                                        pendingDigitalOutputWriteTicket =
                                            failureContext.Ticket;
                                        MarkDigitalOutputMutationAccepted(
                                            failureContext.Ticket);
                                        MarkDigitalOutputMutationOutcomeUnverified();
                                        SetDigitalOutputWriteOutcomeUncertain(
                                            true);
                                        TextDiagnosticOperationSummary.Text =
                                            FormatOperationTicket(
                                                failureContext.Ticket);
                                        TextDigitalOutputWriteStatus.Text =
                                            "OUTPUT WRITE ACCEPTED AS TICKET "
                                            + failureContext.Ticket.TicketId
                                                .ToString(
                                                    CultureInfo
                                                        .InvariantCulture)
                                            + ", BUT POST-SUBMISSION SESSION VALIDATION FAILED. Automatic replay and stale-session polling are blocked; physically verify the output before explicit acknowledgement.";
                                        break;

                                    default:
                                        MarkDigitalOutputMutationOutcomeUnverified();
                                        SetDigitalOutputWriteOutcomeUncertain(
                                            true);
                                        TextDigitalOutputWriteStatus.Text =
                                            "OUTPUT WRITE SUBMISSION OUTCOME UNVERIFIED. Socket write started, but no authoritative PLC rejection or accepted ticket was obtained. Automatic replay is blocked; physically verify the output before explicit acknowledgement.";
                                        break;
                                }
                            }
                            else
                            {
                                MarkDigitalOutputMutationOutcomeUnverified();
                                SetDigitalOutputWriteOutcomeUncertain(true);
                                TextDigitalOutputWriteStatus.Text =
                                    "OUTPUT WRITE SUBMISSION OUTCOME UNVERIFIED. The request may have reached the PLC. Automatic replay is blocked; physically verify the output before explicit acknowledgement.";
                            }
                        }
                        catch (Exception journalError)
                        {
                            SetDigitalOutputWriteOutcomeUncertain(true);
                            TextDigitalOutputWriteStatus.Text =
                                "OUTPUT WRITE FAILED AND THE DURABLE JOURNAL TRANSITION ALSO FAILED. Treat the physical output as unverified; do not replay.";
                            CheckConfirmDigitalOutputWrite.IsChecked = false;
                            WriteLog(TextDigitalOutputWriteStatus.Text);
                            UpdateUiState();
                            throw new InvalidOperationException(
                                "Digital output submission and durable journal transition both failed.",
                                new AggregateException(error, journalError));
                        }

                        CheckConfirmDigitalOutputWrite.IsChecked = false;
                        WriteLog(TextDigitalOutputWriteStatus.Text);
                        UpdateUiState();
                        throw;
                    }

                    if (ticket == null)
                    {
                        MarkDigitalOutputMutationOutcomeUnverified();
                        TextDigitalOutputWriteStatus.Text =
                            "OUTPUT WRITE SUBMISSION OUTCOME UNVERIFIED. No ticket was returned, so automatic replay is blocked.";
                        WriteLog(TextDigitalOutputWriteStatus.Text);
                        throw new InvalidOperationException(
                            "Digital output write submission returned no ticket.");
                    }

                    pendingDigitalOutputWriteTicket = ticket;
                    MarkDigitalOutputMutationAccepted(ticket);
                    AdoptDiagnosticOperationTicket(ticket);
                    TextDiagnosticOperationSummary.Text =
                        FormatOperationTicket(ticket);
                    if (ReferenceEquals(ownerConnection, connection)
                        && HasExactDigitalOutputWriteSessionIdentity(
                            ownerConnection,
                            request,
                            ticket))
                    {
                        SetDigitalOutputWriteOutcomeUncertain(false);
                        TextDigitalOutputWriteStatus.Text =
                            "Write accepted as ticket "
                            + ticket.TicketId.ToString(
                                CultureInfo.InvariantCulture)
                            + ". Refresh the ticket until terminal; a successful terminal triggers exact output-shadow readback.";
                    }
                    else
                    {
                        MarkDigitalOutputWriteConnectionLost();
                    }
                    CheckConfirmDigitalOutputWrite.IsChecked = false;
                    WriteLog(
                        "Digital output write accepted. Ticket="
                        + ticket.TicketId.ToString(CultureInfo.InvariantCulture)
                        + ", IOReference="
                        + FormatHex32(request.IOReference)
                        + ", Mask=0x"
                        + request.Mask.ToString("X16")
                        + ".");
                });
        }

        private void ButtonAcknowledgeDigitalOutputWriteUncertain_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!CanAcknowledgeDigitalOutputWriteUncertain())
            {
                return;
            }

            var confirmation = MessageBox.Show(
                TranslateUiText(
                    "This clears only the GUI output-write interlock. It does not prove whether the PLC applied the write.")
                    + Environment.NewLine
                    + TranslateUiText(
                        "Confirm the physical output and PLC output shadow independently before continuing.")
                    + Environment.NewLine
                    + TranslateUiText(
                        "Clear the unverified-outcome interlock now?"),
                TranslateUiText(
                    "Acknowledge Unverified Digital Output Outcome"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);
            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            // MessageBox runs a nested dispatcher loop. Recheck every gate so
            // a concurrent UI transition cannot clear an unrelated record.
            if (!CanAcknowledgeDigitalOutputWriteUncertain())
            {
                return;
            }

            if (HasActiveDiagnosticsMutationJournalRecord
                && diagnosticsMutationJournal.CurrentRecord.Kind
                    == DiagnosticsMutationKind.DigitalOutputWrite)
            {
                try
                {
                    ResolveDiagnosticsMutationJournal(
                        DiagnosticsMutationKind.DigitalOutputWrite);
                }
                catch (Exception error)
                {
                    WriteLog(
                        "Digital output acknowledgement failed to persist the Resolved tombstone: "
                        + error.Message);
                    MessageBox.Show(
                        TranslateUiText(
                            "The durable Resolved tombstone could not be written. The output interlock remains active.")
                            + Environment.NewLine
                            + error.Message,
                        TranslateUiText("Output Recovery Failed"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    UpdateUiState();
                    return;
                }
            }
            ClearPendingDigitalOutputWriteSubmission();
            ResetSelectedDigitalOutputShadow();
            TextDigitalOutputWriteStatus.Text =
                "Operator acknowledged the unverified outcome. Read a fresh output shadow before another write.";
            WriteLog(
                "Digital output write unverified-outcome interlock was explicitly acknowledged by the operator.");
            UpdateUiState();
        }

        private void DigitalOutputWriteTuple_Changed(
            object sender,
            TextChangedEventArgs e)
        {
            ResetDigitalOutputWritePhysicalVerification();
            if (CheckConfirmDigitalOutputWrite != null)
            {
                CheckConfirmDigitalOutputWrite.IsChecked = false;
            }

            UpdateUiState();
        }

        private void CheckConfirmDigitalOutputWrite_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private void DigitalOutputWritePhysicalVerification_Changed(
            object sender,
            RoutedEventArgs e)
        {
            digitalOutputUncertainAcknowledgementState
                .SetPhysicalVerification(
                    CheckDigitalOutputWritePhysicallyVerified != null
                        && CheckDigitalOutputWritePhysicallyVerified.IsChecked
                            == true);
            UpdateUiState();
        }

        private void GridEtherCATTopology_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!ReferenceEquals(e.Source, sender))
            {
                return;
            }

            var row = GridEtherCATTopology.SelectedItem
                as EtherCATTopologyNodeRow;
            InvalidateTopologyIoLiveMonitorSelection();
            UpdateSelectedTopologyNodeText(row);
            TextSelectedNodeHealth.Text =
                "Selected node health has not been read.";
            TextSelectedDigitalIO.Text =
                "Selected digital I/O has not been read.";
            ResetSelectedDigitalOutputShadow();
            UpdateUiState();
        }

        private void RefreshTopologyIoCapabilityState()
        {
            InvalidateTopologyIoLiveMonitorSelection();
            if (!SupportsCapability(
                    LMCDiagnosticCapability.EtherCATTopology))
            {
                ClearTopologyIoState();
                TextEtherCATTopologySummary.Text =
                    "EtherCAT topology capability is not advertised. Commands 0x7E11/0x7E12 remain fail-closed.";
                return;
            }

            var supportsNodeHealth = SupportsCapability(
                LMCDiagnosticCapability.EtherCATNodeHealth);
            var supportsDigitalIo = SupportsCapability(
                LMCDiagnosticCapability.DigitalIORead);
            foreach (var row in etherCATTopologyRows)
            {
                row.SetHealthCapabilityAvailable(supportsNodeHealth);
                row.SetDigitalInputCapabilityAvailable(supportsDigitalIo);
            }

            RefreshEtherCATTopologySummary();

            if (etherCATTopology == null)
            {
                TextEtherCATTopologySummary.Text =
                    "Topology capability is advertised. Connect auto-loads the configured CREVIS coupler, slot modules, and drive schema; select Reload to retry or refresh it.";
            }

            if (!supportsNodeHealth)
            {
                TextSelectedNodeHealth.Text =
                    "Node Health capability is not advertised; command 0x7E13 is disabled.";
            }

            if (!supportsDigitalIo)
            {
                TextSelectedDigitalIO.Text =
                    "Digital I/O Read capability is not advertised; command 0x7E22 is disabled.";
                ResetSelectedDigitalOutputShadow();
            }

            if (!SupportsCapability(LMCDiagnosticCapability.DigitalIOWrite)
                && pendingDigitalOutputWriteRequest == null
                && !digitalOutputWriteOutcomeUncertain)
            {
                TextDigitalOutputWriteStatus.Text =
                    "Digital I/O Write capability is not advertised; command 0x7E23 remains fail-closed.";
            }
        }

        private void UpdateTopologyIoUiState(
            LMCConnection currentConnection,
            bool connected,
            bool idle)
        {
            if (ButtonLoadEtherCATTopology == null)
            {
                return;
            }

            var supportsTopology = SupportsCapability(
                LMCDiagnosticCapability.EtherCATTopology);
            var supportsNodeHealth = supportsTopology
                && SupportsCapability(
                    LMCDiagnosticCapability.EtherCATNodeHealth);
            var supportsDigitalIO = supportsTopology
                && SupportsCapability(LMCDiagnosticCapability.DigitalIORead);
            var supportsDigitalOutputWrite = supportsNodeHealth
                && supportsDigitalIO
                && SupportsCapability(LMCDiagnosticCapability.DigitalIOWrite);
            var row = GridEtherCATTopology.SelectedItem
                as EtherCATTopologyNodeRow;
            var hasCurrentTopology = etherCATTopology != null;
            var selectedEntry = row == null ? null : row.Entry;

            // Keep the manual retry entry point available before capabilities
            // are loaded. It refreshes capabilities first, then reloads the
            // topology when bit 14 is advertised.
            ButtonLoadEtherCATTopology.IsEnabled = connected && idle;
            ButtonSaveConfiguredTopologyEvidence.IsEnabled = idle
                && !string.IsNullOrWhiteSpace(
                    latestConfiguredTopologyEvidence);
            ButtonSaveTopologyIoLiveEvidence.IsEnabled = idle
                && topologyIoLiveEvidenceJournal.CaptureSnapshot()
                    .Records.Count != 0;
            ButtonReadSelectedNodeHealth.IsEnabled = connected
                && idle
                && supportsNodeHealth
                && hasCurrentTopology
                && selectedEntry != null;
            ButtonReadSelectedDigitalInput.IsEnabled = connected
                && idle
                && supportsDigitalIO
                && hasCurrentTopology
                && CanReadDigitalIo(
                    selectedEntry,
                    LMCDigitalIODirection.Input);
            ButtonReadSelectedDigitalOutput.IsEnabled = connected
                && idle
                && supportsDigitalIO
                && hasCurrentTopology
                && CanReadDigitalIo(
                    selectedEntry,
                    LMCDigitalIODirection.Output);

            var hasOutputSelection = hasCurrentTopology
                && CanReadDigitalIo(
                    selectedEntry,
                    LMCDigitalIODirection.Output);
            var approvedOutputReference = connected
                && currentConnection != null
                && currentConnection.IsConnected
                && hasOutputSelection
                && IsApprovedDigitalOutputWriteReference(
                    currentConnection.Diagnostics
                        .GetApprovedDigitalOutputWriteReferences(),
                    selectedEntry.IOReference);
            var hasCurrentOutputShadow = HasCurrentDigitalOutputShadow(
                selectedEntry);
            var operationIsTerminal = diagnosticOperationStatus != null
                && diagnosticOperationStatus.IsTerminal;
            var operationSlotAvailable = diagnosticOperationTicket == null
                || operationIsTerminal;
            var canSubmitOutputWrite = operationSlotAvailable
                && EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation)
                    .IsAllowed
                && DiagnosticsMutationJournalCanArm;

            TextDigitalOutputWriteValue.IsEnabled = connected
                && idle
                && hasOutputSelection;
            TextDigitalOutputWriteMask.IsEnabled = connected
                && idle
                && hasOutputSelection;
            CheckConfirmDigitalOutputWrite.IsEnabled = connected
                && idle
                && supportsDigitalOutputWrite
                && approvedOutputReference
                && hasCurrentOutputShadow;
            ButtonSubmitDigitalOutputWrite.IsEnabled = connected
                && idle
                && supportsDigitalOutputWrite
                && approvedOutputReference
                && hasCurrentOutputShadow
                && canSubmitOutputWrite
                && CheckConfirmDigitalOutputWrite.IsChecked == true;
            CheckDigitalOutputWritePhysicallyVerified.IsEnabled = idle
                && digitalOutputWriteOutcomeUncertain
                && EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation.ExistingResourceCleanup)
                    .IsAllowed;
            ButtonAcknowledgeDigitalOutputWriteUncertain.IsEnabled =
                CanAcknowledgeDigitalOutputWriteUncertain(idle);
        }

        private void ClearTopologyIoState()
        {
            InvalidateTopologyIoLiveMonitorSession();
            etherCATTopology = null;
            configuredCrevisEntryCount = 0;
            etherCATTopologyLoadOrigin = null;
            RefreshLegacyHealthConfiguredSlaveIndices();
            etherCATTopologyRows.Clear();

            if (GridEtherCATTopology == null)
            {
                return;
            }

            GridEtherCATTopology.ItemsSource = null;
            GridEtherCATTopology.ItemsSource = etherCATTopologyRows;
            GridEtherCATTopology.SelectedItem = null;
            TextEtherCATTopologySummary.Text =
                "Connect to auto-load CREVIS / Topology. Reload refreshes capabilities and retries; configured CREVIS rows require PLC bit 14.";
            UpdateConfiguredTopologyComparisonForUnavailableCurrent(
                "No current configured topology is displayed. A same-endpoint reconnect will compare against the last successful in-process baseline.");
            UpdateSelectedTopologyNodeText(null);
            TextSelectedNodeHealth.Text =
                "Selected node health has not been read.";
            TextSelectedDigitalIO.Text =
                "Selected digital I/O has not been read.";
            if (HasUnresolvedDigitalOutputWrite)
            {
                SetDigitalOutputWriteOutcomeUncertain(true);
            }
            ResetSelectedDigitalOutputShadow();
            RefreshConfiguredTopologyEvidenceAvailability();
        }

        private void RefreshEtherCATTopologySummary()
        {
            if (etherCATTopology == null
                || TextEtherCATTopologySummary == null)
            {
                return;
            }

            TextEtherCATTopologySummary.Text =
                FormatEtherCATTopology(etherCATTopology)
                + Environment.NewLine
                + "Load="
                + (string.IsNullOrWhiteSpace(etherCATTopologyLoadOrigin)
                    ? "unknown"
                    : etherCATTopologyLoadOrigin)
                + ", Configured CREVIS entries="
                + configuredCrevisEntryCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", LiveHealth="
                + (SupportsCapability(
                        LMCDiagnosticCapability.EtherCATNodeHealth)
                    ? "advertised"
                    : "not advertised")
                + ", DigitalInput="
                + (SupportsCapability(
                        LMCDiagnosticCapability.DigitalIORead)
                    ? "advertised"
                    : "not advertised")
                + ", DigitalOutput="
                + (SupportsCapability(
                        LMCDiagnosticCapability.DigitalIOWrite)
                    ? "advertised"
                    : "not advertised")
                + "."
                + Environment.NewLine
                + "ConfiguredComparison="
                + (latestConfiguredTopologyComparison == null
                    ? "INITIAL PENDING"
                    : latestConfiguredTopologyComparison.Kind
                        .ToString()
                        .ToUpperInvariant())
                + ", ConfiguredSHA256="
                + (lastSuccessfulConfiguredTopologySnapshot == null
                    ? "n/a"
                    : lastSuccessfulConfiguredTopologySnapshot.Sha256)
                + ".";
        }

        private void ResetSelectedDigitalOutputShadow()
        {
            selectedDigitalOutputShadow = null;
            ResetDigitalOutputWritePhysicalVerification();
            if (TextDigitalOutputExpectedRevision != null)
            {
                TextDigitalOutputExpectedRevision.Text = "-";
                CheckConfirmDigitalOutputWrite.IsChecked = false;
                if (digitalOutputWriteOutcomeUncertain)
                {
                    TextDigitalOutputWriteStatus.Text =
                        "OUTPUT WRITE OUTCOME UNVERIFIED. Automatic replay is blocked. Restore the exact session and reread if possible, or physically verify the output before explicit acknowledgement.";
                }
                else if (pendingDigitalOutputWriteRequest != null)
                {
                    TextDigitalOutputWriteStatus.Text =
                        digitalOutputWriteTerminalSucceeded
                            ? "Output write terminal succeeded, but exact shadow readback is still required."
                            : "Output write ticket is pending. Refresh the ticket; do not submit another mutation.";
                }
                else
                {
                    TextDigitalOutputWriteStatus.Text =
                        "Read a valid selected output shadow before writing.";
                }
            }
        }

        private bool HasCurrentDigitalOutputShadow(
            LMCEtherCATTopologyEntry entry)
        {
            return entry != null
                && etherCATTopology != null
                && selectedDigitalOutputShadow != null
                && selectedDigitalOutputShadow.IsValid
                && selectedDigitalOutputShadow
                    .HasValidatedTopologyBinding
                && selectedDigitalOutputShadow.Direction
                    == LMCDigitalIODirection.Output
                && selectedDigitalOutputShadow.TopologyRevision
                    == etherCATTopology.TopologyRevision
                && selectedDigitalOutputShadow.IOReference
                    == entry.IOReference
                && selectedDigitalOutputShadow.NodeId == entry.NodeId
                && selectedDigitalOutputShadow.OutputRevision != 0;
        }

        private LMCDigitalIOValue RequireCurrentDigitalOutputShadow(
            LMCEtherCATTopology topology,
            LMCEtherCATTopologyEntry entry)
        {
            if (!HasCurrentDigitalOutputShadow(entry)
                || topology == null
                || selectedDigitalOutputShadow.TopologyRevision
                    != topology.TopologyRevision)
            {
                throw new InvalidOperationException(
                    "Read a valid output shadow for the currently selected topology node first.");
            }

            return selectedDigitalOutputShadow;
        }

        private static bool IsApprovedDigitalOutputWriteReference(
            IReadOnlyList<uint> approvedReferences,
            uint ioReference)
        {
            if (approvedReferences == null || ioReference == 0)
            {
                return false;
            }

            foreach (var approvedReference in approvedReferences)
            {
                if (approvedReference == ioReference)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasExactDigitalOutputWriteSessionIdentity(
            LMCConnection currentConnection,
            LMCDigitalOutputWriteRequest request,
            LMCOperationTicket ticket)
        {
            return currentConnection != null
                && currentConnection.IsConnected
                && request != null
                && request.IsSnapshotBound
                && request.BelongsToCurrentSession(currentConnection)
                && ticket != null
                && ticket.OperationKind
                    == LMCOperationKind.DigitalOutputWrite
                && ticket.BelongsToCurrentSession(currentConnection)
                && ticket.SubmissionTopologyRevision
                    == request.TopologyRevision
                && request.SourceDiagnosticsBootId != 0
                && ticket.DiagnosticsBootId
                    == request.SourceDiagnosticsBootId;
        }

        private static bool HasExactDigitalOutputWriteReadbackIdentity(
            LMCConnection currentConnection,
            LMCDigitalOutputWriteRequest request,
            LMCOperationTicket ticket,
            LMCDigitalIOValue readback)
        {
            return HasExactDigitalOutputWriteSessionIdentity(
                    currentConnection,
                    request,
                    ticket)
                && readback != null
                && readback.BelongsToCurrentSession(currentConnection)
                && readback.DiagnosticsBootId
                    == request.SourceDiagnosticsBootId
                && readback.DiagnosticsBootId
                    == ticket.DiagnosticsBootId;
        }

        private bool HasCurrentDigitalOutputWriteReadbackContinuation(
            LMCConnection expectedConnection,
            LMCDigitalOutputWriteRequest request,
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIOValue originalShadow,
            LMCOperationTicket ticket,
            LMCOperationTicket pendingTicket)
        {
            return ReferenceEquals(connection, expectedConnection)
                && expectedConnection != null
                && expectedConnection.IsConnected
                && ticket != null
                && ReferenceEquals(diagnosticOperationTicket, ticket)
                && ticket.BelongsToCurrentSession(expectedConnection)
                && ReferenceEquals(
                    pendingDigitalOutputWriteRequest,
                    request)
                && ReferenceEquals(
                    pendingDigitalOutputWriteEntry,
                    entry)
                && ReferenceEquals(
                    pendingDigitalOutputWriteOriginalShadow,
                    originalShadow)
                && ReferenceEquals(
                    pendingDigitalOutputWriteTicket,
                    pendingTicket)
                && IsSameDigitalOutputWriteTicket(ticket, pendingTicket);
        }

        private async System.Threading.Tasks.Task<string>
            VerifyDigitalOutputWriteReadbackAsync(
                LMCConnection expectedConnection,
                LMCOperationTicket ticket,
                LMCOperationStatus status,
                CancellationToken cancellationToken)
        {
            if (ticket == null
                || status == null
                || ticket.OperationKind != LMCOperationKind.DigitalOutputWrite
                || !status.IsTerminal
                || !ReferenceEquals(connection, expectedConnection)
                || expectedConnection == null
                || !ReferenceEquals(diagnosticOperationTicket, ticket)
                || !ticket.BelongsToCurrentSession(expectedConnection))
            {
                return string.Empty;
            }

            if (resolvedDigitalOutputWriteTicket != null
                && IsSameDigitalOutputWriteTicket(
                    ticket,
                    resolvedDigitalOutputWriteTicket))
            {
                return Environment.NewLine
                    + (resolvedDigitalOutputWriteSummary ?? string.Empty);
            }

            if (pendingDigitalOutputWriteTicket == null
                || !IsSameDigitalOutputWriteTicket(
                    ticket,
                    pendingDigitalOutputWriteTicket))
            {
                const string mismatch =
                    "Digital output terminal ticket does not match the pending write context. The outcome remains unverified and automatic replay is blocked.";
                MarkDigitalOutputMutationOutcomeUnverified();
                SetDigitalOutputWriteOutcomeUncertain(true);
                TextDigitalOutputWriteStatus.Text = mismatch;
                return Environment.NewLine + mismatch;
            }

            if (!status.IsSuccessful)
            {
                var failed =
                    "Digital output write reached a known non-success terminal state; no automatic replay was attempted.";
                ResolveDiagnosticsMutationJournal(
                    DiagnosticsMutationKind.DigitalOutputWrite);
                pendingDigitalOutputWriteRequest = null;
                pendingDigitalOutputWriteEntry = null;
                pendingDigitalOutputWriteOriginalShadow = null;
                pendingDigitalOutputWriteTicket = null;
                digitalOutputWriteTerminalSucceeded = false;
                SetDigitalOutputWriteOutcomeUncertain(false);
                resolvedDigitalOutputWriteTicket = ticket;
                resolvedDigitalOutputWriteSummary = failed;
                TextDigitalOutputWriteStatus.Text = failed;
                return Environment.NewLine + failed;
            }

            if (!digitalOutputWriteTerminalSucceeded)
            {
                MarkDigitalOutputMutationTerminalSuccess(ticket);
                digitalOutputWriteTerminalSucceeded = true;
            }

            var request = pendingDigitalOutputWriteRequest;
            var entry = pendingDigitalOutputWriteEntry;
            var originalShadow = pendingDigitalOutputWriteOriginalShadow;
            var pendingTicket = pendingDigitalOutputWriteTicket;
            if (request == null || entry == null || originalShadow == null)
            {
                const string missing =
                    "Digital output write reached success, but this GUI has no matching submission context. Read Output Shadow manually before trusting the result.";
                MarkDigitalOutputMutationOutcomeUnverified();
                SetDigitalOutputWriteOutcomeUncertain(true);
                TextDigitalOutputWriteStatus.Text = missing;
                return Environment.NewLine + missing;
            }

            try
            {
                var currentConnection = expectedConnection;
                var topology = RequireEtherCATTopology();
                if (!ReferenceEquals(currentConnection, connection)
                    || !HasExactDigitalOutputWriteSessionIdentity(
                            currentConnection,
                        request,
                        ticket))
                {
                    throw new InvalidOperationException(
                        "The output write ticket and source snapshot do not belong to the same current connection session and BootId.");
                }

                var readRequest = new LMCDigitalIOReadRequest(
                    request.TopologyRevision,
                    request.IOReference,
                    LMCDigitalIODirection.Output,
                    GetDigitalIoBitWidth(
                        entry,
                        LMCDigitalIODirection.Output));
                var readback = await currentConnection.Diagnostics
                    .ReadDigitalIOAsync(
                        topology,
                        readRequest,
                        cancellationToken);
                if (!HasCurrentDigitalOutputWriteReadbackContinuation(
                        expectedConnection,
                        request,
                        entry,
                        originalShadow,
                        ticket,
                        pendingTicket))
                {
                    return string.Empty;
                }

                selectedDigitalOutputShadow = readback;
                TextDigitalOutputExpectedRevision.Text =
                    readback.OutputRevision == 0
                        ? "-"
                        : FormatHex32(readback.OutputRevision);
                TextSelectedDigitalIO.Text =
                    FormatDigitalIOValue(entry, readback);

                var matches = ReferenceEquals(currentConnection, connection)
                    && HasExactDigitalOutputWriteReadbackIdentity(
                        currentConnection,
                        request,
                        ticket,
                        readback)
                    && readback.IsValid
                    && readback.NodeId == entry.NodeId
                    && readback.Direction == LMCDigitalIODirection.Output
                    && readback.TopologyRevision == request.TopologyRevision
                    && readback.IOReference == request.IOReference
                    && readback.BitWidth == originalShadow.BitWidth
                    && readback.ValidMask == originalShadow.ValidMask
                    && readback.OutputRevision != 0
                    && readback.OutputRevision != request.ExpectedOutputRevision
                    && readback.Value
                        == ((originalShadow.Value & ~request.Mask)
                            | (request.Value & request.Mask));
                var summary = matches
                    ? "Digital output write VERIFIED by exact output-shadow reread. NewRevision="
                        + FormatHex32(readback.OutputRevision)
                        + ", Value=0x"
                        + readback.Value.ToString("X16")
                        + "."
                    : "Digital output write NOT VERIFIED by exact shadow reread. Expected full value=0x"
                        + ((originalShadow.Value & ~request.Mask)
                            | (request.Value & request.Mask)).ToString("X16")
                        + ", Actual full value=0x"
                        + readback.Value.ToString("X16")
                        + ", Node="
                        + FormatHex32(readback.NodeId)
                        + ", IOReference="
                        + FormatHex32(readback.IOReference)
                        + ", PreviousRevision="
                        + FormatHex32(request.ExpectedOutputRevision)
                        + ", ActualRevision="
                        + (readback.OutputRevision == 0
                            ? "0"
                            : FormatHex32(readback.OutputRevision))
                        + ".";
                TextDigitalOutputWriteStatus.Text = summary;
                WriteLog(summary);
                if (matches)
                {
                    ResolveDiagnosticsMutationJournal(
                        DiagnosticsMutationKind.DigitalOutputWrite);
                    pendingDigitalOutputWriteRequest = null;
                    pendingDigitalOutputWriteEntry = null;
                    pendingDigitalOutputWriteOriginalShadow = null;
                    pendingDigitalOutputWriteTicket = null;
                    digitalOutputWriteTerminalSucceeded = false;
                    SetDigitalOutputWriteOutcomeUncertain(false);
                    resolvedDigitalOutputWriteTicket = ticket;
                    resolvedDigitalOutputWriteSummary = summary;
                }
                else
                {
                    MarkDigitalOutputMutationReadbackMismatch();
                    SetDigitalOutputWriteOutcomeUncertain(true);
                }
                return Environment.NewLine + summary;
            }
            catch (Exception error)
            {
                if (!HasCurrentDigitalOutputWriteReadbackContinuation(
                        expectedConnection,
                        request,
                        entry,
                        originalShadow,
                        ticket,
                        pendingTicket))
                {
                    return string.Empty;
                }

                SetDigitalOutputWriteOutcomeUncertain(true);
                Exception journalError = null;
                try
                {
                    MarkDigitalOutputMutationOutcomeUnverified();
                }
                catch (Exception transitionError)
                {
                    journalError = transitionError;
                }

                var combinedError = journalError == null
                    ? error
                    : new AggregateException(error, journalError);
                var summary =
                    "Digital output write terminal was successful, but output-shadow readback FAILED: "
                    + error.Message
                    + (journalError == null
                        ? string.Empty
                        : " Durable journal transition also failed: "
                            + journalError.Message);
                TextDigitalOutputWriteStatus.Text = summary;
                WriteLog(summary + Environment.NewLine + combinedError);
                return Environment.NewLine + summary;
            }
        }

        private void UpdateDigitalOutputWriteStatusAfterShadowRead(
            LMCDigitalIOValue readback)
        {
            var request = pendingDigitalOutputWriteRequest;
            var ticket = pendingDigitalOutputWriteTicket;
            var currentConnection = connection;
            if (digitalOutputWriteOutcomeUncertain
                && digitalOutputWriteTerminalSucceeded
                && request != null
                && HasExactDigitalOutputWriteReadbackIdentity(
                    currentConnection,
                    request,
                    ticket,
                    readback)
                && pendingDigitalOutputWriteEntry != null
                && pendingDigitalOutputWriteOriginalShadow != null
                && readback != null
                && readback.IsValid
                && readback.NodeId == pendingDigitalOutputWriteEntry.NodeId
                && readback.Direction == LMCDigitalIODirection.Output
                && readback.TopologyRevision == request.TopologyRevision
                && readback.IOReference == request.IOReference
                && readback.OutputRevision != 0
                && readback.OutputRevision != request.ExpectedOutputRevision
                && readback.BitWidth
                    == pendingDigitalOutputWriteOriginalShadow.BitWidth
                && readback.ValidMask
                    == pendingDigitalOutputWriteOriginalShadow.ValidMask
                && readback.Value
                    == ((pendingDigitalOutputWriteOriginalShadow.Value
                            & ~request.Mask)
                        | (request.Value & request.Mask)))
            {
                var verified =
                    "Digital output write VERIFIED by a later exact output-shadow reread. NewRevision="
                    + FormatHex32(readback.OutputRevision)
                    + ", Value=0x"
                    + readback.Value.ToString("X16")
                    + ".";
                ResolveDiagnosticsMutationJournal(
                    DiagnosticsMutationKind.DigitalOutputWrite);
                pendingDigitalOutputWriteRequest = null;
                pendingDigitalOutputWriteEntry = null;
                pendingDigitalOutputWriteOriginalShadow = null;
                if (pendingDigitalOutputWriteTicket != null)
                {
                    resolvedDigitalOutputWriteTicket =
                        pendingDigitalOutputWriteTicket;
                    resolvedDigitalOutputWriteSummary = verified;
                }
                pendingDigitalOutputWriteTicket = null;
                digitalOutputWriteTerminalSucceeded = false;
                SetDigitalOutputWriteOutcomeUncertain(false);
                TextDigitalOutputWriteStatus.Text = verified;
                WriteLog(verified);
                return;
            }

            if (digitalOutputWriteOutcomeUncertain)
            {
                TextDigitalOutputWriteStatus.Text =
                    "OUTPUT WRITE OUTCOME REMAINS UNVERIFIED. The latest shadow did not satisfy the original owner/session/BootId, node, topology, I/O reference, revision, and full-value preservation proof.";
            }
            else if (pendingDigitalOutputWriteRequest != null)
            {
                TextDigitalOutputWriteStatus.Text =
                    digitalOutputWriteTerminalSucceeded
                        ? "Output write terminal succeeded, but this shadow does not complete exact readback verification."
                        : "Output write ticket is pending. Refresh the ticket before interpreting this shadow.";
            }
            else
            {
                TextDigitalOutputWriteStatus.Text = readback != null
                    && readback.IsValid
                    && readback.OutputRevision != 0
                    ? "Output shadow is valid. Review Value and Mask, then explicitly confirm the write."
                    : "Output shadow is not writable: the snapshot is invalid or has no CAS revision.";
            }
        }

        private static ulong ParseUInt64Wire(string value, string fieldName)
        {
            var text = (value ?? string.Empty).Trim();
            var style = NumberStyles.Integer;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(2);
                style = NumberStyles.AllowHexSpecifier;
            }

            ulong result;
            if (text.Length == 0
                || !ulong.TryParse(
                    text,
                    style,
                    CultureInfo.InvariantCulture,
                    out result))
            {
                throw new InvalidOperationException(
                    fieldName
                    + " must be a UInt64 in decimal or 0x-prefixed hexadecimal form.");
            }

            return result;
        }

        private static bool IsSameDigitalOutputWriteTicket(
            LMCOperationTicket left,
            LMCOperationTicket right)
        {
            return IsSameDiagnosticOperationTicket(left, right)
                && left.SubmissionTopologyRevision
                    == right.SubmissionTopologyRevision
                && left.QueuedCycle == right.QueuedCycle;
        }

        private static ulong ParseNonZeroUInt64Wire(
            string value,
            string fieldName)
        {
            var result = ParseUInt64Wire(value, fieldName);
            if (result == 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be non-zero.");
            }

            return result;
        }

        private LMCEtherCATTopology RequireEtherCATTopology()
        {
            if (etherCATTopology == null)
            {
                throw new InvalidOperationException(
                    "Load the EtherCAT topology first.");
            }

            return etherCATTopology;
        }

        private EtherCATTopologyNodeRow RequireSelectedTopologyNode()
        {
            RequireEtherCATTopology();
            var row = GridEtherCATTopology.SelectedItem
                as EtherCATTopologyNodeRow;
            if (row == null)
            {
                throw new InvalidOperationException(
                    "Select one EtherCAT topology row first.");
            }

            return row;
        }

        private static bool CanReadDigitalIo(
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIODirection direction)
        {
            if (entry == null || entry.IOReference == 0)
            {
                return false;
            }

            var byteWidth = direction == LMCDigitalIODirection.Input
                ? entry.InputBytes
                : entry.OutputBytes;
            return byteWidth != 0 && byteWidth <= sizeof(ulong);
        }

        private static byte GetDigitalIoBitWidth(
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIODirection direction)
        {
            if (!CanReadDigitalIo(entry, direction))
            {
                throw new InvalidOperationException(
                    direction == LMCDigitalIODirection.Input
                        ? "The selected topology node has no readable digital input bundle."
                        : "The selected topology node has no readable digital output-shadow bundle.");
            }

            var byteWidth = direction == LMCDigitalIODirection.Input
                ? entry.InputBytes
                : entry.OutputBytes;
            return checked((byte)(byteWidth * 8));
        }

        private void UpdateSelectedTopologyNodeText(
            EtherCATTopologyNodeRow row)
        {
            if (TextSelectedTopologyNode == null)
            {
                return;
            }

            if (row == null)
            {
                TextSelectedTopologyNode.Text =
                    "Select a topology row to inspect health or digital I/O.";
                return;
            }

            TextSelectedTopologyNode.Text =
                "Selected "
                + row.Name
                + ": NodeId="
                + row.NodeId
                + ", IOReference="
                + row.IOReference
                + ", InputBits="
                + row.InputBits
                + ", OutputBits="
                + row.OutputBits
                + ".";
        }

        private static string FormatEtherCATTopology(
            LMCEtherCATTopology topology)
        {
            return "TopologyRevision=0x"
                + topology.TopologyRevision.ToString("X8")
                + ", Nodes="
                + topology.Entries.Count.ToString(CultureInfo.InvariantCulture)
                + ", Slaves="
                + topology.Info.ConfiguredSlaveCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", SlotModules="
                + topology.Info.SlotModuleCount.ToString(
                    CultureInfo.InvariantCulture)
                + ", PhysicalAxes="
                + topology.Info.PhysicalAxisCount.ToString(
                    CultureInfo.InvariantCulture)
                + Environment.NewLine
                + "EntryStride="
                + topology.Info.EntryStride.ToString(
                    CultureInfo.InvariantCulture)
                + ", ChunkLimit="
                + topology.Info.MaxEntriesPerChunk.ToString(
                    CultureInfo.InvariantCulture)
                + ", Flags="
                + topology.Info.TopologyFlags
                + ", CrcKind="
                + topology.Info.CrcKind;
        }

        private static string FormatEtherCATNodeHealth(
            LMCEtherCATTopologyEntry entry,
            LMCEtherCATNodeHealth health)
        {
            return "Name="
                + entry.Name
                + Environment.NewLine
                + "TopologyRevision="
                + FormatHex32(health.TopologyRevision)
                + ", NodeId="
                + FormatHex32(health.NodeId)
                + Environment.NewLine
                + "Flags="
                + health.HealthFlags
                + ", Online="
                + health.Online
                + ", EtherCATState=0x"
                + health.EtherCATState.ToString("X2")
                + Environment.NewLine
                + "ALStatus="
                + FormatHex16(health.ALStatusCode)
                + ", SlaveState="
                + FormatHex32(health.SlaveState)
                + ", ClassState="
                + FormatHex32(health.ClassState)
                + Environment.NewLine
                + "DS402Status="
                + FormatHex32(health.DS402StatusWord)
                + ", AxisError="
                + FormatHex32(health.AxisError)
                + Environment.NewLine
                + "Cycle="
                + health.CycleCounter.ToString(CultureInfo.InvariantCulture)
                + ", SnapshotSequence="
                + health.SnapshotSequence.ToString(
                    CultureInfo.InvariantCulture)
                + ", TimestampUs="
                + health.TimestampMicroseconds.ToString(
                    CultureInfo.InvariantCulture)
                + Environment.NewLine
                + "LastValidCycle="
                + health.LastValidCycle.ToString(
                    CultureInfo.InvariantCulture)
                + ", LastStateChangeCycle="
                + health.LastStateChangeCycle.ToString(
                    CultureInfo.InvariantCulture);
        }

        private static string FormatDigitalIOValue(
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIOValue value)
        {
            return "Name="
                + entry.Name
                + Environment.NewLine
                + "TopologyRevision="
                + FormatHex32(value.TopologyRevision)
                + ", NodeId="
                + FormatHex32(value.NodeId)
                + Environment.NewLine
                + "IOReference="
                + FormatHex32(value.IOReference)
                + ", Direction="
                + value.Direction
                + ", BitWidth="
                + value.BitWidth.ToString(CultureInfo.InvariantCulture)
                + Environment.NewLine
                + "Status="
                + value.StatusFlags
                + ", IsValid="
                + value.IsValid
                + Environment.NewLine
                + "Value=0x"
                + value.Value.ToString("X16")
                + ", ValidMask=0x"
                + value.ValidMask.ToString("X16")
                + Environment.NewLine
                + "Cycle="
                + value.CycleCounter.ToString(CultureInfo.InvariantCulture)
                + ", OutputRevision="
                + (value.OutputRevision == 0
                    ? "n/a"
                    : FormatHex32(value.OutputRevision))
                + (value.Direction == LMCDigitalIODirection.Output
                    ? Environment.NewLine
                        + "NOTE: output value is the PLC RT owner shadow, not physical feedback."
                    : string.Empty);
        }

        private static string FormatHex16(ushort value)
        {
            return "0x" + value.ToString("X4");
        }

        private static string FormatHex32(uint value)
        {
            return "0x" + value.ToString("X8");
        }

        private sealed class EtherCATTopologyNodeRow : INotifyPropertyChanged
        {
            private LMCEtherCATNodeHealth liveHealth;
            private LMCDigitalIOValue liveDigitalInput;
            private string liveHealthReadError;
            private string liveDigitalInputReadError;
            private bool liveHealthCapabilityUnavailable;
            private bool liveDigitalInputCapabilityUnavailable;

            internal EtherCATTopologyNodeRow(
                LMCEtherCATTopologyEntry entry)
            {
                Entry = entry ?? throw new ArgumentNullException("entry");
            }

            internal LMCEtherCATTopologyEntry Entry { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;

            internal void ApplyHealth(LMCEtherCATNodeHealth health)
            {
                if (health == null || health.NodeId != Entry.NodeId)
                {
                    throw new ArgumentException(
                        "Live health does not belong to this topology row.",
                        "health");
                }

                liveHealth = health;
                liveHealthReadError = null;
                liveHealthCapabilityUnavailable = false;
                RaiseLivePropertiesChanged();
            }

            internal void ApplyDigitalInput(LMCDigitalIOValue value)
            {
                if (value == null
                    || value.NodeId != Entry.NodeId
                    || value.Direction != LMCDigitalIODirection.Input)
                {
                    throw new ArgumentException(
                        "Live digital input does not belong to this topology row.",
                        "value");
                }

                liveDigitalInput = value;
                liveDigitalInputReadError = null;
                liveDigitalInputCapabilityUnavailable = false;
                RaiseLivePropertiesChanged();
            }

            internal void ApplyHealthReadError(string message)
            {
                liveHealthCapabilityUnavailable = false;
                liveHealthReadError = NormalizeLiveReadError(message);
                RaiseLivePropertiesChanged();
            }

            internal void ApplyDigitalInputReadError(string message)
            {
                liveDigitalInputCapabilityUnavailable = false;
                liveDigitalInputReadError = NormalizeLiveReadError(message);
                RaiseLivePropertiesChanged();
            }

            internal void SetHealthCapabilityAvailable(bool available)
            {
                if (available)
                {
                    if (!liveHealthCapabilityUnavailable)
                    {
                        return;
                    }

                    liveHealthCapabilityUnavailable = false;
                }
                else
                {
                    liveHealth = null;
                    liveHealthReadError = null;
                    liveHealthCapabilityUnavailable = true;
                }

                RaiseLivePropertiesChanged();
            }

            internal void SetDigitalInputCapabilityAvailable(bool available)
            {
                if (!HasReadableDigitalInput)
                {
                    return;
                }

                if (available)
                {
                    if (!liveDigitalInputCapabilityUnavailable)
                    {
                        return;
                    }

                    liveDigitalInputCapabilityUnavailable = false;
                }
                else
                {
                    liveDigitalInput = null;
                    liveDigitalInputReadError = null;
                    liveDigitalInputCapabilityUnavailable = true;
                }

                RaiseLivePropertiesChanged();
            }

            private static string NormalizeLiveReadError(string message)
            {
                return string.IsNullOrWhiteSpace(message)
                    ? "unknown read error"
                    : message;
            }

            public ushort TopologyIndex { get { return Entry.TopologyIndex; } }
            public string Name { get { return Entry.Name; } }
            public LMCEtherCATTopologyNodeKind NodeKind
            {
                get { return Entry.NodeKind; }
            }
            public string NodeId { get { return FormatHex32(Entry.NodeId); } }
            public string ParentNodeId
            {
                get
                {
                    return Entry.ParentNodeId == 0
                        ? "-"
                        : FormatHex32(Entry.ParentNodeId);
                }
            }
            public string MasterSlaveIndex
            {
                get
                {
                    return Entry.HasMasterSlaveIndex
                        ? Entry.MasterSlaveIndex.ToString(
                            CultureInfo.InvariantCulture)
                        : "-";
                }
            }
            public string SlotIndex
            {
                get
                {
                    return Entry.HasSlotIndex
                        ? Entry.SlotIndex.ToString(CultureInfo.InvariantCulture)
                        : "-";
                }
            }
            public string AxisReference
            {
                get
                {
                    return Entry.PhysicalAxisReference == 0
                        ? "-"
                        : Entry.PhysicalAxisReference.ToString(
                            CultureInfo.InvariantCulture);
                }
            }
            public string SdoReference
            {
                get
                {
                    return Entry.SdoSlaveReference == 0
                        ? "-"
                        : Entry.SdoSlaveReference.ToString(
                            CultureInfo.InvariantCulture);
                }
            }
            public ushort InputBits
            {
                get { return checked((ushort)(Entry.InputBytes * 8)); }
            }
            public ushort OutputBits
            {
                get { return checked((ushort)(Entry.OutputBytes * 8)); }
            }
            public string IOReference
            {
                get
                {
                    return Entry.IOReference == 0
                        ? "-"
                        : FormatHex32(Entry.IOReference);
                }
            }
            public string Identity
            {
                get
                {
                    return FormatHex32(Entry.VendorId)
                        + " / "
                        + FormatHex32(Entry.ProductCode);
                }
            }

            public string LiveOnline
            {
                get
                {
                    if (liveHealthCapabilityUnavailable)
                    {
                        return "UNAVAILABLE";
                    }

                    if (liveHealth == null)
                    {
                        return "-";
                    }

                    var value = liveHealth.Online ? "Yes" : "No";
                    return liveHealthReadError == null
                        ? value
                        : "stale " + value;
                }
            }

            public string LiveEtherCATState
            {
                get
                {
                    if (liveHealthCapabilityUnavailable)
                    {
                        return "UNAVAILABLE";
                    }

                    if (liveHealth == null)
                    {
                        return "-";
                    }

                    var value = "0x"
                        + liveHealth.EtherCATState.ToString("X2");
                    return liveHealthReadError == null
                        ? value
                        : "stale " + value;
                }
            }

            public string LiveALStatus
            {
                get
                {
                    if (liveHealthCapabilityUnavailable)
                    {
                        return "UNAVAILABLE";
                    }

                    if (liveHealth == null)
                    {
                        return "-";
                    }

                    var value = FormatHex16(liveHealth.ALStatusCode);
                    return liveHealthReadError == null
                        ? value
                        : "stale " + value;
                }
            }

            public string LiveQuality
            {
                get
                {
                    var health = liveHealthCapabilityUnavailable
                        ? "Health=UNAVAILABLE"
                        : liveHealthReadError != null
                            ? "Health=ERROR: " + liveHealthReadError
                            : liveHealth == null
                                ? "Health=not sampled"
                                : "Health=" + liveHealth.HealthFlags;
                    if (!HasReadableDigitalInput)
                    {
                        return health;
                    }

                    var input = liveDigitalInputCapabilityUnavailable
                        ? "DI=UNAVAILABLE"
                        : liveDigitalInputReadError != null
                            ? "DI=ERROR: " + liveDigitalInputReadError
                            : liveDigitalInput == null
                                ? "DI=not sampled"
                                : "DI=" + liveDigitalInput.StatusFlags;
                    return health + "; " + input;
                }
            }

            public string LiveCycle
            {
                get
                {
                    var healthCycle = liveHealth == null
                        ? "-"
                        : liveHealth.CycleCounter.ToString(
                            CultureInfo.InvariantCulture);
                    if (!HasReadableDigitalInput)
                    {
                        return "H=" + healthCycle;
                    }

                    var inputCycle = liveDigitalInput == null
                        ? "-"
                        : liveDigitalInput.CycleCounter.ToString(
                            CultureInfo.InvariantCulture);
                    return "H=" + healthCycle + "; DI=" + inputCycle;
                }
            }

            public string LiveDigitalInput
            {
                get
                {
                    if (liveDigitalInputCapabilityUnavailable)
                    {
                        return "UNAVAILABLE";
                    }

                    if (liveDigitalInput == null)
                    {
                        return liveDigitalInputReadError == null
                            ? "-"
                            : "error";
                    }

                    var value = liveDigitalInput.IsValid
                        ? "0x" + liveDigitalInput.Value.ToString("X8")
                        : "invalid (" + liveDigitalInput.StatusFlags + ")";
                    return liveDigitalInputReadError == null
                        ? value
                        : "stale " + value;
                }
            }

            private bool HasReadableDigitalInput
            {
                get
                {
                    return Entry.IOReference != 0
                        && Entry.InputBytes != 0
                        && Entry.InputBytes <= sizeof(ulong);
                }
            }

            private void RaiseLivePropertiesChanged()
            {
                RaisePropertyChanged("LiveOnline");
                RaisePropertyChanged("LiveEtherCATState");
                RaisePropertyChanged("LiveALStatus");
                RaisePropertyChanged("LiveQuality");
                RaisePropertyChanged("LiveCycle");
                RaisePropertyChanged("LiveDigitalInput");
            }

            private void RaisePropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(
                        this,
                        new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const string RecoveryRecordRetirementReason =
            "Operator physically verified the current machine and accepted that the listed old-PLC command outcomes remain unknown.";

        private readonly string recoveryRecordRetirementLedgerDirectoryPath;
        private RecoveryRecordRetirementLedger recoveryRecordRetirementLedger;
        private string recoveryRecordRetirementLedgerOpenError;
        private string recoveryRecordRetirementLedgerRuntimeError;
        private bool recoveryRecordRetirementRestartRequired;

        internal Func<string, string, MessageBoxResult>
            RecoveryRecordRetirementConfirmationOverride { get; set; }
        internal Action RecoveryRecordRetirementExitOverride { get; set; }
        internal Action RecoveryRecordRetirementAfterConfirmationTestHook
        {
            get;
            set;
        }

        internal bool RecoveryRecordRetirementRestartRequired
        {
            get { return recoveryRecordRetirementRestartRequired; }
        }

        private bool RecoveryRecordRetirementLedgerUnavailable
        {
            get
            {
                return recoveryRecordRetirementLedger == null
                    || !string.IsNullOrEmpty(
                        recoveryRecordRetirementLedgerOpenError)
                    || !string.IsNullOrEmpty(
                        recoveryRecordRetirementLedgerRuntimeError);
            }
        }

        private void InitializeRecoveryRecordRetirementLedger()
        {
            try
            {
                recoveryRecordRetirementLedger =
                    recoveryRecordRetirementLedgerDirectoryPath == null
                        ? RecoveryRecordRetirementLedger.OpenDefault()
                        : RecoveryRecordRetirementLedger.Open(
                            recoveryRecordRetirementLedgerDirectoryPath);
                recoveryRecordRetirementLedgerOpenError = null;
                recoveryRecordRetirementLedgerRuntimeError = null;
            }
            catch (Exception error)
            {
                var ledger = recoveryRecordRetirementLedger;
                recoveryRecordRetirementLedger = null;
                if (ledger != null)
                {
                    ledger.Dispose();
                }

                recoveryRecordRetirementLedgerOpenError =
                    error.GetType().Name + ": " + error.Message;
                WriteLog(
                    "Recovery retirement ledger is unavailable. Stale-record "
                    + "retirement is fail-closed: "
                    + recoveryRecordRetirementLedgerOpenError);
            }
        }

        private void DisposeRecoveryRecordRetirementLedger()
        {
            var ledger = recoveryRecordRetirementLedger;
            recoveryRecordRetirementLedger = null;
            if (ledger != null)
            {
                ledger.Dispose();
            }
        }

        private bool TryFinalizeCommittedAxisPowerRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || axisPowerOnRecoveryJournal == null
                || !axisPowerOnRecoveryJournal.HasActiveRecord)
            {
                return false;
            }

            var evidence = axisPowerOnRecoveryJournal
                .CaptureActiveRetirementEvidence();
            var decision = recoveryRecordRetirementLedger
                .FindPendingDecision(evidence);
            if (decision == null)
            {
                return false;
            }

            axisPowerOnRecoveryJournal.ResolveOperatorRetirement(
                evidence,
                decision,
                MonotonicRetirementUtcNow(evidence.UpdatedUtc));
            LogCommittedRetirementFinalizedAtStartup(evidence, decision);
            return true;
        }

        private bool TryFinalizeCommittedAxisCommandRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || axisCommandRecoveryJournal == null
                || !axisCommandRecoveryJournal.HasActiveRecord)
            {
                return false;
            }

            var evidence = axisCommandRecoveryJournal
                .CaptureActiveRetirementEvidence();
            var decision = recoveryRecordRetirementLedger
                .FindPendingDecision(evidence);
            if (decision == null)
            {
                return false;
            }

            axisCommandRecoveryJournal.ResolveOperatorRetirement(
                evidence,
                decision,
                MonotonicRetirementUtcNow(evidence.UpdatedUtc));
            LogCommittedRetirementFinalizedAtStartup(evidence, decision);
            return true;
        }

        private bool TryFinalizeCommittedMotionRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || motionUncertaintyJournal == null
                || !motionUncertaintyJournal.HasActiveRecord)
            {
                return false;
            }

            var evidence = motionUncertaintyJournal
                .CaptureActiveRetirementEvidence();
            var decision = recoveryRecordRetirementLedger
                .FindPendingDecision(evidence);
            if (decision == null)
            {
                return false;
            }

            motionUncertaintyJournal.ResolveOperatorRetirement(
                evidence,
                decision,
                MonotonicRetirementUtcNow(evidence.UpdatedUtc));
            LogCommittedRetirementFinalizedAtStartup(evidence, decision);
            return true;
        }

        private bool TryFinalizeCommittedGroupProfileRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || groupProfileLockRecoveryJournal == null
                || !groupProfileLockRecoveryJournal.HasActiveRecord)
            {
                return false;
            }

            var evidence = groupProfileLockRecoveryJournal
                .CaptureActiveRetirementEvidence();
            var decision = recoveryRecordRetirementLedger
                .FindPendingDecision(evidence);
            if (decision == null)
            {
                return false;
            }

            groupProfileLockRecoveryJournal.ResolveOperatorRetirement(
                evidence,
                decision,
                MonotonicRetirementUtcNow(evidence.UpdatedUtc));
            LogCommittedRetirementFinalizedAtStartup(evidence, decision);
            return true;
        }

        private bool TryFinalizeCommittedGroupPowerRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || groupPowerRecoveryJournal == null
                || !groupPowerRecoveryJournal.HasActiveRecord)
            {
                return false;
            }

            var evidence = groupPowerRecoveryJournal
                .CaptureActiveRetirementEvidence();
            var decision = recoveryRecordRetirementLedger
                .FindPendingDecision(evidence);
            if (decision == null)
            {
                return false;
            }

            groupPowerRecoveryJournal.ResolveOperatorRetirement(
                evidence,
                decision,
                MonotonicRetirementUtcNow(evidence.UpdatedUtc));
            LogCommittedRetirementFinalizedAtStartup(evidence, decision);
            return true;
        }

        private bool TryFinalizeCommittedGroupResetRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || groupResetRecoveryJournal == null
                || !groupResetRecoveryJournal.HasActiveRecord)
            {
                return false;
            }

            var evidence = groupResetRecoveryJournal
                .CaptureActiveRetirementEvidence();
            var decision = recoveryRecordRetirementLedger
                .FindPendingDecision(evidence);
            if (decision == null)
            {
                return false;
            }

            groupResetRecoveryJournal.ResolveOperatorRetirement(
                evidence,
                decision,
                MonotonicRetirementUtcNow(evidence.UpdatedUtc));
            LogCommittedRetirementFinalizedAtStartup(evidence, decision);
            return true;
        }

        private bool
            TryFinalizeCommittedAxisQualificationRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || axisQualificationRecoveryJournal == null
                || !axisQualificationRecoveryJournal.HasActiveRecord)
            {
                return false;
            }

            var evidence = axisQualificationRecoveryJournal
                .CaptureActiveRetirementEvidence();
            var decision = recoveryRecordRetirementLedger
                .FindPendingDecision(evidence);
            if (decision == null)
            {
                return false;
            }

            axisQualificationRecoveryJournal.ResolveOperatorRetirement(
                evidence,
                decision,
                MonotonicRetirementUtcNow(evidence.UpdatedUtc));
            LogCommittedRetirementFinalizedAtStartup(evidence, decision);
            return true;
        }

        private bool
            TryFinalizeCommittedDiagnosticsMutationRetirementAtStartup()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || diagnosticsMutationJournal == null
                || !HasRetirableLegacyDiagnosticsMutationRecord)
            {
                return false;
            }

            var record = diagnosticsMutationJournal.CurrentRecord;
            foreach (var decision in recoveryRecordRetirementLedger
                .CommittedDecisions)
            {
                var source = decision.SourceEvidence;
                if (source.Owner
                        != RecoveryRecordOwner.DiagnosticsMutation
                    || source.RecordIdentity != record.Identity
                    || source.EndpointEvidenceKind
                        != RecoveryEndpointEvidenceKind
                            .OperatorClassifiedLegacyEndpoint)
                {
                    continue;
                }

                var evidence = diagnosticsMutationJournal
                    .CaptureLegacyEndpointBoundRetirementEvidence(
                        source.EndpointIp,
                        source.EndpointPort);
                if (!decision.MatchesSourceEvidence(evidence))
                {
                    continue;
                }

                diagnosticsMutationJournal.ResolveOperatorRetirement(
                    evidence,
                    decision,
                    MonotonicRetirementUtcNow(evidence.UpdatedUtc));
                LogCommittedRetirementFinalizedAtStartup(
                    evidence,
                    decision);
                return true;
            }

            return false;
        }

        private void LogCommittedRetirementFinalizedAtStartup(
            RecoveryJournalSourceEvidence evidence,
            RecoveryRecordRetirementDecision decision)
        {
            WriteLog(
                "Recovery retirement crash-finalization applied exact-byte "
                + "CAS for "
                + evidence.Owner
                + ", Record="
                + evidence.RecordIdentity.ToString("D")
                + ", SourceSha256="
                + evidence.OriginalSha256
                + ", LedgerEntrySha256="
                + decision.DurableEntrySha256
                + ". The archived command outcome remains unknown.");
        }

        private async void ButtonArchiveAndRetireStaleRecovery_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CheckConfirmStaleRecoveryRetirement.IsChecked != true)
            {
                WriteLog(
                    "Stale recovery retirement was not started because the "
                    + "physical-verification acknowledgement is not checked.");
                return;
            }

            var operationSlotAvailable = diagnosticOperationTicket == null
                || (diagnosticOperationStatus != null
                    && diagnosticOperationStatus.IsTerminal);
            var admission = EvaluateDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.RetireStaleRecoveryEvidence,
                operationSlotAvailable);
            if (!admission.IsAllowed)
            {
                WriteLog(
                    CreateDiagnosticsAdmissionException(
                        "Archive and Retire Stale Recovery",
                        admission).Message);
                return;
            }

            var retirementCompleted = false;
            await RunOperationAsync(
                "Archive and Retire Stale Recovery",
                async () =>
                {
                    var capturedConnection = RequireConnection();
                    if (!IsRecoveryIdentityReadOnlyConnection(
                            capturedConnection))
                    {
                        throw new InvalidOperationException(
                            "Stale recovery retirement requires the current "
                            + "connection to remain in recovery-identity "
                            + "read-only quarantine.");
                    }

                    var capturedSessionGeneration =
                        capturedConnection.SessionGeneration;
                    var capturedEndpointIp = RequiredConnectedRemoteIp();
                    var capturedEndpointPort = RequiredConnectedRemotePort();

                    await RefreshDiagnosticsCapabilitiesAsync(
                        capturedConnection);
                    var firstCapabilities =
                        RequireCurrentRetirementCapabilities(
                            capturedConnection,
                            "pre-confirmation");
                    var firstActiveEvidence =
                        CaptureStaleRecoveryRetirementEvidence(
                            capturedEndpointIp,
                            capturedEndpointPort,
                            firstCapabilities);
                    var firstEvidence = firstActiveEvidence
                        .Where(item => RecoveryRetirementEndpointMatches(
                                item,
                                capturedEndpointIp,
                                capturedEndpointPort)
                            && !RecoveryRetirementIdentityMatches(
                                item,
                                firstCapabilities))
                        .ToList();
                    var firstExactCurrentEvidence = firstActiveEvidence
                        .Where(item => RecoveryRetirementEndpointMatches(
                                item,
                                capturedEndpointIp,
                                capturedEndpointPort)
                            && RecoveryRetirementIdentityMatches(
                                item,
                                firstCapabilities))
                        .ToList();
                    var firstOtherEndpointEvidence = firstActiveEvidence
                        .Where(item => !RecoveryRetirementEndpointMatches(
                            item,
                            capturedEndpointIp,
                            capturedEndpointPort))
                        .ToList();
                    if (firstEvidence.Count == 0)
                    {
                        throw new InvalidOperationException(
                            "No stale recovery record matches the quarantined "
                            + "endpoint. Exact-current records remain active and "
                            + "must use their exact recovery workflow.");
                    }

                    var prompt = FormatRecoveryRetirementConfirmation(
                        firstEvidence,
                        firstExactCurrentEvidence,
                        firstOtherEndpointEvidence,
                        capturedEndpointIp,
                        capturedEndpointPort,
                        firstCapabilities);
                    var confirmation =
                        RecoveryRecordRetirementConfirmationOverride == null
                            ? MessageBox.Show(
                                this,
                                prompt,
                                TranslateUiText(
                                    "Archive and Retire Stale Recovery"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning,
                                MessageBoxResult.No)
                            : RecoveryRecordRetirementConfirmationOverride(
                                prompt,
                                TranslateUiText(
                                    "Archive and Retire Stale Recovery"));
                    if (confirmation != MessageBoxResult.Yes)
                    {
                        throw new InvalidOperationException(
                            "Operator declined stale recovery retirement. No "
                            + "ledger decision or journal resolution was written.");
                    }

                    RecoveryRecordRetirementAfterConfirmationTestHook
                        ?.Invoke();

                    EnsureSameRetirementConnection(
                        capturedConnection,
                        capturedSessionGeneration,
                        capturedEndpointIp,
                        capturedEndpointPort);
                    await RefreshDiagnosticsCapabilitiesAsync(
                        capturedConnection);
                    var secondCapabilities =
                        RequireCurrentRetirementCapabilities(
                            capturedConnection,
                            "post-confirmation");
                    if ((firstActiveEvidence.Any(item =>
                            RecoveryRetirementEndpointMatches(
                                item,
                                capturedEndpointIp,
                                capturedEndpointPort)
                            && item.DiagnosticsBuild != 0)
                            && secondCapabilities.DiagnosticsBuild
                                != firstCapabilities.DiagnosticsBuild)
                        || secondCapabilities.DiagnosticsBootId
                            != firstCapabilities.DiagnosticsBootId
                        || secondCapabilities.MapRevision
                            != firstCapabilities.MapRevision
                        || secondCapabilities.ObservationSequence
                            <= firstCapabilities.ObservationSequence)
                    {
                        throw new InvalidOperationException(
                            "The current PLC capability identity changed or "
                            + "was not freshly observed after confirmation. "
                            + "No retirement decision was committed.");
                    }

                    var secondActiveEvidence =
                        CaptureStaleRecoveryRetirementEvidence(
                            capturedEndpointIp,
                            capturedEndpointPort,
                            secondCapabilities);
                    EnsureExactRecoveryEvidenceVector(
                        firstActiveEvidence,
                        secondActiveEvidence);

                    var decisionUtc = DateTime.UtcNow;
                    foreach (var evidence in firstEvidence)
                    {
                        if (decisionUtc < evidence.UpdatedUtc)
                        {
                            decisionUtc = evidence.UpdatedUtc;
                        }
                    }

                    var operatorIdentity = GetRecoveryRetirementOperator();
                    var decisions = new List<
                        RecoveryRecordRetirementDecision>(
                            firstEvidence.Count);
                    foreach (var evidence in firstEvidence)
                    {
                        decisions.Add(
                            recoveryRecordRetirementLedger
                                .CommitOperatorRetirement(
                                    evidence,
                                    capturedEndpointIp,
                                    capturedEndpointPort,
                                    secondCapabilities.DiagnosticsBuild,
                                    secondCapabilities.DiagnosticsBootId,
                                    secondCapabilities.MapRevision,
                                    operatorIdentity,
                                    RecoveryRecordRetirementReason,
                                    decisionUtc));
                    }

                    for (var index = 0;
                        index < firstEvidence.Count;
                        index++)
                    {
                        ResolveCommittedRecoveryRetirement(
                            firstEvidence[index],
                            decisions[index],
                            MonotonicRetirementUtcNow(
                                firstEvidence[index].UpdatedUtc));
                    }

                    recoveryRecordRetirementRestartRequired = true;
                    CheckConfirmStaleRecoveryRetirement.IsChecked = false;
                    WriteLog(
                        "Archived and retired "
                        + firstEvidence.Count.ToString(
                            CultureInfo.InvariantCulture)
                        + " stale recovery record(s). Exact source bytes and "
                        + "immutable decision metadata are under "
                        + recoveryRecordRetirementLedger.DirectoryPath
                        + ". Every old command outcome remains unknown. No "
                        + "Motion, Power, SDO, Write, or cleanup command was "
                        + "sent. "
                        + firstExactCurrentEvidence.Count.ToString(
                            CultureInfo.InvariantCulture)
                        + " exact-current recovery record(s) were kept active. "
                        + firstOtherEndpointEvidence.Count.ToString(
                            CultureInfo.InvariantCulture)
                        + " other-endpoint recovery record(s) were also kept "
                        + "active. "
                        + (firstExactCurrentEvidence.Count == 0
                                && firstOtherEndpointEvidence.Count == 0
                            ? string.Empty
                            : "After restart, kept records must be addressed at "
                                + "their recorded endpoints. Every exact-current "
                                + "record must finish its exact status-only "
                                + "recovery before Motion, Power, or approved "
                                + "SDO Write controls open. ")
                        + "This quarantined session will now close and "
                        + "the application must restart before reconnecting.");

                    retirementCompleted = true;
                    await CloseCurrentConnectionAsync(false);
                },
                true);

            if (!retirementCompleted)
            {
                return;
            }

            if (RecoveryRecordRetirementExitOverride != null)
            {
                RecoveryRecordRetirementExitOverride();
                return;
            }

            MessageBox.Show(
                this,
                TranslateUiText(
                    "The exact stale recovery records were archived and retired. "
                        + "Their command outcomes remain unknown. The quarantined "
                        + "TCP session is closed; the application will now exit. "
                        + "Start it again and address kept records at their recorded "
                        + "endpoints. Any kept exact-current "
                        + "record must finish its exact status-only recovery before "
                        + "Motion, Power, or the approved SDO Write can open."),
                TranslateUiText("Restart Required"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Close();
        }

        private void RecoveryIdentityRetirementConfirmation_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private LMCDiagnosticCapabilities
            RequireCurrentRetirementCapabilities(
                LMCConnection capturedConnection,
                string phase)
        {
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || !capabilities.IsBoundTo(
                    capturedConnection.Diagnostics,
                    capturedConnection.SessionGeneration))
            {
                throw new InvalidOperationException(
                    "Stale recovery retirement "
                    + phase
                    + " requires fresh, current-session nonzero "
                    + "DiagnosticsBootId and MapRevision.");
            }

            if (groupResetRecoveryJournal != null
                && groupResetRecoveryJournal.HasActiveRecord
                && string.Equals(
                    groupResetRecoveryJournal.CurrentRecord.PlcIp,
                    RequiredConnectedRemoteIp(),
                    StringComparison.Ordinal)
                && groupResetRecoveryJournal.CurrentRecord.PlcTcpPort
                    == RequiredConnectedRemotePort()
                && capabilities.DiagnosticsBuild == 0)
            {
                throw new InvalidOperationException(
                    "Stale build-bound recovery retirement "
                    + phase
                    + " requires a nonzero DiagnosticsBuild.");
            }

            return capabilities;
        }

        private List<RecoveryJournalSourceEvidence>
            CaptureStaleRecoveryRetirementEvidence(
                string currentEndpointIp,
                int currentEndpointPort,
                LMCDiagnosticCapabilities currentCapabilities)
        {
            EnsureRecoveryRetirementInfrastructureAvailable();
            var evidence = new List<RecoveryJournalSourceEvidence>(8);
            if (axisPowerOnRecoveryJournal.HasActiveRecord)
            {
                evidence.Add(
                    axisPowerOnRecoveryJournal
                        .CaptureActiveRetirementEvidence());
            }
            if (axisCommandRecoveryJournal.HasActiveRecord)
            {
                evidence.Add(
                    axisCommandRecoveryJournal
                        .CaptureActiveRetirementEvidence());
            }
            if (motionUncertaintyJournal.HasActiveRecord)
            {
                evidence.Add(
                    motionUncertaintyJournal
                        .CaptureActiveRetirementEvidence());
            }
            if (groupProfileLockRecoveryJournal.HasActiveRecord)
            {
                evidence.Add(
                    groupProfileLockRecoveryJournal
                        .CaptureActiveRetirementEvidence());
            }
            if (groupPowerRecoveryJournal.HasActiveRecord)
            {
                evidence.Add(
                    groupPowerRecoveryJournal
                        .CaptureActiveRetirementEvidence());
            }
            if (groupResetRecoveryJournal.HasActiveRecord)
            {
                evidence.Add(
                    groupResetRecoveryJournal
                        .CaptureActiveRetirementEvidence());
            }
            if (axisQualificationRecoveryJournal.HasActiveRecord)
            {
                evidence.Add(
                    axisQualificationRecoveryJournal
                        .CaptureActiveRetirementEvidence());
            }
            if (HasRetirableLegacyDiagnosticsMutationRecord)
            {
                evidence.Add(
                    diagnosticsMutationJournal
                        .CaptureLegacyEndpointBoundRetirementEvidence(
                            currentEndpointIp,
                            currentEndpointPort));
            }

            if (evidence.Count == 0)
            {
                throw new InvalidOperationException(
                    "No active durable recovery record is available for stale "
                    + "identity retirement.");
            }
            if (evidence.Any(item =>
                    RecoveryRetirementEndpointMatches(
                        item,
                        currentEndpointIp,
                        currentEndpointPort)
                    && item.DiagnosticsBuild != 0)
                && currentCapabilities.DiagnosticsBuild == 0)
            {
                throw new InvalidOperationException(
                    "Build-bound recovery retirement requires a current nonzero "
                    + "DiagnosticsBuild. Nothing was retired.");
            }

            return evidence;
        }

        private static bool RecoveryRetirementEndpointMatches(
            RecoveryJournalSourceEvidence evidence,
            string endpointIp,
            int endpointPort)
        {
            return evidence != null
                && string.Equals(
                    evidence.EndpointIp,
                    endpointIp,
                    StringComparison.Ordinal)
                && evidence.EndpointPort == endpointPort;
        }

        private static bool RecoveryRetirementIdentityMatches(
            RecoveryJournalSourceEvidence evidence,
            LMCDiagnosticCapabilities currentCapabilities)
        {
            return evidence != null
                && currentCapabilities != null
                && (evidence.DiagnosticsBuild == 0
                    || evidence.DiagnosticsBuild
                        == currentCapabilities.DiagnosticsBuild)
                && evidence.DiagnosticsBootId
                    == currentCapabilities.DiagnosticsBootId
                && evidence.MapRevision == currentCapabilities.MapRevision;
        }

        private void EnsureRecoveryRetirementInfrastructureAvailable()
        {
            if (RecoveryRecordRetirementLedgerUnavailable
                || AxisPowerOnRecoveryJournalUnavailable
                || AxisCommandRecoveryJournalUnavailable
                || MotionUncertaintyJournalUnavailable
                || GroupProfileLockRecoveryJournalUnavailable
                || GroupPowerRecoveryJournalUnavailable
                || GroupResetRecoveryJournalUnavailable
                || AxisQualificationRecoveryJournalUnavailable
                || DiagnosticsMutationJournalUnavailable)
            {
                throw new InvalidOperationException(
                    "Stale recovery retirement is fail-closed because its "
                    + "ledger or one of the eight durable recovery journals is "
                    + "unavailable. "
                    + GetRecoveryRecordRetirementUnavailableGuidance());
            }
        }

        private void EnsureSameRetirementConnection(
            LMCConnection capturedConnection,
            long capturedSessionGeneration,
            string capturedEndpointIp,
            int capturedEndpointPort)
        {
            if (!ReferenceEquals(connection, capturedConnection)
                || capturedConnection == null
                || !capturedConnection.IsConnected
                || capturedConnection.SessionGeneration
                    != capturedSessionGeneration
                || !IsRecoveryIdentityReadOnlyConnection(
                    capturedConnection)
                || !string.Equals(
                    RequiredConnectedRemoteIp(),
                    capturedEndpointIp,
                    StringComparison.Ordinal)
                || RequiredConnectedRemotePort() != capturedEndpointPort)
            {
                throw new InvalidOperationException(
                    "The quarantined connection, session, or endpoint changed "
                    + "after confirmation. No retirement decision was committed.");
            }
        }

        private static void EnsureExactRecoveryEvidenceVector(
            IReadOnlyList<RecoveryJournalSourceEvidence> expected,
            IReadOnlyList<RecoveryJournalSourceEvidence> actual)
        {
            if (expected == null
                || actual == null
                || expected.Count != actual.Count)
            {
                throw new InvalidOperationException(
                    "The active recovery record set changed after confirmation. "
                    + "No retirement decision was committed.");
            }

            for (var index = 0; index < expected.Count; index++)
            {
                if (expected[index].Owner != actual[index].Owner
                    || !expected[index].ExactSourceEquals(actual[index]))
                {
                    throw new InvalidOperationException(
                        "Recovery evidence changed after confirmation. No "
                        + "retirement decision was committed.");
                }
            }
        }

        private void ResolveCommittedRecoveryRetirement(
            RecoveryJournalSourceEvidence evidence,
            RecoveryRecordRetirementDecision decision,
            DateTime updatedUtc)
        {
            switch (evidence.Owner)
            {
                case RecoveryRecordOwner.AxisPower:
                    axisPowerOnRecoveryJournal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        updatedUtc);
                    break;

                case RecoveryRecordOwner.AxisCommand:
                    axisCommandRecoveryJournal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        updatedUtc);
                    break;

                case RecoveryRecordOwner.Motion:
                    motionUncertaintyJournal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        updatedUtc);
                    break;

                case RecoveryRecordOwner.GroupProfileLock:
                    groupProfileLockRecoveryJournal
                        .ResolveOperatorRetirement(
                            evidence,
                            decision,
                            updatedUtc);
                    break;

                case RecoveryRecordOwner.GroupPower:
                    groupPowerRecoveryJournal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        updatedUtc);
                    break;

                case RecoveryRecordOwner.GroupReset:
                    groupResetRecoveryJournal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        updatedUtc);
                    break;

                case RecoveryRecordOwner.AxisQualification:
                    axisQualificationRecoveryJournal
                        .ResolveOperatorRetirement(
                            evidence,
                            decision,
                            updatedUtc);
                    break;

                case RecoveryRecordOwner.DiagnosticsMutation:
                    diagnosticsMutationJournal.ResolveOperatorRetirement(
                        evidence,
                        decision,
                        updatedUtc);
                    diagnosticsMutationRecoveredAtStartup = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("evidence");
            }
        }

        private string FormatRecoveryRetirementConfirmation(
            IReadOnlyList<RecoveryJournalSourceEvidence> staleEvidence,
            IReadOnlyList<RecoveryJournalSourceEvidence> exactCurrentEvidence,
            IReadOnlyList<RecoveryJournalSourceEvidence> otherEndpointEvidence,
            string endpointIp,
            int endpointPort,
            LMCDiagnosticCapabilities currentCapabilities)
        {
            var text = new StringBuilder();
            text.AppendLine(
                TranslateUiText(
                    "This action archives the exact original recovery journal "
                        + "bytes, then marks only the listed old-PLC records Resolved."));
            text.AppendLine(
                TranslateUiText(
                    "It does NOT prove whether any old command succeeded or "
                        + "failed. Every listed outcome remains UNKNOWN."));
            if (staleEvidence.Any(item =>
                item.EndpointEvidenceKind
                    == RecoveryEndpointEvidenceKind
                        .OperatorClassifiedLegacyEndpoint))
            {
                text.AppendLine(
                    TranslateUiText(
                        "LEGACY DIAGNOSTICS WARNING: the original mutation "
                            + "journal contains no PLC endpoint. You are "
                            + "explicitly classifying the current quarantined "
                            + "endpoint as its target; this classification is "
                            + "stored separately from the exact original bytes."));
            }
            text.AppendLine(
                TranslateUiText(
                    "No Motion, Power, SDO, Write, replay, or cleanup command is "
                        + "sent. A second read-only Capabilities query will recheck "
                        + "the same TCP session and PLC identity after confirmation."));
            text.AppendLine(
                TranslateUiText(
                    "After success, this quarantined connection closes and the "
                        + "application must restart."));
            text.AppendLine();
            text.Append(TranslateUiText("Current: "))
                .Append(endpointIp)
                .Append(':')
                .Append(endpointPort.ToString(CultureInfo.InvariantCulture))
                .Append(", Build=0x")
                .Append(currentCapabilities.DiagnosticsBuild.ToString("X8"))
                .Append(", BootId=0x")
                .Append(currentCapabilities.DiagnosticsBootId.ToString("X8"))
                .Append(", MapRevision=0x")
                .Append(currentCapabilities.MapRevision.ToString("X8"))
                .AppendLine();
            text.AppendLine(
                TranslateUiText(
                    "RETIRE STALE - records to archive and resolve:"));
            foreach (var item in staleEvidence)
            {
                AppendRecoveryRetirementEvidenceLine(text, item);
            }
            text.AppendLine();
            text.AppendLine(
                TranslateUiText(
                    "KEEP EXACT CURRENT - records left active for exact recovery:"));
            if (exactCurrentEvidence.Count == 0)
            {
                text.AppendLine(TranslateUiText("- none"));
            }
            else
            {
                foreach (var item in exactCurrentEvidence)
                {
                    AppendRecoveryRetirementEvidenceLine(text, item);
                }
            }
            text.AppendLine();
            text.AppendLine(
                TranslateUiText(
                    "KEEP OTHER ENDPOINT - records left active for their recorded endpoint:"));
            if (otherEndpointEvidence.Count == 0)
            {
                text.AppendLine(TranslateUiText("- none"));
            }
            else
            {
                foreach (var item in otherEndpointEvidence)
                {
                    AppendRecoveryRetirementEvidenceLine(text, item);
                }
            }
            text.AppendLine();
            text.Append(
                TranslateUiText(
                    "Proceed only if you independently verified the physical "
                        + "machine and drive state."));
            return text.ToString();
        }

        private static void AppendRecoveryRetirementEvidenceLine(
            StringBuilder text,
            RecoveryJournalSourceEvidence evidence)
        {
            text.Append("- ")
                .Append(evidence.Owner)
                .Append(" | ")
                .Append(evidence.TargetName)
                .Append(" Ref=")
                .Append(evidence.TargetReference.ToString(
                    CultureInfo.InvariantCulture))
                .Append(" | ")
                .Append(evidence.Operation)
                .Append(" | Record=")
                .Append(evidence.RecordIdentity.ToString("D"))
                .Append(" | Stored Build=0x")
                .Append(evidence.DiagnosticsBuild.ToString("X8"))
                .Append(", BootId=0x")
                .Append(evidence.DiagnosticsBootId.ToString("X8"))
                .Append(", MapRevision=0x")
                .Append(evidence.MapRevision.ToString("X8"))
                .Append(" | SHA256=")
                .Append(evidence.OriginalSha256)
                .AppendLine();
        }

        private void RefreshRecoveryIdentityRetirementUi()
        {
            if (PanelRecoveryIdentityRetirement == null)
            {
                return;
            }

            var quarantineVisible =
                IsRecoveryIdentityReadOnlyExitPermitted()
                || recoveryRecordRetirementRestartRequired;
            PanelRecoveryIdentityRetirement.Visibility = quarantineVisible
                ? Visibility.Visible
                : Visibility.Collapsed;
            if (!quarantineVisible)
            {
                CheckConfirmStaleRecoveryRetirement.IsChecked = false;
                CheckConfirmStaleRecoveryRetirement.IsEnabled = false;
                ButtonArchiveAndRetireStaleRecovery.IsEnabled = false;
                return;
            }

            TextRecoveryIdentityRetirementSnapshot.Text =
                BuildRecoveryIdentityRetirementSnapshot();
            var canPrepareRetirement =
                CanRetireStaleRecoveryEvidenceFromUi();
            CheckConfirmStaleRecoveryRetirement.IsEnabled =
                canPrepareRetirement;
            ButtonArchiveAndRetireStaleRecovery.IsEnabled =
                CheckConfirmStaleRecoveryRetirement.IsChecked == true
                && canPrepareRetirement;
        }

        private bool CanRetireStaleRecoveryEvidenceFromUi()
        {
            if (recoveryRecordRetirementRestartRequired
                || operationRunning
                || safetyCommandRunning
                || safetyMonitorCount != 0
                || qualificationRunning
                || (diagnosticOperationTicket != null
                    && (diagnosticOperationStatus == null
                        || !diagnosticOperationStatus.IsTerminal))
                || RecoveryRecordRetirementLedgerUnavailable
                || AxisPowerOnRecoveryJournalUnavailable
                || AxisCommandRecoveryJournalUnavailable
                || MotionUncertaintyJournalUnavailable
                || GroupProfileLockRecoveryJournalUnavailable
                || GroupPowerRecoveryJournalUnavailable
                || GroupResetRecoveryJournalUnavailable
                || AxisQualificationRecoveryJournalUnavailable
                || DiagnosticsMutationJournalUnavailable)
            {
                return false;
            }

            var currentConnection = connection;
            if (currentConnection == null
                || !currentConnection.IsConnected
                || !IsRecoveryIdentityReadOnlyConnection(currentConnection))
            {
                return false;
            }

            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || !capabilities.IsBoundTo(
                    currentConnection.Diagnostics,
                    currentConnection.SessionGeneration))
            {
                return false;
            }

            List<RecoveryRetirementMetadata> records;
            try
            {
                records = GetActiveRecoveryRetirementMetadata();
                var endpointIp = RequiredConnectedRemoteIp();
                var endpointPort = RequiredConnectedRemotePort();
                return records.Count > 0
                    && (!records.Any(item =>
                            RecoveryRetirementEndpointMatches(
                                item,
                                endpointIp,
                                endpointPort)
                            && item.DiagnosticsBuild != 0)
                        || capabilities.DiagnosticsBuild != 0)
                    && records.Any(item =>
                        RecoveryRetirementEndpointMatches(
                            item,
                            endpointIp,
                            endpointPort)
                        && !RecoveryRetirementIdentityMatches(
                            item,
                            capabilities));
            }
            catch
            {
                return false;
            }
        }

        private static bool RecoveryRetirementIdentityMatches(
            RecoveryRetirementMetadata metadata,
            LMCDiagnosticCapabilities currentCapabilities)
        {
            return metadata != null
                && currentCapabilities != null
                && (metadata.DiagnosticsBuild == 0
                    || metadata.DiagnosticsBuild
                        == currentCapabilities.DiagnosticsBuild)
                && metadata.DiagnosticsBootId
                    == currentCapabilities.DiagnosticsBootId
                && metadata.MapRevision == currentCapabilities.MapRevision;
        }

        private static bool RecoveryRetirementEndpointMatches(
            RecoveryRetirementMetadata metadata,
            string endpointIp,
            int endpointPort)
        {
            return metadata != null
                && string.Equals(
                    metadata.EndpointIp,
                    endpointIp,
                    StringComparison.Ordinal)
                && metadata.EndpointPort == endpointPort;
        }

        private string BuildRecoveryIdentityRetirementSnapshot()
        {
            var text = new StringBuilder();
            if (recoveryRecordRetirementRestartRequired)
            {
                text.AppendLine(
                    "Restart required. The retired session cannot be reused.");
            }

            var currentConnection = connection;
            var capabilities = diagnosticCapabilities;
            if (currentConnection != null
                && currentConnection.IsConnected
                && capabilities != null
                && capabilities.DiagnosticsBootId != 0
                && capabilities.MapRevision != 0)
            {
                text.Append("Current PLC: ")
                    .Append(RequiredConnectedRemoteIp())
                    .Append(':')
                    .Append(RequiredConnectedRemotePort().ToString(
                        CultureInfo.InvariantCulture))
                    .Append(" | Build=0x")
                    .Append(capabilities.DiagnosticsBuild.ToString("X8"))
                    .Append(" | BootId=0x")
                    .Append(capabilities.DiagnosticsBootId.ToString("X8"))
                    .Append(" | MapRevision=0x")
                    .Append(capabilities.MapRevision.ToString("X8"))
                    .AppendLine();
            }
            else
            {
                text.AppendLine("Current PLC identity: unavailable");
            }

            text.AppendLine("Active durable recovery records:");
            var records = GetActiveRecoveryRetirementMetadata();
            if (records.Count == 0)
            {
                text.AppendLine("- none");
            }
            else
            {
                foreach (var item in records)
                {
                    var disposition = "KEEP OTHER ENDPOINT";
                    if (currentConnection != null
                        && currentConnection.IsConnected
                        && capabilities != null
                        && string.Equals(
                            item.EndpointIp,
                            RequiredConnectedRemoteIp(),
                            StringComparison.Ordinal)
                        && item.EndpointPort == RequiredConnectedRemotePort())
                    {
                        disposition = RecoveryRetirementIdentityMatches(
                                item,
                                capabilities)
                            ? "KEEP EXACT CURRENT"
                            : "RETIRE STALE";
                    }

                    text.Append("- ")
                        .Append(disposition)
                        .Append(" | ")
                        .Append(item.Owner)
                        .Append(" | ")
                        .Append(item.TargetName)
                        .Append(" Ref=")
                        .Append(item.TargetReference.ToString(
                            CultureInfo.InvariantCulture))
                        .Append(" | ")
                        .Append(item.Operation)
                        .Append(" | Record=")
                        .Append(item.RecordIdentity.ToString("D"))
                        .Append(" | Build=0x")
                        .Append(item.DiagnosticsBuild.ToString("X8"))
                        .Append(" | BootId=0x")
                        .Append(item.DiagnosticsBootId.ToString("X8"))
                        .Append(" | MapRevision=0x")
                        .Append(item.MapRevision.ToString("X8"))
                        .AppendLine();
                }
            }

            text.Append("Other blockers not retired here: DiagnosticsMutation=")
                .Append(!HasActiveDiagnosticsMutationJournalRecord
                    ? "none"
                    : !HasRetirableLegacyDiagnosticsMutationRecord
                        ? "active-nonretirable"
                        : CanListLegacyDiagnosticsMutationForRetirement()
                            ? "none"
                            : "active-endpoint-unbound/reconnect-required")
                .Append(", RecorderDouble=")
                .Append(HasActiveRecorderDoubleRecoveryJournalRecord
                    ? "active"
                    : "none")
                .AppendLine();
            if (RecoveryRecordRetirementLedgerUnavailable)
            {
                text.Append("Retirement ledger: unavailable | ")
                    .Append(GetRecoveryRecordRetirementUnavailableGuidance())
                    .AppendLine();
            }
            else
            {
                text.Append("Immutable archive: ")
                    .Append(recoveryRecordRetirementLedger.DirectoryPath)
                    .AppendLine();
            }

            text.Append(
                "Retirement never proves the old result and never sends "
                + "Motion, Power, SDO, Write, replay, or cleanup.");
            return text.ToString();
        }

        private List<RecoveryRetirementMetadata>
            GetActiveRecoveryRetirementMetadata()
        {
            var values = new List<RecoveryRetirementMetadata>(8);
            if (axisPowerOnRecoveryJournal != null)
            {
                var record = axisPowerOnRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    values.Add(new RecoveryRetirementMetadata(
                        RecoveryRecordOwner.AxisPower,
                        record.Identity,
                        record.EndpointIp,
                        record.EndpointPort,
                        0,
                        record.DiagnosticsBootId,
                        record.MapRevision,
                        record.AxisName,
                        record.AxisReference,
                        record.ExpectedPowerOn ? "PowerOn" : "PowerOff"));
                }
            }
            if (axisCommandRecoveryJournal != null)
            {
                var record = axisCommandRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    values.Add(new RecoveryRetirementMetadata(
                        RecoveryRecordOwner.AxisCommand,
                        record.Identity,
                        record.EndpointIp,
                        record.EndpointPort,
                        0,
                        record.DiagnosticsBootId,
                        record.MapRevision,
                        record.AxisName,
                        record.AxisReference,
                        record.Operation.ToString()));
                }
            }
            if (motionUncertaintyJournal != null)
            {
                var record = motionUncertaintyJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    values.Add(new RecoveryRetirementMetadata(
                        RecoveryRecordOwner.Motion,
                        record.Identity,
                        record.EndpointIp,
                        record.EndpointPort,
                        0,
                        record.DiagnosticsBootId,
                        record.MapRevision,
                        record.TargetName,
                        record.TargetReference,
                        record.Operation));
                }
            }
            if (groupProfileLockRecoveryJournal != null)
            {
                var record = groupProfileLockRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    values.Add(new RecoveryRetirementMetadata(
                        RecoveryRecordOwner.GroupProfileLock,
                        record.Identity,
                        record.EndpointIp,
                        record.EndpointPort,
                        0,
                        record.DiagnosticsBootId,
                        record.MapRevision,
                        record.GroupName,
                        record.GroupReference,
                        record.ExpectedProfileLocked ? "Lock" : "Unlock"));
                }
            }
            if (groupPowerRecoveryJournal != null)
            {
                var record = groupPowerRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    values.Add(new RecoveryRetirementMetadata(
                        RecoveryRecordOwner.GroupPower,
                        record.Identity,
                        record.EndpointIp,
                        record.EndpointPort,
                        0,
                        record.DiagnosticsBootId,
                        record.MapRevision,
                        record.GroupName,
                        record.GroupReference,
                        record.ExpectedPowerOn ? "PowerOn" : "PowerOff"));
                }
            }
            if (groupResetRecoveryJournal != null)
            {
                var record = groupResetRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    values.Add(new RecoveryRetirementMetadata(
                        RecoveryRecordOwner.GroupReset,
                        record.Identity,
                        record.PlcIp,
                        record.PlcTcpPort,
                        record.DiagnosticsBuild,
                        record.DiagnosticsBootId,
                        record.MapRevision,
                        record.GroupName,
                        record.GroupReference,
                        "Reset/StableErrorClearance"));
                }
            }
            if (axisQualificationRecoveryJournal != null)
            {
                var record = axisQualificationRecoveryJournal.CurrentRecord;
                if (record != null && record.IsActive)
                {
                    values.Add(new RecoveryRetirementMetadata(
                        RecoveryRecordOwner.AxisQualification,
                        record.Identity,
                        record.EndpointIp,
                        record.EndpointPort,
                        record.DiagnosticsBuild,
                        record.DiagnosticsBootId,
                        record.MapRevision,
                        record.AxisName,
                        record.AxisReference,
                        "SingleAxisQualification/" + record.Stage));
                }
            }
            if (CanListLegacyDiagnosticsMutationForRetirement())
            {
                var record = diagnosticsMutationJournal.CurrentRecord;
                values.Add(new RecoveryRetirementMetadata(
                    RecoveryRecordOwner.DiagnosticsMutation,
                    record.Identity,
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    0,
                    record.DiagnosticsBootId,
                    record.IdentityRevision,
                    record.TargetText,
                    record.SdoWriteMetadata.SlaveReference,
                    "SdoWrite/OutcomeUnverified/"
                        + "LEGACY_ENDPOINT_OPERATOR_CLASSIFIED"));
            }
            return values;
        }

        private bool CanListLegacyDiagnosticsMutationForRetirement()
        {
            var currentConnection = connection;
            return HasRetirableLegacyDiagnosticsMutationRecord
                && currentConnection != null
                && currentConnection.IsConnected
                && IsRecoveryIdentityReadOnlyConnection(currentConnection);
        }

        private string GetRecoveryRecordRetirementUnavailableGuidance()
        {
            if (DiagnosticsMutationJournalUnavailable)
            {
                return GetDiagnosticsMutationJournalUnavailableGuidance();
            }
            if (!string.IsNullOrEmpty(
                    recoveryRecordRetirementLedgerRuntimeError))
            {
                return recoveryRecordRetirementLedgerRuntimeError;
            }
            if (!string.IsNullOrEmpty(
                    recoveryRecordRetirementLedgerOpenError))
            {
                return recoveryRecordRetirementLedgerOpenError;
            }
            return RecoveryRecordRetirementLedgerUnavailable
                ? "No retirement ledger is open."
                : "The immutable retirement ledger is available.";
        }

        private static string GetRecoveryRetirementOperator()
        {
            var user = Environment.UserName;
            var domain = Environment.UserDomainName;
            if (string.IsNullOrWhiteSpace(user))
            {
                user = "unknown-windows-user";
            }
            return string.IsNullOrWhiteSpace(domain)
                ? user
                : domain + "\\" + user;
        }

        private static DateTime MonotonicRetirementUtcNow(
            DateTime notBeforeUtc)
        {
            var now = DateTime.UtcNow;
            return now < notBeforeUtc ? notBeforeUtc : now;
        }

        private sealed class RecoveryRetirementMetadata
        {
            internal RecoveryRetirementMetadata(
                RecoveryRecordOwner owner,
                Guid recordIdentity,
                string endpointIp,
                int endpointPort,
                uint diagnosticsBuild,
                uint diagnosticsBootId,
                uint mapRevision,
                string targetName,
                ushort targetReference,
                string operation)
            {
                Owner = owner;
                RecordIdentity = recordIdentity;
                EndpointIp = endpointIp;
                EndpointPort = endpointPort;
                DiagnosticsBuild = diagnosticsBuild;
                DiagnosticsBootId = diagnosticsBootId;
                MapRevision = mapRevision;
                TargetName = targetName;
                TargetReference = targetReference;
                Operation = operation;
            }

            internal RecoveryRecordOwner Owner { get; private set; }
            internal Guid RecordIdentity { get; private set; }
            internal string EndpointIp { get; private set; }
            internal int EndpointPort { get; private set; }
            internal uint DiagnosticsBuild { get; private set; }
            internal uint DiagnosticsBootId { get; private set; }
            internal uint MapRevision { get; private set; }
            internal string TargetName { get; private set; }
            internal ushort TargetReference { get; private set; }
            internal string Operation { get; private set; }
        }
    }
}

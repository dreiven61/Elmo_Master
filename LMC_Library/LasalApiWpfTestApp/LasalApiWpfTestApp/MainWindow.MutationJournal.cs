using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly string diagnosticsMutationJournalDirectoryPath;
        private DiagnosticsMutationJournal diagnosticsMutationJournal;
        private string diagnosticsMutationJournalOpenError;
        private string diagnosticsMutationJournalRuntimeError;
        private bool diagnosticsMutationRecoveredAtStartup;
        private LMCConnection diagnosticsMutationSessionOwner;
        private long diagnosticsMutationSessionGeneration;
        private Guid? diagnosticsMutationRestartRecoveryAttemptedIdentity;

        private bool HasActiveDiagnosticsMutationJournalRecord
        {
            get
            {
                return diagnosticsMutationJournal != null
                    && diagnosticsMutationJournal.HasActiveRecord;
            }
        }

        private bool DiagnosticsMutationJournalCanArm
        {
            get
            {
                return diagnosticsMutationJournal != null
                    && string.IsNullOrEmpty(
                        diagnosticsMutationJournalOpenError)
                    && string.IsNullOrEmpty(
                        diagnosticsMutationJournalRuntimeError)
                    && !diagnosticsMutationJournal.HasActiveRecord;
            }
        }

        private bool DiagnosticsMutationJournalUnavailable
        {
            get
            {
                return diagnosticsMutationJournal == null
                    || !string.IsNullOrEmpty(
                        diagnosticsMutationJournalOpenError)
                    || !string.IsNullOrEmpty(
                        diagnosticsMutationJournalRuntimeError);
            }
        }

        private void InitializeDiagnosticsMutationJournal()
        {
            try
            {
                diagnosticsMutationJournal =
                    diagnosticsMutationJournalDirectoryPath == null
                        ? DiagnosticsMutationJournal.OpenDefault()
                        : DiagnosticsMutationJournal.Open(
                            diagnosticsMutationJournalDirectoryPath);
                diagnosticsMutationRecoveredAtStartup =
                    diagnosticsMutationJournal.HasActiveRecord;
                diagnosticsMutationJournalOpenError = null;
                diagnosticsMutationJournalRuntimeError = null;
            }
            catch (Exception error)
            {
                diagnosticsMutationJournal = null;
                diagnosticsMutationRecoveredAtStartup = false;
                diagnosticsMutationJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
            }

            RefreshDiagnosticsMutationJournalUi();
        }

        private void DisposeDiagnosticsMutationJournal()
        {
            var journal = diagnosticsMutationJournal;
            diagnosticsMutationJournal = null;
            diagnosticsMutationSessionOwner = null;
            diagnosticsMutationSessionGeneration = 0;
            diagnosticsMutationRestartRecoveryAttemptedIdentity = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private void EnsureDiagnosticsMutationJournalCanArm(
            string operation)
        {
            if (diagnosticsMutationJournal == null)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the durable mutation journal is unavailable. "
                    + (string.IsNullOrEmpty(
                            diagnosticsMutationJournalOpenError)
                        ? "No journal was opened."
                        : diagnosticsMutationJournalOpenError));
            }

            if (!string.IsNullOrEmpty(
                    diagnosticsMutationJournalRuntimeError))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the durable mutation journal faulted during this process. "
                    + diagnosticsMutationJournalRuntimeError);
            }

            if (diagnosticsMutationJournal.HasActiveRecord)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked by unresolved durable mutation evidence. "
                    + FormatDiagnosticsMutationRecord(
                        diagnosticsMutationJournal.CurrentRecord));
            }
        }

        private void ArmSdoWriteMutationJournal(
            LMCSdoRequest request,
            LMCConnection ownerConnection,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            if (request == null || !request.IsWrite)
            {
                throw new ArgumentException(
                    "A durable SDO mutation record requires a Write request.",
                    "request");
            }

            EnsureDiagnosticsMutationJournalCanArm("SDO Write");
            try
            {
                diagnosticsMutationJournal.Arm(
                    DiagnosticsMutationKind.SdoWrite,
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    diagnosticsBootId,
                    mapRevision,
                    GetDiagnosticsMutationSessionGeneration(ownerConnection),
                    "Slave="
                        + request.SlaveReference.ToString(
                            CultureInfo.InvariantCulture)
                        + ",Object=0x"
                        + request.ObjectIndex.ToString("X4")
                        + ",SubIndex="
                        + request.SubIndex.ToString(
                            CultureInfo.InvariantCulture)
                        + ",Type="
                        + request.ValueType
                        + ",Length="
                        + request.DataLength.ToString(
                            CultureInfo.InvariantCulture),
                    "WriteData=" + BitConverter.ToString(request.WriteData),
                    new DiagnosticsSdoWriteMutationMetadata(
                        request.SlaveReference,
                        request.ObjectIndex,
                        request.SubIndex,
                        request.ValueType,
                        request.DataLength,
                        request.TimeoutCycles,
                        request.WriteData));
            }
            catch (Exception error)
            {
                RecordDiagnosticsMutationJournalRuntimeFault(error);
                throw;
            }
            RefreshDiagnosticsMutationJournalUi();
        }

        private void ArmDigitalOutputMutationJournal(
            LMCDigitalOutputWriteRequest request,
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIOValue sourceShadow,
            LMCConnection ownerConnection)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (sourceShadow == null)
            {
                throw new ArgumentNullException("sourceShadow");
            }

            EnsureDiagnosticsMutationJournalCanArm(
                "Digital Output Write");
            var expectedFullValue =
                (sourceShadow.Value & ~request.Mask)
                | (request.Value & request.Mask);
            try
            {
                diagnosticsMutationJournal.Arm(
                    DiagnosticsMutationKind.DigitalOutputWrite,
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    request.SourceDiagnosticsBootId,
                    request.TopologyRevision,
                    GetDiagnosticsMutationSessionGeneration(ownerConnection),
                    "Node=0x"
                        + entry.NodeId.ToString("X8")
                        + ",IOReference=0x"
                        + request.IOReference.ToString("X8"),
                    "FullValue=0x"
                        + expectedFullValue.ToString("X16")
                        + ",Mask=0x"
                        + request.Mask.ToString("X16")
                        + ",SourceRevision=0x"
                        + request.ExpectedOutputRevision.ToString("X8"));
            }
            catch (Exception error)
            {
                RecordDiagnosticsMutationJournalRuntimeFault(error);
                throw;
            }
            RefreshDiagnosticsMutationJournalUi();
        }

        private void TransitionDiagnosticsMutationJournal(
            DiagnosticsMutationKind kind,
            DiagnosticsMutationState state,
            uint ticketId)
        {
            var current = RequireActiveDiagnosticsMutationRecord(kind);
            if (current.State == state)
            {
                if (ticketId != 0
                    && current.TicketId != 0
                    && current.TicketId != ticketId)
                {
                    throw new InvalidOperationException(
                        "The durable mutation ticket does not match the active record.");
                }

                return;
            }

            if ((current.State == DiagnosticsMutationState.OutcomeUnverified
                    || current.State
                        == DiagnosticsMutationState.ReadbackMismatch)
                && state == DiagnosticsMutationState.OutcomeUnverified)
            {
                return;
            }

            try
            {
                diagnosticsMutationJournal.Transition(
                    current.Identity,
                    state,
                    GetDiagnosticsMutationTransitionUtc(current),
                    ticketId);
            }
            catch (Exception error)
            {
                RecordDiagnosticsMutationJournalRuntimeFault(error);
                throw;
            }
            RefreshDiagnosticsMutationJournalUi();
        }

        private void ResolveDiagnosticsMutationJournal(
            DiagnosticsMutationKind kind)
        {
            var current = RequireActiveDiagnosticsMutationRecord(kind);
            try
            {
                diagnosticsMutationJournal.Resolve(
                    current.Identity,
                    GetDiagnosticsMutationTransitionUtc(current));
            }
            catch (Exception error)
            {
                RecordDiagnosticsMutationJournalRuntimeFault(error);
                throw;
            }
            diagnosticsMutationRecoveredAtStartup = false;
            RefreshDiagnosticsMutationJournalUi();
        }

        private DiagnosticsMutationRecord RequireActiveDiagnosticsMutationRecord(
            DiagnosticsMutationKind kind)
        {
            if (diagnosticsMutationJournal == null)
            {
                throw new InvalidOperationException(
                    "The durable diagnostics mutation journal is unavailable.");
            }

            var current = diagnosticsMutationJournal.CurrentRecord;
            if (current == null || !current.IsActive)
            {
                throw new InvalidOperationException(
                    "No active durable diagnostics mutation record exists.");
            }

            if (current.Kind != kind)
            {
                throw new InvalidOperationException(
                    "The active durable mutation kind is "
                    + current.Kind
                    + ", not "
                    + kind
                    + ".");
            }

            return current;
        }

        private void MarkSdoWriteMutationAccepted(LMCOperationTicket ticket)
        {
            if (ticket == null
                || ticket.OperationKind != LMCOperationKind.SDOWrite)
            {
                return;
            }

            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.AcceptedPendingTerminal,
                ticket.TicketId);
        }

        private void MarkSdoWriteMutationOutcomeUnverified()
        {
            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.OutcomeUnverified,
                0);
        }

        private void MarkSdoWriteMutationTerminalSuccess(
            LMCOperationTicket ticket)
        {
            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.TerminalSuccessPendingReadback,
                ticket.TicketId);
        }

        private void MarkSdoWriteMutationReadbackUnverified(
            bool exactComparableValueMismatch)
        {
            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.SdoWrite,
                exactComparableValueMismatch
                    ? DiagnosticsMutationState.ReadbackMismatch
                    : DiagnosticsMutationState.OutcomeUnverified,
                0);
        }

        private void MarkDigitalOutputMutationAccepted(
            LMCOperationTicket ticket)
        {
            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.DigitalOutputWrite,
                DiagnosticsMutationState.AcceptedPendingTerminal,
                ticket.TicketId);
        }

        private void MarkDigitalOutputMutationOutcomeUnverified()
        {
            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.DigitalOutputWrite,
                DiagnosticsMutationState.OutcomeUnverified,
                0);
        }

        private void MarkDigitalOutputMutationTerminalSuccess(
            LMCOperationTicket ticket)
        {
            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.DigitalOutputWrite,
                DiagnosticsMutationState.TerminalSuccessPendingReadback,
                ticket.TicketId);
        }

        private void MarkDigitalOutputMutationReadbackMismatch()
        {
            TransitionDiagnosticsMutationJournal(
                DiagnosticsMutationKind.DigitalOutputWrite,
                DiagnosticsMutationState.ReadbackMismatch,
                0);
        }

        private void MarkActiveDiagnosticsMutationConnectionLost()
        {
            if (!HasActiveDiagnosticsMutationJournalRecord)
            {
                return;
            }

            var current = diagnosticsMutationJournal.CurrentRecord;
            if (current.Kind == DiagnosticsMutationKind.SdoWrite)
            {
                MarkSdoWriteMutationOutcomeUnverified();
            }
            else if (current.Kind
                == DiagnosticsMutationKind.DigitalOutputWrite)
            {
                MarkDigitalOutputMutationOutcomeUnverified();
            }
        }

        private long GetDiagnosticsMutationSessionGeneration(
            LMCConnection ownerConnection)
        {
            if (ownerConnection == null)
            {
                throw new ArgumentNullException("ownerConnection");
            }

            if (!ReferenceEquals(
                    diagnosticsMutationSessionOwner,
                    ownerConnection))
            {
                // The SDK session generation is intentionally internal. This
                // app-owned positive value is forensic process metadata only;
                // BootId/revision and SDK provenance checks remain authoritative.
                diagnosticsMutationSessionOwner = ownerConnection;
                diagnosticsMutationSessionGeneration = DateTime.UtcNow.Ticks;
                if (diagnosticsMutationSessionGeneration <= 0)
                {
                    diagnosticsMutationSessionGeneration = 1;
                }
            }

            return diagnosticsMutationSessionGeneration;
        }

        private static DateTime GetDiagnosticsMutationTransitionUtc(
            DiagnosticsMutationRecord current)
        {
            var now = DateTime.UtcNow;
            return now < current.UpdatedUtc ? current.UpdatedUtc : now;
        }

        private string GetDiagnosticsMutationJournalGuidance()
        {
            if (!HasActiveDiagnosticsMutationJournalRecord)
            {
                return string.Empty;
            }

            var record = diagnosticsMutationJournal.CurrentRecord;
            var staleSdoReadback = d5SdoPendingWriteReadback != null
                && !d5SdoPendingWriteReadback
                    .MatchesOwnerCurrentSession(connection);
            return "Durable evidence remains: "
                + FormatDiagnosticsMutationRecord(record)
                + (diagnosticsMutationRecoveredAtStartup
                    ? " Physically verify the target, then use Persisted Mutation Recovery acknowledgement. No command will be replayed."
                    : staleSdoReadback
                        ? " The exact SDO readback cannot run in the current connection session. Physically verify the target and PLC state, then use Persisted Mutation Recovery acknowledgement. No command will be replayed."
                    : " Complete the current ticket/readback workflow. No command will be replayed.");
        }

        private string GetDiagnosticsMutationJournalUnavailableGuidance()
        {
            var detail = diagnosticsMutationJournalRuntimeError
                ?? diagnosticsMutationJournalOpenError
                ?? "No durable mutation journal was opened.";
            var closeGuidance = HasActiveDiagnosticsMutationJournalRecord
                ? " Active durable evidence remains, so connection/window close stays blocked until that evidence is resolved."
                : " No active durable evidence remains, so normal connection/window exit is available.";
            return "The durable mutation journal is unavailable. New live/mutation commands and tracked D5 reads are disabled; ordinary non-D5 read-only inspection, Stop, PowerOff, and Group Stop remain available."
                + closeGuidance
                + " "
                + detail;
        }

        private void RecordDiagnosticsMutationJournalRuntimeFault(
            Exception error)
        {
            diagnosticsMutationJournalRuntimeError =
                error.GetType().Name + ": " + error.Message;
            RefreshDiagnosticsMutationJournalUi();
        }

        private static string FormatDiagnosticsMutationRecord(
            DiagnosticsMutationRecord record)
        {
            if (record == null)
            {
                return "No mutation record.";
            }

            return "Kind="
                + record.Kind
                + ", State="
                + record.State
                + ", Ticket="
                + (record.TicketId == 0
                    ? "UNKNOWN"
                    : record.TicketId.ToString(
                        CultureInfo.InvariantCulture))
                + ", BootId=0x"
                + record.DiagnosticsBootId.ToString("X8")
                + ", Revision=0x"
                + record.IdentityRevision.ToString("X8")
                + ", Target="
                + record.TargetText
                + ", Expected="
                + record.ExpectedText
                + ", UpdatedUtc="
                + record.UpdatedUtc.ToString(
                    "O",
                    CultureInfo.InvariantCulture)
                + ".";
        }

        private void RefreshDiagnosticsMutationJournalUi()
        {
            if (TextPersistedMutationStatus == null)
            {
                return;
            }

            if (DiagnosticsMutationJournalUnavailable)
            {
                TextPersistedMutationStatus.Text =
                    "MUTATION JOURNAL UNAVAILABLE. "
                    + GetDiagnosticsMutationJournalUnavailableGuidance()
                    + (HasActiveDiagnosticsMutationJournalRecord
                        ? " Active evidence remains: "
                            + FormatDiagnosticsMutationRecord(
                                diagnosticsMutationJournal.CurrentRecord)
                        : string.Empty);
            }
            else if (diagnosticsMutationJournal.HasActiveRecord)
            {
                TextPersistedMutationStatus.Text =
                    (diagnosticsMutationRecoveredAtStartup
                        ? "RECOVERED UNRESOLVED MUTATION. "
                        : "ACTIVE DURABLE MUTATION. ")
                    + FormatDiagnosticsMutationRecord(
                        diagnosticsMutationJournal.CurrentRecord)
                    + " Automatic replay is disabled.";
            }
            else
            {
                TextPersistedMutationStatus.Text =
                    "Mutation journal ready. No unresolved durable write record.";
            }
        }

        private void UpdateDiagnosticsMutationJournalUiState(bool idle)
        {
            RefreshDiagnosticsMutationJournalUi();
            if (CheckPersistedMutationPhysicallyVerified == null)
            {
                return;
            }

            var recoveryIdentityReadOnly =
                IsRecoveryIdentityReadOnlyExitPermitted();
            var startupRecoveryAvailable = !recoveryIdentityReadOnly
                && StaleSdoWriteReadbackRecoveryPolicy
                    .CanAcknowledgeStartupRecovery(
                        idle,
                        !DiagnosticsMutationJournalUnavailable,
                        diagnosticsMutationRecoveredAtStartup,
                        HasActiveDiagnosticsMutationJournalRecord,
                        HasPendingD5SdoWriteReadback,
                        HasD5SdoTicketOrQuarantine,
                        HasUnresolvedDigitalOutputWrite);
            var staleSdoRecoveryAvailable = !recoveryIdentityReadOnly
                && CanAcknowledgeStaleSdoWriteReadback(idle);
            var exactRestartRecoveryAvailable = !recoveryIdentityReadOnly
                && CanAttemptExactSdoRestartRecovery(idle);
            var canAcknowledge = startupRecoveryAvailable
                || staleSdoRecoveryAvailable;
            CheckPersistedMutationPhysicallyVerified.IsEnabled =
                canAcknowledge && !exactRestartRecoveryAvailable;
            ButtonAcknowledgePersistedMutation.IsEnabled =
                exactRestartRecoveryAvailable
                || StaleSdoWriteReadbackRecoveryPolicy.CanConfirm(
                    canAcknowledge,
                    CheckPersistedMutationPhysicallyVerified.IsChecked
                        == true);
            ButtonAcknowledgePersistedMutation.Content =
                exactRestartRecoveryAvailable
                    ? "Verify Recovered SDO Readback"
                    : staleSdoRecoveryAvailable
                    ? "Acknowledge Stale SDO Write"
                    : "Acknowledge Recovered Mutation";
        }

        private bool CanAttemptExactSdoRestartRecovery(bool idle)
        {
            var currentConnection = connection;
            var record = diagnosticsMutationJournal == null
                ? null
                : diagnosticsMutationJournal.CurrentRecord;
            var alreadyAttempted = record != null
                && diagnosticsMutationRestartRecoveryAttemptedIdentity
                    .HasValue
                && diagnosticsMutationRestartRecoveryAttemptedIdentity.Value
                    == record.Identity;
            if (!DiagnosticsSdoRestartRecoveryPolicy.CanAttempt(
                    record,
                    diagnosticsMutationRecoveredAtStartup,
                    idle,
                    currentConnection != null
                        && currentConnection.IsConnected,
                    HasPendingD5SdoWriteReadback,
                    HasD5SdoTicketOrQuarantine,
                    HasUnresolvedDigitalOutputWrite,
                    alreadyAttempted))
            {
                return false;
            }

            return IsExactApprovedSdoWriteTarget(
                record.SdoWriteMetadata,
                currentConnection.Diagnostics
                    .GetApprovedSdoWriteTargets());
        }

        private static bool IsExactApprovedSdoWriteTarget(
            DiagnosticsSdoWriteMutationMetadata metadata,
            IReadOnlyList<LMCSdoWriteTarget> targets)
        {
            if (metadata == null || targets == null)
            {
                return false;
            }

            var expectedData = metadata.ExpectedWriteData;
            var rawValue = (uint)expectedData[0]
                | ((uint)expectedData[1] << 8)
                | ((uint)expectedData[2] << 16)
                | ((uint)expectedData[3] << 24);
            var integerValue = metadata.ValueType
                    == LMCSignalValueType.Int32
                ? unchecked((long)(int)rawValue)
                : (long)rawValue;
            for (var index = 0; index < targets.Count; index++)
            {
                var target = targets[index];
                if (target == null
                    || target.SlaveReference != metadata.SlaveReference
                    || target.ObjectIndex != metadata.ObjectIndex
                    || target.SubIndex != metadata.SubIndex
                    || target.ValueType != metadata.ValueType
                    || target.DataLength != metadata.DataLength)
                {
                    continue;
                }

                LMCSdoRequest canonicalRequest;
                try
                {
                    canonicalRequest = target.CreateRequest(
                        integerValue,
                        metadata.TimeoutCycles);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (ByteArraysEqual(
                        expectedData,
                        canonicalRequest.WriteData))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }

        private bool CanAcknowledgeStaleSdoWriteReadback(bool idle)
        {
            var pending = d5SdoPendingWriteReadback;
            var record = diagnosticsMutationJournal == null
                ? null
                : diagnosticsMutationJournal.CurrentRecord;
            return StaleSdoWriteReadbackRecoveryPolicy.CanAcknowledge(
                idle,
                !DiagnosticsMutationJournalUnavailable,
                pending != null,
                pending != null
                    && pending.MatchesOwnerCurrentSession(connection),
                HasD5SdoTicketOrQuarantine,
                HasUnresolvedDigitalOutputWrite,
                record,
                pending == null ? 0 : pending.WriteTicket.TicketId,
                pending == null ? 0 : pending.DiagnosticsBootId,
                pending == null ? 0 : pending.SubmissionMapRevision);
        }

        private bool IsDiagnosticsMutationRecoveryIdle()
        {
            return !operationRunning
                && !safetyCommandRunning
                && safetyMonitorCount == 0
                && !qualificationRunning;
        }

        private void PersistedMutationPhysicalVerification_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private async void ButtonAcknowledgePersistedMutation_Click(
            object sender,
            RoutedEventArgs e)
        {
            var admission = EvaluateDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.ExistingResourceCleanup);
            if (!admission.IsAllowed)
            {
                WriteLog(
                    CreateDiagnosticsAdmissionException(
                        "Acknowledge Recovered Mutation",
                        admission).Message);
                UpdateUiState();
                return;
            }

            var recoveryIdle = IsDiagnosticsMutationRecoveryIdle();
            if (CanAttemptExactSdoRestartRecovery(recoveryIdle))
            {
                await RunOperationAsync(
                    "Verify Recovered SDO Readback",
                    () => RecoverPersistedSdoWriteByExactReadbackAsync(
                        recoveryIdle));
                return;
            }

            var staleSdoRecovery =
                CanAcknowledgeStaleSdoWriteReadback(recoveryIdle);
            var startupRecovery =
                StaleSdoWriteReadbackRecoveryPolicy
                    .CanAcknowledgeStartupRecovery(
                        recoveryIdle,
                        !DiagnosticsMutationJournalUnavailable,
                        diagnosticsMutationRecoveredAtStartup,
                        HasActiveDiagnosticsMutationJournalRecord,
                        HasPendingD5SdoWriteReadback,
                        HasD5SdoTicketOrQuarantine,
                        HasUnresolvedDigitalOutputWrite);
            if ((!staleSdoRecovery && !startupRecovery)
                || CheckPersistedMutationPhysicallyVerified.IsChecked != true)
            {
                return;
            }

            var record = diagnosticsMutationJournal.CurrentRecord;
            var recordIdentity = record.Identity;
            var pendingReadback = staleSdoRecovery
                ? d5SdoPendingWriteReadback
                : null;
            var confirmation = MessageBox.Show(
                TranslateUiText(staleSdoRecovery
                    ? "The exact SDO readback cannot run in the current connection session. Confirm that the SDO target and PLC state were checked independently."
                    : "Confirm that the physical target and PLC state were checked independently.")
                    + Environment.NewLine
                    + FormatDiagnosticsMutationRecord(record)
                    + Environment.NewLine
                    + TranslateUiText(
                        "This writes a durable Resolved tombstone. It does not replay the command or prove the previous outcome."),
                TranslateUiText("Acknowledge Recovered Mutation"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);
            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            recoveryIdle = IsDiagnosticsMutationRecoveryIdle();
            try
            {
                if (staleSdoRecovery)
                {
                    var committed =
                        StaleSdoWriteReadbackRecoveryCommitter.TryCommit(
                            () =>
                            {
                                recoveryIdle =
                                    IsDiagnosticsMutationRecoveryIdle();
                                var currentRecord =
                                    diagnosticsMutationJournal == null
                                        ? null
                                        : diagnosticsMutationJournal
                                            .CurrentRecord;
                                return currentRecord != null
                                    && currentRecord.Identity
                                        == recordIdentity
                                    && ReferenceEquals(
                                        pendingReadback,
                                        d5SdoPendingWriteReadback)
                                    && CanAcknowledgeStaleSdoWriteReadback(
                                        recoveryIdle);
                            },
                            () => ResolveDiagnosticsMutationJournal(
                                DiagnosticsMutationKind.SdoWrite),
                            () => d5SdoPendingWriteReadback = null);
                    if (!committed)
                    {
                        WriteLog(
                            "Mutation recovery acknowledgement was not applied because the guarded state changed while confirmation was open.");
                        UpdateUiState();
                        return;
                    }
                }
                else
                {
                    recoveryIdle = IsDiagnosticsMutationRecoveryIdle();
                    var currentRecord =
                        diagnosticsMutationJournal == null
                            ? null
                            : diagnosticsMutationJournal.CurrentRecord;
                    var startupStillValid = currentRecord != null
                        && currentRecord.Identity == recordIdentity
                        && StaleSdoWriteReadbackRecoveryPolicy
                            .CanAcknowledgeStartupRecovery(
                                recoveryIdle,
                                !DiagnosticsMutationJournalUnavailable,
                                diagnosticsMutationRecoveredAtStartup,
                                HasActiveDiagnosticsMutationJournalRecord,
                                HasPendingD5SdoWriteReadback,
                                HasD5SdoTicketOrQuarantine,
                                HasUnresolvedDigitalOutputWrite);
                    if (!startupStillValid)
                    {
                        WriteLog(
                            "Mutation recovery acknowledgement was not applied because the guarded state changed while confirmation was open.");
                        UpdateUiState();
                        return;
                    }

                    diagnosticsMutationJournal.Resolve(
                        record.Identity,
                        GetDiagnosticsMutationTransitionUtc(record));
                }
            }
            catch (Exception error)
            {
                RecordDiagnosticsMutationJournalRuntimeFault(error);
                WriteLog(
                    "Recovered durable mutation acknowledgement failed: "
                    + error.Message);
                MessageBox.Show(
                    TranslateUiText(
                        "The durable Resolved tombstone could not be written. The interlock remains active.")
                        + Environment.NewLine
                        + error.Message,
                    TranslateUiText("Mutation Recovery Failed"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                UpdateUiState();
                return;
            }
            if (staleSdoRecovery)
            {
                TextDiagnosticOperationSummary.Text =
                    "Stale-session SDO Write readback was physically verified and explicitly acknowledged. The durable record is resolved; no command was replayed and the acknowledgement is not protocol proof.";
                CloseExternalD5TrackingLogIfResolved(
                    "WRITE_STALE_READBACK_PHYSICAL_ACK");
            }
            diagnosticsMutationRecoveredAtStartup = false;
            CheckPersistedMutationPhysicallyVerified.IsChecked = false;
            WriteLog(
                (staleSdoRecovery
                    ? "Stale-session SDO Write readback"
                    : "Recovered durable mutation")
                + " was physically verified and explicitly acknowledged. No command was replayed. "
                + FormatDiagnosticsMutationRecord(record));
            RefreshDiagnosticsMutationJournalUi();
            UpdateUiState();
        }

        private async Task RecoverPersistedSdoWriteByExactReadbackAsync(
            bool recoveryWasIdle)
        {
            var currentConnection = RequireConnection();
            var record = RequireActiveDiagnosticsMutationRecord(
                DiagnosticsMutationKind.SdoWrite);
            if (!CanAttemptExactSdoRestartRecovery(recoveryWasIdle))
            {
                throw new InvalidOperationException(
                    "The exact recovered SDO readback is no longer eligible. No SDO command was submitted.");
            }

            diagnosticsMutationRestartRecoveryAttemptedIdentity =
                record.Identity;
            UpdateUiState();

            LMCDiagnosticCapabilities observedCapabilities = null;
            DiagnosticsSdoRestartRecoveryResult result;
            try
            {
                result = await DiagnosticsSdoRestartRecoveryOrchestrator
                    .TryRecoverAsync(
                        diagnosticsMutationJournal,
                        diagnosticsMutationRecoveredAtStartup,
                        recoveryWasIdle,
                        ReferenceEquals(currentConnection, connection)
                            && currentConnection.IsConnected,
                        HasPendingD5SdoWriteReadback,
                        HasD5SdoTicketOrQuarantine,
                        HasUnresolvedDigitalOutputWrite,
                        metadata => IsExactApprovedSdoWriteTarget(
                            metadata,
                            currentConnection.Diagnostics
                                .GetApprovedSdoWriteTargets()),
                        async () =>
                        {
                            observedCapabilities =
                                await ReadExternalD5TrackingCapabilitiesAsync(
                                    currentConnection,
                                    "restart-sdo-write-exact-readback",
                                    record.SdoWriteMetadata.DataLength,
                                    true);
                            return new
                                DiagnosticsSdoRestartRecoveryCapabilities(
                                    observedCapabilities.DiagnosticsBootId,
                                    observedCapabilities.MapRevision,
                                    observedCapabilities.Supports(
                                        LMCDiagnosticCapability.SDORead),
                                    observedCapabilities.Supports(
                                        LMCDiagnosticCapability
                                            .SDOReadGeneralInline),
                                    observedCapabilities.MaxSdoDataBytes);
                        },
                        metadata => ReadPersistedSdoWriteExactTargetAsync(
                            currentConnection,
                            observedCapabilities,
                            metadata));
            }
            catch (Exception error)
            {
                TextDiagnosticOperationSummary.Text =
                    "Recovered SDO exact readback failed. The durable interlock remains active and no Write was replayed. "
                    + error.Message;
                throw;
            }

            ApplySdoRestartRecoveryResult(record, result);
        }

        private void ApplySdoRestartRecoveryResult(
            DiagnosticsMutationRecord originalRecord,
            DiagnosticsSdoRestartRecoveryResult result)
        {
            if (result == null)
            {
                throw new InvalidDataException(
                    "Restart SDO recovery returned no result.");
            }

            switch (result.Disposition)
            {
                case DiagnosticsSdoRestartRecoveryDisposition.Verified:
                    diagnosticsMutationRecoveredAtStartup = false;
                    CheckPersistedMutationPhysicallyVerified.IsChecked =
                        false;
                    TextDiagnosticOperationSummary.Text =
                        "Recovered SDO Write was resolved by one exact read-only SDO readback under the original BootId/MapRevision. No Write was replayed.";
                    WriteLog(
                        "Recovered durable SDO Write exact readback MATCH. Durable Resolved tombstone was persisted before this result. No Write was replayed. "
                        + FormatDiagnosticsMutationRecord(originalRecord));
                    CloseExternalD5TrackingLogIfResolved(
                        "RESTART_EXACT_READBACK_VERIFIED");
                    break;

                case DiagnosticsSdoRestartRecoveryDisposition
                    .ReadbackMismatch:
                    TextDiagnosticOperationSummary.Text =
                        "Recovered SDO Write exact readback MISMATCH. The durable ReadbackMismatch interlock remains active; no Write was replayed.";
                    WriteLog(
                        "Recovered durable SDO Write exact readback MISMATCH. Durable evidence remains unresolved. No Write was replayed. "
                        + FormatDiagnosticsMutationRecord(originalRecord));
                    CloseExternalD5TrackingLogIfResolved(
                        "RESTART_EXACT_READBACK_MISMATCH");
                    break;

                default:
                    TextDiagnosticOperationSummary.Text =
                        "Recovered SDO Write was not resolved. disposition="
                        + result.Disposition
                        + ". The durable interlock remains active and no Write was replayed.";
                    WriteLog(
                        "Recovered durable SDO Write exact readback was not applied. disposition="
                        + result.Disposition
                        + ". No Write was replayed.");
                    CloseExternalD5TrackingLogIfResolved(
                        "RESTART_EXACT_READBACK_NOT_APPLIED");
                    break;
            }

            RefreshDiagnosticsMutationJournalUi();
        }

        private async Task<byte[]> ReadPersistedSdoWriteExactTargetAsync(
            LMCConnection ownerConnection,
            LMCDiagnosticCapabilities capabilities,
            DiagnosticsSdoWriteMutationMetadata metadata)
        {
            if (!ReferenceEquals(ownerConnection, connection)
                || !ownerConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "The restart SDO readback connection changed before submission.");
            }

            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "The restart SDO readback has no validated capability identity.");
            }

            var request = LMCSdoRequest.CreateRead(
                metadata.SlaveReference,
                metadata.ObjectIndex,
                metadata.SubIndex,
                metadata.ValueType,
                metadata.DataLength,
                metadata.TimeoutCycles);
            const string stage = "restart-sdo-write-exact-readback";
            var submissionGuard = ArmExternalD5SubmissionOutcomeGuard(
                LMCOperationKind.SDORead,
                request,
                ownerConnection,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                metadata.SlaveReference,
                metadata.TimeoutCycles,
                stage);

            LMCOperationTicket ticket;
            try
            {
                ticket = await ownerConnection.Diagnostics.SubmitSdoAsync(
                    request,
                    CancellationToken.None);
            }
            catch (Exception error)
            {
                try
                {
                    D5ExternalReadFailureOrchestrator
                        .RouteSubmissionFailure(
                            error,
                            (state, detail) =>
                                DisarmExternalD5SubmissionOutcomeGuard(
                                    submissionGuard,
                                    state,
                                    detail),
                            (acceptedTicket,
                                actualBootId,
                                actualMapRevision) =>
                            {
                                TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                                    submissionGuard,
                                    acceptedTicket,
                                    actualBootId,
                                    actualMapRevision);
                                PreserveExternalD5Ticket(
                                    acceptedTicket,
                                    request,
                                    ownerConnection,
                                    metadata.SlaveReference,
                                    metadata.TimeoutCycles,
                                    actualMapRevision,
                                    stage);
                            },
                            (unresolvedError, failureContext) =>
                                PreserveExternalD5RawSubmissionOutcomeUncertain(
                                    submissionGuard,
                                    unresolvedError,
                                    failureContext));
                }
                catch (Exception routingError)
                {
                    throw new InvalidOperationException(
                        "Restart SDO readback submission failed and durable outcome routing also failed. The interlock remains active.",
                        new AggregateException(error, routingError));
                }

                throw;
            }

            TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                submissionGuard,
                ticket,
                ticket.DiagnosticsBootId,
                ticket.SubmissionMapRevision);
            PreserveExternalD5Ticket(
                ticket,
                request,
                ownerConnection,
                metadata.SlaveReference,
                metadata.TimeoutCycles,
                ticket.SubmissionMapRevision,
                stage);
            if (ticket.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || ticket.SubmissionMapRevision
                    != capabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "The restart SDO read ticket identity changed after preflight. The accepted ticket remains quarantined and the mutation interlock remains active.");
            }

            DisarmExternalD5SubmissionOutcomeGuard(
                submissionGuard,
                "ACCEPTED_TICKET",
                ticket.TicketId.ToString(CultureInfo.InvariantCulture));

            var terminalStatus = await WaitForRestartSdoReadTerminalAsync(
                ownerConnection,
                ticket,
                GetD5SdoQualificationTerminalWaitMilliseconds(
                    metadata.TimeoutCycles,
                    capabilities.BaseCycleTimeUs));
            if (!terminalStatus.IsSuccessful)
            {
                throw new InvalidOperationException(
                    "The restart SDO read reached terminal state "
                    + terminalStatus.State
                    + " with outcome "
                    + terminalStatus.Outcome
                    + ".");
            }

            var resultData = terminalStatus.ResultData;
            if (terminalStatus.ResultValueType != metadata.ValueType
                || terminalStatus.ResultLength != metadata.DataLength
                || resultData.Length != metadata.DataLength)
            {
                throw new InvalidDataException(
                    "The restart SDO read terminal result metadata does not match the durable typed target.");
            }

            return resultData;
        }

        private async Task<LMCOperationStatus>
            WaitForRestartSdoReadTerminalAsync(
                LMCConnection ownerConnection,
                LMCOperationTicket ticket,
                int timeoutMilliseconds)
        {
            var deadlineUtc = DateTime.UtcNow.AddMilliseconds(
                timeoutMilliseconds);
            while (true)
            {
                if (!ReferenceEquals(ownerConnection, connection)
                    || !ownerConnection.IsConnected)
                {
                    throw new InvalidOperationException(
                        "The restart SDO read connection changed while its accepted ticket was pending.");
                }

                var status = await ownerConnection.Diagnostics
                    .GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None);
                d5SdoQualificationActiveStatus = status;
                if (status.IsTerminal)
                {
                    ClearActiveD5SdoQualificationTicket();
                    return status;
                }

                if (DateTime.UtcNow >= deadlineUtc)
                {
                    throw new TimeoutException(
                        "The restart SDO read ticket did not reach a terminal state within "
                        + timeoutMilliseconds.ToString(
                            CultureInfo.InvariantCulture)
                        + " ms. The accepted ticket remains preserved for cleanup.");
                }

                await Task.Delay(D5SdoQualificationPollMilliseconds);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                DisposeMaintenanceActionRecoveryJournal();
            }
            finally
            {
                try
                {
                    DisposeAxisQualificationRecoveryJournal();
                }
                finally
                {
                    try
                    {
                        DisposeGroupResetRecoveryJournal();
                    }
                    finally
                    {
                        try
                        {
                            DisposeRecoveryRecordRetirementLedger();
                        }
                        finally
                        {
                            try
                            {
                                DisposeGroupPowerRecoveryJournal();
                            }
                            finally
                            {
                                try
                                {
                                    DisposeAxisCommandRecoveryJournal();
                                }
                                finally
                                {
                                    try
                                    {
                                        DisposeAxisPowerOnRecoveryJournal();
                                    }
                                    finally
                                    {
                                        try
                                        {
                                            DisposeMotionUncertaintyJournal();
                                        }
                                        finally
                                        {
                                            try
                                            {
                                                DisposeGroupProfileLockRecoveryJournal();
                                            }
                                            finally
                                            {
                                                try
                                                {
                                                    DisposeRecorderDoubleRecoveryJournal();
                                                }
                                                finally
                                                {
                                                    try
                                                    {
                                                        DisposeDiagnosticsMutationJournal();
                                                    }
                                                    finally
                                                    {
                                                        base.OnClosed(e);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

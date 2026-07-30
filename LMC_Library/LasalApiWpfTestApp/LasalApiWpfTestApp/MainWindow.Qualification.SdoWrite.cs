using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const string D5SdoWriteSameValueScenario =
            "D5SdoWriteSameValue";

        private async void ButtonRunD5SdoWriteSameValueQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            D5SdoWriteSameValueInput input;
            try
            {
                input = ReadD5SdoWriteSameValueInput();
            }
            catch (Exception error)
            {
                TextD5SdoWriteQualificationSummary.Text =
                    "NOT STARTED: " + error.Message;
                TextOperationState.Text =
                    "Same-value SDO Write qualification validation failed";
                WriteLog(
                    "Same-value SDO Write qualification not started: "
                    + error.Message);
                return;
            }

            try
            {
                await RunQualificationAsync(
                    D5SdoWriteSameValueScenario,
                    cancellationToken =>
                        RunD5SdoWriteSameValueQualificationAsync(
                            input,
                            cancellationToken));
            }
            finally
            {
                ResetD5SdoWriteSameValueOperatorConfirmations();
                UpdateUiState();
                RefreshD5SdoQualificationOutput();
            }
        }

        private void D5SdoWriteQualificationInput_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private D5SdoWriteSameValueInput
            ReadD5SdoWriteSameValueInput()
        {
            var currentConnection = connection;
            if (currentConnection == null || !currentConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "An active PLC connection is required.");
            }

            if (!HasCachedD5SdoWriteSameValueContract())
            {
                throw new NotSupportedException(
                    GetD5SdoWriteSameValueGateReason());
            }

            if (!HasD5SdoWriteSameValueOperatorConfirmation())
            {
                throw new InvalidOperationException(
                    "All four SDO Write activation confirmations are required before any qualification request is sent.");
            }

            var target = ComboD5SdoWriteQualificationTarget.SelectedItem
                as LMCSdoWriteTarget;
            if (target == null
                || approvedSdoWriteTargets.Count != 1
                || !ReferenceEquals(approvedSdoWriteTargets[0], target))
            {
                throw new InvalidOperationException(
                    "Select the single SDK-approved SDO Write target.");
            }

            var timeoutCycles = ParseUInt32(
                TextD5SdoWriteQualificationTimeoutCycles.Text,
                "same-value SDO Write timeout cycles");
            if (timeoutCycles < 1 || timeoutCycles > 60000)
            {
                throw new InvalidOperationException(
                    "Same-value SDO Write timeout must be between 1 and 60000 cycles.");
            }

            return new D5SdoWriteSameValueInput(
                target,
                approvedSdoWriteTargets.ToArray(),
                diagnosticCapabilities,
                timeoutCycles);
        }

        private async Task RunD5SdoWriteSameValueQualificationAsync(
            D5SdoWriteSameValueInput input,
            CancellationToken cancellationToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            EnsureNoPendingManualD5Operation();
            EnsureNoUnresolvedD5SdoQualificationTicket(
                "Same-value SDO Write qualification");
            if (!DiagnosticsMutationJournalCanArm)
            {
                throw new InvalidOperationException(
                    "The durable mutation journal is not available for a new SDO Write.");
            }

            var currentConnection = RequireConnection();
            var diagnostics = currentConnection.Diagnostics;
            var terminalWaitMilliseconds =
                GetD5SdoQualificationTerminalWaitMilliseconds(
                    input.TimeoutCycles,
                    input.InitialCapabilities.BaseCycleTimeUs);
            var currentCapabilities = input.InitialCapabilities;
            D5SdoQuarantineHandle writeGuard = null;
            var safetyVerificationCount = 0;
            var readSubmissionCount = 0;
            uint baselineTicketId = 0;
            uint preWriteGuardTicketId = 0;
            uint readbackTicketId = 0;

            SetD5SdoWriteSameValueProgress(
                2,
                "Preflight accepted locally; reading the exact approved target baseline");
            WriteD5SdoQualificationLog(
                "event=D5_WRITE_SAME_VALUE_PREFLIGHT",
                "wireMutation=false",
                "target=" + QualificationValue(input.Target.ToString()),
                "timeoutCycles=" + input.TimeoutCycles.ToString(
                    CultureInfo.InvariantCulture),
                "bootId=0x"
                    + input.InitialCapabilities.DiagnosticsBootId.ToString(
                        "X8"),
                "mapRevision=0x"
                    + input.InitialCapabilities.MapRevision.ToString("X8"),
                "approvedTargetCount=1",
                "operatorConfirmations=4/4",
                "sentinelWrite=false",
                "automaticRestore=false",
                "automaticReplay=false",
                "verdict=PASS");

            var request = new
                D5SdoWriteSameValueQualificationRequest(
                    currentConnection,
                    input.InitialCapabilities,
                    input.ApprovedTargets,
                    input.Target,
                    input.TimeoutCycles);
            var result = await
                D5SdoWriteSameValueQualificationOrchestrator.RunAsync(
                    request,
                    new D5SdoWriteSameValueQualificationOperations
                    {
                        ReadCapabilitiesAsync = async token =>
                        {
                            var capabilities =
                                await ReadD5SdoQualificationCapabilitiesAsync(
                                    diagnostics,
                                    token,
                                    "same-value Write identity sample");
                            currentCapabilities = capabilities;
                            diagnosticCapabilities = capabilities;
                            TextDiagnosticsCapabilities.Text =
                                FormatCapabilities(capabilities);
                            return capabilities;
                        },
                        SubmitAsync = async (sdoRequest, token) =>
                        {
                            if (!sdoRequest.IsWrite)
                            {
                                readSubmissionCount++;
                                var isBaseline = readSubmissionCount == 1;
                                SetD5SdoWriteSameValueProgress(
                                    isBaseline ? 12 : 55,
                                    isBaseline
                                        ? "Submitting exact target baseline Read"
                                        : "Re-reading the exact target immediately before Write");
                                var readTicket =
                                    await SubmitD5SdoQualificationAsync(
                                    currentConnection,
                                    diagnostics,
                                    sdoRequest,
                                    currentCapabilities.DiagnosticsBootId,
                                    currentCapabilities.MapRevision,
                                    token,
                                    isBaseline
                                        ? "same-value-baseline"
                                        : "same-value-prewrite-guard",
                                    "0x" + sdoRequest.ObjectIndex.ToString(
                                        "X4"),
                                    sdoRequest.SubIndex,
                                    terminalWaitMilliseconds);
                                if (isBaseline)
                                {
                                    baselineTicketId = readTicket.TicketId;
                                }
                                else
                                {
                                    preWriteGuardTicketId =
                                        readTicket.TicketId;
                                }

                                return readTicket;
                            }

                            SetD5SdoWriteSameValueProgress(
                                70,
                                "Submitting the byte-identical SDO Write once");
                            return await SubmitD5SdoWriteSameValueAsync(
                                currentConnection,
                                diagnostics,
                                sdoRequest,
                                writeGuard,
                                token);
                        },
                        WaitForTerminalAsync = async (ticket, token) =>
                        {
                            string stage;
                            if (ticket == null)
                            {
                                stage = "same-value-unknown";
                            }
                            else if (ticket.OperationKind
                                == LMCOperationKind.SDOWrite)
                            {
                                stage = "same-value-write";
                            }
                            else if (ticket.TicketId == baselineTicketId)
                            {
                                stage = "same-value-baseline";
                            }
                            else if (ticket.TicketId
                                == preWriteGuardTicketId)
                            {
                                stage = "same-value-prewrite-guard";
                            }
                            else if (ticket.TicketId == readbackTicketId)
                            {
                                stage = "same-value-readback";
                            }
                            else if (ticket.SubmittedSdoRequest != null
                                && ticket.SubmittedSdoRequest.ObjectIndex
                                    == input.Target.ObjectIndex
                                && ticket.SubmittedSdoRequest.SubIndex
                                    == input.Target.SubIndex
                                && writeGuard == null)
                            {
                                stage = "same-value-readback-or-baseline";
                            }
                            else
                            {
                                stage = "same-value-read";
                            }

                            var status =
                                await WaitForD5SdoQualificationTerminalAsync(
                                    diagnostics,
                                    ticket,
                                    terminalWaitMilliseconds,
                                    token,
                                    stage);
                            WriteD5SdoQualificationTerminalLog(
                                stage,
                                ticket,
                                status);
                            return status;
                        },
                        VerifySafeAxisAsync = async (target, token) =>
                        {
                            safetyVerificationCount++;
                            var isFinalSafetyCheck =
                                safetyVerificationCount > 1;
                            SetD5SdoWriteSameValueProgress(
                                isFinalSafetyCheck ? 60 : 35,
                                isFinalSafetyCheck
                                    ? "Re-verifying the safe axis state after confirmation"
                                    : "Verifying PowerOn=False, Standstill=True, and three stable position samples");
                            await VerifyD5SdoQualificationSafeAxisAsync(
                                currentConnection,
                                target.SlaveReference,
                                "_LMCAxis" + target.SlaveReference.ToString(
                                    CultureInfo.InvariantCulture),
                                token);
                            return true;
                        },
                        ConfirmWriteAsync = (writeRequest, token) =>
                        {
                            token.ThrowIfCancellationRequested();
                            SetD5SdoWriteSameValueProgress(
                                45,
                                "Operator confirmations accepted; safety and baseline will be re-checked before journal arm");
                            TextD5SdoWriteQualificationSummary.Text =
                                FormatArmedSdoWriteConfirmation(writeRequest)
                                    .Replace(
                                        "SDO WRITE CONFIRMATION ARMED - NOT SUBMITTED",
                                        "SDO WRITE QUALIFICATION SNAPSHOT - NOT YET SUBMITTED");
                            WriteQualificationLog(
                                "event=SDO_WRITE_OPERATOR_CONFIRMATION_ACCEPTED",
                                "request=" + QualificationValue(
                                    FormatArmedSdoWriteConfirmation(
                                        writeRequest)),
                                "modalDialog=false");
                            return Task.FromResult(true);
                        },
                        ArmJournal = (scope, writeRequest, capabilities) =>
                        {
                            SetD5SdoWriteSameValueProgress(
                                65,
                                "Arming durable mutation and submission-outcome evidence before Write");
                            writeGuard = ArmExternalD5SubmissionOutcomeGuard(
                                LMCOperationKind.SDOWrite,
                                writeRequest,
                                currentConnection,
                                capabilities.DiagnosticsBootId,
                                capabilities.MapRevision,
                                writeRequest.SlaveReference,
                                writeRequest.TimeoutCycles,
                                "same-value-write");
                            CommitQualificationIrreversibleOutcome(
                                "same-value SDO Write durable journal armed before the single Write attempt");
                        },
                        AdoptWriteTicketBeforeValidation = (scope, ticket) =>
                        {
                            try
                            {
                                if (ticket == null)
                                {
                                    throw new InvalidOperationException(
                                        "The SDO Write submit returned no ticket after the wire attempt.");
                                }

                                TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                                    writeGuard,
                                    ticket,
                                    ticket.DiagnosticsBootId,
                                    ticket.SubmissionMapRevision);
                                PreserveExternalD5Ticket(
                                    ticket,
                                    scope.WriteRequest,
                                    currentConnection,
                                    scope.WriteRequest.SlaveReference,
                                    scope.WriteRequest.TimeoutCycles,
                                    ticket.SubmissionMapRevision,
                                    "same-value-write-returned-before-validation");
                            }
                            catch (Exception adoptionError)
                            {
                                try
                                {
                                    MarkSdoWriteMutationOutcomeUnverified();
                                }
                                catch (Exception journalError)
                                {
                                    throw new InvalidOperationException(
                                        "The returned SDO Write ticket could not be adopted and the durable journal could not be marked OutcomeUnverified. Do not replay the Write.",
                                        new AggregateException(
                                            adoptionError,
                                            journalError));
                                }

                                throw new InvalidOperationException(
                                    "The returned SDO Write ticket could not be adopted before validation. Durable outcome evidence remains unresolved; do not replay the Write.",
                                    adoptionError);
                            }
                        },
                        MarkWriteAccepted = (scope, ticket) =>
                        {
                            DisarmExternalD5SubmissionOutcomeGuard(
                                writeGuard,
                                "ACCEPTED_TICKET_VALIDATED",
                                ticket.TicketId.ToString(
                                    CultureInfo.InvariantCulture));
                            writeGuard = null;
                        },
                        MarkWriteTerminalSuccess = (scope, ticket, status) =>
                        {
                            MarkSdoWriteMutationTerminalSuccess(ticket);
                            WriteD5SdoQualificationLog(
                                "event=D5_WRITE_SAME_VALUE_TERMINAL",
                                "ticket=" + ticket.TicketId.ToString(
                                    CultureInfo.InvariantCulture),
                                "state=" + status.State,
                                "outcome=" + status.Outcome,
                                "readbackRequired=true",
                                "mutationBlocked=true");
                        },
                        CreateVerificationContext =
                            (writeRequest, ticket, status) =>
                            {
                                var verification = diagnostics
                                    .CreateSdoWriteVerificationContext(
                                        writeRequest,
                                        ticket,
                                        status);
                                d5SdoPendingWriteReadback = verification;
                                ShowPendingD5SdoWriteReadbackStatus();
                                return verification;
                            },
                        SubmitReadbackAsync = async (
                            verification,
                            readbackRequest,
                            token) =>
                        {
                            SetD5SdoWriteSameValueProgress(
                                75,
                                "Submitting the guarded exact readback");
                            var readbackTicket =
                                await SubmitD5SdoWriteSameValueReadbackAsync(
                                currentConnection,
                                verification,
                                readbackRequest,
                                currentCapabilities,
                                token);
                            readbackTicketId = readbackTicket.TicketId;
                            return readbackTicket;
                        },
                        ResolveJournalAfterVerified = scope =>
                        {
                            ResolveDiagnosticsMutationJournal(
                                DiagnosticsMutationKind.SdoWrite);
                            d5SdoPendingWriteReadback = null;
                        },
                        RecoveryRequired = (scope, error) =>
                            PublishD5SdoWriteSameValueRecoveryRequired(
                                scope,
                                error)
                    },
                    cancellationToken);

            var completed = result.RecoveryScope;
            WriteD5SdoQualificationLog(
                "event=D5_WRITE_SAME_VALUE_ASSERT",
                "baseline="
                    + BitConverter.ToString(completed.BaselineData),
                "write="
                    + BitConverter.ToString(
                        completed.WriteRequest.WriteData),
                "readback="
                    + BitConverter.ToString(
                        completed.ReadbackStatus.ResultData),
                "baselineTicket="
                    + completed.BaselineTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture),
                "preWriteGuardTicket="
                    + completed.PreWriteGuardTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture),
                "writeTicket="
                    + completed.WriteTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture),
                "readbackTicket="
                    + completed.ReadbackTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture),
                "journalResolved=true",
                "secondSafetyVerified="
                    + completed.SecondSafetyVerified,
                "singleWriterConfirmed=true",
                "sentinelWrite=false",
                "automaticRestore=false",
                "verdict=PASS");
            CloseExternalD5TrackingLogIfResolved(
                "SAME_VALUE_WRITE_EXACT_READBACK_VERIFIED");
            SetD5SdoWriteSameValueProgress(
                100,
                "PASS: baseline, byte-identical Write, and exact readback verified; durable mutation interlock resolved");
            TextD5SdoWriteQualificationSummary.Text =
                "PASS | Target=" + input.Target
                + " | Baseline="
                + BitConverter.ToString(completed.BaselineData)
                + " | Tickets="
                + completed.BaselineTicket.TicketId.ToString(
                    CultureInfo.InvariantCulture)
                + "/"
                + completed.PreWriteGuardTicket.TicketId.ToString(
                    CultureInfo.InvariantCulture)
                + "/"
                + completed.WriteTicket.TicketId.ToString(
                    CultureInfo.InvariantCulture)
                + "/"
                + completed.ReadbackTicket.TicketId.ToString(
                    CultureInfo.InvariantCulture)
                + " | Sentinel=false | AutoRestore=false";
        }

        private async Task<LMCOperationTicket>
            SubmitD5SdoWriteSameValueAsync(
                LMCConnection ownerConnection,
                LMCDiagnostics diagnostics,
                LMCSdoRequest request,
                D5SdoQuarantineHandle submissionGuard,
                CancellationToken cancellationToken)
        {
            if (submissionGuard == null)
            {
                throw new InvalidOperationException(
                    "The same-value SDO Write submission guard was not durably armed.");
            }

            try
            {
                return await SendQualificationCommandAsync(
                    "same-value SDO Write submit",
                    cancellationToken,
                    () => diagnostics.SubmitSdoAsync(
                        request,
                        CancellationToken.None));
            }
            catch (Exception error)
            {
                try
                {
                    D5ExternalReadFailureOrchestrator.RouteSubmissionFailure(
                        error,
                        (state, detail) =>
                            DisarmExternalD5SubmissionOutcomeGuard(
                                submissionGuard,
                                state,
                                detail),
                        (ticket, actualBootId, actualMapRevision) =>
                        {
                            TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                                submissionGuard,
                                ticket,
                                actualBootId,
                                actualMapRevision);
                            PreserveExternalD5Ticket(
                                ticket,
                                request,
                                ownerConnection,
                                request.SlaveReference,
                                request.TimeoutCycles,
                                actualMapRevision,
                                "same-value-write");
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
                        "The same-value SDO Write failed and its durable submission-outcome routing also failed. Do not replay the Write.",
                        new AggregateException(error, routingError));
                }

                throw;
            }
        }

        private async Task<LMCOperationTicket>
            SubmitD5SdoWriteSameValueReadbackAsync(
                LMCConnection ownerConnection,
                LMCSdoWriteVerificationContext verification,
                LMCSdoRequest request,
                LMCDiagnosticCapabilities capabilities,
                CancellationToken cancellationToken)
        {
            var guard = ArmExternalD5SubmissionOutcomeGuard(
                LMCOperationKind.SDORead,
                request,
                ownerConnection,
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision,
                request.SlaveReference,
                request.TimeoutCycles,
                "same-value-readback");
            LMCOperationTicket ticket;
            try
            {
                ticket = await SendQualificationCommandAsync(
                    "same-value SDO Write exact readback submit",
                    cancellationToken,
                    () => verification.SubmitReadbackAsync(
                        request,
                        CancellationToken.None));
            }
            catch (Exception error)
            {
                try
                {
                    D5ExternalReadFailureOrchestrator.RouteSubmissionFailure(
                        error,
                        (state, detail) =>
                            DisarmExternalD5SubmissionOutcomeGuard(
                                guard,
                                state,
                                detail),
                        (accepted, actualBootId, actualMapRevision) =>
                        {
                            TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                                guard,
                                accepted,
                                actualBootId,
                                actualMapRevision);
                            PreserveExternalD5Ticket(
                                accepted,
                                request,
                                ownerConnection,
                                request.SlaveReference,
                                request.TimeoutCycles,
                                actualMapRevision,
                                "same-value-readback");
                        },
                        (unresolvedError, failureContext) =>
                            PreserveExternalD5RawSubmissionOutcomeUncertain(
                                guard,
                                unresolvedError,
                                failureContext));
                }
                catch (Exception routingError)
                {
                    throw new InvalidOperationException(
                        "The exact SDO Write readback failed and its submission-outcome routing also failed. The mutation interlock remains active.",
                        new AggregateException(error, routingError));
                }

                throw;
            }

            TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                guard,
                ticket,
                ticket.DiagnosticsBootId,
                ticket.SubmissionMapRevision);
            PreserveExternalD5Ticket(
                ticket,
                request,
                ownerConnection,
                request.SlaveReference,
                request.TimeoutCycles,
                ticket.SubmissionMapRevision,
                "same-value-readback");
            DisarmExternalD5SubmissionOutcomeGuard(
                guard,
                "ACCEPTED_TICKET",
                ticket.TicketId.ToString(CultureInfo.InvariantCulture));
            return ticket;
        }

        private void PublishD5SdoWriteSameValueRecoveryRequired(
            D5SdoWriteSameValueRecoveryScope scope,
            Exception error)
        {
            if (scope == null || error == null)
            {
                return;
            }

            if (scope.VerificationContext != null)
            {
                d5SdoPendingWriteReadback = scope.VerificationContext;
            }

            TextD5SdoWriteQualificationSummary.Text =
                "RECOVERY REQUIRED | Stage=" + scope.Stage
                + " | WriteAttempted=" + scope.WriteSubmitAttempted
                + " | WriteUncertain="
                + scope.WriteSubmissionOutcomeUncertain
                + " | ReadbackVerified=" + scope.ReadbackVerified
                + " | JournalActive="
                + HasActiveDiagnosticsMutationJournalRecord
                + " | No automatic replay or restore";
            WriteD5SdoQualificationLog(
                "event=D5_WRITE_SAME_VALUE_RECOVERY_REQUIRED",
                "stage=" + scope.Stage,
                "baselineSubmitAttempted="
                    + scope.BaselineSubmitAttempted,
                "preWriteGuardSubmitAttempted="
                    + scope.PreWriteGuardSubmitAttempted,
                "preWriteGuardSubmissionOutcomeUncertain="
                    + scope.PreWriteGuardSubmissionOutcomeUncertain,
                "secondSafetyVerified=" + scope.SecondSafetyVerified,
                "writeSubmitAttempted=" + scope.WriteSubmitAttempted,
                "writeSubmissionOutcomeUncertain="
                    + scope.WriteSubmissionOutcomeUncertain,
                "writeTicket=" + (scope.WriteTicket == null
                    ? "NONE"
                    : scope.WriteTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture)),
                "readbackSubmitAttempted="
                    + scope.ReadbackSubmitAttempted,
                "readbackSubmissionOutcomeUncertain="
                    + scope.ReadbackSubmissionOutcomeUncertain,
                "readbackVerified=" + scope.ReadbackVerified,
                "journalArmed=" + scope.JournalArmed,
                "journalResolved=" + scope.JournalResolved,
                "activeDurableMutation="
                    + HasActiveDiagnosticsMutationJournalRecord,
                "automaticReplay=false",
                "automaticRestore=false",
                "errorType=" + error.GetType().Name,
                "error=" + QualificationValue(error.Message),
                "verdict=RECOVERY_REQUIRED");
        }

        private bool HasCachedD5SdoWriteSameValueContract()
        {
            var evaluation = EvaluateCachedSdoWritePolicy();
            var capabilities = diagnosticCapabilities;
            var target = ComboD5SdoWriteQualificationTarget == null
                ? null
                : ComboD5SdoWriteQualificationTarget.SelectedItem
                    as LMCSdoWriteTarget;
            return evaluation != null
                && evaluation.CanAttemptSubmission
                && capabilities != null
                && capabilities.BaseCycleTimeUs != 0
                && capabilities.MaxSdoDataBytes == 4
                && evaluation.ApprovedTargets.Count == 1
                && approvedSdoWriteTargets.Count == 1
                && target != null
                && ReferenceEquals(approvedSdoWriteTargets[0], target)
                && ReferenceEquals(evaluation.ApprovedTargets[0], target);
        }

        private bool HasD5SdoWriteSameValueOperatorConfirmation()
        {
            return CheckConfirmD5SdoWriteUi24Unused != null
                && CheckConfirmD5SdoWriteUi24Unused.IsChecked == true
                && CheckConfirmD5SdoWriteOriginalRecorded.IsChecked == true
                && CheckConfirmD5SdoWriteCaptureRunning.IsChecked == true
                && CheckConfirmD5SdoWriteSingleWriter.IsChecked == true;
        }

        private void ResetD5SdoWriteSameValueOperatorConfirmations()
        {
            if (CheckConfirmD5SdoWriteUi24Unused == null)
            {
                return;
            }

            CheckConfirmD5SdoWriteUi24Unused.IsChecked = false;
            CheckConfirmD5SdoWriteOriginalRecorded.IsChecked = false;
            CheckConfirmD5SdoWriteCaptureRunning.IsChecked = false;
            CheckConfirmD5SdoWriteSingleWriter.IsChecked = false;
        }

        private string GetD5SdoWriteSameValueGateReason()
        {
            if (connection == null || !connection.IsConnected)
            {
                return "CLOSED: connect to the PLC first.";
            }

            var evaluation = EvaluateCachedSdoWritePolicy();
            if (evaluation == null)
            {
                return "CLOSED: the cached SDK SDO Write policy could not be evaluated.";
            }

            if ((evaluation.Blockers
                    & LMCSdoWritePolicyBlockers.NoApprovedTarget) != 0)
            {
                return "CLOSED: this SDK build has no approved SDO Write target. Confirm UI[24] is unused and select one test axis before enabling matching SDK/PLC gates.";
            }

            if (!evaluation.CanAttemptSubmission)
            {
                return "CLOSED: cached SDK/PLC policy blockers="
                    + evaluation.Blockers
                    + ". Refresh diagnostics capabilities only if the cache is missing or stale.";
            }

            if (evaluation.ApprovedTargets.Count != 1
                || approvedSdoWriteTargets.Count != 1)
            {
                return "CLOSED: activation qualification requires exactly one approved target; multi-axis Write exposure is not accepted.";
            }

            var selectedTarget = ComboD5SdoWriteQualificationTarget
                == null
                    ? null
                    : ComboD5SdoWriteQualificationTarget.SelectedItem
                        as LMCSdoWriteTarget;
            if (selectedTarget == null
                || !ReferenceEquals(
                    evaluation.ApprovedTargets[0],
                    selectedTarget)
                || !ReferenceEquals(
                    approvedSdoWriteTargets[0],
                    selectedTarget))
            {
                return "CLOSED: select the single SDK-approved qualification target.";
            }

            if (diagnosticCapabilities == null
                || diagnosticCapabilities.BaseCycleTimeUs == 0
                || diagnosticCapabilities.MaxSdoDataBytes != 4)
            {
                return "CLOSED: activation qualification additionally requires nonzero BaseCycleTimeUs and exact MaxSdoDataBytes=4.";
            }

            if (!IsD5SdoWriteRunnerAdmissionReady())
            {
                return "CLOSED: runner admission requires an idle connection, no active motion, no pending manual D5 ticket, and no diagnostics mutation interlock.";
            }

            if (!DiagnosticsMutationJournalCanArm)
            {
                return "CLOSED: the durable mutation journal is unavailable or already contains unresolved evidence.";
            }

            if (!HasD5SdoWriteSameValueOperatorConfirmation())
            {
                return "CLOSED: select all four activation confirmations, including the single-writer window. The next attempt cannot send a request until then.";
            }

            return "READY TO PREFLIGHT: the runner will still prove PowerOn=False, Standstill=True, and stable position before Read baseline -> one same-value Write -> exact guarded Readback. Sentinel and automatic restore remain disabled.";
        }

        private bool IsD5SdoWriteRunnerAdmissionReady()
        {
            var connected = connection != null && connection.IsConnected;
            var idle = !operationRunning
                && !safetyCommandRunning
                && safetyMonitorCount == 0
                && !qualificationRunning;
            var manualD5OperationPending = diagnosticOperationTicket != null
                && (diagnosticOperationStatus == null
                    || !diagnosticOperationStatus.IsTerminal);
            return connected
                && idle
                && !motionMayBeActive
                && !manualD5OperationPending
                && !HasDiagnosticsMutationCommandInterlock;
        }

        private int GetD5SdoWriteOperatorConfirmationCount()
        {
            var count = 0;
            count += CheckConfirmD5SdoWriteUi24Unused != null
                && CheckConfirmD5SdoWriteUi24Unused.IsChecked == true
                    ? 1
                    : 0;
            count += CheckConfirmD5SdoWriteOriginalRecorded != null
                && CheckConfirmD5SdoWriteOriginalRecorded.IsChecked == true
                    ? 1
                    : 0;
            count += CheckConfirmD5SdoWriteCaptureRunning != null
                && CheckConfirmD5SdoWriteCaptureRunning.IsChecked == true
                    ? 1
                    : 0;
            count += CheckConfirmD5SdoWriteSingleWriter != null
                && CheckConfirmD5SdoWriteSingleWriter.IsChecked == true
                    ? 1
                    : 0;
            return count;
        }

        private string BuildD5SdoWriteReadinessMatrix()
        {
            var evaluation = EvaluateCachedSdoWritePolicy();
            var capabilities = diagnosticCapabilities;
            var selectedTarget = ComboD5SdoWriteQualificationTarget
                == null
                    ? null
                    : ComboD5SdoWriteQualificationTarget.SelectedItem
                        as LMCSdoWriteTarget;
            var exactQualificationTarget = evaluation != null
                && evaluation.ApprovedTargets.Count == 1
                && approvedSdoWriteTargets.Count == 1
                && selectedTarget != null
                && ReferenceEquals(
                    evaluation.ApprovedTargets[0],
                    selectedTarget)
                && ReferenceEquals(
                    approvedSdoWriteTargets[0],
                    selectedTarget);
            var exactQualificationCapabilities = capabilities != null
                && capabilities.BaseCycleTimeUs != 0
                && capabilities.MaxSdoDataBytes == 4;
            var runnerAdmissionReady =
                IsD5SdoWriteRunnerAdmissionReady();
            var confirmationCount =
                GetD5SdoWriteOperatorConfirmationCount();
            var readyToPreflight = evaluation != null
                && evaluation.CanAttemptSubmission
                && exactQualificationTarget
                && exactQualificationCapabilities
                && runnerAdmissionReady
                && DiagnosticsMutationJournalCanArm
                && confirmationCount == 4;

            var builder = new StringBuilder();
            builder.Append("OVERALL       ")
                .Append(readyToPreflight
                    ? "READY_TO_PREFLIGHT"
                    : "CLOSED")
                .AppendLine(" | EVALUATION_WIRE=NONE");
            builder.Append("SDK POLICY    ")
                .Append(evaluation != null
                    && evaluation.CanAttemptSubmission
                        ? "PASS"
                        : "FAIL")
                .Append(" | blockers=")
                .Append(evaluation == null
                    ? "NO_CONNECTION_INSTANCE"
                    : evaluation.Blockers.ToString())
                .Append(" | targets=")
                .Append(evaluation == null
                    ? "UNKNOWN"
                    : evaluation.ApprovedTargets.Count.ToString(
                        CultureInfo.InvariantCulture))
                .AppendLine();
            builder.Append("PLC CAPS      ")
                .Append(capabilities == null ? "FAIL" : "CACHED")
                .Append(" | bit8/read=")
                .Append(FormatD5SdoWriteCapability(
                    capabilities,
                    LMCDiagnosticCapability.SDORead))
                .Append(" bit9/write=")
                .Append(FormatD5SdoWriteCapability(
                    capabilities,
                    LMCDiagnosticCapability.SDOWrite))
                .Append(" bit13/general=")
                .Append(FormatD5SdoWriteCapability(
                    capabilities,
                    LMCDiagnosticCapability.SDOReadGeneralInline))
                .AppendLine();
            builder.Append("IDENTITY      ")
                .Append(capabilities != null
                    && capabilities.DiagnosticsBootId != 0
                    && capabilities.MapRevision != 0
                        ? "PASS"
                        : "FAIL")
                .Append(" | BootId=")
                .Append(capabilities == null
                    ? "NONE"
                    : "0x" + capabilities.DiagnosticsBootId.ToString(
                        "X8",
                        CultureInfo.InvariantCulture))
                .Append(" MapRevision=")
                .Append(capabilities == null
                    ? "NONE"
                    : "0x" + capabilities.MapRevision.ToString(
                        "X8",
                        CultureInfo.InvariantCulture))
                .AppendLine();
            builder.Append("PAYLOAD       ")
                .Append(exactQualificationCapabilities
                    && capabilities.MaxRequestPayloadBytes >= 36
                    && capabilities.MaxResponsePayloadBytes >= 64
                        ? "PASS"
                        : "FAIL")
                .Append(" | SDO=")
                .Append(capabilities == null
                    ? "NONE"
                    : capabilities.MaxSdoDataBytes.ToString(
                        CultureInfo.InvariantCulture))
                .Append(" req=")
                .Append(capabilities == null
                    ? "NONE"
                    : capabilities.MaxRequestPayloadBytes.ToString(
                        CultureInfo.InvariantCulture))
                .Append(" resp=")
                .Append(capabilities == null
                    ? "NONE"
                    : capabilities.MaxResponsePayloadBytes.ToString(
                        CultureInfo.InvariantCulture))
                .Append(" cycleUs=")
                .Append(capabilities == null
                    ? "NONE"
                    : capabilities.BaseCycleTimeUs.ToString(
                        CultureInfo.InvariantCulture))
                .AppendLine();
            builder.Append("QUAL TARGET   ")
                .Append(exactQualificationTarget ? "PASS" : "FAIL")
                .Append(" | selected=")
                .Append(selectedTarget == null
                    ? "NONE"
                    : selectedTarget.ToString())
                .AppendLine();
            builder.Append("RUNNER        ")
                .Append(runnerAdmissionReady ? "PASS" : "FAIL")
                .Append(" | connected=")
                .Append(connection != null && connection.IsConnected)
                .Append(" idle=")
                .Append(!operationRunning
                    && !safetyCommandRunning
                    && safetyMonitorCount == 0
                    && !qualificationRunning)
                .Append(" motionClear=")
                .Append(!motionMayBeActive)
                .Append(" manualTicketClear=")
                .Append(diagnosticOperationTicket == null
                    || (diagnosticOperationStatus != null
                        && diagnosticOperationStatus.IsTerminal))
                .Append(" mutationInterlockClear=")
                .Append(!HasDiagnosticsMutationCommandInterlock)
                .AppendLine();
            builder.Append("JOURNAL       ")
                .Append(DiagnosticsMutationJournalCanArm
                    ? "PASS"
                    : "FAIL")
                .AppendLine();
            builder.Append("CONFIRMATIONS ")
                .Append(confirmationCount == 4 ? "PASS" : "FAIL")
                .Append(" | ")
                .Append(confirmationCount.ToString(
                    CultureInfo.InvariantCulture))
                .AppendLine("/4");
            builder.AppendLine(
                "AXIS PROOF     PENDING_RUNNER_PREFLIGHT | PowerOn=False, Standstill=True, stable position are not cached here.");
            builder.Append("NEXT          ")
                .Append(GetD5SdoWriteSameValueGateReason());
            return builder.ToString();
        }

        private static string FormatD5SdoWriteCapability(
            LMCDiagnosticCapabilities capabilities,
            LMCDiagnosticCapability capability)
        {
            return capabilities == null
                ? "NONE"
                : (capabilities.Supports(capability) ? "1" : "0");
        }

        private void UpdateD5SdoWriteSameValueQualificationUiState(
            bool d5QualificationReady)
        {
            if (ButtonRunD5SdoWriteSameValueQualification == null
                || TextD5SdoWriteQualificationGateStatus == null)
            {
                return;
            }

            ComboD5SdoWriteQualificationTarget.IsEnabled =
                d5QualificationReady;
            TextD5SdoWriteQualificationTimeoutCycles.IsEnabled =
                d5QualificationReady;
            CheckConfirmD5SdoWriteUi24Unused.IsEnabled =
                d5QualificationReady;
            CheckConfirmD5SdoWriteOriginalRecorded.IsEnabled =
                d5QualificationReady;
            CheckConfirmD5SdoWriteCaptureRunning.IsEnabled =
                d5QualificationReady;
            CheckConfirmD5SdoWriteSingleWriter.IsEnabled =
                d5QualificationReady;
            ButtonRunD5SdoWriteSameValueQualification.IsEnabled =
                d5QualificationReady
                && DiagnosticsMutationJournalCanArm
                && HasCachedD5SdoWriteSameValueContract()
                && HasD5SdoWriteSameValueOperatorConfirmation();
            ButtonRunD5SdoWriteSameValueQualification.ToolTip =
                GetD5SdoWriteSameValueGateReason();
            TextD5SdoWriteQualificationGateStatus.Text =
                BuildD5SdoWriteReadinessMatrix();
        }

        private void SetD5SdoWriteSameValueProgress(
            int progress,
            string summary)
        {
            SetD5SdoQualificationProgress(progress, summary);
            TextD5SdoWriteQualificationSummary.Text = summary;
        }

        private sealed class D5SdoWriteSameValueInput
        {
            private readonly LMCSdoWriteTarget[] approvedTargets;

            internal D5SdoWriteSameValueInput(
                LMCSdoWriteTarget target,
                LMCSdoWriteTarget[] approvedTargets,
                LMCDiagnosticCapabilities initialCapabilities,
                uint timeoutCycles)
            {
                Target = target ?? throw new ArgumentNullException("target");
                this.approvedTargets = approvedTargets == null
                    ? throw new ArgumentNullException("approvedTargets")
                    : (LMCSdoWriteTarget[])approvedTargets.Clone();
                InitialCapabilities = initialCapabilities
                    ?? throw new ArgumentNullException(
                        "initialCapabilities");
                TimeoutCycles = timeoutCycles;
            }

            internal LMCSdoWriteTarget Target { get; private set; }
            internal IReadOnlyList<LMCSdoWriteTarget> ApprovedTargets
            {
                get { return Array.AsReadOnly(approvedTargets); }
            }
            internal LMCDiagnosticCapabilities InitialCapabilities
            {
                get;
                private set;
            }
            internal uint TimeoutCycles { get; private set; }
        }
    }
}

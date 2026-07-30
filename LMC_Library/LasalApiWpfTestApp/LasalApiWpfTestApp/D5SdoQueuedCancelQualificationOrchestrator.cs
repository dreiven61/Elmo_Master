using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum D5SdoQueuedCancelQualificationDisposition
    {
        Qualified = 0,
        NotQualifiedRace = 1
    }

    internal sealed class D5SdoQueuedCancelQualificationRequest
    {
        private readonly byte[] expectedResultData;

        internal D5SdoQueuedCancelQualificationRequest(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoRequest readRequest,
            byte[] expectedResultData)
        {
            Connection = connection
                ?? throw new ArgumentNullException("connection");
            Capabilities = capabilities
                ?? throw new ArgumentNullException("capabilities");
            ReadRequest = readRequest
                ?? throw new ArgumentNullException("readRequest");
            if (expectedResultData == null)
            {
                throw new ArgumentNullException("expectedResultData");
            }

            this.expectedResultData = (byte[])expectedResultData.Clone();
        }

        internal LMCConnection Connection { get; private set; }
        internal LMCDiagnosticCapabilities Capabilities { get; private set; }
        internal LMCSdoRequest ReadRequest { get; private set; }

        internal byte[] CopyExpectedResultData()
        {
            return (byte[])expectedResultData.Clone();
        }
    }

    internal sealed class D5SdoQueuedCancelRecoveryScope
    {
        internal D5SdoQueuedCancelRecoveryScope(
            D5SdoQueuedCancelQualificationRequest request)
        {
            Request = request ?? throw new ArgumentNullException("request");
            Stage = "PREFLIGHT_COMPLETE";
        }

        internal D5SdoQueuedCancelQualificationRequest Request
        {
            get;
            private set;
        }

        internal string Stage { get; set; }
        internal bool TargetSubmitAttempted { get; set; }
        internal bool TargetSubmissionOutcomeUncertain { get; set; }
        internal bool CancelAttempted { get; set; }
        internal bool CancelAccepted { get; set; }
        internal bool CancelOutcomeUncertain { get; set; }
        internal bool CancelInvalidStateRace { get; set; }
        internal bool RecoverySubmitAttempted { get; set; }
        internal bool RecoverySubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket TargetTicket { get; set; }
        internal LMCOperationStatus TargetTerminalStatus { get; set; }
        internal LMCOperationTicket RecoveryTicket { get; set; }

        internal bool HasAcceptedTickets
        {
            get { return TargetTicket != null || RecoveryTicket != null; }
        }

        internal bool HasUncertainSubmissionOutcome
        {
            get
            {
                return TargetSubmissionOutcomeUncertain
                    || RecoverySubmissionOutcomeUncertain;
            }
        }
    }

    internal sealed class D5SdoQueuedCancelQualificationResult
    {
        internal D5SdoQueuedCancelQualificationResult(
            D5SdoQueuedCancelQualificationDisposition disposition,
            D5SdoQueuedCancelRecoveryScope recoveryScope,
            LMCOperationStatus targetTerminalStatus,
            LMCOperationStatus recoveryTerminalStatus,
            Exception invalidStateRaceException)
        {
            Disposition = disposition;
            RecoveryScope = recoveryScope
                ?? throw new ArgumentNullException("recoveryScope");
            TargetTerminalStatus = targetTerminalStatus
                ?? throw new ArgumentNullException("targetTerminalStatus");
            RecoveryTerminalStatus = recoveryTerminalStatus;
            InvalidStateRaceException = invalidStateRaceException;
        }

        internal D5SdoQueuedCancelQualificationDisposition Disposition
        {
            get;
            private set;
        }

        internal D5SdoQueuedCancelRecoveryScope RecoveryScope
        {
            get;
            private set;
        }

        internal LMCOperationStatus TargetTerminalStatus
        {
            get;
            private set;
        }

        internal LMCOperationStatus RecoveryTerminalStatus
        {
            get;
            private set;
        }

        internal Exception InvalidStateRaceException
        {
            get;
            private set;
        }
    }

    internal sealed class D5SdoQueuedCancelQualificationOperations
    {
        internal Func<LMCSdoRequest, CancellationToken,
            Task<LMCOperationTicket>> SubmitAsync { get; set; }
        internal Func<LMCOperationTicket, CancellationToken, Task>
            CancelAsync { get; set; }
        internal Func<LMCOperationTicket, CancellationToken,
            Task<LMCOperationStatus>> WaitForTerminalAsync { get; set; }
        internal Action<D5SdoQueuedCancelRecoveryScope, Exception>
            RecoveryRequired { get; set; }

        internal void Validate()
        {
            if (SubmitAsync == null
                || CancelAsync == null
                || WaitForTerminalAsync == null
                || RecoveryRequired == null)
            {
                throw new ArgumentException(
                    "Submit, one-shot cancel, terminal wait, and recovery publication operations are required.");
            }
        }
    }

    internal static class D5SdoQueuedCancelQualificationOrchestrator
    {
        internal static async Task<D5SdoQueuedCancelQualificationResult>
            RunAsync(
                D5SdoQueuedCancelQualificationRequest request,
                D5SdoQueuedCancelQualificationOperations operations,
                CancellationToken cancellationToken)
        {
            ValidatePreflight(request);
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            operations.Validate();
            var scope = new D5SdoQueuedCancelRecoveryScope(request);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "SUBMIT_TARGET";
                scope.TargetSubmitAttempted = true;
                try
                {
                    scope.TargetTicket = await operations.SubmitAsync(
                        request.ReadRequest,
                        cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(scope, error, false);
                    throw;
                }

                scope.Stage = "TARGET_ACCEPTED";
                ValidateTicket(scope.TargetTicket, request, "target");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "CANCEL_TARGET";
                scope.CancelAttempted = true;
                Exception invalidStateRace = null;
                try
                {
                    await operations.CancelAsync(
                        scope.TargetTicket,
                        cancellationToken);
                    scope.CancelAccepted = true;
                }
                catch (Exception error)
                {
                    if (IsExactInvalidState(error))
                    {
                        invalidStateRace = error;
                        scope.CancelInvalidStateRace = true;
                    }
                    else
                    {
                        scope.CancelOutcomeUncertain =
                            !HasExactCommandResponse(error);
                        throw;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = scope.CancelInvalidStateRace
                    ? "WAIT_RACE_TERMINAL"
                    : "WAIT_CANCELLED_TERMINAL";
                scope.TargetTerminalStatus =
                    await operations.WaitForTerminalAsync(
                        scope.TargetTicket,
                        cancellationToken);

                if (scope.CancelInvalidStateRace)
                {
                    ValidateAnyTerminalStatus(
                        scope.TargetTicket,
                        scope.TargetTerminalStatus,
                        request,
                        "race target");
                    scope.Stage = "NOT_QUALIFIED_RACE";
                    return new D5SdoQueuedCancelQualificationResult(
                        D5SdoQueuedCancelQualificationDisposition
                            .NotQualifiedRace,
                        scope,
                        scope.TargetTerminalStatus,
                        null,
                        invalidStateRace);
                }

                ValidateCancelledStatus(
                    scope.TargetTicket,
                    scope.TargetTerminalStatus,
                    request);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_RECOVERY";
                scope.RecoverySubmitAttempted = true;
                try
                {
                    scope.RecoveryTicket = await operations.SubmitAsync(
                        request.ReadRequest,
                        cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(scope, error, true);
                    throw;
                }

                scope.Stage = "RECOVERY_ACCEPTED";
                ValidateTicket(scope.RecoveryTicket, request, "recovery");
                if (scope.RecoveryTicket.TicketId
                    == scope.TargetTicket.TicketId)
                {
                    throw new InvalidOperationException(
                        "The queued-cancel recovery SDO Read reused the target ticket identity.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "WAIT_RECOVERY_TERMINAL";
                var recoveryTerminalStatus =
                    await operations.WaitForTerminalAsync(
                        scope.RecoveryTicket,
                        cancellationToken);
                ValidateRecoveryStatus(
                    scope.RecoveryTicket,
                    recoveryTerminalStatus,
                    request);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "COMPLETE";
                return new D5SdoQueuedCancelQualificationResult(
                    D5SdoQueuedCancelQualificationDisposition.Qualified,
                    scope,
                    scope.TargetTerminalStatus,
                    recoveryTerminalStatus,
                    null);
            }
            catch (Exception primaryError)
            {
                try
                {
                    operations.RecoveryRequired(scope, primaryError);
                }
                catch (Exception recoveryError)
                {
                    throw new InvalidOperationException(
                        "D5 SDO queued-cancel recovery scope publication failed.",
                        new AggregateException(
                            primaryError,
                            recoveryError));
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw;
            }
        }

        private static void ValidatePreflight(
            D5SdoQueuedCancelQualificationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var capabilities = request.Capabilities;
            if (!capabilities.Supports(LMCDiagnosticCapability.SDORead)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline)
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || capabilities.MaxSdoDataBytes != 4)
            {
                throw new NotSupportedException(
                    "D5 queued-cancel qualification requires SDO Read/general-inline, nonzero BootId/MapRevision, and MaxSdoDataBytes=4.");
            }

            var readRequest = request.ReadRequest;
            if (readRequest.IsWrite
                || readRequest.SlaveReference < 1
                || readRequest.SlaveReference > 4
                || readRequest.ObjectIndex != 0x6061
                || readRequest.SubIndex != 0
                || readRequest.ValueType != LMCSignalValueType.Int8
                || readRequest.DataLength != 1
                || readRequest.TimeoutCycles < 1
                || readRequest.TimeoutCycles > 60000)
            {
                throw new ArgumentException(
                    "The queued-cancel probe must be Slave 1..4, 0x6061:0, Int8/1 Read, with TimeoutCycles 1..60000.",
                    "request");
            }

            if (request.CopyExpectedResultData().Length != 1)
            {
                throw new ArgumentException(
                    "The queued-cancel recovery expected result must be exactly one byte.",
                    "request");
            }
        }

        private static void CaptureSubmissionFailure(
            D5SdoQueuedCancelRecoveryScope scope,
            Exception error,
            bool recovery)
        {
            LMCSdoSubmissionFailureContext context;
            if (!LMCSdoSubmissionFailureContext.TryGet(error, out context))
            {
                return;
            }

            if (context.SubmissionOutcome
                == LMCSdoSubmissionOutcome.OutcomeUncertain)
            {
                if (recovery)
                {
                    scope.RecoverySubmissionOutcomeUncertain = true;
                }
                else
                {
                    scope.TargetSubmissionOutcomeUncertain = true;
                }
            }
            else if (context.SubmissionOutcome
                == LMCSdoSubmissionOutcome.Accepted)
            {
                if (recovery)
                {
                    scope.RecoveryTicket = context.Ticket;
                }
                else
                {
                    scope.TargetTicket = context.Ticket;
                }
            }
        }

        private static bool IsExactInvalidState(Exception error)
        {
            var commandError = error as LMCDiagnosticsCommandException;
            return commandError != null
                && commandError.Response != null
                && commandError.Response.Detail
                    == LMCDiagnosticsDetailCode.InvalidState;
        }

        private static bool HasExactCommandResponse(Exception error)
        {
            var commandError = error as LMCDiagnosticsCommandException;
            return commandError != null && commandError.Response != null;
        }

        private static void ValidateTicket(
            LMCOperationTicket ticket,
            D5SdoQueuedCancelQualificationRequest request,
            string label)
        {
            if (ticket == null
                || !ticket.BelongsToCurrentSession(request.Connection)
                || ticket.OperationKind != LMCOperationKind.SDORead
                || ticket.DiagnosticsBootId
                    != request.Capabilities.DiagnosticsBootId
                || ticket.SubmissionMapRevision
                    != request.Capabilities.MapRevision
                || ticket.ResultValueType
                    != request.ReadRequest.ValueType
                || ticket.RequestedResultLength
                    != request.ReadRequest.DataLength)
            {
                throw new InvalidOperationException(
                    "The " + label
                    + " queued-cancel ticket provenance, identity, type, or length is invalid.");
            }
        }

        private static void ValidateAnyTerminalStatus(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoQueuedCancelQualificationRequest request,
            string label)
        {
            ValidateTicket(ticket, request, label);
            if (status == null
                || status.Response == null
                || !status.Response.IsSuccess
                || status.TicketId != ticket.TicketId
                || status.OperationKind != ticket.OperationKind
                || status.DiagnosticsBootId != ticket.DiagnosticsBootId
                || status.SubmitCycle != ticket.QueuedCycle
                || status.CompletionCycle == 0
                || !status.IsTerminal)
            {
                throw new InvalidOperationException(
                    "The " + label
                    + " queued-cancel status is not an exact terminal status for its ticket.");
            }
        }

        private static void ValidateCancelledStatus(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoQueuedCancelQualificationRequest request)
        {
            ValidateAnyTerminalStatus(
                ticket,
                status,
                request,
                "cancelled target");
            if (status.State != LMCOperationState.Cancelled
                || status.Outcome != LMCOperationOutcome.Cancelled
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultLength != 0
                || status.ResultValueType != LMCSignalValueType.Invalid
                || status.ResultData.Length != 0)
            {
                throw new InvalidOperationException(
                    "The accepted queued CancelOperation did not end in exact Cancelled/Cancelled with zero error, detail, and result data.");
            }
        }

        private static void ValidateRecoveryStatus(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoQueuedCancelQualificationRequest request)
        {
            ValidateAnyTerminalStatus(
                ticket,
                status,
                request,
                "recovery");
            var expected = request.CopyExpectedResultData();
            var actual = status.ResultData;
            if (status.State != LMCOperationState.Completed
                || status.Outcome != LMCOperationOutcome.Success
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultLength != expected.Length
                || status.ResultValueType != request.ReadRequest.ValueType
                || actual.Length != expected.Length)
            {
                throw new InvalidOperationException(
                    "The queued-cancel recovery SDO Read did not complete with the exact expected type, length, and success state.");
            }

            for (var index = 0; index < expected.Length; index++)
            {
                if (actual[index] != expected[index])
                {
                    throw new InvalidOperationException(
                        "The queued-cancel recovery SDO Read value differs from the baseline.");
                }
            }
        }
    }
}

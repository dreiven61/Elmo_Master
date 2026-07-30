using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class D5SdoTimeoutQualificationRequest
    {
        private readonly byte[] expectedRecoveryData;

        internal D5SdoTimeoutQualificationRequest(
            LMCConnection connection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoRequest timeoutRequest,
            LMCSdoRequest recoveryRequest,
            byte[] expectedRecoveryData)
        {
            Connection = connection
                ?? throw new ArgumentNullException("connection");
            Capabilities = capabilities
                ?? throw new ArgumentNullException("capabilities");
            TimeoutRequest = timeoutRequest
                ?? throw new ArgumentNullException("timeoutRequest");
            RecoveryRequest = recoveryRequest
                ?? throw new ArgumentNullException("recoveryRequest");
            if (expectedRecoveryData == null)
            {
                throw new ArgumentNullException("expectedRecoveryData");
            }

            this.expectedRecoveryData =
                (byte[])expectedRecoveryData.Clone();
        }

        internal LMCConnection Connection { get; private set; }
        internal LMCDiagnosticCapabilities Capabilities { get; private set; }
        internal LMCSdoRequest TimeoutRequest { get; private set; }
        internal LMCSdoRequest RecoveryRequest { get; private set; }
        internal byte[] ExpectedRecoveryData
        {
            get { return (byte[])expectedRecoveryData.Clone(); }
        }

        internal byte[] CopyExpectedRecoveryData()
        {
            return (byte[])expectedRecoveryData.Clone();
        }
    }

    internal sealed class D5SdoTimeoutRecoveryScope
    {
        internal D5SdoTimeoutRecoveryScope(
            D5SdoTimeoutQualificationRequest request)
        {
            Request = request ?? throw new ArgumentNullException("request");
            Stage = "PREFLIGHT";
        }

        internal D5SdoTimeoutQualificationRequest Request
        {
            get;
            private set;
        }

        internal string Stage { get; set; }
        internal bool TimeoutSubmitAttempted { get; set; }
        internal LMCOperationTicket TimeoutTicket { get; set; }
        internal bool TimeoutSubmissionOutcomeUncertain { get; set; }
        internal int RecoverySubmitAttemptCount { get; set; }
        internal int RecoveryResourceBusyRejectionCount { get; set; }
        internal bool RecoverySubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket RecoveryTicket { get; set; }
        internal Exception LastResourceBusyException { get; set; }

        internal bool HasAcceptedTickets
        {
            get
            {
                return TimeoutTicket != null || RecoveryTicket != null;
            }
        }

        internal bool HasUncertainSubmissionOutcome
        {
            get
            {
                return TimeoutSubmissionOutcomeUncertain
                    || RecoverySubmissionOutcomeUncertain;
            }
        }
    }

    internal sealed class D5SdoTimeoutQualificationResult
    {
        internal D5SdoTimeoutQualificationResult(
            D5SdoTimeoutRecoveryScope recoveryScope,
            LMCOperationStatus timeoutTerminalStatus,
            LMCOperationStatus recoveryTerminalStatus)
        {
            RecoveryScope = recoveryScope
                ?? throw new ArgumentNullException("recoveryScope");
            TimeoutTerminalStatus = timeoutTerminalStatus
                ?? throw new ArgumentNullException("timeoutTerminalStatus");
            RecoveryTerminalStatus = recoveryTerminalStatus
                ?? throw new ArgumentNullException("recoveryTerminalStatus");
        }

        internal D5SdoTimeoutRecoveryScope RecoveryScope
        {
            get;
            private set;
        }

        internal LMCOperationStatus TimeoutTerminalStatus
        {
            get;
            private set;
        }

        internal LMCOperationStatus RecoveryTerminalStatus
        {
            get;
            private set;
        }
    }

    internal sealed class D5SdoTimeoutQualificationOperations
    {
        internal Func<LMCSdoRequest, CancellationToken,
            Task<LMCOperationTicket>> SubmitAsync { get; set; }
        internal Func<LMCOperationTicket, CancellationToken,
            Task<LMCOperationStatus>> WaitForTerminalAsync { get; set; }
        internal Func<int, CancellationToken, Task> DelayAsync { get; set; }
        internal Action<D5SdoTimeoutRecoveryScope, Exception>
            RecoveryRequired { get; set; }

        internal void Validate()
        {
            if (SubmitAsync == null
                || WaitForTerminalAsync == null
                || DelayAsync == null
                || RecoveryRequired == null)
            {
                throw new ArgumentException(
                    "Submit, terminal wait, delay, and recovery publication operations are required.");
            }
        }
    }

    internal static class D5SdoTimeoutQualificationOrchestrator
    {
        internal const int MaximumRecoverySubmitAttempts = 600;
        internal const int RecoveryRetryDelayMilliseconds = 25;
        internal const uint EtherCatSdoTimeoutDetail = 0x05040000u;

        internal static async Task<D5SdoTimeoutQualificationResult>
            RunAsync(
                D5SdoTimeoutQualificationRequest request,
                D5SdoTimeoutQualificationOperations operations,
                CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            if (operations.RecoveryRequired == null)
            {
                throw new ArgumentException(
                    "A recovery scope publication operation is required.",
                    "operations");
            }

            var scope = new D5SdoTimeoutRecoveryScope(request);
            try
            {
                operations.Validate();
                ValidatePreflight(request);
                scope.Stage = "PREFLIGHT_COMPLETE";
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_TIMEOUT";
                scope.TimeoutSubmitAttempted = true;
                try
                {
                    scope.TimeoutTicket = await operations.SubmitAsync(
                        request.TimeoutRequest,
                        cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        error,
                        false);
                    throw;
                }

                scope.Stage = "TIMEOUT_ACCEPTED";
                ValidateTicket(
                    scope.TimeoutTicket,
                    request,
                    request.TimeoutRequest,
                    "timeout");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "WAIT_TIMEOUT_TERMINAL";
                var timeoutTerminalStatus =
                    await operations.WaitForTerminalAsync(
                        scope.TimeoutTicket,
                        cancellationToken);
                ValidateTimeoutTerminal(
                    scope.TimeoutTicket,
                    timeoutTerminalStatus,
                    request);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_RECOVERY";
                for (var attempt = 1;
                    attempt <= MaximumRecoverySubmitAttempts;
                    attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    scope.RecoverySubmitAttemptCount = attempt;
                    try
                    {
                        scope.RecoveryTicket =
                            await operations.SubmitAsync(
                                request.RecoveryRequest,
                                cancellationToken);
                    }
                    catch (Exception error)
                    {
                        CaptureSubmissionFailure(
                            scope,
                            error,
                            true);
                        if (!IsExactResourceBusyRejection(
                                error,
                                request))
                        {
                            throw;
                        }

                        scope.RecoveryResourceBusyRejectionCount++;
                        scope.LastResourceBusyException = error;
                        if (attempt >= MaximumRecoverySubmitAttempts)
                        {
                            throw new TimeoutException(
                                "The timed-out D5 SDO executor remained ResourceBusy for all 600 bounded recovery Submit attempts.",
                                error);
                        }

                        await operations.DelayAsync(
                            RecoveryRetryDelayMilliseconds,
                            cancellationToken);
                        continue;
                    }

                    break;
                }

                scope.Stage = "RECOVERY_ACCEPTED";
                ValidateTicket(
                    scope.RecoveryTicket,
                    request,
                    request.RecoveryRequest,
                    "recovery");
                if (scope.RecoveryTicket.TicketId
                    == scope.TimeoutTicket.TicketId)
                {
                    throw new InvalidOperationException(
                        "The recovery SDO Read reused the timed-out ticket identity.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "WAIT_RECOVERY_TERMINAL";
                var recoveryTerminalStatus =
                    await operations.WaitForTerminalAsync(
                        scope.RecoveryTicket,
                        cancellationToken);
                ValidateRecoveryTerminal(
                    scope.RecoveryTicket,
                    recoveryTerminalStatus,
                    request);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "COMPLETE";
                return new D5SdoTimeoutQualificationResult(
                    scope,
                    timeoutTerminalStatus,
                    recoveryTerminalStatus);
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
                        "D5 SDO timeout recovery scope publication failed.",
                        new AggregateException(
                            primaryError,
                            recoveryError));
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw;
            }
        }

        private static void ValidatePreflight(
            D5SdoTimeoutQualificationRequest request)
        {
            var capabilities = request.Capabilities;
            if (!capabilities.Supports(
                    LMCDiagnosticCapability.SDORead)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline))
            {
                throw new NotSupportedException(
                    "D5 timeout qualification requires SDO Read and general-inline capabilities.");
            }

            if (capabilities.MaxSdoDataBytes != 4)
            {
                throw new InvalidOperationException(
                    "D5 timeout qualification requires MaxSdoDataBytes=4.");
            }

            if (capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "D5 timeout qualification requires nonzero DiagnosticsBootId and MapRevision values.");
            }

            ValidateCanonicalRead(request.TimeoutRequest, "timeoutRequest");
            ValidateCanonicalRead(request.RecoveryRequest, "recoveryRequest");
            if (request.TimeoutRequest.TimeoutCycles != 1)
            {
                throw new ArgumentException(
                    "The timeout probe must use exactly one TimeoutCycle.",
                    "request");
            }

            if (request.RecoveryRequest.TimeoutCycles < 2
                || request.RecoveryRequest.TimeoutCycles > 60000)
            {
                throw new ArgumentException(
                    "The recovery SDO Read TimeoutCycles must be 2..60000.",
                    "request");
            }

            if (!HasSameTarget(
                    request.TimeoutRequest,
                    request.RecoveryRequest))
            {
                throw new ArgumentException(
                    "Timeout and recovery SDO Reads must use the exact same canonical target.",
                    "request");
            }

            if (request.CopyExpectedRecoveryData().Length != 1)
            {
                throw new ArgumentException(
                    "The expected Int8 recovery value must contain exactly one byte.",
                    "request");
            }
        }

        private static void ValidateCanonicalRead(
            LMCSdoRequest request,
            string parameterName)
        {
            if (request.IsWrite
                || request.SlaveReference < 1
                || request.SlaveReference > 4
                || request.ObjectIndex != 0x6061
                || request.SubIndex != 0
                || request.ValueType != LMCSignalValueType.Int8
                || request.DataLength != 1)
            {
                throw new ArgumentException(
                    "D5 timeout qualification requires a Slave 1..4 0x6061:0 Int8/1 SDO Read.",
                    parameterName);
            }
        }

        private static bool HasSameTarget(
            LMCSdoRequest left,
            LMCSdoRequest right)
        {
            return left.SlaveReference == right.SlaveReference
                && left.ObjectIndex == right.ObjectIndex
                && left.SubIndex == right.SubIndex
                && left.ValueType == right.ValueType
                && left.DataLength == right.DataLength
                && left.OperationFlags == right.OperationFlags;
        }

        private static void CaptureSubmissionFailure(
            D5SdoTimeoutRecoveryScope scope,
            Exception error,
            bool isRecovery)
        {
            LMCSdoSubmissionFailureContext context;
            if (!LMCSdoSubmissionFailureContext.TryGet(
                    error,
                    out context))
            {
                return;
            }

            if (context.SubmissionOutcome
                == LMCSdoSubmissionOutcome.OutcomeUncertain)
            {
                if (isRecovery)
                {
                    scope.RecoverySubmissionOutcomeUncertain = true;
                }
                else
                {
                    scope.TimeoutSubmissionOutcomeUncertain = true;
                }

                return;
            }

            if (context.SubmissionOutcome
                != LMCSdoSubmissionOutcome.Accepted)
            {
                return;
            }

            if (isRecovery)
            {
                scope.RecoveryTicket = context.Ticket;
            }
            else
            {
                scope.TimeoutTicket = context.Ticket;
            }
        }

        private static bool IsExactResourceBusyRejection(
            Exception error,
            D5SdoTimeoutQualificationRequest request)
        {
            var commandError = error as LMCDiagnosticsCommandException;
            if (commandError == null
                || commandError.Response == null
                || commandError.Response.Detail
                    != LMCDiagnosticsDetailCode.ResourceBusy)
            {
                return false;
            }

            LMCSdoSubmissionFailureContext context;
            return LMCSdoSubmissionFailureContext.TryGet(
                    error,
                    out context)
                && context.Phase == LMCSdoSubmissionPhase.Submission
                && context.SubmissionOutcome
                    == LMCSdoSubmissionOutcome.Rejected
                && ReferenceEquals(
                    context.Request,
                    request.RecoveryRequest)
                && context.Ticket == null
                && context.DiagnosticsBootId
                    == request.Capabilities.DiagnosticsBootId
                && context.MapRevision
                    == request.Capabilities.MapRevision;
        }

        private static void ValidateTicket(
            LMCOperationTicket ticket,
            D5SdoTimeoutQualificationRequest request,
            LMCSdoRequest submittedRequest,
            string label)
        {
            if (ticket == null)
            {
                throw new InvalidOperationException(
                    "The " + label
                    + " SDO Read Submit did not return a ticket.");
            }

            if (!ticket.BelongsToCurrentSession(request.Connection)
                || ticket.OperationKind != LMCOperationKind.SDORead
                || ticket.DiagnosticsBootId
                    != request.Capabilities.DiagnosticsBootId
                || ticket.SubmissionMapRevision
                    != request.Capabilities.MapRevision
                || ticket.ResultValueType != submittedRequest.ValueType
                || ticket.RequestedResultLength
                    != submittedRequest.DataLength)
            {
                throw new InvalidOperationException(
                    "The " + label
                    + " SDO Read ticket provenance, BootId, MapRevision, type, or length is invalid.");
            }
        }

        private static void ValidateTimeoutTerminal(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoTimeoutQualificationRequest request)
        {
            ValidateTicket(
                ticket,
                request,
                request.TimeoutRequest,
                "timeout");
            if (!HasExactStatusIdentity(ticket, status)
                || status.State != LMCOperationState.Expired
                || status.Outcome != LMCOperationOutcome.TimedOut
                || status.OperationErrorId != 0
                || status.OperationDetail != EtherCatSdoTimeoutDetail
                || status.ResultLength != 0
                || status.ResultValueType != LMCSignalValueType.Invalid
                || status.ResultData.Length != 0
                || unchecked(status.CompletionCycle - status.SubmitCycle)
                    < request.TimeoutRequest.TimeoutCycles)
            {
                throw new InvalidOperationException(
                    "The timeout SDO Read terminal status is not exact Expired/TimedOut with 0x05040000 and no result data.");
            }
        }

        private static void ValidateRecoveryTerminal(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoTimeoutQualificationRequest request)
        {
            ValidateTicket(
                ticket,
                request,
                request.RecoveryRequest,
                "recovery");
            var resultData = status == null
                ? null
                : status.ResultData;
            if (!HasExactStatusIdentity(ticket, status)
                || status.State != LMCOperationState.Completed
                || status.Outcome != LMCOperationOutcome.Success
                || !status.IsSuccessful
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultLength
                    != request.RecoveryRequest.DataLength
                || status.ResultValueType
                    != request.RecoveryRequest.ValueType
                || !ByteArraysEqual(
                    resultData,
                    request.CopyExpectedRecoveryData()))
            {
                throw new InvalidOperationException(
                    "The recovery SDO Read terminal status is not exact Completed/Success with the baseline Int8 value.");
            }
        }

        private static bool HasExactStatusIdentity(
            LMCOperationTicket ticket,
            LMCOperationStatus status)
        {
            return status != null
                && status.Response != null
                && status.Response.IsSuccess
                && status.Response.ErrorId == 0
                && status.Response.Detail
                    == LMCDiagnosticsDetailCode.None
                && status.TicketId == ticket.TicketId
                && status.OperationKind == ticket.OperationKind
                && status.DiagnosticsBootId == ticket.DiagnosticsBootId
                && status.SubmitCycle == ticket.QueuedCycle;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                {
                    return false;
                }
            }

            return true;
        }
    }
}

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class D5SdoContentionQualificationRequest
    {
        private readonly byte[] expectedResultData;

        internal D5SdoContentionQualificationRequest(
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

            this.expectedResultData =
                (byte[])expectedResultData.Clone();
        }

        internal LMCConnection Connection { get; private set; }
        internal LMCDiagnosticCapabilities Capabilities { get; private set; }
        internal LMCSdoRequest ReadRequest { get; private set; }
        internal byte[] ExpectedResultData
        {
            get { return (byte[])expectedResultData.Clone(); }
        }

        internal byte[] CopyExpectedResultData()
        {
            return (byte[])expectedResultData.Clone();
        }
    }

    internal sealed class D5SdoContentionRecoveryScope
    {
        internal D5SdoContentionRecoveryScope(
            D5SdoContentionQualificationRequest request)
        {
            Request = request ?? throw new ArgumentNullException("request");
            Stage = "PREFLIGHT_COMPLETE";
        }

        internal D5SdoContentionQualificationRequest Request
        {
            get;
            private set;
        }

        internal string Stage { get; set; }
        internal bool FirstSubmitAttempted { get; set; }
        internal bool SecondSubmitAttempted { get; set; }
        internal bool ThirdSubmitAttempted { get; set; }
        internal bool SecondExactResourceBusyConfirmed { get; set; }
        internal bool FirstSubmissionOutcomeUncertain { get; set; }
        internal bool SecondSubmissionOutcomeUncertain { get; set; }
        internal bool ThirdSubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket FirstTicket { get; set; }
        internal LMCOperationTicket UnexpectedSecondTicket { get; set; }
        internal LMCOperationTicket ThirdTicket { get; set; }

        internal bool HasAcceptedTickets
        {
            get
            {
                return FirstTicket != null
                    || UnexpectedSecondTicket != null
                    || ThirdTicket != null;
            }
        }

        internal bool HasUncertainSubmissionOutcome
        {
            get
            {
                return FirstSubmissionOutcomeUncertain
                    || SecondSubmissionOutcomeUncertain
                    || ThirdSubmissionOutcomeUncertain;
            }
        }
    }

    internal sealed class D5SdoContentionQualificationResult
    {
        internal D5SdoContentionQualificationResult(
            D5SdoContentionRecoveryScope recoveryScope,
            LMCOperationStatus firstTerminalStatus,
            LMCOperationStatus thirdTerminalStatus,
            Exception secondResourceBusyException)
        {
            RecoveryScope = recoveryScope
                ?? throw new ArgumentNullException("recoveryScope");
            FirstTerminalStatus = firstTerminalStatus
                ?? throw new ArgumentNullException("firstTerminalStatus");
            ThirdTerminalStatus = thirdTerminalStatus
                ?? throw new ArgumentNullException("thirdTerminalStatus");
            SecondResourceBusyException = secondResourceBusyException
                ?? throw new ArgumentNullException(
                    "secondResourceBusyException");
        }

        internal D5SdoContentionRecoveryScope RecoveryScope
        {
            get;
            private set;
        }

        internal LMCOperationStatus FirstTerminalStatus
        {
            get;
            private set;
        }

        internal LMCOperationStatus ThirdTerminalStatus
        {
            get;
            private set;
        }

        internal Exception SecondResourceBusyException
        {
            get;
            private set;
        }
    }

    internal sealed class D5SdoContentionQualificationOperations
    {
        internal Func<LMCSdoRequest, CancellationToken,
            Task<LMCOperationTicket>> SubmitAsync { get; set; }
        internal Func<LMCOperationTicket, CancellationToken,
            Task<LMCOperationStatus>> WaitForTerminalAsync { get; set; }
        internal Action<D5SdoContentionRecoveryScope, Exception>
            RecoveryRequired { get; set; }

        internal void Validate()
        {
            if (SubmitAsync == null
                || WaitForTerminalAsync == null
                || RecoveryRequired == null)
            {
                throw new ArgumentException(
                    "Submit, terminal wait, and recovery publication operations are required.");
            }
        }
    }

    internal static class D5SdoContentionQualificationOrchestrator
    {
        internal static async Task<D5SdoContentionQualificationResult>
            RunAsync(
                D5SdoContentionQualificationRequest request,
                D5SdoContentionQualificationOperations operations,
                CancellationToken cancellationToken)
        {
            ValidatePreflight(request);
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            operations.Validate();
            var scope = new D5SdoContentionRecoveryScope(request);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_FIRST";
                scope.FirstSubmitAttempted = true;
                try
                {
                    scope.FirstTicket = await operations.SubmitAsync(
                        request.ReadRequest,
                        cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        error,
                        D5SdoContentionSubmitOrdinal.First);
                    throw;
                }

                scope.Stage = "FIRST_ACCEPTED";
                ValidateTicket(scope.FirstTicket, request, "first");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_SECOND_CONCURRENT";
                scope.SecondSubmitAttempted = true;
                Exception busyException = null;
                try
                {
                    scope.UnexpectedSecondTicket =
                        await operations.SubmitAsync(
                            request.ReadRequest,
                            cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        error,
                        D5SdoContentionSubmitOrdinal.Second);
                    if (!IsExactResourceBusyRejection(error, request))
                    {
                        throw;
                    }

                    busyException = error;
                    scope.SecondExactResourceBusyConfirmed = true;
                }

                if (scope.UnexpectedSecondTicket != null)
                {
                    ValidateTicket(
                        scope.UnexpectedSecondTicket,
                        request,
                        "unexpected second");
                    throw new InvalidOperationException(
                        "The concurrent second SDO Read was unexpectedly accepted. Its ticket is preserved and the third Submit is blocked.");
                }

                if (busyException == null
                    || !scope.SecondExactResourceBusyConfirmed)
                {
                    throw new InvalidOperationException(
                        "The concurrent second SDO Read did not return exact ResourceBusy with a Rejected submission outcome.");
                }

                scope.Stage = "SECOND_RESOURCE_BUSY_CONFIRMED";
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "WAIT_FIRST_TERMINAL";
                var firstTerminalStatus =
                    await operations.WaitForTerminalAsync(
                        scope.FirstTicket,
                        cancellationToken);
                ValidateTerminalStatus(
                    scope.FirstTicket,
                    firstTerminalStatus,
                    request,
                    "first");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_THIRD";
                scope.ThirdSubmitAttempted = true;
                try
                {
                    scope.ThirdTicket = await operations.SubmitAsync(
                        request.ReadRequest,
                        cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        error,
                        D5SdoContentionSubmitOrdinal.Third);
                    throw;
                }

                scope.Stage = "THIRD_ACCEPTED";
                ValidateTicket(scope.ThirdTicket, request, "third");
                if (scope.ThirdTicket.TicketId == scope.FirstTicket.TicketId)
                {
                    throw new InvalidOperationException(
                        "The post-contention third SDO Read reused the first ticket identity.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "WAIT_THIRD_TERMINAL";
                var thirdTerminalStatus =
                    await operations.WaitForTerminalAsync(
                        scope.ThirdTicket,
                        cancellationToken);
                ValidateTerminalStatus(
                    scope.ThirdTicket,
                    thirdTerminalStatus,
                    request,
                    "third");
                EnsureResultConsistency(
                    firstTerminalStatus,
                    thirdTerminalStatus);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "COMPLETE";
                return new D5SdoContentionQualificationResult(
                    scope,
                    firstTerminalStatus,
                    thirdTerminalStatus,
                    busyException);
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
                        "D5 SDO contention recovery scope publication failed.",
                        new AggregateException(
                            primaryError,
                            recoveryError));
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw;
            }
        }

        private static void ValidatePreflight(
            D5SdoContentionQualificationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.ReadRequest.IsWrite)
            {
                throw new ArgumentException(
                    "D5 contention qualification requires an SDO Read request.",
                    "request");
            }

            var capabilities = request.Capabilities;
            if (!capabilities.Supports(
                    LMCDiagnosticCapability.SDORead)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline))
            {
                throw new NotSupportedException(
                    "D5 contention qualification requires SDO Read and general-inline capabilities.");
            }

            if (capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "D5 contention qualification requires nonzero DiagnosticsBootId and MapRevision values.");
            }

            if (capabilities.MaxSdoDataBytes != 4)
            {
                throw new InvalidOperationException(
                    "D5 contention qualification requires MaxSdoDataBytes=4.");
            }

            var readRequest = request.ReadRequest;
            if (readRequest.SlaveReference < 1
                || readRequest.SlaveReference > 4
                || readRequest.ObjectIndex != 0x6061
                || readRequest.SubIndex != 0
                || readRequest.ValueType != LMCSignalValueType.Int8
                || readRequest.DataLength != 1
                || readRequest.TimeoutCycles < 1
                || readRequest.TimeoutCycles > 60000)
            {
                throw new ArgumentException(
                    "The known-valid contention probe must be Slave 1..4, 0x6061:0, Int8/1, with TimeoutCycles 1..60000.",
                    "request");
            }

            if (request.CopyExpectedResultData().Length
                != request.ReadRequest.DataLength)
            {
                throw new ArgumentException(
                    "Expected result data must exactly match the SDO Read DataLength.",
                    "request");
            }
        }

        private static void CaptureSubmissionFailure(
            D5SdoContentionRecoveryScope scope,
            Exception error,
            D5SdoContentionSubmitOrdinal ordinal)
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
                SetOutcomeUncertain(scope, ordinal);
                return;
            }

            if (context.SubmissionOutcome
                != LMCSdoSubmissionOutcome.Accepted)
            {
                return;
            }

            switch (ordinal)
            {
                case D5SdoContentionSubmitOrdinal.First:
                    scope.FirstTicket = context.Ticket;
                    break;
                case D5SdoContentionSubmitOrdinal.Second:
                    scope.UnexpectedSecondTicket = context.Ticket;
                    break;
                case D5SdoContentionSubmitOrdinal.Third:
                    scope.ThirdTicket = context.Ticket;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ordinal");
            }
        }

        private static void SetOutcomeUncertain(
            D5SdoContentionRecoveryScope scope,
            D5SdoContentionSubmitOrdinal ordinal)
        {
            switch (ordinal)
            {
                case D5SdoContentionSubmitOrdinal.First:
                    scope.FirstSubmissionOutcomeUncertain = true;
                    break;
                case D5SdoContentionSubmitOrdinal.Second:
                    scope.SecondSubmissionOutcomeUncertain = true;
                    break;
                case D5SdoContentionSubmitOrdinal.Third:
                    scope.ThirdSubmissionOutcomeUncertain = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ordinal");
            }
        }

        private static bool IsExactResourceBusyRejection(
            Exception error,
            D5SdoContentionQualificationRequest request)
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
                && ReferenceEquals(context.Request, request.ReadRequest)
                && context.Ticket == null
                && context.DiagnosticsBootId
                    == request.Capabilities.DiagnosticsBootId
                && context.MapRevision
                    == request.Capabilities.MapRevision;
        }

        private static void ValidateTicket(
            LMCOperationTicket ticket,
            D5SdoContentionQualificationRequest request,
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
                || ticket.ResultValueType
                    != request.ReadRequest.ValueType
                || ticket.RequestedResultLength
                    != request.ReadRequest.DataLength)
            {
                throw new InvalidOperationException(
                    "The " + label
                    + " SDO Read ticket provenance, BootId, MapRevision, type, or length is invalid.");
            }
        }

        private static void ValidateTerminalStatus(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoContentionQualificationRequest request,
            string label)
        {
            ValidateTicket(ticket, request, label);
            if (status == null
                || status.Response == null
                || !status.Response.IsSuccess
                || status.TicketId != ticket.TicketId
                || status.OperationKind != LMCOperationKind.SDORead
                || status.DiagnosticsBootId != ticket.DiagnosticsBootId
                || status.SubmitCycle != ticket.QueuedCycle
                || status.State != LMCOperationState.Completed
                || status.Outcome != LMCOperationOutcome.Success
                || status.CompletionCycle == 0
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultValueType
                    != request.ReadRequest.ValueType
                || status.ResultLength
                    != request.ReadRequest.DataLength
                || status.ResultData.Length
                    != request.ReadRequest.DataLength
                || !ByteArraysEqual(
                    status.ResultData,
                    request.CopyExpectedResultData()))
            {
                throw new InvalidOperationException(
                    "The " + label
                    + " SDO Read terminal status is not exact Completed/Success with matching identity, type, length, and value.");
            }
        }

        private static void EnsureResultConsistency(
            LMCOperationStatus first,
            LMCOperationStatus third)
        {
            if (first.ResultValueType != third.ResultValueType
                || first.ResultLength != third.ResultLength
                || !ByteArraysEqual(
                    first.ResultData,
                    third.ResultData))
            {
                throw new InvalidOperationException(
                    "The first and post-contention SDO Read results are inconsistent.");
            }
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

        private enum D5SdoContentionSubmitOrdinal
        {
            First = 1,
            Second = 2,
            Third = 3
        }
    }
}

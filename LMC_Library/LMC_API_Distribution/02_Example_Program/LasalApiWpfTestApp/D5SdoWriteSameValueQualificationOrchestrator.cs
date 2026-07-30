using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class D5SdoWriteSameValueQualificationRequest
    {
        private readonly ReadOnlyCollection<LMCSdoWriteTarget>
            approvedTargets;

        internal D5SdoWriteSameValueQualificationRequest(
            LMCConnection connection,
            LMCDiagnosticCapabilities initialCapabilities,
            IReadOnlyList<LMCSdoWriteTarget> approvedTargets,
            LMCSdoWriteTarget target,
            uint timeoutCycles)
        {
            Connection = connection
                ?? throw new ArgumentNullException("connection");
            InitialCapabilities = initialCapabilities
                ?? throw new ArgumentNullException("initialCapabilities");
            if (approvedTargets == null)
            {
                throw new ArgumentNullException("approvedTargets");
            }

            Target = target ?? throw new ArgumentNullException("target");
            var copy = new LMCSdoWriteTarget[approvedTargets.Count];
            for (var index = 0; index < approvedTargets.Count; index++)
            {
                copy[index] = approvedTargets[index];
            }

            this.approvedTargets = Array.AsReadOnly(copy);
            TimeoutCycles = timeoutCycles;
        }

        internal LMCConnection Connection { get; private set; }
        internal LMCDiagnosticCapabilities InitialCapabilities
        {
            get;
            private set;
        }
        internal IReadOnlyList<LMCSdoWriteTarget> ApprovedTargets
        {
            get { return approvedTargets; }
        }
        internal LMCSdoWriteTarget Target { get; private set; }
        internal uint TimeoutCycles { get; private set; }
    }

    internal sealed class D5SdoWriteSameValueRecoveryScope
    {
        private byte[] baselineData;

        internal D5SdoWriteSameValueRecoveryScope(
            D5SdoWriteSameValueQualificationRequest request)
        {
            Request = request ?? throw new ArgumentNullException("request");
            Stage = "PREFLIGHT";
            baselineData = new byte[0];
        }

        internal D5SdoWriteSameValueQualificationRequest Request
        {
            get;
            private set;
        }

        internal string Stage { get; set; }
        internal LMCSdoRequest BaselineRequest { get; set; }
        internal bool BaselineSubmitAttempted { get; set; }
        internal bool BaselineSubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket BaselineTicket { get; set; }
        internal LMCOperationStatus BaselineStatus { get; set; }
        internal byte[] BaselineData
        {
            get { return (byte[])baselineData.Clone(); }
            set
            {
                baselineData = value == null
                    ? new byte[0]
                    : (byte[])value.Clone();
            }
        }
        internal LMCDiagnosticCapabilities PreWriteCapabilities
        {
            get;
            set;
        }
        internal LMCSdoRequest WriteRequest { get; set; }
        internal bool SafetyVerified { get; set; }
        internal bool ConfirmationAccepted { get; set; }
        internal bool SecondSafetyVerified { get; set; }
        internal LMCSdoRequest PreWriteGuardRequest { get; set; }
        internal bool PreWriteGuardSubmitAttempted { get; set; }
        internal bool PreWriteGuardSubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket PreWriteGuardTicket { get; set; }
        internal LMCOperationStatus PreWriteGuardStatus { get; set; }
        internal bool JournalArmAttempted { get; set; }
        internal bool JournalArmed { get; set; }
        internal bool WriteSubmitAttempted { get; set; }
        internal bool WriteSubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket WriteTicket { get; set; }
        internal LMCOperationStatus WriteStatus { get; set; }
        internal LMCSdoWriteVerificationContext VerificationContext
        {
            get;
            set;
        }
        internal LMCSdoRequest ReadbackRequest { get; set; }
        internal bool ReadbackSubmitAttempted { get; set; }
        internal bool ReadbackSubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket ReadbackTicket { get; set; }
        internal LMCOperationStatus ReadbackStatus { get; set; }
        internal LMCDiagnosticCapabilities FinalCapabilities { get; set; }
        internal bool ReadbackVerified { get; set; }
        internal bool JournalResolved { get; set; }

        internal bool HasAcceptedTickets
        {
            get
            {
                return BaselineTicket != null
                    || PreWriteGuardTicket != null
                    || WriteTicket != null
                    || ReadbackTicket != null;
            }
        }

        internal bool HasUncertainSubmissionOutcome
        {
            get
            {
                return BaselineSubmissionOutcomeUncertain
                    || PreWriteGuardSubmissionOutcomeUncertain
                    || WriteSubmissionOutcomeUncertain
                    || ReadbackSubmissionOutcomeUncertain;
            }
        }
    }

    internal sealed class D5SdoWriteSameValueQualificationResult
    {
        internal D5SdoWriteSameValueQualificationResult(
            D5SdoWriteSameValueRecoveryScope recoveryScope)
        {
            RecoveryScope = recoveryScope
                ?? throw new ArgumentNullException("recoveryScope");
        }

        internal D5SdoWriteSameValueRecoveryScope RecoveryScope
        {
            get;
            private set;
        }
    }

    internal sealed class D5SdoWriteSameValueQualificationOperations
    {
        internal Func<CancellationToken, Task<LMCDiagnosticCapabilities>>
            ReadCapabilitiesAsync { get; set; }
        internal Func<LMCSdoRequest, CancellationToken,
            Task<LMCOperationTicket>> SubmitAsync { get; set; }
        internal Func<LMCOperationTicket, CancellationToken,
            Task<LMCOperationStatus>> WaitForTerminalAsync { get; set; }
        internal Func<LMCSdoWriteTarget, CancellationToken, Task<bool>>
            VerifySafeAxisAsync { get; set; }
        internal Func<LMCSdoRequest, CancellationToken, Task<bool>>
            ConfirmWriteAsync { get; set; }
        internal Action<D5SdoWriteSameValueRecoveryScope,
            LMCSdoRequest, LMCDiagnosticCapabilities> ArmJournal
        {
            get;
            set;
        }
        internal Action<D5SdoWriteSameValueRecoveryScope,
            LMCOperationTicket> AdoptWriteTicketBeforeValidation
        {
            get;
            set;
        }
        internal Action<D5SdoWriteSameValueRecoveryScope,
            LMCOperationTicket> MarkWriteAccepted { get; set; }
        internal Action<D5SdoWriteSameValueRecoveryScope,
            LMCOperationTicket, LMCOperationStatus>
            MarkWriteTerminalSuccess { get; set; }
        internal Func<LMCSdoRequest, LMCOperationTicket,
            LMCOperationStatus, LMCSdoWriteVerificationContext>
            CreateVerificationContext { get; set; }
        internal Func<LMCSdoWriteVerificationContext, LMCSdoRequest,
            CancellationToken, Task<LMCOperationTicket>>
            SubmitReadbackAsync { get; set; }
        internal Action<D5SdoWriteSameValueRecoveryScope>
            ResolveJournalAfterVerified { get; set; }
        internal Action<D5SdoWriteSameValueRecoveryScope, Exception>
            RecoveryRequired { get; set; }

        internal void Validate()
        {
            if (ReadCapabilitiesAsync == null
                || SubmitAsync == null
                || WaitForTerminalAsync == null
                || VerifySafeAxisAsync == null
                || ConfirmWriteAsync == null
                || ArmJournal == null
                || AdoptWriteTicketBeforeValidation == null
                || MarkWriteAccepted == null
                || MarkWriteTerminalSuccess == null
                || CreateVerificationContext == null
                || SubmitReadbackAsync == null
                || ResolveJournalAfterVerified == null
                || RecoveryRequired == null)
            {
                throw new ArgumentException(
                    "Every same-value SDO Write qualification operation is required.");
            }
        }
    }

    internal static class D5SdoWriteSameValueQualificationOrchestrator
    {
        internal static async Task<
            D5SdoWriteSameValueQualificationResult> RunAsync(
                D5SdoWriteSameValueQualificationRequest request,
                D5SdoWriteSameValueQualificationOperations operations,
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
                    "A same-value SDO Write recovery publication operation is required.",
                    "operations");
            }

            var scope = new D5SdoWriteSameValueRecoveryScope(request);
            try
            {
                operations.Validate();
                ValidatePreflight(request);
                scope.Stage = "PREFLIGHT_COMPLETE";
                cancellationToken.ThrowIfCancellationRequested();

                scope.BaselineRequest = LMCSdoRequest.CreateRead(
                    request.Target.SlaveReference,
                    request.Target.ObjectIndex,
                    request.Target.SubIndex,
                    request.Target.ValueType,
                    request.Target.DataLength,
                    request.TimeoutCycles);
                scope.Stage = "SUBMIT_BASELINE_READ";
                scope.BaselineSubmitAttempted = true;
                try
                {
                    scope.BaselineTicket = await operations.SubmitAsync(
                        scope.BaselineRequest,
                        cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        scope.BaselineRequest,
                        error,
                        SubmissionSlot.Baseline);
                    throw;
                }

                ValidateTicket(
                    scope.BaselineTicket,
                    request,
                    scope.BaselineRequest,
                    LMCOperationKind.SDORead,
                    request.InitialCapabilities,
                    "baseline");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "WAIT_BASELINE_TERMINAL";
                scope.BaselineStatus =
                    await operations.WaitForTerminalAsync(
                        scope.BaselineTicket,
                        cancellationToken);
                ValidateReadTerminal(
                    scope.BaselineTicket,
                    scope.BaselineStatus,
                    request,
                    scope.BaselineRequest,
                    null,
                    "baseline");
                scope.BaselineData = scope.BaselineStatus.ResultData;
                scope.WriteRequest = CreateSameValueWriteRequest(
                    request,
                    scope.BaselineData);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "READ_PREWRITE_CAPABILITIES";
                scope.PreWriteCapabilities =
                    await operations.ReadCapabilitiesAsync(
                        cancellationToken);
                ValidateFreshStableCapabilities(
                    request.InitialCapabilities,
                    scope.PreWriteCapabilities,
                    request.Connection,
                    "pre-Write");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "VERIFY_SAFE_AXIS";
                if (!await operations.VerifySafeAxisAsync(
                        request.Target,
                        cancellationToken))
                {
                    throw new InvalidOperationException(
                        "Same-value SDO Write qualification requires a verified safe axis state.");
                }

                scope.SafetyVerified = true;
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "CONFIRM_WRITE";
                if (!await operations.ConfirmWriteAsync(
                        scope.WriteRequest,
                        cancellationToken))
                {
                    throw new InvalidOperationException(
                        "Same-value SDO Write qualification was not explicitly confirmed.");
                }

                scope.ConfirmationAccepted = true;
                cancellationToken.ThrowIfCancellationRequested();

                scope.PreWriteGuardRequest = LMCSdoRequest.CreateRead(
                    request.Target.SlaveReference,
                    request.Target.ObjectIndex,
                    request.Target.SubIndex,
                    request.Target.ValueType,
                    request.Target.DataLength,
                    request.TimeoutCycles);
                scope.Stage = "SUBMIT_PREWRITE_GUARD_READ";
                scope.PreWriteGuardSubmitAttempted = true;
                try
                {
                    scope.PreWriteGuardTicket =
                        await operations.SubmitAsync(
                            scope.PreWriteGuardRequest,
                            cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        scope.PreWriteGuardRequest,
                        error,
                        SubmissionSlot.PreWriteGuard);
                    throw;
                }

                ValidateTicket(
                    scope.PreWriteGuardTicket,
                    request,
                    scope.PreWriteGuardRequest,
                    LMCOperationKind.SDORead,
                    scope.PreWriteCapabilities,
                    "pre-Write guard");
                if (scope.PreWriteGuardTicket.TicketId
                    == scope.BaselineTicket.TicketId)
                {
                    throw new InvalidOperationException(
                        "The pre-Write guard Read reused the initial baseline ticket identity.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "WAIT_PREWRITE_GUARD_TERMINAL";
                scope.PreWriteGuardStatus =
                    await operations.WaitForTerminalAsync(
                        scope.PreWriteGuardTicket,
                        cancellationToken);
                ValidateReadTerminal(
                    scope.PreWriteGuardTicket,
                    scope.PreWriteGuardStatus,
                    request,
                    scope.PreWriteGuardRequest,
                    scope.BaselineData,
                    "pre-Write guard");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "VERIFY_FINAL_SAFE_AXIS";
                if (!await operations.VerifySafeAxisAsync(
                        request.Target,
                        cancellationToken))
                {
                    throw new InvalidOperationException(
                        "Same-value SDO Write qualification requires a final safe-axis proof after the unchanged pre-Write guard Read.");
                }

                scope.SecondSafetyVerified = true;
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "ARM_DURABLE_JOURNAL";
                scope.JournalArmAttempted = true;
                operations.ArmJournal(
                    scope,
                    scope.WriteRequest,
                    scope.PreWriteCapabilities);
                scope.JournalArmed = true;
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_WRITE";
                scope.WriteSubmitAttempted = true;
                try
                {
                    scope.WriteTicket = await operations.SubmitAsync(
                        scope.WriteRequest,
                        cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        scope.WriteRequest,
                        error,
                        SubmissionSlot.Write);
                    throw;
                }

                scope.Stage = "ADOPT_RETURNED_WRITE_TICKET";
                operations.AdoptWriteTicketBeforeValidation(
                    scope,
                    scope.WriteTicket);

                ValidateTicket(
                    scope.WriteTicket,
                    request,
                    scope.WriteRequest,
                    LMCOperationKind.SDOWrite,
                    scope.PreWriteCapabilities,
                    "Write");
                if (scope.WriteTicket.TicketId
                        == scope.BaselineTicket.TicketId
                    || scope.WriteTicket.TicketId
                        == scope.PreWriteGuardTicket.TicketId)
                {
                    throw new InvalidOperationException(
                        "The SDO Write reused a pre-Write Read ticket identity.");
                }

                scope.Stage = "MARK_WRITE_ACCEPTED";
                operations.MarkWriteAccepted(scope, scope.WriteTicket);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "WAIT_WRITE_TERMINAL";
                scope.WriteStatus = await operations.WaitForTerminalAsync(
                    scope.WriteTicket,
                    cancellationToken);
                ValidateWriteTerminal(
                    scope.WriteTicket,
                    scope.WriteStatus,
                    request,
                    scope.WriteRequest);
                scope.Stage = "MARK_WRITE_TERMINAL_SUCCESS";
                operations.MarkWriteTerminalSuccess(
                    scope,
                    scope.WriteTicket,
                    scope.WriteStatus);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "CREATE_VERIFICATION_CONTEXT";
                scope.VerificationContext =
                    operations.CreateVerificationContext(
                        scope.WriteRequest,
                        scope.WriteTicket,
                        scope.WriteStatus);
                ValidateVerificationContext(scope, request);
                scope.ReadbackRequest = scope.VerificationContext
                    .CreateReadRequest(request.TimeoutCycles);
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "SUBMIT_GUARDED_READBACK";
                scope.ReadbackSubmitAttempted = true;
                try
                {
                    scope.ReadbackTicket =
                        await operations.SubmitReadbackAsync(
                            scope.VerificationContext,
                            scope.ReadbackRequest,
                            cancellationToken);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        scope.ReadbackRequest,
                        error,
                        SubmissionSlot.Readback);
                    throw;
                }

                ValidateTicket(
                    scope.ReadbackTicket,
                    request,
                    scope.ReadbackRequest,
                    LMCOperationKind.SDORead,
                    scope.PreWriteCapabilities,
                    "readback");
                if (scope.ReadbackTicket.TicketId
                        == scope.BaselineTicket.TicketId
                    || scope.ReadbackTicket.TicketId
                        == scope.PreWriteGuardTicket.TicketId
                    || scope.ReadbackTicket.TicketId
                        == scope.WriteTicket.TicketId)
                {
                    throw new InvalidOperationException(
                        "Baseline, pre-Write guard, Write, and guarded readback must use four distinct ticket identities.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "WAIT_READBACK_TERMINAL";
                scope.ReadbackStatus =
                    await operations.WaitForTerminalAsync(
                        scope.ReadbackTicket,
                        cancellationToken);
                ValidateReadTerminal(
                    scope.ReadbackTicket,
                    scope.ReadbackStatus,
                    request,
                    scope.ReadbackRequest,
                    scope.BaselineData,
                    "readback");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "READ_FINAL_CAPABILITIES";
                scope.FinalCapabilities =
                    await operations.ReadCapabilitiesAsync(
                        cancellationToken);
                ValidateFreshStableCapabilities(
                    scope.PreWriteCapabilities,
                    scope.FinalCapabilities,
                    request.Connection,
                    "final");
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "EVALUATE_EXACT_READBACK";
                if (scope.VerificationContext.Evaluate(
                        scope.ReadbackRequest,
                        scope.ReadbackTicket,
                        request.Connection,
                        scope.FinalCapabilities,
                        scope.ReadbackStatus)
                    != LMCSdoWriteVerificationVerdict.Verified)
                {
                    throw new InvalidOperationException(
                        "The guarded same-value SDO Write readback was not exactly verified.");
                }

                scope.ReadbackVerified = true;
                cancellationToken.ThrowIfCancellationRequested();

                scope.Stage = "RESOLVE_DURABLE_JOURNAL";
                operations.ResolveJournalAfterVerified(scope);
                scope.JournalResolved = true;
                scope.Stage = "COMPLETE";
                return new D5SdoWriteSameValueQualificationResult(scope);
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
                        "Same-value SDO Write recovery scope publication failed.",
                        new AggregateException(
                            primaryError,
                            recoveryError));
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw;
            }
        }

        private enum SubmissionSlot
        {
            Baseline,
            PreWriteGuard,
            Write,
            Readback
        }

        private static void ValidatePreflight(
            D5SdoWriteSameValueQualificationRequest request)
        {
            if (!request.Connection.IsConnected)
            {
                throw new InvalidOperationException(
                    "Same-value SDO Write qualification requires a connected owner.");
            }

            ValidateCapabilities(
                request.InitialCapabilities,
                request.Connection,
                "initial");
            if (request.ApprovedTargets.Count != 1
                || request.ApprovedTargets[0] == null
                || !ReferenceEquals(
                    request.ApprovedTargets[0],
                    request.Target))
            {
                throw new NotSupportedException(
                    "Same-value SDO Write qualification requires exactly one selected SDK-approved target.");
            }

            if (request.Target.SlaveReference < 1
                || request.Target.SlaveReference > 4
                || request.Target.DataLength != 4
                || (request.Target.ValueType != LMCSignalValueType.Int32
                    && request.Target.ValueType
                        != LMCSignalValueType.UInt32))
            {
                throw new ArgumentException(
                    "The selected SDO Write target must be Slave 1..4 and exact Int32/UInt32 with four data bytes.",
                    "request");
            }

            if (request.TimeoutCycles < 1
                || request.TimeoutCycles > 60000)
            {
                throw new ArgumentOutOfRangeException(
                    "request",
                    "Same-value SDO Write TimeoutCycles must be 1..60000.");
            }
        }

        private static void ValidateCapabilities(
            LMCDiagnosticCapabilities capabilities,
            LMCConnection connection,
            string stage)
        {
            var required = LMCDiagnosticCapability.SDORead
                | LMCDiagnosticCapability.SDOWrite
                | LMCDiagnosticCapability.SDOReadGeneralInline;
            if (connection == null
                || !connection.IsConnected
                || capabilities == null
                || capabilities.Response == null
                || !capabilities.Response.IsSuccess
                || !capabilities.IsBoundTo(
                    connection.Diagnostics,
                    connection.SessionGeneration)
                || !capabilities.Supports(required)
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || capabilities.BaseCycleTimeUs == 0
                || capabilities.MaxSdoDataBytes != 4
                || capabilities.MaxRequestPayloadBytes < 36
                || capabilities.MaxResponsePayloadBytes < 64)
            {
                throw new InvalidOperationException(
                    "Same-value SDO Write " + stage
                    + " capabilities are not a fresh owner/session-bound full SDO Read/Write contract.");
            }
        }

        private static void ValidateFreshStableCapabilities(
            LMCDiagnosticCapabilities expected,
            LMCDiagnosticCapabilities actual,
            LMCConnection connection,
            string stage)
        {
            ValidateCapabilities(actual, connection, stage);
            if (actual.ObservationSequence <= expected.ObservationSequence
                || !HasStableFullContract(expected, actual))
            {
                throw new InvalidOperationException(
                    "Same-value SDO Write " + stage
                    + " capabilities are not a fresh, increasing observation of the same full D5 contract.");
            }
        }

        private static bool HasStableFullContract(
            LMCDiagnosticCapabilities expected,
            LMCDiagnosticCapabilities actual)
        {
            return expected != null
                && actual != null
                && actual.DiagnosticsBuild == expected.DiagnosticsBuild
                && actual.CapabilityBits == expected.CapabilityBits
                && actual.MapRevision == expected.MapRevision
                && actual.CatalogEntryCount == expected.CatalogEntryCount
                && actual.MaxBulkSignals == expected.MaxBulkSignals
                && actual.MaxRecorderChannels
                    == expected.MaxRecorderChannels
                && actual.RecorderBufferCount
                    == expected.RecorderBufferCount
                && actual.MaxRecorderSamples == expected.MaxRecorderSamples
                && actual.BaseCycleTimeUs == expected.BaseCycleTimeUs
                && actual.MaxRequestPayloadBytes
                    == expected.MaxRequestPayloadBytes
                && actual.MaxResponsePayloadBytes
                    == expected.MaxResponsePayloadBytes
                && actual.MaxChunkDataBytes == expected.MaxChunkDataBytes
                && actual.CatalogEntryStride == expected.CatalogEntryStride
                && actual.SignalValueEntryStride
                    == expected.SignalValueEntryStride
                && actual.RecorderBytesPerBank
                    == expected.RecorderBytesPerBank
                && actual.MaxSdoDataBytes == expected.MaxSdoDataBytes
                && actual.DiagnosticsBootId == expected.DiagnosticsBootId;
        }

        private static LMCSdoRequest CreateSameValueWriteRequest(
            D5SdoWriteSameValueQualificationRequest request,
            byte[] baselineData)
        {
            if (baselineData == null || baselineData.Length != 4)
            {
                throw new InvalidOperationException(
                    "The same-value Write baseline must contain exactly four bytes.");
            }

            var raw = (uint)baselineData[0]
                | ((uint)baselineData[1] << 8)
                | ((uint)baselineData[2] << 16)
                | ((uint)baselineData[3] << 24);
            long value = request.Target.ValueType
                    == LMCSignalValueType.Int32
                ? (long)unchecked((int)raw)
                : (long)raw;
            var writeRequest = request.Target.CreateRequest(
                value,
                request.TimeoutCycles);
            if (!ByteArraysEqual(
                    writeRequest.WriteData,
                    baselineData))
            {
                throw new InvalidOperationException(
                    "The same-value SDO Write request is not byte-identical to the exact baseline read.");
            }

            return writeRequest;
        }

        private static void ValidateTicket(
            LMCOperationTicket ticket,
            D5SdoWriteSameValueQualificationRequest request,
            LMCSdoRequest submittedRequest,
            LMCOperationKind expectedKind,
            LMCDiagnosticCapabilities capabilities,
            string stage)
        {
            if (ticket == null
                || !ticket.BelongsToCurrentSession(request.Connection)
                || ticket.OperationKind != expectedKind
                || ticket.DiagnosticsBootId != capabilities.DiagnosticsBootId
                || ticket.SubmissionMapRevision != capabilities.MapRevision
                || !LMCSdoWriteVerificationContext.RequestsEqual(
                    submittedRequest,
                    ticket.SubmittedSdoRequest)
                || (expectedKind == LMCOperationKind.SDORead
                    && (ticket.RequestedResultLength
                            != submittedRequest.DataLength
                        || ticket.ResultValueType
                            != submittedRequest.ValueType))
                || (expectedKind == LMCOperationKind.SDOWrite
                    && (ticket.RequestedResultLength != 0
                        || ticket.ResultValueType
                            != LMCSignalValueType.Invalid)))
            {
                throw new InvalidOperationException(
                    "The " + stage
                    + " ticket does not match the exact owner/session, capability identity, operation, and submitted request.");
            }
        }

        private static void ValidateReadTerminal(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoWriteSameValueQualificationRequest request,
            LMCSdoRequest readRequest,
            byte[] expectedData,
            string stage)
        {
            if (!HasExactStatusIdentity(ticket, status, request.Connection)
                || status.State != LMCOperationState.Completed
                || status.Outcome != LMCOperationOutcome.Success
                || !status.IsSuccessful
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultLength != readRequest.DataLength
                || status.ResultValueType != readRequest.ValueType
                || status.ResultData.Length != readRequest.DataLength
                || (expectedData != null
                    && !ByteArraysEqual(status.ResultData, expectedData)))
            {
                throw new InvalidOperationException(
                    "The " + stage
                    + " SDO Read terminal is not exact Completed/Success with the required four-byte value.");
            }
        }

        private static void ValidateWriteTerminal(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            D5SdoWriteSameValueQualificationRequest request,
            LMCSdoRequest writeRequest)
        {
            if (!HasExactStatusIdentity(ticket, status, request.Connection)
                || status.State != LMCOperationState.Completed
                || status.Outcome != LMCOperationOutcome.Success
                || !status.IsSuccessful
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultLength != 0
                || status.ResultValueType != LMCSignalValueType.Invalid
                || status.ResultData.Length != 0
                || !LMCSdoWriteVerificationContext.RequestsEqual(
                    writeRequest,
                    ticket.SubmittedSdoRequest))
            {
                throw new InvalidOperationException(
                    "The SDO Write terminal is not the exact owner/session-bound Completed/Success status with no result data.");
            }
        }

        private static bool HasExactStatusIdentity(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            LMCConnection connection)
        {
            return ticket != null
                && status != null
                && status.Response != null
                && status.Response.IsSuccess
                && status.Response.ErrorId == 0
                && status.Response.Detail == LMCDiagnosticsDetailCode.None
                && status.IsBoundTo(
                    connection.Diagnostics,
                    connection.SessionGeneration)
                && status.TicketId == ticket.TicketId
                && status.OperationKind == ticket.OperationKind
                && status.DiagnosticsBootId == ticket.DiagnosticsBootId
                && status.SubmitCycle == ticket.QueuedCycle;
        }

        private static void ValidateVerificationContext(
            D5SdoWriteSameValueRecoveryScope scope,
            D5SdoWriteSameValueQualificationRequest request)
        {
            var context = scope.VerificationContext;
            if (context == null
                || !context.MatchesOwnerCurrentSession(request.Connection)
                || context.WriteTicket != scope.WriteTicket
                || context.SlaveReference != request.Target.SlaveReference
                || context.ObjectIndex != request.Target.ObjectIndex
                || context.SubIndex != request.Target.SubIndex
                || context.ValueType != request.Target.ValueType
                || context.DataLength != request.Target.DataLength
                || !ByteArraysEqual(
                    context.ExpectedWriteData,
                    scope.BaselineData))
            {
                throw new InvalidOperationException(
                    "The SDK SDO Write verification context does not preserve the exact owner, target, ticket, and baseline bytes.");
            }
        }

        private static void CaptureSubmissionFailure(
            D5SdoWriteSameValueRecoveryScope scope,
            LMCSdoRequest expectedRequest,
            Exception error,
            SubmissionSlot slot)
        {
            LMCSdoSubmissionFailureContext context;
            if (!LMCSdoSubmissionFailureContext.TryGet(
                    error,
                    out context)
                || !LMCSdoWriteVerificationContext.RequestsEqual(
                    expectedRequest,
                    context.Request))
            {
                return;
            }

            if (context.SubmissionOutcome
                == LMCSdoSubmissionOutcome.OutcomeUncertain)
            {
                SetSubmissionUncertain(scope, slot);
            }
            else if (context.SubmissionOutcome
                    == LMCSdoSubmissionOutcome.Accepted
                && context.Ticket != null)
            {
                SetAcceptedTicket(scope, slot, context.Ticket);
            }
        }

        private static void SetSubmissionUncertain(
            D5SdoWriteSameValueRecoveryScope scope,
            SubmissionSlot slot)
        {
            if (slot == SubmissionSlot.Baseline)
            {
                scope.BaselineSubmissionOutcomeUncertain = true;
            }
            else if (slot == SubmissionSlot.PreWriteGuard)
            {
                scope.PreWriteGuardSubmissionOutcomeUncertain = true;
            }
            else if (slot == SubmissionSlot.Write)
            {
                scope.WriteSubmissionOutcomeUncertain = true;
            }
            else
            {
                scope.ReadbackSubmissionOutcomeUncertain = true;
            }
        }

        private static void SetAcceptedTicket(
            D5SdoWriteSameValueRecoveryScope scope,
            SubmissionSlot slot,
            LMCOperationTicket ticket)
        {
            if (slot == SubmissionSlot.Baseline)
            {
                scope.BaselineTicket = ticket;
            }
            else if (slot == SubmissionSlot.PreWriteGuard)
            {
                scope.PreWriteGuardTicket = ticket;
            }
            else if (slot == SubmissionSlot.Write)
            {
                scope.WriteTicket = ticket;
            }
            else
            {
                scope.ReadbackTicket = ticket;
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
    }
}

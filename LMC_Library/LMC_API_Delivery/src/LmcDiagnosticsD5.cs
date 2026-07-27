using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        private readonly object operationBootIdSync = new object();
        private bool hasOperationBootId;
        private long operationBootIdSessionGeneration;
        private uint operationBootId;
        private const int InlineSdoTerminalPollAllowance = 32;
        private const uint InlineSdoPollIntervalMicroseconds = 1000;
        private readonly SemaphoreSlim inlineSdoReadGate =
            new SemaphoreSlim(1, 1);

        public LMCOperationTicket SubmitPIWrite(
            LMCPIWriteRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = connection.SessionGeneration;
            return SubmitPIWriteCore(request, sessionGeneration);
        }

        private LMCOperationTicket SubmitPIWriteCore(
            LMCPIWriteRequest request,
            long sessionGeneration)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidatePIWriteCapabilities(
                capabilities,
                sessionGeneration,
                request);
            RememberOperationBootId(
                sessionGeneration,
                capabilities.DiagnosticsBootId);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.SubmitPIWrite(
                    requestId,
                    request,
                    capabilities.DiagnosticsBootId),
                sessionGeneration);

            LMCOperationSubmission submission;
            try
            {
                submission = LMC_DiagnosticsParser.ParseSubmitOperation(
                    raw,
                    requestId,
                    LMCOperationKind.PIWrite,
                    capabilities.DiagnosticsBootId,
                    "SubmitPIWrite");
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return CreatePIWriteTicket(
                submission,
                capabilities.MapRevision,
                sessionGeneration);
        }

        public async Task<LMCOperationTicket> SubmitPIWriteAsync(
            LMCPIWriteRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            return await RunStateMutatingAsync(
                () => SubmitPIWriteCore(request, sessionGeneration),
                cancellationToken).ConfigureAwait(false);
        }

        public LMCOperationTicket SubmitSdo(LMCSdoRequest request)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                ValidateSdoSubmitPolicy(request);
                attemptTracker.BeginSessionPreflight();

                var sessionGeneration = connection.SessionGeneration;
                connection.EnsureSessionGeneration(sessionGeneration);
                return SubmitSdoTrackedCore(
                    request,
                    sessionGeneration,
                    attemptTracker);
            }
            catch (Exception exception)
            {
                LMCSdoSubmissionFailureContext.Attach(
                    exception,
                    attemptTracker.CreateFailureContext());
                throw;
            }
        }

        private LMCOperationTicket SubmitSdoTrackedCore(
            LMCSdoRequest request,
            long sessionGeneration,
            LMCSdoSubmissionAttemptTracker attemptTracker)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            attemptTracker.BeginCapabilityPreflight();
            var capabilities = GetCapabilities();
            attemptTracker.RecordCapabilityIdentity(
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision);
            return SubmitSdoCore(
                request,
                sessionGeneration,
                capabilities,
                attemptTracker);
        }

        private LMCOperationTicket SubmitSdoCore(
            LMCSdoRequest request,
            long sessionGeneration,
            LMCDiagnosticCapabilities capabilities,
            ILMCSdoSubmissionAttemptTracker attemptTracker = null)
        {
            ValidateSdoCapabilities(
                capabilities,
                sessionGeneration,
                request);
            RememberOperationBootId(
                sessionGeneration,
                capabilities.DiagnosticsBootId);
            if (attemptTracker != null)
            {
                attemptTracker.BeginSubmission();
            }

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.SubmitSdo(
                    requestId,
                    capabilities.MapRevision,
                    request,
                    capabilities.DiagnosticsBootId),
                sessionGeneration,
                attemptTracker == null
                    ? null
                    : new Action(
                        attemptTracker.MarkSubmissionOutcomeUncertain));

            LMCOperationSubmission submission;
            try
            {
                submission = LMC_DiagnosticsParser.ParseSubmitOperation(
                    raw,
                    requestId,
                    request.IsWrite
                        ? LMCOperationKind.SDOWrite
                        : LMCOperationKind.SDORead,
                    capabilities.DiagnosticsBootId,
                    "SubmitSDO");
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                if (attemptTracker != null)
                {
                    attemptTracker.MarkSubmissionRejected();
                }

                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }

            var ticket = CreateSdoTicket(
                submission,
                sessionGeneration,
                request,
                capabilities.MapRevision,
                capabilities.MaxChunkDataBytes);
            if (attemptTracker != null)
            {
                attemptTracker.MarkSubmissionAccepted(ticket);
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return ticket;
        }

        public async Task<LMCOperationTicket> SubmitSdoAsync(
            LMCSdoRequest request,
            CancellationToken cancellationToken)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                ValidateSdoSubmitPolicy(request);
                attemptTracker.BeginSessionPreflight();

                var sessionGeneration = connection.SessionGeneration;
                connection.EnsureSessionGeneration(sessionGeneration);
                return await RunStateMutatingAsync(
                    () => SubmitSdoTrackedCore(
                        request,
                        sessionGeneration,
                        attemptTracker),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LMCSdoSubmissionFailureContext.Attach(
                    exception,
                    attemptTracker.CreateFailureContext());
                throw;
            }
        }

        public LMCOperationStatus GetOperationStatus(
            LMCOperationTicket ticket)
        {
            var sessionGeneration = ValidateOperationTicket(ticket);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.GetOperationStatus(
                    requestId,
                    ticket.TicketId,
                    ticket.DiagnosticsBootId),
                sessionGeneration);

            try
            {
                var status = LMC_DiagnosticsParser.ParseOperationStatus(
                    raw,
                    requestId,
                    ticket);
                connection.EnsureSessionGeneration(sessionGeneration);
                return status;
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }
        }

        public async Task<LMCOperationStatus> GetOperationStatusAsync(
            LMCOperationTicket ticket,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateOperationTicket(ticket);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.GetOperationStatus(
                    requestId,
                    ticket.TicketId,
                    ticket.DiagnosticsBootId),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);

            try
            {
                var status = LMC_DiagnosticsParser.ParseOperationStatus(
                    raw,
                    requestId,
                    ticket);
                connection.EnsureSessionGeneration(sessionGeneration);
                return status;
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }
        }

        public void CancelOperation(LMCOperationTicket ticket)
        {
            var sessionGeneration = ValidateOperationTicket(ticket);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.CancelOperation(
                    requestId,
                    ticket.TicketId,
                    ticket.DiagnosticsBootId),
                sessionGeneration);

            try
            {
                LMC_DiagnosticsParser.ParseCancelOperation(
                    raw,
                    requestId,
                    ticket);
                connection.EnsureSessionGeneration(sessionGeneration);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }
        }

        public async Task CancelOperationAsync(
            LMCOperationTicket ticket,
            CancellationToken cancellationToken)
        {
            await RunStateMutatingAsync(
                () => CancelOperation(ticket),
                cancellationToken).ConfigureAwait(false);
        }

        public LMCSdoResultChunk ReadSdoResultChunk(
            LMCSdoResultChunkRequest request)
        {
            ValidateSdoResultChunkRequest(request);
            var sessionGeneration = ValidateOperationTicket(request.Ticket);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadSdoResultChunk(requestId, request),
                sessionGeneration);

            try
            {
                var chunk = LMC_DiagnosticsParser.ParseSdoResultChunk(
                    raw,
                    requestId,
                    request);
                connection.EnsureSessionGeneration(sessionGeneration);
                return chunk;
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }
        }

        public async Task<LMCSdoResultChunk> ReadSdoResultChunkAsync(
            LMCSdoResultChunkRequest request,
            CancellationToken cancellationToken)
        {
            ValidateSdoResultChunkRequest(request);
            var sessionGeneration = ValidateOperationTicket(request.Ticket);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadSdoResultChunk(requestId, request),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);

            try
            {
                var chunk = LMC_DiagnosticsParser.ParseSdoResultChunk(
                    raw,
                    requestId,
                    request);
                connection.EnsureSessionGeneration(sessionGeneration);
                return chunk;
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }
        }

        internal LMCInlineSdoReadCompletion ReadInlineSdoToTerminal(
            LMCSdoRequest request,
            long expectedSessionGeneration,
            LMCDriveReadAttemptTracker attemptTracker)
        {
            if (attemptTracker == null)
            {
                throw new ArgumentNullException("attemptTracker");
            }

            attemptTracker.BeginSdoRead(request);
            ValidateInlineSdoReadRequest(request);
            connection.EnsureSessionGeneration(expectedSessionGeneration);

            inlineSdoReadGate.Wait();
            LMCOperationTicket ticket = null;
            try
            {
                connection.EnsureSessionGeneration(expectedSessionGeneration);
                var submission = SubmitInlineSdoRead(
                    request,
                    expectedSessionGeneration,
                    attemptTracker);
                ticket = submission.Ticket;
                var pollLimit = GetInlineSdoTerminalPollLimit(
                    request.TimeoutCycles);
                var pollDelayMilliseconds =
                    GetInlineSdoPollDelayMilliseconds(
                        submission.BaseCycleTimeUs);

                for (var poll = 0; poll < pollLimit; poll++)
                {
                    attemptTracker.BeginStatusPolling();
                    var status = GetOperationStatus(ticket);
                    attemptTracker.RecordOperationStatus(status);
                    if (status.IsTerminal)
                    {
                        return RequireSuccessfulInlineSdoRead(ticket, status);
                    }

                    if (poll + 1 < pollLimit)
                    {
                        Thread.Sleep(pollDelayMilliseconds);
                    }
                }

                throw CreateInlineSdoPollingTimeout(ticket, pollLimit);
            }
            catch (LMCSdoReadCommandException)
            {
                throw;
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                throw CreateInlineSdoCommandException(
                    ticket == null
                        ? LMCSdoReadCommandStage.Submission
                        : LMCSdoReadCommandStage.StatusPolling,
                    ticket,
                    exception);
            }
            finally
            {
                inlineSdoReadGate.Release();
            }
        }

        internal async Task<LMCInlineSdoReadCompletion>
            ReadInlineSdoToTerminalAsync(
                LMCSdoRequest request,
                long expectedSessionGeneration,
                LMCDriveReadAttemptTracker attemptTracker,
                CancellationToken cancellationToken)
        {
            if (attemptTracker == null)
            {
                throw new ArgumentNullException("attemptTracker");
            }

            attemptTracker.BeginSdoRead(request);
            ValidateInlineSdoReadRequest(request);
            connection.EnsureSessionGeneration(expectedSessionGeneration);

            await inlineSdoReadGate.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            LMCOperationTicket ticket = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                connection.EnsureSessionGeneration(expectedSessionGeneration);
                var submission = await RunStateMutatingAsync(
                    () => SubmitInlineSdoRead(
                        request,
                        expectedSessionGeneration,
                        attemptTracker),
                    cancellationToken).ConfigureAwait(false);
                ticket = submission.Ticket;
                var pollLimit = GetInlineSdoTerminalPollLimit(
                    request.TimeoutCycles);
                var pollDelayMilliseconds =
                    GetInlineSdoPollDelayMilliseconds(
                        submission.BaseCycleTimeUs);

                for (var poll = 0; poll < pollLimit; poll++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    attemptTracker.BeginStatusPolling();
                    var status = await GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None).ConfigureAwait(false);
                    attemptTracker.RecordOperationStatus(status);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (status.IsTerminal)
                    {
                        return RequireSuccessfulInlineSdoRead(ticket, status);
                    }

                    if (poll + 1 < pollLimit)
                    {
                        await Task.Delay(
                            pollDelayMilliseconds,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                throw CreateInlineSdoPollingTimeout(ticket, pollLimit);
            }
            catch (LMCSdoReadCommandException)
            {
                throw;
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                throw CreateInlineSdoCommandException(
                    ticket == null
                        ? LMCSdoReadCommandStage.Submission
                        : LMCSdoReadCommandStage.StatusPolling,
                    ticket,
                    exception);
            }
            catch (OperationCanceledException exception)
            {
                if (ticket == null)
                {
                    throw;
                }

                throw new LMCSdoReadWaitCanceledException(
                    ticket,
                    exception,
                    cancellationToken);
            }
            finally
            {
                inlineSdoReadGate.Release();
            }
        }

        private static LMCSdoReadCommandException
            CreateInlineSdoCommandException(
                LMCSdoReadCommandStage stage,
                LMCOperationTicket ticket,
                LMCDiagnosticsCommandException exception)
        {
            return new LMCSdoReadCommandException(
                stage,
                ticket,
                exception);
        }

        internal static int GetInlineSdoPollDelayMilliseconds(
            uint baseCycleTimeUs)
        {
            if (baseCycleTimeUs == 0)
            {
                throw new InvalidDataException(
                    "Typed SDO polling requires a non-zero BaseCycleTimeUs capability.");
            }

            return checked((int)(
                ((ulong)baseCycleTimeUs
                    + InlineSdoPollIntervalMicroseconds
                    - 1)
                / InlineSdoPollIntervalMicroseconds));
        }

        private LMCInlineSdoReadSubmission SubmitInlineSdoRead(
            LMCSdoRequest request,
            long expectedSessionGeneration,
            LMCDriveReadAttemptTracker attemptTracker)
        {
            connection.EnsureSessionGeneration(expectedSessionGeneration);
            LMCDiagnosticCapabilities capabilities;
            try
            {
                capabilities = GetCapabilities();
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                throw CreateInlineSdoCommandException(
                    LMCSdoReadCommandStage.CapabilityPreflight,
                    null,
                    exception);
            }

            attemptTracker.RecordCapabilityIdentity(
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision);
            GetInlineSdoPollDelayMilliseconds(
                capabilities.BaseCycleTimeUs);
            LMCOperationTicket ticket;
            try
            {
                ticket = SubmitSdoCore(
                    request,
                    expectedSessionGeneration,
                    capabilities,
                    attemptTracker);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                throw CreateInlineSdoCommandException(
                    LMCSdoReadCommandStage.Submission,
                    null,
                    exception);
            }

            return new LMCInlineSdoReadSubmission(
                ticket,
                capabilities.BaseCycleTimeUs);
        }

        internal static int GetInlineSdoTerminalPollLimit(
            uint timeoutCycles)
        {
            if (timeoutCycles < 1
                || timeoutCycles
                    > LMCDiagnosticsSdoPolicy.MaximumReadTimeoutCycles)
            {
                throw new ArgumentOutOfRangeException("timeoutCycles");
            }

            return checked(
                (int)timeoutCycles + InlineSdoTerminalPollAllowance);
        }

        private static void ValidateInlineSdoReadRequest(
            LMCSdoRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            ValidateSdoSubmitPolicy(request);
            if (request.IsWrite
                || request.DataLength
                    > LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes)
            {
                throw new NotSupportedException(
                    "The typed SDO helper supports inline SDO Read results only.");
            }
        }

        private static LMCInlineSdoReadCompletion
            RequireSuccessfulInlineSdoRead(
                LMCOperationTicket ticket,
                LMCOperationStatus status)
        {
            if (!status.IsSuccessful)
            {
                throw new LMCSdoReadOperationException(ticket, status);
            }

            return new LMCInlineSdoReadCompletion(ticket, status);
        }

        private static LMCSdoReadPollingTimeoutException
            CreateInlineSdoPollingTimeout(
                LMCOperationTicket ticket,
                int pollLimit)
        {
            return new LMCSdoReadPollingTimeoutException(
                ticket,
                pollLimit);
        }

        private static void ValidateSdoWritePolicy(LMCSdoRequest request)
        {
            if (!request.IsWrite)
            {
                return;
            }

            if (LMCSdoRequest.IsPermanentlyUnsafeObject(request.ObjectIndex))
            {
                throw new InvalidOperationException(
                    "Direct SDO write is permanently blocked for DS402 control and target objects.");
            }
        }

        private static void ValidateSdoSubmitPolicy(LMCSdoRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            ValidateSdoWritePolicy(request);
            if (request.IsWrite)
            {
                LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(request);
                return;
            }

            LMCDiagnosticsSdoPolicy.RequireReadAllowed(request);
        }

        private void ValidatePIWriteCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            LMCPIWriteRequest request)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (capabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration
                || capabilities.DiagnosticsBootId == 0)
            {
                throw new InvalidDataException(
                    "PI Write capabilities contain a stale session or zero DiagnosticsBootId.");
            }

            if (!capabilities.Supports(LMCDiagnosticCapability.PIWrite))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise PI Write diagnostics.");
            }

            if (!capabilities.Supports(LMCDiagnosticCapability.SignalCatalog)
                || capabilities.MapRevision == 0
                || capabilities.MapRevision != request.MapRevision)
            {
                throw new InvalidOperationException(
                    "PI Write requires the exact active Signal Catalog revision.");
            }

            if (LMC_DiagnosticsFrame.SubmitPiWriteRequestPayloadLength
                    > capabilities.MaxRequestPayloadBytes
                || LMC_DiagnosticsParser.SubmitOperationPayloadLength
                    > capabilities.MaxResponsePayloadBytes)
            {
                throw new InvalidDataException(
                    "PI Write capability payload limits are inconsistent.");
            }

            LMCDiagnosticsWritePolicy.RequirePIWriteAllowed(request);

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateSdoCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            LMCSdoRequest request)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (capabilities.ConnectionSessionGeneration
                != expectedSessionGeneration)
            {
                throw new InvalidOperationException(
                    "Diagnostics capabilities belong to a stale connection session.");
            }

            var requiredCapability = request.IsWrite
                ? LMCDiagnosticCapability.SDOWrite
                : LMCDiagnosticCapability.SDORead;
            if (!capabilities.Supports(requiredCapability))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise the requested SDO operation.");
            }

            if (!request.IsWrite
                && !LMCDiagnosticsSdoPolicy.IsLegacyFirstSliceRead(request)
                && !capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise general inline SDO Read support.");
            }

            if (capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || capabilities.MaxSdoDataBytes == 0)
            {
                throw new InvalidDataException(
                    "D5 SDO capability identity or MaxSdoDataBytes is invalid.");
            }

            var usesExtendedResultChunks = !request.IsWrite
                && request.DataLength
                    > LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes;
            if (usesExtendedResultChunks
                && !capabilities.Supports(
                    LMCDiagnosticCapability.ExtendedSdoResultChunk))
            {
                throw new NotSupportedException(
                    "The requested SDO Read length requires ExtendedSdoResultChunk capability.");
            }

            var requestPayloadLength = checked(
                LMC_DiagnosticsFrame.SubmitSdoRequestHeaderPayloadLength
                + (request.IsWrite ? request.DataLength : 0));
            if (request.DataLength > capabilities.MaxSdoDataBytes
                || requestPayloadLength > capabilities.MaxRequestPayloadBytes
                || LMC_DiagnosticsParser.OperationStatusPayloadLength
                    > capabilities.MaxResponsePayloadBytes)
            {
                throw new InvalidDataException(
                    "D5 SDO capability payload limits cannot carry this operation.");
            }

            if (usesExtendedResultChunks
                && (capabilities.MaxChunkDataBytes == 0
                    || LMC_DiagnosticsParser.SdoResultChunkResponseHeaderPayloadLength
                        + capabilities.MaxChunkDataBytes
                        > capabilities.MaxResponsePayloadBytes))
            {
                throw new InvalidDataException(
                    "Extended SDO result chunk limits are inconsistent.");
            }

            LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(request);

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private LMCOperationTicket CreateSdoTicket(
            LMCOperationSubmission submission,
            long sessionGeneration,
            LMCSdoRequest request,
            uint submissionMapRevision,
            ushort maxChunkDataBytes)
        {
            return new LMCOperationTicket(
                submission.TicketId,
                submission.OperationKind,
                submission.QueuedCycle,
                submission.DiagnosticsBootId,
                submissionMapRevision,
                sessionGeneration,
                this,
                !request.IsWrite,
                request.IsWrite ? (ushort)0 : request.DataLength,
                request.IsWrite
                    ? LMCSignalValueType.Invalid
                    : request.ValueType,
                !request.IsWrite
                    && request.DataLength
                        > LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes,
                submission.OperationKind == LMCOperationKind.SDORead
                    && request.DataLength
                        > LMC_DiagnosticsFrame.MaxD5InlineSdoDataBytes
                    ? maxChunkDataBytes
                    : (ushort)0);
        }

        private LMCOperationTicket CreatePIWriteTicket(
            LMCOperationSubmission submission,
            uint submissionMapRevision,
            long sessionGeneration)
        {
            return new LMCOperationTicket(
                submission.TicketId,
                submission.OperationKind,
                submission.QueuedCycle,
                submission.DiagnosticsBootId,
                submissionMapRevision,
                sessionGeneration,
                this,
                false,
                0,
                LMCSignalValueType.Invalid);
        }

        private static void ValidateSdoResultChunkRequest(
            LMCSdoResultChunkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.Ticket.UsesExtendedResultChunks)
            {
                throw new InvalidOperationException(
                    "The SDO ticket does not use extended result chunks.");
            }
        }

        private long ValidateOperationTicket(LMCOperationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            if (!ReferenceEquals(ticket.Owner, this))
            {
                throw new InvalidOperationException(
                    "The operation ticket belongs to a different LMCConnection.");
            }

            var sessionGeneration = connection.SessionGeneration;
            if (ticket.ConnectionSessionGeneration != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The operation ticket belongs to a stale connection session.");
            }

            lock (operationBootIdSync)
            {
                if (!hasOperationBootId
                    || operationBootIdSessionGeneration != sessionGeneration
                    || operationBootId != ticket.DiagnosticsBootId)
                {
                    throw new InvalidOperationException(
                        "The operation ticket DiagnosticsBootId is stale.");
                }
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private void RememberOperationBootId(
            long sessionGeneration,
            uint diagnosticsBootId)
        {
            lock (operationBootIdSync)
            {
                hasOperationBootId = true;
                operationBootIdSessionGeneration = sessionGeneration;
                operationBootId = diagnosticsBootId;
            }
        }

        private void HandleD5DomainError(
            long sessionGeneration,
            LMCDiagnosticsCommandException exception)
        {
            InvalidateCatalogRevisionOnMismatch(
                sessionGeneration,
                exception);

            if (exception != null
                && exception.Response != null
                && exception.Response.Detail
                    == LMCDiagnosticsDetailCode.BootIdMismatch)
            {
                lock (operationBootIdSync)
                {
                    if (operationBootIdSessionGeneration
                        == sessionGeneration)
                    {
                        hasOperationBootId = false;
                        operationBootId = 0;
                    }
                }
            }
        }
    }
}

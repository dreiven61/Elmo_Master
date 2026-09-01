using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns the immutable compile-time SDO Write targets approved by
        /// this SDK build. An empty list means every SDO Write is blocked
        /// before transmission.
        /// </summary>
        public IReadOnlyList<LMCSdoWriteTarget> GetApprovedSdoWriteTargets()
        {
            return LMCDiagnosticsWritePolicy.GetApprovedSdoWriteTargets();
        }

        /// <summary>
        /// Evaluates the SDK SDO Write allowlist and one cached diagnostics
        /// capability observation without refreshing capabilities or sending
        /// any RPC request. This is a submission-policy check only; the caller
        /// must separately prove axis, drive-program, operator, journal, and
        /// physical safety conditions.
        /// </summary>
        public LMCSdoWritePolicyEvaluation EvaluateSdoWritePolicy(
            LMCDiagnosticCapabilities cachedCapabilities)
        {
            bool isConnected;
            var sessionGeneration = connection.CaptureSessionGeneration(
                out isConnected);
            return LMCDiagnosticsWritePolicy.EvaluateSdoWritePolicy(
                this,
                sessionGeneration,
                isConnected,
                cachedCapabilities,
                LMCDiagnosticsWritePolicy.GetApprovedSdoWriteTargets());
        }

        /// <summary>
        /// Submits one active-policy 1, 2, or 4-byte SDO Read and waits for
        /// its ticket to reach a bounded successful terminal state. The
        /// returned result preserves the accepted ticket and exact terminal
        /// status. This helper never accepts Write or extended-result reads.
        /// </summary>
        public LMCSdoReadResult ReadSdoInline(LMCSdoRequest request)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                ValidateInlineSdoReadRequest(request);
                attemptTracker.BeginSessionPreflight();

                var sessionGeneration = connection.SessionGeneration;
                connection.EnsureSessionGeneration(sessionGeneration);
                attemptTracker.BeginCapabilityPreflight();
                return ReadInlineSdoToTerminalCore(
                    request,
                    sessionGeneration,
                    attemptTracker,
                    null,
                    null);
            }
            catch (Exception exception)
            {
                LMCSdoSubmissionFailureContext.Attach(
                    exception,
                    attemptTracker.CreateFailureContext());
                throw;
            }
        }

        /// <summary>
        /// Asynchronously submits one active-policy 1, 2, or 4-byte SDO Read
        /// and waits for its ticket. An observed terminal status wins a
        /// concurrent cancellation request. Cancelling after ticket acceptance
        /// while the last observed status is non-terminal stops only the PC
        /// wait and throws LMCSdoReadWaitCanceledException with the accepted
        /// ticket and last status; it does not cancel or replay the PLC ticket.
        /// </summary>
        public async Task<LMCSdoReadResult> ReadSdoInlineAsync(
            LMCSdoRequest request,
            CancellationToken cancellationToken)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                ValidateInlineSdoReadRequest(request);
                attemptTracker.BeginSessionPreflight();
                cancellationToken.ThrowIfCancellationRequested();

                var sessionGeneration = connection.SessionGeneration;
                connection.EnsureSessionGeneration(sessionGeneration);
                attemptTracker.BeginCapabilityPreflight();
                return await ReadInlineSdoToTerminalCoreAsync(
                    request,
                    sessionGeneration,
                    attemptTracker,
                    null,
                    null,
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
            RequireCurrentSignalCatalog(request.Catalog);
            if (request.Catalog.ConnectionSessionGeneration
                != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The PI Write Catalog does not belong to the captured RPC session.");
            }

            var capabilities = GetCapabilities(sessionGeneration);
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

        public LMCOperationTicket SubmitSdo(
            LMCSdoRequest request,
            LMCOperationTicket requiredIdentityTicket)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (requiredIdentityTicket == null)
                {
                    throw new ArgumentNullException(
                        "requiredIdentityTicket");
                }

                ValidateSdoSubmitPolicy(request);
                attemptTracker.BeginSessionPreflight();

                var sessionGeneration = connection.SessionGeneration;
                ValidateRequiredSdoSubmissionIdentity(
                    requiredIdentityTicket,
                    request,
                    sessionGeneration,
                    null);
                return SubmitSdoTrackedCore(
                    request,
                    sessionGeneration,
                    attemptTracker,
                    requiredIdentityTicket);
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
            LMCSdoSubmissionAttemptTracker attemptTracker,
            LMCOperationTicket requiredIdentityTicket = null)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            if (requiredIdentityTicket != null)
            {
                ValidateRequiredSdoSubmissionIdentity(
                    requiredIdentityTicket,
                    request,
                    sessionGeneration,
                    null);
            }

            attemptTracker.BeginCapabilityPreflight();
            var capabilities = GetCapabilities();
            attemptTracker.RecordCapabilityIdentity(
                capabilities.DiagnosticsBootId,
                capabilities.MapRevision);
            if (requiredIdentityTicket != null)
            {
                ValidateRequiredSdoSubmissionIdentity(
                    requiredIdentityTicket,
                    request,
                    sessionGeneration,
                    capabilities);
            }

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
            Action beforeWrite = null;
            if (attemptTracker != null)
            {
                beforeWrite = () =>
                {
                    attemptTracker.MarkSubmissionOutcomeUncertain();
                };
            }

            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.SubmitSdo(
                    requestId,
                    capabilities.MapRevision,
                    request,
                    capabilities.DiagnosticsBootId),
                sessionGeneration,
                beforeWrite);

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

            LMCOperationTicket publishedTicket = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.SubmitSdo,
                () => publishedTicket = ticket);
            return publishedTicket;
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

        internal async Task<LMCOperationTicket>
            SubmitSdoWriteIdentityPinnedAsync(
                LMCSdoRequest request,
                LMCDiagnosticCapabilities requiredCapabilities,
                LMCSdoWriteTarget requiredTarget,
                CancellationToken cancellationToken)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (requiredCapabilities == null)
                {
                    throw new ArgumentNullException(
                        "requiredCapabilities");
                }

                if (requiredTarget == null)
                {
                    throw new ArgumentNullException("requiredTarget");
                }

                if (!request.IsWrite)
                {
                    throw new InvalidOperationException(
                        "Identity-pinned SDO submission accepts Write requests only.");
                }

                ValidateSdoSubmitPolicy(request);
                attemptTracker.BeginSessionPreflight();

                var sessionGeneration = connection.SessionGeneration;
                ValidateRequiredSdoWriteSubmissionIdentity(
                    request,
                    requiredCapabilities,
                    requiredTarget,
                    sessionGeneration,
                    null);
                return await RunStateMutatingAsync(
                    () => SubmitSdoWriteIdentityPinnedCore(
                        request,
                        requiredCapabilities,
                        requiredTarget,
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

        internal async Task<LMCOperationTicket>
            SubmitSdoWriteIdentityPinnedAsync(
                LMCSdoRequest request,
                LMCDiagnosticCapabilities requiredCapabilities,
                CancellationToken cancellationToken)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
                if (requiredCapabilities == null)
                {
                    throw new ArgumentNullException("requiredCapabilities");
                }
                if (!request.IsWrite)
                {
                    throw new InvalidOperationException(
                        "Identity-pinned SDO submission accepts Write requests only.");
                }

                ValidateSdoSubmitPolicy(request);
                attemptTracker.BeginSessionPreflight();
                var sessionGeneration = connection.SessionGeneration;
                ValidateRequiredSdoWriteSubmissionIdentity(
                    request,
                    requiredCapabilities,
                    null,
                    sessionGeneration,
                    null);
                return await RunStateMutatingAsync(
                    () => SubmitSdoWriteIdentityPinnedCore(
                        request,
                        requiredCapabilities,
                        null,
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

        private LMCOperationTicket SubmitSdoWriteIdentityPinnedCore(
            LMCSdoRequest request,
            LMCDiagnosticCapabilities requiredCapabilities,
            LMCSdoWriteTarget requiredTarget,
            long sessionGeneration,
            LMCSdoSubmissionAttemptTracker attemptTracker)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            attemptTracker.BeginCapabilityPreflight();
            var freshCapabilities = GetCapabilities();
            attemptTracker.RecordCapabilityIdentity(
                freshCapabilities.DiagnosticsBootId,
                freshCapabilities.MapRevision);
            ValidateRequiredSdoWriteSubmissionIdentity(
                request,
                requiredCapabilities,
                requiredTarget,
                sessionGeneration,
                freshCapabilities);
            return SubmitSdoCore(
                request,
                sessionGeneration,
                freshCapabilities,
                attemptTracker);
        }

        public async Task<LMCOperationTicket> SubmitSdoAsync(
            LMCSdoRequest request,
            LMCOperationTicket requiredIdentityTicket,
            CancellationToken cancellationToken)
        {
            var attemptTracker = new LMCSdoSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (requiredIdentityTicket == null)
                {
                    throw new ArgumentNullException(
                        "requiredIdentityTicket");
                }

                ValidateSdoSubmitPolicy(request);
                attemptTracker.BeginSessionPreflight();

                var sessionGeneration = connection.SessionGeneration;
                ValidateRequiredSdoSubmissionIdentity(
                    requiredIdentityTicket,
                    request,
                    sessionGeneration,
                    null);
                return await RunStateMutatingAsync(
                    () => SubmitSdoTrackedCore(
                        request,
                        sessionGeneration,
                        attemptTracker,
                        requiredIdentityTicket),
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
                return status.BindProvenance(
                    this,
                    sessionGeneration);
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
                return status.BindProvenance(
                    this,
                    sessionGeneration);
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
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.CancelOperation,
                    () => { });
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

        internal LMCSdoReadResult ReadInlineSdoToTerminal(
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

            return ReadInlineSdoToTerminalCore(
                request,
                expectedSessionGeneration,
                attemptTracker,
                attemptTracker.BeginStatusPolling,
                attemptTracker.RecordOperationStatus);
        }

        private LMCSdoReadResult ReadInlineSdoToTerminalCore(
            LMCSdoRequest request,
            long expectedSessionGeneration,
            ILMCSdoSubmissionAttemptTracker attemptTracker,
            Action beginStatusPolling,
            Action<LMCOperationStatus> recordOperationStatus)
        {
            if (attemptTracker == null)
            {
                throw new ArgumentNullException("attemptTracker");
            }

            inlineSdoReadGate.Wait();
            LMCOperationTicket ticket = null;
            LMCOperationStatus lastObservedStatus = null;
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
                    if (beginStatusPolling != null)
                    {
                        beginStatusPolling();
                    }

                    var status = GetOperationStatus(ticket);
                    lastObservedStatus = status;
                    if (recordOperationStatus != null)
                    {
                        recordOperationStatus(status);
                    }

                    if (status.IsTerminal)
                    {
                        return RequireSuccessfulInlineSdoRead(ticket, status);
                    }

                    if (poll + 1 < pollLimit)
                    {
                        Thread.Sleep(pollDelayMilliseconds);
                    }
                }

                throw CreateInlineSdoPollingTimeout(
                    ticket,
                    lastObservedStatus,
                    pollLimit);
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

        internal async Task<LMCSdoReadResult>
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

            return await ReadInlineSdoToTerminalCoreAsync(
                request,
                expectedSessionGeneration,
                attemptTracker,
                attemptTracker.BeginStatusPolling,
                attemptTracker.RecordOperationStatus,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<LMCSdoReadResult>
            ReadInlineSdoToTerminalCoreAsync(
                LMCSdoRequest request,
                long expectedSessionGeneration,
                ILMCSdoSubmissionAttemptTracker attemptTracker,
                Action beginStatusPolling,
                Action<LMCOperationStatus> recordOperationStatus,
                CancellationToken cancellationToken)
        {
            if (attemptTracker == null)
            {
                throw new ArgumentNullException("attemptTracker");
            }

            await inlineSdoReadGate.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            LMCOperationTicket ticket = null;
            LMCOperationStatus lastObservedStatus = null;
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
                    if (beginStatusPolling != null)
                    {
                        beginStatusPolling();
                    }

                    var status = await GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None).ConfigureAwait(false);
                    lastObservedStatus = status;
                    if (recordOperationStatus != null)
                    {
                        recordOperationStatus(status);
                    }

                    if (status.IsTerminal)
                    {
                        return RequireSuccessfulInlineSdoRead(ticket, status);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (poll + 1 < pollLimit)
                    {
                        await Task.Delay(
                            pollDelayMilliseconds,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                throw CreateInlineSdoPollingTimeout(
                    ticket,
                    lastObservedStatus,
                    pollLimit);
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

                if (lastObservedStatus != null
                    && lastObservedStatus.IsTerminal)
                {
                    return RequireSuccessfulInlineSdoRead(
                        ticket,
                        lastObservedStatus);
                }

                throw new LMCSdoReadWaitCanceledException(
                    ticket,
                    lastObservedStatus,
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
            ILMCSdoSubmissionAttemptTracker attemptTracker)
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

            if (request.IsWrite
                || (request.DataLength != 1
                    && request.DataLength != 2
                    && request.DataLength != 4))
            {
                throw new NotSupportedException(
                    "The typed SDO helper supports 1, 2, or 4-byte inline SDO Read results only.");
            }

            ValidateSdoSubmitPolicy(request);
        }

        private static LMCSdoReadResult
            RequireSuccessfulInlineSdoRead(
                LMCOperationTicket ticket,
                LMCOperationStatus status)
        {
            if (!status.IsSuccessful)
            {
                throw new LMCSdoReadOperationException(ticket, status);
            }

            return new LMCSdoReadResult(ticket, status);
        }

        private static LMCSdoReadPollingTimeoutException
            CreateInlineSdoPollingTimeout(
                LMCOperationTicket ticket,
                LMCOperationStatus lastObservedStatus,
                int pollLimit)
        {
            return new LMCSdoReadPollingTimeoutException(
                ticket,
                lastObservedStatus,
                pollLimit);
        }

        private static void ValidateSdoWritePolicy(LMCSdoRequest request)
        {
            if (!request.IsWrite)
            {
                return;
            }

            // Generic SDO Write no longer denies valid object addresses by ObjectIndex.
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

            if (request.IsWrite)
            {
                LMCDiagnosticsWritePolicy
                    .RequireSdoWriteVerificationCapabilities(capabilities);
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

        private void ValidateRequiredSdoSubmissionIdentity(
            LMCOperationTicket requiredIdentityTicket,
            LMCSdoRequest readRequest,
            long expectedSessionGeneration,
            LMCDiagnosticCapabilities freshCapabilities)
        {
            if (requiredIdentityTicket == null)
            {
                throw new ArgumentNullException(
                    "requiredIdentityTicket");
            }

            if (!ReferenceEquals(requiredIdentityTicket.Owner, this))
            {
                throw new InvalidOperationException(
                    "The required SDO identity ticket belongs to a different LMCConnection.");
            }

            if (requiredIdentityTicket.OperationKind
                    != LMCOperationKind.SDOWrite
                || requiredIdentityTicket.SubmittedSdoRequest == null
                || !requiredIdentityTicket.SubmittedSdoRequest.IsWrite)
            {
                throw new InvalidOperationException(
                    "Guarded SDO readback requires an SDO Write ticket with immutable submitted Write provenance.");
            }

            if (!LMCSdoWriteVerificationContext
                .ReadMatchesWriteTarget(
                    readRequest,
                    requiredIdentityTicket.SubmittedSdoRequest))
            {
                throw new InvalidOperationException(
                    "The guarded SDO Read target, type, and length must exactly match the submitted Write provenance.");
            }

            if (requiredIdentityTicket.ConnectionSessionGeneration
                    != expectedSessionGeneration
                || expectedSessionGeneration
                    != connection.SessionGeneration)
            {
                throw new InvalidOperationException(
                    "The required SDO identity ticket belongs to a stale connection session.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
            if (freshCapabilities == null)
            {
                return;
            }

            if (freshCapabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration
                || !freshCapabilities.IsBoundTo(
                    this,
                    expectedSessionGeneration)
                || freshCapabilities.DiagnosticsBootId
                    != requiredIdentityTicket.DiagnosticsBootId
                || freshCapabilities.MapRevision
                    != requiredIdentityTicket.SubmissionMapRevision)
            {
                throw new InvalidOperationException(
                    "The current diagnostics BootId or MapRevision does not match the required SDO identity ticket.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateRequiredSdoWriteSubmissionIdentity(
            LMCSdoRequest writeRequest,
            LMCDiagnosticCapabilities requiredCapabilities,
            LMCSdoWriteTarget requiredTarget,
            long expectedSessionGeneration,
            LMCDiagnosticCapabilities freshCapabilities)
        {
            if (!connection.IsConnected
                || expectedSessionGeneration <= 0
                || expectedSessionGeneration != connection.SessionGeneration)
            {
                throw new InvalidOperationException(
                    "The identity-pinned SDO Write belongs to a disconnected or stale connection session.");
            }

            if (requiredCapabilities == null
                || !requiredCapabilities.IsBoundTo(
                    this,
                    expectedSessionGeneration)
                || requiredCapabilities.DiagnosticsBuild == 0
                || requiredCapabilities.DiagnosticsBootId == 0
                || requiredCapabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "The identity-pinned SDO Write requires nonzero Build, BootId, and MapRevision capabilities from this diagnostics owner and session.");
            }

            if (requiredTarget != null
                && (!requiredTarget.Matches(writeRequest)
                    || !IsApprovedSdoWriteTarget(requiredTarget)))
            {
                throw new InvalidOperationException(
                    "The identity-pinned SDO Write request does not exactly match its SDK-approved target tuple and range.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
            if (freshCapabilities == null)
            {
                return;
            }

            if (!freshCapabilities.IsBoundTo(
                    this,
                    expectedSessionGeneration)
                || freshCapabilities.DiagnosticsBuild
                    != requiredCapabilities.DiagnosticsBuild
                || freshCapabilities.DiagnosticsBootId
                    != requiredCapabilities.DiagnosticsBootId
                || freshCapabilities.MapRevision
                    != requiredCapabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "Fresh diagnostics capabilities do not match the identity-pinned SDO Write Build, BootId, or MapRevision. No Write was submitted.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private static bool IsApprovedSdoWriteTarget(
            LMCSdoWriteTarget candidate)
        {
            var approvedTargets =
                LMCDiagnosticsWritePolicy.GetApprovedSdoWriteTargets();
            for (var index = 0; index < approvedTargets.Count; index++)
            {
                var approved = approvedTargets[index];
                if (approved != null
                    && candidate.SlaveReference == approved.SlaveReference
                    && candidate.ObjectIndex == approved.ObjectIndex
                    && candidate.SubIndex == approved.SubIndex
                    && candidate.ValueType == approved.ValueType
                    && candidate.DataLength == approved.DataLength
                    && candidate.MinimumIntegerValue
                        == approved.MinimumIntegerValue
                    && candidate.MaximumIntegerValue
                        == approved.MaximumIntegerValue)
                {
                    return true;
                }
            }

            return false;
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
                    : (ushort)0,
                request);
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

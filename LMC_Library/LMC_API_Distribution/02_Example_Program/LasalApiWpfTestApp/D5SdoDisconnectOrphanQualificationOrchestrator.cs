using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum D5SdoDisconnectOrphanQualificationDisposition
    {
        TerminalBeforeTransportLoss = 0,
        ApplicationRecoveryOnly = 1,
        OrphanQualified = 2
    }

    internal sealed class D5SdoOwnerTransportLossObservation
    {
        internal D5SdoOwnerTransportLossObservation(
            LMCOperationStatus lastStatusBeforeLoss,
            bool ownerTransportLossObserved)
        {
            LastStatusBeforeLoss = lastStatusBeforeLoss
                ?? throw new ArgumentNullException("lastStatusBeforeLoss");
            OwnerTransportLossObserved = ownerTransportLossObserved;
        }

        internal LMCOperationStatus LastStatusBeforeLoss
        {
            get;
            private set;
        }

        internal bool OwnerTransportLossObserved { get; private set; }
    }

    internal sealed class D5SdoDisconnectOrphanQualificationRequest
    {
        private readonly byte[] expectedRecoveryData;

        internal D5SdoDisconnectOrphanQualificationRequest(
            LMCConnection oldOwnerConnection,
            LMCDiagnosticCapabilities initialCapabilities,
            LMCSdoRequest longRunningReadRequest,
            LMCSdoRequest recoveryReadRequest,
            byte[] expectedRecoveryData)
        {
            OldOwnerConnection = oldOwnerConnection
                ?? throw new ArgumentNullException("oldOwnerConnection");
            InitialCapabilities = initialCapabilities
                ?? throw new ArgumentNullException("initialCapabilities");
            LongRunningReadRequest = longRunningReadRequest
                ?? throw new ArgumentNullException("longRunningReadRequest");
            RecoveryReadRequest = recoveryReadRequest
                ?? throw new ArgumentNullException("recoveryReadRequest");
            if (expectedRecoveryData == null)
            {
                throw new ArgumentNullException("expectedRecoveryData");
            }

            this.expectedRecoveryData =
                (byte[])expectedRecoveryData.Clone();
        }

        internal LMCConnection OldOwnerConnection { get; private set; }
        internal LMCDiagnosticCapabilities InitialCapabilities
        {
            get;
            private set;
        }
        internal LMCSdoRequest LongRunningReadRequest
        {
            get;
            private set;
        }
        internal LMCSdoRequest RecoveryReadRequest
        {
            get;
            private set;
        }
        internal byte[] ExpectedRecoveryData
        {
            get { return (byte[])expectedRecoveryData.Clone(); }
        }
    }

    internal sealed class D5SdoDisconnectOrphanRecoveryScope
    {
        internal D5SdoDisconnectOrphanRecoveryScope(
            D5SdoDisconnectOrphanQualificationRequest request)
        {
            Request = request ?? throw new ArgumentNullException("request");
            Stage = "PREFLIGHT";
        }

        internal D5SdoDisconnectOrphanQualificationRequest Request
        {
            get;
            private set;
        }
        internal string Stage { get; set; }
        internal D5SdoQuarantineSnapshot LedgerStartSnapshot { get; set; }
        internal D5SdoQuarantineHandle OldEvidenceHandle { get; set; }
        internal D5SdoQuarantineSnapshot EvidenceBaseline { get; set; }
        internal bool OldSubmitAttempted { get; set; }
        internal bool OldSubmissionOutcomeUncertain { get; set; }
        internal LMCOperationTicket OldTicket { get; set; }
        internal LMCOperationStatus LastStatusBeforeLoss { get; set; }
        internal bool OwnerTransportLossObserved { get; set; }
        internal bool HasExactRunningWitness { get; set; }
        internal LMCConnection NewOwnerConnection { get; set; }
        internal LMCDiagnosticCapabilities FirstRecoveryCapabilities
        {
            get;
            set;
        }
        internal LMCDiagnosticCapabilities RecoveryCapabilities
        {
            get;
            set;
        }
        internal LMCDiagnosticCapabilities FinalRecoveryCapabilities
        {
            get;
            set;
        }
        internal D5SdoRecoveryScopeDecision RecoveryScopeDecision
        {
            get;
            set;
        }
        internal LMCOperationTicket FirstRecoveryTicket { get; set; }
        internal LMCOperationStatus FirstRecoveryStatus { get; set; }
        internal LMCOperationTicket SecondRecoveryTicket { get; set; }
        internal LMCOperationStatus SecondRecoveryStatus { get; set; }
        internal bool FirstRecoverySubmissionOutcomeUncertain { get; set; }
        internal bool SecondRecoverySubmissionOutcomeUncertain { get; set; }
        internal int FirstRecoverySubmitAttemptCount { get; set; }
        internal int SecondRecoverySubmitAttemptCount { get; set; }
        internal int RecoveryResourceBusyRejectionCount { get; set; }
        internal Exception LastRecoveryResourceBusyException { get; set; }
        internal int RecoverySubmitAttemptLimit { get; set; }
        internal int RecoveryRetryAdmissionBudgetMilliseconds { get; set; }
        internal long RecoveryRetryStartMilliseconds { get; set; }
        internal long RecoveryRetryDeadlineMilliseconds { get; set; }
        internal long RecoveryRetryLastObservedMilliseconds { get; set; }
    }

    internal sealed class D5SdoDisconnectOrphanQualificationResult
    {
        internal D5SdoDisconnectOrphanQualificationResult(
            D5SdoDisconnectOrphanQualificationDisposition disposition,
            D5SdoDisconnectOrphanRecoveryScope recoveryScope,
            bool newConnectionRecovery,
            bool orphanQualified)
        {
            if (orphanQualified && !newConnectionRecovery)
            {
                throw new ArgumentException(
                    "Orphan qualification requires new-connection recovery proof.",
                    "orphanQualified");
            }

            var dispositionMatches =
                disposition
                    == D5SdoDisconnectOrphanQualificationDisposition
                        .TerminalBeforeTransportLoss
                    ? !newConnectionRecovery && !orphanQualified
                    : disposition
                        == D5SdoDisconnectOrphanQualificationDisposition
                            .ApplicationRecoveryOnly
                        ? newConnectionRecovery && !orphanQualified
                        : disposition
                            == D5SdoDisconnectOrphanQualificationDisposition
                                .OrphanQualified
                            && newConnectionRecovery
                            && orphanQualified;
            if (!dispositionMatches)
            {
                throw new ArgumentException(
                    "The D5 disconnect/orphan disposition does not match its recovery and orphan proof flags.",
                    "disposition");
            }

            Disposition = disposition;
            RecoveryScope = recoveryScope
                ?? throw new ArgumentNullException("recoveryScope");
            NewConnectionRecovery = newConnectionRecovery;
            OrphanQualified = orphanQualified;
        }

        internal D5SdoDisconnectOrphanQualificationDisposition Disposition
        {
            get;
            private set;
        }
        internal D5SdoDisconnectOrphanRecoveryScope RecoveryScope
        {
            get;
            private set;
        }
        internal bool NewConnectionRecovery { get; private set; }
        internal bool OrphanQualified { get; private set; }
    }

    internal sealed class D5SdoDisconnectOrphanQualificationOperations
    {
        internal Func<LMCConnection, bool> IsConnected { get; set; }
        internal Func<LMCConnection, LMCSdoRequest, CancellationToken,
            Task<LMCOperationTicket>> SubmitOldReadAsync { get; set; }
        internal Func<LMCOperationTicket, CancellationToken,
            Task<D5SdoOwnerTransportLossObservation>>
            ObserveOwnerTransportLossAsync { get; set; }
        internal Func<CancellationToken, Task<LMCConnection>>
            OpenNewConnectionAsync { get; set; }
        internal Func<LMCConnection, CancellationToken,
            Task<LMCDiagnosticCapabilities>> ReadCapabilitiesAsync
        {
            get;
            set;
        }
        internal Func<LMCConnection, LMCSdoRequest, CancellationToken,
            Task<LMCOperationTicket>> SubmitRecoveryReadAsync { get; set; }
        internal Func<LMCConnection, LMCOperationTicket,
            CancellationToken, Task<LMCOperationStatus>>
            WaitRecoveryTerminalAsync { get; set; }
        internal Func<long> GetMonotonicMilliseconds { get; set; }
        internal Func<int, CancellationToken, Task> DelayAsync { get; set; }
        internal Action<D5SdoDisconnectOrphanQualificationResult>
            WritePassLog { get; set; }
        internal Action<D5SdoDisconnectOrphanRecoveryScope, Exception>
            RecoveryRequired { get; set; }

        internal void Validate()
        {
            if (IsConnected == null
                || SubmitOldReadAsync == null
                || ObserveOwnerTransportLossAsync == null
                || OpenNewConnectionAsync == null
                || ReadCapabilitiesAsync == null
                || SubmitRecoveryReadAsync == null
                || WaitRecoveryTerminalAsync == null
                || GetMonotonicMilliseconds == null
                || DelayAsync == null
                || WritePassLog == null
                || RecoveryRequired == null)
            {
                throw new ArgumentException(
                    "D5 disconnect/orphan qualification requires connection observation, submission, transport-loss, reconnect, capability, terminal, monotonic-clock, delay, log, and recovery-publication operations.");
            }
        }
    }

    internal static class D5SdoDisconnectOrphanQualificationOrchestrator
    {
        private const long SuccessfulProofLedgerVersionAdvance = 6;
        internal const int RecoveryRetryDelayMilliseconds = 25;
        internal const int RecoveryRetryMarginMilliseconds = 5000;
        internal const int MaximumRecoveryRetryAdmissionMilliseconds = 120000;
        internal const int MaximumRecoverySubmitAttempts =
            ((MaximumRecoveryRetryAdmissionMilliseconds - 1)
                / RecoveryRetryDelayMilliseconds) + 1;

        internal static async Task<
            D5SdoDisconnectOrphanQualificationResult> RunAsync(
                D5SdoDisconnectOrphanQualificationRequest request,
                D5SdoQuarantineLedger quarantine,
                D5SdoDisconnectOrphanQualificationOperations operations,
                CancellationToken cancellationToken)
        {
            ValidatePreflight(request, quarantine);
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            operations.Validate();
            var scope = new D5SdoDisconnectOrphanRecoveryScope(request);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                scope.LedgerStartSnapshot = quarantine.CaptureSnapshot();
                if (scope.LedgerStartSnapshot.Entries.Count != 0)
                {
                    throw new InvalidOperationException(
                        "The D5 quarantine changed after the empty-ledger preflight.");
                }

                if (!operations.IsConnected(request.OldOwnerConnection))
                {
                    throw new InvalidOperationException(
                        "The old D5 owner must be connected before the long-running Read is submitted.");
                }

                scope.Stage = "ARM_OLD_SUBMISSION";
                scope.OldEvidenceHandle = quarantine.ArmUnknown(
                    LMCOperationKind.SDORead,
                    request.LongRunningReadRequest,
                    request.OldOwnerConnection,
                    request.InitialCapabilities.DiagnosticsBootId,
                    request.InitialCapabilities.MapRevision,
                    request.LongRunningReadRequest.SlaveReference,
                    request.LongRunningReadRequest.TimeoutCycles,
                    "disconnect-orphan-old-submit",
                    "old owner-loss probe submission outcome pending");

                scope.Stage = "SUBMIT_OLD_READ";
                scope.OldSubmitAttempted = true;
                try
                {
                    scope.OldTicket = await operations.SubmitOldReadAsync(
                        request.OldOwnerConnection,
                        request.LongRunningReadRequest,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    CaptureSubmissionFailure(
                        scope,
                        quarantine,
                        scope.OldEvidenceHandle,
                        request.LongRunningReadRequest,
                        request.OldOwnerConnection,
                        error,
                        true,
                        0);
                    throw;
                }

                ValidateTicketForLedger(
                    scope.OldTicket,
                    request.OldOwnerConnection,
                    "old owner-loss probe");
                quarantine.TransitionToAccepted(
                    scope.OldEvidenceHandle,
                    scope.OldTicket,
                    scope.OldTicket.DiagnosticsBootId,
                    scope.OldTicket.SubmissionMapRevision);
                ValidateTicket(
                    scope.OldTicket,
                    request.OldOwnerConnection,
                    request.InitialCapabilities,
                    request.LongRunningReadRequest,
                    "old owner-loss probe");
                scope.EvidenceBaseline = quarantine.CaptureSnapshot();
                ValidateSingleOldEvidence(scope);

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "OBSERVE_EXTERNAL_OWNER_LOSS";
                var observation = await operations
                    .ObserveOwnerTransportLossAsync(
                        scope.OldTicket,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (observation == null)
                {
                    throw new InvalidOperationException(
                        "The external D5 owner-loss observer returned no evidence.");
                }

                scope.LastStatusBeforeLoss =
                    observation.LastStatusBeforeLoss;
                scope.OwnerTransportLossObserved =
                    observation.OwnerTransportLossObserved;
                ValidateStatusIdentity(
                    scope.OldTicket,
                    scope.LastStatusBeforeLoss,
                    "last old-owner status");

                if (scope.LastStatusBeforeLoss.IsTerminal)
                {
                    ValidateExactTerminalStatus(
                        scope.OldTicket,
                        scope.LastStatusBeforeLoss,
                        request.LongRunningReadRequest,
                        "last old-owner status");
                    cancellationToken.ThrowIfCancellationRequested();
                    scope.Stage = "TERMINAL_BEFORE_EXTERNAL_LOSS";
                    quarantine.Disarm(scope.OldEvidenceHandle);
                    scope.OldEvidenceHandle = null;
                    return new D5SdoDisconnectOrphanQualificationResult(
                        D5SdoDisconnectOrphanQualificationDisposition
                            .TerminalBeforeTransportLoss,
                        scope,
                        false,
                        false);
                }

                ValidatePendingLossStatus(scope.LastStatusBeforeLoss);
                scope.HasExactRunningWitness =
                    scope.LastStatusBeforeLoss.State
                        == LMCOperationState.Running;
                if (!scope.OwnerTransportLossObserved)
                {
                    throw new InvalidOperationException(
                        "No owner transport loss was observed while the old D5 ticket was non-terminal.");
                }

                if (operations.IsConnected(request.OldOwnerConnection))
                {
                    throw new InvalidOperationException(
                        "The external owner-loss hook returned while the old LMCConnection was still connected.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "OPEN_NEW_CONNECTION";
                scope.NewOwnerConnection = await operations
                    .OpenNewConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
                ValidateNewOwnerConnection(scope, operations);

                scope.Stage = "READ_RECOVERY_CAPABILITIES_1";
                scope.FirstRecoveryCapabilities = await operations
                    .ReadCapabilitiesAsync(
                        scope.NewOwnerConnection,
                        cancellationToken)
                    .ConfigureAwait(false);
                scope.Stage = "READ_RECOVERY_CAPABILITIES_2";
                scope.RecoveryCapabilities = await operations
                    .ReadCapabilitiesAsync(
                        scope.NewOwnerConnection,
                        cancellationToken)
                    .ConfigureAwait(false);
                ValidateRecoveryCapabilities(scope);
                scope.RecoveryRetryAdmissionBudgetMilliseconds =
                    CalculateRecoveryRetryAdmissionBudgetMilliseconds(scope);
                scope.RecoverySubmitAttemptLimit =
                    CalculateRecoverySubmitAttemptLimit(scope);
                scope.RecoveryRetryStartMilliseconds =
                    ReadRecoveryMonotonicMilliseconds(scope, operations);
                scope.RecoveryRetryDeadlineMilliseconds = checked(
                    scope.RecoveryRetryStartMilliseconds
                    + scope.RecoveryRetryAdmissionBudgetMilliseconds);
                EnsureConnectionsRemainSeparated(scope, operations);

                scope.RecoveryScopeDecision =
                    D5SdoRecoveryScopePolicy.Evaluate(
                        scope.EvidenceBaseline.Entries,
                        scope.NewOwnerConnection,
                        scope.RecoveryCapabilities.DiagnosticsBootId,
                        scope.RecoveryCapabilities.MapRevision);
                if (!scope.RecoveryScopeDecision.NewConnectionRecovery
                    || scope.RecoveryScopeDecision.MixedEvidenceSessions
                    || !string.Equals(
                        scope.RecoveryScopeDecision.ScopeCode,
                        "new_connection_session",
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "The old D5 evidence does not prove one exact new-connection recovery scope.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "FIRST_RECOVERY_READ";
                var first = await SubmitAndWaitRecoveryWithBusyRetryAsync(
                    scope,
                    quarantine,
                    operations,
                    1,
                    cancellationToken).ConfigureAwait(false);
                scope.FirstRecoveryTicket = first.Ticket;
                scope.FirstRecoveryStatus = first.Status;

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "SECOND_RECOVERY_READ";
                var second = await SubmitAndWaitRecoveryWithBusyRetryAsync(
                    scope,
                    quarantine,
                    operations,
                    2,
                    cancellationToken).ConfigureAwait(false);
                scope.SecondRecoveryTicket = second.Ticket;
                scope.SecondRecoveryStatus = second.Status;
                if (scope.SecondRecoveryTicket.TicketId
                    == scope.FirstRecoveryTicket.TicketId)
                {
                    throw new InvalidOperationException(
                        "The two new-connection recovery Reads reused one ticket identity.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                scope.Stage = "READ_RECOVERY_CAPABILITIES_FINAL";
                scope.FinalRecoveryCapabilities = await operations
                    .ReadCapabilitiesAsync(
                        scope.NewOwnerConnection,
                        cancellationToken)
                    .ConfigureAwait(false);
                ValidateFinalRecoveryCapabilities(scope);
                EnsureConnectionsRemainSeparated(scope, operations);
                var candidate = quarantine.CaptureSnapshot();
                if (candidate.Version
                    != CheckedAdd(
                        scope.EvidenceBaseline.Version,
                        CheckedAdd(
                            SuccessfulProofLedgerVersionAdvance,
                            2L
                                * scope
                                    .RecoveryResourceBusyRejectionCount)))
                {
                    throw new InvalidOperationException(
                        "The D5 quarantine changed outside the two proof-owned accepted-ticket guards; ABA or persistent evidence mutation prevents clear.");
                }

                // A last Running status and a disconnected PC socket prove
                // only the application-visible boundary. They do not prove
                // that the PLC accepted the exact executor token in
                // MarkOrphan or drained its late callback. Until a durable
                // PLC lifecycle witness is available, fail closed and never
                // promote this recovery to OrphanQualified.
                const bool orphanQualified = false;
                const D5SdoDisconnectOrphanQualificationDisposition
                    disposition =
                        D5SdoDisconnectOrphanQualificationDisposition
                            .ApplicationRecoveryOnly;
                var result = new
                    D5SdoDisconnectOrphanQualificationResult(
                        disposition,
                        scope,
                        true,
                        orphanQualified);

                scope.Stage = "COMMIT_PROOF";
                cancellationToken.ThrowIfCancellationRequested();
                if (!quarantine.TryClearAfterProof(
                        scope.EvidenceBaseline,
                        candidate,
                        () =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            EnsureConnectionsRemainSeparated(
                                scope,
                                operations);
                            operations.WritePassLog(result);
                        }))
                {
                    throw new InvalidOperationException(
                        "The D5 quarantine changed before proof commit; recovery evidence was preserved.");
                }

                scope.OldEvidenceHandle = null;
                scope.Stage = "COMPLETE";
                return result;
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
                        "D5 disconnect/orphan recovery scope publication failed.",
                        new AggregateException(
                            primaryError,
                            recoveryError));
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw;
            }
        }

        private static void ValidatePreflight(
            D5SdoDisconnectOrphanQualificationRequest request,
            D5SdoQuarantineLedger quarantine)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (quarantine == null)
            {
                throw new ArgumentNullException("quarantine");
            }

            if (quarantine.HasEntries)
            {
                throw new InvalidOperationException(
                    "D5 disconnect/orphan qualification requires an empty quarantine before its old probe is armed.");
            }

            ValidateCapabilities(
                request.InitialCapabilities,
                "initialCapabilities");
            ValidateCapabilityProvenance(
                request.InitialCapabilities,
                request.OldOwnerConnection,
                "initialCapabilities");
            ValidateProbeRequest(request.LongRunningReadRequest);
            ValidateRecoveryRequest(request.RecoveryReadRequest);
            if (request.LongRunningReadRequest.SlaveReference
                != request.RecoveryReadRequest.SlaveReference)
            {
                throw new ArgumentException(
                    "The owner-loss probe and recovery Reads must use the same SlaveReference.",
                    "request");
            }

            var expected = request.ExpectedRecoveryData;
            if (expected.Length != 1)
            {
                throw new ArgumentException(
                    "The exact Int8 recovery value must contain one byte.",
                    "request");
            }
        }

        private static void ValidateProbeRequest(LMCSdoRequest request)
        {
            if (request.IsWrite
                || request.SlaveReference < 1
                || request.SlaveReference > 4
                || request.ObjectIndex == 0
                || (request.DataLength != 1
                    && request.DataLength != 2
                    && request.DataLength != 4)
                || request.TimeoutCycles < 1
                || request.TimeoutCycles > 60000)
            {
                throw new ArgumentException(
                    "The external owner-loss probe must be an exact Slave 1..4, nonzero-object, typed 1/2/4-byte SDO Read with TimeoutCycles 1..60000.",
                    "request");
            }
        }

        private static void ValidateRecoveryRequest(LMCSdoRequest request)
        {
            if (request.IsWrite
                || request.SlaveReference < 1
                || request.SlaveReference > 4
                || request.ObjectIndex != 0x6061
                || request.SubIndex != 0
                || request.ValueType != LMCSignalValueType.Int8
                || request.DataLength != 1
                || request.TimeoutCycles < 1
                || request.TimeoutCycles > 60000)
            {
                throw new ArgumentException(
                    "New-connection proof requires Slave 1..4, 0x6061:0, Int8/1 Read with TimeoutCycles 1..60000.",
                    "request");
            }
        }

        private static void ValidateCapabilities(
            LMCDiagnosticCapabilities capabilities,
            string argumentName)
        {
            if (capabilities == null
                || capabilities.Response == null
                || !capabilities.Response.IsSuccess
                || !capabilities.Supports(LMCDiagnosticCapability.SDORead)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline)
                || capabilities.MaxSdoDataBytes != 4
                || capabilities.BaseCycleTimeUs == 0
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "D5 disconnect/orphan "
                    + argumentName
                    + " requires SDO Read/general-inline, MaxSdoDataBytes=4, nonzero BaseCycleTimeUs, and nonzero BootId/MapRevision.");
            }
        }

        private static void ValidateSingleOldEvidence(
            D5SdoDisconnectOrphanRecoveryScope scope)
        {
            var entries = scope.EvidenceBaseline.Entries;
            if (scope.LedgerStartSnapshot == null
                || entries.Count != 1
                || scope.EvidenceBaseline.Version
                    != CheckedAdd(scope.LedgerStartSnapshot.Version, 2))
            {
                throw new InvalidOperationException(
                    "The D5 disconnect/orphan baseline must contain exactly one old accepted Read ticket after exactly one arm/accept transition pair.");
            }

            var evidence = entries[0];
            if (evidence.TicketId != scope.OldTicket.TicketId
                || evidence.OperationKind != LMCOperationKind.SDORead
                || !ReferenceEquals(
                    evidence.OwnerConnection,
                    scope.Request.OldOwnerConnection)
                || evidence.DiagnosticsBootId
                    != scope.OldTicket.DiagnosticsBootId
                || evidence.MapRevision
                    != scope.OldTicket.SubmissionMapRevision
                || !evidence.HasRequestMetadata
                || evidence.SlaveReference
                    != scope.Request.LongRunningReadRequest.SlaveReference
                || evidence.TimeoutCycles
                    != scope.Request.LongRunningReadRequest.TimeoutCycles
                || evidence.ObjectIndex
                    != scope.Request.LongRunningReadRequest.ObjectIndex
                || evidence.SubIndex
                    != scope.Request.LongRunningReadRequest.SubIndex
                || evidence.ValueType
                    != scope.Request.LongRunningReadRequest.ValueType
                || evidence.DataLength
                    != scope.Request.LongRunningReadRequest.DataLength)
            {
                throw new InvalidOperationException(
                    "The D5 disconnect/orphan baseline does not match its old accepted Read ticket.");
            }
        }

        private static void ValidatePendingLossStatus(
            LMCOperationStatus status)
        {
            if ((status.State != LMCOperationState.Queued
                    && status.State != LMCOperationState.Running)
                || status.Outcome != LMCOperationOutcome.NoneOrPending
                || status.CompletionCycle != 0
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultLength != 0
                || status.ResultValueType != LMCSignalValueType.Invalid
                || status.ResultData.Length != 0)
            {
                throw new InvalidOperationException(
                    "The last old-owner status is not an exact Queued/Running non-terminal witness.");
            }
        }

        private static void ValidateNewOwnerConnection(
            D5SdoDisconnectOrphanRecoveryScope scope,
            D5SdoDisconnectOrphanQualificationOperations operations)
        {
            if (scope.NewOwnerConnection == null
                || ReferenceEquals(
                    scope.NewOwnerConnection,
                    scope.Request.OldOwnerConnection)
                || !operations.IsConnected(scope.NewOwnerConnection))
            {
                throw new InvalidOperationException(
                    "D5 orphan recovery requires a connected, distinct new LMCConnection owner.");
            }

            if (operations.IsConnected(scope.Request.OldOwnerConnection))
            {
                throw new InvalidOperationException(
                    "The old D5 owner became connected again during new-owner recovery.");
            }
        }

        private static void ValidateRecoveryCapabilities(
            D5SdoDisconnectOrphanRecoveryScope scope)
        {
            ValidateCapabilities(
                scope.FirstRecoveryCapabilities,
                "first recovery capability sample");
            ValidateCapabilityProvenance(
                scope.FirstRecoveryCapabilities,
                scope.NewOwnerConnection,
                "first recovery capability sample");
            ValidateCapabilities(
                scope.RecoveryCapabilities,
                "second recovery capability sample");
            ValidateCapabilityProvenance(
                scope.RecoveryCapabilities,
                scope.NewOwnerConnection,
                "second recovery capability sample");
            var initial = scope.Request.InitialCapabilities;
            var first = scope.FirstRecoveryCapabilities;
            var second = scope.RecoveryCapabilities;
            if (!HasStableD5ReadContract(initial, first)
                || !HasStableD5ReadContract(first, second)
                || second.ObservationSequence
                    <= first.ObservationSequence)
            {
                throw new InvalidOperationException(
                    "D5 orphan recovery requires two fresh, increasing new-owner capability observations with a stable BootId/MapRevision, diagnostics build, capability mask, BaseCycleTimeUs, and SDO payload contract across the old and new owners.");
            }
        }

        private static void ValidateFinalRecoveryCapabilities(
            D5SdoDisconnectOrphanRecoveryScope scope)
        {
            ValidateCapabilities(
                scope.FinalRecoveryCapabilities,
                "final recovery capability sample");
            ValidateCapabilityProvenance(
                scope.FinalRecoveryCapabilities,
                scope.NewOwnerConnection,
                "final recovery capability sample");
            var expected = scope.RecoveryCapabilities;
            var actual = scope.FinalRecoveryCapabilities;
            if (!HasStableD5ReadContract(expected, actual)
                || actual.ObservationSequence
                    <= expected.ObservationSequence)
            {
                throw new InvalidOperationException(
                    "D5 orphan recovery requires a fresh final capability observation with a stable BootId/MapRevision, diagnostics build, capability mask, BaseCycleTimeUs, and SDO payload contract after the two exact new-connection recovery Reads.");
            }
        }

        private static bool HasStableD5ReadContract(
            LMCDiagnosticCapabilities expected,
            LMCDiagnosticCapabilities actual)
        {
            return expected != null
                && actual != null
                && actual.DiagnosticsBootId == expected.DiagnosticsBootId
                && actual.MapRevision == expected.MapRevision
                && actual.DiagnosticsBuild == expected.DiagnosticsBuild
                && actual.CapabilityBits == expected.CapabilityBits
                && actual.BaseCycleTimeUs == expected.BaseCycleTimeUs
                && actual.MaxSdoDataBytes == expected.MaxSdoDataBytes
                && actual.MaxRequestPayloadBytes
                    == expected.MaxRequestPayloadBytes
                && actual.MaxResponsePayloadBytes
                    == expected.MaxResponsePayloadBytes;
        }

        private static void ValidateCapabilityProvenance(
            LMCDiagnosticCapabilities capabilities,
            LMCConnection ownerConnection,
            string sampleName)
        {
            if (ownerConnection == null
                || !capabilities.IsBoundTo(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration))
            {
                throw new InvalidOperationException(
                    "D5 disconnect/orphan "
                    + sampleName
                    + " is not bound to the exact LMCConnection diagnostics owner and session generation.");
            }
        }

        private static async Task<RecoveryReadResult>
            SubmitAndWaitRecoveryWithBusyRetryAsync(
                D5SdoDisconnectOrphanRecoveryScope scope,
                D5SdoQuarantineLedger quarantine,
                D5SdoDisconnectOrphanQualificationOperations operations,
                int ordinal,
                CancellationToken cancellationToken)
        {
            for (var attempt = 1;
                attempt <= scope.RecoverySubmitAttemptLimit;
                attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (attempt > 1)
                {
                    EnsureRecoveryRetryAdmissionAvailable(scope, operations);
                }
                if (ordinal == 1)
                {
                    scope.FirstRecoverySubmitAttemptCount = attempt;
                }
                else
                {
                    scope.SecondRecoverySubmitAttemptCount = attempt;
                }

                try
                {
                    return await SubmitAndWaitRecoveryAsync(
                        scope,
                        quarantine,
                        operations,
                        ordinal,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    if (!IsExactResourceBusyRejection(error, scope))
                    {
                        throw;
                    }

                    scope.RecoveryResourceBusyRejectionCount++;
                    scope.LastRecoveryResourceBusyException = error;
                    cancellationToken.ThrowIfCancellationRequested();
                    var now = ReadRecoveryMonotonicMilliseconds(
                        scope,
                        operations);
                    if (attempt >= scope.RecoverySubmitAttemptLimit
                        || now >= scope.RecoveryRetryDeadlineMilliseconds)
                    {
                        throw new TimeoutException(
                            "The disconnected-owner D5 executor remained ResourceBusy for all "
                                + scope.RecoverySubmitAttemptLimit
                                + " bounded recovery Submit attempts or until the monotonic retry-admission deadline (25 ms scheduled retry interval; an already-started RPC is not claimed as part of that deadline).",
                            error);
                    }

                    var remainingMilliseconds = checked(
                        scope.RecoveryRetryDeadlineMilliseconds - now);
                    var delayMilliseconds = (int)Math.Min(
                        RecoveryRetryDelayMilliseconds,
                        remainingMilliseconds);
                    await operations.DelayAsync(
                        delayMilliseconds,
                        cancellationToken).ConfigureAwait(false);
                }
            }

            throw new InvalidOperationException(
                "The bounded D5 recovery Submit loop ended unexpectedly.");
        }

        private static int CalculateRecoverySubmitAttemptLimit(
            D5SdoDisconnectOrphanRecoveryScope scope)
        {
            var admissionBudgetMilliseconds =
                scope.RecoveryRetryAdmissionBudgetMilliseconds;
            if (admissionBudgetMilliseconds <= 0)
            {
                throw new InvalidOperationException(
                    "The D5 recovery retry-admission budget is invalid.");
            }

            var attempts =
                (((ulong)admissionBudgetMilliseconds - 1UL)
                    / (ulong)RecoveryRetryDelayMilliseconds) + 1UL;
            if (attempts < 2UL)
            {
                attempts = 2UL;
            }

            return checked((int)Math.Min(
                attempts,
                (ulong)MaximumRecoverySubmitAttempts));
        }

        private static int CalculateRecoveryRetryAdmissionBudgetMilliseconds(
            D5SdoDisconnectOrphanRecoveryScope scope)
        {
            var requestDurationMicroseconds =
                (ulong)scope.Request.LongRunningReadRequest.TimeoutCycles
                * scope.RecoveryCapabilities.BaseCycleTimeUs;
            var requestDurationMilliseconds =
                (requestDurationMicroseconds + 999UL) / 1000UL;
            var admissionBudgetMilliseconds = Math.Min(
                (ulong)MaximumRecoveryRetryAdmissionMilliseconds,
                requestDurationMilliseconds
                    + (ulong)RecoveryRetryMarginMilliseconds);
            if (admissionBudgetMilliseconds == 0UL)
            {
                throw new InvalidOperationException(
                    "The D5 recovery retry-admission budget resolved to zero.");
            }

            return checked((int)admissionBudgetMilliseconds);
        }

        private static void EnsureRecoveryRetryAdmissionAvailable(
            D5SdoDisconnectOrphanRecoveryScope scope,
            D5SdoDisconnectOrphanQualificationOperations operations)
        {
            if (ReadRecoveryMonotonicMilliseconds(scope, operations)
                >= scope.RecoveryRetryDeadlineMilliseconds)
            {
                throw new TimeoutException(
                    "The disconnected-owner D5 recovery retry-admission deadline elapsed before another Submit attempt could start.",
                    scope.LastRecoveryResourceBusyException);
            }
        }

        private static long ReadRecoveryMonotonicMilliseconds(
            D5SdoDisconnectOrphanRecoveryScope scope,
            D5SdoDisconnectOrphanQualificationOperations operations)
        {
            var now = operations.GetMonotonicMilliseconds();
            if (now < 0
                || now < scope.RecoveryRetryLastObservedMilliseconds)
            {
                throw new InvalidOperationException(
                    "The D5 recovery retry clock returned a negative or regressing observation.");
            }

            scope.RecoveryRetryLastObservedMilliseconds = now;
            return now;
        }

        private static bool IsExactResourceBusyRejection(
            Exception error,
            D5SdoDisconnectOrphanRecoveryScope scope)
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
            return LMCSdoSubmissionFailureContext.TryGet(error, out context)
                && context.Phase == LMCSdoSubmissionPhase.Submission
                && context.SubmissionOutcome
                    == LMCSdoSubmissionOutcome.Rejected
                && ReferenceEquals(
                    context.Request,
                    scope.Request.RecoveryReadRequest)
                && context.Ticket == null
                && context.DiagnosticsBootId
                    == scope.RecoveryCapabilities.DiagnosticsBootId
                && context.MapRevision
                    == scope.RecoveryCapabilities.MapRevision;
        }

        private static async Task<RecoveryReadResult>
            SubmitAndWaitRecoveryAsync(
                D5SdoDisconnectOrphanRecoveryScope scope,
                D5SdoQuarantineLedger quarantine,
                D5SdoDisconnectOrphanQualificationOperations operations,
                int ordinal,
                CancellationToken cancellationToken)
        {
            EnsureConnectionsRemainSeparated(scope, operations);
            var request = scope.Request.RecoveryReadRequest;
            var guard = quarantine.ArmUnknown(
                LMCOperationKind.SDORead,
                request,
                scope.NewOwnerConnection,
                scope.RecoveryCapabilities.DiagnosticsBootId,
                scope.RecoveryCapabilities.MapRevision,
                request.SlaveReference,
                request.TimeoutCycles,
                "disconnect-orphan-recovery-" + ordinal,
                "proof-owned recovery submission outcome pending");

            LMCOperationTicket ticket;
            try
            {
                ticket = await operations.SubmitRecoveryReadAsync(
                    scope.NewOwnerConnection,
                    request,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                CaptureSubmissionFailure(
                    scope,
                    quarantine,
                    guard,
                    request,
                    scope.NewOwnerConnection,
                    error,
                    false,
                    ordinal);
                throw;
            }

            ValidateTicketForLedger(
                ticket,
                scope.NewOwnerConnection,
                "new-connection recovery " + ordinal);
            quarantine.TransitionToAccepted(
                guard,
                ticket,
                ticket.DiagnosticsBootId,
                ticket.SubmissionMapRevision);
            SetRecoveryTicket(scope, ordinal, ticket);
            ValidateTicket(
                ticket,
                scope.NewOwnerConnection,
                scope.RecoveryCapabilities,
                request,
                "new-connection recovery " + ordinal);
            cancellationToken.ThrowIfCancellationRequested();

            var status = await operations.WaitRecoveryTerminalAsync(
                scope.NewOwnerConnection,
                ticket,
                cancellationToken).ConfigureAwait(false);
            SetRecoveryStatus(scope, ordinal, status);
            ValidateRecoveryStatus(
                ticket,
                status,
                scope.Request.ExpectedRecoveryData,
                "new-connection recovery " + ordinal);
            cancellationToken.ThrowIfCancellationRequested();
            quarantine.Disarm(guard);
            return new RecoveryReadResult(ticket, status);
        }

        private static void CaptureSubmissionFailure(
            D5SdoDisconnectOrphanRecoveryScope scope,
            D5SdoQuarantineLedger quarantine,
            D5SdoQuarantineHandle handle,
            LMCSdoRequest request,
            LMCConnection owner,
            Exception error,
            bool oldSubmission,
            int recoveryOrdinal)
        {
            LMCSdoSubmissionFailureContext context;
            if (!LMCSdoSubmissionFailureContext.TryGet(
                    error,
                    out context))
            {
                SetSubmissionUncertain(
                    scope,
                    oldSubmission,
                    recoveryOrdinal);
                return;
            }

            if (!ReferenceEquals(context.Request, request))
            {
                SetSubmissionUncertain(
                    scope,
                    oldSubmission,
                    recoveryOrdinal);
                return;
            }

            if (context.SubmissionOutcome
                    == LMCSdoSubmissionOutcome.NotAttempted
                || context.SubmissionOutcome
                    == LMCSdoSubmissionOutcome.Rejected)
            {
                quarantine.Disarm(handle);
                return;
            }

            if (context.SubmissionOutcome
                == LMCSdoSubmissionOutcome.OutcomeUncertain)
            {
                SetSubmissionUncertain(
                    scope,
                    oldSubmission,
                    recoveryOrdinal);
                if (context.DiagnosticsBootId != 0
                    && context.MapRevision != 0)
                {
                    quarantine.ReconcileUnknown(
                        handle,
                        context.DiagnosticsBootId,
                        context.MapRevision);
                }
                return;
            }

            if (context.SubmissionOutcome
                == LMCSdoSubmissionOutcome.Accepted)
            {
                ValidateTicketForLedger(
                    context.Ticket,
                    owner,
                    oldSubmission
                        ? "accepted old owner-loss failure"
                        : "accepted recovery failure "
                            + recoveryOrdinal);
                quarantine.TransitionToAccepted(
                    handle,
                    context.Ticket,
                    context.DiagnosticsBootId,
                    context.MapRevision);
                if (oldSubmission)
                {
                    scope.OldTicket = context.Ticket;
                }
                else
                {
                    SetRecoveryTicket(
                        scope,
                        recoveryOrdinal,
                        context.Ticket);
                }
                return;
            }

            SetSubmissionUncertain(
                scope,
                oldSubmission,
                recoveryOrdinal);
        }

        private static void SetSubmissionUncertain(
            D5SdoDisconnectOrphanRecoveryScope scope,
            bool oldSubmission,
            int recoveryOrdinal)
        {
            if (oldSubmission)
            {
                scope.OldSubmissionOutcomeUncertain = true;
            }
            else if (recoveryOrdinal == 1)
            {
                scope.FirstRecoverySubmissionOutcomeUncertain = true;
            }
            else
            {
                scope.SecondRecoverySubmissionOutcomeUncertain = true;
            }
        }

        private static void SetRecoveryTicket(
            D5SdoDisconnectOrphanRecoveryScope scope,
            int recoveryOrdinal,
            LMCOperationTicket ticket)
        {
            if (recoveryOrdinal == 1)
            {
                scope.FirstRecoveryTicket = ticket;
            }
            else
            {
                scope.SecondRecoveryTicket = ticket;
            }
        }

        private static void SetRecoveryStatus(
            D5SdoDisconnectOrphanRecoveryScope scope,
            int recoveryOrdinal,
            LMCOperationStatus status)
        {
            if (recoveryOrdinal == 1)
            {
                scope.FirstRecoveryStatus = status;
            }
            else
            {
                scope.SecondRecoveryStatus = status;
            }
        }

        private static void ValidateTicketForLedger(
            LMCOperationTicket ticket,
            LMCConnection owner,
            string stage)
        {
            if (ticket == null
                || ticket.TicketId == 0
                || ticket.OperationKind != LMCOperationKind.SDORead
                || ticket.DiagnosticsBootId == 0
                || ticket.SubmissionMapRevision == 0
                || !ticket.BelongsTo(owner))
            {
                throw new InvalidOperationException(
                    "The "
                    + stage
                    + " ticket cannot be preserved as exact owner-bound SDO Read evidence.");
            }
        }

        private static void ValidateTicket(
            LMCOperationTicket ticket,
            LMCConnection owner,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoRequest request,
            string stage)
        {
            ValidateTicketForLedger(ticket, owner, stage);
            if (!ticket.BelongsToCurrentSession(owner)
                || ticket.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || ticket.SubmissionMapRevision
                    != capabilities.MapRevision
                || ticket.UsesExtendedResultChunks
                || ticket.RequestedResultLength != request.DataLength
                || ticket.ResultValueType != request.ValueType)
            {
                throw new InvalidOperationException(
                    "The "
                    + stage
                    + " ticket does not match the exact owner/session/capability/request identity.");
            }
        }

        private static void ValidateStatusIdentity(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            string stage)
        {
            if (status == null
                || status.Response == null
                || !status.Response.IsSuccess
                || status.TicketId != ticket.TicketId
                || status.OperationKind != ticket.OperationKind
                || status.DiagnosticsBootId != ticket.DiagnosticsBootId
                || status.SubmitCycle != ticket.QueuedCycle)
            {
                throw new InvalidOperationException(
                    "The "
                    + stage
                    + " does not match the exact ticket identity.");
            }
        }

        private static void ValidateExactTerminalStatus(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            LMCSdoRequest request,
            string stage)
        {
            ValidateStatusIdentity(ticket, status, stage);
            var stateAndOutcomeMatch =
                status.State == LMCOperationState.Completed
                    ? status.Outcome == LMCOperationOutcome.Success
                    : status.State == LMCOperationState.Failed
                        ? status.Outcome == LMCOperationOutcome.Failed
                        : status.State == LMCOperationState.Cancelled
                            ? status.Outcome
                                == LMCOperationOutcome.Cancelled
                            : status.State == LMCOperationState.Expired
                                && status.Outcome
                                    == LMCOperationOutcome.TimedOut;
            if (!stateAndOutcomeMatch || status.CompletionCycle == 0)
            {
                throw new InvalidOperationException(
                    "The "
                    + stage
                    + " is not an exact terminal state/outcome witness.");
            }

            var data = status.ResultData;
            if (status.State == LMCOperationState.Completed)
            {
                if (status.OperationErrorId != 0
                    || status.OperationDetail != 0
                    || status.ResultLength != request.DataLength
                    || status.ResultValueType != request.ValueType
                    || data.Length != request.DataLength)
                {
                    throw new InvalidOperationException(
                        "The completed "
                        + stage
                        + " does not contain the exact SDO Read result metadata.");
                }

                return;
            }

            if ((status.State == LMCOperationState.Cancelled
                    || status.State == LMCOperationState.Expired)
                && status.OperationErrorId != 0)
            {
                throw new InvalidOperationException(
                    "The cancelled/expired "
                    + stage
                    + " contains an invalid operation error.");
            }

            if (status.ResultLength != 0
                || status.ResultValueType != LMCSignalValueType.Invalid
                || data.Length != 0)
            {
                throw new InvalidOperationException(
                    "The unsuccessful "
                    + stage
                    + " must not contain SDO result data.");
            }
        }

        private static void ValidateRecoveryStatus(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            byte[] expectedData,
            string stage)
        {
            ValidateStatusIdentity(ticket, status, stage);
            var actualData = status.ResultData;
            if (status.State != LMCOperationState.Completed
                || status.Outcome != LMCOperationOutcome.Success
                || status.CompletionCycle == 0
                || status.OperationErrorId != 0
                || status.OperationDetail != 0
                || status.ResultValueType != LMCSignalValueType.Int8
                || status.ResultLength != 1
                || actualData.Length != 1
                || expectedData == null
                || expectedData.Length != 1
                || actualData[0] != expectedData[0])
            {
                throw new InvalidOperationException(
                    "The "
                    + stage
                    + " terminal result is not exact Completed/Success Int8/1 same-value proof.");
            }
        }

        private static void EnsureConnectionsRemainSeparated(
            D5SdoDisconnectOrphanRecoveryScope scope,
            D5SdoDisconnectOrphanQualificationOperations operations)
        {
            if (operations.IsConnected(scope.Request.OldOwnerConnection)
                || scope.NewOwnerConnection == null
                || ReferenceEquals(
                    scope.NewOwnerConnection,
                    scope.Request.OldOwnerConnection)
                || !operations.IsConnected(scope.NewOwnerConnection))
            {
                throw new InvalidOperationException(
                    "D5 new-connection proof requires the old owner to remain disconnected and the distinct new owner to remain connected.");
            }
        }

        private static long CheckedAdd(long value, long increment)
        {
            if (value > long.MaxValue - increment)
            {
                throw new InvalidOperationException(
                    "The D5 quarantine version cannot represent proof-owned transitions.");
            }

            return value + increment;
        }

        private sealed class RecoveryReadResult
        {
            internal RecoveryReadResult(
                LMCOperationTicket ticket,
                LMCOperationStatus status)
            {
                Ticket = ticket;
                Status = status;
            }

            internal LMCOperationTicket Ticket { get; private set; }
            internal LMCOperationStatus Status { get; private set; }
        }
    }
}

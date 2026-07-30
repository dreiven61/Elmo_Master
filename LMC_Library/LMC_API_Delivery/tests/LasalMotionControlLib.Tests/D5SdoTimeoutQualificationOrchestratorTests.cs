using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoTimeoutQualificationOrchestratorTests
    {
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint MapRevision = 0x957F101Eu;
        private static readonly byte[] ExpectedRecoveryData = { 8 };

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5Timeout.PreflightPublishesAndDoesNoIo",
                PreflightPublishesAndDoesNoIo);
            tests.Add(
                "Qualification.D5Timeout.PreCanceledPublishesScope",
                PreCanceledPublishesScope);
            tests.Add(
                "Qualification.D5Timeout.ImmediateRecoverySucceeds",
                ImmediateRecoverySucceeds);
            tests.Add(
                "Qualification.D5Timeout.ExactBusyRetriesThenSucceeds",
                ExactBusyRetriesThenSucceeds);
            tests.Add(
                "Qualification.D5Timeout.RetryExhaustionIsExact",
                RetryExhaustionIsExact);
            tests.Add(
                "Qualification.D5Timeout.UncertainAndAmbiguousNeverRetry",
                UncertainAndAmbiguousNeverRetry);
            tests.Add(
                "Qualification.D5Timeout.AcceptedFailureTicketsPreserved",
                AcceptedFailureTicketsPreserved);
            tests.Add(
                "Qualification.D5Timeout.ExactBusyEvidenceRequired",
                ExactBusyEvidenceRequired);
            tests.Add(
                "Qualification.D5Timeout.CancellationDuringDelayPublishes",
                CancellationDuringDelayPublishes);
            tests.Add(
                "Qualification.D5Timeout.TicketProvenanceIsExact",
                TicketProvenanceIsExact);
            tests.Add(
                "Qualification.D5Timeout.TimeoutTerminalIsExact",
                TimeoutTerminalIsExact);
            tests.Add(
                "Qualification.D5Timeout.RecoveryTerminalIsExact",
                RecoveryTerminalIsExact);
            tests.Add(
                "Qualification.D5Timeout.ExpectedValueIsImmutable",
                ExpectedValueIsImmutable);
            tests.Add(
                "Qualification.D5Timeout.RecoveryPublicationFailureAggregates",
                RecoveryPublicationFailureAggregates);
        }

        private static void PreflightPublishesAndDoesNoIo()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var operations = harness.CreateOperations();
                var invalid = new[]
                {
                    Request(connection, timeoutRequest: Read(5, timeoutCycles: 1)),
                    Request(connection, timeoutRequest: Read(objectIndex: 0x6060, timeoutCycles: 1)),
                    Request(connection, timeoutRequest: Read(subIndex: 1, timeoutCycles: 1)),
                    Request(connection, timeoutRequest: Read(valueType: LMCSignalValueType.UInt8, timeoutCycles: 1)),
                    Request(
                        connection,
                        timeoutRequest: Read(
                            valueType: LMCSignalValueType.Int16,
                            dataLength: 2,
                            timeoutCycles: 1)),
                    Request(connection, timeoutRequest: Read(timeoutCycles: 2)),
                    Request(connection, recoveryRequest: Read(timeoutCycles: 1)),
                    Request(connection, recoveryRequest: Read(timeoutCycles: 60001)),
                    Request(
                        connection,
                        recoveryRequest: Read(
                            slaveReference: 2,
                            timeoutCycles: 100)),
                    Request(
                        connection,
                        capabilities: Capabilities(
                            connection,
                            LMCDiagnosticCapability.SDORead)),
                    Request(
                        connection,
                        capabilities: Capabilities(
                            connection,
                            RequiredCapabilities,
                            maxSdoDataBytes: 2)),
                    Request(
                        connection,
                        capabilities: Capabilities(
                            connection,
                            RequiredCapabilities,
                            diagnosticsBootId: 0)),
                    Request(
                        connection,
                        capabilities: Capabilities(
                            connection,
                            RequiredCapabilities,
                            mapRevision: 0)),
                    Request(
                        connection,
                        expectedRecoveryData: new byte[] { 8, 9 }),
                    Request(
                        connection,
                        timeoutRequest: LMCSdoRequest.CreateWrite(
                            1,
                            0x6061,
                            0,
                            LMCSignalValueType.Int32,
                            new byte[] { 8, 0, 0, 0 },
                            1))
                };

                AssertEx.Throws<ArgumentNullException>(
                    () => Run(null, operations));
                AssertEx.Throws<ArgumentNullException>(
                    () => Run(Request(connection), null));
                AssertEx.Throws<ArgumentException>(
                    () => Run(
                        Request(connection),
                        new D5SdoTimeoutQualificationOperations
                        {
                            SubmitAsync = operations.SubmitAsync,
                            RecoveryRequired = operations.RecoveryRequired
                        }));
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.Equal("PREFLIGHT", harness.RecoveryScope.Stage);
                AssertEx.Throws<ArgumentException>(
                    () => Run(
                        Request(connection),
                        new D5SdoTimeoutQualificationOperations
                        {
                            SubmitAsync = operations.SubmitAsync,
                            WaitForTerminalAsync =
                                operations.WaitForTerminalAsync,
                            DelayAsync = operations.DelayAsync
                        }));
                AssertEx.Equal(1, harness.RecoveryCount);

                for (var index = 0; index < invalid.Length; index++)
                {
                    AssertEx.Throws<Exception>(
                        () => Run(invalid[index], operations));
                }

                AssertEx.Equal(0, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(0, harness.DelayCount);
                AssertEx.Equal(invalid.Length + 1, harness.RecoveryCount);
                AssertEx.Equal("PREFLIGHT", harness.RecoveryScope.Stage);
            }
        }

        private static void PreCanceledPublishesScope()
        {
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(connection);
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations(),
                        cancellation.Token));

                AssertEx.Equal(0, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(0, harness.DelayCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.Equal(
                    "PREFLIGHT_COMPLETE",
                    harness.RecoveryScope.Stage);
                AssertEx.False(harness.RecoveryScope.HasAcceptedTickets);
                AssertEx.False(
                    harness.RecoveryScope.HasUncertainSubmissionOutcome);
            }
        }

        private static void ImmediateRecoverySucceeds()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var result = Run(
                    harness.Request,
                    harness.CreateOperations());

                AssertEx.Equal(
                    "SubmitTimeout,WaitTimeout,SubmitRecovery1,WaitRecovery",
                    string.Join(",", harness.Events));
                AssertEx.Equal(2, harness.SubmitCount);
                AssertEx.Equal(2, harness.WaitCount);
                AssertEx.Equal(0, harness.DelayCount);
                AssertEx.Equal(0, harness.RecoveryCount);
                AssertEx.Equal("COMPLETE", result.RecoveryScope.Stage);
                AssertEx.True(result.RecoveryScope.TimeoutSubmitAttempted);
                AssertEx.Equal(1, result.RecoveryScope.RecoverySubmitAttemptCount);
                AssertEx.Equal(
                    0,
                    result.RecoveryScope.RecoveryResourceBusyRejectionCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.TimeoutTicket,
                        result.RecoveryScope.TimeoutTicket));
                AssertEx.True(
                    ReferenceEquals(
                        harness.RecoveryTicket,
                        result.RecoveryScope.RecoveryTicket));
                AssertEx.True(result.RecoveryScope.HasAcceptedTickets);
                AssertEx.False(
                    result.RecoveryScope.HasUncertainSubmissionOutcome);
                AssertEx.Equal(
                    LMCOperationState.Expired,
                    result.TimeoutTerminalStatus.State);
                AssertEx.SequenceEqual(
                    ExpectedRecoveryData,
                    result.RecoveryTerminalStatus.ResultData);
            }
        }

        private static void ExactBusyRetriesThenSucceeds()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection)
                {
                    BusyFailuresBeforeSuccess = 3
                };

                var result = Run(
                    harness.Request,
                    harness.CreateOperations());

                AssertEx.Equal(5, harness.SubmitCount);
                AssertEx.Equal(2, harness.WaitCount);
                AssertEx.Equal(3, harness.DelayCount);
                AssertEx.Equal(0, harness.RecoveryCount);
                AssertEx.Equal(4, result.RecoveryScope.RecoverySubmitAttemptCount);
                AssertEx.Equal(
                    3,
                    result.RecoveryScope.RecoveryResourceBusyRejectionCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.LastBusyException,
                        result.RecoveryScope.LastResourceBusyException));
                for (var index = 0; index < harness.DelayValues.Count; index++)
                {
                    AssertEx.Equal(
                        D5SdoTimeoutQualificationOrchestrator
                            .RecoveryRetryDelayMilliseconds,
                        harness.DelayValues[index]);
                }

                AssertEx.Equal("COMPLETE", result.RecoveryScope.Stage);
            }
        }

        private static void RetryExhaustionIsExact()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection)
                {
                    BusyFailuresBeforeSuccess =
                        D5SdoTimeoutQualificationOrchestrator
                            .MaximumRecoverySubmitAttempts
                };

                var error = AssertEx.Throws<TimeoutException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations()));

                AssertEx.Contains(
                    "all 600 bounded recovery Submit attempts",
                    error.Message);
                AssertEx.True(
                    ReferenceEquals(
                        harness.LastBusyException,
                        error.InnerException));
                AssertEx.Equal(
                    1 + D5SdoTimeoutQualificationOrchestrator
                        .MaximumRecoverySubmitAttempts,
                    harness.SubmitCount);
                AssertEx.Equal(
                    D5SdoTimeoutQualificationOrchestrator
                        .MaximumRecoverySubmitAttempts - 1,
                    harness.DelayCount);
                AssertEx.Equal(1, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.Equal(
                    D5SdoTimeoutQualificationOrchestrator
                        .MaximumRecoverySubmitAttempts,
                    harness.RecoveryScope.RecoverySubmitAttemptCount);
                AssertEx.Equal(
                    D5SdoTimeoutQualificationOrchestrator
                        .MaximumRecoverySubmitAttempts,
                    harness.RecoveryScope
                        .RecoveryResourceBusyRejectionCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.TimeoutTicket,
                        harness.RecoveryScope.TimeoutTicket));
                AssertEx.Equal(null, harness.RecoveryScope.RecoveryTicket);
                AssertEx.Equal(
                    "SUBMIT_RECOVERY",
                    harness.RecoveryScope.Stage);
            }
        }

        private static void UncertainAndAmbiguousNeverRetry()
        {
            using (var connection = new LMCConnection())
            {
                var timeoutUncertainHarness = new Harness(connection);
                var timeoutUncertain = SubmissionFailure(
                    timeoutUncertainHarness.Request.TimeoutRequest,
                    new InvalidOperationException(
                        "timeout wire outcome uncertain"),
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    DiagnosticsBootId,
                    MapRevision);
                timeoutUncertainHarness.TimeoutSubmitError = timeoutUncertain;

                var observedTimeoutUncertain =
                    AssertEx.Throws<InvalidOperationException>(
                        () => Run(
                            timeoutUncertainHarness.Request,
                            timeoutUncertainHarness.CreateOperations()));

                AssertEx.True(
                    ReferenceEquals(
                        timeoutUncertain,
                        observedTimeoutUncertain));
                AssertEx.Equal(1, timeoutUncertainHarness.SubmitCount);
                AssertEx.Equal(0, timeoutUncertainHarness.WaitCount);
                AssertEx.Equal(0, timeoutUncertainHarness.DelayCount);
                AssertEx.Equal(1, timeoutUncertainHarness.RecoveryCount);
                AssertEx.True(
                    timeoutUncertainHarness.RecoveryScope
                        .TimeoutSubmissionOutcomeUncertain);
                AssertEx.True(
                    timeoutUncertainHarness.RecoveryScope
                        .HasUncertainSubmissionOutcome);

                var uncertainHarness = new Harness(connection);
                var uncertain = SubmissionFailure(
                    uncertainHarness.Request.RecoveryRequest,
                    new InvalidOperationException("wire outcome uncertain"),
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    DiagnosticsBootId,
                    MapRevision);
                uncertainHarness.RecoverySubmissionErrorFactory =
                    attempt => uncertain;

                var observedUncertain = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        uncertainHarness.Request,
                        uncertainHarness.CreateOperations()));

                AssertEx.True(ReferenceEquals(uncertain, observedUncertain));
                AssertEx.Equal(2, uncertainHarness.SubmitCount);
                AssertEx.Equal(0, uncertainHarness.DelayCount);
                AssertEx.Equal(1, uncertainHarness.RecoveryCount);
                AssertEx.True(
                    uncertainHarness.RecoveryScope
                        .RecoverySubmissionOutcomeUncertain);
                AssertEx.True(
                    uncertainHarness.RecoveryScope
                        .HasUncertainSubmissionOutcome);

                var ambiguousHarness = new Harness(connection);
                var ambiguous = new InvalidOperationException(
                    "ambiguous submission failure");
                ambiguousHarness.RecoverySubmissionErrorFactory =
                    attempt => ambiguous;

                var observedAmbiguous = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        ambiguousHarness.Request,
                        ambiguousHarness.CreateOperations()));

                AssertEx.True(ReferenceEquals(ambiguous, observedAmbiguous));
                AssertEx.Equal(2, ambiguousHarness.SubmitCount);
                AssertEx.Equal(0, ambiguousHarness.DelayCount);
                AssertEx.Equal(1, ambiguousHarness.RecoveryCount);
                AssertEx.False(
                    ambiguousHarness.RecoveryScope
                        .HasUncertainSubmissionOutcome);
            }
        }

        private static void AcceptedFailureTicketsPreserved()
        {
            using (var connection = new LMCConnection())
            {
                var timeoutHarness = new Harness(connection);
                var timeoutAccepted = AcceptedFailure(
                    timeoutHarness.Request.TimeoutRequest,
                    timeoutHarness.TimeoutTicket);
                timeoutHarness.TimeoutSubmitError = timeoutAccepted;

                var observedTimeout = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        timeoutHarness.Request,
                        timeoutHarness.CreateOperations()));

                AssertEx.True(
                    ReferenceEquals(timeoutAccepted, observedTimeout));
                AssertEx.Equal(1, timeoutHarness.SubmitCount);
                AssertEx.Equal(0, timeoutHarness.WaitCount);
                AssertEx.Equal(0, timeoutHarness.DelayCount);
                AssertEx.True(
                    ReferenceEquals(
                        timeoutHarness.TimeoutTicket,
                        timeoutHarness.RecoveryScope.TimeoutTicket));

                var recoveryHarness = new Harness(connection);
                var recoveryAccepted = AcceptedFailure(
                    recoveryHarness.Request.RecoveryRequest,
                    recoveryHarness.RecoveryTicket);
                recoveryHarness.RecoverySubmissionErrorFactory =
                    attempt => recoveryAccepted;

                var observedRecovery = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        recoveryHarness.Request,
                        recoveryHarness.CreateOperations()));

                AssertEx.True(
                    ReferenceEquals(recoveryAccepted, observedRecovery));
                AssertEx.Equal(2, recoveryHarness.SubmitCount);
                AssertEx.Equal(1, recoveryHarness.WaitCount);
                AssertEx.Equal(0, recoveryHarness.DelayCount);
                AssertEx.True(
                    ReferenceEquals(
                        recoveryHarness.TimeoutTicket,
                        recoveryHarness.RecoveryScope.TimeoutTicket));
                AssertEx.True(
                    ReferenceEquals(
                        recoveryHarness.RecoveryTicket,
                        recoveryHarness.RecoveryScope.RecoveryTicket));
            }
        }

        private static void ExactBusyEvidenceRequired()
        {
            using (var connection = new LMCConnection())
            {
                var failures = new Func<Harness, Exception>[]
                {
                    harness => new LMCDiagnosticsCommandException(
                        "busy without submission context",
                        Response(LMCDiagnosticsDetailCode.ResourceBusy)),
                    harness => SubmissionFailure(
                        harness.Request.RecoveryRequest,
                        new LMCDiagnosticsCommandException(
                            "wrong detail",
                            Response(LMCDiagnosticsDetailCode.NotReady)),
                        LMCSdoSubmissionOutcome.Rejected,
                        DiagnosticsBootId,
                        MapRevision),
                    harness => SubmissionFailure(
                        Read(timeoutCycles: 100),
                        new LMCDiagnosticsCommandException(
                            "wrong request instance",
                            Response(LMCDiagnosticsDetailCode.ResourceBusy)),
                        LMCSdoSubmissionOutcome.Rejected,
                        DiagnosticsBootId,
                        MapRevision),
                    harness => SubmissionFailure(
                        harness.Request.RecoveryRequest,
                        new LMCDiagnosticsCommandException(
                            "wrong BootId",
                            Response(LMCDiagnosticsDetailCode.ResourceBusy)),
                        LMCSdoSubmissionOutcome.Rejected,
                        DiagnosticsBootId + 1,
                        MapRevision),
                    harness => SubmissionFailure(
                        harness.Request.RecoveryRequest,
                        new LMCDiagnosticsCommandException(
                            "wrong MapRevision",
                            Response(LMCDiagnosticsDetailCode.ResourceBusy)),
                        LMCSdoSubmissionOutcome.Rejected,
                        DiagnosticsBootId,
                        MapRevision + 1),
                    harness => SubmissionFailure(
                        harness.Request.RecoveryRequest,
                        new InvalidOperationException(
                            "non-command busy rejection"),
                        LMCSdoSubmissionOutcome.Rejected,
                        DiagnosticsBootId,
                        MapRevision)
                };

                for (var index = 0; index < failures.Length; index++)
                {
                    var harness = new Harness(connection);
                    var failure = failures[index](harness);
                    harness.RecoverySubmissionErrorFactory =
                        attempt => failure;

                    var observed = AssertEx.Throws<Exception>(
                        () => Run(
                            harness.Request,
                            harness.CreateOperations()));

                    AssertEx.True(ReferenceEquals(failure, observed));
                    AssertEx.Equal(2, harness.SubmitCount);
                    AssertEx.Equal(0, harness.DelayCount);
                    AssertEx.Equal(1, harness.RecoveryCount);
                    AssertEx.Equal(
                        0,
                        harness.RecoveryScope
                            .RecoveryResourceBusyRejectionCount);
                }
            }
        }

        private static void CancellationDuringDelayPublishes()
        {
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(connection)
                {
                    BusyFailuresBeforeSuccess = 10,
                    CancelOnFirstDelay = cancellation
                };

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations(),
                        cancellation.Token));

                AssertEx.Equal(2, harness.SubmitCount);
                AssertEx.Equal(1, harness.WaitCount);
                AssertEx.Equal(1, harness.DelayCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.Equal(1, harness.RecoveryScope.RecoverySubmitAttemptCount);
                AssertEx.Equal(
                    1,
                    harness.RecoveryScope
                        .RecoveryResourceBusyRejectionCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.TimeoutTicket,
                        harness.RecoveryScope.TimeoutTicket));
                AssertEx.Equal(null, harness.RecoveryScope.RecoveryTicket);
            }
        }

        private static void TicketProvenanceIsExact()
        {
            using (var connection = new LMCConnection())
            using (var foreign = new LMCConnection())
            {
                var invalidTickets = new[]
                {
                    Ticket(
                        foreign,
                        Read(timeoutCycles: 1),
                        201),
                    Ticket(
                        connection,
                        Read(timeoutCycles: 1),
                        202,
                        connectionSessionGeneration:
                            connection.SessionGeneration + 1),
                    Ticket(
                        connection,
                        Read(timeoutCycles: 1),
                        203,
                        diagnosticsBootId: DiagnosticsBootId + 1),
                    Ticket(
                        connection,
                        Read(timeoutCycles: 1),
                        204,
                        mapRevision: MapRevision + 1),
                    Ticket(
                        connection,
                        Read(timeoutCycles: 1),
                        205,
                        expectedResultValueType: LMCSignalValueType.UInt8),
                    Ticket(
                        connection,
                        Read(timeoutCycles: 1),
                        206,
                        expectedResultLength: 2)
                };

                for (var index = 0; index < invalidTickets.Length; index++)
                {
                    var harness = new Harness(connection)
                    {
                        TimeoutTicketOverride = invalidTickets[index]
                    };

                    AssertEx.Throws<InvalidOperationException>(
                        () => Run(
                            harness.Request,
                            harness.CreateOperations()));

                    AssertEx.Equal(1, harness.SubmitCount);
                    AssertEx.Equal(0, harness.WaitCount);
                    AssertEx.Equal(1, harness.RecoveryCount);
                    AssertEx.True(
                        ReferenceEquals(
                            invalidTickets[index],
                            harness.RecoveryScope.TimeoutTicket));
                }

                var reused = new Harness(connection);
                reused.RecoveryTicketOverride = reused.TimeoutTicket;

                var reusedError = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        reused.Request,
                        reused.CreateOperations()));

                AssertEx.Contains("reused", reusedError.Message);
                AssertEx.Equal(2, reused.SubmitCount);
                AssertEx.Equal(1, reused.WaitCount);
                AssertEx.Equal(1, reused.RecoveryCount);
                AssertEx.True(
                    ReferenceEquals(
                        reused.TimeoutTicket,
                        reused.RecoveryScope.RecoveryTicket));
            }
        }

        private static void TimeoutTerminalIsExact()
        {
            using (var connection = new LMCConnection())
            {
                var invalidStatusFactories = new Func<LMCOperationTicket, LMCOperationStatus>[]
                {
                    ticket => null,
                    ticket => TimeoutStatus(
                        ticket,
                        state: LMCOperationState.Failed),
                    ticket => TimeoutStatus(
                        ticket,
                        outcome: LMCOperationOutcome.Failed),
                    ticket => TimeoutStatus(ticket, operationErrorId: 1),
                    ticket => TimeoutStatus(ticket, operationDetail: 0),
                    ticket => TimeoutStatus(
                        ticket,
                        resultLength: 1,
                        resultValueType: LMCSignalValueType.Int8,
                        resultData: new byte[] { 8 }),
                    ticket => TimeoutStatus(ticket, ticketId: ticket.TicketId + 1),
                    ticket => TimeoutStatus(
                        ticket,
                        diagnosticsBootId: ticket.DiagnosticsBootId + 1),
                    ticket => TimeoutStatus(
                        ticket,
                        submitCycle: ticket.QueuedCycle + 1),
                    ticket => TimeoutStatus(
                        ticket,
                        completionCycle: ticket.QueuedCycle),
                    ticket => TimeoutStatus(
                        ticket,
                        operationKind: LMCOperationKind.PIWrite),
                    ticket => TimeoutStatus(
                        ticket,
                        response: Response(
                            LMCDiagnosticsDetailCode.ResourceBusy))
                };

                for (var index = 0;
                    index < invalidStatusFactories.Length;
                    index++)
                {
                    var harness = new Harness(connection);
                    harness.TimeoutStatusOverrideSet = true;
                    harness.TimeoutStatusOverride =
                        invalidStatusFactories[index](harness.TimeoutTicket);

                    AssertEx.Throws<InvalidOperationException>(
                        () => Run(
                            harness.Request,
                            harness.CreateOperations()));

                    AssertEx.Equal(1, harness.SubmitCount);
                    AssertEx.Equal(1, harness.WaitCount);
                    AssertEx.Equal(0, harness.DelayCount);
                    AssertEx.Equal(1, harness.RecoveryCount);
                    AssertEx.True(
                        ReferenceEquals(
                            harness.TimeoutTicket,
                            harness.RecoveryScope.TimeoutTicket));
                }
            }
        }

        private static void RecoveryTerminalIsExact()
        {
            using (var connection = new LMCConnection())
            {
                var invalidStatusFactories = new Func<LMCOperationTicket, LMCOperationStatus>[]
                {
                    ticket => null,
                    ticket => RecoveryStatus(
                        ticket,
                        state: LMCOperationState.Failed,
                        outcome: LMCOperationOutcome.Failed),
                    ticket => RecoveryStatus(ticket, operationErrorId: 1),
                    ticket => RecoveryStatus(ticket, operationDetail: 1),
                    ticket => RecoveryStatus(ticket, resultLength: 2),
                    ticket => RecoveryStatus(
                        ticket,
                        resultValueType: LMCSignalValueType.UInt8),
                    ticket => RecoveryStatus(
                        ticket,
                        resultData: new byte[] { 9 }),
                    ticket => RecoveryStatus(ticket, ticketId: ticket.TicketId + 1),
                    ticket => RecoveryStatus(
                        ticket,
                        diagnosticsBootId: ticket.DiagnosticsBootId + 1),
                    ticket => RecoveryStatus(
                        ticket,
                        submitCycle: ticket.QueuedCycle + 1),
                    ticket => RecoveryStatus(
                        ticket,
                        operationKind: LMCOperationKind.PIWrite),
                    ticket => RecoveryStatus(
                        ticket,
                        response: Response(
                            LMCDiagnosticsDetailCode.ResourceBusy))
                };

                for (var index = 0;
                    index < invalidStatusFactories.Length;
                    index++)
                {
                    var harness = new Harness(connection);
                    harness.RecoveryStatusOverrideSet = true;
                    harness.RecoveryStatusOverride =
                        invalidStatusFactories[index](harness.RecoveryTicket);

                    AssertEx.Throws<InvalidOperationException>(
                        () => Run(
                            harness.Request,
                            harness.CreateOperations()));

                    AssertEx.Equal(2, harness.SubmitCount);
                    AssertEx.Equal(2, harness.WaitCount);
                    AssertEx.Equal(0, harness.DelayCount);
                    AssertEx.Equal(1, harness.RecoveryCount);
                    AssertEx.True(
                        ReferenceEquals(
                            harness.TimeoutTicket,
                            harness.RecoveryScope.TimeoutTicket));
                    AssertEx.True(
                        ReferenceEquals(
                            harness.RecoveryTicket,
                            harness.RecoveryScope.RecoveryTicket));
                }
            }
        }

        private static void ExpectedValueIsImmutable()
        {
            using (var connection = new LMCConnection())
            {
                var source = new byte[] { 8 };
                var request = Request(
                    connection,
                    expectedRecoveryData: source);
                source[0] = 9;
                var copy = request.ExpectedRecoveryData;
                copy[0] = 7;
                var harness = new Harness(connection, request);

                var result = Run(request, harness.CreateOperations());

                AssertEx.SequenceEqual(
                    new byte[] { 8 },
                    request.ExpectedRecoveryData);
                AssertEx.SequenceEqual(
                    new byte[] { 8 },
                    result.RecoveryTerminalStatus.ResultData);
            }
        }

        private static void RecoveryPublicationFailureAggregates()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var primary = new InvalidOperationException(
                    "ambiguous recovery submission");
                var publication = new ApplicationException(
                    "scope publication failed");
                harness.RecoverySubmissionErrorFactory = attempt => primary;
                harness.RecoveryPublicationError = publication;

                var observed = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        harness.Request,
                        harness.CreateOperations()));

                AssertEx.Contains("publication failed", observed.Message);
                var aggregate = observed.InnerException as AggregateException;
                AssertEx.NotNull(aggregate);
                AssertEx.Equal(2, aggregate.InnerExceptions.Count);
                AssertEx.True(
                    ReferenceEquals(primary, aggregate.InnerExceptions[0]));
                AssertEx.True(
                    ReferenceEquals(publication, aggregate.InnerExceptions[1]));
                AssertEx.Equal(1, harness.RecoveryCount);
            }
        }

        private static D5SdoTimeoutQualificationResult Run(
            D5SdoTimeoutQualificationRequest request,
            D5SdoTimeoutQualificationOperations operations,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return D5SdoTimeoutQualificationOrchestrator.RunAsync(
                    request,
                    operations,
                    cancellationToken)
                .GetAwaiter()
                .GetResult();
        }

        private static D5SdoTimeoutQualificationRequest Request(
            LMCConnection connection,
            LMCSdoRequest timeoutRequest = null,
            LMCSdoRequest recoveryRequest = null,
            LMCDiagnosticCapabilities capabilities = null,
            byte[] expectedRecoveryData = null)
        {
            return new D5SdoTimeoutQualificationRequest(
                connection,
                capabilities ?? Capabilities(connection),
                timeoutRequest ?? Read(timeoutCycles: 1),
                recoveryRequest ?? Read(timeoutCycles: 100),
                expectedRecoveryData ?? ExpectedRecoveryData);
        }

        private static LMCSdoRequest Read(
            ushort slaveReference = 1,
            ushort objectIndex = 0x6061,
            byte subIndex = 0,
            LMCSignalValueType valueType = LMCSignalValueType.Int8,
            ushort dataLength = 1,
            uint timeoutCycles = 100)
        {
            return LMCSdoRequest.CreateRead(
                slaveReference,
                objectIndex,
                subIndex,
                valueType,
                dataLength,
                timeoutCycles);
        }

        private static LMCDiagnosticCapability RequiredCapabilities
        {
            get
            {
                return LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline;
            }
        }

        private static LMCDiagnosticCapabilities Capabilities(
            LMCConnection connection,
            LMCDiagnosticCapability capabilities =
                LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline,
            ushort maxSdoDataBytes = 4,
            uint diagnosticsBootId = DiagnosticsBootId,
            uint mapRevision = MapRevision)
        {
            return new LMCDiagnosticCapabilities(
                Response(LMCDiagnosticsDetailCode.None),
                connection.SessionGeneration,
                1,
                (uint)capabilities,
                mapRevision,
                0,
                0,
                0,
                0,
                0,
                1000,
                1320,
                2040,
                1280,
                80,
                16,
                0,
                maxSdoDataBytes,
                diagnosticsBootId).BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration,
                    1);
        }

        private static LMCOperationTicket Ticket(
            LMCConnection connection,
            LMCSdoRequest request,
            uint ticketId,
            uint? queuedCycle = null,
            uint diagnosticsBootId = DiagnosticsBootId,
            uint mapRevision = MapRevision,
            long? connectionSessionGeneration = null,
            ushort? expectedResultLength = null,
            LMCSignalValueType? expectedResultValueType = null)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                queuedCycle ?? ticketId + 10,
                diagnosticsBootId,
                mapRevision,
                connectionSessionGeneration ?? connection.SessionGeneration,
                connection.Diagnostics,
                true,
                expectedResultLength ?? request.DataLength,
                expectedResultValueType ?? request.ValueType,
                false,
                0,
                request);
        }

        private static LMCOperationStatus TimeoutStatus(
            LMCOperationTicket ticket,
            LMCOperationState state = LMCOperationState.Expired,
            LMCOperationOutcome outcome = LMCOperationOutcome.TimedOut,
            uint? ticketId = null,
            LMCOperationKind operationKind = LMCOperationKind.SDORead,
            uint? submitCycle = null,
            uint? completionCycle = null,
            short operationErrorId = 0,
            uint operationDetail =
                D5SdoTimeoutQualificationOrchestrator
                    .EtherCatSdoTimeoutDetail,
            uint resultLength = 0,
            LMCSignalValueType resultValueType = LMCSignalValueType.Invalid,
            byte[] resultData = null,
            uint? diagnosticsBootId = null,
            LMCDiagnosticsResponse response = null)
        {
            return new LMCOperationStatus(
                response ?? Response(LMCDiagnosticsDetailCode.None),
                ticketId ?? ticket.TicketId,
                operationKind,
                state,
                submitCycle ?? ticket.QueuedCycle,
                completionCycle ?? ticket.QueuedCycle + 1,
                outcome,
                operationErrorId,
                operationDetail,
                resultLength,
                resultValueType,
                resultData ?? new byte[0],
                diagnosticsBootId ?? ticket.DiagnosticsBootId);
        }

        private static LMCOperationStatus RecoveryStatus(
            LMCOperationTicket ticket,
            LMCOperationState state = LMCOperationState.Completed,
            LMCOperationOutcome outcome = LMCOperationOutcome.Success,
            uint? ticketId = null,
            LMCOperationKind operationKind = LMCOperationKind.SDORead,
            uint? submitCycle = null,
            uint? completionCycle = null,
            short operationErrorId = 0,
            uint operationDetail = 0,
            uint resultLength = 1,
            LMCSignalValueType resultValueType = LMCSignalValueType.Int8,
            byte[] resultData = null,
            uint? diagnosticsBootId = null,
            LMCDiagnosticsResponse response = null)
        {
            return new LMCOperationStatus(
                response ?? Response(LMCDiagnosticsDetailCode.None),
                ticketId ?? ticket.TicketId,
                operationKind,
                state,
                submitCycle ?? ticket.QueuedCycle,
                completionCycle ?? ticket.QueuedCycle + 10,
                outcome,
                operationErrorId,
                operationDetail,
                resultLength,
                resultValueType,
                resultData ?? ExpectedRecoveryData,
                diagnosticsBootId ?? ticket.DiagnosticsBootId);
        }

        private static LMCDiagnosticsCommandException ExactBusy(
            LMCSdoRequest request)
        {
            return (LMCDiagnosticsCommandException)SubmissionFailure(
                request,
                new LMCDiagnosticsCommandException(
                    "exact ResourceBusy rejection",
                    Response(LMCDiagnosticsDetailCode.ResourceBusy)),
                LMCSdoSubmissionOutcome.Rejected,
                DiagnosticsBootId,
                MapRevision);
        }

        private static TException SubmissionFailure<TException>(
            LMCSdoRequest request,
            TException error,
            LMCSdoSubmissionOutcome outcome,
            uint diagnosticsBootId,
            uint mapRevision)
            where TException : Exception
        {
            LMCSdoSubmissionFailureContext.Attach(
                error,
                new LMCSdoSubmissionFailureContext(
                    request,
                    LMCSdoSubmissionPhase.Submission,
                    outcome,
                    diagnosticsBootId,
                    mapRevision,
                    null));
            return error;
        }

        private static InvalidOperationException AcceptedFailure(
            LMCSdoRequest request,
            LMCOperationTicket ticket)
        {
            var error = new InvalidOperationException(
                "accepted ticket post-validation failure");
            LMCSdoSubmissionFailureContext.Attach(
                error,
                new LMCSdoSubmissionFailureContext(
                    request,
                    LMCSdoSubmissionPhase.PostSubmissionValidation,
                    LMCSdoSubmissionOutcome.Accepted,
                    DiagnosticsBootId,
                    MapRevision,
                    ticket));
            return error;
        }

        private static LMCDiagnosticsResponse Response(
            LMCDiagnosticsDetailCode detail)
        {
            return new LMCDiagnosticsResponse(
                new LMC_Response
                {
                    IsFrameValid = true,
                    HeaderStatus = 0
                },
                1,
                LMCDiagnosticsResponseFlags.None,
                detail == LMCDiagnosticsDetailCode.None
                    ? (ushort)0
                    : (ushort)1,
                detail == LMCDiagnosticsDetailCode.None
                    ? (short)0
                    : (short)-32000,
                1,
                (uint)detail);
        }

        private sealed class Harness
        {
            internal Harness(
                LMCConnection connection,
                D5SdoTimeoutQualificationRequest request = null)
            {
                Connection = connection;
                Request = request ?? D5SdoTimeoutQualificationOrchestratorTests
                    .Request(connection);
                TimeoutTicket = Ticket(
                    connection,
                    Request.TimeoutRequest,
                    101);
                RecoveryTicket = Ticket(
                    connection,
                    Request.RecoveryRequest,
                    102);
                Events = new List<string>();
                DelayValues = new List<int>();
            }

            internal LMCConnection Connection { get; private set; }
            internal D5SdoTimeoutQualificationRequest Request
            {
                get;
                private set;
            }

            internal LMCOperationTicket TimeoutTicket { get; private set; }
            internal LMCOperationTicket RecoveryTicket { get; private set; }
            internal LMCOperationTicket TimeoutTicketOverride { get; set; }
            internal LMCOperationTicket RecoveryTicketOverride { get; set; }
            internal LMCOperationStatus TimeoutStatusOverride { get; set; }
            internal LMCOperationStatus RecoveryStatusOverride { get; set; }
            internal bool TimeoutStatusOverrideSet { get; set; }
            internal bool RecoveryStatusOverrideSet { get; set; }
            internal Exception TimeoutSubmitError { get; set; }
            internal Func<int, Exception> RecoverySubmissionErrorFactory
            {
                get;
                set;
            }

            internal int BusyFailuresBeforeSuccess { get; set; }
            internal CancellationTokenSource CancelOnFirstDelay { get; set; }
            internal Exception RecoveryPublicationError { get; set; }
            internal Exception LastBusyException { get; private set; }
            internal List<string> Events { get; private set; }
            internal List<int> DelayValues { get; private set; }
            internal int SubmitCount { get; private set; }
            internal int WaitCount { get; private set; }
            internal int DelayCount { get; private set; }
            internal int RecoveryCount { get; private set; }
            internal D5SdoTimeoutRecoveryScope RecoveryScope
            {
                get;
                private set;
            }

            internal D5SdoTimeoutQualificationOperations CreateOperations()
            {
                return new D5SdoTimeoutQualificationOperations
                {
                    SubmitAsync = (request, cancellationToken) =>
                    {
                        SubmitCount++;
                        if (SubmitCount == 1)
                        {
                            Events.Add("SubmitTimeout");
                            if (!ReferenceEquals(
                                    Request.TimeoutRequest,
                                    request))
                            {
                                throw new InvalidOperationException(
                                    "The exact timeout request instance was not submitted.");
                            }

                            if (TimeoutSubmitError != null)
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    TimeoutSubmitError);
                            }

                            return Task.FromResult(
                                TimeoutTicketOverride ?? TimeoutTicket);
                        }

                        var recoveryAttempt = SubmitCount - 1;
                        Events.Add("SubmitRecovery" + recoveryAttempt);
                        if (!ReferenceEquals(
                                Request.RecoveryRequest,
                                request))
                        {
                            throw new InvalidOperationException(
                                "The exact recovery request instance was not reused.");
                        }

                        Exception error = null;
                        if (RecoverySubmissionErrorFactory != null)
                        {
                            error = RecoverySubmissionErrorFactory(
                                recoveryAttempt);
                        }
                        else if (recoveryAttempt
                            <= BusyFailuresBeforeSuccess)
                        {
                            error = ExactBusy(Request.RecoveryRequest);
                            LastBusyException = error;
                        }

                        if (error != null)
                        {
                            return TaskFromException<LMCOperationTicket>(error);
                        }

                        return Task.FromResult(
                            RecoveryTicketOverride ?? RecoveryTicket);
                    },
                    WaitForTerminalAsync = (ticket, cancellationToken) =>
                    {
                        WaitCount++;
                        if (WaitCount == 1)
                        {
                            Events.Add("WaitTimeout");
                            var expected =
                                TimeoutTicketOverride ?? TimeoutTicket;
                            if (!ReferenceEquals(expected, ticket))
                            {
                                throw new InvalidOperationException(
                                    "A foreign timeout ticket was awaited.");
                            }

                            return Task.FromResult(
                                TimeoutStatusOverrideSet
                                    ? TimeoutStatusOverride
                                    : TimeoutStatus(ticket));
                        }

                        if (WaitCount == 2)
                        {
                            Events.Add("WaitRecovery");
                            var expected =
                                RecoveryTicketOverride ?? RecoveryTicket;
                            if (!ReferenceEquals(expected, ticket))
                            {
                                throw new InvalidOperationException(
                                    "A foreign recovery ticket was awaited.");
                            }

                            return Task.FromResult(
                                RecoveryStatusOverrideSet
                                    ? RecoveryStatusOverride
                                    : RecoveryStatus(ticket));
                        }

                        throw new InvalidOperationException(
                            "An unexpected terminal wait was attempted.");
                    },
                    DelayAsync = (milliseconds, cancellationToken) =>
                    {
                        DelayCount++;
                        DelayValues.Add(milliseconds);
                        Events.Add("Delay" + milliseconds);
                        if (CancelOnFirstDelay != null
                            && DelayCount == 1)
                        {
                            CancelOnFirstDelay.Cancel();
                        }

                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            return Task.FromResult(0);
                        }
                        catch (Exception error)
                        {
                            return TaskFromException(error);
                        }
                    },
                    RecoveryRequired = (scope, error) =>
                    {
                        RecoveryCount++;
                        RecoveryScope = scope;
                        if (RecoveryPublicationError != null)
                        {
                            throw RecoveryPublicationError;
                        }
                    }
                };
            }

            private static Task<T> TaskFromException<T>(Exception error)
            {
                var completion = new TaskCompletionSource<T>();
                completion.SetException(error);
                return completion.Task;
            }

            private static Task TaskFromException(Exception error)
            {
                var completion = new TaskCompletionSource<int>();
                completion.SetException(error);
                return completion.Task;
            }
        }
    }
}

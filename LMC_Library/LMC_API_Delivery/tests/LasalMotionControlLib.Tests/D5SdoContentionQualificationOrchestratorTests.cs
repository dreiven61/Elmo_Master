using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoContentionQualificationOrchestratorTests
    {
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint MapRevision = 0x957F101Eu;
        private static readonly byte[] ExpectedResult = { 8 };
        private static readonly LMCSdoRequest KnownValidReadRequest =
            LMCSdoRequest.CreateRead(
                1,
                0x6061,
                0,
                LMCSignalValueType.Int8,
                1,
                100);

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5Contention.PreflightExactAndZeroOperation",
                PreflightExactAndZeroOperation);
            tests.Add(
                "Qualification.D5Contention.PreCanceledPublishesZeroOperationScope",
                PreCanceledPublishesZeroOperationScope);
            tests.Add(
                "Qualification.D5Contention.ExactBusyThenRecoveryRead",
                ExactBusyThenRecoveryRead);
            tests.Add(
                "Qualification.D5Contention.UnexpectedSecondPreservedBlocksThird",
                UnexpectedSecondPreservedBlocksThird);
            tests.Add(
                "Qualification.D5Contention.AmbiguousSecondNeverResends",
                AmbiguousSecondNeverResends);
            tests.Add(
                "Qualification.D5Contention.ExactBusyEvidenceRequired",
                ExactBusyEvidenceRequired);
            tests.Add(
                "Qualification.D5Contention.AcceptedFailureTicketsPreserved",
                AcceptedFailureTicketsPreserved);
            tests.Add(
                "Qualification.D5Contention.CancellationPreservesFirst",
                CancellationPreservesFirst);
            tests.Add(
                "Qualification.D5Contention.TicketProvenanceExact",
                TicketProvenanceExact);
            tests.Add(
                "Qualification.D5Contention.TerminalIdentityTypeLengthValueExact",
                TerminalIdentityTypeLengthValueExact);
            tests.Add(
                "Qualification.D5Contention.RecoveryPublicationFailureAggregates",
                RecoveryPublicationFailureAggregates);
            tests.Add(
                "Qualification.D5Contention.ExpectedValueImmutable",
                ExpectedValueImmutable);
        }

        private static void PreflightExactAndZeroOperation()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var operations = harness.CreateOperations();
                var valid = Request(connection);
                var invalid = new[]
                {
                    Request(
                        connection,
                        ReadRequest(slaveReference: 5)),
                    Request(
                        connection,
                        ReadRequest(objectIndex: 0x6060)),
                    Request(
                        connection,
                        ReadRequest(subIndex: 1)),
                    Request(
                        connection,
                        ReadRequest(
                            valueType: LMCSignalValueType.UInt8)),
                    Request(
                        connection,
                        LMCSdoRequest.CreateRead(
                            1,
                            0x6061,
                            0,
                            LMCSignalValueType.Int16,
                            2,
                            100)),
                    Request(
                        connection,
                        ReadRequest(timeoutCycles: 60001)),
                    Request(
                        connection,
                        ReadRequest(),
                        Capabilities(
                            connection,
                            LMCDiagnosticCapability.SDORead,
                            4,
                            DiagnosticsBootId,
                            MapRevision)),
                    Request(
                        connection,
                        ReadRequest(),
                        Capabilities(
                            connection,
                            RequiredCapabilities,
                            2,
                            DiagnosticsBootId,
                            MapRevision)),
                    Request(
                        connection,
                        ReadRequest(),
                        Capabilities(
                            connection,
                            RequiredCapabilities,
                            4,
                            0,
                            MapRevision)),
                    Request(
                        connection,
                        ReadRequest(),
                        Capabilities(
                            connection,
                            RequiredCapabilities,
                            4,
                            DiagnosticsBootId,
                            0)),
                    new D5SdoContentionQualificationRequest(
                        connection,
                        Capabilities(connection),
                        ReadRequest(),
                        new byte[] { 8, 9 }),
                    new D5SdoContentionQualificationRequest(
                        connection,
                        Capabilities(connection),
                        LMCSdoRequest.CreateWrite(
                            1,
                            0x6061,
                            0,
                            LMCSignalValueType.Int32,
                            new byte[] { 8, 0, 0, 0 },
                            100),
                        new byte[] { 8, 0, 0, 0 })
                };

                AssertEx.Throws<ArgumentNullException>(
                    () => Run(null, operations));
                for (var index = 0; index < invalid.Length; index++)
                {
                    AssertEx.Throws<Exception>(
                        () => Run(invalid[index], operations));
                }

                AssertEx.Throws<ArgumentNullException>(
                    () => Run(valid, null));
                AssertEx.Throws<ArgumentException>(
                    () => Run(
                        valid,
                        new D5SdoContentionQualificationOperations
                        {
                            SubmitAsync = operations.SubmitAsync,
                            RecoveryRequired = operations.RecoveryRequired
                        }));

                AssertEx.Equal(0, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(0, harness.RecoveryCount);
            }
        }

        private static void PreCanceledPublishesZeroOperationScope()
        {
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(connection);
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations(),
                        cancellation.Token));

                AssertEx.Equal(0, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.Equal(
                    "PREFLIGHT_COMPLETE",
                    harness.RecoveryScope.Stage);
                AssertEx.False(harness.RecoveryScope.HasAcceptedTickets);
            }
        }

        private static void ExactBusyThenRecoveryRead()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var result = Run(
                    Request(connection),
                    harness.CreateOperations());

                AssertEx.Equal(
                    "Submit1,Submit2,Wait1,Submit3,Wait3",
                    string.Join(",", harness.Events));
                AssertEx.Equal(3, harness.SubmitCount);
                AssertEx.Equal(2, harness.WaitCount);
                AssertEx.Equal(0, harness.RecoveryCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.BusyException,
                        result.SecondResourceBusyException));
                AssertEx.True(
                    ReferenceEquals(
                        harness.FirstTicket,
                        result.RecoveryScope.FirstTicket));
                AssertEx.True(
                    ReferenceEquals(
                        harness.ThirdTicket,
                        result.RecoveryScope.ThirdTicket));
                AssertEx.Equal(null, result.RecoveryScope.UnexpectedSecondTicket);
                AssertEx.True(
                    result.RecoveryScope.SecondExactResourceBusyConfirmed);
                AssertEx.False(
                    result.RecoveryScope.HasUncertainSubmissionOutcome);
                AssertEx.Equal("COMPLETE", result.RecoveryScope.Stage);
                AssertEx.SequenceEqual(
                    ExpectedResult,
                    result.FirstTerminalStatus.ResultData);
                AssertEx.SequenceEqual(
                    result.FirstTerminalStatus.ResultData,
                    result.ThirdTerminalStatus.ResultData);
            }
        }

        private static void UnexpectedSecondPreservedBlocksThird()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                harness.AcceptSecond = true;

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations()));

                AssertEx.Contains("unexpectedly accepted", error.Message);
                AssertEx.Equal("Submit1,Submit2", string.Join(",", harness.Events));
                AssertEx.Equal(2, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.SecondTicket,
                        harness.RecoveryScope.UnexpectedSecondTicket));
                AssertEx.True(harness.RecoveryScope.HasAcceptedTickets);
                AssertEx.False(harness.RecoveryScope.ThirdSubmitAttempted);
            }
        }

        private static void AmbiguousSecondNeverResends()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var ambiguous = SubmissionFailure(
                    harness.Request,
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    DiagnosticsBootId,
                    MapRevision);
                harness.SecondError = ambiguous;

                var observed = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations()));

                AssertEx.True(ReferenceEquals(ambiguous, observed));
                AssertEx.Equal(2, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.True(
                    harness.RecoveryScope.SecondSubmissionOutcomeUncertain);
                AssertEx.True(
                    harness.RecoveryScope.HasUncertainSubmissionOutcome);
                AssertEx.False(
                    harness.RecoveryScope.SecondExactResourceBusyConfirmed);
                AssertEx.False(harness.RecoveryScope.ThirdSubmitAttempted);
            }
        }

        private static void ExactBusyEvidenceRequired()
        {
            using (var connection = new LMCConnection())
            {
                var failures = new Func<Harness, Exception>[]
                {
                    harness => new LMCDiagnosticsCommandException(
                        "busy without outcome context",
                        Response(LMCDiagnosticsDetailCode.ResourceBusy)),
                    harness => SubmissionFailure(
                        harness.Request,
                        LMCDiagnosticsDetailCode.TypeMismatch,
                        LMCSdoSubmissionOutcome.Rejected,
                        DiagnosticsBootId,
                        MapRevision),
                    harness => SubmissionFailure(
                        harness.Request,
                        LMCDiagnosticsDetailCode.ResourceBusy,
                        LMCSdoSubmissionOutcome.Rejected,
                        DiagnosticsBootId,
                        MapRevision + 1)
                };

                for (var index = 0; index < failures.Length; index++)
                {
                    var harness = new Harness(connection);
                    var expected = failures[index](harness);
                    harness.SecondError = expected;

                    var observed = AssertEx.Throws<Exception>(
                        () => Run(
                            Request(connection),
                            harness.CreateOperations()));

                    AssertEx.True(ReferenceEquals(expected, observed));
                    AssertEx.Equal(2, harness.SubmitCount);
                    AssertEx.Equal(0, harness.WaitCount);
                    AssertEx.Equal(1, harness.RecoveryCount);
                    AssertEx.False(
                        harness.RecoveryScope
                            .SecondExactResourceBusyConfirmed);
                    AssertEx.False(harness.RecoveryScope.ThirdSubmitAttempted);
                }
            }
        }

        private static void AcceptedFailureTicketsPreserved()
        {
            using (var connection = new LMCConnection())
            {
                var firstHarness = new Harness(connection);
                var firstError = AcceptedFailure(
                    firstHarness.Request,
                    firstHarness.FirstTicket);
                firstHarness.FirstError = firstError;

                var observedFirst = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        firstHarness.CreateOperations()));
                AssertEx.True(ReferenceEquals(firstError, observedFirst));
                AssertEx.Equal(1, firstHarness.SubmitCount);
                AssertEx.Equal(0, firstHarness.WaitCount);
                AssertEx.True(
                    ReferenceEquals(
                        firstHarness.FirstTicket,
                        firstHarness.RecoveryScope.FirstTicket));

                var thirdHarness = new Harness(connection);
                var thirdError = AcceptedFailure(
                    thirdHarness.Request,
                    thirdHarness.ThirdTicket);
                thirdHarness.ThirdError = thirdError;

                var observedThird = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        thirdHarness.CreateOperations()));
                AssertEx.True(ReferenceEquals(thirdError, observedThird));
                AssertEx.Equal(3, thirdHarness.SubmitCount);
                AssertEx.Equal(1, thirdHarness.WaitCount);
                AssertEx.True(
                    ReferenceEquals(
                        thirdHarness.FirstTicket,
                        thirdHarness.RecoveryScope.FirstTicket));
                AssertEx.True(
                    ReferenceEquals(
                        thirdHarness.ThirdTicket,
                        thirdHarness.RecoveryScope.ThirdTicket));
            }
        }

        private static void CancellationPreservesFirst()
        {
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(connection);
                harness.CancelAfterFirstSubmit = cancellation;

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations(),
                        cancellation.Token));

                AssertEx.Equal(1, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.FirstTicket,
                        harness.RecoveryScope.FirstTicket));
                AssertEx.False(harness.RecoveryScope.SecondSubmitAttempted);
            }
        }

        private static void TicketProvenanceExact()
        {
            using (var connection = new LMCConnection())
            using (var foreign = new LMCConnection())
            {
                var harness = new Harness(connection);
                harness.FirstTicketOverride = Ticket(
                    foreign,
                    harness.Request,
                    91);

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations()));

                AssertEx.Contains("provenance", error.Message);
                AssertEx.Equal(1, harness.SubmitCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.FirstTicketOverride,
                        harness.RecoveryScope.FirstTicket));
            }
        }

        private static void TerminalIdentityTypeLengthValueExact()
        {
            using (var connection = new LMCConnection())
            {
                var invalidStatuses = new Func<Harness, LMCOperationStatus>[]
                {
                    harness => Status(
                        harness.FirstTicket,
                        resultValueType: LMCSignalValueType.UInt8),
                    harness => Status(
                        harness.FirstTicket,
                        resultLength: 2),
                    harness => Status(
                        harness.FirstTicket,
                        resultData: new byte[] { 9 }),
                    harness => Status(
                        harness.FirstTicket,
                        ticketId: harness.FirstTicket.TicketId + 1),
                    harness => Status(
                        harness.FirstTicket,
                        diagnosticsBootId: DiagnosticsBootId + 1),
                    harness => Status(
                        harness.FirstTicket,
                        state: LMCOperationState.Failed,
                        outcome: LMCOperationOutcome.Failed,
                        operationErrorId: -1,
                        operationDetail: 1)
                };

                for (var index = 0; index < invalidStatuses.Length; index++)
                {
                    var harness = new Harness(connection);
                    harness.FirstStatusOverride =
                        invalidStatuses[index](harness);

                    var error = AssertEx.Throws<InvalidOperationException>(
                        () => Run(
                            Request(connection),
                            harness.CreateOperations()));

                    AssertEx.Contains("terminal status", error.Message);
                    AssertEx.Equal(2, harness.SubmitCount);
                    AssertEx.Equal(1, harness.WaitCount);
                    AssertEx.Equal(1, harness.RecoveryCount);
                    AssertEx.False(harness.RecoveryScope.ThirdSubmitAttempted);
                    AssertEx.NotNull(harness.RecoveryScope.FirstTicket);
                }

                var thirdHarness = new Harness(connection);
                thirdHarness.ThirdStatusOverride = Status(
                    thirdHarness.ThirdTicket,
                    resultData: new byte[] { 7 });
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        thirdHarness.CreateOperations()));
                AssertEx.Equal(3, thirdHarness.SubmitCount);
                AssertEx.Equal(2, thirdHarness.WaitCount);
                AssertEx.NotNull(thirdHarness.RecoveryScope.ThirdTicket);
            }
        }

        private static void RecoveryPublicationFailureAggregates()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var primary = new InvalidOperationException("primary");
                var publication = new InvalidOperationException("publication");
                harness.SecondError = primary;
                harness.RecoveryError = publication;

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations()));

                AssertEx.Contains("publication failed", error.Message);
                var aggregate = error.InnerException as AggregateException;
                AssertEx.NotNull(aggregate);
                AssertEx.True(
                    ReferenceEquals(primary, aggregate.InnerExceptions[0]));
                AssertEx.True(
                    ReferenceEquals(
                        publication,
                        aggregate.InnerExceptions[1]));
                AssertEx.Equal(2, harness.SubmitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
            }
        }

        private static void ExpectedValueImmutable()
        {
            using (var connection = new LMCConnection())
            {
                var source = new byte[] { 8 };
                var request = new D5SdoContentionQualificationRequest(
                    connection,
                    Capabilities(connection),
                    KnownValidReadRequest,
                    source);
                source[0] = 9;
                var copy = request.ExpectedResultData;
                copy[0] = 7;

                var harness = new Harness(connection);
                var result = Run(request, harness.CreateOperations());

                AssertEx.SequenceEqual(
                    new byte[] { 8 },
                    request.ExpectedResultData);
                AssertEx.SequenceEqual(
                    new byte[] { 8 },
                    result.ThirdTerminalStatus.ResultData);
            }
        }

        private static D5SdoContentionQualificationResult Run(
            D5SdoContentionQualificationRequest request,
            D5SdoContentionQualificationOperations operations,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return D5SdoContentionQualificationOrchestrator.RunAsync(
                    request,
                    operations,
                    cancellationToken)
                .GetAwaiter()
                .GetResult();
        }

        private static D5SdoContentionQualificationRequest Request(
            LMCConnection connection,
            LMCSdoRequest readRequest = null,
            LMCDiagnosticCapabilities capabilities = null)
        {
            return new D5SdoContentionQualificationRequest(
                connection,
                capabilities ?? Capabilities(connection),
                readRequest ?? KnownValidReadRequest,
                ExpectedResult);
        }

        private static LMCSdoRequest ReadRequest(
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
            uint ticketId)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                ticketId + 10,
                DiagnosticsBootId,
                MapRevision,
                connection.SessionGeneration,
                connection.Diagnostics,
                true,
                request.DataLength,
                request.ValueType,
                false,
                0,
                request);
        }

        private static LMCOperationStatus Status(
            LMCOperationTicket ticket,
            LMCOperationState state = LMCOperationState.Completed,
            LMCOperationOutcome outcome = LMCOperationOutcome.Success,
            uint? ticketId = null,
            uint? diagnosticsBootId = null,
            LMCSignalValueType resultValueType = LMCSignalValueType.Int8,
            uint resultLength = 1,
            byte[] resultData = null,
            short operationErrorId = 0,
            uint operationDetail = 0)
        {
            return new LMCOperationStatus(
                Response(LMCDiagnosticsDetailCode.None),
                ticketId ?? ticket.TicketId,
                LMCOperationKind.SDORead,
                state,
                ticket.QueuedCycle,
                ticket.QueuedCycle + 10,
                outcome,
                operationErrorId,
                operationDetail,
                resultLength,
                resultValueType,
                resultData ?? ExpectedResult,
                diagnosticsBootId ?? ticket.DiagnosticsBootId);
        }

        private static LMCDiagnosticsCommandException SubmissionFailure(
            LMCSdoRequest request,
            LMCDiagnosticsDetailCode detail,
            LMCSdoSubmissionOutcome outcome,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var error = new LMCDiagnosticsCommandException(
                "test submission failure",
                Response(detail));
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
            internal Harness(LMCConnection connection)
            {
                Connection = connection;
                Request = KnownValidReadRequest;
                FirstTicket = Ticket(connection, Request, 101);
                SecondTicket = Ticket(connection, Request, 102);
                ThirdTicket = Ticket(connection, Request, 103);
                BusyException = SubmissionFailure(
                    Request,
                    LMCDiagnosticsDetailCode.ResourceBusy,
                    LMCSdoSubmissionOutcome.Rejected,
                    DiagnosticsBootId,
                    MapRevision);
                Events = new List<string>();
            }

            internal LMCConnection Connection { get; private set; }
            internal LMCSdoRequest Request { get; private set; }
            internal LMCOperationTicket FirstTicket { get; private set; }
            internal LMCOperationTicket SecondTicket { get; private set; }
            internal LMCOperationTicket ThirdTicket { get; private set; }
            internal LMCDiagnosticsCommandException BusyException
            {
                get;
                private set;
            }

            internal List<string> Events { get; private set; }
            internal Exception FirstError { get; set; }
            internal Exception SecondError { get; set; }
            internal Exception ThirdError { get; set; }
            internal Exception RecoveryError { get; set; }
            internal bool AcceptSecond { get; set; }
            internal CancellationTokenSource CancelAfterFirstSubmit
            {
                get;
                set;
            }

            internal LMCOperationTicket FirstTicketOverride { get; set; }
            internal LMCOperationStatus FirstStatusOverride { get; set; }
            internal LMCOperationStatus ThirdStatusOverride { get; set; }
            internal int SubmitCount { get; private set; }
            internal int WaitCount { get; private set; }
            internal int RecoveryCount { get; private set; }
            internal D5SdoContentionRecoveryScope RecoveryScope
            {
                get;
                private set;
            }

            internal D5SdoContentionQualificationOperations
                CreateOperations()
            {
                return new D5SdoContentionQualificationOperations
                {
                    SubmitAsync = (request, cancellationToken) =>
                    {
                        SubmitCount++;
                        Events.Add("Submit" + SubmitCount);
                        if (!ReferenceEquals(Request, request))
                        {
                            throw new InvalidOperationException(
                                "The exact immutable request was not reused.");
                        }

                        if (SubmitCount == 1)
                        {
                            if (FirstError != null)
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    FirstError);
                            }

                            if (CancelAfterFirstSubmit != null)
                            {
                                CancelAfterFirstSubmit.Cancel();
                            }

                            return Task.FromResult(
                                FirstTicketOverride ?? FirstTicket);
                        }

                        if (SubmitCount == 2)
                        {
                            if (AcceptSecond)
                            {
                                return Task.FromResult(SecondTicket);
                            }

                            return TaskFromException<LMCOperationTicket>(
                                SecondError ?? BusyException);
                        }

                        if (SubmitCount == 3)
                        {
                            if (ThirdError != null)
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    ThirdError);
                            }

                            return Task.FromResult(ThirdTicket);
                        }

                        throw new InvalidOperationException(
                            "The orchestrator automatically resent an SDO Read.");
                    },
                    WaitForTerminalAsync = (ticket, cancellationToken) =>
                    {
                        WaitCount++;
                        if (ReferenceEquals(ticket, FirstTicket))
                        {
                            Events.Add("Wait1");
                            return Task.FromResult(
                                FirstStatusOverride ?? Status(FirstTicket));
                        }

                        if (ReferenceEquals(ticket, ThirdTicket))
                        {
                            Events.Add("Wait3");
                            return Task.FromResult(
                                ThirdStatusOverride ?? Status(ThirdTicket));
                        }

                        throw new InvalidOperationException(
                            "A foreign ticket was passed to terminal wait.");
                    },
                    RecoveryRequired = (scope, error) =>
                    {
                        RecoveryCount++;
                        RecoveryScope = scope;
                        if (RecoveryError != null)
                        {
                            throw RecoveryError;
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
        }
    }
}

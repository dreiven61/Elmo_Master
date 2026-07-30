using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoQueuedCancelQualificationOrchestratorTests
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
                "Qualification.D5QueuedCancel.PreflightExactAndZeroOperation",
                PreflightExactAndZeroOperation);
            tests.Add(
                "Qualification.D5QueuedCancel.ExactCancelThenRecovery",
                ExactCancelThenRecovery);
            tests.Add(
                "Qualification.D5QueuedCancel.InvalidStateRaceDrainsWithoutRecovery",
                InvalidStateRaceDrainsWithoutRecovery);
            tests.Add(
                "Qualification.D5QueuedCancel.AmbiguousCancelPreservesWithoutRetry",
                AmbiguousCancelPreservesWithoutRetry);
            tests.Add(
                "Qualification.D5QueuedCancel.ExplicitCancelFailureIsNotAmbiguous",
                ExplicitCancelFailureIsNotAmbiguous);
            tests.Add(
                "Qualification.D5QueuedCancel.CancellationPreservesTarget",
                CancellationPreservesTarget);
            tests.Add(
                "Qualification.D5QueuedCancel.CancelledTerminalMustBeExact",
                CancelledTerminalMustBeExact);
            tests.Add(
                "Qualification.D5QueuedCancel.RecoveryIdentityAndValueMustBeExact",
                RecoveryIdentityAndValueMustBeExact);
            tests.Add(
                "Qualification.D5QueuedCancel.AcceptedSubmitFailureTicketPreserved",
                AcceptedSubmitFailureTicketPreserved);
            tests.Add(
                "Qualification.D5QueuedCancel.ExpectedValueImmutable",
                ExpectedValueImmutable);
        }

        private static void PreflightExactAndZeroOperation()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var operations = harness.CreateOperations();
                var invalid = new[]
                {
                    Request(
                        connection,
                        LMCSdoRequest.CreateRead(
                            1,
                            0x6060,
                            0,
                            LMCSignalValueType.Int8,
                            1,
                            100)),
                    Request(
                        connection,
                        LMCSdoRequest.CreateWrite(
                            1,
                            0x6061,
                            0,
                            LMCSignalValueType.Int32,
                            new byte[] { 8, 0, 0, 0 },
                            100)),
                    Request(
                        connection,
                        KnownValidReadRequest,
                        Capabilities(
                            connection,
                            LMCDiagnosticCapability.SDORead)),
                    Request(
                        connection,
                        KnownValidReadRequest,
                        Capabilities(
                            connection,
                            RequiredCapabilities,
                            2)),
                    new D5SdoQueuedCancelQualificationRequest(
                        connection,
                        Capabilities(connection),
                        KnownValidReadRequest,
                        new byte[] { 8, 9 })
                };

                AssertEx.Throws<ArgumentNullException>(
                    () => Run(null, operations));
                for (var index = 0; index < invalid.Length; index++)
                {
                    AssertEx.Throws<Exception>(
                        () => Run(invalid[index], operations));
                }

                AssertEx.Throws<ArgumentNullException>(
                    () => Run(Request(connection), null));
                AssertEx.Throws<ArgumentException>(
                    () => Run(
                        Request(connection),
                        new D5SdoQueuedCancelQualificationOperations
                        {
                            SubmitAsync = operations.SubmitAsync,
                            CancelAsync = operations.CancelAsync,
                            RecoveryRequired = operations.RecoveryRequired
                        }));

                AssertEx.Equal(0, harness.SubmitCount);
                AssertEx.Equal(0, harness.CancelCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(0, harness.RecoveryCount);
            }
        }

        private static void ExactCancelThenRecovery()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var result = Run(
                    Request(connection),
                    harness.CreateOperations());

                AssertEx.Equal(
                    "Submit1,Cancel,Wait1,Submit2,Wait2",
                    string.Join(",", harness.Events));
                AssertEx.Equal(2, harness.SubmitCount);
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(2, harness.WaitCount);
                AssertEx.Equal(0, harness.RecoveryCount);
                AssertEx.Equal(
                    D5SdoQueuedCancelQualificationDisposition.Qualified,
                    result.Disposition);
                AssertEx.True(result.RecoveryScope.CancelAttempted);
                AssertEx.True(result.RecoveryScope.CancelAccepted);
                AssertEx.False(result.RecoveryScope.CancelOutcomeUncertain);
                AssertEx.Equal(
                    LMCOperationState.Cancelled,
                    result.TargetTerminalStatus.State);
                AssertEx.Equal(
                    LMCOperationState.Completed,
                    result.RecoveryTerminalStatus.State);
                AssertEx.SequenceEqual(
                    ExpectedResult,
                    result.RecoveryTerminalStatus.ResultData);
                AssertEx.Equal("COMPLETE", result.RecoveryScope.Stage);
            }
        }

        private static void InvalidStateRaceDrainsWithoutRecovery()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                harness.CancelError = CommandFailure(
                    LMCDiagnosticsDetailCode.InvalidState);
                harness.TargetStatusOverride = CompletedStatus(
                    harness.TargetTicket);

                var result = Run(
                    Request(connection),
                    harness.CreateOperations());

                AssertEx.Equal(
                    "Submit1,Cancel,Wait1",
                    string.Join(",", harness.Events));
                AssertEx.Equal(1, harness.SubmitCount);
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(1, harness.WaitCount);
                AssertEx.Equal(0, harness.RecoveryCount);
                AssertEx.Equal(
                    D5SdoQueuedCancelQualificationDisposition
                        .NotQualifiedRace,
                    result.Disposition);
                AssertEx.True(result.RecoveryScope.CancelInvalidStateRace);
                AssertEx.False(result.RecoveryScope.CancelAccepted);
                AssertEx.False(result.RecoveryScope.RecoverySubmitAttempted);
                AssertEx.True(result.InvalidStateRaceException != null);
                AssertEx.Equal(
                    LMCOperationState.Completed,
                    result.TargetTerminalStatus.State);
                AssertEx.Equal(
                    "NOT_QUALIFIED_RACE",
                    result.RecoveryScope.Stage);
            }
        }

        private static void AmbiguousCancelPreservesWithoutRetry()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var ambiguous = new TimeoutException(
                    "cancel response unavailable");
                harness.CancelError = ambiguous;

                var observed = AssertEx.Throws<TimeoutException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations()));

                AssertEx.True(ReferenceEquals(ambiguous, observed));
                AssertEx.Equal("Submit1,Cancel", string.Join(",", harness.Events));
                AssertEx.Equal(1, harness.SubmitCount);
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.True(harness.RecoveryScope.CancelAttempted);
                AssertEx.True(harness.RecoveryScope.CancelOutcomeUncertain);
                AssertEx.True(
                    ReferenceEquals(
                        harness.TargetTicket,
                        harness.RecoveryScope.TargetTicket));
                AssertEx.False(
                    harness.RecoveryScope.RecoverySubmitAttempted);
            }
        }

        private static void ExplicitCancelFailureIsNotAmbiguous()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var rejected = CommandFailure(
                    LMCDiagnosticsDetailCode.BootIdMismatch);
                harness.CancelError = rejected;

                var observed = AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations()));

                AssertEx.True(ReferenceEquals(rejected, observed));
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.False(harness.RecoveryScope.CancelOutcomeUncertain);
                AssertEx.False(
                    harness.RecoveryScope.RecoverySubmitAttempted);
            }
        }

        private static void CancellationPreservesTarget()
        {
            using (var connection = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(connection);
                harness.CancelAfterTargetSubmit = cancellation;

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations(),
                        cancellation.Token));

                AssertEx.Equal(1, harness.SubmitCount);
                AssertEx.Equal(0, harness.CancelCount);
                AssertEx.Equal(0, harness.WaitCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.TargetTicket,
                        harness.RecoveryScope.TargetTicket));
                AssertEx.False(harness.RecoveryScope.CancelAttempted);
            }
        }

        private static void CancelledTerminalMustBeExact()
        {
            using (var connection = new LMCConnection())
            {
                var invalidStatuses = new Func<Harness, LMCOperationStatus>[]
                {
                    harness => Status(
                        harness.TargetTicket,
                        LMCOperationState.Completed,
                        LMCOperationOutcome.Success,
                        LMCSignalValueType.Int8,
                        1,
                        ExpectedResult),
                    harness => Status(
                        harness.TargetTicket,
                        LMCOperationState.Cancelled,
                        LMCOperationOutcome.Cancelled,
                        LMCSignalValueType.Invalid,
                        0,
                        new byte[0],
                        operationDetail: 1),
                    harness => Status(
                        harness.TargetTicket,
                        LMCOperationState.Cancelled,
                        LMCOperationOutcome.Cancelled,
                        LMCSignalValueType.Int8,
                        1,
                        ExpectedResult)
                };

                for (var index = 0; index < invalidStatuses.Length; index++)
                {
                    var harness = new Harness(connection);
                    harness.TargetStatusOverride = invalidStatuses[index](harness);

                    AssertEx.Throws<InvalidOperationException>(
                        () => Run(
                            Request(connection),
                            harness.CreateOperations()));

                    AssertEx.Equal(1, harness.CancelCount);
                    AssertEx.Equal(1, harness.WaitCount);
                    AssertEx.Equal(1, harness.RecoveryCount);
                    AssertEx.False(
                        harness.RecoveryScope.RecoverySubmitAttempted);
                }
            }
        }

        private static void RecoveryIdentityAndValueMustBeExact()
        {
            using (var connection = new LMCConnection())
            {
                var sameTicket = new Harness(connection);
                sameTicket.RecoveryTicketOverride = sameTicket.TargetTicket;
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        sameTicket.CreateOperations()));
                AssertEx.Equal(2, sameTicket.SubmitCount);
                AssertEx.Equal(1, sameTicket.WaitCount);
                AssertEx.Equal(1, sameTicket.RecoveryCount);

                var wrongValue = new Harness(connection);
                wrongValue.RecoveryStatusOverride = CompletedStatus(
                    wrongValue.RecoveryTicket,
                    new byte[] { 9 });
                AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        wrongValue.CreateOperations()));
                AssertEx.Equal(2, wrongValue.SubmitCount);
                AssertEx.Equal(2, wrongValue.WaitCount);
                AssertEx.Equal(1, wrongValue.RecoveryCount);
            }
        }

        private static void AcceptedSubmitFailureTicketPreserved()
        {
            using (var connection = new LMCConnection())
            {
                var harness = new Harness(connection);
                var acceptedFailure = AcceptedFailure(
                    harness.Request,
                    harness.TargetTicket);
                harness.TargetSubmitError = acceptedFailure;

                var observed = AssertEx.Throws<InvalidOperationException>(
                    () => Run(
                        Request(connection),
                        harness.CreateOperations()));

                AssertEx.True(ReferenceEquals(acceptedFailure, observed));
                AssertEx.Equal(1, harness.SubmitCount);
                AssertEx.Equal(0, harness.CancelCount);
                AssertEx.Equal(1, harness.RecoveryCount);
                AssertEx.True(
                    ReferenceEquals(
                        harness.TargetTicket,
                        harness.RecoveryScope.TargetTicket));
            }
        }

        private static void ExpectedValueImmutable()
        {
            using (var connection = new LMCConnection())
            {
                var source = new byte[] { 8 };
                var request = new D5SdoQueuedCancelQualificationRequest(
                    connection,
                    Capabilities(connection),
                    KnownValidReadRequest,
                    source);
                source[0] = 9;
                var copy = request.CopyExpectedResultData();
                copy[0] = 7;

                var harness = new Harness(connection);
                var result = Run(request, harness.CreateOperations());

                AssertEx.SequenceEqual(
                    new byte[] { 8 },
                    request.CopyExpectedResultData());
                AssertEx.SequenceEqual(
                    new byte[] { 8 },
                    result.RecoveryTerminalStatus.ResultData);
            }
        }

        private static D5SdoQueuedCancelQualificationResult Run(
            D5SdoQueuedCancelQualificationRequest request,
            D5SdoQueuedCancelQualificationOperations operations,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return D5SdoQueuedCancelQualificationOrchestrator.RunAsync(
                    request,
                    operations,
                    cancellationToken)
                .GetAwaiter()
                .GetResult();
        }

        private static D5SdoQueuedCancelQualificationRequest Request(
            LMCConnection connection,
            LMCSdoRequest readRequest = null,
            LMCDiagnosticCapabilities capabilities = null)
        {
            return new D5SdoQueuedCancelQualificationRequest(
                connection,
                capabilities ?? Capabilities(connection),
                readRequest ?? KnownValidReadRequest,
                ExpectedResult);
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
            ushort maxSdoDataBytes = 4)
        {
            return new LMCDiagnosticCapabilities(
                Response(LMCDiagnosticsDetailCode.None),
                connection.SessionGeneration,
                1,
                (uint)capabilities,
                MapRevision,
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
                DiagnosticsBootId).BindProvenance(
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

        private static LMCOperationStatus CancelledStatus(
            LMCOperationTicket ticket)
        {
            return Status(
                ticket,
                LMCOperationState.Cancelled,
                LMCOperationOutcome.Cancelled,
                LMCSignalValueType.Invalid,
                0,
                new byte[0]);
        }

        private static LMCOperationStatus CompletedStatus(
            LMCOperationTicket ticket,
            byte[] resultData = null)
        {
            return Status(
                ticket,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                1,
                resultData ?? ExpectedResult);
        }

        private static LMCOperationStatus Status(
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            LMCSignalValueType resultValueType,
            uint resultLength,
            byte[] resultData,
            short operationErrorId = 0,
            uint operationDetail = 0)
        {
            return new LMCOperationStatus(
                Response(LMCDiagnosticsDetailCode.None),
                ticket.TicketId,
                ticket.OperationKind,
                state,
                ticket.QueuedCycle,
                ticket.QueuedCycle + 10,
                outcome,
                operationErrorId,
                operationDetail,
                resultLength,
                resultValueType,
                resultData,
                ticket.DiagnosticsBootId);
        }

        private static LMCDiagnosticsCommandException CommandFailure(
            LMCDiagnosticsDetailCode detail)
        {
            return new LMCDiagnosticsCommandException(
                "test command failure",
                Response(detail));
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
                Request = KnownValidReadRequest;
                TargetTicket = Ticket(connection, Request, 101);
                RecoveryTicket = Ticket(connection, Request, 102);
                Events = new List<string>();
            }

            internal LMCSdoRequest Request { get; private set; }
            internal LMCOperationTicket TargetTicket { get; private set; }
            internal LMCOperationTicket RecoveryTicket { get; private set; }
            internal List<string> Events { get; private set; }
            internal Exception TargetSubmitError { get; set; }
            internal Exception RecoverySubmitError { get; set; }
            internal Exception CancelError { get; set; }
            internal LMCOperationTicket TargetTicketOverride { get; set; }
            internal LMCOperationTicket RecoveryTicketOverride { get; set; }
            internal LMCOperationStatus TargetStatusOverride { get; set; }
            internal LMCOperationStatus RecoveryStatusOverride { get; set; }
            internal CancellationTokenSource CancelAfterTargetSubmit
            {
                get;
                set;
            }

            internal int SubmitCount { get; private set; }
            internal int CancelCount { get; private set; }
            internal int WaitCount { get; private set; }
            internal int RecoveryCount { get; private set; }
            internal D5SdoQueuedCancelRecoveryScope RecoveryScope
            {
                get;
                private set;
            }

            internal D5SdoQueuedCancelQualificationOperations
                CreateOperations()
            {
                return new D5SdoQueuedCancelQualificationOperations
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
                            if (TargetSubmitError != null)
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    TargetSubmitError);
                            }

                            if (CancelAfterTargetSubmit != null)
                            {
                                CancelAfterTargetSubmit.Cancel();
                            }

                            return Task.FromResult(
                                TargetTicketOverride ?? TargetTicket);
                        }

                        if (SubmitCount == 2)
                        {
                            if (RecoverySubmitError != null)
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    RecoverySubmitError);
                            }

                            return Task.FromResult(
                                RecoveryTicketOverride ?? RecoveryTicket);
                        }

                        throw new InvalidOperationException(
                            "The orchestrator automatically resent an SDO Read.");
                    },
                    CancelAsync = (ticket, cancellationToken) =>
                    {
                        CancelCount++;
                        Events.Add("Cancel");
                        if (!ReferenceEquals(
                                ticket,
                                TargetTicketOverride ?? TargetTicket))
                        {
                            return TaskFromException(
                                new InvalidOperationException(
                                    "Cancel received a foreign ticket."));
                        }

                        return CancelError == null
                            ? Task.FromResult(0)
                            : TaskFromException(CancelError);
                    },
                    WaitForTerminalAsync = (ticket, cancellationToken) =>
                    {
                        WaitCount++;
                        var actualTarget = TargetTicketOverride ?? TargetTicket;
                        var actualRecovery =
                            RecoveryTicketOverride ?? RecoveryTicket;
                        if (ReferenceEquals(ticket, actualTarget))
                        {
                            Events.Add("Wait1");
                            return Task.FromResult(
                                TargetStatusOverride
                                    ?? CancelledStatus(actualTarget));
                        }

                        if (ReferenceEquals(ticket, actualRecovery))
                        {
                            Events.Add("Wait2");
                            return Task.FromResult(
                                RecoveryStatusOverride
                                    ?? CompletedStatus(actualRecovery));
                        }

                        return TaskFromException<LMCOperationStatus>(
                            new InvalidOperationException(
                                "Terminal wait received a foreign ticket."));
                    },
                    RecoveryRequired = (scope, error) =>
                    {
                        RecoveryCount++;
                        RecoveryScope = scope;
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

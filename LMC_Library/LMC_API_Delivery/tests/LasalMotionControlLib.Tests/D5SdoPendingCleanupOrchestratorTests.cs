using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoPendingCleanupOrchestratorTests
    {
        private const uint DiagnosticsBootId = 0x1234ABCDu;
        private const uint MapRevision = 0x957F101Eu;
        private const int PollMilliseconds = 25;
        private static readonly DateTime CurrentUtc =
            new DateTime(2026, 7, 27, 1, 2, 3, DateTimeKind.Utc);

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5PendingCleanup.PreflightFailsClosed",
                PreflightFailsClosed);
            tests.Add(
                "Qualification.D5PendingCleanup.IdentityMismatchQuarantines",
                IdentityMismatchQuarantines);
            tests.Add(
                "Qualification.D5PendingCleanup.CachedTerminalSkipsDispatch",
                CachedTerminalSkipsDispatch);
            tests.Add(
                "Qualification.D5PendingCleanup.CachedPendingRefreshes",
                CachedPendingRefreshes);
            tests.Add(
                "Qualification.D5PendingCleanup.QueuedCancelRequiresCancelled",
                QueuedCancelRequiresCancelled);
            tests.Add(
                "Qualification.D5PendingCleanup.InvalidStateRaceWaits",
                InvalidStateRaceWaits);
            tests.Add(
                "Qualification.D5PendingCleanup.RunningWaitsWithoutCancel",
                RunningWaitsWithoutCancel);
            tests.Add(
                "Qualification.D5PendingCleanup.WriteTerminalAndCancel",
                WriteTerminalAndCancel);
            tests.Add(
                "Qualification.D5PendingCleanup.WriteUnverifiedTerminalQuarantines",
                WriteUnverifiedTerminalQuarantines);
            tests.Add(
                "Qualification.D5PendingCleanup.WriteReadbackInterlockExact",
                WriteReadbackInterlockExact);
            tests.Add(
                "Qualification.D5PendingCleanup.CommandFailuresPreserved",
                CommandFailuresPreserved);
            tests.Add(
                "Qualification.D5PendingCleanup.TimeoutAndWaitBounds",
                TimeoutAndWaitBounds);
        }

        private static void PreflightFailsClosed()
        {
            using (var current = new LMCConnection())
            using (var previous = new LMCConnection())
            {
                var foreignTicket = Ticket(previous, 1);
                var ownerHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var ownerError = AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            foreignTicket,
                            null,
                            previous,
                            current,
                            MapRevision,
                            null),
                        ownerHarness));
                AssertEx.Contains(
                    "earlier LMCConnection",
                    ownerError.Message);
                ownerHarness.AssertNoDispatch();

                var ownerClaimHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            foreignTicket,
                            null,
                            current,
                            current,
                            MapRevision,
                            null),
                        ownerClaimHarness));
                ownerClaimHarness.AssertNoDispatch();

                var currentTicket = Ticket(current, 2);
                var mapHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var mapError = AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            currentTicket,
                            null,
                            current,
                            current,
                            MapRevision + 1,
                            null),
                        mapHarness));
                AssertEx.Contains("MapRevision", mapError.Message);
                mapHarness.AssertNoDispatch();

                var operationKindHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var operationKindError =
                    AssertEx.Throws<InvalidOperationException>(
                        () => Resolve(
                            Request(
                                current,
                                Ticket(
                                    current,
                                    3,
                                    LMCOperationKind.PIWrite),
                                null,
                                null),
                            operationKindHarness));
                AssertEx.Contains(
                    "SDO Read or SDO Write",
                    operationKindError.Message);
                operationKindHarness.AssertNoDispatch();

                AssertEx.Equal(
                    D5SdoTicketNotFoundDisposition
                        .ResolveReadBySlotContract,
                    D5SdoPendingCleanupOrchestrator
                        .EvaluateTicketNotFound(currentTicket));
                AssertEx.Equal(
                    D5SdoTicketNotFoundDisposition
                        .QuarantineWriteOutcomeUnverified,
                    D5SdoPendingCleanupOrchestrator
                        .EvaluateTicketNotFound(
                            Ticket(
                                current,
                                4,
                                LMCOperationKind.SDOWrite)));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoPendingCleanupOrchestrator
                        .EvaluateTicketNotFound(
                            Ticket(
                                current,
                                5,
                                LMCOperationKind.PIWrite)));

                var wrongStatus = Status(
                    foreignTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                var statusHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        new D5SdoPendingCleanupRequest(
                            currentTicket,
                            wrongStatus,
                            current,
                            current,
                            MapRevision,
                            null),
                        statusHarness));
                statusHarness.AssertNoDispatch();
            }
        }

        private static void IdentityMismatchQuarantines()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 10);
                var bootHarness = new Harness(
                    DiagnosticsBootId + 1,
                    MapRevision);
                var bootResult = Resolve(
                    Request(connection, ticket, null, null),
                    bootHarness);
                AssertEx.False(bootResult.IsResolved);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineDiagnosticsBootIdChanged,
                    bootResult.Disposition);
                AssertEx.Equal(
                    "diagnostics_boot_id_changed",
                    bootResult.QuarantineReason);
                AssertEx.Equal(1, bootHarness.CapabilitiesCount);
                AssertEx.Equal(0, bootHarness.StatusCount);
                AssertEx.Equal(0, bootHarness.CancelCount);

                var mapHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision + 1);
                var mapResult = Resolve(
                    Request(connection, ticket, null, null),
                    mapHarness);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineMapRevisionChanged,
                    mapResult.Disposition);
                AssertEx.Equal(
                    "diagnostics_map_revision_changed",
                    mapResult.QuarantineReason);
                AssertEx.Equal(1, mapHarness.CapabilitiesCount);
                AssertEx.Equal(0, mapHarness.StatusCount);
                AssertEx.Equal(0, mapHarness.CancelCount);

                var bothChangedHarness = new Harness(
                    DiagnosticsBootId + 1,
                    MapRevision + 1);
                var bothChangedResult = Resolve(
                    Request(connection, ticket, null, null),
                    bothChangedHarness);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineDiagnosticsBootIdChanged,
                    bothChangedResult.Disposition);
                AssertEx.Equal(
                    "diagnostics_boot_id_changed",
                    bothChangedResult.QuarantineReason);
                AssertEx.Equal(0, bothChangedHarness.StatusCount);
                AssertEx.Equal(0, bothChangedHarness.CancelCount);

                var nullCapabilitiesHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                nullCapabilitiesHarness.ReturnNullCapabilities = true;
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        nullCapabilitiesHarness));
                AssertEx.Equal(1, nullCapabilitiesHarness.CapabilitiesCount);
                AssertEx.Equal(0, nullCapabilitiesHarness.StatusCount);
                AssertEx.Equal(0, nullCapabilitiesHarness.CancelCount);

                var zeroBootHarness = new Harness(0, MapRevision);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        zeroBootHarness));
                AssertEx.Equal(1, zeroBootHarness.CapabilitiesCount);
                AssertEx.Equal(0, zeroBootHarness.StatusCount);
                AssertEx.Equal(0, zeroBootHarness.CancelCount);

                var zeroMapHarness = new Harness(DiagnosticsBootId, 0);
                AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        zeroMapHarness));
                AssertEx.Equal(1, zeroMapHarness.CapabilitiesCount);
                AssertEx.Equal(0, zeroMapHarness.StatusCount);
                AssertEx.Equal(0, zeroMapHarness.CancelCount);
            }
        }

        private static void CachedTerminalSkipsDispatch()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 20);
                var states = new[]
                {
                    LMCOperationState.Completed,
                    LMCOperationState.Failed,
                    LMCOperationState.Cancelled,
                    LMCOperationState.Expired
                };
                var outcomes = new[]
                {
                    LMCOperationOutcome.Success,
                    LMCOperationOutcome.Failed,
                    LMCOperationOutcome.Cancelled,
                    LMCOperationOutcome.TimedOut
                };

                for (var index = 0; index < states.Length; index++)
                {
                    var terminal = Status(
                        ticket,
                        states[index],
                        outcomes[index]);
                    var harness = new Harness(
                        DiagnosticsBootId,
                        MapRevision);

                    var result = Resolve(
                        Request(connection, ticket, terminal, null),
                        harness);

                    AssertEx.True(result.IsResolved);
                    AssertEx.True(result.UsedCachedTerminal);
                    AssertEx.True(ReferenceEquals(terminal, result.Status));
                    AssertEx.False(result.CancelAttempted);
                    AssertEx.Equal(0, result.StatusReadCount);
                    AssertEx.Equal(0, result.WaitMilliseconds);
                    AssertEx.Equal(1, harness.CapabilitiesCount);
                    AssertEx.Equal(0, harness.StatusCount);
                    AssertEx.Equal(0, harness.CancelCount);
                    AssertEx.Equal(0, harness.ObservedCount);
                    AssertEventOrder(harness, "cleanup-started");
                }
            }
        }

        private static void CachedPendingRefreshes()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 25);
                var cachedQueued = Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending);
                var queuedToRunning = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                queuedToRunning.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                queuedToRunning.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success));

                var runningResult = Resolve(
                    Request(connection, ticket, cachedQueued, null),
                    queuedToRunning);
                AssertEx.True(runningResult.IsResolved);
                AssertEx.False(runningResult.UsedCachedTerminal);
                AssertEx.False(runningResult.CancelAttempted);
                AssertEx.Equal(0, queuedToRunning.CancelCount);
                AssertEx.Equal(2, queuedToRunning.StatusCount);

                var cachedRunning = Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending);
                var runningToQueued = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                runningToQueued.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                runningToQueued.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled));

                var queuedResult = Resolve(
                    Request(connection, ticket, cachedRunning, null),
                    runningToQueued);
                AssertEx.True(queuedResult.IsResolved);
                AssertEx.True(queuedResult.CancelAttempted);
                AssertEx.True(queuedResult.CancelAccepted);
                AssertEx.Equal(1, runningToQueued.CancelCount);
                AssertEx.Equal(2, runningToQueued.StatusCount);
            }
        }

        private static void QueuedCancelRequiresCancelled()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 30);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                var cancelled = Status(
                    ticket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled);
                harness.Statuses.Enqueue(cancelled);

                var result = Resolve(
                    Request(connection, ticket, null, null),
                    harness);

                AssertEx.True(result.IsResolved);
                AssertEx.True(result.CancelAttempted);
                AssertEx.True(result.CancelAccepted);
                AssertEx.False(result.CancelRaceResolved);
                AssertEx.True(ReferenceEquals(cancelled, result.Status));
                AssertEx.Equal(2, result.StatusReadCount);
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(2, harness.ObservedCount);
                AssertEventOrder(
                    harness,
                    "cleanup-started",
                    "status",
                    "cancel-accepted",
                    "status");

                var invalidHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                invalidHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                invalidHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success));
                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        invalidHarness));
                AssertEx.Contains("Cancelled/Cancelled", error.Message);
                AssertEx.Equal(1, invalidHarness.CancelCount);
            }
        }

        private static void InvalidStateRaceWaits()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 40);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.CancelError = DiagnosticsError(
                    LMCDiagnosticsDetailCode.InvalidState);
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                var completed = Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                harness.Statuses.Enqueue(completed);

                var result = Resolve(
                    Request(connection, ticket, null, null),
                    harness);

                AssertEx.True(result.IsResolved);
                AssertEx.True(result.CancelAttempted);
                AssertEx.False(result.CancelAccepted);
                AssertEx.True(result.CancelRaceResolved);
                AssertEx.True(ReferenceEquals(completed, result.Status));
                AssertEx.Equal(3, result.StatusReadCount);
                AssertEx.Equal(1, harness.CancelCount);
                AssertEx.Equal(1, harness.DelayCount);
                AssertEventOrder(
                    harness,
                    "cleanup-started",
                    "status",
                    "cancel-race-resolved",
                    "status",
                    "status");
            }
        }

        private static void RunningWaitsWithoutCancel()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 50);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                var failed = Status(
                    ticket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed);
                harness.Statuses.Enqueue(failed);

                var result = Resolve(
                    Request(connection, ticket, null, null),
                    harness);

                AssertEx.True(result.IsResolved);
                AssertEx.False(result.CancelAttempted);
                AssertEx.Equal(0, harness.CancelCount);
                AssertEx.True(ReferenceEquals(failed, result.Status));
                AssertEx.Equal(2, result.StatusReadCount);
            }
        }

        private static void WriteTerminalAndCancel()
        {
            using (var connection = new LMCConnection())
            {
                var terminalTicket = Ticket(
                    connection,
                    55,
                    LMCOperationKind.SDOWrite);
                var terminalStatus = Status(
                    terminalTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                var terminalHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var terminalResult = Resolve(
                    Request(
                        connection,
                        terminalTicket,
                        terminalStatus,
                        null),
                    terminalHarness);

                AssertEx.True(terminalResult.IsResolved);
                AssertEx.True(terminalResult.UsedCachedTerminal);
                AssertEx.Equal(
                    LMCOperationKind.SDOWrite,
                    terminalResult.Status.OperationKind);
                AssertEx.Equal(0u, terminalResult.Status.ResultLength);
                AssertEx.Equal(
                    LMCSignalValueType.Invalid,
                    terminalResult.Status.ResultValueType);
                AssertEx.Equal(0, terminalResult.Status.ResultData.Length);
                AssertEx.False(terminalResult.CancelAttempted);
                AssertEx.Equal(0, terminalHarness.StatusCount);
                AssertEx.Equal(0, terminalHarness.CancelCount);

                var queuedTicket = Ticket(
                    connection,
                    56,
                    LMCOperationKind.SDOWrite);
                var cancelHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                cancelHarness.Statuses.Enqueue(Status(
                    queuedTicket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                cancelHarness.Statuses.Enqueue(Status(
                    queuedTicket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled));
                var cancelResult = Resolve(
                    Request(connection, queuedTicket, null, null),
                    cancelHarness);

                AssertEx.True(cancelResult.IsResolved);
                AssertEx.True(cancelResult.CancelAttempted);
                AssertEx.True(cancelResult.CancelAccepted);
                AssertEx.Equal(
                    LMCOperationKind.SDOWrite,
                    cancelResult.Status.OperationKind);
                AssertEx.Equal(
                    LMCOperationState.Cancelled,
                    cancelResult.Status.State);
                AssertEx.Equal(
                    LMCOperationOutcome.Cancelled,
                    cancelResult.Status.Outcome);
                AssertEx.Equal(2, cancelResult.StatusReadCount);
                AssertEx.Equal(1, cancelHarness.CancelCount);

                var priorCancelTicket = Ticket(
                    connection,
                    560,
                    LMCOperationKind.SDOWrite);
                var priorCancelled = Status(
                    priorCancelTicket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled);
                var priorCancelHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var priorCancelResult = Resolve(
                    Request(
                        connection,
                        priorCancelTicket,
                        priorCancelled,
                        null,
                        true),
                    priorCancelHarness);
                AssertEx.True(priorCancelResult.IsResolved);
                AssertEx.True(priorCancelResult.CancelAccepted);
                AssertEx.Equal(0, priorCancelHarness.StatusCount);
                AssertEx.Equal(0, priorCancelHarness.CancelCount);

                var priorPendingTicket = Ticket(
                    connection,
                    561,
                    LMCOperationKind.SDOWrite);
                var priorPendingHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                priorPendingHarness.Statuses.Enqueue(Status(
                    priorPendingTicket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                priorPendingHarness.Statuses.Enqueue(Status(
                    priorPendingTicket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled));
                var priorPendingResult = Resolve(
                    Request(
                        connection,
                        priorPendingTicket,
                        null,
                        null,
                        true),
                    priorPendingHarness);
                AssertEx.True(priorPendingResult.IsResolved);
                AssertEx.True(priorPendingResult.CancelAccepted);
                AssertEx.Equal(2, priorPendingHarness.StatusCount);
                AssertEx.Equal(0, priorPendingHarness.CancelCount);
            }
        }

        private static void WriteUnverifiedTerminalQuarantines()
        {
            using (var connection = new LMCConnection())
            {
                var failedTicket = Ticket(
                    connection,
                    57,
                    LMCOperationKind.SDOWrite);
                var failedStatus = Status(
                    failedTicket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed);
                var failedHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var failedResult = Resolve(
                    Request(
                        connection,
                        failedTicket,
                        failedStatus,
                        null),
                    failedHarness);

                AssertEx.False(failedResult.IsResolved);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineWriteTerminalOutcomeUnverified,
                    failedResult.Disposition);
                AssertEx.Equal(
                    "write_terminal_outcome_unverified",
                    failedResult.QuarantineReason);
                AssertEx.True(ReferenceEquals(failedStatus, failedResult.Status));
                AssertEx.Equal(0, failedHarness.StatusCount);
                AssertEx.Equal(0, failedHarness.CancelCount);

                var expiredTicket = Ticket(
                    connection,
                    58,
                    LMCOperationKind.SDOWrite);
                var raceHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                raceHarness.CancelError = DiagnosticsError(
                    LMCDiagnosticsDetailCode.InvalidState);
                raceHarness.Statuses.Enqueue(Status(
                    expiredTicket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                raceHarness.Statuses.Enqueue(Status(
                    expiredTicket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                var completedAfterRace = Status(
                    expiredTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                raceHarness.Statuses.Enqueue(completedAfterRace);

                var raceResult = Resolve(
                    Request(connection, expiredTicket, null, null),
                    raceHarness);
                AssertEx.False(raceResult.IsResolved);
                AssertEx.Equal(
                    D5SdoPendingCleanupDisposition
                        .QuarantineWriteTerminalOutcomeUnverified,
                    raceResult.Disposition);
                AssertEx.True(raceResult.CancelAttempted);
                AssertEx.False(raceResult.CancelAccepted);
                AssertEx.True(raceResult.CancelRaceResolved);
                AssertEx.True(ReferenceEquals(
                    completedAfterRace,
                    raceResult.Status));

                var cancelAcceptedTicket = Ticket(
                    connection,
                    59,
                    LMCOperationKind.SDOWrite);
                var cancelAcceptedHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                cancelAcceptedHarness.Statuses.Enqueue(Status(
                    cancelAcceptedTicket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                var completedAfterCancel = Status(
                    cancelAcceptedTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                cancelAcceptedHarness.Statuses.Enqueue(completedAfterCancel);

                var cancelAcceptedResult = Resolve(
                    Request(
                        connection,
                        cancelAcceptedTicket,
                        null,
                        null),
                    cancelAcceptedHarness);
                AssertEx.False(cancelAcceptedResult.IsResolved);
                AssertEx.True(cancelAcceptedResult.CancelAccepted);
                AssertEx.True(ReferenceEquals(
                    completedAfterCancel,
                    cancelAcceptedResult.Status));

                var priorCompletedTicket = Ticket(
                    connection,
                    590,
                    LMCOperationKind.SDOWrite);
                var priorCompleted = Status(
                    priorCompletedTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                var priorCompletedHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                var priorCompletedResult = Resolve(
                    Request(
                        connection,
                        priorCompletedTicket,
                        priorCompleted,
                        null,
                        true),
                    priorCompletedHarness);
                AssertEx.False(priorCompletedResult.IsResolved);
                AssertEx.True(priorCompletedResult.CancelAccepted);
                AssertEx.Equal(0, priorCompletedHarness.StatusCount);
                AssertEx.Equal(0, priorCompletedHarness.CancelCount);

                var cancelledWithoutProof = Status(
                    failedTicket,
                    LMCOperationState.Cancelled,
                    LMCOperationOutcome.Cancelled);
                AssertEx.Equal(
                    D5SdoTerminalResolutionDisposition
                        .QuarantineWriteOutcomeUnverified,
                    D5SdoPendingCleanupOrchestrator
                        .EvaluateTerminalResolution(
                            failedTicket,
                            cancelledWithoutProof,
                            false,
                            false));
                AssertEx.Equal(
                    D5SdoTerminalResolutionDisposition.Resolve,
                    D5SdoPendingCleanupOrchestrator
                        .EvaluateTerminalResolution(
                            failedTicket,
                            cancelledWithoutProof,
                            true,
                            false));
                AssertEx.Equal(
                    D5SdoTerminalResolutionDisposition
                        .QuarantineWriteOutcomeUnverified,
                    D5SdoPendingCleanupOrchestrator
                        .EvaluateTerminalResolution(
                            failedTicket,
                            Status(
                                failedTicket,
                                LMCOperationState.Expired,
                                LMCOperationOutcome.TimedOut),
                            false,
                            false));
                AssertEx.Throws<InvalidOperationException>(
                    () => D5SdoPendingCleanupOrchestrator
                        .EvaluateTerminalResolution(
                            failedTicket,
                            Status(
                                failedTicket,
                                LMCOperationState.Running,
                                LMCOperationOutcome.NoneOrPending),
                            false,
                            false));
            }
        }

        private static void WriteReadbackInterlockExact()
        {
            var source = new byte[] { 0x78, 0x56, 0x34, 0x12 };
            var writeRequest = LMCSdoRequest.CreateWrite(
                2,
                0x2000,
                3,
                LMCSignalValueType.UInt32,
                source,
                100);
            var initPayload = new byte[24];
            TestFrame.WriteUInt32(initPayload, 0, 64);
            var successAck = TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00"));
            using (var ownerServer = new FakeRpcServer(
                new FakeRpcStep(
                    0x8080,
                    TestFrame.Response(0, initPayload)),
                new FakeRpcStep(0x405C, successAck),
                new FakeRpcStep(0x405D, successAck)))
            using (var ownerConnection = new LMCConnection())
            using (var foreignConnection = new LMCConnection())
            {
                ownerConnection.RpcInitConnection(
                    "127.0.0.1",
                    ownerServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var writeTicket = new LMCOperationTicket(
                    590,
                    LMCOperationKind.SDOWrite,
                    9,
                    DiagnosticsBootId,
                    MapRevision,
                    ownerConnection.SessionGeneration,
                    ownerConnection.Diagnostics,
                    false,
                    0,
                    LMCSignalValueType.Invalid,
                    submittedSdoRequest: writeRequest);
                var writeTerminalStatus = new LMCOperationStatus(
                    Response(LMCDiagnosticsDetailCode.None),
                    writeTicket.TicketId,
                    LMCOperationKind.SDOWrite,
                    LMCOperationState.Completed,
                    writeTicket.QueuedCycle,
                    writeTicket.QueuedCycle + 1,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    0,
                    LMCSignalValueType.Invalid,
                    new byte[0],
                    DiagnosticsBootId).BindProvenance(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration);
                var requirement = ownerConnection.Diagnostics
                    .CreateSdoWriteVerificationContext(
                    writeRequest,
                    writeTicket,
                    writeTerminalStatus,
                    request => true);
                source[0] = 0;

                AssertEx.True(ReferenceEquals(
                    writeTicket,
                    requirement.WriteTicket));
                AssertEx.True(
                    writeTicket.BelongsToCurrentSession(ownerConnection));
                AssertEx.False(
                    writeTicket.BelongsToCurrentSession(foreignConnection));
                AssertEx.Equal(
                    DiagnosticsBootId,
                    requirement.DiagnosticsBootId);
                AssertEx.Equal(
                    MapRevision,
                    requirement.SubmissionMapRevision);
                AssertEx.Equal((ushort)2, requirement.SlaveReference);
                AssertEx.Equal((ushort)0x2000, requirement.ObjectIndex);
                AssertEx.Equal((byte)3, requirement.SubIndex);
                AssertEx.Equal(
                    LMCSignalValueType.UInt32,
                    requirement.ValueType);
                AssertEx.Equal((ushort)4, requirement.DataLength);
                AssertEx.SequenceEqual(
                    new byte[] { 0x78, 0x56, 0x34, 0x12 },
                    requirement.ExpectedWriteData);
                var exposedExpected = requirement.ExpectedWriteData;
                exposedExpected[0] = 0;
                AssertEx.Equal(
                    (byte)0x78,
                    requirement.ExpectedWriteData[0]);

                var exactRead = requirement.CreateReadRequest(200);
                AssertEx.True(requirement.MatchesReadRequest(exactRead));
                AssertEx.False(requirement.MatchesReadRequest(
                    LMCSdoRequest.CreateRead(
                        1,
                        0x2000,
                        3,
                        LMCSignalValueType.UInt32,
                        4,
                        200)));
                AssertEx.False(requirement.MatchesReadRequest(
                    LMCSdoRequest.CreateRead(
                        2,
                        0x2001,
                        3,
                        LMCSignalValueType.UInt32,
                        4,
                        200)));
                AssertEx.False(requirement.MatchesReadRequest(
                    LMCSdoRequest.CreateRead(
                        2,
                        0x2000,
                        4,
                        LMCSignalValueType.UInt32,
                        4,
                        200)));
                AssertEx.False(requirement.MatchesReadRequest(
                    LMCSdoRequest.CreateRead(
                        2,
                        0x2000,
                        3,
                        LMCSignalValueType.Int32,
                        4,
                        200)));
                AssertEx.False(requirement.MatchesReadRequest(
                    LMCSdoRequest.CreateRead(
                        2,
                        0x2000,
                        3,
                        LMCSignalValueType.UInt16,
                        2,
                        200)));
                AssertEx.False(
                    requirement.MatchesReadRequest(writeRequest));

                var exactCapabilities = ReadbackCapabilities(
                    ownerConnection.Diagnostics,
                    ownerConnection.SessionGeneration,
                    DiagnosticsBootId,
                    MapRevision);
                AssertEx.True(requirement.MatchesCurrentIdentity(
                    ownerConnection,
                    exactCapabilities));
                AssertEx.False(requirement.MatchesCurrentIdentity(
                    foreignConnection,
                    exactCapabilities));
                AssertEx.False(requirement.MatchesCurrentIdentity(
                    ownerConnection,
                    ReadbackCapabilities(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        DiagnosticsBootId + 1,
                        MapRevision)));
                AssertEx.False(requirement.MatchesCurrentIdentity(
                    ownerConnection,
                    ReadbackCapabilities(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        DiagnosticsBootId,
                        MapRevision + 1)));
                AssertEx.False(requirement.MatchesCurrentIdentity(
                    ownerConnection,
                    null));

                var readTicket = new LMCOperationTicket(
                    591,
                    LMCOperationKind.SDORead,
                    10,
                    DiagnosticsBootId,
                    MapRevision,
                    ownerConnection.SessionGeneration,
                    ownerConnection.Diagnostics,
                    true,
                    4,
                    LMCSignalValueType.UInt32,
                    submittedSdoRequest: exactRead);
                var exactStatus = new LMCOperationStatus(
                    Response(LMCDiagnosticsDetailCode.None),
                    readTicket.TicketId,
                    readTicket.OperationKind,
                    LMCOperationState.Completed,
                    10,
                    11,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    4,
                    LMCSignalValueType.UInt32,
                    new byte[] { 0x78, 0x56, 0x34, 0x12 },
                    readTicket.DiagnosticsBootId).BindProvenance(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Verified,
                    requirement.Evaluate(
                        exactRead,
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        exactStatus));

                var wrongStatusTicket = new LMCOperationStatus(
                    Response(LMCDiagnosticsDetailCode.None),
                    readTicket.TicketId + 1,
                    readTicket.OperationKind,
                    LMCOperationState.Completed,
                    10,
                    11,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    4,
                    LMCSignalValueType.UInt32,
                    new byte[] { 0x78, 0x56, 0x34, 0x12 },
                    readTicket.DiagnosticsBootId).BindProvenance(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        exactRead,
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        wrongStatusTicket));
                var wrongStatusBoot = new LMCOperationStatus(
                    Response(LMCDiagnosticsDetailCode.None),
                    readTicket.TicketId,
                    readTicket.OperationKind,
                    LMCOperationState.Completed,
                    10,
                    11,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    4,
                    LMCSignalValueType.UInt32,
                    new byte[] { 0x78, 0x56, 0x34, 0x12 },
                    readTicket.DiagnosticsBootId + 1).BindProvenance(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        exactRead,
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        wrongStatusBoot));

                var mismatchStatus = new LMCOperationStatus(
                    Response(LMCDiagnosticsDetailCode.None),
                    readTicket.TicketId,
                    readTicket.OperationKind,
                    LMCOperationState.Completed,
                    10,
                    11,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    4,
                    LMCSignalValueType.UInt32,
                    new byte[] { 0x79, 0x56, 0x34, 0x12 },
                    readTicket.DiagnosticsBootId).BindProvenance(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        exactRead,
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        mismatchStatus));
                var wrongTypeStatus = new LMCOperationStatus(
                    Response(LMCDiagnosticsDetailCode.None),
                    readTicket.TicketId,
                    readTicket.OperationKind,
                    LMCOperationState.Completed,
                    10,
                    11,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    4,
                    LMCSignalValueType.Int32,
                    new byte[] { 0x78, 0x56, 0x34, 0x12 },
                    readTicket.DiagnosticsBootId).BindProvenance(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        exactRead,
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        wrongTypeStatus));
                var wrongLengthStatus = new LMCOperationStatus(
                    Response(LMCDiagnosticsDetailCode.None),
                    readTicket.TicketId,
                    readTicket.OperationKind,
                    LMCOperationState.Completed,
                    10,
                    11,
                    LMCOperationOutcome.Success,
                    0,
                    0,
                    3,
                    LMCSignalValueType.UInt32,
                    new byte[] { 0x78, 0x56, 0x34 },
                    readTicket.DiagnosticsBootId).BindProvenance(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration);
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        exactRead,
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        wrongLengthStatus));
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        exactRead,
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        Status(
                            readTicket,
                            LMCOperationState.Failed,
                            LMCOperationOutcome.Failed)));
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        LMCSdoRequest.CreateRead(
                            2,
                            0x2001,
                            3,
                            LMCSignalValueType.UInt32,
                            4,
                            200),
                        readTicket,
                        ownerConnection,
                        exactCapabilities,
                        exactStatus));

                var wrongBootTicket = new LMCOperationTicket(
                    592,
                    LMCOperationKind.SDORead,
                    10,
                    DiagnosticsBootId + 1,
                    MapRevision,
                    ownerConnection.SessionGeneration,
                    ownerConnection.Diagnostics,
                    true,
                    4,
                    LMCSignalValueType.UInt32,
                    submittedSdoRequest: exactRead);
                AssertEx.False(requirement.MatchesReadTicketIdentity(
                    wrongBootTicket,
                    ownerConnection,
                    exactCapabilities));
                AssertEx.Equal(
                    LMCSdoWriteVerificationVerdict.Pending,
                    requirement.Evaluate(
                        exactRead,
                        wrongBootTicket,
                        ownerConnection,
                        exactCapabilities,
                        exactStatus));

                var wrongMapTicket = new LMCOperationTicket(
                    593,
                    LMCOperationKind.SDORead,
                    10,
                    DiagnosticsBootId,
                    MapRevision + 1,
                    ownerConnection.SessionGeneration,
                    ownerConnection.Diagnostics,
                    true,
                    4,
                    LMCSignalValueType.UInt32,
                    submittedSdoRequest: exactRead);
                AssertEx.False(requirement.MatchesReadTicketIdentity(
                    wrongMapTicket,
                    ownerConnection,
                    exactCapabilities));
                AssertEx.False(requirement.MatchesReadTicketIdentity(
                    readTicket,
                    foreignConnection,
                    exactCapabilities));
                AssertEx.False(requirement.MatchesReadTicketIdentity(
                    readTicket,
                    ownerConnection,
                    ReadbackCapabilities(
                        ownerConnection.Diagnostics,
                        ownerConnection.SessionGeneration,
                        DiagnosticsBootId + 1,
                        MapRevision)));

                using (var failedReconnectServer = new FakeRpcServer(
                    new FakeRpcStep(
                        0x8080,
                        TestFrame.Response(7, new byte[0]))))
                {
                    AssertEx.Throws<InvalidOperationException>(
                        () => ownerConnection.RpcInitConnection(
                            "127.0.0.1",
                            failedReconnectServer.Port,
                            "127.0.0.1"));
                    AssertEx.False(
                        writeTicket.BelongsToCurrentSession(
                            ownerConnection));
                    AssertEx.False(
                        requirement.MatchesOwnerCurrentSession(
                            ownerConnection));
                    AssertEx.Equal(
                        LMCSdoWriteVerificationVerdict.Pending,
                        requirement.Evaluate(
                            exactRead,
                            readTicket,
                            ownerConnection,
                            exactCapabilities,
                            exactStatus));
                    failedReconnectServer.Verify();
                }

                ownerServer.Verify();

                AssertEx.Throws<ArgumentNullException>(
                    () => ownerConnection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                        null,
                        writeTicket,
                        writeTerminalStatus,
                        request => true));
                AssertEx.Throws<ArgumentNullException>(
                    () => ownerConnection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                        writeRequest,
                        null,
                        writeTerminalStatus,
                        request => true));
                AssertEx.Throws<ArgumentNullException>(
                    () => ownerConnection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                        writeRequest,
                        writeTicket,
                        null,
                        request => true));
                AssertEx.Throws<ArgumentNullException>(
                    () => ownerConnection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                        writeRequest,
                        writeTicket,
                        writeTerminalStatus,
                        null));
                AssertEx.Throws<ArgumentException>(
                    () => ownerConnection.Diagnostics
                        .CreateSdoWriteVerificationContext(
                        exactRead,
                        writeTicket,
                        writeTerminalStatus,
                        request => true));
            }
        }

        private static void CommandFailuresPreserved()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 60);
                var capabilitiesFailure = DiagnosticsError(
                    LMCDiagnosticsDetailCode.ResourceBusy);
                var capabilitiesHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                capabilitiesHarness.CapabilitiesError = capabilitiesFailure;
                var observedCapabilitiesFailure =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            capabilitiesHarness));
                AssertEx.True(ReferenceEquals(
                    capabilitiesFailure,
                    observedCapabilitiesFailure));
                AssertEx.Equal(0, capabilitiesHarness.StatusCount);
                AssertEx.Equal(0, capabilitiesHarness.CancelCount);

                var statusFailure = DiagnosticsError(
                    LMCDiagnosticsDetailCode.ResourceBusy);
                var statusHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                statusHarness.StatusError = statusFailure;
                var observedStatusFailure =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            statusHarness));
                AssertEx.True(ReferenceEquals(
                    statusFailure,
                    observedStatusFailure));

                var cancelFailure = DiagnosticsError(
                    LMCDiagnosticsDetailCode.ResourceBusy);
                var cancelHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                cancelHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                cancelHarness.CancelError = cancelFailure;
                var observedCancelFailure =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            cancelHarness));
                AssertEx.True(ReferenceEquals(
                    cancelFailure,
                    observedCancelFailure));
                AssertEx.Equal(1, cancelHarness.StatusCount);
                AssertEx.Equal(1, cancelHarness.CancelCount);

                var nullResponseCancel = new LMCDiagnosticsCommandException(
                    "cancel response missing",
                    null);
                var nullResponseHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                nullResponseHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Queued,
                    LMCOperationOutcome.NoneOrPending));
                nullResponseHarness.CancelError = nullResponseCancel;
                var observedNullResponse =
                    AssertEx.Throws<LMCDiagnosticsCommandException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            nullResponseHarness));
                AssertEx.True(ReferenceEquals(
                    nullResponseCancel,
                    observedNullResponse));

                var timeoutFailure = new TimeoutException(
                    "transport timeout");
                var timeoutHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                timeoutHarness.StatusError = timeoutFailure;
                var observedTimeoutFailure =
                    AssertEx.Throws<TimeoutException>(
                        () => Resolve(
                            Request(connection, ticket, null, null),
                            timeoutHarness));
                AssertEx.True(ReferenceEquals(
                    timeoutFailure,
                    observedTimeoutFailure));
            }
        }

        private static void TimeoutAndWaitBounds()
        {
            AssertEx.Equal(
                15000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(null, CurrentUtc));
            AssertEx.Equal(
                15000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddSeconds(14),
                        CurrentUtc));
            AssertEx.Equal(
                120000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddSeconds(119),
                        CurrentUtc));
            AssertEx.Equal(
                21000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddSeconds(20),
                        CurrentUtc));
            AssertEx.Equal(
                120000,
                D5SdoPendingCleanupOrchestrator
                    .CalculateWaitMilliseconds(
                        CurrentUtc.AddMinutes(10),
                        CurrentUtc));

            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection, 70);
                var harness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                harness.DelayAdvanceMilliseconds = 15001;
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                harness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));

                var error = AssertEx.Throws<TimeoutException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        harness));
                AssertEx.Contains("15000 ms cleanup bound", error.Message);
                AssertEx.Equal(2, harness.StatusCount);
                AssertEx.Equal(0, harness.CancelCount);
                AssertEx.Equal(1, harness.DelayCount);

                var boundaryHarness = new Harness(
                    DiagnosticsBootId,
                    MapRevision);
                boundaryHarness.DelayAdvanceMilliseconds = 15000;
                boundaryHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                boundaryHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));
                boundaryHarness.Statuses.Enqueue(Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending));

                AssertEx.Throws<TimeoutException>(
                    () => Resolve(
                        Request(connection, ticket, null, null),
                        boundaryHarness));
                AssertEx.Equal(3, boundaryHarness.StatusCount);
                AssertEx.Equal(2, boundaryHarness.DelayCount);
            }
        }

        private static D5SdoPendingCleanupResult Resolve(
            D5SdoPendingCleanupRequest request,
            Harness harness)
        {
            return D5SdoPendingCleanupOrchestrator.CleanupAsync(
                    request,
                    harness.CreateOperations(),
                    PollMilliseconds)
                .GetAwaiter()
                .GetResult();
        }

        private static void AssertEventOrder(
            Harness harness,
            params string[] expected)
        {
            AssertEx.Equal(expected.Length, harness.Events.Count);
            for (var index = 0; index < expected.Length; index++)
            {
                AssertEx.Equal(expected[index], harness.Events[index]);
            }
        }

        private static D5SdoPendingCleanupRequest Request(
            LMCConnection connection,
            LMCOperationTicket ticket,
            LMCOperationStatus cachedStatus,
            DateTime? deadlineUtc,
            bool queuedCancelAccepted = false)
        {
            return new D5SdoPendingCleanupRequest(
                ticket,
                cachedStatus,
                connection,
                connection,
                MapRevision,
                deadlineUtc,
                queuedCancelAccepted);
        }

        private static LMCOperationTicket Ticket(
            LMCConnection connection,
            uint ticketId,
            LMCOperationKind operationKind = LMCOperationKind.SDORead)
        {
            var isRead = operationKind == LMCOperationKind.SDORead;
            return new LMCOperationTicket(
                ticketId,
                operationKind,
                10,
                DiagnosticsBootId,
                MapRevision,
                1,
                connection.Diagnostics,
                isRead,
                isRead ? (ushort)1 : (ushort)0,
                isRead
                    ? LMCSignalValueType.Int8
                    : LMCSignalValueType.Invalid);
        }

        private static LMCOperationStatus Status(
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome)
        {
            var pending = state == LMCOperationState.Queued
                || state == LMCOperationState.Running;
            var successful = state == LMCOperationState.Completed
                && outcome == LMCOperationOutcome.Success;
            var hasReadResult = successful
                && ticket.OperationKind == LMCOperationKind.SDORead;
            return new LMCOperationStatus(
                Response(LMCDiagnosticsDetailCode.None),
                ticket.TicketId,
                ticket.OperationKind,
                state,
                10,
                pending ? 0u : 11u,
                outcome,
                pending || successful ? (short)0 : (short)-1,
                pending || successful ? 0u : 1u,
                hasReadResult ? 1u : 0u,
                hasReadResult
                    ? LMCSignalValueType.Int8
                    : LMCSignalValueType.Invalid,
                hasReadResult ? new byte[] { 8 } : new byte[0],
                ticket.DiagnosticsBootId).BindProvenance(
                    ticket.Owner,
                    ticket.ConnectionSessionGeneration);
        }

        private static LMCDiagnosticCapabilities ReadbackCapabilities(
            LMCDiagnostics owner,
            long connectionSessionGeneration,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            return new LMCDiagnosticCapabilities(
                Response(LMCDiagnosticsDetailCode.None),
                connectionSessionGeneration,
                1,
                (uint)(LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline),
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
                4,
                diagnosticsBootId).BindProvenance(
                    owner,
                    connectionSessionGeneration,
                    1);
        }

        private static LMCDiagnosticsCommandException DiagnosticsError(
            LMCDiagnosticsDetailCode detail)
        {
            return new LMCDiagnosticsCommandException(
                "test diagnostics failure",
                Response(detail));
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
            private readonly uint diagnosticsBootId;
            private readonly uint mapRevision;

            internal Harness(uint diagnosticsBootId, uint mapRevision)
            {
                this.diagnosticsBootId = diagnosticsBootId;
                this.mapRevision = mapRevision;
                Statuses = new Queue<LMCOperationStatus>();
                Events = new List<string>();
                DelayAdvanceMilliseconds = PollMilliseconds;
            }

            internal Queue<LMCOperationStatus> Statuses { get; private set; }
            internal List<string> Events { get; private set; }
            internal Exception CapabilitiesError { get; set; }
            internal Exception StatusError { get; set; }
            internal Exception CancelError { get; set; }
            internal bool ReturnNullCapabilities { get; set; }
            internal int DelayAdvanceMilliseconds { get; set; }
            internal int CapabilitiesCount { get; private set; }
            internal int StatusCount { get; private set; }
            internal int CancelCount { get; private set; }
            internal int DelayCount { get; private set; }
            internal int ObservedCount { get; private set; }
            internal long ElapsedMilliseconds { get; private set; }

            internal D5SdoPendingCleanupOperations CreateOperations()
            {
                return new D5SdoPendingCleanupOperations
                {
                    ReadCapabilitiesAsync = () =>
                    {
                        CapabilitiesCount++;
                        if (CapabilitiesError != null)
                        {
                            return TaskFromException<
                                LMCDiagnosticCapabilities>(
                                    CapabilitiesError);
                        }

                        if (ReturnNullCapabilities)
                        {
                            return Task.FromResult<LMCDiagnosticCapabilities>(
                                null);
                        }

                        return Task.FromResult(Capabilities(
                            diagnosticsBootId,
                            mapRevision));
                    },
                    ReadStatusAsync = ticket =>
                    {
                        StatusCount++;
                        if (StatusError != null)
                        {
                            return TaskFromException<LMCOperationStatus>(
                                StatusError);
                        }

                        if (Statuses.Count == 0)
                        {
                            throw new InvalidOperationException(
                                "No queued test status remains.");
                        }

                        return Task.FromResult(Statuses.Dequeue());
                    },
                    CancelAsync = ticket =>
                    {
                        CancelCount++;
                        if (CancelError != null)
                        {
                            return TaskFromException(CancelError);
                        }

                        return Task.FromResult(true);
                    },
                    DelayAsync = milliseconds =>
                    {
                        DelayCount++;
                        ElapsedMilliseconds +=
                            DelayAdvanceMilliseconds;
                        return Task.FromResult(true);
                    },
                    ReadUtcNow = () => CurrentUtc,
                    ReadWaitElapsedMilliseconds =
                        () => ElapsedMilliseconds,
                    StatusObserved = status =>
                    {
                        ObservedCount++;
                        Events.Add("status");
                    },
                    CleanupStarted = () => Events.Add("cleanup-started"),
                    CancelAccepted = () => Events.Add("cancel-accepted"),
                    CancelRaceResolved =
                        () => Events.Add("cancel-race-resolved")
                };
            }

            internal void AssertNoDispatch()
            {
                AssertEx.Equal(0, CapabilitiesCount);
                AssertEx.Equal(0, StatusCount);
                AssertEx.Equal(0, CancelCount);
            }

            private static LMCDiagnosticCapabilities Capabilities(
                uint diagnosticsBootId,
                uint mapRevision)
            {
                return new LMCDiagnosticCapabilities(
                    Response(LMCDiagnosticsDetailCode.None),
                    1,
                    1,
                    (uint)LMCDiagnosticCapability.SDORead,
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
                    12,
                    diagnosticsBootId);
            }

            private static Task<T> TaskFromException<T>(Exception error)
            {
                var completion = new TaskCompletionSource<T>();
                completion.SetException(error);
                return completion.Task;
            }

            private static Task TaskFromException(Exception error)
            {
                var completion = new TaskCompletionSource<bool>();
                completion.SetException(error);
                return completion.Task;
            }
        }
    }
}

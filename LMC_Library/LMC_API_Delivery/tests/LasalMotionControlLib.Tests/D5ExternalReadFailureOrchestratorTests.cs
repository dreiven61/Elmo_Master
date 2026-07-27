using System;
using System.Collections.Generic;
using System.IO;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5ExternalReadFailureOrchestratorTests
    {
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint MapRevision = 0x957F101Eu;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5ExternalRead.MissingContextQuarantines",
                MissingContextQuarantines);
            tests.Add(
                "Qualification.D5ExternalRead.PreSubmissionDisarms",
                PreSubmissionDisarms);
            tests.Add(
                "Qualification.D5ExternalRead.UncertainQuarantines",
                UncertainQuarantines);
            tests.Add(
                "Qualification.D5ExternalRead.KnownTicketPreserveOrder",
                KnownTicketPreserveOrder);
            tests.Add(
                "Qualification.D5ExternalRead.TerminalDisarms",
                TerminalDisarms);
            tests.Add(
                "Qualification.D5ExternalRead.TerminalGenericDisarms",
                TerminalGenericDisarms);
            tests.Add(
                "Qualification.D5ExternalRead.CompositeSecondPreSubmitDisarms",
                CompositeSecondPreSubmitDisarms);
            tests.Add(
                "Qualification.D5ManualSubmit.FailureRouting",
                ManualSubmissionFailureRouting);
        }

        private static void MissingContextQuarantines()
        {
            var error = new IOException("transport");
            var calls = Route(error, null);

            AssertEx.Equal(1, calls.Count);
            AssertEx.Equal("Q:IOException:none", calls[0]);
        }

        private static void PreSubmissionDisarms()
        {
            var noAttemptError = new InvalidDataException("capabilities");
            LMCDriveReadFailureContext.Attach(
                noAttemptError,
                Context(
                    LMCDriveReadAttemptPhase.CapabilityPreflight,
                    new LMCSdoReadAttemptSnapshot[0]));
            var noAttemptCalls = Route(noAttemptError, null);
            AssertEx.Equal(1, noAttemptCalls.Count);
            AssertEx.Contains("D:PRE_SUBMISSION_FAILURE", noAttemptCalls[0]);

            var rejectedError = new InvalidOperationException("rejected");
            LMCDriveReadFailureContext.Attach(
                rejectedError,
                Context(
                    LMCDriveReadAttemptPhase.Submission,
                    new[]
                    {
                        Snapshot(
                            1,
                            0x6061,
                            LMCSdoSubmissionOutcome.Rejected,
                            null,
                            null)
                    }));
            var rejectedCalls = Route(rejectedError, null);
            AssertEx.Equal(1, rejectedCalls.Count);
            AssertEx.Contains(
                "D:PRE_TICKET_COMMAND_REJECTED",
                rejectedCalls[0]);
        }

        private static void UncertainQuarantines()
        {
            var error = new InvalidDataException("malformed submit");
            LMCDriveReadFailureContext.Attach(
                error,
                Context(
                    LMCDriveReadAttemptPhase.Submission,
                    new[]
                    {
                        Snapshot(
                            1,
                            0x6061,
                            LMCSdoSubmissionOutcome.OutcomeUncertain,
                            null,
                            null)
                    }));

            var calls = Route(error, null);
            AssertEx.Equal(1, calls.Count);
            AssertEx.Equal(
                "Q:InvalidDataException:Submission:89ABCDEF",
                calls[0]);
        }

        private static void KnownTicketPreserveOrder()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection.Diagnostics, 0x11223344u);
                var status = Status(
                    ticket,
                    LMCOperationState.Running,
                    LMCOperationOutcome.NoneOrPending);
                var error = new IOException("status response lost");
                LMCDriveReadFailureContext.Attach(
                    error,
                    Context(
                        LMCDriveReadAttemptPhase.StatusPolling,
                        new[]
                        {
                            Snapshot(
                                1,
                                0x6061,
                                LMCSdoSubmissionOutcome.Accepted,
                                ticket,
                                status)
                        }));

                LMCOperationTicket preserved = null;
                uint preservedBootId = 0;
                uint preservedMapRevision = 0;
                var calls = Route(
                    error,
                    (value, bootId, mapRevision) =>
                    {
                        preserved = value;
                        preservedBootId = bootId;
                        preservedMapRevision = mapRevision;
                    });

                AssertEx.Equal(2, calls.Count);
                AssertEx.Equal("P:" + ticket.TicketId, calls[0]);
                AssertEx.Contains("D:KNOWN_TICKET_PRESERVED", calls[1]);
                AssertEx.True(ReferenceEquals(ticket, preserved));
                AssertEx.Equal(DiagnosticsBootId, preservedBootId);
                AssertEx.Equal(MapRevision, preservedMapRevision);
            }
        }

        private static void TerminalDisarms()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection.Diagnostics, 0x22334455u);
                var status = Status(
                    ticket,
                    LMCOperationState.Failed,
                    LMCOperationOutcome.Failed);
                var error = new LMCSdoReadOperationException(ticket, status);
                LMCDriveReadFailureContext.Attach(
                    error,
                    Context(
                        LMCDriveReadAttemptPhase.StatusPolling,
                        new[]
                        {
                            Snapshot(
                                1,
                                0x6061,
                                LMCSdoSubmissionOutcome.Accepted,
                                ticket,
                                status)
                        }));

                var calls = Route(error, null);
                AssertEx.Equal(1, calls.Count);
                AssertEx.Contains("D:TERMINAL_OPERATION_FAILURE", calls[0]);
            }
        }

        private static void TerminalGenericDisarms()
        {
            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection.Diagnostics, 0x2738495Au);
                var status = Status(
                    ticket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                var error = new InvalidDataException("result projection");
                LMCDriveReadFailureContext.Attach(
                    error,
                    Context(
                        LMCDriveReadAttemptPhase.ResultMaterialization,
                        new[]
                        {
                            Snapshot(
                                1,
                                0x6061,
                                LMCSdoSubmissionOutcome.Accepted,
                                ticket,
                                status)
                        }));

                var calls = Route(error, null);
                AssertEx.Equal(1, calls.Count);
                AssertEx.Contains("D:TERMINAL_FAILURE_CONTEXT", calls[0]);
            }
        }

        private static void CompositeSecondPreSubmitDisarms()
        {
            using (var connection = new LMCConnection())
            {
                var firstTicket = Ticket(connection.Diagnostics, 0x33445566u);
                var firstStatus = Status(
                    firstTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success);
                var error = new IOException("second capabilities response lost");
                LMCDriveReadFailureContext.Attach(
                    error,
                    new LMCDriveReadFailureContext(
                        LMCDriveReadOperationKind.DriveStatus,
                        1,
                        LMCDriveReadAttemptPhase.CapabilityPreflight,
                        true,
                        new[]
                        {
                            Snapshot(
                                1,
                                0x6041,
                                LMCSdoSubmissionOutcome.Accepted,
                                firstTicket,
                                firstStatus),
                            Snapshot(
                                2,
                                0x6061,
                                LMCSdoSubmissionOutcome.NotAttempted,
                                null,
                                null)
                        }));

                var calls = Route(error, null);
                AssertEx.Equal(1, calls.Count);
                AssertEx.Contains("D:PRE_SUBMISSION_FAILURE", calls[0]);
            }
        }

        private static void ManualSubmissionFailureRouting()
        {
            var request = LMCSdoRequest.CreateRead(
                1,
                0x1000,
                0,
                LMCSignalValueType.UInt32,
                4,
                100);

            var missingCalls = RouteSubmission(
                new IOException("missing context"),
                null);
            AssertEx.Equal(1, missingCalls.Count);
            AssertEx.Equal("Q:IOException:none", missingCalls[0]);

            var preflightError = new OperationCanceledException();
            LMCSdoSubmissionFailureContext.Attach(
                preflightError,
                new LMCSdoSubmissionFailureContext(
                    request,
                    LMCSdoSubmissionPhase.SessionPreflight,
                    LMCSdoSubmissionOutcome.NotAttempted,
                    0,
                    0,
                    null));
            var preflightCalls = RouteSubmission(preflightError, null);
            AssertEx.Equal(1, preflightCalls.Count);
            AssertEx.Contains(
                "D:PRE_SUBMISSION_FAILURE",
                preflightCalls[0]);

            var rejectedError = new InvalidOperationException("rejected");
            LMCSdoSubmissionFailureContext.Attach(
                rejectedError,
                new LMCSdoSubmissionFailureContext(
                    request,
                    LMCSdoSubmissionPhase.Submission,
                    LMCSdoSubmissionOutcome.Rejected,
                    DiagnosticsBootId,
                    MapRevision,
                    null));
            var rejectedCalls = RouteSubmission(rejectedError, null);
            AssertEx.Equal(1, rejectedCalls.Count);
            AssertEx.Contains(
                "D:EXPLICIT_PLC_REJECTION",
                rejectedCalls[0]);

            var uncertainError = new InvalidDataException("malformed");
            LMCSdoSubmissionFailureContext.Attach(
                uncertainError,
                new LMCSdoSubmissionFailureContext(
                    request,
                    LMCSdoSubmissionPhase.Submission,
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    DiagnosticsBootId,
                    MapRevision,
                    null));
            var uncertainCalls = RouteSubmission(uncertainError, null);
            AssertEx.Equal(1, uncertainCalls.Count);
            AssertEx.Equal(
                "Q:InvalidDataException:Submission:89ABCDEF:957F101E",
                uncertainCalls[0]);

            using (var connection = new LMCConnection())
            {
                var ticket = Ticket(connection.Diagnostics, 0x44556677u);
                var acceptedError = new InvalidOperationException(
                    "session changed after acceptance");
                LMCSdoSubmissionFailureContext.Attach(
                    acceptedError,
                    new LMCSdoSubmissionFailureContext(
                        request,
                        LMCSdoSubmissionPhase.PostSubmissionValidation,
                        LMCSdoSubmissionOutcome.Accepted,
                        DiagnosticsBootId,
                        MapRevision,
                        ticket));

                LMCOperationTicket preserved = null;
                uint preservedBootId = 0;
                uint preservedMapRevision = 0;
                var acceptedCalls = RouteSubmission(
                    acceptedError,
                    (value, bootId, mapRevision) =>
                    {
                        preserved = value;
                        preservedBootId = bootId;
                        preservedMapRevision = mapRevision;
                    });
                AssertEx.Equal(2, acceptedCalls.Count);
                AssertEx.Equal("P:" + ticket.TicketId, acceptedCalls[0]);
                AssertEx.Contains(
                    "D:KNOWN_TICKET_PRESERVED",
                    acceptedCalls[1]);
                AssertEx.True(ReferenceEquals(ticket, preserved));
                AssertEx.Equal(DiagnosticsBootId, preservedBootId);
                AssertEx.Equal(MapRevision, preservedMapRevision);
            }
        }

        private static List<string> Route(
            Exception error,
            Action<LMCOperationTicket, uint, uint> captureTicket)
        {
            var calls = new List<string>();
            D5ExternalReadFailureOrchestrator.RouteFailure(
                error,
                (state, detail) => calls.Add("D:" + state + ":" + detail),
                (ticket, bootId, mapRevision) =>
                {
                    calls.Add("P:" + ticket.TicketId);
                    if (captureTicket != null)
                    {
                        captureTicket(ticket, bootId, mapRevision);
                    }
                },
                (unresolved, context) => calls.Add(
                    "Q:"
                        + unresolved.GetType().Name
                        + ":"
                        + (context == null
                            ? "none"
                            : context.Phase
                                + ":"
                                + (context.CurrentSdoAttempt == null
                                    ? 0u
                                    : context.CurrentSdoAttempt
                                        .DiagnosticsBootId).ToString("X8"))));
            return calls;
        }

        private static List<string> RouteSubmission(
            Exception error,
            Action<LMCOperationTicket, uint, uint> captureTicket)
        {
            var calls = new List<string>();
            D5ExternalReadFailureOrchestrator.RouteSubmissionFailure(
                error,
                (state, detail) => calls.Add("D:" + state + ":" + detail),
                (ticket, bootId, mapRevision) =>
                {
                    calls.Add("P:" + ticket.TicketId);
                    if (captureTicket != null)
                    {
                        captureTicket(ticket, bootId, mapRevision);
                    }
                },
                (unresolved, context) => calls.Add(
                    "Q:"
                        + unresolved.GetType().Name
                        + ":"
                        + (context == null
                            ? "none"
                            : context.Phase
                                + ":"
                                + context.DiagnosticsBootId.ToString("X8")
                                + ":"
                                + context.MapRevision.ToString("X8"))));
            return calls;
        }

        private static LMCDriveReadFailureContext Context(
            LMCDriveReadAttemptPhase phase,
            IList<LMCSdoReadAttemptSnapshot> attempts)
        {
            return new LMCDriveReadFailureContext(
                LMCDriveReadOperationKind.DriveOperationMode,
                1,
                phase,
                false,
                attempts);
        }

        private static LMCSdoReadAttemptSnapshot Snapshot(
            int attemptNumber,
            ushort objectIndex,
            LMCSdoSubmissionOutcome outcome,
            LMCOperationTicket ticket,
            LMCOperationStatus status)
        {
            return new LMCSdoReadAttemptSnapshot(
                attemptNumber,
                LMCSdoRequest.CreateRead(
                    1,
                    objectIndex,
                    0,
                    objectIndex == 0x6041
                        ? LMCSignalValueType.BitField16
                        : LMCSignalValueType.Int8,
                    objectIndex == 0x6041 ? (ushort)2 : (ushort)1,
                    100),
                outcome,
                ticket,
                status,
                DiagnosticsBootId,
                MapRevision);
        }

        private static LMCOperationTicket Ticket(
            LMCDiagnostics diagnostics,
            uint ticketId)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                10,
                DiagnosticsBootId,
                MapRevision,
                1,
                diagnostics,
                true,
                1,
                LMCSignalValueType.Int8);
        }

        private static LMCOperationStatus Status(
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome)
        {
            var successful = state == LMCOperationState.Completed
                && outcome == LMCOperationOutcome.Success;
            var pending = state == LMCOperationState.Queued
                || state == LMCOperationState.Running;
            return new LMCOperationStatus(
                new LMCDiagnosticsResponse(
                    new LMC_Response
                    {
                        IsFrameValid = true,
                        HeaderStatus = 0
                    },
                    1,
                    LMCDiagnosticsResponseFlags.None,
                    0,
                    0,
                    1,
                    0),
                ticket.TicketId,
                ticket.OperationKind,
                state,
                10,
                pending ? 0u : 11u,
                outcome,
                pending || successful ? (short)0 : (short)-1,
                pending || successful ? 0u : 1u,
                successful ? 1u : 0u,
                successful
                    ? LMCSignalValueType.Int8
                    : LMCSignalValueType.Invalid,
                successful ? new byte[] { 8 } : new byte[0],
                ticket.DiagnosticsBootId);
        }
    }
}

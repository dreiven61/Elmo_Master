using System;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal static class D5ExternalReadFailureOrchestrator
    {
        internal static void RouteFailure(
            Exception error,
            Action<string, string> disarmGuard,
            Action<LMCOperationTicket> preserveKnownTicket,
            Action<Exception, LMCDriveReadFailureContext> quarantineUnknown)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (disarmGuard == null)
            {
                throw new ArgumentNullException("disarmGuard");
            }

            if (preserveKnownTicket == null)
            {
                throw new ArgumentNullException("preserveKnownTicket");
            }

            if (quarantineUnknown == null)
            {
                throw new ArgumentNullException("quarantineUnknown");
            }

            LMCDriveReadFailureContext context;
            if (!LMCDriveReadFailureContext.TryGet(error, out context))
            {
                quarantineUnknown(error, null);
                return;
            }

            LMCSdoReadAttemptSnapshot acceptedNonTerminal = null;
            var acceptedNonTerminalCount = 0;
            for (var index = 0; index < context.SdoAttempts.Count; index++)
            {
                var attempt = context.SdoAttempts[index];
                if (attempt.GenericSubmissionOutcome
                    == LMCSdoSubmissionOutcome.OutcomeUncertain)
                {
                    quarantineUnknown(error, context);
                    return;
                }

                if (attempt.GenericSubmissionOutcome
                        == LMCSdoSubmissionOutcome.Accepted
                    && !attempt.IsTerminal)
                {
                    acceptedNonTerminal = attempt;
                    acceptedNonTerminalCount++;
                }
            }

            if (acceptedNonTerminalCount > 1)
            {
                quarantineUnknown(error, context);
                return;
            }

            if (acceptedNonTerminalCount == 1)
            {
                preserveKnownTicket(acceptedNonTerminal.Ticket);
                disarmGuard(
                    "KNOWN_TICKET_PRESERVED",
                    CreateKnownTicketDetail(error));
                return;
            }

            var currentAttempt = context.CurrentSdoAttempt;
            if (currentAttempt == null
                || currentAttempt.GenericSubmissionOutcome
                    == LMCSdoSubmissionOutcome.NotAttempted)
            {
                disarmGuard(
                    "PRE_SUBMISSION_FAILURE",
                    context.Phase + ":" + error.GetType().Name);
                return;
            }

            if (currentAttempt.GenericSubmissionOutcome
                == LMCSdoSubmissionOutcome.Rejected)
            {
                disarmGuard(
                    "PRE_TICKET_COMMAND_REJECTED",
                    CreateRejectedDetail(error, context));
                return;
            }

            if (currentAttempt.GenericSubmissionOutcome
                    == LMCSdoSubmissionOutcome.Accepted
                && currentAttempt.IsTerminal)
            {
                var operationFailure = error as LMCSdoReadOperationException;
                if (operationFailure != null)
                {
                    disarmGuard(
                        "TERMINAL_OPERATION_FAILURE",
                        operationFailure.OperationStatus.State
                            + "/"
                            + operationFailure.OperationStatus.Outcome);
                    return;
                }

                disarmGuard(
                    "TERMINAL_FAILURE_CONTEXT",
                    context.Phase
                        + ":"
                        + currentAttempt.LastOperationStatus.State
                        + "/"
                        + currentAttempt.LastOperationStatus.Outcome
                        + ":"
                        + error.GetType().Name);
                return;
            }

            quarantineUnknown(error, context);
        }

        internal static void RouteSubmissionFailure(
            Exception error,
            Action<string, string> disarmGuard,
            Action<LMCOperationTicket> preserveKnownTicket,
            Action<Exception, LMCSdoSubmissionFailureContext>
                quarantineUnknown)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (disarmGuard == null)
            {
                throw new ArgumentNullException("disarmGuard");
            }

            if (preserveKnownTicket == null)
            {
                throw new ArgumentNullException("preserveKnownTicket");
            }

            if (quarantineUnknown == null)
            {
                throw new ArgumentNullException("quarantineUnknown");
            }

            LMCSdoSubmissionFailureContext context;
            if (!LMCSdoSubmissionFailureContext.TryGet(error, out context))
            {
                quarantineUnknown(error, null);
                return;
            }

            switch (context.SubmissionOutcome)
            {
                case LMCSdoSubmissionOutcome.NotAttempted:
                    disarmGuard(
                        "PRE_SUBMISSION_FAILURE",
                        context.Phase + ":" + error.GetType().Name);
                    return;

                case LMCSdoSubmissionOutcome.Rejected:
                    disarmGuard(
                        "EXPLICIT_PLC_REJECTION",
                        CreateSubmissionRejectedDetail(error, context));
                    return;

                case LMCSdoSubmissionOutcome.OutcomeUncertain:
                    quarantineUnknown(error, context);
                    return;

                case LMCSdoSubmissionOutcome.Accepted:
                    preserveKnownTicket(context.Ticket);
                    disarmGuard(
                        "KNOWN_TICKET_PRESERVED",
                        "post_submission_validation:"
                            + error.GetType().Name);
                    return;

                default:
                    quarantineUnknown(error, context);
                    return;
            }
        }

        private static string CreateKnownTicketDetail(Exception error)
        {
            if (error is LMCSdoReadPollingTimeoutException)
            {
                return "polling_timeout";
            }

            if (error is LMCSdoReadWaitCanceledException)
            {
                return "wait_cancelled";
            }

            var commandFailure = error as LMCSdoReadCommandException;
            if (commandFailure != null)
            {
                return "status_command_failure:"
                    + (commandFailure.Response == null
                        ? "response_unavailable"
                        : commandFailure.Response.Detail.ToString());
            }

            return "status_failure:" + error.GetType().Name;
        }

        private static string CreateRejectedDetail(
            Exception error,
            LMCDriveReadFailureContext context)
        {
            var commandFailure = error as LMCSdoReadCommandException;
            if (commandFailure != null)
            {
                return commandFailure.Stage
                    + ":"
                    + (commandFailure.Response == null
                        ? "response_unavailable"
                        : commandFailure.Response.Detail.ToString());
            }

            return context.Phase + ":" + error.GetType().Name;
        }

        private static string CreateSubmissionRejectedDetail(
            Exception error,
            LMCSdoSubmissionFailureContext context)
        {
            var commandFailure = error as LMCDiagnosticsCommandException;
            if (commandFailure != null)
            {
                return commandFailure.Response == null
                    ? "response_unavailable"
                    : commandFailure.Response.Detail.ToString();
            }

            return context.Phase + ":" + error.GetType().Name;
        }
    }
}

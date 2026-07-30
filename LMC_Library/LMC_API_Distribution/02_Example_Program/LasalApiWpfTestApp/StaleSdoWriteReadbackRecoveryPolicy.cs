using System;

namespace LasalMotionControlApiExample
{
    internal static class StaleSdoWriteReadbackRecoveryPolicy
    {
        internal static bool CanAcknowledge(
            bool idle,
            bool journalAvailable,
            bool pendingReadbackExists,
            bool pendingBelongsToCurrentSession,
            bool hasD5TicketOrQuarantine,
            bool hasUnresolvedDigitalOutputWrite,
            DiagnosticsMutationRecord record,
            uint pendingWriteTicketId,
            uint pendingDiagnosticsBootId,
            uint pendingMapRevision)
        {
            if (!idle
                || !journalAvailable
                || !pendingReadbackExists
                || pendingBelongsToCurrentSession
                || hasD5TicketOrQuarantine
                || hasUnresolvedDigitalOutputWrite
                || record == null
                || !record.IsActive
                || record.Kind != DiagnosticsMutationKind.SdoWrite
                || pendingWriteTicketId == 0
                || record.TicketId != pendingWriteTicketId
                || record.DiagnosticsBootId != pendingDiagnosticsBootId
                || record.IdentityRevision != pendingMapRevision)
            {
                return false;
            }

            return record.State
                    == DiagnosticsMutationState
                        .TerminalSuccessPendingReadback
                || record.State == DiagnosticsMutationState.OutcomeUnverified
                || record.State == DiagnosticsMutationState.ReadbackMismatch;
        }

        internal static bool CanConfirm(
            bool acknowledgementAvailable,
            bool physicalTargetVerified)
        {
            return acknowledgementAvailable && physicalTargetVerified;
        }

        internal static bool CanAcknowledgeStartupRecovery(
            bool idle,
            bool journalAvailable,
            bool recoveredAtStartup,
            bool activeJournalRecordExists,
            bool exactWriteReadbackPending,
            bool hasD5TicketOrQuarantine,
            bool hasUnresolvedDigitalOutputWrite)
        {
            return idle
                && journalAvailable
                && recoveredAtStartup
                && activeJournalRecordExists
                && !exactWriteReadbackPending
                && !hasD5TicketOrQuarantine
                && !hasUnresolvedDigitalOutputWrite;
        }
    }

    internal static class StaleSdoWriteReadbackRecoveryCommitter
    {
        internal static bool TryCommit(
            Func<bool> guardedStateIsCurrent,
            Action persistResolvedTombstone,
            Action clearPendingReadback)
        {
            if (guardedStateIsCurrent == null)
            {
                throw new ArgumentNullException("guardedStateIsCurrent");
            }

            if (persistResolvedTombstone == null)
            {
                throw new ArgumentNullException("persistResolvedTombstone");
            }

            if (clearPendingReadback == null)
            {
                throw new ArgumentNullException("clearPendingReadback");
            }

            if (!guardedStateIsCurrent())
            {
                return false;
            }

            // Persistence must complete before volatile state is released.
            // If it throws, clearPendingReadback is never invoked.
            persistResolvedTombstone();
            clearPendingReadback();
            return true;
        }
    }
}

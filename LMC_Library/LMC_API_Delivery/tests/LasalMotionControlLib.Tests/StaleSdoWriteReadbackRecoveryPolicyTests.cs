using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;

namespace LasalMotionControlLib.Tests
{
    internal static class StaleSdoWriteReadbackRecoveryPolicyTests
    {
        private const uint TicketId = 17;
        private const uint DiagnosticsBootId = 0x12345678u;
        private const uint MapRevision = 0x87654321u;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Policy.SdoRecovery.StaleMatchingEvidenceAccepted",
                StaleMatchingEvidenceAccepted);
            tests.Add(
                "Policy.SdoRecovery.CurrentSessionAndBusyStatesRejected",
                CurrentSessionAndBusyStatesRejected);
            tests.Add(
                "Policy.SdoRecovery.JournalMismatchRejected",
                JournalMismatchRejected);
            tests.Add(
                "Policy.SdoRecovery.CommitPersistsBeforeClear",
                CommitPersistsBeforeClear);
        }

        private static void StaleMatchingEvidenceAccepted()
        {
            var record = CreateRecord(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.TerminalSuccessPendingReadback,
                TicketId,
                DiagnosticsBootId,
                MapRevision);
            var available = CanAcknowledge(record);

            AssertEx.True(available);
            AssertEx.False(
                StaleSdoWriteReadbackRecoveryPolicy.CanConfirm(
                    available,
                    false));
            AssertEx.True(
                StaleSdoWriteReadbackRecoveryPolicy.CanConfirm(
                    available,
                    true));

            AssertEx.True(CanAcknowledge(CreateRecord(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.OutcomeUnverified,
                TicketId,
                DiagnosticsBootId,
                MapRevision)));
            AssertEx.True(CanAcknowledge(CreateRecord(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.ReadbackMismatch,
                TicketId,
                DiagnosticsBootId,
                MapRevision)));
        }

        private static void CurrentSessionAndBusyStatesRejected()
        {
            var record = CreateRecord(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.TerminalSuccessPendingReadback,
                TicketId,
                DiagnosticsBootId,
                MapRevision);

            AssertEx.False(CanAcknowledge(record, idle: false));
            AssertEx.False(CanAcknowledge(
                record,
                journalAvailable: false));
            AssertEx.False(CanAcknowledge(
                record,
                pendingReadbackExists: false));
            AssertEx.False(CanAcknowledge(
                record,
                pendingBelongsToCurrentSession: true));
            AssertEx.False(CanAcknowledge(
                record,
                hasD5TicketOrQuarantine: true));
            AssertEx.False(CanAcknowledge(
                record,
                hasUnresolvedDigitalOutputWrite: true));

            AssertEx.True(
                StaleSdoWriteReadbackRecoveryPolicy
                    .CanAcknowledgeStartupRecovery(
                        true,
                        true,
                        true,
                        true,
                        false,
                        false,
                        false));
            AssertEx.False(
                StaleSdoWriteReadbackRecoveryPolicy
                    .CanAcknowledgeStartupRecovery(
                        true,
                        true,
                        true,
                        true,
                        true,
                        false,
                        false),
                "Startup recovery must never bypass a current-process exact readback interlock.");
        }

        private static void JournalMismatchRejected()
        {
            AssertEx.False(CanAcknowledge(null));
            AssertEx.False(CanAcknowledge(CreateRecord(
                DiagnosticsMutationKind.DigitalOutputWrite,
                DiagnosticsMutationState.TerminalSuccessPendingReadback,
                TicketId,
                DiagnosticsBootId,
                MapRevision)));
            AssertEx.False(CanAcknowledge(CreateRecord(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.AcceptedPendingTerminal,
                TicketId,
                DiagnosticsBootId,
                MapRevision)));
            AssertEx.False(CanAcknowledge(CreateRecord(
                DiagnosticsMutationKind.SdoWrite,
                DiagnosticsMutationState.Resolved,
                TicketId,
                DiagnosticsBootId,
                MapRevision)));
            AssertEx.False(StaleSdoWriteReadbackRecoveryPolicy.CanAcknowledge(
                true,
                true,
                true,
                false,
                false,
                false,
                CreateRecord(
                    DiagnosticsMutationKind.SdoWrite,
                    DiagnosticsMutationState.OutcomeUnverified,
                    TicketId,
                    DiagnosticsBootId,
                    MapRevision),
                TicketId + 1,
                DiagnosticsBootId,
                MapRevision));
            AssertEx.False(StaleSdoWriteReadbackRecoveryPolicy.CanAcknowledge(
                true,
                true,
                true,
                false,
                false,
                false,
                CreateRecord(
                    DiagnosticsMutationKind.SdoWrite,
                    DiagnosticsMutationState.OutcomeUnverified,
                    TicketId,
                    DiagnosticsBootId,
                    MapRevision),
                TicketId,
                DiagnosticsBootId + 1,
                MapRevision));
            AssertEx.False(StaleSdoWriteReadbackRecoveryPolicy.CanAcknowledge(
                true,
                true,
                true,
                false,
                false,
                false,
                CreateRecord(
                    DiagnosticsMutationKind.SdoWrite,
                    DiagnosticsMutationState.OutcomeUnverified,
                    TicketId,
                    DiagnosticsBootId,
                    MapRevision),
                TicketId,
                DiagnosticsBootId,
                MapRevision + 1));
        }

        private static void CommitPersistsBeforeClear()
        {
            var calls = new List<string>();
            AssertEx.False(
                StaleSdoWriteReadbackRecoveryCommitter.TryCommit(
                    () => false,
                    () => calls.Add("persist"),
                    () => calls.Add("clear")));
            AssertEx.Equal(0, calls.Count);

            AssertEx.Throws<InvalidOperationException>(() =>
                StaleSdoWriteReadbackRecoveryCommitter.TryCommit(
                    () => true,
                    () =>
                    {
                        calls.Add("persist");
                        throw new InvalidOperationException(
                            "Injected durable write failure.");
                    },
                    () => calls.Add("clear")));
            AssertEx.Equal(1, calls.Count);
            AssertEx.Equal("persist", calls[0]);

            calls.Clear();
            AssertEx.True(
                StaleSdoWriteReadbackRecoveryCommitter.TryCommit(
                    () => true,
                    () => calls.Add("persist"),
                    () => calls.Add("clear")));
            AssertEx.Equal(2, calls.Count);
            AssertEx.Equal("persist", calls[0]);
            AssertEx.Equal("clear", calls[1]);
        }

        private static bool CanAcknowledge(
            DiagnosticsMutationRecord record,
            bool idle = true,
            bool journalAvailable = true,
            bool pendingReadbackExists = true,
            bool pendingBelongsToCurrentSession = false,
            bool hasD5TicketOrQuarantine = false,
            bool hasUnresolvedDigitalOutputWrite = false)
        {
            return StaleSdoWriteReadbackRecoveryPolicy.CanAcknowledge(
                idle,
                journalAvailable,
                pendingReadbackExists,
                pendingBelongsToCurrentSession,
                hasD5TicketOrQuarantine,
                hasUnresolvedDigitalOutputWrite,
                record,
                TicketId,
                DiagnosticsBootId,
                MapRevision);
        }

        private static DiagnosticsMutationRecord CreateRecord(
            DiagnosticsMutationKind kind,
            DiagnosticsMutationState state,
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            var createdUtc = new DateTime(
                2026,
                7,
                28,
                0,
                0,
                0,
                DateTimeKind.Utc);
            return new DiagnosticsMutationRecord(
                Guid.NewGuid(),
                kind,
                state,
                createdUtc,
                createdUtc,
                diagnosticsBootId,
                mapRevision,
                1,
                ticketId,
                "target",
                "expected");
        }
    }
}

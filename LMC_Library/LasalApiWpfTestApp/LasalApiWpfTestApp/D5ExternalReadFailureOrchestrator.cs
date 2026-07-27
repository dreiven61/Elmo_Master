using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal static class D5ExternalReadFailureOrchestrator
    {
        internal static void RouteFailure(
            Exception error,
            Action<string, string> disarmGuard,
            Action<LMCOperationTicket, uint, uint> preserveKnownTicket,
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
                preserveKnownTicket(
                    acceptedNonTerminal.Ticket,
                    acceptedNonTerminal.DiagnosticsBootId,
                    acceptedNonTerminal.MapRevision);
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
            Action<LMCOperationTicket, uint, uint> preserveKnownTicket,
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
                    preserveKnownTicket(
                        context.Ticket,
                        context.DiagnosticsBootId,
                        context.MapRevision);
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

    internal sealed class D5SdoQuarantineHandle
    {
        private readonly object ledgerIdentity;
        private readonly long entryId;

        internal D5SdoQuarantineHandle(
            object ledgerIdentity,
            long entryId)
        {
            this.ledgerIdentity = ledgerIdentity;
            this.entryId = entryId;
        }

        internal bool BelongsTo(object expectedLedgerIdentity)
        {
            return ReferenceEquals(ledgerIdentity, expectedLedgerIdentity);
        }

        internal bool Matches(long expectedEntryId)
        {
            return entryId == expectedEntryId;
        }
    }

    internal sealed class D5SdoQuarantineEvidence
    {
        internal D5SdoQuarantineEvidence(
            long entryId,
            long entryRevision,
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision,
            ushort slaveReference,
            uint timeoutCycles,
            LMCConnection ownerConnection,
            string stage,
            string reason,
            string evidenceId)
        {
            EntryId = entryId;
            EntryRevision = entryRevision;
            TicketId = ticketId;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            SlaveReference = slaveReference;
            TimeoutCycles = timeoutCycles;
            OwnerConnection = ownerConnection;
            Stage = stage;
            Reason = reason;
            EvidenceId = evidenceId;
        }

        internal long EntryId { get; private set; }
        internal long EntryRevision { get; private set; }
        internal uint TicketId { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal ushort SlaveReference { get; private set; }
        internal uint TimeoutCycles { get; private set; }
        internal LMCConnection OwnerConnection { get; private set; }
        internal string Stage { get; private set; }
        internal string Reason { get; private set; }
        internal string EvidenceId { get; private set; }

        internal bool ContentEquals(D5SdoQuarantineEvidence other)
        {
            return other != null
                && EntryId == other.EntryId
                && EntryRevision == other.EntryRevision
                && TicketId == other.TicketId
                && DiagnosticsBootId == other.DiagnosticsBootId
                && MapRevision == other.MapRevision
                && SlaveReference == other.SlaveReference
                && TimeoutCycles == other.TimeoutCycles
                && ReferenceEquals(OwnerConnection, other.OwnerConnection)
                && string.Equals(Stage, other.Stage, StringComparison.Ordinal)
                && string.Equals(Reason, other.Reason, StringComparison.Ordinal)
                && string.Equals(
                    EvidenceId,
                    other.EvidenceId,
                    StringComparison.Ordinal);
        }
    }

    internal sealed class D5SdoQuarantineSnapshot
    {
        private readonly object ledgerIdentity;
        private readonly ReadOnlyCollection<D5SdoQuarantineEvidence> entries;

        internal D5SdoQuarantineSnapshot(
            object ledgerIdentity,
            long version,
            IList<D5SdoQuarantineEvidence> capturedEntries)
        {
            this.ledgerIdentity = ledgerIdentity;
            Version = version;
            entries = new ReadOnlyCollection<D5SdoQuarantineEvidence>(
                capturedEntries == null
                    ? new List<D5SdoQuarantineEvidence>()
                    : new List<D5SdoQuarantineEvidence>(capturedEntries));
        }

        internal long Version { get; private set; }
        internal IReadOnlyList<D5SdoQuarantineEvidence> Entries
        {
            get { return entries; }
        }

        internal bool BelongsTo(object expectedLedgerIdentity)
        {
            return ReferenceEquals(ledgerIdentity, expectedLedgerIdentity);
        }
    }

    internal sealed class D5SdoQuarantineLedger
    {
        private sealed class Entry
        {
            internal long EntryId;
            internal long EntryRevision;
            internal uint TicketId;
            internal uint DiagnosticsBootId;
            internal uint MapRevision;
            internal ushort SlaveReference;
            internal uint TimeoutCycles;
            internal LMCConnection OwnerConnection;
            internal string Stage;
            internal string Reason;
            internal string EvidenceId;

            internal D5SdoQuarantineEvidence Capture()
            {
                return new D5SdoQuarantineEvidence(
                    EntryId,
                    EntryRevision,
                    TicketId,
                    DiagnosticsBootId,
                    MapRevision,
                    SlaveReference,
                    TimeoutCycles,
                    OwnerConnection,
                    Stage,
                    Reason,
                    EvidenceId);
            }
        }

        private readonly object sync = new object();
        private readonly object ledgerIdentity = new object();
        private readonly List<Entry> entries = new List<Entry>();
        private long version;
        private long nextEntryId;
        private bool proofCommitInProgress;

        internal bool HasEntries
        {
            get
            {
                lock (sync)
                {
                    return entries.Count != 0;
                }
            }
        }

        internal int Count
        {
            get
            {
                lock (sync)
                {
                    return entries.Count;
                }
            }
        }

        internal D5SdoQuarantineHandle ArmUnknown(
            LMCConnection ownerConnection,
            uint diagnosticsBootId,
            uint mapRevision,
            ushort slaveReference,
            uint timeoutCycles,
            string stage,
            string reason,
            string evidenceId = null)
        {
            return AddEntry(
                0,
                diagnosticsBootId,
                mapRevision,
                slaveReference,
                timeoutCycles,
                ownerConnection,
                stage,
                reason,
                evidenceId ?? Guid.NewGuid().ToString("N"));
        }

        internal D5SdoQuarantineHandle QuarantineKnownTicket(
            LMCOperationTicket ticket,
            LMCConnection ownerConnection,
            ushort slaveReference,
            uint timeoutCycles,
            string stage,
            string reason,
            string evidenceId,
            uint mapRevision)
        {
            RequireSdoReadTicket(ticket);
            RequireTicketOwner(ticket, ownerConnection);
            if (ticket.SubmissionMapRevision != mapRevision)
            {
                throw new InvalidOperationException(
                    "The quarantined D5 ticket MapRevision does not match its evidence.");
            }

            return AddEntry(
                ticket.TicketId,
                ticket.DiagnosticsBootId,
                mapRevision,
                slaveReference,
                timeoutCycles,
                ownerConnection,
                stage,
                reason,
                evidenceId);
        }

        internal D5SdoQuarantineEvidence GetEvidence(
            D5SdoQuarantineHandle handle)
        {
            lock (sync)
            {
                return RequireEntry(handle).Capture();
            }
        }

        internal D5SdoQuarantineEvidence ReconcileUnknown(
            D5SdoQuarantineHandle handle,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            if (diagnosticsBootId == 0 || mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Submission identity requires non-zero BootId and MapRevision.");
            }

            lock (sync)
            {
                RequireMutationAllowed();
                var entry = RequireEntry(handle);
                if (entry.TicketId != 0)
                {
                    throw new InvalidOperationException(
                        "Only unknown-ticket evidence can reconcile submission identity.");
                }

                if (entry.DiagnosticsBootId == diagnosticsBootId
                    && entry.MapRevision == mapRevision)
                {
                    return entry.Capture();
                }

                RequireEntryRevisionAvailable(entry);
                AdvanceVersion();
                entry.EntryRevision++;
                entry.DiagnosticsBootId = diagnosticsBootId;
                entry.MapRevision = mapRevision;
                return entry.Capture();
            }
        }

        internal D5SdoQuarantineEvidence TransitionToAccepted(
            D5SdoQuarantineHandle handle,
            LMCOperationTicket ticket,
            uint actualDiagnosticsBootId,
            uint actualMapRevision)
        {
            RequireSdoReadTicket(ticket);
            if (actualDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "actualDiagnosticsBootId");
            }

            if (actualMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("actualMapRevision");
            }

            if (ticket.DiagnosticsBootId != actualDiagnosticsBootId
                || ticket.SubmissionMapRevision != actualMapRevision)
            {
                throw new InvalidOperationException(
                    "The accepted D5 ticket does not match the actual submission identity.");
            }

            lock (sync)
            {
                RequireMutationAllowed();
                var entry = RequireEntry(handle);
                if (entry.TicketId != 0)
                {
                    throw new InvalidOperationException(
                        "Only unknown-ticket evidence can transition to an accepted ticket.");
                }

                RequireTicketOwner(ticket, entry.OwnerConnection);

                EnsureKnownTicketIsUnique(
                    entry,
                    ticket.TicketId,
                    actualDiagnosticsBootId,
                    entry.OwnerConnection);

                RequireEntryRevisionAvailable(entry);
                AdvanceVersion();
                entry.EntryRevision++;
                entry.TicketId = ticket.TicketId;
                entry.DiagnosticsBootId = actualDiagnosticsBootId;
                entry.MapRevision = actualMapRevision;

                return entry.Capture();
            }
        }

        internal D5SdoQuarantineEvidence Disarm(
            D5SdoQuarantineHandle handle)
        {
            lock (sync)
            {
                RequireMutationAllowed();
                var index = RequireEntryIndex(handle);
                var evidence = entries[index].Capture();
                AdvanceVersion();
                entries.RemoveAt(index);
                return evidence;
            }
        }

        internal D5SdoQuarantineSnapshot CaptureSnapshot()
        {
            lock (sync)
            {
                return CaptureSnapshotCore();
            }
        }

        internal bool TryClearAfterProof(
            D5SdoQuarantineSnapshot baseline,
            D5SdoQuarantineSnapshot candidate,
            Action beforeClear)
        {
            if (baseline == null)
            {
                throw new ArgumentNullException("baseline");
            }

            if (candidate == null)
            {
                throw new ArgumentNullException("candidate");
            }

            if (beforeClear == null)
            {
                throw new ArgumentNullException("beforeClear");
            }

            lock (sync)
            {
                RequireMutationAllowed();
                if (!baseline.BelongsTo(ledgerIdentity)
                    || !candidate.BelongsTo(ledgerIdentity))
                {
                    throw new InvalidOperationException(
                        "The D5 quarantine snapshot belongs to a different ledger.");
                }

                var live = CaptureSnapshotCore();
                if (candidate.Version != version
                    || !EvidenceSequencesEqual(
                        candidate.Entries,
                        live.Entries)
                    || !EvidenceSequencesEqual(
                        baseline.Entries,
                        candidate.Entries))
                {
                    return false;
                }

                RequireVersionAvailable();
                proofCommitInProgress = true;
                try
                {
                    beforeClear();
                    AdvanceVersion();
                    entries.Clear();
                }
                finally
                {
                    proofCommitInProgress = false;
                }

                return true;
            }
        }

        private D5SdoQuarantineHandle AddEntry(
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision,
            ushort slaveReference,
            uint timeoutCycles,
            LMCConnection ownerConnection,
            string stage,
            string reason,
            string evidenceId)
        {
            ValidateEvidence(
                diagnosticsBootId,
                mapRevision,
                slaveReference,
                timeoutCycles,
                ownerConnection,
                stage,
                reason,
                evidenceId);

            lock (sync)
            {
                RequireMutationAllowed();
                for (var index = 0; index < entries.Count; index++)
                {
                    if (string.Equals(
                        entries[index].EvidenceId,
                        evidenceId,
                        StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "The D5 quarantine evidence id is already active.");
                    }
                }


                if (ticketId != 0)
                {
                    EnsureKnownTicketIsUnique(
                        null,
                        ticketId,
                        diagnosticsBootId,
                        ownerConnection);
                }

                if (nextEntryId == long.MaxValue)
                {
                    throw new InvalidOperationException(
                        "The D5 quarantine entry sequence is exhausted.");
                }

                AdvanceVersion();
                nextEntryId++;
                var entry = new Entry
                {
                    EntryId = nextEntryId,
                    EntryRevision = 1,
                    TicketId = ticketId,
                    DiagnosticsBootId = diagnosticsBootId,
                    MapRevision = mapRevision,
                    SlaveReference = slaveReference,
                    TimeoutCycles = timeoutCycles,
                    OwnerConnection = ownerConnection,
                    Stage = stage,
                    Reason = reason,
                    EvidenceId = evidenceId
                };
                entries.Add(entry);
                return new D5SdoQuarantineHandle(
                    ledgerIdentity,
                    entry.EntryId);
            }
        }

        private Entry RequireEntry(D5SdoQuarantineHandle handle)
        {
            return entries[RequireEntryIndex(handle)];
        }

        private int RequireEntryIndex(D5SdoQuarantineHandle handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException("handle");
            }

            if (!handle.BelongsTo(ledgerIdentity))
            {
                throw new InvalidOperationException(
                    "The D5 quarantine handle belongs to a different ledger.");
            }

            for (var index = 0; index < entries.Count; index++)
            {
                if (handle.Matches(entries[index].EntryId))
                {
                    return index;
                }
            }

            throw new InvalidOperationException(
                "The D5 quarantine handle is stale or already resolved.");
        }

        private D5SdoQuarantineSnapshot CaptureSnapshotCore()
        {
            var captured = new D5SdoQuarantineEvidence[entries.Count];
            for (var index = 0; index < entries.Count; index++)
            {
                captured[index] = entries[index].Capture();
            }

            return new D5SdoQuarantineSnapshot(
                ledgerIdentity,
                version,
                captured);
        }

        private void EnsureKnownTicketIsUnique(
            Entry excludedEntry,
            uint ticketId,
            uint diagnosticsBootId,
            LMCConnection ownerConnection)
        {
            for (var index = 0; index < entries.Count; index++)
            {
                var current = entries[index];
                if (!ReferenceEquals(current, excludedEntry)
                    && current.TicketId == ticketId
                    && current.DiagnosticsBootId == diagnosticsBootId
                    && ReferenceEquals(
                        current.OwnerConnection,
                        ownerConnection))
                {
                    throw new InvalidOperationException(
                        "The accepted D5 ticket is already quarantined.");
                }
            }
        }

        private static bool EvidenceSequencesEqual(
            IReadOnlyList<D5SdoQuarantineEvidence> left,
            IReadOnlyList<D5SdoQuarantineEvidence> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (var index = 0; index < left.Count; index++)
            {
                if (!left[index].ContentEquals(right[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateEvidence(
            uint diagnosticsBootId,
            uint mapRevision,
            ushort slaveReference,
            uint timeoutCycles,
            LMCConnection ownerConnection,
            string stage,
            string reason,
            string evidenceId)
        {
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            if (slaveReference < 1 || slaveReference > 4)
            {
                throw new ArgumentOutOfRangeException("slaveReference");
            }

            if (timeoutCycles < 1 || timeoutCycles > 60000)
            {
                throw new ArgumentOutOfRangeException("timeoutCycles");
            }

            if (ownerConnection == null)
            {
                throw new ArgumentNullException("ownerConnection");
            }

            if (string.IsNullOrWhiteSpace(stage))
            {
                throw new ArgumentException(
                    "D5 quarantine stage is required.",
                    "stage");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException(
                    "D5 quarantine reason is required.",
                    "reason");
            }

            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                throw new ArgumentException(
                    "D5 quarantine evidence id is required.",
                    "evidenceId");
            }
        }

        private static void RequireSdoReadTicket(LMCOperationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            if (ticket.TicketId == 0
                || ticket.DiagnosticsBootId == 0
                || ticket.OperationKind != LMCOperationKind.SDORead)
            {
                throw new ArgumentException(
                    "D5 quarantine requires a non-zero SDO Read ticket.",
                    "ticket");
            }
        }

        private static void RequireTicketOwner(
            LMCOperationTicket ticket,
            LMCConnection ownerConnection)
        {
            if (!ticket.BelongsTo(ownerConnection))
            {
                throw new InvalidOperationException(
                    "The D5 quarantine ticket belongs to a different LMCConnection.");
            }
        }

        private static void RequireEntryRevisionAvailable(Entry entry)
        {
            if (entry.EntryRevision == long.MaxValue)
            {
                throw new InvalidOperationException(
                    "The D5 quarantine entry revision is exhausted.");
            }
        }

        private void AdvanceVersion()
        {
            RequireVersionAvailable();
            version++;
        }

        private void RequireVersionAvailable()
        {
            if (version == long.MaxValue)
            {
                throw new InvalidOperationException(
                    "The D5 quarantine ledger version is exhausted.");
            }
        }

        private void RequireMutationAllowed()
        {
            if (proofCommitInProgress)
            {
                throw new InvalidOperationException(
                    "The D5 quarantine cannot mutate while recovery proof is committing.");
            }
        }
    }
}

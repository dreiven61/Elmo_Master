using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum D5SdoPendingCleanupDisposition
    {
        Resolved = 0,
        QuarantineDiagnosticsBootIdChanged = 1,
        QuarantineMapRevisionChanged = 2,
        QuarantineWriteTerminalOutcomeUnverified = 3
    }

    internal enum D5SdoTicketNotFoundDisposition
    {
        ResolveReadBySlotContract = 0,
        QuarantineWriteOutcomeUnverified = 1
    }

    internal enum D5SdoTerminalResolutionDisposition
    {
        Resolve = 0,
        QuarantineWriteOutcomeUnverified = 1
    }

    internal enum D5SdoWriteReadbackVerdict
    {
        Pending = 0,
        Verified = 1
    }

    internal sealed class D5SdoWriteReadbackRequirement
    {
        private readonly byte[] expectedWriteData;

        internal D5SdoWriteReadbackRequirement(
            LMCSdoRequest writeRequest,
            LMCOperationTicket writeTicket,
            LMCConnection ownerConnection)
        {
            if (writeRequest == null)
            {
                throw new ArgumentNullException("writeRequest");
            }

            if (writeTicket == null)
            {
                throw new ArgumentNullException("writeTicket");
            }

            if (ownerConnection == null)
            {
                throw new ArgumentNullException("ownerConnection");
            }

            if (!writeRequest.IsWrite
                || writeRequest.DataLength != 4
                || writeRequest.WriteData.Length != 4)
            {
                throw new ArgumentException(
                    "Pending SDO Write readback requires an exact four-byte Write request.",
                    "writeRequest");
            }

            if (writeTicket.OperationKind != LMCOperationKind.SDOWrite
                || !writeTicket.BelongsToCurrentSession(ownerConnection))
            {
                throw new ArgumentException(
                    "Pending SDO Write readback requires the accepted Write ticket from the owner's current connection session.",
                    "writeTicket");
            }

            SlaveReference = writeRequest.SlaveReference;
            ObjectIndex = writeRequest.ObjectIndex;
            SubIndex = writeRequest.SubIndex;
            ValueType = writeRequest.ValueType;
            DataLength = writeRequest.DataLength;
            TimeoutCycles = writeRequest.TimeoutCycles;
            expectedWriteData = writeRequest.WriteData;
            WriteTicket = writeTicket;
            OwnerConnection = ownerConnection;
            DiagnosticsBootId = writeTicket.DiagnosticsBootId;
            SubmissionMapRevision = writeTicket.SubmissionMapRevision;
        }

        internal ushort SlaveReference { get; private set; }
        internal ushort ObjectIndex { get; private set; }
        internal byte SubIndex { get; private set; }
        internal LMCSignalValueType ValueType { get; private set; }
        internal ushort DataLength { get; private set; }
        internal uint TimeoutCycles { get; private set; }
        internal LMCOperationTicket WriteTicket { get; private set; }
        internal LMCConnection OwnerConnection { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint SubmissionMapRevision { get; private set; }
        internal byte[] ExpectedWriteData
        {
            get { return (byte[])expectedWriteData.Clone(); }
        }

        internal LMCSdoRequest CreateReadRequest(uint timeoutCycles)
        {
            return LMCSdoRequest.CreateRead(
                SlaveReference,
                ObjectIndex,
                SubIndex,
                ValueType,
                DataLength,
                timeoutCycles);
        }

        internal bool MatchesReadRequest(LMCSdoRequest request)
        {
            return request != null
                && !request.IsWrite
                && request.SlaveReference == SlaveReference
                && request.ObjectIndex == ObjectIndex
                && request.SubIndex == SubIndex
                && request.ValueType == ValueType
                && request.DataLength == DataLength;
        }

        internal bool MatchesCurrentIdentity(
            LMCConnection currentConnection,
            LMCDiagnosticCapabilities freshCapabilities)
        {
            return MatchesOwnerCurrentSession(currentConnection)
                && freshCapabilities != null
                && freshCapabilities.DiagnosticsBootId
                    == DiagnosticsBootId
                && freshCapabilities.MapRevision
                    == SubmissionMapRevision;
        }

        internal bool MatchesOwnerCurrentSession(
            LMCConnection currentConnection)
        {
            return currentConnection != null
                && ReferenceEquals(OwnerConnection, currentConnection)
                && WriteTicket.BelongsToCurrentSession(currentConnection);
        }

        internal bool MatchesReadTicketIdentity(
            LMCOperationTicket readTicket,
            LMCConnection currentConnection,
            LMCDiagnosticCapabilities freshCapabilities)
        {
            return MatchesCurrentIdentity(
                    currentConnection,
                    freshCapabilities)
                && readTicket != null
                && readTicket.OperationKind == LMCOperationKind.SDORead
                && readTicket.BelongsToCurrentSession(currentConnection)
                && readTicket.DiagnosticsBootId == DiagnosticsBootId
                && readTicket.SubmissionMapRevision
                    == SubmissionMapRevision;
        }

        internal D5SdoWriteReadbackVerdict Evaluate(
            LMCSdoRequest request,
            LMCOperationTicket readTicket,
            LMCConnection currentConnection,
            LMCDiagnosticCapabilities freshCapabilities,
            LMCOperationStatus status)
        {
            if (!MatchesReadRequest(request)
                || !MatchesReadTicketIdentity(
                    readTicket,
                    currentConnection,
                    freshCapabilities)
                || status == null
                || status.TicketId != readTicket.TicketId
                || status.OperationKind != LMCOperationKind.SDORead
                || status.DiagnosticsBootId != DiagnosticsBootId
                || !status.IsSuccessful
                || status.ResultValueType != ValueType
                || status.ResultLength != DataLength
                || !ByteArraysEqual(status.ResultData, expectedWriteData))
            {
                return D5SdoWriteReadbackVerdict.Pending;
            }

            return D5SdoWriteReadbackVerdict.Verified;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal sealed class D5SdoPendingCleanupRequest
    {
        internal D5SdoPendingCleanupRequest(
            LMCOperationTicket ticket,
            LMCOperationStatus cachedStatus,
            LMCConnection ownerConnection,
            LMCConnection currentConnection,
            uint mapRevision,
            DateTime? deadlineUtc,
            bool queuedCancelAccepted = false)
        {
            Ticket = ticket;
            CachedStatus = cachedStatus;
            OwnerConnection = ownerConnection;
            CurrentConnection = currentConnection;
            MapRevision = mapRevision;
            DeadlineUtc = deadlineUtc;
            QueuedCancelAccepted = queuedCancelAccepted;
        }

        internal LMCOperationTicket Ticket { get; private set; }
        internal LMCOperationStatus CachedStatus { get; private set; }
        internal LMCConnection OwnerConnection { get; private set; }
        internal LMCConnection CurrentConnection { get; private set; }
        internal uint MapRevision { get; private set; }
        internal DateTime? DeadlineUtc { get; private set; }
        internal bool QueuedCancelAccepted { get; private set; }
    }

    internal sealed class D5SdoPendingCleanupOperations
    {
        internal Func<Task<LMCDiagnosticCapabilities>> ReadCapabilitiesAsync
        {
            get;
            set;
        }

        internal Func<LMCOperationTicket, Task<LMCOperationStatus>>
            ReadStatusAsync { get; set; }

        internal Func<LMCOperationTicket, Task> CancelAsync { get; set; }
        internal Func<int, Task> DelayAsync { get; set; }
        internal Func<DateTime> ReadUtcNow { get; set; }
        internal Func<long> ReadWaitElapsedMilliseconds { get; set; }
        internal Action<LMCOperationStatus> StatusObserved { get; set; }
        internal Action CleanupStarted { get; set; }
        internal Action CancelAccepted { get; set; }
        internal Action CancelRaceResolved { get; set; }
    }

    internal sealed class D5SdoPendingCleanupResult
    {
        internal D5SdoPendingCleanupResult(
            D5SdoPendingCleanupDisposition disposition,
            string quarantineReason,
            LMCOperationStatus status,
            bool usedCachedTerminal,
            bool cancelAttempted,
            bool cancelAccepted,
            bool cancelRaceResolved,
            int statusReadCount,
            int waitMilliseconds)
        {
            Disposition = disposition;
            QuarantineReason = quarantineReason;
            Status = status;
            UsedCachedTerminal = usedCachedTerminal;
            CancelAttempted = cancelAttempted;
            CancelAccepted = cancelAccepted;
            CancelRaceResolved = cancelRaceResolved;
            StatusReadCount = statusReadCount;
            WaitMilliseconds = waitMilliseconds;
        }

        internal D5SdoPendingCleanupDisposition Disposition
        {
            get;
            private set;
        }

        internal string QuarantineReason { get; private set; }
        internal LMCOperationStatus Status { get; private set; }
        internal bool UsedCachedTerminal { get; private set; }
        internal bool CancelAttempted { get; private set; }
        internal bool CancelAccepted { get; private set; }
        internal bool CancelRaceResolved { get; private set; }
        internal int StatusReadCount { get; private set; }
        internal int WaitMilliseconds { get; private set; }

        internal bool IsResolved
        {
            get
            {
                return Disposition
                    == D5SdoPendingCleanupDisposition.Resolved;
            }
        }
    }

    internal static class D5SdoPendingCleanupOrchestrator
    {
        internal const int MinimumWaitMilliseconds = 15000;
        internal const int MaximumWaitMilliseconds = 120000;

        internal static D5SdoTicketNotFoundDisposition
            EvaluateTicketNotFound(LMCOperationTicket ticket)
        {
            RequireSdoTicket(ticket);
            return ticket.OperationKind == LMCOperationKind.SDOWrite
                ? D5SdoTicketNotFoundDisposition
                    .QuarantineWriteOutcomeUnverified
                : D5SdoTicketNotFoundDisposition
                    .ResolveReadBySlotContract;
        }

        internal static D5SdoTerminalResolutionDisposition
            EvaluateTerminalResolution(
                LMCOperationTicket ticket,
                LMCOperationStatus status,
                bool queuedCancelAccepted,
                bool cancelRaceResolved)
        {
            RequireSdoTicket(ticket);
            ValidateStatus(ticket, status, "status");
            if (!status.IsTerminal)
            {
                throw new InvalidOperationException(
                    "D5 SDO terminal resolution requires a terminal status.");
            }

            if (ticket.OperationKind != LMCOperationKind.SDOWrite)
            {
                return D5SdoTerminalResolutionDisposition.Resolve;
            }

            if (cancelRaceResolved)
            {
                return D5SdoTerminalResolutionDisposition
                    .QuarantineWriteOutcomeUnverified;
            }

            if (queuedCancelAccepted)
            {
                return status.State == LMCOperationState.Cancelled
                    && status.Outcome == LMCOperationOutcome.Cancelled
                        ? D5SdoTerminalResolutionDisposition.Resolve
                        : D5SdoTerminalResolutionDisposition
                            .QuarantineWriteOutcomeUnverified;
            }

            return status.State == LMCOperationState.Completed
                && status.Outcome == LMCOperationOutcome.Success
                    ? D5SdoTerminalResolutionDisposition.Resolve
                    : D5SdoTerminalResolutionDisposition
                        .QuarantineWriteOutcomeUnverified;
        }

        internal static async Task<D5SdoPendingCleanupResult> CleanupAsync(
            D5SdoPendingCleanupRequest request,
            D5SdoPendingCleanupOperations operations,
            int pollMilliseconds)
        {
            ValidateRequest(request);
            ValidateOperations(operations, pollMilliseconds);

            var capabilities = await operations.ReadCapabilitiesAsync();
            if (capabilities == null
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "D5 ticket cleanup requires a non-zero current DiagnosticsBootId and MapRevision.");
            }

            if (capabilities.DiagnosticsBootId
                != request.Ticket.DiagnosticsBootId)
            {
                return Quarantine(
                    D5SdoPendingCleanupDisposition
                        .QuarantineDiagnosticsBootIdChanged,
                    "diagnostics_boot_id_changed");
            }

            if (capabilities.MapRevision != request.MapRevision)
            {
                return Quarantine(
                    D5SdoPendingCleanupDisposition
                        .QuarantineMapRevisionChanged,
                    "diagnostics_map_revision_changed");
            }

            if (operations.CleanupStarted != null)
            {
                operations.CleanupStarted();
            }

            var status = request.CachedStatus;
            var usedCachedTerminal = status != null && status.IsTerminal;
            var statusReadCount = 0;
            if (status == null || !status.IsTerminal)
            {
                status = await ReadStatusAsync(
                    request.Ticket,
                    operations);
                statusReadCount++;
            }

            var cancelAttempted = request.QueuedCancelAccepted;
            var cancelAccepted = request.QueuedCancelAccepted;
            var cancelAcceptedThisCleanup = false;
            var cancelRaceResolved = false;
            if (status.State == LMCOperationState.Queued
                && !cancelAccepted)
            {
                cancelAttempted = true;
                try
                {
                    await operations.CancelAsync(request.Ticket);
                    cancelAccepted = true;
                    cancelAcceptedThisCleanup = true;
                    if (operations.CancelAccepted != null)
                    {
                        operations.CancelAccepted();
                    }
                }
                catch (LMCDiagnosticsCommandException error)
                {
                    if (error.Response == null
                        || error.Response.Detail
                            != LMCDiagnosticsDetailCode.InvalidState)
                    {
                        throw;
                    }

                    cancelRaceResolved = true;
                    if (operations.CancelRaceResolved != null)
                    {
                        operations.CancelRaceResolved();
                    }
                }
            }

            var waitMilliseconds = 0;
            if (!status.IsTerminal || cancelAcceptedThisCleanup)
            {
                waitMilliseconds = CalculateWaitMilliseconds(
                    request.DeadlineUtc,
                    operations.ReadUtcNow());
                var waitResult = await WaitForTerminalAsync(
                    request.Ticket,
                    operations,
                    waitMilliseconds,
                    pollMilliseconds);
                status = waitResult.Status;
                statusReadCount += waitResult.StatusReadCount;
            }

            if (!status.IsTerminal)
            {
                throw new TimeoutException(
                    "D5 SDO cleanup did not resolve the pending ticket.");
            }

            var terminalResolution = EvaluateTerminalResolution(
                request.Ticket,
                status,
                cancelAccepted,
                cancelRaceResolved);
            if (terminalResolution
                == D5SdoTerminalResolutionDisposition
                    .QuarantineWriteOutcomeUnverified)
            {
                return new D5SdoPendingCleanupResult(
                    D5SdoPendingCleanupDisposition
                        .QuarantineWriteTerminalOutcomeUnverified,
                    "write_terminal_outcome_unverified",
                    status,
                    usedCachedTerminal,
                    cancelAttempted,
                    cancelAccepted,
                    cancelRaceResolved,
                    statusReadCount,
                    waitMilliseconds);
            }

            if (cancelAccepted
                && request.Ticket.OperationKind
                    != LMCOperationKind.SDOWrite
                && (status.State != LMCOperationState.Cancelled
                    || status.Outcome != LMCOperationOutcome.Cancelled))
            {
                throw new InvalidOperationException(
                    "CancelOperation was accepted but the D5 SDO ticket did not become Cancelled/Cancelled.");
            }

            return new D5SdoPendingCleanupResult(
                D5SdoPendingCleanupDisposition.Resolved,
                null,
                status,
                usedCachedTerminal,
                cancelAttempted,
                cancelAccepted,
                cancelRaceResolved,
                statusReadCount,
                waitMilliseconds);
        }

        internal static int CalculateWaitMilliseconds(
            DateTime? deadlineUtc,
            DateTime currentUtc)
        {
            var remainingMilliseconds = 0L;
            if (deadlineUtc.HasValue)
            {
                remainingMilliseconds = Math.Max(
                    0L,
                    (long)Math.Ceiling(
                        (deadlineUtc.Value - currentUtc)
                            .TotalMilliseconds));
            }

            var requested = Math.Max(
                MinimumWaitMilliseconds,
                remainingMilliseconds + 1000L);
            return checked((int)Math.Min(
                MaximumWaitMilliseconds,
                requested));
        }

        private static async Task<D5SdoPendingWaitResult>
            WaitForTerminalAsync(
                LMCOperationTicket ticket,
                D5SdoPendingCleanupOperations operations,
                int timeoutMilliseconds,
                int pollMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();
            Func<long> readElapsed = operations.ReadWaitElapsedMilliseconds
                ?? (() => stopwatch.ElapsedMilliseconds);
            LMCOperationStatus status = null;
            var statusReadCount = 0;
            while (readElapsed() <= timeoutMilliseconds)
            {
                status = await ReadStatusAsync(ticket, operations);
                statusReadCount++;
                if (status.IsTerminal)
                {
                    return new D5SdoPendingWaitResult(
                        status,
                        statusReadCount);
                }

                await operations.DelayAsync(pollMilliseconds);
            }

            throw new TimeoutException(
                "D5 SDO ticket "
                + ticket.TicketId.ToString(CultureInfo.InvariantCulture)
                + " remained "
                + (status == null ? "unknown" : status.State.ToString())
                + " beyond the "
                + timeoutMilliseconds.ToString(
                    CultureInfo.InvariantCulture)
                + " ms cleanup bound. The Cancel Test button does not send a PLC Stop command.");
        }

        private static async Task<LMCOperationStatus> ReadStatusAsync(
            LMCOperationTicket ticket,
            D5SdoPendingCleanupOperations operations)
        {
            var status = await operations.ReadStatusAsync(ticket);
            ValidateStatus(ticket, status, "status");
            if (operations.StatusObserved != null)
            {
                operations.StatusObserved(status);
            }

            return status;
        }

        private static D5SdoPendingCleanupResult Quarantine(
            D5SdoPendingCleanupDisposition disposition,
            string reason)
        {
            return new D5SdoPendingCleanupResult(
                disposition,
                reason,
                null,
                false,
                false,
                false,
                false,
                0,
                0);
        }

        private static void ValidateRequest(
            D5SdoPendingCleanupRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Ticket == null)
            {
                throw new ArgumentException(
                    "D5 cleanup ticket is required.",
                    "request");
            }

            RequireSdoTicket(request.Ticket);

            if (request.CurrentConnection == null)
            {
                throw new ArgumentException(
                    "D5 cleanup current connection is required.",
                    "request");
            }

            if (request.OwnerConnection == null
                || !ReferenceEquals(
                    request.OwnerConnection,
                    request.CurrentConnection)
                || !request.Ticket.BelongsTo(request.CurrentConnection))
            {
                throw new InvalidOperationException(
                    "The preserved D5 ticket belongs to an earlier LMCConnection. Use the quarantine recovery proof instead of querying it from the new session.");
            }

            if (request.MapRevision == 0
                || request.Ticket.SubmissionMapRevision
                    != request.MapRevision)
            {
                throw new InvalidOperationException(
                    "The preserved D5 ticket has an invalid or inconsistent submission MapRevision.");
            }

            if (request.CachedStatus != null)
            {
                ValidateStatus(
                    request.Ticket,
                    request.CachedStatus,
                    "cachedStatus");
            }
        }

        private static void RequireSdoTicket(LMCOperationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            if (ticket.OperationKind != LMCOperationKind.SDORead
                && ticket.OperationKind != LMCOperationKind.SDOWrite)
            {
                throw new InvalidOperationException(
                    "D5 SDO cleanup requires an SDO Read or SDO Write ticket.");
            }
        }

        private static void ValidateOperations(
            D5SdoPendingCleanupOperations operations,
            int pollMilliseconds)
        {
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            if (operations.ReadCapabilitiesAsync == null
                || operations.ReadStatusAsync == null
                || operations.CancelAsync == null
                || operations.DelayAsync == null
                || operations.ReadUtcNow == null)
            {
                throw new ArgumentException(
                    "D5 cleanup capability, status, cancel, delay, and clock delegates are required.",
                    "operations");
            }

            if (pollMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("pollMilliseconds");
            }
        }

        private static void ValidateStatus(
            LMCOperationTicket ticket,
            LMCOperationStatus status,
            string argumentName)
        {
            if (status == null
                || status.TicketId != ticket.TicketId
                || status.OperationKind != ticket.OperationKind
                || status.DiagnosticsBootId != ticket.DiagnosticsBootId)
            {
                throw new InvalidOperationException(
                    "D5 cleanup "
                    + argumentName
                    + " does not match the preserved ticket identity.");
            }
        }

        private sealed class D5SdoPendingWaitResult
        {
            internal D5SdoPendingWaitResult(
                LMCOperationStatus status,
                int statusReadCount)
            {
                Status = status;
                StatusReadCount = statusReadCount;
            }

            internal LMCOperationStatus Status { get; private set; }
            internal int StatusReadCount { get; private set; }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public enum LMCSdoWriteVerificationVerdict
    {
        Pending = 0,
        Verified = 1
    }

    public sealed class LMCSdoWriteVerificationContext
    {
        private readonly LMCDiagnostics owner;
        private readonly byte[] expectedWriteData;
        private readonly long capabilityObservationSequenceBaseline;

        internal LMCSdoWriteVerificationContext(
            LMCDiagnostics owner,
            LMCSdoRequest writeRequest,
            LMCOperationTicket writeTicket,
            long capabilityObservationSequenceBaseline)
        {
            this.owner = owner ?? throw new ArgumentNullException("owner");
            if (writeRequest == null)
            {
                throw new ArgumentNullException("writeRequest");
            }

            WriteTicket = writeTicket
                ?? throw new ArgumentNullException("writeTicket");
            SlaveReference = writeRequest.SlaveReference;
            ObjectIndex = writeRequest.ObjectIndex;
            SubIndex = writeRequest.SubIndex;
            ValueType = writeRequest.ValueType;
            DataLength = writeRequest.DataLength;
            TimeoutCycles = writeRequest.TimeoutCycles;
            expectedWriteData = writeRequest.WriteData;
            ConnectionSessionGeneration =
                writeTicket.ConnectionSessionGeneration;
            DiagnosticsBootId = writeTicket.DiagnosticsBootId;
            SubmissionMapRevision =
                writeTicket.SubmissionMapRevision;
            this.capabilityObservationSequenceBaseline =
                capabilityObservationSequenceBaseline;
        }

        public ushort SlaveReference { get; private set; }
        public ushort ObjectIndex { get; private set; }
        public byte SubIndex { get; private set; }
        public LMCSignalValueType ValueType { get; private set; }
        public ushort DataLength { get; private set; }
        public uint TimeoutCycles { get; private set; }
        public LMCOperationTicket WriteTicket { get; private set; }
        public long ConnectionSessionGeneration { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint SubmissionMapRevision { get; private set; }
        public byte[] ExpectedWriteData
        {
            get { return (byte[])expectedWriteData.Clone(); }
        }

        public LMCSdoRequest CreateReadRequest()
        {
            return CreateReadRequest(TimeoutCycles);
        }

        public LMCSdoRequest CreateReadRequest(uint timeoutCycles)
        {
            return LMCSdoRequest.CreateRead(
                SlaveReference,
                ObjectIndex,
                SubIndex,
                ValueType,
                DataLength,
                timeoutCycles);
        }

        public bool MatchesReadRequest(LMCSdoRequest request)
        {
            return request != null
                && !request.IsWrite
                && request.SlaveReference == SlaveReference
                && request.ObjectIndex == ObjectIndex
                && request.SubIndex == SubIndex
                && request.ValueType == ValueType
                && request.DataLength == DataLength;
        }

        public bool MatchesOwnerCurrentSession(
            LMCConnection currentConnection)
        {
            return currentConnection != null
                && currentConnection.IsConnected
                && ReferenceEquals(owner, currentConnection.Diagnostics)
                && WriteTicket.BelongsToCurrentSession(currentConnection)
                && currentConnection.SessionGeneration
                    == ConnectionSessionGeneration;
        }

        public bool MatchesCurrentIdentity(
            LMCConnection currentConnection,
            LMCDiagnosticCapabilities freshCapabilities)
        {
            return MatchesOwnerCurrentSession(currentConnection)
                && freshCapabilities != null
                && freshCapabilities.IsBoundTo(
                    owner,
                    ConnectionSessionGeneration)
                && freshCapabilities.ObservationSequence
                    > capabilityObservationSequenceBaseline
                && freshCapabilities.ConnectionSessionGeneration
                    == ConnectionSessionGeneration
                && freshCapabilities.DiagnosticsBootId
                    == DiagnosticsBootId
                && freshCapabilities.MapRevision
                    == SubmissionMapRevision;
        }

        public bool MatchesReadTicketIdentity(
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
                    == SubmissionMapRevision
                && readTicket.RequestedResultLength == DataLength
                && readTicket.ResultValueType == ValueType
                && MatchesReadRequest(readTicket.SubmittedSdoRequest);
        }

        public LMCOperationTicket SubmitReadback(
            LMCSdoRequest readRequest)
        {
            RequireMatchingReadRequest(readRequest);
            return owner.SubmitSdo(readRequest, WriteTicket);
        }

        public LMCOperationTicket SubmitReadback(uint timeoutCycles)
        {
            return SubmitReadback(CreateReadRequest(timeoutCycles));
        }

        public Task<LMCOperationTicket> SubmitReadbackAsync(
            LMCSdoRequest readRequest,
            CancellationToken cancellationToken)
        {
            RequireMatchingReadRequest(readRequest);
            return owner.SubmitSdoAsync(
                readRequest,
                WriteTicket,
                cancellationToken);
        }

        public Task<LMCOperationTicket> SubmitReadbackAsync(
            uint timeoutCycles,
            CancellationToken cancellationToken)
        {
            return SubmitReadbackAsync(
                CreateReadRequest(timeoutCycles),
                cancellationToken);
        }

        public LMCSdoWriteVerificationVerdict Evaluate(
            LMCSdoRequest readRequest,
            LMCOperationTicket readTicket,
            LMCConnection currentConnection,
            LMCDiagnosticCapabilities freshCapabilities,
            LMCOperationStatus status)
        {
            if (!MatchesReadRequest(readRequest)
                || !MatchesReadTicketIdentity(
                    readTicket,
                    currentConnection,
                    freshCapabilities)
                || !RequestsEqual(
                    readRequest,
                    readTicket.SubmittedSdoRequest)
                || status == null
                || !status.IsBoundTo(
                    owner,
                    ConnectionSessionGeneration)
                || status.TicketId != readTicket.TicketId
                || status.OperationKind != LMCOperationKind.SDORead
                || status.SubmitCycle != readTicket.QueuedCycle
                || status.DiagnosticsBootId != DiagnosticsBootId
                || !status.IsSuccessful
                || status.ResultValueType != ValueType
                || status.ResultLength != DataLength
                || !ByteArraysEqual(
                    status.ResultData,
                    expectedWriteData))
            {
                return LMCSdoWriteVerificationVerdict.Pending;
            }

            return LMCSdoWriteVerificationVerdict.Verified;
        }

        internal static bool RequestsEqual(
            LMCSdoRequest left,
            LMCSdoRequest right)
        {
            return left != null
                && right != null
                && left.SlaveReference == right.SlaveReference
                && left.OperationFlags == right.OperationFlags
                && left.ObjectIndex == right.ObjectIndex
                && left.SubIndex == right.SubIndex
                && left.ValueType == right.ValueType
                && left.TimeoutCycles == right.TimeoutCycles
                && left.DataLength == right.DataLength
                && ByteArraysEqual(
                    left.WriteDataUnsafe,
                    right.WriteDataUnsafe);
        }

        internal static bool ReadMatchesWriteTarget(
            LMCSdoRequest readRequest,
            LMCSdoRequest writeRequest)
        {
            return readRequest != null
                && writeRequest != null
                && !readRequest.IsWrite
                && writeRequest.IsWrite
                && readRequest.SlaveReference
                    == writeRequest.SlaveReference
                && readRequest.ObjectIndex == writeRequest.ObjectIndex
                && readRequest.SubIndex == writeRequest.SubIndex
                && readRequest.ValueType == writeRequest.ValueType
                && readRequest.DataLength == writeRequest.DataLength;
        }

        private void RequireMatchingReadRequest(LMCSdoRequest readRequest)
        {
            if (!MatchesReadRequest(readRequest))
            {
                throw new ArgumentException(
                    "The SDO readback request does not match this Write verification context.",
                    "readRequest");
            }
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

    public sealed partial class LMCDiagnostics
    {
        public LMCSdoWriteVerificationContext
            CreateSdoWriteVerificationContext(
                LMCSdoRequest approvedWriteRequest,
                LMCOperationTicket acceptedWriteTicket,
                LMCOperationStatus writeTerminalStatus)
        {
            return CreateSdoWriteVerificationContext(
                approvedWriteRequest,
                acceptedWriteTicket,
                writeTerminalStatus,
                request =>
                {
                    LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(
                        request);
                    return true;
                });
        }

        internal LMCSdoWriteVerificationContext
            CreateSdoWriteVerificationContext(
                LMCSdoRequest approvedWriteRequest,
                LMCOperationTicket acceptedWriteTicket,
                LMCOperationStatus writeTerminalStatus,
                Func<LMCSdoRequest, bool> approvalPredicate)
        {
            if (approvedWriteRequest == null)
            {
                throw new ArgumentNullException("approvedWriteRequest");
            }

            if (acceptedWriteTicket == null)
            {
                throw new ArgumentNullException("acceptedWriteTicket");
            }

            if (writeTerminalStatus == null)
            {
                throw new ArgumentNullException("writeTerminalStatus");
            }

            if (approvalPredicate == null)
            {
                throw new ArgumentNullException("approvalPredicate");
            }

            ValidateSdoWritePolicy(approvedWriteRequest);
            var expectedWriteLength = LMCDiagnosticsSdoPolicy
                .ExpectedReadLength(approvedWriteRequest.ValueType);
            if (!approvedWriteRequest.IsWrite
                || approvedWriteRequest.DataLength != expectedWriteLength
                || approvedWriteRequest.WriteDataUnsafe.Length
                    != expectedWriteLength)
            {
                throw new ArgumentException(
                    "SDO Write verification requires an exact canonical 1/2/4-byte scalar Write request.",
                    "approvedWriteRequest");
            }

            var sessionGeneration = connection.SessionGeneration;
            if (acceptedWriteTicket.OperationKind
                    != LMCOperationKind.SDOWrite
                || !ReferenceEquals(acceptedWriteTicket.Owner, this)
                || acceptedWriteTicket.ConnectionSessionGeneration
                    != sessionGeneration
                || acceptedWriteTicket.DiagnosticsBootId == 0
                || acceptedWriteTicket.SubmissionMapRevision == 0)
            {
                throw new ArgumentException(
                    "SDO Write verification requires an accepted Write ticket from this diagnostics owner's current session.",
                    "acceptedWriteTicket");
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            if (acceptedWriteTicket.SubmittedSdoRequest == null
                || !LMCSdoWriteVerificationContext.RequestsEqual(
                    approvedWriteRequest,
                    acceptedWriteTicket.SubmittedSdoRequest))
            {
                throw new ArgumentException(
                    "The accepted Write ticket was not created from the supplied SDO Write request.",
                    "acceptedWriteTicket");
            }

            if (!writeTerminalStatus.IsBoundTo(
                    this,
                    sessionGeneration)
                || writeTerminalStatus.TicketId
                    != acceptedWriteTicket.TicketId
                || writeTerminalStatus.OperationKind
                    != LMCOperationKind.SDOWrite
                || writeTerminalStatus.SubmitCycle
                    != acceptedWriteTicket.QueuedCycle
                || writeTerminalStatus.DiagnosticsBootId
                    != acceptedWriteTicket.DiagnosticsBootId
                || !writeTerminalStatus.IsSuccessful
                || writeTerminalStatus.ResultLength != 0
                || writeTerminalStatus.ResultValueType
                    != LMCSignalValueType.Invalid
                || writeTerminalStatus.ResultData.Length != 0)
            {
                throw new ArgumentException(
                    "SDO Write verification requires the exact owner/session-bound Completed+Success terminal status for the accepted Write ticket.",
                    "writeTerminalStatus");
            }

            if (!approvalPredicate(approvedWriteRequest))
            {
                throw new InvalidOperationException(
                    "The SDO Write request is not approved for verification context creation.");
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCSdoWriteVerificationContext(
                this,
                approvedWriteRequest,
                acceptedWriteTicket,
                CurrentCapabilityObservationSequence);
        }
    }
}

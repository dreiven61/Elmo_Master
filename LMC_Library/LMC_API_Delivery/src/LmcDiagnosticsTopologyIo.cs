using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        public LMCEtherCATTopologyInfo GetEtherCATTopologyInfo()
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities(sessionGeneration);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology,
                LMC_DiagnosticsFrame.CommonRequestPayloadLength,
                LMC_DiagnosticsParser.TopologyInfoPayloadLength,
                "GetEtherCATTopologyInfo",
                false);
            return GetEtherCATTopologyInfo(sessionGeneration);
        }

        public async Task<LMCEtherCATTopologyInfo> GetEtherCATTopologyInfoAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology,
                LMC_DiagnosticsFrame.CommonRequestPayloadLength,
                LMC_DiagnosticsParser.TopologyInfoPayloadLength,
                "GetEtherCATTopologyInfo",
                false);
            return await GetEtherCATTopologyInfoAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
        }

        public LMCEtherCATTopologyChunk GetEtherCATTopologyChunk(
            uint expectedTopologyRevision,
            ushort startIndex,
            ushort maxEntries)
        {
            ValidateTopologyChunkRequest(expectedTopologyRevision, maxEntries);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities(sessionGeneration);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology,
                LMC_DiagnosticsFrame.TopologyChunkRequestPayloadLength,
                checked(LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength
                    + maxEntries * LMC_DiagnosticsParser.TopologyEntryStride),
                "GetEtherCATTopologyChunk",
                false);
            return GetEtherCATTopologyChunk(
                sessionGeneration,
                expectedTopologyRevision,
                startIndex,
                maxEntries);
        }

        public async Task<LMCEtherCATTopologyChunk> GetEtherCATTopologyChunkAsync(
            uint expectedTopologyRevision,
            ushort startIndex,
            ushort maxEntries,
            CancellationToken cancellationToken)
        {
            ValidateTopologyChunkRequest(expectedTopologyRevision, maxEntries);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology,
                LMC_DiagnosticsFrame.TopologyChunkRequestPayloadLength,
                checked(LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength
                    + maxEntries * LMC_DiagnosticsParser.TopologyEntryStride),
                "GetEtherCATTopologyChunk",
                false);
            return await GetEtherCATTopologyChunkAsync(
                sessionGeneration,
                expectedTopologyRevision,
                startIndex,
                maxEntries,
                cancellationToken).ConfigureAwait(false);
        }

        public LMCEtherCATTopology GetEtherCATTopology()
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities(sessionGeneration);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology,
                LMC_DiagnosticsFrame.TopologyChunkRequestPayloadLength,
                LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength
                    + LMC_DiagnosticsParser.TopologyEntryStride,
                "GetEtherCATTopology",
                false);

            var info = GetEtherCATTopologyInfo(sessionGeneration);
            var entries = DownloadEtherCATTopology(
                sessionGeneration,
                info,
                GetTopologyChunkLimit(capabilities, info));
            ValidateCompleteTopology(info, entries);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCEtherCATTopology(info, entries).BindProvenance(
                this,
                sessionGeneration);
        }

        public async Task<LMCEtherCATTopology> GetEtherCATTopologyAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology,
                LMC_DiagnosticsFrame.TopologyChunkRequestPayloadLength,
                LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength
                    + LMC_DiagnosticsParser.TopologyEntryStride,
                "GetEtherCATTopology",
                false);

            var info = await GetEtherCATTopologyInfoAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var entries = await DownloadEtherCATTopologyAsync(
                sessionGeneration,
                info,
                GetTopologyChunkLimit(capabilities, info),
                cancellationToken).ConfigureAwait(false);
            ValidateCompleteTopology(info, entries);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCEtherCATTopology(info, entries).BindProvenance(
                this,
                sessionGeneration);
        }

        /// <summary>
        /// Reads one node by raw topology identity. This compatibility
        /// overload validates the wire echo and coherent response shape only;
        /// it cannot validate the node-kind-specific meaning without a
        /// topology model. Prefer the topology-bound overload.
        /// </summary>
        public LMCEtherCATNodeHealth ReadEtherCATNodeHealth(
            uint topologyRevision,
            uint nodeId)
        {
            ValidateTopologyNodeIdentity(topologyRevision, nodeId);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities(sessionGeneration);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                LMC_DiagnosticsFrame.NodeHealthRequestPayloadLength,
                LMC_DiagnosticsParser.NodeHealthPayloadLength,
                "ReadEtherCATNodeHealth",
                false);

            return ReadEtherCATNodeHealthCore(
                topologyRevision,
                nodeId,
                sessionGeneration);
        }

        /// <summary>
        /// Reads one node and fail-closed validates the response against the
        /// supplied topology entry, including DS402 payload presence.
        /// </summary>
        public LMCEtherCATNodeHealth ReadEtherCATNodeHealth(
            uint nodeId,
            LMCEtherCATTopology topology)
        {
            RequireCurrentTopologyNode(topology, nodeId);
            var sessionGeneration = topology.ConnectionSessionGeneration;
            var capabilities = GetCapabilities(sessionGeneration);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                LMC_DiagnosticsFrame.NodeHealthRequestPayloadLength,
                LMC_DiagnosticsParser.NodeHealthPayloadLength,
                "ReadEtherCATNodeHealth",
                false);
            var health = ReadEtherCATNodeHealthCore(
                topology.TopologyRevision,
                nodeId,
                sessionGeneration);
            topology.ValidateNodeHealth(health);
            return health;
        }

        /// <summary>
        /// Asynchronously reads one node by raw topology identity. This
        /// compatibility overload validates the wire echo and coherent
        /// response shape only; prefer the topology-bound overload when the
        /// node-kind-specific meaning must be validated.
        /// </summary>
        public async Task<LMCEtherCATNodeHealth> ReadEtherCATNodeHealthAsync(
            uint topologyRevision,
            uint nodeId,
            CancellationToken cancellationToken)
        {
            ValidateTopologyNodeIdentity(topologyRevision, nodeId);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                LMC_DiagnosticsFrame.NodeHealthRequestPayloadLength,
                LMC_DiagnosticsParser.NodeHealthPayloadLength,
                "ReadEtherCATNodeHealth",
                false);
            return await ReadEtherCATNodeHealthCoreAsync(
                topologyRevision,
                nodeId,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads one node and fail-closed validates the
        /// response against the supplied topology entry, including DS402
        /// payload presence.
        /// </summary>
        public async Task<LMCEtherCATNodeHealth>
            ReadEtherCATNodeHealthAsync(
                uint nodeId,
                LMCEtherCATTopology topology,
                CancellationToken cancellationToken)
        {
            RequireCurrentTopologyNode(topology, nodeId);
            var sessionGeneration = topology.ConnectionSessionGeneration;
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                LMC_DiagnosticsFrame.NodeHealthRequestPayloadLength,
                LMC_DiagnosticsParser.NodeHealthPayloadLength,
                "ReadEtherCATNodeHealth",
                false);
            var health = await ReadEtherCATNodeHealthCoreAsync(
                topology.TopologyRevision,
                nodeId,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            topology.ValidateNodeHealth(health);
            return health;
        }

        /// <summary>
        /// Asynchronously reads one topology-bound node using a capability
        /// snapshot already observed from this diagnostics facade in the
        /// current connection session. The snapshot, capability bits, payload
        /// limits, and topology entry are validated before the single node
        /// health exchange; capabilities are not read again from the wire.
        /// </summary>
        public async Task<LMCEtherCATNodeHealth>
            ReadEtherCATNodeHealthAsync(
                uint nodeId,
                LMCEtherCATTopology topology,
                LMCDiagnosticCapabilities capabilities,
                CancellationToken cancellationToken)
        {
            RequireCurrentTopologyNode(topology, nodeId);
            var sessionGeneration = topology.ConnectionSessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            ValidatePinnedTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                LMC_DiagnosticsFrame.NodeHealthRequestPayloadLength,
                LMC_DiagnosticsParser.NodeHealthPayloadLength,
                "ReadEtherCATNodeHealth");

            var health = await ReadEtherCATNodeHealthCoreAsync(
                topology.TopologyRevision,
                nodeId,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            topology.ValidateNodeHealth(health);
            return health;
        }

        /// <summary>
        /// Reads digital I/O by a raw request identity. This compatibility
        /// overload validates the response echo and coherent value shape only;
        /// it cannot bind IOReference and response NodeId to a topology entry.
        /// Prefer the topology-bound overload.
        /// </summary>
        public LMCDigitalIOValue ReadDigitalIO(
            LMCDigitalIOReadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities(sessionGeneration);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength,
                LMC_DiagnosticsParser.DigitalIOPayloadLength,
                "ReadDigitalIO",
                false);

            return ReadDigitalIOCore(
                request,
                capabilities,
                sessionGeneration);
        }

        /// <summary>
        /// Validates the request against the supplied topology before wire
        /// access, reads the value, then validates IOReference, NodeId,
        /// direction, and width against the same topology.
        /// </summary>
        public LMCDigitalIOValue ReadDigitalIO(
            LMCEtherCATTopology topology,
            LMCDigitalIOReadRequest request)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            topology.ValidateDigitalIOReadRequest(request);
            RequireCurrentTopology(topology);
            var sessionGeneration = topology.ConnectionSessionGeneration;
            var capabilities = GetCapabilities(sessionGeneration);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength,
                LMC_DiagnosticsParser.DigitalIOPayloadLength,
                "ReadDigitalIO",
                false);
            var value = ReadDigitalIOCore(
                request,
                capabilities,
                sessionGeneration);
            return value.BindToTopology(topology);
        }

        /// <summary>
        /// Asynchronously reads digital I/O by a raw request identity. This
        /// compatibility overload validates the response echo and coherent
        /// value shape only; prefer the topology-bound overload when topology
        /// entry binding is required.
        /// </summary>
        public async Task<LMCDigitalIOValue> ReadDigitalIOAsync(
            LMCDigitalIOReadRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength,
                LMC_DiagnosticsParser.DigitalIOPayloadLength,
                "ReadDigitalIO",
                false);
            return await ReadDigitalIOCoreAsync(
                request,
                capabilities,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously validates the request against the supplied topology
        /// before wire access, reads the value, then validates IOReference,
        /// NodeId, direction, and width against the same topology.
        /// </summary>
        public async Task<LMCDigitalIOValue> ReadDigitalIOAsync(
            LMCEtherCATTopology topology,
            LMCDigitalIOReadRequest request,
            CancellationToken cancellationToken)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            topology.ValidateDigitalIOReadRequest(request);
            RequireCurrentTopology(topology);
            var sessionGeneration = topology.ConnectionSessionGeneration;
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength,
                LMC_DiagnosticsParser.DigitalIOPayloadLength,
                "ReadDigitalIO",
                false);
            var value = await ReadDigitalIOCoreAsync(
                request,
                capabilities,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return value.BindToTopology(topology);
        }

        /// <summary>
        /// Asynchronously reads topology-bound digital I/O using a capability
        /// snapshot already observed from this diagnostics facade in the
        /// current connection session. The snapshot, capability bits, payload
        /// limits, and topology request are validated before the single
        /// digital-I/O exchange; capabilities are not read again from the wire.
        /// </summary>
        public async Task<LMCDigitalIOValue> ReadDigitalIOAsync(
            LMCEtherCATTopology topology,
            LMCDigitalIOReadRequest request,
            LMCDiagnosticCapabilities capabilities,
            CancellationToken cancellationToken)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            topology.ValidateDigitalIOReadRequest(request);
            RequireCurrentTopology(topology);
            var sessionGeneration = topology.ConnectionSessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            ValidatePinnedTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength,
                LMC_DiagnosticsParser.DigitalIOPayloadLength,
                "ReadDigitalIO");

            var value = await ReadDigitalIOCoreAsync(
                request,
                capabilities,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return value.BindToTopology(topology);
        }

        /// <summary>
        /// Creates an executable, fail-closed output request from a valid
        /// topology-bound output snapshot read by this diagnostics facade in
        /// the current connection session. Raw digital-I/O observations cannot
        /// authorize a write request.
        /// </summary>
        public LMCDigitalOutputWriteRequest CreateDigitalOutputWriteRequest(
            LMCDigitalIOValue outputSnapshot,
            ulong value,
            ulong mask)
        {
            if (outputSnapshot == null)
            {
                throw new ArgumentNullException("outputSnapshot");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            ValidateDigitalOutputWriteSnapshot(
                outputSnapshot,
                sessionGeneration);
            return LMCDigitalOutputWriteRequest.FromOutputSnapshot(
                outputSnapshot,
                value,
                mask);
        }

        /// <summary>
        /// Returns the immutable compile-time digital-output IOReference list.
        /// An empty list blocks all output writes before command 0x7E23.
        /// </summary>
        public IReadOnlyList<uint> GetApprovedDigitalOutputWriteReferences()
        {
            return LMCDiagnosticsDigitalOutputWritePolicy.GetApprovedIOReferences();
        }

        public LMCOperationTicket SubmitDigitalOutputWrite(
            LMCDigitalOutputWriteRequest request)
        {
            return SubmitDigitalOutputWrite(
                request,
                LMCDiagnosticsDigitalOutputWritePolicy.IsApproved);
        }

        private LMCOperationTicket SubmitDigitalOutputWrite(
            LMCDigitalOutputWriteRequest request,
            Func<uint, bool> isApprovedIOReference)
        {
            var attemptTracker =
                new LMCDigitalOutputWriteSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                attemptTracker.BeginSessionPreflight();
                var sessionGeneration = connection.SessionGeneration;
                connection.EnsureSessionGeneration(sessionGeneration);
                ValidateDigitalOutputWriteRequestSession(
                    request,
                    sessionGeneration);
                return SubmitDigitalOutputWriteCore(
                    request,
                    sessionGeneration,
                    attemptTracker,
                    isApprovedIOReference);
            }
            catch (Exception exception)
            {
                LMCDigitalOutputWriteSubmissionFailureContext.Attach(
                    exception,
                    attemptTracker.CreateFailureContext());
                throw;
            }
        }

        public async Task<LMCOperationTicket> SubmitDigitalOutputWriteAsync(
            LMCDigitalOutputWriteRequest request,
            CancellationToken cancellationToken)
        {
            return await SubmitDigitalOutputWriteAsync(
                request,
                cancellationToken,
                LMCDiagnosticsDigitalOutputWritePolicy.IsApproved)
                .ConfigureAwait(false);
        }

        private async Task<LMCOperationTicket> SubmitDigitalOutputWriteAsync(
            LMCDigitalOutputWriteRequest request,
            CancellationToken cancellationToken,
            Func<uint, bool> isApprovedIOReference)
        {
            var attemptTracker =
                new LMCDigitalOutputWriteSubmissionAttemptTracker(request);
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                attemptTracker.BeginSessionPreflight();
                var sessionGeneration = connection.SessionGeneration;
                connection.EnsureSessionGeneration(sessionGeneration);
                ValidateDigitalOutputWriteRequestSession(
                    request,
                    sessionGeneration);
                return await RunStateMutatingAsync(
                    () => SubmitDigitalOutputWriteCore(
                        request,
                        sessionGeneration,
                        attemptTracker,
                        isApprovedIOReference),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LMCDigitalOutputWriteSubmissionFailureContext.Attach(
                    exception,
                    attemptTracker.CreateFailureContext());
                throw;
            }
        }

        private LMCOperationTicket SubmitDigitalOutputWriteCore(
            LMCDigitalOutputWriteRequest request,
            long sessionGeneration,
            LMCDigitalOutputWriteSubmissionAttemptTracker attemptTracker,
            Func<uint, bool> isApprovedIOReference)
        {
            if (isApprovedIOReference == null)
            {
                throw new ArgumentNullException("isApprovedIOReference");
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            attemptTracker.BeginCapabilityPreflight();
            var capabilities = GetCapabilities(sessionGeneration);
            attemptTracker.RecordCapabilityIdentity(
                capabilities.DiagnosticsBootId);
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth
                    | LMCDiagnosticCapability.DigitalIORead
                    | LMCDiagnosticCapability.DigitalIOWrite,
                LMC_DiagnosticsFrame.DigitalOutputWriteRequestPayloadLength,
                LMC_DiagnosticsParser.SubmitOperationPayloadLength,
                "SubmitDigitalOutputWrite",
                true);
            ValidateDigitalOutputWriteRequestCapabilities(
                request,
                capabilities);

            if (!isApprovedIOReference(request.IOReference))
            {
                throw new UnauthorizedAccessException(
                    "Digital output IOReference "
                    + request.IOReference
                    + " is not in the immutable SDK write allowlist.");
            }

            RememberOperationBootId(
                sessionGeneration,
                capabilities.DiagnosticsBootId);
            attemptTracker.BeginSubmission();
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.SubmitDigitalOutputWrite(
                    requestId,
                    request,
                    capabilities.DiagnosticsBootId),
                sessionGeneration,
                attemptTracker.MarkSubmissionOutcomeUncertain);
            LMCOperationSubmission submission;
            try
            {
                submission = LMC_DiagnosticsParser.ParseSubmitOperation(
                    raw,
                    requestId,
                    LMCOperationKind.DigitalOutputWrite,
                    capabilities.DiagnosticsBootId,
                    "SubmitDigitalOutputWrite");
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                attemptTracker.MarkSubmissionRejected();
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }
            catch (Exception exception)
                when (exception is LMCDiagnosticsNotSupportedException
                    || exception is LMCDiagnosticsDispatchRejectedException)
            {
                attemptTracker.MarkSubmissionRejected();
                throw;
            }

            var ticket = new LMCOperationTicket(
                submission.TicketId,
                submission.OperationKind,
                submission.QueuedCycle,
                submission.DiagnosticsBootId,
                request.TopologyRevision,
                sessionGeneration,
                this,
                false,
                0,
                LMCSignalValueType.Invalid);
            attemptTracker.MarkSubmissionAccepted(ticket);
            LMCOperationTicket publishedTicket = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.SubmitDigitalOutputWrite,
                () => publishedTicket = ticket);
            return publishedTicket;
        }

        private void ValidateDigitalOutputWriteSnapshot(
            LMCDigitalIOValue outputSnapshot,
            long sessionGeneration)
        {
            var requiredCapabilities =
                LMCDiagnosticCapability.EtherCATTopology
                | LMCDiagnosticCapability.EtherCATNodeHealth
                | LMCDiagnosticCapability.DigitalIORead
                | LMCDiagnosticCapability.DigitalIOWrite;
            if (!outputSnapshot.BelongsToCurrentSession(connection)
                || outputSnapshot.ConnectionSessionGeneration
                    != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The digital-output snapshot does not belong to this active RPC session; read a fresh output snapshot after connecting.");
            }

            if (outputSnapshot.Direction != LMCDigitalIODirection.Output
                || !outputSnapshot.IsValid
                || outputSnapshot.OutputRevision == 0
                || !outputSnapshot.HasValidatedTopologyBinding)
            {
                throw new InvalidOperationException(
                    "A digital-output write requires a topology-bound valid Output snapshot with a non-zero OutputRevision; use the topology-bound ReadDigitalIO overload.");
            }

            if (outputSnapshot.DiagnosticsBootId == 0
                || (outputSnapshot.SourceCapabilities
                        & requiredCapabilities)
                    != requiredCapabilities)
            {
                throw new InvalidOperationException(
                    "The digital-output snapshot was not read under the complete topology, health, read, write, and stable BootId capability contract.");
            }
        }

        private void ValidateDigitalOutputWriteRequestSession(
            LMCDigitalOutputWriteRequest request,
            long sessionGeneration)
        {
            if (!request.IsSnapshotBound
                || !request.BelongsToCurrentSession(connection))
            {
                throw new InvalidOperationException(
                    "Detached or stale digital-output requests cannot be submitted; create the request from a fresh current-session Output snapshot.");
            }

            var source = request.SourceSnapshot;
            ValidateDigitalOutputWriteSnapshot(source, sessionGeneration);
            if (source.TopologyRevision != request.TopologyRevision
                || source.IOReference != request.IOReference
                || source.OutputRevision
                    != request.ExpectedOutputRevision
                || (request.Mask & ~source.ValidMask) != 0)
            {
                throw new InvalidOperationException(
                    "The digital-output request no longer matches its immutable source snapshot.");
            }
        }

        private static void ValidateDigitalOutputWriteRequestCapabilities(
            LMCDigitalOutputWriteRequest request,
            LMCDiagnosticCapabilities capabilities)
        {
            if (request.SourceDiagnosticsBootId == 0
                || request.SourceDiagnosticsBootId
                    != capabilities.DiagnosticsBootId)
            {
                throw new InvalidOperationException(
                    "The PLC DiagnosticsBootId changed after the output snapshot was read; read a fresh output snapshot before writing.");
            }
        }

        private LMCEtherCATTopologyInfo GetEtherCATTopologyInfo(
            long sessionGeneration)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(requestId),
                sessionGeneration);
            var info = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                raw,
                requestId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return info;
        }

        private async Task<LMCEtherCATTopologyInfo> GetEtherCATTopologyInfoAsync(
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.GetEtherCATTopologyInfo(requestId),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var info = LMC_DiagnosticsParser.ParseEtherCATTopologyInfo(
                raw,
                requestId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return info;
        }

        private LMCEtherCATTopologyChunk GetEtherCATTopologyChunk(
            long sessionGeneration,
            uint expectedTopologyRevision,
            ushort startIndex,
            ushort maxEntries)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    requestId,
                    expectedTopologyRevision,
                    startIndex,
                    maxEntries),
                sessionGeneration);
            var chunk = LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                raw,
                requestId,
                expectedTopologyRevision,
                startIndex,
                maxEntries);
            connection.EnsureSessionGeneration(sessionGeneration);
            return chunk;
        }

        private async Task<LMCEtherCATTopologyChunk> GetEtherCATTopologyChunkAsync(
            long sessionGeneration,
            uint expectedTopologyRevision,
            ushort startIndex,
            ushort maxEntries,
            CancellationToken cancellationToken)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.GetEtherCATTopologyChunk(
                    requestId,
                    expectedTopologyRevision,
                    startIndex,
                    maxEntries),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var chunk = LMC_DiagnosticsParser.ParseEtherCATTopologyChunk(
                raw,
                requestId,
                expectedTopologyRevision,
                startIndex,
                maxEntries);
            connection.EnsureSessionGeneration(sessionGeneration);
            return chunk;
        }

        private List<LMCEtherCATTopologyEntry> DownloadEtherCATTopology(
            long sessionGeneration,
            LMCEtherCATTopologyInfo info,
            ushort maxEntriesPerChunk)
        {
            var entries = new List<LMCEtherCATTopologyEntry>(info.TotalNodeCount);
            ushort startIndex = 0;

            while (startIndex < info.TotalNodeCount)
            {
                var remaining = info.TotalNodeCount - startIndex;
                var maxEntries = checked((ushort)Math.Min(
                    maxEntriesPerChunk,
                    remaining));
                var chunk = GetEtherCATTopologyChunk(
                    sessionGeneration,
                    info.TopologyRevision,
                    startIndex,
                    maxEntries);
                ValidateTopologyChunk(info, chunk);
                entries.AddRange(chunk.Entries);
                startIndex = checked((ushort)(startIndex + chunk.ReturnedCount));
            }

            return entries;
        }

        private async Task<List<LMCEtherCATTopologyEntry>>
            DownloadEtherCATTopologyAsync(
                long sessionGeneration,
                LMCEtherCATTopologyInfo info,
                ushort maxEntriesPerChunk,
                CancellationToken cancellationToken)
        {
            var entries = new List<LMCEtherCATTopologyEntry>(info.TotalNodeCount);
            ushort startIndex = 0;

            while (startIndex < info.TotalNodeCount)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remaining = info.TotalNodeCount - startIndex;
                var maxEntries = checked((ushort)Math.Min(
                    maxEntriesPerChunk,
                    remaining));
                var chunk = await GetEtherCATTopologyChunkAsync(
                    sessionGeneration,
                    info.TopologyRevision,
                    startIndex,
                    maxEntries,
                    cancellationToken).ConfigureAwait(false);
                ValidateTopologyChunk(info, chunk);
                entries.AddRange(chunk.Entries);
                startIndex = checked((ushort)(startIndex + chunk.ReturnedCount));
            }

            return entries;
        }

        private static void ValidateTopologyChunk(
            LMCEtherCATTopologyInfo info,
            LMCEtherCATTopologyChunk chunk)
        {
            if (chunk == null
                || chunk.TopologyRevision != info.TopologyRevision
                || chunk.TotalNodeCount != info.TotalNodeCount
                || chunk.EntryStride != info.EntryStride
                || chunk.ReturnedCount == 0)
            {
                throw new InvalidDataException(
                    "EtherCAT topology changed or made no progress while downloading.");
            }
        }

        internal static void ValidateCompleteTopology(
            LMCEtherCATTopologyInfo info,
            IReadOnlyList<LMCEtherCATTopologyEntry> entries)
        {
            if (entries.Count != info.TotalNodeCount)
            {
                throw new InvalidDataException(
                    "EtherCAT topology entry count does not match its info record.");
            }

            var nodes = new Dictionary<uint, LMCEtherCATTopologyEntry>();
            var ioReferences = new HashSet<uint>();
            var sdoSlaveReferences = new HashSet<ushort>();
            var physicalAxisReferences = new HashSet<ushort>();
            var slotKeys = new HashSet<ulong>();
            var slaveCount = 0;
            var slotModuleCount = 0;
            var physicalAxisCount = 0;

            foreach (var entry in entries)
            {
                if (entry == null
                    || entry.TopologyIndex != nodes.Count
                    || nodes.ContainsKey(entry.NodeId))
                {
                    throw new InvalidDataException(
                        "EtherCAT topology has a null, duplicate, or unordered node.");
                }

                if (entry.NodeKind == LMCEtherCATTopologyNodeKind.EtherCATSlave)
                {
                    if (entry.MasterSlaveIndex != slaveCount)
                    {
                        throw new InvalidDataException(
                            "EtherCAT slave indices must be unique, ordered, and zero based.");
                    }

                    slaveCount++;
                }
                else
                {
                    LMCEtherCATTopologyEntry parent;
                    if (!nodes.TryGetValue(entry.ParentNodeId, out parent)
                        || parent.NodeKind
                            != LMCEtherCATTopologyNodeKind.EtherCATSlave)
                    {
                        throw new InvalidDataException(
                            "A slot module parent must be an earlier EtherCAT slave.");
                    }

                    var slotKey = ((ulong)entry.ParentNodeId << 16)
                        | entry.SlotIndex;
                    if (!slotKeys.Add(slotKey))
                    {
                        throw new InvalidDataException(
                            "EtherCAT topology contains duplicate slot indices for one parent.");
                    }

                    slotModuleCount++;
                }

                if ((entry.NodeFlags
                    & LMCEtherCATTopologyNodeFlags.PhysicalAxis) != 0)
                {
                    if (entry.PhysicalAxisReference > info.PhysicalAxisCount
                        || !physicalAxisReferences.Add(
                            entry.PhysicalAxisReference))
                    {
                        throw new InvalidDataException(
                            "Physical axis references must be unique and within the advertised range.");
                    }

                    physicalAxisCount++;
                }

                if (entry.SdoSlaveReference != 0
                    && !sdoSlaveReferences.Add(entry.SdoSlaveReference))
                {
                    throw new InvalidDataException(
                        "EtherCAT topology contains duplicate SDO slave references.");
                }

                if (entry.IOReference != 0
                    && !ioReferences.Add(entry.IOReference))
                {
                    throw new InvalidDataException(
                        "EtherCAT topology contains duplicate IOReference values.");
                }

                nodes.Add(entry.NodeId, entry);
            }

            if (slaveCount != info.ConfiguredSlaveCount
                || slotModuleCount != info.SlotModuleCount
                || physicalAxisCount != info.PhysicalAxisCount)
            {
                throw new InvalidDataException(
                    "EtherCAT topology node-kind counts do not match its info record.");
            }

            var calculatedRevision =
                LMC_DiagnosticsParser.ComputeEtherCATTopologyRevision(entries);
            if (calculatedRevision != info.TopologyRevision)
            {
                throw new InvalidDataException(
                    "EtherCAT topology canonical CRC does not match TopologyRevision.");
            }
        }

        private static ushort GetTopologyChunkLimit(
            LMCDiagnosticCapabilities capabilities,
            LMCEtherCATTopologyInfo info)
        {
            var responseLimit = checked((ushort)Math.Min(
                LMC_DiagnosticsFrame.MaxTopologyEntriesPerChunk,
                (capabilities.MaxResponsePayloadBytes
                    - LMC_DiagnosticsParser.TopologyChunkHeaderPayloadLength)
                    / LMC_DiagnosticsParser.TopologyEntryStride));
            var limit = checked((ushort)Math.Min(
                responseLimit,
                info.MaxEntriesPerChunk));

            if (limit == 0)
            {
                throw new InvalidDataException(
                    "Negotiated response limits cannot carry a topology entry.");
            }

            return limit;
        }

        private async Task<LMCEtherCATNodeHealth>
            ReadEtherCATNodeHealthCoreAsync(
                uint topologyRevision,
                uint nodeId,
                long sessionGeneration,
                CancellationToken cancellationToken)
        {
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                    requestId,
                    topologyRevision,
                    nodeId),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var health = LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                raw,
                requestId,
                topologyRevision,
                nodeId);
            LMCEtherCATNodeHealth publishedHealth = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.ReadEtherCATNodeHealth,
                () => publishedHealth = health);
            return publishedHealth;
        }

        private LMCEtherCATNodeHealth ReadEtherCATNodeHealthCore(
            uint topologyRevision,
            uint nodeId,
            long sessionGeneration)
        {
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadEtherCATNodeHealth(
                    requestId,
                    topologyRevision,
                    nodeId),
                sessionGeneration);
            var health = LMC_DiagnosticsParser.ParseEtherCATNodeHealth(
                raw,
                requestId,
                topologyRevision,
                nodeId);
            LMCEtherCATNodeHealth publishedHealth = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.ReadEtherCATNodeHealth,
                () => publishedHealth = health);
            return publishedHealth;
        }

        private LMCDigitalIOValue ReadDigitalIOCore(
            LMCDigitalIOReadRequest request,
            LMCDiagnosticCapabilities capabilities,
            long sessionGeneration)
        {
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadDigitalIO(requestId, request),
                sessionGeneration);
            var value = LMC_DiagnosticsParser.ParseDigitalIO(
                raw,
                requestId,
                request);
            LMCDigitalIOValue publishedValue = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.ReadDigitalIO,
                () => publishedValue = value.BindTo(
                    this,
                    sessionGeneration,
                    capabilities));
            return publishedValue;
        }

        private async Task<LMCDigitalIOValue> ReadDigitalIOCoreAsync(
            LMCDigitalIOReadRequest request,
            LMCDiagnosticCapabilities capabilities,
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadDigitalIO(requestId, request),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var value = LMC_DiagnosticsParser.ParseDigitalIO(
                raw,
                requestId,
                request);
            LMCDigitalIOValue publishedValue = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.ReadDigitalIO,
                () => publishedValue = value.BindTo(
                    this,
                    sessionGeneration,
                    capabilities));
            return publishedValue;
        }

        private void ValidatePinnedTopologyIoCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            LMCDiagnosticCapability requiredCapabilities,
            int requiredRequestPayloadBytes,
            int requiredResponsePayloadBytes,
            string commandName)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (!capabilities.IsBoundTo(this, expectedSessionGeneration))
            {
                throw new InvalidOperationException(
                    "Diagnostics capabilities are not bound to this diagnostics owner and current connection session.");
            }

            ValidateTopologyIoCapabilities(
                capabilities,
                expectedSessionGeneration,
                requiredCapabilities,
                requiredRequestPayloadBytes,
                requiredResponsePayloadBytes,
                commandName,
                false);
        }

        private void ValidateTopologyIoCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            LMCDiagnosticCapability requiredCapabilities,
            int requiredRequestPayloadBytes,
            int requiredResponsePayloadBytes,
            string commandName,
            bool requiresBootId)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (capabilities.ConnectionSessionGeneration
                != expectedSessionGeneration)
            {
                throw new InvalidOperationException(
                    "Diagnostics capabilities belong to a stale connection session.");
            }

            if (!capabilities.Supports(requiredCapabilities))
            {
                throw new NotSupportedException(
                    commandName
                    + " requires PLC diagnostics capability "
                    + requiredCapabilities
                    + ".");
            }

            if (requiresBootId && capabilities.DiagnosticsBootId == 0)
            {
                throw new InvalidDataException(
                    commandName + " requires a non-zero DiagnosticsBootId.");
            }

            if (requiredRequestPayloadBytes > capabilities.MaxRequestPayloadBytes
                || requiredResponsePayloadBytes
                    > capabilities.MaxResponsePayloadBytes)
            {
                throw new InvalidDataException(
                    commandName
                    + " exceeds the payload limits advertised by the PLC.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private static void ValidateTopologyChunkRequest(
            uint expectedTopologyRevision,
            ushort maxEntries)
        {
            if (expectedTopologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedTopologyRevision");
            }

            if (maxEntries == 0
                || maxEntries > LMC_DiagnosticsFrame.MaxTopologyEntriesPerChunk)
            {
                throw new ArgumentOutOfRangeException("maxEntries");
            }
        }

        private static void ValidateTopologyNodeIdentity(
            uint topologyRevision,
            uint nodeId)
        {
            if (topologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException("topologyRevision");
            }

            if (nodeId == 0)
            {
                throw new ArgumentOutOfRangeException("nodeId");
            }
        }

        private void RequireCurrentTopology(
            LMCEtherCATTopology topology)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            if (!topology.IsBoundTo(this, sessionGeneration))
            {
                throw new InvalidOperationException(
                    "The EtherCAT topology belongs to a different or stale diagnostics session. Reload the topology after connecting.");
            }
        }

        private LMCEtherCATTopologyEntry RequireCurrentTopologyNode(
            LMCEtherCATTopology topology,
            uint nodeId)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            LMCEtherCATTopologyEntry entry;
            if (nodeId == 0 || !topology.TryGetNode(nodeId, out entry))
            {
                throw new ArgumentOutOfRangeException(
                    "nodeId",
                    "The node does not belong to the supplied EtherCAT topology.");
            }

            RequireCurrentTopology(topology);

            return entry;
        }
    }
}

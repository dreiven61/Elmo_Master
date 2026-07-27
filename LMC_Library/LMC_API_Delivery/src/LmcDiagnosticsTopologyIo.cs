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
            var capabilities = GetCapabilities();
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
            var capabilities = GetCapabilities();
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
            var capabilities = GetCapabilities();
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
            return new LMCEtherCATTopology(info, entries);
        }

        public async Task<LMCEtherCATTopology> GetEtherCATTopologyAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
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
            return new LMCEtherCATTopology(info, entries);
        }

        public LMCEtherCATNodeHealth ReadEtherCATNodeHealth(
            uint topologyRevision,
            uint nodeId)
        {
            ValidateTopologyNodeIdentity(topologyRevision, nodeId);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.EtherCATNodeHealth,
                LMC_DiagnosticsFrame.NodeHealthRequestPayloadLength,
                LMC_DiagnosticsParser.NodeHealthPayloadLength,
                "ReadEtherCATNodeHealth",
                false);

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
            connection.EnsureSessionGeneration(sessionGeneration);
            return health;
        }

        public async Task<LMCEtherCATNodeHealth> ReadEtherCATNodeHealthAsync(
            uint topologyRevision,
            uint nodeId,
            CancellationToken cancellationToken)
        {
            ValidateTopologyNodeIdentity(topologyRevision, nodeId);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
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
            connection.EnsureSessionGeneration(sessionGeneration);
            return health;
        }

        public LMCDigitalIOValue ReadDigitalIO(
            LMCDigitalIOReadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateTopologyIoCapabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATTopology
                    | LMCDiagnosticCapability.DigitalIORead,
                LMC_DiagnosticsFrame.DigitalIOReadRequestPayloadLength,
                LMC_DiagnosticsParser.DigitalIOPayloadLength,
                "ReadDigitalIO",
                false);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadDigitalIO(requestId, request),
                sessionGeneration);
            var value = LMC_DiagnosticsParser.ParseDigitalIO(
                raw,
                requestId,
                request);
            connection.EnsureSessionGeneration(sessionGeneration);
            return value;
        }

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

            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadDigitalIO(requestId, request),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var value = LMC_DiagnosticsParser.ParseDigitalIO(
                raw,
                requestId,
                request);
            connection.EnsureSessionGeneration(sessionGeneration);
            return value;
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
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = connection.SessionGeneration;
            return SubmitDigitalOutputWriteCore(request, sessionGeneration);
        }

        public async Task<LMCOperationTicket> SubmitDigitalOutputWriteAsync(
            LMCDigitalOutputWriteRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            return await RunStateMutatingAsync(
                () => SubmitDigitalOutputWriteCore(request, sessionGeneration),
                cancellationToken).ConfigureAwait(false);
        }

        private LMCOperationTicket SubmitDigitalOutputWriteCore(
            LMCDigitalOutputWriteRequest request,
            long sessionGeneration)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
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

            if (!LMCDiagnosticsDigitalOutputWritePolicy.IsApproved(
                    request.IOReference))
            {
                throw new UnauthorizedAccessException(
                    "Digital output IOReference "
                    + request.IOReference
                    + " is not in the immutable SDK write allowlist.");
            }

            RememberOperationBootId(
                sessionGeneration,
                capabilities.DiagnosticsBootId);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.SubmitDigitalOutputWrite(
                    requestId,
                    request,
                    capabilities.DiagnosticsBootId),
                sessionGeneration);
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
                HandleD5DomainError(sessionGeneration, exception);
                throw;
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCOperationTicket(
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
    }
}

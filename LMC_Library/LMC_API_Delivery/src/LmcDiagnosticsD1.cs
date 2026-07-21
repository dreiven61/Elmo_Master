using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        private readonly object catalogRevisionSync = new object();
        private bool hasCatalogRevision;
        private long catalogRevisionSessionGeneration;
        private uint catalogRevision;

        public LMCSignalCatalogInfo GetSignalCatalogInfo()
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog,
                LMC_DiagnosticsFrame.CommonRequestPayloadLength,
                LMC_DiagnosticsParser.CatalogInfoPayloadLength,
                "GetSignalCatalogInfo");
            var info = GetSignalCatalogInfo(sessionGeneration);
            ValidateCatalogInfoAgainstCapabilities(
                sessionGeneration,
                capabilities,
                info);
            return info;
        }

        public async Task<LMCSignalCatalogInfo> GetSignalCatalogInfoAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                cancellationToken).ConfigureAwait(false);
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog,
                LMC_DiagnosticsFrame.CommonRequestPayloadLength,
                LMC_DiagnosticsParser.CatalogInfoPayloadLength,
                "GetSignalCatalogInfo");
            var info = await GetSignalCatalogInfoAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateCatalogInfoAgainstCapabilities(
                sessionGeneration,
                capabilities,
                info);
            return info;
        }

        public LMCSignalCatalogChunk GetSignalCatalogChunk(
            uint expectedMapRevision,
            ushort startIndex,
            ushort maxEntries)
        {
            ValidateCatalogChunkRequest(maxEntries);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog,
                LMC_DiagnosticsFrame.CatalogChunkRequestPayloadLength,
                checked(LMC_DiagnosticsParser.CatalogChunkHeaderPayloadLength
                    + maxEntries * LMC_DiagnosticsParser.CatalogEntryStride),
                "GetSignalCatalogChunk");
            ValidateExpectedMapRevisionAgainstCapabilities(
                sessionGeneration,
                capabilities,
                expectedMapRevision,
                "GetSignalCatalogChunk");
            var chunk = GetSignalCatalogChunk(
                sessionGeneration,
                expectedMapRevision,
                startIndex,
                maxEntries);
            ValidateCatalogChunkAgainstCapabilities(
                sessionGeneration,
                capabilities,
                chunk);
            return chunk;
        }

        public async Task<LMCSignalCatalogChunk> GetSignalCatalogChunkAsync(
            uint expectedMapRevision,
            ushort startIndex,
            ushort maxEntries,
            CancellationToken cancellationToken)
        {
            ValidateCatalogChunkRequest(maxEntries);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                cancellationToken).ConfigureAwait(false);
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog,
                LMC_DiagnosticsFrame.CatalogChunkRequestPayloadLength,
                checked(LMC_DiagnosticsParser.CatalogChunkHeaderPayloadLength
                    + maxEntries * LMC_DiagnosticsParser.CatalogEntryStride),
                "GetSignalCatalogChunk");
            ValidateExpectedMapRevisionAgainstCapabilities(
                sessionGeneration,
                capabilities,
                expectedMapRevision,
                "GetSignalCatalogChunk");
            var chunk = await GetSignalCatalogChunkAsync(
                sessionGeneration,
                expectedMapRevision,
                startIndex,
                maxEntries,
                cancellationToken).ConfigureAwait(false);
            ValidateCatalogChunkAgainstCapabilities(
                sessionGeneration,
                capabilities,
                chunk);
            return chunk;
        }

        public LMCSignalCatalog GetSignalCatalog()
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog,
                LMC_DiagnosticsFrame.CatalogChunkRequestPayloadLength,
                LMC_DiagnosticsParser.CatalogChunkHeaderPayloadLength
                    + LMC_DiagnosticsParser.CatalogEntryStride,
                "GetSignalCatalog");
            var info = GetSignalCatalogInfo(sessionGeneration);
            ValidateCatalogInfoAgainstCapabilities(
                sessionGeneration,
                capabilities,
                info);
            var entries = DownloadSignalCatalog(
                sessionGeneration,
                info,
                GetCatalogChunkLimit(capabilities));
            ValidateCatalogMapRevision(sessionGeneration, info, entries);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCSignalCatalog(info, entries);
        }

        public async Task<LMCSignalCatalog> GetSignalCatalogAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                cancellationToken).ConfigureAwait(false);
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog,
                LMC_DiagnosticsFrame.CatalogChunkRequestPayloadLength,
                LMC_DiagnosticsParser.CatalogChunkHeaderPayloadLength
                    + LMC_DiagnosticsParser.CatalogEntryStride,
                "GetSignalCatalog");
            var info = await GetSignalCatalogInfoAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateCatalogInfoAgainstCapabilities(
                sessionGeneration,
                capabilities,
                info);
            var entries = await DownloadSignalCatalogAsync(
                sessionGeneration,
                info,
                GetCatalogChunkLimit(capabilities),
                cancellationToken).ConfigureAwait(false);
            ValidateCatalogMapRevision(sessionGeneration, info, entries);
            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCSignalCatalog(info, entries);
        }

        public LMCEtherCATHealth ReadEtherCATHealth()
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATHealth,
                LMC_DiagnosticsFrame.CommonRequestPayloadLength,
                LMC_DiagnosticsParser.HealthHeaderPayloadLength
                    + 4 * LMC_DiagnosticsParser.SlaveHealthEntryStride,
                "ReadEtherCATHealth");
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadEtherCATHealth(requestId),
                sessionGeneration);
            var health = LMC_DiagnosticsParser.ParseEtherCATHealth(
                raw,
                requestId);
            ValidateHealthAgainstCapabilities(capabilities, health);
            connection.EnsureSessionGeneration(sessionGeneration);
            return health;
        }

        public async Task<LMCEtherCATHealth> ReadEtherCATHealthAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                cancellationToken).ConfigureAwait(false);
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.EtherCATHealth,
                LMC_DiagnosticsFrame.CommonRequestPayloadLength,
                LMC_DiagnosticsParser.HealthHeaderPayloadLength
                    + 4 * LMC_DiagnosticsParser.SlaveHealthEntryStride,
                "ReadEtherCATHealth");
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadEtherCATHealth(requestId),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var health = LMC_DiagnosticsParser.ParseEtherCATHealth(
                raw,
                requestId);
            ValidateHealthAgainstCapabilities(capabilities, health);
            connection.EnsureSessionGeneration(sessionGeneration);
            return health;
        }

        public LMCSignalValue ReadPI(uint signalId)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.PIRead,
                LMC_DiagnosticsFrame.ReadPIRequestPayloadLength,
                LMC_DiagnosticsParser.ReadPIPayloadLength,
                "ReadPI");
            var expectedMapRevision = GetCatalogRevision(
                sessionGeneration,
                capabilities);
            return ReadPI(
                sessionGeneration,
                signalId,
                expectedMapRevision,
                LMCSignalValueType.Invalid);
        }

        public LMCSignalValue ReadPI(
            uint signalId,
            uint expectedMapRevision,
            LMCSignalValueType expectedType)
        {
            RequireExactPublicMapRevision(expectedMapRevision);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.PIRead,
                LMC_DiagnosticsFrame.ReadPIRequestPayloadLength,
                LMC_DiagnosticsParser.ReadPIPayloadLength,
                "ReadPI");
            ValidateExpectedMapRevisionAgainstCapabilities(
                sessionGeneration,
                capabilities,
                expectedMapRevision,
                "ReadPI");
            return ReadPI(
                sessionGeneration,
                signalId,
                expectedMapRevision,
                expectedType);
        }

        private LMCSignalValue ReadPI(
            long sessionGeneration,
            uint signalId,
            uint expectedMapRevision,
            LMCSignalValueType expectedType)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadPI(
                    requestId,
                    expectedMapRevision,
                    signalId,
                    expectedType),
                sessionGeneration);
            LMCSignalValue value;
            try
            {
                value = LMC_DiagnosticsParser.ParsePI(
                    raw,
                    requestId,
                    expectedMapRevision,
                    signalId,
                    expectedType);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }
            connection.EnsureSessionGeneration(sessionGeneration);
            return value;
        }

        public Task<LMCSignalValue> ReadPIAsync(
            uint signalId,
            CancellationToken cancellationToken)
        {
            return ReadPIWithCatalogRevisionAsync(
                signalId,
                cancellationToken);
        }

        public async Task<LMCSignalValue> ReadPIAsync(
            uint signalId,
            uint expectedMapRevision,
            LMCSignalValueType expectedType,
            CancellationToken cancellationToken)
        {
            RequireExactPublicMapRevision(expectedMapRevision);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                cancellationToken).ConfigureAwait(false);
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.PIRead,
                LMC_DiagnosticsFrame.ReadPIRequestPayloadLength,
                LMC_DiagnosticsParser.ReadPIPayloadLength,
                "ReadPI");
            ValidateExpectedMapRevisionAgainstCapabilities(
                sessionGeneration,
                capabilities,
                expectedMapRevision,
                "ReadPI");
            return await ReadPIAsync(
                sessionGeneration,
                signalId,
                expectedMapRevision,
                expectedType,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<LMCSignalValue> ReadPIWithCatalogRevisionAsync(
            uint signalId,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                cancellationToken).ConfigureAwait(false);
            ValidateD1Capabilities(
                capabilities,
                sessionGeneration,
                LMCDiagnosticCapability.SignalCatalog
                    | LMCDiagnosticCapability.PIRead,
                LMC_DiagnosticsFrame.ReadPIRequestPayloadLength,
                LMC_DiagnosticsParser.ReadPIPayloadLength,
                "ReadPI");
            uint expectedMapRevision;
            if (!TryGetCatalogRevision(sessionGeneration, out expectedMapRevision))
            {
                var info = await GetSignalCatalogInfoAsync(
                    sessionGeneration,
                    cancellationToken).ConfigureAwait(false);
                ValidateCatalogInfoAgainstCapabilities(
                    sessionGeneration,
                    capabilities,
                    info);
                expectedMapRevision = info.MapRevision;
            }

            return await ReadPIAsync(
                sessionGeneration,
                signalId,
                expectedMapRevision,
                LMCSignalValueType.Invalid,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<LMCSignalValue> ReadPIAsync(
            long sessionGeneration,
            uint signalId,
            uint expectedMapRevision,
            LMCSignalValueType expectedType,
            CancellationToken cancellationToken)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadPI(
                    requestId,
                    expectedMapRevision,
                    signalId,
                    expectedType),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            LMCSignalValue value;
            try
            {
                value = LMC_DiagnosticsParser.ParsePI(
                    raw,
                    requestId,
                    expectedMapRevision,
                    signalId,
                    expectedType);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }
            connection.EnsureSessionGeneration(sessionGeneration);
            return value;
        }

        private LMCSignalCatalogInfo GetSignalCatalogInfo(
            long sessionGeneration)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.GetSignalCatalogInfo(requestId),
                sessionGeneration);
            var info = LMC_DiagnosticsParser.ParseSignalCatalogInfo(
                raw,
                requestId);
            RememberCatalogRevision(sessionGeneration, info.MapRevision);
            connection.EnsureSessionGeneration(sessionGeneration);
            return info;
        }

        private async Task<LMCSignalCatalogInfo> GetSignalCatalogInfoAsync(
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.GetSignalCatalogInfo(requestId),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var info = LMC_DiagnosticsParser.ParseSignalCatalogInfo(
                raw,
                requestId);
            RememberCatalogRevision(sessionGeneration, info.MapRevision);
            connection.EnsureSessionGeneration(sessionGeneration);
            return info;
        }

        private LMCSignalCatalogChunk GetSignalCatalogChunk(
            long sessionGeneration,
            uint expectedMapRevision,
            ushort startIndex,
            ushort maxEntries)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.GetSignalCatalogChunk(
                    requestId,
                    expectedMapRevision,
                    startIndex,
                    maxEntries),
                sessionGeneration);
            LMCSignalCatalogChunk chunk;
            try
            {
                chunk = LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    raw,
                    requestId,
                    expectedMapRevision,
                    startIndex,
                    maxEntries);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }
            RememberCatalogRevision(sessionGeneration, chunk.MapRevision);
            connection.EnsureSessionGeneration(sessionGeneration);
            return chunk;
        }

        private async Task<LMCSignalCatalogChunk> GetSignalCatalogChunkAsync(
            long sessionGeneration,
            uint expectedMapRevision,
            ushort startIndex,
            ushort maxEntries,
            CancellationToken cancellationToken)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.GetSignalCatalogChunk(
                    requestId,
                    expectedMapRevision,
                    startIndex,
                    maxEntries),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            LMCSignalCatalogChunk chunk;
            try
            {
                chunk = LMC_DiagnosticsParser.ParseSignalCatalogChunk(
                    raw,
                    requestId,
                    expectedMapRevision,
                    startIndex,
                    maxEntries);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }
            RememberCatalogRevision(sessionGeneration, chunk.MapRevision);
            connection.EnsureSessionGeneration(sessionGeneration);
            return chunk;
        }

        private List<LMCSignalCatalogEntry> DownloadSignalCatalog(
            long sessionGeneration,
            LMCSignalCatalogInfo info,
            ushort maxEntriesPerChunk)
        {
            var entries = new List<LMCSignalCatalogEntry>(info.TotalCount);
            var signalIds = new HashSet<uint>();
            ushort startIndex = 0;

            while (startIndex < info.TotalCount)
            {
                var remaining = info.TotalCount - startIndex;
                var maxEntries = checked((ushort)Math.Min(
                    maxEntriesPerChunk,
                    remaining));
                var chunk = GetSignalCatalogChunk(
                    sessionGeneration,
                    info.MapRevision,
                    startIndex,
                    maxEntries);
                ValidateCatalogChunk(info, chunk);

                if (chunk.ReturnedCount == 0)
                {
                    throw new InvalidDataException(
                        "Signal Catalog download made no progress before TotalCount.");
                }

                AddCatalogEntries(entries, signalIds, chunk.Entries);
                startIndex = checked((ushort)(startIndex + chunk.ReturnedCount));
            }

            return entries;
        }

        private async Task<List<LMCSignalCatalogEntry>> DownloadSignalCatalogAsync(
            long sessionGeneration,
            LMCSignalCatalogInfo info,
            ushort maxEntriesPerChunk,
            CancellationToken cancellationToken)
        {
            var entries = new List<LMCSignalCatalogEntry>(info.TotalCount);
            var signalIds = new HashSet<uint>();
            ushort startIndex = 0;

            while (startIndex < info.TotalCount)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remaining = info.TotalCount - startIndex;
                var maxEntries = checked((ushort)Math.Min(
                    maxEntriesPerChunk,
                    remaining));
                var chunk = await GetSignalCatalogChunkAsync(
                    sessionGeneration,
                    info.MapRevision,
                    startIndex,
                    maxEntries,
                    cancellationToken).ConfigureAwait(false);
                ValidateCatalogChunk(info, chunk);

                if (chunk.ReturnedCount == 0)
                {
                    throw new InvalidDataException(
                        "Signal Catalog download made no progress before TotalCount.");
                }

                AddCatalogEntries(entries, signalIds, chunk.Entries);
                startIndex = checked((ushort)(startIndex + chunk.ReturnedCount));
            }

            return entries;
        }

        private static void ValidateCatalogChunk(
            LMCSignalCatalogInfo info,
            LMCSignalCatalogChunk chunk)
        {
            if (chunk.MapRevision != info.MapRevision
                || chunk.TotalCount != info.TotalCount
                || chunk.EntryStride != info.EntryStride)
            {
                throw new InvalidDataException(
                    "Signal Catalog changed while it was being downloaded.");
            }
        }

        private static void AddCatalogEntries(
            ICollection<LMCSignalCatalogEntry> destination,
            ISet<uint> signalIds,
            IReadOnlyList<LMCSignalCatalogEntry> source)
        {
            foreach (var entry in source)
            {
                if (!signalIds.Add(entry.SignalId))
                {
                    throw new InvalidDataException(
                        "Signal Catalog contains duplicate SignalId values.");
                }

                destination.Add(entry);
            }
        }

        private uint GetCatalogRevision(
            long sessionGeneration,
            LMCDiagnosticCapabilities capabilities)
        {
            uint knownRevision;
            if (TryGetCatalogRevision(sessionGeneration, out knownRevision))
            {
                return knownRevision;
            }

            var info = GetSignalCatalogInfo(sessionGeneration);
            ValidateCatalogInfoAgainstCapabilities(
                sessionGeneration,
                capabilities,
                info);
            return info.MapRevision;
        }

        private bool TryGetCatalogRevision(
            long sessionGeneration,
            out uint knownRevision)
        {
            lock (catalogRevisionSync)
            {
                if (hasCatalogRevision
                    && catalogRevisionSessionGeneration == sessionGeneration)
                {
                    knownRevision = catalogRevision;
                    return true;
                }
            }

            knownRevision = 0;
            return false;
        }

        private void RememberCatalogRevision(
            long sessionGeneration,
            uint mapRevision)
        {
            lock (catalogRevisionSync)
            {
                hasCatalogRevision = true;
                catalogRevisionSessionGeneration = sessionGeneration;
                catalogRevision = mapRevision;
            }
        }

        private void ValidateCatalogMapRevision(
            long sessionGeneration,
            LMCSignalCatalogInfo info,
            IReadOnlyList<LMCSignalCatalogEntry> entries)
        {
            var calculatedRevision =
                LMC_DiagnosticsParser.ComputeCatalogMapRevision(entries);
            if (calculatedRevision != info.MapRevision)
            {
                InvalidateCatalogRevision(sessionGeneration);
                throw new InvalidDataException(
                    "Signal Catalog canonical CRC does not match MapRevision. Expected 0x"
                    + info.MapRevision.ToString("X8")
                    + ", calculated 0x"
                    + calculatedRevision.ToString("X8")
                    + ".");
            }
        }

        private void InvalidateCatalogRevisionOnMismatch(
            long sessionGeneration,
            LMCDiagnosticsCommandException exception)
        {
            if (exception.Response.Detail
                == LMCDiagnosticsDetailCode.MapRevisionMismatch)
            {
                InvalidateCatalogRevision(sessionGeneration);
            }
        }

        private void InvalidateCatalogRevision(long sessionGeneration)
        {
            lock (catalogRevisionSync)
            {
                if (hasCatalogRevision
                    && catalogRevisionSessionGeneration == sessionGeneration)
                {
                    hasCatalogRevision = false;
                    catalogRevision = 0;
                }
            }
        }

        private void ValidateD1Capabilities(
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

            if (capabilities.MapRevision == 0)
            {
                throw new InvalidDataException(
                    commandName
                    + " capability requires a non-zero Catalog MapRevision.");
            }

            if (requiredRequestPayloadBytes > capabilities.MaxRequestPayloadBytes
                || requiredResponsePayloadBytes
                    > capabilities.MaxResponsePayloadBytes)
            {
                throw new InvalidDataException(
                    commandName
                    + " exceeds the payload limits advertised by the PLC.");
            }

            uint cachedMapRevision;
            if (TryGetCatalogRevision(
                    expectedSessionGeneration,
                    out cachedMapRevision)
                && cachedMapRevision != capabilities.MapRevision)
            {
                InvalidateCatalogRevision(expectedSessionGeneration);
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateCatalogInfoAgainstCapabilities(
            long sessionGeneration,
            LMCDiagnosticCapabilities capabilities,
            LMCSignalCatalogInfo info)
        {
            if (info == null
                || info.MapRevision != capabilities.MapRevision
                || info.TotalCount != capabilities.CatalogEntryCount
                || info.EntryStride != capabilities.CatalogEntryStride
                || capabilities.SignalValueEntryStride
                    != LMC_DiagnosticsParser.SignalValueEntryStride)
            {
                InvalidateCatalogRevision(sessionGeneration);
                throw new InvalidDataException(
                    "Signal Catalog info does not match the negotiated diagnostics capabilities.");
            }
        }

        private void ValidateCatalogChunkAgainstCapabilities(
            long sessionGeneration,
            LMCDiagnosticCapabilities capabilities,
            LMCSignalCatalogChunk chunk)
        {
            if (chunk == null
                || chunk.MapRevision != capabilities.MapRevision
                || chunk.TotalCount != capabilities.CatalogEntryCount
                || chunk.EntryStride != capabilities.CatalogEntryStride)
            {
                InvalidateCatalogRevision(sessionGeneration);
                throw new InvalidDataException(
                    "Signal Catalog chunk does not match the negotiated diagnostics capabilities.");
            }
        }

        private static void ValidateHealthAgainstCapabilities(
            LMCDiagnosticCapabilities capabilities,
            LMCEtherCATHealth health)
        {
            if (health == null
                || health.MapRevision != capabilities.MapRevision)
            {
                throw new InvalidDataException(
                    "EtherCAT Health does not match the negotiated Catalog MapRevision.");
            }
        }

        private void ValidateExpectedMapRevisionAgainstCapabilities(
            long sessionGeneration,
            LMCDiagnosticCapabilities capabilities,
            uint expectedMapRevision,
            string commandName)
        {
            if (expectedMapRevision == 0
                || expectedMapRevision == capabilities.MapRevision)
            {
                return;
            }

            InvalidateCatalogRevision(sessionGeneration);
            throw new InvalidOperationException(
                commandName
                + " ExpectedMapRevision does not match the negotiated diagnostics capabilities.");
        }

        private static ushort GetCatalogChunkLimit(
            LMCDiagnosticCapabilities capabilities)
        {
            var availableEntryBytes = capabilities.MaxResponsePayloadBytes
                - LMC_DiagnosticsParser.CatalogChunkHeaderPayloadLength;
            var maxEntries = Math.Min(
                LMC_DiagnosticsFrame.MaxCatalogEntriesPerChunk,
                availableEntryBytes / LMC_DiagnosticsParser.CatalogEntryStride);
            if (maxEntries < 1)
            {
                throw new InvalidDataException(
                    "SignalCatalog capability cannot carry one Catalog entry.");
            }

            return checked((ushort)maxEntries);
        }

        private static void ValidateCatalogChunkRequest(ushort maxEntries)
        {
            if (maxEntries == 0
                || maxEntries > LMC_DiagnosticsFrame.MaxCatalogEntriesPerChunk)
            {
                throw new ArgumentOutOfRangeException(
                    "maxEntries",
                    "Catalog chunks must request between 1 and 16 entries.");
            }
        }

        private static void RequireExactPublicMapRevision(
            uint expectedMapRevision)
        {
            if (expectedMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedMapRevision",
                    "Public ReadPI requires a non-zero exact Catalog revision.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        private readonly object bulkBootIdSync = new object();
        private bool hasBulkBootId;
        private long bulkBootIdSessionGeneration;
        private uint bulkBootId;

        public LMCBulkConfiguration ConfigureBulk(
            IReadOnlyList<uint> signalIds)
        {
            var copiedSignalIds =
                LMC_DiagnosticsFrame.CopyAndValidateBulkSignalIds(signalIds);
            var sessionGeneration = connection.SessionGeneration;
            return ConfigureBulkCore(
                copiedSignalIds,
                sessionGeneration,
                0);
        }

        internal LMCBulkConfiguration ConfigureBulkExact(
            IReadOnlyList<uint> signalIds,
            LMCSignalCatalog catalog)
        {
            RequireCurrentSignalCatalog(catalog);

            var copiedSignalIds =
                LMC_DiagnosticsFrame.CopyAndValidateBulkSignalIds(signalIds);
            return ConfigureBulkCore(
                copiedSignalIds,
                catalog.ConnectionSessionGeneration,
                catalog.MapRevision);
        }

        private LMCBulkConfiguration ConfigureBulkCore(
            uint[] copiedSignalIds,
            long sessionGeneration,
            uint expectedMapRevision)
        {
            connection.EnsureSessionGeneration(sessionGeneration);

            var capabilities = GetCapabilities(sessionGeneration);
            ValidateBulkCapabilities(
                capabilities,
                sessionGeneration,
                copiedSignalIds.Length);
            if (expectedMapRevision != 0
                && capabilities.MapRevision != expectedMapRevision)
            {
                InvalidateCatalogRevision(sessionGeneration);
                throw new InvalidOperationException(
                    "ConfigureBulk ExpectedMapRevision does not match the negotiated diagnostics capabilities.");
            }

            var mapRevision = expectedMapRevision == 0
                ? capabilities.MapRevision
                : expectedMapRevision;
            RememberBulkBootId(
                sessionGeneration,
                capabilities.DiagnosticsBootId);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ConfigureBulk(
                    requestId,
                    mapRevision,
                    0,
                    copiedSignalIds),
                sessionGeneration);

            LMCBulkStatus status;
            try
            {
                status = LMC_DiagnosticsParser.ParseConfigureBulk(
                    raw,
                    requestId,
                    mapRevision,
                    0,
                    checked((ushort)copiedSignalIds.Length));
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return new LMCBulkConfiguration(
                status,
                capabilities.DiagnosticsBootId,
                sessionGeneration,
                this,
                copiedSignalIds);
        }

        public async Task<LMCBulkConfiguration> ConfigureBulkAsync(
            IReadOnlyList<uint> signalIds,
            CancellationToken cancellationToken)
        {
            var copiedSignalIds =
                LMC_DiagnosticsFrame.CopyAndValidateBulkSignalIds(signalIds);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            return await RunStateMutatingAsync(
                () => ConfigureBulkCore(
                    copiedSignalIds,
                    sessionGeneration,
                    0),
                cancellationToken).ConfigureAwait(false);
        }

        internal async Task<LMCBulkConfiguration> ConfigureBulkExactAsync(
            IReadOnlyList<uint> signalIds,
            LMCSignalCatalog catalog,
            CancellationToken cancellationToken)
        {
            RequireCurrentSignalCatalog(catalog);

            var copiedSignalIds =
                LMC_DiagnosticsFrame.CopyAndValidateBulkSignalIds(signalIds);
            var sessionGeneration = catalog.ConnectionSessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            return await RunStateMutatingAsync(
                () => ConfigureBulkCore(
                    copiedSignalIds,
                    sessionGeneration,
                    catalog.MapRevision),
                cancellationToken).ConfigureAwait(false);
        }

        public LMCBulkStatus ReadBulkStatus(
            LMCBulkConfiguration configuration)
        {
            var sessionGeneration =
                ValidateBulkConfiguration(configuration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadBulkStatus(
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision),
                sessionGeneration);

            LMCBulkStatus status;
            try
            {
                status = LMC_DiagnosticsParser.ParseBulkStatus(
                    raw,
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision,
                    configuration.SignalCount);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return status;
        }

        public async Task<LMCBulkStatus> ReadBulkStatusAsync(
            LMCBulkConfiguration configuration,
            CancellationToken cancellationToken)
        {
            var sessionGeneration =
                ValidateBulkConfiguration(configuration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadBulkStatus(
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);

            LMCBulkStatus status;
            try
            {
                status = LMC_DiagnosticsParser.ParseBulkStatus(
                    raw,
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision,
                    configuration.SignalCount);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return status;
        }

        public LMCBulkSnapshot ReadBulk(
            LMCBulkConfiguration configuration)
        {
            var sessionGeneration =
                ValidateBulkConfiguration(configuration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadBulkSnapshot(
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision),
                sessionGeneration);

            LMCBulkSnapshot snapshot;
            try
            {
                snapshot = LMC_DiagnosticsParser.ParseBulkSnapshot(
                    raw,
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision,
                    configuration.SignalIds);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return snapshot;
        }

        public async Task<LMCBulkSnapshot> ReadBulkAsync(
            LMCBulkConfiguration configuration,
            CancellationToken cancellationToken)
        {
            var sessionGeneration =
                ValidateBulkConfiguration(configuration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadBulkSnapshot(
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);

            LMCBulkSnapshot snapshot;
            try
            {
                snapshot = LMC_DiagnosticsParser.ParseBulkSnapshot(
                    raw,
                    requestId,
                    configuration.BulkId,
                    configuration.ConfigRevision,
                    configuration.MapRevision,
                    configuration.SignalIds);
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                InvalidateCatalogRevisionOnMismatch(
                    sessionGeneration,
                    exception);
                throw;
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return snapshot;
        }

        public void ReleaseBulk(LMCBulkConfiguration configuration)
        {
            var sessionGeneration =
                ValidateBulkConfiguration(configuration);
            configuration.BeginRelease();

            try
            {
                var requestId = NextRequestId();
                var raw = connection.Exchange(
                    LMC_DiagnosticsFrame.ReleaseBulk(
                        requestId,
                        configuration.BulkId,
                        configuration.ConfigRevision,
                        configuration.MapRevision),
                    sessionGeneration);
                LMC_DiagnosticsParser.ParseReleaseBulk(raw, requestId);
                connection.EnsureSessionGeneration(sessionGeneration);
                configuration.CompleteRelease();
            }
            catch
            {
                configuration.CancelRelease();
                throw;
            }
        }

        public async Task ReleaseBulkAsync(
            LMCBulkConfiguration configuration,
            CancellationToken cancellationToken)
        {
            await RunStateMutatingAsync(
                () => ReleaseBulk(configuration),
                cancellationToken).ConfigureAwait(false);
        }

        private void ValidateBulkCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            int signalCount)
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

            if (!capabilities.Supports(
                LMCDiagnosticCapability.BulkSnapshot))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise BulkSnapshot diagnostics.");
            }

            if (capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || capabilities.MaxBulkSignals == 0
                || capabilities.MaxBulkSignals
                    > LMC_DiagnosticsFrame.MaxBulkSignalCount
                || capabilities.SignalValueEntryStride
                    != LMC_DiagnosticsParser.SignalValueEntryStride)
            {
                throw new InvalidDataException(
                    "BulkSnapshot capability limits are invalid for diagnostics schema version 1.");
            }

            if (signalCount > capabilities.MaxBulkSignals)
            {
                throw new ArgumentOutOfRangeException(
                    "signalIds",
                    "The Bulk signal count exceeds the connected PLC capability.");
            }

            var configurePayloadBytes = checked(
                LMC_DiagnosticsFrame.ConfigureBulkRequestHeaderPayloadLength
                + signalCount * sizeof(uint));
            var snapshotPayloadBytes = checked(
                LMC_DiagnosticsParser.BulkSnapshotHeaderPayloadLength
                + signalCount * LMC_DiagnosticsParser.SignalValueEntryStride);
            if (configurePayloadBytes > capabilities.MaxRequestPayloadBytes
                || snapshotPayloadBytes > capabilities.MaxResponsePayloadBytes)
            {
                throw new InvalidDataException(
                    "BulkSnapshot capability payload limits cannot carry the requested configuration.");
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

        private long ValidateBulkConfiguration(
            LMCBulkConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (!ReferenceEquals(configuration.Owner, this))
            {
                throw new InvalidOperationException(
                    "The Bulk configuration belongs to a different LMCConnection.");
            }

            configuration.EnsureUsable();

            var sessionGeneration = connection.SessionGeneration;
            if (configuration.ConnectionSessionGeneration
                != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The Bulk configuration belongs to a stale connection session.");
            }

            lock (bulkBootIdSync)
            {
                if (!hasBulkBootId
                    || bulkBootIdSessionGeneration != sessionGeneration
                    || bulkBootId != configuration.DiagnosticsBootId)
                {
                    throw new InvalidOperationException(
                        "The Bulk configuration DiagnosticsBootId is stale.");
                }
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private void RememberBulkBootId(
            long sessionGeneration,
            uint diagnosticsBootId)
        {
            lock (bulkBootIdSync)
            {
                hasBulkBootId = true;
                bulkBootIdSessionGeneration = sessionGeneration;
                bulkBootId = diagnosticsBootId;
            }
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        public LMCPIBulkBuilder CreatePIBulkBuilder(
            LMCSignalCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            RequireCurrentSignalCatalog(catalog);
            return new LMCPIBulkBuilder(this, catalog);
        }

        public LMCSignalValue ReadPI(
            LMCSignalCatalog catalog,
            string alias)
        {
            var entry = GetReadablePIEntry(catalog, alias);
            return ReadCatalogPI(catalog, entry);
        }

        public Task<LMCSignalValue> ReadPIAsync(
            LMCSignalCatalog catalog,
            string alias,
            CancellationToken cancellationToken)
        {
            var entry = GetReadablePIEntry(catalog, alias);
            return ReadCatalogPIAsync(
                catalog,
                entry,
                cancellationToken);
        }

        internal void RequireCurrentSignalCatalog(
            LMCSignalCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            if (!catalog.IsBoundTo(this, sessionGeneration))
            {
                throw new InvalidOperationException(
                    "The Signal Catalog belongs to a different or stale diagnostics session. Reload the Catalog after connecting.");
            }
        }

        private LMCSignalCatalogEntry GetReadablePIEntry(
            LMCSignalCatalog catalog,
            string alias)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            if (alias == null)
            {
                throw new ArgumentNullException("alias");
            }

            if (alias.Length == 0)
            {
                throw new ArgumentException(
                    "PI alias must not be empty.",
                    "alias");
            }

            RequireCurrentSignalCatalog(catalog);
            var entry = catalog.GetByAlias(alias);
            if ((entry.AccessFlags & LMCSignalAccessFlags.Readable)
                != LMCSignalAccessFlags.Readable)
            {
                throw new InvalidOperationException(
                    "PI read access is not advertised for alias '"
                    + alias
                    + "'.");
            }

            byte expectedWidth;
            switch (entry.DataType)
            {
                case LMCSignalValueType.Bool:
                    expectedWidth = 1;
                    break;
                case LMCSignalValueType.Int16:
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    expectedWidth = 2;
                    break;
                case LMCSignalValueType.Int32:
                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.Real32:
                case LMCSignalValueType.BitField32:
                    expectedWidth = 4;
                    break;
                default:
                    throw new InvalidDataException(
                        "PI entry type is not supported by diagnostics schema version 1.");
            }

            if (entry.ByteWidth != expectedWidth)
            {
                throw new InvalidDataException(
                    "PI entry type and byte width are inconsistent.");
            }

            return entry;
        }

        private LMCSignalValue ReadCatalogPI(
            LMCSignalCatalog catalog,
            LMCSignalCatalogEntry entry)
        {
            var sessionGeneration = catalog.ConnectionSessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities(sessionGeneration);
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
                catalog.MapRevision,
                "ReadPI");
            return ReadPI(
                sessionGeneration,
                entry.SignalId,
                catalog.MapRevision,
                entry.DataType);
        }

        private async Task<LMCSignalValue> ReadCatalogPIAsync(
            LMCSignalCatalog catalog,
            LMCSignalCatalogEntry entry,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = catalog.ConnectionSessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
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
                catalog.MapRevision,
                "ReadPI");
            return await ReadPIAsync(
                sessionGeneration,
                entry.SignalId,
                catalog.MapRevision,
                entry.DataType,
                cancellationToken).ConfigureAwait(false);
        }
    }
}

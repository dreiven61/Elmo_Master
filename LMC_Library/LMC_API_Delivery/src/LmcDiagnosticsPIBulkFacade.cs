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

            return new LMCPIBulkBuilder(this, catalog);
        }

        public LMCSignalValue ReadPI(
            LMCSignalCatalog catalog,
            string alias)
        {
            var entry = GetReadablePIEntry(catalog, alias);
            return ReadPI(
                entry.SignalId,
                catalog.MapRevision,
                entry.DataType);
        }

        public Task<LMCSignalValue> ReadPIAsync(
            LMCSignalCatalog catalog,
            string alias,
            CancellationToken cancellationToken)
        {
            var entry = GetReadablePIEntry(catalog, alias);
            return ReadPIAsync(
                entry.SignalId,
                catalog.MapRevision,
                entry.DataType,
                cancellationToken);
        }

        private static LMCSignalCatalogEntry GetReadablePIEntry(
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
    }
}

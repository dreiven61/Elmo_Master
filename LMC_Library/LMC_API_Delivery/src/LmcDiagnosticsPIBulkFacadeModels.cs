using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed class LMCPIBulkBuilder
    {
        private const int BuilderStateMutable = 0;
        private const int BuilderStateConfiguring = 1;
        private const int BuilderStateFrozen = 2;

        private readonly object sync = new object();
        private readonly LMCDiagnostics diagnostics;
        private readonly LMCSignalCatalog catalog;
        private readonly List<LMCSignalCatalogEntry> entries =
            new List<LMCSignalCatalogEntry>();
        private readonly Dictionary<uint, LMCSignalCatalogEntry> entriesById =
            new Dictionary<uint, LMCSignalCatalogEntry>();
        private int state;

        internal LMCPIBulkBuilder(
            LMCDiagnostics diagnostics,
            LMCSignalCatalog catalog)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException("diagnostics");
            this.catalog = catalog
                ?? throw new ArgumentNullException("catalog");
        }

        public LMCSignalCatalog Catalog { get { return catalog; } }

        public int Count
        {
            get
            {
                lock (sync)
                {
                    return entries.Count;
                }
            }
        }

        public bool IsConfiguring
        {
            get
            {
                lock (sync)
                {
                    return state == BuilderStateConfiguring;
                }
            }
        }

        public bool IsFrozen
        {
            get
            {
                lock (sync)
                {
                    return state == BuilderStateFrozen;
                }
            }
        }

        public IReadOnlyList<LMCSignalCatalogEntry> Entries
        {
            get
            {
                lock (sync)
                {
                    return new ReadOnlyCollection<LMCSignalCatalogEntry>(
                        new List<LMCSignalCatalogEntry>(entries));
                }
            }
        }

        public void AddEntry(string alias)
        {
            if (alias == null)
            {
                throw new ArgumentNullException("alias");
            }

            if (alias.Length == 0)
            {
                throw new ArgumentException(
                    "Bulk entry alias must not be empty.",
                    "alias");
            }

            AddCanonicalEntry(catalog.GetByAlias(alias));
        }

        public void AddEntry(uint signalId)
        {
            AddCanonicalEntry(FindCatalogEntry(signalId));
        }

        public void AddEntry(LMCSignalCatalogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            AddCanonicalEntry(FindCatalogEntry(entry.SignalId));
        }

        public bool RemoveEntry(uint signalId)
        {
            lock (sync)
            {
                EnsureMutable();

                LMCSignalCatalogEntry entry;
                if (!entriesById.TryGetValue(signalId, out entry))
                {
                    return false;
                }

                entriesById.Remove(signalId);
                entries.Remove(entry);
                return true;
            }
        }

        public void Clear()
        {
            lock (sync)
            {
                EnsureMutable();
                entries.Clear();
                entriesById.Clear();
            }
        }

        public LMCPIBulkReader Configure()
        {
            var signalIds = BeginConfigure();

            try
            {
                var configuration = diagnostics.ConfigureBulkExact(
                    signalIds,
                    catalog.MapRevision);
                CompleteConfigure();
                return new LMCPIBulkReader(
                    diagnostics,
                    catalog,
                    configuration,
                    Entries);
            }
            catch
            {
                CancelConfigure();
                throw;
            }
        }

        public async Task<LMCPIBulkReader> ConfigureAsync(
            CancellationToken cancellationToken)
        {
            var signalIds = BeginConfigure();

            try
            {
                var configuration = await diagnostics.ConfigureBulkExactAsync(
                    signalIds,
                    catalog.MapRevision,
                    cancellationToken).ConfigureAwait(false);
                CompleteConfigure();
                return new LMCPIBulkReader(
                    diagnostics,
                    catalog,
                    configuration,
                    Entries);
            }
            catch
            {
                CancelConfigure();
                throw;
            }
        }

        private void AddCanonicalEntry(LMCSignalCatalogEntry entry)
        {
            ValidateBulkReadable(entry);

            lock (sync)
            {
                EnsureMutable();

                if (entries.Count >= LMC_DiagnosticsFrame.MaxBulkSignalCount)
                {
                    throw new InvalidOperationException(
                        "Bulk configurations cannot contain more than 32 signals.");
                }

                if (entriesById.ContainsKey(entry.SignalId))
                {
                    throw new ArgumentException(
                        "Bulk configurations do not allow duplicate SignalId values.",
                        "entry");
                }

                if (entries.Count != 0
                    && GetCapturePhase(entries[0]) != GetCapturePhase(entry))
                {
                    throw new InvalidOperationException(
                        "Bulk entries must use one common capture phase.");
                }

                entries.Add(entry);
                entriesById.Add(entry.SignalId, entry);
            }
        }

        private LMCSignalCatalogEntry FindCatalogEntry(uint signalId)
        {
            if (signalId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "signalId",
                    "SignalId must be non-zero.");
            }

            for (var index = 0; index < catalog.Entries.Count; index++)
            {
                var candidate = catalog.Entries[index];
                if (candidate.SignalId == signalId)
                {
                    return candidate;
                }
            }

            throw new KeyNotFoundException(
                "Signal Catalog SignalId was not found: 0x"
                + signalId.ToString("X8")
                + ".");
        }

        private uint[] BeginConfigure()
        {
            lock (sync)
            {
                EnsureMutable();
                if (entries.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Add at least one Bulk entry before Configure.");
                }

                var signalIds = new uint[entries.Count];
                for (var index = 0; index < signalIds.Length; index++)
                {
                    signalIds[index] = entries[index].SignalId;
                }

                state = BuilderStateConfiguring;
                return signalIds;
            }
        }

        private void CompleteConfigure()
        {
            lock (sync)
            {
                if (state != BuilderStateConfiguring)
                {
                    throw new InvalidOperationException(
                        "Bulk builder configuration state is invalid.");
                }

                state = BuilderStateFrozen;
            }
        }

        private void CancelConfigure()
        {
            lock (sync)
            {
                if (state == BuilderStateConfiguring)
                {
                    state = BuilderStateMutable;
                }
            }
        }

        private void EnsureMutable()
        {
            if (state == BuilderStateConfiguring)
            {
                throw new InvalidOperationException(
                    "The Bulk builder is currently being configured.");
            }

            if (state == BuilderStateFrozen)
            {
                throw new InvalidOperationException(
                    "The Bulk builder is frozen after a successful Configure.");
            }
        }

        private static void ValidateBulkReadable(
            LMCSignalCatalogEntry entry)
        {
            if ((entry.AccessFlags & LMCSignalAccessFlags.BulkReadable)
                != LMCSignalAccessFlags.BulkReadable)
            {
                throw new InvalidOperationException(
                    "BulkReadable access is not advertised for alias '"
                    + entry.Alias
                    + "'.");
            }

            ValidatePITypeAndWidth(entry);
            GetCapturePhase(entry);
        }

        private static void ValidatePITypeAndWidth(
            LMCSignalCatalogEntry entry)
        {
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
                        "Bulk entry type is not supported by diagnostics schema version 1.");
            }

            if (entry.ByteWidth != expectedWidth)
            {
                throw new InvalidDataException(
                    "Bulk entry type and byte width are inconsistent.");
            }
        }

        private static LMCCapturePhase GetCapturePhase(
            LMCSignalCatalogEntry entry)
        {
            var inputMapped = (entry.SignalFlags
                & LMCSignalFlags.InputMappedPhase) != 0;
            var preOutput = (entry.SignalFlags
                & LMCSignalFlags.PreOutputPhase) != 0;

            if (inputMapped == preOutput)
            {
                throw new InvalidDataException(
                    "Bulk entry capture phase metadata is invalid.");
            }

            return inputMapped
                ? LMCCapturePhase.InputMapped
                : LMCCapturePhase.PreOutput;
        }
    }

    public sealed class LMCPIBulkReader
    {
        private readonly LMCDiagnostics diagnostics;
        private readonly LMCSignalCatalog catalog;
        private readonly LMCBulkConfiguration configuration;
        private readonly ReadOnlyCollection<LMCSignalCatalogEntry> entries;
        private readonly SemaphoreSlim operationGate =
            new SemaphoreSlim(1, 1);
        private readonly object snapshotSync = new object();
        private LMCBulkSnapshot latestSnapshot;

        internal LMCPIBulkReader(
            LMCDiagnostics diagnostics,
            LMCSignalCatalog catalog,
            LMCBulkConfiguration configuration,
            IReadOnlyList<LMCSignalCatalogEntry> configuredEntries)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException("diagnostics");
            this.catalog = catalog
                ?? throw new ArgumentNullException("catalog");
            this.configuration = configuration
                ?? throw new ArgumentNullException("configuration");
            if (configuredEntries == null)
            {
                throw new ArgumentNullException("configuredEntries");
            }

            if (configuration.MapRevision != catalog.MapRevision
                || configuredEntries.Count != configuration.SignalCount)
            {
                throw new InvalidDataException(
                    "Bulk reader configuration does not match its Signal Catalog.");
            }

            var copiedEntries = new List<LMCSignalCatalogEntry>(
                configuredEntries.Count);
            for (var index = 0; index < configuredEntries.Count; index++)
            {
                var entry = configuredEntries[index];
                if (entry == null
                    || entry.SignalId != configuration.SignalIds[index])
                {
                    throw new InvalidDataException(
                        "Bulk reader entry order does not match its configuration.");
                }

                copiedEntries.Add(entry);
            }

            entries = new ReadOnlyCollection<LMCSignalCatalogEntry>(
                copiedEntries);
        }

        public LMCSignalCatalog Catalog { get { return catalog; } }
        public LMCBulkConfiguration Configuration { get { return configuration; } }
        public IReadOnlyList<LMCSignalCatalogEntry> Entries { get { return entries; } }
        public bool IsReleased { get { return configuration.IsReleased; } }

        public bool HasSnapshot
        {
            get
            {
                lock (snapshotSync)
                {
                    return !configuration.IsReleased
                        && latestSnapshot != null;
                }
            }
        }

        public LMCBulkSnapshot LatestSnapshot
        {
            get
            {
                operationGate.Wait();
                try
                {
                    EnsureNotReleased();
                    return RequireLatestSnapshot();
                }
                finally
                {
                    operationGate.Release();
                }
            }
        }

        public LMCBulkStatus ReadStatus()
        {
            operationGate.Wait();
            try
            {
                EnsureNotReleased();
                return diagnostics.ReadBulkStatus(configuration);
            }
            finally
            {
                operationGate.Release();
            }
        }

        public async Task<LMCBulkStatus> ReadStatusAsync(
            CancellationToken cancellationToken)
        {
            await operationGate.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            try
            {
                EnsureNotReleased();
                return await diagnostics.ReadBulkStatusAsync(
                    configuration,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                operationGate.Release();
            }
        }

        public LMCBulkSnapshot Upload()
        {
            operationGate.Wait();
            try
            {
                EnsureNotReleased();
                var snapshot = diagnostics.ReadBulk(configuration);
                RememberSnapshot(snapshot);
                return snapshot;
            }
            finally
            {
                operationGate.Release();
            }
        }

        public async Task<LMCBulkSnapshot> UploadAsync(
            CancellationToken cancellationToken)
        {
            await operationGate.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            try
            {
                EnsureNotReleased();
                var snapshot = await diagnostics.ReadBulkAsync(
                    configuration,
                    cancellationToken).ConfigureAwait(false);
                RememberSnapshot(snapshot);
                return snapshot;
            }
            finally
            {
                operationGate.Release();
            }
        }

        public LMCSignalValueEntry GetEntry(string alias)
        {
            if (alias == null)
            {
                throw new ArgumentNullException("alias");
            }

            return GetEntry(catalog.GetByAlias(alias).SignalId);
        }

        public LMCSignalValueEntry GetEntry(uint signalId)
        {
            operationGate.Wait();
            try
            {
                EnsureNotReleased();
                var snapshot = RequireLatestSnapshot();
                for (var index = 0; index < snapshot.Entries.Count; index++)
                {
                    var entry = snapshot.Entries[index];
                    if (entry.SignalId == signalId)
                    {
                        return entry;
                    }
                }

                throw new KeyNotFoundException(
                    "The latest Bulk snapshot does not contain SignalId 0x"
                    + signalId.ToString("X8")
                    + ".");
            }
            finally
            {
                operationGate.Release();
            }
        }

        public bool TryGetEntry(
            string alias,
            out LMCSignalValueEntry entry)
        {
            LMCSignalCatalogEntry catalogEntry;
            if (!catalog.TryGetByAlias(alias, out catalogEntry))
            {
                entry = null;
                return false;
            }

            return TryGetEntry(catalogEntry.SignalId, out entry);
        }

        public bool TryGetEntry(
            uint signalId,
            out LMCSignalValueEntry entry)
        {
            operationGate.Wait();
            try
            {
                EnsureNotReleased();

                LMCBulkSnapshot snapshot;
                lock (snapshotSync)
                {
                    snapshot = latestSnapshot;
                }

                if (snapshot != null)
                {
                    for (var index = 0; index < snapshot.Entries.Count; index++)
                    {
                        if (snapshot.Entries[index].SignalId == signalId)
                        {
                            entry = snapshot.Entries[index];
                            return true;
                        }
                    }
                }

                entry = null;
                return false;
            }
            finally
            {
                operationGate.Release();
            }
        }

        public void Release()
        {
            operationGate.Wait();
            try
            {
                EnsureNotReleased();
                diagnostics.ReleaseBulk(configuration);
            }
            finally
            {
                operationGate.Release();
            }
        }

        public async Task ReleaseAsync(
            CancellationToken cancellationToken)
        {
            await operationGate.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            try
            {
                EnsureNotReleased();
                await diagnostics.ReleaseBulkAsync(
                    configuration,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                operationGate.Release();
            }
        }

        private void RememberSnapshot(LMCBulkSnapshot snapshot)
        {
            lock (snapshotSync)
            {
                latestSnapshot = snapshot;
            }
        }

        private LMCBulkSnapshot RequireLatestSnapshot()
        {
            lock (snapshotSync)
            {
                if (latestSnapshot == null)
                {
                    throw new InvalidOperationException(
                        "Upload the Bulk snapshot before calling GetEntry.");
                }

                return latestSnapshot;
            }
        }

        private void EnsureNotReleased()
        {
            if (configuration.IsReleased)
            {
                throw new InvalidOperationException(
                    "The Bulk reader has already been released.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LasalMotionControlLib
{
    public enum LMCSignalSourceKind : byte
    {
        Invalid = 0,
        System = 1,
        EtherCATMaster = 2,
        EtherCATSlave = 3,
        PdoInput = 4,
        PdoOutputLastTx = 5,
        MotionAxis = 6,
        PlcApplication = 7
    }

    public enum LMCSignalValueType : byte
    {
        Invalid = 0,
        Bool = 1,
        Int16 = 2,
        UInt16 = 3,
        Int32 = 4,
        UInt32 = 5,
        Real32 = 6,
        BitField16 = 7,
        BitField32 = 8
    }

    public enum LMCCapturePhase : ushort
    {
        None = 0,
        InputMapped = 1,
        PreOutput = 2
    }

    public enum LMCPdoDirection : byte
    {
        None = 0,
        MasterToDrive = 1,
        DriveToMaster = 2
    }

    [Flags]
    public enum LMCSignalAccessFlags : ushort
    {
        None = 0,
        Readable = 1 << 0,
        WritableByPolicy = 1 << 1,
        Recordable = 1 << 2,
        BulkReadable = 1 << 3
    }

    [Flags]
    public enum LMCSignalFlags : ushort
    {
        None = 0,
        ActivePdo = 1 << 0,
        PhysicalAxis = 1 << 1,
        SoftwareAxis = 1 << 2,
        InputMappedPhase = 1 << 3,
        PreOutputPhase = 1 << 4,
        HealthSignal = 1 << 5
    }

    [Flags]
    public enum LMCSignalEntryStatus : byte
    {
        None = 0,
        Valid = 1 << 0,
        StaleFrame = 1 << 1,
        MasterNotOperational = 1 << 2,
        SlaveOffline = 1 << 3,
        SlaveNotOperational = 1 << 4,
        AlError = 1 << 5,
        NotMapped = 1 << 6,
        SourceUnavailable = 1 << 7
    }

    [Flags]
    public enum LMCSignalCatalogFlags : uint
    {
        None = 0,
        FixedStride = 1u << 0,
        AliasAscii7Bit = 1u << 1,
        CanonicalCrc = 1u << 2,
        OpaqueSignalId = 1u << 3
    }

    public enum LMCDiagnosticsCrcKind : uint
    {
        None = 0,
        Crc32IsoHdlc = 1
    }

    [Flags]
    public enum LMCEtherCATMasterFlags : ushort
    {
        None = 0,
        MasterOperational = 1 << 0,
        InvalidFrameActive = 1 << 1
    }

    public sealed class LMCSignalCatalogInfo
    {
        internal LMCSignalCatalogInfo(
            LMCDiagnosticsResponse response,
            uint mapRevision,
            ushort totalCount,
            ushort entryStride,
            ushort aliasBytes,
            ushort signalIdBytes,
            uint catalogFlags,
            uint crcKind)
        {
            Response = response;
            MapRevision = mapRevision;
            TotalCount = totalCount;
            EntryStride = entryStride;
            AliasBytes = aliasBytes;
            SignalIdBytes = signalIdBytes;
            CatalogFlagsValue = catalogFlags;
            CrcKindValue = crcKind;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint MapRevision { get; private set; }
        public ushort TotalCount { get; private set; }
        public ushort EntryStride { get; private set; }
        public ushort AliasBytes { get; private set; }
        public ushort SignalIdBytes { get; private set; }
        public uint CatalogFlagsValue { get; private set; }
        public uint CrcKindValue { get; private set; }

        public LMCSignalCatalogFlags CatalogFlags
        {
            get { return (LMCSignalCatalogFlags)CatalogFlagsValue; }
        }

        public LMCDiagnosticsCrcKind CrcKind
        {
            get { return (LMCDiagnosticsCrcKind)CrcKindValue; }
        }
    }

    public sealed class LMCSignalCatalogEntry
    {
        internal LMCSignalCatalogEntry(
            uint signalId,
            ushort catalogIndex,
            LMCSignalSourceKind sourceKind,
            byte sourceIndex,
            LMCSignalValueType dataType,
            byte byteWidth,
            ushort unitCode,
            LMCSignalAccessFlags accessFlags,
            LMCSignalFlags signalFlags,
            ushort pdoIndex,
            byte pdoSubIndex,
            LMCPdoDirection pdoDirection,
            int scaleNumerator,
            int scaleDenominator,
            int minimumRaw,
            int maximumRaw,
            string alias)
        {
            SignalId = signalId;
            CatalogIndex = catalogIndex;
            SourceKind = sourceKind;
            SourceIndex = sourceIndex;
            DataType = dataType;
            ByteWidth = byteWidth;
            UnitCode = unitCode;
            AccessFlags = accessFlags;
            SignalFlags = signalFlags;
            PdoIndex = pdoIndex;
            PdoSubIndex = pdoSubIndex;
            PdoDirection = pdoDirection;
            ScaleNumerator = scaleNumerator;
            ScaleDenominator = scaleDenominator;
            MinimumRaw = minimumRaw;
            MaximumRaw = maximumRaw;
            Alias = alias;
        }

        public uint SignalId { get; private set; }
        public ushort CatalogIndex { get; private set; }
        public LMCSignalSourceKind SourceKind { get; private set; }
        public byte SourceIndex { get; private set; }
        public LMCSignalValueType DataType { get; private set; }
        public byte ByteWidth { get; private set; }
        public ushort UnitCode { get; private set; }
        public LMCSignalAccessFlags AccessFlags { get; private set; }
        public LMCSignalFlags SignalFlags { get; private set; }
        public ushort PdoIndex { get; private set; }
        public byte PdoSubIndex { get; private set; }
        public LMCPdoDirection PdoDirection { get; private set; }
        public int ScaleNumerator { get; private set; }
        public int ScaleDenominator { get; private set; }
        public int MinimumRaw { get; private set; }
        public int MaximumRaw { get; private set; }
        public string Alias { get; private set; }
    }

    public sealed class LMCSignalCatalogChunk
    {
        private readonly ReadOnlyCollection<LMCSignalCatalogEntry> entries;

        internal LMCSignalCatalogChunk(
            LMCDiagnosticsResponse response,
            uint mapRevision,
            ushort startIndex,
            ushort totalCount,
            ushort entryStride,
            IList<LMCSignalCatalogEntry> entries)
        {
            Response = response;
            MapRevision = mapRevision;
            StartIndex = startIndex;
            TotalCount = totalCount;
            EntryStride = entryStride;
            this.entries = new ReadOnlyCollection<LMCSignalCatalogEntry>(
                new List<LMCSignalCatalogEntry>(entries));
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint MapRevision { get; private set; }
        public ushort StartIndex { get; private set; }
        public ushort TotalCount { get; private set; }
        public ushort EntryStride { get; private set; }
        public ushort ReturnedCount
        {
            get { return checked((ushort)entries.Count); }
        }

        public IReadOnlyList<LMCSignalCatalogEntry> Entries
        {
            get { return entries; }
        }
    }

    public sealed class LMCSignalCatalog
    {
        private readonly ReadOnlyCollection<LMCSignalCatalogEntry> entries;
        private readonly Dictionary<string, LMCSignalCatalogEntry> entriesByAlias;

        internal LMCSignalCatalog(
            LMCSignalCatalogInfo info,
            IList<LMCSignalCatalogEntry> entries)
        {
            Info = info ?? throw new ArgumentNullException("info");
            this.entries = new ReadOnlyCollection<LMCSignalCatalogEntry>(
                new List<LMCSignalCatalogEntry>(entries));
            entriesByAlias = new Dictionary<string, LMCSignalCatalogEntry>(
                StringComparer.Ordinal);

            foreach (var entry in this.entries)
            {
                if (entriesByAlias.ContainsKey(entry.Alias))
                {
                    throw new System.IO.InvalidDataException(
                        "Signal Catalog contains duplicate alias '"
                        + entry.Alias
                        + "'.");
                }

                entriesByAlias.Add(entry.Alias, entry);
            }
        }

        public LMCSignalCatalogInfo Info { get; private set; }
        public uint MapRevision { get { return Info.MapRevision; } }
        public IReadOnlyList<LMCSignalCatalogEntry> Entries { get { return entries; } }

        public bool TryGetByAlias(
            string alias,
            out LMCSignalCatalogEntry entry)
        {
            if (alias == null)
            {
                entry = null;
                return false;
            }

            return entriesByAlias.TryGetValue(alias, out entry);
        }

        public LMCSignalCatalogEntry GetByAlias(string alias)
        {
            LMCSignalCatalogEntry entry;
            if (!TryGetByAlias(alias, out entry))
            {
                throw new KeyNotFoundException(
                    "Signal Catalog alias was not found: " + alias + ".");
            }

            return entry;
        }
    }

    public sealed class LMCEtherCATSlaveHealth
    {
        internal LMCEtherCATSlaveHealth(
            ushort slaveIndex,
            ushort physicalAxis,
            bool online,
            byte etherCATState,
            ushort alStatusCode,
            uint slaveStateBits,
            uint classState,
            uint ds402StatusWord,
            uint axisError,
            uint lastValidCycle,
            uint lastStateChangeCycle)
        {
            SlaveIndex = slaveIndex;
            PhysicalAxis = physicalAxis;
            Online = online;
            EtherCATState = etherCATState;
            ALStatusCode = alStatusCode;
            SlaveStateBits = slaveStateBits;
            ClassState = classState;
            DS402StatusWord = ds402StatusWord;
            AxisError = axisError;
            LastValidCycle = lastValidCycle;
            LastStateChangeCycle = lastStateChangeCycle;
        }

        public ushort SlaveIndex { get; private set; }
        public ushort PhysicalAxis { get; private set; }
        public bool Online { get; private set; }
        public byte EtherCATState { get; private set; }
        public ushort ALStatusCode { get; private set; }
        public uint SlaveStateBits { get; private set; }
        public uint ClassState { get; private set; }
        public uint DS402StatusWord { get; private set; }
        public uint AxisError { get; private set; }
        public uint LastValidCycle { get; private set; }
        public uint LastStateChangeCycle { get; private set; }
    }

    public sealed class LMCEtherCATHealth
    {
        private readonly ReadOnlyCollection<LMCEtherCATSlaveHealth> slaves;

        internal LMCEtherCATHealth(
            LMCDiagnosticsResponse response,
            uint mapRevision,
            LMCCapturePhase capturePhase,
            uint cycleCounter,
            uint timestampLow,
            uint timestampHigh,
            ushort masterState,
            LMCEtherCATMasterFlags masterFlags,
            uint consecutiveInvalidCycles,
            uint invalidCycleTotal,
            uint frameTimeUs,
            uint frameTimeMaxUs,
            uint rtTimeUs,
            uint rtTimeMaxUs,
            uint snapshotSequence,
            IList<LMCEtherCATSlaveHealth> slaves)
        {
            Response = response;
            MapRevision = mapRevision;
            CapturePhase = capturePhase;
            CycleCounter = cycleCounter;
            TimestampLow = timestampLow;
            TimestampHigh = timestampHigh;
            MasterState = masterState;
            MasterFlags = masterFlags;
            ConsecutiveInvalidCycles = consecutiveInvalidCycles;
            InvalidCycleTotal = invalidCycleTotal;
            FrameTimeUs = frameTimeUs;
            FrameTimeMaxUs = frameTimeMaxUs;
            RtTimeUs = rtTimeUs;
            RtTimeMaxUs = rtTimeMaxUs;
            SnapshotSequence = snapshotSequence;
            this.slaves = new ReadOnlyCollection<LMCEtherCATSlaveHealth>(
                new List<LMCEtherCATSlaveHealth>(slaves));
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public uint CycleCounter { get; private set; }
        public uint TimestampLow { get; private set; }
        public uint TimestampHigh { get; private set; }
        public ulong TimestampUs
        {
            get { return ((ulong)TimestampHigh << 32) | TimestampLow; }
        }
        public ushort MasterState { get; private set; }
        public LMCEtherCATMasterFlags MasterFlags { get; private set; }
        public uint ConsecutiveInvalidCycles { get; private set; }
        public uint InvalidCycleTotal { get; private set; }
        public uint FrameTimeUs { get; private set; }
        public uint FrameTimeMaxUs { get; private set; }
        public uint RtTimeUs { get; private set; }
        public uint RtTimeMaxUs { get; private set; }
        public uint SnapshotSequence { get; private set; }
        public IReadOnlyList<LMCEtherCATSlaveHealth> Slaves { get { return slaves; } }
    }

    public sealed class LMCSignalValueEntry
    {
        internal LMCSignalValueEntry(
            uint signalId,
            uint rawValue32,
            LMCSignalValueType valueType,
            LMCSignalEntryStatus entryStatus,
            uint detailCode)
        {
            SignalId = signalId;
            RawValue32 = rawValue32;
            ValueType = valueType;
            EntryStatus = entryStatus;
            DetailCode = detailCode;
        }

        public uint SignalId { get; private set; }
        public uint RawValue32 { get; private set; }
        public int RawInt32 { get { return unchecked((int)RawValue32); } }
        public LMCSignalValueType ValueType { get; private set; }
        public LMCSignalEntryStatus EntryStatus { get; private set; }
        public uint DetailCode { get; private set; }
        public bool IsValid
        {
            get { return EntryStatus == LMCSignalEntryStatus.Valid; }
        }

        public LMCDiagnosticsDetailCode Detail
        {
            get { return (LMCDiagnosticsDetailCode)DetailCode; }
        }
    }

    public sealed class LMCSignalValue
    {
        internal LMCSignalValue(
            LMCDiagnosticsResponse response,
            uint mapRevision,
            LMCCapturePhase capturePhase,
            uint cycleCounter,
            uint timestampLow,
            uint timestampHigh,
            LMCSignalValueEntry entry)
        {
            Response = response;
            MapRevision = mapRevision;
            CapturePhase = capturePhase;
            CycleCounter = cycleCounter;
            TimestampLow = timestampLow;
            TimestampHigh = timestampHigh;
            Entry = entry;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public uint CycleCounter { get; private set; }
        public uint TimestampLow { get; private set; }
        public uint TimestampHigh { get; private set; }
        public ulong TimestampUs
        {
            get { return ((ulong)TimestampHigh << 32) | TimestampLow; }
        }
        public LMCSignalValueEntry Entry { get; private set; }
        public uint SignalId { get { return Entry.SignalId; } }
        public uint RawValue32 { get { return Entry.RawValue32; } }
        public int RawInt32 { get { return Entry.RawInt32; } }
        public LMCSignalValueType ValueType { get { return Entry.ValueType; } }
        public LMCSignalEntryStatus EntryStatus { get { return Entry.EntryStatus; } }
        public bool IsValid { get { return Entry.IsValid; } }
    }
}

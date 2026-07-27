using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LasalMotionControlLib
{
    [Flags]
    public enum LMCEtherCATTopologyFlags : uint
    {
        None = 0,
        FixedStride = 1u << 0,
        NameAscii7Bit = 1u << 1,
        CanonicalCrc = 1u << 2,
        OpaqueNodeId = 1u << 3
    }

    public enum LMCEtherCATTopologyNodeKind : byte
    {
        Invalid = 0,
        EtherCATSlave = 1,
        SlotModule = 2
    }

    [Flags]
    public enum LMCEtherCATTopologyNodeFlags : ushort
    {
        None = 0,
        HasMasterSlaveIndex = 1 << 0,
        SupportsSdo = 1 << 1,
        PhysicalAxis = 1 << 2,
        HasInputs = 1 << 3,
        HasOutputs = 1 << 4,
        Ds402Drive = 1 << 5,
        IoCoupler = 1 << 6,
        HasDigitalIO = 1 << 7
    }

    [Flags]
    public enum LMCEtherCATNodeHealthFlags : ushort
    {
        None = 0,
        Configured = 1 << 0,
        Detected = 1 << 1,
        IdentityMatched = 1 << 2,
        DataValid = 1 << 3,
        DataDefaulted = 1 << 4,
        Ds402DataPresent = 1 << 5
    }

    public enum LMCDigitalIODirection : byte
    {
        Invalid = 0,
        Input = 1,
        Output = 2
    }

    [Flags]
    public enum LMCDigitalIOStatusFlags : ushort
    {
        None = 0,
        Valid = 1 << 0,
        StaleFrame = 1 << 1,
        MasterNotOperational = 1 << 2,
        NodeOffline = 1 << 3,
        NodeNotOperational = 1 << 4,
        AlError = 1 << 5,
        SourceUnavailable = 1 << 6,
        IdentityMismatch = 1 << 7,
        DataDefaulted = 1 << 8
    }

    public sealed class LMCEtherCATTopologyInfo
    {
        internal LMCEtherCATTopologyInfo(
            LMCDiagnosticsResponse response,
            uint topologyRevision,
            ushort totalNodeCount,
            ushort entryStride,
            ushort maxEntriesPerChunk,
            ushort configuredSlaveCount,
            ushort slotModuleCount,
            ushort physicalAxisCount,
            uint topologyFlags,
            uint crcKind)
        {
            Response = response;
            TopologyRevision = topologyRevision;
            TotalNodeCount = totalNodeCount;
            EntryStride = entryStride;
            MaxEntriesPerChunk = maxEntriesPerChunk;
            ConfiguredSlaveCount = configuredSlaveCount;
            SlotModuleCount = slotModuleCount;
            PhysicalAxisCount = physicalAxisCount;
            TopologyFlagsValue = topologyFlags;
            CrcKindValue = crcKind;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint TopologyRevision { get; private set; }
        public ushort TotalNodeCount { get; private set; }
        public ushort EntryStride { get; private set; }
        public ushort MaxEntriesPerChunk { get; private set; }
        public ushort ConfiguredSlaveCount { get; private set; }
        public ushort SlotModuleCount { get; private set; }
        public ushort PhysicalAxisCount { get; private set; }
        public uint TopologyFlagsValue { get; private set; }
        public uint CrcKindValue { get; private set; }

        public LMCEtherCATTopologyFlags TopologyFlags
        {
            get { return (LMCEtherCATTopologyFlags)TopologyFlagsValue; }
        }

        public LMCDiagnosticsCrcKind CrcKind
        {
            get { return (LMCDiagnosticsCrcKind)CrcKindValue; }
        }
    }

    public sealed class LMCEtherCATTopologyEntry
    {
        internal LMCEtherCATTopologyEntry(
            uint nodeId,
            uint parentNodeId,
            ushort topologyIndex,
            ushort masterSlaveIndex,
            LMCEtherCATTopologyNodeKind nodeKind,
            LMCEtherCATTopologyNodeFlags nodeFlags,
            ushort sdoSlaveReference,
            ushort physicalAxisReference,
            ushort slotIndex,
            uint vendorId,
            uint productCode,
            uint revisionNumber,
            uint serialNumber,
            ushort inputBytes,
            ushort outputBytes,
            string name,
            uint ioReference)
        {
            NodeId = nodeId;
            ParentNodeId = parentNodeId;
            TopologyIndex = topologyIndex;
            MasterSlaveIndex = masterSlaveIndex;
            NodeKind = nodeKind;
            NodeFlags = nodeFlags;
            SdoSlaveReference = sdoSlaveReference;
            PhysicalAxisReference = physicalAxisReference;
            SlotIndex = slotIndex;
            VendorId = vendorId;
            ProductCode = productCode;
            RevisionNumber = revisionNumber;
            SerialNumber = serialNumber;
            InputBytes = inputBytes;
            OutputBytes = outputBytes;
            Name = name;
            IOReference = ioReference;
        }

        public uint NodeId { get; private set; }
        public uint ParentNodeId { get; private set; }
        public ushort TopologyIndex { get; private set; }
        public ushort MasterSlaveIndex { get; private set; }
        public LMCEtherCATTopologyNodeKind NodeKind { get; private set; }
        public LMCEtherCATTopologyNodeFlags NodeFlags { get; private set; }
        public ushort SdoSlaveReference { get; private set; }
        public ushort PhysicalAxisReference { get; private set; }
        public ushort SlotIndex { get; private set; }
        public uint VendorId { get; private set; }
        public uint ProductCode { get; private set; }
        public uint RevisionNumber { get; private set; }
        public uint SerialNumber { get; private set; }
        public ushort InputBytes { get; private set; }
        public ushort OutputBytes { get; private set; }
        public string Name { get; private set; }
        public uint IOReference { get; private set; }

        public bool HasMasterSlaveIndex
        {
            get { return MasterSlaveIndex != ushort.MaxValue; }
        }

        public bool HasSlotIndex
        {
            get { return SlotIndex != ushort.MaxValue; }
        }
    }

    public sealed class LMCEtherCATTopologyChunk
    {
        private readonly ReadOnlyCollection<LMCEtherCATTopologyEntry> entries;

        internal LMCEtherCATTopologyChunk(
            LMCDiagnosticsResponse response,
            uint topologyRevision,
            ushort startIndex,
            ushort totalNodeCount,
            ushort entryStride,
            IList<LMCEtherCATTopologyEntry> entries)
        {
            Response = response;
            TopologyRevision = topologyRevision;
            StartIndex = startIndex;
            TotalNodeCount = totalNodeCount;
            EntryStride = entryStride;
            this.entries = new ReadOnlyCollection<LMCEtherCATTopologyEntry>(
                new List<LMCEtherCATTopologyEntry>(entries));
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint TopologyRevision { get; private set; }
        public ushort StartIndex { get; private set; }
        public ushort TotalNodeCount { get; private set; }
        public ushort EntryStride { get; private set; }
        public ushort ReturnedCount
        {
            get { return checked((ushort)entries.Count); }
        }

        public IReadOnlyList<LMCEtherCATTopologyEntry> Entries
        {
            get { return entries; }
        }
    }

    public sealed class LMCEtherCATTopology
    {
        private readonly ReadOnlyCollection<LMCEtherCATTopologyEntry> entries;
        private readonly Dictionary<uint, LMCEtherCATTopologyEntry> entriesByNodeId;

        internal LMCEtherCATTopology(
            LMCEtherCATTopologyInfo info,
            IList<LMCEtherCATTopologyEntry> entries)
        {
            Info = info ?? throw new ArgumentNullException("info");
            this.entries = new ReadOnlyCollection<LMCEtherCATTopologyEntry>(
                new List<LMCEtherCATTopologyEntry>(entries));
            entriesByNodeId = new Dictionary<uint, LMCEtherCATTopologyEntry>();

            foreach (var entry in this.entries)
            {
                if (entriesByNodeId.ContainsKey(entry.NodeId))
                {
                    throw new System.IO.InvalidDataException(
                        "EtherCAT topology contains duplicate NodeId values.");
                }

                entriesByNodeId.Add(entry.NodeId, entry);
            }
        }

        public LMCEtherCATTopologyInfo Info { get; private set; }
        public uint TopologyRevision { get { return Info.TopologyRevision; } }
        public IReadOnlyList<LMCEtherCATTopologyEntry> Entries { get { return entries; } }

        public bool TryGetNode(
            uint nodeId,
            out LMCEtherCATTopologyEntry entry)
        {
            return entriesByNodeId.TryGetValue(nodeId, out entry);
        }
    }

    public sealed class LMCEtherCATNodeHealth
    {
        internal LMCEtherCATNodeHealth(
            LMCDiagnosticsResponse response,
            uint topologyRevision,
            uint nodeId,
            LMCCapturePhase capturePhase,
            LMCEtherCATNodeHealthFlags healthFlags,
            uint cycleCounter,
            ulong timestampMicroseconds,
            uint snapshotSequence,
            bool online,
            byte etherCATState,
            ushort alStatusCode,
            uint slaveState,
            uint classState,
            uint ds402StatusWord,
            uint axisError,
            uint lastValidCycle,
            uint lastStateChangeCycle)
        {
            Response = response;
            TopologyRevision = topologyRevision;
            NodeId = nodeId;
            CapturePhase = capturePhase;
            HealthFlags = healthFlags;
            CycleCounter = cycleCounter;
            TimestampMicroseconds = timestampMicroseconds;
            SnapshotSequence = snapshotSequence;
            Online = online;
            EtherCATState = etherCATState;
            ALStatusCode = alStatusCode;
            SlaveState = slaveState;
            ClassState = classState;
            DS402StatusWord = ds402StatusWord;
            AxisError = axisError;
            LastValidCycle = lastValidCycle;
            LastStateChangeCycle = lastStateChangeCycle;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint TopologyRevision { get; private set; }
        public uint NodeId { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public LMCEtherCATNodeHealthFlags HealthFlags { get; private set; }
        public uint CycleCounter { get; private set; }
        public ulong TimestampMicroseconds { get; private set; }
        public uint SnapshotSequence { get; private set; }
        public bool Online { get; private set; }
        public byte EtherCATState { get; private set; }
        public ushort ALStatusCode { get; private set; }
        public uint SlaveState { get; private set; }
        public uint ClassState { get; private set; }
        public uint DS402StatusWord { get; private set; }
        public uint AxisError { get; private set; }
        public uint LastValidCycle { get; private set; }
        public uint LastStateChangeCycle { get; private set; }
    }

    public sealed class LMCDigitalIOReadRequest
    {
        public LMCDigitalIOReadRequest(
            uint topologyRevision,
            uint ioReference,
            LMCDigitalIODirection expectedDirection,
            byte expectedBitWidth)
        {
            if (topologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException("topologyRevision");
            }

            if (ioReference == 0)
            {
                throw new ArgumentOutOfRangeException("ioReference");
            }

            if (expectedDirection != LMCDigitalIODirection.Input
                && expectedDirection != LMCDigitalIODirection.Output)
            {
                throw new ArgumentOutOfRangeException("expectedDirection");
            }

            if (expectedBitWidth == 0 || expectedBitWidth > 64)
            {
                throw new ArgumentOutOfRangeException("expectedBitWidth");
            }

            TopologyRevision = topologyRevision;
            IOReference = ioReference;
            ExpectedDirection = expectedDirection;
            ExpectedBitWidth = expectedBitWidth;
        }

        public uint TopologyRevision { get; private set; }
        public uint IOReference { get; private set; }
        public LMCDigitalIODirection ExpectedDirection { get; private set; }
        public byte ExpectedBitWidth { get; private set; }
    }

    public sealed class LMCDigitalIOValue
    {
        internal LMCDigitalIOValue(
            LMCDiagnosticsResponse response,
            uint topologyRevision,
            uint ioReference,
            uint nodeId,
            LMCDigitalIODirection direction,
            byte bitWidth,
            LMCDigitalIOStatusFlags statusFlags,
            ulong value,
            ulong validMask,
            uint cycleCounter,
            uint outputRevision)
        {
            Response = response;
            TopologyRevision = topologyRevision;
            IOReference = ioReference;
            NodeId = nodeId;
            Direction = direction;
            BitWidth = bitWidth;
            StatusFlags = statusFlags;
            Value = value;
            ValidMask = validMask;
            CycleCounter = cycleCounter;
            OutputRevision = outputRevision;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint TopologyRevision { get; private set; }
        public uint IOReference { get; private set; }
        public uint NodeId { get; private set; }
        public LMCDigitalIODirection Direction { get; private set; }
        public byte BitWidth { get; private set; }
        public LMCDigitalIOStatusFlags StatusFlags { get; private set; }
        public ulong Value { get; private set; }
        public ulong ValidMask { get; private set; }
        public uint CycleCounter { get; private set; }
        public uint OutputRevision { get; private set; }

        public bool IsValid
        {
            get { return (StatusFlags & LMCDigitalIOStatusFlags.Valid) != 0; }
        }
    }

    public sealed class LMCDigitalOutputWriteRequest
    {
        public LMCDigitalOutputWriteRequest(
            uint topologyRevision,
            uint ioReference,
            ulong value,
            ulong mask,
            uint expectedOutputRevision)
        {
            if (topologyRevision == 0)
            {
                throw new ArgumentOutOfRangeException("topologyRevision");
            }

            if (ioReference == 0)
            {
                throw new ArgumentOutOfRangeException("ioReference");
            }

            if (mask == 0)
            {
                throw new ArgumentOutOfRangeException("mask");
            }

            if ((value & ~mask) != 0)
            {
                throw new ArgumentException(
                    "Digital output Value must be canonical outside Mask.",
                    "value");
            }

            if (expectedOutputRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedOutputRevision");
            }

            TopologyRevision = topologyRevision;
            IOReference = ioReference;
            Value = value;
            Mask = mask;
            ExpectedOutputRevision = expectedOutputRevision;
        }

        public uint TopologyRevision { get; private set; }
        public uint IOReference { get; private set; }
        public ulong Value { get; private set; }
        public ulong Mask { get; private set; }
        public uint ExpectedOutputRevision { get; private set; }
    }

    internal static class LMCDiagnosticsDigitalOutputWritePolicy
    {
        private static readonly ReadOnlyCollection<uint> ApprovedIOReferences =
            new ReadOnlyCollection<uint>(new uint[0]);

        internal static IReadOnlyList<uint> GetApprovedIOReferences()
        {
            return ApprovedIOReferences;
        }

        internal static bool IsApproved(uint ioReference)
        {
            return ApprovedIOReferences.Contains(ioReference);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace LasalMotionControlLib
{
    public enum LMCDigitalOutputWriteSubmissionPhase
    {
        RequestValidation = 0,
        SessionPreflight = 1,
        CapabilityPreflight = 2,
        Submission = 3,
        PostSubmissionValidation = 4
    }

    public enum LMCDigitalOutputWriteSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    /// <summary>
    /// Immutable failure context for SubmitDigitalOutputWrite and
    /// SubmitDigitalOutputWriteAsync. The original exception object and type
    /// are preserved; call TryGet with the caught exception to distinguish a
    /// local preflight failure, explicit PLC rejection, uncertain wire
    /// outcome, and an accepted ticket followed by session validation failure.
    /// </summary>
    public sealed class LMCDigitalOutputWriteSubmissionFailureContext
    {
        private static readonly object FailureContextSync = new object();
        private static readonly ConditionalWeakTable<
            Exception,
            LMCDigitalOutputWriteSubmissionFailureContext> FailureContexts =
                new ConditionalWeakTable<
                    Exception,
                    LMCDigitalOutputWriteSubmissionFailureContext>();

        internal LMCDigitalOutputWriteSubmissionFailureContext(
            LMCDigitalOutputWriteRequest request,
            LMCDigitalOutputWriteSubmissionPhase phase,
            LMCDigitalOutputWriteSubmissionOutcome submissionOutcome,
            uint diagnosticsBootId,
            uint topologyRevision,
            LMCOperationTicket ticket)
        {
            if (!Enum.IsDefined(
                typeof(LMCDigitalOutputWriteSubmissionPhase),
                phase))
            {
                throw new ArgumentOutOfRangeException("phase");
            }

            if (!Enum.IsDefined(
                typeof(LMCDigitalOutputWriteSubmissionOutcome),
                submissionOutcome))
            {
                throw new ArgumentOutOfRangeException("submissionOutcome");
            }

            if (submissionOutcome
                    != LMCDigitalOutputWriteSubmissionOutcome.NotAttempted
                && request == null)
            {
                throw new ArgumentNullException(
                    "request",
                    "A dispatched digital-output submission requires its request.");
            }

            if (request == null
                && phase
                    != LMCDigitalOutputWriteSubmissionPhase.RequestValidation)
            {
                throw new ArgumentException(
                    "A null digital-output request can fail only during request validation.",
                    "phase");
            }

            if (submissionOutcome
                == LMCDigitalOutputWriteSubmissionOutcome.Accepted)
            {
                if (ticket == null)
                {
                    throw new ArgumentNullException(
                        "ticket",
                        "An accepted digital-output submission requires its ticket.");
                }

                if (phase
                    != LMCDigitalOutputWriteSubmissionPhase
                        .PostSubmissionValidation)
                {
                    throw new ArgumentException(
                        "An accepted digital-output ticket failure must occur during post-submission validation.",
                        "phase");
                }

                if (ticket.OperationKind
                        != LMCOperationKind.DigitalOutputWrite
                    || ticket.DiagnosticsBootId != diagnosticsBootId
                    || ticket.SubmissionTopologyRevision
                        != topologyRevision)
                {
                    throw new ArgumentException(
                        "The accepted digital-output ticket does not match the submission identity.",
                        "ticket");
                }
            }
            else if (ticket != null)
            {
                throw new ArgumentException(
                    "Only an accepted digital-output submission can have a ticket.",
                    "ticket");
            }

            if ((submissionOutcome
                        == LMCDigitalOutputWriteSubmissionOutcome.Rejected
                    || submissionOutcome
                        == LMCDigitalOutputWriteSubmissionOutcome
                            .OutcomeUncertain)
                && phase
                    != LMCDigitalOutputWriteSubmissionPhase.Submission)
            {
                throw new ArgumentException(
                    "Rejected and outcome-uncertain digital-output submissions require the Submission phase.",
                    "phase");
            }

            if (submissionOutcome
                    != LMCDigitalOutputWriteSubmissionOutcome.NotAttempted
                && (diagnosticsBootId == 0 || topologyRevision == 0))
            {
                throw new ArgumentException(
                    "A dispatched digital-output submission requires its BootId and TopologyRevision.");
            }

            if (request != null
                && topologyRevision != 0
                && request.TopologyRevision != topologyRevision)
            {
                throw new ArgumentException(
                    "The tracked TopologyRevision does not match the digital-output request.",
                    "topologyRevision");
            }

            Request = request;
            Phase = phase;
            SubmissionOutcome = submissionOutcome;
            DiagnosticsBootId = diagnosticsBootId;
            TopologyRevision = topologyRevision;
            Ticket = ticket;
        }

        /// <summary>
        /// Gets the submitted request, or null when a null argument failed
        /// during RequestValidation.
        /// </summary>
        public LMCDigitalOutputWriteRequest Request { get; private set; }
        public LMCDigitalOutputWriteSubmissionPhase Phase { get; private set; }
        public LMCDigitalOutputWriteSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }
        public uint DiagnosticsBootId { get; private set; }
        public uint TopologyRevision { get; private set; }
        public LMCOperationTicket Ticket { get; private set; }

        public static bool TryGet(
            Exception exception,
            out LMCDigitalOutputWriteSubmissionFailureContext context)
        {
            if (exception == null)
            {
                context = null;
                return false;
            }

            return FailureContexts.TryGetValue(exception, out context);
        }

        internal static void Attach(
            Exception exception,
            LMCDigitalOutputWriteSubmissionFailureContext context)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            lock (FailureContextSync)
            {
                FailureContexts.Remove(exception);
                FailureContexts.Add(exception, context);
            }
        }
    }

    internal sealed class LMCDigitalOutputWriteSubmissionAttemptTracker
    {
        private readonly object sync = new object();
        private readonly LMCDigitalOutputWriteRequest request;
        private LMCDigitalOutputWriteSubmissionPhase phase =
            LMCDigitalOutputWriteSubmissionPhase.RequestValidation;
        private LMCDigitalOutputWriteSubmissionOutcome submissionOutcome =
            LMCDigitalOutputWriteSubmissionOutcome.NotAttempted;
        private uint diagnosticsBootId;
        private uint topologyRevision;
        private LMCOperationTicket ticket;

        internal LMCDigitalOutputWriteSubmissionAttemptTracker(
            LMCDigitalOutputWriteRequest request)
        {
            this.request = request;
        }

        internal void BeginSessionPreflight()
        {
            lock (sync)
            {
                RequireState(
                    LMCDigitalOutputWriteSubmissionPhase.RequestValidation,
                    LMCDigitalOutputWriteSubmissionOutcome.NotAttempted);
                phase =
                    LMCDigitalOutputWriteSubmissionPhase.SessionPreflight;
            }
        }

        internal void BeginCapabilityPreflight()
        {
            lock (sync)
            {
                RequireState(
                    LMCDigitalOutputWriteSubmissionPhase.SessionPreflight,
                    LMCDigitalOutputWriteSubmissionOutcome.NotAttempted);
                phase =
                    LMCDigitalOutputWriteSubmissionPhase.CapabilityPreflight;
            }
        }

        internal void RecordCapabilityIdentity(uint actualDiagnosticsBootId)
        {
            lock (sync)
            {
                RequireState(
                    LMCDigitalOutputWriteSubmissionPhase.CapabilityPreflight,
                    LMCDigitalOutputWriteSubmissionOutcome.NotAttempted);
                diagnosticsBootId = actualDiagnosticsBootId;
                topologyRevision = request.TopologyRevision;
            }
        }

        internal void BeginSubmission()
        {
            lock (sync)
            {
                RequireState(
                    LMCDigitalOutputWriteSubmissionPhase.CapabilityPreflight,
                    LMCDigitalOutputWriteSubmissionOutcome.NotAttempted);
                if (diagnosticsBootId == 0 || topologyRevision == 0)
                {
                    throw new InvalidOperationException(
                        "Digital-output submission requires a validated identity.");
                }

                phase = LMCDigitalOutputWriteSubmissionPhase.Submission;
            }
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            lock (sync)
            {
                RequireState(
                    LMCDigitalOutputWriteSubmissionPhase.Submission,
                    LMCDigitalOutputWriteSubmissionOutcome.NotAttempted);
                submissionOutcome =
                    LMCDigitalOutputWriteSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void MarkSubmissionRejected()
        {
            lock (sync)
            {
                RequireState(
                    LMCDigitalOutputWriteSubmissionPhase.Submission,
                    LMCDigitalOutputWriteSubmissionOutcome.OutcomeUncertain);
                submissionOutcome =
                    LMCDigitalOutputWriteSubmissionOutcome.Rejected;
            }
        }

        internal void MarkSubmissionAccepted(
            LMCOperationTicket acceptedTicket)
        {
            if (acceptedTicket == null)
            {
                throw new ArgumentNullException("acceptedTicket");
            }

            lock (sync)
            {
                RequireState(
                    LMCDigitalOutputWriteSubmissionPhase.Submission,
                    LMCDigitalOutputWriteSubmissionOutcome.OutcomeUncertain);
                if (acceptedTicket.OperationKind
                        != LMCOperationKind.DigitalOutputWrite
                    || acceptedTicket.DiagnosticsBootId
                        != diagnosticsBootId
                    || acceptedTicket.SubmissionTopologyRevision
                        != topologyRevision)
                {
                    throw new ArgumentException(
                        "The accepted ticket does not match the tracked digital-output submission.",
                        "acceptedTicket");
                }

                ticket = acceptedTicket;
                submissionOutcome =
                    LMCDigitalOutputWriteSubmissionOutcome.Accepted;
                phase = LMCDigitalOutputWriteSubmissionPhase
                    .PostSubmissionValidation;
            }
        }

        internal LMCDigitalOutputWriteSubmissionFailureContext
            CreateFailureContext()
        {
            lock (sync)
            {
                return new LMCDigitalOutputWriteSubmissionFailureContext(
                    request,
                    phase,
                    submissionOutcome,
                    diagnosticsBootId,
                    topologyRevision,
                    ticket);
            }
        }

        private void RequireState(
            LMCDigitalOutputWriteSubmissionPhase expectedPhase,
            LMCDigitalOutputWriteSubmissionOutcome expectedOutcome)
        {
            if (phase != expectedPhase
                || submissionOutcome != expectedOutcome)
            {
                throw new InvalidOperationException(
                    "The digital-output submission attempt state transition is invalid.");
            }
        }
    }

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
        private readonly Dictionary<uint, LMCEtherCATTopologyEntry> entriesByIOReference;
        private LMCDiagnostics owner;

        internal LMCEtherCATTopology(
            LMCEtherCATTopologyInfo info,
            IList<LMCEtherCATTopologyEntry> entries)
        {
            Info = info ?? throw new ArgumentNullException("info");
            this.entries = new ReadOnlyCollection<LMCEtherCATTopologyEntry>(
                new List<LMCEtherCATTopologyEntry>(entries));
            entriesByNodeId = new Dictionary<uint, LMCEtherCATTopologyEntry>();
            entriesByIOReference =
                new Dictionary<uint, LMCEtherCATTopologyEntry>();

            foreach (var entry in this.entries)
            {
                if (entriesByNodeId.ContainsKey(entry.NodeId))
                {
                    throw new System.IO.InvalidDataException(
                        "EtherCAT topology contains duplicate NodeId values.");
                }

                entriesByNodeId.Add(entry.NodeId, entry);
                if (entry.IOReference != 0)
                {
                    if (entriesByIOReference.ContainsKey(entry.IOReference))
                    {
                        throw new System.IO.InvalidDataException(
                            "EtherCAT topology contains duplicate IOReference values.");
                    }

                    entriesByIOReference.Add(entry.IOReference, entry);
                }
            }
        }

        internal LMCEtherCATTopology BindProvenance(
            LMCDiagnostics diagnosticsOwner,
            long connectionSessionGeneration)
        {
            if (diagnosticsOwner == null)
            {
                throw new ArgumentNullException("diagnosticsOwner");
            }

            if (connectionSessionGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "connectionSessionGeneration");
            }

            if (owner != null
                && (!ReferenceEquals(owner, diagnosticsOwner)
                    || ConnectionSessionGeneration
                        != connectionSessionGeneration))
            {
                throw new InvalidOperationException(
                    "The EtherCAT topology is already bound to a different diagnostics owner or session.");
            }

            owner = diagnosticsOwner;
            ConnectionSessionGeneration = connectionSessionGeneration;
            return this;
        }

        internal bool IsBoundTo(
            LMCDiagnostics diagnosticsOwner,
            long connectionSessionGeneration)
        {
            return diagnosticsOwner != null
                && ReferenceEquals(owner, diagnosticsOwner)
                && ConnectionSessionGeneration == connectionSessionGeneration;
        }

        public LMCEtherCATTopologyInfo Info { get; private set; }
        public uint TopologyRevision { get { return Info.TopologyRevision; } }
        public IReadOnlyList<LMCEtherCATTopologyEntry> Entries { get { return entries; } }

        internal long ConnectionSessionGeneration { get; private set; }

        public bool BelongsTo(LMCConnection connection)
        {
            return connection != null
                && ReferenceEquals(owner, connection.Diagnostics);
        }

        public bool BelongsToCurrentSession(LMCConnection connection)
        {
            return BelongsTo(connection)
                && connection.IsConnected
                && ConnectionSessionGeneration == connection.SessionGeneration;
        }

        public bool TryGetNode(
            uint nodeId,
            out LMCEtherCATTopologyEntry entry)
        {
            return entriesByNodeId.TryGetValue(nodeId, out entry);
        }

        public bool TryGetIOReference(
            uint ioReference,
            out LMCEtherCATTopologyEntry entry)
        {
            return entriesByIOReference.TryGetValue(ioReference, out entry);
        }

        /// <summary>
        /// Validates that a node-health response belongs to this topology and
        /// that its DS402 payload-presence semantics match the node kind.
        /// Returns the matched immutable topology entry.
        /// </summary>
        public LMCEtherCATTopologyEntry ValidateNodeHealth(
            LMCEtherCATNodeHealth health)
        {
            if (health == null)
            {
                throw new ArgumentNullException("health");
            }

            LMCEtherCATTopologyEntry entry;
            if (health.TopologyRevision != TopologyRevision
                || !entriesByNodeId.TryGetValue(health.NodeId, out entry))
            {
                throw new System.IO.InvalidDataException(
                    "EtherCAT node health does not belong to this topology.");
            }

            var isDs402Drive = (entry.NodeFlags
                & LMCEtherCATTopologyNodeFlags.Ds402Drive) != 0;
            var isDataValid = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.DataValid) != 0;
            var hasDs402Data = (health.HealthFlags
                & LMCEtherCATNodeHealthFlags.Ds402DataPresent) != 0;
            if (hasDs402Data != (isDs402Drive && isDataValid))
            {
                throw new System.IO.InvalidDataException(
                    "EtherCAT node-health DS402 data presence does not match its topology entry and data-valid state.");
            }

            return entry;
        }

        /// <summary>
        /// Validates that a digital-I/O request targets one entry in this
        /// topology with the exact advertised direction and bit width.
        /// Returns the matched immutable topology entry.
        /// </summary>
        public LMCEtherCATTopologyEntry ValidateDigitalIOReadRequest(
            LMCDigitalIOReadRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            LMCEtherCATTopologyEntry entry;
            if (request.TopologyRevision != TopologyRevision
                || !entriesByIOReference.TryGetValue(
                    request.IOReference,
                    out entry))
            {
                throw new System.IO.InvalidDataException(
                    "Digital-I/O request does not belong to this topology.");
            }

            var expectedBitWidth = GetDigitalIOBitWidth(
                entry,
                request.ExpectedDirection);
            if (request.ExpectedBitWidth != expectedBitWidth)
            {
                throw new System.IO.InvalidDataException(
                    "Digital-I/O request width does not match its topology entry and direction.");
            }

            return entry;
        }

        /// <summary>
        /// Validates that a digital-I/O response belongs to the unique
        /// IOReference entry in this topology and matches its node, direction,
        /// and advertised width. Returns the matched immutable topology entry.
        /// </summary>
        public LMCEtherCATTopologyEntry ValidateDigitalIOValue(
            LMCDigitalIOValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            LMCEtherCATTopologyEntry entry;
            if (value.TopologyRevision != TopologyRevision
                || !entriesByIOReference.TryGetValue(
                    value.IOReference,
                    out entry)
                || value.NodeId != entry.NodeId)
            {
                throw new System.IO.InvalidDataException(
                    "Digital-I/O response does not belong to this topology entry.");
            }

            var expectedBitWidth = GetDigitalIOBitWidth(
                entry,
                value.Direction);
            if (value.BitWidth != expectedBitWidth)
            {
                throw new System.IO.InvalidDataException(
                    "Digital-I/O response width does not match its topology entry and direction.");
            }

            return entry;
        }

        private static byte GetDigitalIOBitWidth(
            LMCEtherCATTopologyEntry entry,
            LMCDigitalIODirection direction)
        {
            ushort byteWidth;
            if (direction == LMCDigitalIODirection.Input)
            {
                byteWidth = entry.InputBytes;
            }
            else if (direction == LMCDigitalIODirection.Output)
            {
                byteWidth = entry.OutputBytes;
            }
            else
            {
                throw new System.IO.InvalidDataException(
                    "Digital-I/O direction is not valid for topology binding.");
            }

            if (byteWidth == 0 || byteWidth > sizeof(ulong))
            {
                throw new System.IO.InvalidDataException(
                    "Digital-I/O direction is not available on this topology entry.");
            }

            return checked((byte)(byteWidth * 8));
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
        private LMCDiagnostics owner;

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

        internal LMCDigitalIOValue BindTo(
            LMCDiagnostics diagnosticsOwner,
            long connectionSessionGeneration,
            LMCDiagnosticCapabilities capabilities)
        {
            if (diagnosticsOwner == null)
            {
                throw new ArgumentNullException("diagnosticsOwner");
            }

            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (connectionSessionGeneration <= 0
                || capabilities.ConnectionSessionGeneration
                    != connectionSessionGeneration)
            {
                throw new ArgumentException(
                    "Digital I/O provenance requires the capability snapshot from the same active session.",
                    "connectionSessionGeneration");
            }

            var bound = new LMCDigitalIOValue(
                Response,
                TopologyRevision,
                IOReference,
                NodeId,
                Direction,
                BitWidth,
                StatusFlags,
                Value,
                ValidMask,
                CycleCounter,
                OutputRevision);
            bound.owner = diagnosticsOwner;
            bound.ConnectionSessionGeneration =
                connectionSessionGeneration;
            bound.DiagnosticsBootId = capabilities.DiagnosticsBootId;
            bound.SourceCapabilities = capabilities.Capabilities;
            return bound;
        }

        internal LMCDigitalIOValue BindToTopology(
            LMCEtherCATTopology topology)
        {
            if (topology == null)
            {
                throw new ArgumentNullException("topology");
            }

            if (!topology.IsBoundTo(owner, ConnectionSessionGeneration))
            {
                throw new InvalidOperationException(
                    "The EtherCAT topology and digital-I/O value do not belong to the same active diagnostics session.");
            }

            topology.ValidateDigitalIOValue(this);
            HasValidatedTopologyBinding = true;
            return this;
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
        public uint DiagnosticsBootId { get; private set; }
        public LMCDiagnosticCapability SourceCapabilities { get; private set; }
        public bool HasValidatedTopologyBinding { get; private set; }

        internal long ConnectionSessionGeneration { get; private set; }

        public bool IsValid
        {
            get { return (StatusFlags & LMCDigitalIOStatusFlags.Valid) != 0; }
        }

        public bool BelongsTo(LMCConnection connection)
        {
            return connection != null
                && ReferenceEquals(owner, connection.Diagnostics);
        }

        public bool BelongsToCurrentSession(LMCConnection connection)
        {
            return BelongsTo(connection)
                && connection.IsConnected
                && ConnectionSessionGeneration
                    == connection.SessionGeneration;
        }
    }

    public sealed class LMCDigitalOutputWriteRequest
    {
        private LMCDigitalIOValue sourceSnapshot;

        /// <summary>
        /// Creates a detached wire request. SubmitDigitalOutputWrite rejects
        /// detached requests; use LMCDiagnostics.CreateDigitalOutputWriteRequest
        /// with a current output snapshot for an executable request.
        /// </summary>
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

        internal static LMCDigitalOutputWriteRequest FromOutputSnapshot(
            LMCDigitalIOValue outputSnapshot,
            ulong value,
            ulong mask)
        {
            if (outputSnapshot == null)
            {
                throw new ArgumentNullException("outputSnapshot");
            }

            if (outputSnapshot.Direction != LMCDigitalIODirection.Output
                || !outputSnapshot.IsValid
                || outputSnapshot.OutputRevision == 0
                || !outputSnapshot.HasValidatedTopologyBinding)
            {
                throw new ArgumentException(
                    "A digital-output write requires a topology-bound valid output snapshot with a non-zero revision.",
                    "outputSnapshot");
            }

            if ((mask & ~outputSnapshot.ValidMask) != 0)
            {
                throw new ArgumentException(
                    "Digital output Mask contains bits outside the source snapshot ValidMask.",
                    "mask");
            }

            var request = new LMCDigitalOutputWriteRequest(
                outputSnapshot.TopologyRevision,
                outputSnapshot.IOReference,
                value,
                mask,
                outputSnapshot.OutputRevision);
            request.sourceSnapshot = outputSnapshot;
            return request;
        }

        public uint TopologyRevision { get; private set; }
        public uint IOReference { get; private set; }
        public ulong Value { get; private set; }
        public ulong Mask { get; private set; }
        public uint ExpectedOutputRevision { get; private set; }

        public bool IsSnapshotBound
        {
            get { return sourceSnapshot != null; }
        }

        public uint SourceDiagnosticsBootId
        {
            get
            {
                return sourceSnapshot == null
                    ? 0
                    : sourceSnapshot.DiagnosticsBootId;
            }
        }

        public LMCDiagnosticCapability SourceCapabilities
        {
            get
            {
                return sourceSnapshot == null
                    ? LMCDiagnosticCapability.None
                    : sourceSnapshot.SourceCapabilities;
            }
        }

        public ulong SourceValidMask
        {
            get
            {
                return sourceSnapshot == null
                    ? 0
                    : sourceSnapshot.ValidMask;
            }
        }

        public bool BelongsTo(LMCConnection connection)
        {
            return sourceSnapshot != null
                && sourceSnapshot.BelongsTo(connection);
        }

        public bool BelongsToCurrentSession(LMCConnection connection)
        {
            return sourceSnapshot != null
                && sourceSnapshot.BelongsToCurrentSession(connection);
        }

        internal LMCDigitalIOValue SourceSnapshot
        {
            get { return sourceSnapshot; }
        }
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCRecorderAcceptedOperation
    {
        ConfigureRecorder = 0,
        ConfigureRecoverableDoubleRecorder = 1,
        StartRecorder = 2,
        AdoptRecorder = 3,
        AdoptActiveRecorder = 4,
        AdoptEmptyRecorderConfiguration = 5
    }

    public enum LMCRecorderAcceptedResultKind
    {
        ConfigurationHandle = 0,
        Identity = 1,
        RecoveredConfigurationLease = 2
    }

    /// <summary>
    /// Preserves an exact Recorder resource returned by the PLC when a newer
    /// priority send prevents that resource from being published normally.
    /// The original exception object and type are preserved. The accepted
    /// resource is recovery-only and must be cleaned up in the same session or
    /// reconciled from exact Recorder inventory after reconnect.
    /// </summary>
    public sealed class LMCRecorderAcceptedResultFailureContext
    {
        private static readonly object FailureContextSync = new object();
        private static readonly ConditionalWeakTable<
            Exception,
            LMCRecorderAcceptedResultFailureContext> FailureContexts =
                new ConditionalWeakTable<
                    Exception,
                    LMCRecorderAcceptedResultFailureContext>();

        internal LMCRecorderAcceptedResultFailureContext(
            LMCRecorderAcceptedOperation operation,
            ushort command,
            LMCRecorderConfigurationHandle configurationHandle,
            LMCRecorderIdentity identity,
            LMCRecoveredRecorderConfigurationLease recoveredConfigurationLease,
            LMCRecorderConfigurationHandle sourceConfigurationHandle)
        {
            Operation = operation;
            Command = command;
            ConfigurationHandle = configurationHandle;
            Identity = identity;
            RecoveredConfigurationLease = recoveredConfigurationLease;
            SourceConfigurationHandle = sourceConfigurationHandle;

            var resultCount = (configurationHandle == null ? 0 : 1)
                + (identity == null ? 0 : 1)
                + (recoveredConfigurationLease == null ? 0 : 1);
            if (resultCount != 1)
            {
                throw new ArgumentException(
                    "An accepted Recorder failure requires exactly one result resource.");
            }

            if (configurationHandle != null)
            {
                ResultKind = LMCRecorderAcceptedResultKind.ConfigurationHandle;
                AcceptedResult = configurationHandle;
                DiagnosticsBootId = configurationHandle.DiagnosticsBootId;
                MapRevision = configurationHandle.MapRevision;
                ConfigId = configurationHandle.ConfigId;
                ConfigRevision = configurationHandle.ConfigRevision;
                OwnerSessionEpoch = configurationHandle.OwnerSessionEpoch;
                RecoveryToken = configurationHandle.RecoveryToken;
            }
            else if (identity != null)
            {
                ResultKind = LMCRecorderAcceptedResultKind.Identity;
                AcceptedResult = identity;
                DiagnosticsBootId = identity.DiagnosticsBootId;
                MapRevision = identity.MapRevision;
                ConfigId = identity.ConfigId;
                ConfigRevision = identity.ConfigRevision;
                RecordId = identity.RecordId;
                BufferId = identity.BufferId;
                OwnerSessionEpoch = identity.OwnerSessionEpoch;
                RecoveryToken = sourceConfigurationHandle == null
                    ? Guid.Empty
                    : sourceConfigurationHandle.RecoveryToken;
            }
            else
            {
                ResultKind = LMCRecorderAcceptedResultKind
                    .RecoveredConfigurationLease;
                AcceptedResult = recoveredConfigurationLease;
                DiagnosticsBootId =
                    recoveredConfigurationLease.DiagnosticsBootId;
                MapRevision = recoveredConfigurationLease.MapRevision;
                ConfigId = recoveredConfigurationLease.ConfigId;
                ConfigRevision = recoveredConfigurationLease.ConfigRevision;
                OwnerSessionEpoch =
                    recoveredConfigurationLease.OwnerSessionEpoch;
                PreviousOwnerSessionEpoch =
                    recoveredConfigurationLease.PreviousOwnerSessionEpoch;
                RecoveryToken = Guid.Empty;
            }
        }

        public LMCRecorderAcceptedOperation Operation { get; private set; }
        public ushort Command { get; private set; }
        public LMCRecorderAcceptedResultKind ResultKind { get; private set; }
        public object AcceptedResult { get; private set; }
        public LMCRecorderConfigurationHandle ConfigurationHandle
        {
            get;
            private set;
        }
        public LMCRecorderIdentity Identity { get; private set; }
        public LMCRecoveredRecorderConfigurationLease
            RecoveredConfigurationLease { get; private set; }
        public LMCRecorderConfigurationHandle SourceConfigurationHandle
        {
            get;
            private set;
        }
        public uint DiagnosticsBootId { get; private set; }
        public uint MapRevision { get; private set; }
        public uint ConfigId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint RecordId { get; private set; }
        public uint BufferId { get; private set; }
        public uint OwnerSessionEpoch { get; private set; }
        public uint PreviousOwnerSessionEpoch { get; private set; }
        public Guid RecoveryToken { get; private set; }
        public bool IsAcceptedResultRecoveryOnly { get { return true; } }

        public static bool TryGet(
            Exception exception,
            out LMCRecorderAcceptedResultFailureContext context)
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
            LMCRecorderAcceptedResultFailureContext context)
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

    public enum LMCRecorderState : ushort
    {
        Empty = 0,
        Configured = 1,
        Armed = 2,
        Recording = 3,
        Ready = 4,
        Uploading = 5,
        Fault = 6
    }

    public enum LMCRecorderStopReason : byte
    {
        None = 0,
        SampleCountComplete = 1,
        UserStop = 2,
        TriggerComplete = 3,
        Capacity = 4,
        Error = 5
    }

    public enum LMCRecorderBufferMode : byte
    {
        Single = 0,
        Ring = 1,
        Double = 2
    }

    public enum LMCRecorderTriggerType : byte
    {
        Manual = 0,
        Edge = 1,
        Window = 2,
        Mask = 3
    }

    public enum LMCRecorderTriggerOperator : byte
    {
        None = 0,
        RisingEdge = 1,
        FallingEdge = 2,
        EnterWindow = 3,
        ExitWindow = 4,
        MaskAllSet = 5,
        MaskAnySet = 6,
        MaskAllClear = 7
    }

    public enum LMCRecorderDataEncoding : byte
    {
        SampleMajorRaw32LittleEndian = 1
    }

    public enum LMCRecorderDataCrcPolicy : byte
    {
        None = 0,
        Crc32IsoHdlc = 1
    }

    public sealed class LMCRecorderBankInventoryEntry
    {
        internal LMCRecorderBankInventoryEntry(
            uint recordId,
            uint bufferId,
            uint ownerSessionEpoch,
            uint closedSessionEpoch,
            LMCRecorderState state)
        {
            RecordId = recordId;
            BufferId = bufferId;
            OwnerSessionEpoch = ownerSessionEpoch;
            ClosedSessionEpoch = closedSessionEpoch;
            State = state;
        }

        public uint RecordId { get; private set; }
        public uint BufferId { get; private set; }
        public uint OwnerSessionEpoch { get; private set; }
        public uint ClosedSessionEpoch { get; private set; }
        public LMCRecorderState State { get; private set; }
        public bool IsOwnerSessionClosed
        {
            get { return ClosedSessionEpoch == OwnerSessionEpoch; }
        }
    }

    public sealed class LMCRecorderBankInventory
    {
        private readonly ReadOnlyCollection<LMCRecorderBankInventoryEntry>
            occupiedBanks;

        internal LMCRecorderBankInventory(
            LMCDiagnosticsResponse response,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint configurationOwnerSessionEpoch,
            uint configurationClosedSessionEpoch,
            LMCRecorderState configurationState,
            LMCRecorderBufferMode bufferMode,
            byte recorderBufferCount,
            IList<LMCRecorderBankInventoryEntry> occupiedBanks)
            : this(
                response,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                configurationOwnerSessionEpoch,
                configurationClosedSessionEpoch,
                configurationState,
                bufferMode,
                recorderBufferCount,
                occupiedBanks,
                Guid.Empty)
        {
        }

        internal LMCRecorderBankInventory(
            LMCDiagnosticsResponse response,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint configurationOwnerSessionEpoch,
            uint configurationClosedSessionEpoch,
            LMCRecorderState configurationState,
            LMCRecorderBufferMode bufferMode,
            byte recorderBufferCount,
            IList<LMCRecorderBankInventoryEntry> occupiedBanks,
            Guid recoveryToken)
        {
            Response = response;
            DiagnosticsBootId = diagnosticsBootId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            ConfigurationOwnerSessionEpoch = configurationOwnerSessionEpoch;
            ConfigurationClosedSessionEpoch = configurationClosedSessionEpoch;
            ConfigurationState = configurationState;
            BufferMode = bufferMode;
            RecorderBufferCount = recorderBufferCount;
            RecoveryToken = recoveryToken;
            this.occupiedBanks = new ReadOnlyCollection<
                LMCRecorderBankInventoryEntry>(occupiedBanks);
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint ConfigId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public uint ConfigurationOwnerSessionEpoch { get; private set; }
        public uint ConfigurationClosedSessionEpoch { get; private set; }
        public LMCRecorderState ConfigurationState { get; private set; }
        public LMCRecorderBufferMode BufferMode { get; private set; }
        public byte RecorderBufferCount { get; private set; }
        public Guid RecoveryToken { get; private set; }
        public bool IsRecoverable
        {
            get { return RecoveryToken != Guid.Empty; }
        }
        public IReadOnlyList<LMCRecorderBankInventoryEntry> OccupiedBanks
        {
            get { return occupiedBanks; }
        }

        public bool IsConfigurationOwnerSessionClosed
        {
            get
            {
                return ConfigurationClosedSessionEpoch
                    == ConfigurationOwnerSessionEpoch;
            }
        }
    }

    public sealed class LMCRecoveredRecorderConfigurationLease
    {
        private const int ReleaseStateUsable = 0;
        private const int ReleaseStateInProgress = 1;
        private const int ReleaseStateReleased = 2;
        private const int ReleaseStateOutcomeUnverified = 3;

        private int releaseState;
        private int acceptedResultRecoveryOnly;

        internal LMCRecoveredRecorderConfigurationLease(
            LMCDiagnosticsResponse response,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint previousOwnerSessionEpoch,
            uint ownerSessionEpoch,
            LMCRecorderState state,
            LMCRecorderBufferMode bufferMode,
            byte recorderBufferCount,
            long connectionSessionGeneration,
            LMCDiagnostics owner)
        {
            AdoptionResponse = response;
            DiagnosticsBootId = diagnosticsBootId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            PreviousOwnerSessionEpoch = previousOwnerSessionEpoch;
            OwnerSessionEpoch = ownerSessionEpoch;
            InitialState = state;
            BufferMode = bufferMode;
            RecorderBufferCount = recorderBufferCount;
            ConnectionSessionGeneration = connectionSessionGeneration;
            Owner = owner;
        }

        public LMCDiagnosticsResponse AdoptionResponse { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint ConfigId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public uint PreviousOwnerSessionEpoch { get; private set; }
        public uint OwnerSessionEpoch { get; private set; }
        public LMCRecorderState InitialState { get; private set; }
        public LMCRecorderBufferMode BufferMode { get; private set; }
        public byte RecorderBufferCount { get; private set; }
        public bool IsReleased
        {
            get { return Volatile.Read(ref releaseState) == ReleaseStateReleased; }
        }
        public bool IsReleaseOutcomeUnverified
        {
            get
            {
                return Volatile.Read(ref releaseState)
                    == ReleaseStateOutcomeUnverified;
            }
        }
        public bool IsAcceptedResultRecoveryOnly
        {
            get { return Volatile.Read(ref acceptedResultRecoveryOnly) != 0; }
        }

        internal long ConnectionSessionGeneration { get; private set; }
        internal LMCDiagnostics Owner { get; private set; }

        internal void EnsureUsable()
        {
            EnsureUsableForRecovery();
            if (IsAcceptedResultRecoveryOnly)
            {
                throw new InvalidOperationException(
                    "The accepted recovered Recorder configuration is recovery-only and can only be released.");
            }
        }

        internal void EnsureUsableForRecovery()
        {
            var state = Volatile.Read(ref releaseState);
            if (state == ReleaseStateReleased)
            {
                throw new InvalidOperationException(
                    "The recovered Recorder configuration has already been released.");
            }

            if (state == ReleaseStateInProgress)
            {
                throw new InvalidOperationException(
                    "The recovered Recorder configuration is currently being released.");
            }

            if (state == ReleaseStateOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "The recovered Recorder configuration release outcome is unverified. Reconnect and reconcile exact inventory before any retry.");
            }
        }

        internal void MarkAcceptedResultRecoveryOnly()
        {
            Volatile.Write(ref acceptedResultRecoveryOnly, 1);
        }

        internal void BeginRelease()
        {
            var prior = Interlocked.CompareExchange(
                ref releaseState,
                ReleaseStateInProgress,
                ReleaseStateUsable);
            if (prior != ReleaseStateUsable)
            {
                throw new InvalidOperationException(
                    prior == ReleaseStateReleased
                        ? "The recovered Recorder configuration has already been released."
                        : prior == ReleaseStateOutcomeUnverified
                            ? "The recovered Recorder configuration release outcome is unverified."
                            : "The recovered Recorder configuration is currently being released.");
            }
        }

        internal void CompleteRelease()
        {
            Volatile.Write(ref releaseState, ReleaseStateReleased);
        }

        internal void CancelRelease()
        {
            Interlocked.CompareExchange(
                ref releaseState,
                ReleaseStateUsable,
                ReleaseStateInProgress);
        }

        internal void MarkReleaseOutcomeUnverified()
        {
            Volatile.Write(
                ref releaseState,
                ReleaseStateOutcomeUnverified);
        }
    }

    [Flags]
    public enum LMCRecorderHeaderFlags : ushort
    {
        None = 0,
        CaptureComplete = 1 << 0,
        TriggerPresent = 1 << 1,
        UserStopped = 1 << 2,
        DataCrcPresent = 1 << 3
    }

    public sealed class LMCRecorderConfiguration
    {
        private readonly ReadOnlyCollection<uint> signalIds;

        public LMCRecorderConfiguration(
            IReadOnlyList<uint> signalIds,
            ushort samplePeriodCycles,
            uint sampleCapacity)
            : this(
                signalIds,
                samplePeriodCycles,
                sampleCapacity,
                LMCRecorderBufferMode.Single,
                LMCRecorderTriggerType.Manual,
                LMCSignalValueType.Invalid,
                0,
                0,
                0,
                LMCRecorderTriggerOperator.None,
                0,
                0,
                0)
        {
        }

        public LMCRecorderConfiguration(
            IReadOnlyList<uint> signalIds,
            ushort samplePeriodCycles,
            uint sampleCapacity,
            LMCRecorderBufferMode bufferMode,
            LMCRecorderTriggerType triggerType,
            LMCSignalValueType triggerValueType,
            uint preTriggerSamples,
            uint postTriggerSamples,
            uint triggerSignalId,
            LMCRecorderTriggerOperator triggerOperator,
            uint triggerValue,
            uint triggerMask,
            uint requestedConfigId = 0)
        {
            if (signalIds == null)
            {
                throw new ArgumentNullException("signalIds");
            }

            if (signalIds.Count == 0
                || signalIds.Count > LMC_DiagnosticsFrame.MaxRecorderChannelCount)
            {
                throw new ArgumentOutOfRangeException(
                    "signalIds",
                    "Recorder configurations require between 1 and 32 signals.");
            }

            if (samplePeriodCycles == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "samplePeriodCycles",
                    "SamplePeriodCycles must be at least one.");
            }

            if (sampleCapacity == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "sampleCapacity",
                    "SampleCapacity must be non-zero.");
            }

            if (bufferMode < LMCRecorderBufferMode.Single
                || bufferMode > LMCRecorderBufferMode.Double)
            {
                throw new ArgumentOutOfRangeException("bufferMode");
            }

            if (triggerType < LMCRecorderTriggerType.Manual
                || triggerType > LMCRecorderTriggerType.Mask)
            {
                throw new ArgumentOutOfRangeException("triggerType");
            }

            if (bufferMode == LMCRecorderBufferMode.Ring
                && triggerType == LMCRecorderTriggerType.Manual)
            {
                throw new ArgumentException(
                    "Ring mode requires an edge, window, or mask trigger.",
                    "triggerType");
            }

            if (bufferMode == LMCRecorderBufferMode.Single
                && triggerType != LMCRecorderTriggerType.Manual)
            {
                throw new ArgumentException(
                    "Triggered Recorder configurations require Ring or Double buffer mode.",
                    "bufferMode");
            }

            ValidateTrigger(
                triggerType,
                triggerValueType,
                preTriggerSamples,
                postTriggerSamples,
                triggerSignalId,
                triggerOperator,
                triggerValue,
                triggerMask,
                sampleCapacity);

            var copy = new uint[signalIds.Count];
            var unique = new HashSet<uint>();
            for (var index = 0; index < copy.Length; index++)
            {
                var signalId = signalIds[index];
                if (signalId == 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "signalIds",
                        "Recorder SignalId values must be non-zero.");
                }

                if (!unique.Add(signalId))
                {
                    throw new ArgumentException(
                        "Recorder configurations do not allow duplicate SignalId values.",
                        "signalIds");
                }

                copy[index] = signalId;
            }

            this.signalIds = new ReadOnlyCollection<uint>(copy);
            SamplePeriodCycles = samplePeriodCycles;
            SampleCapacity = sampleCapacity;
            BufferMode = bufferMode;
            TriggerType = triggerType;
            TriggerValueType = triggerValueType;
            PreTriggerSamples = preTriggerSamples;
            PostTriggerSamples = postTriggerSamples;
            TriggerSignalId = triggerSignalId;
            TriggerOperator = triggerOperator;
            TriggerValue = triggerValue;
            TriggerMask = triggerMask;
            RequestedConfigId = requestedConfigId;
        }

        public IReadOnlyList<uint> SignalIds { get { return signalIds; } }
        public ushort ChannelCount { get { return checked((ushort)signalIds.Count); } }
        public ushort SamplePeriodCycles { get; private set; }
        public uint SampleCapacity { get; private set; }
        public LMCRecorderBufferMode BufferMode { get; private set; }
        public LMCRecorderTriggerType TriggerType { get; private set; }
        public LMCSignalValueType TriggerValueType { get; private set; }
        public uint PreTriggerSamples { get; private set; }
        public uint PostTriggerSamples { get; private set; }
        public uint TriggerSignalId { get; private set; }
        public LMCRecorderTriggerOperator TriggerOperator { get; private set; }
        public uint TriggerValue { get; private set; }
        public uint TriggerMask { get; private set; }
        public uint TriggerLowerBound { get { return TriggerValue; } }
        public uint TriggerUpperBound { get { return TriggerMask; } }
        public uint RequestedConfigId { get; private set; }

        public bool RequiresTriggerCapability
        {
            get
            {
                return TriggerType != LMCRecorderTriggerType.Manual
                    || BufferMode == LMCRecorderBufferMode.Ring;
            }
        }

        public bool RequiresDoubleBankCapability
        {
            get { return BufferMode == LMCRecorderBufferMode.Double; }
        }

        private static void ValidateTrigger(
            LMCRecorderTriggerType triggerType,
            LMCSignalValueType triggerValueType,
            uint preTriggerSamples,
            uint postTriggerSamples,
            uint triggerSignalId,
            LMCRecorderTriggerOperator triggerOperator,
            uint triggerValue,
            uint triggerMask,
            uint sampleCapacity)
        {
            if (triggerType == LMCRecorderTriggerType.Manual)
            {
                if (triggerValueType != LMCSignalValueType.Invalid
                    || preTriggerSamples != 0
                    || postTriggerSamples != 0
                    || triggerSignalId != 0
                    || triggerOperator != LMCRecorderTriggerOperator.None
                    || triggerValue != 0
                    || triggerMask != 0)
                {
                    throw new ArgumentException(
                        "Manual Recorder configuration requires all trigger fields to be zero or None.");
                }

                return;
            }

            if (triggerValueType < LMCSignalValueType.Bool
                || triggerValueType > LMCSignalValueType.BitField32)
            {
                throw new ArgumentOutOfRangeException(
                    "triggerValueType",
                    "Triggered Recorder configurations require a supported 32-bit wire value type.");
            }

            if (triggerSignalId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "triggerSignalId",
                    "Triggered Recorder configurations require a non-zero TriggerSignalId.");
            }

            if (preTriggerSamples > sampleCapacity
                || postTriggerSamples > sampleCapacity
                || (ulong)preTriggerSamples
                    + 1
                    + postTriggerSamples > sampleCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    "preTriggerSamples",
                    "PreTriggerSamples, the trigger sample, and PostTriggerSamples must fit in SampleCapacity.");
            }

            switch (triggerType)
            {
                case LMCRecorderTriggerType.Edge:
                    if (triggerOperator != LMCRecorderTriggerOperator.RisingEdge
                        && triggerOperator != LMCRecorderTriggerOperator.FallingEdge)
                    {
                        throw new ArgumentException(
                            "Edge triggers require RisingEdge or FallingEdge.",
                            "triggerOperator");
                    }

                    if (triggerMask != 0)
                    {
                        throw new ArgumentException(
                            "Edge triggers require TriggerMask to be zero.",
                            "triggerMask");
                    }

                    ValidateCanonicalTriggerValue(
                        triggerValueType,
                        triggerValue,
                        "triggerValue");
                    break;

                case LMCRecorderTriggerType.Window:
                    if (triggerOperator != LMCRecorderTriggerOperator.EnterWindow
                        && triggerOperator != LMCRecorderTriggerOperator.ExitWindow)
                    {
                        throw new ArgumentException(
                            "Window triggers require EnterWindow or ExitWindow.",
                            "triggerOperator");
                    }

                    ValidateWindowBounds(
                        triggerValueType,
                        triggerValue,
                        triggerMask);
                    break;

                case LMCRecorderTriggerType.Mask:
                    if (triggerOperator < LMCRecorderTriggerOperator.MaskAllSet
                        || triggerOperator > LMCRecorderTriggerOperator.MaskAllClear)
                    {
                        throw new ArgumentException(
                            "Mask triggers require a mask trigger operator.",
                            "triggerOperator");
                    }

                    if (triggerMask == 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            "triggerMask",
                            "Mask triggers require a non-zero TriggerMask.");
                    }

                    if (triggerValue != 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            "triggerValue",
                            "Mask trigger operators use only TriggerMask; TriggerValue must be zero.");
                    }

                    if (triggerValueType != LMCSignalValueType.BitField16
                        && triggerValueType != LMCSignalValueType.BitField32)
                    {
                        throw new ArgumentException(
                            "Mask triggers require BitField16 or BitField32 values.",
                            "triggerValueType");
                    }

                    ValidateCanonicalTriggerValue(
                        triggerValueType,
                        triggerValue,
                        "triggerValue");
                    ValidateCanonicalTriggerValue(
                        triggerValueType,
                        triggerMask,
                        "triggerMask");
                    break;
            }
        }

        private static void ValidateWindowBounds(
            LMCSignalValueType valueType,
            uint lowerRaw,
            uint upperRaw)
        {
            ValidateCanonicalTriggerValue(valueType, lowerRaw, "triggerValue");
            ValidateCanonicalTriggerValue(valueType, upperRaw, "triggerMask");

            bool ordered;
            switch (valueType)
            {
                case LMCSignalValueType.Int16:
                    ordered = unchecked((short)(ushort)lowerRaw)
                        <= unchecked((short)(ushort)upperRaw);
                    break;

                case LMCSignalValueType.UInt16:
                    ordered = (ushort)lowerRaw <= (ushort)upperRaw;
                    break;

                case LMCSignalValueType.Int32:
                    ordered = unchecked((int)lowerRaw)
                        <= unchecked((int)upperRaw);
                    break;

                case LMCSignalValueType.UInt32:
                    ordered = lowerRaw <= upperRaw;
                    break;

                default:
                    throw new ArgumentException(
                        "Window triggers require Int16, UInt16, Int32, or UInt32 bounds.",
                        "triggerValueType");
            }

            if (!ordered)
            {
                throw new ArgumentOutOfRangeException(
                    "triggerMask",
                    "Window TriggerUpperBound must be greater than or equal to TriggerLowerBound.");
            }
        }

        private static void ValidateCanonicalTriggerValue(
            LMCSignalValueType valueType,
            uint rawValue,
            string parameterName)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                    if (rawValue > 1)
                    {
                        throw new ArgumentOutOfRangeException(
                            parameterName,
                            "Bool trigger values must be canonical zero or one.");
                    }

                    break;

                case LMCSignalValueType.Int16:
                    var canonical = unchecked(
                        (uint)(int)unchecked((short)(ushort)rawValue));
                    if (rawValue != canonical)
                    {
                        throw new ArgumentOutOfRangeException(
                            parameterName,
                            "Int16 trigger values must be sign-extended to 32 bits.");
                    }

                    break;

                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    if ((rawValue & 0xFFFF0000u) != 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            parameterName,
                            "Unsigned 16-bit trigger values must be zero-extended to 32 bits.");
                    }

                    break;
            }
        }
    }

    public sealed class LMCRecorderConfigurationHandle
    {
        private const int ReleaseStateUsable = 0;
        private const int ReleaseStateInProgress = 1;
        private const int ReleaseStateReleased = 2;
        private const int ReleaseStateOutcomeUnverified = 3;

        private readonly ReadOnlyCollection<uint> signalIds;
        private readonly object lifecycleSync = new object();
        private int releaseState;
        private int acceptedResultRecoveryOnly;
        private bool startInProgress;

        internal LMCRecorderConfigurationHandle(
            LMCDiagnosticsResponse response,
            LMCRecorderConfiguration configuration,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint acceptedCapacity,
            uint samplePeriodUs,
            uint reservedDataBytes,
            LMCRecorderState state,
            ushort sampleStrideBytes,
            ushort recorderBufferCount,
            LMCCapturePhase capturePhase,
            uint ownerSessionEpoch,
            ushort maxChunkDataBytes,
            long connectionSessionGeneration,
            LMCDiagnostics owner)
            : this(
                response,
                configuration,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                acceptedCapacity,
                samplePeriodUs,
                reservedDataBytes,
                state,
                sampleStrideBytes,
                recorderBufferCount,
                capturePhase,
                ownerSessionEpoch,
                maxChunkDataBytes,
                connectionSessionGeneration,
                owner,
                Guid.Empty)
        {
        }

        internal LMCRecorderConfigurationHandle(
            LMCDiagnosticsResponse response,
            LMCRecorderConfiguration configuration,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint acceptedCapacity,
            uint samplePeriodUs,
            uint reservedDataBytes,
            LMCRecorderState state,
            ushort sampleStrideBytes,
            ushort recorderBufferCount,
            LMCCapturePhase capturePhase,
            uint ownerSessionEpoch,
            ushort maxChunkDataBytes,
            long connectionSessionGeneration,
            LMCDiagnostics owner,
            Guid recoveryToken)
        {
            ConfigurationResponse = response;
            Configuration = configuration;
            DiagnosticsBootId = diagnosticsBootId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            AcceptedCapacity = acceptedCapacity;
            SamplePeriodUs = samplePeriodUs;
            ReservedDataBytes = reservedDataBytes;
            InitialState = state;
            SampleStrideBytes = sampleStrideBytes;
            RecorderBufferCount = recorderBufferCount;
            CapturePhase = capturePhase;
            OwnerSessionEpoch = ownerSessionEpoch;
            MaxChunkDataBytes = maxChunkDataBytes;
            ConnectionSessionGeneration = connectionSessionGeneration;
            Owner = owner;
            RecoveryToken = recoveryToken;

            var copy = new uint[configuration.SignalIds.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = configuration.SignalIds[index];
            }

            signalIds = new ReadOnlyCollection<uint>(copy);
        }

        public LMCDiagnosticsResponse ConfigurationResponse { get; private set; }
        public LMCRecorderConfiguration Configuration { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint ConfigId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public uint AcceptedCapacity { get; private set; }
        public uint SamplePeriodUs { get; private set; }
        public uint ReservedDataBytes { get; private set; }
        public LMCRecorderState InitialState { get; private set; }
        public ushort ChannelCount { get { return checked((ushort)signalIds.Count); } }
        public ushort SampleStrideBytes { get; private set; }
        public ushort RecorderBufferCount { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public uint OwnerSessionEpoch { get; private set; }
        public IReadOnlyList<uint> SignalIds { get { return signalIds; } }
        public Guid RecoveryToken { get; private set; }
        public bool IsRecoverable
        {
            get { return RecoveryToken != Guid.Empty; }
        }
        public bool IsReleased
        {
            get { return Volatile.Read(ref releaseState) == ReleaseStateReleased; }
        }
        public bool IsReleaseOutcomeUnverified
        {
            get
            {
                return Volatile.Read(ref releaseState)
                    == ReleaseStateOutcomeUnverified;
            }
        }
        public bool IsAcceptedResultRecoveryOnly
        {
            get { return Volatile.Read(ref acceptedResultRecoveryOnly) != 0; }
        }

        internal ushort MaxChunkDataBytes { get; private set; }
        internal long ConnectionSessionGeneration { get; private set; }
        internal LMCDiagnostics Owner { get; private set; }

        internal void EnsureUsable()
        {
            EnsureUsableForRecovery();
            if (IsAcceptedResultRecoveryOnly)
            {
                throw new InvalidOperationException(
                    "The accepted Recorder configuration is recovery-only and can only be released.");
            }
        }

        internal void EnsureUsableForRecovery()
        {
            var state = Volatile.Read(ref releaseState);
            if (state == ReleaseStateReleased)
            {
                throw new InvalidOperationException(
                    "The Recorder configuration has already been released.");
            }

            if (state == ReleaseStateInProgress)
            {
                throw new InvalidOperationException(
                    "The Recorder configuration is currently being released.");
            }

            if (state == ReleaseStateOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "The Recorder configuration release outcome is unverified. Reconnect and reconcile exact inventory before any retry.");
            }
        }

        internal void MarkAcceptedResultRecoveryOnly()
        {
            Volatile.Write(ref acceptedResultRecoveryOnly, 1);
        }

        internal void BeginRelease()
        {
            int prior;
            lock (lifecycleSync)
            {
                if (startInProgress)
                {
                    throw new InvalidOperationException(
                        "The Recorder configuration is currently being started.");
                }

                prior = Interlocked.CompareExchange(
                    ref releaseState,
                    ReleaseStateInProgress,
                    ReleaseStateUsable);
                if (prior != ReleaseStateUsable)
                {
                    throw new InvalidOperationException(
                        prior == ReleaseStateReleased
                            ? "The Recorder configuration has already been released."
                            : prior == ReleaseStateOutcomeUnverified
                                ? "The Recorder configuration release outcome is unverified."
                                : "The Recorder configuration is currently being released.");
                }
            }
        }

        internal void BeginStart()
        {
            lock (lifecycleSync)
            {
                EnsureUsable();
                if (startInProgress)
                {
                    throw new InvalidOperationException(
                        "The Recorder configuration is already being started.");
                }

                startInProgress = true;
            }
        }

        internal void CompleteStart()
        {
            lock (lifecycleSync)
            {
                startInProgress = false;
            }
        }

        internal void CancelStart()
        {
            lock (lifecycleSync)
            {
                startInProgress = false;
            }
        }

        internal void CompleteRelease()
        {
            Volatile.Write(ref releaseState, ReleaseStateReleased);
        }

        internal void CancelRelease()
        {
            Interlocked.CompareExchange(
                ref releaseState,
                ReleaseStateUsable,
                ReleaseStateInProgress);
        }

        internal void MarkReleaseOutcomeUnverified()
        {
            Volatile.Write(
                ref releaseState,
                ReleaseStateOutcomeUnverified);
        }
    }

    public sealed class LMCRecorderIdentity
    {
        private const int BufferStateUsable = 0;
        private const int BufferStateReleasing = 1;
        private const int BufferStateReleased = 2;
        private const int BufferStateOutcomeUnverified = 3;
        private const int RecorderStateUsable = 0;
        private const int RecorderStateReleasing = 1;
        private const int RecorderStateReleased = 2;
        private const int RecorderStateOutcomeUnverified = 3;

        private readonly object metadataSync = new object();
        private ReadOnlyCollection<uint> signalIds;
        private int bufferState;
        private int recorderReleaseState;
        private int acceptedResultRecoveryOnly;
        private bool hasFrozenHeaderMetadata;
        private bool hasAcceptedStartCycleMetadata;
        private LMCRecorderDataCrcPolicy dataCrcPolicy;
        private uint frozenSampleCount;
        private ushort frozenSampleStrideBytes;
        private LMCRecorderHeader frozenHeader;

        internal LMCRecorderIdentity(
            LMCDiagnosticsResponse response,
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint ownerSessionEpoch,
            LMCRecorderState state,
            uint acceptedStartCycle,
            uint capacity,
            LMCCapturePhase capturePhase,
            uint samplePeriodUs,
            LMCRecorderBufferMode bufferMode,
            LMCRecorderTriggerType triggerType,
            uint preTriggerSamples,
            uint postTriggerSamples,
            bool hasConfigurationShape,
            ushort maxChunkDataBytes,
            IReadOnlyList<uint> configuredSignalIds,
            long connectionSessionGeneration,
            LMCDiagnostics owner,
            bool isAdopted)
        {
            Response = response;
            DiagnosticsBootId = diagnosticsBootId;
            RecordId = recordId;
            BufferId = bufferId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            OwnerSessionEpoch = ownerSessionEpoch;
            InitialState = state;
            AcceptedStartCycle = acceptedStartCycle;
            hasAcceptedStartCycleMetadata = acceptedStartCycle != 0;
            Capacity = capacity;
            CapturePhase = capturePhase;
            SamplePeriodUs = samplePeriodUs;
            BufferMode = bufferMode;
            TriggerType = triggerType;
            PreTriggerSamples = preTriggerSamples;
            PostTriggerSamples = postTriggerSamples;
            HasConfigurationShape = hasConfigurationShape;
            MaxChunkDataBytes = maxChunkDataBytes;
            ConnectionSessionGeneration = connectionSessionGeneration;
            Owner = owner;
            IsAdopted = isAdopted;

            var count = configuredSignalIds == null ? 0 : configuredSignalIds.Count;
            var copy = new uint[count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = configuredSignalIds[index];
            }

            signalIds = new ReadOnlyCollection<uint>(copy);
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint RecordId { get; private set; }
        public uint BufferId { get; private set; }
        public uint ConfigId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public uint OwnerSessionEpoch { get; private set; }
        public LMCRecorderState InitialState { get; private set; }
        public uint AcceptedStartCycle { get; private set; }
        public uint Capacity { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public uint SamplePeriodUs { get; private set; }
        public LMCRecorderBufferMode BufferMode { get; private set; }
        public LMCRecorderTriggerType TriggerType { get; private set; }
        public uint PreTriggerSamples { get; private set; }
        public uint PostTriggerSamples { get; private set; }
        public bool HasConfigurationShape { get; private set; }
        public IReadOnlyList<uint> SignalIds { get { return signalIds; } }
        public ushort ChannelCount { get { return checked((ushort)signalIds.Count); } }
        public bool IsBufferReleased
        {
            get { return Volatile.Read(ref bufferState) == BufferStateReleased; }
        }
        public bool IsBufferReleaseOutcomeUnverified
        {
            get
            {
                return Volatile.Read(ref bufferState)
                    == BufferStateOutcomeUnverified;
            }
        }
        public bool IsRecorderReleased
        {
            get
            {
                return Volatile.Read(ref recorderReleaseState)
                    == RecorderStateReleased;
            }
        }
        public bool IsRecorderReleaseOutcomeUnverified
        {
            get
            {
                return Volatile.Read(ref recorderReleaseState)
                    == RecorderStateOutcomeUnverified;
            }
        }
        public bool IsAcceptedResultRecoveryOnly
        {
            get { return Volatile.Read(ref acceptedResultRecoveryOnly) != 0; }
        }
        public bool HasConfigurationMetadata
        {
            get { return ConfigId != 0 && ConfigRevision != 0; }
        }
        public bool HasFrozenHeaderMetadata
        {
            get
            {
                lock (metadataSync)
                {
                    return hasFrozenHeaderMetadata;
                }
            }
        }

        internal ushort MaxChunkDataBytes { get; private set; }
        internal LMCRecorderDataCrcPolicy DataCrcPolicy
        {
            get
            {
                lock (metadataSync)
                {
                    return dataCrcPolicy;
                }
            }
        }

        internal bool HasAcceptedStartCycleMetadata
        {
            get
            {
                lock (metadataSync)
                {
                    return hasAcceptedStartCycleMetadata;
                }
            }
        }
        internal uint FrozenSampleCount
        {
            get
            {
                lock (metadataSync)
                {
                    return frozenSampleCount;
                }
            }
        }
        internal ushort FrozenSampleStrideBytes
        {
            get
            {
                lock (metadataSync)
                {
                    return frozenSampleStrideBytes;
                }
            }
        }
        internal long ConnectionSessionGeneration { get; private set; }
        internal LMCDiagnostics Owner { get; private set; }
        public bool IsAdopted { get; private set; }

        internal void EnsureUsable()
        {
            EnsureUsableForRecovery();
            if (IsAcceptedResultRecoveryOnly)
            {
                throw new InvalidOperationException(
                    "The accepted Recorder identity is recovery-only. Only status, stop, buffer release, and adopted configuration release are allowed.");
            }
        }

        internal void EnsureUsableForRecovery()
        {
            var recorderState = Volatile.Read(ref recorderReleaseState);
            if (recorderState == RecorderStateOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "The Recorder configuration release outcome is unverified. Reconnect and reconcile exact inventory before any retry.");
            }

            if (recorderState != RecorderStateUsable)
            {
                throw new InvalidOperationException(
                    "The Recorder configuration has been released or is being released.");
            }

            var state = Volatile.Read(ref bufferState);
            if (state == BufferStateReleased)
            {
                throw new InvalidOperationException(
                    "The Recorder buffer has already been released.");
            }

            if (state == BufferStateReleasing)
            {
                throw new InvalidOperationException(
                    "The Recorder buffer is currently being released.");
            }

            if (state == BufferStateOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "The Recorder buffer release outcome is unverified. Reconnect and reconcile exact inventory before any retry.");
            }
        }

        internal void MarkAcceptedResultRecoveryOnly()
        {
            Volatile.Write(ref acceptedResultRecoveryOnly, 1);
        }

        internal void BeginBufferRelease()
        {
            var prior = Interlocked.CompareExchange(
                ref bufferState,
                BufferStateReleasing,
                BufferStateUsable);
            if (prior != BufferStateUsable)
            {
                throw new InvalidOperationException(
                    prior == BufferStateReleased
                        ? "The Recorder buffer has already been released."
                        : prior == BufferStateOutcomeUnverified
                            ? "The Recorder buffer release outcome is unverified."
                            : "The Recorder buffer is currently being released.");
            }
        }

        internal void CompleteBufferRelease()
        {
            Volatile.Write(ref bufferState, BufferStateReleased);
        }

        internal void CancelBufferRelease()
        {
            Interlocked.CompareExchange(
                ref bufferState,
                BufferStateUsable,
                BufferStateReleasing);
        }

        internal void MarkBufferReleaseOutcomeUnverified()
        {
            Volatile.Write(
                ref bufferState,
                BufferStateOutcomeUnverified);
        }

        internal void BeginRecorderRelease()
        {
            var prior = Interlocked.CompareExchange(
                ref recorderReleaseState,
                RecorderStateReleasing,
                RecorderStateUsable);
            if (prior != RecorderStateUsable)
            {
                throw new InvalidOperationException(
                    prior == RecorderStateReleased
                        ? "The Recorder configuration has already been released."
                        : prior == RecorderStateOutcomeUnverified
                            ? "The Recorder configuration release outcome is unverified."
                            : "The Recorder configuration is currently being released.");
            }
        }

        internal void CompleteRecorderRelease()
        {
            Volatile.Write(ref recorderReleaseState, RecorderStateReleased);
        }

        internal void CancelRecorderRelease()
        {
            Interlocked.CompareExchange(
                ref recorderReleaseState,
                RecorderStateUsable,
                RecorderStateReleasing);
        }

        internal void MarkRecorderReleaseOutcomeUnverified()
        {
            Volatile.Write(
                ref recorderReleaseState,
                RecorderStateOutcomeUnverified);
        }

        internal void ApplyStatusMetadata(LMCRecorderStatus status)
        {
            if (status == null)
            {
                throw new ArgumentNullException("status");
            }

            lock (metadataSync)
            {
                ApplyConfigurationMetadata(
                    status.ConfigId,
                    status.ConfigRevision,
                    status.Capacity);
                ApplyCapturePhase(status.CapturePhase);

                var mutableStartCycle = !status.IsFrozen
                    && !status.HasTrigger
                    && (!HasConfigurationShape
                        || TriggerType != LMCRecorderTriggerType.Manual
                        || status.SampleCount == 0);
                if (hasAcceptedStartCycleMetadata && mutableStartCycle)
                {
                    throw new InvalidOperationException(
                        "Recorder returned to mutable start-cycle metadata after it was fixed.");
                }

                if (mutableStartCycle)
                {
                    return;
                }

                if (!hasAcceptedStartCycleMetadata)
                {
                    AcceptedStartCycle = status.StartCycle;
                    hasAcceptedStartCycleMetadata = true;
                }
                else if (AcceptedStartCycle != status.StartCycle)
                {
                    throw new InvalidOperationException(
                        "Recorder StartCycle changed for an existing identity.");
                }
            }
        }

        internal void ApplyHeaderMetadata(LMCRecorderHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            lock (metadataSync)
            {
                if (frozenHeader != null
                    && !RecorderHeadersEqual(frozenHeader, header))
                {
                    throw new InvalidOperationException(
                        "Frozen Recorder header changed for an existing identity.");
                }

                ApplyConfigurationMetadata(
                    header.ConfigId,
                    header.ConfigRevision,
                    header.Capacity);
                ApplyCapturePhase(header.CapturePhase);
                if (SamplePeriodUs == 0)
                {
                    SamplePeriodUs = header.SamplePeriodUs;
                }
                else if (SamplePeriodUs != header.SamplePeriodUs)
                {
                    throw new InvalidOperationException(
                        "Recorder SamplePeriodUs changed for an existing identity.");
                }

                if (!hasAcceptedStartCycleMetadata)
                {
                    AcceptedStartCycle = header.StartCycle;
                    hasAcceptedStartCycleMetadata = true;
                }
                else if (AcceptedStartCycle != header.StartCycle)
                {
                    throw new InvalidOperationException(
                        "Recorder StartCycle changed for an existing identity.");
                }

                if (signalIds.Count == 0)
                {
                    var copy = new uint[header.SignalIds.Count];
                    for (var index = 0; index < copy.Length; index++)
                    {
                        copy[index] = header.SignalIds[index];
                    }

                    signalIds = new ReadOnlyCollection<uint>(copy);
                }

                if (hasFrozenHeaderMetadata
                    && (frozenSampleCount != header.SampleCount
                        || frozenSampleStrideBytes != header.SampleStrideBytes
                        || dataCrcPolicy != header.DataCrcPolicy))
                {
                    throw new InvalidOperationException(
                        "Frozen Recorder header metadata changed for an existing identity.");
                }

                frozenSampleCount = header.SampleCount;
                frozenSampleStrideBytes = header.SampleStrideBytes;
                dataCrcPolicy = header.DataCrcPolicy;
                frozenHeader = header;
                hasFrozenHeaderMetadata = true;
            }
        }

        private void ApplyConfigurationMetadata(
            uint configId,
            uint configRevision,
            uint capacity)
        {
            if (configId == 0 || configRevision == 0 || capacity == 0)
            {
                throw new InvalidOperationException(
                    "Recorder configuration metadata must be non-zero.");
            }

            if ((ConfigId != 0 && ConfigId != configId)
                || (ConfigRevision != 0 && ConfigRevision != configRevision)
                || (Capacity != 0 && Capacity != capacity))
            {
                throw new InvalidOperationException(
                    "Recorder configuration metadata changed for an existing identity.");
            }

            ConfigId = configId;
            ConfigRevision = configRevision;
            Capacity = capacity;
        }

        private void ApplyCapturePhase(LMCCapturePhase capturePhase)
        {
            if (capturePhase != LMCCapturePhase.InputMapped
                && capturePhase != LMCCapturePhase.PreOutput)
            {
                throw new InvalidOperationException(
                    "Recorder CapturePhase is invalid.");
            }

            if (CapturePhase != LMCCapturePhase.None
                && CapturePhase != capturePhase)
            {
                throw new InvalidOperationException(
                    "Recorder CapturePhase changed for an existing identity.");
            }

            CapturePhase = capturePhase;
        }

        private static bool RecorderHeadersEqual(
            LMCRecorderHeader left,
            LMCRecorderHeader right)
        {
            if (left.DiagnosticsBootId != right.DiagnosticsBootId
                || left.RecordId != right.RecordId
                || left.BufferId != right.BufferId
                || left.ConfigId != right.ConfigId
                || left.ConfigRevision != right.ConfigRevision
                || left.MapRevision != right.MapRevision
                || left.CapturePhase != right.CapturePhase
                || left.StopReason != right.StopReason
                || left.HeaderFlags != right.HeaderFlags
                || left.SampleCount != right.SampleCount
                || left.Capacity != right.Capacity
                || left.ChannelCount != right.ChannelCount
                || left.SampleStrideBytes != right.SampleStrideBytes
                || left.SamplePeriodUs != right.SamplePeriodUs
                || left.DataEncoding != right.DataEncoding
                || left.DataCrcPolicy != right.DataCrcPolicy
                || left.TriggerIndex != right.TriggerIndex
                || left.StartCycle != right.StartCycle
                || left.TriggerCycle != right.TriggerCycle
                || left.EndCycle != right.EndCycle
                || left.StartTimestampLow != right.StartTimestampLow
                || left.StartTimestampHigh != right.StartTimestampHigh
                || left.TriggerTimestampLow != right.TriggerTimestampLow
                || left.TriggerTimestampHigh != right.TriggerTimestampHigh
                || left.EndTimestampLow != right.EndTimestampLow
                || left.EndTimestampHigh != right.EndTimestampHigh
                || left.DroppedSamples != right.DroppedSamples
                || left.OverflowCount != right.OverflowCount
                || left.SignalIds.Count != right.SignalIds.Count)
            {
                return false;
            }

            for (var index = 0; index < left.SignalIds.Count; index++)
            {
                if (left.SignalIds[index] != right.SignalIds[index])
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class LMCRecorderStatus
    {
        internal LMCRecorderStatus(
            LMCDiagnosticsResponse response,
            uint recordId,
            uint bufferId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            LMCRecorderState state,
            LMCCapturePhase capturePhase,
            LMCRecorderStopReason stopReason,
            uint sampleCount,
            uint capacity,
            uint triggerIndex,
            uint startCycle,
            uint endCycle,
            uint droppedSamples,
            uint overflowCount,
            uint ownerSessionEpoch,
            uint diagnosticsBootId)
        {
            Response = response;
            RecordId = recordId;
            BufferId = bufferId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            State = state;
            CapturePhase = capturePhase;
            StopReason = stopReason;
            SampleCount = sampleCount;
            Capacity = capacity;
            TriggerIndex = triggerIndex;
            StartCycle = startCycle;
            EndCycle = endCycle;
            DroppedSamples = droppedSamples;
            OverflowCount = overflowCount;
            OwnerSessionEpoch = ownerSessionEpoch;
            DiagnosticsBootId = diagnosticsBootId;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint RecordId { get; private set; }
        public uint BufferId { get; private set; }
        public uint ConfigId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCRecorderState State { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public LMCRecorderStopReason StopReason { get; private set; }
        public uint SampleCount { get; private set; }
        public uint Capacity { get; private set; }
        public uint TriggerIndex { get; private set; }
        public uint StartCycle { get; private set; }
        public uint EndCycle { get; private set; }
        public uint DroppedSamples { get; private set; }
        public uint OverflowCount { get; private set; }
        public uint OwnerSessionEpoch { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public bool HasTrigger { get { return TriggerIndex != uint.MaxValue; } }
        public bool IsFrozen
        {
            get
            {
                return State == LMCRecorderState.Ready
                    || State == LMCRecorderState.Uploading;
            }
        }
    }

    public sealed class LMCRecorderHeader
    {
        private readonly ReadOnlyCollection<uint> signalIds;

        internal LMCRecorderHeader(
            LMCDiagnosticsResponse response,
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            LMCCapturePhase capturePhase,
            LMCRecorderStopReason stopReason,
            LMCRecorderHeaderFlags headerFlags,
            uint sampleCount,
            uint capacity,
            ushort sampleStrideBytes,
            uint samplePeriodUs,
            LMCRecorderDataEncoding dataEncoding,
            LMCRecorderDataCrcPolicy dataCrcPolicy,
            uint triggerIndex,
            uint startCycle,
            uint triggerCycle,
            uint endCycle,
            uint startTimestampLow,
            uint startTimestampHigh,
            uint triggerTimestampLow,
            uint triggerTimestampHigh,
            uint endTimestampLow,
            uint endTimestampHigh,
            uint droppedSamples,
            uint overflowCount,
            IList<uint> configuredSignalIds)
        {
            Response = response;
            DiagnosticsBootId = diagnosticsBootId;
            RecordId = recordId;
            BufferId = bufferId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            CapturePhase = capturePhase;
            StopReason = stopReason;
            HeaderFlags = headerFlags;
            SampleCount = sampleCount;
            Capacity = capacity;
            SampleStrideBytes = sampleStrideBytes;
            SamplePeriodUs = samplePeriodUs;
            DataEncoding = dataEncoding;
            DataCrcPolicy = dataCrcPolicy;
            TriggerIndex = triggerIndex;
            StartCycle = startCycle;
            TriggerCycle = triggerCycle;
            EndCycle = endCycle;
            StartTimestampLow = startTimestampLow;
            StartTimestampHigh = startTimestampHigh;
            TriggerTimestampLow = triggerTimestampLow;
            TriggerTimestampHigh = triggerTimestampHigh;
            EndTimestampLow = endTimestampLow;
            EndTimestampHigh = endTimestampHigh;
            DroppedSamples = droppedSamples;
            OverflowCount = overflowCount;
            signalIds = new ReadOnlyCollection<uint>(configuredSignalIds);
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint RecordId { get; private set; }
        public uint BufferId { get; private set; }
        public uint ConfigId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public LMCRecorderStopReason StopReason { get; private set; }
        public LMCRecorderHeaderFlags HeaderFlags { get; private set; }
        public uint SampleCount { get; private set; }
        public uint Capacity { get; private set; }
        public ushort ChannelCount { get { return checked((ushort)signalIds.Count); } }
        public ushort SampleStrideBytes { get; private set; }
        public uint SamplePeriodUs { get; private set; }
        public LMCRecorderDataEncoding DataEncoding { get; private set; }
        public LMCRecorderDataCrcPolicy DataCrcPolicy { get; private set; }
        public uint TriggerIndex { get; private set; }
        public uint StartCycle { get; private set; }
        public uint TriggerCycle { get; private set; }
        public uint EndCycle { get; private set; }
        public uint StartTimestampLow { get; private set; }
        public uint StartTimestampHigh { get; private set; }
        public uint TriggerTimestampLow { get; private set; }
        public uint TriggerTimestampHigh { get; private set; }
        public uint EndTimestampLow { get; private set; }
        public uint EndTimestampHigh { get; private set; }
        public uint DroppedSamples { get; private set; }
        public uint OverflowCount { get; private set; }
        public IReadOnlyList<uint> SignalIds { get { return signalIds; } }
        public bool HasTrigger { get { return TriggerIndex != uint.MaxValue; } }
        public ulong StartTimestampUs
        {
            get { return ((ulong)StartTimestampHigh << 32) | StartTimestampLow; }
        }
        public ulong TriggerTimestampUs
        {
            get { return ((ulong)TriggerTimestampHigh << 32) | TriggerTimestampLow; }
        }
        public ulong EndTimestampUs
        {
            get { return ((ulong)EndTimestampHigh << 32) | EndTimestampLow; }
        }
    }

    public sealed class LMCRecorderChunkRequest
    {
        public LMCRecorderChunkRequest(
            LMCRecorderIdentity identity,
            uint offsetSample,
            ushort requestedSampleCount,
            uint sequence)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (requestedSampleCount == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "requestedSampleCount",
                    "Recorder chunk sample count must be non-zero.");
            }

            Identity = identity;
            OffsetSample = offsetSample;
            RequestedSampleCount = requestedSampleCount;
            Sequence = sequence;
        }

        public LMCRecorderIdentity Identity { get; private set; }
        public uint OffsetSample { get; private set; }
        public ushort RequestedSampleCount { get; private set; }
        public uint Sequence { get; private set; }
    }

    public sealed class LMCRecorderChunk
    {
        private readonly ReadOnlyCollection<byte> data;

        internal LMCRecorderChunk(
            LMCDiagnosticsResponse response,
            uint recordId,
            uint bufferId,
            uint offsetSample,
            ushort returnedSampleCount,
            ushort channelCount,
            uint sequence,
            uint totalSamples,
            ushort sampleStrideBytes,
            uint dataCrc32,
            uint diagnosticsBootId,
            byte[] rawData)
        {
            Response = response;
            RecordId = recordId;
            BufferId = bufferId;
            OffsetSample = offsetSample;
            ReturnedSampleCount = returnedSampleCount;
            ChannelCount = channelCount;
            Sequence = sequence;
            TotalSamples = totalSamples;
            SampleStrideBytes = sampleStrideBytes;
            DataCrc32 = dataCrc32;
            DiagnosticsBootId = diagnosticsBootId;
            data = new ReadOnlyCollection<byte>(rawData);
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint RecordId { get; private set; }
        public uint BufferId { get; private set; }
        public uint OffsetSample { get; private set; }
        public ushort ReturnedSampleCount { get; private set; }
        public ushort ChannelCount { get; private set; }
        public uint Sequence { get; private set; }
        public uint TotalSamples { get; private set; }
        public ushort SampleStrideBytes { get; private set; }
        public uint DataCrc32 { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public IReadOnlyList<byte> Data { get { return data; } }
        public int DataByteCount { get { return data.Count; } }
        public bool IsLastChunk
        {
            get
            {
                return (Response.ResponseFlags
                    & LMCDiagnosticsResponseFlags.LastChunk) != 0;
            }
        }

        internal byte[] CopyData()
        {
            var copy = new byte[data.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = data[index];
            }

            return copy;
        }
    }

    public sealed class LMCRecorderDownloadProgress
    {
        internal LMCRecorderDownloadProgress(
            uint downloadedSamples,
            uint totalSamples,
            int downloadedBytes,
            int totalBytes,
            uint completedChunks)
        {
            DownloadedSamples = downloadedSamples;
            TotalSamples = totalSamples;
            DownloadedBytes = downloadedBytes;
            TotalBytes = totalBytes;
            CompletedChunks = completedChunks;
        }

        public uint DownloadedSamples { get; private set; }
        public uint TotalSamples { get; private set; }
        public int DownloadedBytes { get; private set; }
        public int TotalBytes { get; private set; }
        public uint CompletedChunks { get; private set; }
        public double Fraction
        {
            get
            {
                return TotalBytes == 0
                    ? 1.0
                    : (double)DownloadedBytes / TotalBytes;
            }
        }
    }

    public sealed class LMCRecorderData
    {
        private readonly ReadOnlyCollection<byte> data;

        internal LMCRecorderData(LMCRecorderHeader header, byte[] rawData)
        {
            Header = header ?? throw new ArgumentNullException("header");
            data = new ReadOnlyCollection<byte>(
                rawData ?? throw new ArgumentNullException("rawData"));
        }

        public LMCRecorderHeader Header { get; private set; }
        public IReadOnlyList<byte> Data { get { return data; } }

        public uint GetRawUInt32(uint sampleIndex, ushort channelIndex)
        {
            if (sampleIndex >= Header.SampleCount)
            {
                throw new ArgumentOutOfRangeException("sampleIndex");
            }

            if (channelIndex >= Header.ChannelCount)
            {
                throw new ArgumentOutOfRangeException("channelIndex");
            }

            var offset = checked(
                (int)(sampleIndex * Header.SampleStrideBytes
                    + channelIndex * sizeof(uint)));
            return (uint)(data[offset]
                | data[offset + 1] << 8
                | data[offset + 2] << 16
                | data[offset + 3] << 24);
        }

        public int GetRawInt32(uint sampleIndex, ushort channelIndex)
        {
            return unchecked((int)GetRawUInt32(sampleIndex, channelIndex));
        }
    }
}

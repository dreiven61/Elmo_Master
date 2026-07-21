using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCBulkState : ushort
    {
        Empty = 0,
        Pending = 1,
        Active = 2,
        Failed = 3
    }

    [Flags]
    public enum LMCBulkSnapshotFlags : uint
    {
        None = 0,
        SameCycle = 1u << 0,
        InputMappedPhase = 1u << 1,
        PreOutputPhase = 1u << 2
    }

    public sealed class LMCBulkStatus
    {
        internal LMCBulkStatus(
            LMCDiagnosticsResponse response,
            uint bulkId,
            uint configRevision,
            uint mapRevision,
            LMCBulkState state,
            ushort signalCount,
            uint activationCycle)
        {
            Response = response;
            BulkId = bulkId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            State = state;
            SignalCount = signalCount;
            ActivationCycle = activationCycle;
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint BulkId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCBulkState State { get; private set; }
        public ushort SignalCount { get; private set; }
        public uint ActivationCycle { get; private set; }

        public bool IsActive
        {
            get { return State == LMCBulkState.Active; }
        }
    }

    public sealed class LMCBulkConfiguration
    {
        private const int ReleaseStateUsable = 0;
        private const int ReleaseStateInProgress = 1;
        private const int ReleaseStateReleased = 2;

        private readonly ReadOnlyCollection<uint> signalIds;
        private int releaseState;

        internal LMCBulkConfiguration(
            LMCBulkStatus configuredStatus,
            uint diagnosticsBootId,
            long connectionSessionGeneration,
            LMCDiagnostics owner,
            IReadOnlyList<uint> configuredSignalIds)
        {
            if (configuredStatus == null)
            {
                throw new ArgumentNullException("configuredStatus");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "A Bulk configuration requires a non-zero DiagnosticsBootId.");
            }

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (configuredSignalIds == null)
            {
                throw new ArgumentNullException("configuredSignalIds");
            }

            var copiedSignalIds = new uint[configuredSignalIds.Count];
            for (var index = 0; index < copiedSignalIds.Length; index++)
            {
                copiedSignalIds[index] = configuredSignalIds[index];
            }

            if (copiedSignalIds.Length != configuredStatus.SignalCount)
            {
                throw new ArgumentException(
                    "Configured signal count does not match the Bulk response.",
                    "configuredSignalIds");
            }

            ConfigurationResponse = configuredStatus.Response;
            DiagnosticsBootId = diagnosticsBootId;
            BulkId = configuredStatus.BulkId;
            ConfigRevision = configuredStatus.ConfigRevision;
            MapRevision = configuredStatus.MapRevision;
            InitialState = configuredStatus.State;
            SignalCount = configuredStatus.SignalCount;
            ActivationCycle = configuredStatus.ActivationCycle;
            ConnectionSessionGeneration = connectionSessionGeneration;
            Owner = owner;
            signalIds = new ReadOnlyCollection<uint>(copiedSignalIds);
        }

        public LMCDiagnosticsResponse ConfigurationResponse { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint BulkId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public LMCBulkState InitialState { get; private set; }
        public ushort SignalCount { get; private set; }
        public uint ActivationCycle { get; private set; }
        public IReadOnlyList<uint> SignalIds { get { return signalIds; } }

        public bool IsReleased
        {
            get
            {
                return Volatile.Read(ref releaseState)
                    == ReleaseStateReleased;
            }
        }

        internal long ConnectionSessionGeneration { get; private set; }
        internal LMCDiagnostics Owner { get; private set; }

        internal void EnsureUsable()
        {
            var state = Volatile.Read(ref releaseState);
            if (state == ReleaseStateReleased)
            {
                throw new InvalidOperationException(
                    "The Bulk configuration has already been released.");
            }

            if (state == ReleaseStateInProgress)
            {
                throw new InvalidOperationException(
                    "The Bulk configuration is currently being released.");
            }
        }

        internal void BeginRelease()
        {
            var prior = Interlocked.CompareExchange(
                ref releaseState,
                ReleaseStateInProgress,
                ReleaseStateUsable);

            if (prior == ReleaseStateReleased)
            {
                throw new InvalidOperationException(
                    "The Bulk configuration has already been released.");
            }

            if (prior == ReleaseStateInProgress)
            {
                throw new InvalidOperationException(
                    "The Bulk configuration is currently being released.");
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
    }

    public sealed class LMCBulkSnapshot
    {
        private readonly ReadOnlyCollection<LMCSignalValueEntry> entries;

        internal LMCBulkSnapshot(
            LMCDiagnosticsResponse response,
            uint bulkId,
            uint configRevision,
            uint mapRevision,
            uint cycleCounter,
            uint timestampLow,
            uint timestampHigh,
            ushort entryStride,
            LMCCapturePhase capturePhase,
            uint snapshotSequence,
            LMCBulkSnapshotFlags snapshotFlags,
            IList<LMCSignalValueEntry> entries)
        {
            Response = response;
            BulkId = bulkId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            CycleCounter = cycleCounter;
            TimestampLow = timestampLow;
            TimestampHigh = timestampHigh;
            EntryStride = entryStride;
            CapturePhase = capturePhase;
            SnapshotSequence = snapshotSequence;
            SnapshotFlags = snapshotFlags;
            this.entries = new ReadOnlyCollection<LMCSignalValueEntry>(entries);
        }

        public LMCDiagnosticsResponse Response { get; private set; }
        public uint BulkId { get; private set; }
        public uint ConfigRevision { get; private set; }
        public uint MapRevision { get; private set; }
        public uint CycleCounter { get; private set; }
        public uint TimestampLow { get; private set; }
        public uint TimestampHigh { get; private set; }
        public ushort EntryStride { get; private set; }
        public LMCCapturePhase CapturePhase { get; private set; }
        public uint SnapshotSequence { get; private set; }
        public LMCBulkSnapshotFlags SnapshotFlags { get; private set; }
        public IReadOnlyList<LMCSignalValueEntry> Entries { get { return entries; } }
        public ushort EntryCount { get { return checked((ushort)entries.Count); } }

        public ulong TimestampUs
        {
            get { return ((ulong)TimestampHigh << 32) | TimestampLow; }
        }

        public bool IsPartial
        {
            get
            {
                return (Response.ResponseFlags
                    & LMCDiagnosticsResponseFlags.Partial) != 0;
            }
        }
    }
}

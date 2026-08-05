using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum AxisCommandRecoveryOperation
    {
        Stop = 1,
        Reset = 2
    }

    internal enum AxisCommandRecoveryState
    {
        ArmedBeforeDispatch = 1,
        AcceptedAwaitingProof = 2,
        RecoveryRequired = 3,
        Resolved = 4
    }

    internal sealed class AxisCommandRecoveryRecord
    {
        internal AxisCommandRecoveryRecord(
            Guid identity,
            AxisCommandRecoveryOperation operation,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            int stopDeceleration,
            int stopJerk,
            int requiredStableSampleCount,
            Guid supersededResetIdentity,
            AxisCommandRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Axis command recovery identity cannot be empty.",
                    "identity");
            }

            ValidateOperation(operation);
            EndpointIp = NormalizeEndpointIp(endpointIp);
            if (endpointPort < 1 || endpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException("endpointPort");
            }

            ValidateAxisName(axisName);
            if (axisReference == 0)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            if (requiredStableSampleCount < 1
                || requiredStableSampleCount > 100)
            {
                throw new ArgumentOutOfRangeException(
                    "requiredStableSampleCount");
            }

            if (operation == AxisCommandRecoveryOperation.Stop)
            {
                if (stopDeceleration <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "stopDeceleration");
                }

                if (stopJerk < 0)
                {
                    throw new ArgumentOutOfRangeException("stopJerk");
                }
            }
            else if (stopDeceleration != 0 || stopJerk != 0)
            {
                throw new ArgumentException(
                    "Reset recovery cannot carry Stop motion parameters.");
            }

            if (operation == AxisCommandRecoveryOperation.Reset
                && supersededResetIdentity != Guid.Empty)
            {
                throw new ArgumentException(
                    "Reset recovery cannot supersede another Reset record.",
                    "supersededResetIdentity");
            }

            ValidateState(state);
            if (createdUtc.Kind != DateTimeKind.Utc
                || updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Recovery timestamps must be UTC and monotonic.");
            }

            Identity = identity;
            Operation = operation;
            EndpointPort = endpointPort;
            AxisName = axisName;
            AxisReference = axisReference;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            StopDeceleration = stopDeceleration;
            StopJerk = stopJerk;
            RequiredStableSampleCount = requiredStableSampleCount;
            SupersededResetIdentity = supersededResetIdentity;
            State = state;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
        }

        internal Guid Identity { get; private set; }
        internal AxisCommandRecoveryOperation Operation { get; private set; }
        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal string AxisName { get; private set; }
        internal ushort AxisReference { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal int StopDeceleration { get; private set; }
        internal int StopJerk { get; private set; }
        internal int RequiredStableSampleCount { get; private set; }
        internal Guid SupersededResetIdentity { get; private set; }
        internal AxisCommandRecoveryState State { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }

        internal bool IsActive
        {
            get { return State != AxisCommandRecoveryState.Resolved; }
        }

        internal bool MatchesEndpoint(string endpointIp, int endpointPort)
        {
            string normalized;
            return TryNormalizeEndpointIp(endpointIp, out normalized)
                && string.Equals(
                    EndpointIp,
                    normalized,
                    StringComparison.Ordinal)
                && EndpointPort == endpointPort;
        }

        internal bool MatchesPhysicalIdentity(
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            return MatchesEndpoint(endpointIp, endpointPort)
                && string.Equals(AxisName, axisName, StringComparison.Ordinal)
                && AxisReference == axisReference
                && DiagnosticsBootId == diagnosticsBootId
                && MapRevision == mapRevision;
        }

        internal bool MatchesOperation(
            AxisCommandRecoveryOperation operation,
            int stopDeceleration,
            int stopJerk,
            int requiredStableSampleCount)
        {
            return Operation == operation
                && StopDeceleration == stopDeceleration
                && StopJerk == stopJerk
                && RequiredStableSampleCount == requiredStableSampleCount;
        }

        internal AxisCommandRecoveryRecord Copy()
        {
            return new AxisCommandRecoveryRecord(
                Identity,
                Operation,
                EndpointIp,
                EndpointPort,
                AxisName,
                AxisReference,
                DiagnosticsBootId,
                MapRevision,
                StopDeceleration,
                StopJerk,
                RequiredStableSampleCount,
                SupersededResetIdentity,
                State,
                CreatedUtc,
                UpdatedUtc);
        }

        internal AxisCommandRecoveryRecord TransitionTo(
            AxisCommandRecoveryState nextState,
            DateTime updatedUtc)
        {
            if (!CanTransition(State, nextState))
            {
                throw new InvalidOperationException(
                    "Axis command recovery cannot transition from "
                    + State
                    + " to "
                    + nextState
                    + ".");
            }

            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < UpdatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Recovery transition time cannot move backwards.");
            }

            return new AxisCommandRecoveryRecord(
                Identity,
                Operation,
                EndpointIp,
                EndpointPort,
                AxisName,
                AxisReference,
                DiagnosticsBootId,
                MapRevision,
                StopDeceleration,
                StopJerk,
                RequiredStableSampleCount,
                SupersededResetIdentity,
                nextState,
                CreatedUtc,
                updatedUtc);
        }

        private static bool CanTransition(
            AxisCommandRecoveryState current,
            AxisCommandRecoveryState next)
        {
            if (current == next || current == AxisCommandRecoveryState.Resolved)
            {
                return false;
            }

            if (next == AxisCommandRecoveryState.Resolved)
            {
                return true;
            }

            if (next == AxisCommandRecoveryState.RecoveryRequired)
            {
                return current == AxisCommandRecoveryState.ArmedBeforeDispatch
                    || current
                        == AxisCommandRecoveryState.AcceptedAwaitingProof;
            }

            return next == AxisCommandRecoveryState.AcceptedAwaitingProof
                && (current == AxisCommandRecoveryState.ArmedBeforeDispatch
                    || current == AxisCommandRecoveryState.RecoveryRequired);
        }

        private static void ValidateOperation(
            AxisCommandRecoveryOperation operation)
        {
            if (operation != AxisCommandRecoveryOperation.Stop
                && operation != AxisCommandRecoveryOperation.Reset)
            {
                throw new ArgumentOutOfRangeException("operation");
            }
        }

        private static void ValidateState(AxisCommandRecoveryState state)
        {
            if (state != AxisCommandRecoveryState.ArmedBeforeDispatch
                && state != AxisCommandRecoveryState.AcceptedAwaitingProof
                && state != AxisCommandRecoveryState.RecoveryRequired
                && state != AxisCommandRecoveryState.Resolved)
            {
                throw new ArgumentOutOfRangeException("state");
            }
        }

        private static void ValidateAxisName(string axisName)
        {
            if (string.IsNullOrWhiteSpace(axisName) || axisName.Length > 256)
            {
                throw new ArgumentException(
                    "Axis name must contain from 1 through 256 characters.",
                    "axisName");
            }

            foreach (var value in axisName)
            {
                if (value < 0x20 || value > 0x7E)
                {
                    throw new ArgumentException(
                        "Axis name must be 7-bit printable ASCII.",
                        "axisName");
                }
            }
        }

        private static string NormalizeEndpointIp(string endpointIp)
        {
            string normalized;
            if (!TryNormalizeEndpointIp(endpointIp, out normalized))
            {
                throw new ArgumentException(
                    "Recovery endpoint must be an IPv4 literal.",
                    "endpointIp");
            }

            return normalized;
        }

        private static bool TryNormalizeEndpointIp(
            string endpointIp,
            out string normalized)
        {
            normalized = null;
            IPAddress parsed;
            if (string.IsNullOrWhiteSpace(endpointIp)
                || !IPAddress.TryParse(endpointIp, out parsed)
                || parsed.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }

            normalized = parsed.ToString();
            return true;
        }
    }

    internal sealed class AxisCommandRecoveryJournal : IDisposable
    {
        private const int FormatVersion = 1;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 8192;
        private const int MaximumTextLength = 1024;
        internal const string JournalFileName =
            "axis-command-recovery.bin";
        internal const string LockFileName =
            "axis-command-recovery.lock";
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOAXC1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private AxisCommandRecoveryRecord currentRecord;
        private bool disposed;

        private AxisCommandRecoveryJournal(string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "Journal directory is required.",
                    "requestedDirectoryPath");
            }

            directoryPath = Path.GetFullPath(requestedDirectoryPath);
            journalFilePath = Path.Combine(directoryPath, JournalFileName);
            Directory.CreateDirectory(directoryPath);
            try
            {
                lockStream = new FileStream(
                    Path.Combine(directoryPath, LockFileName),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    1,
                    FileOptions.WriteThrough);
                currentRecord = LoadRecord(journalFilePath);
            }
            catch
            {
                if (lockStream != null)
                {
                    lockStream.Dispose();
                    lockStream = null;
                }

                throw;
            }
        }

        internal string DirectoryPath { get { return directoryPath; } }
        internal string JournalFilePath { get { return journalFilePath; } }

        internal AxisCommandRecoveryRecord CurrentRecord
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord == null
                        ? null
                        : currentRecord.Copy();
                }
            }
        }

        internal bool HasActiveRecord
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord != null && currentRecord.IsActive;
                }
            }
        }

        internal RecoveryJournalSourceEvidence
            CaptureActiveRetirementEvidence()
        {
            lock (sync)
            {
                ThrowIfDisposed();
                return CaptureActiveRetirementEvidenceCore();
            }
        }

        internal AxisCommandRecoveryRecord ResolveOperatorRetirement(
            RecoveryJournalSourceEvidence expectedEvidence,
            RecoveryRecordRetirementDecision committedDecision,
            DateTime updatedUtc)
        {
            if (expectedEvidence == null)
            {
                throw new ArgumentNullException("expectedEvidence");
            }
            if (committedDecision == null)
            {
                throw new ArgumentNullException("committedDecision");
            }
            if (!committedDecision.IsDurablyCommitted)
            {
                throw new InvalidOperationException(
                    "Axis command retirement requires a durably committed ledger decision.");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                if (!committedDecision.MatchesSourceEvidence(
                    expectedEvidence))
                {
                    throw new InvalidOperationException(
                        "The committed retirement decision does not match the expected Axis command source evidence.");
                }

                var currentEvidence =
                    CaptureActiveRetirementEvidenceCore();
                if (!expectedEvidence.ExactSourceEquals(currentEvidence)
                    || !committedDecision.MatchesSourceEvidence(
                        currentEvidence))
                {
                    throw new InvalidOperationException(
                        "Axis command recovery changed after operator confirmation; retirement was not applied.");
                }

                var resolved = currentRecord.TransitionTo(
                    AxisCommandRecoveryState.Resolved,
                    updatedUtc);
                PersistRecord(resolved);
                currentRecord = resolved;
                return resolved.Copy();
            }
        }

        internal static AxisCommandRecoveryJournal Open(string directoryPath)
        {
            return new AxisCommandRecoveryJournal(directoryPath);
        }

        internal static AxisCommandRecoveryJournal OpenDefault()
        {
            return Open(GetDefaultDirectoryPath());
        }

        internal static string GetDefaultDirectoryPath()
        {
            var localApplicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localApplicationData))
            {
                throw new InvalidOperationException(
                    "Windows LocalApplicationData is unavailable.");
            }

            return Path.Combine(
                localApplicationData,
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisCommandRecoveryJournal",
                "v1");
        }

        internal AxisCommandRecoveryRecord ArmBeforeDispatch(
            AxisCommandRecoveryOperation operation,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            int stopDeceleration,
            int stopJerk,
            int requiredStableSampleCount,
            DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved Axis command recovery record already exists.");
                }

                var armed = new AxisCommandRecoveryRecord(
                    Guid.NewGuid(),
                    operation,
                    endpointIp,
                    endpointPort,
                    axisName,
                    axisReference,
                    diagnosticsBootId,
                    mapRevision,
                    stopDeceleration,
                    stopJerk,
                    requiredStableSampleCount,
                    Guid.Empty,
                    AxisCommandRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(armed);
                currentRecord = armed;
                return armed.Copy();
            }
        }

        internal AxisCommandRecoveryRecord
            ReplaceActiveResetWithStopBeforeDispatch(
                Guid resetIdentity,
                string endpointIp,
                int endpointPort,
                string axisName,
                ushort axisReference,
                uint diagnosticsBootId,
                uint mapRevision,
                int stopDeceleration,
                int stopJerk,
                int requiredStableSampleCount,
                DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var reset = RequireCurrentRecord(resetIdentity);
                if (!reset.IsActive
                    || reset.Operation != AxisCommandRecoveryOperation.Reset
                    || !reset.MatchesPhysicalIdentity(
                        endpointIp,
                        endpointPort,
                        axisName,
                        axisReference,
                        diagnosticsBootId,
                        mapRevision))
                {
                    throw new InvalidOperationException(
                        "Only the exact active Reset record can be replaced by Stop.");
                }

                if (createdUtc.Kind != DateTimeKind.Utc
                    || createdUtc < reset.UpdatedUtc)
                {
                    throw new ArgumentOutOfRangeException("createdUtc");
                }

                var stop = new AxisCommandRecoveryRecord(
                    Guid.NewGuid(),
                    AxisCommandRecoveryOperation.Stop,
                    endpointIp,
                    endpointPort,
                    axisName,
                    axisReference,
                    diagnosticsBootId,
                    mapRevision,
                    stopDeceleration,
                    stopJerk,
                    requiredStableSampleCount,
                    reset.Identity,
                    AxisCommandRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(stop);
                currentRecord = stop;
                return stop.Copy();
            }
        }

        internal AxisCommandRecoveryRecord MarkAccepted(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                AxisCommandRecoveryState.AcceptedAwaitingProof,
                updatedUtc);
        }

        internal AxisCommandRecoveryRecord RestoreResetAfterStopNotAttempted(
            Guid stopIdentity,
            AxisCommandRecoveryRecord resetSnapshot,
            DateTime updatedUtc)
        {
            if (resetSnapshot == null)
            {
                throw new ArgumentNullException("resetSnapshot");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                var stop = RequireCurrentRecord(stopIdentity);
                if (!stop.IsActive
                    || stop.Operation != AxisCommandRecoveryOperation.Stop
                    || stop.State
                        != AxisCommandRecoveryState.ArmedBeforeDispatch
                    || !resetSnapshot.IsActive
                    || resetSnapshot.Operation
                        != AxisCommandRecoveryOperation.Reset
                    || stop.SupersededResetIdentity
                        != resetSnapshot.Identity
                    || !stop.MatchesPhysicalIdentity(
                        resetSnapshot.EndpointIp,
                        resetSnapshot.EndpointPort,
                        resetSnapshot.AxisName,
                        resetSnapshot.AxisReference,
                        resetSnapshot.DiagnosticsBootId,
                        resetSnapshot.MapRevision))
                {
                    throw new InvalidOperationException(
                        "Only the exact unsent Armed Stop replacement can restore its prior Reset snapshot.");
                }

                if (updatedUtc.Kind != DateTimeKind.Utc
                    || updatedUtc < stop.UpdatedUtc
                    || updatedUtc < resetSnapshot.UpdatedUtc)
                {
                    throw new ArgumentOutOfRangeException("updatedUtc");
                }

                var restored = new AxisCommandRecoveryRecord(
                    resetSnapshot.Identity,
                    resetSnapshot.Operation,
                    resetSnapshot.EndpointIp,
                    resetSnapshot.EndpointPort,
                    resetSnapshot.AxisName,
                    resetSnapshot.AxisReference,
                    resetSnapshot.DiagnosticsBootId,
                    resetSnapshot.MapRevision,
                    resetSnapshot.StopDeceleration,
                    resetSnapshot.StopJerk,
                    resetSnapshot.RequiredStableSampleCount,
                    resetSnapshot.SupersededResetIdentity,
                    resetSnapshot.State,
                    resetSnapshot.CreatedUtc,
                    updatedUtc);
                PersistRecord(restored);
                currentRecord = restored;
                return restored.Copy();
            }
        }

        internal AxisCommandRecoveryRecord PromoteToRecoveryRequired(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                AxisCommandRecoveryState.RecoveryRequired,
                updatedUtc);
        }

        internal AxisCommandRecoveryRecord Resolve(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                AxisCommandRecoveryState.Resolved,
                updatedUtc);
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                if (lockStream != null)
                {
                    lockStream.Dispose();
                    lockStream = null;
                }
            }
        }

        private AxisCommandRecoveryRecord Transition(
            Guid identity,
            AxisCommandRecoveryState nextState,
            DateTime updatedUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var transitioned = RequireCurrentRecord(identity)
                    .TransitionTo(nextState, updatedUtc);
                PersistRecord(transitioned);
                currentRecord = transitioned;
                return transitioned.Copy();
            }
        }

        private AxisCommandRecoveryRecord RequireCurrentRecord(Guid identity)
        {
            if (currentRecord == null)
            {
                throw new InvalidOperationException(
                    "No Axis command recovery record exists.");
            }

            if (identity == Guid.Empty || currentRecord.Identity != identity)
            {
                throw new InvalidOperationException(
                    "Axis command recovery identity does not match.");
            }

            return currentRecord;
        }

        private RecoveryJournalSourceEvidence
            CaptureActiveRetirementEvidenceCore()
        {
            if (currentRecord == null || !currentRecord.IsActive)
            {
                throw new InvalidOperationException(
                    "No active Axis command recovery record exists for operator retirement.");
            }

            var originalBytes = ReadRetirementSourceBytes();
            var diskRecord = DeserializeRecord(originalBytes);
            if (!RecordsEqual(currentRecord, diskRecord))
            {
                throw new InvalidDataException(
                    "Axis command recovery memory state does not match the exact durable source bytes.");
            }

            return new RecoveryJournalSourceEvidence(
                RecoveryRecordOwner.AxisCommand,
                diskRecord.Identity,
                (int)diskRecord.State,
                diskRecord.CreatedUtc,
                diskRecord.UpdatedUtc,
                diskRecord.EndpointIp,
                diskRecord.EndpointPort,
                diskRecord.DiagnosticsBootId,
                diskRecord.MapRevision,
                "Axis",
                diskRecord.AxisName,
                diskRecord.AxisReference,
                diskRecord.Operation.ToString(),
                "StopDeceleration="
                    + diskRecord.StopDeceleration
                    + ";StopJerk="
                    + diskRecord.StopJerk
                    + ";RequiredStableSampleCount="
                    + diskRecord.RequiredStableSampleCount
                    + ";SupersededResetIdentity="
                    + diskRecord.SupersededResetIdentity.ToString("D"),
                originalBytes);
        }

        private byte[] ReadRetirementSourceBytes()
        {
            using (var stream = new FileStream(
                journalFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            {
                if (stream.Length < 1 || stream.Length > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Axis command recovery source length is invalid.");
                }
                var bytes = new byte[checked((int)stream.Length)];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    var read = stream.Read(
                        bytes,
                        offset,
                        bytes.Length - offset);
                    if (read <= 0)
                    {
                        throw new InvalidDataException(
                            "Axis command recovery source is truncated.");
                    }
                    offset += read;
                }
                return bytes;
            }
        }

        private static bool RecordsEqual(
            AxisCommandRecoveryRecord left,
            AxisCommandRecoveryRecord right)
        {
            return left != null
                && right != null
                && left.Identity == right.Identity
                && left.Operation == right.Operation
                && string.Equals(
                    left.EndpointIp,
                    right.EndpointIp,
                    StringComparison.Ordinal)
                && left.EndpointPort == right.EndpointPort
                && string.Equals(
                    left.AxisName,
                    right.AxisName,
                    StringComparison.Ordinal)
                && left.AxisReference == right.AxisReference
                && left.DiagnosticsBootId == right.DiagnosticsBootId
                && left.MapRevision == right.MapRevision
                && left.StopDeceleration == right.StopDeceleration
                && left.StopJerk == right.StopJerk
                && left.RequiredStableSampleCount
                    == right.RequiredStableSampleCount
                && left.SupersededResetIdentity
                    == right.SupersededResetIdentity
                && left.State == right.State
                && left.CreatedUtc == right.CreatedUtc
                && left.UpdatedUtc == right.UpdatedUtc;
        }

        private void PersistRecord(AxisCommandRecoveryRecord record)
        {
            var bytes = SerializeRecord(record);
            var temporaryPath = Path.Combine(
                directoryPath,
                JournalFileName + "." + Guid.NewGuid().ToString("N") + ".tmp");
            var temporaryExists = false;
            try
            {
                using (var stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough))
                {
                    temporaryExists = true;
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(journalFilePath))
                {
                    File.Replace(temporaryPath, journalFilePath, null, true);
                }
                else
                {
                    File.Move(temporaryPath, journalFilePath);
                }

                temporaryExists = false;
            }
            finally
            {
                if (temporaryExists && File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static AxisCommandRecoveryRecord LoadRecord(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            {
                if (stream.Length < Magic.Length + 8 + ChecksumLength
                    || stream.Length > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Axis command recovery journal length is invalid.");
                }

                var bytes = new byte[checked((int)stream.Length)];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    var read = stream.Read(bytes, offset, bytes.Length - offset);
                    if (read <= 0)
                    {
                        throw new InvalidDataException(
                            "Axis command recovery journal is truncated.");
                    }

                    offset += read;
                }

                return DeserializeRecord(bytes);
            }
        }

        private static byte[] SerializeRecord(AxisCommandRecoveryRecord record)
        {
            byte[] payload;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(record.Identity.ToByteArray());
                writer.Write((int)record.Operation);
                writer.Write((int)record.State);
                writer.Write(record.CreatedUtc.Ticks);
                writer.Write(record.UpdatedUtc.Ticks);
                writer.Write(record.DiagnosticsBootId);
                writer.Write(record.MapRevision);
                writer.Write(record.EndpointPort);
                writer.Write(record.AxisReference);
                writer.Write(record.StopDeceleration);
                writer.Write(record.StopJerk);
                writer.Write(record.RequiredStableSampleCount);
                writer.Write(record.SupersededResetIdentity.ToByteArray());
                WriteText(writer, record.EndpointIp);
                WriteText(writer, record.AxisName);
                writer.Flush();
                payload = stream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(Magic);
                writer.Write(FormatVersion);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
                prefix = stream.ToArray();
            }

            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(prefix);
            }

            var bytes = new byte[prefix.Length + checksum.Length];
            Buffer.BlockCopy(prefix, 0, bytes, 0, prefix.Length);
            Buffer.BlockCopy(
                checksum,
                0,
                bytes,
                prefix.Length,
                checksum.Length);
            return bytes;
        }

        private static AxisCommandRecoveryRecord DeserializeRecord(
            byte[] bytes)
        {
            try
            {
                if (bytes == null
                    || bytes.Length < Magic.Length + 8 + ChecksumLength
                    || bytes.Length > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Axis command recovery journal length is invalid.");
                }

                var checksumOffset = bytes.Length - ChecksumLength;
                byte[] checksum;
                using (var sha256 = SHA256.Create())
                {
                    checksum = sha256.ComputeHash(bytes, 0, checksumOffset);
                }

                if (!ChecksumEquals(checksum, bytes, checksumOffset))
                {
                    throw new InvalidDataException(
                        "Axis command recovery journal checksum is invalid.");
                }

                using (var stream = new MemoryStream(
                    bytes,
                    0,
                    checksumOffset,
                    false))
                using (var reader = new BinaryReader(
                    stream,
                    Encoding.ASCII,
                    true))
                {
                    if (!ByteArraysEqual(Magic, reader.ReadBytes(Magic.Length)))
                    {
                        throw new InvalidDataException(
                            "Axis command recovery journal header is invalid.");
                    }

                    if (reader.ReadInt32() != FormatVersion)
                    {
                        throw new InvalidDataException(
                            "Axis command recovery journal version is unsupported.");
                    }

                    var payloadLength = reader.ReadInt32();
                    if (payloadLength <= 0
                        || payloadLength
                            != checksumOffset - Magic.Length - 8)
                    {
                        throw new InvalidDataException(
                            "Axis command recovery payload length is invalid.");
                    }

                    var payload = reader.ReadBytes(payloadLength);
                    if (payload.Length != payloadLength
                        || stream.Position != checksumOffset)
                    {
                        throw new InvalidDataException(
                            "Axis command recovery payload is incomplete.");
                    }

                    return DeserializePayload(payload);
                }
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception error) when (
                error is EndOfStreamException
                || error is ArgumentException
                || error is OverflowException)
            {
                throw new InvalidDataException(
                    "Axis command recovery journal is invalid.",
                    error);
            }
        }

        private static AxisCommandRecoveryRecord DeserializePayload(
            byte[] payload)
        {
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var identityBytes = reader.ReadBytes(16);
                if (identityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Axis command recovery identity is incomplete.");
                }

                var identity = new Guid(identityBytes);
                var operation = (AxisCommandRecoveryOperation)
                    reader.ReadInt32();
                var state = (AxisCommandRecoveryState)reader.ReadInt32();
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var bootId = reader.ReadUInt32();
                var mapRevision = reader.ReadUInt32();
                var port = reader.ReadInt32();
                var axisReference = reader.ReadUInt16();
                var stopDeceleration = reader.ReadInt32();
                var stopJerk = reader.ReadInt32();
                var stableCount = reader.ReadInt32();
                var supersededResetIdentityBytes = reader.ReadBytes(16);
                if (supersededResetIdentityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Axis command predecessor identity is incomplete.");
                }
                var supersededResetIdentity = new Guid(
                    supersededResetIdentityBytes);
                var endpointIp = ReadText(reader);
                var axisName = ReadText(reader);
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Axis command recovery journal has trailing data.");
                }

                var record = new AxisCommandRecoveryRecord(
                    identity,
                    operation,
                    endpointIp,
                    port,
                    axisName,
                    axisReference,
                    bootId,
                    mapRevision,
                    stopDeceleration,
                    stopJerk,
                    stableCount,
                    supersededResetIdentity,
                    state,
                    createdUtc,
                    updatedUtc);
                if (!string.Equals(
                        endpointIp,
                        record.EndpointIp,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Axis command recovery endpoint is not canonical.");
                }

                return record;
            }
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length < 1 || bytes.Length > MaximumTextLength)
            {
                throw new InvalidOperationException(
                    "Axis command recovery text length is invalid.");
            }

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length < 1 || length > MaximumTextLength)
            {
                throw new InvalidDataException(
                    "Axis command recovery text length is invalid.");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Axis command recovery text is incomplete.");
            }

            foreach (var value in bytes)
            {
                if (value < 0x20 || value > 0x7E)
                {
                    throw new InvalidDataException(
                        "Axis command recovery text is not printable ASCII.");
                }
            }

            return Encoding.ASCII.GetString(bytes);
        }

        private static bool ChecksumEquals(
            byte[] expected,
            byte[] actual,
            int actualOffset)
        {
            if (expected == null
                || expected.Length != ChecksumLength
                || actual == null
                || actualOffset < 0
                || actual.Length - actualOffset != ChecksumLength)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < ChecksumLength; index++)
            {
                difference |= expected[index] ^ actual[actualOffset + index];
            }

            return difference == 0;
        }

        private static bool ByteArraysEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                {
                    return false;
                }
            }

            return true;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(AxisCommandRecoveryJournal).FullName);
            }
        }
    }
}

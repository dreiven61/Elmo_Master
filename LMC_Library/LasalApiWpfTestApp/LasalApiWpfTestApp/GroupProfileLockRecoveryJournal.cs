using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum GroupProfileLockRecoveryState
    {
        ArmedBeforeDispatch = 1,
        RecoveryRequired = 2,
        Resolved = 3,
        AcceptedAwaitingProof = 4
    }

    internal sealed class GroupProfileLockRecoveryRecord
    {
        private const int MaximumGroupNameLength = 256;

        private readonly Guid identity;
        private readonly string endpointIp;
        private readonly int endpointPort;
        private readonly string groupName;
        private readonly ushort groupReference;
        private readonly uint diagnosticsBootId;
        private readonly uint mapRevision;
        private readonly bool expectedProfileLocked;
        private readonly GroupProfileLockRecoveryState state;
        private readonly DateTime createdUtc;
        private readonly DateTime updatedUtc;

        internal GroupProfileLockRecoveryRecord(
            Guid identity,
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            GroupProfileLockRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
            : this(
                identity,
                true,
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                state,
                createdUtc,
                updatedUtc)
        {
        }

        internal GroupProfileLockRecoveryRecord(
            Guid identity,
            bool expectedProfileLocked,
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            GroupProfileLockRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Group profile-lock recovery identity cannot be empty.",
                    "identity");
            }

            var normalizedEndpointIp = NormalizeEndpointIp(
                endpointIp,
                "endpointIp");
            if (endpointPort < 1 || endpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    "endpointPort",
                    "The endpoint port must be from 1 through 65535.");
            }

            ValidateGroupName(groupName, "groupName");
            if (groupReference == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "groupReference",
                    "The group reference must be non-zero.");
            }

            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Recovery identity requires a non-zero diagnostics BootId.");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "mapRevision",
                    "Recovery identity requires a non-zero map revision.");
            }

            ValidateState(state);
            ValidateTimestamps(createdUtc, updatedUtc);

            this.identity = identity;
            this.endpointIp = normalizedEndpointIp;
            this.endpointPort = endpointPort;
            this.groupName = groupName;
            this.groupReference = groupReference;
            this.diagnosticsBootId = diagnosticsBootId;
            this.mapRevision = mapRevision;
            this.expectedProfileLocked = expectedProfileLocked;
            this.state = state;
            this.createdUtc = createdUtc;
            this.updatedUtc = updatedUtc;
        }

        internal Guid Identity
        {
            get { return identity; }
        }

        internal string EndpointIp
        {
            get { return endpointIp; }
        }

        internal int EndpointPort
        {
            get { return endpointPort; }
        }

        internal string GroupName
        {
            get { return groupName; }
        }

        internal ushort GroupReference
        {
            get { return groupReference; }
        }

        internal uint DiagnosticsBootId
        {
            get { return diagnosticsBootId; }
        }

        internal uint MapRevision
        {
            get { return mapRevision; }
        }

        internal bool ExpectedProfileLocked
        {
            get { return expectedProfileLocked; }
        }

        internal GroupProfileLockRecoveryState State
        {
            get { return state; }
        }

        internal DateTime CreatedUtc
        {
            get { return createdUtc; }
        }

        internal DateTime UpdatedUtc
        {
            get { return updatedUtc; }
        }

        internal bool IsActive
        {
            get { return state != GroupProfileLockRecoveryState.Resolved; }
        }

        internal bool MatchesRecoveryIdentity(
            string candidateEndpointIp,
            int candidateEndpointPort,
            string candidateGroupName,
            ushort candidateGroupReference,
            uint candidateDiagnosticsBootId,
            uint candidateMapRevision)
        {
            return MatchesPhysicalRecoveryIdentity(
                    candidateEndpointIp,
                    candidateEndpointPort,
                    candidateGroupName,
                    candidateGroupReference,
                    candidateDiagnosticsBootId,
                    candidateMapRevision)
                && expectedProfileLocked;
        }

        private bool MatchesPhysicalRecoveryIdentity(
            string candidateEndpointIp,
            int candidateEndpointPort,
            string candidateGroupName,
            ushort candidateGroupReference,
            uint candidateDiagnosticsBootId,
            uint candidateMapRevision)
        {
            string normalizedCandidateEndpointIp;
            if (!TryNormalizeEndpointIp(
                    candidateEndpointIp,
                    out normalizedCandidateEndpointIp))
            {
                return false;
            }

            return string.Equals(
                    endpointIp,
                    normalizedCandidateEndpointIp,
                    StringComparison.Ordinal)
                && endpointPort == candidateEndpointPort
                && string.Equals(
                    groupName,
                    candidateGroupName,
                    StringComparison.Ordinal)
                && groupReference == candidateGroupReference
                && diagnosticsBootId == candidateDiagnosticsBootId
                && mapRevision == candidateMapRevision;
        }

        internal bool MatchesRecoveryIdentity(
            string candidateEndpointIp,
            int candidateEndpointPort,
            string candidateGroupName,
            ushort candidateGroupReference,
            uint candidateDiagnosticsBootId,
            uint candidateMapRevision,
            bool candidateExpectedProfileLocked)
        {
            return MatchesPhysicalRecoveryIdentity(
                    candidateEndpointIp,
                    candidateEndpointPort,
                    candidateGroupName,
                    candidateGroupReference,
                    candidateDiagnosticsBootId,
                    candidateMapRevision)
                && expectedProfileLocked
                    == candidateExpectedProfileLocked;
        }

        internal GroupProfileLockRecoveryRecord Copy()
        {
            return new GroupProfileLockRecoveryRecord(
                identity,
                expectedProfileLocked,
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                state,
                createdUtc,
                updatedUtc);
        }

        internal GroupProfileLockRecoveryRecord TransitionTo(
            GroupProfileLockRecoveryState nextState,
            DateTime nextUpdatedUtc)
        {
            if (!CanTransition(
                    state,
                    nextState,
                    expectedProfileLocked))
            {
                throw new InvalidOperationException(
                    "Group profile-lock recovery state cannot transition from "
                    + state
                    + " to "
                    + nextState
                    + ".");
            }

            if (nextUpdatedUtc.Kind != DateTimeKind.Utc
                || nextUpdatedUtc < updatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "nextUpdatedUtc",
                    "Recovery transition time must be UTC and cannot move backwards.");
            }

            return new GroupProfileLockRecoveryRecord(
                identity,
                expectedProfileLocked,
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                nextState,
                createdUtc,
                nextUpdatedUtc);
        }

        private static bool CanTransition(
            GroupProfileLockRecoveryState currentState,
            GroupProfileLockRecoveryState nextState,
            bool expectedProfileLocked)
        {
            if (currentState == nextState
                || currentState == GroupProfileLockRecoveryState.Resolved)
            {
                return false;
            }

            if (nextState == GroupProfileLockRecoveryState.Resolved)
            {
                return currentState
                        == GroupProfileLockRecoveryState.ArmedBeforeDispatch
                    || currentState
                        == GroupProfileLockRecoveryState.RecoveryRequired
                    || currentState
                        == GroupProfileLockRecoveryState.AcceptedAwaitingProof;
            }

            if (nextState
                == GroupProfileLockRecoveryState.AcceptedAwaitingProof)
            {
                return currentState
                        == GroupProfileLockRecoveryState.ArmedBeforeDispatch
                    || (!expectedProfileLocked
                        && currentState
                            == GroupProfileLockRecoveryState
                                .RecoveryRequired);
            }

            return nextState
                    == GroupProfileLockRecoveryState.RecoveryRequired
                && (currentState
                        == GroupProfileLockRecoveryState.ArmedBeforeDispatch
                    || currentState
                        == GroupProfileLockRecoveryState.AcceptedAwaitingProof);
        }

        private static void ValidateState(
            GroupProfileLockRecoveryState value)
        {
            if (value != GroupProfileLockRecoveryState.ArmedBeforeDispatch
                && value != GroupProfileLockRecoveryState.RecoveryRequired
                && value != GroupProfileLockRecoveryState.Resolved
                && value
                    != GroupProfileLockRecoveryState.AcceptedAwaitingProof)
            {
                throw new ArgumentOutOfRangeException("state");
            }
        }

        private static void ValidateTimestamps(
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (createdUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Recovery creation time must be UTC.",
                    "createdUtc");
            }

            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentException(
                    "Recovery update time must be UTC and cannot precede creation.",
                    "updatedUtc");
            }
        }

        private static void ValidateGroupName(
            string value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "The group name cannot be empty.",
                    parameterName);
            }

            if (!string.Equals(
                    value,
                    value.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The group name cannot have leading or trailing whitespace.",
                    parameterName);
            }

            if (value.Length > MaximumGroupNameLength)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The group name is too long.");
            }

            ValidateAscii(value, parameterName);
        }

        private static string NormalizeEndpointIp(
            string value,
            string parameterName)
        {
            string normalized;
            if (!TryNormalizeEndpointIp(value, out normalized))
            {
                throw new ArgumentException(
                    "The endpoint IP must be a valid IPv4 address.",
                    parameterName);
            }

            return normalized;
        }

        private static bool TryNormalizeEndpointIp(
            string value,
            out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(value)
                || !string.Equals(
                    value,
                    value.Trim(),
                    StringComparison.Ordinal))
            {
                return false;
            }

            IPAddress address;
            if (!IPAddress.TryParse(value, out address)
                || address.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }

            normalized = address.ToString();
            return true;
        }

        private static void ValidateAscii(
            string value,
            string parameterName)
        {
            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] > 0x7f)
                {
                    throw new ArgumentException(
                        "Recovery identity text must use 7-bit ASCII.",
                        parameterName);
                }
            }
        }
    }

    internal sealed class GroupProfileLockRecoveryJournal : IDisposable
    {
        internal const string JournalFileName = "journal.dat";
        internal const string LockFileName = "journal.lock";

        private const int LegacyFormatVersion = 1;
        private const int FormatVersion = 2;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 16384;
        private const int MaximumTextByteLength = 1024;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOGPL1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private GroupProfileLockRecoveryRecord currentRecord;
        private bool disposed;

        private GroupProfileLockRecoveryJournal(
            string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "A group profile-lock recovery journal directory is required.",
                    "requestedDirectoryPath");
            }

            directoryPath = Path.GetFullPath(requestedDirectoryPath);
            journalFilePath = Path.Combine(
                directoryPath,
                JournalFileName);

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

        internal string DirectoryPath
        {
            get { return directoryPath; }
        }

        internal string JournalFilePath
        {
            get { return journalFilePath; }
        }

        internal GroupProfileLockRecoveryRecord CurrentRecord
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
                    return currentRecord != null
                        && currentRecord.IsActive;
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

        internal GroupProfileLockRecoveryRecord ResolveOperatorRetirement(
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
                    "Group profile-lock retirement requires a durably committed ledger decision.");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                if (!committedDecision.MatchesSourceEvidence(
                    expectedEvidence))
                {
                    throw new InvalidOperationException(
                        "The committed retirement decision does not match the expected group profile-lock source evidence.");
                }

                var currentEvidence =
                    CaptureActiveRetirementEvidenceCore();
                if (!expectedEvidence.ExactSourceEquals(currentEvidence)
                    || !committedDecision.MatchesSourceEvidence(
                        currentEvidence))
                {
                    throw new InvalidOperationException(
                        "Group profile-lock recovery changed after operator confirmation; retirement was not applied.");
                }

                var resolved = currentRecord.TransitionTo(
                    GroupProfileLockRecoveryState.Resolved,
                    updatedUtc);
                PersistRecord(resolved);
                currentRecord = resolved;
                return resolved.Copy();
            }
        }

        internal static GroupProfileLockRecoveryJournal Open(
            string directoryPath)
        {
            return new GroupProfileLockRecoveryJournal(directoryPath);
        }

        internal static GroupProfileLockRecoveryJournal OpenDefault()
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
                "GroupProfileLockRecoveryJournal",
                "v1");
        }

        internal GroupProfileLockRecoveryRecord ArmBeforeDispatch(
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                true,
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal GroupProfileLockRecoveryRecord ArmBeforeDispatch(
            bool expectedProfileLocked,
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                Guid.NewGuid(),
                expectedProfileLocked,
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal GroupProfileLockRecoveryRecord ArmBeforeDispatch(
            Guid identity,
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                identity,
                true,
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal GroupProfileLockRecoveryRecord ArmBeforeDispatch(
            Guid identity,
            bool expectedProfileLocked,
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved group profile-lock recovery record already exists.");
                }

                if (currentRecord != null
                    && currentRecord.Identity == identity)
                {
                    throw new InvalidOperationException(
                        "A resolved group profile-lock recovery identity cannot be reused.");
                }

                var armed = new GroupProfileLockRecoveryRecord(
                    identity,
                    expectedProfileLocked,
                    endpointIp,
                    endpointPort,
                    groupName,
                    groupReference,
                    diagnosticsBootId,
                    mapRevision,
                    GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(armed);
                currentRecord = armed;
                return armed.Copy();
            }
        }

        internal GroupProfileLockRecoveryRecord
            ReplaceActiveLockWithUnlockBeforeDispatch(
                Guid activeIdentity,
                string endpointIp,
                int endpointPort,
                string groupName,
                ushort groupReference,
                uint diagnosticsBootId,
                uint mapRevision,
                DateTime createdUtc)
        {
            return ReplaceActiveLockWithUnlockBeforeDispatch(
                activeIdentity,
                Guid.NewGuid(),
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal GroupProfileLockRecoveryRecord
            ReplaceActiveLockWithUnlockBeforeDispatch(
                Guid activeIdentity,
                Guid replacementIdentity,
                string endpointIp,
                int endpointPort,
                string groupName,
                ushort groupReference,
                uint diagnosticsBootId,
                uint mapRevision,
                DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(activeIdentity);
                if (!current.IsActive
                    || !current.ExpectedProfileLocked
                    || !current.MatchesRecoveryIdentity(
                        endpointIp,
                        endpointPort,
                        groupName,
                        groupReference,
                        diagnosticsBootId,
                        mapRevision,
                        true))
                {
                    throw new InvalidOperationException(
                        "Only the exact active profile-lock recovery record can be atomically replaced by an unlock record.");
                }

                if (replacementIdentity == Guid.Empty
                    || replacementIdentity == current.Identity)
                {
                    throw new ArgumentException(
                        "The replacement recovery identity must be non-empty and different from the active lock identity.",
                        "replacementIdentity");
                }

                if (createdUtc.Kind != DateTimeKind.Utc
                    || createdUtc < current.UpdatedUtc)
                {
                    throw new ArgumentOutOfRangeException(
                        "createdUtc",
                        "The unlock replacement time must be UTC and cannot precede the active record update.");
                }

                var replacement = new GroupProfileLockRecoveryRecord(
                    replacementIdentity,
                    false,
                    endpointIp,
                    endpointPort,
                    groupName,
                    groupReference,
                    diagnosticsBootId,
                    mapRevision,
                    GroupProfileLockRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(replacement);
                currentRecord = replacement;
                return replacement.Copy();
            }
        }

        internal GroupProfileLockRecoveryRecord PromoteToRecoveryRequired(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                GroupProfileLockRecoveryState.RecoveryRequired,
                updatedUtc);
        }

        internal GroupProfileLockRecoveryRecord MarkAccepted(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                GroupProfileLockRecoveryState.AcceptedAwaitingProof,
                updatedUtc);
        }

        internal GroupProfileLockRecoveryRecord Resolve(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                GroupProfileLockRecoveryState.Resolved,
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

        private GroupProfileLockRecoveryRecord Transition(
            Guid identity,
            GroupProfileLockRecoveryState state,
            DateTime updatedUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var transitioned = current.TransitionTo(
                    state,
                    updatedUtc);
                PersistRecord(transitioned);
                currentRecord = transitioned;
                return transitioned.Copy();
            }
        }

        private GroupProfileLockRecoveryRecord RequireCurrentRecord(
            Guid identity)
        {
            if (currentRecord == null)
            {
                throw new InvalidOperationException(
                    "No group profile-lock recovery record exists.");
            }

            if (identity == Guid.Empty
                || currentRecord.Identity != identity)
            {
                throw new InvalidOperationException(
                    "Group profile-lock recovery transition identity does not match the durable record.");
            }

            return currentRecord;
        }

        private RecoveryJournalSourceEvidence
            CaptureActiveRetirementEvidenceCore()
        {
            if (currentRecord == null || !currentRecord.IsActive)
            {
                throw new InvalidOperationException(
                    "No active group profile-lock recovery record exists for operator retirement.");
            }

            var originalBytes = ReadRetirementSourceBytes();
            var diskRecord = DeserializeRecord(originalBytes);
            if (!RecordsEqual(currentRecord, diskRecord))
            {
                throw new InvalidDataException(
                    "Group profile-lock recovery memory state does not match the exact durable source bytes.");
            }

            return new RecoveryJournalSourceEvidence(
                RecoveryRecordOwner.GroupProfileLock,
                diskRecord.Identity,
                (int)diskRecord.State,
                diskRecord.CreatedUtc,
                diskRecord.UpdatedUtc,
                diskRecord.EndpointIp,
                diskRecord.EndpointPort,
                diskRecord.DiagnosticsBootId,
                diskRecord.MapRevision,
                "Group",
                diskRecord.GroupName,
                diskRecord.GroupReference,
                diskRecord.ExpectedProfileLocked ? "Lock" : "Unlock",
                "ExpectedProfileLocked="
                    + (diskRecord.ExpectedProfileLocked ? "true" : "false"),
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
                        "Group profile-lock recovery source length is invalid.");
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
                            "Group profile-lock recovery source is truncated.");
                    }
                    offset += read;
                }
                return bytes;
            }
        }

        private static bool RecordsEqual(
            GroupProfileLockRecoveryRecord left,
            GroupProfileLockRecoveryRecord right)
        {
            return left != null
                && right != null
                && left.Identity == right.Identity
                && string.Equals(
                    left.EndpointIp,
                    right.EndpointIp,
                    StringComparison.Ordinal)
                && left.EndpointPort == right.EndpointPort
                && string.Equals(
                    left.GroupName,
                    right.GroupName,
                    StringComparison.Ordinal)
                && left.GroupReference == right.GroupReference
                && left.DiagnosticsBootId == right.DiagnosticsBootId
                && left.MapRevision == right.MapRevision
                && left.ExpectedProfileLocked
                    == right.ExpectedProfileLocked
                && left.State == right.State
                && left.CreatedUtc == right.CreatedUtc
                && left.UpdatedUtc == right.UpdatedUtc;
        }

        private void PersistRecord(GroupProfileLockRecoveryRecord record)
        {
            var bytes = SerializeRecord(record);
            var temporaryPath = Path.Combine(
                directoryPath,
                JournalFileName
                    + "."
                    + Guid.NewGuid().ToString("N")
                    + ".tmp");
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
                    File.Replace(
                        temporaryPath,
                        journalFilePath,
                        null,
                        true);
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
                        // Preserve the primary persistence failure.
                    }
                }
            }
        }

        private static GroupProfileLockRecoveryRecord LoadRecord(
            string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan))
            {
                if (stream.Length < Magic.Length + 8 + ChecksumLength
                    || stream.Length > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery journal length is invalid.");
                }

                var bytes = new byte[(int)stream.Length];
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
                            "Group profile-lock recovery journal is incomplete.");
                    }

                    offset += read;
                }

                return DeserializeRecord(bytes);
            }
        }

        private static byte[] SerializeRecord(
            GroupProfileLockRecoveryRecord record)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    payloadStream,
                    Encoding.ASCII,
                    true))
                {
                    writer.Write(record.Identity.ToByteArray());
                    writer.Write((int)record.State);
                    writer.Write(record.ExpectedProfileLocked);
                    writer.Write(record.CreatedUtc.Ticks);
                    writer.Write(record.UpdatedUtc.Ticks);
                    writer.Write(record.DiagnosticsBootId);
                    writer.Write(record.MapRevision);
                    writer.Write(record.EndpointPort);
                    writer.Write(record.GroupReference);
                    WriteText(writer, record.EndpointIp);
                    WriteText(writer, record.GroupName);
                    writer.Flush();
                }

                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var fileStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    fileStream,
                    Encoding.ASCII,
                    true))
                {
                    writer.Write(Magic);
                    writer.Write(FormatVersion);
                    writer.Write(payload.Length);
                    writer.Write(payload);
                    writer.Flush();
                }

                prefix = fileStream.ToArray();
            }

            byte[] checksum;
            using (var sha256 = SHA256.Create())
            {
                checksum = sha256.ComputeHash(prefix);
            }

            var result = new byte[prefix.Length + checksum.Length];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(
                checksum,
                0,
                result,
                prefix.Length,
                checksum.Length);
            return result;
        }

        private static GroupProfileLockRecoveryRecord DeserializeRecord(
            byte[] bytes)
        {
            try
            {
                return DeserializeRecordCore(bytes);
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (EndOfStreamException error)
            {
                throw InvalidRecord(error);
            }
            catch (ArgumentException error)
            {
                throw InvalidRecord(error);
            }
            catch (OverflowException error)
            {
                throw InvalidRecord(error);
            }
        }

        private static GroupProfileLockRecoveryRecord DeserializeRecordCore(
            byte[] bytes)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Group profile-lock recovery journal length is invalid.");
            }

            var checksumOffset = bytes.Length - ChecksumLength;
            byte[] computedChecksum;
            using (var sha256 = SHA256.Create())
            {
                computedChecksum = sha256.ComputeHash(
                    bytes,
                    0,
                    checksumOffset);
            }

            if (!ChecksumEquals(
                    computedChecksum,
                    bytes,
                    checksumOffset))
            {
                throw new InvalidDataException(
                    "Group profile-lock recovery journal checksum is invalid.");
            }

            using (var fileStream = new MemoryStream(
                bytes,
                0,
                checksumOffset,
                false))
            using (var reader = new BinaryReader(
                fileStream,
                Encoding.ASCII,
                true))
            {
                var magic = reader.ReadBytes(Magic.Length);
                if (!ByteArraysEqual(Magic, magic))
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery journal magic is invalid.");
                }

                var version = reader.ReadInt32();
                if (version != LegacyFormatVersion
                    && version != FormatVersion)
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery journal version is unsupported.");
                }

                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength
                        != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery journal payload length is invalid.");
                }

                var payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || fileStream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery journal payload is incomplete.");
                }

                return DeserializePayload(payload, version);
            }
        }

        private static GroupProfileLockRecoveryRecord DeserializePayload(
            byte[] payload,
            int formatVersion)
        {
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(
                stream,
                Encoding.ASCII,
                true))
            {
                var identityBytes = reader.ReadBytes(16);
                if (identityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery identity is incomplete.");
                }

                var identity = new Guid(identityBytes);
                var state = (GroupProfileLockRecoveryState)reader.ReadInt32();
                var expectedProfileLocked = formatVersion
                        == LegacyFormatVersion
                    ? true
                    : ReadExpectedProfileLocked(reader);
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var diagnosticsBootId = reader.ReadUInt32();
                var mapRevision = reader.ReadUInt32();
                var endpointPort = reader.ReadInt32();
                var groupReference = reader.ReadUInt16();
                var endpointIp = ReadText(reader);
                var groupName = ReadText(reader);
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery journal has trailing payload data.");
                }

                return new GroupProfileLockRecoveryRecord(
                    identity,
                    expectedProfileLocked,
                    endpointIp,
                    endpointPort,
                    groupName,
                    groupReference,
                    diagnosticsBootId,
                    mapRevision,
                    state,
                    createdUtc,
                    updatedUtc);
            }
        }

        private static bool ReadExpectedProfileLocked(BinaryReader reader)
        {
            var value = reader.ReadByte();
            if (value > 1)
            {
                throw new InvalidDataException(
                    "Group profile-lock recovery direction is invalid.");
            }

            return value != 0;
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length <= 0
                || bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Group profile-lock recovery text encoding is invalid.");
            }

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length <= 0 || length > MaximumTextByteLength)
            {
                throw new InvalidDataException(
                    "Group profile-lock recovery text length is invalid.");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Group profile-lock recovery text is incomplete.");
            }

            for (var index = 0; index < bytes.Length; index++)
            {
                if (bytes[index] > 0x7f)
                {
                    throw new InvalidDataException(
                        "Group profile-lock recovery text is not 7-bit ASCII.");
                }
            }

            return Encoding.ASCII.GetString(bytes);
        }

        private static InvalidDataException InvalidRecord(Exception error)
        {
            return new InvalidDataException(
                "Group profile-lock recovery journal record is invalid.",
                error);
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

            var difference = 0;
            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    "GroupProfileLockRecoveryJournal");
            }
        }
    }
}

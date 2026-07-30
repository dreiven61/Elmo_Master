using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum GroupPowerRecoveryState
    {
        ArmedBeforeDispatch = 1,
        AcceptedAwaitingProof = 2,
        RecoveryRequired = 3,
        Resolved = 4
    }

    internal sealed class GroupPowerRecoveryRecord
    {
        private const int MaximumGroupNameLength = 256;

        private readonly Guid identity;
        private readonly bool expectedPowerOn;
        private readonly string endpointIp;
        private readonly int endpointPort;
        private readonly string groupName;
        private readonly ushort groupReference;
        private readonly uint diagnosticsBootId;
        private readonly uint mapRevision;
        private readonly GroupPowerRecoveryState state;
        private readonly DateTime createdUtc;
        private readonly DateTime updatedUtc;

        internal GroupPowerRecoveryRecord(
            Guid identity,
            bool expectedPowerOn,
            string endpointIp,
            int endpointPort,
            string groupName,
            ushort groupReference,
            uint diagnosticsBootId,
            uint mapRevision,
            GroupPowerRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Group Power recovery identity cannot be empty.",
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
            this.expectedPowerOn = expectedPowerOn;
            this.endpointIp = normalizedEndpointIp;
            this.endpointPort = endpointPort;
            this.groupName = groupName;
            this.groupReference = groupReference;
            this.diagnosticsBootId = diagnosticsBootId;
            this.mapRevision = mapRevision;
            this.state = state;
            this.createdUtc = createdUtc;
            this.updatedUtc = updatedUtc;
        }

        internal Guid Identity
        {
            get { return identity; }
        }

        internal bool ExpectedPowerOn
        {
            get { return expectedPowerOn; }
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

        internal GroupPowerRecoveryState State
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
            get { return state != GroupPowerRecoveryState.Resolved; }
        }

        internal bool MatchesEndpoint(
            string candidateEndpointIp,
            int candidateEndpointPort)
        {
            string normalizedCandidateEndpointIp;
            return TryNormalizeEndpointIp(
                    candidateEndpointIp,
                    out normalizedCandidateEndpointIp)
                && string.Equals(
                    endpointIp,
                    normalizedCandidateEndpointIp,
                    StringComparison.Ordinal)
                && endpointPort == candidateEndpointPort;
        }

        internal bool MatchesRecoveryIdentity(
            string candidateEndpointIp,
            int candidateEndpointPort,
            string candidateGroupName,
            ushort candidateGroupReference,
            uint candidateDiagnosticsBootId,
            uint candidateMapRevision,
            bool candidateExpectedPowerOn)
        {
            return MatchesEndpoint(
                    candidateEndpointIp,
                    candidateEndpointPort)
                && string.Equals(
                    groupName,
                    candidateGroupName,
                    StringComparison.Ordinal)
                && groupReference == candidateGroupReference
                && diagnosticsBootId == candidateDiagnosticsBootId
                && mapRevision == candidateMapRevision
                && expectedPowerOn == candidateExpectedPowerOn;
        }

        internal GroupPowerRecoveryRecord Copy()
        {
            return new GroupPowerRecoveryRecord(
                identity,
                expectedPowerOn,
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

        internal GroupPowerRecoveryRecord TransitionTo(
            GroupPowerRecoveryState nextState,
            DateTime nextUpdatedUtc)
        {
            if (!CanTransition(state, nextState, expectedPowerOn))
            {
                throw new InvalidOperationException(
                    "Group Power recovery state cannot transition from "
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

            return new GroupPowerRecoveryRecord(
                identity,
                expectedPowerOn,
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
            GroupPowerRecoveryState currentState,
            GroupPowerRecoveryState nextState,
            bool expectedPowerOn)
        {
            if (currentState == nextState
                || currentState == GroupPowerRecoveryState.Resolved)
            {
                return false;
            }

            if (nextState == GroupPowerRecoveryState.Resolved)
            {
                return true;
            }

            if (nextState == GroupPowerRecoveryState.RecoveryRequired)
            {
                return currentState
                        == GroupPowerRecoveryState.ArmedBeforeDispatch
                    || currentState
                        == GroupPowerRecoveryState.AcceptedAwaitingProof;
            }

            if (nextState
                != GroupPowerRecoveryState.AcceptedAwaitingProof)
            {
                return false;
            }

            return currentState
                    == GroupPowerRecoveryState.ArmedBeforeDispatch
                || (!expectedPowerOn
                    && currentState
                        == GroupPowerRecoveryState.RecoveryRequired);
        }

        private static void ValidateState(GroupPowerRecoveryState value)
        {
            if (value != GroupPowerRecoveryState.ArmedBeforeDispatch
                && value != GroupPowerRecoveryState.AcceptedAwaitingProof
                && value != GroupPowerRecoveryState.RecoveryRequired
                && value != GroupPowerRecoveryState.Resolved)
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

            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] < 0x20 || value[index] > 0x7e)
                {
                    throw new ArgumentException(
                        "The group name must use printable 7-bit ASCII.",
                        parameterName);
                }
            }
        }

        private static string NormalizeEndpointIp(
            string value,
            string parameterName)
        {
            string normalized;
            if (!TryNormalizeEndpointIp(value, out normalized))
            {
                throw new ArgumentException(
                    "The endpoint IP must be a valid IPv4 literal without surrounding whitespace.",
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
    }

    internal sealed class GroupPowerRecoveryJournal : IDisposable
    {
        internal const string JournalFileName = "journal.dat";
        internal const string LockFileName = "journal.lock";

        private const int FormatVersion = 1;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 16384;
        private const int MaximumTextByteLength = 1024;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOGPW1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private GroupPowerRecoveryRecord currentRecord;
        private bool disposed;

        private GroupPowerRecoveryJournal(string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "A Group Power recovery journal directory is required.",
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

        internal GroupPowerRecoveryRecord CurrentRecord
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

        internal static GroupPowerRecoveryJournal Open(
            string directoryPath)
        {
            return new GroupPowerRecoveryJournal(directoryPath);
        }

        internal static GroupPowerRecoveryJournal OpenDefault()
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
                "GroupPowerRecoveryJournal",
                "v1");
        }

        internal GroupPowerRecoveryRecord ArmBeforeDispatch(
            bool expectedPowerOn,
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
                expectedPowerOn,
                endpointIp,
                endpointPort,
                groupName,
                groupReference,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal GroupPowerRecoveryRecord ArmBeforeDispatch(
            Guid identity,
            bool expectedPowerOn,
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
                        "An unresolved Group Power recovery record already exists.");
                }

                if (currentRecord != null
                    && currentRecord.Identity == identity)
                {
                    throw new InvalidOperationException(
                        "A resolved Group Power recovery identity cannot be reused.");
                }

                var armed = new GroupPowerRecoveryRecord(
                    identity,
                    expectedPowerOn,
                    endpointIp,
                    endpointPort,
                    groupName,
                    groupReference,
                    diagnosticsBootId,
                    mapRevision,
                    GroupPowerRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(armed);
                currentRecord = armed;
                return armed.Copy();
            }
        }

        internal GroupPowerRecoveryRecord MarkAccepted(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                GroupPowerRecoveryState.AcceptedAwaitingProof,
                updatedUtc);
        }

        internal GroupPowerRecoveryRecord
            ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                Guid oldIdentity,
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
                var current = RequireCurrentRecord(oldIdentity);
                if (!current.IsActive
                    || !current.ExpectedPowerOn
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
                        "Only the exact active Group Power On recovery record can be atomically replaced by Power Off.");
                }

                if (createdUtc.Kind != DateTimeKind.Utc
                    || createdUtc < current.UpdatedUtc)
                {
                    throw new ArgumentOutOfRangeException(
                        "createdUtc",
                        "The Power Off replacement time must be UTC and cannot precede the active record update.");
                }

                var replacement = new GroupPowerRecoveryRecord(
                    Guid.NewGuid(),
                    false,
                    endpointIp,
                    endpointPort,
                    groupName,
                    groupReference,
                    diagnosticsBootId,
                    mapRevision,
                    GroupPowerRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(replacement);
                currentRecord = replacement;
                return replacement.Copy();
            }
        }

        internal GroupPowerRecoveryRecord PromoteToRecoveryRequired(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                GroupPowerRecoveryState.RecoveryRequired,
                updatedUtc);
        }

        internal GroupPowerRecoveryRecord Resolve(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                GroupPowerRecoveryState.Resolved,
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

        private GroupPowerRecoveryRecord Transition(
            Guid identity,
            GroupPowerRecoveryState nextState,
            DateTime updatedUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var transitioned = current.TransitionTo(
                    nextState,
                    updatedUtc);
                PersistRecord(transitioned);
                currentRecord = transitioned;
                return transitioned.Copy();
            }
        }

        private GroupPowerRecoveryRecord RequireCurrentRecord(
            Guid identity)
        {
            if (currentRecord == null)
            {
                throw new InvalidOperationException(
                    "No Group Power recovery record exists.");
            }

            if (identity == Guid.Empty
                || currentRecord.Identity != identity)
            {
                throw new InvalidOperationException(
                    "Group Power recovery transition identity does not match the durable record.");
            }

            return currentRecord;
        }

        private void PersistRecord(GroupPowerRecoveryRecord record)
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

        private static GroupPowerRecoveryRecord LoadRecord(string path)
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
                var fileLength = stream.Length;
                if (fileLength < Magic.Length + 8 + ChecksumLength
                    || fileLength > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Group Power recovery journal length is invalid.");
                }

                var bytes = new byte[checked((int)fileLength)];
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
                            "Group Power recovery journal is truncated.");
                    }

                    offset += read;
                }

                return DeserializeRecord(bytes);
            }
        }

        private static byte[] SerializeRecord(
            GroupPowerRecoveryRecord record)
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
                    writer.Write(record.ExpectedPowerOn ? (byte)1 : (byte)0);
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

        private static GroupPowerRecoveryRecord DeserializeRecord(
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

        private static GroupPowerRecoveryRecord DeserializeRecordCore(
            byte[] bytes)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Group Power recovery journal length is invalid.");
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
                    "Group Power recovery journal checksum is invalid.");
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
                        "Group Power recovery journal magic is invalid.");
                }

                if (reader.ReadInt32() != FormatVersion)
                {
                    throw new InvalidDataException(
                        "Group Power recovery journal version is unsupported.");
                }

                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength
                        != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Group Power recovery journal payload length is invalid.");
                }

                var payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || fileStream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Group Power recovery journal payload is incomplete.");
                }

                return DeserializePayload(payload);
            }
        }

        private static GroupPowerRecoveryRecord DeserializePayload(
            byte[] payload)
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
                        "Group Power recovery identity is incomplete.");
                }

                var identity = new Guid(identityBytes);
                var state = (GroupPowerRecoveryState)reader.ReadInt32();
                var expectedPowerOnValue = reader.ReadByte();
                if (expectedPowerOnValue > 1)
                {
                    throw new InvalidDataException(
                        "Group Power recovery direction is invalid.");
                }

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
                        "Group Power recovery journal has trailing payload data.");
                }

                var record = new GroupPowerRecoveryRecord(
                    identity,
                    expectedPowerOnValue == 1,
                    endpointIp,
                    endpointPort,
                    groupName,
                    groupReference,
                    diagnosticsBootId,
                    mapRevision,
                    state,
                    createdUtc,
                    updatedUtc);
                if (!string.Equals(
                        endpointIp,
                        record.EndpointIp,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Group Power recovery endpoint is not canonical.");
                }

                return record;
            }
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length <= 0
                || bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Group Power recovery text encoding is invalid.");
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
                    "Group Power recovery text length is invalid.");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Group Power recovery text is incomplete.");
            }

            for (var index = 0; index < bytes.Length; index++)
            {
                if (bytes[index] < 0x20 || bytes[index] > 0x7e)
                {
                    throw new InvalidDataException(
                        "Group Power recovery text is not printable 7-bit ASCII.");
                }
            }

            return Encoding.ASCII.GetString(bytes);
        }

        private static InvalidDataException InvalidRecord(Exception error)
        {
            return new InvalidDataException(
                "Group Power recovery journal record is invalid.",
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
                    "GroupPowerRecoveryJournal");
            }
        }
    }
}

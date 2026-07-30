using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum MotionUncertaintyTargetKind
    {
        Axis = 1,
        Group = 2
    }

    internal enum MotionUncertaintyState
    {
        ArmedBeforeDispatch = 1,
        RecoveryRequired = 2,
        Resolved = 3
    }

    internal sealed class MotionUncertaintyRecord
    {
        private const int MaximumTargetNameLength = 256;
        private const int MaximumOperationLength = 128;

        private readonly Guid identity;
        private readonly string endpointIp;
        private readonly int endpointPort;
        private readonly MotionUncertaintyTargetKind targetKind;
        private readonly string targetName;
        private readonly ushort targetReference;
        private readonly string operation;
        private readonly uint diagnosticsBootId;
        private readonly uint mapRevision;
        private readonly MotionUncertaintyState state;
        private readonly DateTime createdUtc;
        private readonly DateTime updatedUtc;

        internal MotionUncertaintyRecord(
            Guid identity,
            string endpointIp,
            int endpointPort,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            uint diagnosticsBootId,
            uint mapRevision,
            MotionUncertaintyState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Motion uncertainty identity cannot be empty.",
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

            ValidateTargetKind(targetKind);
            ValidateText(
                targetName,
                MaximumTargetNameLength,
                "targetName");
            if (targetReference == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "targetReference",
                    "The target reference must be non-zero.");
            }

            ValidateText(
                operation,
                MaximumOperationLength,
                "operation");
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Motion recovery identity requires a non-zero diagnostics BootId.");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "mapRevision",
                    "Motion recovery identity requires a non-zero map revision.");
            }

            ValidateState(state);
            ValidateTimestamps(createdUtc, updatedUtc);

            this.identity = identity;
            this.endpointIp = normalizedEndpointIp;
            this.endpointPort = endpointPort;
            this.targetKind = targetKind;
            this.targetName = targetName;
            this.targetReference = targetReference;
            this.operation = operation;
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

        internal string EndpointIp
        {
            get { return endpointIp; }
        }

        internal int EndpointPort
        {
            get { return endpointPort; }
        }

        internal MotionUncertaintyTargetKind TargetKind
        {
            get { return targetKind; }
        }

        internal string TargetName
        {
            get { return targetName; }
        }

        internal ushort TargetReference
        {
            get { return targetReference; }
        }

        internal string Operation
        {
            get { return operation; }
        }

        internal uint DiagnosticsBootId
        {
            get { return diagnosticsBootId; }
        }

        internal uint MapRevision
        {
            get { return mapRevision; }
        }

        internal MotionUncertaintyState State
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
            get { return state != MotionUncertaintyState.Resolved; }
        }

        internal bool MatchesRecoveryIdentity(
            string candidateEndpointIp,
            int candidateEndpointPort,
            MotionUncertaintyTargetKind candidateTargetKind,
            string candidateTargetName,
            ushort candidateTargetReference,
            string candidateOperation,
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
                && targetKind == candidateTargetKind
                && string.Equals(
                    targetName,
                    candidateTargetName,
                    StringComparison.Ordinal)
                && targetReference == candidateTargetReference
                && string.Equals(
                    operation,
                    candidateOperation,
                    StringComparison.Ordinal)
                && diagnosticsBootId == candidateDiagnosticsBootId
                && mapRevision == candidateMapRevision;
        }

        internal MotionUncertaintyRecord Copy()
        {
            return new MotionUncertaintyRecord(
                identity,
                endpointIp,
                endpointPort,
                targetKind,
                targetName,
                targetReference,
                operation,
                diagnosticsBootId,
                mapRevision,
                state,
                createdUtc,
                updatedUtc);
        }

        internal MotionUncertaintyRecord TransitionTo(
            MotionUncertaintyState nextState,
            DateTime nextUpdatedUtc)
        {
            if (!CanTransition(state, nextState))
            {
                throw new InvalidOperationException(
                    "Motion uncertainty state cannot transition from "
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
                    "Motion uncertainty transition time must be UTC and cannot move backwards.");
            }

            return new MotionUncertaintyRecord(
                identity,
                endpointIp,
                endpointPort,
                targetKind,
                targetName,
                targetReference,
                operation,
                diagnosticsBootId,
                mapRevision,
                nextState,
                createdUtc,
                nextUpdatedUtc);
        }

        private static bool CanTransition(
            MotionUncertaintyState currentState,
            MotionUncertaintyState nextState)
        {
            if (currentState == nextState
                || currentState == MotionUncertaintyState.Resolved)
            {
                return false;
            }

            if (nextState == MotionUncertaintyState.Resolved)
            {
                return currentState
                        == MotionUncertaintyState.ArmedBeforeDispatch
                    || currentState
                        == MotionUncertaintyState.RecoveryRequired;
            }

            return currentState
                    == MotionUncertaintyState.ArmedBeforeDispatch
                && nextState
                    == MotionUncertaintyState.RecoveryRequired;
        }

        private static void ValidateTargetKind(
            MotionUncertaintyTargetKind value)
        {
            if (value != MotionUncertaintyTargetKind.Axis
                && value != MotionUncertaintyTargetKind.Group)
            {
                throw new ArgumentOutOfRangeException("targetKind");
            }
        }

        private static void ValidateState(MotionUncertaintyState value)
        {
            if (value != MotionUncertaintyState.ArmedBeforeDispatch
                && value != MotionUncertaintyState.RecoveryRequired
                && value != MotionUncertaintyState.Resolved)
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
                    "Motion uncertainty creation time must be UTC.",
                    "createdUtc");
            }

            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentException(
                    "Motion uncertainty update time must be UTC and cannot precede creation.",
                    "updatedUtc");
            }
        }

        private static void ValidateText(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Motion recovery identity text cannot be empty.",
                    parameterName);
            }

            if (!string.Equals(
                    value,
                    value.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Motion recovery identity text cannot have leading or trailing whitespace.",
                    parameterName);
            }

            if (value.Length > maximumLength)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Motion recovery identity text is too long.");
            }

            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] > 0x7f)
                {
                    throw new ArgumentException(
                        "Motion recovery identity text must use 7-bit ASCII.",
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
    }

    internal sealed class MotionUncertaintyJournal : IDisposable
    {
        internal const string JournalFileName = "journal.dat";
        internal const string LockFileName = "journal.lock";

        private const int FormatVersion = 1;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 4096;
        private const int MaximumTextByteLength = 1024;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOMUJ1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private MotionUncertaintyRecord currentRecord;
        private bool disposed;

        private MotionUncertaintyJournal(string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "A motion uncertainty journal directory is required.",
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
                CleanupStaleTemporaryFiles();
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

        internal MotionUncertaintyRecord CurrentRecord
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

        internal static MotionUncertaintyJournal Open(string directoryPath)
        {
            return new MotionUncertaintyJournal(directoryPath);
        }

        internal static MotionUncertaintyJournal OpenDefault()
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
                "MotionUncertaintyJournal",
                "v1");
        }

        internal MotionUncertaintyRecord ArmBeforeDispatch(
            string endpointIp,
            int endpointPort,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                Guid.NewGuid(),
                endpointIp,
                endpointPort,
                targetKind,
                targetName,
                targetReference,
                operation,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal MotionUncertaintyRecord ArmBeforeDispatch(
            Guid identity,
            string endpointIp,
            int endpointPort,
            MotionUncertaintyTargetKind targetKind,
            string targetName,
            ushort targetReference,
            string operation,
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
                        "An unresolved motion uncertainty record already exists.");
                }

                if (currentRecord != null
                    && currentRecord.Identity == identity)
                {
                    throw new InvalidOperationException(
                        "A resolved motion uncertainty identity cannot be reused.");
                }

                var armed = new MotionUncertaintyRecord(
                    identity,
                    endpointIp,
                    endpointPort,
                    targetKind,
                    targetName,
                    targetReference,
                    operation,
                    diagnosticsBootId,
                    mapRevision,
                    MotionUncertaintyState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(armed);
                currentRecord = armed;
                return armed.Copy();
            }
        }

        internal MotionUncertaintyRecord PromoteToRecoveryRequired(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                MotionUncertaintyState.RecoveryRequired,
                updatedUtc);
        }

        internal MotionUncertaintyRecord Resolve(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                MotionUncertaintyState.Resolved,
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

        private MotionUncertaintyRecord Transition(
            Guid identity,
            MotionUncertaintyState state,
            DateTime updatedUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var transitioned = current.TransitionTo(state, updatedUtc);
                PersistRecord(transitioned);
                currentRecord = transitioned;
                return transitioned.Copy();
            }
        }

        private MotionUncertaintyRecord RequireCurrentRecord(Guid identity)
        {
            if (currentRecord == null)
            {
                throw new InvalidOperationException(
                    "No motion uncertainty record exists.");
            }

            if (identity == Guid.Empty
                || currentRecord.Identity != identity)
            {
                throw new InvalidOperationException(
                    "Motion uncertainty transition identity does not match the durable record.");
            }

            return currentRecord;
        }

        private void CleanupStaleTemporaryFiles()
        {
            var paths = Directory.GetFiles(
                directoryPath,
                JournalFileName + ".*.tmp",
                SearchOption.TopDirectoryOnly);
            for (var index = 0; index < paths.Length; index++)
            {
                File.Delete(paths[index]);
            }
        }

        private void PersistRecord(MotionUncertaintyRecord record)
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

        private static MotionUncertaintyRecord LoadRecord(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            return DeserializeRecord(File.ReadAllBytes(path));
        }

        private static byte[] SerializeRecord(
            MotionUncertaintyRecord record)
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
                    writer.Write((int)record.TargetKind);
                    writer.Write(record.CreatedUtc.Ticks);
                    writer.Write(record.UpdatedUtc.Ticks);
                    writer.Write(record.DiagnosticsBootId);
                    writer.Write(record.MapRevision);
                    writer.Write(record.EndpointPort);
                    writer.Write(record.TargetReference);
                    WriteText(writer, record.EndpointIp);
                    WriteText(writer, record.TargetName);
                    WriteText(writer, record.Operation);
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
            if (result.Length > MaximumFileLength)
            {
                throw new InvalidOperationException(
                    "Motion uncertainty journal exceeds its bounded format.");
            }

            return result;
        }

        private static MotionUncertaintyRecord DeserializeRecord(
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
            catch (NotSupportedException)
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

        private static MotionUncertaintyRecord DeserializeRecordCore(
            byte[] bytes)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Motion uncertainty journal length is invalid.");
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
                    "Motion uncertainty journal checksum is invalid.");
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
                        "Motion uncertainty journal magic is invalid.");
                }

                var version = reader.ReadInt32();
                if (version != FormatVersion)
                {
                    throw new NotSupportedException(
                        "Motion uncertainty journal version is unsupported: "
                        + version
                        + ".");
                }

                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength
                        != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Motion uncertainty journal payload length is invalid.");
                }

                var payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || fileStream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Motion uncertainty journal payload is incomplete.");
                }

                return DeserializePayload(payload);
            }
        }

        private static MotionUncertaintyRecord DeserializePayload(
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
                        "Motion uncertainty identity is incomplete.");
                }

                var identity = new Guid(identityBytes);
                var state = (MotionUncertaintyState)reader.ReadInt32();
                var targetKind =
                    (MotionUncertaintyTargetKind)reader.ReadInt32();
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var diagnosticsBootId = reader.ReadUInt32();
                var mapRevision = reader.ReadUInt32();
                var endpointPort = reader.ReadInt32();
                var targetReference = reader.ReadUInt16();
                var endpointIp = ReadText(reader);
                var targetName = ReadText(reader);
                var operation = ReadText(reader);
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Motion uncertainty journal has trailing payload data.");
                }

                return new MotionUncertaintyRecord(
                    identity,
                    endpointIp,
                    endpointPort,
                    targetKind,
                    targetName,
                    targetReference,
                    operation,
                    diagnosticsBootId,
                    mapRevision,
                    state,
                    createdUtc,
                    updatedUtc);
            }
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            if (bytes.Length <= 0
                || bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Motion uncertainty text encoding is invalid.");
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
                    "Motion uncertainty text length is invalid.");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Motion uncertainty text is incomplete.");
            }

            for (var index = 0; index < bytes.Length; index++)
            {
                if (bytes[index] > 0x7f)
                {
                    throw new InvalidDataException(
                        "Motion uncertainty text is not 7-bit ASCII.");
                }
            }

            return Encoding.ASCII.GetString(bytes);
        }

        private static InvalidDataException InvalidRecord(Exception error)
        {
            return new InvalidDataException(
                "Motion uncertainty journal record is invalid.",
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
                    "MotionUncertaintyJournal");
            }
        }
    }
}

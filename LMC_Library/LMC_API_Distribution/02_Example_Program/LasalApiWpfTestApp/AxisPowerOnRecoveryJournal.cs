using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum AxisPowerOnRecoveryState
    {
        ArmedBeforeDispatch = 1,
        AcceptedAwaitingProof = 2,
        RecoveryRequired = 3,
        Resolved = 4
    }

    internal sealed class AxisPowerOnRecoveryRecord
    {
        internal AxisPowerOnRecoveryRecord(
            Guid identity,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            AxisPowerOnRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
            : this(
                identity,
                true,
                endpointIp,
                endpointPort,
                axisName,
                axisReference,
                diagnosticsBootId,
                mapRevision,
                state,
                createdUtc,
                updatedUtc)
        {
        }

        internal AxisPowerOnRecoveryRecord(
            Guid identity,
            bool expectedPowerOn,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            AxisPowerOnRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Axis Power On recovery identity cannot be empty.",
                    "identity");
            }

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
            ExpectedPowerOn = expectedPowerOn;
            EndpointPort = endpointPort;
            AxisName = axisName;
            AxisReference = axisReference;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            State = state;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
        }

        internal Guid Identity { get; private set; }
        internal bool ExpectedPowerOn { get; private set; }
        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal string AxisName { get; private set; }
        internal ushort AxisReference { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal AxisPowerOnRecoveryState State { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }

        internal bool IsActive
        {
            get { return State != AxisPowerOnRecoveryState.Resolved; }
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

        internal bool MatchesRecoveryIdentity(
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            return MatchesRecoveryIdentity(
                endpointIp,
                endpointPort,
                axisName,
                axisReference,
                diagnosticsBootId,
                mapRevision,
                true);
        }

        internal bool MatchesRecoveryIdentity(
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            bool expectedPowerOn)
        {
            return MatchesEndpoint(endpointIp, endpointPort)
                && string.Equals(
                    AxisName,
                    axisName,
                    StringComparison.Ordinal)
                && AxisReference == axisReference
                && DiagnosticsBootId == diagnosticsBootId
                && MapRevision == mapRevision
                && ExpectedPowerOn == expectedPowerOn;
        }

        internal AxisPowerOnRecoveryRecord Copy()
        {
            return new AxisPowerOnRecoveryRecord(
                Identity,
                ExpectedPowerOn,
                EndpointIp,
                EndpointPort,
                AxisName,
                AxisReference,
                DiagnosticsBootId,
                MapRevision,
                State,
                CreatedUtc,
                UpdatedUtc);
        }

        internal AxisPowerOnRecoveryRecord TransitionTo(
            AxisPowerOnRecoveryState nextState,
            DateTime updatedUtc)
        {
            if (!CanTransition(State, nextState, ExpectedPowerOn))
            {
                throw new InvalidOperationException(
                    "Axis Power recovery state cannot transition from "
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
                    "Recovery transition time must be UTC and cannot move backwards.");
            }

            return new AxisPowerOnRecoveryRecord(
                Identity,
                ExpectedPowerOn,
                EndpointIp,
                EndpointPort,
                AxisName,
                AxisReference,
                DiagnosticsBootId,
                MapRevision,
                nextState,
                CreatedUtc,
                updatedUtc);
        }

        private static bool CanTransition(
            AxisPowerOnRecoveryState current,
            AxisPowerOnRecoveryState next,
            bool expectedPowerOn)
        {
            if (current == next || current == AxisPowerOnRecoveryState.Resolved)
            {
                return false;
            }

            if (next == AxisPowerOnRecoveryState.Resolved)
            {
                return true;
            }

            if (next == AxisPowerOnRecoveryState.RecoveryRequired)
            {
                return current == AxisPowerOnRecoveryState.ArmedBeforeDispatch
                    || current
                        == AxisPowerOnRecoveryState.AcceptedAwaitingProof;
            }

            if (next != AxisPowerOnRecoveryState.AcceptedAwaitingProof)
            {
                return false;
            }

            return current == AxisPowerOnRecoveryState.ArmedBeforeDispatch
                || (!expectedPowerOn
                    && current
                        == AxisPowerOnRecoveryState.RecoveryRequired);
        }

        private static void ValidateState(AxisPowerOnRecoveryState state)
        {
            if (state != AxisPowerOnRecoveryState.ArmedBeforeDispatch
                && state != AxisPowerOnRecoveryState.AcceptedAwaitingProof
                && state != AxisPowerOnRecoveryState.RecoveryRequired
                && state != AxisPowerOnRecoveryState.Resolved)
            {
                throw new ArgumentOutOfRangeException("state");
            }
        }

        private static void ValidateAxisName(string axisName)
        {
            if (string.IsNullOrWhiteSpace(axisName)
                || axisName.Length > 256)
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

    internal sealed class AxisPowerOnRecoveryJournal : IDisposable
    {
        private const int LegacyFormatVersion = 1;
        private const int FormatVersion = 2;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 8192;
        private const int MaximumTextLength = 1024;
        private const string JournalFileName = "axis-power-on-recovery.bin";
        private const string LockFileName = "axis-power-on-recovery.lock";
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOAXP1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private AxisPowerOnRecoveryRecord currentRecord;
        private bool disposed;

        private AxisPowerOnRecoveryJournal(string requestedDirectoryPath)
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

        internal AxisPowerOnRecoveryRecord CurrentRecord
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

        internal static AxisPowerOnRecoveryJournal Open(string directoryPath)
        {
            return new AxisPowerOnRecoveryJournal(directoryPath);
        }

        internal static AxisPowerOnRecoveryJournal OpenDefault()
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
                "AxisPowerOnRecoveryJournal",
                "v1");
        }

        internal AxisPowerOnRecoveryRecord ArmBeforeDispatch(
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                true,
                endpointIp,
                endpointPort,
                axisName,
                axisReference,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal AxisPowerOnRecoveryRecord ArmBeforeDispatch(
            bool expectedPowerOn,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
            uint diagnosticsBootId,
            uint mapRevision,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                Guid.NewGuid(),
                expectedPowerOn,
                endpointIp,
                endpointPort,
                axisName,
                axisReference,
                diagnosticsBootId,
                mapRevision,
                createdUtc);
        }

        internal AxisPowerOnRecoveryRecord ArmBeforeDispatch(
            Guid identity,
            bool expectedPowerOn,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort axisReference,
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
                        "An unresolved Axis Power recovery record already exists.");
                }

                if (currentRecord != null
                    && currentRecord.Identity == identity)
                {
                    throw new InvalidOperationException(
                        "A resolved Axis Power recovery identity cannot be reused.");
                }

                var armed = new AxisPowerOnRecoveryRecord(
                    identity,
                    expectedPowerOn,
                    endpointIp,
                    endpointPort,
                    axisName,
                    axisReference,
                    diagnosticsBootId,
                    mapRevision,
                    AxisPowerOnRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(armed);
                currentRecord = armed;
                return armed.Copy();
            }
        }

        internal AxisPowerOnRecoveryRecord MarkAccepted(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                AxisPowerOnRecoveryState.AcceptedAwaitingProof,
                updatedUtc);
        }

        internal AxisPowerOnRecoveryRecord
            ReplaceActivePowerOnWithPowerOffBeforeDispatch(
                Guid oldIdentity,
                string endpointIp,
                int endpointPort,
                string axisName,
                ushort axisReference,
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
                        axisName,
                        axisReference,
                        diagnosticsBootId,
                        mapRevision,
                        true))
                {
                    throw new InvalidOperationException(
                        "Only the exact active Axis Power On recovery record can be atomically replaced by Power Off.");
                }

                if (createdUtc.Kind != DateTimeKind.Utc
                    || createdUtc < current.UpdatedUtc)
                {
                    throw new ArgumentOutOfRangeException(
                        "createdUtc",
                        "The Power Off replacement time must be UTC and cannot precede the active record update.");
                }

                var replacement = new AxisPowerOnRecoveryRecord(
                    Guid.NewGuid(),
                    false,
                    endpointIp,
                    endpointPort,
                    axisName,
                    axisReference,
                    diagnosticsBootId,
                    mapRevision,
                    AxisPowerOnRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(replacement);
                currentRecord = replacement;
                return replacement.Copy();
            }
        }

        internal AxisPowerOnRecoveryRecord PromoteToRecoveryRequired(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                AxisPowerOnRecoveryState.RecoveryRequired,
                updatedUtc);
        }

        internal AxisPowerOnRecoveryRecord Resolve(
            Guid identity,
            DateTime updatedUtc)
        {
            return Transition(
                identity,
                AxisPowerOnRecoveryState.Resolved,
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

        private AxisPowerOnRecoveryRecord Transition(
            Guid identity,
            AxisPowerOnRecoveryState nextState,
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

        private AxisPowerOnRecoveryRecord RequireCurrentRecord(
            Guid identity)
        {
            if (currentRecord == null)
            {
                throw new InvalidOperationException(
                    "No Axis Power recovery record exists.");
            }

            if (identity == Guid.Empty
                || currentRecord.Identity != identity)
            {
                throw new InvalidOperationException(
                    "Axis Power recovery transition identity does not match the durable record.");
            }

            return currentRecord;
        }

        private void PersistRecord(AxisPowerOnRecoveryRecord record)
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

        private static AxisPowerOnRecoveryRecord LoadRecord(string path)
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
                        "Axis Power recovery journal length is invalid.");
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
                            "Axis Power recovery journal is truncated.");
                    }

                    offset += read;
                }

                return DeserializeRecord(bytes);
            }
        }

        private static byte[] SerializeRecord(AxisPowerOnRecoveryRecord record)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
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
                writer.Write(record.AxisReference);
                WriteText(writer, record.EndpointIp);
                WriteText(writer, record.AxisName);
                writer.Flush();
                payload = payloadStream.ToArray();
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

        private static AxisPowerOnRecoveryRecord DeserializeRecord(byte[] bytes)
        {
            try
            {
                if (bytes == null
                    || bytes.Length < Magic.Length + 8 + ChecksumLength
                    || bytes.Length > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Axis Power On recovery journal length is invalid.");
                }

                var checksumOffset = bytes.Length - ChecksumLength;
                byte[] computed;
                using (var sha256 = SHA256.Create())
                {
                    computed = sha256.ComputeHash(bytes, 0, checksumOffset);
                }

                if (!ChecksumEquals(computed, bytes, checksumOffset))
                {
                    throw new InvalidDataException(
                        "Axis Power On recovery journal checksum is invalid.");
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
                    var magic = reader.ReadBytes(Magic.Length);
                    if (!ByteArraysEqual(Magic, magic))
                    {
                        throw new InvalidDataException(
                            "Axis Power On recovery journal header is invalid.");
                    }

                    var formatVersion = reader.ReadInt32();
                    if (formatVersion != LegacyFormatVersion
                        && formatVersion != FormatVersion)
                    {
                        throw new InvalidDataException(
                            "Axis Power recovery journal version is unsupported.");
                    }

                    var payloadLength = reader.ReadInt32();
                    if (payloadLength <= 0
                        || payloadLength
                            != checksumOffset - Magic.Length - 8)
                    {
                        throw new InvalidDataException(
                            "Axis Power On recovery journal payload length is invalid.");
                    }

                    var payload = reader.ReadBytes(payloadLength);
                    if (payload.Length != payloadLength
                        || stream.Position != checksumOffset)
                    {
                        throw new InvalidDataException(
                            "Axis Power On recovery journal payload is incomplete.");
                    }

                    return DeserializePayload(payload, formatVersion);
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
                    "Axis Power On recovery journal is invalid.",
                    error);
            }
        }

        private static AxisPowerOnRecoveryRecord DeserializePayload(
            byte[] payload,
            int formatVersion)
        {
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var identityBytes = reader.ReadBytes(16);
                if (identityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Axis Power On recovery identity is incomplete.");
                }

                var identity = new Guid(identityBytes);
                var state = (AxisPowerOnRecoveryState)reader.ReadInt32();
                var expectedPowerOn = true;
                if (formatVersion == FormatVersion)
                {
                    var expectedPowerOnValue = reader.ReadByte();
                    if (expectedPowerOnValue > 1)
                    {
                        throw new InvalidDataException(
                            "Axis Power recovery direction is invalid.");
                    }

                    expectedPowerOn = expectedPowerOnValue == 1;
                }

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
                var endpointIp = ReadText(reader);
                var axisName = ReadText(reader);
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Axis Power On recovery journal has trailing data.");
                }

                var record = new AxisPowerOnRecoveryRecord(
                    identity,
                    expectedPowerOn,
                    endpointIp,
                    port,
                    axisName,
                    axisReference,
                    bootId,
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
                        "Axis Power recovery endpoint is not canonical.");
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
                    "Axis Power On recovery text length is invalid.");
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
                    "Axis Power On recovery text length is invalid.");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Axis Power On recovery text is incomplete.");
            }

            foreach (var value in bytes)
            {
                if (value < 0x20 || value > 0x7E)
                {
                    throw new InvalidDataException(
                        "Axis Power On recovery text is not 7-bit printable ASCII.");
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
                || actualOffset < 0
                || actual == null
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
                    "AxisPowerOnRecoveryJournal");
            }
        }
    }
}

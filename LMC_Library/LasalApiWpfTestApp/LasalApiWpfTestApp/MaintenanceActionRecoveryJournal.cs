using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum MaintenanceActionKind
    {
        LmcHome = 1,
        Ds402Home = 2,
        EncoderTw20ErrorWarningReset = 3,
        EncoderTw19MultiturnPositionReset = 4
    }

    internal enum MaintenanceActionRecoveryState
    {
        ArmedBeforeDispatch = 1,
        RecoveryRequired = 2,
        Resolved = 3
    }

    internal sealed class MaintenanceActionRecoveryRecord
    {
        internal MaintenanceActionRecoveryRecord(
            Guid identity,
            MaintenanceActionKind action,
            string endpointIp,
            int endpointPort,
            uint observedDiagnosticsBuild,
            uint observedDiagnosticsBootId,
            uint observedMapRevision,
            string axisName,
            ushort axisReference,
            uint clientIntentId0,
            uint clientIntentId1,
            uint clientIntentId2,
            uint clientIntentId3,
            uint transportCorrelationId,
            string actionParameters,
            MaintenanceActionRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Maintenance action recovery identity cannot be empty.",
                    "identity");
            }

            ValidateAction(action);
            EndpointIp = NormalizeEndpointIp(endpointIp);
            if (endpointPort < 1 || endpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException("endpointPort");
            }

            if (observedDiagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "observedDiagnosticsBuild");
            }

            if (observedDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "observedDiagnosticsBootId");
            }

            if (observedMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "observedMapRevision");
            }

            ValidateAsciiText(axisName, 256, "axisName");
            if (axisReference < 1 || axisReference > 4)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }

            if (action != MaintenanceActionKind.LmcHome
                && clientIntentId0 == 0
                && clientIntentId1 == 0
                && clientIntentId2 == 0
                && clientIntentId3 == 0)
            {
                throw new ArgumentException(
                    "The 128-bit maintenance client intent must not be all zero.",
                    "clientIntentId0");
            }

            ValidateAsciiText(
                actionParameters,
                2048,
                "actionParameters");
            ValidateState(state);
            if (createdUtc.Kind != DateTimeKind.Utc
                || updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Maintenance recovery timestamps must be UTC and monotonic.");
            }

            Identity = identity;
            Action = action;
            EndpointPort = endpointPort;
            ObservedDiagnosticsBuild = observedDiagnosticsBuild;
            ObservedDiagnosticsBootId = observedDiagnosticsBootId;
            ObservedMapRevision = observedMapRevision;
            AxisName = axisName;
            AxisReference = axisReference;
            ClientIntentId0 = clientIntentId0;
            ClientIntentId1 = clientIntentId1;
            ClientIntentId2 = clientIntentId2;
            ClientIntentId3 = clientIntentId3;
            TransportCorrelationId = transportCorrelationId;
            ActionParameters = actionParameters;
            State = state;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
        }

        internal Guid Identity { get; private set; }
        internal MaintenanceActionKind Action { get; private set; }
        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal uint ObservedDiagnosticsBuild { get; private set; }
        internal uint ObservedDiagnosticsBootId { get; private set; }
        internal uint ObservedMapRevision { get; private set; }
        internal string AxisName { get; private set; }
        internal ushort AxisReference { get; private set; }
        internal uint ClientIntentId0 { get; private set; }
        internal uint ClientIntentId1 { get; private set; }
        internal uint ClientIntentId2 { get; private set; }
        internal uint ClientIntentId3 { get; private set; }
        internal uint TransportCorrelationId { get; private set; }
        internal string ActionParameters { get; private set; }
        internal MaintenanceActionRecoveryState State { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }

        internal bool IsActive
        {
            get { return State != MaintenanceActionRecoveryState.Resolved; }
        }

        internal bool HasAnyClientIntent
        {
            get
            {
                return ClientIntentId0 != 0
                    || ClientIntentId1 != 0
                    || ClientIntentId2 != 0
                    || ClientIntentId3 != 0;
            }
        }

        internal MaintenanceActionRecoveryRecord Copy()
        {
            return new MaintenanceActionRecoveryRecord(
                Identity,
                Action,
                EndpointIp,
                EndpointPort,
                ObservedDiagnosticsBuild,
                ObservedDiagnosticsBootId,
                ObservedMapRevision,
                AxisName,
                AxisReference,
                ClientIntentId0,
                ClientIntentId1,
                ClientIntentId2,
                ClientIntentId3,
                TransportCorrelationId,
                ActionParameters,
                State,
                CreatedUtc,
                UpdatedUtc);
        }

        internal MaintenanceActionRecoveryRecord TransitionTo(
            MaintenanceActionRecoveryState nextState,
            uint transportCorrelationId,
            DateTime updatedUtc)
        {
            if (State == MaintenanceActionRecoveryState.Resolved
                || State == nextState
                || (nextState != MaintenanceActionRecoveryState.RecoveryRequired
                    && nextState != MaintenanceActionRecoveryState.Resolved))
            {
                throw new InvalidOperationException(
                    "Maintenance recovery cannot transition from "
                    + State
                    + " to "
                    + nextState
                    + ".");
            }

            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < UpdatedUtc)
            {
                throw new ArgumentOutOfRangeException("updatedUtc");
            }

            if (TransportCorrelationId != 0
                && transportCorrelationId != 0
                && TransportCorrelationId != transportCorrelationId)
            {
                throw new InvalidOperationException(
                    "The maintenance transport correlation cannot be replaced.");
            }

            return new MaintenanceActionRecoveryRecord(
                Identity,
                Action,
                EndpointIp,
                EndpointPort,
                ObservedDiagnosticsBuild,
                ObservedDiagnosticsBootId,
                ObservedMapRevision,
                AxisName,
                AxisReference,
                ClientIntentId0,
                ClientIntentId1,
                ClientIntentId2,
                ClientIntentId3,
                transportCorrelationId == 0
                    ? TransportCorrelationId
                    : transportCorrelationId,
                ActionParameters,
                nextState,
                CreatedUtc,
                updatedUtc);
        }

        internal bool ExactEquals(MaintenanceActionRecoveryRecord other)
        {
            return other != null
                && Identity == other.Identity
                && Action == other.Action
                && string.Equals(EndpointIp, other.EndpointIp, StringComparison.Ordinal)
                && EndpointPort == other.EndpointPort
                && ObservedDiagnosticsBuild
                    == other.ObservedDiagnosticsBuild
                && ObservedDiagnosticsBootId
                    == other.ObservedDiagnosticsBootId
                && ObservedMapRevision == other.ObservedMapRevision
                && string.Equals(AxisName, other.AxisName, StringComparison.Ordinal)
                && AxisReference == other.AxisReference
                && ClientIntentId0 == other.ClientIntentId0
                && ClientIntentId1 == other.ClientIntentId1
                && ClientIntentId2 == other.ClientIntentId2
                && ClientIntentId3 == other.ClientIntentId3
                && TransportCorrelationId == other.TransportCorrelationId
                && string.Equals(
                    ActionParameters,
                    other.ActionParameters,
                    StringComparison.Ordinal)
                && State == other.State
                && CreatedUtc == other.CreatedUtc
                && UpdatedUtc == other.UpdatedUtc;
        }

        private static void ValidateAction(MaintenanceActionKind action)
        {
            if (action != MaintenanceActionKind.LmcHome
                && action != MaintenanceActionKind.Ds402Home
                && action
                    != MaintenanceActionKind
                        .EncoderTw20ErrorWarningReset
                && action
                    != MaintenanceActionKind
                        .EncoderTw19MultiturnPositionReset)
            {
                throw new ArgumentOutOfRangeException("action");
            }
        }

        private static void ValidateState(
            MaintenanceActionRecoveryState state)
        {
            if (state != MaintenanceActionRecoveryState.ArmedBeforeDispatch
                && state != MaintenanceActionRecoveryState.RecoveryRequired
                && state != MaintenanceActionRecoveryState.Resolved)
            {
                throw new ArgumentOutOfRangeException("state");
            }
        }

        private static void ValidateAsciiText(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > maximumLength)
            {
                throw new ArgumentException(
                    "Maintenance recovery text is required and exceeds no bounds.",
                    parameterName);
            }

            foreach (var character in value)
            {
                if (character < 0x20 || character > 0x7E)
                {
                    throw new ArgumentException(
                        "Maintenance recovery text must be printable 7-bit ASCII.",
                        parameterName);
                }
            }
        }

        private static string NormalizeEndpointIp(string endpointIp)
        {
            IPAddress parsed;
            if (string.IsNullOrWhiteSpace(endpointIp)
                || !IPAddress.TryParse(endpointIp, out parsed)
                || parsed.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    "Maintenance endpoint must be an IPv4 literal.",
                    "endpointIp");
            }

            return parsed.ToString();
        }
    }

    internal sealed class MaintenanceActionRecoveryJournal : IDisposable
    {
        private const int LegacyFormatVersion1 = 1;
        private const int LegacyFormatVersion2 = 2;
        private const int LegacyFormatVersion3 = 3;
        private const int LegacyFormatVersion4 = 4;
        private const int FormatVersion = 5;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 16384;
        private const int MaximumTextByteLength = 4096;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOMNT1");
        internal const string JournalFileName =
            "maintenance-action-recovery.bin";
        internal const string LockFileName =
            "maintenance-action-recovery.lock";

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private MaintenanceActionRecoveryRecord currentRecord;
        private bool recoveredAtStartup;
        private bool disposed;

        private MaintenanceActionRecoveryJournal(string requestedDirectoryPath)
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
                int loadedFormatVersion;
                currentRecord = LoadRecord(
                    journalFilePath,
                    out loadedFormatVersion);
                HandleLegacyRecord(loadedFormatVersion);
                PromoteArmedRecordAtOpen();
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

        internal MaintenanceActionRecoveryRecord CurrentRecord
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord == null ? null : currentRecord.Copy();
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

        internal bool RecoveredAtStartup
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return recoveredAtStartup;
                }
            }
        }

        internal static MaintenanceActionRecoveryJournal Open(
            string directoryPath)
        {
            return new MaintenanceActionRecoveryJournal(directoryPath);
        }

        internal static MaintenanceActionRecoveryJournal OpenDefault()
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
                "MaintenanceActionRecoveryJournal",
                "v1");
        }

        internal MaintenanceActionRecoveryRecord ArmBeforeDispatch(
            MaintenanceActionKind action,
            string endpointIp,
            int endpointPort,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            string axisName,
            ushort axisReference,
            uint clientIntentId0,
            uint clientIntentId1,
            uint clientIntentId2,
            uint clientIntentId3,
            uint transportCorrelationId,
            string actionParameters,
            DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved maintenance action recovery record already exists.");
                }

                if (transportCorrelationId == 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "transportCorrelationId",
                        "Maintenance recovery requires a non-zero transport correlation before dispatch.");
                }

                var hasAnyClientIntent = clientIntentId0 != 0
                    || clientIntentId1 != 0
                    || clientIntentId2 != 0
                    || clientIntentId3 != 0;
                if (action == MaintenanceActionKind.LmcHome
                    && !hasAnyClientIntent)
                {
                    throw new ArgumentException(
                        "LMC Home 0x7D13 requires the exact nonzero wire client-intent identity.",
                        "clientIntentId0");
                }

                if (action == MaintenanceActionKind.LmcHome
                    && !HasExactLmcHomeSemantic(actionParameters))
                {
                    throw new ArgumentException(
                        "The LMC Home recovery record must identify the exact CurrentPositionZero semantic.",
                        "actionParameters");
                }

                if (action == MaintenanceActionKind.Ds402Home
                    && !HasExactDs402HomeSemantic(actionParameters))
                {
                    throw new ArgumentException(
                        "The DS402 Home recovery record must identify the exact non-moving method 37/current-position-zero semantic.",
                        "actionParameters");
                }

                if ((action
                            == MaintenanceActionKind
                                .EncoderTw20ErrorWarningReset
                        || action
                            == MaintenanceActionKind
                                .EncoderTw19MultiturnPositionReset)
                    && !HasExactEncoderMaintenanceSemantic(
                        action,
                        actionParameters))
                {
                    throw new ArgumentException(
                        "The encoder-maintenance recovery record must identify the exact dedicated TW[20] or TW[19] semantic and recovery key.",
                        "actionParameters");
                }

                var armed = new MaintenanceActionRecoveryRecord(
                    Guid.NewGuid(),
                    action,
                    endpointIp,
                    endpointPort,
                    diagnosticsBuild,
                    diagnosticsBootId,
                    mapRevision,
                    axisName,
                    axisReference,
                    clientIntentId0,
                    clientIntentId1,
                    clientIntentId2,
                    clientIntentId3,
                    transportCorrelationId,
                    actionParameters,
                    MaintenanceActionRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(armed);
                currentRecord = armed;
                recoveredAtStartup = false;
                return armed.Copy();
            }
        }

        private void HandleLegacyRecord(int loadedFormatVersion)
        {
            if (currentRecord == null
                || loadedFormatVersion == FormatVersion)
            {
                return;
            }

            if (currentRecord.IsActive)
            {
                throw new InvalidDataException(
                    "Maintenance recovery journal version "
                    + loadedFormatVersion
                    + " contains an active legacy Home/encoder-maintenance record. It remains quarantined and must not be reinterpreted as the current 0x7D13, method-37, or 0x7E53 recovery contract, replayed, or overwritten.");
            }

            // A resolved legacy record is inert. Preserve its original bytes
            // and version so no legacy action semantics are re-stamped as v5.
            // The next valid ArmBeforeDispatch atomically replaces it with a
            // current-format record.
        }

        internal MaintenanceActionRecoveryRecord PromoteToRecoveryRequired(
            MaintenanceActionRecoveryRecord expectedRecord,
            uint transportCorrelationId,
            DateTime updatedUtc)
        {
            return TransitionExpected(
                expectedRecord,
                MaintenanceActionRecoveryState.RecoveryRequired,
                transportCorrelationId,
                updatedUtc);
        }

        internal MaintenanceActionRecoveryRecord Resolve(
            MaintenanceActionRecoveryRecord expectedRecord,
            DateTime updatedUtc)
        {
            return TransitionExpected(
                expectedRecord,
                MaintenanceActionRecoveryState.Resolved,
                0,
                updatedUtc);
        }

        internal MaintenanceActionRecoveryRecord
            ResolveConfirmedRejection(
                MaintenanceActionRecoveryRecord expectedRecord,
                uint transportCorrelationId,
                DateTime updatedUtc)
        {
            if (expectedRecord == null)
            {
                throw new ArgumentNullException("expectedRecord");
            }

            if (transportCorrelationId == 0
                || (expectedRecord.TransportCorrelationId != 0
                    && expectedRecord.TransportCorrelationId
                        != transportCorrelationId))
            {
                throw new InvalidOperationException(
                    "A confirmed rejection must match the exact non-zero maintenance transport correlation.");
            }

            return TransitionExpected(
                expectedRecord,
                MaintenanceActionRecoveryState.Resolved,
                transportCorrelationId,
                updatedUtc);
        }

        internal RecoveryJournalSourceEvidence
            CaptureActiveRetirementEvidence()
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord == null || !currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "No active Home/encoder-maintenance recovery record exists for operator retirement.");
                }

                var originalBytes = File.ReadAllBytes(journalFilePath);
                int formatVersion;
                var diskRecord = DeserializeRecord(
                    originalBytes,
                    out formatVersion);
                var serializedCurrent = SerializeRecord(currentRecord);
                if (formatVersion != FormatVersion
                    || diskRecord == null
                    || !currentRecord.ExactEquals(diskRecord)
                    || !RecoveryJournalSourceEvidence.ConstantTimeEquals(
                        serializedCurrent,
                        originalBytes))
                {
                    throw new InvalidDataException(
                        "Home/encoder-maintenance recovery memory state does not match the exact durable source bytes.");
                }

                return new RecoveryJournalSourceEvidence(
                    RecoveryRecordOwner.MaintenanceAction,
                    diskRecord.Identity,
                    (int)diskRecord.State,
                    diskRecord.CreatedUtc,
                    diskRecord.UpdatedUtc,
                    diskRecord.EndpointIp,
                    diskRecord.EndpointPort,
                    diskRecord.ObservedDiagnosticsBuild,
                    diskRecord.ObservedDiagnosticsBootId,
                    diskRecord.ObservedMapRevision,
                    "Axis",
                    diskRecord.AxisName,
                    diskRecord.AxisReference,
                    "MaintenanceAction/" + diskRecord.Action,
                    "Action=" + ((int)diskRecord.Action).ToString(
                        CultureInfo.InvariantCulture)
                        + ";Intent="
                        + diskRecord.ClientIntentId0.ToString("X8")
                        + diskRecord.ClientIntentId1.ToString("X8")
                        + diskRecord.ClientIntentId2.ToString("X8")
                        + diskRecord.ClientIntentId3.ToString("X8")
                        + ";Correlation="
                        + diskRecord.TransportCorrelationId.ToString(
                            CultureInfo.InvariantCulture)
                        + ";Parameters="
                        + diskRecord.ActionParameters,
                    originalBytes);
            }
        }

        internal MaintenanceActionRecoveryRecord ResolveOperatorRetirement(
            RecoveryJournalSourceEvidence expectedEvidence,
            RecoveryRecordRetirementDecision committedDecision,
            DateTime updatedUtc)
        {
            if (expectedEvidence == null)
            {
                throw new ArgumentNullException("expectedEvidence");
            }
            if (committedDecision == null
                || !committedDecision.IsDurablyCommitted)
            {
                throw new InvalidOperationException(
                    "Home/encoder-maintenance operator retirement requires a durably committed ledger decision.");
            }
            if (!committedDecision.MatchesSourceEvidence(expectedEvidence))
            {
                throw new InvalidOperationException(
                    "The committed retirement decision does not match the expected Home/encoder-maintenance source evidence.");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                var currentEvidence = CaptureActiveRetirementEvidence();
                if (!expectedEvidence.ExactSourceEquals(currentEvidence)
                    || !committedDecision.MatchesSourceEvidence(
                        currentEvidence))
                {
                    throw new InvalidOperationException(
                        "Home/encoder-maintenance recovery changed after operator confirmation; retirement was not applied.");
                }

                var resolved = currentRecord.TransitionTo(
                    MaintenanceActionRecoveryState.Resolved,
                    0,
                    updatedUtc);
                PersistRecord(resolved);
                currentRecord = resolved;
                recoveredAtStartup = false;
                return resolved.Copy();
            }
        }

        private MaintenanceActionRecoveryRecord TransitionExpected(
            MaintenanceActionRecoveryRecord expectedRecord,
            MaintenanceActionRecoveryState nextState,
            uint transportCorrelationId,
            DateTime updatedUtc)
        {
            if (expectedRecord == null)
            {
                throw new ArgumentNullException("expectedRecord");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord == null
                    || !currentRecord.ExactEquals(expectedRecord))
                {
                    throw new InvalidOperationException(
                        "The maintenance recovery record changed after confirmation.");
                }

                var transitioned = currentRecord.TransitionTo(
                    nextState,
                    transportCorrelationId,
                    updatedUtc);
                PersistRecord(transitioned);
                currentRecord = transitioned;
                return transitioned.Copy();
            }
        }

        private void PromoteArmedRecordAtOpen()
        {
            if (currentRecord == null || !currentRecord.IsActive)
            {
                return;
            }

            recoveredAtStartup = true;
            if (currentRecord.State
                != MaintenanceActionRecoveryState.ArmedBeforeDispatch)
            {
                return;
            }

            var now = DateTime.UtcNow;
            if (now < currentRecord.UpdatedUtc)
            {
                now = currentRecord.UpdatedUtc;
            }

            var promoted = currentRecord.TransitionTo(
                MaintenanceActionRecoveryState.RecoveryRequired,
                0,
                now);
            PersistRecord(promoted);
            currentRecord = promoted;
        }

        private void PersistRecord(MaintenanceActionRecoveryRecord record)
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
                        // Preserve the primary persistence failure.
                    }
                }
            }
        }

        private static byte[] SerializeRecord(
            MaintenanceActionRecoveryRecord record)
        {
            byte[] payload;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(record.Identity.ToByteArray());
                writer.Write((int)record.Action);
                WriteText(writer, record.EndpointIp);
                writer.Write(record.EndpointPort);
                writer.Write(record.ObservedDiagnosticsBuild);
                writer.Write(record.ObservedDiagnosticsBootId);
                writer.Write(record.ObservedMapRevision);
                WriteText(writer, record.AxisName);
                writer.Write(record.AxisReference);
                writer.Write(record.ClientIntentId0);
                writer.Write(record.ClientIntentId1);
                writer.Write(record.ClientIntentId2);
                writer.Write(record.ClientIntentId3);
                writer.Write(record.TransportCorrelationId);
                WriteText(writer, record.ActionParameters);
                writer.Write((int)record.State);
                writer.Write(record.CreatedUtc.Ticks);
                writer.Write(record.UpdatedUtc.Ticks);
                writer.Flush();
                payload = stream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
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

        private static MaintenanceActionRecoveryRecord LoadRecord(
            string path,
            out int formatVersion)
        {
            if (!File.Exists(path))
            {
                formatVersion = FormatVersion;
                return null;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception error)
            {
                throw new IOException(
                    "The maintenance recovery journal could not be read.",
                    error);
            }

            try
            {
                return DeserializeRecord(bytes, out formatVersion);
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception error)
            {
                throw new InvalidDataException(
                    "The maintenance recovery journal is corrupt.",
                    error);
            }
        }

        private static MaintenanceActionRecoveryRecord DeserializeRecord(
            byte[] bytes,
            out int formatVersion)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Maintenance recovery journal length is invalid.");
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
                    "Maintenance recovery journal checksum is invalid.");
            }

            using (var stream = new MemoryStream(bytes, 0, checksumOffset, false))
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                if (!ByteArraysEqual(Magic, reader.ReadBytes(Magic.Length)))
                {
                    throw new InvalidDataException(
                        "Maintenance recovery journal magic is invalid.");
                }

                formatVersion = reader.ReadInt32();
                if (formatVersion != LegacyFormatVersion1
                    && formatVersion != LegacyFormatVersion2
                    && formatVersion != LegacyFormatVersion3
                    && formatVersion != LegacyFormatVersion4
                    && formatVersion != FormatVersion)
                {
                    throw new InvalidDataException(
                        "Maintenance recovery journal version is unsupported.");
                }

                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Maintenance recovery journal payload length is invalid.");
                }

                var payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || stream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Maintenance recovery journal payload is incomplete.");
                }

                return DeserializePayload(payload, formatVersion);
            }
        }

        private static MaintenanceActionRecoveryRecord DeserializePayload(
            byte[] payload,
            int formatVersion)
        {
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var identityBytes = reader.ReadBytes(16);
                if (identityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Maintenance recovery identity is incomplete.");
                }

                var result = new MaintenanceActionRecoveryRecord(
                    new Guid(identityBytes),
                    (MaintenanceActionKind)reader.ReadInt32(),
                    ReadText(reader),
                    reader.ReadInt32(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    ReadText(reader),
                    reader.ReadUInt16(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    ReadText(reader),
                    (MaintenanceActionRecoveryState)reader.ReadInt32(),
                    new DateTime(reader.ReadInt64(), DateTimeKind.Utc),
                    new DateTime(reader.ReadInt64(), DateTimeKind.Utc));
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Maintenance recovery journal has trailing payload data.");
                }

                if (formatVersion == FormatVersion
                    && result.TransportCorrelationId == 0)
                {
                    throw new InvalidDataException(
                        "Current maintenance recovery format requires a non-zero transport correlation.");
                }

                if (formatVersion == FormatVersion
                    && result.Action == MaintenanceActionKind.LmcHome
                    && (!result.HasAnyClientIntent
                        || !HasExactLmcHomeSemantic(
                            result.ActionParameters)))
                {
                    throw new InvalidDataException(
                        "Current maintenance recovery format requires an exact LMC Home CurrentPositionZero identity.");
                }

                if (formatVersion == FormatVersion
                    && result.Action == MaintenanceActionKind.Ds402Home
                    && !HasExactDs402HomeSemantic(
                        result.ActionParameters))
                {
                    throw new InvalidDataException(
                        "Current maintenance recovery format requires exact non-moving DS402 method 37/current-position-zero semantics.");
                }

                if (formatVersion == FormatVersion
                    && (result.Action
                            == MaintenanceActionKind
                                .EncoderTw20ErrorWarningReset
                        || result.Action
                            == MaintenanceActionKind
                                .EncoderTw19MultiturnPositionReset)
                    && (!result.HasAnyClientIntent
                        || !HasExactEncoderMaintenanceSemantic(
                            result.Action,
                            result.ActionParameters)))
                {
                    throw new InvalidDataException(
                        "Current encoder-maintenance recovery format requires the exact dedicated TW[20]/TW[19] identity and nonzero client intent.");
                }

                return result;
            }
        }

        private static bool HasExactLmcHomeSemantic(
            string actionParameters)
        {
            Dictionary<string, string> values;
            if (!TryParseExactParameters(
                    actionParameters,
                    5,
                    out values))
            {
                return false;
            }

            uint schema;
            int targetPosition;
            int expectedActualPosition;
            uint timeout;
            return TryReadUInt(values, "Schema", out schema)
                && schema == 1
                && HasValue(
                    values,
                    "Semantic",
                    "CurrentPositionZero")
                && TryReadInt(
                    values,
                    "TargetPosition",
                    out targetPosition)
                && targetPosition == 0
                && TryReadInt(
                    values,
                    "ExpectedActualPosition",
                    out expectedActualPosition)
                && TryReadUInt(values, "TimeoutMs", out timeout)
                && timeout >= 100
                && timeout <= 5000;
        }

        private static bool HasExactDs402HomeSemantic(
            string actionParameters)
        {
            Dictionary<string, string> values;
            if (!TryParseExactParameters(
                    actionParameters,
                    9,
                    out values))
            {
                return false;
            }

            uint schema;
            int method;
            int homeOffset;
            int velocity;
            int acceleration;
            int distanceLimit;
            int torqueLimit;
            uint timeout;
            return TryReadUInt(values, "Schema", out schema)
                && schema == 1
                && TryReadInt(values, "Method", out method)
                && method == 37
                && TryReadInt(values, "HomeOffset", out homeOffset)
                && homeOffset == 0
                && TryReadInt(values, "Velocity", out velocity)
                && velocity == 0
                && TryReadInt(values, "Acceleration", out acceleration)
                && acceleration == 0
                && TryReadInt(values, "DistanceLimit", out distanceLimit)
                && distanceLimit == 0
                && TryReadInt(values, "TorqueLimit", out torqueLimit)
                && torqueLimit == 0
                && HasValue(values, "BufferMode", "Aborting")
                && TryReadUInt(values, "TimeoutMs", out timeout)
                && timeout > 0;
        }

        private static bool HasExactEncoderMaintenanceSemantic(
            MaintenanceActionKind action,
            string actionParameters)
        {
            Dictionary<string, string> values;
            if (!TryParseExactParameters(
                    actionParameters,
                    15,
                    out values))
            {
                return false;
            }

            var tw20 = action
                == MaintenanceActionKind.EncoderTw20ErrorWarningReset;
            if (!tw20
                && action
                    != MaintenanceActionKind
                        .EncoderTw19MultiturnPositionReset)
            {
                return false;
            }

            uint schema;
            uint kind;
            uint profile;
            uint drive;
            uint socket;
            uint commandValue;
            uint timeout;
            uint evidence0;
            uint evidence1;
            uint evidence2;
            uint evidence3;
            return TryReadUInt(values, "Schema", out schema)
                && schema == 1
                && HasValue(
                    values,
                    "Semantic",
                    tw20
                        ? "Tw20ErrorWarningReset"
                        : "Tw19MultiturnPositionReset")
                && TryReadUInt(values, "Kind", out kind)
                && kind == (tw20 ? 1u : 2u)
                && TryReadUInt(values, "Profile", out profile)
                && profile >= 1
                && profile <= ushort.MaxValue
                && TryReadUInt(values, "Drive", out drive)
                && drive >= 1
                && drive <= 4
                && TryReadUInt(values, "Socket", out socket)
                && socket >= 1
                && socket <= 4
                && TryReadUInt(
                    values,
                    "CommandValue",
                    out commandValue)
                && commandValue
                    == LMCEncoderMaintenanceSdoContract.ResetCommandValue
                && HasValue(values, "Object", "0x20FC")
                && HasValue(values, "Sub", tw20 ? "0x02" : "0x01")
                && HasValue(values, "Type", "UInt16")
                && TryReadUInt(
                    values,
                    "TimeoutMilliseconds",
                    out timeout)
                && timeout >= 1
                && timeout <= 60000
                && TryReadUInt(values, "Evidence0", out evidence0)
                && TryReadUInt(values, "Evidence1", out evidence1)
                && TryReadUInt(values, "Evidence2", out evidence2)
                && TryReadUInt(values, "Evidence3", out evidence3)
                && (evidence0 != 0
                    || evidence1 != 0
                    || evidence2 != 0
                    || evidence3 != 0);
        }

        private static bool TryParseExactParameters(
            string source,
            int expectedCount,
            out Dictionary<string, string> values)
        {
            values = new Dictionary<string, string>(
                StringComparer.Ordinal);
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            foreach (var segment in source.Split(';'))
            {
                var separator = segment.IndexOf('=');
                if (separator <= 0 || separator == segment.Length - 1)
                {
                    return false;
                }

                var key = segment.Substring(0, separator);
                if (values.ContainsKey(key))
                {
                    return false;
                }

                values.Add(key, segment.Substring(separator + 1));
            }

            return values.Count == expectedCount;
        }

        private static bool HasValue(
            IDictionary<string, string> values,
            string key,
            string expected)
        {
            string value;
            return values.TryGetValue(key, out value)
                && string.Equals(
                    value,
                    expected,
                    StringComparison.Ordinal);
        }

        private static bool TryReadUInt(
            IDictionary<string, string> values,
            string key,
            out uint result)
        {
            result = 0;
            string value;
            return values.TryGetValue(key, out value)
                && uint.TryParse(value, out result);
        }

        private static bool TryReadInt(
            IDictionary<string, string> values,
            string key,
            out int result)
        {
            result = 0;
            string value;
            return values.TryGetValue(key, out value)
                && int.TryParse(value, out result);
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length <= 0 || bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Maintenance recovery text encoding is invalid.");
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
                    "Maintenance recovery text length is invalid.");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Maintenance recovery text is incomplete.");
            }

            return Encoding.UTF8.GetString(bytes);
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    "MaintenanceActionRecoveryJournal");
            }
        }
    }
}

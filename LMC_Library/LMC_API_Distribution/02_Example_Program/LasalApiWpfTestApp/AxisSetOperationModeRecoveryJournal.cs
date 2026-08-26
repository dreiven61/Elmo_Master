using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum AxisSetOperationModeRecoveryState
    {
        ArmedBeforeDispatch = 1,
        RecoveryRequired = 2,
        Resolved = 3,
        TerminalOutcomeObserved = 4
    }

    internal sealed class AxisSetOperationModeTerminalOutcomeProof
    {
        internal AxisSetOperationModeTerminalOutcomeProof(
            uint queryRequestId,
            LMCAxisSetOperationModeOutcomeRecordState recordState,
            sbyte observedModeRaw,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint sdoExecutorToken,
            LMCAxisSetOperationModeEvidenceFlags evidenceFlags,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration,
            sbyte previousModeRaw,
            uint quarantineReason,
            ushort ds402StatusWord,
            uint contextCheck,
            sbyte requestedModeRaw)
        {
            ValidateTerminal(
                recordState,
                observedModeRaw,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                evidenceFlags,
                startCycle,
                completionCycle,
                nativeCommandState,
                recordGeneration,
                requestedModeRaw);

            QueryRequestId = queryRequestId;
            RecordState = recordState;
            ObservedModeRaw = observedModeRaw;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            SdoExecutorToken = sdoExecutorToken;
            EvidenceFlags = evidenceFlags;
            StartCycle = startCycle;
            CompletionCycle = completionCycle;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
            PreviousModeRaw = previousModeRaw;
            QuarantineReason = quarantineReason;
            Ds402StatusWord = ds402StatusWord;
            ContextCheck = contextCheck;
        }

        internal uint QueryRequestId { get; private set; }
        internal LMCAxisSetOperationModeOutcomeRecordState RecordState { get; private set; }
        internal sbyte ObservedModeRaw { get; private set; }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal uint SdoExecutorToken { get; private set; }
        internal LMCAxisSetOperationModeEvidenceFlags EvidenceFlags { get; private set; }
        internal uint StartCycle { get; private set; }
        internal uint CompletionCycle { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint RecordGeneration { get; private set; }
        internal sbyte PreviousModeRaw { get; private set; }
        internal uint QuarantineReason { get; private set; }
        internal ushort Ds402StatusWord { get; private set; }
        internal uint ContextCheck { get; private set; }

        internal static AxisSetOperationModeTerminalOutcomeProof FromOutcome(
            LMCAxisSetOperationModeOutcomeResult outcome)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }

            if (outcome.Response == null || !outcome.Response.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Only a successful SetOperationMode outcome query can become durable terminal proof.");
            }

            return new AxisSetOperationModeTerminalOutcomeProof(
                outcome.QueryRequestId,
                outcome.RecordState,
                outcome.ObservedModeRaw,
                outcome.OriginalCommandStatus,
                outcome.OriginalErrorId,
                outcome.OriginalDetailCodeValue,
                outcome.SdoExecutorToken,
                outcome.EvidenceFlags,
                outcome.StartCycle,
                outcome.CompletionCycle,
                outcome.NativeCommandState,
                outcome.RecordGeneration,
                outcome.PreviousModeRaw,
                outcome.QuarantineReason,
                outcome.Ds402StatusWord,
                outcome.ContextCheck,
                outcome.RecoveryKey.RequestedModeRaw);
        }

        internal static AxisSetOperationModeTerminalOutcomeProof FromRetirement(
            LMCAxisSetOperationModeOutcomeRetirementResult retirement)
        {
            if (retirement == null)
            {
                throw new ArgumentNullException("retirement");
            }

            if (retirement.Response == null
                || !retirement.Response.IsSuccess
                || !retirement.RetirementConfirmed)
            {
                throw new InvalidOperationException(
                    "Only a successful exact-generation SetOperationMode retirement can resolve the journal.");
            }

            return new AxisSetOperationModeTerminalOutcomeProof(
                0,
                retirement.RecordState,
                retirement.ObservedModeRaw,
                retirement.OriginalCommandStatus,
                retirement.OriginalErrorId,
                retirement.OriginalDetailCodeValue,
                retirement.SdoExecutorToken,
                retirement.EvidenceFlags,
                retirement.StartCycle,
                retirement.CompletionCycle,
                retirement.NativeCommandState,
                retirement.RecordGeneration,
                retirement.PreviousModeRaw,
                retirement.QuarantineReason,
                retirement.Ds402StatusWord,
                retirement.ContextCheck,
                retirement.RecoveryKey.RequestedModeRaw);
        }

        internal bool MatchesRetirement(
            AxisSetOperationModeTerminalOutcomeProof retirementProof)
        {
            if (retirementProof == null)
            {
                return false;
            }

            return RecordState == retirementProof.RecordState
                && ObservedModeRaw == retirementProof.ObservedModeRaw
                && OriginalCommandStatus == retirementProof.OriginalCommandStatus
                && OriginalErrorId == retirementProof.OriginalErrorId
                && OriginalDetailCode == retirementProof.OriginalDetailCode
                && SdoExecutorToken == retirementProof.SdoExecutorToken
                && EvidenceFlags == retirementProof.EvidenceFlags
                && StartCycle == retirementProof.StartCycle
                && CompletionCycle == retirementProof.CompletionCycle
                && NativeCommandState == retirementProof.NativeCommandState
                && RecordGeneration == retirementProof.RecordGeneration
                && PreviousModeRaw == retirementProof.PreviousModeRaw
                && QuarantineReason == retirementProof.QuarantineReason
                && Ds402StatusWord == retirementProof.Ds402StatusWord
                && ContextCheck == retirementProof.ContextCheck;
        }

        private static void ValidateTerminal(
            LMCAxisSetOperationModeOutcomeRecordState recordState,
            sbyte observedModeRaw,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            LMCAxisSetOperationModeEvidenceFlags evidenceFlags,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration,
            sbyte requestedModeRaw)
        {
            if (recordState == LMCAxisSetOperationModeOutcomeRecordState.Running)
            {
                throw new InvalidOperationException(
                    "A running SetOperationMode outcome is not durable terminal proof.");
            }

            if (recordGeneration == 0)
            {
                throw new InvalidOperationException(
                    "Terminal SetOperationMode proof requires a nonzero record generation.");
            }

            if (completionCycle == 0 || completionCycle < startCycle)
            {
                throw new InvalidOperationException(
                    "Terminal SetOperationMode proof requires completion at or after start.");
            }

            if (nativeCommandState != 0)
            {
                throw new InvalidOperationException(
                    "SetOperationMode SDO lifecycle must not expose native axis-command state.");
            }

            var terminalEvidence =
                LMCAxisSetOperationModeEvidenceFlags.OwnerReleased
                | LMCAxisSetOperationModeEvidenceFlags.ExecutorReusable;
            if ((evidenceFlags & terminalEvidence) != terminalEvidence)
            {
                throw new InvalidOperationException(
                    "Terminal SetOperationMode proof requires owner-release and executor-reusable evidence.");
            }

            if (recordState == LMCAxisSetOperationModeOutcomeRecordState.Succeeded)
            {
                var verifyEvidence =
                    LMCAxisSetOperationModeEvidenceFlags.VerifyReadDispatched
                    | LMCAxisSetOperationModeEvidenceFlags.VerifyReadCompleted;
                if (originalCommandStatus != 0
                    || originalErrorId != 0
                    || originalDetailCode != 0
                    || observedModeRaw != requestedModeRaw
                    || (evidenceFlags & verifyEvidence) != verifyEvidence)
                {
                    throw new InvalidOperationException(
                        "Successful SetOperationMode proof requires exact observed mode and completed verify-read evidence.");
                }
            }
        }
    }

    internal sealed class AxisSetOperationModeRecoveryRecord
    {
        internal AxisSetOperationModeRecoveryRecord(
            Guid identity,
            AxisSetOperationModeRecoveryState state,
            uint revision,
            string endpointIp,
            int endpointPort,
            string axisName,
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            uint clientIntentId0,
            uint clientIntentId1,
            uint clientIntentId2,
            uint clientIntentId3,
            ushort axisReference,
            sbyte requestedModeRaw,
            uint timeoutMilliseconds,
            uint flags,
            DateTime createdUtc,
            DateTime updatedUtc,
            AxisSetOperationModeTerminalOutcomeProof terminalOutcomeProof,
            uint retirementRequestId)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException("Recovery journal identity must not be empty.", "identity");
            }

            if (!Enum.IsDefined(typeof(AxisSetOperationModeRecoveryState), state))
            {
                throw new ArgumentOutOfRangeException("state");
            }

            if (revision == 0)
            {
                throw new ArgumentOutOfRangeException("revision");
            }

            EndpointIp = NormalizeEndpoint(endpointIp);
            if (endpointPort < 1 || endpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException("endpointPort");
            }

            if (string.IsNullOrWhiteSpace(axisName) || axisName.Length > 64)
            {
                throw new ArgumentException("Axis name is required and must be bounded.", "axisName");
            }

            var key = new LMCAxisSetOperationModeRecoveryKey(
                schemaVersion,
                originalRequestId,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                clientIntentId0,
                clientIntentId1,
                clientIntentId2,
                clientIntentId3,
                axisReference,
                (LMCDriveOperationMode)requestedModeRaw,
                timeoutMilliseconds);
            if (flags != key.Flags)
            {
                throw new InvalidDataException("SetOperationMode journal flags must remain zero.");
            }

            ValidateUtc(createdUtc, "createdUtc");
            ValidateUtc(updatedUtc, "updatedUtc");
            if (updatedUtc < createdUtc)
            {
                throw new ArgumentException("UpdatedUtc cannot precede CreatedUtc.", "updatedUtc");
            }

            if (state == AxisSetOperationModeRecoveryState.TerminalOutcomeObserved
                && terminalOutcomeProof == null)
            {
                throw new InvalidDataException("TerminalOutcomeObserved requires terminal proof.");
            }

            if (state == AxisSetOperationModeRecoveryState.Resolved)
            {
                if (terminalOutcomeProof == null || retirementRequestId == 0)
                {
                    throw new InvalidDataException(
                        "Resolved SetOperationMode recovery requires terminal and retirement proof.");
                }
            }
            else if (retirementRequestId != 0)
            {
                throw new InvalidDataException(
                    "Only a resolved SetOperationMode recovery may contain a retirement request id.");
            }

            if ((state == AxisSetOperationModeRecoveryState.ArmedBeforeDispatch
                    || state == AxisSetOperationModeRecoveryState.RecoveryRequired)
                && terminalOutcomeProof != null)
            {
                throw new InvalidDataException(
                    "Pre-terminal SetOperationMode recovery states cannot contain terminal proof.");
            }

            Identity = identity;
            State = state;
            Revision = revision;
            EndpointPort = endpointPort;
            AxisName = axisName.Trim();
            SchemaVersion = schemaVersion;
            OriginalRequestId = originalRequestId;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            ClientIntentId0 = clientIntentId0;
            ClientIntentId1 = clientIntentId1;
            ClientIntentId2 = clientIntentId2;
            ClientIntentId3 = clientIntentId3;
            AxisReference = axisReference;
            RequestedModeRaw = requestedModeRaw;
            TimeoutMilliseconds = timeoutMilliseconds;
            Flags = flags;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            TerminalOutcomeProof = terminalOutcomeProof;
            RetirementRequestId = retirementRequestId;
        }

        internal Guid Identity { get; private set; }
        internal AxisSetOperationModeRecoveryState State { get; private set; }
        internal uint Revision { get; private set; }
        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal string AxisName { get; private set; }
        internal ushort SchemaVersion { get; private set; }
        internal uint OriginalRequestId { get; private set; }
        internal uint DiagnosticsBuild { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal uint ClientIntentId0 { get; private set; }
        internal uint ClientIntentId1 { get; private set; }
        internal uint ClientIntentId2 { get; private set; }
        internal uint ClientIntentId3 { get; private set; }
        internal ushort AxisReference { get; private set; }
        internal sbyte RequestedModeRaw { get; private set; }
        internal uint TimeoutMilliseconds { get; private set; }
        internal uint Flags { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }
        internal AxisSetOperationModeTerminalOutcomeProof TerminalOutcomeProof { get; private set; }
        internal uint RetirementRequestId { get; private set; }
        internal bool IsActive { get { return State != AxisSetOperationModeRecoveryState.Resolved; } }
        internal bool HasTerminalOutcomeProof { get { return TerminalOutcomeProof != null; } }

        internal LMCAxisSetOperationModeRecoveryKey ToRecoveryKey()
        {
            return new LMCAxisSetOperationModeRecoveryKey(
                SchemaVersion,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                ClientIntentId0,
                ClientIntentId1,
                ClientIntentId2,
                ClientIntentId3,
                AxisReference,
                (LMCDriveOperationMode)RequestedModeRaw,
                TimeoutMilliseconds);
        }

        internal bool MatchesRecoveryKey(LMCAxisSetOperationModeRecoveryKey key)
        {
            if (key == null)
            {
                return false;
            }

            return SchemaVersion == key.SchemaVersion
                && OriginalRequestId == key.OriginalRequestId
                && DiagnosticsBuild == key.DiagnosticsBuild
                && DiagnosticsBootId == key.DiagnosticsBootId
                && MapRevision == key.MapRevision
                && ClientIntentId0 == key.ClientIntentId0
                && ClientIntentId1 == key.ClientIntentId1
                && ClientIntentId2 == key.ClientIntentId2
                && ClientIntentId3 == key.ClientIntentId3
                && AxisReference == key.AxisReference
                && RequestedModeRaw == key.RequestedModeRaw
                && TimeoutMilliseconds == key.TimeoutMilliseconds
                && Flags == key.Flags;
        }

        private static string NormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint) || endpoint.Length > 255)
            {
                throw new ArgumentException("Endpoint is required and must be bounded.", "endpoint");
            }

            var trimmed = endpoint.Trim();
            IPAddress address;
            if (IPAddress.TryParse(trimmed, out address))
            {
                return address.ToString();
            }

            if (Uri.CheckHostName(trimmed) == UriHostNameType.Unknown)
            {
                throw new ArgumentException("Endpoint must be an IP address or valid host name.", "endpoint");
            }

            return trimmed.ToLowerInvariant();
        }

        private static void ValidateUtc(DateTime value, string argumentName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Recovery journal timestamps must be UTC.", argumentName);
            }
        }
    }

    internal sealed class AxisSetOperationModeRecoveryJournal : IDisposable
    {
        private const string Magic = "ELMOASOM1";
        private const uint FormatVersion = 1;
        private const int MaximumFileLength = 16384;
        private readonly string directoryPath;

        private AxisSetOperationModeRecoveryJournal(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Recovery journal directory is required.", "directoryPath");
            }

            this.directoryPath = Path.GetFullPath(directoryPath);
            Directory.CreateDirectory(this.directoryPath);
            JournalFilePath = Path.Combine(
                this.directoryPath,
                "axis-set-operation-mode-recovery.journal");
            LoadCurrent();

            if (CurrentRecord != null
                && CurrentRecord.State == AxisSetOperationModeRecoveryState.ArmedBeforeDispatch)
            {
                CurrentRecord = Clone(
                    CurrentRecord,
                    AxisSetOperationModeRecoveryState.RecoveryRequired,
                    CheckedIncrement(CurrentRecord.Revision),
                    CurrentRecord.UpdatedUtc,
                    null,
                    0);
                Persist(CurrentRecord);
            }
        }

        internal string JournalFilePath { get; private set; }
        internal AxisSetOperationModeRecoveryRecord CurrentRecord { get; private set; }
        internal bool HasActiveRecord { get { return CurrentRecord != null && CurrentRecord.IsActive; } }

        internal static AxisSetOperationModeRecoveryJournal Open(string directoryPath)
        {
            return new AxisSetOperationModeRecoveryJournal(directoryPath);
        }

        internal static string GetDefaultDirectoryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisSetOperationModeRecoveryJournal",
                "v1");
        }

        internal AxisSetOperationModeRecoveryRecord ArmBeforeDispatch(
            Guid identity,
            string endpointIp,
            int endpointPort,
            string axisName,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            DateTime createdUtc)
        {
            if (HasActiveRecord)
            {
                throw new InvalidOperationException(
                    "An unresolved SetOperationMode recovery record already exists.");
            }

            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            var record = new AxisSetOperationModeRecoveryRecord(
                identity,
                AxisSetOperationModeRecoveryState.ArmedBeforeDispatch,
                1,
                endpointIp,
                endpointPort,
                axisName,
                recoveryKey.SchemaVersion,
                recoveryKey.OriginalRequestId,
                recoveryKey.DiagnosticsBuild,
                recoveryKey.DiagnosticsBootId,
                recoveryKey.MapRevision,
                recoveryKey.ClientIntentId0,
                recoveryKey.ClientIntentId1,
                recoveryKey.ClientIntentId2,
                recoveryKey.ClientIntentId3,
                recoveryKey.AxisReference,
                recoveryKey.RequestedModeRaw,
                recoveryKey.TimeoutMilliseconds,
                recoveryKey.Flags,
                createdUtc,
                createdUtc,
                null,
                0);
            PersistAndPublish(record);
            return record;
        }

        internal AxisSetOperationModeRecoveryRecord PromoteToRecoveryRequired(
            AxisSetOperationModeRecoveryRecord expected,
            DateTime updatedUtc)
        {
            EnsureExpectedCurrent(expected);
            if (expected.State != AxisSetOperationModeRecoveryState.ArmedBeforeDispatch)
            {
                throw new InvalidOperationException(
                    "Only an armed SetOperationMode record can be promoted to recovery-required.");
            }

            var next = Clone(
                expected,
                AxisSetOperationModeRecoveryState.RecoveryRequired,
                CheckedIncrement(expected.Revision),
                updatedUtc,
                null,
                0);
            PersistAndPublish(next);
            return next;
        }

        internal AxisSetOperationModeRecoveryRecord RecordTerminalOutcome(
            AxisSetOperationModeRecoveryRecord expected,
            LMCAxisSetOperationModeOutcomeResult outcome,
            DateTime updatedUtc)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }

            return RecordTerminalOutcomeProof(
                expected,
                outcome.RecoveryKey,
                AxisSetOperationModeTerminalOutcomeProof.FromOutcome(outcome),
                updatedUtc);
        }

        internal AxisSetOperationModeRecoveryRecord RecordTerminalOutcomeProof(
            AxisSetOperationModeRecoveryRecord expected,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            AxisSetOperationModeTerminalOutcomeProof proof,
            DateTime updatedUtc)
        {
            EnsureExpectedCurrent(expected);
            if (expected.State != AxisSetOperationModeRecoveryState.ArmedBeforeDispatch
                && expected.State != AxisSetOperationModeRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    "Terminal SetOperationMode proof can only advance an unresolved pre-terminal record.");
            }

            if (!expected.MatchesRecoveryKey(recoveryKey))
            {
                throw new InvalidOperationException(
                    "SetOperationMode terminal outcome does not match the durable recovery key.");
            }

            if (proof == null)
            {
                throw new ArgumentNullException("proof");
            }

            var next = Clone(
                expected,
                AxisSetOperationModeRecoveryState.TerminalOutcomeObserved,
                CheckedIncrement(expected.Revision),
                updatedUtc,
                proof,
                0);
            PersistAndPublish(next);
            return next;
        }

        internal AxisSetOperationModeRecoveryRecord ResolveAfterRetirement(
            AxisSetOperationModeRecoveryRecord expected,
            LMCAxisSetOperationModeOutcomeRetirementResult retirement,
            DateTime updatedUtc)
        {
            if (retirement == null)
            {
                throw new ArgumentNullException("retirement");
            }

            return ResolveAfterRetirementProof(
                expected,
                retirement.RecoveryKey,
                retirement.RetireRequestId,
                AxisSetOperationModeTerminalOutcomeProof.FromRetirement(retirement),
                updatedUtc);
        }

        internal AxisSetOperationModeRecoveryRecord ResolveAfterRetirementProof(
            AxisSetOperationModeRecoveryRecord expected,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            uint retireRequestId,
            AxisSetOperationModeTerminalOutcomeProof retirementProof,
            DateTime updatedUtc)
        {
            EnsureExpectedCurrent(expected);
            if (expected.State != AxisSetOperationModeRecoveryState.TerminalOutcomeObserved
                || expected.TerminalOutcomeProof == null)
            {
                throw new InvalidOperationException(
                    "SetOperationMode recovery cannot resolve before durable terminal outcome proof exists.");
            }

            if (!expected.MatchesRecoveryKey(recoveryKey))
            {
                throw new InvalidOperationException(
                    "SetOperationMode retirement does not match the durable recovery key.");
            }

            if (retireRequestId == 0)
            {
                throw new InvalidOperationException(
                    "SetOperationMode retirement requires a nonzero request id.");
            }

            if (!expected.TerminalOutcomeProof.MatchesRetirement(retirementProof))
            {
                throw new InvalidOperationException(
                    "SetOperationMode retirement does not match the durable terminal proof.");
            }

            var next = Clone(
                expected,
                AxisSetOperationModeRecoveryState.Resolved,
                CheckedIncrement(expected.Revision),
                updatedUtc,
                expected.TerminalOutcomeProof,
                retireRequestId);
            PersistAndPublish(next);
            return next;
        }

        public void Dispose()
        {
        }

        private void EnsureExpectedCurrent(AxisSetOperationModeRecoveryRecord expected)
        {
            if (expected == null)
            {
                throw new ArgumentNullException("expected");
            }

            if (CurrentRecord == null
                || CurrentRecord.Identity != expected.Identity
                || CurrentRecord.Revision != expected.Revision
                || CurrentRecord.State != expected.State)
            {
                throw new InvalidOperationException(
                    "The SetOperationMode recovery record changed; stale copies cannot mutate durable state.");
            }
        }

        private void PersistAndPublish(AxisSetOperationModeRecoveryRecord record)
        {
            Persist(record);
            CurrentRecord = record;
        }

        private void Persist(AxisSetOperationModeRecoveryRecord record)
        {
            var bytes = Serialize(record);
            var temporaryPath = JournalFilePath + ".tmp";
            var backupPath = JournalFilePath + ".bak";
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

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
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(JournalFilePath))
                {
                    File.Replace(temporaryPath, JournalFilePath, backupPath, true);
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                }
                else
                {
                    File.Move(temporaryPath, JournalFilePath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        private void LoadCurrent()
        {
            if (!File.Exists(JournalFilePath))
            {
                CurrentRecord = null;
                return;
            }

            var info = new FileInfo(JournalFilePath);
            if (info.Length <= 0 || info.Length > MaximumFileLength)
            {
                throw new InvalidDataException("SetOperationMode recovery journal length is invalid.");
            }

            CurrentRecord = Deserialize(File.ReadAllBytes(JournalFilePath));
        }

        private static AxisSetOperationModeRecoveryRecord Clone(
            AxisSetOperationModeRecoveryRecord source,
            AxisSetOperationModeRecoveryState state,
            uint revision,
            DateTime updatedUtc,
            AxisSetOperationModeTerminalOutcomeProof proof,
            uint retirementRequestId)
        {
            return new AxisSetOperationModeRecoveryRecord(
                source.Identity,
                state,
                revision,
                source.EndpointIp,
                source.EndpointPort,
                source.AxisName,
                source.SchemaVersion,
                source.OriginalRequestId,
                source.DiagnosticsBuild,
                source.DiagnosticsBootId,
                source.MapRevision,
                source.ClientIntentId0,
                source.ClientIntentId1,
                source.ClientIntentId2,
                source.ClientIntentId3,
                source.AxisReference,
                source.RequestedModeRaw,
                source.TimeoutMilliseconds,
                source.Flags,
                source.CreatedUtc,
                updatedUtc,
                proof,
                retirementRequestId);
        }

        private static uint CheckedIncrement(uint value)
        {
            if (value == uint.MaxValue)
            {
                throw new InvalidOperationException("SetOperationMode recovery revision overflow.");
            }
            return value + 1U;
        }

        private static byte[] Serialize(AxisSetOperationModeRecoveryRecord record)
        {
            var lines = new System.Collections.Generic.List<string>();
            lines.Add(Magic);
            lines.Add("FormatVersion=" + FormatVersion.ToString(CultureInfo.InvariantCulture));
            lines.Add("Identity=" + record.Identity.ToString("N"));
            lines.Add("State=" + ((int)record.State).ToString(CultureInfo.InvariantCulture));
            lines.Add("Revision=" + record.Revision.ToString(CultureInfo.InvariantCulture));
            lines.Add("EndpointIp=" + EncodeString(record.EndpointIp));
            lines.Add("EndpointPort=" + record.EndpointPort.ToString(CultureInfo.InvariantCulture));
            lines.Add("AxisName=" + EncodeString(record.AxisName));
            lines.Add("SchemaVersion=" + record.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            lines.Add("OriginalRequestId=" + record.OriginalRequestId.ToString(CultureInfo.InvariantCulture));
            lines.Add("DiagnosticsBuild=" + record.DiagnosticsBuild.ToString(CultureInfo.InvariantCulture));
            lines.Add("DiagnosticsBootId=" + record.DiagnosticsBootId.ToString(CultureInfo.InvariantCulture));
            lines.Add("MapRevision=" + record.MapRevision.ToString(CultureInfo.InvariantCulture));
            lines.Add("ClientIntentId0=" + record.ClientIntentId0.ToString(CultureInfo.InvariantCulture));
            lines.Add("ClientIntentId1=" + record.ClientIntentId1.ToString(CultureInfo.InvariantCulture));
            lines.Add("ClientIntentId2=" + record.ClientIntentId2.ToString(CultureInfo.InvariantCulture));
            lines.Add("ClientIntentId3=" + record.ClientIntentId3.ToString(CultureInfo.InvariantCulture));
            lines.Add("AxisReference=" + record.AxisReference.ToString(CultureInfo.InvariantCulture));
            lines.Add("RequestedModeRaw=" + record.RequestedModeRaw.ToString(CultureInfo.InvariantCulture));
            lines.Add("TimeoutMilliseconds=" + record.TimeoutMilliseconds.ToString(CultureInfo.InvariantCulture));
            lines.Add("Flags=" + record.Flags.ToString(CultureInfo.InvariantCulture));
            lines.Add("CreatedUtcTicks=" + record.CreatedUtc.Ticks.ToString(CultureInfo.InvariantCulture));
            lines.Add("UpdatedUtcTicks=" + record.UpdatedUtc.Ticks.ToString(CultureInfo.InvariantCulture));
            lines.Add("RetirementRequestId=" + record.RetirementRequestId.ToString(CultureInfo.InvariantCulture));
            lines.Add("HasTerminalProof=" + (record.TerminalOutcomeProof != null ? "1" : "0"));

            if (record.TerminalOutcomeProof != null)
            {
                var proof = record.TerminalOutcomeProof;
                lines.Add("QueryRequestId=" + proof.QueryRequestId.ToString(CultureInfo.InvariantCulture));
                lines.Add("RecordState=" + ((ushort)proof.RecordState).ToString(CultureInfo.InvariantCulture));
                lines.Add("ObservedModeRaw=" + proof.ObservedModeRaw.ToString(CultureInfo.InvariantCulture));
                lines.Add("OriginalCommandStatus=" + proof.OriginalCommandStatus.ToString(CultureInfo.InvariantCulture));
                lines.Add("OriginalErrorId=" + proof.OriginalErrorId.ToString(CultureInfo.InvariantCulture));
                lines.Add("OriginalDetailCode=" + proof.OriginalDetailCode.ToString(CultureInfo.InvariantCulture));
                lines.Add("SdoExecutorToken=" + proof.SdoExecutorToken.ToString(CultureInfo.InvariantCulture));
                lines.Add("EvidenceFlags=" + ((uint)proof.EvidenceFlags).ToString(CultureInfo.InvariantCulture));
                lines.Add("StartCycle=" + proof.StartCycle.ToString(CultureInfo.InvariantCulture));
                lines.Add("CompletionCycle=" + proof.CompletionCycle.ToString(CultureInfo.InvariantCulture));
                lines.Add("NativeCommandState=" + proof.NativeCommandState.ToString(CultureInfo.InvariantCulture));
                lines.Add("RecordGeneration=" + proof.RecordGeneration.ToString(CultureInfo.InvariantCulture));
                lines.Add("PreviousModeRaw=" + proof.PreviousModeRaw.ToString(CultureInfo.InvariantCulture));
                lines.Add("QuarantineReason=" + proof.QuarantineReason.ToString(CultureInfo.InvariantCulture));
                lines.Add("Ds402StatusWord=" + proof.Ds402StatusWord.ToString(CultureInfo.InvariantCulture));
                lines.Add("ContextCheck=" + proof.ContextCheck.ToString(CultureInfo.InvariantCulture));
            }

            var payload = string.Join("\n", lines.ToArray()) + "\n";
            var payloadBytes = new UTF8Encoding(false).GetBytes(payload);
            var checksum = ComputeSha256Hex(payloadBytes);
            return new UTF8Encoding(false).GetBytes(
                payload + "SHA256=" + checksum + "\n");
        }

        private static AxisSetOperationModeRecoveryRecord Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException("SetOperationMode recovery journal bytes are invalid.");
            }

            var text = new UTF8Encoding(false, true).GetString(bytes);
            if (text.IndexOf('\r') >= 0 || !text.EndsWith("\n", StringComparison.Ordinal))
            {
                throw new InvalidDataException("SetOperationMode recovery journal must use canonical LF framing.");
            }

            var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines.Length < 4 || lines[lines.Length - 1].Length != 0)
            {
                throw new InvalidDataException("SetOperationMode recovery journal framing is invalid.");
            }

            var checksumLineIndex = lines.Length - 2;
            var checksumLine = lines[checksumLineIndex];
            const string checksumPrefix = "SHA256=";
            if (!checksumLine.StartsWith(checksumPrefix, StringComparison.Ordinal)
                || checksumLine.Length != checksumPrefix.Length + 64)
            {
                throw new InvalidDataException("SetOperationMode recovery journal checksum line is invalid.");
            }

            var payload = string.Join("\n", lines.Take(checksumLineIndex).ToArray()) + "\n";
            var payloadBytes = new UTF8Encoding(false).GetBytes(payload);
            var expectedChecksum = checksumLine.Substring(checksumPrefix.Length);
            var actualChecksum = ComputeSha256Hex(payloadBytes);
            if (!FixedTimeHexEquals(expectedChecksum, actualChecksum))
            {
                throw new InvalidDataException("SetOperationMode recovery journal checksum mismatch.");
            }

            var cursor = 0;
            RequireLiteral(lines, ref cursor, Magic);
            var version = ReadUInt(lines, ref cursor, "FormatVersion");
            if (version != FormatVersion)
            {
                throw new InvalidDataException("Unsupported SetOperationMode recovery journal format version.");
            }

            var identityText = ReadString(lines, ref cursor, "Identity");
            Guid identity;
            if (!Guid.TryParseExact(identityText, "N", out identity))
            {
                throw new InvalidDataException("SetOperationMode recovery journal identity is invalid.");
            }

            var stateValue = ReadInt(lines, ref cursor, "State");
            if (!Enum.IsDefined(typeof(AxisSetOperationModeRecoveryState), stateValue))
            {
                throw new InvalidDataException("SetOperationMode recovery state is invalid.");
            }

            var revision = ReadUInt(lines, ref cursor, "Revision");
            var endpointIp = DecodeString(ReadString(lines, ref cursor, "EndpointIp"));
            var endpointPort = ReadInt(lines, ref cursor, "EndpointPort");
            var axisName = DecodeString(ReadString(lines, ref cursor, "AxisName"));
            var schemaVersion = ReadUShort(lines, ref cursor, "SchemaVersion");
            var originalRequestId = ReadUInt(lines, ref cursor, "OriginalRequestId");
            var diagnosticsBuild = ReadUInt(lines, ref cursor, "DiagnosticsBuild");
            var diagnosticsBootId = ReadUInt(lines, ref cursor, "DiagnosticsBootId");
            var mapRevision = ReadUInt(lines, ref cursor, "MapRevision");
            var intent0 = ReadUInt(lines, ref cursor, "ClientIntentId0");
            var intent1 = ReadUInt(lines, ref cursor, "ClientIntentId1");
            var intent2 = ReadUInt(lines, ref cursor, "ClientIntentId2");
            var intent3 = ReadUInt(lines, ref cursor, "ClientIntentId3");
            var axisReference = ReadUShort(lines, ref cursor, "AxisReference");
            var requestedModeRaw = ReadSByte(lines, ref cursor, "RequestedModeRaw");
            var timeoutMilliseconds = ReadUInt(lines, ref cursor, "TimeoutMilliseconds");
            var flags = ReadUInt(lines, ref cursor, "Flags");
            var createdUtc = ReadUtc(lines, ref cursor, "CreatedUtcTicks");
            var updatedUtc = ReadUtc(lines, ref cursor, "UpdatedUtcTicks");
            var retirementRequestId = ReadUInt(lines, ref cursor, "RetirementRequestId");
            var hasProof = ReadInt(lines, ref cursor, "HasTerminalProof");
            if (hasProof != 0 && hasProof != 1)
            {
                throw new InvalidDataException("SetOperationMode terminal-proof marker is invalid.");
            }

            AxisSetOperationModeTerminalOutcomeProof proof = null;
            if (hasProof == 1)
            {
                proof = new AxisSetOperationModeTerminalOutcomeProof(
                    ReadUInt(lines, ref cursor, "QueryRequestId"),
                    (LMCAxisSetOperationModeOutcomeRecordState)ReadUShort(lines, ref cursor, "RecordState"),
                    ReadSByte(lines, ref cursor, "ObservedModeRaw"),
                    ReadUShort(lines, ref cursor, "OriginalCommandStatus"),
                    ReadShort(lines, ref cursor, "OriginalErrorId"),
                    ReadUInt(lines, ref cursor, "OriginalDetailCode"),
                    ReadUInt(lines, ref cursor, "SdoExecutorToken"),
                    (LMCAxisSetOperationModeEvidenceFlags)ReadUInt(lines, ref cursor, "EvidenceFlags"),
                    ReadUInt(lines, ref cursor, "StartCycle"),
                    ReadUInt(lines, ref cursor, "CompletionCycle"),
                    ReadUInt(lines, ref cursor, "NativeCommandState"),
                    ReadUInt(lines, ref cursor, "RecordGeneration"),
                    ReadSByte(lines, ref cursor, "PreviousModeRaw"),
                    ReadUInt(lines, ref cursor, "QuarantineReason"),
                    ReadUShort(lines, ref cursor, "Ds402StatusWord"),
                    ReadUInt(lines, ref cursor, "ContextCheck"),
                    requestedModeRaw);
            }

            if (cursor != checksumLineIndex)
            {
                throw new InvalidDataException("SetOperationMode recovery journal has trailing or missing fields.");
            }

            return new AxisSetOperationModeRecoveryRecord(
                identity,
                (AxisSetOperationModeRecoveryState)stateValue,
                revision,
                endpointIp,
                endpointPort,
                axisName,
                schemaVersion,
                originalRequestId,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                intent0,
                intent1,
                intent2,
                intent3,
                axisReference,
                requestedModeRaw,
                timeoutMilliseconds,
                flags,
                createdUtc,
                updatedUtc,
                proof,
                retirementRequestId);
        }

        private static string EncodeString(string value)
        {
            return Convert.ToBase64String(new UTF8Encoding(false).GetBytes(value));
        }

        private static string DecodeString(string value)
        {
            try
            {
                return new UTF8Encoding(false, true).GetString(Convert.FromBase64String(value));
            }
            catch (Exception error)
            {
                throw new InvalidDataException("SetOperationMode recovery journal string encoding is invalid.", error);
            }
        }

        private static string ComputeSha256Hex(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
            }
        }

        private static bool FixedTimeHexEquals(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < left.Length; index++)
            {
                difference |= char.ToUpperInvariant(left[index]) ^ char.ToUpperInvariant(right[index]);
            }
            return difference == 0;
        }

        private static void RequireLiteral(string[] lines, ref int cursor, string expected)
        {
            if (cursor >= lines.Length || !string.Equals(lines[cursor], expected, StringComparison.Ordinal))
            {
                throw new InvalidDataException("SetOperationMode recovery journal magic is invalid.");
            }
            cursor++;
        }

        private static string ReadString(string[] lines, ref int cursor, string name)
        {
            if (cursor >= lines.Length)
            {
                throw new InvalidDataException("SetOperationMode recovery journal is truncated.");
            }

            var prefix = name + "=";
            var line = lines[cursor++];
            if (!line.StartsWith(prefix, StringComparison.Ordinal))
            {
                throw new InvalidDataException("SetOperationMode recovery journal field order is invalid: " + name + ".");
            }
            return line.Substring(prefix.Length);
        }

        private static uint ReadUInt(string[] lines, ref int cursor, string name)
        {
            uint value;
            if (!uint.TryParse(ReadString(lines, ref cursor, name), NumberStyles.None, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidDataException("SetOperationMode recovery journal uint field is invalid: " + name + ".");
            }
            return value;
        }

        private static ushort ReadUShort(string[] lines, ref int cursor, string name)
        {
            ushort value;
            if (!ushort.TryParse(ReadString(lines, ref cursor, name), NumberStyles.None, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidDataException("SetOperationMode recovery journal ushort field is invalid: " + name + ".");
            }
            return value;
        }

        private static int ReadInt(string[] lines, ref int cursor, string name)
        {
            int value;
            if (!int.TryParse(ReadString(lines, ref cursor, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidDataException("SetOperationMode recovery journal int field is invalid: " + name + ".");
            }
            return value;
        }

        private static short ReadShort(string[] lines, ref int cursor, string name)
        {
            short value;
            if (!short.TryParse(ReadString(lines, ref cursor, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidDataException("SetOperationMode recovery journal short field is invalid: " + name + ".");
            }
            return value;
        }

        private static sbyte ReadSByte(string[] lines, ref int cursor, string name)
        {
            sbyte value;
            if (!sbyte.TryParse(ReadString(lines, ref cursor, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                throw new InvalidDataException("SetOperationMode recovery journal sbyte field is invalid: " + name + ".");
            }
            return value;
        }

        private static DateTime ReadUtc(string[] lines, ref int cursor, string name)
        {
            long ticks;
            if (!long.TryParse(ReadString(lines, ref cursor, name), NumberStyles.None, CultureInfo.InvariantCulture, out ticks)
                || ticks < DateTime.MinValue.Ticks
                || ticks > DateTime.MaxValue.Ticks)
            {
                throw new InvalidDataException("SetOperationMode recovery journal UTC field is invalid: " + name + ".");
            }
            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }
}

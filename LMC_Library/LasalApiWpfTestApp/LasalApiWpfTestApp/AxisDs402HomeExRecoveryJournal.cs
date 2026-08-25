using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum AxisDs402HomeExRecoveryState
    {
        ArmedBeforeDispatch = 1,
        RecoveryRequired = 2,
        Resolved = 3,
        TerminalOutcomeObserved = 4
    }

    internal sealed class AxisDs402HomeExTerminalOutcomeProof
    {
        internal AxisDs402HomeExTerminalOutcomeProof(
            uint queryRequestId,
            LMCAxisDs402HomeExOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            ushort ds402StatusWord,
            int actualPosition,
            int expectedFinalPosition,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration,
            LMCAxisDs402HomeExCleanupProofFlags cleanupProofFlags,
            uint sdoExecutorToken,
            int requestedPosition)
        {
            ValidateTerminal(
                recordState,
                originalCommandStatus,
                originalErrorId,
                originalDetailCode,
                actualPosition,
                expectedFinalPosition,
                startCycle,
                completionCycle,
                nativeCommandState,
                recordGeneration,
                cleanupProofFlags,
                sdoExecutorToken,
                requestedPosition);

            QueryRequestId = queryRequestId;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            Ds402StatusWord = ds402StatusWord;
            ActualPosition = actualPosition;
            ExpectedFinalPosition = expectedFinalPosition;
            StartCycle = startCycle;
            CompletionCycle = completionCycle;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
            CleanupProofFlags = cleanupProofFlags;
            SdoExecutorToken = sdoExecutorToken;
        }

        internal uint QueryRequestId { get; private set; }
        internal LMCAxisDs402HomeExOutcomeRecordState RecordState { get; private set; }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal ushort Ds402StatusWord { get; private set; }
        internal int ActualPosition { get; private set; }
        internal int ExpectedFinalPosition { get; private set; }
        internal uint StartCycle { get; private set; }
        internal uint CompletionCycle { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint RecordGeneration { get; private set; }
        internal LMCAxisDs402HomeExCleanupProofFlags CleanupProofFlags
        {
            get;
            private set;
        }
        internal uint SdoExecutorToken { get; private set; }

        internal static AxisDs402HomeExTerminalOutcomeProof FromOutcome(
            LMCAxisDs402HomeExOutcomeResult outcome)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }

            if (outcome.Response == null
                || !outcome.Response.IsSuccess
                || !outcome.IsTerminal)
            {
                throw new InvalidOperationException(
                    "Only a successful terminal HomeDS402Ex outcome query can become durable proof.");
            }

            return new AxisDs402HomeExTerminalOutcomeProof(
                outcome.QueryRequestId,
                outcome.RecordState,
                outcome.OriginalCommandStatus,
                outcome.OriginalErrorId,
                outcome.OriginalDetailCodeValue,
                outcome.Ds402StatusWord,
                outcome.ActualPosition,
                outcome.ExpectedFinalPosition,
                outcome.StartCycle,
                outcome.CompletionCycle,
                outcome.NativeCommandState,
                outcome.RecordGeneration,
                outcome.CleanupProofFlags,
                outcome.SdoExecutorToken,
                outcome.RecoveryKey.ExecutionPlan.Position);
        }

        internal static AxisDs402HomeExTerminalOutcomeProof FromRetirement(
            LMCAxisDs402HomeExOutcomeRetirementResult retirement)
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
                    "Only a successful exact-generation HomeDS402Ex retirement can resolve the journal.");
            }

            var outcome = retirement.TerminalOutcome;
            return new AxisDs402HomeExTerminalOutcomeProof(
                0,
                outcome.RecordState,
                outcome.OriginalCommandStatus,
                outcome.OriginalErrorId,
                outcome.OriginalDetailCodeValue,
                outcome.Ds402StatusWord,
                outcome.ActualPosition,
                outcome.ExpectedFinalPosition,
                outcome.StartCycle,
                outcome.CompletionCycle,
                outcome.NativeCommandState,
                outcome.RecordGeneration,
                outcome.CleanupProofFlags,
                outcome.SdoExecutorToken,
                outcome.RecoveryKey.ExecutionPlan.Position);
        }

        internal bool MatchesRetirement(
            AxisDs402HomeExTerminalOutcomeProof retirementProof)
        {
            return retirementProof != null
                && RecordState == retirementProof.RecordState
                && OriginalCommandStatus == retirementProof.OriginalCommandStatus
                && OriginalErrorId == retirementProof.OriginalErrorId
                && OriginalDetailCode == retirementProof.OriginalDetailCode
                && Ds402StatusWord == retirementProof.Ds402StatusWord
                && ActualPosition == retirementProof.ActualPosition
                && ExpectedFinalPosition == retirementProof.ExpectedFinalPosition
                && StartCycle == retirementProof.StartCycle
                && CompletionCycle == retirementProof.CompletionCycle
                && NativeCommandState == retirementProof.NativeCommandState
                && RecordGeneration == retirementProof.RecordGeneration
                && CleanupProofFlags == retirementProof.CleanupProofFlags
                && SdoExecutorToken == retirementProof.SdoExecutorToken;
        }

        private static void ValidateTerminal(
            LMCAxisDs402HomeExOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            int actualPosition,
            int expectedFinalPosition,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration,
            LMCAxisDs402HomeExCleanupProofFlags cleanupProofFlags,
            uint sdoExecutorToken,
            int requestedPosition)
        {
            if (recordState == LMCAxisDs402HomeExOutcomeRecordState.Running)
            {
                throw new InvalidOperationException(
                    "A running HomeDS402Ex outcome is not durable terminal proof.");
            }

            if (startCycle == 0
                || completionCycle < startCycle
                || recordGeneration == 0
                || nativeCommandState != 0
                || cleanupProofFlags
                    != LMCAxisDs402HomeExCleanupProofFlags.RequiredForSafeTerminal
                || sdoExecutorToken == 0)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex terminal proof requires exact generation, completed cleanup, and reusable SDO evidence.");
            }

            if (recordState == LMCAxisDs402HomeExOutcomeRecordState.Succeeded)
            {
                int requiredFinalPosition;
                try
                {
                    requiredFinalPosition = checked(-requestedPosition);
                }
                catch (OverflowException error)
                {
                    throw new InvalidOperationException(
                        "HomeDS402Ex terminal proof position cannot be represented.",
                        error);
                }

                if (originalCommandStatus != 0
                    || originalErrorId != 0
                    || originalDetailCode != 0
                    || expectedFinalPosition != requiredFinalPosition
                    || actualPosition != expectedFinalPosition)
                {
                    throw new InvalidOperationException(
                        "Successful HomeDS402Ex durable proof requires exact final-position readback and zero original error status.");
                }
                return;
            }

            if (recordState == LMCAxisDs402HomeExOutcomeRecordState.Aborted)
            {
                if (originalCommandStatus != 1
                    || originalErrorId != -31000
                    || originalDetailCode != 59u)
                {
                    throw new InvalidOperationException(
                        "Aborted HomeDS402Ex durable proof has an invalid terminal error tuple.");
                }
                return;
            }

            if (recordState != LMCAxisDs402HomeExOutcomeRecordState.Failed
                || originalCommandStatus != 1
                || originalErrorId != -31000
                || (originalDetailCode != 58u && originalDetailCode != 61u))
            {
                throw new InvalidOperationException(
                    "Failed HomeDS402Ex durable proof has an invalid terminal error tuple.");
            }
        }
    }

    internal sealed class AxisDs402HomeExRecoveryRecord
    {
        internal AxisDs402HomeExRecoveryRecord(
            Guid identity,
            AxisDs402HomeExRecoveryState state,
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
            int homingMethod,
            int position,
            int detectionVelocityLimit,
            int acceleration,
            int velocityHigh,
            int velocityLow,
            int distanceLimit,
            int torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            uint overallTimeoutMilliseconds,
            uint detectionTimeoutMilliseconds,
            DateTime createdUtc,
            DateTime updatedUtc,
            AxisDs402HomeExTerminalOutcomeProof terminalOutcomeProof,
            uint retirementRequestId)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Recovery journal identity must not be empty.",
                    "identity");
            }
            if (!Enum.IsDefined(typeof(AxisDs402HomeExRecoveryState), state))
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
                throw new ArgumentException(
                    "Axis name is required and must be bounded.",
                    "axisName");
            }

            var key = LMCAxisDs402HomeExRecovery.Rehydrate(
                schemaVersion,
                originalRequestId,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                new LMCAxisDs402HomeExClientIntentId(
                    clientIntentId0,
                    clientIntentId1,
                    clientIntentId2,
                    clientIntentId3),
                axisReference,
                homingMethod,
                position,
                detectionVelocityLimit,
                acceleration,
                velocityHigh,
                velocityLow,
                distanceLimit,
                torqueLimit,
                bufferMode,
                overallTimeoutMilliseconds,
                detectionTimeoutMilliseconds,
                new byte[LMCAxisDs402HomeExExecutionPlan.SpareLength]);

            ValidateUtc(createdUtc, "createdUtc");
            ValidateUtc(updatedUtc, "updatedUtc");
            if (updatedUtc < createdUtc)
            {
                throw new ArgumentException(
                    "UpdatedUtc cannot precede CreatedUtc.",
                    "updatedUtc");
            }

            if (state == AxisDs402HomeExRecoveryState.TerminalOutcomeObserved
                && terminalOutcomeProof == null)
            {
                throw new InvalidDataException(
                    "TerminalOutcomeObserved requires HomeDS402Ex terminal proof.");
            }

            if (state == AxisDs402HomeExRecoveryState.Resolved)
            {
                if (terminalOutcomeProof == null || retirementRequestId == 0)
                {
                    throw new InvalidDataException(
                        "Resolved HomeDS402Ex recovery requires terminal and retirement proof.");
                }
            }
            else if (retirementRequestId != 0)
            {
                throw new InvalidDataException(
                    "Only a resolved HomeDS402Ex record may contain a retirement request id.");
            }

            if ((state == AxisDs402HomeExRecoveryState.ArmedBeforeDispatch
                    || state == AxisDs402HomeExRecoveryState.RecoveryRequired)
                && terminalOutcomeProof != null)
            {
                throw new InvalidDataException(
                    "Pre-terminal HomeDS402Ex recovery states cannot contain terminal proof.");
            }

            Identity = identity;
            State = state;
            Revision = revision;
            EndpointPort = endpointPort;
            AxisName = axisName.Trim();
            SchemaVersion = key.SchemaVersion;
            OriginalRequestId = key.OriginalRequestId;
            DiagnosticsBuild = key.DiagnosticsBuild;
            DiagnosticsBootId = key.DiagnosticsBootId;
            MapRevision = key.MapRevision;
            ClientIntentId0 = key.ClientIntentId.Word0;
            ClientIntentId1 = key.ClientIntentId.Word1;
            ClientIntentId2 = key.ClientIntentId.Word2;
            ClientIntentId3 = key.ClientIntentId.Word3;
            AxisReference = key.AxisReference;
            HomingMethod = key.ExecutionPlan.HomingMethod;
            Position = key.ExecutionPlan.Position;
            DetectionVelocityLimit = key.ExecutionPlan.DetectionVelocityLimit;
            Acceleration = key.ExecutionPlan.Acceleration;
            VelocityHigh = key.ExecutionPlan.VelocityHigh;
            VelocityLow = key.ExecutionPlan.VelocityLow;
            DistanceLimit = key.ExecutionPlan.DistanceLimit;
            TorqueLimit = key.ExecutionPlan.TorqueLimit;
            BufferMode = key.ExecutionPlan.BufferMode;
            OverallTimeoutMilliseconds = key.ExecutionPlan.OverallTimeoutMilliseconds;
            DetectionTimeoutMilliseconds = key.ExecutionPlan.DetectionTimeoutMilliseconds;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            TerminalOutcomeProof = terminalOutcomeProof;
            RetirementRequestId = retirementRequestId;
        }

        internal Guid Identity { get; private set; }
        internal AxisDs402HomeExRecoveryState State { get; private set; }
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
        internal int HomingMethod { get; private set; }
        internal int Position { get; private set; }
        internal int DetectionVelocityLimit { get; private set; }
        internal int Acceleration { get; private set; }
        internal int VelocityHigh { get; private set; }
        internal int VelocityLow { get; private set; }
        internal int DistanceLimit { get; private set; }
        internal int TorqueLimit { get; private set; }
        internal LMCDs402HomeBufferMode BufferMode { get; private set; }
        internal uint OverallTimeoutMilliseconds { get; private set; }
        internal uint DetectionTimeoutMilliseconds { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }
        internal AxisDs402HomeExTerminalOutcomeProof TerminalOutcomeProof
        {
            get;
            private set;
        }
        internal uint RetirementRequestId { get; private set; }
        internal bool IsActive { get { return State != AxisDs402HomeExRecoveryState.Resolved; } }
        internal bool HasTerminalOutcomeProof { get { return TerminalOutcomeProof != null; } }

        internal LMCAxisDs402HomeExRecoveryKey ToRecoveryKey()
        {
            return LMCAxisDs402HomeExRecovery.Rehydrate(
                SchemaVersion,
                OriginalRequestId,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                new LMCAxisDs402HomeExClientIntentId(
                    ClientIntentId0,
                    ClientIntentId1,
                    ClientIntentId2,
                    ClientIntentId3),
                AxisReference,
                HomingMethod,
                Position,
                DetectionVelocityLimit,
                Acceleration,
                VelocityHigh,
                VelocityLow,
                DistanceLimit,
                TorqueLimit,
                BufferMode,
                OverallTimeoutMilliseconds,
                DetectionTimeoutMilliseconds,
                new byte[LMCAxisDs402HomeExExecutionPlan.SpareLength]);
        }

        internal bool MatchesRecoveryKey(LMCAxisDs402HomeExRecoveryKey key)
        {
            if (key == null)
            {
                return false;
            }

            var plan = key.ExecutionPlan;
            if (plan == null)
            {
                return false;
            }

            var spare = plan.Spare;
            for (var index = 0; index < spare.Length; index++)
            {
                if (spare[index] != 0)
                {
                    return false;
                }
            }

            return SchemaVersion == key.SchemaVersion
                && OriginalRequestId == key.OriginalRequestId
                && DiagnosticsBuild == key.DiagnosticsBuild
                && DiagnosticsBootId == key.DiagnosticsBootId
                && MapRevision == key.MapRevision
                && ClientIntentId0 == key.ClientIntentId.Word0
                && ClientIntentId1 == key.ClientIntentId.Word1
                && ClientIntentId2 == key.ClientIntentId.Word2
                && ClientIntentId3 == key.ClientIntentId.Word3
                && AxisReference == key.AxisReference
                && HomingMethod == plan.HomingMethod
                && Position == plan.Position
                && DetectionVelocityLimit == plan.DetectionVelocityLimit
                && Acceleration == plan.Acceleration
                && VelocityHigh == plan.VelocityHigh
                && VelocityLow == plan.VelocityLow
                && DistanceLimit == plan.DistanceLimit
                && TorqueLimit == plan.TorqueLimit
                && BufferMode == plan.BufferMode
                && OverallTimeoutMilliseconds == plan.OverallTimeoutMilliseconds
                && DetectionTimeoutMilliseconds == plan.DetectionTimeoutMilliseconds;
        }

        private static string NormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint) || endpoint.Length > 255)
            {
                throw new ArgumentException(
                    "Endpoint is required and must be bounded.",
                    "endpoint");
            }

            var trimmed = endpoint.Trim();
            IPAddress address;
            if (IPAddress.TryParse(trimmed, out address))
            {
                return address.ToString();
            }

            if (Uri.CheckHostName(trimmed) == UriHostNameType.Unknown)
            {
                throw new ArgumentException(
                    "Endpoint must be an IP address or valid host name.",
                    "endpoint");
            }
            return trimmed.ToLowerInvariant();
        }

        private static void ValidateUtc(DateTime value, string argumentName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Recovery journal timestamps must be UTC.",
                    argumentName);
            }
        }
    }

    internal sealed class AxisDs402HomeExRecoveryJournal : IDisposable
    {
        private const string Magic = "ELMOAH4EX1";
        private const uint FormatVersion = 1;
        private const int MaximumFileLength = 16384;
        private readonly string directoryPath;

        private AxisDs402HomeExRecoveryJournal(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException(
                    "Recovery journal directory is required.",
                    "directoryPath");
            }

            this.directoryPath = Path.GetFullPath(directoryPath);
            Directory.CreateDirectory(this.directoryPath);
            JournalFilePath = Path.Combine(
                this.directoryPath,
                "axis-ds402-home-ex-recovery.journal");
            LoadCurrent();

            if (CurrentRecord != null
                && CurrentRecord.State
                    == AxisDs402HomeExRecoveryState.ArmedBeforeDispatch)
            {
                CurrentRecord = Clone(
                    CurrentRecord,
                    AxisDs402HomeExRecoveryState.RecoveryRequired,
                    CheckedIncrement(CurrentRecord.Revision),
                    CurrentRecord.UpdatedUtc,
                    null,
                    0);
                Persist(CurrentRecord);
            }
        }

        internal string JournalFilePath { get; private set; }
        internal AxisDs402HomeExRecoveryRecord CurrentRecord { get; private set; }
        internal bool HasActiveRecord
        {
            get { return CurrentRecord != null && CurrentRecord.IsActive; }
        }

        internal static AxisDs402HomeExRecoveryJournal Open(string directoryPath)
        {
            return new AxisDs402HomeExRecoveryJournal(directoryPath);
        }

        internal static string GetDefaultDirectoryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Elmo",
                "LasalMotionControlApiExample",
                "AxisDs402HomeExRecoveryJournal",
                "v1");
        }

        internal AxisDs402HomeExRecoveryRecord ArmBeforeDispatch(
            Guid identity,
            string endpointIp,
            int endpointPort,
            string axisName,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            DateTime createdUtc)
        {
            if (HasActiveRecord)
            {
                throw new InvalidOperationException(
                    "An unresolved HomeDS402Ex recovery record already exists.");
            }
            if (recoveryKey == null)
            {
                throw new ArgumentNullException("recoveryKey");
            }

            var plan = recoveryKey.ExecutionPlan;
            var record = new AxisDs402HomeExRecoveryRecord(
                identity,
                AxisDs402HomeExRecoveryState.ArmedBeforeDispatch,
                1,
                endpointIp,
                endpointPort,
                axisName,
                recoveryKey.SchemaVersion,
                recoveryKey.OriginalRequestId,
                recoveryKey.DiagnosticsBuild,
                recoveryKey.DiagnosticsBootId,
                recoveryKey.MapRevision,
                recoveryKey.ClientIntentId.Word0,
                recoveryKey.ClientIntentId.Word1,
                recoveryKey.ClientIntentId.Word2,
                recoveryKey.ClientIntentId.Word3,
                recoveryKey.AxisReference,
                plan.HomingMethod,
                plan.Position,
                plan.DetectionVelocityLimit,
                plan.Acceleration,
                plan.VelocityHigh,
                plan.VelocityLow,
                plan.DistanceLimit,
                plan.TorqueLimit,
                plan.BufferMode,
                plan.OverallTimeoutMilliseconds,
                plan.DetectionTimeoutMilliseconds,
                createdUtc,
                createdUtc,
                null,
                0);
            PersistAndPublish(record);
            return record;
        }

        internal AxisDs402HomeExRecoveryRecord PromoteToRecoveryRequired(
            AxisDs402HomeExRecoveryRecord expected,
            DateTime updatedUtc)
        {
            EnsureExpectedCurrent(expected);
            if (expected.State != AxisDs402HomeExRecoveryState.ArmedBeforeDispatch)
            {
                throw new InvalidOperationException(
                    "Only an armed HomeDS402HomeEx record can be promoted to recovery-required.");
            }

            var next = Clone(
                expected,
                AxisDs402HomeExRecoveryState.RecoveryRequired,
                CheckedIncrement(expected.Revision),
                updatedUtc,
                null,
                0);
            PersistAndPublish(next);
            return next;
        }

        internal AxisDs402HomeExRecoveryRecord RecordTerminalOutcome(
            AxisDs402HomeExRecoveryRecord expected,
            LMCAxisDs402HomeExOutcomeResult outcome,
            DateTime updatedUtc)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }

            return RecordTerminalOutcomeProof(
                expected,
                outcome.RecoveryKey,
                AxisDs402HomeExTerminalOutcomeProof.FromOutcome(outcome),
                updatedUtc);
        }

        internal AxisDs402HomeExRecoveryRecord RecordTerminalOutcomeProof(
            AxisDs402HomeExRecoveryRecord expected,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            AxisDs402HomeExTerminalOutcomeProof proof,
            DateTime updatedUtc)
        {
            EnsureExpectedCurrent(expected);
            if (expected.State != AxisDs402HomeExRecoveryState.ArmedBeforeDispatch
                && expected.State
                    != AxisDs402HomeExRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    "Terminal HomeDS402Ex proof can only advance an unresolved pre-terminal record.");
            }
            if (!expected.MatchesRecoveryKey(recoveryKey))
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex terminal outcome does not match the durable recovery key.");
            }
            if (proof == null)
            {
                throw new ArgumentNullException("proof");
            }

            var next = Clone(
                expected,
                AxisDs402HomeExRecoveryState.TerminalOutcomeObserved,
                CheckedIncrement(expected.Revision),
                updatedUtc,
                proof,
                0);
            PersistAndPublish(next);
            return next;
        }

        internal AxisDs402HomeExRecoveryRecord ResolveAfterRetirement(
            AxisDs402HomeExRecoveryRecord expected,
            LMCAxisDs402HomeExOutcomeRetirementResult retirement,
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
                AxisDs402HomeExTerminalOutcomeProof.FromRetirement(retirement),
                updatedUtc);
        }

        internal AxisDs402HomeExRecoveryRecord ResolveAfterRetirementProof(
            AxisDs402HomeExRecoveryRecord expected,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            uint retireRequestId,
            AxisDs402HomeExTerminalOutcomeProof retirementProof,
            DateTime updatedUtc)
        {
            EnsureExpectedCurrent(expected);
            if (expected.State
                    != AxisDs402HomeExRecoveryState.TerminalOutcomeObserved
                || expected.TerminalOutcomeProof == null)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex recovery cannot resolve before durable terminal proof exists.");
            }
            if (!expected.MatchesRecoveryKey(recoveryKey))
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex retirement does not match the durable recovery key.");
            }
            if (retireRequestId == 0)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex retirement requires a nonzero request id.");
            }
            if (!expected.TerminalOutcomeProof.MatchesRetirement(retirementProof))
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex retirement does not match the durable terminal proof.");
            }

            var next = Clone(
                expected,
                AxisDs402HomeExRecoveryState.Resolved,
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

        private void EnsureExpectedCurrent(AxisDs402HomeExRecoveryRecord expected)
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
                    "The HomeDS402Ex recovery record changed; stale copies cannot mutate durable state.");
            }
        }

        private void PersistAndPublish(AxisDs402HomeExRecoveryRecord record)
        {
            Persist(record);
            CurrentRecord = record;
        }

        private void Persist(AxisDs402HomeExRecoveryRecord record)
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
                    File.Replace(
                        temporaryPath,
                        JournalFilePath,
                        backupPath,
                        true);
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
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal length is invalid.");
            }
            CurrentRecord = Deserialize(File.ReadAllBytes(JournalFilePath));
        }

        private static AxisDs402HomeExRecoveryRecord Clone(
            AxisDs402HomeExRecoveryRecord source,
            AxisDs402HomeExRecoveryState state,
            uint revision,
            DateTime updatedUtc,
            AxisDs402HomeExTerminalOutcomeProof proof,
            uint retirementRequestId)
        {
            return new AxisDs402HomeExRecoveryRecord(
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
                source.HomingMethod,
                source.Position,
                source.DetectionVelocityLimit,
                source.Acceleration,
                source.VelocityHigh,
                source.VelocityLow,
                source.DistanceLimit,
                source.TorqueLimit,
                source.BufferMode,
                source.OverallTimeoutMilliseconds,
                source.DetectionTimeoutMilliseconds,
                source.CreatedUtc,
                updatedUtc,
                proof,
                retirementRequestId);
        }

        private static uint CheckedIncrement(uint value)
        {
            if (value == uint.MaxValue)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex recovery revision overflow.");
            }
            return value + 1U;
        }

        private static byte[] Serialize(AxisDs402HomeExRecoveryRecord record)
        {
            var lines = new List<string>();
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
            lines.Add("HomingMethod=" + record.HomingMethod.ToString(CultureInfo.InvariantCulture));
            lines.Add("Position=" + record.Position.ToString(CultureInfo.InvariantCulture));
            lines.Add("DetectionVelocityLimit=" + record.DetectionVelocityLimit.ToString(CultureInfo.InvariantCulture));
            lines.Add("Acceleration=" + record.Acceleration.ToString(CultureInfo.InvariantCulture));
            lines.Add("VelocityHigh=" + record.VelocityHigh.ToString(CultureInfo.InvariantCulture));
            lines.Add("VelocityLow=" + record.VelocityLow.ToString(CultureInfo.InvariantCulture));
            lines.Add("DistanceLimit=" + record.DistanceLimit.ToString(CultureInfo.InvariantCulture));
            lines.Add("TorqueLimit=" + record.TorqueLimit.ToString(CultureInfo.InvariantCulture));
            lines.Add("BufferMode=" + ((ushort)record.BufferMode).ToString(CultureInfo.InvariantCulture));
            lines.Add("OverallTimeoutMilliseconds=" + record.OverallTimeoutMilliseconds.ToString(CultureInfo.InvariantCulture));
            lines.Add("DetectionTimeoutMilliseconds=" + record.DetectionTimeoutMilliseconds.ToString(CultureInfo.InvariantCulture));
            lines.Add("CreatedUtcTicks=" + record.CreatedUtc.Ticks.ToString(CultureInfo.InvariantCulture));
            lines.Add("UpdatedUtcTicks=" + record.UpdatedUtc.Ticks.ToString(CultureInfo.InvariantCulture));
            lines.Add("RetirementRequestId=" + record.RetirementRequestId.ToString(CultureInfo.InvariantCulture));
            lines.Add("HasTerminalProof=" + (record.TerminalOutcomeProof != null ? "1" : "0"));

            if (record.TerminalOutcomeProof != null)
            {
                var proof = record.TerminalOutcomeProof;
                lines.Add("QueryRequestId=" + proof.QueryRequestId.ToString(CultureInfo.InvariantCulture));
                lines.Add("RecordState=" + ((ushort)proof.RecordState).ToString(CultureInfo.InvariantCulture));
                lines.Add("OriginalCommandStatus=" + proof.OriginalCommandStatus.ToString(CultureInfo.InvariantCulture));
                lines.Add("OriginalErrorId=" + proof.OriginalErrorId.ToString(CultureInfo.InvariantCulture));
                lines.Add("OriginalDetailCode=" + proof.OriginalDetailCode.ToString(CultureInfo.InvariantCulture));
                lines.Add("Ds402StatusWord=" + proof.Ds402StatusWord.ToString(CultureInfo.InvariantCulture));
                lines.Add("ActualPosition=" + proof.ActualPosition.ToString(CultureInfo.InvariantCulture));
                lines.Add("ExpectedFinalPosition=" + proof.ExpectedFinalPosition.ToString(CultureInfo.InvariantCulture));
                lines.Add("StartCycle=" + proof.StartCycle.ToString(CultureInfo.InvariantCulture));
                lines.Add("CompletionCycle=" + proof.CompletionCycle.ToString(CultureInfo.InvariantCulture));
                lines.Add("NativeCommandState=" + proof.NativeCommandState.ToString(CultureInfo.InvariantCulture));
                lines.Add("RecordGeneration=" + proof.RecordGeneration.ToString(CultureInfo.InvariantCulture));
                lines.Add("CleanupProofFlags=" + ((uint)proof.CleanupProofFlags).ToString(CultureInfo.InvariantCulture));
                lines.Add("SdoExecutorToken=" + proof.SdoExecutorToken.ToString(CultureInfo.InvariantCulture));
            }

            var payload = string.Join("\n", lines.ToArray()) + "\n";
            var payloadBytes = new UTF8Encoding(false).GetBytes(payload);
            var checksum = ComputeSha256Hex(payloadBytes);
            return new UTF8Encoding(false).GetBytes(
                payload + "SHA256=" + checksum + "\n");
        }

        private static AxisDs402HomeExRecoveryRecord Deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal bytes are invalid.");
            }

            var text = new UTF8Encoding(false, true).GetString(bytes);
            if (text.IndexOf('\r') >= 0
                || !text.EndsWith("\n", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal must use canonical LF framing.");
            }

            var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines.Length < 4 || lines[lines.Length - 1].Length != 0)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal framing is invalid.");
            }

            var checksumLineIndex = lines.Length - 2;
            var checksumLine = lines[checksumLineIndex];
            const string checksumPrefix = "SHA256=";
            if (!checksumLine.StartsWith(checksumPrefix, StringComparison.Ordinal)
                || checksumLine.Length != checksumPrefix.Length + 64)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal checksum line is invalid.");
            }

            var payload = string.Join(
                "\n",
                lines.Take(checksumLineIndex).ToArray()) + "\n";
            var expectedChecksum = checksumLine.Substring(checksumPrefix.Length);
            var actualChecksum = ComputeSha256Hex(
                new UTF8Encoding(false).GetBytes(payload));
            if (!FixedTimeHexEquals(expectedChecksum, actualChecksum))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal checksum mismatch.");
            }

            var cursor = 0;
            RequireLiteral(lines, ref cursor, Magic);
            var version = ReadUInt(lines, ref cursor, "FormatVersion");
            if (version != FormatVersion)
            {
                throw new InvalidDataException(
                    "Unsupported HomeDS402Ex recovery journal format version.");
            }

            Guid identity;
            if (!Guid.TryParseExact(
                    ReadString(lines, ref cursor, "Identity"),
                    "N",
                    out identity))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal identity is invalid.");
            }

            var stateValue = ReadInt(lines, ref cursor, "State");
            if (!Enum.IsDefined(typeof(AxisDs402HomeExRecoveryState), stateValue))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery state is invalid.");
            }

            var revision = ReadUInt(lines, ref cursor, "Revision");
            var endpointIp = DecodeString(
                ReadString(lines, ref cursor, "EndpointIp"));
            var endpointPort = ReadInt(lines, ref cursor, "EndpointPort");
            var axisName = DecodeString(
                ReadString(lines, ref cursor, "AxisName"));
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
            var homingMethod = ReadInt(lines, ref cursor, "HomingMethod");
            var position = ReadInt(lines, ref cursor, "Position");
            var detectionVelocityLimit = ReadInt(lines, ref cursor, "DetectionVelocityLimit");
            var acceleration = ReadInt(lines, ref cursor, "Acceleration");
            var velocityHigh = ReadInt(lines, ref cursor, "VelocityHigh");
            var velocityLow = ReadInt(lines, ref cursor, "VelocityLow");
            var distanceLimit = ReadInt(lines, ref cursor, "DistanceLimit");
            var torqueLimit = ReadInt(lines, ref cursor, "TorqueLimit");
            var bufferMode = (LMCDs402HomeBufferMode)ReadUShort(
                lines,
                ref cursor,
                "BufferMode");
            var overallTimeoutMilliseconds = ReadUInt(
                lines,
                ref cursor,
                "OverallTimeoutMilliseconds");
            var detectionTimeoutMilliseconds = ReadUInt(
                lines,
                ref cursor,
                "DetectionTimeoutMilliseconds");
            var createdUtc = ReadUtc(lines, ref cursor, "CreatedUtcTicks");
            var updatedUtc = ReadUtc(lines, ref cursor, "UpdatedUtcTicks");
            var retirementRequestId = ReadUInt(
                lines,
                ref cursor,
                "RetirementRequestId");
            var hasProof = ReadInt(lines, ref cursor, "HasTerminalProof");
            if (hasProof != 0 && hasProof != 1)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex terminal-proof marker is invalid.");
            }

            AxisDs402HomeExTerminalOutcomeProof proof = null;
            if (hasProof == 1)
            {
                proof = new AxisDs402HomeExTerminalOutcomeProof(
                    ReadUInt(lines, ref cursor, "QueryRequestId"),
                    (LMCAxisDs402HomeExOutcomeRecordState)ReadUShort(
                        lines,
                        ref cursor,
                        "RecordState"),
                    ReadUShort(lines, ref cursor, "OriginalCommandStatus"),
                    ReadShort(lines, ref cursor, "OriginalErrorId"),
                    ReadUInt(lines, ref cursor, "OriginalDetailCode"),
                    ReadUShort(lines, ref cursor, "Ds402StatusWord"),
                    ReadInt(lines, ref cursor, "ActualPosition"),
                    ReadInt(lines, ref cursor, "ExpectedFinalPosition"),
                    ReadUInt(lines, ref cursor, "StartCycle"),
                    ReadUInt(lines, ref cursor, "CompletionCycle"),
                    ReadUInt(lines, ref cursor, "NativeCommandState"),
                    ReadUInt(lines, ref cursor, "RecordGeneration"),
                    (LMCAxisDs402HomeExCleanupProofFlags)ReadUInt(
                        lines,
                        ref cursor,
                        "CleanupProofFlags"),
                    ReadUInt(lines, ref cursor, "SdoExecutorToken"),
                    position);
            }

            if (cursor != checksumLineIndex)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal has trailing or missing fields.");
            }

            return new AxisDs402HomeExRecoveryRecord(
                identity,
                (AxisDs402HomeExRecoveryState)stateValue,
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
                homingMethod,
                position,
                detectionVelocityLimit,
                acceleration,
                velocityHigh,
                velocityLow,
                distanceLimit,
                torqueLimit,
                bufferMode,
                overallTimeoutMilliseconds,
                detectionTimeoutMilliseconds,
                createdUtc,
                updatedUtc,
                proof,
                retirementRequestId);
        }

        private static string EncodeString(string value)
        {
            return Convert.ToBase64String(
                new UTF8Encoding(false).GetBytes(value));
        }

        private static string DecodeString(string value)
        {
            try
            {
                return new UTF8Encoding(false, true).GetString(
                    Convert.FromBase64String(value));
            }
            catch (Exception error)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal string encoding is invalid.",
                    error);
            }
        }

        private static string ComputeSha256Hex(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", string.Empty);
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
                difference |= char.ToUpperInvariant(left[index])
                    ^ char.ToUpperInvariant(right[index]);
            }
            return difference == 0;
        }

        private static void RequireLiteral(
            string[] lines,
            ref int cursor,
            string expected)
        {
            if (cursor >= lines.Length
                || !string.Equals(
                    lines[cursor],
                    expected,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal magic is invalid.");
            }
            cursor++;
        }

        private static string ReadString(
            string[] lines,
            ref int cursor,
            string name)
        {
            if (cursor >= lines.Length)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal is truncated.");
            }
            var prefix = name + "=";
            var line = lines[cursor++];
            if (!line.StartsWith(prefix, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal field order is invalid: "
                    + name + ".");
            }
            return line.Substring(prefix.Length);
        }

        private static uint ReadUInt(
            string[] lines,
            ref int cursor,
            string name)
        {
            uint value;
            if (!uint.TryParse(
                    ReadString(lines, ref cursor, name),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal uint field is invalid: "
                    + name + ".");
            }
            return value;
        }

        private static ushort ReadUShort(
            string[] lines,
            ref int cursor,
            string name)
        {
            ushort value;
            if (!ushort.TryParse(
                    ReadString(lines, ref cursor, name),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal ushort field is invalid: "
                    + name + ".");
            }
            return value;
        }

        private static int ReadInt(
            string[] lines,
            ref int cursor,
            string name)
        {
            int value;
            if (!int.TryParse(
                    ReadString(lines, ref cursor, name),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal int field is invalid: "
                    + name + ".");
            }
            return value;
        }

        private static short ReadShort(
            string[] lines,
            ref int cursor,
            string name)
        {
            short value;
            if (!short.TryParse(
                    ReadString(lines, ref cursor, name),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal short field is invalid: "
                    + name + ".");
            }
            return value;
        }

        private static DateTime ReadUtc(
            string[] lines,
            ref int cursor,
            string name)
        {
            long ticks;
            if (!long.TryParse(
                    ReadString(lines, ref cursor, name),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out ticks)
                || ticks < DateTime.MinValue.Ticks
                || ticks > DateTime.MaxValue.Ticks)
            {
                throw new InvalidDataException(
                    "HomeDS402Ex recovery journal UTC field is invalid: "
                    + name + ".");
            }
            return new DateTime(ticks, DateTimeKind.Utc);
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum AxisSetPositionRecoveryState
    {
        ArmedBeforeDispatch = 1,
        RecoveryRequired = 2,
        Resolved = 3,
        TerminalOutcomeObserved = 4
    }

    internal sealed class AxisSetPositionTerminalOutcomeProof
    {
        internal AxisSetPositionTerminalOutcomeProof(
            uint queryRequestId,
            LMCAxisSetPositionOutcomeRecordState recordState,
            int appliedPosition,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint nativeCommandState,
            uint recordGeneration)
        {
            if (queryRequestId == 0)
            {
                throw new ArgumentOutOfRangeException("queryRequestId");
            }
            if (recordState
                    != LMCAxisSetPositionOutcomeRecordState.Succeeded
                && recordState
                    != LMCAxisSetPositionOutcomeRecordState.Rejected)
            {
                throw new ArgumentOutOfRangeException("recordState");
            }
            if (recordGeneration == 0)
            {
                throw new ArgumentOutOfRangeException("recordGeneration");
            }

            QueryRequestId = queryRequestId;
            RecordState = recordState;
            AppliedPosition = appliedPosition;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
        }

        internal uint QueryRequestId { get; private set; }
        internal LMCAxisSetPositionOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        internal int AppliedPosition { get; private set; }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint RecordGeneration { get; private set; }

        internal AxisSetPositionTerminalOutcomeProof Copy()
        {
            return new AxisSetPositionTerminalOutcomeProof(
                QueryRequestId,
                RecordState,
                AppliedPosition,
                OriginalCommandStatus,
                OriginalErrorId,
                OriginalDetailCode,
                NativeCommandState,
                RecordGeneration);
        }

        internal bool Matches(
            LMCAxisSetPositionOutcomeRetirementResult retirementResult)
        {
            return retirementResult != null
                && retirementResult.RecordState == RecordState
                && retirementResult.AppliedPosition == AppliedPosition
                && retirementResult.OriginalCommandStatus
                    == OriginalCommandStatus
                && retirementResult.OriginalErrorId == OriginalErrorId
                && retirementResult.OriginalDetailCodeValue
                    == OriginalDetailCode
                && retirementResult.NativeCommandState == NativeCommandState
                && retirementResult.RecordGeneration == RecordGeneration;
        }

        internal bool EqualsExact(
            AxisSetPositionTerminalOutcomeProof other)
        {
            return other != null
                && QueryRequestId == other.QueryRequestId
                && RecordState == other.RecordState
                && AppliedPosition == other.AppliedPosition
                && OriginalCommandStatus == other.OriginalCommandStatus
                && OriginalErrorId == other.OriginalErrorId
                && OriginalDetailCode == other.OriginalDetailCode
                && NativeCommandState == other.NativeCommandState
                && RecordGeneration == other.RecordGeneration;
        }
    }

    internal sealed class AxisSetPositionRecoveryRecord
    {
        internal const int LegacyStorageFormatVersion = 1;
        internal const int CurrentStorageFormatVersion = 2;
        internal const ushort SupportedSchemaVersion = 1;
        internal const ushort SupportedSemanticMode = 1;

        internal AxisSetPositionRecoveryRecord(
            Guid identity,
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
            uint requestId,
            int targetPosition,
            int expectedActualPosition,
            ushort semanticMode,
            ushort schemaVersion,
            AxisSetPositionRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc)
            : this(
                identity,
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
                requestId,
                targetPosition,
                expectedActualPosition,
                semanticMode,
                schemaVersion,
                state,
                createdUtc,
                updatedUtc,
                CurrentStorageFormatVersion,
                null,
                0)
        {
        }

        internal AxisSetPositionRecoveryRecord(
            Guid identity,
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
            uint requestId,
            int targetPosition,
            int expectedActualPosition,
            ushort semanticMode,
            ushort schemaVersion,
            AxisSetPositionRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc,
            int storageFormatVersion,
            AxisSetPositionTerminalOutcomeProof terminalOutcomeProof,
            uint retirementRequestId)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Axis SetPosition recovery identity cannot be empty.",
                    "identity");
            }

            EndpointIp = NormalizeEndpointIp(endpointIp);
            if (endpointPort < 1 || endpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException("endpointPort");
            }
            if (diagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBuild");
            }
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }
            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }

            ValidateAxisName(axisName);
            if (axisReference < 1 || axisReference > 4)
            {
                throw new ArgumentOutOfRangeException("axisReference");
            }
            if ((clientIntentId0 | clientIntentId1 | clientIntentId2
                    | clientIntentId3) == 0)
            {
                throw new ArgumentException(
                    "Axis SetPosition client intent identity cannot be all zero.",
                    "clientIntentId0");
            }
            if (requestId == 0)
            {
                throw new ArgumentOutOfRangeException("requestId");
            }
            if (semanticMode != SupportedSemanticMode)
            {
                throw new ArgumentOutOfRangeException("semanticMode");
            }
            if (schemaVersion != SupportedSchemaVersion)
            {
                throw new ArgumentOutOfRangeException("schemaVersion");
            }

            ValidateState(state);
            ValidateLifecycleEvidence(
                storageFormatVersion,
                state,
                targetPosition,
                terminalOutcomeProof,
                retirementRequestId);
            if (createdUtc.Kind != DateTimeKind.Utc
                || updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Recovery timestamps must be UTC and monotonic.");
            }

            Identity = identity;
            EndpointPort = endpointPort;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            AxisName = axisName;
            AxisReference = axisReference;
            ClientIntentId0 = clientIntentId0;
            ClientIntentId1 = clientIntentId1;
            ClientIntentId2 = clientIntentId2;
            ClientIntentId3 = clientIntentId3;
            RequestId = requestId;
            TargetPosition = targetPosition;
            ExpectedActualPosition = expectedActualPosition;
            SemanticMode = semanticMode;
            SchemaVersion = schemaVersion;
            State = state;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            StorageFormatVersion = storageFormatVersion;
            TerminalOutcomeProof = terminalOutcomeProof == null
                ? null
                : terminalOutcomeProof.Copy();
            RetirementRequestId = retirementRequestId;
        }

        internal Guid Identity { get; private set; }
        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal uint DiagnosticsBuild { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal string AxisName { get; private set; }
        internal ushort AxisReference { get; private set; }
        internal uint ClientIntentId0 { get; private set; }
        internal uint ClientIntentId1 { get; private set; }
        internal uint ClientIntentId2 { get; private set; }
        internal uint ClientIntentId3 { get; private set; }
        internal uint RequestId { get; private set; }
        internal int TargetPosition { get; private set; }
        internal int ExpectedActualPosition { get; private set; }
        internal ushort SemanticMode { get; private set; }
        internal ushort SchemaVersion { get; private set; }
        internal AxisSetPositionRecoveryState State { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }
        internal int StorageFormatVersion { get; private set; }
        internal AxisSetPositionTerminalOutcomeProof TerminalOutcomeProof
        {
            get;
            private set;
        }
        internal uint RetirementRequestId { get; private set; }

        internal bool HasTerminalOutcomeProof
        {
            get { return TerminalOutcomeProof != null; }
        }

        internal bool IsActive
        {
            get { return State != AxisSetPositionRecoveryState.Resolved; }
        }

        internal bool MatchesRecoveryIdentity(
            string endpointIp,
            int endpointPort,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            string axisName,
            ushort axisReference)
        {
            string normalizedEndpoint;
            return TryNormalizeEndpointIp(endpointIp, out normalizedEndpoint)
                && string.Equals(
                    EndpointIp,
                    normalizedEndpoint,
                    StringComparison.Ordinal)
                && EndpointPort == endpointPort
                && DiagnosticsBuild == diagnosticsBuild
                && DiagnosticsBootId == diagnosticsBootId
                && MapRevision == mapRevision
                && string.Equals(AxisName, axisName, StringComparison.Ordinal)
                && AxisReference == axisReference;
        }

        internal bool MatchesIntent(
            uint clientIntentId0,
            uint clientIntentId1,
            uint clientIntentId2,
            uint clientIntentId3,
            uint requestId,
            int targetPosition,
            int expectedActualPosition,
            ushort semanticMode,
            ushort schemaVersion)
        {
            return ClientIntentId0 == clientIntentId0
                && ClientIntentId1 == clientIntentId1
                && ClientIntentId2 == clientIntentId2
                && ClientIntentId3 == clientIntentId3
                && RequestId == requestId
                && TargetPosition == targetPosition
                && ExpectedActualPosition == expectedActualPosition
                && SemanticMode == semanticMode
                && SchemaVersion == schemaVersion;
        }

        internal AxisSetPositionRecoveryRecord Copy()
        {
            return new AxisSetPositionRecoveryRecord(
                Identity,
                EndpointIp,
                EndpointPort,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                AxisName,
                AxisReference,
                ClientIntentId0,
                ClientIntentId1,
                ClientIntentId2,
                ClientIntentId3,
                RequestId,
                TargetPosition,
                ExpectedActualPosition,
                SemanticMode,
                SchemaVersion,
                State,
                CreatedUtc,
                UpdatedUtc,
                StorageFormatVersion,
                TerminalOutcomeProof,
                RetirementRequestId);
        }

        internal AxisSetPositionRecoveryRecord TransitionToRecoveryRequired(
            DateTime updatedUtc)
        {
            if (State != AxisSetPositionRecoveryState.ArmedBeforeDispatch)
            {
                throw new InvalidOperationException(
                    "Only an armed Axis SetPosition recovery record may transition to RecoveryRequired.");
            }
            ValidateTransitionTime(updatedUtc);
            return CreateTransition(
                AxisSetPositionRecoveryState.RecoveryRequired,
                updatedUtc,
                null,
                0);
        }

        internal AxisSetPositionRecoveryRecord ObserveTerminalOutcome(
            LMCAxisSetPositionOutcomeResult outcome,
            DateTime updatedUtc)
        {
            if (State != AxisSetPositionRecoveryState.ArmedBeforeDispatch
                && State != AxisSetPositionRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    "Only an unresolved Axis SetPosition recovery record without terminal proof may observe an outcome.");
            }
            ValidateSuccessfulOutcome(outcome);
            ValidateTransitionTime(updatedUtc);

            var proof = new AxisSetPositionTerminalOutcomeProof(
                outcome.QueryRequestId,
                outcome.RecordState,
                outcome.AppliedPosition,
                outcome.OriginalCommandStatus,
                outcome.OriginalErrorId,
                outcome.OriginalDetailCodeValue,
                outcome.NativeCommandState,
                outcome.RecordGeneration);
            ValidateTerminalCombination(TargetPosition, proof);
            return CreateTransition(
                AxisSetPositionRecoveryState.TerminalOutcomeObserved,
                updatedUtc,
                proof,
                0);
        }

        internal AxisSetPositionRecoveryRecord ResolveAfterRetirement(
            LMCAxisSetPositionOutcomeRetirementResult retirementResult,
            DateTime updatedUtc)
        {
            if (State
                    != AxisSetPositionRecoveryState.TerminalOutcomeObserved
                || TerminalOutcomeProof == null)
            {
                throw new InvalidOperationException(
                    "Axis SetPosition recovery cannot resolve before a terminal outcome is durably observed.");
            }
            if (retirementResult == null)
            {
                throw new ArgumentNullException("retirementResult");
            }
            if (retirementResult.Response == null
                || !retirementResult.Response.IsSuccess
                || !retirementResult.RetirementConfirmed
                || retirementResult.RetireRequestId == 0
                || !MatchesRecoveryKey(retirementResult.RecoveryKey)
                || !TerminalOutcomeProof.Matches(retirementResult))
            {
                throw new InvalidOperationException(
                    "Axis SetPosition retirement result does not exactly match the durable recovery key, generation, and terminal snapshot.");
            }
            ValidateTransitionTime(updatedUtc);
            return CreateTransition(
                AxisSetPositionRecoveryState.Resolved,
                updatedUtc,
                TerminalOutcomeProof,
                retirementResult.RetireRequestId);
        }

        private AxisSetPositionRecoveryRecord CreateTransition(
            AxisSetPositionRecoveryState nextState,
            DateTime updatedUtc,
            AxisSetPositionTerminalOutcomeProof terminalOutcomeProof,
            uint retirementRequestId)
        {

            return new AxisSetPositionRecoveryRecord(
                Identity,
                EndpointIp,
                EndpointPort,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                AxisName,
                AxisReference,
                ClientIntentId0,
                ClientIntentId1,
                ClientIntentId2,
                ClientIntentId3,
                RequestId,
                TargetPosition,
                ExpectedActualPosition,
                SemanticMode,
                SchemaVersion,
                nextState,
                CreatedUtc,
                updatedUtc,
                CurrentStorageFormatVersion,
                terminalOutcomeProof,
                retirementRequestId);
        }

        private void ValidateSuccessfulOutcome(
            LMCAxisSetPositionOutcomeResult outcome)
        {
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }
            if (outcome.Response == null
                || !outcome.Response.IsSuccess
                || outcome.QueryRequestId == 0
                || outcome.RecordGeneration == 0
                || !MatchesRecoveryKey(outcome.RecoveryKey))
            {
                throw new InvalidOperationException(
                    "Only a successful exact Axis SetPosition terminal query may be persisted.");
            }
        }

        private bool MatchesRecoveryKey(
            LMCAxisSetPositionRecoveryKey recoveryKey)
        {
            return recoveryKey != null
                && recoveryKey.SchemaVersion == SchemaVersion
                && recoveryKey.OriginalRequestId == RequestId
                && recoveryKey.DiagnosticsBuild == DiagnosticsBuild
                && recoveryKey.DiagnosticsBootId == DiagnosticsBootId
                && recoveryKey.MapRevision == MapRevision
                && recoveryKey.ClientIntentId0 == ClientIntentId0
                && recoveryKey.ClientIntentId1 == ClientIntentId1
                && recoveryKey.ClientIntentId2 == ClientIntentId2
                && recoveryKey.ClientIntentId3 == ClientIntentId3
                && recoveryKey.AxisReference == AxisReference
                && recoveryKey.TargetPosition == TargetPosition
                && recoveryKey.ExpectedActualPosition
                    == ExpectedActualPosition
                && (ushort)recoveryKey.SemanticMode == SemanticMode;
        }

        private void ValidateTransitionTime(DateTime updatedUtc)
        {
            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < UpdatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Recovery transition time must be UTC and cannot move backwards.");
            }
        }

        private static void ValidateState(AxisSetPositionRecoveryState state)
        {
            if (state != AxisSetPositionRecoveryState.ArmedBeforeDispatch
                && state != AxisSetPositionRecoveryState.RecoveryRequired
                && state != AxisSetPositionRecoveryState.Resolved
                && state
                    != AxisSetPositionRecoveryState.TerminalOutcomeObserved)
            {
                throw new ArgumentOutOfRangeException("state");
            }
        }

        private static void ValidateLifecycleEvidence(
            int storageFormatVersion,
            AxisSetPositionRecoveryState state,
            int targetPosition,
            AxisSetPositionTerminalOutcomeProof terminalOutcomeProof,
            uint retirementRequestId)
        {
            if (storageFormatVersion == LegacyStorageFormatVersion)
            {
                if (state
                        == AxisSetPositionRecoveryState
                            .TerminalOutcomeObserved
                    || terminalOutcomeProof != null
                    || retirementRequestId != 0)
                {
                    throw new ArgumentException(
                        "Legacy Axis SetPosition recovery records cannot contain v2 lifecycle evidence.",
                        "terminalOutcomeProof");
                }
                return;
            }
            if (storageFormatVersion != CurrentStorageFormatVersion)
            {
                throw new ArgumentOutOfRangeException(
                    "storageFormatVersion");
            }

            if (state == AxisSetPositionRecoveryState.ArmedBeforeDispatch
                || state == AxisSetPositionRecoveryState.RecoveryRequired)
            {
                if (terminalOutcomeProof != null || retirementRequestId != 0)
                {
                    throw new ArgumentException(
                        "Pre-terminal Axis SetPosition recovery state cannot contain terminal or retirement evidence.",
                        "terminalOutcomeProof");
                }
                return;
            }

            if (terminalOutcomeProof == null)
            {
                throw new ArgumentException(
                    "Terminal Axis SetPosition recovery state requires durable terminal query proof.",
                    "terminalOutcomeProof");
            }
            ValidateTerminalCombination(targetPosition, terminalOutcomeProof);
            if (state
                    == AxisSetPositionRecoveryState.TerminalOutcomeObserved
                && retirementRequestId != 0)
            {
                throw new ArgumentException(
                    "TerminalOutcomeObserved cannot contain retirement proof.",
                    "retirementRequestId");
            }
            if (state == AxisSetPositionRecoveryState.Resolved
                && retirementRequestId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "retirementRequestId",
                    "Resolved v2 records require a successful retirement request identity.");
            }
        }

        private static void ValidateTerminalCombination(
            int targetPosition,
            AxisSetPositionTerminalOutcomeProof proof)
        {
            var valid = proof.RecordState
                    == LMCAxisSetPositionOutcomeRecordState.Succeeded
                ? proof.OriginalCommandStatus == 0
                    && proof.OriginalErrorId == 0
                    && proof.OriginalDetailCode == 0
                    && proof.AppliedPosition == targetPosition
                    && proof.NativeCommandState == 0
                : proof.RecordState
                        == LMCAxisSetPositionOutcomeRecordState.Rejected
                    && proof.OriginalCommandStatus == 1
                    && proof.AppliedPosition == 0
                    && IsValidTerminalRejection(proof);
            if (!valid)
            {
                throw new ArgumentException(
                    "Axis SetPosition terminal proof contains an invalid result combination.",
                    "proof");
            }
        }

        private static bool IsValidTerminalRejection(
            AxisSetPositionTerminalOutcomeProof proof)
        {
            if (proof.OriginalDetailCode
                    == (uint)LMCAdminDetailCode.NativeCommandRejected)
            {
                return proof.OriginalErrorId == -6
                    && proof.NativeCommandState != 0;
            }

            // Syntax, identity, and storage failures happen before the durable
            // Armed commit and therefore cannot exist in a terminal record.
            var detail = proof.OriginalDetailCode;
            var isPostArmedDetail =
                detail >= (uint)LMCAdminDetailCode.InvalidState
                && detail
                    <= (uint)LMCAdminDetailCode
                        .CoordinatePreconditionFailed;
            return isPostArmedDetail
                && proof.OriginalErrorId == -31000
                && proof.NativeCommandState == 0;
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

    internal sealed class AxisSetPositionRecoveryJournal : IDisposable
    {
        private const int LegacyFormatVersion =
            AxisSetPositionRecoveryRecord.LegacyStorageFormatVersion;
        private const int CurrentFormatVersion =
            AxisSetPositionRecoveryRecord.CurrentStorageFormatVersion;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 8192;
        private const int MaximumTextLength = 1024;
        private const string JournalFileName =
            "axis-set-position-recovery.bin";
        private const string LockFileName =
            "axis-set-position-recovery.lock";
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMOASP1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private AxisSetPositionRecoveryRecord currentRecord;
        private bool disposed;

        private AxisSetPositionRecoveryJournal(string requestedDirectoryPath)
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

        internal string DirectoryPath { get { return directoryPath; } }
        internal string JournalFilePath { get { return journalFilePath; } }

        internal AxisSetPositionRecoveryRecord CurrentRecord
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

        internal static AxisSetPositionRecoveryJournal Open(
            string directoryPath)
        {
            return new AxisSetPositionRecoveryJournal(directoryPath);
        }

        internal static AxisSetPositionRecoveryJournal OpenDefault()
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
                "AxisSetPositionRecoveryJournal",
                "v1");
        }

        internal AxisSetPositionRecoveryRecord ArmBeforeDispatch(
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
            uint requestId,
            int targetPosition,
            int expectedActualPosition,
            ushort semanticMode,
            ushort schemaVersion,
            DateTime createdUtc)
        {
            return ArmBeforeDispatch(
                Guid.NewGuid(),
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
                requestId,
                targetPosition,
                expectedActualPosition,
                semanticMode,
                schemaVersion,
                createdUtc);
        }

        internal AxisSetPositionRecoveryRecord ArmBeforeDispatch(
            Guid identity,
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
            uint requestId,
            int targetPosition,
            int expectedActualPosition,
            ushort semanticMode,
            ushort schemaVersion,
            DateTime createdUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved Axis SetPosition recovery record already exists.");
                }
                if (currentRecord != null
                    && currentRecord.Identity == identity)
                {
                    throw new InvalidOperationException(
                        "A resolved Axis SetPosition recovery identity cannot be reused.");
                }

                var armed = new AxisSetPositionRecoveryRecord(
                    identity,
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
                    requestId,
                    targetPosition,
                    expectedActualPosition,
                    semanticMode,
                    schemaVersion,
                    AxisSetPositionRecoveryState.ArmedBeforeDispatch,
                    createdUtc,
                    createdUtc);
                PersistRecord(armed);
                currentRecord = armed;
                return armed.Copy();
            }
        }

        internal AxisSetPositionRecoveryRecord PromoteToRecoveryRequired(
            AxisSetPositionRecoveryRecord expectedRecord,
            DateTime updatedUtc)
        {
            lock (sync)
            {
                EnsureExpectedRecord(expectedRecord);
                return PersistTransition(
                    currentRecord.TransitionToRecoveryRequired(updatedUtc));
            }
        }

        internal AxisSetPositionRecoveryRecord RecordTerminalOutcome(
            AxisSetPositionRecoveryRecord expectedRecord,
            LMCAxisSetPositionOutcomeResult outcome,
            DateTime updatedUtc)
        {
            lock (sync)
            {
                EnsureExpectedRecord(expectedRecord);
                return PersistTransition(
                    currentRecord.ObserveTerminalOutcome(
                        outcome,
                        updatedUtc));
            }
        }

        internal AxisSetPositionRecoveryRecord ResolveAfterRetirement(
            AxisSetPositionRecoveryRecord expectedRecord,
            LMCAxisSetPositionOutcomeRetirementResult retirementResult,
            DateTime updatedUtc)
        {
            lock (sync)
            {
                EnsureExpectedRecord(expectedRecord);
                return PersistTransition(
                    currentRecord.ResolveAfterRetirement(
                        retirementResult,
                        updatedUtc));
            }
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

        private void EnsureExpectedRecord(
            AxisSetPositionRecoveryRecord expectedRecord)
        {
            if (expectedRecord == null)
            {
                throw new ArgumentNullException("expectedRecord");
            }
            ThrowIfDisposed();
            if (!RecordsEqual(currentRecord, expectedRecord))
            {
                throw new InvalidOperationException(
                    "Axis SetPosition recovery record changed after it was captured; the transition was not applied.");
            }
        }

        private AxisSetPositionRecoveryRecord PersistTransition(
            AxisSetPositionRecoveryRecord transitioned)
        {
            PersistRecord(transitioned);
            currentRecord = transitioned;
            return transitioned.Copy();
        }

        private static bool RecordsEqual(
            AxisSetPositionRecoveryRecord left,
            AxisSetPositionRecoveryRecord right)
        {
            return left != null
                && right != null
                && left.Identity == right.Identity
                && string.Equals(
                    left.EndpointIp,
                    right.EndpointIp,
                    StringComparison.Ordinal)
                && left.EndpointPort == right.EndpointPort
                && left.DiagnosticsBuild == right.DiagnosticsBuild
                && left.DiagnosticsBootId == right.DiagnosticsBootId
                && left.MapRevision == right.MapRevision
                && string.Equals(
                    left.AxisName,
                    right.AxisName,
                    StringComparison.Ordinal)
                && left.AxisReference == right.AxisReference
                && left.ClientIntentId0 == right.ClientIntentId0
                && left.ClientIntentId1 == right.ClientIntentId1
                && left.ClientIntentId2 == right.ClientIntentId2
                && left.ClientIntentId3 == right.ClientIntentId3
                && left.RequestId == right.RequestId
                && left.TargetPosition == right.TargetPosition
                && left.ExpectedActualPosition
                    == right.ExpectedActualPosition
                && left.SemanticMode == right.SemanticMode
                && left.SchemaVersion == right.SchemaVersion
                && left.State == right.State
                && left.CreatedUtc == right.CreatedUtc
                && left.UpdatedUtc == right.UpdatedUtc
                && left.StorageFormatVersion == right.StorageFormatVersion
                && TerminalProofsEqual(
                    left.TerminalOutcomeProof,
                    right.TerminalOutcomeProof)
                && left.RetirementRequestId == right.RetirementRequestId;
        }

        private static bool TerminalProofsEqual(
            AxisSetPositionTerminalOutcomeProof left,
            AxisSetPositionTerminalOutcomeProof right)
        {
            return left == null
                ? right == null
                : left.EqualsExact(right);
        }

        private void PromoteArmedRecordAtOpen()
        {
            if (currentRecord == null
                || currentRecord.State
                    != AxisSetPositionRecoveryState.ArmedBeforeDispatch)
            {
                return;
            }

            var updatedUtc = DateTime.UtcNow;
            if (updatedUtc < currentRecord.UpdatedUtc)
            {
                updatedUtc = currentRecord.UpdatedUtc;
            }
            var recoveryRequired = currentRecord
                .TransitionToRecoveryRequired(updatedUtc);
            PersistRecord(recoveryRequired);
            currentRecord = recoveryRequired;
        }

        private void PersistRecord(AxisSetPositionRecoveryRecord record)
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

        private static AxisSetPositionRecoveryRecord LoadRecord(string path)
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
                        "Axis SetPosition recovery journal length is invalid.");
                }
                var bytes = new byte[checked((int)fileLength)];
                var offset = 0;
                while (offset < bytes.Length)
                {
                    var read = stream.Read(bytes, offset, bytes.Length - offset);
                    if (read <= 0)
                    {
                        throw new InvalidDataException(
                            "Axis SetPosition recovery journal is truncated.");
                    }
                    offset += read;
                }
                return DeserializeRecord(bytes);
            }
        }

        private static byte[] SerializeRecord(
            AxisSetPositionRecoveryRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (record.StorageFormatVersion != CurrentFormatVersion)
            {
                throw new InvalidOperationException(
                    "Only the current Axis SetPosition recovery journal format may be written.");
            }

            byte[] payload;
            using (var payloadStream = new MemoryStream())
            using (var writer = new BinaryWriter(
                payloadStream,
                Encoding.ASCII,
                true))
            {
                writer.Write(record.Identity.ToByteArray());
                writer.Write((int)record.State);
                writer.Write(record.CreatedUtc.Ticks);
                writer.Write(record.UpdatedUtc.Ticks);
                writer.Write(record.DiagnosticsBuild);
                writer.Write(record.DiagnosticsBootId);
                writer.Write(record.MapRevision);
                writer.Write(record.EndpointPort);
                writer.Write(record.AxisReference);
                writer.Write(record.SchemaVersion);
                writer.Write(record.SemanticMode);
                writer.Write(record.RequestId);
                writer.Write(record.ClientIntentId0);
                writer.Write(record.ClientIntentId1);
                writer.Write(record.ClientIntentId2);
                writer.Write(record.ClientIntentId3);
                writer.Write(record.TargetPosition);
                writer.Write(record.ExpectedActualPosition);
                WriteText(writer, record.EndpointIp);
                WriteText(writer, record.AxisName);
                var proof = record.TerminalOutcomeProof;
                writer.Write(proof == null ? (byte)0 : (byte)1);
                if (proof != null)
                {
                    writer.Write(proof.QueryRequestId);
                    writer.Write((ushort)proof.RecordState);
                    writer.Write(proof.AppliedPosition);
                    writer.Write(proof.OriginalCommandStatus);
                    writer.Write(proof.OriginalErrorId);
                    writer.Write(proof.OriginalDetailCode);
                    writer.Write(proof.NativeCommandState);
                    writer.Write(proof.RecordGeneration);
                }
                writer.Write(record.RetirementRequestId);
                writer.Flush();
                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                writer.Write(Magic);
                writer.Write(CurrentFormatVersion);
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

        private static AxisSetPositionRecoveryRecord DeserializeRecord(
            byte[] bytes)
        {
            try
            {
                if (bytes == null
                    || bytes.Length < Magic.Length + 8 + ChecksumLength
                    || bytes.Length > MaximumFileLength)
                {
                    throw new InvalidDataException(
                        "Axis SetPosition recovery journal length is invalid.");
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
                        "Axis SetPosition recovery journal checksum is invalid.");
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
                            "Axis SetPosition recovery journal header is invalid.");
                    }
                    var formatVersion = reader.ReadInt32();
                    if (formatVersion != LegacyFormatVersion
                        && formatVersion != CurrentFormatVersion)
                    {
                        throw new InvalidDataException(
                            "Axis SetPosition recovery journal version is unsupported.");
                    }
                    var payloadLength = reader.ReadInt32();
                    if (payloadLength <= 0
                        || payloadLength
                            != checksumOffset - Magic.Length - 8)
                    {
                        throw new InvalidDataException(
                            "Axis SetPosition recovery journal payload length is invalid.");
                    }
                    var payload = reader.ReadBytes(payloadLength);
                    if (payload.Length != payloadLength
                        || stream.Position != checksumOffset)
                    {
                        throw new InvalidDataException(
                            "Axis SetPosition recovery journal payload is incomplete.");
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
                    "Axis SetPosition recovery journal is invalid.",
                    error);
            }
        }

        private static AxisSetPositionRecoveryRecord DeserializePayload(
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
                        "Axis SetPosition recovery identity is incomplete.");
                }
                var identity = new Guid(identityBytes);
                var state = (AxisSetPositionRecoveryState)reader.ReadInt32();
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var diagnosticsBuild = reader.ReadUInt32();
                var diagnosticsBootId = reader.ReadUInt32();
                var mapRevision = reader.ReadUInt32();
                var endpointPort = reader.ReadInt32();
                var axisReference = reader.ReadUInt16();
                var schemaVersion = reader.ReadUInt16();
                var semanticMode = reader.ReadUInt16();
                var requestId = reader.ReadUInt32();
                var clientIntentId0 = reader.ReadUInt32();
                var clientIntentId1 = reader.ReadUInt32();
                var clientIntentId2 = reader.ReadUInt32();
                var clientIntentId3 = reader.ReadUInt32();
                var targetPosition = reader.ReadInt32();
                var expectedActualPosition = reader.ReadInt32();
                var endpointIp = ReadText(reader);
                var axisName = ReadText(reader);
                AxisSetPositionTerminalOutcomeProof terminalOutcomeProof =
                    null;
                var retirementRequestId = 0U;
                if (formatVersion == CurrentFormatVersion)
                {
                    var hasTerminalOutcomeProof = reader.ReadByte();
                    if (hasTerminalOutcomeProof > 1)
                    {
                        throw new InvalidDataException(
                            "Axis SetPosition recovery terminal proof marker is invalid.");
                    }
                    if (hasTerminalOutcomeProof == 1)
                    {
                        terminalOutcomeProof =
                            new AxisSetPositionTerminalOutcomeProof(
                                reader.ReadUInt32(),
                                (LMCAxisSetPositionOutcomeRecordState)
                                    reader.ReadUInt16(),
                                reader.ReadInt32(),
                                reader.ReadUInt16(),
                                reader.ReadInt16(),
                                reader.ReadUInt32(),
                                reader.ReadUInt32(),
                                reader.ReadUInt32());
                    }
                    retirementRequestId = reader.ReadUInt32();
                }
                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Axis SetPosition recovery journal has trailing data.");
                }

                var record = new AxisSetPositionRecoveryRecord(
                    identity,
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
                    requestId,
                    targetPosition,
                    expectedActualPosition,
                    semanticMode,
                    schemaVersion,
                    state,
                    createdUtc,
                    updatedUtc,
                    formatVersion,
                    terminalOutcomeProof,
                    retirementRequestId);
                if (!string.Equals(
                    endpointIp,
                    record.EndpointIp,
                    StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "Axis SetPosition recovery endpoint is not canonical.");
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
                    "Axis SetPosition recovery text length is invalid.");
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
                    "Axis SetPosition recovery text length is invalid.");
            }
            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Axis SetPosition recovery text is incomplete.");
            }
            foreach (var value in bytes)
            {
                if (value < 0x20 || value > 0x7E)
                {
                    throw new InvalidDataException(
                        "Axis SetPosition recovery text is not 7-bit printable ASCII.");
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
                    "AxisSetPositionRecoveryJournal");
            }
        }
    }
}

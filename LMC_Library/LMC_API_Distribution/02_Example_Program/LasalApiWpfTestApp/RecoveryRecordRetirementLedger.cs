using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;

namespace LasalMotionControlApiExample
{
    internal enum RecoveryRecordOwner
    {
        AxisPower = 1,
        AxisCommand = 2,
        Motion = 3,
        GroupProfileLock = 4,
        GroupPower = 5,
        GroupReset = 6,
        AxisQualification = 7,
        DiagnosticsMutation = 8
    }

    internal enum RecoveryEndpointEvidenceKind
    {
        RecordedSourceEndpoint = 1,
        OperatorClassifiedLegacyEndpoint = 2
    }

    internal sealed class RecoveryJournalSourceEvidence
    {
        private const int MaximumOriginalByteLength = 32768;
        private readonly byte[] originalBytes;

        internal RecoveryJournalSourceEvidence(
            RecoveryRecordOwner owner,
            Guid recordIdentity,
            int stateCode,
            DateTime createdUtc,
            DateTime updatedUtc,
            string endpointIp,
            int endpointPort,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            string targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            string semanticFingerprint,
            byte[] originalBytes,
            RecoveryEndpointEvidenceKind endpointEvidenceKind =
                RecoveryEndpointEvidenceKind.RecordedSourceEndpoint)
        {
            ValidateOwner(owner);
            ValidateEndpointEvidenceKind(owner, endpointEvidenceKind);
            if (recordIdentity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Recovery record identity cannot be empty.",
                    "recordIdentity");
            }
            if (!IsActiveState(owner, stateCode))
            {
                throw new ArgumentOutOfRangeException(
                    "stateCode",
                    "Operator retirement evidence must describe an active recovery state.");
            }
            ValidateTimestamps(createdUtc, updatedUtc);
            EndpointIp = NormalizeEndpointIp(endpointIp);
            if (endpointPort < 1 || endpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException("endpointPort");
            }
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException("diagnosticsBootId");
            }
            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("mapRevision");
            }
            if ((owner == RecoveryRecordOwner.GroupReset
                    || owner == RecoveryRecordOwner.AxisQualification)
                && diagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBuild",
                    "This recovery evidence requires DiagnosticsBuild.");
            }
            ValidateText(targetKind, "targetKind", 64);
            if ((owner == RecoveryRecordOwner.AxisPower
                    || owner == RecoveryRecordOwner.AxisCommand
                    || owner == RecoveryRecordOwner.AxisQualification)
                && !string.Equals(
                    targetKind,
                    "Axis",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Axis recovery evidence must identify an Axis target.",
                    "targetKind");
            }
            if ((owner == RecoveryRecordOwner.GroupProfileLock
                    || owner == RecoveryRecordOwner.GroupPower
                    || owner == RecoveryRecordOwner.GroupReset)
                && !string.Equals(
                    targetKind,
                    "Group",
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Group recovery evidence must identify a Group target.",
                    "targetKind");
            }
            ValidateText(targetName, "targetName", 1024);
            if (targetReference == 0)
            {
                throw new ArgumentOutOfRangeException("targetReference");
            }
            ValidateText(operation, "operation", 1024);
            ValidateText(
                semanticFingerprint,
                "semanticFingerprint",
                4096);
            if (originalBytes == null
                || originalBytes.Length < 1
                || originalBytes.Length > MaximumOriginalByteLength)
            {
                throw new ArgumentException(
                    "Original recovery journal bytes are missing or too large.",
                    "originalBytes");
            }

            Owner = owner;
            EndpointEvidenceKind = endpointEvidenceKind;
            RecordIdentity = recordIdentity;
            StateCode = stateCode;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            EndpointPort = endpointPort;
            DiagnosticsBuild = diagnosticsBuild;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            TargetKind = targetKind;
            TargetName = targetName;
            TargetReference = targetReference;
            Operation = operation;
            SemanticFingerprint = semanticFingerprint;
            this.originalBytes = CloneBytes(originalBytes);
            OriginalSha256 = ComputeSha256(this.originalBytes);
        }

        internal RecoveryJournalSourceEvidence(
            RecoveryRecordOwner owner,
            Guid recordIdentity,
            int stateCode,
            DateTime createdUtc,
            DateTime updatedUtc,
            string endpointIp,
            int endpointPort,
            uint diagnosticsBootId,
            uint mapRevision,
            string targetKind,
            string targetName,
            ushort targetReference,
            string operation,
            string semanticFingerprint,
            byte[] originalBytes)
            : this(
                owner,
                recordIdentity,
                stateCode,
                createdUtc,
                updatedUtc,
                endpointIp,
                endpointPort,
                0,
                diagnosticsBootId,
                mapRevision,
                targetKind,
                targetName,
                targetReference,
                operation,
                semanticFingerprint,
                originalBytes)
        {
        }

        internal RecoveryRecordOwner Owner { get; private set; }
        internal RecoveryEndpointEvidenceKind EndpointEvidenceKind
        {
            get;
            private set;
        }
        internal Guid RecordIdentity { get; private set; }
        internal int StateCode { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }
        internal string EndpointIp { get; private set; }
        internal int EndpointPort { get; private set; }
        internal uint DiagnosticsBuild { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal string TargetKind { get; private set; }
        internal string TargetName { get; private set; }
        internal ushort TargetReference { get; private set; }
        internal string Operation { get; private set; }
        internal string SemanticFingerprint { get; private set; }
        internal string OriginalSha256 { get; private set; }
        internal int OriginalByteLength { get { return originalBytes.Length; } }

        internal byte[] GetOriginalBytes()
        {
            return CloneBytes(originalBytes);
        }

        internal RecoveryJournalSourceEvidence Copy()
        {
            return new RecoveryJournalSourceEvidence(
                Owner,
                RecordIdentity,
                StateCode,
                CreatedUtc,
                UpdatedUtc,
                EndpointIp,
                EndpointPort,
                DiagnosticsBuild,
                DiagnosticsBootId,
                MapRevision,
                TargetKind,
                TargetName,
                TargetReference,
                Operation,
                SemanticFingerprint,
                originalBytes,
                EndpointEvidenceKind);
        }

        internal bool ExactSourceEquals(
            RecoveryJournalSourceEvidence candidate)
        {
            return candidate != null
                && Owner == candidate.Owner
                && EndpointEvidenceKind == candidate.EndpointEvidenceKind
                && RecordIdentity == candidate.RecordIdentity
                && StateCode == candidate.StateCode
                && CreatedUtc == candidate.CreatedUtc
                && UpdatedUtc == candidate.UpdatedUtc
                && string.Equals(
                    EndpointIp,
                    candidate.EndpointIp,
                    StringComparison.Ordinal)
                && EndpointPort == candidate.EndpointPort
                && DiagnosticsBuild == candidate.DiagnosticsBuild
                && DiagnosticsBootId == candidate.DiagnosticsBootId
                && MapRevision == candidate.MapRevision
                && string.Equals(
                    TargetKind,
                    candidate.TargetKind,
                    StringComparison.Ordinal)
                && string.Equals(
                    TargetName,
                    candidate.TargetName,
                    StringComparison.Ordinal)
                && TargetReference == candidate.TargetReference
                && string.Equals(
                    Operation,
                    candidate.Operation,
                    StringComparison.Ordinal)
                && string.Equals(
                    SemanticFingerprint,
                    candidate.SemanticFingerprint,
                    StringComparison.Ordinal)
                && string.Equals(
                    OriginalSha256,
                    candidate.OriginalSha256,
                    StringComparison.Ordinal)
                && ConstantTimeEquals(
                    originalBytes,
                    candidate.originalBytes);
        }

        internal static string ComputeSha256(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            byte[] hash;
            using (var algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(bytes);
            }

            var text = new StringBuilder(hash.Length * 2);
            foreach (var value in hash)
            {
                text.Append(value.ToString("X2", CultureInfo.InvariantCulture));
            }
            return text.ToString();
        }

        internal static bool ConstantTimeEquals(byte[] left, byte[] right)
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

        private static byte[] CloneBytes(byte[] source)
        {
            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }

        private static void ValidateOwner(RecoveryRecordOwner owner)
        {
            if (owner != RecoveryRecordOwner.AxisPower
                && owner != RecoveryRecordOwner.AxisCommand
                && owner != RecoveryRecordOwner.Motion
                && owner != RecoveryRecordOwner.GroupProfileLock
                && owner != RecoveryRecordOwner.GroupPower
                && owner != RecoveryRecordOwner.GroupReset
                && owner != RecoveryRecordOwner.AxisQualification
                && owner != RecoveryRecordOwner.DiagnosticsMutation)
            {
                throw new ArgumentOutOfRangeException("owner");
            }
        }

        private static void ValidateEndpointEvidenceKind(
            RecoveryRecordOwner owner,
            RecoveryEndpointEvidenceKind endpointEvidenceKind)
        {
            if (endpointEvidenceKind
                    != RecoveryEndpointEvidenceKind.RecordedSourceEndpoint
                && endpointEvidenceKind
                    != RecoveryEndpointEvidenceKind
                        .OperatorClassifiedLegacyEndpoint)
            {
                throw new ArgumentOutOfRangeException(
                    "endpointEvidenceKind");
            }

            if (owner == RecoveryRecordOwner.DiagnosticsMutation)
            {
                if (endpointEvidenceKind
                    != RecoveryEndpointEvidenceKind
                        .OperatorClassifiedLegacyEndpoint)
                {
                    throw new ArgumentException(
                        "Legacy diagnostics mutation retirement requires an explicit operator-classified endpoint.",
                        "endpointEvidenceKind");
                }
                return;
            }

            if (endpointEvidenceKind
                != RecoveryEndpointEvidenceKind.RecordedSourceEndpoint)
            {
                throw new ArgumentException(
                    "This recovery owner requires its recorded source endpoint.",
                    "endpointEvidenceKind");
            }
        }

        private static bool IsActiveState(
            RecoveryRecordOwner owner,
            int stateCode)
        {
            if (owner == RecoveryRecordOwner.Motion)
            {
                return stateCode == 1 || stateCode == 2;
            }
            if (owner == RecoveryRecordOwner.GroupProfileLock)
            {
                return stateCode == 1
                    || stateCode == 2
                    || stateCode == 4;
            }
            if (owner == RecoveryRecordOwner.AxisQualification)
            {
                return stateCode >= 1 && stateCode <= 9;
            }
            if (owner == RecoveryRecordOwner.DiagnosticsMutation)
            {
                return stateCode
                    == (int)DiagnosticsMutationState.OutcomeUnverified;
            }
            return stateCode == 1 || stateCode == 2 || stateCode == 3;
        }

        private static void ValidateTimestamps(
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (createdUtc.Kind != DateTimeKind.Utc
                || updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Recovery evidence timestamps must be UTC and monotonic.");
            }
        }

        private static void ValidateText(
            string value,
            string parameterName,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > maximumLength)
            {
                throw new ArgumentException(
                    "Recovery evidence text is missing or too long.",
                    parameterName);
            }

            foreach (var character in value)
            {
                if (char.IsControl(character))
                {
                    throw new ArgumentException(
                        "Recovery evidence text cannot contain control characters.",
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
                    "Recovery evidence endpoint must be an IPv4 literal.",
                    "endpointIp");
            }
            return parsed.ToString();
        }
    }

    internal sealed class RecoveryRecordRetirementDecision
    {
        internal RecoveryRecordRetirementDecision(
            Guid decisionIdentity,
            RecoveryJournalSourceEvidence sourceEvidence,
            string currentEndpointIp,
            int currentEndpointPort,
            uint currentDiagnosticsBuild,
            uint currentDiagnosticsBootId,
            uint currentMapRevision,
            string operatorIdentity,
            string reason,
            DateTime decisionUtc)
        {
            if (decisionIdentity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Retirement decision identity cannot be empty.",
                    "decisionIdentity");
            }
            if (sourceEvidence == null)
            {
                throw new ArgumentNullException("sourceEvidence");
            }

            var normalizedCurrentEndpoint = NormalizeEndpointIp(
                currentEndpointIp);
            if (currentEndpointPort < 1 || currentEndpointPort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    "currentEndpointPort");
            }
            if (!string.Equals(
                    sourceEvidence.EndpointIp,
                    normalizedCurrentEndpoint,
                    StringComparison.Ordinal)
                || sourceEvidence.EndpointPort != currentEndpointPort)
            {
                throw new InvalidOperationException(
                    "Operator retirement requires the same PLC endpoint as the source recovery record.");
            }
            if (currentDiagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "currentDiagnosticsBootId");
            }
            if (currentMapRevision == 0)
            {
                throw new ArgumentOutOfRangeException("currentMapRevision");
            }
            if (sourceEvidence.DiagnosticsBuild != 0
                && currentDiagnosticsBuild == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "currentDiagnosticsBuild");
            }
            if ((sourceEvidence.DiagnosticsBuild == 0
                    || sourceEvidence.DiagnosticsBuild
                        == currentDiagnosticsBuild)
                && sourceEvidence.DiagnosticsBootId
                    == currentDiagnosticsBootId
                && sourceEvidence.MapRevision == currentMapRevision)
            {
                throw new InvalidOperationException(
                    "An exact current recovery identity cannot be operator-retired as superseded.");
            }
            ValidateDecisionText(
                operatorIdentity,
                "operatorIdentity",
                512);
            ValidateDecisionText(reason, "reason", 2048);
            if (decisionUtc.Kind != DateTimeKind.Utc
                || decisionUtc < sourceEvidence.UpdatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "decisionUtc",
                    "Retirement decision time must be UTC and cannot precede the source record.");
            }

            DecisionIdentity = decisionIdentity;
            SourceEvidence = sourceEvidence.Copy();
            CurrentEndpointIp = normalizedCurrentEndpoint;
            CurrentEndpointPort = currentEndpointPort;
            CurrentDiagnosticsBuild = currentDiagnosticsBuild;
            CurrentDiagnosticsBootId = currentDiagnosticsBootId;
            CurrentMapRevision = currentMapRevision;
            OperatorIdentity = operatorIdentity;
            Reason = reason;
            DecisionUtc = decisionUtc;
        }

        internal RecoveryRecordRetirementDecision(
            Guid decisionIdentity,
            RecoveryJournalSourceEvidence sourceEvidence,
            string currentEndpointIp,
            int currentEndpointPort,
            uint currentDiagnosticsBootId,
            uint currentMapRevision,
            string operatorIdentity,
            string reason,
            DateTime decisionUtc)
            : this(
                decisionIdentity,
                sourceEvidence,
                currentEndpointIp,
                currentEndpointPort,
                0,
                currentDiagnosticsBootId,
                currentMapRevision,
                operatorIdentity,
                reason,
                decisionUtc)
        {
        }

        internal Guid DecisionIdentity { get; private set; }
        internal RecoveryJournalSourceEvidence SourceEvidence
        {
            get;
            private set;
        }
        internal string CurrentEndpointIp { get; private set; }
        internal int CurrentEndpointPort { get; private set; }
        internal uint CurrentDiagnosticsBuild { get; private set; }
        internal uint CurrentDiagnosticsBootId { get; private set; }
        internal uint CurrentMapRevision { get; private set; }
        internal string OperatorIdentity { get; private set; }
        internal string Reason { get; private set; }
        internal DateTime DecisionUtc { get; private set; }
        internal bool IsDurablyCommitted { get; private set; }
        internal string DurableEntrySha256 { get; private set; }

        internal bool MatchesSourceEvidence(
            RecoveryJournalSourceEvidence evidence)
        {
            return SourceEvidence.ExactSourceEquals(evidence);
        }

        internal RecoveryRecordRetirementDecision Copy()
        {
            var copy = new RecoveryRecordRetirementDecision(
                DecisionIdentity,
                SourceEvidence,
                CurrentEndpointIp,
                CurrentEndpointPort,
                CurrentDiagnosticsBuild,
                CurrentDiagnosticsBootId,
                CurrentMapRevision,
                OperatorIdentity,
                Reason,
                DecisionUtc);
            if (IsDurablyCommitted)
            {
                copy.MarkDurablyCommitted(DurableEntrySha256);
            }
            return copy;
        }

        internal void MarkDurablyCommitted(string durableEntrySha256)
        {
            if (string.IsNullOrWhiteSpace(durableEntrySha256)
                || durableEntrySha256.Length != 64)
            {
                throw new ArgumentException(
                    "Durable retirement entry SHA-256 is invalid.",
                    "durableEntrySha256");
            }
            if (IsDurablyCommitted
                && !string.Equals(
                    DurableEntrySha256,
                    durableEntrySha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A durable retirement decision cannot change its entry SHA-256.");
            }
            DurableEntrySha256 = durableEntrySha256;
            IsDurablyCommitted = true;
        }

        internal bool MatchesDecisionContext(
            string currentEndpointIp,
            int currentEndpointPort,
            uint currentDiagnosticsBuild,
            uint currentDiagnosticsBootId,
            uint currentMapRevision,
            string operatorIdentity,
            string reason)
        {
            return string.Equals(
                    CurrentEndpointIp,
                    NormalizeEndpointIp(currentEndpointIp),
                    StringComparison.Ordinal)
                && CurrentEndpointPort == currentEndpointPort
                && CurrentDiagnosticsBuild == currentDiagnosticsBuild
                && CurrentDiagnosticsBootId == currentDiagnosticsBootId
                && CurrentMapRevision == currentMapRevision
                && string.Equals(
                    OperatorIdentity,
                    operatorIdentity,
                    StringComparison.Ordinal)
                && string.Equals(Reason, reason, StringComparison.Ordinal);
        }

        private static string NormalizeEndpointIp(string endpointIp)
        {
            IPAddress parsed;
            if (string.IsNullOrWhiteSpace(endpointIp)
                || !IPAddress.TryParse(endpointIp, out parsed)
                || parsed.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException(
                    "Current retirement endpoint must be an IPv4 literal.",
                    "currentEndpointIp");
            }
            return parsed.ToString();
        }

        private static void ValidateDecisionText(
            string value,
            string parameterName,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > maximumLength)
            {
                throw new ArgumentException(
                    "Retirement decision text is missing or too long.",
                    parameterName);
            }
            foreach (var character in value)
            {
                if (char.IsControl(character))
                {
                    throw new ArgumentException(
                        "Retirement decision text cannot contain control characters.",
                        parameterName);
                }
            }
        }
    }

    internal sealed class RecoveryRecordRetirementLedger : IDisposable
    {
        private const int LegacyFormatVersion = 1;
        private const int PreviousFormatVersion = 2;
        private const int FormatVersion = 3;
        private const int ChecksumLength = 32;
        private const int MaximumEntryLength = 65536;
        private const int MaximumTextByteLength = 8192;
        private const int MaximumEntryCount = 4096;
        private const uint MoveFileWriteThrough = 0x00000008;
        private const string LockFileName = "retirement.lock";
        private const string EntryExtension = ".retired";
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMORET1");
        private static readonly Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly List<RecoveryRecordRetirementDecision> decisions =
            new List<RecoveryRecordRetirementDecision>();
        private FileStream lockStream;
        private bool disposed;

        [DllImport(
            "kernel32.dll",
            EntryPoint = "MoveFileExW",
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveFileExWriteThrough(
            string existingFileName,
            string newFileName,
            uint flags);

        private RecoveryRecordRetirementLedger(string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "Retirement ledger directory is required.",
                    "requestedDirectoryPath");
            }

            directoryPath = Path.GetFullPath(requestedDirectoryPath);
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
                LoadCommittedDecisions();
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

        internal IReadOnlyList<RecoveryRecordRetirementDecision>
            CommittedDecisions
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    var copies = new List<RecoveryRecordRetirementDecision>(
                        decisions.Count);
                    foreach (var decision in decisions)
                    {
                        copies.Add(decision.Copy());
                    }
                    return new ReadOnlyCollection<
                        RecoveryRecordRetirementDecision>(copies);
                }
            }
        }

        internal static RecoveryRecordRetirementLedger Open(
            string directoryPath)
        {
            return new RecoveryRecordRetirementLedger(directoryPath);
        }

        internal static RecoveryRecordRetirementLedger OpenDefault()
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
                "RecoveryRecordRetirementLedger",
                "v1");
        }

        internal RecoveryRecordRetirementDecision CommitOperatorRetirement(
            RecoveryJournalSourceEvidence sourceEvidence,
            string currentEndpointIp,
            int currentEndpointPort,
            uint currentDiagnosticsBuild,
            uint currentDiagnosticsBootId,
            uint currentMapRevision,
            string operatorIdentity,
            string reason,
            DateTime decisionUtc)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (sourceEvidence == null)
                {
                    throw new ArgumentNullException("sourceEvidence");
                }

                var existing = FindExactCore(sourceEvidence);
                if (existing != null)
                {
                    if (!existing.MatchesDecisionContext(
                        currentEndpointIp,
                        currentEndpointPort,
                        currentDiagnosticsBuild,
                        currentDiagnosticsBootId,
                        currentMapRevision,
                        operatorIdentity,
                        reason))
                    {
                        throw new InvalidOperationException(
                            "The exact recovery source already has an immutable retirement decision with different context.");
                    }
                    return existing.Copy();
                }

                var decision = new RecoveryRecordRetirementDecision(
                    Guid.NewGuid(),
                    sourceEvidence,
                    currentEndpointIp,
                    currentEndpointPort,
                    currentDiagnosticsBuild,
                    currentDiagnosticsBootId,
                    currentMapRevision,
                    operatorIdentity,
                    reason,
                    decisionUtc);
                var durableEntrySha256 = PersistDecision(decision);
                decision.MarkDurablyCommitted(durableEntrySha256);
                decisions.Add(decision);
                return decision.Copy();
            }
        }

        internal RecoveryRecordRetirementDecision CommitOperatorRetirement(
            RecoveryJournalSourceEvidence sourceEvidence,
            string currentEndpointIp,
            int currentEndpointPort,
            uint currentDiagnosticsBootId,
            uint currentMapRevision,
            string operatorIdentity,
            string reason,
            DateTime decisionUtc)
        {
            return CommitOperatorRetirement(
                sourceEvidence,
                currentEndpointIp,
                currentEndpointPort,
                0,
                currentDiagnosticsBootId,
                currentMapRevision,
                operatorIdentity,
                reason,
                decisionUtc);
        }

        internal RecoveryRecordRetirementDecision FindPendingDecision(
            RecoveryJournalSourceEvidence activeEvidence)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var decision = FindExactCore(activeEvidence);
                return decision == null ? null : decision.Copy();
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

        private RecoveryRecordRetirementDecision FindExactCore(
            RecoveryJournalSourceEvidence sourceEvidence)
        {
            if (sourceEvidence == null)
            {
                return null;
            }
            foreach (var decision in decisions)
            {
                if (decision.MatchesSourceEvidence(sourceEvidence))
                {
                    return decision;
                }
            }
            return null;
        }

        private void LoadCommittedDecisions()
        {
            var files = Directory.GetFiles(
                directoryPath,
                "*" + EntryExtension,
                SearchOption.TopDirectoryOnly);
            if (files.Length > MaximumEntryCount)
            {
                throw new InvalidDataException(
                    "Retirement ledger contains too many immutable entries.");
            }
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var decision = LoadDecision(file);
                if (FindExactCore(decision.SourceEvidence) != null)
                {
                    throw new InvalidDataException(
                        "Retirement ledger contains duplicate exact-source decisions.");
                }
                var expectedName = BuildEntryFileName(
                    decision.SourceEvidence);
                if (!string.Equals(
                    Path.GetFileName(file),
                    expectedName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        "Retirement ledger entry name does not match its source evidence.");
                }
                decisions.Add(decision);
            }
        }

        private string PersistDecision(
            RecoveryRecordRetirementDecision decision)
        {
            var bytes = SerializeDecision(decision);
            var durableEntrySha256 =
                RecoveryJournalSourceEvidence.ComputeSha256(bytes);
            var finalPath = Path.Combine(
                directoryPath,
                BuildEntryFileName(decision.SourceEvidence));
            var temporaryPath = finalPath
                + "."
                + Guid.NewGuid().ToString("N")
                + ".tmp";
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
                if (File.Exists(finalPath))
                {
                    throw new IOException(
                        "An immutable retirement decision already exists for the exact source.");
                }
                if (!MoveFileExWriteThrough(
                    temporaryPath,
                    finalPath,
                    MoveFileWriteThrough))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    throw new IOException(
                        "The immutable retirement decision could not be "
                        + "published durably.",
                        new Win32Exception(errorCode));
                }

                VerifyPublishedDecisionBytes(
                    finalPath,
                    bytes,
                    durableEntrySha256);
                temporaryExists = false;
                return durableEntrySha256;
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

        private static void VerifyPublishedDecisionBytes(
            string finalPath,
            byte[] expectedBytes,
            string expectedSha256)
        {
            var actualBytes = File.ReadAllBytes(finalPath);
            if (!RecoveryJournalSourceEvidence.ConstantTimeEquals(
                    expectedBytes,
                    actualBytes)
                || !string.Equals(
                    RecoveryJournalSourceEvidence.ComputeSha256(actualBytes),
                    expectedSha256,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The durably published retirement decision does not "
                    + "match the exact serialized decision bytes.");
            }
        }

        private static string BuildEntryFileName(
            RecoveryJournalSourceEvidence evidence)
        {
            return ((int)evidence.Owner).ToString(
                    "D2",
                    CultureInfo.InvariantCulture)
                + "-"
                + evidence.RecordIdentity.ToString("N")
                + "-"
                + evidence.OriginalSha256
                + EntryExtension;
        }

        private static RecoveryRecordRetirementDecision LoadDecision(
            string path)
        {
            byte[] bytes;
            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            {
                if (stream.Length < Magic.Length + 8 + ChecksumLength
                    || stream.Length > MaximumEntryLength)
                {
                    throw new InvalidDataException(
                        "Retirement ledger entry length is invalid.");
                }
                bytes = new byte[checked((int)stream.Length)];
                ReadExactly(stream, bytes);
            }
            var decision = DeserializeDecision(bytes);
            decision.MarkDurablyCommitted(
                RecoveryJournalSourceEvidence.ComputeSha256(bytes));
            return decision;
        }

        private static byte[] SerializeDecision(
            RecoveryRecordRetirementDecision decision)
        {
            var source = decision.SourceEvidence;
            byte[] payload;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(
                stream,
                Encoding.UTF8,
                true))
            {
                writer.Write(decision.DecisionIdentity.ToByteArray());
                writer.Write((int)source.Owner);
                writer.Write((int)source.EndpointEvidenceKind);
                writer.Write(source.RecordIdentity.ToByteArray());
                writer.Write(source.StateCode);
                writer.Write(source.CreatedUtc.Ticks);
                writer.Write(source.UpdatedUtc.Ticks);
                writer.Write(source.DiagnosticsBuild);
                writer.Write(source.DiagnosticsBootId);
                writer.Write(source.MapRevision);
                writer.Write(source.EndpointPort);
                writer.Write(source.TargetReference);
                writer.Write(decision.DecisionUtc.Ticks);
                writer.Write(decision.CurrentDiagnosticsBuild);
                writer.Write(decision.CurrentDiagnosticsBootId);
                writer.Write(decision.CurrentMapRevision);
                writer.Write(decision.CurrentEndpointPort);
                WriteText(writer, source.EndpointIp);
                WriteText(writer, source.TargetKind);
                WriteText(writer, source.TargetName);
                WriteText(writer, source.Operation);
                WriteText(writer, source.SemanticFingerprint);
                WriteText(writer, source.OriginalSha256);
                WriteText(writer, decision.CurrentEndpointIp);
                WriteText(writer, decision.OperatorIdentity);
                WriteText(writer, decision.Reason);
                var original = source.GetOriginalBytes();
                writer.Write(original.Length);
                writer.Write(original);
                writer.Flush();
                payload = stream.ToArray();
            }

            byte[] prefix;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(
                stream,
                Encoding.ASCII,
                true))
            {
                writer.Write(Magic);
                writer.Write(FormatVersion);
                writer.Write(payload.Length);
                writer.Write(payload);
                writer.Flush();
                prefix = stream.ToArray();
            }
            if (prefix.Length + ChecksumLength > MaximumEntryLength)
            {
                throw new InvalidOperationException(
                    "Retirement ledger entry exceeds the size limit.");
            }

            byte[] checksum;
            using (var algorithm = SHA256.Create())
            {
                checksum = algorithm.ComputeHash(prefix);
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

        private static RecoveryRecordRetirementDecision DeserializeDecision(
            byte[] bytes)
        {
            try
            {
                if (bytes == null
                    || bytes.Length < Magic.Length + 8 + ChecksumLength
                    || bytes.Length > MaximumEntryLength)
                {
                    throw new InvalidDataException(
                        "Retirement ledger entry length is invalid.");
                }
                var checksumOffset = bytes.Length - ChecksumLength;
                byte[] computed;
                using (var algorithm = SHA256.Create())
                {
                    computed = algorithm.ComputeHash(
                        bytes,
                        0,
                        checksumOffset);
                }
                var actual = new byte[ChecksumLength];
                Buffer.BlockCopy(
                    bytes,
                    checksumOffset,
                    actual,
                    0,
                    actual.Length);
                if (!RecoveryJournalSourceEvidence.ConstantTimeEquals(
                    computed,
                    actual))
                {
                    throw new InvalidDataException(
                        "Retirement ledger entry checksum is invalid.");
                }

                using (var stream = new MemoryStream(
                    bytes,
                    0,
                    checksumOffset,
                    false))
                using (var reader = new BinaryReader(
                    stream,
                    Encoding.UTF8,
                    true))
                {
                    if (!RecoveryJournalSourceEvidence.ConstantTimeEquals(
                        Magic,
                        reader.ReadBytes(Magic.Length)))
                    {
                        throw new InvalidDataException(
                            "Retirement ledger entry header is invalid.");
                    }
                    var formatVersion = reader.ReadInt32();
                    if (formatVersion != LegacyFormatVersion
                        && formatVersion != PreviousFormatVersion
                        && formatVersion != FormatVersion)
                    {
                        throw new InvalidDataException(
                            "Retirement ledger entry version is unsupported.");
                    }
                    var payloadLength = reader.ReadInt32();
                    if (payloadLength <= 0
                        || payloadLength
                            != checksumOffset - Magic.Length - 8)
                    {
                        throw new InvalidDataException(
                            "Retirement ledger payload length is invalid.");
                    }

                    var decisionIdentity = ReadGuid(reader);
                    var owner = (RecoveryRecordOwner)reader.ReadInt32();
                    var endpointEvidenceKind = formatVersion >= 3
                        ? (RecoveryEndpointEvidenceKind)reader.ReadInt32()
                        : RecoveryEndpointEvidenceKind
                            .RecordedSourceEndpoint;
                    var recordIdentity = ReadGuid(reader);
                    var stateCode = reader.ReadInt32();
                    var createdUtc = new DateTime(
                        reader.ReadInt64(),
                        DateTimeKind.Utc);
                    var updatedUtc = new DateTime(
                        reader.ReadInt64(),
                        DateTimeKind.Utc);
                    var sourceBuild = formatVersion >= 2
                        ? reader.ReadUInt32()
                        : 0;
                    var sourceBootId = reader.ReadUInt32();
                    var sourceMapRevision = reader.ReadUInt32();
                    var sourcePort = reader.ReadInt32();
                    var targetReference = reader.ReadUInt16();
                    var decisionUtc = new DateTime(
                        reader.ReadInt64(),
                        DateTimeKind.Utc);
                    var currentBuild = formatVersion >= 2
                        ? reader.ReadUInt32()
                        : 0;
                    var currentBootId = reader.ReadUInt32();
                    var currentMapRevision = reader.ReadUInt32();
                    var currentPort = reader.ReadInt32();
                    var sourceIp = ReadText(reader);
                    var targetKind = ReadText(reader);
                    var targetName = ReadText(reader);
                    var operation = ReadText(reader);
                    var semanticFingerprint = ReadText(reader);
                    var storedSha256 = ReadText(reader);
                    var currentIp = ReadText(reader);
                    var operatorIdentity = ReadText(reader);
                    var reason = ReadText(reader);
                    var originalLength = reader.ReadInt32();
                    if (originalLength < 1 || originalLength > 32768)
                    {
                        throw new InvalidDataException(
                            "Retirement source evidence length is invalid.");
                    }
                    var original = reader.ReadBytes(originalLength);
                    if (original.Length != originalLength
                        || stream.Position != checksumOffset)
                    {
                        throw new InvalidDataException(
                            "Retirement ledger entry is incomplete or has trailing data.");
                    }

                    var evidence = new RecoveryJournalSourceEvidence(
                        owner,
                        recordIdentity,
                        stateCode,
                        createdUtc,
                        updatedUtc,
                        sourceIp,
                        sourcePort,
                        sourceBuild,
                        sourceBootId,
                        sourceMapRevision,
                        targetKind,
                        targetName,
                        targetReference,
                        operation,
                        semanticFingerprint,
                        original,
                        endpointEvidenceKind);
                    if (!string.Equals(
                        storedSha256,
                        evidence.OriginalSha256,
                        StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(
                            "Retirement source SHA-256 does not match its exact bytes.");
                    }
                    return new RecoveryRecordRetirementDecision(
                        decisionIdentity,
                        evidence,
                        currentIp,
                        currentPort,
                        currentBuild,
                        currentBootId,
                        currentMapRevision,
                        operatorIdentity,
                        reason,
                        decisionUtc);
                }
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception error) when (
                error is EndOfStreamException
                || error is ArgumentException
                || error is ArgumentOutOfRangeException
                || error is InvalidOperationException
                || error is OverflowException)
            {
                throw new InvalidDataException(
                    "Retirement ledger entry is invalid.",
                    error);
            }
        }

        private static Guid ReadGuid(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(16);
            if (bytes.Length != 16)
            {
                throw new InvalidDataException(
                    "Retirement ledger GUID is incomplete.");
            }
            return new Guid(bytes);
        }

        private static void WriteText(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length < 1 || bytes.Length > MaximumTextByteLength)
            {
                throw new InvalidOperationException(
                    "Retirement ledger text length is invalid.");
            }
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadText(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length < 1 || length > MaximumTextByteLength)
            {
                throw new InvalidDataException(
                    "Retirement ledger text length is invalid.");
            }
            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException(
                    "Retirement ledger text is incomplete.");
            }
            return StrictUtf8.GetString(bytes);
        }

        private static void ReadExactly(Stream stream, byte[] bytes)
        {
            var offset = 0;
            while (offset < bytes.Length)
            {
                var read = stream.Read(bytes, offset, bytes.Length - offset);
                if (read <= 0)
                {
                    throw new InvalidDataException(
                        "Retirement ledger entry is truncated.");
                }
                offset += read;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    "RecoveryRecordRetirementLedger");
            }
        }
    }
}

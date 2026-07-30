using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal enum RecorderDoubleRecoveryState
    {
        ArmedBeforeConfigureDispatch = 1,
        ConfigurationIdentified = 2,
        CaptureEvidenceAvailable = 3,
        Resolved = 4,
        ResolvedWithoutConfiguration = 5
    }

    internal enum RecorderDoubleRecoveryTokenMarker
    {
        LegacyUnbound = 0,
        ClientTokenV1 = 1
    }

    internal sealed class RecorderDoubleRecoveryBankEvidence
    {
        internal RecorderDoubleRecoveryBankEvidence(
            uint bufferId,
            uint recordId)
        {
            if (bufferId > 1)
            {
                throw new ArgumentOutOfRangeException(
                    "bufferId",
                    "Double-bank recovery accepts only BufferId 0 or 1.");
            }

            if (recordId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordId",
                    "Double-bank recovery requires a non-zero RecordId.");
            }

            BufferId = bufferId;
            RecordId = recordId;
        }

        internal uint BufferId { get; private set; }
        internal uint RecordId { get; private set; }
    }

    internal sealed class RecorderDoubleRecoveryRecord
    {
        private readonly ReadOnlyCollection<
            RecorderDoubleRecoveryBankEvidence> banks;

        internal RecorderDoubleRecoveryRecord(
            Guid identity,
            RecorderDoubleRecoveryState state,
            DateTime createdUtc,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint mapRevision,
            uint requestedConfigId,
            uint configRevision,
            IReadOnlyList<RecorderDoubleRecoveryBankEvidence> banks,
            byte bankReleaseIntentMask = 0,
            byte bankReleaseConfirmedMask = 0,
            bool configurationReleaseIntent = false,
            bool configurationReleaseConfirmed = false,
            RecorderDoubleRecoveryTokenMarker recoveryTokenMarker =
                RecorderDoubleRecoveryTokenMarker.LegacyUnbound)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "Double-bank recovery identity cannot be empty.",
                    "identity");
            }

            ValidateState(state);
            ValidateRecoveryTokenMarker(recoveryTokenMarker);
            ValidateTimestamps(createdUtc, updatedUtc);
            ValidateNonZero(
                diagnosticsBootId,
                "diagnosticsBootId",
                "DiagnosticsBootId");
            ValidateNonZero(mapRevision, "mapRevision", "MapRevision");
            ValidateNonZero(
                requestedConfigId,
                "requestedConfigId",
                "RequestedConfigId");

            var bankCopy = CopyAndValidateBanks(banks);
            if (state
                    == RecorderDoubleRecoveryState
                        .ArmedBeforeConfigureDispatch)
            {
                if (configRevision != 0 || bankCopy.Count != 0)
                {
                    throw new ArgumentException(
                        "Pre-dispatch recovery evidence cannot contain a configuration revision or bank identity.");
                }
            }
            else if (state
                    == RecorderDoubleRecoveryState
                        .ConfigurationIdentified)
            {
                if (configRevision == 0 || bankCopy.Count != 0)
                {
                    throw new ArgumentException(
                        "Identified configuration evidence requires a revision and no bank identity.");
                }
            }
            else if (state
                    == RecorderDoubleRecoveryState
                        .CaptureEvidenceAvailable)
            {
                if (configRevision == 0 || bankCopy.Count == 0)
                {
                    throw new ArgumentException(
                        "Capture recovery evidence requires a configuration revision and at least one bank identity.");
                }
            }
            else if (state
                    == RecorderDoubleRecoveryState
                        .ResolvedWithoutConfiguration)
            {
                if (recoveryTokenMarker
                        != RecorderDoubleRecoveryTokenMarker.ClientTokenV1
                    || configRevision != 0
                    || bankCopy.Count != 0
                    || bankReleaseIntentMask != 0
                    || bankReleaseConfirmedMask != 0
                    || configurationReleaseIntent
                    || configurationReleaseConfirmed)
                {
                    throw new ArgumentException(
                        "Token-qualified absence resolution cannot contain a configuration revision, bank identity, or Release checkpoint.");
                }
            }
            else if (bankCopy.Count != 0 && configRevision == 0)
            {
                throw new ArgumentException(
                    "Resolved capture evidence cannot lose its configuration revision.");
            }

            var expectedBankMask = ComputeBankMask(bankCopy);
            if ((bankReleaseIntentMask & ~expectedBankMask) != 0
                || (bankReleaseConfirmedMask & ~bankReleaseIntentMask) != 0
                || ((bankReleaseIntentMask != 0
                        || configurationReleaseIntent)
                    && configRevision == 0)
                || (configurationReleaseIntent
                    && bankReleaseConfirmedMask != expectedBankMask)
                || (configurationReleaseConfirmed
                    && (!configurationReleaseIntent
                        || state != RecorderDoubleRecoveryState.Resolved))
                || (state == RecorderDoubleRecoveryState.Resolved
                    && (!configurationReleaseConfirmed
                        || configRevision == 0)))
            {
                throw new ArgumentException(
                    "Double-bank durable Release checkpoints are inconsistent with the recorded configuration, banks, or terminal state.");
            }

            Identity = identity;
            State = state;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DiagnosticsBootId = diagnosticsBootId;
            MapRevision = mapRevision;
            RequestedConfigId = requestedConfigId;
            ConfigRevision = configRevision;
            BankReleaseIntentMask = bankReleaseIntentMask;
            BankReleaseConfirmedMask = bankReleaseConfirmedMask;
            ConfigurationReleaseIntent = configurationReleaseIntent;
            ConfigurationReleaseConfirmed = configurationReleaseConfirmed;
            RecoveryTokenMarker = recoveryTokenMarker;
            this.banks = new ReadOnlyCollection<
                RecorderDoubleRecoveryBankEvidence>(bankCopy);
        }

        internal Guid Identity { get; private set; }
        internal RecorderDoubleRecoveryState State { get; private set; }
        internal DateTime CreatedUtc { get; private set; }
        internal DateTime UpdatedUtc { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint MapRevision { get; private set; }
        internal uint RequestedConfigId { get; private set; }
        internal uint ConfigRevision { get; private set; }
        internal byte BankReleaseIntentMask { get; private set; }
        internal byte BankReleaseConfirmedMask { get; private set; }
        internal bool ConfigurationReleaseIntent { get; private set; }
        internal bool ConfigurationReleaseConfirmed { get; private set; }
        internal RecorderDoubleRecoveryTokenMarker RecoveryTokenMarker
        {
            get;
            private set;
        }
        internal Guid RecoveryToken
        {
            get
            {
                return RecoveryTokenMarker
                        == RecorderDoubleRecoveryTokenMarker.ClientTokenV1
                    ? Identity
                    : Guid.Empty;
            }
        }
        internal IReadOnlyList<RecorderDoubleRecoveryBankEvidence> Banks
        {
            get { return banks; }
        }

        internal bool IsActive
        {
            get
            {
                return State != RecorderDoubleRecoveryState.Resolved
                    && State != RecorderDoubleRecoveryState
                        .ResolvedWithoutConfiguration;
            }
        }

        internal bool HasReleaseOutcomeUncertain
        {
            get
            {
                return HasBankReleaseOutcomeUncertain
                    || HasConfigurationReleaseOutcomeUncertain;
            }
        }

        internal bool HasBankReleaseOutcomeUncertain
        {
            get
            {
                return (BankReleaseIntentMask
                    & ~BankReleaseConfirmedMask) != 0;
            }
        }

        internal bool HasConfigurationReleaseOutcomeUncertain
        {
            get
            {
                return ConfigurationReleaseIntent
                    && !ConfigurationReleaseConfirmed;
            }
        }

        internal bool IsBankReleaseConfirmed(uint bufferId, uint recordId)
        {
            var bank = FindExactBank(banks, bufferId, recordId);
            return bank != null
                && (BankReleaseConfirmedMask & BankBit(bufferId)) != 0;
        }

        internal bool IsBankReleasePending(uint bufferId, uint recordId)
        {
            var bank = FindExactBank(banks, bufferId, recordId);
            var bit = BankBit(bufferId);
            return bank != null
                && (BankReleaseIntentMask & bit) != 0
                && (BankReleaseConfirmedMask & bit) == 0;
        }

        internal RecorderDoubleRecoveryRecord WithBankReleaseIntent(
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint bufferId,
            uint recordId)
        {
            ValidateActiveReleaseUpdate(
                updatedUtc,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision);
            if (ConfigurationReleaseIntent)
            {
                throw new InvalidOperationException(
                    "A bank Release cannot start after configuration Release intent is durable.");
            }

            var bank = FindExactBank(banks, bufferId, recordId);
            if (bank == null)
            {
                throw new InvalidOperationException(
                    "Bank Release intent does not match one exact durable BufferId and RecordId.");
            }

            var bit = BankBit(bufferId);
            if ((BankReleaseIntentMask & bit) != 0)
            {
                throw new InvalidOperationException(
                    (BankReleaseConfirmedMask & bit) != 0
                        ? "The exact bank Release is already durably confirmed."
                        : "The exact bank Release outcome is uncertain and must not be dispatched again.");
            }

            return CopyWithReleaseCheckpoints(
                updatedUtc,
                (byte)(BankReleaseIntentMask | bit),
                BankReleaseConfirmedMask,
                ConfigurationReleaseIntent,
                ConfigurationReleaseConfirmed,
                State);
        }

        internal RecorderDoubleRecoveryRecord WithBankReleaseConfirmed(
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint bufferId,
            uint recordId)
        {
            ValidateActiveReleaseUpdate(
                updatedUtc,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision);
            if (FindExactBank(banks, bufferId, recordId) == null)
            {
                throw new InvalidOperationException(
                    "Bank Release confirmation does not match one exact durable BufferId and RecordId.");
            }

            var bit = BankBit(bufferId);
            if ((BankReleaseIntentMask & bit) == 0)
            {
                throw new InvalidOperationException(
                    "Bank Release cannot be confirmed before its durable intent checkpoint.");
            }

            if ((BankReleaseConfirmedMask & bit) != 0)
            {
                return this;
            }

            return CopyWithReleaseCheckpoints(
                updatedUtc,
                BankReleaseIntentMask,
                (byte)(BankReleaseConfirmedMask | bit),
                ConfigurationReleaseIntent,
                ConfigurationReleaseConfirmed,
                State);
        }

        internal RecorderDoubleRecoveryRecord WithConfigurationReleaseIntent(
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision)
        {
            ValidateActiveReleaseUpdate(
                updatedUtc,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision);
            if (ConfigurationReleaseIntent)
            {
                throw new InvalidOperationException(
                    "The exact configuration Release outcome is uncertain and must not be dispatched again.");
            }

            var expectedMask = ComputeBankMask(banks);
            if (BankReleaseConfirmedMask != expectedMask)
            {
                throw new InvalidOperationException(
                    "Every durable bank Release must be confirmed before configuration Release intent.");
            }

            return CopyWithReleaseCheckpoints(
                updatedUtc,
                BankReleaseIntentMask,
                BankReleaseConfirmedMask,
                true,
                false,
                State);
        }

        internal RecorderDoubleRecoveryRecord MergeEvidence(
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            IReadOnlyList<RecorderDoubleRecoveryBankEvidence> evidence)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException(
                    "Resolved Double-bank recovery evidence cannot be changed.");
            }

            ValidateUpdateTime(updatedUtc);
            ValidateExactIdentity(
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision);
            var incoming = CopyAndValidateBanks(evidence);
            var merged = new List<RecorderDoubleRecoveryBankEvidence>(
                banks.Count + incoming.Count);
            for (var index = 0; index < banks.Count; index++)
            {
                merged.Add(banks[index]);
            }

            var changed = ConfigRevision == 0;
            for (var index = 0; index < incoming.Count; index++)
            {
                var candidate = incoming[index];
                var existingByBuffer = FindByBuffer(
                    merged,
                    candidate.BufferId);
                if (existingByBuffer != null)
                {
                    if (existingByBuffer.RecordId != candidate.RecordId)
                    {
                        throw new InvalidOperationException(
                            "A durable Double-bank BufferId cannot be rebound to a different RecordId.");
                    }

                    continue;
                }

                var existingByRecord = FindByRecord(
                    merged,
                    candidate.RecordId);
                if (existingByRecord != null)
                {
                    throw new InvalidOperationException(
                        "A durable Double-bank RecordId cannot identify two buffers.");
                }

                merged.Add(candidate);
                changed = true;
            }

            if (changed
                && (BankReleaseIntentMask != 0
                    || ConfigurationReleaseIntent))
            {
                throw new InvalidOperationException(
                    "Durable Recorder evidence cannot expand after any Release intent checkpoint.");
            }

            if (!changed)
            {
                return this;
            }

            merged.Sort(CompareBanks);
            var nextState = merged.Count == 0
                ? RecorderDoubleRecoveryState.ConfigurationIdentified
                : RecorderDoubleRecoveryState.CaptureEvidenceAvailable;
            return new RecorderDoubleRecoveryRecord(
                Identity,
                nextState,
                CreatedUtc,
                updatedUtc,
                DiagnosticsBootId,
                MapRevision,
                RequestedConfigId,
                configRevision,
                merged,
                BankReleaseIntentMask,
                BankReleaseConfirmedMask,
                ConfigurationReleaseIntent,
                ConfigurationReleaseConfirmed,
                RecoveryTokenMarker);
        }

        internal RecorderDoubleRecoveryRecord ResolveAfterExactRelease(
            RecorderDoubleExactReleaseProof proof,
            DateTime updatedUtc)
        {
            if (proof == null)
            {
                throw new ArgumentNullException("proof");
            }

            if (proof.Identity != Identity)
            {
                throw new InvalidOperationException(
                    "Exact release proof does not match the active Double-bank recovery identity.");
            }

            if (proof.DiagnosticsBootId != DiagnosticsBootId
                || proof.ConfigId != RequestedConfigId
                || proof.ConfigRevision != ConfigRevision
                || proof.MapRevision != MapRevision)
            {
                throw new InvalidOperationException(
                    "Exact release proof does not match the durable Double-bank configuration and bank identity.");
            }

            for (var index = 0;
                index < proof.ReleasedBanks.Count;
                index++)
            {
                var released = proof.ReleasedBanks[index];
                if (FindExactBank(
                        banks,
                        released.BufferId,
                        released.RecordId) == null)
                {
                    throw new InvalidOperationException(
                        "Exact release proof does not match the durable Double-bank configuration and bank identity.");
                }
            }

            if (!IsActive)
            {
                throw new InvalidOperationException(
                    "Double-bank recovery evidence is already resolved.");
            }


            if (!ConfigurationReleaseIntent
                || BankReleaseConfirmedMask != ComputeBankMask(banks))
            {
                throw new InvalidOperationException(
                    "Configuration Release cannot resolve the journal without durable intent and every exact bank confirmation.");
            }

            ValidateUpdateTime(updatedUtc);
            return new RecorderDoubleRecoveryRecord(
                Identity,
                RecorderDoubleRecoveryState.Resolved,
                CreatedUtc,
                updatedUtc,
                DiagnosticsBootId,
                MapRevision,
                RequestedConfigId,
                ConfigRevision,
                banks,
                BankReleaseIntentMask,
                BankReleaseConfirmedMask,
                true,
                true,
                RecoveryTokenMarker);
        }

        internal RecorderDoubleRecoveryRecord
            ResolveAfterExactConfigurationAbsence(
                RecorderDoubleExactConfigurationAbsenceProof proof,
                DateTime updatedUtc)
        {
            if (proof == null)
            {
                throw new ArgumentNullException("proof");
            }

            if (proof.Identity != Identity
                || proof.DiagnosticsBootId != DiagnosticsBootId
                || proof.ConfigId != RequestedConfigId
                || proof.ConfigRevision != ConfigRevision
                || proof.MapRevision != MapRevision)
            {
                throw new InvalidOperationException(
                    "Typed configuration absence does not match the active durable Double-bank identity.");
            }

            if (!IsActive)
            {
                throw new InvalidOperationException(
                    "Double-bank recovery evidence is already resolved.");
            }

            if (!ConfigurationReleaseIntent
                || BankReleaseConfirmedMask != ComputeBankMask(banks))
            {
                throw new InvalidOperationException(
                    "Typed configuration absence cannot resolve the journal without durable configuration Release intent and every exact bank confirmation.");
            }

            ValidateUpdateTime(updatedUtc);
            return new RecorderDoubleRecoveryRecord(
                Identity,
                RecorderDoubleRecoveryState.Resolved,
                CreatedUtc,
                updatedUtc,
                DiagnosticsBootId,
                MapRevision,
                RequestedConfigId,
                ConfigRevision,
                banks,
                BankReleaseIntentMask,
                BankReleaseConfirmedMask,
                true,
                true,
                RecoveryTokenMarker);
        }

        internal RecorderDoubleRecoveryRecord
            ResolveWithoutConfiguration(
                DateTime updatedUtc,
                uint diagnosticsBootId,
                uint configId,
                uint mapRevision,
                Guid recoveryToken)
        {
            if (!IsActive
                || State != RecorderDoubleRecoveryState
                    .ArmedBeforeConfigureDispatch
                || RecoveryTokenMarker
                    != RecorderDoubleRecoveryTokenMarker.ClientTokenV1
                || ConfigRevision != 0
                || banks.Count != 0
                || HasReleaseOutcomeUncertain)
            {
                throw new InvalidOperationException(
                    "Only an active token-qualified pre-dispatch record can resolve without a Recorder configuration.");
            }

            ValidateUpdateTime(updatedUtc);
            if (diagnosticsBootId != DiagnosticsBootId
                || configId != RequestedConfigId
                || mapRevision != MapRevision
                || recoveryToken != RecoveryToken)
            {
                throw new InvalidOperationException(
                    "Typed recoverable configuration absence does not match the exact durable BootId, ConfigId, MapRevision, and recovery token.");
            }

            return new RecorderDoubleRecoveryRecord(
                Identity,
                RecorderDoubleRecoveryState
                    .ResolvedWithoutConfiguration,
                CreatedUtc,
                updatedUtc,
                DiagnosticsBootId,
                MapRevision,
                RequestedConfigId,
                0,
                banks,
                0,
                0,
                false,
                false,
                RecoveryTokenMarker);
        }

        private RecorderDoubleRecoveryRecord CopyWithReleaseCheckpoints(
            DateTime updatedUtc,
            byte bankIntentMask,
            byte bankConfirmedMask,
            bool configurationIntent,
            bool configurationConfirmed,
            RecorderDoubleRecoveryState state)
        {
            return new RecorderDoubleRecoveryRecord(
                Identity,
                state,
                CreatedUtc,
                updatedUtc,
                DiagnosticsBootId,
                MapRevision,
                RequestedConfigId,
                ConfigRevision,
                banks,
                bankIntentMask,
                bankConfirmedMask,
                configurationIntent,
                configurationConfirmed,
                RecoveryTokenMarker);
        }

        private void ValidateActiveReleaseUpdate(
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException(
                    "Resolved Double-bank recovery evidence cannot accept Release checkpoints.");
            }

            ValidateUpdateTime(updatedUtc);
            ValidateExactIdentity(
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision);
        }

        private void ValidateUpdateTime(DateTime updatedUtc)
        {
            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < UpdatedUtc)
            {
                throw new ArgumentOutOfRangeException(
                    "updatedUtc",
                    "Recovery evidence time must be UTC and cannot move backwards.");
            }
        }

        private void ValidateExactIdentity(
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision)
        {
            if (diagnosticsBootId == 0
                || configId == 0
                || configRevision == 0
                || mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "configRevision",
                    "Recovered Recorder identity values must all be non-zero.");
            }

            if (diagnosticsBootId != DiagnosticsBootId
                || configId != RequestedConfigId
                || mapRevision != MapRevision
                || (ConfigRevision != 0
                    && configRevision != ConfigRevision))
            {
                throw new InvalidOperationException(
                    "Recovered Recorder evidence does not match the armed BootId, ConfigId, ConfigRevision, and MapRevision identity.");
            }
        }

        private static List<RecorderDoubleRecoveryBankEvidence>
            CopyAndValidateBanks(
                IReadOnlyList<RecorderDoubleRecoveryBankEvidence> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("banks");
            }

            if (source.Count > 2)
            {
                throw new ArgumentOutOfRangeException(
                    "banks",
                    "Double-bank recovery stores at most two banks.");
            }

            var copy = new List<RecorderDoubleRecoveryBankEvidence>(
                source.Count);
            for (var index = 0; index < source.Count; index++)
            {
                var bank = source[index];
                if (bank == null)
                {
                    throw new ArgumentException(
                        "Double-bank recovery evidence cannot contain a null bank.",
                        "banks");
                }

                if (FindByBuffer(copy, bank.BufferId) != null
                    || FindByRecord(copy, bank.RecordId) != null)
                {
                    throw new ArgumentException(
                        "Double-bank recovery evidence contains a duplicate BufferId or RecordId.",
                        "banks");
                }

                copy.Add(
                    new RecorderDoubleRecoveryBankEvidence(
                        bank.BufferId,
                        bank.RecordId));
            }

            copy.Sort(CompareBanks);
            return copy;
        }

        private static RecorderDoubleRecoveryBankEvidence FindByBuffer(
            IList<RecorderDoubleRecoveryBankEvidence> source,
            uint bufferId)
        {
            for (var index = 0; index < source.Count; index++)
            {
                if (source[index].BufferId == bufferId)
                {
                    return source[index];
                }
            }

            return null;
        }

        private static RecorderDoubleRecoveryBankEvidence FindByRecord(
            IList<RecorderDoubleRecoveryBankEvidence> source,
            uint recordId)
        {
            for (var index = 0; index < source.Count; index++)
            {
                if (source[index].RecordId == recordId)
                {
                    return source[index];
                }
            }

            return null;
        }

        private static RecorderDoubleRecoveryBankEvidence FindExactBank(
            IList<RecorderDoubleRecoveryBankEvidence> source,
            uint bufferId,
            uint recordId)
        {
            var bank = FindByBuffer(source, bufferId);
            return bank != null && bank.RecordId == recordId
                ? bank
                : null;
        }

        private static byte BankBit(uint bufferId)
        {
            if (bufferId > 1)
            {
                throw new ArgumentOutOfRangeException(
                    "bufferId",
                    "Double-bank Release accepts only BufferId 0 or 1.");
            }

            return (byte)(1 << (int)bufferId);
        }

        private static byte ComputeBankMask(
            IList<RecorderDoubleRecoveryBankEvidence> source)
        {
            byte result = 0;
            for (var index = 0; index < source.Count; index++)
            {
                result |= BankBit(source[index].BufferId);
            }

            return result;
        }

        private static int CompareBanks(
            RecorderDoubleRecoveryBankEvidence left,
            RecorderDoubleRecoveryBankEvidence right)
        {
            return left.BufferId.CompareTo(right.BufferId);
        }

        private static void ValidateState(RecorderDoubleRecoveryState state)
        {
            if (state
                    < RecorderDoubleRecoveryState
                        .ArmedBeforeConfigureDispatch
                || state > RecorderDoubleRecoveryState
                    .ResolvedWithoutConfiguration)
            {
                throw new ArgumentOutOfRangeException("state");
            }
        }

        private static void ValidateRecoveryTokenMarker(
            RecorderDoubleRecoveryTokenMarker marker)
        {
            if (marker != RecorderDoubleRecoveryTokenMarker.LegacyUnbound
                && marker
                    != RecorderDoubleRecoveryTokenMarker.ClientTokenV1)
            {
                throw new ArgumentOutOfRangeException(
                    "recoveryTokenMarker");
            }
        }

        private static void ValidateTimestamps(
            DateTime createdUtc,
            DateTime updatedUtc)
        {
            if (createdUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Recovery evidence creation time must be UTC.",
                    "createdUtc");
            }

            if (updatedUtc.Kind != DateTimeKind.Utc
                || updatedUtc < createdUtc)
            {
                throw new ArgumentException(
                    "Recovery evidence update time must be UTC and cannot precede creation.",
                    "updatedUtc");
            }
        }

        private static void ValidateNonZero(
            uint value,
            string parameterName,
            string displayName)
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    displayName + " must be non-zero.");
            }
        }
    }

    internal sealed class RecorderDoubleRecoveryJournalCorruptException
        : IOException
    {
        internal RecorderDoubleRecoveryJournalCorruptException(
            string activeFilePath,
            string quarantineFilePath,
            Exception innerException)
            : base(
                "The Double-bank recovery journal is corrupt. "
                    + "The active file remains fail-closed and an exact quarantine copy was preserved at '"
                    + quarantineFilePath
                    + "'.",
                innerException)
        {
            ActiveFilePath = activeFilePath;
            QuarantineFilePath = quarantineFilePath;
        }

        internal string ActiveFilePath { get; private set; }
        internal string QuarantineFilePath { get; private set; }
    }

    internal sealed class RecorderDoubleRecoveryJournal : IDisposable
    {
        internal const string JournalFileName = "journal.dat";
        internal const string LockFileName = "journal.lock";
        internal const string QuarantineFilePrefix = "journal.corrupt.";

        private const int LegacyFormatVersion = 2;
        private const int FormatVersion = 3;
        private const int ChecksumLength = 32;
        private const int MaximumFileLength = 4096;
        private static readonly byte[] Magic =
            Encoding.ASCII.GetBytes("ELMORDJ1");

        private readonly object sync = new object();
        private readonly string directoryPath;
        private readonly string journalFilePath;
        private FileStream lockStream;
        private RecorderDoubleRecoveryRecord currentRecord;
        private bool disposed;

        private RecorderDoubleRecoveryJournal(string requestedDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(requestedDirectoryPath))
            {
                throw new ArgumentException(
                    "A Double-bank recovery journal directory is required.",
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
                try
                {
                    currentRecord = LoadRecord(journalFilePath);
                }
                catch (InvalidDataException error)
                {
                    var quarantinePath = PreserveQuarantineCopy(
                        journalFilePath);
                    throw new RecorderDoubleRecoveryJournalCorruptException(
                        journalFilePath,
                        quarantinePath,
                        error);
                }
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

        internal RecorderDoubleRecoveryRecord CurrentRecord
        {
            get
            {
                lock (sync)
                {
                    ThrowIfDisposed();
                    return currentRecord;
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

        internal static RecorderDoubleRecoveryJournal Open(
            string directoryPath)
        {
            return new RecorderDoubleRecoveryJournal(directoryPath);
        }

        internal static RecorderDoubleRecoveryJournal OpenDefault()
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
                "RecorderDoubleRecoveryJournal",
                "v1");
        }

        internal RecorderDoubleRecoveryRecord ArmBeforeConfigureDispatch(
            Guid identity,
            DateTime createdUtc,
            uint diagnosticsBootId,
            uint mapRevision,
            uint requestedConfigId)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                if (currentRecord != null && currentRecord.IsActive)
                {
                    throw new InvalidOperationException(
                        "An unresolved Double-bank recovery record already exists.");
                }

                var armed = new RecorderDoubleRecoveryRecord(
                    identity,
                    RecorderDoubleRecoveryState
                        .ArmedBeforeConfigureDispatch,
                    createdUtc,
                    createdUtc,
                    diagnosticsBootId,
                    mapRevision,
                    requestedConfigId,
                    0,
                    new RecorderDoubleRecoveryBankEvidence[0],
                    0,
                    0,
                    false,
                    false,
                    RecorderDoubleRecoveryTokenMarker.ClientTokenV1);
                PersistRecord(armed);
                currentRecord = armed;
                return armed;
            }
        }

        internal RecorderDoubleRecoveryRecord RecordConfigurationReply(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision)
        {
            return MergeEvidence(
                identity,
                updatedUtc,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                new RecorderDoubleRecoveryBankEvidence[0],
                true);
        }

        internal RecorderDoubleRecoveryRecord
            RecordRecoverableConfigurationIdentity(
                Guid identity,
                DateTime updatedUtc,
                uint diagnosticsBootId,
                uint configId,
                uint configRevision,
                uint mapRevision)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                if (current.ConfigRevision != 0
                    || current.RecoveryTokenMarker
                        != RecorderDoubleRecoveryTokenMarker.ClientTokenV1)
                {
                    throw new InvalidOperationException(
                        "Only a token-qualified pre-dispatch record may acquire its configuration revision from recoverable inventory.");
                }

                return MergeEvidence(
                    identity,
                    updatedUtc,
                    diagnosticsBootId,
                    configId,
                    configRevision,
                    mapRevision,
                    new RecorderDoubleRecoveryBankEvidence[0],
                    true);
            }
        }

        internal RecorderDoubleRecoveryRecord RecordCaptureReply(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint recordId,
            uint bufferId)
        {
            return MergeEvidence(
                identity,
                updatedUtc,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                new[]
                {
                    new RecorderDoubleRecoveryBankEvidence(
                        bufferId,
                        recordId)
                },
                false);
        }

        internal RecorderDoubleRecoveryRecord RecordInventory(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            IReadOnlyList<RecorderDoubleRecoveryBankEvidence> banks)
        {
            return MergeEvidence(
                identity,
                updatedUtc,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                banks,
                false);
        }

        internal RecorderDoubleRecoveryRecord BeginBankRelease(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint bufferId,
            uint recordId)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var next = current.WithBankReleaseIntent(
                    updatedUtc,
                    diagnosticsBootId,
                    configId,
                    configRevision,
                    mapRevision,
                    bufferId,
                    recordId);
                PersistRecord(next);
                currentRecord = next;
                return next;
            }
        }

        internal RecorderDoubleRecoveryRecord ConfirmBankRelease(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint bufferId,
            uint recordId)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var next = current.WithBankReleaseConfirmed(
                    updatedUtc,
                    diagnosticsBootId,
                    configId,
                    configRevision,
                    mapRevision,
                    bufferId,
                    recordId);
                if (!ReferenceEquals(current, next))
                {
                    PersistRecord(next);
                    currentRecord = next;
                }

                return currentRecord;
            }
        }

        internal RecorderDoubleRecoveryRecord BeginConfigurationRelease(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var next = current.WithConfigurationReleaseIntent(
                    updatedUtc,
                    diagnosticsBootId,
                    configId,
                    configRevision,
                    mapRevision);
                PersistRecord(next);
                currentRecord = next;
                return next;
            }
        }

        internal RecorderDoubleRecoveryRecord ResolveAfterExactRelease(
            RecorderDoubleExactReleaseProof proof,
            DateTime updatedUtc)
        {
            if (proof == null)
            {
                throw new ArgumentNullException("proof");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(proof.Identity);
                var resolved = current.ResolveAfterExactRelease(
                    proof,
                    updatedUtc);
                PersistRecord(resolved);
                currentRecord = resolved;
                return resolved;
            }
        }

        internal RecorderDoubleRecoveryRecord
            ResolveAfterExactConfigurationAbsence(
                RecorderDoubleExactConfigurationAbsenceProof proof,
                DateTime updatedUtc)
        {
            if (proof == null)
            {
                throw new ArgumentNullException("proof");
            }

            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(proof.Identity);
                var resolved = current
                    .ResolveAfterExactConfigurationAbsence(
                        proof,
                        updatedUtc);
                PersistRecord(resolved);
                currentRecord = resolved;
                return resolved;
            }
        }

        internal RecorderDoubleRecoveryRecord ResolveWithoutConfiguration(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint mapRevision,
            Guid recoveryToken)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                var resolved = current.ResolveWithoutConfiguration(
                    updatedUtc,
                    diagnosticsBootId,
                    configId,
                    mapRevision,
                    recoveryToken);
                PersistRecord(resolved);
                currentRecord = resolved;
                return resolved;
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

        private RecorderDoubleRecoveryRecord MergeEvidence(
            Guid identity,
            DateTime updatedUtc,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            IReadOnlyList<RecorderDoubleRecoveryBankEvidence> banks,
            bool allowConfigurationIdentification)
        {
            lock (sync)
            {
                ThrowIfDisposed();
                var current = RequireCurrentRecord(identity);
                if (current.ConfigRevision == 0
                    && !allowConfigurationIdentification)
                {
                    throw new InvalidOperationException(
                        "Only an exact Configure reply may identify a pre-dispatch Double-bank recovery record. Inventory and capture evidence remain fail-closed until the wire carries an exact recovery token.");
                }

                var merged = current.MergeEvidence(
                    updatedUtc,
                    diagnosticsBootId,
                    configId,
                    configRevision,
                    mapRevision,
                    banks);
                if (!ReferenceEquals(current, merged))
                {
                    PersistRecord(merged);
                    currentRecord = merged;
                }

                return currentRecord;
            }
        }

        private RecorderDoubleRecoveryRecord RequireCurrentRecord(
            Guid identity)
        {
            if (currentRecord == null)
            {
                throw new InvalidOperationException(
                    "No Double-bank recovery record exists.");
            }

            if (identity == Guid.Empty
                || currentRecord.Identity != identity)
            {
                throw new InvalidOperationException(
                    "Double-bank recovery identity does not match the durable record.");
            }

            return currentRecord;
        }

        private void PersistRecord(RecorderDoubleRecoveryRecord record)
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

        private static RecorderDoubleRecoveryRecord LoadRecord(string path)
        {
            if (!File.Exists(path))
            {
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
                    "The Double-bank recovery journal could not be read.",
                    error);
            }

            try
            {
                return DeserializeRecord(bytes);
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception error)
            {
                throw new InvalidDataException(
                    "The Double-bank recovery journal is corrupt.",
                    error);
            }
        }

        private static byte[] SerializeRecord(
            RecorderDoubleRecoveryRecord record)
        {
            byte[] payload;
            using (var payloadStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    payloadStream,
                    Encoding.UTF8,
                    true))
                {
                    writer.Write(record.Identity.ToByteArray());
                    writer.Write((int)record.State);
                    writer.Write(record.CreatedUtc.Ticks);
                    writer.Write(record.UpdatedUtc.Ticks);
                    writer.Write(record.DiagnosticsBootId);
                    writer.Write(record.MapRevision);
                    writer.Write(record.RequestedConfigId);
                    writer.Write(record.ConfigRevision);
                    writer.Write(record.Banks.Count);
                    for (var index = 0;
                        index < record.Banks.Count;
                        index++)
                    {
                        writer.Write(record.Banks[index].BufferId);
                        writer.Write(record.Banks[index].RecordId);
                    }

                    writer.Write(record.BankReleaseIntentMask);
                    writer.Write(record.BankReleaseConfirmedMask);
                    writer.Write(record.ConfigurationReleaseIntent);
                    writer.Write(record.ConfigurationReleaseConfirmed);
                    writer.Write((int)record.RecoveryTokenMarker);

                    writer.Flush();
                }

                payload = payloadStream.ToArray();
            }

            byte[] prefix;
            using (var fileStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(
                    fileStream,
                    Encoding.UTF8,
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

        private static RecorderDoubleRecoveryRecord DeserializeRecord(
            byte[] bytes)
        {
            if (bytes == null
                || bytes.Length < Magic.Length + 8 + ChecksumLength
                || bytes.Length > MaximumFileLength)
            {
                throw new InvalidDataException(
                    "Double-bank recovery journal length is invalid.");
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
                    "Double-bank recovery journal checksum is invalid.");
            }

            using (var fileStream = new MemoryStream(
                bytes,
                0,
                checksumOffset,
                false))
            using (var reader = new BinaryReader(
                fileStream,
                Encoding.UTF8,
                true))
            {
                var magic = reader.ReadBytes(Magic.Length);
                if (!ByteArraysEqual(Magic, magic))
                {
                    throw new InvalidDataException(
                        "Double-bank recovery journal magic is invalid.");
                }

                var version = reader.ReadInt32();
                if (version != LegacyFormatVersion
                    && version != FormatVersion)
                {
                    throw new NotSupportedException(
                        "Double-bank recovery journal version "
                            + version.ToString(CultureInfo.InvariantCulture)
                            + " is unsupported.");
                }

                var payloadLength = reader.ReadInt32();
                if (payloadLength <= 0
                    || payloadLength
                        != checksumOffset - (Magic.Length + 8))
                {
                    throw new InvalidDataException(
                        "Double-bank recovery journal payload length is invalid.");
                }

                var payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength
                    || fileStream.Position != checksumOffset)
                {
                    throw new InvalidDataException(
                        "Double-bank recovery journal payload is incomplete.");
                }

                return DeserializePayload(payload, version);
            }
        }

        private static RecorderDoubleRecoveryRecord DeserializePayload(
            byte[] payload,
            int version)
        {
            using (var stream = new MemoryStream(payload, false))
            using (var reader = new BinaryReader(
                stream,
                Encoding.UTF8,
                true))
            {
                var identityBytes = reader.ReadBytes(16);
                if (identityBytes.Length != 16)
                {
                    throw new InvalidDataException(
                        "Double-bank recovery identity is incomplete.");
                }

                var identity = new Guid(identityBytes);
                var state = (RecorderDoubleRecoveryState)reader.ReadInt32();
                var createdUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var updatedUtc = new DateTime(
                    reader.ReadInt64(),
                    DateTimeKind.Utc);
                var diagnosticsBootId = reader.ReadUInt32();
                var mapRevision = reader.ReadUInt32();
                var requestedConfigId = reader.ReadUInt32();
                var configRevision = reader.ReadUInt32();
                var bankCount = reader.ReadInt32();
                if (bankCount < 0 || bankCount > 2)
                {
                    throw new InvalidDataException(
                        "Double-bank recovery bank count is invalid.");
                }

                var banks = new List<RecorderDoubleRecoveryBankEvidence>(
                    bankCount);
                for (var index = 0; index < bankCount; index++)
                {
                    banks.Add(
                        new RecorderDoubleRecoveryBankEvidence(
                            reader.ReadUInt32(),
                            reader.ReadUInt32()));
                }

                var bankReleaseIntentMask = reader.ReadByte();
                var bankReleaseConfirmedMask = reader.ReadByte();
                var configurationReleaseIntent = reader.ReadBoolean();
                var configurationReleaseConfirmed = reader.ReadBoolean();
                var recoveryTokenMarker = version == LegacyFormatVersion
                    ? RecorderDoubleRecoveryTokenMarker.LegacyUnbound
                    : (RecorderDoubleRecoveryTokenMarker)
                        reader.ReadInt32();

                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException(
                        "Double-bank recovery journal has trailing payload data.");
                }

                try
                {
                    return new RecorderDoubleRecoveryRecord(
                        identity,
                        state,
                        createdUtc,
                        updatedUtc,
                        diagnosticsBootId,
                        mapRevision,
                        requestedConfigId,
                        configRevision,
                        banks,
                        bankReleaseIntentMask,
                        bankReleaseConfirmedMask,
                        configurationReleaseIntent,
                        configurationReleaseConfirmed,
                        recoveryTokenMarker);
                }
                catch (ArgumentException error)
                {
                    throw new InvalidDataException(
                        "Double-bank recovery journal record is invalid.",
                        error);
                }
            }
        }

        private static string PreserveQuarantineCopy(string sourcePath)
        {
            string digest;
            using (var source = new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            using (var sha256 = SHA256.Create())
            {
                digest = BitConverter.ToString(
                    sha256.ComputeHash(source))
                    .Replace("-", string.Empty);
            }

            var directory = Path.GetDirectoryName(sourcePath);
            var quarantinePath = Path.Combine(
                directory,
                QuarantineFilePrefix + digest + ".dat");
            if (File.Exists(quarantinePath))
            {
                return quarantinePath;
            }

            var temporaryPath = quarantinePath
                + "."
                + Guid.NewGuid().ToString("N")
                + ".tmp";
            var temporaryExists = false;
            try
            {
                using (var source = new FileStream(
                    sourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                using (var target = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough))
                {
                    temporaryExists = true;
                    source.CopyTo(target);
                    target.Flush(true);
                }

                try
                {
                    File.Move(temporaryPath, quarantinePath);
                    temporaryExists = false;
                }
                catch (IOException)
                {
                    if (!File.Exists(quarantinePath))
                    {
                        throw;
                    }
                }

                return quarantinePath;
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
                        // Preserve the primary quarantine failure.
                    }
                }
            }
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
                    "RecorderDoubleRecoveryJournal");
            }
        }
    }
}

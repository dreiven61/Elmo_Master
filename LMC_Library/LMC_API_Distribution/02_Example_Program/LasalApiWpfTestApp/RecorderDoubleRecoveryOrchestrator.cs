using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class RecorderDoubleRecoveryResult
    {
        private readonly ReadOnlyCollection<LMCRecorderIdentity>
            adoptedBanks;

        internal RecorderDoubleRecoveryResult(
            RecorderDoubleRecoveryPlan plan,
            LMCRecorderBankInventory inventory,
            LMCRecoveredRecorderConfigurationLease adoptedConfiguration,
            IList<LMCRecorderIdentity> adoptedBanks)
        {
            Plan = plan ?? throw new ArgumentNullException("plan");
            Inventory = inventory
                ?? throw new ArgumentNullException("inventory");
            AdoptedConfiguration = adoptedConfiguration;
            this.adoptedBanks = new ReadOnlyCollection<
                LMCRecorderIdentity>(adoptedBanks);
        }

        internal RecorderDoubleRecoveryResult(
            RecorderDoubleRecoveryRecord resolvedRecord,
            LMCRecorderConfigurationAbsentException configurationAbsence)
        {
            if (resolvedRecord == null)
            {
                throw new ArgumentNullException("resolvedRecord");
            }

            if (configurationAbsence == null)
            {
                throw new ArgumentNullException("configurationAbsence");
            }

            if (resolvedRecord.IsActive)
            {
                throw new InvalidOperationException(
                    "Typed configuration absence must resolve the durable Double-bank record before returning.");
            }

            ResolvedRecord = resolvedRecord;
            ConfigurationAbsence = configurationAbsence;
            adoptedBanks = new ReadOnlyCollection<LMCRecorderIdentity>(
                new LMCRecorderIdentity[0]);
        }

        internal RecorderDoubleRecoveryResult(
            RecorderDoubleRecoveryRecord resolvedRecord,
            LMCRecoverableRecorderConfigurationAbsentException
                recoverableConfigurationAbsence)
        {
            if (resolvedRecord == null)
            {
                throw new ArgumentNullException("resolvedRecord");
            }

            if (recoverableConfigurationAbsence == null)
            {
                throw new ArgumentNullException(
                    "recoverableConfigurationAbsence");
            }

            if (resolvedRecord.IsActive
                || resolvedRecord.State
                    != RecorderDoubleRecoveryState
                        .ResolvedWithoutConfiguration)
            {
                throw new InvalidOperationException(
                    "Typed recoverable configuration absence must durably resolve without configuration before returning.");
            }

            ResolvedRecord = resolvedRecord;
            RecoverableConfigurationAbsence =
                recoverableConfigurationAbsence;
            adoptedBanks = new ReadOnlyCollection<LMCRecorderIdentity>(
                new LMCRecorderIdentity[0]);
        }

        internal RecorderDoubleRecoveryPlan Plan { get; private set; }
        internal LMCRecorderBankInventory Inventory { get; private set; }
        internal LMCRecoveredRecorderConfigurationLease
            AdoptedConfiguration { get; private set; }
        internal RecorderDoubleRecoveryRecord ResolvedRecord
        {
            get;
            private set;
        }
        internal LMCRecorderConfigurationAbsentException
            ConfigurationAbsence
        {
            get;
            private set;
        }
        internal LMCRecoverableRecorderConfigurationAbsentException
            RecoverableConfigurationAbsence
        {
            get;
            private set;
        }
        internal bool IsResolvedByConfigurationAbsence
        {
            get { return ResolvedRecord != null; }
        }
        internal IReadOnlyList<LMCRecorderIdentity>
            AdoptedBanks
        {
            get { return adoptedBanks; }
        }
    }

    internal sealed class RecorderDoubleRecoveryOperations
    {
        internal Func<RecorderDoubleRecoveryRecord, CancellationToken,
            Task<LMCRecorderBankInventory>> ReadRecoverableInventoryAsync
        {
            get;
            set;
        }

        internal Func<RecorderDoubleRecoveryRecord, CancellationToken,
            Task<LMCRecorderBankInventory>> ReadInventoryAsync
        {
            get;
            set;
        }

        internal Func<RecorderDoubleRecoveryPlan,
            RecorderDoubleRecoveryBankTarget,
            Task<LMCRecorderIdentity>> AdoptBankAsync
        {
            get;
            set;
        }

        internal Func<RecorderDoubleRecoveryPlan,
            LMCRecorderBankInventory,
            Task<LMCRecoveredRecorderConfigurationLease>>
            AdoptEmptyConfigurationAsync
        {
            get;
            set;
        }

        internal Action<RecorderDoubleRecoveryPlan>
            EnsureMutationPlanConfirmed { get; set; }

        internal Func<DateTime> UtcNow { get; set; }

        internal void Validate()
        {
            if (ReadRecoverableInventoryAsync == null
                || ReadInventoryAsync == null
                || AdoptBankAsync == null
                || AdoptEmptyConfigurationAsync == null
                || EnsureMutationPlanConfirmed == null
                || UtcNow == null)
            {
                throw new ArgumentException(
                    "All Double-bank recovery operations are required.");
            }
        }
    }

    internal static class RecorderDoubleRecoveryOrchestrator
    {
        internal static async Task<RecorderDoubleRecoveryResult> RunAsync(
            RecorderDoubleRecoveryJournal journal,
            RecorderDoubleRecoveryOperations operations,
            CancellationToken cancellationToken)
        {
            if (journal == null)
            {
                throw new ArgumentNullException("journal");
            }

            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }

            operations.Validate();
            var record = journal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    "An active durable Double-bank recovery record is required.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var identifiedFromRecoverableInventory = false;
            if (record.ConfigRevision == 0)
            {
                if (record.RecoveryTokenMarker
                        != RecorderDoubleRecoveryTokenMarker.ClientTokenV1
                    || record.RecoveryToken == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "A legacy-unbound pre-dispatch Double-bank recovery record cannot send any recovery wire request.");
                }

                LMCRecorderBankInventory recoverableInventory;
                try
                {
                    recoverableInventory = await operations
                        .ReadRecoverableInventoryAsync(
                            record,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (LMCRecoverableRecorderConfigurationAbsentException
                    absence)
                {
                    if (absence.Response == null
                        || absence.Response.Detail
                            != LMCDiagnosticsDetailCode
                                .RecorderConfigurationAbsent)
                    {
                        throw new InvalidOperationException(
                            "Recoverable configuration absence is not an exact typed canonical-empty result.",
                            absence);
                    }

                    var resolved = journal.ResolveWithoutConfiguration(
                        record.Identity,
                        operations.UtcNow(),
                        absence.DiagnosticsBootId,
                        absence.ConfigId,
                        absence.MapRevision,
                        absence.RecoveryToken);
                    return new RecorderDoubleRecoveryResult(
                        resolved,
                        absence);
                }

                ValidateRecoverableInventory(record, recoverableInventory);
                record = journal.RecordRecoverableConfigurationIdentity(
                    record.Identity,
                    operations.UtcNow(),
                    recoverableInventory.DiagnosticsBootId,
                    recoverableInventory.ConfigId,
                    recoverableInventory.ConfigRevision,
                    recoverableInventory.MapRevision);
                identifiedFromRecoverableInventory = true;
            }

            LMCRecorderBankInventory inventory;
            try
            {
                inventory = await operations.ReadInventoryAsync(
                    record,
                    identifiedFromRecoverableInventory
                        ? CancellationToken.None
                        : cancellationToken).ConfigureAwait(false);
            }
            catch (LMCRecorderConfigurationAbsentException absence)
            {
                if (!record.HasConfigurationReleaseOutcomeUncertain)
                {
                    throw new InvalidOperationException(
                        "Typed Recorder configuration absence cannot resolve a durable record without a pending exact configuration Release intent.",
                        absence);
                }

                var proof = new
                    RecorderDoubleExactConfigurationAbsenceProof(
                        record.Identity,
                        absence);
                var resolved = journal
                    .ResolveAfterExactConfigurationAbsence(
                        proof,
                        operations.UtcNow());
                return new RecorderDoubleRecoveryResult(
                    resolved,
                    absence);
            }

            if (inventory == null
                || inventory.RecoveryToken != Guid.Empty
                || inventory.IsRecoverable)
            {
                throw new InvalidOperationException(
                    "The mandatory post-resolution inventory must be an exact standard 0x7E4A result without a recovery token.");
            }

            if (record.HasBankReleaseOutcomeUncertain)
            {
                var absentBanks = RecorderDoubleRecoveryPlanner
                    .FindPendingBankReleaseAbsences(record, inventory);
                for (var index = 0;
                    index < absentBanks.Count;
                    index++)
                {
                    var bank = absentBanks[index];
                    record = journal.ConfirmBankRelease(
                        record.Identity,
                        operations.UtcNow(),
                        record.DiagnosticsBootId,
                        record.RequestedConfigId,
                        record.ConfigRevision,
                        record.MapRevision,
                        bank.BufferId,
                        bank.RecordId);
                }

            }

            cancellationToken.ThrowIfCancellationRequested();

            var plan = RecorderDoubleRecoveryPlanner.Create(record, inventory);
            var discoveredBanks = new List<
                RecorderDoubleRecoveryBankEvidence>(plan.Banks.Count);
            for (var index = 0; index < plan.Banks.Count; index++)
            {
                var bank = plan.Banks[index];
                discoveredBanks.Add(
                    new RecorderDoubleRecoveryBankEvidence(
                        bank.BufferId,
                        bank.RecordId));
            }

            var persisted = journal.RecordInventory(
                record.Identity,
                operations.UtcNow(),
                plan.DiagnosticsBootId,
                plan.ConfigId,
                plan.ConfigRevision,
                plan.MapRevision,
                discoveredBanks);
            plan = RecorderDoubleRecoveryPlanner.Create(
                persisted,
                inventory);
            cancellationToken.ThrowIfCancellationRequested();

            if (plan.Route == RecorderDoubleRecoveryRoute
                    .CurrentSessionOwnsConfiguration)
            {
                throw new InvalidOperationException(
                    "The current diagnostics session already owns this Recorder configuration. Continue with its in-memory handles or reconnect before durable adoption; no recovery mutation was sent.");
            }

            operations.EnsureMutationPlanConfirmed(plan);

            if (plan.Route
                == RecorderDoubleRecoveryRoute.AdoptEmptyConfiguration)
            {
                var configuration = await operations
                    .AdoptEmptyConfigurationAsync(plan, inventory)
                    .ConfigureAwait(false);
                ValidateConfigurationAdoption(plan, configuration);
                return new RecorderDoubleRecoveryResult(
                    plan,
                    inventory,
                    configuration,
                    new LMCRecorderIdentity[0]);
            }

            if (plan.Route
                != RecorderDoubleRecoveryRoute.AdoptOccupiedBanks)
            {
                throw new InvalidOperationException(
                    "The Double-bank recovery route is unsupported.");
            }

            var adoptedBanks = new List<LMCRecorderIdentity>(
                plan.Banks.Count);
            uint adoptedOwnerSessionEpoch = 0;
            for (var index = 0; index < plan.Banks.Count; index++)
            {
                // The first exact Adopt rebinds every occupied bank. Once that
                // mutation starts, finish acquiring all exact bank handles even
                // if the caller cancels; cancellation is honored before it.
                var handle = await operations.AdoptBankAsync(
                    plan,
                    plan.Banks[index]).ConfigureAwait(false);
                ValidateBankAdoption(
                    plan,
                    plan.Banks[index],
                    handle,
                    adoptedOwnerSessionEpoch);
                if (adoptedOwnerSessionEpoch == 0)
                {
                    adoptedOwnerSessionEpoch = handle.OwnerSessionEpoch;
                }

                adoptedBanks.Add(handle);
            }

            return new RecorderDoubleRecoveryResult(
                plan,
                inventory,
                null,
                adoptedBanks);
        }

        private static void ValidateConfigurationAdoption(
            RecorderDoubleRecoveryPlan plan,
            LMCRecoveredRecorderConfigurationLease handle)
        {
            if (handle == null
                || handle.DiagnosticsBootId != plan.DiagnosticsBootId
                || handle.ConfigId != plan.ConfigId
                || handle.ConfigRevision != plan.ConfigRevision
                || handle.MapRevision != plan.MapRevision
                || handle.PreviousOwnerSessionEpoch
                    != plan.PreviousOwnerSessionEpoch
                || handle.OwnerSessionEpoch == 0
                || handle.OwnerSessionEpoch
                    == plan.PreviousOwnerSessionEpoch
                || handle.InitialState != LMCRecorderState.Configured
                || handle.BufferMode != LMCRecorderBufferMode.Double
                || handle.RecorderBufferCount != 2)
            {
                throw new InvalidOperationException(
                    "Empty Double-bank configuration adoption did not return the exact new-owner identity.");
            }
        }

        private static void ValidateRecoverableInventory(
            RecorderDoubleRecoveryRecord record,
            LMCRecorderBankInventory inventory)
        {
            if (inventory == null
                || record == null
                || record.ConfigRevision != 0
                || record.RecoveryTokenMarker
                    != RecorderDoubleRecoveryTokenMarker.ClientTokenV1
                || inventory.DiagnosticsBootId
                    != record.DiagnosticsBootId
                || inventory.ConfigId != record.RequestedConfigId
                || inventory.ConfigRevision == 0
                || inventory.MapRevision != record.MapRevision
                || inventory.RecoveryToken != record.RecoveryToken
                || !inventory.IsRecoverable
                || inventory.ConfigurationState
                    != LMCRecorderState.Configured
                || inventory.BufferMode
                    != LMCRecorderBufferMode.Double
                || inventory.RecorderBufferCount != 2
                || inventory.OccupiedBanks.Count != 0
                || inventory.ConfigurationOwnerSessionEpoch == 0
                || !inventory.IsConfigurationOwnerSessionClosed)
            {
                throw new InvalidOperationException(
                    "Recoverable inventory did not prove the exact token-qualified closed empty Double-bank configuration.");
            }
        }

        private static void ValidateBankAdoption(
            RecorderDoubleRecoveryPlan plan,
            RecorderDoubleRecoveryBankTarget target,
            LMCRecorderIdentity handle,
            uint priorAdoptedOwnerSessionEpoch)
        {
            if (handle == null
                || handle.DiagnosticsBootId != plan.DiagnosticsBootId
                || handle.ConfigId != plan.ConfigId
                || handle.ConfigRevision != plan.ConfigRevision
                || handle.MapRevision != plan.MapRevision
                || handle.RecordId != target.RecordId
                || handle.BufferId != target.BufferId
                || handle.OwnerSessionEpoch == 0
                || handle.OwnerSessionEpoch
                    == plan.PreviousOwnerSessionEpoch
                || (priorAdoptedOwnerSessionEpoch != 0
                    && handle.OwnerSessionEpoch
                        != priorAdoptedOwnerSessionEpoch)
                || handle.InitialState < LMCRecorderState.Armed
                || handle.InitialState > LMCRecorderState.Fault
                // Exact inventory and the derived plan prove Double mode. A
                // raw SDK Adopt handle intentionally has no configuration
                // shape; its ConfigId/ConfigRevision are hydrated by Status.
                || !handle.HasConfigurationMetadata
                || !handle.IsAdopted)
            {
                throw new InvalidOperationException(
                    "Double-bank adoption did not return the exact bank and new-owner identity.");
            }
        }
    }
}

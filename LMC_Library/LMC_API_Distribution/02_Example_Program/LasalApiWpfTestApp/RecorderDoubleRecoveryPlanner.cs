using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum RecorderDoubleRecoveryRoute
    {
        CurrentSessionOwnsConfiguration = 1,
        AdoptEmptyConfiguration = 2,
        AdoptOccupiedBanks = 3
    }

    internal sealed class RecorderDoubleRecoveryBankTarget
    {
        internal RecorderDoubleRecoveryBankTarget(
            uint bufferId,
            uint recordId,
            LMCRecorderState state)
        {
            BufferId = bufferId;
            RecordId = recordId;
            State = state;
        }

        internal uint BufferId { get; private set; }
        internal uint RecordId { get; private set; }
        internal LMCRecorderState State { get; private set; }
    }

    internal sealed class RecorderDoubleRecoveryPlan
    {
        private readonly ReadOnlyCollection<
            RecorderDoubleRecoveryBankTarget> banks;

        internal RecorderDoubleRecoveryPlan(
            RecorderDoubleRecoveryRoute route,
            Guid journalIdentity,
            uint diagnosticsBootId,
            uint configId,
            uint configRevision,
            uint mapRevision,
            uint previousOwnerSessionEpoch,
            IList<RecorderDoubleRecoveryBankTarget> banks)
        {
            Route = route;
            JournalIdentity = journalIdentity;
            DiagnosticsBootId = diagnosticsBootId;
            ConfigId = configId;
            ConfigRevision = configRevision;
            MapRevision = mapRevision;
            PreviousOwnerSessionEpoch = previousOwnerSessionEpoch;
            this.banks = new ReadOnlyCollection<
                RecorderDoubleRecoveryBankTarget>(banks);
        }

        internal RecorderDoubleRecoveryRoute Route { get; private set; }
        internal Guid JournalIdentity { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint ConfigId { get; private set; }
        internal uint ConfigRevision { get; private set; }
        internal uint MapRevision { get; private set; }
        internal uint PreviousOwnerSessionEpoch { get; private set; }
        internal IReadOnlyList<RecorderDoubleRecoveryBankTarget> Banks
        {
            get { return banks; }
        }
    }

    internal sealed class
        RecorderDoubleRecoveryReconfirmationRequiredException
        : InvalidOperationException
    {
        internal RecorderDoubleRecoveryReconfirmationRequiredException(
            string message)
            : base(message)
        {
        }
    }

    internal sealed class RecorderDoubleRecoveryMutationConfirmation
    {
        private readonly ReadOnlyCollection<
            RecorderDoubleRecoveryBankEvidence> banks;

        private RecorderDoubleRecoveryMutationConfirmation(
            RecorderDoubleRecoveryRecord record)
        {
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    "An active durable Double-bank record is required for mutation confirmation.");
            }

            JournalIdentity = record.Identity;
            DiagnosticsBootId = record.DiagnosticsBootId;
            ConfigId = record.RequestedConfigId;
            ConfigRevision = record.ConfigRevision;
            MapRevision = record.MapRevision;
            var copy = new List<RecorderDoubleRecoveryBankEvidence>(
                record.Banks.Count);
            for (var index = 0; index < record.Banks.Count; index++)
            {
                var bank = record.Banks[index];
                copy.Add(new RecorderDoubleRecoveryBankEvidence(
                    bank.BufferId,
                    bank.RecordId));
            }

            banks = new ReadOnlyCollection<
                RecorderDoubleRecoveryBankEvidence>(copy);
        }

        internal Guid JournalIdentity { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint ConfigId { get; private set; }
        internal uint ConfigRevision { get; private set; }
        internal uint MapRevision { get; private set; }
        internal IReadOnlyList<RecorderDoubleRecoveryBankEvidence> Banks
        {
            get { return banks; }
        }

        internal static RecorderDoubleRecoveryMutationConfirmation Capture(
            RecorderDoubleRecoveryRecord record)
        {
            return new RecorderDoubleRecoveryMutationConfirmation(record);
        }
    }

    internal static class RecorderDoubleRecoveryConfirmationPolicy
    {
        internal static void EnsurePlanConfirmedBeforeMutation(
            RecorderDoubleRecoveryMutationConfirmation confirmation,
            RecorderDoubleRecoveryPlan plan)
        {
            if (confirmation == null)
            {
                throw new ArgumentNullException("confirmation");
            }

            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            if (confirmation.JournalIdentity != plan.JournalIdentity
                || confirmation.DiagnosticsBootId != plan.DiagnosticsBootId
                || confirmation.ConfigId != plan.ConfigId
                || confirmation.ConfigRevision != plan.ConfigRevision
                || confirmation.MapRevision != plan.MapRevision)
            {
                throw CreateReconfirmationRequired();
            }

            for (var planIndex = 0;
                planIndex < plan.Banks.Count;
                planIndex++)
            {
                var target = plan.Banks[planIndex];
                var confirmed = false;
                for (var confirmedIndex = 0;
                    confirmedIndex < confirmation.Banks.Count;
                    confirmedIndex++)
                {
                    var bank = confirmation.Banks[confirmedIndex];
                    if (bank.BufferId == target.BufferId
                        && bank.RecordId == target.RecordId)
                    {
                        confirmed = true;
                        break;
                    }
                }

                if (!confirmed)
                {
                    throw CreateReconfirmationRequired();
                }
            }
        }

        private static
            RecorderDoubleRecoveryReconfirmationRequiredException
            CreateReconfirmationRequired()
        {
            return new
                RecorderDoubleRecoveryReconfirmationRequiredException(
                    "Read-only Recorder inventory changed the exact confirmed recovery plan. No Adopt or Release was sent. Review the updated durable journal targets and explicitly confirm again.");
        }
    }

    internal static class RecorderDoubleRecoveryPlanner
    {
        internal static RecorderDoubleRecoveryPlan Create(
            RecorderDoubleRecoveryRecord record,
            LMCRecorderBankInventory inventory)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (!record.IsActive)
            {
                throw new InvalidOperationException(
                    "Resolved Double-bank recovery evidence cannot create a recovery plan.");
            }

            ValidateConfigurationIdentity(record, inventory);
            List<RecorderDoubleRecoveryBankEvidence> ignored;
            var banks = ValidateAndCopyBanks(
                record,
                inventory,
                false,
                out ignored);
            var ownerSessionEpoch =
                inventory.ConfigurationOwnerSessionEpoch;
            var closedSessionEpoch =
                inventory.ConfigurationClosedSessionEpoch;
            if (ownerSessionEpoch == 0
                || (closedSessionEpoch != 0
                    && closedSessionEpoch != ownerSessionEpoch))
            {
                throw new InvalidOperationException(
                    "Recorder inventory contains an invalid configuration owner closure.");
            }

            RecorderDoubleRecoveryRoute route;
            if (closedSessionEpoch == 0)
            {
                route = RecorderDoubleRecoveryRoute
                    .CurrentSessionOwnsConfiguration;
            }
            else if (banks.Count == 0)
            {
                route = RecorderDoubleRecoveryRoute
                    .AdoptEmptyConfiguration;
            }
            else
            {
                route = RecorderDoubleRecoveryRoute.AdoptOccupiedBanks;
            }

            if (record.HasConfigurationReleaseOutcomeUncertain
                && route != RecorderDoubleRecoveryRoute
                    .AdoptEmptyConfiguration)
            {
                throw new InvalidOperationException(
                    "Pending configuration Release intent may retry only after exact presence of the same empty closed configuration is proven.");
            }

            if (record.HasBankReleaseOutcomeUncertain
                && route != RecorderDoubleRecoveryRoute
                    .AdoptOccupiedBanks)
            {
                throw new InvalidOperationException(
                    "Pending bank Release intent may retry only after exact presence and closed-owner adoption of the same bank is proven.");
            }

            return new RecorderDoubleRecoveryPlan(
                route,
                record.Identity,
                inventory.DiagnosticsBootId,
                inventory.ConfigId,
                inventory.ConfigRevision,
                inventory.MapRevision,
                ownerSessionEpoch,
                banks);
        }

        internal static IReadOnlyList<
            RecorderDoubleRecoveryBankEvidence>
            FindPendingBankReleaseAbsences(
                RecorderDoubleRecoveryRecord record,
                LMCRecorderBankInventory inventory)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (!record.IsActive
                || !record.HasBankReleaseOutcomeUncertain)
            {
                throw new InvalidOperationException(
                    "An active pending bank Release intent is required for absence reconciliation.");
            }

            ValidateConfigurationIdentity(record, inventory);
            List<RecorderDoubleRecoveryBankEvidence> absent;
            ValidateAndCopyBanks(
                record,
                inventory,
                true,
                out absent);
            return absent;
        }

        private static void ValidateConfigurationIdentity(
            RecorderDoubleRecoveryRecord record,
            LMCRecorderBankInventory inventory)
        {
            if (inventory.DiagnosticsBootId == 0
                || inventory.ConfigId == 0
                || inventory.ConfigRevision == 0
                || inventory.MapRevision == 0
                || record.ConfigRevision == 0
                || inventory.DiagnosticsBootId != record.DiagnosticsBootId
                || inventory.ConfigId != record.RequestedConfigId
                || inventory.MapRevision != record.MapRevision
                || inventory.ConfigRevision != record.ConfigRevision)
            {
                throw new InvalidOperationException(
                    "Recorder inventory does not match one fully identified durable BootId, ConfigId, ConfigRevision, and MapRevision identity. Configure-response loss remains fail-closed until the wire carries an exact recovery nonce.");
            }

            if (inventory.BufferMode != LMCRecorderBufferMode.Double
                || inventory.RecorderBufferCount != 2
                || inventory.ConfigurationState
                    < LMCRecorderState.Configured
                || inventory.ConfigurationState
                    > LMCRecorderState.Uploading
                || inventory.OccupiedBanks == null
                || inventory.OccupiedBanks.Count > 2
                || (inventory.OccupiedBanks.Count == 0
                    && inventory.ConfigurationState
                        != LMCRecorderState.Configured))
            {
                throw new InvalidOperationException(
                    "Recorder inventory is not one valid Double-bank configuration snapshot.");
            }
        }

        private static List<RecorderDoubleRecoveryBankTarget>
            ValidateAndCopyBanks(
                RecorderDoubleRecoveryRecord record,
                LMCRecorderBankInventory inventory,
                bool allowPendingReleaseAbsence,
                out List<RecorderDoubleRecoveryBankEvidence>
                    pendingReleaseAbsences)
        {
            pendingReleaseAbsences = new List<
                RecorderDoubleRecoveryBankEvidence>(2);
            var result = new List<RecorderDoubleRecoveryBankTarget>(
                inventory.OccupiedBanks.Count);
            var seenRecordIds = new HashSet<uint>();
            var seenBufferIds = new HashSet<uint>();
            for (var index = 0;
                index < inventory.OccupiedBanks.Count;
                index++)
            {
                var bank = inventory.OccupiedBanks[index];
                if (bank == null
                    || bank.RecordId == 0
                    || bank.BufferId > 1
                    || !seenRecordIds.Add(bank.RecordId)
                    || !seenBufferIds.Add(bank.BufferId)
                    || bank.OwnerSessionEpoch
                        != inventory.ConfigurationOwnerSessionEpoch
                    || bank.ClosedSessionEpoch
                        != inventory.ConfigurationClosedSessionEpoch
                    || bank.State < LMCRecorderState.Armed
                    || bank.State > LMCRecorderState.Uploading)
                {
                    throw new InvalidOperationException(
                        "Recorder inventory contains an invalid or ambiguous occupied bank.");
                }

                ValidateDurableBankIdentity(record, bank);
                if (allowPendingReleaseAbsence
                    && !HasExactDurableBank(record, bank))
                {
                    throw new InvalidOperationException(
                        "Bank Release absence reconciliation cannot bind a newly discovered bank after durable Release intent.");
                }

                if (record.IsBankReleaseConfirmed(
                        bank.BufferId,
                        bank.RecordId))
                {
                    throw new InvalidOperationException(
                        "Recorder inventory still contains a bank whose exact Release is durably confirmed.");
                }

                result.Add(new RecorderDoubleRecoveryBankTarget(
                    bank.BufferId,
                    bank.RecordId,
                    bank.State));
            }

            for (var index = 0; index < record.Banks.Count; index++)
            {
                var durableBank = record.Banks[index];
                var found = false;
                for (var inventoryIndex = 0;
                    inventoryIndex < inventory.OccupiedBanks.Count;
                    inventoryIndex++)
                {
                    var inventoryBank =
                        inventory.OccupiedBanks[inventoryIndex];
                    if (inventoryBank.BufferId == durableBank.BufferId
                        && inventoryBank.RecordId == durableBank.RecordId)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    if (record.IsBankReleaseConfirmed(
                            durableBank.BufferId,
                            durableBank.RecordId))
                    {
                        continue;
                    }

                    if (allowPendingReleaseAbsence
                        && record.IsBankReleasePending(
                            durableBank.BufferId,
                            durableBank.RecordId))
                    {
                        pendingReleaseAbsences.Add(
                            new RecorderDoubleRecoveryBankEvidence(
                                durableBank.BufferId,
                                durableBank.RecordId));
                        continue;
                    }

                    throw new InvalidOperationException(
                        "Recorder inventory is missing a durable bank without exact persisted Release evidence.");
                }
            }

            result.Sort((left, right) =>
                left.BufferId.CompareTo(right.BufferId));
            return result;
        }

        private static bool HasExactDurableBank(
            RecorderDoubleRecoveryRecord record,
            LMCRecorderBankInventoryEntry inventoryBank)
        {
            for (var index = 0; index < record.Banks.Count; index++)
            {
                var bank = record.Banks[index];
                if (bank.BufferId == inventoryBank.BufferId
                    && bank.RecordId == inventoryBank.RecordId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateDurableBankIdentity(
            RecorderDoubleRecoveryRecord record,
            LMCRecorderBankInventoryEntry inventoryBank)
        {
            for (var index = 0; index < record.Banks.Count; index++)
            {
                var durableBank = record.Banks[index];
                if ((durableBank.BufferId == inventoryBank.BufferId
                        && durableBank.RecordId
                            != inventoryBank.RecordId)
                    || (durableBank.RecordId == inventoryBank.RecordId
                        && durableBank.BufferId
                            != inventoryBank.BufferId))
                {
                    throw new InvalidOperationException(
                        "Recorder inventory conflicts with a durable bank identity.");
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RecorderDoubleRecoveryPlannerTests
    {
        private const uint BootId = 0x12345678u;
        private const uint ConfigId = 0x31415926u;
        private const uint ConfigRevision = 7;
        private const uint MapRevision = 0x957F101Eu;
        private const uint PreviousOwner = 41;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.RecorderDoubleRecovery.EmptyClosedPlansConfigAdopt",
                EmptyClosedPlansConfigAdopt);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.OccupiedClosedPlansExactBanks",
                OccupiedClosedPlansExactBanks);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.CurrentOwnerPlansNoAdoption",
                CurrentOwnerPlansNoAdoption);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.UnknownRevisionIsFailClosed",
                UnknownRevisionIsFailClosed);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.IdentityMismatchRejected",
                IdentityMismatchRejected);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.DurableBankConflictRejected",
                DurableBankConflictRejected);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.MissingDurableBankIsRejected",
                MissingDurableBankIsRejected);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.InvalidSnapshotRejected",
                InvalidSnapshotRejected);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.UnknownIdentityRequiresReconfirmation",
                UnknownIdentityRequiresReconfirmation);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.NewOccupiedBankRequiresReconfirmation",
                NewOccupiedBankRequiresReconfirmation);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.AdditionalBankRequiresReconfirmation",
                AdditionalBankRequiresReconfirmation);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.ExactTargetsRemainConfirmed",
                ExactTargetsRemainConfirmed);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.ConfirmedTargetSubsetRemainsAllowed",
                ConfirmedTargetSubsetRemainsAllowed);
        }

        private static void EmptyClosedPlansConfigAdopt()
        {
            var plan = RecorderDoubleRecoveryPlanner.Create(
                Record(ConfigRevision),
                Inventory(
                    PreviousOwner,
                    PreviousOwner,
                    LMCRecorderState.Configured));

            AssertEx.Equal(
                RecorderDoubleRecoveryRoute.AdoptEmptyConfiguration,
                plan.Route);
            AssertEx.Equal(ConfigId, plan.ConfigId);
            AssertEx.Equal(ConfigRevision, plan.ConfigRevision);
            AssertEx.Equal(PreviousOwner, plan.PreviousOwnerSessionEpoch);
            AssertEx.Equal(0, plan.Banks.Count);
        }

        private static void OccupiedClosedPlansExactBanks()
        {
            var plan = RecorderDoubleRecoveryPlanner.Create(
                Record(
                    ConfigRevision,
                    new RecorderDoubleRecoveryBankEvidence(1, 102)),
                Inventory(
                    PreviousOwner,
                    PreviousOwner,
                    LMCRecorderState.Ready,
                    new LMCRecorderBankInventoryEntry(
                        102,
                        1,
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Uploading),
                    new LMCRecorderBankInventoryEntry(
                        101,
                        0,
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready)));

            AssertEx.Equal(
                RecorderDoubleRecoveryRoute.AdoptOccupiedBanks,
                plan.Route);
            AssertEx.Equal(2, plan.Banks.Count);
            AssertEx.Equal((uint)0, plan.Banks[0].BufferId);
            AssertEx.Equal((uint)101, plan.Banks[0].RecordId);
            AssertEx.Equal((uint)1, plan.Banks[1].BufferId);
            AssertEx.Equal((uint)102, plan.Banks[1].RecordId);
        }

        private static void CurrentOwnerPlansNoAdoption()
        {
            var plan = RecorderDoubleRecoveryPlanner.Create(
                Record(ConfigRevision),
                Inventory(
                    PreviousOwner,
                    0,
                    LMCRecorderState.Configured));

            AssertEx.Equal(
                RecorderDoubleRecoveryRoute
                    .CurrentSessionOwnsConfiguration,
                plan.Route);
            AssertEx.Equal(PreviousOwner, plan.PreviousOwnerSessionEpoch);
        }

        private static void UnknownRevisionIsFailClosed()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    Record(0),
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured)));
        }

        private static void IdentityMismatchRejected()
        {
            var record = Record(ConfigRevision);
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured,
                        diagnosticsBootId: BootId + 1)));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured,
                        configId: ConfigId + 1)));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured,
                        configRevision: ConfigRevision + 1)));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured,
                        mapRevision: MapRevision + 1)));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    ResolvedRecord(),
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured)));
        }

        private static void DurableBankConflictRejected()
        {
            var record = Record(
                ConfigRevision,
                new RecorderDoubleRecoveryBankEvidence(0, 101));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        new LMCRecorderBankInventoryEntry(
                            999,
                            0,
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Ready))));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        new LMCRecorderBankInventoryEntry(
                            101,
                            1,
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Ready))));
        }

        private static void MissingDurableBankIsRejected()
        {
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    Record(
                        ConfigRevision,
                        new RecorderDoubleRecoveryBankEvidence(0, 101),
                        new RecorderDoubleRecoveryBankEvidence(1, 102)),
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        new LMCRecorderBankInventoryEntry(
                            102,
                            1,
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Ready))));
        }

        private static void InvalidSnapshotRejected()
        {
            var record = Record(ConfigRevision);
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready)));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner + 1,
                        LMCRecorderState.Configured)));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleRecoveryPlanner.Create(
                    record,
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        new LMCRecorderBankInventoryEntry(
                            101,
                            0,
                            PreviousOwner + 1,
                            PreviousOwner + 1,
                            LMCRecorderState.Ready))));
        }

        private static void UnknownIdentityRequiresReconfirmation()
        {
            var confirmation = RecorderDoubleRecoveryMutationConfirmation
                .Capture(Record(0));

            AssertEx.Throws<
                RecorderDoubleRecoveryReconfirmationRequiredException>(
                    () => RecorderDoubleRecoveryConfirmationPolicy
                        .EnsurePlanConfirmedBeforeMutation(
                            confirmation,
                            Plan()));
        }

        private static void NewOccupiedBankRequiresReconfirmation()
        {
            var confirmation = RecorderDoubleRecoveryMutationConfirmation
                .Capture(Record(ConfigRevision));

            AssertEx.Throws<
                RecorderDoubleRecoveryReconfirmationRequiredException>(
                    () => RecorderDoubleRecoveryConfirmationPolicy
                        .EnsurePlanConfirmedBeforeMutation(
                            confirmation,
                            Plan(
                                new RecorderDoubleRecoveryBankEvidence(
                                    0,
                                    101))));
        }

        private static void AdditionalBankRequiresReconfirmation()
        {
            var confirmation = RecorderDoubleRecoveryMutationConfirmation
                .Capture(Record(
                    ConfigRevision,
                    new RecorderDoubleRecoveryBankEvidence(0, 101)));

            AssertEx.Throws<
                RecorderDoubleRecoveryReconfirmationRequiredException>(
                    () => RecorderDoubleRecoveryConfirmationPolicy
                        .EnsurePlanConfirmedBeforeMutation(
                            confirmation,
                            Plan(
                                new RecorderDoubleRecoveryBankEvidence(
                                    0,
                                    101),
                                new RecorderDoubleRecoveryBankEvidence(
                                    1,
                                    102))));
        }

        private static void ExactTargetsRemainConfirmed()
        {
            var bank0 = new RecorderDoubleRecoveryBankEvidence(0, 101);
            var bank1 = new RecorderDoubleRecoveryBankEvidence(1, 102);
            var confirmation = RecorderDoubleRecoveryMutationConfirmation
                .Capture(Record(ConfigRevision, bank0, bank1));

            RecorderDoubleRecoveryConfirmationPolicy
                .EnsurePlanConfirmedBeforeMutation(
                    confirmation,
                    Plan(bank0, bank1));
            AssertEx.Equal(2, confirmation.Banks.Count);
        }

        private static void ConfirmedTargetSubsetRemainsAllowed()
        {
            var bank0 = new RecorderDoubleRecoveryBankEvidence(0, 101);
            var bank1 = new RecorderDoubleRecoveryBankEvidence(1, 102);
            var confirmation = RecorderDoubleRecoveryMutationConfirmation
                .Capture(Record(ConfigRevision, bank0, bank1));

            RecorderDoubleRecoveryConfirmationPolicy
                .EnsurePlanConfirmedBeforeMutation(
                    confirmation,
                    Plan(bank0));
            AssertEx.Equal(2, confirmation.Banks.Count);
        }

        private static RecorderDoubleRecoveryPlan Plan(
            params RecorderDoubleRecoveryBankEvidence[] banks)
        {
            var targets = new List<RecorderDoubleRecoveryBankTarget>(
                banks.Length);
            for (var index = 0; index < banks.Length; index++)
            {
                targets.Add(new RecorderDoubleRecoveryBankTarget(
                    banks[index].BufferId,
                    banks[index].RecordId,
                    LMCRecorderState.Ready));
            }

            return new RecorderDoubleRecoveryPlan(
                banks.Length == 0
                    ? RecorderDoubleRecoveryRoute.AdoptEmptyConfiguration
                    : RecorderDoubleRecoveryRoute.AdoptOccupiedBanks,
                Guid.Parse("11111111-2222-3333-4444-555555555555"),
                BootId,
                ConfigId,
                ConfigRevision,
                MapRevision,
                PreviousOwner,
                targets);
        }

        private static RecorderDoubleRecoveryRecord Record(
            uint configRevision,
            params RecorderDoubleRecoveryBankEvidence[] banks)
        {
            var now = new DateTime(
                2026,
                7,
                28,
                0,
                0,
                0,
                DateTimeKind.Utc);
            var state = configRevision == 0
                ? RecorderDoubleRecoveryState.ArmedBeforeConfigureDispatch
                : banks.Length == 0
                    ? RecorderDoubleRecoveryState.ConfigurationIdentified
                    : RecorderDoubleRecoveryState.CaptureEvidenceAvailable;
            return new RecorderDoubleRecoveryRecord(
                Guid.Parse("11111111-2222-3333-4444-555555555555"),
                state,
                now,
                now,
                BootId,
                MapRevision,
                ConfigId,
                configRevision,
                banks);
        }

        private static RecorderDoubleRecoveryRecord ResolvedRecord()
        {
            var now = new DateTime(
                2026,
                7,
                28,
                0,
                0,
                1,
                DateTimeKind.Utc);
            return new RecorderDoubleRecoveryRecord(
                Guid.Parse("11111111-2222-3333-4444-555555555555"),
                RecorderDoubleRecoveryState.Resolved,
                now,
                now,
                BootId,
                MapRevision,
                ConfigId,
                ConfigRevision,
                new RecorderDoubleRecoveryBankEvidence[0],
                0,
                0,
                true,
                true);
        }

        private static LMCRecorderBankInventory Inventory(
            uint ownerSessionEpoch,
            uint closedSessionEpoch,
            LMCRecorderState state,
            LMCRecorderBankInventoryEntry bank0 = null,
            LMCRecorderBankInventoryEntry bank1 = null,
            uint diagnosticsBootId = BootId,
            uint configId = ConfigId,
            uint configRevision = ConfigRevision,
            uint mapRevision = MapRevision)
        {
            var banks = new List<LMCRecorderBankInventoryEntry>();
            if (bank0 != null)
            {
                banks.Add(bank0);
            }

            if (bank1 != null)
            {
                banks.Add(bank1);
            }

            return new LMCRecorderBankInventory(
                null,
                diagnosticsBootId,
                configId,
                configRevision,
                mapRevision,
                ownerSessionEpoch,
                closedSessionEpoch,
                state,
                LMCRecorderBufferMode.Double,
                2,
                banks);
        }
    }
}

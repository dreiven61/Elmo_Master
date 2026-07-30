using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RecorderDoubleRecoveryOrchestratorTests
    {
        private const uint BootId = 0x12345678u;
        private const uint ConfigId = 0x31415926u;
        private const uint ConfigRevision = 7;
        private const uint MapRevision = 0x957F101Eu;
        private const uint PreviousOwner = 41;
        private const uint NewOwner = 52;
        private static readonly DateTime CreatedUtc = new DateTime(
            2026,
            7,
            28,
            0,
            0,
            0,
            DateTimeKind.Utc);

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.RecorderDoubleRecovery.EmptyConfigAdoptAfterDurableInventory",
                EmptyConfigAdoptAfterDurableInventory);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.BankAdoptIsExactAndUninterruptible",
                BankAdoptIsExactAndUninterruptible);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.RawAdoptModeUsesExactInventoryProof",
                RawAdoptModeUsesExactInventoryProof);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PreCancellationIsZeroIo",
                PreCancellationIsZeroIo);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PostInventoryCancellationIsZeroMutation",
                PostInventoryCancellationIsZeroMutation);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.InventoryExpansionRequiresReconfirmationBeforeAdopt",
                InventoryExpansionRequiresReconfirmationBeforeAdopt);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.CurrentOwnerIsZeroMutation",
                CurrentOwnerIsZeroMutation);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.UnknownRevisionIsZeroMutation",
                UnknownRevisionIsZeroMutation);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.TokenPresentUses4DThen4A",
                TokenPresentUses4DThen4A);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.TokenMismatchIsZero4AWire",
                TokenMismatchIsZero4AWire);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.TokenAbsenceResolvesWithoutRelease",
                TokenAbsenceResolvesWithoutRelease);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.TokenIdentityPersistsAcrossRestart",
                TokenIdentityPersistsAcrossRestart);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.AdoptionMismatchPreservesJournal",
                AdoptionMismatchPreservesJournal);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.InventoryFailurePreservesKnownJournal",
                InventoryFailurePreservesKnownJournal);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.EmptyReleaseResolvesExactJournal",
                EmptyReleaseResolvesExactJournal);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.OccupiedReleaseResolvesExactJournal",
                OccupiedReleaseResolvesExactJournal);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.OccupiedConfigConfirmedNotAppliedRetriesExactIntent",
                OccupiedConfigConfirmedNotAppliedRetriesExactIntent);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.RetainedBankAckConfirmsWithoutReplay",
                RetainedBankAckConfirmsWithoutReplay);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.RetainedConfigAckResolvesWithoutReplay",
                RetainedConfigAckResolvesWithoutReplay);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PendingBankAbsentReconciles",
                PendingBankAbsentReconciles);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PendingBankPresenceRetries",
                PendingBankPresenceRetries);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PendingConfigPresenceRetries",
                PendingConfigPresenceRetries);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PendingConfigReadFailureFailsClosed",
                PendingConfigReadFailureFailsClosed);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PendingConfigAbsenceReconciles",
                PendingConfigAbsenceReconciles);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.AbsenceWithoutReleaseIntentFailsClosed",
                AbsenceWithoutReleaseIntentFailsClosed);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.AbsenceIdentityMismatchFailsClosed",
                AbsenceIdentityMismatchFailsClosed);
            tests.Add(
                "Qualification.RecorderDoubleRecovery.PendingConfigOccupiedIsZeroMutation",
                PendingConfigOccupiedIsZeroMutation);
        }

        private static void EmptyConfigAdoptAfterDurableInventory()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured));
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal("ReadInventory,AdoptEmpty", state.CallsText);
                AssertEx.NotNull(result.AdoptedConfiguration);
                AssertEx.Equal(0, result.AdoptedBanks.Count);
                AssertEx.True(ReferenceEquals(
                    state.Inventory,
                    result.Inventory));
                AssertEx.Equal(NewOwner,
                    result.AdoptedConfiguration.OwnerSessionEpoch);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.ConfigurationIdentified,
                    journal.CurrentRecord.State);
                AssertEx.Equal(ConfigRevision,
                    journal.CurrentRecord.ConfigRevision);
                AssertEx.True(journal.HasActiveRecord);
            });
        }

        private static void BankAdoptIsExactAndUninterruptible()
        {
            WithArmedJournal((journal, identity) =>
            {
                using (var cancellation = new CancellationTokenSource())
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Ready,
                            Bank(101, 0),
                            Bank(102, 1)))
                    {
                        CancelAfterFirstAdopt = cancellation
                    };
                    var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                            journal,
                            state.Create(),
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult();

                    AssertEx.Equal(
                        "ReadInventory,Adopt0,Adopt1",
                        state.CallsText);
                    AssertEx.True(cancellation.IsCancellationRequested);
                    AssertEx.Equal(2, result.AdoptedBanks.Count);
                    AssertEx.True(ReferenceEquals(
                        state.Inventory,
                        result.Inventory));
                    AssertEx.True(result.AdoptedBanks[0].IsAdopted);
                    AssertEx.True(result.AdoptedBanks[1].IsAdopted);
                    AssertEx.Equal((uint)101,
                        result.AdoptedBanks[0].RecordId);
                    AssertEx.Equal((uint)102,
                        result.AdoptedBanks[1].RecordId);
                    AssertEx.Equal(2, journal.CurrentRecord.Banks.Count);
                    AssertEx.True(journal.HasActiveRecord);
                }
            });
        }

        private static void RawAdoptModeUsesExactInventoryProof()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        Bank(101, 0),
                        Bank(102, 1)))
                {
                    ReturnedBankMode = LMCRecorderBufferMode.Single
                };

                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(2, result.AdoptedBanks.Count);
                AssertEx.Equal(
                    LMCRecorderBufferMode.Single,
                    result.AdoptedBanks[0].BufferMode);
                AssertEx.Equal(ConfigId, result.AdoptedBanks[0].ConfigId);
                AssertEx.Equal(
                    ConfigRevision,
                    result.AdoptedBanks[0].ConfigRevision);
            });
        }

        private static void PreCancellationIsZeroIo()
        {
            WithArmedJournal((journal, identity) =>
            {
                using (var cancellation = new CancellationTokenSource())
                {
                    cancellation.Cancel();
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured));
                    AssertEx.Throws<OperationCanceledException>(
                        () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                                journal,
                                state.Create(),
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(string.Empty, state.CallsText);
                    AssertEx.Equal(
                        RecorderDoubleRecoveryState
                            .ArmedBeforeConfigureDispatch,
                        journal.CurrentRecord.State);
                }
            }, false);
        }

        private static void PostInventoryCancellationIsZeroMutation()
        {
            WithArmedJournal((journal, identity) =>
            {
                using (var cancellation = new CancellationTokenSource())
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured))
                    {
                        CancelAfterInventory = cancellation
                    };
                    AssertEx.Throws<OperationCanceledException>(
                        () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                                journal,
                                state.Create(),
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal("ReadInventory", state.CallsText);
                    AssertEx.Equal(
                        RecorderDoubleRecoveryState
                            .ConfigurationIdentified,
                        journal.CurrentRecord.State);
                }
            });
        }

        private static void CurrentOwnerIsZeroMutation()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        0,
                        LMCRecorderState.Configured));
                AssertEx.Throws<InvalidOperationException>(
                    () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                            journal,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal("ReadInventory", state.CallsText);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.ConfigurationIdentified,
                    journal.CurrentRecord.State);
                AssertEx.True(journal.HasActiveRecord);
            });
        }

        private static void AdoptionMismatchPreservesJournal()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        Bank(101, 0)))
                {
                    ReturnedBankRecordId = 999
                };
                AssertEx.Throws<InvalidOperationException>(
                    () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                            journal,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal("ReadInventory,Adopt0", state.CallsText);
                AssertEx.Equal(1, journal.CurrentRecord.Banks.Count);
                AssertEx.True(journal.HasActiveRecord);
            });
        }

        private static void UnknownRevisionIsZeroMutation()
        {
            WithTestDirectory(directory =>
            {
                using (var journal =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    journal.ArmBeforeConfigureDispatch(
                        Guid.NewGuid(),
                        CreatedUtc,
                        BootId,
                        MapRevision,
                        ConfigId);
                }

                ConvertJournalToV2(directory);
                using (var journal =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured));
                    AssertEx.Throws<InvalidOperationException>(
                        () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                                journal,
                                state.Create(),
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(string.Empty, state.CallsText);
                    AssertEx.Equal(
                        RecorderDoubleRecoveryTokenMarker.LegacyUnbound,
                        journal.CurrentRecord.RecoveryTokenMarker);
                    AssertEx.Equal(
                        RecorderDoubleRecoveryState
                            .ArmedBeforeConfigureDispatch,
                        journal.CurrentRecord.State);
                    AssertEx.True(journal.HasActiveRecord);
                }
            });
        }

        private static void TokenPresentUses4DThen4A()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured))
                {
                    RecoverableInventory = TokenInventory(identity)
                };
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    "ReadRecoverableInventory,ReadInventory,AdoptEmpty",
                    state.CallsText);
                AssertEx.NotNull(result.AdoptedConfiguration);
                AssertEx.Equal(ConfigRevision,
                    journal.CurrentRecord.ConfigRevision);
                AssertEx.Equal(
                    RecorderDoubleRecoveryTokenMarker.ClientTokenV1,
                    journal.CurrentRecord.RecoveryTokenMarker);
            }, false);
        }

        private static void TokenMismatchIsZero4AWire()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured))
                {
                    RecoverableInventory = TokenInventory(Guid.NewGuid())
                };
                AssertEx.Throws<InvalidOperationException>(
                    () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                            journal,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal(
                    "ReadRecoverableInventory",
                    state.CallsText);
                AssertEx.Equal((uint)0,
                    journal.CurrentRecord.ConfigRevision);
                AssertEx.True(journal.CurrentRecord.IsActive);
            }, false);
        }

        private static void TokenAbsenceResolvesWithoutRelease()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(null)
                {
                    RecoverableInventoryError =
                        RecoverableConfigurationAbsent(identity)
                };
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    "ReadRecoverableInventory",
                    state.CallsText);
                AssertEx.True(result.IsResolvedByConfigurationAbsence);
                AssertEx.NotNull(
                    result.RecoverableConfigurationAbsence);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState
                        .ResolvedWithoutConfiguration,
                    result.ResolvedRecord.State);
                AssertEx.False(
                    result.ResolvedRecord.ConfigurationReleaseIntent);
                AssertEx.False(
                    result.ResolvedRecord.ConfigurationReleaseConfirmed);
                AssertEx.False(journal.HasActiveRecord);
            }, false);
        }

        private static void TokenIdentityPersistsAcrossRestart()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    journal.ArmBeforeConfigureDispatch(
                        identity,
                        CreatedUtc,
                        BootId,
                        MapRevision,
                        ConfigId);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var first = new FakeOperations(null)
                    {
                        RecoverableInventory = TokenInventory(identity),
                        InventoryError = new IOException(
                            "Crash boundary after durable token resolution.")
                    };
                    AssertEx.Throws<IOException>(
                        () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                                reopened,
                                first.Create(),
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal(
                        "ReadRecoverableInventory,ReadInventory",
                        first.CallsText);
                    AssertEx.Equal(ConfigRevision,
                        reopened.CurrentRecord.ConfigRevision);
                }

                using (var verified =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var second = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured));
                    RecorderDoubleRecoveryOrchestrator.RunAsync(
                            verified,
                            second.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.Equal(
                        "ReadInventory,AdoptEmpty",
                        second.CallsText);
                }
            });
        }

        private static void InventoryFailurePreservesKnownJournal()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(null)
                {
                    InventoryError = new InvalidOperationException(
                        "inventory unavailable")
                };
                AssertEx.Throws<InvalidOperationException>(
                    () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                            journal,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Equal("ReadInventory", state.CallsText);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState
                        .ConfigurationIdentified,
                    journal.CurrentRecord.State);
                AssertEx.True(journal.HasActiveRecord);
            });
        }

        private static LMCRecorderBankInventoryEntry Bank(
            uint recordId,
            uint bufferId)
        {
            return new LMCRecorderBankInventoryEntry(
                recordId,
                bufferId,
                PreviousOwner,
                PreviousOwner,
                LMCRecorderState.Ready);
        }

        private static void EmptyReleaseResolvesExactJournal()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured));
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var bridge = new RecorderDoubleQualificationJournalBridge(
                    journal,
                    identity,
                    () => CreatedUtc.AddSeconds(3));
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => CreatedUtc.AddSeconds(3));

                AssertEx.Throws<InvalidOperationException>(
                    () => bridge.ResolveRecoveredAfterExactRelease(
                        result,
                        true));
                AssertEx.Throws<InvalidOperationException>(
                    () => coordinator
                        .ReleaseRecoveredEmptyConfigurationAndResolveAsync(
                            result,
                            handle =>
                            {
                                handle.BeginRelease();
                                handle.CompleteRelease();
                                return Task.CompletedTask;
                            },
                            false,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                var resolved = coordinator
                    .ReleaseRecoveredEmptyConfigurationAndResolveAsync(
                        result,
                        handle =>
                        {
                            handle.BeginRelease();
                            handle.CompleteRelease();
                            return Task.CompletedTask;
                        },
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.Resolved,
                    resolved.State);
                AssertEx.False(journal.HasActiveRecord);
            });
        }

        private static void OccupiedReleaseResolvesExactJournal()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        Bank(101, 0),
                        Bank(102, 1)));
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var bridge = new RecorderDoubleQualificationJournalBridge(
                    journal,
                    identity,
                    () => CreatedUtc.AddSeconds(3));
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => CreatedUtc.AddSeconds(3));

                for (var index = 0;
                    index < result.AdoptedBanks.Count;
                    index++)
                {
                    coordinator.ReleaseRecoveredBankAsync(
                            result,
                            result.AdoptedBanks[index],
                            handle =>
                            {
                                handle.BeginBufferRelease();
                                handle.CompleteBufferRelease();
                                return Task.CompletedTask;
                            },
                            true,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }

                AssertEx.Throws<InvalidOperationException>(
                    () => bridge.ResolveRecoveredAfterExactRelease(
                        result,
                        true));
                var resolved = coordinator
                    .ReleaseRecoveredOccupiedConfigurationAndResolveAsync(
                        result,
                        result.AdoptedBanks[0],
                        handle =>
                        {
                            handle.BeginRecorderRelease();
                            handle.CompleteRecorderRelease();
                            return Task.CompletedTask;
                        },
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.Resolved,
                    resolved.State);
                AssertEx.False(journal.HasActiveRecord);
            });
        }

        private static void
            OccupiedConfigConfirmedNotAppliedRetriesExactIntent()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        Bank(101, 0),
                        Bank(102, 1)));
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => CreatedUtc.AddSeconds(3));

                for (var index = 0;
                    index < result.AdoptedBanks.Count;
                    index++)
                {
                    coordinator.ReleaseRecoveredBankAsync(
                            result,
                            result.AdoptedBanks[index],
                            handle =>
                            {
                                handle.BeginBufferRelease();
                                handle.CompleteBufferRelease();
                                return Task.CompletedTask;
                            },
                            true,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }

                var configurationHandle = result.AdoptedBanks[0];
                var dispatchCount = 0;
                AssertEx.Throws<InvalidOperationException>(
                    () => coordinator
                        .ReleaseRecoveredOccupiedConfigurationAndResolveAsync(
                            result,
                            configurationHandle,
                            handle =>
                            {
                                dispatchCount++;
                                handle.BeginRecorderRelease();
                                handle.CancelRecorderRelease();
                                return Task.FromException(
                                    new InvalidOperationException(
                                        "Release was confirmed not applied."));
                            },
                            true,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, dispatchCount);
                AssertEx.True(
                    journal.CurrentRecord
                        .HasConfigurationReleaseOutcomeUncertain);
                AssertEx.False(
                    configurationHandle
                        .IsRecorderReleaseOutcomeUnverified);

                var resolved = coordinator
                    .ReleaseRecoveredOccupiedConfigurationAndResolveAsync(
                        result,
                        configurationHandle,
                        handle =>
                        {
                            dispatchCount++;
                            handle.BeginRecorderRelease();
                            handle.CompleteRecorderRelease();
                            return Task.CompletedTask;
                        },
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(2, dispatchCount);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.Resolved,
                    resolved.State);
                AssertEx.True(resolved.ConfigurationReleaseConfirmed);
                AssertEx.False(journal.HasActiveRecord);
            });
        }

        private static void
            InventoryExpansionRequiresReconfirmationBeforeAdopt()
        {
            WithArmedJournal((journal, identity) =>
            {
                var inventory = Inventory(
                    PreviousOwner,
                    PreviousOwner,
                    LMCRecorderState.Ready,
                    Bank(101, 0),
                    Bank(102, 1));
                var confirmation = RecorderDoubleRecoveryMutationConfirmation
                    .Capture(journal.CurrentRecord);
                var firstState = new FakeOperations(inventory);
                var firstOperations = firstState.Create();
                firstOperations.EnsureMutationPlanConfirmed = plan =>
                    RecorderDoubleRecoveryConfirmationPolicy
                        .EnsurePlanConfirmedBeforeMutation(
                            confirmation,
                            plan);

                AssertEx.Throws<
                    RecorderDoubleRecoveryReconfirmationRequiredException>(
                        () => RecorderDoubleRecoveryOrchestrator.RunAsync(
                                journal,
                                firstOperations,
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                AssertEx.Equal("ReadInventory", firstState.CallsText);
                AssertEx.Equal(2, journal.CurrentRecord.Banks.Count);
                AssertEx.True(journal.CurrentRecord.IsActive);

                var reconfirmed = RecorderDoubleRecoveryMutationConfirmation
                    .Capture(journal.CurrentRecord);
                var secondState = new FakeOperations(inventory);
                var secondOperations = secondState.Create();
                secondOperations.EnsureMutationPlanConfirmed = plan =>
                    RecorderDoubleRecoveryConfirmationPolicy
                        .EnsurePlanConfirmedBeforeMutation(
                            reconfirmed,
                            plan);
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        secondOperations,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(
                    "ReadInventory,Adopt0,Adopt1",
                    secondState.CallsText);
                AssertEx.Equal(2, result.AdoptedBanks.Count);
            });
        }

        private static void RetainedBankAckConfirmsWithoutReplay()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        Bank(101, 0),
                        Bank(102, 1)));
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var handle = result.AdoptedBanks[0];
                journal.BeginBankRelease(
                    identity,
                    CreatedUtc.AddSeconds(3),
                    BootId,
                    ConfigId,
                    ConfigRevision,
                    MapRevision,
                    handle.BufferId,
                    handle.RecordId);
                handle.BeginBufferRelease();
                handle.CompleteBufferRelease();
                var replayCount = 0;
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => CreatedUtc.AddSeconds(4));

                coordinator.ReleaseRecoveredBankAsync(
                        result,
                        handle,
                        ignored =>
                        {
                            replayCount++;
                            return Task.CompletedTask;
                        },
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(0, replayCount);
                AssertEx.True(journal.CurrentRecord.IsBankReleaseConfirmed(
                    handle.BufferId,
                    handle.RecordId));
            });
        }

        private static void RetainedConfigAckResolvesWithoutReplay()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Ready,
                        Bank(101, 0),
                        Bank(102, 1)));
                var result = RecorderDoubleRecoveryOrchestrator.RunAsync(
                        journal,
                        state.Create(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => CreatedUtc.AddSeconds(3));
                for (var index = 0;
                    index < result.AdoptedBanks.Count;
                    index++)
                {
                    coordinator.ReleaseRecoveredBankAsync(
                            result,
                            result.AdoptedBanks[index],
                            handle =>
                            {
                                handle.BeginBufferRelease();
                                handle.CompleteBufferRelease();
                                return Task.CompletedTask;
                            },
                            true,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                }

                journal.BeginConfigurationRelease(
                    identity,
                    CreatedUtc.AddSeconds(4),
                    BootId,
                    ConfigId,
                    ConfigRevision,
                    MapRevision);
                var configurationHandle = result.AdoptedBanks[0];
                configurationHandle.BeginRecorderRelease();
                configurationHandle.CompleteRecorderRelease();
                var replayCount = 0;
                coordinator = new RecorderDoubleDurableReleaseCoordinator(
                    journal,
                    identity,
                    () => CreatedUtc.AddSeconds(5));

                var resolved = coordinator
                    .ReleaseRecoveredOccupiedConfigurationAndResolveAsync(
                        result,
                        configurationHandle,
                        ignored =>
                        {
                            replayCount++;
                            return Task.CompletedTask;
                        },
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(0, replayCount);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.Resolved,
                    resolved.State);
                AssertEx.False(journal.HasActiveRecord);
            });
        }

        private static void PendingBankAbsentReconciles()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal = CreateKnownJournal(
                    directory,
                    identity,
                    new RecorderDoubleRecoveryBankEvidence(0, 101),
                    new RecorderDoubleRecoveryBankEvidence(1, 102)))
                {
                    journal.BeginBankRelease(
                        identity,
                        CreatedUtc.AddSeconds(3),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision,
                        0,
                        101);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    AssertEx.True(
                        reopened.CurrentRecord
                            .HasBankReleaseOutcomeUncertain);
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Ready,
                            Bank(102, 1)))
                    {
                        Now = CreatedUtc.AddSeconds(4)
                    };
                    var result = RecorderDoubleRecoveryOrchestrator
                        .RunAsync(
                            reopened,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();

                    AssertEx.Equal(
                        "ReadInventory,Adopt1",
                        state.CallsText);
                    AssertEx.True(
                        reopened.CurrentRecord.IsBankReleaseConfirmed(
                            0,
                            101));
                    AssertEx.False(
                        reopened.CurrentRecord
                            .HasBankReleaseOutcomeUncertain);
                    AssertEx.Equal(1, result.AdoptedBanks.Count);
                    AssertEx.Equal((uint)102,
                        result.AdoptedBanks[0].RecordId);
                }
            });
        }

        private static void PendingBankPresenceRetries()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal = CreateKnownJournal(
                    directory,
                    identity,
                    new RecorderDoubleRecoveryBankEvidence(0, 101),
                    new RecorderDoubleRecoveryBankEvidence(1, 102)))
                {
                    journal.BeginBankRelease(
                        identity,
                        CreatedUtc.AddSeconds(3),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision,
                        0,
                        101);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Ready,
                            Bank(101, 0),
                            Bank(102, 1)))
                    {
                        Now = CreatedUtc.AddSeconds(4)
                    };
                    var result = RecorderDoubleRecoveryOrchestrator
                        .RunAsync(
                            reopened,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.Equal(
                        "ReadInventory,Adopt0,Adopt1",
                        state.CallsText);
                    AssertEx.True(
                        reopened.CurrentRecord
                            .HasBankReleaseOutcomeUncertain);

                    var coordinator = new
                        RecorderDoubleDurableReleaseCoordinator(
                            reopened,
                            identity,
                            () => CreatedUtc.AddSeconds(5));
                    coordinator.ReleaseRecoveredBankAsync(
                            result,
                            result.AdoptedBanks[0],
                            handle =>
                            {
                                handle.BeginBufferRelease();
                                handle.CompleteBufferRelease();
                                return Task.CompletedTask;
                            },
                            true,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.True(
                        reopened.CurrentRecord.IsBankReleaseConfirmed(
                            0,
                            101));
                    AssertEx.False(
                        reopened.CurrentRecord
                            .HasBankReleaseOutcomeUncertain);
                }
            });
        }

        private static void PendingConfigPresenceRetries()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal = CreateKnownJournal(
                    directory,
                    identity))
                {
                    journal.BeginConfigurationRelease(
                        identity,
                        CreatedUtc.AddSeconds(3),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured))
                    {
                        Now = CreatedUtc.AddSeconds(4)
                    };
                    var result = RecorderDoubleRecoveryOrchestrator
                        .RunAsync(
                            reopened,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.Equal(
                        "ReadInventory,AdoptEmpty",
                        state.CallsText);

                    var coordinator = new
                        RecorderDoubleDurableReleaseCoordinator(
                            reopened,
                            identity,
                            () => CreatedUtc.AddSeconds(5));
                    var resolved = coordinator
                        .ReleaseRecoveredEmptyConfigurationAndResolveAsync(
                            result,
                            handle =>
                            {
                                handle.BeginRelease();
                                handle.CompleteRelease();
                                return Task.CompletedTask;
                            },
                            true,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();
                    AssertEx.Equal(
                        RecorderDoubleRecoveryState.Resolved,
                        resolved.State);
                    AssertEx.True(resolved.ConfigurationReleaseConfirmed);
                }
            });
        }

        private static void PendingConfigReadFailureFailsClosed()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal = CreateKnownJournal(
                    directory,
                    identity))
                {
                    journal.BeginConfigurationRelease(
                        identity,
                        CreatedUtc.AddSeconds(3),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured))
                    {
                        InventoryError = new IOException(
                            "Typed configuration absence is unavailable."),
                        Now = CreatedUtc.AddSeconds(4)
                    };
                    AssertEx.Throws<IOException>(
                        () => RecorderDoubleRecoveryOrchestrator
                            .RunAsync(
                                reopened,
                                state.Create(),
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal("ReadInventory", state.CallsText);
                    AssertEx.True(
                        reopened.CurrentRecord
                            .HasConfigurationReleaseOutcomeUncertain);
                }
            });
        }

        private static void PendingConfigOccupiedIsZeroMutation()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal = CreateKnownJournal(
                    directory,
                    identity,
                    new RecorderDoubleRecoveryBankEvidence(0, 101)))
                {
                    journal.BeginBankRelease(
                        identity,
                        CreatedUtc.AddSeconds(3),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision,
                        0,
                        101);
                    journal.ConfirmBankRelease(
                        identity,
                        CreatedUtc.AddSeconds(4),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision,
                        0,
                        101);
                    journal.BeginConfigurationRelease(
                        identity,
                        CreatedUtc.AddSeconds(5),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Ready,
                            Bank(101, 0)))
                    {
                        Now = CreatedUtc.AddSeconds(6)
                    };
                    AssertEx.Throws<InvalidOperationException>(
                        () => RecorderDoubleRecoveryOrchestrator
                            .RunAsync(
                                reopened,
                                state.Create(),
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal("ReadInventory", state.CallsText);
                    AssertEx.True(
                        reopened.CurrentRecord
                            .HasConfigurationReleaseOutcomeUncertain);
                }
            });
        }

        private static void PendingConfigAbsenceReconciles()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal = CreateKnownJournal(
                    directory,
                    identity))
                {
                    journal.BeginConfigurationRelease(
                        identity,
                        CreatedUtc.AddSeconds(3),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured))
                    {
                        InventoryError = ConfigurationAbsent(),
                        Now = CreatedUtc.AddSeconds(4)
                    };
                    var result = RecorderDoubleRecoveryOrchestrator
                        .RunAsync(
                            reopened,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();

                    AssertEx.Equal("ReadInventory", state.CallsText);
                    AssertEx.True(
                        result.IsResolvedByConfigurationAbsence);
                    AssertEx.True(result.Plan == null);
                    AssertEx.True(result.Inventory == null);
                    AssertEx.Equal(
                        RecorderDoubleRecoveryState.Resolved,
                        result.ResolvedRecord.State);
                    AssertEx.True(
                        result.ResolvedRecord
                            .ConfigurationReleaseConfirmed);
                    AssertEx.False(
                        reopened.CurrentRecord.IsActive);
                }

                using (var verified =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    AssertEx.False(verified.CurrentRecord.IsActive);
                    AssertEx.True(
                        verified.CurrentRecord
                            .ConfigurationReleaseConfirmed);
                }
            });
        }

        private static void AbsenceWithoutReleaseIntentFailsClosed()
        {
            WithArmedJournal((journal, identity) =>
            {
                var state = new FakeOperations(
                    Inventory(
                        PreviousOwner,
                        PreviousOwner,
                        LMCRecorderState.Configured))
                {
                    InventoryError = ConfigurationAbsent()
                };
                AssertEx.Throws<InvalidOperationException>(
                    () => RecorderDoubleRecoveryOrchestrator
                        .RunAsync(
                            journal,
                            state.Create(),
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal("ReadInventory", state.CallsText);
                AssertEx.True(journal.CurrentRecord.IsActive);
                AssertEx.False(
                    journal.CurrentRecord
                        .ConfigurationReleaseConfirmed);
            });
        }

        private static void AbsenceIdentityMismatchFailsClosed()
        {
            WithTestDirectory(directory =>
            {
                var identity = Guid.NewGuid();
                using (var journal = CreateKnownJournal(
                    directory,
                    identity))
                {
                    journal.BeginConfigurationRelease(
                        identity,
                        CreatedUtc.AddSeconds(3),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision);
                }

                using (var reopened =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var state = new FakeOperations(
                        Inventory(
                            PreviousOwner,
                            PreviousOwner,
                            LMCRecorderState.Configured))
                    {
                        InventoryError = ConfigurationAbsent(
                            ConfigId + 1),
                        Now = CreatedUtc.AddSeconds(4)
                    };
                    AssertEx.Throws<InvalidOperationException>(
                        () => RecorderDoubleRecoveryOrchestrator
                            .RunAsync(
                                reopened,
                                state.Create(),
                                CancellationToken.None)
                            .GetAwaiter()
                            .GetResult());
                    AssertEx.Equal("ReadInventory", state.CallsText);
                    AssertEx.True(
                        reopened.CurrentRecord
                            .HasConfigurationReleaseOutcomeUncertain);
                }
            });
        }

        private static LMCRecorderConfigurationAbsentException
            ConfigurationAbsent(uint configId = ConfigId)
        {
            var payload = new byte[16];
            TestFrame.WriteUInt16(
                payload,
                0,
                LMC_DiagnosticsFrame.SchemaVersion);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteUInt16(
                payload,
                6,
                unchecked((ushort)(short)-32000));
            TestFrame.WriteUInt32(payload, 8, 1);
            TestFrame.WriteUInt32(
                payload,
                12,
                (uint)LMCDiagnosticsDetailCode
                    .RecorderConfigurationAbsent);

            return AssertEx.Throws<
                LMCRecorderConfigurationAbsentException>(
                    () => LMC_DiagnosticsParser
                        .ParseRecorderBankInventory(
                            TestFrame.Response(0, payload),
                            1,
                            BootId,
                            configId,
                            MapRevision,
                            ConfigRevision));
        }

        private static LMCRecorderBankInventory Inventory(
            uint ownerSessionEpoch,
            uint closedSessionEpoch,
            LMCRecorderState state,
            params LMCRecorderBankInventoryEntry[] banks)
        {
            return new LMCRecorderBankInventory(
                null,
                BootId,
                ConfigId,
                ConfigRevision,
                MapRevision,
                ownerSessionEpoch,
                closedSessionEpoch,
                state,
                LMCRecorderBufferMode.Double,
                2,
                new List<LMCRecorderBankInventoryEntry>(banks));
        }

        private static LMCRecorderBankInventory TokenInventory(Guid token)
        {
            return new LMCRecorderBankInventory(
                null,
                BootId,
                ConfigId,
                ConfigRevision,
                MapRevision,
                PreviousOwner,
                PreviousOwner,
                LMCRecorderState.Configured,
                LMCRecorderBufferMode.Double,
                2,
                new List<LMCRecorderBankInventoryEntry>(),
                token);
        }

        private static LMCRecoverableRecorderConfigurationAbsentException
            RecoverableConfigurationAbsent(Guid recoveryToken)
        {
            var payload = new byte[16];
            TestFrame.WriteUInt16(
                payload,
                0,
                LMC_DiagnosticsFrame.SchemaVersion);
            TestFrame.WriteUInt16(payload, 4, 1);
            TestFrame.WriteUInt16(
                payload,
                6,
                unchecked((ushort)(short)-32000));
            TestFrame.WriteUInt32(payload, 8, 1);
            TestFrame.WriteUInt32(
                payload,
                12,
                (uint)LMCDiagnosticsDetailCode
                    .RecorderConfigurationAbsent);

            return AssertEx.Throws<
                LMCRecoverableRecorderConfigurationAbsentException>(
                    () => LMC_DiagnosticsParser
                        .ParseRecoverableRecorderBankInventory(
                            TestFrame.Response(0, payload),
                            1,
                            BootId,
                            ConfigId,
                            MapRevision,
                            recoveryToken));
        }

        private static void ConvertJournalToV2(string directory)
        {
            var path = Path.Combine(
                directory,
                RecorderDoubleRecoveryJournal.JournalFileName);
            var v3 = File.ReadAllBytes(path);
            const int envelopeLength = 16;
            const int checksumLength = 32;
            const int markerLength = 4;
            var payloadLength = v3.Length
                - envelopeLength
                - checksumLength;
            var v2 = new byte[v3.Length - markerLength];
            Buffer.BlockCopy(v3, 0, v2, 0, envelopeLength);
            WriteInt32LittleEndian(v2, 8, 2);
            WriteInt32LittleEndian(
                v2,
                12,
                payloadLength - markerLength);
            Buffer.BlockCopy(
                v3,
                envelopeLength,
                v2,
                envelopeLength,
                payloadLength - markerLength);
            var checksumOffset = v2.Length - checksumLength;
            using (var sha256 = SHA256.Create())
            {
                var checksum = sha256.ComputeHash(
                    v2,
                    0,
                    checksumOffset);
                Buffer.BlockCopy(
                    checksum,
                    0,
                    v2,
                    checksumOffset,
                    checksum.Length);
            }

            File.WriteAllBytes(path, v2);
        }

        private static void WriteInt32LittleEndian(
            byte[] target,
            int offset,
            int value)
        {
            target[offset] = (byte)value;
            target[offset + 1] = (byte)(value >> 8);
            target[offset + 2] = (byte)(value >> 16);
            target[offset + 3] = (byte)(value >> 24);
        }

        private static RecorderDoubleRecoveryJournal CreateKnownJournal(
            string directory,
            Guid identity,
            params RecorderDoubleRecoveryBankEvidence[] banks)
        {
            var journal = RecorderDoubleRecoveryJournal.Open(directory);
            try
            {
                journal.ArmBeforeConfigureDispatch(
                    identity,
                    CreatedUtc,
                    BootId,
                    MapRevision,
                    ConfigId);
                journal.RecordConfigurationReply(
                    identity,
                    CreatedUtc.AddSeconds(1),
                    BootId,
                    ConfigId,
                    ConfigRevision,
                    MapRevision);
                if (banks.Length != 0)
                {
                    journal.RecordInventory(
                        identity,
                        CreatedUtc.AddSeconds(2),
                        BootId,
                        ConfigId,
                        ConfigRevision,
                        MapRevision,
                        banks);
                }

                return journal;
            }
            catch
            {
                journal.Dispose();
                throw;
            }
        }

        private static void WithTestDirectory(Action<string> body)
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "ElmoRecorderDoubleRecoveryCrashTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                body(directory);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        private static void WithArmedJournal(
            Action<RecorderDoubleRecoveryJournal, Guid> body,
            bool configurationKnown = true)
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "ElmoRecorderDoubleRecoveryOrchestratorTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                using (var journal =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var identity = Guid.NewGuid();
                    journal.ArmBeforeConfigureDispatch(
                        identity,
                        CreatedUtc,
                        BootId,
                        MapRevision,
                        ConfigId);
                    if (configurationKnown)
                    {
                        journal.RecordConfigurationReply(
                            identity,
                            CreatedUtc.AddSeconds(1),
                            BootId,
                            ConfigId,
                            ConfigRevision,
                            MapRevision);
                    }

                    body(journal, identity);
                }
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        private sealed class FakeOperations
        {
            private readonly List<string> calls = new List<string>();
            private readonly LMCRecorderBankInventory inventory;
            internal LMCRecorderBankInventory RecoverableInventory;
            internal CancellationTokenSource CancelAfterInventory;
            internal CancellationTokenSource CancelAfterFirstAdopt;
            internal Exception InventoryError;
            internal Exception RecoverableInventoryError;
            internal uint ReturnedBankRecordId;
            internal LMCRecorderBufferMode ReturnedBankMode =
                LMCRecorderBufferMode.Double;
            internal DateTime Now = CreatedUtc.AddSeconds(2);

            internal FakeOperations(LMCRecorderBankInventory inventory)
            {
                this.inventory = inventory;
            }

            internal string CallsText
            {
                get { return string.Join(",", calls); }
            }

            internal LMCRecorderBankInventory Inventory
            {
                get { return inventory; }
            }

            internal RecorderDoubleRecoveryOperations Create()
            {
                return new RecorderDoubleRecoveryOperations
                {
                    UtcNow = () => Now,
                    ReadRecoverableInventoryAsync =
                        (record, cancellationToken) =>
                    {
                        calls.Add("ReadRecoverableInventory");
                        return RecoverableInventoryError == null
                            ? Task.FromResult(RecoverableInventory)
                            : Task.FromException<LMCRecorderBankInventory>(
                                RecoverableInventoryError);
                    },
                    ReadInventoryAsync = (record, cancellationToken) =>
                    {
                        calls.Add("ReadInventory");
                        if (CancelAfterInventory != null)
                        {
                            CancelAfterInventory.Cancel();
                        }

                        return InventoryError == null
                            ? Task.FromResult(inventory)
                            : Task.FromException<LMCRecorderBankInventory>(
                                InventoryError);
                    },
                    AdoptEmptyConfigurationAsync = (plan, exactInventory) =>
                    {
                        calls.Add("AdoptEmpty");
                        if (!ReferenceEquals(inventory, exactInventory))
                        {
                            throw new InvalidOperationException(
                                "Orchestrator did not pass the exact inventory instance to empty configuration adoption.");
                        }

                        return Task.FromResult(
                            new LMCRecoveredRecorderConfigurationLease(
                                null,
                                plan.DiagnosticsBootId,
                                plan.ConfigId,
                                plan.ConfigRevision,
                                plan.MapRevision,
                                plan.PreviousOwnerSessionEpoch,
                                NewOwner,
                                LMCRecorderState.Configured,
                                LMCRecorderBufferMode.Double,
                                2,
                                7,
                                null));
                    },
                    AdoptBankAsync = (plan, target) =>
                    {
                        calls.Add("Adopt" + target.BufferId);
                        if (CancelAfterFirstAdopt != null
                            && calls.Count == 2)
                        {
                            CancelAfterFirstAdopt.Cancel();
                        }

                        return Task.FromResult(
                            new LMCRecorderIdentity(
                                null,
                                plan.DiagnosticsBootId,
                                ReturnedBankRecordId == 0
                                    ? target.RecordId
                                    : ReturnedBankRecordId,
                                target.BufferId,
                                plan.ConfigId,
                                plan.ConfigRevision,
                                plan.MapRevision,
                                NewOwner,
                                target.State,
                                1,
                                10,
                                LMCCapturePhase.InputMapped,
                                1000,
                                ReturnedBankMode,
                                LMCRecorderTriggerType.Manual,
                                0,
                                0,
                                true,
                                1280,
                                new uint[] { 1 },
                                7,
                                null,
                                true));
                    },
                    EnsureMutationPlanConfirmed = plan => { }
                };
            }
        }
    }
}

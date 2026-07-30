using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RecorderDoubleQualificationJournalBridgeTests
    {
        private const uint BootId = 0x12345678u;
        private const uint ConfigId = 11;
        private const uint ConfigRevision = 12;
        private const uint MapRevision = 0x957F101Eu;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgeArmsExactRequest",
                JournalBridgeArmsExactRequest);
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgePersistsMonotonicCheckpoints",
                JournalBridgePersistsMonotonicCheckpoints);
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgeRejectsCheckpointBeforeConfigure",
                JournalBridgeRejectsCheckpointBeforeConfigure);
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgeRejectsConflictingThirdBank",
                JournalBridgeRejectsConflictingThirdBank);
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgeIdentityIsExact",
                JournalBridgeIdentityIsExact);
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgeResolveRequiresExactRelease",
                JournalBridgeResolveRequiresExactRelease);
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgeResolveAfterExactRelease",
                JournalBridgeResolveAfterExactRelease);
            tests.Add(
                "Qualification.RecorderDouble.JournalBridgeRejectsOtherReleasedScope",
                JournalBridgeRejectsOtherReleasedScope);
            tests.Add(
                "Qualification.RecorderDouble.PendingBankIntentRetriesExactTarget",
                PendingBankIntentRetriesExactTarget);
            tests.Add(
                "Qualification.RecorderDouble.PendingConfigurationIntentRetriesExactTarget",
                PendingConfigurationIntentRetriesExactTarget);
        }

        private static void JournalBridgeArmsExactRequest()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();

                var record = journal.CurrentRecord;
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.ArmedBeforeConfigureDispatch,
                    record.State);
                AssertEx.Equal(BootId, record.DiagnosticsBootId);
                AssertEx.Equal(MapRevision, record.MapRevision);
                AssertEx.Equal(ConfigId, record.RequestedConfigId);
                AssertEx.Equal((uint)0, record.ConfigRevision);
                AssertEx.Equal(0, record.Banks.Count);
                AssertEx.Equal(scope.RecoveryToken, record.Identity);
                AssertEx.Equal(
                    RecorderDoubleRecoveryTokenMarker.ClientTokenV1,
                    record.RecoveryTokenMarker);
                AssertEx.Equal(scope.RecoveryToken, record.RecoveryToken);
            });
        }

        private static void JournalBridgePersistsMonotonicCheckpoints()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.ConfigurationIdentified,
                    journal.CurrentRecord.State);

                scope.BankA = Capture(scope, 101, 0);
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(1, journal.CurrentRecord.Banks.Count);

                scope.BankB = Capture(scope, 102, 1);
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.CaptureEvidenceAvailable,
                    journal.CurrentRecord.State);
                AssertEx.Equal(2, journal.CurrentRecord.Banks.Count);
                AssertEx.Equal((uint)101,
                    journal.CurrentRecord.Banks[0].RecordId);
                AssertEx.Equal((uint)102,
                    journal.CurrentRecord.Banks[1].RecordId);
            });
        }

        private static void JournalBridgeRejectsCheckpointBeforeConfigure()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Throws<InvalidOperationException>(
                    () => bridge.PersistRecoveryCheckpointAsync(scope)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.ArmedBeforeConfigureDispatch,
                    journal.CurrentRecord.State);
            });
        }

        private static void JournalBridgeRejectsConflictingThirdBank()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);
                scope.BankA = Capture(scope, 101, 0);
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();

                scope.UnexpectedThird = Capture(scope, 999, 0);
                AssertEx.Throws<InvalidOperationException>(
                    () => bridge.PersistRecoveryCheckpointAsync(scope)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(1, journal.CurrentRecord.Banks.Count);
                AssertEx.Equal((uint)101,
                    journal.CurrentRecord.Banks[0].RecordId);
            });
        }

        private static void JournalBridgeIdentityIsExact()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);

                var wrongBridge = new RecorderDoubleQualificationJournalBridge(
                    journal,
                    Guid.NewGuid(),
                    () => new DateTime(
                        2026,
                        7,
                        28,
                        0,
                        0,
                        10,
                        DateTimeKind.Utc));
                AssertEx.Throws<InvalidOperationException>(
                    () => wrongBridge.PersistRecoveryCheckpointAsync(scope)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.ArmedBeforeConfigureDispatch,
                    journal.CurrentRecord.State);
            });
        }

        private static void JournalBridgeResolveRequiresExactRelease()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);
                scope.ConfigurationAttempted = true;
                scope.BankAStartAttempted = true;
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Throws<InvalidOperationException>(
                    () => bridge.ResolveAfterExactRelease(
                        scope,
                        true));
                scope.Configuration.BeginRelease();
                scope.Configuration.CompleteRelease();
                AssertEx.Throws<InvalidOperationException>(
                    () => bridge.ResolveAfterExactRelease(
                        scope,
                        true));
                AssertEx.True(journal.HasActiveRecord);
            });
        }

        private static void JournalBridgeResolveAfterExactRelease()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);
                scope.ConfigurationAttempted = true;
                scope.BankA = Capture(scope, 101, 0);
                scope.BankB = Capture(scope, 102, 1);
                scope.BankAStartAttempted = true;
                scope.BankBStartAttempted = true;
                scope.ThirdStartAttempted = true;
                scope.ThirdStartExactBusyConfirmed = true;
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();

                var identity = journal.CurrentRecord.Identity;
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => new DateTime(
                            2026,
                            7,
                            28,
                            0,
                            0,
                            10,
                            DateTimeKind.Utc));
                var operations = ReleaseOperations(journal);
                coordinator.ReleaseQualificationBankAsync(
                        scope,
                        scope.BankB,
                        operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                coordinator.ReleaseQualificationBankAsync(
                        scope,
                        scope.BankA,
                        operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Throws<InvalidOperationException>(
                    () => coordinator
                        .ReleaseQualificationConfigurationAndResolveAsync(
                            scope,
                            operations,
                            false,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                var resolved = coordinator
                    .ReleaseQualificationConfigurationAndResolveAsync(
                        scope,
                        operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.Resolved,
                    resolved.State);
                AssertEx.False(journal.HasActiveRecord);

                var nextIdentity = Guid.NewGuid();
                var next = journal.ArmBeforeConfigureDispatch(
                    nextIdentity,
                    new DateTime(
                        2026,
                        7,
                        28,
                        0,
                        1,
                        0,
                        DateTimeKind.Utc),
                    BootId + 1,
                    MapRevision + 1,
                    ConfigId + 1);
                AssertEx.Equal(nextIdentity, next.Identity);
                AssertEx.True(journal.HasActiveRecord);
            });
        }

        private static RecorderDoubleBankConfigurationLease Configuration(
            RecorderDoubleBankRecoveryScope scope)
        {
            return new RecorderDoubleBankConfigurationLease(
                new object(),
                BootId,
                ConfigId,
                ConfigRevision,
                scope.Request.OwnerToken,
                scope.Request.SessionToken,
                false);
        }

        private static void PendingBankIntentRetriesExactTarget()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);
                scope.ConfigurationAttempted = true;
                scope.BankA = Capture(scope, 101, 0);
                scope.BankAStartAttempted = true;
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();

                var identity = journal.CurrentRecord.Identity;
                journal.BeginBankRelease(
                    identity,
                    new DateTime(
                        2026,
                        7,
                        28,
                        0,
                        0,
                        10,
                        DateTimeKind.Utc),
                    BootId,
                    ConfigId,
                    ConfigRevision,
                    MapRevision,
                    scope.BankA.BufferId,
                    scope.BankA.RecordId);

                var releaseCalls = 0;
                var operations = ReleaseOperations(journal);
                operations.ReleaseBankAsync = capture =>
                {
                    releaseCalls++;
                    AssertEx.True(
                        journal.CurrentRecord.IsBankReleasePending(
                            capture.BufferId,
                            capture.RecordId));
                    return Task.CompletedTask;
                };
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => new DateTime(
                            2026,
                            7,
                            28,
                            0,
                            0,
                            11,
                            DateTimeKind.Utc));

                coordinator.ReleaseQualificationBankAsync(
                        scope,
                        scope.BankA,
                        operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, releaseCalls);
                AssertEx.True(scope.BankA.IsReleased);
                AssertEx.False(
                    journal.CurrentRecord.IsBankReleasePending(0, 101));
                AssertEx.True(journal.HasActiveRecord);
            });
        }

        private static void PendingConfigurationIntentRetriesExactTarget()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);
                scope.ConfigurationAttempted = true;
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();

                var identity = journal.CurrentRecord.Identity;
                journal.BeginConfigurationRelease(
                    identity,
                    new DateTime(
                        2026,
                        7,
                        28,
                        0,
                        0,
                        10,
                        DateTimeKind.Utc),
                    BootId,
                    ConfigId,
                    ConfigRevision,
                    MapRevision);

                var releaseCalls = 0;
                var operations = ReleaseOperations(journal);
                operations.ReleaseConfigurationAsync = configuration =>
                {
                    releaseCalls++;
                    AssertEx.True(
                        journal.CurrentRecord
                            .HasConfigurationReleaseOutcomeUncertain);
                    return Task.CompletedTask;
                };
                var coordinator = new
                    RecorderDoubleDurableReleaseCoordinator(
                        journal,
                        identity,
                        () => new DateTime(
                            2026,
                            7,
                            28,
                            0,
                            0,
                            11,
                            DateTimeKind.Utc));

                var resolved = coordinator
                    .ReleaseQualificationConfigurationAndResolveAsync(
                        scope,
                        operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                AssertEx.Equal(1, releaseCalls);
                AssertEx.True(scope.Configuration.IsReleased);
                AssertEx.Equal(
                    RecorderDoubleRecoveryState.Resolved,
                    resolved.State);
                AssertEx.False(journal.HasActiveRecord);
            });
        }

        private static void JournalBridgeRejectsOtherReleasedScope()
        {
            WithJournal((journal, bridge, scope) =>
            {
                bridge.ArmRecoveryBeforeConfigureAsync(scope)
                    .GetAwaiter()
                    .GetResult();
                scope.Configuration = Configuration(scope);
                scope.ConfigurationAttempted = true;
                scope.BankA = Capture(scope, 101, 0);
                scope.BankAStartAttempted = true;
                bridge.PersistRecoveryCheckpointAsync(scope)
                    .GetAwaiter()
                    .GetResult();

                var other = Scope();
                other.Configuration = Configuration(other);
                other.ConfigurationAttempted = true;
                other.Configuration.BeginRelease();
                other.Configuration.CompleteRelease();

                AssertEx.Throws<InvalidOperationException>(
                    () => bridge.ResolveAfterExactRelease(other, true));
                AssertEx.True(journal.HasActiveRecord);
                AssertEx.Equal(1, journal.CurrentRecord.Banks.Count);
            });
        }

        private static RecorderDoubleBankCaptureLease Capture(
            RecorderDoubleBankRecoveryScope scope,
            uint recordId,
            uint bufferId)
        {
            return new RecorderDoubleBankCaptureLease(
                new object(),
                BootId,
                ConfigId,
                ConfigRevision,
                recordId,
                bufferId,
                scope.Request.OwnerToken,
                scope.Request.SessionToken,
                false);
        }

        private static RecorderDoubleBankQualificationOperations
            ReleaseOperations(RecorderDoubleRecoveryJournal journal)
        {
            return new RecorderDoubleBankQualificationOperations
            {
                ArmRecoveryBeforeConfigureAsync = scope =>
                    Task.CompletedTask,
                PersistRecoveryCheckpointAsync = scope =>
                    Task.CompletedTask,
                ConfigureAsync = (configuration, recoveryToken) =>
                    throw new InvalidOperationException(),
                StartAsync = configuration =>
                    throw new InvalidOperationException(),
                WaitForFrozenAsync = capture =>
                    throw new InvalidOperationException(),
                DownloadAsync = capture =>
                    throw new InvalidOperationException(),
                IsExactResourceBusy = error => false,
                IsReleaseConfirmedNotApplied = error => false,
                RecoveryRequired = (scope, error) => { },
                ReleaseBankAsync = capture =>
                {
                    AssertEx.True(
                        journal.CurrentRecord.IsBankReleasePending(
                            capture.BufferId,
                            capture.RecordId),
                        "Bank intent must be durable before Release dispatch.");
                    return Task.CompletedTask;
                },
                ReleaseConfigurationAsync = configuration =>
                {
                    AssertEx.True(
                        journal.CurrentRecord
                            .HasConfigurationReleaseOutcomeUncertain,
                        "Configuration intent must be durable before Release dispatch.");
                    return Task.CompletedTask;
                }
            };
        }

        private static RecorderDoubleBankRecoveryScope Scope()
        {
            var capabilities = new LMCDiagnosticCapabilities(
                null,
                7,
                3,
                (uint)(LMCDiagnosticCapability.RecorderSingleBank
                    | LMCDiagnosticCapability.RecorderDoubleBank),
                MapRevision,
                24,
                32,
                32,
                2,
                100,
                1000,
                1320,
                2040,
                1280,
                80,
                16,
                800,
                4,
                BootId);
            var configuration = new LMCRecorderConfiguration(
                new uint[] { 1, 2, 3, 4 },
                1,
                4,
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Manual,
                LMCSignalValueType.Invalid,
                0,
                0,
                0,
                LMCRecorderTriggerOperator.None,
                0,
                0,
                ConfigId);
            return new RecorderDoubleBankRecoveryScope(
                new RecorderDoubleBankQualificationRequest(
                    capabilities,
                    configuration,
                    new object(),
                    new object()));
        }

        private static void WithJournal(
            Action<RecorderDoubleRecoveryJournal,
                RecorderDoubleQualificationJournalBridge,
                RecorderDoubleBankRecoveryScope> body)
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "ElmoRecorderDoubleQualificationJournalBridgeTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                using (var journal =
                    RecorderDoubleRecoveryJournal.Open(directory))
                {
                    var tick = 0;
                    var bridge = new
                        RecorderDoubleQualificationJournalBridge(
                            journal,
                            Guid.NewGuid(),
                            () => new DateTime(
                                2026,
                                7,
                                28,
                                0,
                                0,
                                tick++,
                                DateTimeKind.Utc));
                    body(journal, bridge, Scope());
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
    }
}

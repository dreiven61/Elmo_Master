using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RecorderDoubleBankQualificationOrchestratorTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.RecorderDouble.PreflightFailuresAreZeroOperation",
                PreflightFailuresAreZeroOperation);
            tests.Add(
                "Qualification.RecorderDouble.ConfigureOnlyRetainsWithoutStart",
                ConfigureOnlyRetainsWithoutStart);
            tests.Add(
                "Qualification.RecorderDouble.ConfigureOnlyAmbiguousResultPublishesRecovery",
                ConfigureOnlyAmbiguousResultPublishesRecovery);
            tests.Add(
                "Qualification.RecorderDouble.ConfigureOnlyCheckpointFailurePublishesExactLease",
                ConfigureOnlyCheckpointFailurePublishesExactLease);
            tests.Add(
                "Qualification.RecorderDouble.RecoveryArmFailureIsZeroOperation",
                RecoveryArmFailureIsZeroOperation);
            tests.Add(
                "Qualification.RecorderDouble.CheckpointFailurePublishesRecovery",
                CheckpointFailurePublishesRecovery);
            tests.Add(
                "Qualification.RecorderDouble.AcceptedLifecycleBusyAndInvariant",
                AcceptedLifecycleBusyAndInvariant);
            tests.Add(
                "Qualification.RecorderDouble.ForeignConfigurationStopsBeforeStart",
                ForeignConfigurationStopsBeforeStart);
            tests.Add(
                "Qualification.RecorderDouble.MismatchedRequestedConfigStopsBeforeStart",
                MismatchedRequestedConfigStopsBeforeStart);
            tests.Add(
                "Qualification.RecorderDouble.BankAMutationPreservesBoth",
                BankAMutationPreservesBoth);
            tests.Add(
                "Qualification.RecorderDouble.CancellationPreservesBankA",
                CancellationPreservesBankA);
            tests.Add(
                "Qualification.RecorderDouble.UnexpectedThirdPreserved",
                UnexpectedThirdPreserved);
            tests.Add(
                "Qualification.RecorderDouble.ExplicitReleaseOrderAndIsolation",
                ExplicitReleaseOrderAndIsolation);
            tests.Add(
                "Qualification.RecorderDouble.ReleaseSuccessWinsLateCancellation",
                ReleaseSuccessWinsLateCancellation);
            tests.Add(
                "Qualification.RecorderDouble.AmbiguousConfigurePreservesInterlock",
                AmbiguousConfigurePreservesInterlock);
            tests.Add(
                "Qualification.RecorderDouble.ReleaseFailureClassification",
                ReleaseFailureClassification);
            tests.Add(
                "Qualification.RecorderDouble.AmbiguousStartBlocksDestructiveCleanup",
                AmbiguousStartBlocksDestructiveCleanup);
            tests.Add(
                "Qualification.RecorderDouble.PlcCoreReferenceModel",
                PlcCoreReferenceModel);
        }

        private static void ConfigureOnlyRetainsWithoutStart()
        {
            var state = new FakeState();
            var scope = RecorderDoubleBankQualificationOrchestrator
                .ConfigureAndRetainAsync(
                    CreateRequest(
                        CreateCapabilities(
                            LMCDiagnosticCapability.RecorderSingleBank
                                | LMCDiagnosticCapability.RecorderDoubleBank,
                            2,
                            0x12345678u),
                        CreateDoubleConfiguration(),
                        state.OwnerToken,
                        state.SessionToken),
                    state.CreateOperations(),
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            AssertEx.Equal("Configure", string.Join(",", state.Calls));
            AssertEx.Equal("CONFIGURATION_RETAINED", scope.Stage);
            AssertEx.True(scope.ConfigurationAttempted);
            AssertEx.NotNull(scope.Configuration);
            AssertEx.Equal(null, scope.BankA);
            AssertEx.Equal(null, scope.BankB);
            AssertEx.Equal(null, scope.UnexpectedThird);
            AssertEx.False(scope.BankAStartAttempted);
            AssertEx.False(scope.BankBStartAttempted);
            AssertEx.False(scope.ThirdStartAttempted);
            AssertEx.Equal(1, state.RecoveryArmCount);
            AssertEx.Equal(1, state.RecoveryCheckpointCount);
            AssertEx.Equal(0, state.RecoveryCount);
        }

        private static void ConfigureOnlyAmbiguousResultPublishesRecovery()
        {
            var state = new FakeState
            {
                ConfigureError = new IOException("configure response lost")
            };

            var error = AssertEx.Throws<IOException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ConfigureAndRetainAsync(
                        CreateRequest(
                            CreateCapabilities(
                                LMCDiagnosticCapability.RecorderSingleBank
                                    | LMCDiagnosticCapability.RecorderDoubleBank,
                                2,
                                0x12345678u),
                            CreateDoubleConfiguration(),
                            state.OwnerToken,
                            state.SessionToken),
                        state.CreateOperations(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());

            AssertEx.True(ReferenceEquals(state.ConfigureError, error));
            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.True(ReferenceEquals(error, state.RecoveryError));
            AssertEx.Equal("CONFIGURE", state.RecoveryScope.Stage);
            AssertEx.True(state.RecoveryScope.ConfigurationAttempted);
            AssertEx.Equal(null, state.RecoveryScope.Configuration);
            AssertEx.True(state.RecoveryScope.HasAnyPossibleResource);
            AssertEx.Equal(0, state.RecoveryCheckpointCount);
        }

        private static void
            ConfigureOnlyCheckpointFailurePublishesExactLease()
        {
            var state = new FakeState
            {
                RecoveryCheckpointErrorAt = 1,
                RecoveryCheckpointError = new IOException(
                    "configuration checkpoint failed")
            };

            var error = AssertEx.Throws<IOException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ConfigureAndRetainAsync(
                        CreateRequest(
                            CreateCapabilities(
                                LMCDiagnosticCapability.RecorderSingleBank
                                    | LMCDiagnosticCapability.RecorderDoubleBank,
                                2,
                                0x12345678u),
                            CreateDoubleConfiguration(),
                            state.OwnerToken,
                            state.SessionToken),
                        state.CreateOperations(),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());

            AssertEx.True(
                ReferenceEquals(state.RecoveryCheckpointError, error));
            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.True(ReferenceEquals(error, state.RecoveryError));
            AssertEx.Equal(
                "PERSIST_CONFIGURATION",
                state.RecoveryScope.Stage);
            AssertEx.NotNull(state.RecoveryScope.Configuration);
            AssertEx.True(state.RecoveryScope.HasAnyPossibleResource);
            AssertEx.Equal("Configure", string.Join(",", state.Calls));
        }

        private static void PreflightFailuresAreZeroOperation()
        {
            var state = new FakeState();
            var operations = state.CreateOperations();
            var invalidRequests = new[]
            {
                CreateRequest(
                    CreateCapabilities(
                        LMCDiagnosticCapability.RecorderSingleBank,
                        2,
                        0x12345678u),
                    CreateDoubleConfiguration()),
                CreateRequest(
                    CreateCapabilities(
                        LMCDiagnosticCapability.RecorderSingleBank
                            | LMCDiagnosticCapability.RecorderDoubleBank,
                        1,
                        0x12345678u),
                    CreateDoubleConfiguration()),
                CreateRequest(
                    CreateCapabilities(
                        LMCDiagnosticCapability.RecorderSingleBank
                            | LMCDiagnosticCapability.RecorderDoubleBank,
                        2,
                        0),
                    CreateDoubleConfiguration()),
                CreateRequest(
                    CreateCapabilities(
                        LMCDiagnosticCapability.RecorderSingleBank
                            | LMCDiagnosticCapability.RecorderDoubleBank,
                        2,
                        0x12345678u),
                    CreateDoubleConfiguration(0)),
                CreateRequest(
                    CreateCapabilities(
                        LMCDiagnosticCapability.RecorderSingleBank
                            | LMCDiagnosticCapability.RecorderDoubleBank,
                        2,
                        0x12345678u),
                    new LMCRecorderConfiguration(
                        new uint[] { 1, 2, 3, 4 },
                        1,
                        4))
            };

            foreach (var request in invalidRequests)
            {
                AssertEx.Throws<Exception>(
                    () => RecorderDoubleBankQualificationOrchestrator
                        .RunAsync(
                            request,
                            operations,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
            }

            AssertEx.Equal(0, state.Calls.Count);
            AssertEx.Equal(0, state.RecoveryCount);
        }

        private static void AcceptedLifecycleBusyAndInvariant()
        {
            var state = new FakeState();
            var result = Run(state, CancellationToken.None);

            AssertEx.Equal(
                "Configure,StartA,FreezeA,DownloadA1,StartB,FreezeB,DownloadB,StartThird,RereadA",
                string.Join(",", state.Calls));
            AssertEx.Equal((uint)0, result.RecoveryScope.BankA.BufferId);
            AssertEx.Equal((uint)1, result.RecoveryScope.BankB.BufferId);
            AssertEx.Equal((uint)101, result.RecoveryScope.BankA.RecordId);
            AssertEx.Equal((uint)102, result.RecoveryScope.BankB.RecordId);
            AssertEx.Equal(
                result.BankAInitial.HeaderSha256,
                result.BankAReread.HeaderSha256);
            AssertEx.Equal(
                result.BankAInitial.DataSha256,
                result.BankAReread.DataSha256);
            AssertEx.True(
                ReferenceEquals(
                    state.BusyException,
                    result.ThirdStartBusyException));
            AssertEx.Equal(0, state.ReleaseCalls.Count);
            AssertEx.Equal(0, state.RecoveryCount);
            AssertEx.Equal(1, state.RecoveryArmCount);
            AssertEx.Equal(3, state.RecoveryCheckpointCount);
        }

        private static void RecoveryArmFailureIsZeroOperation()
        {
            var state = new FakeState
            {
                RecoveryArmError = new IOException(
                    "durable recovery journal unavailable")
            };
            AssertEx.Throws<IOException>(
                () => Run(state, CancellationToken.None));

            AssertEx.Equal(1, state.RecoveryArmCount);
            AssertEx.Equal(0, state.RecoveryCheckpointCount);
            AssertEx.Equal(0, state.Calls.Count);
            AssertEx.Equal(0, state.RecoveryCount);
        }

        private static void CheckpointFailurePublishesRecovery()
        {
            var state = new FakeState
            {
                RecoveryCheckpointErrorAt = 1,
                RecoveryCheckpointError = new IOException(
                    "durable checkpoint unavailable")
            };
            AssertEx.Throws<IOException>(
                () => Run(state, CancellationToken.None));

            AssertEx.Equal("Configure", string.Join(",", state.Calls));
            AssertEx.Equal(1, state.RecoveryArmCount);
            AssertEx.Equal(1, state.RecoveryCheckpointCount);
            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.Equal(
                "PERSIST_CONFIGURATION",
                state.RecoveryScope.Stage);
            AssertEx.NotNull(state.RecoveryScope.Configuration);
            AssertEx.True(state.RecoveryScope.HasAnyPossibleResource);
        }

        private static void ForeignConfigurationStopsBeforeStart()
        {
            var state = new FakeState();
            state.ForeignConfiguration = true;
            AssertEx.Throws<InvalidOperationException>(
                () => Run(state, CancellationToken.None));

            AssertEx.Equal("Configure", string.Join(",", state.Calls));
            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.NotNull(state.RecoveryScope.Configuration);
            AssertEx.Equal(0, state.ReleaseCalls.Count);
        }

        private static void MismatchedRequestedConfigStopsBeforeStart()
        {
            var state = new FakeState
            {
                ReturnedConfigId = 99
            };
            AssertEx.Throws<InvalidOperationException>(
                () => Run(state, CancellationToken.None));

            AssertEx.Equal("Configure", string.Join(",", state.Calls));
            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.NotNull(state.RecoveryScope.Configuration);
            AssertEx.Equal((uint)99, state.RecoveryScope.Configuration.ConfigId);
            AssertEx.Equal(0, state.ReleaseCalls.Count);
        }

        private static void BankAMutationPreservesBoth()
        {
            var state = new FakeState();
            state.MutateBankAReread = true;
            AssertEx.Throws<InvalidOperationException>(
                () => Run(state, CancellationToken.None));

            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.NotNull(state.RecoveryScope.Configuration);
            AssertEx.NotNull(state.RecoveryScope.BankA);
            AssertEx.NotNull(state.RecoveryScope.BankB);
            AssertEx.Equal("REREAD_A", state.RecoveryScope.Stage);
            AssertEx.Equal(0, state.ReleaseCalls.Count);
        }

        private static void CancellationPreservesBankA()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                var state = new FakeState();
                state.CancelAfterFirstDownload = cancellation;
                AssertEx.Throws<OperationCanceledException>(
                    () => Run(state, cancellation.Token));

                AssertEx.Equal(
                    "Configure,StartA,FreezeA,DownloadA1",
                    string.Join(",", state.Calls));
                AssertEx.Equal(1, state.RecoveryCount);
                AssertEx.NotNull(state.RecoveryScope.Configuration);
                AssertEx.NotNull(state.RecoveryScope.BankA);
                AssertEx.Equal(null, state.RecoveryScope.BankB);
                AssertEx.Equal(0, state.ReleaseCalls.Count);
            }
        }

        private static void UnexpectedThirdPreserved()
        {
            var state = new FakeState();
            state.ReturnUnexpectedThird = true;
            AssertEx.Throws<InvalidOperationException>(
                () => Run(state, CancellationToken.None));

            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.NotNull(state.RecoveryScope.BankA);
            AssertEx.NotNull(state.RecoveryScope.BankB);
            AssertEx.NotNull(state.RecoveryScope.UnexpectedThird);
            AssertEx.Equal((uint)103, state.RecoveryScope.UnexpectedThird.RecordId);
            AssertEx.Equal(0, state.ReleaseCalls.Count);
        }

        private static void ExplicitReleaseOrderAndIsolation()
        {
            var state = new FakeState();
            var result = Run(state, CancellationToken.None);
            var scope = result.RecoveryScope;
            var bankARecordId = scope.BankA.RecordId;
            var bankAOwner = scope.BankA.OwnerToken;

            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseBankAsync(
                        scope,
                        scope.BankA,
                        state.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Equal(0, state.ReleaseCalls.Count);

            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseBankAsync(
                        scope,
                        scope.BankB,
                        state.Operations,
                        false,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Equal(0, state.ReleaseCalls.Count);

            RecorderDoubleBankQualificationOrchestrator.ReleaseBankAsync(
                scope,
                scope.BankB,
                state.Operations,
                true,
                CancellationToken.None).GetAwaiter().GetResult();
            AssertEx.True(scope.BankB.IsReleased);
            AssertEx.False(scope.BankA.IsReleased);
            AssertEx.False(scope.Configuration.IsReleased);
            AssertEx.Equal(bankARecordId, scope.BankA.RecordId);
            AssertEx.True(ReferenceEquals(bankAOwner, scope.BankA.OwnerToken));

            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseConfigurationAsync(
                        scope,
                        state.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());

            RecorderDoubleBankQualificationOrchestrator.ReleaseBankAsync(
                scope,
                scope.BankA,
                state.Operations,
                true,
                CancellationToken.None).GetAwaiter().GetResult();
            RecorderDoubleBankQualificationOrchestrator
                .ReleaseConfigurationAsync(
                    scope,
                    state.Operations,
                    true,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            AssertEx.Equal(
                "ReleaseB,ReleaseA,ReleaseConfig",
                string.Join(",", state.ReleaseCalls));
            AssertEx.True(scope.BankA.IsReleased);
            AssertEx.True(scope.BankB.IsReleased);
            AssertEx.True(scope.Configuration.IsReleased);
        }

        private static void ReleaseSuccessWinsLateCancellation()
        {
            var state = new FakeState();
            var result = Run(state, CancellationToken.None);
            var scope = result.RecoveryScope;
            using (var bankCancellation = new CancellationTokenSource())
            using (var configCancellation = new CancellationTokenSource())
            {
                state.CancelDuringBankRelease = bankCancellation;
                RecorderDoubleBankQualificationOrchestrator.ReleaseBankAsync(
                        scope,
                        scope.BankB,
                        state.Operations,
                        true,
                        bankCancellation.Token)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(scope.BankB.IsReleased);

                state.CancelDuringBankRelease = null;
                RecorderDoubleBankQualificationOrchestrator.ReleaseBankAsync(
                        scope,
                        scope.BankA,
                        state.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(scope.BankA.IsReleased);

                state.CancelDuringConfigurationRelease = configCancellation;
                RecorderDoubleBankQualificationOrchestrator
                    .ReleaseConfigurationAsync(
                        scope,
                        state.Operations,
                        true,
                        configCancellation.Token)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.True(scope.Configuration.IsReleased);
            }
        }

        private static void AmbiguousConfigurePreservesInterlock()
        {
            var state = new FakeState
            {
                ConfigureError = new InvalidOperationException(
                    "configure response lost")
            };

            AssertEx.Throws<InvalidOperationException>(
                () => Run(state, CancellationToken.None));
            AssertEx.Equal(1, state.RecoveryCount);
            AssertEx.NotNull(state.RecoveryScope);
            AssertEx.True(state.RecoveryScope.ConfigurationAttempted);
            AssertEx.Equal(null, state.RecoveryScope.Configuration);
            AssertEx.True(state.RecoveryScope.HasAnyPossibleResource);
        }

        private static void ReleaseFailureClassification()
        {
            var confirmedState = new FakeState();
            var confirmedScope = Run(
                confirmedState,
                CancellationToken.None).RecoveryScope;
            confirmedState.ReleaseBankError =
                confirmedState.ConfirmedNotAppliedReleaseError;
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseBankAsync(
                        confirmedScope,
                        confirmedScope.BankB,
                        confirmedState.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.False(confirmedScope.BankB.IsReleased);
            AssertEx.False(confirmedScope.BankB.IsReleaseOutcomeUnverified);
            confirmedState.ReleaseBankError = null;
            RecorderDoubleBankQualificationOrchestrator.ReleaseBankAsync(
                    confirmedScope,
                    confirmedScope.BankB,
                    confirmedState.Operations,
                    true,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            AssertEx.True(confirmedScope.BankB.IsReleased);

            var uncertainState = new FakeState();
            var uncertainScope = Run(
                uncertainState,
                CancellationToken.None).RecoveryScope;
            uncertainState.ReleaseBankError =
                new InvalidOperationException("release acknowledgement lost");
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseBankAsync(
                        uncertainScope,
                        uncertainScope.BankB,
                        uncertainState.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.True(uncertainScope.BankB.IsReleaseOutcomeUnverified);
            var releaseCallCount = uncertainState.ReleaseCalls.Count;
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseBankAsync(
                        uncertainScope,
                        uncertainScope.BankB,
                        uncertainState.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Equal(releaseCallCount, uncertainState.ReleaseCalls.Count);

            var configState = new FakeState();
            var configScope = Run(
                configState,
                CancellationToken.None).RecoveryScope;
            RecorderDoubleBankQualificationOrchestrator.ReleaseBankAsync(
                    configScope,
                    configScope.BankB,
                    configState.Operations,
                    true,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            RecorderDoubleBankQualificationOrchestrator.ReleaseBankAsync(
                    configScope,
                    configScope.BankA,
                    configState.Operations,
                    true,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            configState.ReleaseConfigurationError =
                new InvalidOperationException("configuration release unknown");
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseConfigurationAsync(
                        configScope,
                        configState.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.True(
                configScope.Configuration.IsReleaseOutcomeUnverified);
            AssertEx.True(configScope.HasAnyPossibleResource);
        }

        private static void AmbiguousStartBlocksDestructiveCleanup()
        {
            var bankAState = new FakeState
            {
                StartErrorAt = 1,
                StartError = new InvalidOperationException(
                    "Bank A start response lost")
            };
            AssertEx.Throws<InvalidOperationException>(
                () => Run(bankAState, CancellationToken.None));
            AssertEx.True(bankAState.RecoveryScope.BankAStartAttempted);
            AssertEx.Equal(null, bankAState.RecoveryScope.BankA);
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseConfigurationAsync(
                        bankAState.RecoveryScope,
                        bankAState.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Equal(0, bankAState.ReleaseCalls.Count);

            var bankBState = new FakeState
            {
                StartErrorAt = 2,
                StartError = new InvalidOperationException(
                    "Bank B start response lost")
            };
            AssertEx.Throws<InvalidOperationException>(
                () => Run(bankBState, CancellationToken.None));
            AssertEx.True(bankBState.RecoveryScope.BankBStartAttempted);
            AssertEx.Equal(null, bankBState.RecoveryScope.BankB);
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseBankAsync(
                        bankBState.RecoveryScope,
                        bankBState.RecoveryScope.BankA,
                        bankBState.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Equal(0, bankBState.ReleaseCalls.Count);

            var thirdState = new FakeState
            {
                StartErrorAt = 3,
                StartError = new InvalidOperationException(
                    "third start response lost")
            };
            AssertEx.Throws<InvalidOperationException>(
                () => Run(thirdState, CancellationToken.None));
            AssertEx.True(thirdState.RecoveryScope.ThirdStartAttempted);
            AssertEx.False(
                thirdState.RecoveryScope.ThirdStartExactBusyConfirmed);
            AssertEx.Equal(null, thirdState.RecoveryScope.UnexpectedThird);
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderDoubleBankQualificationOrchestrator
                    .ReleaseBankAsync(
                        thirdState.RecoveryScope,
                        thirdState.RecoveryScope.BankB,
                        thirdState.Operations,
                        true,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult());
            AssertEx.Equal(0, thirdState.ReleaseCalls.Count);
        }

        private static void PlcCoreReferenceModel()
        {
            const uint firstOwner = 0x101u;
            const uint reboundOwner = 0x202u;
            const uint bootId = 0x12345678u;
            var model = new PlcDoubleBankReferenceModel();
            model.Configure(firstOwner, bootId);

            var bankA = model.Start(firstOwner, bootId);
            AssertEx.Equal((uint)0, bankA.BufferId);
            model.Complete(
                bankA,
                new byte[] { 0xA0, 0x01 },
                new byte[] { 1, 2, 3, 4 });
            var bankAInitial = model.Read(bankA, bootId);

            var staleReadyEntry = model.CaptureRtEntry();
            var bankB = model.Start(firstOwner, bootId);
            AssertEx.Equal((uint)1, bankB.BufferId);
            AssertEx.False(model.TryPublish(staleReadyEntry));
            AssertEx.Equal(
                ReferenceBankState.Armed,
                model.GetState(bankB.BufferId));
            var bankADuringBankB = model.Read(bankA, bootId);
            AssertEx.SequenceEqual(
                bankAInitial.Header,
                bankADuringBankB.Header);
            AssertEx.SequenceEqual(
                bankAInitial.Data,
                bankADuringBankB.Data);

            model.Complete(
                bankB,
                new byte[] { 0xB0, 0x01 },
                new byte[] { 5, 6, 7, 8 });
            AssertEx.Throws<RecorderResourceBusyException>(
                () => model.Start(firstOwner, bootId));

            model.NotifySessionClosed(firstOwner);
            AssertEx.Throws<InvalidOperationException>(
                () => model.Adopt(0, 0, reboundOwner, bootId));
            model.Adopt(bankA.RecordId, bankA.BufferId, reboundOwner, bootId);
            model.Adopt(bankB.RecordId, bankB.BufferId, reboundOwner, bootId);
            AssertEx.Equal(reboundOwner, model.GetOwner(bankA.BufferId));
            AssertEx.Equal(reboundOwner, model.GetOwner(bankB.BufferId));

            model.Release(bankB, reboundOwner, bootId);
            var bankAAfterBankBRelease = model.Read(bankA, bootId);
            AssertEx.SequenceEqual(
                bankAInitial.Header,
                bankAAfterBankBRelease.Header);
            AssertEx.SequenceEqual(
                bankAInitial.Data,
                bankAAfterBankBRelease.Data);
            AssertEx.Equal(
                ReferenceBankState.Configured,
                model.GetState(bankB.BufferId));

            model.Release(bankA, reboundOwner, bootId);
            model.ReleaseConfiguration(reboundOwner, bootId);
            AssertEx.False(model.IsConfigured);

            var inconsistent = new PlcDoubleBankReferenceModel();
            inconsistent.Configure(firstOwner, bootId);
            var inconsistentA = inconsistent.Start(firstOwner, bootId);
            inconsistent.Complete(
                inconsistentA,
                new byte[] { 1 },
                new byte[] { 2 });
            var inconsistentB = inconsistent.Start(firstOwner, bootId);
            inconsistent.Complete(
                inconsistentB,
                new byte[] { 3 },
                new byte[] { 4 });
            inconsistent.NotifySessionClosed(firstOwner);
            inconsistent.CorruptMapForTest(inconsistentB.BufferId);
            AssertEx.Throws<InvalidOperationException>(
                () => inconsistent.Adopt(
                    inconsistentA.RecordId,
                    inconsistentA.BufferId,
                    reboundOwner,
                    bootId));
            AssertEx.Equal(firstOwner, inconsistent.GetOwner(0));
            AssertEx.Equal(firstOwner, inconsistent.GetOwner(1));
        }

        private static RecorderDoubleBankQualificationResult Run(
            FakeState state,
            CancellationToken cancellationToken)
        {
            return RecorderDoubleBankQualificationOrchestrator.RunAsync(
                    CreateRequest(
                        CreateCapabilities(
                            LMCDiagnosticCapability.RecorderSingleBank
                                | LMCDiagnosticCapability.RecorderDoubleBank,
                            2,
                            0x12345678u),
                        CreateDoubleConfiguration(),
                        state.OwnerToken,
                        state.SessionToken),
                    state.CreateOperations(),
                    cancellationToken)
                .GetAwaiter()
                .GetResult();
        }

        private static RecorderDoubleBankQualificationRequest CreateRequest(
            LMCDiagnosticCapabilities capabilities,
            LMCRecorderConfiguration configuration,
            object ownerToken = null,
            object sessionToken = null)
        {
            return new RecorderDoubleBankQualificationRequest(
                capabilities,
                configuration,
                ownerToken ?? new object(),
                sessionToken ?? new object());
        }

        private static LMCRecorderConfiguration CreateDoubleConfiguration(
            uint requestedConfigId = 11)
        {
            return new LMCRecorderConfiguration(
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
                requestedConfigId);
        }

        private static LMCDiagnosticCapabilities CreateCapabilities(
            LMCDiagnosticCapability capabilities,
            ushort bufferCount,
            uint bootId)
        {
            return new LMCDiagnosticCapabilities(
                null,
                7,
                3,
                (uint)capabilities,
                0x10203040u,
                24,
                32,
                32,
                bufferCount,
                100,
                1000,
                1320,
                2040,
                1280,
                80,
                16,
                800,
                4,
                bootId);
        }

        private enum ReferenceBankState
        {
            Empty = 0,
            Configured = 1,
            Armed = 2,
            Recording = 3,
            Ready = 4,
            Uploading = 5
        }

        private sealed class RecorderResourceBusyException
            : InvalidOperationException
        {
            internal RecorderResourceBusyException()
                : base("exact ResourceBusy")
            {
            }
        }

        private sealed class ReferenceLease
        {
            internal ReferenceLease(uint recordId, uint bufferId)
            {
                RecordId = recordId;
                BufferId = bufferId;
            }

            internal uint RecordId { get; private set; }
            internal uint BufferId { get; private set; }
        }

        private sealed class ReferenceSnapshot
        {
            internal ReferenceSnapshot(byte[] header, byte[] data)
            {
                Header = header;
                Data = data;
            }

            internal byte[] Header { get; private set; }
            internal byte[] Data { get; private set; }
        }

        private sealed class ReferenceRtEntry
        {
            internal uint Generation;
            internal ReferenceBankState State;
            internal uint RecordId;
            internal uint BufferId;
        }

        private sealed class ReferenceBank
        {
            internal ReferenceBankState State;
            internal uint RecordId;
            internal uint ConfigId;
            internal uint ConfigRevision;
            internal uint MapRevision;
            internal uint Owner;
            internal uint ClosedOwner;
            internal uint BootId;
            internal byte[] Header;
            internal byte[] Data;

            internal void Reset(ReferenceBankState state)
            {
                State = state;
                RecordId = 0;
                ConfigId = 0;
                ConfigRevision = 0;
                MapRevision = 0;
                Owner = 0;
                ClosedOwner = 0;
                BootId = 0;
                Header = null;
                Data = null;
            }
        }

        private sealed class PlcDoubleBankReferenceModel
        {
            private const uint ConfigId = 11;
            private const uint ConfigRevision = 12;
            private const uint MapRevision = 0x957F101Eu;
            private readonly ReferenceBank[] banks =
            {
                new ReferenceBank(),
                new ReferenceBank()
            };
            private uint owner;
            private uint closedOwner;
            private uint bootId;
            private uint nextRecordId = 101;
            private uint generation;
            private uint activeRecordId;
            private uint activeBufferId;
            private ReferenceBankState activeState;

            internal bool IsConfigured { get; private set; }

            internal void Configure(uint requestedOwner, uint requestedBootId)
            {
                if (requestedOwner == 0 || requestedBootId == 0)
                {
                    throw new InvalidOperationException("invalid lease");
                }

                for (var index = 0; index < banks.Length; index++)
                {
                    if (banks[index].RecordId != 0)
                    {
                        throw new RecorderResourceBusyException();
                    }
                }

                owner = requestedOwner;
                closedOwner = 0;
                bootId = requestedBootId;
                activeRecordId = 0;
                activeBufferId = 0;
                activeState = ReferenceBankState.Configured;
                IsConfigured = true;
                for (var index = 0; index < banks.Length; index++)
                {
                    banks[index].Reset(ReferenceBankState.Configured);
                }
            }

            internal ReferenceLease Start(
                uint requestedOwner,
                uint requestedBootId)
            {
                ValidateConfiguration(requestedOwner, requestedBootId);
                var selected = -1;
                for (var index = 0; index < banks.Length; index++)
                {
                    if (banks[index].State == ReferenceBankState.Armed
                        || banks[index].State == ReferenceBankState.Recording)
                    {
                        throw new RecorderResourceBusyException();
                    }

                    if (selected < 0
                        && banks[index].State == ReferenceBankState.Configured
                        && banks[index].RecordId == 0)
                    {
                        selected = index;
                    }
                }

                if (selected < 0)
                {
                    throw new RecorderResourceBusyException();
                }

                BeginGeneration();
                var bank = banks[selected];
                activeRecordId = nextRecordId++;
                activeBufferId = (uint)selected;
                activeState = ReferenceBankState.Armed;
                bank.RecordId = activeRecordId;
                bank.ConfigId = ConfigId;
                bank.ConfigRevision = ConfigRevision;
                bank.MapRevision = MapRevision;
                bank.Owner = owner;
                bank.ClosedOwner = 0;
                bank.BootId = bootId;
                bank.Header = null;
                bank.Data = null;
                bank.State = ReferenceBankState.Armed;
                EndGeneration();
                return new ReferenceLease(activeRecordId, activeBufferId);
            }

            internal ReferenceRtEntry CaptureRtEntry()
            {
                return new ReferenceRtEntry
                {
                    Generation = generation,
                    State = activeState,
                    RecordId = activeRecordId,
                    BufferId = activeBufferId
                };
            }

            internal bool TryPublish(ReferenceRtEntry entry)
            {
                if (entry == null
                    || (entry.Generation & 1u) != 0
                    || entry.Generation != generation
                    || (entry.State != ReferenceBankState.Armed
                        && entry.State != ReferenceBankState.Recording)
                    || entry.RecordId != activeRecordId
                    || entry.BufferId != activeBufferId
                    || entry.BufferId >= (uint)banks.Length)
                {
                    return false;
                }

                var bank = banks[entry.BufferId];
                if (bank.State != ReferenceBankState.Armed
                    && bank.State != ReferenceBankState.Recording)
                {
                    return false;
                }

                bank.State = activeState;
                return true;
            }

            internal void Complete(
                ReferenceLease lease,
                byte[] header,
                byte[] data)
            {
                var bank = GetExact(lease);
                if (lease.RecordId != activeRecordId
                    || lease.BufferId != activeBufferId
                    || bank.State != ReferenceBankState.Armed)
                {
                    throw new InvalidOperationException("not active");
                }

                activeState = ReferenceBankState.Recording;
                bank.State = ReferenceBankState.Recording;
                bank.Header = (byte[])header.Clone();
                bank.Data = (byte[])data.Clone();
                activeState = ReferenceBankState.Ready;
                bank.State = ReferenceBankState.Ready;
            }

            internal ReferenceSnapshot Read(
                ReferenceLease lease,
                uint requestedBootId)
            {
                var bank = GetExact(lease);
                if (requestedBootId != bootId
                    || requestedBootId != bank.BootId
                    || (bank.State != ReferenceBankState.Ready
                        && bank.State != ReferenceBankState.Uploading))
                {
                    throw new InvalidOperationException("not readable");
                }

                bank.State = ReferenceBankState.Uploading;
                return new ReferenceSnapshot(
                    (byte[])bank.Header.Clone(),
                    (byte[])bank.Data.Clone());
            }

            internal void NotifySessionClosed(uint sessionOwner)
            {
                if (sessionOwner != owner || sessionOwner == 0)
                {
                    return;
                }

                closedOwner = sessionOwner;
                for (var index = 0; index < banks.Length; index++)
                {
                    if (banks[index].RecordId != 0
                        && banks[index].Owner == sessionOwner)
                    {
                        banks[index].ClosedOwner = sessionOwner;
                    }
                }
            }

            internal void Adopt(
                uint recordId,
                uint bufferId,
                uint newOwner,
                uint requestedBootId)
            {
                if (recordId == 0 || bufferId >= (uint)banks.Length)
                {
                    throw new InvalidOperationException(
                        "Double mode requires exact identity");
                }

                var target = banks[bufferId];
                ValidateOccupied(target, recordId, requestedBootId);
                if (target.Owner == newOwner && target.ClosedOwner == 0)
                {
                    return;
                }

                var oldOwner = target.Owner;
                if (oldOwner == 0
                    || target.ClosedOwner != oldOwner
                    || owner != oldOwner
                    || closedOwner != oldOwner)
                {
                    throw new InvalidOperationException("not orphaned");
                }

                for (var index = 0; index < banks.Length; index++)
                {
                    var bank = banks[index];
                    if (bank.RecordId == 0)
                    {
                        continue;
                    }

                    if (bank.Owner != oldOwner
                        || bank.ClosedOwner != oldOwner
                        || bank.BootId != target.BootId
                        || bank.ConfigId != target.ConfigId
                        || bank.ConfigRevision != target.ConfigRevision
                        || bank.MapRevision != target.MapRevision
                        || (int)bank.State < (int)ReferenceBankState.Armed
                        || (int)bank.State > (int)ReferenceBankState.Uploading)
                    {
                        throw new InvalidOperationException(
                            "inconsistent occupied bank");
                    }
                }

                owner = newOwner;
                for (var index = 0; index < banks.Length; index++)
                {
                    if (banks[index].RecordId != 0)
                    {
                        banks[index].Owner = newOwner;
                        banks[index].ClosedOwner = 0;
                    }
                }

                closedOwner = 0;
            }

            internal void Release(
                ReferenceLease lease,
                uint requestedOwner,
                uint requestedBootId)
            {
                var bank = GetExact(lease);
                if (requestedOwner != owner
                    || requestedOwner != bank.Owner
                    || requestedBootId != bootId
                    || requestedBootId != bank.BootId
                    || (bank.State != ReferenceBankState.Ready
                        && bank.State != ReferenceBankState.Uploading))
                {
                    throw new InvalidOperationException("release rejected");
                }

                var current = activeRecordId == lease.RecordId
                    && activeBufferId == lease.BufferId;
                if (current)
                {
                    BeginGeneration();
                }

                bank.Reset(ReferenceBankState.Configured);
                if (current)
                {
                    activeRecordId = 0;
                    activeBufferId = 0;
                    activeState = ReferenceBankState.Configured;
                    EndGeneration();
                }
            }

            internal void ReleaseConfiguration(
                uint requestedOwner,
                uint requestedBootId)
            {
                ValidateConfiguration(requestedOwner, requestedBootId);
                for (var index = 0; index < banks.Length; index++)
                {
                    if (banks[index].RecordId != 0
                        || (int)banks[index].State
                            > (int)ReferenceBankState.Configured)
                    {
                        throw new RecorderResourceBusyException();
                    }
                }

                BeginGeneration();
                for (var index = 0; index < banks.Length; index++)
                {
                    banks[index].Reset(ReferenceBankState.Empty);
                }

                activeRecordId = 0;
                activeBufferId = 0;
                activeState = ReferenceBankState.Empty;
                owner = 0;
                closedOwner = 0;
                bootId = 0;
                IsConfigured = false;
                EndGeneration();
            }

            internal ReferenceBankState GetState(uint bufferId)
            {
                return banks[bufferId].State;
            }

            internal uint GetOwner(uint bufferId)
            {
                return banks[bufferId].Owner;
            }

            internal void CorruptMapForTest(uint bufferId)
            {
                banks[bufferId].MapRevision++;
            }

            private ReferenceBank GetExact(ReferenceLease lease)
            {
                if (lease == null || lease.BufferId >= (uint)banks.Length)
                {
                    throw new InvalidOperationException("invalid identity");
                }

                var bank = banks[lease.BufferId];
                if (bank.RecordId != lease.RecordId)
                {
                    throw new InvalidOperationException("identity mismatch");
                }

                return bank;
            }

            private void ValidateConfiguration(
                uint requestedOwner,
                uint requestedBootId)
            {
                if (!IsConfigured
                    || requestedOwner != owner
                    || closedOwner != 0
                    || requestedBootId != bootId)
                {
                    throw new InvalidOperationException(
                        "configuration lease mismatch");
                }
            }

            private void ValidateOccupied(
                ReferenceBank bank,
                uint recordId,
                uint requestedBootId)
            {
                if (bank.RecordId != recordId
                    || bank.ConfigId != ConfigId
                    || bank.ConfigRevision != ConfigRevision
                    || bank.MapRevision != MapRevision
                    || requestedBootId != bootId
                    || requestedBootId != bank.BootId
                    || (int)bank.State < (int)ReferenceBankState.Armed
                    || (int)bank.State > (int)ReferenceBankState.Uploading)
                {
                    throw new InvalidOperationException(
                        "occupied identity mismatch");
                }
            }

            private void BeginGeneration()
            {
                generation++;
                if ((generation & 1u) == 0)
                {
                    generation++;
                }
            }

            private void EndGeneration()
            {
                generation++;
                if ((generation & 1u) != 0)
                {
                    generation++;
                }
            }
        }

        private sealed class FakeState
        {
            internal readonly object OwnerToken = new object();
            internal readonly object SessionToken = new object();
            internal readonly Exception BusyException =
                new InvalidOperationException("exact ResourceBusy");
            internal readonly Exception ConfirmedNotAppliedReleaseError =
                new InvalidOperationException("confirmed not applied");
            internal readonly List<string> Calls = new List<string>();
            internal readonly List<string> ReleaseCalls =
                new List<string>();
            internal bool ForeignConfiguration;
            internal bool MutateBankAReread;
            internal bool ReturnUnexpectedThird;
            internal CancellationTokenSource CancelAfterFirstDownload;
            internal int RecoveryCount;
            internal RecorderDoubleBankRecoveryScope RecoveryScope;
            internal Exception RecoveryError;
            internal RecorderDoubleBankQualificationOperations Operations;
            internal CancellationTokenSource CancelDuringBankRelease;
            internal CancellationTokenSource CancelDuringConfigurationRelease;
            internal Exception ConfigureError;
            internal Exception ReleaseBankError;
            internal Exception ReleaseConfigurationError;
            internal int StartErrorAt;
            internal Exception StartError;
            internal uint ReturnedConfigId = 11;
            internal int RecoveryArmCount;
            internal int RecoveryCheckpointCount;
            internal Guid RecoveryToken;
            internal Exception RecoveryArmError;
            internal int RecoveryCheckpointErrorAt;
            internal Exception RecoveryCheckpointError;
            private int startCount;
            private int bankADownloadCount;

            internal RecorderDoubleBankQualificationOperations
                CreateOperations()
            {
                if (Operations != null)
                {
                    return Operations;
                }

                Operations = new RecorderDoubleBankQualificationOperations
                {
                    ArmRecoveryBeforeConfigureAsync = scope =>
                    {
                        RecoveryArmCount++;
                        RecoveryToken = Guid.NewGuid();
                        scope.BindRecoveryToken(RecoveryToken);
                        return RecoveryArmError == null
                            ? Task.CompletedTask
                            : Task.FromException(RecoveryArmError);
                    },
                    PersistRecoveryCheckpointAsync = scope =>
                    {
                        RecoveryCheckpointCount++;
                        return RecoveryCheckpointErrorAt
                                    != RecoveryCheckpointCount
                                || RecoveryCheckpointError == null
                            ? Task.CompletedTask
                            : Task.FromException(
                                RecoveryCheckpointError);
                    },
                    ConfigureAsync = (configuration, recoveryToken) =>
                    {
                        Calls.Add("Configure");
                        if (recoveryToken == Guid.Empty
                            || recoveryToken != RecoveryToken)
                        {
                            throw new InvalidOperationException(
                                "Configure did not receive the exact armed recovery token.");
                        }

                        if (ConfigureError != null)
                        {
                            return Task.FromException<
                                RecorderDoubleBankConfigurationLease>(
                                    ConfigureError);
                        }

                        return Task.FromResult(
                            new RecorderDoubleBankConfigurationLease(
                                new object(),
                                0x12345678u,
                                ReturnedConfigId,
                                12,
                                ForeignConfiguration
                                    ? new object()
                                    : OwnerToken,
                                SessionToken,
                                false));
                    },
                    StartAsync = configuration =>
                    {
                        startCount++;
                        if (startCount == 1)
                        {
                            Calls.Add("StartA");
                            if (StartErrorAt == startCount)
                            {
                                return Task.FromException<
                                    RecorderDoubleBankCaptureLease>(
                                        StartError);
                            }

                            return Task.FromResult(
                                Capture(101, 0));
                        }

                        if (startCount == 2)
                        {
                            Calls.Add("StartB");
                            if (StartErrorAt == startCount)
                            {
                                return Task.FromException<
                                    RecorderDoubleBankCaptureLease>(
                                        StartError);
                            }

                            return Task.FromResult(
                                Capture(102, 1));
                        }

                        Calls.Add("StartThird");
                        if (StartErrorAt == startCount)
                        {
                            return Task.FromException<
                                RecorderDoubleBankCaptureLease>(StartError);
                        }

                        if (ReturnUnexpectedThird)
                        {
                            return Task.FromResult(
                                Capture(103, 0));
                        }

                        return Task.FromException<
                            RecorderDoubleBankCaptureLease>(BusyException);
                    },
                    WaitForFrozenAsync = capture =>
                    {
                        Calls.Add(
                            capture.BufferId == 0 ? "FreezeA" : "FreezeB");
                        return Task.FromResult(
                            new RecorderDoubleBankFrozenStatus(
                                capture,
                                true));
                    },
                    DownloadAsync = capture =>
                    {
                        if (capture.BufferId == 0)
                        {
                            bankADownloadCount++;
                            Calls.Add(
                                bankADownloadCount == 1
                                    ? "DownloadA1"
                                    : "RereadA");
                            var data = MutateBankAReread
                                    && bankADownloadCount > 1
                                ? new byte[] { 9, 2, 3, 4 }
                                : new byte[] { 1, 2, 3, 4 };
                            var evidence =
                                new RecorderDoubleBankCaptureEvidence(
                                    capture,
                                    new byte[] { 0xA0, 0x01 },
                                    data);
                            if (bankADownloadCount == 1
                                && CancelAfterFirstDownload != null)
                            {
                                CancelAfterFirstDownload.Cancel();
                            }

                            return Task.FromResult(evidence);
                        }

                        Calls.Add("DownloadB");
                        return Task.FromResult(
                            new RecorderDoubleBankCaptureEvidence(
                                capture,
                                new byte[] { 0xB0, 0x01 },
                                new byte[] { 5, 6, 7, 8 }));
                    },
                    IsExactResourceBusy = error =>
                        ReferenceEquals(error, BusyException),
                    IsReleaseConfirmedNotApplied = error =>
                        ReferenceEquals(
                            error,
                            ConfirmedNotAppliedReleaseError),
                    RecoveryRequired = (scope, error) =>
                    {
                        RecoveryCount++;
                        RecoveryScope = scope;
                        RecoveryError = error;
                    },
                    ReleaseBankAsync = capture =>
                    {
                        ReleaseCalls.Add(
                            capture.BufferId == 1
                                ? "ReleaseB"
                                : "ReleaseA");
                        if (CancelDuringBankRelease != null)
                        {
                            CancelDuringBankRelease.Cancel();
                        }

                        return ReleaseBankError == null
                            ? Task.CompletedTask
                            : Task.FromException(ReleaseBankError);
                    },
                    ReleaseConfigurationAsync = configuration =>
                    {
                        ReleaseCalls.Add("ReleaseConfig");
                        if (CancelDuringConfigurationRelease != null)
                        {
                            CancelDuringConfigurationRelease.Cancel();
                        }

                        return ReleaseConfigurationError == null
                            ? Task.CompletedTask
                            : Task.FromException(
                                ReleaseConfigurationError);
                    }
                };
                return Operations;
            }

            private RecorderDoubleBankCaptureLease Capture(
                uint recordId,
                uint bufferId)
            {
                return new RecorderDoubleBankCaptureLease(
                    new object(),
                    0x12345678u,
                    11,
                    12,
                    recordId,
                    bufferId,
                    OwnerToken,
                    SessionToken,
                    false);
            }

        }
    }
}

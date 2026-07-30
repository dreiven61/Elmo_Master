using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class D5SdoDisconnectOrphanQualificationOrchestratorTests
    {
        private const uint DiagnosticsBootId = 0x89ABCDEFu;
        private const uint MapRevision = 0x957F101Eu;
        private static readonly byte[] ExpectedResult = { 8 };
        private static readonly LMCSdoRequest ProbeRequest =
            LMCSdoRequest.CreateRead(
                1,
                0x6061,
                0,
                LMCSignalValueType.Int8,
                1,
                60000);
        private static readonly LMCSdoRequest RecoveryRequest =
            LMCSdoRequest.CreateRead(
                1,
                0x6061,
                0,
                LMCSignalValueType.Int8,
                1,
                100);

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.D5DisconnectOrphan.RunningTransportLossIsApplicationRecoveryWithoutPlcWitness",
                RunningTransportLossIsApplicationRecoveryWithoutPlcWitness);
            tests.Add(
                "Qualification.D5DisconnectOrphan.QueuedTransportLossIsApplicationRecoveryOnly",
                QueuedTransportLossIsApplicationRecoveryOnly);
            tests.Add(
                "Qualification.D5DisconnectOrphan.TerminalBeforeLossDisarmsWithoutRecovery",
                TerminalBeforeLossDisarmsWithoutRecovery);
            tests.Add(
                "Qualification.D5DisconnectOrphan.CancelledExactTerminalPreservesAcceptedEvidence",
                CancelledExactTerminalPreservesAcceptedEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.MalformedTerminalPreservesAcceptedEvidence",
                MalformedTerminalPreservesAcceptedEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.CapabilityDriftPreservesWithoutRecoveryReplay",
                CapabilityDriftPreservesWithoutRecoveryReplay);
            tests.Add(
                "Qualification.D5DisconnectOrphan.BaseCycleDriftFromOldOwnerPreservesWithoutRecoveryReplay",
                BaseCycleDriftFromOldOwnerPreservesWithoutRecoveryReplay);
            tests.Add(
                "Qualification.D5DisconnectOrphan.FinalCapabilityDriftPreservesRecoveredEvidence",
                FinalCapabilityDriftPreservesRecoveredEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.FinalPayloadContractDriftPreservesRecoveredEvidence",
                FinalPayloadContractDriftPreservesRecoveredEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.RepeatedCapabilityObservationPreservesWithoutRecovery",
                RepeatedCapabilityObservationPreservesWithoutRecovery);
            tests.Add(
                "Qualification.D5DisconnectOrphan.ForeignCapabilityProvenancePreservesWithoutRecovery",
                ForeignCapabilityProvenancePreservesWithoutRecovery);
            tests.Add(
                "Qualification.D5DisconnectOrphan.ResourceBusyDrainRetriesThenRecovers",
                ResourceBusyDrainRetriesThenRecovers);
            tests.Add(
                "Qualification.D5DisconnectOrphan.ResourceBusyDrainExhaustionPreservesEvidence",
                ResourceBusyDrainExhaustionPreservesEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.ResourceBusyMonotonicDeadlinePreservesEvidence",
                ResourceBusyMonotonicDeadlinePreservesEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.SecondInitialRecoverySubmitSurvivesExpiredRetryDeadline",
                SecondInitialRecoverySubmitSurvivesExpiredRetryDeadline);
            tests.Add(
                "Qualification.D5DisconnectOrphan.ResourceBusyCancellationWinsRetryDeadline",
                ResourceBusyCancellationWinsRetryDeadline);
            tests.Add(
                "Qualification.D5DisconnectOrphan.UncertainRecoverySubmitDoesNotRetry",
                UncertainRecoverySubmitDoesNotRetry);
            tests.Add(
                "Qualification.D5DisconnectOrphan.UncertainOldSubmitPreservesUnknownEvidence",
                UncertainOldSubmitPreservesUnknownEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.AcceptedMetadataMismatchPreservesKnownTicket",
                AcceptedMetadataMismatchPreservesKnownTicket);
            tests.Add(
                "Qualification.D5DisconnectOrphan.OldSubmitAbaPreservesAcceptedEvidence",
                OldSubmitAbaPreservesAcceptedEvidence);
            tests.Add(
                "Qualification.D5DisconnectOrphan.CancellationAfterRunningPreservesWithoutReconnect",
                CancellationAfterRunningPreservesWithoutReconnect);
            tests.Add(
                "Qualification.D5DisconnectOrphan.AbaMutationPreservesWithoutClear",
                AbaMutationPreservesWithoutClear);
            tests.Add(
                "Qualification.D5DisconnectOrphan.PassLogFailurePreservesWithoutClear",
                PassLogFailurePreservesWithoutClear);
            tests.Add(
                "Qualification.D5DisconnectOrphan.ProofCommitOwnerRacePreservesWithoutPassLog",
                ProofCommitOwnerRacePreservesWithoutPassLog);
            tests.Add(
                "Qualification.D5DisconnectOrphan.CancellationAfterSecondTerminalPreservesWithoutPassLog",
                CancellationAfterSecondTerminalPreservesWithoutPassLog);
            tests.Add(
                "Qualification.D5DisconnectOrphan.DistinctNewOwnerIsRequired",
                DistinctNewOwnerIsRequired);
            tests.Add(
                "Qualification.D5DisconnectOrphan.ExactRecoveryResultIsRequired",
                ExactRecoveryResultIsRequired);
            tests.Add(
                "Qualification.D5DisconnectOrphan.PreCanceledIsZeroIo",
                PreCanceledIsZeroIo);
        }

        private static void
            RunningTransportLossIsApplicationRecoveryWithoutPlcWitness()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner);

                var result = Run(harness);

                AssertEx.Equal(
                    D5SdoDisconnectOrphanQualificationDisposition
                        .ApplicationRecoveryOnly,
                    result.Disposition);
                AssertEx.True(result.NewConnectionRecovery);
                AssertEx.False(result.OrphanQualified);
                AssertEx.True(result.RecoveryScope.HasExactRunningWitness);
                AssertEx.Equal("COMPLETE", result.RecoveryScope.Stage);
                AssertEx.Equal(1, harness.OldSubmitCount);
                AssertEx.Equal(1, harness.ObserveCount);
                AssertEx.Equal(1, harness.OpenCount);
                AssertEx.Equal(3, harness.CapabilitiesCount);
                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.Equal(1, harness.PassLogCount);
                AssertEx.Equal(0, harness.RecoveryRequiredCount);
                AssertEx.Equal(0, harness.Ledger.Count);
                AssertEx.True(
                    !ReferenceEquals(
                        result.RecoveryScope.FirstRecoveryTicket,
                        result.RecoveryScope.SecondRecoveryTicket));
            }
        }

        private static void QueuedTransportLossIsApplicationRecoveryOnly()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    ObservedState = LMCOperationState.Queued
                };

                var result = Run(harness);

                AssertEx.Equal(
                    D5SdoDisconnectOrphanQualificationDisposition
                        .ApplicationRecoveryOnly,
                    result.Disposition);
                AssertEx.True(result.NewConnectionRecovery);
                AssertEx.False(result.OrphanQualified);
                AssertEx.False(result.RecoveryScope.HasExactRunningWitness);
                AssertEx.Equal(1, harness.PassLogCount);
                AssertEx.Equal(0, harness.RecoveryRequiredCount);
                AssertEx.Equal(0, harness.Ledger.Count);
            }
        }

        private static void TerminalBeforeLossDisarmsWithoutRecovery()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner);
                harness.ObservationStatusOverride =
                    CompletedStatus(harness.OldTicket, ExpectedResult);
                harness.DisconnectOldDuringObservation = false;
                harness.TransportLossObserved = false;

                var result = Run(harness);

                AssertEx.Equal(
                    D5SdoDisconnectOrphanQualificationDisposition
                        .TerminalBeforeTransportLoss,
                    result.Disposition);
                AssertEx.False(result.NewConnectionRecovery);
                AssertEx.False(result.OrphanQualified);
                AssertEx.Equal(
                    "TERMINAL_BEFORE_EXTERNAL_LOSS",
                    result.RecoveryScope.Stage);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.CapabilitiesCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(0, harness.RecoveryRequiredCount);
                AssertEx.Equal(0, harness.Ledger.Count);
            }
        }

        private static void MalformedTerminalPreservesAcceptedEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner);
                harness.ObservationStatusOverride = Status(
                    harness.OldTicket,
                    LMCOperationState.Completed,
                    LMCOperationOutcome.Success,
                    LMCSignalValueType.Int8,
                    2,
                    new byte[] { 8, 9 },
                    harness.OldTicket.QueuedCycle + 10);

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(1, harness.Ledger.Count);
                AssertEx.Equal(
                    harness.OldTicket.TicketId,
                    harness.Ledger.CaptureSnapshot().Entries[0].TicketId);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
            }
        }

        private static void CancelledExactTerminalPreservesAcceptedEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(oldOwner, newOwner);
                harness.ObservationStatusOverride =
                    CompletedStatus(harness.OldTicket, ExpectedResult);
                harness.DisconnectOldDuringObservation = false;
                harness.TransportLossObserved = false;
                harness.CancelDuringObservation = cancellation;

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(harness, cancellation.Token));

                AssertEx.Equal(1, harness.Ledger.Count);
                AssertEx.Equal(
                    harness.OldTicket.TicketId,
                    harness.Ledger.CaptureSnapshot().Entries[0].TicketId);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
            }
        }

        private static void CapabilityDriftPreservesWithoutRecoveryReplay()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner);
                harness.SecondCapabilitiesOverride = Capabilities(
                    newOwner,
                    DiagnosticsBootId,
                    MapRevision + 1);

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(1, harness.OldSubmitCount);
                AssertEx.Equal(1, harness.OpenCount);
                AssertEx.Equal(2, harness.CapabilitiesCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            BaseCycleDriftFromOldOwnerPreservesWithoutRecoveryReplay()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    FirstCapabilitiesOverride = Capabilities(
                        newOwner,
                        DiagnosticsBootId,
                        MapRevision,
                        1,
                        2000),
                    SecondCapabilitiesOverride = Capabilities(
                        newOwner,
                        DiagnosticsBootId,
                        MapRevision,
                        2,
                        2000)
                };

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Contains("BaseCycleTimeUs", error.Message);
                AssertEx.Equal(2, harness.CapabilitiesCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            FinalCapabilityDriftPreservesRecoveredEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    FinalCapabilitiesOverride = Capabilities(
                        newOwner,
                        DiagnosticsBootId,
                        MapRevision + 1)
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(3, harness.CapabilitiesCount);
                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            FinalPayloadContractDriftPreservesRecoveredEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    FinalCapabilitiesOverride = Capabilities(
                        newOwner,
                        DiagnosticsBootId,
                        MapRevision,
                        3,
                        1000,
                        1319,
                        2040)
                };

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Contains("payload contract", error.Message);
                AssertEx.Equal(3, harness.CapabilitiesCount);
                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            RepeatedCapabilityObservationPreservesWithoutRecovery()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var repeated = Capabilities(
                    newOwner,
                    DiagnosticsBootId,
                    MapRevision,
                    1);
                var harness = new Harness(oldOwner, newOwner)
                {
                    FirstCapabilitiesOverride = repeated,
                    SecondCapabilitiesOverride = repeated
                };

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Contains("fresh, increasing", error.Message);
                AssertEx.Equal(2, harness.CapabilitiesCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            ForeignCapabilityProvenancePreservesWithoutRecovery()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    FirstCapabilitiesOverride = Capabilities(
                        oldOwner,
                        DiagnosticsBootId,
                        MapRevision,
                        1)
                };

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Contains("exact LMCConnection", error.Message);
                AssertEx.Equal(2, harness.CapabilitiesCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void ResourceBusyDrainRetriesThenRecovers()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    RecoveryResourceBusyFailuresRemaining = 2
                };

                var result = Run(harness);

                AssertEx.True(result.NewConnectionRecovery);
                AssertEx.False(result.OrphanQualified);
                AssertEx.Equal(4, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.DelayCount);
                AssertEx.Equal(
                    3,
                    result.RecoveryScope.FirstRecoverySubmitAttemptCount);
                AssertEx.Equal(
                    1,
                    result.RecoveryScope.SecondRecoverySubmitAttemptCount);
                AssertEx.Equal(
                    2,
                    result.RecoveryScope
                        .RecoveryResourceBusyRejectionCount);
                AssertEx.Equal(1, harness.PassLogCount);
                AssertEx.Equal(0, harness.Ledger.Count);
            }
        }

        private static void
            ResourceBusyDrainExhaustionPreservesEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    RecoveryResourceBusyFailuresRemaining =
                        D5SdoDisconnectOrphanQualificationOrchestrator
                            .MaximumRecoverySubmitAttempts
                };

                var error = AssertEx.Throws<TimeoutException>(
                    () => Run(harness));

                AssertEx.Contains("bounded recovery Submit attempts", error.Message);
                var attemptLimit =
                    harness.RecoveryScope.RecoverySubmitAttemptLimit;
                AssertEx.True(
                    attemptLimit > 600,
                    "A 60000-cycle probe at 1 ms must receive a retry window longer than the old fixed 15-second budget.");
                AssertEx.Equal(
                    attemptLimit,
                    harness.RecoverySubmitCount);
                AssertEx.Equal(
                    attemptLimit - 1,
                    harness.DelayCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            ResourceBusyMonotonicDeadlinePreservesEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    RecoveryResourceBusyFailuresRemaining = int.MaxValue,
                    RecoverySubmitElapsedIncrementMilliseconds = 30000
                };

                var error = AssertEx.Throws<TimeoutException>(
                    () => Run(harness));

                AssertEx.Contains("monotonic retry-admission deadline", error.Message);
                AssertEx.Equal(3, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.DelayCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            SecondInitialRecoverySubmitSurvivesExpiredRetryDeadline()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    RecoveryWaitElapsedIncrementMilliseconds = 70000
                };

                var result = Run(harness);

                AssertEx.True(result.NewConnectionRecovery);
                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.Equal(0, harness.DelayCount);
                AssertEx.Equal(1, harness.PassLogCount);
                AssertEx.Equal(0, harness.Ledger.Count);
            }
        }

        private static void ResourceBusyCancellationWinsRetryDeadline()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    RecoveryResourceBusyFailuresRemaining = 1,
                    RecoverySubmitElapsedIncrementMilliseconds = 70000,
                    CancelAfterRecoveryResourceBusy = cancellation
                };

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(harness, cancellation.Token));

                AssertEx.Equal(1, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.DelayCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void UncertainRecoverySubmitDoesNotRetry()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    RecoverySubmitError = RecoverySubmissionFailure(
                        LMCSdoSubmissionOutcome.OutcomeUncertain)
                };

                AssertEx.Throws<LMCDiagnosticsCommandException>(
                    () => Run(harness));

                AssertEx.Equal(1, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.DelayCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(2, harness.Ledger.Count);
                AssertEx.True(
                    harness.RecoveryScope
                        .FirstRecoverySubmissionOutcomeUncertain);
            }
        }

        private static void UncertainOldSubmitPreservesUnknownEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner);
                harness.OldSubmitError = SubmissionFailure(
                    LMCSdoSubmissionOutcome.OutcomeUncertain,
                    null);

                AssertEx.Throws<InvalidDataException>(
                    () => Run(harness));

                AssertEx.Equal(1, harness.Ledger.Count);
                AssertEx.Equal(
                    0u,
                    harness.Ledger.CaptureSnapshot().Entries[0].TicketId);
                AssertEx.True(
                    harness.RecoveryScope.OldSubmissionOutcomeUncertain);
                AssertEx.Equal(0, harness.ObserveCount);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
            }
        }

        private static void AcceptedMetadataMismatchPreservesKnownTicket()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner);
                harness.OldTicketOverride = Ticket(
                    oldOwner,
                    ProbeRequest,
                    109,
                    DiagnosticsBootId + 1,
                    MapRevision);

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                var evidence = harness.Ledger.CaptureSnapshot().Entries[0];
                AssertEx.Equal(1, harness.Ledger.Count);
                AssertEx.Equal(109u, evidence.TicketId);
                AssertEx.Equal(DiagnosticsBootId + 1, evidence.DiagnosticsBootId);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(0, harness.ObserveCount);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
            }
        }

        private static void CancellationAfterRunningPreservesWithoutReconnect()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    CancelDuringObservation = cancellation
                };

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(harness, cancellation.Token));

                AssertEx.Equal(1, harness.ObserveCount);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void OldSubmitAbaPreservesAcceptedEvidence()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    MutateLedgerDuringOldSubmit = true
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(1, harness.Ledger.Count);
                AssertEx.Equal(
                    harness.OldTicket.TicketId,
                    harness.Ledger.CaptureSnapshot().Entries[0].TicketId);
                AssertEx.Equal(0, harness.ObserveCount);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
            }
        }

        private static void AbaMutationPreservesWithoutClear()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    MutateLedgerAfterSecondTerminal = true
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void PassLogFailurePreservesWithoutClear()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    PassLogError = new IOException("PASS log unavailable")
                };

                AssertEx.Throws<IOException>(() => Run(harness));

                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(1, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void
            CancellationAfterSecondTerminalPreservesWithoutPassLog()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    CancelAfterSecondTerminal = cancellation
                };

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(harness, cancellation.Token));

                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.True(harness.Ledger.Count >= 1);
            }
        }

        private static void ProofCommitOwnerRacePreservesWithoutPassLog()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    ReconnectOldAtProofCommit = true
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.True(harness.OldConnected);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void DistinctNewOwnerIsRequired()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    OpenReturnsOldOwner = true
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(1, harness.OpenCount);
                AssertEx.Equal(0, harness.CapabilitiesCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(1, harness.Ledger.Count);
            }
        }

        private static void ExactRecoveryResultIsRequired()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            {
                var harness = new Harness(oldOwner, newOwner)
                {
                    SecondRecoveryStatusOverride = CompletedStatus(
                        Ticket(
                            newOwner,
                            RecoveryRequest,
                            202,
                            DiagnosticsBootId,
                            MapRevision),
                        new byte[] { 9 })
                };

                AssertEx.Throws<InvalidOperationException>(
                    () => Run(harness));

                AssertEx.Equal(2, harness.RecoverySubmitCount);
                AssertEx.Equal(2, harness.RecoveryWaitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.True(harness.Ledger.Count >= 1);
            }
        }

        private static void PreCanceledIsZeroIo()
        {
            using (var oldOwner = new LMCConnection())
            using (var newOwner = new LMCConnection())
            using (var cancellation = new CancellationTokenSource())
            {
                var harness = new Harness(oldOwner, newOwner);
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => Run(harness, cancellation.Token));

                AssertEx.Equal(0, harness.OldSubmitCount);
                AssertEx.Equal(0, harness.ObserveCount);
                AssertEx.Equal(0, harness.OpenCount);
                AssertEx.Equal(0, harness.RecoverySubmitCount);
                AssertEx.Equal(0, harness.PassLogCount);
                AssertEx.Equal(1, harness.RecoveryRequiredCount);
                AssertEx.Equal(0, harness.Ledger.Count);
            }
        }

        private static D5SdoDisconnectOrphanQualificationResult Run(
            Harness harness,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return D5SdoDisconnectOrphanQualificationOrchestrator.RunAsync(
                    harness.Request,
                    harness.Ledger,
                    harness.CreateOperations(),
                    cancellationToken)
                .GetAwaiter()
                .GetResult();
        }

        private static LMCDiagnosticCapability RequiredCapabilities
        {
            get
            {
                return LMCDiagnosticCapability.SDORead
                    | LMCDiagnosticCapability.SDOReadGeneralInline;
            }
        }

        private static LMCDiagnosticCapabilities Capabilities(
            LMCConnection connection,
            uint diagnosticsBootId = DiagnosticsBootId,
            uint mapRevision = MapRevision,
            long observationSequence = 1,
            uint baseCycleTimeUs = 1000,
            ushort maxRequestPayloadBytes = 1320,
            ushort maxResponsePayloadBytes = 2040)
        {
            return new LMCDiagnosticCapabilities(
                Response(LMCDiagnosticsDetailCode.None),
                connection.SessionGeneration,
                1,
                (uint)RequiredCapabilities,
                mapRevision,
                0,
                0,
                0,
                0,
                0,
                baseCycleTimeUs,
                maxRequestPayloadBytes,
                maxResponsePayloadBytes,
                1280,
                80,
                16,
                0,
                4,
                diagnosticsBootId).BindProvenance(
                    connection.Diagnostics,
                    connection.SessionGeneration,
                    observationSequence);
        }

        private static LMCOperationTicket Ticket(
            LMCConnection connection,
            LMCSdoRequest request,
            uint ticketId,
            uint diagnosticsBootId,
            uint mapRevision)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDORead,
                ticketId + 10,
                diagnosticsBootId,
                mapRevision,
                connection.SessionGeneration,
                connection.Diagnostics,
                true,
                request.DataLength,
                request.ValueType,
                false,
                0,
                request);
        }

        private static LMCOperationStatus PendingStatus(
            LMCOperationTicket ticket,
            LMCOperationState state)
        {
            return Status(
                ticket,
                state,
                LMCOperationOutcome.NoneOrPending,
                LMCSignalValueType.Invalid,
                0,
                new byte[0],
                0);
        }

        private static LMCOperationStatus CompletedStatus(
            LMCOperationTicket ticket,
            byte[] resultData)
        {
            return Status(
                ticket,
                LMCOperationState.Completed,
                LMCOperationOutcome.Success,
                LMCSignalValueType.Int8,
                1,
                resultData,
                ticket.QueuedCycle + 10);
        }

        private static LMCOperationStatus Status(
            LMCOperationTicket ticket,
            LMCOperationState state,
            LMCOperationOutcome outcome,
            LMCSignalValueType resultValueType,
            uint resultLength,
            byte[] resultData,
            uint completionCycle)
        {
            return new LMCOperationStatus(
                Response(LMCDiagnosticsDetailCode.None),
                ticket.TicketId,
                ticket.OperationKind,
                state,
                ticket.QueuedCycle,
                completionCycle,
                outcome,
                0,
                0,
                resultLength,
                resultValueType,
                resultData,
                ticket.DiagnosticsBootId);
        }

        private static InvalidDataException SubmissionFailure(
            LMCSdoSubmissionOutcome outcome,
            LMCOperationTicket ticket)
        {
            var error = new InvalidDataException("test submit failure");
            LMCSdoSubmissionFailureContext.Attach(
                error,
                new LMCSdoSubmissionFailureContext(
                    ProbeRequest,
                    LMCSdoSubmissionPhase.Submission,
                    outcome,
                    DiagnosticsBootId,
                    MapRevision,
                    ticket));
            return error;
        }

        private static LMCDiagnosticsCommandException
            ExactRecoveryResourceBusy()
        {
            var error = new LMCDiagnosticsCommandException(
                "exact recovery ResourceBusy rejection",
                Response(LMCDiagnosticsDetailCode.ResourceBusy));
            LMCSdoSubmissionFailureContext.Attach(
                error,
                new LMCSdoSubmissionFailureContext(
                    RecoveryRequest,
                    LMCSdoSubmissionPhase.Submission,
                    LMCSdoSubmissionOutcome.Rejected,
                    DiagnosticsBootId,
                    MapRevision,
                    null));
            return error;
        }

        private static LMCDiagnosticsCommandException
            RecoverySubmissionFailure(LMCSdoSubmissionOutcome outcome)
        {
            var error = new LMCDiagnosticsCommandException(
                "test recovery submission failure",
                Response(LMCDiagnosticsDetailCode.InternalError));
            LMCSdoSubmissionFailureContext.Attach(
                error,
                new LMCSdoSubmissionFailureContext(
                    RecoveryRequest,
                    LMCSdoSubmissionPhase.Submission,
                    outcome,
                    DiagnosticsBootId,
                    MapRevision,
                    null));
            return error;
        }

        private static LMCDiagnosticsResponse Response(
            LMCDiagnosticsDetailCode detail)
        {
            return new LMCDiagnosticsResponse(
                new LMC_Response
                {
                    IsFrameValid = true,
                    HeaderStatus = 0
                },
                1,
                LMCDiagnosticsResponseFlags.None,
                detail == LMCDiagnosticsDetailCode.None
                    ? (ushort)0
                    : (ushort)1,
                detail == LMCDiagnosticsDetailCode.None
                    ? (short)0
                    : (short)-32000,
                1,
                (uint)detail);
        }

        private sealed class Harness
        {
            internal Harness(
                LMCConnection oldOwner,
                LMCConnection newOwner)
            {
                OldOwner = oldOwner;
                NewOwner = newOwner;
                Ledger = new D5SdoQuarantineLedger();
                Request = new D5SdoDisconnectOrphanQualificationRequest(
                    oldOwner,
                    Capabilities(oldOwner),
                    ProbeRequest,
                    RecoveryRequest,
                    ExpectedResult);
                OldTicket = Ticket(
                    oldOwner,
                    ProbeRequest,
                    101,
                    DiagnosticsBootId,
                    MapRevision);
                FirstRecoveryTicket = Ticket(
                    newOwner,
                    RecoveryRequest,
                    201,
                    DiagnosticsBootId,
                    MapRevision);
                SecondRecoveryTicket = Ticket(
                    newOwner,
                    RecoveryRequest,
                    202,
                    DiagnosticsBootId,
                    MapRevision);
                OldConnected = true;
                NewConnected = true;
                DisconnectOldDuringObservation = true;
                TransportLossObserved = true;
                ObservedState = LMCOperationState.Running;
                Events = new List<string>();
            }

            internal LMCConnection OldOwner { get; private set; }
            internal LMCConnection NewOwner { get; private set; }
            internal D5SdoQuarantineLedger Ledger { get; private set; }
            internal D5SdoDisconnectOrphanQualificationRequest Request
            {
                get;
                private set;
            }
            internal LMCOperationTicket OldTicket { get; private set; }
            internal LMCOperationTicket FirstRecoveryTicket
            {
                get;
                private set;
            }
            internal LMCOperationTicket SecondRecoveryTicket
            {
                get;
                private set;
            }
            internal List<string> Events { get; private set; }

            internal bool OldConnected { get; set; }
            internal bool NewConnected { get; set; }
            internal bool DisconnectOldDuringObservation { get; set; }
            internal bool TransportLossObserved { get; set; }
            internal bool OpenReturnsOldOwner { get; set; }
            internal bool MutateLedgerDuringOldSubmit { get; set; }
            internal bool MutateLedgerAfterSecondTerminal { get; set; }
            internal bool ReconnectOldAtProofCommit { get; set; }
            internal LMCOperationState ObservedState { get; set; }
            internal Exception OldSubmitError { get; set; }
            internal LMCOperationTicket OldTicketOverride { get; set; }
            internal LMCOperationStatus ObservationStatusOverride { get; set; }
            internal LMCDiagnosticCapabilities FirstCapabilitiesOverride
            {
                get;
                set;
            }
            internal LMCDiagnosticCapabilities SecondCapabilitiesOverride
            {
                get;
                set;
            }
            internal LMCDiagnosticCapabilities FinalCapabilitiesOverride
            {
                get;
                set;
            }
            internal LMCOperationStatus FirstRecoveryStatusOverride
            {
                get;
                set;
            }
            internal LMCOperationStatus SecondRecoveryStatusOverride
            {
                get;
                set;
            }
            internal Exception PassLogError { get; set; }
            internal int RecoveryResourceBusyFailuresRemaining { get; set; }
            internal int RecoverySubmitElapsedIncrementMilliseconds
            {
                get;
                set;
            }
            internal int RecoveryWaitElapsedIncrementMilliseconds
            {
                get;
                set;
            }
            internal Exception RecoverySubmitError { get; set; }
            internal CancellationTokenSource CancelDuringObservation
            {
                get;
                set;
            }
            internal CancellationTokenSource CancelAfterSecondTerminal
            {
                get;
                set;
            }
            internal CancellationTokenSource CancelAfterRecoveryResourceBusy
            {
                get;
                set;
            }

            internal int OldSubmitCount { get; private set; }
            internal int ObserveCount { get; private set; }
            internal int OpenCount { get; private set; }
            internal int CapabilitiesCount { get; private set; }
            internal int RecoverySubmitCount { get; private set; }
            internal int RecoveryAcceptedCount { get; private set; }
            internal int RecoveryWaitCount { get; private set; }
            internal int DelayCount { get; private set; }
            internal int PassLogCount { get; private set; }
            internal int RecoveryRequiredCount { get; private set; }
            internal long MonotonicMilliseconds { get; private set; }
            internal D5SdoDisconnectOrphanRecoveryScope RecoveryScope
            {
                get;
                private set;
            }
            private bool proofCommitOwnerRaceArmed;
            private int oldChecksAfterProofCommitRace;

            internal D5SdoDisconnectOrphanQualificationOperations
                CreateOperations()
            {
                return new D5SdoDisconnectOrphanQualificationOperations
                {
                    IsConnected = connection =>
                    {
                        if (ReferenceEquals(connection, OldOwner))
                        {
                            if (proofCommitOwnerRaceArmed)
                            {
                                oldChecksAfterProofCommitRace++;
                                if (oldChecksAfterProofCommitRace >= 2)
                                {
                                    OldConnected = true;
                                }
                            }

                            return OldConnected;
                        }

                        return ReferenceEquals(connection, NewOwner)
                            && NewConnected;
                    },
                    SubmitOldReadAsync =
                        (connection, request, cancellationToken) =>
                        {
                            OldSubmitCount++;
                            Events.Add("SubmitOld");
                            if (!ReferenceEquals(connection, OldOwner)
                                || !ReferenceEquals(request, ProbeRequest))
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    new InvalidOperationException(
                                        "Old submit did not receive the exact owner/request."));
                            }

                            if (OldSubmitError != null)
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    OldSubmitError);
                            }

                            if (MutateLedgerDuringOldSubmit)
                            {
                                var aba = Ledger.ArmUnknown(
                                    LMCOperationKind.SDORead,
                                    ProbeRequest,
                                    OldOwner,
                                    DiagnosticsBootId,
                                    MapRevision,
                                    ProbeRequest.SlaveReference,
                                    ProbeRequest.TimeoutCycles,
                                    "test-old-submit-aba",
                                    "test-only old-submit transient mutation",
                                    "disconnect-orphan-test-old-submit-aba");
                                Ledger.Disarm(aba);
                            }

                            return Task.FromResult(
                                OldTicketOverride ?? OldTicket);
                        },
                    ObserveOwnerTransportLossAsync =
                        (ticket, cancellationToken) =>
                        {
                            ObserveCount++;
                            Events.Add("ObserveLoss");
                            if (!ReferenceEquals(
                                    ticket,
                                    OldTicketOverride ?? OldTicket))
                            {
                                return TaskFromException<
                                    D5SdoOwnerTransportLossObservation>(
                                        new InvalidOperationException(
                                            "Observer received a foreign old ticket."));
                            }

                            if (DisconnectOldDuringObservation)
                            {
                                OldConnected = false;
                            }

                            if (CancelDuringObservation != null)
                            {
                                CancelDuringObservation.Cancel();
                            }

                            var status = ObservationStatusOverride
                                ?? PendingStatus(ticket, ObservedState);
                            return Task.FromResult(
                                new D5SdoOwnerTransportLossObservation(
                                    status,
                                    TransportLossObserved));
                        },
                    OpenNewConnectionAsync = cancellationToken =>
                    {
                        OpenCount++;
                        Events.Add("OpenNew");
                        return Task.FromResult(
                            OpenReturnsOldOwner ? OldOwner : NewOwner);
                    },
                    ReadCapabilitiesAsync =
                        (connection, cancellationToken) =>
                        {
                            CapabilitiesCount++;
                            Events.Add("Capabilities" + CapabilitiesCount);
                            if (!ReferenceEquals(connection, NewOwner))
                            {
                                return TaskFromException<
                                    LMCDiagnosticCapabilities>(
                                        new InvalidOperationException(
                                            "Capabilities used a foreign owner."));
                            }

                            return Task.FromResult(
                                CapabilitiesCount == 1
                                    ? FirstCapabilitiesOverride
                                        ?? Capabilities(
                                            NewOwner,
                                            DiagnosticsBootId,
                                            MapRevision,
                                            1)
                                    : CapabilitiesCount == 2
                                        ? SecondCapabilitiesOverride
                                            ?? Capabilities(
                                                NewOwner,
                                                DiagnosticsBootId,
                                                MapRevision,
                                                2)
                                        : FinalCapabilitiesOverride
                                            ?? Capabilities(
                                                NewOwner,
                                                DiagnosticsBootId,
                                                MapRevision,
                                                3));
                        },
                    SubmitRecoveryReadAsync =
                        (connection, request, cancellationToken) =>
                        {
                            RecoverySubmitCount++;
                            MonotonicMilliseconds = checked(
                                MonotonicMilliseconds
                                + RecoverySubmitElapsedIncrementMilliseconds);
                            Events.Add(
                                "SubmitRecovery" + RecoverySubmitCount);
                            if (!ReferenceEquals(connection, NewOwner)
                                || !ReferenceEquals(
                                    request,
                                    RecoveryRequest))
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    new InvalidOperationException(
                                        "Recovery submit did not receive the exact owner/request."));
                            }

                            if (RecoveryResourceBusyFailuresRemaining > 0)
                            {
                                RecoveryResourceBusyFailuresRemaining--;
                                if (CancelAfterRecoveryResourceBusy != null)
                                {
                                    CancelAfterRecoveryResourceBusy.Cancel();
                                }
                                return TaskFromException<LMCOperationTicket>(
                                    ExactRecoveryResourceBusy());
                            }

                            if (RecoverySubmitError != null)
                            {
                                return TaskFromException<LMCOperationTicket>(
                                    RecoverySubmitError);
                            }

                            RecoveryAcceptedCount++;
                            if (RecoveryAcceptedCount == 1)
                            {
                                return Task.FromResult(FirstRecoveryTicket);
                            }

                            if (RecoveryAcceptedCount == 2)
                            {
                                return Task.FromResult(SecondRecoveryTicket);
                            }

                            return TaskFromException<LMCOperationTicket>(
                                new InvalidOperationException(
                                    "Recovery Read was replayed."));
                        },
                    WaitRecoveryTerminalAsync =
                        (connection, ticket, cancellationToken) =>
                        {
                            RecoveryWaitCount++;
                            MonotonicMilliseconds = checked(
                                MonotonicMilliseconds
                                + RecoveryWaitElapsedIncrementMilliseconds);
                            Events.Add("WaitRecovery" + RecoveryWaitCount);
                            if (!ReferenceEquals(connection, NewOwner))
                            {
                                return TaskFromException<LMCOperationStatus>(
                                    new InvalidOperationException(
                                        "Recovery wait used a foreign owner."));
                            }

                            LMCOperationStatus status;
                            if (ReferenceEquals(ticket, FirstRecoveryTicket))
                            {
                                status = FirstRecoveryStatusOverride
                                    ?? CompletedStatus(
                                        FirstRecoveryTicket,
                                        ExpectedResult);
                            }
                            else if (ReferenceEquals(
                                ticket,
                                SecondRecoveryTicket))
                            {
                                status = SecondRecoveryStatusOverride
                                    ?? CompletedStatus(
                                        SecondRecoveryTicket,
                                        ExpectedResult);
                            }
                            else
                            {
                                return TaskFromException<LMCOperationStatus>(
                                    new InvalidOperationException(
                                        "Recovery wait received a foreign ticket."));
                            }

                            if (RecoveryWaitCount == 2
                                && MutateLedgerAfterSecondTerminal)
                            {
                                var aba = Ledger.ArmUnknown(
                                    LMCOperationKind.SDORead,
                                    RecoveryRequest,
                                    NewOwner,
                                    DiagnosticsBootId,
                                    MapRevision,
                                    RecoveryRequest.SlaveReference,
                                    RecoveryRequest.TimeoutCycles,
                                    "test-aba",
                                    "test-only transient mutation",
                                    "disconnect-orphan-test-aba");
                                Ledger.Disarm(aba);
                            }

                            if (RecoveryWaitCount == 2
                                && CancelAfterSecondTerminal != null)
                            {
                                CancelAfterSecondTerminal.Cancel();
                            }

                            if (RecoveryWaitCount == 2
                                && ReconnectOldAtProofCommit)
                            {
                                proofCommitOwnerRaceArmed = true;
                            }

                            return Task.FromResult(status);
                        },
                    GetMonotonicMilliseconds = () =>
                        MonotonicMilliseconds,
                    DelayAsync = (milliseconds, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        DelayCount++;
                        MonotonicMilliseconds = checked(
                            MonotonicMilliseconds + milliseconds);
                        return Task.CompletedTask;
                    },
                    WritePassLog = result =>
                    {
                        PassLogCount++;
                        Events.Add("PassLog");
                        if (PassLogError != null)
                        {
                            throw PassLogError;
                        }
                    },
                    RecoveryRequired = (scope, error) =>
                    {
                        RecoveryRequiredCount++;
                        RecoveryScope = scope;
                    }
                };
            }

            private static Task<T> TaskFromException<T>(Exception error)
            {
                var completion = new TaskCompletionSource<T>();
                completion.SetException(error);
                return completion.Task;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class RecorderDoubleExactConfigurationAbsenceProof
    {
        internal RecorderDoubleExactConfigurationAbsenceProof(
            Guid identity,
            LMCRecorderConfigurationAbsentException absence)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "A nonempty Double-bank recovery identity is required.",
                    "identity");
            }

            if (absence == null)
            {
                throw new ArgumentNullException("absence");
            }

            if (absence.Response == null
                || absence.Response.Detail
                    != LMCDiagnosticsDetailCode
                        .RecorderConfigurationAbsent
                || absence.DiagnosticsBootId == 0
                || absence.ConfigId == 0
                || absence.ConfigRevision == 0
                || absence.MapRevision == 0)
            {
                throw new InvalidOperationException(
                    "Recorder configuration absence proof is not an exact typed 0x7E4A canonical-empty result.");
            }

            Identity = identity;
            DiagnosticsBootId = absence.DiagnosticsBootId;
            ConfigId = absence.ConfigId;
            ConfigRevision = absence.ConfigRevision;
            MapRevision = absence.MapRevision;
        }

        internal Guid Identity { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint ConfigId { get; private set; }
        internal uint ConfigRevision { get; private set; }
        internal uint MapRevision { get; private set; }
    }

    internal sealed class RecorderDoubleExactReleaseProof
    {
        private readonly ReadOnlyCollection<
            RecorderDoubleRecoveryBankEvidence> releasedBanks;

        internal RecorderDoubleExactReleaseProof(
            Guid identity,
            RecorderDoubleBankRecoveryScope scope,
            bool explicitSafetyConfirmation)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "A nonempty Double-bank recovery identity is required.",
                    "identity");
            }

            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (!explicitSafetyConfirmation)
            {
                throw new InvalidOperationException(
                    "Explicit safety confirmation is required before resolving Double-bank recovery evidence.");
            }

            if (scope.Request == null
                || scope.Request.Capabilities == null
                || scope.Request.Configuration == null
                || scope.Configuration == null
                || scope.RecoveryToken != identity
                || !scope.ConfigurationAttempted
                || !scope.Configuration.IsReleased
                || HasUnreleasedCapture(scope.BankA)
                || HasUnreleasedCapture(scope.BankB)
                || HasUnreleasedCapture(scope.UnexpectedThird)
                || (scope.BankAStartAttempted && scope.BankA == null)
                || (scope.BankBStartAttempted && scope.BankB == null)
                || (scope.ThirdStartAttempted
                    && !scope.ThirdStartExactBusyConfirmed
                    && scope.UnexpectedThird == null))
            {
                throw new InvalidOperationException(
                    "Every possible Double-bank capture and its configuration must have exact successful Release evidence before resolving the durable journal.");
            }

            var request = scope.Request;
            var configuration = scope.Configuration;
            if (request.Capabilities.DiagnosticsBootId == 0
                || request.Capabilities.MapRevision == 0
                || request.Configuration.RequestedConfigId == 0
                || configuration.DiagnosticsBootId
                    != request.Capabilities.DiagnosticsBootId
                || configuration.ConfigId
                    != request.Configuration.RequestedConfigId
                || configuration.ConfigRevision == 0
                || configuration.UsedZeroIdDiscovery
                || !ReferenceEquals(
                    configuration.OwnerToken,
                    request.OwnerToken)
                || !ReferenceEquals(
                    configuration.SessionToken,
                    request.SessionToken))
            {
                throw new InvalidOperationException(
                    "Released Double-bank configuration evidence does not match the exact qualification request identity.");
            }

            var banks = new List<RecorderDoubleRecoveryBankEvidence>(2);
            AddReleasedBank(banks, scope.BankA, request, configuration);
            AddReleasedBank(banks, scope.BankB, request, configuration);
            AddReleasedBank(
                banks,
                scope.UnexpectedThird,
                request,
                configuration);
            banks.Sort((left, right) =>
                left.BufferId.CompareTo(right.BufferId));

            Identity = identity;
            DiagnosticsBootId = configuration.DiagnosticsBootId;
            ConfigId = configuration.ConfigId;
            ConfigRevision = configuration.ConfigRevision;
            MapRevision = request.Capabilities.MapRevision;
            releasedBanks = new ReadOnlyCollection<
                RecorderDoubleRecoveryBankEvidence>(banks);
        }

        internal RecorderDoubleExactReleaseProof(
            Guid identity,
            RecorderDoubleRecoveryResult result,
            bool explicitSafetyConfirmation)
        {
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "A nonempty Double-bank recovery identity is required.",
                    "identity");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (!explicitSafetyConfirmation)
            {
                throw new InvalidOperationException(
                    "Explicit safety confirmation is required before resolving recovered Double-bank evidence.");
            }

            var plan = result.Plan;
            var inventory = result.Inventory;
            if (plan == null
                || inventory == null
                || plan.JournalIdentity != identity
                || inventory.DiagnosticsBootId != plan.DiagnosticsBootId
                || inventory.ConfigId != plan.ConfigId
                || inventory.ConfigRevision != plan.ConfigRevision
                || inventory.MapRevision != plan.MapRevision
                || inventory.ConfigurationOwnerSessionEpoch
                    != plan.PreviousOwnerSessionEpoch
                || inventory.ConfigurationClosedSessionEpoch
                    != plan.PreviousOwnerSessionEpoch)
            {
                throw new InvalidOperationException(
                    "Recovered Double-bank result does not match its exact durable inventory plan.");
            }

            var banks = new List<RecorderDoubleRecoveryBankEvidence>(
                plan.Banks.Count);
            if (plan.Route
                == RecorderDoubleRecoveryRoute.AdoptEmptyConfiguration)
            {
                var configuration = result.AdoptedConfiguration;
                if (configuration == null
                    || result.AdoptedBanks.Count != 0
                    || !configuration.IsReleased
                    || configuration.IsReleaseOutcomeUnverified
                    || configuration.DiagnosticsBootId
                        != plan.DiagnosticsBootId
                    || configuration.ConfigId != plan.ConfigId
                    || configuration.ConfigRevision != plan.ConfigRevision
                    || configuration.MapRevision != plan.MapRevision
                    || configuration.PreviousOwnerSessionEpoch
                        != plan.PreviousOwnerSessionEpoch)
                {
                    throw new InvalidOperationException(
                        "Recovered empty Double-bank configuration lacks exact successful Release evidence.");
                }
            }
            else if (plan.Route
                == RecorderDoubleRecoveryRoute.AdoptOccupiedBanks)
            {
                if (result.AdoptedConfiguration != null
                    || result.AdoptedBanks.Count != plan.Banks.Count
                    || plan.Banks.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Recovered occupied Double-bank result lacks every exact adopted bank handle.");
                }

                uint ownerSessionEpoch = 0;
                var configurationReleaseConfirmed = false;
                for (var index = 0; index < plan.Banks.Count; index++)
                {
                    var target = plan.Banks[index];
                    var handle = result.AdoptedBanks[index];
                    if (handle == null
                        || !handle.IsAdopted
                        || !handle.IsBufferReleased
                        || handle.IsBufferReleaseOutcomeUnverified
                        || handle.IsRecorderReleaseOutcomeUnverified
                        || handle.DiagnosticsBootId
                            != plan.DiagnosticsBootId
                        || handle.ConfigId != plan.ConfigId
                        || handle.ConfigRevision != plan.ConfigRevision
                        || handle.MapRevision != plan.MapRevision
                        || handle.RecordId != target.RecordId
                        || handle.BufferId != target.BufferId
                        || handle.OwnerSessionEpoch == 0
                        || (ownerSessionEpoch != 0
                            && handle.OwnerSessionEpoch
                                != ownerSessionEpoch))
                    {
                        throw new InvalidOperationException(
                            "Recovered Double-bank handle lacks exact successful Release evidence.");
                    }

                    if (ownerSessionEpoch == 0)
                    {
                        ownerSessionEpoch = handle.OwnerSessionEpoch;
                    }

                    configurationReleaseConfirmed |=
                        handle.IsRecorderReleased;
                    banks.Add(new RecorderDoubleRecoveryBankEvidence(
                        handle.BufferId,
                        handle.RecordId));
                }

                if (!configurationReleaseConfirmed)
                {
                    throw new InvalidOperationException(
                        "Recovered Double-bank configuration lacks an exact successful Release acknowledgement.");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "A current-owner recovery route cannot resolve durable adoption evidence.");
            }

            Identity = identity;
            DiagnosticsBootId = plan.DiagnosticsBootId;
            ConfigId = plan.ConfigId;
            ConfigRevision = plan.ConfigRevision;
            MapRevision = plan.MapRevision;
            releasedBanks = new ReadOnlyCollection<
                RecorderDoubleRecoveryBankEvidence>(banks);
        }

        internal Guid Identity { get; private set; }
        internal uint DiagnosticsBootId { get; private set; }
        internal uint ConfigId { get; private set; }
        internal uint ConfigRevision { get; private set; }
        internal uint MapRevision { get; private set; }
        internal IReadOnlyList<RecorderDoubleRecoveryBankEvidence>
            ReleasedBanks
        {
            get { return releasedBanks; }
        }

        private static void AddReleasedBank(
            IList<RecorderDoubleRecoveryBankEvidence> banks,
            RecorderDoubleBankCaptureLease capture,
            RecorderDoubleBankQualificationRequest request,
            RecorderDoubleBankConfigurationLease configuration)
        {
            if (capture == null)
            {
                return;
            }

            if (!capture.IsReleased
                || capture.DiagnosticsBootId
                    != configuration.DiagnosticsBootId
                || capture.ConfigId != configuration.ConfigId
                || capture.ConfigRevision != configuration.ConfigRevision
                || capture.RecordId == 0
                || capture.BufferId > 1
                || capture.UsedZeroIdDiscovery
                || !ReferenceEquals(capture.OwnerToken, request.OwnerToken)
                || !ReferenceEquals(
                    capture.SessionToken,
                    request.SessionToken))
            {
                throw new InvalidOperationException(
                    "Released Double-bank capture evidence does not match the exact qualification request identity.");
            }

            for (var index = 0; index < banks.Count; index++)
            {
                if (banks[index].BufferId == capture.BufferId
                    || banks[index].RecordId == capture.RecordId)
                {
                    throw new InvalidOperationException(
                        "Released Double-bank capture evidence contains a duplicate or ambiguous bank identity.");
                }
            }

            banks.Add(new RecorderDoubleRecoveryBankEvidence(
                capture.BufferId,
                capture.RecordId));
        }

        private static bool HasUnreleasedCapture(
            RecorderDoubleBankCaptureLease capture)
        {
            return capture != null && !capture.IsReleased;
        }
    }

    internal sealed class RecorderDoubleQualificationJournalBridge
    {
        private readonly RecorderDoubleRecoveryJournal journal;
        private readonly Guid identity;
        private readonly Func<DateTime> utcNow;

        internal RecorderDoubleQualificationJournalBridge(
            RecorderDoubleRecoveryJournal journal,
            Guid identity,
            Func<DateTime> utcNow)
        {
            this.journal = journal
                ?? throw new ArgumentNullException("journal");
            if (identity == Guid.Empty)
            {
                throw new ArgumentException(
                    "A nonempty Double-bank recovery identity is required.",
                    "identity");
            }

            this.utcNow = utcNow
                ?? throw new ArgumentNullException("utcNow");
            this.identity = identity;
        }

        internal Task ArmRecoveryBeforeConfigureAsync(
            RecorderDoubleBankRecoveryScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            var request = scope.Request;
            if (request == null
                || request.Capabilities == null
                || request.Configuration == null)
            {
                throw new InvalidOperationException(
                    "A complete Double-bank qualification request is required before arming recovery.");
            }

            scope.BindRecoveryToken(identity);

            journal.ArmBeforeConfigureDispatch(
                identity,
                utcNow(),
                request.Capabilities.DiagnosticsBootId,
                request.Capabilities.MapRevision,
                request.Configuration.RequestedConfigId);
            return Task.CompletedTask;
        }

        internal Task PersistRecoveryCheckpointAsync(
            RecorderDoubleBankRecoveryScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (scope.Configuration == null)
            {
                throw new InvalidOperationException(
                    "A configuration reply is required before a Double-bank recovery checkpoint.");
            }

            if (scope.RecoveryToken != identity)
            {
                throw new InvalidOperationException(
                    "The qualification recovery token does not match the durable journal identity.");
            }

            var configuration = scope.Configuration;
            var mapRevision = scope.Request.Capabilities.MapRevision;
            var updatedUtc = utcNow();
            journal.RecordConfigurationReply(
                identity,
                updatedUtc,
                configuration.DiagnosticsBootId,
                configuration.ConfigId,
                configuration.ConfigRevision,
                mapRevision);
            PersistCapture(scope.BankA, updatedUtc, mapRevision);
            PersistCapture(scope.BankB, updatedUtc, mapRevision);
            PersistCapture(
                scope.UnexpectedThird,
                updatedUtc,
                mapRevision);
            return Task.CompletedTask;
        }

        internal RecorderDoubleRecoveryRecord ResolveAfterExactRelease(
            RecorderDoubleBankRecoveryScope scope,
            bool explicitSafetyConfirmation)
        {
            var proof = new RecorderDoubleExactReleaseProof(
                identity,
                scope,
                explicitSafetyConfirmation);
            return journal.ResolveAfterExactRelease(proof, utcNow());
        }

        internal RecorderDoubleRecoveryRecord
            ResolveRecoveredAfterExactRelease(
                RecorderDoubleRecoveryResult result,
                bool explicitSafetyConfirmation)
        {
            var proof = new RecorderDoubleExactReleaseProof(
                identity,
                result,
                explicitSafetyConfirmation);
            return journal.ResolveAfterExactRelease(proof, utcNow());
        }

        private void PersistCapture(
            RecorderDoubleBankCaptureLease capture,
            DateTime updatedUtc,
            uint mapRevision)
        {
            if (capture == null)
            {
                return;
            }

            journal.RecordCaptureReply(
                identity,
                updatedUtc,
                capture.DiagnosticsBootId,
                capture.ConfigId,
                capture.ConfigRevision,
                mapRevision,
                capture.RecordId,
                capture.BufferId);
        }

    }
}

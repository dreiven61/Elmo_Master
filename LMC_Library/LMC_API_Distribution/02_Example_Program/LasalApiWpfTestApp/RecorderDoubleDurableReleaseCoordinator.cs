using System;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class RecorderDoubleDurableReleaseCoordinator
    {
        private readonly RecorderDoubleRecoveryJournal journal;
        private readonly Guid identity;
        private readonly Func<DateTime> utcNow;

        internal RecorderDoubleDurableReleaseCoordinator(
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

            this.identity = identity;
            this.utcNow = utcNow
                ?? throw new ArgumentNullException("utcNow");
        }

        internal async Task ReleaseQualificationBankAsync(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture,
            RecorderDoubleBankQualificationOperations operations,
            bool explicitSafetyConfirmation,
            CancellationToken cancellationToken)
        {
            ValidateExplicitConfirmation(explicitSafetyConfirmation);
            if (capture != null && capture.IsReleased)
            {
                ConfirmQualificationBankRelease(scope, capture);
                return;
            }

            var record = journal.CurrentRecord;
            var reusePendingIntent = capture != null
                && record != null
                && record.IsBankReleasePending(
                    capture.BufferId,
                    capture.RecordId);
            if (record != null
                && record.HasBankReleaseOutcomeUncertain
                && !reusePendingIntent)
            {
                throw new InvalidOperationException(
                    "Every exact pending qualification bank Release must finish before a new bank Release may start.");
            }

            await RecorderDoubleBankQualificationOrchestrator
                .ReleaseBankWithDurableIntentAsync(
                    scope,
                    capture,
                    operations,
                    explicitSafetyConfirmation,
                    reusePendingIntent
                        ? (Action)(() => { })
                        : () => BeginQualificationBankRelease(scope, capture),
                    cancellationToken).ConfigureAwait(false);

            if (!capture.IsReleased
                || capture.IsReleaseOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "Qualification bank Release returned without exact successful handle state.");
            }

            ConfirmQualificationBankRelease(scope, capture);
        }

        internal async Task<RecorderDoubleRecoveryRecord>
            ReleaseQualificationConfigurationAndResolveAsync(
                RecorderDoubleBankRecoveryScope scope,
                RecorderDoubleBankQualificationOperations operations,
                bool explicitSafetyConfirmation,
                CancellationToken cancellationToken)
        {
            ValidateExplicitConfirmation(explicitSafetyConfirmation);
            if (scope != null
                && scope.Configuration != null
                && scope.Configuration.IsReleased)
            {
                return ResolveQualification(
                    scope,
                    explicitSafetyConfirmation);
            }

            var reusePendingIntent = journal.CurrentRecord != null
                && journal.CurrentRecord
                    .HasConfigurationReleaseOutcomeUncertain;

            await RecorderDoubleBankQualificationOrchestrator
                .ReleaseConfigurationWithDurableIntentAsync(
                    scope,
                    operations,
                    explicitSafetyConfirmation,
                    reusePendingIntent
                        ? (Action)(() => { })
                        : () => BeginQualificationConfigurationRelease(scope),
                    cancellationToken).ConfigureAwait(false);

            if (!scope.Configuration.IsReleased
                || scope.Configuration.IsReleaseOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "Qualification configuration Release returned without exact successful handle state.");
            }

            return ResolveQualification(scope, explicitSafetyConfirmation);
        }

        internal async Task ReleaseRecoveredBankAsync(
            RecorderDoubleRecoveryResult result,
            LMCRecorderIdentity handle,
            Func<LMCRecorderIdentity, Task> releaseAsync,
            bool explicitSafetyConfirmation,
            CancellationToken cancellationToken)
        {
            ValidateExplicitConfirmation(explicitSafetyConfirmation);
            if (releaseAsync == null)
            {
                throw new ArgumentNullException("releaseAsync");
            }

            var plan = ValidateRecoveredResult(
                result,
                RecorderDoubleRecoveryRoute.AdoptOccupiedBanks);
            if (handle != null && handle.IsBufferReleased)
            {
                ValidateRecoveredBankHandle(
                    result,
                    plan,
                    handle,
                    true,
                    true);
                ConfirmRecoveredBankRelease(handle);
                return;
            }

            ValidateRecoveredBankHandle(
                result,
                plan,
                handle,
                false,
                false);
            var record = journal.CurrentRecord;
            var reusePendingIntent = record.IsBankReleasePending(
                handle.BufferId,
                handle.RecordId);
            if (record.HasBankReleaseOutcomeUncertain
                && !reusePendingIntent)
            {
                throw new InvalidOperationException(
                    "Every exact pending bank Release must be retried before a new bank Release may start.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!reusePendingIntent)
            {
                journal.BeginBankRelease(
                    identity,
                    utcNow(),
                    handle.DiagnosticsBootId,
                    handle.ConfigId,
                    handle.ConfigRevision,
                    handle.MapRevision,
                    handle.BufferId,
                    handle.RecordId);
            }

            await releaseAsync(handle).ConfigureAwait(false);
            if (!handle.IsBufferReleased
                || handle.IsBufferReleaseOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "Recovered bank Release returned without exact successful handle state.");
            }

            ConfirmRecoveredBankRelease(handle);
        }

        internal async Task<RecorderDoubleRecoveryRecord>
            ReleaseRecoveredEmptyConfigurationAndResolveAsync(
                RecorderDoubleRecoveryResult result,
                Func<LMCRecoveredRecorderConfigurationLease, Task>
                    releaseAsync,
                bool explicitSafetyConfirmation,
                CancellationToken cancellationToken)
        {
            ValidateExplicitConfirmation(explicitSafetyConfirmation);
            if (releaseAsync == null)
            {
                throw new ArgumentNullException("releaseAsync");
            }

            var plan = ValidateRecoveredResult(
                result,
                RecorderDoubleRecoveryRoute.AdoptEmptyConfiguration);
            var handle = result.AdoptedConfiguration;
            if (handle == null
                || handle.IsReleaseOutcomeUnverified
                || handle.DiagnosticsBootId != plan.DiagnosticsBootId
                || handle.ConfigId != plan.ConfigId
                || handle.ConfigRevision != plan.ConfigRevision
                || handle.MapRevision != plan.MapRevision)
            {
                throw new InvalidOperationException(
                    "Recovered configuration handle is not the exact usable adoption result.");
            }

            if (handle.IsReleased)
            {
                return ResolveRecovered(result, explicitSafetyConfirmation);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var record = journal.CurrentRecord;
            if (!record.HasConfigurationReleaseOutcomeUncertain)
            {
                journal.BeginConfigurationRelease(
                    identity,
                    utcNow(),
                    plan.DiagnosticsBootId,
                    plan.ConfigId,
                    plan.ConfigRevision,
                    plan.MapRevision);
            }

            await releaseAsync(handle).ConfigureAwait(false);
            if (!handle.IsReleased || handle.IsReleaseOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "Recovered configuration Release returned without exact successful handle state.");
            }

            return ResolveRecovered(result, explicitSafetyConfirmation);
        }

        internal async Task<RecorderDoubleRecoveryRecord>
            ReleaseRecoveredOccupiedConfigurationAndResolveAsync(
                RecorderDoubleRecoveryResult result,
                LMCRecorderIdentity configurationHandle,
                Func<LMCRecorderIdentity, Task> releaseAsync,
                bool explicitSafetyConfirmation,
                CancellationToken cancellationToken)
        {
            ValidateExplicitConfirmation(explicitSafetyConfirmation);
            if (releaseAsync == null)
            {
                throw new ArgumentNullException("releaseAsync");
            }

            var plan = ValidateRecoveredResult(
                result,
                RecorderDoubleRecoveryRoute.AdoptOccupiedBanks);
            ValidateRecoveredBankHandle(
                result,
                plan,
                configurationHandle,
                true,
                true);
            for (var index = 0;
                index < result.AdoptedBanks.Count;
                index++)
            {
                var bank = result.AdoptedBanks[index];
                if (!bank.IsBufferReleased
                    || bank.IsBufferReleaseOutcomeUnverified)
                {
                    throw new InvalidOperationException(
                        "Every recovered bank must have exact successful Release before configuration Release.");
                }
            }

            if (configurationHandle.IsRecorderReleased)
            {
                return ResolveRecovered(result, explicitSafetyConfirmation);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var reusePendingIntent = journal.CurrentRecord
                .HasConfigurationReleaseOutcomeUncertain;
            if (!reusePendingIntent)
            {
                journal.BeginConfigurationRelease(
                    identity,
                    utcNow(),
                    plan.DiagnosticsBootId,
                    plan.ConfigId,
                    plan.ConfigRevision,
                    plan.MapRevision);
            }

            await releaseAsync(configurationHandle).ConfigureAwait(false);
            if (!configurationHandle.IsRecorderReleased
                || configurationHandle
                    .IsRecorderReleaseOutcomeUnverified)
            {
                throw new InvalidOperationException(
                    "Recovered configuration Release returned without exact successful handle state.");
            }

            return ResolveRecovered(result, explicitSafetyConfirmation);
        }

        private void ConfirmQualificationBankRelease(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture)
        {
            if (scope == null
                || scope.Request == null
                || scope.Request.Capabilities == null
                || scope.Configuration == null
                || capture == null
                || (!ReferenceEquals(scope.BankA, capture)
                    && !ReferenceEquals(scope.BankB, capture)
                    && !ReferenceEquals(scope.UnexpectedThird, capture))
                || scope.RecoveryToken != identity
                || !capture.IsReleased
                || capture.IsReleaseOutcomeUnverified
                || capture.DiagnosticsBootId
                    != scope.Configuration.DiagnosticsBootId
                || capture.ConfigId != scope.Configuration.ConfigId
                || capture.ConfigRevision
                    != scope.Configuration.ConfigRevision)
            {
                throw new InvalidOperationException(
                    "Retained qualification bank does not provide exact ACK-success evidence for durable confirmation.");
            }

            journal.ConfirmBankRelease(
                identity,
                utcNow(),
                capture.DiagnosticsBootId,
                capture.ConfigId,
                capture.ConfigRevision,
                scope.Request.Capabilities.MapRevision,
                capture.BufferId,
                capture.RecordId);
        }

        private void ConfirmRecoveredBankRelease(
            LMCRecorderIdentity handle)
        {
            journal.ConfirmBankRelease(
                identity,
                utcNow(),
                handle.DiagnosticsBootId,
                handle.ConfigId,
                handle.ConfigRevision,
                handle.MapRevision,
                handle.BufferId,
                handle.RecordId);
        }

        private RecorderDoubleRecoveryRecord ResolveQualification(
            RecorderDoubleBankRecoveryScope scope,
            bool explicitSafetyConfirmation)
        {
            var proof = new RecorderDoubleExactReleaseProof(
                identity,
                scope,
                explicitSafetyConfirmation);
            return journal.ResolveAfterExactRelease(proof, utcNow());
        }

        private void BeginQualificationBankRelease(
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture)
        {
            journal.BeginBankRelease(
                identity,
                utcNow(),
                capture.DiagnosticsBootId,
                capture.ConfigId,
                capture.ConfigRevision,
                scope.Request.Capabilities.MapRevision,
                capture.BufferId,
                capture.RecordId);
        }

        private void BeginQualificationConfigurationRelease(
            RecorderDoubleBankRecoveryScope scope)
        {
            var configuration = scope.Configuration;
            journal.BeginConfigurationRelease(
                identity,
                utcNow(),
                configuration.DiagnosticsBootId,
                configuration.ConfigId,
                configuration.ConfigRevision,
                scope.Request.Capabilities.MapRevision);
        }

        private RecorderDoubleRecoveryRecord ResolveRecovered(
            RecorderDoubleRecoveryResult result,
            bool explicitSafetyConfirmation)
        {
            var proof = new RecorderDoubleExactReleaseProof(
                identity,
                result,
                explicitSafetyConfirmation);
            return journal.ResolveAfterExactRelease(proof, utcNow());
        }

        private RecorderDoubleRecoveryPlan ValidateRecoveredResult(
            RecorderDoubleRecoveryResult result,
            RecorderDoubleRecoveryRoute expectedRoute)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            var plan = result.Plan;
            var record = journal.CurrentRecord;
            if (plan == null
                || plan.Route != expectedRoute
                || plan.JournalIdentity != identity
                || record == null
                || !record.IsActive
                || record.Identity != identity
                || (record.HasBankReleaseOutcomeUncertain
                    && expectedRoute != RecorderDoubleRecoveryRoute
                        .AdoptOccupiedBanks)
                || record.DiagnosticsBootId != plan.DiagnosticsBootId
                || record.RequestedConfigId != plan.ConfigId
                || record.ConfigRevision != plan.ConfigRevision
                || record.MapRevision != plan.MapRevision)
            {
                throw new InvalidOperationException(
                    "Recovered Release result does not match one exact usable durable record.");
            }

            if (record.HasBankReleaseOutcomeUncertain)
            {
                var absent = RecorderDoubleRecoveryPlanner
                    .FindPendingBankReleaseAbsences(
                        record,
                        result.Inventory);
                if (absent.Count != 0)
                {
                    throw new InvalidOperationException(
                        "Recovered Release result does not prove exact presence of every pending bank.");
                }
            }

            return plan;
        }

        private static void ValidateRecoveredBankHandle(
            RecorderDoubleRecoveryResult result,
            RecorderDoubleRecoveryPlan plan,
            LMCRecorderIdentity handle,
            bool requireBufferReleased,
            bool allowRecorderReleased)
        {
            var found = false;
            for (var index = 0;
                index < result.AdoptedBanks.Count;
                index++)
            {
                if (ReferenceEquals(result.AdoptedBanks[index], handle))
                {
                    found = true;
                    break;
                }
            }

            if (!found
                || handle == null
                || !handle.IsAdopted
                || handle.IsBufferReleased != requireBufferReleased
                || handle.IsBufferReleaseOutcomeUnverified
                || (!allowRecorderReleased && handle.IsRecorderReleased)
                || handle.IsRecorderReleaseOutcomeUnverified
                || handle.DiagnosticsBootId != plan.DiagnosticsBootId
                || handle.ConfigId != plan.ConfigId
                || handle.ConfigRevision != plan.ConfigRevision
                || handle.MapRevision != plan.MapRevision)
            {
                throw new InvalidOperationException(
                    "Recovered bank handle is not one exact usable adoption result.");
            }
        }

        private static void ValidateExplicitConfirmation(
            bool explicitSafetyConfirmation)
        {
            if (!explicitSafetyConfirmation)
            {
                throw new InvalidOperationException(
                    "Explicit safety confirmation is required before durable Recorder Release.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCGroupAxis
    {
        private static readonly ConditionalWeakTable<
            LMCGroupEnableWaitCoordinator,
            LMCGroupResetRecoveryAttachmentRegistry>
                groupResetRecoveryAttachmentRegistries =
                    new ConditionalWeakTable<
                        LMCGroupEnableWaitCoordinator,
                        LMCGroupResetRecoveryAttachmentRegistry>();

        /// <summary>
        /// The latest accepted Group Reset whose group and snapshotted member
        /// axis error-clearance proof has not yet been resolved.
        /// </summary>
        public LMCGroupResetWaitContinuation
            PendingGroupResetWaitContinuation
        {
            get
            {
                lock (groupEnableWaitCoordinator.Sync)
                {
                    return groupEnableWaitCoordinator
                        .PendingResetContinuation;
                }
            }
        }

        /// <summary>
        /// Reconciles an accepted or outcome-uncertain safety mutation on one
        /// snapshotted member axis with the exact pending Group Reset.
        /// A rolled-back negative acknowledgement leaves the member mutation
        /// generation unchanged and therefore returns false.
        /// </summary>
        public bool SupersedePendingGroupResetAfterCapturedMemberSafetyMutation(
            LMCGroupResetWaitContinuation continuation,
            LMCSingleAxis memberAxis)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }
            if (memberAxis == null)
            {
                throw new ArgumentNullException("memberAxis");
            }

            EnsureCurrentSessionForUse();
            EnsureGroupResetContinuationIdentity(continuation);
            if (!ReferenceEquals(memberAxis.Connection, connection)
                || memberAxis.SessionGeneration != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The member axis does not belong to this active connection and session.");
            }

            var bindings = CreateBindingsForContinuation(continuation);
            var capturedMember = false;
            for (var index = 0; index < bindings.Length; index++)
            {
                if (bindings[index].Member.AxisReference
                    == memberAxis.AxisReference)
                {
                    capturedMember = true;
                    break;
                }
            }
            if (!capturedMember)
            {
                throw new InvalidOperationException(
                    "The axis is not a member of the Group Reset snapshot.");
            }

            EnterGroupResetCoordinatorLocks(bindings);
            try
            {
                ObserveGroupResetMutationGenerationsCore(
                    continuation,
                    bindings);
                if (continuation.IsPending
                    && !ReferenceEquals(
                        groupEnableWaitCoordinator
                            .PendingResetContinuation,
                        continuation))
                {
                    throw new InvalidOperationException(
                        "The Group Reset continuation is not the exact pending operation.");
                }

                var evidence = continuation.CaptureEvidence(0);
                var memberMutationDetected = false;
                for (var index = 0;
                    index < evidence.MemberMutations.Length;
                    index++)
                {
                    var memberMutation = evidence.MemberMutations[index];
                    if (memberMutation.AxisReference
                            == memberAxis.AxisReference
                        && memberMutation.InterveningMutationDetected)
                    {
                        memberMutationDetected = true;
                        break;
                    }
                }
                if (!memberMutationDetected)
                {
                    return false;
                }

                if (continuation.IsPending)
                {
                    continuation.MarkSupersededBySafetyMutation();
                    groupEnableWaitCoordinator
                        .PendingResetContinuation = null;
                    return true;
                }

                return groupEnableWaitCoordinator
                    .TryPermanentlySupersedeProvisionalGroupReset(
                        continuation);
            }
            finally
            {
                ExitGroupResetCoordinatorLocks(bindings);
            }
        }

        public Task<LMCGroupResetWaitContinuation>
            BeginGroupResetWaitForStableErrorClearanceAsync(
                CancellationToken cancellationToken)
        {
            return BeginGroupResetWaitForStableErrorClearanceAsync(
                new LMCGroupResetWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupResetWaitContinuation>
            BeginGroupResetWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginGroupResetWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public Task<LMCGroupResetWaitContinuation>
            BeginGroupResetWaitForStableErrorClearanceAsync(
                Action<LMCGroupResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            return BeginGroupResetWaitForStableErrorClearanceAsync(
                new LMCGroupResetWaitOptions(),
                acceptedContinuationObserver,
                cancellationToken);
        }

        public Task<LMCGroupResetWaitContinuation>
            BeginGroupResetWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitOptions options,
                Action<LMCGroupResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return BeginGroupResetWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver);
        }

        public Task<LMCGroupResetWaitContinuation>
            BeginGroupResetWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitOptions options,
                Action<LMCGroupResetPreparedEvidence>
                    preparedEvidenceObserver,
                Action<LMCGroupResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (preparedEvidenceObserver == null)
            {
                throw new ArgumentNullException(
                    "preparedEvidenceObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return BeginGroupResetWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver,
                preparedEvidenceObserver);
        }

        internal async Task<LMCGroupResetWaitContinuation>
            BeginGroupResetWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeResetWriteCommit = null,
                Action<LMCGroupResetWaitContinuation>
                    acceptedContinuationObserver = null,
                Action<LMCGroupResetPreparedEvidence>
                    preparedEvidenceObserver = null)
        {
            ThrowIfGroupResetObserverReentrantMutation();
            var validated = ValidateGroupResetWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var operationId = Guid.NewGuid();
            var tracker = new LMCGroupResetWaitTracker(
                new LMCGroupMemberInfo[0],
                validated.StableSampleCount,
                operationId,
                sessionGeneration);
            LMCGroupResetWaitContinuation continuation = null;
            LMCGroupResetMemberBinding[] bindings = null;
            var groupMutationGateAcquired = false;
            var statusGateAcquired = false;
            var memberMutationGateCount = 0;
            var observerLatchSet = false;
            var observerInvocationActive = false;
            var preparedObserverInvocationActive = false;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();

                if (acceptedContinuationObserver != null
                    || preparedEvidenceObserver != null)
                {
                    await AcquireGroupResetGateAsync(
                        groupEnableWaitCoordinator.StatusObservationGate,
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    statusGateAcquired = true;
                }

                await AcquireGroupResetGateAsync(
                    groupEnableWaitCoordinator.MutationGate,
                    validated,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                groupMutationGateAcquired = true;
                EnsureCurrentSessionForUse();
                EnsureGroupResetSubmissionAdmission();

                var membersResult = await ReadGroupResetMembersSnapshotAsync(
                    validated,
                    cancellationToken,
                    elapsedMilliseconds,
                    tracker).ConfigureAwait(false);
                var members = ValidateGroupResetMembersSnapshot(
                    membersResult);
                tracker = new LMCGroupResetWaitTracker(
                    members,
                    validated.StableSampleCount,
                    operationId,
                    sessionGeneration);
                bindings = CreateGroupResetMemberBindings(members);

                var sortedBindings = SortGroupResetBindings(bindings);
                for (var index = 0; index < sortedBindings.Length; index++)
                {
                    await AcquireGroupResetGateAsync(
                        sortedBindings[index].Coordinator.MutationGate,
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    memberMutationGateCount++;
                }

                EnsureCurrentSessionForUse();
                EnsureGroupResetSubmissionAdmission();
                EnsureGroupResetMemberMutationAdmission(bindings);

                var preparedEvidence =
                    new LMCGroupResetPreparedEvidence(
                        operationId,
                        GroupName,
                        GroupReference,
                        sessionGeneration,
                        members,
                        validated.StableSampleCount);
                Action notifyPrepared = () =>
                {
                    if (beforeResetWriteCommit != null)
                    {
                        beforeResetWriteCommit();
                    }
                    if (preparedEvidenceObserver != null)
                    {
                        preparedObserverInvocationActive = true;
                        using (LMCGroupResetObserverScope.Enter(
                            connection,
                            sessionGeneration,
                            GroupReference,
                            members))
                        {
                            preparedEvidenceObserver(preparedEvidence);
                        }
                        preparedObserverInvocationActive = false;
                    }
                };

                var acknowledgement = await SendGroupResetForWaitAsync(
                    tracker,
                    bindings,
                    validated,
                    cancellationToken,
                    elapsedMilliseconds,
                    notifyPrepared).ConfigureAwait(false);
                if (!acknowledgement.IsSuccess)
                {
                    RollBackRejectedGroupResetMutation(
                        tracker,
                        bindings);
                    tracker.SetAcknowledgement(acknowledgement);
                    connection.PublishSessionBoundSendPriorityResult(
                        sessionGeneration,
                        LMC_CommandId.GroupReset,
                        () => { });
                    throw new LMCGroupResetRejectedException(
                        tracker.CaptureEvidence(
                            elapsedMilliseconds()));
                }

                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.GroupReset,
                    () =>
                    {
                        lock (groupEnableWaitCoordinator.Sync)
                        {
                            EnsureGroupResetSubmissionAdmissionCore();
                            tracker.SetAcknowledgement(acknowledgement);
                            continuation =
                                new LMCGroupResetWaitContinuation(
                                    groupEnableWaitCoordinator,
                                    connection,
                                    GroupName,
                                    GroupReference,
                                    sessionGeneration,
                                    GetBindingCoordinators(bindings),
                                    tracker);
                            groupEnableWaitCoordinator
                                .PendingResetContinuation = continuation;
                            groupEnableWaitCoordinator
                                .ResetAcceptanceObserverInProgress =
                                acceptedContinuationObserver != null;
                        }
                    });

                observerLatchSet = continuation != null
                    && acceptedContinuationObserver != null;
                if (acceptedContinuationObserver != null)
                {
                    groupEnableWaitCoordinator
                        .StatusObservationGate.Release();
                    statusGateAcquired = false;
                    observerInvocationActive = true;
                    using (LMCGroupResetObserverScope.Enter(
                        connection,
                        sessionGeneration,
                        GroupReference,
                        continuation.Members))
                    {
                        acceptedContinuationObserver(continuation);
                    }
                    observerInvocationActive = false;
                }

                ThrowIfGroupResetExpiredAfterPublication(
                    cancellationToken,
                    elapsedMilliseconds,
                    validated.TimeoutMilliseconds);
                return continuation;
            }
            catch (LMCGroupResetRejectedException)
            {
                throw;
            }
            catch (LMCGroupResetWaitPendingException)
            {
                throw;
            }
            catch (LMCGroupResetWaitDeadlineException)
            {
                throw new LMCGroupResetWaitTimeoutException(
                    continuation == null
                        ? tracker.CaptureEvidence(elapsedMilliseconds())
                        : continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                    continuation);
            }
            catch (OperationCanceledException ex)
            {
                if (observerInvocationActive)
                {
                    throw;
                }
                if (preparedObserverInvocationActive)
                {
                    throw new LMCGroupResetSubmissionException(
                        tracker.CaptureEvidence(elapsedMilliseconds()),
                        ex);
                }

                throw new LMCGroupResetWaitCanceledException(
                    continuation == null
                        ? tracker.CaptureEvidence(elapsedMilliseconds())
                        : continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                    continuation,
                    ex,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                if (continuation != null)
                {
                    throw;
                }

                throw new LMCGroupResetSubmissionException(
                    tracker.CaptureEvidence(elapsedMilliseconds()),
                    ex);
            }
            finally
            {
                if (observerLatchSet)
                {
                    lock (groupEnableWaitCoordinator.Sync)
                    {
                        groupEnableWaitCoordinator
                            .ResetAcceptanceObserverInProgress = false;
                        Monitor.PulseAll(groupEnableWaitCoordinator.Sync);
                    }
                }

                ReleaseGroupResetMemberMutationGates(
                    bindings,
                    ref memberMutationGateCount);
                if (groupMutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }
                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator
                        .StatusObservationGate.Release();
                }
            }
        }

        public Task<LMCGroupResetWaitContinuation>
            AttachGroupResetDurableRecoveryAsync(
                LMCGroupResetDurableRecoveryRecord record,
                CancellationToken cancellationToken)
        {
            var options = new LMCGroupResetWaitOptions();
            if (record != null)
            {
                options.StableSampleCount =
                    record.RequiredStableSampleCount;
            }
            return AttachGroupResetDurableRecoveryAsync(
                record,
                options,
                cancellationToken);
        }

        public Task<LMCGroupResetWaitContinuation>
            AttachGroupResetDurableRecoveryAsync(
                LMCGroupResetDurableRecoveryRecord record,
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return AttachGroupResetDurableRecoveryAsync(
                record,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal async Task<LMCGroupResetWaitContinuation>
            AttachGroupResetDurableRecoveryAsync(
                LMCGroupResetDurableRecoveryRecord record,
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            ThrowIfGroupResetObserverReentrantMutation();
            ValidateGroupResetDurableRecoveryRecord(record);
            var validated = ValidateGroupResetWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            if (validated.StableSampleCount
                != record.RequiredStableSampleCount)
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind.InvalidRecord,
                    record,
                    "StableSampleCount must match the durable Group Reset record.");
            }

            var persistedMembers = CreateMembersFromDurableIdentities(
                record.Members);
            var tracker = new LMCGroupResetWaitTracker(
                persistedMembers,
                record.RequiredStableSampleCount,
                record.OperationId,
                record.OwnerSessionGeneration,
                record.PriorSubmissionOutcome,
                true);
            LMCGroupResetMemberBinding[] bindings = null;
            LMCGroupResetWaitContinuation continuation = null;
            var statusGateAcquired = false;
            var groupMutationGateAcquired = false;
            var memberMutationGateCount = 0;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();

                await AcquireGroupResetGateAsync(
                    groupEnableWaitCoordinator.StatusObservationGate,
                    validated,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                statusGateAcquired = true;

                await AcquireGroupResetGateAsync(
                    groupEnableWaitCoordinator.MutationGate,
                    validated,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                groupMutationGateAcquired = true;
                EnsureCurrentSessionForUse();
                ThrowIfGroupResetRecoveryAlreadyAttached(record);
                EnsureGroupResetSubmissionAdmission();

                bindings = CreateGroupResetMemberBindings(
                    persistedMembers);
                var sortedBindings = SortGroupResetBindings(bindings);
                for (var index = 0;
                    index < sortedBindings.Length;
                    index++)
                {
                    await AcquireGroupResetGateAsync(
                        sortedBindings[index].Coordinator.MutationGate,
                        validated,
                        cancellationToken,
                        elapsedMilliseconds).ConfigureAwait(false);
                    memberMutationGateCount++;
                }

                EnsureCurrentSessionForUse();
                ThrowIfGroupResetRecoveryAlreadyAttached(record);
                EnsureGroupResetSubmissionAdmission();
                EnsureGroupResetMemberMutationAdmission(bindings);

                var membersResult = await ReadGroupResetMembersSnapshotAsync(
                    validated,
                    cancellationToken,
                    elapsedMilliseconds,
                    tracker).ConfigureAwait(false);
                LMCGroupMemberInfo[] observedMembers;
                try
                {
                    observedMembers = ValidateGroupResetMembersSnapshot(
                        membersResult);
                }
                catch (Exception ex)
                {
                    throw CreateGroupResetDurableRecoveryException(
                        LMCGroupResetDurableRecoveryFailureKind
                            .MemberSnapshotMismatch,
                        record,
                        "The current 0x20D2 Group Reset member snapshot is invalid.",
                        ex);
                }
                EnsureDurableGroupResetMembersMatch(
                    record,
                    observedMembers);

                EnterGroupResetCoordinatorLocks(bindings);
                try
                {
                    EnsureCurrentSessionForUse();
                    ThrowIfGroupResetRecoveryAlreadyAttachedCore(record);
                    EnsureGroupResetSubmissionAdmissionCore();
                    EnsureGroupResetMemberMutationAdmission(bindings);

                    tracker.SetRecoveryMutationGenerationBaseline(
                        groupEnableWaitCoordinator.MutationGeneration,
                        CaptureGroupResetMemberMutationGenerations(
                            bindings));
                    continuation = new LMCGroupResetWaitContinuation(
                        groupEnableWaitCoordinator,
                        connection,
                        GroupName,
                        GroupReference,
                        sessionGeneration,
                        GetBindingCoordinators(bindings),
                        tracker);

                    connection.PublishSessionBoundSendPriorityResult(
                        sessionGeneration,
                        LMC_CommandId.GetMembers,
                        () =>
                        {
                            ThrowIfGroupResetRecoveryAlreadyAttachedCore(
                                record);
                            EnsureGroupResetSubmissionAdmissionCore();
                            GetGroupResetRecoveryAttachmentRegistry()
                                .AttachedOperationIds.Add(
                                    record.OperationId);
                            groupEnableWaitCoordinator
                                .PendingResetContinuation = continuation;
                        });
                }
                finally
                {
                    ExitGroupResetCoordinatorLocks(bindings);
                }

                ThrowIfGroupResetExpiredAfterPublication(
                    cancellationToken,
                    elapsedMilliseconds,
                    validated.TimeoutMilliseconds);
                return continuation;
            }
            catch (LMCGroupResetDurableRecoveryException)
            {
                throw;
            }
            catch (LMCGroupResetWaitDeadlineException)
            {
                throw new LMCGroupResetDurableRecoveryTimeoutException(
                    record,
                    continuation);
            }
            catch (OperationCanceledException ex)
            {
                throw new LMCGroupResetDurableRecoveryCanceledException(
                    record,
                    continuation,
                    ex,
                    cancellationToken);
            }
            catch (LMCGroupResetWaitPendingException ex)
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind.PendingOperation,
                    record,
                    "Another Group Reset continuation is already pending.",
                    ex);
            }
            catch (Exception ex)
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind.SessionInvalid,
                    record,
                    "Group Reset durable recovery could not be attached to the active session.",
                    ex);
            }
            finally
            {
                ReleaseGroupResetMemberMutationGates(
                    bindings,
                    ref memberMutationGateCount);
                if (groupMutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }
                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator
                        .StatusObservationGate.Release();
                }
            }
        }

        public Task<LMCGroupResetWaitResult>
            ResumeGroupResetWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            var options = new LMCGroupResetWaitOptions();
            if (continuation != null)
            {
                options.StableSampleCount =
                    continuation.RequiredStableSampleCount;
            }

            return ResumeGroupResetWaitForStableErrorClearanceAsync(
                continuation,
                options,
                cancellationToken);
        }

        public Task<LMCGroupResetWaitResult>
            ResumeGroupResetWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitContinuation continuation,
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumeGroupResetWaitForStableErrorClearanceAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal async Task<LMCGroupResetWaitResult>
            ResumeGroupResetWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitContinuation continuation,
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            var validated = ValidateGroupResetWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            EnsureGroupResetContinuationIdentity(continuation);
            if (validated.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted Group Reset continuation.",
                    "options");
            }

            var statusGateAcquired = false;
            var waitRegistered = false;
            var waitCompleted = false;
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupResetContinuationPendingCore(continuation);
                if (groupEnableWaitCoordinator.ResetWaitInProgress)
                {
                    throw new InvalidOperationException(
                        "Another Group Reset status-only wait is already running.");
                }

                groupEnableWaitCoordinator.ResetWaitInProgress = true;
                waitRegistered = true;
            }

            try
            {
                lock (groupEnableWaitCoordinator.Sync)
                {
                    EnsureGroupResetContinuationPendingCore(continuation);
                    continuation.ResetStableProof();
                }

                while (true)
                {
                    try
                    {
                        await AcquireGroupResetGateAsync(
                            groupEnableWaitCoordinator
                                .StatusObservationGate,
                            validated,
                            cancellationToken,
                            elapsedMilliseconds).ConfigureAwait(false);
                        statusGateAcquired = true;
                    }
                    catch (LMCGroupResetWaitDeadlineException)
                    {
                        throw new LMCGroupResetWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupResetWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }

                    LMCGroupResetStatusRound round = null;
                    try
                    {
                        try
                        {
                            ThrowIfGroupResetWaitMutationIntervened(
                                continuation,
                                elapsedMilliseconds);
                            round = await ReadGroupResetStatusRoundAsync(
                                continuation,
                                validated,
                                cancellationToken,
                                elapsedMilliseconds).ConfigureAwait(false);
                            CommitGroupResetStatusRound(
                                continuation,
                                round,
                                validated,
                                cancellationToken,
                                elapsedMilliseconds);
                        }
                        catch (LMCGroupResetInterferenceException)
                        {
                            throw;
                        }
                        catch (LMCGroupResetWaitDeadlineException)
                        {
                            throw new LMCGroupResetWaitTimeoutException(
                                continuation.CaptureEvidence(
                                    elapsedMilliseconds()),
                                continuation);
                        }
                        catch (OperationCanceledException ex)
                        {
                            throw new LMCGroupResetWaitCanceledException(
                                continuation.CaptureEvidence(
                                    elapsedMilliseconds()),
                                continuation,
                                ex,
                                cancellationToken);
                        }
                        catch (LMCGroupResetStatusException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw new LMCGroupResetStatusException(
                                continuation.CaptureEvidence(
                                    elapsedMilliseconds()),
                                continuation,
                                null,
                                null,
                                ex);
                        }

                        ThrowIfGroupResetRoundFailed(
                            continuation,
                            round,
                            elapsedMilliseconds);
                    }
                    finally
                    {
                        if (statusGateAcquired)
                        {
                            groupEnableWaitCoordinator
                                .StatusObservationGate.Release();
                            statusGateAcquired = false;
                        }
                    }

                    if (continuation.IsCompleted)
                    {
                        waitCompleted = true;
                        return new LMCGroupResetWaitResult(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }

                    try
                    {
                        await DelayGroupResetWaitAsync(
                            validated,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCGroupResetWaitDeadlineException)
                    {
                        throw new LMCGroupResetWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupResetWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                }
            }
            finally
            {
                if (waitRegistered)
                {
                    lock (groupEnableWaitCoordinator.Sync)
                    {
                        if (!waitCompleted && continuation.IsPending)
                        {
                            continuation.ResetStableProof();
                        }
                        groupEnableWaitCoordinator.ResetWaitInProgress =
                            false;
                    }
                }
                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator
                        .StatusObservationGate.Release();
                }
            }
        }

        public Task<LMCGroupResetWaitResult>
            GroupResetAndWaitForStableErrorClearanceAsync(
                CancellationToken cancellationToken)
        {
            return GroupResetAndWaitForStableErrorClearanceAsync(
                new LMCGroupResetWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupResetWaitResult>
            GroupResetAndWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return GroupResetAndWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public Task<LMCGroupResetWaitResult>
            GroupResetAndWaitForStableErrorClearanceAsync(
                Action<LMCGroupResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            return GroupResetAndWaitForStableErrorClearanceAsync(
                new LMCGroupResetWaitOptions(),
                acceptedContinuationObserver,
                cancellationToken);
        }

        public Task<LMCGroupResetWaitResult>
            GroupResetAndWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitOptions options,
                Action<LMCGroupResetWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return GroupResetAndWaitForStableErrorClearanceAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver);
        }

        internal async Task<LMCGroupResetWaitResult>
            GroupResetAndWaitForStableErrorClearanceAsync(
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeResetWriteCommit = null,
                Action<LMCGroupResetWaitContinuation>
                    acceptedContinuationObserver = null)
        {
            var continuation = await
                BeginGroupResetWaitForStableErrorClearanceAsync(
                    options,
                    cancellationToken,
                    elapsedMilliseconds,
                    delayAsync,
                    beforeResetWriteCommit,
                    acceptedContinuationObserver).ConfigureAwait(false);
            return await ResumeGroupResetWaitForStableErrorClearanceAsync(
                continuation,
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync).ConfigureAwait(false);
        }

        private void ThrowIfGroupResetWaitBlocksCommand(ushort command)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                if (groupEnableWaitCoordinator
                    .ResetAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "A Group Reset accepted-continuation observer is still running.");
                }

                if (IsGroupResetSafetyCommand(command))
                {
                    return;
                }

                var pending = groupEnableWaitCoordinator
                    .PendingResetContinuation;
                if (pending == null || !pending.IsPending)
                {
                    return;
                }

                throw new LMCGroupResetWaitPendingException(pending);
            }
        }

        private static bool IsGroupResetSafetyCommand(ushort command)
        {
            return command == LMC_CommandId.GroupStop
                || command == LMC_CommandId.GroupProfileUnlock
                || command == LMC_CommandId.GroupPowerOff;
        }

        private void WaitForGroupResetObserverHandoffForSafetyMutation(
            CancellationToken cancellationToken)
        {
            ThrowIfGroupResetObserverReentrantMutation();
            groupEnableWaitCoordinator
                .WaitForGroupResetObserverHandoffForSafetyMutation(
                    cancellationToken);
        }

        private bool WaitForGroupResetObserverHandoffForSafetyMutation(
            CancellationToken cancellationToken,
            int timeoutMilliseconds)
        {
            ThrowIfGroupResetObserverReentrantMutation();
            return groupEnableWaitCoordinator
                .WaitForGroupResetObserverHandoffForSafetyMutation(
                    cancellationToken,
                    timeoutMilliseconds);
        }

        private void ThrowIfGroupResetObserverReentrantMutation()
        {
            LMCGroupResetObserverScope.ThrowIfGroupMutationReentrant(
                connection,
                sessionGeneration,
                GroupReference);
        }

        private static LMCGroupResetWaitOptions
            ValidateGroupResetWaitOptions(
                LMCGroupResetWaitOptions options,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (elapsedMilliseconds == null)
            {
                throw new ArgumentNullException("elapsedMilliseconds");
            }
            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }

            return options.SnapshotAndValidate();
        }

        private static long GetGroupResetWaitRemaining(
            LMCGroupResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCGroupResetWaitDeadlineException();
            }

            return remaining;
        }

        private static async Task AcquireGroupResetGateAsync(
            SemaphoreSlim gate,
            LMCGroupResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetGroupResetWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            var acquired = false;
            using (var deadlineCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    await gate.WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                    acquired = true;
                    cancellationToken.ThrowIfCancellationRequested();
                    if (elapsedMilliseconds()
                        >= options.TimeoutMilliseconds)
                    {
                        throw new LMCGroupResetWaitDeadlineException();
                    }
                }
                catch (OperationCanceledException)
                {
                    if (acquired)
                    {
                        gate.Release();
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupResetWaitDeadlineException();
                }
                catch
                {
                    if (acquired)
                    {
                        gate.Release();
                    }
                    throw;
                }
            }
        }

        private async Task<LMCGroupMembersInfoResult>
            ReadGroupResetMembersSnapshotAsync(
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                LMCGroupResetWaitTracker tracker)
        {
            var raw = await ExchangeGroupResetWireAsync(
                LMC_Frame.LMCGroupGetMembersInfo(GroupReference),
                options,
                cancellationToken,
                elapsedMilliseconds,
                null,
                tracker.MarkTransportInvalidatedAtDeadline)
                .ConfigureAwait(false);
            return LMCConnection.ParseGroupMembersInfoResult(raw);
        }

        private static LMCGroupMemberInfo[]
            ValidateGroupResetMembersSnapshot(
                LMCGroupMembersInfoResult result)
        {
            if (result == null
                || !result.IsSuccess
                || result.AxisCount < 1
                || result.AxisCount > 16)
            {
                throw new InvalidOperationException(
                    "Group Reset requires a successful non-empty 0x20D2 member snapshot.");
            }

            var members = result.Members;
            if (members.Length != result.AxisCount)
            {
                throw new InvalidOperationException(
                    "The 0x20D2 member snapshot count is inconsistent.");
            }

            var references = new HashSet<ushort>();
            for (var index = 0; index < members.Length; index++)
            {
                if (members[index] == null
                    || members[index].Index != index
                    || members[index].AxisReference == 0
                    || !references.Add(members[index].AxisReference))
                {
                    throw new InvalidOperationException(
                        "The 0x20D2 member snapshot contains a zero, duplicate, or misindexed axis reference.");
                }
            }

            return members;
        }

        private void ValidateGroupResetDurableRecoveryRecord(
            LMCGroupResetDurableRecoveryRecord record)
        {
            if (record == null)
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind.InvalidRecord,
                    null,
                    "The durable Group Reset recovery record is required.");
            }
            if (record.OperationId == Guid.Empty
                || (record.PriorSubmissionOutcome
                        != LMCGroupResetSubmissionOutcome.Accepted
                    && record.PriorSubmissionOutcome
                        != LMCGroupResetSubmissionOutcome
                            .OutcomeUncertain)
                || record.OwnerSessionGeneration <= 0
                || record.RequiredStableSampleCount < 1
                || record.RequiredStableSampleCount > 100)
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind.InvalidRecord,
                    record,
                    "The durable Group Reset recovery record is invalid.");
            }
            if (record.GroupReference == 0
                || record.GroupReference != GroupReference
                || !string.Equals(
                    record.GroupName,
                    GroupName,
                    StringComparison.Ordinal))
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind
                        .GroupIdentityMismatch,
                    record,
                    "The durable Group Reset group identity does not match this group.");
            }

            var members = record.Members;
            if (members.Length < 1 || members.Length > 16)
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind.InvalidRecord,
                    record,
                    "The durable Group Reset member snapshot must contain 1 through 16 members.");
            }
            var references = new HashSet<ushort>();
            for (var index = 0; index < members.Length; index++)
            {
                if (members[index] == null
                    || members[index].Index != index
                    || members[index].AxisReference == 0
                    || !references.Add(members[index].AxisReference))
                {
                    throw CreateGroupResetDurableRecoveryException(
                        LMCGroupResetDurableRecoveryFailureKind
                            .InvalidRecord,
                        record,
                        "The durable Group Reset member snapshot contains a zero, duplicate, or misindexed axis reference.");
                }
            }
        }

        private static LMCGroupMemberInfo[]
            CreateMembersFromDurableIdentities(
                LMCGroupResetDurableMemberIdentity[] identities)
        {
            var members = new LMCGroupMemberInfo[identities.Length];
            for (var index = 0; index < identities.Length; index++)
            {
                members[index] = new LMCGroupMemberInfo(
                    identities[index].Index,
                    identities[index].AxisReference,
                    identities[index].DeviceId,
                    identities[index].AxisName);
            }
            return members;
        }

        private static void EnsureDurableGroupResetMembersMatch(
            LMCGroupResetDurableRecoveryRecord record,
            LMCGroupMemberInfo[] observedMembers)
        {
            var expectedMembers = record.Members;
            if (expectedMembers.Length != observedMembers.Length)
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind
                        .MemberSnapshotMismatch,
                    record,
                    "The current 0x20D2 Group Reset member count does not match the durable record.");
            }

            for (var index = 0; index < expectedMembers.Length; index++)
            {
                var expected = expectedMembers[index];
                var observed = observedMembers[index];
                if (expected.Index != observed.Index
                    || expected.AxisReference != observed.AxisReference
                    || expected.DeviceId != observed.DeviceId
                    || !string.Equals(
                        expected.AxisName,
                        observed.AxisName,
                        StringComparison.Ordinal))
                {
                    throw CreateGroupResetDurableRecoveryException(
                        LMCGroupResetDurableRecoveryFailureKind
                            .MemberSnapshotMismatch,
                        record,
                        "The current 0x20D2 Group Reset member identity or order does not match the durable record.");
                }
            }
        }

        private LMCGroupResetRecoveryAttachmentRegistry
            GetGroupResetRecoveryAttachmentRegistry()
        {
            return groupResetRecoveryAttachmentRegistries.GetValue(
                groupEnableWaitCoordinator,
                ignored =>
                    new LMCGroupResetRecoveryAttachmentRegistry());
        }

        private void ThrowIfGroupResetRecoveryAlreadyAttached(
            LMCGroupResetDurableRecoveryRecord record)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                ThrowIfGroupResetRecoveryAlreadyAttachedCore(record);
            }
        }

        private void ThrowIfGroupResetRecoveryAlreadyAttachedCore(
            LMCGroupResetDurableRecoveryRecord record)
        {
            if (GetGroupResetRecoveryAttachmentRegistry()
                .AttachedOperationIds.Contains(record.OperationId))
            {
                throw CreateGroupResetDurableRecoveryException(
                    LMCGroupResetDurableRecoveryFailureKind
                        .DuplicateAttachment,
                    record,
                    "The durable Group Reset operation was already attached in this process session.");
            }
        }

        private static LMCGroupResetDurableRecoveryException
            CreateGroupResetDurableRecoveryException(
                LMCGroupResetDurableRecoveryFailureKind failureKind,
                LMCGroupResetDurableRecoveryRecord record,
                string message,
                Exception innerException = null)
        {
            return new LMCGroupResetDurableRecoveryException(
                failureKind,
                record,
                message,
                innerException);
        }

        private LMCGroupResetMemberBinding[]
            CreateGroupResetMemberBindings(LMCGroupMemberInfo[] members)
        {
            var bindings = new LMCGroupResetMemberBinding[members.Length];
            for (var index = 0; index < members.Length; index++)
            {
                bindings[index] = new LMCGroupResetMemberBinding(
                    members[index],
                    connection.GetAxisPowerOnWaitCoordinator(
                        sessionGeneration,
                        members[index].AxisReference));
            }
            return bindings;
        }

        private static LMCGroupResetMemberBinding[]
            SortGroupResetBindings(
                LMCGroupResetMemberBinding[] bindings)
        {
            if (bindings == null)
            {
                return new LMCGroupResetMemberBinding[0];
            }

            var sorted = (LMCGroupResetMemberBinding[])bindings.Clone();
            Array.Sort(
                sorted,
                (left, right) => left.Member.AxisReference.CompareTo(
                    right.Member.AxisReference));
            return sorted;
        }

        private static LMCAxisPowerOnWaitCoordinator[]
            GetBindingCoordinators(
                LMCGroupResetMemberBinding[] bindings)
        {
            var coordinators = new LMCAxisPowerOnWaitCoordinator[
                bindings.Length];
            for (var index = 0; index < bindings.Length; index++)
            {
                coordinators[index] = bindings[index].Coordinator;
            }
            return coordinators;
        }

        private static void ReleaseGroupResetMemberMutationGates(
            LMCGroupResetMemberBinding[] bindings,
            ref int acquiredCount)
        {
            if (bindings == null || acquiredCount <= 0)
            {
                acquiredCount = 0;
                return;
            }

            var sorted = SortGroupResetBindings(bindings);
            for (var index = acquiredCount - 1; index >= 0; index--)
            {
                sorted[index].Coordinator.MutationGate.Release();
            }
            acquiredCount = 0;
        }

        private void EnsureGroupResetSubmissionAdmission()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupResetSubmissionAdmissionCore();
            }
        }

        private void EnsureGroupResetSubmissionAdmissionCore()
        {
            if (groupEnableWaitCoordinator
                    .ResetAcceptanceObserverInProgress
                || groupEnableWaitCoordinator
                    .EnableAcceptanceObserverInProgress
                || groupEnableWaitCoordinator
                    .PowerAcceptanceObserverInProgress
                || groupEnableWaitCoordinator
                    .DisableAcceptanceObserverInProgress)
            {
                throw new InvalidOperationException(
                    "A group accepted-continuation observer is still running.");
            }

            var pending = groupEnableWaitCoordinator
                .PendingResetContinuation;
            if (pending != null && pending.IsPending)
            {
                throw new LMCGroupResetWaitPendingException(pending);
            }
            if (groupEnableWaitCoordinator.ResetWaitInProgress)
            {
                throw new InvalidOperationException(
                    "Another Group Reset status-only wait is already running.");
            }
        }

        private static void EnsureGroupResetMemberMutationAdmission(
            LMCGroupResetMemberBinding[] bindings)
        {
            for (var index = 0; index < bindings.Length; index++)
            {
                var coordinator = bindings[index].Coordinator;
                lock (coordinator.Sync)
                {
                    if (coordinator.AcceptanceObserverInProgress
                        || coordinator.PowerOffAcceptanceObserverInProgress
                        || coordinator.StopAcceptanceObserverInProgress
                        || coordinator.ResetAcceptanceObserverInProgress)
                    {
                        throw new InvalidOperationException(
                            "A captured member axis accepted-continuation observer is still running.");
                    }
                }
            }
        }

        private async Task<LMC_Response> SendGroupResetForWaitAsync(
            LMCGroupResetWaitTracker tracker,
            LMCGroupResetMemberBinding[] bindings,
            LMCGroupResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforeResetWriteCommit)
        {
            var raw = await ExchangeGroupResetWireAsync(
                LMC_Frame.LMCGroupReset(GroupReference),
                options,
                cancellationToken,
                elapsedMilliseconds,
                () =>
                {
                    if (beforeResetWriteCommit != null)
                    {
                        beforeResetWriteCommit();
                    }
                },
                tracker.MarkTransportInvalidatedAtDeadline,
                () =>
                {
                    tracker.MarkCommandDispatchedInOwnerSession();
                    tracker.MarkSubmissionOutcomeUncertain();
                    var groupGeneration = groupEnableWaitCoordinator
                        .MarkMutationMayHaveBeenSent();
                    groupEnableWaitCoordinator
                        .ResetPendingMutationProof();
                    var memberGenerations = new long[bindings.Length];
                    for (var index = 0;
                        index < bindings.Length;
                        index++)
                    {
                        memberGenerations[index] = bindings[index]
                            .Coordinator.MarkMutationMayHaveBeenSent();
                    }
                    tracker.SetMutationGenerations(
                        groupGeneration,
                        memberGenerations);
                }).ConfigureAwait(false);
            return LMCConnection.ParseCommandAcknowledgement(
                raw,
                "Group Reset");
        }

        private void RollBackRejectedGroupResetMutation(
            LMCGroupResetWaitTracker tracker,
            LMCGroupResetMemberBinding[] bindings)
        {
            var evidence = tracker.CaptureEvidence(0);
            var memberMutations = evidence.MemberMutations;
            for (var index = bindings.Length - 1; index >= 0; index--)
            {
                bindings[index].Coordinator.TryRollbackRejectedMutation(
                    memberMutations[index].ExpectedMutationGeneration);
            }
            groupEnableWaitCoordinator.TryRollbackRejectedMutation(
                tracker.ResetMutationGeneration);
            tracker.ClearMutationGenerationsAfterRejected();
        }

        private async Task<byte[]> ExchangeGroupResetWireAsync(
            byte[] request,
            LMCGroupResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action onWriteStarting,
            Action markTransportInvalidatedAtDeadline,
            Action onWriteCommitted = null)
        {
            var remaining = GetGroupResetWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            using (var deadlineCancellation =
                new CancellationTokenSource())
            using (var preWriteCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    deadlineCancellation.Token))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    return await connection.ExchangeAsyncDrainAfterWrite(
                        request,
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () =>
                        {
                            ThrowIfGroupResetCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            if (onWriteStarting != null)
                            {
                                onWriteStarting();
                            }
                        },
                        onWriteCommitted).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    if (markTransportInvalidatedAtDeadline != null)
                    {
                        markTransportInvalidatedAtDeadline();
                    }
                    throw new LMCGroupResetWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (deadlineCancellation.IsCancellationRequested)
                    {
                        throw new LMCGroupResetWaitDeadlineException();
                    }
                    throw;
                }
            }
        }

        private void EnsureGroupResetContinuationIdentity(
            LMCGroupResetWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            if (continuation == null
                || !ReferenceEquals(
                    continuation.Coordinator,
                    groupEnableWaitCoordinator)
                || !continuation.BelongsTo(
                    connection,
                    sessionGeneration,
                    GroupReference,
                    GroupName))
            {
                throw new InvalidOperationException(
                    "The Group Reset continuation does not belong to this active connection, session, or group.");
            }
        }

        private void EnsureGroupResetContinuationPendingCore(
            LMCGroupResetWaitContinuation continuation)
        {
            EnsureGroupResetContinuationIdentity(continuation);
            if (groupEnableWaitCoordinator
                .ResetAcceptanceObserverInProgress)
            {
                throw new InvalidOperationException(
                    "The Group Reset accepted-continuation observer is still running.");
            }
            if (!continuation.IsPending
                || !ReferenceEquals(
                    groupEnableWaitCoordinator.PendingResetContinuation,
                    continuation))
            {
                if (continuation.State
                        == LMCGroupResetWaitContinuationState
                            .SupersededBySafetyMutation
                    || continuation.State
                        == LMCGroupResetWaitContinuationState
                            .SupersededByInterveningMutation)
                {
                    var bindings = CreateBindingsForContinuation(
                        continuation);
                    continuation.ObserveMutationGenerations(
                        groupEnableWaitCoordinator.MutationGeneration,
                        CaptureGroupResetMemberMutationGenerations(
                            bindings));
                    throw new LMCGroupResetInterferenceException(
                        continuation.CaptureEvidence(0),
                        continuation);
                }

                throw new InvalidOperationException(
                    "The Group Reset continuation is not the latest pending operation.");
            }
        }

        private void ThrowIfGroupResetWaitMutationIntervened(
            LMCGroupResetWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            EnsureGroupResetContinuationIdentity(continuation);
            var bindings = CreateBindingsForContinuation(continuation);
            EnterGroupResetCoordinatorLocks(bindings);
            try
            {
                ObserveGroupResetMutationGenerationsCore(
                    continuation,
                    bindings);
                if (!continuation.IsPending
                    || !ReferenceEquals(
                        groupEnableWaitCoordinator
                            .PendingResetContinuation,
                        continuation)
                    || continuation.HasInterveningMutation)
                {
                    ResolveGroupResetInterferenceCore(
                        continuation,
                        elapsedMilliseconds());
                }
            }
            finally
            {
                ExitGroupResetCoordinatorLocks(bindings);
            }
        }

        private LMCGroupResetMemberBinding[]
            CreateBindingsForContinuation(
                LMCGroupResetWaitContinuation continuation)
        {
            var members = continuation.Members;
            var coordinators = continuation.MemberCoordinators;
            if (members.Length != coordinators.Length)
            {
                throw new InvalidOperationException(
                    "The Group Reset continuation member snapshot is inconsistent.");
            }

            var bindings = new LMCGroupResetMemberBinding[members.Length];
            for (var index = 0; index < members.Length; index++)
            {
                bindings[index] = new LMCGroupResetMemberBinding(
                    members[index],
                    coordinators[index]);
            }
            return bindings;
        }

        private static long[] CaptureGroupResetMemberMutationGenerations(
            LMCGroupResetMemberBinding[] bindings)
        {
            var generations = new long[bindings.Length];
            for (var index = 0; index < bindings.Length; index++)
            {
                generations[index] = bindings[index]
                    .Coordinator.MutationGeneration;
            }
            return generations;
        }

        private void ObserveGroupResetMutationGenerationsCore(
            LMCGroupResetWaitContinuation continuation,
            LMCGroupResetMemberBinding[] bindings)
        {
            continuation.ObserveMutationGenerations(
                groupEnableWaitCoordinator.MutationGeneration,
                CaptureGroupResetMemberMutationGenerations(bindings));
        }

        private void ResolveGroupResetInterferenceCore(
            LMCGroupResetWaitContinuation continuation,
            long elapsedMilliseconds)
        {
            if (continuation.IsPending)
            {
                continuation.MarkSupersededByInterveningMutation();
            }
            if (ReferenceEquals(
                groupEnableWaitCoordinator.PendingResetContinuation,
                continuation))
            {
                groupEnableWaitCoordinator.PendingResetContinuation = null;
            }
            throw new LMCGroupResetInterferenceException(
                continuation.CaptureEvidence(elapsedMilliseconds),
                continuation);
        }

        private void EnterGroupResetCoordinatorLocks(
            LMCGroupResetMemberBinding[] bindings)
        {
            Monitor.Enter(groupEnableWaitCoordinator.Sync);
            var sorted = SortGroupResetBindings(bindings);
            for (var index = 0; index < sorted.Length; index++)
            {
                Monitor.Enter(sorted[index].Coordinator.Sync);
            }
        }

        private void ExitGroupResetCoordinatorLocks(
            LMCGroupResetMemberBinding[] bindings)
        {
            var sorted = SortGroupResetBindings(bindings);
            for (var index = sorted.Length - 1; index >= 0; index--)
            {
                Monitor.Exit(sorted[index].Coordinator.Sync);
            }
            Monitor.Exit(groupEnableWaitCoordinator.Sync);
        }

        private async Task<LMCGroupResetStatusRound>
            ReadGroupResetStatusRoundAsync(
                LMCGroupResetWaitContinuation continuation,
                LMCGroupResetWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds)
        {
            var groupRaw = await ExchangeGroupResetWireAsync(
                LMC_Frame.LMCGroupReadStatus(GroupReference),
                options,
                cancellationToken,
                elapsedMilliseconds,
                () => ThrowIfGroupResetWaitMutationIntervened(
                    continuation,
                    elapsedMilliseconds),
                continuation.MarkTransportInvalidatedAtDeadline)
                .ConfigureAwait(false);
            var groupStatus = LMCConnection
                .ParseGroupReadStatusResult(groupRaw);

            var members = continuation.Members;
            var memberStatuses = new LMCGroupResetMemberStatus[
                members.Length];
            if (!groupStatus.IsReadSuccessful)
            {
                return new LMCGroupResetStatusRound(
                    groupStatus,
                    memberStatuses);
            }

            for (var index = 0; index < members.Length; index++)
            {
                var member = members[index];
                var axisRaw = await ExchangeGroupResetWireAsync(
                    LMC_Frame.LMCAxisReadStatus(
                        member.AxisReference),
                    options,
                    cancellationToken,
                    elapsedMilliseconds,
                    () => ThrowIfGroupResetWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds),
                    continuation.MarkTransportInvalidatedAtDeadline)
                    .ConfigureAwait(false);
                memberStatuses[index] =
                    new LMCGroupResetMemberStatus(
                        member.Index,
                        member.AxisReference,
                        member.DeviceId,
                        member.AxisName,
                        LMCConnection.ParseReadStatusResult(axisRaw));
                if (!memberStatuses[index].Status.IsReadSuccessful)
                {
                    return new LMCGroupResetStatusRound(
                        groupStatus,
                        memberStatuses);
                }
            }

            return new LMCGroupResetStatusRound(
                groupStatus,
                memberStatuses);
        }

        private void CommitGroupResetStatusRound(
            LMCGroupResetWaitContinuation continuation,
            LMCGroupResetStatusRound round,
            LMCGroupResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var bindings = CreateBindingsForContinuation(continuation);
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.ReadStatus,
                () =>
                {
                    EnterGroupResetCoordinatorLocks(bindings);
                    try
                    {
                        ObserveGroupResetMutationGenerationsCore(
                            continuation,
                            bindings);
                        if (!continuation.IsPending
                            || !ReferenceEquals(
                                groupEnableWaitCoordinator
                                    .PendingResetContinuation,
                                continuation)
                            || continuation.HasInterveningMutation)
                        {
                            ResolveGroupResetInterferenceCore(
                                continuation,
                                elapsedMilliseconds());
                        }

                        continuation.ObserveRound(
                            round.GroupStatus,
                            round.MemberStatuses);
                        if (!cancellationToken.IsCancellationRequested
                            && elapsedMilliseconds()
                                < options.TimeoutMilliseconds
                            && continuation.HasStableProof)
                        {
                            continuation.MarkCompleted();
                            groupEnableWaitCoordinator
                                .PendingResetContinuation = null;
                        }
                    }
                    finally
                    {
                        ExitGroupResetCoordinatorLocks(bindings);
                    }
                });

            if (!continuation.IsCompleted)
            {
                ThrowIfGroupResetExpiredAfterWire(
                    cancellationToken,
                    elapsedMilliseconds,
                    options.TimeoutMilliseconds);
                ThrowIfGroupResetWaitMutationIntervened(
                    continuation,
                    elapsedMilliseconds);
            }
        }

        private static void ThrowIfGroupResetRoundFailed(
            LMCGroupResetWaitContinuation continuation,
            LMCGroupResetStatusRound round,
            Func<long> elapsedMilliseconds)
        {
            if (round.GroupStatus == null
                || !round.GroupStatus.IsReadSuccessful)
            {
                throw new LMCGroupResetStatusException(
                    continuation.CaptureEvidence(
                        elapsedMilliseconds()),
                    continuation,
                    round.GroupStatus,
                    null,
                    null);
            }

            for (var index = 0;
                index < round.MemberStatuses.Length;
                index++)
            {
                var memberStatus = round.MemberStatuses[index];
                if (memberStatus == null
                    || memberStatus.Status == null
                    || !memberStatus.Status.IsReadSuccessful)
                {
                    throw new LMCGroupResetStatusException(
                        continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                        continuation,
                        null,
                        memberStatus,
                        null);
                }
            }
        }

        private static async Task DelayGroupResetWaitAsync(
            LMCGroupResetWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = GetGroupResetWaitRemaining(
                options,
                cancellationToken,
                elapsedMilliseconds);
            using (var deadlineCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    await delayAsync(
                        Math.Min(
                            options.PollIntervalMilliseconds,
                            (int)remaining),
                        deadlineCancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupResetWaitDeadlineException();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= options.TimeoutMilliseconds)
            {
                throw new LMCGroupResetWaitDeadlineException();
            }
        }

        private static void ThrowIfGroupResetCannotStartWire(
            CancellationToken cancellationToken,
            CancellationTokenSource deadlineCancellation,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds
                || (deadlineCancellation.IsCancellationRequested
                    && !cancellationToken.IsCancellationRequested))
            {
                throw new LMCGroupResetWaitDeadlineException();
            }
        }

        private static void ThrowIfGroupResetExpiredAfterWire(
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds)
            {
                throw new LMCGroupResetWaitDeadlineException();
            }
        }

        private static void ThrowIfGroupResetExpiredAfterPublication(
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds)
            {
                throw new LMCGroupResetWaitDeadlineException();
            }
        }

        private sealed class LMCGroupResetMemberBinding
        {
            internal LMCGroupResetMemberBinding(
                LMCGroupMemberInfo member,
                LMCAxisPowerOnWaitCoordinator coordinator)
            {
                Member = member;
                Coordinator = coordinator;
            }

            internal LMCGroupMemberInfo Member { get; private set; }
            internal LMCAxisPowerOnWaitCoordinator Coordinator
            {
                get;
                private set;
            }
        }

        private sealed class LMCGroupResetStatusRound
        {
            internal LMCGroupResetStatusRound(
                LMCGroupReadStatusResult groupStatus,
                LMCGroupResetMemberStatus[] memberStatuses)
            {
                GroupStatus = groupStatus;
                MemberStatuses = memberStatuses;
            }

            internal LMCGroupReadStatusResult GroupStatus
            {
                get;
                private set;
            }
            internal LMCGroupResetMemberStatus[] MemberStatuses
            {
                get;
                private set;
            }
        }

        private sealed class LMCGroupResetWaitDeadlineException
            : TimeoutException
        {
        }
    }
}

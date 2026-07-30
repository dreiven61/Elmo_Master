using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public partial class LMCGroupAxis
    {
        private readonly LMCConnection connection;
        private readonly long sessionGeneration;
        private readonly LMCGroupEnableWaitCoordinator groupEnableWaitCoordinator;

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public LMCLookupResult LookupResult { get; private set; }

        /// <summary>
        /// Evidence for an accepted GroupEnable whose locked-standby outcome is not yet resolved.
        /// </summary>
        public LMCGroupEnableWaitContinuation PendingGroupEnableWaitContinuation
        {
            get
            {
                lock (groupEnableWaitCoordinator.Sync)
                {
                    return groupEnableWaitCoordinator.PendingContinuation;
                }
            }
        }

        /// <summary>
        /// The latest accepted GroupStop whose stable standby state has not
        /// yet been resolved. Resuming this continuation sends status reads
        /// only.
        /// </summary>
        public LMCGroupStopWaitContinuation PendingGroupStopWaitContinuation
        {
            get
            {
                lock (groupEnableWaitCoordinator.Sync)
                {
                    return groupEnableWaitCoordinator
                        .PendingStopContinuation;
                }
            }
        }

        /// <summary>
        /// The latest accepted Group Power On or Power Off whose stable target
        /// state has not yet been resolved. Resuming this continuation sends
        /// only 0x2045 status reads.
        /// </summary>
        public LMCGroupPowerStateWaitContinuation
            PendingGroupPowerStateWaitContinuation
        {
            get
            {
                lock (groupEnableWaitCoordinator.Sync)
                {
                    return groupEnableWaitCoordinator
                        .PendingPowerStateContinuation;
                }
            }
        }

        internal LMCConnection Connection
        {
            get { return connection; }
        }

        internal long SessionGeneration
        {
            get { return sessionGeneration; }
        }

        public LMCGroupAxis(LMCConnection connection, string groupName)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            sessionGeneration = connection.SessionGeneration;
            EnsureCurrentSessionForUse();

            GroupName = groupName;
            LookupResult = ResolveGroupLookup(groupName);
            GroupReference = LookupResult.Reference;
            groupEnableWaitCoordinator = connection.GetGroupEnableWaitCoordinator(
                sessionGeneration,
                GroupReference);
        }

        private LMCGroupAxis(
            LMCConnection connection,
            string groupName,
            long sessionGeneration,
            LMCLookupResult lookupResult)
        {
            this.connection = connection;
            this.sessionGeneration = sessionGeneration;
            GroupName = groupName;
            LookupResult = lookupResult;
            GroupReference = lookupResult.Reference;
            groupEnableWaitCoordinator = connection.GetGroupEnableWaitCoordinator(
                sessionGeneration,
                GroupReference);
        }

        public static async Task<LMCGroupAxis> CreateAsync(
            LMCConnection connection,
            string groupName,
            CancellationToken cancellationToken)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            var generation = connection.SessionGeneration;
            connection.EnsureSessionGeneration(generation);

            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCGroupGetByName(groupName),
                generation,
                cancellationToken).ConfigureAwait(false);
            var lookupResult = LMCConnection.ParseLookupResult(
                LMCLookupTargetKind.Group,
                groupName,
                raw);

            connection.EnsureSessionGeneration(generation);
            return new LMCGroupAxis(
                connection,
                groupName,
                generation,
                lookupResult);
        }

        public LMC_Response GetGroupMembersInfo()
        {
            return GetGroupMembersInfoResult().Response;
        }

        public LMCGroupMembersInfoResult GetGroupMembersInfoResult()
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseGroupMembersInfoResult(
                connection.Exchange(
                    LMC_Frame.LMCGroupGetMembersInfo(GroupReference),
                    sessionGeneration));
        }

        public async Task<LMCGroupMembersInfoResult> GetGroupMembersInfoResultAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCGroupGetMembersInfo(GroupReference),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseGroupMembersInfoResult(raw);
        }

        /// <summary>
        /// Locks the configured group profile. The legacy method name is kept for API compatibility.
        /// This command does not power the group axes; use GroupPowerOn or GroupPowerOnAsync for power.
        /// </summary>
        public LMC_Response GroupEnable()
        {
            EnsureCurrentSessionForUse();
            groupEnableWaitCoordinator.MutationGate.Wait();
            try
            {
                BeginDirectGroupEnable();
                try
                {
                    return SendGroupEnable();
                }
                finally
                {
                    EndDirectGroupEnable();
                }
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        /// <summary>
        /// Locks the configured group profile. The legacy method name is kept for API compatibility.
        /// This command does not power the group axes; use GroupPowerOn or GroupPowerOnAsync for power.
        /// </summary>
        public async Task<LMC_Response> GroupEnableAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            await groupEnableWaitCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                BeginDirectGroupEnable();
                try
                {
                    return await SendGroupEnableAsyncUnchecked(
                        cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    EndDirectGroupEnable();
                }
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        /// <summary>
        /// Sends exactly one GroupEnable request and returns after its accepted
        /// acknowledgement is preserved. No GroupReadStatus request is sent by
        /// this method.
        /// </summary>
        public Task<LMCGroupEnableWaitContinuation>
            BeginGroupEnableWaitForLockedStandbyAsync(
                CancellationToken cancellationToken)
        {
            return BeginGroupEnableWaitForLockedStandbyAsync(
                new LMCGroupEnableWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupEnableWaitContinuation>
            BeginGroupEnableWaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginGroupEnableWaitForLockedStandbyAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Sends exactly one GroupEnable request and invokes the observer once
        /// after the accepted continuation is published and before any helper-
        /// owned GroupReadStatus request. If the observer throws, the accepted
        /// continuation remains pending and the original exception is propagated.
        /// </summary>
        public Task<LMCGroupEnableWaitContinuation>
            BeginGroupEnableWaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                Action<LMCGroupEnableWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return BeginGroupEnableWaitForLockedStandbyAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                acceptedContinuationObserver);
        }

        /// <summary>
        /// Sends exactly one GroupEnable request, then polls GroupReadStatus until powered,
        /// locked standby is observed for three consecutive samples.
        /// </summary>
        public Task<LMCGroupEnableWaitResult>
            GroupEnableAndWaitForLockedStandbyAsync(
                CancellationToken cancellationToken)
        {
            return GroupEnableAndWaitForLockedStandbyAsync(
                new LMCGroupEnableWaitOptions(),
                cancellationToken);
        }

        /// <summary>
        /// Applies one total deadline to the initial GroupEnable gate/write and status polling.
        /// Cancellation or deadline before the write sends no GroupEnable and creates no
        /// continuation. After an accepted acknowledgement, timeout or cancellation preserves
        /// a pending continuation that must be resumed or explicitly released. A deadline
        /// after an RPC write invalidates the transport; inspect the typed evidence before retry.
        /// </summary>
        public Task<LMCGroupEnableWaitResult>
            GroupEnableAndWaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return GroupEnableAndWaitForLockedStandbyAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Sends GroupEnable exactly once, publishes the accepted continuation
        /// to the observer before the first status read, then performs status-
        /// only locked-standby verification.
        /// </summary>
        public Task<LMCGroupEnableWaitResult>
            GroupEnableAndWaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                Action<LMCGroupEnableWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            var stopwatch = Stopwatch.StartNew();
            return GroupEnableAndWaitForLockedStandbyAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                null,
                acceptedContinuationObserver);
        }

        /// <summary>
        /// Resumes polling for an already accepted GroupEnable without sending another
        /// GroupEnable request.
        /// </summary>
        public Task<LMCGroupEnableWaitResult>
            ResumeGroupEnableWaitForLockedStandbyAsync(
                LMCGroupEnableWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            return ResumeGroupEnableWaitForLockedStandbyAsync(
                continuation,
                new LMCGroupEnableWaitOptions
                {
                    StableSampleCount = continuation == null
                        ? LMCGroupEnableWaitOptions.DefaultStableSampleCount
                        : continuation.RequiredStableSampleCount
                },
                cancellationToken);
        }

        /// <summary>
        /// Resumes bounded status polling for the exact group/session continuation.
        /// This method sends zero GroupEnable requests.
        /// </summary>
        public Task<LMCGroupEnableWaitResult>
            ResumeGroupEnableWaitForLockedStandbyAsync(
                LMCGroupEnableWaitContinuation continuation,
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumeGroupEnableWaitForLockedStandbyAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Polls only GroupReadStatus until powered locked standby is observed
        /// for three consecutive successful samples. This read-only helper is
        /// suitable for a new process/session and never sends GroupEnable.
        /// </summary>
        public Task<LMCGroupLockedStandbyWaitResult>
            WaitForLockedStandbyAsync(
                CancellationToken cancellationToken)
        {
            return WaitForLockedStandbyAsync(
                new LMCGroupEnableWaitOptions(),
                cancellationToken);
        }

        /// <summary>
        /// Applies one total deadline to status-gate admission, each 0x2045
        /// exchange, and the delays between status-only polls.
        /// </summary>
        public Task<LMCGroupLockedStandbyWaitResult>
            WaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return WaitForLockedStandbyAsync(
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Clears an accepted-enable continuation without wire traffic only after the same
        /// group/session observed three consecutive successful Disabled/Unlocked samples or
        /// three consecutive successful PowerOff samples.
        /// </summary>
        public bool TryReleasePendingGroupEnableForRetry(
            LMCGroupEnableWaitContinuation continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            EnsureCurrentSessionForUse();

            lock (groupEnableWaitCoordinator.Sync)
            {
                ValidateContinuationOwnerAndSession(continuation);

                if (!ReferenceEquals(
                    groupEnableWaitCoordinator.PendingContinuation,
                    continuation))
                {
                    return false;
                }

                if (groupEnableWaitCoordinator.WaitInProgress
                    || !continuation.HasRetryReleaseProof)
                {
                    return false;
                }

                CompletePendingGroupEnableContinuation(continuation);
                return true;
            }
        }

        /// <summary>
        /// Invalidates accumulated status proof for an accepted GroupEnable without
        /// sending wire traffic or resolving its pending continuation. This local-only
        /// operation is intended for a Stop or Power Off reservation boundary.
        /// </summary>
        public bool InvalidatePendingGroupEnableWaitStatusProof()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                var continuation =
                    groupEnableWaitCoordinator.PendingContinuation;
                if (continuation == null || !continuation.IsPending)
                {
                    return false;
                }

                continuation.ResetProofCounters();
                return true;
            }
        }

        /// <summary>
        /// Unlocks the group profile. The legacy method name is kept for API compatibility.
        /// This command does not power off the group axes; use GroupPowerOff or GroupPowerOffAsync.
        /// The current LASAL adapter rejects unlock while the profile is not in position.
        /// </summary>
        public LMC_Response GroupDisable()
        {
            EnsureCurrentSessionForUse();
            groupEnableWaitCoordinator.StatusObservationGate.Wait();
            try
            {
                groupEnableWaitCoordinator.MutationGate.Wait();
                try
                {
                    ThrowIfRawGroupDisableCommandIsUnsafe();
                    var response = SendGroupDisable();
                    ReleasePendingGroupEnableAfterDisable(response);
                    return response;
                }
                finally
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }
            }
            finally
            {
                groupEnableWaitCoordinator.StatusObservationGate.Release();
            }
        }

        /// <summary>
        /// Unlocks the group profile. The legacy method name is kept for API compatibility.
        /// This command does not power off the group axes; use GroupPowerOff or GroupPowerOffAsync.
        /// The current LASAL adapter rejects unlock while the profile is not in position.
        /// </summary>
        public async Task<LMC_Response> GroupDisableAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            await groupEnableWaitCoordinator.StatusObservationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                await groupEnableWaitCoordinator.MutationGate.WaitAsync(
                    cancellationToken).ConfigureAwait(false);
                try
                {
                    ThrowIfRawGroupDisableCommandIsUnsafe();
                    var response = await SendAsyncUnchecked(
                        LMC_Frame.LMCGroupDisable(GroupReference),
                        cancellationToken).ConfigureAwait(false);
                    ReleasePendingGroupEnableAfterDisable(response);
                    return response;
                }
                finally
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }
            }
            finally
            {
                groupEnableWaitCoordinator.StatusObservationGate.Release();
            }
        }

        /// <summary>
        /// Starts LASAL RobotOn processing for the group. A successful ACK confirms command
        /// acceptance only; it does not guarantee that every member axis is servo-ready.
        /// </summary>
        public LMC_Response GroupPowerOn()
        {
            return SendRawGroupPowerCommand(true);
        }

        /// <summary>
        /// Starts LASAL RobotOn processing for the group. A successful ACK confirms command
        /// acceptance only; it does not guarantee that every member axis is servo-ready.
        /// </summary>
        public async Task<LMC_Response> GroupPowerOnAsync(
            CancellationToken cancellationToken)
        {
            return await SendRawGroupPowerCommandAsync(
                true,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts LASAL RobotOff processing for the group. A successful ACK confirms command
        /// acceptance only; it does not guarantee that every member axis is powered off.
        /// </summary>
        public LMC_Response GroupPowerOff()
        {
            return SendRawGroupPowerCommand(false);
        }

        /// <summary>
        /// Starts LASAL RobotOff processing for the group. A successful ACK confirms command
        /// acceptance only; it does not guarantee that every member axis is powered off.
        /// </summary>
        public async Task<LMC_Response> GroupPowerOffAsync(
            CancellationToken cancellationToken)
        {
            return await SendRawGroupPowerCommandAsync(
                false,
                cancellationToken).ConfigureAwait(false);
        }

        public Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerOnWaitForStableStateAsync(
                CancellationToken cancellationToken)
        {
            return BeginGroupPowerOnWaitForStableStateAsync(
                new LMCGroupPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerOnWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken)
        {
            return BeginGroupPowerStateWaitForStableStateAsync(
                true,
                options,
                null,
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerOnWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            return BeginGroupPowerStateWaitForStableStateAsync(
                true,
                options,
                acceptedContinuationObserver,
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerOffWaitForStableStateAsync(
                CancellationToken cancellationToken)
        {
            return BeginGroupPowerOffWaitForStableStateAsync(
                new LMCGroupPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerOffWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken)
        {
            return BeginGroupPowerStateWaitForStableStateAsync(
                false,
                options,
                null,
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerOffWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            return BeginGroupPowerStateWaitForStableStateAsync(
                false,
                options,
                acceptedContinuationObserver,
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitResult>
            ResumeGroupPowerStateWaitForStableStateAsync(
                LMCGroupPowerStateWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            return ResumeGroupPowerStateWaitForStableStateAsync(
                continuation,
                new LMCGroupPowerStateWaitOptions
                {
                    StableSampleCount = continuation == null
                        ? LMCGroupPowerStateWaitOptions
                            .DefaultStableSampleCount
                        : continuation.RequiredStableSampleCount
                },
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitResult>
            ResumeGroupPowerStateWaitForStableStateAsync(
                LMCGroupPowerStateWaitContinuation continuation,
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumeGroupPowerStateWaitForStableStateAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public Task<LMCGroupPowerStateWaitResult>
            GroupPowerOnAndWaitForStableStateAsync(
                CancellationToken cancellationToken)
        {
            return GroupPowerOnAndWaitForStableStateAsync(
                new LMCGroupPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitResult>
            GroupPowerOnAndWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken)
        {
            return GroupPowerStateAndWaitForStableStateAsync(
                true,
                options,
                null,
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitResult>
            GroupPowerOnAndWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            return GroupPowerStateAndWaitForStableStateAsync(
                true,
                options,
                acceptedContinuationObserver,
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitResult>
            GroupPowerOffAndWaitForStableStateAsync(
                CancellationToken cancellationToken)
        {
            return GroupPowerOffAndWaitForStableStateAsync(
                new LMCGroupPowerStateWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitResult>
            GroupPowerOffAndWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken)
        {
            return GroupPowerStateAndWaitForStableStateAsync(
                false,
                options,
                null,
                cancellationToken);
        }

        public Task<LMCGroupPowerStateWaitResult>
            GroupPowerOffAndWaitForStableStateAsync(
                LMCGroupPowerStateWaitOptions options,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            if (acceptedContinuationObserver == null)
            {
                throw new ArgumentNullException(
                    "acceptedContinuationObserver");
            }

            return GroupPowerStateAndWaitForStableStateAsync(
                false,
                options,
                acceptedContinuationObserver,
                cancellationToken);
        }

        /// <summary>
        /// Polls GroupReadStatus until the requested power state is observed for
        /// three consecutive successful samples. This helper is read-only and
        /// never sends GroupPowerOn or GroupPowerOff.
        /// </summary>
        public Task<LMCGroupPowerStateWaitResult> WaitForPowerStateAsync(
            bool expectedPowerOn,
            CancellationToken cancellationToken)
        {
            return WaitForPowerStateAsync(
                expectedPowerOn,
                new LMCGroupPowerStateWaitOptions(),
                cancellationToken);
        }

        /// <summary>
        /// Polls only GroupReadStatus under one total deadline that includes
        /// status-gate admission, wire exchange, and inter-poll delays.
        /// </summary>
        public Task<LMCGroupPowerStateWaitResult> WaitForPowerStateAsync(
            bool expectedPowerOn,
            LMCGroupPowerStateWaitOptions options,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return WaitForPowerStateAsync(
                expectedPowerOn,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public LMC_Response GroupReset()
        {
            return SendGroupReset();
        }

        public Task<LMC_Response> GroupResetAsync(
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupReset(GroupReference),
                cancellationToken);
        }

        public LMC_Response GroupStop(int deceleration, int jerk)
        {
            return SendGroupStop(deceleration, jerk);
        }

        public Task<LMC_Response> GroupStopAsync(
            int deceleration,
            int jerk,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupStop(GroupReference, deceleration, jerk),
                cancellationToken);
        }

        /// <summary>
        /// Sends exactly one 0x2085 GroupStop request and returns after its
        /// successful acknowledgement is preserved. No 0x2045 status read is
        /// sent by this method.
        /// </summary>
        public Task<LMCGroupStopWaitContinuation>
            BeginGroupStopWaitForStableStandbyAsync(
                int deceleration,
                int jerk,
                CancellationToken cancellationToken)
        {
            return BeginGroupStopWaitForStableStandbyAsync(
                deceleration,
                jerk,
                new LMCGroupStopWaitOptions(),
                cancellationToken);
        }

        public Task<LMCGroupStopWaitContinuation>
            BeginGroupStopWaitForStableStandbyAsync(
                int deceleration,
                int jerk,
                LMCGroupStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginGroupStopWaitForStableStandbyAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Resumes status-only 0x2045 polling for an accepted GroupStop. This
        /// method never sends another 0x2085 request.
        /// </summary>
        public Task<LMCGroupStopWaitResult>
            ResumeGroupStopWaitForStableStandbyAsync(
                LMCGroupStopWaitContinuation continuation,
                CancellationToken cancellationToken)
        {
            var options = new LMCGroupStopWaitOptions();
            if (continuation != null)
            {
                options.StableSampleCount =
                    continuation.RequiredStableSampleCount;
            }

            return ResumeGroupStopWaitForStableStandbyAsync(
                continuation,
                options,
                cancellationToken);
        }

        public Task<LMCGroupStopWaitResult>
            ResumeGroupStopWaitForStableStandbyAsync(
                LMCGroupStopWaitContinuation continuation,
                LMCGroupStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return ResumeGroupStopWaitForStableStandbyAsync(
                continuation,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        /// <summary>
        /// Sends GroupStop exactly once, then polls only GroupReadStatus until
        /// LASAL group standby is observed for three consecutive samples.
        /// </summary>
        public Task<LMCGroupStopWaitResult>
            GroupStopAndWaitForStableStandbyAsync(
                int deceleration,
                int jerk,
                CancellationToken cancellationToken)
        {
            return GroupStopAndWaitForStableStandbyAsync(
                deceleration,
                jerk,
                new LMCGroupStopWaitOptions(),
                cancellationToken);
        }

        /// <summary>
        /// Sends one GroupStop under a total deadline and verifies completion
        /// with status-only polling. Timeout and cancellation exceptions retain
        /// the accepted ACK and last parsed status when available.
        /// </summary>
        public Task<LMCGroupStopWaitResult>
            GroupStopAndWaitForStableStandbyAsync(
                int deceleration,
                int jerk,
                LMCGroupStopWaitOptions options,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return GroupStopAndWaitForStableStandbyAsync(
                deceleration,
                jerk,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        public uint GroupReadStatus()
        {
            LMC_Response response;
            return ReadGroupStatusValue(out response);
        }

        public uint GroupReadStatus(out LMC_Response response)
        {
            return ReadGroupStatusValue(out response);
        }

        public LMCGroupReadStatusResult GroupReadStatusResult()
        {
            EnsureCurrentSessionForUse();
            groupEnableWaitCoordinator.StatusObservationGate.Wait();
            try
            {
                var continuation = CaptureGroupEnableWaitObservationTarget();
                try
                {
                    var raw = connection.Exchange(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        sessionGeneration);
                    var result = LMCConnection.ParseGroupReadStatusResult(raw);
                    connection.PublishSendPriorityResult(
                        LMC_CommandId.GroupStatus,
                        () => ObserveGroupEnableWaitStatus(
                            continuation,
                            result));
                    return result;
                }
                catch
                {
                    ResetPendingGroupEnableProof(continuation);
                    throw;
                }
            }
            finally
            {
                groupEnableWaitCoordinator.StatusObservationGate.Release();
            }
        }

        public async Task<LMCGroupReadStatusResult> GroupReadStatusResultAsync(
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            await groupEnableWaitCoordinator.StatusObservationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                var continuation = CaptureGroupEnableWaitObservationTarget();
                try
                {
                    var raw = await connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(GroupReference),
                        sessionGeneration,
                        cancellationToken).ConfigureAwait(false);
                    var result = LMCConnection.ParseGroupReadStatusResult(raw);
                    connection.PublishSendPriorityResult(
                        LMC_CommandId.GroupStatus,
                        () => ObserveGroupEnableWaitStatus(
                            continuation,
                            result));
                    return result;
                }
                catch
                {
                    ResetPendingGroupEnableProof(continuation);
                    throw;
                }
            }
            finally
            {
                groupEnableWaitCoordinator.StatusObservationGate.Release();
            }
        }

        private async Task<LMCGroupEnableStatusPollOutcome>
            GroupReadStatusResultForEnableWaitAsync(
                LMCGroupEnableWaitContinuation expectedContinuation,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                int timeoutMilliseconds)
        {
            var remaining = timeoutMilliseconds - elapsedMilliseconds();
            if (remaining <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (TryCompleteLockedStandbyContinuationAtDeadline(
                    expectedContinuation,
                    elapsedMilliseconds()))
                {
                    return new LMCGroupEnableStatusPollOutcome(
                        expectedContinuation.LastObservedStatus,
                        true);
                }

                throw new LMCGroupEnableWaitPreWireTimeoutException();
            }

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
                    await groupEnableWaitCoordinator.StatusObservationGate
                        .WaitAsync(preWriteCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (TryCompleteLockedStandbyContinuationAtDeadline(
                        expectedContinuation,
                        elapsedMilliseconds()))
                    {
                        return new LMCGroupEnableStatusPollOutcome(
                            expectedContinuation.LastObservedStatus,
                            true);
                    }

                    throw new LMCGroupEnableWaitPreWireTimeoutException();
                }

                var mutationGateAcquired = false;
                try
                {
                    try
                    {
                        await groupEnableWaitCoordinator.MutationGate
                            .WaitAsync(preWriteCancellation.Token)
                            .ConfigureAwait(false);
                        mutationGateAcquired = true;
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupEnableWaitPreWireTimeoutException();
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (TryCompleteLockedStandbyContinuation(
                        expectedContinuation,
                        elapsedMilliseconds()))
                    {
                        return new LMCGroupEnableStatusPollOutcome(
                            expectedContinuation.LastObservedStatus,
                            true);
                    }

                    ThrowIfCanceledOrDeadlineExpiredBeforeWire(
                        cancellationToken,
                        deadlineCancellation,
                        elapsedMilliseconds,
                        timeoutMilliseconds);

                    var continuation = CaptureGroupEnableWaitObservationTarget();
                    if (!ReferenceEquals(
                        continuation,
                        expectedContinuation))
                    {
                        throw new LMCGroupEnableWaitResolvedException(
                            expectedContinuation,
                            elapsedMilliseconds());
                    }

                    try
                    {
                        var raw = await connection.ExchangeAsyncDrainAfterWrite(
                            LMC_Frame.LMCGroupReadStatus(GroupReference),
                            sessionGeneration,
                            preWriteCancellation.Token,
                            deadlineCancellation.Token,
                            () =>
                            {
                                ThrowIfCanceledOrDeadlineExpiredBeforeWire(
                                    cancellationToken,
                                    deadlineCancellation,
                                    elapsedMilliseconds,
                                    timeoutMilliseconds);

                                EnsureContinuationStillPending(
                                    expectedContinuation,
                                    elapsedMilliseconds());
                            },
                            null).ConfigureAwait(false);
                        var result =
                            LMCConnection.ParseGroupReadStatusResult(raw);
                        connection.PublishSessionBoundSendPriorityResult(
                            sessionGeneration,
                            LMC_CommandId.GroupStatus,
                            () => ObserveGroupEnableWaitStatus(
                                continuation,
                                result));
                        ThrowIfCanceledOrDeadlineExpiredAfterWire(
                            cancellationToken,
                            deadlineCancellation,
                            elapsedMilliseconds,
                            timeoutMilliseconds);

                        var completed = result.IsSuccess
                            && TryCompleteLockedStandbyContinuation(
                                expectedContinuation,
                                elapsedMilliseconds());
                        return new LMCGroupEnableStatusPollOutcome(
                            result,
                            completed);
                    }
                    catch (LMCPostWriteDeadlineException)
                    {
                        continuation.MarkTransportInvalidatedAtDeadline();
                        ResetPendingGroupEnableProof(continuation);
                        throw new LMCGroupEnableWaitPreWireTimeoutException();
                    }
                    catch (OperationCanceledException)
                    {
                        ResetPendingGroupEnableProof(continuation);
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupEnableWaitPreWireTimeoutException();
                    }
                    catch
                    {
                        ResetPendingGroupEnableProof(continuation);
                        throw;
                    }
                }
                finally
                {
                    if (mutationGateAcquired)
                    {
                        groupEnableWaitCoordinator.MutationGate.Release();
                    }

                    groupEnableWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        public LMCGroupReadActualPositionResult GroupReadActualPosition(
            LMC_COORD_SYSTEM coordinateSystem)
        {
            EnsureCurrentSessionForUse();
            return LMCConnection.ParseGroupReadActualPositionResult(
                connection.Exchange(
                    LMC_Frame.LMCGroupReadActualPosition(
                        GroupReference,
                        coordinateSystem),
                    sessionGeneration),
                coordinateSystem);
        }

        public async Task<LMCGroupReadActualPositionResult>
            GroupReadActualPositionAsync(
                LMC_COORD_SYSTEM coordinateSystem,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            var raw = await connection.ExchangeAsync(
                LMC_Frame.LMCGroupReadActualPosition(
                    GroupReference,
                    coordinateSystem),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            return LMCConnection.ParseGroupReadActualPositionResult(
                raw,
                coordinateSystem);
        }

        public LMC_Response MoveLinearAbsoluteEx(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return SendMoveLinearAbsolute(position, velocity, acceleration, deceleration, jerk);
        }

        public Task<LMC_Response> MoveLinearAbsoluteExAsync(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            CancellationToken cancellationToken)
        {
            return MoveLinearAbsoluteExAsync(
                position,
                velocity,
                acceleration,
                deceleration,
                jerk,
                new LMCGroupMotionOptions(),
                cancellationToken);
        }

        public LMC_Response MoveLinearAbsoluteEx(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options)
        {
            return Send(
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    options));
        }

        public Task<LMC_Response> MoveLinearAbsoluteExAsync(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options,
            CancellationToken cancellationToken)
        {
            return SendAsync(
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk,
                    options),
                cancellationToken);
        }

        public LMCAdminResponse MoveLinearRelativeEx(
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return MoveLinearRelativeEx(
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                new LMCGroupMotionOptions());
        }

        public LMCAdminResponse MoveLinearRelativeEx(
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options)
        {
            return connection.Admin.GroupMoveLinearRelative(
                this,
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                options);
        }

        public LMCAdminResponse MoveLinearRelativeEx(
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options,
            LMCAdminCapabilities verifiedCapabilities)
        {
            return connection.Admin.GroupMoveLinearRelative(
                this,
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                options,
                verifiedCapabilities);
        }

        public Task<LMCAdminResponse> MoveLinearRelativeExAsync(
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            CancellationToken cancellationToken)
        {
            return MoveLinearRelativeExAsync(
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                new LMCGroupMotionOptions(),
                cancellationToken);
        }

        public Task<LMCAdminResponse> MoveLinearRelativeExAsync(
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options,
            CancellationToken cancellationToken)
        {
            return connection.Admin.GroupMoveLinearRelativeAsync(
                this,
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                options,
                cancellationToken);
        }

        public Task<LMCAdminResponse> MoveLinearRelativeExAsync(
            int[] distance,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk,
            LMCGroupMotionOptions options,
            LMCAdminCapabilities verifiedCapabilities,
            CancellationToken cancellationToken)
        {
            return connection.Admin.GroupMoveLinearRelativeAsync(
                this,
                distance,
                velocity,
                acceleration,
                deceleration,
                jerk,
                options,
                verifiedCapabilities,
                cancellationToken);
        }

        public LMC_Response SetKinTransformCartesian4Axis(
            LMCSingleAxis axisX,
            LMCSingleAxis axisY,
            LMCSingleAxis axisZ,
            LMCSingleAxis axisU)
        {
            ValidateKinematicAxes(axisX, axisY, axisZ, axisU);

            return SendShortAcknowledgement(
                LMC_Frame.LMCGroupSetKinTransformCartesian(
                    GroupReference,
                    LMCCartesianKinematicTransform.CreateFourAxis(
                        axisX.AxisReference,
                        axisY.AxisReference,
                        axisZ.AxisReference,
                        axisU.AxisReference)));
        }

        public Task<LMC_Response> SetKinTransformCartesian4AxisAsync(
            LMCSingleAxis axisX,
            LMCSingleAxis axisY,
            LMCSingleAxis axisZ,
            LMCSingleAxis axisU,
            CancellationToken cancellationToken)
        {
            ValidateKinematicAxes(axisX, axisY, axisZ, axisU);

            return SendShortAcknowledgementAsync(
                LMC_Frame.LMCGroupSetKinTransformCartesian(
                    GroupReference,
                    LMCCartesianKinematicTransform.CreateFourAxis(
                        axisX.AxisReference,
                        axisY.AxisReference,
                        axisZ.AxisReference,
                        axisU.AxisReference)),
                cancellationToken);
        }

        internal async Task<LMCGroupEnableWaitResult>
            GroupEnableAndWaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeGroupEnableWriteCommit = null,
                Action<LMCGroupEnableWaitContinuation>
                    acceptedContinuationObserver = null)
        {
            var validatedOptions = ValidateGroupEnableWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var continuation = await
                BeginGroupEnableWaitForLockedStandbyAsync(
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds,
                    delayAsync,
                    acceptedContinuationObserver,
                    beforeGroupEnableWriteCommit).ConfigureAwait(false);
            return await ResumeGroupEnableWaitForLockedStandbyAsync(
                continuation,
                validatedOptions,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                false).ConfigureAwait(false);
        }

        internal async Task<LMCGroupEnableWaitContinuation>
            BeginGroupEnableWaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action<LMCGroupEnableWaitContinuation>
                    acceptedContinuationObserver = null,
                Action beforeGroupEnableWriteCommit = null,
                Action beforeAcceptedContinuationPublication = null)
        {
            var validatedOptions = ValidateGroupEnableWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var submissionTracker =
                new LMCGroupEnableSubmissionTracker();
            var waitStarted = false;
            var mutationGateAcquired = false;
            var observerLatchSet = false;
            var observerInvocationActive = false;
            LMCGroupEnableWaitContinuation continuation = null;

            EnsureCurrentSessionForUse();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remaining = validatedOptions.TimeoutMilliseconds
                    - elapsedMilliseconds();
                if (remaining <= 0)
                {
                    throw new LMCGroupEnableWaitPreWireTimeoutException();
                }

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
                        await groupEnableWaitCoordinator.MutationGate
                            .WaitAsync(preWriteCancellation.Token)
                            .ConfigureAwait(false);
                        mutationGateAcquired = true;
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupEnableWaitPreWireTimeoutException();
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    if (elapsedMilliseconds()
                        >= validatedOptions.TimeoutMilliseconds)
                    {
                        throw new LMCGroupEnableWaitPreWireTimeoutException();
                    }

                    BeginNewGroupEnableWait();
                    waitStarted = true;

                    var publication = await SendGroupEnableForWaitAsync(
                        submissionTracker,
                        validatedOptions.StableSampleCount,
                        acceptedContinuationObserver != null,
                        cancellationToken,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        elapsedMilliseconds,
                        validatedOptions.TimeoutMilliseconds,
                        beforeGroupEnableWriteCommit,
                        beforeAcceptedContinuationPublication)
                        .ConfigureAwait(false);
                    continuation = publication.Continuation;
                    observerLatchSet = continuation != null
                        && acceptedContinuationObserver != null;

                    if (!publication.Acknowledgement.IsSuccess)
                    {
                        throw new LMCGroupEnableRejectedException(
                            submissionTracker.CaptureEvidence(
                                elapsedMilliseconds()));
                    }

                    if (acceptedContinuationObserver != null)
                    {
                        groupEnableWaitCoordinator.MutationGate.Release();
                        mutationGateAcquired = false;
                        observerInvocationActive = true;
                        acceptedContinuationObserver(continuation);
                        observerInvocationActive = false;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    if (elapsedMilliseconds()
                            >= validatedOptions.TimeoutMilliseconds
                        || (deadlineCancellation.IsCancellationRequested
                            && !cancellationToken.IsCancellationRequested))
                    {
                        ThrowGroupEnableWaitTimeout(
                            continuation,
                            elapsedMilliseconds());
                    }
                }

                return continuation;
            }
            catch (LMCGroupEnableWaitPreWireTimeoutException)
            {
                if (continuation == null)
                {
                    throw new LMCGroupEnableWaitTimeoutException(
                        submissionTracker.CaptureEvidence(
                            elapsedMilliseconds()),
                        null);
                }

                ThrowGroupEnableWaitTimeout(
                    continuation,
                    elapsedMilliseconds());
                throw;
            }
            catch (OperationCanceledException ex)
            {
                if (observerInvocationActive)
                {
                    throw;
                }

                var evidence = continuation == null
                    ? submissionTracker.CaptureEvidence(
                        elapsedMilliseconds())
                    : continuation.CaptureEvidence(
                        elapsedMilliseconds());
                if (continuation != null)
                {
                    continuation.ResetProofCounters();
                }

                throw new LMCGroupEnableWaitCanceledException(
                    evidence,
                    continuation,
                    ex,
                    cancellationToken);
            }
            finally
            {
                if (observerLatchSet)
                {
                    lock (groupEnableWaitCoordinator.Sync)
                    {
                        groupEnableWaitCoordinator
                            .EnableAcceptanceObserverInProgress = false;
                    }
                }

                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }

                if (waitStarted)
                {
                    EndGroupEnableWait();
                }
            }
        }

        internal async Task<LMCGroupStopWaitContinuation>
            BeginGroupStopWaitForStableStandbyAsync(
                int deceleration,
                int jerk,
                LMCGroupStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeAcceptedContinuationPublication = null,
                Action beforeStopWriteCommit = null)
        {
            var validatedOptions = ValidateGroupStopWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var stopRequest = LMC_Frame.LMCGroupStop(
                GroupReference,
                deceleration,
                jerk);
            var tracker = new LMCGroupStopWaitTracker(
                validatedOptions.StableSampleCount);
            LMCGroupStopWaitContinuation continuation = null;
            var statusGateAcquired = false;
            var mutationGateAcquired = false;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();
                await AcquireGroupStopStatusGateAsync(
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                statusGateAcquired = true;
                await AcquireGroupStopMutationGateAsync(
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;
                EnsureCurrentSessionForUse();

                var acknowledgement = await SendGroupStopForWaitAsync(
                    stopRequest,
                    tracker,
                    cancellationToken,
                    elapsedMilliseconds,
                    validatedOptions.TimeoutMilliseconds,
                    beforeStopWriteCommit).ConfigureAwait(false);
                if (!acknowledgement.IsSuccess)
                {
                    throw new LMCGroupStopRejectedException(
                        tracker.CaptureEvidence(elapsedMilliseconds()));
                }

                if (beforeAcceptedContinuationPublication != null)
                {
                    beforeAcceptedContinuationPublication();
                }

                continuation = new LMCGroupStopWaitContinuation(
                    groupEnableWaitCoordinator,
                    connection,
                    GroupName,
                    GroupReference,
                    sessionGeneration,
                    tracker);
                lock (groupEnableWaitCoordinator.Sync)
                {
                    var previous = groupEnableWaitCoordinator
                        .PendingStopContinuation;
                    if (previous != null && previous.IsPending)
                    {
                        previous.MarkSuperseded();
                    }

                    groupEnableWaitCoordinator.PendingStopContinuation =
                        continuation;
                }

                ThrowIfGroupStopWaitExpiredAfterPublication(
                    cancellationToken,
                    elapsedMilliseconds,
                    validatedOptions.TimeoutMilliseconds);
                return continuation;
            }
            catch (LMCGroupStopRejectedException)
            {
                throw;
            }
            catch (LMCGroupPowerStateWaitDeadlineException)
            {
                throw new LMCGroupStopWaitTimeoutException(
                    continuation == null
                        ? tracker.CaptureEvidence(elapsedMilliseconds())
                        : continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                    continuation);
            }
            catch (OperationCanceledException ex)
            {
                throw new LMCGroupStopWaitCanceledException(
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

                throw new LMCGroupStopSubmissionException(
                    tracker.CaptureEvidence(elapsedMilliseconds()),
                    ex);
            }
            finally
            {
                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }

                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator.StatusObservationGate
                        .Release();
                }
            }
        }

        internal async Task<LMCGroupStopWaitResult>
            ResumeGroupStopWaitForStableStandbyAsync(
                LMCGroupStopWaitContinuation continuation,
                LMCGroupStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null,
                Action beforeTransportInvalidatedPublication = null)
        {
            var validatedOptions = ValidateGroupStopWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            EnsureGroupStopContinuationOwner(continuation);
            if (validatedOptions.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted GroupStop continuation.",
                    "options");
            }

            var waitRegistered = false;
            var waitCompleted = false;
            try
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCGroupStopWaitCanceledException(
                        continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                        continuation,
                        ex,
                        cancellationToken);
                }
                BeginResumeGroupStopWait(continuation);
                waitRegistered = true;

                while (true)
                {
                    ThrowIfGroupStopWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds);

                    LMCGroupReadStatusResult status;
                    try
                    {
                        status = await
                            GroupReadStatusResultForPowerStateWaitAsync(
                                continuation.Tracker,
                                cancellationToken,
                                elapsedMilliseconds,
                                validatedOptions.TimeoutMilliseconds,
                                continuation,
                                beforeStatusResultPublication,
                                afterStatusResultPublication,
                                beforeStatusCoordinatorLock,
                                beforeTransportInvalidatedPublication)
                            .ConfigureAwait(false);
                    }
                    catch (LMCGroupStopInterferenceException)
                    {
                        throw;
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new LMCGroupStopWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupStopWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCGroupStopStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            null,
                            ex);
                    }

                    if (continuation.IsCompleted)
                    {
                        var evidence = continuation.CaptureEvidence(
                            elapsedMilliseconds());
                        waitCompleted = true;
                        return new LMCGroupStopWaitResult(
                            evidence,
                            continuation);
                    }

                    ThrowIfGroupStopWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds);

                    if (!status.IsSuccess)
                    {
                        throw new LMCGroupStopStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            status,
                            null);
                    }

                    try
                    {
                        await DelayGroupStopWaitAsync(
                            validatedOptions,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new LMCGroupStopWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupStopWaitCanceledException(
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
                        if (!waitCompleted)
                        {
                            continuation.ResetProofCounters();
                            var enableContinuation =
                                groupEnableWaitCoordinator
                                    .PendingContinuation;
                            if (enableContinuation != null
                                && enableContinuation.IsPending)
                            {
                                enableContinuation.ResetProofCounters();
                            }
                        }

                        groupEnableWaitCoordinator.StopWaitInProgress = false;
                    }
                }
            }
        }

        internal async Task<LMCGroupStopWaitResult>
            GroupStopAndWaitForStableStandbyAsync(
                int deceleration,
                int jerk,
                LMCGroupStopWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStopWriteCommit = null,
                Action afterAcceptedContinuationPublication = null)
        {
            var continuation = await
                BeginGroupStopWaitForStableStandbyAsync(
                    deceleration,
                    jerk,
                    options,
                    cancellationToken,
                    elapsedMilliseconds,
                    delayAsync,
                    null,
                    beforeStopWriteCommit).ConfigureAwait(false);
            if (afterAcceptedContinuationPublication != null)
            {
                afterAcceptedContinuationPublication();
            }

            return await ResumeGroupStopWaitForStableStandbyAsync(
                continuation,
                options,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync).ConfigureAwait(false);
        }

        private async Task<LMC_Response> SendGroupStopForWaitAsync(
            byte[] stopRequest,
            LMCGroupStopWaitTracker tracker,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds,
            Action beforeStopWriteCommit)
        {
            var remaining = timeoutMilliseconds - elapsedMilliseconds();
            if (remaining <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new LMCGroupPowerStateWaitDeadlineException();
            }

            using (var deadlineCancellation =
                new CancellationTokenSource())
            using (var preWriteCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    deadlineCancellation.Token))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                byte[] raw;
                try
                {
                    raw = await connection.ExchangeAsyncDrainAfterWrite(
                        stopRequest,
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () =>
                        {
                            ThrowIfGroupStopWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                timeoutMilliseconds);
                            if (beforeStopWriteCommit != null)
                            {
                                beforeStopWriteCommit();
                            }
                        },
                        () =>
                        {
                            tracker.MarkSubmissionOutcomeUncertain();
                            tracker.SetStopMutationGeneration(
                                groupEnableWaitCoordinator
                                    .MarkMutationMayHaveBeenSent());
                            groupEnableWaitCoordinator
                                .ResetPendingMutationProof();
                        }).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }

                var acknowledgement =
                    LMCConnection.ParseCommandAcknowledgement(
                        raw,
                        "GroupStop");
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.GroupStop,
                    () => tracker.SetAcknowledgement(acknowledgement));
                return acknowledgement;
            }
        }

        private async Task AcquireGroupStopStatusGateAsync(
            LMCGroupStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetGroupStopWaitRemaining(
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
                    await groupEnableWaitCoordinator.StatusObservationGate
                        .WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
            }
        }

        private async Task AcquireGroupStopMutationGateAsync(
            LMCGroupStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetGroupStopWaitRemaining(
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
                    await groupEnableWaitCoordinator.MutationGate
                        .WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
            }
        }

        private void EnsureGroupStopContinuationOwner(
            LMCGroupStopWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupStopContinuationOwnerCore(continuation);
            }
        }

        private void EnsureGroupStopContinuationOwnerCore(
            LMCGroupStopWaitContinuation continuation)
        {
            if (continuation == null
                || !continuation.IsPending
                || !ReferenceEquals(
                    continuation.Coordinator,
                    groupEnableWaitCoordinator)
                || !continuation.BelongsTo(
                    connection,
                    sessionGeneration,
                    GroupReference)
                || !ReferenceEquals(
                    groupEnableWaitCoordinator.PendingStopContinuation,
                    continuation))
            {
                throw new InvalidOperationException(
                    "The GroupStop continuation does not belong to this active connection, session, group, or latest pending operation.");
            }
        }

        private void BeginResumeGroupStopWait(
            LMCGroupStopWaitContinuation continuation)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupStopContinuationOwnerCore(continuation);
                if (groupEnableWaitCoordinator.StopWaitInProgress)
                {
                    throw new InvalidOperationException(
                        "Another GroupStop status-only wait is already running.");
                }

                groupEnableWaitCoordinator.StopWaitInProgress = true;
                continuation.ResetProofCounters();
                var enableContinuation =
                    groupEnableWaitCoordinator.PendingContinuation;
                if (enableContinuation != null
                    && enableContinuation.IsPending)
                {
                    enableContinuation.ResetProofCounters();
                }
            }
        }

        private static long GetGroupStopWaitRemaining(
            LMCGroupStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCGroupPowerStateWaitDeadlineException();
            }

            return remaining;
        }

        private static async Task DelayGroupStopWaitAsync(
            LMCGroupStopWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = options.TimeoutMilliseconds - elapsedMilliseconds();
            if (remaining <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new LMCGroupPowerStateWaitDeadlineException();
            }

            using (var deadlineCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    await delayAsync(
                        Math.Min(options.PollIntervalMilliseconds, (int)remaining),
                        deadlineCancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (elapsedMilliseconds() >= options.TimeoutMilliseconds
                    || (deadlineCancellation.IsCancellationRequested
                        && !cancellationToken.IsCancellationRequested))
                {
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
            }
        }

        private void ThrowIfGroupStopWaitMutationIntervened(
            LMCGroupStopWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            var actualGeneration =
                groupEnableWaitCoordinator.MutationGeneration;
            continuation.ObserveMutationGeneration(actualGeneration);
            if (continuation.StopMutationGeneration <= 0
                || actualGeneration != continuation.StopMutationGeneration)
            {
                throw new LMCGroupStopInterferenceException(
                    continuation.CaptureEvidence(elapsedMilliseconds()),
                    continuation);
            }
        }

        private static bool CanResolveGroupStopAtPublication(
            CancellationToken cancellationToken,
            CancellationTokenSource deadlineCancellation,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            return !cancellationToken.IsCancellationRequested
                && !deadlineCancellation.IsCancellationRequested
                && elapsedMilliseconds() < timeoutMilliseconds;
        }

        private static LMCGroupStopWaitOptions ValidateGroupStopWaitOptions(
            LMCGroupStopWaitOptions options,
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

        private static void ThrowIfGroupStopWaitCannotStartWire(
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
                throw new LMCGroupPowerStateWaitDeadlineException();
            }
        }

        private static void ThrowIfGroupStopWaitExpiredAfterWire(
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
                throw new LMCGroupPowerStateWaitDeadlineException();
            }
        }

        private static void ThrowIfGroupStopWaitExpiredAfterPublication(
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds)
            {
                throw new LMCGroupPowerStateWaitDeadlineException();
            }
        }

        private Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerStateWaitForStableStateAsync(
                bool expectedPowerOn,
                LMCGroupPowerStateWaitOptions options,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return BeginGroupPowerStateWaitForStableStateAsync(
                expectedPowerOn,
                options,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync,
                acceptedContinuationObserver);
        }

        internal async Task<LMCGroupPowerStateWaitContinuation>
            BeginGroupPowerStateWaitForStableStateAsync(
                bool expectedPowerOn,
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver = null,
                Action beforePowerWriteCommit = null,
                Action beforeAcceptedContinuationPublication = null,
                Action afterPowerAcknowledgementParsed = null)
        {
            var validatedOptions = ValidateGroupPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCGroupPowerStateWaitTracker(
                expectedPowerOn,
                validatedOptions.StableSampleCount);
            LMCGroupPowerStateWaitContinuation continuation = null;
            var statusGateAcquired = false;
            var mutationGateAcquired = false;
            var observerLatchSet = false;
            var observerInvocationActive = false;

            try
            {
                EnsureCurrentSessionForUse();
                cancellationToken.ThrowIfCancellationRequested();
                await AcquireGroupPowerStateStatusGateAsync(
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                statusGateAcquired = true;
                await AcquireGroupPowerStateMutationGateAsync(
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds).ConfigureAwait(false);
                mutationGateAcquired = true;
                EnsureCurrentSessionForUse();

                lock (groupEnableWaitCoordinator.Sync)
                {
                    if (groupEnableWaitCoordinator
                        .PowerAcceptanceObserverInProgress)
                    {
                        throw new InvalidOperationException(
                            "An accepted Group Power observer is still running.");
                    }

                    var pending = groupEnableWaitCoordinator
                        .PendingPowerStateContinuation;
                    if (expectedPowerOn
                        && pending != null
                        && pending.IsPending)
                    {
                        throw new LMCGroupPowerStateWaitPendingException(
                            pending);
                    }
                }

                var request = expectedPowerOn
                    ? LMC_Frame.LMCGroupPowerOn(GroupReference)
                    : LMC_Frame.LMCGroupPowerOff(GroupReference);
                var publication = await SendGroupPowerForWaitAsync(
                    request,
                    tracker,
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds,
                    beforePowerWriteCommit,
                    beforeAcceptedContinuationPublication,
                    acceptedContinuationObserver != null,
                    afterPowerAcknowledgementParsed)
                    .ConfigureAwait(false);
                continuation = publication.Continuation;
                observerLatchSet = continuation != null
                    && acceptedContinuationObserver != null;
                if (!publication.Acknowledgement.IsSuccess)
                {
                    throw new LMCGroupPowerRejectedException(
                        tracker.CaptureEvidence(elapsedMilliseconds()));
                }

                if (acceptedContinuationObserver != null)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                    mutationGateAcquired = false;
                    groupEnableWaitCoordinator.StatusObservationGate
                        .Release();
                    statusGateAcquired = false;
                    observerInvocationActive = true;
                    acceptedContinuationObserver(continuation);
                    observerInvocationActive = false;
                }

                ThrowIfGroupPowerStateWaitExpiredAfterPublication(
                    cancellationToken,
                    elapsedMilliseconds,
                    validatedOptions.TimeoutMilliseconds);
                return continuation;
            }
            catch (LMCGroupPowerRejectedException)
            {
                throw;
            }
            catch (LMCGroupPowerStateWaitPendingException)
            {
                throw;
            }
            catch (LMCGroupPowerStateWaitDeadlineException)
            {
                throw new LMCGroupPowerStateWaitTimeoutException(
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

                throw new LMCGroupPowerStateWaitCanceledException(
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

                throw new LMCGroupPowerSubmissionException(
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
                            .PowerAcceptanceObserverInProgress = false;
                    }
                }

                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }

                if (statusGateAcquired)
                {
                    groupEnableWaitCoordinator.StatusObservationGate
                        .Release();
                }
            }
        }

        private Task<LMCGroupPowerStateWaitResult>
            GroupPowerStateAndWaitForStableStateAsync(
                bool expectedPowerOn,
                LMCGroupPowerStateWaitOptions options,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            return GroupPowerStateAndWaitForStableStateAsync(
                expectedPowerOn,
                options,
                acceptedContinuationObserver,
                cancellationToken,
                () => stopwatch.ElapsedMilliseconds,
                DelayAsync);
        }

        internal async Task<LMCGroupPowerStateWaitResult>
            GroupPowerStateAndWaitForStableStateAsync(
                bool expectedPowerOn,
                LMCGroupPowerStateWaitOptions options,
                Action<LMCGroupPowerStateWaitContinuation>
                    acceptedContinuationObserver,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforePowerWriteCommit = null,
                Action beforeAcceptedContinuationPublication = null,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null,
                Action afterPowerAcknowledgementParsed = null)
        {
            var validatedOptions = ValidateGroupPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var continuation = await
                BeginGroupPowerStateWaitForStableStateAsync(
                    expectedPowerOn,
                    validatedOptions,
                    cancellationToken,
                    elapsedMilliseconds,
                    delayAsync,
                    acceptedContinuationObserver,
                    beforePowerWriteCommit,
                    beforeAcceptedContinuationPublication,
                    afterPowerAcknowledgementParsed)
                .ConfigureAwait(false);
            return await ResumeGroupPowerStateWaitForStableStateAsync(
                continuation,
                validatedOptions,
                cancellationToken,
                elapsedMilliseconds,
                delayAsync,
                beforeStatusResultPublication,
                afterStatusResultPublication,
                beforeStatusCoordinatorLock).ConfigureAwait(false);
        }

        internal async Task<LMCGroupPowerStateWaitResult>
            ResumeGroupPowerStateWaitForStableStateAsync(
                LMCGroupPowerStateWaitContinuation continuation,
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null,
                Action beforeTransportInvalidatedPublication = null)
        {
            var validatedOptions = ValidateGroupPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            EnsureGroupPowerStateContinuationOwner(continuation);
            if (validatedOptions.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted Group Power continuation.",
                    "options");
            }

            var waitRegistered = false;
            var waitCompleted = false;
            try
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException ex)
                {
                    throw new LMCGroupPowerStateWaitCanceledException(
                        continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                        continuation,
                        ex,
                        cancellationToken);
                }

                BeginResumeGroupPowerStateWait(continuation);
                waitRegistered = true;

                while (true)
                {
                    ThrowIfGroupPowerStateWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds);

                    LMCGroupReadStatusResult status;
                    try
                    {
                        status = await ReadGroupStatusForAcceptedPowerWaitAsync(
                            continuation,
                            validatedOptions,
                            cancellationToken,
                            elapsedMilliseconds,
                            beforeStatusResultPublication,
                            afterStatusResultPublication,
                            beforeStatusCoordinatorLock,
                            beforeTransportInvalidatedPublication)
                            .ConfigureAwait(false);
                    }
                    catch (LMCGroupPowerInterferenceException)
                    {
                        throw;
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new LMCGroupPowerStateWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupPowerStateWaitCanceledException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCGroupPowerStateStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            null,
                            ex);
                    }

                    if (continuation.IsCompleted)
                    {
                        waitCompleted = true;
                        return new LMCGroupPowerStateWaitResult(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }

                    ThrowIfGroupPowerStateWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds);
                    if (!status.IsSuccess)
                    {
                        throw new LMCGroupPowerStateStatusException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation,
                            status,
                            null);
                    }

                    try
                    {
                        await DelayGroupPowerStateWaitAsync(
                            validatedOptions,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new LMCGroupPowerStateWaitTimeoutException(
                            continuation.CaptureEvidence(
                                elapsedMilliseconds()),
                            continuation);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupPowerStateWaitCanceledException(
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
                            continuation.ResetProofCounters();
                            var enableContinuation =
                                groupEnableWaitCoordinator
                                    .PendingContinuation;
                            if (enableContinuation != null
                                && enableContinuation.IsPending)
                            {
                                enableContinuation.ResetProofCounters();
                            }
                        }

                        groupEnableWaitCoordinator
                            .PowerStateWaitInProgress = false;
                    }
                }
            }
        }

        private async Task<LMCGroupPowerSubmissionPublication>
            SendGroupPowerForWaitAsync(
            byte[] request,
            LMCGroupPowerStateWaitTracker tracker,
            LMCGroupPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Action beforePowerWriteCommit,
            Action beforeAcceptedContinuationPublication,
            bool acceptanceObserverWillRun,
            Action afterPowerAcknowledgementParsed)
        {
            var remaining = GetGroupPowerStateWaitRemaining(
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
                byte[] raw;
                try
                {
                    raw = await connection.ExchangeAsyncDrainAfterWrite(
                        request,
                        sessionGeneration,
                        preWriteCancellation.Token,
                        deadlineCancellation.Token,
                        () =>
                        {
                            ThrowIfGroupPowerStateWaitCannotStartWire(
                                cancellationToken,
                                deadlineCancellation,
                                elapsedMilliseconds,
                                options.TimeoutMilliseconds);
                            if (beforePowerWriteCommit != null)
                            {
                                beforePowerWriteCommit();
                            }
                        },
                        () =>
                        {
                            tracker.MarkSubmissionOutcomeUncertain();
                            tracker.SetPowerMutationGeneration(
                                groupEnableWaitCoordinator
                                    .MarkMutationMayHaveBeenSent());
                            groupEnableWaitCoordinator
                                .ResetPendingMutationProof();
                        }).ConfigureAwait(false);
                }
                catch (LMCPostWriteDeadlineException)
                {
                    tracker.MarkTransportInvalidatedAtDeadline();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }

                var acknowledgement =
                    LMCConnection.ParseCommandAcknowledgement(
                        raw,
                        tracker.ExpectedPowerOn
                            ? "GroupPowerOn"
                            : "GroupPowerOff");
                if (afterPowerAcknowledgementParsed != null)
                {
                    afterPowerAcknowledgementParsed();
                }

                LMCGroupPowerStateWaitContinuation continuation = null;
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_Frame.GetRequestCommand(request),
                    () =>
                    {
                        lock (groupEnableWaitCoordinator.Sync)
                        {
                            tracker.SetAcknowledgement(acknowledgement);
                            if (!acknowledgement.IsSuccess)
                            {
                                return;
                            }

                            if (beforeAcceptedContinuationPublication
                                != null)
                            {
                                beforeAcceptedContinuationPublication();
                            }

                            continuation =
                                new LMCGroupPowerStateWaitContinuation(
                                    groupEnableWaitCoordinator,
                                    connection,
                                    GroupName,
                                    GroupReference,
                                    sessionGeneration,
                                    tracker);
                            continuation.ObserveMutationGeneration(
                                groupEnableWaitCoordinator
                                    .MutationGeneration);
                            var previous = groupEnableWaitCoordinator
                                .PendingPowerStateContinuation;
                            if (previous != null && previous.IsPending)
                            {
                                previous.MarkSuperseded();
                            }

                            groupEnableWaitCoordinator
                                .PendingPowerStateContinuation =
                                continuation;
                            groupEnableWaitCoordinator
                                .PowerAcceptanceObserverInProgress =
                                acceptanceObserverWillRun;
                        }
                    });
                return new LMCGroupPowerSubmissionPublication(
                    acknowledgement,
                    continuation);
            }
        }

        private sealed class LMCGroupPowerSubmissionPublication
        {
            internal LMCGroupPowerSubmissionPublication(
                LMC_Response acknowledgement,
                LMCGroupPowerStateWaitContinuation continuation)
            {
                Acknowledgement = acknowledgement
                    ?? throw new ArgumentNullException("acknowledgement");
                Continuation = continuation;
            }

            internal LMC_Response Acknowledgement { get; private set; }
            internal LMCGroupPowerStateWaitContinuation Continuation
            {
                get;
                private set;
            }
        }

        private async Task<LMCGroupReadStatusResult>
            ReadGroupStatusForAcceptedPowerWaitAsync(
                LMCGroupPowerStateWaitContinuation continuation,
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Action beforeStatusResultPublication,
                Action afterStatusResultPublication,
                Action beforeStatusCoordinatorLock,
                Action beforeTransportInvalidatedPublication)
        {
            var remaining = GetGroupPowerStateWaitRemaining(
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
                var statusGateAcquired = false;
                var mutationGateAcquired = false;
                LMCGroupEnableWaitContinuation enableContinuation = null;
                try
                {
                    try
                    {
                        await groupEnableWaitCoordinator
                            .StatusObservationGate
                            .WaitAsync(preWriteCancellation.Token)
                            .ConfigureAwait(false);
                        statusGateAcquired = true;
                        await groupEnableWaitCoordinator.MutationGate
                            .WaitAsync(preWriteCancellation.Token)
                            .ConfigureAwait(false);
                        mutationGateAcquired = true;
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupPowerStateWaitDeadlineException();
                    }

                    EnsureGroupPowerStateContinuationOwner(continuation);
                    ThrowIfGroupPowerStateWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds);
                    enableContinuation =
                        CaptureGroupEnableWaitObservationTarget();

                    byte[] raw;
                    try
                    {
                        raw = await connection.ExchangeAsyncDrainAfterWrite(
                            LMC_Frame.LMCGroupReadStatus(GroupReference),
                            sessionGeneration,
                            preWriteCancellation.Token,
                            deadlineCancellation.Token,
                            () =>
                            {
                                ThrowIfGroupPowerStateWaitCannotStartWire(
                                    cancellationToken,
                                    deadlineCancellation,
                                    elapsedMilliseconds,
                                    options.TimeoutMilliseconds);
                                EnsureGroupPowerStateContinuationOwner(
                                    continuation);
                                ThrowIfGroupPowerStateWaitMutationIntervened(
                                    continuation,
                                    elapsedMilliseconds);
                            },
                            null).ConfigureAwait(false);
                    }
                    catch (LMCPostWriteDeadlineException)
                    {
                        if (beforeTransportInvalidatedPublication != null)
                        {
                            beforeTransportInvalidatedPublication();
                        }

                        continuation.MarkTransportInvalidatedAtDeadline();
                        throw new LMCGroupPowerStateWaitDeadlineException();
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupPowerStateWaitDeadlineException();
                    }

                    var result =
                        LMCConnection.ParseGroupReadStatusResult(raw);
                    if (beforeStatusResultPublication != null)
                    {
                        beforeStatusResultPublication();
                    }

                    var completed = false;
                    connection.PublishSessionBoundSendPriorityResult(
                        sessionGeneration,
                        LMC_CommandId.GroupStatus,
                        () =>
                        {
                            if (beforeStatusCoordinatorLock != null)
                            {
                                groupEnableWaitCoordinator.MutationGate
                                    .Release();
                                mutationGateAcquired = false;
                                try
                                {
                                    beforeStatusCoordinatorLock();
                                }
                                finally
                                {
                                    groupEnableWaitCoordinator.MutationGate
                                        .Wait();
                                    mutationGateAcquired = true;
                                }
                            }

                            lock (groupEnableWaitCoordinator.Sync)
                            {
                                EnsureGroupPowerStateContinuationOwnerCore(
                                    continuation);
                                var actualMutationGeneration =
                                    groupEnableWaitCoordinator
                                        .MutationGeneration;
                                continuation.ObserveMutationGeneration(
                                    actualMutationGeneration);
                                if (continuation.PowerMutationGeneration <= 0
                                    || actualMutationGeneration
                                        != continuation
                                            .PowerMutationGeneration)
                                {
                                    throw new
                                        LMCGroupPowerInterferenceException(
                                            continuation.CaptureEvidence(
                                                elapsedMilliseconds()),
                                            continuation);
                                }

                                ObserveGroupEnableWaitStatus(
                                    enableContinuation,
                                    result);
                                continuation.Observe(result);
                                if (continuation.HasStableProof
                                    && CanResolveGroupPowerStateAtPublication(
                                        cancellationToken,
                                        deadlineCancellation,
                                        elapsedMilliseconds,
                                        options.TimeoutMilliseconds))
                                {
                                    continuation.MarkCompleted();
                                    groupEnableWaitCoordinator
                                        .PendingPowerStateContinuation = null;
                                    completed = true;
                                }
                            }
                        });

                    if (afterStatusResultPublication != null)
                    {
                        afterStatusResultPublication();
                    }

                    if (completed)
                    {
                        return result;
                    }

                    ThrowIfGroupPowerStateWaitExpiredAfterWire(
                        cancellationToken,
                        deadlineCancellation,
                        elapsedMilliseconds,
                        options.TimeoutMilliseconds);
                    ThrowIfGroupPowerStateWaitMutationIntervened(
                        continuation,
                        elapsedMilliseconds);
                    return result;
                }
                catch
                {
                    ResetPendingGroupEnableProof(enableContinuation);
                    throw;
                }
                finally
                {
                    if (mutationGateAcquired)
                    {
                        groupEnableWaitCoordinator.MutationGate.Release();
                    }

                    if (statusGateAcquired)
                    {
                        groupEnableWaitCoordinator.StatusObservationGate
                            .Release();
                    }
                }
            }
        }

        private void BeginResumeGroupPowerStateWait(
            LMCGroupPowerStateWaitContinuation continuation)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupPowerStateContinuationOwnerCore(continuation);
                if (groupEnableWaitCoordinator
                    .PowerAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "The accepted Group Power observer is still running.");
                }
                if (groupEnableWaitCoordinator.PowerStateWaitInProgress)
                {
                    throw new InvalidOperationException(
                        "Another Group Power status-only wait is already running.");
                }

                groupEnableWaitCoordinator.PowerStateWaitInProgress = true;
                continuation.ResetProofCounters();
                var enableContinuation =
                    groupEnableWaitCoordinator.PendingContinuation;
                if (enableContinuation != null
                    && enableContinuation.IsPending)
                {
                    enableContinuation.ResetProofCounters();
                }
            }
        }

        private void EnsureGroupPowerStateContinuationOwner(
            LMCGroupPowerStateWaitContinuation continuation)
        {
            EnsureCurrentSessionForUse();
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupPowerStateContinuationOwnerCore(continuation);
            }
        }

        private void EnsureGroupPowerStateContinuationOwnerCore(
            LMCGroupPowerStateWaitContinuation continuation)
        {
            if (continuation == null
                || !continuation.IsPending
                || !ReferenceEquals(
                    continuation.Coordinator,
                    groupEnableWaitCoordinator)
                || !continuation.BelongsTo(
                    connection,
                    sessionGeneration,
                    GroupReference)
                || !ReferenceEquals(
                    groupEnableWaitCoordinator
                        .PendingPowerStateContinuation,
                    continuation))
            {
                throw new LMCGroupPowerStateWaitResolvedException(
                    continuation);
            }
        }

        private void ThrowIfGroupPowerStateWaitMutationIntervened(
            LMCGroupPowerStateWaitContinuation continuation,
            Func<long> elapsedMilliseconds)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureGroupPowerStateContinuationOwnerCore(continuation);
                var actualGeneration = groupEnableWaitCoordinator
                    .MutationGeneration;
                continuation.ObserveMutationGeneration(actualGeneration);
                if (continuation.PowerMutationGeneration <= 0
                    || actualGeneration
                        != continuation.PowerMutationGeneration)
                {
                    throw new LMCGroupPowerInterferenceException(
                        continuation.CaptureEvidence(
                            elapsedMilliseconds()),
                        continuation);
                }
            }
        }

        private async Task AcquireGroupPowerStateStatusGateAsync(
            LMCGroupPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetGroupPowerStateWaitRemaining(
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
                    await groupEnableWaitCoordinator.StatusObservationGate
                        .WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
            }
        }

        private async Task AcquireGroupPowerStateMutationGateAsync(
            LMCGroupPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            var remaining = GetGroupPowerStateWaitRemaining(
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
                    await groupEnableWaitCoordinator.MutationGate
                        .WaitAsync(deadlineCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
            }
        }

        private static long GetGroupPowerStateWaitRemaining(
            LMCGroupPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCGroupPowerStateWaitDeadlineException();
            }

            return remaining;
        }

        private static bool CanResolveGroupPowerStateAtPublication(
            CancellationToken cancellationToken,
            CancellationTokenSource deadlineCancellation,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            return !cancellationToken.IsCancellationRequested
                && !deadlineCancellation.IsCancellationRequested
                && elapsedMilliseconds() < timeoutMilliseconds;
        }

        private static void ThrowIfGroupPowerStateWaitExpiredAfterPublication(
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= timeoutMilliseconds)
            {
                throw new LMCGroupPowerStateWaitDeadlineException();
            }
        }

        internal async Task<LMCGroupLockedStandbyWaitResult>
            WaitForLockedStandbyAsync(
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            var validatedOptions = ValidateGroupEnableWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCGroupLockedStandbyWaitTracker(
                validatedOptions.StableSampleCount);

            EnsureCurrentSessionForUse();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                while (true)
                {
                    LMCGroupReadStatusResult status;
                    try
                    {
                        status = await
                            GroupReadStatusResultForPowerStateWaitAsync(
                                tracker,
                                cancellationToken,
                                elapsedMilliseconds,
                                validatedOptions.TimeoutMilliseconds)
                            .ConfigureAwait(false);
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new
                            LMCGroupLockedStandbyWaitTimeoutException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new
                            LMCGroupLockedStandbyWaitCanceledException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()),
                                ex,
                                cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCGroupLockedStandbyStatusException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()),
                            null,
                            ex);
                    }

                    if (!status.IsSuccess)
                    {
                        throw new LMCGroupLockedStandbyStatusException(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()),
                            status,
                            null);
                    }

                    if (tracker.HasStableProof)
                    {
                        return new LMCGroupLockedStandbyWaitResult(
                            tracker.CaptureEvidence(
                                elapsedMilliseconds()));
                    }

                    try
                    {
                        await DelayGroupLockedStandbyWaitAsync(
                            validatedOptions,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new
                            LMCGroupLockedStandbyWaitTimeoutException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new
                            LMCGroupLockedStandbyWaitCanceledException(
                                tracker.CaptureEvidence(
                                    elapsedMilliseconds()),
                                ex,
                                cancellationToken);
                    }
                }
            }
            catch (LMCGroupLockedStandbyWaitTimeoutException)
            {
                throw;
            }
            catch (LMCGroupLockedStandbyWaitCanceledException)
            {
                throw;
            }
            catch (LMCGroupLockedStandbyStatusException)
            {
                throw;
            }
            catch (OperationCanceledException ex)
            {
                throw new LMCGroupLockedStandbyWaitCanceledException(
                    tracker.CaptureEvidence(elapsedMilliseconds()),
                    ex,
                    cancellationToken);
            }
        }

        private static async Task DelayGroupLockedStandbyWaitAsync(
            LMCGroupEnableWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = options.TimeoutMilliseconds
                - elapsedMilliseconds();
            if (remaining <= 0)
            {
                throw new LMCGroupPowerStateWaitDeadlineException();
            }

            await delayAsync(
                Math.Min(options.PollIntervalMilliseconds, (int)remaining),
                cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (elapsedMilliseconds() >= options.TimeoutMilliseconds)
            {
                throw new LMCGroupPowerStateWaitDeadlineException();
            }
        }

        internal async Task<LMCGroupPowerStateWaitResult>
            WaitForPowerStateAsync(
                bool expectedPowerOn,
                LMCGroupPowerStateWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            var validatedOptions = ValidateGroupPowerStateWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);
            var tracker = new LMCGroupPowerStateWaitTracker(
                expectedPowerOn,
                validatedOptions.StableSampleCount);

            EnsureCurrentSessionForUse();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                while (true)
                {
                    LMCGroupReadStatusResult status;
                    try
                    {
                        status = await GroupReadStatusResultForPowerStateWaitAsync(
                            tracker,
                            cancellationToken,
                            elapsedMilliseconds,
                            validatedOptions.TimeoutMilliseconds)
                            .ConfigureAwait(false);
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new LMCGroupPowerStateWaitTimeoutException(
                            tracker.CaptureEvidence(elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupPowerStateWaitCanceledException(
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            ex,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new LMCGroupPowerStateStatusException(
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            null,
                            ex);
                    }

                    if (!status.IsSuccess)
                    {
                        throw new LMCGroupPowerStateStatusException(
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            status,
                            null);
                    }

                    if (tracker.HasStableProof)
                    {
                        return new LMCGroupPowerStateWaitResult(
                            tracker.CaptureEvidence(elapsedMilliseconds()));
                    }

                    try
                    {
                        await DelayGroupPowerStateWaitAsync(
                            validatedOptions,
                            cancellationToken,
                            elapsedMilliseconds,
                            delayAsync).ConfigureAwait(false);
                    }
                    catch (LMCGroupPowerStateWaitDeadlineException)
                    {
                        throw new LMCGroupPowerStateWaitTimeoutException(
                            tracker.CaptureEvidence(elapsedMilliseconds()));
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new LMCGroupPowerStateWaitCanceledException(
                            tracker.CaptureEvidence(elapsedMilliseconds()),
                            ex,
                            cancellationToken);
                    }
                }
            }
            catch (LMCGroupPowerStateWaitTimeoutException)
            {
                throw;
            }
            catch (LMCGroupPowerStateWaitCanceledException)
            {
                throw;
            }
            catch (LMCGroupPowerStateStatusException)
            {
                throw;
            }
            catch (OperationCanceledException ex)
            {
                throw new LMCGroupPowerStateWaitCanceledException(
                    tracker.CaptureEvidence(elapsedMilliseconds()),
                    ex,
                    cancellationToken);
            }
        }

        private async Task<LMCGroupReadStatusResult>
            GroupReadStatusResultForPowerStateWaitAsync(
                LMCGroupStatusWaitTracker tracker,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                int timeoutMilliseconds,
                LMCGroupStopWaitContinuation stopContinuation = null,
                Action beforeStatusResultPublication = null,
                Action afterStatusResultPublication = null,
                Action beforeStatusCoordinatorLock = null,
                Action beforeTransportInvalidatedPublication = null)
        {
            var remaining = timeoutMilliseconds - elapsedMilliseconds();
            if (remaining <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new LMCGroupPowerStateWaitDeadlineException();
            }

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
                    await groupEnableWaitCoordinator.StatusObservationGate
                        .WaitAsync(preWriteCancellation.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }

                var mutationGateAcquired = false;
                LMCGroupEnableWaitContinuation continuation = null;
                try
                {
                    try
                    {
                        await groupEnableWaitCoordinator.MutationGate
                            .WaitAsync(preWriteCancellation.Token)
                            .ConfigureAwait(false);
                        mutationGateAcquired = true;
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupPowerStateWaitDeadlineException();
                    }

                    if (stopContinuation != null)
                    {
                        EnsureGroupStopContinuationOwner(
                            stopContinuation);
                        ThrowIfGroupStopWaitMutationIntervened(
                            stopContinuation,
                            elapsedMilliseconds);
                    }

                    continuation = CaptureGroupEnableWaitObservationTarget();

                    byte[] raw;
                    try
                    {
                        raw = await connection.ExchangeAsyncDrainAfterWrite(
                            LMC_Frame.LMCGroupReadStatus(GroupReference),
                            sessionGeneration,
                            preWriteCancellation.Token,
                            deadlineCancellation.Token,
                            () =>
                            {
                                ThrowIfGroupPowerStateWaitCannotStartWire(
                                    cancellationToken,
                                    deadlineCancellation,
                                    elapsedMilliseconds,
                                    timeoutMilliseconds);
                                if (stopContinuation != null)
                                {
                                    EnsureGroupStopContinuationOwner(
                                        stopContinuation);
                                    ThrowIfGroupStopWaitMutationIntervened(
                                        stopContinuation,
                                        elapsedMilliseconds);
                                }
                            },
                            null).ConfigureAwait(false);
                    }
                    catch (LMCPostWriteDeadlineException)
                    {
                        if (beforeTransportInvalidatedPublication != null)
                        {
                            beforeTransportInvalidatedPublication();
                        }

                        if (stopContinuation == null)
                        {
                            tracker.MarkTransportInvalidatedAtDeadline();
                        }
                        else
                        {
                            stopContinuation
                                .MarkTransportInvalidatedAtDeadline();
                        }
                        throw new LMCGroupPowerStateWaitDeadlineException();
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new LMCGroupPowerStateWaitDeadlineException();
                    }

                    var result = LMCConnection.ParseGroupReadStatusResult(raw);
                    if (beforeStatusResultPublication != null)
                    {
                        beforeStatusResultPublication();
                    }

                    var stopCompleted = false;
                    connection.PublishSessionBoundSendPriorityResult(
                        sessionGeneration,
                        LMC_CommandId.GroupStatus,
                        () =>
                        {
                            if (beforeStatusCoordinatorLock != null)
                            {
                                groupEnableWaitCoordinator.MutationGate
                                    .Release();
                                mutationGateAcquired = false;
                                try
                                {
                                    beforeStatusCoordinatorLock();
                                }
                                finally
                                {
                                    groupEnableWaitCoordinator.MutationGate
                                        .Wait();
                                    mutationGateAcquired = true;
                                }
                            }

                            var stopTracker = tracker
                                as LMCGroupStopWaitTracker;
                            if (stopTracker == null)
                            {
                                ObserveGroupEnableWaitStatus(
                                    continuation,
                                    result);
                                tracker.Observe(result);
                                return;
                            }

                            long actualMutationGeneration;
                            var published = groupEnableWaitCoordinator
                                .TryPublishForMutationGeneration(
                                    stopTracker.StopMutationGeneration,
                                    () =>
                                    {
                                        EnsureGroupStopContinuationOwnerCore(
                                            stopContinuation);
                                        ObserveGroupEnableWaitStatus(
                                            continuation,
                                            result);
                                        stopContinuation.Observe(result);
                                        if (stopContinuation
                                                .HasStableStandbyProof
                                            && CanResolveGroupStopAtPublication(
                                                cancellationToken,
                                                deadlineCancellation,
                                                elapsedMilliseconds,
                                                timeoutMilliseconds))
                                        {
                                            stopContinuation.MarkCompleted();
                                            groupEnableWaitCoordinator
                                                .PendingStopContinuation = null;
                                            stopCompleted = true;
                                        }
                                    },
                                    out actualMutationGeneration);
                            stopContinuation.ObserveMutationGeneration(
                                actualMutationGeneration);
                            if (!published)
                            {
                                throw new LMCGroupStopInterferenceException(
                                    stopContinuation.CaptureEvidence(
                                        elapsedMilliseconds()),
                                    stopContinuation);
                            }
                        });

                    if (afterStatusResultPublication != null)
                    {
                        afterStatusResultPublication();
                    }

                    if (stopCompleted)
                    {
                        return result;
                    }

                    ThrowIfGroupPowerStateWaitExpiredAfterWire(
                        cancellationToken,
                        deadlineCancellation,
                        elapsedMilliseconds,
                        timeoutMilliseconds);
                    return result;
                }
                catch
                {
                    ResetPendingGroupEnableProof(continuation);
                    throw;
                }
                finally
                {
                    if (mutationGateAcquired)
                    {
                        groupEnableWaitCoordinator.MutationGate.Release();
                    }

                    groupEnableWaitCoordinator.StatusObservationGate.Release();
                }
            }
        }

        private static async Task DelayGroupPowerStateWaitAsync(
            LMCGroupPowerStateWaitOptions options,
            CancellationToken cancellationToken,
            Func<long> elapsedMilliseconds,
            Func<int, CancellationToken, Task> delayAsync)
        {
            var remaining = options.TimeoutMilliseconds - elapsedMilliseconds();
            if (remaining <= 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new LMCGroupPowerStateWaitDeadlineException();
            }

            using (var deadlineCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken))
            {
                deadlineCancellation.CancelAfter((int)remaining);
                try
                {
                    await delayAsync(
                        Math.Min(options.PollIntervalMilliseconds, (int)remaining),
                        deadlineCancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (elapsedMilliseconds() >= options.TimeoutMilliseconds
                    || (deadlineCancellation.IsCancellationRequested
                        && !cancellationToken.IsCancellationRequested))
                {
                    throw new LMCGroupPowerStateWaitDeadlineException();
                }
            }
        }

        private static LMCGroupPowerStateWaitOptions
            ValidateGroupPowerStateWaitOptions(
                LMCGroupPowerStateWaitOptions options,
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

        private static void ThrowIfGroupPowerStateWaitCannotStartWire(
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
                throw new LMCGroupPowerStateWaitDeadlineException();
            }
        }

        private static void ThrowIfGroupPowerStateWaitExpiredAfterWire(
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
                throw new LMCGroupPowerStateWaitDeadlineException();
            }
        }

        internal async Task<LMCGroupEnableWaitResult>
            ResumeGroupEnableWaitForLockedStandbyAsync(
                LMCGroupEnableWaitContinuation continuation,
                LMCGroupEnableWaitOptions options,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync,
                bool reusedAcceptedAcknowledgement = true)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            var validatedOptions = ValidateGroupEnableWaitOptions(
                options,
                elapsedMilliseconds,
                delayAsync);

            EnsureCurrentSessionForUse();
            ValidateResumeContinuation(
                continuation,
                validatedOptions,
                elapsedMilliseconds());

            var waitStarted = false;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                BeginResumeGroupEnableWait(
                    continuation,
                    validatedOptions,
                    elapsedMilliseconds());
                waitStarted = true;

                return await PollGroupEnableLockedStandbyAsync(
                    continuation,
                    validatedOptions,
                    reusedAcceptedAcknowledgement,
                    cancellationToken,
                    elapsedMilliseconds,
                    delayAsync).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                var canceled = new LMCGroupEnableWaitCanceledException(
                    continuation.CaptureEvidence(
                        elapsedMilliseconds()),
                    continuation,
                    ex,
                    cancellationToken);
                if (waitStarted)
                {
                    continuation.ResetProofCounters();
                }

                throw canceled;
            }
            finally
            {
                if (waitStarted)
                {
                    EndGroupEnableWait();
                }
            }
        }

        private async Task<LMCGroupEnableWaitResult>
            PollGroupEnableLockedStandbyAsync(
                LMCGroupEnableWaitContinuation continuation,
                LMCGroupEnableWaitOptions options,
                bool reusedAcceptedAcknowledgement,
                CancellationToken cancellationToken,
                Func<long> elapsedMilliseconds,
                Func<int, CancellationToken, Task> delayAsync)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                LMCGroupEnableStatusPollOutcome statusPoll;
                try
                {
                    statusPoll = await GroupReadStatusResultForEnableWaitAsync(
                        continuation,
                        cancellationToken,
                        elapsedMilliseconds,
                        options.TimeoutMilliseconds).ConfigureAwait(false);
                }
                catch (LMCGroupEnableWaitPreWireTimeoutException)
                {
                    ThrowGroupEnableWaitTimeout(
                        continuation,
                        elapsedMilliseconds());
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (LMCGroupEnableWaitResolvedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var statusException = new LMCGroupEnableStatusException(
                        continuation,
                        null,
                        ex,
                        elapsedMilliseconds());
                    continuation.ResetProofCounters();
                    throw statusException;
                }

                if (statusPoll.Completed)
                {
                    return new LMCGroupEnableWaitResult(
                        continuation,
                        statusPoll.Status,
                        reusedAcceptedAcknowledgement,
                        elapsedMilliseconds());
                }

                var status = statusPoll.Status;
                EnsureContinuationStillPending(
                    continuation,
                    elapsedMilliseconds());
                cancellationToken.ThrowIfCancellationRequested();

                if (!status.IsSuccess)
                {
                    throw new LMCGroupEnableStatusException(
                        continuation,
                        status,
                        null,
                        elapsedMilliseconds());
                }

                if (elapsedMilliseconds() >= options.TimeoutMilliseconds)
                {
                    ThrowGroupEnableWaitTimeout(
                        continuation,
                        elapsedMilliseconds());
                }

                var remaining = options.TimeoutMilliseconds
                    - elapsedMilliseconds();
                if (remaining <= 0)
                {
                    ThrowGroupEnableWaitTimeout(
                        continuation,
                        elapsedMilliseconds());
                }

                await delayAsync(
                    Math.Min(options.PollIntervalMilliseconds, (int)remaining),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        private static LMCGroupEnableWaitOptions ValidateGroupEnableWaitOptions(
            LMCGroupEnableWaitOptions options,
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

        private static Task DelayAsync(
            int delayMilliseconds,
            CancellationToken cancellationToken)
        {
            return Task.Delay(delayMilliseconds, cancellationToken);
        }

        private static void ThrowIfCanceledOrDeadlineExpiredBeforeWire(
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
                throw new LMCGroupEnableWaitPreWireTimeoutException();
            }
        }

        private static void ThrowIfCanceledOrDeadlineExpiredAfterWire(
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
                throw new LMCGroupEnableWaitPreWireTimeoutException();
            }
        }

        private void BeginNewGroupEnableWait()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                if (groupEnableWaitCoordinator
                    .EnableAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "An accepted GroupEnable observer is still running.");
                }

                if (groupEnableWaitCoordinator.WaitInProgress)
                {
                    throw new InvalidOperationException(
                        "A group-enable wait operation is already in progress.");
                }

                if (groupEnableWaitCoordinator.DirectEnableInProgress)
                {
                    throw new InvalidOperationException(
                        "A direct GroupEnable command is already in progress.");
                }

                if (groupEnableWaitCoordinator.PendingContinuation != null)
                {
                    throw new LMCGroupEnablePendingException(
                        groupEnableWaitCoordinator.PendingContinuation);
                }

                groupEnableWaitCoordinator.WaitInProgress = true;
            }
        }

        private void BeginResumeGroupEnableWait(
            LMCGroupEnableWaitContinuation continuation,
            LMCGroupEnableWaitOptions options,
            long elapsedMilliseconds)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                if (groupEnableWaitCoordinator
                    .EnableAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "The accepted GroupEnable observer is still running.");
                }

                if (groupEnableWaitCoordinator.WaitInProgress)
                {
                    throw new InvalidOperationException(
                        "A group-enable wait operation is already in progress.");
                }

                if (groupEnableWaitCoordinator.DirectEnableInProgress)
                {
                    throw new InvalidOperationException(
                        "A direct GroupEnable command is already in progress.");
                }

                ValidateResumeContinuationCore(
                    continuation,
                    options,
                    elapsedMilliseconds);
                groupEnableWaitCoordinator.WaitInProgress = true;
            }
        }

        private void BeginDirectGroupEnable()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                if (groupEnableWaitCoordinator
                    .EnableAcceptanceObserverInProgress)
                {
                    throw new InvalidOperationException(
                        "The accepted GroupEnable observer is still running.");
                }

                if (groupEnableWaitCoordinator.PendingContinuation != null)
                {
                    throw new LMCGroupEnablePendingException(
                        groupEnableWaitCoordinator.PendingContinuation);
                }

                if (groupEnableWaitCoordinator.WaitInProgress
                    || groupEnableWaitCoordinator.DirectEnableInProgress)
                {
                    throw new InvalidOperationException(
                        "A GroupEnable command or wait operation is already in progress.");
                }

                groupEnableWaitCoordinator.DirectEnableInProgress = true;
            }
        }

        private void EndDirectGroupEnable()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                groupEnableWaitCoordinator.DirectEnableInProgress = false;
            }
        }

        private void ValidateResumeContinuation(
            LMCGroupEnableWaitContinuation continuation,
            LMCGroupEnableWaitOptions options,
            long elapsedMilliseconds)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                ValidateResumeContinuationCore(
                    continuation,
                    options,
                    elapsedMilliseconds);
            }
        }

        private void ValidateResumeContinuationCore(
            LMCGroupEnableWaitContinuation continuation,
            LMCGroupEnableWaitOptions options,
            long elapsedMilliseconds)
        {
            ValidateContinuationOwnerAndSession(continuation);

            if (!ReferenceEquals(
                groupEnableWaitCoordinator.PendingContinuation,
                continuation)
                || !continuation.IsPending)
            {
                throw new LMCGroupEnableWaitResolvedException(
                    continuation,
                    elapsedMilliseconds);
            }

            if (options.StableSampleCount
                != continuation.RequiredStableSampleCount)
            {
                throw new ArgumentException(
                    "StableSampleCount must match the accepted continuation.",
                    "options");
            }
        }

        private void ValidateContinuationOwnerAndSession(
            LMCGroupEnableWaitContinuation continuation)
        {
            if (!ReferenceEquals(
                continuation.Coordinator,
                groupEnableWaitCoordinator))
            {
                throw new ArgumentException(
                    "The continuation belongs to a different connection, session, or group reference.",
                    "continuation");
            }

            if (continuation.SessionGeneration != sessionGeneration
                || continuation.GroupReference != GroupReference)
            {
                throw new InvalidOperationException(
                    "The continuation does not belong to the current group session.");
            }
        }

        internal LMCGroupEnableWaitContinuation
            CaptureGroupEnableWaitObservationTarget()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                return groupEnableWaitCoordinator.PendingContinuation;
            }
        }

        internal void ObserveGroupEnableWaitStatus(
            LMCGroupEnableWaitContinuation continuation,
            LMCGroupReadStatusResult status)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                if (continuation != null
                    && ReferenceEquals(
                        groupEnableWaitCoordinator.PendingContinuation,
                        continuation)
                    && continuation.IsPending)
                {
                    continuation.Observe(status);
                }
            }
        }

        internal void ResetPendingGroupEnableProof(
            LMCGroupEnableWaitContinuation continuation)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                if (continuation != null
                    && ReferenceEquals(
                        groupEnableWaitCoordinator.PendingContinuation,
                        continuation)
                    && continuation.IsPending)
                {
                    continuation.ResetProofCounters();
                }
            }
        }

        private bool TryCompleteLockedStandbyContinuation(
            LMCGroupEnableWaitContinuation continuation,
            long elapsedMilliseconds)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureContinuationStillPendingCore(
                    continuation,
                    elapsedMilliseconds);
                if (!continuation.HasLockedStandbyProof)
                {
                    return false;
                }

                CompletePendingGroupEnableContinuation(continuation);
                return true;
            }
        }

        private bool TryCompleteLockedStandbyContinuationAtDeadline(
            LMCGroupEnableWaitContinuation continuation,
            long elapsedMilliseconds)
        {
            if (!groupEnableWaitCoordinator.StatusObservationGate.Wait(0))
            {
                EnsureContinuationStillPending(
                    continuation,
                    elapsedMilliseconds);
                return false;
            }

            var mutationGateAcquired = false;
            try
            {
                mutationGateAcquired =
                    groupEnableWaitCoordinator.MutationGate.Wait(0);
                if (!mutationGateAcquired)
                {
                    EnsureContinuationStillPending(
                        continuation,
                        elapsedMilliseconds);
                    return false;
                }

                return TryCompleteLockedStandbyContinuation(
                    continuation,
                    elapsedMilliseconds);
            }
            finally
            {
                if (mutationGateAcquired)
                {
                    groupEnableWaitCoordinator.MutationGate.Release();
                }

                groupEnableWaitCoordinator.StatusObservationGate.Release();
            }
        }

        private void EnsureContinuationStillPending(
            LMCGroupEnableWaitContinuation continuation,
            long elapsedMilliseconds)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                EnsureContinuationStillPendingCore(
                    continuation,
                    elapsedMilliseconds);
            }
        }

        private void EnsureContinuationStillPendingCore(
            LMCGroupEnableWaitContinuation continuation,
            long elapsedMilliseconds)
        {
            if (!ReferenceEquals(
                groupEnableWaitCoordinator.PendingContinuation,
                continuation)
                || !continuation.IsPending)
            {
                throw new LMCGroupEnableWaitResolvedException(
                    continuation,
                    elapsedMilliseconds);
            }
        }

        private void CompletePendingGroupEnableContinuation(
            LMCGroupEnableWaitContinuation continuation)
        {
            continuation.MarkCompleted();
            groupEnableWaitCoordinator.PendingContinuation = null;
        }

        private void ReleasePendingGroupEnableAfterDisable(
            LMC_Response response)
        {
            if (response == null || !response.IsSuccess)
            {
                return;
            }

            lock (groupEnableWaitCoordinator.Sync)
            {
                var continuation = groupEnableWaitCoordinator.PendingContinuation;
                if (continuation != null)
                {
                    CompletePendingGroupEnableContinuation(continuation);
                }
            }
        }

        private static void ThrowGroupEnableWaitTimeout(
            LMCGroupEnableWaitContinuation continuation,
            long elapsedMilliseconds = 0)
        {
            var exception = new LMCGroupEnableWaitTimeoutException(
                continuation.CaptureEvidence(elapsedMilliseconds),
                continuation);
            continuation.ResetProofCounters();
            throw exception;
        }

        private void EndGroupEnableWait()
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                groupEnableWaitCoordinator.WaitInProgress = false;
            }
        }

        private sealed class LMCGroupEnableWaitPreWireTimeoutException
            : TimeoutException
        {
        }

        private sealed class LMCGroupPowerStateWaitDeadlineException
            : TimeoutException
        {
        }

        private sealed class LMCGroupEnableStatusPollOutcome
        {
            internal LMCGroupEnableStatusPollOutcome(
                LMCGroupReadStatusResult status,
                bool completed)
            {
                Status = status;
                Completed = completed;
            }

            internal LMCGroupReadStatusResult Status { get; private set; }
            internal bool Completed { get; private set; }
        }

        private LMCLookupResult ResolveGroupLookup(string groupName)
        {
            EnsureCurrentSessionForUse();
            var lookupRaw = connection.Exchange(
                LMC_Frame.LMCGroupGetByName(groupName),
                sessionGeneration);
            return LMCConnection.ParseLookupResult(
                LMCLookupTargetKind.Group,
                groupName,
                lookupRaw);
        }

        private LMC_Response SendGroupEnable()
        {
            return SendUnchecked(
                LMC_Frame.LMCGroupEnable(GroupReference));
        }

        private Task<LMC_Response> SendGroupEnableAsyncUnchecked(
            CancellationToken cancellationToken)
        {
            return SendAsyncUnchecked(
                LMC_Frame.LMCGroupEnable(GroupReference),
                cancellationToken);
        }

        private async Task<LMCGroupEnableSubmissionPublication>
            SendGroupEnableForWaitAsync(
            LMCGroupEnableSubmissionTracker submissionTracker,
            int requiredStableSampleCount,
            bool acceptanceObserverWillRun,
            CancellationToken cancellationToken,
            CancellationToken preWriteCancellationToken,
            CancellationToken postWriteDeadlineToken,
            Func<long> elapsedMilliseconds,
            int timeoutMilliseconds,
            Action beforeGroupEnableWriteCommit,
            Action beforeAcceptedContinuationPublication)
        {
            EnsureCurrentSessionForUse();
            try
            {
                var raw = await connection.ExchangeAsyncDrainAfterWrite(
                    LMC_Frame.LMCGroupEnable(GroupReference),
                    sessionGeneration,
                    preWriteCancellationToken,
                    postWriteDeadlineToken,
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (elapsedMilliseconds() >= timeoutMilliseconds)
                        {
                            throw new LMCGroupEnableWaitPreWireTimeoutException();
                        }
                        if (beforeGroupEnableWriteCommit != null)
                        {
                            beforeGroupEnableWriteCommit();
                        }
                    },
                    () =>
                    {
                        submissionTracker.MarkSubmissionOutcomeUncertain();
                        groupEnableWaitCoordinator
                            .MarkMutationMayHaveBeenSent();
                        groupEnableWaitCoordinator
                            .ResetPendingMutationProof();
                    }).ConfigureAwait(false);
                var response = LMCConnection.ParseCommandAcknowledgement(
                    raw,
                    "GroupEnable");
                LMCGroupEnableWaitContinuation continuation = null;
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.GroupProfileLock,
                    () =>
                    {
                        lock (groupEnableWaitCoordinator.Sync)
                        {
                            submissionTracker.SetAcknowledgement(response);
                            if (!response.IsSuccess)
                            {
                                return;
                            }

                            if (beforeAcceptedContinuationPublication
                                != null)
                            {
                                beforeAcceptedContinuationPublication();
                            }

                            continuation =
                                new LMCGroupEnableWaitContinuation(
                                    groupEnableWaitCoordinator,
                                    GroupName,
                                    GroupReference,
                                    sessionGeneration,
                                    submissionTracker,
                                    requiredStableSampleCount);
                            groupEnableWaitCoordinator.PendingContinuation =
                                continuation;
                            groupEnableWaitCoordinator
                                .EnableAcceptanceObserverInProgress =
                                acceptanceObserverWillRun;
                        }
                    });
                return new LMCGroupEnableSubmissionPublication(
                    response,
                    continuation);
            }
            catch (LMCPostWriteDeadlineException)
            {
                submissionTracker.MarkTransportInvalidatedAtDeadline();
                throw new LMCGroupEnableWaitPreWireTimeoutException();
            }
            catch (OperationCanceledException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new LMCGroupEnableWaitPreWireTimeoutException();
            }
            catch (LMCGroupEnableWaitPreWireTimeoutException)
            {
                throw;
            }
            catch (LMCSendPreemptedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LMCGroupEnableSubmissionException(
                    submissionTracker.CaptureEvidence(
                        elapsedMilliseconds()),
                    ex);
            }
        }

        private sealed class LMCGroupEnableSubmissionPublication
        {
            internal LMCGroupEnableSubmissionPublication(
                LMC_Response acknowledgement,
                LMCGroupEnableWaitContinuation continuation)
            {
                Acknowledgement = acknowledgement
                    ?? throw new ArgumentNullException("acknowledgement");
                Continuation = continuation;
            }

            internal LMC_Response Acknowledgement { get; private set; }
            internal LMCGroupEnableWaitContinuation Continuation
            {
                get;
                private set;
            }
        }

        private LMC_Response SendGroupDisable()
        {
            return SendUnchecked(
                LMC_Frame.LMCGroupDisable(GroupReference));
        }

        private LMC_Response SendGroupReset()
        {
            return Send(LMC_Frame.LMCGroupReset(GroupReference));
        }

        private LMC_Response SendGroupStop(int deceleration, int jerk)
        {
            return Send(LMC_Frame.LMCGroupStop(GroupReference, deceleration, jerk));
        }

        private uint ReadGroupStatusValue(out LMC_Response response)
        {
            var result = GroupReadStatusResult();
            response = result.Response;

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "GroupReadStatus failed. Status="
                    + response.Status
                    + ", ErrorId="
                    + response.ErrorId
                    + ".");
            }

            return result.State;
        }

        private LMC_Response SendMoveLinearAbsolute(
            int[] position,
            int velocity,
            int acceleration,
            int deceleration,
            int jerk)
        {
            return Send(
                LMC_Frame.LMCGroupMoveLinearAbsolute(
                    GroupReference,
                    position,
                    velocity,
                    acceleration,
                    deceleration,
                    jerk));
        }

        private LMC_Response SendRawGroupPowerCommand(
            bool expectedPowerOn)
        {
            EnsureCurrentSessionForUse();
            groupEnableWaitCoordinator.MutationGate.Wait();
            try
            {
                ThrowIfRawGroupPowerCommandIsUnsafe(expectedPowerOn);
                return SendUnchecked(
                    expectedPowerOn
                        ? LMC_Frame.LMCGroupPowerOn(GroupReference)
                        : LMC_Frame.LMCGroupPowerOff(GroupReference),
                    true);
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        private async Task<LMC_Response> SendRawGroupPowerCommandAsync(
            bool expectedPowerOn,
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            await groupEnableWaitCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                ThrowIfRawGroupPowerCommandIsUnsafe(expectedPowerOn);
                return await SendAsyncUnchecked(
                    expectedPowerOn
                        ? LMC_Frame.LMCGroupPowerOn(GroupReference)
                        : LMC_Frame.LMCGroupPowerOff(GroupReference),
                    cancellationToken,
                    true).ConfigureAwait(false);
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        private void ThrowIfRawGroupPowerCommandIsUnsafe(
            bool expectedPowerOn)
        {
            lock (groupEnableWaitCoordinator.Sync)
            {
                var pending = groupEnableWaitCoordinator
                    .PendingPowerStateContinuation;
                if (pending == null || !pending.IsPending)
                {
                    return;
                }

                if (!expectedPowerOn && pending.ExpectedPowerOn)
                {
                    return;
                }

                throw new LMCGroupPowerStateWaitPendingException(
                    pending);
            }
        }

        private LMC_Response Send(byte[] request)
        {
            EnsureCurrentSessionForUse();
            groupEnableWaitCoordinator.MutationGate.Wait();
            try
            {
                return SendUnchecked(request);
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        private LMC_Response SendUnchecked(
            byte[] request,
            bool resetPendingMutationProof = false)
        {
            EnsureCurrentSessionForUse();
            var raw = connection.Exchange(
                request,
                sessionGeneration,
                () =>
                {
                    groupEnableWaitCoordinator
                        .MarkMutationMayHaveBeenSent();
                    if (resetPendingMutationProof)
                    {
                        groupEnableWaitCoordinator
                            .ResetPendingMutationProof();
                    }
                });
            var response = LMCConnection.ParseCommandAcknowledgement(
                raw,
                "Group command");
            LMC_Response publishedResponse = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_Frame.GetRequestCommand(request),
                () => publishedResponse = response);
            return publishedResponse;
        }

        private async Task<LMC_Response> SendAsync(
            byte[] request,
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            await groupEnableWaitCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                return await SendAsyncUnchecked(
                    request,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        private async Task<LMC_Response> SendAsyncUnchecked(
            byte[] request,
            CancellationToken cancellationToken,
            bool resetPendingMutationProof = false)
        {
            EnsureCurrentSessionForUse();
            cancellationToken.ThrowIfCancellationRequested();
            var raw = await connection.ExchangeAsync(
                request,
                sessionGeneration,
                cancellationToken,
                () =>
                {
                    groupEnableWaitCoordinator
                        .MarkMutationMayHaveBeenSent();
                    if (resetPendingMutationProof)
                    {
                        groupEnableWaitCoordinator
                            .ResetPendingMutationProof();
                    }
                })
                .ConfigureAwait(false);
            var response = LMCConnection.ParseCommandAcknowledgement(
                raw,
                "Group command");
            LMC_Response publishedResponse = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_Frame.GetRequestCommand(request),
                () => publishedResponse = response);
            return publishedResponse;
        }

        private LMC_Response SendShortAcknowledgement(byte[] request)
        {
            EnsureCurrentSessionForUse();
            groupEnableWaitCoordinator.MutationGate.Wait();
            try
            {
                return SendShortAcknowledgementUnchecked(request);
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        private LMC_Response SendShortAcknowledgementUnchecked(byte[] request)
        {
            EnsureCurrentSessionForUse();
            var raw = connection.Exchange(
                request,
                sessionGeneration,
                () => groupEnableWaitCoordinator
                    .MarkMutationMayHaveBeenSent());
            var response = LMCConnection.ParseShortAcknowledgement(
                raw,
                "SetKinTransformCartesian4Axis");
            LMC_Response publishedResponse = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_Frame.GetRequestCommand(request),
                () => publishedResponse = response);
            return publishedResponse;
        }

        private async Task<LMC_Response> SendShortAcknowledgementAsync(
            byte[] request,
            CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            await groupEnableWaitCoordinator.MutationGate.WaitAsync(
                cancellationToken).ConfigureAwait(false);
            try
            {
                return await SendShortAcknowledgementAsyncUnchecked(
                    request,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                groupEnableWaitCoordinator.MutationGate.Release();
            }
        }

        private async Task<LMC_Response>
            SendShortAcknowledgementAsyncUnchecked(
                byte[] request,
                CancellationToken cancellationToken)
        {
            EnsureCurrentSessionForUse();
            cancellationToken.ThrowIfCancellationRequested();
            var raw = await connection.ExchangeAsync(
                request,
                sessionGeneration,
                cancellationToken,
                () => groupEnableWaitCoordinator
                    .MarkMutationMayHaveBeenSent())
                .ConfigureAwait(false);
            var response = LMCConnection.ParseShortAcknowledgement(
                raw,
                "SetKinTransformCartesian4Axis");
            LMC_Response publishedResponse = null;
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_Frame.GetRequestCommand(request),
                () => publishedResponse = response);
            return publishedResponse;
        }

        private void EnsureCurrentSessionForUse()
        {
            connection.EnsureSessionGeneration(sessionGeneration);
        }

        private void ValidateKinematicAxes(
            LMCSingleAxis axisX,
            LMCSingleAxis axisY,
            LMCSingleAxis axisZ,
            LMCSingleAxis axisU)
        {
            var axes = new[] { axisX, axisY, axisZ, axisU };
            var usedReferences = new System.Collections.Generic.HashSet<ushort>();

            for (var index = 0; index < axes.Length; index++)
            {
                var axis = axes[index];
                if (axis == null)
                {
                    throw new ArgumentNullException("axis" + index);
                }

                if (!ReferenceEquals(connection, axis.Connection))
                {
                    throw new ArgumentException(
                        "All kinematic axes must belong to the same LMCConnection as the group.");
                }

                axis.EnsureCurrentSessionForUse();

                if (!usedReferences.Add(axis.AxisReference))
                {
                    throw new ArgumentException(
                        "Each kinematic axis reference must be unique.");
                }
            }
        }
    }

    public sealed class LMCGroup : LMCGroupAxis
    {
        public LMCGroup(LMCConnection connection, string groupName)
            : base(connection, groupName)
        {
        }
    }
}

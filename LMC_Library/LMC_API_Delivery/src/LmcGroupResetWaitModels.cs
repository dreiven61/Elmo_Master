using System;
using System.Collections.Generic;
using System.Threading;

namespace LasalMotionControlLib
{
    internal sealed class LMCGroupResetRecoveryAttachmentRegistry
    {
        internal LMCGroupResetRecoveryAttachmentRegistry()
        {
            AttachedOperationIds = new HashSet<Guid>();
        }

        internal HashSet<Guid> AttachedOperationIds { get; private set; }
    }

    internal static class LMCGroupResetObserverScope
    {
        private static readonly AsyncLocal<ScopeState> current =
            new AsyncLocal<ScopeState>();

        internal static IDisposable Enter(
            LMCConnection connection,
            long sessionGeneration,
            ushort groupReference,
            LMCGroupMemberInfo[] members)
        {
            var previous = current.Value;
            current.Value = new ScopeState(
                connection,
                sessionGeneration,
                groupReference,
                members);
            return new ScopeLease(previous);
        }

        internal static void ThrowIfGroupMutationReentrant(
            LMCConnection connection,
            long sessionGeneration,
            ushort groupReference)
        {
            var scope = current.Value;
            if (scope != null
                && ReferenceEquals(scope.Connection, connection)
                && scope.SessionGeneration == sessionGeneration
                && scope.GroupReference == groupReference)
            {
                throw new InvalidOperationException(
                    "A Group Reset accepted-continuation observer cannot issue a reentrant group mutation.");
            }
        }

        internal static void ThrowIfMemberMutationReentrant(
            LMCConnection connection,
            long sessionGeneration,
            ushort axisReference)
        {
            var scope = current.Value;
            if (scope != null
                && ReferenceEquals(scope.Connection, connection)
                && scope.SessionGeneration == sessionGeneration
                && scope.MemberReferences.Contains(axisReference))
            {
                throw new InvalidOperationException(
                    "A Group Reset accepted-continuation observer cannot issue a reentrant captured-member mutation.");
            }
        }

        private sealed class ScopeState
        {
            internal ScopeState(
                LMCConnection connection,
                long sessionGeneration,
                ushort groupReference,
                LMCGroupMemberInfo[] members)
            {
                Connection = connection;
                SessionGeneration = sessionGeneration;
                GroupReference = groupReference;
                MemberReferences = new HashSet<ushort>();
                for (var index = 0; index < members.Length; index++)
                {
                    MemberReferences.Add(members[index].AxisReference);
                }
            }

            internal LMCConnection Connection { get; private set; }
            internal long SessionGeneration { get; private set; }
            internal ushort GroupReference { get; private set; }
            internal HashSet<ushort> MemberReferences { get; private set; }
        }

        private sealed class ScopeLease : IDisposable
        {
            private readonly ScopeState previous;
            private bool disposed;

            internal ScopeLease(ScopeState previous)
            {
                this.previous = previous;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    current.Value = previous;
                    disposed = true;
                }
            }
        }
    }

    public enum LMCGroupResetSubmissionOutcome
    {
        NotAttempted = 0,
        Rejected = 1,
        OutcomeUncertain = 2,
        Accepted = 3
    }

    public enum LMCGroupResetWaitContinuationState
    {
        Pending = 1,
        Completed = 2,
        SupersededBySafetyMutation = 3,
        SupersededByInterveningMutation = 4
    }

    public sealed class LMCGroupResetDurableMemberIdentity
    {
        public LMCGroupResetDurableMemberIdentity(
            int index,
            ushort axisReference,
            ushort deviceId,
            string axisName)
        {
            Index = index;
            AxisReference = axisReference;
            DeviceId = deviceId;
            AxisName = axisName ?? string.Empty;
        }

        public int Index { get; private set; }
        public ushort AxisReference { get; private set; }
        public ushort DeviceId { get; private set; }
        public string AxisName { get; private set; }

        internal static LMCGroupResetDurableMemberIdentity[] FromMembers(
            LMCGroupMemberInfo[] source)
        {
            if (source == null)
            {
                return new LMCGroupResetDurableMemberIdentity[0];
            }

            var result = new LMCGroupResetDurableMemberIdentity[
                source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                var member = source[index];
                result[index] = member == null
                    ? null
                    : new LMCGroupResetDurableMemberIdentity(
                        member.Index,
                        member.AxisReference,
                        member.DeviceId,
                        member.AxisName);
            }
            return result;
        }

        internal static LMCGroupResetDurableMemberIdentity[] Clone(
            LMCGroupResetDurableMemberIdentity[] source)
        {
            if (source == null)
            {
                return new LMCGroupResetDurableMemberIdentity[0];
            }

            var result = new LMCGroupResetDurableMemberIdentity[
                source.Length];
            for (var index = 0; index < source.Length; index++)
            {
                var member = source[index];
                result[index] = member == null
                    ? null
                    : new LMCGroupResetDurableMemberIdentity(
                        member.Index,
                        member.AxisReference,
                        member.DeviceId,
                        member.AxisName);
            }
            return result;
        }
    }

    /// <summary>
    /// Immutable command-before evidence observed after the exact member
    /// snapshot and immediately before the Group Reset write boundary.
    /// </summary>
    public sealed class LMCGroupResetPreparedEvidence
    {
        private readonly LMCGroupResetDurableMemberIdentity[] members;

        internal LMCGroupResetPreparedEvidence(
            Guid operationId,
            string groupName,
            ushort groupReference,
            long sessionGeneration,
            LMCGroupMemberInfo[] members,
            int requiredStableSampleCount)
        {
            OperationId = operationId;
            GroupName = groupName ?? string.Empty;
            GroupReference = groupReference;
            SessionGeneration = sessionGeneration;
            this.members = LMCGroupResetDurableMemberIdentity.FromMembers(
                members);
            RequiredStableSampleCount = requiredStableSampleCount;
        }

        public Guid OperationId { get; private set; }
        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public long SessionGeneration { get; private set; }
        public LMCGroupResetSubmissionOutcome SubmissionOutcome
        {
            get { return LMCGroupResetSubmissionOutcome.NotAttempted; }
        }
        public LMCGroupResetDurableMemberIdentity[] Members
        {
            get { return LMCGroupResetDurableMemberIdentity.Clone(members); }
        }
        public int RequiredStableSampleCount { get; private set; }
        public bool RecoveredFromDurableRecord { get { return false; } }
        public bool CommandDispatchedInOwnerSession { get { return false; } }
    }

    /// <summary>
    /// Persisted identity used to attach status-only Group Reset recovery to a
    /// new connection session. Attach never replays the Group Reset command.
    /// </summary>
    public sealed class LMCGroupResetDurableRecoveryRecord
    {
        private readonly LMCGroupResetDurableMemberIdentity[] members;

        public LMCGroupResetDurableRecoveryRecord(
            Guid operationId,
            LMCGroupResetSubmissionOutcome priorSubmissionOutcome,
            string groupName,
            ushort groupReference,
            long ownerSessionGeneration,
            LMCGroupResetDurableMemberIdentity[] members,
            int requiredStableSampleCount)
        {
            OperationId = operationId;
            PriorSubmissionOutcome = priorSubmissionOutcome;
            GroupName = groupName;
            GroupReference = groupReference;
            OwnerSessionGeneration = ownerSessionGeneration;
            this.members = LMCGroupResetDurableMemberIdentity.Clone(members);
            RequiredStableSampleCount = requiredStableSampleCount;
        }

        public Guid OperationId { get; private set; }
        public LMCGroupResetSubmissionOutcome PriorSubmissionOutcome
        {
            get;
            private set;
        }
        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public long OwnerSessionGeneration { get; private set; }
        public LMCGroupResetDurableMemberIdentity[] Members
        {
            get { return LMCGroupResetDurableMemberIdentity.Clone(members); }
        }
        public int RequiredStableSampleCount { get; private set; }
    }

    public sealed class LMCGroupResetWaitOptions
    {
        public const int DefaultTimeoutMilliseconds = 5000;
        public const int DefaultPollIntervalMilliseconds = 50;
        public const int DefaultStableSampleCount = 3;

        public LMCGroupResetWaitOptions()
        {
            TimeoutMilliseconds = DefaultTimeoutMilliseconds;
            PollIntervalMilliseconds = DefaultPollIntervalMilliseconds;
            StableSampleCount = DefaultStableSampleCount;
        }

        public int TimeoutMilliseconds { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public int StableSampleCount { get; set; }

        internal LMCGroupResetWaitOptions SnapshotAndValidate()
        {
            if (TimeoutMilliseconds < 1 || TimeoutMilliseconds > 600000)
            {
                throw new ArgumentOutOfRangeException(
                    "TimeoutMilliseconds",
                    "TimeoutMilliseconds must be between 1 and 600000.");
            }

            if (PollIntervalMilliseconds < 1
                || PollIntervalMilliseconds > TimeoutMilliseconds)
            {
                throw new ArgumentOutOfRangeException(
                    "PollIntervalMilliseconds",
                    "PollIntervalMilliseconds must be positive and no greater than TimeoutMilliseconds.");
            }

            if (StableSampleCount < 1 || StableSampleCount > 100)
            {
                throw new ArgumentOutOfRangeException(
                    "StableSampleCount",
                    "StableSampleCount must be between 1 and 100.");
            }

            return new LMCGroupResetWaitOptions
            {
                TimeoutMilliseconds = TimeoutMilliseconds,
                PollIntervalMilliseconds = PollIntervalMilliseconds,
                StableSampleCount = StableSampleCount
            };
        }
    }

    public sealed class LMCGroupResetMemberStatus
    {
        internal LMCGroupResetMemberStatus(
            int index,
            ushort axisReference,
            ushort deviceId,
            string axisName,
            LMCReadStatusResult status)
        {
            Index = index;
            AxisReference = axisReference;
            DeviceId = deviceId;
            AxisName = axisName ?? string.Empty;
            Status = status;
        }

        public int Index { get; private set; }
        public ushort AxisReference { get; private set; }
        public ushort DeviceId { get; private set; }
        public string AxisName { get; private set; }
        public LMCReadStatusResult Status { get; private set; }
    }

    public sealed class LMCGroupResetMemberMutationEvidence
    {
        internal LMCGroupResetMemberMutationEvidence(
            ushort axisReference,
            long expectedMutationGeneration,
            long observedMutationGeneration,
            bool mutationBaselineCaptured)
        {
            AxisReference = axisReference;
            ExpectedMutationGeneration = expectedMutationGeneration;
            ObservedMutationGeneration = observedMutationGeneration;
            MutationBaselineCaptured = mutationBaselineCaptured;
        }

        public ushort AxisReference { get; private set; }
        public long ExpectedMutationGeneration { get; private set; }
        public long ObservedMutationGeneration { get; private set; }
        public bool MutationBaselineCaptured { get; private set; }
        public bool InterveningMutationDetected
        {
            get
            {
                return MutationBaselineCaptured
                    && ExpectedMutationGeneration
                        != ObservedMutationGeneration;
            }
        }
    }

    /// <summary>
    /// Immutable evidence for one accepted-once Group Reset operation. One
    /// stable sample is a complete 0x2045 group read followed by one 0x2028
    /// read for every member captured by the pre-reset 0x20D2 snapshot.
    /// </summary>
    public sealed class LMCGroupResetWaitEvidence
    {
        private readonly LMCGroupMemberInfo[] members;
        private readonly LMCGroupResetMemberStatus[] lastObservedMemberStatuses;
        private readonly LMCGroupResetMemberMutationEvidence[] memberMutations;

        internal LMCGroupResetWaitEvidence(
            LMCGroupResetSubmissionOutcome submissionOutcome,
            LMC_Response acknowledgement,
            LMCGroupMemberInfo[] members,
            LMCGroupReadStatusResult lastObservedGroupStatus,
            LMCGroupResetMemberStatus[] lastObservedMemberStatuses,
            int statusRoundCount,
            int stableSampleCount,
            int requiredStableSampleCount,
            long resetMutationGeneration,
            long observedGroupMutationGeneration,
            LMCGroupResetMemberMutationEvidence[] memberMutations,
            bool mutationBaselineCaptured,
            Guid operationId,
            long commandOwnerSessionGeneration,
            bool recoveredFromDurableRecord,
            bool commandDispatchedInOwnerSession,
            bool transportInvalidatedAtDeadline,
            long elapsedMilliseconds)
        {
            SubmissionOutcome = submissionOutcome;
            Acknowledgement = acknowledgement;
            this.members = CloneMembers(members);
            LastObservedGroupStatus = lastObservedGroupStatus;
            this.lastObservedMemberStatuses = CloneMemberStatuses(
                lastObservedMemberStatuses);
            StatusRoundCount = statusRoundCount;
            StableSampleCount = stableSampleCount;
            RequiredStableSampleCount = requiredStableSampleCount;
            ResetMutationGeneration = resetMutationGeneration;
            ObservedGroupMutationGeneration = observedGroupMutationGeneration;
            this.memberMutations = CloneMemberMutations(memberMutations);
            MutationBaselineCaptured = mutationBaselineCaptured;
            OperationId = operationId;
            CommandOwnerSessionGeneration = commandOwnerSessionGeneration;
            RecoveredFromDurableRecord = recoveredFromDurableRecord;
            CommandDispatchedInOwnerSession =
                commandDispatchedInOwnerSession;
            TransportInvalidatedAtDeadline = transportInvalidatedAtDeadline;
            ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        }

        public LMCGroupResetSubmissionOutcome SubmissionOutcome
        {
            get;
            private set;
        }

        public bool CommandMayHaveBeenSent
        {
            get
            {
                return SubmissionOutcome
                    != LMCGroupResetSubmissionOutcome.NotAttempted;
            }
        }

        public LMC_Response Acknowledgement { get; private set; }
        public bool ResetAccepted
        {
            get
            {
                return SubmissionOutcome
                    == LMCGroupResetSubmissionOutcome.Accepted;
            }
        }

        public LMCGroupMemberInfo[] Members
        {
            get { return CloneMembers(members); }
        }

        public LMCGroupReadStatusResult LastObservedGroupStatus
        {
            get;
            private set;
        }

        public LMCGroupResetMemberStatus[] LastObservedMemberStatuses
        {
            get { return CloneMemberStatuses(lastObservedMemberStatuses); }
        }

        public int StatusRoundCount { get; private set; }
        public int StableSampleCount { get; private set; }
        public int RequiredStableSampleCount { get; private set; }
        public long ResetMutationGeneration { get; private set; }
        public long ObservedGroupMutationGeneration { get; private set; }
        public bool MutationBaselineCaptured { get; private set; }
        public Guid OperationId { get; private set; }
        public long CommandOwnerSessionGeneration { get; private set; }
        public bool RecoveredFromDurableRecord { get; private set; }
        public bool CommandDispatchedInOwnerSession { get; private set; }

        public LMCGroupResetMemberMutationEvidence[] MemberMutations
        {
            get { return CloneMemberMutations(memberMutations); }
        }

        public bool InterveningMutationDetected
        {
            get
            {
                if (MutationBaselineCaptured
                    && ResetMutationGeneration
                        != ObservedGroupMutationGeneration)
                {
                    return true;
                }

                for (var index = 0; index < memberMutations.Length; index++)
                {
                    if (memberMutations[index].InterveningMutationDetected)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool TransportInvalidatedAtDeadline { get; private set; }
        public long ElapsedMilliseconds { get; private set; }

        internal static LMCGroupMemberInfo[] CloneMembers(
            LMCGroupMemberInfo[] source)
        {
            return source == null
                ? new LMCGroupMemberInfo[0]
                : (LMCGroupMemberInfo[])source.Clone();
        }

        internal static LMCGroupResetMemberStatus[] CloneMemberStatuses(
            LMCGroupResetMemberStatus[] source)
        {
            return source == null
                ? new LMCGroupResetMemberStatus[0]
                : (LMCGroupResetMemberStatus[])source.Clone();
        }

        internal static LMCGroupResetMemberMutationEvidence[]
            CloneMemberMutations(
                LMCGroupResetMemberMutationEvidence[] source)
        {
            return source == null
                ? new LMCGroupResetMemberMutationEvidence[0]
                : (LMCGroupResetMemberMutationEvidence[])source.Clone();
        }
    }

    internal sealed class LMCGroupResetWaitTracker
    {
        private readonly LMCGroupMemberInfo[] members;
        private readonly int requiredStableSampleCount;
        private readonly Guid operationId;
        private readonly long commandOwnerSessionGeneration;
        private readonly bool recoveredFromDurableRecord;
        private readonly long[] expectedMemberMutationGenerations;
        private readonly long[] observedMemberMutationGenerations;
        private LMCGroupResetSubmissionOutcome submissionOutcome;
        private LMC_Response acknowledgement;
        private LMCGroupReadStatusResult lastObservedGroupStatus;
        private LMCGroupResetMemberStatus[] lastObservedMemberStatuses;
        private int statusRoundCount;
        private int stableSampleCount;
        private long resetMutationGeneration;
        private long observedGroupMutationGeneration;
        private bool mutationBaselineCaptured;
        private bool commandDispatchedInOwnerSession;
        private bool transportInvalidatedAtDeadline;

        internal LMCGroupResetWaitTracker(
            LMCGroupMemberInfo[] members,
            int requiredStableSampleCount,
            Guid operationId,
            long commandOwnerSessionGeneration,
            LMCGroupResetSubmissionOutcome submissionOutcome =
                LMCGroupResetSubmissionOutcome.NotAttempted,
            bool recoveredFromDurableRecord = false)
        {
            this.members = LMCGroupResetWaitEvidence.CloneMembers(members);
            this.requiredStableSampleCount = requiredStableSampleCount;
            this.operationId = operationId;
            this.commandOwnerSessionGeneration =
                commandOwnerSessionGeneration;
            this.submissionOutcome = submissionOutcome;
            this.recoveredFromDurableRecord = recoveredFromDurableRecord;
            expectedMemberMutationGenerations = new long[this.members.Length];
            observedMemberMutationGenerations = new long[this.members.Length];
            lastObservedMemberStatuses = new LMCGroupResetMemberStatus[0];
        }

        internal bool HasStableProof
        {
            get { return stableSampleCount >= requiredStableSampleCount; }
        }

        internal long ResetMutationGeneration
        {
            get { return resetMutationGeneration; }
        }

        internal void SetMutationGenerations(
            long groupGeneration,
            long[] memberGenerations)
        {
            if (groupGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException("groupGeneration");
            }
            if (memberGenerations == null
                || memberGenerations.Length != members.Length)
            {
                throw new ArgumentException(
                    "Member mutation generation count does not match the captured member snapshot.",
                    "memberGenerations");
            }

            resetMutationGeneration = groupGeneration;
            observedGroupMutationGeneration = groupGeneration;
            for (var index = 0; index < memberGenerations.Length; index++)
            {
                if (memberGenerations[index] <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "memberGenerations");
                }
                expectedMemberMutationGenerations[index] =
                    memberGenerations[index];
                observedMemberMutationGenerations[index] =
                    memberGenerations[index];
            }
            mutationBaselineCaptured = true;
        }

        internal void SetRecoveryMutationGenerationBaseline(
            long groupGeneration,
            long[] memberGenerations)
        {
            if (groupGeneration < 0)
            {
                throw new ArgumentOutOfRangeException("groupGeneration");
            }
            if (memberGenerations == null
                || memberGenerations.Length != members.Length)
            {
                throw new ArgumentException(
                    "Member mutation generation count does not match the captured member snapshot.",
                    "memberGenerations");
            }

            resetMutationGeneration = groupGeneration;
            observedGroupMutationGeneration = groupGeneration;
            for (var index = 0; index < memberGenerations.Length; index++)
            {
                if (memberGenerations[index] < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "memberGenerations");
                }
                expectedMemberMutationGenerations[index] =
                    memberGenerations[index];
                observedMemberMutationGenerations[index] =
                    memberGenerations[index];
            }
            mutationBaselineCaptured = true;
        }

        internal void ObserveMutationGenerations(
            long groupGeneration,
            long[] memberGenerations)
        {
            observedGroupMutationGeneration = groupGeneration;
            if (memberGenerations == null
                || memberGenerations.Length != members.Length)
            {
                return;
            }

            Array.Copy(
                memberGenerations,
                observedMemberMutationGenerations,
                memberGenerations.Length);
        }

        internal void ClearMutationGenerationsAfterRejected()
        {
            resetMutationGeneration = 0;
            observedGroupMutationGeneration = 0;
            mutationBaselineCaptured = false;
            Array.Clear(
                expectedMemberMutationGenerations,
                0,
                expectedMemberMutationGenerations.Length);
            Array.Clear(
                observedMemberMutationGenerations,
                0,
                observedMemberMutationGenerations.Length);
        }

        internal bool HasInterveningMutation
        {
            get
            {
                if (!mutationBaselineCaptured
                    || observedGroupMutationGeneration
                        != resetMutationGeneration)
                {
                    return true;
                }

                for (var index = 0;
                    index < expectedMemberMutationGenerations.Length;
                    index++)
                {
                    if (observedMemberMutationGenerations[index]
                            != expectedMemberMutationGenerations[index])
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal void MarkSubmissionOutcomeUncertain()
        {
            if (submissionOutcome
                == LMCGroupResetSubmissionOutcome.NotAttempted)
            {
                submissionOutcome =
                    LMCGroupResetSubmissionOutcome.OutcomeUncertain;
            }
        }

        internal void MarkCommandDispatchedInOwnerSession()
        {
            commandDispatchedInOwnerSession = true;
        }

        internal void SetAcknowledgement(LMC_Response value)
        {
            acknowledgement = value;
            submissionOutcome = value != null
                    && value.IsFrameValid
                    && value.IsSuccess
                ? LMCGroupResetSubmissionOutcome.Accepted
                : LMCGroupResetSubmissionOutcome.Rejected;
        }

        internal void ObserveRound(
            LMCGroupReadStatusResult groupStatus,
            LMCGroupResetMemberStatus[] memberStatuses)
        {
            lastObservedGroupStatus = groupStatus;
            lastObservedMemberStatuses =
                LMCGroupResetWaitEvidence.CloneMemberStatuses(memberStatuses);
            statusRoundCount++;

            var stable = groupStatus != null
                && groupStatus.IsReadSuccessful
                && !groupStatus.HasGroupError
                && memberStatuses != null
                && memberStatuses.Length == members.Length;
            if (stable)
            {
                for (var index = 0; index < memberStatuses.Length; index++)
                {
                    var status = memberStatuses[index] == null
                        ? null
                        : memberStatuses[index].Status;
                    if (status == null
                        || !status.IsReadSuccessful
                        || status.HasAxisError)
                    {
                        stable = false;
                        break;
                    }
                }
            }

            stableSampleCount = stable ? stableSampleCount + 1 : 0;
        }

        internal void ResetStableProof()
        {
            stableSampleCount = 0;
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            transportInvalidatedAtDeadline = true;
        }

        internal LMCGroupResetWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            var mutations = new LMCGroupResetMemberMutationEvidence[
                members.Length];
            for (var index = 0; index < members.Length; index++)
            {
                mutations[index] =
                    new LMCGroupResetMemberMutationEvidence(
                        members[index].AxisReference,
                        expectedMemberMutationGenerations[index],
                        observedMemberMutationGenerations[index],
                        mutationBaselineCaptured);
            }

            return new LMCGroupResetWaitEvidence(
                submissionOutcome,
                acknowledgement,
                members,
                lastObservedGroupStatus,
                lastObservedMemberStatuses,
                statusRoundCount,
                stableSampleCount,
                requiredStableSampleCount,
                resetMutationGeneration,
                observedGroupMutationGeneration,
                mutations,
                mutationBaselineCaptured,
                operationId,
                commandOwnerSessionGeneration,
                recoveredFromDurableRecord,
                commandDispatchedInOwnerSession,
                transportInvalidatedAtDeadline,
                elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Session-bound evidence that one 0x2049 acknowledgement was accepted.
    /// Resume performs status reads only and never sends another 0x2049.
    /// Process-local mutation attribution covers the group and the member-axis
    /// references captured before Reset. Other RPC clients and PLC logic remain
    /// outside this attribution boundary.
    /// </summary>
    public sealed class LMCGroupResetWaitContinuation
    {
        private readonly object stateSync;
        private readonly LMCConnection ownerConnection;
        private readonly LMCGroupResetWaitTracker tracker;
        private LMCGroupResetWaitContinuationState state;

        internal LMCGroupResetWaitContinuation(
            LMCGroupEnableWaitCoordinator coordinator,
            LMCConnection ownerConnection,
            string groupName,
            ushort groupReference,
            long sessionGeneration,
            LMCAxisPowerOnWaitCoordinator[] memberCoordinators,
            LMCGroupResetWaitTracker tracker)
        {
            Coordinator = coordinator
                ?? throw new ArgumentNullException("coordinator");
            this.ownerConnection = ownerConnection
                ?? throw new ArgumentNullException("ownerConnection");
            MemberCoordinators = memberCoordinators
                ?? throw new ArgumentNullException("memberCoordinators");
            this.tracker = tracker ?? throw new ArgumentNullException("tracker");
            stateSync = coordinator.Sync;
            GroupName = groupName ?? throw new ArgumentNullException("groupName");
            GroupReference = groupReference;
            SessionGeneration = sessionGeneration;
            state = LMCGroupResetWaitContinuationState.Pending;
        }

        internal LMCGroupEnableWaitCoordinator Coordinator
        {
            get;
            private set;
        }

        internal LMCAxisPowerOnWaitCoordinator[] MemberCoordinators
        {
            get;
            private set;
        }

        public string GroupName { get; private set; }
        public ushort GroupReference { get; private set; }
        public long SessionGeneration { get; private set; }

        public LMCGroupResetWaitContinuationState State
        {
            get { lock (stateSync) { return state; } }
        }

        public bool IsPending
        {
            get
            {
                lock (stateSync)
                {
                    return state
                        == LMCGroupResetWaitContinuationState.Pending;
                }
            }
        }

        internal bool IsCompleted
        {
            get
            {
                lock (stateSync)
                {
                    return state
                        == LMCGroupResetWaitContinuationState.Completed;
                }
            }
        }

        public LMC_Response Acknowledgement
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).Acknowledgement; } }
        }

        public LMCGroupResetSubmissionOutcome SubmissionOutcome
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).SubmissionOutcome; } }
        }

        public Guid OperationId
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).OperationId; } }
        }

        public long CommandOwnerSessionGeneration
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).CommandOwnerSessionGeneration; } }
        }

        public bool RecoveredFromDurableRecord
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).RecoveredFromDurableRecord; } }
        }

        public bool CommandDispatchedInOwnerSession
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).CommandDispatchedInOwnerSession; } }
        }

        public LMCGroupMemberInfo[] Members
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).Members; } }
        }

        public LMCGroupReadStatusResult LastObservedGroupStatus
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).LastObservedGroupStatus; } }
        }

        public LMCGroupResetMemberStatus[] LastObservedMemberStatuses
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).LastObservedMemberStatuses; } }
        }

        public int StatusRoundCount
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).StatusRoundCount; } }
        }

        public int StableSampleCount
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).StableSampleCount; } }
        }

        public int RequiredStableSampleCount
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).RequiredStableSampleCount; } }
        }

        public long ResetMutationGeneration
        {
            get { lock (stateSync) { return tracker.CaptureEvidence(0).ResetMutationGeneration; } }
        }

        internal bool BelongsTo(
            LMCConnection connection,
            long sessionGeneration,
            ushort groupReference,
            string groupName)
        {
            return ReferenceEquals(ownerConnection, connection)
                && SessionGeneration == sessionGeneration
                && GroupReference == groupReference
                && string.Equals(
                    GroupName,
                    groupName,
                    StringComparison.Ordinal);
        }

        internal bool HasStableProof
        {
            get { lock (stateSync) { return tracker.HasStableProof; } }
        }

        internal bool HasInterveningMutation
        {
            get { lock (stateSync) { return tracker.HasInterveningMutation; } }
        }

        internal void ObserveMutationGenerations(
            long groupGeneration,
            long[] memberGenerations)
        {
            lock (stateSync)
            {
                tracker.ObserveMutationGenerations(
                    groupGeneration,
                    memberGenerations);
            }
        }

        internal void ObserveRound(
            LMCGroupReadStatusResult groupStatus,
            LMCGroupResetMemberStatus[] memberStatuses)
        {
            lock (stateSync)
            {
                tracker.ObserveRound(groupStatus, memberStatuses);
            }
        }

        internal void ResetStableProof()
        {
            lock (stateSync)
            {
                tracker.ResetStableProof();
            }
        }

        internal void MarkTransportInvalidatedAtDeadline()
        {
            lock (stateSync)
            {
                tracker.MarkTransportInvalidatedAtDeadline();
            }
        }

        internal LMCGroupResetWaitEvidence CaptureEvidence(
            long elapsedMilliseconds)
        {
            lock (stateSync)
            {
                return tracker.CaptureEvidence(elapsedMilliseconds);
            }
        }

        internal void MarkCompleted()
        {
            lock (stateSync)
            {
                state = LMCGroupResetWaitContinuationState.Completed;
            }
        }

        internal void MarkSupersededBySafetyMutation()
        {
            lock (stateSync)
            {
                if (state == LMCGroupResetWaitContinuationState.Pending)
                {
                    state = LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation;
                }
            }
        }

        internal void MarkSupersededByInterveningMutation()
        {
            lock (stateSync)
            {
                if (state == LMCGroupResetWaitContinuationState.Pending)
                {
                    state = LMCGroupResetWaitContinuationState
                        .SupersededByInterveningMutation;
                }
            }
        }

        internal bool TryRestoreAfterRejectedSafetyMutation()
        {
            lock (stateSync)
            {
                if (state != LMCGroupResetWaitContinuationState
                        .SupersededBySafetyMutation)
                {
                    return false;
                }

                state = LMCGroupResetWaitContinuationState.Pending;
                return true;
            }
        }
    }

    public sealed class LMCGroupResetWaitResult
    {
        internal LMCGroupResetWaitResult(
            LMCGroupResetWaitEvidence evidence,
            LMCGroupResetWaitContinuation continuation)
        {
            Evidence = evidence ?? throw new ArgumentNullException("evidence");
            Continuation = continuation;
        }

        public LMCGroupResetWaitEvidence Evidence { get; private set; }
        public LMCGroupResetWaitContinuation Continuation { get; private set; }
        public LMCGroupResetSubmissionOutcome SubmissionOutcome
        {
            get { return Evidence.SubmissionOutcome; }
        }
        public LMC_Response Acknowledgement
        {
            get { return Evidence.Acknowledgement; }
        }
        public bool ResetAccepted { get { return Evidence.ResetAccepted; } }
        public bool RecoveredFromDurableRecord
        {
            get { return Evidence.RecoveredFromDurableRecord; }
        }
        public bool CommandDispatchedInOwnerSession
        {
            get { return Evidence.CommandDispatchedInOwnerSession; }
        }
        public LMCGroupReadStatusResult FinalGroupStatus
        {
            get { return Evidence.LastObservedGroupStatus; }
        }
        public LMCGroupResetMemberStatus[] FinalMemberStatuses
        {
            get { return Evidence.LastObservedMemberStatuses; }
        }
        public int StatusRoundCount { get { return Evidence.StatusRoundCount; } }
        public int StableSampleCount { get { return Evidence.StableSampleCount; } }
        public int RequiredStableSampleCount
        {
            get { return Evidence.RequiredStableSampleCount; }
        }
        public bool TransportInvalidatedAtDeadline
        {
            get { return Evidence.TransportInvalidatedAtDeadline; }
        }
        public long ElapsedMilliseconds
        {
            get { return Evidence.ElapsedMilliseconds; }
        }
    }

    public enum LMCGroupResetDurableRecoveryFailureKind
    {
        InvalidRecord = 1,
        GroupIdentityMismatch = 2,
        MemberSnapshotMismatch = 3,
        DuplicateAttachment = 4,
        PendingOperation = 5,
        SessionInvalid = 6,
        AttachFailed = 7
    }

    public sealed class LMCGroupResetDurableRecoveryException
        : InvalidOperationException
    {
        internal LMCGroupResetDurableRecoveryException(
            LMCGroupResetDurableRecoveryFailureKind failureKind,
            LMCGroupResetDurableRecoveryRecord record,
            string message,
            Exception innerException = null)
            : base(message, innerException)
        {
            FailureKind = failureKind;
            Record = record;
        }

        public LMCGroupResetDurableRecoveryFailureKind FailureKind
        {
            get;
            private set;
        }
        public LMCGroupResetDurableRecoveryRecord Record { get; private set; }
    }

    public sealed class LMCGroupResetDurableRecoveryTimeoutException
        : TimeoutException
    {
        internal LMCGroupResetDurableRecoveryTimeoutException(
            LMCGroupResetDurableRecoveryRecord record,
            LMCGroupResetWaitContinuation continuation)
            : base("Group Reset durable recovery attach timed out.")
        {
            Record = record;
            Continuation = continuation;
        }

        public LMCGroupResetDurableRecoveryRecord Record { get; private set; }
        public LMCGroupResetWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupResetDurableRecoveryCanceledException
        : OperationCanceledException
    {
        internal LMCGroupResetDurableRecoveryCanceledException(
            LMCGroupResetDurableRecoveryRecord record,
            LMCGroupResetWaitContinuation continuation,
            Exception innerException,
            CancellationToken cancellationToken)
            : base(
                "Group Reset durable recovery attach was canceled.",
                innerException,
                cancellationToken)
        {
            Record = record;
            Continuation = continuation;
        }

        public LMCGroupResetDurableRecoveryRecord Record { get; private set; }
        public LMCGroupResetWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupResetRejectedException
        : InvalidOperationException
    {
        internal LMCGroupResetRejectedException(
            LMCGroupResetWaitEvidence evidence)
            : base("Group Reset was rejected.")
        {
            Evidence = evidence;
        }

        public LMCGroupResetWaitEvidence Evidence { get; private set; }
        public LMC_Response Acknowledgement
        {
            get { return Evidence == null ? null : Evidence.Acknowledgement; }
        }
    }

    public sealed class LMCGroupResetSubmissionException
        : InvalidOperationException
    {
        internal LMCGroupResetSubmissionException(
            LMCGroupResetWaitEvidence evidence,
            Exception innerException)
            : base("Group Reset submission did not produce a recoverable accepted continuation.", innerException)
        {
            Evidence = evidence;
        }

        public LMCGroupResetWaitEvidence Evidence { get; private set; }
    }

    public sealed class LMCGroupResetWaitTimeoutException
        : TimeoutException
    {
        internal LMCGroupResetWaitTimeoutException(
            LMCGroupResetWaitEvidence evidence,
            LMCGroupResetWaitContinuation continuation)
            : base("Group Reset stable error-clearance wait timed out.")
        {
            Evidence = evidence;
            Continuation = continuation;
        }

        public LMCGroupResetWaitEvidence Evidence { get; private set; }
        public LMCGroupResetWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupResetWaitCanceledException
        : OperationCanceledException
    {
        internal LMCGroupResetWaitCanceledException(
            LMCGroupResetWaitEvidence evidence,
            LMCGroupResetWaitContinuation continuation,
            Exception innerException,
            CancellationToken cancellationToken)
            : base(
                "Group Reset stable error-clearance wait was canceled.",
                innerException,
                cancellationToken)
        {
            Evidence = evidence;
            Continuation = continuation;
        }

        public LMCGroupResetWaitEvidence Evidence { get; private set; }
        public LMCGroupResetWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupResetStatusException
        : InvalidOperationException
    {
        internal LMCGroupResetStatusException(
            LMCGroupResetWaitEvidence evidence,
            LMCGroupResetWaitContinuation continuation,
            LMCGroupReadStatusResult failedGroupStatus,
            LMCGroupResetMemberStatus failedMemberStatus,
            Exception innerException)
            : base("Group Reset status verification failed.", innerException)
        {
            Evidence = evidence;
            Continuation = continuation;
            FailedGroupStatus = failedGroupStatus;
            FailedMemberStatus = failedMemberStatus;
        }

        public LMCGroupResetWaitEvidence Evidence { get; private set; }
        public LMCGroupResetWaitContinuation Continuation { get; private set; }
        public LMCGroupReadStatusResult FailedGroupStatus { get; private set; }
        public LMCGroupResetMemberStatus FailedMemberStatus { get; private set; }
    }

    public sealed class LMCGroupResetInterferenceException
        : InvalidOperationException
    {
        internal LMCGroupResetInterferenceException(
            LMCGroupResetWaitEvidence evidence,
            LMCGroupResetWaitContinuation continuation)
            : base("A process-local group or captured member-axis mutation intervened after Group Reset.")
        {
            Evidence = evidence;
            Continuation = continuation;
        }

        public LMCGroupResetWaitEvidence Evidence { get; private set; }
        public LMCGroupResetWaitContinuation Continuation { get; private set; }
    }

    public sealed class LMCGroupResetWaitPendingException
        : InvalidOperationException
    {
        internal LMCGroupResetWaitPendingException(
            LMCGroupResetWaitContinuation continuation)
            : base("An accepted Group Reset is still awaiting status-only error-clearance proof.")
        {
            Continuation = continuation;
        }

        public LMCGroupResetWaitContinuation Continuation { get; private set; }
    }
}

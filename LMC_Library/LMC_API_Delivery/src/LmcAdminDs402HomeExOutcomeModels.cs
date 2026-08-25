using System;

namespace LasalMotionControlLib
{
    public enum LMCAxisDs402HomeExOutcomeRecordState : ushort
    {
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Aborted = 4
    }

    [Flags]
    public enum LMCAxisDs402HomeExCleanupProofFlags : uint
    {
        None = 0,
        StartBitLow = 1u << 0,
        CspModeRestored = 1u << 1,
        SetpointAligned = 1u << 2,
        RtOwnerReleased = 1u << 3,
        HomingParametersRestored = 1u << 4,
        SdoExecutorDrained = 1u << 5,
        RequiredForSafeTerminal = StartBitLow
            | CspModeRestored
            | SetpointAligned
            | RtOwnerReleased
            | HomingParametersRestored
            | SdoExecutorDrained
    }

    internal sealed class LMCParsedAxisDs402HomeExOutcome
    {
        internal LMCParsedAxisDs402HomeExOutcome(
            LMCAdminResponse response,
            LMCAxisDs402HomeExOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            ushort ds402StatusWord,
            int actualPosition,
            int expectedFinalPosition,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration,
            LMCAxisDs402HomeExCleanupProofFlags cleanupProofFlags,
            uint sdoExecutorToken)
        {
            Response = response;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCode = originalDetailCode;
            Ds402StatusWord = ds402StatusWord;
            ActualPosition = actualPosition;
            ExpectedFinalPosition = expectedFinalPosition;
            StartCycle = startCycle;
            CompletionCycle = completionCycle;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
            CleanupProofFlags = cleanupProofFlags;
            SdoExecutorToken = sdoExecutorToken;
        }

        internal LMCAdminResponse Response { get; private set; }
        internal LMCAxisDs402HomeExOutcomeRecordState RecordState { get; private set; }
        internal ushort OriginalCommandStatus { get; private set; }
        internal short OriginalErrorId { get; private set; }
        internal uint OriginalDetailCode { get; private set; }
        internal ushort Ds402StatusWord { get; private set; }
        internal int ActualPosition { get; private set; }
        internal int ExpectedFinalPosition { get; private set; }
        internal uint StartCycle { get; private set; }
        internal uint CompletionCycle { get; private set; }
        internal uint NativeCommandState { get; private set; }
        internal uint RecordGeneration { get; private set; }
        internal LMCAxisDs402HomeExCleanupProofFlags CleanupProofFlags { get; private set; }
        internal uint SdoExecutorToken { get; private set; }
    }

    public sealed class LMCAxisDs402HomeExOutcomeQueryException
        : LMCAdminCommandException
    {
        internal LMCAxisDs402HomeExOutcomeQueryException(
            LMCAdminResponse response,
            LMCAxisDs402HomeExRecoveryKey recoveryKey)
            : base(
                "ReadAxisDs402HomeExOutcome failed. The original HomeDS402Ex outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
        }

        public LMCAxisDs402HomeExRecoveryKey RecoveryKey { get; private set; }
    }

    public sealed class LMCAxisDs402HomeExOutcomeRetirementException
        : LMCAdminCommandException
    {
        internal LMCAxisDs402HomeExOutcomeRetirementException(
            LMCAdminResponse response,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
            uint expectedRecordGeneration)
            : base(
                "RetireAxisDs402HomeExOutcome failed. The HomeDS402Ex outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
            ExpectedRecordGeneration = expectedRecordGeneration;
        }

        public LMCAxisDs402HomeExRecoveryKey RecoveryKey { get; private set; }
        public uint ExpectedRecordGeneration { get; private set; }
    }
}

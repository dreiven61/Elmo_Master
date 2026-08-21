using System;

namespace LasalMotionControlLib
{
    public enum LMCAxisSetOperationModeOutcomeRecordState : ushort
    {
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Aborted = 4
    }

    [Flags]
    public enum LMCAxisSetOperationModeEvidenceFlags : uint
    {
        None = 0,
        WriteRequested = 1u << 0,
        WriteDispatched = 1u << 1,
        VerifyReadDispatched = 1u << 2,
        VerifyReadCompleted = 1u << 3,
        OwnerReleased = 1u << 4,
        ExecutorReusable = 1u << 5
    }

    public sealed class LMCAxisSetOperationModeOutcomeResult
    {
        internal LMCAxisSetOperationModeOutcomeResult(
            LMCAdminResponse response,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            LMCParsedAxisSetOperationModeOutcome parsed)
        {
            if (parsed == null)
            {
                throw new ArgumentNullException("parsed");
            }

            Response = response;
            RecoveryKey = recoveryKey;
            RecordState = parsed.RecordState;
            ObservedModeRaw = parsed.ObservedModeRaw;
            OriginalCommandStatus = parsed.OriginalCommandStatus;
            OriginalErrorId = parsed.OriginalErrorId;
            OriginalDetailCodeValue = parsed.OriginalDetailCode;
            SdoExecutorToken = parsed.SdoExecutorToken;
            EvidenceFlags = parsed.EvidenceFlags;
            StartCycle = parsed.StartCycle;
            CompletionCycle = parsed.CompletionCycle;
            NativeCommandState = parsed.NativeCommandState;
            RecordGeneration = parsed.RecordGeneration;
            PreviousModeRaw = parsed.PreviousModeRaw;
            QuarantineReason = parsed.QuarantineReason;
            Ds402StatusWord = parsed.Ds402StatusWord;
            ContextCheck = parsed.ContextCheck;
        }

        public LMCAdminResponse Response { get; private set; }
        public uint QueryRequestId { get { return Response.RequestId; } }
        public LMCAxisSetOperationModeRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public LMCAxisSetOperationModeOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        public sbyte ObservedModeRaw { get; private set; }
        public LMCDriveOperationMode ObservedMode
        {
            get { return (LMCDriveOperationMode)ObservedModeRaw; }
        }
        public ushort OriginalCommandStatus { get; private set; }
        public short OriginalErrorId { get; private set; }
        public uint OriginalDetailCodeValue { get; private set; }
        public LMCAdminDetailCode OriginalDetailCode
        {
            get { return (LMCAdminDetailCode)OriginalDetailCodeValue; }
        }
        public uint SdoExecutorToken { get; private set; }
        public LMCAxisSetOperationModeEvidenceFlags EvidenceFlags
        {
            get;
            private set;
        }
        public uint StartCycle { get; private set; }
        public uint CompletionCycle { get; private set; }
        public uint NativeCommandState { get; private set; }
        public uint RecordGeneration { get; private set; }
        public sbyte PreviousModeRaw { get; private set; }
        public LMCDriveOperationMode PreviousMode
        {
            get { return (LMCDriveOperationMode)PreviousModeRaw; }
        }
        public uint QuarantineReason { get; private set; }
        public ushort Ds402StatusWord { get; private set; }
        public uint ContextCheck { get; private set; }

        public bool IsTerminal
        {
            get
            {
                return RecordState
                    != LMCAxisSetOperationModeOutcomeRecordState.Running;
            }
        }

        public bool ModeChangeSucceeded
        {
            get
            {
                return RecordState
                        == LMCAxisSetOperationModeOutcomeRecordState.Succeeded
                    && OriginalCommandStatus == 0
                    && OriginalErrorId == 0
                    && OriginalDetailCodeValue == 0
                    && ObservedModeRaw == RecoveryKey.RequestedModeRaw;
            }
        }

        public bool WriteWasDispatched
        {
            get
            {
                return (EvidenceFlags
                    & LMCAxisSetOperationModeEvidenceFlags.WriteDispatched)
                    != 0;
            }
        }

        public bool SucceededWithoutWrite
        {
            get { return ModeChangeSucceeded && !WriteWasDispatched; }
        }
    }

    public sealed class LMCAxisSetOperationModeOutcomeQueryException
        : LMCAdminCommandException
    {
        internal LMCAxisSetOperationModeOutcomeQueryException(
            LMCAdminResponse response,
            LMCAxisSetOperationModeRecoveryKey recoveryKey)
            : base(
                "ReadAxisSetOperationModeOutcome failed. The original SetOperationMode outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
        }

        public LMCAxisSetOperationModeRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public uint QueryRequestId { get { return Response.RequestId; } }
    }
}

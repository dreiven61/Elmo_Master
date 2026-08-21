namespace LasalMotionControlLib
{
    public sealed class LMCAxisSetOperationModeOutcomeRetirementResult
    {
        internal LMCAxisSetOperationModeOutcomeRetirementResult(
            LMCAdminResponse response,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            LMCParsedAxisSetOperationModeOutcome parsed)
        {
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
        public uint RetireRequestId { get { return Response.RequestId; } }
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
        public uint QuarantineReason { get; private set; }
        public ushort Ds402StatusWord { get; private set; }
        public uint ContextCheck { get; private set; }
        public bool RetirementConfirmed { get { return true; } }
    }

    public sealed class LMCAxisSetOperationModeOutcomeRetirementException
        : LMCAdminCommandException
    {
        internal LMCAxisSetOperationModeOutcomeRetirementException(
            LMCAdminResponse response,
            LMCAxisSetOperationModeRecoveryKey recoveryKey,
            uint recordGeneration)
            : base(
                "RetireAxisSetOperationModeOutcome failed. The retained outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
            RecordGeneration = recordGeneration;
        }

        public LMCAxisSetOperationModeRecoveryKey RecoveryKey
        {
            get;
            private set;
        }
        public uint RecordGeneration { get; private set; }
        public uint RetireRequestId { get { return Response.RequestId; } }
    }
}

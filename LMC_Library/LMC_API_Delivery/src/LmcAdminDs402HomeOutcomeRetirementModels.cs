namespace LasalMotionControlLib
{
    /// <summary>
    /// Confirms that the exact terminal DS402 Home record and generation were
    /// retired. The PLC retains a tombstone, so resending the same exact
    /// retire request is idempotent and returns the same terminal snapshot.
    /// </summary>
    public sealed class LMCAxisDs402HomeOutcomeRetirementResult
    {
        internal LMCAxisDs402HomeOutcomeRetirementResult(
            LMCAdminResponse response,
            LMCAxisDs402HomeRecoveryKey recoveryKey,
            LMCAxisDs402HomeOutcomeRecordState recordState,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            ushort ds402StatusWord,
            int actualPosition,
            uint startCycle,
            uint completionCycle,
            uint nativeCommandState,
            uint recordGeneration)
        {
            Response = response;
            RecoveryKey = recoveryKey;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCodeValue = originalDetailCode;
            Ds402StatusWord = ds402StatusWord;
            ActualPosition = actualPosition;
            StartCycle = startCycle;
            CompletionCycle = completionCycle;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
        }

        public LMCAdminResponse Response { get; private set; }
        public uint RetireRequestId { get { return Response.RequestId; } }
        public LMCAxisDs402HomeRecoveryKey RecoveryKey { get; private set; }
        public LMCAxisDs402HomeOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        public ushort OriginalCommandStatus { get; private set; }
        public short OriginalErrorId { get; private set; }
        public uint OriginalDetailCodeValue { get; private set; }
        public ushort Ds402StatusWord { get; private set; }
        public int ActualPosition { get; private set; }
        public uint StartCycle { get; private set; }
        public uint CompletionCycle { get; private set; }
        public uint NativeCommandState { get; private set; }
        public uint RecordGeneration { get; private set; }

        public LMCAdminDetailCode OriginalDetailCode
        {
            get { return (LMCAdminDetailCode)OriginalDetailCodeValue; }
        }

        public bool RetirementConfirmed { get { return true; } }

        public bool HomingSucceeded
        {
            get
            {
                return RecordState
                        == LMCAxisDs402HomeOutcomeRecordState.Succeeded
                    && OriginalCommandStatus == 0
                    && OriginalErrorId == 0
                    && OriginalDetailCodeValue == 0;
            }
        }
    }

    /// <summary>
    /// The exact retire request was rejected. The retained Home record remains
    /// unresolved and callers may retry only with the same recovery key and
    /// record generation after correcting the reported domain condition.
    /// </summary>
    public sealed class LMCAxisDs402HomeOutcomeRetirementException
        : LMCAdminCommandException
    {
        internal LMCAxisDs402HomeOutcomeRetirementException(
            LMCAdminResponse response,
            LMCAxisDs402HomeRecoveryKey recoveryKey,
            uint recordGeneration)
            : base(
                "RetireAxisDs402HomeOutcome failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCodeValue
                    + ". The retained DS402 Home outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
            RecordGeneration = recordGeneration;
        }

        public LMCAxisDs402HomeRecoveryKey RecoveryKey { get; private set; }
        public uint RecordGeneration { get; private set; }
        public uint RetireRequestId { get { return Response.RequestId; } }
    }
}

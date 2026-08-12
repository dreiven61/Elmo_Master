namespace LasalMotionControlLib
{
    /// <summary>
    /// Confirms that the exact terminal SetPosition record and generation were
    /// retired. The paired PLC must retain an idempotent tombstone so the same
    /// exact request may be retried after a lost response.
    /// </summary>
    public sealed class LMCAxisSetPositionOutcomeRetirementResult
    {
        internal LMCAxisSetPositionOutcomeRetirementResult(
            LMCAdminResponse response,
            LMCAxisSetPositionRecoveryKey recoveryKey,
            LMCAxisSetPositionOutcomeRecordState recordState,
            int appliedPosition,
            ushort originalCommandStatus,
            short originalErrorId,
            uint originalDetailCode,
            uint nativeCommandState,
            uint recordGeneration)
        {
            Response = response;
            RecoveryKey = recoveryKey;
            RecordState = recordState;
            AppliedPosition = appliedPosition;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCodeValue = originalDetailCode;
            NativeCommandState = nativeCommandState;
            RecordGeneration = recordGeneration;
        }

        public LMCAdminResponse Response { get; private set; }
        public uint RetireRequestId { get { return Response.RequestId; } }
        public LMCAxisSetPositionRecoveryKey RecoveryKey { get; private set; }
        public LMCAxisSetPositionOutcomeRecordState RecordState
        {
            get;
            private set;
        }
        public int AppliedPosition { get; private set; }
        public ushort OriginalCommandStatus { get; private set; }
        public short OriginalErrorId { get; private set; }
        public uint OriginalDetailCodeValue { get; private set; }
        public uint NativeCommandState { get; private set; }
        public uint RecordGeneration { get; private set; }

        public LMCAdminDetailCode OriginalDetailCode
        {
            get { return (LMCAdminDetailCode)OriginalDetailCodeValue; }
        }

        public bool RetirementConfirmed { get { return true; } }

        public bool OriginalCommandSucceeded
        {
            get
            {
                return RecordState
                        == LMCAxisSetPositionOutcomeRecordState.Succeeded
                    && OriginalCommandStatus == 0
                    && OriginalErrorId == 0
                    && OriginalDetailCodeValue == 0;
            }
        }
    }

    /// <summary>
    /// The exact terminal retirement was rejected. The retained SetPosition
    /// outcome remains unresolved and no local durable journal may be cleared.
    /// </summary>
    public sealed class LMCAxisSetPositionOutcomeRetirementException
        : LMCAdminCommandException
    {
        internal LMCAxisSetPositionOutcomeRetirementException(
            LMCAdminResponse response,
            LMCAxisSetPositionRecoveryKey recoveryKey,
            uint recordGeneration)
            : base(
                "RetireAxisSetPositionOutcome failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCodeValue
                    + ". The retained SetPosition outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
            RecordGeneration = recordGeneration;
        }

        public LMCAxisSetPositionRecoveryKey RecoveryKey { get; private set; }
        public uint RecordGeneration { get; private set; }
        public uint RetireRequestId { get { return Response.RequestId; } }
    }
}

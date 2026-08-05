using System;

namespace LasalMotionControlLib
{
    public enum LMCAxisSetPositionOutcomeRecordState : ushort
    {
        Succeeded = 2,
        Rejected = 3
    }

    /// <summary>
    /// Exact terminal record returned by the read-only SetAxisPosition outcome
    /// query. A rejected record is still a successful query result.
    /// </summary>
    public sealed class LMCAxisSetPositionOutcomeResult
    {
        internal LMCAxisSetPositionOutcomeResult(
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
        public uint QueryRequestId { get { return Response.RequestId; } }
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
    /// The read-only lookup itself failed. This never proves that the original
    /// SetAxisPosition mutation was not dispatched and must not resolve a
    /// durable recovery record.
    /// </summary>
    public sealed class LMCAxisSetPositionOutcomeQueryException
        : LMCAdminCommandException
    {
        internal LMCAxisSetPositionOutcomeQueryException(
            LMCAdminResponse response,
            LMCAxisSetPositionRecoveryKey recoveryKey)
            : base(
                "ReadAxisSetPositionOutcome failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCodeValue
                    + ". The original SetAxisPosition outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
        }

        public LMCAxisSetPositionRecoveryKey RecoveryKey { get; private set; }
        public uint QueryRequestId { get { return Response.RequestId; } }
    }
}

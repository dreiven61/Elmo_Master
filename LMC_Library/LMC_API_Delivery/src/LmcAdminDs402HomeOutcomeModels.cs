using System;

namespace LasalMotionControlLib
{
    public enum LMCAxisDs402HomeOutcomeRecordState : ushort
    {
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Aborted = 4
    }

    internal static class LMCAxisDs402HomeOutcomeSemantics
    {
        private const ushort BaseStateMask = 0x006F;
        private const ushort FaultMask = 0x0008;
        private const ushort HomingErrorMask = 0x2000;

        internal static bool IsSucceeded(
            bool responseSucceeded,
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
            return responseSucceeded
                && recordState
                    == LMCAxisDs402HomeOutcomeRecordState.Succeeded
                && originalCommandStatus == 0
                && originalErrorId == 0
                && originalDetailCode == 0
                && IsAllowedBaseState(ds402StatusWord)
                && (ds402StatusWord & FaultMask) == 0
                && (ds402StatusWord & HomingErrorMask) == 0
                && actualPosition == 0
                && startCycle != 0
                && completionCycle >= startCycle
                && nativeCommandState == 0
                && recordGeneration != 0;
        }

        private static bool IsAllowedBaseState(ushort ds402StatusWord)
        {
            var baseState = (ushort)(ds402StatusWord & BaseStateMask);
            return baseState == 0x0040
                || baseState == 0x0021
                || baseState == 0x0023
                || baseState == 0x0027;
        }
    }

    /// <summary>
    /// Exact retained record returned by the read-only DS402 Home outcome
    /// query. Running is not terminal. Only Succeeded proves that the PLC-side
    /// homing state machine completed successfully for this recovery key.
    /// </summary>
    public sealed class LMCAxisDs402HomeOutcomeResult
    {
        internal LMCAxisDs402HomeOutcomeResult(
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
        public uint QueryRequestId { get { return Response.RequestId; } }
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

        public bool IsTerminal
        {
            get
            {
                return RecordState
                    != LMCAxisDs402HomeOutcomeRecordState.Running;
            }
        }

        public bool HomingSucceeded
        {
            get
            {
                return LMCAxisDs402HomeOutcomeSemantics.IsSucceeded(
                    Response != null && Response.IsSuccess,
                    RecordState,
                    OriginalCommandStatus,
                    OriginalErrorId,
                    OriginalDetailCodeValue,
                    Ds402StatusWord,
                    ActualPosition,
                    StartCycle,
                    CompletionCycle,
                    NativeCommandState,
                    RecordGeneration);
            }
        }
    }

    /// <summary>
    /// The read-only lookup itself failed. This does not prove whether the
    /// original DS402 Home command ran and must not resolve durable recovery.
    /// </summary>
    public sealed class LMCAxisDs402HomeOutcomeQueryException
        : LMCAdminCommandException
    {
        internal LMCAxisDs402HomeOutcomeQueryException(
            LMCAdminResponse response,
            LMCAxisDs402HomeRecoveryKey recoveryKey)
            : base(
                "ReadAxisDs402HomeOutcome failed. ErrorId="
                    + response.ErrorId
                    + ", DetailCode="
                    + response.DetailCodeValue
                    + ". The original DS402 Home outcome remains unresolved.",
                response)
        {
            RecoveryKey = recoveryKey;
        }

        public LMCAxisDs402HomeRecoveryKey RecoveryKey { get; private set; }
        public uint QueryRequestId { get { return Response.RequestId; } }
    }
}

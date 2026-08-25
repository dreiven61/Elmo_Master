using System;
using System.Threading;

namespace LasalMotionControlLib
{
    /// <summary>
    /// One-shot confirmation token for an already approved HomeDS402Ex
    /// execution plan. Creating this token does not approve a scale/profile.
    /// </summary>
    public sealed class LMCAxisDs402HomeExExecuteToken
    {
        private int consumed;

        private LMCAxisDs402HomeExExecuteToken()
        {
        }

        public static LMCAxisDs402HomeExExecuteToken Create()
        {
            return new LMCAxisDs402HomeExExecuteToken();
        }

        public bool IsConsumed
        {
            get { return Volatile.Read(ref consumed) != 0; }
        }

        internal void ConsumeForPreparation()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This HomeDS402Ex confirmation token has already prepared an intent.");
            }
        }
    }

    /// <summary>
    /// Frozen one-shot HomeDS402Ex command. The constructor and approved-plan
    /// preparation path remain internal until engineering scale/profile policy
    /// is explicitly qualified.
    /// </summary>
    public sealed class LMCPreparedAxisDs402HomeEx
    {
        private int consumed;

        internal LMCPreparedAxisDs402HomeEx(
            LMCConnection connectionOwner,
            LMCSingleAxis axis,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            long connectionSessionGeneration,
            LMCAxisDs402HomeExRecoveryKey recoveryKey)
        {
            ConnectionOwner = connectionOwner;
            Axis = axis;
            VerifiedCapabilities = verifiedCapabilities;
            VerifiedDiagnosticCapabilities = verifiedDiagnosticCapabilities;
            ConnectionSessionGeneration = connectionSessionGeneration;
            RecoveryKey = recoveryKey;
        }

        public LMCAxisDs402HomeExRecoveryKey RecoveryKey { get; private set; }
        public uint RequestId { get { return RecoveryKey.OriginalRequestId; } }
        public ushort AxisReference { get { return RecoveryKey.AxisReference; } }
        public LMCAxisDs402HomeExExecutionPlan ExecutionPlan
        {
            get { return RecoveryKey.ExecutionPlan; }
        }
        public long ConnectionSessionGeneration { get; private set; }
        public bool IsConsumed
        {
            get { return Volatile.Read(ref consumed) != 0; }
        }

        internal LMCConnection ConnectionOwner { get; private set; }
        internal LMCSingleAxis Axis { get; private set; }
        internal LMCAdminCapabilities VerifiedCapabilities { get; private set; }
        internal LMCDiagnosticCapabilities VerifiedDiagnosticCapabilities
        {
            get;
            private set;
        }

        internal void ThrowIfConsumed()
        {
            if (IsConsumed)
            {
                throw new InvalidOperationException(
                    "This prepared HomeDS402Ex command crossed its one-shot write boundary and cannot be replayed.");
            }
        }

        internal void ConsumeAtWriteBoundary()
        {
            if (Interlocked.CompareExchange(ref consumed, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "This prepared HomeDS402Ex command crossed its one-shot write boundary and cannot be replayed.");
            }
        }
    }

    public sealed class LMCAxisDs402HomeExStartAcknowledgement
    {
        internal LMCAxisDs402HomeExStartAcknowledgement(
            LMCAdminResponse response,
            LMCPreparedAxisDs402HomeEx preparedCommand,
            int homingMethod,
            uint nativeCommandState)
        {
            Response = response;
            PreparedCommand = preparedCommand;
            HomingMethod = homingMethod;
            NativeCommandState = nativeCommandState;
        }

        public LMCAdminResponse Response { get; private set; }
        public LMCPreparedAxisDs402HomeEx PreparedCommand { get; private set; }
        public int HomingMethod { get; private set; }
        public uint NativeCommandState { get; private set; }
        public bool IsAccepted
        {
            get { return Response != null && Response.IsSuccess; }
        }
    }

    public sealed class LMCAxisDs402HomeExRejectedException
        : LMCAdminCommandException
    {
        internal LMCAxisDs402HomeExRejectedException(
            LMCAxisDs402HomeExStartAcknowledgement acknowledgement)
            : base(
                "StartAxisDs402HomeEx was rejected. This is not homing completion evidence.",
                acknowledgement.Response)
        {
            Acknowledgement = acknowledgement;
        }

        public LMCAxisDs402HomeExStartAcknowledgement Acknowledgement
        {
            get;
            private set;
        }
    }

    public sealed class LMCAxisDs402HomeExOutcomeUncertainException
        : InvalidOperationException
    {
        internal LMCAxisDs402HomeExOutcomeUncertainException(
            LMCPreparedAxisDs402HomeEx preparedCommand,
            Exception innerException)
            : base(
                "HomeDS402Ex may have been accepted. Persist the exact recovery key, never replay Start, and resolve only through the read-only outcome lifecycle.",
                innerException)
        {
            PreparedCommand = preparedCommand;
            RecoveryKey = preparedCommand.RecoveryKey;
        }

        public LMCPreparedAxisDs402HomeEx PreparedCommand { get; private set; }
        public LMCAxisDs402HomeExRecoveryKey RecoveryKey { get; private set; }
    }

    /// <summary>
    /// Exact retained HomeDS402Ex record returned by 0x7D1C. Running is not
    /// terminal. Succeeded is valid only after the strict parser has verified
    /// the final-position readback and all safe-terminal cleanup proof flags.
    /// </summary>
    public sealed class LMCAxisDs402HomeExOutcomeResult
    {
        internal LMCAxisDs402HomeExOutcomeResult(
            LMCAdminResponse response,
            LMCAxisDs402HomeExRecoveryKey recoveryKey,
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
            RecoveryKey = recoveryKey;
            RecordState = recordState;
            OriginalCommandStatus = originalCommandStatus;
            OriginalErrorId = originalErrorId;
            OriginalDetailCodeValue = originalDetailCode;
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

        public LMCAdminResponse Response { get; private set; }
        public uint QueryRequestId { get { return Response.RequestId; } }
        public LMCAxisDs402HomeExRecoveryKey RecoveryKey { get; private set; }
        public LMCAxisDs402HomeExOutcomeRecordState RecordState { get; private set; }
        public ushort OriginalCommandStatus { get; private set; }
        public short OriginalErrorId { get; private set; }
        public uint OriginalDetailCodeValue { get; private set; }
        public ushort Ds402StatusWord { get; private set; }
        public int ActualPosition { get; private set; }
        public int ExpectedFinalPosition { get; private set; }
        public uint StartCycle { get; private set; }
        public uint CompletionCycle { get; private set; }
        public uint NativeCommandState { get; private set; }
        public uint RecordGeneration { get; private set; }
        public LMCAxisDs402HomeExCleanupProofFlags CleanupProofFlags
        {
            get;
            private set;
        }
        public uint SdoExecutorToken { get; private set; }

        public LMCAdminDetailCode OriginalDetailCode
        {
            get { return (LMCAdminDetailCode)OriginalDetailCodeValue; }
        }

        public bool IsTerminal
        {
            get
            {
                return RecordState
                    != LMCAxisDs402HomeExOutcomeRecordState.Running;
            }
        }

        public bool HomingSucceeded
        {
            get
            {
                return Response != null
                    && Response.IsSuccess
                    && RecordState
                        == LMCAxisDs402HomeExOutcomeRecordState.Succeeded
                    && OriginalCommandStatus == 0
                    && OriginalErrorId == 0
                    && OriginalDetailCodeValue == 0
                    && RecordGeneration != 0
                    && ActualPosition == ExpectedFinalPosition
                    && CleanupProofFlags
                        == LMCAxisDs402HomeExCleanupProofFlags.RequiredForSafeTerminal;
            }
        }
    }

    /// <summary>
    /// Exact terminal snapshot returned by 0x7D1D after the PLC proves the
    /// requested nonzero record generation was retired.
    /// </summary>
    public sealed class LMCAxisDs402HomeExOutcomeRetirementResult
    {
        internal LMCAxisDs402HomeExOutcomeRetirementResult(
            LMCAxisDs402HomeExOutcomeResult terminalOutcome)
        {
            if (terminalOutcome == null || !terminalOutcome.IsTerminal)
            {
                throw new ArgumentException(
                    "HomeDS402Ex retirement requires a terminal snapshot.",
                    "terminalOutcome");
            }

            TerminalOutcome = terminalOutcome;
        }

        public LMCAxisDs402HomeExOutcomeResult TerminalOutcome
        {
            get;
            private set;
        }
        public LMCAdminResponse Response { get { return TerminalOutcome.Response; } }
        public uint RetireRequestId { get { return Response.RequestId; } }
        public LMCAxisDs402HomeExRecoveryKey RecoveryKey
        {
            get { return TerminalOutcome.RecoveryKey; }
        }
        public uint RecordGeneration
        {
            get { return TerminalOutcome.RecordGeneration; }
        }
        public bool RetirementConfirmed { get { return true; } }
        public bool HomingSucceeded { get { return TerminalOutcome.HomingSucceeded; } }
    }
}

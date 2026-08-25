using System;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Rehydrates an exact persisted HomeDS402Ex recovery identity for the
    /// read-only Outcome/Retire lifecycle. This surface cannot create a
    /// prepared Start command and therefore cannot bypass engineering-profile
    /// approval.
    /// </summary>
    public static class LMCAxisDs402HomeExRecovery
    {
        public static LMCAxisDs402HomeExRecoveryKey Rehydrate(
            ushort schemaVersion,
            uint originalRequestId,
            uint diagnosticsBuild,
            uint diagnosticsBootId,
            uint mapRevision,
            LMCAxisDs402HomeExClientIntentId clientIntentId,
            ushort axisReference,
            int homingMethod,
            int position,
            int detectionVelocityLimit,
            int acceleration,
            int velocityHigh,
            int velocityLow,
            int distanceLimit,
            int torqueLimit,
            LMCDs402HomeBufferMode bufferMode,
            uint overallTimeoutMilliseconds,
            uint detectionTimeoutMilliseconds,
            byte[] spare)
        {
            var executionPlan = new LMCAxisDs402HomeExExecutionPlan(
                homingMethod,
                position,
                detectionVelocityLimit,
                acceleration,
                velocityHigh,
                velocityLow,
                distanceLimit,
                torqueLimit,
                bufferMode,
                overallTimeoutMilliseconds,
                detectionTimeoutMilliseconds,
                spare);

            return new LMCAxisDs402HomeExRecoveryKey(
                schemaVersion,
                originalRequestId,
                diagnosticsBuild,
                diagnosticsBootId,
                mapRevision,
                clientIntentId,
                axisReference,
                executionPlan);
        }
    }
}

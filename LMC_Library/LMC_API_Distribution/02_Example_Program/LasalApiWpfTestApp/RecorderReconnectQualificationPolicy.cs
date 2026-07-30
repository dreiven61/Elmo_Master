using System;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal enum RecorderReconnectCleanupRoute
    {
        OriginalSession = 0,
        ExactReconnect = 1,
        RecoveryRequired = 2
    }

    internal enum RecorderQualificationCleanupAction
    {
        StopAndRefresh = 0,
        Release = 1,
        Preserve = 2
    }

    internal static class RecorderReconnectQualificationPolicy
    {
        internal static void QuarantineUnvalidatedAdoption(
            LMCRecorderIdentity adoptedIdentity,
            bool adoptionValidated,
            Action<LMCRecorderIdentity> preserve)
        {
            if (adoptedIdentity == null)
            {
                throw new ArgumentNullException("adoptedIdentity");
            }

            if (adoptionValidated)
            {
                throw new InvalidOperationException(
                    "A validated Recorder adoption must use the normal cleanup route.");
            }

            if (preserve == null)
            {
                throw new ArgumentNullException("preserve");
            }

            preserve(adoptedIdentity);
        }

        internal static void EnsureAutomaticCleanupAllowed(
            bool hasAdoptedIdentity,
            bool adoptionValidated)
        {
            if (hasAdoptedIdentity != adoptionValidated)
            {
                throw new InvalidOperationException(
                    "Automatic cleanup requires an Adopt identity and its preserved-identity/new-owner validation to agree.");
            }
        }

        internal static RecorderReconnectCleanupRoute SelectCleanupRoute(
            bool originalSessionUsable,
            bool hasExpectation,
            bool hasAdoptedIdentity,
            bool adoptionValidated)
        {
            if (hasAdoptedIdentity != adoptionValidated)
            {
                return RecorderReconnectCleanupRoute.RecoveryRequired;
            }

            if (originalSessionUsable)
            {
                return RecorderReconnectCleanupRoute.OriginalSession;
            }

            if (hasExpectation)
            {
                return RecorderReconnectCleanupRoute.ExactReconnect;
            }

            return RecorderReconnectCleanupRoute.RecoveryRequired;
        }

        internal static bool CanContinueAfterRejectedStop(
            LMCDiagnosticsDetailCode detail,
            LMCRecorderState state)
        {
            return detail == LMCDiagnosticsDetailCode.InvalidState
                && (state == LMCRecorderState.Ready
                    || state == LMCRecorderState.Uploading);
        }

        internal static RecorderQualificationCleanupAction
            SelectCleanupAction(LMCRecorderState state)
        {
            switch (state)
            {
                case LMCRecorderState.Armed:
                case LMCRecorderState.Recording:
                    return RecorderQualificationCleanupAction.StopAndRefresh;
                case LMCRecorderState.Ready:
                case LMCRecorderState.Uploading:
                    return RecorderQualificationCleanupAction.Release;
                default:
                    return RecorderQualificationCleanupAction.Preserve;
            }
        }

        internal static bool CanRunManualCleanup(
            bool recoveryQuarantined,
            bool statusConfirmed,
            bool bufferReleasePending,
            bool configurationReleasePending,
            LMCRecorderState? confirmedState)
        {
            if (!bufferReleasePending)
            {
                return configurationReleasePending;
            }

            if (!recoveryQuarantined)
            {
                return true;
            }

            return statusConfirmed
                && confirmedState.HasValue
                && SelectCleanupAction(confirmedState.Value)
                    != RecorderQualificationCleanupAction.Preserve;
        }
    }
}

using System;
using System.Collections.Generic;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RecorderReconnectQualificationPolicyTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.RecorderReconnect.SafetyPolicy",
                SafetyPolicy);
            tests.Add(
                "Qualification.RecorderReconnect.CleanupRouteMatrix",
                CleanupRouteMatrix);
            tests.Add(
                "Qualification.RecorderReconnect.ManualRecoveryButtonPolicy",
                ManualRecoveryButtonPolicy);
        }

        private static void SafetyPolicy()
        {
            RecorderReconnectQualificationPolicy
                .EnsureAutomaticCleanupAllowed(false, false);
            RecorderReconnectQualificationPolicy
                .EnsureAutomaticCleanupAllowed(true, true);
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderReconnectQualificationPolicy
                    .EnsureAutomaticCleanupAllowed(true, false));
            AssertEx.Throws<InvalidOperationException>(
                () => RecorderReconnectQualificationPolicy
                    .EnsureAutomaticCleanupAllowed(false, true));

            AssertEx.True(
                RecorderReconnectQualificationPolicy
                    .CanContinueAfterRejectedStop(
                        LMCDiagnosticsDetailCode.InvalidState,
                        LMCRecorderState.Ready));
            AssertEx.True(
                RecorderReconnectQualificationPolicy
                    .CanContinueAfterRejectedStop(
                        LMCDiagnosticsDetailCode.InvalidState,
                        LMCRecorderState.Uploading));

            foreach (LMCRecorderState state in Enum.GetValues(
                typeof(LMCRecorderState)))
            {
                if (state == LMCRecorderState.Ready
                    || state == LMCRecorderState.Uploading)
                {
                    continue;
                }

                AssertEx.False(
                    RecorderReconnectQualificationPolicy
                        .CanContinueAfterRejectedStop(
                            LMCDiagnosticsDetailCode.InvalidState,
                            state));
            }

            AssertEx.False(
                RecorderReconnectQualificationPolicy
                    .CanContinueAfterRejectedStop(
                        LMCDiagnosticsDetailCode.ResourceBusy,
                        LMCRecorderState.Ready));

            AssertEx.Equal(
                RecorderQualificationCleanupAction.StopAndRefresh,
                RecorderReconnectQualificationPolicy.SelectCleanupAction(
                    LMCRecorderState.Armed));
            AssertEx.Equal(
                RecorderQualificationCleanupAction.StopAndRefresh,
                RecorderReconnectQualificationPolicy.SelectCleanupAction(
                    LMCRecorderState.Recording));
            AssertEx.Equal(
                RecorderQualificationCleanupAction.Release,
                RecorderReconnectQualificationPolicy.SelectCleanupAction(
                    LMCRecorderState.Ready));
            AssertEx.Equal(
                RecorderQualificationCleanupAction.Release,
                RecorderReconnectQualificationPolicy.SelectCleanupAction(
                    LMCRecorderState.Uploading));
            AssertEx.Equal(
                RecorderQualificationCleanupAction.Preserve,
                RecorderReconnectQualificationPolicy.SelectCleanupAction(
                    LMCRecorderState.Empty));
            AssertEx.Equal(
                RecorderQualificationCleanupAction.Preserve,
                RecorderReconnectQualificationPolicy.SelectCleanupAction(
                    LMCRecorderState.Configured));
            AssertEx.Equal(
                RecorderQualificationCleanupAction.Preserve,
                RecorderReconnectQualificationPolicy.SelectCleanupAction(
                    LMCRecorderState.Fault));
        }

        private static void CleanupRouteMatrix()
        {
            AssertEx.Equal(
                RecorderReconnectCleanupRoute.OriginalSession,
                RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                    true,
                    false,
                    false,
                    false));
            AssertEx.Equal(
                RecorderReconnectCleanupRoute.OriginalSession,
                RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                    true,
                    true,
                    false,
                    false));
            AssertEx.Equal(
                RecorderReconnectCleanupRoute.ExactReconnect,
                RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                    false,
                    true,
                    false,
                    false));
            AssertEx.Equal(
                RecorderReconnectCleanupRoute.ExactReconnect,
                RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                    false,
                    true,
                    true,
                    true));
            AssertEx.Equal(
                RecorderReconnectCleanupRoute.RecoveryRequired,
                RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                    false,
                    false,
                    false,
                    false));
            AssertEx.Equal(
                RecorderReconnectCleanupRoute.RecoveryRequired,
                RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                    true,
                    true,
                    true,
                    false));
            AssertEx.Equal(
                RecorderReconnectCleanupRoute.RecoveryRequired,
                RecorderReconnectQualificationPolicy.SelectCleanupRoute(
                    false,
                    true,
                    false,
                    true));
        }

        private static void ManualRecoveryButtonPolicy()
        {
            AssertEx.False(
                RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                    true,
                    false,
                    true,
                    true,
                    null));
            AssertEx.True(
                RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                    true,
                    true,
                    true,
                    true,
                    LMCRecorderState.Recording));
            AssertEx.True(
                RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                    true,
                    true,
                    true,
                    true,
                    LMCRecorderState.Ready));
            AssertEx.False(
                RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                    true,
                    true,
                    true,
                    true,
                    LMCRecorderState.Fault));
            AssertEx.True(
                RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                    true,
                    false,
                    false,
                    true,
                    null));
            AssertEx.False(
                RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                    true,
                    false,
                    false,
                    false,
                    null));
        }
    }
}

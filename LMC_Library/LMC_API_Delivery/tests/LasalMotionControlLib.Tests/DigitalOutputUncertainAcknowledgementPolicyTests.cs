using System.Collections.Generic;
using LasalMotionControlApiExample;

namespace LasalMotionControlLib.Tests
{
    internal static class DigitalOutputUncertainAcknowledgementPolicyTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Policy.DigitalOutputUncertainAcknowledgement.AllGatesRequired",
                AllGatesRequired);
            tests.Add(
                "Policy.DigitalOutputUncertainAcknowledgement.LifecycleInvalidatesVerification",
                LifecycleInvalidatesVerification);
        }

        private static void AllGatesRequired()
        {
            var values = new[] { false, true };
            foreach (var idle in values)
            {
                foreach (var outcomeUncertain in values)
                {
                    foreach (var physicalVerificationConfirmed in values)
                    {
                        var expected = idle
                            && outcomeUncertain
                            && physicalVerificationConfirmed;
                        var actual =
                            DigitalOutputUncertainAcknowledgementPolicy
                                .CanAcknowledge(
                                    idle,
                                    outcomeUncertain,
                                    physicalVerificationConfirmed);
                        AssertEx.Equal(
                            expected,
                            actual,
                            "Unexpected digital-output acknowledgement gate for idle="
                                + idle
                                + ", outcomeUncertain="
                                + outcomeUncertain
                                + ", physicalVerificationConfirmed="
                                + physicalVerificationConfirmed
                                + ".");
                    }
                }
            }
        }

        private static void LifecycleInvalidatesVerification()
        {
            var state = new DigitalOutputUncertainAcknowledgementState();
            AssertEx.False(state.CanAcknowledge(true));

            state.SetOutcomeUncertain(true);
            state.SetPhysicalVerification(true);
            AssertEx.True(state.CanAcknowledge(true));

            state.InvalidatePhysicalVerification();
            AssertEx.False(
                state.CanAcknowledge(true),
                "Tuple, selection, shadow, or new-write invalidation must clear verification.");

            state.SetPhysicalVerification(true);
            state.SetOutcomeUncertain(true);
            AssertEx.False(
                state.CanAcknowledge(true),
                "Every transition into uncertainty must require fresh verification.");

            state.SetPhysicalVerification(true);
            state.SetOutcomeUncertain(false);
            AssertEx.False(state.OutcomeUncertain);
            AssertEx.False(state.PhysicalVerificationConfirmed);
            AssertEx.False(
                state.CanAcknowledge(true),
                "Resolving uncertainty must clear the previous verification.");
        }
    }
}

namespace LasalMotionControlApiExample
{
    internal static class DigitalOutputUncertainAcknowledgementPolicy
    {
        internal static bool CanAcknowledge(
            bool idle,
            bool outcomeUncertain,
            bool physicalVerificationConfirmed)
        {
            return idle
                && outcomeUncertain
                && physicalVerificationConfirmed;
        }
    }

    internal sealed class DigitalOutputUncertainAcknowledgementState
    {
        internal bool OutcomeUncertain { get; private set; }

        internal bool PhysicalVerificationConfirmed { get; private set; }

        internal void SetOutcomeUncertain(bool value)
        {
            OutcomeUncertain = value;
            PhysicalVerificationConfirmed = false;
        }

        internal void SetPhysicalVerification(bool value)
        {
            PhysicalVerificationConfirmed = value;
        }

        internal void InvalidatePhysicalVerification()
        {
            PhysicalVerificationConfirmed = false;
        }

        internal bool CanAcknowledge(bool idle)
        {
            return DigitalOutputUncertainAcknowledgementPolicy.CanAcknowledge(
                idle,
                OutcomeUncertain,
                PhysicalVerificationConfirmed);
        }
    }
}

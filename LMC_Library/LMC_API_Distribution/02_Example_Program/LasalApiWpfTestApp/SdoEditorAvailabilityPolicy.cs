namespace LasalMotionControlApiExample
{
    internal static class SdoEditorAvailabilityPolicy
    {
        internal static bool CanEditRequest(
            bool operationRunning,
            bool exactWriteReadbackPending)
        {
            // Both flags serialize which request may be submitted; neither
            // changes the immutable request object already handed to the API.
            // Keep the draft editor available. Exact write readback is still
            // enforced by admission and exact request matching at Submit.
            return true;
        }
    }
}

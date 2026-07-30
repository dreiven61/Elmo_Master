using System.Collections.Generic;
using LasalMotionControlApiExample;

namespace LasalMotionControlLib.Tests
{
    internal static class SdoEditorAvailabilityPolicyTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Policy.SdoEditor.OrdinaryInFlightWriteRemainsEditable",
                OrdinaryInFlightWriteRemainsEditable);
            tests.Add(
                "Policy.SdoEditor.ExactWriteReadbackKeepsDraftEditable",
                ExactWriteReadbackKeepsDraftEditable);
        }

        private static void OrdinaryInFlightWriteRemainsEditable()
        {
            AssertEx.True(
                SdoEditorAvailabilityPolicy.CanEditRequest(false, false));
            AssertEx.True(
                SdoEditorAvailabilityPolicy.CanEditRequest(true, false),
                "An ordinary in-flight request must not lock preparation of the next request.");
        }

        private static void ExactWriteReadbackKeepsDraftEditable()
        {
            AssertEx.True(
                SdoEditorAvailabilityPolicy.CanEditRequest(false, true));
            AssertEx.True(
                SdoEditorAvailabilityPolicy.CanEditRequest(true, true),
                "Exact write readback must keep the draft editor available while exact matching still gates Submit.");
        }
    }
}

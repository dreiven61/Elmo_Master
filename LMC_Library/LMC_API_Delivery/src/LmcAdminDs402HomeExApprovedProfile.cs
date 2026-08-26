using System;

namespace LasalMotionControlLib
{
    public sealed partial class LMCAdmin
    {
        /// <summary>
        /// Internal HOMEEX-01/02 bridge from an explicitly approved physical-axis
        /// engineering profile to the existing one-shot HomeDS402Ex preparation
        /// lifecycle. This surface stays internal until the repository profile
        /// manifest is approved and paired runtime/capability activation occurs.
        /// </summary>
        internal LMCPreparedAxisDs402HomeEx PrepareAxisDs402HomeExApprovedProfile(
            LMCSingleAxis axis,
            LMCAxisDs402HomeExParameters parameters,
            LMCAxisDs402HomeExApprovedProfile approvedProfile,
            LMCAdminCapabilities verifiedCapabilities,
            LMCDiagnosticCapabilities verifiedDiagnosticCapabilities,
            LMCAxisDs402HomeExExecuteToken executeToken)
        {
            if (axis == null)
            {
                throw new ArgumentNullException("axis");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (approvedProfile == null)
            {
                throw new ArgumentNullException("approvedProfile");
            }
            if (verifiedDiagnosticCapabilities == null)
            {
                throw new ArgumentNullException("verifiedDiagnosticCapabilities");
            }

            if (approvedProfile.AxisReference != axis.AxisReference)
            {
                throw new InvalidOperationException(
                    "The approved HomeDS402Ex profile does not belong to the requested physical axis.");
            }

            var executionPlan = approvedProfile.CreateExecutionPlan(
                parameters,
                verifiedDiagnosticCapabilities.MapRevision);

            return PrepareAxisDs402HomeExApprovedPlan(
                axis,
                executionPlan,
                verifiedCapabilities,
                verifiedDiagnosticCapabilities,
                executeToken);
        }
    }
}

using System;

namespace LasalMotionControlLib
{
    public partial class LMCSingleAxis
    {
        internal void EnsureAdminStartAxisDs402HomeExMutationAdmission(
            byte[] request)
        {
            if (request == null
                || LMC_Frame.GetRequestCommand(request)
                    != LMC_CommandId.StartAxisDs402HomeEx)
            {
                throw new ArgumentException(
                    "The axis mutation request is not StartAxisDs402HomeEx.",
                    "request");
            }

            EnsureCurrentSessionForUse();
            EnsureAxisMutationAdmission(request);
        }
    }
}

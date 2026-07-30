using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace LasalMotionControlApiExample
{
    internal static class GroupStopQualificationOrchestrator
    {
        internal static async Task RunWithFallbackAsync(
            Func<Task> primaryAsync,
            Action releaseGateBeforeFallback,
            Func<Task> fallbackAsync,
            Func<Exception, Exception, Exception> aggregateFactory)
        {
            if (primaryAsync == null)
            {
                throw new ArgumentNullException("primaryAsync");
            }

            if (releaseGateBeforeFallback == null)
            {
                throw new ArgumentNullException(
                    "releaseGateBeforeFallback");
            }

            if (fallbackAsync == null)
            {
                throw new ArgumentNullException("fallbackAsync");
            }

            if (aggregateFactory == null)
            {
                throw new ArgumentNullException("aggregateFactory");
            }

            try
            {
                await primaryAsync();
            }
            catch (Exception primaryError)
            {
                releaseGateBeforeFallback();

                try
                {
                    await fallbackAsync();
                }
                catch (Exception cleanupError)
                {
                    throw aggregateFactory(primaryError, cleanupError);
                }

                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw new InvalidOperationException(
                    "Primary qualification failure was not rethrown.");
            }
        }
    }
}

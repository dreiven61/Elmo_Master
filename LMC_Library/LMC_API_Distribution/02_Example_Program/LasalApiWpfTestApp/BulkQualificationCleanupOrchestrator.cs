using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal static class BulkQualificationCleanupOrchestrator
    {
        internal static async Task<bool> ReleaseAndRethrowPrimaryAsync(
            LMCPIBulkReader reader,
            Exception primaryError,
            Func<LMCPIBulkReader, Task<bool>> releaseAsync,
            Func<Exception, Exception, Exception> aggregateFactory)
        {
            if (releaseAsync == null)
            {
                throw new ArgumentNullException("releaseAsync");
            }

            if (aggregateFactory == null)
            {
                throw new ArgumentNullException("aggregateFactory");
            }

            var released = false;
            Exception cleanupError = null;
            if (reader != null && !reader.IsReleased)
            {
                try
                {
                    released = await releaseAsync(reader);
                    if (!reader.IsReleased)
                    {
                        throw new InvalidOperationException(
                            "Bulk cleanup returned without releasing its reader.");
                    }
                }
                catch (Exception error)
                {
                    cleanupError = error;
                }
            }

            if (cleanupError != null)
            {
                if (primaryError != null)
                {
                    throw aggregateFactory(primaryError, cleanupError);
                }

                ExceptionDispatchInfo.Capture(cleanupError).Throw();
                throw new InvalidOperationException(
                    "Bulk cleanup failure was not rethrown.");
            }

            if (primaryError != null)
            {
                ExceptionDispatchInfo.Capture(primaryError).Throw();
                throw new InvalidOperationException(
                    "Bulk primary failure was not rethrown.");
            }

            return released;
        }
    }
}

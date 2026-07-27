using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class GroupStopStableStandbyResult
    {
        internal GroupStopStableStandbyResult(
            LMC_Response stopResponse,
            LMCGroupReadStatusResult status,
            int statusReadCount)
        {
            StopResponse = stopResponse;
            Status = status;
            StatusReadCount = statusReadCount;
        }

        internal LMC_Response StopResponse { get; private set; }
        internal LMCGroupReadStatusResult Status { get; private set; }
        internal int StatusReadCount { get; private set; }
    }

    internal static class GroupStopQualificationOrchestrator
    {
        internal const int RequiredStableStandbySamples = 3;

        internal static async Task<GroupStopStableStandbyResult>
            StopAndVerifyStableStandbyAsync(
                LMCGroupAxis group,
                int decelerationRaw,
                int jerkRaw,
                Func<Func<Task<LMC_Response>>, Task<LMC_Response>>
                    dispatchStopAsync,
                Func<Func<Task<LMCGroupReadStatusResult>>,
                    Task<LMCGroupReadStatusResult>> dispatchStatusAsync,
                int timeoutMilliseconds,
                int pollMilliseconds,
                Func<int, Task> delayAsync)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            if (dispatchStopAsync == null)
            {
                throw new ArgumentNullException("dispatchStopAsync");
            }

            if (dispatchStatusAsync == null)
            {
                throw new ArgumentNullException("dispatchStatusAsync");
            }

            if (timeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException("timeoutMilliseconds");
            }

            if (pollMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("pollMilliseconds");
            }

            if (delayAsync == null)
            {
                throw new ArgumentNullException("delayAsync");
            }

            var stopResponse = await dispatchStopAsync(
                    () => group.GroupStopAsync(
                        decelerationRaw,
                        jerkRaw,
                        CancellationToken.None));
            EnsureStopSuccess(stopResponse);

            var timeout = Stopwatch.StartNew();
            var stableStandbySamples = 0;
            var statusReadCount = 0;
            LMCGroupReadStatusResult latest = null;

            while (timeout.ElapsedMilliseconds <= timeoutMilliseconds)
            {
                latest = await dispatchStatusAsync(
                        () => group.GroupReadStatusResultAsync(
                            CancellationToken.None));
                statusReadCount++;
                EnsureStatusSuccess(latest);

                stableStandbySamples = latest.IsStandby
                    ? stableStandbySamples + 1
                    : 0;
                if (stableStandbySamples
                    >= RequiredStableStandbySamples)
                {
                    return new GroupStopStableStandbyResult(
                        stopResponse,
                        latest,
                        statusReadCount);
                }

                await delayAsync(pollMilliseconds);
            }

            throw new TimeoutException(
                "Stable Group IsStandby was not observed within "
                + timeoutMilliseconds
                + " ms after Group Stop. StatusReads="
                + statusReadCount
                + ", LastState="
                + (latest == null
                    ? "none"
                    : "0x" + latest.State.ToString("X8"))
                + ".");
        }

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

        private static void EnsureStopSuccess(LMC_Response response)
        {
            if (response != null
                && response.IsFrameValid
                && response.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                "Group Stop failed. FrameValid="
                + (response != null && response.IsFrameValid)
                + ", Status="
                + (response == null ? 0 : response.Status)
                + ", ErrorId="
                + (response == null ? 0 : response.ErrorId)
                + ".");
        }

        private static void EnsureStatusSuccess(
            LMCGroupReadStatusResult status)
        {
            if (status != null && status.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                "Group status failed after Group Stop. ErrorId="
                + (status == null ? 0 : status.ErrorId)
                + ", GroupErrorId="
                + (status == null ? 0 : status.GroupErrorId)
                + ".");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using LasalMotionControlLib.Tests;

namespace LasalMotionControlApiExample
{
    internal static class AsyncCommandGatePolicyTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Wpf.AsyncGate.FreeGateAcquiresAndReleases",
                FreeGateAcquiresAndReleases);
            tests.Add(
                "Wpf.AsyncGate.BusyGateTimesOutZeroWire",
                BusyGateTimesOutZeroWire);
            tests.Add(
                "Wpf.AsyncGate.CancellationWinsBeforeTimeout",
                CancellationWinsBeforeTimeout);
        }

        private static void FreeGateAcquiresAndReleases()
        {
            using (var gate = new SemaphoreSlim(1, 1))
            {
                AsyncCommandGatePolicy.WaitAsync(
                        gate,
                        "Read Status",
                        100,
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                AssertEx.Equal(0, gate.CurrentCount);
                gate.Release();
                AssertEx.Equal(1, gate.CurrentCount);
            }
        }

        private static void BusyGateTimesOutZeroWire()
        {
            using (var gate = new SemaphoreSlim(0, 1))
            {
                var stopwatch = Stopwatch.StartNew();
                var error = AssertEx.Throws<AsyncCommandGateTimeoutException>(
                    () => AsyncCommandGatePolicy.WaitAsync(
                            gate,
                            "Move Relative",
                            40,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult());
                stopwatch.Stop();

                AssertEx.Equal("Move Relative", error.Operation);
                AssertEx.Equal(40, error.TimeoutMilliseconds);
                AssertEx.Contains("No command was sent", error.Message);
                AssertEx.Equal(0, gate.CurrentCount);
                AssertEx.True(stopwatch.ElapsedMilliseconds < 2000);
            }
        }

        private static void CancellationWinsBeforeTimeout()
        {
            using (var gate = new SemaphoreSlim(0, 1))
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => AsyncCommandGatePolicy.WaitAsync(
                            gate,
                            "Read Position",
                            1000,
                            cancellation.Token)
                        .GetAwaiter()
                        .GetResult());
                AssertEx.Equal(0, gate.CurrentCount);
            }
        }
    }
}

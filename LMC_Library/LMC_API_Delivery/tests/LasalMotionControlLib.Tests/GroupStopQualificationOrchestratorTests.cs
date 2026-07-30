using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlApiExample;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class GroupStopQualificationOrchestratorTests
    {
        private const ushort GroupReference = 0x0100;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.GroupStop.NormalOrchestration",
                NormalOrchestration);
            tests.Add(
                "Qualification.GroupStop.InitialFailureFallbackPreservesPrimary",
                InitialFailureFallbackPreservesPrimary);
            tests.Add(
                "Qualification.GroupStop.FallbackFailureAggregates",
                FallbackFailureAggregates);
            tests.Add(
                "Qualification.GroupStop.FallbackPreservesSynchronizationContext",
                FallbackPreservesSynchronizationContext);
        }

        private static void NormalOrchestration()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = Connect(server))
            {
                var group = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var result = StopAndVerify(group);

                AssertEx.Equal(3, result.StatusPollCount);
                AssertEx.True(result.FinalStatus.IsStandby);
                AssertEx.True(result.Acknowledgement.IsSuccess);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 1, 3);
            }
        }

        private static void InitialFailureFallbackPreservesPrimary()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                StopStep(false),
                StopStep(true),
                StatusStep(),
                StatusStep(),
                StatusStep(),
                CloseStep()))
            using (var connection = Connect(server))
            {
                var group = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var gateReleased = false;
                var safeStateVerified = false;

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => GroupStopQualificationOrchestrator
                        .RunWithFallbackAsync(
                            async () =>
                            {
                                await StopAndVerifyAsync(group);
                            },
                            () => gateReleased = true,
                            async () =>
                            {
                                var result = await StopAndVerifyAsync(group);
                                safeStateVerified = result.FinalStatus.IsStandby;
                            },
                            CreateAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Contains("GroupStop was rejected", error.Message);
                AssertEx.True(gateReleased);
                AssertEx.True(safeStateVerified);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 3);
            }
        }

        private static void FallbackFailureAggregates()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                GroupLookupStep(),
                StopStep(false),
                StopStep(false),
                CloseStep()))
            using (var connection = Connect(server))
            {
                var group = new LMCGroupAxis(
                    connection,
                    "_LMCRobotBase1");
                var gateReleased = false;
                var safeStateVerified = false;

                var error = AssertEx.Throws<InvalidOperationException>(
                    () => GroupStopQualificationOrchestrator
                        .RunWithFallbackAsync(
                            async () =>
                            {
                                await StopAndVerifyAsync(group);
                            },
                            () => gateReleased = true,
                            async () =>
                            {
                                var result = await StopAndVerifyAsync(group);
                                safeStateVerified = result.FinalStatus.IsStandby;
                            },
                            CreateAggregate)
                        .GetAwaiter()
                        .GetResult());

                AssertEx.Contains("cleanup did not verify", error.Message);
                AssertEx.True(error.InnerException is AggregateException);
                var aggregate = (AggregateException)error.InnerException;
                AssertEx.Equal(2, aggregate.InnerExceptions.Count);
                AssertEx.True(gateReleased);
                AssertEx.False(safeStateVerified);

                connection.CloseConnection();
                server.Verify();
                AssertCommandCounts(server, 2, 0);
            }
        }

        private static void FallbackPreservesSynchronizationContext()
        {
            var previousContext = SynchronizationContext.Current;
            using (var context = new PumpSynchronizationContext())
            {
                SynchronizationContext.SetSynchronizationContext(context);
                try
                {
                    var callerThreadId = Thread.CurrentThread.ManagedThreadId;
                    var primaryError = new InvalidOperationException(
                        "primary qualification failure");
                    var primaryCompletion =
                        new TaskCompletionSource<int>(
                            TaskCreationOptions.RunContinuationsAsynchronously);
                    var gateThreadId = 0;
                    var fallbackThreadId = 0;
                    var fallbackSawCallerContext = false;

                    var qualification = GroupStopQualificationOrchestrator
                        .RunWithFallbackAsync(
                            () => primaryCompletion.Task,
                            () => gateThreadId =
                                Thread.CurrentThread.ManagedThreadId,
                            () =>
                            {
                                fallbackThreadId =
                                    Thread.CurrentThread.ManagedThreadId;
                                fallbackSawCallerContext = ReferenceEquals(
                                    SynchronizationContext.Current,
                                    context);
                                return Task.FromResult(0);
                            },
                            CreateAggregate);

                    ThreadPool.QueueUserWorkItem(
                        state => primaryCompletion.SetException(
                            primaryError));
                    context.RunUntilCompleted(
                        qualification,
                        5000);

                    var observed = AssertEx.Throws<InvalidOperationException>(
                        () => qualification.GetAwaiter().GetResult());
                    AssertEx.True(ReferenceEquals(primaryError, observed));
                    AssertEx.Equal(callerThreadId, gateThreadId);
                    AssertEx.Equal(callerThreadId, fallbackThreadId);
                    AssertEx.True(fallbackSawCallerContext);
                    AssertEx.True(context.PostCount > 0);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(
                        previousContext);
                }
            }
        }

        private static LMCGroupStopWaitResult StopAndVerify(
            LMCGroupAxis group)
        {
            return StopAndVerifyAsync(group).GetAwaiter().GetResult();
        }

        private static Task<LMCGroupStopWaitResult> StopAndVerifyAsync(
            LMCGroupAxis group)
        {
            return group.GroupStopAndWaitForStableStandbyAsync(
                1000,
                0,
                new LMCGroupStopWaitOptions
                {
                    TimeoutMilliseconds = 1000,
                    PollIntervalMilliseconds = 1,
                    StableSampleCount = 3
                },
                CancellationToken.None);
        }

        private static Exception CreateAggregate(
            Exception primaryError,
            Exception cleanupError)
        {
            return new InvalidOperationException(
                "Qualification failed and cleanup did not verify a safe state.",
                new AggregateException(primaryError, cleanupError));
        }

        private static LMCConnection Connect(FakeRpcServer server)
        {
            var connection = new LMCConnection();
            connection.RpcInitConnection(
                "127.0.0.1",
                server.Port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
            return connection;
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(
                0x405C,
                TestFrame.Response(0, new byte[4]));
        }

        private static FakeRpcStep GroupLookupStep()
        {
            var payload = new byte[6];
            TestFrame.WriteUInt16(payload, 4, GroupReference);
            return new FakeRpcStep(
                0x1042,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep StopStep(bool success)
        {
            return new FakeRpcStep(
                0x2085,
                TestFrame.Response(
                    success ? (ushort)0 : (ushort)1,
                    new byte[8]));
        }

        private static FakeRpcStep StatusStep()
        {
            var payload = new byte[12];
            TestFrame.WriteUInt32(payload, 0, 0x00020000u);
            TestFrame.WriteUInt16(payload, 4, 0x4000);
            return new FakeRpcStep(
                0x2045,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(0, new byte[4]));
        }

        private static void AssertCommandCounts(
            FakeRpcServer server,
            int expectedStopCount,
            int expectedStatusCount)
        {
            var stopCount = 0;
            var statusCount = 0;
            var moveCount = 0;
            foreach (var request in server.ReceivedRequests)
            {
                var command = TestFrame.ReadUInt16(request, 0);
                if (command == 0x2085)
                {
                    stopCount++;
                }
                else if (command == 0x2045)
                {
                    statusCount++;
                }
                else if (command == 0x7D22)
                {
                    moveCount++;
                }
            }

            AssertEx.Equal(expectedStopCount, stopCount);
            AssertEx.Equal(expectedStatusCount, statusCount);
            AssertEx.Equal(0, moveCount);
        }

        private sealed class PumpSynchronizationContext
            : SynchronizationContext,
              IDisposable
        {
            private readonly Queue<WorkItem> pending =
                new Queue<WorkItem>();
            private readonly AutoResetEvent workAvailable =
                new AutoResetEvent(false);

            internal int PostCount { get; private set; }

            public override void Post(
                SendOrPostCallback callback,
                object state)
            {
                lock (pending)
                {
                    pending.Enqueue(new WorkItem(callback, state));
                    PostCount++;
                }

                workAvailable.Set();
            }

            internal void RunUntilCompleted(
                Task task,
                int timeoutMilliseconds)
            {
                var timeout = Stopwatch.StartNew();
                while (!task.IsCompleted)
                {
                    WorkItem work = null;
                    lock (pending)
                    {
                        if (pending.Count > 0)
                        {
                            work = pending.Dequeue();
                        }
                    }

                    if (work != null)
                    {
                        work.Callback(work.State);
                        continue;
                    }

                    if (timeout.ElapsedMilliseconds
                        >= timeoutMilliseconds)
                    {
                        throw new TimeoutException(
                            "SynchronizationContext pump timed out.");
                    }

                    workAvailable.WaitOne(20);
                }
            }

            public void Dispose()
            {
                workAvailable.Dispose();
            }

            private sealed class WorkItem
            {
                internal WorkItem(
                    SendOrPostCallback callback,
                    object state)
                {
                    Callback = callback;
                    State = state;
                }

                internal SendOrPostCallback Callback { get; private set; }
                internal object State { get; private set; }
            }
        }
    }
}

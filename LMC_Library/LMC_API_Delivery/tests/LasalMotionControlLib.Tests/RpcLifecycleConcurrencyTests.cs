using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RpcLifecycleConcurrencyTests
    {
        private const int WaitTimeoutMilliseconds = 3000;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Rpc.LifecycleConcurrency.CloseWaitsForInFlightAndRejectsQueued",
                CloseWaitsForInFlightAndRejectsQueued);
            tests.Add(
                "Rpc.LifecycleConcurrency.ConcurrentCloseSendsOnce",
                ConcurrentCloseSendsOnce);
            tests.Add(
                "Rpc.LifecycleConcurrency.CancelledCloseBehindInitPreservesInit",
                CancelledCloseBehindInitPreservesInit);
            tests.Add(
                "Rpc.LifecycleConcurrency.ConcurrentInitSerializesAndSecondWins",
                ConcurrentInitSerializesAndSecondWins);
            tests.Add(
                "Rpc.LifecycleConcurrency.CancelledCloseAbortsInFlightLocally",
                CancelledCloseAbortsInFlightLocally);
            tests.Add(
                "Rpc.LifecycleConcurrency.LateTransportFaultCannotOverwriteExplicitClose",
                LateTransportFaultCannotOverwriteExplicitClose);
            tests.Add(
                "Rpc.LifecycleConcurrency.LateTransportFaultCannotDamageReplacementInit",
                LateTransportFaultCannotDamageReplacementInit);
            tests.Add(
                "Rpc.LifecycleConcurrency.CancelledInitDuringSessionHandshakeCleansUp",
                CancelledInitDuringSessionHandshakeCleansUp);
            tests.Add(
                "Rpc.LifecycleConcurrency.CancelledInitDuringCallbackHandshakeCleansUp",
                CancelledInitDuringCallbackHandshakeCleansUp);
            tests.Add(
                "Rpc.LifecycleConcurrency.ConnectingStateRejectsReentrantShutdown",
                ConnectingStateRejectsReentrantShutdown);
            tests.Add(
                "Rpc.LifecycleConcurrency.ClosingStateRejectsReentrantInit",
                ClosingStateRejectsReentrantInit);
            tests.Add(
                "Rpc.LifecycleConcurrency.ClosingStateRejectsReentrantAsyncInit",
                ClosingStateRejectsReentrantAsyncInit);
            tests.Add(
                "Rpc.LifecycleConcurrency.ClosingStateRejectsReentrantShutdown",
                ClosingStateRejectsReentrantShutdown);
            tests.Add(
                "Rpc.LifecycleConcurrency.StateEventTaskRunReentryRejectsWithoutDeadlock",
                StateEventTaskRunReentryRejectsWithoutDeadlock);
            tests.Add(
                "Rpc.LifecycleConcurrency.NestedConnectionStateEventsPreserveOuterGuard",
                NestedConnectionStateEventsPreserveOuterGuard);
            tests.Add(
                "Rpc.LifecycleConcurrency.ConcurrentCloseAndDisposeSendsOnce",
                ConcurrentCloseAndDisposeSendsOnce);
            tests.Add(
                "Rpc.LifecycleConcurrency.ConcurrentDisposeSendsOnce",
                ConcurrentDisposeSendsOnce);
            tests.Add(
                "Rpc.LifecycleConcurrency.CloseHonorsBoundedCallbackJoin",
                CloseHonorsBoundedCallbackJoin);
            tests.Add(
                "Rpc.LifecycleConcurrency.StaleCallbackFailureCannotLeakIntoReplacement",
                StaleCallbackFailureCannotLeakIntoReplacement);
        }

        private static void CloseWaitsForInFlightAndRejectsQueued()
        {
            using (var requestReceived = new ManualResetEventSlim(false))
            using (var releaseResponse = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                BlockingStep(
                    0x2045,
                    TestFrame.Response(0, new byte[12]),
                    requestReceived,
                    releaseResponse),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                Initialize(connection, server.Port);
                var active = connection.ExchangeAsync(
                    LMC_Frame.LMCGroupReadStatus(0x0100),
                    CancellationToken.None);
                AssertSignaled(requestReceived, "active request");

                var close = connection.CloseConnectionAsync(
                    CancellationToken.None);
                AssertState(connection, LMCConnectionState.Closing);

                var queued = connection.ExchangeAsync(
                    LMC_Frame.LMCGroupEnable(0x0100),
                    CancellationToken.None);
                AssertTaskStarted(queued, "queued request");
                releaseResponse.Set();

                AssertEx.Equal(20, active.GetAwaiter().GetResult().Length);
                close.GetAwaiter().GetResult();
                AssertEx.Throws<InvalidOperationException>(
                    () => queued.GetAwaiter().GetResult());
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void ConcurrentCloseSendsOnce()
        {
            using (var closeReceived = new ManualResetEventSlim(false))
            using (var releaseClose = new ManualResetEventSlim(false))
            using (var secondStarted = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                BlockingStep(
                    0x405D,
                    SuccessAcknowledgement(),
                    closeReceived,
                    releaseClose)))
            using (var connection = new LMCConnection())
            {
                Initialize(connection, server.Port);
                var first = connection.CloseConnectionAsync(
                    CancellationToken.None);
                AssertSignaled(closeReceived, "first close request");

                var second = Task.Run(
                    () =>
                    {
                        secondStarted.Set();
                        connection.CloseConnection();
                    });
                AssertSignaled(secondStarted, "second close caller");
                releaseClose.Set();

                first.GetAwaiter().GetResult();
                second.GetAwaiter().GetResult();
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void CancelledCloseBehindInitPreservesInit()
        {
            using (var initReceived = new ManualResetEventSlim(false))
            using (var releaseInit = new ManualResetEventSlim(false))
            using (var closeStarted = new ManualResetEventSlim(false))
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                BlockingStep(
                    0x8080,
                    InitResponse(),
                    initReceived,
                    releaseInit),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var init = connection.RpcInitConnectionAsync(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask,
                    CancellationToken.None);
                AssertSignaled(initReceived, "init request");
                AssertState(connection, LMCConnectionState.Connecting);

                var close = Task.Run(
                    () =>
                    {
                        closeStarted.Set();
                        connection.CloseConnectionAsync(
                            cancellation.Token).GetAwaiter().GetResult();
                    });
                AssertSignaled(closeStarted, "queued close caller");
                cancellation.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => close.GetAwaiter().GetResult());
                AssertEx.Equal(
                    LMCConnectionState.Connecting,
                    connection.State,
                    "A close cancelled before lifecycle ownership must not alter init state.");

                releaseInit.Set();
                init.GetAwaiter().GetResult();
                AssertEx.True(connection.IsConnected);
                AssertEx.True(connection.IsRpcInitialized);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ConcurrentInitSerializesAndSecondWins()
        {
            using (var firstInitReceived = new ManualResetEventSlim(false))
            using (var releaseFirstInit = new ManualResetEventSlim(false))
            using (var replacementStarted = new ManualResetEventSlim(false))
            using (var firstCloseReceived = new ManualResetEventSlim(false))
            using (var releaseFirstClose = new ManualResetEventSlim(false))
            using (var firstServer = new FakeRpcServer(
                BlockingStep(
                    0x8080,
                    InitResponse(),
                    firstInitReceived,
                    releaseFirstInit),
                CallbackStep(),
                BlockingStep(
                    0x405D,
                    SuccessAcknowledgement(),
                    firstCloseReceived,
                    releaseFirstClose)))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var first = connection.RpcInitConnectionAsync(
                    "127.0.0.1",
                    firstServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask,
                    CancellationToken.None);
                AssertSignaled(firstInitReceived, "first init request");
                var firstGeneration = connection.SessionGeneration;
                AssertEx.True(firstGeneration > 0);

                var replacement = Task.Run(
                    () =>
                    {
                        replacementStarted.Set();
                        connection.RpcInitConnectionAsync(
                            "127.0.0.1",
                            secondServer.Port,
                            "127.0.0.1",
                            0,
                            LMCConnection.DefaultEventMask,
                            CancellationToken.None).GetAwaiter().GetResult();
                    });
                AssertSignaled(replacementStarted, "replacement init caller");
                releaseFirstInit.Set();

                first.GetAwaiter().GetResult();
                AssertSignaled(firstCloseReceived, "replacement close request");
                var replacementGeneration = connection.SessionGeneration;
                AssertEx.True(
                    replacementGeneration > firstGeneration,
                    "Replacement init must reserve a new session before replacing the old transport.");
                AssertEx.Equal(LMCConnectionState.Closing, connection.State);

                releaseFirstClose.Set();
                replacement.GetAwaiter().GetResult();
                AssertEx.True(connection.IsConnected);
                AssertEx.True(connection.IsRpcInitialized);
                AssertEx.Equal(
                    replacementGeneration,
                    connection.SessionGeneration,
                    "Replacement init must publish its reserved session generation.");

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void CancelledCloseAbortsInFlightLocally()
        {
            using (var requestReceived = new ManualResetEventSlim(false))
            using (var releaseResponse = new ManualResetEventSlim(false))
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x2045,
                    TestFrame.Response(0, new byte[12]))
                {
                    InspectRequest = request =>
                    {
                        requestReceived.Set();
                        AssertEx.True(
                            releaseResponse.Wait(WaitTimeoutMilliseconds),
                            "Timed out waiting to release the in-flight response.");
                    },
                    AllowClientDisconnectAfterRequest = true
                }))
            using (var connection = new LMCConnection())
            {
                Initialize(connection, server.Port);
                var active = connection.ExchangeAsync(
                    LMC_Frame.LMCGroupReadStatus(0x0100),
                    CancellationToken.None);
                AssertSignaled(requestReceived, "active request");

                var close = connection.CloseConnectionAsync(
                    cancellation.Token);
                AssertState(connection, LMCConnectionState.Closing);
                cancellation.Cancel();
                AssertEx.Throws<OperationCanceledException>(
                    () => close.GetAwaiter().GetResult());
                AssertDisconnected(connection);

                releaseResponse.Set();
                AssertTransportFailure(active);
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void LateTransportFaultCannotOverwriteExplicitClose()
        {
            var options = new LMCConnectionOptions
            {
                CallbackThreadJoinTimeoutMilliseconds =
                    WaitTimeoutMilliseconds
            };
            var faultedTransitions = 0;

            using (var callbackEntered = new ManualResetEventSlim(false))
            using (var releaseCallback = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x2045, new byte[0])
                {
                    CloseAfterResponse = true
                }))
            using (var connection = new LMCConnection(options))
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState == LMCConnectionState.Faulted)
                    {
                        Interlocked.Increment(ref faultedTransitions);
                    }
                };
                connection.CallbackReceived += delegate
                {
                    callbackEntered.Set();
                    AssertEx.True(
                        releaseCallback.Wait(WaitTimeoutMilliseconds),
                        "Timed out waiting to release the callback handler.");
                };

                Task<byte[]> active = null;
                try
                {
                    Initialize(connection, server.Port);
                    SendCallback(connection);
                    AssertSignaled(callbackEntered, "callback handler");

                    active = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(0x0100),
                        CancellationToken.None);
                    AssertEx.True(
                        SpinWait.SpinUntil(
                            () => !connection.IsCallbackListenerRunning,
                            WaitTimeoutMilliseconds),
                        "Transport invalidation did not detach the callback listener.");
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State,
                        "The fault transition must still be waiting for the callback handler.");

                    connection.CloseConnection();
                    AssertDisconnected(connection);
                }
                finally
                {
                    releaseCallback.Set();
                }

                AssertTransportFailure(active);
                AssertDisconnected(connection);
                AssertEx.Equal(
                    0,
                    Interlocked.CompareExchange(
                        ref faultedTransitions,
                        0,
                        0),
                    "A detached transport must not overwrite an explicit close with Faulted.");
                server.Verify();
            }
        }

        private static void LateTransportFaultCannotDamageReplacementInit()
        {
            var options = new LMCConnectionOptions
            {
                CallbackThreadJoinTimeoutMilliseconds =
                    WaitTimeoutMilliseconds
            };
            var faultedTransitions = 0;

            using (var callbackEntered = new ManualResetEventSlim(false))
            using (var releaseCallback = new ManualResetEventSlim(false))
            using (var activeServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x2045, new byte[0])
                {
                    CloseAfterResponse = true
                }))
            using (var replacementServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState == LMCConnectionState.Faulted)
                    {
                        Interlocked.Increment(ref faultedTransitions);
                    }
                };
                connection.CallbackReceived += delegate
                {
                    callbackEntered.Set();
                    AssertEx.True(
                        releaseCallback.Wait(WaitTimeoutMilliseconds),
                        "Timed out waiting to release the callback handler.");
                };

                Task<byte[]> active = null;
                Task replacement = null;
                try
                {
                    Initialize(connection, activeServer.Port);
                    SendCallback(connection);
                    AssertSignaled(callbackEntered, "callback handler");

                    active = connection.ExchangeAsync(
                        LMC_Frame.LMCGroupReadStatus(0x0100),
                        CancellationToken.None);
                    AssertEx.True(
                        SpinWait.SpinUntil(
                            () => !connection.IsCallbackListenerRunning,
                            WaitTimeoutMilliseconds),
                        "Transport invalidation did not detach the callback listener.");

                    replacement = Task.Run(
                        () => Initialize(
                            connection,
                            replacementServer.Port));
                    AssertState(connection, LMCConnectionState.Connecting);
                }
                finally
                {
                    releaseCallback.Set();
                }

                replacement.GetAwaiter().GetResult();
                AssertTransportFailure(active);
                AssertEx.True(connection.IsConnected);
                AssertEx.True(connection.IsRpcInitialized);
                AssertEx.True(connection.IsCallbackListenerRunning);
                AssertEx.NotNull(connection.CallbackLocalEndPoint);
                AssertEx.NotNull(connection.RpcSessionInitResponse);
                AssertEx.NotNull(connection.RpcCallbackRegistrationResponse);
                AssertEx.Equal(
                    0,
                    Interlocked.CompareExchange(
                        ref faultedTransitions,
                        0,
                        0),
                    "The old transport must not fault the replacement session.");

                connection.CloseConnection();
                AssertDisconnected(connection);
                activeServer.Verify();
                replacementServer.Verify();
            }
        }

        private static void CancelledInitDuringSessionHandshakeCleansUp()
        {
            using (var requestReceived = new ManualResetEventSlim(false))
            using (var releaseResponse = new ManualResetEventSlim(false))
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                BlockingDisconnectStep(
                    0x8080,
                    InitResponse(),
                    requestReceived,
                    releaseResponse)))
            using (var connection = new LMCConnection())
            {
                var init = connection.RpcInitConnectionAsync(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask,
                    cancellation.Token);
                AssertSignaled(requestReceived, "session init request");

                try
                {
                    cancellation.Cancel();
                }
                finally
                {
                    releaseResponse.Set();
                }

                AssertEx.Throws<OperationCanceledException>(
                    () => init.GetAwaiter().GetResult());
                AssertDisconnected(connection);
                AssertEx.True(
                    connection.LastInitializationException
                        is OperationCanceledException);
                server.Verify();
            }
        }

        private static void CancelledInitDuringCallbackHandshakeCleansUp()
        {
            using (var requestReceived = new ManualResetEventSlim(false))
            using (var releaseResponse = new ManualResetEventSlim(false))
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                BlockingDisconnectStep(
                    0x405C,
                    SuccessAcknowledgement(),
                    requestReceived,
                    releaseResponse)))
            using (var connection = new LMCConnection())
            {
                var init = connection.RpcInitConnectionAsync(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask,
                    cancellation.Token);
                AssertSignaled(requestReceived, "callback registration request");

                try
                {
                    cancellation.Cancel();
                }
                finally
                {
                    releaseResponse.Set();
                }

                AssertEx.Throws<OperationCanceledException>(
                    () => init.GetAwaiter().GetResult());
                AssertDisconnected(connection);
                AssertEx.True(
                    connection.LastInitializationException
                        is OperationCanceledException);
                server.Verify();
            }
        }

        private static void ConnectingStateRejectsReentrantShutdown()
        {
            Exception closeException = null;
            Exception disposeException = null;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState != LMCConnectionState.Connecting)
                    {
                        return;
                    }

                    closeException = CaptureException(
                        () => connection.CloseConnection());
                    disposeException = CaptureException(
                        () => connection.Dispose());
                };

                Initialize(connection, server.Port);
                AssertLifecycleReentryRejected(
                    closeException,
                    "CloseConnection during Connecting");
                AssertLifecycleReentryRejected(
                    disposeException,
                    "Dispose during Connecting");
                AssertEx.True(connection.IsConnected);
                AssertEx.True(connection.IsRpcInitialized);

                connection.CloseConnection();
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void ClosingStateRejectsReentrantInit()
        {
            Exception initException = null;
            var handled = 0;

            using (var activeServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var replacementServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState != LMCConnectionState.Closing
                        || Interlocked.Exchange(ref handled, 1) != 0)
                    {
                        return;
                    }

                    initException = CaptureException(
                        () => connection.RpcInitConnection(
                            "127.0.0.1",
                            replacementServer.Port,
                            "127.0.0.1",
                            0,
                            LMCConnection.DefaultEventMask));
                };

                Initialize(connection, activeServer.Port);
                connection.CloseConnection();

                AssertLifecycleReentryRejected(
                    initException,
                    "RpcInitConnection during Closing");
                AssertEx.Equal(
                    0,
                    replacementServer.ReceivedRequests.Count,
                    "Rejected replacement init must send no wire data.");
                AssertDisconnected(connection);
                activeServer.Verify();
            }
        }

        private static void ClosingStateRejectsReentrantAsyncInit()
        {
            Exception initException = null;
            Task unexpectedInit = null;
            var handled = 0;

            using (var activeServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var replacementServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState != LMCConnectionState.Closing
                        || Interlocked.Exchange(ref handled, 1) != 0)
                    {
                        return;
                    }

                    initException = CaptureException(
                        () => unexpectedInit = connection.RpcInitConnectionAsync(
                            "127.0.0.1",
                            replacementServer.Port,
                            "127.0.0.1",
                            0,
                            LMCConnection.DefaultEventMask,
                            CancellationToken.None));
                };

                Initialize(connection, activeServer.Port);
                connection.CloseConnection();

                if (unexpectedInit != null)
                {
                    unexpectedInit.GetAwaiter().GetResult();
                }

                AssertLifecycleReentryRejected(
                    initException,
                    "RpcInitConnectionAsync during Closing");
                AssertEx.Equal<Task>(
                    null,
                    unexpectedInit,
                    "Rejected async init must not queue background work.");
                AssertEx.Equal(
                    0,
                    replacementServer.ReceivedRequests.Count,
                    "Rejected async replacement init must send no wire data.");
                AssertDisconnected(connection);
                activeServer.Verify();
            }
        }

        private static void ClosingStateRejectsReentrantShutdown()
        {
            Exception closeException = null;
            Exception asyncCloseException = null;
            Exception disposeException = null;
            Task unexpectedClose = null;
            var handled = 0;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState != LMCConnectionState.Closing
                        || Interlocked.Exchange(ref handled, 1) != 0)
                    {
                        return;
                    }

                    closeException = CaptureException(
                        () => connection.CloseConnection());
                    asyncCloseException = CaptureException(
                        () => unexpectedClose = connection.CloseConnectionAsync(
                            CancellationToken.None));
                    disposeException = CaptureException(
                        () => connection.Dispose());
                };

                Initialize(connection, server.Port);
                connection.CloseConnection();

                if (unexpectedClose != null)
                {
                    unexpectedClose.GetAwaiter().GetResult();
                }

                AssertLifecycleReentryRejected(
                    closeException,
                    "CloseConnection during Closing");
                AssertLifecycleReentryRejected(
                    asyncCloseException,
                    "CloseConnectionAsync during Closing");
                AssertLifecycleReentryRejected(
                    disposeException,
                    "Dispose during Closing");
                AssertEx.Equal<Task>(
                    null,
                    unexpectedClose,
                    "Rejected async close must not queue background work.");
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void StateEventTaskRunReentryRejectsWithoutDeadlock()
        {
            Exception reentryException = null;
            Exception handlerException = null;
            Task reentry = null;
            var handled = 0;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState != LMCConnectionState.Closing
                        || Interlocked.Exchange(ref handled, 1) != 0)
                    {
                        return;
                    }

                    try
                    {
                        reentry = Task.Run(
                            () =>
                            {
                                reentryException = CaptureException(
                                    () => connection.CloseConnection());
                            });
                        AssertEx.True(
                            reentry.Wait(WaitTimeoutMilliseconds),
                            "Task.Run lifecycle reentry deadlocked the state handler.");
                    }
                    catch (Exception exception)
                    {
                        handlerException = exception;
                    }
                };

                Initialize(connection, server.Port);
                connection.CloseConnection();
                if (reentry != null && !reentry.IsCompleted)
                {
                    AssertEx.True(
                        reentry.Wait(WaitTimeoutMilliseconds),
                        "Task.Run lifecycle reentry did not terminate after outer close.");
                }

                AssertEx.Equal<Exception>(null, handlerException);
                AssertLifecycleReentryRejected(
                    reentryException,
                    "Task.Run CloseConnection during Closing");
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void NestedConnectionStateEventsPreserveOuterGuard()
        {
            Exception outerHandlerException = null;
            Exception innerHandlerException = null;
            Exception nestedReentryException = null;
            Task nestedReentry = null;
            var outerHandled = 0;
            var innerHandled = 0;

            using (var outerServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var innerServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var outerConnection = new LMCConnection())
            using (var innerConnection = new LMCConnection())
            {
                outerConnection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState != LMCConnectionState.Closing
                        || Interlocked.Exchange(ref outerHandled, 1) != 0)
                    {
                        return;
                    }

                    outerHandlerException = CaptureException(
                        () => innerConnection.CloseConnection());
                };
                innerConnection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState != LMCConnectionState.Closing
                        || Interlocked.Exchange(ref innerHandled, 1) != 0)
                    {
                        return;
                    }

                    try
                    {
                        nestedReentry = Task.Run(
                            () =>
                            {
                                nestedReentryException = CaptureException(
                                    () => outerConnection.CloseConnection());
                            });
                        AssertEx.True(
                            nestedReentry.Wait(WaitTimeoutMilliseconds),
                            "Nested state-event reentry deadlocked the outer connection.");
                    }
                    catch (Exception exception)
                    {
                        innerHandlerException = exception;
                    }
                };

                Initialize(outerConnection, outerServer.Port);
                Initialize(innerConnection, innerServer.Port);
                outerConnection.CloseConnection();
                if (nestedReentry != null && !nestedReentry.IsCompleted)
                {
                    AssertEx.True(
                        nestedReentry.Wait(WaitTimeoutMilliseconds),
                        "Nested state-event reentry did not terminate after outer close.");
                }

                AssertEx.Equal<Exception>(null, outerHandlerException);
                AssertEx.Equal<Exception>(null, innerHandlerException);
                AssertLifecycleReentryRejected(
                    nestedReentryException,
                    "Nested A-to-B-to-A CloseConnection");
                AssertDisconnected(outerConnection);
                AssertDisconnected(innerConnection);
                outerServer.Verify();
                innerServer.Verify();
            }
        }

        private static void ConcurrentCloseAndDisposeSendsOnce()
        {
            using (var closeReceived = new ManualResetEventSlim(false))
            using (var releaseClose = new ManualResetEventSlim(false))
            using (var secondStarted = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                BlockingStep(
                    0x405D,
                    SuccessAcknowledgement(),
                    closeReceived,
                    releaseClose)))
            using (var connection = new LMCConnection())
            {
                Initialize(connection, server.Port);
                var close = connection.CloseConnectionAsync(
                    CancellationToken.None);
                AssertSignaled(closeReceived, "close request");

                var dispose = Task.Run(
                    () =>
                    {
                        secondStarted.Set();
                        connection.Dispose();
                    });
                AssertSignaled(secondStarted, "dispose caller");
                releaseClose.Set();

                close.GetAwaiter().GetResult();
                dispose.GetAwaiter().GetResult();
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void ConcurrentDisposeSendsOnce()
        {
            using (var closeReceived = new ManualResetEventSlim(false))
            using (var releaseClose = new ManualResetEventSlim(false))
            using (var secondStarted = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                BlockingStep(
                    0x405D,
                    SuccessAcknowledgement(),
                    closeReceived,
                    releaseClose)))
            using (var connection = new LMCConnection())
            {
                Initialize(connection, server.Port);
                var first = Task.Run(() => connection.Dispose());
                AssertSignaled(closeReceived, "first dispose request");

                var second = Task.Run(
                    () =>
                    {
                        secondStarted.Set();
                        connection.Dispose();
                    });
                AssertSignaled(secondStarted, "second dispose caller");
                releaseClose.Set();

                first.GetAwaiter().GetResult();
                second.GetAwaiter().GetResult();
                AssertDisconnected(connection);
                server.Verify();
            }
        }

        private static void CloseHonorsBoundedCallbackJoin()
        {
            var options = new LMCConnectionOptions
            {
                CallbackThreadJoinTimeoutMilliseconds = 200
            };

            using (var callbackEntered = new ManualResetEventSlim(false))
            using (var releaseCallback = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                connection.CallbackReceived += delegate
                {
                    callbackEntered.Set();
                    AssertEx.True(
                        releaseCallback.Wait(WaitTimeoutMilliseconds),
                        "Timed out waiting to release the callback handler.");
                };

                Task close = null;
                try
                {
                    Initialize(connection, server.Port);
                    SendCallback(connection);
                    AssertSignaled(callbackEntered, "callback handler");

                    close = Task.Run(() => connection.CloseConnection());
                    AssertEx.True(
                        close.Wait(2000),
                        "Close did not honor the configured callback join bound.");
                    AssertDisconnected(connection);
                }
                finally
                {
                    releaseCallback.Set();
                }

                if (close != null && !close.IsCompleted)
                {
                    AssertEx.True(
                        close.Wait(WaitTimeoutMilliseconds),
                        "Close did not finish after releasing the callback handler.");
                }
                server.Verify();
            }
        }

        private static void StaleCallbackFailureCannotLeakIntoReplacement()
        {
            const string oldFailureMessage = "old callback generation failure";
            const string currentFailureMessage =
                "current callback generation failure";
            var options = new LMCConnectionOptions
            {
                CallbackThreadJoinTimeoutMilliseconds = 100
            };

            using (var oldCallbackEntered = new ManualResetEventSlim(false))
            using (var releaseOldCallback = new ManualResetEventSlim(false))
            using (var currentErrorReported = new ManualResetEventSlim(false))
            using (var replacementCallbackReceived =
                new ManualResetEventSlim(false))
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var replacementServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                Thread oldCallbackThread = null;
                var callbackCount = 0;
                var errorCount = 0;
                string reportedErrorMessage = null;
                LMCCallbackEventArgs oldCallback = null;
                LMCCallbackEventArgs replacementCallback = null;

                connection.CallbackReceived += delegate(
                    object sender,
                    LMCCallbackEventArgs e)
                {
                    var invocation = Interlocked.Increment(
                        ref callbackCount);
                    if (invocation == 1)
                    {
                        oldCallback = e;
                        AssertEx.True(e.BelongsTo(connection));
                        AssertEx.True(
                            e.BelongsToCurrentSession(connection),
                            "The first callback must be current before Close.");
                        oldCallbackThread = Thread.CurrentThread;
                        oldCallbackEntered.Set();
                        AssertEx.True(
                            releaseOldCallback.Wait(
                                WaitTimeoutMilliseconds),
                            "Timed out waiting to release the old callback handler.");
                        AssertEx.False(
                            e.BelongsToCurrentSession(connection),
                            "The blocked old handler must remain stale after replacement.");
                        throw new InvalidOperationException(
                            oldFailureMessage);
                    }

                    if (invocation == 2)
                    {
                        replacementCallback = e;
                        AssertEx.True(e.BelongsTo(connection));
                        AssertEx.True(
                            e.BelongsToCurrentSession(connection),
                            "The replacement callback must own the current session.");
                        throw new InvalidOperationException(
                            currentFailureMessage);
                    }

                    AssertEx.True(e.BelongsTo(connection));
                    AssertEx.True(e.BelongsToCurrentSession(connection));
                    replacementCallbackReceived.Set();
                };
                connection.CallbackListenerError += delegate(
                    object sender,
                    LMCCallbackErrorEventArgs e)
                {
                    reportedErrorMessage = e.Exception.Message;
                    Interlocked.Increment(ref errorCount);
                    currentErrorReported.Set();
                };

                try
                {
                    Initialize(connection, firstServer.Port);
                    SendCallback(connection);
                    AssertSignaled(
                        oldCallbackEntered,
                        "old callback handler");
                    AssertEx.NotNull(oldCallbackThread);
                    AssertEx.NotNull(oldCallback);
                    var oldSessionGeneration =
                        oldCallback.SessionGeneration;
                    AssertEx.True(oldSessionGeneration > 0);

                    var close = Task.Run(
                        () => connection.CloseConnection());
                    AssertEx.True(
                        close.Wait(2000),
                        "Close did not honor the callback join bound before replacement.");
                    close.GetAwaiter().GetResult();
                    AssertDisconnected(connection);
                    AssertEx.True(oldCallback.BelongsTo(connection));
                    AssertEx.False(
                        oldCallback.BelongsToCurrentSession(connection),
                        "Close must stale an event even while its handler is blocked.");
                    firstServer.Verify();

                    Initialize(connection, replacementServer.Port);
                    AssertEx.True(connection.IsConnected);
                    AssertEx.True(connection.IsCallbackListenerRunning);
                    AssertEx.Equal(0L, connection.RejectedCallbackCount);
                    AssertEx.False(
                        oldCallback.BelongsToCurrentSession(connection),
                        "Reconnect must not revalidate an old callback event.");

                    SendCallback(connection);
                    AssertSignaled(
                        currentErrorReported,
                        "current-generation callback error");
                    AssertEx.Equal(1, Volatile.Read(ref errorCount));
                    AssertEx.Equal(
                        currentFailureMessage,
                        reportedErrorMessage);
                    AssertEx.NotNull(replacementCallback);
                    AssertEx.True(
                        replacementCallback.SessionGeneration
                            > oldSessionGeneration,
                        "Replacement callbacks must capture the new session generation.");
                    AssertEx.True(
                        replacementCallback.BelongsToCurrentSession(connection));
                    AssertEx.False(
                        oldCallback.BelongsToCurrentSession(connection));

                    releaseOldCallback.Set();
                    AssertEx.True(
                        oldCallbackThread.Join(
                            WaitTimeoutMilliseconds),
                        "The old callback thread did not finish after release.");
                    AssertEx.Equal(
                        1,
                        Volatile.Read(ref errorCount),
                        "The stale callback failure leaked into the replacement session.");
                    AssertEx.Equal(0L, connection.RejectedCallbackCount);
                    AssertEx.True(connection.IsConnected);
                    AssertEx.True(connection.IsCallbackListenerRunning);

                    SendCallback(connection);
                    AssertSignaled(
                        replacementCallbackReceived,
                        "replacement callback after stale handler exit");
                    AssertEx.Equal(3, Volatile.Read(ref callbackCount));
                    AssertEx.Equal(1, Volatile.Read(ref errorCount));
                    AssertEx.Equal(0L, connection.RejectedCallbackCount);
                    AssertEx.True(connection.IsConnected);
                    AssertEx.True(connection.IsCallbackListenerRunning);

                    connection.CloseConnection();
                    AssertDisconnected(connection);
                    replacementServer.Verify();
                }
                finally
                {
                    releaseOldCallback.Set();
                }
            }
        }

        private static FakeRpcStep BlockingStep(
            ushort command,
            byte[] response,
            ManualResetEventSlim received,
            ManualResetEventSlim release)
        {
            return new FakeRpcStep(command, response)
            {
                InspectRequest = request =>
                {
                    received.Set();
                    AssertEx.True(
                        release.Wait(WaitTimeoutMilliseconds),
                        "Timed out waiting to release command 0x"
                        + command.ToString("X4")
                        + ".");
                }
            };
        }

        private static FakeRpcStep BlockingDisconnectStep(
            ushort command,
            byte[] response,
            ManualResetEventSlim received,
            ManualResetEventSlim release)
        {
            var step = BlockingStep(
                command,
                response,
                received,
                release);
            step.AllowClientDisconnectAfterRequest = true;
            return step;
        }

        private static FakeRpcStep InitStep()
        {
            return new FakeRpcStep(0x8080, InitResponse());
        }

        private static FakeRpcStep CallbackStep()
        {
            return new FakeRpcStep(0x405C, SuccessAcknowledgement());
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(0x405D, SuccessAcknowledgement());
        }

        private static byte[] InitResponse()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return TestFrame.Response(0, payload);
        }

        private static byte[] SuccessAcknowledgement()
        {
            return TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00"));
        }

        private static void Initialize(LMCConnection connection, int port)
        {
            connection.RpcInitConnection(
                "127.0.0.1",
                port,
                "127.0.0.1",
                0,
                LMCConnection.DefaultEventMask);
        }

        private static void SendCallback(LMCConnection connection)
        {
            var destination = connection.CallbackLocalEndPoint;
            AssertEx.NotNull(destination);

            using (var sender = new UdpClient(AddressFamily.InterNetwork))
            {
                var payload = TestFrame.Hex("AA 55 01 02");
                sender.Send(
                    payload,
                    payload.Length,
                    destination);
            }
        }

        private static Exception CaptureException(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static void AssertLifecycleReentryRejected(
            Exception exception,
            string operation)
        {
            AssertEx.True(
                exception is InvalidOperationException,
                operation + " must throw InvalidOperationException.");
            AssertEx.Contains(
                "ConnectionStateChanged",
                exception.Message);
        }

        private static void AssertSignaled(
            ManualResetEventSlim signal,
            string operation)
        {
            AssertEx.True(
                signal.Wait(WaitTimeoutMilliseconds),
                "Timed out waiting for " + operation + ".");
        }

        private static void AssertState(
            LMCConnection connection,
            LMCConnectionState expected)
        {
            AssertEx.True(
                SpinWait.SpinUntil(
                    () => connection.State == expected,
                    WaitTimeoutMilliseconds),
                "Timed out waiting for connection state " + expected + ".");
        }

        private static void AssertTaskStarted(Task task, string operation)
        {
            AssertEx.True(
                SpinWait.SpinUntil(
                    () => task.Status != TaskStatus.WaitingForActivation
                        && task.Status != TaskStatus.WaitingToRun,
                    WaitTimeoutMilliseconds),
                "Timed out waiting for " + operation + " to start.");
        }

        private static void AssertDisconnected(LMCConnection connection)
        {
            AssertEx.Equal(LMCConnectionState.Disconnected, connection.State);
            AssertEx.False(connection.IsConnected);
            AssertEx.False(connection.IsRpcInitialized);
            AssertEx.False(connection.IsCallbackListenerRunning);
        }

        private static void AssertTransportFailure(Task<byte[]> operation)
        {
            try
            {
                operation.GetAwaiter().GetResult();
            }
            catch (Exception error)
                when (error is IOException
                    || error is SocketException
                    || error is ObjectDisposedException)
            {
                return;
            }

            throw new InvalidOperationException(
                "Expected the in-flight operation to fail after local close.");
        }
    }
}

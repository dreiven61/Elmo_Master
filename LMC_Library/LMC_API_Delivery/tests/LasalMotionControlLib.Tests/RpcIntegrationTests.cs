using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class RpcIntegrationTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add("Rpc.Success.EphemeralCallbackUdpAndClose", SuccessEphemeralCallbackUdpAndClose);
            tests.Add("Rpc.Callback.HandlerFailureReportsAndListenerContinues", CallbackHandlerFailureReportsAndListenerContinues);
            tests.Add("Rpc.Callback.ErrorHandlerFailureDoesNotStopListener", CallbackErrorHandlerFailureDoesNotStopListener);
            tests.Add(
                "Rpc.Callback.ProvenanceOwnerAndCloseInvalidation",
                CallbackProvenanceOwnerAndCloseInvalidation);
            tests.Add("Rpc.Callback.ReentrantCloseConnectionStopsListener", ReentrantCallbackCloseConnectionStopsListener);
            tests.Add("Rpc.Callback.ReentrantDisposeStopsListener", ReentrantCallbackDisposeStopsListener);
            tests.Add("Rpc.Failure.InitStatusCleansUp", InitStatusFailureCleansUp);
            tests.Add(
                "Rpc.Failure.InitShortErrorPreservedWithoutLegacyRetry",
                InitShortErrorPreservedWithoutLegacyRetry);
            tests.Add("Rpc.Failure.MalformedInitShapeCleansUp", MalformedInitShapeCleansUp);
            tests.Add("Rpc.Failure.CallbackAckCleansUp", CallbackAckFailureCleansUp);
            tests.Add("Rpc.Failure.MalformedCallbackAckCleansUp", MalformedCallbackAckCleansUp);
            tests.Add("Rpc.Failure.TruncatedResponseCleansUp", TruncatedResponseCleansUp);
            tests.Add("Rpc.Failure.OversizedInitResponseRejectedBeforeBodyRead", OversizedInitResponseRejectedBeforeBodyRead);
            tests.Add("Rpc.Failure.OversizedDiagnosticsResponseInvalidatesTransport", OversizedDiagnosticsResponseInvalidatesTransport);
            tests.Add("Rpc.ResponseLimit.MaximumRecorderChunkAllowed", MaximumRecorderChunkAllowed);
            tests.Add("Rpc.ResponseLimit.OversizedRecorderChunkInvalidatesTransport", OversizedRecorderChunkInvalidatesTransport);
            tests.Add("Rpc.Validation.ConcreteLocalIpv4Required", ConcreteLocalIpv4Required);
            tests.Add("Rpc.Validation.OptionsAreClonedAndValidated", OptionsAreClonedAndValidated);
            tests.Add("Rpc.Validation.UnknownCommandRejectedBeforeWire", UnknownCommandRejectedBeforeWire);
            tests.Add(
                "Rpc.Validation.AxisStopInvalidParametersRejectedBeforeWire",
                AxisStopInvalidParametersRejectedBeforeWire);
            tests.Add("Rpc.Callback.RejectsUnexpectedSource", RejectsUnexpectedCallbackSource);
            tests.Add("Rpc.Validation.InvalidReconnectKeepsCurrentSession", InvalidReconnectKeepsCurrentSession);
            tests.Add("Rpc.Lifecycle.CloseErrorThrowsAndCleansUp", CloseErrorThrowsAndCleansUp);
            tests.Add(
                "Rpc.Lifecycle.QualificationZeroLingerCloseOmitsRpcClose",
                QualificationAbortOmitsRpcClose);
            tests.Add("Rpc.Lifecycle.TimeoutInvalidatesTransport", TimeoutInvalidatesTransport);
            tests.Add("Rpc.Lifecycle.QueuedCancellationKeepsActiveRequest", QueuedCancellationKeepsActiveRequest);
            tests.Add("Rpc.Lifecycle.InFlightCancellationInvalidatesTransport", InFlightCancellationInvalidatesTransport);
            tests.Add("Rpc.Lifecycle.ReconnectRejectsStaleGroup", ReconnectRejectsStaleGroup);
            tests.Add(
                "Rpc.TestHarness.RequestObservationUsesStableSnapshots",
                RequestObservationUsesStableSnapshots);
            tests.Add("Rpc.Async.InitAndClose", AsyncInitAndClose);
            tests.Add("Rpc.AxisConstructor.AxisInfoSuccess", AxisConstructorAxisInfoSuccess);
            tests.Add("Rpc.AxisConstructor.MismatchedAxisInfoDescriptorRejected", AxisConstructorMismatchedAxisInfoDescriptorRejected);
            tests.Add("Rpc.AxisCreateAsync.MismatchedAxisInfoDescriptorRejected", AxisCreateAsyncMismatchedAxisInfoDescriptorRejected);
            tests.Add("Rpc.AxisConstructor.MalformedAxisInfoRejected", AxisConstructorMalformedAxisInfoRejected);
            tests.Add("Rpc.AxisConstructor.CommandErrorRejected", AxisConstructorCommandErrorRejected);
            tests.Add("Rpc.AxisConstructor.ShortAxisInfoErrorPreserved", AxisConstructorShortAxisInfoErrorPreserved);
            tests.Add("Rpc.AxisCreateAsync.ShortAxisInfoErrorPreserved", AxisCreateAsyncShortAxisInfoErrorPreserved);
            tests.Add("Rpc.AxisConstructor.ShortAxisInfoSuccessRejected", AxisConstructorShortAxisInfoSuccessRejected);
            tests.Add("Rpc.AxisCreateAsync.LookupErrorPreserved", AxisCreateAsyncLookupErrorPreserved);
            tests.Add("Rpc.AxisConstructor.LookupErrorPreserved", AxisConstructorLookupErrorPreserved);
            tests.Add("Rpc.GroupCreateAsync.LookupErrorPreserved", GroupCreateAsyncLookupErrorPreserved);
            tests.Add("Rpc.AxisReadStatus.ShortErrorPreserved", AxisReadStatusShortErrorPreserved);
            tests.Add("Rpc.Group.PositionAndKinematics", GroupPositionAndKinematics);
        }

        private static void RequestObservationUsesStableSnapshots()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                var requestsBeforeConnection = server.ReceivedRequests;
                var sessionsBeforeConnection =
                    server.ReceivedRequestSessionOrdinals;

                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Equal(0, requestsBeforeConnection.Count);
                AssertEx.Equal(0, sessionsBeforeConnection.Count);
                AssertEx.Equal(2, server.ReceivedRequests.Count);
                AssertEx.Equal(
                    server.ReceivedRequests.Count,
                    server.ReceivedRequestSessionOrdinals.Count);

                connection.CloseConnection();
                server.Verify();

                AssertEx.Equal(0, requestsBeforeConnection.Count);
                AssertEx.Equal(0, sessionsBeforeConnection.Count);
                AssertEx.Equal(3, server.ReceivedRequests.Count);
                AssertEx.Equal(
                    server.ReceivedRequests.Count,
                    server.ReceivedRequestSessionOrdinals.Count);
            }
        }

        private static void QualificationAbortOmitsRpcClose()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0, new byte[0])
                {
                    RequireClientDisconnectBeforeRequest = true
                }))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                connection.AbortTransportForQualification();

                AssertEx.True(
                    connection.LastQualificationAbortUsedZeroLinger);
                AssertEx.False(connection.IsConnected);
                AssertEx.False(connection.IsRpcInitialized);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal(
                    LMCConnectionState.Disconnected,
                    connection.State);
                AssertEx.Equal(null, connection.RpcCloseResponse);
                AssertEx.Equal(null, connection.LastCloseException);
                server.Verify();
            }
        }

        private static void SuccessEphemeralCallbackUdpAndClose()
        {
            const uint eventMask = 0xA5A55A5Au;
            var initPayload = new byte[24];
            TestFrame.WriteUInt32(initPayload, 0, 64);
            var successAck = TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00"));
            var callbackDatagram = TestFrame.Hex("DE AD BE EF 01 23 45 67");
            var registeredPort = 0;
            IPAddress registeredAddress = null;
            byte[] receivedPayload = null;
            IPEndPoint callbackRemoteEndPoint = null;
            var stateTransitions = new List<LMCConnectionState>();

            using (var callbackSignal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, TestFrame.Response(0, initPayload))
                {
                    ResponseChunks = new[] { 1, 2, 3, 5, 7 },
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Hex("80 80 00 00 01 00 00 00 00"),
                        request)
                },
                new FakeRpcStep(0x405C, successAck)
                {
                    ResponseChunks = new[] { 2, 1 },
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(20, request.Length);
                        AssertEx.Equal((ushort)12, TestFrame.ReadUInt16(request, 4));
                        AssertEx.Equal(eventMask, TestFrame.ReadUInt32(request, 8));

                        registeredPort = TestFrame.ReadInt32(request, 12);
                        AssertEx.True(
                            registeredPort > 0 && registeredPort <= ushort.MaxValue,
                            "An ephemeral callback request must register the bound UDP port.");

                        registeredAddress = new IPAddress(
                            new byte[]
                            {
                                request[16],
                                request[17],
                                request[18],
                                request[19]
                            });
                        AssertEx.Equal(IPAddress.Loopback, registeredAddress);
                    },
                    AfterResponse = request =>
                    {
                        using (var udp = new UdpClient(AddressFamily.InterNetwork))
                        {
                            udp.Send(
                                callbackDatagram,
                                callbackDatagram.Length,
                                new IPEndPoint(registeredAddress, registeredPort));
                        }
                    }
                },
                new FakeRpcStep(0x405D, successAck)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Hex("5D 40 00 00 01 00 00 00 00"),
                        request)
                }))
            using (var connection = new LMCConnection())
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    stateTransitions.Add(e.CurrentState);
                };
                connection.CallbackReceived += delegate(object sender, LMCCallbackEventArgs e)
                {
                    var mutableCopy = e.Payload;
                    mutableCopy[0] = 0;
                    receivedPayload = e.Payload;
                    var mutableRemoteEndPoint = e.RemoteEndPoint;
                    var receivedPort = mutableRemoteEndPoint.Port;
                    mutableRemoteEndPoint.Port = receivedPort == 1 ? 2 : 1;
                    callbackRemoteEndPoint = e.RemoteEndPoint;
                    AssertEx.Equal(
                        receivedPort,
                        callbackRemoteEndPoint.Port);
                    callbackSignal.Set();
                };

                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    eventMask);

                AssertEx.True(connection.IsRpcInitialized);
                AssertEx.True(connection.IsConnected);
                AssertEx.Equal(LMCConnectionState.Connected, connection.State);
                AssertEx.True(connection.IsCallbackListenerRunning);
                AssertEx.Equal(registeredPort, connection.CallbackPort);
                AssertEx.Equal(eventMask, connection.EventMask);
                AssertEx.NotNull(connection.CallbackLocalEndPoint);
                AssertEx.Equal(registeredPort, connection.CallbackLocalEndPoint.Port);
                AssertEx.NotNull(connection.RpcSessionInitResponse);
                AssertEx.True(connection.RpcSessionInitResponse.IsSuccess);
                AssertEx.Equal((ushort)24, connection.RpcSessionInitResponse.PayloadLength);
                AssertEx.NotNull(connection.RpcCallbackRegistrationResponse);
                AssertEx.True(connection.RpcCallbackRegistrationResponse.IsSuccess);
                AssertEx.True(connection.RpcCallbackRegistrationResponse.HasCommandResult);

                AssertEx.True(
                    callbackSignal.Wait(2000),
                    "The UDP callback was not received within two seconds.");
                AssertEx.SequenceEqual(callbackDatagram, receivedPayload);
                AssertEx.NotNull(callbackRemoteEndPoint);
                AssertEx.Equal(IPAddress.Loopback, callbackRemoteEndPoint.Address);

                connection.CloseConnection();

                AssertEx.False(connection.IsRpcInitialized);
                AssertEx.False(connection.IsConnected);
                AssertEx.Equal(LMCConnectionState.Disconnected, connection.State);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal(0, connection.CallbackPort);
                AssertEx.Equal(0u, connection.EventMask);
                AssertEx.Equal<IPEndPoint>(null, connection.CallbackLocalEndPoint);
                AssertEx.NotNull(connection.RpcCloseResponse);
                AssertEx.True(connection.RpcCloseResponse.IsSuccess);
                AssertEx.Equal(4, stateTransitions.Count);
                AssertEx.Equal(LMCConnectionState.Connecting, stateTransitions[0]);
                AssertEx.Equal(LMCConnectionState.Connected, stateTransitions[1]);
                AssertEx.Equal(LMCConnectionState.Closing, stateTransitions[2]);
                AssertEx.Equal(LMCConnectionState.Disconnected, stateTransitions[3]);

                server.Verify();
            }
        }

        private static void CallbackHandlerFailureReportsAndListenerContinues()
        {
            var callbackCount = 0;
            Exception reportedException = null;

            using (var errorSignal = new ManualResetEventSlim(false))
            using (var secondCallbackSignal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.CallbackReceived += delegate
                {
                    if (Interlocked.Increment(ref callbackCount) == 1)
                    {
                        throw new InvalidOperationException(
                            "Expected callback handler failure.");
                    }

                    secondCallbackSignal.Set();
                };
                connection.CallbackListenerError += delegate(
                    object sender,
                    LMCCallbackErrorEventArgs e)
                {
                    reportedException = e.Exception;
                    errorSignal.Set();
                };

                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                SendCallback(
                    connection,
                    TestFrame.Hex("01 02 03 04"));

                AssertEx.True(
                    errorSignal.Wait(2000),
                    "Callback handler failure was not reported.");
                AssertEx.NotNull(reportedException);
                AssertEx.Equal(
                    typeof(InvalidOperationException),
                    reportedException.GetType());
                AssertEx.Contains(
                    "Expected callback handler failure",
                    reportedException.Message);
                AssertEx.True(connection.IsCallbackListenerRunning);

                SendCallback(
                    connection,
                    TestFrame.Hex("05 06 07 08"));

                AssertEx.True(
                    secondCallbackSignal.Wait(2000),
                    "Callback listener did not deliver a callback after a handler failure.");
                AssertEx.Equal(2, callbackCount);
                AssertEx.True(connection.IsCallbackListenerRunning);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CallbackProvenanceOwnerAndCloseInvalidation()
        {
            LMCCallbackEventArgs callback = null;
            object callbackSender = null;

            using (var callbackSignal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var unrelatedServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            using (var unrelatedConnection = new LMCConnection())
            {
                connection.CallbackReceived += delegate(
                    object sender,
                    LMCCallbackEventArgs e)
                {
                    callbackSender = sender;
                    callback = e;
                    callbackSignal.Set();
                };

                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                unrelatedConnection.RpcInitConnection(
                    "127.0.0.1",
                    unrelatedServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var activeSessionGeneration = connection.SessionGeneration;
                AssertEx.Equal(1L, activeSessionGeneration);
                AssertEx.Equal(
                    1L,
                    unrelatedConnection.SessionGeneration);
                AssertEx.Equal(
                    activeSessionGeneration,
                    unrelatedConnection.SessionGeneration,
                    "Both owners must deliberately collide on the same numeric session generation.");
                AssertEx.True(unrelatedConnection.IsCallbackListenerRunning);

                SendCallback(
                    connection,
                    TestFrame.Hex("10 20 30 40"));
                AssertEx.True(
                    callbackSignal.Wait(2000),
                    "The callback provenance event was not received.");
                AssertEx.True(ReferenceEquals(connection, callbackSender));
                AssertEx.NotNull(callback);
                AssertEx.Equal(
                    activeSessionGeneration,
                    callback.SessionGeneration);
                AssertEx.True(callback.BelongsTo(connection));
                AssertEx.False(callback.BelongsTo(unrelatedConnection));
                AssertEx.False(callback.BelongsTo(null));
                AssertEx.True(
                    callback.BelongsToCurrentSession(connection));
                AssertEx.False(
                    callback.BelongsToCurrentSession(unrelatedConnection));

                connection.CloseConnection();

                AssertEx.True(
                    callback.BelongsTo(connection),
                    "Immutable ownership provenance must survive Close.");
                AssertEx.Equal(
                    activeSessionGeneration,
                    callback.SessionGeneration);
                AssertEx.False(
                    callback.BelongsToCurrentSession(connection),
                    "A callback captured before Close must become stale.");
                unrelatedConnection.CloseConnection();
                server.Verify();
                unrelatedServer.Verify();
            }
        }

        private static void CallbackErrorHandlerFailureDoesNotStopListener()
        {
            var callbackCount = 0;
            var errorCount = 0;

            using (var errorSignal = new ManualResetEventSlim(false))
            using (var secondCallbackSignal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.CallbackReceived += delegate
                {
                    if (Interlocked.Increment(ref callbackCount) == 1)
                    {
                        throw new InvalidOperationException(
                            "Trigger callback listener error event.");
                    }

                    secondCallbackSignal.Set();
                };
                connection.CallbackListenerError += delegate
                {
                    Interlocked.Increment(ref errorCount);
                    errorSignal.Set();
                    throw new InvalidOperationException(
                        "Expected callback error handler failure.");
                };

                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                SendCallback(
                    connection,
                    TestFrame.Hex("10 20 30 40"));

                AssertEx.True(
                    errorSignal.Wait(2000),
                    "Callback listener error handler was not invoked.");
                AssertEx.Equal(1, errorCount);
                AssertEx.True(connection.IsCallbackListenerRunning);

                SendCallback(
                    connection,
                    TestFrame.Hex("50 60 70 80"));

                AssertEx.True(
                    secondCallbackSignal.Wait(2000),
                    "Callback listener stopped after its error handler threw.");
                AssertEx.Equal(2, callbackCount);
                AssertEx.True(connection.IsCallbackListenerRunning);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void ReentrantCallbackCloseConnectionStopsListener()
        {
            RunReentrantCallbackShutdown(
                "CloseConnection",
                connection => connection.CloseConnection());
        }

        private static void ReentrantCallbackDisposeStopsListener()
        {
            RunReentrantCallbackShutdown(
                "Dispose",
                connection => connection.Dispose());
        }

        private static void RunReentrantCallbackShutdown(
            string operationName,
            Action<LMCConnection> shutdown)
        {
            var options = new LMCConnectionOptions
            {
                CallbackThreadJoinTimeoutMilliseconds = 5000
            };
            Exception shutdownException = null;

            using (var shutdownSignal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                connection.CallbackReceived += delegate
                {
                    try
                    {
                        shutdown(connection);
                    }
                    catch (Exception ex)
                    {
                        shutdownException = ex;
                    }
                    finally
                    {
                        shutdownSignal.Set();
                    }
                };

                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                SendCallback(
                    connection,
                    TestFrame.Hex("AA BB CC DD"));

                AssertEx.True(
                    shutdownSignal.Wait(2000),
                    operationName
                    + " did not return from the callback listener thread; "
                    + "a callback-thread self-join is likely.");
                AssertEx.Equal<Exception>(null, shutdownException);
                AssertEx.Equal(
                    LMCConnectionState.Disconnected,
                    connection.State);
                AssertEx.False(connection.IsConnected);
                AssertConnectionClosed(connection);

                server.Verify();
            }
        }

        private static void InitStatusFailureCleansUp()
        {
            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, TestFrame.Response(7, new byte[0]))))
            using (var connection = new LMCConnection())
            {
                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertEx.Contains("Status=7", exception.Message);
                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void InitShortErrorPreservedWithoutLegacyRetry()
        {
            using (var server = new FakeRpcServer(
                new FakeRpcStep(
                    0x8080,
                    TestFrame.Response(
                        1,
                        TestFrame.Hex("01 00 FF FF")))))
            using (var connection = new LMCConnection())
            {
                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertEx.Contains("Status=1", exception.Message);
                AssertEx.Contains("ErrorId=-1", exception.Message);
                AssertEx.Equal(1, server.AcceptedClientCount);
                AssertEx.Equal(1, server.ReceivedRequests.Count);
                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void CallbackAckFailureCleansUp()
        {
            var initPayload = new byte[24];
            TestFrame.WriteUInt32(initPayload, 0, 64);

            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, TestFrame.Response(0, initPayload)),
                new FakeRpcStep(
                    0x405C,
                    TestFrame.Response(0, TestFrame.Hex("10 00 F8 FF")))))
            using (var connection = new LMCConnection())
            {
                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertEx.Contains("Status=16", exception.Message);
                AssertEx.Contains("ErrorId=-8", exception.Message);
                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void MalformedInitShapeCleansUp()
        {
            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, TestFrame.Response(0, new byte[23]))))
            using (var connection = new LMCConnection())
            {
                AssertEx.Throws<InvalidDataException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void MalformedCallbackAckCleansUp()
        {
            var initPayload = new byte[24];
            TestFrame.WriteUInt32(initPayload, 0, 64);

            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, TestFrame.Response(0, initPayload)),
                new FakeRpcStep(0x405C, TestFrame.Response(0, new byte[8]))))
            using (var connection = new LMCConnection())
            {
                AssertEx.Throws<InvalidDataException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void TruncatedResponseCleansUp()
        {
            var truncatedResponse = new byte[10];
            TestFrame.WriteUInt16(truncatedResponse, 2, 4);
            truncatedResponse[8] = 0xAA;
            truncatedResponse[9] = 0xBB;

            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, truncatedResponse)
                {
                    CloseAfterResponse = true
                }))
            using (var connection = new LMCConnection())
            {
                AssertEx.Throws<EndOfStreamException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void OversizedInitResponseRejectedBeforeBodyRead()
        {
            var oversizedHeader = new byte[8];
            TestFrame.WriteUInt16(oversizedHeader, 2, 25);

            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, oversizedHeader)
                {
                    CloseAfterResponse = true
                }))
            using (var connection = new LMCConnection())
            {
                var exception = AssertEx.Throws<InvalidDataException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertEx.Contains("0x8080", exception.Message);
                AssertEx.Contains("maximum allowed is 24", exception.Message);
                AssertEx.True(
                    connection.LastTransportException is InvalidDataException,
                    "An oversized response must invalidate the transport as malformed input.");
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void OversizedDiagnosticsResponseInvalidatesTransport()
        {
            var oversizedHeader = new byte[8];
            TestFrame.WriteUInt16(oversizedHeader, 2, 69);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x7E00, oversizedHeader)
                {
                    CloseAfterResponse = true
                }))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidDataException>(
                    () => connection.Diagnostics.GetCapabilities());

                AssertEx.Contains("0x7E00", exception.Message);
                AssertEx.Contains("maximum allowed is 68", exception.Message);
                AssertEx.True(
                    connection.LastTransportException is InvalidDataException,
                    "An oversized response must invalidate an initialized transport.");
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void MaximumRecorderChunkAllowed()
        {
            const int maximumPayloadLength = 1972;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x7E46,
                    TestFrame.Response(0, new byte[maximumPayloadLength])),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var raw = connection.Exchange(
                    TestFrame.Request(0x7E46, 0, new byte[0]));

                AssertEx.Equal(8 + maximumPayloadLength, raw.Length);
                AssertEx.True(connection.IsConnected);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void OversizedRecorderChunkInvalidatesTransport()
        {
            var oversizedHeader = new byte[8];
            TestFrame.WriteUInt16(oversizedHeader, 2, 1973);

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x7E46, oversizedHeader)
                {
                    CloseAfterResponse = true
                }))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidDataException>(
                    () => connection.Exchange(
                        TestFrame.Request(0x7E46, 0, new byte[0])));

                AssertEx.Contains("maximum allowed is 1972", exception.Message);
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertConnectionClosed(connection);
                server.Verify();
            }
        }

        private static void ConcreteLocalIpv4Required()
        {
            using (var connection = new LMCConnection())
            {
                var exception = AssertEx.Throws<ArgumentException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        1,
                        "0.0.0.0",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertEx.Equal("localAddress", exception.ParamName);
                AssertConnectionClosed(connection);
            }
        }

        private static void OptionsAreClonedAndValidated()
        {
            var sourceOptions = new LMCConnectionOptions
            {
                ConnectTimeoutMilliseconds = 111,
                ReceiveTimeoutMilliseconds = 222,
                SendTimeoutMilliseconds = 333,
                CallbackThreadJoinTimeoutMilliseconds = 444,
                ValidateCallbackSourceAddress = false
            };

            using (var connection = new LMCConnection(sourceOptions))
            {
                sourceOptions.ReceiveTimeoutMilliseconds = 999;
                var copy = connection.Options;

                AssertEx.Equal(111, copy.ConnectTimeoutMilliseconds);
                AssertEx.Equal(222, copy.ReceiveTimeoutMilliseconds);
                AssertEx.Equal(333, copy.SendTimeoutMilliseconds);
                AssertEx.Equal(444, copy.CallbackThreadJoinTimeoutMilliseconds);
                AssertEx.False(copy.ValidateCallbackSourceAddress);

                copy.ReceiveTimeoutMilliseconds = 777;
                AssertEx.Equal(222, connection.Options.ReceiveTimeoutMilliseconds);
            }

            var invalid = new LMCConnectionOptions
            {
                ReceiveTimeoutMilliseconds = 0
            };
            var exception = AssertEx.Throws<ArgumentOutOfRangeException>(
                () => new LMCConnection(invalid));
            AssertEx.Equal("ReceiveTimeoutMilliseconds", exception.ParamName);
        }

        private static void UnknownCommandRejectedBeforeWire()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<NotSupportedException>(
                    () => connection.Exchange(
                        TestFrame.Request(0xFFFF, 0, new byte[0])));

                AssertEx.Contains("0xFFFF", exception.Message);
                AssertEx.True(connection.IsConnected);
                AssertEx.Equal<Exception>(null, connection.LastTransportException);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void AxisStopInvalidParametersRejectedBeforeWire()
        {
            RunAxisConstructorScenario(
                AxisInfoResponse(0x1234),
                connection =>
                {
                    var axis = new LMCAxis(connection, "_LMCAxis1");

                    var syncException =
                        AssertEx.Throws<ArgumentOutOfRangeException>(
                            () => axis.Stop(0, 0));
                    AssertEx.Equal(
                        "deceleration",
                        syncException.ParamName);

                    var asyncException =
                        AssertEx.Throws<ArgumentOutOfRangeException>(
                            () => axis.StopAsync(
                                    1,
                                    -1,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult());
                    AssertEx.Equal("jerk", asyncException.ParamName);

                    AssertEx.True(connection.IsConnected);
                    AssertEx.Equal<Exception>(
                        null,
                        connection.LastTransportException);
                });
        }

        private static void RejectsUnexpectedCallbackSource()
        {
            var callbackCount = 0;

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.CallbackReceived += delegate
                {
                    Interlocked.Increment(ref callbackCount);
                };

                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                using (var sender = new UdpClient(
                    new IPEndPoint(IPAddress.Parse("127.0.0.2"), 0)))
                {
                    var payload = TestFrame.Hex("AA 55 01 02");
                    sender.Send(
                        payload,
                        payload.Length,
                        connection.CallbackLocalEndPoint);
                }

                AssertEx.True(
                    SpinWait.SpinUntil(
                        () => connection.RejectedCallbackCount == 1,
                        2000),
                    "Unexpected-source callback was not rejected.");
                AssertEx.Equal(0, callbackCount);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void InvalidReconnectKeepsCurrentSession()
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        0,
                        "127.0.0.1",
                        0,
                        LMCConnection.DefaultEventMask));

                AssertEx.True(connection.IsConnected);
                AssertEx.True(connection.IsRpcInitialized);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void CloseErrorThrowsAndCleansUp()
        {
            var closeError = TestFrame.Response(
                0,
                TestFrame.Hex("10 00 F8 FF"));

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x405D, closeError)))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<IOException>(
                    connection.CloseConnection);

                AssertEx.Contains("local transport was closed", exception.Message);
                AssertEx.NotNull(connection.LastCloseException);
                AssertEx.NotNull(connection.RpcCloseResponse);
                AssertEx.Equal((ushort)16, connection.RpcCloseResponse.Status);
                AssertEx.Equal((short)-8, connection.RpcCloseResponse.ErrorId);
                AssertEx.Equal(LMCConnectionState.Disconnected, connection.State);
                AssertEx.False(connection.IsRpcInitialized);
                AssertEx.False(connection.IsCallbackListenerRunning);
                server.Verify();
            }
        }

        private static void TimeoutInvalidatesTransport()
        {
            var options = new LMCConnectionOptions
            {
                ReceiveTimeoutMilliseconds = 100
            };

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x2045,
                    TestFrame.Response(0, new byte[12]))
                {
                    ResponseDelayMilliseconds = 350,
                    AllowClientDisconnectAfterRequest = true
                }))
            using (var connection = new LMCConnection(options))
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                AssertEx.Throws<IOException>(
                    () => connection.Exchange(
                        LMC_Frame.LMCGroupReadStatus(0x0100)));

                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertEx.False(connection.IsRpcInitialized);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.NotNull(connection.LastTransportException);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Exchange(
                        LMC_Frame.LMCGroupReadStatus(0x0100)));

                server.Verify();
                connection.CloseConnection();
                AssertEx.Equal(LMCConnectionState.Disconnected, connection.State);
            }
        }

        private static void QueuedCancellationKeepsActiveRequest()
        {
            using (var requestStarted = new ManualResetEventSlim(false))
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x2045,
                    TestFrame.Response(0, new byte[12]))
                {
                    InspectRequest = request => requestStarted.Set(),
                    ResponseDelayMilliseconds = 300
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var active = connection.ExchangeAsync(
                    LMC_Frame.LMCGroupReadStatus(0x0100),
                    CancellationToken.None);
                AssertEx.True(requestStarted.Wait(2000));

                var queued = connection.ExchangeAsync(
                    LMC_Frame.LMCGroupEnable(0x0100),
                    cancellation.Token);
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => queued.GetAwaiter().GetResult());
                var activeResponse = active.GetAwaiter().GetResult();

                AssertEx.Equal(20, activeResponse.Length);
                AssertEx.True(connection.IsConnected);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void InFlightCancellationInvalidatesTransport()
        {
            var lookupPayload = new byte[6];
            TestFrame.WriteUInt16(lookupPayload, 4, 0x0100);

            using (var requestStarted = new ManualResetEventSlim(false))
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, lookupPayload))
                {
                    InspectRequest = request => requestStarted.Set(),
                    ResponseDelayMilliseconds = 300,
                    AllowClientDisconnectAfterRequest = true
                }))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var operation = LMCGroupAxis.CreateAsync(
                    connection,
                    "_LMCRobotBase1",
                    cancellation.Token);
                AssertEx.True(requestStarted.Wait(2000));
                cancellation.Cancel();

                AssertEx.Throws<OperationCanceledException>(
                    () => operation.GetAwaiter().GetResult());
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertEx.False(connection.IsRpcInitialized);
                AssertEx.False(connection.IsCallbackListenerRunning);

                server.Verify();
                connection.CloseConnection();
            }
        }

        private static void ReconnectRejectsStaleGroup()
        {
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);

            using (var firstServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    firstServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);
                var firstGeneration = connection.SessionGeneration;
                var staleGroup = new LMCGroup(connection, "_LMCRobotBase1");
                AssertEx.NotNull(staleGroup.LookupResult);
                AssertEx.Equal(
                    LMCLookupTargetKind.Group,
                    staleGroup.LookupResult.TargetKind);
                AssertEx.Equal(
                    staleGroup.GroupReference,
                    staleGroup.LookupResult.Reference);

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => staleGroup.GroupEnable());
                AssertEx.Contains("inactive RPC session", exception.Message);
                AssertEx.Throws<InvalidOperationException>(
                    () => connection.Exchange(
                        LMC_Frame.LMCGroupEnable(0x0100),
                        firstGeneration));

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void AsyncInitAndClose()
        {
            var axisLookupPayload = new byte[6];
            TestFrame.WriteUInt16(axisLookupPayload, 4, 0x1234);
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);
            var axisInfoPayload = TestFrame.Hex(
                "34 12 00 00 00 00 00 00");

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(
                    0x103C,
                    TestFrame.Response(0, axisLookupPayload))
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x103C,
                            0,
                            NamePayload("_LMCAxis9")),
                        request)
                },
                new FakeRpcStep(
                    0x202B,
                    TestFrame.Response(0, axisInfoPayload)),
                new FakeRpcStep(
                    0x1042,
                    TestFrame.Response(0, groupLookupPayload)),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnectionAsync(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask,
                    CancellationToken.None).GetAwaiter().GetResult();

                AssertEx.True(connection.IsConnected);
                var axis = LMCSingleAxis.CreateAsync(
                    connection,
                    "_LMCAxis9",
                    CancellationToken.None).GetAwaiter().GetResult();
                var group = LMCGroupAxis.CreateAsync(
                    connection,
                    "_LMCRobotBase1",
                    CancellationToken.None).GetAwaiter().GetResult();

                AssertEx.Equal("_LMCAxis9", axis.AxisName);
                AssertEx.Equal((ushort)0x1234, axis.AxisReference);
                AssertEx.NotNull(axis.LookupResult);
                AssertEx.Equal(
                    LMCLookupTargetKind.Axis,
                    axis.LookupResult.TargetKind);
                AssertEx.Equal("_LMCAxis9", axis.LookupResult.ObjectName);
                AssertEx.Equal(
                    axis.AxisReference,
                    axis.LookupResult.Reference);
                AssertEx.Equal((ushort)0x0100, group.GroupReference);
                AssertEx.NotNull(group.LookupResult);
                AssertEx.Equal(
                    LMCLookupTargetKind.Group,
                    group.LookupResult.TargetKind);
                AssertEx.Equal(
                    "_LMCRobotBase1",
                    group.LookupResult.ObjectName);
                AssertEx.Equal(
                    group.GroupReference,
                    group.LookupResult.Reference);
                connection.CloseConnectionAsync(
                    CancellationToken.None).GetAwaiter().GetResult();
                AssertEx.Equal(LMCConnectionState.Disconnected, connection.State);
                server.Verify();
            }
        }

        private static void AxisConstructorAxisInfoSuccess()
        {
            RunAxisConstructorScenario(
                AxisInfoResponse(0x1234),
                connection =>
                {
                    var axis = new LMCAxis(connection, "_LMCAxis1");

                    AssertEx.Equal("_LMCAxis1", axis.AxisName);
                    AssertEx.Equal((ushort)0x1234, axis.AxisReference);
                    AssertEx.NotNull(axis.LookupResult);
                    AssertEx.Equal(
                        LMCLookupTargetKind.Axis,
                        axis.LookupResult.TargetKind);
                    AssertEx.Equal(
                        axis.AxisReference,
                        axis.LookupResult.Reference);
                    AssertEx.True(axis.LookupResult.Response.IsFrameValid);
                    AssertEx.NotNull(axis.AxisInfoResponse);
                    AssertEx.True(axis.AxisInfoResponse.IsFrameValid);
                    AssertEx.True(axis.AxisInfoResponse.HasCommandResult);
                    AssertEx.True(axis.AxisInfoResponse.IsSuccess);
                });
        }

        private static void AxisConstructorMismatchedAxisInfoDescriptorRejected()
        {
            RunAxisConstructorScenario(
                AxisInfoResponse(0x4321),
                connection =>
                {
                    var exception = AssertEx.Throws<InvalidDataException>(
                        () => new LMCAxis(connection, "_LMCAxis1"));

                    AssertEx.Contains("0x00004321", exception.Message);
                    AssertEx.Contains("0x1234", exception.Message);
                });
        }

        private static void AxisCreateAsyncMismatchedAxisInfoDescriptorRejected()
        {
            RunAxisConstructorScenario(
                AxisInfoResponse(0x4321),
                connection =>
                {
                    var exception = AssertEx.Throws<InvalidDataException>(
                        () => LMCSingleAxis.CreateAsync(
                            connection,
                            "_LMCAxis1",
                            CancellationToken.None).GetAwaiter().GetResult());

                    AssertEx.Contains("0x00004321", exception.Message);
                    AssertEx.Contains("0x1234", exception.Message);
                });
        }

        private static void AxisConstructorMalformedAxisInfoRejected()
        {
            RunAxisConstructorScenario(
                TestFrame.Response(0, new byte[7]),
                connection => AssertEx.Throws<InvalidDataException>(
                    () => new LMCAxis(connection, "_LMCAxis1")));
        }

        private static void AxisConstructorCommandErrorRejected()
        {
            RunAxisConstructorScenario(
                AxisInfoResponse(0x1234, 16, -8),
                connection =>
                {
                    var exception = AssertEx.Throws<InvalidOperationException>(
                        () => new LMCAxis(connection, "_LMCAxis1"));

                    AssertEx.Contains("Status=16", exception.Message);
                    AssertEx.Contains("ErrorId=-8", exception.Message);
                });
        }

        private static void AxisConstructorShortAxisInfoErrorPreserved()
        {
            RunAxisConstructorScenario(
                TestFrame.Response(
                    1,
                    TestFrame.Hex("01 00 FC FF")),
                connection =>
                {
                    var exception = AssertEx.Throws<InvalidOperationException>(
                        () => new LMCAxis(connection, "_LMCAxis1"));

                    AssertEx.Contains("Status=1", exception.Message);
                    AssertEx.Contains("ErrorId=-4", exception.Message);
                });
        }

        private static void AxisCreateAsyncShortAxisInfoErrorPreserved()
        {
            RunAxisConstructorScenario(
                TestFrame.Response(
                    1,
                    TestFrame.Hex("01 00 FC FF")),
                connection =>
                {
                    var exception = AssertEx.Throws<InvalidOperationException>(
                        () => LMCSingleAxis.CreateAsync(
                            connection,
                            "_LMCAxis1",
                            CancellationToken.None).GetAwaiter().GetResult());

                    AssertEx.Contains("Status=1", exception.Message);
                    AssertEx.Contains("ErrorId=-4", exception.Message);
                });
        }

        private static void AxisConstructorShortAxisInfoSuccessRejected()
        {
            RunAxisConstructorScenario(
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")),
                connection => AssertEx.Throws<InvalidDataException>(
                    () => new LMCAxis(connection, "_LMCAxis1")));
        }

        private static void AxisCreateAsyncLookupErrorPreserved()
        {
            var exception = RunLookupFailureScenario(
                0x103C,
                "_LMCAxis1",
                connection =>
                {
                    LMCSingleAxis.CreateAsync(
                        connection,
                        "_LMCAxis1",
                        CancellationToken.None).GetAwaiter().GetResult();
                });

            AssertLookupFailure(
                exception,
                LMCLookupTargetKind.Axis,
                "_LMCAxis1");
        }

        private static void AxisConstructorLookupErrorPreserved()
        {
            var exception = RunLookupFailureScenario(
                0x103C,
                "_LMCAxis1",
                connection =>
                {
                    new LMCAxis(connection, "_LMCAxis1");
                });

            AssertLookupFailure(
                exception,
                LMCLookupTargetKind.Axis,
                "_LMCAxis1");
        }

        private static void GroupCreateAsyncLookupErrorPreserved()
        {
            var exception = RunLookupFailureScenario(
                0x1042,
                "_LMCRobotBase1",
                connection =>
                {
                    LMCGroupAxis.CreateAsync(
                        connection,
                        "_LMCRobotBase1",
                        CancellationToken.None).GetAwaiter().GetResult();
                });

            AssertLookupFailure(
                exception,
                LMCLookupTargetKind.Group,
                "_LMCRobotBase1");
        }

        private static LMCLookupException RunLookupFailureScenario(
            ushort command,
            string objectName,
            Action<LMCConnection> create)
        {
            var lookupError = TestFrame.Response(
                1,
                TestFrame.Hex("01 00 FE FF"));

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(command, lookupError)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(
                            command,
                            0,
                            NamePayload(objectName)),
                        request)
                },
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<LMCLookupException>(
                    () => create(connection));

                connection.CloseConnection();
                server.Verify();
                return exception;
            }
        }

        private static void AssertLookupFailure(
            LMCLookupException exception,
            LMCLookupTargetKind targetKind,
            string objectName)
        {
            AssertEx.Contains(
                targetKind + " lookup failed for '" + objectName + "'",
                exception.Message);
            AssertEx.Contains("HeaderStatus=1", exception.Message);
            AssertEx.Contains("CommandStatus=1", exception.Message);
            AssertEx.Contains("ErrorId=-2", exception.Message);
            AssertEx.Equal(targetKind, exception.TargetKind);
            AssertEx.Equal(objectName, exception.ObjectName);
            AssertEx.False(exception.HasLookupPayload);
            AssertEx.Equal((ushort)0, exception.LookupReference);
            AssertEx.Equal((ushort)1, exception.Response.HeaderStatus);
            AssertEx.True(exception.Response.HasCommandResult);
            AssertEx.Equal((ushort)1, exception.Response.CommandStatus);
            AssertEx.Equal((short)-2, exception.Response.ErrorId);
            AssertEx.Contains(
                "Raw=01 00 04 00 00 00 00 00 01 00 FE FF",
                exception.Message);

            var rawCopy = exception.RawResponse;
            rawCopy[0] = 0xFF;
            AssertEx.Equal((byte)0x01, exception.RawResponse[0]);
        }

        private static void AxisReadStatusShortErrorPreserved()
        {
            var initPayload = new byte[24];
            TestFrame.WriteUInt32(initPayload, 0, 64);
            var successAck = TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00"));
            var lookupPayload = new byte[6];
            TestFrame.WriteUInt16(lookupPayload, 4, 0x1234);
            var axisInfoPayload = TestFrame.Hex("34 12 00 00 00 00 00 00");
            var shortError = TestFrame.Response(
                1,
                TestFrame.Hex("10 00 FD FF"));

            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, TestFrame.Response(0, initPayload)),
                new FakeRpcStep(0x405C, successAck),
                new FakeRpcStep(0x103C, TestFrame.Response(0, lookupPayload)),
                new FakeRpcStep(0x202B, TestFrame.Response(0, axisInfoPayload)),
                new FakeRpcStep(0x2028, shortError),
                new FakeRpcStep(0x405D, successAck)))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var axis = new LMCAxis(connection, "_LMCAxis1");
                var result = axis.ReadStatusResult();

                AssertEx.False(result.IsSuccess);
                AssertEx.Equal((short)-3, result.ErrorId);
                AssertEx.Equal((ushort)1, result.Response.HeaderStatus);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void GroupPositionAndKinematics()
        {
            var initPayload = new byte[24];
            TestFrame.WriteUInt32(initPayload, 0, 64);
            var successAck = TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00"));
            var longSuccessAck = TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00 00 00 00 00"));
            var groupLookupPayload = new byte[6];
            TestFrame.WriteUInt16(groupLookupPayload, 4, 0x0100);
            var groupPositionPayload = new byte[68];
            var groupStopPayload = new byte[16];
            TestFrame.WriteInt32(groupStopPayload, 0, 1000);
            TestFrame.WriteInt32(groupStopPayload, 4, 0);
            TestFrame.WriteInt32(groupStopPayload, 8, 1);
            TestFrame.WriteInt32(groupStopPayload, 12, 1);

            for (var index = 0; index < 9; index++)
            {
                TestFrame.WriteInt32(groupPositionPayload, index * 4, index - 4);
            }

            var steps = new List<FakeRpcStep>
            {
                new FakeRpcStep(0x8080, TestFrame.Response(0, initPayload)),
                new FakeRpcStep(0x405C, successAck),
                new FakeRpcStep(0x1042, TestFrame.Response(0, groupLookupPayload)),
                new FakeRpcStep(0x2051, TestFrame.Response(0, groupPositionPayload))
                {
                    ResponseChunks = new[] { 1, 3, 5, 7, 11 },
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x2051,
                            0x0100,
                            new byte[] { 1, 0, 0, 0, 1, 0, 0, 0 }),
                        request)
                }
            };

            for (var index = 0; index < 4; index++)
            {
                var reference = (ushort)(index + 1);
                var lookupPayload = new byte[6];
                TestFrame.WriteUInt16(lookupPayload, 4, reference);

                steps.Add(new FakeRpcStep(0x103C, TestFrame.Response(0, lookupPayload)));
                steps.Add(new FakeRpcStep(0x202B, AxisInfoResponse(reference)));
            }

            steps.Add(
                new FakeRpcStep(0x204A, longSuccessAck)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(0x204A, 0x0100, new byte[] { 1 }),
                        request)
                });
            steps.Add(
                new FakeRpcStep(0x20E7, successAck)
                {
                    ResponseChunks = new[] { 2, 2, 1 },
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(1328, request.Length);
                        AssertEx.Equal((ushort)1320, TestFrame.ReadUInt16(request, 4));
                        AssertEx.Equal((ushort)0x0100, TestFrame.ReadUInt16(request, 6));

                        for (var index = 0; index < 4; index++)
                        {
                            AssertEx.Equal(
                                (uint)(index + 1),
                                TestFrame.ReadUInt32(request, 8 + index * 40 + 28));
                        }

                        AssertEx.Equal(4, TestFrame.ReadInt32(request, 8 + 640));
                        AssertEx.Equal(2, TestFrame.ReadInt32(request, 8 + 1308));
                        AssertEx.Equal((byte)1, request[8 + 1312]);
                    }
                });
            steps.Add(
                new FakeRpcStep(0x2047, longSuccessAck)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(0x2047, 0x0100, new byte[] { 1 }),
                        request)
                });
            steps.Add(
                new FakeRpcStep(0x2049, longSuccessAck)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(0x2049, 0x0100, new byte[] { 1 }),
                        request)
                });
            steps.Add(
                new FakeRpcStep(0x2085, longSuccessAck)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x2085,
                            0x0100,
                            groupStopPayload),
                        request)
                });
            steps.Add(
                new FakeRpcStep(0x20A4, longSuccessAck)
                {
                    InspectRequest = request =>
                    {
                        AssertEx.Equal(104, request.Length);
                        AssertEx.Equal(100, TestFrame.ReadInt32(request, 8));
                        AssertEx.Equal(-200, TestFrame.ReadInt32(request, 12));
                        AssertEx.Equal(300, TestFrame.ReadInt32(request, 16));
                        AssertEx.Equal(400, TestFrame.ReadInt32(request, 20));
                        AssertEx.Equal(0, TestFrame.ReadInt32(request, 8 + 80));
                        AssertEx.Equal(0, TestFrame.ReadInt32(request, 8 + 84));
                        AssertEx.Equal(1, TestFrame.ReadInt32(request, 8 + 88));
                        AssertEx.Equal(1, TestFrame.ReadInt32(request, 8 + 92));
                    }
                });
            steps.Add(
                new FakeRpcStep(0x2048, longSuccessAck)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(0x2048, 0x0100, new byte[] { 1 }),
                        request)
                });
            steps.Add(
                new FakeRpcStep(0x204B, longSuccessAck)
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(0x204B, 0x0100, new byte[] { 1 }),
                        request)
                });
            steps.Add(new FakeRpcStep(0x405D, successAck));

            using (var server = new FakeRpcServer(steps.ToArray()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var group = new LMCGroup(connection, "_LMCRobotBase1");

                AssertEx.Throws<NotSupportedException>(
                    () => group.GroupReadActualPosition(LMC_COORD_SYSTEM.Mcs));
                AssertEx.Throws<NotSupportedException>(
                    () => group.MoveLinearAbsoluteEx(
                        new[] { 1, 2, 3, 4 },
                        1,
                        1,
                        1,
                        0,
                        new LMCGroupMotionOptions
                        {
                            CoordinateSystem = LMC_COORD_SYSTEM.Acs
                        }));
                AssertEx.Throws<ArgumentException>(
                    () => group.MoveLinearAbsoluteEx(
                        new[] { 1, 2, 3, 4, 5 },
                        1,
                        1,
                        1,
                        0));

                var position = group.GroupReadActualPosition(LMC_COORD_SYSTEM.Acs);

                AssertEx.True(position.IsSuccess);
                AssertEx.Equal(LMC_COORD_SYSTEM.Acs, position.CoordinateSystem);
                AssertEx.Equal(-4, position.PositionsRaw[0]);
                AssertEx.Equal(4, position.PositionsRaw[8]);
                AssertEx.Equal(0, position.PositionsRaw[15]);

                var axes = new[]
                {
                    new LMCAxis(connection, "_LMCAxis1"),
                    new LMCAxis(connection, "_LMCAxis2"),
                    new LMCAxis(connection, "_LMCAxis3"),
                    new LMCAxis(connection, "_LMCAxis4")
                };

                AssertEx.True(
                    group.GroupPowerOnAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                        .IsSuccess);

                var response = group.SetKinTransformCartesian4Axis(
                    axes[0],
                    axes[1],
                    axes[2],
                    axes[3]);

                AssertEx.True(response.IsSuccess);

                AssertEx.True(group.GroupEnable().IsSuccess);
                AssertEx.True(group.GroupReset().IsSuccess);
                AssertEx.True(group.GroupStop(1000, 0).IsSuccess);
                AssertEx.True(
                    group.MoveLinearAbsoluteEx(
                        new[] { 100, -200, 300, 400 },
                        1000,
                        2000,
                        2000,
                        0,
                        new LMCGroupMotionOptions
                        {
                            CoordinateSystem = LMC_COORD_SYSTEM.None,
                            TransitionMode = LMC_GROUP_TRANSITION_MODE.ExactStop,
                            BufferMode = LMC_BUFFER_MODE.Aborting,
                            Execute = true
                        }).IsSuccess);
                AssertEx.True(
                    group.GroupDisableAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                        .IsSuccess);
                AssertEx.True(
                    group.GroupPowerOffAsync(CancellationToken.None)
                        .GetAwaiter()
                        .GetResult()
                        .IsSuccess);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void RunAxisConstructorScenario(
            byte[] axisInfoResponse,
            Action<LMCConnection> assertion)
        {
            var initPayload = new byte[24];
            TestFrame.WriteUInt32(initPayload, 0, 64);
            var successAck = TestFrame.Response(
                0,
                TestFrame.Hex("00 00 00 00"));
            var lookupPayload = new byte[6];
            TestFrame.WriteUInt16(lookupPayload, 4, 0x1234);

            using (var server = new FakeRpcServer(
                new FakeRpcStep(0x8080, TestFrame.Response(0, initPayload)),
                new FakeRpcStep(0x405C, successAck),
                new FakeRpcStep(0x103C, TestFrame.Response(0, lookupPayload))
                {
                    InspectRequest = request => AssertEx.SequenceEqual(
                        TestFrame.Request(
                            0x103C,
                            0,
                            NamePayload("_LMCAxis1")),
                        request)
                },
                new FakeRpcStep(0x202B, axisInfoResponse)
                {
                    InspectRequest = request => AssertEx.Equal(
                        (ushort)0x1234,
                        TestFrame.ReadUInt16(request, 6))
                },
                new FakeRpcStep(0x405D, successAck)))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                assertion(connection);
                connection.CloseConnection();
                server.Verify();
            }
        }

        private static byte[] AxisInfoResponse(
            ushort axisReference,
            ushort commandStatus = 0,
            short errorId = 0)
        {
            var payload = new byte[8];
            TestFrame.WriteUInt32(payload, 0, axisReference);
            TestFrame.WriteUInt16(payload, 4, commandStatus);
            TestFrame.WriteInt16(payload, 6, errorId);
            return TestFrame.Response(0, payload);
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
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
        }

        private static void SendCallback(
            LMCConnection connection,
            byte[] payload)
        {
            var destination = connection.CallbackLocalEndPoint;
            AssertEx.NotNull(destination);

            using (var sender = new UdpClient(AddressFamily.InterNetwork))
            {
                sender.Send(
                    payload,
                    payload.Length,
                    destination);
            }
        }

        private static byte[] NamePayload(string value)
        {
            var payload = new byte[80];
            var bytes = System.Text.Encoding.ASCII.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, payload, 0, bytes.Length);
            return payload;
        }

        private static void AssertConnectionClosed(LMCConnection connection)
        {
            AssertEx.False(connection.IsRpcInitialized);
            AssertEx.False(connection.IsCallbackListenerRunning);
            AssertEx.Equal(0, connection.CallbackPort);
            AssertEx.Equal(0u, connection.EventMask);
            AssertEx.Equal<IPEndPoint>(null, connection.CallbackLocalEndPoint);
            AssertEx.Equal<LMC_Response>(null, connection.RpcSessionInitResponse);
            AssertEx.Equal<LMC_Response>(null, connection.RpcCallbackRegistrationResponse);
        }
    }
}

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
            tests.Add("Rpc.Failure.InitStatusCleansUp", InitStatusFailureCleansUp);
            tests.Add("Rpc.Failure.MalformedInitShapeCleansUp", MalformedInitShapeCleansUp);
            tests.Add("Rpc.Failure.CallbackAckCleansUp", CallbackAckFailureCleansUp);
            tests.Add("Rpc.Failure.MalformedCallbackAckCleansUp", MalformedCallbackAckCleansUp);
            tests.Add("Rpc.Failure.TruncatedResponseCleansUp", TruncatedResponseCleansUp);
            tests.Add("Rpc.Validation.ConcreteLocalIpv4Required", ConcreteLocalIpv4Required);
            tests.Add("Rpc.Validation.OptionsAreClonedAndValidated", OptionsAreClonedAndValidated);
            tests.Add("Rpc.Callback.RejectsUnexpectedSource", RejectsUnexpectedCallbackSource);
            tests.Add("Rpc.Validation.InvalidReconnectKeepsCurrentSession", InvalidReconnectKeepsCurrentSession);
            tests.Add("Rpc.Lifecycle.CloseErrorThrowsAndCleansUp", CloseErrorThrowsAndCleansUp);
            tests.Add("Rpc.Lifecycle.TimeoutInvalidatesTransport", TimeoutInvalidatesTransport);
            tests.Add("Rpc.Lifecycle.QueuedCancellationKeepsActiveRequest", QueuedCancellationKeepsActiveRequest);
            tests.Add("Rpc.Lifecycle.InFlightCancellationInvalidatesTransport", InFlightCancellationInvalidatesTransport);
            tests.Add("Rpc.Lifecycle.ReconnectRejectsStaleGroup", ReconnectRejectsStaleGroup);
            tests.Add("Rpc.Async.InitAndClose", AsyncInitAndClose);
            tests.Add("Rpc.AxisConstructor.AxisInfoSuccess", AxisConstructorAxisInfoSuccess);
            tests.Add("Rpc.AxisConstructor.MalformedAxisInfoRejected", AxisConstructorMalformedAxisInfoRejected);
            tests.Add("Rpc.AxisConstructor.CommandErrorRejected", AxisConstructorCommandErrorRejected);
            tests.Add("Rpc.AxisConstructor.ShortAxisInfoErrorPreserved", AxisConstructorShortAxisInfoErrorPreserved);
            tests.Add("Rpc.AxisCreateAsync.ShortAxisInfoErrorPreserved", AxisCreateAsyncShortAxisInfoErrorPreserved);
            tests.Add("Rpc.AxisConstructor.ShortAxisInfoSuccessRejected", AxisConstructorShortAxisInfoSuccessRejected);
            tests.Add("Rpc.AxisConstructor.LookupErrorPreserved", AxisConstructorLookupErrorPreserved);
            tests.Add("Rpc.AxisReadStatus.ShortErrorPreserved", AxisReadStatusShortErrorPreserved);
            tests.Add("Rpc.Group.PositionAndKinematics", GroupPositionAndKinematics);
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
                    callbackRemoteEndPoint = e.RemoteEndPoint;
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
                AssertEx.Equal((ushort)0x0100, group.GroupReference);
                connection.CloseConnectionAsync(
                    CancellationToken.None).GetAwaiter().GetResult();
                AssertEx.Equal(LMCConnectionState.Disconnected, connection.State);
                server.Verify();
            }
        }

        private static void AxisConstructorAxisInfoSuccess()
        {
            RunAxisConstructorScenario(
                TestFrame.Response(
                    0,
                    TestFrame.Hex("44 33 22 11 00 00 00 00")),
                connection =>
                {
                    var axis = new LMCAxis(connection, "_LMCAxis1");

                    AssertEx.Equal("_LMCAxis1", axis.AxisName);
                    AssertEx.Equal((ushort)0x1234, axis.AxisReference);
                    AssertEx.NotNull(axis.AxisInfoResponse);
                    AssertEx.True(axis.AxisInfoResponse.IsFrameValid);
                    AssertEx.True(axis.AxisInfoResponse.HasCommandResult);
                    AssertEx.True(axis.AxisInfoResponse.IsSuccess);
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
                TestFrame.Response(
                    0,
                    TestFrame.Hex("44 33 22 11 10 00 F8 FF")),
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

        private static void AxisConstructorLookupErrorPreserved()
        {
            var lookupError = TestFrame.Response(
                1,
                TestFrame.Hex("01 00 FE FF"));

            using (var server = new FakeRpcServer(
                InitStep(),
                CallbackStep(),
                new FakeRpcStep(0x103C, lookupError),
                CloseStep()))
            using (var connection = new LMCConnection())
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    LMCConnection.DefaultEventMask);

                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => LMCSingleAxis.CreateAsync(
                        connection,
                        "_LMCAxis1",
                        CancellationToken.None).GetAwaiter().GetResult());

                AssertEx.Contains("Axis lookup failed for '_LMCAxis1'", exception.Message);
                AssertEx.Contains("HeaderStatus=1", exception.Message);
                AssertEx.Contains("CommandStatus=1", exception.Message);
                AssertEx.Contains("ErrorId=-2", exception.Message);
                AssertEx.Contains(
                    "Raw=01 00 04 00 00 00 00 00 01 00 FE FF",
                    exception.Message);

                connection.CloseConnection();
                server.Verify();
            }
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

            for (var index = 0; index < 16; index++)
            {
                TestFrame.WriteInt32(groupPositionPayload, index * 4, index - 8);
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
                            new byte[] { 2, 0, 0, 0, 1, 0, 0, 0 }),
                        request)
                }
            };

            for (var index = 0; index < 4; index++)
            {
                var reference = (ushort)(index + 1);
                var lookupPayload = new byte[6];
                TestFrame.WriteUInt16(lookupPayload, 4, reference);

                steps.Add(new FakeRpcStep(0x103C, TestFrame.Response(0, lookupPayload)));
                steps.Add(new FakeRpcStep(0x202B, longSuccessAck));
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
                var position = group.GroupReadActualPosition(LMC_COORD_SYSTEM.Mcs);

                AssertEx.True(position.IsSuccess);
                AssertEx.Equal(-8, position.PositionsRaw[0]);
                AssertEx.Equal(7, position.PositionsRaw[15]);

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

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
            tests.Add("Rpc.AxisConstructor.AxisInfoSuccess", AxisConstructorAxisInfoSuccess);
            tests.Add("Rpc.AxisConstructor.MalformedAxisInfoRejected", AxisConstructorMalformedAxisInfoRejected);
            tests.Add("Rpc.AxisConstructor.CommandErrorRejected", AxisConstructorCommandErrorRejected);
            tests.Add("Rpc.AxisReadStatus.ShortErrorPreserved", AxisReadStatusShortErrorPreserved);
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
                connection.CallbackReceived += delegate(object sender, LMCCallbackEventArgs e)
                {
                    receivedPayload = (byte[])e.Payload.Clone();
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
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal(0, connection.CallbackPort);
                AssertEx.Equal(0u, connection.EventMask);
                AssertEx.Equal<IPEndPoint>(null, connection.CallbackLocalEndPoint);
                AssertEx.NotNull(connection.RpcCloseResponse);
                AssertEx.True(connection.RpcCloseResponse.IsSuccess);

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

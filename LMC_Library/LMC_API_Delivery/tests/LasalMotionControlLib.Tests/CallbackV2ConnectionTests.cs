using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlLib.Tests
{
    internal static class CallbackV2ConnectionTests
    {
        private const ulong FirstCookie = 0x11223344AABBCCDDUL;
        private const ulong SecondCookie = 0x5566778899AABBCCUL;

        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Rpc.CallbackV2.OptionsCloneAndPreWireValidation",
                OptionsCloneAndPreWireValidation);
            tests.Add(
                "Rpc.CallbackV2.SessionInitTransientFailureRetriesSameSocket",
                SessionInitTransientFailureRetriesSameSocket);
            tests.Add(
                "Rpc.CallbackV2.SessionInitPersistentFailureStopsAfterOneRetry",
                SessionInitPersistentFailureStopsAfterOneRetry);
            tests.Add(
                "Rpc.CallbackV2.SessionInitDifferentFailureDoesNotRetry",
                SessionInitDifferentFailureDoesNotRetry);
            tests.Add(
                "Rpc.CallbackV2.SessionInitReservedFailureDoesNotRetry",
                SessionInitReservedFailureDoesNotRetry);
            tests.Add(
                "Rpc.CallbackV2.SessionInitRetryCancellationStopsBeforeSecondRequest",
                SessionInitRetryCancellationStopsBeforeSecondRequest);
            tests.Add(
                "Rpc.CallbackV2.NegotiationEarlyTypedDispatch",
                NegotiationEarlyTypedDispatch);
            tests.Add(
                "Rpc.CallbackV2.D5TerminalTicketCorrelation",
                D5TerminalTicketCorrelation);
            tests.Add(
                "Rpc.CallbackV2.StrictFenceAndBoundedReceive",
                StrictFenceAndBoundedReceive);
            tests.Add(
                "Rpc.CallbackV2.HandlerFailureContinues",
                HandlerFailureContinues);
            tests.Add(
                "Rpc.CallbackV2.GateOwnershipSurvivesJoinTimeout",
                GateOwnershipSurvivesJoinTimeout);
            tests.Add(
                "Rpc.CallbackV2.ReentrantCloseInvalidatesTypedEvent",
                ReentrantCloseInvalidatesTypedEvent);
            tests.Add(
                "Rpc.CallbackV2.SafetyAbortDetachRejectsTypedEvent",
                SafetyAbortDetachRejectsTypedEvent);
            tests.Add(
                "Rpc.CallbackV2.InvalidResponseNoDowngrade",
                InvalidResponseNoDowngrade);
            tests.Add(
                "Rpc.CallbackV2.ReconnectRejectsOldSession",
                ReconnectRejectsOldSession);
        }

        private static void OptionsCloneAndPreWireValidation()
        {
            var defaults = new LMCConnectionOptions();
            AssertEx.Equal(
                LMCCallbackRegistrationMode.LegacyRaw,
                defaults.CallbackRegistrationMode);
            AssertEx.Equal(
                (ushort)512,
                defaults.CallbackRequestedMaxDatagramBytes);

            Func<ulong> factory = () => FirstCookie;
            var source = new LMCConnectionOptions
            {
                CallbackRegistrationMode =
                    LMCCallbackRegistrationMode.Version2WakeHint,
                CallbackRequestedMaxDatagramBytes = 256,
                ValidateCallbackSourceAddress = false,
                CallbackCookieFactory = factory
            };
            using (var cloned = new LMCConnection(source))
            {
                source.CallbackRegistrationMode =
                    LMCCallbackRegistrationMode.LegacyRaw;
                source.CallbackRequestedMaxDatagramBytes = 52;
                source.CallbackCookieFactory = null;
                var copy = cloned.Options;
                AssertEx.Equal(
                    LMCCallbackRegistrationMode.Version2WakeHint,
                    copy.CallbackRegistrationMode);
                AssertEx.Equal(
                    (ushort)256,
                    copy.CallbackRequestedMaxDatagramBytes);
                AssertEx.False(copy.ValidateCallbackSourceAddress);
                AssertEx.Equal(factory, copy.CallbackCookieFactory);

                copy.CallbackRequestedMaxDatagramBytes = 52;
                AssertEx.Equal(
                    (ushort)256,
                    cloned.Options.CallbackRequestedMaxDatagramBytes);
            }

            var invalidMode = new LMCConnectionOptions
            {
                CallbackRegistrationMode =
                    (LMCCallbackRegistrationMode)123
            };
            AssertEx.Equal(
                "CallbackRegistrationMode",
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new LMCConnection(invalidMode)).ParamName);
            var invalidMaximum = new LMCConnectionOptions
            {
                CallbackRequestedMaxDatagramBytes = 51
            };
            AssertEx.Equal(
                "CallbackRequestedMaxDatagramBytes",
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new LMCConnection(invalidMaximum)).ParamName);
            invalidMaximum.CallbackRequestedMaxDatagramBytes = 513;
            AssertEx.Equal(
                "CallbackRequestedMaxDatagramBytes",
                AssertEx.Throws<ArgumentOutOfRangeException>(
                    () => new LMCConnection(invalidMaximum)).ParamName);

            var cookieCalls = 0;
            var options = V2Options(
                () => Interlocked.Increment(ref cookieCalls) == 1
                    ? FirstCookie
                    : 0UL);
            using (var server = new FakeRpcServer(
                InitStep(),
                V2Step(0x101u, 0x201u),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    1u);
                var generation = connection.SessionGeneration;
                var endpoint = connection.CallbackLocalEndPoint;

                var maskError = AssertEx.Throws<ArgumentException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        2u));
                AssertEx.Equal("eventMask", maskError.ParamName);
                AssertEx.Equal(1, cookieCalls);
                AssertEx.True(connection.IsConnected);
                AssertEx.Equal(generation, connection.SessionGeneration);
                AssertEx.Equal(endpoint, connection.CallbackLocalEndPoint);

                AssertEx.Throws<InvalidOperationException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        1u));
                AssertEx.Equal(2, cookieCalls);
                AssertEx.True(connection.IsConnected);
                AssertEx.Equal(generation, connection.SessionGeneration);
                AssertEx.Equal(endpoint, connection.CallbackLocalEndPoint);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void SessionInitTransientFailureRetriesSameSocket()
        {
            using (var server = new FakeRpcServer(
                InitShortFailureStep(-1),
                InitStep(),
                V2Step(0x111u, 0x222u),
                CloseStep()))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    1u);

                AssertEx.True(connection.IsConnected);
                AssertEx.Equal(1, server.AcceptedClientCount);
                AssertEx.Equal(3, server.ReceivedRequests.Count);
                AssertEx.Equal(
                    (ushort)0x8080,
                    TestFrame.ReadUInt16(server.ReceivedRequests[0], 0));
                AssertEx.Equal(
                    (ushort)0x8080,
                    TestFrame.ReadUInt16(server.ReceivedRequests[1], 0));
                AssertEx.Equal(
                    (ushort)0x405C,
                    TestFrame.ReadUInt16(server.ReceivedRequests[2], 0));
                AssertEx.Equal(1, server.ReceivedRequestSessionOrdinals[0]);
                AssertEx.Equal(1, server.ReceivedRequestSessionOrdinals[1]);
                AssertEx.Equal(1, server.ReceivedRequestSessionOrdinals[2]);

                var evidence = connection
                    .LastRpcSessionInitializationEvidence;
                AssertEx.NotNull(evidence);
                AssertEx.Equal(
                    LMCRpcSessionInitializationOutcome.Succeeded,
                    evidence.Outcome);
                AssertEx.Equal(2, evidence.AttemptCount);
                AssertEx.True(evidence.CanonicalRetryUsed);
                AssertEx.Equal(
                    connection.CurrentSessionGeneration,
                    evidence.SessionGeneration);
                AssertEx.NotNull(evidence.FirstFailureResponse);
                AssertEx.Equal((short)-1, evidence.FirstFailureResponse.ErrorId);
                AssertEx.NotNull(evidence.LastReceivedResponse);
                AssertEx.True(evidence.LastReceivedResponse.IsSuccess);
                AssertEx.Equal<string>(null, evidence.FailureType);
                AssertEx.Equal<string>(null, evidence.FailureMessage);

                connection.CloseConnection();
                AssertEx.True(ReferenceEquals(
                    evidence,
                    connection.LastRpcSessionInitializationEvidence));
                server.Verify();
            }
        }

        private static void SessionInitPersistentFailureStopsAfterOneRetry()
        {
            using (var server = new FakeRpcServer(
                InitShortFailureStep(-1),
                InitShortFailureStep(-1)))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        1u));

                AssertEx.Contains("Status=1", exception.Message);
                AssertEx.Contains("ErrorId=-1", exception.Message);
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertEx.False(connection.IsConnected);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal(1, server.AcceptedClientCount);
                AssertEx.Equal(2, server.ReceivedRequests.Count);
                AssertEx.Equal(1, server.ReceivedRequestSessionOrdinals[0]);
                AssertEx.Equal(1, server.ReceivedRequestSessionOrdinals[1]);
                var evidence = connection
                    .LastRpcSessionInitializationEvidence;
                AssertEx.NotNull(evidence);
                AssertEx.Equal(
                    LMCRpcSessionInitializationOutcome.Failed,
                    evidence.Outcome);
                AssertEx.Equal(2, evidence.AttemptCount);
                AssertEx.True(evidence.CanonicalRetryUsed);
                AssertEx.NotNull(evidence.FirstFailureResponse);
                AssertEx.Equal((short)-1, evidence.FirstFailureResponse.ErrorId);
                AssertEx.NotNull(evidence.LastReceivedResponse);
                AssertEx.Equal((short)-1, evidence.LastReceivedResponse.ErrorId);
                AssertEx.Equal(
                    typeof(InvalidOperationException).FullName,
                    evidence.FailureType);
                AssertEx.Contains("Status=1", evidence.FailureMessage);
                AssertEx.Contains("ErrorId=-1", evidence.FailureMessage);
                server.Verify();
            }
        }

        private static void SessionInitDifferentFailureDoesNotRetry()
        {
            using (var server = new FakeRpcServer(
                InitShortFailureStep(-4)))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        1u));

                AssertEx.Contains("Status=1", exception.Message);
                AssertEx.Contains("ErrorId=-4", exception.Message);
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertEx.False(connection.IsConnected);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal(1, server.AcceptedClientCount);
                AssertEx.Equal(1, server.ReceivedRequests.Count);
                var evidence = connection
                    .LastRpcSessionInitializationEvidence;
                AssertEx.NotNull(evidence);
                AssertEx.Equal(
                    LMCRpcSessionInitializationOutcome.Failed,
                    evidence.Outcome);
                AssertEx.Equal(1, evidence.AttemptCount);
                AssertEx.False(evidence.CanonicalRetryUsed);
                AssertEx.Equal<LMC_Response>(
                    null,
                    evidence.FirstFailureResponse);
                AssertEx.NotNull(evidence.LastReceivedResponse);
                AssertEx.Equal((short)-4, evidence.LastReceivedResponse.ErrorId);
                server.Verify();
            }
        }

        private static void SessionInitReservedFailureDoesNotRetry()
        {
            using (var server = new FakeRpcServer(
                new FakeRpcStep(
                    0x8080,
                    TestFrame.Response(
                        1,
                        TestFrame.Hex("01 00 FF FF"),
                        1))))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                var exception = AssertEx.Throws<InvalidOperationException>(
                    () => connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        1u));

                AssertEx.Contains("Status=1", exception.Message);
                AssertEx.Contains("ErrorId=-1", exception.Message);
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertEx.False(connection.IsConnected);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal(1, server.AcceptedClientCount);
                AssertEx.Equal(1, server.ReceivedRequests.Count);
                var evidence = connection
                    .LastRpcSessionInitializationEvidence;
                AssertEx.NotNull(evidence);
                AssertEx.Equal(
                    LMCRpcSessionInitializationOutcome.Failed,
                    evidence.Outcome);
                AssertEx.Equal(1, evidence.AttemptCount);
                AssertEx.False(evidence.CanonicalRetryUsed);
                AssertEx.Equal<LMC_Response>(
                    null,
                    evidence.FirstFailureResponse);
                AssertEx.NotNull(evidence.LastReceivedResponse);
                AssertEx.Equal((uint)1, evidence.LastReceivedResponse.HeaderReserved);
                AssertEx.Equal((short)-1, evidence.LastReceivedResponse.ErrorId);
                server.Verify();
            }
        }

        private static void
            SessionInitRetryCancellationStopsBeforeSecondRequest()
        {
            using (var cancellation = new CancellationTokenSource())
            using (var server = new FakeRpcServer(
                InitShortFailureStep(-1)))
            {
                var options = V2Options(() => FirstCookie);
                options.RpcSessionInitRetryScheduledObserver =
                    cancellation.Cancel;
                using (var connection = new LMCConnection(options))
                {
                    AssertEx.Throws<OperationCanceledException>(
                        () => connection.RpcInitConnectionAsync(
                                "127.0.0.1",
                                server.Port,
                                "127.0.0.1",
                                0,
                                1u,
                                cancellation.Token)
                            .GetAwaiter()
                            .GetResult());

                    AssertEx.Equal(
                        LMCConnectionState.Disconnected,
                        connection.State);
                    AssertEx.False(connection.IsConnected);
                    AssertEx.False(connection.IsCallbackListenerRunning);
                    AssertEx.Equal(1, server.AcceptedClientCount);
                    AssertEx.Equal(1, server.ReceivedRequests.Count);
                    var evidence = connection
                        .LastRpcSessionInitializationEvidence;
                    AssertEx.NotNull(evidence);
                    AssertEx.Equal(
                        LMCRpcSessionInitializationOutcome.Cancelled,
                        evidence.Outcome);
                    AssertEx.Equal(1, evidence.AttemptCount);
                    AssertEx.True(evidence.CanonicalRetryUsed);
                    AssertEx.NotNull(evidence.FirstFailureResponse);
                    AssertEx.Equal(
                        (short)-1,
                        evidence.FirstFailureResponse.ErrorId);
                    AssertEx.NotNull(evidence.LastReceivedResponse);
                    AssertEx.Equal(
                        (short)-1,
                        evidence.LastReceivedResponse.ErrorId);
                    AssertEx.Equal(
                        typeof(OperationCanceledException).FullName,
                        evidence.FailureType);
                    server.Verify();
                }
            }
        }

        private static void NegotiationEarlyTypedDispatch()
        {
            const uint eventMask = 0x80000001u;
            const uint bootId = 0x01020304u;
            const uint sessionEpoch = 0x55667788u;
            byte[] observedRequest = null;
            LMCCallbackWakeHintEventArgs received = null;
            var rawCount = 0;
            var stateAtDispatch = LMCConnectionState.Disconnected;

            var callbackStep = V2Step(bootId, sessionEpoch);
            callbackStep.InspectRequest = request =>
            {
                observedRequest = (byte[])request.Clone();
                AssertV2Request(
                    request,
                    eventMask,
                    512,
                    FirstCookie);
                SendEarlyWake(request, bootId, sessionEpoch, 7UL);
            };

            using (var receivedSignal = new ManualResetEventSlim(false))
            using (var connectedBeforeActivation =
                new ManualResetEventSlim(false))
            using (var releaseConnectedHandler =
                new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                callbackStep,
                CloseStep()))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                connection.ConnectionStateChanged += delegate(
                    object sender,
                    LMCConnectionStateChangedEventArgs e)
                {
                    if (e.CurrentState == LMCConnectionState.Connected)
                    {
                        connectedBeforeActivation.Set();
                        releaseConnectedHandler.Wait(5000);
                    }
                };
                connection.CallbackReceived += delegate
                {
                    Interlocked.Increment(ref rawCount);
                };
                connection.CallbackWakeHintReceived += delegate(
                    object sender,
                    LMCCallbackWakeHintEventArgs e)
                {
                    stateAtDispatch = connection.State;
                    var endpointCopy = e.RemoteEndPoint;
                    var receivedPort = endpointCopy.Port;
                    endpointCopy.Port = receivedPort == 1 ? 2 : 1;
                    AssertEx.Equal(receivedPort, e.RemoteEndPoint.Port);
                    received = e;
                    receivedSignal.Set();
                };

                Exception openingFailure = null;
                var openingThread = new Thread(
                    () =>
                    {
                        try
                        {
                            connection.RpcInitConnection(
                                "127.0.0.1",
                                server.Port,
                                "127.0.0.1",
                                0,
                                eventMask);
                        }
                        catch (Exception ex)
                        {
                            openingFailure = ex;
                        }
                    });
                openingThread.Start();

                try
                {
                    AssertEx.True(
                        connectedBeforeActivation.Wait(2000),
                        "The opener did not reach Connected before activation.");
                    AssertEx.Equal(
                        LMCConnectionState.Connected,
                        connection.State);
                    AssertEx.False(
                        receivedSignal.Wait(100),
                        "A typed callback escaped before gate activation.");
                }
                finally
                {
                    releaseConnectedHandler.Set();
                }

                AssertEx.True(
                    openingThread.Join(2000),
                    "The gated connection opener did not complete.");
                AssertEx.Equal<Exception>(null, openingFailure);

                AssertEx.NotNull(observedRequest);
                AssertEx.True(connection.IsConnected);
                AssertEx.True(connection.IsCallbackListenerRunning);
                var localEndPointCopy = connection.CallbackLocalEndPoint;
                var callbackPort = localEndPointCopy.Port;
                localEndPointCopy.Port = callbackPort == 1 ? 2 : 1;
                AssertEx.Equal(
                    callbackPort,
                    connection.CallbackLocalEndPoint.Port);
                AssertEx.NotNull(connection.RpcCallbackRegistrationResponse);
                AssertEx.True(
                    connection.RpcCallbackRegistrationResponse.IsSuccess);
                AssertEx.True(
                    connection.RpcCallbackRegistrationResponse.HasCommandResult);
                AssertEx.NotNull(
                    connection.RpcCallbackRegistrationV2Response);
                AssertEx.Equal(
                    (ushort)2,
                    connection.RpcCallbackRegistrationV2Response
                        .AcceptedVersion);
                AssertEx.Equal(
                    bootId,
                    connection.RpcCallbackRegistrationV2Response
                        .DiagnosticsBootId);
                AssertEx.Equal(
                    sessionEpoch,
                    connection.RpcCallbackRegistrationV2Response
                        .SessionEpoch);
                AssertEx.Equal(
                    FirstCookie,
                    connection.RpcCallbackRegistrationV2Response
                        .SessionFence.Cookie);

                AssertEx.True(
                    receivedSignal.Wait(2000),
                    "The early version-2 datagram was not dispatched.");
                AssertEx.Equal(LMCConnectionState.Connected, stateAtDispatch);
                AssertEx.Equal(0, rawCount);
                AssertEx.NotNull(received);
                AssertEx.True(received.BelongsTo(connection));
                AssertEx.True(received.BelongsToCurrentSession(connection));
                AssertEx.Equal(7UL, received.WakeHint.Sequence);
                AssertEx.False(received.WakeHint.IsAuthoritative);
                AssertEx.True(
                    received.WakeHint.RequiresAuthoritativeTcpQuery);
                AssertEx.Equal(1L, connection.AcceptedCallbackWakeHintCount);
                AssertEx.Equal(0L, connection.RejectedCallbackCount);

                connection.CloseConnection();
                AssertEx.False(received.BelongsToCurrentSession(connection));
                AssertEx.Equal<LMCCallbackRegistrationV2Response>(
                    null,
                    connection.RpcCallbackRegistrationV2Response);
                server.Verify();
            }
        }

        private static void StrictFenceAndBoundedReceive()
        {
            var typedCount = 0;
            var rawCount = 0;
            var errorCount = 0;
            var statisticsCount = 0;
            var acceptedStatisticsCount = 0;
            LMCCallbackV2StatisticsChangedEventArgs lastStatistics = null;
            LMCCallbackV2StatisticsChangedEventArgs lastAcceptedStatistics = null;

            using (var server = new FakeRpcServer(
                InitStep(),
                V2Step(0x301u, 0x401u),
                CloseStep()))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie, false)))
            {
                connection.CallbackReceived += delegate
                {
                    Interlocked.Increment(ref rawCount);
                };
                connection.CallbackWakeHintReceived += delegate
                {
                    Interlocked.Increment(ref typedCount);
                };
                connection.CallbackV2StatisticsChanged += (sender, e) =>
                {
                    lastStatistics = e;
                    if (e.DecisionKind
                        == LMCCallbackFenceDecisionKind.AcceptedWakeHint)
                    {
                        lastAcceptedStatistics = e;
                        Interlocked.Increment(ref acceptedStatisticsCount);
                    }
                    Interlocked.Increment(ref statisticsCount);
                };
                connection.CallbackListenerError += delegate
                {
                    Interlocked.Increment(ref errorCount);
                };
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    1u);

                var valid10 = EncodeWake(connection, 10UL);
                var wrongBoot = Clone(valid10);
                TestFrame.WriteUInt32(wrongBoot, 16, 0x302u);
                var wrongSession = Clone(valid10);
                TestFrame.WriteUInt32(wrongSession, 20, 0x402u);
                var wrongCookie = Clone(valid10);
                TestFrame.WriteUInt32(wrongCookie, 24, 0x01020304u);
                var invalidPolicy = EncodeWake(connection, 11UL);
                TestFrame.WriteUInt16(invalidPolicy, 10, 2);

                using (var sender = new UdpClient(
                    new IPEndPoint(IPAddress.Loopback, 0)))
                {
                    Send(sender, connection, new byte[] { 1, 2, 3, 4 });
                    Send(sender, connection, wrongBoot);
                    Send(sender, connection, wrongSession);
                    Send(sender, connection, wrongCookie);

                    using (var wrongSource = new UdpClient(
                        new IPEndPoint(
                            IPAddress.Parse("127.0.0.2"),
                            0)))
                    {
                        Send(wrongSource, connection, valid10);
                    }

                    Send(sender, connection, valid10);
                    Send(sender, connection, valid10);
                    Send(sender, connection, EncodeWake(connection, 9UL));
                    Send(sender, connection, invalidPolicy);
                    Send(sender, connection, new byte[513]);
                    Send(sender, connection, EncodeWake(connection, 11UL));
                }

                AssertEx.True(
                    SpinWait.SpinUntil(
                        () => connection.AcceptedCallbackWakeHintCount == 2
                            && connection.RejectedCallbackCount == 9
                            && Volatile.Read(ref typedCount) == 2
                            && Volatile.Read(ref statisticsCount) == 11,
                        3000),
                    "The version-2 rejection matrix did not settle.");
                AssertEx.Equal(2, typedCount);
                AssertEx.Equal(0, rawCount);
                AssertEx.Equal(0, errorCount);
                AssertEx.Equal(11, statisticsCount);
                AssertEx.Equal(2, acceptedStatisticsCount);
                AssertEx.Equal(1L, connection.DuplicateCallbackWakeHintCount);
                AssertEx.Equal(1L, connection.OutOfOrderCallbackWakeHintCount);
                AssertEx.NotNull(lastStatistics);
                AssertEx.NotNull(lastAcceptedStatistics);
                AssertEx.Equal(
                    LMCCallbackFenceDecisionKind.AcceptedWakeHint,
                    lastAcceptedStatistics.DecisionKind);
                AssertEx.Equal(
                    LMCCallbackProtocolError.None,
                    lastAcceptedStatistics.ProtocolError);
                AssertEx.Equal(2L, lastStatistics.AcceptedWakeHintCount);
                AssertEx.Equal(9L, lastStatistics.RejectedCount);
                AssertEx.Equal(1L, lastStatistics.DuplicateWakeHintCount);
                AssertEx.Equal(1L, lastStatistics.OutOfOrderWakeHintCount);
                AssertEx.True(connection.IsCallbackListenerRunning);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void D5TerminalTicketCorrelation()
        {
            const uint bootId = 0x11224488u;
            const uint sessionEpoch = 0x55667788u;
            const uint ticketId = 0xA1B2C3D4u;
            LMCCallbackWakeHintEventArgs received = null;

            using (var signal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                V2Step(bootId, sessionEpoch),
                CloseStep()))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            using (var otherConnection = new LMCConnection())
            {
                connection.CallbackWakeHintReceived += delegate(
                    object sender,
                    LMCCallbackWakeHintEventArgs e)
                {
                    received = e;
                    signal.Set();
                };
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    1u);
                Send(connection, EncodeWake(connection, 1UL));
                AssertEx.True(signal.Wait(2000));
                AssertEx.NotNull(received);

                AssertEx.Equal(
                    (ushort)1,
                    (ushort)LMCCallbackWakeHintEventType
                        .DiagnosticsOperationTerminalAvailable);
                var ticket = D5WriteTicket(
                    connection.Diagnostics,
                    connection.SessionGeneration,
                    ticketId,
                    bootId);
                AssertEx.True(
                    received.MatchesD5OperationTerminalTicket(
                        connection,
                        ticket));
                AssertEx.False(
                    received.MatchesD5OperationTerminalTicket(null, ticket));
                AssertEx.False(
                    received.MatchesD5OperationTerminalTicket(
                        connection,
                        null));
                AssertEx.False(
                    received.MatchesD5OperationTerminalTicket(
                        connection,
                        D5WriteTicket(
                            otherConnection.Diagnostics,
                            connection.SessionGeneration,
                            ticketId,
                            bootId)));
                AssertEx.False(
                    received.MatchesD5OperationTerminalTicket(
                        connection,
                        D5WriteTicket(
                            connection.Diagnostics,
                            connection.SessionGeneration + 1,
                            ticketId,
                            bootId)));
                AssertEx.False(
                    received.MatchesD5OperationTerminalTicket(
                        connection,
                        D5WriteTicket(
                            connection.Diagnostics,
                            connection.SessionGeneration,
                            ticketId + 1,
                            bootId)));
                AssertEx.False(
                    received.MatchesD5OperationTerminalTicket(
                        connection,
                        D5WriteTicket(
                            connection.Diagnostics,
                            connection.SessionGeneration,
                            ticketId,
                            bootId + 1)));

                AssertEx.False(
                    WithWakeHint(
                        received,
                        connection,
                        2,
                        1u,
                        ticketId,
                        0,
                        new byte[0])
                        .MatchesD5OperationTerminalTicket(
                            connection,
                            ticket));
                AssertEx.False(
                    WithWakeHint(
                        received,
                        connection,
                        1,
                        2u,
                        ticketId,
                        0,
                        new byte[0])
                        .MatchesD5OperationTerminalTicket(
                            connection,
                            ticket));
                AssertEx.False(
                    WithWakeHint(
                        received,
                        connection,
                        1,
                        1u,
                        0u,
                        0,
                        new byte[0])
                        .MatchesD5OperationTerminalTicket(
                            connection,
                            ticket));
                AssertEx.False(
                    WithWakeHint(
                        received,
                        connection,
                        1,
                        1u,
                        ticketId,
                        1,
                        new byte[0])
                        .MatchesD5OperationTerminalTicket(
                            connection,
                            ticket));
                AssertEx.False(
                    WithWakeHint(
                        received,
                        connection,
                        1,
                        1u,
                        ticketId,
                        0,
                        new byte[] { 1 })
                        .MatchesD5OperationTerminalTicket(
                            connection,
                            ticket));

                connection.CloseConnection();
                AssertEx.False(
                    received.MatchesD5OperationTerminalTicket(
                        connection,
                        ticket));
                server.Verify();
            }
        }

        private static void HandlerFailureContinues()
        {
            var typedCount = 0;
            Exception reported = null;
            using (var errorSignal = new ManualResetEventSlim(false))
            using (var secondSignal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                V2Step(0x501u, 0x601u),
                CloseStep()))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                connection.CallbackWakeHintReceived += delegate
                {
                    if (Interlocked.Increment(ref typedCount) == 1)
                    {
                        throw new InvalidOperationException(
                            "Expected version-2 handler failure.");
                    }

                    secondSignal.Set();
                };
                connection.CallbackListenerError += delegate(
                    object sender,
                    LMCCallbackErrorEventArgs e)
                {
                    reported = e.Exception;
                    errorSignal.Set();
                };
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    1u);

                Send(connection, EncodeWake(connection, 1UL));
                AssertEx.True(errorSignal.Wait(2000));
                AssertEx.NotNull(reported);
                AssertEx.Contains(
                    "Expected version-2 handler failure",
                    reported.Message);
                Send(connection, EncodeWake(connection, 2UL));
                AssertEx.True(secondSignal.Wait(2000));
                AssertEx.Equal(2, typedCount);
                AssertEx.Equal(2L, connection.AcceptedCallbackWakeHintCount);
                AssertEx.True(connection.IsCallbackListenerRunning);

                connection.CloseConnection();
                server.Verify();
            }
        }

        private static void GateOwnershipSurvivesJoinTimeout()
        {
            using (var workerReady = new ManualResetEventSlim(false))
            using (var releaseWorker = new ManualResetEventSlim(false))
            using (var workerReleased = new ManualResetEventSlim(false))
            {
                var options = V2Options(() => FirstCookie);
                options.CallbackThreadJoinTimeoutMilliseconds = 1;
                options.CallbackThreadReadyBeforeGateWaitObserver = () =>
                {
                    workerReady.Set();
                    releaseWorker.Wait();
                    workerReleased.Set();
                };

                using (var server = new FakeRpcServer(
                    InitStep(),
                    V2Step(0x611u, 0x612u),
                    CloseStep()))
                using (var connection = new LMCConnection(options))
                {
                    connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        1u);
                    AssertEx.True(workerReady.Wait(2000));

                    connection.CloseConnection();
                    AssertEx.False(connection.IsCallbackListenerRunning);
                    AssertEx.Equal(
                        LMCConnectionState.Disconnected,
                        connection.State);

                    releaseWorker.Set();
                    AssertEx.True(workerReleased.Wait(2000));
                    Thread.Sleep(50);
                    server.Verify();
                }
            }
        }

        private static void ReentrantCloseInvalidatesTypedEvent()
        {
            LMCCallbackWakeHintEventArgs received = null;
            using (var closedSignal = new ManualResetEventSlim(false))
            using (var server = new FakeRpcServer(
                InitStep(),
                V2Step(0x621u, 0x622u),
                CloseStep()))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                connection.CallbackWakeHintReceived += delegate(
                    object sender,
                    LMCCallbackWakeHintEventArgs e)
                {
                    received = e;
                    connection.CloseConnection();
                    closedSignal.Set();
                };
                connection.RpcInitConnection(
                    "127.0.0.1",
                    server.Port,
                    "127.0.0.1",
                    0,
                    1u);

                Send(connection, EncodeWake(connection, 1UL));
                AssertEx.True(closedSignal.Wait(2000));
                AssertEx.NotNull(received);
                AssertEx.False(received.BelongsToCurrentSession(connection));
                AssertEx.False(connection.IsConnected);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal(
                    LMCConnectionState.Disconnected,
                    connection.State);
                server.Verify();
            }
        }

        private static void SafetyAbortDetachRejectsTypedEvent()
        {
            var typedCount = 0;
            using (var clientDetached = new ManualResetEventSlim(false))
            using (var releaseAbort = new ManualResetEventSlim(false))
            using (var datagramProcessed = new ManualResetEventSlim(false))
            {
                var options = V2Options(() => FirstCookie);
                options.SafetyPreemptionClientDetachedObserver = () =>
                {
                    clientDetached.Set();
                    releaseAbort.Wait(5000);
                };
                options.CallbackV2DatagramProcessedObserver = () =>
                    datagramProcessed.Set();

                using (var server = new FakeRpcServer(
                    InitStep(),
                    V2Step(0x631u, 0x632u),
                    new FakeRpcStep(0, new byte[0])
                    {
                        RequireClientDisconnectBeforeRequest = true
                    }))
                using (var connection = new LMCConnection(options))
                {
                    connection.CallbackWakeHintReceived += delegate
                    {
                        Interlocked.Increment(ref typedCount);
                    };
                    connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        1u);

                    LMCSafetyPreemptionAbortEvidence evidence = null;
                    Exception abortFailure = null;
                    var abortThread = new Thread(
                        () =>
                        {
                            try
                            {
                                evidence = connection
                                    .AbortTransportForSafetyPreemption(
                                        connection.SessionGeneration);
                            }
                            catch (Exception ex)
                            {
                                abortFailure = ex;
                            }
                        });
                    abortThread.Start();

                    try
                    {
                        AssertEx.True(clientDetached.Wait(2000));
                        AssertEx.True(connection.IsConnected);
                        AssertEx.True(
                            connection.IsCallbackListenerRunning);

                        Send(connection, EncodeWake(connection, 1UL));
                        AssertEx.True(
                            datagramProcessed.Wait(2000),
                            "The detached-session datagram was not evaluated.");
                        AssertEx.Equal(0, Volatile.Read(ref typedCount));
                        AssertEx.Equal(
                            0L,
                            connection.AcceptedCallbackWakeHintCount);
                        AssertEx.Equal(
                            0L,
                            connection.RejectedCallbackCount);
                    }
                    finally
                    {
                        releaseAbort.Set();
                    }

                    AssertEx.True(abortThread.Join(2000));
                    AssertEx.Equal<Exception>(null, abortFailure);
                    AssertEx.NotNull(evidence);
                    AssertEx.True(evidence.TransportDetached);
                    AssertEx.True(evidence.FaultStatePublished);
                    AssertEx.Equal(
                        LMCConnectionState.Faulted,
                        connection.State);
                    AssertEx.False(connection.IsCallbackListenerRunning);
                    AssertEx.Equal(0, Volatile.Read(ref typedCount));
                    server.Verify();
                }
            }
        }

        private static void InvalidResponseNoDowngrade()
        {
            AssertInvalidResponseNoDowngrade(
                CanonicalFailurePayload(),
                typeof(InvalidOperationException),
                "Status=1, ErrorId=-1");

            var malformedFailure = CanonicalFailurePayload();
            malformedFailure[4] = 1;
            AssertInvalidResponseNoDowngrade(
                malformedFailure,
                typeof(InvalidDataException),
                "zero every accepted-fence field");
            AssertInvalidResponseNoDowngrade(
                new byte[4],
                typeof(InvalidDataException),
                "exactly 20 payload bytes");
        }

        private static void ReconnectRejectsOldSession()
        {
            var cookieCall = 0;
            var options = V2Options(
                () => Interlocked.Increment(ref cookieCall) == 1
                    ? FirstCookie
                    : SecondCookie);
            LMCCallbackWakeHintEventArgs firstEvent = null;
            LMCCallbackV2StatisticsChangedEventArgs firstStatistics = null;
            var typedCount = 0;
            var statisticsCount = 0;

            using (var firstSignal = new ManualResetEventSlim(false))
            using (var secondSignal = new ManualResetEventSlim(false))
            using (var firstServer = new FakeRpcServer(
                InitStep(),
                V2Step(0x701u, 0x801u),
                CloseStep()))
            using (var secondServer = new FakeRpcServer(
                InitStep(),
                V2Step(0x702u, 0x802u),
                CloseStep()))
            using (var connection = new LMCConnection(options))
            {
                connection.CallbackWakeHintReceived += delegate(
                    object sender,
                    LMCCallbackWakeHintEventArgs e)
                {
                    if (Interlocked.Increment(ref typedCount) == 1)
                    {
                        firstEvent = e;
                        firstSignal.Set();
                    }
                    else
                    {
                        secondSignal.Set();
                    }
                };
                connection.CallbackV2StatisticsChanged += delegate(
                    object sender,
                    LMCCallbackV2StatisticsChangedEventArgs e)
                {
                    if (Interlocked.Increment(ref statisticsCount) == 1)
                    {
                        firstStatistics = e;
                    }
                };
                connection.RpcInitConnection(
                    "127.0.0.1",
                    firstServer.Port,
                    "127.0.0.1",
                    0,
                    1u);
                var oldDatagram = EncodeWake(connection, 1UL);
                Send(connection, oldDatagram);
                AssertEx.True(firstSignal.Wait(2000));
                AssertEx.NotNull(firstEvent);
                AssertEx.True(firstEvent.BelongsToCurrentSession(connection));
                AssertEx.NotNull(firstStatistics);
                AssertEx.True(
                    firstStatistics.BelongsToCurrentSession(connection));

                connection.RpcInitConnection(
                    "127.0.0.1",
                    secondServer.Port,
                    "127.0.0.1",
                    0,
                    1u);
                AssertEx.False(firstEvent.BelongsToCurrentSession(connection));
                AssertEx.False(
                    firstStatistics.BelongsToCurrentSession(connection));
                AssertEx.Equal(
                    SecondCookie,
                    connection.RpcCallbackRegistrationV2Response
                        .SessionFence.Cookie);
                AssertEx.Equal(0L, connection.RejectedCallbackCount);

                Send(connection, oldDatagram);
                AssertEx.True(
                    SpinWait.SpinUntil(
                        () => connection.RejectedCallbackCount == 1,
                        2000));
                AssertEx.Equal(1, typedCount);
                Send(connection, EncodeWake(connection, 1UL));
                AssertEx.True(secondSignal.Wait(2000));
                AssertEx.Equal(2, typedCount);
                AssertEx.Equal(1L, connection.AcceptedCallbackWakeHintCount);

                connection.CloseConnection();
                firstServer.Verify();
                secondServer.Verify();
            }
        }

        private static void AssertInvalidResponseNoDowngrade(
            byte[] payload,
            Type expectedExceptionType,
            string expectedMessage)
        {
            using (var server = new FakeRpcServer(
                InitStep(),
                new FakeRpcStep(
                    0x405C,
                    TestFrame.Response(0, payload)),
                new FakeRpcStep(0, new byte[0])
                {
                    RequireClientDisconnectBeforeRequest = true
                }))
            using (var connection = new LMCConnection(
                V2Options(() => FirstCookie)))
            {
                Exception observed = null;
                try
                {
                    connection.RpcInitConnection(
                        "127.0.0.1",
                        server.Port,
                        "127.0.0.1",
                        0,
                        1u);
                }
                catch (Exception ex)
                {
                    observed = ex;
                }

                AssertEx.NotNull(observed);
                AssertEx.Equal(expectedExceptionType, observed.GetType());
                AssertEx.Contains(expectedMessage, observed.Message);
                AssertEx.Equal(LMCConnectionState.Faulted, connection.State);
                AssertEx.False(connection.IsConnected);
                AssertEx.False(connection.IsCallbackListenerRunning);
                AssertEx.Equal<LMCCallbackRegistrationV2Response>(
                    null,
                    connection.RpcCallbackRegistrationV2Response);
                AssertEx.Equal(2, server.ReceivedRequests.Count);
                AssertEx.Equal(
                    (ushort)32,
                    TestFrame.ReadUInt16(server.ReceivedRequests[1], 4));
                server.Verify();
            }
        }

        private static LMCConnectionOptions V2Options(
            Func<ulong> cookieFactory,
            bool validateLegacySource = true)
        {
            return new LMCConnectionOptions
            {
                CallbackRegistrationMode =
                    LMCCallbackRegistrationMode.Version2WakeHint,
                CallbackRequestedMaxDatagramBytes = 512,
                ValidateCallbackSourceAddress = validateLegacySource,
                CallbackCookieFactory = cookieFactory
            };
        }

        private static FakeRpcStep InitStep()
        {
            var payload = new byte[24];
            TestFrame.WriteUInt32(payload, 0, 64);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(0, payload));
        }

        private static FakeRpcStep InitShortFailureStep(short errorId)
        {
            var payload = new byte[4];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteInt16(payload, 2, errorId);
            return new FakeRpcStep(
                0x8080,
                TestFrame.Response(1, payload));
        }

        private static FakeRpcStep V2Step(
            uint bootId,
            uint sessionEpoch)
        {
            return new FakeRpcStep(0x405C, null)
            {
                ResponseFactory = request => TestFrame.Response(
                    0,
                    SuccessPayload(request, bootId, sessionEpoch))
            };
        }

        private static FakeRpcStep CloseStep()
        {
            return new FakeRpcStep(
                0x405D,
                TestFrame.Response(
                    0,
                    TestFrame.Hex("00 00 00 00")));
        }

        private static byte[] SuccessPayload(
            byte[] request,
            uint bootId,
            uint sessionEpoch)
        {
            AssertEx.Equal(40, request.Length);
            AssertEx.Equal((ushort)32, TestFrame.ReadUInt16(request, 4));
            var payload = new byte[20];
            TestFrame.WriteUInt16(payload, 4, 2);
            TestFrame.WriteUInt16(
                payload,
                6,
                TestFrame.ReadUInt16(request, 22));
            TestFrame.WriteUInt32(payload, 8, bootId);
            TestFrame.WriteUInt32(payload, 12, sessionEpoch);
            return payload;
        }

        private static byte[] CanonicalFailurePayload()
        {
            var payload = new byte[20];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteInt16(payload, 2, -1);
            return payload;
        }

        private static void AssertV2Request(
            byte[] request,
            uint eventMask,
            ushort maximum,
            ulong cookie)
        {
            AssertEx.Equal(40, request.Length);
            AssertEx.Equal((ushort)0x405C, TestFrame.ReadUInt16(request, 0));
            AssertEx.Equal((ushort)32, TestFrame.ReadUInt16(request, 4));
            AssertEx.Equal(eventMask, TestFrame.ReadUInt32(request, 8));
            AssertEx.True(TestFrame.ReadInt32(request, 12) > 0);
            AssertEx.SequenceEqual(
                new byte[] { 127, 0, 0, 1 },
                new byte[]
                {
                    request[16],
                    request[17],
                    request[18],
                    request[19]
                });
            AssertEx.Equal((ushort)2, TestFrame.ReadUInt16(request, 20));
            AssertEx.Equal(maximum, TestFrame.ReadUInt16(request, 22));
            AssertEx.Equal((uint)cookie, TestFrame.ReadUInt32(request, 24));
            AssertEx.Equal(
                (uint)(cookie >> 32),
                TestFrame.ReadUInt32(request, 28));
            AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 32));
            AssertEx.Equal(0u, TestFrame.ReadUInt32(request, 36));
        }

        private static void SendEarlyWake(
            byte[] request,
            uint bootId,
            uint sessionEpoch,
            ulong sequence)
        {
            var callbackAddress = new byte[]
            {
                request[16],
                request[17],
                request[18],
                request[19]
            };
            var fence = new LMCCallbackSessionFence(
                1,
                IPAddress.Loopback.GetAddressBytes(),
                TestFrame.ReadUInt32(request, 8),
                TestFrame.ReadUInt16(request, 22),
                bootId,
                sessionEpoch,
                TestFrame.ReadUInt32(request, 24),
                TestFrame.ReadUInt32(request, 28));
            var datagram = LMCCallbackProtocol.EncodeDatagram(
                WakeWrite(sequence),
                fence,
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
            using (var sender = new UdpClient(
                new IPEndPoint(IPAddress.Loopback, 0)))
            {
                sender.Send(
                    datagram,
                    datagram.Length,
                    new IPEndPoint(
                        new IPAddress(callbackAddress),
                        TestFrame.ReadInt32(request, 12)));
            }
        }

        private static byte[] EncodeWake(
            LMCConnection connection,
            ulong sequence)
        {
            return LMCCallbackProtocol.EncodeDatagram(
                WakeWrite(sequence),
                connection.RpcCallbackRegistrationV2Response.SessionFence,
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
        }

        private static LMCCallbackDatagramWrite WakeWrite(ulong sequence)
        {
            return new LMCCallbackDatagramWrite(
                1,
                1u,
                sequence,
                0xA1B2C3D4u,
                0x01020304u,
                0,
                new byte[0]);
        }

        private static void Send(
            LMCConnection connection,
            byte[] datagram)
        {
            using (var sender = new UdpClient(
                new IPEndPoint(IPAddress.Loopback, 0)))
            {
                Send(sender, connection, datagram);
            }
        }

        private static void Send(
            UdpClient sender,
            LMCConnection connection,
            byte[] datagram)
        {
            sender.Send(
                datagram,
                datagram.Length,
                connection.CallbackLocalEndPoint);
        }

        private static byte[] Clone(byte[] value)
        {
            return (byte[])value.Clone();
        }

        private static LMCOperationTicket D5WriteTicket(
            LMCDiagnostics owner,
            long sessionGeneration,
            uint ticketId,
            uint bootId)
        {
            return new LMCOperationTicket(
                ticketId,
                LMCOperationKind.SDOWrite,
                0x01020304u,
                bootId,
                1u,
                sessionGeneration,
                owner,
                false,
                0,
                LMCSignalValueType.Invalid);
        }

        private static LMCCallbackWakeHintEventArgs WithWakeHint(
            LMCCallbackWakeHintEventArgs source,
            LMCConnection connection,
            ushort eventType,
            uint eventMaskBit,
            uint eventId,
            byte deliveryClass,
            byte[] payload)
        {
            var lifetimeField = typeof(LMCCallbackWakeHintEventArgs).GetField(
                "connectionLifetimeGeneration",
                BindingFlags.Instance | BindingFlags.NonPublic);
            AssertEx.NotNull(lifetimeField);
            var lifetimeGeneration = (long)lifetimeField.GetValue(source);
            var fence = connection.RpcCallbackRegistrationV2Response
                .SessionFence;
            var datagram = new LMCCallbackDatagram(
                eventType,
                eventMaskBit,
                fence.BootId,
                fence.SessionEpoch,
                fence.CookieLo,
                fence.CookieHi,
                source.WakeHint.Sequence,
                eventId,
                source.WakeHint.PlcTimeMs,
                deliveryClass,
                payload);
            return new LMCCallbackWakeHintEventArgs(
                new LMCCallbackWakeHint(datagram),
                source.RemoteEndPoint,
                source.ReceivedAtUtc,
                connection,
                lifetimeGeneration,
                source.SessionGeneration);
        }
    }
}

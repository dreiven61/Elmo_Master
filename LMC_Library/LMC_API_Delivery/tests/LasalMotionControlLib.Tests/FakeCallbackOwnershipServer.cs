using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LasalMotionControlLib.Tests
{
    internal sealed class FakeCallbackOwnershipRequest
    {
        internal FakeCallbackOwnershipRequest(
            int sessionOrdinal,
            string peerAddress,
            byte[] frame)
        {
            SessionOrdinal = sessionOrdinal;
            PeerAddress = peerAddress;
            Frame = (byte[])frame.Clone();
        }

        internal int SessionOrdinal { get; private set; }
        internal string PeerAddress { get; private set; }
        internal byte[] Frame { get; private set; }
        internal ushort Command
        {
            get { return TestFrame.ReadUInt16(Frame, 0); }
        }
    }

    internal sealed class FakeCallbackOwnershipServer : IDisposable
    {
        private const int IoTimeoutMilliseconds = 3000;
        private const int JoinTimeoutMilliseconds = 5000;
        private const uint BootId = 0xA1B2C3D4u;

        private sealed class ClientSession
        {
            internal int Ordinal;
            internal string PeerAddress;
            internal TcpClient Client;
            internal Thread Worker;
            internal NetworkStream PreopenedStream;
            internal bool CloseCommandReceived;
            internal bool RetiredByTakeover;
            internal bool ImmediatePeerClose;
            internal readonly ManualResetEventSlim Disconnected =
                new ManualResetEventSlim(false);
        }

        private sealed class ProcessResult
        {
            internal byte[] Response;
            internal bool CloseAfterResponse;
        }

        private readonly object sync = new object();
        private readonly TcpListener listener;
        private readonly Thread acceptWorker;
        private readonly List<ClientSession> sessions =
            new List<ClientSession>();
        private readonly List<FakeCallbackOwnershipRequest> requests =
            new List<FakeCallbackOwnershipRequest>();
        private readonly List<Exception> workerExceptions =
            new List<Exception>();
        private readonly List<string> events = new List<string>();
        private volatile bool disposed;
        private int acceptedClientCount;
        private ClientSession owner;
        private byte[] ownerRegistration;
        private uint sessionEpoch;
        private int activeClientWorkers;
        private readonly bool holdDifferentIpInitialization;
        private readonly bool closeDifferentIpOnAccept;
        private readonly bool holdSameIpOwnerRetirement;

        internal FakeCallbackOwnershipServer(
            bool holdDifferentIpInitialization = false,
            bool closeDifferentIpOnAccept = false,
            bool holdSameIpOwnerRetirement = false)
        {
            this.holdDifferentIpInitialization =
                holdDifferentIpInitialization;
            this.closeDifferentIpOnAccept = closeDifferentIpOnAccept;
            this.holdSameIpOwnerRetirement =
                holdSameIpOwnerRetirement;
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            acceptWorker = new Thread(AcceptLoop)
            {
                IsBackground = true,
                Name = "LMC fake callback ownership accept"
            };
            acceptWorker.Start();
        }

        internal int Port { get; private set; }
        internal int AcceptedClientCount
        {
            get { return Volatile.Read(ref acceptedClientCount); }
        }
        internal int TakeoverCount { get; private set; }
        internal int RejectCount { get; private set; }
        internal int LateNonOwnerDisconnectCount { get; private set; }
        internal int OwnerCloseCount { get; private set; }
        internal int OldOwnerRetireBarrierCount { get; private set; }
        internal int CurrentOwnerSessionOrdinal
        {
            get
            {
                lock (sync)
                {
                    return owner == null ? 0 : owner.Ordinal;
                }
            }
        }
        internal IList<FakeCallbackOwnershipRequest> Requests
        {
            get
            {
                lock (sync)
                {
                    return requests.ToArray();
                }
            }
        }
        internal IList<string> Events
        {
            get
            {
                lock (sync)
                {
                    return events.ToArray();
                }
            }
        }

        internal void Verify(int expectedRequestCount)
        {
            if (!SpinWait.SpinUntil(
                () => Volatile.Read(ref activeClientWorkers) == 0,
                JoinTimeoutMilliseconds))
            {
                throw new TimeoutException(
                    "Fake callback ownership client workers did not finish.");
            }

            lock (sync)
            {
                if (workerExceptions.Count != 0)
                {
                    throw new AggregateException(
                        "Fake callback ownership server failed.",
                        workerExceptions.ToArray());
                }

                AssertEx.Equal(
                    expectedRequestCount,
                    requests.Count,
                    "Fake callback ownership request count mismatch.");
            }
        }

        private void AcceptLoop()
        {
            try
            {
                while (!disposed)
                {
                    TcpClient client;
                    try
                    {
                        client = listener.AcceptTcpClient();
                    }
                    catch (SocketException)
                    {
                        if (disposed)
                        {
                            return;
                        }

                        throw;
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }

                    var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    var session = new ClientSession
                    {
                        Ordinal = Interlocked.Increment(
                            ref acceptedClientCount),
                        PeerAddress = endpoint.Address.ToString(),
                        Client = client
                    };
                    session.Worker = new Thread(() => HandleClient(session))
                    {
                        IsBackground = true,
                        Name = "LMC fake callback ownership client"
                    };
                    lock (sync)
                    {
                        sessions.Add(session);
                        if (this.closeDifferentIpOnAccept
                            && owner != null
                            && !string.Equals(
                                owner.PeerAddress,
                                session.PeerAddress,
                                StringComparison.Ordinal))
                        {
                            RejectCount++;
                            session.ImmediatePeerClose = true;
                            session.PreopenedStream = client.GetStream();
                            client.Client.Shutdown(SocketShutdown.Send);
                        }
                    }

                    Interlocked.Increment(ref activeClientWorkers);
                    session.Worker.Start();
                }
            }
            catch (Exception ex)
            {
                if (!disposed)
                {
                    lock (sync)
                    {
                        workerExceptions.Add(ex);
                    }
                }
            }
        }

        private void HandleClient(ClientSession session)
        {
            try
            {
                var client = session.Client;
                client.NoDelay = true;
                client.ReceiveTimeout = IoTimeoutMilliseconds;
                client.SendTimeout = IoTimeoutMilliseconds;
                using (var stream = session.PreopenedStream
                    ?? client.GetStream())
                {
                    while (!disposed)
                    {
                        byte[] request;
                        try
                        {
                            request = ReadRequest(stream);
                        }
                        catch (Exception ex) when (IsExpectedDisconnect(ex))
                        {
                            break;
                        }

                        lock (sync)
                        {
                            requests.Add(new FakeCallbackOwnershipRequest(
                                session.Ordinal,
                                session.PeerAddress,
                                request));
                            events.Add(
                                "REQUEST:"
                                + session.Ordinal
                                + ":0x"
                                + TestFrame.ReadUInt16(request, 0)
                                    .ToString("X4"));
                        }

                        CallbackOwnershipWireTool.EnsureAllowedRequest(
                            request);
                        if (session.ImmediatePeerClose)
                        {
                            break;
                        }

                        var processed = Process(session, request);
                        if (processed.Response != null)
                        {
                            stream.Write(
                                processed.Response,
                                0,
                                processed.Response.Length);
                            stream.Flush();
                            lock (sync)
                            {
                                events.Add(
                                    "RESPONSE:"
                                    + session.Ordinal
                                    + ":0x"
                                    + TestFrame.ReadUInt16(request, 0)
                                        .ToString("X4"));
                            }
                        }
                        if (processed.CloseAfterResponse)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!disposed && !IsExpectedDisconnect(ex))
                {
                    lock (sync)
                    {
                        workerExceptions.Add(ex);
                    }
                }
            }
            finally
            {
                lock (sync)
                {
                    if (ReferenceEquals(owner, session))
                    {
                        owner = null;
                        ownerRegistration = null;
                    }
                    else if (!session.CloseCommandReceived)
                    {
                        LateNonOwnerDisconnectCount++;
                    }

                    events.Add("DISCONNECT:" + session.Ordinal);
                }

                session.Client.Close();
                session.Disconnected.Set();
                Interlocked.Decrement(ref activeClientWorkers);
            }
        }

        private ProcessResult Process(
            ClientSession session,
            byte[] request)
        {
            var command = TestFrame.ReadUInt16(request, 0);
            if (command == LMC_CommandId.RpcSessionInit)
            {
                lock (sync)
                {
                    if (owner != null
                        && !string.Equals(
                            owner.PeerAddress,
                            session.PeerAddress,
                            StringComparison.Ordinal))
                    {
                        if (holdDifferentIpInitialization)
                        {
                            return new ProcessResult
                            {
                                Response = null,
                                CloseAfterResponse = false
                            };
                        }

                        RejectCount++;
                        return new ProcessResult
                        {
                            Response = null,
                            CloseAfterResponse = true
                        };
                    }
                }

                var payload = new byte[24];
                TestFrame.WriteUInt32(payload, 0, 64);
                return new ProcessResult
                {
                    Response = TestFrame.Response(0, payload)
                };
            }

            if (command == LMC_CommandId.CloseConnection)
            {
                lock (sync)
                {
                    if (!ReferenceEquals(owner, session))
                    {
                        throw new InvalidOperationException(
                            "A non-owner sent 0x405D to the fake server.");
                    }

                    session.CloseCommandReceived = true;
                    owner = null;
                    ownerRegistration = null;
                    OwnerCloseCount++;
                }

                return new ProcessResult
                {
                    Response = TestFrame.Response(
                        0,
                        TestFrame.Hex("00 00 00 00")),
                    CloseAfterResponse = true
                };
            }

            var payloadBytes = new byte[
                LMCCallbackProtocol.RegistrationV2RequestPayloadBytes];
            Buffer.BlockCopy(
                request,
                LMC_Frame.HeaderSize,
                payloadBytes,
                0,
                payloadBytes.Length);
            var parsed = LMCCallbackProtocol.ParseRegistrationV2Payload(
                payloadBytes,
                IPAddress.Parse(session.PeerAddress).GetAddressBytes(),
                LMCCallbackProtocolPolicy.InitialV2WakeHint);
            if (!parsed.IsAccepted)
            {
                return new ProcessResult
                {
                    Response = RegistrationFailure(),
                    CloseAfterResponse = false
                };
            }

            ClientSession retire = null;
            ProcessResult result;
            lock (sync)
            {
                if (owner == null)
                {
                    owner = session;
                    ownerRegistration = (byte[])request.Clone();
                    sessionEpoch++;
                    result = new ProcessResult
                    {
                        Response = RegistrationSuccess(
                            request,
                            sessionEpoch)
                    };
                }
                else if (ReferenceEquals(owner, session))
                {
                    if (!FramesEqual(ownerRegistration, request))
                    {
                        result = new ProcessResult
                        {
                            Response = RegistrationFailure()
                        };
                    }
                    else
                    {
                        result = new ProcessResult
                        {
                            Response = RegistrationSuccess(
                                request,
                                sessionEpoch)
                        };
                    }
                }
                else if (string.Equals(
                    owner.PeerAddress,
                    session.PeerAddress,
                    StringComparison.Ordinal))
                {
                    if (!holdSameIpOwnerRetirement)
                    {
                        retire = owner;
                        retire.RetiredByTakeover = true;
                    }
                    owner = session;
                    ownerRegistration = (byte[])request.Clone();
                    sessionEpoch++;
                    TakeoverCount++;
                    result = new ProcessResult
                    {
                        Response = RegistrationSuccess(
                            request,
                            sessionEpoch)
                    };
                }
                else
                {
                    RejectCount++;
                    result = new ProcessResult
                    {
                        Response = RegistrationFailure(),
                        CloseAfterResponse = true
                    };
                }
            }

            if (retire != null)
            {
                try
                {
                    retire.Client.Client.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                }
                catch (ObjectDisposedException)
                {
                }

                retire.Client.Close();
                if (!retire.Disconnected.Wait(JoinTimeoutMilliseconds))
                {
                    throw new TimeoutException(
                        "Fake takeover did not observe the retired owner disconnect barrier.");
                }

                lock (sync)
                {
                    OldOwnerRetireBarrierCount++;
                    events.Add("TAKEOVER_BARRIER:" + retire.Ordinal);
                }
            }

            return result;
        }

        private static byte[] RegistrationSuccess(
            byte[] request,
            uint epoch)
        {
            var payload = new byte[20];
            TestFrame.WriteUInt16(payload, 4, 2);
            TestFrame.WriteUInt16(
                payload,
                6,
                TestFrame.ReadUInt16(request, 22));
            TestFrame.WriteUInt32(payload, 8, BootId);
            TestFrame.WriteUInt32(payload, 12, epoch);
            return TestFrame.Response(0, payload);
        }

        private static byte[] RegistrationFailure()
        {
            var payload = new byte[20];
            TestFrame.WriteUInt16(payload, 0, 1);
            TestFrame.WriteInt16(payload, 2, -1);
            return TestFrame.Response(0, payload);
        }

        private static bool FramesEqual(byte[] left, byte[] right)
        {
            if (left == null
                || right == null
                || left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static byte[] ReadRequest(NetworkStream stream)
        {
            var header = ReadExact(stream, LMC_Frame.HeaderSize);
            var payloadLength = TestFrame.ReadUInt16(header, 4);
            if (payloadLength
                > LMCCallbackProtocol.RegistrationV2RequestPayloadBytes)
            {
                throw new InvalidDataException(
                    "Fake callback request payload is over the fixed maximum.");
            }

            var payload = payloadLength == 0
                ? new byte[0]
                : ReadExact(stream, payloadLength);
            var request = new byte[header.Length + payload.Length];
            Buffer.BlockCopy(header, 0, request, 0, header.Length);
            if (payload.Length != 0)
            {
                Buffer.BlockCopy(
                    payload,
                    0,
                    request,
                    header.Length,
                    payload.Length);
            }

            return request;
        }

        private static byte[] ReadExact(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = stream.Read(buffer, offset, count - offset);
                if (read <= 0)
                {
                    throw new EndOfStreamException();
                }

                offset += read;
            }

            return buffer;
        }

        private static bool IsExpectedDisconnect(Exception exception)
        {
            if (exception is EndOfStreamException
                || exception is ObjectDisposedException)
            {
                return true;
            }

            var socket = exception as SocketException;
            if (socket != null)
            {
                return socket.SocketErrorCode == SocketError.ConnectionReset
                    || socket.SocketErrorCode == SocketError.ConnectionAborted
                    || socket.SocketErrorCode == SocketError.Shutdown
                    || socket.SocketErrorCode == SocketError.OperationAborted
                    || socket.SocketErrorCode == SocketError.Interrupted;
            }

            var io = exception as IOException;
            return io != null
                && io.InnerException != null
                && IsExpectedDisconnect(io.InnerException);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            listener.Stop();
            lock (sync)
            {
                foreach (var session in sessions)
                {
                    session.Client.Close();
                }
            }

            acceptWorker.Join(JoinTimeoutMilliseconds);
            ClientSession[] snapshot;
            lock (sync)
            {
                snapshot = sessions.ToArray();
            }

            foreach (var session in snapshot)
            {
                if (session.Worker != null && session.Worker.IsAlive)
                {
                    session.Worker.Join(JoinTimeoutMilliseconds);
                }

                if (session.Worker == null || !session.Worker.IsAlive)
                {
                    session.Disconnected.Dispose();
                }
            }
        }
    }
}

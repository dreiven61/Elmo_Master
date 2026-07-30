using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LasalMotionControlLib.Tests
{
    internal sealed class FakeRpcStep
    {
        internal FakeRpcStep(ushort command, byte[] response)
        {
            Command = command;
            Response = response;
        }

        internal ushort Command { get; private set; }
        internal byte[] Response { get; private set; }
        internal int[] ResponseChunks { get; set; }
        internal int ResponseDelayMilliseconds { get; set; }
        internal bool AllowClientDisconnectAfterRequest { get; set; }
        internal bool ContinueWithNextClientAfterResponseWriteDisconnect
        {
            get;
            set;
        }
        internal Action BeforeResponse { get; set; }
        internal bool RequireClientDisconnectBeforeRequest { get; set; }
        internal bool ContinueWithNextClientAfterDisconnect { get; set; }
        internal Action<byte[]> InspectRequest { get; set; }
        internal Action<byte[]> AfterResponse { get; set; }
        internal bool CloseClientBeforeResponse { get; set; }
        internal bool CloseClientBeforeResponseAndContinue { get; set; }
        internal bool WaitForClientDisconnectBeforeResponseAndContinue
        {
            get;
            set;
        }
        internal Action AfterClientDisconnect { get; set; }
        internal bool CloseAfterResponse { get; set; }
        internal bool CloseClientAfterResponseAndContinue { get; set; }
    }

    internal sealed class FakeRpcServer : IDisposable
    {
        private const int IoTimeoutMilliseconds = 3000;
        private const int JoinTimeoutMilliseconds = 5000;

        private readonly TcpListener listener;
        private readonly FakeRpcStep[] steps;
        private readonly Thread worker;
        private Exception workerException;
        private volatile bool disposed;

        internal FakeRpcServer(params FakeRpcStep[] steps)
        {
            this.steps = steps ?? new FakeRpcStep[0];
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            ReceivedRequests = new List<byte[]>();
            ReceivedRequestSessionOrdinals = new List<int>();

            worker = new Thread(Run)
            {
                IsBackground = true,
                Name = "LMC fake RPC server"
            };
            worker.Start();
        }

        internal int Port { get; private set; }
        internal int AcceptedClientCount { get; private set; }
        internal IList<byte[]> ReceivedRequests { get; private set; }
        internal IList<int> ReceivedRequestSessionOrdinals { get; private set; }

        internal void Verify()
        {
            if (!worker.Join(JoinTimeoutMilliseconds))
            {
                throw new TimeoutException("Fake RPC server did not finish.");
            }

            if (workerException != null)
            {
                throw new InvalidOperationException(
                    "Fake RPC server failed.",
                    workerException);
            }

            AssertEx.Equal(
                CountExpectedRequests(),
                ReceivedRequests.Count,
                "Fake RPC request count mismatch.");
        }

        private int CountExpectedRequests()
        {
            var count = 0;
            foreach (var step in steps)
            {
                if (step.RequireClientDisconnectBeforeRequest)
                {
                    if (step.ContinueWithNextClientAfterDisconnect)
                    {
                        continue;
                    }

                    break;
                }

                count++;
            }

            return count;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            listener.Stop();

            if (worker.IsAlive)
            {
                worker.Join(JoinTimeoutMilliseconds);
            }
        }

        private void Run()
        {
            try
            {
                var stepIndex = 0;
                while (true)
                {
                    var acceptNextClient = false;
                    using (var client = listener.AcceptTcpClient())
                    {
                        AcceptedClientCount++;
                        var sessionOrdinal = AcceptedClientCount;

                        client.NoDelay = true;
                        client.ReceiveTimeout = IoTimeoutMilliseconds;
                        client.SendTimeout = IoTimeoutMilliseconds;

                        using (var stream = client.GetStream())
                        {
                            while (stepIndex < steps.Length)
                            {
                                var step = steps[stepIndex];
                                byte[] request;
                                try
                                {
                                    request = ReadRequest(stream);
                                }
                                catch (Exception ex) when (
                                    step.RequireClientDisconnectBeforeRequest &&
                                    IsExpectedClientDisconnect(ex))
                                {
                                    stepIndex++;
                                    if (step.ContinueWithNextClientAfterDisconnect)
                                    {
                                        acceptNextClient = true;
                                        break;
                                    }

                                    return;
                                }
                                if (step.RequireClientDisconnectBeforeRequest)
                                {
                                    throw new InvalidOperationException(
                                        "The client sent an RPC request instead of disconnecting its transport.");
                                }

                                ReceivedRequests.Add(request);
                                ReceivedRequestSessionOrdinals.Add(sessionOrdinal);

                                AssertEx.Equal(
                                    step.Command,
                                    TestFrame.ReadUInt16(request, 0),
                                    "Unexpected RPC command.");

                                if (step.InspectRequest != null)
                                {
                                    step.InspectRequest(request);
                                }

                                if (step.ResponseDelayMilliseconds > 0)
                                {
                                    Thread.Sleep(step.ResponseDelayMilliseconds);
                                }

                                if (step.BeforeResponse != null)
                                {
                                    step.BeforeResponse();
                                }

                                if (step
                                    .WaitForClientDisconnectBeforeResponseAndContinue)
                                {
                                    WaitForClientDisconnect(client);
                                    if (step.AfterClientDisconnect != null)
                                    {
                                        step.AfterClientDisconnect();
                                    }
                                    stepIndex++;
                                    acceptNextClient = true;
                                    break;
                                }

                                if (step.CloseClientBeforeResponse)
                                {
                                    return;
                                }
                                if (step.CloseClientBeforeResponseAndContinue)
                                {
                                    stepIndex++;
                                    acceptNextClient = true;
                                    break;
                                }

                                try
                                {
                                    WriteResponse(stream, step.Response, step.ResponseChunks);
                                }
                                catch (Exception ex) when (
                                    step.AllowClientDisconnectAfterRequest &&
                                    IsExpectedClientDisconnect(ex))
                                {
                                    if (step
                                        .ContinueWithNextClientAfterResponseWriteDisconnect)
                                    {
                                        stepIndex++;
                                        acceptNextClient = true;
                                        break;
                                    }

                                    return;
                                }

                                if (step.AfterResponse != null)
                                {
                                    step.AfterResponse(request);
                                }

                                stepIndex++;
                                if (step.CloseClientAfterResponseAndContinue)
                                {
                                    acceptNextClient = true;
                                    break;
                                }

                                if (step.CloseAfterResponse)
                                {
                                    return;
                                }
                            }
                        }
                    }

                    if (!acceptNextClient)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!disposed)
                {
                    workerException = ex;
                }
            }
            finally
            {
                try
                {
                    listener.Stop();
                }
                catch
                {
                }
            }
        }

        private static bool IsExpectedClientDisconnect(Exception exception)
        {
            if (exception is EndOfStreamException)
            {
                return true;
            }

            var socketException = exception as SocketException;
            if (socketException != null)
            {
                return IsExpectedClientDisconnect(socketException.SocketErrorCode);
            }

            var ioException = exception as IOException;
            return ioException != null &&
                   ioException.InnerException != null &&
                   IsExpectedClientDisconnect(ioException.InnerException);
        }

        private static void WaitForClientDisconnect(TcpClient client)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(
                IoTimeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    if (client.Client.Poll(100000, SelectMode.SelectRead)
                        && client.Client.Available == 0)
                    {
                        return;
                    }
                }
                catch (SocketException ex) when (
                    IsExpectedClientDisconnect(ex.SocketErrorCode))
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }

            throw new TimeoutException(
                "The client did not disconnect before the held response deadline.");
        }

        private static bool IsExpectedClientDisconnect(SocketError socketError)
        {
            return socketError == SocketError.ConnectionReset ||
                   socketError == SocketError.ConnectionAborted ||
                   socketError == SocketError.Shutdown;
        }

        private static byte[] ReadRequest(NetworkStream stream)
        {
            var header = ReadExact(stream, 8);
            var payloadLength = TestFrame.ReadUInt16(header, 4);
            var payload = payloadLength == 0
                ? new byte[0]
                : ReadExact(stream, payloadLength);
            var request = new byte[header.Length + payload.Length];

            Buffer.BlockCopy(header, 0, request, 0, header.Length);
            if (payload.Length > 0)
            {
                Buffer.BlockCopy(payload, 0, request, header.Length, payload.Length);
            }

            return request;
        }

        private static byte[] ReadExact(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;

            while (offset < count)
            {
                var bytesRead = stream.Read(buffer, offset, count - offset);
                if (bytesRead <= 0)
                {
                    throw new EndOfStreamException();
                }

                offset += bytesRead;
            }

            return buffer;
        }

        private static void WriteResponse(
            NetworkStream stream,
            byte[] response,
            int[] chunks)
        {
            var safeResponse = response ?? new byte[0];
            var offset = 0;

            if (chunks != null)
            {
                foreach (var requestedChunk in chunks)
                {
                    if (offset >= safeResponse.Length)
                    {
                        break;
                    }

                    var count = Math.Min(requestedChunk, safeResponse.Length - offset);
                    if (count <= 0)
                    {
                        continue;
                    }

                    stream.Write(safeResponse, offset, count);
                    stream.Flush();
                    offset += count;
                }
            }

            if (offset < safeResponse.Length)
            {
                stream.Write(safeResponse, offset, safeResponse.Length - offset);
                stream.Flush();
            }
        }
    }
}

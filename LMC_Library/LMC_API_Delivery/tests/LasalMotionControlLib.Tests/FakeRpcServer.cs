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
        internal Action<byte[]> InspectRequest { get; set; }
        internal Action<byte[]> AfterResponse { get; set; }
        internal bool CloseAfterResponse { get; set; }
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

            worker = new Thread(Run)
            {
                IsBackground = true,
                Name = "LMC fake RPC server"
            };
            worker.Start();
        }

        internal int Port { get; private set; }
        internal IList<byte[]> ReceivedRequests { get; private set; }

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
                steps.Length,
                ReceivedRequests.Count,
                "Fake RPC request count mismatch.");
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
                using (var client = listener.AcceptTcpClient())
                {
                    client.NoDelay = true;
                    client.ReceiveTimeout = IoTimeoutMilliseconds;
                    client.SendTimeout = IoTimeoutMilliseconds;

                    using (var stream = client.GetStream())
                    {
                        foreach (var step in steps)
                        {
                            var request = ReadRequest(stream);
                            ReceivedRequests.Add(request);

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

                            try
                            {
                                WriteResponse(stream, step.Response, step.ResponseChunks);
                            }
                            catch (Exception) when (step.AllowClientDisconnectAfterRequest)
                            {
                                return;
                            }

                            if (step.AfterResponse != null)
                            {
                                step.AfterResponse(request);
                            }

                            if (step.CloseAfterResponse)
                            {
                                break;
                            }
                        }
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

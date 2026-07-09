using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LasalMotionControlLib
{
    public sealed class LMCConnection : IDisposable
    {
        public const int DefaultCallbackPort = 5003;
        public const uint DefaultEventMask = 0xFFFFFFFF;

        private const int ReceiveTimeoutMilliseconds = 3000;
        private const int SendTimeoutMilliseconds = 3000;
        private const int ResponseStatusLength = 4;
        private const int MinimumParsedResponseLength = LMC_Frame.HeaderSize + ResponseStatusLength;
        private const int CallbackThreadJoinTimeoutMilliseconds = 500;

        private readonly object sync = new object();
        private TcpClient client;
        private UdpClient callbackListener;
        private Thread callbackThread;
        private volatile bool callbackListenerRunning;

        public bool IsRpcInitialized { get; private set; }
        public bool IsCallbackListenerRunning
        {
            get { return callbackListenerRunning; }
        }

        public int CallbackPort { get; private set; }
        public uint EventMask { get; private set; }
        public IPEndPoint CallbackLocalEndPoint { get; private set; }
        public LMC_Response RpcSessionInitResponse { get; private set; }
        public LMC_Response RpcCallbackRegistrationResponse { get; private set; }

        public event EventHandler<LMCCallbackEventArgs> CallbackReceived;
        public event EventHandler<LMCCallbackErrorEventArgs> CallbackListenerError;

        public void RpcInitConnection(
            string remoteAddress,
            int remotePort,
            string localAddress)
        {
            RpcInitConnection(
                remoteAddress,
                remotePort,
                localAddress,
                DefaultCallbackPort,
                DefaultEventMask);
        }

        public void RpcInitConnection(
            string remoteAddress,
            int remotePort,
            string localAddress,
            int callbackPort,
            uint eventMask)
        {
            CloseConnection(false);

            if (callbackPort < 0 || callbackPort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    "callbackPort",
                    "Callback port must be between 0 and 65535.");
            }

            var parsedLocalAddress = IPAddress.Parse(localAddress);
            var localEndPoint = new IPEndPoint(parsedLocalAddress, 0);

            client = new TcpClient(localEndPoint)
            {
                NoDelay = true,
                ReceiveTimeout = ReceiveTimeoutMilliseconds,
                SendTimeout = SendTimeoutMilliseconds
            };

            try
            {
                client.Connect(IPAddress.Parse(remoteAddress), remotePort);

                RpcSessionInitResponse = Parse(Exchange(LMC_Frame.RpcSessionInit()));
                EnsureSuccess("RPC session init", RpcSessionInitResponse);

                StartCallbackListener(parsedLocalAddress, callbackPort);

                RpcCallbackRegistrationResponse = Parse(
                    Exchange(
                        LMC_Frame.RpcCallbackRegistration(
                            eventMask,
                            callbackPort,
                            parsedLocalAddress.GetAddressBytes())));
                EnsureSuccess("RPC callback registration", RpcCallbackRegistrationResponse);

                CallbackPort = callbackPort;
                EventMask = eventMask;
                IsRpcInitialized = true;
            }
            catch
            {
                CloseConnection(false);
                throw;
            }
        }

        public void LMC_RpcInitConnection(
            string remoteAddress,
            int remotePort,
            string localAddress)
        {
            RpcInitConnection(remoteAddress, remotePort, localAddress);
        }

        public void LMC_RpcInitConnection(
            string remoteAddress,
            int remotePort,
            string localAddress,
            int callbackPort,
            uint eventMask)
        {
            RpcInitConnection(
                remoteAddress,
                remotePort,
                localAddress,
                callbackPort,
                eventMask);
        }

        public void CloseConnection()
        {
            CloseConnection(true);
        }

        public void LMC_CloseConnection()
        {
            CloseConnection();
        }

        internal byte[] Exchange(byte[] request)
        {
            lock (sync)
            {
                EnsureConnected();

                var stream = client.GetStream();
                stream.Write(request, 0, request.Length);

                var header = ReadExact(stream, LMC_Frame.HeaderSize);
                var payloadLength = LMC_Frame.GetResponsePayloadLength(header);
                var payload = payloadLength == 0
                    ? new byte[0]
                    : ReadExact(stream, payloadLength);

                return CombineResponse(header, payload);
            }
        }

        internal static LMC_Response Parse(byte[] raw)
        {
            var response = new LMC_Response { Raw = raw };

            if (raw == null || raw.Length < MinimumParsedResponseLength)
            {
                return response;
            }

            response.Status = LMC_Frame.ReadUInt16(raw, raw.Length - 4);
            response.ErrorId = unchecked((short)LMC_Frame.ReadUInt16(raw, raw.Length - 2));

            return response;
        }

        public void Dispose()
        {
            CloseConnection(true);
        }

        private void CloseConnection(bool sendCloseCommand)
        {
            var currentClient = client;

            try
            {
                if (sendCloseCommand && currentClient != null && currentClient.Connected)
                {
                    Exchange(LMC_Frame.CloseConnection());
                }
            }
            catch
            {
            }
            finally
            {
                if (currentClient != null)
                {
                    currentClient.Close();
                }

                client = null;
                StopCallbackListener();
                IsRpcInitialized = false;
                CallbackPort = 0;
                EventMask = 0;
                RpcSessionInitResponse = null;
                RpcCallbackRegistrationResponse = null;
            }
        }

        private void EnsureConnected()
        {
            if (client == null || !client.Connected)
            {
                throw new InvalidOperationException("LMC connection is not open.");
            }
        }

        private void StartCallbackListener(IPAddress localAddress, int callbackPort)
        {
            StopCallbackListener();

            var listener = new UdpClient(new IPEndPoint(localAddress, callbackPort));

            callbackListener = listener;
            CallbackLocalEndPoint = (IPEndPoint)listener.Client.LocalEndPoint;
            callbackListenerRunning = true;

            callbackThread = new Thread(ReceiveCallbackLoop)
            {
                IsBackground = true,
                Name = "LMC RPC callback listener"
            };

            callbackThread.Start();
        }

        private void StopCallbackListener()
        {
            callbackListenerRunning = false;

            var listener = callbackListener;
            callbackListener = null;

            if (listener != null)
            {
                listener.Close();
            }

            var thread = callbackThread;
            if (thread != null
                && thread.IsAlive
                && Thread.CurrentThread.ManagedThreadId != thread.ManagedThreadId)
            {
                thread.Join(CallbackThreadJoinTimeoutMilliseconds);
            }

            callbackThread = null;
            CallbackLocalEndPoint = null;
        }

        private void ReceiveCallbackLoop()
        {
            while (callbackListenerRunning)
            {
                try
                {
                    var listener = callbackListener;
                    if (listener == null)
                    {
                        break;
                    }

                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    var payload = listener.Receive(ref remoteEndPoint);

                    OnCallbackReceived(
                        new LMCCallbackEventArgs(
                            payload,
                            remoteEndPoint,
                            DateTime.UtcNow));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    if (callbackListenerRunning)
                    {
                        OnCallbackListenerError(new LMCCallbackErrorEventArgs(ex));
                    }
                }
                catch (Exception ex)
                {
                    if (callbackListenerRunning)
                    {
                        OnCallbackListenerError(new LMCCallbackErrorEventArgs(ex));
                    }
                }
            }
        }

        private void OnCallbackReceived(LMCCallbackEventArgs e)
        {
            var handler = CallbackReceived;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch (Exception ex)
                {
                    OnCallbackListenerError(new LMCCallbackErrorEventArgs(ex));
                }
            }
        }

        private void OnCallbackListenerError(LMCCallbackErrorEventArgs e)
        {
            var handler = CallbackListenerError;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch
                {
                }
            }
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

        private static void EnsureSuccess(string operation, LMC_Response response)
        {
            if (response == null)
            {
                throw new InvalidOperationException(operation + " failed without a response.");
            }

            if (response.IsSuccess)
            {
                return;
            }

            throw new InvalidOperationException(
                operation
                + " failed. Status="
                + response.Status
                + ", ErrorId="
                + response.ErrorId
                + ".");
        }

        private static byte[] CombineResponse(byte[] header, byte[] payload)
        {
            var response = new byte[header.Length + payload.Length];

            Buffer.BlockCopy(header, 0, response, 0, header.Length);

            if (payload.Length > 0)
            {
                Buffer.BlockCopy(payload, 0, response, header.Length, payload.Length);
            }

            return response;
        }
    }
}

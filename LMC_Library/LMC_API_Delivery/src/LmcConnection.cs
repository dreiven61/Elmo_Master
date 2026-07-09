using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace LasalMotionControlLib
{
    public sealed class LMCConnection : IDisposable
    {
        private const int ReceiveTimeoutMilliseconds = 3000;
        private const int SendTimeoutMilliseconds = 3000;
        private const int ResponseStatusLength = 4;
        private const int MinimumParsedResponseLength = LMC_Frame.HeaderSize + ResponseStatusLength;

        private static readonly byte[] CloseConnectionRequest =
        {
            0x5D, 0x40, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00
        };

        private readonly object sync = new object();
        private TcpClient client;

        public void RpcInitConnection(
            string remoteAddress,
            int remotePort,
            string localAddress)
        {
            CloseConnection(false);

            var localEndPoint = new IPEndPoint(IPAddress.Parse(localAddress), 0);

            client = new TcpClient(localEndPoint)
            {
                NoDelay = true,
                ReceiveTimeout = ReceiveTimeoutMilliseconds,
                SendTimeout = SendTimeoutMilliseconds
            };

            client.Connect(IPAddress.Parse(remoteAddress), remotePort);
        }

        public void LMC_RpcInitConnection(
            string remoteAddress,
            int remotePort,
            string localAddress)
        {
            RpcInitConnection(remoteAddress, remotePort, localAddress);
        }

        public void CloseConnection()
        {
            CloseConnection(false);
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
            CloseConnection(false);
        }

        private void CloseConnection(bool sendCloseCommand)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                if (sendCloseCommand && client.Connected)
                {
                    Exchange(CloseConnectionRequest);
                }
            }
            catch
            {
            }
            finally
            {
                client.Close();
                client = null;
            }
        }

        private void EnsureConnected()
        {
            if (client == null || !client.Connected)
            {
                throw new InvalidOperationException("LMC connection is not open.");
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

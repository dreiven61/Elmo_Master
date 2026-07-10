using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LasalMotionControlLib
{
    public sealed class LMCConnection : IDisposable
    {
        public const int DefaultCallbackPort = 5003;
        public const uint DefaultEventMask = 0xFFFFFFFF;

        private const int ReceiveTimeoutMilliseconds = 3000;
        private const int SendTimeoutMilliseconds = 3000;
        private const int CallbackThreadJoinTimeoutMilliseconds = 500;
        private const int HeaderStatusOffset = 0;
        private const int HeaderReservedOffset = 4;
        private const int ValuePayloadOffset = 0;
        private const int ShortCommandStatusPayloadOffset = 0;
        private const int ShortCommandErrorPayloadOffset = 2;
        private const int CommandStatusPayloadOffset = 4;
        private const int CommandErrorPayloadOffset = 6;
        private const int LookupReferencePayloadOffset = 4;
        private const int FunctionStatusPayloadOffset = 4;
        private const int FunctionErrorPayloadOffset = 6;
        private const int AxisErrorPayloadOffset = 8;
        private const int AxisStatusWordPayloadOffset = 10;
        private const int GroupErrorPayloadOffset = 8;
        private const int GroupMembersAxisReferencesPayloadOffset = 0;
        private const int GroupMembersDeviceIdsPayloadOffset = 32;
        private const int GroupMembersStatusPayloadOffset = 64;
        private const int GroupMembersErrorPayloadOffset = 66;
        private const int GroupMembersNamesPayloadOffset = 68;
        private const int GroupMembersAxisCountPayloadOffset = 1348;
        private const int UInt16ByteLength = 2;
        private const int UInt32ByteLength = 4;
        private const int ShortAcknowledgementPayloadLength = 4;
        private const int AcknowledgementPayloadLength = 8;
        private const int ReadStatusPayloadLength = 12;
        private const int ReadActualPositionPayloadLength = 8;
        private const int GroupReadStatusPayloadLength = 12;
        private const int GroupMembersInfoPayloadLength = 1350;
        private const int RpcSessionInitPayloadLength = 24;
        private const int MaximumGroupMemberCount = 16;
        private const int GroupMemberNameLength = 80;
        private const int LookupPayloadLength =
            LookupReferencePayloadOffset + UInt16ByteLength;

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
        public LMC_Response RpcCloseResponse { get; private set; }

        public event EventHandler<LMCCallbackEventArgs> CallbackReceived;
        public event EventHandler<LMCCallbackErrorEventArgs> CallbackListenerError;

        public void RpcInitConnection(
            string remoteAddress,
            int remotePort,
            string localAddress)
        {
            OpenRpcConnection(
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
            OpenRpcConnection(
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

        private void OpenRpcConnection(
            string remoteAddress,
            int remotePort,
            string localAddress,
            int callbackPort,
            uint eventMask)
        {
            CloseConnection(true);
            RpcCloseResponse = null;

            if (callbackPort < 0 || callbackPort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    "callbackPort",
                    "Callback port must be between 0 and 65535.");
            }

            var parsedRemoteAddress = ParseIPv4Address(remoteAddress, "remoteAddress");
            var parsedLocalAddress = ParseIPv4Address(localAddress, "localAddress");
            var localEndPoint = new IPEndPoint(parsedLocalAddress, 0);

            client = new TcpClient(localEndPoint)
            {
                NoDelay = true,
                ReceiveTimeout = ReceiveTimeoutMilliseconds,
                SendTimeout = SendTimeoutMilliseconds
            };

            try
            {
                client.Connect(parsedRemoteAddress, remotePort);

                RpcSessionInitResponse = Parse(
                    Exchange(LMC_Frame.RpcSessionInit()));
                EnsureSuccess("RPC session init", RpcSessionInitResponse);
                EnsureExactPayloadLength(
                    RpcSessionInitResponse,
                    RpcSessionInitPayloadLength,
                    "RPC session init");

                StartCallbackListener(parsedLocalAddress, callbackPort);
                var registeredCallbackPort = CallbackLocalEndPoint.Port;

                RpcCallbackRegistrationResponse = ParseShortAcknowledgement(
                    Exchange(
                        LMC_Frame.RpcCallbackRegistration(
                            eventMask,
                            registeredCallbackPort,
                            parsedLocalAddress.GetAddressBytes())),
                    "RPC callback registration");
                EnsureSuccess("RPC callback registration", RpcCallbackRegistrationResponse);

                CallbackPort = registeredCallbackPort;
                EventMask = eventMask;
                IsRpcInitialized = true;
            }
            catch
            {
                CloseConnection(false);
                throw;
            }
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
            var safeRaw = raw ?? new byte[0];
            var response = new LMC_Response
            {
                Raw = safeRaw,
                Payload = new byte[0]
            };

            if (safeRaw.Length < LMC_Frame.HeaderSize)
            {
                return response;
            }

            response.HeaderStatus = LMC_Frame.ReadUInt16(safeRaw, HeaderStatusOffset);
            response.PayloadLength = LMC_Frame.GetResponsePayloadLength(safeRaw);
            response.HeaderReserved = LMC_Frame.ReadUInt32(safeRaw, HeaderReservedOffset);

            var expectedLength = LMC_Frame.HeaderSize + response.PayloadLength;
            response.IsFrameValid = safeRaw.Length == expectedLength;

            if (safeRaw.Length >= expectedLength && response.PayloadLength > 0)
            {
                response.Payload = new byte[response.PayloadLength];
                Buffer.BlockCopy(
                    safeRaw,
                    LMC_Frame.HeaderSize,
                    response.Payload,
                    0,
                    response.PayloadLength);
            }

            return response;
        }

        internal static LMC_Response ParseAcknowledgement(byte[] raw)
        {
            var response = Parse(raw);

            if (response.Payload.Length == ShortAcknowledgementPayloadLength)
            {
                response.CommandStatus =
                    LMC_Frame.ReadUInt16(response.Payload, ShortCommandStatusPayloadOffset);
                response.ErrorId = unchecked(
                    (short)LMC_Frame.ReadUInt16(
                        response.Payload,
                        ShortCommandErrorPayloadOffset));
                response.HasCommandResult = true;
            }
            else if (response.Payload.Length == AcknowledgementPayloadLength)
            {
                response.CommandStatus =
                    LMC_Frame.ReadUInt16(response.Payload, CommandStatusPayloadOffset);
                response.ErrorId = unchecked(
                    (short)LMC_Frame.ReadUInt16(response.Payload, CommandErrorPayloadOffset));
                response.HasCommandResult = true;
            }

            return response;
        }

        internal static bool TryParseLookupReference(
            byte[] raw,
            out LMC_Response response,
            out ushort reference)
        {
            reference = 0;
            response = Parse(raw);

            if (!response.IsSuccess
                || response.Payload.Length != LookupPayloadLength)
            {
                return false;
            }

            reference = LMC_Frame.ReadUInt16(
                response.Payload,
                LookupReferencePayloadOffset);
            return reference != 0;
        }

        internal static uint ParseUInt32Value(byte[] raw, out LMC_Response response)
        {
            response = Parse(raw);

            if (!response.IsSuccess || response.Payload.Length < UInt32ByteLength)
            {
                return 0;
            }

            return LMC_Frame.ReadUInt32(response.Payload, ValuePayloadOffset);
        }

        internal static int ParseInt32Value(byte[] raw, out LMC_Response response)
        {
            response = Parse(raw);

            if (!response.IsSuccess || response.Payload.Length < UInt32ByteLength)
            {
                return 0;
            }

            return LMC_Frame.ReadInt32(response.Payload, ValuePayloadOffset);
        }

        internal static LMCReadStatusResult ParseReadStatusResult(byte[] raw)
        {
            bool isShortError;
            var response = ParseTypedResponse(
                raw,
                ReadStatusPayloadLength,
                "ReadStatus",
                out isShortError);

            if (isShortError)
            {
                return new LMCReadStatusResult(
                    response,
                    0,
                    response.CommandStatus,
                    response.ErrorId,
                    0,
                    0);
            }

            var functionStatus = LMC_Frame.ReadUInt16(
                response.Payload,
                FunctionStatusPayloadOffset);
            var errorId = ReadInt16(response.Payload, FunctionErrorPayloadOffset);

            SetFunctionResult(response, functionStatus, errorId);

            return new LMCReadStatusResult(
                response,
                LMC_Frame.ReadUInt32(response.Payload, ValuePayloadOffset),
                functionStatus,
                errorId,
                LMC_Frame.ReadUInt16(response.Payload, AxisErrorPayloadOffset),
                LMC_Frame.ReadUInt16(response.Payload, AxisStatusWordPayloadOffset));
        }

        internal static LMCReadActualPositionResult ParseReadActualPositionResult(byte[] raw)
        {
            bool isShortError;
            var response = ParseTypedResponse(
                raw,
                ReadActualPositionPayloadLength,
                "ReadActualPosition",
                out isShortError);

            if (isShortError)
            {
                return new LMCReadActualPositionResult(
                    response,
                    0,
                    response.CommandStatus,
                    response.ErrorId);
            }

            var functionStatus = LMC_Frame.ReadUInt16(
                response.Payload,
                FunctionStatusPayloadOffset);
            var errorId = ReadInt16(response.Payload, FunctionErrorPayloadOffset);

            SetFunctionResult(response, functionStatus, errorId);

            return new LMCReadActualPositionResult(
                response,
                LMC_Frame.ReadInt32(response.Payload, ValuePayloadOffset),
                functionStatus,
                errorId);
        }

        internal static LMCGroupReadStatusResult ParseGroupReadStatusResult(byte[] raw)
        {
            bool isShortError;
            var response = ParseTypedResponse(
                raw,
                GroupReadStatusPayloadLength,
                "GroupReadStatus",
                out isShortError);

            if (isShortError)
            {
                return new LMCGroupReadStatusResult(
                    response,
                    0,
                    response.CommandStatus,
                    response.ErrorId,
                    0);
            }

            var functionStatus = LMC_Frame.ReadUInt16(
                response.Payload,
                FunctionStatusPayloadOffset);
            var errorId = ReadInt16(response.Payload, FunctionErrorPayloadOffset);

            SetFunctionResult(response, functionStatus, errorId);

            return new LMCGroupReadStatusResult(
                response,
                LMC_Frame.ReadUInt32(response.Payload, ValuePayloadOffset),
                functionStatus,
                errorId,
                LMC_Frame.ReadUInt16(response.Payload, GroupErrorPayloadOffset));
        }

        internal static LMCGroupMembersInfoResult ParseGroupMembersInfoResult(byte[] raw)
        {
            bool isShortError;
            var response = ParseTypedResponse(
                raw,
                GroupMembersInfoPayloadLength,
                "GetGroupMembersInfo",
                out isShortError);

            if (isShortError)
            {
                return new LMCGroupMembersInfoResult(
                    response,
                    new ushort[MaximumGroupMemberCount],
                    new ushort[MaximumGroupMemberCount],
                    CreateEmptyGroupMemberNames(),
                    0,
                    response.CommandStatus,
                    response.ErrorId);
            }

            var axisCount = response.Payload[GroupMembersAxisCountPayloadOffset];

            if (axisCount > MaximumGroupMemberCount)
            {
                throw new InvalidDataException(
                    "GetGroupMembersInfo response contains an axis count greater than 16.");
            }

            var axisReferences = new ushort[MaximumGroupMemberCount];
            var deviceIds = new ushort[MaximumGroupMemberCount];
            var axisNames = new string[MaximumGroupMemberCount];

            for (var index = 0; index < MaximumGroupMemberCount; index++)
            {
                axisReferences[index] = LMC_Frame.ReadUInt16(
                    response.Payload,
                    GroupMembersAxisReferencesPayloadOffset + index * UInt16ByteLength);
                deviceIds[index] = LMC_Frame.ReadUInt16(
                    response.Payload,
                    GroupMembersDeviceIdsPayloadOffset + index * UInt16ByteLength);
                axisNames[index] = ReadFixedAsciiString(
                    response.Payload,
                    GroupMembersNamesPayloadOffset + index * GroupMemberNameLength,
                    GroupMemberNameLength);
            }

            var functionStatus = LMC_Frame.ReadUInt16(
                response.Payload,
                GroupMembersStatusPayloadOffset);
            var errorId = ReadInt16(response.Payload, GroupMembersErrorPayloadOffset);

            SetFunctionResult(response, functionStatus, errorId);

            return new LMCGroupMembersInfoResult(
                response,
                axisReferences,
                deviceIds,
                axisNames,
                axisCount,
                functionStatus,
                errorId);
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
                    RpcCloseResponse = ParseShortAcknowledgement(
                        Exchange(LMC_Frame.CloseConnection()),
                        "RPC close");
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

        private static IPAddress ParseIPv4Address(string value, string parameterName)
        {
            var address = IPAddress.Parse(value);

            if (address.AddressFamily != AddressFamily.InterNetwork
                || address.Equals(IPAddress.Any)
                || address.Equals(IPAddress.Broadcast)
                || address.Equals(IPAddress.None))
            {
                throw new ArgumentException(
                    "A concrete IPv4 address is required.",
                    parameterName);
            }

            return address;
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

        private static LMC_Response ParseTypedResponse(
            byte[] raw,
            int expectedPayloadLength,
            string operation,
            out bool isShortError)
        {
            var response = ParseAcknowledgement(raw);
            isShortError = response.IsFrameValid
                && response.PayloadLength == ShortAcknowledgementPayloadLength
                && response.HasCommandResult
                && !response.IsSuccess;

            if (isShortError)
            {
                return response;
            }

            EnsureExactPayloadLength(
                response,
                expectedPayloadLength,
                operation);

            return response;
        }

        private static string[] CreateEmptyGroupMemberNames()
        {
            var names = new string[MaximumGroupMemberCount];

            for (var index = 0; index < names.Length; index++)
            {
                names[index] = string.Empty;
            }

            return names;
        }

        private static void EnsureExactPayloadLength(
            LMC_Response response,
            int expectedPayloadLength,
            string operation)
        {
            if (response == null
                || !response.IsFrameValid
                || response.PayloadLength != expectedPayloadLength
                || response.Payload.Length != expectedPayloadLength)
            {
                throw new InvalidDataException(
                    operation
                    + " response must contain exactly "
                    + expectedPayloadLength
                    + " payload bytes.");
            }
        }

        private static LMC_Response ParseShortAcknowledgement(
            byte[] raw,
            string operation)
        {
            var response = ParseAcknowledgement(raw);

            if (!response.IsFrameValid
                || response.PayloadLength != ShortAcknowledgementPayloadLength
                || !response.HasCommandResult)
            {
                throw new InvalidDataException(
                    operation + " response must contain exactly 4 payload bytes.");
            }

            return response;
        }

        private static void SetFunctionResult(
            LMC_Response response,
            ushort functionStatus,
            short errorId)
        {
            response.CommandStatus = functionStatus;
            response.ErrorId = errorId;
            response.HasCommandResult = true;
            response.CommandStatusIsBitField = true;
        }

        private static short ReadInt16(byte[] buffer, int offset)
        {
            return unchecked((short)LMC_Frame.ReadUInt16(buffer, offset));
        }

        private static string ReadFixedAsciiString(
            byte[] buffer,
            int offset,
            int length)
        {
            var stringLength = 0;

            while (stringLength < length && buffer[offset + stringLength] != 0)
            {
                stringLength++;
            }

            return Encoding.ASCII.GetString(buffer, offset, stringLength);
        }
    }
}

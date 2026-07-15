using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed class LMCConnection : IDisposable
    {
        public const int DefaultCallbackPort = 5003;
        public const uint DefaultEventMask = 0xFFFFFFFF;

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
        private const int GroupPositionsCount = 16;
        private const int GroupPositionsStatusPayloadOffset = 64;
        private const int GroupPositionsErrorPayloadOffset = 66;
        private const int UInt16ByteLength = 2;
        private const int UInt32ByteLength = 4;
        private const int ShortAcknowledgementPayloadLength = 4;
        private const int AcknowledgementPayloadLength = 8;
        private const int ReadStatusPayloadLength = 12;
        private const int ReadActualPositionPayloadLength = 8;
        private const int GroupReadStatusPayloadLength = 12;
        private const int GroupReadActualPositionPayloadLength = 68;
        private const int GroupMembersInfoPayloadLength = 1350;
        private const int RpcSessionInitPayloadLength = 24;
        private const int MaximumGroupMemberCount = 16;
        private const int GroupMemberNameLength = 80;
        private const int LookupDiagnosticRawByteLimit = 128;
        private const int LookupPayloadLength =
            LookupReferencePayloadOffset + UInt16ByteLength;
        private const long AnySessionGeneration = -1;

        private readonly object sync = new object();
        private readonly object lifecycleSync = new object();
        private readonly object callbackSync = new object();
        private readonly LMCConnectionOptions options;
        private TcpClient client;
        private UdpClient callbackListener;
        private Thread callbackThread;
        private IPAddress expectedCallbackAddress;
        private volatile bool callbackListenerRunning;
        private int connectionState;
        private long rejectedCallbackCount;
        private long sessionGeneration;

        public LMCConnection()
            : this(new LMCConnectionOptions())
        {
        }

        public LMCConnection(LMCConnectionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.options = options.CloneAndValidate();
            connectionState = (int)LMCConnectionState.Disconnected;
        }

        public bool IsRpcInitialized { get; private set; }
        public bool IsConnected
        {
            get { return State == LMCConnectionState.Connected; }
        }

        public LMCConnectionState State
        {
            get { return (LMCConnectionState)Volatile.Read(ref connectionState); }
        }

        public LMCConnectionOptions Options
        {
            get { return options.CloneAndValidate(); }
        }

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
        public Exception LastTransportException { get; private set; }
        public Exception LastInitializationException { get; private set; }
        public Exception LastCloseException { get; private set; }
        public long RejectedCallbackCount
        {
            get { return Interlocked.Read(ref rejectedCallbackCount); }
        }

        public event EventHandler<LMCCallbackEventArgs> CallbackReceived;
        public event EventHandler<LMCCallbackErrorEventArgs> CallbackListenerError;
        public event EventHandler<LMCConnectionStateChangedEventArgs>
            ConnectionStateChanged;

        internal long SessionGeneration
        {
            get { return Interlocked.Read(ref sessionGeneration); }
        }

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

        public Task RpcInitConnectionAsync(
            string remoteAddress,
            int remotePort,
            string localAddress,
            CancellationToken cancellationToken)
        {
            return Task.Run(
                () => OpenRpcConnection(
                    remoteAddress,
                    remotePort,
                    localAddress,
                    DefaultCallbackPort,
                    DefaultEventMask,
                    cancellationToken));
        }

        public Task RpcInitConnectionAsync(
            string remoteAddress,
            int remotePort,
            string localAddress,
            int callbackPort,
            uint eventMask,
            CancellationToken cancellationToken)
        {
            return Task.Run(
                () => OpenRpcConnection(
                    remoteAddress,
                    remotePort,
                    localAddress,
                    callbackPort,
                    eventMask,
                    cancellationToken));
        }

        public void CloseConnection()
        {
            CloseConnectionCore(true, true);
        }

        public Task CloseConnectionAsync(CancellationToken cancellationToken)
        {
            return Task.Run(
                () => CloseConnectionCore(
                    true,
                    true,
                    cancellationToken));
        }

        private void OpenRpcConnection(
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
                eventMask,
                CancellationToken.None);
        }

        private void OpenRpcConnection(
            string remoteAddress,
            int remotePort,
            string localAddress,
            int callbackPort,
            uint eventMask,
            CancellationToken cancellationToken)
        {
            EnterGate(lifecycleSync, cancellationToken);

            try
            {
                OpenRpcConnectionLocked(
                    remoteAddress,
                    remotePort,
                    localAddress,
                    callbackPort,
                    eventMask,
                    cancellationToken);
            }
            finally
            {
                Monitor.Exit(lifecycleSync);
            }
        }

        private void OpenRpcConnectionLocked(
            string remoteAddress,
            int remotePort,
            string localAddress,
            int callbackPort,
            uint eventMask,
            CancellationToken cancellationToken)
        {
            if (callbackPort < 0 || callbackPort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    "callbackPort",
                    "Callback port must be between 0 and 65535.");
            }

            if (remotePort < 1 || remotePort > 65535)
            {
                throw new ArgumentOutOfRangeException(
                    "remotePort",
                    "Remote port must be between 1 and 65535.");
            }

            var parsedRemoteAddress = ParseIPv4Address(remoteAddress, "remoteAddress");
            var parsedLocalAddress = ParseIPv4Address(localAddress, "localAddress");

            cancellationToken.ThrowIfCancellationRequested();
            CloseConnectionCoreLocked(true, false, CancellationToken.None);
            cancellationToken.ThrowIfCancellationRequested();

            RpcCloseResponse = null;
            LastTransportException = null;
            LastInitializationException = null;
            LastCloseException = null;
            Interlocked.Exchange(ref rejectedCallbackCount, 0);
            var localEndPoint = new IPEndPoint(parsedLocalAddress, 0);
            expectedCallbackAddress = parsedRemoteAddress;
            SetConnectionState(LMCConnectionState.Connecting, null);

            var openingClient = new TcpClient(localEndPoint)
            {
                NoDelay = true,
                ReceiveTimeout = options.ReceiveTimeoutMilliseconds,
                SendTimeout = options.SendTimeoutMilliseconds
            };
            client = openingClient;
            Interlocked.Increment(ref sessionGeneration);

            try
            {
                using (cancellationToken.Register(
                    () => AbortForCancellation(openingClient)))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ConnectWithTimeout(openingClient, parsedRemoteAddress, remotePort);
                    cancellationToken.ThrowIfCancellationRequested();

                    RpcSessionInitResponse = Parse(
                        ExchangeCore(
                            LMC_Frame.RpcSessionInit(),
                            openingClient,
                            true,
                            cancellationToken,
                            AnySessionGeneration));
                    EnsureSuccess("RPC session init", RpcSessionInitResponse);
                    EnsureExactPayloadLength(
                        RpcSessionInitResponse,
                        RpcSessionInitPayloadLength,
                        "RPC session init");

                    StartCallbackListener(parsedLocalAddress, callbackPort);
                    var registeredCallbackPort = CallbackLocalEndPoint.Port;

                    RpcCallbackRegistrationResponse = ParseShortAcknowledgement(
                        ExchangeCore(
                            LMC_Frame.RpcCallbackRegistration(
                                eventMask,
                                registeredCallbackPort,
                                parsedLocalAddress.GetAddressBytes()),
                            openingClient,
                            true,
                            cancellationToken,
                            AnySessionGeneration),
                        "RPC callback registration");
                    EnsureSuccess(
                        "RPC callback registration",
                        RpcCallbackRegistrationResponse);

                    CallbackPort = registeredCallbackPort;
                    EventMask = eventMask;
                }

                IsRpcInitialized = true;
                SetConnectionState(LMCConnectionState.Connected, null);
            }
            catch (Exception ex)
            {
                var failure = cancellationToken.IsCancellationRequested
                    ? (Exception)new OperationCanceledException(cancellationToken)
                    : ex;

                LastInitializationException = failure;
                if (!cancellationToken.IsCancellationRequested
                    && IsTransportException(ex))
                {
                    LastTransportException = ex;
                }

                CloseConnectionCoreLocked(false, false, CancellationToken.None);
                SetConnectionState(
                    cancellationToken.IsCancellationRequested
                        ? LMCConnectionState.Disconnected
                        : LMCConnectionState.Faulted,
                    failure);

                if (!ReferenceEquals(failure, ex))
                {
                    throw failure;
                }

                throw;
            }
        }

        internal byte[] Exchange(byte[] request)
        {
            return ExchangeCore(
                request,
                client,
                false,
                CancellationToken.None,
                AnySessionGeneration);
        }

        internal byte[] Exchange(byte[] request, long expectedGeneration)
        {
            return ExchangeCore(
                request,
                client,
                false,
                CancellationToken.None,
                expectedGeneration);
        }

        private byte[] ExchangeCore(
            byte[] request,
            TcpClient operationClient,
            bool allowLifecycleState,
            CancellationToken cancellationToken,
            long expectedGeneration)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            EnterGate(sync, cancellationToken);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureConnected(operationClient, allowLifecycleState);
                EnsureExpectedGeneration(expectedGeneration);

                using (cancellationToken.Register(
                    () => AbortForCancellation(operationClient)))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var stream = operationClient.GetStream();
                    stream.Write(request, 0, request.Length);

                    var header = ReadExact(stream, LMC_Frame.HeaderSize);
                    var payloadLength = LMC_Frame.GetResponsePayloadLength(header);
                    var payload = payloadLength == 0
                        ? new byte[0]
                        : ReadExact(stream, payloadLength);

                    return CombineResponse(header, payload);
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancellationToken);
                }

                if (IsTransportException(ex))
                {
                    MarkTransportFault(ex, operationClient);
                }

                throw;
            }
            finally
            {
                Monitor.Exit(sync);
            }
        }

        internal Task<byte[]> ExchangeAsync(
            byte[] request,
            CancellationToken cancellationToken)
        {
            return ExchangeAsync(
                request,
                AnySessionGeneration,
                cancellationToken);
        }

        internal Task<byte[]> ExchangeAsync(
            byte[] request,
            long expectedGeneration,
            CancellationToken cancellationToken)
        {
            var operationClient = client;

            return Task.Run(
                () => ExchangeCore(
                    request,
                    operationClient,
                    false,
                    cancellationToken,
                    expectedGeneration));
        }

        internal void EnsureSessionGeneration(long expectedGeneration)
        {
            if (State != LMCConnectionState.Connected
                || expectedGeneration != SessionGeneration)
            {
                throw new InvalidOperationException(
                    "The axis or group handle belongs to an inactive RPC session; create it again after reconnecting.");
            }
        }

        private void EnsureExpectedGeneration(long expectedGeneration)
        {
            if (expectedGeneration != AnySessionGeneration
                && expectedGeneration != SessionGeneration)
            {
                throw new InvalidOperationException(
                    "The axis or group handle belongs to an inactive RPC session; create it again after reconnecting.");
            }
        }

        /*
         * All RPC requests are serialized by sync.  A queued async cancellation
         * is checked before it owns the gate, so it cannot abort another request.
         * Once bytes may have been sent, cancellation invalidates this transport
         * because the command outcome and stream position are no longer known.
         */

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

        internal static LMC_Response ParseCommandAcknowledgement(
            byte[] raw,
            string operation)
        {
            var response = ParseAcknowledgement(raw);

            if (!response.IsFrameValid
                || !response.HasCommandResult
                || (response.PayloadLength != ShortAcknowledgementPayloadLength
                    && response.PayloadLength != AcknowledgementPayloadLength))
            {
                throw new InvalidDataException(
                    operation
                    + " response must contain exactly 4 or 8 acknowledgement payload bytes.");
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

        internal static InvalidOperationException CreateLookupFailureException(
            string targetKind,
            string objectName,
            byte[] raw)
        {
            var response = ParseAcknowledgement(raw);
            var message = new StringBuilder();
            var rawLength = raw == null ? 0 : raw.Length;
            var hasLookupPayload = response.Payload.Length == LookupPayloadLength;
            var lookupReference = hasLookupPayload
                ? LMC_Frame.ReadUInt16(
                    response.Payload,
                    LookupReferencePayloadOffset)
                : (ushort)0;

            message.Append(targetKind)
                .Append(" lookup failed for '")
                .Append(objectName)
                .Append("'. FrameValid=")
                .Append(response.IsFrameValid)
                .Append(", HeaderStatus=")
                .Append(response.HeaderStatus)
                .Append(", PayloadLength=")
                .Append(response.PayloadLength)
                .Append(", ParsedPayloadLength=")
                .Append(response.Payload.Length)
                .Append(", RawLength=")
                .Append(rawLength);

            if (response.HasCommandResult)
            {
                message.Append(", CommandStatus=")
                    .Append(response.CommandStatus)
                    .Append(", ErrorId=")
                    .Append(response.ErrorId);
            }

            if (hasLookupPayload)
            {
                message.Append(", Reference=")
                    .Append(lookupReference);
            }
            else
            {
                message.Append(", ExpectedPayloadLength=")
                    .Append(LookupPayloadLength);
            }

            message.Append(". ");
            if (response.IsFrameValid
                && response.HeaderStatus != 0
                && response.HasCommandResult
                && response.PayloadLength == ShortAcknowledgementPayloadLength
                && response.ErrorId == -2)
            {
                message.Append(
                    "The LASAL object registry entry is not ready or the object name did not match. ");
            }
            else if (response.IsFrameValid
                && response.HeaderStatus == 0
                && hasLookupPayload
                && lookupReference == 0)
            {
                message.Append(
                    "A zero descriptor is not valid for the LASAL dispatcher. ");
            }

            message.Append("Raw=")
                .Append(ToHex(raw))
                .Append(".");

            return new InvalidOperationException(message.ToString());
        }

        private static string ToHex(byte[] raw)
        {
            if (raw == null || raw.Length == 0)
            {
                return "<empty>";
            }

            var byteCount = Math.Min(raw.Length, LookupDiagnosticRawByteLimit);
            var hex = BitConverter.ToString(raw, 0, byteCount).Replace("-", " ");
            return byteCount == raw.Length ? hex : hex + " ...";
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

        internal static LMCGroupReadActualPositionResult
            ParseGroupReadActualPositionResult(
                byte[] raw,
                LMC_COORD_SYSTEM coordinateSystem)
        {
            bool isShortError;
            var response = ParseTypedResponse(
                raw,
                GroupReadActualPositionPayloadLength,
                "GroupReadActualPosition",
                out isShortError);

            if (isShortError)
            {
                return new LMCGroupReadActualPositionResult(
                    response,
                    coordinateSystem,
                    new int[GroupPositionsCount],
                    response.CommandStatus,
                    response.ErrorId);
            }

            var positions = new int[GroupPositionsCount];
            for (var index = 0; index < positions.Length; index++)
            {
                positions[index] = LMC_Frame.ReadInt32(
                    response.Payload,
                    index * UInt32ByteLength);
            }

            var functionStatus = LMC_Frame.ReadUInt16(
                response.Payload,
                GroupPositionsStatusPayloadOffset);
            var errorId = ReadInt16(
                response.Payload,
                GroupPositionsErrorPayloadOffset);

            SetFunctionResult(response, functionStatus, errorId);

            return new LMCGroupReadActualPositionResult(
                response,
                coordinateSystem,
                positions,
                functionStatus,
                errorId);
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
            CloseConnectionCore(true, false);
        }

        private void CloseConnectionCore(
            bool sendCloseCommand,
            bool throwOnCloseError)
        {
            CloseConnectionCore(
                sendCloseCommand,
                throwOnCloseError,
                CancellationToken.None);
        }

        private void CloseConnectionCore(
            bool sendCloseCommand,
            bool throwOnCloseError,
            CancellationToken cancellationToken)
        {
            EnterGate(lifecycleSync, cancellationToken);

            try
            {
                CloseConnectionCoreLocked(
                    sendCloseCommand,
                    throwOnCloseError,
                    cancellationToken);
            }
            finally
            {
                Monitor.Exit(lifecycleSync);
            }
        }

        private void CloseConnectionCoreLocked(
            bool sendCloseCommand,
            bool throwOnCloseError,
            CancellationToken cancellationToken)
        {
            var currentClient = client;
            Exception closeException = null;

            if (currentClient != null)
            {
                LastCloseException = null;
                SetConnectionState(LMCConnectionState.Closing, null);
            }

            try
            {
                if (sendCloseCommand && currentClient != null && currentClient.Connected)
                {
                    RpcCloseResponse = ParseShortAcknowledgement(
                        ExchangeCore(
                            LMC_Frame.CloseConnection(),
                            currentClient,
                            true,
                            cancellationToken,
                            AnySessionGeneration),
                        "RPC close");
                    EnsureSuccess("RPC close", RpcCloseResponse);
                }
            }
            catch (Exception ex)
            {
                closeException = ex;
                LastCloseException = ex;
            }
            finally
            {
                if (currentClient != null)
                {
                    Interlocked.CompareExchange(
                        ref client,
                        null,
                        currentClient);
                    currentClient.Close();
                }
                StopCallbackListener();
                IsRpcInitialized = false;
                CallbackPort = 0;
                EventMask = 0;
                expectedCallbackAddress = null;
                RpcSessionInitResponse = null;
                RpcCallbackRegistrationResponse = null;
                SetConnectionState(LMCConnectionState.Disconnected, null);
            }

            if (throwOnCloseError && closeException != null)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(
                        "RPC close was cancelled; the local transport was closed.",
                        closeException,
                        cancellationToken);
                }

                throw new IOException(
                    "LMC close acknowledgement failed; the local transport was closed.",
                    closeException);
            }
        }

        private void EnsureConnected(
            TcpClient operationClient,
            bool allowLifecycleState)
        {
            if (operationClient == null
                || !ReferenceEquals(client, operationClient)
                || !operationClient.Connected)
            {
                throw new InvalidOperationException("LMC connection is not open.");
            }

            if (!allowLifecycleState && State != LMCConnectionState.Connected)
            {
                throw new InvalidOperationException(
                    "LMC connection is not available in state " + State + ".");
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
            var callbackSourceAddress = expectedCallbackAddress;
            var thread = new Thread(
                () => ReceiveCallbackLoop(listener, callbackSourceAddress))
            {
                IsBackground = true,
                Name = "LMC RPC callback listener"
            };

            lock (callbackSync)
            {
                callbackListener = listener;
                CallbackLocalEndPoint = (IPEndPoint)listener.Client.LocalEndPoint;
                callbackListenerRunning = true;
                callbackThread = thread;
            }

            thread.Start();
        }

        private void StopCallbackListener()
        {
            UdpClient listener;
            Thread thread;

            lock (callbackSync)
            {
                callbackListenerRunning = false;
                listener = callbackListener;
                thread = callbackThread;
                callbackListener = null;
                callbackThread = null;
                CallbackLocalEndPoint = null;
            }

            if (listener != null)
            {
                listener.Close();
            }

            if (thread != null
                && thread.IsAlive
                && Thread.CurrentThread.ManagedThreadId != thread.ManagedThreadId)
            {
                thread.Join(options.CallbackThreadJoinTimeoutMilliseconds);
            }
        }

        private void ReceiveCallbackLoop(
            UdpClient ownedListener,
            IPAddress ownedSourceAddress)
        {
            while (IsCurrentCallbackListener(ownedListener))
            {
                try
                {
                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    var payload = ownedListener.Receive(ref remoteEndPoint);

                    if (!IsCurrentCallbackListener(ownedListener))
                    {
                        break;
                    }

                    if (options.ValidateCallbackSourceAddress
                        && ownedSourceAddress != null
                        && !ownedSourceAddress.Equals(remoteEndPoint.Address))
                    {
                        Interlocked.Increment(ref rejectedCallbackCount);
                        continue;
                    }

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
                    if (IsCurrentCallbackListener(ownedListener))
                    {
                        OnCallbackListenerError(new LMCCallbackErrorEventArgs(ex));
                    }
                }
                catch (Exception ex)
                {
                    if (IsCurrentCallbackListener(ownedListener))
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

        private bool IsCurrentCallbackListener(UdpClient ownedListener)
        {
            return callbackListenerRunning
                && ReferenceEquals(callbackListener, ownedListener);
        }

        internal static LMC_Response ParseShortAcknowledgement(
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

        private void ConnectWithTimeout(
            TcpClient targetClient,
            IPAddress remoteAddress,
            int remotePort)
        {
            var asyncResult = targetClient.BeginConnect(
                remoteAddress,
                remotePort,
                null,
                null);

            try
            {
                if (!asyncResult.AsyncWaitHandle.WaitOne(
                    options.ConnectTimeoutMilliseconds))
                {
                    targetClient.Close();
                    throw new TimeoutException(
                        "LMC TCP connection timed out after "
                        + options.ConnectTimeoutMilliseconds
                        + " ms.");
                }

                targetClient.EndConnect(asyncResult);
            }
            finally
            {
                asyncResult.AsyncWaitHandle.Close();
            }
        }

        private void MarkTransportFault(
            Exception exception,
            TcpClient operationClient)
        {
            if (!TryDetachClient(operationClient))
            {
                return;
            }

            LastTransportException = exception;
            CloseClientQuietly(operationClient);
            InvalidateDetachedTransport(exception);
        }

        private void AbortForCancellation(TcpClient operationClient)
        {
            if (!TryDetachClient(operationClient))
            {
                return;
            }

            var exception = new OperationCanceledException(
                "The RPC operation was cancelled after it acquired the transport. Reconnect before issuing another command.");

            CloseClientQuietly(operationClient);
            InvalidateDetachedTransport(exception);
        }

        private bool TryDetachClient(TcpClient operationClient)
        {
            if (operationClient == null)
            {
                return false;
            }

            return ReferenceEquals(
                Interlocked.CompareExchange(
                    ref client,
                    null,
                    operationClient),
                operationClient);
        }

        private void InvalidateDetachedTransport(Exception exception)
        {
            IsRpcInitialized = false;
            CallbackPort = 0;
            EventMask = 0;
            expectedCallbackAddress = null;
            RpcSessionInitResponse = null;
            RpcCallbackRegistrationResponse = null;
            StopCallbackListener();

            if (State != LMCConnectionState.Closing)
            {
                SetConnectionState(LMCConnectionState.Faulted, exception);
            }
        }

        private static void CloseClientQuietly(TcpClient targetClient)
        {
            if (targetClient == null)
            {
                return;
            }

            try
            {
                targetClient.Close();
            }
            catch
            {
            }
        }

        private static bool IsTransportException(Exception exception)
        {
            return exception is IOException
                || exception is SocketException
                || exception is ObjectDisposedException
                || exception is TimeoutException;
        }

        private void SetConnectionState(
            LMCConnectionState state,
            Exception exception)
        {
            var previous = (LMCConnectionState)Interlocked.Exchange(
                ref connectionState,
                (int)state);

            if (previous == state)
            {
                return;
            }

            var handler = ConnectionStateChanged;
            if (handler == null)
            {
                return;
            }

            try
            {
                handler(
                    this,
                    new LMCConnectionStateChangedEventArgs(
                        previous,
                        state,
                        exception));
            }
            catch
            {
            }
        }

        private static void EnterGate(
            object gate,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Monitor.TryEnter(gate, 50))
                {
                    return;
                }
            }
        }
    }
}

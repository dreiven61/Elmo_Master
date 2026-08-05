using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    internal sealed class LMCPostWriteDeadlineException : TimeoutException
    {
        internal LMCPostWriteDeadlineException(Exception innerException)
            : base(
                "The RPC response deadline expired after the write-commit boundary; the transport was invalidated to preserve stream alignment.",
                innerException)
        {
        }
    }

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
        private const long AnyConnectionLifetimeGeneration = -1;

        private static readonly AsyncLocal<ConnectionStateEventScope>
            activeConnectionStateEventScope =
                new AsyncLocal<ConnectionStateEventScope>();

        private readonly object sync = new object();
        private readonly object lifecycleSync = new object();
        private readonly object lifecycleStateSync = new object();
        private readonly object callbackSync = new object();
        private readonly object groupEnableWaitRegistrySync = new object();
        private readonly object axisPowerOnWaitRegistrySync = new object();
        private readonly Dictionary<ushort, LMCGroupEnableWaitCoordinator>
            groupEnableWaitCoordinators =
                new Dictionary<ushort, LMCGroupEnableWaitCoordinator>();
        private readonly Dictionary<ushort, LMCAxisPowerOnWaitCoordinator>
            axisPowerOnWaitCoordinators =
                new Dictionary<ushort, LMCAxisPowerOnWaitCoordinator>();
        private readonly LMCConnectionOptions options;
        private readonly LMCSendPriorityCoordinator sendPriorityCoordinator;
        private TcpClient client;
        private UdpClient callbackListener;
        private Thread callbackThread;
        private IPAddress expectedCallbackAddress;
        private volatile bool callbackListenerRunning;
        private int connectionState;
        private long rejectedCallbackCount;
        private long sessionGeneration;
        private long connectionLifetimeGeneration;
        private long clientLifetimeGeneration;
        private long clientSessionGeneration;
        private long callbackListenerLifetimeGeneration;
        private long groupEnableWaitRegistryGeneration = long.MinValue;
        private long axisPowerOnWaitRegistryGeneration = long.MinValue;

        private sealed class ConnectionStateEventScope
        {
            // Task.Run captures this reference. IsActive becomes false only
            // after the synchronous state-event invocation has returned.
            internal ConnectionStateEventScope(
                LMCConnection source,
                ConnectionStateEventScope parent)
            {
                Source = source;
                Parent = parent;
                IsActive = true;
            }

            internal LMCConnection Source { get; private set; }
            internal ConnectionStateEventScope Parent { get; private set; }
            internal volatile bool IsActive;
        }

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
            sendPriorityCoordinator = this.options.SendPriorityCoordinator;
            connectionState = (int)LMCConnectionState.Disconnected;
            Diagnostics = new LMCDiagnostics(this);
            Admin = new LMCAdmin(this);
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
        public LMCDiagnostics Diagnostics { get; private set; }
        public LMCAdmin Admin { get; private set; }
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
            EnsureLifecycleOperationNotReentrant();
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
            EnsureLifecycleOperationNotReentrant();
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
            EnsureLifecycleOperationNotReentrant();
            return Task.Run(
                () => CloseConnectionCore(
                    true,
                    true,
                    cancellationToken));
        }

        /// <summary>
        /// Aborts the local TCP transport with zero-time linger and stops the
        /// callback listener without sending the RPC Close command. This is
        /// intentionally internal and exists only for the WPF qualification
        /// path. A TCP reset still does not prove PLC orphan handling by
        /// itself.
        /// </summary>
        internal void AbortTransportForQualification()
        {
            EnsureLifecycleOperationNotReentrant();
            EnterGate(lifecycleSync, CancellationToken.None);
            try
            {
                var currentClient = client;
                if (currentClient == null || !currentClient.Connected)
                {
                    throw new InvalidOperationException(
                        "The qualification transport is not connected.");
                }

                if (!TryApplyAbortiveLinger(currentClient))
                {
                    throw new InvalidOperationException(
                        "The qualification transport did not retain zero-time linger.");
                }

                LastQualificationAbortUsedZeroLinger = true;
                CloseConnectionCoreLocked(
                    false,
                    false,
                    CancellationToken.None);
            }
            finally
            {
                Monitor.Exit(lifecycleSync);
            }
        }

        /// <summary>
        /// Immediately detaches and closes the currently published TCP client
        /// without sending RPC Close. This is the explicit production escape
        /// hatch for a safety operation that cannot wait behind an in-flight RPC
        /// response. The caller must reconnect this LMCConnection, reacquire the
        /// exact object identity, and issue the safety command exactly once using
        /// a newly bound object. This method never sends that command.
        /// </summary>
        /// <remarks>
        /// A response to an already-written RPC cannot be interleaved with a new
        /// request on the same byte stream. Closing the old client first preserves
        /// framing. Late failure from the detached exchange is isolated by client
        /// identity and connection-lifetime checks and cannot fault a later client.
        /// Zero-time linger is best effort: failure to configure linger is exposed
        /// in the returned evidence but never prevents the local close.
        /// </remarks>
        public LMCSafetyPreemptionAbortEvidence
            AbortTransportForSafetyPreemption()
        {
            return AbortTransportForSafetyPreemptionCore(null);
        }

        /// <summary>
        /// Identity-pinned safety abort. If a reconnect has already replaced the
        /// expected session, this overload throws before detaching any client.
        /// Use the SessionGeneration from the accepted operation being preempted.
        /// </summary>
        public LMCSafetyPreemptionAbortEvidence
            AbortTransportForSafetyPreemption(
            long expectedSessionGeneration)
        {
            if (expectedSessionGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedSessionGeneration",
                    "The expected RPC session generation must be positive.");
            }

            return AbortTransportForSafetyPreemptionCore(
                expectedSessionGeneration);
        }

        private LMCSafetyPreemptionAbortEvidence
            AbortTransportForSafetyPreemptionCore(
            long? expectedSessionGeneration)
        {
            TcpClient currentClient;
            long detachedLifetimeGeneration;
            long sessionGenerationAtAbort;
            LMCConnectionState stateBefore;

            // Intentionally do not acquire lifecycleSync. A normal Close can
            // hold that lock while waiting behind an already-written RPC. The
            // safety path must still be able to detach that exact old client.
            lock (lifecycleStateSync)
            {
                sessionGenerationAtAbort = client == null
                    ? SessionGeneration
                    : clientSessionGeneration;
                if (expectedSessionGeneration.HasValue
                    && sessionGenerationAtAbort
                        != expectedSessionGeneration.Value)
                {
                    throw new
                        LMCSafetyPreemptionSessionMismatchException(
                            expectedSessionGeneration.Value,
                            sessionGenerationAtAbort);
                }

                stateBefore = State;
                currentClient = client;
                if (currentClient == null)
                {
                    return new LMCSafetyPreemptionAbortEvidence(
                        sessionGenerationAtAbort,
                        stateBefore,
                        State,
                        false,
                        false,
                        false);
                }

                detachedLifetimeGeneration = clientLifetimeGeneration;
                client = null;
                clientLifetimeGeneration = 0;
                clientSessionGeneration = 0;
            }

            var abortiveLingerApplied =
                TryApplyAbortiveLinger(currentClient);
            var exception =
                new LMCSafetyPreemptionTransportAbortedException(
                    sessionGenerationAtAbort);

            CloseClientQuietly(currentClient);
            var faultStatePublished = InvalidateDetachedTransport(
                exception,
                detachedLifetimeGeneration,
                true);

            return new LMCSafetyPreemptionAbortEvidence(
                sessionGenerationAtAbort,
                stateBefore,
                State,
                true,
                abortiveLingerApplied,
                faultStatePublished);
        }

        internal bool LastQualificationAbortUsedZeroLinger
        {
            get;
            private set;
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
            EnsureLifecycleOperationNotReentrant();
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
            // Reserve the new session before the previous transport is closed.
            // While that client remains published, pinned abort compares its
            // bound session. Once it is detached, the reservation makes an old
            // generation fail closed even before the new client is published.
            var openingSessionGeneration = ReserveOpeningSession();
            var reservedObserver = options
                .SessionReservedBeforeClientPublishObserver;
            if (reservedObserver != null)
            {
                reservedObserver();
            }

            CloseConnectionCoreLocked(true, false, CancellationToken.None);
            cancellationToken.ThrowIfCancellationRequested();

            var openingLifetimeGeneration = AdvanceConnectionLifetime();

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
            PublishClientForReservedSession(
                openingClient,
                openingLifetimeGeneration,
                openingSessionGeneration);

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
                    cancellationToken.ThrowIfCancellationRequested();

                    StartCallbackListener(
                        parsedLocalAddress,
                        callbackPort,
                        openingLifetimeGeneration,
                        openingSessionGeneration);
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
                    cancellationToken.ThrowIfCancellationRequested();

                    CallbackPort = registeredCallbackPort;
                    EventMask = eventMask;
                }

                cancellationToken.ThrowIfCancellationRequested();
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

        internal byte[] Exchange(
            byte[] request,
            long expectedGeneration,
            Action onWriteStarting)
        {
            return Exchange(
                request,
                expectedGeneration,
                onWriteStarting,
                null,
                null);
        }

        internal byte[] Exchange(
            byte[] request,
            long expectedGeneration,
            Action onWriteStarting,
            Func<byte[], bool> responseValidator,
            Action responsePublisher)
        {
            return ExchangeCore(
                request,
                client,
                false,
                CancellationToken.None,
                expectedGeneration,
                onWriteStarting,
                responseValidator,
                responsePublisher);
        }

        private byte[] ExchangeCore(
            byte[] request,
            TcpClient operationClient,
            bool allowLifecycleState,
            CancellationToken cancellationToken,
            long expectedGeneration,
            Action onWriteStarting = null,
            Func<byte[], bool> responseValidator = null,
            Action responsePublisher = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            EnterGate(sync, cancellationToken);

            var responseValidationFailed = false;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureConnected(operationClient, allowLifecycleState);
                EnsureExpectedGeneration(expectedGeneration);

                using (cancellationToken.Register(
                    () => AbortForCancellation(operationClient)))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var command = LMC_Frame.GetRequestCommand(request);
                    var maximumPayloadLength =
                        LMC_ResponsePayloadLimits.GetMaximumPayloadLength(command);
                    var stream = operationClient.GetStream();
                    if (sendPriorityCoordinator != null)
                    {
                        sendPriorityCoordinator.ValidateBeforeWrite(command);
                    }

                    if (onWriteStarting != null)
                    {
                        onWriteStarting();
                    }

                    stream.Write(request, 0, request.Length);

                    var header = ReadExact(stream, LMC_Frame.HeaderSize);
                    var payloadLength = LMC_Frame.GetResponsePayloadLength(header);

                    if (payloadLength > maximumPayloadLength)
                    {
                        throw new InvalidDataException(
                            "RPC response for command 0x"
                            + command.ToString("X4")
                            + " declares "
                            + payloadLength
                            + " payload bytes; the maximum allowed is "
                            + maximumPayloadLength
                            + ".");
                    }

                    var payload = payloadLength == 0
                        ? new byte[0]
                        : ReadExact(stream, payloadLength);

                    var response = CombineResponse(header, payload);
                    if (responseValidator != null)
                    {
                        try
                        {
                            if (responseValidator(response))
                            {
                                PublishExchangeBoundSendPriorityResult(
                                    operationClient,
                                    expectedGeneration,
                                    command,
                                    responsePublisher);
                            }
                        }
                        catch
                        {
                            responseValidationFailed = true;
                            throw;
                        }
                    }

                    return response;
                }
            }
            catch (Exception ex)
            {
                if (responseValidationFailed)
                {
                    MarkTransportFault(ex, operationClient);
                    throw;
                }

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
            return ExchangeAsync(
                request,
                expectedGeneration,
                cancellationToken,
                null);
        }

        internal Task<byte[]> ExchangeAsync(
            byte[] request,
            long expectedGeneration,
            CancellationToken cancellationToken,
            Action onWriteStarting)
        {
            return ExchangeAsync(
                request,
                expectedGeneration,
                cancellationToken,
                onWriteStarting,
                null,
                null);
        }

        internal Task<byte[]> ExchangeAsync(
            byte[] request,
            long expectedGeneration,
            CancellationToken cancellationToken,
            Action onWriteStarting,
            Func<byte[], bool> responseValidator,
            Action responsePublisher)
        {
            var operationClient = client;

            return Task.Run(
                () => ExchangeCore(
                    request,
                    operationClient,
                    false,
                    cancellationToken,
                    expectedGeneration,
                    onWriteStarting,
                    responseValidator,
                    responsePublisher));
        }

        /// <summary>
        /// Applies cancellation while waiting for the connection gate and immediately
        /// before the write-commit boundary. Once Stream.Write may have started, the
        /// response is drained without cancellation so the RPC stream remains reusable.
        /// </summary>
        internal Task<byte[]> ExchangeAsyncDrainAfterWrite(
            byte[] request,
            long expectedGeneration,
            CancellationToken preWriteCancellationToken,
            Action onWriteStarting)
        {
            return ExchangeAsyncDrainAfterWrite(
                request,
                expectedGeneration,
                preWriteCancellationToken,
                CancellationToken.None,
                onWriteStarting,
                null);
        }

        /// <summary>
        /// Preserves the legacy drain behavior for caller cancellation while
        /// allowing a distinct post-write deadline to invalidate the transport.
        /// onWriteCommitted runs only after the final authoritative pre-write
        /// cancellation check and immediately before Stream.Write may start.
        /// </summary>
        internal Task<byte[]> ExchangeAsyncDrainAfterWrite(
            byte[] request,
            long expectedGeneration,
            CancellationToken preWriteCancellationToken,
            CancellationToken postWriteDeadlineToken,
            Action onWriteStarting,
            Action onWriteCommitted)
        {
            var operationClient = client;

            return Task.Run(
                () => ExchangeCoreDrainAfterWrite(
                    request,
                    operationClient,
                    preWriteCancellationToken,
                    expectedGeneration,
                    postWriteDeadlineToken,
                    onWriteStarting,
                    onWriteCommitted));
        }

        private byte[] ExchangeCoreDrainAfterWrite(
            byte[] request,
            TcpClient operationClient,
            CancellationToken preWriteCancellationToken,
            long expectedGeneration,
            CancellationToken postWriteDeadlineToken,
            Action onWriteStarting,
            Action onWriteCommitted)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            EnterGate(sync, preWriteCancellationToken);
            var writeMayHaveStarted = false;
            var preWriteCallbackFailed = false;
            var postWriteDeadlineAborted = 0;

            try
            {
                preWriteCancellationToken.ThrowIfCancellationRequested();
                EnsureConnected(operationClient, false);
                EnsureExpectedGeneration(expectedGeneration);

                var command = LMC_Frame.GetRequestCommand(request);
                var maximumPayloadLength =
                    LMC_ResponsePayloadLimits.GetMaximumPayloadLength(command);
                var stream = operationClient.GetStream();
                if (sendPriorityCoordinator != null)
                {
                    sendPriorityCoordinator.ValidateBeforeWrite(command);
                }

                preWriteCancellationToken.ThrowIfCancellationRequested();
                if (onWriteStarting != null)
                {
                    try
                    {
                        onWriteStarting();
                    }
                    catch
                    {
                        preWriteCallbackFailed = true;
                        throw;
                    }
                }
                preWriteCancellationToken.ThrowIfCancellationRequested();

                if (onWriteCommitted != null)
                {
                    try
                    {
                        onWriteCommitted();
                    }
                    catch
                    {
                        preWriteCallbackFailed = true;
                        throw;
                    }
                }

                // Cancellation stops being authoritative at this commit boundary.
                // Stream.Write can partially transmit before throwing, so the flag must
                // be set immediately before invoking it rather than after it returns.
                writeMayHaveStarted = true;
                byte[] response;
                using (postWriteDeadlineToken.Register(
                    () =>
                    {
                        if (AbortForPostWriteDeadline(operationClient))
                        {
                            Interlocked.Exchange(
                                ref postWriteDeadlineAborted,
                                1);
                        }
                    }))
                {
                    stream.Write(request, 0, request.Length);

                    var header = ReadExact(stream, LMC_Frame.HeaderSize);
                    var payloadLength = LMC_Frame.GetResponsePayloadLength(header);

                    if (payloadLength > maximumPayloadLength)
                    {
                        throw new InvalidDataException(
                            "RPC response for command 0x"
                            + command.ToString("X4")
                            + " declares "
                            + payloadLength
                            + " payload bytes; the maximum allowed is "
                            + maximumPayloadLength
                            + ".");
                    }

                    var payload = payloadLength == 0
                        ? new byte[0]
                        : ReadExact(stream, payloadLength);

                    response = CombineResponse(header, payload);
                }

                if (Volatile.Read(ref postWriteDeadlineAborted) != 0)
                {
                    throw new LMCPostWriteDeadlineException(null);
                }

                return response;
            }
            catch (Exception ex)
            {
                if (ex is LMCPostWriteDeadlineException)
                {
                    throw;
                }

                if (writeMayHaveStarted
                    && Volatile.Read(ref postWriteDeadlineAborted) != 0)
                {
                    throw new LMCPostWriteDeadlineException(ex);
                }

                if (!writeMayHaveStarted
                    && preWriteCancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(
                        preWriteCancellationToken);
                }

                if (!writeMayHaveStarted && preWriteCallbackFailed)
                {
                    throw;
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

        internal void PublishSendPriorityResult(
            ushort command,
            Action publish)
        {
            if (sendPriorityCoordinator != null)
            {
                sendPriorityCoordinator.PublishResult(command, publish);
                return;
            }

            publish();
        }

        private void PublishExchangeBoundSendPriorityResult(
            TcpClient operationClient,
            long expectedGeneration,
            ushort command,
            Action publish)
        {
            if (publish == null)
            {
                throw new ArgumentNullException("publish");
            }

            // ExchangeCore owns sync here. Do not acquire lifecycleSync: Close
            // owns lifecycleSync before it waits for sync. This narrower state
            // lock pins the exact client/session/lifetime through publication.
            lock (lifecycleStateSync)
            {
                if (State != LMCConnectionState.Connected
                    || expectedGeneration <= 0
                    || sessionGeneration != expectedGeneration
                    || clientSessionGeneration != expectedGeneration
                    || !ReferenceEquals(client, operationClient)
                    || clientLifetimeGeneration <= 0
                    || connectionLifetimeGeneration
                        != clientLifetimeGeneration)
                {
                    throw new InvalidOperationException(
                        "The RPC response cannot be published because its exact connection session is no longer active.");
                }

                PublishSendPriorityResult(command, publish);
            }
        }

        internal void PublishSessionBoundSendPriorityResult(
            long expectedGeneration,
            ushort command,
            Action publish)
        {
            if (publish == null)
            {
                throw new ArgumentNullException("publish");
            }

            // Linearize result publication against Close/Open. Close owns this
            // gate before it moves the connection to Closing, so a response is
            // either published in the same active session or rejected after
            // the lifecycle transition completes.
            EnterGate(lifecycleSync, CancellationToken.None);
            try
            {
                EnsureSessionGeneration(expectedGeneration);
                PublishSendPriorityResult(
                    command,
                    () =>
                    {
                        EnsureSessionGeneration(expectedGeneration);
                        publish();
                    });
            }
            finally
            {
                Monitor.Exit(lifecycleSync);
            }
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

        /// <summary>
        /// Detaches only the still-current transport for an exact RPC session
        /// after a mutation crossed its write boundary without a definitive
        /// correlated result. A newer session is never detached.
        /// </summary>
        internal bool TryInvalidateSessionAfterUncertainMutation(
            long expectedSessionGeneration,
            Exception exception)
        {
            if (expectedSessionGeneration <= 0 || exception == null)
            {
                return false;
            }

            EnterGate(lifecycleSync, CancellationToken.None);
            try
            {
                TcpClient currentClient;
                long detachedLifetimeGeneration;
                lock (lifecycleStateSync)
                {
                    if (State != LMCConnectionState.Connected
                        || sessionGeneration
                            != expectedSessionGeneration
                        || clientSessionGeneration
                            != expectedSessionGeneration
                        || client == null
                        || clientLifetimeGeneration <= 0
                        || connectionLifetimeGeneration
                            != clientLifetimeGeneration)
                    {
                        return false;
                    }

                    currentClient = client;
                    detachedLifetimeGeneration =
                        clientLifetimeGeneration;
                    client = null;
                    clientLifetimeGeneration = 0;
                    clientSessionGeneration = 0;
                }

                CloseClientQuietly(currentClient);
                return InvalidateDetachedTransport(
                    exception,
                    detachedLifetimeGeneration,
                    true);
            }
            finally
            {
                Monitor.Exit(lifecycleSync);
            }
        }

        internal long CaptureSessionGeneration(out bool isConnected)
        {
            EnterGate(lifecycleSync, CancellationToken.None);
            try
            {
                var generation = SessionGeneration;
                isConnected = State == LMCConnectionState.Connected;
                return generation;
            }
            finally
            {
                Monitor.Exit(lifecycleSync);
            }
        }

        internal LMCGroupEnableWaitCoordinator GetGroupEnableWaitCoordinator(
            long expectedGeneration,
            ushort groupReference)
        {
            EnsureSessionGeneration(expectedGeneration);

            lock (groupEnableWaitRegistrySync)
            {
                EnsureSessionGeneration(expectedGeneration);

                if (groupEnableWaitRegistryGeneration != expectedGeneration)
                {
                    groupEnableWaitCoordinators.Clear();
                    groupEnableWaitRegistryGeneration = expectedGeneration;
                }

                LMCGroupEnableWaitCoordinator coordinator;
                if (!groupEnableWaitCoordinators.TryGetValue(
                    groupReference,
                    out coordinator))
                {
                    coordinator = new LMCGroupEnableWaitCoordinator();
                    groupEnableWaitCoordinators.Add(groupReference, coordinator);
                }

                return coordinator;
            }
        }

        internal LMCAxisPowerOnWaitCoordinator GetAxisPowerOnWaitCoordinator(
            long expectedGeneration,
            ushort axisReference)
        {
            EnsureSessionGeneration(expectedGeneration);

            lock (axisPowerOnWaitRegistrySync)
            {
                EnsureSessionGeneration(expectedGeneration);

                if (axisPowerOnWaitRegistryGeneration != expectedGeneration)
                {
                    axisPowerOnWaitCoordinators.Clear();
                    axisPowerOnWaitRegistryGeneration = expectedGeneration;
                }

                LMCAxisPowerOnWaitCoordinator coordinator;
                if (!axisPowerOnWaitCoordinators.TryGetValue(
                        axisReference,
                        out coordinator))
                {
                    coordinator = new LMCAxisPowerOnWaitCoordinator();
                    axisPowerOnWaitCoordinators.Add(axisReference, coordinator);
                }

                return coordinator;
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
                var payload = new byte[response.PayloadLength];
                Buffer.BlockCopy(
                    safeRaw,
                    LMC_Frame.HeaderSize,
                    payload,
                    0,
                    response.PayloadLength);
                response.Payload = payload;
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

        internal static LMCLookupResult ParseLookupResult(
            LMCLookupTargetKind targetKind,
            string objectName,
            byte[] raw)
        {
            LMC_Response response;
            ushort reference;
            if (!TryParseLookupReference(raw, out response, out reference))
            {
                throw CreateLookupFailureException(
                    targetKind,
                    objectName,
                    raw);
            }

            return new LMCLookupResult(
                targetKind,
                objectName,
                reference,
                response);
        }

        internal static LMCLookupException CreateLookupFailureException(
            string targetKind,
            string objectName,
            byte[] raw)
        {
            LMCLookupTargetKind parsedTargetKind;
            if (string.Equals(
                targetKind,
                "Axis",
                StringComparison.Ordinal))
            {
                parsedTargetKind = LMCLookupTargetKind.Axis;
            }
            else if (string.Equals(
                targetKind,
                "Group",
                StringComparison.Ordinal))
            {
                parsedTargetKind = LMCLookupTargetKind.Group;
            }
            else
            {
                throw new ArgumentOutOfRangeException("targetKind");
            }

            return CreateLookupFailureException(
                parsedTargetKind,
                objectName,
                raw);
        }

        private static LMCLookupException CreateLookupFailureException(
            LMCLookupTargetKind targetKind,
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

            return new LMCLookupException(
                targetKind,
                objectName,
                response,
                hasLookupPayload,
                lookupReference,
                message.ToString());
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
            EnsureLifecycleOperationNotReentrant();
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
            AdvanceConnectionLifetime();
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
                    ClearPublishedClient(currentClient);
                    currentClient.Close();
                }
                StopCallbackListener();
                lock (lifecycleStateSync)
                {
                    ClearConnectionMetadata();
                }
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

        private void StartCallbackListener(
            IPAddress localAddress,
            int callbackPort,
            long lifetimeGeneration,
            long openingSessionGeneration)
        {
            StopCallbackListener();

            var listener = new UdpClient(new IPEndPoint(localAddress, callbackPort));
            var callbackSourceAddress = expectedCallbackAddress;
            var thread = new Thread(
                () => ReceiveCallbackLoop(
                    listener,
                    callbackSourceAddress,
                    lifetimeGeneration,
                    openingSessionGeneration))
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
                callbackListenerLifetimeGeneration = lifetimeGeneration;
            }

            thread.Start();
        }

        private void StopCallbackListener()
        {
            StopCallbackListener(AnyConnectionLifetimeGeneration);
        }

        private void StopCallbackListener(long expectedLifetimeGeneration)
        {
            UdpClient listener;
            Thread thread;

            lock (callbackSync)
            {
                if (expectedLifetimeGeneration
                        != AnyConnectionLifetimeGeneration
                    && callbackListenerLifetimeGeneration
                        != expectedLifetimeGeneration)
                {
                    return;
                }

                callbackListenerRunning = false;
                listener = callbackListener;
                thread = callbackThread;
                callbackListener = null;
                callbackThread = null;
                CallbackLocalEndPoint = null;
                callbackListenerLifetimeGeneration = 0;
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
            IPAddress ownedSourceAddress,
            long ownedLifetimeGeneration,
            long openingSessionGeneration)
        {
            while (IsCurrentCallbackListener(
                ownedListener,
                ownedLifetimeGeneration))
            {
                try
                {
                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    var payload = ownedListener.Receive(ref remoteEndPoint);

                    if (!IsCurrentCallbackListener(
                        ownedListener,
                        ownedLifetimeGeneration))
                    {
                        break;
                    }

                    if (options.ValidateCallbackSourceAddress
                        && ownedSourceAddress != null
                        && !ownedSourceAddress.Equals(remoteEndPoint.Address))
                    {
                        TryIncrementRejectedCallbackCount(
                            ownedListener,
                            ownedLifetimeGeneration);
                        continue;
                    }

                    OnCallbackReceived(
                        ownedListener,
                        ownedLifetimeGeneration,
                        new LMCCallbackEventArgs(
                            payload,
                            remoteEndPoint,
                            DateTime.UtcNow,
                            this,
                            ownedLifetimeGeneration,
                            openingSessionGeneration));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    OnCallbackListenerError(
                        ownedListener,
                        ownedLifetimeGeneration,
                        new LMCCallbackErrorEventArgs(ex));
                }
                catch (Exception ex)
                {
                    OnCallbackListenerError(
                        ownedListener,
                        ownedLifetimeGeneration,
                        new LMCCallbackErrorEventArgs(ex));
                }
            }
        }

        private void OnCallbackReceived(
            UdpClient ownedListener,
            long ownedLifetimeGeneration,
            LMCCallbackEventArgs e)
        {
            if (!IsCurrentCallbackListener(
                ownedListener,
                ownedLifetimeGeneration))
            {
                return;
            }

            var handler = CallbackReceived;
            if (handler != null)
            {
                try
                {
                    handler(this, e);
                }
                catch (Exception ex)
                {
                    OnCallbackListenerError(
                        ownedListener,
                        ownedLifetimeGeneration,
                        new LMCCallbackErrorEventArgs(ex));
                }
            }
        }

        private void OnCallbackListenerError(
            UdpClient ownedListener,
            long ownedLifetimeGeneration,
            LMCCallbackErrorEventArgs e)
        {
            if (!IsCurrentCallbackListener(
                ownedListener,
                ownedLifetimeGeneration))
            {
                return;
            }

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

        private bool IsCurrentCallbackListener(
            UdpClient ownedListener,
            long ownedLifetimeGeneration)
        {
            lock (callbackSync)
            {
                return callbackListenerRunning
                    && ReferenceEquals(callbackListener, ownedListener)
                    && callbackListenerLifetimeGeneration
                        == ownedLifetimeGeneration;
            }
        }

        internal bool IsCurrentCallbackSession(
            long expectedLifetimeGeneration,
            long expectedSessionGeneration)
        {
            // Combined provenance checks use lifecycle -> callback lock order.
            // Callback-only paths never acquire lifecycleStateSync while
            // holding callbackSync, so Close/receive cannot form a lock cycle.
            lock (lifecycleStateSync)
            {
                lock (callbackSync)
                {
                    return expectedLifetimeGeneration > 0
                        && expectedSessionGeneration > 0
                        && connectionLifetimeGeneration
                            == expectedLifetimeGeneration
                        && sessionGeneration == expectedSessionGeneration
                        && callbackListenerRunning
                        && callbackListener != null
                        && callbackListenerLifetimeGeneration
                            == expectedLifetimeGeneration;
                }
            }
        }

        private void TryIncrementRejectedCallbackCount(
            UdpClient ownedListener,
            long ownedLifetimeGeneration)
        {
            lock (callbackSync)
            {
                if (!callbackListenerRunning
                    || !ReferenceEquals(callbackListener, ownedListener)
                    || callbackListenerLifetimeGeneration
                        != ownedLifetimeGeneration)
                {
                    return;
                }

                Interlocked.Increment(ref rejectedCallbackCount);
            }
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
            long detachedLifetimeGeneration;
            if (!TryDetachClient(
                operationClient,
                out detachedLifetimeGeneration))
            {
                return;
            }

            CloseClientQuietly(operationClient);
            InvalidateDetachedTransport(
                exception,
                detachedLifetimeGeneration,
                true);
        }

        private void AbortForCancellation(TcpClient operationClient)
        {
            long detachedLifetimeGeneration;
            if (!TryDetachClient(
                operationClient,
                out detachedLifetimeGeneration))
            {
                return;
            }

            var exception = new OperationCanceledException(
                "The RPC operation was cancelled after it acquired the transport. Reconnect before issuing another command.");

            CloseClientQuietly(operationClient);
            InvalidateDetachedTransport(
                exception,
                detachedLifetimeGeneration,
                false);
        }

        private bool AbortForPostWriteDeadline(TcpClient operationClient)
        {
            long detachedLifetimeGeneration;
            if (!TryDetachClient(
                operationClient,
                out detachedLifetimeGeneration))
            {
                return false;
            }

            var exception = new TimeoutException(
                "The RPC response deadline expired after the write-commit boundary; reconnect before issuing another command.");

            CloseClientQuietly(operationClient);
            InvalidateDetachedTransport(
                exception,
                detachedLifetimeGeneration,
                true);
            return true;
        }

        private bool TryDetachClient(
            TcpClient operationClient,
            out long detachedLifetimeGeneration)
        {
            detachedLifetimeGeneration = 0;
            if (operationClient == null)
            {
                return false;
            }

            lock (lifecycleStateSync)
            {
                if (!ReferenceEquals(client, operationClient))
                {
                    return false;
                }

                detachedLifetimeGeneration = clientLifetimeGeneration;
                client = null;
                clientLifetimeGeneration = 0;
                clientSessionGeneration = 0;
                return true;
            }
        }

        private bool InvalidateDetachedTransport(
            Exception exception,
            long detachedLifetimeGeneration,
            bool recordTransportException)
        {
            lock (lifecycleStateSync)
            {
                if (connectionLifetimeGeneration
                    != detachedLifetimeGeneration)
                {
                    return false;
                }

                if (recordTransportException)
                {
                    LastTransportException = exception;
                }

                ClearConnectionMetadata();
            }

            StopCallbackListener(detachedLifetimeGeneration);

            lock (lifecycleStateSync)
            {
                if (connectionLifetimeGeneration
                        == detachedLifetimeGeneration
                    && State != LMCConnectionState.Closing)
                {
                    SetConnectionState(LMCConnectionState.Faulted, exception);
                    return true;
                }
            }

            return false;
        }

        private long AdvanceConnectionLifetime()
        {
            lock (lifecycleStateSync)
            {
                return ++connectionLifetimeGeneration;
            }
        }

        private long ReserveOpeningSession()
        {
            lock (lifecycleStateSync)
            {
                return ++sessionGeneration;
            }
        }

        private void PublishClientForReservedSession(
            TcpClient openingClient,
            long lifetimeGeneration,
            long openingSessionGeneration)
        {
            lock (lifecycleStateSync)
            {
                clientLifetimeGeneration = lifetimeGeneration;
                client = openingClient;
                var observer = options
                    .ClientPublishedBeforeSessionBindObserver;
                if (observer != null)
                {
                    observer();
                }

                // The client and its reserved session become observable to a
                // pinned safety abort under this single state lock.
                clientSessionGeneration = openingSessionGeneration;
            }
        }

        private void ClearPublishedClient(TcpClient expectedClient)
        {
            lock (lifecycleStateSync)
            {
                if (!ReferenceEquals(client, expectedClient))
                {
                    return;
                }

                client = null;
                clientLifetimeGeneration = 0;
                clientSessionGeneration = 0;
            }
        }

        private void ClearConnectionMetadata()
        {
            IsRpcInitialized = false;
            CallbackPort = 0;
            EventMask = 0;
            expectedCallbackAddress = null;
            RpcSessionInitResponse = null;
            RpcCallbackRegistrationResponse = null;
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

        private static bool TryApplyAbortiveLinger(TcpClient targetClient)
        {
            if (targetClient == null)
            {
                return false;
            }

            try
            {
                // Linger enabled with a zero timeout requests an abortive TCP
                // close (RST) instead of an orderly FIN.
                targetClient.Client.LingerState = new LingerOption(true, 0);
                var appliedLinger = targetClient.Client.LingerState;
                return appliedLinger.Enabled && appliedLinger.LingerTime == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsTransportException(Exception exception)
        {
            return exception is IOException
                || exception is InvalidDataException
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

            var previousEventScope = activeConnectionStateEventScope.Value;
            var currentEventScope = new ConnectionStateEventScope(
                this,
                previousEventScope);
            activeConnectionStateEventScope.Value = currentEventScope;

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
            finally
            {
                currentEventScope.IsActive = false;
                activeConnectionStateEventScope.Value = previousEventScope;
            }
        }

        private void EnsureLifecycleOperationNotReentrant()
        {
            for (var eventScope = activeConnectionStateEventScope.Value;
                eventScope != null;
                eventScope = eventScope.Parent)
            {
                if (eventScope.IsActive
                    && ReferenceEquals(eventScope.Source, this))
                {
                    ThrowLifecycleOperationReentry();
                }
            }

            if (Monitor.IsEntered(lifecycleSync))
            {
                ThrowLifecycleOperationReentry();
            }
        }

        private static void ThrowLifecycleOperationReentry()
        {
            throw new InvalidOperationException(
                "Connection lifecycle methods cannot be called "
                + "reentrantly from a ConnectionStateChanged handler. "
                + "Defer the operation until the handler returns.");
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

using System;
using System.Net;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Identifies the production meaning of a version-2 callback wake hint.
    /// </summary>
    public enum LMCCallbackWakeHintEventType : ushort
    {
        DiagnosticsOperationTerminalAvailable = 1
    }

    public sealed class LMCCallbackProtocolPolicy
    {
        private readonly Func<uint, bool> registrationMaskPredicate;
        private readonly Func<ushort, uint, bool> eventIdentifierPredicate;
        private readonly Func<byte, bool> deliveryClassPredicate;
        private readonly Func<ushort, short, bool> registrationResultPredicate;
        private readonly Func<int, bool> payloadLengthPredicate;
        private readonly Func<uint, bool> eventMaskBitPredicate;

        public LMCCallbackProtocolPolicy(
            Func<uint, bool> registrationMaskPredicate,
            Func<ushort, uint, bool> eventIdentifierPredicate,
            Func<byte, bool> deliveryClassPredicate,
            Func<ushort, short, bool> registrationResultPredicate)
            : this(
                registrationMaskPredicate,
                eventIdentifierPredicate,
                deliveryClassPredicate,
                registrationResultPredicate,
                payloadBytes => true,
                eventMaskBit => true)
        {
        }

        public LMCCallbackProtocolPolicy(
            Func<uint, bool> registrationMaskPredicate,
            Func<ushort, uint, bool> eventIdentifierPredicate,
            Func<byte, bool> deliveryClassPredicate,
            Func<ushort, short, bool> registrationResultPredicate,
            Func<int, bool> payloadLengthPredicate)
            : this(
                registrationMaskPredicate,
                eventIdentifierPredicate,
                deliveryClassPredicate,
                registrationResultPredicate,
                payloadLengthPredicate,
                eventMaskBit => true)
        {
        }

        public LMCCallbackProtocolPolicy(
            Func<uint, bool> registrationMaskPredicate,
            Func<ushort, uint, bool> eventIdentifierPredicate,
            Func<byte, bool> deliveryClassPredicate,
            Func<ushort, short, bool> registrationResultPredicate,
            Func<int, bool> payloadLengthPredicate,
            Func<uint, bool> eventMaskBitPredicate)
        {
            this.registrationMaskPredicate = registrationMaskPredicate;
            this.eventIdentifierPredicate = eventIdentifierPredicate;
            this.deliveryClassPredicate = deliveryClassPredicate;
            this.registrationResultPredicate = registrationResultPredicate;
            this.payloadLengthPredicate = payloadLengthPredicate;
            this.eventMaskBitPredicate = eventMaskBitPredicate;
        }

        public static LMCCallbackProtocolPolicy FailClosed
        {
            get
            {
                return new LMCCallbackProtocolPolicy(
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);
            }
        }

        /// <summary>
        /// Gets the initial production policy for version-2 UDP wake hints.
        /// </summary>
        public static LMCCallbackProtocolPolicy InitialV2WakeHint
        {
            get
            {
                return new LMCCallbackProtocolPolicy(
                    eventMask => (eventMask & 1u) == 1u,
                    (eventType, eventId) =>
                        eventType == (ushort)LMCCallbackWakeHintEventType
                            .DiagnosticsOperationTerminalAvailable
                        && eventId != 0,
                    deliveryClass => deliveryClass == 0,
                    (status, errorId) => status == 0 && errorId == 0,
                    payloadBytes => payloadBytes == 0,
                    eventMaskBit => eventMaskBit == 1u);
            }
        }

        internal bool ApprovesRegistrationMask(uint eventMask)
        {
            return InvokeFailClosed(
                registrationMaskPredicate,
                eventMask);
        }

        internal bool ApprovesEventMaskBit(uint eventMaskBit)
        {
            return InvokeFailClosed(
                eventMaskBitPredicate,
                eventMaskBit);
        }

        internal bool ApprovesEventIdentifier(
            ushort eventType,
            uint eventId)
        {
            if (eventIdentifierPredicate == null)
            {
                return false;
            }

            try
            {
                return eventIdentifierPredicate(eventType, eventId);
            }
            catch
            {
                return false;
            }
        }

        internal bool ApprovesDeliveryClass(byte deliveryClass)
        {
            return InvokeFailClosed(
                deliveryClassPredicate,
                deliveryClass);
        }

        internal bool ApprovesRegistrationResult(
            ushort status,
            short errorId)
        {
            if (registrationResultPredicate == null)
            {
                return false;
            }

            try
            {
                return registrationResultPredicate(status, errorId);
            }
            catch
            {
                return false;
            }
        }

        internal bool ApprovesPayloadLength(int payloadBytes)
        {
            return InvokeFailClosed(
                payloadLengthPredicate,
                payloadBytes);
        }

        private static bool InvokeFailClosed<T>(
            Func<T, bool> predicate,
            T value)
        {
            if (predicate == null)
            {
                return false;
            }

            try
            {
                return predicate(value);
            }
            catch
            {
                return false;
            }
        }
    }

    public sealed class LMCCallbackRegistrationV2Request
    {
        private readonly byte[] callbackIPv4;

        public LMCCallbackRegistrationV2Request(
            uint eventMask,
            int callbackPort,
            byte[] callbackIPv4,
            ushort requestedMaxDatagram,
            uint clientCookieLo,
            uint clientCookieHi)
            : this(
                eventMask,
                callbackPort,
                callbackIPv4,
                requestedMaxDatagram,
                clientCookieLo,
                clientCookieHi,
                0,
                0)
        {
        }

        internal LMCCallbackRegistrationV2Request(
            uint eventMask,
            int callbackPort,
            byte[] callbackIPv4,
            ushort requestedMaxDatagram,
            uint clientCookieLo,
            uint clientCookieHi,
            uint flags,
            uint reserved)
        {
            EventMask = eventMask;
            CallbackPort = callbackPort;
            this.callbackIPv4 = CloneOrEmpty(callbackIPv4);
            RequestedMaxDatagram = requestedMaxDatagram;
            ClientCookieLo = clientCookieLo;
            ClientCookieHi = clientCookieHi;
            Flags = flags;
            Reserved = reserved;
        }

        public uint EventMask { get; private set; }
        public int CallbackPort { get; private set; }
        public byte[] CallbackIPv4
        {
            get { return CloneOrEmpty(callbackIPv4); }
        }
        public ushort ProtocolVersion
        {
            get { return LMCCallbackProtocol.ProtocolVersion2; }
        }
        public ushort RequestedMaxDatagram { get; private set; }
        public uint ClientCookieLo { get; private set; }
        public uint ClientCookieHi { get; private set; }
        public ulong ClientCookie
        {
            get
            {
                return ((ulong)ClientCookieHi << 32) | ClientCookieLo;
            }
        }
        public uint Flags { get; private set; }
        public uint Reserved { get; private set; }

        private static byte[] CloneOrEmpty(byte[] value)
        {
            return value == null ? new byte[0] : (byte[])value.Clone();
        }
    }

    public sealed class LMCCallbackSessionFence
    {
        private readonly byte[] expectedSourceIPv4;

        internal LMCCallbackSessionFence(
            long listenerGeneration,
            byte[] expectedSourceIPv4,
            uint registeredEventMask,
            ushort maxDatagramBytes,
            uint bootId,
            uint sessionEpoch,
            uint cookieLo,
            uint cookieHi)
        {
            ListenerGeneration = listenerGeneration;
            this.expectedSourceIPv4 = CloneOrEmpty(expectedSourceIPv4);
            RegisteredEventMask = registeredEventMask;
            MaxDatagramBytes = maxDatagramBytes;
            BootId = bootId;
            SessionEpoch = sessionEpoch;
            CookieLo = cookieLo;
            CookieHi = cookieHi;
        }

        public long ListenerGeneration { get; private set; }
        public byte[] ExpectedSourceIPv4
        {
            get { return CloneOrEmpty(expectedSourceIPv4); }
        }
        public uint RegisteredEventMask { get; private set; }
        public ushort MaxDatagramBytes { get; private set; }
        public uint BootId { get; private set; }
        public uint SessionEpoch { get; private set; }
        public uint CookieLo { get; private set; }
        public uint CookieHi { get; private set; }
        public ulong Cookie
        {
            get { return ((ulong)CookieHi << 32) | CookieLo; }
        }

        private static byte[] CloneOrEmpty(byte[] value)
        {
            return value == null ? new byte[0] : (byte[])value.Clone();
        }
    }

    public sealed class LMCCallbackRegistrationV2Response
    {
        internal LMCCallbackRegistrationV2Response(
            ushort status,
            short errorId,
            ushort acceptedVersion,
            ushort acceptedMaxDatagram,
            uint diagnosticsBootId,
            uint sessionEpoch,
            uint acceptedFlags,
            LMCCallbackSessionFence sessionFence)
        {
            Status = status;
            ErrorId = errorId;
            AcceptedVersion = acceptedVersion;
            AcceptedMaxDatagram = acceptedMaxDatagram;
            DiagnosticsBootId = diagnosticsBootId;
            SessionEpoch = sessionEpoch;
            AcceptedFlags = acceptedFlags;
            SessionFence = sessionFence;
        }

        public ushort Status { get; private set; }
        public short ErrorId { get; private set; }
        public ushort AcceptedVersion { get; private set; }
        public ushort AcceptedMaxDatagram { get; private set; }
        public uint DiagnosticsBootId { get; private set; }
        public uint SessionEpoch { get; private set; }
        public uint AcceptedFlags { get; private set; }
        public LMCCallbackSessionFence SessionFence { get; private set; }
    }

    public sealed class LMCCallbackDatagramWrite
    {
        private readonly byte[] payload;

        public LMCCallbackDatagramWrite(
            ushort eventType,
            uint eventMaskBit,
            ulong sequence,
            uint eventId,
            uint plcTimeMs,
            byte deliveryClass,
            byte[] payload)
            : this(
                eventType,
                eventMaskBit,
                sequence,
                eventId,
                plcTimeMs,
                deliveryClass,
                0,
                payload)
        {
        }

        internal LMCCallbackDatagramWrite(
            ushort eventType,
            uint eventMaskBit,
            ulong sequence,
            uint eventId,
            uint plcTimeMs,
            byte deliveryClass,
            byte flags,
            byte[] payload)
        {
            EventType = eventType;
            EventMaskBit = eventMaskBit;
            Sequence = sequence;
            EventId = eventId;
            PlcTimeMs = plcTimeMs;
            DeliveryClass = deliveryClass;
            Flags = flags;
            this.payload = CloneOrEmpty(payload);
        }

        public ushort EventType { get; private set; }
        public uint EventMaskBit { get; private set; }
        public ulong Sequence { get; private set; }
        public uint EventId { get; private set; }
        public uint PlcTimeMs { get; private set; }
        public byte DeliveryClass { get; private set; }
        public byte Flags { get; private set; }
        public byte[] Payload
        {
            get { return CloneOrEmpty(payload); }
        }

        private static byte[] CloneOrEmpty(byte[] value)
        {
            return value == null ? new byte[0] : (byte[])value.Clone();
        }
    }

    internal sealed class LMCCallbackDatagram
    {
        private readonly byte[] payload;

        internal LMCCallbackDatagram(
            ushort eventType,
            uint eventMaskBit,
            uint bootId,
            uint sessionEpoch,
            uint cookieLo,
            uint cookieHi,
            ulong sequence,
            uint eventId,
            uint plcTimeMs,
            byte deliveryClass,
            byte[] payload)
        {
            EventType = eventType;
            EventMaskBit = eventMaskBit;
            BootId = bootId;
            SessionEpoch = sessionEpoch;
            CookieLo = cookieLo;
            CookieHi = cookieHi;
            Sequence = sequence;
            EventId = eventId;
            PlcTimeMs = plcTimeMs;
            DeliveryClass = deliveryClass;
            this.payload = payload == null
                ? new byte[0]
                : (byte[])payload.Clone();
        }

        internal ushort EventType { get; private set; }
        internal uint EventMaskBit { get; private set; }
        internal uint BootId { get; private set; }
        internal uint SessionEpoch { get; private set; }
        internal uint CookieLo { get; private set; }
        internal uint CookieHi { get; private set; }
        internal ulong Sequence { get; private set; }
        internal uint EventId { get; private set; }
        internal uint PlcTimeMs { get; private set; }
        internal byte DeliveryClass { get; private set; }
        internal byte[] Payload
        {
            get { return (byte[])payload.Clone(); }
        }
    }

    public sealed class LMCCallbackWakeHint
    {
        private readonly byte[] payload;

        internal LMCCallbackWakeHint(LMCCallbackDatagram datagram)
        {
            EventType = datagram.EventType;
            EventMaskBit = datagram.EventMaskBit;
            BootId = datagram.BootId;
            SessionEpoch = datagram.SessionEpoch;
            CookieLo = datagram.CookieLo;
            CookieHi = datagram.CookieHi;
            Sequence = datagram.Sequence;
            EventId = datagram.EventId;
            PlcTimeMs = datagram.PlcTimeMs;
            DeliveryClass = datagram.DeliveryClass;
            payload = datagram.Payload;
        }

        public ushort EventType { get; private set; }
        public uint EventMaskBit { get; private set; }
        public uint BootId { get; private set; }
        public uint SessionEpoch { get; private set; }
        public uint CookieLo { get; private set; }
        public uint CookieHi { get; private set; }
        public ulong Sequence { get; private set; }
        public uint EventId { get; private set; }
        public uint PlcTimeMs { get; private set; }
        public byte DeliveryClass { get; private set; }
        public byte[] Payload
        {
            get { return (byte[])payload.Clone(); }
        }
        public bool IsAuthoritative
        {
            get { return false; }
        }
        public bool RequiresAuthoritativeTcpQuery
        {
            get { return true; }
        }
    }

    public sealed class LMCCallbackWakeHintEventArgs : EventArgs
    {
        private readonly LMCConnection ownerConnection;
        private readonly IPEndPoint remoteEndPoint;
        private readonly long connectionLifetimeGeneration;
        private readonly long sessionGeneration;

        internal LMCCallbackWakeHintEventArgs(
            LMCCallbackWakeHint wakeHint,
            IPEndPoint remoteEndPoint,
            DateTime receivedAtUtc,
            LMCConnection ownerConnection,
            long connectionLifetimeGeneration,
            long sessionGeneration)
        {
            if (wakeHint == null)
            {
                throw new ArgumentNullException("wakeHint");
            }

            WakeHint = wakeHint;
            this.remoteEndPoint = CloneEndPoint(remoteEndPoint);
            ReceivedAtUtc = receivedAtUtc;
            this.ownerConnection = ownerConnection;
            this.connectionLifetimeGeneration =
                connectionLifetimeGeneration;
            this.sessionGeneration = sessionGeneration;
        }

        public LMCCallbackWakeHint WakeHint { get; private set; }
        public IPEndPoint RemoteEndPoint
        {
            get { return CloneEndPoint(remoteEndPoint); }
        }
        public DateTime ReceivedAtUtc { get; private set; }
        public long SessionGeneration
        {
            get { return sessionGeneration; }
        }

        public bool BelongsTo(LMCConnection connection)
        {
            return connection != null
                && ReferenceEquals(ownerConnection, connection);
        }

        public bool BelongsToCurrentSession(LMCConnection connection)
        {
            return BelongsTo(connection)
                && connection.IsCurrentCallbackV2Session(
                    connectionLifetimeGeneration,
                    sessionGeneration);
        }

        /// <summary>
        /// Returns true only when this wake hint exactly correlates to a
        /// retained D5 operation ticket on the current RPC session.
        /// </summary>
        public bool MatchesD5OperationTerminalTicket(
            LMCConnection connection,
            LMCOperationTicket ticket)
        {
            if (connection == null || ticket == null)
            {
                return false;
            }

            try
            {
                return BelongsToCurrentSession(connection)
                    && ticket.BelongsToCurrentSession(connection)
                    && WakeHint.EventType
                        == (ushort)LMCCallbackWakeHintEventType
                            .DiagnosticsOperationTerminalAvailable
                    && WakeHint.EventMaskBit == 1u
                    && WakeHint.DeliveryClass == 0
                    && WakeHint.Payload.Length == 0
                    && WakeHint.EventId != 0
                    && WakeHint.EventId == ticket.TicketId
                    && WakeHint.BootId == ticket.DiagnosticsBootId;
            }
            catch
            {
                return false;
            }
        }

        private static IPEndPoint CloneEndPoint(IPEndPoint value)
        {
            if (value == null)
            {
                return null;
            }

            return new IPEndPoint(
                new IPAddress(value.Address.GetAddressBytes()),
                value.Port);
        }
    }

    public enum LMCCallbackProtocolError
    {
        None = 0,
        NullPayload,
        WrongLength,
        RegistrationMaskNotApproved,
        CallbackPortOutOfRange,
        CallbackAddressInvalid,
        CallbackAddressMismatch,
        CallbackSourceAddressInvalid,
        CallbackSourceAddressMismatch,
        ListenerGenerationInvalid,
        ProtocolVersionMismatch,
        MaxDatagramOutOfRange,
        CookieZero,
        FlagsNonZero,
        ReservedNonZero,
        RegistrationResultNotApproved,
        AcceptedMaxDatagramInvalid,
        BootIdZero,
        SessionEpochZero,
        MagicMismatch,
        HeaderLengthMismatch,
        DatagramLengthMismatch,
        DatagramTooLarge,
        PayloadTooLarge,
        PayloadLengthMismatch,
        EventMaskNotSingleBit,
        EventMaskNotSubscribed,
        EventIdentifierNotApproved,
        DeliveryClassNotApproved,
        StaleBootId,
        StaleSessionEpoch,
        StaleCookie,
        PayloadNotApproved,
        EventMaskBitNotApproved
    }

    internal sealed class LMCCallbackParseResult<T>
        where T : class
    {
        private LMCCallbackParseResult(
            T value,
            LMCCallbackProtocolError error)
        {
            Value = value;
            Error = error;
        }

        internal T Value { get; private set; }
        internal LMCCallbackProtocolError Error { get; private set; }
        internal bool IsAccepted
        {
            get { return Error == LMCCallbackProtocolError.None; }
        }

        internal static LMCCallbackParseResult<T> Accept(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return new LMCCallbackParseResult<T>(
                value,
                LMCCallbackProtocolError.None);
        }

        internal static LMCCallbackParseResult<T> Reject(
            LMCCallbackProtocolError error)
        {
            if (error == LMCCallbackProtocolError.None)
            {
                throw new ArgumentOutOfRangeException("error");
            }

            return new LMCCallbackParseResult<T>(null, error);
        }
    }

    public enum LMCCallbackFenceDecisionKind
    {
        AcceptedWakeHint = 0,
        Malformed,
        UnexpectedSourceAddress,
        StaleListenerGeneration,
        StaleBootId,
        StaleSessionEpoch,
        StaleCookie,
        DuplicateSequence,
        OutOfOrderSequence
    }

    public sealed class LMCCallbackFenceDecision
    {
        internal LMCCallbackFenceDecision(
            LMCCallbackFenceDecisionKind kind,
            LMCCallbackProtocolError protocolError,
            LMCCallbackWakeHint wakeHint)
        {
            Kind = kind;
            ProtocolError = protocolError;
            WakeHint = wakeHint;
        }

        public LMCCallbackFenceDecisionKind Kind { get; private set; }
        public LMCCallbackProtocolError ProtocolError { get; private set; }
        public LMCCallbackWakeHint WakeHint { get; private set; }
        public bool IsAccepted
        {
            get { return Kind == LMCCallbackFenceDecisionKind.AcceptedWakeHint; }
        }
    }

    /// <summary>
    /// Immutable receiver-side version-2 callback counter evidence captured
    /// immediately after one datagram decision is committed to the current
    /// connection session.
    /// </summary>
    public sealed class LMCCallbackV2StatisticsChangedEventArgs : EventArgs
    {
        internal LMCCallbackV2StatisticsChangedEventArgs(
            LMCCallbackFenceDecisionKind decisionKind,
            LMCCallbackProtocolError protocolError,
            long acceptedWakeHintCount,
            long rejectedCount,
            long duplicateWakeHintCount,
            long outOfOrderWakeHintCount,
            LMCConnection ownerConnection,
            long connectionLifetimeGeneration,
            long sessionGeneration)
        {
            DecisionKind = decisionKind;
            ProtocolError = protocolError;
            AcceptedWakeHintCount = acceptedWakeHintCount;
            RejectedCount = rejectedCount;
            DuplicateWakeHintCount = duplicateWakeHintCount;
            OutOfOrderWakeHintCount = outOfOrderWakeHintCount;
            this.ownerConnection = ownerConnection;
            this.connectionLifetimeGeneration =
                connectionLifetimeGeneration;
            this.sessionGeneration = sessionGeneration;
        }

        private readonly LMCConnection ownerConnection;
        private readonly long connectionLifetimeGeneration;
        private readonly long sessionGeneration;

        public LMCCallbackFenceDecisionKind DecisionKind { get; private set; }
        public LMCCallbackProtocolError ProtocolError { get; private set; }
        public long AcceptedWakeHintCount { get; private set; }
        public long RejectedCount { get; private set; }
        public long DuplicateWakeHintCount { get; private set; }
        public long OutOfOrderWakeHintCount { get; private set; }
        public long SessionGeneration
        {
            get { return sessionGeneration; }
        }

        public bool BelongsTo(LMCConnection connection)
        {
            return connection != null
                && ReferenceEquals(ownerConnection, connection);
        }

        public bool BelongsToCurrentSession(LMCConnection connection)
        {
            return BelongsTo(connection)
                && connection.IsCurrentCallbackV2Session(
                    connectionLifetimeGeneration,
                    sessionGeneration);
        }
    }
}

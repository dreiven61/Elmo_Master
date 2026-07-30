using System;
using System.IO;

namespace LasalMotionControlLib
{
    public enum LMCLookupTargetKind
    {
        Axis = 1,
        Group = 2
    }

    /// <summary>
    /// Immutable successful LASAL object lookup result. A successful lookup
    /// always contains the exact six-byte lookup payload and a non-zero
    /// dispatcher reference.
    /// </summary>
    public sealed class LMCLookupResult
    {
        internal LMCLookupResult(
            LMCLookupTargetKind targetKind,
            string objectName,
            ushort reference,
            LMC_Response response)
        {
            if (targetKind != LMCLookupTargetKind.Axis
                && targetKind != LMCLookupTargetKind.Group)
            {
                throw new ArgumentOutOfRangeException("targetKind");
            }

            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentException(
                    "The LASAL object name must not be empty.",
                    "objectName");
            }

            if (reference == 0)
            {
                throw new ArgumentOutOfRangeException("reference");
            }

            if (response == null
                || !response.IsFrameValid
                || !response.IsSuccess
                || response.PayloadLength != 6
                || response.Payload.Length != 6
                || LMC_Frame.ReadUInt16(response.Payload, 4) != reference)
            {
                throw new ArgumentException(
                    "A lookup result requires an exact successful six-byte response.",
                    "response");
            }

            TargetKind = targetKind;
            ObjectName = objectName;
            Reference = reference;
            Response = response;
        }

        public LMCLookupTargetKind TargetKind { get; private set; }
        public string ObjectName { get; private set; }
        public ushort Reference { get; private set; }
        public LMC_Response Response { get; private set; }
    }

    /// <summary>
    /// Structured lookup failure. The parsed response and a defensive copy of
    /// the original bytes are retained so callers do not have to parse the
    /// diagnostic message.
    /// </summary>
    public sealed class LMCLookupException : InvalidOperationException
    {
        private readonly byte[] rawResponse;

        internal LMCLookupException(
            LMCLookupTargetKind targetKind,
            string objectName,
            LMC_Response response,
            bool hasLookupPayload,
            ushort lookupReference,
            string message)
            : base(message)
        {
            if (targetKind != LMCLookupTargetKind.Axis
                && targetKind != LMCLookupTargetKind.Group)
            {
                throw new ArgumentOutOfRangeException("targetKind");
            }

            TargetKind = targetKind;
            ObjectName = objectName;
            Response = response ?? throw new ArgumentNullException("response");
            HasLookupPayload = hasLookupPayload;
            LookupReference = lookupReference;
            rawResponse = response.Raw;
        }

        public LMCLookupTargetKind TargetKind { get; private set; }
        public string ObjectName { get; private set; }
        public LMC_Response Response { get; private set; }
        public bool HasLookupPayload { get; private set; }
        public ushort LookupReference { get; private set; }

        public byte[] RawResponse
        {
            get { return (byte[])rawResponse.Clone(); }
        }
    }

    public enum LMCConnectionState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Closing = 3,
        Faulted = 4
    }

    /// <summary>
    /// Evidence produced when a caller deliberately invalidates the current
    /// RPC transport so a safety command can be issued in a fresh session.
    /// The abort never sends an RPC Close frame and never sends the safety
    /// command itself.
    /// </summary>
    public sealed class LMCSafetyPreemptionAbortEvidence
    {
        internal LMCSafetyPreemptionAbortEvidence(
            long sessionGeneration,
            LMCConnectionState stateBefore,
            LMCConnectionState stateAfter,
            bool transportDetached,
            bool abortiveLingerApplied,
            bool faultStatePublished)
        {
            SessionGeneration = sessionGeneration;
            StateBefore = stateBefore;
            StateAfter = stateAfter;
            TransportDetached = transportDetached;
            AbortiveLingerApplied = abortiveLingerApplied;
            FaultStatePublished = faultStatePublished;
        }

        /// <summary>
        /// Session generation that was current when preemption was requested.
        /// Objects bound to this generation must not be reused after reconnect.
        /// </summary>
        public long SessionGeneration { get; private set; }

        public LMCConnectionState StateBefore { get; private set; }
        public LMCConnectionState StateAfter { get; private set; }

        /// <summary>
        /// True only when this call detached and closed a published TCP client.
        /// False means the transport had already been detached by another fault
        /// or deadline path; reconnect is still required before a safety send.
        /// </summary>
        public bool TransportDetached { get; private set; }

        /// <summary>
        /// True when zero-time linger was successfully applied before close.
        /// A false value does not mean the local transport remained open.
        /// </summary>
        public bool AbortiveLingerApplied { get; private set; }

        /// <summary>
        /// True when this call also published the typed Faulted connection
        /// state. False is expected when a concurrent Close or reconnect has
        /// already advanced lifecycle ownership; TransportDetached remains the
        /// authoritative local-close evidence.
        /// </summary>
        public bool FaultStatePublished { get; private set; }

        /// <summary>
        /// Safety preemption always requires a fresh RPC session and fresh
        /// object lookup before the safety command is sent exactly once.
        /// </summary>
        public bool ReconnectRequired
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Typed connection fault published after deliberate safety preemption.
    /// This is a local transport outcome only; it does not prove that any
    /// subsequent safety command was accepted or completed by the PLC.
    /// </summary>
    public sealed class LMCSafetyPreemptionTransportAbortedException
        : IOException
    {
        internal LMCSafetyPreemptionTransportAbortedException(
            long sessionGeneration)
            : base(
                "The RPC transport for session generation "
                + sessionGeneration
                + " was aborted for safety preemption. Reconnect, reacquire "
                + "the exact object identity, and issue the safety command "
                + "once in the fresh session.")
        {
            SessionGeneration = sessionGeneration;
        }

        public long SessionGeneration { get; private set; }
    }

    /// <summary>
    /// The caller tried to abort a different RPC session from the one that is
    /// currently published. No transport is detached in this case.
    /// </summary>
    public sealed class LMCSafetyPreemptionSessionMismatchException
        : InvalidOperationException
    {
        internal LMCSafetyPreemptionSessionMismatchException(
            long expectedSessionGeneration,
            long observedSessionGeneration)
            : base(
                "Safety preemption expected RPC session generation "
                + expectedSessionGeneration
                + " but observed "
                + observedSessionGeneration
                + ". No transport was detached.")
        {
            ExpectedSessionGeneration = expectedSessionGeneration;
            ObservedSessionGeneration = observedSessionGeneration;
        }

        public long ExpectedSessionGeneration { get; private set; }
        public long ObservedSessionGeneration { get; private set; }
    }

    public sealed class LMCConnectionStateChangedEventArgs : EventArgs
    {
        internal LMCConnectionStateChangedEventArgs(
            LMCConnectionState previousState,
            LMCConnectionState currentState,
            Exception exception)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Exception = exception;
        }

        public LMCConnectionState PreviousState { get; private set; }
        public LMCConnectionState CurrentState { get; private set; }
        public Exception Exception { get; private set; }
    }

    public sealed class LMCConnectionOptions
    {
        public LMCConnectionOptions()
        {
            ConnectTimeoutMilliseconds = 3000;
            ReceiveTimeoutMilliseconds = 3000;
            SendTimeoutMilliseconds = 3000;
            CallbackThreadJoinTimeoutMilliseconds = 500;
            ValidateCallbackSourceAddress = true;
        }

        public int ConnectTimeoutMilliseconds { get; set; }
        public int ReceiveTimeoutMilliseconds { get; set; }
        public int SendTimeoutMilliseconds { get; set; }
        public int CallbackThreadJoinTimeoutMilliseconds { get; set; }
        public bool ValidateCallbackSourceAddress { get; set; }
        public LMCSendPriorityCoordinator SendPriorityCoordinator
        {
            get;
            set;
        }

        internal Action SessionReservedBeforeClientPublishObserver
        {
            get;
            set;
        }

        internal Action ClientPublishedBeforeSessionBindObserver
        {
            get;
            set;
        }

        internal LMCConnectionOptions CloneAndValidate()
        {
            ValidatePositiveTimeout(
                ConnectTimeoutMilliseconds,
                "ConnectTimeoutMilliseconds");
            ValidatePositiveTimeout(
                ReceiveTimeoutMilliseconds,
                "ReceiveTimeoutMilliseconds");
            ValidatePositiveTimeout(
                SendTimeoutMilliseconds,
                "SendTimeoutMilliseconds");
            ValidatePositiveTimeout(
                CallbackThreadJoinTimeoutMilliseconds,
                "CallbackThreadJoinTimeoutMilliseconds");

            return new LMCConnectionOptions
            {
                ConnectTimeoutMilliseconds = ConnectTimeoutMilliseconds,
                ReceiveTimeoutMilliseconds = ReceiveTimeoutMilliseconds,
                SendTimeoutMilliseconds = SendTimeoutMilliseconds,
                CallbackThreadJoinTimeoutMilliseconds =
                    CallbackThreadJoinTimeoutMilliseconds,
                ValidateCallbackSourceAddress = ValidateCallbackSourceAddress,
                SendPriorityCoordinator = SendPriorityCoordinator,
                SessionReservedBeforeClientPublishObserver =
                    SessionReservedBeforeClientPublishObserver,
                ClientPublishedBeforeSessionBindObserver =
                    ClientPublishedBeforeSessionBindObserver
            };
        }

        private static void ValidatePositiveTimeout(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Timeouts must be greater than zero milliseconds.");
            }
        }
    }
}

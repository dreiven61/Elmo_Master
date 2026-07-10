using System;

namespace LasalMotionControlLib
{
    public enum LMCConnectionState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Closing = 3,
        Faulted = 4
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
                ValidateCallbackSourceAddress = ValidateCallbackSourceAddress
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

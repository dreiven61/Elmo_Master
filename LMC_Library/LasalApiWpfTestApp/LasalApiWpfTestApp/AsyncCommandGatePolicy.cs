using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlApiExample
{
    internal sealed class AsyncCommandGateTimeoutException : TimeoutException
    {
        internal AsyncCommandGateTimeoutException(
            string operation,
            int timeoutMilliseconds)
            : base(
                operation
                + " did not acquire the command send gate within "
                + timeoutMilliseconds
                + " ms. No command was sent by this queued operation.")
        {
            Operation = operation;
            TimeoutMilliseconds = timeoutMilliseconds;
        }

        internal string Operation { get; private set; }
        internal int TimeoutMilliseconds { get; private set; }
    }

    internal static class AsyncCommandGatePolicy
    {
        internal const int OrdinaryGateTimeoutMilliseconds = 1000;
        internal const int SafetyGateGraceMilliseconds = 250;

        internal static async Task WaitAsync(
            SemaphoreSlim gate,
            string operation,
            int timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(nameof(gate));
            }

            if (string.IsNullOrWhiteSpace(operation))
            {
                throw new ArgumentException(
                    "An operation name is required.",
                    nameof(operation));
            }

            if (timeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timeoutMilliseconds));
            }

            if (!await gate.WaitAsync(
                    timeoutMilliseconds,
                    cancellationToken).ConfigureAwait(false))
            {
                throw new AsyncCommandGateTimeoutException(
                    operation,
                    timeoutMilliseconds);
            }
        }

        internal static Task<bool> TryWaitAsync(
            SemaphoreSlim gate,
            int timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(nameof(gate));
            }

            if (timeoutMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timeoutMilliseconds));
            }

            return gate.WaitAsync(timeoutMilliseconds, cancellationToken);
        }
    }
}

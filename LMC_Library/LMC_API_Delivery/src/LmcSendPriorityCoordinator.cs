using System;
using System.Globalization;
using System.Threading;

namespace LasalMotionControlLib
{
    public enum LMCSendPreemptionPhase
    {
        BeforeWire = 0,
        ResultDiscarded = 1
    }

    /// <summary>
    /// Coordinates application-level priority sends without cancelling an RPC
    /// that already crossed its final pre-transmission check. Preemptible
    /// scopes flow through async continuations and Task.Run via AsyncLocal.
    /// </summary>
    public sealed class LMCSendPriorityCoordinator
    {
        private static readonly AsyncLocal<ScopeFrame> CurrentScope =
            new AsyncLocal<ScopeFrame>();

        private readonly object resultPublicationSync = new object();
        private long generation;

        public long CurrentGeneration
        {
            get { return Interlocked.Read(ref generation); }
        }

        /// <summary>
        /// Reserves a priority send. Call this before awaiting any application
        /// gate so older preemptible scopes become stale immediately.
        /// </summary>
        public long ReservePrioritySend()
        {
            lock (resultPublicationSync)
            {
                return Interlocked.Increment(ref generation);
            }
        }

        /// <summary>
        /// Applies one captured generation to every RPC made by the current
        /// logical async flow, including compound SDK operations.
        /// </summary>
        public IDisposable BeginPreemptibleScope(
            long expectedGeneration,
            string operation)
        {
            if (expectedGeneration < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "expectedGeneration");
            }

            return PushScope(
                expectedGeneration,
                RequireOperation(operation),
                false);
        }

        /// <summary>
        /// Marks the current logical flow as the priority send associated with
        /// the latest reservation. This overrides an inherited preemptible
        /// scope while that reservation remains current.
        /// </summary>
        public IDisposable BeginPriorityScope(
            long reservedGeneration,
            string operation)
        {
            var actualGeneration = CurrentGeneration;
            if (reservedGeneration <= 0
                || reservedGeneration != actualGeneration)
            {
                throw new ArgumentOutOfRangeException(
                    "reservedGeneration",
                    "A priority scope requires the latest completed priority reservation.");
            }

            return PushScope(
                reservedGeneration,
                RequireOperation(operation),
                true);
        }

        internal void ValidateBeforeWrite(ushort command)
        {
            ValidateCurrentScope(
                command,
                LMCSendPreemptionPhase.BeforeWire);
        }

        internal void PublishResult(ushort command, Action publish)
        {
            if (publish == null)
            {
                throw new ArgumentNullException("publish");
            }

            lock (resultPublicationSync)
            {
                ValidateCurrentScope(
                    command,
                    LMCSendPreemptionPhase.ResultDiscarded);
                publish();
            }
        }

        private void ValidateCurrentScope(
            ushort command,
            LMCSendPreemptionPhase phase)
        {
            var frame = CurrentScope.Value;
            var actualGeneration = CurrentGeneration;
            while (frame != null)
            {
                if (!ReferenceEquals(frame.Coordinator, this))
                {
                    frame = frame.Parent;
                    continue;
                }

                // Only the latest explicit priority reservation may override
                // an inherited stale preemptible scope. A newer reservation
                // also preempts an older priority sender before it writes.
                if (frame.IsPriority)
                {
                    if (frame.ExpectedGeneration == actualGeneration)
                    {
                        return;
                    }

                    throw new LMCSendPreemptedException(
                        frame.Operation,
                        command,
                        frame.ExpectedGeneration,
                        actualGeneration,
                        phase);
                }

                if (frame.ExpectedGeneration != actualGeneration)
                {
                    throw new LMCSendPreemptedException(
                        frame.Operation,
                        command,
                        frame.ExpectedGeneration,
                        actualGeneration,
                        phase);
                }

                frame = frame.Parent;
            }

            // The coordinator is optional. Unscoped SDK users retain the
            // original transport behavior.
        }

        private IDisposable PushScope(
            long expectedGeneration,
            string operation,
            bool isPriority)
        {
            var frame = new ScopeFrame(
                this,
                expectedGeneration,
                operation,
                isPriority,
                CurrentScope.Value);
            CurrentScope.Value = frame;
            return new ScopeLease(frame);
        }

        private static string RequireOperation(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
            {
                throw new ArgumentException(
                    "An operation name is required.",
                    "operation");
            }

            return operation;
        }

        private sealed class ScopeFrame
        {
            internal ScopeFrame(
                LMCSendPriorityCoordinator coordinator,
                long expectedGeneration,
                string operation,
                bool isPriority,
                ScopeFrame parent)
            {
                Coordinator = coordinator;
                ExpectedGeneration = expectedGeneration;
                Operation = operation;
                IsPriority = isPriority;
                Parent = parent;
            }

            internal LMCSendPriorityCoordinator Coordinator { get; private set; }
            internal long ExpectedGeneration { get; private set; }
            internal string Operation { get; private set; }
            internal bool IsPriority { get; private set; }
            internal ScopeFrame Parent { get; private set; }
        }

        private sealed class ScopeLease : IDisposable
        {
            private ScopeFrame frame;

            internal ScopeLease(ScopeFrame frame)
            {
                this.frame = frame;
            }

            public void Dispose()
            {
                var disposing = Volatile.Read(ref frame);
                if (disposing == null)
                {
                    return;
                }

                if (!ReferenceEquals(CurrentScope.Value, disposing))
                {
                    throw new InvalidOperationException(
                        "Send-priority scopes must be disposed in LIFO order on their owning async flow.");
                }

                // Do not consume the lease until its owning flow can actually
                // pop it. A caller that disposes an outer scope too early may
                // then dispose the inner scope and retry the outer scope
                // without leaving a stale AsyncLocal frame behind.
                disposing = Interlocked.Exchange(ref frame, null);
                if (disposing == null)
                {
                    return;
                }

                CurrentScope.Value = disposing.Parent;
            }
        }
    }

    /// <summary>
    /// Indicates that a scoped RPC was rejected before wire transmission or
    /// that its already-drained response was discarded before publication
    /// because a newer priority send had been reserved.
    /// </summary>
    public sealed class LMCSendPreemptedException : InvalidOperationException
    {
        internal LMCSendPreemptedException(
            string operation,
            ushort command,
            long expectedGeneration,
            long actualGeneration)
            : this(
                operation,
                command,
                expectedGeneration,
                actualGeneration,
                LMCSendPreemptionPhase.BeforeWire)
        {
        }

        internal LMCSendPreemptedException(
            string operation,
            ushort command,
            long expectedGeneration,
            long actualGeneration,
            LMCSendPreemptionPhase phase)
            : base(CreateMessage(operation, command, phase))
        {
            Operation = operation;
            Command = command;
            ExpectedGeneration = expectedGeneration;
            ActualGeneration = actualGeneration;
            Phase = phase;
        }

        public string Operation { get; private set; }
        public ushort Command { get; private set; }
        public long ExpectedGeneration { get; private set; }
        public long ActualGeneration { get; private set; }
        public LMCSendPreemptionPhase Phase { get; private set; }

        private static string CreateMessage(
            string operation,
            ushort command,
            LMCSendPreemptionPhase phase)
        {
            var commandText = command.ToString(
                "X4",
                CultureInfo.InvariantCulture);
            if (phase == LMCSendPreemptionPhase.ResultDiscarded)
            {
                return operation
                    + " response for command 0x"
                    + commandText
                    + " was discarded because a newer Stop or Power Off request was reserved after transmission.";
            }

            return operation
                + " was cancelled before command 0x"
                + commandText
                + " transmission because a newer Stop or Power Off request was reserved.";
        }
    }
}

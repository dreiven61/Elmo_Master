using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        public const ushort ProtocolSchemaVersion =
            LMC_DiagnosticsFrame.SchemaVersion;

        private readonly LMCConnection connection;
        private int requestSequence;
        private long capabilityObservationSequence;

        internal LMCDiagnostics(LMCConnection connection)
        {
            this.connection = connection
                ?? throw new ArgumentNullException("connection");
        }

        public LMCDiagnosticCapabilities GetCapabilities()
        {
            var sessionGeneration = connection.SessionGeneration;
            return GetCapabilities(sessionGeneration);
        }

        private LMCDiagnosticCapabilities GetCapabilities(
            long sessionGeneration)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.GetDiagnosticsCapabilities(requestId),
                sessionGeneration);
            var capabilities = LMC_DiagnosticsParser.ParseCapabilities(
                raw,
                requestId,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return capabilities.BindProvenance(
                this,
                sessionGeneration,
                NextCapabilityObservationSequence());
        }

        public async Task<LMCDiagnosticCapabilities> GetCapabilitiesAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            return await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<LMCDiagnosticCapabilities> GetCapabilitiesAsync(
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.GetDiagnosticsCapabilities(requestId),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var capabilities = LMC_DiagnosticsParser.ParseCapabilities(
                raw,
                requestId,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return capabilities.BindProvenance(
                this,
                sessionGeneration,
                NextCapabilityObservationSequence());
        }

        private uint NextRequestId()
        {
            uint requestId;

            do
            {
                requestId = unchecked(
                    (uint)Interlocked.Increment(ref requestSequence));
            }
            while (requestId == 0);

            return requestId;
        }

        private long NextCapabilityObservationSequence()
        {
            var sequence = Interlocked.Increment(
                ref capabilityObservationSequence);
            if (sequence <= 0)
            {
                throw new InvalidOperationException(
                    "Diagnostics capability observation sequence overflowed.");
            }

            return sequence;
        }

        internal long CurrentCapabilityObservationSequence
        {
            get
            {
                return Interlocked.Read(
                    ref capabilityObservationSequence);
            }
        }

        private static Task<T> RunStateMutatingAsync<T>(
            Func<T> operation,
            CancellationToken cancellationToken)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return operation();
                },
                CancellationToken.None);
        }

        private static Task RunStateMutatingAsync(
            Action operation,
            CancellationToken cancellationToken)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    operation();
                },
                CancellationToken.None);
        }
    }
}

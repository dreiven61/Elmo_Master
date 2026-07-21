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

        internal LMCDiagnostics(LMCConnection connection)
        {
            this.connection = connection
                ?? throw new ArgumentNullException("connection");
        }

        public LMCDiagnosticCapabilities GetCapabilities()
        {
            var sessionGeneration = connection.SessionGeneration;
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
            return capabilities;
        }

        public async Task<LMCDiagnosticCapabilities> GetCapabilitiesAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
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
            return capabilities;
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

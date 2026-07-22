using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    /// <summary>
    /// Read-only LASAL-local administrative API. These commands do not create
    /// motion and do not expose native MotionLib enum values on the wire.
    /// </summary>
    public sealed class LMCAdmin
    {
        public const ushort ProtocolSchemaVersion =
            LMC_AdminFrame.SchemaVersion;

        private readonly LMCConnection connection;
        private int requestSequence;

        internal LMCAdmin(LMCConnection connection)
        {
            this.connection = connection
                ?? throw new ArgumentNullException("connection");
        }

        public LMCAdminCapabilities GetCapabilities()
        {
            var sessionGeneration = connection.SessionGeneration;
            return GetCapabilitiesCore(sessionGeneration);
        }

        private LMCAdminCapabilities GetCapabilitiesCore(
            long sessionGeneration)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.GetCapabilities(requestId),
                sessionGeneration);
            var result = LMC_AdminParser.ParseCapabilities(
                raw,
                requestId,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return result;
        }

        public async Task<LMCAdminCapabilities> GetCapabilitiesAsync(
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            return await GetCapabilitiesCoreAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<LMCAdminCapabilities> GetCapabilitiesCoreAsync(
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            connection.EnsureSessionGeneration(sessionGeneration);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.GetCapabilities(requestId),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var result = LMC_AdminParser.ParseCapabilities(
                raw,
                requestId,
                sessionGeneration);
            connection.EnsureSessionGeneration(sessionGeneration);
            return result;
        }

        public LMCAxisParameterResult ReadAxisParameter(
            ushort axisReference,
            LMCAxisParameterKey key)
        {
            var sessionGeneration = connection.SessionGeneration;
            return ReadAxisParameterCore(
                axisReference,
                key,
                sessionGeneration);
        }

        private LMCAxisParameterResult ReadAxisParameterCore(
            ushort axisReference,
            LMCAxisParameterKey key,
            long sessionGeneration)
        {
            LMC_AdminFrame.ValidateAxisReference(axisReference);
            LMC_AdminFrame.ValidateAxisParameterKey(key);

            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilitiesCore(sessionGeneration);
            ValidateAxisCapabilities(
                capabilities,
                sessionGeneration,
                axisReference,
                key);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.ReadAxisParameter(
                    requestId,
                    axisReference,
                    key),
                sessionGeneration);
            var result = LMC_AdminParser.ParseAxisParameter(
                raw,
                requestId,
                axisReference,
                key);
            connection.EnsureSessionGeneration(sessionGeneration);
            return result;
        }

        public LMCAxisParameterResult ReadAxisParameter(
            LMCSingleAxis axis,
            LMCAxisParameterKey key)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            return ReadAxisParameterCore(
                axis.AxisReference,
                key,
                sessionGeneration);
        }

        public Task<LMCAxisParameterResult> ReadAxisParameterAsync(
            ushort axisReference,
            LMCAxisParameterKey key,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            return ReadAxisParameterCoreAsync(
                axisReference,
                key,
                sessionGeneration,
                cancellationToken);
        }

        private async Task<LMCAxisParameterResult> ReadAxisParameterCoreAsync(
            ushort axisReference,
            LMCAxisParameterKey key,
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            LMC_AdminFrame.ValidateAxisReference(axisReference);
            LMC_AdminFrame.ValidateAxisParameterKey(key);

            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesCoreAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateAxisCapabilities(
                capabilities,
                sessionGeneration,
                axisReference,
                key);

            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.ReadAxisParameter(
                    requestId,
                    axisReference,
                    key),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var result = LMC_AdminParser.ParseAxisParameter(
                raw,
                requestId,
                axisReference,
                key);
            connection.EnsureSessionGeneration(sessionGeneration);
            return result;
        }

        public Task<LMCAxisParameterResult> ReadAxisParameterAsync(
            LMCSingleAxis axis,
            LMCAxisParameterKey key,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateAxisOwner(axis);
            return ReadAxisParameterCoreAsync(
                axis.AxisReference,
                key,
                sessionGeneration,
                cancellationToken);
        }

        public LMCGroupParametersResult ReadGroupParameters(
            ushort groupReference,
            LMCGroupParameterSelection selection)
        {
            var sessionGeneration = connection.SessionGeneration;
            return ReadGroupParametersCore(
                groupReference,
                selection,
                sessionGeneration);
        }

        private LMCGroupParametersResult ReadGroupParametersCore(
            ushort groupReference,
            LMCGroupParameterSelection selection,
            long sessionGeneration)
        {
            LMC_AdminFrame.ValidateGroupReference(groupReference);
            LMC_AdminFrame.ValidateGroupSelection(selection);

            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilitiesCore(sessionGeneration);
            ValidateGroupCapabilities(
                capabilities,
                sessionGeneration,
                groupReference,
                selection);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_AdminFrame.ReadGroupParameters(
                    requestId,
                    groupReference,
                    selection),
                sessionGeneration);
            var result = LMC_AdminParser.ParseGroupParameters(
                raw,
                requestId,
                groupReference,
                selection);
            connection.EnsureSessionGeneration(sessionGeneration);
            return result;
        }

        public LMCGroupParametersResult ReadGroupParameters(
            LMCGroupAxis group,
            LMCGroupParameterSelection selection)
        {
            var sessionGeneration = ValidateGroupOwner(group);
            return ReadGroupParametersCore(
                group.GroupReference,
                selection,
                sessionGeneration);
        }

        public Task<LMCGroupParametersResult> ReadGroupParametersAsync(
            ushort groupReference,
            LMCGroupParameterSelection selection,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = connection.SessionGeneration;
            return ReadGroupParametersCoreAsync(
                groupReference,
                selection,
                sessionGeneration,
                cancellationToken);
        }

        private async Task<LMCGroupParametersResult> ReadGroupParametersCoreAsync(
            ushort groupReference,
            LMCGroupParameterSelection selection,
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            LMC_AdminFrame.ValidateGroupReference(groupReference);
            LMC_AdminFrame.ValidateGroupSelection(selection);

            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesCoreAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateGroupCapabilities(
                capabilities,
                sessionGeneration,
                groupReference,
                selection);

            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_AdminFrame.ReadGroupParameters(
                    requestId,
                    groupReference,
                    selection),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var result = LMC_AdminParser.ParseGroupParameters(
                raw,
                requestId,
                groupReference,
                selection);
            connection.EnsureSessionGeneration(sessionGeneration);
            return result;
        }

        public Task<LMCGroupParametersResult> ReadGroupParametersAsync(
            LMCGroupAxis group,
            LMCGroupParameterSelection selection,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateGroupOwner(group);
            return ReadGroupParametersCoreAsync(
                group.GroupReference,
                selection,
                sessionGeneration,
                cancellationToken);
        }

        private long ValidateAxisOwner(LMCSingleAxis axis)
        {
            if (axis == null)
            {
                throw new ArgumentNullException("axis");
            }

            if (!ReferenceEquals(axis.Connection, connection))
            {
                throw new InvalidOperationException(
                    "The axis belongs to another or stale connection session.");
            }

            var sessionGeneration = axis.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private long ValidateGroupOwner(LMCGroupAxis group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            if (!ReferenceEquals(group.Connection, connection))
            {
                throw new InvalidOperationException(
                    "The group belongs to another or stale connection session.");
            }

            var sessionGeneration = group.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private void ValidateAxisCapabilities(
            LMCAdminCapabilities capabilities,
            long expectedSessionGeneration,
            ushort axisReference,
            LMCAxisParameterKey key)
        {
            if (capabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration
                || !capabilities.Supports(
                    LMCAdminFeature.AxisParameterRead)
                || axisReference > capabilities.PhysicalAxisCount
                || capabilities.MaxAxisParameterCount != 1
                || !capabilities.Supports(key))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise this axis parameter read.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateGroupCapabilities(
            LMCAdminCapabilities capabilities,
            long expectedSessionGeneration,
            ushort groupReference,
            LMCGroupParameterSelection selection)
        {
            if (capabilities.ConnectionSessionGeneration
                    != expectedSessionGeneration
                || !capabilities.Supports(
                    LMCAdminFeature.GroupParameterRead)
                || groupReference != capabilities.GroupReference
                || CountSelectedGroupParameters(selection)
                    > capabilities.MaxGroupParameterCount
                || !capabilities.Supports(selection))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise this group parameter read.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private static int CountSelectedGroupParameters(
            LMCGroupParameterSelection selection)
        {
            var value = (uint)selection;
            var count = 0;
            while (value != 0)
            {
                count += (int)(value & 1u);
                value >>= 1;
            }

            return count;
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
    }
}

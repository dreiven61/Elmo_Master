using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        private readonly object recorderBootIdSync = new object();
        private bool hasRecorderBootId;
        private long recorderBootIdSessionGeneration;
        private uint recorderBootId;

        public LMCRecorderConfigurationHandle ConfigureRecorder(
            LMCRecorderConfiguration configuration)
        {
            return ConfigureRecorderCore(
                configuration,
                connection.SessionGeneration);
        }

        private LMCRecorderConfigurationHandle ConfigureRecorderCore(
            LMCRecorderConfiguration configuration,
            long sessionGeneration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateRecorderCapabilities(
                capabilities,
                sessionGeneration,
                configuration);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ConfigureRecorder(
                    requestId,
                    capabilities.MapRevision,
                    configuration,
                    capabilities.DiagnosticsBootId),
                sessionGeneration);

            var handle = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseConfigureRecorder(
                    raw,
                    requestId,
                    configuration,
                    capabilities,
                    sessionGeneration,
                    this));

            connection.EnsureSessionGeneration(sessionGeneration);
            RememberRecorderBootId(
                sessionGeneration,
                handle.DiagnosticsBootId);
            return handle;
        }

        public async Task<LMCRecorderConfigurationHandle> ConfigureRecorderAsync(
            LMCRecorderConfiguration configuration,
            CancellationToken cancellationToken)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ConfigureRecorderCore(
                        configuration,
                        sessionGeneration);
                },
                CancellationToken.None).ConfigureAwait(false);
        }

        public LMCRecorderIdentity StartRecorder(
            LMCRecorderConfigurationHandle configuration)
        {
            var sessionGeneration = ValidateRecorderConfiguration(configuration);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.StartRecorder(requestId, configuration),
                sessionGeneration);
            var identity = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseStartRecorder(
                    raw,
                    requestId,
                    configuration,
                    sessionGeneration,
                    this));
            connection.EnsureSessionGeneration(sessionGeneration);
            return identity;
        }

        public async Task<LMCRecorderIdentity> StartRecorderAsync(
            LMCRecorderConfigurationHandle configuration,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () => StartRecorder(configuration),
                CancellationToken.None).ConfigureAwait(false);
        }

        public void TriggerRecorder(LMCRecorderIdentity identity)
        {
            var sessionGeneration = ValidateTriggeredRecorderIdentity(identity);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.TriggerRecorder(requestId, identity),
                sessionGeneration);
            ParseRecorderResponse(
                sessionGeneration,
                () =>
                {
                    LMC_DiagnosticsParser.ParseTriggerRecorder(raw, requestId);
                });
            connection.EnsureSessionGeneration(sessionGeneration);
        }

        public async Task TriggerRecorderAsync(
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            await RunStateMutatingAsync(
                () => TriggerRecorder(identity),
                cancellationToken).ConfigureAwait(false);
        }

        public void StopRecorder(LMCRecorderIdentity identity)
        {
            var sessionGeneration = ValidateRecorderIdentity(identity, true);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.StopRecorder(requestId, identity),
                sessionGeneration);
            ParseRecorderResponse(
                sessionGeneration,
                () =>
                {
                    LMC_DiagnosticsParser.ParseStopRecorder(raw, requestId);
                });
            connection.EnsureSessionGeneration(sessionGeneration);
        }

        public async Task StopRecorderAsync(
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(
                () => StopRecorder(identity),
                CancellationToken.None).ConfigureAwait(false);
        }

        public LMCRecorderStatus GetRecorderStatus(
            LMCRecorderIdentity identity)
        {
            var sessionGeneration = ValidateRecorderIdentity(identity, false);
            return ReadRecorderStatusCore(identity, sessionGeneration);
        }

        public async Task<LMCRecorderStatus> GetRecorderStatusAsync(
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateRecorderIdentity(identity, false);
            return await ReadRecorderStatusCoreAsync(
                identity,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
        }

        public LMCRecorderHeader GetRecorderHeader(
            LMCRecorderIdentity identity)
        {
            var sessionGeneration = ValidateRecorderIdentity(identity, false);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadRecorderHeader(requestId, identity),
                sessionGeneration);
            var header = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderHeader(
                    raw,
                    requestId,
                    identity));
            identity.ApplyHeaderMetadata(header);
            connection.EnsureSessionGeneration(sessionGeneration);
            return header;
        }

        public async Task<LMCRecorderHeader> GetRecorderHeaderAsync(
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateRecorderIdentity(identity, false);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadRecorderHeader(requestId, identity),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var header = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderHeader(
                    raw,
                    requestId,
                    identity));
            identity.ApplyHeaderMetadata(header);
            connection.EnsureSessionGeneration(sessionGeneration);
            return header;
        }

        public LMCRecorderChunk ReadRecorderChunk(
            LMCRecorderChunkRequest request)
        {
            ValidateRecorderChunkRequest(request);
            var sessionGeneration = ValidateRecorderIdentity(
                request.Identity,
                false);
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadRecorderChunk(requestId, request),
                sessionGeneration);
            var chunk = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderChunk(
                    raw,
                    requestId,
                    request));
            connection.EnsureSessionGeneration(sessionGeneration);
            return chunk;
        }

        public async Task<LMCRecorderChunk> ReadRecorderChunkAsync(
            LMCRecorderChunkRequest request,
            CancellationToken cancellationToken)
        {
            ValidateRecorderChunkRequest(request);
            var sessionGeneration = ValidateRecorderIdentity(
                request.Identity,
                false);
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadRecorderChunk(requestId, request),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var chunk = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderChunk(
                    raw,
                    requestId,
                    request));
            connection.EnsureSessionGeneration(sessionGeneration);
            return chunk;
        }

        public void ReleaseRecorderBuffer(LMCRecorderIdentity identity)
        {
            var sessionGeneration = ValidateRecorderIdentity(identity, true);
            RequireRecorderConfigurationMetadataBeforeBufferRelease(identity);
            identity.BeginBufferRelease();
            try
            {
                var requestId = NextRequestId();
                var raw = connection.Exchange(
                    LMC_DiagnosticsFrame.ReleaseRecorderBuffer(
                        requestId,
                        identity),
                    sessionGeneration);
                ParseRecorderResponse(
                    sessionGeneration,
                    () =>
                    {
                        LMC_DiagnosticsParser.ParseReleaseRecorderBuffer(
                            raw,
                            requestId);
                    });
                connection.EnsureSessionGeneration(sessionGeneration);
                identity.CompleteBufferRelease();
            }
            catch
            {
                identity.CancelBufferRelease();
                throw;
            }
        }

        public async Task ReleaseRecorderBufferAsync(
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(
                () => ReleaseRecorderBuffer(identity),
                CancellationToken.None).ConfigureAwait(false);
        }

        public void ReleaseRecorder(
            LMCRecorderConfigurationHandle configuration)
        {
            var sessionGeneration = ValidateRecorderConfiguration(configuration);
            configuration.BeginRelease();
            try
            {
                var requestId = NextRequestId();
                var raw = connection.Exchange(
                    LMC_DiagnosticsFrame.ReleaseRecorder(
                        requestId,
                        configuration),
                    sessionGeneration);
                ParseRecorderResponse(
                    sessionGeneration,
                    () =>
                    {
                        LMC_DiagnosticsParser.ParseReleaseRecorder(
                            raw,
                            requestId);
                    });
                connection.EnsureSessionGeneration(sessionGeneration);
                configuration.CompleteRelease();
            }
            catch
            {
                configuration.CancelRelease();
                throw;
            }
        }

        public async Task ReleaseRecorderAsync(
            LMCRecorderConfigurationHandle configuration,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(
                () => ReleaseRecorder(configuration),
                CancellationToken.None).ConfigureAwait(false);
        }

        public void ReleaseRecorder(LMCRecorderIdentity identity)
        {
            var sessionGeneration =
                ValidateRecorderIdentityForConfigurationRelease(identity);
            identity.BeginRecorderRelease();
            try
            {
                var requestId = NextRequestId();
                var raw = connection.Exchange(
                    LMC_DiagnosticsFrame.ReleaseRecorder(requestId, identity),
                    sessionGeneration);
                ParseRecorderResponse(
                    sessionGeneration,
                    () =>
                    {
                        LMC_DiagnosticsParser.ParseReleaseRecorder(
                            raw,
                            requestId);
                    });
                connection.EnsureSessionGeneration(sessionGeneration);
                identity.CompleteRecorderRelease();
            }
            catch
            {
                identity.CancelRecorderRelease();
                throw;
            }
        }

        public async Task ReleaseRecorderAsync(
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(
                () => ReleaseRecorder(identity),
                CancellationToken.None).ConfigureAwait(false);
        }

        public LMCRecorderIdentity AdoptRecorder(
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId)
        {
            return AdoptRecorderCore(
                diagnosticsBootId,
                recordId,
                bufferId,
                connection.SessionGeneration);
        }

        private LMCRecorderIdentity AdoptRecorderCore(
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            long sessionGeneration)
        {
            ValidateRecorderAdoptionArguments(diagnosticsBootId, recordId);
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateRecorderAdoptionCapabilities(
                capabilities,
                sessionGeneration,
                diagnosticsBootId,
                bufferId);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.AdoptRecorder(
                    requestId,
                    diagnosticsBootId,
                    recordId,
                    bufferId),
                sessionGeneration);
            var adoption = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseAdoptRecorder(
                    raw,
                    requestId,
                    diagnosticsBootId,
                    recordId,
                    bufferId));
            RememberRecorderBootId(sessionGeneration, diagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return CreateAdoptedIdentity(
                adoption,
                capabilities,
                sessionGeneration);
        }

        public async Task<LMCRecorderIdentity> AdoptRecorderAsync(
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            CancellationToken cancellationToken)
        {
            ValidateRecorderAdoptionArguments(diagnosticsBootId, recordId);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return AdoptRecorderCore(
                        diagnosticsBootId,
                        recordId,
                        bufferId,
                        sessionGeneration);
                },
                CancellationToken.None).ConfigureAwait(false);
        }

        private LMCRecorderStatus ReadRecorderStatusCore(
            LMCRecorderIdentity identity,
            long sessionGeneration)
        {
            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadRecorderStatus(requestId, identity),
                sessionGeneration);
            var status = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    raw,
                    requestId,
                    identity));
            identity.ApplyStatusMetadata(status);
            connection.EnsureSessionGeneration(sessionGeneration);
            return status;
        }

        private async Task<LMCRecorderStatus> ReadRecorderStatusCoreAsync(
            LMCRecorderIdentity identity,
            long sessionGeneration,
            CancellationToken cancellationToken)
        {
            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadRecorderStatus(requestId, identity),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var status = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderStatus(
                    raw,
                    requestId,
                    identity));
            identity.ApplyStatusMetadata(status);
            connection.EnsureSessionGeneration(sessionGeneration);
            return status;
        }

        private void ValidateRecorderCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            LMCRecorderConfiguration configuration)
        {
            ValidateRecorderCapabilityEnvelope(
                capabilities,
                expectedSessionGeneration);

            if (!capabilities.Supports(
                LMCDiagnosticCapability.RecorderSingleBank))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise RecorderSingleBank diagnostics.");
            }

            if (configuration.RequiresTriggerCapability
                && !capabilities.Supports(
                    LMCDiagnosticCapability.RecorderTrigger))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise Recorder trigger diagnostics.");
            }

            if (configuration.RequiresDoubleBankCapability
                && !capabilities.Supports(
                    LMCDiagnosticCapability.RecorderDoubleBank))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise RecorderDoubleBank diagnostics.");
            }

            if (configuration.ChannelCount > capabilities.MaxRecorderChannels)
            {
                throw new ArgumentOutOfRangeException(
                    "configuration",
                    "The Recorder channel count exceeds the connected PLC capability.");
            }

            if (configuration.SampleCapacity > capabilities.MaxRecorderSamples)
            {
                throw new ArgumentOutOfRangeException(
                    "configuration",
                    "The Recorder sample capacity exceeds the connected PLC capability.");
            }

            var sampleStrideBytes = checked(
                (uint)configuration.ChannelCount * sizeof(uint));
            if (sampleStrideBytes > capabilities.MaxChunkDataBytes)
            {
                throw new InvalidDataException(
                    "MaxChunkDataBytes cannot carry one complete Recorder sample for the requested channel count.");
            }

            if (configuration.BufferMode == LMCRecorderBufferMode.Double
                && capabilities.RecorderBufferCount < 2)
            {
                throw new InvalidDataException(
                    "RecorderDoubleBank capability requires at least two Recorder banks.");
            }

            var requiredBankBytes = checked(
                (ulong)configuration.ChannelCount
                * sizeof(uint)
                * configuration.SampleCapacity);
            if (requiredBankBytes > capabilities.RecorderBytesPerBank)
            {
                throw new ArgumentOutOfRangeException(
                    "configuration",
                    "The Recorder configuration exceeds RecorderBytesPerBank.");
            }

            var configurePayloadBytes = checked(
                LMC_DiagnosticsFrame.ConfigureRecorderRequestHeaderPayloadLength
                + configuration.ChannelCount * sizeof(uint));
            var headerPayloadBytes = checked(
                LMC_DiagnosticsParser.RecorderHeaderResponseHeaderPayloadLength
                + configuration.ChannelCount * sizeof(uint));
            if (configurePayloadBytes > capabilities.MaxRequestPayloadBytes
                || headerPayloadBytes > capabilities.MaxResponsePayloadBytes)
            {
                throw new InvalidDataException(
                    "Recorder capability payload limits cannot carry the requested configuration.");
            }

            checked
            {
                var ignoredSamplePeriodUs =
                    (uint)configuration.SamplePeriodCycles
                    * capabilities.BaseCycleTimeUs;
                GC.KeepAlive(ignoredSamplePeriodUs);
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateRecorderCapabilityEnvelope(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (capabilities.ConnectionSessionGeneration
                != expectedSessionGeneration)
            {
                throw new InvalidOperationException(
                    "Diagnostics capabilities belong to a stale connection session.");
            }

            if (capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || capabilities.MaxRecorderChannels == 0
                || capabilities.MaxRecorderChannels
                    > LMC_DiagnosticsFrame.MaxRecorderChannelCount
                || capabilities.RecorderBufferCount == 0
                || capabilities.RecorderBufferCount > 2
                || capabilities.MaxRecorderSamples == 0
                || capabilities.BaseCycleTimeUs == 0
                || capabilities.MaxRequestPayloadBytes
                    < LMC_DiagnosticsFrame.ConfigureRecorderRequestHeaderPayloadLength
                        + sizeof(uint)
                || capabilities.MaxResponsePayloadBytes
                    < LMC_DiagnosticsParser.RecorderHeaderResponseHeaderPayloadLength
                        + sizeof(uint)
                || capabilities.MaxChunkDataBytes == 0
                || (capabilities.MaxChunkDataBytes & 3) != 0
                || capabilities.MaxChunkDataBytes
                    > LMC_DiagnosticsFrame.AbsoluteMaxRecorderChunkDataBytes
                || capabilities.MaxChunkDataBytes
                    > capabilities.MaxResponsePayloadBytes
                        - LMC_DiagnosticsParser.RecorderChunkResponseHeaderPayloadLength
                || capabilities.RecorderBytesPerBank == 0)
            {
                throw new InvalidDataException(
                    "Recorder capability limits are invalid for diagnostics schema version 1.");
            }

            var supportsDoubleBank = capabilities.Supports(
                LMCDiagnosticCapability.RecorderDoubleBank);
            if (supportsDoubleBank != (capabilities.RecorderBufferCount == 2))
            {
                throw new InvalidDataException(
                    "RecorderDoubleBank capability must match RecorderBufferCount=2.");
            }

            uint cachedMapRevision;
            if (TryGetCatalogRevision(
                    expectedSessionGeneration,
                    out cachedMapRevision)
                && cachedMapRevision != capabilities.MapRevision)
            {
                InvalidateCatalogRevision(expectedSessionGeneration);
            }
        }

        private void ValidateRecorderAdoptionCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            uint expectedDiagnosticsBootId,
            uint bufferId)
        {
            ValidateRecorderCapabilityEnvelope(
                capabilities,
                expectedSessionGeneration);
            if (!capabilities.Supports(
                LMCDiagnosticCapability.RecorderSingleBank))
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise Recorder diagnostics.");
            }

            if (capabilities.DiagnosticsBootId != expectedDiagnosticsBootId)
            {
                throw new InvalidOperationException(
                    "The requested Recorder belongs to a different DiagnosticsBootId.");
            }

            if (bufferId >= capabilities.RecorderBufferCount)
            {
                throw new ArgumentOutOfRangeException(
                    "bufferId",
                    "BufferId exceeds the connected PLC Recorder bank count.");
            }
        }

        private static void ValidateRecorderAdoptionArguments(
            uint diagnosticsBootId,
            uint recordId)
        {
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "AdoptRecorder requires a non-zero DiagnosticsBootId.");
            }

            if (recordId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "recordId",
                    "AdoptRecorder requires a non-zero RecordId.");
            }
        }

        private long ValidateRecorderConfiguration(
            LMCRecorderConfigurationHandle configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (!ReferenceEquals(configuration.Owner, this))
            {
                throw new InvalidOperationException(
                    "The Recorder configuration belongs to a different LMCConnection.");
            }

            configuration.EnsureUsable();
            var sessionGeneration = connection.SessionGeneration;
            if (configuration.ConnectionSessionGeneration != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The Recorder configuration belongs to a stale connection session.");
            }

            ValidateRememberedRecorderBootId(
                sessionGeneration,
                configuration.DiagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private long ValidateRecorderIdentity(
            LMCRecorderIdentity identity,
            bool requireOwner)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (!ReferenceEquals(identity.Owner, this))
            {
                throw new InvalidOperationException(
                    "The Recorder identity belongs to a different LMCConnection.");
            }

            identity.EnsureUsable();
            if (requireOwner && identity.OwnerSessionEpoch == 0)
            {
                throw new InvalidOperationException(
                    "This Recorder operation requires an adopted control owner.");
            }

            var sessionGeneration = connection.SessionGeneration;
            if (identity.ConnectionSessionGeneration != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The Recorder identity belongs to a stale connection session. Use AdoptRecorder after reconnect.");
            }

            ValidateRememberedRecorderBootId(
                sessionGeneration,
                identity.DiagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private long ValidateTriggeredRecorderIdentity(
            LMCRecorderIdentity identity)
        {
            var sessionGeneration = ValidateRecorderIdentity(identity, true);
            if (!identity.HasConfigurationShape
                || identity.TriggerType == LMCRecorderTriggerType.Manual)
            {
                throw new InvalidOperationException(
                    "TriggerRecorder requires a locally configured D4 edge, window, or mask Recorder identity.");
            }

            return sessionGeneration;
        }

        private long ValidateRecorderIdentityForConfigurationRelease(
            LMCRecorderIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            if (!ReferenceEquals(identity.Owner, this))
            {
                throw new InvalidOperationException(
                    "The Recorder identity belongs to a different LMCConnection.");
            }

            if (!identity.IsAdopted)
            {
                throw new InvalidOperationException(
                    "Release a locally configured Recorder through its LMCRecorderConfigurationHandle.");
            }

            if (!identity.HasConfigurationMetadata
                || identity.OwnerSessionEpoch == 0)
            {
                throw new InvalidOperationException(
                    "Read Recorder status or header after AdoptRecorder before releasing its configuration.");
            }

            if (!identity.IsBufferReleased)
            {
                throw new InvalidOperationException(
                    "Release the Recorder buffer before releasing its configuration through an identity.");
            }

            var sessionGeneration = connection.SessionGeneration;
            if (identity.ConnectionSessionGeneration != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The Recorder identity belongs to a stale connection session. Use AdoptRecorder after reconnect.");
            }

            ValidateRememberedRecorderBootId(
                sessionGeneration,
                identity.DiagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private static void ValidateRecorderChunkRequest(
            LMCRecorderChunkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.Identity.HasFrozenHeaderMetadata)
            {
                throw new InvalidOperationException(
                    "Read Recorder header before requesting Recorder chunks.");
            }

            if (request.Identity.ChannelCount != 0)
            {
                var requestBytes = checked(
                    (uint)request.RequestedSampleCount
                    * request.Identity.ChannelCount
                    * sizeof(uint));
                if (requestBytes > request.Identity.MaxChunkDataBytes)
                {
                    throw new ArgumentOutOfRangeException(
                        "request",
                        "Requested Recorder chunk exceeds MaxChunkDataBytes.");
                }
            }

            if (request.OffsetSample >= request.Identity.FrozenSampleCount)
            {
                throw new ArgumentOutOfRangeException(
                    "request",
                    "Recorder chunk offset exceeds the frozen sample count.");
            }
        }

        private static void RequireRecorderConfigurationMetadataBeforeBufferRelease(
            LMCRecorderIdentity identity)
        {
            if (!identity.HasConfigurationMetadata)
            {
                throw new InvalidOperationException(
                    "Read Recorder status or header after AdoptRecorder before releasing its buffer.");
            }
        }

        private LMCRecorderIdentity CreateAdoptedIdentity(
            LMCRecorderAdoption adoption,
            LMCDiagnosticCapabilities capabilities,
            long sessionGeneration)
        {
            return new LMCRecorderIdentity(
                adoption.Response,
                adoption.DiagnosticsBootId,
                adoption.RecordId,
                adoption.BufferId,
                0,
                0,
                capabilities.MapRevision,
                adoption.OwnerSessionEpoch,
                adoption.State,
                0,
                0,
                LMCCapturePhase.None,
                0,
                LMCRecorderBufferMode.Single,
                LMCRecorderTriggerType.Manual,
                false,
                capabilities.MaxChunkDataBytes,
                new uint[0],
                sessionGeneration,
                this,
                true);
        }

        private T ParseRecorderResponse<T>(
            long sessionGeneration,
            Func<T> parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            try
            {
                return parser();
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleRecorderDomainError(sessionGeneration, exception);
                throw;
            }
        }

        private void ParseRecorderResponse(
            long sessionGeneration,
            Action parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            try
            {
                parser();
            }
            catch (LMCDiagnosticsCommandException exception)
            {
                HandleRecorderDomainError(sessionGeneration, exception);
                throw;
            }
        }

        private void HandleRecorderDomainError(
            long sessionGeneration,
            LMCDiagnosticsCommandException exception)
        {
            InvalidateCatalogRevisionOnMismatch(
                sessionGeneration,
                exception);

            if (exception != null
                && exception.Response != null
                && exception.Response.Detail
                    == LMCDiagnosticsDetailCode.BootIdMismatch)
            {
                lock (recorderBootIdSync)
                {
                    if (recorderBootIdSessionGeneration == sessionGeneration)
                    {
                        hasRecorderBootId = false;
                        recorderBootId = 0;
                    }
                }
            }
        }

        private void RememberRecorderBootId(
            long sessionGeneration,
            uint diagnosticsBootId)
        {
            lock (recorderBootIdSync)
            {
                hasRecorderBootId = true;
                recorderBootIdSessionGeneration = sessionGeneration;
                recorderBootId = diagnosticsBootId;
            }
        }

        private void ValidateRememberedRecorderBootId(
            long sessionGeneration,
            uint diagnosticsBootId)
        {
            lock (recorderBootIdSync)
            {
                if (!hasRecorderBootId
                    || recorderBootIdSessionGeneration != sessionGeneration
                    || recorderBootId != diagnosticsBootId)
                {
                    throw new InvalidOperationException(
                        "The Recorder DiagnosticsBootId is stale.");
                }
            }
        }
    }
}

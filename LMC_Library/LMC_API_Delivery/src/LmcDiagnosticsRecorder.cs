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

            if (configuration.BufferMode == LMCRecorderBufferMode.Double)
            {
                throw new NotSupportedException(
                    "Double-bank Recorder configurations require ConfigureRecoverableDoubleRecorder and an exact recovery token.");
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

            RememberRecorderBootId(
                sessionGeneration,
                handle.DiagnosticsBootId);
            return PublishAcceptedRecorderResult(
                sessionGeneration,
                LMC_CommandId.ConfigureRecorder,
                LMCRecorderAcceptedOperation.ConfigureRecorder,
                handle,
                null);
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

        public LMCRecorderConfigurationHandle
            ConfigureRecoverableDoubleRecorder(
                LMCRecorderConfiguration configuration,
                Guid recoveryToken)
        {
            return ConfigureRecoverableDoubleRecorderCore(
                configuration,
                recoveryToken,
                connection.SessionGeneration,
                null);
        }

        private LMCRecorderConfigurationHandle
            ConfigureRecoverableDoubleRecorderCore(
                LMCRecorderConfiguration configuration,
                Guid recoveryToken,
                long sessionGeneration,
                LMCDiagnosticCapabilities pinnedCapabilities)
        {
            ValidateRecoverableDoubleRecorderArguments(
                configuration,
                recoveryToken);
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = pinnedCapabilities ?? GetCapabilities();
            if (pinnedCapabilities == null)
            {
                ValidateRecorderCapabilities(
                    capabilities,
                    sessionGeneration,
                    configuration);
            }
            else
            {
                ValidatePinnedRecoverableDoubleRecorderCapabilities(
                    capabilities,
                    sessionGeneration,
                    configuration);
            }

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ConfigureRecoverableDoubleRecorder(
                    requestId,
                    capabilities.MapRevision,
                    configuration,
                    capabilities.DiagnosticsBootId,
                    recoveryToken),
                sessionGeneration);
            var handle = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser
                    .ParseConfigureRecoverableDoubleRecorder(
                        raw,
                        requestId,
                        configuration,
                        recoveryToken,
                        capabilities,
                        sessionGeneration,
                        this));
            RememberRecorderBootId(
                sessionGeneration,
                handle.DiagnosticsBootId);
            return PublishAcceptedRecorderResult(
                sessionGeneration,
                LMC_CommandId.ConfigureRecoverableDoubleRecorder,
                LMCRecorderAcceptedOperation
                    .ConfigureRecoverableDoubleRecorder,
                handle,
                null);
        }

        public async Task<LMCRecorderConfigurationHandle>
            ConfigureRecoverableDoubleRecorderAsync(
                LMCRecorderConfiguration configuration,
                Guid recoveryToken,
                CancellationToken cancellationToken)
        {
            ValidateRecoverableDoubleRecorderArguments(
                configuration,
                recoveryToken);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ConfigureRecoverableDoubleRecorderCore(
                        configuration,
                        recoveryToken,
                        sessionGeneration,
                        null);
                },
                CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates a recoverable Double Recorder configuration against one
        /// exact capability snapshot without sending a Configure request.
        /// The snapshot must belong to this diagnostics facade and current
        /// connection session.
        /// </summary>
        public void ValidateRecoverableDoubleRecorderConfiguration(
            LMCRecorderConfiguration configuration,
            Guid recoveryToken,
            LMCDiagnosticCapabilities capabilities)
        {
            ValidateRecoverableDoubleRecorderArguments(
                configuration,
                recoveryToken);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            ValidatePinnedRecoverableDoubleRecorderCapabilities(
                capabilities,
                sessionGeneration,
                configuration);
            connection.EnsureSessionGeneration(sessionGeneration);
        }

        /// <summary>
        /// Configures a recoverable Double Recorder using one exact capability
        /// snapshot. Capabilities are not read again from the wire. The PLC
        /// receives the pinned BootId and MapRevision and must reject a stale
        /// identity before applying the configuration.
        /// </summary>
        public async Task<LMCRecorderConfigurationHandle>
            ConfigureRecoverableDoubleRecorderAsync(
                LMCRecorderConfiguration configuration,
                Guid recoveryToken,
                LMCDiagnosticCapabilities capabilities,
                CancellationToken cancellationToken)
        {
            ValidateRecoverableDoubleRecorderArguments(
                configuration,
                recoveryToken);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            ValidatePinnedRecoverableDoubleRecorderCapabilities(
                capabilities,
                sessionGeneration,
                configuration);
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ConfigureRecoverableDoubleRecorderCore(
                        configuration,
                        recoveryToken,
                        sessionGeneration,
                        capabilities);
                },
                CancellationToken.None).ConfigureAwait(false);
        }

        public LMCRecorderIdentity StartRecorder(
            LMCRecorderConfigurationHandle configuration)
        {
            var sessionGeneration = ValidateRecorderConfiguration(
                configuration,
                false);
            configuration.BeginStart();
            try
            {
                var requestId = NextRequestId();
                var raw = connection.Exchange(
                    LMC_DiagnosticsFrame.StartRecorder(
                        requestId,
                        configuration),
                    sessionGeneration);
                var identity = ParseRecorderResponse(
                    sessionGeneration,
                    () => LMC_DiagnosticsParser.ParseStartRecorder(
                        raw,
                        requestId,
                        configuration,
                        sessionGeneration,
                        this));
                var publishedIdentity = PublishAcceptedRecorderResult(
                    sessionGeneration,
                    LMC_CommandId.StartRecorder,
                    LMCRecorderAcceptedOperation.StartRecorder,
                    identity,
                    configuration);
                configuration.CompleteStart();
                return publishedIdentity;
            }
            catch
            {
                configuration.CancelStart();
                throw;
            }
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
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.TriggerRecorder,
                () => { });
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
            var sessionGeneration = ValidateRecorderIdentity(
                identity,
                true,
                true);
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
            connection.PublishSessionBoundSendPriorityResult(
                sessionGeneration,
                LMC_CommandId.StopRecorder,
                () => { });
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
            var sessionGeneration = ValidateRecorderIdentity(
                identity,
                false,
                true);
            return ReadRecorderStatusCore(identity, sessionGeneration);
        }

        public async Task<LMCRecorderStatus> GetRecorderStatusAsync(
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            var sessionGeneration = ValidateRecorderIdentity(
                identity,
                false,
                true);
            return await ReadRecorderStatusCoreAsync(
                identity,
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
        }

        public LMCRecorderHeader GetRecorderHeader(
            LMCRecorderIdentity identity)
        {
            var sessionGeneration = ValidateRecorderIdentity(
                identity,
                false,
                false);
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
            var sessionGeneration = ValidateRecorderIdentity(
                identity,
                false,
                false);
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
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = ValidateRecorderIdentity(
                request.Identity,
                false,
                false);
            ValidateRecorderChunkRequest(request);
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
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var sessionGeneration = ValidateRecorderIdentity(
                request.Identity,
                false,
                false);
            ValidateRecorderChunkRequest(request);
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
            var sessionGeneration = ValidateRecorderIdentity(
                identity,
                true,
                true);
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
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.ReleaseRecorderBuffer,
                    identity.CompleteBufferRelease);
            }
            catch (LMCDiagnosticsCommandException)
            {
                identity.CancelBufferRelease();
                throw;
            }
            catch (LMCDiagnosticsDispatchRejectedException)
            {
                identity.CancelBufferRelease();
                throw;
            }
            catch (LMCSendPreemptedException exception)
                when (exception.Phase == LMCSendPreemptionPhase.BeforeWire)
            {
                identity.CancelBufferRelease();
                throw;
            }
            catch
            {
                identity.MarkBufferReleaseOutcomeUnverified();
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
            var sessionGeneration = ValidateRecorderConfiguration(
                configuration,
                true);
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
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.ReleaseRecorder,
                    configuration.CompleteRelease);
            }
            catch (LMCDiagnosticsCommandException)
            {
                configuration.CancelRelease();
                throw;
            }
            catch (LMCDiagnosticsDispatchRejectedException)
            {
                configuration.CancelRelease();
                throw;
            }
            catch (LMCSendPreemptedException exception)
                when (exception.Phase == LMCSendPreemptionPhase.BeforeWire)
            {
                configuration.CancelRelease();
                throw;
            }
            catch
            {
                configuration.MarkReleaseOutcomeUnverified();
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

        public void ReleaseRecorder(
            LMCRecoveredRecorderConfigurationLease configuration)
        {
            var sessionGeneration =
                ValidateRecoveredRecorderConfiguration(configuration);
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
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.ReleaseRecorder,
                    configuration.CompleteRelease);
            }
            catch (LMCDiagnosticsCommandException)
            {
                configuration.CancelRelease();
                throw;
            }
            catch (LMCDiagnosticsDispatchRejectedException)
            {
                configuration.CancelRelease();
                throw;
            }
            catch (LMCSendPreemptedException exception)
                when (exception.Phase == LMCSendPreemptionPhase.BeforeWire)
            {
                configuration.CancelRelease();
                throw;
            }
            catch
            {
                configuration.MarkReleaseOutcomeUnverified();
                throw;
            }
        }

        public async Task ReleaseRecorderAsync(
            LMCRecoveredRecorderConfigurationLease configuration,
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
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    LMC_CommandId.ReleaseRecorder,
                    identity.CompleteRecorderRelease);
            }
            catch (LMCDiagnosticsCommandException)
            {
                identity.CancelRecorderRelease();
                throw;
            }
            catch (LMCDiagnosticsDispatchRejectedException)
            {
                identity.CancelRecorderRelease();
                throw;
            }
            catch (LMCSendPreemptedException exception)
                when (exception.Phase == LMCSendPreemptionPhase.BeforeWire)
            {
                identity.CancelRecorderRelease();
                throw;
            }
            catch
            {
                identity.MarkRecorderReleaseOutcomeUnverified();
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

        public LMCRecorderBankInventory ReadRecorderBankInventory(
            uint diagnosticsBootId,
            uint configId,
            uint mapRevision,
            uint configRevision = 0)
        {
            ValidateRecorderBankInventoryArguments(
                diagnosticsBootId,
                configId,
                mapRevision);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateRecorderBankInventoryCapabilities(
                capabilities,
                sessionGeneration,
                diagnosticsBootId,
                mapRevision);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadRecorderBankInventory(
                    requestId,
                    diagnosticsBootId,
                    configId,
                    mapRevision,
                    configRevision),
                sessionGeneration);
            var inventory = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderBankInventory(
                    raw,
                    requestId,
                    diagnosticsBootId,
                    configId,
                    mapRevision,
                    configRevision));
            RememberRecorderBootId(sessionGeneration, diagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return inventory;
        }

        public Task<LMCRecorderBankInventory> ReadRecorderBankInventoryAsync(
            uint diagnosticsBootId,
            uint configId,
            uint mapRevision,
            CancellationToken cancellationToken)
        {
            return ReadRecorderBankInventoryAsync(
                diagnosticsBootId,
                configId,
                mapRevision,
                0,
                cancellationToken);
        }

        public async Task<LMCRecorderBankInventory>
            ReadRecorderBankInventoryAsync(
                uint diagnosticsBootId,
                uint configId,
                uint mapRevision,
                uint configRevision,
                CancellationToken cancellationToken)
        {
            ValidateRecorderBankInventoryArguments(
                diagnosticsBootId,
                configId,
                mapRevision);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateRecorderBankInventoryCapabilities(
                capabilities,
                sessionGeneration,
                diagnosticsBootId,
                mapRevision);

            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadRecorderBankInventory(
                    requestId,
                    diagnosticsBootId,
                    configId,
                    mapRevision,
                    configRevision),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var inventory = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser.ParseRecorderBankInventory(
                    raw,
                    requestId,
                    diagnosticsBootId,
                    configId,
                    mapRevision,
                    configRevision));
            RememberRecorderBootId(sessionGeneration, diagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return inventory;
        }

        public LMCRecorderBankInventory
            ReadRecoverableRecorderBankInventory(
                uint diagnosticsBootId,
                uint configId,
                uint mapRevision,
                Guid recoveryToken)
        {
            ValidateRecorderBankInventoryArguments(
                diagnosticsBootId,
                configId,
                mapRevision);
            ValidateRecorderRecoveryToken(recoveryToken);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateRecoverableRecorderBankInventoryCapabilities(
                capabilities,
                sessionGeneration,
                diagnosticsBootId,
                mapRevision);

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.ReadRecoverableRecorderBankInventory(
                    requestId,
                    diagnosticsBootId,
                    configId,
                    mapRevision,
                    recoveryToken),
                sessionGeneration);
            var inventory = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        raw,
                        requestId,
                        diagnosticsBootId,
                        configId,
                        mapRevision,
                        recoveryToken));
            RememberRecorderBootId(sessionGeneration, diagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return inventory;
        }

        public async Task<LMCRecorderBankInventory>
            ReadRecoverableRecorderBankInventoryAsync(
                uint diagnosticsBootId,
                uint configId,
                uint mapRevision,
                Guid recoveryToken,
                CancellationToken cancellationToken)
        {
            ValidateRecorderBankInventoryArguments(
                diagnosticsBootId,
                configId,
                mapRevision);
            ValidateRecorderRecoveryToken(recoveryToken);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = await GetCapabilitiesAsync(
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            ValidateRecoverableRecorderBankInventoryCapabilities(
                capabilities,
                sessionGeneration,
                diagnosticsBootId,
                mapRevision);

            var requestId = NextRequestId();
            var raw = await connection.ExchangeAsync(
                LMC_DiagnosticsFrame.ReadRecoverableRecorderBankInventory(
                    requestId,
                    diagnosticsBootId,
                    configId,
                    mapRevision,
                    recoveryToken),
                sessionGeneration,
                cancellationToken).ConfigureAwait(false);
            var inventory = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser
                    .ParseRecoverableRecorderBankInventory(
                        raw,
                        requestId,
                        diagnosticsBootId,
                        configId,
                        mapRevision,
                        recoveryToken));
            RememberRecorderBootId(sessionGeneration, diagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return inventory;
        }

        public LMCRecoveredRecorderConfigurationLease
            AdoptEmptyRecorderConfiguration(
                LMCRecorderBankInventory inventory)
        {
            return AdoptEmptyRecorderConfigurationCore(
                inventory,
                connection.SessionGeneration);
        }

        private LMCRecoveredRecorderConfigurationLease
            AdoptEmptyRecorderConfigurationCore(
                LMCRecorderBankInventory inventory,
                long sessionGeneration)
        {
            ValidateEmptyRecorderConfigurationInventory(inventory);
            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateRecorderBankInventoryCapabilities(
                capabilities,
                sessionGeneration,
                inventory.DiagnosticsBootId,
                inventory.MapRevision);
            if (capabilities.MaxRequestPayloadBytes
                    < LMC_DiagnosticsFrame
                        .AdoptEmptyRecorderConfigurationRequestPayloadLength
                || capabilities.MaxResponsePayloadBytes
                    < LMC_DiagnosticsParser
                        .AdoptEmptyRecorderConfigurationResponsePayloadLength)
            {
                throw new InvalidDataException(
                    "Recorder capability payload limits cannot carry empty-configuration recovery metadata.");
            }

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                LMC_DiagnosticsFrame.AdoptEmptyRecorderConfiguration(
                    requestId,
                    inventory),
                sessionGeneration);
            var lease = ParseRecorderResponse(
                sessionGeneration,
                () => LMC_DiagnosticsParser
                    .ParseAdoptEmptyRecorderConfiguration(
                        raw,
                        requestId,
                        inventory,
                        sessionGeneration,
                        this));
            RememberRecorderBootId(
                sessionGeneration,
                inventory.DiagnosticsBootId);
            return PublishAcceptedRecorderResult(
                sessionGeneration,
                LMC_CommandId.AdoptEmptyRecorderConfiguration,
                LMCRecorderAcceptedOperation
                    .AdoptEmptyRecorderConfiguration,
                lease,
                null);
        }

        public async Task<LMCRecoveredRecorderConfigurationLease>
            AdoptEmptyRecorderConfigurationAsync(
                LMCRecorderBankInventory inventory,
                CancellationToken cancellationToken)
        {
            ValidateEmptyRecorderConfigurationInventory(inventory);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return AdoptEmptyRecorderConfigurationCore(
                        inventory,
                        sessionGeneration);
                },
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
                false,
                connection.SessionGeneration);
        }

        public LMCRecorderIdentity AdoptActiveRecorder(
            uint diagnosticsBootId)
        {
            return AdoptRecorderCore(
                diagnosticsBootId,
                0,
                0,
                true,
                connection.SessionGeneration);
        }

        private LMCRecorderIdentity AdoptRecorderCore(
            uint diagnosticsBootId,
            uint recordId,
            uint bufferId,
            bool discoverActive,
            long sessionGeneration)
        {
            if (discoverActive)
            {
                ValidateRecorderActiveAdoptionArguments(diagnosticsBootId);
            }
            else
            {
                ValidateRecorderAdoptionArguments(diagnosticsBootId, recordId);
            }

            connection.EnsureSessionGeneration(sessionGeneration);
            var capabilities = GetCapabilities();
            ValidateRecorderAdoptionCapabilities(
                capabilities,
                sessionGeneration,
                diagnosticsBootId,
                bufferId);
            if (discoverActive)
            {
                ValidateActiveRecorderDiscoveryCapabilities(capabilities);
            }

            var requestId = NextRequestId();
            var raw = connection.Exchange(
                discoverActive
                    ? LMC_DiagnosticsFrame.AdoptActiveRecorder(
                        requestId,
                        diagnosticsBootId)
                    : LMC_DiagnosticsFrame.AdoptRecorder(
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
            var identity = CreateAdoptedIdentity(
                adoption,
                capabilities,
                sessionGeneration);
            RememberRecorderBootId(sessionGeneration, diagnosticsBootId);
            return PublishAcceptedRecorderResult(
                sessionGeneration,
                LMC_CommandId.AdoptRecorder,
                discoverActive
                    ? LMCRecorderAcceptedOperation.AdoptActiveRecorder
                    : LMCRecorderAcceptedOperation.AdoptRecorder,
                identity,
                null);
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
                        false,
                        sessionGeneration);
                },
                CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<LMCRecorderIdentity> AdoptActiveRecorderAsync(
            uint diagnosticsBootId,
            CancellationToken cancellationToken)
        {
            ValidateRecorderActiveAdoptionArguments(diagnosticsBootId);
            var sessionGeneration = connection.SessionGeneration;
            connection.EnsureSessionGeneration(sessionGeneration);
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return AdoptRecorderCore(
                        diagnosticsBootId,
                        0,
                        0,
                        true,
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

        private static void ValidateRecoverableDoubleRecorderArguments(
            LMCRecorderConfiguration configuration,
            Guid recoveryToken)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (configuration.BufferMode != LMCRecorderBufferMode.Double)
            {
                throw new ArgumentException(
                    "Recoverable Recorder configuration requires Double buffer mode.",
                    "configuration");
            }

            if (configuration.RequestedConfigId == 0)
            {
                throw new ArgumentException(
                    "Recoverable Double-bank Recorder configuration requires a caller-selected non-zero RequestedConfigId.",
                    "configuration");
            }

            if (recoveryToken == Guid.Empty)
            {
                throw new ArgumentException(
                    "Recorder recovery tokens must be nonempty.",
                    "recoveryToken");
            }
        }

        private void
            ValidatePinnedRecoverableDoubleRecorderCapabilities(
                LMCDiagnosticCapabilities capabilities,
                long expectedSessionGeneration,
                LMCRecorderConfiguration configuration)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (!capabilities.IsBoundTo(
                    this,
                    expectedSessionGeneration))
            {
                throw new InvalidOperationException(
                    "Diagnostics capabilities are not bound to this diagnostics owner and current connection session.");
            }

            ValidateRecorderCapabilities(
                capabilities,
                expectedSessionGeneration,
                configuration);
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

            var configureHeaderPayloadLength =
                configuration.BufferMode == LMCRecorderBufferMode.Double
                    ? LMC_DiagnosticsFrame
                        .ConfigureRecoverableDoubleRecorderRequestHeaderPayloadLength
                    : LMC_DiagnosticsFrame
                        .ConfigureRecorderRequestHeaderPayloadLength;
            var configurePayloadBytes = checked(
                configureHeaderPayloadLength
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

        private static void ValidateActiveRecorderDiscoveryCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            if (capabilities.RecorderBufferCount != 1
                || capabilities.Supports(
                    LMCDiagnosticCapability.RecorderDoubleBank))
            {
                throw new NotSupportedException(
                    "AdoptActiveRecorder is only defined for a single-bank Recorder.");
            }
        }

        private void ValidateRecorderBankInventoryCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            uint expectedDiagnosticsBootId,
            uint expectedMapRevision)
        {
            ValidateRecorderCapabilityEnvelope(
                capabilities,
                expectedSessionGeneration);
            if (!capabilities.Supports(
                    LMCDiagnosticCapability.RecorderSingleBank)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.RecorderDoubleBank)
                || capabilities.RecorderBufferCount != 2)
            {
                throw new NotSupportedException(
                    "The connected PLC does not advertise two-bank Recorder recovery diagnostics.");
            }

            if (capabilities.DiagnosticsBootId != expectedDiagnosticsBootId)
            {
                throw new InvalidOperationException(
                    "The requested Recorder configuration belongs to a different DiagnosticsBootId.");
            }

            if (capabilities.MapRevision != expectedMapRevision)
            {
                throw new InvalidOperationException(
                    "The requested Recorder configuration belongs to a different Catalog revision.");
            }

            if (capabilities.MaxRequestPayloadBytes
                    < LMC_DiagnosticsFrame
                        .RecorderBankInventoryRequestPayloadLength
                || capabilities.MaxResponsePayloadBytes
                    < LMC_DiagnosticsParser
                        .RecorderBankInventoryResponsePayloadLength)
            {
                throw new InvalidDataException(
                    "Recorder capability payload limits cannot carry bank inventory recovery metadata.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private void ValidateRecoverableRecorderBankInventoryCapabilities(
            LMCDiagnosticCapabilities capabilities,
            long expectedSessionGeneration,
            uint expectedDiagnosticsBootId,
            uint expectedMapRevision)
        {
            ValidateRecorderBankInventoryCapabilities(
                capabilities,
                expectedSessionGeneration,
                expectedDiagnosticsBootId,
                expectedMapRevision);
            if (capabilities.MaxRequestPayloadBytes
                    < LMC_DiagnosticsFrame
                        .RecoverableRecorderBankInventoryRequestPayloadLength
                || capabilities.MaxResponsePayloadBytes
                    < LMC_DiagnosticsParser
                        .RecoverableRecorderBankInventoryResponsePayloadLength)
            {
                throw new InvalidDataException(
                    "Recorder capability payload limits cannot carry token-qualified bank inventory recovery metadata.");
            }

            connection.EnsureSessionGeneration(expectedSessionGeneration);
        }

        private static void ValidateRecorderBankInventoryArguments(
            uint diagnosticsBootId,
            uint configId,
            uint mapRevision)
        {
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "Recorder bank inventory requires a non-zero DiagnosticsBootId.");
            }

            if (configId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "configId",
                    "Recorder bank inventory requires a non-zero ConfigId.");
            }

            if (mapRevision == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "mapRevision",
                    "Recorder bank inventory requires a non-zero exact Catalog revision.");
            }
        }

        private static void ValidateRecorderRecoveryToken(Guid recoveryToken)
        {
            if (recoveryToken == Guid.Empty)
            {
                throw new ArgumentException(
                    "Recorder recovery tokens must be nonempty.",
                    "recoveryToken");
            }
        }

        private static void ValidateEmptyRecorderConfigurationInventory(
            LMCRecorderBankInventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (inventory.DiagnosticsBootId == 0
                || inventory.ConfigId == 0
                || inventory.ConfigRevision == 0
                || inventory.MapRevision == 0
                || inventory.IsRecoverable
                || inventory.RecoveryToken != Guid.Empty
                || inventory.ConfigurationOwnerSessionEpoch == 0
                || !inventory.IsConfigurationOwnerSessionClosed
                || inventory.ConfigurationState != LMCRecorderState.Configured
                || inventory.BufferMode != LMCRecorderBufferMode.Double
                || inventory.RecorderBufferCount != 2
                || inventory.OccupiedBanks.Count != 0)
            {
                throw new ArgumentException(
                    "AdoptEmptyRecorderConfiguration requires an exact standard 0x7E4A closed, empty, two-bank Recorder inventory without a recovery token.",
                    "inventory");
            }
        }

        private static void ValidateRecorderActiveAdoptionArguments(
            uint diagnosticsBootId)
        {
            if (diagnosticsBootId == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "diagnosticsBootId",
                    "AdoptActiveRecorder requires a non-zero DiagnosticsBootId.");
            }
        }

        private long ValidateRecorderConfiguration(
            LMCRecorderConfigurationHandle configuration,
            bool allowRecoveryOnly)
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

            if (allowRecoveryOnly)
            {
                configuration.EnsureUsableForRecovery();
            }
            else
            {
                configuration.EnsureUsable();
            }
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

        private long ValidateRecoveredRecorderConfiguration(
            LMCRecoveredRecorderConfigurationLease configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (!ReferenceEquals(configuration.Owner, this))
            {
                throw new InvalidOperationException(
                    "The recovered Recorder configuration belongs to a different LMCConnection.");
            }

            configuration.EnsureUsableForRecovery();
            var sessionGeneration = connection.SessionGeneration;
            if (configuration.ConnectionSessionGeneration != sessionGeneration)
            {
                throw new InvalidOperationException(
                    "The recovered Recorder configuration belongs to a stale connection session.");
            }

            ValidateRememberedRecorderBootId(
                sessionGeneration,
                configuration.DiagnosticsBootId);
            connection.EnsureSessionGeneration(sessionGeneration);
            return sessionGeneration;
        }

        private long ValidateRecorderIdentity(
            LMCRecorderIdentity identity,
            bool requireOwner,
            bool allowRecoveryOnly)
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

            if (allowRecoveryOnly)
            {
                identity.EnsureUsableForRecovery();
            }
            else
            {
                identity.EnsureUsable();
            }
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
            var sessionGeneration = ValidateRecorderIdentity(
                identity,
                true,
                false);
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
                    "Read Recorder status after AdoptRecorder before releasing its configuration.");
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
                    "Read Recorder status after AdoptRecorder before releasing its buffer.");
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
                0,
                0,
                false,
                capabilities.MaxChunkDataBytes,
                new uint[0],
                sessionGeneration,
                this,
                true);
        }

        private T PublishAcceptedRecorderResult<T>(
            long sessionGeneration,
            ushort command,
            LMCRecorderAcceptedOperation operation,
            T result,
            LMCRecorderConfigurationHandle sourceConfigurationHandle)
            where T : class
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            try
            {
                T publishedResult = null;
                connection.PublishSessionBoundSendPriorityResult(
                    sessionGeneration,
                    command,
                    () => publishedResult = result);
                return publishedResult;
            }
            catch (Exception exception)
            {
                var configurationHandle =
                    result as LMCRecorderConfigurationHandle;
                var identity = result as LMCRecorderIdentity;
                var recoveredConfigurationLease =
                    result as LMCRecoveredRecorderConfigurationLease;

                if (configurationHandle != null)
                {
                    configurationHandle.MarkAcceptedResultRecoveryOnly();
                }

                if (identity != null)
                {
                    identity.MarkAcceptedResultRecoveryOnly();
                }

                if (recoveredConfigurationLease != null)
                {
                    recoveredConfigurationLease
                        .MarkAcceptedResultRecoveryOnly();
                }

                if (sourceConfigurationHandle != null)
                {
                    sourceConfigurationHandle
                        .MarkAcceptedResultRecoveryOnly();
                }

                LMCRecorderAcceptedResultFailureContext.Attach(
                    exception,
                    new LMCRecorderAcceptedResultFailureContext(
                        operation,
                        command,
                        configurationHandle,
                        identity,
                        recoveredConfigurationLease,
                        sourceConfigurationHandle));
                throw;
            }
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

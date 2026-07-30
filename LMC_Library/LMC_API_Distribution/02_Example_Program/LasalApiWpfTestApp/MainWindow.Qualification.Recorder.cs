using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const int RecorderQualificationChannelCount = 4;
        private const int RecorderQualificationPollMilliseconds = 25;
        private const int RecorderQualificationRpcTimeoutMilliseconds = 15000;
        private const int RecorderReconnectAdoptTimeoutMilliseconds = 5000;

        private async void ButtonRunRecorderSingleQualification_Click(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "RecorderSingleManual",
                RunRecorderSingleQualificationAsync);
        }

        private async void ButtonRunRecorderRingQualification_Click(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "RecorderRingForcedTrigger",
                RunRecorderRingQualificationAsync);
        }

        private async void ButtonRunRecorderDoubleQualification_Click(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "RecorderDoubleBank",
                RunRecorderDoubleQualificationAsync);
        }

        private async void ButtonRunRecorderSoakQualification_Click(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "RecorderTriggerLifecycleSoak",
                RunRecorderSoakQualificationAsync);
        }

        private async void ButtonRunRecorderReconnectExactQualification_Click(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "RecorderReconnectExactAdopt",
                cancellationToken => RunRecorderReconnectQualificationAsync(
                    false,
                    cancellationToken));
        }

        private async void
            ButtonRunRecorderReconnectDiscoveryQualification_Click(
                object sender,
                System.Windows.RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "RecorderReconnectDiscoveryAdopt",
                cancellationToken => RunRecorderReconnectQualificationAsync(
                    true,
                    cancellationToken));
        }

        private async Task RunRecorderSingleQualificationAsync(
            CancellationToken cancellationToken)
        {
            const uint sampleCapacity = 1000;
            var context = await PrepareRecorderQualificationAsync(
                sampleCapacity,
                false,
                cancellationToken);
            var diagnostics = RequireConnection().Diagnostics;
            LMCRecorderConfigurationHandle handle = null;
            LMCRecorderIdentity identity = null;
            Exception primaryError = null;

            try
            {
                SetQualificationProgress(10, "Configuring 4-channel Single Manual Recorder");
                var configuration = new LMCRecorderConfiguration(
                    context.SignalIds,
                    1,
                    sampleCapacity);

                handle = await SendQualificationCommandAsync(
                    "Recorder Single Configure",
                    cancellationToken,
                    () => diagnostics.ConfigureRecorderAsync(
                        configuration,
                        CancellationToken.None),
                    value => handle = value);
                AssertRecorderConfigurationHandle(
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderBufferMode.Single,
                    LMCRecorderTriggerType.Manual);
                WriteQualificationLog(
                    "event=CONFIGURED",
                    "configId=" + handle.ConfigId,
                    "configRevision=" + handle.ConfigRevision,
                    "channels=" + handle.ChannelCount,
                    "capacity=" + handle.AcceptedCapacity,
                    "stride=" + handle.SampleStrideBytes,
                    "samplePeriodUs=" + handle.SamplePeriodUs);

                SetQualificationProgress(20, "Starting Single Manual Recorder");
                identity = await SendQualificationCommandAsync(
                    "Recorder Single Start",
                    cancellationToken,
                    () => diagnostics.StartRecorderAsync(
                        handle,
                        CancellationToken.None),
                    value => identity = value);
                AssertRecorderIdentity(identity, handle, context);
                WriteQualificationLog(
                    "event=STARTED",
                    "recordId=" + identity.RecordId,
                    "bufferId=" + identity.BufferId,
                    "startCycle=" + identity.AcceptedStartCycle,
                    "state=" + identity.InitialState);

                SetQualificationProgress(30, "Waiting for natural sample-count completion");
                var status = await WaitForRecorderStateAsync(
                    diagnostics,
                    identity,
                    handle,
                    value => value.State == LMCRecorderState.Ready,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    cancellationToken,
                    "SINGLE_READY");
                AssertRecorderTerminalStatus(
                    status,
                    handle,
                    sampleCapacity,
                    LMCRecorderStopReason.SampleCountComplete,
                    false,
                    uint.MaxValue);
                WriteQualificationLog(
                    "event=ASSERT",
                    "name=SingleTerminalStatus",
                    "state=" + status.State,
                    "stopReason=" + status.StopReason,
                    "sampleCount=" + status.SampleCount,
                    "dropped=" + status.DroppedSamples,
                    "overflow=" + status.OverflowCount,
                    "verdict=PASS");

                SetQualificationProgress(48, "Reading and validating frozen Recorder header");
                var header = await SendQualificationCommandAsync(
                    "Recorder Single header",
                    cancellationToken,
                    () => diagnostics.GetRecorderHeaderAsync(
                        identity,
                        CancellationToken.None));
                AssertRecorderHeader(
                    header,
                    identity,
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderStopReason.SampleCountComplete,
                    false,
                    uint.MaxValue);

                SetQualificationProgress(58, "Downloading immutable Recorder data A");
                var dataA = await DownloadRecorderQualificationAsync(
                    diagnostics,
                    identity,
                    context.Capabilities.MaxChunkDataBytes,
                    cancellationToken,
                    "SINGLE_A");
                AssertRecorderData(
                    dataA,
                    identity,
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderStopReason.SampleCountComplete,
                    false,
                    uint.MaxValue);
                var hashA = ComputeRecorderQualificationSha256(dataA.Data);

                SetQualificationProgress(72, "Downloading immutable Recorder data B");
                var dataB = await DownloadRecorderQualificationAsync(
                    diagnostics,
                    identity,
                    context.Capabilities.MaxChunkDataBytes,
                    cancellationToken,
                    "SINGLE_B");
                AssertRecorderData(
                    dataB,
                    identity,
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderStopReason.SampleCountComplete,
                    false,
                    uint.MaxValue);
                var hashB = ComputeRecorderQualificationSha256(dataB.Data);
                if (!dataA.Data.SequenceEqual(dataB.Data) || hashA != hashB)
                {
                    throw new InvalidOperationException(
                        "The two immutable Recorder downloads do not contain identical raw bytes.");
                }

                WriteQualificationLog(
                    "event=ASSERT",
                    "name=DoubleDownloadIdentity",
                    "bytes=" + dataA.Data.Length,
                    "sha256=" + hashA,
                    "secondSha256=" + hashB,
                    "channelOrder=" + FormatRecorderQualificationSignalIds(
                        context.SignalIds),
                    "chunkCoverage=QUALIFICATION_GATED_EXACT",
                    "verdict=PASS");
                SetQualificationProgress(88, "Single Manual assertions PASS; releasing resources");
            }
            catch (Exception error)
            {
                primaryError = error;
                throw;
            }
            finally
            {
                await CleanupRecorderQualificationPreservingPrimaryAsync(
                    diagnostics,
                    handle,
                    identity,
                    "SINGLE",
                    primaryError);
            }

            await VerifyRecorderQualificationDoubleReleaseBlockedAsync(
                diagnostics,
                handle,
                identity,
                cancellationToken);
        }

        private async Task RunRecorderRingQualificationAsync(
            CancellationToken cancellationToken)
        {
            const uint sampleCapacity = 1000;
            const uint preTriggerSamples = 100;
            const uint postTriggerSamples = 899;
            var context = await PrepareRecorderQualificationAsync(
                sampleCapacity,
                true,
                cancellationToken);
            var diagnostics = RequireConnection().Diagnostics;
            LMCRecorderConfigurationHandle handle = null;
            LMCRecorderIdentity identity = null;
            Exception primaryError = null;

            try
            {
                SetQualificationProgress(10, "Configuring 4-channel Ring Edge Recorder");
                var configuration = BuildForcedTriggerRecorderConfiguration(
                    context,
                    sampleCapacity,
                    preTriggerSamples,
                    postTriggerSamples);
                handle = await SendQualificationCommandAsync(
                    "Recorder Ring Configure",
                    cancellationToken,
                    () => diagnostics.ConfigureRecorderAsync(
                        configuration,
                        CancellationToken.None),
                    value => handle = value);
                AssertRecorderConfigurationHandle(
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Edge);

                identity = await SendQualificationCommandAsync(
                    "Recorder Ring Start",
                    cancellationToken,
                    () => diagnostics.StartRecorderAsync(
                        handle,
                        CancellationToken.None),
                    value => identity = value);
                AssertRecorderIdentity(identity, handle, context);
                WriteQualificationLog(
                    "event=STARTED",
                    "recordId=" + identity.RecordId,
                    "bufferId=" + identity.BufferId,
                    "triggerSignal=0x" + context.TriggerSignal.SignalId.ToString("X8"),
                    "triggerType=Edge",
                    "triggerSource=FORCED_API");

                SetQualificationProgress(25, "Waiting for at least 100 pre-trigger samples");
                var preTriggerStatus = await WaitForRecorderStateAsync(
                    diagnostics,
                    identity,
                    handle,
                    value => value.State == LMCRecorderState.Recording
                        && value.SampleCount >= preTriggerSamples,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    cancellationToken,
                    "RING_PREHISTORY");
                if (preTriggerStatus.IsFrozen)
                {
                    throw new InvalidOperationException(
                        "Ring Recorder froze before the forced TriggerRecorder request.");
                }

                WriteQualificationLog(
                    "event=ASSERT",
                    "name=PreHistory",
                    "sampleCount=" + preTriggerStatus.SampleCount,
                    "required=" + preTriggerSamples,
                    "verdict=PASS");

                SetQualificationProgress(42, "Publishing forced Recorder trigger");
                await SendQualificationCommandAsync(
                    "Recorder Ring forced Trigger",
                    cancellationToken,
                    () => diagnostics.TriggerRecorderAsync(
                        identity,
                        CancellationToken.None));
                WriteQualificationLog(
                    "event=TRIGGER_SENT",
                    "recordId=" + identity.RecordId,
                    "bufferId=" + identity.BufferId,
                    "source=TriggerRecorderAsync");

                SetQualificationProgress(52, "Waiting for TriggerComplete terminal status");
                var terminal = await WaitForRecorderStateAsync(
                    diagnostics,
                    identity,
                    handle,
                    value => value.State == LMCRecorderState.Ready,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    cancellationToken,
                    "RING_READY");
                AssertRecorderTerminalStatus(
                    terminal,
                    handle,
                    sampleCapacity,
                    LMCRecorderStopReason.TriggerComplete,
                    true,
                    preTriggerSamples);

                var header = await SendQualificationCommandAsync(
                    "Recorder Ring header",
                    cancellationToken,
                    () => diagnostics.GetRecorderHeaderAsync(
                        identity,
                        CancellationToken.None));
                AssertRecorderHeader(
                    header,
                    identity,
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderStopReason.TriggerComplete,
                    true,
                    preTriggerSamples);

                SetQualificationProgress(68, "Downloading forced-trigger Ring data");
                var data = await DownloadRecorderQualificationAsync(
                    diagnostics,
                    identity,
                    context.Capabilities.MaxChunkDataBytes,
                    cancellationToken,
                    "RING");
                AssertRecorderData(
                    data,
                    identity,
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderStopReason.TriggerComplete,
                    true,
                    preTriggerSamples);
                var hash = ComputeRecorderQualificationSha256(data.Data);
                WriteQualificationLog(
                    "event=ASSERT",
                    "name=RingForcedTrigger",
                    "stopReason=" + terminal.StopReason,
                    "triggerIndex=" + terminal.TriggerIndex,
                    "samples=" + terminal.SampleCount,
                    "bytes=" + data.Data.Length,
                    "sha256=" + hash,
                    "chunkCoverage=QUALIFICATION_GATED_EXACT",
                    "duplicateChunks=0",
                    "gapChunks=0",
                    "verdict=PASS");
                SetQualificationProgress(88, "Ring forced-trigger assertions PASS; releasing resources");
            }
            catch (Exception error)
            {
                primaryError = error;
                throw;
            }
            finally
            {
                await CleanupRecorderQualificationPreservingPrimaryAsync(
                    diagnostics,
                    handle,
                    identity,
                    "RING",
                    primaryError);
            }
        }

        private async Task RunRecorderReconnectQualificationAsync(
            bool discoverActive,
            CancellationToken cancellationToken)
        {
            const uint sampleCapacity = 1000;
            const uint preTriggerSamples = 100;
            const uint postTriggerSamples = 899;
            var context = await PrepareRecorderQualificationAsync(
                sampleCapacity,
                true,
                cancellationToken);
            if (discoverActive
                && (context.Capabilities.RecorderBufferCount != 1
                    || context.Capabilities.Supports(
                        LMCDiagnosticCapability.RecorderDoubleBank)))
            {
                ThrowRecorderQualificationSkip(
                    "0/0 active Recorder discovery is defined only for a single-bank PLC.");
            }

            var endpoint = CaptureRecorderReconnectEndpoint();
            var originalConnection = RequireConnection();
            var originalDiagnostics = originalConnection.Diagnostics;
            LMCRecorderConfigurationHandle handle = null;
            LMCRecorderIdentity originalIdentity = null;
            LMCRecorderIdentity adoptedIdentity = null;
            LMCConnection adoptedConnection = null;
            RecorderReconnectExpectation expectation = null;
            var adoptionValidated = false;
            Exception primaryError = null;
            var scope = discoverActive
                ? "RECONNECT_DISCOVERY"
                : "RECONNECT_EXACT";

            try
            {
                SetQualificationProgress(
                    8,
                    "Configuring active Ring Recorder for reconnect/adopt");
                var configuration = BuildForcedTriggerRecorderConfiguration(
                    context,
                    sampleCapacity,
                    preTriggerSamples,
                    postTriggerSamples);
                handle = await SendQualificationCommandAsync(
                    "Recorder reconnect Configure",
                    cancellationToken,
                    () => originalDiagnostics.ConfigureRecorderAsync(
                        configuration,
                        CancellationToken.None),
                    value => handle = value);
                AssertRecorderConfigurationHandle(
                    handle,
                    context,
                    sampleCapacity,
                    LMCRecorderBufferMode.Ring,
                    LMCRecorderTriggerType.Edge);

                originalIdentity = await SendQualificationCommandAsync(
                    "Recorder reconnect Start",
                    cancellationToken,
                    () => originalDiagnostics.StartRecorderAsync(
                        handle,
                        CancellationToken.None),
                    value => originalIdentity = value);
                AssertRecorderIdentity(originalIdentity, handle, context);
                expectation = CreateRecorderReconnectExpectation(
                    handle,
                    originalIdentity,
                    context);
                UpdateRecorderAdoptionFields(originalIdentity);
                WriteQualificationLog(
                    "event=RECORDER_IDENTITY_CHECKPOINT",
                    "mode=" + (discoverActive ? "DISCOVERY_0_0" : "EXACT"),
                    "bootId=0x" + expectation.DiagnosticsBootId.ToString("X8"),
                    "recordId=" + expectation.RecordId,
                    "bufferId=" + expectation.BufferId,
                    "ownerSessionEpoch=" + expectation.OwnerSessionEpoch,
                    "verdict=PASS");

                SetQualificationProgress(
                    18,
                    "Waiting for active Ring pre-history before disconnect");
                var activeStatus = await WaitForRecorderStateAsync(
                    originalDiagnostics,
                    originalIdentity,
                    handle,
                    value => value.State == LMCRecorderState.Recording
                        && value.SampleCount >= preTriggerSamples,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    cancellationToken,
                    "RECONNECT_PREHISTORY");
                if (activeStatus.IsFrozen)
                {
                    throw new InvalidOperationException(
                        "Reconnect Recorder froze before the intentional connection close.");
                }

                WriteQualificationLog(
                    "event=RECORDER_IDENTITY_PRESERVED",
                    "mode=" + (discoverActive ? "DISCOVERY_0_0" : "EXACT"),
                    "bootId=0x" + expectation.DiagnosticsBootId.ToString("X8"),
                    "recordId=" + expectation.RecordId,
                    "bufferId=" + expectation.BufferId,
                    "ownerSessionEpoch=" + expectation.OwnerSessionEpoch,
                    "configId=" + expectation.ConfigId,
                    "configRevision=" + expectation.ConfigRevision,
                    "mapRevision=0x" + expectation.MapRevision.ToString("X8"),
                    "bufferMode=" + expectation.BufferMode,
                    "triggerType=" + expectation.TriggerType,
                    "preTrigger=" + expectation.PreTriggerSamples,
                    "postTrigger=" + expectation.PostTriggerSamples,
                    "signals=" + FormatRecorderQualificationSignalIds(
                        expectation.SignalIds),
                    "samplesBeforeClose=" + activeStatus.SampleCount);

                SetQualificationProgress(
                    28,
                    "Closing the owning RPC connection while Recorder remains active");
                await CloseRecorderQualificationConnectionAsync(
                    originalConnection,
                    cancellationToken);

                WriteQualificationLog(
                    "event=CONNECTION_CLOSED",
                    "resourceReleased=false",
                    "identityPreserved=true");

                SetQualificationProgress(
                    38,
                    "Reconnecting and refreshing diagnostics capabilities");
                adoptedConnection = await OpenRecorderQualificationConnectionAsync(
                    endpoint,
                    cancellationToken,
                    false);
                var capabilities =
                    await RefreshRecorderReconnectCapabilitiesAsync(
                        adoptedConnection,
                        expectation,
                        discoverActive,
                        cancellationToken,
                        false);
                ApplyRecorderReconnectCapabilitiesToUi(capabilities);

                SetQualificationProgress(
                    50,
                    discoverActive
                        ? "Adopting the preserved Recorder through the 0/0 discovery sentinel"
                        : "Adopting the preserved Recorder by exact RecordId/BufferId");
                var diagnostics = adoptedConnection.Diagnostics;
                adoptedIdentity = await AdoptRecorderReconnectAsync(
                    diagnostics,
                    expectation,
                    discoverActive,
                    cancellationToken,
                    false,
                    "Recorder reconnect",
                    value => adoptedIdentity = value);
                WriteQualificationLog(
                    "event=RECORDER_ADOPT_RESPONSE",
                    "mode=" + (discoverActive ? "DISCOVERY_0_0" : "EXACT"),
                    "expectedBootId=0x"
                        + expectation.DiagnosticsBootId.ToString("X8"),
                    "actualBootId=0x"
                        + adoptedIdentity.DiagnosticsBootId.ToString("X8"),
                    "expectedRecordId=" + expectation.RecordId,
                    "actualRecordId=" + adoptedIdentity.RecordId,
                    "expectedBufferId=" + expectation.BufferId,
                    "actualBufferId=" + adoptedIdentity.BufferId,
                    "oldOwnerSessionEpoch=" + expectation.OwnerSessionEpoch,
                    "actualOwnerSessionEpoch="
                        + adoptedIdentity.OwnerSessionEpoch,
                    "initialState=" + adoptedIdentity.InitialState);
                AssertRecorderReconnectAdoption(
                    adoptedIdentity,
                    expectation);
                adoptionValidated = true;
                WriteQualificationLog(
                    "event=RECORDER_ADOPTED",
                    "mode=" + (discoverActive ? "DISCOVERY_0_0" : "EXACT"),
                    "bootId=0x" + adoptedIdentity.DiagnosticsBootId.ToString("X8"),
                    "recordId=" + adoptedIdentity.RecordId,
                    "bufferId=" + adoptedIdentity.BufferId,
                    "oldOwnerSessionEpoch=" + expectation.OwnerSessionEpoch,
                    "newOwnerSessionEpoch=" + adoptedIdentity.OwnerSessionEpoch,
                    "initialState=" + adoptedIdentity.InitialState,
                    "verdict=PASS");

                SetQualificationProgress(
                    60,
                    "Reading adopted Recorder status and stopping it if still active");
                var reconnectStateOperations =
                    CreateRecorderReconnectStateOperations(
                        diagnostics,
                        adoptedIdentity,
                        expectation,
                        cancellationToken,
                        false,
                        scope + "_ACTIVE");
                var releasableState =
                    await RecorderQualificationCleanupOrchestrator
                        .EnsureReleasableStateAsync(
                            reconnectStateOperations,
                            RecorderQualificationRpcTimeoutMilliseconds,
                            RecorderQualificationPollMilliseconds);
                var status = releasableState.Status;
                var stopSent = releasableState.StopAttempted;

                SetQualificationProgress(
                    70,
                    "Reading frozen adopted header and downloading immutable data");
                var header = await SendQualificationCommandAsync(
                    "Recorder reconnect adopted Header",
                    cancellationToken,
                    () => diagnostics.GetRecorderHeaderAsync(
                        adoptedIdentity,
                        CancellationToken.None));
                AssertRecorderReconnectHeader(
                    header,
                    adoptedIdentity,
                    handle,
                    context);
                var data = await DownloadRecorderQualificationAsync(
                    diagnostics,
                    adoptedIdentity,
                    capabilities.MaxChunkDataBytes,
                    cancellationToken,
                    scope);
                AssertRecorderData(
                    data,
                    adoptedIdentity,
                    handle,
                    context,
                    header.SampleCount,
                    header.StopReason,
                    header.HasTrigger,
                    header.TriggerIndex);
                WriteQualificationLog(
                    "event=ASSERT",
                    "name=RecorderReconnectAdoptDownload",
                    "mode=" + (discoverActive ? "DISCOVERY_0_0" : "EXACT"),
                    "stopSent=" + stopSent,
                    "stopReason=" + header.StopReason,
                    "samples=" + header.SampleCount,
                    "bytes=" + data.Data.Length,
                    "sha256=" + ComputeRecorderQualificationSha256(data.Data),
                    "identityMatch=PASS",
                    "bootIdMatch=PASS",
                    "ownerEpochChanged=PASS",
                    "verdict=PASS");
                SetQualificationProgress(
                    88,
                    "Reconnect/adopt assertions PASS; releasing adopted resources");
            }
            catch (Exception error)
            {
                primaryError = error;
                throw;
            }
            finally
            {
                var originalSessionUsable =
                    ReferenceEquals(connection, originalConnection)
                    && originalConnection.IsConnected;
                var cleanupRoute = RecorderReconnectQualificationPolicy
                    .SelectCleanupRoute(
                        originalSessionUsable,
                        expectation != null,
                        adoptedIdentity != null,
                        adoptionValidated);
                WriteQualificationLog(
                    "event=RECORDER_CLEANUP_ROUTE",
                    "route=" + cleanupRoute,
                    "originalSessionUsable=" + originalSessionUsable,
                    "hasExpectation=" + (expectation != null),
                    "hasAdoptedIdentity=" + (adoptedIdentity != null),
                    "adoptionValidated=" + adoptionValidated);

                if (cleanupRoute
                    == RecorderReconnectCleanupRoute.OriginalSession)
                {
                    await CleanupRecorderQualificationPreservingPrimaryAsync(
                        originalDiagnostics,
                        handle,
                        originalIdentity,
                        scope + "_ORIGINAL_SESSION",
                        primaryError);
                }
                else if (cleanupRoute
                    == RecorderReconnectCleanupRoute.ExactReconnect)
                {
                    var cleanupOwnership =
                        await CleanupRecorderReconnectPreservingPrimaryAsync(
                            endpoint,
                            expectation,
                            adoptedConnection,
                            adoptedIdentity,
                            adoptionValidated,
                            scope,
                            primaryError);
                    adoptedConnection = cleanupOwnership.Connection;
                    adoptedIdentity = cleanupOwnership.Identity;
                }
                else
                {
                    if (adoptedIdentity != null
                        && !adoptionValidated
                        && adoptedConnection != null
                        && adoptedConnection.IsConnected
                        && ReferenceEquals(connection, adoptedConnection))
                    {
                        PreserveUnvalidatedRecorderAdoption(
                            adoptedIdentity,
                            scope,
                            primaryError);
                    }
                    WriteQualificationLog(
                        "event=CLEANUP_RECOVERY_REQUIRED",
                        "scope=" + scope,
                        "reason=no_safe_automatic_cleanup_route",
                        "automaticMutation=false",
                        "verdict=FAIL");
                    var recoveryError = new InvalidOperationException(
                        "Recorder reconnect cleanup has no safe automatic route. The original session is unavailable and no fully validated exact-recovery identity is available.");
                    if (primaryError == null)
                    {
                        throw recoveryError;
                    }

                    throw CreateRecorderQualificationCleanupException(
                        scope,
                        primaryError,
                        recoveryError);
                }
            }

            if (adoptedIdentity == null
                || !adoptedIdentity.IsBufferReleased
                || !adoptedIdentity.IsRecorderReleased)
            {
                throw new InvalidOperationException(
                    "Reconnect/adopt qualification did not release both adopted Recorder resources.");
            }

            WriteQualificationLog(
                "event=ASSERT",
                "name=RecorderReconnectAdoptCleanup",
                "mode=" + (discoverActive ? "DISCOVERY_0_0" : "EXACT"),
                "bufferReleased=true",
                "configurationReleased=true",
                "verdict=PASS");
        }

        private async Task RunRecorderSoakQualificationAsync(
            CancellationToken cancellationToken)
        {
            const uint sampleCapacity = 32;
            const uint preTriggerSamples = 16;
            const uint postTriggerSamples = 15;
            var iterations = ParseQualificationPositiveInt32(
                TextQualificationRecorderIterations.Text,
                "Recorder qualification iterations");
            if (iterations > 1000)
            {
                throw new InvalidOperationException(
                    "Recorder qualification iterations must not exceed 1000.");
            }

            var context = await PrepareRecorderQualificationAsync(
                sampleCapacity,
                true,
                cancellationToken);
            var diagnostics = RequireConnection().Diagnostics;
            var completed = 0;
            var resourceBusyCount = 0;
            var droppedSamples = 0UL;
            var overflowCount = 0UL;
            var stopwatch = Stopwatch.StartNew();

            for (var iteration = 1; iteration <= iterations; iteration++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                LMCRecorderConfigurationHandle handle = null;
                LMCRecorderIdentity identity = null;
                Exception primaryError = null;
                try
                {
                    var configuration = BuildForcedTriggerRecorderConfiguration(
                        context,
                        sampleCapacity,
                        preTriggerSamples,
                        postTriggerSamples);
                    handle = await SendQualificationCommandAsync(
                        "Recorder soak Configure " + iteration,
                        cancellationToken,
                        () => diagnostics.ConfigureRecorderAsync(
                            configuration,
                            CancellationToken.None),
                        value => handle = value);
                    AssertRecorderConfigurationHandle(
                        handle,
                        context,
                        sampleCapacity,
                        LMCRecorderBufferMode.Ring,
                        LMCRecorderTriggerType.Edge);

                    identity = await SendQualificationCommandAsync(
                        "Recorder soak Start " + iteration,
                        cancellationToken,
                        () => diagnostics.StartRecorderAsync(
                            handle,
                            CancellationToken.None),
                        value => identity = value);
                    AssertRecorderIdentity(identity, handle, context);

                    await WaitForRecorderStateAsync(
                        diagnostics,
                        identity,
                        handle,
                        value => value.State == LMCRecorderState.Recording
                            && value.SampleCount >= preTriggerSamples,
                        RecorderQualificationRpcTimeoutMilliseconds,
                        cancellationToken,
                        "SOAK_PREHISTORY_" + iteration);

                    await SendQualificationCommandAsync(
                        "Recorder soak forced Trigger " + iteration,
                        cancellationToken,
                        () => diagnostics.TriggerRecorderAsync(
                            identity,
                            CancellationToken.None));

                    var terminal = await WaitForRecorderStateAsync(
                        diagnostics,
                        identity,
                        handle,
                        value => value.State == LMCRecorderState.Ready,
                        RecorderQualificationRpcTimeoutMilliseconds,
                        cancellationToken,
                        "SOAK_READY_" + iteration);
                    AssertRecorderTerminalStatus(
                        terminal,
                        handle,
                        sampleCapacity,
                        LMCRecorderStopReason.TriggerComplete,
                        true,
                        preTriggerSamples);

                    var header = await SendQualificationCommandAsync(
                        "Recorder soak header " + iteration,
                        cancellationToken,
                        () => diagnostics.GetRecorderHeaderAsync(
                            identity,
                            CancellationToken.None));
                    AssertRecorderHeader(
                        header,
                        identity,
                        handle,
                        context,
                        sampleCapacity,
                        LMCRecorderStopReason.TriggerComplete,
                        true,
                        preTriggerSamples);

                    var data = await DownloadRecorderQualificationAsync(
                        diagnostics,
                        identity,
                        context.Capabilities.MaxChunkDataBytes,
                        cancellationToken,
                        "SOAK_" + iteration);
                    AssertRecorderData(
                        data,
                        identity,
                        handle,
                        context,
                        sampleCapacity,
                        LMCRecorderStopReason.TriggerComplete,
                        true,
                        preTriggerSamples);

                    droppedSamples += header.DroppedSamples;
                    overflowCount += header.OverflowCount;
                    completed++;
                    var hash = ComputeRecorderQualificationSha256(data.Data);
                    WriteQualificationLog(
                        "event=SOAK_ITERATION",
                        "iteration=" + iteration,
                        "recordId=" + identity.RecordId,
                        "bufferId=" + identity.BufferId,
                        "configRevision=" + identity.ConfigRevision,
                        "samples=" + header.SampleCount,
                        "bytes=" + data.Data.Length,
                        "sha256=" + hash,
                        "dropped=" + header.DroppedSamples,
                        "overflow=" + header.OverflowCount,
                        "verdict=PASS");
                }
                catch (LMCDiagnosticsCommandException error)
                {
                    primaryError = error;
                    if (error.Response != null
                        && error.Response.Detail == LMCDiagnosticsDetailCode.ResourceBusy)
                    {
                        resourceBusyCount++;
                    }

                    WriteQualificationLog(
                        "event=SOAK_ITERATION",
                        "iteration=" + iteration,
                        "verdict=FAIL",
                        "detail=" + (error.Response == null
                            ? "none"
                            : error.Response.Detail.ToString()),
                        "error=" + QualificationValue(error.Message));
                    throw;
                }
                catch (Exception error)
                {
                    primaryError = error;
                    WriteQualificationLog(
                        "event=SOAK_ITERATION",
                        "iteration=" + iteration,
                        "verdict=FAIL",
                        "errorType=" + error.GetType().Name,
                        "error=" + QualificationValue(error.Message));
                    throw;
                }
                finally
                {
                    await CleanupRecorderQualificationPreservingPrimaryAsync(
                        diagnostics,
                        handle,
                        identity,
                        "SOAK_" + iteration,
                        primaryError);
                }

                var progress = 5 + (int)(90L * iteration / iterations);
                SetQualificationProgress(
                    progress,
                    "Recorder trigger soak " + iteration + "/" + iterations);
                if (iteration % 10 == 0 || iteration == iterations)
                {
                    WriteQualificationLog(
                        "event=SOAK_PROGRESS",
                        "completed=" + completed,
                        "requested=" + iterations,
                        "resourceBusy=" + resourceBusyCount,
                        "dropped=" + droppedSamples,
                        "overflow=" + overflowCount);
                }
            }

            stopwatch.Stop();
            if (completed != iterations
                || resourceBusyCount != 0
                || droppedSamples != 0
                || overflowCount != 0)
            {
                throw new InvalidOperationException(
                    "Recorder soak summary did not satisfy the zero-error contract.");
            }

            WriteQualificationLog(
                "event=ASSERT",
                "name=RecorderTriggerSoak",
                "completed=" + completed,
                "requested=" + iterations,
                "resourceBusy=0",
                "dropped=0",
                "overflow=0",
                "elapsedMs=" + stopwatch.ElapsedMilliseconds,
                "rtEvidence=NOT_MEASURED_BY_WPF",
                "verdict=PASS");
        }

        private async Task<RecorderQualificationContext>
            PrepareRecorderQualificationAsync(
                uint sampleCapacity,
                bool requireTrigger,
                CancellationToken cancellationToken)
        {
            return await PrepareRecorderQualificationAsync(
                sampleCapacity,
                requireTrigger,
                false,
                cancellationToken);
        }

        private async Task<RecorderQualificationContext>
            PrepareRecorderQualificationAsync(
                uint sampleCapacity,
                bool requireTrigger,
                bool requireDouble,
                CancellationToken cancellationToken)
        {
            EnsureNoActiveRecorderQualificationConflict();
            var connection = RequireConnection();
            var capabilities = await SendQualificationCommandAsync(
                "Recorder qualification capabilities",
                cancellationToken,
                () => connection.Diagnostics.GetCapabilitiesAsync(
                    CancellationToken.None));
            if (!capabilities.Response.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Diagnostics capability refresh did not succeed.");
            }

            if (!capabilities.HasStableDiagnosticsBootId)
            {
                throw new InvalidOperationException(
                    "Recorder qualification requires a stable non-zero DiagnosticsBootId.");
            }

            RequireRecorderQualificationCapability(
                capabilities,
                LMCDiagnosticCapability.SignalCatalog,
                "SignalCatalog");

            RequireRecorderQualificationCapability(
                capabilities,
                LMCDiagnosticCapability.RecorderSingleBank,
                "RecorderSingleBank");
            if (requireTrigger)
            {
                RequireRecorderQualificationCapability(
                    capabilities,
                    LMCDiagnosticCapability.RecorderTrigger,
                    "RecorderTrigger");
            }

            if (requireDouble)
            {
                RequireRecorderQualificationCapability(
                    capabilities,
                    LMCDiagnosticCapability.RecorderDoubleBank,
                    "RecorderDoubleBank");
                if (capabilities.RecorderBufferCount != 2)
                {
                    ThrowRecorderQualificationSkip(
                        "Double-bank qualification requires exactly two Recorder buffers; advertised count="
                        + capabilities.RecorderBufferCount
                        + ".");
                }
            }

            if (capabilities.MaxRecorderChannels
                < RecorderQualificationChannelCount)
            {
                ThrowRecorderQualificationSkip(
                    "MaxRecorderChannels="
                    + capabilities.MaxRecorderChannels
                    + " cannot run the required 4-channel qualification.");
            }

            if (capabilities.MaxRecorderSamples < sampleCapacity)
            {
                ThrowRecorderQualificationSkip(
                    "MaxRecorderSamples="
                    + capabilities.MaxRecorderSamples
                    + " is smaller than required capacity "
                    + sampleCapacity
                    + ".");
            }

            if (capabilities.MaxChunkDataBytes
                < RecorderQualificationChannelCount * sizeof(uint))
            {
                ThrowRecorderQualificationSkip(
                    "MaxChunkDataBytes="
                    + capabilities.MaxChunkDataBytes
                    + " is smaller than one 4-channel sample stride.");
            }

            var requiredBytes = checked(
                (ulong)sampleCapacity
                * RecorderQualificationChannelCount
                * sizeof(uint));
            if (capabilities.RecorderBytesPerBank < requiredBytes)
            {
                ThrowRecorderQualificationSkip(
                    "RecorderBytesPerBank="
                    + capabilities.RecorderBytesPerBank
                    + " is smaller than required bytes "
                    + requiredBytes
                    + ".");
            }

            var catalog = RequireDiagnosticCatalog();
            if (catalog.Info.MapRevision != capabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "The loaded PI Catalog MapRevision is stale. Reload the PI Catalog before qualification.");
            }

            var signals = catalog.Entries
                .Where(
                    entry => (entry.AccessFlags & LMCSignalAccessFlags.Recordable)
                        == LMCSignalAccessFlags.Recordable)
                .Take(RecorderQualificationChannelCount)
                .ToArray();
            if (signals.Length != RecorderQualificationChannelCount)
            {
                throw new InvalidOperationException(
                    "The loaded Catalog does not contain four Recordable signals in Catalog order.");
            }

            var triggerSignal = signals.FirstOrDefault(
                entry => IsRecorderQualificationEdgeType(entry.DataType));
            if (requireTrigger && triggerSignal == null)
            {
                throw new InvalidOperationException(
                    "None of the first four Catalog-order Recordable signals supports an Edge trigger value type.");
            }

            var context = new RecorderQualificationContext
            {
                Capabilities = capabilities,
                Signals = signals,
                SignalIds = signals.Select(entry => entry.SignalId).ToArray(),
                TriggerSignal = triggerSignal
            };
            WriteQualificationLog(
                "event=PRECHECK",
                "capabilityBits=0x" + capabilities.CapabilityBits.ToString("X8"),
                "bootId=0x" + capabilities.DiagnosticsBootId.ToString("X8"),
                "mapRevision=0x" + capabilities.MapRevision.ToString("X8"),
                "signals=" + FormatRecorderQualificationSignalIds(
                    context.SignalIds),
                "aliases=" + QualificationValue(
                    string.Join(",", signals.Select(entry => entry.Alias))),
                "doubleAdvertised=" + capabilities.Supports(
                    LMCDiagnosticCapability.RecorderDoubleBank),
                "doubleMode=" + (requireDouble ? "REQUESTED" : "NOT_TESTED"),
                "doubleReason=" + (requireDouble
                    ? "Recoverable Manual Double-bank retained lifecycle"
                    : "This scenario covers Single or Ring only"));
            return context;
        }

        private RecorderReconnectEndpoint CaptureRecorderReconnectEndpoint()
        {
            return new RecorderReconnectEndpoint
            {
                RemoteIp = RequiredText(TextRemoteIp.Text, "PLC IP"),
                RemotePort = ParsePort(
                    TextRemotePort.Text,
                    "TCP port",
                    false),
                LocalIp = RequiredText(TextLocalIp.Text, "PC local IPv4"),
                CallbackPort = ParsePort(
                    TextCallbackPort.Text,
                    "Callback UDP port",
                    true)
            };
        }

        private static RecorderReconnectExpectation
            CreateRecorderReconnectExpectation(
                LMCRecorderConfigurationHandle handle,
                LMCRecorderIdentity identity,
                RecorderQualificationContext context)
        {
            return new RecorderReconnectExpectation
            {
                DiagnosticsBootId = identity.DiagnosticsBootId,
                RecordId = identity.RecordId,
                BufferId = identity.BufferId,
                OwnerSessionEpoch = identity.OwnerSessionEpoch,
                ConfigId = handle.ConfigId,
                ConfigRevision = handle.ConfigRevision,
                MapRevision = handle.MapRevision,
                Capacity = handle.AcceptedCapacity,
                SamplePeriodUs = handle.SamplePeriodUs,
                ChannelCount = handle.ChannelCount,
                BufferMode = handle.Configuration.BufferMode,
                TriggerType = handle.Configuration.TriggerType,
                PreTriggerSamples = handle.Configuration.PreTriggerSamples,
                PostTriggerSamples = handle.Configuration.PostTriggerSamples,
                SignalIds = context.SignalIds.ToArray()
            };
        }

        private async Task CloseRecorderQualificationConnectionAsync(
            LMCConnection expectedConnection,
            CancellationToken cancellationToken)
        {
            Exception closeError = null;
            var priorConnectionTransition = connectionTransitionRunning;
            connectionTransitionRunning = true;
            UpdateUiState();
            try
            {
                await SendQualificationCommandAsync(
                    "Recorder reconnect CloseConnection",
                    cancellationToken,
                    () => expectedConnection.CloseConnectionAsync(
                        CancellationToken.None));
            }
            catch (Exception error)
            {
                closeError = error;
            }
            finally
            {
                if (ReferenceEquals(connection, expectedConnection))
                {
                    connection = null;
                }

                DetachConnection(expectedConnection);
                expectedConnection.Dispose();
                ClearLoadedObjects();
                connectionTransitionRunning = priorConnectionTransition;
                UpdateUiState();
            }

            if (closeError != null)
            {
                ExceptionDispatchInfo.Capture(closeError).Throw();
            }
        }

        private async Task<LMCConnection>
            OpenRecorderQualificationConnectionAsync(
                RecorderReconnectEndpoint endpoint,
                CancellationToken cancellationToken,
                bool cleanupGate)
        {
            var priorConnectionTransition = connectionTransitionRunning;
            connectionTransitionRunning = true;
            UpdateUiState();
            LMCConnection newConnection = null;
            try
            {
                var priorConnection = connection;
                if (priorConnection != null)
                {
                    if (priorConnection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "Recorder reconnect cannot replace an active unexpected connection.");
                    }

                    connection = null;
                    DetachConnection(priorConnection);
                    priorConnection.Dispose();
                    ClearLoadedObjects();
                }

                newConnection = CreateCoordinatedConnection();
                AttachConnection(newConnection);
                connection = newConnection;
                ClearLoadedObjects();
                UpdateUiState();

                Func<Task> connect = () =>
                    newConnection.RpcInitConnectionAsync(
                        endpoint.RemoteIp,
                        endpoint.RemotePort,
                        endpoint.LocalIp,
                        endpoint.CallbackPort,
                        LMCConnection.DefaultEventMask,
                        CancellationToken.None);
                if (cleanupGate)
                {
                    await SendQualificationCleanupCommandAsync(
                        "Recorder reconnect cleanup RpcInitConnection",
                        connect);
                }
                else
                {
                    await SendQualificationCommandAsync(
                        "Recorder reconnect RpcInitConnection",
                        cancellationToken,
                        connect);
                }

                WriteQualificationLog(
                    "event=CONNECTION_REOPENED",
                    "endpoint=" + QualificationValue(
                        endpoint.RemoteIp + ":" + endpoint.RemotePort),
                    "callback=" + QualificationValue(
                        newConnection.CallbackLocalEndPoint == null
                            ? "none"
                            : newConnection.CallbackLocalEndPoint.ToString()));
                return newConnection;
            }
            catch
            {
                if (newConnection != null
                    && ReferenceEquals(connection, newConnection))
                {
                    connection = null;
                }

                if (newConnection != null)
                {
                    DetachConnection(newConnection);
                    newConnection.Dispose();
                }

                ClearLoadedObjects();
                throw;
            }
            finally
            {
                connectionTransitionRunning = priorConnectionTransition;
                UpdateUiState();
            }
        }

        private async Task<LMCDiagnosticCapabilities>
            RefreshRecorderReconnectCapabilitiesAsync(
                LMCConnection currentConnection,
                RecorderReconnectExpectation expectation,
                bool discoverActive,
                CancellationToken cancellationToken,
                bool cleanupGate)
        {
            LMCDiagnosticCapabilities capabilities;
            if (cleanupGate)
            {
                capabilities = await SendQualificationCleanupCommandAsync(
                    "Recorder reconnect cleanup capabilities",
                    () => currentConnection.Diagnostics.GetCapabilitiesAsync(
                        CancellationToken.None));
            }
            else
            {
                capabilities = await SendQualificationCommandAsync(
                    "Recorder reconnect capabilities",
                    cancellationToken,
                    () => currentConnection.Diagnostics.GetCapabilitiesAsync(
                        CancellationToken.None));
            }

            AssertRecorderReconnectCapabilities(
                capabilities,
                expectation,
                discoverActive);
            WriteQualificationLog(
                "event=RECONNECT_CAPABILITIES",
                "bootId=0x" + capabilities.DiagnosticsBootId.ToString("X8"),
                "mapRevision=0x" + capabilities.MapRevision.ToString("X8"),
                "recorderBuffers=" + capabilities.RecorderBufferCount,
                "sameBoot=PASS",
                "sameMapRevision=PASS");
            return capabilities;
        }

        private void ApplyRecorderReconnectCapabilitiesToUi(
            LMCDiagnosticCapabilities capabilities)
        {
            diagnosticCapabilities = capabilities;
            TextDiagnosticsCapabilities.Text = FormatCapabilities(capabilities);
            UpdateRecorderBufferModeOptions();
            UpdateUiState();
        }

        private static void AssertRecorderReconnectCapabilities(
            LMCDiagnosticCapabilities capabilities,
            RecorderReconnectExpectation expectation,
            bool discoverActive)
        {
            var requiredBytes = checked(
                (ulong)expectation.Capacity
                * expectation.ChannelCount
                * sizeof(uint));
            if (capabilities == null
                || capabilities.Response == null
                || !capabilities.Response.IsSuccess
                || !capabilities.HasStableDiagnosticsBootId
                || capabilities.DiagnosticsBootId
                    != expectation.DiagnosticsBootId
                || capabilities.MapRevision != expectation.MapRevision
                || !capabilities.Supports(
                    LMCDiagnosticCapability.RecorderSingleBank)
                || !capabilities.Supports(
                    LMCDiagnosticCapability.RecorderTrigger)
                || capabilities.MaxRecorderChannels
                    < expectation.ChannelCount
                || capabilities.MaxRecorderSamples < expectation.Capacity
                || capabilities.MaxChunkDataBytes
                    < expectation.ChannelCount * sizeof(uint)
                || capabilities.RecorderBytesPerBank < requiredBytes
                || (discoverActive
                    && (capabilities.RecorderBufferCount != 1
                        || capabilities.Supports(
                            LMCDiagnosticCapability.RecorderDoubleBank))))
            {
                throw new InvalidOperationException(
                    "Reconnect capabilities do not match the preserved Recorder resource or adoption mode.");
            }
        }

        private static void AssertRecorderReconnectAdoption(
            LMCRecorderIdentity adoptedIdentity,
            RecorderReconnectExpectation expectation)
        {
            RecorderQualificationCleanupOrchestrator
                .ValidateReconnectAdoption(
                    adoptedIdentity,
                    expectation.DiagnosticsBootId,
                    expectation.RecordId,
                    expectation.BufferId,
                    expectation.MapRevision,
                    expectation.OwnerSessionEpoch);
        }

        private async Task<LMCRecorderIdentity> AdoptRecorderReconnectAsync(
            LMCDiagnostics diagnostics,
            RecorderReconnectExpectation expectation,
            bool discoverActive,
            CancellationToken cancellationToken,
            bool cleanupGate,
            string operation,
            Action<LMCRecorderIdentity> preserveBeforeResultApplication)
        {
            var stopwatch = Stopwatch.StartNew();
            var attempt = 0;
            while (true)
            {
                attempt++;
                try
                {
                    Func<Task<LMCRecorderIdentity>> adopt = discoverActive
                        ? (Func<Task<LMCRecorderIdentity>>)(() =>
                            diagnostics.AdoptActiveRecorderAsync(
                                expectation.DiagnosticsBootId,
                                CancellationToken.None))
                        : () => diagnostics.AdoptRecorderAsync(
                            expectation.DiagnosticsBootId,
                            expectation.RecordId,
                            expectation.BufferId,
                            CancellationToken.None);
                    return cleanupGate
                        ? await SendQualificationCleanupCommandAsync(
                            operation + " exact Adopt",
                            adopt,
                            preserveBeforeResultApplication)
                        : await SendQualificationCommandAsync(
                            operation
                                + (discoverActive
                                    ? " 0/0 discovery Adopt"
                                    : " exact Adopt"),
                            cancellationToken,
                            adopt,
                            preserveBeforeResultApplication);
                }
                catch (LMCDiagnosticsCommandException error)
                    when (error.Response != null
                        && error.Response.Detail
                            == LMCDiagnosticsDetailCode.HandleOrGenerationStale
                        && stopwatch.ElapsedMilliseconds
                            < RecorderReconnectAdoptTimeoutMilliseconds)
                {
                    WriteQualificationLog(
                        "event=RECORDER_ADOPT_RETRY",
                        "operation=" + QualificationValue(operation),
                        "mode=" + (discoverActive ? "DISCOVERY_0_0" : "EXACT"),
                        "attempt=" + attempt,
                        "detail=" + error.Response.Detail,
                        "elapsedMs=" + stopwatch.ElapsedMilliseconds);
                    if (cleanupGate)
                    {
                        await Task.Delay(RecorderQualificationPollMilliseconds);
                    }
                    else
                    {
                        await Task.Delay(
                            RecorderQualificationPollMilliseconds,
                            cancellationToken);
                    }
                }
            }
        }

        private static void AssertRecorderReconnectStatus(
            LMCRecorderStatus status,
            LMCRecorderIdentity identity,
            RecorderReconnectExpectation expectation)
        {
            RecorderQualificationCleanupOrchestrator
                .ValidateReconnectStatus(
                    status,
                    identity,
                    expectation.DiagnosticsBootId,
                    expectation.RecordId,
                    expectation.BufferId,
                    expectation.ConfigId,
                    expectation.ConfigRevision,
                    expectation.MapRevision,
                    expectation.Capacity);
        }

        private static void AssertRecorderReconnectHeader(
            LMCRecorderHeader header,
            LMCRecorderIdentity identity,
            LMCRecorderConfigurationHandle handle,
            RecorderQualificationContext context)
        {
            if (header == null
                || header.SampleCount == 0
                || header.SampleCount > handle.AcceptedCapacity
                || (header.StopReason != LMCRecorderStopReason.UserStop
                    && header.StopReason
                        != LMCRecorderStopReason.TriggerComplete)
                || (header.HasTrigger
                    && header.TriggerIndex
                        != handle.Configuration.PreTriggerSamples)
                || (!header.HasTrigger
                    && header.TriggerIndex != uint.MaxValue))
            {
                throw new InvalidOperationException(
                    "Adopted Recorder header does not satisfy the reconnect qualification terminal contract.");
            }

            AssertRecorderHeader(
                header,
                identity,
                handle,
                context,
                header.SampleCount,
                header.StopReason,
                header.HasTrigger,
                header.TriggerIndex);
        }

        private RecorderQualificationCleanupOperations
            CreateRecorderReconnectStateOperations(
                LMCDiagnostics diagnostics,
                LMCRecorderIdentity identity,
                RecorderReconnectExpectation expectation,
                CancellationToken cancellationToken,
                bool cleanupGate,
                string scope)
        {
            return new RecorderQualificationCleanupOperations
            {
                ReadStatusAsync = () => cleanupGate
                    ? SendQualificationCleanupCommandAsync(
                        "Recorder adopted cleanup Status " + scope,
                        () => diagnostics.GetRecorderStatusAsync(
                            identity,
                            CancellationToken.None))
                    : SendQualificationCommandAsync(
                        "Recorder reconnect adopted Status " + scope,
                        cancellationToken,
                        () => diagnostics.GetRecorderStatusAsync(
                            identity,
                            CancellationToken.None)),
                StopAsync = () => cleanupGate
                    ? SendQualificationCleanupCommandAsync(
                        "Recorder adopted cleanup Stop " + scope,
                        () => diagnostics.StopRecorderAsync(
                            identity,
                            CancellationToken.None))
                    : SendQualificationCommandAsync(
                        "Recorder reconnect adopted Stop " + scope,
                        cancellationToken,
                        () => diagnostics.StopRecorderAsync(
                            identity,
                            CancellationToken.None)),
                ValidateStatus = status => AssertRecorderReconnectStatus(
                    status,
                    identity,
                    expectation),
                DelayAsync = milliseconds => cleanupGate
                    ? Task.Delay(milliseconds)
                    : Task.Delay(milliseconds, cancellationToken),
                StopRaceResolved = status => WriteQualificationLog(
                    "event=RECORDER_STOP_RACE_RESOLVED",
                    "scope=" + scope,
                    "state=" + status.State,
                    "stopReason=" + status.StopReason,
                    "verdict=PASS"),
                RecoveryRequired = status => WriteQualificationLog(
                    "event=CLEANUP_RECOVERY_REQUIRED",
                    "scope=" + scope,
                    "state=" + status.State,
                    "recordId=" + identity.RecordId,
                    "bufferId=" + identity.BufferId,
                    "automaticRelease=false",
                    "verdict=FAIL"),
                IsBufferReleasePending = () => !identity.IsBufferReleased,
                IsConfigurationReleasePending = () =>
                    !identity.IsRecorderReleased,
                ReleaseBufferAsync = async () =>
                {
                    await SendQualificationCleanupCommandAsync(
                        "Recorder adopted cleanup buffer Release " + scope,
                        () => diagnostics.ReleaseRecorderBufferAsync(
                            identity,
                            CancellationToken.None));
                    WriteQualificationLog(
                        "event=CLEANUP_BUFFER_RELEASE",
                        "scope=" + scope,
                        "releasePath=ADOPTED_IDENTITY",
                        "recordId=" + identity.RecordId,
                        "bufferId=" + identity.BufferId,
                        "verdict=PASS");
                },
                ReleaseConfigurationAsync = async () =>
                {
                    await SendQualificationCleanupCommandAsync(
                        "Recorder adopted cleanup configuration Release "
                            + scope,
                        () => diagnostics.ReleaseRecorderAsync(
                            identity,
                            CancellationToken.None));
                    WriteQualificationLog(
                        "event=CLEANUP_CONFIG_RELEASE",
                        "scope=" + scope,
                        "releasePath=ADOPTED_IDENTITY",
                        "configId=" + expectation.ConfigId,
                        "verdict=PASS");
                }
            };
        }

        private RecorderQualificationCleanupOperations
            CreateRecorderOwnedCleanupOperations(
                LMCDiagnostics diagnostics,
                LMCRecorderConfigurationHandle handle,
                LMCRecorderIdentity identity,
                string scope)
        {
            return new RecorderQualificationCleanupOperations
            {
                ReadStatusAsync = () => SendQualificationCleanupCommandAsync(
                    "Recorder cleanup Status " + scope,
                    () => diagnostics.GetRecorderStatusAsync(
                        identity,
                        CancellationToken.None)),
                StopAsync = () => SendQualificationCleanupCommandAsync(
                    "Recorder cleanup Stop " + scope,
                    () => diagnostics.StopRecorderAsync(
                        identity,
                        CancellationToken.None)),
                ValidateStatus = status => AssertRecorderStatusIdentity(
                    status,
                    identity,
                    handle),
                DelayAsync = milliseconds => Task.Delay(milliseconds),
                StopRaceResolved = status => WriteQualificationLog(
                    "event=RECORDER_STOP_RACE_RESOLVED",
                    "scope=" + scope,
                    "state=" + status.State,
                    "stopReason=" + status.StopReason,
                    "verdict=PASS"),
                RecoveryRequired = status => WriteQualificationLog(
                    "event=CLEANUP_RECOVERY_REQUIRED",
                    "scope=" + scope,
                    "state=" + status.State,
                    "bufferReleased=false",
                    "configurationReleased=false",
                    "verdict=FAIL"),
                IsBufferReleasePending = () => identity != null
                    && !identity.IsBufferReleased,
                IsConfigurationReleasePending = () => handle != null
                    && !handle.IsReleased,
                ReleaseBufferAsync = async () =>
                {
                    await SendQualificationCleanupCommandAsync(
                        "Recorder cleanup buffer Release " + scope,
                        () => diagnostics.ReleaseRecorderBufferAsync(
                            identity,
                            CancellationToken.None));
                    WriteQualificationLog(
                        "event=CLEANUP_BUFFER_RELEASE",
                        "scope=" + scope,
                        "releasePath=ORIGINAL_IDENTITY",
                        "recordId=" + identity.RecordId,
                        "bufferId=" + identity.BufferId,
                        "verdict=PASS");
                },
                ReleaseConfigurationAsync = async () =>
                {
                    await SendQualificationCleanupCommandAsync(
                        "Recorder cleanup configuration Release " + scope,
                        () => diagnostics.ReleaseRecorderAsync(
                            handle,
                            CancellationToken.None));
                    WriteQualificationLog(
                        "event=CLEANUP_CONFIG_RELEASE",
                        "scope=" + scope,
                        "configId=" + handle.ConfigId,
                        "releasePath=CONFIGURATION_HANDLE",
                        "verdict=PASS");
                }
            };
        }

        private async Task<RecorderReconnectOwnership>
            CleanupRecorderReconnectPreservingPrimaryAsync(
                RecorderReconnectEndpoint endpoint,
                RecorderReconnectExpectation expectation,
                LMCConnection adoptedConnection,
                LMCRecorderIdentity adoptedIdentity,
                bool adoptionValidated,
                string scope,
                Exception primaryError)
        {
            RecorderReconnectOwnership ownership = null;
            await RecorderQualificationCleanupOrchestrator
                .CleanupAndRethrowPrimaryAsync(
                    primaryError,
                    async () =>
                    {
                        try
                        {
                            ownership = await CleanupRecorderReconnectAsync(
                                endpoint,
                                expectation,
                                adoptedConnection,
                                adoptedIdentity,
                                adoptionValidated,
                                scope);
                        }
                        catch (Exception cleanupError)
                        {
                            if (recorderIdentity != null
                                && !recorderIdentity.IsRecorderReleased)
                            {
                                PreserveRecorderQualificationRecovery(
                                    null,
                                    recorderIdentity,
                                    scope,
                                    cleanupError);
                            }

                            WriteQualificationLog(
                                "event=CLEANUP_RECOVERY_REQUIRED",
                                "scope=" + scope,
                                "bootId=0x"
                                    + expectation.DiagnosticsBootId.ToString("X8"),
                                "recordId=" + expectation.RecordId,
                                "bufferId=" + expectation.BufferId,
                                "primaryError=" + QualificationValue(
                                    primaryError == null
                                        ? "none"
                                        : primaryError.GetType().Name + ": "
                                            + primaryError.Message),
                                "cleanupError=" + QualificationValue(
                                    cleanupError.GetType().Name + ": "
                                    + cleanupError.Message),
                                "automaticRelease=false",
                                "recoveryHandle=" + (recorderIdentity == null
                                    ? "UNAVAILABLE"
                                    : "PRESERVED_IN_MANUAL_RECORDER_UI"),
                                "verdict=FAIL");
                            throw;
                        }
                    },
                    (primary, cleanup) =>
                        CreateRecorderQualificationCleanupException(
                            scope,
                            primary,
                            cleanup));
            return ownership;
        }

        private async Task<RecorderReconnectOwnership>
            CleanupRecorderReconnectAsync(
                RecorderReconnectEndpoint endpoint,
                RecorderReconnectExpectation expectation,
                LMCConnection adoptedConnection,
                LMCRecorderIdentity adoptedIdentity,
                bool adoptionValidated,
                string scope)
        {
            RecorderReconnectQualificationPolicy
                .EnsureAutomaticCleanupAllowed(
                    adoptedIdentity != null,
                    adoptionValidated);

            var currentConnection = adoptedConnection;
            var identity = adoptedIdentity;
            var ownsIdentity = currentConnection != null
                && ReferenceEquals(connection, currentConnection)
                && currentConnection.IsConnected
                && identity != null;
            if (!ownsIdentity)
            {
                currentConnection = connection;
                if (currentConnection == null || !currentConnection.IsConnected)
                {
                    currentConnection =
                        await OpenRecorderQualificationConnectionAsync(
                            endpoint,
                            CancellationToken.None,
                            true);
                }

                var capabilities =
                    await RefreshRecorderReconnectCapabilitiesAsync(
                        currentConnection,
                        expectation,
                        false,
                        CancellationToken.None,
                        true);
                ApplyRecorderReconnectCapabilitiesToUi(capabilities);
                identity = await AdoptRecorderReconnectAsync(
                    currentConnection.Diagnostics,
                    expectation,
                    false,
                    CancellationToken.None,
                    true,
                    "Recorder reconnect cleanup",
                    value =>
                    {
                        identity = value;
                        recorderConfiguration = null;
                        recorderIdentity = value;
                        recorderQualificationRecoveryReleaseOnly = true;
                        recorderQualificationRecoveryStatusConfirmed = false;
                        recorderStatus = null;
                    });
                AssertRecorderReconnectAdoption(identity, expectation);
                WriteQualificationLog(
                    "event=CLEANUP_RECOVERY_ADOPT",
                    "scope=" + scope,
                    "recordId=" + identity.RecordId,
                    "bufferId=" + identity.BufferId,
                    "newOwnerSessionEpoch=" + identity.OwnerSessionEpoch,
                    "verdict=PASS");
            }

            recorderConfiguration = null;
            recorderIdentity = identity;
            recorderQualificationRecoveryReleaseOnly = false;
            recorderQualificationRecoveryStatusConfirmed = false;
            recorderStatus = null;
            await CleanupAdoptedRecorderQualificationAsync(
                currentConnection.Diagnostics,
                identity,
                expectation,
                scope);
            if (ReferenceEquals(recorderIdentity, identity)
                && identity.IsRecorderReleased)
            {
                recorderIdentity = null;
            }

            return new RecorderReconnectOwnership(
                currentConnection,
                identity);
        }

        private async Task CleanupAdoptedRecorderQualificationAsync(
            LMCDiagnostics diagnostics,
            LMCRecorderIdentity identity,
            RecorderReconnectExpectation expectation,
            string scope)
        {
            var operations = CreateRecorderReconnectStateOperations(
                diagnostics,
                identity,
                expectation,
                CancellationToken.None,
                true,
                scope + "_ADOPTED");
            await RecorderQualificationCleanupOrchestrator
                .CleanupOwnedResourcesAsync(
                    operations,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    RecorderQualificationPollMilliseconds);
        }

        private void EnsureNoActiveRecorderQualificationConflict()
        {
            if ((recorderConfiguration != null
                    && !recorderConfiguration.IsReleased)
                || (recorderIdentity != null
                    && !recorderIdentity.IsRecorderReleased))
            {
                throw new InvalidOperationException(
                    "Release the Recorder resource owned by the manual UI before starting qualification.");
            }
        }

        private void RequireRecorderQualificationCapability(
            LMCDiagnosticCapabilities capabilities,
            LMCDiagnosticCapability capability,
            string name)
        {
            if (!capabilities.Supports(capability))
            {
                ThrowRecorderQualificationSkip(
                    name + " is not advertised by the connected PLC.");
            }
        }

        private void ThrowRecorderQualificationSkip(string reason)
        {
            WriteQualificationLog(
                "event=SKIP",
                "reason=" + QualificationValue(reason));
            throw new NotSupportedException("SKIP: " + reason);
        }

        private static bool IsRecorderQualificationEdgeType(
            LMCSignalValueType valueType)
        {
            return valueType >= LMCSignalValueType.Bool
                && valueType <= LMCSignalValueType.BitField32;
        }

        private static LMCRecorderConfiguration
            BuildForcedTriggerRecorderConfiguration(
                RecorderQualificationContext context,
                uint sampleCapacity,
                uint preTriggerSamples,
                uint postTriggerSamples)
        {
            return new LMCRecorderConfiguration(
                context.SignalIds,
                1,
                sampleCapacity,
                LMCRecorderBufferMode.Ring,
                LMCRecorderTriggerType.Edge,
                context.TriggerSignal.DataType,
                preTriggerSamples,
                postTriggerSamples,
                context.TriggerSignal.SignalId,
                LMCRecorderTriggerOperator.RisingEdge,
                RecorderQualificationUnreachableEdgeThreshold(
                    context.TriggerSignal.DataType),
                0);
        }

        private static uint RecorderQualificationUnreachableEdgeThreshold(
            LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                    return 1;
                case LMCSignalValueType.Int16:
                    return 0x00007FFFu;
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return 0x0000FFFFu;
                case LMCSignalValueType.Int32:
                    return 0x7FFFFFFFu;
                case LMCSignalValueType.Real32:
                    return 0x7F7FFFFFu;
                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.BitField32:
                    return uint.MaxValue;
                default:
                    throw new NotSupportedException(
                        "The selected Catalog signal type cannot be used for an Edge trigger.");
            }
        }

        private async Task<LMCRecorderStatus> WaitForRecorderStateAsync(
            LMCDiagnostics diagnostics,
            LMCRecorderIdentity identity,
            LMCRecorderConfigurationHandle handle,
            Func<LMCRecorderStatus, bool> predicate,
            int timeoutMilliseconds,
            CancellationToken cancellationToken,
            string stage)
        {
            var stopwatch = Stopwatch.StartNew();
            var poll = 0;
            LMCRecorderState? lastState = null;
            while (stopwatch.ElapsedMilliseconds <= timeoutMilliseconds)
            {
                var status = await SendQualificationCommandAsync(
                    "Recorder status " + stage,
                    cancellationToken,
                    () => diagnostics.GetRecorderStatusAsync(
                        identity,
                        CancellationToken.None));
                poll++;
                AssertRecorderStatusIdentity(status, identity, handle);
                if (!lastState.HasValue
                    || lastState.Value != status.State
                    || poll % 10 == 0
                    || predicate(status))
                {
                    WriteQualificationLog(
                        "event=RECORDER_POLL",
                        "stage=" + stage,
                        "poll=" + poll,
                        "state=" + status.State,
                        "samples=" + status.SampleCount,
                        "stopReason=" + status.StopReason,
                        "triggerIndex=" + (status.HasTrigger
                            ? status.TriggerIndex.ToString(
                                CultureInfo.InvariantCulture)
                            : "none"));
                }

                if (status.State == LMCRecorderState.Fault)
                {
                    throw new InvalidOperationException(
                        stage + " entered Recorder Fault state with StopReason="
                        + status.StopReason
                        + ".");
                }

                if (predicate(status))
                {
                    return status;
                }

                if (status.IsFrozen)
                {
                    throw new InvalidOperationException(
                        stage + " reached terminal state "
                        + status.State
                        + " before the expected condition.");
                }

                lastState = status.State;
                await Task.Delay(
                    RecorderQualificationPollMilliseconds,
                    cancellationToken);
            }

            throw new TimeoutException(
                stage + " did not reach the expected Recorder state within "
                + timeoutMilliseconds
                + " ms.");
        }

        private async Task<RecorderQualificationDownload>
            DownloadRecorderQualificationAsync(
                LMCDiagnostics diagnostics,
                LMCRecorderIdentity identity,
                ushort maxChunkDataBytes,
                CancellationToken cancellationToken,
                string stage)
        {
            // The SDK convenience downloader owns several wire requests in a
            // single call. Qualification performs the same exact-coverage
            // checks per request so Stop/PowerOff can win the app gate between
            // Recorder chunks.
            var header = await SendQualificationCommandAsync(
                "Recorder download header " + stage,
                cancellationToken,
                () => diagnostics.GetRecorderHeaderAsync(
                    identity,
                    CancellationToken.None));
            RecorderQualificationCleanupOrchestrator
                .ThrowIfCancellationRequestedAfterRpc(cancellationToken);
            if (header == null)
            {
                throw new InvalidOperationException(
                    "Recorder download returned no header object.");
            }

            if (header.SampleStrideBytes == 0)
            {
                throw new InvalidOperationException(
                    "Recorder download header has zero sample stride.");
            }

            var totalByteCount64 = checked(
                (ulong)header.SampleCount * header.SampleStrideBytes);
            if (totalByteCount64 > int.MaxValue)
            {
                throw new InvalidOperationException(
                    "Recorder data exceeds the maximum contiguous PC buffer size.");
            }

            var totalByteCount = (int)totalByteCount64;
            var data = new byte[totalByteCount];
            if (header.SampleCount == 0)
            {
                return new RecorderQualificationDownload(header, data, 0);
            }

            var maxSamplesPerChunk = maxChunkDataBytes
                / header.SampleStrideBytes;
            if (maxSamplesPerChunk == 0)
            {
                throw new InvalidOperationException(
                    "MaxChunkDataBytes cannot carry one Recorder sample.");
            }

            if (maxSamplesPerChunk > ushort.MaxValue)
            {
                maxSamplesPerChunk = ushort.MaxValue;
            }

            uint offsetSample = 0;
            uint sequence = 1;
            uint completedChunks = 0;
            while (offsetSample < header.SampleCount)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remaining = header.SampleCount - offsetSample;
                var requestedSampleCount = checked((ushort)Math.Min(
                    remaining,
                    maxSamplesPerChunk));
                var request = new LMCRecorderChunkRequest(
                    identity,
                    offsetSample,
                    requestedSampleCount,
                    sequence);
                var chunk = await SendQualificationCommandAsync(
                    "Recorder download chunk " + stage,
                    cancellationToken,
                    () => diagnostics.ReadRecorderChunkAsync(
                        request,
                        CancellationToken.None));
                RecorderQualificationCleanupOrchestrator
                    .ThrowIfCancellationRequestedAfterRpc(cancellationToken);
                if (chunk == null)
                {
                    throw new InvalidOperationException(
                        "Recorder chunk download returned no data object.");
                }

                var reachesEnd = checked(
                    offsetSample + chunk.ReturnedSampleCount)
                    == header.SampleCount;
                var expectedChunkBytes = checked(
                    chunk.ReturnedSampleCount * header.SampleStrideBytes);
                if (chunk.Response == null
                    || !chunk.Response.IsSuccess
                    || chunk.DiagnosticsBootId != identity.DiagnosticsBootId
                    || chunk.RecordId != identity.RecordId
                    || chunk.BufferId != identity.BufferId
                    || chunk.OffsetSample != offsetSample
                    || chunk.Sequence != sequence
                    || chunk.ReturnedSampleCount == 0
                    || chunk.ReturnedSampleCount > requestedSampleCount
                    || chunk.TotalSamples != header.SampleCount
                    || chunk.ChannelCount != header.ChannelCount
                    || chunk.SampleStrideBytes != header.SampleStrideBytes
                    || chunk.DataByteCount != expectedChunkBytes
                    || chunk.IsLastChunk != reachesEnd)
                {
                    throw new InvalidOperationException(
                        "Recorder chunk identity, coverage, or immutable metadata changed during gated download.");
                }

                var chunkData = chunk.Data.ToArray();
                var destinationOffset = checked(
                    (int)(offsetSample * header.SampleStrideBytes));
                Buffer.BlockCopy(
                    chunkData,
                    0,
                    data,
                    destinationOffset,
                    chunkData.Length);
                offsetSample = checked(
                    offsetSample + chunk.ReturnedSampleCount);
                completedChunks++;
                sequence = unchecked(sequence + 1);
                if (sequence == 0)
                {
                    sequence = 1;
                }
            }

            RecorderQualificationCleanupOrchestrator
                .ThrowIfCancellationRequestedAfterRpc(cancellationToken);

            WriteQualificationLog(
                "event=RECORDER_DOWNLOAD",
                "stage=" + stage,
                "samples=" + header.SampleCount,
                "bytes=" + totalByteCount,
                "chunks=" + completedChunks,
                "gate=PER_RPC",
                "verdict=PASS");
            return new RecorderQualificationDownload(
                header,
                data,
                completedChunks);
        }

        private async Task CleanupRecorderQualificationPreservingPrimaryAsync(
            LMCDiagnostics diagnostics,
            LMCRecorderConfigurationHandle handle,
            LMCRecorderIdentity identity,
            string scope,
            Exception primaryError)
        {
            await RecorderQualificationCleanupOrchestrator
                .CleanupAndRethrowPrimaryAsync(
                    primaryError,
                    async () =>
                    {
                        try
                        {
                            await CleanupRecorderQualificationAsync(
                                diagnostics,
                                handle,
                                identity,
                                scope);
                        }
                        catch (Exception cleanupError)
                        {
                            PreserveRecorderQualificationRecovery(
                                handle,
                                identity,
                                scope,
                                cleanupError);
                            WriteQualificationLog(
                                "event=CLEANUP",
                                "scope=" + scope,
                                "verdict=FAIL",
                                "primaryError=" + QualificationValue(
                                    primaryError == null
                                        ? "none"
                                        : primaryError.GetType().Name + ": "
                                            + primaryError.Message),
                                "cleanupError=" + QualificationValue(
                                    cleanupError.GetType().Name + ": "
                                    + cleanupError.Message),
                                "recoveryHandle=PRESERVED_IN_MANUAL_RECORDER_UI");
                            throw;
                        }
                    },
                    (primary, cleanup) =>
                        CreateRecorderQualificationCleanupException(
                            scope,
                            primary,
                            cleanup));
        }

        private void PreserveRecorderQualificationRecovery(
            LMCRecorderConfigurationHandle handle,
            LMCRecorderIdentity identity,
            string scope,
            Exception cleanupError)
        {
            if (handle != null && !handle.IsReleased)
            {
                recorderConfiguration = handle;
            }

            if (identity != null && !identity.IsRecorderReleased)
            {
                recorderIdentity = identity;
            }

            recorderQualificationRecoveryReleaseOnly = true;
            recorderQualificationRecoveryStatusConfirmed = false;
            recorderStatus = null;
            TextRecorderSummary.Text =
                "Recorder qualification cleanup failed for "
                + scope
                + ". The same-session ownership was quarantined; inspect Status "
                + "before explicit state-aware Release cleanup. Config-only "
                + "tails can be retried without Status. Error="
                + cleanupError.Message;
            UpdateUiState();
        }

        private void PreserveUnvalidatedRecorderAdoption(
            LMCRecorderIdentity adoptedIdentity,
            string scope,
            Exception validationError)
        {
            RecorderReconnectQualificationPolicy
                .QuarantineUnvalidatedAdoption(
                    adoptedIdentity,
                    false,
                    identity =>
                    {
                        recorderConfiguration = null;
                        recorderIdentity = identity;
                        recorderQualificationRecoveryReleaseOnly = true;
                        recorderQualificationRecoveryStatusConfirmed = false;
                    });
            recorderStatus = null;
            UpdateRecorderAdoptionFields(adoptedIdentity);
            TextRecorderSummary.Text =
                "Recorder Adopt returned an ownership handle, but reconnect "
                + "identity validation failed for "
                + scope
                + ". No automatic Status, Stop, or Release command was sent. "
                    + "Read Status manually to confirm the quarantined identity. "
                    + "Release Recorder will then run the allowed state-aware "
                    + "Stop/poll/Release route. Error="
                + (validationError == null
                    ? "unknown"
                    : validationError.Message);
            WriteQualificationLog(
                "event=RECORDER_RECOVERY_HANDLE_QUARANTINED",
                "scope=" + scope,
                "recordId=" + adoptedIdentity.RecordId,
                "bufferId=" + adoptedIdentity.BufferId,
                "ownerSessionEpoch=" + adoptedIdentity.OwnerSessionEpoch,
                "automaticMutation=false",
                "statusConfirmationRequired=true",
                "recoveryHandle=PRESERVED_IN_MANUAL_RECORDER_UI",
                "verdict=FAIL");
            UpdateUiState();
        }

        private static InvalidOperationException
            CreateRecorderQualificationCleanupException(
                string scope,
                Exception primaryError,
                Exception cleanupError)
        {
            return new InvalidOperationException(
                "Recorder qualification failed and cleanup also failed for "
                + scope
                + ". Primary="
                + primaryError.GetType().Name
                + ": "
                + primaryError.Message
                + "; Cleanup="
                + cleanupError.GetType().Name
                + ": "
                + cleanupError.Message,
                new AggregateException(primaryError, cleanupError));
        }

        private async Task CleanupRecorderQualificationAsync(
            LMCDiagnostics diagnostics,
            LMCRecorderConfigurationHandle handle,
            LMCRecorderIdentity identity,
            string scope)
        {
            var operations = CreateRecorderOwnedCleanupOperations(
                diagnostics,
                handle,
                identity,
                scope);
            await RecorderQualificationCleanupOrchestrator
                .CleanupOwnedResourcesAsync(
                    operations,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    RecorderQualificationPollMilliseconds);
            if (ReferenceEquals(recorderIdentity, identity)
                && (identity == null || identity.IsRecorderReleased))
            {
                recorderIdentity = null;
            }

            if (ReferenceEquals(recorderConfiguration, handle)
                && (handle == null || handle.IsReleased))
            {
                recorderConfiguration = null;
            }
        }

        private async Task VerifyRecorderQualificationDoubleReleaseBlockedAsync(
            LMCDiagnostics diagnostics,
            LMCRecorderConfigurationHandle handle,
            LMCRecorderIdentity identity,
            CancellationToken cancellationToken)
        {
            if (handle == null
                || identity == null
                || !handle.IsReleased
                || !identity.IsBufferReleased)
            {
                throw new InvalidOperationException(
                    "Recorder double-release probe requires released buffer and configuration objects.");
            }

            var bufferBlocked = false;
            try
            {
                await SendQualificationCommandAsync(
                    "Recorder local buffer double-release probe",
                    cancellationToken,
                    () => diagnostics.ReleaseRecorderBufferAsync(
                        identity,
                        CancellationToken.None));
            }
            catch (LMCDiagnosticsCommandException error)
            {
                throw new InvalidOperationException(
                    "Recorder buffer double-release probe reached the PLC instead of the local guard.",
                    error);
            }
            catch (InvalidOperationException error)
            {
                const string expectedMessage =
                    "The Recorder buffer has already been released.";
                if (error.GetType() != typeof(InvalidOperationException)
                    || !string.Equals(
                        error.Message,
                        expectedMessage,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Recorder buffer double-release probe returned an unexpected local exception: "
                        + error.GetType().Name
                        + ": "
                        + error.Message,
                        error);
                }

                bufferBlocked = true;
            }

            var configurationBlocked = false;
            try
            {
                await SendQualificationCommandAsync(
                    "Recorder local configuration double-release probe",
                    cancellationToken,
                    () => diagnostics.ReleaseRecorderAsync(
                        handle,
                        CancellationToken.None));
            }
            catch (LMCDiagnosticsCommandException error)
            {
                throw new InvalidOperationException(
                    "Recorder configuration double-release probe reached the PLC instead of the local guard.",
                    error);
            }
            catch (InvalidOperationException error)
            {
                const string expectedMessage =
                    "The Recorder configuration has already been released.";
                if (error.GetType() != typeof(InvalidOperationException)
                    || !string.Equals(
                        error.Message,
                        expectedMessage,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Recorder configuration double-release probe returned an unexpected local exception: "
                        + error.GetType().Name
                        + ": "
                        + error.Message,
                        error);
                }

                configurationBlocked = true;
            }

            if (!bufferBlocked || !configurationBlocked)
            {
                throw new InvalidOperationException(
                    "Recorder second Release unexpectedly passed a local released-object guard.");
            }

            WriteQualificationLog(
                "event=RECORDER_DOUBLE_RELEASE",
                "bufferLocalBlock=PASS",
                "configurationLocalBlock=PASS",
                "secondWireExpected=0",
                "verdict=PASS");
        }

        private static void AssertRecorderConfigurationHandle(
            LMCRecorderConfigurationHandle handle,
            RecorderQualificationContext context,
            uint sampleCapacity,
            LMCRecorderBufferMode bufferMode,
            LMCRecorderTriggerType triggerType)
        {
            if (handle == null
                || handle.DiagnosticsBootId
                    != context.Capabilities.DiagnosticsBootId
                || handle.MapRevision != context.Capabilities.MapRevision
                || handle.ChannelCount != RecorderQualificationChannelCount
                || handle.AcceptedCapacity != sampleCapacity
                || handle.SampleStrideBytes
                    != RecorderQualificationChannelCount * sizeof(uint)
                || handle.Configuration.BufferMode != bufferMode
                || handle.Configuration.TriggerType != triggerType
                || handle.Configuration.SamplePeriodCycles != 1
                || handle.SamplePeriodUs != context.Capabilities.BaseCycleTimeUs)
            {
                throw new InvalidOperationException(
                    "Recorder configuration acceptance does not match the requested qualification shape.");
            }

            AssertRecorderSignalOrder(
                handle.SignalIds,
                context.SignalIds,
                "configuration handle");
        }

        private static void AssertRecorderIdentity(
            LMCRecorderIdentity identity,
            LMCRecorderConfigurationHandle handle,
            RecorderQualificationContext context)
        {
            if (identity == null
                || identity.DiagnosticsBootId != handle.DiagnosticsBootId
                || identity.ConfigId != handle.ConfigId
                || identity.ConfigRevision != handle.ConfigRevision
                || identity.MapRevision != handle.MapRevision
                || identity.Capacity != handle.AcceptedCapacity
                || identity.SamplePeriodUs != handle.SamplePeriodUs
                || identity.ChannelCount != handle.ChannelCount
                || identity.BufferMode != handle.Configuration.BufferMode
                || identity.TriggerType != handle.Configuration.TriggerType)
            {
                throw new InvalidOperationException(
                    "Recorder identity does not match its accepted configuration handle.");
            }

            AssertRecorderSignalOrder(
                identity.SignalIds,
                context.SignalIds,
                "Recorder identity");
        }

        private static void AssertRecorderStatusIdentity(
            LMCRecorderStatus status,
            LMCRecorderIdentity identity,
            LMCRecorderConfigurationHandle handle)
        {
            if (status == null
                || status.DiagnosticsBootId != identity.DiagnosticsBootId
                || status.RecordId != identity.RecordId
                || status.BufferId != identity.BufferId
                || status.ConfigId != handle.ConfigId
                || status.ConfigRevision != handle.ConfigRevision
                || status.MapRevision != handle.MapRevision
                || status.Capacity != handle.AcceptedCapacity)
            {
                throw new InvalidOperationException(
                    "Recorder status identity or revision changed during qualification.");
            }
        }

        private static void AssertRecorderTerminalStatus(
            LMCRecorderStatus status,
            LMCRecorderConfigurationHandle handle,
            uint sampleCapacity,
            LMCRecorderStopReason stopReason,
            bool expectTrigger,
            uint expectedTriggerIndex)
        {
            if (status.State != LMCRecorderState.Ready
                || status.StopReason != stopReason
                || status.SampleCount != sampleCapacity
                || status.Capacity != handle.AcceptedCapacity
                || status.DroppedSamples != 0
                || status.OverflowCount != 0
                || status.HasTrigger != expectTrigger
                || (expectTrigger
                    && status.TriggerIndex != expectedTriggerIndex))
            {
                throw new InvalidOperationException(
                    "Recorder terminal status does not satisfy the qualification contract.");
            }
        }

        private static void AssertRecorderHeader(
            LMCRecorderHeader header,
            LMCRecorderIdentity identity,
            LMCRecorderConfigurationHandle handle,
            RecorderQualificationContext context,
            uint sampleCapacity,
            LMCRecorderStopReason stopReason,
            bool expectTrigger,
            uint expectedTriggerIndex)
        {
            var requiredFlags = LMCRecorderHeaderFlags.CaptureComplete;
            if (expectTrigger)
            {
                requiredFlags |= LMCRecorderHeaderFlags.TriggerPresent;
            }

            if (header == null
                || header.DiagnosticsBootId != identity.DiagnosticsBootId
                || header.RecordId != identity.RecordId
                || header.BufferId != identity.BufferId
                || header.ConfigId != handle.ConfigId
                || header.ConfigRevision != handle.ConfigRevision
                || header.MapRevision != handle.MapRevision
                || header.SampleCount != sampleCapacity
                || header.Capacity != handle.AcceptedCapacity
                || header.ChannelCount != RecorderQualificationChannelCount
                || header.SampleStrideBytes
                    != RecorderQualificationChannelCount * sizeof(uint)
                || header.SamplePeriodUs != handle.SamplePeriodUs
                || header.StopReason != stopReason
                || header.DroppedSamples != 0
                || header.OverflowCount != 0
                || header.HasTrigger != expectTrigger
                || (header.HeaderFlags & requiredFlags) != requiredFlags
                || (!expectTrigger
                    && (header.HeaderFlags
                        & LMCRecorderHeaderFlags.TriggerPresent) != 0)
                || (expectTrigger
                    && header.TriggerIndex != expectedTriggerIndex))
            {
                throw new InvalidOperationException(
                    "Recorder header does not satisfy the frozen metadata contract.");
            }

            AssertRecorderSignalOrder(
                header.SignalIds,
                context.SignalIds,
                "Recorder header");
        }

        private static void AssertRecorderData(
            RecorderQualificationDownload data,
            LMCRecorderIdentity identity,
            LMCRecorderConfigurationHandle handle,
            RecorderQualificationContext context,
            uint sampleCapacity,
            LMCRecorderStopReason stopReason,
            bool expectTrigger,
            uint expectedTriggerIndex)
        {
            if (data == null)
            {
                throw new InvalidOperationException(
                    "Recorder download returned no data object.");
            }

            AssertRecorderHeader(
                data.Header,
                identity,
                handle,
                context,
                sampleCapacity,
                stopReason,
                expectTrigger,
                expectedTriggerIndex);
            var expectedBytes = checked(
                (int)(sampleCapacity
                    * RecorderQualificationChannelCount
                    * sizeof(uint)));
            if (data.Data.Length != expectedBytes)
            {
                throw new InvalidOperationException(
                    "Recorder data byte count mismatch. Expected="
                    + expectedBytes
                    + ", Actual="
                    + data.Data.Length
                    + ".");
            }
        }

        private static void AssertRecorderSignalOrder(
            IReadOnlyList<uint> actual,
            IReadOnlyList<uint> expected,
            string source)
        {
            if (actual == null
                || expected == null
                || actual.Count != expected.Count)
            {
                throw new InvalidOperationException(
                    "Recorder signal count changed in " + source + ".");
            }

            for (var index = 0; index < expected.Count; index++)
            {
                if (actual[index] != expected[index])
                {
                    throw new InvalidOperationException(
                        "Recorder Catalog signal order changed in "
                        + source
                        + " at channel "
                        + index
                        + ".");
                }
            }
        }

        private static string ComputeRecorderQualificationSha256(
            IReadOnlyList<byte> data)
        {
            var bytes = data as byte[];
            if (bytes == null)
            {
                bytes = data.ToArray();
            }

            using (var sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(bytes))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }

        private static string FormatRecorderQualificationSignalIds(
            IReadOnlyList<uint> signalIds)
        {
            return string.Join(
                ",",
                signalIds.Select(signalId => "0x" + signalId.ToString("X8")));
        }

        private sealed class RecorderQualificationContext
        {
            public LMCDiagnosticCapabilities Capabilities { get; set; }
            public LMCSignalCatalogEntry[] Signals { get; set; }
            public uint[] SignalIds { get; set; }
            public LMCSignalCatalogEntry TriggerSignal { get; set; }
        }

        private sealed class RecorderReconnectEndpoint
        {
            public string RemoteIp { get; set; }
            public int RemotePort { get; set; }
            public string LocalIp { get; set; }
            public int CallbackPort { get; set; }
        }

        private sealed class RecorderReconnectExpectation
        {
            public uint DiagnosticsBootId { get; set; }
            public uint RecordId { get; set; }
            public uint BufferId { get; set; }
            public uint OwnerSessionEpoch { get; set; }
            public uint ConfigId { get; set; }
            public uint ConfigRevision { get; set; }
            public uint MapRevision { get; set; }
            public uint Capacity { get; set; }
            public uint SamplePeriodUs { get; set; }
            public ushort ChannelCount { get; set; }
            public LMCRecorderBufferMode BufferMode { get; set; }
            public LMCRecorderTriggerType TriggerType { get; set; }
            public uint PreTriggerSamples { get; set; }
            public uint PostTriggerSamples { get; set; }
            public uint[] SignalIds { get; set; }
        }

        private sealed class RecorderReconnectOwnership
        {
            public RecorderReconnectOwnership(
                LMCConnection connection,
                LMCRecorderIdentity identity)
            {
                Connection = connection;
                Identity = identity;
            }

            public LMCConnection Connection { get; private set; }
            public LMCRecorderIdentity Identity { get; private set; }
        }

        private sealed class RecorderQualificationDownload
        {
            public RecorderQualificationDownload(
                LMCRecorderHeader header,
                byte[] data,
                uint completedChunks)
            {
                Header = header;
                Data = data;
                CompletedChunks = completedChunks;
            }

            public LMCRecorderHeader Header { get; private set; }
            public byte[] Data { get; private set; }
            public uint CompletedChunks { get; private set; }
        }
    }
}

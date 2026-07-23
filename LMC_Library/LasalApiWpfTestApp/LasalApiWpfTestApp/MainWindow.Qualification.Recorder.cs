using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

        private async void ButtonRunRecorderSoakQualification_Click(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "RecorderTriggerLifecycleSoak",
                RunRecorderSoakQualificationAsync);
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
                        CancellationToken.None));
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
                        CancellationToken.None));
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
                        CancellationToken.None));
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
                        CancellationToken.None));
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
                            CancellationToken.None));
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
                            CancellationToken.None));
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
                "doubleMode=NOT_TESTED",
                "doubleReason=Public qualification covers Single and Ring only");
            return context;
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
                        + cleanupError.Message));
                if (primaryError == null)
                {
                    throw;
                }

                throw CreateRecorderQualificationCleanupException(
                    scope,
                    primaryError,
                    cleanupError);
            }
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
            if (identity != null && !identity.IsBufferReleased)
            {
                var status = await SendQualificationCleanupCommandAsync(
                    "Recorder cleanup status " + scope,
                    () => diagnostics.GetRecorderStatusAsync(
                        identity,
                        CancellationToken.None));
                AssertRecorderStatusIdentity(status, identity, handle);
                if (status.State == LMCRecorderState.Armed
                    || status.State == LMCRecorderState.Recording)
                {
                    try
                    {
                        await SendQualificationCleanupCommandAsync(
                            "Recorder cleanup Stop " + scope,
                            () => diagnostics.StopRecorderAsync(
                                identity,
                                CancellationToken.None));
                    }
                    catch (LMCDiagnosticsCommandException error)
                        when (error.Response != null
                            && error.Response.Detail
                                == LMCDiagnosticsDetailCode.InvalidState)
                    {
                        status = await SendQualificationCleanupCommandAsync(
                            "Recorder cleanup status after rejected Stop "
                                + scope,
                            () => diagnostics.GetRecorderStatusAsync(
                                identity,
                                CancellationToken.None));
                        AssertRecorderStatusIdentity(status, identity, handle);
                        if (!status.IsFrozen)
                        {
                            throw;
                        }
                    }

                    status = await WaitForRecorderCleanupReadyAsync(
                        diagnostics,
                        identity,
                        handle,
                        scope);
                }

                if (status.State == LMCRecorderState.Fault)
                {
                    WriteQualificationLog(
                        "event=CLEANUP_RECOVERY_REQUIRED",
                        "scope=" + scope,
                        "state=Fault",
                        "bufferReleased=false",
                        "configurationReleased=false",
                        "verdict=FAIL");
                    throw new InvalidOperationException(
                        "Recorder cleanup found a Fault buffer. PLC 0x7E47 does not release Fault state; explicit recovery is required.");
                }

                if (status.State != LMCRecorderState.Ready
                    && status.State != LMCRecorderState.Uploading)
                {
                    throw new InvalidOperationException(
                        "Recorder cleanup cannot release a buffer in State="
                        + status.State
                        + ".");
                }

                await SendQualificationCleanupCommandAsync(
                    "Recorder cleanup buffer Release " + scope,
                    () => diagnostics.ReleaseRecorderBufferAsync(
                        identity,
                        CancellationToken.None));
                WriteQualificationLog(
                    "event=CLEANUP_BUFFER_RELEASE",
                    "scope=" + scope,
                    "recordId=" + identity.RecordId,
                    "bufferId=" + identity.BufferId,
                    "stateBeforeRelease=" + status.State,
                    "verdict=PASS");
            }

            if (handle != null && !handle.IsReleased)
            {
                if (identity != null && !identity.IsBufferReleased)
                {
                    throw new InvalidOperationException(
                        "Recorder configuration release was blocked because its buffer is not released.");
                }

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

        private async Task<LMCRecorderStatus> WaitForRecorderCleanupReadyAsync(
            LMCDiagnostics diagnostics,
            LMCRecorderIdentity identity,
            LMCRecorderConfigurationHandle handle,
            string scope)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds
                <= RecorderQualificationRpcTimeoutMilliseconds)
            {
                var status = await SendQualificationCleanupCommandAsync(
                    "Recorder cleanup Ready poll " + scope,
                    () => diagnostics.GetRecorderStatusAsync(
                        identity,
                        CancellationToken.None));
                AssertRecorderStatusIdentity(status, identity, handle);
                if (status.State == LMCRecorderState.Fault)
                {
                    WriteQualificationLog(
                        "event=CLEANUP_RECOVERY_REQUIRED",
                        "scope=" + scope,
                        "state=Fault",
                        "bufferReleased=false",
                        "configurationReleased=false",
                        "verdict=FAIL");
                    throw new InvalidOperationException(
                        "Recorder cleanup entered Fault state; PLC 0x7E47 cannot release this buffer.");
                }

                if (status.State == LMCRecorderState.Ready)
                {
                    return status;
                }

                await Task.Delay(RecorderQualificationPollMilliseconds);
            }

            throw new TimeoutException(
                "Recorder cleanup "
                + scope
                + " did not reach releasable Ready state within "
                + RecorderQualificationRpcTimeoutMilliseconds
                + " ms.");
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;
using Microsoft.Win32;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly List<TransportQualificationSample>
            transportQualificationSamples =
                new List<TransportQualificationSample>();
        private TransportQualificationSummary transportQualificationSummary;

        private async void ButtonRunTransportQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "Phase5ReadOnlyGroupStatusRtt",
                RunTransportQualificationAsync);
        }

        private void ButtonSaveTransportQualificationCsv_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (transportQualificationSummary == null
                && transportQualificationSamples.Count == 0)
            {
                WriteLog("Transport qualification CSV has no samples.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Save Phase 5 read-only API RPC samples",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = DateTime.Now.ToString(
                        "yyyyMMdd_HHmmss",
                        CultureInfo.InvariantCulture)
                    + "_Phase5_GroupStatus_ApiRpc.csv"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            WriteTransportQualificationCsv(
                dialog.FileName,
                transportQualificationSummary,
                transportQualificationSamples);
            WriteLog("Transport qualification CSV saved: " + dialog.FileName);
            TextOperationState.Text = "Transport qualification CSV saved";
        }

        private async Task RunTransportQualificationAsync(
            CancellationToken cancellationToken)
        {
            transportQualificationSamples.Clear();
            transportQualificationSummary = null;

            var currentGroup = RequireGroup();
            var warmupCount = ParseQualificationNonNegativeInt32(
                TextQualificationTransportWarmup.Text,
                "Transport warm-up requests");
            var measuredCount = ParseQualificationPositiveInt32(
                TextQualificationTransportIterations.Text,
                "Transport measured requests");

            TransportQualificationAnalysis.ValidateRequestCounts(
                warmupCount,
                measuredCount);

            if (groupPowerVerificationPending
                || groupPowerOffVerificationPending
                || groupProfileLockVerificationPending
                || groupStatusRefreshRequired)
            {
                throw new InvalidOperationException(
                    "Read Group Status once and finish all Group power/profile verification before starting the transport qualification.");
            }

            WriteQualificationLog(
                "event=CONFIG",
                "target=GroupReadStatus(0x2045)",
                "mode=sequential_read_only",
                "group=" + QualificationValue(currentGroup.GroupName),
                "groupReference=" + currentGroup.GroupReference,
                "warmup=" + warmupCount,
                "measured=" + measuredCount,
                "percentile=nearest_rank",
                "metricScope=PC_API_RPC_ELAPSED",
                "uiDispatchAndCommandGate=EXCLUDED",
                "plcDispatch=NOT_MEASURED");

            var warmupCompleted = 0;
            Stopwatch measurementWindow = null;

            try
            {
                SetQualificationProgress(3, "Read-only API RPC preflight");
                var preflight = await ReadQualificationGroupStatusAsync(
                    currentGroup,
                    cancellationToken);
                ValidateTransportQualificationResult(
                    "Transport qualification preflight Group Status",
                    preflight,
                    true);

                for (var index = 0; index < warmupCount; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var warmup = await ReadQualificationGroupStatusAsync(
                        currentGroup,
                        cancellationToken);
                    ValidateTransportQualificationResult(
                        "Transport qualification warm-up Group Status",
                        warmup,
                        true);
                    warmupCompleted++;

                    if ((index + 1) % Math.Max(1, warmupCount / 10) == 0
                        || index + 1 == warmupCount)
                    {
                        SetQualificationProgress(
                            3 + (int)(7L * (index + 1) / Math.Max(1, warmupCount)),
                            "Warm-up "
                            + (index + 1)
                            + "/"
                            + warmupCount);
                    }
                }

                SetQualificationProgress(10, "Measuring sequential 0x2045 API RPC");
                measurementWindow = Stopwatch.StartNew();
                var reportEvery = Math.Max(1, measuredCount / 100);
                byte[] stableRaw = null;

                for (var index = 0; index < measuredCount; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var startOffsetMicroseconds = TicksToMicroseconds(
                        measurementWindow.ElapsedTicks);
                    var timedCall = await ReadTimedTransportGroupStatusAsync(
                        currentGroup,
                        cancellationToken);
                    var result = timedCall.Result;

                    if (result == null || result.Response == null)
                    {
                        throw new InvalidDataException(
                            "Transport qualification received no Group Status response.");
                    }

                    var raw = result.Response.Raw ?? new byte[0];
                    transportQualificationSamples.Add(
                        new TransportQualificationSample
                        {
                            Iteration = index + 1,
                            StartOffsetMicroseconds = startOffsetMicroseconds,
                            ElapsedMicroseconds = timedCall.ElapsedMicroseconds,
                            ResponseBytes = raw.Length,
                            PayloadBytes = result.Response.PayloadLength,
                            HeaderStatus = result.Response.HeaderStatus,
                            FunctionStatus = result.FunctionStatus,
                            ErrorId = result.ErrorId,
                            GroupErrorId = result.GroupErrorId,
                            State = result.State,
                            IsFrameValid = result.Response.IsFrameValid,
                            Raw = (byte[])raw.Clone()
                        });

                    ValidateTransportQualificationResult(
                        "Transport qualification measured Group Status",
                        result,
                        true);

                    if (stableRaw == null)
                    {
                        stableRaw = (byte[])raw.Clone();
                    }
                    else if (!stableRaw.SequenceEqual(raw))
                    {
                        throw new InvalidDataException(
                            "Transport qualification Group Status bytes changed at measured request "
                            + (index + 1)
                            + ". A stationary, byte-stable response is required for a PASS verdict.");
                    }

                    if ((index + 1) % reportEvery == 0
                        || index + 1 == measuredCount)
                    {
                        SetQualificationProgress(
                            10 + (int)(85L * (index + 1) / measuredCount),
                            "Measured "
                            + (index + 1)
                            + "/"
                            + measuredCount
                            + " sequential reads");
                    }
                }

                measurementWindow.Stop();
                TransportQualificationAnalysis.FinalizeSampleHashes(
                    transportQualificationSamples);
                cancellationToken.ThrowIfCancellationRequested();
                transportQualificationSummary =
                    CreateTransportQualificationSummary(
                        currentGroup,
                        warmupCount,
                        warmupCompleted,
                        measuredCount,
                        measurementWindow.Elapsed,
                        transportQualificationSamples,
                        "PASS",
                        null);

                var summary = transportQualificationSummary;
                SetQualificationProgress(98, "Writing read-only API RPC summary");
                WriteQualificationLog(
                    "event=RESULT",
                    "target=GroupReadStatus(0x2045)",
                    "metricScope=PC_API_RPC_ELAPSED",
                    "uiDispatchAndCommandGate=EXCLUDED",
                    "plcDispatch=NOT_MEASURED",
                    "samples=" + summary.MeasuredCount,
                    "minUs=" + TransportQualificationAnalysis.FormatDouble(
                        summary.MinimumMicroseconds),
                    "p50Us=" + TransportQualificationAnalysis.FormatDouble(
                        summary.P50Microseconds),
                    "p95Us=" + TransportQualificationAnalysis.FormatDouble(
                        summary.P95Microseconds),
                    "p99Us=" + TransportQualificationAnalysis.FormatDouble(
                        summary.P99Microseconds),
                    "maxUs=" + TransportQualificationAnalysis.FormatDouble(
                        summary.MaximumMicroseconds),
                    "meanUs=" + TransportQualificationAnalysis.FormatDouble(
                        summary.MeanMicroseconds),
                    "rpcActivePerSec="
                        + TransportQualificationAnalysis.FormatDouble(
                            summary.RpcActiveThroughputPerSecond),
                    "wallPerSec="
                        + TransportQualificationAnalysis.FormatDouble(
                            summary.WallThroughputPerSecond),
                    "responseBytes=" + summary.ResponseByteLengths,
                    "payloadBytes=" + summary.PayloadByteLengths,
                    "uniqueRawHashes=" + summary.UniqueRawHashCount,
                    "rawStable=" + summary.RawStable,
                    "verdict=PASS");
            }
            catch (OperationCanceledException error)
            {
                FinalizePartialTransportQualification(
                    currentGroup,
                    warmupCount,
                    warmupCompleted,
                    measuredCount,
                    measurementWindow,
                    "ABORTED",
                    error);
                throw;
            }
            catch (Exception error)
            {
                FinalizePartialTransportQualification(
                    currentGroup,
                    warmupCount,
                    warmupCompleted,
                    measuredCount,
                    measurementWindow,
                    "FAIL",
                    error);
                throw;
            }
        }

        private static void ValidateTransportQualificationResult(
            string operation,
            LMCGroupReadStatusResult result,
            bool requireStationary)
        {
            EnsureGroupStatusSuccess(operation, result);

            var response = result.Response;
            var raw = response == null ? null : response.Raw;
            if (response == null
                || !response.IsFrameValid
                || response.PayloadLength != 12
                || raw == null
                || raw.Length != 20)
            {
                throw new InvalidDataException(
                    operation
                    + " must return a valid 20-byte frame with a 12-byte payload.");
            }

            if (requireStationary && !IsGroupInPosition(result))
            {
                throw new InvalidOperationException(
                    operation
                    + " requires a powered, profile-locked Group at InPosition. State=0x"
                    + result.State.ToString("X8", CultureInfo.InvariantCulture)
                    + ".");
            }
        }

        private async Task<TimedTransportGroupStatusCall>
            ReadTimedTransportGroupStatusAsync(
                LMCGroupAxis currentGroup,
                CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await commandSendGate.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureNoNewSafetyRequest(
                    qualificationSafetyGeneration,
                    "Transport qualification Group status");

                var latency = Stopwatch.StartNew();
                var result = await currentGroup.GroupReadStatusResultAsync(
                        CancellationToken.None)
                    .ConfigureAwait(false);
                latency.Stop();
                return new TimedTransportGroupStatusCall
                {
                    Result = result,
                    ElapsedMicroseconds = TicksToMicroseconds(
                        latency.ElapsedTicks)
                };
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private void FinalizePartialTransportQualification(
            LMCGroupAxis currentGroup,
            int warmupCount,
            int warmupCompleted,
            int requestedMeasuredCount,
            Stopwatch measurementWindow,
            string verdict,
            Exception error)
        {
            if (measurementWindow != null && measurementWindow.IsRunning)
            {
                measurementWindow.Stop();
            }

            TransportQualificationAnalysis.FinalizeSampleHashes(
                transportQualificationSamples);
            transportQualificationSummary =
                CreateTransportQualificationSummary(
                    currentGroup,
                    warmupCount,
                    warmupCompleted,
                    requestedMeasuredCount,
                    measurementWindow == null
                        ? TimeSpan.Zero
                        : measurementWindow.Elapsed,
                    transportQualificationSamples,
                    verdict,
                    error == null ? null : error.Message);
        }

        private TransportQualificationSummary
            CreateTransportQualificationSummary(
                LMCGroupAxis currentGroup,
                int warmupCount,
                int warmupCompleted,
                int requestedMeasuredCount,
                TimeSpan wallElapsed,
                IList<TransportQualificationSample> samples,
                string verdict,
                string error)
        {
            return TransportQualificationAnalysis.CreateSummary(
                new TransportQualificationRunMetadata
                {
                    RunId = qualificationRunId,
                    CompletedUtc = DateTime.UtcNow,
                    Endpoint = TextRemoteIp.Text + ":" + TextRemotePort.Text,
                    GroupName = currentGroup.GroupName,
                    GroupReference = currentGroup.GroupReference
                },
                warmupCount,
                warmupCompleted,
                requestedMeasuredCount,
                wallElapsed,
                samples,
                verdict,
                error);
        }

        private static double TicksToMicroseconds(long ticks)
        {
            return ticks * 1000000.0 / Stopwatch.Frequency;
        }

        private static void WriteTransportQualificationCsv(
            string path,
            TransportQualificationSummary summary,
            IList<TransportQualificationSample> samples)
        {
            using (var writer = new StreamWriter(
                path,
                false,
                new UTF8Encoding(false)))
            {
                TransportQualificationAnalysis.WriteCsv(
                    writer,
                    summary,
                    samples);
            }
        }

        private sealed class TimedTransportGroupStatusCall
        {
            internal LMCGroupReadStatusResult Result { get; set; }
            internal double ElapsedMicroseconds { get; set; }
        }

    }
}

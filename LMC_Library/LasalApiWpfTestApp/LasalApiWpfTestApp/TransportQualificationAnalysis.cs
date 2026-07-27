using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LasalMotionControlApiExample
{
    internal static class TransportQualificationAnalysis
    {
        internal const int MinimumMeasuredRequestCount = 10000;
        internal const int MaximumRequestCount = 100000;

        internal static void ValidateRequestCounts(
            int warmupCount,
            int measuredCount)
        {
            if (warmupCount < 0)
            {
                throw new InvalidOperationException(
                    "Transport warm-up request count must not be negative.");
            }

            if (measuredCount < MinimumMeasuredRequestCount)
            {
                throw new InvalidOperationException(
                    "Phase 5 transport qualification requires at least "
                    + MinimumMeasuredRequestCount
                    + " measured requests.");
            }

            if (warmupCount > MaximumRequestCount
                || measuredCount > MaximumRequestCount)
            {
                throw new InvalidOperationException(
                    "Transport warm-up and measured request counts must not exceed "
                    + MaximumRequestCount
                    + ".");
            }
        }

        internal static void FinalizeSampleHashes(
            IList<TransportQualificationSample> samples)
        {
            if (samples == null || samples.Count == 0)
            {
                return;
            }

            using (var sha256 = SHA256.Create())
            {
                for (var index = 0; index < samples.Count; index++)
                {
                    var sample = samples[index];
                    if (sample == null)
                    {
                        throw new ArgumentException(
                            "Transport samples must not contain null entries.",
                            "samples");
                    }

                    var raw = sample.Raw;
                    if (raw != null)
                    {
                        sample.RawSha256 = ToLowerHex(
                            sha256.ComputeHash(raw));
                        if (index == 0 || index == samples.Count - 1)
                        {
                            sample.RawHex = ToLowerHex(raw);
                        }
                    }

                    sample.Raw = null;
                }
            }
        }

        internal static TransportQualificationSummary CreateSummary(
            TransportQualificationRunMetadata metadata,
            int warmupCount,
            int warmupCompleted,
            int requestedMeasuredCount,
            TimeSpan wallElapsed,
            IList<TransportQualificationSample> samples,
            string verdict,
            string error)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            ValidateRequestCounts(warmupCount, requestedMeasuredCount);
            if (warmupCompleted < 0 || warmupCompleted > warmupCount)
            {
                throw new ArgumentOutOfRangeException(
                    "warmupCompleted",
                    "Completed warm-up requests must be within the requested range.");
            }

            if (wallElapsed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    "wallElapsed",
                    "Wall elapsed time must not be negative.");
            }

            if (!string.Equals(verdict, "PASS", StringComparison.Ordinal)
                && !string.Equals(verdict, "FAIL", StringComparison.Ordinal)
                && !string.Equals(verdict, "ABORTED", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Transport verdict must be PASS, FAIL, or ABORTED.",
                    "verdict");
            }

            var safeSamples = samples
                ?? new List<TransportQualificationSample>();
            if (safeSamples.Any(sample => sample == null))
            {
                throw new ArgumentException(
                    "Transport samples must not contain null entries.",
                    "samples");
            }

            var sorted = safeSamples
                .Select(sample => sample.ElapsedMicroseconds)
                .OrderBy(value => value)
                .ToArray();
            var totalMicroseconds = sorted.Sum();
            var wallSeconds = wallElapsed.TotalSeconds;
            var allRawHashesPresent = safeSamples.Count > 0
                && safeSamples.All(
                    sample => !string.IsNullOrEmpty(sample.RawSha256));
            var uniqueRawHashCount = safeSamples
                .Select(sample => sample.RawSha256)
                .Where(value => !string.IsNullOrEmpty(value))
                .Distinct(StringComparer.Ordinal)
                .Count();
            var rawStable = allRawHashesPresent
                && uniqueRawHashCount == 1;
            var allResponsesSuccessful = safeSamples.Count > 0
                && safeSamples.All(
                    sample => sample.IsFrameValid
                        && sample.ResponseBytes == 20
                        && sample.PayloadBytes == 12
                        && sample.HeaderStatus == 0
                        && sample.FunctionStatus == 0
                        && sample.ErrorId == 0
                        && sample.GroupErrorId == 0);

            if (string.Equals(verdict, "PASS", StringComparison.Ordinal)
                && (requestedMeasuredCount < MinimumMeasuredRequestCount
                    || safeSamples.Count < MinimumMeasuredRequestCount
                    || safeSamples.Count != requestedMeasuredCount
                    || warmupCompleted != warmupCount
                    || !allRawHashesPresent
                    || !rawStable
                    || !allResponsesSuccessful))
            {
                throw new InvalidOperationException(
                    "PASS requires every requested warm-up and measured request, at least "
                    + MinimumMeasuredRequestCount
                    + " measured samples, successful 20-byte/12-byte response frames, a SHA256 hash for every sample, and one byte-stable response hash.");
            }

            return new TransportQualificationSummary
            {
                RunId = metadata.RunId,
                CompletedUtc = metadata.CompletedUtc,
                Endpoint = metadata.Endpoint,
                GroupName = metadata.GroupName,
                GroupReference = metadata.GroupReference,
                WarmupCount = warmupCount,
                WarmupCompleted = warmupCompleted,
                RequestedMeasuredCount = requestedMeasuredCount,
                MeasuredCount = safeSamples.Count,
                Verdict = verdict,
                Error = error,
                WallElapsedMilliseconds = wallElapsed.TotalMilliseconds,
                MinimumMicroseconds = sorted.Length == 0 ? 0 : sorted[0],
                P50Microseconds = sorted.Length == 0
                    ? 0
                    : NearestRankPercentile(sorted, 0.50),
                P95Microseconds = sorted.Length == 0
                    ? 0
                    : NearestRankPercentile(sorted, 0.95),
                P99Microseconds = sorted.Length == 0
                    ? 0
                    : NearestRankPercentile(sorted, 0.99),
                MaximumMicroseconds = sorted.Length == 0
                    ? 0
                    : sorted[sorted.Length - 1],
                MeanMicroseconds = sorted.Length == 0
                    ? 0
                    : totalMicroseconds / sorted.Length,
                RpcActiveThroughputPerSecond = totalMicroseconds <= 0
                    ? 0
                    : safeSamples.Count * 1000000.0 / totalMicroseconds,
                WallThroughputPerSecond = wallSeconds <= 0
                    ? 0
                    : safeSamples.Count / wallSeconds,
                ResponseByteLengths = string.Join(
                    ",",
                    safeSamples.Select(sample => sample.ResponseBytes)
                        .Distinct()
                        .OrderBy(value => value)),
                PayloadByteLengths = string.Join(
                    ",",
                    safeSamples.Select(sample => sample.PayloadBytes)
                        .Distinct()
                        .OrderBy(value => value)),
                UniqueRawHashCount = uniqueRawHashCount,
                RawStable = rawStable,
                FirstRawHex = safeSamples.Count == 0
                    ? string.Empty
                    : safeSamples[0].RawHex,
                LastRawHex = safeSamples.Count == 0
                    ? string.Empty
                    : safeSamples[safeSamples.Count - 1].RawHex
            };
        }

        internal static double NearestRankPercentile(
            double[] sorted,
            double percentile)
        {
            if (sorted == null || sorted.Length == 0)
            {
                throw new ArgumentException(
                    "A percentile requires sorted samples.",
                    "sorted");
            }

            if (percentile <= 0 || percentile > 1)
            {
                throw new ArgumentOutOfRangeException(
                    "percentile",
                    "Percentile must be greater than zero and at most one.");
            }

            var rank = (int)Math.Ceiling(percentile * sorted.Length);
            var index = Math.Max(0, Math.Min(sorted.Length - 1, rank - 1));
            return sorted[index];
        }

        internal static string FormatDouble(double value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }

        internal static void WriteCsv(
            TextWriter writer,
            TransportQualificationSummary summary,
            IList<TransportQualificationSample> samples)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            var safeSamples = samples
                ?? new List<TransportQualificationSample>();
            if (safeSamples.Any(sample => sample == null))
            {
                throw new ArgumentException(
                    "Transport samples must not contain null entries.",
                    "samples");
            }

            writer.WriteLine(
                "# LASAL Phase 5 read-only transport qualification CSV v1");
            writer.WriteLine("# target=GroupReadStatus(0x2045)");
            writer.WriteLine("# metricScope=PC_API_RPC_ELAPSED");
            writer.WriteLine("# uiDispatchAndCommandGate=EXCLUDED");
            writer.WriteLine("# plcDispatch=NOT_MEASURED");
            writer.WriteLine("# taskJitter=NOT_MEASURED");
            writer.WriteLine("# overrun=NOT_MEASURED");
            writer.WriteLine("# percentile=nearest-rank");

            if (summary != null)
            {
                writer.WriteLine(
                    "# run=" + EscapeMetadata(summary.RunId));
                writer.WriteLine(
                    "# verdict=" + EscapeMetadata(summary.Verdict));
                writer.WriteLine(
                    "# endpoint=" + EscapeMetadata(summary.Endpoint));
                writer.WriteLine(
                    "# completedUtc="
                    + summary.CompletedUtc.ToString(
                        "yyyy-MM-ddTHH:mm:ss.fffZ",
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# group=" + EscapeMetadata(summary.GroupName));
                writer.WriteLine(
                    "# groupReference="
                    + summary.GroupReference.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# warmupCompleted="
                    + summary.WarmupCompleted.ToString(
                        CultureInfo.InvariantCulture)
                    + ",warmupRequested="
                    + summary.WarmupCount.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# measuredCompleted="
                    + summary.MeasuredCount.ToString(
                        CultureInfo.InvariantCulture)
                    + ",measuredRequested="
                    + summary.RequestedMeasuredCount.ToString(
                        CultureInfo.InvariantCulture));
                if (!string.IsNullOrEmpty(summary.Error))
                {
                    writer.WriteLine(
                        "# error=" + EscapeMetadata(summary.Error));
                }

                writer.WriteLine(
                    "# wallElapsedMs="
                    + FormatDouble(summary.WallElapsedMilliseconds));
                writer.WriteLine(
                    "# minUs=" + FormatDouble(summary.MinimumMicroseconds)
                    + ",p50Us=" + FormatDouble(summary.P50Microseconds)
                    + ",p95Us=" + FormatDouble(summary.P95Microseconds)
                    + ",p99Us=" + FormatDouble(summary.P99Microseconds)
                    + ",maxUs=" + FormatDouble(summary.MaximumMicroseconds)
                    + ",meanUs=" + FormatDouble(summary.MeanMicroseconds));
                writer.WriteLine(
                    "# rpcActivePerSec="
                    + FormatDouble(summary.RpcActiveThroughputPerSecond)
                    + ",wallPerSec="
                    + FormatDouble(summary.WallThroughputPerSecond));
                writer.WriteLine(
                    "# responseBytes=" + summary.ResponseByteLengths
                    + ",payloadBytes=" + summary.PayloadByteLengths
                    + ",uniqueRawHashes="
                    + summary.UniqueRawHashCount.ToString(
                        CultureInfo.InvariantCulture)
                    + ",rawStable=" + summary.RawStable);
                writer.WriteLine(
                    "# firstRawHex=" + (summary.FirstRawHex ?? string.Empty));
                writer.WriteLine(
                    "# lastRawHex=" + (summary.LastRawHex ?? string.Empty));
            }
            else
            {
                writer.WriteLine("# result=partial_or_aborted");
            }

            writer.WriteLine(
                "iteration,start_offset_us,elapsed_us,response_bytes,payload_bytes,frame_valid,header_status,function_status,error_id,group_error_id,state_hex,raw_sha256");
            foreach (var sample in safeSamples)
            {
                writer.Write(
                    sample.Iteration.ToString(CultureInfo.InvariantCulture));
                writer.Write(",");
                writer.Write(FormatDouble(sample.StartOffsetMicroseconds));
                writer.Write(",");
                writer.Write(FormatDouble(sample.ElapsedMicroseconds));
                writer.Write(",");
                writer.Write(
                    sample.ResponseBytes.ToString(
                        CultureInfo.InvariantCulture));
                writer.Write(",");
                writer.Write(
                    sample.PayloadBytes.ToString(
                        CultureInfo.InvariantCulture));
                writer.Write(",");
                writer.Write(sample.IsFrameValid ? "true" : "false");
                writer.Write(",");
                writer.Write(
                    sample.HeaderStatus.ToString(
                        CultureInfo.InvariantCulture));
                writer.Write(",");
                writer.Write(
                    sample.FunctionStatus.ToString(
                        CultureInfo.InvariantCulture));
                writer.Write(",");
                writer.Write(
                    sample.ErrorId.ToString(
                        CultureInfo.InvariantCulture));
                writer.Write(",");
                writer.Write(
                    sample.GroupErrorId.ToString(
                        CultureInfo.InvariantCulture));
                writer.Write(",0x");
                writer.Write(
                    sample.State.ToString(
                        "X8",
                        CultureInfo.InvariantCulture));
                writer.Write(",");
                writer.WriteLine(sample.RawSha256 ?? string.Empty);
            }
        }

        private static string EscapeMetadata(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string ToLowerHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            for (var index = 0; index < bytes.Length; index++)
            {
                builder.Append(
                    bytes[index].ToString(
                        "x2",
                        CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }

    internal sealed class TransportQualificationRunMetadata
    {
        internal string RunId { get; set; }
        internal DateTime CompletedUtc { get; set; }
        internal string Endpoint { get; set; }
        internal string GroupName { get; set; }
        internal ushort GroupReference { get; set; }
    }

    internal sealed class TransportQualificationSample
    {
        internal int Iteration { get; set; }
        internal double StartOffsetMicroseconds { get; set; }
        internal double ElapsedMicroseconds { get; set; }
        internal int ResponseBytes { get; set; }
        internal ushort PayloadBytes { get; set; }
        internal ushort HeaderStatus { get; set; }
        internal ushort FunctionStatus { get; set; }
        internal short ErrorId { get; set; }
        internal ushort GroupErrorId { get; set; }
        internal uint State { get; set; }
        internal bool IsFrameValid { get; set; }
        internal byte[] Raw { get; set; }
        internal string RawSha256 { get; set; }
        internal string RawHex { get; set; }
    }

    internal sealed class TransportQualificationSummary
    {
        internal string RunId { get; set; }
        internal DateTime CompletedUtc { get; set; }
        internal string Endpoint { get; set; }
        internal string GroupName { get; set; }
        internal ushort GroupReference { get; set; }
        internal int WarmupCount { get; set; }
        internal int WarmupCompleted { get; set; }
        internal int RequestedMeasuredCount { get; set; }
        internal int MeasuredCount { get; set; }
        internal string Verdict { get; set; }
        internal string Error { get; set; }
        internal double WallElapsedMilliseconds { get; set; }
        internal double MinimumMicroseconds { get; set; }
        internal double P50Microseconds { get; set; }
        internal double P95Microseconds { get; set; }
        internal double P99Microseconds { get; set; }
        internal double MaximumMicroseconds { get; set; }
        internal double MeanMicroseconds { get; set; }
        internal double RpcActiveThroughputPerSecond { get; set; }
        internal double WallThroughputPerSecond { get; set; }
        internal string ResponseByteLengths { get; set; }
        internal string PayloadByteLengths { get; set; }
        internal int UniqueRawHashCount { get; set; }
        internal bool RawStable { get; set; }
        internal string FirstRawHex { get; set; }
        internal string LastRawHex { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using LasalMotionControlApiExample;

namespace LasalMotionControlLib.Tests
{
    internal static class TransportQualificationAnalysisTests
    {
        internal static void Register(ICollection<TestCase> tests)
        {
            tests.Add(
                "Qualification.Transport.RequestCountBoundaries",
                RequestCountBoundaries);
            tests.Add(
                "Qualification.Transport.NearestRankStatistics",
                NearestRankStatisticsAndThroughput);
            tests.Add(
                "Qualification.Transport.Sha256RawCleanup",
                Sha256StabilityAndRawCleanup);
            tests.Add(
                "Qualification.Transport.PassEvidenceInvariants",
                PassEvidenceInvariants);
            tests.Add(
                "Qualification.Transport.ZeroSampleCsvMetadata",
                ZeroSampleFailureAndAbortedCsvMetadata);
            tests.Add(
                "Qualification.Transport.CsvInvariantCulture",
                CsvUsesInvariantCulture);
        }

        private static void RequestCountBoundaries()
        {
            TransportQualificationAnalysis.ValidateRequestCounts(0, 10000);
            TransportQualificationAnalysis.ValidateRequestCounts(
                100000,
                100000);

            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.ValidateRequestCounts(
                    -1,
                    10000));
            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.ValidateRequestCounts(
                    0,
                    9999));
            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.ValidateRequestCounts(
                    100001,
                    10000));
            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.ValidateRequestCounts(
                    0,
                    100001));
        }

        private static void NearestRankStatisticsAndThroughput()
        {
            var samples = new List<TransportQualificationSample>
            {
                CreateSample(1, 4.0, new byte[] { 0x20, 0x45 }),
                CreateSample(2, 1.0, new byte[] { 0x20, 0x45 }),
                CreateSample(3, 3.0, new byte[] { 0x20, 0x45 }),
                CreateSample(4, 2.0, new byte[] { 0x20, 0x45 })
            };
            TransportQualificationAnalysis.FinalizeSampleHashes(samples);

            var summary = TransportQualificationAnalysis.CreateSummary(
                CreateMetadata(),
                0,
                0,
                10000,
                TimeSpan.FromSeconds(2),
                samples,
                "ABORTED",
                "fixture stopped early");

            AssertEx.Equal(1.0, summary.MinimumMicroseconds);
            AssertEx.Equal(2.0, summary.P50Microseconds);
            AssertEx.Equal(4.0, summary.P95Microseconds);
            AssertEx.Equal(4.0, summary.P99Microseconds);
            AssertEx.Equal(4.0, summary.MaximumMicroseconds);
            AssertEx.Equal(2.5, summary.MeanMicroseconds);
            AssertEx.Equal(
                400000.0,
                summary.RpcActiveThroughputPerSecond);
            AssertEx.Equal(2.0, summary.WallThroughputPerSecond);
        }

        private static void Sha256StabilityAndRawCleanup()
        {
            var stableSamples = new List<TransportQualificationSample>
            {
                CreateSample(1, 10.0, new byte[] { 0x01, 0x02 }),
                CreateSample(2, 11.0, new byte[] { 0x01, 0x02 })
            };
            TransportQualificationAnalysis.FinalizeSampleHashes(
                stableSamples);

            AssertEx.Equal<byte[]>(null, stableSamples[0].Raw);
            AssertEx.Equal<byte[]>(null, stableSamples[1].Raw);
            AssertEx.Equal(
                stableSamples[0].RawSha256,
                stableSamples[1].RawSha256);
            AssertEx.Equal(64, stableSamples[0].RawSha256.Length);
            AssertEx.Equal("0102", stableSamples[0].RawHex);
            AssertEx.Equal("0102", stableSamples[1].RawHex);

            var stableSummary = TransportQualificationAnalysis.CreateSummary(
                CreateMetadata(),
                0,
                0,
                10000,
                TimeSpan.FromMilliseconds(25),
                stableSamples,
                "ABORTED",
                null);
            AssertEx.True(stableSummary.RawStable);
            AssertEx.Equal(1, stableSummary.UniqueRawHashCount);

            var unstableSamples = new List<TransportQualificationSample>
            {
                CreateSample(1, 10.0, new byte[] { 0x01 }),
                CreateSample(2, 11.0, new byte[] { 0x02 })
            };
            TransportQualificationAnalysis.FinalizeSampleHashes(
                unstableSamples);
            var unstableSummary = TransportQualificationAnalysis.CreateSummary(
                CreateMetadata(),
                0,
                0,
                10000,
                TimeSpan.FromMilliseconds(25),
                unstableSamples,
                "FAIL",
                "response bytes changed");

            AssertEx.Equal<byte[]>(null, unstableSamples[0].Raw);
            AssertEx.Equal<byte[]>(null, unstableSamples[1].Raw);
            AssertEx.False(unstableSummary.RawStable);
            AssertEx.Equal(2, unstableSummary.UniqueRawHashCount);
        }

        private static void PassEvidenceInvariants()
        {
            var samples = CreateStableSamples(10000);
            TransportQualificationAnalysis.FinalizeSampleHashes(samples);

            var summary = TransportQualificationAnalysis.CreateSummary(
                CreateMetadata(),
                100,
                100,
                10000,
                TimeSpan.FromSeconds(1),
                samples,
                "PASS",
                null);
            AssertEx.Equal("PASS", summary.Verdict);
            AssertEx.Equal(10000, summary.MeasuredCount);
            AssertEx.True(summary.RawStable);

            string csv;
            using (var writer = new StringWriter(
                CultureInfo.InvariantCulture))
            {
                TransportQualificationAnalysis.WriteCsv(
                    writer,
                    summary,
                    samples);
                csv = writer.ToString();
            }

            AssertEx.Contains("# verdict=PASS", csv);
            AssertEx.Contains(
                "# measuredCompleted=10000,measuredRequested=10000",
                csv);
            AssertEx.Contains("uniqueRawHashes=1,rawStable=True", csv);

            var stableHash = samples[0].RawSha256;
            samples[samples.Count - 1].RawSha256 = null;
            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.CreateSummary(
                    CreateMetadata(),
                    100,
                    100,
                    10000,
                    TimeSpan.FromSeconds(1),
                    samples,
                    "PASS",
                    null));

            samples[samples.Count - 1].RawSha256 =
                new string('0', stableHash.Length);
            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.CreateSummary(
                    CreateMetadata(),
                    100,
                    100,
                    10000,
                    TimeSpan.FromSeconds(1),
                    samples,
                    "PASS",
                    null));

            samples[samples.Count - 1].RawSha256 = stableHash;
            var incomplete = samples.Take(9999).ToList();
            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.CreateSummary(
                    CreateMetadata(),
                    100,
                    100,
                    10000,
                    TimeSpan.FromSeconds(1),
                    incomplete,
                    "PASS",
                    null));

            samples[0].IsFrameValid = false;
            AssertPassRejected(samples);
            samples[0].IsFrameValid = true;
            samples[0].ResponseBytes = 19;
            AssertPassRejected(samples);
            samples[0].ResponseBytes = 20;
            samples[0].PayloadBytes = 11;
            AssertPassRejected(samples);
            samples[0].PayloadBytes = 12;
            samples[0].HeaderStatus = 1;
            AssertPassRejected(samples);
            samples[0].HeaderStatus = 0;
            samples[0].FunctionStatus = 1;
            AssertPassRejected(samples);
            samples[0].FunctionStatus = 0;
            samples[0].ErrorId = 1;
            AssertPassRejected(samples);
            samples[0].ErrorId = 0;
            samples[0].GroupErrorId = 1;
            AssertPassRejected(samples);
            samples[0].GroupErrorId = 0;
        }

        private static void ZeroSampleFailureAndAbortedCsvMetadata()
        {
            var samples = new List<TransportQualificationSample>();
            var failure = TransportQualificationAnalysis.CreateSummary(
                CreateMetadata(),
                10,
                4,
                10000,
                TimeSpan.Zero,
                samples,
                "FAIL",
                "broken\r\n# verdict=PASS");
            var failureCsv = WriteCsv(failure, samples);

            AssertEx.Contains("# verdict=FAIL", failureCsv);
            AssertEx.Contains(
                "# warmupCompleted=4,warmupRequested=10",
                failureCsv);
            AssertEx.Contains(
                "# measuredCompleted=0,measuredRequested=10000",
                failureCsv);
            AssertEx.Contains("# error=broken # verdict=PASS", failureCsv);
            AssertEx.False(
                failureCsv.IndexOf(
                    "\n# verdict=PASS",
                    StringComparison.Ordinal) >= 0,
                "Error metadata must not inject a new CSV metadata line.");

            var aborted = TransportQualificationAnalysis.CreateSummary(
                CreateMetadata(),
                10,
                0,
                10000,
                TimeSpan.Zero,
                samples,
                "ABORTED",
                "canceled\nby operator");
            var abortedCsv = WriteCsv(aborted, samples);
            AssertEx.Contains("# verdict=ABORTED", abortedCsv);
            AssertEx.Contains(
                "# measuredCompleted=0,measuredRequested=10000",
                abortedCsv);
            AssertEx.Contains("# error=canceled by operator", abortedCsv);
        }

        private static void CsvUsesInvariantCulture()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.GetCultureInfo("fr-FR");
                var samples = new List<TransportQualificationSample>
                {
                    CreateSample(1, 1.5, new byte[] { 0x01 })
                };
                samples[0].StartOffsetMicroseconds = 2.25;
                TransportQualificationAnalysis.FinalizeSampleHashes(samples);
                var summary = TransportQualificationAnalysis.CreateSummary(
                    CreateMetadata(),
                    0,
                    0,
                    10000,
                    TimeSpan.FromTicks(12345000),
                    samples,
                    "ABORTED",
                    null);

                var csv = WriteCsv(summary, samples);
                AssertEx.Contains("# wallElapsedMs=1234.500", csv);
                AssertEx.Contains(
                    "1,2.250,1.500,20,12,true,0,0,0,0,0x00000001,",
                    csv);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        private static List<TransportQualificationSample>
            CreateStableSamples(int count)
        {
            var samples = new List<TransportQualificationSample>(count);
            var raw = new byte[] { 0x20, 0x45, 0x00, 0x00 };
            for (var index = 0; index < count; index++)
            {
                samples.Add(CreateSample(index + 1, 10.0, raw));
            }

            return samples;
        }

        private static void AssertPassRejected(
            IList<TransportQualificationSample> samples)
        {
            AssertEx.Throws<InvalidOperationException>(
                () => TransportQualificationAnalysis.CreateSummary(
                    CreateMetadata(),
                    100,
                    100,
                    10000,
                    TimeSpan.FromSeconds(1),
                    samples,
                    "PASS",
                    null));
        }

        private static TransportQualificationSample CreateSample(
            int iteration,
            double elapsedMicroseconds,
            byte[] raw)
        {
            return new TransportQualificationSample
            {
                Iteration = iteration,
                StartOffsetMicroseconds = iteration - 1,
                ElapsedMicroseconds = elapsedMicroseconds,
                ResponseBytes = 20,
                PayloadBytes = 12,
                HeaderStatus = 0,
                FunctionStatus = 0,
                ErrorId = 0,
                GroupErrorId = 0,
                State = 1,
                IsFrameValid = true,
                Raw = raw
            };
        }

        private static TransportQualificationRunMetadata CreateMetadata()
        {
            return new TransportQualificationRunMetadata
            {
                RunId = "test-run",
                CompletedUtc = new DateTime(
                    2026,
                    7,
                    24,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                Endpoint = "127.0.0.1:5000",
                GroupName = "Group1",
                GroupReference = 1
            };
        }

        private static string WriteCsv(
            TransportQualificationSummary summary,
            IList<TransportQualificationSample> samples)
        {
            using (var writer = new StringWriter(
                CultureInfo.InvariantCulture))
            {
                TransportQualificationAnalysis.WriteCsv(
                    writer,
                    summary,
                    samples);
                return writer.ToString();
            }
        }
    }
}

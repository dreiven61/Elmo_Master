using System;
using System.Threading;
using System.Threading.Tasks;

namespace LasalMotionControlLib
{
    public sealed partial class LMCDiagnostics
    {
        public Task<LMCRecorderData> DownloadRecorderAsync(
            LMCRecorderIdentity identity,
            IProgress<LMCRecorderDownloadProgress> progress,
            CancellationToken cancellationToken)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            return Task.Run(
                () => DownloadRecorderCore(
                    identity,
                    progress,
                    cancellationToken),
                CancellationToken.None);
        }

        private LMCRecorderData DownloadRecorderCore(
            LMCRecorderIdentity identity,
            IProgress<LMCRecorderDownloadProgress> progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var header = GetRecorderHeader(identity);
            cancellationToken.ThrowIfCancellationRequested();

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
                ReportRecorderProgress(progress, 0, 0, 0, 0, 0);
                return new LMCRecorderData(header, data);
            }

            var maxSamplesPerChunk = identity.MaxChunkDataBytes
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
                var chunk = ReadRecorderChunk(request);

                if (chunk.TotalSamples != header.SampleCount
                    || chunk.ChannelCount != header.ChannelCount
                    || chunk.SampleStrideBytes != header.SampleStrideBytes)
                {
                    throw new InvalidOperationException(
                        "Recorder chunk metadata changed during immutable download.");
                }

                var chunkData = chunk.CopyData();
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
                ReportRecorderProgress(
                    progress,
                    offsetSample,
                    header.SampleCount,
                    checked((int)(offsetSample * header.SampleStrideBytes)),
                    totalByteCount,
                    completedChunks);

                sequence = unchecked(sequence + 1);
                if (sequence == 0)
                {
                    sequence = 1;
                }
            }

            return new LMCRecorderData(header, data);
        }

        private static void ReportRecorderProgress(
            IProgress<LMCRecorderDownloadProgress> progress,
            uint downloadedSamples,
            uint totalSamples,
            int downloadedBytes,
            int totalBytes,
            uint completedChunks)
        {
            if (progress != null)
            {
                progress.Report(
                    new LMCRecorderDownloadProgress(
                        downloadedSamples,
                        totalSamples,
                        downloadedBytes,
                        totalBytes,
                        completedChunks));
            }
        }
    }
}

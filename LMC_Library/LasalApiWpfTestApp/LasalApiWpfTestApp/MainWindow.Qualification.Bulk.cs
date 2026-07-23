using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const int QualificationBulkExpectedSignalCount = 24;
        private const int QualificationBulkActivationTimeoutMilliseconds = 5000;
        private const int QualificationBulkStatusPollMilliseconds = 50;

        private async void ButtonRunBulkSnapshotSoakQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "Bulk24EntrySnapshotSoak",
                RunBulkSnapshotSoakQualificationAsync);
        }

        private async void ButtonRunBulkLifecycleQualification_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunQualificationAsync(
                "BulkLifecycleReleaseReuse",
                RunBulkLifecycleQualificationAsync);
        }

        private async Task RunBulkSnapshotSoakQualificationAsync(
            CancellationToken cancellationToken)
        {
            var input = ReadQualificationBulkInput();
            SetQualificationProgress(3, "Refreshing Bulk capabilities and Catalog");
            var context = await PrepareQualificationBulkContextAsync(
                cancellationToken);

            LMCPIBulkReader reader = null;
            Exception primaryError = null;
            var successCount = 0;
            var partialCount = 0;
            var invalidCount = 0;
            var errorCount = 0;
            var latencyTotalMilliseconds = 0.0;
            var latencyMinimumMilliseconds = double.MaxValue;
            var latencyMaximumMilliseconds = 0.0;
            uint firstCycle = 0;
            uint lastCycle = 0;
            uint previousCycle = 0;
            uint previousSequence = 0;
            ulong previousTimestampUs = 0;
            var hasPreviousSnapshot = false;

            try
            {
                SetQualificationProgress(8, "Configuring all 24 BulkReadable signals");
                reader = await ConfigureQualificationBulkReaderAsync(
                    context,
                    cancellationToken,
                    "snapshot-soak");
                await WaitForQualificationBulkActiveAsync(
                    reader,
                    cancellationToken,
                    "snapshot-soak");

                for (var iteration = 0; iteration < input.Iterations; iteration++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var latency = Stopwatch.StartNew();
                    LMCBulkSnapshot snapshot;
                    try
                    {
                        snapshot = await SendQualificationCommandAsync(
                            "Bulk snapshot soak upload",
                            cancellationToken,
                            () => reader.UploadAsync(CancellationToken.None));
                    }
                    catch
                    {
                        errorCount++;
                        throw;
                    }
                    finally
                    {
                        latency.Stop();
                    }

                    var invalidThisSnapshot = snapshot.Entries.Count(
                        entry => !entry.IsValid || entry.DetailCode != 0);
                    if (snapshot.IsPartial)
                    {
                        partialCount++;
                    }

                    invalidCount += invalidThisSnapshot;
                    ValidateQualificationBulkSnapshot(
                        context,
                        reader,
                        snapshot,
                        hasPreviousSnapshot,
                        previousCycle,
                        previousTimestampUs,
                        previousSequence);
                    successCount++;
                    var elapsedMilliseconds = latency.Elapsed.TotalMilliseconds;
                    latencyTotalMilliseconds += elapsedMilliseconds;
                    latencyMinimumMilliseconds = Math.Min(
                        latencyMinimumMilliseconds,
                        elapsedMilliseconds);
                    latencyMaximumMilliseconds = Math.Max(
                        latencyMaximumMilliseconds,
                        elapsedMilliseconds);

                    if (!hasPreviousSnapshot)
                    {
                        firstCycle = snapshot.CycleCounter;
                    }

                    lastCycle = snapshot.CycleCounter;
                    previousCycle = snapshot.CycleCounter;
                    previousTimestampUs = snapshot.TimestampUs;
                    previousSequence = snapshot.SnapshotSequence;
                    hasPreviousSnapshot = true;

                    WriteQualificationLog(
                        "event=BULK_SNAPSHOT",
                        "iteration=" + (iteration + 1).ToString(
                            CultureInfo.InvariantCulture),
                        "bulkId=" + snapshot.BulkId.ToString(
                            CultureInfo.InvariantCulture),
                        "configRevision=" + snapshot.ConfigRevision.ToString(
                            CultureInfo.InvariantCulture),
                        "mapRevision=0x" + snapshot.MapRevision.ToString("X8"),
                        "cycle=" + snapshot.CycleCounter.ToString(
                            CultureInfo.InvariantCulture),
                        "timestampUs=" + snapshot.TimestampUs.ToString(
                            CultureInfo.InvariantCulture),
                        "sequence=" + snapshot.SnapshotSequence.ToString(
                            CultureInfo.InvariantCulture),
                        "flags=0x" + ((uint)snapshot.SnapshotFlags).ToString("X8"),
                        "partial=" + snapshot.IsPartial,
                        "invalid=" + invalidThisSnapshot.ToString(
                            CultureInfo.InvariantCulture),
                        "latencyMs=" + elapsedMilliseconds.ToString(
                            "F3",
                            CultureInfo.InvariantCulture));

                    SetQualificationProgress(
                        15 + (75 * (iteration + 1) / input.Iterations),
                        "Bulk snapshot "
                            + (iteration + 1).ToString(CultureInfo.InvariantCulture)
                            + "/"
                            + input.Iterations.ToString(CultureInfo.InvariantCulture));

                    if (iteration + 1 < input.Iterations
                        && input.IntervalMilliseconds > 0)
                    {
                        await Task.Delay(
                            input.IntervalMilliseconds,
                            cancellationToken);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException error)
            {
                primaryError = error;
                WriteQualificationLog(
                    "event=BULK_SUMMARY",
                    "verdict=ABORTED",
                    "success=" + successCount.ToString(
                        CultureInfo.InvariantCulture),
                    "requested=" + input.Iterations.ToString(
                        CultureInfo.InvariantCulture),
                    "partialCount=" + partialCount.ToString(
                        CultureInfo.InvariantCulture),
                    "invalidCount=" + invalidCount.ToString(
                        CultureInfo.InvariantCulture),
                    "errorCount=" + errorCount.ToString(
                        CultureInfo.InvariantCulture));
                throw;
            }
            catch (Exception error)
            {
                primaryError = error;
                WriteQualificationLog(
                    "event=BULK_SUMMARY",
                    "verdict=FAIL",
                    "success=" + successCount.ToString(
                        CultureInfo.InvariantCulture),
                    "requested=" + input.Iterations.ToString(
                        CultureInfo.InvariantCulture),
                    "partialCount=" + partialCount.ToString(
                        CultureInfo.InvariantCulture),
                    "invalidCount=" + invalidCount.ToString(
                        CultureInfo.InvariantCulture),
                    "errorCount=" + Math.Max(1, errorCount).ToString(
                        CultureInfo.InvariantCulture));
                throw;
            }
            finally
            {
                try
                {
                    await ReleaseQualificationBulkReaderAsync(
                        reader,
                        "snapshot-soak");
                }
                catch (Exception cleanupError)
                {
                    if (primaryError != null)
                    {
                        throw CreateQualificationBulkCleanupException(
                            "snapshot-soak",
                            primaryError,
                            cleanupError);
                    }

                    throw;
                }
            }

            var cycleDelta = unchecked(lastCycle - firstCycle);
            var latencyAverageMilliseconds = latencyTotalMilliseconds
                / successCount;
            WriteQualificationLog(
                "event=BULK_SUMMARY",
                "verdict=PASS",
                "success=" + successCount.ToString(
                    CultureInfo.InvariantCulture),
                "requested=" + input.Iterations.ToString(
                    CultureInfo.InvariantCulture),
                "latencyMinMs=" + latencyMinimumMilliseconds.ToString(
                    "F3",
                    CultureInfo.InvariantCulture),
                "latencyAvgMs=" + latencyAverageMilliseconds.ToString(
                    "F3",
                    CultureInfo.InvariantCulture),
                "latencyMaxMs=" + latencyMaximumMilliseconds.ToString(
                    "F3",
                    CultureInfo.InvariantCulture),
                "cycleDelta=" + cycleDelta.ToString(
                    CultureInfo.InvariantCulture),
                "partialCount=" + partialCount.ToString(
                    CultureInfo.InvariantCulture),
                "invalidCount=" + invalidCount.ToString(
                    CultureInfo.InvariantCulture),
                "errorCount=" + errorCount.ToString(
                    CultureInfo.InvariantCulture),
                "cleanup=PASS");
            TextBulkSummary.Text = "Qualification PASS: "
                + successCount.ToString(CultureInfo.InvariantCulture)
                + "/"
                + input.Iterations.ToString(CultureInfo.InvariantCulture)
                + " snapshots, 24 valid entries each; resource released.";
        }

        private async Task RunBulkLifecycleQualificationAsync(
            CancellationToken cancellationToken)
        {
            var input = ReadQualificationBulkInput();
            SetQualificationProgress(3, "Refreshing Bulk capabilities and Catalog");
            var context = await PrepareQualificationBulkContextAsync(
                cancellationToken);
            var configureCount = 0;
            var activeCount = 0;
            var snapshotCount = 0;
            var cleanupCount = 0;
            var errorCount = 0;
            var latencyTotalMilliseconds = 0.0;
            var latencyMinimumMilliseconds = double.MaxValue;
            var latencyMaximumMilliseconds = 0.0;

            try
            {
                for (var iteration = 0; iteration < input.Iterations; iteration++)
                {
                    LMCPIBulkReader reader = null;
                    Exception iterationPrimaryError = null;
                    try
                    {
                        reader = await ConfigureQualificationBulkReaderAsync(
                            context,
                            cancellationToken,
                            "lifecycle-" + (iteration + 1).ToString(
                                CultureInfo.InvariantCulture));
                        configureCount++;
                        await WaitForQualificationBulkActiveAsync(
                            reader,
                            cancellationToken,
                            "lifecycle-" + (iteration + 1).ToString(
                                CultureInfo.InvariantCulture));
                        activeCount++;

                        cancellationToken.ThrowIfCancellationRequested();
                        var latency = Stopwatch.StartNew();
                        LMCBulkSnapshot snapshot;
                        try
                        {
                            snapshot = await SendQualificationCommandAsync(
                                "Bulk lifecycle upload",
                                cancellationToken,
                                () => reader.UploadAsync(
                                    CancellationToken.None));
                        }
                        finally
                        {
                            latency.Stop();
                        }

                        ValidateQualificationBulkSnapshot(
                            context,
                            reader,
                            snapshot,
                            false,
                            0,
                            0,
                            0);
                        snapshotCount++;
                        var elapsedMilliseconds = latency.Elapsed.TotalMilliseconds;
                        latencyTotalMilliseconds += elapsedMilliseconds;
                        latencyMinimumMilliseconds = Math.Min(
                            latencyMinimumMilliseconds,
                            elapsedMilliseconds);
                        latencyMaximumMilliseconds = Math.Max(
                            latencyMaximumMilliseconds,
                            elapsedMilliseconds);
                        WriteQualificationLog(
                            "event=BULK_LIFECYCLE",
                            "iteration=" + (iteration + 1).ToString(
                                CultureInfo.InvariantCulture),
                            "bulkId=" + snapshot.BulkId.ToString(
                                CultureInfo.InvariantCulture),
                            "configRevision=" + snapshot.ConfigRevision.ToString(
                                CultureInfo.InvariantCulture),
                            "cycle=" + snapshot.CycleCounter.ToString(
                                CultureInfo.InvariantCulture),
                            "latencyMs=" + elapsedMilliseconds.ToString(
                                "F3",
                                CultureInfo.InvariantCulture),
                            "snapshot=PASS");
                    }
                    catch (Exception error)
                    {
                        iterationPrimaryError = error;
                        throw;
                    }
                    finally
                    {
                        try
                        {
                            if (await ReleaseQualificationBulkReaderAsync(
                                reader,
                                "lifecycle-" + (iteration + 1).ToString(
                                    CultureInfo.InvariantCulture)))
                            {
                                cleanupCount++;
                            }
                        }
                        catch (Exception cleanupError)
                        {
                            if (iterationPrimaryError != null)
                            {
                                throw CreateQualificationBulkCleanupException(
                                    "lifecycle-" + (iteration + 1).ToString(
                                        CultureInfo.InvariantCulture),
                                    iterationPrimaryError,
                                    cleanupError);
                            }

                            throw;
                        }
                    }

                    SetQualificationProgress(
                        8 + (82 * (iteration + 1) / input.Iterations),
                        "Bulk lifecycle "
                            + (iteration + 1).ToString(CultureInfo.InvariantCulture)
                            + "/"
                            + input.Iterations.ToString(CultureInfo.InvariantCulture));
                    if (iteration + 1 < input.Iterations
                        && input.IntervalMilliseconds > 0)
                    {
                        await Task.Delay(
                            input.IntervalMilliseconds,
                            cancellationToken);
                    }
                }

                SetQualificationProgress(92, "Verifying post-soak Configure reuse");
                LMCPIBulkReader reuseReader = null;
                Exception reusePrimaryError = null;
                try
                {
                    reuseReader = await ConfigureQualificationBulkReaderAsync(
                        context,
                        cancellationToken,
                        "post-soak-reuse");
                    await WaitForQualificationBulkActiveAsync(
                        reuseReader,
                        cancellationToken,
                        "post-soak-reuse");
                    cancellationToken.ThrowIfCancellationRequested();
                    WriteQualificationLog(
                        "event=BULK_REUSE_PROBE",
                        "verdict=PASS",
                        "bulkId=" + reuseReader.Configuration.BulkId.ToString(
                            CultureInfo.InvariantCulture),
                        "configRevision="
                            + reuseReader.Configuration.ConfigRevision.ToString(
                                CultureInfo.InvariantCulture));
                }
                catch (Exception error)
                {
                    reusePrimaryError = error;
                    throw;
                }
                finally
                {
                    try
                    {
                        await ReleaseQualificationBulkReaderAsync(
                            reuseReader,
                            "post-soak-reuse");
                    }
                    catch (Exception cleanupError)
                    {
                        if (reusePrimaryError != null)
                        {
                            throw CreateQualificationBulkCleanupException(
                                "post-soak-reuse",
                                reusePrimaryError,
                                cleanupError);
                        }

                        throw;
                    }
                }

                await VerifyQualificationBulkDoubleReleaseBlockedAsync(
                    reuseReader,
                    cancellationToken);

                var latencyAverageMilliseconds = latencyTotalMilliseconds
                    / snapshotCount;
                WriteQualificationLog(
                    "event=BULK_LIFECYCLE_SUMMARY",
                    "verdict=PASS",
                    "configured=" + configureCount.ToString(
                        CultureInfo.InvariantCulture),
                    "active=" + activeCount.ToString(
                        CultureInfo.InvariantCulture),
                    "snapshots=" + snapshotCount.ToString(
                        CultureInfo.InvariantCulture),
                    "cleanup=" + cleanupCount.ToString(
                        CultureInfo.InvariantCulture),
                    "requested=" + input.Iterations.ToString(
                        CultureInfo.InvariantCulture),
                    "reuseProbe=PASS",
                    "doubleReleaseLocalBlock=PASS",
                    "latencyMinMs=" + latencyMinimumMilliseconds.ToString(
                        "F3",
                        CultureInfo.InvariantCulture),
                    "latencyAvgMs=" + latencyAverageMilliseconds.ToString(
                        "F3",
                        CultureInfo.InvariantCulture),
                    "latencyMaxMs=" + latencyMaximumMilliseconds.ToString(
                        "F3",
                        CultureInfo.InvariantCulture),
                    "errorCount=0");
                TextBulkSummary.Text = "Qualification PASS: "
                    + cleanupCount.ToString(CultureInfo.InvariantCulture)
                    + "/"
                    + input.Iterations.ToString(CultureInfo.InvariantCulture)
                    + " lifecycle resources released; reuse probe passed.";
            }
            catch (OperationCanceledException)
            {
                WriteQualificationLog(
                    "event=BULK_LIFECYCLE_SUMMARY",
                    "verdict=ABORTED",
                    "configured=" + configureCount.ToString(
                        CultureInfo.InvariantCulture),
                    "active=" + activeCount.ToString(
                        CultureInfo.InvariantCulture),
                    "snapshots=" + snapshotCount.ToString(
                        CultureInfo.InvariantCulture),
                    "cleanup=" + cleanupCount.ToString(
                        CultureInfo.InvariantCulture),
                    "requested=" + input.Iterations.ToString(
                        CultureInfo.InvariantCulture),
                    "errorCount=" + errorCount.ToString(
                        CultureInfo.InvariantCulture));
                throw;
            }
            catch
            {
                errorCount++;
                WriteQualificationLog(
                    "event=BULK_LIFECYCLE_SUMMARY",
                    "verdict=FAIL",
                    "configured=" + configureCount.ToString(
                        CultureInfo.InvariantCulture),
                    "active=" + activeCount.ToString(
                        CultureInfo.InvariantCulture),
                    "snapshots=" + snapshotCount.ToString(
                        CultureInfo.InvariantCulture),
                    "cleanup=" + cleanupCount.ToString(
                        CultureInfo.InvariantCulture),
                    "requested=" + input.Iterations.ToString(
                        CultureInfo.InvariantCulture),
                    "errorCount=" + errorCount.ToString(
                        CultureInfo.InvariantCulture));
                throw;
            }
        }

        private QualificationBulkInput ReadQualificationBulkInput()
        {
            var iterations = ParseQualificationPositiveInt32(
                TextQualificationBulkIterations.Text,
                "Bulk iterations");
            var intervalMilliseconds = ParseQualificationNonNegativeInt32(
                TextQualificationBulkIntervalMs.Text,
                "Bulk interval milliseconds");
            if (iterations > 10000)
            {
                throw new InvalidOperationException(
                    "Bulk iterations must not exceed 10000.");
            }

            if (intervalMilliseconds > 60000)
            {
                throw new InvalidOperationException(
                    "Bulk interval milliseconds must not exceed 60000.");
            }

            return new QualificationBulkInput
            {
                Iterations = iterations,
                IntervalMilliseconds = intervalMilliseconds
            };
        }

        private async Task<QualificationBulkContext>
            PrepareQualificationBulkContextAsync(
                CancellationToken cancellationToken)
        {
            EnsureNoDiagnosticsResources(
                "Release the current Bulk and Recorder resources before qualification.");
            var diagnostics = RequireConnection().Diagnostics;

            var capabilities = await SendQualificationCommandAsync(
                "Bulk qualification capabilities",
                cancellationToken,
                () => diagnostics.GetCapabilitiesAsync(
                    CancellationToken.None));
            if (!capabilities.Response.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Diagnostics capability refresh did not succeed.");
            }

            if (!capabilities.Supports(LMCDiagnosticCapability.SignalCatalog)
                || !capabilities.Supports(LMCDiagnosticCapability.BulkSnapshot))
            {
                throw new NotSupportedException(
                    "SignalCatalog and BulkSnapshot capabilities are required.");
            }

            if (!capabilities.HasStableDiagnosticsBootId)
            {
                throw new InvalidOperationException(
                    "Bulk qualification requires a stable non-zero DiagnosticsBootId.");
            }

            if (capabilities.CatalogEntryCount
                    != QualificationBulkExpectedSignalCount
                || capabilities.MaxBulkSignals
                    < QualificationBulkExpectedSignalCount)
            {
                throw new InvalidOperationException(
                    "Bulk qualification requires exactly 24 Catalog entries and MaxBulkSignals >= 24.");
            }

            // GetSignalCatalogAsync is a bounded compound read. Passing the
            // scenario token after dispatch can abort the shared RPC
            // connection, so keep the app gate for the whole public call and
            // only honor cancellation before the delegate starts.
            var catalog = await SendQualificationCommandAsync(
                "Bulk qualification signal Catalog",
                cancellationToken,
                () => diagnostics.GetSignalCatalogAsync(
                    CancellationToken.None));
            if (catalog.MapRevision != capabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "Fresh Catalog MapRevision does not match capabilities.");
            }

            var bulkEntries = catalog.Entries
                .Where(entry => (entry.AccessFlags
                    & LMCSignalAccessFlags.BulkReadable)
                    == LMCSignalAccessFlags.BulkReadable)
                .ToList();
            if (bulkEntries.Count != QualificationBulkExpectedSignalCount)
            {
                throw new InvalidOperationException(
                    "Fresh Catalog must expose exactly 24 BulkReadable entries; actual="
                    + bulkEntries.Count.ToString(CultureInfo.InvariantCulture)
                    + ".");
            }

            for (var index = 0; index < bulkEntries.Count; index++)
            {
                var flags = bulkEntries[index].SignalFlags;
                if ((flags & LMCSignalFlags.InputMappedPhase) == 0
                    || (flags & LMCSignalFlags.PreOutputPhase) != 0)
                {
                    throw new InvalidOperationException(
                        "Bulk qualification requires every selected Catalog entry to use InputMapped phase.");
                }
            }

            WriteQualificationLog(
                "event=BULK_PREFLIGHT",
                "build=" + capabilities.DiagnosticsBuild.ToString(
                    CultureInfo.InvariantCulture),
                "bootId=0x" + capabilities.DiagnosticsBootId.ToString("X8"),
                "mapRevision=0x" + capabilities.MapRevision.ToString("X8"),
                "catalogEntries=" + catalog.Entries.Count.ToString(
                    CultureInfo.InvariantCulture),
                "bulkReadable=" + bulkEntries.Count.ToString(
                    CultureInfo.InvariantCulture),
                "maxBulkSignals=" + capabilities.MaxBulkSignals.ToString(
                    CultureInfo.InvariantCulture));

            return new QualificationBulkContext
            {
                Diagnostics = diagnostics,
                Capabilities = capabilities,
                Catalog = catalog,
                Entries = bulkEntries
            };
        }

        private async Task<LMCPIBulkReader>
            ConfigureQualificationBulkReaderAsync(
                QualificationBulkContext context,
                CancellationToken cancellationToken,
                string label)
        {
            var builder = context.Diagnostics.CreatePIBulkBuilder(
                context.Catalog);
            foreach (var entry in context.Entries)
            {
                builder.AddEntry(entry);
            }

            if (builder.Count != QualificationBulkExpectedSignalCount)
            {
                throw new InvalidOperationException(
                    "Bulk builder did not preserve the exact 24-entry Catalog order.");
            }

            var reader = await SendQualificationCommandAsync(
                "Bulk qualification Configure " + label,
                cancellationToken,
                () => builder.ConfigureAsync(CancellationToken.None));
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!builder.IsFrozen
                    || !reader.Configuration.ConfigurationResponse.IsSuccess
                    || reader.Configuration.DiagnosticsBootId
                        != context.Capabilities.DiagnosticsBootId
                    || reader.Configuration.MapRevision != context.Catalog.MapRevision
                    || reader.Configuration.SignalCount
                        != QualificationBulkExpectedSignalCount)
                {
                    throw new InvalidOperationException(
                        "Configured Bulk identity does not match the fresh capabilities and Catalog.");
                }

                for (var index = 0; index < context.Entries.Count; index++)
                {
                    if (reader.Configuration.SignalIds[index]
                            != context.Entries[index].SignalId
                        || reader.Entries[index].SignalId
                            != context.Entries[index].SignalId)
                    {
                        throw new InvalidOperationException(
                            "Configured Bulk signal order differs from Catalog order at index "
                            + index.ToString(CultureInfo.InvariantCulture)
                            + ".");
                    }
                }

                WriteQualificationLog(
                    "event=BULK_CONFIGURED",
                    "label=" + QualificationValue(label),
                    "bulkId=" + reader.Configuration.BulkId.ToString(
                        CultureInfo.InvariantCulture),
                    "configRevision="
                        + reader.Configuration.ConfigRevision.ToString(
                            CultureInfo.InvariantCulture),
                    "mapRevision=0x"
                        + reader.Configuration.MapRevision.ToString("X8"),
                    "initialState=" + reader.Configuration.InitialState,
                    "signalCount=" + reader.Configuration.SignalCount.ToString(
                        CultureInfo.InvariantCulture));
                return reader;
            }
            catch (Exception primaryError)
            {
                try
                {
                    await ReleaseQualificationBulkReaderAsync(
                        reader,
                        label + "-configure-validation");
                }
                catch (Exception cleanupError)
                {
                    throw CreateQualificationBulkCleanupException(
                        label + "-configure-validation",
                        primaryError,
                        cleanupError);
                }

                throw;
            }
        }

        private async Task<LMCBulkStatus> WaitForQualificationBulkActiveAsync(
            LMCPIBulkReader reader,
            CancellationToken cancellationToken,
            string label)
        {
            var timeout = Stopwatch.StartNew();
            var polls = 0;
            while (true)
            {
                var status = await SendQualificationCommandAsync(
                    "Bulk qualification status " + label,
                    cancellationToken,
                    () => reader.ReadStatusAsync(CancellationToken.None));
                polls++;
                if (!status.Response.IsSuccess
                    || status.BulkId != reader.Configuration.BulkId
                    || status.ConfigRevision
                        != reader.Configuration.ConfigRevision
                    || status.MapRevision != reader.Configuration.MapRevision
                    || status.SignalCount != reader.Configuration.SignalCount)
                {
                    throw new InvalidOperationException(
                        "Bulk status identity does not match its configuration.");
                }

                WriteQualificationLog(
                    "event=BULK_STATUS",
                    "label=" + QualificationValue(label),
                    "poll=" + polls.ToString(CultureInfo.InvariantCulture),
                    "state=" + status.State,
                    "activationCycle=" + status.ActivationCycle.ToString(
                        CultureInfo.InvariantCulture));
                if (status.IsActive)
                {
                    return status;
                }

                if (status.State == LMCBulkState.Empty
                    || status.State == LMCBulkState.Failed)
                {
                    throw new InvalidOperationException(
                        "Bulk entered terminal non-active state "
                        + status.State
                        + ".");
                }

                if (timeout.ElapsedMilliseconds
                    >= QualificationBulkActivationTimeoutMilliseconds)
                {
                    throw new TimeoutException(
                        "Bulk did not become Active within "
                        + QualificationBulkActivationTimeoutMilliseconds.ToString(
                            CultureInfo.InvariantCulture)
                        + " ms.");
                }

                await Task.Delay(
                    QualificationBulkStatusPollMilliseconds,
                    cancellationToken);
            }
        }

        private void ValidateQualificationBulkSnapshot(
            QualificationBulkContext context,
            LMCPIBulkReader reader,
            LMCBulkSnapshot snapshot,
            bool hasPreviousSnapshot,
            uint previousCycle,
            ulong previousTimestampUs,
            uint previousSequence)
        {
            if (snapshot == null || !snapshot.Response.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Bulk snapshot response did not succeed.");
            }

            if (snapshot.BulkId != reader.Configuration.BulkId
                || snapshot.ConfigRevision
                    != reader.Configuration.ConfigRevision
                || snapshot.MapRevision != reader.Configuration.MapRevision)
            {
                throw new InvalidOperationException(
                    "Bulk snapshot identity does not match its configuration.");
            }

            if (snapshot.EntryCount != QualificationBulkExpectedSignalCount
                || snapshot.Entries.Count
                    != QualificationBulkExpectedSignalCount
                || snapshot.EntryStride
                    != context.Capabilities.SignalValueEntryStride)
            {
                throw new InvalidOperationException(
                    "Bulk snapshot entry count or stride is not the expected 24-entry contract.");
            }

            var expectedFlags = LMCBulkSnapshotFlags.SameCycle
                | LMCBulkSnapshotFlags.InputMappedPhase;
            if (snapshot.CapturePhase != LMCCapturePhase.InputMapped
                || (snapshot.SnapshotFlags & expectedFlags) != expectedFlags
                || (snapshot.SnapshotSequence & 1u) != 0)
            {
                throw new InvalidOperationException(
                    "Bulk snapshot phase, flags, or even seqlock contract failed.");
            }

            if (snapshot.IsPartial
                || snapshot.Response.ResponseFlags
                    != LMCDiagnosticsResponseFlags.None)
            {
                throw new InvalidOperationException(
                    "Baseline Bulk snapshot must not be Partial.");
            }

            var invalidCount = 0;
            for (var index = 0; index < context.Entries.Count; index++)
            {
                var actual = snapshot.Entries[index];
                var expected = context.Entries[index];
                if (actual.SignalId != expected.SignalId
                    || actual.ValueType != expected.DataType)
                {
                    throw new InvalidOperationException(
                        "Bulk snapshot signal order or type mismatch at index "
                        + index.ToString(CultureInfo.InvariantCulture)
                        + ".");
                }

                if (!actual.IsValid || actual.DetailCode != 0)
                {
                    invalidCount++;
                }
            }

            if (invalidCount != 0)
            {
                throw new InvalidOperationException(
                    "Baseline Bulk snapshot contains "
                    + invalidCount.ToString(CultureInfo.InvariantCulture)
                    + " invalid entries.");
            }

            if (hasPreviousSnapshot)
            {
                EnsureQualificationBulkCounterNonDecreasing(
                    "CycleCounter",
                    previousCycle,
                    snapshot.CycleCounter);
                EnsureQualificationBulkCounterNonDecreasing(
                    "SnapshotSequence",
                    previousSequence,
                    snapshot.SnapshotSequence);
                EnsureQualificationBulkTimestampNonDecreasing(
                    previousTimestampUs,
                    snapshot.TimestampUs);
            }

        }

        private async Task<bool> ReleaseQualificationBulkReaderAsync(
            LMCPIBulkReader reader,
            string label)
        {
            if (reader == null || reader.IsReleased)
            {
                return false;
            }

            SetQualificationProgress(
                qualificationProgress,
                "Releasing Bulk resource " + label);
            try
            {
                await SendQualificationCleanupCommandAsync(
                    "Bulk qualification Release " + label,
                    () => reader.ReleaseAsync(CancellationToken.None));
                WriteQualificationLog(
                    "event=CLEANUP",
                    "resource=Bulk",
                    "label=" + QualificationValue(label),
                    "bulkId=" + reader.Configuration.BulkId.ToString(
                        CultureInfo.InvariantCulture),
                    "released=" + reader.IsReleased,
                    "verdict=PASS");
                return true;
            }
            catch (Exception error)
            {
                WriteQualificationLog(
                    "event=CLEANUP",
                    "resource=Bulk",
                    "label=" + QualificationValue(label),
                    "bulkId=" + reader.Configuration.BulkId.ToString(
                        CultureInfo.InvariantCulture),
                    "released=" + reader.IsReleased,
                    "verdict=FAIL",
                    "error=" + QualificationValue(error.Message));
                throw;
            }
        }

        private static InvalidOperationException
            CreateQualificationBulkCleanupException(
                string label,
                Exception primaryError,
                Exception cleanupError)
        {
            return new InvalidOperationException(
                "Bulk qualification failed and cleanup also failed for "
                + label
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

        private async Task VerifyQualificationBulkDoubleReleaseBlockedAsync(
            LMCPIBulkReader reader,
            CancellationToken cancellationToken)
        {
            if (reader == null || !reader.IsReleased)
            {
                throw new InvalidOperationException(
                    "Bulk double-release probe requires a released reader.");
            }

            try
            {
                await SendQualificationCommandAsync(
                    "Bulk qualification local double-release probe",
                    cancellationToken,
                    () => reader.ReleaseAsync(CancellationToken.None));
            }
            catch (LMCDiagnosticsCommandException error)
            {
                throw new InvalidOperationException(
                    "Bulk double-release probe reached the PLC instead of the local released-reader guard.",
                    error);
            }
            catch (InvalidOperationException error)
            {
                const string expectedMessage =
                    "The Bulk reader has already been released.";
                if (error.GetType() != typeof(InvalidOperationException)
                    || !string.Equals(
                        error.Message,
                        expectedMessage,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Bulk double-release probe returned an unexpected local exception: "
                        + error.GetType().Name
                        + ": "
                        + error.Message,
                        error);
                }

                WriteQualificationLog(
                    "event=BULK_DOUBLE_RELEASE",
                    "expected=local_InvalidOperationException",
                    "actual=" + error.GetType().Name,
                    "secondWireExpected=0",
                    "verdict=PASS");
                return;
            }

            throw new InvalidOperationException(
                "A second Bulk Release unexpectedly passed the local released-reader guard.");
        }

        private static void EnsureQualificationBulkCounterNonDecreasing(
            string fieldName,
            uint previous,
            uint current)
        {
            var forwardDelta = unchecked(current - previous);
            if (forwardDelta > int.MaxValue)
            {
                throw new InvalidOperationException(
                    fieldName
                    + " moved backward outside the wrap-aware UInt32 window: previous="
                    + previous.ToString(CultureInfo.InvariantCulture)
                    + ", current="
                    + current.ToString(CultureInfo.InvariantCulture)
                    + ".");
            }
        }

        private static void EnsureQualificationBulkTimestampNonDecreasing(
            ulong previous,
            ulong current)
        {
            var forwardDelta = unchecked(current - previous);
            if (forwardDelta > long.MaxValue)
            {
                throw new InvalidOperationException(
                    "TimestampUs moved backward outside the wrap-aware UInt64 window: previous="
                    + previous.ToString(CultureInfo.InvariantCulture)
                    + ", current="
                    + current.ToString(CultureInfo.InvariantCulture)
                    + ".");
            }
        }

        private sealed class QualificationBulkInput
        {
            public int Iterations { get; set; }
            public int IntervalMilliseconds { get; set; }
        }

        private sealed class QualificationBulkContext
        {
            public LMCDiagnostics Diagnostics { get; set; }
            public LMCDiagnosticCapabilities Capabilities { get; set; }
            public LMCSignalCatalog Catalog { get; set; }
            public IReadOnlyList<LMCSignalCatalogEntry> Entries { get; set; }
        }
    }
}

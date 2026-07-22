using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using LasalMotionControlLib;
using Microsoft.Win32;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly List<DiagnosticSignalRow> diagnosticSignalRows =
            new List<DiagnosticSignalRow>();

        private LMCDiagnosticCapabilities diagnosticCapabilities;
        private LMCSignalCatalog diagnosticCatalog;
        private LMCBulkConfiguration bulkConfiguration;
        private LMCRecorderConfigurationHandle recorderConfiguration;
        private LMCRecorderIdentity recorderIdentity;
        private LMCRecorderStatus recorderStatus;
        private LMCRecorderHeader recorderHeader;
        private LMCRecorderData recorderData;
        private CancellationTokenSource recorderDownloadCancellation;
        private LMCOperationTicket diagnosticOperationTicket;
        private LMCOperationStatus diagnosticOperationStatus;
        private byte[] diagnosticOperationResult;
        private bool updatingRecorderConfigurationOptions;

        private void InitializeDiagnosticsUi()
        {
            GridSignalCatalog.ItemsSource = diagnosticSignalRows;
            GridEtherCatHealth.ItemsSource = Array.Empty<HealthSlaveRow>();
            GridBulkSnapshot.ItemsSource = Array.Empty<BulkValueRow>();
            ComboRecorderPlotSignal.ItemsSource =
                Array.Empty<RecorderPlotSignalItem>();

            ComboRecorderBufferMode.ItemsSource = new[]
            {
                LMCRecorderBufferMode.Single
            };
            ComboRecorderBufferMode.SelectedItem =
                LMCRecorderBufferMode.Single;
            ComboRecorderTriggerType.ItemsSource = new[]
            {
                LMCRecorderTriggerType.Manual,
                LMCRecorderTriggerType.Edge,
                LMCRecorderTriggerType.Window,
                LMCRecorderTriggerType.Mask
            };
            ComboRecorderTriggerType.SelectedItem =
                LMCRecorderTriggerType.Manual;
            ComboRecorderTriggerSignal.ItemsSource =
                Array.Empty<DiagnosticSignalRow>();
            UpdateRecorderTriggerControls();

            ComboSdoOperation.ItemsSource = new[]
            {
                SdoOperationMode.Read
            };
            ComboSdoOperation.SelectedItem = SdoOperationMode.Read;
            ComboSdoValueType.ItemsSource = new[]
            {
                LMCSignalValueType.UInt32
            };
            ComboSdoValueType.SelectedItem = LMCSignalValueType.UInt32;
            ComboSdoDataLength.ItemsSource = new ushort[]
            {
                4
            };
            ComboSdoDataLength.SelectedItem = (ushort)4;
            UpdateSdoOperationControls();
            UpdateRecorderEstimate();
        }

        private async void ButtonDiagnosticsCapabilities_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh Diagnostics Capabilities",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    diagnosticCapabilities =
                        await currentConnection.Diagnostics.GetCapabilitiesAsync(
                            CancellationToken.None);

                    TextDiagnosticsCapabilities.Text =
                        FormatCapabilities(diagnosticCapabilities);
                    UpdateRecorderBufferModeOptions();
                    UpdateRecorderEstimate();
                });
        }

        private async void ButtonReadEtherCatHealth_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read EtherCAT Health",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.EtherCATHealth,
                        "EtherCAT Health");
                    var currentConnection = RequireConnection();
                    var health =
                        await currentConnection.Diagnostics.ReadEtherCATHealthAsync(
                            CancellationToken.None);

                    TextEtherCatHealthSummary.Text = FormatHealth(health);
                    GridEtherCatHealth.ItemsSource = health.Slaves
                        .Select(value => new HealthSlaveRow(value))
                        .ToArray();
                });
        }

        private async void ButtonLoadSignalCatalog_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Load Signal Catalog",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.SignalCatalog,
                        "Signal Catalog");
                    EnsureNoDiagnosticsResources(
                        "Release Bulk and Recorder resources before reloading the Catalog.");

                    var currentConnection = RequireConnection();
                    var catalog =
                        await currentConnection.Diagnostics.GetSignalCatalogAsync(
                            CancellationToken.None);

                    diagnosticCatalog = catalog;
                    diagnosticSignalRows.Clear();
                    foreach (var entry in catalog.Entries)
                    {
                        var row = new DiagnosticSignalRow(
                            entry,
                            entry.Alias.EndsWith(
                                ".actual_position",
                                StringComparison.Ordinal));
                        row.PropertyChanged +=
                            DiagnosticSignalRow_PropertyChanged;
                        diagnosticSignalRows.Add(row);
                    }

                    GridSignalCatalog.ItemsSource = null;
                    GridSignalCatalog.ItemsSource = diagnosticSignalRows;
                    PopulateRecorderTriggerSignals();
                    UpdateRecorderEstimate();
                    WriteLog(
                        "Catalog loaded. Revision=0x"
                        + catalog.MapRevision.ToString("X8")
                        + ", Entries="
                        + catalog.Entries.Count
                        + ", default selection=actual_position signals.");
                });
        }

        private async void ButtonReadSelectedPi_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Selected PI",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.PIRead,
                        "PI Read");
                    var catalog = RequireDiagnosticCatalog();
                    var selected = GetSelectedSignalRows(
                        LMCSignalAccessFlags.Readable,
                        "PI-readable",
                        0,
                        "PI Read");
                    var diagnostics = RequireConnection().Diagnostics;

                    foreach (var row in selected)
                    {
                        var value = await diagnostics.ReadPIAsync(
                            row.Entry.SignalId,
                            catalog.MapRevision,
                            row.Entry.DataType,
                            CancellationToken.None);
                        row.UpdateValue(value.Entry, value.CycleCounter);
                    }

                    WriteLog(
                        "PI read completed for "
                        + selected.Count
                        + " signal(s) at Catalog revision 0x"
                        + catalog.MapRevision.ToString("X8")
                        + ".");
                });
        }

        private async void ButtonConfigureBulk_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Configure Bulk Snapshot",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.BulkSnapshot,
                        "Bulk Snapshot");
                    RequireDiagnosticCatalog();
                    if (bulkConfiguration != null
                        && !bulkConfiguration.IsReleased)
                    {
                        throw new InvalidOperationException(
                            "Release the current Bulk configuration first.");
                    }

                    var selected = GetSelectedSignalRows(
                        LMCSignalAccessFlags.BulkReadable,
                        "Bulk-readable",
                        diagnosticCapabilities.MaxBulkSignals,
                        "Bulk Snapshot");
                    bulkConfiguration =
                        await RequireConnection().Diagnostics.ConfigureBulkAsync(
                            selected.Select(row => row.Entry.SignalId).ToArray(),
                            CancellationToken.None);

                    GridBulkSnapshot.ItemsSource = Array.Empty<BulkValueRow>();
                    TextBulkSummary.Text = FormatBulkConfiguration(
                        bulkConfiguration);
                });
        }

        private async void ButtonReadBulkStatus_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Bulk Status",
                async () =>
                {
                    var configuration = RequireBulkConfiguration();
                    var status =
                        await RequireConnection().Diagnostics.ReadBulkStatusAsync(
                            configuration,
                            CancellationToken.None);
                    TextBulkSummary.Text = FormatBulkStatus(status);
                });
        }

        private async void ButtonReadBulkSnapshot_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Bulk Snapshot",
                async () =>
                {
                    var configuration = RequireBulkConfiguration();
                    var snapshot =
                        await RequireConnection().Diagnostics.ReadBulkAsync(
                            configuration,
                            CancellationToken.None);

                    var rows = new List<BulkValueRow>(snapshot.Entries.Count);
                    for (var index = 0; index < snapshot.Entries.Count; index++)
                    {
                        var entry = snapshot.Entries[index];
                        var catalogRow = FindSignalRow(entry.SignalId);
                        if (catalogRow != null)
                        {
                            catalogRow.UpdateValue(entry, snapshot.CycleCounter);
                        }

                        rows.Add(new BulkValueRow(catalogRow, entry));
                    }

                    GridBulkSnapshot.ItemsSource = rows;
                    TextBulkSummary.Text = FormatBulkSnapshot(snapshot);
                });
        }

        private async void ButtonReleaseBulk_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Release Bulk Snapshot",
                async () =>
                {
                    var configuration = RequireBulkConfiguration();
                    await RequireConnection().Diagnostics.ReleaseBulkAsync(
                        configuration,
                        CancellationToken.None);
                    bulkConfiguration = null;
                    GridBulkSnapshot.ItemsSource = Array.Empty<BulkValueRow>();
                    TextBulkSummary.Text = "Bulk configuration released.";
                });
        }

        private async void ButtonConfigureRecorder_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Configure Recorder",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.RecorderSingleBank,
                        "Recorder Single Bank");
                    RequireDiagnosticCatalog();
                    if (recorderConfiguration != null
                        && !recorderConfiguration.IsReleased)
                    {
                        throw new InvalidOperationException(
                            "Release the current Recorder configuration first.");
                    }

                    var selected = GetSelectedSignalRows(
                        LMCSignalAccessFlags.Recordable,
                        "Recordable",
                        diagnosticCapabilities.MaxRecorderChannels,
                        "Recorder");
                    var configuration = BuildRecorderConfiguration(selected);

                    if (configuration.RequiresTriggerCapability)
                    {
                        EnsureCapability(
                            LMCDiagnosticCapability.RecorderTrigger,
                            "Recorder Trigger");
                    }

                    if (configuration.RequiresDoubleBankCapability)
                    {
                        EnsureCapability(
                            LMCDiagnosticCapability.RecorderDoubleBank,
                            "Recorder Double Bank");
                    }

                    recorderConfiguration =
                        await RequireConnection().Diagnostics.ConfigureRecorderAsync(
                            configuration,
                            CancellationToken.None);
                    recorderIdentity = null;
                    recorderStatus = null;
                    ClearRecorderDownload();
                    TextRecorderSummary.Text = FormatRecorderConfiguration(
                        recorderConfiguration);
                });
        }

        private async void ButtonStartRecorder_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Start Recorder",
                async () =>
                {
                    var configuration = RequireRecorderConfiguration();
                    recorderIdentity =
                        await RequireConnection().Diagnostics.StartRecorderAsync(
                            configuration,
                            CancellationToken.None);
                    recorderStatus = null;
                    ClearRecorderDownload();
                    UpdateRecorderAdoptionFields(recorderIdentity);
                    TextRecorderSummary.Text = FormatRecorderIdentity(
                        recorderIdentity);
                });
        }

        private async void ButtonAdoptRecorder_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Adopt Recorder",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.RecorderSingleBank,
                        "Recorder");
                    if ((recorderConfiguration != null
                            && !recorderConfiguration.IsReleased)
                        || (recorderIdentity != null
                            && !recorderIdentity.IsRecorderReleased))
                    {
                        throw new InvalidOperationException(
                            "Release the current Recorder resource before adopting another record.");
                    }

                    var diagnosticsBootId = ParseNonZeroUInt32Wire(
                        TextRecorderAdoptBootId.Text,
                        "Recorder DiagnosticsBootId");
                    var recordId = ParseUInt32Wire(
                        TextRecorderAdoptRecordId.Text,
                        "Recorder RecordId");
                    var bufferId = ParseUInt32Wire(
                        TextRecorderAdoptBufferId.Text,
                        "Recorder BufferId");

                    recorderConfiguration = null;
                    var diagnostics = RequireConnection().Diagnostics;
                    if (recordId == 0 && bufferId == 0)
                    {
                        recorderIdentity =
                            await diagnostics.AdoptActiveRecorderAsync(
                                diagnosticsBootId,
                                CancellationToken.None);
                    }
                    else
                    {
                        if (recordId == 0)
                        {
                            throw new InvalidOperationException(
                                "Recorder RecordId must be nonzero for exact adoption. "
                                + "Use RecordId=0 and BufferId=0 together to discover "
                                + "the current single-bank Recorder.");
                        }

                        recorderIdentity =
                            await diagnostics.AdoptRecorderAsync(
                                diagnosticsBootId,
                                recordId,
                                bufferId,
                                CancellationToken.None);
                    }
                    recorderStatus = null;
                    ClearRecorderDownload();
                    UpdateRecorderAdoptionFields(recorderIdentity);
                    TextRecorderSummary.Text = FormatRecorderIdentity(
                        recorderIdentity)
                        + Environment.NewLine
                        + "Recorder adopted. Read Status for authoritative terminal metadata, "
                        + "or Header before download; Release recovers Status metadata when needed.";
                });
        }

        private async void ButtonStopRecorder_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Stop Recorder",
                async () =>
                {
                    var identity = RequireRecorderIdentity();
                    var diagnostics = RequireConnection().Diagnostics;
                    recorderStatus =
                        await diagnostics.GetRecorderStatusAsync(
                            identity,
                            CancellationToken.None);
                    if (recorderStatus.State != LMCRecorderState.Armed
                        && recorderStatus.State != LMCRecorderState.Recording)
                    {
                        TextRecorderSummary.Text =
                            "Stop not sent: Recorder State="
                            + recorderStatus.State
                            + " is already frozen or terminal. Stop is valid only "
                            + "in Armed or Recording."
                            + Environment.NewLine
                            + FormatRecorderStatus(recorderStatus);
                        return;
                    }

                    try
                    {
                        await diagnostics.StopRecorderAsync(
                            identity,
                            CancellationToken.None);
                    }
                    catch (LMCDiagnosticsCommandException exception)
                        when (exception.Response != null
                            && exception.Response.Detail
                                == LMCDiagnosticsDetailCode.InvalidState)
                    {
                        recorderStatus =
                            await diagnostics.GetRecorderStatusAsync(
                                identity,
                                CancellationToken.None);
                        if (!recorderStatus.IsFrozen)
                        {
                            throw;
                        }

                        TextRecorderSummary.Text =
                            "Stop no longer required: Recorder reached State="
                            + recorderStatus.State
                            + " before the Stop request was accepted."
                            + Environment.NewLine
                            + FormatRecorderStatus(recorderStatus);
                        return;
                    }

                    recorderStatus = null;
                    TextRecorderSummary.Text =
                        "Stop request sequence published. Refresh Status until State=Ready; "
                        + "the final StopReason and TriggerIndex are authoritative.";
                });
        }

        private async void ButtonTriggerRecorder_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Trigger Recorder",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.RecorderTrigger,
                        "Recorder Trigger");
                    var configuration = RequireRecorderConfiguration();
                    if (configuration.Configuration.TriggerType
                        == LMCRecorderTriggerType.Manual)
                    {
                        throw new InvalidOperationException(
                            "Trigger Now requires an Edge, Window, or Mask Recorder configuration.");
                    }

                    var identity = RequireRecorderIdentity();
                    await RequireConnection().Diagnostics.TriggerRecorderAsync(
                        identity,
                        CancellationToken.None);
                    recorderStatus = null;
                    TextRecorderSummary.Text =
                        "Trigger request sequence published for RecordId="
                        + identity.RecordId
                        + ". Refresh Status until State=Ready; the final StopReason "
                        + "and TriggerIndex are authoritative.";
                });
        }

        private async void ButtonRecorderStatus_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Recorder Status",
                async () =>
                {
                    var identity = RequireRecorderIdentity();
                    recorderStatus =
                        await RequireConnection().Diagnostics.GetRecorderStatusAsync(
                            identity,
                            CancellationToken.None);
                    TextRecorderSummary.Text = FormatRecorderStatus(
                        recorderStatus);
                });
        }

        private async void ButtonReadRecorderHeader_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Recorder Header",
                async () =>
                {
                    var identity = RequireRecorderIdentity();
                    recorderHeader =
                        await RequireConnection().Diagnostics.GetRecorderHeaderAsync(
                            identity,
                            CancellationToken.None);
                    TextRecorderSummary.Text = FormatRecorderHeader(
                        recorderHeader);
                });
        }

        private async void ButtonDownloadRecorder_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Download Recorder",
                async () =>
                {
                    var identity = RequireRecorderIdentity();
                    if ((recorderStatus == null || !recorderStatus.IsFrozen)
                        && recorderHeader == null)
                    {
                        throw new InvalidOperationException(
                            "Refresh Recorder status and wait for State=Ready before downloading.");
                    }

                    var cancellation = new CancellationTokenSource();
                    recorderDownloadCancellation = cancellation;
                    ProgressRecorderDownload.Value = 0;
                    UpdateUiState();

                    try
                    {
                        var progress =
                            new Progress<LMCRecorderDownloadProgress>(
                                value =>
                                {
                                    ProgressRecorderDownload.Value = Math.Max(
                                        0,
                                        Math.Min(1, value.Fraction));
                                    TextRecorderSummary.Text =
                                        "Downloading "
                                        + value.DownloadedSamples
                                        + "/"
                                        + value.TotalSamples
                                        + " samples, "
                                        + value.DownloadedBytes
                                        + "/"
                                        + value.TotalBytes
                                        + " bytes, chunks="
                                        + value.CompletedChunks;
                                });

                        recorderData =
                            await RequireConnection().Diagnostics.DownloadRecorderAsync(
                                identity,
                                progress,
                                cancellation.Token);
                        recorderHeader = recorderData.Header;
                        ProgressRecorderDownload.Value = 1;
                        TextRecorderSummary.Text = FormatRecorderData(recorderData);
                        PopulateRecorderPlotSignals();
                    }
                    finally
                    {
                        if (ReferenceEquals(
                            recorderDownloadCancellation,
                            cancellation))
                        {
                            recorderDownloadCancellation = null;
                        }

                        cancellation.Dispose();
                        UpdateUiState();
                    }
                });
        }

        private void ButtonCancelRecorderDownload_Click(
            object sender,
            RoutedEventArgs e)
        {
            var cancellation = recorderDownloadCancellation;
            if (cancellation == null)
            {
                return;
            }

            cancellation.Cancel();
            TextOperationState.Text = "Recorder download cancellation requested";
            WriteLog("Recorder download cancellation requested.");
        }

        private async void ButtonExportRecorderCsv_Click(
            object sender,
            RoutedEventArgs e)
        {
            var data = recorderData;
            if (data == null)
            {
                WriteLog("Export Recorder CSV requires downloaded data.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = "lasal-recorder-"
                    + DateTime.Now.ToString(
                        "yyyyMMdd-HHmmss",
                        CultureInfo.InvariantCulture)
                    + ".csv",
                OverwritePrompt = true,
                Title = "Export LASAL Recorder CSV"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            var channels = BuildRecorderChannelItems(data.Header.SignalIds);
            await RunOperationAsync(
                "Export Recorder CSV",
                async () =>
                {
                    await Task.Run(
                        () => WriteRecorderCsv(
                            dialog.FileName,
                            data,
                            channels));
                    TextRecorderSummary.Text =
                        "CSV export complete. File=" + dialog.FileName;
                });
        }

        private async void ButtonReleaseRecorder_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Release Recorder",
                async () =>
                {
                    var diagnostics = RequireConnection().Diagnostics;
                    var identity = recorderIdentity;
                    if (identity != null
                        && !identity.IsRecorderReleased
                        && !identity.HasConfigurationMetadata)
                    {
                        recorderStatus = await diagnostics.GetRecorderStatusAsync(
                            identity,
                            CancellationToken.None);
                    }

                    if (identity != null
                        && !identity.IsBufferReleased)
                    {
                        await diagnostics.ReleaseRecorderBufferAsync(
                            identity,
                            CancellationToken.None);
                    }

                    if (recorderConfiguration != null
                        && !recorderConfiguration.IsReleased)
                    {
                        await diagnostics.ReleaseRecorderAsync(
                            recorderConfiguration,
                            CancellationToken.None);
                    }
                    else if (identity != null
                        && !identity.IsRecorderReleased)
                    {
                        await diagnostics.ReleaseRecorderAsync(
                            identity,
                            CancellationToken.None);
                    }

                    recorderIdentity = null;
                    recorderConfiguration = null;
                    recorderStatus = null;
                    TextRecorderSummary.Text = recorderData == null
                        ? "Recorder resources released."
                        : "Recorder PLC resources released. Downloaded PC data remains available for plot and CSV export.";
                });
        }

        private void ComboRecorderPlotSignal_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            RenderRecorderPlot();
        }

        private void CanvasRecorderPlot_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            RenderRecorderPlot();
        }

        private void RecorderConfiguration_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (updatingRecorderConfigurationOptions)
            {
                return;
            }

            NormalizeRecorderConfigurationSelection();
            UpdateRecorderTriggerControls();
            UpdateRecorderEstimate();
            if (ButtonConnect != null)
            {
                UpdateUiState();
            }
        }

        private void RecorderConfiguration_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            UpdateRecorderEstimate();
        }

        private void DiagnosticSignalRow_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            if (string.Equals(
                e.PropertyName,
                "IsSelected",
                StringComparison.Ordinal))
            {
                UpdateRecorderEstimate();
            }
        }

        private void SdoOperation_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateSdoOperationControls();
            if (ButtonConnect != null)
            {
                UpdateUiState();
            }
        }

        private async void ButtonSubmitSdo_Click(
            object sender,
            RoutedEventArgs e)
        {
            LMCSdoRequest request;
            string validationMessage;
            if (!TryCreateFirstSliceSdoReadRequest(
                out request,
                out validationMessage))
            {
                TextDiagnosticOperationSummary.Text =
                    "Not submitted: " + validationMessage;
                TextOperationState.Text = "Submit SDO validation failed";
                WriteLog("Submit SDO not submitted: " + validationMessage);
                return;
            }

            await RunOperationAsync(
                "Submit SDO Read",
                async () =>
                {
                    diagnosticOperationTicket =
                        await RequireConnection().Diagnostics.SubmitSdoAsync(
                            request,
                            CancellationToken.None);
                    diagnosticOperationStatus = null;
                    diagnosticOperationResult = null;
                    TextDiagnosticOperationSummary.Text = FormatOperationTicket(
                        diagnosticOperationTicket);
                });
        }

        private async void ButtonRefreshDiagnosticOperation_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh Diagnostics Operation",
                async () =>
                {
                    var ticket = RequireDiagnosticOperationTicket();
                    diagnosticOperationStatus =
                        await RequireConnection().Diagnostics.GetOperationStatusAsync(
                            ticket,
                            CancellationToken.None);
                    if (diagnosticOperationStatus.IsSuccessful
                        && ticket.OperationKind == LMCOperationKind.SDORead)
                    {
                        if (!ticket.UsesExtendedResultChunks)
                        {
                            diagnosticOperationResult =
                                diagnosticOperationStatus.ResultData;
                        }
                    }
                    else
                    {
                        diagnosticOperationResult = null;
                    }
                    TextDiagnosticOperationSummary.Text = FormatOperationStatus(
                        diagnosticOperationStatus)
                        + (diagnosticOperationStatus.IsSuccessful
                            && ticket.UsesExtendedResultChunks
                            ? Environment.NewLine
                                + (diagnosticOperationResult == null
                                    ? "Extended result is ready. Click Download Result to read validated 0x7E51 chunks."
                                    : "Extended result remains downloaded: "
                                        + diagnosticOperationResult.Length
                                        + " bytes, preview="
                                        + FormatBytePreview(
                                            diagnosticOperationResult,
                                            128))
                            : string.Empty);
                });
        }

        private async void ButtonDownloadSdoResult_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Download SDO Result",
                async () =>
                {
                    var ticket = RequireDiagnosticOperationTicket();
                    if (!ticket.UsesExtendedResultChunks)
                    {
                        throw new InvalidOperationException(
                            "The current SDO result is inline; Refresh Ticket already retrieved it.");
                    }

                    if (diagnosticOperationStatus == null
                        || !diagnosticOperationStatus.IsSuccessful)
                    {
                        throw new InvalidOperationException(
                            "Refresh Ticket and wait for a successful terminal SDO Read before downloading result chunks.");
                    }

                    if (diagnosticCapabilities == null
                        || diagnosticCapabilities.MaxChunkDataBytes == 0)
                    {
                        throw new InvalidOperationException(
                            "Refresh diagnostics capabilities before downloading result chunks.");
                    }

                    var expectedLength = ticket.RequestedResultLength;
                    var result = new byte[expectedLength];
                    uint offset = 0;
                    uint sequence = 1;
                    var completedChunks = 0;
                    var diagnostics = RequireConnection().Diagnostics;
                    while (offset < expectedLength)
                    {
                        var requestedByteCount = checked((ushort)Math.Min(
                            (uint)diagnosticCapabilities.MaxChunkDataBytes,
                            expectedLength - offset));
                        var request = new LMCSdoResultChunkRequest(
                            ticket,
                            offset,
                            requestedByteCount,
                            sequence);
                        var chunk = await diagnostics.ReadSdoResultChunkAsync(
                            request,
                            CancellationToken.None);
                        if (chunk.ReturnedByteCount == 0)
                        {
                            throw new InvalidDataException(
                                "PLC returned an empty SDO result chunk before completion.");
                        }

                        var bytes = chunk.Data;
                        Buffer.BlockCopy(
                            bytes,
                            0,
                            result,
                            checked((int)offset),
                            bytes.Length);
                        offset += chunk.ReturnedByteCount;
                        completedChunks++;
                        sequence = unchecked(sequence + 1);
                        if (sequence == 0)
                        {
                            sequence = 1;
                        }
                    }

                    diagnosticOperationResult = result;
                    TextDiagnosticOperationSummary.Text =
                        FormatOperationStatus(diagnosticOperationStatus)
                        + Environment.NewLine
                        + "Extended result downloaded: "
                        + result.Length
                        + " bytes in "
                        + completedChunks
                        + " chunk(s), preview="
                        + FormatBytePreview(result, 128);
                });
        }

        private async void ButtonExportSdoResult_Click(
            object sender,
            RoutedEventArgs e)
        {
            var result = diagnosticOperationResult;
            if (result == null)
            {
                WriteLog(
                    "Save SDO Result requires a successful inline or downloaded extended result.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".bin",
                Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*",
                FileName = "lasal-sdo-result-"
                    + DateTime.Now.ToString(
                        "yyyyMMdd-HHmmss",
                        CultureInfo.InvariantCulture)
                    + ".bin",
                OverwritePrompt = true,
                Title = "Save LASAL SDO Result"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            await RunOperationAsync(
                "Save SDO Result",
                () => Task.Run(
                    () => File.WriteAllBytes(dialog.FileName, result)));
        }

        private async void ButtonCancelDiagnosticOperation_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Cancel Diagnostics Operation",
                async () =>
                {
                    var ticket = RequireDiagnosticOperationTicket();
                    if (diagnosticOperationStatus != null
                        && diagnosticOperationStatus.IsTerminal)
                    {
                        throw new InvalidOperationException(
                            "The current diagnostics operation is already terminal.");
                    }

                    await RequireConnection().Diagnostics.CancelOperationAsync(
                        ticket,
                        CancellationToken.None);
                    TextDiagnosticOperationSummary.Text =
                        "Cancel accepted for TicketId="
                        + ticket.TicketId
                        + ". Refresh the ticket to read its terminal state.";
                });
        }

        private async void ButtonSubmitPiWrite_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Submit PI Write",
                async () =>
                {
                    EnsureCapability(
                        LMCDiagnosticCapability.PIWrite,
                        "PI Write");
                    var catalog = RequireDiagnosticCatalog();
                    var selected = GetSelectedSignalRows(
                        LMCSignalAccessFlags.WritableByPolicy,
                        "WritableByPolicy",
                        0,
                        "PI Write");
                    if (selected.Count != 1)
                    {
                        throw new InvalidOperationException(
                            "PI Write requires exactly one checked Catalog signal.");
                    }

                    var signal = selected[0].Entry;
                    var request = new LMCPIWriteRequest(
                        catalog,
                        signal,
                        signal.DataType,
                        ParseUInt32Wire(
                            TextPiWriteRawValue.Text,
                            "PI Write raw value"));

                    diagnosticOperationTicket =
                        await RequireConnection().Diagnostics.SubmitPIWriteAsync(
                            request,
                            CancellationToken.None);
                    diagnosticOperationStatus = null;
                    diagnosticOperationResult = null;
                    TextDiagnosticOperationSummary.Text = FormatOperationTicket(
                        diagnosticOperationTicket);
                });
        }

        private void UpdateDiagnosticsUiState(bool connected, bool idle)
        {
            if (ButtonDiagnosticsCapabilities == null)
            {
                return;
            }

            var supportsHealth = SupportsCapability(
                LMCDiagnosticCapability.EtherCATHealth);
            var supportsCatalog = SupportsCapability(
                LMCDiagnosticCapability.SignalCatalog);
            var supportsPi = SupportsCapability(
                LMCDiagnosticCapability.PIRead);
            var supportsBulk = SupportsCapability(
                LMCDiagnosticCapability.BulkSnapshot);
            var supportsRecorder = SupportsCapability(
                LMCDiagnosticCapability.RecorderSingleBank);
            var supportsRecorderTrigger = SupportsCapability(
                LMCDiagnosticCapability.RecorderTrigger);
            var supportsRecorderDouble = SupportsCapability(
                LMCDiagnosticCapability.RecorderDoubleBank);
            var supportsPiWrite = SupportsCapability(
                LMCDiagnosticCapability.PIWrite);
            var supportsSdoRead = SupportsCapability(
                LMCDiagnosticCapability.SDORead);
            var hasCatalog = diagnosticCatalog != null;
            var hasBulk = bulkConfiguration != null
                && !bulkConfiguration.IsReleased;
            var hasRecorderConfiguration = recorderConfiguration != null
                && !recorderConfiguration.IsReleased;
            var hasRecorderIdentity = recorderIdentity != null
                && !recorderIdentity.IsRecorderReleased;
            var downloadRunning = recorderDownloadCancellation != null;
            var recorderCanStop = recorderStatus == null
                || recorderStatus.State == LMCRecorderState.Armed
                || recorderStatus.State == LMCRecorderState.Recording;
            var selectedRecorderBufferMode =
                ComboRecorderBufferMode.SelectedItem is LMCRecorderBufferMode
                    ? (LMCRecorderBufferMode)ComboRecorderBufferMode.SelectedItem
                    : LMCRecorderBufferMode.Single;
            var selectedRecorderTriggerType =
                ComboRecorderTriggerType.SelectedItem is LMCRecorderTriggerType
                    ? (LMCRecorderTriggerType)ComboRecorderTriggerType.SelectedItem
                    : LMCRecorderTriggerType.Manual;
            var recorderOptionSupported =
                (selectedRecorderTriggerType == LMCRecorderTriggerType.Manual
                    || supportsRecorderTrigger)
                && (selectedRecorderBufferMode != LMCRecorderBufferMode.Double
                    || supportsRecorderDouble)
                && (selectedRecorderBufferMode != LMCRecorderBufferMode.Ring
                    || selectedRecorderTriggerType
                        != LMCRecorderTriggerType.Manual);

            ButtonDiagnosticsCapabilities.IsEnabled = connected && idle;
            ButtonReadEtherCatHealth.IsEnabled = connected
                && idle
                && supportsHealth;
            ButtonLoadSignalCatalog.IsEnabled = connected
                && idle
                && supportsCatalog
                && !hasBulk
                && !hasRecorderConfiguration
                && !hasRecorderIdentity;
            ButtonReadSelectedPi.IsEnabled = connected
                && idle
                && supportsPi
                && hasCatalog;

            ButtonConfigureBulk.IsEnabled = connected
                && idle
                && supportsBulk
                && hasCatalog
                && !hasBulk;
            ButtonReadBulkStatus.IsEnabled = connected && idle && hasBulk;
            ButtonReadBulkSnapshot.IsEnabled = connected && idle && hasBulk;
            ButtonReleaseBulk.IsEnabled = connected && idle && hasBulk;

            var recorderInputsEnabled = idle
                && !hasRecorderConfiguration
                && !hasRecorderIdentity;
            var recorderTriggerInputsEnabled = recorderInputsEnabled
                && selectedRecorderTriggerType != LMCRecorderTriggerType.Manual;
            TextRecorderSamplePeriod.IsEnabled = recorderInputsEnabled;
            TextRecorderSampleCapacity.IsEnabled = recorderInputsEnabled;
            ComboRecorderBufferMode.IsEnabled = recorderInputsEnabled;
            ComboRecorderTriggerType.IsEnabled = recorderInputsEnabled;
            ComboRecorderTriggerOperator.IsEnabled =
                recorderTriggerInputsEnabled;
            ComboRecorderTriggerSignal.IsEnabled = recorderTriggerInputsEnabled;
            TextRecorderPreTrigger.IsEnabled = recorderTriggerInputsEnabled;
            TextRecorderPostTrigger.IsEnabled = recorderTriggerInputsEnabled;
            TextRecorderTriggerValue.IsEnabled = recorderTriggerInputsEnabled
                && selectedRecorderTriggerType != LMCRecorderTriggerType.Mask;
            TextRecorderTriggerMask.IsEnabled = recorderTriggerInputsEnabled
                && (selectedRecorderTriggerType
                        == LMCRecorderTriggerType.Window
                    || selectedRecorderTriggerType
                        == LMCRecorderTriggerType.Mask);
            ButtonConfigureRecorder.IsEnabled = connected
                && idle
                && supportsRecorder
                && hasCatalog
                && !hasRecorderConfiguration
                && !hasRecorderIdentity
                && recorderOptionSupported;
            var recorderAdoptionInputsEnabled = idle
                && !hasRecorderConfiguration
                && !hasRecorderIdentity;
            TextRecorderAdoptBootId.IsEnabled = recorderAdoptionInputsEnabled;
            TextRecorderAdoptRecordId.IsEnabled = recorderAdoptionInputsEnabled;
            TextRecorderAdoptBufferId.IsEnabled = recorderAdoptionInputsEnabled;
            ButtonAdoptRecorder.IsEnabled = connected
                && idle
                && supportsRecorder
                && !hasRecorderConfiguration
                && !hasRecorderIdentity;
            ButtonStartRecorder.IsEnabled = connected
                && idle
                && hasRecorderConfiguration
                && !hasRecorderIdentity;
            ButtonStopRecorder.IsEnabled = connected
                && idle
                && hasRecorderIdentity
                && !recorderIdentity.IsBufferReleased
                && recorderCanStop;
            ButtonTriggerRecorder.IsEnabled = connected
                && idle
                && supportsRecorderTrigger
                && hasRecorderIdentity
                && hasRecorderConfiguration
                && recorderConfiguration.Configuration.TriggerType
                    != LMCRecorderTriggerType.Manual
                && !recorderIdentity.IsBufferReleased;
            ButtonRecorderStatus.IsEnabled = connected
                && idle
                && hasRecorderIdentity
                && !recorderIdentity.IsBufferReleased;
            ButtonReadRecorderHeader.IsEnabled = connected
                && idle
                && hasRecorderIdentity
                && !recorderIdentity.IsBufferReleased;
            ButtonDownloadRecorder.IsEnabled = connected
                && idle
                && hasRecorderIdentity
                && ((recorderStatus != null && recorderStatus.IsFrozen)
                    || recorderHeader != null)
                && !downloadRunning;
            ButtonCancelRecorderDownload.IsEnabled = downloadRunning;
            ButtonExportRecorderCsv.IsEnabled = idle
                && recorderData != null;
            ButtonReleaseRecorder.IsEnabled = connected
                && idle
                && (hasRecorderIdentity || hasRecorderConfiguration);

            var operationIsTerminal = diagnosticOperationStatus != null
                && diagnosticOperationStatus.IsTerminal;
            var canSubmitOperation = diagnosticOperationTicket == null
                || operationIsTerminal;
            var sdoInputsEnabled = idle && canSubmitOperation;
            ComboSdoOperation.IsEnabled = false;
            TextSdoSlaveReference.IsEnabled = sdoInputsEnabled;
            TextSdoIndex.IsEnabled = false;
            TextSdoSubIndex.IsEnabled = false;
            ComboSdoValueType.IsEnabled = false;
            ComboSdoDataLength.IsEnabled = false;
            TextSdoTimeoutCycles.IsEnabled = sdoInputsEnabled;
            TextSdoWriteData.IsEnabled = false;
            ButtonSubmitSdo.IsEnabled = connected
                && idle
                && canSubmitOperation
                && supportsSdoRead;
            ButtonRefreshDiagnosticOperation.IsEnabled = connected
                && idle
                && diagnosticOperationTicket != null;
            ButtonCancelDiagnosticOperation.IsEnabled = connected
                && idle
                && diagnosticOperationTicket != null
                && !operationIsTerminal;
            ButtonDownloadSdoResult.IsEnabled = connected
                && idle
                && diagnosticOperationTicket != null
                && diagnosticOperationTicket.UsesExtendedResultChunks
                && diagnosticOperationStatus != null
                && diagnosticOperationStatus.IsSuccessful
                && diagnosticOperationResult == null;
            ButtonExportSdoResult.IsEnabled = idle
                && diagnosticOperationResult != null;
            TextPiWriteRawValue.IsEnabled = idle;
            ButtonSubmitPiWrite.IsEnabled = connected
                && idle
                && supportsPiWrite
                && hasCatalog
                && canSubmitOperation;
        }

        private void ClearDiagnosticsState()
        {
            var cancellation = recorderDownloadCancellation;
            recorderDownloadCancellation = null;
            cancellation?.Cancel();
            cancellation?.Dispose();

            diagnosticCapabilities = null;
            diagnosticCatalog = null;
            diagnosticSignalRows.Clear();
            bulkConfiguration = null;
            recorderConfiguration = null;
            recorderIdentity = null;
            recorderStatus = null;
            recorderHeader = null;
            recorderData = null;
            diagnosticOperationTicket = null;
            diagnosticOperationStatus = null;
            diagnosticOperationResult = null;
            UpdateRecorderBufferModeOptions();

            if (GridSignalCatalog != null)
            {
                GridSignalCatalog.ItemsSource = null;
                GridSignalCatalog.ItemsSource = diagnosticSignalRows;
                GridEtherCatHealth.ItemsSource = Array.Empty<HealthSlaveRow>();
                GridBulkSnapshot.ItemsSource = Array.Empty<BulkValueRow>();
                ComboRecorderPlotSignal.ItemsSource =
                    Array.Empty<RecorderPlotSignalItem>();
                ComboRecorderTriggerSignal.ItemsSource =
                    Array.Empty<DiagnosticSignalRow>();
                CanvasRecorderPlot.Children.Clear();
                ProgressRecorderDownload.Value = 0;
                TextDiagnosticsCapabilities.Text =
                    "Connect, then refresh diagnostics capabilities.";
                TextEtherCatHealthSummary.Text = "Health has not been read.";
                TextBulkSummary.Text =
                    "Load the PI Catalog and check Bulk-readable signals first.";
                TextRecorderSummary.Text =
                    "Load the PI Catalog and check Recordable signals first.";
                TextRecorderPlotRange.Text = "No downloaded data.";
                TextDiagnosticOperationSummary.Text =
                    "First-slice SDO Read only: slave 1..4, object 0x1000:0, UInt32, 4 bytes, timeout 1..60000 cycles. Submit stays disabled until the PLC advertises SDORead; Refresh Ticket retrieves the inline result. SDO Write and extended-result download are unavailable.";
            }
        }

        private void ClearRecorderDownload()
        {
            recorderHeader = null;
            recorderData = null;
            ProgressRecorderDownload.Value = 0;
            ComboRecorderPlotSignal.ItemsSource =
                Array.Empty<RecorderPlotSignalItem>();
            CanvasRecorderPlot.Children.Clear();
            TextRecorderPlotRange.Text = "No downloaded data.";
        }

        private bool SupportsCapability(LMCDiagnosticCapability capability)
        {
            return diagnosticCapabilities != null
                && diagnosticCapabilities.Supports(capability);
        }

        private void EnsureCapability(
            LMCDiagnosticCapability capability,
            string displayName)
        {
            if (diagnosticCapabilities == null)
            {
                throw new InvalidOperationException(
                    "Refresh diagnostics capabilities first.");
            }

            if (!diagnosticCapabilities.Supports(capability))
            {
                throw new NotSupportedException(
                    displayName + " is not advertised by the connected PLC.");
            }
        }

        private void EnsureNoDiagnosticsResources(string message)
        {
            if ((bulkConfiguration != null && !bulkConfiguration.IsReleased)
                || (recorderConfiguration != null
                    && !recorderConfiguration.IsReleased)
                || (recorderIdentity != null
                    && !recorderIdentity.IsRecorderReleased))
            {
                throw new InvalidOperationException(message);
            }
        }

        private LMCSignalCatalog RequireDiagnosticCatalog()
        {
            if (diagnosticCatalog == null)
            {
                throw new InvalidOperationException(
                    "Load the PI Signal Catalog first.");
            }

            return diagnosticCatalog;
        }

        private LMCBulkConfiguration RequireBulkConfiguration()
        {
            if (bulkConfiguration == null || bulkConfiguration.IsReleased)
            {
                throw new InvalidOperationException(
                    "Configure a Bulk snapshot first.");
            }

            return bulkConfiguration;
        }

        private LMCRecorderConfigurationHandle RequireRecorderConfiguration()
        {
            if (recorderConfiguration == null
                || recorderConfiguration.IsReleased)
            {
                throw new InvalidOperationException(
                    "Configure the Recorder first.");
            }

            return recorderConfiguration;
        }

        private LMCRecorderIdentity RequireRecorderIdentity()
        {
            if (recorderIdentity == null
                || recorderIdentity.IsRecorderReleased)
            {
                throw new InvalidOperationException(
                    "Start or adopt the Recorder first.");
            }

            return recorderIdentity;
        }

        private void UpdateRecorderAdoptionFields(
            LMCRecorderIdentity identity)
        {
            TextRecorderAdoptBootId.Text =
                "0x" + identity.DiagnosticsBootId.ToString("X8");
            TextRecorderAdoptRecordId.Text =
                identity.RecordId.ToString(CultureInfo.InvariantCulture);
            TextRecorderAdoptBufferId.Text =
                identity.BufferId.ToString(CultureInfo.InvariantCulture);
        }

        private List<DiagnosticSignalRow> GetSelectedSignalRows(
            LMCSignalAccessFlags requiredAccess,
            string accessName,
            int maxCount,
            string operationName)
        {
            var selected = diagnosticSignalRows
                .Where(row => row.IsSelected)
                .ToList();
            if (selected.Count == 0)
            {
                throw new InvalidOperationException(
                    "Check at least one signal in the PI Catalog.");
            }

            var denied = selected
                .Where(
                    row => (row.Entry.AccessFlags & requiredAccess)
                        != requiredAccess)
                .Select(row => row.Entry.Alias)
                .ToArray();
            if (denied.Length != 0)
            {
                throw new InvalidOperationException(
                    accessName
                    + " access is not advertised for: "
                    + string.Join(", ", denied)
                    + ".");
            }

            if (maxCount > 0 && selected.Count > maxCount)
            {
                throw new InvalidOperationException(
                    operationName
                    + " selection exceeds the connected PLC limit of "
                    + maxCount
                    + " signals.");
            }

            return selected;
        }

        private DiagnosticSignalRow FindSignalRow(uint signalId)
        {
            return diagnosticSignalRows.FirstOrDefault(
                row => row.Entry.SignalId == signalId);
        }

        private void PopulateRecorderTriggerSignals()
        {
            var triggerType = ComboRecorderTriggerType.SelectedItem
                is LMCRecorderTriggerType
                    ? (LMCRecorderTriggerType)
                        ComboRecorderTriggerType.SelectedItem
                    : LMCRecorderTriggerType.Manual;
            var previous = ComboRecorderTriggerSignal.SelectedItem
                as DiagnosticSignalRow;
            var entries = diagnosticSignalRows
                .Where(
                    row => (row.Entry.AccessFlags
                            & LMCSignalAccessFlags.Recordable) != 0
                        && IsRecorderTriggerValueTypeSupported(
                            triggerType,
                            row.Entry.DataType))
                .ToArray();
            ComboRecorderTriggerSignal.ItemsSource = entries;
            ComboRecorderTriggerSignal.SelectedItem = entries.FirstOrDefault(
                row => previous != null
                    && row.Entry.SignalId == previous.Entry.SignalId)
                ?? entries.FirstOrDefault(
                    row => row.Entry.Alias.EndsWith(
                        ".actual_position",
                        StringComparison.Ordinal))
                ?? entries.FirstOrDefault();
        }

        private void UpdateRecorderBufferModeOptions()
        {
            if (ComboRecorderBufferMode == null)
            {
                return;
            }

            var previousMode = ComboRecorderBufferMode.SelectedItem
                is LMCRecorderBufferMode
                    ? (LMCRecorderBufferMode)
                        ComboRecorderBufferMode.SelectedItem
                    : LMCRecorderBufferMode.Single;
            var modes = new List<LMCRecorderBufferMode>
            {
                LMCRecorderBufferMode.Single
            };
            if (SupportsCapability(LMCDiagnosticCapability.RecorderTrigger))
            {
                modes.Add(LMCRecorderBufferMode.Ring);
            }

            if (SupportsCapability(LMCDiagnosticCapability.RecorderDoubleBank))
            {
                modes.Add(LMCRecorderBufferMode.Double);
            }

            var triggerType = ComboRecorderTriggerType != null
                && ComboRecorderTriggerType.SelectedItem
                    is LMCRecorderTriggerType
                        ? (LMCRecorderTriggerType)
                            ComboRecorderTriggerType.SelectedItem
                        : LMCRecorderTriggerType.Manual;
            var selectedMode = modes.Contains(previousMode)
                ? previousMode
                : LMCRecorderBufferMode.Single;
            if (triggerType != LMCRecorderTriggerType.Manual
                && modes.Contains(LMCRecorderBufferMode.Ring)
                && selectedMode == LMCRecorderBufferMode.Single)
            {
                selectedMode = LMCRecorderBufferMode.Ring;
            }
            else if (triggerType == LMCRecorderTriggerType.Manual
                && selectedMode == LMCRecorderBufferMode.Ring)
            {
                selectedMode = LMCRecorderBufferMode.Single;
            }

            updatingRecorderConfigurationOptions = true;
            try
            {
                ComboRecorderBufferMode.ItemsSource = modes;
                ComboRecorderBufferMode.SelectedItem = selectedMode;
            }
            finally
            {
                updatingRecorderConfigurationOptions = false;
            }
        }

        private void NormalizeRecorderConfigurationSelection()
        {
            if (ComboRecorderBufferMode == null
                || ComboRecorderTriggerType == null)
            {
                return;
            }

            var bufferMode = ComboRecorderBufferMode.SelectedItem
                is LMCRecorderBufferMode
                    ? (LMCRecorderBufferMode)
                        ComboRecorderBufferMode.SelectedItem
                    : LMCRecorderBufferMode.Single;
            var triggerType = ComboRecorderTriggerType.SelectedItem
                is LMCRecorderTriggerType
                    ? (LMCRecorderTriggerType)
                        ComboRecorderTriggerType.SelectedItem
                    : LMCRecorderTriggerType.Manual;
            LMCRecorderBufferMode? replacement = null;
            if (triggerType != LMCRecorderTriggerType.Manual
                && bufferMode == LMCRecorderBufferMode.Single
                && ComboRecorderBufferMode.Items.Contains(
                    LMCRecorderBufferMode.Ring))
            {
                replacement = LMCRecorderBufferMode.Ring;
            }
            else if (triggerType == LMCRecorderTriggerType.Manual
                && bufferMode == LMCRecorderBufferMode.Ring)
            {
                replacement = LMCRecorderBufferMode.Single;
            }

            if (!replacement.HasValue)
            {
                return;
            }

            updatingRecorderConfigurationOptions = true;
            try
            {
                ComboRecorderBufferMode.SelectedItem = replacement.Value;
            }
            finally
            {
                updatingRecorderConfigurationOptions = false;
            }
        }

        private static bool IsRecorderTriggerValueTypeSupported(
            LMCRecorderTriggerType triggerType,
            LMCSignalValueType valueType)
        {
            switch (triggerType)
            {
                case LMCRecorderTriggerType.Window:
                    return valueType == LMCSignalValueType.Int16
                        || valueType == LMCSignalValueType.UInt16
                        || valueType == LMCSignalValueType.Int32
                        || valueType == LMCSignalValueType.UInt32;

                case LMCRecorderTriggerType.Mask:
                    return valueType == LMCSignalValueType.BitField16
                        || valueType == LMCSignalValueType.BitField32;

                default:
                    return valueType > LMCSignalValueType.Invalid
                        && valueType <= LMCSignalValueType.BitField32;
            }
        }

        private LMCRecorderConfiguration BuildRecorderConfiguration(
            IReadOnlyList<DiagnosticSignalRow> selected)
        {
            var signalIds = selected
                .Select(row => row.Entry.SignalId)
                .ToArray();
            var samplePeriodCycles = ParseUInt16(
                TextRecorderSamplePeriod.Text,
                "Recorder sample period cycles");
            var sampleCapacity = ParseUInt32(
                TextRecorderSampleCapacity.Text,
                "Recorder sample capacity");
            var bufferMode = RequireSelectedEnum<LMCRecorderBufferMode>(
                ComboRecorderBufferMode,
                "Recorder buffer mode");
            var triggerType = RequireSelectedEnum<LMCRecorderTriggerType>(
                ComboRecorderTriggerType,
                "Recorder trigger type");

            if (triggerType == LMCRecorderTriggerType.Manual)
            {
                return new LMCRecorderConfiguration(
                    signalIds,
                    samplePeriodCycles,
                    sampleCapacity,
                    bufferMode,
                    LMCRecorderTriggerType.Manual,
                    LMCSignalValueType.Invalid,
                    0,
                    0,
                    0,
                    LMCRecorderTriggerOperator.None,
                    0,
                    0);
            }

            var triggerSignal = ComboRecorderTriggerSignal.SelectedItem
                as DiagnosticSignalRow;
            if (triggerSignal == null)
            {
                throw new InvalidOperationException(
                    "Select a Catalog signal for the Recorder trigger.");
            }

            var triggerValueType = triggerSignal.Entry.DataType;
            var triggerValue = triggerType == LMCRecorderTriggerType.Mask
                ? 0
                : ParseRecorderTriggerRawValue(
                    TextRecorderTriggerValue.Text,
                    triggerValueType,
                    triggerType == LMCRecorderTriggerType.Window
                        ? "Recorder window lower bound"
                        : "Recorder trigger value");
            uint triggerMask;
            switch (triggerType)
            {
                case LMCRecorderTriggerType.Edge:
                    triggerMask = 0;
                    break;

                case LMCRecorderTriggerType.Window:
                    triggerMask = ParseRecorderTriggerRawValue(
                        TextRecorderTriggerMask.Text,
                        triggerValueType,
                        "Recorder window upper bound");
                    ValidateRecorderWindowBounds(
                        triggerValueType,
                        triggerValue,
                        triggerMask);
                    break;

                default:
                    triggerMask = ParseRecorderTriggerRawValue(
                        TextRecorderTriggerMask.Text,
                        triggerValueType,
                        "Recorder trigger mask");
                    break;
            }

            return new LMCRecorderConfiguration(
                signalIds,
                samplePeriodCycles,
                sampleCapacity,
                bufferMode,
                triggerType,
                triggerValueType,
                ParseUInt32Wire(
                    TextRecorderPreTrigger.Text,
                    "Recorder pre-trigger samples"),
                ParseUInt32Wire(
                    TextRecorderPostTrigger.Text,
                    "Recorder post-trigger samples"),
                triggerSignal.Entry.SignalId,
                RequireSelectedEnum<LMCRecorderTriggerOperator>(
                    ComboRecorderTriggerOperator,
                    "Recorder trigger operator"),
                triggerValue,
                triggerMask);
        }

        private void UpdateRecorderTriggerControls()
        {
            if (ComboRecorderTriggerType == null
                || ComboRecorderTriggerOperator == null
                || TextRecorderTriggerValueLabel == null
                || TextRecorderTriggerMaskLabel == null)
            {
                return;
            }

            var triggerType = ComboRecorderTriggerType.SelectedItem
                is LMCRecorderTriggerType
                    ? (LMCRecorderTriggerType)
                        ComboRecorderTriggerType.SelectedItem
                    : LMCRecorderTriggerType.Manual;
            LMCRecorderTriggerOperator[] operators;
            switch (triggerType)
            {
                case LMCRecorderTriggerType.Edge:
                    operators = new[]
                    {
                        LMCRecorderTriggerOperator.RisingEdge,
                        LMCRecorderTriggerOperator.FallingEdge
                    };
                    TextRecorderTriggerValueLabel.Text =
                        "Threshold raw value (decimal or 0x...)";
                    TextRecorderTriggerMaskLabel.Text =
                        "Unused for Edge (wire value forced to 0)";
                    break;

                case LMCRecorderTriggerType.Window:
                    operators = new[]
                    {
                        LMCRecorderTriggerOperator.EnterWindow,
                        LMCRecorderTriggerOperator.ExitWindow
                    };
                    TextRecorderTriggerValueLabel.Text =
                        "Lower bound raw (decimal or 0x...)";
                    TextRecorderTriggerMaskLabel.Text =
                        "Upper bound raw (decimal or 0x...)";
                    break;

                case LMCRecorderTriggerType.Mask:
                    operators = new[]
                    {
                        LMCRecorderTriggerOperator.MaskAllSet,
                        LMCRecorderTriggerOperator.MaskAnySet,
                        LMCRecorderTriggerOperator.MaskAllClear
                    };
                    TextRecorderTriggerValueLabel.Text =
                        "Unused for Mask (wire value forced to 0)";
                    TextRecorderTriggerMaskLabel.Text =
                        "Bit mask raw, non-zero (decimal or 0x...)";
                    break;

                default:
                    operators = new[] { LMCRecorderTriggerOperator.None };
                    TextRecorderTriggerValueLabel.Text =
                        "Trigger value (ignored in Manual mode)";
                    TextRecorderTriggerMaskLabel.Text =
                        "Trigger mask (ignored in Manual mode)";
                    break;
            }

            ComboRecorderTriggerOperator.ItemsSource = operators;
            ComboRecorderTriggerOperator.SelectedIndex = 0;
            PopulateRecorderTriggerSignals();
        }

        private void UpdateRecorderEstimate()
        {
            if (TextRecorderEstimate == null
                || TextRecorderSamplePeriod == null
                || TextRecorderSampleCapacity == null)
            {
                return;
            }

            ushort samplePeriodCycles;
            uint sampleCapacity;
            if (!ushort.TryParse(
                    (TextRecorderSamplePeriod.Text ?? string.Empty).Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out samplePeriodCycles)
                || samplePeriodCycles == 0
                || !uint.TryParse(
                    (TextRecorderSampleCapacity.Text ?? string.Empty).Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out sampleCapacity)
                || sampleCapacity == 0)
            {
                TextRecorderEstimate.Text =
                    "Estimate unavailable until period and capacity are valid positive integers.";
                return;
            }

            var channelCount = diagnosticSignalRows.Count(
                row => row.IsSelected
                    && (row.Entry.AccessFlags & LMCSignalAccessFlags.Recordable)
                        != 0);
            var rawBytes = (ulong)channelCount
                * sampleCapacity
                * sizeof(uint);
            var baseCycleTimeUs = diagnosticCapabilities == null
                || diagnosticCapabilities.BaseCycleTimeUs == 0
                    ? 1000u
                    : diagnosticCapabilities.BaseCycleTimeUs;
            var durationSeconds = (double)sampleCapacity
                * samplePeriodCycles
                * baseCycleTimeUs
                / 1000000.0;
            var capabilityLimit = diagnosticCapabilities == null
                ? "Capabilities not loaded"
                : "PLC max channels="
                    + diagnosticCapabilities.MaxRecorderChannels
                    + ", samples="
                    + diagnosticCapabilities.MaxRecorderSamples;

            TextRecorderEstimate.Text =
                "Estimate: channels="
                + channelCount
                + ", raw="
                + rawBytes.ToString(CultureInfo.InvariantCulture)
                + " bytes ("
                + (rawBytes / 1048576.0).ToString(
                    "F2",
                    CultureInfo.InvariantCulture)
                + " MiB), duration="
                + durationSeconds.ToString("F3", CultureInfo.InvariantCulture)
                + " s @ base cycle "
                + baseCycleTimeUs
                + " us. "
                + capabilityLimit
                + ".";
        }

        private void UpdateSdoOperationControls()
        {
            if (ComboSdoOperation == null || TextSdoWriteData == null)
            {
                return;
            }

            TextSdoWriteData.IsEnabled = false;
        }

        private bool TryCreateFirstSliceSdoReadRequest(
            out LMCSdoRequest request,
            out string validationMessage)
        {
            request = null;

            if (diagnosticCapabilities == null)
            {
                validationMessage =
                    "Refresh diagnostics capabilities first.";
                return false;
            }

            if (!diagnosticCapabilities.Supports(
                LMCDiagnosticCapability.SDORead))
            {
                validationMessage =
                    "SDO Read is not advertised by the connected PLC.";
                return false;
            }

            if (diagnosticOperationTicket != null
                && (diagnosticOperationStatus == null
                    || !diagnosticOperationStatus.IsTerminal))
            {
                validationMessage =
                    "Refresh or cancel the current operation ticket before submitting another SDO Read.";
                return false;
            }

            try
            {
                var mode = RequireSelectedEnum<SdoOperationMode>(
                    ComboSdoOperation,
                    "SDO operation");
                var valueType = RequireSelectedEnum<LMCSignalValueType>(
                    ComboSdoValueType,
                    "SDO value type");
                var dataLength = ParseUInt16Wire(
                    ComboSdoDataLength.Text,
                    "SDO data length",
                    false);
                var slaveReference = ParseUInt16Wire(
                    TextSdoSlaveReference.Text,
                    "SDO slave reference",
                    false);
                var objectIndex = ParseUInt16Wire(
                    TextSdoIndex.Text,
                    "SDO object index",
                    false);
                var subIndex = ParseByteWire(
                    TextSdoSubIndex.Text,
                    "SDO sub-index");
                var timeoutCycles = ParseUInt32(
                    TextSdoTimeoutCycles.Text,
                    "SDO timeout cycles");

                if (mode != SdoOperationMode.Read)
                {
                    validationMessage =
                        "SDO Write is unavailable; select Read.";
                    return false;
                }

                if (slaveReference < 1 || slaveReference > 4)
                {
                    validationMessage =
                        "Slave reference must be between 1 and 4.";
                    return false;
                }

                if (objectIndex != 0x1000 || subIndex != 0)
                {
                    validationMessage =
                        "Only object 0x1000:0 is available in the first slice.";
                    return false;
                }

                if (valueType != LMCSignalValueType.UInt32
                    || dataLength != 4)
                {
                    validationMessage =
                        "Only UInt32, 4-byte SDO Read is available in the first slice.";
                    return false;
                }

                if (timeoutCycles < 1 || timeoutCycles > 60000)
                {
                    validationMessage =
                        "Timeout must be between 1 and 60000 cycles.";
                    return false;
                }

                if (diagnosticCapabilities.MaxSdoDataBytes < 4)
                {
                    validationMessage =
                        "The PLC MaxSdoDataBytes capability is less than 4.";
                    return false;
                }

                request = LMCSdoRequest.CreateRead(
                    slaveReference,
                    objectIndex,
                    subIndex,
                    valueType,
                    dataLength,
                    timeoutCycles);
                validationMessage = null;
                return true;
            }
            catch (InvalidOperationException error)
            {
                validationMessage = error.Message;
                return false;
            }
            catch (ArgumentException error)
            {
                validationMessage = error.Message;
                return false;
            }
        }

        private LMCOperationTicket RequireDiagnosticOperationTicket()
        {
            if (diagnosticOperationTicket == null)
            {
                throw new InvalidOperationException(
                    "Submit an SDO or PI Write operation first.");
            }

            return diagnosticOperationTicket;
        }

        private static T RequireSelectedEnum<T>(
            Selector selector,
            string fieldName)
            where T : struct
        {
            if (!(selector.SelectedItem is T))
            {
                throw new InvalidOperationException(
                    "Select a valid " + fieldName + ".");
            }

            return (T)selector.SelectedItem;
        }

        private static ushort ParseUInt16(string value, string fieldName)
        {
            ushort result;
            if (!ushort.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result)
                || result == 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be between 1 and 65535.");
            }

            return result;
        }

        private static uint ParseUInt32(string value, string fieldName)
        {
            uint result;
            if (!uint.TryParse(
                (value ?? string.Empty).Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result)
                || result == 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be between 1 and 4294967295.");
            }

            return result;
        }

        private static uint ParseUInt32Wire(string value, string fieldName)
        {
            var text = (value ?? string.Empty).Trim();
            var style = NumberStyles.Integer;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(2);
                style = NumberStyles.AllowHexSpecifier;
            }

            uint result;
            if (text.Length == 0
                || !uint.TryParse(
                    text,
                    style,
                    CultureInfo.InvariantCulture,
                    out result))
            {
                throw new InvalidOperationException(
                    fieldName
                    + " must be a UInt32 in decimal or 0x-prefixed hexadecimal form.");
            }

            return result;
        }

        private static uint ParseNonZeroUInt32Wire(
            string value,
            string fieldName)
        {
            var result = ParseUInt32Wire(value, fieldName);
            if (result == 0)
            {
                throw new InvalidOperationException(
                    fieldName + " must be non-zero.");
            }

            return result;
        }

        private static uint ParseRecorderTriggerRawValue(
            string value,
            LMCSignalValueType valueType,
            string fieldName)
        {
            var text = (value ?? string.Empty).Trim();
            if (!text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (valueType == LMCSignalValueType.Int16)
                {
                    short signed16;
                    if (!short.TryParse(
                        text,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out signed16))
                    {
                        throw new InvalidOperationException(
                            fieldName
                            + " must be an Int16 decimal or a canonical 0x-prefixed raw value.");
                    }

                    return unchecked((uint)(int)signed16);
                }

                if (valueType == LMCSignalValueType.Int32)
                {
                    int signed32;
                    if (!int.TryParse(
                        text,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out signed32))
                    {
                        throw new InvalidOperationException(
                            fieldName
                            + " must be an Int32 decimal or a 0x-prefixed raw value.");
                    }

                    return unchecked((uint)signed32);
                }
            }

            return ParseUInt32Wire(value, fieldName);
        }

        private static void ValidateRecorderWindowBounds(
            LMCSignalValueType valueType,
            uint lowerRaw,
            uint upperRaw)
        {
            bool ordered;
            switch (valueType)
            {
                case LMCSignalValueType.Int16:
                    var lowerInt16 = unchecked((short)(ushort)lowerRaw);
                    var upperInt16 = unchecked((short)(ushort)upperRaw);
                    if (lowerRaw != unchecked((uint)(int)lowerInt16)
                        || upperRaw != unchecked((uint)(int)upperInt16))
                    {
                        throw new InvalidOperationException(
                            "Window Int16 bounds must be sign-extended canonical raw values.");
                    }

                    ordered = lowerInt16 <= upperInt16;
                    break;

                case LMCSignalValueType.UInt16:
                    if ((lowerRaw & 0xFFFF0000u) != 0
                        || (upperRaw & 0xFFFF0000u) != 0)
                    {
                        throw new InvalidOperationException(
                            "Window UInt16 bounds must be zero-extended canonical raw values.");
                    }

                    ordered = (ushort)lowerRaw <= (ushort)upperRaw;
                    break;

                case LMCSignalValueType.Int32:
                    ordered = unchecked((int)lowerRaw)
                        <= unchecked((int)upperRaw);
                    break;

                case LMCSignalValueType.UInt32:
                    ordered = lowerRaw <= upperRaw;
                    break;

                default:
                    throw new InvalidOperationException(
                        "Window trigger signals must use Int16, UInt16, Int32, or UInt32 values.");
            }

            if (!ordered)
            {
                throw new InvalidOperationException(
                    "Recorder window upper bound must be greater than or equal to the lower bound.");
            }
        }

        private static ushort ParseUInt16Wire(
            string value,
            string fieldName,
            bool allowZero)
        {
            var result = ParseUInt32Wire(value, fieldName);
            if (result > ushort.MaxValue || (!allowZero && result == 0))
            {
                throw new InvalidOperationException(
                    fieldName
                    + (allowZero
                        ? " must be between 0 and 65535."
                        : " must be between 1 and 65535."));
            }

            return (ushort)result;
        }

        private static byte ParseByteWire(string value, string fieldName)
        {
            var result = ParseUInt32Wire(value, fieldName);
            if (result > byte.MaxValue)
            {
                throw new InvalidOperationException(
                    fieldName + " must be between 0 and 255.");
            }

            return (byte)result;
        }

        private static string FormatCapabilities(
            LMCDiagnosticCapabilities capabilities)
        {
            return "Build="
                + capabilities.DiagnosticsBuild
                + ", Bits=0x"
                + capabilities.CapabilityBits.ToString("X8")
                + " ("
                + capabilities.Capabilities
                + ")"
                + Environment.NewLine
                + "MapRevision=0x"
                + capabilities.MapRevision.ToString("X8")
                + ", CatalogEntries="
                + capabilities.CatalogEntryCount
                + ", BaseCycle="
                + capabilities.BaseCycleTimeUs
                + " us, BootId=0x"
                + capabilities.DiagnosticsBootId.ToString("X8")
                + Environment.NewLine
                + "MaxBulk="
                + capabilities.MaxBulkSignals
                + ", MaxRecorderChannels="
                + capabilities.MaxRecorderChannels
                + ", MaxRecorderSamples="
                + capabilities.MaxRecorderSamples
                + ", MaxChunk="
                + capabilities.MaxChunkDataBytes
                + " bytes, RecorderBuffers="
                + capabilities.RecorderBufferCount
                + ", MaxSDO="
                + capabilities.MaxSdoDataBytes
                + " bytes";
        }

        private static string FormatHealth(LMCEtherCATHealth health)
        {
            return "MapRevision=0x"
                + health.MapRevision.ToString("X8")
                + ", Phase="
                + health.CapturePhase
                + ", Cycle="
                + health.CycleCounter
                + ", Timestamp="
                + health.TimestampUs
                + " us, Sequence="
                + health.SnapshotSequence
                + Environment.NewLine
                + "MasterState="
                + FormatEtherCatState(health.MasterState)
                + ", Flags="
                + health.MasterFlags
                + ", InvalidCycles="
                + health.ConsecutiveInvalidCycles
                + "/"
                + health.InvalidCycleTotal
                + ", Frame="
                + health.FrameTimeUs
                + "/"
                + health.FrameTimeMaxUs
                + " us, RT="
                + health.RtTimeUs
                + "/"
                + health.RtTimeMaxUs
                + " us";
        }

        private static string FormatBulkConfiguration(
            LMCBulkConfiguration configuration)
        {
            return "BulkId="
                + configuration.BulkId
                + ", ConfigRevision="
                + configuration.ConfigRevision
                + ", MapRevision=0x"
                + configuration.MapRevision.ToString("X8")
                + ", State="
                + configuration.InitialState
                + ", Signals="
                + configuration.SignalCount
                + ", ActivationCycle="
                + configuration.ActivationCycle;
        }

        private static string FormatBulkStatus(LMCBulkStatus status)
        {
            return "BulkId="
                + status.BulkId
                + ", ConfigRevision="
                + status.ConfigRevision
                + ", MapRevision=0x"
                + status.MapRevision.ToString("X8")
                + ", State="
                + status.State
                + ", Signals="
                + status.SignalCount
                + ", ActivationCycle="
                + status.ActivationCycle;
        }

        private static string FormatBulkSnapshot(LMCBulkSnapshot snapshot)
        {
            return "BulkId="
                + snapshot.BulkId
                + ", Cycle="
                + snapshot.CycleCounter
                + ", Timestamp="
                + snapshot.TimestampUs
                + " us, Phase="
                + snapshot.CapturePhase
                + ", Sequence="
                + snapshot.SnapshotSequence
                + ", Flags="
                + snapshot.SnapshotFlags
                + ", Partial="
                + snapshot.IsPartial;
        }

        private static string FormatRecorderConfiguration(
            LMCRecorderConfigurationHandle configuration)
        {
            return "ConfigId="
                + configuration.ConfigId
                + ", Revision="
                + configuration.ConfigRevision
                + ", MapRevision=0x"
                + configuration.MapRevision.ToString("X8")
                + ", State="
                + configuration.InitialState
                + ", Mode="
                + configuration.Configuration.BufferMode
                + ", Trigger="
                + configuration.Configuration.TriggerType
                + ", Channels="
                + configuration.ChannelCount
                + ", Capacity="
                + configuration.AcceptedCapacity
                + ", Period="
                + configuration.SamplePeriodUs
                + " us, Duration="
                + ((ulong)configuration.AcceptedCapacity
                    * configuration.SamplePeriodUs)
                + " us, Reserved="
                + configuration.ReservedDataBytes
                + " bytes";
        }

        private static string FormatRecorderIdentity(
            LMCRecorderIdentity identity)
        {
            return "RecordId="
                + identity.RecordId
                + ", BufferId="
                + identity.BufferId
                + ", ConfigId="
                + identity.ConfigId
                + ", State="
                + identity.InitialState
                + ", StartCycle="
                + identity.AcceptedStartCycle
                + ", Capacity="
                + identity.Capacity
                + ", Period="
                + identity.SamplePeriodUs
                + " us";
        }

        private static string FormatRecorderStatus(LMCRecorderStatus status)
        {
            return "RecordId="
                + status.RecordId
                + ", BufferId="
                + status.BufferId
                + ", State="
                + status.State
                + ", Stop="
                + status.StopReason
                + ", Samples="
                + status.SampleCount
                + "/"
                + status.Capacity
                + ", TriggerIndex="
                + (status.HasTrigger
                    ? status.TriggerIndex.ToString(CultureInfo.InvariantCulture)
                    : "none")
                + ", Cycles="
                + status.StartCycle
                + ".."
                + status.EndCycle
                + ", Dropped="
                + status.DroppedSamples
                + ", Overflow="
                + status.OverflowCount;
        }

        private static string FormatRecorderHeader(LMCRecorderHeader header)
        {
            return "Header RecordId="
                + header.RecordId
                + ", BufferId="
                + header.BufferId
                + ", ConfigId="
                + header.ConfigId
                + ", MapRevision=0x"
                + header.MapRevision.ToString("X8")
                + ", Phase="
                + header.CapturePhase
                + Environment.NewLine
                + "Samples="
                + header.SampleCount
                + "/"
                + header.Capacity
                + ", Channels="
                + header.ChannelCount
                + ", Stride="
                + header.SampleStrideBytes
                + " bytes, Period="
                + header.SamplePeriodUs
                + " us, Stop="
                + header.StopReason
                + Environment.NewLine
                + "Cycles="
                + header.StartCycle
                + ".."
                + header.EndCycle
                + ", Trigger="
                + (header.HasTrigger
                    ? header.TriggerIndex.ToString(CultureInfo.InvariantCulture)
                        + " @ cycle "
                        + header.TriggerCycle
                    : "none")
                + ", CRC="
                + header.DataCrcPolicy;
        }

        private static string FormatRecorderData(LMCRecorderData data)
        {
            var header = data.Header;
            return "Download to PC memory complete. RecordId="
                + header.RecordId
                + ", BufferId="
                + header.BufferId
                + ", Samples="
                + header.SampleCount
                + ", Channels="
                + header.ChannelCount
                + ", Stride="
                + header.SampleStrideBytes
                + " bytes, Period="
                + header.SamplePeriodUs
                + " us, Stop="
                + header.StopReason
                + ", CRC="
                + header.DataCrcPolicy;
        }

        private static string FormatOperationTicket(LMCOperationTicket ticket)
        {
            return "TicketId="
                + ticket.TicketId
                + ", Kind="
                + ticket.OperationKind
                + ", QueuedCycle="
                + ticket.QueuedCycle
                + ", BootId=0x"
                + ticket.DiagnosticsBootId.ToString("X8")
                + (ticket.UsesExtendedResultChunks
                    ? ", ExtendedResult="
                        + ticket.RequestedResultLength
                        + " bytes"
                    : string.Empty)
                + ". Refresh the ticket until it reaches a terminal state.";
        }

        private static string FormatOperationStatus(LMCOperationStatus status)
        {
            var resultData = status.ResultData;
            return "TicketId="
                + status.TicketId
                + ", Kind="
                + status.OperationKind
                + ", State="
                + status.State
                + ", Outcome="
                + status.Outcome
                + Environment.NewLine
                + "SubmitCycle="
                + status.SubmitCycle
                + ", CompletionCycle="
                + status.CompletionCycle
                + ", ErrorId="
                + status.OperationErrorId
                + ", Detail=0x"
                + status.OperationDetail.ToString("X8")
                + Environment.NewLine
                + "ResultType="
                + status.ResultValueType
                + ", ResultLength="
                + status.ResultLength
                + ", Data="
                + (resultData.Length == 0
                    ? "-"
                    : BitConverter.ToString(resultData).Replace("-", " "));
        }

        private static string FormatBytePreview(byte[] data, int maxBytes)
        {
            if (data == null || data.Length == 0)
            {
                return "-";
            }

            var previewLength = Math.Min(data.Length, Math.Max(1, maxBytes));
            var preview = BitConverter.ToString(data, 0, previewLength)
                .Replace("-", " ");
            return previewLength == data.Length
                ? preview
                : preview + " ...";
        }

        private static string FormatEtherCatState(uint state)
        {
            string name;
            switch (state)
            {
                case 0:
                    name = "None";
                    break;
                case 1:
                    name = "Init";
                    break;
                case 2:
                    name = "PreOp";
                    break;
                case 3:
                    name = "Boot";
                    break;
                case 4:
                    name = "SafeOp";
                    break;
                case 8:
                    name = "Op";
                    break;
                default:
                    name = "Unknown";
                    break;
            }

            return state + " (" + name + ")";
        }

        private static string FormatRawValue(
            uint rawValue,
            LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                    return rawValue == 0 ? "false" : "true";
                case LMCSignalValueType.Int16:
                    return unchecked((short)rawValue).ToString(
                        CultureInfo.InvariantCulture);
                case LMCSignalValueType.Int32:
                    return unchecked((int)rawValue).ToString(
                        CultureInfo.InvariantCulture);
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return "0x" + (rawValue & 0xFFFFu).ToString("X4");
                case LMCSignalValueType.Real32:
                    return BitConverter.ToSingle(
                            BitConverter.GetBytes(rawValue),
                            0)
                        .ToString("R", CultureInfo.InvariantCulture);
                default:
                    return "0x" + rawValue.ToString("X8");
            }
        }

        private void PopulateRecorderPlotSignals()
        {
            var data = recorderData;
            if (data == null)
            {
                ComboRecorderPlotSignal.ItemsSource =
                    Array.Empty<RecorderPlotSignalItem>();
                return;
            }

            var items = BuildRecorderChannelItems(data.Header.SignalIds);
            ComboRecorderPlotSignal.ItemsSource = items;
            ComboRecorderPlotSignal.SelectedIndex = items.Count == 0 ? -1 : 0;
            RenderRecorderPlot();
        }

        private List<RecorderPlotSignalItem> BuildRecorderChannelItems(
            IReadOnlyList<uint> signalIds)
        {
            var items = new List<RecorderPlotSignalItem>(signalIds.Count);
            for (var index = 0; index < signalIds.Count; index++)
            {
                var row = FindSignalRow(signalIds[index]);
                items.Add(
                    new RecorderPlotSignalItem(
                        checked((ushort)index),
                        signalIds[index],
                        row == null
                            ? "signal_0x" + signalIds[index].ToString("X8")
                            : row.Entry.Alias,
                        row == null
                            ? LMCSignalValueType.UInt32
                            : row.Entry.DataType,
                        row == null ? (ushort)0 : row.Entry.UnitCode,
                        row == null ? 1 : row.Entry.ScaleNumerator,
                        row == null ? 1 : row.Entry.ScaleDenominator));
            }

            return items;
        }

        private void RenderRecorderPlot()
        {
            if (CanvasRecorderPlot == null)
            {
                return;
            }

            CanvasRecorderPlot.Children.Clear();
            var data = recorderData;
            var channel =
                ComboRecorderPlotSignal.SelectedItem as RecorderPlotSignalItem;
            if (data == null || channel == null || data.Header.SampleCount == 0)
            {
                TextRecorderPlotRange.Text = "No downloaded data.";
                return;
            }

            var width = CanvasRecorderPlot.ActualWidth;
            var height = CanvasRecorderPlot.ActualHeight;
            if (width < 80 || height < 60)
            {
                return;
            }

            const double left = 48;
            const double right = 12;
            const double top = 12;
            const double bottom = 28;
            var plotWidth = Math.Max(1, width - left - right);
            var plotHeight = Math.Max(1, height - top - bottom);

            for (var grid = 0; grid <= 4; grid++)
            {
                var y = top + plotHeight * grid / 4.0;
                CanvasRecorderPlot.Children.Add(
                    new Line
                    {
                        X1 = left,
                        X2 = left + plotWidth,
                        Y1 = y,
                        Y2 = y,
                        Stroke = Brushes.Gainsboro,
                        StrokeThickness = 1
                    });
            }

            var desiredBuckets = Math.Max(1, (int)Math.Floor(plotWidth / 2));
            var sampleCount = data.Header.SampleCount;
            var samples = BuildRecorderEnvelope(
                data,
                channel,
                desiredBuckets);

            if (samples.Count == 0)
            {
                TextRecorderPlotRange.Text =
                    "Selected channel contains no finite values.";
                return;
            }

            var minimum = samples.Min(sample => sample.Value);
            var maximum = samples.Max(sample => sample.Value);
            var range = maximum - minimum;
            if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            {
                range = 1;
            }

            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(37, 99, 171)),
                StrokeThickness = 1.5
            };
            foreach (var sample in samples)
            {
                var x = left + (sampleCount <= 1
                    ? 0
                    : plotWidth * sample.Index / (sampleCount - 1.0));
                var y = top + plotHeight
                    - plotHeight * (sample.Value - minimum) / range;
                polyline.Points.Add(new Point(x, y));
            }

            CanvasRecorderPlot.Children.Add(polyline);
            AddCanvasLabel(
                CanvasRecorderPlot,
                maximum.ToString("G6", CultureInfo.InvariantCulture),
                2,
                top - 6);
            AddCanvasLabel(
                CanvasRecorderPlot,
                minimum.ToString("G6", CultureInfo.InvariantCulture),
                2,
                top + plotHeight - 8);
            AddCanvasLabel(CanvasRecorderPlot, "0", left - 2, top + plotHeight + 4);
            AddCanvasLabel(
                CanvasRecorderPlot,
                (sampleCount - 1).ToString(CultureInfo.InvariantCulture),
                left + plotWidth - 42,
                top + plotHeight + 4);

            TextRecorderPlotRange.Text =
                "Samples="
                + sampleCount
                + ", plotted="
                + samples.Count
                + ", min="
                + minimum.ToString("G9", CultureInfo.InvariantCulture)
                + ", max="
                + maximum.ToString("G9", CultureInfo.InvariantCulture)
                + ", period="
                + data.Header.SamplePeriodUs
                + " us";
        }

        private static void AddPlotSample(
            LMCRecorderData data,
            RecorderPlotSignalItem channel,
            uint sampleIndex,
            ICollection<PlotSample> destination)
        {
            var value = GetRecorderPlotValue(data, channel, sampleIndex);
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                destination.Add(new PlotSample(sampleIndex, value));
            }
        }

        private static List<PlotSample> BuildRecorderEnvelope(
            LMCRecorderData data,
            RecorderPlotSignalItem channel,
            int desiredBuckets)
        {
            var sampleCount = data.Header.SampleCount;
            var destination = new List<PlotSample>(desiredBuckets * 2 + 2);
            if (sampleCount <= (uint)(desiredBuckets * 2))
            {
                for (uint sampleIndex = 0;
                    sampleIndex < sampleCount;
                    sampleIndex++)
                {
                    AddPlotSample(data, channel, sampleIndex, destination);
                }

                return destination;
            }

            var bucketSize = Math.Max(
                1u,
                (uint)Math.Ceiling((double)sampleCount / desiredBuckets));
            for (uint bucketStart = 0;
                bucketStart < sampleCount;
                bucketStart += bucketSize)
            {
                var bucketEnd = Math.Min(sampleCount, bucketStart + bucketSize);
                PlotSample minimum = null;
                PlotSample maximum = null;
                for (var sampleIndex = bucketStart;
                    sampleIndex < bucketEnd;
                    sampleIndex++)
                {
                    var value = GetRecorderPlotValue(
                        data,
                        channel,
                        sampleIndex);
                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        continue;
                    }

                    var sample = new PlotSample(sampleIndex, value);
                    if (minimum == null || sample.Value < minimum.Value)
                    {
                        minimum = sample;
                    }

                    if (maximum == null || sample.Value > maximum.Value)
                    {
                        maximum = sample;
                    }
                }

                if (minimum == null)
                {
                    continue;
                }

                if (minimum.Index <= maximum.Index)
                {
                    destination.Add(minimum);
                    if (maximum.Index != minimum.Index)
                    {
                        destination.Add(maximum);
                    }
                }
                else
                {
                    destination.Add(maximum);
                    destination.Add(minimum);
                }
            }

            if (destination.Count == 0 || destination[0].Index != 0)
            {
                var first = new List<PlotSample>(1);
                AddPlotSample(data, channel, 0, first);
                if (first.Count != 0)
                {
                    destination.Insert(0, first[0]);
                }
            }

            if (destination.Count == 0
                || destination[destination.Count - 1].Index
                    != sampleCount - 1)
            {
                AddPlotSample(
                    data,
                    channel,
                    sampleCount - 1,
                    destination);
            }

            return destination;
        }

        private static double GetRecorderPlotValue(
            LMCRecorderData data,
            RecorderPlotSignalItem channel,
            uint sampleIndex)
        {
            var raw = data.GetRawUInt32(sampleIndex, channel.ChannelIndex);
            switch (channel.DataType)
            {
                case LMCSignalValueType.Int16:
                    return unchecked((short)raw);
                case LMCSignalValueType.Int32:
                    return unchecked((int)raw);
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return raw & 0xFFFFu;
                case LMCSignalValueType.Real32:
                    return BitConverter.ToSingle(BitConverter.GetBytes(raw), 0);
                default:
                    return raw;
            }
        }

        private static void AddCanvasLabel(
            Canvas canvas,
            string text,
            double left,
            double top)
        {
            var label = new TextBlock
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10,
                Foreground = Brushes.DimGray,
                Text = text
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }

        private static void WriteRecorderCsv(
            string path,
            LMCRecorderData data,
            IReadOnlyList<RecorderPlotSignalItem> channels)
        {
            using (var writer = new StreamWriter(
                path,
                false,
                new UTF8Encoding(true)))
            {
                var header = data.Header;
                writer.WriteLine("# LASAL Recorder CSV v1");
                writer.WriteLine(
                    "# diagnosticsBootId,0x"
                    + header.DiagnosticsBootId.ToString("X8"));
                writer.WriteLine(
                    "# recordId,"
                    + header.RecordId.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# bufferId,"
                    + header.BufferId.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# configId,"
                    + header.ConfigId.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# configRevision,"
                    + header.ConfigRevision.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# mapRevision,0x" + header.MapRevision.ToString("X8"));
                writer.WriteLine("# capturePhase," + header.CapturePhase);
                writer.WriteLine("# stopReason," + header.StopReason);
                writer.WriteLine(
                    "# startCycle,"
                    + header.StartCycle.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# triggerCycle,"
                    + header.TriggerCycle.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# endCycle,"
                    + header.EndCycle.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# startTimestampUs,"
                    + header.StartTimestampUs.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# triggerTimestampUs,"
                    + header.TriggerTimestampUs.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# endTimestampUs,"
                    + header.EndTimestampUs.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# samplePeriodUs,"
                    + header.SamplePeriodUs.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# sampleCount,"
                    + header.SampleCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# channelCount,"
                    + header.ChannelCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine(
                    "# sampleStrideBytes,"
                    + header.SampleStrideBytes.ToString(
                        CultureInfo.InvariantCulture));
                writer.WriteLine("# dataEncoding," + header.DataEncoding);
                writer.WriteLine("# dataCrcPolicy," + header.DataCrcPolicy);
                writer.WriteLine(
                    "# channel_fields,index,signalId,alias,dataType,unitCode,scaleNumerator,scaleDenominator");
                foreach (var channel in channels)
                {
                    writer.Write("# channel,");
                    writer.Write(
                        channel.ChannelIndex.ToString(
                            CultureInfo.InvariantCulture));
                    writer.Write(",0x");
                    writer.Write(channel.SignalId.ToString("X8"));
                    writer.Write(',');
                    writer.Write(EscapeCsv(channel.Alias));
                    writer.Write(',');
                    writer.Write(channel.DataType);
                    writer.Write(',');
                    writer.Write(
                        channel.UnitCode.ToString(CultureInfo.InvariantCulture));
                    writer.Write(',');
                    writer.Write(
                        channel.ScaleNumerator.ToString(
                            CultureInfo.InvariantCulture));
                    writer.Write(',');
                    writer.WriteLine(
                        channel.ScaleDenominator.ToString(
                            CultureInfo.InvariantCulture));
                }

                writer.Write("sample_index,relative_time_us");
                foreach (var channel in channels)
                {
                    writer.Write(',');
                    writer.Write(EscapeCsv(channel.DisplayName));
                }

                writer.WriteLine();
                for (uint sampleIndex = 0;
                    sampleIndex < data.Header.SampleCount;
                    sampleIndex++)
                {
                    writer.Write(sampleIndex.ToString(CultureInfo.InvariantCulture));
                    writer.Write(',');
                    writer.Write(
                        ((ulong)sampleIndex * data.Header.SamplePeriodUs)
                            .ToString(CultureInfo.InvariantCulture));

                    foreach (var channel in channels)
                    {
                        writer.Write(',');
                        var raw = data.GetRawUInt32(
                            sampleIndex,
                            channel.ChannelIndex);
                        writer.Write(FormatCsvValue(raw, channel.DataType));
                    }

                    writer.WriteLine();
                }
            }
        }

        private static string FormatCsvValue(
            uint raw,
            LMCSignalValueType dataType)
        {
            switch (dataType)
            {
                case LMCSignalValueType.Int16:
                    return unchecked((short)raw).ToString(
                        CultureInfo.InvariantCulture);
                case LMCSignalValueType.Int32:
                    return unchecked((int)raw).ToString(
                        CultureInfo.InvariantCulture);
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return (raw & 0xFFFFu).ToString(
                        CultureInfo.InvariantCulture);
                case LMCSignalValueType.Real32:
                    return BitConverter.ToSingle(BitConverter.GetBytes(raw), 0)
                        .ToString("R", CultureInfo.InvariantCulture);
                default:
                    return raw.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static string EscapeCsv(string value)
        {
            var safe = value ?? string.Empty;
            if (safe.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return safe;
            }

            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private enum SdoOperationMode
        {
            Read
        }

        private sealed class DiagnosticSignalRow : INotifyPropertyChanged
        {
            private bool isSelected;
            private string rawValue = "-";
            private string entryStatus = "-";
            private string cycleCounter = "-";

            public DiagnosticSignalRow(
                LMCSignalCatalogEntry entry,
                bool selected)
            {
                Entry = entry ?? throw new ArgumentNullException("entry");
                isSelected = selected;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public LMCSignalCatalogEntry Entry { get; private set; }
            public byte Axis { get { return Entry.SourceIndex; } }
            public string Alias { get { return Entry.Alias; } }
            public string DisplayName
            {
                get
                {
                    return Alias
                        + " (0x"
                        + Entry.SignalId.ToString("X8")
                        + ", "
                        + Entry.DataType
                        + ")";
                }
            }
            public string SignalIdHex
            {
                get { return "0x" + Entry.SignalId.ToString("X8"); }
            }

            public string DataType { get { return Entry.DataType.ToString(); } }
            public string PdoAddress
            {
                get
                {
                    return Entry.PdoIndex == 0
                        ? "-"
                        : "0x"
                            + Entry.PdoIndex.ToString("X4")
                            + ":"
                            + Entry.PdoSubIndex;
                }
            }

            public string Direction { get { return Entry.PdoDirection.ToString(); } }
            public string RawValue { get { return rawValue; } }
            public string EntryStatus { get { return entryStatus; } }
            public string CycleCounter { get { return cycleCounter; } }

            public bool IsSelected
            {
                get { return isSelected; }
                set
                {
                    if (isSelected == value)
                    {
                        return;
                    }

                    isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }

            public void UpdateValue(
                LMCSignalValueEntry value,
                uint cycle)
            {
                rawValue = FormatRawValue(value.RawValue32, value.ValueType);
                entryStatus = value.EntryStatus
                    + (value.DetailCode == 0
                        ? string.Empty
                        : "/" + value.Detail);
                cycleCounter = cycle.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged("RawValue");
                OnPropertyChanged("EntryStatus");
                OnPropertyChanged("CycleCounter");
            }

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

        private sealed class HealthSlaveRow
        {
            public HealthSlaveRow(LMCEtherCATSlaveHealth value)
            {
                SlaveIndex = value.SlaveIndex;
                PhysicalAxis = value.PhysicalAxis;
                Online = value.Online;
                EtherCATState = FormatEtherCatState(value.EtherCATState);
                ALStatusCode = "0x" + value.ALStatusCode.ToString("X4");
                DS402StatusWord = "0x" + value.DS402StatusWord.ToString("X8");
                AxisError = "0x" + value.AxisError.ToString("X8");
                LastValidCycle = value.LastValidCycle;
                LastStateChangeCycle = value.LastStateChangeCycle;
            }

            public ushort SlaveIndex { get; private set; }
            public ushort PhysicalAxis { get; private set; }
            public bool Online { get; private set; }
            public string EtherCATState { get; private set; }
            public string ALStatusCode { get; private set; }
            public string DS402StatusWord { get; private set; }
            public string AxisError { get; private set; }
            public uint LastValidCycle { get; private set; }
            public uint LastStateChangeCycle { get; private set; }
        }

        private sealed class BulkValueRow
        {
            public BulkValueRow(
                DiagnosticSignalRow catalogRow,
                LMCSignalValueEntry value)
            {
                Alias = catalogRow == null
                    ? "signal_0x" + value.SignalId.ToString("X8")
                    : catalogRow.Entry.Alias;
                SignalIdHex = "0x" + value.SignalId.ToString("X8");
                DataType = value.ValueType.ToString();
                RawValue = FormatRawValue(value.RawValue32, value.ValueType);
                EntryStatus = value.EntryStatus.ToString();
                Detail = value.DetailCode == 0
                    ? "-"
                    : value.Detail + " (" + value.DetailCode + ")";
            }

            public string Alias { get; private set; }
            public string SignalIdHex { get; private set; }
            public string DataType { get; private set; }
            public string RawValue { get; private set; }
            public string EntryStatus { get; private set; }
            public string Detail { get; private set; }
        }

        private sealed class RecorderPlotSignalItem
        {
            public RecorderPlotSignalItem(
                ushort channelIndex,
                uint signalId,
                string alias,
                LMCSignalValueType dataType,
                ushort unitCode,
                int scaleNumerator,
                int scaleDenominator)
            {
                ChannelIndex = channelIndex;
                SignalId = signalId;
                Alias = alias;
                DataType = dataType;
                UnitCode = unitCode;
                ScaleNumerator = scaleNumerator;
                ScaleDenominator = scaleDenominator;
            }

            public ushort ChannelIndex { get; private set; }
            public uint SignalId { get; private set; }
            public string Alias { get; private set; }
            public LMCSignalValueType DataType { get; private set; }
            public ushort UnitCode { get; private set; }
            public int ScaleNumerator { get; private set; }
            public int ScaleDenominator { get; private set; }
            public string DisplayName
            {
                get
                {
                    return ChannelIndex
                        + ": "
                        + Alias
                        + " ("
                        + DataType
                        + ")";
                }
            }
        }

        private sealed class PlotSample
        {
            public PlotSample(uint index, double value)
            {
                Index = index;
                Value = value;
            }

            public uint Index { get; private set; }
            public double Value { get; private set; }
        }
    }
}

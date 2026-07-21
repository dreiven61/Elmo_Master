using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.InternalArgs;
using ElmoMotionControlComponents.GMAS.MMCLibDotNET;
using Microsoft.Win32;

namespace PmasApiWpfTestApp.Version2
{
    public partial class PacketCaptureWindow
    {
        private readonly ObservableCollection<PmasSignalRow> _signalRows = new ObservableCollection<PmasSignalRow>();
        private readonly ObservableCollection<PmasBulkSnapshotRow> _bulkSnapshotRows = new ObservableCollection<PmasBulkSnapshotRow>();
        private MMCPIBulkRead _piBulkRead;
        private RecorderConfiguration _recorderConfiguration;
        private int[] _downloadedRecorderData;
        private bool _hasRecorderStatus;
        private uint _recorderRemainingIndex;
        private uint _recorderTriggerStatus;
        private UploadRecorderHeaderParam _recorderHeader;
        private byte[] _lastSdoResult;
        private string _lastSdoResultText;
        private int _piReadCycle;

        private void InitializeDiagnosticsModels()
        {
            GridSignalCatalog.ItemsSource = _signalRows;
            GridBulkSnapshot.ItemsSource = _bulkSnapshotRows;
            GridEtherCatHealth.ItemsSource = new ObservableCollection<PmasHealthRow>();
            ComboRecorderTriggerSignal.ItemsSource = _signalRows;
            ComboRecorderPlotSignal.Items.Clear();
            ComboRecorderPlotSignal.Items.Add("Recorder buffer 0");
            ComboRecorderPlotSignal.SelectedIndex = 0;

            TextDiagnosticsCapabilities.Text =
                "PMAS direct mode: click Refresh Capabilities for the local MMCLib mapping. This is not a controller-advertised LASAL capability bitmask.";
            TextBulkSummary.Text = "Configure PMAS PI entries first, then configure the native PI bulk reader.";
            TextRecorderSummary.Text = "Select PI Catalog rows and click Use Selected PI, or enter verified native PMAS uiRv/uiRp values. Start re-reads the visible fields and sends MMC_BeginRecordingCmdEx.";
            TextDiagnosticOperationSummary.Text = "PMAS SDO calls are synchronous. Submit performs UploadSDO or DownloadSDO immediately; no ticket is created. Typed result bytes are not the raw wire payload.";
            UpdateRecorderEstimate();
            UpdateRecorderActionAvailability();
        }

        private void ButtonDiagnosticsCapabilities_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("PMAS capability mapping", delegate
            {
                var version = typeof(MMCConnection).Assembly.GetName().Version;
                TextDiagnosticsCapabilities.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "MMCLibDotNET={0}\n"
                    + "Direct: Axis/Group motion, MMCNetwork communication diagnostics, PI read/write, PI bulk, Recorder, typed SDO.\n"
                    + "No 1:1 PMAS contract: LASAL capability bits, Bulk lease/status/release, Recorder BootId/RecordId/BufferId adopt/release/CRC, SDO operation ticket/status/cancel/chunk.",
                    version == null ? "unknown" : version.ToString());
            });
        }

        private void ButtonReadEtherCatHealth_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GetCommDiagnosticsEx", delegate
            {
                Context.EnsureConnected();
                var network = new MMCNetwork(Context.Handle);
                var diagnostics = new MmcCommDiagnosticsEx();
                network.GetCommDiagnosticsEx(ref diagnostics);

                var rows = new ObservableCollection<PmasHealthRow>();
                var slaveDiagnostics = diagnostics.DiagnosticsSlavesArr ?? new MMC_ETHERCAT_DIAGNOSTICS_INFO[0];
                for (var index = 0; index < slaveDiagnostics.Length; index++)
                {
                    var item = slaveDiagnostics[index];
                    var hasPortErrors = item.RXErrorsPort0 != 0
                        || item.RXErrorsPort1 != 0
                        || item.RXErrorsPort2 != 0
                        || item.RXErrorsPort3 != 0
                        || item.InvalidFramesPort0 != 0
                        || item.InvalidFramesPort1 != 0
                        || item.InvalidFramesPort2 != 0
                        || item.InvalidFramesPort3 != 0
                        || item.LostLinkErrorsPort0 != 0
                        || item.LostLinkErrorsPort1 != 0
                        || item.LostLinkErrorsPort2 != 0
                        || item.LostLinkErrorsPort3 != 0;
                    if (!hasPortErrors)
                    {
                        continue;
                    }

                    rows.Add(new PmasHealthRow
                    {
                        SlaveIndex = index.ToString(CultureInfo.InvariantCulture),
                        PhysicalAxis = "n/a",
                        Online = "n/a",
                        EtherCATState = "n/a",
                        ALStatusCode = "n/a",
                        DS402StatusWord = "n/a",
                        AxisError = string.Format(
                            CultureInfo.InvariantCulture,
                            "RX={0}/{1}/{2}/{3}, Invalid={4}/{5}/{6}/{7}, Lost={8}/{9}/{10}/{11}",
                            item.RXErrorsPort0,
                            item.RXErrorsPort1,
                            item.RXErrorsPort2,
                            item.RXErrorsPort3,
                            item.InvalidFramesPort0,
                            item.InvalidFramesPort1,
                            item.InvalidFramesPort2,
                            item.InvalidFramesPort3,
                            item.LostLinkErrorsPort0,
                            item.LostLinkErrorsPort1,
                            item.LostLinkErrorsPort2,
                            item.LostLinkErrorsPort3),
                        LastValidCycle = "n/a",
                        LastStateChangeCycle = "n/a"
                    });
                }

                GridEtherCatHealth.ItemsSource = rows;
                TextEtherCatHealthSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Master NetworkState={0}, MainPathDetected={1}, RedundancyPathDetected={2}, Status={3}, ErrorId={4}, NonZeroPortErrorRows={5}\nMain/redundancy counts are path counts and are not summed into unique online slaves. Port diagnostics do not prove axis identity, per-slave Online, AL status, or DS402 state.\n{6}",
                    diagnostics.usNetworkState,
                    diagnostics.usMainSlaveCount,
                    diagnostics.usRedundancySlaveCount,
                    diagnostics.usStatus,
                    diagnostics.usErrorID,
                    rows.Count,
                    FormatObject(diagnostics));
            });
        }

        private void ButtonLoadSignalCatalog_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMCSingleAxis ctor (GetAxisByName+GetDriveID) + MMC_GetPIVarInfo", delegate
            {
                Context.EnsureConnected();
                var axisNames = SplitValues(TextPiAxisNames.Text);
                var indexes = ParseUInt16Array(TextPiIndexes.Text);
                if (axisNames.Length == 0 || indexes.Length == 0)
                {
                    throw new InvalidOperationException("PI Axis Names and PI Indexes must not be empty.");
                }

                if (!(ComboPiDirection.SelectedItem is PIVarDirection))
                {
                    throw new InvalidOperationException("PI direction is not selected.");
                }

                if (!(ComboPiVarType.SelectedItem is VAR_TYPE))
                {
                    throw new InvalidOperationException("PI value type is not selected.");
                }

                var direction = (PIVarDirection)ComboPiDirection.SelectedItem;
                var expectedType = (VAR_TYPE)ComboPiVarType.SelectedItem;
                if (direction == PIVarDirection.ePI_NONE)
                {
                    throw new InvalidOperationException("PI direction cannot be ePI_NONE.");
                }

                _piBulkRead = null;
                _bulkSnapshotRows.Clear();
                _signalRows.Clear();
                foreach (var axisName in axisNames)
                {
                    var axis = new MMCSingleAxis(axisName, Context.Handle);
                    foreach (var index in indexes)
                    {
                        var info = new NC_PI_ENTRY();
                        axis.GetPIVarInfo(index, direction, ref info);
                        var rawType = Convert.ToByte(info.ucVarType, CultureInfo.InvariantCulture);
                        VAR_TYPE varType;
                        var isTypeSupported = TryMapPiVarType(rawType, out varType);
                        var typeStatus = !isTypeSupported
                            ? "Unsupported PMAS PI type; direct read/write/bulk blocked"
                            : varType == expectedType
                                ? "Info loaded"
                                : string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Controller type {0}; expected {1}; using controller type",
                                    varType,
                                    expectedType);
                        _signalRows.Add(new PmasSignalRow
                        {
                            IsSelected = isTypeSupported,
                            Axis = axis.AxisName,
                            AxisObject = axis,
                            AxisReference = axis.AxisReference,
                            Alias = DecodePiAlias(info.pAliasing),
                            PiIndex = index,
                            DirectionValue = direction,
                            VarTypeValue = varType,
                            IsTypeSupported = isTypeSupported,
                            SignalIdHex = string.Format(CultureInfo.InvariantCulture, "PI:{0}", index),
                            DataType = isTypeSupported
                                ? varType.ToString()
                                : info.ucVarType + " (unsupported)",
                            PdoAddress = string.Format(CultureInfo.InvariantCulture, "0x{0:X4}:{1}", info.usCanOpenIndex, info.ucCanOpenSubIndex),
                            Direction = direction.ToString(),
                            RawValue = "not read",
                            EntryStatus = typeStatus,
                            CycleCounter = "-"
                        });
                    }
                }

                ComboRecorderTriggerSignal.ItemsSource = _signalRows;
                if (_signalRows.Count > 0)
                {
                    ComboRecorderTriggerSignal.SelectedIndex = 0;
                }

                TextDiagnosticsCapabilities.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "PMAS PI entry list loaded: {0} entries, Supported={1}, Unsupported={2}. Direct Read Selected is sequential, not a same-cycle snapshot. This is built from user-configured axis names/indexes; PMAS does not expose the LASAL global SignalId catalog contract.",
                    _signalRows.Count,
                    _signalRows.Count(row => row.IsTypeSupported),
                    _signalRows.Count(row => !row.IsTypeSupported));
                UpdateRecorderEstimate();
            });
        }

        private void ButtonReadSelectedPi_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_ReadPIVar", delegate
            {
                Context.EnsureConnected();
                var selected = GetSelectedSignalRows();
                if (selected.Count == 0)
                {
                    throw new InvalidOperationException("Select at least one PI entry.");
                }
                EnsureSupportedPiRows(selected, "Direct PI read");

                _piReadCycle++;
                foreach (var row in selected)
                {
                    var axis = row.AxisObject ?? new MMCSingleAxis(row.Axis, Context.Handle);
                    var value = new PI_VAR_UNION();
                    axis.ReadPIVar(row.PiIndex, row.DirectionValue, row.VarTypeValue, ref value);
                    row.RawValue = FormatPiValue(value, row.VarTypeValue);
                    row.EntryStatus = "Read OK";
                    row.CycleCounter = _piReadCycle.ToString(CultureInfo.InvariantCulture);
                }
            });
        }

        private void ButtonConfigureBulk_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_GetPIVarInfo via MMCPIBulkRead.AddEntry", delegate
            {
                Context.EnsureConnected();
                var selected = GetSelectedSignalRows();
                if (selected.Count == 0)
                {
                    throw new InvalidOperationException("Select at least one PI entry.");
                }
                EnsureSupportedPiRows(selected, "PI bulk configuration");

                if (!(ComboPiBulkConfig.SelectedItem is NC_BULKREAD_CONFIG_PI_ENUM))
                {
                    throw new InvalidOperationException("PI Bulk configuration is not selected.");
                }

                _piBulkRead = null;
                _bulkSnapshotRows.Clear();
                foreach (var row in _signalRows)
                {
                    row.IsBulkConfigured = false;
                    row.BulkEntry = default(PI_BULKREAD_ENTRY);
                }

                var bulkRead = new MMCPIBulkRead(Context.Handle, (NC_BULKREAD_CONFIG_PI_ENUM)ComboPiBulkConfig.SelectedItem);
                var pendingEntries = new List<KeyValuePair<PmasSignalRow, PI_BULKREAD_ENTRY>>();
                foreach (var row in selected)
                {
                    var axis = row.AxisObject ?? new MMCSingleAxis(row.Axis, Context.Handle);
                    var entry = new PI_BULKREAD_ENTRY
                    {
                        usAxisRef = axis.AxisReference,
                        usIndex = row.PiIndex,
                        eDirection = (byte)row.DirectionValue
                    };
                    bulkRead.AddEntry(axis, entry);
                    pendingEntries.Add(new KeyValuePair<PmasSignalRow, PI_BULKREAD_ENTRY>(row, entry));
                }

                foreach (var pendingEntry in pendingEntries)
                {
                    pendingEntry.Key.BulkEntry = pendingEntry.Value;
                    pendingEntry.Key.IsBulkConfigured = true;
                }
                _piBulkRead = bulkRead;

                TextBulkSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "PMAS PI bulk has {0} entries. AddEntry queried PI metadata from the controller. First Upload sends ConfigurePIBulkRead and PerformPIBulkRead. PMAS has no LASAL Bulk ID/lease.",
                    selected.Count);
            });
        }

        private void ButtonReadBulkStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("PMAS PI bulk local status", delegate
            {
                var count = _signalRows.Count(row => row.IsBulkConfigured);
                TextBulkSummary.Text = _piBulkRead == null
                    ? "PI bulk reader is not configured. PMAS has no controller-side LASAL Bulk Status command."
                    : string.Format(CultureInfo.InvariantCulture, "PI bulk reader configured locally. Entries={0}. PMAS has no controller-side LASAL Bulk Status command.", count);
            });
        }

        private void ButtonReadBulkSnapshot_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMCPIBulkRead.Upload (first Config+Perform; later Perform)", delegate
            {
                if (_piBulkRead == null)
                {
                    throw new InvalidOperationException("Configure the PI bulk reader first.");
                }

                var configured = _signalRows.Where(row => row.IsBulkConfigured).ToList();
                _piBulkRead.Upload();
                _bulkSnapshotRows.Clear();
                foreach (var row in configured)
                {
                    object value;
                    _piBulkRead.GetEntry(row.BulkEntry, out value);
                    var formatted = FormatPiBulkValue(value);
                    row.RawValue = formatted;
                    row.EntryStatus = "Bulk read OK";
                    _bulkSnapshotRows.Add(new PmasBulkSnapshotRow
                    {
                        Alias = row.DisplayName,
                        SignalIdHex = row.SignalIdHex,
                        DataType = row.DataType,
                        RawValue = formatted,
                        EntryStatus = "Read OK",
                        Detail = "PMAS PI Bulk"
                    });
                }

                TextBulkSummary.Text = string.Format(CultureInfo.InvariantCulture, "Latest PMAS PI bulk upload returned {0} entries.", _bulkSnapshotRows.Count);
            });
        }

        private void ButtonReleaseBulk_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Release PMAS PI bulk client state", delegate
            {
                _piBulkRead = null;
                foreach (var row in _signalRows)
                {
                    row.IsBulkConfigured = false;
                }
                _bulkSnapshotRows.Clear();
                TextBulkSummary.Text = "Local PI bulk object released. No controller packet was sent because PMAS has no LASAL Bulk Release command.";
            });
        }

        private void ButtonConfigureRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Configure PMAS recorder parameters", delegate
            {
                _recorderConfiguration = ReadRecorderConfiguration();
                TextRecorderSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Configured locally. Gap={0}, DataLength={1}, SignalBitMask=0x{2:X8}, SignalIds={3}, RecorderParams={4}. Start will send MMC_BeginRecordingCmdEx.",
                    _recorderConfiguration.Gap,
                    _recorderConfiguration.DataLength,
                    _recorderConfiguration.SignalBitMask,
                    string.Join(",", _recorderConfiguration.SignalIds),
                    string.Join(",", _recorderConfiguration.RecorderParams));
            });
        }

        private void ButtonUseSelectedPiForRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Use selected PI entries for PMAS recorder", delegate
            {
                var selected = GetSelectedSignalRows();
                if (selected.Count == 0)
                {
                    throw new InvalidOperationException("Select at least one PI Catalog entry.");
                }

                EnsureSupportedPiRows(selected, "Recorder PI mapping");
                if (selected.Count > 22)
                {
                    throw new InvalidOperationException("Recorder supports at most 22 native uiRv signal IDs.");
                }

                var signalIds = selected.Select(BuildRecorderPiSignalId).ToArray();
                var signalBitMask = (1u << signalIds.Length) - 1u;
                TextRecorderSignalIds.Text = string.Join(",", signalIds.Select(value => string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", value)));
                TextRecorderTriggerMask.Text = string.Format(CultureInfo.InvariantCulture, "0x{0:X8}", signalBitMask);
                _recorderConfiguration = null;
                TextRecorderSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Mapped {0} selected PI entries to native uiRv. SignalBitMask=0x{1:X8}. Review native uiRp trigger parameters, then Configure or Start.",
                    signalIds.Length,
                    signalBitMask);
                UpdateRecorderEstimate();
            });
        }

        private void ButtonStartRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_BeginRecordingCmdEx", delegate
            {
                Context.EnsureConnected();
                _recorderConfiguration = ReadRecorderConfiguration();
                InvalidateRecorderControllerState();
                _downloadedRecorderData = null;
                ProgressRecorderDownload.Value = 0;
                CanvasRecorderPlot.Children.Clear();
                TextRecorderPlotRange.Text = "Starting a new recording; previous downloaded data was cleared.";
                TextRecorderSummary.Text = "MMC_BeginRecordingCmdEx is running; previous Recorder status, header, and downloaded data were invalidated.";
                UpdateRecorderActionAvailability();

                MMCConnection.BeginRecordingEx(
                    Context.Handle,
                    _recorderConfiguration.Gap,
                    _recorderConfiguration.DataLength,
                    _recorderConfiguration.SignalBitMask,
                    _recorderConfiguration.RecorderParams,
                    _recorderConfiguration.SignalIds);
                TextRecorderPlotRange.Text = "New recording started; no data from this run has been downloaded.";
                TextRecorderSummary.Text = "MMC_BeginRecordingCmdEx accepted. Use Refresh Status to read RecordingIndex and TriggerStatus.";
                UpdateRecorderActionAvailability();
            });
        }

        private void ButtonTriggerRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("PMAS Trigger Now", delegate
            {
                TextRecorderSummary.Text = "No verified MMCLibDotNET v3.0.0.7 Trigger Now wrapper exists. Configure the trigger condition through native uiRp values before MMC_BeginRecordingCmdEx.";
            });
        }

        private void ButtonStopRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_StopRecordingCmd", delegate
            {
                Context.EnsureConnected();
                InvalidateRecorderControllerState();
                TextRecorderSummary.Text = "MMC_StopRecordingCmd is running; cached status and header were invalidated.";
                UpdateRecorderActionAvailability();
                MMCConnection.StopRecording(Context.Handle);
                TextRecorderSummary.Text = "MMC_StopRecordingCmd completed. Refresh status and read the header before download.";
                UpdateRecorderActionAvailability();
            });
        }

        private void ButtonRecorderStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_RecStatusCmd", delegate
            {
                Context.EnsureConnected();
                InvalidateRecorderControllerState();
                TextRecorderSummary.Text = "MMC_RecStatusCmd is running; the previous status and header were invalidated.";
                UpdateRecorderActionAvailability();
                uint recordingIndex;
                uint triggerStatus;
                MMCConnection.GetRecordingStatus(Context.Handle, out recordingIndex, out triggerStatus);
                _hasRecorderStatus = true;
                _recorderRemainingIndex = recordingIndex;
                _recorderTriggerStatus = triggerStatus;
                var readyMask = GetRecorderReadyBufferMask(triggerStatus);
                _recorderHeader = null;

                TextRecorderSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "RemainingIndex={0}, uiSr=0x{1:X8}, Phase={2}, ReadyBuffers={3}. {4}",
                    _recorderRemainingIndex,
                    triggerStatus,
                    GetRecorderPhaseName(triggerStatus),
                    GetRecorderReadyBufferName(readyMask),
                    (readyMask & 0x03u) == 0 ? "Refresh again after the recording reaches a ready state." : "Read Header before Download.");
                UpdateRecorderActionAvailability();
            });
        }

        private void ButtonReadRecorderHeader_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_UploadDataHeaderCmd", delegate
            {
                Context.EnsureConnected();
                _recorderHeader = null;
                TextRecorderSummary.Text = "MMC_UploadDataHeaderCmd is running; the previous header was invalidated.";
                UpdateRecorderActionAvailability();
                EnsureRecorderHasReadyBuffer();
                UploadRecorderHeaderParam header;
                MMCConnection.GetRecordingDataHeader(Context.Handle, out header);
                if (header.Status != 0 || header.ErrorID != 0)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Recorder header failed. Status=0x{0:X4}, ErrorID={1}.",
                        header.Status,
                        header.ErrorID));
                }

                if (header.Rl == 0)
                {
                    throw new InvalidOperationException("Recorder header has Rl=0. No samples are available; refresh status and verify that the selected buffer is ready.");
                }

                _recorderHeader = header;
                TextRecorderFrom.Text = "0";
                TextRecorderTo.Text = (header.Rl - 1).ToString(CultureInfo.InvariantCulture);
                TextRecorderSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Global Recorder header accepted: Rl={0}, Rg={1}, Rc=0x{2:X8}, Ti={3}, Ts={4}. Download range was set to [0..{5}].\n{6}",
                    header.Rl,
                    header.Rg,
                    header.Rc,
                    header.Ti,
                    header.Ts,
                    header.Rl - 1,
                    FormatObject(header));
                UpdateRecorderActionAvailability();
            });
        }

        private void ButtonDownloadRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_UploadDataCmd", delegate
            {
                Context.EnsureConnected();
                var from = ParseUInt32(TextRecorderFrom.Text);
                var to = ParseUInt32(TextRecorderTo.Text);
                var bufferIndex = ParseUInt32(TextRecorderBufferIndex.Text);
                EnsureRecorderBufferReady(bufferIndex);
                if (_recorderHeader == null)
                {
                    throw new InvalidOperationException("Read and validate the Recorder header before Download.");
                }

                if (to < from)
                {
                    throw new InvalidOperationException("Recorder To must be greater than or equal to From.");
                }

                if (to >= _recorderHeader.Rl)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Recorder range [{0}..{1}] exceeds header Rl={2}. To must be less than Rl.",
                        from,
                        to,
                        _recorderHeader.Rl));
                }

                var length = checked((int)(to - from + 1));
                var data = new int[length];
                _downloadedRecorderData = null;
                ProgressRecorderDownload.Value = 0;
                CanvasRecorderPlot.Children.Clear();
                TextRecorderPlotRange.Text = "Recorder download is in progress; no current data is available for export.";
                TextRecorderSummary.Text = "MMC_UploadDataCmd is running; the previous downloaded data was cleared.";
                UpdateRecorderActionAvailability();
                MMCConnection.GetRecordingData(Context.Handle, from, to, bufferIndex, out data);
                _downloadedRecorderData = data;
                ProgressRecorderDownload.Value = 1;
                TextRecorderSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Downloaded {0} Int32 values from [{1}..{2}], BufferIndex={3}. Data is in PC memory; Export CSV writes a file.",
                    data.Length,
                    from,
                    to,
                    bufferIndex);
                DrawRecorderPlot();
                UpdateRecorderActionAvailability();
            });
        }

        private void ButtonCancelRecorderDownload_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Cancel recorder download", delegate
            {
                TextRecorderSummary.Text = "The PMAS wrapper call is synchronous; there is no active PC-side asynchronous download to cancel. This button never stops controller recording.";
            });
        }

        private void ButtonExportRecorderCsv_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Export recorder CSV", delegate
            {
                if (_downloadedRecorderData == null || _downloadedRecorderData.Length == 0)
                {
                    throw new InvalidOperationException("Download recorder data first.");
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "PMAS_Recorder_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".csv"
                };
                if (dialog.ShowDialog(this) != true)
                {
                    return;
                }

                using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine("sample_index,raw_value");
                    for (var index = 0; index < _downloadedRecorderData.Length; index++)
                    {
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1}", index, _downloadedRecorderData[index]));
                    }
                }

                TextRecorderSummary.Text = "CSV exported: " + dialog.FileName;
            });
        }

        private void ButtonReleaseRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Release PMAS recorder PC state", delegate
            {
                _recorderConfiguration = null;
                _downloadedRecorderData = null;
                InvalidateRecorderControllerState();
                ProgressRecorderDownload.Value = 0;
                CanvasRecorderPlot.Children.Clear();
                TextRecorderPlotRange.Text = "No downloaded data.";
                TextRecorderSummary.Text = "Local recorder data cleared. PMAS has no LASAL Recorder Buffer/Configuration Release command.";
                UpdateRecorderActionAvailability();
            });
        }

        private void ButtonAdoptRecorder_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("PMAS recorder adoption", delegate
            {
                TextRecorderSummary.Text = "PMAS MMCLib accesses recorder state by connection handle and has no LASAL BootId/RecordId/BufferId adoption contract. No controller packet was sent.";
            });
        }

        private void RecorderConfiguration_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_uiInitialized)
            {
                UpdateRecorderEstimate();
            }
        }

        private void RecorderConfiguration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_uiInitialized)
            {
                UpdateRecorderEstimate();
            }
        }

        private void UpdateRecorderEstimate()
        {
            if (TextRecorderEstimate == null)
            {
                return;
            }

            try
            {
                var capacity = ParseUInt32(TextRecorderSampleCapacity.Text);
                var signalIds = TextRecorderSignalIds == null ? new uint[0] : ParseUInt32Array(TextRecorderSignalIds.Text);
                TextRecorderEstimate.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "PMAS native inputs: DataLength={0}, SignalCount={1}. Exact memory/trigger interpretation is defined by Maestro Recorder Params, not the LASAL Recorder schema.",
                    capacity,
                    signalIds.Length);
            }
            catch
            {
                TextRecorderEstimate.Text = "Enter valid PMAS recorder numeric values.";
            }
        }

        private void ComboRecorderPlotSignal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DrawRecorderPlot();
        }

        private void CanvasRecorderPlot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawRecorderPlot();
        }

        private void DrawRecorderPlot()
        {
            if (CanvasRecorderPlot == null)
            {
                return;
            }

            CanvasRecorderPlot.Children.Clear();
            if (_downloadedRecorderData == null || _downloadedRecorderData.Length == 0
                || CanvasRecorderPlot.ActualWidth <= 1 || CanvasRecorderPlot.ActualHeight <= 1)
            {
                if (TextRecorderPlotRange != null)
                {
                    TextRecorderPlotRange.Text = "No downloaded data.";
                }
                return;
            }

            var min = _downloadedRecorderData.Min();
            var max = _downloadedRecorderData.Max();
            var span = Math.Max(1.0, max - (double)min);
            var width = CanvasRecorderPlot.ActualWidth;
            var height = CanvasRecorderPlot.ActualHeight;
            var maximumPoints = Math.Max(2, (int)Math.Floor(width));
            var step = Math.Max(1, (int)Math.Ceiling(_downloadedRecorderData.Length / (double)maximumPoints));
            var points = new PointCollection();
            for (var index = 0; index < _downloadedRecorderData.Length; index += step)
            {
                var x = _downloadedRecorderData.Length == 1
                    ? 0
                    : index * (width - 1) / (_downloadedRecorderData.Length - 1.0);
                var y = height - 1 - ((_downloadedRecorderData[index] - min) / span * (height - 2));
                points.Add(new Point(x, y));
            }

            CanvasRecorderPlot.Children.Add(new Polyline
            {
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 1.2,
                Points = points
            });
            TextRecorderPlotRange.Text = string.Format(CultureInfo.InvariantCulture, "Count={0}, Min={1}, Max={2}, PlotStep={3}", _downloadedRecorderData.Length, min, max, step);
        }

        private void ButtonSubmitSdo_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_SendSdoCmd (synchronous)", delegate
            {
                _lastSdoResult = null;
                _lastSdoResultText = null;
                var axis = RequireCaptureAxis();
                var index = ParseUInt16(TextSdoIndex.Text);
                var subIndex = ParseByte(TextSdoSubIndex.Text);
                var timeout = checked((int)ParseUInt32(TextSdoTimeoutCycles.Text));
                var operation = Convert.ToString(ComboSdoOperation.SelectedItem, CultureInfo.InvariantCulture);
                var valueType = Convert.ToString(ComboSdoValueType.SelectedItem, CultureInfo.InvariantCulture);
                var dataLength = GetSdoDataLength(valueType);

                if (string.Equals(operation, "Read", StringComparison.OrdinalIgnoreCase))
                {
                    _lastSdoResultText = ReadSdoValue(axis, index, subIndex, timeout, valueType, out _lastSdoResult);
                }
                else if (string.Equals(operation, "Write", StringComparison.OrdinalIgnoreCase))
                {
                    WriteSdoValue(axis, index, subIndex, timeout, valueType, TextSdoWriteData.Text);
                    _lastSdoResult = new byte[0];
                    _lastSdoResultText = "Write completed";
                }
                else
                {
                    throw new InvalidOperationException("Select Read or Write.");
                }

                TextDiagnosticOperationSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Synchronous {0} completed. Axis={1}, Index=0x{2:X4}:{3}, Type={4}, TypedLength={5}, Result={6}. No operation ticket was created.",
                    operation,
                    axis.AxisName,
                    index,
                    subIndex,
                    valueType,
                    dataLength,
                    _lastSdoResultText);
            });
        }

        private void ButtonRefreshDiagnosticOperation_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Refresh PMAS SDO result", delegate
            {
                TextDiagnosticOperationSummary.Text = "PMAS SDO is synchronous; there is no ticket status call. Last result: " + (_lastSdoResultText ?? "none");
            });
        }

        private void ButtonDownloadSdoResult_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Show PMAS SDO result", delegate
            {
                TextDiagnosticOperationSummary.Text = _lastSdoResult == null
                    ? "No SDO result is available."
                    : "Host-endian bytes re-encoded from the typed SDO value (not raw wire payload): 0x" + BitConverter.ToString(_lastSdoResult).Replace("-", string.Empty);
            });
        }

        private void ButtonExportSdoResult_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Save PMAS SDO result", delegate
            {
                if (_lastSdoResult == null || _lastSdoResult.Length == 0)
                {
                    throw new InvalidOperationException("No non-empty SDO read result is available.");
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*",
                    FileName = "PMAS_SDO_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".bin"
                };
                if (dialog.ShowDialog(this) == true)
                {
                    File.WriteAllBytes(dialog.FileName, _lastSdoResult);
                    TextDiagnosticOperationSummary.Text = "Host-endian typed-value bytes saved (not raw wire payload): " + dialog.FileName;
                }
            });
        }

        private void ButtonCancelDiagnosticOperation_Click(object sender, RoutedEventArgs e)
        {
            ExecuteLocalAction("Cancel PMAS SDO operation", delegate
            {
                TextDiagnosticOperationSummary.Text = "No ticket exists to cancel. The MMCLib UploadSDO/DownloadSDO call completes synchronously or throws.";
            });
        }

        private void ButtonSubmitPiWrite_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_WritePIVar", delegate
            {
                Context.EnsureConnected();
                var selected = GetSelectedSignalRows();
                if (selected.Count != 1)
                {
                    throw new InvalidOperationException("Select exactly one PI entry for a direct PMAS write.");
                }
                EnsureSupportedPiRows(selected, "Direct PI write");

                var row = selected[0];
                var axis = row.AxisObject ?? new MMCSingleAxis(row.Axis, Context.Handle);
                var value = ParsePiValue(TextPiWriteRawValue.Text, row.VarTypeValue);
                axis.WritePIVar(row.PiIndex, value, row.VarTypeValue);
                row.RawValue = TextPiWriteRawValue.Text;
                row.EntryStatus = "Write issued";
                TextDiagnosticOperationSummary.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Direct PMAS PI write completed. Axis={0}, Index={1}, Type={2}, Value={3}. PMAS does not apply the LASAL SDK/PLC allowlist contract; equipment-side write safety remains the operator's responsibility.",
                    row.Axis,
                    row.PiIndex,
                    row.VarTypeValue,
                    TextPiWriteRawValue.Text);
            });
        }

        private void SdoOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_uiInitialized || TextDiagnosticOperationSummary == null)
            {
                return;
            }

            TextDiagnosticOperationSummary.Text = string.Format(
                CultureInfo.InvariantCulture,
                "PMAS direct {0}: Submit performs a synchronous typed SDO call. Ticket controls below have no controller-side PMAS equivalent.",
                Convert.ToString(ComboSdoOperation.SelectedItem, CultureInfo.InvariantCulture));
        }

        private void SdoValueType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSdoDerivedDataLength();
        }

        private void UpdateSdoDerivedDataLength()
        {
            if (ComboSdoValueType == null || ComboSdoDataLength == null)
            {
                return;
            }

            var valueType = Convert.ToString(ComboSdoValueType.SelectedItem, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(valueType))
            {
                return;
            }

            ComboSdoDataLength.SelectedItem = GetSdoDataLength(valueType).ToString(CultureInfo.InvariantCulture);
        }

        private static int GetSdoDataLength(string valueType)
        {
            switch (valueType)
            {
                case "Byte":
                    return 1;
                case "Int16":
                case "UInt16":
                    return 2;
                case "Int32":
                case "UInt32":
                case "Float":
                    return 4;
                default:
                    throw new NotSupportedException("Unsupported SDO value type: " + valueType);
            }
        }

        private List<PmasSignalRow> GetSelectedSignalRows()
        {
            return _signalRows.Where(row => row.IsSelected).ToList();
        }

        private void RecorderDownloadSelection_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_uiInitialized)
            {
                UpdateRecorderActionAvailability();
            }
        }

        private static uint BuildRecorderPiSignalId(PmasSignalRow row)
        {
            if (row.DirectionValue != PIVarDirection.ePI_INPUT
                && row.DirectionValue != PIVarDirection.ePI_OUTPUT)
            {
                throw new InvalidOperationException("Recorder PI mapping supports only input or output PI entries.");
            }

            return ((uint)row.AxisReference << 16)
                | ((uint)row.PiIndex & 0x3FFFu)
                | ((uint)row.DirectionValue << 14);
        }

        private void InvalidateRecorderControllerState()
        {
            _hasRecorderStatus = false;
            _recorderRemainingIndex = 0;
            _recorderTriggerStatus = 0;
            _recorderHeader = null;
        }

        private static uint GetRecorderReadyBufferMask(uint triggerStatus)
        {
            return (triggerStatus >> 8) & 0xFFu;
        }

        private static bool IsRecorderBufferReady(uint bufferIndex, uint readyMask)
        {
            return bufferIndex <= 1 && (readyMask & (1u << (int)bufferIndex)) != 0;
        }

        private void EnsureRecorderBufferReady(uint bufferIndex)
        {
            if (bufferIndex > 1)
            {
                throw new InvalidOperationException("PMAS Recorder Buffer index must be 0 or 1.");
            }

            if (!_hasRecorderStatus)
            {
                throw new InvalidOperationException("Refresh Recorder status before reading the header or downloading data.");
            }

            var readyMask = GetRecorderReadyBufferMask(_recorderTriggerStatus);
            if (!IsRecorderBufferReady(bufferIndex, readyMask))
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Recorder BufferIndex={0} is not ready. uiSr=0x{1:X8}, ReadyBuffers={2}.",
                    bufferIndex,
                    _recorderTriggerStatus,
                    GetRecorderReadyBufferName(readyMask)));
            }
        }

        private void EnsureRecorderHasReadyBuffer()
        {
            if (!_hasRecorderStatus)
            {
                throw new InvalidOperationException("Refresh Recorder status before reading the header.");
            }

            if ((GetRecorderReadyBufferMask(_recorderTriggerStatus) & 0x03u) == 0)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Recorder has no ready buffer. uiSr=0x{0:X8}.",
                    _recorderTriggerStatus));
            }
        }

        private static string GetRecorderPhaseName(uint triggerStatus)
        {
            switch (triggerStatus & 0xFFu)
            {
                case 0:
                    return "Arming";
                case 1:
                    return "Waiting Opposite Trigger";
                case 2:
                    return "Waiting Trigger";
                case 3:
                    return "Trigger Detected";
                case 4:
                    return "No Trigger";
                default:
                    return "Unknown(" + (triggerStatus & 0xFFu).ToString(CultureInfo.InvariantCulture) + ")";
            }
        }

        private static string GetRecorderReadyBufferName(uint readyMask)
        {
            switch (readyMask)
            {
                case 0:
                    return "None";
                case 1:
                    return "BufferIndex 0 (native Buffer 1)";
                case 2:
                    return "BufferIndex 1 (native Buffer 2)";
                case 3:
                    return "BufferIndex 0 and 1 (both native buffers)";
                default:
                    return "UnknownMask(0x" + readyMask.ToString("X2", CultureInfo.InvariantCulture) + ")";
            }
        }

        private void UpdateRecorderActionAvailability()
        {
            if (ButtonReadRecorderHeader == null || ButtonDownloadRecorder == null || ButtonExportRecorderCsv == null)
            {
                return;
            }

            var readyMask = GetRecorderReadyBufferMask(_recorderTriggerStatus);
            ButtonReadRecorderHeader.IsEnabled = Context.IsConnected && _hasRecorderStatus && (readyMask & 0x03u) != 0;

            var canDownload = false;
            if (Context.IsConnected && _recorderHeader != null && TextRecorderBufferIndex != null)
            {
                try
                {
                    var bufferIndex = ParseUInt32(TextRecorderBufferIndex.Text);
                    var from = ParseUInt32(TextRecorderFrom.Text);
                    var to = ParseUInt32(TextRecorderTo.Text);
                    canDownload = IsRecorderBufferReady(bufferIndex, readyMask)
                        && from <= to
                        && to < _recorderHeader.Rl;
                }
                catch
                {
                    canDownload = false;
                }
            }

            ButtonDownloadRecorder.IsEnabled = canDownload;
            ButtonExportRecorderCsv.IsEnabled = _downloadedRecorderData != null && _downloadedRecorderData.Length > 0;
        }

        private static void EnsureSupportedPiRows(IEnumerable<PmasSignalRow> rows, string operation)
        {
            var unsupported = rows.Where(row => !row.IsTypeSupported).ToArray();
            if (unsupported.Length == 0)
            {
                return;
            }

            throw new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0} is blocked because these controller PI types have no safe VAR_TYPE overload in MMCLibDotNET v3.0.0.7: {1}",
                operation,
                string.Join(", ", unsupported.Select(row => row.DisplayName + "=" + row.DataType))));
        }

        private RecorderConfiguration ReadRecorderConfiguration()
        {
            var signalIds = ParseUInt32Array(TextRecorderSignalIds.Text);
            var recorderParams = ParseUInt32Array(TextRecorderParams.Text);
            if (signalIds.Length == 0)
            {
                throw new InvalidOperationException("Recorder Signal IDs must not be empty.");
            }

            if (recorderParams.Length == 0)
            {
                throw new InvalidOperationException("Recorder Params must not be empty.");
            }

            if (signalIds.Length > 22)
            {
                throw new InvalidOperationException("Recorder supports at most 22 native uiRv signal IDs.");
            }

            if (recorderParams.Length > 8)
            {
                throw new InvalidOperationException("Recorder supports at most 8 native uiRp parameters.");
            }

            var gap = ParseUInt32(TextRecorderSamplePeriod.Text);
            var dataLength = ParseUInt32(TextRecorderSampleCapacity.Text);
            var signalBitMask = ParseUInt32(TextRecorderTriggerMask.Text);
            if (gap == 0 || dataLength == 0 || signalBitMask == 0)
            {
                throw new InvalidOperationException("Recorder Gap, DataLength, and Signal Bit Mask must be positive.");
            }

            var allowedMask = (1u << signalIds.Length) - 1u;
            if ((signalBitMask & ~allowedMask) != 0)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Recorder Signal Bit Mask 0x{0:X8} selects a bit outside the {1} supplied uiRv entries.",
                    signalBitMask,
                    signalIds.Length));
            }

            return new RecorderConfiguration
            {
                Gap = gap,
                DataLength = dataLength,
                SignalBitMask = signalBitMask,
                SignalIds = signalIds,
                RecorderParams = recorderParams
            };
        }

        private string ReadSdoValue(MMCSingleAxis axis, ushort index, byte subIndex, int timeout, string valueType, out byte[] bytes)
        {
            switch (valueType)
            {
                case "Byte":
                    byte byteValue;
                    axis.UploadSDO(index, subIndex, out byteValue, timeout);
                    bytes = new[] { byteValue };
                    return byteValue.ToString(CultureInfo.InvariantCulture);
                case "Int16":
                    short int16Value;
                    axis.UploadSDO(index, subIndex, out int16Value, timeout);
                    bytes = BitConverter.GetBytes(int16Value);
                    return int16Value.ToString(CultureInfo.InvariantCulture);
                case "UInt16":
                    ushort uint16Value;
                    axis.UploadSDO(index, subIndex, out uint16Value, timeout);
                    bytes = BitConverter.GetBytes(uint16Value);
                    return uint16Value.ToString(CultureInfo.InvariantCulture);
                case "Int32":
                    int int32Value;
                    axis.UploadSDO(index, subIndex, out int32Value, timeout);
                    bytes = BitConverter.GetBytes(int32Value);
                    return int32Value.ToString(CultureInfo.InvariantCulture);
                case "UInt32":
                    uint uint32Value;
                    axis.UploadSDO(index, subIndex, out uint32Value, timeout);
                    bytes = BitConverter.GetBytes(uint32Value);
                    return uint32Value.ToString(CultureInfo.InvariantCulture);
                case "Float":
                    float floatValue;
                    axis.UploadSDO(index, subIndex, out floatValue, timeout);
                    bytes = BitConverter.GetBytes(floatValue);
                    return floatValue.ToString(CultureInfo.InvariantCulture);
                default:
                    throw new NotSupportedException("Unsupported SDO value type: " + valueType);
            }
        }

        private void WriteSdoValue(MMCSingleAxis axis, ushort index, byte subIndex, int timeout, string valueType, string text)
        {
            switch (valueType)
            {
                case "Byte":
                    axis.DownloadSDO(index, subIndex, ParseByte(text), timeout);
                    return;
                case "Int16":
                    axis.DownloadSDO(index, subIndex, ParseInt16(text), timeout);
                    return;
                case "UInt16":
                    axis.DownloadSDO(index, subIndex, ParseUInt16(text), timeout);
                    return;
                case "Int32":
                    axis.DownloadSDO(index, subIndex, ParseInt32(text), timeout);
                    return;
                case "UInt32":
                    axis.DownloadSDO(index, subIndex, ParseUInt32(text), timeout);
                    return;
                case "Float":
                    axis.DownloadSDO(index, subIndex, ParseFloat(text), timeout);
                    return;
                default:
                    throw new NotSupportedException("Unsupported SDO value type: " + valueType);
            }
        }

        private static short ParseInt16(string value)
        {
            var normalized = Normalize(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? unchecked((short)ushort.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture))
                : short.Parse(normalized, CultureInfo.InvariantCulture);
        }

        private static bool TryMapPiVarType(byte rawType, out VAR_TYPE varType)
        {
            switch (rawType)
            {
                case 1:
                    varType = VAR_TYPE.S_BYTE;
                    return true;
                case 2:
                    varType = VAR_TYPE.BYTE;
                    return true;
                case 3:
                    varType = VAR_TYPE.SHORT;
                    return true;
                case 4:
                    varType = VAR_TYPE.USHORT;
                    return true;
                case 5:
                    varType = VAR_TYPE.INT;
                    return true;
                case 6:
                    varType = VAR_TYPE.UINT;
                    return true;
                case 9:
                    varType = VAR_TYPE.FLOAT;
                    return true;
                default:
                    varType = VAR_TYPE.NOT_SET;
                    return false;
            }
        }

        private static string FormatPiValue(PI_VAR_UNION value, VAR_TYPE varType)
        {
            switch (varType)
            {
                case VAR_TYPE.S_BYTE:
                    return value.s_byte.ToString(CultureInfo.InvariantCulture);
                case VAR_TYPE.BYTE:
                    return value._byte.ToString(CultureInfo.InvariantCulture);
                case VAR_TYPE.SHORT:
                    return value._int16.ToString(CultureInfo.InvariantCulture);
                case VAR_TYPE.USHORT:
                    return value._uint16.ToString(CultureInfo.InvariantCulture);
                case VAR_TYPE.INT:
                    return value._int32.ToString(CultureInfo.InvariantCulture);
                case VAR_TYPE.UINT:
                    return value._uint32.ToString(CultureInfo.InvariantCulture);
                case VAR_TYPE.FLOAT:
                    return value._single.ToString(CultureInfo.InvariantCulture);
                default:
                    throw new NotSupportedException("Unsupported PI type: " + varType);
            }
        }

        private static PI_VAR_UNION ParsePiValue(string text, VAR_TYPE varType)
        {
            var normalized = Normalize(text);
            switch (varType)
            {
                case VAR_TYPE.S_BYTE:
                    return new PI_VAR_UNION { s_byte = normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? unchecked((sbyte)ParseByte(normalized)) : sbyte.Parse(normalized, CultureInfo.InvariantCulture) };
                case VAR_TYPE.BYTE:
                    return new PI_VAR_UNION { _byte = ParseByte(normalized) };
                case VAR_TYPE.SHORT:
                    return new PI_VAR_UNION { _int16 = ParseInt16(normalized) };
                case VAR_TYPE.USHORT:
                    return new PI_VAR_UNION { _uint16 = ParseUInt16(normalized) };
                case VAR_TYPE.INT:
                    return new PI_VAR_UNION { _int32 = ParseInt32(normalized) };
                case VAR_TYPE.UINT:
                    return new PI_VAR_UNION { _uint32 = ParseUInt32(normalized) };
                case VAR_TYPE.FLOAT:
                    return new PI_VAR_UNION { _single = ParseFloat(normalized) };
                default:
                    throw new NotSupportedException("Unsupported PI type: " + varType);
            }
        }

        private static string DecodePiAlias(byte[] aliasBytes)
        {
            if (aliasBytes == null || aliasBytes.Length == 0)
            {
                return string.Empty;
            }

            var length = Array.IndexOf(aliasBytes, (byte)0);
            if (length < 0)
            {
                length = aliasBytes.Length;
            }
            return Encoding.ASCII.GetString(aliasBytes, 0, length).Trim();
        }

        private static string FormatPiBulkValue(object value)
        {
            if (value == null)
            {
                return "<null>";
            }

            var valueProperty = value.GetType().GetProperty("Value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (valueProperty != null)
            {
                value = valueProperty.GetValue(value, null);
            }

            if (value == null)
            {
                return "<null>";
            }

            var bytes = value as byte[];
            if (bytes != null)
            {
                return "0x" + BitConverter.ToString(bytes).Replace("-", string.Empty);
            }

            var formattable = value as IFormattable;
            return formattable == null
                ? Convert.ToString(value, CultureInfo.InvariantCulture)
                : formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        private sealed class RecorderConfiguration
        {
            public uint Gap { get; set; }
            public uint DataLength { get; set; }
            public uint SignalBitMask { get; set; }
            public uint[] RecorderParams { get; set; }
            public uint[] SignalIds { get; set; }
        }
    }

    internal sealed class PmasSignalRow : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _rawValue;
        private string _entryStatus;
        private string _cycleCounter;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetField(ref _isSelected, value); }
        }

        public string Axis { get; set; }
        public ushort AxisReference { get; set; }
        public string Alias { get; set; }
        public ushort PiIndex { get; set; }
        public PIVarDirection DirectionValue { get; set; }
        public VAR_TYPE VarTypeValue { get; set; }
        public bool IsTypeSupported { get; set; }
        public string SignalIdHex { get; set; }
        public string DataType { get; set; }
        public string PdoAddress { get; set; }
        public string Direction { get; set; }

        public string RawValue
        {
            get { return _rawValue; }
            set { SetField(ref _rawValue, value); }
        }

        public string EntryStatus
        {
            get { return _entryStatus; }
            set { SetField(ref _entryStatus, value); }
        }

        public string CycleCounter
        {
            get { return _cycleCounter; }
            set { SetField(ref _cycleCounter, value); }
        }

        public string DisplayName
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} / PI {1} / {2}",
                    Axis,
                    PiIndex,
                    string.IsNullOrWhiteSpace(Alias) ? "<no alias>" : Alias);
            }
        }

        internal PI_BULKREAD_ENTRY BulkEntry { get; set; }
        internal bool IsBulkConfigured { get; set; }
        internal MMCSingleAxis AxisObject { get; set; }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    internal sealed class PmasBulkSnapshotRow
    {
        public string Alias { get; set; }
        public string SignalIdHex { get; set; }
        public string DataType { get; set; }
        public string RawValue { get; set; }
        public string EntryStatus { get; set; }
        public string Detail { get; set; }
    }

    internal sealed class PmasHealthRow
    {
        public string SlaveIndex { get; set; }
        public string PhysicalAxis { get; set; }
        public string Online { get; set; }
        public string EtherCATState { get; set; }
        public string ALStatusCode { get; set; }
        public string DS402StatusWord { get; set; }
        public string AxisError { get; set; }
        public string LastValidCycle { get; set; }
        public string LastStateChangeCycle { get; set; }
    }
}

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
        private const int RecorderManualCleanupTimeoutMilliseconds = 15000;
        private const int RecorderManualCleanupPollMilliseconds = 25;

        // D4 Double-bank exposure is deliberately split by operator route.
        // Keep every proof gate fail-closed until that exact route has matching
        // PLC build, recovery, and live qualification evidence.
        private static readonly bool RecorderDoubleManualActionsReady = false;
        private static readonly bool
            RecorderDoubleManualConfigureRouteReady = false;
        private static readonly bool
            RecorderDoubleQualificationExecutionReady = false;
        private static readonly bool
            RecorderDoubleReconnectRecoveryReady = false;

        private readonly List<DiagnosticSignalRow> diagnosticSignalRows =
            new List<DiagnosticSignalRow>();

        private LMCDiagnosticCapabilities diagnosticCapabilities;
        private LMCSignalCatalog diagnosticCatalog;
        private LMCBulkConfiguration bulkConfiguration;
        private bool bulkQualificationRecoveryPending;
        private LMCRecorderConfigurationHandle recorderConfiguration;
        private LMCRecorderIdentity recorderIdentity;
        private bool recorderQualificationRecoveryReleaseOnly;
        private bool recorderQualificationRecoveryStatusConfirmed;
        private LMCRecorderStatus recorderStatus;
        private LMCRecorderHeader recorderHeader;
        private LMCRecorderData recorderData;
        private CancellationTokenSource recorderDownloadCancellation;
        private LMCOperationTicket diagnosticOperationTicket;
        private LMCOperationTicket callbackDiagnosticRefreshTicket;
        private LMCOperationStatus diagnosticOperationStatus;
        private byte[] diagnosticOperationResult;
        private bool diagnosticOperationCancelAccepted;
        private CancellationTokenSource inlineSdoReadWaitCancellation;
        private bool updatingRecorderConfigurationOptions;
        private bool refreshingSdoWriteTargetSelection;
        private IReadOnlyList<LMCSdoWriteTarget> approvedSdoWriteTargets =
            Array.Empty<LMCSdoWriteTarget>();
        private SdoEditorDraftSnapshot pendingSdoEditorDraftSnapshot;
        private readonly SdoWriteConfirmationState sdoWriteConfirmationState =
            new SdoWriteConfirmationState();
        private SdoWriteActivationQualificationProof
            sdoWriteActivationQualificationProof;

        private sealed class SdoEditorDraftSnapshot
        {
            internal SdoEditorDraftSnapshot(
                LMCSdoWriteVerificationContext pendingReadback,
                LMCConnection ownerConnection,
                long ownerSessionGeneration,
                SdoOperationMode operation,
                LMCSdoWriteTarget writeTarget,
                string slaveReference,
                string objectIndex,
                string subIndex,
                LMCSignalValueType valueType,
                ushort dataLength,
                string timeoutCycles,
                string writeData)
            {
                PendingReadback = pendingReadback;
                OwnerConnection = ownerConnection;
                OwnerSessionGeneration = ownerSessionGeneration;
                Operation = operation;
                WriteTarget = writeTarget;
                SlaveReference = slaveReference;
                ObjectIndex = objectIndex;
                SubIndex = subIndex;
                ValueType = valueType;
                DataLength = dataLength;
                TimeoutCycles = timeoutCycles;
                WriteData = writeData;
            }

            internal LMCSdoWriteVerificationContext PendingReadback
            {
                get;
                private set;
            }

            internal LMCConnection OwnerConnection { get; private set; }
            internal long OwnerSessionGeneration { get; private set; }
            internal SdoOperationMode Operation { get; private set; }
            internal LMCSdoWriteTarget WriteTarget { get; private set; }
            internal string SlaveReference { get; private set; }
            internal string ObjectIndex { get; private set; }
            internal string SubIndex { get; private set; }
            internal LMCSignalValueType ValueType { get; private set; }
            internal ushort DataLength { get; private set; }
            internal string TimeoutCycles { get; private set; }
            internal string WriteData { get; private set; }
        }

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
                SdoOperationMode.Read,
                SdoOperationMode.Write
            };
            ComboSdoOperation.SelectedItem = SdoOperationMode.Read;
            ComboSdoWriteTarget.ItemsSource = approvedSdoWriteTargets;
            ComboD5SdoWriteQualificationTarget.ItemsSource =
                approvedSdoWriteTargets;
            ComboSdoDataLength.ItemsSource = new ushort[]
            {
                1,
                2,
                4
            };
            ComboSdoDataLength.SelectedItem = (ushort)4;
            ComboSdoValueType.ItemsSource = new[]
            {
                LMCSignalValueType.Bool,
                LMCSignalValueType.Int16,
                LMCSignalValueType.UInt16,
                LMCSignalValueType.Int32,
                LMCSignalValueType.UInt32,
                LMCSignalValueType.Real32,
                LMCSignalValueType.BitField16,
                LMCSignalValueType.BitField32,
                LMCSignalValueType.Int8,
                LMCSignalValueType.UInt8,
                LMCSignalValueType.BitField8
            };
            ComboSdoValueType.SelectedItem = LMCSignalValueType.UInt32;
            UpdateSdoOperationControls();
            UpdateRecorderEstimate();
            InitializeTopologyIoUi();
        }

        private async void ButtonDiagnosticsCapabilities_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh Diagnostics Capabilities",
                async () =>
                {
                    await RefreshDiagnosticsCapabilitiesAsync(
                        RequireConnection());
                });
        }

        private async Task RefreshDiagnosticsCapabilitiesAsync(
            LMCConnection currentConnection)
        {
            if (currentConnection == null)
            {
                throw new ArgumentNullException("currentConnection");
            }

            diagnosticCapabilities =
                await currentConnection.Diagnostics.GetCapabilitiesAsync(
                    CancellationToken.None);
            RefreshApprovedSdoWriteTargets(currentConnection);

            TextDiagnosticsCapabilities.Text =
                FormatCapabilities(diagnosticCapabilities);
            RefreshTopologyIoCapabilityState();
            UpdateRecorderBufferModeOptions();
            UpdateRecorderEstimate();
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

                    TextEtherCatHealthSummary.Text = FormatHealth(health)
                        + Environment.NewLine
                        + "0x7E10 reports fixed legacy drive slots 0..3 only. "
                        + "CFG slave is resolved from the loaded topology; "
                        + "CREVIS configuration and live capability state are shown in the separate topology table.";
                    GridEtherCatHealth.ItemsSource = health.Slaves
                        .Select(
                            value => new HealthSlaveRow(
                                value,
                                ResolveConfiguredSlaveIndex(
                                    value.PhysicalAxis)))
                        .ToArray();
                });
        }

        private string ResolveConfiguredSlaveIndex(ushort physicalAxis)
        {
            var topology = etherCATTopology;
            if (topology == null)
            {
                return "-";
            }

            var entry = topology.Entries.FirstOrDefault(
                value => value.PhysicalAxisReference == physicalAxis
                    && value.NodeKind
                        == LMCEtherCATTopologyNodeKind.EtherCATSlave);
            return entry == null || !entry.HasMasterSlaveIndex
                ? "-"
                : entry.MasterSlaveIndex.ToString(
                    CultureInfo.InvariantCulture);
        }

        private void RefreshLegacyHealthConfiguredSlaveIndices()
        {
            if (GridEtherCatHealth == null)
            {
                return;
            }

            var rows = (GridEtherCatHealth.ItemsSource
                    as IEnumerable<HealthSlaveRow>)
                ?.ToArray();
            if (rows == null || rows.Length == 0)
            {
                return;
            }

            foreach (var row in rows)
            {
                row.SetConfiguredSlaveIndex(
                    ResolveConfiguredSlaveIndex(row.PhysicalAxis));
            }

            GridEtherCatHealth.ItemsSource = null;
            GridEtherCatHealth.ItemsSource = rows;
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
                    EnsureNoUnresolvedDiagnosticMutation(
                        "Configure Bulk Snapshot");
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
                    bulkQualificationRecoveryPending = false;

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
                    bulkQualificationRecoveryPending = false;
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
                    EnsureNoUnresolvedDiagnosticMutation(
                        "Configure Recorder");
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
                        EnsureRecorderDoubleManualActionsReady();

                        EnsureCapability(
                            LMCDiagnosticCapability.RecorderDoubleBank,
                            "Recorder Double Bank");
                    }

                    try
                    {
                        await DispatchRecorderManualConfigureAsync(
                            configuration.RequiresDoubleBankCapability,
                            RecorderDoubleManualConfigureRouteReady,
                            async () =>
                            {
                                recorderConfiguration =
                                    await RequireConnection().Diagnostics
                                        .ConfigureRecorderAsync(
                                            configuration,
                                            CancellationToken.None);
                            },
                            () => ConfigureManualRecoverableDoubleRecorderAsync(
                                configuration));
                    }
                    catch (Exception error)
                    {
                        LMCRecorderAcceptedResultFailureContext recovery;
                        if (LMCRecorderAcceptedResultFailureContext.TryGet(
                                error,
                                out recovery)
                            && !configuration.RequiresDoubleBankCapability
                            && recovery.ConfigurationHandle != null)
                        {
                            PreserveRecorderQualificationRecovery(
                                recovery.ConfigurationHandle,
                                null,
                                "Manual Recorder Configure accepted result",
                                error);
                        }

                        throw;
                    }
                    recorderIdentity = null;
                    recorderQualificationRecoveryReleaseOnly = false;
                    recorderQualificationRecoveryStatusConfirmed = false;
                    recorderStatus = null;
                    ClearRecorderDownload();
                    if (!configuration.RequiresDoubleBankCapability)
                    {
                        TextRecorderSummary.Text = FormatRecorderConfiguration(
                            recorderConfiguration);
                    }
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
                    EnsureNoUnresolvedDiagnosticMutation(
                        "Start Recorder");
                    var configuration = RequireRecorderConfiguration();
                    try
                    {
                        recorderIdentity = await RequireConnection()
                            .Diagnostics.StartRecorderAsync(
                                configuration,
                                CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        LMCRecorderAcceptedResultFailureContext recovery;
                        if (LMCRecorderAcceptedResultFailureContext.TryGet(
                                error,
                                out recovery)
                            && recovery.Identity != null)
                        {
                            PreserveRecorderQualificationRecovery(
                                recovery.SourceConfigurationHandle
                                    ?? configuration,
                                recovery.Identity,
                                "Manual Recorder Start accepted result",
                                error);
                        }

                        throw;
                    }
                    recorderQualificationRecoveryReleaseOnly = false;
                    recorderQualificationRecoveryStatusConfirmed = false;
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
                    EnsureNoUnresolvedDiagnosticMutation(
                        "Adopt Recorder");
                    EnsureCapability(
                        LMCDiagnosticCapability.RecorderSingleBank,
                        "Recorder");
                    if (!RecorderDoubleReconnectRecoveryReady
                        && SupportsCapability(
                            LMCDiagnosticCapability.RecorderDoubleBank))
                    {
                        EnsureRecorderDoubleReconnectRecoveryReady();
                    }

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
                    try
                    {
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
                    }
                    catch (Exception error)
                    {
                        LMCRecorderAcceptedResultFailureContext recovery;
                        if (LMCRecorderAcceptedResultFailureContext.TryGet(
                                error,
                                out recovery)
                            && recovery.Identity != null)
                        {
                            PreserveUnvalidatedRecorderAdoption(
                                recovery.Identity,
                                "Manual Recorder Adopt accepted result",
                                error);
                        }

                        throw;
                    }
                    recorderQualificationRecoveryReleaseOnly = false;
                    recorderQualificationRecoveryStatusConfirmed = false;
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
                    EnsureNoUnresolvedDiagnosticMutation(
                        "Trigger Recorder");
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
                    if (recorderQualificationRecoveryReleaseOnly)
                    {
                        recorderQualificationRecoveryStatusConfirmed = true;
                        var cleanupAction = RecorderReconnectQualificationPolicy
                            .SelectCleanupAction(recorderStatus.State);
                        if (cleanupAction
                            == RecorderQualificationCleanupAction.StopAndRefresh)
                        {
                            TextRecorderSummary.Text += Environment.NewLine
                                + "Quarantined recovery identity Status confirmed. "
                                + "Release Recorder is enabled and will explicitly "
                                + "Stop, wait for Ready/Uploading, then release.";
                        }
                        else if (cleanupAction
                            == RecorderQualificationCleanupAction.Release)
                        {
                            TextRecorderSummary.Text += Environment.NewLine
                                + "Quarantined recovery identity Status confirmed in "
                                + "a releasable state. Release Recorder is enabled.";
                        }
                        else
                        {
                            TextRecorderSummary.Text += Environment.NewLine
                                + "Quarantined recovery remains mutation-blocked in State="
                                + recorderStatus.State
                                + ". Recover the PLC resource externally and refresh Status.";
                        }
                    }
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
                Filter = TranslateUiText(
                    "CSV files (*.csv)|*.csv|All files (*.*)|*.*"),
                FileName = "lasal-recorder-"
                    + DateTime.Now.ToString(
                        "yyyyMMdd-HHmmss",
                        CultureInfo.InvariantCulture)
                    + ".csv",
                OverwritePrompt = true,
                Title = TranslateUiText("Export LASAL Recorder CSV")
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
                ReleaseManualRecorderResourcesAsync);
        }

        private async Task ReleaseManualRecorderResourcesAsync()
        {
            var diagnostics = RequireConnection().Diagnostics;
            var identity = recorderIdentity;
            var configuration = recorderConfiguration;
            var bufferReleasePending = identity != null
                && !identity.IsBufferReleased;
            var configurationReleasePending = configuration != null
                ? !configuration.IsReleased
                : identity != null && !identity.IsRecorderReleased;
            if (!RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                recorderQualificationRecoveryReleaseOnly,
                recorderQualificationRecoveryStatusConfirmed,
                bufferReleasePending,
                configurationReleasePending,
                recorderStatus == null
                    ? (LMCRecorderState?)null
                    : recorderStatus.State))
            {
                throw new InvalidOperationException(
                    "Recorder recovery cleanup is blocked until Status confirms Armed, Recording, Ready, or Uploading. Config-only cleanup does not require Status.");
            }

            var statusValidatedThisAttempt = false;
            var operations = new RecorderQualificationCleanupOperations
            {
                ReadStatusAsync = async () =>
                {
                    recorderStatus = await diagnostics.GetRecorderStatusAsync(
                        identity,
                        CancellationToken.None);
                    return recorderStatus;
                },
                StopAsync = () => diagnostics.StopRecorderAsync(
                    identity,
                    CancellationToken.None),
                ValidateStatus = status =>
                {
                    AssertManualRecorderStatusIdentity(status, identity);
                    statusValidatedThisAttempt = true;
                },
                DelayAsync = milliseconds => Task.Delay(milliseconds),
                RecoveryRequired = status => recorderStatus = status,
                IsBufferReleasePending = () => identity != null
                    && !identity.IsBufferReleased,
                IsConfigurationReleasePending = () => configuration != null
                    ? !configuration.IsReleased
                    : identity != null && !identity.IsRecorderReleased,
                ReleaseBufferAsync = () => diagnostics.ReleaseRecorderBufferAsync(
                    identity,
                    CancellationToken.None),
                ReleaseConfigurationAsync = () => configuration != null
                    ? diagnostics.ReleaseRecorderAsync(
                        configuration,
                        CancellationToken.None)
                    : diagnostics.ReleaseRecorderAsync(
                        identity,
                        CancellationToken.None)
            };

            try
            {
                await RecorderQualificationCleanupOrchestrator
                    .CleanupOwnedResourcesAsync(
                        operations,
                        RecorderManualCleanupTimeoutMilliseconds,
                        RecorderManualCleanupPollMilliseconds);
            }
            catch (Exception error)
            {
                var bufferStillPending = identity != null
                    && !identity.IsBufferReleased;
                var configurationStillPending = configuration != null
                    ? !configuration.IsReleased
                    : identity != null && !identity.IsRecorderReleased;
                if (bufferStillPending || configurationStillPending)
                {
                    recorderQualificationRecoveryReleaseOnly = true;
                    recorderQualificationRecoveryStatusConfirmed =
                        bufferStillPending && statusValidatedThisAttempt;
                    TextRecorderSummary.Text =
                        "Recorder cleanup did not complete. Remaining ownership "
                        + "is quarantined for explicit retry; bufferPending="
                        + bufferStillPending
                        + ", configurationPending="
                        + configurationStillPending
                        + ", lastState="
                        + (recorderStatus == null
                            ? "unknown"
                            : recorderStatus.State.ToString())
                        + ". Error="
                        + error.Message;
                    UpdateUiState();
                }

                throw;
            }

            recorderIdentity = null;
            recorderConfiguration = null;
            recorderQualificationRecoveryReleaseOnly = false;
            recorderQualificationRecoveryStatusConfirmed = false;
            recorderStatus = null;
            TextRecorderSummary.Text = recorderData == null
                ? "Recorder resources released."
                : "Recorder PLC resources released. Downloaded PC data remains available for plot and CSV export.";
        }

        private static void AssertManualRecorderStatusIdentity(
            LMCRecorderStatus status,
            LMCRecorderIdentity identity)
        {
            if (status == null
                || status.Response == null
                || !status.Response.IsSuccess
                || identity == null
                || status.DiagnosticsBootId != identity.DiagnosticsBootId
                || status.RecordId != identity.RecordId
                || status.BufferId != identity.BufferId
                || status.ConfigId != identity.ConfigId
                || status.ConfigRevision != identity.ConfigRevision
                || status.MapRevision != identity.MapRevision
                || status.Capacity != identity.Capacity
                || status.OwnerSessionEpoch != identity.OwnerSessionEpoch)
            {
                throw new InvalidOperationException(
                    "Recorder manual cleanup Status does not match the adopted or locally owned identity.");
            }
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
            if (ComboSdoOperation.SelectedItem is SdoOperationMode operation
                && operation != SdoOperationMode.Write)
            {
                sdoWriteConfirmationState.Clear();
            }

            UpdateSdoOperationControls();
            if (ButtonConnect != null)
            {
                UpdateUiState();
            }
        }

        private void SdoValueType_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            InvalidateSdoWriteConfirmationAfterEditorChange();

            if (ComboSdoDataLength != null
                && ComboSdoValueType.SelectedItem is LMCSignalValueType valueType)
            {
                ComboSdoDataLength.SelectedItem = GetSdoReadDataLength(valueType);
            }
        }

        private void SdoWriteDataLength_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            InvalidateSdoWriteConfirmationAfterEditorChange();
        }

        private void SdoWriteRequestText_Changed(
            object sender,
            TextChangedEventArgs e)
        {
            InvalidateSdoWriteConfirmationAfterEditorChange();
        }

        private void InvalidateSdoWriteConfirmationAfterEditorChange()
        {
            sdoWriteConfirmationState.Clear();
            if (ButtonSubmitSdo != null
                && ComboSdoOperation != null
                && ComboSdoOperation.SelectedItem is SdoOperationMode mode
                && mode == SdoOperationMode.Write
                && !HasPendingD5SdoWriteReadback)
            {
                ButtonSubmitSdo.Content = "Arm SDO Write";
                UiLocalizationService.Apply(
                    ButtonSubmitSdo,
                    currentUiLanguage);
            }
        }

        private void SdoWriteTarget_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (refreshingSdoWriteTargetSelection)
            {
                return;
            }

            sdoWriteConfirmationState.Clear();
            ApplySelectedSdoWriteTarget();
            if (ButtonConnect != null)
            {
                UpdateUiState();
            }
        }

        private void ButtonLoadRequiredSdoReadback_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!HasPendingD5SdoWriteReadback)
            {
                return;
            }

            if (!CaptureSdoEditorDraftBeforeRequiredReadback(
                d5SdoPendingWriteReadback))
            {
                ShowPendingD5SdoWriteReadbackStatus();
                return;
            }

            ApplyPendingD5SdoWriteReadbackToUi();
            UpdateUiState();
        }

        private async void ButtonReadSdoInline_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (inlineSdoReadWaitCancellation != null)
            {
                WriteLog(
                    "Read SDO Inline ignored: another Inline wait already owns the PC-side cancellation source.");
                return;
            }

            LMCSdoRequest request;
            string validationMessage;
            if (!TryCreateInlineSdoReadRequest(
                out request,
                out validationMessage))
            {
                TextDiagnosticOperationSummary.Text =
                    "Not submitted: " + validationMessage;
                TextOperationState.Text =
                    "Read SDO Inline validation failed";
                WriteLog(
                    "Read SDO Inline not submitted: "
                    + validationMessage);
                return;
            }

            var cancellation = new CancellationTokenSource();
            inlineSdoReadWaitCancellation = cancellation;
            UpdateUiState();
            try
            {
                await RunOperationAsync(
                    "Read SDO Inline",
                    () => ReadSdoInlineFromUiAsync(
                        request,
                        cancellation.Token));
            }
            finally
            {
                if (ReferenceEquals(
                    inlineSdoReadWaitCancellation,
                    cancellation))
                {
                    inlineSdoReadWaitCancellation = null;
                }

                cancellation.Dispose();
                UpdateUiState();
            }
        }

        private void ButtonCancelSdoInlineWait_Click(
            object sender,
            RoutedEventArgs e)
        {
            var cancellation = inlineSdoReadWaitCancellation;
            if (cancellation == null
                || cancellation.IsCancellationRequested)
            {
                return;
            }

            cancellation.Cancel();
            TextOperationState.Text =
                "Read SDO Inline PC wait cancellation requested";
            TextDiagnosticOperationSummary.Text =
                "PC-side Inline Read wait cancellation requested. No CancelOperation was sent to the PLC and the SDO request will not be retried. If a ticket was already accepted, its last observed status and ticket will be preserved for manual cleanup.";
            WriteLog(
                "Read SDO Inline PC-side wait cancellation requested; no PLC CancelOperation or replay was sent.");
            UpdateUiState();
        }

        private async Task ReadSdoInlineFromUiAsync(
            LMCSdoRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.IsWrite
                || (request.DataLength != 1
                    && request.DataLength != 2
                    && request.DataLength != 4))
            {
                throw new NotSupportedException(
                    "Read SDO Inline accepts ordinary 1/2/4-byte Read requests only.");
            }

            if (HasPendingD5SdoWriteReadback)
            {
                throw new InvalidOperationException(
                    "Read SDO Inline is blocked while exact SDO Write readback is pending. Use the existing Submit/Refresh workflow for that readback.");
            }

            var currentOperationIsTerminal =
                diagnosticOperationStatus != null
                && diagnosticOperationStatus.IsTerminal;
            var operationSlotAvailable = diagnosticOperationTicket == null
                || currentOperationIsTerminal;
            var admission = EvaluateDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.TrackedD5ReadOnlyInspection,
                operationSlotAvailable);
            if (!admission.IsAllowed)
            {
                throw CreateDiagnosticsAdmissionException(
                    "Read SDO Inline",
                    admission);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var currentConnection = RequireConnection();
            const string operationStage = "manual-sdo-inline-read";
            var capabilities =
                await ReadExternalD5TrackingCapabilitiesAsync(
                    currentConnection,
                    operationStage,
                    request.DataLength,
                    RequiresGeneralInlineSdoRead(request));
            D5SdoQuarantineHandle submissionGuard;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                RequireManualSdoOperationCapabilities(
                    capabilities,
                    request);
                submissionGuard = ArmExternalD5SubmissionOutcomeGuard(
                    LMCOperationKind.SDORead,
                    request,
                    currentConnection,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    request.SlaveReference,
                    request.TimeoutCycles,
                    operationStage);
            }
            catch (Exception preSubmissionError)
            {
                var resolution = preSubmissionError
                        is OperationCanceledException
                    ? "NOT_SUBMITTED_CANCELLED"
                    : "NOT_SUBMITTED_PREFLIGHT_FAILED";
                WriteExternalD5TrackingLog(
                    "event=D5_EXTERNAL_NOT_SUBMITTED",
                    "stage=" + operationStage,
                    "operationKind=" + LMCOperationKind.SDORead,
                    "errorType="
                        + preSubmissionError.GetType().Name,
                    "error=" + QualificationValue(
                        preSubmissionError.Message),
                    "verdict=" + resolution);
                CloseExternalD5TrackingLogIfResolved(resolution);
                throw;
            }

            LMCSdoReadResult result;
            try
            {
                result = await currentConnection.Diagnostics
                    .ReadSdoInlineAsync(request, cancellationToken);
            }
            catch (Exception error)
            {
                LMCSdoSubmissionFailureContext failureContext;
                LMCSdoSubmissionFailureContext.TryGet(
                    error,
                    out failureContext);
                var terminalFailure = error
                    as LMCSdoReadOperationException;
                var lastObservedStatus =
                    GetInlineSdoReadObservedStatus(error);
                try
                {
                    if (terminalFailure != null)
                    {
                        TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                            submissionGuard,
                            terminalFailure.Ticket,
                            terminalFailure.Ticket.DiagnosticsBootId,
                            terminalFailure.Ticket.SubmissionMapRevision);
                        AdoptDiagnosticOperationTicket(
                            terminalFailure.Ticket);
                        diagnosticOperationStatus =
                            terminalFailure.OperationStatus;
                        diagnosticOperationResult = null;
                        DisarmExternalD5SubmissionOutcomeGuard(
                            submissionGuard,
                            "TERMINAL_OPERATION_FAILURE",
                            terminalFailure.OperationStatus.State
                                + "/"
                                + terminalFailure.OperationStatus.Outcome);
                    }
                    else
                    {
                        D5ExternalReadFailureOrchestrator
                            .RouteSubmissionFailure(
                                error,
                                (state, detail) =>
                                    DisarmExternalD5SubmissionOutcomeGuard(
                                        submissionGuard,
                                        state,
                                        detail),
                                (ticket, actualBootId, actualMapRevision) =>
                                {
                                    TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                                        submissionGuard,
                                        ticket,
                                        actualBootId,
                                        actualMapRevision);
                                    AdoptDiagnosticOperationTicket(ticket);
                                    diagnosticOperationStatus =
                                        lastObservedStatus;
                                    diagnosticOperationResult = null;
                                    PreserveExternalD5Ticket(
                                        ticket,
                                        request,
                                        currentConnection,
                                        request.SlaveReference,
                                        request.TimeoutCycles,
                                        actualMapRevision,
                                        operationStage);
                                },
                                (unresolvedError, unresolvedContext) =>
                                    PreserveExternalD5RawSubmissionOutcomeUncertain(
                                        submissionGuard,
                                        unresolvedError,
                                        unresolvedContext));
                    }
                }
                catch (Exception journalOrRoutingError)
                {
                    throw new InvalidOperationException(
                        "Inline SDO Read failed and durable outcome routing also failed. Do not submit the request again until the PLC state is resolved.",
                        new AggregateException(
                            error,
                            journalOrRoutingError));
                }

                TextDiagnosticOperationSummary.Text =
                    FormatInlineSdoReadFailure(
                        error,
                        failureContext,
                        lastObservedStatus);
                throw;
            }

            TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                submissionGuard,
                result.Ticket,
                result.Ticket.DiagnosticsBootId,
                result.Ticket.SubmissionMapRevision);
            AdoptDiagnosticOperationTicket(result.Ticket);
            diagnosticOperationStatus = result.Status;
            diagnosticOperationResult = result.ResultData;
            DisarmExternalD5SubmissionOutcomeGuard(
                submissionGuard,
                "TERMINAL_SUCCESS",
                result.Status.State + "/" + result.Status.Outcome);
            TextDiagnosticOperationSummary.Text =
                FormatInlineSdoReadSuccess(result);
        }

        private async void ButtonSubmitSdo_Click(
            object sender,
            RoutedEventArgs e)
        {
            LMCSdoRequest request;
            string validationMessage;
            if (!TryCreateSdoRequest(
                out request,
                out validationMessage))
            {
                TextDiagnosticOperationSummary.Text =
                    "Not submitted: " + validationMessage;
                TextOperationState.Text = "Submit SDO validation failed";
                WriteLog("Submit SDO not submitted: " + validationMessage);
                return;
            }

            var operationKind = request.IsWrite
                ? LMCOperationKind.SDOWrite
                : LMCOperationKind.SDORead;
            var isRequiredWriteReadback =
                HasPendingD5SdoWriteReadback
                && d5SdoPendingWriteReadback.MatchesReadRequest(request);
            var requiredWriteReadback = isRequiredWriteReadback
                ? d5SdoPendingWriteReadback
                : null;
            var operationName = isRequiredWriteReadback
                ? "Submit Required SDO Write Readback"
                : request.IsWrite
                    ? "Arm / Submit SDO Write"
                    : "Submit SDO Read";
            var operationStage = isRequiredWriteReadback
                ? "manual-sdo-write-readback-submit"
                : request.IsWrite
                    ? "manual-sdo-write-submit"
                    : "manual-sdo-read-submit";

            var confirmationArmed = false;
            await RunOperationAsync(
                operationName,
                async () =>
                {
                    var currentOperationIsTerminal =
                        diagnosticOperationStatus != null
                        && diagnosticOperationStatus.IsTerminal;
                    var operationSlotAvailable =
                        diagnosticOperationTicket == null
                        || currentOperationIsTerminal;
                    var admission = EvaluateDiagnosticsAdmission(
                        isRequiredWriteReadback
                            ? DiagnosticsAdmissionOperation
                                .RequiredExactSdoWriteReadback
                            : request.IsWrite
                                ? DiagnosticsAdmissionOperation
                                    .TrackedD5Submit
                                : DiagnosticsAdmissionOperation
                                    .TrackedD5ReadOnlyInspection,
                        operationSlotAvailable);
                    if (!admission.IsAllowed)
                    {
                        throw CreateDiagnosticsAdmissionException(
                            operationName,
                            admission);
                    }

                    var currentConnection = RequireConnection();
                    if (isRequiredWriteReadback
                        && !requiredWriteReadback
                            .MatchesOwnerCurrentSession(
                                currentConnection))
                    {
                        throw new InvalidOperationException(
                            "The required SDO Write readback belongs to another or stale connection session. No readback was submitted and the interlock remains active.");
                    }

                    var capabilities =
                        await ReadExternalD5TrackingCapabilitiesAsync(
                            currentConnection,
                            operationStage,
                            request.DataLength,
                            isRequiredWriteReadback
                                || RequiresGeneralInlineSdoRead(request));
                    D5SdoQuarantineHandle submissionGuard;
                    try
                    {
                        if (isRequiredWriteReadback
                            && !requiredWriteReadback
                                .MatchesCurrentIdentity(
                                    currentConnection,
                                    capabilities))
                        {
                            throw new InvalidOperationException(
                                "The current diagnostics BootId or MapRevision differs from the original SDO Write. No readback was submitted and the interlock remains active.");
                        }

                        RequireManualSdoOperationCapabilities(
                            capabilities,
                            request);
                        if (request.IsWrite)
                        {
                            await VerifyD5SdoQualificationSafeAxisAsync(
                                currentConnection,
                                request.SlaveReference,
                                "_LMCAxis"
                                    + request.SlaveReference.ToString(
                                        CultureInfo.InvariantCulture),
                                CancellationToken.None);
                            if (!sdoWriteConfirmationState.TryConsumeOrArm(
                                currentConnection,
                                currentConnection.SessionGeneration,
                                capabilities.DiagnosticsBootId,
                                capabilities.MapRevision,
                                request))
                            {
                                confirmationArmed = true;
                                TextDiagnosticOperationSummary.Text =
                                    FormatArmedSdoWriteConfirmation(request);
                                WriteLog(
                                    "SDO Write confirmation armed without submission. Edit fields freely; only a second click with the exact same immutable request and connection identity can submit it.");
                                WriteExternalD5TrackingLog(
                                    "event=D5_EXTERNAL_CONFIRMATION_ARMED",
                                    "stage=" + operationStage,
                                    "operationKind=" + operationKind,
                                    "verdict=NOT_SUBMITTED");
                                CloseExternalD5TrackingLogIfResolved(
                                    "CONFIRMATION_ARMED_NOT_SUBMITTED");
                                return;
                            }

                            await VerifyD5SdoQualificationSafeAxisAsync(
                                currentConnection,
                                request.SlaveReference,
                                "_LMCAxis"
                                    + request.SlaveReference.ToString(
                                        CultureInfo.InvariantCulture),
                                CancellationToken.None);
                        }

                        submissionGuard =
                            ArmExternalD5SubmissionOutcomeGuard(
                                operationKind,
                                request,
                                currentConnection,
                                capabilities.DiagnosticsBootId,
                                capabilities.MapRevision,
                                request.SlaveReference,
                                request.TimeoutCycles,
                                operationStage);
                    }
                    catch (Exception preSubmissionError)
                    {
                        var resolution = preSubmissionError
                                is OperationCanceledException
                            ? "NOT_SUBMITTED_CANCELLED"
                            : "NOT_SUBMITTED_PREFLIGHT_FAILED";
                        WriteExternalD5TrackingLog(
                            "event=D5_EXTERNAL_NOT_SUBMITTED",
                            "stage=" + operationStage,
                            "operationKind=" + operationKind,
                            "errorType="
                                + preSubmissionError.GetType().Name,
                            "error=" + QualificationValue(
                                preSubmissionError.Message),
                            "verdict=" + resolution);
                        CloseExternalD5TrackingLogIfResolved(resolution);
                        throw;
                    }
                    LMCOperationTicket submittedTicket;
                    try
                    {
                        submittedTicket = isRequiredWriteReadback
                            ? await requiredWriteReadback
                                .SubmitReadbackAsync(
                                     request,
                                     CancellationToken.None)
                            : await currentConnection.Diagnostics
                                    .SubmitSdoAsync(
                                        request,
                                        CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        if (request.IsWrite)
                        {
                            RetireSdoWriteActivationQualificationProof();
                        }

                        try
                        {
                            D5ExternalReadFailureOrchestrator
                                .RouteSubmissionFailure(
                                    error,
                                    (state, detail) =>
                                        DisarmExternalD5SubmissionOutcomeGuard(
                                            submissionGuard,
                                            state,
                                            detail),
                                    (ticket, actualBootId, actualMapRevision) =>
                                    {
                                        TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                                            submissionGuard,
                                            ticket,
                                            actualBootId,
                                            actualMapRevision);
                                        AdoptDiagnosticOperationTicket(ticket);
                                        PreserveExternalD5Ticket(
                                            ticket,
                                            request,
                                            currentConnection,
                                            request.SlaveReference,
                                            request.TimeoutCycles,
                                            actualMapRevision,
                                            operationStage);
                                    },
                                    (unresolvedError, failureContext) =>
                                        PreserveExternalD5RawSubmissionOutcomeUncertain(
                                            submissionGuard,
                                            unresolvedError,
                                            failureContext));
                        }
                        catch (Exception journalOrRoutingError)
                        {
                            throw new InvalidOperationException(
                                "SDO submission failed and durable outcome routing also failed. Treat the Write outcome as unverified and do not replay.",
                                new AggregateException(
                                    error,
                                    journalOrRoutingError));
                        }

                        throw;
                    }

                    TransitionExternalD5SubmissionOutcomeGuardToAccepted(
                        submissionGuard,
                        submittedTicket,
                        submittedTicket.DiagnosticsBootId,
                        submittedTicket.SubmissionMapRevision);
                    AdoptDiagnosticOperationTicket(submittedTicket);
                    PreserveExternalD5Ticket(
                        submittedTicket,
                        request,
                        currentConnection,
                        request.SlaveReference,
                        request.TimeoutCycles,
                        submittedTicket.SubmissionMapRevision,
                        operationStage);
                    DisarmExternalD5SubmissionOutcomeGuard(
                        submissionGuard,
                        "ACCEPTED_TICKET",
                        diagnosticOperationTicket.TicketId.ToString(
                            CultureInfo.InvariantCulture));
                    TextDiagnosticOperationSummary.Text = FormatOperationTicket(
                        diagnosticOperationTicket);
                });

            if (confirmationArmed)
            {
                TextOperationState.Text =
                    "SDO Write confirmation armed; no Write submitted";
                UpdateUiState();
            }
        }

        private void AdoptDiagnosticOperationTicket(
            LMCOperationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            diagnosticOperationTicket = ticket;
            diagnosticOperationStatus = null;
            diagnosticOperationResult = null;
            diagnosticOperationCancelAccepted = false;
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
                    var currentConnection = RequireConnection();
                    await RefreshDiagnosticOperationCoreAsync(
                        currentConnection,
                        ticket,
                        CancellationToken.None,
                        "manual-sdo-status");
                });
        }

        private async Task<bool> RefreshDiagnosticOperationCoreAsync(
            LMCConnection currentConnection,
            LMCOperationTicket ticket,
            CancellationToken cancellationToken,
            string source)
        {
            if (currentConnection == null)
            {
                throw new ArgumentNullException(nameof(currentConnection));
            }

            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            if (!ReferenceEquals(connection, currentConnection)
                || !ReferenceEquals(diagnosticOperationTicket, ticket)
                || !ticket.BelongsToCurrentSession(currentConnection))
            {
                throw new InvalidOperationException(
                    "The retained D5 operation ticket does not belong to the current connection session.");
            }

            var pendingWriteReadback = d5SdoPendingWriteReadback;
            if (pendingWriteReadback != null
                && ticket.OperationKind == LMCOperationKind.SDORead
                && !pendingWriteReadback.MatchesOwnerCurrentSession(
                    currentConnection))
            {
                throw new InvalidOperationException(
                    "The pending SDO Write readback belongs to another or stale connection session. Its ticket was not queried and the interlock remains active.");
            }

            var operationStatus = await currentConnection.Diagnostics
                .GetOperationStatusAsync(ticket, cancellationToken);
            if (!ReferenceEquals(connection, currentConnection)
                || !ReferenceEquals(diagnosticOperationTicket, ticket)
                || !ticket.BelongsToCurrentSession(currentConnection))
            {
                return false;
            }

            LMCDiagnosticCapabilities readbackTerminalCapabilities = null;
            if (pendingWriteReadback != null
                && ticket.OperationKind == LMCOperationKind.SDORead
                && operationStatus.IsTerminal)
            {
                readbackTerminalCapabilities =
                    await ReadExternalD5TrackingCapabilitiesAsync(
                        currentConnection,
                        source + "-write-readback-terminal",
                        pendingWriteReadback.DataLength,
                        true,
                        ticket);
                if (!ReferenceEquals(connection, currentConnection)
                    || !ReferenceEquals(diagnosticOperationTicket, ticket)
                    || !ticket.BelongsToCurrentSession(currentConnection))
                {
                    return false;
                }
            }

            diagnosticOperationStatus = operationStatus;
            var completedWriteRequest = ticket.OperationKind
                    == LMCOperationKind.SDOWrite
                ? d5SdoQualificationActiveRequest
                : null;
            var hadPendingWriteReadback = HasPendingD5SdoWriteReadback;
            CompleteExternalD5TicketIfTerminal(
                ticket,
                operationStatus,
                source,
                currentConnection,
                readbackTerminalCapabilities);
            var digitalOutputWriteReadbackSummary =
                await VerifyDigitalOutputWriteReadbackAsync(
                    currentConnection,
                    ticket,
                    operationStatus,
                    cancellationToken);
            if (!ReferenceEquals(connection, currentConnection)
                || !ticket.BelongsToCurrentSession(currentConnection))
            {
                return false;
            }

            if (!ReferenceEquals(ticket, diagnosticOperationTicket))
            {
                return true;
            }

            if (operationStatus.IsSuccessful
                && ticket.OperationKind == LMCOperationKind.SDORead)
            {
                if (!ticket.UsesExtendedResultChunks)
                {
                    diagnosticOperationResult = operationStatus.ResultData;
                }
            }
            else
            {
                diagnosticOperationResult = null;
            }

            TextDiagnosticOperationSummary.Text = FormatOperationStatus(
                operationStatus)
                + (operationStatus.IsSuccessful
                    && ticket.OperationKind == LMCOperationKind.SDOWrite
                    ? FormatSdoWriteManualReadbackWarning(
                        completedWriteRequest)
                    : string.Empty)
                + digitalOutputWriteReadbackSummary
                + (hadPendingWriteReadback
                    && ticket.OperationKind == LMCOperationKind.SDORead
                    && operationStatus.IsTerminal
                    ? HasPendingD5SdoWriteReadback
                        ? Environment.NewLine
                            + "Exact SDO Write readback NOT VERIFIED; the mutation/Close interlock remains active. Retry only the auto-filled exact Read."
                        : Environment.NewLine
                            + "Exact SDO Write target readback VERIFIED; the mutation/Close interlock is cleared."
                    : string.Empty)
                + (operationStatus.IsSuccessful
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
            return true;
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
                Filter = TranslateUiText(
                    "Binary files (*.bin)|*.bin|All files (*.*)|*.*"),
                FileName = "lasal-sdo-result-"
                    + DateTime.Now.ToString(
                        "yyyyMMdd-HHmmss",
                        CultureInfo.InvariantCulture)
                    + ".bin",
                OverwritePrompt = true,
                Title = TranslateUiText("Save LASAL SDO Result")
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
                        && diagnosticOperationStatus.State
                            != LMCOperationState.Queued)
                    {
                        throw new InvalidOperationException(
                            "Only a queued diagnostics operation can be cancelled.");
                    }

                    try
                    {
                        await RequireConnection().Diagnostics
                            .CancelOperationAsync(
                                ticket,
                                CancellationToken.None);
                    }
                    catch (Exception error)
                    {
                        if (ticket.OperationKind
                                == LMCOperationKind.SDOWrite
                            && ReferenceEquals(
                                d5SdoQualificationActiveTicket,
                                ticket))
                        {
                            var commandError = error
                                as LMCDiagnosticsCommandException;
                            var reason = commandError != null
                                && commandError.Response != null
                                && commandError.Response.Detail
                                    == LMCDiagnosticsDetailCode.InvalidState
                                    ? "write_cancel_rejected_invalid_state"
                                    : "write_cancel_outcome_unverified";
                            QuarantineStaleSessionD5SdoQualificationTicket(
                                reason);
                        }

                        throw;
                    }

                    diagnosticOperationCancelAccepted = true;
                    if (ticket.OperationKind == LMCOperationKind.SDOWrite
                        && ReferenceEquals(
                            d5SdoQualificationActiveTicket,
                            ticket))
                    {
                        try
                        {
                            WriteExternalD5TrackingLog(
                                "event=D5_EXTERNAL_CANCEL",
                                "ticket=" + ticket.TicketId.ToString(
                                    CultureInfo.InvariantCulture),
                                "operationKind=" + ticket.OperationKind,
                                "observedStateBeforeCancel="
                                    + (diagnosticOperationStatus == null
                                        ? "UNKNOWN"
                                        : diagnosticOperationStatus.State
                                            .ToString()),
                                "result=ACCEPTED_QUEUED_CANCEL",
                                "plcStopCommand=false");
                        }
                        catch
                        {
                            QuarantineStaleSessionD5SdoQualificationTicket(
                                "write_cancel_acceptance_log_unavailable");
                            throw;
                        }
                    }

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
                    if (!Phase1AllowsPiWrite)
                    {
                        throw new NotSupportedException(
                            "PI Write is disabled in Phase 1 because a lost submit response cannot be reconciled safely by the read-only D5 recovery proof.");
                    }

                    EnsureNoUnresolvedDiagnosticMutation(
                        "Manual Submit PI Write");
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
                    diagnosticOperationCancelAccepted = false;
                    TextDiagnosticOperationSummary.Text = FormatOperationTicket(
                        diagnosticOperationTicket);
                });
        }

        private void UpdateDiagnosticsUiState(
            LMCConnection currentConnection,
            bool connected,
            bool idle)
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
            var supportsRecorderDoubleAdvertised = SupportsCapability(
                LMCDiagnosticCapability.RecorderDoubleBank);
            var supportsRecorderDoubleManual =
                RecorderDoubleManualActionsReady
                && RecorderDoubleManualConfigureRouteReady
                && supportsRecorderDoubleAdvertised;
            var supportsPiWrite = SupportsCapability(
                LMCDiagnosticCapability.PIWrite);
            var supportsSdoRead = SupportsSdoRead();
            var supportsGeneralSdoRead = SupportsGeneralInlineSdoRead();
            var supportsSdoWrite = SupportsSdoWrite();
            var diagnosticMutationCommandInterlocked =
                HasDiagnosticsMutationCommandInterlock;
            var hasCatalog = diagnosticCatalog != null;
            var hasBulk = bulkConfiguration != null
                && !bulkConfiguration.IsReleased;
            var hasRecorderConfiguration = recorderConfiguration != null
                && !recorderConfiguration.IsReleased;
            var hasRecorderIdentity = recorderIdentity != null
                && !recorderIdentity.IsRecorderReleased;
            var recorderBufferReleasePending = hasRecorderIdentity
                && !recorderIdentity.IsBufferReleased;
            var recorderConfigurationReleasePending =
                hasRecorderConfiguration
                || (hasRecorderIdentity && recorderConfiguration == null);
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
                    || supportsRecorderDoubleManual)
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
                && !diagnosticMutationCommandInterlocked
                && supportsBulk
                && hasCatalog
                && !hasBulk;
            ButtonReadBulkStatus.IsEnabled = connected
                && idle
                && hasBulk
                && !bulkQualificationRecoveryPending;
            ButtonReadBulkSnapshot.IsEnabled = connected
                && idle
                && hasBulk
                && !bulkQualificationRecoveryPending;
            ButtonReleaseBulk.IsEnabled = connected && idle && hasBulk;

            var recorderInputsEnabled = idle
                && !diagnosticMutationCommandInterlocked
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
                && !diagnosticMutationCommandInterlocked
                && supportsRecorder
                && hasCatalog
                && !hasRecorderConfiguration
                && !hasRecorderIdentity
                && recorderOptionSupported;
            var recorderAdoptionInputsEnabled = idle
                && !diagnosticMutationCommandInterlocked
                && !hasRecorderConfiguration
                && !hasRecorderIdentity;
            TextRecorderAdoptBootId.IsEnabled = recorderAdoptionInputsEnabled;
            TextRecorderAdoptRecordId.IsEnabled = recorderAdoptionInputsEnabled;
            TextRecorderAdoptBufferId.IsEnabled = recorderAdoptionInputsEnabled;
            ButtonAdoptRecorder.IsEnabled = connected
                && idle
                && !diagnosticMutationCommandInterlocked
                && supportsRecorder
                && !hasRecorderConfiguration
                && !hasRecorderIdentity
                && (!supportsRecorderDoubleAdvertised
                    || RecorderDoubleReconnectRecoveryReady);
            ButtonStartRecorder.IsEnabled = connected
                && idle
                && !diagnosticMutationCommandInterlocked
                && hasRecorderConfiguration
                && !hasRecorderIdentity
                && !recorderQualificationRecoveryReleaseOnly;
            ButtonStopRecorder.IsEnabled = connected
                && idle
                && hasRecorderIdentity
                && !recorderQualificationRecoveryReleaseOnly
                && !recorderIdentity.IsBufferReleased
                && recorderCanStop;
            ButtonTriggerRecorder.IsEnabled = connected
                && idle
                && !diagnosticMutationCommandInterlocked
                && supportsRecorderTrigger
                && hasRecorderIdentity
                && !recorderQualificationRecoveryReleaseOnly
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
                && !recorderQualificationRecoveryReleaseOnly
                && !recorderIdentity.IsBufferReleased;
            ButtonDownloadRecorder.IsEnabled = connected
                && idle
                && hasRecorderIdentity
                && !recorderQualificationRecoveryReleaseOnly
                && ((recorderStatus != null && recorderStatus.IsFrozen)
                    || recorderHeader != null)
                && !downloadRunning;
            ButtonCancelRecorderDownload.IsEnabled = downloadRunning;
            ButtonExportRecorderCsv.IsEnabled = idle
                && recorderData != null;
            ButtonReleaseRecorder.IsEnabled = connected
                && idle
                && RecorderReconnectQualificationPolicy.CanRunManualCleanup(
                    recorderQualificationRecoveryReleaseOnly,
                    recorderQualificationRecoveryStatusConfirmed,
                    recorderBufferReleasePending,
                    recorderConfigurationReleasePending,
                    recorderStatus == null
                        ? (LMCRecorderState?)null
                        : recorderStatus.State);

            var operationIsTerminal = diagnosticOperationStatus != null
                && diagnosticOperationStatus.IsTerminal;
            var operationSlotAvailable = diagnosticOperationTicket == null
                || operationIsTerminal;
            var canSubmitMutationOperation = EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation.TrackedD5Submit,
                    operationSlotAvailable)
                .IsAllowed;
            var canSubmitReadOnlyOperation = EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation
                        .TrackedD5ReadOnlyInspection,
                    operationSlotAvailable)
                .IsAllowed;
            var requiredReadbackSubmissionAvailable =
                EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation
                        .RequiredExactSdoWriteReadback,
                    operationSlotAvailable)
                .IsAllowed;
            // The click handler snapshots and validates the request before the
            // asynchronous submit starts, so later edits cannot mutate the
            // in-flight request. Keep the editor available while that request
            // is running and while exact D5 write readback remains pending.
            var sdoInputsEnabled =
                SdoEditorAvailabilityPolicy.CanEditRequest(
                    operationRunning,
                    HasPendingD5SdoWriteReadback);
            if (HasPendingD5SdoWriteReadback)
            {
                ShowPendingD5SdoWriteReadbackStatus();
            }

            var sdoOperation = ComboSdoOperation.SelectedItem
                is SdoOperationMode
                    ? (SdoOperationMode)ComboSdoOperation.SelectedItem
                    : SdoOperationMode.Read;
            var isSdoWrite = sdoOperation == SdoOperationMode.Write;
            var canSubmitSdoOperation = (isSdoWrite
                    ? canSubmitMutationOperation
                    : canSubmitReadOnlyOperation)
                || requiredReadbackSubmissionAvailable;
            if (!isSdoWrite
                && supportsSdoRead
                && !supportsGeneralSdoRead
                && canSubmitReadOnlyOperation)
            {
                TextSdoIndex.Text = "0x1000";
                TextSdoSubIndex.Text = "0";
                ComboSdoValueType.SelectedItem = LMCSignalValueType.UInt32;
                ComboSdoDataLength.SelectedItem = (ushort)4;
            }

            // Operation and target selection only edit a local draft. Keep
            // them independent from the current PLC capability observation;
            // the Submit/Inline gates below remain capability- and
            // admission-controlled.
            ComboSdoOperation.IsEnabled = sdoInputsEnabled;
            ComboSdoWriteTarget.IsEnabled = sdoInputsEnabled
                && isSdoWrite
                && approvedSdoWriteTargets.Count != 0;
            TextSdoSlaveReference.IsEnabled = sdoInputsEnabled;
            TextSdoIndex.IsEnabled = sdoInputsEnabled
                && (HasPendingD5SdoWriteReadback
                    || isSdoWrite
                    || supportsGeneralSdoRead);
            TextSdoSubIndex.IsEnabled = sdoInputsEnabled
                && (HasPendingD5SdoWriteReadback
                    || isSdoWrite
                    || supportsGeneralSdoRead);
            ComboSdoValueType.IsEnabled = sdoInputsEnabled
                && (HasPendingD5SdoWriteReadback
                    || isSdoWrite
                    || supportsGeneralSdoRead);
            ComboSdoDataLength.IsEnabled = sdoInputsEnabled
                && (HasPendingD5SdoWriteReadback
                    || isSdoWrite
                    || supportsGeneralSdoRead);
            TextSdoTimeoutCycles.IsEnabled = sdoInputsEnabled;
            TextSdoWriteData.IsEnabled = sdoInputsEnabled
                && isSdoWrite;
            ButtonLoadRequiredSdoReadback.IsEnabled =
                requiredReadbackSubmissionAvailable;
            ButtonSubmitSdo.Content = HasPendingD5SdoWriteReadback
                ? requiredReadbackSubmissionAvailable
                    ? "Submit Required Exact Readback"
                    : "Readback Session Mismatch"
                : isSdoWrite
                    ? sdoWriteConfirmationState.IsArmed
                            ? "Confirm & Submit SDO Write"
                            : "Arm SDO Write"
                    : "Submit SDO Read";
            ButtonSubmitSdo.ToolTip = isSdoWrite
                && !HasPendingD5SdoWriteReadback
                    ? "Write Once uses an exact-request two-click confirmation, safe-axis preflight, durable no-replay journal, and mandatory exact readback. Known targets are optional presets."
                    : "Read mode submits one tracked SDO Read.";
            ButtonSubmitSdo.IsEnabled = connected
                && idle
                && canSubmitSdoOperation
                && (HasPendingD5SdoWriteReadback
                    ? requiredReadbackSubmissionAvailable
                        && supportsGeneralSdoRead
                        && !isSdoWrite
                    : isSdoWrite
                    ? supportsSdoWrite
                        && DiagnosticsMutationJournalCanArm
                    : supportsSdoRead);
            var inlineReadLength = ComboSdoDataLength.SelectedItem
                is ushort
                    ? (ushort)ComboSdoDataLength.SelectedItem
                    : (ushort)0;
            ButtonReadSdoInline.IsEnabled = connected
                && idle
                && inlineSdoReadWaitCancellation == null
                && canSubmitReadOnlyOperation
                && supportsSdoRead
                && !isSdoWrite
                && !HasPendingD5SdoWriteReadback
                && (inlineReadLength == 1
                    || inlineReadLength == 2
                    || inlineReadLength == 4);
            ButtonCancelSdoInlineWait.IsEnabled =
                inlineSdoReadWaitCancellation != null
                && !inlineSdoReadWaitCancellation.IsCancellationRequested;
            ButtonRefreshDiagnosticOperation.IsEnabled = connected
                && idle
                && diagnosticOperationTicket != null;
            ButtonCancelDiagnosticOperation.IsEnabled = connected
                && idle
                && diagnosticOperationTicket != null
                && !diagnosticOperationCancelAccepted
                && (diagnosticOperationStatus == null
                    || diagnosticOperationStatus.State
                        == LMCOperationState.Queued);
            ButtonDownloadSdoResult.IsEnabled = connected
                && idle
                && diagnosticOperationTicket != null
                && diagnosticOperationTicket.UsesExtendedResultChunks
                && diagnosticOperationStatus != null
                && diagnosticOperationStatus.IsSuccessful
                && diagnosticOperationResult == null;
            ButtonExportSdoResult.IsEnabled = idle
                && diagnosticOperationResult != null;
            TextPiWriteRawValue.IsEnabled = idle && Phase1AllowsPiWrite;
            ButtonSubmitPiWrite.IsEnabled = connected
                && idle
                && Phase1AllowsPiWrite
                && supportsPiWrite
                && hasCatalog
                && canSubmitMutationOperation
                && !AnyDiagnosticsMutationJournalUnavailable;
            UpdateDiagnosticsMutationJournalUiState(idle);
            UpdateTopologyIoUiState(currentConnection, connected, idle);
        }

        private void ClearDiagnosticsState()
        {
            ResetD5SdoWriteSameValueOperatorConfirmations();
            RetireSdoWriteActivationQualificationProof();
            ClearRecorderDoubleVolatileSessionState();
            var cancellation = recorderDownloadCancellation;
            recorderDownloadCancellation = null;
            cancellation?.Cancel();
            cancellation?.Dispose();

            var inlineWaitCancellation =
                inlineSdoReadWaitCancellation;
            if (inlineWaitCancellation != null
                && !inlineWaitCancellation.IsCancellationRequested)
            {
                inlineWaitCancellation.Cancel();
            }

            diagnosticCapabilities = null;
            diagnosticCatalog = null;
            diagnosticSignalRows.Clear();
            bulkConfiguration = null;
            bulkQualificationRecoveryPending = false;
            recorderConfiguration = null;
            recorderIdentity = null;
            recorderQualificationRecoveryReleaseOnly = false;
            recorderQualificationRecoveryStatusConfirmed = false;
            recorderStatus = null;
            recorderHeader = null;
            recorderData = null;
            diagnosticOperationTicket = null;
            if (callbackDiagnosticRefreshTicket != null)
            {
                operationRunning = false;
            }

            callbackDiagnosticRefreshTicket = null;
            diagnosticOperationStatus = null;
            diagnosticOperationResult = null;
            diagnosticOperationCancelAccepted = false;
            approvedSdoWriteTargets = Array.Empty<LMCSdoWriteTarget>();
            ClearTopologyIoState();
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
                ComboSdoWriteTarget.ItemsSource = approvedSdoWriteTargets;
                ComboD5SdoWriteQualificationTarget.ItemsSource =
                    approvedSdoWriteTargets;
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
                    "SDO Read and Generic Write support exact 1/2/4-byte typed values. Known targets are optional presets. Semantic motion objects remain blocked; Write Once requires two-click confirmation, durable no-replay journal, and exact readback.";
                if (HasPendingD5SdoWriteReadback)
                {
                    ShowPendingD5SdoWriteReadbackStatus();
                }
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

        private bool SupportsSdoRead()
        {
            return diagnosticCapabilities != null
                && diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.SDORead)
                && diagnosticCapabilities.DiagnosticsBootId != 0
                && diagnosticCapabilities.MapRevision != 0
                && diagnosticCapabilities.MaxSdoDataBytes >= 4
                && diagnosticCapabilities.MaxRequestPayloadBytes >= 32
                && diagnosticCapabilities.MaxResponsePayloadBytes >= 64;
        }

        private bool SupportsGeneralInlineSdoRead()
        {
            return SupportsSdoRead()
                && diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline)
                && diagnosticCapabilities.MaxSdoDataBytes == 4;
        }

        private bool SupportsSdoWrite()
        {
            var evaluation = EvaluateCachedSdoWritePolicy();
            return evaluation != null
                && evaluation.CanAttemptSubmission;
        }

        private LMCSdoWritePolicyEvaluation EvaluateCachedSdoWritePolicy()
        {
            var currentConnection = connection;
            return currentConnection == null
                ? null
                : currentConnection.Diagnostics.EvaluateSdoWritePolicy(
                    diagnosticCapabilities);
        }

        private void RefreshApprovedSdoWriteTargets(
            LMCConnection currentConnection)
        {
            if (currentConnection == null)
            {
                throw new ArgumentNullException("currentConnection");
            }

            RefreshSdoWriteTargetItems(
                currentConnection.Diagnostics.GetApprovedSdoWriteTargets());
        }

        private void RefreshSdoWriteTargetItems(
            IReadOnlyList<LMCSdoWriteTarget> refreshedTargets)
        {
            var previousTarget = ComboSdoWriteTarget.SelectedItem
                as LMCSdoWriteTarget;
            var previousQualificationTarget =
                ComboD5SdoWriteQualificationTarget.SelectedItem
                    as LMCSdoWriteTarget;
            approvedSdoWriteTargets = refreshedTargets
                ?? Array.Empty<LMCSdoWriteTarget>();
            var retainedTarget = FindEquivalentSdoWriteTarget(
                approvedSdoWriteTargets,
                previousTarget);
            var nextTarget = retainedTarget
                ?? (approvedSdoWriteTargets.Count == 0
                    ? null
                    : approvedSdoWriteTargets[0]);
            var retainedQualificationTarget = FindEquivalentSdoWriteTarget(
                approvedSdoWriteTargets,
                previousQualificationTarget);
            var nextQualificationTarget = retainedQualificationTarget
                ?? (approvedSdoWriteTargets.Count == 1
                    ? approvedSdoWriteTargets[0]
                    : null);

            // Rebinding can raise SelectionChanged twice (old -> null, then
            // null -> retained/new). Capability refresh is not an operator
            // target choice, so neither event may overwrite the draft.
            refreshingSdoWriteTargetSelection = true;
            try
            {
                ComboSdoWriteTarget.ItemsSource = approvedSdoWriteTargets;
                ComboSdoWriteTarget.SelectedItem = nextTarget;
                ComboD5SdoWriteQualificationTarget.ItemsSource =
                    approvedSdoWriteTargets;
                ComboD5SdoWriteQualificationTarget.SelectedItem =
                    nextQualificationTarget;
            }
            finally
            {
                refreshingSdoWriteTargetSelection = false;
            }
        }

        private static LMCSdoWriteTarget FindEquivalentSdoWriteTarget(
            IReadOnlyList<LMCSdoWriteTarget> targets,
            LMCSdoWriteTarget previousTarget)
        {
            if (targets == null || previousTarget == null)
            {
                return null;
            }

            foreach (var target in targets)
            {
                if (ReferenceEquals(target, previousTarget)
                    || (target != null
                        && target.SlaveReference
                            == previousTarget.SlaveReference
                        && target.ObjectIndex == previousTarget.ObjectIndex
                        && target.SubIndex == previousTarget.SubIndex
                        && target.ValueType == previousTarget.ValueType
                        && target.DataLength == previousTarget.DataLength
                        && target.MinimumIntegerValue
                            == previousTarget.MinimumIntegerValue
                        && target.MaximumIntegerValue
                            == previousTarget.MaximumIntegerValue))
                {
                    return target;
                }
            }

            return null;
        }

        private bool HasCurrentSdoWriteActivationQualificationProof(
            LMCConnection currentConnection,
            LMCDiagnosticCapabilities capabilities,
            LMCSdoWriteTarget target)
        {
            var proof = sdoWriteActivationQualificationProof;
            if (proof == null)
            {
                return false;
            }

            if (proof.MatchesCurrent(
                currentConnection,
                capabilities,
                target))
            {
                return true;
            }

            RetireSdoWriteActivationQualificationProof();
            return false;
        }

        private void RetireSdoWriteActivationQualificationProof()
        {
            var proof = sdoWriteActivationQualificationProof;
            sdoWriteActivationQualificationProof = null;
            if (proof != null)
            {
                proof.Revoke();
            }

            sdoWriteConfirmationState.Clear();
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

            if (RecorderDoubleManualActionsReady
                && RecorderDoubleManualConfigureRouteReady
                && SupportsCapability(
                    LMCDiagnosticCapability.RecorderDoubleBank))
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

        private static void EnsureRecorderDoubleManualActionsReady()
        {
            if (!RecorderDoubleManualActionsReady)
            {
                throw new InvalidOperationException(
                    "Double-bank manual Recorder actions are blocked: "
                    + "ManualActions proof gate is CLOSED. "
                    + "PLC build/RAM/jitter and live manual lifecycle evidence are required.");
            }

            if (!RecorderDoubleManualConfigureRouteReady)
            {
                throw new InvalidOperationException(
                    "Double-bank manual Recorder Configure is blocked: "
                    + "the durable recoverable Configure route is CLOSED. "
                    + "The ordinary Configure route cannot accept Double mode.");
            }
        }

        internal static async Task DispatchRecorderManualConfigureAsync(
            bool requiresDoubleBank,
            bool recoverableDoubleRouteReady,
            Func<Task> configureStandardAsync,
            Func<Task> configureRecoverableDoubleAsync)
        {
            if (configureStandardAsync == null)
            {
                throw new ArgumentNullException("configureStandardAsync");
            }

            if (configureRecoverableDoubleAsync == null)
            {
                throw new ArgumentNullException(
                    "configureRecoverableDoubleAsync");
            }

            if (!requiresDoubleBank)
            {
                await configureStandardAsync();
                return;
            }

            if (!recoverableDoubleRouteReady)
            {
                throw new InvalidOperationException(
                    "Double-bank manual Recorder Configure is blocked: "
                    + "the durable recoverable Configure route is CLOSED. "
                    + "No standard or recoverable Configure request was sent.");
            }

            await configureRecoverableDoubleAsync();
        }

        private Task
            ConfigureManualRecoverableDoubleRecorderAsync(
                LMCRecorderConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return ConfigureManualRecoverableDoubleRecorderCoreAsync(
                configuration);
        }

        private static void EnsureRecorderDoubleReconnectRecoveryReady()
        {
            if (RecorderDoubleReconnectRecoveryReady)
            {
                return;
            }

            throw new InvalidOperationException(
                "Double-bank Recorder adoption/recovery is blocked: "
                + "ReconnectRecovery proof gate is CLOSED. "
                + "Exact external-session-loss inventory/adopt/reset evidence is required before wire.");
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
            if (ComboSdoOperation == null
                || ComboSdoWriteTarget == null
                || TextSdoWriteData == null)
            {
                return;
            }

            if (HasPendingD5SdoWriteReadback)
            {
                ShowPendingD5SdoWriteReadbackStatus();
                return;
            }

            var mode = ComboSdoOperation.SelectedItem is SdoOperationMode
                ? (SdoOperationMode)ComboSdoOperation.SelectedItem
                : SdoOperationMode.Read;
            if (mode == SdoOperationMode.Write)
            {
                ButtonSubmitSdo.Content = sdoWriteConfirmationState.IsArmed
                        ? "Confirm & Submit SDO Write"
                        : "Arm SDO Write";
                TextDiagnosticOperationSummary.Text =
                    "Generic scalar SDO Write accepts direct Slave/Object/SubIndex/Type/Length/Value input. Known targets are optional presets. Semantic motion objects remain blocked; Write Once requires PLC bits 8/9/13, PowerOn=False, Standstill=True, stable position, exact two-click confirmation, durable journal, and exact readback.";
            }
            else
            {
                ButtonSubmitSdo.Content = "Submit SDO Read";
                TextDiagnosticOperationSummary.Text =
                    "SDO Read supports exact 1/2/4-byte typed values. Read SDO Inline waits for and displays the terminal typed/raw result in one action; Submit/Refresh remains available for low-level ticket diagnostics. Bit 13 enables editable nonzero object index and sub-index; a bit-8-only PLC uses fixed 0x1000:0 UInt32/4.";
            }
        }

        private void ApplySelectedSdoWriteTarget()
        {
            if (ComboSdoOperation == null
                || ComboSdoWriteTarget == null
                || !(ComboSdoOperation.SelectedItem is SdoOperationMode)
                || (SdoOperationMode)ComboSdoOperation.SelectedItem
                    != SdoOperationMode.Write
                || !(ComboSdoWriteTarget.SelectedItem
                    is LMCSdoWriteTarget target))
            {
                return;
            }

            TextSdoSlaveReference.Text = target.SlaveReference.ToString(
                CultureInfo.InvariantCulture);
            TextSdoIndex.Text = "0x" + target.ObjectIndex.ToString("X4");
            TextSdoSubIndex.Text = target.SubIndex.ToString(
                CultureInfo.InvariantCulture);
            ComboSdoValueType.SelectedItem = target.ValueType;
            ComboSdoDataLength.SelectedItem = target.DataLength;
        }

        private void ApplyPendingD5SdoWriteReadbackToUi()
        {
            var requirement = d5SdoPendingWriteReadback;
            if (requirement == null
                || ComboSdoOperation == null
                || TextSdoSlaveReference == null)
            {
                return;
            }

            ComboSdoOperation.SelectedItem = SdoOperationMode.Read;
            TextSdoSlaveReference.Text = requirement.SlaveReference.ToString(
                CultureInfo.InvariantCulture);
            TextSdoIndex.Text = "0x" + requirement.ObjectIndex.ToString("X4");
            TextSdoSubIndex.Text = requirement.SubIndex.ToString(
                CultureInfo.InvariantCulture);
            ComboSdoValueType.SelectedItem = requirement.ValueType;
            ComboSdoDataLength.SelectedItem = requirement.DataLength;
            TextSdoTimeoutCycles.Text = requirement.TimeoutCycles.ToString(
                CultureInfo.InvariantCulture);
            ShowPendingD5SdoWriteReadbackStatus();
        }

        private bool CaptureSdoEditorDraftBeforeRequiredReadback(
            LMCSdoWriteVerificationContext requirement)
        {
            var currentConnection = connection;
            if (requirement == null
                || currentConnection == null
                || !currentConnection.IsConnected
                || !requirement.MatchesOwnerCurrentSession(
                    currentConnection)
                || ComboSdoOperation == null
                || !(ComboSdoOperation.SelectedItem
                    is SdoOperationMode operation)
                || !(ComboSdoValueType.SelectedItem
                    is LMCSignalValueType valueType)
                || !(ComboSdoDataLength.SelectedItem
                    is ushort dataLength))
            {
                return false;
            }

            var existing = pendingSdoEditorDraftSnapshot;
            if (existing != null
                && ReferenceEquals(
                    existing.PendingReadback,
                    requirement)
                && ReferenceEquals(
                    existing.OwnerConnection,
                    currentConnection)
                && existing.OwnerSessionGeneration
                    == currentConnection.SessionGeneration)
            {
                return true;
            }

            pendingSdoEditorDraftSnapshot = new SdoEditorDraftSnapshot(
                requirement,
                currentConnection,
                currentConnection.SessionGeneration,
                operation,
                ComboSdoWriteTarget.SelectedItem as LMCSdoWriteTarget,
                TextSdoSlaveReference.Text,
                TextSdoIndex.Text,
                TextSdoSubIndex.Text,
                valueType,
                dataLength,
                TextSdoTimeoutCycles.Text,
                TextSdoWriteData.Text);
            return true;
        }

        private bool TryRestoreSdoEditorDraftAfterVerifiedReadback(
            LMCSdoWriteVerificationContext requirement,
            LMCConnection verifiedConnection)
        {
            var snapshot = pendingSdoEditorDraftSnapshot;
            pendingSdoEditorDraftSnapshot = null;
            if (snapshot == null
                || requirement == null
                || verifiedConnection == null
                || !verifiedConnection.IsConnected
                || !ReferenceEquals(
                    snapshot.PendingReadback,
                    requirement)
                || !ReferenceEquals(
                    snapshot.OwnerConnection,
                    verifiedConnection)
                || !ReferenceEquals(connection, verifiedConnection)
                || snapshot.OwnerSessionGeneration
                    != verifiedConnection.SessionGeneration
                || !IsRequiredSdoReadbackStillLoadedInEditor(
                    requirement))
            {
                return false;
            }

            ComboSdoOperation.SelectedItem = snapshot.Operation;

            LMCSdoWriteTarget restoredTarget = null;
            if (snapshot.WriteTarget != null)
            {
                restoredTarget = approvedSdoWriteTargets.FirstOrDefault(
                    value => value.SlaveReference
                            == snapshot.WriteTarget.SlaveReference
                        && value.ObjectIndex
                            == snapshot.WriteTarget.ObjectIndex
                        && value.SubIndex == snapshot.WriteTarget.SubIndex
                        && value.ValueType == snapshot.WriteTarget.ValueType
                        && value.DataLength
                            == snapshot.WriteTarget.DataLength);
            }

            ComboSdoWriteTarget.SelectedItem = restoredTarget;
            TextSdoSlaveReference.Text = snapshot.SlaveReference;
            TextSdoIndex.Text = snapshot.ObjectIndex;
            TextSdoSubIndex.Text = snapshot.SubIndex;
            ComboSdoValueType.SelectedItem = snapshot.ValueType;
            ComboSdoDataLength.SelectedItem = snapshot.DataLength;
            TextSdoTimeoutCycles.Text = snapshot.TimeoutCycles;
            TextSdoWriteData.Text = snapshot.WriteData;
            return true;
        }

        private bool IsRequiredSdoReadbackStillLoadedInEditor(
            LMCSdoWriteVerificationContext requirement)
        {
            return requirement != null
                && ComboSdoOperation.SelectedItem
                    is SdoOperationMode operation
                && operation == SdoOperationMode.Read
                && string.Equals(
                    TextSdoSlaveReference.Text,
                    requirement.SlaveReference.ToString(
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                && string.Equals(
                    TextSdoIndex.Text,
                    "0x" + requirement.ObjectIndex.ToString("X4"),
                    StringComparison.Ordinal)
                && string.Equals(
                    TextSdoSubIndex.Text,
                    requirement.SubIndex.ToString(
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)
                && ComboSdoValueType.SelectedItem
                    is LMCSignalValueType valueType
                && valueType == requirement.ValueType
                && ComboSdoDataLength.SelectedItem is ushort dataLength
                && dataLength == requirement.DataLength
                && string.Equals(
                    TextSdoTimeoutCycles.Text,
                    requirement.TimeoutCycles.ToString(
                        CultureInfo.InvariantCulture),
                    StringComparison.Ordinal);
        }

        private void ShowPendingD5SdoWriteReadbackStatus()
        {
            var requirement = d5SdoPendingWriteReadback;
            if (requirement == null
                || ButtonSubmitSdo == null
                || TextDiagnosticOperationSummary == null)
            {
                return;
            }

            var ownerSessionExact = requirement
                .MatchesOwnerCurrentSession(connection);
            ButtonSubmitSdo.Content = ownerSessionExact
                ? "Submit Required Exact Readback"
                : "Readback Session Mismatch";
            TextDiagnosticOperationSummary.Text =
                "SDO Write transport completed. Exact manual readback is required for "
                + FormatD5SdoWriteReadbackTarget(requirement)
                + "; expected bytes="
                + BitConverter.ToString(requirement.ExpectedWriteData)
                + ". Required identity: BootId=0x"
                + requirement.DiagnosticsBootId.ToString("X8")
                + ", MapRevision=0x"
                + requirement.SubmissionMapRevision.ToString("X8")
                + ". Select Load Required Exact Readback to load the required request without sending it. The previous editor draft is retained only in this process and is restored after VERIFIED only while the editor still contains the untouched loaded request; any later operator edit wins and is never overwritten. "
                + (ownerSessionExact
                    ? "Mutation and Close remain blocked until an exact successful match under this same connection session."
                    : "The current connection session is not the original Write session; no readback may be submitted. Mutation and Close remain blocked until independent physical verification and explicit Persisted Mutation Recovery acknowledgement.");
        }

        private bool TryCreateSdoRequest(
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

            if (diagnosticOperationTicket != null
                && (diagnosticOperationStatus == null
                    || !diagnosticOperationStatus.IsTerminal))
            {
                validationMessage =
                    "Refresh or cancel the current operation ticket before submitting another SDO operation.";
                return false;
            }

            try
            {
                var mode = RequireSelectedEnum<SdoOperationMode>(
                    ComboSdoOperation,
                    "SDO operation");
                var timeoutCycles = ParseUInt32(
                    TextSdoTimeoutCycles.Text,
                    "SDO timeout cycles");
                if (timeoutCycles < 1 || timeoutCycles > 60000)
                {
                    validationMessage =
                        "Timeout must be between 1 and 60000 cycles.";
                    return false;
                }

                if (HasPendingD5SdoWriteReadback
                    && mode != SdoOperationMode.Read)
                {
                    validationMessage =
                        "Only the required exact SDO Readback may be submitted while the Write readback interlock is active.";
                    return false;
                }

                if (mode == SdoOperationMode.Write)
                {
                    if (!diagnosticCapabilities.Supports(
                        LMCDiagnosticCapability.SDOWrite))
                    {
                        validationMessage =
                            "SDO Write is not advertised by the connected PLC.";
                        return false;
                    }

                    if (!diagnosticCapabilities.Supports(
                        LMCDiagnosticCapability.SDORead))
                    {
                        validationMessage =
                            "The manual SDO Write workflow requires SDO Read for post-write verification and recovery.";
                        return false;
                    }

                    if (!diagnosticCapabilities.Supports(
                        LMCDiagnosticCapability.SDOReadGeneralInline))
                    {
                        validationMessage =
                            "The manual SDO Write workflow requires SDOReadGeneralInline for exact target readback.";
                        return false;
                    }

                    var writeSlaveReference = ParseUInt16Wire(
                        TextSdoSlaveReference.Text,
                        "SDO slave reference",
                        false);
                    var writeObjectIndex = ParseUInt16Wire(
                        TextSdoIndex.Text,
                        "SDO object index",
                        false);
                    var writeSubIndex = ParseByteWire(
                        TextSdoSubIndex.Text,
                        "SDO sub-index");
                    var writeValueType =
                        RequireSelectedEnum<LMCSignalValueType>(
                        ComboSdoValueType,
                        "SDO value type");
                    var writeDataLength = ParseUInt16Wire(
                        ComboSdoDataLength.Text,
                        "SDO data length",
                        false);
                    if (writeSlaveReference < 1
                        || writeSlaveReference > 4)
                    {
                        validationMessage =
                            "Generic SDO Write supports Slave reference 1 through 4 only.";
                        return false;
                    }

                    var expectedWriteLength =
                        GetSdoReadDataLength(writeValueType);
                    if (writeDataLength != expectedWriteLength)
                    {
                        validationMessage =
                            "Data length must match the selected type: 8-bit types=1, 16-bit types=2, 32-bit types=4.";
                        return false;
                    }

                    if (diagnosticCapabilities.MaxSdoDataBytes
                            < writeDataLength
                        || diagnosticCapabilities.MaxRequestPayloadBytes
                            < 32 + writeDataLength)
                    {
                        validationMessage =
                            "The PLC capability payload limits cannot carry the requested SDO Write.";
                        return false;
                    }

                    var writeData = ParseSdoWriteScalarData(
                        TextSdoWriteData.Text,
                        writeValueType,
                        writeDataLength);
                    request = LMCSdoRequest.CreateWrite(
                        writeSlaveReference,
                        writeObjectIndex,
                        writeSubIndex,
                        writeValueType,
                        writeData,
                        timeoutCycles);
                    LMCDiagnosticsWritePolicy.RequireSdoWriteAllowed(request);
                    validationMessage = null;
                    return true;
                }

                if (!diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.SDORead))
                {
                    validationMessage =
                        "SDO Read is not advertised by the connected PLC.";
                    return false;
                }

                if (HasPendingD5SdoWriteReadback
                    && !diagnosticCapabilities.Supports(
                        LMCDiagnosticCapability.SDOReadGeneralInline))
                {
                    validationMessage =
                        "The pending exact SDO Write readback requires SDOReadGeneralInline.";
                    return false;
                }

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

                if (slaveReference < 1 || slaveReference > 4)
                {
                    validationMessage =
                        "Slave reference must be between 1 and 4.";
                    return false;
                }

                var expectedLength = GetSdoReadDataLength(valueType);
                if (dataLength != expectedLength)
                {
                    validationMessage =
                        "Data length must match the selected type: 8-bit types=1, 16-bit types=2, 32-bit types=4.";
                    return false;
                }

                if (diagnosticCapabilities.MaxSdoDataBytes < dataLength)
                {
                    validationMessage =
                        "The selected data length exceeds the PLC MaxSdoDataBytes capability.";
                    return false;
                }

                request = LMCSdoRequest.CreateRead(
                    slaveReference,
                    objectIndex,
                    subIndex,
                    valueType,
                    dataLength,
                    timeoutCycles);
                if (HasPendingD5SdoWriteReadback
                    && !d5SdoPendingWriteReadback.MatchesReadRequest(request))
                {
                    request = null;
                    validationMessage =
                        "The pending SDO Write interlock accepts only the exact same Slave/Object/SubIndex/ValueType/DataLength Readback.";
                    return false;
                }

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
            catch (NotSupportedException error)
            {
                validationMessage = error.Message;
                return false;
            }
        }

        private bool TryCreateInlineSdoReadRequest(
            out LMCSdoRequest request,
            out string validationMessage)
        {
            request = null;
            if (HasPendingD5SdoWriteReadback)
            {
                validationMessage =
                    "Read SDO Inline cannot be used for a pending exact SDO Write readback. Use Submit/Refresh so the write verification interlock remains authoritative.";
                return false;
            }

            var mode = ComboSdoOperation.SelectedItem
                is SdoOperationMode
                    ? (SdoOperationMode)ComboSdoOperation.SelectedItem
                    : SdoOperationMode.Read;
            if (mode != SdoOperationMode.Read)
            {
                validationMessage =
                    "Read SDO Inline accepts Read mode only; it never sends SDO Write.";
                return false;
            }

            if (!TryCreateSdoRequest(
                out request,
                out validationMessage))
            {
                return false;
            }

            if (request.IsWrite
                || (request.DataLength != 1
                    && request.DataLength != 2
                    && request.DataLength != 4))
            {
                request = null;
                validationMessage =
                    "Read SDO Inline accepts ordinary 1/2/4-byte Read requests only.";
                return false;
            }

            if (RequiresGeneralInlineSdoRead(request)
                && !diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline))
            {
                request = null;
                validationMessage =
                    "This SDO Read requires SDOReadGeneralInline capability. The connected PLC currently permits only 0x1000:0 UInt32/4.";
                return false;
            }

            return true;
        }

        private static ushort GetSdoReadDataLength(
            LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                case LMCSignalValueType.Int8:
                case LMCSignalValueType.UInt8:
                case LMCSignalValueType.BitField8:
                    return 1;
                case LMCSignalValueType.Int16:
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return 2;
                case LMCSignalValueType.Int32:
                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.Real32:
                case LMCSignalValueType.BitField32:
                    return 4;
                default:
                    throw new InvalidOperationException(
                        "The selected SDO value type is unsupported.");
            }
        }

        private static byte[] ParseSdoWriteScalarData(
            string value,
            LMCSignalValueType valueType,
            ushort dataLength)
        {
            var expectedLength = GetSdoReadDataLength(valueType);
            if (dataLength != expectedLength)
            {
                throw new InvalidOperationException(
                    "The SDO Write data length does not match the selected value type.");
            }

            var text = (value ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                throw new InvalidOperationException(
                    "SDO Write value is required.");
            }

            uint raw;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                raw = ParseUInt32Wire(text, "SDO Write value");
                var maximumRaw = dataLength == 1
                    ? 0xFFu
                    : dataLength == 2
                        ? 0xFFFFu
                        : uint.MaxValue;
                if (raw > maximumRaw)
                {
                    throw new InvalidOperationException(
                        "The raw hexadecimal SDO Write value does not fit the selected data length.");
                }
            }
            else
            {
                raw = ParseSdoWriteScalarDecimal(text, valueType);
            }

            if (valueType == LMCSignalValueType.Bool && raw > 1)
            {
                throw new InvalidOperationException(
                    "Bool SDO Write accepts only false/true or 0/1.");
            }

            var data = new byte[dataLength];
            for (var index = 0; index < data.Length; index++)
            {
                data[index] = (byte)(raw >> (index * 8));
            }

            return data;
        }

        private static uint ParseSdoWriteScalarDecimal(
            string text,
            LMCSignalValueType valueType)
        {
            switch (valueType)
            {
                case LMCSignalValueType.Bool:
                    if (string.Equals(
                        text,
                        "true",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return 1;
                    }

                    if (string.Equals(
                        text,
                        "false",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return 0;
                    }

                    return ParseUnsignedSdoWriteDecimal(text, 1);
                case LMCSignalValueType.Int8:
                    return unchecked((byte)ParseSignedSdoWriteDecimal(
                        text,
                        sbyte.MinValue,
                        sbyte.MaxValue));
                case LMCSignalValueType.Int16:
                    return unchecked((ushort)ParseSignedSdoWriteDecimal(
                        text,
                        short.MinValue,
                        short.MaxValue));
                case LMCSignalValueType.Int32:
                    return unchecked((uint)(int)ParseSignedSdoWriteDecimal(
                        text,
                        int.MinValue,
                        int.MaxValue));
                case LMCSignalValueType.UInt8:
                case LMCSignalValueType.BitField8:
                    return ParseUnsignedSdoWriteDecimal(text, byte.MaxValue);
                case LMCSignalValueType.UInt16:
                case LMCSignalValueType.BitField16:
                    return ParseUnsignedSdoWriteDecimal(text, ushort.MaxValue);
                case LMCSignalValueType.UInt32:
                case LMCSignalValueType.BitField32:
                    return ParseUnsignedSdoWriteDecimal(text, uint.MaxValue);
                case LMCSignalValueType.Real32:
                    float realValue;
                    if (!float.TryParse(
                        text,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out realValue)
                        || float.IsNaN(realValue)
                        || float.IsInfinity(realValue))
                    {
                        throw new InvalidOperationException(
                            "Real32 SDO Write value must be a finite invariant-culture number or raw 0x hexadecimal bits.");
                    }

                    var realBytes = BitConverter.GetBytes(realValue);
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(realBytes);
                    }

                    return (uint)realBytes[0]
                        | ((uint)realBytes[1] << 8)
                        | ((uint)realBytes[2] << 16)
                        | ((uint)realBytes[3] << 24);
                default:
                    throw new InvalidOperationException(
                        "The selected SDO Write value type is unsupported.");
            }
        }

        private static long ParseSignedSdoWriteDecimal(
            string text,
            long minimum,
            long maximum)
        {
            long value;
            if (!long.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value)
                || value < minimum
                || value > maximum)
            {
                throw new InvalidOperationException(
                    "The signed SDO Write value is outside the selected type range.");
            }

            return value;
        }

        private static uint ParseUnsignedSdoWriteDecimal(
            string text,
            uint maximum)
        {
            uint value;
            if (!uint.TryParse(
                text,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value)
                || value > maximum)
            {
                throw new InvalidOperationException(
                    "The unsigned SDO Write value is outside the selected type range.");
            }

            return value;
        }

        private static void RequireManualSdoOperationCapabilities(
            LMCDiagnosticCapabilities capabilities,
            LMCSdoRequest request)
        {
            if (capabilities == null)
            {
                throw new ArgumentNullException("capabilities");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var requiredCapability = request.IsWrite
                ? LMCDiagnosticCapability.SDOWrite
                : LMCDiagnosticCapability.SDORead;
            if (!capabilities.Supports(requiredCapability))
            {
                throw new NotSupportedException(
                    "The connected PLC no longer advertises the selected SDO operation.");
            }

            if (request.IsWrite
                && (!capabilities.Supports(
                        LMCDiagnosticCapability.SDORead)
                    || !capabilities.Supports(
                        LMCDiagnosticCapability.SDOReadGeneralInline)))
            {
                throw new NotSupportedException(
                    "Manual SDO Write requires SDORead and SDOReadGeneralInline for exact target readback and recovery evidence.");
            }

            var requiredRequestBytes = 32
                + (request.IsWrite ? request.DataLength : 0);
            if (capabilities.MaxSdoDataBytes < request.DataLength
                || capabilities.MaxRequestPayloadBytes < requiredRequestBytes
                || capabilities.MaxResponsePayloadBytes < 64)
            {
                throw new InvalidOperationException(
                    "The refreshed PLC capability limits cannot carry the selected SDO operation.");
            }
        }

        private static string FormatArmedSdoWriteConfirmation(
            LMCSdoRequest request)
        {
            if (request == null || !request.IsWrite)
            {
                throw new ArgumentException(
                    "An SDO Write request is required.",
                    "request");
            }

            var writeData = request.WriteData;
            return "SDO WRITE CONFIRMATION ARMED - NOT SUBMITTED"
                    + Environment.NewLine
                    + "The selected axis passed PowerOn=False, Standstill=True, and stable-position checks."
                    + Environment.NewLine
                    + "Review this immutable snapshot, then click Confirm & Submit SDO Write. The safety checks run again immediately before journal arm and submission."
                    + Environment.NewLine
                    + "The editor stays available. If any request field changes, the next click arms the changed snapshot instead of submitting the old one."
                    + Environment.NewLine
                    + "Keep the target under one writer until terminal status and exact readback are complete."
                    + Environment.NewLine
                    + "Slave: " + request.SlaveReference.ToString(
                        CultureInfo.InvariantCulture)
                    + Environment.NewLine
                    + "Object: 0x" + request.ObjectIndex.ToString("X4")
                    + ":" + request.SubIndex.ToString(
                        CultureInfo.InvariantCulture)
                    + Environment.NewLine
                    + "Type: " + request.ValueType
                    + Environment.NewLine
                    + "Value: " + FormatSdoWriteSnapshotValue(request, writeData)
                    + Environment.NewLine
                    + "Wire bytes: " + BitConverter.ToString(writeData)
                    + Environment.NewLine
                    + "Timeout cycles: " + request.TimeoutCycles.ToString(
                        CultureInfo.InvariantCulture);
        }

        private static string FormatSdoWriteSnapshotValue(
            LMCSdoRequest request,
            byte[] writeData)
        {
            if (request == null
                || writeData == null
                || writeData.Length != 4)
            {
                return "UNAVAILABLE";
            }

            var raw = (uint)writeData[0]
                | ((uint)writeData[1] << 8)
                | ((uint)writeData[2] << 16)
                | ((uint)writeData[3] << 24);
            if (request.ValueType == LMCSignalValueType.Int32)
            {
                return unchecked((int)raw).ToString(
                    CultureInfo.InvariantCulture)
                    + " (0x" + raw.ToString("X8") + ")";
            }

            if (request.ValueType == LMCSignalValueType.UInt32)
            {
                return raw.ToString(CultureInfo.InvariantCulture)
                    + " (0x" + raw.ToString("X8") + ")";
            }

            return "0x" + raw.ToString("X8");
        }

        private static string FormatSdoWriteManualReadbackWarning(
            LMCSdoRequest request)
        {
            return Environment.NewLine
                + "SDO Write transport terminal only; no automatic target readback was performed. The draft editor remains editable, but the next SDO submission is restricted to the required exact Read, and mutation/Close remain blocked until it confirms "
                + (request == null
                    ? "the written target and value."
                    : "0x"
                        + request.ObjectIndex.ToString("X4")
                        + ":"
                        + request.SubIndex.ToString(
                            CultureInfo.InvariantCulture)
                        + " as "
                        + request.ValueType
                        + "/"
                        + request.DataLength.ToString(
                            CultureInfo.InvariantCulture)
                        + " bytes with the expected value.");
        }

        private static string FormatD5SdoWriteReadbackTarget(
            LMCSdoWriteVerificationContext requirement)
        {
            if (requirement == null)
            {
                return "UNKNOWN";
            }

            return "Slave "
                + requirement.SlaveReference.ToString(
                    CultureInfo.InvariantCulture)
                + ", 0x"
                + requirement.ObjectIndex.ToString("X4")
                + ":"
                + requirement.SubIndex.ToString(
                    CultureInfo.InvariantCulture)
                + ", "
                + requirement.ValueType
                + "/"
                + requirement.DataLength.ToString(
                    CultureInfo.InvariantCulture)
                + " bytes, BootId=0x"
                + requirement.DiagnosticsBootId.ToString("X8")
                + ", MapRevision=0x"
                + requirement.SubmissionMapRevision.ToString("X8");
        }

        private LMCOperationTicket RequireDiagnosticOperationTicket()
        {
            if (diagnosticOperationTicket == null)
            {
                throw new InvalidOperationException(
                    "Submit an SDO, PI Write, or digital output operation first.");
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
                + (ticket.OperationKind == LMCOperationKind.DigitalOutputWrite
                    ? ", TopologyRevision=0x"
                        + ticket.SubmissionTopologyRevision.ToString("X8")
                    : ", MapRevision=0x"
                        + ticket.SubmissionMapRevision.ToString("X8"))
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

        private static string FormatInlineSdoReadSuccess(
            LMCSdoReadResult result)
        {
            var resultData = result.ResultData;
            return FormatOperationStatus(result.Status)
                + Environment.NewLine
                + "Inline SDO Read terminal success: TypedValue="
                + FormatInlineSdoTypedValue(result.Value)
                + " ("
                + result.ValueType
                + "), Raw="
                + BitConverter.ToString(resultData).Replace("-", " ")
                + ". No manual Refresh was required.";
        }

        private static string FormatInlineSdoReadFailure(
            Exception error,
            LMCSdoSubmissionFailureContext failureContext,
            LMCOperationStatus lastObservedStatus)
        {
            var terminalFailure = error as LMCSdoReadOperationException;
            if (terminalFailure != null)
            {
                return FormatOperationStatus(
                        terminalFailure.OperationStatus)
                    + Environment.NewLine
                    + "Inline SDO Read reached a terminal failure. The terminal ticket is displayed and must not be resubmitted automatically. Error="
                    + error.Message;
            }

            if (failureContext != null
                && failureContext.SubmissionOutcome
                    == LMCSdoSubmissionOutcome.Accepted)
            {
                return (lastObservedStatus == null
                        ? FormatOperationTicket(failureContext.Ticket)
                        : FormatOperationStatus(lastObservedStatus))
                    + Environment.NewLine
                    + "Inline wait failed after PLC ticket acceptance. The ticket is preserved in the existing D5 manual-cleanup path; use Refresh Ticket and do not resubmit. Error="
                    + error.Message;
            }

            if (failureContext != null
                && failureContext.SubmissionOutcome
                    == LMCSdoSubmissionOutcome.OutcomeUncertain)
            {
                return "Inline SDO Read submission outcome is uncertain and was quarantined. Do not resubmit until manual D5 recovery resolves the PLC slot. Error="
                    + error.Message;
            }

            return "Inline SDO Read was not accepted by the PLC. No automatic retry was attempted. Error="
                + error.Message;
        }

        private static LMCOperationStatus GetInlineSdoReadObservedStatus(
            Exception error)
        {
            var operationFailure = error as LMCSdoReadOperationException;
            if (operationFailure != null)
            {
                return operationFailure.OperationStatus;
            }

            var pollingTimeout = error
                as LMCSdoReadPollingTimeoutException;
            if (pollingTimeout != null)
            {
                return pollingTimeout.LastObservedStatus;
            }

            var waitCanceled = error
                as LMCSdoReadWaitCanceledException;
            return waitCanceled == null
                ? null
                : waitCanceled.LastObservedStatus;
        }

        private static string FormatInlineSdoTypedValue(object value)
        {
            var formattable = value as IFormattable;
            return formattable == null
                ? Convert.ToString(value, CultureInfo.InvariantCulture)
                : formattable.ToString(null, CultureInfo.InvariantCulture);
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
            Read,
            Write
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
                rawValue = value.IsValid
                    ? FormatRawValue(value.RawValue32, value.ValueType)
                    : "UNAVAILABLE";
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
            public HealthSlaveRow(
                LMCEtherCATSlaveHealth value,
                string configuredSlaveIndex)
            {
                SlaveIndex = value.SlaveIndex;
                ConfiguredSlaveIndex = configuredSlaveIndex;
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
            public string ConfiguredSlaveIndex { get; private set; }
            public ushort PhysicalAxis { get; private set; }
            public bool Online { get; private set; }
            public string EtherCATState { get; private set; }
            public string ALStatusCode { get; private set; }
            public string DS402StatusWord { get; private set; }
            public string AxisError { get; private set; }
            public uint LastValidCycle { get; private set; }
            public uint LastStateChangeCycle { get; private set; }

            public void SetConfiguredSlaveIndex(string value)
            {
                ConfiguredSlaveIndex = value;
            }
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
                RawValue = value.IsValid
                    ? FormatRawValue(value.RawValue32, value.ValueType)
                    : "UNAVAILABLE";
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly string maintenanceActionRecoveryJournalDirectoryPath;
        private MaintenanceActionRecoveryJournal maintenanceActionRecoveryJournal;
        private string maintenanceActionRecoveryJournalError;
        private LMCPreparedEncoderMaintenance armedEncoderMaintenance;
        private string armedEncoderMaintenanceFingerprint;
        private LMCEncoderMaintenanceRecoveryKey
            latestEncoderMaintenanceRecoveryKey;
        private LMCHomeRecoveryKey latestLmcHomeRecoveryKey;
        private LMCAxisDs402HomeRecoveryKey latestDs402HomeRecoveryKey;

        private bool MaintenanceActionRecoveryJournalUnavailable
        {
            get
            {
                return maintenanceActionRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        maintenanceActionRecoveryJournalError);
            }
        }

        private bool HasUnresolvedMaintenanceAction
        {
            get
            {
                return maintenanceActionRecoveryJournal != null
                    && maintenanceActionRecoveryJournal.HasActiveRecord;
            }
        }

        private bool MaintenanceActionRecoveryJournalCanArm
        {
            get
            {
                return !MaintenanceActionRecoveryJournalUnavailable
                    && !maintenanceActionRecoveryJournal.HasActiveRecord;
            }
        }

        internal MaintenanceActionRecoveryRecord
            ActiveMaintenanceActionRecoveryRecordForTests
        {
            get
            {
                return HasUnresolvedMaintenanceAction
                    ? maintenanceActionRecoveryJournal.CurrentRecord
                    : null;
            }
        }

        internal bool EncoderMaintenanceStepOneConfirmedForTests
        {
            get { return AreEncoderMaintenanceStepOneChecksConfirmed; }
        }

        private bool AreEncoderMaintenanceStepOneChecksConfirmed
        {
            get
            {
                return CheckTestResetPowerOffVerified != null
                    && CheckTestResetPowerOffVerified.IsChecked == true
                    && CheckTestResetPhysicalPositionVerified.IsChecked
                        == true
                    && CheckTestResetExactTargetVerified.IsChecked == true
                    && CheckEncoderMaintenanceCompatibilityVerified
                        .IsChecked == true;
            }
        }

        private void InitializeMaintenanceActionUi()
        {
            ComboEncoderMaintenanceKind.ItemsSource = new[]
            {
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset,
                LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset
            };
            ComboEncoderMaintenanceKind.SelectedItem =
                LMCEncoderMaintenanceKind.Tw20ErrorWarningReset;
            ComboTestResetAxis.ItemsSource = new ushort[] { 1, 2, 3, 4 };
            ComboTestResetAxis.SelectedItem = (ushort)1;
            ComboTestResetSocket.ItemsSource = new[]
            {
                LMCEncoderFeedbackSocket.Socket1
            };
            ComboTestResetSocket.SelectedItem =
                LMCEncoderFeedbackSocket.Socket1;

            try
            {
                maintenanceActionRecoveryJournal =
                    maintenanceActionRecoveryJournalDirectoryPath == null
                        ? MaintenanceActionRecoveryJournal.OpenDefault()
                        : MaintenanceActionRecoveryJournal.Open(
                            maintenanceActionRecoveryJournalDirectoryPath);
                if (maintenanceActionRecoveryJournal.HasActiveRecord)
                {
                    var active = maintenanceActionRecoveryJournal
                        .CurrentRecord;
                    if (active.Action == MaintenanceActionKind.LmcHome)
                    {
                        latestLmcHomeRecoveryKey =
                            RecreateLmcHomeRecoveryKey(active);
                    }
                    else if (IsEncoderMaintenanceAction(active.Action))
                    {
                        latestEncoderMaintenanceRecoveryKey =
                            RecreateEncoderMaintenanceRecoveryKey(active);
                    }
                }
            }
            catch (Exception error)
            {
                if (maintenanceActionRecoveryJournal != null)
                {
                    maintenanceActionRecoveryJournal.Dispose();
                }

                maintenanceActionRecoveryJournal = null;
                maintenanceActionRecoveryJournalError = error.Message;
                WriteLog(
                    "SAFETY: Home/encoder-maintenance recovery journal is unavailable. "
                    + "All Home and encoder-maintenance sends remain disabled: "
                    + error.Message);
            }

            if (HasUnresolvedMaintenanceAction)
            {
                var recovered = maintenanceActionRecoveryJournal
                    .CurrentRecord;
                TextRemoteIp.Text = recovered.EndpointIp;
                TextRemotePort.Text = recovered.EndpointPort.ToString(
                    CultureInfo.InvariantCulture);
                TextAxisName.Text = recovered.AxisName;
            }

            RefreshMaintenanceActionRecoveryUi();
        }

        private void EnsureMaintenanceRecoveryEndpoint(
            string endpointIp,
            int endpointPort)
        {
            if (!HasUnresolvedMaintenanceAction)
            {
                return;
            }

            var record = maintenanceActionRecoveryJournal.CurrentRecord;
            if (!string.Equals(
                    record.EndpointIp,
                    endpointIp,
                    StringComparison.Ordinal)
                || record.EndpointPort != endpointPort)
            {
                throw new InvalidOperationException(
                    "Reconnect is blocked because the selected endpoint does not match the durable Home/encoder-maintenance recovery record. Stored endpoint="
                    + record.EndpointIp
                    + ":"
                    + record.EndpointPort
                    + ".");
            }
        }

        private async Task EnsureMaintenanceRecoveryConnectionIdentityAsync(
            string operation)
        {
            if (!HasUnresolvedMaintenanceAction)
            {
                return;
            }

            var record = maintenanceActionRecoveryJournal.CurrentRecord;
            var currentConnection = RequireConnection();
            var capabilities = await currentConnection.Diagnostics
                .GetCapabilitiesAsync(CancellationToken.None);
            diagnosticCapabilities = capabilities;
            if (record.ObservedDiagnosticsBuild
                    != capabilities.DiagnosticsBuild
                || record.ObservedDiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || record.ObservedMapRevision
                    != capabilities.MapRevision)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    operation
                    + " is blocked because DiagnosticsBuild, BootId, or MapRevision does not match the durable Home/encoder-maintenance recovery record. Stored Build/Boot/Map=0x"
                    + record.ObservedDiagnosticsBuild.ToString("X8")
                    + "/0x"
                    + record.ObservedDiagnosticsBootId.ToString("X8")
                    + "/0x"
                    + record.ObservedMapRevision.ToString("X8")
                    + ", current=0x"
                    + capabilities.DiagnosticsBuild.ToString("X8")
                    + "/0x"
                    + capabilities.DiagnosticsBootId.ToString("X8")
                    + "/0x"
                    + capabilities.MapRevision.ToString("X8")
                    + ".");
            }
        }

        private void DisposeMaintenanceActionRecoveryJournal()
        {
            var journal = maintenanceActionRecoveryJournal;
            maintenanceActionRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private void UpdateMaintenanceActionUiState(
            bool connected,
            bool idle,
            bool axisReady,
            bool liveCommandAllowed)
        {
            if (ButtonLmcHome == null)
            {
                return;
            }

            var activeRecovery = HasUnresolvedMaintenanceAction;
            var activeRecoveryRecord = activeRecovery
                ? maintenanceActionRecoveryJournal.CurrentRecord
                : null;
            var manualRecoveryResolutionAllowed = activeRecoveryRecord != null
                && activeRecoveryRecord.Action
                    != MaintenanceActionKind.Ds402Home
                && activeRecoveryRecord.Action
                    != MaintenanceActionKind.LmcHome
                && !IsEncoderMaintenanceAction(
                    activeRecoveryRecord.Action);
            var diagnosticsIdentityReady = diagnosticCapabilities != null
                && diagnosticCapabilities.Response != null
                && diagnosticCapabilities.Response.IsSuccess
                && diagnosticCapabilities.DiagnosticsBuild != 0
                && diagnosticCapabilities.DiagnosticsBootId != 0
                && diagnosticCapabilities.MapRevision != 0;
            var lmcCapability = adminCapabilities != null
                && adminCapabilities.Supports(LMCAdminFeature.AxisHome);
            var ds402Capability = adminCapabilities != null
                && adminCapabilities.Supports(
                    LMCAdminFeature.AxisDs402Home);
            var selectedEncoderKind =
                ComboEncoderMaintenanceKind.SelectedItem
                    is LMCEncoderMaintenanceKind
                    ? (LMCEncoderMaintenanceKind)
                        ComboEncoderMaintenanceKind.SelectedItem
                    : LMCEncoderMaintenanceKind.Tw20ErrorWarningReset;
            var resetCapability = diagnosticsIdentityReady
                && diagnosticCapabilities.Supports(
                    RequiredEncoderMaintenanceCapability(
                        selectedEncoderKind));
            var homeInputEnabled = idle && !activeRecovery;

            ButtonRefreshHomeCapabilities.IsEnabled = connected && idle;
            ButtonRefreshTestResetCapabilities.IsEnabled = connected && idle;
            ButtonReadHomeStatus.IsEnabled = connected && idle && axisReady;
            TextLmcHomeTimeout.IsEnabled = homeInputEnabled;
            TextDs402HomeTimeout.IsEnabled = homeInputEnabled;
            CheckHomeOneShotConfirmed.IsEnabled = homeInputEnabled;
            ButtonLmcHome.IsEnabled = axisReady
                && liveCommandAllowed
                && MaintenanceActionRecoveryJournalCanArm
                && MotionUncertaintyJournalCanArm
                && diagnosticsIdentityReady
                && lmcCapability
                && CheckHomeOneShotConfirmed.IsChecked == true;
            ButtonDs402Home.IsEnabled = axisReady
                && liveCommandAllowed
                && MaintenanceActionRecoveryJournalCanArm
                && MotionUncertaintyJournalCanArm
                && diagnosticsIdentityReady
                && ds402Capability
                && CheckHomeOneShotConfirmed.IsChecked == true;

            CheckMaintenanceRecoveryPhysicallyVerified.IsEnabled =
                connected && idle && manualRecoveryResolutionAllowed;
            ButtonResolveMaintenanceRecovery.IsEnabled = connected
                && idle
                && activeRecovery
                && manualRecoveryResolutionAllowed
                && CheckMaintenanceRecoveryPhysicallyVerified.IsChecked
                    == true;

            var resetInputEnabled = idle
                && !activeRecovery
                && armedEncoderMaintenance == null;
            ComboEncoderMaintenanceKind.IsEnabled = resetInputEnabled;
            TextEncoderMaintenanceProfile.IsEnabled = false;
            ComboTestResetAxis.IsEnabled = resetInputEnabled;
            ComboTestResetSocket.IsEnabled = false;
            TextTestResetTimeout.IsEnabled = resetInputEnabled;
            CheckTestResetPowerOffVerified.IsEnabled = resetInputEnabled;
            CheckTestResetPhysicalPositionVerified.IsEnabled =
                resetInputEnabled;
            CheckTestResetExactTargetVerified.IsEnabled = resetInputEnabled;
            CheckEncoderMaintenanceCompatibilityVerified.IsEnabled =
                resetInputEnabled;
            ButtonArmTestReset.IsEnabled = connected
                && idle
                && !activeRecovery
                && resetCapability
                && AreEncoderMaintenanceStepOneChecksConfirmed;
            CheckTestResetFinalConfirmed.IsEnabled = idle
                && armedEncoderMaintenance != null
                && !activeRecovery;
            ButtonExecuteTestReset.IsEnabled = connected
                && idle
                && armedEncoderMaintenance != null
                && MaintenanceActionRecoveryJournalCanArm
                && CheckTestResetFinalConfirmed.IsChecked == true;
            ButtonReadTestResetStatus.IsEnabled = connected
                && idle
                && ((latestEncoderMaintenanceRecoveryKey != null)
                    || (activeRecoveryRecord != null
                        && IsEncoderMaintenanceAction(
                            activeRecoveryRecord.Action)));

            RefreshMaintenanceActionRecoveryUi();
        }

        private async void ButtonRefreshMaintenanceCapabilities_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh Home/Encoder Maintenance Capabilities",
                async () =>
                {
                    await RefreshMaintenanceCapabilitiesAsync(
                        RequireConnection());
                    TextHomeResult.Text =
                        TranslateUiText("Capabilities refreshed.")
                        + " LMC Home="
                        + adminCapabilities.Supports(
                            LMCAdminFeature.AxisHome)
                        + ", DS402 Home="
                        + adminCapabilities.Supports(
                            LMCAdminFeature.AxisDs402Home)
                        + ", EncoderTw20ErrorWarningReset="
                        + diagnosticCapabilities.Supports(
                            LMCDiagnosticCapability
                                .EncoderTw20ErrorWarningReset)
                        + ", EncoderTw19MultiturnPositionReset="
                        + diagnosticCapabilities.Supports(
                            LMCDiagnosticCapability
                                .EncoderTw19MultiturnPositionReset)
                        + ".";
                    WriteLog(
                        "Home/Encoder capabilities: DiagnosticsBits=0x"
                        + diagnosticCapabilities.CapabilityBits.ToString(
                            "X8",
                            CultureInfo.InvariantCulture)
                        + "; DiagnosticsBuild="
                        + diagnosticCapabilities.DiagnosticsBuild.ToString(
                            CultureInfo.InvariantCulture)
                        + "; BootId=0x"
                        + diagnosticCapabilities.DiagnosticsBootId.ToString(
                            "X8",
                            CultureInfo.InvariantCulture)
                        + "; MapRevision=0x"
                        + diagnosticCapabilities.MapRevision.ToString(
                            "X8",
                            CultureInfo.InvariantCulture)
                        + "; TW20="
                        + diagnosticCapabilities.Supports(
                            LMCDiagnosticCapability
                                .EncoderTw20ErrorWarningReset)
                        + "; TW19="
                        + diagnosticCapabilities.Supports(
                            LMCDiagnosticCapability
                                .EncoderTw19MultiturnPositionReset)
                        + "; AdminFeatures=0x"
                        + ((uint)adminCapabilities.Features).ToString(
                            "X8",
                            CultureInfo.InvariantCulture));
                });
        }

        private async Task RefreshMaintenanceCapabilitiesAsync(
            LMCConnection currentConnection)
        {
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            adminCapabilities = await currentConnection.Admin
                .GetCapabilitiesAsync(CancellationToken.None);
            TextAdminCapabilities.Text =
                FormatAdminCapabilities(adminCapabilities);
        }

        private void MaintenanceInput_Changed(
            object sender,
            RoutedEventArgs e)
        {
            if (CheckHomeOneShotConfirmed != null)
            {
                CheckHomeOneShotConfirmed.IsChecked = false;
            }

            UpdateUiState();
        }

        private void MaintenanceConfirmation_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private async void ButtonLmcHome_Click(
            object sender,
            RoutedEventArgs e)
        {
            const string operation =
                "LMC Home Start (Current Position Zero)";
            if (CheckHomeOneShotConfirmed.IsChecked != true)
            {
                TextHomeResult.Text =
                    TranslateUiText(
                        "BLOCKED: explicit one-shot Home confirmation is required. No RPC was sent.");
                return;
            }

            CheckHomeOneShotConfirmed.IsChecked = false;
            await RunOperationAsync(
                operation,
                async () =>
                {
                    EnsureMaintenanceActionCanStart(
                        "LMC Home",
                        LMCAdminFeature.AxisHome,
                        LMCDiagnosticCapability.None);
                    var currentConnection = RequireConnection();
                    var currentAxis = RequireAxis();
                    await RefreshMaintenanceCapabilitiesAsync(
                        currentConnection);
                    EnsureMaintenanceActionCanStart(
                        "LMC Home",
                        LMCAdminFeature.AxisHome,
                        LMCDiagnosticCapability.None);

                    var actualPosition = await currentAxis
                        .GetActualPositionResultAsync(CancellationToken.None);
                    EnsureAxisPositionSuccess(
                        "LMC Home current-position guard",
                        actualPosition);
                    var parameters = ReadLmcHomeParameters(
                        actualPosition.PositionRaw);
                    var prepared = currentAxis.PrepareLMC_Home(
                        parameters,
                        adminCapabilities,
                        diagnosticCapabilities,
                        LMCHomeExecuteToken.Create());
                    var key = prepared.RecoveryKey;
                    latestLmcHomeRecoveryKey = key;
                    var intent = key.ClientIntentId;

                    var recovery = maintenanceActionRecoveryJournal
                        .ArmBeforeDispatch(
                            MaintenanceActionKind.LmcHome,
                            RequiredText(TextRemoteIp.Text, "PLC IP"),
                            ParsePort(TextRemotePort.Text, "TCP port", false),
                            key.DiagnosticsBuild,
                            key.OriginalDiagnosticsBootId,
                            key.MapRevision,
                            currentAxis.AxisName,
                            key.AxisReference,
                            intent.Word0,
                            intent.Word1,
                            intent.Word2,
                            intent.Word3,
                            key.OriginalRequestId,
                            FormatLmcHomeIdentity(key),
                            DateTime.UtcNow);
                    try
                    {
                        var acknowledgement = await currentAxis.LMC_HomeAsync(
                            prepared,
                            CancellationToken.None);
                        PromoteMaintenanceRecovery(
                            recovery,
                            acknowledgement.RequestId);
                        TextHomeResult.Text =
                            TranslateUiText(
                                "LMC Home Start=Accepted; Outcome=NotQueried")
                            + Environment.NewLine
                            + "RequestId="
                            + acknowledgement.RequestId
                            + ", AxisRef="
                            + acknowledgement.AxisReference
                            + ", Semantic="
                            + acknowledgement.SemanticMode
                            + ", ExpectedActualPosition="
                            + key.ExpectedActualPosition
                            + ", NativeCommandState=0x"
                            + acknowledgement.NativeCommandState.ToString("X8")
                            + Environment.NewLine
                            + TranslateUiText(
                                "Read Home Status performs the exact 0x7D18 outcome query; the start acknowledgement is not completion proof.");
                    }
                    catch (LMCHomeStartRejectedException rejected)
                    {
                        ResolveMaintenanceConfirmedRejection(
                            recovery,
                            rejected.Acknowledgement.Response.RequestId);
                        ClearMotionOnConfirmedRejection(
                            currentAxis.AxisName,
                            "LMC Home",
                            rejected.Acknowledgement.Response);
                        throw;
                    }
                    catch
                    {
                        PromoteMaintenanceRecovery(recovery, prepared.RequestId);
                        throw;
                    }
                });
            if (string.Equals(
                TextOperationState.Text,
                operation + " completed",
                StringComparison.Ordinal))
            {
                TextOperationState.Text =
                    "LMC Home Start accepted; outcome pending";
                WriteLog(
                    "LMC Home outcome pending. Use Read Home Status for exact completion proof.");
            }
        }

        private async void ButtonDs402Home_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CheckHomeOneShotConfirmed.IsChecked != true)
            {
                TextHomeResult.Text =
                    TranslateUiText(
                        "BLOCKED: explicit one-shot Home confirmation is required. No RPC was sent.");
                return;
            }

            CheckHomeOneShotConfirmed.IsChecked = false;
            await RunOperationAsync(
                "DS402 Home",
                async () =>
                {
                    EnsureMaintenanceActionCanStart(
                        "DS402 Home",
                        LMCAdminFeature.AxisDs402Home,
                        LMCDiagnosticCapability.None);
                    var currentConnection = RequireConnection();
                    var currentAxis = RequireAxis();
                    await RefreshMaintenanceCapabilitiesAsync(
                        currentConnection);
                    EnsureMaintenanceActionCanStart(
                        "DS402 Home",
                        LMCAdminFeature.AxisDs402Home,
                        LMCDiagnosticCapability.None);

                    var parameters = ReadDs402HomeParameters();
                    var prepared = currentAxis.PrepareDs402Home(
                        parameters,
                        adminCapabilities,
                        diagnosticCapabilities,
                        LMCAxisDs402HomeExecuteToken.Create());
                    var key = prepared.RecoveryKey;
                    latestDs402HomeRecoveryKey = key;
                    var intent = key.ClientIntentId;
                    var recovery = maintenanceActionRecoveryJournal
                        .ArmBeforeDispatch(
                            MaintenanceActionKind.Ds402Home,
                            RequiredText(TextRemoteIp.Text, "PLC IP"),
                            ParsePort(TextRemotePort.Text, "TCP port", false),
                            key.DiagnosticsBuild,
                            key.DiagnosticsBootId,
                            key.MapRevision,
                            currentAxis.AxisName,
                            currentAxis.AxisReference,
                            intent.Word0,
                            intent.Word1,
                            intent.Word2,
                            intent.Word3,
                            key.RequestId,
                            FormatDs402HomeParameters(parameters),
                            DateTime.UtcNow);
                    try
                    {
                        var acknowledgement = await currentAxis.Ds402HomeAsync(
                            prepared,
                            CancellationToken.None);
                        PromoteMaintenanceRecovery(recovery, key.RequestId);
                        TextHomeResult.Text =
                            TranslateUiText(
                                "DS402 Home Start=Accepted; Outcome=NotQueried")
                            + Environment.NewLine
                            + "RequestId="
                            + key.RequestId
                            + ", Method="
                            + acknowledgement.HomingMethod
                            + ", NativeCommandState=0x"
                            + acknowledgement.NativeCommandState.ToString("X8")
                            + Environment.NewLine
                            + TranslateUiText(
                                "Read Home Status performs the exact 0x7D16 outcome query; IsReferenced alone is not completion proof.");
                    }
                    catch (LMCAxisDs402HomeRejectedException rejected)
                    {
                        ResolveMaintenanceConfirmedRejection(
                            recovery,
                            rejected.Acknowledgement.Response.RequestId);
                        throw;
                    }
                    catch
                    {
                        PromoteMaintenanceRecovery(recovery, key.RequestId);
                        throw;
                    }
                });
        }

        private async void ButtonReadHomeStatus_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Home Status",
                async () =>
                {
                    var currentAxis = RequireAxis();
                    var recovery = HasUnresolvedMaintenanceAction
                        ? maintenanceActionRecoveryJournal.CurrentRecord
                        : null;
                    if (recovery != null
                        && recovery.Action
                            == MaintenanceActionKind.LmcHome)
                    {
                        await ReadExactLmcHomeOutcomeAsync(
                            currentAxis,
                            recovery);
                        return;
                    }

                    if (recovery != null
                        && recovery.Action
                            == MaintenanceActionKind.Ds402Home)
                    {
                        await ReadExactDs402HomeOutcomeAsync(
                            currentAxis,
                            recovery);
                        return;
                    }

                    var status = await currentAxis.ReadStatusResultAsync(
                        CancellationToken.None);
                    EnsureAxisStatusSuccess("Read Home Status", status);
                    TextHomeResult.Text =
                        TranslateUiText("Home status read-only sample")
                        + Environment.NewLine
                        + "Axis="
                        + currentAxis.AxisName
                        + ", Ref="
                        + currentAxis.AxisReference
                        + ", IsReferenced="
                        + status.IsReferenced
                        + ", PowerOn="
                        + status.IsPowerOn
                        + ", Standstill="
                        + status.IsStandstill
                        + Environment.NewLine
                        + TranslateUiText(
                            "This sample does not replay Home and does not by itself prove LMC or DS402 Home completion.");
                });
        }

        private async Task ReadExactLmcHomeOutcomeAsync(
            LMCSingleAxis currentAxis,
            MaintenanceActionRecoveryRecord recovery)
        {
            if (!string.Equals(
                    currentAxis.AxisName,
                    recovery.AxisName,
                    StringComparison.Ordinal)
                || currentAxis.AxisReference != recovery.AxisReference)
            {
                throw new InvalidOperationException(
                    "Load the exact Axis from the durable LMC Home recovery record before querying its outcome.");
            }

            var currentConnection = RequireConnection();
            await RefreshMaintenanceCapabilitiesAsync(currentConnection);
            if (recovery.ObservedDiagnosticsBuild
                    != diagnosticCapabilities.DiagnosticsBuild
                || recovery.ObservedMapRevision
                    != diagnosticCapabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "The current diagnostics Build/MapRevision does not match the durable LMC Home recovery key. The outcome remains unresolved.");
            }

            var key = latestLmcHomeRecoveryKey;
            if (!LmcHomeRecoveryKeyMatchesRecord(key, recovery))
            {
                key = RecreateLmcHomeRecoveryKey(recovery);
            }

            var outcome = await currentAxis.ReadLMC_HomeOutcomeAsync(
                key,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            TextHomeResult.Text =
                TranslateUiText("LMC Home Start=Accepted; Outcome=")
                + outcome.RecordState
                + Environment.NewLine
                + "HomeSucceeded="
                + outcome.HomeSucceeded
                + ", OriginalStatus="
                + outcome.OriginalCommandStatus
                + ", OriginalErrorId="
                + outcome.OriginalErrorId
                + ", OriginalDetail="
                + outcome.OriginalDetailCode
                + Environment.NewLine
                + "RawDriveBefore="
                + outcome.RawDrivePositionBefore
                + ", RawDriveAfter="
                + outcome.RawDrivePositionAfter
                + ", ActualApplicationAfter="
                + outcome.ActualApplicationPositionAfter
                + ", SetApplicationAfter="
                + outcome.SetApplicationPositionAfter
                + ", RecordGeneration="
                + outcome.RecordGeneration;
            WriteLog(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "LMC Home outcome: RecordState={0}; HomeSucceeded={1}; OriginalStatus={2}; OriginalErrorId={3}; OriginalDetail={4} ({5}); AxisStatus=0x{6:X8}; AxisError={7}; RawDriveBefore={8}; RawDriveAfter={9}; ActualApplicationAfter={10}; SetApplicationAfter={11}; ActualInternalAfter={12}; SetInternalAfter={13}; DestinationInternalAfter={14}; MasterInternalAfter={15}; NativeCommandState={16}; EvidenceFlags=0x{17:X8}; StopState=0x{18:X8} ({19}); RuntimePhase={20}; RecordGeneration={21}.",
                    outcome.RecordState,
                    outcome.HomeSucceeded,
                    outcome.OriginalCommandStatus,
                    outcome.OriginalErrorId,
                    outcome.OriginalDetailCodeValue,
                    outcome.OriginalDetailCode,
                    outcome.AxisStatus,
                    outcome.AxisError,
                    outcome.RawDrivePositionBefore,
                    outcome.RawDrivePositionAfter,
                    outcome.ActualApplicationPositionAfter,
                    outcome.SetApplicationPositionAfter,
                    outcome.ActualInternalPositionAfter,
                    outcome.SetInternalPositionAfter,
                    outcome.DestinationInternalPositionAfter,
                    outcome.MasterInternalPositionAfter,
                    outcome.NativeCommandState,
                    outcome.EvidenceFlags,
                    outcome.StopState,
                    unchecked((int)outcome.StopState),
                    outcome.RuntimePhase,
                    outcome.RecordGeneration));

            if (!outcome.IsTerminal)
            {
                TextHomeResult.Text += Environment.NewLine
                    + TranslateUiText(
                        "Running is not completion evidence. The durable record remains active and no Home is replayed.");
                return;
            }

            var retirement = await currentAxis.RetireLMC_HomeOutcomeAsync(
                outcome,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            if (!LmcHomeTerminalSnapshotsMatch(outcome, retirement))
            {
                throw new InvalidDataException(
                    "The 0x7D19 retirement snapshot does not exactly match the terminal 0x7D18 outcome. The durable record remains active.");
            }

            maintenanceActionRecoveryJournal.Resolve(
                recovery,
                MaintenanceRecoveryUtcNow(recovery.UpdatedUtc));
            RefreshMaintenanceActionRecoveryUi();
            TextHomeResult.Text += Environment.NewLine
                + TranslateUiText(
                    "Exact terminal 0x7D18 outcome and matching 0x7D19 retirement verified. The LMC Home no-replay record was resolved.");
        }

        private static bool LmcHomeTerminalSnapshotsMatch(
            LMCHomeOutcomeResult outcome,
            LMCHomeOutcomeRetirementResult retirement)
        {
            if (outcome == null
                || !outcome.IsTerminal
                || retirement == null
                || !retirement.RetirementConfirmed
                || retirement.Outcome == null)
            {
                return false;
            }

            var retired = retirement.Outcome;
            return outcome.RecoveryKey.Equals(retired.RecoveryKey)
                && outcome.RecordState == retired.RecordState
                && outcome.OriginalCommandStatus
                    == retired.OriginalCommandStatus
                && outcome.OriginalErrorId == retired.OriginalErrorId
                && outcome.OriginalDetailCodeValue
                    == retired.OriginalDetailCodeValue
                && outcome.AxisStatus == retired.AxisStatus
                && outcome.AxisError == retired.AxisError
                && outcome.RawDrivePositionBefore
                    == retired.RawDrivePositionBefore
                && outcome.RawDrivePositionAfter
                    == retired.RawDrivePositionAfter
                && outcome.ActualApplicationPositionAfter
                    == retired.ActualApplicationPositionAfter
                && outcome.SetApplicationPositionAfter
                    == retired.SetApplicationPositionAfter
                && outcome.ActualInternalPositionAfter
                    == retired.ActualInternalPositionAfter
                && outcome.SetInternalPositionAfter
                    == retired.SetInternalPositionAfter
                && outcome.DestinationInternalPositionAfter
                    == retired.DestinationInternalPositionAfter
                && outcome.MasterInternalPositionAfter
                    == retired.MasterInternalPositionAfter
                && outcome.NativeCommandState == retired.NativeCommandState
                && outcome.EvidenceFlags == retired.EvidenceFlags
                && outcome.StartMilliseconds == retired.StartMilliseconds
                && outcome.CompletionMilliseconds
                    == retired.CompletionMilliseconds
                && outcome.StopState == retired.StopState
                && outcome.RuntimePhase == retired.RuntimePhase
                && outcome.RecordGeneration == retired.RecordGeneration;
        }

        private static bool LmcHomeRecoveryKeyMatchesRecord(
            LMCHomeRecoveryKey key,
            MaintenanceActionRecoveryRecord record)
        {
            return key != null
                && record != null
                && key.OriginalRequestId == record.TransportCorrelationId
                && key.DiagnosticsBuild
                    == record.ObservedDiagnosticsBuild
                && key.OriginalDiagnosticsBootId
                    == record.ObservedDiagnosticsBootId
                && key.MapRevision == record.ObservedMapRevision
                && key.AxisReference == record.AxisReference
                && key.ClientIntentId0 == record.ClientIntentId0
                && key.ClientIntentId1 == record.ClientIntentId1
                && key.ClientIntentId2 == record.ClientIntentId2
                && key.ClientIntentId3 == record.ClientIntentId3;
        }

        private async Task ReadExactDs402HomeOutcomeAsync(
            LMCSingleAxis currentAxis,
            MaintenanceActionRecoveryRecord recovery)
        {
            if (!string.Equals(
                    currentAxis.AxisName,
                    recovery.AxisName,
                    StringComparison.Ordinal)
                || currentAxis.AxisReference != recovery.AxisReference)
            {
                throw new InvalidOperationException(
                    "Load the exact Axis from the durable DS402 Home recovery record before querying its outcome.");
            }

            var currentConnection = RequireConnection();
            await RefreshMaintenanceCapabilitiesAsync(currentConnection);
            if (recovery.ObservedDiagnosticsBuild
                    != diagnosticCapabilities.DiagnosticsBuild
                || recovery.ObservedDiagnosticsBootId
                    != diagnosticCapabilities.DiagnosticsBootId
                || recovery.ObservedMapRevision
                    != diagnosticCapabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "The current diagnostics identity does not match the durable DS402 Home recovery key. The outcome remains unresolved.");
            }

            var key = latestDs402HomeRecoveryKey;
            if (!Ds402RecoveryKeyMatchesRecord(key, recovery))
            {
                key = RecreateDs402RecoveryKey(recovery);
            }

            var outcome = await currentAxis.ReadDs402HomeOutcomeAsync(
                key,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            var phase = outcome.RecordState
                == LMCAxisDs402HomeOutcomeRecordState.Running
                    ? "Running"
                    : "Terminal";
            TextHomeResult.Text =
                TranslateUiText("DS402 Home Start=Accepted; Outcome=")
                + phase
                + Environment.NewLine
                + "RecordState="
                + outcome.RecordState
                + ", HomingSucceeded="
                + outcome.HomingSucceeded
                + ", OriginalStatus="
                + outcome.OriginalCommandStatus
                + ", OriginalErrorId="
                + outcome.OriginalErrorId
                + ", OriginalDetail="
                + outcome.OriginalDetailCode
                + Environment.NewLine
                + "StatusWord=0x"
                + outcome.Ds402StatusWord.ToString("X4")
                + ", ActualPosition="
                + outcome.ActualPosition
                + ", StartCycle="
                + outcome.StartCycle
                + ", CompletionCycle="
                + outcome.CompletionCycle
                + ", RecordGeneration="
                + outcome.RecordGeneration;

            if (!outcome.IsTerminal)
            {
                TextHomeResult.Text += Environment.NewLine
                    + TranslateUiText(
                        "Running is not completion evidence. The durable record remains active and no Home is replayed.");
                return;
            }

            var retirement = await currentAxis
                .RetireDs402HomeOutcomeAsync(
                    outcome,
                    adminCapabilities,
                    diagnosticCapabilities,
                    CancellationToken.None);
            if (!Ds402HomeTerminalSnapshotsMatch(outcome, retirement))
            {
                throw new InvalidDataException(
                    "The 0x7D17 retirement snapshot does not exactly match the terminal 0x7D16 outcome. The durable record remains active.");
            }

            maintenanceActionRecoveryJournal.Resolve(
                recovery,
                MaintenanceRecoveryUtcNow(recovery.UpdatedUtc));
            RefreshMaintenanceActionRecoveryUi();
            TextHomeResult.Text += Environment.NewLine
                + TranslateUiText(
                    "Exact terminal 0x7D16 outcome and matching 0x7D17 retirement verified. The non-moving Home no-replay record was resolved.");
        }

        private static bool Ds402HomeTerminalSnapshotsMatch(
            LMCAxisDs402HomeOutcomeResult outcome,
            LMCAxisDs402HomeOutcomeRetirementResult retirement)
        {
            return outcome != null
                && outcome.IsTerminal
                && retirement != null
                && retirement.RetirementConfirmed
                && Ds402HomeRecoveryKeysMatch(
                    outcome.RecoveryKey,
                    retirement.RecoveryKey)
                && outcome.RecordState == retirement.RecordState
                && outcome.OriginalCommandStatus
                    == retirement.OriginalCommandStatus
                && outcome.OriginalErrorId == retirement.OriginalErrorId
                && outcome.OriginalDetailCodeValue
                    == retirement.OriginalDetailCodeValue
                && outcome.Ds402StatusWord == retirement.Ds402StatusWord
                && outcome.ActualPosition == retirement.ActualPosition
                && outcome.StartCycle == retirement.StartCycle
                && outcome.CompletionCycle == retirement.CompletionCycle
                && outcome.NativeCommandState
                    == retirement.NativeCommandState
                && outcome.RecordGeneration == retirement.RecordGeneration;
        }

        private static bool Ds402HomeRecoveryKeysMatch(
            LMCAxisDs402HomeRecoveryKey first,
            LMCAxisDs402HomeRecoveryKey second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            var firstParameters = first.Parameters;
            var secondParameters = second.Parameters;
            return first.SchemaVersion == second.SchemaVersion
                && first.RequestId == second.RequestId
                && first.DiagnosticsBuild == second.DiagnosticsBuild
                && first.DiagnosticsBootId == second.DiagnosticsBootId
                && first.MapRevision == second.MapRevision
                && first.AxisReference == second.AxisReference
                && first.ClientIntentId != null
                && first.ClientIntentId.Equals(second.ClientIntentId)
                && firstParameters != null
                && secondParameters != null
                && firstParameters.HomingMethod
                    == secondParameters.HomingMethod
                && firstParameters.Position == secondParameters.Position
                && firstParameters.Velocity == secondParameters.Velocity
                && firstParameters.Acceleration
                    == secondParameters.Acceleration
                && firstParameters.DistanceLimit
                    == secondParameters.DistanceLimit
                && firstParameters.TorqueLimit
                    == secondParameters.TorqueLimit
                && firstParameters.BufferMode == secondParameters.BufferMode
                && firstParameters.TimeoutMilliseconds
                    == secondParameters.TimeoutMilliseconds;
        }

        private static bool Ds402RecoveryKeyMatchesRecord(
            LMCAxisDs402HomeRecoveryKey key,
            MaintenanceActionRecoveryRecord record)
        {
            return key != null
                && key.RequestId == record.TransportCorrelationId
                && key.DiagnosticsBuild
                    == record.ObservedDiagnosticsBuild
                && key.DiagnosticsBootId
                    == record.ObservedDiagnosticsBootId
                && key.MapRevision == record.ObservedMapRevision
                && key.AxisReference == record.AxisReference
                && key.ClientIntentId.Word0 == record.ClientIntentId0
                && key.ClientIntentId.Word1 == record.ClientIntentId1
                && key.ClientIntentId.Word2 == record.ClientIntentId2
                && key.ClientIntentId.Word3 == record.ClientIntentId3;
        }

        private static LMCAxisDs402HomeRecoveryKey
            RecreateDs402RecoveryKey(
                MaintenanceActionRecoveryRecord record)
        {
            if (record == null
                || record.Action != MaintenanceActionKind.Ds402Home
                || record.TransportCorrelationId == 0)
            {
                throw new InvalidOperationException(
                    "The durable record is not a reconstructable DS402 Home recovery key.");
            }

            var values = ParseMaintenanceParameters(
                record.ActionParameters);
            if (ReadParameterInt(values, "Method")
                    != LMCAxisDs402HomeParameters
                        .CurrentPositionZeroHomingMethod
                || ReadParameterInt(values, "HomeOffset") != 0
                || ReadParameterInt(values, "Velocity") != 0
                || ReadParameterInt(values, "Acceleration") != 0
                || ReadParameterInt(values, "DistanceLimit") != 0
                || ReadParameterInt(values, "TorqueLimit") != 0
                || !string.Equals(
                    ReadParameter(values, "BufferMode"),
                    LMCDs402HomeBufferMode.Aborting.ToString(),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The durable DS402 Home record is not the exact non-moving method 37/current-position-zero semantic.");
            }

            var parameters = new LMCAxisDs402HomeParameters(
                ReadParameterUInt(values, "TimeoutMs"));
            return new LMCAxisDs402HomeRecoveryKey(
                checked((ushort)ReadParameterUInt(values, "Schema")),
                record.TransportCorrelationId,
                record.ObservedDiagnosticsBuild,
                record.ObservedDiagnosticsBootId,
                record.ObservedMapRevision,
                new LMCAxisDs402HomeClientIntentId(
                    record.ClientIntentId0,
                    record.ClientIntentId1,
                    record.ClientIntentId2,
                    record.ClientIntentId3),
                record.AxisReference,
                parameters);
        }

        internal static LMCHomeRecoveryKey
            RecreateLmcHomeRecoveryKey(
                MaintenanceActionRecoveryRecord record)
        {
            if (record == null
                || record.Action != MaintenanceActionKind.LmcHome
                || record.TransportCorrelationId == 0
                || !record.HasAnyClientIntent)
            {
                throw new InvalidOperationException(
                    "The durable record is not an authoritative LMC Home recovery key.");
            }

            var values = ParseMaintenanceParameters(
                record.ActionParameters);
            if (!string.Equals(
                    ReadParameter(values, "Semantic"),
                    "CurrentPositionZero",
                    StringComparison.Ordinal)
                || ReadParameterInt(values, "TargetPosition") != 0)
            {
                throw new InvalidOperationException(
                    "The durable LMC Home semantic identity is invalid.");
            }

            return new LMCHomeRecoveryKey(
                checked((ushort)ReadParameterUInt(values, "Schema")),
                record.TransportCorrelationId,
                record.ObservedDiagnosticsBuild,
                record.ObservedDiagnosticsBootId,
                record.ObservedMapRevision,
                record.ClientIntentId0,
                record.ClientIntentId1,
                record.ClientIntentId2,
                record.ClientIntentId3,
                record.AxisReference,
                ReadParameterInt(values, "ExpectedActualPosition"),
                checked((int)ReadParameterUInt(values, "TimeoutMs")));
        }

        private static bool IsEncoderMaintenanceAction(
            MaintenanceActionKind action)
        {
            return action
                    == MaintenanceActionKind
                        .EncoderTw20ErrorWarningReset
                || action
                    == MaintenanceActionKind
                        .EncoderTw19MultiturnPositionReset;
        }

        private static MaintenanceActionKind
            MaintenanceActionForEncoderKind(
                LMCEncoderMaintenanceKind kind)
        {
            switch (kind)
            {
                case LMCEncoderMaintenanceKind.Tw20ErrorWarningReset:
                    return MaintenanceActionKind
                        .EncoderTw20ErrorWarningReset;
                case LMCEncoderMaintenanceKind
                    .Tw19MultiturnPositionReset:
                    return MaintenanceActionKind
                        .EncoderTw19MultiturnPositionReset;
                default:
                    throw new ArgumentOutOfRangeException("kind");
            }
        }

        private static LMCDiagnosticCapability
            RequiredEncoderMaintenanceCapability(
                LMCEncoderMaintenanceKind kind)
        {
            switch (kind)
            {
                case LMCEncoderMaintenanceKind.Tw20ErrorWarningReset:
                    return LMCDiagnosticCapability
                        .EncoderTw20ErrorWarningReset;
                case LMCEncoderMaintenanceKind
                    .Tw19MultiturnPositionReset:
                    return LMCDiagnosticCapability
                        .EncoderTw19MultiturnPositionReset;
                default:
                    throw new ArgumentOutOfRangeException("kind");
            }
        }

        internal static LMCEncoderMaintenanceRecoveryKey
            RecreateEncoderMaintenanceRecoveryKey(
                MaintenanceActionRecoveryRecord record)
        {
            if (record == null
                || !IsEncoderMaintenanceAction(record.Action)
                || record.TransportCorrelationId == 0
                || !record.HasAnyClientIntent)
            {
                throw new InvalidOperationException(
                    "The durable record is not an authoritative encoder-maintenance recovery key.");
            }

            var values = ParseMaintenanceParameters(
                record.ActionParameters);
            var kind = (LMCEncoderMaintenanceKind)checked(
                (ushort)ReadParameterUInt(values, "Kind"));
            if (MaintenanceActionForEncoderKind(kind) != record.Action)
            {
                throw new InvalidOperationException(
                    "The durable encoder-maintenance kind does not match its action.");
            }

            var expectedSemantic = kind
                == LMCEncoderMaintenanceKind.Tw20ErrorWarningReset
                    ? "Tw20ErrorWarningReset"
                    : "Tw19MultiturnPositionReset";
            var expectedSubIndex = kind
                == LMCEncoderMaintenanceKind.Tw20ErrorWarningReset
                    ? "0x02"
                    : "0x01";
            var socketValue = ReadParameterUInt(values, "Socket");
            if (!string.Equals(
                    ReadParameter(values, "Semantic"),
                    expectedSemantic,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ReadParameter(values, "Object"),
                    "0x20FC",
                    StringComparison.Ordinal)
                || !string.Equals(
                    ReadParameter(values, "Sub"),
                    expectedSubIndex,
                    StringComparison.Ordinal)
                || !string.Equals(
                    ReadParameter(values, "Type"),
                    "UInt16",
                    StringComparison.Ordinal)
                || ReadParameterUInt(values, "Drive")
                    != record.AxisReference
                || ReadParameterUInt(values, "CommandValue")
                    != LMCEncoderMaintenanceSdoContract.ResetCommandValue)
            {
                throw new InvalidOperationException(
                    "The durable encoder-maintenance semantic identity is invalid.");
            }

            return new LMCEncoderMaintenanceRecoveryKey(
                checked((ushort)ReadParameterUInt(values, "Schema")),
                record.TransportCorrelationId,
                record.ObservedDiagnosticsBuild,
                record.ObservedDiagnosticsBootId,
                record.ObservedMapRevision,
                new LMCEncoderMaintenanceClientIntentId(
                    record.ClientIntentId0,
                    record.ClientIntentId1,
                    record.ClientIntentId2,
                    record.ClientIntentId3),
                kind,
                checked((ushort)ReadParameterUInt(values, "Profile")),
                record.AxisReference,
                (LMCEncoderFeedbackSocket)socketValue,
                ReadParameterUInt(values, "TimeoutMilliseconds"),
                new LMCEncoderMaintenanceCompatibilityEvidenceId(
                    ReadParameterUInt(values, "Evidence0"),
                    ReadParameterUInt(values, "Evidence1"),
                    ReadParameterUInt(values, "Evidence2"),
                    ReadParameterUInt(values, "Evidence3")));
        }

        private static bool EncoderMaintenanceRecoveryKeyMatchesRecord(
            LMCEncoderMaintenanceRecoveryKey key,
            MaintenanceActionRecoveryRecord record)
        {
            return key != null
                && record != null
                && IsEncoderMaintenanceAction(record.Action)
                && MaintenanceActionForEncoderKind(key.Kind)
                    == record.Action
                && key.OriginalRequestId
                    == record.TransportCorrelationId
                && key.DiagnosticsBuild
                    == record.ObservedDiagnosticsBuild
                && key.DiagnosticsBootId
                    == record.ObservedDiagnosticsBootId
                && key.MapRevision == record.ObservedMapRevision
                && key.DriveReference == record.AxisReference
                && key.ClientIntentId.Word0 == record.ClientIntentId0
                && key.ClientIntentId.Word1 == record.ClientIntentId1
                && key.ClientIntentId.Word2 == record.ClientIntentId2
                && key.ClientIntentId.Word3 == record.ClientIntentId3
                && string.Equals(
                    FormatEncoderMaintenanceIdentity(key),
                    record.ActionParameters,
                    StringComparison.Ordinal);
        }

        private static bool EncoderMaintenanceTerminalSnapshotsMatch(
            LMCEncoderMaintenanceOutcomeResult first,
            LMCEncoderMaintenanceOutcomeResult second)
        {
            return first != null
                && second != null
                && first.IsTerminal
                && second.IsTerminal
                && first.RecoveryKey.Equals(second.RecoveryKey)
                && first.RecordState == second.RecordState
                && first.OriginalCommandStatus
                    == second.OriginalCommandStatus
                && first.OriginalErrorId == second.OriginalErrorId
                && first.OriginalDetailCode == second.OriginalDetailCode
                && first.SdoAbortCode == second.SdoAbortCode
                && first.StartCycle == second.StartCycle
                && first.WriteCompletionCycle
                    == second.WriteCompletionCycle
                && first.CompletionCycle == second.CompletionCycle
                && first.ExecutorState == second.ExecutorState
                && first.VerificationFlagsValue
                    == second.VerificationFlagsValue
                && first.PreEvidence0 == second.PreEvidence0
                && first.PostEvidence0 == second.PostEvidence0
                && first.PreEvidence1 == second.PreEvidence1
                && first.PostEvidence1 == second.PostEvidence1
                && first.StatusWord == second.StatusWord
                && first.AxisError == second.AxisError
                && first.DriveErrorCode == second.DriveErrorCode
                && first.ActualPosition == second.ActualPosition
                && first.RecordGeneration == second.RecordGeneration
                && first.OwnerGeneration == second.OwnerGeneration;
        }

        private static IDictionary<string, string>
            ParseMaintenanceParameters(string source)
        {
            var result = new Dictionary<string, string>(
                StringComparer.Ordinal);
            foreach (var segment in source.Split(';'))
            {
                var separator = segment.IndexOf('=');
                if (separator <= 0 || separator == segment.Length - 1)
                {
                    throw new InvalidOperationException(
                        "The durable maintenance parameter record is malformed.");
                }

                var key = segment.Substring(0, separator);
                var value = segment.Substring(separator + 1);
                if (result.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        "The durable maintenance parameter record contains a duplicate field.");
                }

                result.Add(key, value);
            }

            return result;
        }

        private static string ReadParameter(
            IDictionary<string, string> values,
            string name)
        {
            string result;
            if (!values.TryGetValue(name, out result))
            {
                throw new InvalidOperationException(
                    "The durable maintenance parameter record is missing "
                    + name
                    + ".");
            }

            return result;
        }

        private static int ReadParameterInt(
            IDictionary<string, string> values,
            string name)
        {
            return ParseMaintenanceInt(ReadParameter(values, name), name);
        }

        private static uint ReadParameterUInt(
            IDictionary<string, string> values,
            string name)
        {
            return ParseMaintenanceUInt(ReadParameter(values, name), name);
        }

        private void TestResetInput_Changed(
            object sender,
            RoutedEventArgs e)
        {
            if (CheckTestResetPowerOffVerified != null)
            {
                CheckTestResetPowerOffVerified.IsChecked = false;
                CheckTestResetPhysicalPositionVerified.IsChecked = false;
                CheckTestResetExactTargetVerified.IsChecked = false;
                CheckEncoderMaintenanceCompatibilityVerified.IsChecked =
                    false;
            }

            ClearArmedTestReset(
                "The encoder-maintenance input changed; repeat every Step 1 check and arm the exact request again.");
            UpdateUiState();
        }

        private void TestResetConfirmation_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private async void ButtonArmTestReset_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!AreEncoderMaintenanceStepOneChecksConfirmed)
            {
                TextTestResetResult.Text =
                    TranslateUiText(
                        "BLOCKED: all Step 1 physical and encoder compatibility checks are required. No encoder-maintenance operation was armed or sent.");
                return;
            }

            await RunOperationAsync(
                "Arm TEST ONLY Encoder Maintenance",
                async () =>
                {
                    var request = ReadEncoderMaintenanceRequest();
                    var requiredCapability =
                        RequiredEncoderMaintenanceCapability(request.Kind);
                    EnsureMaintenanceActionCanStart(
                        "Arm test-only encoder maintenance",
                        LMCAdminFeature.None,
                        requiredCapability);
                    var currentConnection = RequireConnection();
                    await RefreshMaintenanceCapabilitiesAsync(
                        currentConnection);
                    EnsureMaintenanceActionCanStart(
                        "Arm test-only encoder maintenance",
                        LMCAdminFeature.None,
                        requiredCapability);

                    var selectedAxis = await GetPhysicalAxisAsync(
                        request.DriveReference);
                    var stableStatus =
                        await WaitForStablePowerOffAndStandstillAsync(
                            selectedAxis,
                            1000);
                    EnsureAxisStatusSuccess(
                        "Encoder maintenance Power Off verification",
                        stableStatus);
                    switch (request.Kind)
                    {
                        case LMCEncoderMaintenanceKind
                            .Tw20ErrorWarningReset:
                            armedEncoderMaintenance =
                                currentConnection.Diagnostics
                                .PrepareTw20EncoderErrorWarningReset(
                                    (LMCTw20EncoderErrorWarningResetRequest)
                                        request,
                                    diagnosticCapabilities,
                                    LMCTw20EncoderErrorWarningResetExecuteToken
                                        .Create());
                            break;
                        case LMCEncoderMaintenanceKind
                            .Tw19MultiturnPositionReset:
                            armedEncoderMaintenance =
                                currentConnection.Diagnostics
                                .PrepareTw19MultiturnPositionReset(
                                    (LMCTw19MultiturnPositionResetRequest)
                                        request,
                                    diagnosticCapabilities,
                                    LMCTw19MultiturnPositionResetExecuteToken
                                        .Create());
                            break;
                        default:
                            throw new InvalidOperationException(
                                "Unsupported encoder-maintenance kind.");
                    }

                    armedEncoderMaintenanceFingerprint =
                        FormatEncoderMaintenanceIdentity(
                            armedEncoderMaintenance.RecoveryKey);
                    CheckTestResetFinalConfirmed.IsChecked = false;
                    TextTestResetResult.Text =
                        TranslateUiText(
                            "Step 1 armed in PC memory only; no encoder-maintenance RPC was sent.")
                        + Environment.NewLine
                        + armedEncoderMaintenanceFingerprint
                        + Environment.NewLine
                        + TranslateUiText(
                            "Select Step 2 confirmation to execute this exact request once.");
                });
        }

        private async void ButtonExecuteTestReset_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (armedEncoderMaintenance == null
                || CheckTestResetFinalConfirmed.IsChecked != true)
            {
                TextTestResetResult.Text =
                    TranslateUiText(
                        "BLOCKED: complete every Step 1 check and the final Step 2 confirmation. No encoder-maintenance RPC was sent.");
                return;
            }

            var prepared = armedEncoderMaintenance;
            var fingerprint = armedEncoderMaintenanceFingerprint;
            CheckTestResetFinalConfirmed.IsChecked = false;
            armedEncoderMaintenance = null;
            armedEncoderMaintenanceFingerprint = null;
            await RunOperationAsync(
                "Execute TEST ONLY Encoder Maintenance",
                async () =>
                {
                    if (!string.Equals(
                            fingerprint,
                            FormatEncoderMaintenanceIdentity(
                                prepared.RecoveryKey),
                            StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "The armed encoder-maintenance request changed. No RPC was sent.");
                    }

                    var requiredCapability =
                        RequiredEncoderMaintenanceCapability(
                            prepared.RecoveryKey.Kind);
                    EnsureMaintenanceActionCanStart(
                        "Execute test-only encoder maintenance",
                        LMCAdminFeature.None,
                        requiredCapability);
                    var currentConnection = RequireConnection();
                    var key = prepared.RecoveryKey;
                    var intent = key.ClientIntentId;
                    var selectedAxis = await GetPhysicalAxisAsync(
                        key.DriveReference);
                    var recovery = maintenanceActionRecoveryJournal
                        .ArmBeforeDispatch(
                            MaintenanceActionForEncoderKind(key.Kind),
                            RequiredText(TextRemoteIp.Text, "PLC IP"),
                            ParsePort(TextRemotePort.Text, "TCP port", false),
                            key.DiagnosticsBuild,
                            key.DiagnosticsBootId,
                            key.MapRevision,
                            selectedAxis.AxisName,
                            key.DriveReference,
                            intent.Word0,
                            intent.Word1,
                            intent.Word2,
                            intent.Word3,
                            key.OriginalRequestId,
                            fingerprint,
                            DateTime.UtcNow);
                    try
                    {
                        var acknowledgement = await currentConnection
                            .Diagnostics.StartEncoderMaintenanceAsync(
                                prepared,
                                CancellationToken.None);
                        latestEncoderMaintenanceRecoveryKey = key;
                        PromoteMaintenanceRecovery(
                            recovery,
                            key.OriginalRequestId);
                        TextTestResetResult.Text =
                            TranslateUiText(
                                "Encoder Maintenance Start=Accepted; Outcome=NotQueried")
                            + Environment.NewLine
                            + "RequestId="
                            + key.OriginalRequestId
                            + ", Kind="
                            + key.Kind
                            + ", RecordGeneration="
                            + acknowledgement.RecordGeneration
                            + ", OwnerGeneration="
                            + acknowledgement.OwnerGeneration
                            + ", StartCycle="
                            + acknowledgement.StartCycle
                            + ", "
                            + fingerprint
                            + Environment.NewLine
                            + TranslateUiText(
                                "Use Read Encoder Maintenance Outcome. Start acceptance is not completion proof and the prepared command must never be replayed.");
                    }
                    catch (LMCEncoderMaintenanceCommandRejectedException)
                    {
                        ResolveMaintenanceConfirmedRejection(
                            recovery,
                            key.OriginalRequestId);
                        latestEncoderMaintenanceRecoveryKey = null;
                        throw;
                    }
                    catch
                    {
                        latestEncoderMaintenanceRecoveryKey = key;
                        PromoteMaintenanceRecovery(
                            recovery,
                            key.OriginalRequestId);
                        throw;
                    }
                });
        }

        private async void ButtonReadTestResetStatus_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Encoder Maintenance Outcome",
                async () =>
                {
                    var recovery = HasUnresolvedMaintenanceAction
                        ? maintenanceActionRecoveryJournal.CurrentRecord
                        : null;
                    if (recovery == null
                        || !IsEncoderMaintenanceAction(recovery.Action))
                    {
                        throw new InvalidOperationException(
                            "No active encoder-maintenance recovery record is available. No replay is permitted.");
                    }

                    var currentConnection = RequireConnection();
                    await RefreshMaintenanceCapabilitiesAsync(
                        currentConnection);
                    var key = latestEncoderMaintenanceRecoveryKey;
                    if (!EncoderMaintenanceRecoveryKeyMatchesRecord(
                            key,
                            recovery))
                    {
                        key = RecreateEncoderMaintenanceRecoveryKey(recovery);
                    }

                    var outcome = await currentConnection.Diagnostics
                        .ReadEncoderMaintenanceOutcomeAsync(
                            key,
                            CancellationToken.None);
                    TextTestResetResult.Text =
                        TranslateUiText(
                            "Encoder Maintenance Outcome")
                        + Environment.NewLine
                        + "RequestId="
                        + key.OriginalRequestId
                        + ", Kind="
                        + outcome.Kind
                        + ", State="
                        + outcome.RecordState
                        + ", OriginalStatus="
                        + outcome.OriginalCommandStatus
                        + ", OriginalErrorId="
                        + outcome.OriginalErrorId
                        + ", OriginalDetail="
                        + outcome.OriginalDetailCode
                        + ", SdoAbort=0x"
                        + outcome.SdoAbortCode.ToString("X8")
                        + Environment.NewLine
                        + "VerificationFlags=0x"
                        + outcome.VerificationFlagsValue.ToString("X8")
                        + ", StatusWord=0x"
                        + outcome.StatusWord.ToString("X4")
                        + ", AxisError="
                        + outcome.AxisError
                        + ", DriveError=0x"
                        + outcome.DriveErrorCode.ToString("X8")
                        + ", ActualPosition="
                        + outcome.ActualPosition;

                    if (!outcome.IsTerminal)
                    {
                        TextTestResetResult.Text += Environment.NewLine
                            + TranslateUiText(
                                "Running is not completion evidence. The durable encoder-maintenance record remains active and no command is replayed.");
                        return;
                    }

                    var retirement = await currentConnection.Diagnostics
                        .RetireEncoderMaintenanceOutcomeAsync(
                            outcome,
                            CancellationToken.None);
                    if (!EncoderMaintenanceTerminalSnapshotsMatch(
                            outcome,
                            retirement.TerminalOutcome))
                    {
                        throw new InvalidDataException(
                            "The 0x7E55 retirement snapshot does not exactly match the terminal 0x7E54 outcome. The durable record remains active.");
                    }

                    maintenanceActionRecoveryJournal.Resolve(
                        recovery,
                        MaintenanceRecoveryUtcNow(recovery.UpdatedUtc));
                    latestEncoderMaintenanceRecoveryKey = null;
                    RefreshMaintenanceActionRecoveryUi();
                    TextTestResetResult.Text += Environment.NewLine
                        + TranslateUiText(
                            "Exact terminal encoder-maintenance outcome and matching retirement verified. The no-replay record was resolved.");
                    TextTestResetResult.Text += Environment.NewLine
                        + TranslateUiText(
                            "PLC terminal proves the exact SDO write completion and cleanup, not the physical encoder effect. Verify the effect independently before further operation.");
                    if (outcome.Kind
                        == LMCEncoderMaintenanceKind
                            .Tw19MultiturnPositionReset)
                    {
                        TextTestResetResult.Text += Environment.NewLine
                            + TranslateUiText(
                                "TW[19] position reset requires successful LMC Home current-position-zero before any subsequent motion.");
                    }
                });
        }

        private async void ButtonResolveMaintenanceRecovery_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CheckMaintenanceRecoveryPhysicallyVerified.IsChecked != true)
            {
                return;
            }

            await RunOperationAsync(
                "Resolve Home/Encoder Maintenance Recovery Record",
                async () =>
                {
                    var journal = maintenanceActionRecoveryJournal;
                    if (journal == null || !journal.HasActiveRecord)
                    {
                        throw new InvalidOperationException(
                            "No active Home/encoder-maintenance recovery record exists.");
                    }

                    var record = journal.CurrentRecord;
                    if (record.Action == MaintenanceActionKind.Ds402Home
                        || record.Action == MaintenanceActionKind.LmcHome
                        || IsEncoderMaintenanceAction(record.Action))
                    {
                        throw new InvalidOperationException(
                            "LMC Home, DS402 Home, and encoder maintenance require their exact terminal outcome queries and retirement. Manual operator resolution is disabled.");
                    }

                    var currentConnection = RequireConnection();
                    if (!string.Equals(
                            record.EndpointIp,
                            RequiredText(TextRemoteIp.Text, "PLC IP"),
                            StringComparison.Ordinal)
                        || record.EndpointPort
                            != ParsePort(
                                TextRemotePort.Text,
                                "TCP port",
                                false))
                    {
                        throw new InvalidOperationException(
                            "The current endpoint does not match the durable Home/encoder-maintenance recovery record.");
                    }

                    await RefreshMaintenanceCapabilitiesAsync(
                        currentConnection);
                    if (record.ObservedDiagnosticsBuild
                            != diagnosticCapabilities.DiagnosticsBuild
                        || record.ObservedDiagnosticsBootId
                            != diagnosticCapabilities.DiagnosticsBootId
                        || record.ObservedMapRevision
                            != diagnosticCapabilities.MapRevision)
                    {
                        throw new InvalidOperationException(
                            "The current diagnostics Build/BootId/MapRevision does not match the durable Home/encoder-maintenance recovery identity.");
                    }

                    var recoveryAxis = await GetPhysicalAxisAsync(
                        record.AxisReference);
                    if (!string.Equals(
                            recoveryAxis.AxisName,
                            record.AxisName,
                            StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "The current Axis name/reference does not match the durable Home/encoder-maintenance recovery identity.");
                    }

                    var status = await recoveryAxis.ReadStatusResultAsync(
                        CancellationToken.None);
                    EnsureAxisStatusSuccess(
                        "Resolve Home/encoder-maintenance recovery inspection",
                        status);
                    journal.Resolve(
                        record,
                        MaintenanceRecoveryUtcNow(record.UpdatedUtc));
                    CheckMaintenanceRecoveryPhysicallyVerified.IsChecked =
                        false;
                    TextHomeResult.Text =
                        TranslateUiText(
                            "Recovery record resolved without sending a command. Last read-only status: IsReferenced=")
                        + status.IsReferenced
                        + ", PowerOn="
                        + status.IsPowerOn
                        + ", Standstill="
                        + status.IsStandstill
                        + ".";
                    TextTestResetResult.Text =
                        TranslateUiText(
                            "Recovery record resolved without command replay. Physical state and Home remain separate checks.");
                    RefreshMaintenanceActionRecoveryUi();
                });
        }

        private void MaintenanceRecoveryConfirmation_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private void PromoteMaintenanceRecovery(
            MaintenanceActionRecoveryRecord expected,
            uint correlationId)
        {
            try
            {
                var current = maintenanceActionRecoveryJournal.CurrentRecord;
                if (current != null
                    && current.State
                        == MaintenanceActionRecoveryState.RecoveryRequired)
                {
                    if (current.Identity != expected.Identity
                        || (current.TransportCorrelationId != 0
                            && correlationId != 0
                            && current.TransportCorrelationId
                                != correlationId))
                    {
                        throw new InvalidOperationException(
                            "The active maintenance recovery record does not match the command being promoted.");
                    }

                    return;
                }

                maintenanceActionRecoveryJournal
                    .PromoteToRecoveryRequired(
                        expected,
                        correlationId,
                        MaintenanceRecoveryUtcNow(expected.UpdatedUtc));
            }
            catch (Exception error)
            {
                maintenanceActionRecoveryJournalError = error.Message;
                WriteLog(
                    "SAFETY: Home/encoder-maintenance recovery promotion failed; all new mutation commands remain blocked: "
                    + error.Message);
                throw;
            }
            finally
            {
                RefreshMaintenanceActionRecoveryUi();
            }
        }

        private void ResolveMaintenanceConfirmedRejection(
            MaintenanceActionRecoveryRecord expected,
            uint correlationId)
        {
            try
            {
                maintenanceActionRecoveryJournal
                    .ResolveConfirmedRejection(
                        expected,
                        correlationId,
                        MaintenanceRecoveryUtcNow(expected.UpdatedUtc));
            }
            catch (Exception error)
            {
                maintenanceActionRecoveryJournalError = error.Message;
                WriteLog(
                    "SAFETY: confirmed command rejection could not retire the exact Home/encoder-maintenance recovery record; all new mutations remain blocked: "
                    + error.Message);
                throw;
            }
            finally
            {
                RefreshMaintenanceActionRecoveryUi();
            }
        }

        private void EnsureMaintenanceActionCanStart(
            string operation,
            LMCAdminFeature requiredAdminFeature,
            LMCDiagnosticCapability requiredDiagnosticCapability)
        {
            var currentConnection = RequireConnection();
            if (IsRecoveryIdentityReadOnlyConnection(currentConnection))
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked by recovery-identity read-only quarantine.");
            }

            if (!MaintenanceActionRecoveryJournalCanArm)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the durable Home/encoder-maintenance recovery journal is unavailable or unresolved.");
            }

            if (HasUnresolvedDiagnosticMutation
                || HasUnresolvedAxisPowerState()
                || HasUnresolvedAxisCommandState()
                || HasUnresolvedAxisQualificationState()
                || HasUnresolvedGroupPowerState()
                || HasUnresolvedGroupResetState()
                || HasUnresolvedGroupProfileLockState()
                || motionMayBeActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked by another unresolved mutation, recovery, or motion state.");
            }

            if (requiredAdminFeature != LMCAdminFeature.None
                && (adminCapabilities == null
                    || !adminCapabilities.Supports(requiredAdminFeature)))
            {
                throw new NotSupportedException(
                    "The current Admin capability does not advertise "
                    + requiredAdminFeature
                    + ".");
            }

            if (diagnosticCapabilities == null
                || diagnosticCapabilities.DiagnosticsBuild == 0
                || diagnosticCapabilities.DiagnosticsBootId == 0
                || diagnosticCapabilities.MapRevision == 0
                || (requiredDiagnosticCapability
                        != LMCDiagnosticCapability.None
                    && !diagnosticCapabilities.Supports(
                        requiredDiagnosticCapability)))
            {
                throw new NotSupportedException(
                    "The current Diagnostics capability/identity does not authorize "
                    + operation
                    + ".");
            }
        }

        private void RefreshMaintenanceActionRecoveryUi()
        {
            if (TextMaintenanceRecoveryStatus == null)
            {
                return;
            }

            if (MaintenanceActionRecoveryJournalUnavailable)
            {
                TextMaintenanceRecoveryStatus.Text =
                    TranslateUiText(
                        "SAFETY: Home/encoder-maintenance recovery journal unavailable. No Home or encoder-maintenance command is permitted. ")
                    + (maintenanceActionRecoveryJournalError
                        ?? "Unknown journal error.");
                return;
            }

            var record = maintenanceActionRecoveryJournal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                TextMaintenanceRecoveryStatus.Text =
                    TranslateUiText(
                        "No unresolved Home/encoder-maintenance recovery record. Commands still require live capability and explicit confirmation.");
                return;
            }

            TextMaintenanceRecoveryStatus.Text =
                TranslateUiText(
                    "SAFETY: NO-REPLAY RECOVERY ACTIVE. Action=")
                + record.Action
                + ", State="
                + record.State
                + ", Endpoint="
                + record.EndpointIp
                + ":"
                + record.EndpointPort
                + ", Axis="
                + record.AxisName
                + "/"
                + record.AxisReference
                + ", Build/Boot/Map=0x"
                + record.ObservedDiagnosticsBuild.ToString("X8")
                + "/0x"
                + record.ObservedDiagnosticsBootId.ToString("X8")
                + "/0x"
                + record.ObservedMapRevision.ToString("X8")
                + ", Correlation="
                + record.TransportCorrelationId
                + ". The previous action will not be replayed.";
            if (record.Action == MaintenanceActionKind.Ds402Home)
            {
                TextMaintenanceRecoveryStatus.Text += " "
                    + TranslateUiText(
                        "DS402 Home requires Read Home Status to obtain the exact terminal 0x7D16 outcome; manual record resolution is disabled.");
            }
            else if (record.Action == MaintenanceActionKind.LmcHome)
            {
                TextMaintenanceRecoveryStatus.Text += " "
                    + TranslateUiText(
                        "LMC Home requires Read Home Status to obtain the exact terminal 0x7D18 outcome; manual record resolution is disabled.");
            }
            else if (IsEncoderMaintenanceAction(record.Action))
            {
                TextMaintenanceRecoveryStatus.Text += " "
                    + TranslateUiText(
                        "Encoder maintenance requires Read Encoder Maintenance Outcome and exact terminal retirement; manual record resolution is disabled.");
            }
        }

        private string GetMaintenanceActionGlobalWarning()
        {
            var record = maintenanceActionRecoveryJournal == null
                ? null
                : maintenanceActionRecoveryJournal.CurrentRecord;
            return TranslateUiText(
                    "SAFETY: HOME/ENCODER-MAINTENANCE NO-REPLAY QUARANTINE. ")
                + (record == null
                    ? TranslateUiText(
                        "The durable recovery journal is unavailable; Home and encoder-maintenance commands remain blocked.")
                    : TranslateUiText("Unresolved action=")
                        + record.Action
                        + ", "
                        + TranslateUiText("axis=")
                        + record.AxisName
                        + "/"
                        + record.AxisReference
                        + ". "
                        + TranslateUiText(
                            "Do not replay it. Use exact outcome/status evidence or explicit physical operator retirement as permitted by the action contract."));
        }

        private void ClearArmedTestReset(string reason)
        {
            if (armedEncoderMaintenance == null)
            {
                return;
            }

            armedEncoderMaintenance = null;
            armedEncoderMaintenanceFingerprint = null;
            if (CheckTestResetFinalConfirmed != null)
            {
                CheckTestResetFinalConfirmed.IsChecked = false;
            }
            if (TextTestResetResult != null)
            {
                TextTestResetResult.Text = TranslateUiText(reason);
            }
        }

        internal LMCHomeParameters ReadLmcHomeParameters(
            int expectedActualPosition)
        {
            return new LMCHomeParameters(
                expectedActualPosition,
                ParseMaintenanceInt(
                    TextLmcHomeTimeout.Text,
                    "LMC Home timeout"));
        }

        internal LMCAxisDs402HomeParameters ReadDs402HomeParameters()
        {
            return new LMCAxisDs402HomeParameters(
                ParseMaintenanceUInt(
                    TextDs402HomeTimeout.Text,
                    "DS402 Home timeout"));
        }

        internal LMCEncoderMaintenanceRequest
            ReadEncoderMaintenanceRequest()
        {
            if (!(ComboEncoderMaintenanceKind.SelectedItem
                    is LMCEncoderMaintenanceKind)
                || !(ComboTestResetAxis.SelectedItem is ushort))
            {
                throw new InvalidOperationException(
                    "Encoder-maintenance kind and drive are required.");
            }

            var timeoutMilliseconds = ParseMaintenanceUInt(
                TextTestResetTimeout.Text,
                "Encoder-maintenance timeout");
            if (timeoutMilliseconds > 60000)
            {
                throw new ArgumentOutOfRangeException(
                    "Encoder-maintenance timeout",
                    "Encoder-maintenance timeout milliseconds must be between 1 and 60000.");
            }

            var drive = (ushort)ComboTestResetAxis.SelectedItem;
            var kind = (LMCEncoderMaintenanceKind)
                ComboEncoderMaintenanceKind.SelectedItem;
            if (kind
                == LMCEncoderMaintenanceKind.Tw20ErrorWarningReset)
            {
                return new LMCTw20EncoderErrorWarningResetRequest(
                    drive,
                    timeoutMilliseconds);
            }

            if (kind
                == LMCEncoderMaintenanceKind.Tw19MultiturnPositionReset)
            {
                return new LMCTw19MultiturnPositionResetRequest(
                    drive,
                    timeoutMilliseconds);
            }

            throw new InvalidOperationException(
                "Unsupported encoder-maintenance kind.");
        }

        internal static string FormatLmcHomeIdentity(
            LMCHomeRecoveryKey value)
        {
            return "Schema="
                + value.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture)
                + ";Semantic=CurrentPositionZero;ExpectedActualPosition="
                + value.ExpectedActualPosition.ToString(
                    CultureInfo.InvariantCulture)
                + ";TargetPosition=0"
                + ";TimeoutMs="
                + value.TimeoutMilliseconds.ToString(
                    CultureInfo.InvariantCulture);
        }

        private static string FormatDs402HomeParameters(
            LMCAxisDs402HomeParameters value)
        {
            return "Schema="
                + LMCAdmin.ProtocolSchemaVersion.ToString(
                    CultureInfo.InvariantCulture)
                + ";Method="
                + value.HomingMethod.ToString(CultureInfo.InvariantCulture)
                + ";HomeOffset="
                + value.Position.ToString(CultureInfo.InvariantCulture)
                + ";Velocity="
                + value.Velocity.ToString(CultureInfo.InvariantCulture)
                + ";Acceleration="
                + value.Acceleration.ToString(CultureInfo.InvariantCulture)
                + ";DistanceLimit=0;TorqueLimit=0;BufferMode=Aborting;TimeoutMs="
                + value.TimeoutMilliseconds.ToString(
                    CultureInfo.InvariantCulture);
        }

        internal static string FormatEncoderMaintenanceIdentity(
            LMCEncoderMaintenanceRecoveryKey value)
        {
            var tw20 = value.Kind
                == LMCEncoderMaintenanceKind.Tw20ErrorWarningReset;
            return "Schema="
                + value.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture)
                + ";Semantic="
                + (tw20
                    ? "Tw20ErrorWarningReset"
                    : "Tw19MultiturnPositionReset")
                + ";Kind="
                + ((ushort)value.Kind).ToString(
                    CultureInfo.InvariantCulture)
                + ";Profile="
                + value.CompatibilityProfileId.ToString(
                    CultureInfo.InvariantCulture)
                + ";Drive="
                + value.DriveReference.ToString(CultureInfo.InvariantCulture)
                + ";Socket="
                + ((uint)value.FeedbackSocket).ToString(
                    CultureInfo.InvariantCulture)
                + ";CommandValue="
                + value.CommandValue.ToString(CultureInfo.InvariantCulture)
                + ";Object=0x"
                + value.ObjectIndex.ToString(
                    "X4",
                    CultureInfo.InvariantCulture)
                + ";Sub=0x"
                + value.SubIndex.ToString(
                    "X2",
                    CultureInfo.InvariantCulture)
                + ";Type="
                + value.ValueType.ToString()
                + ";TimeoutMilliseconds="
                + value.TimeoutMilliseconds.ToString(
                    CultureInfo.InvariantCulture)
                + ";Evidence0="
                + value.CompatibilityEvidenceId.Word0.ToString(
                    CultureInfo.InvariantCulture)
                + ";Evidence1="
                + value.CompatibilityEvidenceId.Word1.ToString(
                    CultureInfo.InvariantCulture)
                + ";Evidence2="
                + value.CompatibilityEvidenceId.Word2.ToString(
                    CultureInfo.InvariantCulture)
                + ";Evidence3="
                + value.CompatibilityEvidenceId.Word3.ToString(
                    CultureInfo.InvariantCulture);
        }


        private static int ParseMaintenanceInt(
            string value,
            string fieldName)
        {
            int parsed;
            if (!int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out parsed))
            {
                throw new FormatException(
                    fieldName + " must be a signed 32-bit integer.");
            }

            return parsed;
        }

        private static uint ParseMaintenanceUInt(
            string value,
            string fieldName)
        {
            uint parsed;
            if (!uint.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out parsed)
                || parsed == 0)
            {
                throw new FormatException(
                    fieldName + " must be a positive 32-bit integer.");
            }

            return parsed;
        }

        private static DateTime MaintenanceRecoveryUtcNow(
            DateTime minimumUtc)
        {
            var now = DateTime.UtcNow;
            return now < minimumUtc ? minimumUtc : now;
        }
    }
}

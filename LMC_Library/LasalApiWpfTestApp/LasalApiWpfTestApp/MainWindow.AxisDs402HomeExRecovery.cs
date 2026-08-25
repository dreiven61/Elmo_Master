using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private AxisDs402HomeExRecoveryJournal axisDs402HomeExRecoveryJournal;
        private string axisDs402HomeExRecoveryJournalError;
        private bool axisDs402HomeExRecoveryUiInitialized;
        private bool axisDs402HomeExInterlockHooked;

        private GroupBox groupAxisDs402HomeExRecovery;
        private Button buttonRefreshAxisDs402HomeExCapabilities;
        private Button buttonRecoverAxisDs402HomeEx;
        private TextBlock textAxisDs402HomeExRecoveryStatus;

        static MainWindow()
        {
            EventManager.RegisterClassHandler(
                typeof(MainWindow),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(MainWindowAxisDs402HomeExLoaded),
                true);
        }

        internal AxisDs402HomeExRecoveryJournal
            AxisDs402HomeExRecoveryJournalForTests
        {
            get { return axisDs402HomeExRecoveryJournal; }
        }

        internal AxisDs402HomeExRecoveryRecord
            ActiveAxisDs402HomeExRecoveryRecordForTests
        {
            get
            {
                return HasActiveAxisDs402HomeExRecoveryRecord
                    ? axisDs402HomeExRecoveryJournal.CurrentRecord
                    : null;
            }
        }

        internal bool AxisDs402HomeExRecoveryInterlockForTests
        {
            get { return HasActiveAxisDs402HomeExRecoveryRecord; }
        }

        internal GroupBox AxisDs402HomeExRecoveryGroupForTests
        {
            get { return groupAxisDs402HomeExRecovery; }
        }

        internal Button AxisDs402HomeExRecoverButtonForTests
        {
            get { return buttonRecoverAxisDs402HomeEx; }
        }

        internal void InitializeAxisDs402HomeExRecoveryForTests()
        {
            InitializeAxisDs402HomeExRecoveryUi();
        }

        internal void RefreshAxisDs402HomeExRecoveryForTests()
        {
            RefreshAxisDs402HomeExRecoveryUi();
            ApplyAxisDs402HomeExGlobalInterlock();
        }

        internal AxisDs402HomeExRecoveryRecord
            ArmAxisDs402HomeExRecoveryKeyForTests(
                string endpointIp,
                int endpointPort,
                string axisName,
                LMCAxisDs402HomeExRecoveryKey recoveryKey,
                DateTime createdUtc)
        {
            EnsureAxisDs402HomeExRecoveryJournalCanArm(
                "HomeDS402Ex test pre-dispatch arm");
            var record = axisDs402HomeExRecoveryJournal.ArmBeforeDispatch(
                Guid.NewGuid(),
                endpointIp,
                endpointPort,
                axisName,
                recoveryKey,
                createdUtc);
            RefreshAxisDs402HomeExRecoveryUi();
            ApplyAxisDs402HomeExGlobalInterlock();
            return record;
        }

        internal AxisDs402HomeExRecoveryRecord
            ArmAxisDs402HomeExRecoveryBeforeDispatch(
                LMCPreparedAxisDs402HomeEx preparedCommand)
        {
            if (preparedCommand == null)
            {
                throw new ArgumentNullException("preparedCommand");
            }

            EnsureAxisDs402HomeExRecoveryJournalCanArm(
                "HomeDS402Ex pre-dispatch arm");
            if (HasActiveAxisSetOperationModeRecoveryRecord)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex Start is blocked while SetOperationMode recovery is unresolved.");
            }

            var record = axisDs402HomeExRecoveryJournal.ArmBeforeDispatch(
                Guid.NewGuid(),
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                preparedCommand.Axis.AxisName,
                preparedCommand.RecoveryKey,
                DateTime.UtcNow);
            RefreshAxisDs402HomeExRecoveryUi(
                "ARMED BEFORE DISPATCH. The exact HomeDS402Ex identity is durable. "
                + "This method does not send Start; the caller may cross the Start write boundary only after this return.");
            ApplyAxisDs402HomeExGlobalInterlock();
            return record;
        }

        private static void MainWindowAxisDs402HomeExLoaded(
            object sender,
            RoutedEventArgs e)
        {
            var window = sender as MainWindow;
            if (window != null)
            {
                window.InitializeAxisDs402HomeExRecoveryUi();
            }
        }

        private bool HasActiveAxisDs402HomeExRecoveryRecord
        {
            get
            {
                return axisDs402HomeExRecoveryJournal != null
                    && axisDs402HomeExRecoveryJournal.HasActiveRecord;
            }
        }

        private bool AxisDs402HomeExRecoveryJournalUnavailable
        {
            get
            {
                return axisDs402HomeExRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        axisDs402HomeExRecoveryJournalError);
            }
        }

        private void InitializeAxisDs402HomeExRecoveryUi()
        {
            if (axisDs402HomeExRecoveryUiInitialized)
            {
                RefreshAxisDs402HomeExRecoveryUi();
                ApplyAxisDs402HomeExGlobalInterlock();
                return;
            }

            axisDs402HomeExRecoveryUiInitialized = true;
            CreateAxisDs402HomeExRecoveryControls();
            InitializeAxisDs402HomeExRecoveryJournal();
            HookAxisDs402HomeExUiInterlock();
            RefreshAxisDs402HomeExRecoveryUi();
            ApplyAxisDs402HomeExGlobalInterlock();
        }

        private void InitializeAxisDs402HomeExRecoveryJournal()
        {
            try
            {
                var directory = diagnosticsMutationJournalDirectoryPath == null
                    ? AxisDs402HomeExRecoveryJournal
                        .GetDefaultDirectoryPath()
                    : Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "AxisDs402HomeExRecovery");
                axisDs402HomeExRecoveryJournal =
                    AxisDs402HomeExRecoveryJournal.Open(directory);
                axisDs402HomeExRecoveryJournalError = null;

                if (axisDs402HomeExRecoveryJournal.HasActiveRecord)
                {
                    var record = axisDs402HomeExRecoveryJournal.CurrentRecord;
                    if (TextRemoteIp != null)
                    {
                        TextRemoteIp.Text = record.EndpointIp;
                    }
                    if (TextRemotePort != null)
                    {
                        TextRemotePort.Text = record.EndpointPort.ToString(
                            CultureInfo.InvariantCulture);
                    }
                    WriteLog(
                        "SAFETY: recovered unresolved HomeDS402Ex journal. "
                        + "No HomeDS402Ex Start UI exists and Start replay is blocked. "
                        + "Recovery may use only exact 0x7D1C outcome query and 0x7D1D exact-generation retirement. "
                        + FormatAxisDs402HomeExRecoveryRecord(record));
                }
            }
            catch (Exception error)
            {
                if (axisDs402HomeExRecoveryJournal != null)
                {
                    axisDs402HomeExRecoveryJournal.Dispose();
                }
                axisDs402HomeExRecoveryJournal = null;
                axisDs402HomeExRecoveryJournalError =
                    error.GetType().Name + ": " + error.Message;
                WriteLog(
                    "SAFETY: HomeDS402Ex recovery journal is unavailable. "
                    + "Any future HomeDS402Ex Start integration remains fail-closed: "
                    + axisDs402HomeExRecoveryJournalError);
            }
        }

        private void CreateAxisDs402HomeExRecoveryControls()
        {
            if (groupAxisDs402HomeExRecovery != null)
            {
                return;
            }

            var host = ScrollReadOnlyApi == null
                ? null
                : ScrollReadOnlyApi.Content as Panel;
            if (host == null)
            {
                throw new InvalidOperationException(
                    "Read-only API tab host is unavailable for HomeDS402Ex recovery UI.");
            }

            groupAxisDs402HomeExRecovery = new GroupBox
            {
                Header = "HomeDS402Ex - durable no-replay recovery (Start UI closed)"
            };
            var root = new StackPanel();
            groupAxisDs402HomeExRecovery.Content = root;

            root.Children.Add(new TextBlock
            {
                Foreground = Brushes.DarkRed,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Text = "NO HOMEDS402EX START UI. Engineering scale/profile and LASAL runtime activation are not qualified. "
                    + "This panel only recovers an already durable exact intent through 0x7D1C/0x7D1D."
            });
            root.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 4, 0, 6),
                Foreground = Brushes.DimGray,
                TextWrapping = TextWrapping.Wrap,
                Text = "Recovery is bound to endpoint + DiagnosticsBuild + BootId + MapRevision + RequestId + 128-bit ClientIntentId + axis + every frozen HomeDS402Ex plan field. "
                    + "Terminal proof is persisted before exact-generation retirement; Start is never reconstructed from the journal."
            });

            var actions = new WrapPanel();
            root.Children.Add(actions);
            buttonRefreshAxisDs402HomeExCapabilities = new Button
            {
                Content = "Refresh HomeEx Capabilities"
            };
            buttonRefreshAxisDs402HomeExCapabilities.Click +=
                ButtonRefreshAxisDs402HomeExCapabilities_Click;
            actions.Children.Add(buttonRefreshAxisDs402HomeExCapabilities);

            buttonRecoverAxisDs402HomeEx = new Button
            {
                Content = "Query / Retire HomeEx Recovery (No Start Replay)"
            };
            buttonRecoverAxisDs402HomeEx.Click +=
                ButtonRecoverAxisDs402HomeEx_Click;
            actions.Children.Add(buttonRecoverAxisDs402HomeEx);

            textAxisDs402HomeExRecoveryStatus = new TextBlock
            {
                Margin = new Thickness(0, 4, 0, 0),
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap
            };
            root.Children.Add(textAxisDs402HomeExRecoveryStatus);
            host.Children.Add(groupAxisDs402HomeExRecovery);
        }

        private void HookAxisDs402HomeExUiInterlock()
        {
            if (axisDs402HomeExInterlockHooked
                || GridWorkspaceLayout == null)
            {
                return;
            }

            GridWorkspaceLayout.PreviewMouseDown +=
                AxisDs402HomeExInterlock_PreviewMouseDown;
            GridWorkspaceLayout.PreviewKeyDown +=
                AxisDs402HomeExInterlock_PreviewKeyDown;
            axisDs402HomeExInterlockHooked = true;
        }

        private void AxisDs402HomeExInterlock_PreviewMouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!HasActiveAxisDs402HomeExRecoveryRecord)
            {
                return;
            }

            var button = FindAncestorButton(
                e.OriginalSource as DependencyObject);
            if (button == null || IsAllowedDuringAxisDs402HomeExRecovery(button))
            {
                return;
            }

            e.Handled = true;
            WriteLog(
                "SAFETY: blocked UI command while HomeDS402Ex recovery is unresolved. "
                + "No command was sent; HomeDS402Ex Start replay remains impossible from this UI.");
        }

        private void AxisDs402HomeExInterlock_PreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (!HasActiveAxisDs402HomeExRecoveryRecord
                || (e.Key != Key.Enter && e.Key != Key.Space))
            {
                return;
            }

            var button = Keyboard.FocusedElement as Button;
            if (button == null || IsAllowedDuringAxisDs402HomeExRecovery(button))
            {
                return;
            }

            e.Handled = true;
            WriteLog(
                "SAFETY: blocked keyboard command while HomeDS402Ex recovery is unresolved. "
                + "No command was sent; only read-only recovery and safety actions remain allowed.");
        }

        private bool IsAllowedDuringAxisDs402HomeExRecovery(Button button)
        {
            if (button == null)
            {
                return false;
            }

            if (ReferenceEquals(
                    button,
                    buttonRefreshAxisDs402HomeExCapabilities)
                || ReferenceEquals(button, buttonRecoverAxisDs402HomeEx)
                || ReferenceEquals(
                    button,
                    buttonRefreshAxisSetOperationModeCapabilities)
                || ReferenceEquals(button, buttonRecoverAxisSetOperationMode))
            {
                return true;
            }

            switch (button.Name)
            {
                case "ButtonConnect":
                case "ButtonPowerOff":
                case "ButtonStop":
                case "ButtonGroupPowerOff":
                case "ButtonGroupStop":
                case "ButtonReadStatus":
                case "ButtonReadPosition":
                case "ButtonAdminCapabilities":
                case "ButtonReadAdminAxisParameter":
                case "ButtonReadAdminGroupParameters":
                case "ButtonGetDriveOperationMode":
                case "ButtonReadDriveStatus":
                case "ButtonGetDriveErrorCode":
                    return true;
                default:
                    return false;
            }
        }

        private void ApplyAxisDs402HomeExGlobalInterlock()
        {
            if (!HasActiveAxisDs402HomeExRecoveryRecord
                || GridWorkspaceLayout == null)
            {
                return;
            }

            DisableAxisDs402HomeExBlockedButtonsRecursive(GridWorkspaceLayout);
            if (TextRemoteIp != null)
            {
                TextRemoteIp.IsEnabled = false;
            }
            if (TextRemotePort != null)
            {
                TextRemotePort.IsEnabled = false;
            }
        }

        private void DisableAxisDs402HomeExBlockedButtonsRecursive(
            DependencyObject parent)
        {
            if (parent == null)
            {
                return;
            }

            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var index = 0; index < count; index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                var button = child as Button;
                if (button != null
                    && !IsAllowedDuringAxisDs402HomeExRecovery(button))
                {
                    button.IsEnabled = false;
                }
                DisableAxisDs402HomeExBlockedButtonsRecursive(child);
            }
        }

        private async void ButtonRefreshAxisDs402HomeExCapabilities_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh HomeDS402Ex Capabilities",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    adminCapabilities = await currentConnection.Admin
                        .GetCapabilitiesAsync(CancellationToken.None);
                    await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
                    RefreshAxisDs402HomeExRecoveryUi();
                });
        }

        private async void ButtonRecoverAxisDs402HomeEx_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Recover HomeDS402Ex Outcome",
                RecoverAxisDs402HomeExAsync);
        }

        private async Task RecoverAxisDs402HomeExAsync()
        {
            var record = RequireActiveAxisDs402HomeExRecoveryRecord(
                "HomeDS402Ex recovery");
            var currentConnection = RequireConnection();
            EnsureAxisDs402HomeExRecoveryEndpointMatchesCurrent(
                record,
                "HomeDS402Ex recovery");

            adminCapabilities = await currentConnection.Admin
                .GetCapabilitiesAsync(CancellationToken.None);
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureAxisDs402HomeExCapabilitiesReady("HomeDS402Ex recovery");
            EnsureAxisDs402HomeExRecoveryIdentity(
                record,
                "HomeDS402Ex recovery");

            var currentAxis = await GetPhysicalAxisAsync(record.AxisReference);
            if (!string.Equals(
                    currentAxis.AxisName,
                    record.AxisName,
                    StringComparison.Ordinal)
                || currentAxis.AxisReference != record.AxisReference)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    "HomeDS402Ex recovery axis identity does not match the durable record.");
            }

            if (record.State == AxisDs402HomeExRecoveryState.ArmedBeforeDispatch)
            {
                record = axisDs402HomeExRecoveryJournal
                    .PromoteToRecoveryRequired(
                        record,
                        MonotonicAxisDs402HomeExUtc(record.UpdatedUtc));
            }

            var key = record.ToRecoveryKey();
            if (record.State
                == AxisDs402HomeExRecoveryState.TerminalOutcomeObserved)
            {
                await RetireObservedAxisDs402HomeExOutcomeAsync(
                    currentAxis,
                    record,
                    key);
                return;
            }

            if (record.State != AxisDs402HomeExRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex recovery record is not queryable: "
                    + record.State + ".");
            }

            var outcome = await currentAxis.ReadDs402HomeExOutcomeAsync(
                key,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            if (!outcome.IsTerminal)
            {
                RefreshAxisDs402HomeExRecoveryUi(
                    "OUTCOME RUNNING. QueryRequestId="
                    + outcome.QueryRequestId.ToString(CultureInfo.InvariantCulture)
                    + ". No Start replay or retirement was sent.");
                return;
            }

            record = axisDs402HomeExRecoveryJournal.RecordTerminalOutcome(
                record,
                outcome,
                MonotonicAxisDs402HomeExUtc(record.UpdatedUtc));
            RefreshAxisDs402HomeExRecoveryUi(
                "TERMINAL OUTCOME STORED DURABLY. State="
                + outcome.RecordState
                + ", Generation="
                + outcome.RecordGeneration.ToString(CultureInfo.InvariantCulture)
                + ". Attempting exact-generation 0x7D1D retirement; Start remains unavailable.");

            await RetireObservedAxisDs402HomeExOutcomeAsync(
                currentAxis,
                record,
                key);
        }

        private async Task RetireObservedAxisDs402HomeExOutcomeAsync(
            LMCSingleAxis currentAxis,
            AxisDs402HomeExRecoveryRecord record,
            LMCAxisDs402HomeExRecoveryKey key)
        {
            if (record == null
                || record.State
                    != AxisDs402HomeExRecoveryState.TerminalOutcomeObserved
                || record.TerminalOutcomeProof == null)
            {
                throw new InvalidOperationException(
                    "HomeDS402Ex retirement requires durable terminal proof first.");
            }

            var terminalOutcome = RehydrateAxisDs402HomeExTerminalOutcome(
                key,
                record.TerminalOutcomeProof);
            var retirement = await currentAxis.RetireDs402HomeExOutcomeAsync(
                terminalOutcome,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            var resolved = axisDs402HomeExRecoveryJournal
                .ResolveAfterRetirement(
                    record,
                    retirement,
                    MonotonicAxisDs402HomeExUtc(record.UpdatedUtc));
            RefreshAxisDs402HomeExRecoveryUi(
                "RECOVERY RESOLVED. Terminal outcome="
                + resolved.TerminalOutcomeProof.RecordState
                + ", exact generation="
                + resolved.TerminalOutcomeProof.RecordGeneration.ToString(
                    CultureInfo.InvariantCulture)
                + ", RetireRequestId="
                + resolved.RetirementRequestId.ToString(
                    CultureInfo.InvariantCulture)
                + ". No HomeDS402Ex Start was sent by recovery.");
            WriteLog(
                "HomeDS402Ex recovery resolved only after durable terminal proof and successful exact-generation retirement. "
                + FormatAxisDs402HomeExRecoveryRecord(resolved));
            UpdateUiState();
        }

        private static LMCAxisDs402HomeExOutcomeResult
            RehydrateAxisDs402HomeExTerminalOutcome(
                LMCAxisDs402HomeExRecoveryKey key,
                AxisDs402HomeExTerminalOutcomeProof proof)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (proof == null)
            {
                throw new ArgumentNullException("proof");
            }
            if (proof.QueryRequestId == 0)
            {
                throw new InvalidOperationException(
                    "Durable HomeDS402Ex terminal proof requires the original nonzero outcome-query request id before retirement retry.");
            }

            var transport = new LMC_Response
            {
                IsFrameValid = true,
                HeaderStatus = 0,
                HasCommandResult = true,
                CommandStatus = 0,
                ErrorId = 0,
                PayloadLength = 176,
                Payload = new byte[176]
            };
            var response = new LMCAdminResponse(
                transport,
                key.SchemaVersion,
                0,
                0,
                0,
                proof.QueryRequestId,
                0);
            return new LMCAxisDs402HomeExOutcomeResult(
                response,
                key,
                proof.RecordState,
                proof.OriginalCommandStatus,
                proof.OriginalErrorId,
                proof.OriginalDetailCode,
                proof.Ds402StatusWord,
                proof.ActualPosition,
                proof.ExpectedFinalPosition,
                proof.StartCycle,
                proof.CompletionCycle,
                proof.NativeCommandState,
                proof.RecordGeneration,
                proof.CleanupProofFlags,
                proof.SdoExecutorToken);
        }

        private AxisDs402HomeExRecoveryRecord
            RequireActiveAxisDs402HomeExRecoveryRecord(string operation)
        {
            if (AxisDs402HomeExRecoveryJournalUnavailable)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the HomeDS402Ex recovery journal is unavailable. "
                    + (axisDs402HomeExRecoveryJournalError
                        ?? "No journal was opened."));
            }

            var record = axisDs402HomeExRecoveryJournal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires an unresolved durable HomeDS402Ex record.");
            }
            return record;
        }

        private void EnsureAxisDs402HomeExRecoveryJournalCanArm(
            string operation)
        {
            if (AxisDs402HomeExRecoveryJournalUnavailable)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because durable HomeDS402Ex recovery is unavailable. "
                    + (axisDs402HomeExRecoveryJournalError
                        ?? "No journal was opened."));
            }
            if (axisDs402HomeExRecoveryJournal.HasActiveRecord)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because an unresolved HomeDS402Ex recovery record already exists.");
            }
        }

        private void EnsureAxisDs402HomeExRecoveryEndpointMatchesCurrent(
            AxisDs402HomeExRecoveryRecord record,
            string operation)
        {
            var endpointIp = RequiredConnectedRemoteIp();
            var endpointPort = RequiredConnectedRemotePort();
            if (!string.Equals(
                    record.EndpointIp,
                    endpointIp,
                    StringComparison.OrdinalIgnoreCase)
                || record.EndpointPort != endpointPort)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    operation
                    + " is blocked because the endpoint does not match the durable HomeDS402Ex record. Stored="
                    + record.EndpointIp + ":"
                    + record.EndpointPort.ToString(CultureInfo.InvariantCulture)
                    + ", current=" + endpointIp + ":"
                    + endpointPort.ToString(CultureInfo.InvariantCulture)
                    + ". No outcome query or retirement was sent.");
            }
        }

        private void EnsureAxisDs402HomeExRecoveryIdentity(
            AxisDs402HomeExRecoveryRecord record,
            string operation)
        {
            if (diagnosticCapabilities == null
                || record.DiagnosticsBuild
                    != diagnosticCapabilities.DiagnosticsBuild
                || record.DiagnosticsBootId
                    != diagnosticCapabilities.DiagnosticsBootId
                || record.MapRevision
                    != diagnosticCapabilities.MapRevision)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    operation
                    + " is blocked because DiagnosticsBuild/BootId/MapRevision does not match the durable HomeDS402Ex record. No 0x7D1C/0x7D1D request was sent.");
            }
        }

        private void EnsureAxisDs402HomeExCapabilitiesReady(string operation)
        {
            if (adminCapabilities == null
                || adminCapabilities.Response == null
                || !adminCapabilities.Response.IsSuccess
                || !adminCapabilities.Supports(LMCAdminFeature.AxisDs402HomeEx)
                || adminCapabilities.ErrorCatalogVersion < 7)
            {
                throw new NotSupportedException(
                    operation
                    + " requires Admin feature AxisDs402HomeEx and ErrorCatalogVersion >= 7. The production capability gate remains closed until the PLC advertises both.");
            }

            if (diagnosticCapabilities == null
                || diagnosticCapabilities.Response == null
                || !diagnosticCapabilities.Response.IsSuccess
                || diagnosticCapabilities.DiagnosticsBuild == 0
                || diagnosticCapabilities.DiagnosticsBootId == 0
                || diagnosticCapabilities.MapRevision == 0)
            {
                throw new NotSupportedException(
                    operation
                    + " requires current-session nonzero DiagnosticsBuild, BootId and MapRevision.");
            }
        }

        private void RefreshAxisDs402HomeExRecoveryUi()
        {
            RefreshAxisDs402HomeExRecoveryUi(null);
        }

        private void RefreshAxisDs402HomeExRecoveryUi(string message)
        {
            if (textAxisDs402HomeExRecoveryStatus == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                textAxisDs402HomeExRecoveryStatus.Text = message;
            }
            else if (AxisDs402HomeExRecoveryJournalUnavailable)
            {
                textAxisDs402HomeExRecoveryStatus.Text =
                    "JOURNAL UNAVAILABLE - future HomeDS402Ex Start remains fail-closed. "
                    + (axisDs402HomeExRecoveryJournalError
                        ?? "No journal was opened.");
            }
            else if (!axisDs402HomeExRecoveryJournal.HasActiveRecord)
            {
                textAxisDs402HomeExRecoveryStatus.Text =
                    "No unresolved HomeDS402Ex recovery record. Start UI intentionally remains closed until HOMEEX-01/02/06..11 are qualified.";
            }
            else
            {
                textAxisDs402HomeExRecoveryStatus.Text =
                    FormatAxisDs402HomeExRecoveryRecord(
                        axisDs402HomeExRecoveryJournal.CurrentRecord);
            }

            if (buttonRecoverAxisDs402HomeEx != null)
            {
                buttonRecoverAxisDs402HomeEx.IsEnabled =
                    HasActiveAxisDs402HomeExRecoveryRecord;
            }
            if (buttonRefreshAxisDs402HomeExCapabilities != null)
            {
                buttonRefreshAxisDs402HomeExCapabilities.IsEnabled = true;
            }
        }

        private static string FormatAxisDs402HomeExRecoveryRecord(
            AxisDs402HomeExRecoveryRecord record)
        {
            if (record == null)
            {
                return "HomeDS402Ex recovery record: none";
            }

            return "HomeDS402ExRecovery State=" + record.State
                + ", Revision=" + record.Revision.ToString(CultureInfo.InvariantCulture)
                + ", Endpoint=" + record.EndpointIp + ":"
                + record.EndpointPort.ToString(CultureInfo.InvariantCulture)
                + ", Axis=" + record.AxisName + "#"
                + record.AxisReference.ToString(CultureInfo.InvariantCulture)
                + ", Build/Boot/Map=0x" + record.DiagnosticsBuild.ToString("X8")
                + "/0x" + record.DiagnosticsBootId.ToString("X8")
                + "/0x" + record.MapRevision.ToString("X8")
                + ", RequestId=" + record.OriginalRequestId.ToString(CultureInfo.InvariantCulture)
                + ", Method=" + record.HomingMethod.ToString(CultureInfo.InvariantCulture)
                + ", Position=" + record.Position.ToString(CultureInfo.InvariantCulture)
                + (record.TerminalOutcomeProof == null
                    ? string.Empty
                    : ", Terminal=" + record.TerminalOutcomeProof.RecordState
                        + ", Generation="
                        + record.TerminalOutcomeProof.RecordGeneration.ToString(
                            CultureInfo.InvariantCulture));
        }

        private static DateTime MonotonicAxisDs402HomeExUtc(DateTime previous)
        {
            var now = DateTime.UtcNow;
            return now > previous ? now : previous.AddTicks(1);
        }
    }
}

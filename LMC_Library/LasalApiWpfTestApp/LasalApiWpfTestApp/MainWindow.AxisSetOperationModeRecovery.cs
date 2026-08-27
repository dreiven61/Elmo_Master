using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const LMCAdminFeature AxisSetOperationModeCapabilityTriad =
            LMCAdminFeature.AxisSetOperationModeStart
            | LMCAdminFeature.AxisSetOperationModeOutcomeRead
            | LMCAdminFeature.AxisSetOperationModeOutcomeRetire;

        private AxisSetOperationModeRecoveryJournal
            axisSetOperationModeRecoveryJournal;
        private string axisSetOperationModeRecoveryJournalError;
        private bool axisSetOperationModeUiInterlockHooked;
        private bool axisSetOperationModeInterlockReapplyQueued;

        private GroupBox groupAxisSetOperationModeRecovery;
        private ComboBox comboAxisSetOperationModeReference;
        private ComboBox comboAxisSetOperationModeRequestedMode;
        private TextBox textAxisSetOperationModeTimeout;
        private CheckBox checkAxisSetOperationModeOneShotConfirmed;
        private Button buttonRefreshAxisSetOperationModeCapabilities;
        private Button buttonStartAxisSetOperationMode;
        private Button buttonRecoverAxisSetOperationMode;
        private TextBlock textAxisSetOperationModeRecoveryStatus;

        internal AxisSetOperationModeRecoveryJournal
            AxisSetOperationModeRecoveryJournalForTests
        {
            get { return axisSetOperationModeRecoveryJournal; }
        }

        internal AxisSetOperationModeRecoveryRecord
            ActiveAxisSetOperationModeRecoveryRecordForTests
        {
            get
            {
                return HasActiveAxisSetOperationModeRecoveryRecord
                    ? axisSetOperationModeRecoveryJournal.CurrentRecord
                    : null;
            }
        }

        internal bool AxisSetOperationModeRecoveryInterlockForTests
        {
            get { return HasActiveAxisSetOperationModeRecoveryRecord; }
        }

        internal GroupBox AxisSetOperationModeRecoveryGroupForTests
        {
            get { return groupAxisSetOperationModeRecovery; }
        }

        internal Button AxisSetOperationModeStartButtonForTests
        {
            get { return buttonStartAxisSetOperationMode; }
        }

        internal Button AxisSetOperationModeRecoverButtonForTests
        {
            get { return buttonRecoverAxisSetOperationMode; }
        }

        internal CheckBox AxisSetOperationModeConfirmationForTests
        {
            get { return checkAxisSetOperationModeOneShotConfirmed; }
        }

        internal ComboBox AxisSetOperationModeRequestedModeForTests
        {
            get { return comboAxisSetOperationModeRequestedMode; }
        }

        internal void RefreshAxisSetOperationModeRecoveryUiForTests()
        {
            RefreshAxisSetOperationModeRecoveryUi();
            ApplyAxisSetOperationModeGlobalInterlock();
        }

        private bool HasActiveAxisSetOperationModeRecoveryRecord
        {
            get
            {
                return axisSetOperationModeRecoveryJournal != null
                    && axisSetOperationModeRecoveryJournal.HasActiveRecord;
            }
        }

        private bool AxisSetOperationModeRecoveryJournalUnavailable
        {
            get
            {
                return axisSetOperationModeRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        axisSetOperationModeRecoveryJournalError);
            }
        }

        private bool AxisSetOperationModeRecoveryJournalCanArm
        {
            get
            {
                return !AxisSetOperationModeRecoveryJournalUnavailable
                    && !axisSetOperationModeRecoveryJournal.HasActiveRecord;
            }
        }

        private void InitializeAxisSetOperationModeRecoveryUi(
            ushort[] physicalAxisReferences)
        {
            CreateAxisSetOperationModeRecoveryControls(
                physicalAxisReferences);
            InitializeAxisSetOperationModeRecoveryJournal();
            HookAxisSetOperationModeUiInterlock();
            RefreshAxisSetOperationModeRecoveryUi();
        }

        private void InitializeAxisSetOperationModeRecoveryJournal()
        {
            try
            {
                var directory = diagnosticsMutationJournalDirectoryPath == null
                    ? AxisSetOperationModeRecoveryJournal
                        .GetDefaultDirectoryPath()
                    : Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "AxisSetOperationModeRecovery");
                axisSetOperationModeRecoveryJournal =
                    AxisSetOperationModeRecoveryJournal.Open(directory);
                axisSetOperationModeRecoveryJournalError = null;

                if (axisSetOperationModeRecoveryJournal.HasActiveRecord)
                {
                    var record = axisSetOperationModeRecoveryJournal
                        .CurrentRecord;
                    TextRemoteIp.Text = record.EndpointIp;
                    TextRemotePort.Text = record.EndpointPort.ToString(
                        CultureInfo.InvariantCulture);
                    comboAxisSetOperationModeReference.SelectedItem =
                        record.AxisReference;
                    var recoveredMode =
                        (LMCDriveOperationMode)record.RequestedModeRaw;
                    if (!comboAxisSetOperationModeRequestedMode.Items.Contains(
                            recoveredMode))
                    {
                        comboAxisSetOperationModeRequestedMode.Items.Add(
                            recoveredMode);
                    }
                    comboAxisSetOperationModeRequestedMode.SelectedItem =
                        recoveredMode;
                    textAxisSetOperationModeTimeout.Text =
                        record.TimeoutMilliseconds.ToString(
                            CultureInfo.InvariantCulture);
                    WriteLog(
                        "SAFETY: recovered unresolved SetOperationMode journal. "
                        + "Start replay is blocked. Recovery may use only the "
                        + "exact retained outcome query/retirement path. "
                        + FormatAxisSetOperationModeRecoveryRecord(record));
                }
            }
            catch (Exception error)
            {
                if (axisSetOperationModeRecoveryJournal != null)
                {
                    axisSetOperationModeRecoveryJournal.Dispose();
                }

                axisSetOperationModeRecoveryJournal = null;
                axisSetOperationModeRecoveryJournalError =
                    error.GetType().Name + ": " + error.Message;
                WriteLog(
                    "SAFETY: SetOperationMode recovery journal is unavailable. "
                    + "New SetOperationMode Start remains fail-closed: "
                    + axisSetOperationModeRecoveryJournalError);
            }
        }

        private void DisposeAxisSetOperationModeRecoveryJournal()
        {
            var journal = axisSetOperationModeRecoveryJournal;
            axisSetOperationModeRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private void CreateAxisSetOperationModeRecoveryControls(
            ushort[] physicalAxisReferences)
        {
            if (groupAxisSetOperationModeRecovery != null)
            {
                return;
            }

            var host = ScrollReadOnlyApi == null
                ? null
                : ScrollReadOnlyApi.Content as Panel;
            if (host == null)
            {
                throw new InvalidOperationException(
                    "Read-only API tab host is unavailable for SetOperationMode recovery UI.");
            }

            groupAxisSetOperationModeRecovery = new GroupBox
            {
                Header = "Set Operation Mode - PLC-supported target / durable no-replay recovery"
            };
            var root = new StackPanel();
            groupAxisSetOperationModeRecovery.Content = root;

            root.Children.Add(new TextBlock
            {
                Foreground = Brushes.DarkRed,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Text = "LIVE DRIVE MODE WRITE. Start sends 0x7D23 once only. "
                    + "A successful Start response is acceptance only, not mode-change completion. "
                    + "After any uncertain or accepted Start, automatic 0x7D23/0x6060 replay is forbidden."
            });
            root.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 4, 0, 6),
                Foreground = Brushes.DimGray,
                TextWrapping = TextWrapping.Wrap,
                Text = "Recovery is bound to endpoint + DiagnosticsBuild + BootId + MapRevision + "
                    + "128-bit ClientIntentId + RequestId + axis + requested mode. "
                    + "Recovery queries 0x7D24 only; terminal proof is persisted before exact-generation 0x7D25 retirement."
            });

            var inputs = new WrapPanel();
            root.Children.Add(inputs);

            var axisPanel = new StackPanel { Width = 190 };
            axisPanel.Children.Add(new TextBlock
            {
                Text = "Physical axis reference (1..4)",
                Foreground = Brushes.DimGray
            });
            comboAxisSetOperationModeReference = new ComboBox
            {
                Width = 150,
                ItemsSource = physicalAxisReferences,
                SelectedItem = (ushort)1
            };
            comboAxisSetOperationModeReference.SelectionChanged +=
                AxisSetOperationModeInputChanged;
            axisPanel.Children.Add(comboAxisSetOperationModeReference);
            inputs.Children.Add(axisPanel);

            var modePanel = new StackPanel { Width = 240 };
            modePanel.Children.Add(new TextBlock
            {
                Text = "Requested mode (PLC-advertised only)",
                Foreground = Brushes.DimGray
            });
            comboAxisSetOperationModeRequestedMode = new ComboBox
            {
                Width = 220,
                IsEnabled = false
            };
            comboAxisSetOperationModeRequestedMode.SelectionChanged +=
                AxisSetOperationModeInputChanged;
            modePanel.Children.Add(comboAxisSetOperationModeRequestedMode);
            modePanel.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 4, 12, 4),
                Foreground = Brushes.DarkOrange,
                TextWrapping = TextWrapping.Wrap,
                Text = "Software targets are limited to PP(1), PV(3), IP(7), and CSP(8). "
                    + "The selector stays empty until the connected PLC advertises a supported-mode mask. "
                    + "Homing(6) remains owned by HomeDS402/HomeDS402Ex."
            });
            inputs.Children.Add(modePanel);

            var timeoutPanel = new StackPanel { Width = 180 };
            timeoutPanel.Children.Add(new TextBlock
            {
                Text = "Timeout (ms, nonzero)",
                Foreground = Brushes.DimGray
            });
            textAxisSetOperationModeTimeout = new TextBox
            {
                Width = 150,
                Text = "5000"
            };
            textAxisSetOperationModeTimeout.TextChanged +=
                AxisSetOperationModeInputChanged;
            timeoutPanel.Children.Add(textAxisSetOperationModeTimeout);
            inputs.Children.Add(timeoutPanel);

            checkAxisSetOperationModeOneShotConfirmed = new CheckBox
            {
                Margin = new Thickness(0, 4, 0, 6),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Content = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = "I verified the exact drive/axis and understand that this may write DS402 0x6060:0 to the selected PLC-advertised mode once only. "
                        + "If the response or completion is uncertain I will use the durable recovery query and will not send Start again."
                }
            };
            checkAxisSetOperationModeOneShotConfirmed.Checked +=
                AxisSetOperationModeInputChanged;
            checkAxisSetOperationModeOneShotConfirmed.Unchecked +=
                AxisSetOperationModeInputChanged;
            root.Children.Add(checkAxisSetOperationModeOneShotConfirmed);

            var actions = new WrapPanel();
            root.Children.Add(actions);
            buttonRefreshAxisSetOperationModeCapabilities = new Button
            {
                Content = "Refresh Mode Capabilities"
            };
            buttonRefreshAxisSetOperationModeCapabilities.Click +=
                ButtonRefreshAxisSetOperationModeCapabilities_Click;
            actions.Children.Add(buttonRefreshAxisSetOperationModeCapabilities);

            buttonStartAxisSetOperationMode = new Button
            {
                Content = "Start Selected Mode Once (0x7D23)",
                IsEnabled = false
            };
            buttonStartAxisSetOperationMode.Click +=
                ButtonStartAxisSetOperationMode_Click;
            actions.Children.Add(buttonStartAxisSetOperationMode);

            buttonRecoverAxisSetOperationMode = new Button
            {
                Content = "Query / Retire Recovery (No Start Replay)",
                IsEnabled = false
            };
            buttonRecoverAxisSetOperationMode.Click +=
                ButtonRecoverAxisSetOperationMode_Click;
            actions.Children.Add(buttonRecoverAxisSetOperationMode);

            textAxisSetOperationModeRecoveryStatus = new TextBlock
            {
                Margin = new Thickness(0, 4, 0, 0),
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap,
                Text = "SetOperationMode recovery journal is initializing."
            };
            root.Children.Add(textAxisSetOperationModeRecoveryStatus);

            host.Children.Add(groupAxisSetOperationModeRecovery);
        }

        private void HookAxisSetOperationModeUiInterlock()
        {
            if (axisSetOperationModeUiInterlockHooked
                || GridWorkspaceLayout == null)
            {
                return;
            }

            GridWorkspaceLayout.PreviewMouseDown +=
                AxisSetOperationModeInterlock_PreviewMouseDown;
            GridWorkspaceLayout.PreviewKeyDown +=
                AxisSetOperationModeInterlock_PreviewKeyDown;
            axisSetOperationModeUiInterlockHooked = true;
        }

        private void AxisSetOperationModeInterlock_PreviewMouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!HasActiveAxisSetOperationModeRecoveryRecord)
            {
                return;
            }

            var button = FindAncestorButton(e.OriginalSource as DependencyObject);
            if (button == null || IsAllowedDuringAxisSetOperationModeRecovery(button))
            {
                return;
            }

            e.Handled = true;
            WriteLog(
                "SAFETY: blocked UI command while SetOperationMode recovery is unresolved. "
                + "No command was sent; Start replay remains prohibited.");
        }

        private void AxisSetOperationModeInterlock_PreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (!HasActiveAxisSetOperationModeRecoveryRecord
                || (e.Key != Key.Enter && e.Key != Key.Space))
            {
                return;
            }

            var button = Keyboard.FocusedElement as Button;
            if (button == null || IsAllowedDuringAxisSetOperationModeRecovery(button))
            {
                return;
            }

            e.Handled = true;
            WriteLog(
                "SAFETY: blocked keyboard command while SetOperationMode recovery is unresolved. "
                + "No command was sent; Start replay remains prohibited.");
        }

        private static Button FindAncestorButton(DependencyObject current)
        {
            while (current != null)
            {
                var button = current as Button;
                if (button != null)
                {
                    return button;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private bool IsAllowedDuringAxisSetOperationModeRecovery(Button button)
        {
            if (button == null)
            {
                return false;
            }

            if (ReferenceEquals(
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

        private void ApplyAxisSetOperationModeGlobalInterlock()
        {
            if (!HasActiveAxisSetOperationModeRecoveryRecord
                || GridWorkspaceLayout == null)
            {
                return;
            }

            DisableBlockedButtonsRecursive(GridWorkspaceLayout);
            if (TextRemoteIp != null)
            {
                TextRemoteIp.IsEnabled = false;
            }
            if (TextRemotePort != null)
            {
                TextRemotePort.IsEnabled = false;
            }

            if (!axisSetOperationModeInterlockReapplyQueued)
            {
                axisSetOperationModeInterlockReapplyQueued = true;
                Dispatcher.BeginInvoke(
                    DispatcherPriority.ContextIdle,
                    new Action(
                        () =>
                        {
                            axisSetOperationModeInterlockReapplyQueued = false;
                            if (HasActiveAxisSetOperationModeRecoveryRecord)
                            {
                                DisableBlockedButtonsRecursive(
                                    GridWorkspaceLayout);
                            }
                        }));
            }
        }

        private void DisableBlockedButtonsRecursive(DependencyObject parent)
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
                    && !IsAllowedDuringAxisSetOperationModeRecovery(button))
                {
                    button.IsEnabled = false;
                }

                DisableBlockedButtonsRecursive(child);
            }
        }

        private void RefreshAxisSetOperationModeSupportedModeSelector(
            LMCDriveOperationMode? preferredMode = null)
        {
            if (comboAxisSetOperationModeRequestedMode == null
                || HasActiveAxisSetOperationModeRecoveryRecord)
            {
                return;
            }

            LMCDriveOperationMode? previous = preferredMode;
            if (!previous.HasValue
                && comboAxisSetOperationModeRequestedMode.SelectedItem
                    is LMCDriveOperationMode)
            {
                previous = (LMCDriveOperationMode)
                    comboAxisSetOperationModeRequestedMode.SelectedItem;
            }

            comboAxisSetOperationModeRequestedMode.Items.Clear();
            if (adminCapabilities != null
                && adminCapabilities.Response != null
                && adminCapabilities.Response.IsSuccess
                && adminCapabilities.Supports(
                    AxisSetOperationModeCapabilityTriad))
            {
                foreach (var mode in new[]
                {
                    LMCDriveOperationMode.ProfilePosition,
                    LMCDriveOperationMode.ProfileVelocity,
                    LMCDriveOperationMode.InterpolatedPosition,
                    LMCDriveOperationMode.CyclicSynchronousPosition
                })
                {
                    if (adminCapabilities.SupportsSetOperationMode(mode))
                    {
                        comboAxisSetOperationModeRequestedMode.Items.Add(mode);
                    }
                }
            }

            if (previous.HasValue
                && comboAxisSetOperationModeRequestedMode.Items.Contains(
                    previous.Value))
            {
                comboAxisSetOperationModeRequestedMode.SelectedItem =
                    previous.Value;
            }
            else if (comboAxisSetOperationModeRequestedMode.Items.Contains(
                LMCDriveOperationMode.CyclicSynchronousPosition))
            {
                comboAxisSetOperationModeRequestedMode.SelectedItem =
                    LMCDriveOperationMode.CyclicSynchronousPosition;
            }
            else if (comboAxisSetOperationModeRequestedMode.Items.Count > 0)
            {
                comboAxisSetOperationModeRequestedMode.SelectedIndex = 0;
            }
        }

        private void UpdateAxisSetOperationModeRecoveryUiState(
            bool connected,
            bool idle)
        {
            if (groupAxisSetOperationModeRecovery == null)
            {
                return;
            }

            var active = HasActiveAxisSetOperationModeRecoveryRecord;
            var triadReady = HasAxisSetOperationModeCapabilityTriad();
            var diagnosticsReady = HasStableAxisSetOperationModeDiagnosticsIdentity();
            var confirmed = checkAxisSetOperationModeOneShotConfirmed != null
                && checkAxisSetOperationModeOneShotConfirmed.IsChecked == true;
            uint timeout;
            var timeoutValid = TryGetAxisSetOperationModeTimeout(out timeout);
            var axisSelected = comboAxisSetOperationModeReference != null
                && comboAxisSetOperationModeReference.SelectedItem is ushort;
            var modeSelected = comboAxisSetOperationModeRequestedMode != null
                && comboAxisSetOperationModeRequestedMode.SelectedItem
                    is LMCDriveOperationMode
                && adminCapabilities != null
                && adminCapabilities.SupportsSetOperationMode(
                    (LMCDriveOperationMode)
                        comboAxisSetOperationModeRequestedMode.SelectedItem);
            var admissionAllowed = !active
                && EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation)
                    .IsAllowed;

            buttonRefreshAxisSetOperationModeCapabilities.IsEnabled =
                connected && idle;
            buttonStartAxisSetOperationMode.IsEnabled = connected
                && idle
                && admissionAllowed
                && AxisSetOperationModeRecoveryJournalCanArm
                && triadReady
                && diagnosticsReady
                && confirmed
                && timeoutValid
                && axisSelected
                && modeSelected;
            buttonRecoverAxisSetOperationMode.IsEnabled = connected
                && idle
                && active
                && !AxisSetOperationModeRecoveryJournalUnavailable;

            comboAxisSetOperationModeReference.IsEnabled = idle && !active;
            comboAxisSetOperationModeRequestedMode.IsEnabled = idle
                && !active
                && triadReady
                && comboAxisSetOperationModeRequestedMode.Items.Count > 0;
            textAxisSetOperationModeTimeout.IsEnabled = idle && !active;
            checkAxisSetOperationModeOneShotConfirmed.IsEnabled =
                idle && !active;

            RefreshAxisSetOperationModeRecoveryUi();
            ApplyAxisSetOperationModeGlobalInterlock();
        }

        private async void ButtonRefreshAxisSetOperationModeCapabilities_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh SetOperationMode Capabilities",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    adminCapabilities = await currentConnection.Admin
                        .GetCapabilitiesAsync(CancellationToken.None);
                    RefreshAxisSetOperationModeSupportedModeSelector();
                    await RefreshDiagnosticsCapabilitiesAsync(
                        currentConnection);
                    RefreshAxisSetOperationModeRecoveryUi();
                });
        }

        private async void ButtonStartAxisSetOperationMode_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Set Operation Mode Selected Mode Once",
                StartAxisSetOperationModeOnceAsync);
        }

        private async Task StartAxisSetOperationModeOnceAsync()
        {
            if (!AxisSetOperationModeRecoveryJournalCanArm)
            {
                throw new InvalidOperationException(
                    "SetOperationMode Start is blocked because the durable recovery journal is unavailable or unresolved.");
            }

            EnsureNoUnresolvedDiagnosticMutation(
                "SetOperationMode Start");
            if (checkAxisSetOperationModeOneShotConfirmed == null
                || checkAxisSetOperationModeOneShotConfirmed.IsChecked != true)
            {
                throw new InvalidOperationException(
                    "Explicit SetOperationMode one-shot confirmation is required.");
            }

            var timeoutMilliseconds = RequireAxisSetOperationModeTimeout();
            var axisReference = RequireAxisSetOperationModeAxisReference();
            if (comboAxisSetOperationModeRequestedMode == null
                || !(comboAxisSetOperationModeRequestedMode.SelectedItem
                    is LMCDriveOperationMode))
            {
                throw new InvalidOperationException(
                    "Refresh capabilities and select a PLC-advertised SetOperationMode target first.");
            }
            var requestedMode = (LMCDriveOperationMode)
                comboAxisSetOperationModeRequestedMode.SelectedItem;
            var currentConnection = RequireConnection();
            adminCapabilities = await currentConnection.Admin
                .GetCapabilitiesAsync(CancellationToken.None);
            RefreshAxisSetOperationModeSupportedModeSelector(requestedMode);
            if (!adminCapabilities.SupportsSetOperationMode(requestedMode))
            {
                throw new NotSupportedException(
                    "The connected PLC no longer advertises the selected SetOperationMode target. No Start was sent.");
            }
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureAxisSetOperationModeCapabilitiesReady(
                "SetOperationMode Start");

            var currentAxis = await GetPhysicalAxisAsync(axisReference);
            var prepared = currentAxis.PrepareSetOperationMode(
                requestedMode,
                timeoutMilliseconds,
                adminCapabilities,
                diagnosticCapabilities,
                LMCAxisSetOperationModeExecuteToken.Create());
            var record = axisSetOperationModeRecoveryJournal
                .ArmBeforeDispatch(
                    Guid.NewGuid(),
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort(),
                    currentAxis.AxisName,
                    prepared.RecoveryKey,
                    DateTime.UtcNow);
            checkAxisSetOperationModeOneShotConfirmed.IsChecked = false;
            RefreshAxisSetOperationModeRecoveryUi();
            ApplyAxisSetOperationModeGlobalInterlock();

            try
            {
                var acknowledgement = await currentAxis
                    .SetOperationModeAsync(
                        prepared,
                        CancellationToken.None);
                if (acknowledgement == null
                    || !acknowledgement.IsAccepted)
                {
                    throw new InvalidOperationException(
                        "SetOperationMode returned without an accepted acknowledgement.");
                }

                record = PromoteAxisSetOperationModeRecoveryIfArmed(
                    record,
                    "accepted Start acknowledgement");
                WriteLog(
                    "SetOperationMode Start ACK accepted once. This is not completion evidence. "
                    + "The durable record remains active and 0x7D23 replay is blocked.");
                await RecoverAxisSetOperationModeAsync();
            }
            catch (LMCAxisSetOperationModeRejectedException error)
            {
                PromoteAxisSetOperationModeRecoveryIfArmed(
                    record,
                    "definitive Start rejection");
                RefreshAxisSetOperationModeRecoveryUi(
                    "START REJECTED: "
                    + error.Response.DetailCode
                    + ". The durable identity is retained conservatively; no Start replay is allowed. "
                    + "Use the exact outcome query path before any future Start.");
                throw;
            }
            catch (LMCAxisSetOperationModeOutcomeUncertainException)
            {
                PromoteAxisSetOperationModeRecoveryIfArmed(
                    record,
                    "uncertain Start response");
                RefreshAxisSetOperationModeRecoveryUi(
                    "START OUTCOME UNCERTAIN. Reconnect only to the exact stored endpoint/PLC identity and use Query / Retire Recovery. "
                    + "Do not replay 0x7D23 or 0x6060.");
                throw;
            }
            catch
            {
                if (prepared.IsConsumed)
                {
                    PromoteAxisSetOperationModeRecoveryIfArmed(
                        record,
                        "post-write-boundary Start failure");
                }

                throw;
            }
            finally
            {
                UpdateUiState();
            }
        }

        private async void ButtonRecoverAxisSetOperationMode_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Recover SetOperationMode Outcome",
                RecoverAxisSetOperationModeAsync);
        }

        private async Task RecoverAxisSetOperationModeAsync()
        {
            var record = RequireActiveAxisSetOperationModeRecoveryRecord(
                "SetOperationMode recovery");
            var currentConnection = RequireConnection();
            EnsureAxisSetOperationModeRecoveryEndpointMatchesCurrent(
                record,
                "SetOperationMode recovery");

            adminCapabilities = await currentConnection.Admin
                .GetCapabilitiesAsync(CancellationToken.None);
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureAxisSetOperationModeCapabilitiesReady(
                "SetOperationMode recovery");
            EnsureAxisSetOperationModeRecoveryIdentity(
                record,
                "SetOperationMode recovery");

            var currentAxis = await GetPhysicalAxisAsync(
                record.AxisReference);
            if (!string.Equals(
                    currentAxis.AxisName,
                    record.AxisName,
                    StringComparison.Ordinal)
                || currentAxis.AxisReference != record.AxisReference)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    "SetOperationMode recovery axis identity does not match the durable record.");
            }

            if (record.State
                == AxisSetOperationModeRecoveryState.ArmedBeforeDispatch)
            {
                record = axisSetOperationModeRecoveryJournal
                    .PromoteToRecoveryRequired(
                        record,
                        MonotonicAxisSetOperationModeUtc(record.UpdatedUtc));
            }

            var key = record.ToRecoveryKey();
            if (record.State
                == AxisSetOperationModeRecoveryState.TerminalOutcomeObserved)
            {
                await RetireObservedAxisSetOperationModeOutcomeAsync(
                    currentAxis,
                    record,
                    key);
                return;
            }

            if (record.State
                != AxisSetOperationModeRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    "SetOperationMode recovery record is not in a queryable state: "
                    + record.State
                    + ".");
            }

            var outcome = await currentAxis.ReadSetOperationModeOutcomeAsync(
                key,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            if (!outcome.IsTerminal)
            {
                RefreshAxisSetOperationModeRecoveryUi(
                    "OUTCOME RUNNING. QueryRequestId="
                    + outcome.QueryRequestId.ToString(
                        CultureInfo.InvariantCulture)
                    + ". No Start replay or retirement was sent.");
                return;
            }

            record = axisSetOperationModeRecoveryJournal.RecordTerminalOutcome(
                record,
                outcome,
                MonotonicAxisSetOperationModeUtc(record.UpdatedUtc));
            RefreshAxisSetOperationModeRecoveryUi(
                "TERMINAL OUTCOME STORED DURABLY. State="
                + outcome.RecordState
                + ", Generation="
                + outcome.RecordGeneration.ToString(
                    CultureInfo.InvariantCulture)
                + ". Attempting exact-generation 0x7D25 retirement; Start remains blocked.");

            await RetireObservedAxisSetOperationModeOutcomeAsync(
                currentAxis,
                record,
                key);
        }

        private async Task RetireObservedAxisSetOperationModeOutcomeAsync(
            LMCSingleAxis currentAxis,
            AxisSetOperationModeRecoveryRecord record,
            LMCAxisSetOperationModeRecoveryKey key)
        {
            if (record == null
                || record.State
                    != AxisSetOperationModeRecoveryState.TerminalOutcomeObserved
                || record.TerminalOutcomeProof == null)
            {
                throw new InvalidOperationException(
                    "SetOperationMode retirement requires durable terminal proof first.");
            }

            var retirement = await currentAxis
                .RetireSetOperationModeOutcomeAsync(
                    key,
                    record.TerminalOutcomeProof.RecordGeneration,
                    adminCapabilities,
                    diagnosticCapabilities,
                    CancellationToken.None);
            var resolved = axisSetOperationModeRecoveryJournal
                .ResolveAfterRetirement(
                    record,
                    retirement,
                    MonotonicAxisSetOperationModeUtc(record.UpdatedUtc));
            RefreshAxisSetOperationModeRecoveryUi(
                "RECOVERY RESOLVED. Terminal outcome="
                + resolved.TerminalOutcomeProof.RecordState
                + ", exact generation="
                + resolved.TerminalOutcomeProof.RecordGeneration.ToString(
                    CultureInfo.InvariantCulture)
                + ", RetireRequestId="
                + resolved.RetirementRequestId.ToString(
                    CultureInfo.InvariantCulture)
                + ". A future Start requires a new explicit confirmation.");
            WriteLog(
                "SetOperationMode recovery resolved only after durable terminal proof and successful exact-generation retirement. "
                + FormatAxisSetOperationModeRecoveryRecord(resolved));
            UpdateUiState();
        }

        private AxisSetOperationModeRecoveryRecord
            PromoteAxisSetOperationModeRecoveryIfArmed(
                AxisSetOperationModeRecoveryRecord captured,
                string reason)
        {
            if (axisSetOperationModeRecoveryJournal == null)
            {
                return captured;
            }

            var current = axisSetOperationModeRecoveryJournal.CurrentRecord;
            if (current == null
                || !current.IsActive
                || current.State
                    != AxisSetOperationModeRecoveryState.ArmedBeforeDispatch)
            {
                return current ?? captured;
            }

            if (captured != null
                && (current.Identity != captured.Identity
                    || current.Revision != captured.Revision))
            {
                throw new InvalidOperationException(
                    "SetOperationMode recovery identity changed before promotion.");
            }

            var promoted = axisSetOperationModeRecoveryJournal
                .PromoteToRecoveryRequired(
                    current,
                    MonotonicAxisSetOperationModeUtc(current.UpdatedUtc));
            WriteLog(
                "SetOperationMode durable journal promoted to RecoveryRequired after "
                + reason
                + ". No automatic Start replay is permitted.");
            return promoted;
        }

        private AxisSetOperationModeRecoveryRecord
            RequireActiveAxisSetOperationModeRecoveryRecord(
                string operation)
        {
            if (AxisSetOperationModeRecoveryJournalUnavailable)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the SetOperationMode recovery journal is unavailable. "
                    + (axisSetOperationModeRecoveryJournalError
                        ?? "No journal was opened."));
            }

            var record = axisSetOperationModeRecoveryJournal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires an unresolved durable SetOperationMode record.");
            }

            return record;
        }

        private void EnsureAxisSetOperationModeRecoveryEndpointMatchesCurrent(
            AxisSetOperationModeRecoveryRecord record,
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
                    + " is blocked because the connected endpoint does not match the durable SetOperationMode record. Stored="
                    + record.EndpointIp
                    + ":"
                    + record.EndpointPort.ToString(CultureInfo.InvariantCulture)
                    + ", current="
                    + endpointIp
                    + ":"
                    + endpointPort.ToString(CultureInfo.InvariantCulture)
                    + ".");
            }
        }

        private void EnsureAxisSetOperationModeRecoveryIdentity(
            AxisSetOperationModeRecoveryRecord record,
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
                    + " is blocked because DiagnosticsBuild/BootId/MapRevision does not match the durable SetOperationMode record. Stored=0x"
                    + record.DiagnosticsBuild.ToString("X8")
                    + "/0x"
                    + record.DiagnosticsBootId.ToString("X8")
                    + "/0x"
                    + record.MapRevision.ToString("X8")
                    + ", current=0x"
                    + (diagnosticCapabilities == null
                        ? 0u
                        : diagnosticCapabilities.DiagnosticsBuild).ToString("X8")
                    + "/0x"
                    + (diagnosticCapabilities == null
                        ? 0u
                        : diagnosticCapabilities.DiagnosticsBootId).ToString("X8")
                    + "/0x"
                    + (diagnosticCapabilities == null
                        ? 0u
                        : diagnosticCapabilities.MapRevision).ToString("X8")
                    + ". No outcome query or retirement was sent.");
            }
        }

        private bool HasAxisSetOperationModeCapabilityTriad()
        {
            return adminCapabilities != null
                && adminCapabilities.Response != null
                && adminCapabilities.Response.IsSuccess
                && adminCapabilities.Supports(
                    AxisSetOperationModeCapabilityTriad)
                && adminCapabilities.ErrorCatalogVersion >= 6;
        }

        private bool HasStableAxisSetOperationModeDiagnosticsIdentity()
        {
            return diagnosticCapabilities != null
                && diagnosticCapabilities.Response != null
                && diagnosticCapabilities.Response.IsSuccess
                && diagnosticCapabilities.DiagnosticsBuild != 0
                && diagnosticCapabilities.DiagnosticsBootId != 0
                && diagnosticCapabilities.MapRevision != 0;
        }

        private void EnsureAxisSetOperationModeCapabilitiesReady(
            string operation)
        {
            if (!HasAxisSetOperationModeCapabilityTriad())
            {
                throw new NotSupportedException(
                    operation
                    + " requires Admin capability bits Start/OutcomeRead/OutcomeRetire and ErrorCatalogVersion >= 6. "
                    + "The current activation gate remains closed unless the PLC advertises all three bits.");
            }

            if (!HasStableAxisSetOperationModeDiagnosticsIdentity())
            {
                throw new NotSupportedException(
                    operation
                    + " requires current-session nonzero DiagnosticsBuild, BootId, and MapRevision.");
            }
        }

        private ushort RequireAxisSetOperationModeAxisReference()
        {
            if (comboAxisSetOperationModeReference == null
                || !(comboAxisSetOperationModeReference.SelectedItem is ushort))
            {
                throw new InvalidOperationException(
                    "SetOperationMode physical axis reference is required.");
            }

            var value = (ushort)comboAxisSetOperationModeReference.SelectedItem;
            if (value < 1 || value > 4)
            {
                throw new InvalidOperationException(
                    "SetOperationMode physical axis reference must be between 1 and 4.");
            }

            return value;
        }

        private uint RequireAxisSetOperationModeTimeout()
        {
            uint value;
            if (!TryGetAxisSetOperationModeTimeout(out value))
            {
                throw new InvalidOperationException(
                    "SetOperationMode timeout must be a nonzero UInt32 millisecond value.");
            }

            return value;
        }

        private bool TryGetAxisSetOperationModeTimeout(out uint value)
        {
            return uint.TryParse(
                    textAxisSetOperationModeTimeout == null
                        ? string.Empty
                        : textAxisSetOperationModeTimeout.Text.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value)
                && value != 0;
        }

        private void AxisSetOperationModeInputChanged(
            object sender,
            RoutedEventArgs e)
        {
            if (uiInitializationComplete)
            {
                UpdateUiState();
            }
        }

        private void AxisSetOperationModeInputChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (uiInitializationComplete)
            {
                UpdateUiState();
            }
        }

        private void AxisSetOperationModeInputChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (uiInitializationComplete)
            {
                UpdateUiState();
            }
        }

        private void RefreshAxisSetOperationModeRecoveryUi(
            string overrideStatus = null)
        {
            if (textAxisSetOperationModeRecoveryStatus == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(overrideStatus))
            {
                textAxisSetOperationModeRecoveryStatus.Text = overrideStatus;
                return;
            }

            if (AxisSetOperationModeRecoveryJournalUnavailable)
            {
                textAxisSetOperationModeRecoveryStatus.Text =
                    "JOURNAL UNAVAILABLE - START FAIL-CLOSED. "
                    + (axisSetOperationModeRecoveryJournalError
                        ?? "No journal was opened.");
                return;
            }

            var record = axisSetOperationModeRecoveryJournal.CurrentRecord;
            if (record != null && record.IsActive)
            {
                textAxisSetOperationModeRecoveryStatus.Text =
                    "UNRESOLVED / START REPLAY BLOCKED | "
                    + FormatAxisSetOperationModeRecoveryRecord(record);
                return;
            }

            var triad = HasAxisSetOperationModeCapabilityTriad();
            var diagnostics = HasStableAxisSetOperationModeDiagnosticsIdentity();
            textAxisSetOperationModeRecoveryStatus.Text =
                "Journal ready; no unresolved record. AdminTriad="
                + triad
                + ", SupportedModeMask=0x"
                + (adminCapabilities == null
                    ? 0
                    : adminCapabilities.SetOperationModeSupportedMask)
                    .ToString("X4")
                + ", DiagnosticsIdentity="
                + diagnostics
                + ". Current PLC activation is expected to keep Start disabled until bits 8/9/10 are explicitly enabled after MODE-13 evidence passes.";
        }

        private static string FormatAxisSetOperationModeRecoveryRecord(
            AxisSetOperationModeRecoveryRecord record)
        {
            if (record == null)
            {
                return "Record=<null>";
            }

            return "State="
                + record.State
                + ", Rev="
                + record.Revision.ToString(CultureInfo.InvariantCulture)
                + ", Endpoint="
                + record.EndpointIp
                + ":"
                + record.EndpointPort.ToString(CultureInfo.InvariantCulture)
                + ", Axis="
                + record.AxisName
                + "/"
                + record.AxisReference.ToString(CultureInfo.InvariantCulture)
                + ", Mode="
                + record.RequestedModeRaw.ToString(CultureInfo.InvariantCulture)
                + ", RequestId="
                + record.OriginalRequestId.ToString(CultureInfo.InvariantCulture)
                + ", Build/Boot/Map=0x"
                + record.DiagnosticsBuild.ToString("X8")
                + "/0x"
                + record.DiagnosticsBootId.ToString("X8")
                + "/0x"
                + record.MapRevision.ToString("X8")
                + (record.TerminalOutcomeProof == null
                    ? string.Empty
                    : ", Terminal="
                        + record.TerminalOutcomeProof.RecordState
                        + ", Generation="
                        + record.TerminalOutcomeProof.RecordGeneration.ToString(
                            CultureInfo.InvariantCulture));
        }

        private static DateTime MonotonicAxisSetOperationModeUtc(
            DateTime previousUtc)
        {
            var now = DateTime.UtcNow;
            return now < previousUtc ? previousUtc : now;
        }
    }
}

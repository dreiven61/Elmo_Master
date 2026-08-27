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
        private const LMCAdminFeature AxisSetPositionCapabilityTriad =
            LMCAdminFeature.AxisSetPosition
            | LMCAdminFeature.AxisSetPositionOutcomeRead
            | LMCAdminFeature.AxisSetPositionOutcomeRetirement;

        private AxisSetPositionRecoveryJournal axisSetPositionRecoveryJournal;
        private string axisSetPositionRecoveryJournalError;
        private bool axisSetPositionUiInterlockHooked;
        private bool axisSetPositionInterlockReapplyQueued;

        private GroupBox groupAxisSetPositionRecovery;
        private ComboBox comboAxisSetPositionReference;
        private TextBox textAxisSetPositionTarget;
        private TextBox textAxisSetPositionExpectedActual;
        private CheckBox checkAxisSetPositionOneShotConfirmed;
        private Button buttonRefreshAxisSetPositionCapabilities;
        private Button buttonStartAxisSetPosition;
        private Button buttonRecoverAxisSetPosition;
        private TextBlock textAxisSetPositionRecoveryStatus;

        internal AxisSetPositionRecoveryJournal AxisSetPositionRecoveryJournalForTests
        {
            get { return axisSetPositionRecoveryJournal; }
        }

        internal AxisSetPositionRecoveryRecord ActiveAxisSetPositionRecoveryRecordForTests
        {
            get
            {
                return HasActiveAxisSetPositionRecoveryRecord
                    ? axisSetPositionRecoveryJournal.CurrentRecord
                    : null;
            }
        }

        internal bool AxisSetPositionRecoveryInterlockForTests
        {
            get { return HasActiveAxisSetPositionRecoveryRecord; }
        }

        internal GroupBox AxisSetPositionRecoveryGroupForTests
        {
            get { return groupAxisSetPositionRecovery; }
        }

        internal Button AxisSetPositionStartButtonForTests
        {
            get { return buttonStartAxisSetPosition; }
        }

        internal Button AxisSetPositionRecoverButtonForTests
        {
            get { return buttonRecoverAxisSetPosition; }
        }

        internal CheckBox AxisSetPositionConfirmationForTests
        {
            get { return checkAxisSetPositionOneShotConfirmed; }
        }

        internal TextBox AxisSetPositionTargetForTests
        {
            get { return textAxisSetPositionTarget; }
        }

        internal TextBox AxisSetPositionExpectedActualForTests
        {
            get { return textAxisSetPositionExpectedActual; }
        }

        internal ComboBox AxisSetPositionReferenceForTests
        {
            get { return comboAxisSetPositionReference; }
        }

        internal void RefreshAxisSetPositionRecoveryUiForTests()
        {
            RefreshAxisSetPositionRecoveryUi();
            ApplyAxisSetPositionGlobalInterlock();
        }

        private bool HasActiveAxisSetPositionRecoveryRecord
        {
            get
            {
                return axisSetPositionRecoveryJournal != null
                    && axisSetPositionRecoveryJournal.HasActiveRecord;
            }
        }

        private bool AxisSetPositionRecoveryJournalUnavailable
        {
            get
            {
                return axisSetPositionRecoveryJournal == null
                    || !string.IsNullOrEmpty(axisSetPositionRecoveryJournalError);
            }
        }

        private bool AxisSetPositionRecoveryJournalCanArm
        {
            get
            {
                return !AxisSetPositionRecoveryJournalUnavailable
                    && !axisSetPositionRecoveryJournal.HasActiveRecord;
            }
        }

        private void InitializeAxisSetPositionRecoveryUi()
        {
            CreateAxisSetPositionRecoveryControls();
            InitializeAxisSetPositionRecoveryJournal();
            HookAxisSetPositionUiInterlock();
            RefreshAxisSetPositionRecoveryUi();
            ApplyAxisSetPositionGlobalInterlock();
        }

        private void InitializeAxisSetPositionRecoveryJournal()
        {
            try
            {
                var directory = diagnosticsMutationJournalDirectoryPath == null
                    ? AxisSetPositionRecoveryJournal.GetDefaultDirectoryPath()
                    : Path.Combine(
                        diagnosticsMutationJournalDirectoryPath,
                        "AxisSetPositionRecovery");
                axisSetPositionRecoveryJournal =
                    AxisSetPositionRecoveryJournal.Open(directory);
                axisSetPositionRecoveryJournalError = null;

                if (axisSetPositionRecoveryJournal.HasActiveRecord)
                {
                    var record = axisSetPositionRecoveryJournal.CurrentRecord;
                    TextRemoteIp.Text = record.EndpointIp;
                    TextRemotePort.Text = record.EndpointPort.ToString(
                        CultureInfo.InvariantCulture);
                    comboAxisSetPositionReference.SelectedItem =
                        record.AxisReference;
                    textAxisSetPositionTarget.Text =
                        record.TargetPosition.ToString(CultureInfo.InvariantCulture);
                    textAxisSetPositionExpectedActual.Text =
                        record.ExpectedActualPosition.ToString(
                            CultureInfo.InvariantCulture);
                    WriteLog(
                        "SAFETY: recovered unresolved SetPosition journal. "
                        + "0x7D12 Start replay is blocked. Recovery may use only "
                        + "the exact 0x7D14 outcome query and 0x7D1A "
                        + "generation-bound retirement path. "
                        + FormatAxisSetPositionRecoveryRecord(record));
                }
            }
            catch (Exception error)
            {
                if (axisSetPositionRecoveryJournal != null)
                {
                    axisSetPositionRecoveryJournal.Dispose();
                }
                axisSetPositionRecoveryJournal = null;
                axisSetPositionRecoveryJournalError =
                    error.GetType().Name + ": " + error.Message;
                WriteLog(
                    "SAFETY: SetPosition recovery journal is unavailable. "
                    + "New SetPosition Start remains fail-closed: "
                    + axisSetPositionRecoveryJournalError);
            }
        }

        private void DisposeAxisSetPositionRecoveryJournal()
        {
            var journal = axisSetPositionRecoveryJournal;
            axisSetPositionRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private void CreateAxisSetPositionRecoveryControls()
        {
            if (groupAxisSetPositionRecovery != null)
            {
                return;
            }

            var host = ScrollReadOnlyApi == null
                ? null
                : ScrollReadOnlyApi.Content as Panel;
            if (host == null)
            {
                throw new InvalidOperationException(
                    "Read-only API tab host is unavailable for SetPosition recovery UI.");
            }

            groupAxisSetPositionRecovery = new GroupBox
            {
                Header = "Set Position - durable no-replay recovery"
            };
            var root = new StackPanel();
            groupAxisSetPositionRecovery.Content = root;

            root.Children.Add(new TextBlock
            {
                Foreground = Brushes.DarkRed,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Text = "COORDINATE MUTATION. 0x7D12 is one-shot only. "
                    + "The durable recovery record is written before the Start "
                    + "write boundary. Any accepted or uncertain Start blocks "
                    + "automatic replay until exact outcome query/retirement completes."
            });
            root.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 4, 0, 6),
                Foreground = Brushes.DimGray,
                TextWrapping = TextWrapping.Wrap,
                Text = "Recovery is bound to endpoint + DiagnosticsBuild + "
                    + "BootId + MapRevision + RequestId + 128-bit ClientIntentId "
                    + "+ axis + target + expected actual position. "
                    + "Current PLC activation is expected to keep SetPosition "
                    + "disabled until SP-01..03/05/06 are qualified."
            });

            var inputs = new WrapPanel();
            root.Children.Add(inputs);

            var axisPanel = new StackPanel { Width = 190 };
            axisPanel.Children.Add(new TextBlock
            {
                Text = "Physical axis reference (1..4)",
                Foreground = Brushes.DimGray
            });
            comboAxisSetPositionReference = new ComboBox
            {
                Width = 150,
                ItemsSource = new ushort[] { 1, 2, 3, 4 },
                SelectedItem = (ushort)1
            };
            comboAxisSetPositionReference.SelectionChanged +=
                AxisSetPositionInputChanged;
            axisPanel.Children.Add(comboAxisSetPositionReference);
            inputs.Children.Add(axisPanel);

            var targetPanel = new StackPanel { Width = 190 };
            targetPanel.Children.Add(new TextBlock
            {
                Text = "Target position (DINT)",
                Foreground = Brushes.DimGray
            });
            textAxisSetPositionTarget = new TextBox
            {
                Width = 160,
                Text = "0"
            };
            textAxisSetPositionTarget.TextChanged += AxisSetPositionInputChanged;
            targetPanel.Children.Add(textAxisSetPositionTarget);
            inputs.Children.Add(targetPanel);

            var expectedPanel = new StackPanel { Width = 220 };
            expectedPanel.Children.Add(new TextBlock
            {
                Text = "Expected actual position (DINT)",
                Foreground = Brushes.DimGray
            });
            textAxisSetPositionExpectedActual = new TextBox
            {
                Width = 180,
                Text = "0"
            };
            textAxisSetPositionExpectedActual.TextChanged +=
                AxisSetPositionInputChanged;
            expectedPanel.Children.Add(textAxisSetPositionExpectedActual);
            inputs.Children.Add(expectedPanel);

            checkAxisSetPositionOneShotConfirmed = new CheckBox
            {
                Margin = new Thickness(0, 4, 0, 6),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Content = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = "I verified the exact powered drive/axis, target and "
                        + "fresh expected actual position. I understand that "
                        + "SetPosition changes the coordinate origin once only. "
                        + "If the response is uncertain I will use durable "
                        + "recovery and will not send 0x7D12 Start again."
                }
            };
            checkAxisSetPositionOneShotConfirmed.Checked +=
                AxisSetPositionInputChanged;
            checkAxisSetPositionOneShotConfirmed.Unchecked +=
                AxisSetPositionInputChanged;
            root.Children.Add(checkAxisSetPositionOneShotConfirmed);

            var actions = new WrapPanel();
            root.Children.Add(actions);
            buttonRefreshAxisSetPositionCapabilities = new Button
            {
                Content = "Refresh SetPosition Capabilities"
            };
            buttonRefreshAxisSetPositionCapabilities.Click +=
                ButtonRefreshAxisSetPositionCapabilities_Click;
            actions.Children.Add(buttonRefreshAxisSetPositionCapabilities);

            buttonStartAxisSetPosition = new Button
            {
                Content = "Start SetPosition Once (0x7D12)",
                IsEnabled = false
            };
            buttonStartAxisSetPosition.Click +=
                ButtonStartAxisSetPosition_Click;
            actions.Children.Add(buttonStartAxisSetPosition);

            buttonRecoverAxisSetPosition = new Button
            {
                Content = "Query / Retire SetPosition Recovery (No Start Replay)",
                IsEnabled = false
            };
            buttonRecoverAxisSetPosition.Click +=
                ButtonRecoverAxisSetPosition_Click;
            actions.Children.Add(buttonRecoverAxisSetPosition);

            textAxisSetPositionRecoveryStatus = new TextBlock
            {
                Margin = new Thickness(0, 4, 0, 0),
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap,
                Text = "SetPosition recovery journal is initializing."
            };
            root.Children.Add(textAxisSetPositionRecoveryStatus);
            host.Children.Add(groupAxisSetPositionRecovery);
        }

        private void HookAxisSetPositionUiInterlock()
        {
            if (axisSetPositionUiInterlockHooked || GridWorkspaceLayout == null)
            {
                return;
            }

            GridWorkspaceLayout.PreviewMouseDown +=
                AxisSetPositionInterlock_PreviewMouseDown;
            GridWorkspaceLayout.PreviewKeyDown +=
                AxisSetPositionInterlock_PreviewKeyDown;
            axisSetPositionUiInterlockHooked = true;
        }

        private void AxisSetPositionInterlock_PreviewMouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!HasActiveAxisSetPositionRecoveryRecord)
            {
                return;
            }

            var button = FindAncestorButton(e.OriginalSource as DependencyObject);
            if (button == null || IsAllowedDuringAxisSetPositionRecovery(button))
            {
                return;
            }

            e.Handled = true;
            WriteLog(
                "SAFETY: blocked UI command while SetPosition recovery is "
                + "unresolved. No mutation was sent; 0x7D12 replay remains "
                + "prohibited.");
        }

        private void AxisSetPositionInterlock_PreviewKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (!HasActiveAxisSetPositionRecoveryRecord
                || (e.Key != Key.Enter && e.Key != Key.Space))
            {
                return;
            }

            var button = Keyboard.FocusedElement as Button;
            if (button == null || IsAllowedDuringAxisSetPositionRecovery(button))
            {
                return;
            }

            e.Handled = true;
            WriteLog(
                "SAFETY: blocked keyboard command while SetPosition recovery "
                + "is unresolved. No mutation was sent.");
        }

        private bool IsAllowedDuringAxisSetPositionRecovery(Button button)
        {
            if (button == null)
            {
                return false;
            }

            if (ReferenceEquals(button, buttonRefreshAxisSetPositionCapabilities)
                || ReferenceEquals(button, buttonRecoverAxisSetPosition)
                || ReferenceEquals(
                    button,
                    buttonRefreshAxisSetOperationModeCapabilities)
                || ReferenceEquals(button, buttonRecoverAxisSetOperationMode)
                || ReferenceEquals(
                    button,
                    buttonRefreshAxisDs402HomeExCapabilities)
                || ReferenceEquals(button, buttonRecoverAxisDs402HomeEx))
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

        private void ApplyAxisSetPositionGlobalInterlock()
        {
            if (!HasActiveAxisSetPositionRecoveryRecord
                || GridWorkspaceLayout == null)
            {
                return;
            }

            DisableAxisSetPositionBlockedButtonsRecursive(GridWorkspaceLayout);
            TextRemoteIp.IsEnabled = false;
            TextRemotePort.IsEnabled = false;

            if (!axisSetPositionInterlockReapplyQueued)
            {
                axisSetPositionInterlockReapplyQueued = true;
                Dispatcher.BeginInvoke(
                    DispatcherPriority.ContextIdle,
                    new Action(
                        () =>
                        {
                            axisSetPositionInterlockReapplyQueued = false;
                            if (HasActiveAxisSetPositionRecoveryRecord)
                            {
                                DisableAxisSetPositionBlockedButtonsRecursive(
                                    GridWorkspaceLayout);
                                TextRemoteIp.IsEnabled = false;
                                TextRemotePort.IsEnabled = false;
                            }
                        }));
            }
        }

        private void DisableAxisSetPositionBlockedButtonsRecursive(
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
                    && !IsAllowedDuringAxisSetPositionRecovery(button))
                {
                    button.IsEnabled = false;
                }
                DisableAxisSetPositionBlockedButtonsRecursive(child);
            }
        }

        private void UpdateAxisSetPositionRecoveryUiState(bool connected, bool idle)
        {
            if (groupAxisSetPositionRecovery == null)
            {
                return;
            }

            var active = HasActiveAxisSetPositionRecoveryRecord;
            int target;
            int expectedActual;
            var valuesValid = TryGetAxisSetPositionValues(
                out target,
                out expectedActual);
            var axisSelected = comboAxisSetPositionReference != null
                && comboAxisSetPositionReference.SelectedItem is ushort;
            var confirmed = checkAxisSetPositionOneShotConfirmed != null
                && checkAxisSetPositionOneShotConfirmed.IsChecked == true;
            var admissionAllowed = !active
                && EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation.NewLiveOrMutation)
                    .IsAllowed;

            buttonRefreshAxisSetPositionCapabilities.IsEnabled = connected && idle;
            buttonStartAxisSetPosition.IsEnabled = connected
                && idle
                && admissionAllowed
                && AxisSetPositionRecoveryJournalCanArm
                && HasAxisSetPositionCapabilityTriad()
                && HasStableAxisSetPositionDiagnosticsIdentity()
                && confirmed
                && valuesValid
                && axisSelected;
            buttonRecoverAxisSetPosition.IsEnabled = connected
                && idle
                && active
                && !AxisSetPositionRecoveryJournalUnavailable;

            comboAxisSetPositionReference.IsEnabled = idle && !active;
            textAxisSetPositionTarget.IsEnabled = idle && !active;
            textAxisSetPositionExpectedActual.IsEnabled = idle && !active;
            checkAxisSetPositionOneShotConfirmed.IsEnabled = idle && !active;

            RefreshAxisSetPositionRecoveryUi();
            ApplyAxisSetPositionGlobalInterlock();
        }

        private async void ButtonRefreshAxisSetPositionCapabilities_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh SetPosition Capabilities",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    adminCapabilities = await currentConnection.Admin
                        .GetCapabilitiesAsync(CancellationToken.None);
                    await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
                    RefreshAxisSetPositionRecoveryUi();
                });
        }

        private async void ButtonStartAxisSetPosition_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "SetPosition Once",
                StartAxisSetPositionOnceAsync);
        }

        private async Task StartAxisSetPositionOnceAsync()
        {
            if (!AxisSetPositionRecoveryJournalCanArm)
            {
                throw new InvalidOperationException(
                    "SetPosition Start is blocked because the durable recovery "
                    + "journal is unavailable or unresolved.");
            }
            if (HasActiveAxisSetOperationModeRecoveryRecord
                || HasActiveAxisDs402HomeExRecoveryRecord)
            {
                throw new InvalidOperationException(
                    "SetPosition Start is blocked while another durable axis "
                    + "mutation recovery is unresolved.");
            }

            EnsureNoUnresolvedDiagnosticMutation("SetPosition Start");
            if (checkAxisSetPositionOneShotConfirmed == null
                || checkAxisSetPositionOneShotConfirmed.IsChecked != true)
            {
                throw new InvalidOperationException(
                    "Explicit SetPosition one-shot confirmation is required.");
            }

            int targetPosition;
            int expectedActualPosition;
            if (!TryGetAxisSetPositionValues(
                    out targetPosition,
                    out expectedActualPosition))
            {
                throw new InvalidOperationException(
                    "SetPosition target and expected actual position must both "
                    + "be valid DINT values.");
            }

            var axisReference = RequireAxisSetPositionAxisReference();
            var currentConnection = RequireConnection();
            adminCapabilities = await currentConnection.Admin
                .GetCapabilitiesAsync(CancellationToken.None);
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureAxisSetPositionCapabilitiesReady("SetPosition Start");

            var currentAxis = await GetPhysicalAxisAsync(axisReference);
            var prepared = currentAxis.PrepareSetPosition(
                targetPosition,
                expectedActualPosition,
                adminCapabilities,
                diagnosticCapabilities,
                LMCAxisSetPositionExecuteToken.Create());
            var key = prepared.RecoveryKey;
            var record = axisSetPositionRecoveryJournal.ArmBeforeDispatch(
                Guid.NewGuid(),
                RequiredConnectedRemoteIp(),
                RequiredConnectedRemotePort(),
                key.DiagnosticsBuild,
                key.DiagnosticsBootId,
                key.MapRevision,
                currentAxis.AxisName,
                key.AxisReference,
                key.ClientIntentId0,
                key.ClientIntentId1,
                key.ClientIntentId2,
                key.ClientIntentId3,
                key.OriginalRequestId,
                key.TargetPosition,
                key.ExpectedActualPosition,
                (ushort)key.SemanticMode,
                key.SchemaVersion,
                DateTime.UtcNow);
            checkAxisSetPositionOneShotConfirmed.IsChecked = false;
            RefreshAxisSetPositionRecoveryUi();
            ApplyAxisSetPositionGlobalInterlock();

            try
            {
                var result = await currentAxis.SetPositionExAsync(
                    prepared,
                    CancellationToken.None);
                if (result == null || !result.IsSuccess)
                {
                    throw new InvalidOperationException(
                        "SetPosition returned without a successful correlated response.");
                }

                record = PromoteAxisSetPositionRecoveryIfArmed(
                    record,
                    "successful correlated Start response");
                WriteLog(
                    "SetPosition 0x7D12 response received once. The durable "
                    + "record remains active; completion is proven only through "
                    + "0x7D14 terminal outcome and 0x7D1A retirement.");
                await RecoverAxisSetPositionAsync();
            }
            catch (LMCAxisSetPositionRejectedException)
            {
                PromoteAxisSetPositionRecoveryIfArmed(
                    record,
                    "definitive Start rejection");
                RefreshAxisSetPositionRecoveryUi(
                    "START REJECTED. The durable exact identity remains active. "
                    + "Do not replay 0x7D12; use exact outcome query/recovery.");
                throw;
            }
            catch (LMCAxisSetPositionOutcomeUncertainException)
            {
                PromoteAxisSetPositionRecoveryIfArmed(
                    record,
                    "uncertain Start response");
                RefreshAxisSetPositionRecoveryUi(
                    "START OUTCOME UNCERTAIN. Reconnect only to the stored "
                    + "endpoint/PLC identity and use Query / Retire Recovery. "
                    + "Do not replay 0x7D12.");
                throw;
            }
            catch
            {
                if (prepared.IsConsumed)
                {
                    PromoteAxisSetPositionRecoveryIfArmed(
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

        private async void ButtonRecoverAxisSetPosition_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Recover SetPosition Outcome",
                RecoverAxisSetPositionAsync);
        }

        private async Task RecoverAxisSetPositionAsync()
        {
            var record = RequireActiveAxisSetPositionRecoveryRecord(
                "SetPosition recovery");
            var currentConnection = RequireConnection();
            EnsureAxisSetPositionRecoveryEndpointMatchesCurrent(
                record,
                "SetPosition recovery");

            adminCapabilities = await currentConnection.Admin
                .GetCapabilitiesAsync(CancellationToken.None);
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureAxisSetPositionCapabilitiesReady("SetPosition recovery");
            EnsureAxisSetPositionRecoveryIdentity(
                record,
                "SetPosition recovery");

            var currentAxis = await GetPhysicalAxisAsync(record.AxisReference);
            if (!string.Equals(
                    currentAxis.AxisName,
                    record.AxisName,
                    StringComparison.Ordinal)
                || currentAxis.AxisReference != record.AxisReference)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    "SetPosition recovery axis identity does not match the "
                    + "durable record.");
            }

            if (record.State == AxisSetPositionRecoveryState.ArmedBeforeDispatch)
            {
                record = axisSetPositionRecoveryJournal.PromoteToRecoveryRequired(
                    record,
                    MonotonicAxisSetPositionUtc(record.UpdatedUtc));
            }

            var key = ToAxisSetPositionRecoveryKey(record);
            if (record.State
                == AxisSetPositionRecoveryState.TerminalOutcomeObserved)
            {
                await RetireObservedAxisSetPositionOutcomeAsync(
                    currentAxis,
                    record,
                    key);
                return;
            }

            if (record.State != AxisSetPositionRecoveryState.RecoveryRequired)
            {
                throw new InvalidOperationException(
                    "SetPosition recovery record is not queryable: "
                    + record.State + ".");
            }

            var outcome = await currentAxis.ReadSetPositionOutcomeAsync(
                key,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            if (!outcome.IsTerminal)
            {
                RefreshAxisSetPositionRecoveryUi(
                    "OUTCOME RUNNING. QueryRequestId="
                    + outcome.QueryRequestId.ToString(CultureInfo.InvariantCulture)
                    + ". No Start replay or retirement was sent.");
                return;
            }

            record = axisSetPositionRecoveryJournal.RecordTerminalOutcome(
                record,
                outcome,
                MonotonicAxisSetPositionUtc(record.UpdatedUtc));
            RefreshAxisSetPositionRecoveryUi(
                "TERMINAL OUTCOME STORED DURABLY. State="
                + outcome.RecordState
                + ", Generation="
                + outcome.RecordGeneration.ToString(CultureInfo.InvariantCulture)
                + ". Attempting exact-generation 0x7D1A retirement; 0x7D12 "
                + "remains blocked.");

            await RetireObservedAxisSetPositionOutcomeAsync(
                currentAxis,
                record,
                key);
        }

        private async Task RetireObservedAxisSetPositionOutcomeAsync(
            LMCSingleAxis currentAxis,
            AxisSetPositionRecoveryRecord record,
            LMCAxisSetPositionRecoveryKey key)
        {
            if (record == null
                || record.State
                    != AxisSetPositionRecoveryState.TerminalOutcomeObserved
                || record.TerminalOutcomeProof == null)
            {
                throw new InvalidOperationException(
                    "SetPosition retirement requires durable terminal proof first.");
            }

            var retirement = await currentAxis.RetireSetPositionOutcomeAsync(
                key,
                record.TerminalOutcomeProof.RecordGeneration,
                adminCapabilities,
                diagnosticCapabilities,
                CancellationToken.None);
            var resolved = axisSetPositionRecoveryJournal.ResolveAfterRetirement(
                record,
                retirement,
                MonotonicAxisSetPositionUtc(record.UpdatedUtc));
            RefreshAxisSetPositionRecoveryUi(
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
                "SetPosition recovery resolved only after durable terminal proof "
                + "and successful exact-generation retirement. "
                + FormatAxisSetPositionRecoveryRecord(resolved));
            UpdateUiState();
        }

        private AxisSetPositionRecoveryRecord PromoteAxisSetPositionRecoveryIfArmed(
            AxisSetPositionRecoveryRecord captured,
            string reason)
        {
            if (axisSetPositionRecoveryJournal == null)
            {
                return captured;
            }

            var current = axisSetPositionRecoveryJournal.CurrentRecord;
            if (current == null
                || !current.IsActive
                || current.State != AxisSetPositionRecoveryState.ArmedBeforeDispatch)
            {
                return current ?? captured;
            }
            if (captured != null && current.Identity != captured.Identity)
            {
                throw new InvalidOperationException(
                    "SetPosition recovery identity changed before promotion.");
            }

            var promoted = axisSetPositionRecoveryJournal.PromoteToRecoveryRequired(
                current,
                MonotonicAxisSetPositionUtc(current.UpdatedUtc));
            WriteLog(
                "SetPosition durable journal promoted to RecoveryRequired after "
                + reason + ". No automatic Start replay is permitted.");
            return promoted;
        }

        private AxisSetPositionRecoveryRecord RequireActiveAxisSetPositionRecoveryRecord(
            string operation)
        {
            if (AxisSetPositionRecoveryJournalUnavailable)
            {
                throw new InvalidOperationException(
                    operation
                    + " is blocked because the SetPosition recovery journal is "
                    + "unavailable. "
                    + (axisSetPositionRecoveryJournalError
                        ?? "No journal was opened."));
            }

            var record = axisSetPositionRecoveryJournal.CurrentRecord;
            if (record == null || !record.IsActive)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires an unresolved durable SetPosition record.");
            }
            return record;
        }

        private void EnsureAxisSetPositionRecoveryEndpointMatchesCurrent(
            AxisSetPositionRecoveryRecord record,
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
                    + " is blocked because the connected endpoint does not "
                    + "match the durable SetPosition record. Stored="
                    + record.EndpointIp + ":"
                    + record.EndpointPort.ToString(CultureInfo.InvariantCulture)
                    + ", current=" + endpointIp + ":"
                    + endpointPort.ToString(CultureInfo.InvariantCulture) + ".");
            }
        }

        private void EnsureAxisSetPositionRecoveryIdentity(
            AxisSetPositionRecoveryRecord record,
            string operation)
        {
            if (diagnosticCapabilities == null
                || record.DiagnosticsBuild != diagnosticCapabilities.DiagnosticsBuild
                || record.DiagnosticsBootId != diagnosticCapabilities.DiagnosticsBootId
                || record.MapRevision != diagnosticCapabilities.MapRevision)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    operation
                    + " is blocked because DiagnosticsBuild/BootId/MapRevision "
                    + "does not match the durable SetPosition record. No "
                    + "0x7D14/0x7D1A request was sent.");
            }
        }

        private void EnsureAxisSetPositionCapabilitiesReady(string operation)
        {
            if (!HasAxisSetPositionCapabilityTriad())
            {
                throw new NotSupportedException(
                    operation
                    + " requires Admin SetPosition/Outcome/Retire capability "
                    + "triad. Current production activation is expected to keep "
                    + "bits 3/5/7 OFF until SP-01..03/05/06 pass.");
            }
            if (!HasStableAxisSetPositionDiagnosticsIdentity())
            {
                throw new NotSupportedException(
                    operation
                    + " requires current-session nonzero DiagnosticsBuild, "
                    + "BootId and MapRevision.");
            }
        }

        private bool HasAxisSetPositionCapabilityTriad()
        {
            return adminCapabilities != null
                && adminCapabilities.Response != null
                && adminCapabilities.Response.IsSuccess
                && adminCapabilities.Supports(LMCAdminFeature.AxisSetPosition)
                && adminCapabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRead)
                && adminCapabilities.Supports(
                    LMCAdminFeature.AxisSetPositionOutcomeRetirement);
        }

        private bool HasStableAxisSetPositionDiagnosticsIdentity()
        {
            return diagnosticCapabilities != null
                && diagnosticCapabilities.Response != null
                && diagnosticCapabilities.Response.IsSuccess
                && diagnosticCapabilities.DiagnosticsBuild != 0
                && diagnosticCapabilities.DiagnosticsBootId != 0
                && diagnosticCapabilities.MapRevision != 0;
        }

        private ushort RequireAxisSetPositionAxisReference()
        {
            if (comboAxisSetPositionReference == null
                || !(comboAxisSetPositionReference.SelectedItem is ushort))
            {
                throw new InvalidOperationException(
                    "SetPosition physical axis reference is required.");
            }
            var value = (ushort)comboAxisSetPositionReference.SelectedItem;
            if (value < 1 || value > 4)
            {
                throw new InvalidOperationException(
                    "SetPosition physical axis reference must be between 1 and 4.");
            }
            return value;
        }

        private bool TryGetAxisSetPositionValues(
            out int targetPosition,
            out int expectedActualPosition)
        {
            return int.TryParse(
                    textAxisSetPositionTarget == null
                        ? string.Empty
                        : textAxisSetPositionTarget.Text.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out targetPosition)
                && int.TryParse(
                    textAxisSetPositionExpectedActual == null
                        ? string.Empty
                        : textAxisSetPositionExpectedActual.Text.Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out expectedActualPosition);
        }

        private void AxisSetPositionInputChanged(
            object sender,
            RoutedEventArgs e)
        {
            if (uiInitializationComplete)
            {
                UpdateUiState();
            }
        }

        private void AxisSetPositionInputChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (uiInitializationComplete)
            {
                UpdateUiState();
            }
        }

        private void AxisSetPositionInputChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (uiInitializationComplete)
            {
                UpdateUiState();
            }
        }

        private void RefreshAxisSetPositionRecoveryUi(string message = null)
        {
            if (textAxisSetPositionRecoveryStatus == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                textAxisSetPositionRecoveryStatus.Text = message;
                return;
            }
            if (AxisSetPositionRecoveryJournalUnavailable)
            {
                textAxisSetPositionRecoveryStatus.Text =
                    "JOURNAL UNAVAILABLE - START FAIL-CLOSED. "
                    + (axisSetPositionRecoveryJournalError
                        ?? "No journal was opened.");
                return;
            }

            var record = axisSetPositionRecoveryJournal.CurrentRecord;
            if (record != null && record.IsActive)
            {
                textAxisSetPositionRecoveryStatus.Text =
                    "UNRESOLVED / 0x7D12 REPLAY BLOCKED | "
                    + FormatAxisSetPositionRecoveryRecord(record);
                return;
            }

            textAxisSetPositionRecoveryStatus.Text =
                "Journal ready; no unresolved SetPosition record. AdminTriad="
                + HasAxisSetPositionCapabilityTriad()
                + ", DiagnosticsIdentity="
                + HasStableAxisSetPositionDiagnosticsIdentity()
                + ". Start remains disabled until current PLC advertises the "
                + "paired SetPosition capability triad after SP qualification.";
        }

        private static LMCAxisSetPositionRecoveryKey ToAxisSetPositionRecoveryKey(
            AxisSetPositionRecoveryRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            return new LMCAxisSetPositionRecoveryKey(
                record.SchemaVersion,
                record.RequestId,
                record.DiagnosticsBuild,
                record.DiagnosticsBootId,
                record.MapRevision,
                record.ClientIntentId0,
                record.ClientIntentId1,
                record.ClientIntentId2,
                record.ClientIntentId3,
                record.AxisReference,
                record.TargetPosition,
                record.ExpectedActualPosition,
                (LMCAxisSetPositionSemanticMode)record.SemanticMode);
        }

        private static string FormatAxisSetPositionRecoveryRecord(
            AxisSetPositionRecoveryRecord record)
        {
            if (record == null)
            {
                return "SetPosition recovery record: none";
            }
            return "State=" + record.State
                + ", Endpoint=" + record.EndpointIp + ":"
                + record.EndpointPort.ToString(CultureInfo.InvariantCulture)
                + ", Axis=" + record.AxisName + "/"
                + record.AxisReference.ToString(CultureInfo.InvariantCulture)
                + ", Target="
                + record.TargetPosition.ToString(CultureInfo.InvariantCulture)
                + ", ExpectedActual="
                + record.ExpectedActualPosition.ToString(
                    CultureInfo.InvariantCulture)
                + ", RequestId="
                + record.RequestId.ToString(CultureInfo.InvariantCulture)
                + ", Build/Boot/Map=0x"
                + record.DiagnosticsBuild.ToString("X8") + "/0x"
                + record.DiagnosticsBootId.ToString("X8") + "/0x"
                + record.MapRevision.ToString("X8")
                + (record.TerminalOutcomeProof == null
                    ? string.Empty
                    : ", Terminal=" + record.TerminalOutcomeProof.RecordState
                        + ", Generation="
                        + record.TerminalOutcomeProof.RecordGeneration.ToString(
                            CultureInfo.InvariantCulture));
        }

        private static DateTime MonotonicAxisSetPositionUtc(DateTime previousUtc)
        {
            var now = DateTime.UtcNow;
            return now > previousUtc ? now : previousUtc.AddTicks(1);
        }
    }
}

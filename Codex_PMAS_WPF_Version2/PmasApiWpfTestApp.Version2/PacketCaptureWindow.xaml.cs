using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using PmasApiWpfTestApp.Version2.Services;

namespace PmasApiWpfTestApp.Version2
{
    public partial class PacketCaptureWindow : Window
    {
        private int _captureSequence;
        private bool _uiInitialized;

        public PacketCaptureWindow()
        {
            InitializeComponent();
            Context = new PmasControllerContext();
            Context.Logs.CollectionChanged += OnLogsCollectionChanged;
            Closing += PacketCaptureWindow_Closing;
            DataContext = this;
        }

        internal PmasControllerContext Context { get; private set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_uiInitialized)
            {
                return;
            }

            _uiInitialized = true;
            InitializeMotionCombos();
            InitializeDiagnosticsCombos();
            UpdateConnectionUi();
            LogLocal("Version2 ready. Start Wireshark separately, then use the CAPTURE markers to correlate each MMCLib call with the wire traffic.");
        }

        private void InitializeMotionCombos()
        {
            ComboAxisUnit.Items.Add("Elmo controller engineering unit (direct double)");
            ComboAxisUnit.SelectedIndex = 0;
            ComboGroupUnit.Items.Add("Elmo controller engineering unit (direct double)");
            ComboGroupUnit.SelectedIndex = 0;

            FillEnumCombo(ComboDirection, typeof(MC_DIRECTION_ENUM));
            SelectComboItemByName(ComboDirection, "MC_POSITIVE_DIRECTION");

            FillEnumCombo(ComboGroupCoordinate, typeof(MC_COORD_SYSTEM_ENUM));
            SelectComboItemByName(ComboGroupCoordinate, "MC_MCS_COORD");

            FillEnumCombo(ComboGroupTransition, typeof(NC_TRANSITION_MODE_ENUM));
            SelectComboItemByName(ComboGroupTransition, "MC_TM_NONE_MODE");

            FillEnumCombo(ComboGroupBuffer, typeof(MC_BUFFERED_MODE_ENUM));
            SelectComboItemByName(ComboGroupBuffer, "MC_ABORTING_MODE", "MC_ABORTING");
        }

        private void InitializeDiagnosticsCombos()
        {
            FillEnumCombo(ComboPiDirection, typeof(PIVarDirection));
            SelectComboItemByName(ComboPiDirection, "ePI_INPUT");

            ComboPiVarType.Items.Add(VAR_TYPE.S_BYTE);
            ComboPiVarType.Items.Add(VAR_TYPE.BYTE);
            ComboPiVarType.Items.Add(VAR_TYPE.SHORT);
            ComboPiVarType.Items.Add(VAR_TYPE.USHORT);
            ComboPiVarType.Items.Add(VAR_TYPE.INT);
            ComboPiVarType.Items.Add(VAR_TYPE.UINT);
            ComboPiVarType.Items.Add(VAR_TYPE.FLOAT);
            SelectComboItemByName(ComboPiVarType, "USHORT");

            FillFilteredEnumCombo(ComboPiBulkConfig, typeof(NC_BULKREAD_CONFIG_PI_ENUM));
            SelectComboItemByName(ComboPiBulkConfig, "eBULKREAD_CONFIG_PI_1");

            ComboRecorderBufferMode.Items.Add("PMAS native recorder");
            ComboRecorderBufferMode.SelectedIndex = 0;
            ComboRecorderTriggerType.Items.Add("Configured by recorder params");
            ComboRecorderTriggerType.SelectedIndex = 0;
            ComboRecorderTriggerOperator.Items.Add("Configured by recorder params");
            ComboRecorderTriggerOperator.SelectedIndex = 0;

            ComboSdoOperation.Items.Add("Read");
            ComboSdoOperation.Items.Add("Write");
            ComboSdoOperation.SelectedIndex = 0;
            ComboSdoValueType.Items.Add("Byte");
            ComboSdoValueType.Items.Add("Int16");
            ComboSdoValueType.Items.Add("UInt16");
            ComboSdoValueType.Items.Add("Int32");
            ComboSdoValueType.Items.Add("UInt32");
            ComboSdoValueType.Items.Add("Float");
            ComboSdoValueType.SelectedIndex = 4;
            ComboSdoDataLength.Items.Add("1");
            ComboSdoDataLength.Items.Add("2");
            ComboSdoDataLength.Items.Add("4");
            ComboSdoDataLength.SelectedIndex = 1;
            UpdateSdoDerivedDataLength();

            InitializeDiagnosticsModels();
        }

        private static void FillEnumCombo(ComboBox combo, Type enumType)
        {
            combo.Items.Clear();
            foreach (var value in Enum.GetValues(enumType))
            {
                combo.Items.Add(value);
            }

            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private static void FillFilteredEnumCombo(ComboBox combo, Type enumType)
        {
            combo.Items.Clear();
            foreach (var value in Enum.GetValues(enumType))
            {
                var name = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (name.IndexOf("NONE", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("MAX", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    combo.Items.Add(value);
                }
            }

            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private static void SelectComboItemByName(ComboBox combo, params string[] names)
        {
            foreach (var item in combo.Items)
            {
                var itemName = Convert.ToString(item, CultureInfo.InvariantCulture);
                if (names.Any(name => string.Equals(name, itemName, StringComparison.OrdinalIgnoreCase)))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        internal void ExecuteCaptureAction(string apiName, Action action)
        {
            var captureId = ++_captureSequence;
            var stopwatch = Stopwatch.StartNew();
            SetOperationState(string.Format(CultureInfo.InvariantCulture, "CAPTURE #{0:0000} {1} running", captureId, apiName));
            Context.Log(string.Format(CultureInfo.InvariantCulture, "CAPTURE #{0:0000} START API={1}", captureId, apiName));

            try
            {
                action();
                stopwatch.Stop();
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CAPTURE #{0:0000} PASS API={1} ElapsedMs={2:F3}",
                    captureId,
                    apiName,
                    stopwatch.Elapsed.TotalMilliseconds));
                SetOperationState(string.Format(CultureInfo.InvariantCulture, "CAPTURE #{0:0000} PASS", captureId));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogException(captureId, apiName, stopwatch.Elapsed.TotalMilliseconds, ex);
                SetOperationState(string.Format(CultureInfo.InvariantCulture, "CAPTURE #{0:0000} FAILED", captureId));
            }
            finally
            {
                UpdateConnectionUi();
            }
        }

        internal void ExecuteLocalAction(string name, Action action)
        {
            SetOperationState(name + " running");
            try
            {
                action();
                Context.Log("LOCAL (no controller packet): " + name + " completed.");
                SetOperationState(name + " completed");
            }
            catch (Exception ex)
            {
                Context.Log("LOCAL (no controller packet): " + name + " failed: " + ex.Message);
                SetOperationState(name + " failed");
            }
        }

        private void LogException(int captureId, string apiName, double elapsedMs, Exception ex)
        {
            var mmcException = ex as MMCException;
            if (mmcException != null)
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "CAPTURE #{0:0000} FAILED API={1} ElapsedMs={2:F3} Command={3} LibraryError={4} MMCError={5} Status={6} AxisRef={7} AxisName={8}",
                    captureId,
                    apiName,
                    elapsedMs,
                    mmcException.CommandID,
                    mmcException.LibraryError,
                    mmcException.MMCError,
                    mmcException.Status,
                    mmcException.AxisRef,
                    string.IsNullOrWhiteSpace(mmcException.AxisName) ? "-" : mmcException.AxisName));
                MessageBox.Show(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} failed.\nCommand={1}\nLibraryError={2}\nMMCError={3}\nStatus={4}\nAxisRef={5}",
                        apiName,
                        mmcException.CommandID,
                        mmcException.LibraryError,
                        mmcException.MMCError,
                        mmcException.Status,
                        mmcException.AxisRef),
                    apiName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "CAPTURE #{0:0000} FAILED API={1} ElapsedMs={2:F3} Error={3}",
                captureId,
                apiName,
                elapsedMs,
                ex.Message));
            MessageBox.Show(ex.Message, apiName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCaptureAction("MMC_RpcInitConnection / ConnectRPC", delegate
            {
                ResetControllerBoundState("Connection parameters changed. Reload PMAS objects after Connect.");
                Context.Connect(
                    Normalize(TextRemoteIp.Text),
                    ParseInt32(TextRemotePort.Text),
                    Normalize(TextLocalIp.Text),
                    ParseInt32(TextCallbackPort.Text),
                    0xFFFFFFFFu);
            });
        }

        private void ButtonCloseConnection_Click(object sender, RoutedEventArgs e)
        {
            if (!Context.IsConnected)
            {
                ExecuteLocalAction("Close while already disconnected", () => ResetControllerBoundState("Already disconnected. Reload PMAS objects after the next Connect."));
                return;
            }

            ExecuteCaptureAction("MMC_CloseConnection", delegate
            {
                try
                {
                    Context.Disconnect();
                }
                finally
                {
                    ResetControllerBoundState("Connection closed. Reload PMAS objects after the next Connect.");
                }
            });
        }

        private void ResetControllerBoundState(string reason)
        {
            captureAxis = null;
            captureGroup = null;
            captureGroupMemberNames = null;
            captureGroupAxes = null;
            _piBulkRead = null;
            _recorderConfiguration = null;
            InvalidateRecorderControllerState();
            _lastSdoResult = null;
            _lastSdoResultText = null;
            _piReadCycle = 0;
            _signalRows.Clear();
            _bulkSnapshotRows.Clear();

            if (TextAxisReference != null)
            {
                TextAxisReference.Text = "not loaded";
            }

            if (TextGroupReference != null)
            {
                TextGroupReference.Text = "not loaded";
            }

            if (TextAxisResult != null)
            {
                TextAxisResult.Text = reason;
            }

            if (TextGroupResult != null)
            {
                TextGroupResult.Text = reason;
            }

            if (TextBulkSummary != null)
            {
                TextBulkSummary.Text = reason;
            }

            if (TextEtherCatHealthSummary != null)
            {
                TextEtherCatHealthSummary.Text = reason;
            }

            if (GridEtherCatHealth != null)
            {
                GridEtherCatHealth.ItemsSource = new ObservableCollection<PmasHealthRow>();
            }

            if (TextDiagnosticOperationSummary != null)
            {
                TextDiagnosticOperationSummary.Text = reason;
            }

            if (TextRecorderSummary != null)
            {
                TextRecorderSummary.Text = _downloadedRecorderData == null
                    ? reason
                    : reason + " Previously downloaded recorder data remains in PC memory until Release or a new Start.";
            }
        }

        private void ButtonOpenAdvanced_Click(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow();
            window.Owner = this;
            window.Show();
            LogLocal("Advanced PMAS window opened with its own connection context.");
        }

        private void ButtonCopyLog_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Context.LogText))
            {
                Clipboard.SetText(Context.LogText);
                LogLocal("Execution log copied to clipboard.");
            }
        }

        private void ButtonClearLog_Click(object sender, RoutedEventArgs e)
        {
            Context.Logs.Clear();
            LogLocal("Execution log cleared.");
        }

        private void MotionTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != TabsMotion)
            {
                return;
            }

            var selectedTab = TabsMotion.SelectedItem as TabItem;
            var scrollViewer = selectedTab == null ? null : FindFirstVisualChild<ScrollViewer>(selectedTab);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToTop();
            }
        }

        private static T FindFirstVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (var index = 0; index < count; index++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
                var typed = child as T;
                if (typed != null)
                {
                    return typed;
                }

                var nested = FindFirstVisualChild<T>(child);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private void PacketCaptureWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Context != null && Context.IsConnected)
            {
                try
                {
                    Context.Disconnect();
                }
                catch (Exception ex)
                {
                    Context.Log("Window close connection cleanup failed: " + ex.Message);
                }
                finally
                {
                    ResetControllerBoundState("Window is closing.");
                }
            }
        }

        private void OnLogsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (TextExecutionLog == null)
            {
                return;
            }

            TextExecutionLog.Text = Context.LogText;
            TextExecutionLog.ScrollToEnd();
        }

        private void UpdateConnectionUi()
        {
            if (TextConnectionState == null)
            {
                return;
            }

            TextConnectionState.Text = Context.IsConnected
                ? string.Format(CultureInfo.InvariantCulture, "Connected / Handle={0}", Context.Handle)
                : "Disconnected";

            if (!Context.IsConnected)
            {
                TextCallbackState.Text = "Stopped";
                UpdateRecorderActionAvailability();
                return;
            }

            try
            {
                var connection = Context.GetConnectionObject();
                var listenerPort = MMCConnection.GetUDPListenerPortNumber(Context.Handle);
                TextCallbackState.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Opened={0}, Callback={1}, Listener={2}",
                    connection.IsUDPChannelOpened,
                    connection.CbUdpPort,
                    listenerPort);
            }
            catch (Exception ex)
            {
                TextCallbackState.Text = "Unavailable: " + ex.Message;
            }

            UpdateRecorderActionAvailability();
        }

        internal void LogLocal(string message)
        {
            Context.Log(message);
        }

        internal void SetOperationState(string value)
        {
            if (TextOperationState != null)
            {
                TextOperationState.Text = value;
            }
        }

        internal static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        internal static string[] SplitValues(string value)
        {
            return (value ?? string.Empty)
                .Split(new[] { ',', ';', '\r', '\n', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static int ParseInt32(string value)
        {
            var normalized = Normalize(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? unchecked((int)uint.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture))
                : int.Parse(normalized, CultureInfo.InvariantCulture);
        }

        internal static uint ParseUInt32(string value)
        {
            var normalized = Normalize(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? uint.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : uint.Parse(normalized, CultureInfo.InvariantCulture);
        }

        internal static ushort ParseUInt16(string value)
        {
            var normalized = Normalize(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? ushort.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : ushort.Parse(normalized, CultureInfo.InvariantCulture);
        }

        internal static byte ParseByte(string value)
        {
            var normalized = Normalize(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? byte.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : byte.Parse(normalized, CultureInfo.InvariantCulture);
        }

        internal static double ParseDouble(string value)
        {
            return double.Parse(Normalize(value), CultureInfo.InvariantCulture);
        }

        internal static float ParseFloat(string value)
        {
            return float.Parse(Normalize(value), CultureInfo.InvariantCulture);
        }

        internal static ushort[] ParseUInt16Array(string value)
        {
            return SplitValues(value).Select(ParseUInt16).ToArray();
        }

        internal static uint[] ParseUInt32Array(string value)
        {
            return SplitValues(value).Select(ParseUInt32).ToArray();
        }

        internal MC_BUFFERED_MODE_ENUM GetAbortingMode()
        {
            foreach (var value in Enum.GetValues(typeof(MC_BUFFERED_MODE_ENUM)))
            {
                var name = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (name.IndexOf("ABORTING", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return (MC_BUFFERED_MODE_ENUM)value;
                }
            }

            return (MC_BUFFERED_MODE_ENUM)Enum.GetValues(typeof(MC_BUFFERED_MODE_ENUM)).GetValue(0);
        }

        internal MC_BUFFERED_MODE_ENUM GetSelectedGroupBufferMode()
        {
            return ComboGroupBuffer.SelectedItem is MC_BUFFERED_MODE_ENUM
                ? (MC_BUFFERED_MODE_ENUM)ComboGroupBuffer.SelectedItem
                : GetAbortingMode();
        }

        internal MC_COORD_SYSTEM_ENUM GetSelectedGroupCoordinate()
        {
            if (!(ComboGroupCoordinate.SelectedItem is MC_COORD_SYSTEM_ENUM))
            {
                throw new InvalidOperationException("Group coordinate system is not selected.");
            }

            return (MC_COORD_SYSTEM_ENUM)ComboGroupCoordinate.SelectedItem;
        }

        internal NC_TRANSITION_MODE_ENUM GetSelectedGroupTransition()
        {
            if (!(ComboGroupTransition.SelectedItem is NC_TRANSITION_MODE_ENUM))
            {
                throw new InvalidOperationException("Group transition mode is not selected.");
            }

            return (NC_TRANSITION_MODE_ENUM)ComboGroupTransition.SelectedItem;
        }

        internal string FormatObject(object value)
        {
            var builder = new StringBuilder();
            AppendObject(builder, value, "value", 0);
            return builder.ToString().Trim();
        }

        private static void AppendObject(StringBuilder builder, object value, string name, int depth)
        {
            var indent = new string(' ', depth * 2);
            if (value == null)
            {
                builder.AppendLine(indent + name + " = <null>");
                return;
            }

            var type = value.GetType();
            if (depth >= 3 || type.IsPrimitive || value is decimal || value is string || value is Enum)
            {
                builder.AppendLine(indent + name + " = " + Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string))
            {
                var index = 0;
                foreach (var item in enumerable)
                {
                    AppendObject(builder, item, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", depth + 1);
                    index++;
                }

                if (index == 0)
                {
                    builder.AppendLine(indent + name + " = []");
                }
                return;
            }

            builder.AppendLine(indent + name + " : " + type.Name);
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                AppendObject(builder, field.GetValue(value), field.Name, depth + 1);
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(item => item.CanRead && item.GetIndexParameters().Length == 0))
            {
                try
                {
                    AppendObject(builder, property.GetValue(value, null), property.Name, depth + 1);
                }
                catch
                {
                }
            }
        }

        internal static string FormatVector(double[] values)
        {
            if (values == null)
            {
                return "<null>";
            }

            return string.Join(",", values.Select(value => value.ToString("0.###", CultureInfo.InvariantCulture)));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using PmasApiWpfTestApp.Models;
using PmasApiWpfTestApp.Services;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow : Window
    {
        private MMCBulkRead _bulkRead;

        public MainWindow()
        {
            InitializeComponent();
            Context = new PmasControllerContext();
            CoverageItems = new ObservableCollection<ApiCoverageItem>(CreateCoverageItems());
            DataContext = this;
            InitializeComboBoxes();
            Context.Logs.CollectionChanged += OnLogsCollectionChanged;
        }

        public PmasControllerContext Context { get; private set; }

        public ObservableCollection<ApiCoverageItem> CoverageItems { get; private set; }

        private void InitializeComboBoxes()
        {
            FillCombo(ComboBufferedMode, typeof(MC_BUFFERED_MODE_ENUM));
            FillCombo(ComboGroupBufferedMode, typeof(MC_BUFFERED_MODE_ENUM));
            SelectDefaultAbortingMode(ComboBufferedMode);
            SelectDefaultAbortingMode(ComboGroupBufferedMode);

            FillCombo(ComboDirection, typeof(MC_DIRECTION_ENUM));
            ComboDirection.SelectedItem = MC_DIRECTION_ENUM.MC_SHORTEST_WAY;
            FillCombo(ComboOpMode, typeof(OPM402));
            FillCombo(ComboExecutionMode, typeof(MC_EXECUTION_MODE));
            FillCombo(ComboParameter, typeof(MMC_PARAMETER_LIST_ENUM));
            FillCombo(ComboBoolParameter, typeof(MMC_BOOLEAN_PARAMETER_LIST_ENUM));
            FillCombo(ComboPiDirection, typeof(PIVarDirection));
            FillCombo(ComboBulkConfig, typeof(NC_BULKREAD_CONFIG_ENUM));
            FillCombo(ComboBulkPreset, typeof(NC_BULKREAD_PRESET_ENUM));
            FillCombo(ComboGroupTransitionMode, typeof(NC_TRANSITION_MODE_ENUM));
            FillCombo(ComboGroupCoordSystem, typeof(MC_COORD_SYSTEM_ENUM));
            FillCombo(ComboConditionOperation, typeof(MC_CONDITIONFB_OPERATION_TYPE));

            ComboSetPositionMode.Items.Add("Absolute");
            ComboSetPositionMode.Items.Add("Relative");
            ComboSetPositionMode.Items.Add("Modulo");
            ComboSetPositionMode.SelectedIndex = 0;

            ComboSdoType.Items.Add("Byte");
            ComboSdoType.Items.Add("Int16");
            ComboSdoType.Items.Add("UInt16");
            ComboSdoType.Items.Add("Int32");
            ComboSdoType.Items.Add("UInt32");
            ComboSdoType.Items.Add("Float");
            ComboSdoType.SelectedIndex = 0;
        }

        private static void FillCombo(ComboBox combo, Type enumType)
        {
            foreach (var value in Enum.GetValues(enumType))
            {
                combo.Items.Add(value);
            }

            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private static void SelectDefaultAbortingMode(ComboBox combo)
        {
            var abortingMode = combo.Items
                .Cast<object>()
                .FirstOrDefault(item =>
                {
                    var name = Convert.ToString(item, CultureInfo.InvariantCulture);
                    return string.Equals(name, "MC_ABORTING", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "MC_ABORTING_MODE", StringComparison.OrdinalIgnoreCase);
                });

            if (abortingMode != null)
            {
                combo.SelectedItem = abortingMode;
            }
        }

        private void ExecuteAction(string functionName, Action action)
        {
            try
            {
                action();
                Context.Log(functionName + " completed.");
            }
            catch (Exception ex)
            {
                HandleException(functionName, ex);
            }
        }

        private void HandleException(string functionName, Exception ex)
        {
            var mmcException = ex as MMCException;
            if (mmcException != null)
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} failed: Command={1}, LibraryError={2}, MMCError={3}, Status={4}, AxisRef={5}, AxisName={6}",
                    functionName,
                    mmcException.CommandID,
                    mmcException.LibraryError,
                    mmcException.MMCError,
                    mmcException.Status,
                    mmcException.AxisRef,
                    string.IsNullOrWhiteSpace(mmcException.AxisName) ? "-" : mmcException.AxisName));

                if (mmcException.MMCError == MMCErrors.NC_NODE_NOT_FOUND)
                {
                    MessageBox.Show(
                        BuildNodeNotFoundMessage(functionName),
                        functionName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}\nCommand={1}\nLibraryError={2}\nMMCError={3}\nStatus={4}\nAxisRef={5}",
                        functionName + " failed.",
                        mmcException.CommandID,
                        mmcException.LibraryError,
                        mmcException.MMCError,
                        mmcException.Status,
                        mmcException.AxisRef),
                    functionName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Context.Log(functionName + " failed: " + ex.Message);
            MessageBox.Show(ex.Message, functionName, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string BuildNodeNotFoundMessage(string functionName)
        {
            if (string.Equals(functionName, "MMC_GetAxisByNameCmd", StringComparison.Ordinal))
            {
                var axisName = NormalizeNumeric(TextAxisName.Text);
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Axis name not found (NC_NODE_NOT_FOUND).\n입력 Axis Name: {0}\nMDS Resource에 등록된 실제 축 이름으로 입력하세요. (예: X, Y, Aux)",
                    string.IsNullOrWhiteSpace(axisName) ? "<empty>" : axisName);
            }

            if (string.Equals(functionName, "MMC_GetGroupByNameCmd", StringComparison.Ordinal))
            {
                var groupName = NormalizeNumeric(TextGroupName.Text);
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Group name not found (NC_NODE_NOT_FOUND).\n입력 Group Name: {0}\nMDS Resource에 등록된 실제 그룹 이름으로 입력하세요.",
                    string.IsNullOrWhiteSpace(groupName) ? "<empty>" : groupName);
            }

            return "NC_NODE_NOT_FOUND: 축/그룹 이름이 컨트롤러 Resource와 일치하지 않습니다.";
        }

        private static string NormalizeNumeric(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private int ParseInt32(string value)
        {
            var normalized = NormalizeNumeric(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : int.Parse(normalized, CultureInfo.InvariantCulture);
        }

        private uint ParseUInt32(string value)
        {
            var normalized = NormalizeNumeric(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? uint.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : uint.Parse(normalized, CultureInfo.InvariantCulture);
        }

        private ushort ParseUInt16(string value)
        {
            var normalized = NormalizeNumeric(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? ushort.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : ushort.Parse(normalized, CultureInfo.InvariantCulture);
        }

        private byte ParseByte(string value)
        {
            var normalized = NormalizeNumeric(value);
            return normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? byte.Parse(normalized.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : byte.Parse(normalized, CultureInfo.InvariantCulture);
        }

        private double ParseDouble(string value)
        {
            return double.Parse(NormalizeNumeric(value), CultureInfo.InvariantCulture);
        }

        private float ParseSingle(string value)
        {
            return float.Parse(NormalizeNumeric(value), CultureInfo.InvariantCulture);
        }

        private static string[] SplitValues(string value)
        {
            return (value ?? string.Empty)
                .Split(new[] { ',', ';', '\r', '\n', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private ushort[] ParseUInt16Array(string value)
        {
            return SplitValues(value).Select(ParseUInt16).ToArray();
        }

        private uint[] ParseUInt32Array(string value)
        {
            return SplitValues(value).Select(ParseUInt32).ToArray();
        }

        private double[] ParseDoubleArray(string value)
        {
            return SplitValues(value).Select(ParseDouble).ToArray();
        }

        private double[] ParseDoubleArray(string value, int minLength)
        {
            var values = ParseDoubleArray(value);
            if (values.Length >= minLength)
            {
                return values;
            }

            var padded = new double[minLength];
            Array.Copy(values, padded, values.Length);
            return padded;
        }

        private string DumpObject(object value)
        {
            var builder = new StringBuilder();
            DumpObject(builder, value, 0, "value");
            return builder.ToString().Trim();
        }

        private void DumpObject(StringBuilder builder, object value, int depth, string name)
        {
            var indent = new string(' ', depth * 2);
            if (value == null)
            {
                builder.AppendLine(indent + name + " = <null>");
                return;
            }

            var type = value.GetType();
            if (depth > 3)
            {
                builder.AppendLine(indent + name + " = <max-depth>");
                return;
            }

            if (type.IsPrimitive || value is decimal || value is string || value is Enum)
            {
                builder.AppendLine(indent + name + " = " + Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string))
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    DumpObject(builder, item, depth + 1, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]");
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
                DumpObject(builder, field.GetValue(value), depth + 1, field.Name);
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
            {
                object propertyValue;
                try
                {
                    propertyValue = property.GetValue(value, null);
                }
                catch
                {
                    continue;
                }

                DumpObject(builder, propertyValue, depth + 1, property.Name);
            }
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_RpcInitConnection", delegate
            {
                Context.Connect(
                    NormalizeNumeric(TextRemoteIp.Text),
                    ParseInt32(TextRemotePort.Text),
                    NormalizeNumeric(TextLocalIp.Text),
                    ParseInt32(TextLocalPort.Text),
                    ParseUInt32(TextEventMask.Text));
            });
        }

        private void ButtonCloseConnection_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_CloseConnection", () => Context.Disconnect());
        }

        private void ButtonOpenUdpChannel_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_OpenUdpChannelCmdEx", delegate
            {
                Context.EnsureConnected();
                var connection = Context.GetConnectionObject();
                var listenerPort = MMCConnection.GetUDPListenerPortNumber(Context.Handle);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "UDP callback state: IsOpened={0}, CallbackPort={1}, ListenerPort={2}. ConnectRPC already assigns the UDP channel in this wrapper.",
                    connection.IsUDPChannelOpened,
                    connection.CbUdpPort,
                    listenerPort));
            });
        }

        private void ButtonGetAxisByName_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetAxisByNameCmd", () => Context.LoadAxis(NormalizeNumeric(TextAxisName.Text)));
        }

        private void ButtonGetGroupByName_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetGroupByNameCmd", () => Context.LoadGroup(NormalizeNumeric(TextGroupName.Text), NormalizeNumeric(TextGroupAxes.Text)));
        }

        private void ButtonGetErrorDescription_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetErrorCodeDescriptionByID", delegate
            {
                Context.EnsureConnected();
                string resolution;
                string description;
                MMCConnection.GetErrorCodeDescriptionByID(Context.Handle, ParseInt32(TextErrorCode.Text), ParseByte(TextErrorType.Text), out resolution, out description);
                Context.Log(string.Format(CultureInfo.InvariantCulture, "Description={0}, Resolution={1}", description ?? "-", resolution ?? "-"));
            });
        }

        private void ButtonCopyExecutionLog_Click(object sender, RoutedEventArgs e)
        {
            var logText = Context.LogText;
            if (string.IsNullOrWhiteSpace(logText))
            {
                return;
            }

            Clipboard.SetText(logText);
            Context.Log("Execution log copied to clipboard.");
        }

        private void ButtonClearExecutionLog_Click(object sender, RoutedEventArgs e)
        {
            Context.Logs.Clear();
            Context.Log("Execution log cleared.");
        }

        private void OnLogsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (TextExecutionLog == null)
            {
                return;
            }

            TextExecutionLog.ScrollToEnd();
        }
    }
}

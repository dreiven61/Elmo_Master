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
using PmasApiWpfTestApp.Version2.Models;
using PmasApiWpfTestApp.Version2.Services;

namespace PmasApiWpfTestApp.Version2
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
            ComboPiVarType.Items.Add(VAR_TYPE.S_BYTE);
            ComboPiVarType.Items.Add(VAR_TYPE.BYTE);
            ComboPiVarType.Items.Add(VAR_TYPE.SHORT);
            ComboPiVarType.Items.Add(VAR_TYPE.USHORT);
            ComboPiVarType.Items.Add(VAR_TYPE.INT);
            ComboPiVarType.Items.Add(VAR_TYPE.UINT);
            ComboPiVarType.Items.Add(VAR_TYPE.FLOAT);
            SelectComboItemByName(ComboPiVarType, "USHORT");
            FillFilteredCombo(ComboBulkConfig, typeof(NC_BULKREAD_CONFIG_ENUM), IsSelectableBulkConfig);
            SelectComboItemByName(ComboBulkConfig, "eBULKREAD_CONFIG_2", "eBULKREAD_CONFIG_1");
            FillCombo(ComboBulkPreset, typeof(NC_BULKREAD_PRESET_ENUM));
            FillCombo(ComboPiBulkDirection, typeof(PIVarDirection));
            FillFilteredCombo(ComboPiBulkConfig, typeof(NC_BULKREAD_CONFIG_PI_ENUM), IsSelectableBulkConfig);
            SelectComboItemByName(ComboPiBulkConfig, "eBULKREAD_CONFIG_PI_1");
            FillCombo(ComboGroupTransitionMode, typeof(NC_TRANSITION_MODE_ENUM));
            FillCombo(ComboGroupCoordSystem, typeof(MC_COORD_SYSTEM_ENUM));
            SelectComboItemByName(ComboGroupCoordSystem, "MC_MCS_COORD");
            SelectComboItemByName(ComboGroupTransitionMode, "MC_TM_NONE_MODE");
            FillCombo(ComboCycleGroup1BufferedMode, typeof(MC_BUFFERED_MODE_ENUM));
            FillCombo(ComboCycleGroup1TransitionMode, typeof(NC_TRANSITION_MODE_ENUM));
            FillCombo(ComboCycleGroup1CoordSystem, typeof(MC_COORD_SYSTEM_ENUM));
            SelectComboItemByName(ComboCycleGroup1BufferedMode, "MC_ABORTING_MODE", "MC_ABORTING");
            SelectComboItemByName(ComboCycleGroup1CoordSystem, "MC_MCS_COORD");
            SelectComboItemByName(ComboCycleGroup1TransitionMode, "MC_TM_NONE_MODE");
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

        private static void FillFilteredCombo(ComboBox combo, Type enumType, Func<object, bool> predicate)
        {
            foreach (var value in Enum.GetValues(enumType))
            {
                if (predicate(value))
                {
                    combo.Items.Add(value);
                }
            }

            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private static bool IsSelectableBulkConfig(object value)
        {
            var name = Convert.ToString(value, CultureInfo.InvariantCulture);
            return name.IndexOf("NONE", StringComparison.OrdinalIgnoreCase) < 0
                && name.IndexOf("MAX", StringComparison.OrdinalIgnoreCase) < 0;
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

                var mmcErrorName = Convert.ToString(mmcException.MMCError, CultureInfo.InvariantCulture);
                if (string.Equals(mmcErrorName, "NC_ONE_GRP_MEMBER_IS_DISABLED", StringComparison.Ordinal))
                {
                    MessageBox.Show(
                        "NC_ONE_GRP_MEMBER_IS_DISABLED: 그룹 멤버 중 하나 이상이 Disabled 상태입니다.\n\nGroup 탭에서 Read Member Status로 멤버 상태를 확인하고, Power On Members 또는 Prepare Group MCS를 먼저 실행하세요.",
                        functionName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (string.Equals(mmcErrorName, "NC_COORD_SYSTEM_TYPE_OUT_OF_RANGE", StringComparison.Ordinal))
                {
                    MessageBox.Show(
                        "NC_COORD_SYSTEM_TYPE_OUT_OF_RANGE: 현재 그룹에 요청한 좌표계가 유효하지 않습니다.\n\nGroup Axes CSV가 실제 멤버명과 맞는지 확인하고, MMC_GetGroupMembersInfo로 축 목록을 자동 반영한 뒤 MMC_SetKinTransform 또는 Prepare Group MCS를 실행하세요.",
                        functionName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (string.Equals(mmcErrorName, "NC_ALL_AXES_SHOULD_BE_IN_KINEMATIC", StringComparison.Ordinal))
                {
                    MessageBox.Show(
                        "NC_ALL_AXES_SHOULD_BE_IN_KINEMATIC: 그룹 멤버 전체가 kinematic 정의에 포함되어야 합니다.\n\n현재 프로그램은 a01->X, a02->Y, a03->Z, a04->U, 이후 축은 V,W,N1..N9로 포함합니다. 최신 빌드로 다시 실행하고, 4축 그룹이면 End Point를 X,Y,Z,U 순서로 입력하세요.",
                        functionName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (string.Equals(mmcErrorName, "NC_GROUP_ISNOT_ENABLED", StringComparison.Ordinal)
                    || string.Equals(mmcErrorName, "NC_ERR_KINEMATIC_WAS_NOT_DEFINED", StringComparison.Ordinal)
                    || string.Equals(mmcErrorName, "NC_MCS_COORD_SYSTEM_IS_NOT_SET", StringComparison.Ordinal)
                    || string.Equals(mmcErrorName, "NC_COORD_SYSTEM_NOT_ENABLE", StringComparison.Ordinal))
                {
                    MessageBox.Show(
                        mmcErrorName + ": 그룹 모션 준비가 완료되지 않았습니다.\n\nPrepare Group MCS를 다시 실행하세요. 순서는 PowerOn members -> SetKinTransform -> GroupEnable 입니다.",
                        functionName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (string.Equals(mmcErrorName, "NC_MOTION_FORBIDDEN_ON_MCS_SW_LIMIT", StringComparison.Ordinal)
                    || string.Equals(mmcErrorName, "NC_MOTION_FORBIDDEN_ON_ACS_SW_LIMIT", StringComparison.Ordinal)
                    || string.Equals(mmcErrorName, "NC_MA_MOTION_TOWARD_SW_LIMIT_FORBIDDEN", StringComparison.Ordinal)
                    || string.Equals(mmcErrorName, "NC_MOTION_FORBIDDEN_ON_HW_LIMIT", StringComparison.Ordinal))
                {
                    MessageBox.Show(
                        mmcErrorName + ": 요청 위치가 limit 방향이거나 soft/hard limit에 걸렸습니다.\n\nRead Group Pos로 현재 위치를 확인하고, Absolute 대신 작은 Relative 이동으로 먼저 테스트하세요.",
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

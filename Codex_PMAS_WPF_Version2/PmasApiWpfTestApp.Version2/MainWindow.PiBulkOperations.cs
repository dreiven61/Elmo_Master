using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.InternalArgs;
using ElmoMotionControlComponents.GMAS.MMCLibDotNET;

namespace PmasApiWpfTestApp.Version2
{
    public partial class MainWindow
    {
        private MMCPIBulkRead _piBulkRead;
        private readonly List<PIBulkEntryContext> _piBulkEntries = new List<PIBulkEntryContext>();

        private void ButtonGetPIVarInfoByAlias_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetPIVarInfoByAlias", delegate
            {
                Context.EnsureAxis();
                var alias = NormalizePiAlias(TextPiAlias.Text);
                if (alias.Length == 0)
                {
                    throw new InvalidOperationException("PI alias is empty.");
                }

                NC_PI_INFO_BY_ALIAS info;
                var result = Context.SingleAxis.GetPIVarInfoByAlias(alias, out info);
                LogPIVarInfoByAlias(alias, result, info);
                if (result != 0 || !IsPlausiblePIVarInfoByAlias(info))
                {
                    throw new InvalidOperationException("PI alias lookup failed or returned invalid data. Error=" + FormatMmcErrorCode(result) + ". Check the exact PI map alias, for example I0x6041.0.");
                }
            });
        }

        private void ButtonGetPIVarInfo_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetPIVarInfo", delegate
            {
                Context.EnsureAxis();
                var index = ParseUInt16(TextPiIndex.Text);
                var direction = (PIVarDirection)ComboPiDirection.SelectedItem;

                var entry = new NC_PI_ENTRY();
                Context.SingleAxis.GetPIVarInfo(index, direction, ref entry);
                LogPIVarInfo(index, direction, entry);
            });
        }

        private void ButtonReadPIVar_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ReadPIVar", delegate
            {
                Context.EnsureAxis();
                var index = ParseUInt16(TextPiIndex.Text);
                var direction = (PIVarDirection)ComboPiDirection.SelectedItem;
                var varType = GetSelectedPiVarType();
                WarnIfPIVarInfoDoesNotMatch(index, direction, varType);

                var value = new PI_VAR_UNION();
                Context.SingleAxis.ReadPIVar(
                    index,
                    direction,
                    varType,
                    ref value);

                TextPiValue.Text = FormatPiValue(value, varType);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "PI {0} read value = {1}",
                    varType,
                    TextPiValue.Text));
            });
        }

        private void ButtonWritePIVar_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_WritePIVar", delegate
            {
                Context.EnsureAxis();
                var index = ParseUInt16(TextPiIndex.Text);
                var varType = GetSelectedPiVarType();
                var value = ParsePiValue(TextPiValue.Text, varType);
                Context.SingleAxis.WritePIVar(
                    index,
                    value,
                    varType);

                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "PI {0} write issued. Index={1}, Value={2}",
                    varType,
                    index,
                    TextPiValue.Text));
            });
        }

        private void ButtonConfigBulkRead_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ConfigBulkReadCmd", delegate
            {
                Context.EnsureConnected();
                var nodeRefs = ParseUInt16Array(TextBulkNodeRefs.Text);
                if (nodeRefs.Length == 0)
                {
                    throw new InvalidOperationException("Node Refs are empty.");
                }

                _bulkRead = new MMCBulkRead(Context.Handle);
                if (CheckBulkUsePreset.IsChecked == true)
                {
                    _bulkRead.Init(
                        (NC_BULKREAD_PRESET_ENUM)ComboBulkPreset.SelectedItem,
                        (NC_BULKREAD_CONFIG_ENUM)ComboBulkConfig.SelectedItem,
                        nodeRefs,
                        (ushort)nodeRefs.Length);
                }
                else
                {
                    var customValues = ParseUInt32Array(TextBulkCustomValues.Text);
                    _bulkRead.Init(
                        customValues,
                        (NC_BULKREAD_CONFIG_ENUM)ComboBulkConfig.SelectedItem,
                        nodeRefs,
                        (ushort)nodeRefs.Length);
                }

                _bulkRead.Config();
                Context.Log("BulkRead configured. Nodes=" + string.Join(",", nodeRefs.Select(v => v.ToString(CultureInfo.InvariantCulture))));
            });
        }

        private void ButtonPerformBulkRead_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_PerformBulkReadCmd", delegate
            {
                if (_bulkRead == null)
                {
                    throw new InvalidOperationException("BulkRead is not initialized. Run MMC_ConfigBulkReadCmd first.");
                }

                if (!_bulkRead.IsConfigured)
                {
                    throw new InvalidOperationException("BulkRead is not configured.");
                }

                _bulkRead.Perform();
                LogBulkReadResult();
            });
        }

        private void ButtonConfigPiBulkRead_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ConfigureBulkReadPI", delegate
            {
                Context.EnsureConnected();
                var axisNames = SplitValues(NormalizeNumeric(TextPiBulkAxisNames.Text));
                var indexes = ParseUInt16Array(TextPiBulkIndexes.Text);
                var direction = (PIVarDirection)ComboPiBulkDirection.SelectedItem;
                var config = (NC_BULKREAD_CONFIG_PI_ENUM)ComboPiBulkConfig.SelectedItem;

                if (axisNames.Length == 0)
                {
                    throw new InvalidOperationException("PI Bulk Axis Names are empty.");
                }

                if (indexes.Length == 0)
                {
                    throw new InvalidOperationException("PI Bulk Indexes are empty.");
                }

                if (direction == PIVarDirection.ePI_NONE)
                {
                    throw new InvalidOperationException("PI Bulk Direction cannot be ePI_NONE.");
                }

                _piBulkRead = new MMCPIBulkRead(Context.Handle, config);
                _piBulkEntries.Clear();

                foreach (var axisName in axisNames)
                {
                    var axis = new MMCSingleAxis(axisName, Context.Handle);
                    foreach (var index in indexes)
                    {
                        var entry = new PI_BULKREAD_ENTRY
                        {
                            usAxisRef = axis.AxisReference,
                            usIndex = index,
                            eDirection = (byte)direction
                        };

                        _piBulkRead.AddEntry(axis, entry);
                        _piBulkEntries.Add(new PIBulkEntryContext(axis.AxisName, axis.AxisReference, index, direction, entry));
                    }
                }

                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "PIBulkRead configured. Config={0}, Entries={1}, Axes={2}, Indexes={3}, Direction={4}",
                    config,
                    _piBulkEntries.Count,
                    string.Join(",", axisNames),
                    string.Join(",", indexes.Select(v => v.ToString(CultureInfo.InvariantCulture))),
                    direction));
            });
        }

        private void ButtonPerformPiBulkRead_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_PerformBulkReadCmdPI", delegate
            {
                if (_piBulkRead == null || _piBulkEntries.Count == 0)
                {
                    throw new InvalidOperationException("PIBulkRead is not configured. Run MMC_ConfigureBulkReadPI first.");
                }

                _piBulkRead.Upload();
                foreach (var entry in _piBulkEntries)
                {
                    object value = null;
                    _piBulkRead.GetEntry(entry.Entry, out value);
                    LogPIBulkReadValue(entry, value);
                }
            });
        }

        private void LogPIVarInfo(ushort index, PIVarDirection direction, NC_PI_ENTRY entry)
        {
            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "PI Info: Index={0}, Direction={1}, BitSize={2}, BitOffset={3}, CanOpen=0x{4:X4}:{5}, VarType={6}, Alias={7}",
                index,
                direction,
                entry.uiBitSize,
                entry.uiBitOffset,
                entry.usCanOpenIndex,
                entry.ucCanOpenSubIndex,
                FormatPiVarType(entry.ucVarType),
                DecodePiAlias(entry.pAliasing)));
        }

        private void LogPIVarInfoByAlias(string alias, short result, NC_PI_INFO_BY_ALIAS info)
        {
            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "PI Alias Info: Alias={0}, Return={1}, PIVarIndex={2}, Direction={3}, BitSize={4}, BitOffset={5}, CanOpen=0x{6:X4}:{7}, VarType={8}",
                alias,
                FormatMmcErrorCode(result),
                info.usPIVarOffset,
                FormatPiDirection(info.ucDirection),
                info.uiBitSize,
                info.uiBitOffset,
                info.usCanOpenIndex,
                info.ucCanOpenSubIndex,
                FormatPiVarType(info.ucVarType)));
        }

        private static bool IsPlausiblePIVarInfoByAlias(NC_PI_INFO_BY_ALIAS info)
        {
            return info.uiBitSize > 0
                && info.uiBitSize <= 11200
                && info.usCanOpenIndex != 0
                && (info.ucDirection == (byte)PIVarDirection.ePI_INPUT || info.ucDirection == (byte)PIVarDirection.ePI_OUTPUT);
        }

        private static string NormalizePiAlias(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private void WarnIfPIVarInfoDoesNotMatch(ushort index, PIVarDirection direction, VAR_TYPE requestedType)
        {
            var entry = new NC_PI_ENTRY();
            Context.SingleAxis.GetPIVarInfo(index, direction, ref entry);

            int expectedPiVarType;
            uint expectedBitSize;
            if (!TryGetExpectedPIVarInfo(requestedType, out expectedPiVarType, out expectedBitSize))
            {
                return;
            }

            var actualPiVarType = Convert.ToInt32(entry.ucVarType, CultureInfo.InvariantCulture);
            if (actualPiVarType == expectedPiVarType && entry.uiBitSize == expectedBitSize)
            {
                return;
            }

            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "Warning: PI Index={0}, Direction={1} is VarType={2}, BitSize={3}; requested {4}. Read value may be truncated or misinterpreted.",
                index,
                direction,
                FormatPiVarType(entry.ucVarType),
                entry.uiBitSize,
                requestedType));
        }

        private static VAR_TYPE GetSelectedPiVarTypeFromItem(object item)
        {
            if (item is VAR_TYPE)
            {
                return (VAR_TYPE)item;
            }

            throw new InvalidOperationException("PI Type is not selected.");
        }

        private VAR_TYPE GetSelectedPiVarType()
        {
            return GetSelectedPiVarTypeFromItem(ComboPiVarType.SelectedItem);
        }

        private static bool TryGetExpectedPIVarInfo(VAR_TYPE varType, out int piVarType, out uint bitSize)
        {
            switch (varType)
            {
                case VAR_TYPE.S_BYTE:
                    piVarType = 1;
                    bitSize = 8;
                    return true;
                case VAR_TYPE.BYTE:
                    piVarType = 2;
                    bitSize = 8;
                    return true;
                case VAR_TYPE.SHORT:
                    piVarType = 3;
                    bitSize = 16;
                    return true;
                case VAR_TYPE.USHORT:
                    piVarType = 4;
                    bitSize = 16;
                    return true;
                case VAR_TYPE.INT:
                    piVarType = 5;
                    bitSize = 32;
                    return true;
                case VAR_TYPE.UINT:
                    piVarType = 6;
                    bitSize = 32;
                    return true;
                case VAR_TYPE.FLOAT:
                    piVarType = 9;
                    bitSize = 32;
                    return true;
                default:
                    piVarType = 0;
                    bitSize = 0;
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
                    throw new NotSupportedException("Unsupported PI Type: " + varType);
            }
        }

        private static PI_VAR_UNION ParsePiValue(string text, VAR_TYPE varType)
        {
            var normalized = NormalizeNumeric(text);
            switch (varType)
            {
                case VAR_TYPE.S_BYTE:
                    return new PI_VAR_UNION { s_byte = ParseSByteValue(normalized) };
                case VAR_TYPE.BYTE:
                    return new PI_VAR_UNION { _byte = ParseByteValue(normalized) };
                case VAR_TYPE.SHORT:
                    return new PI_VAR_UNION { _int16 = ParseInt16Value(normalized) };
                case VAR_TYPE.USHORT:
                    return new PI_VAR_UNION { _uint16 = ParseUInt16Value(normalized) };
                case VAR_TYPE.INT:
                    return new PI_VAR_UNION { _int32 = ParseInt32Value(normalized) };
                case VAR_TYPE.UINT:
                    return new PI_VAR_UNION { _uint32 = ParseUInt32Value(normalized) };
                case VAR_TYPE.FLOAT:
                    return new PI_VAR_UNION { _single = float.Parse(normalized, CultureInfo.InvariantCulture) };
                default:
                    throw new NotSupportedException("Unsupported PI Type: " + varType);
            }
        }

        private static sbyte ParseSByteValue(string value)
        {
            return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? unchecked((sbyte)byte.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture))
                : sbyte.Parse(value, CultureInfo.InvariantCulture);
        }

        private static byte ParseByteValue(string value)
        {
            return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? byte.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : byte.Parse(value, CultureInfo.InvariantCulture);
        }

        private static short ParseInt16Value(string value)
        {
            return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? unchecked((short)ushort.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture))
                : short.Parse(value, CultureInfo.InvariantCulture);
        }

        private static ushort ParseUInt16Value(string value)
        {
            return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? ushort.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : ushort.Parse(value, CultureInfo.InvariantCulture);
        }

        private static int ParseInt32Value(string value)
        {
            return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? unchecked((int)uint.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture))
                : int.Parse(value, CultureInfo.InvariantCulture);
        }

        private static uint ParseUInt32Value(string value)
        {
            return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? uint.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : uint.Parse(value, CultureInfo.InvariantCulture);
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

        private static string FormatPiVarType(PI_VAR_TYPES varType)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}({1})", varType, Convert.ToInt32(varType, CultureInfo.InvariantCulture));
        }

        private static string FormatPiVarType(byte varType)
        {
            return Enum.IsDefined(typeof(PI_VAR_TYPES), (int)varType)
                ? string.Format(CultureInfo.InvariantCulture, "{0}({1})", (PI_VAR_TYPES)varType, varType)
                : varType.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatPiDirection(byte direction)
        {
            return Enum.IsDefined(typeof(PIVarDirection), (int)direction)
                ? string.Format(CultureInfo.InvariantCulture, "{0}({1})", (PIVarDirection)direction, direction)
                : direction.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatMmcErrorCode(short code)
        {
            return Enum.IsDefined(typeof(MMCErrors), (int)code)
                ? string.Format(CultureInfo.InvariantCulture, "{0}({1})", (MMCErrors)code, code)
                : code.ToString(CultureInfo.InvariantCulture);
        }

        private void LogBulkReadResult()
        {
            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "BulkRead result. Config={0}, Preset={1}, Nodes={2}, ReadBufSize={3}",
                _bulkRead.ConfigEnum,
                _bulkRead.PresetEnum,
                _bulkRead.NodesNum,
                _bulkRead.ReadBufSize));

            if (_bulkRead.PresetEnum == NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_1)
            {
                LogObjectArray("Preset1", _bulkRead.Preset_1);
                return;
            }

            if (_bulkRead.PresetEnum == NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_2)
            {
                LogObjectArray("Preset2", _bulkRead.Preset_2);
                return;
            }

            if (_bulkRead.PresetEnum == NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_3)
            {
                LogObjectArray("Preset3", _bulkRead.Preset_3);
                return;
            }

            if (_bulkRead.PresetEnum == NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_4)
            {
                LogObjectArray("Preset4", _bulkRead.Preset_4);
                return;
            }

            if (_bulkRead.PresetEnum == NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_5)
            {
                LogObjectArray("Preset5", _bulkRead.Preset_5);
                return;
            }

            var readResult = _bulkRead.ReadResult ?? new uint[0];
            Context.Log("BulkRead raw count = " + readResult.Length.ToString(CultureInfo.InvariantCulture));
            Context.Log("BulkRead raw data = " + string.Join(",", readResult.Select(v => v.ToString(CultureInfo.InvariantCulture))));
        }

        private void LogObjectArray(string label, Array values)
        {
            if (values == null)
            {
                Context.Log(label + " data = <null>");
                return;
            }

            Context.Log(label + " count = " + values.Length.ToString(CultureInfo.InvariantCulture));
            for (var index = 0; index < values.Length; index++)
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}[{1}] = {2}",
                    label,
                    index,
                    FormatObjectFields(values.GetValue(index))));
            }
        }

        private static string FormatObjectFields(object value)
        {
            if (value == null)
            {
                return "<null>";
            }

            var type = value.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (fields.Length == 0)
            {
                return FormatObjectValue(value);
            }

            return string.Join(", ", fields.Select(field => field.Name + "=" + FormatObjectValue(field.GetValue(value))));
        }

        private void LogPIBulkReadValue(PIBulkEntryContext entry, object value)
        {
            NC_PI_ENTRY info;
            object parameterValue;
            if (TryExtractPIBulkReadParameter(value, out info, out parameterValue))
            {
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "PIBulk Axis={0}, AxisRef={1}, Index={2}, Direction={3}, Value={4}, CanOpen=0x{5:X4}:{6}, VarType={7}, Alias={8}",
                    entry.AxisName,
                    entry.AxisRef,
                    entry.Index,
                    entry.Direction,
                    FormatObjectValue(parameterValue),
                    info.usCanOpenIndex,
                    info.ucCanOpenSubIndex,
                    FormatPiVarType(info.ucVarType),
                    DecodePiAlias(info.pAliasing)));
                return;
            }

            Context.Log(string.Format(
                CultureInfo.InvariantCulture,
                "PIBulk Axis={0}, AxisRef={1}, Index={2}, Direction={3}, Value={4}",
                entry.AxisName,
                entry.AxisRef,
                entry.Index,
                entry.Direction,
                FormatObjectValue(value)));
        }

        private static bool TryExtractPIBulkReadParameter(object source, out NC_PI_ENTRY info, out object value)
        {
            info = new NC_PI_ENTRY();
            value = null;

            if (source == null)
            {
                return false;
            }

            var type = source.GetType();
            if (!string.Equals(type.FullName, "ElmoMotionControlComponents.GMAS.MMCLibDotNET.MMCPIBulkReadParameter", StringComparison.Ordinal))
            {
                return false;
            }

            var infoProperty = type.GetProperty("Info", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var valueProperty = type.GetProperty("Value", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (infoProperty == null || valueProperty == null)
            {
                return false;
            }

            var infoValue = infoProperty.GetValue(source, null);
            if (!(infoValue is NC_PI_ENTRY))
            {
                return false;
            }

            info = (NC_PI_ENTRY)infoValue;
            value = valueProperty.GetValue(source, null);
            return true;
        }

        private static string FormatObjectValue(object value)
        {
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
            return formattable != null
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private sealed class PIBulkEntryContext
        {
            public PIBulkEntryContext(string axisName, ushort axisRef, ushort index, PIVarDirection direction, PI_BULKREAD_ENTRY entry)
            {
                AxisName = axisName;
                AxisRef = axisRef;
                Index = index;
                Direction = direction;
                Entry = entry;
            }

            public string AxisName { get; private set; }

            public ushort AxisRef { get; private set; }

            public ushort Index { get; private set; }

            public PIVarDirection Direction { get; private set; }

            public PI_BULKREAD_ENTRY Entry { get; private set; }
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow
    {
        private void ButtonGetPIVarInfoByAlias_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetPIVarInfoByAlias", delegate
            {
                Context.EnsureAxis();
                var alias = NormalizeNumeric(TextPiAlias.Text);
                if (alias.Length == 0)
                {
                    throw new InvalidOperationException("PI alias is empty.");
                }

                NC_PI_INFO_BY_ALIAS info;
                Context.SingleAxis.GetPIVarInfoByAlias(alias, out info);
                Context.Log(DumpObject(info));
            });
        }

        private void ButtonReadPIVarUShort_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ReadPIVarUShort", delegate
            {
                Context.EnsureAxis();
                var value = new PI_VAR_UNION();
                Context.SingleAxis.ReadPIVar(
                    ParseUInt16(TextPiIndex.Text),
                    (PIVarDirection)ComboPiDirection.SelectedItem,
                    VAR_TYPE.USHORT,
                    ref value);

                TextPiValue.Text = value._uint16.ToString(CultureInfo.InvariantCulture);
                Context.Log("PI USHORT read value = " + TextPiValue.Text);
            });
        }

        private void ButtonWritePIVarUShort_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_WritePIVarUShort", delegate
            {
                Context.EnsureAxis();
                var value = new PI_VAR_UNION { _uint16 = ParseUInt16(TextPiValue.Text) };
                Context.SingleAxis.WritePIVar(
                    ParseUInt16(TextPiIndex.Text),
                    value,
                    VAR_TYPE.USHORT);
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
                var readResult = _bulkRead.ReadResult ?? new uint[0];
                Context.Log("BulkRead count = " + readResult.Length.ToString(CultureInfo.InvariantCulture));
                Context.Log("BulkRead data = " + string.Join(",", readResult.Select(v => v.ToString(CultureInfo.InvariantCulture))));
            });
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;

namespace PmasApiWpfTestApp
{
    public partial class MainWindow
    {
        private void ButtonPowerOn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_PowerCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.PowerOn((MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            });
        }

        private void ButtonPowerOff_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_PowerCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.PowerOff((MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            });
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_Reset", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.Reset();
            });
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_StopCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.Stop((MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            });
        }

        private void ButtonReadParameter_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ReadParameter", delegate
            {
                Context.EnsureAxis();
                var value = Context.SingleAxis.GetParameter((MMC_PARAMETER_LIST_ENUM)ComboParameter.SelectedItem, ParseInt32(TextParameterIndex.Text));
                TextParameterValue.Text = value.ToString(CultureInfo.InvariantCulture);
                Context.Log("Parameter value = " + TextParameterValue.Text);
            });
        }

        private void ButtonWriteParameter_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_WriteParameter", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.SetParameter(ParseDouble(TextParameterValue.Text), (MMC_PARAMETER_LIST_ENUM)ComboParameter.SelectedItem, ParseInt32(TextParameterIndex.Text));
            });
        }

        private void ButtonReadBoolParameter_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ReadBoolParameter", delegate
            {
                Context.EnsureAxis();
                var value = Context.SingleAxis.GetBoolParameter((MMC_BOOLEAN_PARAMETER_LIST_ENUM)ComboBoolParameter.SelectedItem, ParseInt32(TextBoolParameterIndex.Text));
                TextBoolParameterResult.Text = value.ToString();
                Context.Log("Bool parameter value = " + value);
            });
        }

        private void ButtonChangeOpMode_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ChngOpMode", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.SetOpMode(
                    (OPM402)ComboOpMode.SelectedItem,
                    (MC_EXECUTION_MODE)ComboExecutionMode.SelectedItem,
                    ParseDouble(TextInitialOpModeValue.Text));
            });
        }

        private void ButtonSetPosition_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_SetPositionCmd", delegate
            {
                Context.EnsureAxis();
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "Requested SetPosition value={0}, mode={1}. MMCLibDotNET v3.0.0.7 does not expose a public single-axis SetPosition wrapper, so this API cannot be invoked from the provided .NET assembly as-is.",
                    NormalizeNumeric(TextSetPosition.Text),
                    ComboSetPositionMode.SelectedItem));
            });
        }

        private void ButtonReadActualPosition_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ReadActualPositionCmd", delegate
            {
                Context.EnsureAxis();
                var position = Context.SingleAxis.GetActualPosition();
                Context.Log("Actual position = " + position.ToString(CultureInfo.InvariantCulture));
            });
        }

        private void ButtonReadStatus_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_ReadStatusCmd", delegate
            {
                Context.EnsureAxis();
                var state = Context.SingleAxis.ReadStatus();
                Context.Log("ReadStatus = 0x" + state.ToString("X", CultureInfo.InvariantCulture));
            });
        }

        private void ButtonGetStatusRegister_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_GetStatusRegisterCmd", delegate
            {
                Context.EnsureAxis();
                uint statusRegister = 0;
                uint mcsLimitRegister = 0;
                byte endMotionReason = 0;
                Context.SingleAxis.GetStatusRegister(ref statusRegister, ref mcsLimitRegister, ref endMotionReason);
                Context.Log(string.Format(
                    CultureInfo.InvariantCulture,
                    "Axis status register=0x{0:X}, mcsLimit=0x{1:X}, endMotionReason={2}",
                    statusRegister,
                    mcsLimitRegister,
                    endMotionReason));
            });
        }

        private void ButtonMoveAbsoluteEx_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveAbsoluteExCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.MoveAbsoluteEx(
                    ParseDouble(TextAbsPosition.Text),
                    ParseDouble(TextVelocity.Text),
                    ParseDouble(TextMotionAcceleration.Text),
                    ParseDouble(TextMotionDeceleration.Text),
                    ParseDouble(TextMotionJerk.Text),
                    (MC_DIRECTION_ENUM)ComboDirection.SelectedItem,
                    (MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            });
        }

        private void ButtonMoveRelativeEx_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveRelativeExCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.MoveRelativeEx(
                    ParseDouble(TextRelDistance.Text),
                    ParseDouble(TextVelocity.Text),
                    ParseDouble(TextMotionAcceleration.Text),
                    ParseDouble(TextMotionDeceleration.Text),
                    ParseDouble(TextMotionJerk.Text),
                    (MC_DIRECTION_ENUM)ComboDirection.SelectedItem,
                    (MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            });
        }

        private void ButtonMoveVelocityEx_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_MoveVelocityExCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.MoveVelocityEx(
                    ParseDouble(TextVelocity.Text),
                    ParseDouble(TextMotionAcceleration.Text),
                    ParseDouble(TextMotionDeceleration.Text),
                    ParseDouble(TextMotionJerk.Text),
                    (MC_DIRECTION_ENUM)ComboDirection.SelectedItem,
                    (MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem);
            });
        }

        private void ButtonSetOverride_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_SetOverrideCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.SetOverride(
                    ParseSingle(TextOverrideAcceleration.Text),
                    ParseSingle(TextOverrideJerk.Text),
                    ParseSingle(TextOverrideVelocity.Text),
                    ParseUInt16(TextOverrideIndex.Text));
            });
        }

        private void ButtonSendSdoRead_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_SendSdoCmd", delegate
            {
                Context.EnsureAxis();
                var objectIndex = ParseUInt16(TextSdoIndex.Text);
                var objectSubIndex = ParseByte(TextSdoSubIndex.Text);
                switch (Convert.ToString(ComboSdoType.SelectedItem, CultureInfo.InvariantCulture))
                {
                    case "Byte":
                        byte byteValue;
                        Context.SingleAxis.UploadSDO(objectIndex, objectSubIndex, out byteValue, 500);
                        TextSdoValue.Text = byteValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "Int16":
                        short int16Value;
                        Context.SingleAxis.UploadSDO(objectIndex, objectSubIndex, out int16Value, 500);
                        TextSdoValue.Text = int16Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "UInt16":
                        ushort uint16Value;
                        Context.SingleAxis.UploadSDO(objectIndex, objectSubIndex, out uint16Value, 500);
                        TextSdoValue.Text = uint16Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "Int32":
                        int int32Value;
                        Context.SingleAxis.UploadSDO(objectIndex, objectSubIndex, out int32Value, 500);
                        TextSdoValue.Text = int32Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "UInt32":
                        uint uint32Value;
                        Context.SingleAxis.UploadSDO(objectIndex, objectSubIndex, out uint32Value, 500);
                        TextSdoValue.Text = uint32Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "Float":
                        float floatValue;
                        Context.SingleAxis.UploadSDO(objectIndex, objectSubIndex, out floatValue, 500);
                        TextSdoValue.Text = floatValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported SDO type.");
                }

                Context.Log("SDO read value = " + TextSdoValue.Text);
            });
        }

        private void ButtonSendSdoWrite_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_SendSdoCmd", delegate
            {
                Context.EnsureAxis();
                var objectIndex = ParseUInt16(TextSdoIndex.Text);
                var objectSubIndex = ParseByte(TextSdoSubIndex.Text);
                switch (Convert.ToString(ComboSdoType.SelectedItem, CultureInfo.InvariantCulture))
                {
                    case "Byte":
                        Context.SingleAxis.DownloadSDO(objectIndex, objectSubIndex, ParseByte(TextSdoValue.Text), 500);
                        break;
                    case "Int16":
                        Context.SingleAxis.DownloadSDO(objectIndex, objectSubIndex, short.Parse(NormalizeNumeric(TextSdoValue.Text), CultureInfo.InvariantCulture), 500);
                        break;
                    case "UInt16":
                        Context.SingleAxis.DownloadSDO(objectIndex, objectSubIndex, ParseUInt16(TextSdoValue.Text), 500);
                        break;
                    case "Int32":
                        Context.SingleAxis.DownloadSDO(objectIndex, objectSubIndex, ParseInt32(TextSdoValue.Text), 500);
                        break;
                    case "UInt32":
                        Context.SingleAxis.DownloadSDO(objectIndex, objectSubIndex, ParseUInt32(TextSdoValue.Text), 500);
                        break;
                    case "Float":
                        Context.SingleAxis.DownloadSDO(objectIndex, objectSubIndex, ParseSingle(TextSdoValue.Text), 500);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported SDO type.");
                }
            });
        }

        private void ButtonHomeDs402Ex_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction("MMC_HomeDS402ExCmd", delegate
            {
                Context.EnsureAxis();
                Context.SingleAxis.HomeDS402Ex(
                    ParseDouble(TextHomePosition.Text),
                    ParseDouble(TextHomeDetectionVelocityLimit.Text),
                    ParseSingle(TextHomeAccel.Text),
                    ParseSingle(TextHomeVelocityHigh.Text),
                    ParseSingle(TextHomeVelocityLow.Text),
                    ParseSingle(TextHomeDistanceLimit.Text),
                    ParseSingle(TextHomeTorqueLimit.Text),
                    (MC_BUFFERED_MODE_ENUM)ComboBufferedMode.SelectedItem,
                    ParseInt32(TextHomeMethod.Text),
                    ParseUInt32(TextHomeTimeLimit.Text),
                    ParseUInt32(TextHomeDetectionTimeLimit.Text),
                    1,
                    new byte[8]);
            });
        }
    }
}

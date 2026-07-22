using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private const ushort AdminGroupReference = 0x0100;
        private LMCAdminCapabilities adminCapabilities;

        private void InitializeReadOnlyApiUi()
        {
            var physicalAxisReferences = new ushort[] { 1, 2, 3, 4 };
            ComboAdminAxisReference.ItemsSource = physicalAxisReferences;
            ComboAdminAxisReference.SelectedItem = (ushort)1;
            ComboDriveReadAxisReference.ItemsSource = physicalAxisReferences;
            ComboDriveReadAxisReference.SelectedItem = (ushort)1;

            ComboAdminAxisParameter.ItemsSource = new[]
            {
                LMCAxisParameterKey.SoftwareMinPosition,
                LMCAxisParameterKey.SoftwareMaxPosition,
                LMCAxisParameterKey.EndPositionToleranceWindow,
                LMCAxisParameterKey.MaxVelocity,
                LMCAxisParameterKey.MaxAcceleration,
                LMCAxisParameterKey.ReferencePosition
            };
            ComboAdminAxisParameter.SelectedItem =
                LMCAxisParameterKey.SoftwareMinPosition;

            ComboAdminGroupSelection.ItemsSource = new[]
            {
                LMCGroupParameterSelection.PathVelocityLimit,
                LMCGroupParameterSelection.PathAccelerationLimit,
                LMCGroupParameterSelection.JerkTime,
                LMCGroupParameterSelection.All
            };
            ComboAdminGroupSelection.SelectedItem =
                LMCGroupParameterSelection.All;
        }

        private void ClearReadOnlyApiState()
        {
            adminCapabilities = null;
            if (TextAdminCapabilities != null)
            {
                TextAdminCapabilities.Text =
                    "Admin capabilities have not been read.";
                TextAdminAxisParameterResult.Text =
                    "No axis parameter result.";
                TextAdminGroupParameterResult.Text =
                    "No group parameter result.";
                TextDriveReadResult.Text = "No drive read result.";
            }
        }

        private void UpdateReadOnlyApiUiState(bool connected, bool idle)
        {
            if (ButtonAdminCapabilities == null)
            {
                return;
            }

            var canReadAxisParameter = adminCapabilities != null
                && adminCapabilities.Supports(
                    LMCAdminFeature.AxisParameterRead);
            var canReadGroupParameters = adminCapabilities != null
                && adminCapabilities.Supports(
                    LMCAdminFeature.GroupParameterRead);

            ButtonAdminCapabilities.IsEnabled = connected && idle;
            ButtonReadAdminAxisParameter.IsEnabled = connected
                && idle
                && canReadAxisParameter;
            ButtonReadAdminGroupParameters.IsEnabled = connected
                && idle
                && canReadGroupParameters;
            ButtonGetDriveOperationMode.IsEnabled = connected && idle;
            ButtonReadDriveStatus.IsEnabled = connected && idle;

            ComboAdminAxisReference.IsEnabled = idle;
            ComboAdminAxisParameter.IsEnabled = idle;
            ComboAdminGroupSelection.IsEnabled = idle;
            ComboDriveReadAxisReference.IsEnabled = idle;
        }

        private async void ButtonAdminCapabilities_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Refresh Admin Capabilities",
                async () =>
                {
                    var currentConnection = RequireConnection();
                    adminCapabilities = null;
                    TextAdminCapabilities.Text =
                        "Refreshing Admin capabilities...";
                    adminCapabilities = await currentConnection.Admin
                        .GetCapabilitiesAsync(CancellationToken.None);
                    TextAdminCapabilities.Text =
                        FormatAdminCapabilities(adminCapabilities);
                });
        }

        private async void ButtonReadAdminAxisParameter_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Axis Parameter",
                async () =>
                {
                    var currentCapabilities = RequireAdminCapabilities();
                    var axisReference = RequirePhysicalAxisReference(
                        ComboAdminAxisReference,
                        "Admin axis reference");
                    var key = RequireSelectedEnum<LMCAxisParameterKey>(
                        ComboAdminAxisParameter,
                        "Axis parameter key");
                    if (!currentCapabilities.Supports(key))
                    {
                        throw new NotSupportedException(
                            "The cached Admin capabilities do not advertise "
                            + key
                            + ". Refresh Admin Capabilities after changing the PLC program.");
                    }

                    var result = await RequireConnection().Admin
                        .ReadAxisParameterAsync(
                            axisReference,
                            key,
                            CancellationToken.None);
                    TextAdminAxisParameterResult.Text =
                        FormatAdminAxisParameter(result);
                });
        }

        private async void ButtonReadAdminGroupParameters_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Group Parameters",
                async () =>
                {
                    var currentCapabilities = RequireAdminCapabilities();
                    var selection =
                        RequireSelectedEnum<LMCGroupParameterSelection>(
                            ComboAdminGroupSelection,
                            "Group parameter selection");
                    if (!currentCapabilities.Supports(selection))
                    {
                        throw new NotSupportedException(
                            "The cached Admin capabilities do not advertise "
                            + selection
                            + ". Refresh Admin Capabilities after changing the PLC program.");
                    }

                    var result = await RequireConnection().Admin
                        .ReadGroupParametersAsync(
                            AdminGroupReference,
                            selection,
                            CancellationToken.None);
                    TextAdminGroupParameterResult.Text =
                        FormatAdminGroupParameters(result);
                });
        }

        private async void ButtonGetDriveOperationMode_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Get Drive Operation Mode",
                async () =>
                {
                    var axisReference = RequirePhysicalAxisReference(
                        ComboDriveReadAxisReference,
                        "Drive axis reference");
                    var currentAxis = await GetPhysicalAxisAsync(axisReference);
                    var result = await currentAxis.GetDriveOperationModeAsync(
                        CancellationToken.None);
                    TextDriveReadResult.Text =
                        FormatDriveOperationMode(result);
                });
        }

        private async void ButtonReadDriveStatus_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Read Drive Status",
                async () =>
                {
                    var axisReference = RequirePhysicalAxisReference(
                        ComboDriveReadAxisReference,
                        "Drive axis reference");
                    var currentAxis = await GetPhysicalAxisAsync(axisReference);
                    var result = await currentAxis.ReadDriveStatusAsync(
                        CancellationToken.None);
                    TextDriveReadResult.Text = FormatDriveStatus(result);
                });
        }

        private LMCAdminCapabilities RequireAdminCapabilities()
        {
            RequireConnection();
            if (adminCapabilities == null)
            {
                throw new InvalidOperationException(
                    "Refresh Admin Capabilities first.");
            }

            return adminCapabilities;
        }

        private async Task<LMCSingleAxis> GetPhysicalAxisAsync(
            ushort expectedAxisReference)
        {
            var currentConnection = RequireConnection();
            if (axis != null && axis.AxisReference == expectedAxisReference)
            {
                return axis;
            }

            var axisName = "_LMCAxis"
                + expectedAxisReference.ToString(
                    CultureInfo.InvariantCulture);
            var selectedAxis = await LMCSingleAxis.CreateAsync(
                currentConnection,
                axisName,
                CancellationToken.None);
            if (selectedAxis.AxisReference != expectedAxisReference)
            {
                throw new InvalidDataException(
                    axisName
                    + " resolved to axis reference "
                    + selectedAxis.AxisReference
                    + " instead of "
                    + expectedAxisReference
                    + ".");
            }

            return selectedAxis;
        }

        private static ushort RequirePhysicalAxisReference(
            ComboBox comboBox,
            string fieldName)
        {
            if (!(comboBox.SelectedItem is ushort))
            {
                throw new InvalidOperationException(
                    fieldName + " is required.");
            }

            var axisReference = (ushort)comboBox.SelectedItem;
            if (axisReference < 1 || axisReference > 4)
            {
                throw new InvalidOperationException(
                    fieldName + " must be between 1 and 4.");
            }

            return axisReference;
        }

        private static string FormatAdminCapabilities(
            LMCAdminCapabilities capabilities)
        {
            return "Schema="
                + capabilities.Response.SchemaVersion
                + ", Features="
                + capabilities.Features
                + ", RequestId="
                + capabilities.Response.RequestId
                + Environment.NewLine
                + "PhysicalAxes="
                + capabilities.PhysicalAxisCount
                + ", AxisParameterMask=0x"
                + capabilities.AxisParameterMask.ToString("X8")
                + ", MaxAxisParameters="
                + capabilities.MaxAxisParameterCount
                + Environment.NewLine
                + "GroupRef=0x"
                + capabilities.GroupReference.ToString("X4")
                + ", GroupSelection="
                + capabilities.GroupParameterSelection
                + ", MaxGroupParameters="
                + capabilities.MaxGroupParameterCount
                + ", ErrorCatalogVersion="
                + capabilities.ErrorCatalogVersion;
        }

        private static string FormatAdminAxisParameter(
            LMCAxisParameterResult result)
        {
            return "AxisRef="
                + result.AxisReference
                + ", Key="
                + result.Key
                + ", Value="
                + result.Value
                + ", Type="
                + result.ValueType
                + ", Unit="
                + result.Unit
                + ", RequestId="
                + result.Response.RequestId;
        }

        private static string FormatAdminGroupParameters(
            LMCGroupParametersResult result)
        {
            return "GroupRef=0x"
                + result.GroupReference.ToString("X4")
                + ", Selection="
                + result.Selection
                + ", RequestId="
                + result.Response.RequestId
                + Environment.NewLine
                + FormatAdminGroupValue(
                    result,
                    LMCGroupParameterKey.PathVelocityLimit)
                + ", "
                + FormatAdminGroupValue(
                    result,
                    LMCGroupParameterKey.PathAccelerationLimit)
                + ", "
                + FormatAdminGroupValue(
                    result,
                    LMCGroupParameterKey.JerkTime);
        }

        private static string FormatAdminGroupValue(
            LMCGroupParametersResult result,
            LMCGroupParameterKey key)
        {
            int value;
            LMCAdminUnit unit;
            return result.TryGetValue(key, out value, out unit)
                ? key + "=" + value + " " + unit
                : key + "=<not selected>";
        }

        private static string FormatDriveOperationMode(
            LMCDriveOperationModeResult result)
        {
            return "AxisRef="
                + result.AxisReference
                + ", Mode="
                + result.Mode
                + ", Raw="
                + result.RawValue
                + ", Known="
                + result.IsKnownMode
                + Environment.NewLine
                + "TicketId="
                + result.Ticket.TicketId
                + ", State="
                + result.OperationStatus.State
                + ", CompletionCycle="
                + result.OperationStatus.CompletionCycle;
        }

        private static string FormatDriveStatus(LMCDriveStatus result)
        {
            return "AxisRef="
                + result.AxisReference
                + ", ReadSuccessful="
                + result.IsReadSuccessful
                + ", Atomic="
                + result.IsAtomicSnapshot
                + Environment.NewLine
                + "LASAL State=0x"
                + result.AxisStatus.State.ToString("X8")
                + ", AxisErrorFlags=0x"
                + result.AxisErrorFlags.ToString("X4")
                + Environment.NewLine
                + "DS402 0x6041:0=0x"
                + result.Ds402StatusWord.ToString("X4")
                + ", 0x6061:0="
                + result.OperationMode
                + " (raw "
                + result.OperationModeRaw
                + ")"
                + Environment.NewLine
                + "PositionLimit="
                + result.IsLasalPositionLimitActive
                + ", DS402InternalLimit="
                + result.IsDs402InternalLimitActive
                + ", AnyLimit="
                + result.HasAnyLimitIndication
                + Environment.NewLine
                + "StatusWordTicket="
                + result.StatusWordTicket.TicketId
                + ", ModeTicket="
                + result.OperationModeResult.Ticket.TicketId;
        }
    }
}

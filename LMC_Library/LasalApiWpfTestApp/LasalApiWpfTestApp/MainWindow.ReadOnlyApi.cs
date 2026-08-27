using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        private const string AxisSetOperationModeRejectEvidenceMagic =
            "ELMOASOMREJECT1";
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

            InitializeAxisSetOperationModeRecoveryUi(
                physicalAxisReferences);
            buttonStartAxisSetOperationMode.Click -=
                ButtonStartAxisSetOperationMode_Click;
            buttonStartAxisSetOperationMode.Click +=
                ButtonStartAxisSetOperationModeWithRejectResolution_Click;
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
            ButtonGetDriveOperationMode.IsEnabled = connected
                && idle
                && EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation
                        .TrackedD5ReadOnlyInspection)
                    .IsAllowed;
            ButtonReadDriveStatus.IsEnabled = connected
                && idle
                && EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation
                        .TrackedD5ReadOnlyInspection)
                    .IsAllowed;
            ButtonGetDriveErrorCode.IsEnabled = connected
                && idle
                && EvaluateDiagnosticsAdmission(
                    DiagnosticsAdmissionOperation
                        .TrackedD5ReadOnlyInspection)
                    .IsAllowed;

            ComboAdminAxisReference.IsEnabled = idle;
            ComboAdminAxisParameter.IsEnabled = idle;
            ComboAdminGroupSelection.IsEnabled = idle;
            ComboDriveReadAxisReference.IsEnabled = idle;

            UpdateAxisSetOperationModeRecoveryUiState(
                connected,
                idle);
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
                    EnsureDiagnosticsAdmission(
                        DiagnosticsAdmissionOperation
                            .TrackedD5ReadOnlyInspection,
                        "Get Drive Operation Mode");
                    var axisReference = RequirePhysicalAxisReference(
                        ComboDriveReadAxisReference,
                        "Drive axis reference");
                    var currentConnection = RequireConnection();
                    var currentAxis = await GetPhysicalAxisAsync(axisReference);
                    var result = await RunTrackedExternalD5ReadAsync(
                        currentConnection,
                        axisReference,
                        LMCSingleAxis.DefaultDriveReadTimeoutCycles,
                        "drive-operation-mode-0x6061",
                        1,
                        () => currentAxis.GetDriveOperationModeAsync(
                            CancellationToken.None));
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
                    EnsureDiagnosticsAdmission(
                        DiagnosticsAdmissionOperation
                            .TrackedD5ReadOnlyInspection,
                        "Read Drive Status");
                    var axisReference = RequirePhysicalAxisReference(
                        ComboDriveReadAxisReference,
                        "Drive axis reference");
                    var currentConnection = RequireConnection();
                    var currentAxis = await GetPhysicalAxisAsync(axisReference);
                    var result = await RunTrackedExternalD5ReadAsync(
                        currentConnection,
                        axisReference,
                        LMCSingleAxis.DefaultDriveReadTimeoutCycles,
                        "drive-status-0x6041-0x6061",
                        2,
                        () => currentAxis.ReadDriveStatusAsync(
                            CancellationToken.None));
                    TextDriveReadResult.Text = FormatDriveStatus(result);
                });
        }

        private async void ButtonGetDriveErrorCode_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Get Drive Error Code",
                async () =>
                {
                    EnsureDiagnosticsAdmission(
                        DiagnosticsAdmissionOperation
                            .TrackedD5ReadOnlyInspection,
                        "Get Drive Error Code");
                    var axisReference = RequirePhysicalAxisReference(
                        ComboDriveReadAxisReference,
                        "Drive axis reference");
                    var currentConnection = RequireConnection();
                    var currentAxis = await GetPhysicalAxisAsync(axisReference);
                    var result = await RunTrackedExternalD5ReadAsync(
                        currentConnection,
                        axisReference,
                        LMCSingleAxis.DefaultDriveReadTimeoutCycles,
                        "drive-error-code-0x603F",
                        2,
                        () => currentAxis.GetDriveErrorCodeAsync(
                            CancellationToken.None));
                    TextDriveReadResult.Text = FormatDriveErrorCode(result);
                });
        }

        private async void
            ButtonStartAxisSetOperationModeWithRejectResolution_Click(
                object sender,
                RoutedEventArgs e)
        {
            await RunOperationAsync(
                "Set Operation Mode Selected Mode Once",
                async () =>
                {
                    try
                    {
                        await StartAxisSetOperationModeOnceAsync();
                    }
                    catch (LMCAxisSetOperationModeRejectedException error)
                    {
                        var record =
                            RequireActiveAxisSetOperationModeRecoveryRecord(
                                "definitive SetOperationMode Start rejection");
                        var evidencePath =
                            ResolveDefinitiveAxisSetOperationModeStartRejection(
                                record,
                                error.Acknowledgement.PreparedCommand.RecoveryKey,
                                error.Response.SchemaVersion,
                                error.Response.CommandStatus,
                                error.Response.ErrorId,
                                error.Response.RequestId,
                                error.Response.DetailCodeValue,
                                error.Response.IsSuccess,
                                DateTime.UtcNow);
                        RefreshAxisSetOperationModeRecoveryUi(
                            "START REJECTED DEFINITIVELY: "
                            + error.Response.DetailCode
                            + ". PLC rejected the request before creating a retained SetOperationMode outcome. "
                            + "The rejection and original pre-dispatch journal were archived durably at "
                            + evidencePath
                            + "; the recovery interlock is cleared. A future Start requires a new explicit confirmation and new identity.");
                        UpdateUiState();
                        throw;
                    }
                });
        }

        internal string ResolveAxisSetOperationModeDefinitiveRejectionForTests(
            AxisSetOperationModeRecoveryRecord captured,
            LMCAxisSetOperationModeRecoveryKey rejectedKey,
            ushort responseSchemaVersion,
            ushort commandStatus,
            short errorId,
            uint responseRequestId,
            uint detailCode,
            bool responseIsSuccess)
        {
            return ResolveDefinitiveAxisSetOperationModeStartRejection(
                captured,
                rejectedKey,
                responseSchemaVersion,
                commandStatus,
                errorId,
                responseRequestId,
                detailCode,
                responseIsSuccess,
                DateTime.UtcNow);
        }

        private string ResolveDefinitiveAxisSetOperationModeStartRejection(
            AxisSetOperationModeRecoveryRecord captured,
            LMCAxisSetOperationModeRecoveryKey rejectedKey,
            ushort responseSchemaVersion,
            ushort commandStatus,
            short errorId,
            uint responseRequestId,
            uint detailCode,
            bool responseIsSuccess,
            DateTime rejectedUtc)
        {
            var journal = axisSetOperationModeRecoveryJournal;
            if (journal == null || captured == null || rejectedKey == null)
            {
                throw new InvalidOperationException(
                    "Definitive SetOperationMode rejection cannot resolve without the exact durable journal and recovery key.");
            }

            var current = journal.CurrentRecord;
            if (current == null
                || !current.IsActive
                || current.Identity != captured.Identity
                || current.Revision != captured.Revision
                || current.State != captured.State
                || (current.State
                        != AxisSetOperationModeRecoveryState.RecoveryRequired
                    && current.State
                        != AxisSetOperationModeRecoveryState.ArmedBeforeDispatch))
            {
                throw new InvalidOperationException(
                    "Definitive SetOperationMode rejection cannot resolve a stale or non-preterminal recovery record.");
            }

            if (!current.MatchesRecoveryKey(rejectedKey)
                || responseSchemaVersion != current.SchemaVersion
                || responseRequestId != current.OriginalRequestId
                || responseIsSuccess
                || (commandStatus == 0 && errorId == 0 && detailCode == 0))
            {
                throw new InvalidOperationException(
                    "Definitive SetOperationMode rejection proof does not match the exact durable request identity.");
            }

            if (rejectedUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "SetOperationMode rejection evidence timestamp must be UTC.",
                    "rejectedUtc");
            }

            var journalPath = journal.JournalFilePath;
            var directory = Path.GetDirectoryName(journalPath);
            if (string.IsNullOrWhiteSpace(directory)
                || !File.Exists(journalPath))
            {
                throw new InvalidOperationException(
                    "The durable SetOperationMode journal file is unavailable for rejection archival.");
            }

            var originalJournalBytes = File.ReadAllBytes(journalPath);
            var evidencePath = Path.Combine(
                directory,
                "axis-set-operation-mode-rejected-"
                    + current.Identity.ToString("N")
                    + ".evidence");
            PersistAxisSetOperationModeRejectEvidence(
                evidencePath,
                current,
                rejectedKey,
                responseSchemaVersion,
                commandStatus,
                errorId,
                responseRequestId,
                detailCode,
                rejectedUtc,
                originalJournalBytes);

            try
            {
                File.Delete(journalPath);
                if (File.Exists(journalPath))
                {
                    throw new IOException(
                        "The active SetOperationMode journal could not be removed after durable rejection archival.");
                }

                journal.Dispose();
                axisSetOperationModeRecoveryJournal = null;
                axisSetOperationModeRecoveryJournal =
                    AxisSetOperationModeRecoveryJournal.Open(directory);
                axisSetOperationModeRecoveryJournalError = null;
                if (axisSetOperationModeRecoveryJournal.HasActiveRecord)
                {
                    throw new InvalidDataException(
                        "The SetOperationMode journal unexpectedly remained active after definitive rejection archival.");
                }
            }
            catch (Exception error)
            {
                if (axisSetOperationModeRecoveryJournal != null)
                {
                    axisSetOperationModeRecoveryJournal.Dispose();
                    axisSetOperationModeRecoveryJournal = null;
                }
                axisSetOperationModeRecoveryJournalError =
                    error.GetType().Name + ": " + error.Message;
                throw;
            }

            WriteLog(
                "SetOperationMode definitive Start rejection archived durably; no retained PLC outcome exists and the recovery interlock was cleared. Evidence="
                + evidencePath
                + ".");
            return evidencePath;
        }

        private static void PersistAxisSetOperationModeRejectEvidence(
            string evidencePath,
            AxisSetOperationModeRecoveryRecord record,
            LMCAxisSetOperationModeRecoveryKey rejectedKey,
            ushort responseSchemaVersion,
            ushort commandStatus,
            short errorId,
            uint responseRequestId,
            uint detailCode,
            DateTime rejectedUtc,
            byte[] originalJournalBytes)
        {
            if (File.Exists(evidencePath))
            {
                throw new IOException(
                    "SetOperationMode definitive-rejection evidence already exists for this journal identity.");
            }

            var lines = new[]
            {
                AxisSetOperationModeRejectEvidenceMagic,
                "FormatVersion=1",
                "Identity=" + record.Identity.ToString("N"),
                "JournalState=" + ((int)record.State).ToString(CultureInfo.InvariantCulture),
                "JournalRevision=" + record.Revision.ToString(CultureInfo.InvariantCulture),
                "EndpointIpBase64=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(record.EndpointIp)),
                "EndpointPort=" + record.EndpointPort.ToString(CultureInfo.InvariantCulture),
                "AxisNameBase64=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(record.AxisName)),
                "SchemaVersion=" + record.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                "OriginalRequestId=" + record.OriginalRequestId.ToString(CultureInfo.InvariantCulture),
                "DiagnosticsBuild=" + record.DiagnosticsBuild.ToString(CultureInfo.InvariantCulture),
                "DiagnosticsBootId=" + record.DiagnosticsBootId.ToString(CultureInfo.InvariantCulture),
                "MapRevision=" + record.MapRevision.ToString(CultureInfo.InvariantCulture),
                "ClientIntentId0=" + record.ClientIntentId0.ToString(CultureInfo.InvariantCulture),
                "ClientIntentId1=" + record.ClientIntentId1.ToString(CultureInfo.InvariantCulture),
                "ClientIntentId2=" + record.ClientIntentId2.ToString(CultureInfo.InvariantCulture),
                "ClientIntentId3=" + record.ClientIntentId3.ToString(CultureInfo.InvariantCulture),
                "AxisReference=" + record.AxisReference.ToString(CultureInfo.InvariantCulture),
                "RequestedModeRaw=" + record.RequestedModeRaw.ToString(CultureInfo.InvariantCulture),
                "TimeoutMilliseconds=" + record.TimeoutMilliseconds.ToString(CultureInfo.InvariantCulture),
                "Flags=" + record.Flags.ToString(CultureInfo.InvariantCulture),
                "RejectedKeyExact=" + record.MatchesRecoveryKey(rejectedKey),
                "ResponseSchemaVersion=" + responseSchemaVersion.ToString(CultureInfo.InvariantCulture),
                "ResponseCommandStatus=" + commandStatus.ToString(CultureInfo.InvariantCulture),
                "ResponseErrorId=" + errorId.ToString(CultureInfo.InvariantCulture),
                "ResponseRequestId=" + responseRequestId.ToString(CultureInfo.InvariantCulture),
                "ResponseDetailCode=" + detailCode.ToString(CultureInfo.InvariantCulture),
                "RejectedUtcTicks=" + rejectedUtc.Ticks.ToString(CultureInfo.InvariantCulture),
                "OriginalJournalSha256=" + ComputeAxisSetOperationModeRejectSha256Hex(originalJournalBytes),
                "OriginalJournalBase64=" + Convert.ToBase64String(originalJournalBytes)
            };
            var payload = string.Join("\n", lines) + "\n";
            var payloadBytes = new UTF8Encoding(false).GetBytes(payload);
            var finalBytes = new UTF8Encoding(false).GetBytes(
                payload
                + "SHA256="
                + ComputeAxisSetOperationModeRejectSha256Hex(payloadBytes)
                + "\n");
            var temporaryPath = evidencePath + ".tmp";
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            var temporaryExists = false;
            try
            {
                using (var stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough))
                {
                    temporaryExists = true;
                    stream.Write(finalBytes, 0, finalBytes.Length);
                    stream.Flush(true);
                }
                File.Move(temporaryPath, evidencePath);
                temporaryExists = false;
            }
            finally
            {
                if (temporaryExists && File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static string ComputeAxisSetOperationModeRejectSha256Hex(
            byte[] bytes)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(bytes ?? new byte[0]);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var value in hash)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }
                return builder.ToString();
            }
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

        private static string FormatDriveErrorCode(
            LMCDriveErrorCodeResult result)
        {
            return "AxisRef="
                + result.AxisReference
                + ", DS402 0x603F:0=0x"
                + result.ErrorCode.ToString("X4")
                + ", HasError="
                + result.HasError
                + ", ReadSuccessful="
                + result.IsSuccessful
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
                + ", DS402Fault="
                + result.HasDs402Fault
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

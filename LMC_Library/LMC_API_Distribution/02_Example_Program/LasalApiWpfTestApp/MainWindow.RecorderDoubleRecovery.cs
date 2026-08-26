using System;
using System.Globalization;
using System.Text;
using System.Windows;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private readonly string recorderDoubleRecoveryJournalDirectoryPath;
        private RecorderDoubleRecoveryJournal
            recorderDoubleRecoveryJournal;
        private string recorderDoubleRecoveryJournalOpenError;
        private string recorderDoubleRecoveryJournalRuntimeError;
        private bool recorderDoubleRecoveryRecoveredAtStartup;

        private bool HasActiveRecorderDoubleRecoveryJournalRecord
        {
            get
            {
                return recorderDoubleRecoveryJournal != null
                    && recorderDoubleRecoveryJournal.HasActiveRecord;
            }
        }

        private bool RecorderDoubleRecoveryJournalUnavailable
        {
            get
            {
                return recorderDoubleRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        recorderDoubleRecoveryJournalOpenError)
                    || !string.IsNullOrEmpty(
                        recorderDoubleRecoveryJournalRuntimeError);
            }
        }

        private bool AnyDiagnosticsMutationJournalUnavailable
        {
            get
            {
                return DiagnosticsMutationJournalUnavailable
                    || RecorderDoubleRecoveryJournalUnavailable;
            }
        }

        private void InitializeRecorderDoubleRecoveryJournal()
        {
            try
            {
                recorderDoubleRecoveryJournal =
                    recorderDoubleRecoveryJournalDirectoryPath == null
                        ? RecorderDoubleRecoveryJournal.OpenDefault()
                        : RecorderDoubleRecoveryJournal.Open(
                            recorderDoubleRecoveryJournalDirectoryPath);
                recorderDoubleRecoveryRecoveredAtStartup =
                    recorderDoubleRecoveryJournal.HasActiveRecord;
                recorderDoubleRecoveryJournalOpenError = null;
                recorderDoubleRecoveryJournalRuntimeError = null;
            }
            catch (Exception error)
            {
                recorderDoubleRecoveryJournal = null;
                recorderDoubleRecoveryRecoveredAtStartup = false;
                recorderDoubleRecoveryJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
            }

            RefreshRecorderDoubleRecoveryJournalUi();
        }

        private void DisposeRecorderDoubleRecoveryJournal()
        {
            var journal = recorderDoubleRecoveryJournal;
            recorderDoubleRecoveryJournal = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private string GetAnyDiagnosticsMutationJournalUnavailableGuidance()
        {
            var guidance = new StringBuilder();
            if (DiagnosticsMutationJournalUnavailable)
            {
                guidance.Append(
                    GetDiagnosticsMutationJournalUnavailableGuidance());
            }

            if (RecorderDoubleRecoveryJournalUnavailable)
            {
                if (guidance.Length != 0)
                {
                    guidance.Append(' ');
                }

                guidance.Append(
                    GetRecorderDoubleRecoveryJournalUnavailableGuidance());
            }

            if (AxisPowerOnRecoveryJournalUnavailable)
            {
                if (guidance.Length != 0)
                {
                    guidance.Append(' ');
                }

                guidance.Append(GetAxisPowerOnRecoveryGuidance());
            }

            if (GroupPowerRecoveryJournalUnavailable)
            {
                if (guidance.Length != 0)
                {
                    guidance.Append(' ');
                }

                guidance.Append(GetGroupPowerRecoveryGuidance());
            }

            if (GroupResetRecoveryJournalUnavailable)
            {
                if (guidance.Length != 0)
                {
                    guidance.Append(' ');
                }

                guidance.Append(GetGroupResetRecoveryGuidance());
            }

            return guidance.ToString();
        }

        private string GetRecorderDoubleRecoveryJournalGuidance()
        {
            if (!HasActiveRecorderDoubleRecoveryJournalRecord)
            {
                return string.Empty;
            }

            return "Durable Double-bank recovery evidence remains: "
                + FormatRecorderDoubleRecoveryRecord(
                    recorderDoubleRecoveryJournal.CurrentRecord)
                + (recorderDoubleRecoveryRecoveredAtStartup
                    ? " It was recovered at startup."
                    : string.Empty)
                + " Automatic Configure, Start, inventory, adoption, and Release replay are disabled. Exact recovery remains behind the closed ReconnectRecovery proof gate.";
        }

        private string
            GetRecorderDoubleRecoveryJournalUnavailableGuidance()
        {
            var detail = recorderDoubleRecoveryJournalRuntimeError
                ?? recorderDoubleRecoveryJournalOpenError
                ?? "No Double-bank recovery journal was opened.";
            return "The Double-bank recovery journal is unavailable. New live/mutation commands are disabled; ordinary non-D5 read-only inspection, Stop, PowerOff, and Group Stop remain available. No Double-bank recovery command will be replayed. "
                + detail;
        }

        private static string FormatRecorderDoubleRecoveryRecord(
            RecorderDoubleRecoveryRecord record)
        {
            if (record == null)
            {
                return "No Double-bank recovery record.";
            }

            var bankText = new StringBuilder();
            for (var index = 0; index < record.Banks.Count; index++)
            {
                if (index != 0)
                {
                    bankText.Append(',');
                }

                var bank = record.Banks[index];
                bankText.Append("B")
                    .Append(bank.BufferId.ToString(
                        CultureInfo.InvariantCulture))
                    .Append(":R")
                    .Append(bank.RecordId.ToString(
                        CultureInfo.InvariantCulture));
            }

            return "Identity="
                + record.Identity.ToString("D")
                + ", State="
                + record.State
                + ", BootId=0x"
                + record.DiagnosticsBootId.ToString("X8")
                + ", MapRevision=0x"
                + record.MapRevision.ToString("X8")
                + ", ConfigId=0x"
                + record.RequestedConfigId.ToString("X8")
                + ", ConfigRevision=0x"
                + record.ConfigRevision.ToString("X8")
                + ", Banks="
                + (bankText.Length == 0 ? "none" : bankText.ToString())
                + ", BankReleaseIntent=0x"
                + record.BankReleaseIntentMask.ToString("X2")
                + ", BankReleaseConfirmed=0x"
                + record.BankReleaseConfirmedMask.ToString("X2")
                + ", ConfigReleaseIntent="
                + record.ConfigurationReleaseIntent
                + ", ConfigReleaseConfirmed="
                + record.ConfigurationReleaseConfirmed
                + ", TokenMarker="
                + record.RecoveryTokenMarker
                + ", UpdatedUtc="
                + record.UpdatedUtc.ToString(
                    "O",
                    CultureInfo.InvariantCulture)
                + ".";
        }

        private void RefreshRecorderDoubleRecoveryJournalUi()
        {
            if (TextRecorderDoubleRecoveryStatus == null)
            {
                return;
            }

            if (RecorderDoubleRecoveryJournalUnavailable)
            {
                TextRecorderDoubleRecoveryStatus.Text =
                    "DOUBLE RECOVERY JOURNAL UNAVAILABLE. "
                    + GetRecorderDoubleRecoveryJournalUnavailableGuidance();
                return;
            }

            var record = recorderDoubleRecoveryJournal.CurrentRecord;
            if (record != null && record.IsActive)
            {
                TextRecorderDoubleRecoveryStatus.Text =
                    (recorderDoubleRecoveryRecoveredAtStartup
                        ? "RECOVERED UNRESOLVED DOUBLE RECORD. "
                        : "ACTIVE DOUBLE RECOVERY RECORD. ")
                    + FormatRecorderDoubleRecoveryRecord(record)
                    + (HasRecorderDoubleRetainedQualification
                        ? " Same-session qualification handles are retained."
                        : string.Empty)
                    + (HasRecorderDoubleRetainedRecovery
                        ? " Reconnect recovery handles are retained."
                        : string.Empty)
                    + " Automatic replay is disabled.";
            }
            else if (record != null)
            {
                TextRecorderDoubleRecoveryStatus.Text =
                    "Double recovery journal resolved. "
                    + FormatRecorderDoubleRecoveryRecord(record);
            }
            else
            {
                TextRecorderDoubleRecoveryStatus.Text =
                    "Double recovery journal ready. No unresolved durable Double-bank record.";
            }
        }

        private void UpdateRecorderDoubleRecoveryUiState(
            bool connected,
            bool idle,
            bool recorderDoubleContractReady)
        {
            RefreshRecorderDoubleRecoveryJournalUi();
            var record = HasActiveRecorderDoubleRecoveryJournalRecord
                ? recorderDoubleRecoveryJournal.CurrentRecord
                : null;
            var capabilityIdentityMatches =
                record != null
                && diagnosticCapabilities != null
                && diagnosticCapabilities.DiagnosticsBootId
                    == record.DiagnosticsBootId
                && diagnosticCapabilities.MapRevision
                    == record.MapRevision;
            var explicitConfirmation =
                CheckConfirmRecorderDoubleRelease.IsChecked == true;
            var sameSessionRetained =
                HasRecorderDoubleRetainedQualification
                && ReferenceEquals(
                    connection,
                    recorderDoubleRetainedQualificationConnection)
                && recorderDoubleRetainedQualificationConnection != null
                && recorderDoubleRetainedQualificationConnection.IsConnected;
            var sameSessionCleanupDenial = sameSessionRetained
                ? GetRecorderDoubleLifecycleAdmissionDenial(true)
                : null;
            var sameSessionCleanupRouteReady =
                IsRecorderDoubleSameSessionCleanupRouteReady(
                    recorderDoubleRetainedQualificationScope,
                    RecorderDoubleManualActionsReady,
                    RecorderDoubleManualConfigureRouteReady,
                    RecorderDoubleQualificationExecutionReady,
                    RecorderDoubleReconnectRecoveryReady);
            var releaseConfirmationText =
                BuildRecorderDoubleReleaseConfirmationText(
                    record,
                    sameSessionRetained);
            CheckConfirmRecorderDoubleRelease.Content =
                releaseConfirmationText;
            CheckConfirmRecorderDoubleRelease.ToolTip =
                releaseConfirmationText;
            CheckConfirmRecorderDoubleRelease.IsEnabled =
                connected
                && idle
                && !motionMayBeActive
                && record != null
                && !RecorderDoubleRecoveryJournalUnavailable
                && (sameSessionRetained
                    ? sameSessionCleanupRouteReady
                    : RecorderDoubleReconnectRecoveryReady);
            ButtonReleaseRecorderDoubleRetained.IsEnabled =
                connected
                && idle
                && !motionMayBeActive
                && !RecorderDoubleRecoveryJournalUnavailable
                && record != null
                && sameSessionRetained
                && sameSessionCleanupDenial == null
                && explicitConfirmation
                && sameSessionCleanupRouteReady;
            ButtonReleaseRecorderDoubleRetained.Content =
                sameSessionCleanupRouteReady
                    ? "Cleanup Retained Double"
                    : "Cleanup Retained Double (gates closed)";
            ButtonReleaseRecorderDoubleRetained.ToolTip =
                !sameSessionRetained
                    ? "No exact same-session Double-bank qualification handles are retained."
                    : sameSessionCleanupDenial != null
                        ? "Same-session cleanup is blocked: "
                            + sameSessionCleanupDenial
                        : !sameSessionCleanupRouteReady
                            ? recorderDoubleRetainedQualificationScope != null
                                && recorderDoubleRetainedQualificationScope
                                    .ConfigurationOnlyRetention
                                ? "Manual Double configuration-only cleanup is blocked before wire because its route gates are closed."
                                : "Double-bank retained cleanup is blocked before wire because its proof gates are closed."
                            : !explicitConfirmation
                                ? "Confirm the exact journal identity and Release order first."
                                : recorderDoubleRetainedQualificationScope
                                    .ConfigurationOnlyRetention
                                    ? "Release the exact retained configuration only."
                                    : "Release Bank B, Bank A, then the exact configuration.";
            ButtonRecoverRecorderDoubleJournal.IsEnabled =
                connected
                && idle
                && !motionMayBeActive
                && !RecorderDoubleRecoveryJournalUnavailable
                && record != null
                && !sameSessionRetained
                && recorderDoubleContractReady
                && capabilityIdentityMatches
                && explicitConfirmation
                && RecorderDoubleReconnectRecoveryReady;
            ButtonRecoverRecorderDoubleJournal.Content =
                RecorderDoubleReconnectRecoveryReady
                    ? HasRecorderDoubleRetainedRecovery
                        ? "Continue Double Recovery"
                        : "Recover Double Journal"
                    : "Recover Double Journal (gate closed)";
            ButtonRecoverRecorderDoubleJournal.ToolTip =
                record == null
                    ? "No unresolved Double-bank recovery record exists."
                    : !RecorderDoubleReconnectRecoveryReady
                        ? "Double-bank recovery is blocked before wire: ReconnectRecovery proof gate is CLOSED. No inventory, adoption, or Release command will be sent."
                        : !recorderDoubleContractReady
                            ? "Double-bank recovery requires the exact advertised two-bank contract."
                            : !capabilityIdentityMatches
                                ? "Current capability BootId/MapRevision does not match the durable Double-bank record."
                                : sameSessionRetained
                                    ? sameSessionCleanupDenial != null
                                        ? "Same-session cleanup is unsafe: "
                                            + sameSessionCleanupDenial
                                        : "Use Cleanup Retained Double for the exact current-session handles."
                                    : !explicitConfirmation
                                        ? "Confirm the exact journal identity and Release order first."
                                        : "Recover the exact durable Double-bank record without automatic replay.";
        }

        private string BuildRecorderDoubleReleaseConfirmationText(
            RecorderDoubleRecoveryRecord record,
            bool sameSessionRetained)
        {
            if (record == null)
            {
                return "I verified the exact Double journal identity and full displayed Release plan.";
            }

            var text = new StringBuilder();
            text.Append("I verified journal ");
            text.Append(record.Identity.ToString("D"));
            text.Append(", Config 0x");
            text.Append(record.RequestedConfigId.ToString("X8"));
            text.Append("/");
            text.Append(record.ConfigRevision.ToString(
                CultureInfo.InvariantCulture));
            text.Append(", and exact plan: ");

            if (sameSessionRetained
                && recorderDoubleRetainedQualificationScope != null)
            {
                var scope = recorderDoubleRetainedQualificationScope;
                if (scope.UnexpectedThird != null
                    || (scope.ThirdStartAttempted
                        && !scope.ThirdStartExactBusyConfirmed))
                {
                    text.Append(
                        "third Start was not exact ResourceBusy; same-session Release is prohibited -> disconnect/reconnect for exact inventory inspection only; conflicting inventory requires external manual recovery, no automatic Release");
                }
                else
                {
                    AppendRecorderDoubleQualificationReleaseTarget(
                        text,
                        scope.BankB,
                        "BankB");
                    AppendRecorderDoubleQualificationReleaseTarget(
                        text,
                        scope.BankA,
                        "BankA");
                    text.Append("Configuration");
                }
            }
            else if (record.ConfigRevision == 0)
            {
                text.Append(
                    "4D/4A read-only identity and inventory discovery -> stop before Adopt/Release -> review the exact discovered plan -> confirm again");
            }
            else
            {
                var hasUnconfirmedBank = false;
                for (var index = 0; index < record.Banks.Count; index++)
                {
                    var bank = record.Banks[index];
                    if (!record.IsBankReleaseConfirmed(
                            bank.BufferId,
                            bank.RecordId))
                    {
                        hasUnconfirmedBank = true;
                        break;
                    }
                }

                text.Append(
                    "4A read-only inventory; any new target stops before Adopt/Release and requires confirmation again; unchanged exact plan -> ");
                if (hasUnconfirmedBank)
                {
                    text.Append("exact Adopt -> Status/Stop-to-Ready -> ");
                }

                for (var index = record.Banks.Count - 1;
                    index >= 0;
                    index--)
                {
                    var bank = record.Banks[index];
                    if (record.IsBankReleaseConfirmed(
                            bank.BufferId,
                            bank.RecordId))
                    {
                        continue;
                    }

                    text.Append("Bank");
                    text.Append(bank.BufferId.ToString(
                        CultureInfo.InvariantCulture));
                    text.Append("(Record ");
                    text.Append(bank.RecordId.ToString(
                        CultureInfo.InvariantCulture));
                    text.Append(") -> ");
                }

                text.Append(
                    hasUnconfirmedBank
                        ? "Configuration"
                        : "exact empty Configuration Adopt -> Configuration Release");
            }

            text.Append(". No automatic replay.");
            return text.ToString();
        }

        private static void AppendRecorderDoubleQualificationReleaseTarget(
            StringBuilder text,
            RecorderDoubleBankCaptureLease capture,
            string name)
        {
            if (capture == null || capture.IsReleased)
            {
                return;
            }

            text.Append(name);
            text.Append("(Record ");
            text.Append(capture.RecordId.ToString(CultureInfo.InvariantCulture));
            text.Append(", Buffer ");
            text.Append(capture.BufferId.ToString(CultureInfo.InvariantCulture));
            text.Append(") -> ");
        }

        private async void ButtonRecoverRecorderDoubleJournal_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // This proof gate must remain the first executable guard. A
                // programmatic click therefore cannot issue capability,
                // inventory, adoption, or Release traffic while the route is
                // dormant.
                EnsureRecorderDoubleReconnectRecoveryReady();
                await RunQualificationAsync(
                    "RecorderDoubleReconnectRecovery",
                    RunRecorderDoubleReconnectRecoveryAsync);
            }
            catch (Exception error)
            {
                TextOperationState.Text = "RecorderDoubleRecovery failed";
                WriteLog(error.Message);
            }

            UpdateUiState();
        }
    }
}

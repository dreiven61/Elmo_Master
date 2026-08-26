using System;
using System.Globalization;
using System.Threading.Tasks;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private string axisQualificationRecoveryJournalDirectoryPath;
        private AxisQualificationRecoveryJournal
            axisQualificationRecoveryJournal;
        private string axisQualificationRecoveryJournalOpenError;
        private string axisQualificationRecoveryJournalRuntimeError;
        private Guid? currentAxisQualificationRecoveryIdentity;

        private bool AxisQualificationRecoveryJournalCanArm
        {
            get
            {
                return axisQualificationRecoveryJournal != null
                    && string.IsNullOrEmpty(
                        axisQualificationRecoveryJournalOpenError)
                    && string.IsNullOrEmpty(
                        axisQualificationRecoveryJournalRuntimeError)
                    && !axisQualificationRecoveryJournal.HasActiveRecord;
            }
        }

        private bool AxisQualificationRecoveryJournalUnavailable
        {
            get
            {
                return axisQualificationRecoveryJournal == null
                    || !string.IsNullOrEmpty(
                        axisQualificationRecoveryJournalOpenError)
                    || !string.IsNullOrEmpty(
                        axisQualificationRecoveryJournalRuntimeError);
            }
        }

        private bool HasActiveAxisQualificationRecoveryRecord
        {
            get
            {
                return axisQualificationRecoveryJournal != null
                    && axisQualificationRecoveryJournal.HasActiveRecord;
            }
        }

        private bool HasUnresolvedAxisQualificationState()
        {
            return HasActiveAxisQualificationRecoveryRecord;
        }

        private bool IsCurrentAxisQualificationMutationScope()
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            var currentConnection = connection;
            return qualificationRunning
                && record != null
                && currentAxisQualificationRecoveryIdentity.HasValue
                && record.Identity
                    == currentAxisQualificationRecoveryIdentity.Value
                && !record.WasCrashPromoted
                && currentConnection != null
                && currentConnection.IsConnected
                && record.OwnerSessionGeneration
                    == currentConnection.SessionGeneration
                && record.MatchesEndpoint(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort());
        }

        private void InitializeAxisQualificationRecoveryJournal()
        {
            try
            {
                axisQualificationRecoveryJournal =
                    axisQualificationRecoveryJournalDirectoryPath == null
                        ? AxisQualificationRecoveryJournal.OpenDefault(true)
                        : AxisQualificationRecoveryJournal.Open(
                            axisQualificationRecoveryJournalDirectoryPath,
                            true);
                axisQualificationRecoveryJournalOpenError = null;
                axisQualificationRecoveryJournalRuntimeError = null;
                currentAxisQualificationRecoveryIdentity = null;

                TryFinalizeCommittedAxisQualificationRetirementAtStartup();
                axisQualificationRecoveryJournal
                    .PromoteRecoveredVolatileStage();
                var record = GetActiveAxisQualificationRecoveryRecord();
                if (record != null)
                {
                    ApplyRecoveredAxisQualificationRecord(record);
                }
            }
            catch (Exception error)
            {
                var journal = axisQualificationRecoveryJournal;
                axisQualificationRecoveryJournal = null;
                if (journal != null)
                {
                    journal.Dispose();
                }

                axisQualificationRecoveryJournalOpenError =
                    error.GetType().Name + ": " + error.Message;
                WriteLog(
                    "Single Axis qualification recovery journal is unavailable. New Motion, Power, and mutation commands are fail-closed; explicit Stop and Power Off remain available with their command-level journals: "
                    + axisQualificationRecoveryJournalOpenError);
            }
        }

        private void DisposeAxisQualificationRecoveryJournal()
        {
            var journal = axisQualificationRecoveryJournal;
            axisQualificationRecoveryJournal = null;
            currentAxisQualificationRecoveryIdentity = null;
            if (journal != null)
            {
                journal.Dispose();
            }
        }

        private AxisQualificationRecoveryRecord
            GetActiveAxisQualificationRecoveryRecord()
        {
            var journal = axisQualificationRecoveryJournal;
            if (journal == null || !journal.HasActiveRecord)
            {
                return null;
            }

            var record = journal.CurrentRecord;
            return record != null && record.IsActive ? record : null;
        }

        private void ApplyRecoveredAxisQualificationRecord(
            AxisQualificationRecoveryRecord record)
        {
            if (record == null || !record.IsActive)
            {
                return;
            }

            if (TextRemoteIp != null)
            {
                TextRemoteIp.Text = record.EndpointIp;
            }
            if (TextRemotePort != null)
            {
                TextRemotePort.Text = record.EndpointPort.ToString(
                    CultureInfo.InvariantCulture);
            }
            if (TextAxisName != null)
            {
                TextAxisName.Text = record.AxisName;
            }
            if (TextAxisQualificationProgress != null)
            {
                TextAxisQualificationProgress.Text =
                    "RECOVERY: " + record.Stage + ". "
                    + GetAxisQualificationRecoveryGuidance(record);
            }

            WriteLog(
                "SAFETY: recovered durable Single Axis qualification stage="
                + record.Stage
                + ", Axis="
                + record.AxisName
                + ", Ref="
                + record.AxisReference.ToString(CultureInfo.InvariantCulture)
                + ", Record="
                + record.Identity.ToString("D")
                + (record.WasCrashPromoted
                    ? ", CrashPromoted=true"
                    : string.Empty)
                + ". No Power, Move, Stop, or Power Off command is replayed automatically. "
                + GetAxisQualificationRecoveryGuidance(record));
        }

        private void ArmAxisQualificationRecoveryBeforePowerOn(
            AxisQualificationIdentity identity,
            AxisQualificationInput input)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (!AxisQualificationRecoveryJournalCanArm)
            {
                throw CreateAxisQualificationRecoveryException(
                    "arm-before-PowerOn",
                    null);
            }

            try
            {
                var record = axisQualificationRecoveryJournal.ArmBeforePowerOn(
                    identity.EndpointIp,
                    identity.EndpointPort,
                    identity.SessionGeneration,
                    identity.AxisName,
                    identity.AxisReference,
                    identity.DiagnosticsBuild,
                    identity.DiagnosticsBootId,
                    identity.MapRevision,
                    input.DeltaRaw,
                    input.VelocityRaw,
                    input.AccelerationRaw,
                    input.DecelerationRaw,
                    input.JerkRaw,
                    input.ToleranceRaw,
                    qualificationSafetyGeneration,
                    DateTime.UtcNow);
                axisQualificationRecoveryJournalRuntimeError = null;
                currentAxisQualificationRecoveryIdentity = record.Identity;
                WriteAxisQualificationRecoveryTransition(
                    "armed-before-PowerOn",
                    record);
            }
            catch (Exception error)
            {
                SetAxisQualificationRecoveryRuntimeError(
                    "arm-before-PowerOn",
                    error);
                throw CreateAxisQualificationRecoveryException(
                    "arm-before-PowerOn",
                    error);
            }
        }

        private void CheckpointAxisQualificationPowerOnAccepted(
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            RequireCurrentAxisQualificationRecoveryIdentity(
                currentAxis,
                record,
                operation);
            if (record.Stage
                >= AxisQualificationRecoveryStage.PowerOnAccepted)
            {
                return;
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.MarkPowerOnAccepted(
                    expected,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void CheckpointAxisQualificationPowerOnStableBeforeChildResolve(
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            RequireCurrentAxisQualificationRecoveryIdentity(
                currentAxis,
                record,
                operation);
            if (record.Stage
                < AxisQualificationRecoveryStage.PowerOnAccepted)
            {
                record = PersistAxisQualificationTransition(
                    operation + " accepted repair",
                    (journal, expected) => journal.MarkPowerOnAccepted(
                        expected,
                        MonotonicUtcNow(expected.UpdatedUtc)));
            }
            if (record.Stage
                >= AxisQualificationRecoveryStage.PowerOnStable)
            {
                return;
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.MarkPowerOnStable(
                    expected,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void PrepareAxisQualificationMoveRecovery(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            int startPositionRaw,
            int targetPositionRaw,
            string operation)
        {
            var record = RequireCapturedAxisQualificationRecoveryIdentity(
                identity,
                currentAxis,
                operation);
            if (record.Stage != AxisQualificationRecoveryStage.PowerOnStable)
            {
                throw new InvalidOperationException(
                    operation
                    + " requires the durable PowerOnStable sequence stage.");
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.PrepareMove(
                    expected,
                    startPositionRaw,
                    targetPositionRaw,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void CheckpointAxisQualificationMoveAccepted(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = RequireCapturedAxisQualificationRecoveryIdentity(
                identity,
                currentAxis,
                operation);
            if (record.Stage >= AxisQualificationRecoveryStage.MoveAccepted)
            {
                return;
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.MarkMoveAccepted(
                    expected,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void CheckpointAxisQualificationMoveStable(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = RequireCapturedAxisQualificationRecoveryIdentity(
                identity,
                currentAxis,
                operation);
            if (record.Stage >= AxisQualificationRecoveryStage.MoveStable)
            {
                return;
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.MarkMoveStable(
                    expected,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void CheckpointAxisQualificationStopAccepted(
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            RequireCurrentAxisQualificationRecoveryIdentity(
                currentAxis,
                record,
                operation);
            if (record.Stage >= AxisQualificationRecoveryStage.StopAccepted)
            {
                return;
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.MarkStopAccepted(
                    expected,
                    Math.Max(
                        expected.SafetyGeneration,
                        qualificationSafetyGeneration),
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void CheckpointAxisQualificationStopStableBeforeChildResolve(
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            RequireCurrentAxisQualificationRecoveryIdentity(
                currentAxis,
                record,
                operation);
            if (record.Stage < AxisQualificationRecoveryStage.StopAccepted)
            {
                record = PersistAxisQualificationTransition(
                    operation + " accepted repair",
                    (journal, expected) => journal.MarkStopAccepted(
                        expected,
                        Math.Max(
                            expected.SafetyGeneration,
                            qualificationSafetyGeneration),
                        MonotonicUtcNow(expected.UpdatedUtc)));
            }
            if (record.Stage >= AxisQualificationRecoveryStage.StopStable)
            {
                return;
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.MarkStopStable(
                    expected,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void CheckpointAxisQualificationPowerOffAccepted(
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            RequireCurrentAxisQualificationRecoveryIdentity(
                currentAxis,
                record,
                operation);
            if (record.Stage
                >= AxisQualificationRecoveryStage.PowerOffAccepted)
            {
                return;
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.MarkPowerOffAccepted(
                    expected,
                    Math.Max(
                        expected.SafetyGeneration,
                        qualificationSafetyGeneration),
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void ResolveAxisQualificationAfterStablePowerOffBeforeChildResolve(
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            RequireCurrentAxisQualificationRecoveryIdentity(
                currentAxis,
                record,
                operation);
            if (record.Stage
                < AxisQualificationRecoveryStage.PowerOffAccepted)
            {
                record = PersistAxisQualificationTransition(
                    operation + " accepted repair",
                    (journal, expected) => journal.MarkPowerOffAccepted(
                        expected,
                        Math.Max(
                            expected.SafetyGeneration,
                            qualificationSafetyGeneration),
                        MonotonicUtcNow(expected.UpdatedUtc)));
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.ResolveSafe(
                    expected,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private void ResolveAxisQualificationKnownNoEffectBeforePowerOn(
            AxisQualificationIdentity identity,
            LMCSingleAxis currentAxis,
            string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            record = RequireCapturedAxisQualificationRecoveryIdentity(
                identity,
                currentAxis,
                operation);
            if (record.Stage
                != AxisQualificationRecoveryStage.ArmedBeforePowerOn)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot claim a known no-effect result after the Power On boundary became uncertain.");
            }

            PersistAxisQualificationTransition(
                operation,
                (journal, expected) => journal.ResolveSafe(
                    expected,
                    MonotonicUtcNow(expected.UpdatedUtc)));
        }

        private AxisQualificationRecoveryRecord
            PersistAxisQualificationTransition(
                string operation,
                Func<AxisQualificationRecoveryJournal,
                    AxisQualificationRecoveryRecord,
                    AxisQualificationRecoveryRecord> transition)
        {
            if (transition == null)
            {
                throw new ArgumentNullException("transition");
            }
            var journal = axisQualificationRecoveryJournal;
            var current = GetActiveAxisQualificationRecoveryRecord();
            if (journal == null || current == null)
            {
                throw CreateAxisQualificationRecoveryException(
                    operation,
                    null);
            }

            try
            {
                var next = transition(journal, current);
                axisQualificationRecoveryJournalRuntimeError = null;
                WriteAxisQualificationRecoveryTransition(operation, next);
                UpdateUiState();
                return next;
            }
            catch (Exception error)
            {
                SetAxisQualificationRecoveryRuntimeError(operation, error);
                throw CreateAxisQualificationRecoveryException(
                    operation,
                    error);
            }
        }

        private AxisQualificationRecoveryRecord
            RequireCapturedAxisQualificationRecoveryIdentity(
                AxisQualificationIdentity identity,
                LMCSingleAxis currentAxis,
                string operation)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                throw CreateAxisQualificationRecoveryException(
                    operation,
                    null);
            }
            RequireCurrentAxisQualificationRecoveryIdentity(
                currentAxis,
                record,
                operation);
            if (record.OwnerSessionGeneration != identity.SessionGeneration
                || !record.MatchesRecoveryIdentity(
                    identity.EndpointIp,
                    identity.EndpointPort,
                    identity.SessionGeneration,
                    identity.AxisName,
                    identity.AxisReference,
                    identity.DiagnosticsBuild,
                    identity.DiagnosticsBootId,
                    identity.MapRevision))
            {
                throw new InvalidOperationException(
                    operation
                    + " does not match the captured qualification connection/session identity.");
            }
            return record;
        }

        private void RequireCurrentAxisQualificationRecoveryIdentity(
            LMCSingleAxis currentAxis,
            AxisQualificationRecoveryRecord record,
            string operation)
        {
            var currentConnection = RequireConnection();
            var capabilities = RequireStableAxisQualificationRecoveryIdentity(
                operation);
            if (currentAxis == null
                || !ReferenceEquals(axis, currentAxis)
                || !ReferenceEquals(currentAxis.Connection, currentConnection)
                || !record.MatchesEndpoint(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort())
                || !string.Equals(
                    record.AxisName,
                    currentAxis.AxisName,
                    StringComparison.Ordinal)
                || record.AxisReference != currentAxis.AxisReference
                || record.DiagnosticsBuild != capabilities.DiagnosticsBuild
                || record.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || record.MapRevision != capabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    operation
                    + " cannot change the qualification recovery record because endpoint, Axis, DiagnosticsBuild, BootId, or MapRevision does not match.");
            }
        }

        private void EnsureAxisQualificationRecoveryEndpoint(
            string endpointIp,
            int endpointPort)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record != null
                && !record.MatchesEndpoint(endpointIp, endpointPort))
            {
                throw new InvalidOperationException(
                    "Connect endpoint does not match the durable Single Axis qualification record.");
            }
        }

        private async Task
            EnsureAxisQualificationRecoveryConnectionIdentityAsync(
                string operation)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }

            var currentConnection = RequireConnection();
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            var capabilities = RequireStableAxisQualificationRecoveryIdentity(
                operation);
            if (!record.MatchesEndpoint(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort())
                || record.DiagnosticsBuild != capabilities.DiagnosticsBuild
                || record.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || record.MapRevision != capabilities.MapRevision)
            {
                throw new RecoveryConnectionIdentityMismatchException(
                    operation
                    + " is blocked because the current PLC does not match the durable Single Axis qualification identity. Stored Build=0x"
                    + record.DiagnosticsBuild.ToString("X8")
                    + ", current Build=0x"
                    + capabilities.DiagnosticsBuild.ToString("X8")
                    + ", stored BootId=0x"
                    + record.DiagnosticsBootId.ToString("X8")
                    + ", current BootId=0x"
                    + capabilities.DiagnosticsBootId.ToString("X8")
                    + ", stored MapRevision=0x"
                    + record.MapRevision.ToString("X8")
                    + ", current MapRevision=0x"
                    + capabilities.MapRevision.ToString("X8")
                    + ".");
            }
        }

        private void EnsureAxisQualificationRecoveryLookupAllowed(
            string axisName)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record != null
                && !string.Equals(
                    record.AxisName,
                    axisName,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "A different Axis cannot be loaded during Single Axis qualification recovery. No lookup RPC was sent.");
            }
        }

        private void EnsureLoadedAxisMatchesAxisQualificationRecovery(
            LMCSingleAxis loadedAxis)
        {
            var record = GetActiveAxisQualificationRecoveryRecord();
            if (record == null)
            {
                return;
            }
            var capabilities = RequireStableAxisQualificationRecoveryIdentity(
                "Load Single Axis qualification recovery");
            if (loadedAxis == null
                || !record.MatchesEndpoint(
                    RequiredConnectedRemoteIp(),
                    RequiredConnectedRemotePort())
                || !string.Equals(
                    record.AxisName,
                    loadedAxis.AxisName,
                    StringComparison.Ordinal)
                || record.AxisReference != loadedAxis.AxisReference
                || record.DiagnosticsBuild != capabilities.DiagnosticsBuild
                || record.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || record.MapRevision != capabilities.MapRevision)
            {
                throw new InvalidOperationException(
                    "Loaded Axis does not match the durable Single Axis qualification identity.");
            }
        }

        private LMCDiagnosticCapabilities
            RequireStableAxisQualificationRecoveryIdentity(string operation)
        {
            var currentConnection = RequireConnection();
            var capabilities = diagnosticCapabilities;
            if (capabilities == null
                || capabilities.DiagnosticsBuild == 0
                || capabilities.DiagnosticsBootId == 0
                || capabilities.MapRevision == 0
                || !capabilities.IsBoundTo(
                    currentConnection.Diagnostics,
                    currentConnection.SessionGeneration))
            {
                throw new InvalidOperationException(
                    operation
                    + " requires a current nonzero DiagnosticsBuild, BootId, and MapRevision bound to the live session.");
            }
            return capabilities;
        }

        private string GetAxisQualificationRecoveryGuidance()
        {
            return GetAxisQualificationRecoveryGuidance(
                GetActiveAxisQualificationRecoveryRecord());
        }

        private string GetAxisQualificationRecoveryGuidance(
            AxisQualificationRecoveryRecord record)
        {
            if (!string.IsNullOrEmpty(
                    axisQualificationRecoveryJournalRuntimeError))
            {
                return axisQualificationRecoveryJournalRuntimeError;
            }
            if (!string.IsNullOrEmpty(
                    axisQualificationRecoveryJournalOpenError))
            {
                return axisQualificationRecoveryJournalOpenError;
            }
            if (record == null)
            {
                return AxisQualificationRecoveryJournalUnavailable
                    ? "No Single Axis qualification recovery journal is available."
                    : "No unresolved Single Axis qualification sequence exists.";
            }

            if (record.Stage
                >= AxisQualificationRecoveryStage.PowerOffAccepted)
            {
                return "Reconnect and load the exact Axis, then resume status-only Power Off verification. Never replay Power On or Move.";
            }
            if (record.Stage >= AxisQualificationRecoveryStage.MovePrepared)
            {
                return "Reconnect and load the exact Axis. Use only explicit Stop and Power Off safety controls with stable proof; no command is replayed automatically.";
            }
            return "Reconnect and load the exact Axis, then use explicit Power Off with stable PowerOff plus Standstill proof. Power On and Move replay are blocked.";
        }

        private void SetAxisQualificationRecoveryRuntimeError(
            string phase,
            Exception error)
        {
            axisQualificationRecoveryJournalRuntimeError =
                phase
                + ": "
                + (error == null
                    ? "unknown durable journal failure"
                    : error.GetType().Name + ": " + error.Message);
            WriteLog(
                "Single Axis qualification recovery journal failure; the active sequence remains fail-closed: "
                + axisQualificationRecoveryJournalRuntimeError);
            UpdateUiState();
        }

        private InvalidOperationException
            CreateAxisQualificationRecoveryException(
                string operation,
                Exception innerException)
        {
            var detail = !string.IsNullOrEmpty(
                    axisQualificationRecoveryJournalRuntimeError)
                ? axisQualificationRecoveryJournalRuntimeError
                : !string.IsNullOrEmpty(
                    axisQualificationRecoveryJournalOpenError)
                    ? axisQualificationRecoveryJournalOpenError
                    : "The journal is missing or already has an active record.";
            return new InvalidOperationException(
                operation
                + " is blocked because the Single Axis qualification recovery journal cannot advance durably. "
                + detail,
                innerException);
        }

        private void WriteAxisQualificationRecoveryTransition(
            string operation,
            AxisQualificationRecoveryRecord record)
        {
            WriteLog(
                "Axis qualification recovery checkpoint="
                + record.Stage
                + ", Revision="
                + record.RecordRevision.ToString(CultureInfo.InvariantCulture)
                + ", Record="
                + record.Identity.ToString("D")
                + ", Operation="
                + operation
                + ".");
        }
    }
}

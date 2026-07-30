using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    internal sealed class RecorderDoubleReleaseConfirmedNotAppliedException
        : InvalidOperationException
    {
        internal RecorderDoubleReleaseConfirmedNotAppliedException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }

    internal sealed class RecorderDoublePartialRecoveryAdoption
    {
        private readonly List<LMCRecorderIdentity> adoptedBanks =
            new List<LMCRecorderIdentity>();

        internal RecorderDoublePartialRecoveryAdoption(
            RecorderDoubleRecoveryPlan plan,
            LMCRecorderBankInventory inventory)
        {
            Plan = plan ?? throw new ArgumentNullException("plan");
            Inventory = inventory ?? throw new ArgumentNullException("inventory");
        }

        internal RecorderDoubleRecoveryPlan Plan { get; private set; }
        internal LMCRecorderBankInventory Inventory { get; private set; }
        internal IReadOnlyList<LMCRecorderIdentity> AdoptedBanks
        {
            get { return adoptedBanks; }
        }

        internal void Add(
            RecorderDoubleRecoveryBankTarget target,
            LMCRecorderIdentity handle)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (handle == null)
            {
                throw new ArgumentNullException("handle");
            }

            for (var index = 0; index < adoptedBanks.Count; index++)
            {
                var existing = adoptedBanks[index];
                if (existing.RecordId == handle.RecordId
                    && existing.BufferId == handle.BufferId)
                {
                    return;
                }
            }

            // Retain every distinct accepted identity before target/owner
            // validation. A mismatched RecordId or BufferId is rejected by
            // ValidateRecorderDoubleRawAdoptedIdentity after the callback, but
            // the accepted resource must remain reachable for reconciliation.
            adoptedBanks.Add(handle);
        }

        internal LMCRecorderIdentity Find(
            RecorderDoubleRecoveryBankTarget target)
        {
            for (var index = 0; index < adoptedBanks.Count; index++)
            {
                var handle = adoptedBanks[index];
                if (handle.RecordId == target.RecordId
                    && handle.BufferId == target.BufferId)
                {
                    return handle;
                }
            }

            return null;
        }
    }

    public partial class MainWindow
    {
        private RecorderDoubleBankQualificationResult
            recorderDoubleRetainedQualificationResult;
        private RecorderDoubleBankRecoveryScope
            recorderDoubleRetainedQualificationScope;
        private RecorderDoubleBankQualificationOperations
            recorderDoubleRetainedQualificationOperations;
        private RecorderDoubleDurableReleaseCoordinator
            recorderDoubleRetainedQualificationReleaseCoordinator;
        private LMCConnection recorderDoubleRetainedQualificationConnection;
        private LMCDiagnostics recorderDoubleRetainedQualificationDiagnostics;
        private Exception recorderDoubleRetainedQualificationError;

        private RecorderDoubleRecoveryResult recorderDoubleRetainedRecoveryResult;
        private RecorderDoublePartialRecoveryAdoption
            recorderDoublePartialRecoveryAdoption;
        private LMCConnection recorderDoubleRetainedRecoveryConnection;
        private LMCDiagnostics recorderDoubleRetainedRecoveryDiagnostics;
        private bool HasRecorderDoubleRetainedQualification
        {
            get
            {
                return recorderDoubleRetainedQualificationScope != null
                    && recorderDoubleRetainedQualificationOperations != null
                    && recorderDoubleRetainedQualificationReleaseCoordinator
                        != null;
            }
        }

        private bool HasRecorderDoubleRetainedRecovery
        {
            get
            {
                return recorderDoubleRetainedRecoveryResult != null
                    || recorderDoublePartialRecoveryAdoption != null;
            }
        }

        private async Task
            ConfigureManualRecoverableDoubleRecorderCoreAsync(
                LMCRecorderConfiguration sourceConfiguration)
        {
            if (sourceConfiguration == null)
            {
                throw new ArgumentNullException("sourceConfiguration");
            }

            if (sourceConfiguration.BufferMode
                != LMCRecorderBufferMode.Double)
            {
                throw new ArgumentException(
                    "The recoverable manual adapter accepts only Double Recorder configurations.",
                    "sourceConfiguration");
            }

            if (RecorderDoubleRecoveryJournalUnavailable)
            {
                throw new InvalidOperationException(
                    GetRecorderDoubleRecoveryJournalUnavailableGuidance());
            }

            if (HasActiveRecorderDoubleRecoveryJournalRecord
                || HasRecorderDoubleRetainedQualification
                || HasRecorderDoubleRetainedRecovery)
            {
                throw new InvalidOperationException(
                    "Resolve the existing Double-bank recovery record before another manual Configure.");
            }

            if ((recorderConfiguration != null
                    && !recorderConfiguration.IsReleased)
                || (recorderIdentity != null
                    && !recorderIdentity.IsRecorderReleased))
            {
                throw new InvalidOperationException(
                    "Release the ordinary Recorder resource before manual Double Configure.");
            }

            var currentConnection = RequireConnection();
            var diagnostics = currentConnection.Diagnostics;
            await RefreshDiagnosticsCapabilitiesAsync(currentConnection);
            EnsureRecorderDoubleSession(
                currentConnection,
                diagnostics,
                "manual Configure capability preflight");
            var capturedCapabilities = diagnosticCapabilities;
            if (capturedCapabilities == null
                || capturedCapabilities.MapRevision == 0
                || capturedCapabilities.BaseCycleTimeUs == 0)
            {
                throw new InvalidOperationException(
                    "Manual Double Configure requires current nonzero MapRevision and BaseCycleTimeUs capabilities.");
            }

            var recoveryIdentity = Guid.NewGuid();
            var requestedConfigId =
                CreateRecorderDoubleRequestedConfigId(recoveryIdentity);
            var configuration =
                CloneRecorderDoubleManualConfiguration(
                    sourceConfiguration,
                    requestedConfigId);
            diagnostics.ValidateRecoverableDoubleRecorderConfiguration(
                configuration,
                recoveryIdentity,
                capturedCapabilities);
            var request = new RecorderDoubleBankQualificationRequest(
                capturedCapabilities,
                configuration,
                diagnostics,
                currentConnection);
            var bridge = new RecorderDoubleQualificationJournalBridge(
                recorderDoubleRecoveryJournal,
                recoveryIdentity,
                () => DateTime.UtcNow);
            var coordinator = new RecorderDoubleDurableReleaseCoordinator(
                recorderDoubleRecoveryJournal,
                recoveryIdentity,
                () => DateTime.UtcNow);
            var operations =
                CreateRecorderDoubleManualConfigureOperations(
                    capturedCapabilities,
                    configuration,
                    currentConnection,
                    diagnostics,
                    recoveryIdentity,
                    bridge,
                    coordinator);

            var scope = await RecorderDoubleBankQualificationOrchestrator
                .ConfigureAndRetainAsync(
                    request,
                    operations,
                    CancellationToken.None);
            RetainRecorderDoubleQualification(
                null,
                scope,
                operations,
                coordinator,
                currentConnection,
                diagnostics,
                null);
            ShowRecorderDoubleManualConfigureRetention(scope, null);
        }

        private RecorderDoubleBankQualificationOperations
            CreateRecorderDoubleManualConfigureOperations(
                LMCDiagnosticCapabilities capturedCapabilities,
                LMCRecorderConfiguration configuration,
                LMCConnection expectedConnection,
                LMCDiagnostics diagnostics,
                Guid recoveryIdentity,
                RecorderDoubleQualificationJournalBridge bridge,
                RecorderDoubleDurableReleaseCoordinator coordinator)
        {
            LMCRecorderConfigurationHandle nativeConfiguration = null;
            RecorderDoubleBankRecoveryScope activeScope = null;
            var operations = new RecorderDoubleBankQualificationOperations();

            Action<LMCRecorderConfigurationHandle> retainAccepted = value =>
            {
                if (value == null || activeScope == null)
                {
                    throw new InvalidOperationException(
                        "An active manual Double recovery scope and exact accepted configuration are required.");
                }

                nativeConfiguration = value;
                activeScope.Configuration =
                    new RecorderDoubleBankConfigurationLease(
                        value,
                        value.DiagnosticsBootId,
                        value.ConfigId,
                        value.ConfigRevision,
                        diagnostics,
                        expectedConnection,
                        false);
            };

            operations.ArmRecoveryBeforeConfigureAsync = async scope =>
            {
                activeScope = scope;
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "manual Configure recovery arm");
                try
                {
                    await bridge.ArmRecoveryBeforeConfigureAsync(scope);
                }
                catch (Exception error)
                {
                    if (IsRecorderDoubleJournalRuntimeFailure(error))
                    {
                        RememberRecorderDoubleJournalRuntimeError(error);
                    }

                    throw;
                }
            };
            operations.PersistRecoveryCheckpointAsync = async scope =>
            {
                try
                {
                    await bridge.PersistRecoveryCheckpointAsync(scope);
                }
                catch (Exception error)
                {
                    if (IsRecorderDoubleJournalRuntimeFailure(error))
                    {
                        RememberRecorderDoubleJournalRuntimeError(error);
                    }

                    throw;
                }
            };
            operations.ConfigureAsync = async (requested, token) =>
            {
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "manual ConfigureRecoverableDoubleRecorder");
                if (!ReferenceEquals(requested, configuration)
                    || token != recoveryIdentity
                    || activeScope == null
                    || activeScope.RecoveryToken != recoveryIdentity)
                {
                    throw new InvalidOperationException(
                        "Manual Double Configure received a foreign request or recovery token.");
                }

                try
                {
                    var accepted = await diagnostics
                        .ConfigureRecoverableDoubleRecorderAsync(
                            requested,
                            token,
                            capturedCapabilities,
                            CancellationToken.None);
                    retainAccepted(accepted);
                }
                catch (Exception error)
                {
                    PreserveRecorderAcceptedResult<
                        LMCRecorderConfigurationHandle>(
                            error,
                            retainAccepted);

                    LMCRecorderAcceptedResultFailureContext context;
                    if (activeScope.Configuration != null
                        && LMCRecorderAcceptedResultFailureContext.TryGet(
                            error,
                            out context)
                        && context.Operation
                            == LMCRecorderAcceptedOperation
                                .ConfigureRecoverableDoubleRecorder
                        && context.Command == 0x7E4C
                        && context.ResultKind
                            == LMCRecorderAcceptedResultKind
                                .ConfigurationHandle
                        && ReferenceEquals(
                            context.ConfigurationHandle,
                            nativeConfiguration))
                    {
                        try
                        {
                            ValidateRecorderDoubleManualConfigurationHandle(
                                nativeConfiguration,
                                activeScope.Configuration,
                                capturedCapabilities,
                                configuration,
                                recoveryIdentity,
                                diagnostics,
                                expectedConnection);
                            activeScope.Stage = "PERSIST_CONFIGURATION";
                            await operations
                                .PersistRecoveryCheckpointAsync(activeScope);
                            activeScope.Stage = "CONFIGURATION_RETAINED";
                        }
                        catch (Exception retentionError)
                        {
                            // The original send-preemption error remains the
                            // primary result. The exact lease stays reachable
                            // even when its durable checkpoint cannot advance.
                            error.Data[
                                "RecorderDoubleAcceptedRetentionError"] =
                                retentionError.ToString();
                        }
                    }

                    throw;
                }

                ValidateRecorderDoubleManualConfigurationHandle(
                    nativeConfiguration,
                    activeScope.Configuration,
                    capturedCapabilities,
                    configuration,
                    recoveryIdentity,
                    diagnostics,
                    expectedConnection);
                return activeScope.Configuration;
            };
            operations.StartAsync = value =>
                Task.FromException<RecorderDoubleBankCaptureLease>(
                    CreateRecorderDoubleManualConfigureStageError("Start"));
            operations.WaitForFrozenAsync = value =>
                Task.FromException<RecorderDoubleBankFrozenStatus>(
                    CreateRecorderDoubleManualConfigureStageError(
                        "WaitForFrozen"));
            operations.DownloadAsync = value =>
                Task.FromException<RecorderDoubleBankCaptureEvidence>(
                    CreateRecorderDoubleManualConfigureStageError(
                        "Download"));
            operations.IsExactResourceBusy = error => false;
            operations.IsReleaseConfirmedNotApplied = error =>
                error is RecorderDoubleReleaseConfirmedNotAppliedException;
            operations.RecoveryRequired = (scope, error) =>
            {
                RetainRecorderDoubleQualification(
                    null,
                    scope,
                    operations,
                    coordinator,
                    expectedConnection,
                    diagnostics,
                    error);
                ShowRecorderDoubleManualConfigureRetention(scope, error);
            };
            operations.ReleaseBankAsync = value =>
                Task.FromException(
                    CreateRecorderDoubleManualConfigureStageError(
                        "ReleaseBank"));
            operations.ReleaseConfigurationAsync = value =>
                ReleaseRecorderDoubleQualificationConfigurationNativeAsync(
                    value,
                    nativeConfiguration,
                    expectedConnection,
                    diagnostics);
            return operations;
        }

        private static LMCRecorderConfiguration
            CloneRecorderDoubleManualConfiguration(
                LMCRecorderConfiguration source,
                uint requestedConfigId)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.BufferMode != LMCRecorderBufferMode.Double
                || requestedConfigId == 0)
            {
                throw new ArgumentException(
                    "Manual Double configuration cloning requires Double mode and a nonzero RequestedConfigId.",
                    "source");
            }

            return new LMCRecorderConfiguration(
                source.SignalIds,
                source.SamplePeriodCycles,
                source.SampleCapacity,
                source.BufferMode,
                source.TriggerType,
                source.TriggerValueType,
                source.PreTriggerSamples,
                source.PostTriggerSamples,
                source.TriggerSignalId,
                source.TriggerOperator,
                source.TriggerValue,
                source.TriggerMask,
                requestedConfigId);
        }

        private static void
            ValidateRecorderDoubleManualConfigurationHandle(
                LMCRecorderConfigurationHandle handle,
                RecorderDoubleBankConfigurationLease lease,
                LMCDiagnosticCapabilities capabilities,
                LMCRecorderConfiguration configuration,
                Guid recoveryIdentity,
                LMCDiagnostics ownerToken,
                LMCConnection sessionToken)
        {
            if (handle == null
                || lease == null
                || capabilities == null
                || configuration == null)
            {
                throw new InvalidOperationException(
                    "Manual Double Configure did not retain a complete exact result.");
            }

            var expectedStride = checked(
                (ushort)(configuration.ChannelCount * 4));
            var expectedReservedBytes = checked(
                (uint)((ulong)handle.AcceptedCapacity
                    * expectedStride
                    * 2u));
            var expectedSamplePeriodUs = checked(
                (uint)configuration.SamplePeriodCycles
                    * capabilities.BaseCycleTimeUs);
            if (!ReferenceEquals(handle.Configuration, configuration)
                || handle.DiagnosticsBootId
                    != capabilities.DiagnosticsBootId
                || handle.MapRevision != capabilities.MapRevision
                || handle.ConfigId == 0
                || handle.ConfigId != configuration.RequestedConfigId
                || handle.ConfigRevision == 0
                || handle.InitialState != LMCRecorderState.Configured
                || !handle.IsRecoverable
                || handle.RecoveryToken != recoveryIdentity
                || handle.RecorderBufferCount != 2
                || handle.ChannelCount != configuration.ChannelCount
                || handle.SampleStrideBytes != expectedStride
                || handle.AcceptedCapacity == 0
                || handle.AcceptedCapacity > configuration.SampleCapacity
                || handle.ReservedDataBytes != expectedReservedBytes
                || handle.SamplePeriodUs != expectedSamplePeriodUs
                || handle.OwnerSessionEpoch == 0
                || !ReferenceEquals(lease.NativeHandle, handle)
                || lease.DiagnosticsBootId != handle.DiagnosticsBootId
                || lease.ConfigId != handle.ConfigId
                || lease.ConfigRevision != handle.ConfigRevision
                || lease.UsedZeroIdDiscovery
                || !ReferenceEquals(lease.OwnerToken, ownerToken)
                || !ReferenceEquals(lease.SessionToken, sessionToken))
            {
                throw new InvalidOperationException(
                    "Manual Double Configure returned invalid identity, capacity, token, or provenance metadata.");
            }

            for (var index = 0; index < configuration.SignalIds.Count; index++)
            {
                if (handle.SignalIds[index]
                    != configuration.SignalIds[index])
                {
                    throw new InvalidOperationException(
                        "Manual Double Configure returned a mismatched signal order.");
                }
            }
        }

        private static InvalidOperationException
            CreateRecorderDoubleManualConfigureStageError(string operation)
        {
            return new InvalidOperationException(
                "Manual Double config-only adapter must not execute "
                + operation
                + ".");
        }

        private void ShowRecorderDoubleManualConfigureRetention(
            RecorderDoubleBankRecoveryScope scope,
            Exception error)
        {
            if (scope == null)
            {
                return;
            }

            var configuration = scope.Configuration;
            TextRecorderSummary.Text =
                "Double Recorder manual Configure retained for explicit cleanup."
                + Environment.NewLine
                + "Stage="
                + scope.Stage
                + ", Recovery="
                + scope.RecoveryToken.ToString("D")
                + ", Config="
                + (configuration == null
                    ? "unknown (disconnect/reconnect recovery required)"
                    : "0x"
                        + configuration.ConfigId.ToString("X8")
                        + "/"
                        + configuration.ConfigRevision)
                + (error == null
                    ? string.Empty
                    : Environment.NewLine
                        + "Configure result="
                        + error.GetType().Name
                        + ": "
                        + error.Message);
        }

        private async Task RunRecorderDoubleQualificationAsync(
            CancellationToken cancellationToken)
        {
            EnsureRecorderDoubleQualificationProofGatesReady();
            cancellationToken.ThrowIfCancellationRequested();
            if (RecorderDoubleRecoveryJournalUnavailable)
            {
                throw new InvalidOperationException(
                    GetRecorderDoubleRecoveryJournalUnavailableGuidance());
            }

            if (HasActiveRecorderDoubleRecoveryJournalRecord
                || HasRecorderDoubleRetainedQualification
                || HasRecorderDoubleRetainedRecovery)
            {
                throw new InvalidOperationException(
                    "Resolve the existing Double-bank recovery record before starting another qualification.");
            }

            const uint sampleCapacity = 1000;
            var context = await PrepareRecorderQualificationAsync(
                sampleCapacity,
                false,
                true,
                cancellationToken);
            var currentConnection = RequireConnection();
            var diagnostics = currentConnection.Diagnostics;
            var recoveryIdentity = Guid.NewGuid();
            var requestedConfigId =
                CreateRecorderDoubleRequestedConfigId(recoveryIdentity);
            var configuration = new LMCRecorderConfiguration(
                context.SignalIds,
                1,
                sampleCapacity,
                LMCRecorderBufferMode.Double,
                LMCRecorderTriggerType.Manual,
                LMCSignalValueType.Invalid,
                0,
                0,
                0,
                LMCRecorderTriggerOperator.None,
                0,
                0,
                requestedConfigId);
            var request = new RecorderDoubleBankQualificationRequest(
                context.Capabilities,
                configuration,
                diagnostics,
                currentConnection);
            var bridge = new RecorderDoubleQualificationJournalBridge(
                recorderDoubleRecoveryJournal,
                recoveryIdentity,
                () => DateTime.UtcNow);
            var coordinator = new RecorderDoubleDurableReleaseCoordinator(
                recorderDoubleRecoveryJournal,
                recoveryIdentity,
                () => DateTime.UtcNow);
            var operations = CreateRecorderDoubleQualificationOperations(
                context,
                currentConnection,
                diagnostics,
                recoveryIdentity,
                bridge,
                coordinator,
                cancellationToken);

            SetQualificationProgress(
                8,
                "Arming durable Double-bank recovery before Configure");
            var result = await RecorderDoubleBankQualificationOrchestrator
                .RunAsync(request, operations, cancellationToken);
            RetainRecorderDoubleQualification(
                result,
                result.RecoveryScope,
                operations,
                coordinator,
                currentConnection,
                diagnostics,
                null);
            SetQualificationProgress(
                92,
                "Double-bank proof complete; both banks remain retained for explicit cleanup");
            WriteQualificationLog(
                "event=RECORDER_DOUBLE_RETAINED",
                "identity=" + recoveryIdentity.ToString("D"),
                "configId=0x" + requestedConfigId.ToString("X8"),
                "bankARecordId=" + result.RecoveryScope.BankA.RecordId,
                "bankABufferId=" + result.RecoveryScope.BankA.BufferId,
                "bankBRecordId=" + result.RecoveryScope.BankB.RecordId,
                "bankBBufferId=" + result.RecoveryScope.BankB.BufferId,
                "bankAHeaderSha256=" + result.BankAInitial.HeaderSha256,
                "bankADataSha256=" + result.BankAInitial.DataSha256,
                "bankBHeaderSha256=" + result.BankB.HeaderSha256,
                "bankBDataSha256=" + result.BankB.DataSha256,
                "bankARereadStable=PASS",
                "thirdStart=EXACT_RESOURCE_BUSY",
                "automaticRelease=false",
                "verdict=PASS_RETAINED");
        }

        private RecorderDoubleBankQualificationOperations
            CreateRecorderDoubleQualificationOperations(
                RecorderQualificationContext context,
                LMCConnection expectedConnection,
                LMCDiagnostics diagnostics,
                Guid recoveryIdentity,
                RecorderDoubleQualificationJournalBridge bridge,
                RecorderDoubleDurableReleaseCoordinator coordinator,
                CancellationToken qualificationCancellationToken)
        {
            LMCRecorderConfigurationHandle nativeConfiguration = null;
            RecorderDoubleBankRecoveryScope activeScope = null;
            var operations = new RecorderDoubleBankQualificationOperations();
            operations.ArmRecoveryBeforeConfigureAsync = async scope =>
            {
                activeScope = scope;
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "arm recovery");
                try
                {
                    await bridge.ArmRecoveryBeforeConfigureAsync(scope);
                }
                catch (Exception error)
                {
                    if (IsRecorderDoubleJournalRuntimeFailure(error))
                    {
                        RememberRecorderDoubleJournalRuntimeError(error);
                    }

                    throw;
                }
            };
            operations.PersistRecoveryCheckpointAsync = async scope =>
            {
                try
                {
                    await bridge.PersistRecoveryCheckpointAsync(scope);
                }
                catch (Exception error)
                {
                    if (IsRecorderDoubleJournalRuntimeFailure(error))
                    {
                        RememberRecorderDoubleJournalRuntimeError(error);
                    }

                    throw;
                }
            };
            operations.ConfigureAsync = async (configuration, token) =>
            {
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "ConfigureRecoverableDoubleRecorder");
                nativeConfiguration = await SendQualificationCommandAsync(
                    "Recorder Double recoverable Configure",
                    qualificationCancellationToken,
                    () => diagnostics.ConfigureRecoverableDoubleRecorderAsync(
                        configuration,
                        token,
                        CancellationToken.None),
                    value =>
                    {
                        nativeConfiguration = value;
                        activeScope.Configuration =
                            new RecorderDoubleBankConfigurationLease(
                                value,
                                value.DiagnosticsBootId,
                                value.ConfigId,
                                value.ConfigRevision,
                                diagnostics,
                                expectedConnection,
                                false);
                    });
                AssertRecorderConfigurationHandle(
                    nativeConfiguration,
                    context,
                    configuration.SampleCapacity,
                    LMCRecorderBufferMode.Double,
                    LMCRecorderTriggerType.Manual);
                if (!nativeConfiguration.IsRecoverable
                    || nativeConfiguration.RecoveryToken != recoveryIdentity
                    || nativeConfiguration.ConfigId
                        != configuration.RequestedConfigId
                    || nativeConfiguration.RecorderBufferCount != 2)
                {
                    throw new InvalidOperationException(
                        "Recoverable Double Configure returned an invalid token, ConfigId, or two-bank contract.");
                }

                return activeScope.Configuration;
            };
            operations.StartAsync = async configuration =>
            {
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "StartRecorder");
                var nativeHandle = RequireRecorderDoubleNativeConfiguration(
                    configuration);
                if (!ReferenceEquals(nativeHandle, nativeConfiguration))
                {
                    throw new InvalidOperationException(
                        "Double Start received a configuration from another qualification scope.");
                }

                RecorderDoubleBankCaptureLease acceptedLease = null;
                var acceptedStartStage = activeScope.Stage;
                var identity = await SendQualificationCommandAsync(
                    "Recorder Double Start",
                    qualificationCancellationToken,
                    () => diagnostics.StartRecorderAsync(
                        nativeHandle,
                        CancellationToken.None),
                    value =>
                    {
                        acceptedLease = new RecorderDoubleBankCaptureLease(
                            value,
                            value.DiagnosticsBootId,
                            value.ConfigId,
                            value.ConfigRevision,
                            value.RecordId,
                            value.BufferId,
                            diagnostics,
                            expectedConnection,
                            false);
                        if (acceptedStartStage == "START_A")
                        {
                            activeScope.BankA = acceptedLease;
                        }
                        else if (acceptedStartStage == "START_B")
                        {
                            activeScope.BankB = acceptedLease;
                        }
                        else if (acceptedStartStage == "THIRD_START_BUSY")
                        {
                            activeScope.UnexpectedThird = acceptedLease;
                        }
                        else
                        {
                            // Preserve first; the orchestrator will reject the
                            // unexpected stage after the SDK result returns.
                            activeScope.UnexpectedThird = acceptedLease;
                        }
                    });
                AssertRecorderIdentity(identity, nativeHandle, context);
                if (acceptedStartStage != "START_A"
                    && acceptedStartStage != "START_B"
                    && acceptedStartStage != "THIRD_START_BUSY")
                {
                    throw new InvalidOperationException(
                        "Recorder Double accepted Start returned in an unexpected recovery stage. The exact identity was retained as UnexpectedThird for reconciliation.");
                }

                return acceptedLease;
            };
            operations.WaitForFrozenAsync = async capture =>
            {
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "wait for frozen bank");
                var identity = RequireRecorderDoubleNativeIdentity(capture);
                var status = await WaitForRecorderStateAsync(
                    diagnostics,
                    identity,
                    nativeConfiguration,
                    value => value.State == LMCRecorderState.Ready,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    qualificationCancellationToken,
                    "DOUBLE_READY_B" + capture.BufferId);
                AssertRecorderTerminalStatus(
                    status,
                    nativeConfiguration,
                    nativeConfiguration.AcceptedCapacity,
                    LMCRecorderStopReason.SampleCountComplete,
                    false,
                    uint.MaxValue);
                return new RecorderDoubleBankFrozenStatus(
                    capture,
                    status.IsFrozen);
            };
            operations.DownloadAsync = async capture =>
            {
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "download frozen bank");
                var identity = RequireRecorderDoubleNativeIdentity(capture);
                var download = await DownloadRecorderQualificationAsync(
                    diagnostics,
                    identity,
                    context.Capabilities.MaxChunkDataBytes,
                    qualificationCancellationToken,
                    "DOUBLE_B" + capture.BufferId);
                AssertRecorderData(
                    download,
                    identity,
                    nativeConfiguration,
                    context,
                    nativeConfiguration.AcceptedCapacity,
                    LMCRecorderStopReason.SampleCountComplete,
                    false,
                    uint.MaxValue);
                return new RecorderDoubleBankCaptureEvidence(
                    capture,
                    download.Header,
                    download.Data);
            };
            operations.IsExactResourceBusy = IsExactRecorderDoubleResourceBusy;
            operations.IsReleaseConfirmedNotApplied = error =>
                error is RecorderDoubleReleaseConfirmedNotAppliedException;
            operations.RecoveryRequired = (scope, error) =>
                RetainRecorderDoubleQualification(
                    null,
                    scope,
                    operations,
                    coordinator,
                    expectedConnection,
                    diagnostics,
                    error);
            operations.ReleaseBankAsync = capture =>
                ReleaseRecorderDoubleQualificationBankNativeAsync(
                    capture,
                    nativeConfiguration,
                    expectedConnection,
                    diagnostics);
            operations.ReleaseConfigurationAsync = configuration =>
                ReleaseRecorderDoubleQualificationConfigurationNativeAsync(
                    configuration,
                    nativeConfiguration,
                    expectedConnection,
                    diagnostics);
            return operations;
        }

        private async Task ReleaseRecorderDoubleQualificationBankNativeAsync(
            RecorderDoubleBankCaptureLease capture,
            LMCRecorderConfigurationHandle configuration,
            LMCConnection expectedConnection,
            LMCDiagnostics diagnostics)
        {
            var identity = RequireRecorderDoubleNativeIdentity(capture);
            try
            {
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "release retained Double bank");
                var cleanup = new RecorderQualificationCleanupOperations
                {
                    ReadStatusAsync = () =>
                        SendQualificationCleanupCommandAsync(
                            "Recorder Double retained Status",
                            () => diagnostics.GetRecorderStatusAsync(
                                identity,
                                CancellationToken.None)),
                    StopAsync = () => SendQualificationCleanupCommandAsync(
                        "Recorder Double retained Stop",
                        () => diagnostics.StopRecorderAsync(
                            identity,
                            CancellationToken.None)),
                    ValidateStatus = status => AssertRecorderStatusIdentity(
                        status,
                        identity,
                        configuration),
                    DelayAsync = milliseconds => Task.Delay(milliseconds),
                    StopRaceResolved = status => { },
                    RecoveryRequired = status => { }
                };
                await RecorderQualificationCleanupOrchestrator
                    .EnsureReleasableStateAsync(
                        cleanup,
                        RecorderQualificationRpcTimeoutMilliseconds,
                        RecorderQualificationPollMilliseconds);
                await SendQualificationCleanupCommandAsync(
                    "Recorder Double retained bank Release",
                    () => diagnostics.ReleaseRecorderBufferAsync(
                        identity,
                        CancellationToken.None));
            }
            catch (Exception error)
            {
                if (!identity.IsBufferReleased
                    && !identity.IsBufferReleaseOutcomeUnverified)
                {
                    throw new
                        RecorderDoubleReleaseConfirmedNotAppliedException(
                            "The native Double-bank buffer Release was confirmed not applied.",
                            error);
                }

                throw;
            }
        }

        private async Task
            ReleaseRecorderDoubleQualificationConfigurationNativeAsync(
                RecorderDoubleBankConfigurationLease configuration,
                LMCRecorderConfigurationHandle expectedNativeConfiguration,
                LMCConnection expectedConnection,
                LMCDiagnostics diagnostics)
        {
            var nativeConfiguration =
                RequireRecorderDoubleNativeConfiguration(configuration);
            try
            {
                EnsureRecorderDoubleSession(
                    expectedConnection,
                    diagnostics,
                    "release retained Double configuration");
                if (!ReferenceEquals(
                        nativeConfiguration,
                        expectedNativeConfiguration))
                {
                    throw new InvalidOperationException(
                        "Double configuration Release received a foreign native handle.");
                }

                await SendQualificationCleanupCommandAsync(
                    "Recorder Double retained configuration Release",
                    () => diagnostics.ReleaseRecorderAsync(
                        nativeConfiguration,
                        CancellationToken.None));
            }
            catch (Exception error)
            {
                if (!nativeConfiguration.IsReleased
                    && !nativeConfiguration.IsReleaseOutcomeUnverified)
                {
                    throw new
                        RecorderDoubleReleaseConfirmedNotAppliedException(
                            "The native Double-bank configuration Release was confirmed not applied.",
                            error);
                }

                throw;
            }
        }

        private void RetainRecorderDoubleQualification(
            RecorderDoubleBankQualificationResult result,
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankQualificationOperations operations,
            RecorderDoubleDurableReleaseCoordinator coordinator,
            LMCConnection ownerConnection,
            LMCDiagnostics diagnostics,
            Exception error)
        {
            if (scope == null || operations == null || coordinator == null)
            {
                throw new InvalidOperationException(
                    "A complete Double-bank retained qualification scope is required.");
            }

            recorderDoubleRetainedQualificationResult = result;
            recorderDoubleRetainedQualificationScope = scope;
            recorderDoubleRetainedQualificationOperations = operations;
            recorderDoubleRetainedQualificationReleaseCoordinator =
                coordinator;
            recorderDoubleRetainedQualificationConnection = ownerConnection;
            recorderDoubleRetainedQualificationDiagnostics = diagnostics;
            recorderDoubleRetainedQualificationError = error;
        }

        private async void ButtonReleaseRecorderDoubleRetained_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                EnsureRecorderDoubleSameSessionCleanupRouteReady(
                    recorderDoubleRetainedQualificationScope);
                await RunQualificationAsync(
                    "RecorderDoubleSameSessionCleanup",
                    RunRecorderDoubleSameSessionCleanupAsync);
            }
            catch (Exception error)
            {
                TextOperationState.Text =
                    "RecorderDoubleSameSessionCleanup failed";
                WriteLog(error.Message);
                UpdateUiState();
            }
        }

        private async Task RunRecorderDoubleSameSessionCleanupAsync(
            CancellationToken cancellationToken)
        {
            EnsureRecorderDoubleSameSessionCleanupRouteReady(
                recorderDoubleRetainedQualificationScope);
            EnsureRecorderDoubleReleaseConfirmed();
            EnsureRecorderDoubleLifecycleAdmission(
                true,
                "same-session retained cleanup");
            var scope = recorderDoubleRetainedQualificationScope;
            var operations = recorderDoubleRetainedQualificationOperations;
            var coordinator =
                recorderDoubleRetainedQualificationReleaseCoordinator;
            EnsureRecorderDoubleSession(
                recorderDoubleRetainedQualificationConnection,
                recorderDoubleRetainedQualificationDiagnostics,
                "same-session retained cleanup");
            if (scope == null
                || operations == null
                || coordinator == null
                || scope.RecoveryToken == Guid.Empty
                || recorderDoubleRecoveryJournal.CurrentRecord == null
                || recorderDoubleRecoveryJournal.CurrentRecord.Identity
                    != scope.RecoveryToken)
            {
                throw new InvalidOperationException(
                    "The retained Double-bank scope does not match the active durable record.");
            }

            EnsureRecorderDoubleSameSessionThirdStartIsReleasable(scope);
            var configurationOnly = scope.ConfigurationOnlyRetention;
            ConsumeRecorderDoubleReleaseConfirmation();

            CommitRecorderDoubleRecoveryMutation();
            await ReleaseRetainedRecorderDoubleCaptureAsync(
                coordinator,
                scope,
                scope.BankB,
                operations,
                "Bank B");
            await ReleaseRetainedRecorderDoubleCaptureAsync(
                coordinator,
                scope,
                scope.BankA,
                operations,
                "Bank A");
            await coordinator.ReleaseQualificationConfigurationAndResolveAsync(
                scope,
                operations,
                true,
                CancellationToken.None);
            ClearRecorderDoubleRetainedQualification();
            recorderDoubleRecoveryRecoveredAtStartup = false;
            CheckConfirmRecorderDoubleRelease.IsChecked = false;
            WriteQualificationLog(
                "event=RECORDER_DOUBLE_CLEANUP",
                "route=SAME_SESSION_RETAINED",
                configurationOnly
                    ? "releaseOrder=ConfigurationOnly"
                    : "releaseOrder=B,A,Configuration",
                "journalResolved=true",
                "verdict=PASS");
        }

        private async Task ReleaseRetainedRecorderDoubleCaptureAsync(
            RecorderDoubleDurableReleaseCoordinator coordinator,
            RecorderDoubleBankRecoveryScope scope,
            RecorderDoubleBankCaptureLease capture,
            RecorderDoubleBankQualificationOperations operations,
            string name)
        {
            if (capture == null)
            {
                return;
            }

            await coordinator.ReleaseQualificationBankAsync(
                scope,
                capture,
                operations,
                true,
                CancellationToken.None);
            WriteQualificationLog(
                "event=RECORDER_DOUBLE_BANK_RELEASED",
                "route=SAME_SESSION_RETAINED",
                "bank=" + QualificationValue(name),
                "recordId=" + capture.RecordId,
                "bufferId=" + capture.BufferId,
                "verdict=PASS");
        }

        private async Task RunRecorderDoubleReconnectRecoveryAsync(
            CancellationToken cancellationToken)
        {
            EnsureRecorderDoubleReconnectRecoveryReady();
            EnsureRecorderDoubleReleaseConfirmed();
            EnsureRecorderDoubleLifecycleAdmission(
                false,
                "reconnect/startup recovery");
            var currentConnection = RequireConnection();
            var diagnostics = currentConnection.Diagnostics;
            EnsureRecorderDoubleRecoveryCapabilityIdentity();
            var mutationConfirmation =
                RecorderDoubleRecoveryMutationConfirmation.Capture(
                    recorderDoubleRecoveryJournal.CurrentRecord);
            ConsumeRecorderDoubleReleaseConfirmation();

            RecorderDoubleRecoveryResult result;
            if (recorderDoubleRetainedRecoveryResult != null)
            {
                EnsureRecorderDoubleSession(
                    recorderDoubleRetainedRecoveryConnection,
                    recorderDoubleRetainedRecoveryDiagnostics,
                    "continue retained recovery cleanup");
                result = recorderDoubleRetainedRecoveryResult;
            }
            else if (recorderDoublePartialRecoveryAdoption != null)
            {
                EnsureRecorderDoubleSession(
                    recorderDoubleRetainedRecoveryConnection,
                    recorderDoubleRetainedRecoveryDiagnostics,
                    "continue partial Double adoption");
                RecorderDoubleRecoveryConfirmationPolicy
                    .EnsurePlanConfirmedBeforeMutation(
                        mutationConfirmation,
                        recorderDoublePartialRecoveryAdoption.Plan);
                CommitRecorderDoubleRecoveryMutation();
                result = await ContinueRecorderDoublePartialAdoptionAsync(
                    recorderDoublePartialRecoveryAdoption,
                    diagnostics);
                RetainRecorderDoubleRecoveryResult(
                    result,
                    currentConnection,
                    diagnostics);
            }
            else
            {
                var attempt = new RecorderDoubleRecoveryAttemptState();
                var operations = CreateRecorderDoubleRecoveryOperations(
                    currentConnection,
                    diagnostics,
                    attempt,
                    mutationConfirmation);
                try
                {
                    result = await RecorderDoubleRecoveryOrchestrator.RunAsync(
                        recorderDoubleRecoveryJournal,
                        operations,
                        cancellationToken);
                }
                catch
                {
                    if (attempt.EmptyConfigurationLease != null)
                    {
                        RetainRecorderDoubleRecoveryResult(
                            new RecorderDoubleRecoveryResult(
                                attempt.Plan,
                                attempt.Inventory,
                                attempt.EmptyConfigurationLease,
                                new LMCRecorderIdentity[0]),
                            currentConnection,
                            diagnostics);
                    }
                    else if (attempt.PartialAdoption != null)
                    {
                        recorderDoublePartialRecoveryAdoption =
                            attempt.PartialAdoption;
                        recorderDoubleRetainedRecoveryConnection =
                            currentConnection;
                        recorderDoubleRetainedRecoveryDiagnostics = diagnostics;
                    }

                    throw;
                }

                if (result.IsResolvedByConfigurationAbsence)
                {
                    ClearRecorderDoubleRetainedRecovery();
                    recorderDoubleRecoveryRecoveredAtStartup = false;
                    CheckConfirmRecorderDoubleRelease.IsChecked = false;
                    WriteQualificationLog(
                        "event=RECORDER_DOUBLE_RECOVERY",
                        "route=TYPED_CONFIGURATION_ABSENCE",
                        "releaseWire=0",
                        "journalResolved=true",
                        "verdict=PASS");
                    return;
                }

                RetainRecorderDoubleRecoveryResult(
                    result,
                    currentConnection,
                    diagnostics);
            }

            RecorderDoubleRecoveryConfirmationPolicy
                .EnsurePlanConfirmedBeforeMutation(
                    mutationConfirmation,
                    result.Plan);
            CommitRecorderDoubleRecoveryMutation();
            await ReleaseRecoveredRecorderDoubleAsync(result, diagnostics);
            ClearRecorderDoubleRetainedRecovery();
            recorderDoubleRecoveryRecoveredAtStartup = false;
            CheckConfirmRecorderDoubleRelease.IsChecked = false;
            WriteQualificationLog(
                "event=RECORDER_DOUBLE_RECOVERY",
                "route=" + result.Plan.Route,
                "journalResolved=true",
                "automaticReplay=false",
                "verdict=PASS");
        }

        private RecorderDoubleRecoveryOperations
            CreateRecorderDoubleRecoveryOperations(
                LMCConnection expectedConnection,
                LMCDiagnostics diagnostics,
                RecorderDoubleRecoveryAttemptState attempt,
                RecorderDoubleRecoveryMutationConfirmation
                    mutationConfirmation)
        {
            return new RecorderDoubleRecoveryOperations
            {
                ReadRecoverableInventoryAsync = (record, token) =>
                {
                    EnsureRecorderDoubleSession(
                        expectedConnection,
                        diagnostics,
                        "read recoverable Double inventory");
                    return SendQualificationCommandAsync(
                        "Recorder Double recoverable inventory",
                        token,
                        () => diagnostics
                            .ReadRecoverableRecorderBankInventoryAsync(
                                record.DiagnosticsBootId,
                                record.RequestedConfigId,
                                record.MapRevision,
                                record.RecoveryToken,
                                CancellationToken.None));
                },
                ReadInventoryAsync = async (record, token) =>
                {
                    EnsureRecorderDoubleSession(
                        expectedConnection,
                        diagnostics,
                        "read standard Double inventory");
                    var inventory = await SendQualificationCommandAsync(
                        "Recorder Double standard inventory",
                        token,
                        () => diagnostics.ReadRecorderBankInventoryAsync(
                            record.DiagnosticsBootId,
                            record.RequestedConfigId,
                            record.MapRevision,
                            record.ConfigRevision,
                            CancellationToken.None));
                    attempt.Inventory = inventory;
                    return inventory;
                },
                AdoptBankAsync = async (plan, target) =>
                {
                    CommitRecorderDoubleRecoveryMutation();
                    if (attempt.Inventory == null)
                    {
                        throw new InvalidOperationException(
                            "Double recovery cannot Adopt before exact standard inventory.");
                    }

                    if (attempt.PartialAdoption == null)
                    {
                        attempt.PartialAdoption =
                            new RecorderDoublePartialRecoveryAdoption(
                                plan,
                                attempt.Inventory);
                    }

                    var handle = attempt.PartialAdoption.AdoptedBanks.Count == 0
                        ? await SendQualificationCommandAsync(
                            "Recorder Double exact bank Adopt",
                            CancellationToken.None,
                            () => diagnostics.AdoptRecorderAsync(
                                plan.DiagnosticsBootId,
                                target.RecordId,
                                target.BufferId,
                                CancellationToken.None),
                            value => attempt.PartialAdoption.Add(
                                target,
                                value))
                        : await SendQualificationCleanupCommandAsync(
                            "Recorder Double finish exact bank Adopt",
                            () => diagnostics.AdoptRecorderAsync(
                                plan.DiagnosticsBootId,
                                target.RecordId,
                                target.BufferId,
                                CancellationToken.None),
                            value => attempt.PartialAdoption.Add(
                                target,
                                value));
                    ValidateRecorderDoubleRawAdoptedIdentity(
                        plan,
                        target,
                        handle,
                        attempt.PartialAdoption.AdoptedBanks);
                    attempt.PartialAdoption.Add(target, handle);
                    await HydrateAndValidateRecorderDoubleAdoptedIdentityAsync(
                        plan,
                        target,
                        handle,
                        attempt.PartialAdoption.AdoptedBanks,
                        diagnostics);
                    return handle;
                },
                AdoptEmptyConfigurationAsync = async (plan, inventory) =>
                {
                    CommitRecorderDoubleRecoveryMutation();
                    return await SendQualificationCommandAsync(
                        "Recorder Double empty configuration Adopt",
                        CancellationToken.None,
                        () => diagnostics
                            .AdoptEmptyRecorderConfigurationAsync(
                                inventory,
                                CancellationToken.None),
                        value =>
                        {
                            attempt.Plan = plan;
                            attempt.Inventory = inventory;
                            attempt.EmptyConfigurationLease = value;
                        });
                },
                EnsureMutationPlanConfirmed = plan =>
                    RecorderDoubleRecoveryConfirmationPolicy
                        .EnsurePlanConfirmedBeforeMutation(
                            mutationConfirmation,
                            plan),
                UtcNow = () => DateTime.UtcNow
            };
        }

        private async Task<RecorderDoubleRecoveryResult>
            ContinueRecorderDoublePartialAdoptionAsync(
                RecorderDoublePartialRecoveryAdoption partial,
                LMCDiagnostics diagnostics)
        {
            var ordered = new List<LMCRecorderIdentity>(
                partial.Plan.Banks.Count);
            for (var index = 0; index < partial.Plan.Banks.Count; index++)
            {
                var target = partial.Plan.Banks[index];
                var handle = partial.Find(target);
                if (handle == null)
                {
                    handle = await SendQualificationCleanupCommandAsync(
                        "Recorder Double continue exact bank Adopt",
                        () => diagnostics.AdoptRecorderAsync(
                            partial.Plan.DiagnosticsBootId,
                            target.RecordId,
                            target.BufferId,
                            CancellationToken.None),
                        value =>
                        {
                            handle = value;
                            partial.Add(target, value);
                        });
                    ValidateRecorderDoubleRawAdoptedIdentity(
                        partial.Plan,
                        target,
                        handle,
                        partial.AdoptedBanks);
                    partial.Add(target, handle);
                }

                await HydrateAndValidateRecorderDoubleAdoptedIdentityAsync(
                    partial.Plan,
                    target,
                    handle,
                    partial.AdoptedBanks,
                    diagnostics);
                ordered.Add(handle);
            }

            return new RecorderDoubleRecoveryResult(
                partial.Plan,
                partial.Inventory,
                null,
                ordered);
        }

        private async Task ReleaseRecoveredRecorderDoubleAsync(
            RecorderDoubleRecoveryResult result,
            LMCDiagnostics diagnostics)
        {
            var coordinator = new RecorderDoubleDurableReleaseCoordinator(
                recorderDoubleRecoveryJournal,
                result.Plan.JournalIdentity,
                () => DateTime.UtcNow);
            if (result.Plan.Route
                == RecorderDoubleRecoveryRoute.AdoptEmptyConfiguration)
            {
                await coordinator
                    .ReleaseRecoveredEmptyConfigurationAndResolveAsync(
                        result,
                        handle => SendQualificationCleanupCommandAsync(
                            "Recorder Double recovered empty configuration Release",
                            () => diagnostics.ReleaseRecorderAsync(
                                handle,
                                CancellationToken.None)),
                        true,
                        CancellationToken.None);
                return;
            }

            if (result.Plan.Route
                != RecorderDoubleRecoveryRoute.AdoptOccupiedBanks)
            {
                throw new InvalidOperationException(
                    "Only exact adopted Double-bank recovery results may enter Release.");
            }

            for (var index = result.AdoptedBanks.Count - 1;
                index >= 0;
                index--)
            {
                var handle = result.AdoptedBanks[index];
                if (!handle.IsBufferReleased)
                {
                    await EnsureRecoveredRecorderDoubleBankReleasableAsync(
                        result.Plan,
                        handle,
                        diagnostics);
                }

                await coordinator.ReleaseRecoveredBankAsync(
                    result,
                    handle,
                    value => SendQualificationCleanupCommandAsync(
                        "Recorder Double recovered bank Release",
                        () => diagnostics.ReleaseRecorderBufferAsync(
                            value,
                            CancellationToken.None)),
                    true,
                    CancellationToken.None);
            }

            var configurationHandle =
                SelectRecorderDoubleConfigurationReleaseHandle(result);
            await coordinator
                .ReleaseRecoveredOccupiedConfigurationAndResolveAsync(
                    result,
                    configurationHandle,
                    value => SendQualificationCleanupCommandAsync(
                        "Recorder Double recovered configuration Release",
                        () => diagnostics.ReleaseRecorderAsync(
                            value,
                            CancellationToken.None)),
                    true,
                    CancellationToken.None);
        }

        private async Task EnsureRecoveredRecorderDoubleBankReleasableAsync(
            RecorderDoubleRecoveryPlan plan,
            LMCRecorderIdentity identity,
            LMCDiagnostics diagnostics)
        {
            var operations = new RecorderQualificationCleanupOperations
            {
                ReadStatusAsync = () => SendQualificationCleanupCommandAsync(
                    "Recorder Double recovered Status",
                    () => diagnostics.GetRecorderStatusAsync(
                        identity,
                        CancellationToken.None)),
                StopAsync = () => SendQualificationCleanupCommandAsync(
                    "Recorder Double recovered Stop",
                    () => diagnostics.StopRecorderAsync(
                        identity,
                        CancellationToken.None)),
                ValidateStatus = status =>
                    ValidateRecorderDoubleRecoveredStatus(
                        plan,
                        identity,
                        status),
                DelayAsync = milliseconds => Task.Delay(milliseconds),
                StopRaceResolved = status => { },
                RecoveryRequired = status => { }
            };
            await RecorderQualificationCleanupOrchestrator
                .EnsureReleasableStateAsync(
                    operations,
                    RecorderQualificationRpcTimeoutMilliseconds,
                    RecorderQualificationPollMilliseconds);
        }

        private void RetainRecorderDoubleRecoveryResult(
            RecorderDoubleRecoveryResult result,
            LMCConnection ownerConnection,
            LMCDiagnostics diagnostics)
        {
            recorderDoubleRetainedRecoveryResult = result
                ?? throw new ArgumentNullException("result");
            recorderDoublePartialRecoveryAdoption = null;
            recorderDoubleRetainedRecoveryConnection = ownerConnection;
            recorderDoubleRetainedRecoveryDiagnostics = diagnostics;
        }

        private void ClearRecorderDoubleRetainedQualification()
        {
            recorderDoubleRetainedQualificationResult = null;
            recorderDoubleRetainedQualificationScope = null;
            recorderDoubleRetainedQualificationOperations = null;
            recorderDoubleRetainedQualificationReleaseCoordinator = null;
            recorderDoubleRetainedQualificationConnection = null;
            recorderDoubleRetainedQualificationDiagnostics = null;
            recorderDoubleRetainedQualificationError = null;
        }

        private void ClearRecorderDoubleRetainedRecovery()
        {
            recorderDoubleRetainedRecoveryResult = null;
            recorderDoublePartialRecoveryAdoption = null;
            recorderDoubleRetainedRecoveryConnection = null;
            recorderDoubleRetainedRecoveryDiagnostics = null;
        }

        private void ClearRecorderDoubleVolatileSessionState()
        {
            ClearRecorderDoubleRetainedQualification();
            ClearRecorderDoubleRetainedRecovery();
            if (CheckConfirmRecorderDoubleRelease != null)
            {
                CheckConfirmRecorderDoubleRelease.IsChecked = false;
            }
        }

        private void CommitRecorderDoubleRecoveryMutation()
        {
            CommitQualificationIrreversibleOutcome(
                "Recorder Double recovery mutation was accepted");
        }

        private void RecorderDoubleReleaseConfirmation_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateUiState();
        }

        private void EnsureRecorderDoubleReleaseConfirmed()
        {
            if (CheckConfirmRecorderDoubleRelease.IsChecked != true)
            {
                throw new InvalidOperationException(
                    "Explicitly confirm the exact Double-bank journal identity and Release order before cleanup or recovery.");
            }
        }

        private void ConsumeRecorderDoubleReleaseConfirmation()
        {
            EnsureRecorderDoubleReleaseConfirmed();
            CheckConfirmRecorderDoubleRelease.IsChecked = false;
        }

        private void EnsureRecorderDoubleLifecycleAdmission(
            bool requireRetainedQualification,
            string operation)
        {
            var denial = GetRecorderDoubleLifecycleAdmissionDenial(
                requireRetainedQualification);
            if (denial != null)
            {
                throw new InvalidOperationException(
                    "Recorder Double " + operation + " is blocked: " + denial);
            }
        }

        private string GetRecorderDoubleLifecycleAdmissionDenial(
            bool requireRetainedQualification)
        {
            if (RecorderDoubleRecoveryJournalUnavailable)
            {
                return GetRecorderDoubleRecoveryJournalUnavailableGuidance();
            }

            if (!HasActiveRecorderDoubleRecoveryJournalRecord)
            {
                return "no active durable Double-bank record exists.";
            }

            if (DiagnosticsMutationJournalUnavailable
                || HasActiveDiagnosticsMutationJournalRecord
                || HasPendingD5SdoWriteReadback
                || HasD5SdoTicketOrQuarantine
                || HasUnresolvedDigitalOutputWrite)
            {
                return "another diagnostics mutation, D5 ticket/readback, digital-output write, or unavailable general journal is unresolved.";
            }

            if (motionMayBeActive)
            {
                return "motion may still be active.";
            }

            if (bulkConfiguration != null && !bulkConfiguration.IsReleased)
            {
                return "a Bulk reader is still owned by this session.";
            }

            if ((recorderConfiguration != null
                    && !recorderConfiguration.IsReleased)
                || (recorderIdentity != null
                    && !recorderIdentity.IsRecorderReleased))
            {
                return "the ordinary single-handle Recorder UI still owns a resource.";
            }

            if (requireRetainedQualification)
            {
                if (!HasRecorderDoubleRetainedQualification)
                {
                    return "no same-session retained Double-bank scope exists.";
                }

                if (HasRecorderDoubleRetainedRecovery)
                {
                    return "a reconnect recovery result is already retained.";
                }

                var scope = recorderDoubleRetainedQualificationScope;
                if ((scope.ConfigurationAttempted
                        && scope.Configuration == null)
                    || (scope.BankAStartAttempted
                        && scope.BankA == null)
                    || (scope.BankBStartAttempted
                        && scope.BankB == null))
                {
                    return "a same-session Configure or bank Start has no exact returned handle. Send no Release; disconnect/reconnect for token-qualified exact inventory recovery.";
                }

                if (scope.UnexpectedThird != null
                    || (scope.ThirdStartAttempted
                        && !scope.ThirdStartExactBusyConfirmed))
                {
                    return "third Start was not exact ResourceBusy. The two-bank durable journal cannot identify a safe same-session Release target; disconnect/reconnect only for exact inventory inspection. Conflicting inventory requires external manual recovery and no automatic Release.";
                }
            }
            else if (HasRecorderDoubleRetainedQualification)
            {
                return "same-session qualification handles exist; use Cleanup Retained Double instead of reconnect recovery.";
            }

            return null;
        }

        private static void
            EnsureRecorderDoubleSameSessionThirdStartIsReleasable(
                RecorderDoubleBankRecoveryScope scope)
        {
            if (scope == null
                || scope.UnexpectedThird != null
                || (scope.ThirdStartAttempted
                    && !scope.ThirdStartExactBusyConfirmed))
            {
                throw new InvalidOperationException(
                    "Same-session Double cleanup is blocked because third Start was not exact ResourceBusy. Send no Release; disconnect/reconnect only for exact inventory inspection. Conflicting inventory requires external manual recovery and no automatic Release.");
            }
        }

        private void EnsureRecorderDoubleRecoveryCapabilityIdentity()
        {
            var record = recorderDoubleRecoveryJournal.CurrentRecord;
            if (record == null
                || diagnosticCapabilities == null
                || !diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.RecorderSingleBank)
                || !diagnosticCapabilities.Supports(
                    LMCDiagnosticCapability.RecorderDoubleBank)
                || diagnosticCapabilities.RecorderBufferCount != 2
                || diagnosticCapabilities.DiagnosticsBootId
                    != record.DiagnosticsBootId
                || diagnosticCapabilities.MapRevision != record.MapRevision)
            {
                throw new InvalidOperationException(
                    "Current capabilities do not match the exact durable Double-bank BootId, MapRevision, and two-buffer contract.");
            }
        }

        private static void
            EnsureRecorderDoubleQualificationProofGatesReady()
        {
            var missing = new List<string>();
            if (!RecorderDoubleQualificationExecutionReady)
            {
                missing.Add("QualificationExecution proof gate is CLOSED");
            }

            if (!RecorderDoubleReconnectRecoveryReady)
            {
                missing.Add("ReconnectRecovery proof gate is CLOSED");
            }

            if (missing.Count != 0)
            {
                throw new InvalidOperationException(
                    "Double-bank qualification is blocked before wire: "
                    + string.Join("; ", missing)
                    + ".");
            }
        }

        internal static bool
            IsRecorderDoubleSameSessionCleanupRouteReady(
                RecorderDoubleBankRecoveryScope scope,
                bool manualActionsReady,
                bool manualConfigureRouteReady,
                bool qualificationExecutionReady,
                bool reconnectRecoveryReady)
        {
            if (scope != null && scope.ConfigurationOnlyRetention)
            {
                return scope.HasValidConfigurationOnlyRetentionShape
                    && manualActionsReady
                    && manualConfigureRouteReady;
            }

            return qualificationExecutionReady
                && reconnectRecoveryReady;
        }

        private static void
            EnsureRecorderDoubleSameSessionCleanupRouteReady(
                RecorderDoubleBankRecoveryScope scope)
        {
            if (scope != null
                && scope.ConfigurationOnlyRetention
                && !scope.HasValidConfigurationOnlyRetentionShape)
            {
                throw new InvalidOperationException(
                    "Manual Double configuration-only cleanup is blocked before wire: the retained scope contains bank or Start state.");
            }

            if (IsRecorderDoubleSameSessionCleanupRouteReady(
                    scope,
                    RecorderDoubleManualActionsReady,
                    RecorderDoubleManualConfigureRouteReady,
                    RecorderDoubleQualificationExecutionReady,
                    RecorderDoubleReconnectRecoveryReady))
            {
                return;
            }

            throw new InvalidOperationException(
                scope != null && scope.ConfigurationOnlyRetention
                    ? "Manual Double configuration-only cleanup is blocked before wire: ManualActions or ManualConfigureRoute gate is CLOSED."
                    : "Double-bank qualification cleanup is blocked before wire: QualificationExecution or ReconnectRecovery proof gate is CLOSED.");
        }

        private void EnsureRecorderDoubleSession(
            LMCConnection expectedConnection,
            LMCDiagnostics diagnostics,
            string operation)
        {
            if (expectedConnection == null
                || diagnostics == null
                || !ReferenceEquals(connection, expectedConnection)
                || !expectedConnection.IsConnected
                || !ReferenceEquals(
                    expectedConnection.Diagnostics,
                    diagnostics))
            {
                throw new InvalidOperationException(
                    "Recorder Double "
                    + operation
                    + " belongs to a stale RPC session. Use durable reconnect recovery.");
            }
        }

        internal static uint CreateRecorderDoubleRequestedConfigId(
            Guid recoveryIdentity)
        {
            if (recoveryIdentity == Guid.Empty)
            {
                throw new ArgumentException(
                    "A nonempty Double-bank recovery identity is required.",
                    "recoveryIdentity");
            }

            var bytes = recoveryIdentity.ToByteArray();
            var value = (uint)(bytes[0]
                | (bytes[1] << 8)
                | (bytes[2] << 16)
                | (bytes[3] << 24));
            return value == 0 ? 1u : value;
        }

        internal static bool IsExactRecorderDoubleResourceBusy(
            Exception error)
        {
            var commandError = error as LMCDiagnosticsCommandException;
            return commandError != null
                && commandError.Response != null
                && commandError.Response.Detail
                    == LMCDiagnosticsDetailCode.ResourceBusy;
        }

        private static LMCRecorderConfigurationHandle
            RequireRecorderDoubleNativeConfiguration(
                RecorderDoubleBankConfigurationLease configuration)
        {
            var native = configuration == null
                ? null
                : configuration.NativeHandle
                    as LMCRecorderConfigurationHandle;
            if (native == null)
            {
                throw new InvalidOperationException(
                    "Double-bank configuration wrapper has no exact native handle.");
            }

            return native;
        }

        private static LMCRecorderIdentity
            RequireRecorderDoubleNativeIdentity(
                RecorderDoubleBankCaptureLease capture)
        {
            var native = capture == null
                ? null
                : capture.NativeIdentity as LMCRecorderIdentity;
            if (native == null)
            {
                throw new InvalidOperationException(
                    "Double-bank capture wrapper has no exact native identity.");
            }

            return native;
        }

        private static void ValidateRecorderDoubleAdoptedIdentity(
            RecorderDoubleRecoveryPlan plan,
            RecorderDoubleRecoveryBankTarget target,
            LMCRecorderIdentity handle,
            IReadOnlyList<LMCRecorderIdentity> priorHandles)
        {
            uint priorOwnerSessionEpoch = 0;
            if (priorHandles != null && priorHandles.Count != 0)
            {
                priorOwnerSessionEpoch = priorHandles[0].OwnerSessionEpoch;
            }

            if (handle == null
                || handle.Response == null
                || !handle.Response.IsSuccess
                || !handle.IsAdopted
                || handle.DiagnosticsBootId != plan.DiagnosticsBootId
                || handle.RecordId != target.RecordId
                || handle.BufferId != target.BufferId
                || handle.MapRevision != plan.MapRevision
                || handle.OwnerSessionEpoch == 0
                || handle.OwnerSessionEpoch
                    == plan.PreviousOwnerSessionEpoch
                || (priorOwnerSessionEpoch != 0
                    && handle.OwnerSessionEpoch
                        != priorOwnerSessionEpoch)
                || handle.InitialState < LMCRecorderState.Armed
                || handle.InitialState > LMCRecorderState.Fault
                || !handle.HasConfigurationMetadata)
            {
                throw new InvalidOperationException(
                    "Double-bank recovery Adopt returned an invalid exact bank or owner identity.");
            }
        }

        private static void ValidateRecorderDoubleRawAdoptedIdentity(
            RecorderDoubleRecoveryPlan plan,
            RecorderDoubleRecoveryBankTarget target,
            LMCRecorderIdentity handle,
            IReadOnlyList<LMCRecorderIdentity> priorHandles)
        {
            uint priorOwnerSessionEpoch = 0;
            if (priorHandles != null && priorHandles.Count != 0)
            {
                priorOwnerSessionEpoch = priorHandles[0].OwnerSessionEpoch;
            }

            if (handle == null
                || handle.Response == null
                || !handle.Response.IsSuccess
                || !handle.IsAdopted
                || handle.DiagnosticsBootId != plan.DiagnosticsBootId
                || handle.RecordId != target.RecordId
                || handle.BufferId != target.BufferId
                || handle.MapRevision != plan.MapRevision
                || handle.OwnerSessionEpoch == 0
                || handle.OwnerSessionEpoch
                    == plan.PreviousOwnerSessionEpoch
                || (priorOwnerSessionEpoch != 0
                    && handle.OwnerSessionEpoch
                        != priorOwnerSessionEpoch)
                || handle.InitialState < LMCRecorderState.Armed
                || handle.InitialState > LMCRecorderState.Fault)
            {
                throw new InvalidOperationException(
                    "Double-bank recovery Adopt returned an invalid raw bank or owner identity.");
            }
        }

        private async Task
            HydrateAndValidateRecorderDoubleAdoptedIdentityAsync(
                RecorderDoubleRecoveryPlan plan,
                RecorderDoubleRecoveryBankTarget target,
                LMCRecorderIdentity handle,
                IReadOnlyList<LMCRecorderIdentity> adoptedHandles,
                LMCDiagnostics diagnostics)
        {
            var status = await SendQualificationCleanupCommandAsync(
                "Recorder Double adopted identity Status",
                () => diagnostics.GetRecorderStatusAsync(
                    handle,
                    CancellationToken.None));
            ValidateRecorderDoubleRecoveredStatus(plan, handle, status);
            ValidateRecorderDoubleAdoptedIdentity(
                plan,
                target,
                handle,
                adoptedHandles);
        }

        private static void ValidateRecorderDoubleRecoveredStatus(
            RecorderDoubleRecoveryPlan plan,
            LMCRecorderIdentity identity,
            LMCRecorderStatus status)
        {
            if (status == null
                || status.Response == null
                || !status.Response.IsSuccess
                || identity == null
                || status.DiagnosticsBootId != plan.DiagnosticsBootId
                || status.ConfigId != plan.ConfigId
                || status.ConfigRevision != plan.ConfigRevision
                || status.MapRevision != plan.MapRevision
                || status.RecordId != identity.RecordId
                || status.BufferId != identity.BufferId
                || status.OwnerSessionEpoch != identity.OwnerSessionEpoch
                || status.Capacity == 0)
            {
                throw new InvalidOperationException(
                    "Recovered Double-bank Status does not match the exact adopted identity and durable plan.");
            }
        }

        private static LMCRecorderIdentity
            SelectRecorderDoubleConfigurationReleaseHandle(
                RecorderDoubleRecoveryResult result)
        {
            LMCRecorderIdentity fallback = null;
            for (var index = 0; index < result.AdoptedBanks.Count; index++)
            {
                var handle = result.AdoptedBanks[index];
                if (fallback == null)
                {
                    fallback = handle;
                }

                if (handle.BufferId == 0)
                {
                    return handle;
                }
            }

            if (fallback == null)
            {
                throw new InvalidOperationException(
                    "Recovered occupied Double-bank result has no configuration Release handle.");
            }

            return fallback;
        }

        private void RememberRecorderDoubleJournalRuntimeError(
            Exception error)
        {
            recorderDoubleRecoveryJournalRuntimeError =
                error.GetType().Name + ": " + error.Message;
        }

        internal static bool IsRecorderDoubleJournalRuntimeFailure(
            Exception error)
        {
            return error is System.IO.IOException
                || error is UnauthorizedAccessException
                || error is System.Security.SecurityException
                || error is NotSupportedException
                || error is ObjectDisposedException;
        }

        private sealed class RecorderDoubleRecoveryAttemptState
        {
            internal LMCRecorderBankInventory Inventory { get; set; }
            internal RecorderDoubleRecoveryPlan Plan { get; set; }
            internal LMCRecoveredRecorderConfigurationLease
                EmptyConfigurationLease { get; set; }
            internal RecorderDoublePartialRecoveryAdoption PartialAdoption
            {
                get;
                set;
            }
        }
    }
}

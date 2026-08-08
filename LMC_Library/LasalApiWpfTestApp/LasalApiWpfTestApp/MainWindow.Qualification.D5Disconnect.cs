using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LasalMotionControlLib;

namespace LasalMotionControlApiExample
{
    public partial class MainWindow
    {
        private async void
            ButtonRunD5SdoDisconnectRecoveryQualification_Click(
                object sender,
                RoutedEventArgs e)
        {
            if (!HasCachedD5ReadQualificationContract())
            {
                const string reason =
                    "Not started: the cached PLC capabilities do not advertise the exact D5 SDO Read contract (SDORead + SDOReadGeneralInline, MaxSdoDataBytes=4, nonzero BootId/MapRevision). No RPC was sent.";
                TextD5SdoQualificationProgress.Text = reason;
                TextOperationState.Text =
                    "D5 abrupt-disconnect recovery not started";
                WriteLog(reason);
                return;
            }

            D5SdoQualificationInput input;
            try
            {
                input = ReadD5SdoQualificationInput();
            }
            catch (Exception error)
            {
                TextD5SdoQualificationProgress.Text =
                    "Not started: " + error.Message;
                TextOperationState.Text =
                    "D5 abrupt-disconnect recovery validation failed";
                WriteLog(
                    "D5 abrupt-disconnect recovery not started: "
                    + error.Message);
                return;
            }

            try
            {
                await RunQualificationAsync(
                    "D5SdoAbruptDisconnectApplicationRecovery",
                    cancellationToken =>
                        RunD5SdoDisconnectApplicationRecoveryAsync(
                            input,
                            cancellationToken));
            }
            finally
            {
                UpdateUiState();
                RefreshD5SdoQualificationOutput();
            }
        }

        private async Task RunD5SdoDisconnectApplicationRecoveryAsync(
            D5SdoQualificationInput input,
            CancellationToken cancellationToken)
        {
            if (d5SdoQualificationActiveTicket != null)
            {
                SetD5SdoQualificationProgress(
                    1,
                    "Resolving the previous D5 ticket before abrupt-disconnect recovery");
                await CleanupPendingD5SdoQualificationAsync();
            }

            if (d5SdoQualificationQuarantine.HasEntries)
            {
                throw new InvalidOperationException(
                    "A D5 ticket or uncertain submission remains quarantined. "
                    + GetD5SdoResolutionGuidance());
            }

            var remoteAddress = RequiredText(TextRemoteIp.Text, "PLC IP");
            var remotePort = ParsePort(
                TextRemotePort.Text,
                "TCP port",
                false);
            var localAddress = RequiredText(
                TextLocalIp.Text,
                "PC local IPv4");
            var callbackPort = ParsePort(
                TextCallbackPort.Text,
                "Callback UDP port",
                true);
            var oldConnection = RequireConnection();
            LMCConnection newConnection = null;
            var newConnectionAdopted = false;
            D5SdoDisconnectOrphanRecoveryScope publishedRecoveryScope = null;
            Exception publishedRecoveryError = null;

            try
            {
                EnsureNoPendingManualD5Operation();
                SetD5SdoQualificationProgress(
                    4,
                    "Verifying selected physical axis is powered off and stationary");
                await VerifyD5SdoQualificationSafeAxisAsync(
                    oldConnection,
                    input.SlaveReference,
                    input.AxisObjectName,
                    cancellationToken);

                SetD5SdoQualificationProgress(
                    10,
                    "Refreshing and comparing old-owner D5 capabilities");
                var firstCapabilities =
                    await ReadD5SdoQualificationCapabilitiesAsync(
                        oldConnection.Diagnostics,
                        cancellationToken,
                        "disconnect capability sample 1");
                await Task.Delay(
                    D5SdoQualificationPollMilliseconds,
                    cancellationToken);
                var capabilities =
                    await ReadD5SdoQualificationCapabilitiesAsync(
                        oldConnection.Diagnostics,
                        cancellationToken,
                        "disconnect capability sample 2");
                RequireStableD5SdoQualificationCapabilities(
                    firstCapabilities,
                    capabilities,
                    "disconnect preflight");
                diagnosticCapabilities = capabilities;
                TextDiagnosticsCapabilities.Text =
                    FormatCapabilities(capabilities);

                var terminalWaitMilliseconds =
                    GetD5SdoQualificationTerminalWaitMilliseconds(
                        input.TimeoutCycles,
                        capabilities.BaseCycleTimeUs);
                var recoveryRequest = LMCSdoRequest.CreateRead(
                    input.SlaveReference,
                    0x6061,
                    0,
                    LMCSignalValueType.Int8,
                    1,
                    input.TimeoutCycles);

                WriteD5SdoQualificationLog(
                    "event=D5_DISCONNECT_PREFLIGHT",
                    "wireMutation=false",
                    "disconnectMode=LOCAL_TCP_ZERO_LINGER_CLOSE_NO_RPC_CLOSE",
                    "slave=" + input.SlaveReference.ToString(
                        CultureInfo.InvariantCulture),
                    "probeObject=0x" + input.ObjectIndex.ToString("X4"),
                    "probeSubIndex=" + input.SubIndex.ToString(
                        CultureInfo.InvariantCulture),
                    "probeValueType=" + input.ValueType,
                    "probeLength=" + input.DataLength.ToString(
                        CultureInfo.InvariantCulture),
                    "bootId=0x" + capabilities.DiagnosticsBootId.ToString("X8"),
                    "mapRevision=0x" + capabilities.MapRevision.ToString("X8"),
                    "plcOrphanLifecycleWitness=false",
                    "orphanQualified=false",
                    "verdict=PASS");

                SetD5SdoQualificationProgress(
                    18,
                    "Reading exact 0x6061:0 baseline before disconnect");
                var baselineTicket = await SubmitD5SdoQualificationAsync(
                    oldConnection,
                    oldConnection.Diagnostics,
                    recoveryRequest,
                    capabilities.DiagnosticsBootId,
                    capabilities.MapRevision,
                    cancellationToken,
                    "disconnect-baseline",
                    "0x6061",
                    0,
                    terminalWaitMilliseconds);
                var baselineStatus =
                    await WaitForD5SdoQualificationTerminalAsync(
                        oldConnection.Diagnostics,
                        baselineTicket,
                        terminalWaitMilliseconds,
                        cancellationToken,
                        "disconnect-baseline");
                var expectedRecoveryData = baselineStatus.ResultData;
                D5SdoQualificationAnalysis.ValidateKnownValidRecovery(
                    baselineTicket,
                    baselineStatus,
                    capabilities.DiagnosticsBootId,
                    LMCSignalValueType.Int8,
                    expectedRecoveryData);
                WriteD5SdoQualificationTerminalLog(
                    "disconnect-baseline",
                    baselineTicket,
                    baselineStatus);

                var beforeDisconnectCapabilities =
                    await ReadD5SdoQualificationCapabilitiesAsync(
                        oldConnection.Diagnostics,
                        cancellationToken,
                        "disconnect pre-submit capability");
                RequireStableD5SdoQualificationCapabilities(
                    capabilities,
                    beforeDisconnectCapabilities,
                    "baseline-to-disconnect");

                SetD5SdoQualificationProgress(
                    32,
                    "Submitting the old-owner probe and waiting for Running/Queued evidence");
                var recoveryRetryClock = Stopwatch.StartNew();
                var result = await
                    D5SdoDisconnectOrphanQualificationOrchestrator.RunAsync(
                        new D5SdoDisconnectOrphanQualificationRequest(
                            oldConnection,
                            beforeDisconnectCapabilities,
                            input.AbortRequest,
                            recoveryRequest,
                            expectedRecoveryData),
                        d5SdoQualificationQuarantine,
                        new D5SdoDisconnectOrphanQualificationOperations
                        {
                            IsConnected = item => item != null
                                && item.IsConnected,
                            SubmitOldReadAsync = (owner, request, token) =>
                                SubmitD5SdoDisconnectRawAsync(
                                    owner,
                                    request,
                                    token,
                                    "disconnect-old-probe"),
                            ObserveOwnerTransportLossAsync =
                                (ticket, token) =>
                                    ObserveAndAbortD5OldOwnerAsync(
                                        oldConnection,
                                        ticket,
                                        terminalWaitMilliseconds,
                                        token),
                            OpenNewConnectionAsync = async token =>
                            {
                                newConnection = await
                                    OpenD5DisconnectRecoveryConnectionAsync(
                                        remoteAddress,
                                        remotePort,
                                        localAddress,
                                        callbackPort,
                                        token);
                                return newConnection;
                            },
                            ReadCapabilitiesAsync = (owner, token) =>
                                ReadD5SdoDisconnectCapabilitiesAsync(
                                    owner,
                                    token),
                            SubmitRecoveryReadAsync =
                                (owner, request, token) =>
                                    SubmitD5SdoDisconnectRawAsync(
                                        owner,
                                        request,
                                        token,
                                        "disconnect-recovery"),
                            WaitRecoveryTerminalAsync =
                                (owner, ticket, token) =>
                                    WaitForD5SdoDisconnectTerminalAsync(
                                        owner,
                                        ticket,
                                        terminalWaitMilliseconds,
                                        token),
                            GetMonotonicMilliseconds = () =>
                                recoveryRetryClock.ElapsedMilliseconds,
                            DelayAsync = (milliseconds, token) =>
                                Task.Delay(milliseconds, token),
                            WritePassLog =
                                WriteD5DisconnectApplicationRecoveryPassLog,
                            RecoveryRequired = (scope, error) =>
                            {
                                publishedRecoveryScope = scope;
                                publishedRecoveryError = error;
                            }
                        },
                        cancellationToken);

                if (result.Disposition
                    == D5SdoDisconnectOrphanQualificationDisposition
                        .TerminalBeforeTransportLoss)
                {
                    WriteD5SdoQualificationLog(
                        "event=D5_DISCONNECT_ASSERT",
                        "oldTicket=" + result.RecoveryScope.OldTicket.TicketId
                            .ToString(CultureInfo.InvariantCulture),
                        "lastOldState="
                            + result.RecoveryScope.LastStatusBeforeLoss.State,
                        "transportAborted=false",
                        "newConnectionRecovery=false",
                        "plcOrphanLifecycleWitness=false",
                        "orphanQualified=false",
                        "verdict=INCONCLUSIVE_TERMINAL_BEFORE_LOSS");
                    throw new QualificationInconclusiveException(
                        "The probe reached a terminal state before the local TCP abort. No disconnect recovery was exercised; choose a probe that remains pending long enough to observe Running or Queued.");
                }

                CommitQualificationIrreversibleOutcome(
                    "D5 application-recovery proof was logged and its quarantine evidence was cleared");
                AdoptD5DisconnectRecoveryConnection(
                    oldConnection,
                    newConnection,
                    result.RecoveryScope.FinalRecoveryCapabilities);
                newConnectionAdopted = true;
                await TryAutoLoadEtherCATTopologyAfterConnectAsync(
                    newConnection);
                SetD5SdoQualificationProgress(
                    98,
                    "Abrupt TCP disconnect -> distinct new connection -> two-ticket application recovery PASS; PLC orphan proof remains unavailable");
            }
            catch
            {
                if (publishedRecoveryScope != null
                    && publishedRecoveryError != null)
                {
                    WriteD5SdoDisconnectRecoveryScope(
                        publishedRecoveryScope,
                        publishedRecoveryError);
                }

                if (newConnection != null
                    && newConnection.IsConnected
                    && !ReferenceEquals(connection, newConnection))
                {
                    AdoptD5DisconnectRecoveryConnection(
                        oldConnection,
                        newConnection,
                        null);
                    newConnectionAdopted = true;
                    await TryAutoLoadEtherCATTopologyAfterConnectAsync(
                        newConnection);
                }
                else if (ReferenceEquals(connection, newConnection))
                {
                    newConnectionAdopted = true;
                }
                else if (!oldConnection.IsConnected
                    && ReferenceEquals(connection, oldConnection))
                {
                    DetachConnection(oldConnection);
                    connection = null;
                    ClearLoadedObjects();
                    UpdateUiState();
                }

                throw;
            }
            finally
            {
                if (!oldConnection.IsConnected
                    && !ReferenceEquals(connection, oldConnection))
                {
                    oldConnection.Dispose();
                }

                if (newConnection != null && !newConnectionAdopted)
                {
                    newConnection.Dispose();
                }
            }
        }

        private async Task<D5SdoOwnerTransportLossObservation>
            ObserveAndAbortD5OldOwnerAsync(
                LMCConnection oldConnection,
                LMCOperationTicket ticket,
                int timeoutMilliseconds,
                CancellationToken cancellationToken)
        {
            var timeout = Stopwatch.StartNew();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var status = await SendQualificationCommandAsync(
                    "D5 disconnect old-owner status",
                    cancellationToken,
                    () => oldConnection.Diagnostics.GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None));
                if (status.IsTerminal)
                {
                    return new D5SdoOwnerTransportLossObservation(
                        status,
                        false);
                }

                if (status.State == LMCOperationState.Running
                    || timeout.ElapsedMilliseconds
                        >= Math.Min(
                            D5SdoQualificationSafetyTimeoutMilliseconds,
                            timeoutMilliseconds))
                {
                    await AbortD5OldOwnerTransportAsync(
                        oldConnection,
                        cancellationToken);
                    if (oldConnection.IsConnected)
                    {
                        throw new InvalidOperationException(
                            "The qualification TCP abort returned while the old owner still reported Connected.");
                    }

                    return new D5SdoOwnerTransportLossObservation(
                        status,
                        true);
                }

                await Task.Delay(
                    D5SdoQualificationPollMilliseconds,
                    cancellationToken);
            }
        }

        private async Task AbortD5OldOwnerTransportAsync(
            LMCConnection oldConnection,
            CancellationToken cancellationToken)
        {
            const string operation =
                "D5 qualification local TCP abort";
            cancellationToken.ThrowIfCancellationRequested();
            await commandSendGate.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureNoNewSafetyRequest(
                    qualificationSafetyGeneration,
                    operation);

                // Stop/PowerOff reserves its generation on the UI thread
                // before waiting for this gate. Run the final check and the
                // actual abort on that same thread so a safety reservation
                // cannot race between them.
                Dispatcher.Invoke(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        EnsureNoNewSafetyRequest(
                            qualificationSafetyGeneration,
                            operation);
                        using (sendPriorityCoordinator.BeginPreemptibleScope(
                            qualificationSafetyGeneration,
                            operation))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            EnsureNoNewSafetyRequest(
                                qualificationSafetyGeneration,
                                operation);
                            oldConnection.AbortTransportForQualification();
                        }
                    });
            }
            finally
            {
                commandSendGate.Release();
            }
        }

        private async Task<LMCConnection>
            OpenD5DisconnectRecoveryConnectionAsync(
                string remoteAddress,
                int remotePort,
                string localAddress,
                int callbackPort,
                CancellationToken cancellationToken)
        {
            var admission = EvaluateDiagnosticsAdmission(
                DiagnosticsAdmissionOperation.ConnectOrReconnect);
            if (!admission.IsAllowed)
            {
                throw CreateDiagnosticsAdmissionException(
                    "D5 abrupt-disconnect recovery reconnect",
                    admission);
            }

            var recoveryConnection = CreateCoordinatedConnection();
            try
            {
                await recoveryConnection.RpcInitConnectionAsync(
                    remoteAddress,
                    remotePort,
                    localAddress,
                    callbackPort,
                    1u,
                    cancellationToken);
                return recoveryConnection;
            }
            catch
            {
                recoveryConnection.Dispose();
                throw;
            }
        }

        private Task<LMCDiagnosticCapabilities>
            ReadD5SdoDisconnectCapabilitiesAsync(
                LMCConnection owner,
                CancellationToken cancellationToken)
        {
            return SendQualificationCommandAsync(
                "D5 disconnect recovery capabilities",
                cancellationToken,
                () => owner.Diagnostics.GetCapabilitiesAsync(
                    CancellationToken.None));
        }

        private Task<LMCOperationTicket> SubmitD5SdoDisconnectRawAsync(
            LMCConnection owner,
            LMCSdoRequest request,
            CancellationToken cancellationToken,
            string stage)
        {
            return SendQualificationCommandAsync(
                "D5 SDO " + stage + " submit",
                cancellationToken,
                () => owner.Diagnostics.SubmitSdoAsync(
                    request,
                    CancellationToken.None));
        }

        private async Task<LMCOperationStatus>
            WaitForD5SdoDisconnectTerminalAsync(
                LMCConnection owner,
                LMCOperationTicket ticket,
                int timeoutMilliseconds,
                CancellationToken cancellationToken)
        {
            var timeout = Stopwatch.StartNew();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var status = await SendQualificationCommandAsync(
                    "D5 disconnect recovery status",
                    cancellationToken,
                    () => owner.Diagnostics.GetOperationStatusAsync(
                        ticket,
                        CancellationToken.None));
                if (status.IsTerminal)
                {
                    return status;
                }

                if (timeout.ElapsedMilliseconds >= timeoutMilliseconds)
                {
                    throw new TimeoutException(
                        "D5 disconnect recovery ticket "
                        + ticket.TicketId.ToString(
                            CultureInfo.InvariantCulture)
                        + " did not reach a terminal state within "
                        + timeoutMilliseconds.ToString(
                            CultureInfo.InvariantCulture)
                        + " ms. LastState="
                        + status.State
                        + ".");
                }

                await Task.Delay(
                    D5SdoQualificationPollMilliseconds,
                    cancellationToken);
            }
        }

        private void AdoptD5DisconnectRecoveryConnection(
            LMCConnection oldConnection,
            LMCConnection newConnection,
            LMCDiagnosticCapabilities capabilities)
        {
            if (newConnection == null || !newConnection.IsConnected)
            {
                throw new InvalidOperationException(
                    "The D5 disconnect recovery connection is not connected and cannot be adopted by the GUI.");
            }

            if (ReferenceEquals(connection, newConnection))
            {
                return;
            }

            if (ReferenceEquals(connection, oldConnection))
            {
                DetachConnection(oldConnection);
            }

            AttachConnection(newConnection);
            connection = newConnection;
            ClearLoadedObjects();
            if (capabilities != null)
            {
                diagnosticCapabilities = capabilities;
                TextDiagnosticsCapabilities.Text =
                    FormatCapabilities(capabilities);
            }

            UpdateUiState();
        }

        private bool HasCachedD5ReadQualificationContract()
        {
            var capabilities = diagnosticCapabilities;
            var owner = connection;
            return owner != null
                && owner.IsConnected
                && capabilities != null
                && capabilities.Response != null
                && capabilities.Response.IsSuccess
                && capabilities.IsBoundTo(
                    owner.Diagnostics,
                    owner.SessionGeneration)
                && capabilities.Supports(LMCDiagnosticCapability.SDORead)
                && capabilities.Supports(
                    LMCDiagnosticCapability.SDOReadGeneralInline)
                && capabilities.MaxSdoDataBytes == 4
                && capabilities.BaseCycleTimeUs != 0
                && capabilities.MaxRequestPayloadBytes
                    >= LMC_DiagnosticsFrame
                        .SubmitSdoRequestHeaderPayloadLength
                && capabilities.MaxResponsePayloadBytes
                    >= LMC_DiagnosticsParser.OperationStatusPayloadLength
                && capabilities.DiagnosticsBootId != 0
                && capabilities.MapRevision != 0;
        }

        private void WriteD5DisconnectApplicationRecoveryPassLog(
            D5SdoDisconnectOrphanQualificationResult result)
        {
            if (result == null || result.RecoveryScope == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Action write = () =>
                WriteD5SdoQualificationLog(
                    "event=D5_DISCONNECT_ASSERT",
                    "disconnectMode=LOCAL_TCP_ZERO_LINGER_CLOSE_NO_RPC_CLOSE",
                    "oldTicket="
                        + result.RecoveryScope.OldTicket.TicketId.ToString(
                            CultureInfo.InvariantCulture),
                    "lastOldState="
                        + result.RecoveryScope.LastStatusBeforeLoss.State,
                    "pcRunningWitness="
                        + (result.RecoveryScope.HasExactRunningWitness
                            ? "true"
                            : "false"),
                    "transportAborted=true",
                    "rpcCloseSent=false",
                    "firstRecoveryTicket="
                        + result.RecoveryScope.FirstRecoveryTicket.TicketId
                            .ToString(CultureInfo.InvariantCulture),
                    "secondRecoveryTicket="
                        + result.RecoveryScope.SecondRecoveryTicket.TicketId
                            .ToString(CultureInfo.InvariantCulture),
                    "firstRecoverySubmitAttempts="
                        + result.RecoveryScope
                            .FirstRecoverySubmitAttemptCount.ToString(
                                CultureInfo.InvariantCulture),
                    "secondRecoverySubmitAttempts="
                        + result.RecoveryScope
                            .SecondRecoverySubmitAttemptCount.ToString(
                                CultureInfo.InvariantCulture),
                    "resourceBusyRejections="
                        + result.RecoveryScope
                            .RecoveryResourceBusyRejectionCount.ToString(
                                CultureInfo.InvariantCulture),
                    "automaticSubmitRetry=EXACT_REJECTED_RESOURCE_BUSY_ONLY",
                    "newConnectionRecovery=true",
                    "plcOrphanLifecycleWitness=false",
                    "orphanProof=NOT_PROVEN_BY_WPF",
                    "orphanQualified=false",
                    "verdict=PASS_APPLICATION_RECOVERY");

            if (Dispatcher.CheckAccess())
            {
                write();
                return;
            }

            // The orchestrator invokes this callback inside the quarantine
            // proof commit. Marshal synchronously so a logging failure throws
            // before the evidence ledger is cleared.
            Dispatcher.Invoke(write);
        }

        private void WriteD5SdoDisconnectRecoveryScope(
            D5SdoDisconnectOrphanRecoveryScope scope,
            Exception error)
        {
            WriteD5SdoQualificationLog(
                "event=D5_DISCONNECT_RECOVERY_SCOPE",
                "stage=" + scope.Stage,
                "oldTicket=" + (scope.OldTicket == null
                    ? "NONE"
                    : scope.OldTicket.TicketId.ToString(
                        CultureInfo.InvariantCulture)),
                "lastOldState=" + (scope.LastStatusBeforeLoss == null
                    ? "NONE"
                    : scope.LastStatusBeforeLoss.State.ToString()),
                "ownerTransportLossObserved=" + scope.OwnerTransportLossObserved,
                "pcRunningWitness=" + scope.HasExactRunningWitness,
                "newConnection=" + (scope.NewOwnerConnection == null
                    ? "NONE"
                    : scope.NewOwnerConnection.State.ToString()),
                "firstRecoveryTicket="
                    + (scope.FirstRecoveryTicket == null
                        ? "NONE"
                        : scope.FirstRecoveryTicket.TicketId.ToString(
                            CultureInfo.InvariantCulture)),
                "secondRecoveryTicket="
                    + (scope.SecondRecoveryTicket == null
                        ? "NONE"
                        : scope.SecondRecoveryTicket.TicketId.ToString(
                            CultureInfo.InvariantCulture)),
                "firstRecoverySubmitAttempts="
                    + scope.FirstRecoverySubmitAttemptCount.ToString(
                        CultureInfo.InvariantCulture),
                "secondRecoverySubmitAttempts="
                    + scope.SecondRecoverySubmitAttemptCount.ToString(
                        CultureInfo.InvariantCulture),
                "resourceBusyRejections="
                    + scope.RecoveryResourceBusyRejectionCount.ToString(
                        CultureInfo.InvariantCulture),
                "recoverySubmitAttemptLimit="
                    + scope.RecoverySubmitAttemptLimit.ToString(
                        CultureInfo.InvariantCulture),
                "plcOrphanLifecycleWitness=false",
                "orphanQualified=false",
                "quarantineCount="
                    + d5SdoQualificationQuarantine.Count.ToString(
                        CultureInfo.InvariantCulture),
                "errorType=" + error.GetType().Name,
                "error=" + QualificationValue(error.Message),
                "automaticSubmitRetry=EXACT_REJECTED_RESOURCE_BUSY_ONLY",
                "verdict=RECOVERY_REQUIRED");
        }
    }
}
